//using Dapper;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using Serilog;
//using Serilog.Events;
//using System.Data;
//using System.Dynamic;
//using System.Text;
//using UnifiedLogin.BusinessLogic.Base;
//using UnifiedLogin.BusinessLogic.CacheHelper;
//using UnifiedLogin.BusinessLogic.Logic;
//using UnifiedLogin.BusinessLogic.Logic.Helper;
//using UnifiedLogin.BusinessLogic.Logic.Interfaces;
//using UnifiedLogin.BusinessLogic.Logic.Product;
//using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
//using UnifiedLogin.BusinessLogic.Repository.Interfaces;
//using UnifiedLogin.BusinessLogic.Services.Interfaces;
//using UnifiedLogin.BusinessLogic.ThirdParty;
//using UnifiedLogin.DataAccess.Helper;
//using UnifiedLogin.SharedObjects;
//using UnifiedLogin.SharedObjects.Audit.Common;
//using UnifiedLogin.SharedObjects.Batch;
//using UnifiedLogin.SharedObjects.BlackBook;
//using UnifiedLogin.SharedObjects.Cache;
//using UnifiedLogin.SharedObjects.Constants;
//using UnifiedLogin.SharedObjects.Enterprise;
//using UnifiedLogin.SharedObjects.EnterpriseRole;
//using UnifiedLogin.SharedObjects.Enum;
//using UnifiedLogin.SharedObjects.Extensions;
//using UnifiedLogin.SharedObjects.Helper;
//using UnifiedLogin.SharedObjects.IdentityConfig;
//using UnifiedLogin.SharedObjects.Landing;
//using UnifiedLogin.SharedObjects.Landing.Enum;
//using UnifiedLogin.SharedObjects.Landing.UserUpdate;
//using UnifiedLogin.SharedObjects.Mappers;
//using UnifiedLogin.SharedObjects.Product;
//using UnifiedLogin.SharedObjects.Saml;
//using static UnifiedLogin.BusinessLogic.Repository.UserRepository;

//namespace UnifiedLogin.BusinessLogic.Services;

///// <summary>
///// Orchestration service that owns all multi-SP / transactional / external-service
///// logic previously embedded inside UserRepository.
///// </summary>
//public sealed class UserService : IUserService
//{
//    #region Fields

//    private readonly IDbConnection _db;
//    private readonly IUserRepositoryAsync _userRepo;
//    private readonly IUserLoginRepositoryAsync _userLoginRepo;
//    private readonly IPersonaRepositoryAsync _personaRepo;
//    private readonly IOrganizationRepositoryAsync _orgRepo;
//    private readonly IContactMechanismUsageTypeRepositoryAsync _usageTypeRepo;
//    private readonly IProductRepositoryAsync _productRepo;
//    private readonly IProductInternalSettingRepositoryAsync _internalSettingRepo;
//    private readonly IPropertyRepositoryAsync _propertyRepo;
//    private readonly ICredentialRepositoryAsync _credentialRepo;
//    private readonly IUserRoleRightRepositoryAsync _userRoleRightRepo;
//    private readonly ISamlRepositoryAsync _samlRepo;
//    private readonly IManageProductAssetOptimizationFactory _aoFactory;
//    private readonly IManagePersona _managePersona;
//    private readonly ICacheService _cache;
//    private readonly IUserClaimsAccessor _userClaimAccessor;
//    private readonly ILogger<UserService> _logger;

//    private static readonly CacheEntryOptions PropertyInstanceCacheOptions = new() { ExpirationTimeInMinutes = 1 };
//    private static readonly CacheEntryOptions ProductSettingTypeCacheOptions = new() { ExpirationTimeInMinutes = 2 };

//    private const string ProfileErrorMessage = "Update profile Error: Create Contact Mechanism failed.";
//    private const string ProfileLinkUsageTypeErrorMessage = "Update profile Error: Link UsageType to Party Contact Mechanism failed.";

//    // Lazy factory methods — replace with injected interface when ManageBlueBook / ManageUnifiedSettings expose one.
//    private ManageBlueBook CreateBlueBook() => new(_userClaimAccessor.Current);
//    private ManageUnifiedSettings CreateUnifiedSettings() => new(_userClaimAccessor.Current);

//    #endregion

//    #region Constructor

//    public UserService(
//        IDbConnection db,
//        IUserRepositoryAsync userRepo,
//        IUserLoginRepositoryAsync userLoginRepo,
//        IPersonaRepositoryAsync personaRepo,
//        IOrganizationRepositoryAsync orgRepo,
//        IContactMechanismUsageTypeRepositoryAsync usageTypeRepo,
//        IProductRepositoryAsync productRepo,
//        IProductInternalSettingRepositoryAsync internalSettingRepo,
//        IPropertyRepositoryAsync propertyRepo,
//        ICredentialRepositoryAsync credentialRepo,
//        IUserRoleRightRepositoryAsync userRoleRightRepo,
//        ISamlRepositoryAsync samlRepo,
//        IManageProductAssetOptimizationFactory aoFactory,
//        IManagePersona managePersona,
//        ICacheService cache,
//        IUserClaimsAccessor userClaimAccessor,
//        ILogger<UserService> logger)
//    {
//        _db                  = db                  ?? throw new ArgumentNullException(nameof(db));
//        _userRepo            = userRepo            ?? throw new ArgumentNullException(nameof(userRepo));
//        _userLoginRepo       = userLoginRepo       ?? throw new ArgumentNullException(nameof(userLoginRepo));
//        _personaRepo         = personaRepo         ?? throw new ArgumentNullException(nameof(personaRepo));
//        _orgRepo             = orgRepo             ?? throw new ArgumentNullException(nameof(orgRepo));
//        _usageTypeRepo       = usageTypeRepo       ?? throw new ArgumentNullException(nameof(usageTypeRepo));
//        _productRepo         = productRepo         ?? throw new ArgumentNullException(nameof(productRepo));
//        _internalSettingRepo = internalSettingRepo ?? throw new ArgumentNullException(nameof(internalSettingRepo));
//        _propertyRepo        = propertyRepo        ?? throw new ArgumentNullException(nameof(propertyRepo));
//        _credentialRepo      = credentialRepo      ?? throw new ArgumentNullException(nameof(credentialRepo));
//        _userRoleRightRepo   = userRoleRightRepo   ?? throw new ArgumentNullException(nameof(userRoleRightRepo));
//        _samlRepo            = samlRepo            ?? throw new ArgumentNullException(nameof(samlRepo));
//        _aoFactory           = aoFactory           ?? throw new ArgumentNullException(nameof(aoFactory));
//        _managePersona       = managePersona       ?? throw new ArgumentNullException(nameof(managePersona));
//        _cache               = cache               ?? throw new ArgumentNullException(nameof(cache));
//        _userClaimAccessor   = userClaimAccessor   ?? throw new ArgumentNullException(nameof(userClaimAccessor));
//        _logger              = logger              ?? throw new ArgumentNullException(nameof(logger));
//    }

//    #endregion

//    // ════════════════════════════════════════════════════════════════════════
//    // IUserService — simple orchestration
//    // ════════════════════════════════════════════════════════════════════════

//    #region GetUnifiedSettingDataAsync

//    /// <inheritdoc/>
//    public Task<bool> GetUnifiedSettingDataAsync(string settingName, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            var data = CreateUnifiedSettings().GetCompanyInternalSettings(
//                _userClaimAccessor.Current.OrganizationRealPageGuid, "UPFM", "company");
//            return Task.FromResult(data?.Keys?.FirstOrDefault(p => p.Name == settingName)?.Value == "1");
//        }
//        catch
//        {
//            return Task.FromResult(false);
//        }
//    }

//    #endregion

//    #region ThirdPartyIdpBulkUpdateAsync

//    /// <inheritdoc/>
//    public async Task<RepositoryResponse> ThirdPartyIdpBulkUpdateAsync(
//        IList<long> userIds, bool isEnabled,
//        CancellationToken cancellationToken = default)
//    {
//        var userClaims = _userClaimAccessor.Current;
//        try
//        {
//            if (userIds.Count == 0) return new RepositoryResponse();

//            var updatedIds = await _userRepo.ThirdPartyIdpBulkUpdateAsync(
//                userIds, isEnabled, userClaims.OrganizationPartyId, cancellationToken);

//            if (updatedIds.Count > 0)
//                await AuditBulkIdpUpdateAsync(updatedIds, isEnabled, userClaims, cancellationToken);

//            return new RepositoryResponse();
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "{Method} failed", nameof(ThirdPartyIdpBulkUpdateAsync));
//            return new RepositoryResponse { ErrorMessage = "Unable to perform bulk Third-Party Identity Provider update." };
//        }
//    }

//    #endregion

//    #region UpdateUserStatusByCompanyAsync

//    /// <inheritdoc/>
//    public async Task<RepositoryResponse> UpdateUserStatusByCompanyAsync(
//        Guid realPageId, long organizationPartyId,
//        int statusTypeId, DateTime fromDate, DateTime? thruDate,
//        CancellationToken cancellationToken = default)
//    {
//        var userClaims = _userClaimAccessor.Current;

//        IUserLoginOnly impersonatorLogin = new UserLoginOnly();
//        if (userClaims.ImpersonatedBy != Guid.Empty)
//            impersonatorLogin = await _userLoginRepo.GetUserLoginOnlyAsync(userClaims.ImpersonatedBy);

//        // Simple status update (no orchestration)
//        var response = await _userRepo.UpdateUserStatusByCompanyAsync(
//            realPageId, organizationPartyId, statusTypeId, fromDate, thruDate, cancellationToken);

//        // Orchestration: disable all products for non-primary personas when user is disabled
//        if (statusTypeId == (int)UserUiStatusType.Disabled)
//        {
//            var personas = await _db.QueryAsync<dynamic>(
//                new CommandDefinition(
//                    StoredProcNameConstants.SP_ListPersonaToDisableUserProduct,
//                    new { OrganizationPartyId = organizationPartyId, PersonRealPageId = realPageId },
//                    commandType: CommandType.StoredProcedure,
//                    cancellationToken: cancellationToken));

//            foreach (var item in personas.Where(p => !(bool)p.PrimaryOrganization))
//                await ProcessDisableUserProductDataAsync(
//                    (long)item.PersonaId, (Guid)item.EditorRealPageId,
//                    (long)item.EditorPersonaId, (int?)item.UserTypeId,
//                    impersonatorLogin.UserId, cancellationToken);
//        }

//        return response;
//    }

//    #endregion

//    #region ProcessDisableUserProductDataAsync

//    /// <inheritdoc/>
//    public async Task ProcessDisableUserProductDataAsync(
//        long assignUserPersonaId, Guid createUserRealPageId,
//        long createUserPersonaId, int? userTypeId, long impersonatorUserId,
//        CancellationToken cancellationToken = default)
//    {
//        var userClaims = _userClaimAccessor.Current;
//        var createUserResponse = new CreateUserResponse<IErrorData>();
//        var errorStatus = new Status<IErrorData>();

//        // Cached product setting types (replaces RPObjectCache in original)
//        var productSettingTypes = await _cache.GetOrSetAsync(
//            "listProductSettingType",
//            async ct => (IList<ProductSettingType>)
//                (await _productRepo.ListProductSettingTypeAsync()).ToList(),
//            ProductSettingTypeCacheOptions,
//            cancellationToken) ?? [];

//        var productsToDisable = await GetListOfProductsToRemoveByPersonaIdAsync(
//            assignUserPersonaId, cancellationToken);

//        if (userTypeId != (int)UserRoleType.UserNoEmail)
//            productsToDisable.Add(new ProductBatch
//            {
//                ProductId    = (int)ProductEnum.SalesForce,
//                StatusTypeId = 5, RetryCount = 0,
//                InputJson    = new RolePropertyList { PropertyList = [], RoleList = [], IsAssigned = false }
//            });

//        if (createUserPersonaId <= 0 || assignUserPersonaId <= 0 || productsToDisable.Count == 0)
//            return;

//        string aoInputJson = string.Empty;
//        if (productsToDisable.Any(y => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)y.ProductId)))
//            aoInputJson = BundleAoProducts(productsToDisable);

//        foreach (var product in productsToDisable)
//        {
//            if (product.ProductId == (int)ProductEnum.UnifiedPlatform) continue;

//            string inputJson = product.ProductId == (int)ProductEnum.AssetOptimizer
//                ? aoInputJson
//                : JsonConvert.SerializeObject(product.InputJson);

//            await SaveProductBatchNoTxAsync(
//                product, createUserRealPageId, createUserPersonaId, assignUserPersonaId,
//                inputJson, impersonatorUserId, cancellationToken: cancellationToken);
//        }
//    }

//    #endregion

//    #region DisableUserProductAsync

//    /// <inheritdoc/>
//    public async Task DisableUserProductAsync(
//        Guid createUserRealPageId, long createUserPersonaId,
//        IList<UserLoginOnly> userLogins,
//        CancellationToken cancellationToken = default)
//    {
//        var userClaims = _userClaimAccessor.Current;
//        IUserLoginOnly impersonatorLogin = new UserLoginOnly();
//        if (userClaims.ImpersonatedBy != Guid.Empty)
//            impersonatorLogin = await _userLoginRepo.GetUserLoginOnlyAsync(userClaims.ImpersonatedBy);

//        await Parallel.ForEachAsync(
//            userLogins,
//            new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = cancellationToken },
//            async (userLoginOnly, ct) =>
//            {
//                try
//                {
//                    var login          = await _userLoginRepo.GetUserLoginOnlyAsync(userLoginOnly.RealPageId);
//                    var orgList        = await _userLoginRepo.ListOrganizationByLoginNameAsync(login.LoginName);
//                    var primaryStatus  = await _userLoginRepo.GetUserOrganizationWithStatusAsync(login.UserId, login.LastLogin, 0, true);

//                    if (orgList == null || primaryStatus == null) return;

//                    if (userClaims.OrganizationPartyId == primaryStatus.PartyId && orgList.Count > 1)
//                    {
//                        foreach (var org in orgList)
//                        {
//                            var orgStatus = await _userLoginRepo.GetUserOrganizationWithStatusAsync(
//                                login.UserId, login.LastLogin, org.OrganizationPartyId, false);

//                            if (orgStatus.Status != UserUiStatusType.Disabled) continue;

//                            var persona = _managePersona.GetFirstAvailablePersonaByCompany(login.RealPageId, org.OrganizationPartyId);
//                            var adminRealPageId = await _orgRepo.GetOrganizationAdminUserRealPageIdAsync(org.OrganizationRealPageId);
//                            var adminPersona    = _managePersona.GetFirstAvailablePersonaByCompany(adminRealPageId, org.OrganizationPartyId);

//                            Guid editorRealPageId  = userClaims.OrganizationPartyId == adminPersona.OrganizationPartyId
//                                ? userClaims.UserRealPageGuid : adminRealPageId;
//                            long editorPersonaId   = userClaims.OrganizationPartyId == adminPersona.OrganizationPartyId
//                                ? userClaims.PersonaId : adminPersona.PersonaId;

//                            await ProcessDisableUserProductDataAsync(
//                                persona.PersonaId, editorRealPageId, editorPersonaId,
//                                persona.UserTypeId, impersonatorLogin.UserId, ct);
//                        }
//                    }
//                    else
//                    {
//                        var orgStatus = await _userLoginRepo.GetUserOrganizationWithStatusAsync(
//                            login.UserId, login.LastLogin, userClaims.OrganizationPartyId, false);

//                        if (orgStatus.Status != UserUiStatusType.Disabled) return;

//                        var persona = _managePersona.GetFirstAvailablePersonaByCompany(
//                            login.RealPageId, userClaims.OrganizationPartyId);

//                        await ProcessDisableUserProductDataAsync(
//                            persona.PersonaId, createUserRealPageId, createUserPersonaId,
//                            persona.UserTypeId, impersonatorLogin.UserId, ct);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "{Method} failed for user {RealPageId}",
//                        nameof(DisableUserProductAsync), userLoginOnly.RealPageId);
//                }
//            });
//    }

//    #endregion

//    #region ActivateUserProductsAsync

//    /// <inheritdoc/>
//    public async Task ActivateUserProductsAsync(
//        Guid createUserRealPageId, long createUserPersonaId,
//        IList<UserLoginOnly> userLogins,
//        CancellationToken cancellationToken = default)
//    {
//        var userClaims = _userClaimAccessor.Current;
//        foreach (var ul in userLogins)
//        {
//            var persona = _managePersona.GetFirstAvailablePersonaByCompany(
//                ul.RealPageId, userClaims.OrganizationPartyId);
//            await ProcessActivatedUserProductBatchDataAsync(
//                persona.PersonaId, createUserRealPageId, createUserPersonaId, cancellationToken);
//        }
//    }

//    #endregion

//    #region ActivateSalesForceUserAsync

//    /// <inheritdoc/>
//    public async Task ActivateSalesForceUserAsync(
//        Guid createUserRealPageId, long createUserPersonaId,
//        IList<UserLoginOnly> userLogins, bool isAssigned,
//        CancellationToken cancellationToken = default)
//    {
//        var userClaims = _userClaimAccessor.Current;
//        IUserLoginOnly impersonatorLogin = new UserLoginOnly();
//        if (userClaims.ImpersonatedBy != Guid.Empty)
//            impersonatorLogin = await _userLoginRepo.GetUserLoginOnlyAsync(userClaims.ImpersonatedBy);

//        foreach (var ul in userLogins)
//        {
//            var editorPersona  = await _personaRepo.GetPersonaAsync(createUserPersonaId, false, cancellationToken);
//            var personaList    = await _personaRepo.ListPersonaAsync(ul.RealPageId, cancellationToken);
//            var persona        = personaList.FirstOrDefault(p => p.OrganizationPartyId == editorPersona.OrganizationPartyId);

//            if (persona is null || persona.UserTypeId == (int)UserRoleType.UserNoEmail) continue;

//            var pb = new ProductBatch
//            {
//                ProductId    = (int)ProductEnum.SalesForce,
//                StatusTypeId = 5, RetryCount = 0,
//                InputJson    = new RolePropertyList { PropertyList = [], RoleList = [], IsAssigned = isAssigned }
//            };

//            await SaveProductBatchNoTxAsync(
//                pb, createUserRealPageId, createUserPersonaId, persona.PersonaId,
//                JsonConvert.SerializeObject(pb.InputJson), impersonatorLogin.UserId,
//                cancellationToken: cancellationToken);
//        }
//    }

//    #endregion

//    #region AssignProductsToAdministratorsAsync

//    /// <inheritdoc/>
//    public async Task AssignProductsToAdministratorsAsync(
//        Guid organizationRealPageId, long assignUserPersonaId = 0,
//        CancellationToken cancellationToken = default)
//    {
//        if (organizationRealPageId == Guid.Empty)
//            throw new ArgumentException("Invalid organization realPageId.", nameof(organizationRealPageId));

//        var userClaims = _userClaimAccessor.Current;
//        var aoProducts = await GetEditorUserAoProductAsync(
//            userClaims.UserRealPageGuid, userClaims.PersonaId, organizationRealPageId, cancellationToken);

//        IUserLoginOnly impersonatorLogin = new UserLoginOnly();
//        if (userClaims.ImpersonatedBy != Guid.Empty)
//            impersonatorLogin = await _userLoginRepo.GetUserLoginOnlyAsync(userClaims.ImpersonatedBy);

//        var org        = await _orgRepo.GetOrganizationAsync(organizationRealPageId);
//        var personaList = await _personaRepo.ListPersonaByOrganizationPartyIdAsync(
//            org.PartyId, null, (int)UserRoleType.SuperUser, cancellationToken);

//        if (assignUserPersonaId > 0)
//            personaList = personaList.Where(p => p.PersonaId == assignUserPersonaId).ToList();

//        var orgResult = await _db.QuerySingleOrDefaultAsync<dynamic>(
//            new CommandDefinition(
//                StoredProcNameConstants.SP_ListOrganizations,
//                new { RealPageId = organizationRealPageId },
//                commandType: CommandType.StoredProcedure,
//                cancellationToken: cancellationToken));

//        if (orgResult is null) return;

//        Guid editorAccessId = new Guid(orgResult.PersonRealPageId.ToString());
//        long createPersonaId = await _personaRepo.GetActivePersonaIdAsync(editorAccessId, cancellationToken);

//        int totalProductCount = 0, adminsUpdated = 0;
//        var errorStatus = new Status<IErrorData>();

//        foreach (var persona in personaList)
//        {
//            var ulpList = (await _db.QueryAsync<UserLoginPersona>(
//                new CommandDefinition(
//                    StoredProcNameConstants.SP_GetUserLoginPersona,
//                    new { UserLoginId = persona.UserId, OrganizationPartyId = org.PartyId },
//                    commandType: CommandType.StoredProcedure,
//                    cancellationToken: cancellationToken))).ToList();

//            if (ulpList.Count == 0) continue;
//            if (ulpList[0].StatusTypeId is 23 or 24) continue; // disabled/expired admins

//            var count = await SaveProductDetailsAsync(
//                null, [], null, createPersonaId, persona.PersonaId,
//                editorAccessId, organizationRealPageId, errorStatus,
//                (int)UserRoleType.SuperUser, true, impersonatorLogin.UserId,
//                aoProducts, false, false, 0, "update", false, null, cancellationToken);

//            adminsUpdated++;
//            totalProductCount = Math.Max(totalProductCount, count);
//        }

//        if (adminsUpdated > 0 && totalProductCount > 0)
//        {
//            string logMessage = $"{{0}} {{1}} performed Refresh Admin Users in {orgResult.Name} for {adminsUpdated} "
//                + (adminsUpdated > 1 ? "administrators" : "administrator")
//                + $"; at most {totalProductCount} new product user"
//                + (totalProductCount > 1 ? "s were created" : " was created.");

//            LogActivity.WriteActivity(new ActivityDetails
//            {
//                LogActivityTypeName      = LogActivityTypeConstants.PRODUCT_ACCESS,
//                LogCategoryName          = LogActivityCategoryType.ProductAccess.ToString(),
//                CorrelationId            = userClaims.CorrelationId.ToString(),
//                BooksMasterOrganizationId = userClaims.OrganizationMasterId,
//                OrganizationPartyId      = userClaims.OrganizationPartyId,
//                Message                  = string.Format(logMessage, userClaims.FirstName, userClaims.LastName),
//                FromUserLoginName        = userClaims.LoginName,
//                FromUserLoginId          = userClaims.UserId,
//                FromUserFirstName        = userClaims.FirstName,
//                FromUserLastName         = userClaims.LastName,
//                FromUserRealpageId       = userClaims.UserRealPageGuid.ToString()
//            });
//        }
//    }

