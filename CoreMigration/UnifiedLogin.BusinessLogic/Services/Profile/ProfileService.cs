using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.Services.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Services;

/// <summary>
/// Orchestrates UpdateProfile and ListPersons by composing the injected
/// async repositories. No BaseRepository inheritance, no <c>new</c> keyword.
///
/// All writes in UpdateProfile share a single IDbConnection transaction.
/// All reads use the injected async repositories.
/// What was done and why:
//Decision Reasoning
//ProfileService(not a repository)   These 550 lines are orchestration, not data access — they call 20 + SPs, apply business rules, and invoke external services
//Single IDbConnection _db +BeginTransaction()   Mirrors the original single repository.UnitOfWork.BeginTransaction() — all writes are atomic
//Pre - transaction async repo reads    Reads that don't need to be transactional (GetPersonaAsync(long, bool, CancellationToken), GetPartyRelationshipAsync(Guid, Guid, string, string, string, CancellationToken)) use the injected repos outside the transaction
//HandlePhoneNumbersAsync extracted   The ~130-line phone loop is cohesive enough to be its own private method
//HandleEmailContactAsync extracted Same reasoning — 60-line email block
//IUserRepository injected as sync    GetSuperVisorInformation(long, long) has no async version yet — marked with // TODO comment in code
//UserListTypeFilter as nested private enum It is an implementation detail of ListPersons(IList<int>, Guid?, int?, RequestParameter, bool), not a public contract
//BuildFilterJson(RequestParameter?) / BuildSortJson(RequestParameter?) as static Pure transformations with no captured state
//AuditActivityLog(String, string, string, UserActivityLogInfo, UserDetails) keeps LogActivity.WriteActivity(...) static call No async audit service exists yet — wrapped in try/catch so audit failures never break the main flow

/// </summary>
public sealed class ProfileService : IProfileService
{
    #region Fields

    private readonly IDbConnection _db;

    // Async repos — read-only pre-transaction work
    private readonly IPersonaRepositoryAsync       _personaRepo;
    private readonly IPersonRepositoryAsync        _personRepo;
    private readonly IPartyRelationshipRepositoryAsync _partyRelRepo;
    private readonly IProductRepositoryAsync       _productRepo;
    private readonly IProductInternalSettingRepositoryAsync _internalSettingRepo;
    private readonly IUserLoginRepositoryAsync     _userLoginRepo;
    private readonly IProfileRepositoryAsync       _profileRepo;

    // Sync repo — GetSuperVisorInformation has no async version yet
    private readonly IUserRepository _userRepository;

    private readonly IUserClaimsAccessor _userClaimAccessor;
    private readonly ILogger<ProfileService> _logger;

    #endregion

    #region Constructor

