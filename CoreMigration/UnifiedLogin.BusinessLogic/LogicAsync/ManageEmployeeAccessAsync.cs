using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.Services.User;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.EmployeeAccess;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using UnifiedLogin.SharedObjects.Product.UPFMProduct;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// True-async implementation of <see cref="IManageEmployeeAccessAsync"/>.
/// <para>
/// Replaces the previous stub that self-injected <see cref="IManageEmployeeAccessAsync"/>
/// and the legacy <see cref="UnifiedLogin.BusinessLogic.Logic.ManageEmployeeAccess"/> which:
/// <list type="bullet">
///   <item>Inherited <c>ManageProductBase</c> — eliminated; context resolved via <see cref="IProductContextServiceAsync"/>.</item>
///   <item>Created 12+ inline <c>new Xxx(_userClaim)</c> instances — all replaced with DI-injected async interfaces.</item>
///   <item>Used <c>DefaultUserClaim userClaim</c> parameter on <c>GetOrCreateEmployeePersonaId</c> — removed; <see cref="IUserClaimsAccessor"/> used instead.</item>
///   <item>Instantiated <c>new UnifiedLoginRepository()</c> inline — replaced by <see cref="IUnifiedLoginRepositoryAsync"/>.</item>
///   <item>Used <c>WriteToDiagnosticLog</c>/<c>WriteToErrorLog</c> — replaced by <see cref="ILogger{T}"/>.</item>
/// </list>
/// </para>
/// <para><b>DI registration:</b> <c>Scoped</c>.</para>
/// </summary>
public sealed class ManageEmployeeAccessAsync : IManageEmployeeAccessAsync
{
    private const string ProductStatusSettingType = "ProductStatus";

    private readonly IUserClaimsAccessor                   _userClaims;
    private readonly IProductContextServiceAsync           _productContext;
    private readonly IUnifiedLoginRepositoryAsync          _unifiedLoginRepository;
    private readonly IManageBlueBookAsync                  _blueBook;
    private readonly IManagePersonaAsync                   _managePersona;
    private readonly IManageOrganizationAsync              _manageOrganization;
    private readonly IProductRepositoryAsync               _productRepository;
    private readonly IUserRepositoryAsync                  _userRepository;
    private readonly IUserLoginRepositoryAsync             _userLoginRepository;
    private readonly IManageUPFMProductsIntegrationAsync   _manageUPFMProductsIntegration;
    private readonly IIntegrationTypeFactoryAsync          _integrationTypeFactory;
    private readonly IUserCreationService                  _userCreationService;
    private readonly ILogger<ManageEmployeeAccessAsync>    _logger;

