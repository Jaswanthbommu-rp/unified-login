using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data;
using System.Dynamic;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.Services.Audit;
using UnifiedLogin.BusinessLogic.Services.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.DataAccess.Helper;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.EnterpriseRole;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.RealConnect;
using OrganizationPrimary = UnifiedLogin.BusinessLogic.Repository.UserRepository.OrganizationPrimary;
using UnifiedLogin.SharedObjects.Saml;
using UnifiedLogin.SharedObjects.Landing.UserUpdate;
using SO = UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Services;

/// <summary>
/// Async-first orchestration service for all user lifecycle operations.
/// Replaces the 8 000-line <c>UserRepository</c> for every method that
/// coordinates more than one stored-procedure call, calls an external service,
/// or contains business rules.
///
/// Key improvements over the legacy sync version:
/// <list type="bullet">
///   <item>Single DI constructor — no <c>new</c> keyword anywhere.</item>
///   <item><c>IDbConnection</c> + <c>IDbTransaction</c> instead of <c>IRepository</c> / <c>UnitOfWork</c>.</item>
///   <item><c>CommandDefinition</c> with <c>CancellationToken</c> on every DB call.</item>
///   <item><c>IUserClaimsAccessor</c> instead of stale <c>DefaultUserClaim</c> field.</item>
///   <item><c>IMemoryCache</c> instead of static <c>RPObjectCache</c>.</item>
///   <item><c>Task.WhenAll</c> + <c>SemaphoreSlim</c> instead of <c>Parallel.ForEach</c>.</item>
///   <item>All empty <c>catch {}</c> blocks replaced by structured <c>_logger.LogError</c>.</item>
///   <item>Each <c>#region</c> in <c>CreateUser</c> / <c>UpdateUser</c> extracted to a
///         focused <c>private async Task</c> method.</item>
/// </list>
/// </summary>
public sealed class UserServiceAsync : IUserServiceAsync
{
    #region Fields

    private readonly IConnectionFactory _connectionFactory;
    private readonly IUserLoginRepositoryAsync _userLoginRepo;
    private readonly IUserRepositoryAsync _userRepo;
    private readonly IOrganizationRepositoryAsync _orgRepo;
    private readonly IManagePersonaAsync _managePersona;
    private readonly IPersonaRepositoryAsync _personaRepo;
    private readonly IContactMechanismUsageTypeRepositoryAsync _contactMechUsageTypeRepo;
    private readonly IRoleTypeRepositoryAsync _roleTypeRepo;
    private readonly IManageBlueBookAsync _manageBlueBook;
    private readonly IManageUnifiedSettingsAsync _manageUnifiedSettings;
    private readonly IProductInternalSettingRepositoryAsync _productSettingRepo;
    private readonly IManageProductAssetOptimizationAsync _manageAo;
    private readonly IUserClaimsAccessor _userClaims;
    private readonly IUserAuditService _auditService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<UserServiceAsync> _logger;

    // Bounded parallelism for per-user product-disable loops (mirrors legacy MaxDegreeOfParallelism = 5)
    private const int MaxProductDisableParallelism = 5;

    #endregion

    #region Constructor

    public UserServiceAsync(
        IConnectionFactory connectionFactory,
        IUserLoginRepositoryAsync userLoginRepo,
        IUserRepositoryAsync userRepo,
        IOrganizationRepositoryAsync orgRepo,
        IManagePersonaAsync managePersona,
        IPersonaRepositoryAsync personaRepo,
        IContactMechanismUsageTypeRepositoryAsync contactMechUsageTypeRepo,
        IRoleTypeRepositoryAsync roleTypeRepo,
        IManageBlueBookAsync manageBlueBook,
        IManageUnifiedSettingsAsync manageUnifiedSettings,
        IProductInternalSettingRepositoryAsync productSettingRepo,
        IManageProductAssetOptimizationAsync manageAo,
        IUserClaimsAccessor userClaims,
        IUserAuditService auditService,
        IMemoryCache cache,
        ILogger<UserServiceAsync> logger)
    {
        _connectionFactory      = connectionFactory      ?? throw new ArgumentNullException(nameof(connectionFactory));
        _userLoginRepo          = userLoginRepo          ?? throw new ArgumentNullException(nameof(userLoginRepo));
        _userRepo               = userRepo               ?? throw new ArgumentNullException(nameof(userRepo));
        _orgRepo                = orgRepo                ?? throw new ArgumentNullException(nameof(orgRepo));
        _managePersona          = managePersona          ?? throw new ArgumentNullException(nameof(managePersona));
        _personaRepo            = personaRepo            ?? throw new ArgumentNullException(nameof(personaRepo));
        _contactMechUsageTypeRepo = contactMechUsageTypeRepo ?? throw new ArgumentNullException(nameof(contactMechUsageTypeRepo));
        _roleTypeRepo           = roleTypeRepo           ?? throw new ArgumentNullException(nameof(roleTypeRepo));
        _manageBlueBook         = manageBlueBook         ?? throw new ArgumentNullException(nameof(manageBlueBook));
        _manageUnifiedSettings  = manageUnifiedSettings  ?? throw new ArgumentNullException(nameof(manageUnifiedSettings));
        _productSettingRepo     = productSettingRepo     ?? throw new ArgumentNullException(nameof(productSettingRepo));
        _manageAo               = manageAo               ?? throw new ArgumentNullException(nameof(manageAo));
        _userClaims             = userClaims             ?? throw new ArgumentNullException(nameof(userClaims));
        _auditService           = auditService           ?? throw new ArgumentNullException(nameof(auditService));
        _cache                  = cache                  ?? throw new ArgumentNullException(nameof(cache));
        _logger                 = logger                 ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    // ── CreateUser ────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<CreateUserResponse<IErrorData>> CreateUserAsync(
        ProfileDetail newProfile, IList<Persona> persona, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(newProfile);
        ArgumentNullException.ThrowIfNull(persona);

        var response = new CreateUserResponse<IErrorData>();
        var utcNow   = DateTime.UtcNow;

        // ── Pre-transaction lookups (read-only) ───────────────────────────
        var userLoginOnly   = await _userLoginRepo.GetUserLoginOnlyAsync(newProfile.userLogin.LoginName);
        var orgPartyId      = newProfile.organization[0].PartyId;
        var orgRealPageId   = newProfile.organization[0].RealPageId;

        var existingOrgs    = userLoginOnly is not null
            ? await _userLoginRepo.ListOrganizationByLoginNameAsync(newProfile.userLogin.LoginName)
            : new List<UserOrganization>();

        // Duplicate check — same username already in this company
        if (existingOrgs.Any(o => o.OrganizationPartyId == orgPartyId))
            return Failure(response, "User.CreateUser.1", "Username already exists in this company.");

        // GAP-fix #1 + #2: fetch booksCustomerMasterId and profileChanged BEFORE domain-exists check
        long booksCustomerMasterId = 0;
        bool profileChanged = false;
        if (userLoginOnly is not null)
        {
            var userDetails = await _userRepo.GetUserDetailsAsync(
                personaId: null, userRealPageId: userLoginOnly.RealPageId.ToString(), ct);
            booksCustomerMasterId = userDetails?.BooksCustomerMasterId ?? 0;
            profileChanged = IsUserProfileChanged(newProfile, userDetails);
        }

        var orgExists = await IsLoginNameExistsAsAdminInOtherDomainAsync(
                            newProfile.userLogin.LoginName, orgRealPageId, booksCustomerMasterId, ct);

        // Fail fast: check for duplicate user-type BEFORE expensive lookups (idpTypes, externalOrg, etc.)
        if (IsDuplicateUserType(newProfile, existingOrgs, orgExists))
            return Failure(response, "User.CreateUser.2", "This user type already exists for this username.");

        // ── Remaining pre-transaction lookups (only reached if not a duplicate) ─
        var idpTypes        = await _orgRepo.GetOrganizationIdentityProviderTypeAsync(orgRealPageId);
        var externalOrg     = await _orgRepo.GetOrganizationAsync(DefaultUserClaim.ExternalCompanyRealPageId);
        var emailUsageTypes = await _contactMechUsageTypeRepo.ListContactMechanismUsageTypeAsync("Email Notification", ct);
        var roleTypes       = await _roleTypeRepo.GetRoleTypeAsync("User Role", null, ct);
        var platformAdminRole = await ResolvePlatformAdminRoleAsync(ct);
        var isDelegateAdmin = await GetUnifiedSettingDataAsync("delegateadministrators", ct);

        var currentPrimaryStatus = userLoginOnly is not null
            ? await _userLoginRepo.GetUserOrganizationWithStatusAsync(userLoginOnly.UserId, userLoginOnly.LastLogin, 0, true)
            : null;

        if (userLoginOnly is not null)
            newProfile.IsRPEmployee = orgRealPageId == DefaultUserClaim.EmployeeCompanyRealPageId;

        // GAP-fix #6: get AO products for SuperUser (non-clone) before transaction
        IList<string>? aoProductsAvailableForUser = null;
        if (newProfile.UserTypeId == (int)UserRoleType.SuperUser && !newProfile.ClonedUser)
        {
            aoProductsAvailableForUser = await GetEditorUserAoProductAsync(
                _userClaims.UserRealPageGuid, _userClaims.PersonaId, orgRealPageId, ct);
        }

        // GAP-fix #3 + #4: fetch flag and primary batch before clone-user block
        bool usePropertyInstanceUnifiedLogin = await GetPropertyInstanceUnifiedLoginAsync(ct);
        var primaryPropertiesBatch = newProfile.productBatch
            ?.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedPlatform);

        // GAP-fix #5: clone-user product batch preparation
        if (newProfile.ClonedUser)
        {
            await PrepareCloneUserProductBatchAsync(
                newProfile, primaryPropertiesBatch, usePropertyInstanceUnifiedLogin, ct);
        }

        // ── Transaction ───────────────────────────────────────────────────
        using var conn = _connectionFactory.GetConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        var processTracker = "Init";
        try
        {
            var idpType = idpTypes.FirstOrDefault(i => i.IsLocal == !newProfile.userLogin.Is3rdPartyIDP)
                          ?? idpTypes[0];

            var fromDate = newProfile.userLogin.FromDate ?? utcNow; // GAP-fix #8: declare here
            if (newProfile.userLogin.ThruDate == null)
                newProfile.userLogin.ThruDate = new DateTime(9999, 12, 31);

            long   userId       = 0;
            Guid   personRealId = Guid.Empty;
            long   userEmailContactMechanismId = 0;
            bool   primaryOrganization = ResolvePrimaryOrganizationFlag(orgExists, newProfile);

            var orgList = new List<OrganizationPrimary>();

            // ── A. New user branch ────────────────────────────────────────
            if (existingOrgs.Count == 0)
            {
                processTracker = "Create Person";
                (userId, personRealId) = await CreatePersonAsync(conn, tx, newProfile, ct);

                processTracker = "Create UserLogin";
                userId = await CreateUserLoginAsync(conn, tx, newProfile, userId, ct);

                processTracker = "Update UserLogin Password";
                await UpdateUserLoginPasswordAsync(conn, tx, newProfile, orgPartyId, fromDate, ct);

                processTracker = "TelecommunicationNumbers";
                await SaveTelecommunicationNumbersAsync(conn, tx, newProfile, ct);

                processTracker = "Save Notification Email";
                userEmailContactMechanismId = await SaveNotificationEmailAsync(
                    conn, tx, newProfile, emailUsageTypes, utcNow, ct);

                orgList.Add(new OrganizationPrimary
                {
                    OrganizationRealPageId = orgRealPageId,
                    OrganizationPartyId    = orgPartyId,
                    PrimaryOrganization    = primaryOrganization,
                    OrganizationFromDate   = fromDate,
                    OrganizationThruDate   = newProfile.userLogin.ThruDate
                });
            }
            // ── B. Existing user branch ───────────────────────────────────
            else
            {
                userId       = userLoginOnly!.UserId;
                personRealId = userLoginOnly.RealPageId;
                newProfile.RealPageId = userLoginOnly.RealPageId;

                orgList = await ResolveOrganizationListForExistingUserAsync(
                    conn, tx, newProfile, userLoginOnly, externalOrg,
                    existingOrgs, orgPartyId, orgRealPageId,
                    currentPrimaryStatus, primaryOrganization, fromDate, ct);
            }

            // ── C. Pending email notification ─────────────────────────────
            processTracker = "Pending Email Notification";
            if (idpType.IsLocal)
                await SavePendingEmailNotificationAsync(conn, tx, newProfile, personRealId,
                    orgRealPageId, emailUsageTypes, userEmailContactMechanismId, utcNow, ct);

            // ── D. Resolve status ThruDate from org activity config ───────
            var primaryOrgId  = orgList.Any(o => o.PrimaryOrganization)
                                    ? orgList.First(o => o.PrimaryOrganization).OrganizationPartyId
                                    : currentPrimaryStatus?.PartyId ?? orgPartyId;

            var statusThruDate = await ResolveStatusThruDateAsync(conn, primaryOrgId, fromDate, ct);

            long assignUserPersonaId = 0;
            long userLoginPersonaId  = 0;
            int  greenBookRole       = 0; // GAP-fix #14: captured from LinkPersonaToRoleAsync for SaveProductDetails

            // ── E. Per-org: CreateUserLoginPersona → CreatePersona → LinkRole → SetUserType
            foreach (var org in orgList)
            {
                processTracker = $"CreateUserLoginPersona for org {org.OrganizationPartyId}";
                var (userStatus, currentStatusThruDate) = DetermineUserStatus(
                    org, externalOrg.PartyId, orgPartyId, idpType.IsLocal,
                    currentPrimaryStatus, fromDate, statusThruDate, newProfile,
                    existingOrgs.Count);

                userLoginPersonaId = await CreateUserLoginPersonaAsync(
                    conn, tx, userId, org, userStatus, currentStatusThruDate, newProfile, ct);

                processTracker = $"CreatePersona for org {org.OrganizationPartyId}";
                var newPersonaId = await CreatePersonaAsync(
                    conn, tx, userId, userLoginPersonaId, personRealId, org,
                    persona[0], currentPrimaryStatus, externalOrg.PartyId, fromDate, ct);

                if (newProfile.ExternalUserRelationship?.ThirdPartyRelationShipId > 0)
                    await SaveExternalUserRelationshipAsync(conn, tx, userLoginPersonaId, newProfile, ct);

                // GAP-fix #12: link persona to UnifiedUI enterprise role template (sync L:1204-1220)
                processTracker = $"InsertUpdateEnterpriseRoleToUser for org {org.OrganizationPartyId}";
                var enterpriseRoleBatch = newProfile.productBatch
                    ?.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedUI);
                if (enterpriseRoleBatch?.InputJson?.RoleList?.Count > 0)
                {
                    int roleTemplateId = Convert.ToInt32(enterpriseRoleBatch.InputJson.RoleList.FirstOrDefault());
                    await InsertUpdateEnterpriseRoleToUserAsync(conn, tx, roleTemplateId, newPersonaId, ct);
                }

                processTracker = $"LinkPersonaToRole for org {org.OrganizationPartyId}";
                greenBookRole = await LinkPersonaToRoleAsync(conn, tx, newPersonaId, org.OrganizationRealPageId,
                    newProfile, roleTypes, platformAdminRole, usePropertyInstanceUnifiedLogin, ct);

                processTracker = $"SetUserType for org {org.OrganizationPartyId}";
                await SetUserTypeAsync(conn, tx, personRealId, org, newProfile, roleTypes, ct);

                if (org.OrganizationPartyId == orgPartyId)
                {
                    assignUserPersonaId      = newPersonaId;
                    response.PersonaId       = newPersonaId;
                }

                processTracker = "Create EmployeeId";
                await CreateEmployeeIdAsync(conn, tx, userLoginPersonaId, newProfile.EmployeeId, ct);

                processTracker = "Create Supervisor";
                if (newProfile.SuperVisorUserId > 0)
                    await CreateSupervisorAsync(conn, tx, userId, newProfile, ct);
            }

            // ── F. Handle RP-Employee user-type update across previous orgs
            processTracker = "Update User Type for RP Employee";
            if (newProfile.UserTypeId == (int)UserRoleType.RealPageEmployee &&
                !existingOrgs.Any(x => x.PartyRoleTypeId == (int)UserRoleType.RealPageEmployee))
            {
                await UpdateUserTypeForRpEmployeeAsync(conn, tx, personRealId, existingOrgs, ct);
            }

            // ── G. Custom fields ──────────────────────────────────────────
            processTracker = "Create User Custom Fields";
            await SaveCustomFieldsAsync(conn, tx, userId, orgPartyId, newProfile, ct);

            // ── H. Identity Provider link ─────────────────────────────────
            processTracker = "Link Identity Provider";
            if (primaryOrgId == orgPartyId)
                await LinkIdentityProviderAsync(conn, tx, userId, idpType.ContactMechanismId, ct);

            // ── I. Product Batch ──────────────────────────────────────────
            processTracker = "Save Product Details";
            var creatorPersonaId = _userClaims.PersonaId;
            await SaveProductDetailsAsync(
                conn, tx, newProfile.productBatch ?? [], response,
                creatorPersonaId, assignUserPersonaId,
                _userClaims.UserRealPageGuid, orgRealPageId,
                userTypeId:         newProfile.UserTypeId,
                userIsActive:       true,
                impersonatorUserId: _userClaims.UserId,
                aoProducts:         aoProductsAvailableForUser,
                migratedUser:       newProfile.MigratedUser,
                isCreateUser:       true,
                unifiedPlatformRole: greenBookRole,
                operationType:      "add",
                roleIdList:         newProfile.RoleIdList,
                ct:                 ct);

            // ── J. Delegate Admin roles ───────────────────────────────────
            processTracker = "Enterprise Roles Delegate User";
            if (isDelegateAdmin && newProfile.IsDelegateAdmin &&
                newProfile.DelegateRoleTemplate?.RoleTemplateId?.Any() == true)
            {
                await InsertUpdateDelegateAdminRoleAsync(conn, tx, userLoginPersonaId,
                    newProfile.DelegateRoleTemplate.RoleTemplateId.ToList(), ct);
            }

            newProfile.userLogin.UserId    = userId;
            newProfile.userLogin.RealPageId = personRealId;
            response.UserStatus            = "User created successfully.";
            response.UserRealPageGuid      = personRealId;
            response.Status                = new Status<IErrorData> { Success = true };