//    #endregion

//    #region ProcessDisabledUsersAsync

//    /// <inheritdoc/>
//    public async Task ProcessDisabledUsersAsync(
//        IList<ProcessUserLogin> userLogins,
//        CancellationToken cancellationToken = default)
//    {
//        var userClaims = _userClaimAccessor.Current;
//        var managePerson   = new ManagePerson();
//        var profileLogic   = new ManageProfile(userClaims);
//        var companyAdmins  = new Dictionary<Guid, Persona>();

//        IUserLoginOnly impersonatorLogin = new UserLoginOnly();
//        if (userClaims.ImpersonatedBy != Guid.Empty)
//            impersonatorLogin = await _userLoginRepo.GetUserLoginOnlyAsync(userClaims.ImpersonatedBy);

//        foreach (var ul in userLogins)
//        {
//            var login    = await _userLoginRepo.GetUserLoginOnlyAsync(ul.UserRealPageId);
//            var org      = await _orgRepo.GetOrganizationAsync(ul.OrganizationRealPageId);
//            var person   = managePerson.GetPerson(ul.UserRealPageId);
//            var orgList  = await _userLoginRepo.ListAllOrganizationByLoginNameAsync(login.LoginName);
//            Guid primaryGuid = orgList.FirstOrDefault(p => p.PrimaryOrganization)?.OrganizationRealPageId ?? Guid.Empty;

//            var currentClaim = GetCurrentUserClaim(profileLogic, org);

//            foreach (var userOrg in orgList)
//            {
//                Persona editorPersona = null;
//                long orgPartyId = orgList.First(o => o.OrganizationRealPageId == ul.OrganizationRealPageId).OrganizationPartyId;
//                var userLogin = await _userLoginRepo.GetUserLoginAsync(ul.UserRealPageId, orgPartyId);
//                bool isDisabled = userLogin.StatusId == (int)UserUiStatusType.Disabled;

//                if (!companyAdmins.ContainsKey(userOrg.OrganizationRealPageId))
//                {
//                    var result = (await _db.QueryAsync<dynamic>(
//                        new CommandDefinition(
//                            StoredProcNameConstants.SP_ListOrganizations,
//                            new { RealPageId = userOrg.OrganizationRealPageId },
//                            commandType: CommandType.StoredProcedure,
//                            cancellationToken: cancellationToken))).FirstOrDefault();

//                    if (result is not null)
//                    {
//                        Guid adminId = new Guid(Convert.ToString(result.PersonRealPageId));
//                        editorPersona = _managePersona.GetFirstAvailablePersonaByCompany(adminId, (long)result.PartyId);
//                        companyAdmins[userOrg.OrganizationRealPageId] = editorPersona;
//                    }
//                }
//                else
//                {
//                    editorPersona = companyAdmins[userOrg.OrganizationRealPageId];
//                }

//                var persona = _managePersona.GetFirstAvailablePersonaByCompany(ul.UserRealPageId, userOrg.OrganizationPartyId);

//                if (!isDisabled && (ul.OrganizationRealPageId == primaryGuid || ul.OrganizationRealPageId == userOrg.OrganizationRealPageId))
//                {
//                    await _db.ExecuteAsync(new CommandDefinition(
//                        StoredProcNameConstants.SP_UpdateUserStatusByCompany,
//                        new { RealPageId = ul.UserRealPageId, OrganizationPartyId = userOrg.OrganizationPartyId, StatusTypeId = UserUiStatusType.Disabled, FromDate = ul.FromDate },
//                        commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));

//                    DateTime? thruDateCst = userLogin.ThruDate is not null
//                        ? TimeZoneInfo.ConvertTime(userLogin.ThruDate.Value, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"))
//                        : null;

//                    string msg = $"{person.FirstName} {person.LastName} was deactivated by the system due to the scheduled User Expires date. | "
//                        + (thruDateCst.HasValue ? thruDateCst.Value.ToShortDateString() + "/ " + thruDateCst.Value.ToShortTimeString() : string.Empty) + " CST";
//                    AddActivityLog(LogActivityTypeConstants.UPDATE_USER, LogActivityCategoryType.User, msg, person, login, userOrg, currentClaim);
//                }

//                if (editorPersona is not null && (ul.OrganizationRealPageId == primaryGuid || ul.OrganizationRealPageId == userOrg.OrganizationRealPageId))
//                {
//                    await ProcessDisableUserProductDataAsync(
//                        persona.PersonaId, editorPersona.RealPageId, editorPersona.PersonaId,
//                        persona.UserTypeId, impersonatorLogin.UserId, cancellationToken);
//                }
//            }
//        }
//    }

//    #endregion

//    // ════════════════════════════════════════════════════════════════════════
//    // IUserService — large transactional orchestration
//    // ════════════════════════════════════════════════════════════════════════

//    #region CreateUserAsync

//    /// <inheritdoc/>
//    public async Task<CreateUserResponse<IErrorData>> CreateUserAsync(
//        ProfileDetail newProfile, IList<Persona> persona,
//        CancellationToken cancellationToken = default)
//    {
//        var userClaims = _userClaimAccessor.Current;
//        var response   = new CreateUserResponse<IErrorData>();
//        var errorStatus = new Status<IErrorData>();
//        string processTracker = "";

//        DateTime utcNow = DateTime.UtcNow, utcMax = DateTime.MaxValue.ToUniversalTime();
//        DateTime? fromDate = utcNow, thruDate = null;
//        long organizationPartyId = 0, userId = 0;
//        long? personaId = null, contactMechanismId = null;
//        Guid organizationRealPageId = Guid.Empty, personRealPageId = Guid.Empty;
//        long userEmailContactMechanismId = 0, cloneUserPersonaId = 0;
//        int greenBookRole = 0;
//        List<int> greenBookRoles = [];
//        bool profileChanged = false;
//        long booksCustomerMasterId = 0;

//        // ── Pre-transaction reads ────────────────────────────────────────────
//        var platformAdminRole = (await _internalSettingRepo.GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, cancellationToken))
//            .FirstOrDefault(s => s.Name.Equals("PlatformAdminRole", StringComparison.OrdinalIgnoreCase))?.Value;

//        bool isDelegateAdminEnabled = await GetUnifiedSettingDataAsync("delegateadministrators", cancellationToken);
//        bool usePropertyInstance    = await GetPropertyInstanceUnifiedLoginAsync(cancellationToken);

//        IUserLoginOnly impersonatorLogin = new UserLoginOnly();
//        if (userClaims.ImpersonatedBy != Guid.Empty)
//            impersonatorLogin = await _userLoginRepo.GetUserLoginOnlyAsync(userClaims.ImpersonatedBy);

//        IUserLoginOnly userLoginOnly = await _userLoginRepo.GetUserLoginOnlyAsync(newProfile.userLogin.LoginName);

//        if (newProfile.organization[0].RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId)
//            newProfile.IsRPEmployee = true;

//        IList<UserOrganization> userPersonaOrganizationList = [];
//        OrganizationStatus currentPrimaryOrgStatus = null;
//        if (userLoginOnly is not null)
//        {
//            var userDetails = await _userRepo.GetUserDetailsAsync(null, userLoginOnly.RealPageId.ToString(), cancellationToken);
//            booksCustomerMasterId = userDetails.BooksCustomerMasterId;
//            profileChanged = IsUserProfileChanged(newProfile, userDetails);
//            userPersonaOrganizationList = await _userLoginRepo.ListOrganizationByLoginNameAsync(newProfile.userLogin.LoginName);

//            if (userPersonaOrganizationList.Any(i => i.OrganizationPartyId == newProfile.organization[0].PartyId))
//            {
//                errorStatus.Success = false; errorStatus.ErrorCode = "User.CreateUser.1"; errorStatus.ErrorMsg = "Username already exists in this company.";
//                return new CreateUserResponse<IErrorData> { Status = errorStatus, UserStatus = errorStatus.ErrorMsg };
//            }
//            currentPrimaryOrgStatus = await _userLoginRepo.GetUserOrganizationWithStatusAsync(userLoginOnly.UserId, userLoginOnly.LastLogin, 0, true);
//        }

//        var orgExternalUser = await _orgRepo.GetOrganizationAsync(realPageId: DefaultUserClaim.ExternalCompanyRealPageId);
//        var emailUsageType  = await _usageTypeRepo.ListContactMechanismUsageTypeAsync("Email Notification");

//        if (newProfile.organization?.Count > 0)
//        {
//            organizationPartyId     = newProfile.organization[0].PartyId;
//            organizationRealPageId  = newProfile.organization[0].RealPageId;
//        }
//        var identityProviders = newProfile.organization?.Count > 0
//            ? await _orgRepo.GetOrganizationIdentityProviderTypeAsync(newProfile.organization[0].RealPageId)
//            : new List<IdentityProviderType>();

//        var primaryPropertiesBatch = newProfile.productBatch?.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedPlatform);

//        // ── Clone user ───────────────────────────────────────────────────────
//        if (newProfile.ClonedUser)
//        {
//            cloneUserPersonaId = newProfile.Persona[0].PersonaId;
//            var clonePersona   = await _personaRepo.GetPersonaAsync(cloneUserPersonaId, false, cancellationToken);
//            var cloneOrgList   = await _userLoginRepo.ListOrganizationByEnterpriseUserIdAsync(clonePersona.RealPageId, null);
//            clonePersona.Organization = cloneOrgList.FirstOrDefault(i => i.PartyId == clonePersona.OrganizationPartyId);
//            bool isExternal = clonePersona.Organization?.RelationshipType?.Equals("User Type", StringComparison.OrdinalIgnoreCase) == true
//                && clonePersona.Organization?.RoleNameFrom?.Equals("External User", StringComparison.OrdinalIgnoreCase) == true;

//            var userProducts = (await _db.QueryAsync<PersonaProductUserDetails>(new CommandDefinition(
//                StoredProcNameConstants.SP_ListProductsByPersonaId,
//                new { PersonaId = newProfile.Persona[0].PersonaId, ProductStatusValue = ((int)ProductBatchStatusType.Success).ToString() },
//                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();

//            if (userProducts.Any(m => m.ProductId == 89 || m.ProductId == 104))
//            {
//                int adminId = userProducts.First(m => m.ProductId == 89 || m.ProductId == 104).ProductId;
//                var attribs = await _samlRepo.GetProductSamlDetailsAsync(newProfile.Persona[0].PersonaId, adminId, cancellationToken);
//                if (!attribs.Any()) userProducts.RemoveAll(a => a.ProductId == adminId);
//            }

//            if (userProducts.Count > 0)
//            {
//                long createPersonaId = userClaims.UserRealPageGuid != Guid.Empty
//                    ? await _personaRepo.GetActivePersonaIdAsync(userClaims.UserRealPageGuid, cancellationToken) : 0;

//                foreach (var product in newProfile.productBatch) userProducts.RemoveAll(a => a.ProductId == product.ProductId);

//                UPFMProperty upfmProperty = new();
//                if (primaryPropertiesBatch != null) upfmProperty.id = primaryPropertiesBatch.InputJson.PropertyList.ToList();

//                var personaProductSettings = await _personaRepo.GetPersonaProductSettingsAsync(cloneUserPersonaId, cancellationToken);
//                var pbData = new ManageCloneProductBatch(userClaims).GetUserProductBatchData(
//                    cloneUserPersonaId, userProducts, createPersonaId, upfmProperty, personaProductSettings, isExternal);

//                foreach (var pb in pbData) newProfile.productBatch.Add(pb);
//                foreach (var pb in newProfile.productBatch.Where(x => !x.InputJson.IsAssigned).ToList())
//                    newProfile.productBatch.Remove(pb);

//                // UL role/properties from clone
//                var ulRole = (await _db.QueryAsync<dynamic>(new CommandDefinition(
//                    StoredProcNameConstants.SP_ListRolesForProductsByPersonaId,
//                    new { ProductId = (int)ProductEnum.UnifiedPlatform, PersonaId = cloneUserPersonaId },
//                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();

//                IEnumerable<dynamic> ulPropInstances = usePropertyInstance
//                    ? await _db.QueryAsync<dynamic>(new CommandDefinition(
//                        StoredProcNameConstants.SP_GetPropertyInstanceByPersonaId,
//                        new { PersonaId = cloneUserPersonaId, ProductId = (int)ProductEnum.UnifiedPlatform },
//                        commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))
//                    : [];

//                if (ulPropInstances.Any() && ulRole.Count > 0)
//                {
//                    newProfile.productBatch.Add(new ProductBatch
//                    {
//                        ProductId = (int)ProductEnum.UnifiedPlatform, StatusTypeId = 5, RetryCount = 0,
//                        InputJson = new RolePropertyList
//                        {
//                            RoleList = ulRole.Select<dynamic, string>(r => Convert.ToString(r.RoleId)).ToList(),
//                            PropertyList = ulPropInstances.Select<dynamic, string>(p => Convert.ToString(p.PropertyInstanceID)).ToList()
//                        }
//                    });
//                }
//            }
//        }

//        // ── AO products for SuperUser ────────────────────────────────────────
//        IList<string> aoProductsAvailable = null;
//        if (newProfile.UserTypeId == (int)UserRoleType.SuperUser && !newProfile.ClonedUser)
//            aoProductsAvailable = await GetEditorUserAoProductAsync(
//                userClaims.UserRealPageGuid, userClaims.PersonaId, organizationRealPageId, cancellationToken);

//        // ── Role types ───────────────────────────────────────────────────────
//        var roleTypesList = (await _db.QueryAsync<RoleType>(new CommandDefinition(
//            StoredProcNameConstants.SP_ListRoleType, new { RoleTypeName = "User Role" },
//            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();

//        var SuperUserRole  = roleTypesList.SingleOrDefault(p => p.Name.Equals("SuperUser",        StringComparison.OrdinalIgnoreCase));
//        var UserRole       = roleTypesList.SingleOrDefault(p => p.Name.Equals("User",             StringComparison.OrdinalIgnoreCase));
//        var UserNoEmailRole = roleTypesList.SingleOrDefault(p => p.Name.Equals("User (No Email)", StringComparison.OrdinalIgnoreCase));
//        var rpEmployee     = roleTypesList.SingleOrDefault(p => p.Name.Equals("realpage employee",StringComparison.OrdinalIgnoreCase));
//        var rpExternalUser = roleTypesList.SingleOrDefault(p => p.Name.Equals("external user",    StringComparison.OrdinalIgnoreCase));

//        var userOrgExists    = await IsLoginNameExistsAsAdminInOtherDomainAsync(
//            newProfile.userLogin.LoginName, newProfile.organization[0].RealPageId, booksCustomerMasterId, cancellationToken);

//        bool primaryOrganization = !(userOrgExists.UserExistsAsAdminInOtherDomain || userOrgExists.UserExistsAsRegularUserInOtherDomain)
//            || userOrgExists.UserExistsInThisOrganization
//            || newProfile.organization[0].OrganizationDomain.Name.Equals("Primary");

//        // ── Transaction ──────────────────────────────────────────────────────
//        OpenIfClosed();
//        using var tx = _db.BeginTransaction();
//        IList<OrganizationPrimary> orgList = [];

//        try
//        {
//            fromDate  = newProfile.userLogin.FromDate ?? fromDate;
//            thruDate  = newProfile.userLogin.ThruDate;
//            if (newProfile.userLogin.ThruDate is null) newProfile.userLogin.ThruDate = new DateTime(9999, 12, 31);

//            string sourceType = newProfile.MigratedUser
//                ? CreateUserSourceType.MigrationTool.ToString()
//                : newProfile.CreateUserSourceType?.ToString() ?? CreateUserSourceType.UnifiedPlatform.ToString();

//            IIdentityProviderType idpt = identityProviders.FirstOrDefault(a => a.IsLocal == !newProfile.userLogin.Is3rdPartyIDP)
//                ?? identityProviders.FirstOrDefault();

//            // User type conflict check
//            if (newProfile.UserTypeId != (int)UserRoleType.ExternalUser
//                && userPersonaOrganizationList.Any(x => x.PartyRoleTypeId != (int)UserRoleType.ExternalUser)
//                && newProfile.UserTypeId != (int)UserRoleType.RealPageEmployee
//                && userPersonaOrganizationList.Any(x => x.PartyRoleTypeId != (int)UserRoleType.RealPageEmployee)
//                && (!userOrgExists.UserExistsAsAdminInOtherDomain || !userOrgExists.UserExistsAsRegularUserInOtherDomain)
//                && userOrgExists.UserExistsInThisOrganization)
//            {
//                tx.Rollback();
//                errorStatus.Success = false; errorStatus.ErrorCode = "User.CreateUser.2"; errorStatus.ErrorMsg = "This user type already exists for this username.";
//                return new CreateUserResponse<IErrorData> { Status = errorStatus, UserStatus = errorStatus.ErrorMsg };
//            }

//            RepositoryResponse rr;

//            if (userPersonaOrganizationList.Count == 0)
//            {
//                // ── Create Person ────────────────────────────────────────────
//                processTracker = "Create Person";
//                rr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreatePerson, tx,
//                    new { newProfile.Title, newProfile.FirstName, newProfile.MiddleName, newProfile.LastName, newProfile.Suffix, PreferredContactMethodId = 0, RealPageId = Guid.Empty }, cancellationToken);
//                newProfile.RealPageId = rr.RealPageId; newProfile.PartyId = rr.Id;
//                if (!string.IsNullOrEmpty(rr.ErrorMessage)) return RollbackError(tx, errorStatus, "User.CreateUser.3", rr.ErrorMessage, response);

//                // ── Create UserLogin ─────────────────────────────────────────
//                processTracker = "Create UserLogin";
//                rr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreateUserLogin, tx,
//                    new { newProfile.RealPageId, newProfile.userLogin.LoginName, CreateUserSourceType = sourceType }, cancellationToken);
//                if (rr.Id == 0) return RollbackError(tx, errorStatus, "User.CreateUser.4", "Username already exists!", response);
//                if (!string.IsNullOrEmpty(rr.ErrorMessage)) return RollbackError(tx, errorStatus, "User.CreateUser.5", rr.ErrorMessage, response);
//                userId = rr.Id; personRealPageId = newProfile.RealPageId;

//                // ── Set password ─────────────────────────────────────────────
//                if (!string.IsNullOrEmpty(newProfile.Password))
//                {
//                    var pwd = newProfile.Password.PasswordHash();
//                    newProfile.userLogin.PasswordHash = pwd.PasswordHash;
//                    newProfile.userLogin.PasswordSalt = pwd.PasswordSalt;
//                }
//                await Q<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserLogin, tx,
//                    new { newProfile.RealPageId, newProfile.userLogin.LoginName, newProfile.userLogin.PasswordHash, newProfile.userLogin.PasswordSalt, FromDate = fromDate, newProfile.userLogin.ThruDate, PartyId = organizationPartyId }, cancellationToken);

//                // ── Phones ───────────────────────────────────────────────────
//                if (newProfile.TelecommunicationNumber.Count > 0)
//                {
//                    var phoneRr = await UpdateProfileAsync(tx, newProfile.RealPageId, newProfile, cancellationToken);
//                    if (phoneRr.Id == 0) return RollbackError(tx, errorStatus, "User.CreateUser.17", "There was an error while new user profile update.", response);
//                }

//                // ── Notification email ───────────────────────────────────────
//                processTracker = "Save notification email";
//                if (newProfile.UserTypeId != (int)UserRoleType.UserNoEmail)
//                    newProfile.NotificationEmail = string.IsNullOrEmpty(newProfile.NotificationEmail) && EmailFormatValidation.IsValidEmail(newProfile.userLogin.LoginName)
//                        ? newProfile.userLogin.LoginName : newProfile.NotificationEmail;

//                if (!string.IsNullOrEmpty(newProfile.NotificationEmail) && EmailFormatValidation.IsValidEmail(newProfile.NotificationEmail))
//                {
//                    var emailCm = emailUsageType.SingleOrDefault(p => p.Name.Equals("Email", StringComparison.OrdinalIgnoreCase));

//                    rr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreateContactMechanism, tx, new { ContactMechanismId = (long?)null }, cancellationToken);
//                    if (rr.Id == 0) return RollbackError(tx, errorStatus, "User.CreateUser.19", "An error was encountered when creating a contact mechanism.", response);
//                    contactMechanismId = rr.Id;

//                    rr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_LinkContactMechanismToParty, tx,
//                        new { newProfile.RealPageId, PartyContactMechanismId = 0L, ContactMechanismId = contactMechanismId, FromDate = utcNow, ThruDate = utcMax }, cancellationToken);
//                    if (rr.Id == 0) return RollbackError(tx, errorStatus, "User.CreateUser.20", "An error was encountered while linking user contact mechanism.", response);
//                    response.PartyContactMechanismIdTo = rr.Id;

//                    rr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism, tx,
//                        new { PartyContactMechanismId = response.PartyContactMechanismIdTo, ContactMechanismUsageTypeId = emailCm.ContactMechanismUsageTypeId }, cancellationToken);
//                    if (rr.Id == 0) return RollbackError(tx, errorStatus, "User.CreateUser.21", "An error was encountered when assigning a usage type to the contact mechanism.", response);

//                    rr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreateElectronicAddress, tx,
//                        new { ContactMechanismId = contactMechanismId, ElectronicAddressString = newProfile.NotificationEmail, ElectronicAddressType = emailCm.Name }, cancellationToken);
//                    if (rr.Id == 0) return RollbackError(tx, errorStatus, "User.CreateUser.22", "An error was encountered when creating an email address.", response);
//                    userEmailContactMechanismId = response.PartyContactMechanismIdTo;
//                }

//                orgList = [new OrganizationPrimary { OrganizationRealPageId = organizationRealPageId, OrganizationPartyId = organizationPartyId, PrimaryOrganization = primaryOrganization, OrganizationFromDate = fromDate.Value, OrganizationThruDate = thruDate }];
//            }
//            else
//            {
//                // ── Existing user ────────────────────────────────────────────
//                userId = userLoginOnly.UserId; personRealPageId = userLoginOnly.RealPageId; newProfile.RealPageId = userLoginOnly.RealPageId;