    public ProfileService(
        IDbConnection db,
        IPersonaRepositoryAsync personaRepo,
        IPersonRepositoryAsync personRepo,
        IPartyRelationshipRepositoryAsync partyRelRepo,
        IProductRepositoryAsync productRepo,
        IProductInternalSettingRepositoryAsync internalSettingRepo,
        IUserLoginRepositoryAsync userLoginRepo,
        IProfileRepositoryAsync profileRepo,
        IUserRepository userRepository,
        IUserClaimsAccessor userClaimAccessor,
        ILogger<ProfileService> logger)
    {
        _db                  = db                  ?? throw new ArgumentNullException(nameof(db));
        _personaRepo         = personaRepo         ?? throw new ArgumentNullException(nameof(personaRepo));
        _personRepo          = personRepo          ?? throw new ArgumentNullException(nameof(personRepo));
        _partyRelRepo        = partyRelRepo        ?? throw new ArgumentNullException(nameof(partyRelRepo));
        _productRepo         = productRepo         ?? throw new ArgumentNullException(nameof(productRepo));
        _internalSettingRepo = internalSettingRepo ?? throw new ArgumentNullException(nameof(internalSettingRepo));
        _userLoginRepo       = userLoginRepo       ?? throw new ArgumentNullException(nameof(userLoginRepo));
        _profileRepo         = profileRepo         ?? throw new ArgumentNullException(nameof(profileRepo));
        _userRepository      = userRepository      ?? throw new ArgumentNullException(nameof(userRepository));
        _userClaimAccessor   = userClaimAccessor   ?? throw new ArgumentNullException(nameof(userClaimAccessor));
        _logger              = logger              ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region IProfileService — UpdateProfileAsync

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateProfileAsync(
        Guid realPageId,
        IProfile profile,
        CancellationToken cancellationToken = default)
    {
        var userClaims = _userClaimAccessor.Current;
        var response   = new RepositoryResponse();

        // ── Pre-transaction reads (no writes, no transaction needed) ──────────

        var personaId            = await _personaRepo.GetActivePersonaIdAsync(realPageId, cancellationToken);
        var persona              = await _personaRepo.GetPersonaAsync(personaId, false, cancellationToken);
        var toUserLogInfo        = await GetUserActivityLogInfoAsync(personaId, cancellationToken);

        // Resolve impersonator display name for audit messages
        UserDetails impersonatorInfo = userClaims.ImpersonatedBy != Guid.Empty
            ? _userRepository.GetUserDetails(userRealPageId: userClaims.ImpersonatedBy.ToString())
            : null;

        var organizationRealPageId = persona?.Organization?.RealPageId ?? Guid.Empty;

        // Determine user type and product access
        var partyRelationship = await _partyRelRepo.GetPartyRelationshipAsync(
            realPageId, organizationRealPageId, null, null, "User Type", cancellationToken);

        bool isSuperUser                  = partyRelationship?.RoleTypeIdFrom == (int)UserRoleType.SuperUser;
        bool residentPortalAssignedToUser = false;
        bool isKnockProductAssignedToUser = false;

        if (!isSuperUser)
        {
            var assignedProducts = await _productRepo.ListProductsByPersonaIdAsync(
                personaId, (int)ProductBatchStatusType.Success, cancellationToken);

            residentPortalAssignedToUser = assignedProducts.Any(p => p.ProductId == (int)ProductEnum.ResidentPortal);
            isKnockProductAssignedToUser = assignedProducts.Any(p => p.ProductId == (int)ProductEnum.KnockCRM);
        }

        // ── Transactional writes ───────────────────────────────────────────────

        OpenIfClosed();
        using var tx = _db.BeginTransaction();
        try
        {
            // Resolve impersonator UserId for batch jobs
            UserLoginOnly impersonatorLogin = new();
            if (userClaims.ImpersonatedBy != Guid.Empty)
            {
                impersonatorLogin = await _db.QuerySingleOrDefaultAsync<UserLoginOnly>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_GetUserLoginOnly,
                        new { RealPageId = userClaims.ImpersonatedBy },
                        transaction: tx,
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: cancellationToken))
                    ?? new UserLoginOnly();
            }

            // ── Update core person record ───────────────────────────────────
            var oldPerson = await _db.QuerySingleOrDefaultAsync<Person>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_GetPerson,
                    new { realPageId },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            bool customJobTitleChanged = oldPerson?.Title != profile.Title;

            response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_UpdatePerson,
                    new
                    {
                        RealPageId               = realPageId,
                        Title                    = profile.Title,
                        FirstName                = profile.FirstName,
                        MiddleName               = profile.MiddleName,
                        LastName                 = profile.LastName,
                        Suffix                   = profile.Suffix,
                        PreferredContactMethodId = profile.PreferredContactMethodId
                    },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken))
                ?? new RepositoryResponse();

            if (response.Id == 0)
            {
                response.ErrorMessage = "Update profile Error: Update person failed.";
                tx.Rollback();
                return response;
            }

            // Audit: job title / preferred contact method changes
            if (oldPerson is not null)
            {
                if (oldPerson.Title != profile.Title)
                    AuditActivityLog(oldPerson.Title, profile.Title, "Company Job Title", toUserLogInfo, impersonatorInfo, userClaims);

                if (oldPerson.PreferredContactMethodId != profile.PreferredContactMethodId
                    && profile.PreferredContactMethodId > 0)
                {
                    var preferredMethods = (await _db.QueryAsync<PreferredContactMethod>(
                        new CommandDefinition(
                            StoredProcNameConstants.SP_ListPreferredContactMethods,
                            transaction: tx,
                            commandType: CommandType.StoredProcedure,
                            cancellationToken: cancellationToken))).ToList();

                    var oldValue = preferredMethods.FirstOrDefault(p => p.PreferredContactMethodId == oldPerson.PreferredContactMethodId)?.Name;
                    var newValue = preferredMethods.FirstOrDefault(p => p.PreferredContactMethodId == profile.PreferredContactMethodId)?.Name;
                    AuditActivityLog(oldValue, newValue, "Preferred Contact Method", toUserLogInfo, impersonatorInfo, userClaims);
                }
            }

            // ── Job title (party role + person-to-org link) ────────────────
            bool industryStandardJobChanged = false;

            var employmentRel = await _db.QuerySingleOrDefaultAsync<PartyRelationship>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_GetPartyRelationshipByRealPageId,
                    new { RealPageIdFrom = realPageId, RealPageIdTo = organizationRealPageId, RelationshipTypeName = "Employment" },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            int roleTypeIdFrom = employmentRel?.RoleTypeIdFrom > 0 ? (int)employmentRel.RoleTypeIdFrom : 0;

            var currentPartyRole = await _db.QuerySingleOrDefaultAsync<PartyRole>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_GetPartyRoleByRealPageId,
                    new { realPageId },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            if (profile.PartyRole is not null && profile.PartyRole.PartyRoleId > 0)
            {
                // Get Employer role type id
                var roleTypes = (await _db.QueryAsync<RoleType>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_ListRoleType,
                        new { RoleTypeName = "Organization Role" },
                        transaction: tx,
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: cancellationToken))).ToList();

                var employer     = roleTypes.SingleOrDefault(r => r.Name == "Employer");
                int roleTypeIdTo = employer is not null ? (int)employer.PartyRoleTypeId : 0;

                // Unlink old / link new job title relationship
                await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_UpdatePersonToOrganization,
                        new
                        {
                            PersonRealPageId       = realPageId,
                            OrganizationRealPageId = organizationRealPageId,
                            UnlinkRoleTypeIdFrom   = roleTypeIdFrom,
                            LinkRoleTypeIdFrom     = profile.PartyRole.RoleTypeId,
                            RoleTypeIdTo           = roleTypeIdTo
                        },
                        transaction: tx,
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: cancellationToken));

                response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_UpdatePartyRoleByRealPageId,
                        new { PartyRoleId = profile.PartyRole.PartyRoleId, RoleTypeID = profile.PartyRole.RoleTypeId },
                        transaction: tx,
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: cancellationToken))
                    ?? new RepositoryResponse();
            }
            else if (profile.PartyRole is not null)
            {
                response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_CreatePartyRoleByRealPageId,
                        new { RealPageId = realPageId, RoleTypeId = profile.PartyRole.RoleTypeId },
                        transaction: tx,
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: cancellationToken))
                    ?? new RepositoryResponse();
            }

            if (response.Id == 0 && profile.PartyRole is not null)
            {
                response.ErrorMessage = "Update profile Error: Job Title failed.";
                tx.Rollback();
                return response;
            }

            // Audit job title change
            if (currentPartyRole is not null
                && profile.PartyRole is not null
                && currentPartyRole.RoleTypeId != profile.PartyRole.RoleTypeId)
            {
                var personRoleTypes = (await _db.QueryAsync<RoleType>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_ListRoleType,
                        new { RoleTypeName = "person role", OrganizationPartyID = (int?)null },
                        transaction: tx,
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: cancellationToken))).ToList();

                var oldTitle = personRoleTypes.FirstOrDefault(r => r.PartyRoleTypeId == roleTypeIdFrom)?.Name;
                var newTitle = personRoleTypes.FirstOrDefault(r => r.PartyRoleTypeId == profile.PartyRole.RoleTypeId)?.Name;
                AuditActivityLog(oldTitle, newTitle, "Industry Job Title", toUserLogInfo, impersonatorInfo, userClaims);
                industryStandardJobChanged = true;
            }

            // ── Phone numbers ───────────────────────────────────────────────
            bool isPhoneNumberChange = await HandlePhoneNumbersAsync(
                realPageId, profile, toUserLogInfo, impersonatorInfo, userClaims, tx, cancellationToken);

            // ── Email contacts ──────────────────────────────────────────────
            response = await HandleEmailContactAsync(
                realPageId, profile, toUserLogInfo, impersonatorInfo, userClaims, tx, cancellationToken);

            if (response.Id == 0 && !string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                tx.Rollback();
                return response;
            }

            // ── Product batch (ResidentPortal / KnockCRM) ──────────────────
            if (!isSuperUser && (industryStandardJobChanged || customJobTitleChanged) && residentPortalAssignedToUser)
            {
                await SaveProductBatchAsync(
                    (int)ProductEnum.ResidentPortal, personaId,
                    userClaims, impersonatorLogin.UserId,
                    (int)BatchProcessType.ProfileUpdate, tx, cancellationToken);
            }

            if (isPhoneNumberChange && isKnockProductAssignedToUser)
            {
                await SaveProductBatchAsync(
                    (int)ProductEnum.KnockCRM, personaId,
                    userClaims, impersonatorLogin.UserId,
                    (int)BatchProcessType.ProfileUpdate, tx, cancellationToken);
            }

            tx.Commit();
            return response;
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "UpdateProfileAsync failed for RealPageId={Id}", realPageId);
            response.Id           = 0;
            response.ErrorMessage = $"Update profile Error: {ex.Message}";
            return response;
        }
    }

    #endregion

    #region IProfileService — ListPersonsAsync

    /// <inheritdoc/>
    public async Task<IList<ProfileDetail>> ListPersonsAsync(
        IList<int> organizationActiveProductIdList,
        Guid? realPageId = null,
        int? parentPartyRoleTypeId = null,
        RequestParameter dataFilterSort = null,
        bool isExport = false,
        CancellationToken cancellationToken = default)
    {
        var userClaims = _userClaimAccessor.Current;

        // ── Shared-product expansion ───────────────────────────────────────
        var sharedProducts = await _internalSettingRepo
            .GetProductSettingByTypeAsync(SettingConstants.SharedProductSettingName, cancellationToken);

        foreach (var sp in sharedProducts)
        {
            if (organizationActiveProductIdList.Contains(sp.ProductId)
                && int.TryParse(sp.Value, out var linkedId)
                && !organizationActiveProductIdList.Contains(linkedId))
            {
                organizationActiveProductIdList.Add(linkedId);
            }
        }

        // ── Determine user-list filter type ───────────────────────────────
        var filterType = UserListTypeFilter.ExcludeSupportAndSuperUsers;

        if (userClaims.UserRealPageGuid != Guid.Empty)
        {
            var rel = await _partyRelRepo.GetPartyRelationshipAsync(
                userClaims.UserRealPageGuid, userClaims.OrganizationRealPageGuid,
                null, null, "User Type", cancellationToken);

            bool isCallerSuperUser = rel?.RoleTypeIdFrom == (int)UserRoleType.SuperUser;

            if (isCallerSuperUser)
                filterType = UserListTypeFilter.ExcludeSupportUsers;

            if (userClaims.RealPageEmployee
                || (userClaims.IsRPEmployee && userClaims.OrganizationRealPageGuid != DefaultUserClaim.EmployeeCompanyRealPageId)
                || userClaims.ImpersonatedBy != Guid.Empty)
            {
                filterType = UserListTypeFilter.ViewAllUsers;
            }

            // Operator-scoped filtering
            var externalRel = await _profileRepo.GetExternalUserRelationshipAsync(
                userClaims.OrganizationPartyId, userClaims.UserId, cancellationToken);

            if (externalRel is not null
                && !string.IsNullOrEmpty(externalRel.OperatorCode)
                && !string.IsNullOrEmpty(externalRel.OperatorValue))
            {
                filterType  = UserListTypeFilter.OperatorUsers;
                dataFilterSort = ApplyOperatorFilter(dataFilterSort, externalRel);
            }

            // ThirdPartyRelationShipId == 10 forces external-user filter
            if (externalRel?.ThirdPartyRelationShipId == 10)
            {
                dataFilterSort ??= new RequestParameter { FilterBy = new Dictionary<string, string>() };
                dataFilterSort.FilterBy ??= new Dictionary<string, string>();

                if (dataFilterSort.FilterBy.ContainsKey("userType"))
                    dataFilterSort.FilterBy["userType"] = "404";
                else
                    dataFilterSort.FilterBy["userType"] = "404";
            }
        }

        // ── Build SP parameters ────────────────────────────────────────────
        var assignedProductsJson = BuildAssignedProductsJson(organizationActiveProductIdList, sharedProducts);
        var filterByJson         = BuildFilterJson(dataFilterSort, sharedProducts);
        var sortByJson           = BuildSortJson(dataFilterSort);

        var spName = isExport
            ? StoredProcNameConstants.SP_ListPersonsExport
            : StoredProcNameConstants.SP_ListPersons;

        var spParam = new
        {
            RealPageId            = realPageId,
            ParentPartyRoleTypeId = parentPartyRoleTypeId,
            UserListFilterType    = (int)filterType,
            AssignedProducts      = assignedProductsJson,
            FilterBy              = filterByJson,
            SortBy                = sortByJson,
            RowsPerPage  = dataFilterSort?.Pages?.ResultsPerPage == 100 ? 0 : (dataFilterSort?.Pages?.ResultsPerPage ?? 0),
            PageNumber   = (dataFilterSort?.Pages?.ResultsPerPage == 100 || dataFilterSort?.Pages?.StartRow <= 0)
                           ? 1 : (dataFilterSort?.Pages?.StartRow ?? 1)
        };

        // ── Multi-mapping query ────────────────────────────────────────────
        // Replaces: repository.GetManyWithSpliOn<ProfileDetail, UserLogin, int, string, ProfileDetail>
        var items = (await _db.QueryAsync<ProfileDetail, UserLogin, int, string, ProfileDetail>(
            spName,
            (profileDetail, userLogin, productCount, userType) =>
            {
                profileDetail.userLogin                            = userLogin;
                profileDetail.userLogin.PartyId                    = profileDetail.PartyId;
                profileDetail.userLogin.RealPageId                 = profileDetail.RealPageId;
                profileDetail.userLogin.LoginNameType              = EmailFormatValidation.IsValidEmail(userLogin.LoginName) ? "email" : "";
                profileDetail.SummaryCount.TotalAssignedProducts   = productCount;
                profileDetail.AssignedProducts                     = null;
                profileDetail.contactMechanism                     = null;
                profileDetail.organization                         = null;
                profileDetail.PartyRole                            = null;
                profileDetail.TelecommunicationNumber              = null;
                profileDetail.InactivePersona                      = null;
                profileDetail.Persona                              = null;

                if (userType is not null)
                {
                    var cleaned = Regex.Replace(userType, @"[^A-Za-z0-9]+", "");
                    if (Enum.TryParse<UserRoleType>(cleaned, true, out var parsed))
                        profileDetail.userLogin.UserRoleType = parsed;
                }

                // Stub status fields — real values resolved per-user below
                profileDetail.userLogin.IsPending  = false;
                profileDetail.userLogin.IsExpired  = false;
                profileDetail.userLogin.IsActive   = true;
                profileDetail.userLogin.IsLocked   = false;
                profileDetail.userLogin.Status     = UserUiStatusType.Active;

                return profileDetail;
            },
            spParam,
            splitOn: "UserId,Products,UserType",
            commandType: CommandType.StoredProcedure)).ToList();

        // ── Per-user enrichment ────────────────────────────────────────────
        // TODO: Replace with a batch query to eliminate the N+1 pattern.
        foreach (var item in items)
        {
            // Resolve real login status (IsPending, IsExpired, IsActive, IsLocked…)
            var fullLogin = await _userLoginRepo.GetUserLoginAsync(
                item.RealPageId, userClaims.OrganizationPartyId);

            if (fullLogin is not null)
                item.userLogin = fullLogin;

            // Supervisor info (sync — no async version of IUserRepository yet)
            item.SuperVisorUser = _userRepository
                .GetSuperVisorInformation(item.userLogin.UserId, userClaims.OrganizationPartyId)
                ?? new UserInfoLite();

            // Default phone
            var phones = (await _db.QueryAsync<TelecommunicationNumber>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_ListTelecommunicationNumbersForPerson,
                    new { realPageId = item.RealPageId },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken))).ToList();

            var defaultPhone = phones.FirstOrDefault(p => p.IsDefault);
            if (defaultPhone is not null)
            {
                item.PhoneNumber     = defaultPhone.PhoneNumber;
                item.PhoneNumberType = defaultPhone.contactMechanismUsageType?.Name;
            }
        }

        // Deactivated users show 0 products
        items.FindAll(i => i.userLogin.Status == UserUiStatusType.Disabled)
             .ForEach(d =>
             {
                 d.SummaryCount.TotalAssignedProducts = 0;
                 d.userLogin.Status = UserUiStatusType.Deactivated;
             });

        return items;
    }

    #endregion

    #region Private — UpdateProfile helpers

    /// <summary>
    /// Handles all phone-number adds, updates, deletes and the contact preference.
    /// Returns true when any phone number changed (used to decide Knock product batch).
    /// </summary>
    private async Task<bool> HandlePhoneNumbersAsync(
        Guid realPageId,
        IProfile profile,
        UserActivityLogInfoAsync toUserLogInfo,
        UserDetails impersonatorInfo,
        DefaultUserClaim userClaims,
        IDbTransaction tx,
        CancellationToken cancellationToken)
    {
        bool isPhoneNumberChange = false;

        var existingPhones = (await _db.QueryAsync<TelecommunicationNumber>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListTelecommunicationNumbersForPerson,
                new { realPageId },
                transaction: tx,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))).ToList();

        var usageTypes = (await _db.QueryAsync<ContactMechanismUsageType>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListContactMechanismUsageType,
                new { ContactMechanismUsageTypeName = "phone type" },
                transaction: tx,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))).ToList();

        foreach (var phone in profile.TelecommunicationNumber.ToList())
        {
            if (phone.ContactMechanismId == 0 && !string.IsNullOrEmpty(phone.PhoneNumber))
            {
                var phoneTypeName = usageTypes.FirstOrDefault(u => u.ContactMechanismUsageTypeId == phone.contactMechanismUsageType?.ContactMechanismUsageTypeId)?.Name;
                AuditActivityLog($"{phone.ISOCode}({phone.CountryCode}) {phone.PhoneNumber},{phoneTypeName}", " ", "Added Phone Number", toUserLogInfo, impersonatorInfo, userClaims);
            }
            if (phone.IsDeleted && !string.IsNullOrEmpty(phone.PhoneNumber))
            {
                var phoneTypeName = usageTypes.FirstOrDefault(u => u.ContactMechanismUsageTypeId == phone.contactMechanismUsageType?.ContactMechanismUsageTypeId)?.Name;
                AuditActivityLog($"{phone.ISOCode}({phone.CountryCode}) {phone.PhoneNumber},{phoneTypeName}", " ", "Deleted Phone Number", toUserLogInfo, impersonatorInfo, userClaims);
            }

            if (phone.ContactMechanismId == 0)
            {
                if (phone.IsDeleted)
                {
                    profile.TelecommunicationNumber.Remove((TelecommunicationNumber)phone);
                    continue;
                }

                if (phone.PhoneNumber?.Trim().Length > 0 && phone.contactMechanismUsageType?.ContactMechanismUsageTypeId > 0)
                {
                    // New phone: create mechanism → link to party → assign usage type
                    var createMechResp = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                        new CommandDefinition(StoredProcNameConstants.SP_CreateContactMechanism,
                            new { ContactMechanismId = 0 }, transaction: tx,
                            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))
                        ?? new RepositoryResponse();

                    if (createMechResp.Id == 0) continue;

                    phone.ContactMechanismId = Convert.ToInt32(createMechResp.Id);

                    var linkPartyResp = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                        new CommandDefinition(StoredProcNameConstants.SP_LinkContactMechanismToParty,
                            new { RealPageId = realPageId, PartyContactMechanismId = 0, ContactMechanismId = phone.ContactMechanismId, FromDate = DateTime.UtcNow, ThruDate = DateTime.MaxValue.ToUniversalTime() },
                            transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))
                        ?? new RepositoryResponse();

                    if (linkPartyResp.Id == 0) continue;

                    phone.PartyContactMechanismId = linkPartyResp.Id;

                    await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                        new CommandDefinition(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism,
                            new { PartyContactMechanismId = linkPartyResp.Id, ContactMechanismUsageTypeId = phone.contactMechanismUsageType.ContactMechanismUsageTypeId },
                            transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));

                    isPhoneNumberChange = true;
                }
            }
            else if (phone.IsDeleted)
            {
                // Expire existing phone
                await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                    new CommandDefinition(StoredProcNameConstants.SP_ExpirePartyContactMechanism,
                        new { PartyContactMechanismID = phone.PartyContactMechanismId, RealPageId = profile.RealPageId },
                        transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));

                profile.TelecommunicationNumber.Remove((TelecommunicationNumber)phone);
                isPhoneNumberChange = true;
            }
            else
            {
                // Update usage type or expire when cleared
                if (phone.contactMechanismUsageType?.ContactMechanismUsageTypeId > 0 && phone.PhoneNumber?.Trim().Length > 0)
                {
                    await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                        new CommandDefinition(StoredProcNameConstants.SP_UpdateContactMechanismUsageForParty,
                            new { PartyContactMechanismID = phone.PartyContactMechanismId, ContactMechanismUsageTypeId = phone.contactMechanismUsageType.ContactMechanismUsageTypeId },
                            transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
                }
                else
                {
                    await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                        new CommandDefinition(StoredProcNameConstants.SP_LinkContactMechanismToParty,
                            new { RealPageId = realPageId, PartyContactMechanismId = phone.PartyContactMechanismId, ContactMechanismId = phone.ContactMechanismId, FromDate = DateTime.UtcNow, ThruDate = DateTime.UtcNow },
                            transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
                }
                isPhoneNumberChange = true;
            }

            // Persist phone digits
            if (!phone.IsDeleted && phone.ContactMechanismId > 0 && phone.PhoneNumber?.Trim().Length > 0)
            {
                await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                    new CommandDefinition(StoredProcNameConstants.SP_CreateTelecommunicationNumber,
                        new { ContactMechanismId = phone.ContactMechanismId, AreaCode = phone.AreaCode, CountryCode = phone.CountryCode, PhoneNumber = phone.PhoneNumber, ISOCode = phone.ISOCode, Default = phone.IsDefault },
                        transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));

                // Audit phone number changes
                foreach (var existing in existingPhones.Where(e => e.ContactMechanismId == phone.ContactMechanismId))
                {
                    var oldType = usageTypes.FirstOrDefault(u => u.ContactMechanismUsageTypeId == existing.ContactMechanismUsageTypeId)?.Name;
                    var newType = usageTypes.FirstOrDefault(u => u.ContactMechanismUsageTypeId == phone.contactMechanismUsageType?.ContactMechanismUsageTypeId)?.Name;

                    if (existing.PhoneNumber != phone.PhoneNumber || oldType != newType || existing.CountryCode != phone.CountryCode)
                    {
                        AuditActivityLog(
                            $"{existing.ISOCode}({existing.CountryCode}) {existing.PhoneNumber},{oldType}",
                            $"{phone.ISOCode}({phone.CountryCode}) {phone.PhoneNumber},{newType}",
                            "Phone Number", toUserLogInfo, impersonatorInfo, userClaims);
                    }
                }
            }
        }

        // Contact preference (preferred / default phone)
        if (profile.TelecommunicationNumber.Any())
            await UpdateContactPreferenceAsync(profile.RealPageId, profile.TelecommunicationNumber.ToList(), tx, cancellationToken);

        // Audit default phone change
        if (existingPhones.Any() && profile.TelecommunicationNumber.Any())
        {
            var oldDefault = existingPhones.FirstOrDefault(p => p.IsDefault);
            var newDefault = profile.TelecommunicationNumber.FirstOrDefault(p => p.IsDefault);
            if (oldDefault is not null && newDefault is not null && oldDefault.PhoneNumber != newDefault.PhoneNumber)
            {
                var oldType = usageTypes.FirstOrDefault(u => u.ContactMechanismUsageTypeId == oldDefault.ContactMechanismUsageTypeId)?.Name;
                var newType = usageTypes.FirstOrDefault(u => u.ContactMechanismUsageTypeId == newDefault.contactMechanismUsageType?.ContactMechanismUsageTypeId)?.Name;
                AuditActivityLog(
                    $"{oldDefault.ISOCode}({oldDefault.CountryCode}) {oldDefault.PhoneNumber},{oldType}",
                    $"{newDefault.ISOCode}({newDefault.CountryCode}) {newDefault.PhoneNumber},{newType}",
                    "Default Phone Number", toUserLogInfo, impersonatorInfo, userClaims);
            }
        }

        return isPhoneNumberChange;
    }

    /// <summary>
    /// Handles the single primary email contact: create, update, or expire.
    /// </summary>
    private async Task<RepositoryResponse> HandleEmailContactAsync(
        Guid realPageId,
        IProfile profile,
        UserActivityLogInfoAsync toUserLogInfo,
        UserDetails impersonatorInfo,
        DefaultUserClaim userClaims,
        IDbTransaction tx,
        CancellationToken cancellationToken)
    {
        var response = new RepositoryResponse { Id = 1 };  // assume OK unless a step fails

        var oldEmails = (await _db.QueryAsync<ElectronicAddress>(
            new CommandDefinition(StoredProcNameConstants.SP_ListEmailsForPerson,
                new { realPageId }, transaction: tx,
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();

        if (!oldEmails.Any())
            oldEmails.Add(new ElectronicAddress { AddressString = "", contactMechanismUsageType = new ContactMechanismUsageType { ContactMechanismUsageTypeId = 302 } });

        var email = profile.EmailContacts[0];

        if (email.ContactMechanismId == 0 && email.AddressString.Trim().Length > 0)
        {
            // Create contact mechanism → link to party → assign usage type
            var mechResp = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(StoredProcNameConstants.SP_CreateContactMechanism,
                    new { ContactMechanismId = 0 }, transaction: tx,
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))
                ?? new RepositoryResponse();

            if (mechResp.Id == 0) { response.ErrorMessage = "Update profile Error: Create Contact Mechanism failed for Electronic Email Address."; return response; }

            email.ContactMechanismId = (int)mechResp.Id;

            var linkResp = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(StoredProcNameConstants.SP_LinkContactMechanismToParty,
                    new { RealPageId = realPageId, PartyContactMechanismId = 0, ContactMechanismId = mechResp.Id, FromDate = DateTime.UtcNow, ThruDate = DateTime.MaxValue.ToUniversalTime() },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))
                ?? new RepositoryResponse();

            if (linkResp.Id == 0) { response.ErrorMessage = "Update profile Error: Create Contact Mechanism failed for Electronic Email Address."; return response; }

            email.PartyContactMechanismId = (int)linkResp.Id;

            await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism,
                    new { PartyContactMechanismId = linkResp.Id, ContactMechanismUsageTypeId = email.contactMechanismUsageType.ContactMechanismUsageTypeId },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        if (email.AddressString.Trim().Length > 0)
        {
            response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(StoredProcNameConstants.SP_CreateElectronicAddress,
                    new { ContactMechanismId = email.ContactMechanismId, ElectronicAddressString = email.AddressString, ElectronicAddressType = email.AddressType },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))
                ?? new RepositoryResponse();

            if (response.Id == 0) response.ErrorMessage = "Update profile Error: Link Electronic Email Address details failed.";
        }
        else if (email.PartyContactMechanismId != 0)
        {
            // Blank email — expire the contact mechanism
            await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(StoredProcNameConstants.SP_ExpirePartyContactMechanism,
                    new { PartyContactMechanismID = email.PartyContactMechanismId, RealPageId = profile.RealPageId },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));

            email.ContactMechanismId = 0;
            email.ContactMechanismUsageTypeId = 0;
            email.PartyContactMechanismId = 0;
            response.Id = 1;
        }

        // Audit secondary email change
        var oldSecondary = oldEmails.Where(e => e.ContactMechanismUsageTypeId == 302).Select(e => e.AddressString).FirstOrDefault() ?? string.Empty;
        if (oldSecondary != email.AddressString)
            AuditActivityLog(oldSecondary, email.AddressString, "Secondary Email", toUserLogInfo, impersonatorInfo, userClaims);

        return response;
    }

    /// <summary>
    /// Creates a batch process group and queues a product batch record.
    /// Replaces: ProfileRepository.SaveProductBatch (private, sync).
    /// </summary>
    private async Task SaveProductBatchAsync(
        int productId,
        long assignUserPersonaId,
        DefaultUserClaim userClaims,
        long impersonatorUserId,
        int batchProcessTypeId,
        IDbTransaction tx,
        CancellationToken cancellationToken)
    {
        var batchGroup = await CreateBatchProcessGroupAsync(tx, cancellationToken);
        var inputJson  = JsonConvert.SerializeObject(
            new RolePropertyList { PropertyList = [], RoleList = [], IsAssigned = true });

        var result = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateProductBatch,
                new
                {
                    PersonRealPageId      = userClaims.UserRealPageGuid,
                    CreateUserPersonaId   = userClaims.PersonaId,
                    AssignUserPersonaId   = assignUserPersonaId,
                    ProductId             = productId,
                    StatusTypeId          = 5,
                    RetryCount            = 0,
                    InputJson             = inputJson,
                    CorrelationId         = userClaims.CorrelationId.ToString(),
                    BatchProcessTypeId    = batchProcessTypeId,
                    BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                    ImpersonatorUserId    = impersonatorUserId,
                    UseAPIV2              = true
                },
                transaction: tx,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        if (result?.Id == 0)
            _logger.LogWarning("SaveProductBatch: SP_CreateProductBatch returned 0 for ProductId={P}", productId);
    }

    /// <summary>
    /// Creates a batch process group and returns its ID.
    /// Replaces: ProfileRepository.CreateBatchProcessGroup (private, sync).
    /// </summary>
    private async Task<BatchProcessorGroup> CreateBatchProcessGroupAsync(
        IDbTransaction tx, CancellationToken cancellationToken)
    {
        var param = new DynamicParameters();
        param.Add("@BatchProcessorGroupID", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

        try
        {
            await _db.ExecuteAsync(
                new CommandDefinition(
                    StoredProcNameConstants.SP_CreateBatchProcessorGroup,
                    param,
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateBatchProcessGroupAsync failed");
        }

        return new BatchProcessorGroup
        {
            BatchProcessorGroupId             = param.Get<int>("@BatchProcessorGroupID"),
            BatchProcessorGroupActivityLogged = false
        };
    }

    /// <summary>
    /// Adds or removes the preferred/default contact mechanism preference.
    /// Replaces: ProfileRepository.UpdateContactPreference (private, sync).
    /// </summary>
    private async Task UpdateContactPreferenceAsync(
        Guid realPageId,
        List<TelecommunicationNumber> phones,
        IDbTransaction tx,
        CancellationToken cancellationToken)
    {
        var preferred = phones.FirstOrDefault(p => !p.IsDeleted && p.IsPreferred);

        var current = (await _db.QueryAsync<TelecommunicationNumber>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListTelecommunicationNumbersForPerson,
                new { realPageId },
                transaction: tx,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))).ToList();

        var currentPreferredId = current.FirstOrDefault(p => p.IsPreferred)?.ContactMechanismId;

        if ((currentPreferredId is null && preferred is not null)
            || (preferred is not null && currentPreferredId != preferred.ContactMechanismId))
        {
            await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_AddUpdateContactMechanismPreference,
                    new { CurrentContactMechanismId = preferred!.ContactMechanismId, PreviousPreferenceId = currentPreferredId ?? 0 },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));
        }
        else if (preferred is null && currentPreferredId is not null)
        {
            await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_DeleteContactMechanismPreference,
                    new { ContactMechanismId = currentPreferredId },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));
        }
    }

    #endregion

    #region Private — ListPersons helpers

    /// <summary>Controls how the user list is filtered.</summary>
    private enum UserListTypeFilter
    {
        ViewAllUsers                  = 0,
        ExcludeSupportUsers           = 1,
        ExcludeSupportAndSuperUsers   = 2,
        OperatorUsers                 = 3
    }

    private static RequestParameter ApplyOperatorFilter(
        RequestParameter dataFilterSort,
        ExternalUserRelationship rel)
    {
        var operatorValue = $"{rel.OperatorCode}|{rel.OperatorValue}";
        dataFilterSort ??= new RequestParameter { FilterBy = new Dictionary<string, string>() };
        dataFilterSort.FilterBy ??= new Dictionary<string, string>();

        // Replace or add Operator key
        var operatorKey = dataFilterSort.FilterBy.Keys.FirstOrDefault(k => k.Equals("Operator", StringComparison.OrdinalIgnoreCase));
        if (operatorKey is not null) dataFilterSort.FilterBy.Remove(operatorKey);
        dataFilterSort.FilterBy["Operator"] = operatorValue;

        // Force ExternalUser type
        var userTypeKey = dataFilterSort.FilterBy.Keys.FirstOrDefault(k => k.Equals("UserType", StringComparison.OrdinalIgnoreCase));
        if (userTypeKey is not null) dataFilterSort.FilterBy.Remove(userTypeKey);
        dataFilterSort.FilterBy["userType"] = "405";

        return dataFilterSort;
    }

    private static string BuildAssignedProductsJson(
        IList<int> productIds,
        IList<ProductInternalSettingByType> sharedProducts)
    {
        var filter = new List<FilterTableType>
        {
            new() { ColumnName = "ProductId", SearchValue = string.Join(",", productIds) }
        };
        return JsonConvert.SerializeObject(new { assignedProducts = filter });
    }

    private static string BuildFilterJson(
        RequestParameter dataFilterSort,
        IList<ProductInternalSettingByType> sharedProducts)
    {
        if (dataFilterSort?.FilterBy is null) return string.Empty;

        var validKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Name", "ProductId", "Status", "UserType", "OffsetMinutes",
            "RoleTemplateId", "PrimaryProperties", "PersonaHasProductError", "Operator"
        };

        var filterBy = dataFilterSort.FilterBy
            .Where(f => validKeys.Contains(f.Key))
            .Select(f =>
            {
                // Replace ProductId value with shared-product target when applicable
                if (f.Key.Equals("ProductId", StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(f.Value, out var pid))
                {
                    var sp = sharedProducts.FirstOrDefault(m => m.ProductId == pid);
                    return new FilterTableType { ColumnName = f.Key, SearchValue = sp?.Value ?? f.Value };
                }
                return new FilterTableType { ColumnName = f.Key, SearchValue = f.Value };
            })
            .ToList();

        return filterBy.Count > 0 ? JsonConvert.SerializeObject(new { filterBy }) : string.Empty;
    }

    private static string BuildSortJson(RequestParameter dataFilterSort)
    {
        if (dataFilterSort?.SortBy is null) return string.Empty;

        var sortBy = dataFilterSort.SortBy
            .Select(s => new SortTableType
            {
                ColumnName    = s.Key,
                SortDirection = s.Value[..Math.Min(128, s.Value.Length)]
            })
            .ToList();

        return sortBy.Count > 0 ? JsonConvert.SerializeObject(new { sortBy }) : string.Empty;
    }

    #endregion

    #region Private — Shared helpers

    private async Task<UserActivityLogInfoAsync> GetUserActivityLogInfoAsync(
        long personaId, CancellationToken cancellationToken)
    {
        // Replaces: ProfileRepository.GetUserActivityLogInfo (sync, uses new ManagePerson / ManageUserLogin)
        var persona   = await _personaRepo.GetPersonaAsync(personaId, false, cancellationToken);
        var userLogin = await _userLoginRepo.GetUserLoginOnlyAsync(persona.RealPageId);
        var person    = await _personRepo.GetPersonAsync(persona.RealPageId, cancellationToken);

        return new UserActivityLogInfoAsync
        {
            FirstName                 = person?.FirstName,
            LastName                  = person?.LastName,
            RealPageId                = userLogin?.RealPageId ?? Guid.Empty,
            LoginName                 = userLogin?.LoginName,
            BooksOrganizationMasterId = persona.Organization?.BooksMasterId ?? 0,
            OrganizationPartyId       = persona.OrganizationPartyId,
            UserId                    = userLogin?.UserId ?? 0
        };
    }

    /// <summary>
    /// Replaces: ProfileRepository.AuditActivityLog — same logic, no static Serilog dependency.
    /// Static LogActivity.WriteActivity call is kept (no async version exists yet).
    /// </summary>
    private void AuditActivityLog(
        string oldValue,
        string newValue,
        string fieldName,
        UserActivityLogInfoAsync toUserLogInfo,
        UserDetails impersonatorInfo,
        DefaultUserClaim userClaims)
    {
        try
        {
            var additionalInfo = new List<AdditionalParameters>();
            string message;

            if (fieldName is "Deleted Phone Number")
            {
                message = impersonatorInfo is not null
                    ? $"RealPage Access ({impersonatorInfo.FirstName} {impersonatorInfo.LastName}) Deleted Phone Number {oldValue}."
                    : $"{userClaims.FirstName} {userClaims.LastName} Deleted Phone Number {oldValue}.";
                additionalInfo.Add(new AdditionalParameters { Key = "Phone Number", Value = $"{{\"action\":\"Deleted\",\"value\":\"{oldValue}\"}}" });
            }
            else if (fieldName is "Added Phone Number")
            {
                message = impersonatorInfo is not null
                    ? $"RealPage Access ({impersonatorInfo.FirstName} {impersonatorInfo.LastName}) Added Phone Number {oldValue}."
                    : $"{userClaims.FirstName} {userClaims.LastName} Added Phone Number {oldValue}.";
                additionalInfo.Add(new AdditionalParameters { Key = "Phone Number", Value = $"{{\"action\":\"Added\",\"value\":\"{oldValue}\"}}" });
            }
            else
            {
                oldValue = string.IsNullOrEmpty(oldValue) ? "Blank Value" : oldValue;
                newValue = string.IsNullOrEmpty(newValue) ? "Blank Value" : newValue;
                message  = impersonatorInfo is not null
                    ? $"RealPage Access ({impersonatorInfo.FirstName} {impersonatorInfo.LastName}) updated {fieldName} from {oldValue} to {newValue}."
                    : $"{userClaims.FirstName} {userClaims.LastName} updated {fieldName} from {oldValue} to {newValue}.";
                additionalInfo.Add(new AdditionalParameters { Key = fieldName, Value = $"{{\"action\":\"Updated To\",\"value\":\"{(newValue == "Blank Value" ? " " : newValue)}\"}}" });
                additionalInfo.Add(new AdditionalParameters { Key = fieldName, Value = $"{{\"action\":\"Updated From\",\"value\":\"{(oldValue == "Blank Value" ? " " : oldValue)}\"}}" });
            }

            LogActivity.WriteActivity(new ActivityDetails
            {
                LogActivityTypeName       = LogActivityTypeConstants.UPDATE_USER,
                LogCategoryName           = LogActivityCategoryType.User.ToString(),
                CorrelationId             = userClaims.CorrelationId.ToString(),
                BooksMasterOrganizationId = userClaims.OrganizationMasterId,
                OrganizationPartyId       = userClaims.OrganizationPartyId,
                Message                   = message,
                FromUserLoginName         = userClaims.LoginName,
                FromUserLoginId           = userClaims.UserId,
                FromUserRealpageId        = userClaims.UserRealPageGuid.ToString(),
                FromUserFirstName         = userClaims.FirstName,
                FromUserLastName          = userClaims.LastName,
                ToUserLoginName           = toUserLogInfo.LoginName,
                ToUserLoginId             = toUserLogInfo.UserId,
                ToUserFirstName           = toUserLogInfo.FirstName,
                ToUserLastName            = toUserLogInfo.LastName,
                ToUserRealpageId          = toUserLogInfo.RealPageId.ToString(),
                AdditionalInformation     = additionalInfo
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AuditActivityLog failed for field={Field}", fieldName);
        }
    }

    private void OpenIfClosed()
    {
        if (_db.State != ConnectionState.Open)
            _db.Open();
    }

    #endregion
}