            tx.Commit();
            _logger.LogInformation("CreateUserAsync succeeded. UserId={UserId} PersonaId={PersonaId}",
                userId, assignUserPersonaId);
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex,
                "CreateUserAsync failed at step '{ProcessTracker}' for login '{Login}'",
                processTracker, newProfile.userLogin.LoginName);
            return Failure(response, "User.CreateUser.24",
                $"Create User Error: {ex.Message}. Process: {processTracker}");
        }

        return response;
    }

    // ── UpdateNewUser ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateNewUserAsync(
        string userLogin, Profile newProfile, int partyRoleTypeId,
        string companyJobTitle, string activityToken, CancellationToken ct = default)
    {
        var response = new RepositoryResponse();

        var userLoginOnly = await _userLoginRepo.GetUserLoginOnlyAsync(userLogin);
        if (userLoginOnly is null)
        {
            response.ErrorMessage = "User Name is incorrect or not found.";
            return response;
        }

        // Resolve org — ListOrganizationByEnterpriseUserIdAsync gives the same data as ListOrganizationByRealPageId
        var orgList = await _userLoginRepo.ListOrganizationByEnterpriseUserIdAsync(userLoginOnly.RealPageId, string.Empty);
        var orgPartyId = orgList?.FirstOrDefault()?.PartyId ?? 0;

        // Validate one-time activity token via direct SP call (SP_GetActivityToken)
        using var readConn = _connectionFactory.GetReadOnlyConnection();
        readConn.Open();
        var tokenDetail = await readConn.QuerySingleOrDefaultAsync<dynamic>(new CommandDefinition(
            StoredProcNameConstants.SP_GetActivityToken,
            new { EnterpriseUserName  = userLogin,
                  ActivityToken       = activityToken,
                  ActivityTypeId      = (int)ActivityType.NewUserRegistrationVerification,
                  OrganizationPartyId = orgPartyId },
            commandType: CommandType.StoredProcedure, cancellationToken: ct));

        long enterpriseUserId = tokenDetail is not null ? (long)(tokenDetail.EnterpriseUserId ?? 0) : 0;
        if (enterpriseUserId <= 0)
        {
            response.ErrorMessage = "Validation token does not match with user.";
            return response;
        }

        using var conn = _connectionFactory.GetConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            var utcNow  = DateTime.UtcNow;
            var maxDate = DateTime.MaxValue.ToUniversalTime();

            // Update person name / title
            await conn.ExecuteAsync(new CommandDefinition(
                StoredProcNameConstants.SP_UpdatePerson,
                new { userLoginOnly.RealPageId, newProfile.Title,
                      newProfile.FirstName, newProfile.MiddleName, newProfile.LastName },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

            // Update UserLogin: status active, from/thru dates
            await conn.ExecuteAsync(new CommandDefinition(
                StoredProcNameConstants.SP_UpdateUserLogin,
                new { userLoginOnly.RealPageId, PartyId = orgPartyId,
                      FromDate = utcNow, ThruDate = maxDate },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

            // Link partyRoleType (job title / role) — SP_LinkPersonToOrganization
            await conn.ExecuteAsync(new CommandDefinition(
                StoredProcNameConstants.SP_LinkPersonToOrganization,
                new { PersonRealPageId = userLoginOnly.RealPageId,
                      OrganizationRealPageId = orgList?.FirstOrDefault()?.RealPageId,
                      RoleTypeIdFrom = partyRoleTypeId,
                      RoleTypeIdTo   = 0 },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

            // Update UserLoginPersona status → Active
            await conn.ExecuteAsync(new CommandDefinition(
                StoredProcNameConstants.SP_UpdateUserStatusByCompany,
                new { userLoginOnly.RealPageId, OrganizationPartyId = orgPartyId,
                      StatusTypeId = (int)UserUiStatusType.Active,
                      FromDate = utcNow, ThruDate = (DateTime?)null },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

            // Expire the activity token
            await conn.ExecuteAsync(new CommandDefinition(
                StoredProcNameConstants.SP_UpdateActivityAttempt,
                new { EnterpriseUserName = userLogin,
                      ActivityTypeId = (int)ActivityType.NewUserRegistrationVerification },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

            response.Id = 1;
            tx.Commit();
            _logger.LogInformation("UpdateNewUserAsync succeeded for '{UserLogin}'", userLogin);
        }
        catch (Exception ex)
        {
            tx.Rollback();
            response.ErrorMessage = ex.Message;
            _logger.LogError(ex, "UpdateNewUserAsync failed for '{UserLogin}'", userLogin);
        }

        return response;
    }

    // ── UpdateUser ────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateUserAsync(
        Guid loggedInUserRealPageId, IProfileDetail newProfile, IProfileDetail oldProfile,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(newProfile);

        using var conn = _connectionFactory.GetConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        var processTracker = "Init";
        try
        {
            var response = await UpdateUserDataAsync(conn, tx, loggedInUserRealPageId, newProfile, oldProfile, ct);
            tx.Commit();
            return response;
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "UpdateUserAsync failed at step '{Step}' for persona '{PersonaId}'",
                processTracker, newProfile.Persona?.FirstOrDefault()?.PersonaId);
            return new RepositoryResponse { ErrorMessage = $"Update User Error: {ex.Message}. Process: {processTracker}" };
        }
    }

    // ── UpdateUserListUser ────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateUserListUserAsync(
        ProfileDetail userProfile, IList<Persona> updatePersona,
        IList<Persona> deletePersona, int userTypeId, IList<Organization> listOrg,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(userProfile);

        var orgRealPageId = listOrg?.FirstOrDefault()?.RealPageId ?? Guid.Empty;
        var orgPartyId    = listOrg?.FirstOrDefault()?.PartyId    ?? 0;
        var personRealId  = userProfile.RealPageId;

        using var conn = _connectionFactory.GetConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        var processTracker = "Init";
        try
        {
            var response = new RepositoryResponse();
            var utcNow   = DateTime.UtcNow;
            var maxDate  = DateTime.MaxValue.ToUniversalTime();

            // Update UserLogin dates
            processTracker = "Update User Login";
            await conn.ExecuteAsync(new CommandDefinition(
                StoredProcNameConstants.SP_UpdateUserLogin,
                new { userProfile.RealPageId, PartyId = orgPartyId,
                      FromDate = utcNow, ThruDate = maxDate },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

            // Update user-type (party relationship)
            processTracker = "Update User Type";
            await UpdateUserTypeFromListAsync(conn, tx, personRealId, orgRealPageId, userTypeId, ct);

            // Add / update personas
            processTracker = "Update Persona List";
            foreach (var p in updatePersona ?? Enumerable.Empty<Persona>())
                await UpdatePersonaAsync(conn, tx, p, orgPartyId, ct);

            // Delete removed personas + their products
            processTracker = "Delete Persona List";
            foreach (var p in deletePersona ?? Enumerable.Empty<Persona>())
                await DeletePersonaAsync(conn, tx, p.PersonaId, ct);

            response.Id = 1;
            tx.Commit();
            _logger.LogInformation("UpdateUserListUserAsync succeeded for RealPageId={RealPageId}",
                userProfile.RealPageId);
            return response;
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex,
                "UpdateUserListUserAsync failed at '{Step}' for RealPageId={RealPageId}",
                processTracker, userProfile.RealPageId);
            return new RepositoryResponse { ErrorMessage = ex.Message };
        }
    }

    // ── DisableUserProduct ────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task DisableUserProductAsync(
        Guid createUserRealPageId, long createUserPersonaId,
        IList<UserLoginOnly> userLogins, CancellationToken ct = default)
    {
        var semaphore = new SemaphoreSlim(MaxProductDisableParallelism);
        var tasks = userLogins.Select(async ul =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                await DisableUserProductDataAsync(createUserRealPageId, createUserPersonaId, ul, ct);
            }
            finally
            {
                semaphore.Release();
            }
        });
        await Task.WhenAll(tasks);
    }

    // ── ActivateUserProducts ──────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task ActivateUserProductsAsync(
        Guid createUserRealPageId, long createUserPersonaId,
        IList<UserLoginOnly> userLogins, CancellationToken ct = default)
    {
        foreach (var ul in userLogins)
        {
            var persona = await _managePersona.GetFirstAvailablePersonaByCompanyAsync(
                ul.RealPageId, _userClaims.OrganizationPartyId, ct);
            if (persona is null) continue;

            await ProcessActivatedUserProductBatchAsync(
                persona.PersonaId, createUserRealPageId, createUserPersonaId, ct);
        }
    }

    // ── AssignProductsToAdministrators ────────────────────────────────────────

    /// <inheritdoc/>
    public async Task AssignProductsToAdministratorsAsync(
        Guid organizationRealPageId, long assignUserPersonaId = 0, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(organizationRealPageId, Guid.Empty,
            nameof(organizationRealPageId));

        var org         = await _orgRepo.GetOrganizationAsync(organizationRealPageId);
        var personaList = await _personaRepo.ListPersonaByOrganizationPartyIdAsync(
                              org.PartyId, isDefault: null, (int)UserRoleType.SuperUser, ct);

        if (assignUserPersonaId > 0)
            personaList = personaList.Where(p => p.PersonaId == assignUserPersonaId).ToList();

        using var conn = _connectionFactory.GetConnection();
        conn.Open();

        // Resolve RP-employee editor persona (shared across all admins)
        var rpAccessResult = await conn.QuerySingleOrDefaultAsync<dynamic>(new CommandDefinition(
            StoredProcNameConstants.SP_ListOrganizations,
            new { RealPageId = organizationRealPageId },
            commandType: CommandType.StoredProcedure, cancellationToken: ct));

        if (rpAccessResult is null) return;

        Guid rpAccessId     = new Guid(rpAccessResult.PersonRealPageId.ToString());
        long editorPersonaId = await conn.QuerySingleOrDefaultAsync<long>(new CommandDefinition(
            StoredProcNameConstants.SP_GetActivePersona,
            new { RealPageId = rpAccessId },
            commandType: CommandType.StoredProcedure, cancellationToken: ct));

        var impersonatorId = _userClaims.ImpersonatedBy != Guid.Empty
            ? (await _userLoginRepo.GetUserLoginOnlyAsync(_userClaims.ImpersonatedBy))?.UserId ?? 0
            : 0L;

        foreach (var p in personaList)
        {
            var ulp = await conn.QuerySingleOrDefaultAsync<UserLoginPersona>(new CommandDefinition(
                StoredProcNameConstants.SP_GetUserLoginPersona,
                new { UserLoginId = p.UserId, OrganizationPartyId = org.PartyId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));

            if (ulp is null) continue;
            if (ulp.StatusTypeId is 23 or 24) continue; // Disabled or Expired — skip

            await ProcessActivatedUserProductBatchAsync(p.PersonaId, rpAccessId, editorPersonaId, ct);
            _logger.LogDebug("AssignProductsToAdministrators: updated persona {PersonaId}", p.PersonaId);
        }
    }

    // ── ActivateSalesForceUser ────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task ActivateSalesForceUserAsync(
        Guid createUserRealPageId, long createUserPersonaId,
        IList<UserLoginOnly> userLogins, bool isAssigned, CancellationToken ct = default)
    {
        var impersonatorId = _userClaims.ImpersonatedBy != Guid.Empty
            ? (await _userLoginRepo.GetUserLoginOnlyAsync(_userClaims.ImpersonatedBy))?.UserId ?? 0
            : 0L;

        var editorPersona = await _managePersona.GetPersonaAsync(createUserPersonaId, withRights: false, ct);

        foreach (var ul in userLogins)
        {
            var persona = await _managePersona.GetPersonaAsync(
                (await _managePersona.GetFirstAvailablePersonaByCompanyAsync(
                    ul.RealPageId, _userClaims.OrganizationPartyId, ct))?.PersonaId ?? 0,
                withRights: false, ct);

            if (persona is null) continue;

            var sfBatch = new ProductBatch
            {
                ProductId     = (int)ProductEnum.SalesForce,
                StatusTypeId  = 5,
                RetryCount    = 0,
                InputJson     = new RolePropertyList
                {
                    PropertyList = new List<string>(),
                    RoleList     = new List<string>(),
                    IsAssigned   = isAssigned
                }
            };

            using var conn = _connectionFactory.GetConnection();
            conn.Open();
            await SaveProductBatchAsync(conn, null, sfBatch, createUserPersonaId,
                persona.PersonaId, createUserRealPageId, impersonatorId,
                JsonConvert.SerializeObject(sfBatch.InputJson), ct);

            _logger.LogDebug("ActivateSalesForceUser: queued SF batch for persona {PersonaId} isAssigned={IsAssigned}",
                persona.PersonaId, isAssigned);
        }
    }

    // ── ProcessDisabledUsers ──────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task ProcessDisabledUsersAsync(
        IList<ProcessUserLogin> userLogins, CancellationToken ct = default)
    {
        _logger.LogDebug("ProcessDisabledUsers: processing {Count} user logins", userLogins.Count);

        var impersonatorId = _userClaims.ImpersonatedBy != Guid.Empty
            ? (await _userLoginRepo.GetUserLoginOnlyAsync(_userClaims.ImpersonatedBy))?.UserId ?? 0
            : 0L;

        // Build a per-company admin-persona cache so we only look up once per company
        var companyAdminCache  = new Dictionary<Guid, Persona?>();
        var companyPartyIdCache = new Dictionary<Guid, long>();   // orgRealPageId → orgPartyId

        // Group by organisation to batch up SP_ListOrganizations lookups
        var byOrg = userLogins.GroupBy(u => u.OrganizationRealPageId);

        foreach (var orgGroup in byOrg)
        {
            var orgRealPageId = orgGroup.Key;

            if (!companyAdminCache.ContainsKey(orgRealPageId))
            {
                using var conn = _connectionFactory.GetReadOnlyConnection();
                conn.Open();
                var orgRow = await conn.QuerySingleOrDefaultAsync<dynamic>(new CommandDefinition(
                    StoredProcNameConstants.SP_ListOrganizations,
                    new { RealPageId = orgRealPageId },
                    commandType: CommandType.StoredProcedure, cancellationToken: ct));

                if (orgRow is not null)
                {
                    Guid rpAccessId = new Guid(Convert.ToString(orgRow.PersonRealPageId));
                    long orgPartyIdFromRow = (long)orgRow.PartyId;
                    var adminPersona = await _managePersona.GetFirstAvailablePersonaByCompanyAsync(
                        rpAccessId, orgPartyIdFromRow, ct);
                    companyAdminCache[orgRealPageId]   = adminPersona;
                    companyPartyIdCache[orgRealPageId] = orgPartyIdFromRow;
                }
                else
                {
                    companyAdminCache[orgRealPageId]   = null;
                    companyPartyIdCache[orgRealPageId] = 0;
                }
            }

            var editorPersona = companyAdminCache[orgRealPageId];
            var currentOrgPartyId = companyPartyIdCache.GetValueOrDefault(orgRealPageId);

            foreach (var ul in orgGroup)
            {
                var userLoginOnly = await _userLoginRepo.GetUserLoginOnlyAsync(ul.UserRealPageId);
                if (userLoginOnly is null) continue;

                var userLogin = await _userLoginRepo.GetUserLoginAsync(ul.UserRealPageId, currentOrgPartyId);
                if (userLogin is null || userLogin.StatusId == (int)UserUiStatusType.Disabled) continue;

                var persona = await _managePersona.GetFirstAvailablePersonaByCompanyAsync(
                    ul.UserRealPageId, currentOrgPartyId, ct);

                using var conn = _connectionFactory.GetConnection();
                conn.Open();

                var updateResult = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_UpdateUserStatusByCompany,
                        new { RealPageId = ul.UserRealPageId, OrganizationPartyId = currentOrgPartyId,
                              StatusTypeId = UserUiStatusType.Disabled, FromDate = ul.FromDate },
                        commandType: CommandType.StoredProcedure, cancellationToken: ct));

                if (updateResult?.Id > 0 && editorPersona is not null && persona is not null)
                {
                    await ProcessDisableUserProductDataAsync(
                        persona.PersonaId, editorPersona.RealPageId,
                        editorPersona.PersonaId, persona.UserTypeId,
                        impersonatorId, ct);

                    var thruDateCst = userLogin.ThruDate is not null
                        ? TimeZoneInfo.ConvertTime(userLogin.ThruDate.Value,
                            TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"))
                        : (DateTime?)null;

                    var msg = $"{userLoginOnly.LoginName} was deactivated by the system " +
                              $"due to the scheduled User Expires date. | " +
                              $"{(thruDateCst.HasValue ? thruDateCst.Value.ToShortDateString() + " " + thruDateCst.Value.ToShortTimeString() : string.Empty)} CST";

                    _logger.LogDebug("ProcessDisabledUsers: {Message}", msg);
                }
            }
        }
    }

    // ── ProcessDisableUserProductData ─────────────────────────────────────────

    /// <inheritdoc/>
    public async Task ProcessDisableUserProductDataAsync(
        long assignUserPersonaId, Guid createUserRealPageId,
        long createUserPersonaId, int? userTypeId, long impersonatorUserId,
        CancellationToken ct = default)
    {
        using var conn = _connectionFactory.GetConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            await ProcessDisableUserProductDataCoreAsync(
                conn, tx, assignUserPersonaId, createUserRealPageId,
                createUserPersonaId, userTypeId, impersonatorUserId, ct);
            tx.Commit();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex,
                "ProcessDisableUserProductDataAsync failed for persona {PersonaId}", assignUserPersonaId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task ProcessDisableUserProductDataInTransactionAsync(
        long assignUserPersonaId, Guid createUserRealPageId,
        long createUserPersonaId, int? userTypeId, long impersonatorUserId,
        IDbTransaction transaction, CancellationToken ct = default)
    {
        await ProcessDisableUserProductDataCoreAsync(
            transaction.Connection!, transaction,
            assignUserPersonaId, createUserRealPageId,
            createUserPersonaId, userTypeId, impersonatorUserId, ct);
    }

    // ── InsertNewPhoneNumberFromImport ────────────────────────────────────────

    /// <inheritdoc/>
    public async Task InsertNewPhoneNumberFromImportAsync(
        IProfileDetail profile, IDbTransaction transaction, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(transaction);

        var conn      = transaction.Connection!;
        var personaId = profile.Persona[0].PersonaId;
        var utcNow    = DateTime.UtcNow;
        var maxDate   = DateTime.MaxValue.ToUniversalTime();

        foreach (var phone in profile.TelecommunicationNumber ?? Enumerable.Empty<TelecommunicationNumber>())
        {
            // Create contact mechanism
            var mechResult = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_CreateContactMechanism,
                    new { ContactMechanismId = (long?)null },
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure, cancellationToken: ct));

            if (mechResult?.Id is null or 0) continue;

            // Link to party
            await conn.ExecuteAsync(new CommandDefinition(
                StoredProcNameConstants.SP_LinkContactMechanismToParty,
                new { profile.RealPageId, ContactMechanismId = mechResult.Id,
                      FromDate = utcNow, ThruDate = maxDate },
                transaction: transaction,
                commandType: CommandType.StoredProcedure, cancellationToken: ct));

            // Create telecoms number
            await conn.ExecuteAsync(new CommandDefinition(
                StoredProcNameConstants.SP_CreateTelecommunicationNumber,
                new { ContactMechanismId = mechResult.Id,
                      phone.CountryCode, phone.AreaCode,
                      ContactNumber = phone.PhoneNumber },
                transaction: transaction,
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        }
    }

    // ── ThirdPartyIdpBulkUpdate ───────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<RepositoryResponse> ThirdPartyIdpBulkUpdateAsync(
        IList<long> userIds, bool isEnabled, CancellationToken ct = default)
    {
        var response = new RepositoryResponse();
        if (userIds.Count == 0) return response;

        using var conn = _connectionFactory.GetConnection();
        conn.Open();

        var updatedIds = (await conn.QueryAsync<long>(new CommandDefinition(
            StoredProcNameConstants.SP_UpdateUsersIDP,
            new
            {
                OrganizationPartyId = _userClaims.OrganizationPartyId,
                UserIds  = TableValueParamHelper.ConvertToTableValuedParameter(
                               userIds.ToList(), "Enterprise.BigIntListType"),
                IsEnabled = isEnabled
            },
            commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();

        response.Id = updatedIds.Count;

        if (updatedIds.Count > 0)
            await _auditService.LogBulkIdpUpdateAsync(updatedIds, isEnabled, ct);

        _logger.LogInformation("ThirdPartyIdpBulkUpdate: {Count} users updated. isEnabled={IsEnabled}",
            updatedIds.Count, isEnabled);

        return response;
    }

    // ── UpdateUserStatusByCompany ─────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateUserStatusByCompanyAsync(
        Guid realPageId, long organizationPartyId,
        int statusTypeId, DateTime fromDate, DateTime? thruDate, CancellationToken ct = default)
    {
        using var conn = _connectionFactory.GetConnection();
        conn.Open();

        var result = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateUserStatusByCompany,
                new { realPageId, organizationPartyId, statusTypeId, fromDate, thruDate },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));

        result ??= new RepositoryResponse();

        // Orchestration: if disabling, also remove products for each affected persona
        if (result.Id > 0 && statusTypeId == (int)UserUiStatusType.Disabled)
        {
            var personasToDisable = await conn.QueryAsync<Persona>(new CommandDefinition(
                StoredProcNameConstants.SP_ListPersonaToDisableUserProduct,
                new { RealPageId = realPageId, OrganizationPartyId = organizationPartyId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));

            var impersonatorId = _userClaims.ImpersonatedBy != Guid.Empty
                ? (await _userLoginRepo.GetUserLoginOnlyAsync(_userClaims.ImpersonatedBy))?.UserId ?? 0
                : 0L;

            foreach (var persona in personasToDisable)
            {
                await ProcessDisableUserProductDataAsync(
                    persona.PersonaId, _userClaims.UserRealPageGuid,
                    _userClaims.PersonaId, persona.UserTypeId,
                    impersonatorId, ct);
            }

            _logger.LogInformation(
                "UpdateUserStatusByCompany: disabled {PersonaCount} persona products for RealPageId={RealPageId}",
                personasToDisable.Count(), realPageId);
        }

        return result;
    }

    // ── GetUnifiedSettingData ─────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<bool> GetUnifiedSettingDataAsync(string settingName, CancellationToken ct = default)
    {
        try
        {
            var settings = await _manageUnifiedSettings.GetCompanyInternalSettingsAsync(
                _userClaims.OrganizationRealPageGuid, "UPFM", "company", ct);

            return settings?.Keys
                       ?.FirstOrDefault(k => k.Name == settingName)
                       ?.Value == "1";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "GetUnifiedSettingDataAsync: could not read setting '{SettingName}', returning false",
                settingName);
            return false;
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<(long userId, Guid personRealId)> CreatePersonAsync(
        IDbConnection conn, IDbTransaction tx, ProfileDetail profile, CancellationToken ct)
    {
        var result = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreatePerson,
                new { profile.Title, FirstName = profile.FirstName, profile.MiddleName,
                      LastName = profile.LastName, profile.Suffix,
                      PreferredContactMethodId = 0, RealPageId = Guid.Empty },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

        if (result is null || result.ErrorMessage != string.Empty)
            throw new InvalidOperationException($"CreatePerson failed: {result?.ErrorMessage}");

        profile.RealPageId = result.RealPageId;
        profile.PartyId    = result.Id;
        return (result.Id, result.RealPageId);
    }

    private async Task<long> CreateUserLoginAsync(
        IDbConnection conn, IDbTransaction tx, ProfileDetail profile, long userId, CancellationToken ct)
    {
        var sourceType = profile.MigratedUser
            ? CreateUserSourceType.MigrationTool.ToString()
            : (profile.CreateUserSourceType?.ToString() ?? CreateUserSourceType.UnifiedPlatform.ToString());

        var result = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateUserLogin,
                new { profile.RealPageId, profile.userLogin.LoginName,
                      CreateUserSourceType = sourceType },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

        if (result?.Id is null or 0)
            throw new InvalidOperationException($"CreateUserLogin failed: {result?.ErrorMessage ?? "Username already exists!"}");

        return result.Id;
    }

    private async Task UpdateUserLoginPasswordAsync(
        IDbConnection conn, IDbTransaction tx, ProfileDetail profile,
        long orgPartyId, DateTime fromDate, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(profile.Password))
        {
            var pwd = profile.Password.PasswordHash();
            profile.userLogin.PasswordHash = pwd.PasswordHash;
            profile.userLogin.PasswordSalt = pwd.PasswordSalt;
        }

        await conn.ExecuteAsync(new CommandDefinition(
            StoredProcNameConstants.SP_UpdateUserLogin,
            new { profile.RealPageId, LoginName = profile.userLogin.LoginName,
                  profile.userLogin.PasswordHash, profile.userLogin.PasswordSalt,
                  FromDate = fromDate, ThruDate = profile.userLogin.ThruDate,
                  PartyId  = orgPartyId },
            transaction: tx, commandType: CommandType.StoredProcedure,
            cancellationToken: ct));
    }

    private async Task SaveTelecommunicationNumbersAsync(
        IDbConnection conn, IDbTransaction tx, ProfileDetail profile, CancellationToken ct)
    {
        if (profile.TelecommunicationNumber?.Count > 0)
            await InsertNewPhoneNumberFromImportAsync(profile, tx, ct);
    }

    private async Task<long> SaveNotificationEmailAsync(
        IDbConnection conn, IDbTransaction tx, ProfileDetail profile,
        IList<ContactMechanismUsageType> emailUsageTypes,
        DateTime utcNow, CancellationToken ct)
    {
        if (profile.UserTypeId != (int)UserRoleType.UserNoEmail)
        {
            if (string.IsNullOrEmpty(profile.NotificationEmail) &&
                EmailFormatValidation.IsValidEmail(profile.userLogin.LoginName))
                profile.NotificationEmail = profile.userLogin.LoginName;
        }

        if (string.IsNullOrEmpty(profile.NotificationEmail) ||
            !EmailFormatValidation.IsValidEmail(profile.NotificationEmail))
            return 0;

        var emailUsageType = emailUsageTypes.SingleOrDefault(
            p => p.Name.Equals("Email", StringComparison.OrdinalIgnoreCase));

        // Create contact mechanism
        var mechResult = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateContactMechanism,
                new { ContactMechanismId = (long?)null },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

        if (mechResult?.Id is null or 0)
            throw new InvalidOperationException("CreateContactMechanism failed.");

        var contactMechId = mechResult.Id;
        var maxDate       = DateTime.MaxValue.ToUniversalTime();

        // Link to party
        var linkResult = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_LinkContactMechanismToParty,
                new { profile.RealPageId, ContactMechanismId = contactMechId,
                      FromDate = utcNow, ThruDate = maxDate },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

        if (linkResult?.Id is null or 0)
            throw new InvalidOperationException("LinkContactMechanismToParty failed.");

        // Link usage type
        await conn.ExecuteAsync(new CommandDefinition(
            StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism,
            new { PartyContactMechanismId = linkResult.Id,
                  emailUsageType?.ContactMechanismUsageTypeId },
            transaction: tx, commandType: CommandType.StoredProcedure,
            cancellationToken: ct));

        // Create electronic address (email)
        var emailResult = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateElectronicAddress,
                new { ContactMechanismId = contactMechId,
                      ElectronicAddressString = profile.NotificationEmail,
                      ElectronicAddressType   = emailUsageType?.Name },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

        if (emailResult?.Id is null or 0)
            throw new InvalidOperationException("CreateElectronicAddress failed.");

        return linkResult.Id;
    }

    private async Task<long> CreateUserLoginPersonaAsync(
        IDbConnection conn, IDbTransaction tx, long userId,
        OrganizationPrimary org, int userStatusId, DateTime? statusThruDate,
        ProfileDetail profile, CancellationToken ct)
    {
        object param = _userClaims.ImpersonatedByName is not null
            ? new { UserLoginId = userId, StatusTypeId = userStatusId,
                    org.OrganizationPartyId, org.PrimaryOrganization,
                    FromDate = org.OrganizationFromDate, ThruDate = org.OrganizationThruDate,
                    StatusThruDate = statusThruDate, profile.IsRPEmployee,
                    profile.IsDelegateAdmin, profile.IsRealPartner }
            : new { UserLoginId = userId, StatusTypeId = userStatusId,
                    org.OrganizationPartyId, org.PrimaryOrganization,
                    FromDate = org.OrganizationFromDate, ThruDate = org.OrganizationThruDate,
                    StatusThruDate = statusThruDate, profile.IsRPEmployee,
                    profile.IsDelegateAdmin };

        var result = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateUserLoginPersona, param,
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

        if (result?.Id is null or 0)
            throw new InvalidOperationException("CreateUserLoginPersona failed: " + result?.ErrorMessage);

        return result.Id;
    }

    private async Task<long> CreatePersonaAsync(
        IDbConnection conn, IDbTransaction tx, long userId,
        long userLoginPersonaId, Guid personRealId, OrganizationPrimary org,
        Persona personaFromUI, OrganizationStatus? primaryOrgStatus,
        long externalOrgPartyId, DateTime fromDate, CancellationToken ct)
    {
        var personaTypeId = personaFromUI.Name?.ToLowerInvariant() switch
        {
            "system administrator" => (long)PersonaType.SuperUser,
            _                      => (long)PersonaType.Primary
        };
        if (org.OrganizationPartyId == externalOrgPartyId)
            personaTypeId = (long)PersonaType.Primary;

        DateTime? personaFrom = org.OrganizationPartyId == externalOrgPartyId && primaryOrgStatus is not null
            ? primaryOrgStatus.FromDate
            : personaFromUI.FromDate ?? fromDate;

        var result = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreatePersona,
                new { PersonRealPageId = personRealId, userLoginPersonaId,
                      org.OrganizationRealPageId, PersonaTypeId = personaTypeId,
                      UserId = userId,
                      PersonaEnvironmentTypeId = personaFromUI.PersonaEnvironmentTypeId,
                      FromDate = personaFrom, ThruDate = personaFromUI.ThruDate,
                      PersonaId = (long?)null },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

        if (result?.Id is null or 0)
            throw new InvalidOperationException("CreatePersona failed: " + result?.ErrorMessage);

        return result.Id;
    }

    // GAP-fix #13: multi-role loop + SP_AddUpdatePropertyMapping / SP_AddUpdatePropertyInstanceMapping
    private async Task<int> LinkPersonaToRoleAsync(
        IDbConnection conn, IDbTransaction tx, long personaId,
        Guid orgRealPageId, ProfileDetail profile,
        IList<RoleType> roleTypes, string? platformAdminRole,
        bool usePropertyInstanceUnifiedLogin,
        CancellationToken ct)
    {
        var enterpriseRoles = (await conn.QueryAsync<EnterpriseRole>(new CommandDefinition(
            StoredProcNameConstants.SP_SecurityListRolesByRealPageID,
            new { realPageId = orgRealPageId },
            transaction: tx, commandType: CommandType.StoredProcedure,
            cancellationToken: ct))).ToList();

        var superUser = roleTypes.SingleOrDefault(r =>
            r.Name.Equals("SuperUser", StringComparison.OrdinalIgnoreCase));
        var gbBatch = profile.productBatch
            ?.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedPlatform);

        // ── Resolve: single role or multi-role list ───────────────────────────
        int       greenBookRole  = 0;
        List<int> greenBookRoles = [];

        if (superUser?.PartyRoleTypeId == profile.UserTypeId && !string.IsNullOrEmpty(platformAdminRole))
        {
            // SuperUser → platform admin role (single)
            greenBookRole = enterpriseRoles
                .FirstOrDefault(r => r.Role.Equals(platformAdminRole, StringComparison.OrdinalIgnoreCase))
                ?.RoleId ?? 0;
        }
        else if (gbBatch?.InputJson?.RoleList?.Count > 0)
        {
            // UI/clone-prep supplied a list of roles (multi-role path, sync L:1245)
            greenBookRoles = gbBatch.InputJson.RoleList
                .Select(r => int.TryParse(r, out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();
        }
        else
        {
            // Fallback: org default role → "Basic End User" (sync L:1262-1268)
            var defaultRole = await conn.QuerySingleOrDefaultAsync<dynamic>(new CommandDefinition(
                StoredProcNameConstants.SP_GetUnifiedLoginDefaultRole,
                new { RealPageID = orgRealPageId },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));
            greenBookRole = defaultRole is not null
                ? (int)defaultRole.RoleId
                : (enterpriseRoles.FirstOrDefault(r =>
                       r.Role.Equals("Basic End User", StringComparison.OrdinalIgnoreCase))?.RoleId ?? 0);
        }

        // ── Link role(s) via SP_LinkPersonaToRole ─────────────────────────────
        bool roleIsLinked = false;

        if (greenBookRole > 0)
        {
            var result = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_LinkPersonaToRole,
                    new { personaID = personaId, roleID = greenBookRole,
                          CreatedBy = _userClaims.UserId, personaPrivilgeID = 0 },
                    transaction: tx, commandType: CommandType.StoredProcedure,
                    cancellationToken: ct));
            roleIsLinked = result?.Id > 0;
        }
        else
        {
            // Multi-role loop (sync L:1294-1311)
            foreach (var role in greenBookRoles)
            {
                var result = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_LinkPersonaToRole,
                        new { personaID = personaId, roleID = role,
                              CreatedBy = _userClaims.UserId, personaPrivilgeID = 0 },
                        transaction: tx, commandType: CommandType.StoredProcedure,
                        cancellationToken: ct));

                if (result?.Id is null or 0)
                {
                    roleIsLinked = false;
                    break;
                }
                roleIsLinked = true;
            }
        }

        if (!roleIsLinked)
            throw new InvalidOperationException(
                "There was an error associating the persona to a user role. [User.CreateUser.10]");

        // ── Property mapping (sync L:1324-1362) ───────────────────────────────
        // SuperUser: force PropertyList = ["-1"] (all properties)
        if (superUser?.PartyRoleTypeId == profile.UserTypeId
            && enterpriseRoles.Any(r =>
                r.Role.Equals(platformAdminRole, StringComparison.OrdinalIgnoreCase) && r.RoleId > 0))
        {
            gbBatch = new ProductBatch
            {
                InputJson = new RolePropertyList { PropertyList = ["-1"] }
            };
        }

        if (gbBatch?.InputJson?.PropertyList?.Count > 0
            || gbBatch?.InputJson?.RemovedPropertyList?.Count > 0)
        {
            var propertyJson = JsonConvert.SerializeObject(gbBatch);
            RepositoryResponse? mappingResult;

            if (!usePropertyInstanceUnifiedLogin)
            {
                mappingResult = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_AddUpdatePropertyMapping,
                        new { PersonaId = personaId, ProductId = (int)ProductEnum.UnifiedPlatform,
                              PropertyJSON = propertyJson },
                        transaction: tx, commandType: CommandType.StoredProcedure,
                        cancellationToken: ct));
            }
            else
            {
                mappingResult = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_AddUpdatePropertyInstanceMapping,
                        new { PersonaId = personaId, ProductId = (int)ProductEnum.UnifiedPlatform,
                              PropertyInstanceJSON = propertyJson },
                        transaction: tx, commandType: CommandType.StoredProcedure,
                        cancellationToken: ct));
            }

            if (mappingResult?.Id is null or 0)
                throw new InvalidOperationException(
                    $"Property mapping failed for persona {personaId}. [User.CreateUser.27]");
        }

        return greenBookRole > 0 ? greenBookRole : greenBookRoles.FirstOrDefault();
    }

    private async Task SetUserTypeAsync(
        IDbConnection conn, IDbTransaction tx,
        Guid personRealId, OrganizationPrimary org,
        ProfileDetail profile, IList<RoleType> roleTypes, CancellationToken ct)
    {
        var superUser    = roleTypes.SingleOrDefault(r => r.Name.Equals("SuperUser",       StringComparison.OrdinalIgnoreCase));
        var user         = roleTypes.SingleOrDefault(r => r.Name.Equals("User",            StringComparison.OrdinalIgnoreCase));
        var userNoEmail  = roleTypes.SingleOrDefault(r => r.Name.Equals("User (No Email)", StringComparison.OrdinalIgnoreCase));
        var rpEmployee   = roleTypes.SingleOrDefault(r => r.Name.Equals("realpage employee", StringComparison.OrdinalIgnoreCase));
        var externalUser = roleTypes.SingleOrDefault(r => r.Name.Equals("external user",   StringComparison.OrdinalIgnoreCase));

        var orgRoleTypes = (await conn.QueryAsync<RoleType>(new CommandDefinition(
            StoredProcNameConstants.SP_ListRoleType,
            new { RoleTypeName = "Organization Role" },
            transaction: tx, commandType: CommandType.StoredProcedure,
            cancellationToken: ct))).ToList();

        var userType  = orgRoleTypes.SingleOrDefault(r => r.Name == "User Type");
        var employer  = orgRoleTypes.SingleOrDefault(r => r.Name == "Employer");

        if (employer is null || userType is null || superUser is null || user is null)
            throw new InvalidOperationException("Required role types are missing.");

        var roleTypeIdFrom = profile.UserTypeId switch
        {
            (int)UserRoleType.SuperUser       => superUser.PartyRoleTypeId,
            (int)UserRoleType.RealPageEmployee => rpEmployee?.PartyRoleTypeId ?? user.PartyRoleTypeId,
            (int)UserRoleType.UserNoEmail     => userNoEmail?.PartyRoleTypeId ?? user.PartyRoleTypeId,
            (int)UserRoleType.ExternalUser    => externalUser?.PartyRoleTypeId ?? user.PartyRoleTypeId,
            _                                  => user.PartyRoleTypeId
        };

        var linkResult = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_LinkPersonToOrganization,
                new { PersonRealPageId = personRealId,
                      org.OrganizationRealPageId,
                      RoleTypeIdFrom = roleTypeIdFrom,
                      RoleTypeIdTo   = (int)userType.PartyRoleTypeId },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

        if (linkResult is null)
            throw new InvalidOperationException("LinkPersonToOrganization failed.");
    }

    private async Task SaveExternalUserRelationshipAsync(
        IDbConnection conn, IDbTransaction tx,
        long userLoginPersonaId, ProfileDetail profile, CancellationToken ct)
    {
        var rel = profile.ExternalUserRelationship!;
        var result = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateExternalUserRelationship,
                new { userLoginPersonaId,
                      ThirdPartyRelationshipId     = rel.ThirdPartyRelationShipId,
                      CompanyName                  = rel.ThirdPartyCompanyName,
                      ThirdPartyCompanyRealPageId  = rel.ThirdPartyCompanyRealPageId,
                      rel.OperatorCode, rel.OperatorValue },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

        if (result?.Id is null or 0)
            throw new InvalidOperationException("SaveExternalUserRelationship failed.");
    }

    private async Task CreateEmployeeIdAsync(
        IDbConnection conn, IDbTransaction tx,
        long userLoginPersonaId, string? employeeId, CancellationToken ct)
    {
        if (userLoginPersonaId <= 0) return;

        var result = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateEmployeeId,
                new { userLoginPersonaId, EmployeeId = employeeId },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

        if (result?.Id is null or 0)
            throw new InvalidOperationException("CreateEmployeeId failed.");
    }

    private async Task CreateSupervisorAsync(
        IDbConnection conn, IDbTransaction tx, long userId, ProfileDetail profile, CancellationToken ct)
    {
        var result = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_InsertUpdateSuperVisor,
                new { UserId = userId, profile.SuperVisorUserId },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

        if (result?.Id is null or 0)
            throw new InvalidOperationException("InsertUpdateSupervisor failed.");
    }

    private async Task SaveCustomFieldsAsync(
        IDbConnection conn, IDbTransaction tx,
        long userId, long orgPartyId, ProfileDetail profile, CancellationToken ct)
    {
        if (profile.CustomFields?.Count is null or 0) return;

        var ulpList = await conn.QueryAsync<UserLoginPersona>(new CommandDefinition(
            StoredProcNameConstants.SP_GetUserLoginPersona,
            new { UserLoginId = userId, OrganizationPartyId = orgPartyId },
            transaction: tx, commandType: CommandType.StoredProcedure,
            cancellationToken: ct));

        var ulpId = ulpList.FirstOrDefault()?.UserLoginPersonaId ?? 0;
        profile.CustomFields.ToList().ForEach(c => c.UserLoginPersonaId = ulpId);

        var json = JsonConvert.SerializeObject(profile.CustomFields);
        if (!ValidateJson.IsValidJson<IList<CustomFieldValue>>(json)) return;

        var result = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_AddUpdateFieldValue,
                new { JSON = json, CreatedBy = _userClaims.UserId },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

        if (result?.Id == 0 && !string.IsNullOrWhiteSpace(result?.ErrorMessage))
            throw new InvalidOperationException("SaveCustomFields failed: " + result.ErrorMessage);
    }

    private async Task LinkIdentityProviderAsync(
        IDbConnection conn, IDbTransaction tx, long userId, long contactMechanismId, CancellationToken ct)
    {
        var result = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_LinkIdentityProviderToUserLogin,
                new { UserId = userId, ContactMechanismID = contactMechanismId },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

        if (result?.Id is null or 0)
            throw new InvalidOperationException("LinkIdentityProviderToUserLogin failed.");
    }

    private async Task InsertUpdateDelegateAdminRoleAsync(
        IDbConnection conn, IDbTransaction tx,
        long userLoginPersonaId, List<int> templateRoleIds, CancellationToken ct)
    {
        foreach (var roleId in templateRoleIds)
        {
            await conn.ExecuteAsync(new CommandDefinition(
                StoredProcNameConstants.SP_UpdateEnterpriseRoleProductBatch,
                new { UserLoginPersonaId = userLoginPersonaId, RoleTemplateId = roleId },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));
        }
    }

    // GAP-fix #12: port of InsertUpdateEnterpriseRoleToUser (sync L:7710-7718)
    private async Task InsertUpdateEnterpriseRoleToUserAsync(
        IDbConnection conn, IDbTransaction tx,
        int roleTemplateId, long personaId, CancellationToken ct)
    {
        var result = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_InsertUpdateRoleTemplateUserMapping,
                new { RoleTemplateId = roleTemplateId, PersonaId = personaId },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

        if (result?.Id is null or 0)
            throw new InvalidOperationException(
                "User not assigned to Enterprise Role. [User.CreateUser.9]");
    }

    // GAP-fix #14: full port of sync SaveProductDetails with all missing parameters
    private async Task SaveProductDetailsAsync(
        IDbConnection conn, IDbTransaction tx,
        IList<ProductBatch> productList,
        CreateUserResponse<IErrorData> response,
        long creatorPersonaId, long assignPersonaId,
        Guid editorRealPageId, Guid orgRealPageId,
        int userTypeId, bool userIsActive,
        long impersonatorUserId,
        IList<string>? aoProducts = null,
        bool migratedUser = false,
        bool isCreateUser = false,
        int unifiedPlatformRole = 0,
        string operationType = "update",
        bool isRealpageAccessUser = false,
        IList<string>? roleIdList = null,
        CancellationToken ct = default)
    {
        if (productList.Count == 0) return;

        int batchProcessTypeId = (int)BatchProcessType.CreateUpdateProductUser;
        var vendorRoleIdList   = new List<string>();

        // ── 1. Extract UnifiedUI enterprise role ─────────────────────────────
        int enterpriseRoleId = 0;
        var primaryPropertyBatch = productList.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedUI);
        if (primaryPropertyBatch?.InputJson?.RoleList?.Count > 0)
        {
            enterpriseRoleId = Convert.ToInt32(primaryPropertyBatch.InputJson.RoleList.FirstOrDefault());
            productList.Remove(primaryPropertyBatch);
        }

        // ── 2. Role template mappings ─────────────────────────────────────────
        var roleTemplateProductRole = new List<RoleTemplateProductRole>();
        if (enterpriseRoleId > 0)
        {
            roleTemplateProductRole = (await conn.QueryAsync<RoleTemplateProductRole>(
                new CommandDefinition(StoredProcNameConstants.SP_GetRoleTemplateProductRoleMappings,
                    new { RoleTemplateId = enterpriseRoleId, OrganizationRealPageId = orgRealPageId },
                    transaction: tx, commandType: CommandType.StoredProcedure,
                    cancellationToken: ct))).ToList();
        }

        // ── 3. SuperUser branch: auto-assign all org products ─────────────────
        if (userIsActive && userTypeId == (int)UserRoleType.SuperUser)
        {
            var userProducts = (await conn.QueryAsync<PersonaProductUserDetails>(
                new CommandDefinition(StoredProcNameConstants.SP_ListProductsByPersonaId,
                    new { PersonaId = assignPersonaId },
                    transaction: tx, commandType: CommandType.StoredProcedure,
                    cancellationToken: ct))).ToList();

            var productsAssignedToCompany = await GetOrganizationProductListForAdminUserAsync(
                conn, tx, orgRealPageId, aoProducts, ct);

            if (roleIdList != null)
                vendorRoleIdList = roleIdList.ToList();

            var productListToCreate = new List<ProductBatch>();
            foreach (var prod in productsAssignedToCompany)
            {
                if (userProducts == null
                    || !userProducts.Any(a => a.ProductId == prod.ProductId)
                    || userProducts.Any(a => a.ProductId == prod.ProductId && a.ProductStatus == (int)ProductBatchStatusType.Deleted)
                    || userProducts.Any(a => a.ProductId == prod.ProductId && a.ProductStatus == (int)ProductBatchStatusType.Deactivated)
                    || isRealpageAccessUser)
                {
                    if (productListToCreate.All(a => a.ProductId != prod.ProductId))
                    {
                        productListToCreate.Add(new ProductBatch
                        {
                            ProductId            = prod.ProductId,
                            StatusTypeId         = 5,
                            RetryCount           = 0,
                            BatchProcessorGroupId = 0,
                            InputJson = new RolePropertyList
                            {
                                PropertyRoleList = new List<PropertyRoleList>(),
                                PropertyList     = new List<string>(),
                                RoleList         = prod.ProductId == (int)ProductEnum.VendorMarketplace
                                                       ? vendorRoleIdList : new List<string>(),
                                IsVendorRoleIdOverride = prod.ProductId == (int)ProductEnum.VendorMarketplace
                                                         && vendorRoleIdList?.Count > 0,
                                IsAssigned = true
                            }
                        });
                    }
                }
            }

            // Merge any explicitly submitted products (e.g. role overrides from UI)
            if (productList != null)
            {
                foreach (var product in productList)
                {
                    if (!productListToCreate.Any(p => p.ProductId == product.ProductId))
                        productListToCreate.Add(product);
                }
            }

            // GreenBook Cares + IsEditUserRequiresProduct filtering
            var creatorUserProducts = (await conn.QueryAsync<PersonaProductUserDetails>(
                new CommandDefinition(StoredProcNameConstants.SP_ListProductsByPersonaId,
                    new { PersonaId = creatorPersonaId },
                    transaction: tx, commandType: CommandType.StoredProcedure,
                    cancellationToken: ct))).ToList();

            var allProducts = (await conn.QueryAsync<GbProductMap>(
                new CommandDefinition(StoredProcNameConstants.SP_ListProduct, null,
                    transaction: tx, commandType: CommandType.StoredProcedure,
                    cancellationToken: ct))).ToList();

            foreach (var productmap in productListToCreate)
            {
                bool isGreenBookCaresEnabled = false;
                var cacheKey = $"productInternalSetting_{productmap.ProductId}";
                var productInternalSettingList = await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
                    return (await conn.QueryAsync<ProductInternalSetting>(
                        new CommandDefinition(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                            new { ProductId = productmap.ProductId },
                            commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();
                }) ?? new List<ProductInternalSetting>();

                var editUserRequiresProduct   = productInternalSettingList.FirstOrDefault(s => s.Name.Equals("IsEditUserRequiresProduct",     StringComparison.OrdinalIgnoreCase))?.Value;
                var greenbookCaresCheckRequired = productInternalSettingList.FirstOrDefault(s => s.Name.Equals("IsGreenbookCaresCheckRequired", StringComparison.OrdinalIgnoreCase))?.Value;

                bool isEditUserRequiresProduct    = editUserRequiresProduct    != null && editUserRequiresProduct    != "0";
                bool isGreenbookCaresCheckRequired = greenbookCaresCheckRequired != null && greenbookCaresCheckRequired != "0";

                if (isGreenbookCaresCheckRequired)
                {
                    var productDetails  = allProducts.FirstOrDefault(x => x.ProductId == productmap.ProductId);
                    string udmSource    = productDetails?.UDMSourceCode?.Length > 0 ? productDetails.UDMSourceCode! : (productDetails?.BooksProductCode ?? string.Empty);
                    var companyMapping  = await _manageBlueBook.GetProductCompanyMappingAsync(orgRealPageId, udmSource, ct);
                    var booksInstance   = await _manageBlueBook.GetCompanyInstanceByUPFMCompanyIdAsync(orgRealPageId.ToString().ToLower(), ct);
                    int customerCompanyId = booksInstance?.Attributes?.CustomerCompanyMap?.FirstOrDefault()?.CustomerCompanyId ?? 0;
                    string? domain       = booksInstance?.Attributes?.Domain;

                    if (!string.IsNullOrEmpty(domain) && customerCompanyId != 0)
                    {
                        var booksMap        = await _manageBlueBook.GetCustomerCompanyMapByCustomerCompanyIdAsync(customerCompanyId, domain, ct);
                        var findBooksCode   = booksMap?.Where(p => p.Source == udmSource);
                        if (findBooksCode?.Count() == 1)
                            isGreenBookCaresEnabled = findBooksCode.First().CompanyInstance?.FirstOrDefault()?.GreenBookCares ?? false;
                    }

                    if (companyMapping != null && isGreenBookCaresEnabled)
                    {
                        if (isEditUserRequiresProduct)
                        {
                            if (creatorUserProducts.Any(x => x.ProductId == productmap.ProductId && x.ProductStatus == 8)
                                && !productList.Any(p => p.ProductId == productmap.ProductId))
                                productList.Add(productmap);
                        }
                        else if (!productList.Any(p => p.ProductId == productmap.ProductId))
                            productList.Add(productmap);
                    }
                }
                else if (isEditUserRequiresProduct)
                {
                    if (creatorUserProducts.Any(x => x.ProductId == productmap.ProductId && x.ProductStatus == 8)
                        && !productList.Any(p => p.ProductId == productmap.ProductId))
                        productList.Add(productmap);
                }
                else if (!productList.Any(p => p.ProductId == productmap.ProductId))
                    productList.Add(productmap);
            }
        }

        // ── 4. Non-SuperUser + enterprise role → role-template products ───────
        if (userIsActive && userTypeId != (int)UserRoleType.SuperUser
            && !migratedUser && enterpriseRoleId > 0 && operationType == "add")
        {
            var roleTemplateProducts = (await conn.QueryAsync<int>(
                new CommandDefinition(StoredProcNameConstants.SP_GetEnterpriseRoleProductsByOrganization,
                    new { RoleTemplateId = enterpriseRoleId, OrganizationRealPageId = orgRealPageId },
                    transaction: tx, commandType: CommandType.StoredProcedure,
                    cancellationToken: ct))).ToList();

            foreach (var product in roleTemplateProducts)
            {
                batchProcessTypeId = (int)BatchProcessType.CreateUpdateProductUser;
                var productRoleData    = roleTemplateProductRole.Where(p => p.ProductId == product);
                var roleTemplateRoles  = productRoleData.Select(p => new { p.RoleTemplateProductRoleMappingId, p.ProductRoleId }).Distinct();
                var productRoles       = roleTemplateRoles
                    .Where(r => r.RoleTemplateProductRoleMappingId != 0)
                    .Select(r => r.ProductRoleId)
                    .ToList();

                if (product == (int)ProductEnum.UnifiedPlatform)
                {
                    int enterprisePlatformRole = Convert.ToInt32(productRoles.FirstOrDefault());
                    if (enterprisePlatformRole > 0 && unifiedPlatformRole > 0)
                    {
                        await UpdateGreenBookRoleInTransactionAsync(conn, tx,
                            new List<int> { enterprisePlatformRole },
                            assignPersonaId,
                            new List<long> { unifiedPlatformRole },
                            _userClaims.UserId, ct);
                    }
                }

                var productBatch = productList.FirstOrDefault(p => p.ProductId == product);
                if (productBatch == null && product != (int)ProductEnum.UnifiedPlatform)
                {
                    batchProcessTypeId = product == (int)ProductEnum.KnockCRM
                        ? (int)BatchProcessType.CreateUpdateProductUser
                        : (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser;

                    ProductBatch pb;
                    if (product == 94 && enterpriseRoleId > 0 && roleTemplateProductRole.Any(e => e.ProductId == 94))
                    {
                        var licenses = roleTemplateProductRole.Where(a => a.ProductId == 94)
                            .Select(r => r.AttributeValue).Distinct().ToList() ?? new List<string>();
                        pb = new ProductBatch
                        {
                            ProductId            = product, StatusTypeId = 5, RetryCount = 0,
                            BatchProcessorGroupId = 0,
                            InputJson = new RolePropertyList
                            {
                                PropertyRoleList = new List<PropertyRoleList>(), PropertyList = new List<string>(),
                                RoleList = productRoles, IsAssigned = true, IsAssignedNewPropertyByDefault = false,
                                UsePrimaryProperties = true,
                                RCLicenseDetails = new RCProductBatch { LearnerLicenseId = licenses, ManagerLicenseId = new List<string>() }
                            }
                        };
                    }
                    else
                    {
                        pb = new ProductBatch
                        {
                            ProductId            = product, StatusTypeId = 5, RetryCount = 0,
                            BatchProcessorGroupId = 0,
                            InputJson = new RolePropertyList
                            {
                                PropertyRoleList = new List<PropertyRoleList>(), PropertyList = new List<string>(),
                                RoleList = productRoles, IsAssigned = true, IsAssignedNewPropertyByDefault = false,
                                UsePrimaryProperties = true
                            }
                        };
                    }

                    if (!productList.Any(p => p.ProductId == product))
                        productList.Add(pb);
                }
            }
        }

        // ── 5. Cleanup: remove EasyLMS / ClientPortal; add SalesForce; AO-BM pair ─
        var easyLms = productList.FirstOrDefault(p => p.ProductId == (int)ProductEnum.EasyLMS);
        if (easyLms != null) productList.Remove(easyLms);

        if (isRealpageAccessUser)
        {
            var clientPortal = productList.FirstOrDefault(p => p.ProductId == (int)ProductEnum.ClientPortal);
            if (clientPortal != null) productList.Remove(clientPortal);
        }

        if (userTypeId != (int)UserRoleType.UserNoEmail
            && !productList.Any(p => p.ProductId == (int)ProductEnum.SalesForce))
        {
            productList.Add(new ProductBatch
            {
                ProductId = (int)ProductEnum.SalesForce, StatusTypeId = 5, RetryCount = 0,
                BatchProcessorGroupId = 0,
                InputJson = new RolePropertyList
                {
                    PropertyList = new List<string>(), RoleList = new List<string>(),
                    IsAssigned = isCreateUser || userIsActive
                }
            });
        }

        var aoPA = productList.FirstOrDefault(x => x.ProductId == (int)ProductEnum.AoPerformanceAnalytics);
        if (aoPA != null && aoPA.InputJson?.IsAssigned == false)
        {
            productList.Add(new ProductBatch
            {
                ProductId = (int)ProductEnum.AoBenchmarking, StatusTypeId = 5, RetryCount = 0,
                BatchProcessorGroupId = 0,
                InputJson = new RolePropertyList { PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = false }
            });
        }

        // ── 6. Save loop ──────────────────────────────────────────────────────
        if (productList.Count == 0 || creatorPersonaId <= 0 || assignPersonaId <= 0) return;

        // VendorMarketplace: creator persona = assign persona, resolve real realPageId from persona
        if (productList.Any(a => a.ProductId == (int)ProductEnum.VendorMarketplace) && vendorRoleIdList?.Count > 0)
        {
            creatorPersonaId = assignPersonaId;
            var persona = await conn.QuerySingleOrDefaultAsync<Persona>(
                new CommandDefinition(StoredProcNameConstants.SP_GetPersona,
                    new { personaId = assignPersonaId },
                    transaction: tx, commandType: CommandType.StoredProcedure,
                    cancellationToken: ct));
            if (persona != null) editorRealPageId = persona.RealPageId;
        }

        // OneSite + Lead2Lease must be combined into a single batch call
        if (userTypeId != (int)UserRoleType.SuperUser
            && productList.Any(a => a.ProductId == (int)ProductEnum.OneSite)
            && productList.Any(a => a.ProductId == (int)ProductEnum.Lead2Lease))
        {
            var pbOneSite    = productList.First(a => a.ProductId == (int)ProductEnum.OneSite);
            var pbLead2Lease = productList.FirstOrDefault(a => a.ProductId == (int)ProductEnum.Lead2Lease);

            var combined = new Dictionary<string, RolePropertyList>
            {
                [ProductEnum.OneSite.ToString()] = pbOneSite.InputJson
            };
            if (pbLead2Lease != null) combined[ProductEnum.Lead2Lease.ToString()] = pbLead2Lease.InputJson;

            await SaveProductBatchAsync(conn, tx, pbOneSite, creatorPersonaId, assignPersonaId,
                editorRealPageId, impersonatorUserId,
                JsonConvert.SerializeObject(combined), ct);

            productList.Remove(pbOneSite);
            if (pbLead2Lease != null) productList.Remove(pbLead2Lease);
        }

        // Bundle AO sub-products
        var aoInputJsonString = BundleAoProducts(productList);

        if (isRealpageAccessUser && creatorPersonaId == assignPersonaId)
            aoInputJsonString = JsonConvert.SerializeObject(
                new RolePropertyList { PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = true, CompanyId = 0 });

        // Save each remaining product
        foreach (var product in productList)
        {
            if (product.ProductId == (int)ProductEnum.UnifiedPlatform || product.ProductId == (int)ProductEnum.UnifiedUI)
                continue;

            product.BatchProcessorGroupId = 0;
            var inputJson = product.ProductId == (int)ProductEnum.AssetOptimizer
                ? aoInputJsonString
                : JsonConvert.SerializeObject(product.InputJson);

            await SaveProductBatchAsync(conn, tx, product, creatorPersonaId, assignPersonaId,
                editorRealPageId, impersonatorUserId, inputJson, ct);
        }
    }

    // Port of UpdateGreenBookRole — runs within an existing transaction
    private async Task UpdateGreenBookRoleInTransactionAsync(
        IDbConnection conn, IDbTransaction tx,
        List<int> newRoleIds, long assignPersonaId,
        List<long> existingRoleIds, long createdBy, CancellationToken ct)
    {
        if (newRoleIds.Count == 0) return;

        var newIds     = newRoleIds.ConvertAll(x => (long)x);
        var commonIds  = newIds.Intersect(existingRoleIds).ToList();
        newIds.RemoveAll(x => commonIds.Contains(x));
        existingRoleIds.RemoveAll(x => commonIds.Contains(x));

        foreach (var id in existingRoleIds)
        {
            await conn.ExecuteAsync(new CommandDefinition(
                StoredProcNameConstants.SP_LinkPersonaToRole,
                new { PersonaID = assignPersonaId, RoleID = id, IsDeleted = true, CreatedBy = createdBy, PersonaPrivilgeID = 0 },
                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
        }

        foreach (var id in newIds)
        {
            await conn.ExecuteAsync(new CommandDefinition(
                StoredProcNameConstants.SP_LinkPersonaToRole,
                new { PersonaID = assignPersonaId, RoleID = id, IsDeleted = false, CreatedBy = createdBy, PersonaPrivilgeID = 0 },
                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
        }
    }

    // Port of GetOrganizationProductListForAdminUser — async with IMemoryCache
    private async Task<List<ProductUI>> GetOrganizationProductListForAdminUserAsync(
        IDbConnection conn, IDbTransaction tx,
        Guid orgRealPageId, IList<string>? aoProducts, CancellationToken ct)
    {
        var cacheKey = $"getListProductsByOrganizationForAdminUser_{orgRealPageId}";
        var products = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3);
            return (await conn.QueryAsync<ProductUI>(
                new CommandDefinition(StoredProcNameConstants.SP_ListProductsByOrganizationForAdminUser,
                    new { OrganizationRealPageId = orgRealPageId },
                    transaction: tx, commandType: CommandType.StoredProcedure,
                    cancellationToken: ct))).ToList();
        }) ?? new List<ProductUI>();

        // Filter AO products to those the editor admin has access to
        foreach (var prod in products.Where(a => a.UDMSourceCode == "AO").ToList())
        {
            if (aoProducts == null || !aoProducts.Contains(prod.ProductCode))
                products.Remove(prod);
        }

        return products;
    }

    // Port of BundleAoProducts — pure static helper
    private static string BundleAoProducts(IList<ProductBatch> productList, int batchProcessorGroupId = 0)
    {
        var aoProductList = productList.Where(y => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)y.ProductId)).ToList();
        if (!aoProductList.Any()) return string.Empty;

        dynamic expandoList = new ExpandoObject();
        expandoList.IsAssigned = true;
        expandoList.AoUserCompanyPropertyRoleDetailList = new List<ExpandoObject>();

        foreach (var aoProduct in aoProductList)
        {
            dynamic expandoAo = new ExpandoObject();
            expandoAo.SelectedRoleValues      = aoProduct.InputJson.RoleList;
            expandoAo.SelectedPortfolioValues  = aoProduct.InputJson.PropertyList;
            expandoAo.CompanyId               = aoProduct.InputJson.CompanyId;
            expandoAo.Product                 = ProductEnumHelper.GetAoProductId((ProductEnum)aoProduct.ProductId);
            expandoAo.DivisionName            = ProductEnumHelper.GetAoDivisionName((ProductEnum)aoProduct.ProductId);
            expandoAo.PropertyGroups          = aoProduct.InputJson.PropertyGroupList;
            expandoAo.IsAssigned              = aoProduct.InputJson.IsAssigned;
            expandoAo.ProductId               = aoProduct.ProductId;
            expandoAo.UsePrimaryProperties    = aoProduct.InputJson.UsePrimaryProperties;
            expandoList.AoUserCompanyPropertyRoleDetailList.Add(expandoAo);
            productList.Remove(aoProduct);
        }

        var aoProductsBatch = new ProductBatch
        {
            ProductId = (int)ProductEnum.AssetOptimizer, StatusTypeId = 5, RetryCount = 0,
            BatchProcessorGroupId = batchProcessorGroupId, InputJson = null
        };
        productList.Add(aoProductsBatch);

        return JsonConvert.SerializeObject(expandoList);
    }

    private async Task SaveProductBatchAsync(
        IDbConnection conn, IDbTransaction? tx,
        IProductBatch product, long editorPersonaId, long assignPersonaId,
        Guid editorRealPageId, long impersonatorUserId,
        string inputJsonString, CancellationToken ct)
    {
        await conn.ExecuteAsync(new CommandDefinition(
            StoredProcNameConstants.SP_CreateProductBatch,
            new { EditorPersonaId  = editorPersonaId,
                  AssignPersonaId  = assignPersonaId,
                  EditorRealPageId = editorRealPageId,
                  product.ProductId, product.StatusTypeId,
                  product.RetryCount, product.BatchProcessorGroupId,
                  InputJson        = inputJsonString,
                  ImpersonatorUserId = impersonatorUserId },
            transaction: tx,
            commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    private async Task ProcessDisableUserProductDataCoreAsync(
        IDbConnection conn, IDbTransaction tx,
        long assignUserPersonaId, Guid createUserRealPageId,
        long createUserPersonaId, int? userTypeId,
        long impersonatorUserId, CancellationToken ct)
    {
        _logger.LogDebug(
            "ProcessDisableUserProductData: assignPersona={Assign} editorPersona={Editor}",
            assignUserPersonaId, createUserPersonaId);

        if (createUserPersonaId <= 0 || assignUserPersonaId <= 0) return;

        // Retrieve products currently active for the persona
        var productList = (await conn.QueryAsync<ProductBatch>(new CommandDefinition(
            StoredProcNameConstants.SP_ListProductsByPersonaId,
            new { PersonaId = assignUserPersonaId,
                  ProductStatusValue = ((int)ProductBatchStatusType.Success).ToString() },
            transaction: tx, commandType: CommandType.StoredProcedure,
            cancellationToken: ct))).ToList();

        if (productList.Count == 0) return;

        // Add SalesForce deactivation unless UserNoEmail
        if (userTypeId != (int)UserRoleType.UserNoEmail)
        {
            productList.Add(new ProductBatch
            {
                ProductId    = (int)ProductEnum.SalesForce,
                StatusTypeId = 5,
                InputJson    = new RolePropertyList { IsAssigned = false }
            });
        }

        // Create batch group (outer logging record)
        var batchGroup = await CreateBatchProcessGroupAsync(conn, tx, ct);

        // Load product setting types (cached 2 min)
        var settingTypes = await _cache.GetOrCreateAsync("listProductSettingType", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
            return (await conn.QueryAsync<ProductSettingType>(new CommandDefinition(
                StoredProcNameConstants.SP_ListProductSettingType, null,
                commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();
        });

        foreach (var product in productList.Where(p => p.ProductId != (int)ProductEnum.UnifiedPlatform))
        {
            product.BatchProcessorGroupId = batchGroup.BatchProcessorGroupId;
            await SaveProductBatchAsync(conn, tx, product, createUserPersonaId, assignUserPersonaId,
                createUserRealPageId, impersonatorUserId,
                JsonConvert.SerializeObject(product.InputJson), ct);
        }
    }

    private async Task<BatchProcessorGroup> CreateBatchProcessGroupAsync(
        IDbConnection conn, IDbTransaction? tx, CancellationToken ct)
    {
        var param = new DynamicParameters();
        param.Add("@BatchProcessorGroupID", 0, DbType.Int32, ParameterDirection.Output);
        try
        {
            await conn.ExecuteAsync(new CommandDefinition(
                StoredProcNameConstants.SP_CreateBatchProcessorGroup, param,
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

            return new BatchProcessorGroup
            {
                BatchProcessorGroupId           = param.Get<int>("@BatchProcessorGroupID"),
                BatchProcessorGroupActivityLogged = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateBatchProcessGroup failed — continuing with groupId=0");
            return new BatchProcessorGroup { BatchProcessorGroupId = 0 };
        }
    }

    private async Task ProcessActivatedUserProductBatchAsync(
        long personaId, Guid editorRealPageId, long editorPersonaId, CancellationToken ct)
    {
        using var conn = _connectionFactory.GetConnection();
        conn.Open();

        var products = await conn.QueryAsync<ProductBatch>(new CommandDefinition(
            StoredProcNameConstants.SP_ListProductsByPersonaId,
            new { PersonaId = personaId,
                  ProductStatusValue = ((int)ProductBatchStatusType.Success).ToString() },
            commandType: CommandType.StoredProcedure, cancellationToken: ct));

        foreach (var batch in products)
        {
            await SaveProductBatchAsync(conn, null, batch, editorPersonaId, personaId,
                editorRealPageId, 0, JsonConvert.SerializeObject(batch.InputJson), ct);
        }
    }

    private async Task DisableUserProductDataAsync(
        Guid createUserRealPageId, long createUserPersonaId,
        UserLoginOnly user, CancellationToken ct)
    {
        var userLoginOnly = await _userLoginRepo.GetUserLoginOnlyAsync(user.RealPageId);
        if (userLoginOnly is null) return;

        var existingOrgs       = await _userLoginRepo.ListOrganizationByLoginNameAsync(userLoginOnly.LoginName);
        var currentPrimaryStatus = await _userLoginRepo.GetUserOrganizationWithStatusAsync(
            userLoginOnly.UserId, userLoginOnly.LastLogin, 0, true);

        if (existingOrgs is null || currentPrimaryStatus is null) return;

        var persona = await _managePersona.GetFirstAvailablePersonaByCompanyAsync(
            user.RealPageId, _userClaims.OrganizationPartyId, ct);
        if (persona is null) return;

        var impersonatorId = _userClaims.ImpersonatedBy != Guid.Empty
            ? (await _userLoginRepo.GetUserLoginOnlyAsync(_userClaims.ImpersonatedBy))?.UserId ?? 0
            : 0L;

        await ProcessDisableUserProductDataAsync(
            persona.PersonaId, createUserRealPageId,
            createUserPersonaId, persona.UserTypeId,
            impersonatorId, ct);
    }

    private async Task<DateTime?> ResolveStatusThruDateAsync(
        IDbConnection conn, long primaryOrgId, DateTime fromDate, CancellationToken ct)
    {
        var activities = await conn.QueryAsync<Activity>(new CommandDefinition(
            StoredProcNameConstants.SP_ListActivity,
            new { PartyId = primaryOrgId },
            commandType: CommandType.StoredProcedure, cancellationToken: ct));

        var newUserActivity = activities.FirstOrDefault(
            a => a.ActivityTypeId == (int)ActivityType.NewUserRegistration);

        return newUserActivity is not null
            ? fromDate.AddMinutes(newUserActivity.ActivityTokenExpirationMinutes)
            : fromDate.AddHours(72);
    }

    private async Task<string?> ResolvePlatformAdminRoleAsync(CancellationToken ct)
    {
        var settings = await _productSettingRepo.GetProductInternalSettingsAsync(
            (int)ProductEnum.UnifiedPlatform, ct);
        return settings.FirstOrDefault(s =>
            s.Name.Equals("PlatformAdminRole", StringComparison.OrdinalIgnoreCase))?.Value;
    }

    private async Task<RepositoryResponse> UpdateUserDataAsync(
        IDbConnection conn, IDbTransaction tx,
        Guid loggedInUserRealPageId, IProfileDetail newProfile, IProfileDetail oldProfile,
        CancellationToken ct)
    {
        var response  = new RepositoryResponse { Id = oldProfile.Persona[0].PersonPartyId };
        var utcNow    = DateTime.UtcNow;
        var utcMaxVal = DateTime.MaxValue.ToUniversalTime();

        // ── Pre-fetch lookups ─────────────────────────────────────────────────
        var loginOnly = await _userLoginRepo.GetUserLoginOnlyAsync(newProfile.RealPageId)
                        ?? throw new InvalidOperationException("User login record not found.");

        var userPersonaOrgList   = await _userLoginRepo.ListOrganizationByLoginNameAsync(loginOnly.LoginName);
        var externalOrg          = await _orgRepo.GetOrganizationAsync(DefaultUserClaim.ExternalCompanyRealPageId);
        var idpTypeList          = oldProfile.Persona[0].Organization?.RealPageId != Guid.Empty
            ? (IList<IdentityProviderType>)await _orgRepo.GetOrganizationIdentityProviderTypeAsync(oldProfile.Persona[0].Organization.RealPageId)
            : [];
        var currentPrimaryStatus = await _userLoginRepo.GetUserOrganizationWithStatusAsync(newProfile.userLogin.UserId, loginOnly.LastLogin, 0, true);
        var currentOrgStatus     = await _userLoginRepo.GetUserOrganizationWithStatusAsync(newProfile.userLogin.UserId, loginOnly.LastLogin, oldProfile.Persona[0].OrganizationPartyId, false);
        var creatorPersonaId     = await _personaRepo.GetActivePersonaIdAsync(loggedInUserRealPageId, ct);
        var personaList          = (await _personaRepo.ListActivePersonaAsync(newProfile.RealPageId, true, ct)).ToList();
        var emailUsageTypes      = await _contactMechUsageTypeRepo.ListContactMechanismUsageTypeAsync("Email Notification", ct);
        var platformAdminRole    = await ResolvePlatformAdminRoleAsync(ct);
        var isDelegateAdmin      = await GetUnifiedSettingDataAsync("delegateadministrators", ct);

        // Enterprise roles list for GB role resolution
        var enterpriseRoles = (await conn.QueryAsync<EnterpriseRole>(new CommandDefinition(
            StoredProcNameConstants.SP_SecurityListRolesByRealPageID,
            new { realPageId = oldProfile.Persona[0].Organization.RealPageId },
            transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();

        // Existing GB role IDs assigned to the persona
        var existingRoleIds = (await conn.QueryAsync<dynamic>(new CommandDefinition(
            StoredProcNameConstants.SP_ListRolesForProductsByPersonaId,
            new { PersonaID = oldProfile.Persona[0].PersonaId, ProductID = (int)ProductEnum.UnifiedPlatform },
            transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct)))
            .Select(r => (long)Convert.ToInt64(r.RoleId)).ToList();

        // AO products (only needed for SuperUser editors)
        IList<string>? aoProducts = null;
        if (newProfile.UserTypeId == (int)UserRoleType.SuperUser)
            aoProducts = await GetEditorUserAoProductAsync(loggedInUserRealPageId, creatorPersonaId, oldProfile.Persona[0].Organization.RealPageId, ct);

        // Impersonator userId
        long impersonatorUserId = 0L;
        if (_userClaims.ImpersonatedBy != Guid.Empty)
            impersonatorUserId = (await _userLoginRepo.GetUserLoginOnlyAsync(_userClaims.ImpersonatedBy))?.UserId ?? 0L;

        // ── Pure-logic flags ──────────────────────────────────────────────────
        bool userIsExternalEverywhere = userPersonaOrgList.All(x => x.PartyRoleTypeId == (int)UserRoleType.ExternalUser);
        var  userBatch       = ComputeUserBatch(newProfile, oldProfile, userIsExternalEverywhere);
        bool profileChanged  = !string.Equals(newProfile.FirstName,  oldProfile.FirstName,  StringComparison.OrdinalIgnoreCase)
                            || !string.Equals(newProfile.MiddleName, oldProfile.MiddleName, StringComparison.OrdinalIgnoreCase)
                            || !string.Equals(newProfile.LastName,   oldProfile.LastName,   StringComparison.OrdinalIgnoreCase);
        bool loginNameChanged    = !newProfile.userLogin.LoginName.Equals(loginOnly.LoginName, StringComparison.OrdinalIgnoreCase);
        bool employeeIdChanged   = !string.Equals(newProfile.EmployeeId ?? "", oldProfile.EmployeeId ?? "", StringComparison.OrdinalIgnoreCase);
        bool supervisorIdChanged = newProfile.SuperVisorUserId != oldProfile.SuperVisorUserId;
        bool isPrimaryOrg        = currentOrgStatus.PrimaryOrganization;
        bool fromExternal        = userBatch.UserTypeChangedToFromExternal.Equals("FromExternal", StringComparison.OrdinalIgnoreCase);

        if (!isPrimaryOrg)
            personaList = personaList.Where(p => p.OrganizationPartyId == currentOrgStatus.PartyId).ToList();

        var productBatchData = newProfile.productBatch;

        // These track audit-relevant state; no DB use but kept for future audit logging parity with sync.
        bool isEnterpriseRolesUpdated   = false;
        bool isEnterpriseRoleUnassigned = false;
        bool isPrimaryPropsUpdated      = false;
        _ = isEnterpriseRolesUpdated;
        _ = isEnterpriseRoleUnassigned;
        _ = isPrimaryPropsUpdated;

        // ── Get UserLoginPersona list ─────────────────────────────────────────
        var ulpList = (await conn.QueryAsync<UserLoginPersona>(new CommandDefinition(
            StoredProcNameConstants.SP_GetUserLoginPersona,
            new { UserLoginId = newProfile.userLogin.UserId, OrganizationPartyId = oldProfile.Persona[0].OrganizationPartyId },
            transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();

        // ── Update Person ─────────────────────────────────────────────────────
        if (profileChanged && (isPrimaryOrg || fromExternal))
        {
            var r = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                StoredProcNameConstants.SP_UpdatePerson,
                new { RealPageId = newProfile.RealPageId, FirstName = newProfile.FirstName,
                      MiddleName = newProfile.MiddleName, LastName   = newProfile.LastName },
                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
            if (r?.Id == 0)
                throw new InvalidOperationException("Update User Error: Update person failed.");
            response.Id = r?.Id ?? response.Id;
        }

        // ── Resolve IDP type ──────────────────────────────────────────────────
        var idpt = idpTypeList.FirstOrDefault(a => a.IsLocal == !newProfile.userLogin.Is3rdPartyIDP)
                   ?? idpTypeList.FirstOrDefault();

        if (response.Id != 0)
        {
            // ── Link Identity Provider ────────────────────────────────────────
            if (isPrimaryOrg || fromExternal)
            {
                var r = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                    StoredProcNameConstants.SP_LinkIdentityProviderToUserLogin,
                    new { UserId = newProfile.userLogin.UserId, ContactMechanismID = idpt?.ContactMechanismId },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
                if (r?.Id == 0)
                    throw new InvalidOperationException("Update User Error: Link Identity Provider to UserLogin failed.");
            }

            // ── Update RealPartner ────────────────────────────────────────────
            if (oldProfile.IsRealPartner != newProfile.IsRealPartner && !string.IsNullOrEmpty(_userClaims.ImpersonatedByName))
            {
                var r = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                    StoredProcNameConstants.SP_UpdateUserLoginPersona,
                    new { UserLoginId        = newProfile.userLogin.UserId,
                          StatusTypeId       = currentPrimaryStatus.StatusTypeId,
                          OrganizationPartyId = oldProfile.Persona[0].OrganizationPartyId,
                          Primaryorganization = true,
                          StatusThruDate     = currentPrimaryStatus.StatusThruDate,
                          IsRealPartner      = newProfile.IsRealPartner },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
                if (r?.Id == 0)
                    throw new InvalidOperationException("Update User Error: Update RealPartner failed.");
            }

            // ── Update UserLogin ──────────────────────────────────────────────
            if (newProfile.userLogin != null)
            {
                bool isFeatureUser = newProfile.userLogin.FromDate.HasValue
                                     && newProfile.userLogin.FromDate.Value.Date > DateTime.Now.Date;

                if (!newProfile.userLogin.ThruDate.HasValue)
                    newProfile.userLogin.ThruDate = new DateTime(9999, 12, 31);

                string loginNameToUse = (isPrimaryOrg || fromExternal)
                    ? newProfile.userLogin.LoginName
                    : loginOnly.LoginName;

                var loginResult = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                    StoredProcNameConstants.SP_UpdateUserLogin,
                    new { RealPageId = newProfile.RealPageId, LoginName = loginNameToUse,
                          FromDate   = newProfile.userLogin.FromDate,
                          ThruDate   = newProfile.userLogin.ThruDate,
                          PartyId    = oldProfile.Persona[0].OrganizationPartyId },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
                if (loginResult?.Id == 0)
                    throw new InvalidOperationException("Update User Error: Update user login detail failed.");

                // Handle user type changes to/from External
                if (idpt != null && !string.IsNullOrEmpty(userBatch.UserTypeChangedToFromExternal))
                {
                    await ChangeUserTypeExternalAsync(conn, tx, externalOrg, currentPrimaryStatus,
                        newProfile, oldProfile.Persona[0], userPersonaOrgList, emailUsageTypes,
                        loginOnly, idpt, userBatch.UserTypeChangedToFromExternal, ct);
                }

                // Custom fields
                if (newProfile.CustomFields?.Count > 0)
                {
                    if (newProfile.CustomFields.Any(c => c.UserLoginPersonaId == 0) && ulpList.Count > 0)
                        newProfile.CustomFields.ToList().ForEach(c => c.UserLoginPersonaId = ulpList[0].UserLoginPersonaId);

                    var cfJson = JsonConvert.SerializeObject(newProfile.CustomFields);
                    if (ValidateJson.IsValidJson<IList<CustomFieldValue>>(cfJson))
                    {
                        var cfResult = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                            StoredProcNameConstants.SP_AddUpdateFieldValue,
                            new { JSON = cfJson, CreatedBy = _userClaims.UserId },
                            transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
                        if (cfResult?.Id == 0 && !string.IsNullOrWhiteSpace(cfResult.ErrorMessage))
                            throw new InvalidOperationException($"Update User Error: Update custom fields failed: {cfResult.ErrorMessage}");
                    }
                }

                // Status change logic
                bool isAccessLevelChanged  = newProfile.userLogin.Is3rdPartyIDP != loginOnly.Is3rdPartyIDP;
                bool isEffectiveDateFuture = newProfile.userLogin.FromDate.HasValue
                                            && newProfile.userLogin.FromDate.Value > DateTime.UtcNow;

                if (newProfile.userLogin.IsActive != currentOrgStatus.IsActive || isAccessLevelChanged || isEffectiveDateFuture)
                {
                    DateTime?       statusThruDate = null;
                    UserUiStatusType statusTypeId  = UserUiStatusType.UnDefined;

                    // Disabled-to-active + local IDP + never logged in → Pending
                    if (currentOrgStatus.StatusTypeId == (int)UserUiStatusType.Disabled
                        && newProfile.userLogin.IsActive == true
                        && loginOnly.LastLogin == null
                        && idpt?.IsLocal == true)
                        statusTypeId = UserUiStatusType.Pending;
                    else
                        statusTypeId = UserUiStatusType.Active;

                    if (newProfile.userLogin.FromDate.HasValue && newProfile.userLogin.ThruDate.HasValue
                        && newProfile.userLogin.FromDate.Value <= DateTime.UtcNow
                        && newProfile.userLogin.ThruDate.Value.ToUniversalTime() < DateTime.UtcNow)
                    {
                        statusTypeId   = UserUiStatusType.Disabled;
                        statusThruDate = null;
                    }

                    if (newProfile.userLogin.IsActive == false && currentOrgStatus.IsActive == true)
                    {
                        statusTypeId   = UserUiStatusType.Disabled;
                        statusThruDate = null;
                    }

                    // Was pending/disabled or was 3rd-party + never logged-in + switching to local → keep Pending
                    if (idpt?.IsLocal == true
                        && (newProfile.userLogin.Status == UserUiStatusType.Disabled
                            || newProfile.userLogin.Status == UserUiStatusType.Pending
                            || (loginOnly.Is3rdPartyIDP == true && loginOnly.LastLogin == null)))
                    {
                        if (currentOrgStatus.FromDate > DateTime.UtcNow
                            && newProfile.userLogin.FromDate.HasValue
                            && newProfile.userLogin.FromDate.Value <= DateTime.UtcNow)
                            statusTypeId = UserUiStatusType.Pending;
                    }

                    if (isEffectiveDateFuture)
                    {
                        statusTypeId   = UserUiStatusType.Disabled;
                        statusThruDate = null;
                    }

                    if (statusTypeId == UserUiStatusType.Pending)
                        statusThruDate = await ResolveStatusThruDateAsync(conn, _userClaims.OrganizationPartyId,
                            newProfile.userLogin.FromDate!.Value, ct);

                    if (statusTypeId != UserUiStatusType.UnDefined)
                    {
                        await conn.ExecuteAsync(new CommandDefinition(
                            StoredProcNameConstants.SP_UpdateUserStatusByCompany,
                            new { RealPageId         = newProfile.RealPageId,
                                  OrganizationPartyId = oldProfile.Persona[0].OrganizationPartyId,
                                  StatusTypeId       = statusTypeId,
                                  FromDate           = newProfile.userLogin.FromDate,
                                  StatusThruDate     = statusThruDate },
                            transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
                    }
                }
            }

            // ── Update Persona type (if user type changed) ────────────────────
            if (userBatch.UserTypeChanged)
            {
                int personaTypeId = userBatch.BatchProcessUserType switch
                {
                    (int)BatchProcessType.UserTypeAdminToRegular
                    or (int)BatchProcessType.UserTypeAdminToExternal => (int)PersonaType.Primary,
                    (int)BatchProcessType.UserTypeRegularToAdmin
                    or (int)BatchProcessType.UserTypeExternalToAdmin => (int)PersonaType.SuperUser,
                    _ => 0
                };
                if (personaTypeId > 0)
                {
                    var pr = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                        StoredProcNameConstants.SP_UpdatePersona,
                        new { PersonaId              = oldProfile.Persona[0].PersonaId,
                              PersonaTypeId          = personaTypeId,
                              PersonaEnvironmentTypeId = oldProfile.Persona[0].PersonaEnvironmentTypeId },
                        transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
                    if (pr?.Id == 0)
                        throw new InvalidOperationException("Persona name was not associated to the Persona.");
                }
            }

            // ── Update User Type ──────────────────────────────────────────────
            var relType = await conn.QuerySingleOrDefaultAsync<PartyRelationship>(new CommandDefinition(
                StoredProcNameConstants.SP_GetPartyRelationshipByRealPageId,
                new { RealPageIdFrom     = newProfile.RealPageId,
                      RealPageIdTo       = oldProfile.Persona[0].Organization?.RealPageId ?? Guid.Empty,
                      RoleTypeName       = (string?)null,
                      RelationshipTypeName = "User Type" },
                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));

            if (relType is not null && relType.RoleTypeIdFrom != newProfile.UserTypeId)
            {
                var utr = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                    StoredProcNameConstants.SP_UpdatePersonToOrganization,
                    new { PersonRealPageId       = newProfile.RealPageId,
                          OrganizationRealPageId  = oldProfile.Persona[0].Organization?.RealPageId ?? Guid.Empty,
                          UnlinkRoleTypeIdFrom   = relType.RoleTypeIdFrom,
                          LinkRoleTypeIdFrom     = newProfile.UserTypeId,
                          RoleTypeIdTo           = relType.RoleTypeIdTo },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
                if (utr?.Id == 0)
                    throw new InvalidOperationException("Update User Error: Unable to set new user type.");
            }

            // ── Update notification email ─────────────────────────────────────
            bool isFeatureUserForEmail = newProfile.userLogin.FromDate.HasValue
                                        && newProfile.userLogin.FromDate.Value.Date > DateTime.Now.Date;

            var (priorEmail, priorPCMId, priorCMId) = await GetExistingNotificationEmailAsync(
                conn, tx, newProfile.RealPageId, emailUsageTypes, ct);

            bool isContactMechUpdated = false;
            if (priorCMId != 0)
            {
                if (newProfile.UserTypeId != (int)UserRoleType.UserNoEmail
                    && (loginNameChanged || userBatch.IsUserTypeChangedFromNoEmailToRegular || userBatch.IsUserTypeChangedFromNoEmailToExternal))
                {
                    await conn.ExecuteAsync(new CommandDefinition(
                        StoredProcNameConstants.SP_CreateElectronicAddress,
                        new { ContactMechanismId = priorCMId, ElectronicAddressString = newProfile.userLogin.LoginName },
                        transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
                    isContactMechUpdated = true;
                }
            }
            else if (newProfile.UserTypeId != (int)UserRoleType.UserNoEmail)
            {
                newProfile.NotificationEmail = newProfile.userLogin.LoginName;
            }

            bool endExistingEmail = (!string.IsNullOrEmpty(priorEmail) && string.IsNullOrEmpty(newProfile.NotificationEmail))
                                 || (!string.IsNullOrEmpty(priorEmail) && !string.IsNullOrEmpty(newProfile.NotificationEmail)
                                     && !priorEmail.Equals(newProfile.NotificationEmail, StringComparison.OrdinalIgnoreCase));

            if (endExistingEmail)
            {
                var expR = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                    StoredProcNameConstants.SP_ExpirePartyContactMechanism,
                    new { RealPageId = newProfile.RealPageId, PartyContactMechanismId = priorPCMId },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
                if (expR?.Id == 0)
                    throw new InvalidOperationException("An error was encountered when ending a contact mechanism.");
            }

            if (!isContactMechUpdated
                && !string.IsNullOrEmpty(newProfile.NotificationEmail)
                && (isFeatureUserForEmail || !(priorEmail ?? "").Equals(newProfile.NotificationEmail ?? "", StringComparison.OrdinalIgnoreCase))
                && EmailFormatValidation.IsValidEmail(newProfile.NotificationEmail))
            {
                var emailType = emailUsageTypes.SingleOrDefault(p => p.Name.Equals("Email", StringComparison.OrdinalIgnoreCase));

                // Create contact mechanism chain
                var mechR = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                    StoredProcNameConstants.SP_CreateContactMechanism, new { ContactMechanismId = (long?)null },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
                if (mechR?.Id is null or 0) throw new InvalidOperationException("CreateContactMechanism failed.");
                long newCMId = mechR.Id;

                var linkR = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                    StoredProcNameConstants.SP_LinkContactMechanismToParty,
                    new { newProfile.RealPageId, ContactMechanismId = newCMId,
                          FromDate = utcNow, ThruDate = utcMaxVal },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
                if (linkR?.Id is null or 0) throw new InvalidOperationException("LinkContactMechanismToParty failed.");
                long newPCMId = linkR.Id;

                await conn.ExecuteAsync(new CommandDefinition(
                    StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism,
                    new { PartyContactMechanismId = newPCMId, emailType?.ContactMechanismUsageTypeId },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));

                await conn.ExecuteAsync(new CommandDefinition(
                    StoredProcNameConstants.SP_CreateElectronicAddress,
                    new { ContactMechanismId = newCMId, ElectronicAddressString = newProfile.NotificationEmail, ElectronicAddressType = emailType?.Name },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));

                // "Pending email notification" for future-dated users
                if (idpt?.IsLocal == true && isFeatureUserForEmail)
                {
                    var orgCMs = await conn.QueryAsync<CommonAddress>(new CommandDefinition(
                        StoredProcNameConstants.SP_ListContactMechanismsForPerson,
                        new { RealPageId = oldProfile.Persona[0].Organization.RealPageId },
                        transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
                    long fromPCMId = orgCMs.FirstOrDefault()?.PartyContactMechanismId ?? 0;
                    if (fromPCMId > 0 && newPCMId > 0)
                    {
                        await conn.ExecuteAsync(new CommandDefinition(
                            StoredProcNameConstants.SP_CreateCommunicationEvent,
                            new { StatusTypeID              = (int)EmailStatusType.EmailPending,
                                  FromPartyContactMechanismId = fromPCMId,
                                  ToPartyContactMechanismId   = newPCMId,
                                  Started                    = utcNow, Ended = utcNow,
                                  Note                       = "pending",
                                  CommunicationEventID       = (long?)null },
                            transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
                    }
                }
            }

            // ── Update UserEmployeeId ─────────────────────────────────────────
            if (employeeIdChanged)
            {
                if (oldProfile.UserEmployeeId > 0)
                {
                    var empR = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                        StoredProcNameConstants.SP_UpdateEmployeeId,
                        new { oldProfile.UserEmployeeId, newProfile.EmployeeId },
                        transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
                    if (empR?.Id == 0) throw new InvalidOperationException("An error was encountered when updating user employee.");
                }
                else if (ulpList.Count > 0)
                {
                    var empR = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                        StoredProcNameConstants.SP_CreateEmployeeId,
                        new { ulpList[0].UserLoginPersonaId, newProfile.EmployeeId },
                        transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
                    if (empR?.Id == 0) throw new InvalidOperationException("An error was encountered when creating user employee ID.");
                }
            }

            // ── Update Supervisor ─────────────────────────────────────────────
            if (supervisorIdChanged)
            {
                await conn.ExecuteAsync(new CommandDefinition(
                    StoredProcNameConstants.SP_InsertUpdateSuperVisor,
                    new { UserId = newProfile.userLogin.UserId, SuperVisorUserId = newProfile.SuperVisorUserId },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
            }

            // ── Update delegate roles ─────────────────────────────────────────
            if (isDelegateAdmin && (newProfile.IsDelegateAdmin || oldProfile.IsDelegateAdmin) && ulpList.Count > 0)
            {
                if (newProfile.IsDelegateAdmin != oldProfile.IsDelegateAdmin)
                {
                    await conn.ExecuteAsync(new CommandDefinition(
                        StoredProcNameConstants.SP_UpdateDelegateAdminStatus,
                        new { IsDelegateAdmin = newProfile.IsDelegateAdmin, UserLoginPersonaId = ulpList[0].UserLoginPersonaId },
                        transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
                }
                await InsertUpdateDelegateAdminRoleAsync(conn, tx, ulpList[0].UserLoginPersonaId,
                    newProfile.DelegateRoleTemplate?.RoleTemplateId?.ToList() ?? [], ct);
            }

            // ── Update external user company association ───────────────────────
            if (newProfile.ExternalUserRelationship?.ThirdPartyRelationShipId > 0
                && (newProfile.ExternalUserRelationship.OperatorCode != oldProfile.ExternalUserRelationship?.OperatorCode
                    || newProfile.ExternalUserRelationship.OperatorValue != oldProfile.ExternalUserRelationship?.OperatorValue))
            {
                await conn.ExecuteAsync(new CommandDefinition(
                    StoredProcNameConstants.SP_UpdateExternalUserRelationship,
                    new { UserLoginPersonaId          = newProfile.ExternalUserRelationship.UserLoginPersonaId,
                          ThirdPartyRelationshipId    = newProfile.ExternalUserRelationship.ThirdPartyRelationShipId,
                          CompanyName                 = newProfile.ExternalUserRelationship.ThirdPartyCompanyName,
                          ThirdPartyCompanyRealPageId = newProfile.ExternalUserRelationship.ThirdPartyCompanyRealPageId,
                          OperatorCode                = newProfile.ExternalUserRelationship.OperatorCode,
                          OperatorValue               = newProfile.ExternalUserRelationship.OperatorValue },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
            }

            // ── Update Enterprise role ────────────────────────────────────────
            var entRoleBatch = newProfile.productBatch.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedUI);
            if (entRoleBatch?.InputJson?.RoleList?.Count > 0)
            {
                int rtId = Convert.ToInt32(entRoleBatch.InputJson.RoleList.FirstOrDefault());
                if (rtId != 0)
                {
                    isEnterpriseRolesUpdated = true;
                    await InsertUpdateEnterpriseRoleToUserAsync(conn, tx, rtId, oldProfile.Persona[0].PersonaId, ct);
                }
                else
                {
                    await conn.ExecuteAsync(new CommandDefinition(
                        StoredProcNameConstants.SP_UnassignEnterpriseRoleFromUser,
                        new { PersonaId = oldProfile.Persona[0].PersonaId },
                        transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
                }
            }
            else if (oldProfile.RoleTemplateId > 0 && newProfile.RoleTemplateId == 0)
            {
                isEnterpriseRoleUnassigned = true;
                await conn.ExecuteAsync(new CommandDefinition(
                    StoredProcNameConstants.SP_UnassignEnterpriseRoleFromUser,
                    new { PersonaId = oldProfile.Persona[0].PersonaId },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
            }

            // ── Assign batch group IDs ────────────────────────────────────────
            if (productBatchData?.Count > 0)
            {
                var batchGroup = await CreateBatchProcessGroupAsync(conn, tx, ct);
                foreach (var item in productBatchData)
                    item.BatchProcessorGroupId = batchGroup.BatchProcessorGroupId;
            }

            // ── SaveProductDetails (active, user type unchanged) ──────────────
            if (newProfile.userLogin.IsActive.GetBooleanValue() && !userBatch.UserTypeChanged)
            {
                await SaveProductDetailsAsync(
                    conn, tx, productBatchData ?? [], new CreateUserResponse<IErrorData>(),
                    creatorPersonaId, oldProfile.Persona[0].PersonaId,
                    loggedInUserRealPageId, oldProfile.Persona[0].Organization.RealPageId,
                    userTypeId:       newProfile.UserTypeId,
                    userIsActive:     true,
                    impersonatorUserId: impersonatorUserId,
                    aoProducts:       aoProducts,
                    migratedUser:     false,
                    isCreateUser:     false,
                    unifiedPlatformRole: 0,
                    operationType:    "update",
                    ct:               ct);
            }

            // ── Disable all company products (inactive user) ──────────────────
            if (!newProfile.userLogin.IsActive.GetBooleanValue())
            {
                foreach (var compPersona in personaList)
                {
                    await ProcessDisableUserProductDataCoreAsync(
                        conn, tx, compPersona.PersonaId, loggedInUserRealPageId,
                        creatorPersonaId, newProfile.UserTypeId, impersonatorUserId, ct);
                }
            }

            // ── GreenBook role resolution ─────────────────────────────────────
            var greenBookRoles = new List<int>();
            var gbBatch = newProfile.productBatch.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedPlatform);

            if (gbBatch != null)
            {
                greenBookRoles = gbBatch.InputJson?.RoleList?
                    .Select(r => int.Parse(r)).ToList() ?? [];

                if (greenBookRoles.Count == 0 || userBatch.UserTypeChanged)
                {
                    greenBookRoles.Clear();
                    greenBookRoles.Add(await GetUnifiedPlatformDefaultRoleAsync(
                        conn, tx, oldProfile.Persona[0].Organization.RealPageId, enterpriseRoles, ct));
                }
            }
            else if (userBatch.UserTypeChanged)
            {
                var roleTypes = (await conn.QueryAsync<RoleType>(new CommandDefinition(
                    StoredProcNameConstants.SP_ListRoleType, new { RoleTypeName = "User Role" },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();

                var superUserRole = roleTypes.SingleOrDefault(p => p.Name == "SuperUser");
                if (superUserRole?.PartyRoleTypeId == newProfile.UserTypeId)
                {
                    var adminRole = enterpriseRoles.FirstOrDefault(r => r.Role.Equals(platformAdminRole, StringComparison.OrdinalIgnoreCase));
                    if (adminRole?.RoleId > 0)
                    {
                        greenBookRoles.Add(adminRole.RoleId);
                        gbBatch = new ProductBatch
                        {
                            ProductId = (int)ProductEnum.UnifiedPlatform,
                            InputJson = new RolePropertyList { PropertyList = new List<string> { "-1" } }
                        };
                    }
                }
                else
                {
                    greenBookRoles.Add(await GetUnifiedPlatformDefaultRoleAsync(
                        conn, tx, oldProfile.Persona[0].Organization.RealPageId, enterpriseRoles, ct));
                }
            }

            await UpdateGreenBookRoleInTransactionAsync(conn, tx,
                new List<int>(greenBookRoles), oldProfile.Persona[0].PersonaId,
                new List<long>(existingRoleIds), loginOnly.UserId, ct);

            // ── Property instance mapping ─────────────────────────────────────
            if (gbBatch?.InputJson?.PropertyList?.Count > 0 || gbBatch?.InputJson?.RemovedPropertyList?.Count > 0)
            {
                isPrimaryPropsUpdated = true;
                await conn.ExecuteAsync(new CommandDefinition(
                    StoredProcNameConstants.SP_AddUpdatePropertyInstanceMapping,
                    new { PersonaId          = oldProfile.Persona[0].PersonaId,
                          ProductId          = (int)ProductEnum.UnifiedPlatform,
                          PropertyInstanceJSON = JsonConvert.SerializeObject(gbBatch) },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
            }
        }

        response.RealPageId = newProfile.RealPageId;
        _logger.LogInformation("UpdateUserDataAsync succeeded for persona {PersonaId}", oldProfile.Persona[0].PersonaId);
        return response;
    }

    // Port of sync GetUserBatch (UserRepository.cs L:6075-6173) — pure logic, no DB calls
    private static UserBatchEntity ComputeUserBatch(
        IProfileDetail newProfile, IProfileDetail oldProfile, bool userIsExternalEverywhere)
    {
        if (newProfile.UserTypeId == oldProfile.UserTypeId) return new UserBatchEntity();

        int oldType = oldProfile.UserTypeId;
        int newType = newProfile.UserTypeId;

        if (oldType == (int)UserRoleType.User     && newType == (int)UserRoleType.SuperUser)
            return new UserBatchEntity { BatchProcessUserType = (int)BatchProcessType.UserTypeRegularToAdmin, UserTypeChanged = true };
        if (oldType == (int)UserRoleType.User     && newType == (int)UserRoleType.ExternalUser)
            return new UserBatchEntity { UserTypeChangedToFromExternal = "ToExternal" };
        if (oldType == (int)UserRoleType.SuperUser && newType == (int)UserRoleType.User)
            return new UserBatchEntity { BatchProcessUserType = (int)BatchProcessType.UserTypeAdminToRegular, UserTypeChanged = true };
        if (oldType == (int)UserRoleType.SuperUser && newType == (int)UserRoleType.ExternalUser)
            return new UserBatchEntity { UserTypeChangedToFromExternal = "ToExternal", BatchProcessUserType = (int)BatchProcessType.UserTypeAdminToExternal, UserTypeChanged = true };
        if (oldType == (int)UserRoleType.UserNoEmail && newType == (int)UserRoleType.User)
            return new UserBatchEntity { IsUserTypeChangedFromNoEmailToRegular = true };
        if (oldType == (int)UserRoleType.UserNoEmail && newType == (int)UserRoleType.ExternalUser)
            return new UserBatchEntity { IsUserTypeChangedFromNoEmailToExternal = true, UserTypeChangedToFromExternal = "ToExternal" };
        if (oldType == (int)UserRoleType.ExternalUser && newType == (int)UserRoleType.SuperUser && userIsExternalEverywhere)
            return new UserBatchEntity { UserTypeChangedToFromExternal = "FromExternal", BatchProcessUserType = (int)BatchProcessType.UserTypeExternalToAdmin, UserTypeChanged = true };
        if (oldType == (int)UserRoleType.ExternalUser && newType == (int)UserRoleType.User && userIsExternalEverywhere)
            return new UserBatchEntity { UserTypeChangedToFromExternal = "FromExternal" };

        return new UserBatchEntity();
    }

    // Port of sync ChangeUserTypeExternal (UserRepository.cs L:5501-5735)
    private async Task ChangeUserTypeExternalAsync(
        IDbConnection conn, IDbTransaction tx,
        Organization externalOrg, OrganizationStatus currentPrimaryStatus,
        IProfileDetail profile, IPersona persona,
        IList<UserOrganization> userPersonaOrgList,
        IList<ContactMechanismUsageType> emailUsageTypes,
        IUserLoginOnly userLoginOnly,
        IIdentityProviderType idpt,
        string userTypeChangedToFromExternal, CancellationToken ct)
    {
        bool isExternalEverywhere = userPersonaOrgList.All(x => x.PartyRoleTypeId == (int)UserRoleType.ExternalUser);

        // ── ToExternal: user was primary in this org, org count > 1 ──────────
        if (userTypeChangedToFromExternal.Equals("ToExternal", StringComparison.OrdinalIgnoreCase)
            && userPersonaOrgList.Count > 1
            && userPersonaOrgList.Any(x => x.OrganizationPartyId == persona.OrganizationPartyId && x.PrimaryOrganization))
        {
            // 1. Create UserLoginPersona for ExternalUsers org (primary)
            var ulpR = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                StoredProcNameConstants.SP_CreateUserLoginPersona,
                new { UserLoginId       = profile.userLogin.UserId,
                      StatusTypeId      = profile.userLogin.Status,
                      OrganizationPartyId = externalOrg.PartyId,
                      PrimaryOrganization = true,
                      FromDate          = currentPrimaryStatus.FromDate,
                      ThruDate          = currentPrimaryStatus.ThruDate,
                      StatusThruDate    = currentPrimaryStatus.StatusThruDate },
                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
            if (ulpR?.Id == 0)
                throw new InvalidOperationException("Update User Error: Add to External Users as primary failed.");

            // 2. Create Persona for ExternalUsers org
            var personaR = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                StoredProcNameConstants.SP_CreatePersona,
                new { PersonRealPageId        = profile.RealPageId,
                      UserLoginPersonaId       = ulpR!.Id,
                      OrganizationRealPageId   = externalOrg.RealPageId,
                      PersonaTypeId            = (int)PersonaType.Primary,
                      UserId                   = profile.userLogin.UserId,
                      PersonaEnvironmentTypeId = persona.PersonaEnvironmentTypeId,
                      FromDate                 = currentPrimaryStatus.FromDate,
                      ThruDate                 = persona.ThruDate,
                      personaId                = (long?)null },
                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
            long newPersonaId = personaR?.Id ?? 0;

            // 3. Link to Basic End User role for ExternalUsers
            var extRoles = (await conn.QueryAsync<EnterpriseRole>(new CommandDefinition(
                StoredProcNameConstants.SP_SecurityListRolesByRealPageID,
                new { realPageId = externalOrg.RealPageId },
                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();

            int basicEndUserRoleId = extRoles.FirstOrDefault(r => r.Role.Equals("Basic End User", StringComparison.OrdinalIgnoreCase))?.RoleId ?? 0;
            if (newPersonaId > 0 && basicEndUserRoleId > 0)
            {
                var roleR = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                    StoredProcNameConstants.SP_LinkPersonaToRole,
                    new { personaID = newPersonaId, roleID = basicEndUserRoleId,
                          CreatedBy = _userClaims.UserId, personaPrivilgeID = 0 },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
                if (roleR?.Id == 0)
                    throw new InvalidOperationException("Update User Error: Linking Persona to role for External Users failed.");
            }

            // 4. Get role types and link person type to external org
            var orgRoleTypes  = (await conn.QueryAsync<RoleType>(new CommandDefinition(
                StoredProcNameConstants.SP_ListRoleType, new { RoleTypeName = "Organization Role" },
                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();
            var userTypeInOrgRoleType = orgRoleTypes.SingleOrDefault(p => p.Name.Equals("User Type", StringComparison.OrdinalIgnoreCase));
            var userRoleTypes = (await conn.QueryAsync<RoleType>(new CommandDefinition(
                StoredProcNameConstants.SP_ListRoleType, new { RoleTypeName = "User Role" },
                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();

            int roleTypeIdFrom = profile.UserTypeId switch
            {
                (int)UserRoleType.User          => userRoleTypes.SingleOrDefault(p => p.Name == "User")?.PartyRoleTypeId                 ?? 0,
                (int)UserRoleType.SuperUser      => userRoleTypes.SingleOrDefault(p => p.Name == "SuperUser")?.PartyRoleTypeId            ?? 0,
                (int)UserRoleType.RealPageEmployee => userRoleTypes.SingleOrDefault(p => p.Name.Equals("realpage employee", StringComparison.OrdinalIgnoreCase))?.PartyRoleTypeId ?? 0,
                (int)UserRoleType.UserNoEmail    => userRoleTypes.SingleOrDefault(p => p.Name.Equals("User (No Email)", StringComparison.OrdinalIgnoreCase))?.PartyRoleTypeId    ?? 0,
                (int)UserRoleType.ExternalUser   => userRoleTypes.SingleOrDefault(p => p.Name.Equals("external user", StringComparison.OrdinalIgnoreCase))?.PartyRoleTypeId      ?? 0,
                _                               => userRoleTypes.SingleOrDefault(p => p.Name == "User")?.PartyRoleTypeId                 ?? 0
            };

            var linkR = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                StoredProcNameConstants.SP_LinkPersonToOrganization,
                new { PersonRealPageId     = profile.RealPageId,
                      OrganizationRealPageId = externalOrg.RealPageId,
                      RoleTypeIdFrom       = roleTypeIdFrom,
                      RoleTypeIdTo         = userTypeInOrgRoleType?.PartyRoleTypeId ?? 0 },
                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
            if (linkR is null)
                throw new InvalidOperationException("Update User Error: Link person to External Users failed.");

            // 5. Pending email notification for local IDP
            if (idpt.IsLocal)
            {
                var orgCMs = (await conn.QueryAsync<CommonAddress>(new CommandDefinition(
                    StoredProcNameConstants.SP_ListContactMechanismsForPerson,
                    new { RealPageId = externalOrg.RealPageId },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();
                var userCMs = (await conn.QueryAsync<CommonAddress>(new CommandDefinition(
                    StoredProcNameConstants.SP_ListContactMechanismsForPerson,
                    new { RealPageId = profile.RealPageId },
                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();

                long orgPCMId  = orgCMs.FirstOrDefault()?.PartyContactMechanismId ?? 0;
                long userPCMId = userCMs.FirstOrDefault()?.PartyContactMechanismId ?? 0;
                var utcNow = DateTime.UtcNow;
                if (orgPCMId > 0 && userPCMId > 0)
                {
                    await conn.ExecuteAsync(new CommandDefinition(
                        StoredProcNameConstants.SP_CreateCommunicationEvent,
                        new { StatusTypeID              = (int)EmailStatusType.EmailPending,
                              FromPartyContactMechanismId = orgPCMId,
                              ToPartyContactMechanismId   = userPCMId,
                              Started                    = utcNow, Ended = utcNow,
                              Note                       = "pending",
                              CommunicationEventID       = (long?)null },
                        transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
                }
            }
        }

        // ── FromExternal: user was external everywhere, moving to non-external ─
        if (userTypeChangedToFromExternal.Equals("FromExternal", StringComparison.OrdinalIgnoreCase) && isExternalEverywhere)
        {
            // Unlink from ExternalUsers org
            var unlinkR = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                StoredProcNameConstants.SP_UnlinkPersonToOrganization,
                new { PersonRealPageId = profile.RealPageId, OrganizationRealPageId = externalOrg.RealPageId },
                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
            if (unlinkR?.Id == 0)
                throw new InvalidOperationException("Update User Error: Unlink user from External Users failed.");

            // Set current org as primary
            var setPrimaryR = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                StoredProcNameConstants.SP_UpdateUserLoginPersona,
                new { UserLoginId       = profile.userLogin.UserId,
                      StatusTypeId      = currentPrimaryStatus.StatusTypeId,
                      OrganizationPartyId = persona.OrganizationPartyId,
                      Primaryorganization = true,
                      StatusThruDate    = currentPrimaryStatus.StatusThruDate },
                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
            if (setPrimaryR?.Id == 0)
                throw new InvalidOperationException("Update User Error: Update UserLoginPersona for FromExternal failed.");
        }
    }

    // Fetch existing notification email contact mechanism for a person
    private async Task<(string priorEmail, long partyContactMechId, long contactMechId)> GetExistingNotificationEmailAsync(
        IDbConnection conn, IDbTransaction tx,
        Guid realPageId, IList<ContactMechanismUsageType> emailUsageTypes, CancellationToken ct)
    {
        var contacts = await conn.QueryAsync<CommonAddress>(new CommandDefinition(
            StoredProcNameConstants.SP_ListContactMechanismsForPerson,
            new { RealPageId = realPageId },
            transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));

        foreach (var cm in contacts)
        {
            if (emailUsageTypes.Any(u => u.ContactMechanismUsageTypeId == cm.ContactMechanismUsageTypeId))
                return (cm.AddressString, cm.PartyContactMechanismId, cm.ContactMechanismId);
        }
        return ("", 0, 0);
    }

    // Port of sync GetUnifiedPlatformDefaultRole (UserRepository.cs L:7973-7988)
    private async Task<int> GetUnifiedPlatformDefaultRoleAsync(
        IDbConnection conn, IDbTransaction tx,
        Guid orgRealPageId, IList<EnterpriseRole> enterpriseRoles, CancellationToken ct)
    {
        var row = await conn.QuerySingleOrDefaultAsync<dynamic>(new CommandDefinition(
            StoredProcNameConstants.SP_GetUnifiedLoginDefaultRole,
            new { RealPageID = orgRealPageId },
            transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));

        return row is not null
            ? Convert.ToInt32(row.RoleId)
            : enterpriseRoles.FirstOrDefault(r => r.Role == "Basic End User")?.RoleId ?? 0;
    }

    private async Task UpdateUserTypeFromListAsync(
        IDbConnection conn, IDbTransaction tx,
        Guid personRealId, Guid orgRealPageId, int userTypeId, CancellationToken ct)
    {
        // Mirror of the UpdateUser #region "Update User Type" logic
        var relType = await conn.QuerySingleOrDefaultAsync<PartyRelationship>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetPartyRelationshipByRealPageId,
                new { realPageIdFrom = personRealId, realPageIdTo = orgRealPageId,
                      roleTypeName = (string?)null, relationshipTypeName = "User Type" },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

        if (relType is null || relType.RoleTypeIdFrom == userTypeId) return;

        await conn.ExecuteAsync(new CommandDefinition(
            StoredProcNameConstants.SP_UpdatePersonToOrganization,
            new { personRealId, orgRealPageId,
                  unlinkRoleTypeIdFrom = relType.RoleTypeIdFrom,
                  linkRoleTypeIdFrom   = userTypeId,
                  roleTypeIdTo         = relType.RoleTypeIdTo },
            transaction: tx, commandType: CommandType.StoredProcedure,
            cancellationToken: ct));
    }

    private async Task UpdatePersonaAsync(
        IDbConnection conn, IDbTransaction tx, Persona persona, long orgPartyId, CancellationToken ct)
    {
        await conn.ExecuteAsync(new CommandDefinition(
            StoredProcNameConstants.SP_UpdatePersona,
            new { persona.PersonaId, persona.Name, persona.FromDate, persona.ThruDate },
            transaction: tx, commandType: CommandType.StoredProcedure,
            cancellationToken: ct));
    }

    private async Task DeletePersonaAsync(
        IDbConnection conn, IDbTransaction tx, long personaId, CancellationToken ct)
    {
        await conn.ExecuteAsync(new CommandDefinition(
            StoredProcNameConstants.SP_UpdatePersona,
            new { personaId, ThruDate = DateTime.UtcNow },
            transaction: tx, commandType: CommandType.StoredProcedure,
            cancellationToken: ct));
    }

    private async Task SavePendingEmailNotificationAsync(
        IDbConnection conn, IDbTransaction tx, ProfileDetail profile, Guid personRealId,
        Guid orgRealPageId, IList<ContactMechanismUsageType> emailUsageTypes,
        long userEmailContactMechanismId, DateTime utcNow, CancellationToken ct)
    {
        // Resolve org contact mechanism id for the "from" address
        var orgMechanisms = await conn.QueryAsync<CommonAddress>(new CommandDefinition(
            StoredProcNameConstants.SP_ListContactMechanismsForPerson,
            new { RealPageId = orgRealPageId },
            transaction: tx, commandType: CommandType.StoredProcedure,
            cancellationToken: ct));

        var orgContactMechId = orgMechanisms.FirstOrDefault()?.PartyContactMechanismId ?? 0;
        if (orgContactMechId <= 0 || userEmailContactMechanismId <= 0) return;

        await conn.ExecuteAsync(new CommandDefinition(
            StoredProcNameConstants.SP_CreateCommunicationEvent,
            new { StatusTypeID              = (int)EmailStatusType.EmailPending,
                  FromPartyContactMechanismId = orgContactMechId,
                  ToPartyContactMechanismId   = userEmailContactMechanismId,
                  Started                    = utcNow,
                  Ended                      = utcNow,
                  Note                       = "pending",
                  CommunicationEventID       = (long?)null },
            transaction: tx, commandType: CommandType.StoredProcedure,
            cancellationToken: ct));
    }

    private async Task UpdateUserTypeForRpEmployeeAsync(
        IDbConnection conn, IDbTransaction tx,
        Guid personRealId, IList<UserOrganization> existingOrgs, CancellationToken ct)
    {
        foreach (var prevOrg in existingOrgs)
        {
            var relType = await conn.QuerySingleOrDefaultAsync<PartyRelationship>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_GetPartyRelationshipByRealPageId,
                    new { realPageIdFrom = personRealId,
                          realPageIdTo   = prevOrg.OrganizationRealPageId,
                          roleTypeName   = (string?)null,
                          relationshipTypeName = "User Type" },
                    transaction: tx, commandType: CommandType.StoredProcedure,
                    cancellationToken: ct));

            if (relType is null) continue;
            if (relType.RoleTypeIdFrom == (int)UserRoleType.ExternalUser) continue;

            await conn.ExecuteAsync(new CommandDefinition(
                StoredProcNameConstants.SP_UpdatePersonToOrganization,
                new { personRealId, prevOrg.OrganizationRealPageId,
                      unlinkRoleTypeIdFrom = relType.RoleTypeIdFrom,
                      linkRoleTypeIdFrom   = (int)UserRoleType.ExternalUser,
                      roleTypeIdTo         = relType.RoleTypeIdTo },
                transaction: tx, commandType: CommandType.StoredProcedure,
                cancellationToken: ct));
        }
    }

    // ── Pure-logic helpers (no DB) ────────────────────────────────────────────

    private static CreateUserResponse<IErrorData> Failure(
        CreateUserResponse<IErrorData> response, string code, string message)
    {
        response.Status = new Status<IErrorData> { Success = false, ErrorCode = code, ErrorMsg = message };
        response.UserStatus = message;
        return response;
    }

    private static bool IsDuplicateUserType(
        ProfileDetail profile, IList<UserOrganization> existingOrgs, UserOrganizationExists orgExists)
    {
        return profile.UserTypeId != (int)UserRoleType.ExternalUser &&
               existingOrgs.Any(x => x.PartyRoleTypeId != (int)UserRoleType.ExternalUser) &&
               profile.UserTypeId != (int)UserRoleType.RealPageEmployee &&
               existingOrgs.Any(x => x.PartyRoleTypeId != (int)UserRoleType.RealPageEmployee) &&
               (!orgExists.UserExistsAsAdminInOtherDomain || !orgExists.UserExistsAsRegularUserInOtherDomain) &&
               orgExists.UserExistsInThisOrganization;
    }

    // GAP-fix #7: add OrganizationDomain.Name == "Primary" check (sync L:443-452)
    private static bool ResolvePrimaryOrganizationFlag(
        UserOrganizationExists orgExists, ProfileDetail profile)
    {
        bool isCrossDomainCase =
            (orgExists.UserExistsAsAdminInOtherDomain || orgExists.UserExistsAsRegularUserInOtherDomain)
            && !orgExists.UserExistsInThisOrganization;

        if (!isCrossDomainCase) return true;

        // When user exists in another domain but not this org, only treat as primary
        // if the incoming org domain is explicitly named "Primary" (sync L:447-449)
        return profile.organization[0].OrganizationDomain?.Name.Equals("Primary") == true;
    }

    // GAP-fix #11: full two-branch port of sync DetermineUserStatus (UserRepository.cs L:950-1028)
    private static (int userStatus, DateTime? statusThruDate) DetermineUserStatus(
        OrganizationPrimary org, long externalOrgPartyId, long targetOrgPartyId,
        bool isLocalIdp, OrganizationStatus? currentPrimaryStatus,
        DateTime fromDate, DateTime? registrationThruDate, ProfileDetail profile,
        int existingOrgsCount)
    {
        bool futureFromDate = fromDate.Subtract(DateTime.UtcNow).TotalMinutes >= 15;

        // ── Branch 1: ExternalUsers company (sync L:950-985) ─────────────────
        if (org.OrganizationPartyId == externalOrgPartyId)
        {
            // Already pending in primary org → keep that status
            if (currentPrimaryStatus?.IsPending == true)
                return (currentPrimaryStatus.StatusTypeId, null);

            // User only existed in ExternalUsers before and is not yet active
            if (existingOrgsCount == 1 && currentPrimaryStatus?.IsActive != true)
            {
                if (futureFromDate)
                    return ((int)UserUiStatusType.Disabled, registrationThruDate);

                return isLocalIdp
                    ? ((int)UserUiStatusType.Pending, registrationThruDate)
                    : ((int)UserUiStatusType.Active, null);
            }

            return ((int)UserUiStatusType.Active, null);
        }

        // ── Branch 2: Current (target) company (sync L:988-1028) ─────────────
        if (futureFromDate)
            return ((int)UserUiStatusType.Disabled, registrationThruDate);

        if (!isLocalIdp)
            return ((int)UserUiStatusType.Active, null);

        if (currentPrimaryStatus is null)
        {
            return profile.userLogin.doNotForceChangePassword == true
                ? ((int)UserUiStatusType.Active, null)
                : ((int)UserUiStatusType.Pending, registrationThruDate);
        }

        if (profile.UserTypeId != (int)UserRoleType.ExternalUser && currentPrimaryStatus.IsPending == true)
            return (currentPrimaryStatus.StatusTypeId, currentPrimaryStatus.StatusThruDate);

        return ((int)UserUiStatusType.Active, null);
    }

    // GAP-fix #9: full 3-branch implementation (sync UserRepository.cs L:769-887)
    private async Task<List<OrganizationPrimary>> ResolveOrganizationListForExistingUserAsync(
        IDbConnection conn, IDbTransaction tx,
        ProfileDetail profile, IUserLoginOnly userLoginOnly,
        Organization externalOrg, IList<UserOrganization> existingOrgs,
        long orgPartyId, Guid orgRealPageId,
        OrganizationStatus? currentPrimaryStatus, bool primaryOrganization,
        DateTime fromDate, CancellationToken ct)
    {
        var orgList = new List<OrganizationPrimary>();

        // Branch 1 — adding as External User
        if (profile.UserTypeId == (int)UserRoleType.ExternalUser)
        {
            orgList.Add(new OrganizationPrimary
            {
                OrganizationRealPageId = orgRealPageId,
                OrganizationPartyId    = orgPartyId,
                PrimaryOrganization    = false,
                OrganizationFromDate   = fromDate,
                OrganizationThruDate   = profile.userLogin.ThruDate
            });

            // If user is in exactly one external-user company and not yet linked to ExternalUsers org,
            // add the ExternalUsers org as primary (mirrors sync L:791-813)
            bool onlyExternalOrg = existingOrgs.Count == 1 &&
                existingOrgs.Any(i => i.PartyRoleTypeId == (int)UserRoleType.ExternalUser);
            bool alreadyLinkedToExternal = existingOrgs.Any(
                m => m.OrganizationPartyId == externalOrg.PartyId);

            if (onlyExternalOrg && !alreadyLinkedToExternal)
            {
                DateTime newFromDate = fromDate;
                if (currentPrimaryStatus?.FromDate < fromDate)
                    newFromDate = currentPrimaryStatus.FromDate;

                orgList.Add(new OrganizationPrimary
                {
                    OrganizationRealPageId = externalOrg.RealPageId,
                    OrganizationPartyId    = externalOrg.PartyId,
                    PrimaryOrganization    = true,
                    OrganizationFromDate   = newFromDate,
                    OrganizationThruDate   = null
                });
            }
        }
        // Branch 2 — switching from ExternalUsers primary to a real company (sync L:817-848)
        else if (existingOrgs.Any(x => x.PrimaryOrganization &&
                                       x.OrganizationPartyId == externalOrg.PartyId))
        {
            // Unlink from ExternalUsers organisation
            var unlinkResult = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_UnlinkPersonToOrganization,
                    new { PersonRealPageId = userLoginOnly.RealPageId,
                          OrganizationRealPageId = externalOrg.RealPageId },
                    transaction: tx, commandType: CommandType.StoredProcedure,
                    cancellationToken: ct));

            if (unlinkResult is null)
                throw new InvalidOperationException(
                    "SP_UnlinkPersonToOrganization failed (User.CreateUser.6).");

            orgList.Add(new OrganizationPrimary
            {
                OrganizationRealPageId = orgRealPageId,
                OrganizationPartyId    = orgPartyId,
                PrimaryOrganization    = true,
                OrganizationFromDate   = fromDate,
                OrganizationThruDate   = profile.userLogin.ThruDate
            });
        }
        // Branch 3 — regular add to an additional organisation (sync L:850-863)
        else
        {
            orgList.Add(new OrganizationPrimary
            {
                OrganizationRealPageId = orgRealPageId,
                OrganizationPartyId    = orgPartyId,
                PrimaryOrganization    = primaryOrganization,
                OrganizationFromDate   = fromDate,
                OrganizationThruDate   = profile.userLogin.ThruDate
            });
        }

        // GAP-fix #2: if name fields changed for a non-external user, update person (sync L:865-886)
        bool profileChanged = false;
        if (profile.UserTypeId != (int)UserRoleType.ExternalUser)
        {
            var userDetails = await _userRepo.GetUserDetailsAsync(
                personaId: null, userRealPageId: userLoginOnly.RealPageId.ToString(), ct);
            profileChanged = IsUserProfileChanged(profile, userDetails);
        }

        if (profileChanged && profile.UserTypeId != (int)UserRoleType.ExternalUser)
        {
            var updateResult = await conn.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_UpdatePerson,
                    new { RealPageId = profile.RealPageId,
                          profile.FirstName, profile.MiddleName, profile.LastName },
                    transaction: tx, commandType: CommandType.StoredProcedure,
                    cancellationToken: ct));

            if (updateResult?.Id is null or 0)
                throw new InvalidOperationException(
                    "SP_UpdatePerson failed for existing user (User.CreateUser.26).");
        }

        return orgList;
    }

    // GAP-fix #3: reads 'UsePropertyInstanceUnifiedLogin' product setting (cached)
    private async Task<bool> GetPropertyInstanceUnifiedLoginAsync(CancellationToken ct)
    {
        const string cacheKey = "usePropertyInstanceUnifiedLogin";
        var value = await _cache.GetOrCreateAsync<string>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
            var settings = await _productSettingRepo.GetProductInternalSettingsAsync(
                (int)ProductEnum.UnifiedPlatform, ct);
            return settings.FirstOrDefault(s =>
                s.Name.Equals("UsePropertyInstanceUnifiedLogin", StringComparison.OrdinalIgnoreCase))?.Value;
        });
        return value == "1";
    }

    // GAP-fix #6: get AO products available to the editor user for a given company
    private async Task<IList<string>?> GetEditorUserAoProductAsync(
        Guid editorRealPageGuid, long editorPersonaId, Guid orgRealPageId, CancellationToken ct)
    {
        using var conn = _connectionFactory.GetReadOnlyConnection();
        conn.Open();

        var companyProducts = (await conn.QueryAsync<ProductUI>(new CommandDefinition(
            StoredProcNameConstants.SP_ListProductsByOrganization,
            new { OrganizationRealPageId = orgRealPageId },
            commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();

        if (!companyProducts.Any(p => p.ProductId == (int)ProductEnum.AssetOptimizer))
            return null;

        // GAP-fix #6: delegate to async AO service (sync L:5051-5054 in UserRepository)
        var aoProducts = await _manageAo.GetGbSupportedAoEditorUserProductsToAssignAsync(
            editorPersonaId, ct);

        return aoProducts.Count > 0 ? aoProducts : null;
    }

    // GAP-fix #5: clone-user product-batch preparation (sync UserRepository.cs L:307-423)
    private async Task PrepareCloneUserProductBatchAsync(
        ProfileDetail profile, ProductBatch? primaryPropertiesBatch,
        bool usePropertyInstanceUnifiedLogin, CancellationToken ct)
    {
        long clonePersonaId = profile.Persona[0].PersonaId;

        var clonePersona = await _managePersona.GetPersonaAsync(clonePersonaId, withRights: false, ct);
        var orgList = await _userLoginRepo.ListOrganizationByEnterpriseUserIdAsync(
            clonePersona.RealPageId, string.Empty);
        var personaOrg = orgList.FirstOrDefault(o => o.PartyId == clonePersona.OrganizationPartyId);

        bool isExternalUser = personaOrg is not null &&
            personaOrg.RelationshipType?.Equals("User Type", StringComparison.OrdinalIgnoreCase) == true &&
            personaOrg.RoleNameFrom?.Equals("External User", StringComparison.OrdinalIgnoreCase) == true;

        using var conn = _connectionFactory.GetReadOnlyConnection();
        conn.Open();

        // Get products currently on clone persona
        var userProducts = (await conn.QueryAsync<PersonaProductUserDetails>(new CommandDefinition(
            StoredProcNameConstants.SP_ListProductsByPersonaId,
            new { PersonaId = clonePersonaId,
                  ProductStatusValue = ((int)ProductBatchStatusType.Success).ToString() },
            commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();

        // Remove AdminSupport product if no SAML attributes (sync L:320-328)
        var adminSupportProduct = userProducts.FirstOrDefault(m => m.ProductId == 89 || m.ProductId == 104);
        if (adminSupportProduct is not null)
        {
            var samlAttrs = (await conn.QueryAsync<SamlAttributes>(new CommandDefinition(
                StoredProcNameConstants.SP_GetProductSamlDetails,
                new { PersonaId = clonePersonaId, ProductId = adminSupportProduct.ProductId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();

            if (samlAttrs.Count == 0)
                userProducts.RemoveAll(a => a.ProductId == adminSupportProduct.ProductId);
        }

        if (userProducts.Count > 0)
        {
            long editorPersonaId = _userClaims.PersonaId;

            // Remove products already in the new profile batch
            foreach (var existing in profile.productBatch ?? [])
                userProducts.RemoveAll(a => a.ProductId == existing.ProductId);

            var upfmProperty = new UPFMProperty();
            if (primaryPropertiesBatch?.InputJson?.PropertyList is not null)
                upfmProperty.id = primaryPropertiesBatch.InputJson.PropertyList.ToList();

            var personaProductSettings = await _personaRepo.GetPersonaProductSettingsAsync(
                clonePersonaId, ct);

            // ManageCloneProductBatch is sync — run on thread pool (CPU-bound list manipulation)
            // GetUserClaim() provides backward-compat DefaultUserClaim from IUserClaimsAccessor
            var manageClone = new ManageCloneProductBatch(_userClaims.GetUserClaim());
            var pbData = await Task.Run(() =>
                manageClone.GetUserProductBatchData(
                    clonePersonaId, userProducts, editorPersonaId,
                    upfmProperty, personaProductSettings, isExternalUser), ct);

            foreach (var pb in pbData)
                profile.productBatch!.Add(pb);

            // Remove deselected products (sync L:363-367)
            var deselected = profile.productBatch!.Where(x => x.InputJson?.IsAssigned == false).ToList();
            foreach (var pb in deselected)
                profile.productBatch.Remove(pb);
        }

        // Build UnifiedPlatform product batch from clone user's roles + properties (sync L:370-422)
        var ulRole = (await conn.QueryAsync<dynamic>(new CommandDefinition(
            StoredProcNameConstants.SP_ListRolesForProductsByPersonaId,
            new { ProductId = (int)ProductEnum.UnifiedPlatform, PersonaId = clonePersonaId },
            commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();

        List<string> roleList = ulRole.Select(r => (string)(Convert.ToString((object)r.RoleId) ?? string.Empty)).ToList();
        var propertyList = new List<string>();

        if (!usePropertyInstanceUnifiedLogin)
        {
            var ulProperties = (await conn.QueryAsync<dynamic>(new CommandDefinition(
                StoredProcNameConstants.SP_ListPropertyMapping,
                new { PersonaId = clonePersonaId, ProductId = (int)ProductEnum.UnifiedPlatform },
                commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();
            propertyList = ulProperties.Select(p => (string)(Convert.ToString((object)p.PropertyID) ?? string.Empty)).ToList();
        }
        else
        {
            var ulInstances = (await conn.QueryAsync<dynamic>(new CommandDefinition(
                StoredProcNameConstants.SP_GetPropertyInstanceByPersonaId,
                new { PersonaId = clonePersonaId, ProductId = (int)ProductEnum.UnifiedPlatform },
                commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();
            propertyList = ulInstances.Select(p => (string)(Convert.ToString((object)p.PropertyInstanceID) ?? string.Empty)).ToList();
        }

        if (roleList.Count > 0 || propertyList.Count > 0)
        {
            profile.productBatch!.Add(new ProductBatch
            {
                ProductId    = (int)ProductEnum.UnifiedPlatform,
                StatusTypeId = 5,
                RetryCount   = 0,
                InputJson    = new RolePropertyList
                {
                    PropertyList = propertyList,
                    RoleList     = roleList
                }
            });
        }
    }

    // GAP-fix: async port of UserRepository.IsLoginNameExistsAsAdminInOtherDomain (sync L:7936)
    private async Task<UserOrganizationExists> IsLoginNameExistsAsAdminInOtherDomainAsync(
        string loginName, Guid organizationRealPageId, long booksMasterId, CancellationToken ct)
    {
        var result = new UserOrganizationExists();
        var userOrgs = await _userLoginRepo.ListOrganizationByLoginNameAsync(loginName);

        result.UserExists = userOrgs?.Count > 0;
        result.UserExistsInThisOrganization = userOrgs?.Any(a => a.OrganizationRealPageId == organizationRealPageId) == true;
        result.UserExistsAsNoEmail = userOrgs?.Any(p => p.PartyRoleTypeId == (int)UserRoleType.UserNoEmail) == true;

        if (result.UserExists && !result.UserExistsInThisOrganization)
        {
            var primaryOrg = userOrgs!.FirstOrDefault(m => m.PrimaryOrganization);
            bool isAdmin   = primaryOrg?.PartyRoleTypeId == (int)UserRoleType.SuperUser;
            bool isUser    = primaryOrg?.PartyRoleTypeId == (int)UserRoleType.User;

            if (primaryOrg is not null && (isAdmin || isUser) && primaryOrg.BooksCustomerMasterId == booksMasterId)
            {
                // Check if the company has multiple domains (same BooksCustomerMasterId)
                var orgDomains = await _orgRepo.GetOrganizationListByBooksCustomerMasterIdAsync(booksMasterId);
                if (orgDomains?.Count > 1)
                {
                    result.UserExists = false;
                    result.UserExistsAsAdminInOtherDomain = isAdmin;
                    result.UserExistsAsRegularUserInOtherDomain = isUser;
                }
            }
        }

        return result;
    }

    // GAP-fix #2: pure-logic name-change detection (mirrors sync UserRepository.IsUserProfileChanged)
    private static bool IsUserProfileChanged(IProfileDetail profile, UserDetails? userDetails)
    {
        if (userDetails is null) return false;
        return !string.Equals(profile.FirstName,  userDetails.FirstName,  StringComparison.OrdinalIgnoreCase)
            || !string.Equals(profile.MiddleName, userDetails.MiddleName, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(profile.LastName,   userDetails.LastName,   StringComparison.OrdinalIgnoreCase);
    }
}