//                if (newProfile.UserTypeId == (int)UserRoleType.ExternalUser)
//                {
//                    orgList = [new OrganizationPrimary { OrganizationRealPageId = organizationRealPageId, OrganizationPartyId = organizationPartyId, PrimaryOrganization = false, OrganizationFromDate = fromDate.Value, OrganizationThruDate = thruDate }];
//                    if (userPersonaOrganizationList.Count == 1 && userPersonaOrganizationList.Any(i => i.PartyRoleTypeId == (int)UserRoleType.ExternalUser)
//                        && !userPersonaOrganizationList.Any(m => m.OrganizationPartyId == orgExternalUser.PartyId))
//                    {
//                        DateTime newFrom = fromDate.Value;
//                        if (currentPrimaryOrgStatus?.FromDate < fromDate.Value) newFrom = currentPrimaryOrgStatus.FromDate;
//                        orgList.Add(new OrganizationPrimary { OrganizationRealPageId = orgExternalUser.RealPageId, OrganizationPartyId = orgExternalUser.PartyId, PrimaryOrganization = true, OrganizationFromDate = newFrom });
//                    }
//                }
//                else if (userPersonaOrganizationList.Any(x => x.PrimaryOrganization && x.OrganizationPartyId == orgExternalUser.PartyId))
//                {
//                    rr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_UnlinkPersonToOrganization, tx,
//                        new { PersonRealPageId = userLoginOnly.RealPageId, OrganizationRealPageId = orgExternalUser.RealPageId }, cancellationToken);
//                    if (rr is null) return RollbackError(tx, errorStatus, "User.CreateUser.6", "There was an error unassociating the user to a user role.", response);
//                    orgList = [new OrganizationPrimary { OrganizationRealPageId = organizationRealPageId, OrganizationPartyId = organizationPartyId, PrimaryOrganization = true, OrganizationFromDate = fromDate.Value, OrganizationThruDate = thruDate }];
//                }
//                else
//                {
//                    orgList = [new OrganizationPrimary { OrganizationRealPageId = organizationRealPageId, OrganizationPartyId = organizationPartyId, PrimaryOrganization = primaryOrganization, OrganizationFromDate = fromDate.Value, OrganizationThruDate = thruDate }];
//                }

//                if (profileChanged && newProfile.UserTypeId != (int)UserRoleType.ExternalUser)
//                {
//                    rr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePerson, tx,
//                        new { RealPageId = newProfile.RealPageId, newProfile.FirstName, newProfile.MiddleName, newProfile.LastName }, cancellationToken);
//                    if (rr.Id == 0) return RollbackError(tx, errorStatus, "User.CreateUser.26", rr.ErrorMessage, response);
//                }
//            }

//            // ── Pending communication event ──────────────────────────────────
//            processTracker = "Pending email notification";
//            if (idpt?.IsLocal == true)
//            {
//                var orgCms  = await ListContactMechanismForPersonAsync(tx, organizationRealPageId, emailUsageType, cancellationToken);
//                if (userEmailContactMechanismId == 0)
//                {
//                    var userCms = await ListContactMechanismForPersonAsync(tx, personRealPageId, emailUsageType, cancellationToken);
//                    if (userCms.Count > 0) userEmailContactMechanismId = userCms[0].PartyContactMechanismId;
//                }
//                long orgCmId = orgCms.FirstOrDefault()?.PartyContactMechanismId ?? 0;
//                if (orgCmId > 0 && userEmailContactMechanismId > 0)
//                    await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreateCommunicationEvent, tx,
//                        new { StatusTypeID = (int)EmailStatusType.EmailPending, FromPartyContactMechanismId = orgCmId, ToPartyContactMechanismId = userEmailContactMechanismId, Started = utcNow, Ended = utcNow, Note = "pending", CommunicationEventID = (long?)null }, cancellationToken);
//            }

//            // ── Status threshold ─────────────────────────────────────────────
//            long primaryOrgId = orgList.Any(x => x.PrimaryOrganization)
//                ? orgList.First(x => x.PrimaryOrganization).OrganizationPartyId
//                : currentPrimaryOrgStatus?.PartyId ?? organizationPartyId;

//            var activityDetails = (await _db.QueryAsync<Activity>(new CommandDefinition(
//                StoredProcNameConstants.SP_ListActivity, new { PartyId = primaryOrgId },
//                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();
//            var newUserActivity = activityDetails.FirstOrDefault(x => x.ActivityTypeId == (int)ActivityType.NewUserRegistration);
//            DateTime? statusThruDate = newUserActivity is not null
//                ? fromDate.Value.AddMinutes(newUserActivity.ActivityTokenExpirationMinutes)
//                : fromDate.Value.AddHours(72);

//            int userStatusId  = (int)UserUiStatusType.Active;
//            long AssignUserPersonaId = 0, userLoginPersonaId = 0;

//            // ── Per-org loop ─────────────────────────────────────────────────
//            foreach (var currentOrg in orgList)
//            {
//                DateTime? currentStatusThruDate = statusThruDate;

//                // Determine status
//                userStatusId = DetermineUserStatus(
//                    currentOrg, orgExternalUser, userPersonaOrganizationList,
//                    currentPrimaryOrgStatus, fromDate.Value, idpt, newProfile,
//                    ref currentStatusThruDate);

//                if (persona is null) return RollbackError(tx, errorStatus, "User.CreateUser.8", "User has no persona.", response);
//                var personaFromUI = persona[0];
//                DateTime? personaFrom = (currentPrimaryOrgStatus is not null && orgExternalUser.PartyId == currentOrg.OrganizationPartyId)
//                    ? currentPrimaryOrgStatus.FromDate : personaFromUI.FromDate ?? utcNow;
//                DateTime? personaThru = (currentPrimaryOrgStatus is not null && orgExternalUser.PartyId == currentOrg.OrganizationPartyId)
//                    ? null : personaFromUI.ThruDate;

//                // Create UserLoginPersona
//                object ulpParam = userClaims.ImpersonatedByName is not null
//                    ? new { UserLoginId = userId, StatusTypeId = userStatusId, OrganizationPartyId = currentOrg.OrganizationPartyId, PrimaryOrganization = currentOrg.PrimaryOrganization, FromDate = currentOrg.OrganizationFromDate, ThruDate = currentOrg.OrganizationThruDate, StatusThruDate = currentStatusThruDate, newProfile.IsRPEmployee, newProfile.IsDelegateAdmin, newProfile.IsRealPartner }
//                    : (object)new { UserLoginId = userId, StatusTypeId = userStatusId, OrganizationPartyId = currentOrg.OrganizationPartyId, PrimaryOrganization = currentOrg.PrimaryOrganization, FromDate = currentOrg.OrganizationFromDate, ThruDate = currentOrg.OrganizationThruDate, StatusThruDate = currentStatusThruDate, newProfile.IsRPEmployee, newProfile.IsDelegateAdmin };

//                rr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreateUserLoginPersona, tx, ulpParam, cancellationToken);
//                if (rr.Id == 0) return RollbackError(tx, errorStatus, "User.CreateUser.7", "Error creating the user login status.", response);
//                userLoginPersonaId = rr.Id;

//                // Determine persona type
//                long? ptid = currentOrg.OrganizationPartyId == orgExternalUser.PartyId ? (int)PersonaType.Primary
//                    : personaFromUI.Name.ToLowerInvariant() switch { "system administrator" => (int)PersonaType.SuperUser, _ => (int)PersonaType.Primary };

//                personaId = null;
//                rr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreatePersona, tx,
//                    new { PersonRealPageId = personRealPageId, UserLoginPersonaId = userLoginPersonaId, OrganizationRealPageId = currentOrg.OrganizationRealPageId, PersonaTypeId = ptid, UserId = userId, personaFromUI.PersonaEnvironmentTypeId, FromDate = personaFrom, ThruDate = personaThru, personaId }, cancellationToken);
//                if (rr.Id == 0) return RollbackError(tx, errorStatus, "User.CreateUser.9", "Persona was not created.", response);
//                personaId = rr.Id;
//                if (organizationPartyId == currentOrg.OrganizationPartyId) { AssignUserPersonaId = rr.Id; response.PersonaId = rr.Id; }

//                // External user company association
//                if (FeatureFlag.GetUserCompanyAssociationFeatureFlag() && newProfile.ExternalUserRelationship?.ThirdPartyRelationShipId > 0)
//                {
//                    var euRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_UpdateExternalUserRelationship, tx,
//                        new { UserLoginPersonaId = userLoginPersonaId, ThirdPartyRelationshipId = newProfile.ExternalUserRelationship.ThirdPartyRelationShipId, CompanyName = newProfile.ExternalUserRelationship.ThirdPartyCompanyName, ThirdPartyCompanyRealPageId = newProfile.ExternalUserRelationship.ThirdPartyCompanyRealPageId, OperatorCode = newProfile.ExternalUserRelationship.OperatorCode, OperatorValue = newProfile.ExternalUserRelationship.OperatorValue }, cancellationToken);
//                    if (euRr?.Id == 0) throw new InvalidOperationException("Create ExternalUser Relationship failed.");
//                }

//                // Enterprise role
//                var entRole = newProfile.productBatch?.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedUI);
//                if (entRole?.InputJson?.RoleList?.Count > 0)
//                {
//                    var erRr = await InsertUpdateEnterpriseRoleToUserAsync(tx, Convert.ToInt32(entRole.InputJson.RoleList.First()), personaId, cancellationToken);
//                    if (erRr.Id == 0) return RollbackError(tx, errorStatus, "User.CreateUser.9", "User not assigned to Enterprise Role.", response);
//                }

//                // Enterprise roles for org
//                var enterpriseRoles = (await _db.QueryAsync<EnterpriseRole>(new CommandDefinition(
//                    StoredProcNameConstants.SP_SecurityListRolesByRealPageID, new { realPageId = currentOrg.OrganizationRealPageId },
//                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();

//                var gbBatch = newProfile.productBatch?.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedPlatform);
//                (greenBookRole, greenBookRoles) = await ResolveGreenBookRoleAsync(
//                    tx, currentOrg.OrganizationPartyId, orgExternalUser.PartyId, newProfile, gbBatch,
//                    SuperUserRole, platformAdminRole, enterpriseRoles, cloneUserPersonaId, cancellationToken);

//                bool roleLinked = await LinkPersonaToRolesAsync(tx, personaId.Value, greenBookRole, greenBookRoles, userClaims.UserId, cancellationToken);
//                if (!roleLinked) return RollbackError(tx, errorStatus, "User.CreateUser.10", "There was an error associating the persona to a user role.", response);

//                // Property mapping (SuperUser platform admin)
//                if (SuperUserRole?.PartyRoleTypeId == newProfile.UserTypeId && enterpriseRoles.Any(r => r.Role.Equals(platformAdminRole, StringComparison.OrdinalIgnoreCase) && r.RoleId > 0))
//                    gbBatch = new ProductBatch { InputJson = new RolePropertyList { PropertyList = ["-1"] } };

//                if (gbBatch?.InputJson?.PropertyList?.Count > 0 || gbBatch?.InputJson?.RemovedPropertyList?.Count > 0)
//                {
//                    string propJson = JsonConvert.SerializeObject(gbBatch);
//                    string propSp = usePropertyInstance ? StoredProcNameConstants.SP_AddUpdatePropertyInstanceMapping : StoredProcNameConstants.SP_AddUpdatePropertyMapping;
//                    object propParam = usePropertyInstance
//                        ? (object)new { PersonaId = personaId, ProductId = (int)ProductEnum.UnifiedPlatform, PropertyInstanceJSON = propJson }
//                        : new { PersonaId = personaId, ProductId = (int)ProductEnum.UnifiedPlatform, PropertyJSON = propJson };
//                    rr = await Q<RepositoryResponse>(propSp, tx, propParam, cancellationToken);
//                    if (rr.Id == 0) return RollbackError(tx, errorStatus, "User.CreateUser.27", $"Error assigning top level properties to persona: {personaId}.", response);
//                }

//                // EmployeeId
//                if (userLoginPersonaId > 0)
//                {
//                    rr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreateEmployeeId, tx, new { UserLoginPersonaId = userLoginPersonaId, newProfile.EmployeeId }, cancellationToken);
//                    if (rr.Id == 0) return RollbackError(tx, errorStatus, "User.CreateUser.28", $"Error creating EmployeeId: {userLoginPersonaId}", response);
//                }

//                // Supervisor
//                if (newProfile.SuperVisorUserId > 0)
//                {
//                    rr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_InsertUpdateSuperVisor, tx, new { UserId = userId, newProfile.SuperVisorUserId }, cancellationToken);
//                    if (rr.Id == 0) return RollbackError(tx, errorStatus, "User.CreateUser.28", $"Error creating Supervisor: {userLoginPersonaId}", response);
//                }

//                // Employer / UserType role types
//                processTracker = "Set Default Employment Role";
//                var orgRoleTypes = (await _db.QueryAsync<RoleType>(new CommandDefinition(
//                    StoredProcNameConstants.SP_ListRoleType, new { RoleTypeName = "Organization Role" },
//                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();
//                var Employer  = orgRoleTypes.SingleOrDefault(p => p.Name == "Employer");
//                var UserTypeR = orgRoleTypes.SingleOrDefault(p => p.Name == "User Type");
//                if (Employer  is null) return RollbackError(tx, errorStatus, "User.CreateUser.13", "Employer role is missing.", response);
//                if (UserTypeR is null) return RollbackError(tx, errorStatus, "User.CreateUser.14", "User Type role is missing.", response);
//                if (SuperUserRole is null || UserRole is null || UserNoEmailRole is null || rpEmployee is null)
//                    return RollbackError(tx, errorStatus, "User.CreateUser.15", "User role(s) missing.", response);

//                int rtFrom = newProfile.UserTypeId switch
//                {
//                    (int)UserRoleType.SuperUser        => SuperUserRole.PartyRoleTypeId,
//                    (int)UserRoleType.RealPageEmployee => rpEmployee.PartyRoleTypeId,
//                    (int)UserRoleType.UserNoEmail      => UserNoEmailRole.PartyRoleTypeId,
//                    (int)UserRoleType.ExternalUser     => rpExternalUser?.PartyRoleTypeId ?? 0,
//                    _                                  => UserRole.PartyRoleTypeId
//                };

//                rr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_LinkPersonToOrganization, tx,
//                    new { PersonRealPageId = personRealPageId, OrganizationRealPageId = currentOrg.OrganizationRealPageId, RoleTypeIdFrom = rtFrom, RoleTypeIdTo = (int)UserTypeR.PartyRoleTypeId }, cancellationToken);
//                if (rr is null) return RollbackError(tx, errorStatus, "User.CreateUser.16", "There was an error associating the user to a user role.", response);
//            }

//            // ── RP Employee: update previous orgs ────────────────────────────
//            if (!userPersonaOrganizationList.Any(x => x.PartyRoleTypeId == (int)UserRoleType.RealPageEmployee)
//                && newProfile.UserTypeId == (int)UserRoleType.RealPageEmployee)
//            {
//                foreach (var prev in userPersonaOrganizationList)
//                {
//                    var relType = await Q<PartyRelationship>(StoredProcNameConstants.SP_GetPartyRelationshipByRealPageId, tx,
//                        new { realPageIdFrom = personRealPageId, realPageIdTo = prev.OrganizationRealPageId, roleTypeName = (string)null, relationshipTypeName = "User Type" }, cancellationToken);
//                    if (relType is not null && relType.RoleTypeIdFrom != (int)UserRoleType.ExternalUser)
//                    {
//                        rr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePersonToOrganization, tx,
//                            new { personRealPageId, prev.OrganizationRealPageId, unlinkRoleTypeIdFrom = relType.RoleTypeIdFrom, linkRoleTypeIdFrom = (int)UserRoleType.ExternalUser, roleTypeIdTo = relType.RoleTypeIdTo }, cancellationToken);
//                        if (rr.Id == 0) return RollbackError(tx, errorStatus, "User.CreateUser.29", "Unable to set new user type.", response);
//                    }
//                }
//            }

//            // ── Custom fields ────────────────────────────────────────────────
//            processTracker = "Create User custom fields";
//            if (newProfile.CustomFields?.Count > 0)
//            {
//                var ulpList = (await _db.QueryAsync<UserLoginPersona>(new CommandDefinition(
//                    StoredProcNameConstants.SP_GetUserLoginPersona, new { UserLoginId = userId, OrganizationPartyId = organizationPartyId },
//                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();
//                newProfile.CustomFields.ToList().ForEach(c => c.UserLoginPersonaId = ulpList[0].UserLoginPersonaId);
//                string cfJson = JsonConvert.SerializeObject(newProfile.CustomFields);
//                if (ValidateJson.IsValidJson<IList<CustomFieldValue>>(cfJson))
//                {
//                    rr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_AddUpdateFieldValue, tx, new { JSON = cfJson, CreatedBy = userClaims.UserId }, cancellationToken);
//                    if (rr.Id == 0 && !string.IsNullOrWhiteSpace(rr.ErrorMessage))
//                        return RollbackError(tx, errorStatus, "User.CreateUser.18", "User Custom Fields was not created.", response);
//                }
//            }

//            // ── Identity provider ────────────────────────────────────────────
//            if (primaryOrgId == organizationPartyId)
//            {
//                rr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_LinkIdentityProviderToUserLogin, tx,
//                    new { UserId = userId, ContactMechanismID = idpt?.ContactMechanismId }, cancellationToken);
//                if (rr.Id == 0) return RollbackError(tx, errorStatus, "User.CreateUser.23", "Link Identity Provider to UserLogin failed.", response);
//            }

//            // ── Save product details ─────────────────────────────────────────
//            processTracker = "SaveProductDetails";
//            await SaveProductDetailsAsync(
//                tx, newProfile.productBatch, response, userClaims.PersonaId, AssignUserPersonaId,
//                userClaims.UserRealPageGuid, organizationRealPageId, errorStatus, newProfile.UserTypeId,
//                true, impersonatorLogin.UserId, aoProductsAvailable, newProfile.MigratedUser, true,
//                greenBookRole, "add", false, newProfile.RoleIdList, cancellationToken);

//            // ── Delegate admin roles ─────────────────────────────────────────
//            if (isDelegateAdminEnabled && newProfile.IsDelegateAdmin && newProfile.DelegateRoleTemplate?.RoleTemplateId?.Any() == true)
//            {
//                var ulpList = (await _db.QueryAsync<UserLoginPersona>(new CommandDefinition(
//                    StoredProcNameConstants.SP_GetUserLoginPersona, new { UserLoginId = userId, OrganizationPartyId = organizationPartyId },
//                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();
//                var daRr = await InsertUpdateDelegateAdminRoleAsync(tx, ulpList[0].UserLoginPersonaId, newProfile.DelegateRoleTemplate.RoleTemplateId.ToList(), cancellationToken);
//                if (daRr.Id == 0) throw new InvalidOperationException("Create Delegate Admin Role failed.");
//            }

//            newProfile.userLogin.UserId   = userId;
//            newProfile.userLogin.RealPageId = personRealPageId;
//            response.UserStatus      = "User created successfully.";
//            response.Status          = errorStatus;
//            response.UserRealPageGuid = personRealPageId;
//            tx.Commit();
//            return response;
//        }
//        catch (Exception ex)
//        {
//            tx.Rollback();
//            _logger.LogError(ex, "CreateUserAsync failed. Process={P}", processTracker);
//            errorStatus.Success = false; errorStatus.ErrorCode = "User.CreateUser.24";
//            errorStatus.ErrorMsg = $"Create User Error: {ex.Message}. Process: {processTracker}";
//            return new CreateUserResponse<IErrorData> { Status = errorStatus, UserStatus = errorStatus.ErrorMsg, UserRealPageGuid = Guid.Empty };
//        }
//    }

//    #endregion

//    #region UpdateNewUserAsync

//    /// <inheritdoc/>
//    public async Task<RepositoryResponse> UpdateNewUserAsync(
//        string userLogin, Profile newProfile, int partyRoleTypeId,
//        string companyJobTitle, string activityToken,
//        CancellationToken cancellationToken = default)
//    {
//        var response = new RepositoryResponse();
//        var loginOnly = await _userLoginRepo.GetUserLoginOnlyAsync(userLogin);
//        if (loginOnly is null) { response.ErrorMessage = "User Name is incorrect or not found."; return response; }

//        long orgPartyId = 0;
//        var listOrg = await _credentialRepo.ListOrganizationByRealPageIdAsync(loginOnly.RealPageId);
//        if (listOrg?.Count > 0) orgPartyId = listOrg[0].PartyId;

//        var tokenDetail = await _credentialRepo.GetActivityTokenAsync(userLogin, activityToken, (int)ActivityType.NewUserRegistrationVerification, orgPartyId);
//        if (tokenDetail is null || tokenDetail.EnterpriseUserId <= 0) { response.ErrorMessage = "Validation token does not match with user."; return response; }

//        var person = new ManagePerson().GetPerson(loginOnly.RealPageId);
//        if (person is null) { response.ErrorMessage = "Person details not found."; return response; }

//        OpenIfClosed();
//        using var tx = _db.BeginTransaction();
//        try
//        {
//            person.Title = companyJobTitle;
//            response = await Q<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePerson, tx,
//                new { loginOnly.RealPageId, person.FirstName, person.MiddleName, person.LastName, person.Title, person.Suffix, person.PreferredContactMethodId }, cancellationToken)
//                ?? new RepositoryResponse { ErrorMessage = "New profile Error: Company Job Title update failed." };

//            if (response.Id > 0)
//            {
//                response = await UpdateUserProfileAsync(tx, person.RealPageId, newProfile, cancellationToken);
//                if (response.Id == 0) { tx.Rollback(); response.ErrorMessage = "Error: " + response.ErrorMessage; return response; }
//            }

//            tx.Commit();
//        }
//        catch (Exception ex)
//        {
//            tx.Rollback();
//            response.ErrorMessage = "Error: " + ex.Message;
//        }
//        return response;
//    }

//    #endregion

//    #region UpdateUserListUserAsync