    public ManageEmployeeAccessAsync(
        IUserClaimsAccessor                  userClaims,
        IProductContextServiceAsync          productContext,
        IUnifiedLoginRepositoryAsync         unifiedLoginRepository,
        IManageBlueBookAsync                 blueBook,
        IManagePersonaAsync                  managePersona,
        IManageOrganizationAsync             manageOrganization,
        IProductRepositoryAsync              productRepository,
        IUserRepositoryAsync                 userRepository,
        IUserLoginRepositoryAsync            userLoginRepository,
        IManageUPFMProductsIntegrationAsync  manageUPFMProductsIntegration,
        IIntegrationTypeFactoryAsync         integrationTypeFactory,
        IUserCreationService                 userCreationService,
        ILogger<ManageEmployeeAccessAsync>   logger)
    {
        _userClaims                   = userClaims                   ?? throw new ArgumentNullException(nameof(userClaims));
        _productContext               = productContext               ?? throw new ArgumentNullException(nameof(productContext));
        _unifiedLoginRepository       = unifiedLoginRepository       ?? throw new ArgumentNullException(nameof(unifiedLoginRepository));
        _blueBook                     = blueBook                     ?? throw new ArgumentNullException(nameof(blueBook));
        _managePersona                = managePersona                ?? throw new ArgumentNullException(nameof(managePersona));
        _manageOrganization           = manageOrganization           ?? throw new ArgumentNullException(nameof(manageOrganization));
        _productRepository            = productRepository            ?? throw new ArgumentNullException(nameof(productRepository));
        _userRepository               = userRepository               ?? throw new ArgumentNullException(nameof(userRepository));
        _userLoginRepository          = userLoginRepository          ?? throw new ArgumentNullException(nameof(userLoginRepository));
        _manageUPFMProductsIntegration = manageUPFMProductsIntegration ?? throw new ArgumentNullException(nameof(manageUPFMProductsIntegration));
        _integrationTypeFactory       = integrationTypeFactory       ?? throw new ArgumentNullException(nameof(integrationTypeFactory));
        _userCreationService          = userCreationService          ?? throw new ArgumentNullException(nameof(userCreationService));
        _logger                       = logger                       ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Public Methods

    /// <inheritdoc/>
    public async Task<ListResponse> GetCompaniesAsync(
        long              editorPersonaId,
        string            filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfZero(editorPersonaId);

        var response = new ListResponse();
        try
        {
            _logger.LogDebug("{CorrelationId} GetCompaniesAsync start EditorPersonaId={EditorPersonaId}",
                _userClaims.CorrelationId, editorPersonaId);

            var (_, error) = await _productContext
                .GetUserContextAsync(editorPersonaId, editorPersonaId, (int)ProductEnum.SupportTool, cancellationToken)
                .ConfigureAwait(false);
            if (error is not null) return error;

            var organizationTypeIds = await GetUserAccessOrganizationTypesAsync(editorPersonaId, cancellationToken)
                .ConfigureAwait(false);

            var gbAllCompanies = await _unifiedLoginRepository
                .ListCompaniesAsync(filter, organizationTypeIds, cancellationToken)
                .ConfigureAwait(false);
            var gbAllActiveCompanies = gbAllCompanies.Where(c => c.IsActive == true).ToList();

            var bbCompanies = await _blueBook
                .GetCompanyListByCompIdsAsync(gbAllActiveCompanies, cancellationToken)
                .ConfigureAwait(false);
            var mergedCompanies = MergeCompanies(gbAllActiveCompanies, bbCompanies);

            return new ListResponse
            {
                Records     = mergedCompanies.Cast<object>().ToList(),
                TotalRows   = mergedCompanies.Count,
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages  = 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{CorrelationId} GetCompaniesAsync failed EditorPersonaId={EditorPersonaId}",
                _userClaims.CorrelationId, editorPersonaId);
            response.IsError     = true;
            response.ErrorReason = "EmployeeAccess - There was a problem getting the companies.";
            return response;
        }
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetUsersAsync(
        long              editorPersonaId,
        string            filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfZero(editorPersonaId);

        var response = new ListResponse();
        try
        {
            _logger.LogDebug("{CorrelationId} GetUsersAsync start EditorPersonaId={EditorPersonaId}",
                _userClaims.CorrelationId, editorPersonaId);

            var (_, error) = await _productContext
                .GetUserContextAsync(editorPersonaId, editorPersonaId, (int)ProductEnum.SupportTool, cancellationToken)
                .ConfigureAwait(false);
            if (error is not null) return error;

            var organizationTypeIds = await GetUserAccessOrganizationTypesAsync(editorPersonaId, cancellationToken)
                .ConfigureAwait(false);

            var gbAllCompanies = await _unifiedLoginRepository
                .ListCompaniesAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            var ulUsersByFilter = await _unifiedLoginRepository
                .ListUsersAsync(filter, organizationTypeIds, cancellationToken)
                .ConfigureAwait(false);

            foreach (var item in ulUsersByFilter)
            {
                if (item.Name3rdPartyIDP?.ToUpper() == "IDENTITYSERVER")
                    item.Name3rdPartyIDP = "None";
            }

            var mergedUsers = MergeUserCompanies(gbAllCompanies, ulUsersByFilter);

            return new ListResponse
            {
                Records     = mergedUsers.Cast<object>().ToList(),
                TotalRows   = mergedUsers.Count,
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages  = 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{CorrelationId} GetUsersAsync failed EditorPersonaId={EditorPersonaId}",
                _userClaims.CorrelationId, editorPersonaId);
            response.IsError     = true;
            response.ErrorReason = "EmployeeAccess - There was a problem getting the users.";
            return response;
        }
    }

    /// <inheritdoc/>
    public async Task<EmployeePersona> GetOrCreateEmployeePersonaIdAsync(
        Guid              companyRealPageId,
        CancellationToken cancellationToken = default)
    {
        var employeePersona = new EmployeePersona();

        var realPageIdToUse = _userClaims.ImpersonatedBy != Guid.Empty
            ? _userClaims.ImpersonatedBy
            : _userClaims.UserRealPageGuid;

        employeePersona.RealpageUserId = realPageIdToUse;

        var currentUser = await _userRepository
            .GetUserDetailsAsync(null, realPageIdToUse.ToString(), cancellationToken)
            .ConfigureAwait(false);

        if (currentUser is null) return employeePersona;

        var loginName = _userClaims.ImpersonatedBy != Guid.Empty
            ? currentUser.LoginName
            : _userClaims.LoginName;

        var userPersonaOrganizationList = await _userLoginRepository
            .ListOrganizationByLoginNameAsync(loginName)
            .ConfigureAwait(false);

        if (userPersonaOrganizationList?.Count > 0)
        {
            var userProductAdGroups = await _productRepository
                .GetPersonaProductsAdGroupsCountAsync(currentUser.PersonaId, cancellationToken)
                .ConfigureAwait(false);
            var orgProducts = await _productRepository
                .GetProductIdsByCompanyAsync(companyRealPageId, cancellationToken)
                .ConfigureAwait(false);

            var matchedProductData = userProductAdGroups
                .Where(p => orgProducts.Contains(p.ProductId))
                .ToList();
            int maxCount = matchedProductData.Count > 0 ? matchedProductData.Max(x => x.ADGroupCount) : 0;

            var orgPersonaList = userPersonaOrganizationList
                .Where(x => x.OrganizationRealPageId == companyRealPageId)
                .ToList();
            int orgPersonaCount = orgPersonaList.Count;
            bool isRealPageEmployeeInOrg = true;

            foreach (var userPersona in orgPersonaList)
            {
                var userOrgInfo = await _userRepository
                    .GetUserDetailsAsync(userPersona.PersonaId, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                if (userOrgInfo is not null && !userOrgInfo.IsRPEmployee)
                {
                    isRealPageEmployeeInOrg = false;
                    break;
                }
            }

            var user = userPersonaOrganizationList.FirstOrDefault(x => x.OrganizationRealPageId == companyRealPageId);
            if (user is not null)
            {
                employeePersona.PersonaId = user.PersonaId;
            }
            else
            {
                employeePersona.PersonaId = await CreatePersonaInCompanyAsync(
                    currentUser.LoginName, companyRealPageId, currentUser, cancellationToken)
                    .ConfigureAwait(false);
                orgPersonaCount++;
            }

            if (isRealPageEmployeeInOrg && maxCount > 0 && orgPersonaCount > 0)
            {
                int newPersonasToCreate = maxCount - orgPersonaCount;
                for (int i = 1; i <= newPersonasToCreate; i++)
                {
                    string personaName = orgPersonaCount >= 2
                        ? "Secondary " + orgPersonaCount
                        : "Secondary";
                    await _managePersona
                        .CreateAdditionalPersonaAsync(companyRealPageId, currentUser.UserId, currentUser.UserId, personaName, cancellationToken)
                        .ConfigureAwait(false);
                    orgPersonaCount++;
                }
            }
        }

        return employeePersona;
    }

    /// <inheritdoc/>
    public async Task<string> CreateEmployeeProductUserAsync(
        int               productId,
        long              personaId,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfZero(productId);
        ArgumentOutOfRangeException.ThrowIfZero(personaId);

        var productInternalSettings = await _productRepository
            .GetProductInternalSettingsAsync(productId, cancellationToken)
            .ConfigureAwait(false);

        var supportsEmployeeAccess = productInternalSettings
            .FirstOrDefault(s => s.Name.Equals("SI_SupportsEmployeeCreation", StringComparison.OrdinalIgnoreCase))?.Value;
        var adGroupWithoutUserCreation = productInternalSettings
            .FirstOrDefault(s => s.Name.Equals("ProductAssignedViaADGroupWithoutUserCreation", StringComparison.OrdinalIgnoreCase))?.Value;

        if (string.IsNullOrEmpty(supportsEmployeeAccess) || supportsEmployeeAccess == "0")
            return "Product does not support employee creation.";

        if (supportsEmployeeAccess == "-1" || adGroupWithoutUserCreation == "1")
            return string.Empty;

        var userPersona = await _managePersona
            .GetPersonaAsync(personaId, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var personaList = await _managePersona
            .ListPersonaAsync(userPersona.RealPageId, cancellationToken)
            .ConfigureAwait(false);
        var companyPersonaList = personaList
            .Where(p => p.OrganizationPartyId == userPersona.OrganizationPartyId)
            .ToList();

        var employeePersona = personaList
            .FirstOrDefault(p => p.Organization.RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId);
        if (employeePersona is null)
            return "Employee persona could not be found in RealPage Employee company.";

        var productAdGroups = await _productRepository
            .GetAdGroupsForProductAsync(productId, cancellationToken)
            .ConfigureAwait(false);

        if (productAdGroups.Count > 0)
        {
            var userAdGroups = await _productRepository
                .GetAdGroupsForUserAsync(employeePersona.PersonaId, cancellationToken)
                .ConfigureAwait(false);

            // Fetch once and reuse — avoids duplicate SP call in the original
            var userProductToADGroups = await _userRepository
                .GetEmployeeProductADGroupMappingAsync(personaId, productId, cancellationToken)
                .ConfigureAwait(false);
            var existingProductAdGroupInfo = userProductToADGroups.FirstOrDefault();

            var isProductAssigned = await _productRepository
                .IsProductAssignedAsync(personaId, (int)ProductBatchStatusType.Success, productId, cancellationToken)
                .ConfigureAwait(false);

            if (isProductAssigned)
            {
                if (productAdGroups.All(p => p.ADGroupId != existingProductAdGroupInfo?.ADGroupId))
                {
                    await _productRepository
                        .UpdateProductSettingProductStatusAsync(personaId, productId, ProductStatusSettingType, (int)ProductBatchStatusType.Deleted, cancellationToken)
                        .ConfigureAwait(false);
                    return "DeletedProductLogin";
                }

                if (userProductToADGroups.Any(uptag => userAdGroups.Any(p => p.ADGroupId == uptag.ADGroupId)))
                    return string.Empty;

                if (userAdGroups.All(p => p.ADGroupId != existingProductAdGroupInfo?.ADGroupId))
                {
                    if (companyPersonaList.Count == 1 &&
                        userAdGroups.All(p => productAdGroups.FirstOrDefault(p1 => p1.ADGroupId == p.ADGroupId) is null))
                    {
                        await _productRepository
                            .UpdateProductSettingProductStatusAsync(personaId, productId, ProductStatusSettingType, (int)ProductBatchStatusType.Deleted, cancellationToken)
                            .ConfigureAwait(false);
                        return "You are no longer in an ADGroup for this product.";
                    }

                    if (userAdGroups.All(p => p.ADGroupId != existingProductAdGroupInfo?.ADGroupId))
                    {
                        await _productRepository
                            .UpdateProductSettingProductStatusAsync(personaId, productId, ProductStatusSettingType, (int)ProductBatchStatusType.Deleted, cancellationToken)
                            .ConfigureAwait(false);
                        return "DeletedProductLogin";
                    }
                }
            }
        }

        long adminUserPersonaId   = 0;
        var  adminCreatorRealPageId = Guid.Empty;

        if (userPersona.Organization.RealPageId != Guid.Empty)
        {
            adminCreatorRealPageId = await _manageOrganization
                .GetOrganizationAdminUserRealPageIdAsync(userPersona.Organization.RealPageId, cancellationToken)
                .ConfigureAwait(false);
            if (adminCreatorRealPageId == Guid.Empty)
                return "Missing company admin user.";

            var adminPersona = await _managePersona
                .GetFirstAvailablePersonaByCompanyAsync(adminCreatorRealPageId, userPersona.OrganizationPartyId, cancellationToken)
                .ConfigureAwait(false);
            adminUserPersonaId = adminPersona?.PersonaId ?? 0;
        }

        var productIntegrationType = productInternalSettings
            .FirstOrDefault(s => s.Name.Equals("ProductIntegrationType", StringComparison.OrdinalIgnoreCase))?.Value;

        if (string.Equals(productIntegrationType, "UPFM", StringComparison.OrdinalIgnoreCase))
        {
            var userADGroupsRoles = await _productRepository
                .GetAdGroupRolesByPersonaAsync(employeePersona.PersonaId, cancellationToken)
                .ConfigureAwait(false);
            var adGroupIds = userADGroupsRoles?.Where(y => y.ProductId == productId).Select(x => x.ADGroupId);

            if (adGroupIds is not null && productAdGroups.Any(y => adGroupIds.Contains(y.ADGroupId)))
            {
                var hasProperties = productInternalSettings
                    .FirstOrDefault(s => s.Name.Equals("UPFMProductsHasProperties", StringComparison.OrdinalIgnoreCase))?.Value;
                List<string> propertyList = hasProperties == "0" ? [] : ["-1"];
                List<string> roleList = userADGroupsRoles!
                    .Where(x => x.ProductId == productId)
                    .Select(y => y.RoleId.ToString())
                    .ToList();

                var upfmPropertyRole = new UPFMProductPropertyRole
                {
                    IsAssigned   = true,
                    PropertyList = propertyList,
                    RoleList     = roleList
                };

                var (upfmResult, _) = await _manageUPFMProductsIntegration
                    .ManageUPFMProductUserAsync(_userClaims.PersonaId, personaId, upfmPropertyRole, isEmpAccess: true, cancellationToken)
                    .ConfigureAwait(false);
                return upfmResult;
            }

            return "No ADGroups for UPFM products.";
        }
        else
        {
            var rolePropertyList = new RolePropertyList { PropertyList = ["-1"] };

            var productUser = new ProductUserProperitiesRoles
            {
                RealPageId                = adminCreatorRealPageId,
                ProductId                 = productId,
                CreateUserPersonaId       = adminUserPersonaId,
                AssignUserPersonaId       = personaId,
                CorrelationId             = _userClaims.CorrelationId,
                InputJson                 = JsonConvert.SerializeObject(rolePropertyList),
                CreateRealPageEmployee    = true,
                RealPageEmployeePersonaId = employeePersona.PersonaId
            };

            var integration = _integrationTypeFactory.GetIntegrationStandardV1(productId);
            var (result, _) = await integration.CreateUserAsync(productUser, cancellationToken).ConfigureAwait(false);
            return result;
        }
    }

    #endregion

    #region Private Helpers

    private async Task<string> GetUserAccessOrganizationTypesAsync(
        long              editorPersonaId,
        CancellationToken cancellationToken)
    {
        var userADGroups = (await _unifiedLoginRepository
            .GetPersonaADGroupsAsync(editorPersonaId, cancellationToken)
            .ConfigureAwait(false))
            .Select(x => x.ADGroupId)
            .ToList();

        var orgTypeADGroups = await _unifiedLoginRepository
            .GetOrgTypesADGroupsAsync(cancellationToken)
            .ConfigureAwait(false);

        var filteredOrgTypeIds = orgTypeADGroups
            .GroupBy(x => x.OrganizationTypeId)
            .Where(g =>
                !g.Any(x => x.ADGroupId is not null and not 0)
                || g.Any(x => userADGroups.Any(uad => uad == x.ADGroupId)))
            .Select(g => g.Key)
            .ToList();

        if (filteredOrgTypeIds.Count == 0)
            filteredOrgTypeIds.Add(0);

        return string.Join(",", filteredOrgTypeIds);
    }

    private async Task<long> CreatePersonaInCompanyAsync(
        string            loginName,
        Guid              companyRealPageId,
        UserDetails       currentUser,
        CancellationToken cancellationToken)
    {
        var newProfile = await BuildNewProfileAsync(companyRealPageId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        newProfile.IsRPEmployee = true;
        if (companyRealPageId != DefaultUserClaim.EmployeeCompanyRealPageId)
        {
            newProfile.ExternalUserRelationship = new ExternalUserRelationship
            {
                ThirdPartyRelationShipId = 5,
                ThirdPartyRelationShip   = "5",
                ThirdPartyCompanyName    = "RealPage"
            };
            newProfile.UserTypeId = (int)UserRoleType.ExternalUser;
        }

        var createResponse = await _userCreationService
            .CreateUserAsync(newProfile, newProfile.Persona, cancellationToken)
            .ConfigureAwait(false);
        return createResponse?.PersonaId ?? 0;
    }

    private async Task<ProfileDetail> BuildNewProfileAsync(
        Guid              companyRealPageId,
        UserDetails       currentUser,
        CancellationToken cancellationToken)
    {
        var newProfile = new ProfileDetail
        {
            FirstName            = currentUser.FirstName,
            LastName             = currentUser.LastName,
            CreateUserSourceType = CreateUserSourceType.UnifiedPlatform
        };

        var personaEnvironment      = await _managePersona.GetPersonaEnvironmentTypeAsync(cancellationToken).ConfigureAwait(false);
        var personaEnv              = personaEnvironment.SingleOrDefault(p => p.Name == "Production");
        var productInternalSettings = await _productRepository.GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, cancellationToken).ConfigureAwait(false);
        var org                     = await _manageOrganization.GetOrganizationAsync(companyRealPageId, cancellationToken: cancellationToken).ConfigureAwait(false);

        newProfile.organization.Add(org);
        newProfile.UserTypeId = 405;

        var persona = new Persona
        {
            Name                     = newProfile.UserTypeId == (int)UserRoleType.SuperUser ? "System Administrator" : "Primary",
            PersonaName              = "Primary",
            PersonaEnvironmentTypeId = (int)(personaEnv?.PersonaEnvironmentTypeId ?? 0),
            FromDate                 = DateTime.UtcNow.AddMinutes(-5),
            ThruDate                 = null
        };
        newProfile.Persona = new List<Persona> { persona };

        newProfile.userLogin = new UserLogin
        {
            LoginName     = currentUser.LoginName,
            IsActive      = true,
            IsPending     = false,
            IsExpired     = false,
            FromDate      = DateTime.UtcNow.AddMinutes(-5),
            Is3rdPartyIDP = false
        };

        var rolePropertyList = new RolePropertyList
        {
            IsVendorRecommendationChanges        = false,
            IsInsuranceExpired                   = false,
            IsVendorNotLinkedToAnyProperty       = false,
            Notifications                        = new Notifications
            {
                amenitiesViaEmail  = false,
                managerFdiViaEmail = false,
                managerMrViaEmail  = false
            },
            CanReceiveMonthlyReport              = false,
            IsAssignedNewPropertyByDefault       = false,
            UsePrimaryProperties                 = false,
            HasAccessToAllCurrentFutureProperties = false,
            HasAccessToSiteSpendManagementOnly   = false
        };

        // ProductId 3 = UnifiedPlatform — fetch default role for new employee personas
        var integration = _integrationTypeFactory.GetIntegration(3);
        var rolesResult = await integration.GetRolesAsync(
            _userClaims.PersonaId, 0, org.PartyId, null, null!, cancellationToken)
            .ConfigureAwait(false);

        var roleRights = rolesResult.Records?.Cast<UnifiedLoginRoleRights>().ToList() ?? [];

        var defaultRole      = productInternalSettings.FirstOrDefault(s => s.Name.Equals("EmployeeExternelUserDefautRole", StringComparison.OrdinalIgnoreCase))?.Value;
        var platformAdminRole = productInternalSettings.FirstOrDefault(s => s.Name.Equals("PlatformAdminRole", StringComparison.OrdinalIgnoreCase))?.Value;
        defaultRole ??= platformAdminRole;

        var role = roleRights.FirstOrDefault(x => x.Role == defaultRole && x.Roletype == "System");
        if (role is not null)
            rolePropertyList.RoleList = [role.RoleId.ToString()];

        rolePropertyList.PropertyList = ["-1"];

        newProfile.productBatch = [new ProductBatch { ProductId = 3, StatusTypeId = 5, RetryCount = 0, InputJson = rolePropertyList }];

        return newProfile;
    }

    private static List<CompanyDetails> MergeCompanies(
        List<UnifiedLoginCompany> gbCompanies,
        IList<Company>            bbCompanies)
    {
        var result = new List<CompanyDetails>();
        foreach (var gb in gbCompanies)
        {
            var bb = bbCompanies.FirstOrDefault(p => p.CustomerCompanyId == gb.BooksCustomerMasterId);
            var cd = new CompanyDetails
            {
                CompanyName      = gb.CompanyName,
                CompanyRealPageId = gb.CompanyRealPageId,
                UserRealPageId   = gb.UserRealPageId,
                UserLoginAs      = gb.UserLoginAs,
                PartyId          = gb.PartyId,
                IsActive         = gb.IsActive
            };

            if (bb is not null)
            {
                cd.CompanyId   = bb.CustomerCompanyId;
                cd.PhoneNumber = bb.PhoneNumber;
                if (bb.CustomerCompanyLocation is not null)
                {
                    foreach (var loc in bb.CustomerCompanyLocation.Where(l => l.IsPrimary == true))
                    {
                        cd.Address    = loc.Address;
                        cd.City       = loc.City;
                        cd.Country    = loc.Country;
                        cd.County     = loc.County;
                        cd.State      = loc.State;
                        cd.PostalCode = loc.PostalCode;
                    }
                }
                result.Add(cd);
            }
            else if (gb.BooksCustomerMasterId == -2)
            {
                cd.Address = "REALPAGE INTERNAL USE ONLY!";
                result.Add(cd);
            }
        }
        return result;
    }

    private static List<UserDetail> MergeUserCompanies(
        List<UnifiedLoginCompany> gbCompanies,
        List<UserDetail>          ulUsers)
    {
        foreach (var gb in gbCompanies)
        {
            foreach (var user in ulUsers.Where(u => gb.PartyId == u.CompanyId))
            {
                user.CompanyRealPageId = gb.CompanyRealPageId;
                user.UserRealPageId    = gb.UserRealPageId;
                user.BooksMasterId     = gb.CompanyId;
            }
        }
        return ulUsers;
    }

    #endregion
}