//    /// <inheritdoc/>
//    public async Task<RepositoryResponse> UpdateUserListUserAsync(
//        ProfileDetail userProfile, IList<Persona> updatePersona,
//        IList<Persona> deletePersona, int userTypeId, IList<Organization> listOrg,
//        CancellationToken cancellationToken = default)
//    {
//        var response = new RepositoryResponse();
//        DateTime utcNow = DateTime.UtcNow, utcMax = DateTime.MaxValue.ToUniversalTime();
//        DateTime? fromDate = utcNow, thruDate = utcMax;
//        Guid personRealPageId = userProfile.RealPageId;
//        Guid organizationRealPageId = listOrg?.Count > 0 ? listOrg[0].RealPageId : Guid.Empty;
//        long? orgPartyId = listOrg?.Count > 0 ? listOrg[0].PartyId : (long?)null;
//        string processTracker = "";

//        OpenIfClosed();
//        using var tx = _db.BeginTransaction();
//        try
//        {
//            processTracker = "Update User Login";
//            var currentLogin = await Q<UserLogin>(StoredProcNameConstants.SP_GetUserLogin, tx, new { userProfile.RealPageId }, cancellationToken);
//            if (userProfile.userLogin.LoginName != currentLogin.LoginName)
//            {
//                var checkLogin = await Q<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, tx, new { EnterpriseUserName = userProfile.userLogin.LoginName }, cancellationToken);
//                if (checkLogin is not null) { tx.Rollback(); response.ErrorMessage = "Username already exists!"; return response; }
//            }

//            fromDate = userProfile.userLogin.FromDate?.ToUniversalTime() ?? fromDate;
//            if (userProfile.userLogin.ThruDate.HasValue && userProfile.userLogin.ThruDate.Value.Date == utcMax.Date)
//                userProfile.userLogin.ThruDate = userProfile.userLogin.ThruDate.Value.AddMilliseconds(-999);

//            response = await Q<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserLogin, tx,
//                new { userProfile.RealPageId, userProfile.userLogin.LoginName, currentLogin.PasswordHash, currentLogin.PasswordSalt, FromDate = fromDate, ThruDate = userProfile.userLogin.ThruDate, PartyId = orgPartyId }, cancellationToken);

//            DateTime? statusThruDate = userProfile.userLogin.IsActive == true ? null : (DateTime?)DateTime.UtcNow.AddMinutes(-1);
//            await _db.ExecuteAsync(new CommandDefinition(
//                StoredProcNameConstants.SP_UpdateUserStatusByCompany,
//                new { RealPageId = userProfile.RealPageId, OrganizationPartyId = orgPartyId, StatusTypeId = UserUiStatusType.Active, FromDate = fromDate, StatusThruDate = statusThruDate },
//                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));

//            // Update notification email
//            if (!string.IsNullOrEmpty(userProfile.NotificationEmail) && EmailFormatValidation.IsValidEmail(userProfile.NotificationEmail))
//            {
//                if (userProfile.contactMechanism.Count > 0)
//                {
//                    var emailAddr = userProfile.contactMechanism.FirstOrDefault(p => p.contactMechanismUsageType.ContactMechanismUsageTypeId == 301);
//                    var emailRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreateElectronicAddress, tx,
//                        new { emailAddr.ContactMechanismId, ElectronicAddressString = userProfile.NotificationEmail }, cancellationToken);
//                    if (emailRr.Id == 0) { tx.Rollback(); response.ErrorMessage = "Notification email was not created."; return response; }
//                }
//                else
//                {
//                    await CreateNotificationEmailAsync(tx, userProfile.RealPageId, userProfile.NotificationEmail, utcNow, utcMax, cancellationToken);
//                }
//            }

//            // Update User Type
//            processTracker = "Update User Type";
//            var relType = await Q<PartyRelationship>(StoredProcNameConstants.SP_GetPartyRelationshipByRealPageId, tx,
//                new { realPageIdFrom = userProfile.RealPageId, realPageIdTo = organizationRealPageId, roleTypeName = (string)null, relationshipTypeName = "User Type" }, cancellationToken);

//            if (relType.RoleTypeIdFrom != userTypeId)
//            {
//                var rtRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePersonToOrganization, tx,
//                    new { personRealPageId, organizationRealPageId, unlinkRoleTypeIdFrom = relType.RoleTypeIdFrom, linkRoleTypeIdFrom = userTypeId, roleTypeIdTo = relType.RoleTypeIdTo }, cancellationToken);
//                if (rtRr.Id == 0) { tx.Rollback(); response.ErrorMessage = "Unable to set new user type."; return response; }
//            }

//            // Persona updates
//            processTracker = "Update Persona List";
//            foreach (var up in updatePersona)
//            {
//                if (up.PersonaId == 0)
//                {
//                    fromDate = up.FromDate ?? utcNow; thruDate = up.ThruDate;
//                    var pRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreatePersona, tx,
//                        new { personRealPageId, organizationRealPageId, personaTypeId = 1L, up.PersonaEnvironmentTypeId, fromDate, thruDate, personaId = (long?)null }, cancellationToken);
//                    if (pRr.Id == 0) { tx.Rollback(); response.ErrorMessage = "Persona was not created."; return response; }

//                    var ptRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreatePersonaType, tx, new { personaName = up.Name, personaTypeId = (long?)null }, cancellationToken);
//                    if (ptRr.Id == 0) { tx.Rollback(); response.ErrorMessage = $"Persona name: {up.Name} was not created."; return response; }

//                    var puRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePersona, tx, new { personaId = pRr.Id, up.PersonaEnvironmentTypeId, personaTypeId = ptRr.Id }, cancellationToken);
//                    if (puRr.Id == 0) { tx.Rollback(); response.ErrorMessage = $"Persona name: {up.Name} was not associated."; return response; }
//                }
//                else
//                {
//                    fromDate = up.FromDate ?? utcNow; thruDate = up.ThruDate;
//                    await Q<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePersona, tx, new { personaId = up.PersonaId, up.PersonaTypeId, up.PersonaEnvironmentTypeId, fromDate, thruDate }, cancellationToken);
//                    var ptRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreatePersonaType, tx, new { personaName = up.Name, personaTypeId = (long?)null }, cancellationToken);
//                    if (ptRr.Id == 0) { tx.Rollback(); response.ErrorMessage = $"Persona name: {up.Name} was not created."; return response; }
//                    var puRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePersona, tx, new { personaId = up.PersonaId, personaTypeId = ptRr.Id, up.PersonaEnvironmentTypeId }, cancellationToken);
//                    if (puRr.Id == 0) { tx.Rollback(); response.ErrorMessage = $"Persona name: {up.Name} was not associated."; return response; }
//                }
//            }

//            foreach (var dp in deletePersona)
//                await Q<Persona>(StoredProcNameConstants.SP_RemovePersona, tx, new { personaId = dp.PersonaId }, cancellationToken);

//            tx.Commit();
//            response.RealPageId = userProfile.RealPageId;
//        }
//        catch (Exception ex)
//        {
//            tx.Rollback();
//            response.ErrorMessage = $"Update User Error: {ex.Message}. Process: {processTracker}";
//        }
//        return response;
//    }

//    #endregion

//    #region UpdateUserAsync

//    /// <inheritdoc/>
//    public async Task<RepositoryResponse> UpdateUserAsync(
//        Guid loggedInUserRealPageId, IProfileDetail newProfile, IProfileDetail oldProfile,
//        CancellationToken cancellationToken = default)
//    {
//        var userClaims = _userClaimAccessor.Current;

//        // ── Pre-transaction reads ────────────────────────────────────────────
//        var orgExternalUser    = await _orgRepo.GetOrganizationAsync(realPageId: DefaultUserClaim.ExternalCompanyRealPageId);
//        var loginOnly          = await _userLoginRepo.GetUserLoginOnlyAsync(newProfile.RealPageId);
//        var userOrgList        = await _userLoginRepo.ListOrganizationByLoginNameAsync(loginOnly.LoginName);
//        bool userIsExternal    = userOrgList.All(x => x.PartyRoleTypeId == (int)UserRoleType.ExternalUser);

//        IList<IdentityProviderType> idpList = oldProfile.Persona[0]?.Organization is not null
//            ? await _orgRepo.GetOrganizationIdentityProviderTypeAsync(oldProfile.Persona[0].Organization.RealPageId) : [];

//        var existingRoleIds      = await _userRoleRightRepo.GetRoleIdsByPersonaAsync(oldProfile.Persona[0].PersonaId, (int)ProductEnum.UnifiedPlatform);
//        var currentPrimaryStatus = await _userLoginRepo.GetUserOrganizationWithStatusAsync(newProfile.userLogin.UserId, loginOnly.LastLogin, 0, true);
//        var currentOrgStatus     = await _userLoginRepo.GetUserOrganizationWithStatusAsync(newProfile.userLogin.UserId, loginOnly.LastLogin, oldProfile.Persona[0].OrganizationPartyId, false);
//        long createUserPersonaId = await _personaRepo.GetActivePersonaIdAsync(loggedInUserRealPageId);
//        var personaList          = await _personaRepo.ListActivePersonaAsync(newProfile.RealPageId, true);
//        if (!currentOrgStatus.PrimaryOrganization)
//            personaList = personaList.Where(p => p.OrganizationPartyId == currentOrgStatus.PartyId).ToList();

//        IList<string> aoProductsAvailable = null;
//        if (newProfile.UserTypeId == (int)UserRoleType.SuperUser)
//            aoProductsAvailable = await GetEditorUserAoProductAsync(
//                loggedInUserRealPageId, createUserPersonaId, oldProfile.Persona[0].Organization.RealPageId, cancellationToken);

//        var emailUsageType  = await _usageTypeRepo.ListContactMechanismUsageTypeAsync("Email Notification", cancellationToken);
//        var enterpriseRole  = newProfile.productBatch.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedUI);
//        int enterpriseRoleId = newProfile.RoleTemplateId;
//        if (enterpriseRole?.InputJson?.RoleList?.Count > 0)
//            enterpriseRoleId = Convert.ToInt32(enterpriseRole.InputJson.RoleList.First());

//        var userDetails     = await _userRepo.GetUserDetailsAsync(oldProfile.Persona[0].PersonaId, cancellationToken: cancellationToken);
//        var productBatchData = (newProfile.userLogin.Status == UserUiStatusType.Disabled && enterpriseRoleId == 0)
//            ? await GetActivatedUserProductBatchDataAsync(oldProfile.Persona[0].PersonaId, newProfile.productBatch, cancellationToken)
//            : newProfile.productBatch;

//        var primaryOrg  = await _orgRepo.GetOrganizationAsync(realPageId: currentPrimaryStatus.RealPageId);
//        var currentOrg  = await _orgRepo.GetOrganizationAsync(organizationPartyId: oldProfile.Persona[0].OrganizationPartyId);
//        bool isCurrentOrgPrimary = primaryOrg.PartyId == userClaims.OrganizationPartyId;

//        // Primary property handling
//        ProductBatch primaryPropertyBatch = null;
//        if (currentOrg.EnablePrimaryProperties == 1)
//            primaryPropertyBatch = newProfile.productBatch.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedPlatform);

//        // Editor/assigned persona list for profile update batch
//        var editorAssignedList = new List<EditorAssignedPersona>();
//        foreach (var o in (await _userLoginRepo.ListOrganizationByLoginNameAsync(loginOnly.LoginName)))
//        {
//            if (o.BooksCustomerMasterId == -1 || o.BooksMasterId == -1) continue;
//            Guid adminId       = await _orgRepo.GetOrganizationAdminUserRealPageIdAsync(o.OrganizationRealPageId);
//            var  editorPersona = _managePersona.GetFirstAvailablePersonaByCompany(adminId, o.OrganizationPartyId);
//            var  assignPersona = _managePersona.GetFirstAvailablePersonaByCompany(newProfile.RealPageId, o.OrganizationPartyId);
//            editorAssignedList.Add(new EditorAssignedPersona
//            {
//                AssignedPersonaId       = assignPersona.PersonaId,
//                AssignedUserTypeId      = assignPersona.UserTypeId ?? 0,
//                EditorPersonaId         = editorPersona.PersonaId,
//                EditorPersonaRealPageId = editorPersona.RealPageId,
//                OrganizationRealPageId  = o.OrganizationRealPageId
//            });
//        }

//        // Primary properties
//        if (primaryPropertyBatch is not null && enterpriseRoleId == 0)
//        {
//            var ulInstances = await _propertyRepo.ListUPFMPropertyInstanceByPersonaAsync(oldProfile.Persona[0].PersonaId, ProductEnum.UnifiedPlatform, cancellationToken);
//            var currentProps = ulInstances.Select(p => Convert.ToString(p.InstanceId)).ToList();
//            if (currentProps.Count > 0 && primaryPropertyBatch.InputJson.PropertyList?.Count > 0)
//                productBatchData = await UpdateProductBatchDataWithPrimaryPropertiesAsync(
//                    createUserPersonaId, oldProfile.Persona[0].PersonaId, primaryPropertyBatch, productBatchData.ToList(), currentProps, cancellationToken);
//        }

//        IUserLoginOnly impersonatorLogin = new UserLoginOnly();
//        if (userClaims.ImpersonatedBy != Guid.Empty)
//            impersonatorLogin = await _userLoginRepo.GetUserLoginOnlyAsync(userClaims.ImpersonatedBy);

//        var unifiedSettings = CreateUnifiedSettings();
//        var companyProductSettings = unifiedSettings.GetUnifiedSettings(userClaims.OrganizationPartyId, "company");
//        int orgUsePrimaryProps = 0;
//        if (companyProductSettings.Any(a => a.Name.Equals("PrimaryProperty", StringComparison.OrdinalIgnoreCase)))
//            int.TryParse(companyProductSettings.First(a => a.Name.Equals("PrimaryProperty", StringComparison.OrdinalIgnoreCase)).Value, out orgUsePrimaryProps);

//        var platformSettings = await _internalSettingRepo.GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, cancellationToken);
//        var platformAdminRole = platformSettings.FirstOrDefault(s => s.Name.Equals("PlatformAdminRole", StringComparison.OrdinalIgnoreCase))?.Value;

//        var companyList  = await _orgRepo.GetCompanyListAsync(null, 0, null, (int)userClaims.OrganizationPartyId, new RequestParameter(), cancellationToken);
//        bool isRPAccess  = companyList.Any(a => a.RealPageAccessUser == userClaims.LoginName);
//        bool isDelegateEnabled = await GetUnifiedSettingDataAsync("delegateadministrators", cancellationToken);
//        bool deleteOldPropInstanceMapping = false;
//        bool externalUserRelUpdated = IsExternalUserRelationUpdated(newProfile, oldProfile, out deleteOldPropInstanceMapping);

//        var entity = new UpdateUserProfileEntity
//        {
//            LoggedInUserRealPageId      = loggedInUserRealPageId,
//            NewProfile                  = newProfile,
//            OldProfile                  = oldProfile,
//            CreateUserPersonaId         = createUserPersonaId,
//            SaveProductBatchError       = "Save Product(s) Error: ",
//            IsCurrentOrgThePrimaryOrg   = isCurrentOrgPrimary,
//            IdentityProviderTypeList    = idpList,
//            ProductBatchData            = productBatchData,
//            EmailUsageType              = emailUsageType,
//            OrganizationExternalUser    = orgExternalUser,
//            UserLoginOnly               = loginOnly,
//            UserPersonaOrganizationList = userOrgList,
//            ExistingRoleIds             = existingRoleIds,
//            CurrentPrimaryOrgStatus     = currentPrimaryStatus,
//            CurrentOrgStatus            = currentOrgStatus,
//            PersonaList                 = personaList,
//            AoProductsAvailableForUser  = aoProductsAvailable,
//            UserIsExternalEverywhere    = userIsExternal,
//            CurrentOrg                  = currentOrg,
//            EditorAssignedPersonaList   = editorAssignedList
//        };

//        return await UpdateUserDataAsync(
//            entity, impersonatorLogin, orgUsePrimaryProps,
//            platformAdminRole, isRPAccess, isDelegateEnabled,
//            deleteOldPropInstanceMapping, externalUserRelUpdated,
//            primaryPropertyBatch, cancellationToken);
//    }

//    #endregion

//    #region InsertNewPhoneNumberFromImportAsync

//    /// <inheritdoc/>
//    public async Task InsertNewPhoneNumberFromImportAsync(
//        IProfileDetail profile, IDbTransaction tx,
//        CancellationToken cancellationToken = default)
//    {
//        DateTime utcNow = DateTime.UtcNow, utcMax = DateTime.MaxValue.ToUniversalTime();
//        var userClaims = _userClaimAccessor.Current;
//        long personaId = profile.Persona[0].PersonaId;
//        var toUserLogInfo = await GetUserActivityLogInfoAsync(tx, personaId, profile.RealPageId, cancellationToken);
//        UserDetails impersonatorInfo = userClaims.ImpersonatedBy != Guid.Empty
//            ? await _userRepo.GetUserDetailsAsync(null, userClaims.ImpersonatedBy.ToString(), cancellationToken) : null;

//        var telecoms = (await _db.QueryAsync<TelecommunicationNumber>(new CommandDefinition(
//            StoredProcNameConstants.SP_ListTelecommunicationNumbersForPerson, new { profile.RealPageId },
//            transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();

//        string insertPhone = profile.TelecommunicationNumber[0].AreaCode + profile.TelecommunicationNumber[0].PhoneNumber;
//        var existingNumbers = telecoms.Select(m => m.AreaCode + m.PhoneNumber).ToList();
//        TelecommunicationNumber oldDefault = telecoms.FirstOrDefault(t => t.IsDefault);
//        if (oldDefault is not null) oldDefault = ClonePhone(oldDefault);
//        telecoms.ForEach(t => t.IsDefault = false);

//        if (existingNumbers.Contains(insertPhone)) return;

//        var allPhones = profile.TelecommunicationNumber.Concat(telecoms).ToList();
//        var usageTypes = (await _db.QueryAsync<ContactMechanismUsageType>(new CommandDefinition(
//            StoredProcNameConstants.SP_ListContactMechanismUsageType, new { ContactMechanismUsageTypeName = "phone type" },
//            transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();

//        foreach (ITelecommunicationNumber phone in allPhones)
//        {
//            if (phone.ContactMechanismId == 0 && !string.IsNullOrEmpty(phone.PhoneNumber))
//            {
//                var phoneType = profile.TelecommunicationNumber.FirstOrDefault(t => t.ContactMechanismId == phone.ContactMechanismId);
//                string typeName = usageTypes.Where(r => r.ContactMechanismUsageTypeId == phoneType?.contactMechanismUsageType.ContactMechanismUsageTypeId).Select(r => r.Name).FirstOrDefault();
//                AuditActivityLog($"{phone.ISOCode}({phone.CountryCode}) {phone.PhoneNumber},{typeName}", " ", "Added Phone Number", toUserLogInfo, impersonatorInfo);
//            }

//            if (phone.ContactMechanismId == 0 && phone.PhoneNumber?.Trim().Length > 0 && phone.contactMechanismUsageType.ContactMechanismUsageTypeId > 0)
//            {
//                var cmRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreateContactMechanism, tx, new { ContactMechanismId = 0 }, cancellationToken);
//                if (cmRr.Id == 0) { continue; }
//                phone.ContactMechanismId = (int)cmRr.Id;
//                var linkRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_LinkContactMechanismToParty, tx,
//                    new { RealPageId = profile.RealPageId, PartyContactMechanismId = 0L, ContactMechanismId = phone.ContactMechanismId, FromDate = utcNow, ThruDate = utcMax }, cancellationToken);
//                if (linkRr.Id == 0) continue;
//                phone.PartyContactMechanismId = linkRr.Id;
//                await Q<RepositoryResponse>(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism, tx,
//                    new { PartyContactMechanismId = linkRr.Id, ContactMechanismUsageTypeId = phone.contactMechanismUsageType.ContactMechanismUsageTypeId }, cancellationToken);
//            }

//            if (phone.ContactMechanismId > 0 && phone.PhoneNumber?.Trim().Length > 0)
//            {
//                await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreateTelecommunicationNumber, tx,
//                    new
//                    {
//                        ContactMechanismId = phone.ContactMechanismId,
//                        AreaCode           = phone.AreaCode,
//                        CountryCode        = string.IsNullOrWhiteSpace(phone.CountryCode) ? "+1" : phone.CountryCode,
//                        PhoneNumber        = phone.PhoneNumber,
//                        ISOCode            = string.IsNullOrWhiteSpace(phone.ISOCode) ? "US" : phone.ISOCode,
//                        Default            = phone.IsDefault
//                    }, cancellationToken);

//                if (telecoms.Any() && profile.TelecommunicationNumber.Any() && oldDefault?.ContactMechanismId == phone.ContactMechanismId)
//                {
//                    var newDefault = profile.TelecommunicationNumber.FirstOrDefault(t => t.IsDefault);
//                    if (oldDefault is not null && newDefault is not null && oldDefault.PhoneNumber != newDefault.PhoneNumber)
//                    {
//                        string oldType = usageTypes.Where(r => r.ContactMechanismUsageTypeId == oldDefault.ContactMechanismUsageTypeId).Select(r => r.Name).FirstOrDefault();
//                        string newType = usageTypes.Where(r => r.ContactMechanismUsageTypeId == newDefault.contactMechanismUsageType.ContactMechanismUsageTypeId).Select(r => r.Name).FirstOrDefault();
//                        AuditActivityLog($"{oldDefault.ISOCode}({oldDefault.CountryCode}) {oldDefault.PhoneNumber},{oldType}", $"{newDefault.ISOCode}({newDefault.CountryCode}) {newDefault.PhoneNumber},{newType}", "Default Phone Number", toUserLogInfo, impersonatorInfo);
//                    }
//                }
//            }
//        }
//    }

//    #endregion

//    // ════════════════════════════════════════════════════════════════════════
//    // Private — UpdateUserData (the largest orchestration helper)
//    // ════════════════════════════════════════════════════════════════════════

//    #region UpdateUserDataAsync

//    private async Task<RepositoryResponse> UpdateUserDataAsync(
//        UpdateUserProfileEntity e,
//        IUserLoginOnly impersonatorLogin,
//        int orgUsePrimaryProps,
//        string platformAdminRole,
//        bool isRPAccess,
//        bool isDelegateAdmin,
//        bool deleteOldPropInstanceMapping,
//        bool externalUserRelUpdated,
//        ProductBatch primaryPropertyBatch,
//        CancellationToken cancellationToken)
//    {
//        var userClaims = _userClaimAccessor.Current;
//        var response = new RepositoryResponse();
//        bool committed = false;
//        bool isPrimaryPropertiesUpdated = false, isEnterpriseRolesUpdated = false, isEnterpriseRoleUnassigned = false;
//        string enterpriseUserRoleUpdated = "", enterpriseRoleUnassigned = "";
//        var greenBookRoles = new List<int>();
//        var gbProdBatch    = new ProductBatch();

//        bool usePropertyInstances = await GetPropertyInstanceUnifiedLoginAsync(cancellationToken);
//        var  userBatchEntity      = GetUserBatch(e.NewProfile, e.OldProfile, e.UserIsExternalEverywhere);

//        bool profileChanged    = IsUserProfileChanged(e.NewProfile, e.OldProfile);
//        bool loginNameChanged  = IsUserLoginNameChanged(e.NewProfile, e.OldProfile);
//        bool employeeIdChanged = IsEmployeeIdChanged(e.NewProfile, e.OldProfile);
//        bool supervisorChanged = IsSupervisorIdChanged(e.NewProfile, e.OldProfile);

//        OpenIfClosed();
//        using var tx = _db.BeginTransaction();

//        try
//        {
//           response.Id = e.OldProfile.Persona[0].PersonPartyId;

//            var ulpList = (await _db.QueryAsync<UserLoginPersona>(new CommandDefinition(
//                StoredProcNameConstants.SP_GetUserLoginPersona,
//                new { UserLoginId = e.NewProfile.userLogin.UserId, OrganizationPartyId = e.OldProfile.Persona[0].OrganizationPartyId },
//                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();

//            var enterpriseRoles = (await _db.QueryAsync<EnterpriseRole>(new CommandDefinition(
//                StoredProcNameConstants.SP_SecurityListRolesByRealPageID,
//                new { realPageId = e.OldProfile.Persona[0].Organization.RealPageId },
//                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();

//            // Update Person
//            if (profileChanged && (e.IsCurrentOrgThePrimaryOrg || userBatchEntity.UserTypeChangedToFromExternal.Equals("FromExternal", StringComparison.OrdinalIgnoreCase)))
//            {
//                var pRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePerson, tx,
//                    new { RealPageId = e.NewProfile.RealPageId, e.NewProfile.FirstName, e.NewProfile.MiddleName, e.NewProfile.LastName }, cancellationToken);
//                if (pRr.Id == 0) throw new InvalidOperationException("Update User Error: Update person failed.");
//            }

//            // Identity provider
//            var idpt = e.IdentityProviderTypeList.FirstOrDefault(a => a.IsLocal == !e.NewProfile.userLogin.Is3rdPartyIDP)
//                ?? e.IdentityProviderTypeList.FirstOrDefault();

//            if (e.IsCurrentOrgThePrimaryOrg || userBatchEntity.UserTypeChangedToFromExternal.Equals("FromExternal", StringComparison.OrdinalIgnoreCase))
//            {
//                var idpRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_LinkIdentityProviderToUserLogin, tx,
//                    new { UserId = e.NewProfile.userLogin.UserId, ContactMechanismID = idpt?.ContactMechanismId }, cancellationToken);
//                if (idpRr.Id == 0) throw new InvalidOperationException("Update User Error: Link Identity Provider to UserLogin failed.");
//            }

//            // RealPartner update
//            if (e.OldProfile.IsRealPartner != e.NewProfile.IsRealPartner && userClaims.ImpersonatedByName is not null)
//            {
//                var rpRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserLoginPersona, tx,
//                    new { UserLoginId = e.NewProfile.userLogin.UserId, StatusTypeId = e.CurrentPrimaryOrgStatus.StatusTypeId, OrganizationPartyId = e.OldProfile.Persona[0].OrganizationPartyId, Primaryorganization = true, StatusThruDate = e.CurrentPrimaryOrgStatus.StatusThruDate, IsRealPartner = e.NewProfile.IsRealPartner }, cancellationToken);
//                if (rpRr.Id == 0) throw new InvalidOperationException("Update User Error: Update Realpartner failed.");
//            }

//            // Update UserLogin
//            if (e.NewProfile.userLogin is not null)
//            {
//                bool isFeatureUser = e.NewProfile.userLogin.FromDate.Value.Date > DateTime.Now.Date;
//                if (e.NewProfile.userLogin.ThruDate is null) e.NewProfile.userLogin.ThruDate = new DateTime(9999, 12, 31);

//                var ulRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserLogin, tx,
//                    new { RealPageId = e.NewProfile.RealPageId, LoginName = e.IsCurrentOrgThePrimaryOrg || userBatchEntity.UserTypeChangedToFromExternal.Equals("FromExternal", StringComparison.OrdinalIgnoreCase) ? e.NewProfile.userLogin.LoginName : e.UserLoginOnly.LoginName, FromDate = e.NewProfile.userLogin.FromDate.Value, ThruDate = e.NewProfile.userLogin.ThruDate, PartyId = e.OldProfile.Persona[0].OrganizationPartyId }, cancellationToken);
//                if (ulRr.Id == 0) throw new InvalidOperationException("Update User Error: Update user login detail failed.");

//                // User type change to/from external
//                string extErr = await ChangeUserTypeExternalAsync(tx, e.OrganizationExternalUser, e.CurrentPrimaryOrgStatus, e.NewProfile, e.OldProfile.Persona[0], e.UserPersonaOrganizationList, e.EmailUsageType, e.UserLoginOnly, idpt, userBatchEntity.UserTypeChangedToFromExternal, cancellationToken);
//                if (!string.IsNullOrEmpty(extErr)) throw new InvalidOperationException(extErr);

//                // Custom fields
//                if (e.NewProfile.CustomFields?.Count > 0)
//                {
//                    e.NewProfile.CustomFields.ToList().ForEach(c => { if (c.UserLoginPersonaId == 0) c.UserLoginPersonaId = ulpList[0].UserLoginPersonaId; });
//                    string cfJson = JsonConvert.SerializeObject(e.NewProfile.CustomFields);
//                    if (ValidateJson.IsValidJson<IList<CustomFieldValue>>(cfJson))
//                    {
//                        var cfRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_AddUpdateFieldValue, tx, new { JSON = cfJson, CreatedBy = userClaims.UserId }, cancellationToken);
//                        if (cfRr.Id == 0 && !string.IsNullOrWhiteSpace(cfRr.ErrorMessage))
//                            throw new InvalidOperationException($"Update User Error: Update custom fields {cfRr.ErrorMessage}.");
//                    }
//                }

//                // Status change
//                bool idpChanged    = e.NewProfile.userLogin.Is3rdPartyIDP != e.UserLoginOnly.Is3rdPartyIDP;
//                bool futureDate    = e.NewProfile.userLogin.FromDate.Value > DateTime.UtcNow;
//                bool statusChanged = e.NewProfile.userLogin.IsActive != e.CurrentOrgStatus.IsActive;
//                if (statusChanged || idpChanged || futureDate)
//                {
//                    var newStatusType = ResolveUpdateUserStatus(e, idpt, futureDate);
//                    if (newStatusType != UserUiStatusType.UnDefined)
//                    {
//                        DateTime? statusThru = null;
//                        if (newStatusType == UserUiStatusType.Pending)
//                        {
//                            var acts = (await _db.QueryAsync<Activity>(new CommandDefinition(StoredProcNameConstants.SP_ListActivity, new { PartyId = userClaims.OrganizationPartyId }, transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();
//                            var act  = acts.FirstOrDefault(x => x.ActivityTypeId == (int)ActivityType.NewUserRegistration);
//                            statusThru = act is not null ? e.NewProfile.userLogin.FromDate.Value.AddMinutes(act.ActivityTokenExpirationMinutes) : e.NewProfile.userLogin.FromDate.Value.Date.AddHours(72);
//                        }
//                        var stRr = await _db.ExecuteAsync(new CommandDefinition(StoredProcNameConstants.SP_UpdateUserStatusByCompany,
//                            new { RealPageId = e.NewProfile.RealPageId, OrganizationPartyId = e.OldProfile.Persona[0].OrganizationPartyId, StatusTypeId = newStatusType, FromDate = e.NewProfile.userLogin.FromDate, StatusThruDate = statusThru },
//                            transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
//                        if (stRr == 0) throw new InvalidOperationException("Update User Error: Update user status failed.");
//                    }
//                }
//            }

//            // Update Persona
//            if (userBatchEntity.UserTypeChanged)
//            {
//                int personaTypeId = userBatchEntity.BatchProcessUserType is (int)BatchProcessType.UserTypeAdminToRegular or (int)BatchProcessType.UserTypeAdminToExternal
//                    ? (int)PersonaType.Primary : (int)PersonaType.SuperUser;
//                var pRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePersona, tx,
//                    new { PersonaId = e.OldProfile.Persona[0].PersonaId, PersonaTypeId = personaTypeId, PersonaEnvironmentTypeId = e.OldProfile.Persona[0].PersonaEnvironmentTypeId }, cancellationToken);
//                if (pRr.Id == 0) throw new InvalidOperationException("Persona name was not associated to the Persona.");
//            }

//            // Update User Type
//            var relType = await Q<PartyRelationship>(StoredProcNameConstants.SP_GetPartyRelationshipByRealPageId, tx,
//                new { RealPageIdFrom = e.NewProfile.RealPageId, RealPageIdTo = e.OldProfile.Persona[0].Organization.RealPageId, RoleTypeName = (string)null, RelationshipTypeName = "User Type" }, cancellationToken);
//            if (relType is not null && relType.RoleTypeIdFrom != e.NewProfile.UserTypeId)
//            {
//                var utRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePersonToOrganization, tx,
//                    new { PersonRealPageId = e.NewProfile.RealPageId, OrganizationRealPageId = e.OldProfile.Persona[0].Organization.RealPageId, UnlinkRoleTypeIdFrom = relType.RoleTypeIdFrom, LinkRoleTypeIdFrom = e.NewProfile.UserTypeId, RoleTypeIdTo = relType.RoleTypeIdTo }, cancellationToken);
//                if (utRr.Id == 0) throw new InvalidOperationException("Update User Error: Unable to set new user type.");
//            }

//            // Notification email
//            await UpdateNotificationEmailAsync(tx, e, idpt, cancellationToken);

//            // From import phone numbers
//            if (e.NewProfile.IsFromImport && e.NewProfile.TelecommunicationNumber?.Count > 0)
//                await InsertNewPhoneNumberFromImportAsync(e.NewProfile, tx, cancellationToken);

//            // EmployeeId
//            if (employeeIdChanged)
//            {
//                if (e.OldProfile.UserEmployeeId > 0)
//                {
//                    var eRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_UpdateEmployeeId, tx, new { e.OldProfile.UserEmployeeId, e.NewProfile.EmployeeId }, cancellationToken);
//                    if (eRr.Id == 0) throw new InvalidOperationException("An error was encountered when updating an user employee.");
//                }
//                else
//                {
//                    var eRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreateEmployeeId, tx, new { ulpList[0].UserLoginPersonaId, e.NewProfile.EmployeeId }, cancellationToken);
//                    if (eRr.Id == 0) throw new InvalidOperationException("An error was encountered when updating an user employee.");
//                }
//            }

//            // Supervisor
//            if (supervisorChanged)
//            {
//                var svRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_InsertUpdateSuperVisor, tx, new { UserId = e.NewProfile.userLogin.UserId, SuperVisorUserId = e.NewProfile.SuperVisorUserId }, cancellationToken);
//                if (svRr.Id == 0) throw new InvalidOperationException("An error was encountered when updating supervisor for an user employee.");
//                var svInfo = await Q<UserInfoLite>(StoredProcNameConstants.SP_GetSuperVisorId, tx, new { UserId = e.NewProfile.userLogin.UserId, OrgPartyId = userClaims.OrganizationPartyId }, cancellationToken);
//                string uName = string.IsNullOrEmpty(userClaims.ImpersonatedByName) ? $"{userClaims.FirstName} {userClaims.LastName}" : $"RealPage Access ({userClaims.ImpersonatedByName}) ";
//                if (svInfo is not null)
//                    AuditActivityLog(e.OldProfile.SuperVisorUser?.LoginName ?? "", svInfo.LoginName, "Supervisor", $"{uName} updated supervisor for {e.NewProfile.FirstName} {e.NewProfile.LastName}. Set to {svInfo.FirstName} {svInfo.LastName}({svInfo.LoginName}).", e.NewProfile);
//                else if (e.NewProfile.SuperVisorUserId == 0 && e.NewProfile.SuperVisorUser?.SuperVisorUserId > 0)
//                    AuditActivityLog(e.OldProfile.SuperVisorUser?.LoginName ?? "", " ", "Supervisor", $"{uName} Deleted supervisor for {e.NewProfile.FirstName} {e.NewProfile.LastName}.", e.NewProfile);
//            }

//            // Delegate admin
//            if (isDelegateAdmin && (e.NewProfile.IsDelegateAdmin || e.OldProfile.IsDelegateAdmin))
//            {
//                if (e.NewProfile.IsDelegateAdmin != e.OldProfile.IsDelegateAdmin)
//                    await Q<RepositoryResponse>(StoredProcNameConstants.SP_UpdateDelegateAdminStatus, tx, new { IsDelegateAdmin = e.NewProfile.IsDelegateAdmin, ulpList[0].UserLoginPersonaId }, cancellationToken);
//                var daRr = await InsertUpdateDelegateAdminRoleAsync(tx, ulpList[0].UserLoginPersonaId, e.NewProfile.DelegateRoleTemplate.RoleTemplateId.ToList(), cancellationToken);
//                if (!string.IsNullOrEmpty(daRr.ErrorMessage)) throw new InvalidOperationException("Unable to Create Delegate Template role.");
//            }

//            // External user company association
//            if (FeatureFlag.GetUserCompanyAssociationFeatureFlag() && externalUserRelUpdated)
//            {
//                var euRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_UpdateExternalUserRelationship, tx,
//                                        new
//                                        {
//                                            UserLoginPersonaId = e.NewProfile.ExternalUserRelationship.UserLoginPersonaId,
//                                            ThirdPartyRelationshipId = e.NewProfile.ExternalUserRelationship.ThirdPartyRelationShipId,
//                                            CompanyName = e.NewProfile.ExternalUserRelationship.ThirdPartyCompanyName,
//                                            ThirdPartyCompanyRealPageId = e.NewProfile.ExternalUserRelationship.ThirdPartyCompanyRealPageId,
//                                            OperatorCode = e.NewProfile.ExternalUserRelationship.OperatorCode,
//                                            OperatorValue = e.NewProfile.ExternalUserRelationship.OperatorValue
//                                        }, cancellationToken);
//                if (euRr?.Id == 0) throw new InvalidOperationException("Update ExternalUser Relationship failed.");
//            }

//            // Delete old property instance mapping when switching operator
//            if (deleteOldPropInstanceMapping)
//            {
//                await Q<RepositoryResponse>(StoredProcNameConstants.SP_DeletePropertyInstanceMappingByPersonaId, tx,
//                    new { PersonaId = e.OldProfile.Persona[0].PersonaId, ProductId = (int)ProductEnum.UnifiedPlatform }, cancellationToken);
//            }

//            // ── GB property mapping (UnifiedPlatform) ────────────────────────
//            gbProdBatch = e.NewProfile.productBatch.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedPlatform)
//                          ?? new ProductBatch { InputJson = new RolePropertyList() };

//            if (gbProdBatch.InputJson.PropertyList?.Count > 0 || gbProdBatch.InputJson.RemovedPropertyList?.Count > 0)
//            {
//                string propJson = JsonConvert.SerializeObject(gbProdBatch);
//                string propSp = usePropertyInstances
//                    ? StoredProcNameConstants.SP_AddUpdatePropertyInstanceMapping
//                    : StoredProcNameConstants.SP_AddUpdatePropertyMapping;
//                object propParam = usePropertyInstances
//                    ? (object)new { PersonaId = e.OldProfile.Persona[0].PersonaId, ProductId = (int)ProductEnum.UnifiedPlatform, PropertyInstanceJSON = propJson }
//                    : new { PersonaId = e.OldProfile.Persona[0].PersonaId, ProductId = (int)ProductEnum.UnifiedPlatform, PropertyJSON = propJson };

//                var propRr = await Q<RepositoryResponse>(propSp, tx, propParam, cancellationToken);
//                if (propRr.Id == 0 && !string.IsNullOrWhiteSpace(propRr.ErrorMessage))
//                    throw new InvalidOperationException($"Update User Error: GB property mapping failed: {propRr.ErrorMessage}");
//                isPrimaryPropertiesUpdated = true;
//            }

//            // ── GB role update ───────────────────────────────────────────────
//            if (gbProdBatch.InputJson.RoleList?.Count > 0)
//            {
//                greenBookRoles = GetGreenBookRoles(gbProdBatch);
//                if (greenBookRoles.Count > 0)
//                    await UpdateGreenBookRoleAsync(
//                        tx, greenBookRoles, e.OldProfile.Persona[0].PersonaId,
//                        e.ExistingRoleIds.ToList(), e.NewProfile.userLogin.UserId, cancellationToken);
//            }

//            // ── Enterprise role update / unassign ────────────────────────────
//            var entRoleBatch = e.NewProfile.productBatch.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedUI);
//            if (entRoleBatch?.InputJson?.RoleList?.Count > 0)
//            {
//                int roleTemplateId = Convert.ToInt32(entRoleBatch.InputJson.RoleList.First());
//                if (roleTemplateId == 0)
//                {
//                    var unassignRr = await UnassignEnterpriseRoleFromUserAsync(
//                        tx, e.OldProfile.Persona[0].PersonaId, cancellationToken);
//                    if (unassignRr.Id == 0 && !string.IsNullOrWhiteSpace(unassignRr.ErrorMessage))
//                        throw new InvalidOperationException($"Unassign Enterprise Role failed: {unassignRr.ErrorMessage}");
//                    isEnterpriseRoleUnassigned = true;
//                    enterpriseRoleUnassigned = entRoleBatch.InputJson.RoleList.First();
//                }
//                else
//                {
//                    var erRr = await InsertUpdateEnterpriseRoleToUserAsync(
//                        tx, roleTemplateId, e.OldProfile.Persona[0].PersonaId, cancellationToken);
//                    if (erRr.Id == 0) throw new InvalidOperationException("Update Enterprise Role failed.");
//                    isEnterpriseRolesUpdated = true;
//                    enterpriseUserRoleUpdated = entRoleBatch.InputJson.RoleList.First();
//                }
//            }

//            // ── VMP vendor admin roles ───────────────────────────────────────
//            var vmpRoles = await GetVMPVendorAdminRolesAsync(tx, e.OldProfile.Persona[0].Organization?.RealPageId, cancellationToken);

//            // ── Save product details for every org the user belongs to ───────
//            foreach (var editorAssigned in e.EditorAssignedPersonaList)
//            {
//                await SaveProductDetailsAsync(
//                    tx, e.ProductBatchData, null,
//                    editorAssigned.EditorPersonaId, editorAssigned.AssignedPersonaId,
//                    editorAssigned.EditorPersonaRealPageId, editorAssigned.OrganizationRealPageId,
//                    new Status<IErrorData>(), editorAssigned.AssignedUserTypeId,
//                    e.CurrentOrgStatus.IsActive, impersonatorLogin.UserId,
//                    e.AoProductsAvailableForUser, false, false,
//                    0, "update", isRPAccess, e.NewProfile.RoleIdList, cancellationToken);
//            }

//            tx.Commit();
//            committed = true;

//            // ── Post-commit: audit / activity logging (never roll back) ───────
//            AuditUserUpdate(e.OldProfile, e.NewProfile);
//            if (isEnterpriseRolesUpdated)
//                LogAuditActivity(LogActivityTypeConstants.ENTERPRISE_ROLES, LogActivityCategoryType.ProductAccess,
//                    $"Enterprise role {enterpriseUserRoleUpdated} assigned.", "EnterpriseRole", e.NewProfile);
//            if (isEnterpriseRoleUnassigned)
//                LogAuditActivity(LogActivityTypeConstants.ENTERPRISE_ROLES, LogActivityCategoryType.ProductAccess,
//                    $"Enterprise role {enterpriseRoleUnassigned} unassigned.", "EnterpriseRoleUnassign", e.NewProfile);
//            if (isPrimaryPropertiesUpdated)
//                LogAuditActivity(LogActivityTypeConstants.UPDATE_USER, LogActivityCategoryType.User,
//                    "Primary properties updated.", "PrimaryProperties", e.NewProfile);
//        }
//        catch (Exception ex)
//        {
//            if (!committed) tx.Rollback();
//            _logger.LogError(ex, "UpdateUserDataAsync failed for RealPageId={Id}", e.NewProfile.RealPageId);
//            response.Id = 0;
//            response.ErrorMessage = $"Update User Error: {ex.Message}";
//        }

//        return response;
//    }

//    #endregion

//    // ════════════════════════════════════════════════════════════════════════
//    // Private — Dapper + transaction micro-helpers
//    // ════════════════════════════════════════════════════════════════════════

//    #region Q<T> / OpenIfClosed / RollbackError / ClonePhone

//    /// <summary>
//    /// Thin Dapper wrapper: executes <paramref name="sp"/> within <paramref name="tx"/>
//    /// and returns a single row (or <c>new T()</c> on NULL).
//    /// Replaces every <c>repository.GetOne&lt;T&gt;(sp, param)</c> call in the original.
//    /// </summary>
//    private async Task<T> Q<T>(string sp, IDbTransaction tx, object param, CancellationToken ct)
//        where T : new()
//    {
//        return await _db.QuerySingleOrDefaultAsync<T>(
//            new CommandDefinition(sp, param,
//                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))
//            ?? new T();
//    }

//    private void OpenIfClosed()
//    {
//        if (_db.State != ConnectionState.Open)
//            _db.Open();
//    }

//    private static CreateUserResponse<IErrorData> RollbackError(
//        IDbTransaction tx, Status<IErrorData> status,
//        string code, string message, CreateUserResponse<IErrorData> _)
//    {
//        tx.Rollback();
//        status.Success = false;
//        status.ErrorCode = code;
//        status.ErrorMsg = message;
//        return new CreateUserResponse<IErrorData> { Status = status, UserStatus = message };
//    }

//    private static TelecommunicationNumber ClonePhone(TelecommunicationNumber p) => new()
//    {
//        ContactMechanismId = p.ContactMechanismId,
//        PhoneNumber = p.PhoneNumber,
//        AreaCode = p.AreaCode,
//        CountryCode = p.CountryCode,
//        ISOCode = p.ISOCode,
//        IsDefault = p.IsDefault,
//        IsPreferred = p.IsPreferred,
//        ContactMechanismUsageTypeId = p.ContactMechanismUsageTypeId,
//        contactMechanismUsageType = p.contactMechanismUsageType,
//        PartyContactMechanismId = p.PartyContactMechanismId
//    };

//    #endregion

//    // ════════════════════════════════════════════════════════════════════════
//    // Private — batch group & product batch helpers
//    // ════════════════════════════════════════════════════════════════════════

//    #region CreateBatchProcessGroupAsync (no-tx & in-tx overloads)

//    private async Task<BatchProcessorGroup> CreateBatchProcessGroupAsync(CancellationToken ct)
//    {
//        var param = new DynamicParameters();
//        param.Add("@BatchProcessorGroupID", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
//        try
//        {
//            await _db.ExecuteAsync(new CommandDefinition(
//                StoredProcNameConstants.SP_CreateBatchProcessorGroup, param,
//                commandType: CommandType.StoredProcedure, cancellationToken: ct));
//        }
//        catch (Exception ex) { _logger.LogWarning(ex, "{M} (no-tx) failed", nameof(CreateBatchProcessGroupAsync)); }
//        return new BatchProcessorGroup { BatchProcessorGroupId = param.Get<int>("@BatchProcessorGroupID") };
//    }

//    private async Task<BatchProcessorGroup> CreateBatchProcessGroupAsync(IDbTransaction tx, CancellationToken ct)
//    {
//        var param = new DynamicParameters();
//        param.Add("@BatchProcessorGroupID", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
//        await _db.ExecuteAsync(new CommandDefinition(
//            StoredProcNameConstants.SP_CreateBatchProcessorGroup, param,
//            transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
//        return new BatchProcessorGroup { BatchProcessorGroupId = param.Get<int>("@BatchProcessorGroupID") };
//    }

//    #endregion

//    #region SaveProductBatchNoTxAsync / SaveProductBatchInTxAsync

//    /// <summary>
//    /// Queues a single product batch record without a wrapping transaction.
//    /// Used by ProcessDisableUserProductDataAsync, ActivateSalesForceUserAsync, etc.
//    /// </summary>
//    private async Task SaveProductBatchNoTxAsync(
//        IProductBatch product, Guid personRealPageId,
//        long createUserPersonaId, long assignUserPersonaId,
//        string inputJson, long impersonatorUserId,
//        int batchProcessTypeId = (int)BatchProcessType.CreateUpdateProductUser,
//        CancellationToken cancellationToken = default)
//    {
//        var batchGroup = await CreateBatchProcessGroupAsync(cancellationToken);

//        await _db.QuerySingleOrDefaultAsync<dynamic>(
//            new CommandDefinition(
//                StoredProcNameConstants.SP_CreateProductBatch,
//                new
//                {
//                    PersonRealPageId = personRealPageId,
//                    CreateUserPersonaId = createUserPersonaId,
//                    AssignUserPersonaId = assignUserPersonaId,
//                    ProductId = product.ProductId,
//                    StatusTypeId = 5,
//                    RetryCount = 0,
//                    InputJson = inputJson,
//                    CorrelationId = Guid.NewGuid().ToString(),
//                    BatchProcessTypeId = batchProcessTypeId,
//                    BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
//                    ImpersonatorUserId = impersonatorUserId,
//                    UseAPIV2 = true
//                },
//                commandType: CommandType.StoredProcedure,
//                cancellationToken: cancellationToken));
//    }

//    /// <summary>
//    /// Queues a single product batch record within an active transaction.
//    /// Used by SaveProductDetailsAsync when called inside CreateUser/UpdateUser.
//    /// </summary>
//    private async Task SaveProductBatchInTxAsync(
//        IDbTransaction tx, IProductBatch product,
//        CreateUserResponse<IErrorData>? createUserResponse,
//        Guid personRealPageId, long createUserPersonaId, long assignUserPersonaId,
//        string inputJson, long impersonatorUserId, int batchProcessTypeId,
//        CancellationToken ct)
//    {
//        var batchGroup = await CreateBatchProcessGroupAsync(tx, ct);

//        var row = await _db.QuerySingleOrDefaultAsync<dynamic>(
//            new CommandDefinition(
//                StoredProcNameConstants.SP_CreateProductBatch,
//                new
//                {
//                    PersonRealPageId = personRealPageId,
//                    CreateUserPersonaId = createUserPersonaId,
//                    AssignUserPersonaId = assignUserPersonaId,
//                    ProductId = product.ProductId,
//                    StatusTypeId = 5,
//                    RetryCount = 0,
//                    InputJson = inputJson,
//                    CorrelationId = Guid.NewGuid().ToString(),
//                    BatchProcessTypeId = batchProcessTypeId,
//                    BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
//                    ImpersonatorUserId = impersonatorUserId,
//                    UseAPIV2 = true
//                },
//                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));

//        long batchId = (long?)row?.Id ?? 0;
//        if (batchId == 0)
//            _logger.LogWarning("SP_CreateProductBatch returned 0 for ProductId={P}", product.ProductId);
//        if (createUserResponse is not null)
//            createUserResponse.ProductBatchId = batchId;
//    }

//    #endregion

//    #region SaveProductDetailsAsync

//    /// <summary>
//    /// Replaces: UserRepository.SaveProductDetails — the core product-batch orchestration loop.
//    /// Handles AO bundling, batch process type resolution, and per-product SP calls.
//    /// Returns the count of batch records created.
//    /// </summary>
//    private async Task<int> SaveProductDetailsAsync(
//        IDbTransaction? tx,
//        IList<ProductBatch> productList,
//        CreateUserResponse<IErrorData>? createUserResponse,
//        long createUserPersonaId, long assignUserPersonaId,
//        Guid personRealPageId, Guid organizationRealPageId,
//        Status<IErrorData> errorStatus, int userTypeId, bool userIsActive,
//        long impersonatorUserId,
//        IList<string>? aoProducts = null, bool migratedUser = false,
//        bool isCreateUser = false, int unifiedPlatformRole = 0,
//        string operationType = "update", bool isRealPageAccessUser = false,
//        IList<string>? roleIdList = null,
//        CancellationToken cancellationToken = default)
//    {
//        if (productList is null || productList.Count == 0) return 0;

//        int savedCount = 0;

//        // Bundle AO products into a single JSON payload
//        bool hasAo = productList.Any(p => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)p.ProductId));
//        string aoJson = hasAo ? BundleAoProducts(productList) : string.Empty;

//        // Org product list for admin enrichment
//        IList<ProductUI> orgProductList = [];
//        if (userTypeId == (int)UserRoleType.SuperUser && !isCreateUser)
//            orgProductList = await GetOrganizationProductListForAdminUserAsync(
//                tx, personRealPageId, organizationRealPageId, aoProducts?.ToList(), cancellationToken);

//        foreach (var product in productList)
//        {
//            if (product.ProductId is (int)ProductEnum.UnifiedUI) continue; // enterprise role handled separately

//            // AO sub-products — coalesced into bundle; skip individual records
//            bool isAoProduct = ProductEnumHelper.GetAoProductList().Contains((ProductEnum)product.ProductId);
//            if (isAoProduct) continue;

//            string inputJson = JsonConvert.SerializeObject(product.InputJson);

//            int batchTypeId = ResolveBatchProcessType(product, operationType, migratedUser, isCreateUser);

//            if (tx is not null)
//                await SaveProductBatchInTxAsync(tx, product, createUserResponse, personRealPageId,
//                    createUserPersonaId, assignUserPersonaId, inputJson, impersonatorUserId, batchTypeId, cancellationToken);
//            else
//                await SaveProductBatchNoTxAsync(product, personRealPageId, createUserPersonaId,
//                    assignUserPersonaId, inputJson, impersonatorUserId, batchTypeId, cancellationToken);

//            savedCount++;
//        }

//        // Save bundled AO record
//        if (hasAo && !string.IsNullOrEmpty(aoJson))
//        {
//            var aoBatchType = isCreateUser ? (int)BatchProcessType.CreateUpdateProductUser : (int)BatchProcessType.CreateUpdateProductUser;
//            var aoPb = new ProductBatch
//            {
//                ProductId = (int)ProductEnum.AssetOptimizer,
//                StatusTypeId = 5,
//                RetryCount = 0,
//                InputJson = new RolePropertyList { IsAssigned = productList.Any(p => p.InputJson.IsAssigned && ProductEnumHelper.GetAoProductList().Contains((ProductEnum)p.ProductId)) }
//            };

//            if (tx is not null)
//                await SaveProductBatchInTxAsync(tx, aoPb, createUserResponse, personRealPageId,
//                    createUserPersonaId, assignUserPersonaId, aoJson, impersonatorUserId, aoBatchType, cancellationToken);
//            else
//                await SaveProductBatchNoTxAsync(aoPb, personRealPageId, createUserPersonaId,
//                    assignUserPersonaId, aoJson, impersonatorUserId, aoBatchType, cancellationToken);

//            savedCount++;
//        }

//        return savedCount;
//    }

//    private static int ResolveBatchProcessType(ProductBatch product, string operationType, bool migratedUser, bool isCreateUser)
//    {
//        if (isCreateUser)
//            return migratedUser ? (int)BatchProcessType.MigrateUser : (int)BatchProcessType.CreateUpdateProductUser;

//        if (product.ProductId == (int)ProductEnum.KnockCRM)
//            return (int)BatchProcessType.CreateUpdateProductUser;

//        return product.InputJson.IsAssigned && operationType.Equals("update", StringComparison.OrdinalIgnoreCase)
//            ? (int)BatchProcessType.CreateUpdateProductUser
//            : (int)BatchProcessType.CreateUpdateProductUser;
//    }

//    private static string BundleAoProducts(IList<ProductBatch> productList)
//    {
//        var aoProducts = productList
//            .Where(p => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)p.ProductId))
//            .ToList();

//        return aoProducts.Count == 0
//            ? string.Empty
//            : JsonConvert.SerializeObject(new { products = aoProducts.Select(p => new { p.ProductId, p.InputJson }) });
//    }

//    #endregion

//    #region GetListOfProductsToRemoveByPersonaIdAsync

//    private async Task<List<ProductBatch>> GetListOfProductsToRemoveByPersonaIdAsync(
//        long personaId, CancellationToken ct)
//    {
//        var rows = await _db.QueryAsync<PersonaProductUserDetails>(
//            new CommandDefinition(
//                StoredProcNameConstants.SP_ListProductsByPersonaId,
//                new { PersonaId = personaId },
//                commandType: CommandType.StoredProcedure, cancellationToken: ct));

//        return rows
//            .Select(p => new ProductBatch
//            {
//                ProductId = p.ProductId,
//                StatusTypeId = 5,
//                RetryCount = 0,
//                InputJson = new RolePropertyList { PropertyList = [], RoleList = [], IsAssigned = false }
//            })
//            .ToList();
//    }

//    #endregion

//    #region GetActivatedUserProductBatchDataAsync / GetUserProductBatchDataAsync / ProcessActivatedUserProductBatchDataAsync

//    private async Task<IList<ProductBatch>> GetActivatedUserProductBatchDataAsync(
//        long personaId, IList<ProductBatch> productBatch, CancellationToken ct)
//    {
//        var result = new List<ProductBatch>(productBatch);

//        var deactivated = await _db.QueryAsync<PersonaProductUserDetails>(
//            new CommandDefinition(
//                StoredProcNameConstants.SP_ListProductsByPersonaId,
//                new { PersonaId = personaId, ProductStatusValue = ((int)UserUiStatusType.Deactivated).ToString() },
//                commandType: CommandType.StoredProcedure, cancellationToken: ct));

//        foreach (var p in deactivated)
//        {
//            if (!result.Any(r => r.ProductId == p.ProductId))
//                result.Add(new ProductBatch
//                {
//                    ProductId = p.ProductId,
//                    StatusTypeId = 5,
//                    RetryCount = 0,
//                    InputJson = new RolePropertyList { PropertyList = [], RoleList = [], IsAssigned = true }
//                });
//        }

//        return result;
//    }

//    private async Task<ProductBatch?> GetUserProductBatchDataAsync(long personaId, int productId, CancellationToken ct)
//    {
//        var row = (await _db.QueryAsync<PersonaProductUserDetails>(
//            new CommandDefinition(
//                StoredProcNameConstants.SP_ListProductsByPersonaId,
//                new { PersonaId = personaId },
//                commandType: CommandType.StoredProcedure, cancellationToken: ct)))
//            .FirstOrDefault(p => p.ProductId == productId);

//        return row is null ? null : new ProductBatch
//        {
//            ProductId = row.ProductId,
//            StatusTypeId = 5,
//            RetryCount = 0,
//            InputJson = new RolePropertyList { PropertyList = [], RoleList = [], IsAssigned = true }
//        };
//    }

//    private async Task ProcessActivatedUserProductBatchDataAsync(
//        long personaId, Guid createUserRealPageId, long createUserPersonaId, CancellationToken ct)
//    {
//        var deactivated = await _db.QueryAsync<PersonaProductUserDetails>(
//            new CommandDefinition(
//                StoredProcNameConstants.SP_ListProductsByPersonaId,
//                new { PersonaId = personaId, ProductStatusValue = ((int)UserUiStatusType.Deactivated).ToString() },
//                commandType: CommandType.StoredProcedure, cancellationToken: ct));

//        foreach (var p in deactivated)
//        {
//            var pb = new ProductBatch
//            {
//                ProductId = p.ProductId,
//                StatusTypeId = 5,
//                RetryCount = 0,
//                InputJson = new RolePropertyList { PropertyList = [], RoleList = [], IsAssigned = true }
//            };

//            await SaveProductBatchNoTxAsync(
//                pb, createUserRealPageId, createUserPersonaId, personaId,
//                JsonConvert.SerializeObject(pb.InputJson), 0,
//                cancellationToken: ct);
//        }
//    }

//    #endregion

//    // ════════════════════════════════════════════════════════════════════════
//    // Private — AO helpers
//    // ════════════════════════════════════════════════════════════════════════

//    #region GetEditorUserAoProductAsync / GetOrganizationProductListForAdminUserAsync / GetVMPVendorAdminRolesAsync

//    private async Task<IList<string>> GetEditorUserAoProductAsync(
//        Guid userRealPageGuid, long personaId, Guid organizationRealPageId,
//        CancellationToken ct)
//    {
//        try
//        {
//            // Replaces: new ManageProductAssetOptimization(_userClaim) inline
//            var aoLogic = _aoFactory.Create(_userClaimAccessor.Current);
//            return aoLogic.GetGbSupportedAoProductsWithUserAdminRole(personaId);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogWarning(ex, "{M} failed for personaId={P}", nameof(GetEditorUserAoProductAsync), personaId);
//            return [];
//        }
//    }

//    private async Task<IList<ProductUI>> GetOrganizationProductListForAdminUserAsync(
//        IDbTransaction? tx, Guid personRealPageId, Guid organizationRealPageId,
//        IList<string>? aoProducts, CancellationToken ct)
//    {
//        var products = (await _db.QueryAsync<ProductUI>(
//            new CommandDefinition(
//                StoredProcNameConstants.SP_ListProductsByOrganization,
//                new { OrganizationRealPageId = organizationRealPageId },
//                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();

//        if (aoProducts?.Count > 0)
//        {
//            var allProducts = (await _db.QueryAsync<dynamic>(
//                new CommandDefinition(
//                    StoredProcNameConstants.SP_GetAllProducts,
//                    commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();

//            foreach (var aoProductName in aoProducts)
//            {
//                var aoEnum = ProductEnumHelper.GetAoProductEnum(aoProductName);
//                var detail = allProducts.FirstOrDefault(x => x.ProductId == (int)aoEnum);
//                if (detail is not null && !products.Any(p => p.ProductId == (int)aoEnum))
//                    products.Add(new ProductUI { ProductId = (int)aoEnum, ProductName = Convert.ToString(detail.Name) });
//            }
//        }

//        return products;
//    }

//    private async Task<List<string>> GetVMPVendorAdminRolesAsync(
//        IDbTransaction? tx, Guid? organizationRealPageId, CancellationToken ct)
//    {
//        if (!organizationRealPageId.HasValue || organizationRealPageId == Guid.Empty) return [];

//        var rows = await _db.QueryAsync<dynamic>(
//            new CommandDefinition(
//                StoredProcNameConstants.SP_ListVMPVendorAdminRoles,
//                new { OrganizationRealPageId = organizationRealPageId },
//                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));

//        return rows.Select(r => Convert.ToString(r.RoleId)).ToList();
//    }

//    #endregion

//    // ════════════════════════════════════════════════════════════════════════
//    // Private — contact mechanism / profile helpers
//    // ════════════════════════════════════════════════════════════════════════

//    #region UpdateProfileAsync / UpdateUserProfileAsync / ListContactMechanismForPersonAsync / CreateNotificationEmailAsync / UpdateNotificationEmailAsync

//    /// <summary>
//    /// Replaces: UserRepository.UpdateProfile(IRepository, Guid, IProfileDetail) —
//    /// persists phone numbers for a new or existing user within a transaction.
//    /// </summary>
//    private async Task<RepositoryResponse> UpdateProfileAsync(
//        IDbTransaction tx, Guid realPageId, IProfileDetail profile, CancellationToken ct)
//    {
//        var response = new RepositoryResponse { Id = 1 };
//        DateTime utcNow = DateTime.UtcNow, utcMax = DateTime.MaxValue.ToUniversalTime();

//        if (profile.TelecommunicationNumber is null || profile.TelecommunicationNumber.Count == 0)
//            return response;

//        var usageTypes = (await _db.QueryAsync<ContactMechanismUsageType>(
//            new CommandDefinition(
//                StoredProcNameConstants.SP_ListContactMechanismUsageType,
//                new { ContactMechanismUsageTypeName = "phone type" },
//                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();

//        foreach (ITelecommunicationNumber phone in profile.TelecommunicationNumber)
//        {
//            if (phone.IsDeleted) continue;

//            if (phone.ContactMechanismId == 0
//                && phone.PhoneNumber?.Trim().Length > 0
//                && phone.contactMechanismUsageType?.ContactMechanismUsageTypeId > 0)
//            {
//                var cmRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreateContactMechanism, tx,
//                    new { ContactMechanismId = (long?)null }, ct);
//                if (cmRr.Id == 0) { response.Id = 0; response.ErrorMessage = ProfileErrorMessage; return response; }

//                phone.ContactMechanismId = (int)cmRr.Id;

//                var linkRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_LinkContactMechanismToParty, tx,
//                    new { realPageId, PartyContactMechanismId = 0L, ContactMechanismId = phone.ContactMechanismId, FromDate = utcNow, ThruDate = utcMax }, ct);
//                if (linkRr.Id == 0) { response.Id = 0; response.ErrorMessage = ProfileErrorMessage; return response; }

//                phone.PartyContactMechanismId = linkRr.Id;

//                var ltRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism, tx,
//                    new { PartyContactMechanismId = linkRr.Id, ContactMechanismUsageTypeId = phone.contactMechanismUsageType.ContactMechanismUsageTypeId }, ct);
//                if (ltRr.Id == 0) { response.Id = 0; response.ErrorMessage = ProfileLinkUsageTypeErrorMessage; return response; }
//            }

//            if (phone.ContactMechanismId > 0 && phone.PhoneNumber?.Trim().Length > 0)
//            {
//                await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreateTelecommunicationNumber, tx,
//                    new
//                    {
//                        ContactMechanismId = phone.ContactMechanismId,
//                        AreaCode = phone.AreaCode,
//                        CountryCode = string.IsNullOrWhiteSpace(phone.CountryCode) ? "+1" : phone.CountryCode,
//                        PhoneNumber = phone.PhoneNumber,
//                        ISOCode = string.IsNullOrWhiteSpace(phone.ISOCode) ? "US" : phone.ISOCode,
//                        Default = phone.IsDefault
//                    }, ct);
//            }
//        }

//        return response;
//    }

//    /// <summary>
//    /// Replaces: UserRepository.UpdateUserProfile(IRepository, Guid, Profile) —
//    /// phones + notification email used by UpdateNewUser.
//    /// </summary>
//    private async Task<RepositoryResponse> UpdateUserProfileAsync(
//        IDbTransaction tx, Guid realPageId, Profile profile, CancellationToken ct)
//    {
//        var response = await UpdateProfileAsync(tx, realPageId, profile, ct);
//        if (response.Id == 0) return response;

//        if (!string.IsNullOrEmpty(profile.NotificationEmail) && EmailFormatValidation.IsValidEmail(profile.NotificationEmail))
//        {
//            DateTime utcNow = DateTime.UtcNow, utcMax = DateTime.MaxValue.ToUniversalTime();
//            var existing = (await _db.QueryAsync<CommonAddress>(
//                new CommandDefinition(
//                    StoredProcNameConstants.SP_ListContactMechanismForPerson, new { realPageId },
//                    transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();

//            var existingEmail = existing.FirstOrDefault(a => a.contactMechanismUsageType?.ContactMechanismUsageTypeId == 301);
//            if (existingEmail is not null)
//            {
//                var rr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreateElectronicAddress, tx,
//                    new { existingEmail.ContactMechanismId, ElectronicAddressString = profile.NotificationEmail, ElectronicAddressType = "Email" }, ct);
//                if (rr.Id == 0) { response.Id = 0; response.ErrorMessage = "Email update failed."; return response; }
//            }
//            else
//            {
//                await CreateNotificationEmailAsync(tx, realPageId, profile.NotificationEmail, utcNow, utcMax, ct);
//            }
//        }

//        return response;
//    }

//    private async Task<List<CommonAddress>> ListContactMechanismForPersonAsync(
//        IDbTransaction tx, Guid realPageId,
//        IEnumerable<ContactMechanismUsageType> usageTypes, CancellationToken ct)
//    {
//        var rows = (await _db.QueryAsync<CommonAddress>(
//            new CommandDefinition(
//                StoredProcNameConstants.SP_ListContactMechanismForPerson, new { realPageId },
//                transaction: tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();

//        // Enrich each address with its usage type name
//        foreach (var addr in rows)
//        {
//            var ut = usageTypes.FirstOrDefault(u =>
//                u.ContactMechanismUsageTypeId == addr.contactMechanismUsageType?.ContactMechanismUsageTypeId);
//            if (ut is not null) addr.contactMechanismUsageType = ut;
//        }

//        return rows;
//    }

//    /// <summary>
//    /// Creates a notification email contact mechanism + electronic address from scratch within a transaction.
//    /// Extracted from CreateUser + UpdateUserListUser to avoid duplication.
//    /// </summary>
//    private async Task CreateNotificationEmailAsync(
//        IDbTransaction tx, Guid realPageId, string email,
//        DateTime utcNow, DateTime utcMax, CancellationToken ct)
//    {
//        var emailUsageTypes = await _usageTypeRepo.ListContactMechanismUsageTypeAsync("Email Notification", ct);
//        var emailCm = emailUsageTypes.FirstOrDefault(u => u.Name.Equals("Email", StringComparison.OrdinalIgnoreCase));
//        if (emailCm is null) return;

//        var cmRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreateContactMechanism, tx,
//            new { ContactMechanismId = (long?)null }, ct);
//        if (cmRr.Id == 0) return;

//        var linkRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_LinkContactMechanismToParty, tx,
//            new { realPageId, PartyContactMechanismId = 0L, ContactMechanismId = cmRr.Id, FromDate = utcNow, ThruDate = utcMax }, ct);
//        if (linkRr.Id == 0) return;

//        await Q<RepositoryResponse>(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism, tx,
//            new { PartyContactMechanismId = linkRr.Id, ContactMechanismUsageTypeId = emailCm.ContactMechanismUsageTypeId }, ct);

//        await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreateElectronicAddress, tx,
//            new { ContactMechanismId = cmRr.Id, ElectronicAddressString = email, ElectronicAddressType = "Email" }, ct);
//    }

//    /// <summary>
//    /// Replaces: inline notification-email update block inside UpdateUserData.
//    /// Only runs when the primary org is being edited and the email actually changed.
//    /// </summary>
//    private async Task UpdateNotificationEmailAsync(
//        IDbTransaction tx, UpdateUserProfileEntity e,
//        IIdentityProviderType? idpt, CancellationToken ct)
//    {
//        if (!e.IsCurrentOrgThePrimaryOrg) return;

//        bool changed = IsNotificationEmailChanged(
//            e.OldProfile.userLogin.NotificationEmail, e.NewProfile.userLogin.NotificationEmail);
//        if (!changed) return;

//        DateTime utcNow = DateTime.UtcNow, utcMax = DateTime.MaxValue.ToUniversalTime();
//        var existing = await ListContactMechanismForPersonAsync(tx, e.NewProfile.RealPageId, e.EmailUsageType, ct);
//        var existingEmail = existing.FirstOrDefault(a => a.contactMechanismUsageType?.ContactMechanismUsageTypeId == 301);

//        if (!string.IsNullOrEmpty(e.NewProfile.userLogin.NotificationEmail)
//            && EmailFormatValidation.IsValidEmail(e.NewProfile.userLogin.NotificationEmail))
//        {
//            if (existingEmail is not null)
//            {
//                await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreateElectronicAddress, tx,
//                    new { existingEmail.ContactMechanismId, ElectronicAddressString = e.NewProfile.userLogin.NotificationEmail, ElectronicAddressType = "Email" }, ct);
//            }
//            else
//            {
//                await CreateNotificationEmailAsync(tx, e.NewProfile.RealPageId, e.NewProfile.userLogin.NotificationEmail, utcNow, utcMax, ct);
//            }

//            // Queue pending email notification for local IDP
//            if (idpt?.IsLocal == true)
//            {
//                var orgCms = await ListContactMechanismForPersonAsync(
//                    tx, e.OldProfile.Persona[0].Organization.RealPageId, e.EmailUsageType, ct);
//                long orgCmId = orgCms.FirstOrDefault()?.PartyContactMechanismId ?? 0;
//                long userCmId = existingEmail?.PartyContactMechanismId ?? 0;
//                if (orgCmId > 0 && userCmId > 0)
//                    await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreateCommunicationEvent, tx,
//                        new { StatusTypeID = (int)EmailStatusType.EmailPending, FromPartyContactMechanismId = orgCmId, ToPartyContactMechanismId = userCmId, Started = utcNow, Ended = utcNow, Note = "pending", CommunicationEventID = (long?)null }, ct);
//            }
//        }
//        else if (existingEmail is not null && string.IsNullOrEmpty(e.NewProfile.userLogin.NotificationEmail))
//        {
//            // User cleared their notification email → expire the CM
//            await Q<RepositoryResponse>(StoredProcNameConstants.SP_ExpirePartyContactMechanism, tx,
//                new { PartyContactMechanismID = existingEmail.PartyContactMechanismId, RealPageId = e.NewProfile.RealPageId }, ct);
//        }
//    }

//    #endregion

//    // ════════════════════════════════════════════════════════════════════════
//    // Private — Enterprise role / delegate admin / GB role helpers
//    // ════════════════════════════════════════════════════════════════════════

//    #region InsertUpdateEnterpriseRoleToUserAsync / InsertUpdateDelegateAdminRoleAsync / UnassignEnterpriseRoleFromUserAsync

//    private async Task<RepositoryResponse> InsertUpdateEnterpriseRoleToUserAsync(
//        IDbTransaction tx, int roleTemplateId, long? personaId, CancellationToken ct)
//    {
//        var schema = await GetRoleRightsSchemaNameAsync(ct);
//        var sp = string.IsNullOrEmpty(schema)
//            ? StoredProcNameConstants.SP_InsertUpdateEnterpriseRoleToUser
//            : $"{schema}.InsertUpdateEnterpriseRoleToUser";
//        return await Q<RepositoryResponse>(sp, tx, new { RoleTemplateId = roleTemplateId, PersonaId = personaId }, ct);
//    }

//    private async Task<RepositoryResponse> InsertUpdateDelegateAdminRoleAsync(
//        IDbTransaction tx, long userLoginPersonaId, List<int> templateRoleList, CancellationToken ct)
//    {
//        return await Q<RepositoryResponse>(StoredProcNameConstants.SP_InsertUpdateDelegateAdminRole, tx,
//            new { UserLoginPersonaId = userLoginPersonaId, RoleTemplateList = JsonConvert.SerializeObject(templateRoleList) }, ct);
//    }

//    private async Task<RepositoryResponse> UnassignEnterpriseRoleFromUserAsync(
//        IDbTransaction tx, long personaId, CancellationToken ct)
//    {
//        var schema = await GetRoleRightsSchemaNameAsync(ct);
//        var sp = string.IsNullOrEmpty(schema)
//            ? StoredProcNameConstants.SP_UnassignEnterpriseRoleFromUser
//            : $"{schema}.UnassignEnterpriseRoleFromUser";
//        return await Q<RepositoryResponse>(sp, tx, new { PersonaId = personaId }, ct);
//    }

//    #endregion

//    #region UpdateGreenBookRoleAsync / GetGreenBookRoles / GetGreenBookRole / ResolveGreenBookRoleAsync / LinkPersonaToRolesAsync

//    /// <summary>
//    /// Replaces: UserRepository.UpdateGreenBookRole(IRepository, List&lt;int&gt;, long, List&lt;long&gt;, long) —
//    /// adds new roles that are not yet linked and removes roles that are no longer needed.
//    /// </summary>
//    private async Task UpdateGreenBookRoleAsync(
//        IDbTransaction tx, List<int> newRoleIds, long personaId,
//        List<long> existingRoleIds, long userId, CancellationToken ct)
//    {
//        // Add roles that are not yet linked
//        foreach (var roleId in newRoleIds.Where(r => !existingRoleIds.Contains(r)))
//        {
//            await Q<RepositoryResponse>(StoredProcNameConstants.SP_LinkPersonaToRole, tx,
//                new { personaID = personaId, roleID = roleId, CreatedBy = userId, personaPrivilgeID = 0 }, ct);
//        }

//        // Remove roles that are no longer in the new set
//        foreach (var roleId in existingRoleIds.Where(r => !newRoleIds.Contains((int)r)))
//        {
//            await Q<RepositoryResponse>(StoredProcNameConstants.SP_UnlinkPersonaFromRole, tx,
//                new { personaID = personaId, roleID = roleId }, ct);
//        }
//    }

//    private static List<int> GetGreenBookRoles(ProductBatch gbProductBatch)
//        => gbProductBatch.InputJson.RoleList?
//               .Where(r => int.TryParse(r, out _))
//               .Select(int.Parse)
//               .ToList() ?? [];

//    private static int GetGreenBookRole(ProductBatch gbProductBatch)
//        => GetGreenBookRoles(gbProductBatch).FirstOrDefault();

//    /// <summary>
//    /// Resolves the UnifiedPlatform role(s) that should be linked to the new persona.
//    /// Replaces the inline branching inside the per-org loop in CreateUser.
//    /// </summary>
//    private async Task<(int single, List<int> multi)> ResolveGreenBookRoleAsync(
//        IDbTransaction tx, long currentOrgPartyId, long orgExternalPartyId,
//        ProfileDetail newProfile, ProductBatch? gbBatch,
//        RoleType? superUserRole, string? platformAdminRole,
//        IList<EnterpriseRole> enterpriseRoles, long cloneUserPersonaId,
//        CancellationToken ct)
//    {
//        int single = 0;
//        List<int> multi = [];

//        if (currentOrgPartyId == orgExternalPartyId)
//        {
//            single = enterpriseRoles
//                .FirstOrDefault(r => r.Role.Equals("Basic End User", StringComparison.OrdinalIgnoreCase))?.RoleId ?? 0;
//        }
//        else if (superUserRole?.PartyRoleTypeId == newProfile.UserTypeId)
//        {
//            single = enterpriseRoles
//                .FirstOrDefault(r => r.Role.Equals(platformAdminRole, StringComparison.OrdinalIgnoreCase))?.RoleId ?? 0;
//        }
//        else if (gbBatch?.InputJson?.RoleList?.Count > 0)
//        {
//            multi = GetGreenBookRoles(gbBatch);
//        }
//        else if (cloneUserPersonaId > 0)
//        {
//            var row = await Q<dynamic>(StoredProcNameConstants.SP_ListRolesForProductsByPersonaId, tx,
//                new { PersonaID = cloneUserPersonaId, ProductID = (int)ProductEnum.UnifiedPlatform }, ct);
//            single = row is not null ? (int)row.RoleId : 0;
//        }
//        else
//        {
//            var defaultRow = await Q<dynamic>(StoredProcNameConstants.SP_GetUnifiedLoginDefaultRole, tx,
//                new { RealPageID = (Guid?)null }, ct);
//            single = defaultRow is not null
//                ? (int)defaultRow.RoleId
//                : enterpriseRoles.FirstOrDefault(r => r.Role.Equals("Basic End User", StringComparison.OrdinalIgnoreCase))?.RoleId ?? 0;
//        }

//        return (single, multi);
//    }

//    /// <summary>
//    /// Links the resolved UnifiedPlatform role(s) to the newly created persona.
//    /// Returns false if any SP call fails.
//    /// </summary>
//    private async Task<bool> LinkPersonaToRolesAsync(
//        IDbTransaction tx, long personaId, int greenBookRole,
//        List<int> greenBookRoles, long userId, CancellationToken ct)
//    {
//        if (greenBookRole > 0)
//        {
//            var rr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_LinkPersonaToRole, tx,
//                new { personaID = personaId, roleID = greenBookRole, CreatedBy = userId, personaPrivilgeID = 0 }, ct);
//            return rr.Id != 0;
//        }

//        foreach (var role in greenBookRoles)
//        {
//            var rr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_LinkPersonaToRole, tx,
//                new { personaID = personaId, roleID = role, CreatedBy = userId, personaPrivilgeID = 0 }, ct);
//            if (rr.Id == 0) return false;
//        }

//        return true;
//    }

//    #endregion

//    // ════════════════════════════════════════════════════════════════════════
//    // Private — user type change helpers
//    // ════════════════════════════════════════════════════════════════════════

//    #region ChangeUserTypeExternalAsync

//    /// <summary>
//    /// Replaces: UserRepository.ChangeUserTypeExternal — manages External Users org
//    /// association when a user switches to or from the External User role type.
//    /// </summary>
//    private async Task<string> ChangeUserTypeExternalAsync(
//        IDbTransaction tx, Organization orgExternalUser,
//        OrganizationStatus? currentPrimaryOrgStatus,
//        IProfileDetail newProfile, IPersona persona,
//        IList<UserOrganization> userOrgList,
//        IEnumerable<ContactMechanismUsageType> emailUsageType,
//        IUserLoginOnly userLoginOnly, IIdentityProviderType? idpt,
//        string userTypeChangedToFromExternal, CancellationToken ct)
//    {
//        if (string.IsNullOrEmpty(userTypeChangedToFromExternal)) return string.Empty;

//        DateTime utcNow = DateTime.UtcNow;

//        if (userTypeChangedToFromExternal.Equals("ToExternal", StringComparison.OrdinalIgnoreCase))
//        {
//            // First external company → add the "External Users" pseudo-org
//            if (userOrgList.Count == 1 && !userOrgList.Any(m => m.OrganizationPartyId == orgExternalUser.PartyId))
//            {
//                var ulpRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreateUserLoginPersona, tx,
//                    new
//                    {
//                        UserLoginId = newProfile.userLogin.UserId,
//                        StatusTypeId = (int)UserUiStatusType.Active,
//                        OrganizationPartyId = orgExternalUser.PartyId,
//                        PrimaryOrganization = true,
//                        FromDate = currentPrimaryOrgStatus?.FromDate ?? utcNow,
//                        ThruDate = (DateTime?)null,
//                        StatusThruDate = (DateTime?)null,
//                        newProfile.IsRPEmployee,
//                        newProfile.IsDelegateAdmin
//                    }, ct);

//                if (ulpRr.Id == 0)
//                    return "Update User Error: Failed to create UserLoginPersona for external org.";

//                var personaRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_CreatePersona, tx,
//                    new
//                    {
//                        PersonRealPageId = newProfile.RealPageId,
//                        UserLoginPersonaId = ulpRr.Id,
//                        OrganizationRealPageId = orgExternalUser.RealPageId,
//                        PersonaTypeId = (int)PersonaType.Primary,
//                        UserId = newProfile.userLogin.UserId,
//                        persona.PersonaEnvironmentTypeId,
//                        FromDate = currentPrimaryOrgStatus?.FromDate ?? utcNow,
//                        ThruDate = (DateTime?)null,
//                        personaId = (long?)null
//                    }, ct);

//                if (personaRr.Id == 0)
//                    return "Update User Error: Failed to create Persona for external org.";
//            }
//        }
//        else if (userTypeChangedToFromExternal.Equals("FromExternal", StringComparison.OrdinalIgnoreCase))
//        {
//            // Switching away from external → remove the External Users pseudo-org
//            if (userOrgList.Any(o => o.PrimaryOrganization && o.OrganizationPartyId == orgExternalUser.PartyId))
//            {
//                var unlinkRr = await Q<RepositoryResponse>(StoredProcNameConstants.SP_UnlinkPersonToOrganization, tx,
//                    new { PersonRealPageId = newProfile.RealPageId, OrganizationRealPageId = orgExternalUser.RealPageId }, ct);

//                if (unlinkRr is null || unlinkRr.Id == 0)
//                    return "Update User Error: Failed to unlink from external org.";
//            }
//        }

//        return string.Empty;
//    }

//    #endregion

//    // ════════════════════════════════════════════════════════════════════════
//    // Private — primary properties helper
//    // ════════════════════════════════════════════════════════════════════════

//    #region UpdateProductBatchDataWithPrimaryPropertiesAsync / IsProductEnabledForUsePrimaryPropertyAsync

//    private async Task<IList<ProductBatch>> UpdateProductBatchDataWithPrimaryPropertiesAsync(
//        long editorPersonaId, long userPersonaId, ProductBatch primaryPropertiesBatch,
//        List<ProductBatch> productBatch, List<string> currentPrimaryProperties,
//        CancellationToken ct)
//    {
//        var blueBook = CreateBlueBook();

//        foreach (var product in productBatch.Where(p => p.ProductId != (int)ProductEnum.UnifiedPlatform))
//        {
//            bool isEnabled = await IsProductEnabledForUsePrimaryPropertyAsync(product.ProductId, ct);
//            if (!isEnabled) continue;

//            var upfmProp = new UPFMProperty { id = currentPrimaryProperties };
//            var translated = blueBook.TranslateProductPrimaryPropertiesData(upfmProp, product.ProductId, null);
//            if (translated?.Records is null) continue;

//            var ids = ((IEnumerable<object>)translated.Records)
//                .OfType<ProductProperty>()
//                .Where(p => p.IsAssigned == true)
//                .Select(p => p.ID)
//                .ToList();

//            if (ids.Count > 0)
//                product.InputJson.PropertyList = ids;
//        }

//        return productBatch;
//    }

//    private async Task<bool> IsProductEnabledForUsePrimaryPropertyAsync(int productId, CancellationToken ct)
//    {
//        var settings = await _internalSettingRepo.GetProductInternalSettingsAsync(productId, ct);
//        return settings
//            .FirstOrDefault(s => s.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase))
//            ?.Value?.Trim() == "1";
//    }

//    #endregion

//    // ════════════════════════════════════════════════════════════════════════
//    // Private — audit / activity log helpers
//    // ════════════════════════════════════════════════════════════════════════

//    #region AuditActivityLog / LogAuditActivity / AddActivityLog / AuditUserUpdate / AuditBulkIdpUpdateAsync

//    /// <summary>
//    /// Writes a field-level change audit event (phone numbers, supervisor, etc.)
//    /// Replaces: inline calls to <c>LogActivity.WriteActivity</c> throughout UserRepository.
//    /// </summary>
//    private void AuditActivityLog(
//        string oldValue, string newValue, string fieldName,
//        UserActivityLogInfo toUserLogInfo, UserDetails? impersonatorInfo)
//    {
//        var userClaims = _userClaimAccessor.Current;
//        try
//        {
//            string userName = impersonatorInfo is not null
//                ? $"RealPage Access ({impersonatorInfo.FirstName} {impersonatorInfo.LastName})"
//                : $"{userClaims.FirstName} {userClaims.LastName}";

//            var additionalInfo = new List<AdditionalParameters>();
//            string message;

//            if (fieldName is "Deleted Phone Number")
//            {
//                message = $"{userName} Deleted Phone Number {oldValue}.";
//                additionalInfo.Add(new AdditionalParameters { Key = "Phone Number", Value = $"{{\"action\":\"Deleted\",\"value\":\"{oldValue}\"}}" });
//            }
//            else if (fieldName is "Added Phone Number")
//            {
//                message = $"{userName} Added Phone Number {oldValue}.";
//                additionalInfo.Add(new AdditionalParameters { Key = "Phone Number", Value = $"{{\"action\":\"Added\",\"value\":\"{oldValue}\"}}" });
//            }
//            else
//            {
//                string safeOld = string.IsNullOrEmpty(oldValue) ? "Blank Value" : oldValue;
//                string safeNew = string.IsNullOrEmpty(newValue) ? "Blank Value" : newValue;
//                message = $"{userName} updated {fieldName} from {safeOld} to {safeNew}.";
//                additionalInfo.Add(new AdditionalParameters { Key = fieldName, Value = $"{{\"action\":\"Updated From\",\"value\":\"{(safeOld == "Blank Value" ? " " : safeOld)}\"}}" });
//                additionalInfo.Add(new AdditionalParameters { Key = fieldName, Value = $"{{\"action\":\"Updated To\",\"value\":\"{(safeNew == "Blank Value" ? " " : safeNew)}\"}}" });
//            }

//            LogActivity.WriteActivity(new ActivityDetails
//            {
//                LogActivityTypeName = LogActivityTypeConstants.UPDATE_USER,
//                LogCategoryName = LogActivityCategoryType.User.ToString(),
//                CorrelationId = userClaims.CorrelationId.ToString(),
//                BooksMasterOrganizationId = userClaims.OrganizationMasterId,
//                OrganizationPartyId = userClaims.OrganizationPartyId,
//                Message = message,
//                FromUserLoginName = userClaims.LoginName,
//                FromUserLoginId = userClaims.UserId,
//                FromUserFirstName = userClaims.FirstName,
//                FromUserLastName = userClaims.LastName,
//                FromUserRealpageId = userClaims.UserRealPageGuid.ToString(),
//                ToUserLoginName = toUserLogInfo.LoginName,
//                ToUserLoginId = toUserLogInfo.UserId,
//                ToUserFirstName = toUserLogInfo.FirstName,
//                ToUserLastName = toUserLogInfo.LastName,
//                ToUserRealpageId = toUserLogInfo.RealPageId.ToString(),
//                AdditionalInformation = additionalInfo
//            });
//        }
//        catch (Exception ex) { _logger.LogWarning(ex, "AuditActivityLog failed for field={F}", fieldName); }
//    }

//    /// <summary>
//    /// Profile-level AuditActivityLog overload — called by UpdateUser for field changes.
//    /// Implements the public <c>AuditActivityLog</c> method on IUserService.
//    /// </summary>
//    public void AuditActivityLog(
//        string oldValue, string newValue, string fieldName,
//        string message, IProfileDetail profile)
//    {
//        var userClaims = _userClaimAccessor.Current;
//        try
//        {
//            LogActivity.WriteActivity(new ActivityDetails
//            {
//                LogActivityTypeName = LogActivityTypeConstants.UPDATE_USER,
//                LogCategoryName = LogActivityCategoryType.User.ToString(),
//                CorrelationId = userClaims.CorrelationId.ToString(),
//                BooksMasterOrganizationId = userClaims.OrganizationMasterId,
//                OrganizationPartyId = userClaims.OrganizationPartyId,
//                Message = message,
//                FromUserLoginName = userClaims.LoginName,
//                FromUserLoginId = userClaims.UserId,
//                FromUserFirstName = userClaims.FirstName,
//                FromUserLastName = userClaims.LastName,
//                FromUserRealpageId = userClaims.UserRealPageGuid.ToString(),
//                ToUserLoginName = profile?.userLogin?.LoginName,
//                ToUserLoginId = profile?.userLogin?.UserId ?? 0,
//                ToUserFirstName = profile?.FirstName,
//                ToUserLastName = profile?.LastName,
//                ToUserRealpageId = profile?.RealPageId.ToString(),
//                AdditionalInformation =
//                [
//                    new() { Key = fieldName, Value = $"{{\"action\":\"Updated From\",\"value\":\"{oldValue}\"}}" },
//                    new() { Key = fieldName, Value = $"{{\"action\":\"Updated To\",\"value\":\"{newValue}\"}}" }
//                ]
//            });
//        }
//        catch (Exception ex) { _logger.LogWarning(ex, "AuditActivityLog(profile) failed for field={F}", fieldName); }
//    }

//    private void LogAuditActivity(
//        string logActivityType, LogActivityCategoryType categoryType,
//        string message, string stepName, IProfileDetail? profile,
//        List<AdditionalParameters>? additionalInformation = null)
//    {
//        var userClaims = _userClaimAccessor.Current;
//        try
//        {
//            LogActivity.WriteActivity(new ActivityDetails
//            {
//                LogActivityTypeName = logActivityType,
//                LogCategoryName = categoryType.ToString(),
//                CorrelationId = userClaims.CorrelationId.ToString(),
//                BooksMasterOrganizationId = userClaims.OrganizationMasterId,
//                OrganizationPartyId = userClaims.OrganizationPartyId,
//                Message = message,
//                FromUserLoginName = userClaims.LoginName,
//                FromUserLoginId = userClaims.UserId,
//                FromUserFirstName = userClaims.FirstName,
//                FromUserLastName = userClaims.LastName,
//                FromUserRealpageId = userClaims.UserRealPageGuid.ToString(),
//                ToUserLoginName = profile?.userLogin?.LoginName,
//                ToUserLoginId = profile?.userLogin?.UserId ?? 0,
//                ToUserFirstName = profile?.FirstName,
//                ToUserLastName = profile?.LastName,
//                ToUserRealpageId = profile?.RealPageId.ToString(),
//                AdditionalInformation = additionalInformation ?? []
//            });
//        }
//        catch (Exception ex) { _logger.LogWarning(ex, "LogAuditActivity failed: step={S}", stepName); }
//    }

//    private void AddActivityLog(
//        string logActivityType, LogActivityCategoryType categoryType,
//        string message, IPerson person,
//        IUserLoginOnly? userLogin = null, IUserOrganization? userOrg = null,
//        DefaultUserClaim? claimOverride = null)
//    {
//        var userClaims = claimOverride ?? _userClaimAccessor.Current;
//        try
//        {
//            LogActivity.WriteActivity(new ActivityDetails
//            {
//                LogActivityTypeName = logActivityType,
//                LogCategoryName = categoryType.ToString(),
//                CorrelationId = userClaims.CorrelationId.ToString(),
//                BooksMasterOrganizationId = userClaims.OrganizationMasterId,
//                OrganizationPartyId = userClaims.OrganizationPartyId,
//                Message = message,
//                FromUserLoginName = userClaims.LoginName,
//                FromUserLoginId = userClaims.UserId,
//                FromUserFirstName = userClaims.FirstName,
//                FromUserLastName = userClaims.LastName,
//                FromUserRealpageId = userClaims.UserRealPageGuid.ToString(),
//                ToUserLoginName = userLogin?.LoginName,
//                ToUserLoginId = userLogin?.UserId ?? 0,
//                ToUserFirstName = person?.FirstName,
//                ToUserLastName = person?.LastName,
//                ToUserRealpageId = userLogin?.RealPageId.ToString()
//            });
//        }
//        catch (Exception ex) { _logger.LogWarning(ex, "AddActivityLog failed"); }
//    }

//    /// <summary>
//    /// Post-commit audit for profile field changes (login name, first/last name, status).
//    /// Replaces: UserRepository.AuditUserUpdate.
//    /// </summary>
//    private void AuditUserUpdate(IProfileDetail oldProfile, IProfileDetail newProfile)
//    {
//        var userClaims = _userClaimAccessor.Current;
//        try
//        {
//            string actor = string.IsNullOrEmpty(userClaims.ImpersonatedByName)
//                ? $"{userClaims.FirstName} {userClaims.LastName}"
//                : $"RealPage Access ({userClaims.ImpersonatedByName})";

//            if (IsUserLoginNameChanged(newProfile, oldProfile))
//                AuditActivityLog(oldProfile.userLogin.LoginName, newProfile.userLogin.LoginName, "Login Name",
//                    $"{actor} updated Login Name from {oldProfile.userLogin.LoginName} to {newProfile.userLogin.LoginName}.", newProfile);

//            if (!string.Equals(oldProfile.FirstName, newProfile.FirstName, StringComparison.OrdinalIgnoreCase))
//                AuditActivityLog(oldProfile.FirstName, newProfile.FirstName, "First Name",
//                    $"{actor} updated First Name from {oldProfile.FirstName} to {newProfile.FirstName}.", newProfile);

//            if (!string.Equals(oldProfile.LastName, newProfile.LastName, StringComparison.OrdinalIgnoreCase))
//                AuditActivityLog(oldProfile.LastName, newProfile.LastName, "Last Name",
//                    $"{actor} updated Last Name from {oldProfile.LastName} to {newProfile.LastName}.", newProfile);

//            if (oldProfile.userLogin.IsActive != newProfile.userLogin.IsActive)
//            {
//                string st = newProfile.userLogin.IsActive == true ? "Activated" : "Deactivated";
//                AuditActivityLog(oldProfile.userLogin.IsActive.ToString(), newProfile.userLogin.IsActive.ToString(),
//                    "User Status", $"{actor} {st} user {newProfile.userLogin.LoginName}.", newProfile);
//            }
//        }
//        catch (Exception ex) { _logger.LogWarning(ex, "AuditUserUpdate failed for {Id}", newProfile.RealPageId); }
//    }

//    /// <summary>
//    /// Audits every user whose IDP flag was flipped by ThirdPartyIdpBulkUpdateAsync.
//    /// Replaces: UserRepository.ActivityLogForBulkIDPUpdate.
//    /// </summary>
//    private async Task AuditBulkIdpUpdateAsync(
//        IList<long> updatedIds, bool isEnabled,
//        DefaultUserClaim userClaims, CancellationToken ct)
//    {
//        var profiles = await _userRepo.GetUserProfilesByUserIdsAsync(
//            userClaims.OrganizationPartyId, updatedIds, ct);

//        string action = isEnabled ? "enabled" : "disabled";

//        foreach (var profile in profiles)
//        {
//            try
//            {
//                string message = string.IsNullOrEmpty(userClaims.ImpersonatedByName)
//                    ? $"{userClaims.FirstName} {userClaims.LastName} {action} Third-Party IDP for {profile.FirstName} {profile.LastName} ({profile.LoginName})."
//                    : $"RealPage Access ({userClaims.ImpersonatedByName}) {action} Third-Party IDP for {profile.FirstName} {profile.LastName} ({profile.LoginName}).";

//                LogActivity.WriteActivity(new ActivityDetails
//                {
//                    LogActivityTypeName = LogActivityTypeConstants.UPDATE_USER,
//                    LogCategoryName = LogActivityCategoryType.User.ToString(),
//                    CorrelationId = userClaims.CorrelationId.ToString(),
//                    BooksMasterOrganizationId = userClaims.OrganizationMasterId,
//                    OrganizationPartyId = userClaims.OrganizationPartyId,
//                    Message = message,
//                    FromUserLoginName = userClaims.LoginName,
//                    FromUserLoginId = userClaims.UserId,
//                    FromUserFirstName = userClaims.FirstName,
//                    FromUserLastName = userClaims.LastName,
//                    FromUserRealpageId = userClaims.UserRealPageGuid.ToString(),
//                    ToUserLoginName = profile.LoginName,
//                    ToUserLoginId = profile.UserId,
//                    ToUserFirstName = profile.FirstName,
//                    ToUserLastName = profile.LastName,
//                    ToUserRealpageId = profile.RealPageId.ToString(),
//                    AdditionalInformation =
//                    [
//                        new() { Key = "Third-Party IDP", Value = $"{{\"action\":\"{char.ToUpper(action[0]) + action[1..]}\",\"value\":\"{action}\"}}" }
//                    ]
//                });
//            }
//            catch (Exception ex) { _logger.LogWarning(ex, "AuditBulkIdpUpdateAsync failed for userId={Id}", profile.UserId); }
//        }
//    }

//    #endregion

//    #region GetUserActivityLogInfoAsync

//    private async Task<UserActivityLogInfo> GetUserActivityLogInfoAsync(
//        IDbTransaction tx, long personaId, Guid userRealPageId, CancellationToken ct)
//    {
//        var persona = await _personaRepo.GetPersonaAsync(personaId, false, ct);
//        var userLogin = await _userLoginRepo.GetUserLoginOnlyAsync(userRealPageId);
//        var person = await Q<Person>(StoredProcNameConstants.SP_GetPerson, tx, new { realPageId = userRealPageId }, ct);

//        return new UserActivityLogInfo
//        {
//            FirstName = person?.FirstName,
//            LastName = person?.LastName,
//            RealPageId = userLogin?.RealPageId ?? Guid.Empty,
//            LoginName = userLogin?.LoginName,
//            BooksOrganizationMasterId = persona?.Organization?.BooksMasterId ?? 0,
//            OrganizationPartyId = persona?.OrganizationPartyId ?? 0,
//            UserId = userLogin?.UserId ?? 0
//        };
//    }

//    #endregion

//    // ════════════════════════════════════════════════════════════════════════
//    // Private — pure / static logic helpers
//    // ════════════════════════════════════════════════════════════════════════

//    #region GetCurrentUserClaim / GetUserBatch

//    private static DefaultUserClaim GetCurrentUserClaim(ManageProfile profileLogic, Organization org)
//    {
//        try { return profileLogic.GetCurrentUserClaim(org); }
//        catch { return new DefaultUserClaim(); }
//    }

//    /// <summary>
//    /// Determines whether the user type changed and in which direction.
//    /// Replaces: UserRepository.GetUserBatch.
//    /// </summary>
//    private static UserBatchEntity GetUserBatch(
//        IProfileDetail newProfile, IProfileDetail oldProfile, bool userIsExternalEverywhere)
//    {
//        var entity = new UserBatchEntity { UserTypeChangedToFromExternal = string.Empty };

//        if (oldProfile.UserTypeId == newProfile.UserTypeId) return entity;

//        bool oldIsExternal = oldProfile.UserTypeId == (int)UserRoleType.ExternalUser;
//        bool newIsExternal = newProfile.UserTypeId == (int)UserRoleType.ExternalUser;
//        bool oldIsSuperUser = oldProfile.UserTypeId == (int)UserRoleType.SuperUser;
//        bool newIsSuperUser = newProfile.UserTypeId == (int)UserRoleType.SuperUser;

//        entity.UserTypeChanged = true;

//        if (newIsExternal && !oldIsExternal)
//        {
//            entity.UserTypeChangedToFromExternal = "ToExternal";
//            entity.BatchProcessUserType = (int)BatchProcessType.UserTypeAdminToExternal;
//        }
//        else if (!newIsExternal && oldIsExternal)
//        {
//            entity.UserTypeChangedToFromExternal = "FromExternal";
//            entity.BatchProcessUserType = (int)BatchProcessType.UserTypeExternalToRegular;
//        }
//        else if (newIsSuperUser && !oldIsSuperUser)
//        {
//            entity.BatchProcessUserType = (int)BatchProcessType.UserTypeRegularToAdmin;
//        }
//        else if (!newIsSuperUser && oldIsSuperUser)
//        {
//            entity.BatchProcessUserType = (int)BatchProcessType.UserTypeAdminToRegular;
//        }

//        return entity;
//    }

//    #endregion

//    #region IsUserProfileChanged / IsUserLoginNameChanged / IsEmployeeIdChanged / IsSupervisorIdChanged / IsNotificationEmailChanged / IsExternalUserRelationUpdated

//    private static bool IsUserProfileChanged(IProfileDetail newProfile, IProfileDetail oldProfile)
//        => !string.Equals(newProfile.FirstName, oldProfile.FirstName, StringComparison.OrdinalIgnoreCase)
//        || !string.Equals(newProfile.LastName, oldProfile.LastName, StringComparison.OrdinalIgnoreCase)
//        || !string.Equals(newProfile.MiddleName, oldProfile.MiddleName, StringComparison.OrdinalIgnoreCase)
//        || !string.Equals(newProfile.Suffix, oldProfile.Suffix, StringComparison.OrdinalIgnoreCase);

//    private static bool IsUserProfileChanged(IProfileDetail newProfile, UserDetails userDetails)
//        => !string.Equals(newProfile.FirstName, userDetails.FirstName, StringComparison.OrdinalIgnoreCase)
//        || !string.Equals(newProfile.LastName, userDetails.LastName, StringComparison.OrdinalIgnoreCase);

//    private static bool IsUserLoginNameChanged(IProfileDetail newProfile, IProfileDetail oldProfile)
//        => !string.Equals(newProfile.userLogin.LoginName, oldProfile.userLogin.LoginName, StringComparison.OrdinalIgnoreCase);

//    private static bool IsEmployeeIdChanged(IProfileDetail newProfile, IProfileDetail oldProfile)
//        => !string.Equals(newProfile.EmployeeId, oldProfile.EmployeeId, StringComparison.OrdinalIgnoreCase);

//    private static bool IsSupervisorIdChanged(IProfileDetail newProfile, IProfileDetail oldProfile)
//        => newProfile.SuperVisorUserId != oldProfile.SuperVisorUserId;

//    private static bool IsNotificationEmailChanged(string? prior, string? next)
//        => !string.Equals(prior, next, StringComparison.OrdinalIgnoreCase);

//    private static bool IsExternalUserRelationUpdated(
//        IProfileDetail newProfile, IProfileDetail oldProfile,
//        out bool deleteOldPropertyInstanceMapping)
//    {
//        deleteOldPropertyInstanceMapping = false;
//        if (!FeatureFlag.GetUserCompanyAssociationFeatureFlag()) return false;
//        if (newProfile.ExternalUserRelationship is null) return false;
//        if (oldProfile.ExternalUserRelationship is null) return true;

//        bool updated = newProfile.ExternalUserRelationship.ThirdPartyRelationShipId
//                           != oldProfile.ExternalUserRelationship.ThirdPartyRelationShipId
//                    || !string.Equals(newProfile.ExternalUserRelationship.OperatorCode,
//                           oldProfile.ExternalUserRelationship.OperatorCode, StringComparison.OrdinalIgnoreCase)
//                    || !string.Equals(newProfile.ExternalUserRelationship.OperatorValue,
//                           oldProfile.ExternalUserRelationship.OperatorValue, StringComparison.OrdinalIgnoreCase);

//        if (updated && newProfile.UserTypeId == (int)UserRoleType.ExternalUser)
//            deleteOldPropertyInstanceMapping = true;

//        return updated;
//    }

//    #endregion

//    #region DetermineUserStatus / ResolveUpdateUserStatus

//    private static int DetermineUserStatus(
//        OrganizationPrimary currentOrg, Organization orgExternalUser,
//        IList<UserOrganization> userPersonaOrgList, OrganizationStatus? currentPrimaryOrgStatus,
//        DateTime fromDate, IIdentityProviderType? idpt, ProfileDetail newProfile,
//        ref DateTime? statusThruDate)
//    {
//        int statusId = (int)UserUiStatusType.Active;

//        if (currentOrg.OrganizationPartyId == orgExternalUser.PartyId)
//        {
//            if (currentPrimaryOrgStatus?.IsPending == true)
//            {
//                statusId = currentPrimaryOrgStatus.StatusTypeId;
//                statusThruDate = null;
//            }
//            else if (userPersonaOrgList.Count == 1 && currentPrimaryOrgStatus?.IsActive == false)
//            {
//                if (fromDate.Subtract(DateTime.UtcNow).TotalMinutes >= 15)
//                    statusId = (int)UserUiStatusType.Disabled;
//                else if (idpt?.IsLocal == true)
//                    statusId = (int)UserUiStatusType.Pending;
//                else { statusId = (int)UserUiStatusType.Active; statusThruDate = null; }
//            }
//            else { statusId = (int)UserUiStatusType.Active; statusThruDate = null; }
//        }
//        else
//        {
//            if (fromDate.Subtract(DateTime.UtcNow).TotalMinutes >= 15)
//                statusId = (int)UserUiStatusType.Disabled;
//            else if (idpt?.IsLocal == true)
//            {
//                if (currentPrimaryOrgStatus is null)
//                {
//                    statusId = newProfile.userLogin.doNotForceChangePassword == true
//                        ? (int)UserUiStatusType.Active
//                        : (int)UserUiStatusType.Pending;
//                    if (newProfile.userLogin.doNotForceChangePassword == true) statusThruDate = null;
//                }
//                else
//                {
//                    statusId = (int)UserUiStatusType.Active; statusThruDate = null;
//                    if (newProfile.UserTypeId != (int)UserRoleType.ExternalUser && currentPrimaryOrgStatus.IsPending)
//                    { statusId = currentPrimaryOrgStatus.StatusTypeId; statusThruDate = currentPrimaryOrgStatus.StatusThruDate; }
//                }
//            }
//            else { statusId = (int)UserUiStatusType.Active; statusThruDate = null; }
//        }

//        return statusId;
//    }

//    private static UserUiStatusType ResolveUpdateUserStatus(
//        UpdateUserProfileEntity e, IIdentityProviderType? idpt, bool futureDate)
//    {
//        if (futureDate) return UserUiStatusType.Disabled;

//        if (e.NewProfile.userLogin.IsActive == true && e.CurrentOrgStatus.IsActive == false)
//        {
//            return idpt?.IsLocal == true && !e.NewProfile.userLogin.doNotForceChangePassword.GetValueOrDefault()
//                ? UserUiStatusType.Pending
//                : UserUiStatusType.Active;
//        }

//        if (e.NewProfile.userLogin.IsActive == false && e.CurrentOrgStatus.IsActive == true)
//            return UserUiStatusType.Disabled;

//        if (e.NewProfile.userLogin.Is3rdPartyIDP != e.UserLoginOnly.Is3rdPartyIDP)
//        {
//            return idpt?.IsLocal == true && !e.NewProfile.userLogin.doNotForceChangePassword.GetValueOrDefault()
//                ? UserUiStatusType.Pending
//                : UserUiStatusType.Active;
//        }

//        return UserUiStatusType.UnDefined;
//    }

//    #endregion

//    #region GetPropertyInstanceUnifiedLoginAsync / GetRoleRightsSchemaNameAsync / IsLoginNameExistsAsAdminInOtherDomainAsync / GetUnifiedPlatformDefaultRoleAsync

//    private async Task<bool> GetPropertyInstanceUnifiedLoginAsync(CancellationToken ct)
//    {
//        return await _cache.GetOrSetAsync(
//            $"getPropertyInstanceUnifiedLogin_{(int)ProductEnum.UnifiedPlatform}",
//            async c =>
//            {
//                var settings = await _internalSettingRepo.GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, c);
//                return settings.FirstOrDefault(s => s.Name.Equals("UsePropertyInstanceUnifiedLogin",
//                    StringComparison.OrdinalIgnoreCase))?.Value == "1";
//            },
//            PropertyInstanceCacheOptions,
//            ct);
//    }

//    /// <summary>
//    /// Replaces: RPObjectCache.GetFromCache("getRoleRightsSchemaName_...") inside UserRepository.
//    /// Used by InsertUpdateEnterpriseRoleToUserAsync and UnassignEnterpriseRoleFromUserAsync.
//    /// </summary>
//    private async Task<string?> GetRoleRightsSchemaNameAsync(CancellationToken ct)
//    {
//        return await _cache.GetOrSetAsync(
//            $"getRoleRightsSchemaName_{(int)ProductEnum.UnifiedPlatform}",
//            async c =>
//            {
//                var settings = await _internalSettingRepo.GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, c);
//                return settings.FirstOrDefault(s => s.Name.Equals("RolesRightsSchemaName",
//                    StringComparison.OrdinalIgnoreCase))?.Value;
//            },
//            PropertyInstanceCacheOptions,
//            ct);
//    }

//    private async Task<UserOrganizationExists> IsLoginNameExistsAsAdminInOtherDomainAsync(
//        string loginName, Guid organizationRealPageId, long booksMasterId, CancellationToken ct)
//    {
//        return await _db.QuerySingleOrDefaultAsync<UserOrganizationExists>(
//            new CommandDefinition(
//                StoredProcNameConstants.SP_IsLoginNameExistsAsAdminInOtherDomain,
//                new { LoginName = loginName, OrganizationRealPageId = organizationRealPageId, BooksMasterId = booksMasterId },
//                commandType: CommandType.StoredProcedure, cancellationToken: ct))
//            ?? new UserOrganizationExists();
//    }

//    private async Task<int> GetUnifiedPlatformDefaultRoleAsync(
//        IDbTransaction tx, Guid realPageId,
//        IEnumerable<EnterpriseRole> enterpriseRoles, CancellationToken ct)
//    {
//        var row = await Q<dynamic>(StoredProcNameConstants.SP_GetUnifiedLoginDefaultRole, tx,
//            new { RealPageID = realPageId }, ct);

//        return row is not null
//            ? (int)row.RoleId
//            : enterpriseRoles.FirstOrDefault(r => r.Role.Equals("Basic End User", StringComparison.OrdinalIgnoreCase))?.RoleId ?? 0;
//    }

//    #endregion

//    #region CompareList (static)

//    private static bool CompareList(List<long> first, List<long> second)
//        => first.Count == second.Count && !first.Except(second).Any();

//    #endregion
//}