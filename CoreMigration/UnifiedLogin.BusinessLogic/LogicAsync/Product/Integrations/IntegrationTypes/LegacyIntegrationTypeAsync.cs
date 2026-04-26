using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Accounting;
using UnifiedLogin.SharedObjects.Product.MarketingCenter;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.Ops;
using UnifiedLogin.SharedObjects.Product.ProspectContactCenter;
using UnifiedLogin.SharedObjects.Product.RentersInsurance;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;
using UnifiedLogin.SharedObjects.Product.Rum;
using UnifiedLogin.SharedObjects.Product.VendorServices;
using UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.ProductImplementation;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.IntegrationTypes;

/// <summary>
/// Native-async implementation of <see cref="IIntegrationTypeAsync"/> for Legacy-mapped products.
/// Replaces <c>LegacyIntegrationType</c> (sync).
/// <para>
/// Routes each operation to the product-specific async service when one exists, or
/// falls back to <see cref="StandardV1ProductIntegrationAsync"/> for products that use
/// the StandardV1 HTTP protocol (ClickPay, LeadManagement, LeadAnalytics,
/// PortfolioManagement, DepositAlternative) and for cross-cutting operations
/// (<c>GetAllRights</c>, <c>ExternalUserProfileChange</c>).
/// </para>
/// <para>
/// Products whose async services are not yet migrated in Phase 5
/// (ClientPortal, SalesForce, SelfProvisioningPortal) return safe default responses
/// marked with <c>// Phase N pending</c> comments.
/// </para>
/// </summary>
public sealed class LegacyIntegrationTypeAsync : IIntegrationTypeAsync
{
    // ── Product ID ────────────────────────────────────────────────────────────
    private readonly int _productId;

    // ── Product-specific async services ──────────────────────────────────────
    private readonly IManageProductOneSiteAsync              _oneSite;
    private readonly IManageProductMarketingCenterAsync      _marketingCenter;
    private readonly IManageProductOneSiteAccountingAsync    _oneSiteAccounting;
    private readonly IManageProductOpsAsync                  _ops;
    private readonly IManageProductVendorServicesAsync       _vendorServices;
    private readonly IManageProductLead2LeaseAsync           _lead2Lease;
    private readonly IManageProductResidentPortalAsync       _residentPortal;
    private readonly IManageProductOnSiteAsync               _onSite;           // OnSite ≠ OneSite
    private readonly IManageProductRentersInsuranceAsync     _rentersInsurance;
    private readonly IManageProductRumAsync                  _rum;
    private readonly IManageProductAssetOptimizationAsync    _assetOptimization;
    private readonly IManageProductAdminSupportPortalAsync   _adminSupportPortal;
    private readonly IManageProductRealConnectAsync          _realConnect;
    private readonly IManageProductProspectContactAsync      _prospectContact;
    private readonly IManageProductRPDocumentManagementAsync _rpDocumentMgmt;

    // ── Cross-product services ────────────────────────────────────────────────
    private readonly IManageUnifiedLoginAsync _manageUnifiedLogin;
    private readonly IManageProductAsync      _manageProduct;
    private readonly IProductRepositoryAsync  _productRepository;

    // ── StandardV1 fallback dependencies ─────────────────────────────────────
    private readonly IDataCollectorAsync       _dataCollector;
    private readonly IManagePersonaAsync       _managePersona;
    private readonly IManageUserLoginAsync     _manageUserLogin;
    private readonly IUserClaimsAccessor       _userClaimsAccessor;
    private readonly IHttpClientFactory        _httpClientFactory;
    private readonly ITokenHelperAsync         _tokenHelper;
    private readonly ICacheService             _cacheService;
    private readonly ILoggerFactory            _loggerFactory;
    private readonly ISamlAttributeServiceAsync _samlAttributeService;

    public LegacyIntegrationTypeAsync(
        int                                     productId,
        // ── Product services ──────────────────────────────────────────────
        IManageProductOneSiteAsync              oneSite,
        IManageProductMarketingCenterAsync      marketingCenter,
        IManageProductOneSiteAccountingAsync    oneSiteAccounting,
        IManageProductOpsAsync                  ops,
        IManageProductVendorServicesAsync       vendorServices,
        IManageProductLead2LeaseAsync           lead2Lease,
        IManageProductResidentPortalAsync       residentPortal,
        IManageProductOnSiteAsync               onSite,
        IManageProductRentersInsuranceAsync     rentersInsurance,
        IManageProductRumAsync                  rum,
        IManageProductAssetOptimizationAsync    assetOptimization,
        IManageProductAdminSupportPortalAsync   adminSupportPortal,
        IManageProductRealConnectAsync          realConnect,
        IManageProductProspectContactAsync      prospectContact,
        IManageProductRPDocumentManagementAsync rpDocumentMgmt,
        // ── Cross-product ─────────────────────────────────────────────────
        IManageUnifiedLoginAsync                manageUnifiedLogin,
        IManageProductAsync                     manageProduct,
        IProductRepositoryAsync                 productRepository,
        // ── StandardV1 fallback ────────────────────────────────────────────
        IDataCollectorAsync                     dataCollector,
        IManagePersonaAsync                     managePersona,
        IManageUserLoginAsync                   manageUserLogin,
        IUserClaimsAccessor                     userClaimsAccessor,
        IHttpClientFactory                      httpClientFactory,
        ITokenHelperAsync                       tokenHelper,
        ICacheService                           cacheService,
        ILoggerFactory                          loggerFactory,
        ISamlAttributeServiceAsync              samlAttributeService)
    {
        _productId          = productId;
        _oneSite            = oneSite;
        _marketingCenter    = marketingCenter;
        _oneSiteAccounting  = oneSiteAccounting;
        _ops                = ops;
        _vendorServices     = vendorServices;
        _lead2Lease         = lead2Lease;
        _residentPortal     = residentPortal;
        _onSite             = onSite;
        _rentersInsurance   = rentersInsurance;
        _rum                = rum;
        _assetOptimization  = assetOptimization;
        _adminSupportPortal = adminSupportPortal;
        _realConnect        = realConnect;
        _prospectContact    = prospectContact;
        _rpDocumentMgmt     = rpDocumentMgmt;
        _manageUnifiedLogin = manageUnifiedLogin;
        _manageProduct      = manageProduct;
        _productRepository  = productRepository;
        _dataCollector      = dataCollector;
        _managePersona      = managePersona;
        _manageUserLogin    = manageUserLogin;
        _userClaimsAccessor = userClaimsAccessor;
        _httpClientFactory  = httpClientFactory;
        _tokenHelper        = tokenHelper;
        _cacheService       = cacheService;
        _loggerFactory        = loggerFactory;
        _samlAttributeService = samlAttributeService;
    }

    // ── Roles ─────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long userPersonaId,
        long partyId, AccessType? accessType, RequestParameter dataFilter,
        CancellationToken ct = default)
    {
        switch (_productId)
        {
            case (int)ProductEnum.OneSite:
                return await _oneSite.GetRolesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.MarketingCenter:
                return await _marketingCenter.GetRolesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.FinancialSuite:
                return await _oneSiteAccounting.GetUserRolesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.OpsBuyer:
                return await _ops.GetRolesAsync(editorPersonaId, userPersonaId, string.Empty, dataFilter, ct);

            case (int)ProductEnum.VendorServices:
                return await _vendorServices.GetRolesAsync(editorPersonaId, userPersonaId, accessType ?? AccessType.Property, dataFilter, ct);

            case (int)ProductEnum.ClientPortal:
            case (int)ProductEnum.AdminSupportPortal:
                return await _adminSupportPortal.GetRolesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.Lead2Lease:
                return await _lead2Lease.GetRolesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.ResidentPortal:
            {
                var levels = await _residentPortal.ListLevelsAsync(editorPersonaId, userPersonaId, ct);
                return ToListResponse(levels);
            }

            case (int)ProductEnum.OnSite:
                return await _onSite.GetRolesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.Insurance:
                return await _rentersInsurance.ListRolesAsync(editorPersonaId, userPersonaId, ct);

            case (int)ProductEnum.UtilityManagement:
                return await _rum.GetRolesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.AoBusinessIntelligence:
            case (int)ProductEnum.AoInvestmentAnalytics:
            case (int)ProductEnum.AoPerformanceAnalytics:
            case (int)ProductEnum.AoRevenueManagement:
            case (int)ProductEnum.AoBenchmarking:
            case (int)ProductEnum.AoLeaseRentOption:
            case (int)ProductEnum.AoAmenityOptimization:
            case (int)ProductEnum.AoAIRevenueManagement:
            case (int)ProductEnum.AoRentControl:
            case (int)ProductEnum.AoMarketAnalytics:
            case (int)ProductEnum.AoAxiometrics:
            case (int)ProductEnum.AoBIX:
            case (int)ProductEnum.AoLuminaAscent:
                return await _assetOptimization.GetProductRolesAsync(
                    editorPersonaId, userPersonaId,
                    await GetAoProductCodeAsync(ct),
                    dataFilter, cancellationToken: ct);

            case (int)ProductEnum.LeadManagement:
            case (int)ProductEnum.LeadAnalytics:
            case (int)ProductEnum.PortfolioManagement:
            case (int)ProductEnum.DepositAlternative:
            case (int)ProductEnum.ClickPay:
            {
                var sv1 = await CreateStandardV1Async(editorPersonaId, userPersonaId, ct);
                return await sv1.GetProductRolesAsync(dataFilter, ct: ct);
            }

            case (int)ProductEnum.RPDocumentManagement:
                return await _rpDocumentMgmt.GetPropertyRolesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.UnifiedPlatform:
                return await _manageUnifiedLogin.GetUserRolesWithRightsAsync(editorPersonaId, userPersonaId, partyId, ct);

            case (int)ProductEnum.RealConnect:
                return await _realConnect.GetRolesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            default:
                return new ListResponse();
        }
    }

    // ── Properties ────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter dataFilter, CancellationToken ct = default)
    {
        switch (_productId)
        {
            case (int)ProductEnum.OneSite:
                return await _oneSite.GetPropertiesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.MarketingCenter:
                return await _marketingCenter.GetPropertiesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.FinancialSuite:
                return await _oneSiteAccounting.GetUserPropertiesNewAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.OpsBuyer:
                return await _ops.GetCompanyAssetsAsync(editorPersonaId, userPersonaId, includeDisabled: false, dataFilter, ct);

            case (int)ProductEnum.VendorServices:
                return await _vendorServices.GetPropertiesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.ClientPortal:
            case (int)ProductEnum.AdminSupportPortal:
                return await _adminSupportPortal.GetPropertiesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.ProspectContactCenter:
                return await _prospectContact.GetPropertiesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.Lead2Lease:
                return await _lead2Lease.GetPropertiesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.ResidentPortal:
                return await _residentPortal.ListPropertiesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.OnSite:
                return await _onSite.GetPropertiesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.Insurance:
                return await _rentersInsurance.ListPropertiesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.UtilityManagement:
                return await _rum.GetPropertiesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.AoBusinessIntelligence:
            case (int)ProductEnum.AoInvestmentAnalytics:
            case (int)ProductEnum.AoPerformanceAnalytics:
            case (int)ProductEnum.AoRevenueManagement:
            case (int)ProductEnum.AoBenchmarking:
            case (int)ProductEnum.AoLeaseRentOption:
            case (int)ProductEnum.AoAmenityOptimization:
            case (int)ProductEnum.AoAIRevenueManagement:
            case (int)ProductEnum.AoRentControl:
            case (int)ProductEnum.AoBIX:
            case (int)ProductEnum.AoLuminaAscent:
                return await _assetOptimization.GetProductPropertiesAsync(
                    editorPersonaId, userPersonaId,
                    await GetAoProductCodeAsync(ct),
                    dataFilter, cancellationToken: ct);

            case (int)ProductEnum.LeadManagement:
            case (int)ProductEnum.LeadAnalytics:
            case (int)ProductEnum.PortfolioManagement:
            case (int)ProductEnum.DepositAlternative:
            {
                var sv1 = await CreateStandardV1Async(editorPersonaId, userPersonaId, ct);
                return await sv1.GetProductPropertiesAsync(dataFilter, ct: ct);
            }

            case (int)ProductEnum.UnifiedPlatform:
            {
                var settings = await _manageProduct.GetProductInternalSettingsAsync(_productId, ct);
                bool usePropertyInstance = settings
                    .Any(s => s.Name.Equals("UsePropertyInstanceUnifiedLogin", StringComparison.OrdinalIgnoreCase)
                           && s.Value == "1");

                return usePropertyInstance
                    ? await _manageUnifiedLogin.GetUPFMPropertiesAsync(
                          editorPersonaId, userPersonaId, assignedOnly: false,
                          ProductEnum.UnifiedPlatform, dataFilter, ct)
                    : await _manageUnifiedLogin.GetPropertiesAsync(
                          editorPersonaId, userPersonaId, assignedOnly: false, dataFilter, ct);
            }

            case (int)ProductEnum.RealConnect:
                return await _realConnect.GetPropertiesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            default:
                return new ListResponse();
        }
    }

    /// <inheritdoc/>
    public Task<ListResponse> GetEnterprisePropertiesAsync(
        long userPersonaId, RequestParameter dataFilter, CancellationToken ct = default)
        => GetPropertiesAsync(userPersonaId, userPersonaId, dataFilter, ct);

    // ── Property Groups ───────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetPropertyGroupsAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter dataFilter, string userLoginName = "", CancellationToken ct = default)
    {
        switch (_productId)
        {
            case (int)ProductEnum.OnSite:
                return await _onSite.GetRegionsAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.ResidentPortal:
            {
                var groups = await _residentPortal.ListMessageGroupsAsync(editorPersonaId, userPersonaId, ct);
                return ToListResponse(groups);
            }

            case (int)ProductEnum.VendorServices:
                return await _vendorServices.GetPropertyGroupsAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.AoBusinessIntelligence:
            case (int)ProductEnum.AoInvestmentAnalytics:
            case (int)ProductEnum.AoPerformanceAnalytics:
            case (int)ProductEnum.AoRevenueManagement:
            case (int)ProductEnum.AoBenchmarking:
            case (int)ProductEnum.AoLeaseRentOption:
            case (int)ProductEnum.AoAmenityOptimization:
            case (int)ProductEnum.AoAIRevenueManagement:
            case (int)ProductEnum.AoRentControl:
            case (int)ProductEnum.AoMarketAnalytics:
            case (int)ProductEnum.AoAxiometrics:
            case (int)ProductEnum.AoBIX:
            case (int)ProductEnum.AoLuminaAscent:
                return await _assetOptimization.GetProductPropertyGroupsAsync(
                    editorPersonaId, userPersonaId,
                    await GetAoProductCodeAsync(ct),
                    userLoginName, ct);

            case (int)ProductEnum.UtilityManagement:
                return await _rum.GetPropertyGroupsAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            case (int)ProductEnum.DepositAlternative:
            case (int)ProductEnum.LeadAnalytics:
            {
                var sv1 = await CreateStandardV1Async(editorPersonaId, userPersonaId, ct);
                return await sv1.GetProductPropertyGroupsAsync(dataFilter, ct: ct);
            }

            case (int)ProductEnum.FinancialSuite:
                return await _oneSiteAccounting.GetUserCompaniesAsync(editorPersonaId, userPersonaId, dataFilter, ct);

            default:
                return new ListResponse();
        }
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetPropertiesByGroupAsync(
        long editorPersonaId, long userPersonaId,
        string propertyGroupId, RequestParameter dataFilter, CancellationToken ct = default)
    {
        switch (_productId)
        {
            case (int)ProductEnum.AoBusinessIntelligence:
            case (int)ProductEnum.AoInvestmentAnalytics:
            case (int)ProductEnum.AoPerformanceAnalytics:
            case (int)ProductEnum.AoRevenueManagement:
            case (int)ProductEnum.AoBenchmarking:
            case (int)ProductEnum.AoLeaseRentOption:
            case (int)ProductEnum.AoAmenityOptimization:
            case (int)ProductEnum.AoAIRevenueManagement:
            case (int)ProductEnum.AoRentControl:
            case (int)ProductEnum.AoBIX:
            case (int)ProductEnum.AoLuminaAscent:
                return await _assetOptimization.GetGroupPropertiesAsync(
                    editorPersonaId, userPersonaId,
                    Convert.ToInt32(propertyGroupId), _productId, ct);

            case (int)ProductEnum.PortfolioManagement:
            case (int)ProductEnum.LeadAnalytics:
            {
                var sv1 = await CreateStandardV1Async(editorPersonaId, userPersonaId, ct);
                return await sv1.GetProductPropertiesByGroupAsync(propertyGroupId, dataFilter, ct: ct);
            }

            case (int)ProductEnum.FinancialSuite:
                return await _oneSiteAccounting.GetPropertyGroupEntitiesAsync(
                    editorPersonaId, userPersonaId, propertyGroupId, dataFilter, ct);

            default:
                return new ListResponse();
        }
    }

    // ── Organizations ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetOrganizationsAsync(
        long editorPersonaId, long userPersonaId,
        string organizationRoleId, string organizationType, CancellationToken ct = default)
    {
        if (_productId == (int)ProductEnum.ClickPay)
        {
            var sv1 = await CreateStandardV1Async(editorPersonaId, userPersonaId, ct);
            return await sv1.GetProductOrganizationsAsync(organizationRoleId, organizationType, ct: ct);
        }

        return new ListResponse();
    }

    // ── Rights ────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRightsForRoleAsync(
        long editorPersonaId, long userPersonaId,
        long roleId, long partyId, bool assignedToRoleOnly, RequestParameter dataFilter,
        CancellationToken ct = default)
    {
        if (_productId == (int)ProductEnum.OneSite)
            return await _oneSite.GetRightsAsync(editorPersonaId, dataFilter, roleId, assignedToRoleOnly, ct);

        if (_productId == (int)ProductEnum.UnifiedPlatform)
            return await _manageUnifiedLogin.GetRightsByRoleAsync(editorPersonaId, partyId, roleId, ct);

        return new ListResponse();
    }

    /// <inheritdoc/>
    public Task<ListResponse> GetRightsForRoleAsync(
        long editorPersonaId, long userPersonaId,
        string roleId, long partyId, bool assignedToRoleOnly, RequestParameter dataFilter,
        CancellationToken ct = default)
        => Task.FromResult(new ListResponse());

    /// <inheritdoc/>
    public async Task<ListResponse> GetAllRightsAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter dataFilter, CancellationToken ct = default)
    {
        var sv1 = await CreateStandardV1Async(editorPersonaId, userPersonaId, ct);
        return await sv1.GetAllRightsAsync(dataFilter, ct: ct);
    }

    // ── User Groups ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetUserGroupsAsync(
        long editorPersonaId, long userPersonaId,
        long partyId, RequestParameter dataFilter, CancellationToken ct = default)
    {
        var sv1 = await CreateStandardV1Async(editorPersonaId, userPersonaId, ct);
        return await sv1.GetProductUserGroupsAsync(dataFilter, ct: ct);
    }

    // ── User Creation & Mutation ──────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<(string result, List<AdditionalParameters> auditParams)> CreateUserAsync(
        ProductUserProperitiesRoles productUser, CancellationToken ct = default)
    {
        switch (_productId)
        {
            case (int)ProductEnum.OneSite:
            {
                var rpList = TryDeserialize<RolePropertyList>(productUser.InputJson);
                if (rpList is null) return ("Input JSON parsing issue; Null object.", []);
                var (err, log) = await _oneSite.ManageOneSiteUserAsync(
                    productUser.CreateUserPersonaId, productUser.AssignUserPersonaId,
                    rpList.RoleList ?? [], rpList.PropertyList ?? [],
                    isUserProfileChanged: false, ct);
                return (err, log);
            }

            case (int)ProductEnum.MarketingCenter:
            {
                var mcList = TryDeserialize<MarketingCenterRoleAndPropertyList>(productUser.InputJson);
                if (mcList is null) return ("Input JSON parsing issue; Null object.", []);
                var (result, log) = await _marketingCenter.ManageMarketingCenterUserAsync(
                    productUser.CreateUserPersonaId, productUser.AssignUserPersonaId,
                    mcList.RoleList ?? [], mcList.PropertyList ?? [],
                    isAssignedNewPropertyByDefault: false, ct);
                return (result, log);
            }

            case (int)ProductEnum.FinancialSuite:
            {
                var acList = TryDeserialize<AccountingRoleAndPropertyList>(productUser.InputJson);
                if (acList is null) return ("Input JSON parsing issue; Null object.", []);
                var (err, log) = await _oneSiteAccounting.ManageAccountingUserAsync(
                    productUser.CreateUserPersonaId, productUser.AssignUserPersonaId,
                    acList.RoleList ?? [], acList.PropertyList ?? [], acList.CompaniesList ?? [],
                    isAccountingAdmin: acList.IsAccountingAdmin,
                    isSiteSpendManagementUser: false,
                    isUnRestrictedAccessToProp: acList.HasAccessToAllCurrentFutureProperties,
                    productUser.BatchProcessType, ct);
                return (err, log);
            }

            case (int)ProductEnum.OpsBuyer:
            {
                var opsList = TryDeserialize<OpsRoleAndPropertyList>(productUser.InputJson);
                if (opsList is null) return ("Input JSON parsing issue; Null object.", []);
                var (err, log) = await _ops.ManageOpsUserAsync(
                    productUser.CreateUserPersonaId, productUser.AssignUserPersonaId,
                    opsList.RoleList ?? [],
                    opsList.PropertyList ?? [],
                    ct);
                return (err, log);
            }

            case (int)ProductEnum.VendorServices:
            {
                var vsNotification = TryDeserialize<UserProductPropertyNotification>(productUser.InputJson);
                if (vsNotification is null) return ("Input JSON parsing issue; Null object.", []);
                var (result, log) = await _vendorServices.ManageVendorServicesUserAsync(
                    productUser.CreateUserPersonaId, productUser.AssignUserPersonaId,
                    vsNotification, productUser.BatchProcessType, ct);
                return (result, log);
            }

            case (int)ProductEnum.ProspectContactCenter:
            {
                var pccRoleProp = TryDeserialize<ProspectContactPropertyRole>(productUser.InputJson);
                if (pccRoleProp is null) return ("Input JSON parsing issue; Null object.", []);
                var (err, log) = await _prospectContact.ManageProductProspectContactUserAsync(
                    productUser.CreateUserPersonaId, productUser.AssignUserPersonaId,
                    pccRoleProp, productUser.BatchProcessType, ct);
                return (err, log);
            }

            case (int)ProductEnum.Lead2Lease:
            {
                var rpList = TryDeserialize<RolePropertyList>(productUser.InputJson);
                if (rpList is null) return ("Input JSON parsing issue; Null object.", []);
                var (err, log) = await _lead2Lease.ManageLead2LeaseUserAsync(
                    productUser.CreateUserPersonaId, productUser.AssignUserPersonaId,
                    rpList.RoleList ?? [], rpList.PropertyList ?? [], ct);
                return (err, log);
            }

            case (int)ProductEnum.ResidentPortal:
            {
                var rpPortal = TryDeserialize<ResidentPortal>(productUser.InputJson);
                if (rpPortal is null) return ("Input JSON parsing issue; Null object.", []);
                var (output, log) = await _residentPortal.ManageResidentPortalUserAsync(
                    productUser.CreateUserPersonaId, productUser.AssignUserPersonaId,
                    rpPortal, productUser.BatchProcessType, ct);
                return (output?.Status?.Success == false ? output.Status?.ErrorMsg ?? "Error" : string.Empty, log);
            }

            case (int)ProductEnum.OnSite:
            {
                var onSiteRoles = TryDeserialize<OnSiteUserPropertyRegionRole>(productUser.InputJson);
                if (onSiteRoles is null) return ("Input JSON parsing issue; Null object.", []);
                var (err, log) = await _onSite.ManageOnSiteUserAsync(
                    productUser.CreateUserPersonaId, productUser.AssignUserPersonaId,
                    onSiteRoles.PropertyList ?? [], onSiteRoles.RegionList ?? [], onSiteRoles.RoleList ?? [],
                    productUser.BatchProcessType, ct);
                return (err, log);
            }

            case (int)ProductEnum.Insurance:
            {
                var riList = TryDeserialize<RentersInsuranceRoleAndPropertyList>(productUser.InputJson);
                if (riList is null) return ("Input JSON parsing issue; Null object.", []);
                var (output, log) = await _rentersInsurance.ManageRentersInsuranceUserAsync(
                    productUser.CreateUserPersonaId, productUser.AssignUserPersonaId,
                    riList, productUser.BatchProcessType, ct);
                return (output?.Status?.Success == false ? output.Status?.ErrorMsg ?? "Error" : string.Empty, log);
            }

            case (int)ProductEnum.UtilityManagement:
            {
                var rumRoles = TryDeserialize<RumUserPropertyRegionRole>(productUser.InputJson);
                if (rumRoles is null) return ("Input JSON parsing issue; Null object.", []);
                var (result, log) = await _rum.ManageRumUserAsync(
                    productUser.CreateUserPersonaId, productUser.AssignUserPersonaId,
                    rumRoles, ct);
                return (result, log);
            }

            case (int)ProductEnum.RPDocumentManagement:
            {
                var rpList = TryDeserialize<RolePropertyList>(productUser.InputJson);
                if (rpList is null) return ("Input JSON parsing issue; Null object.", []);
                return await _rpDocumentMgmt.ManageRPDMUserAsync(
                    productUser.CreateUserPersonaId, productUser.AssignUserPersonaId,
                    rpList, ct);
            }

            case (int)ProductEnum.LeadManagement:
            case (int)ProductEnum.LeadAnalytics:
            case (int)ProductEnum.PortfolioManagement:
            case (int)ProductEnum.DepositAlternative:
            case (int)ProductEnum.ClickPay:
            case (int)ProductEnum.RealConnect:
            {
                var rpList = TryDeserialize<ProductUserRolePropertiesGroups>(productUser.InputJson);
                if (rpList is null) return ("Input JSON parsing issue; Null object.", []);
                var sv1 = await CreateStandardV1Async(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, ct);
                return rpList.IsAssigned
                    ? await sv1.CreateUpdateProductUserAsync(rpList, productUser.BatchProcessType, ct)
                    : (await sv1.UnassignUserAsync(ct), []);
            }

            default:
                return (string.Empty, []);
        }
    }

    /// <inheritdoc/>
    public async Task<string> ChangeUserTypeAsync(
        ProductUserProperitiesRoles batchRecord, CancellationToken ct = default)
    {
        switch (_productId)
        {
            case (int)ProductEnum.OneSite:
            {
                var rpList = TryDeserialize<RolePropertyList>(batchRecord.InputJson);
                if (rpList is null) return "Input JSON parsing issue; Null object.";
                var (err, _) = await _oneSite.ManageOneSiteUserAsync(
                    batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId,
                    rpList.RoleList ?? [], rpList.PropertyList ?? [],
                    isUserProfileChanged: false, ct);
                return err;
            }

            case (int)ProductEnum.MarketingCenter:
            {
                var mcList = TryDeserialize<MarketingCenterRoleAndPropertyList>(batchRecord.InputJson);
                if (mcList is null) return "Input JSON parsing issue; Null object.";
                var (result, _) = await _marketingCenter.ManageMarketingCenterUserAsync(
                    batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId,
                    mcList.RoleList ?? [], mcList.PropertyList ?? [],
                    isAssignedNewPropertyByDefault: false, ct);
                return result;
            }

            case (int)ProductEnum.FinancialSuite:
            {
                var acList = TryDeserialize<AccountingRoleAndPropertyList>(batchRecord.InputJson);
                if (acList is null) return "Input JSON parsing issue; Null object.";
                var (err, _) = await _oneSiteAccounting.ChangeAccountingServiceUserTypeAsync(
                    batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId,
                    acList.RoleList ?? [], acList.PropertyList ?? [], acList.CompaniesList ?? [],
                    isAccountingAdmin: acList.IsAccountingAdmin,
                    isSiteSpendManagementUser: false,
                    isUnRestrictedAccessToProp: acList.HasAccessToAllCurrentFutureProperties,
                    batchRecord.BatchProcessType, ct);
                return err;
            }

            case (int)ProductEnum.OpsBuyer:
            {
                var opsList = TryDeserialize<OpsRoleAndPropertyList>(batchRecord.InputJson);
                if (opsList is null) return "Input JSON parsing issue; Null object.";
                var (err, _) = await _ops.ManageOpsUserAsync(
                    batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId,
                    opsList.RoleList ?? [],
                    opsList.PropertyList ?? [],
                    ct);
                return err;
            }

            case (int)ProductEnum.VendorServices:
            {
                var vsNotification = TryDeserialize<UserProductPropertyNotification>(batchRecord.InputJson);
                if (vsNotification is null) return "Input JSON parsing issue; Null object.";
                var (result, _) = await _vendorServices.ChangeVendorServicesUserTypeAsync(
                    batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId,
                    vsNotification, batchRecord.BatchProcessType, ct);
                return result;
            }

            case (int)ProductEnum.ProspectContactCenter:
            {
                var pccRoleProp = TryDeserialize<ProspectContactPropertyRole>(batchRecord.InputJson);
                if (pccRoleProp is null) return "Input JSON parsing issue; Null object.";
                var (err, _) = await _prospectContact.ChangeProspectContactUserTypeAsync(
                    batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId,
                    pccRoleProp, batchRecord.BatchProcessType, ct);
                return err;
            }

            case (int)ProductEnum.Lead2Lease:
            {
                var rpList = TryDeserialize<RolePropertyList>(batchRecord.InputJson);
                if (rpList is null) return "Input JSON parsing issue; Null object.";
                var (err, _) = await _lead2Lease.ManageLead2LeaseUserAsync(
                    batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId,
                    rpList.RoleList ?? [], rpList.PropertyList ?? [], ct);
                return err;
            }

            case (int)ProductEnum.ResidentPortal:
            {
                var rpPortal = TryDeserialize<ResidentPortal>(batchRecord.InputJson);
                if (rpPortal is null) return "Input JSON parsing issue; Null object.";
                var (output, _) = await _residentPortal.ManageResidentPortalUserAsync(
                    batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId,
                    rpPortal, batchRecord.BatchProcessType, ct);
                return output?.Status?.Success == false ? output.Status?.ErrorMsg ?? "Error" : string.Empty;
            }

            case (int)ProductEnum.OnSite:
            {
                var onSiteRoles = TryDeserialize<OnSiteUserPropertyRegionRole>(batchRecord.InputJson);
                if (onSiteRoles is null) return "Input JSON parsing issue; Null object.";
                var (err, _) = await _onSite.ChangeOnSiteServiceUserTypeAsync(
                    batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId,
                    onSiteRoles.PropertyList ?? [], onSiteRoles.RegionList ?? [], onSiteRoles.RoleList ?? [],
                    batchRecord.BatchProcessType, ct);
                return err;
            }

            case (int)ProductEnum.Insurance:
            {
                var riList = TryDeserialize<RentersInsuranceRoleAndPropertyList>(batchRecord.InputJson);
                if (riList is null) return "Input JSON parsing issue; Null object.";
                var (output, _) = await _rentersInsurance.ChangeRentersInsuranceUserTypeAsync(
                    batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId,
                    riList, batchRecord.BatchProcessType, ct);
                return output?.Status?.Success == false ? output.Status?.ErrorMsg ?? "Error" : string.Empty;
            }

            case (int)ProductEnum.UtilityManagement:
            {
                var rumRoles = TryDeserialize<RumUserPropertyRegionRole>(batchRecord.InputJson);
                if (rumRoles is null) return "Input JSON parsing issue; Null object.";
                var (result, _) = await _rum.ManageRumUserAsync(
                    batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId,
                    rumRoles, ct);
                return result;
            }

            case (int)ProductEnum.RPDocumentManagement:
            {
                var rpList = TryDeserialize<RolePropertyList>(batchRecord.InputJson);
                if (rpList is null) return "Input JSON parsing issue; Null object.";
                var (result, _) = await _rpDocumentMgmt.ManageRPDMUserAsync(
                    batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId,
                    rpList, ct);
                return result;
            }

            case (int)ProductEnum.LeadManagement:
            case (int)ProductEnum.LeadAnalytics:
            case (int)ProductEnum.PortfolioManagement:
            case (int)ProductEnum.DepositAlternative:
            case (int)ProductEnum.ClickPay:
            {
                var rpList = TryDeserialize<ProductUserRolePropertiesGroups>(batchRecord.InputJson);
                if (rpList is null) return "Input JSON parsing issue; Null object.";
                var sv1 = await CreateStandardV1Async(batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, ct);
                return await sv1.ChangeProductUserTypeAsync(rpList, batchRecord.BatchProcessType, ct);
            }

            default:
                return string.Empty;
        }
    }

    /// <inheritdoc/>
    public async Task<string> UpdateUserProfileAsync(
        ProductUserProperitiesRoles productUser, CancellationToken ct = default)
    {
        return productUser.ProductId switch
        {
            (int)ProductEnum.OneSite        => (await _oneSite.ManageOneSiteUserAsync(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, [], [], isUserProfileChanged: true, ct)).error,
            (int)ProductEnum.MarketingCenter => await _marketingCenter.UpdateUserProfileAsync(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, ct),
            (int)ProductEnum.FinancialSuite  => await _oneSiteAccounting.UpdateAccountingUserProfileAsync(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, ct),
            (int)ProductEnum.OpsBuyer        => await _ops.UpdateOPSUserProfileAsync(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, ct),
            (int)ProductEnum.VendorServices  => await _vendorServices.UpdateVendorServicesUserProfileAsync(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, ct),
            (int)ProductEnum.Lead2Lease      => await _lead2Lease.UpdateLead2LeaseUserProfileAsync(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, ct),
            (int)ProductEnum.ResidentPortal  => await UpdateResidentPortalProfileAsync(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, ct),
            (int)ProductEnum.OnSite          => await _onSite.UpdateOnSiteUserProfileAsync(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, ct),
            (int)ProductEnum.Insurance       => await UpdateRentersInsuranceProfileAsync(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, ct),
            (int)ProductEnum.UtilityManagement => await _rum.UpdateUserProfileAsync(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, ct),
            (int)ProductEnum.ProspectContactCenter => await _prospectContact.UpdateProspectContactCenterUserProfileAsync(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, ct),
            (int)ProductEnum.RPDocumentManagement  => await _rpDocumentMgmt.UpdateRPDMUserProfileAsync(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, ct),
            (int)ProductEnum.RealConnect     => await _realConnect.UpdateProductUserProfileAsync(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, ct),
            (int)ProductEnum.AoBusinessIntelligence or (int)ProductEnum.AoInvestmentAnalytics
            or (int)ProductEnum.AoPerformanceAnalytics or (int)ProductEnum.AoRevenueManagement
            or (int)ProductEnum.AoBenchmarking or (int)ProductEnum.AoLeaseRentOption
            or (int)ProductEnum.AoAmenityOptimization or (int)ProductEnum.AoAIRevenueManagement
            or (int)ProductEnum.AoRentControl or (int)ProductEnum.AoBIX or (int)ProductEnum.AoLuminaAscent
                => await _assetOptimization.UpdateUserProfileAsync(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, ct),
            (int)ProductEnum.LeadManagement or (int)ProductEnum.LeadAnalytics
            or (int)ProductEnum.PortfolioManagement or (int)ProductEnum.DepositAlternative
            or (int)ProductEnum.ClickPay =>
                await (await CreateStandardV1Async(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, ct)).UpdateProductUserProfileAsync(ct),
            (int)ProductEnum.IntegrationMarketplace => string.Empty,
            _ => string.Empty
        };
    }

    /// <inheritdoc/>
    public async Task<string> UpdateUserDetailsAsync(
        ProductUserAccountDetails productUserAccountDetails,
        bool internalChange = false, CancellationToken ct = default)
    {
        long editor  = productUserAccountDetails.PersonaId;
        long subject = productUserAccountDetails.PersonaId;

        return productUserAccountDetails.ProductId switch
        {
            (int)ProductEnum.OneSite        => (await _oneSite.ManageOneSiteUserAsync(editor, subject, [], [], isUserProfileChanged: true, ct)).error,
            (int)ProductEnum.MarketingCenter => await _marketingCenter.UpdateUserProfileAsync(editor, subject, ct),
            (int)ProductEnum.FinancialSuite  => await _oneSiteAccounting.UpdateAccountingUserProfileAsync(editor, subject, ct),
            (int)ProductEnum.OpsBuyer        => await _ops.UpdateOPSUserProfileAsync(editor, subject, ct),
            (int)ProductEnum.VendorServices  => await _vendorServices.UpdateVendorServicesUserProfileAsync(editor, subject, ct),
            (int)ProductEnum.Lead2Lease      => await _lead2Lease.UpdateLead2LeaseUserProfileAsync(editor, subject, ct),
            (int)ProductEnum.ResidentPortal  => await UpdateResidentPortalProfileAsync(editor, subject, ct),
            (int)ProductEnum.OnSite          => await _onSite.UpdateOnSiteUserProfileAsync(editor, subject, ct),
            (int)ProductEnum.Insurance       => await UpdateRentersInsuranceProfileAsync(editor, subject, ct),
            (int)ProductEnum.UtilityManagement => await _rum.UpdateUserProfileAsync(editor, subject, ct),
            (int)ProductEnum.ProspectContactCenter => await _prospectContact.UpdateProspectContactCenterUserProfileAsync(editor, subject, ct),
            (int)ProductEnum.RPDocumentManagement  => await _rpDocumentMgmt.UpdateRPDMUserProfileAsync(editor, subject, ct),
            (int)ProductEnum.RealConnect     => await _realConnect.UpdateProductUserProfileAsync(editor, subject, ct),
            (int)ProductEnum.AoBusinessIntelligence => await _assetOptimization.UpdateUserProfileAsync(editor, subject, ct),
            (int)ProductEnum.LeadManagement or (int)ProductEnum.LeadAnalytics
            or (int)ProductEnum.PortfolioManagement or (int)ProductEnum.DepositAlternative
            or (int)ProductEnum.ClickPay =>
                await (await CreateStandardV1Async(editor, subject, ct)).UpdateProductUserProfileAsync(ct),
            _ => string.Empty
        };
    }

    // ── Migration ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter dataFilter, CancellationToken ct = default)
    {
        return _productId switch
        {
            (int)ProductEnum.OneSite        => await _oneSite.GetMigrationUsersAsync(editorPersonaId, dataFilter, ct),
            (int)ProductEnum.FinancialSuite  => await _oneSiteAccounting.GetMigrationUsersAsync(editorPersonaId, dataFilter, ct),
            (int)ProductEnum.VendorServices  => await _vendorServices.GetMigrationUsersAsync(editorPersonaId, dataFilter, ct),
            (int)ProductEnum.Lead2Lease      => await _lead2Lease.GetMigrationUsersAsync(editorPersonaId, dataFilter, ct),
            (int)ProductEnum.ResidentPortal  => await _residentPortal.GetMigrationUsersAsync(editorPersonaId, dataFilter, ct),
            (int)ProductEnum.OnSite          => await _onSite.GetMigrationUsersAsync(editorPersonaId, dataFilter, ct),
            (int)ProductEnum.Insurance       => await _rentersInsurance.GetMigrationUsersAsync(editorPersonaId, dataFilter, ct),
            (int)ProductEnum.UtilityManagement => await _rum.GetMigrationUsersAsync(editorPersonaId, dataFilter, ct),
            (int)ProductEnum.AoBusinessIntelligence or (int)ProductEnum.AoInvestmentAnalytics
            or (int)ProductEnum.AoPerformanceAnalytics or (int)ProductEnum.AoRevenueManagement
            or (int)ProductEnum.AoBenchmarking or (int)ProductEnum.AoLeaseRentOption
            or (int)ProductEnum.AoAmenityOptimization or (int)ProductEnum.AoAIRevenueManagement
            or (int)ProductEnum.AoRentControl or (int)ProductEnum.AoBIX or (int)ProductEnum.AoLuminaAscent
                => await _assetOptimization.GetMigrationUsersAsync(editorPersonaId, dataFilter, ct),
            (int)ProductEnum.AdminSupportPortal or (int)ProductEnum.SalesForce
                => await _adminSupportPortal.GetMigrationUsersAsync(editorPersonaId, dataFilter, ct),
            (int)ProductEnum.ProspectContactCenter => await _prospectContact.GetMigrationUsersAsync(editorPersonaId, dataFilter, ct),
            (int)ProductEnum.RPDocumentManagement  => await _rpDocumentMgmt.GetMigrationUsersAsync(editorPersonaId, dataFilter, ct),
            _ => await GetMigrationUsersViaStandardV1Async(editorPersonaId, dataFilter, ct)
        };
    }

    /// <inheritdoc/>
    public async Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken ct = default)
    {
        return _productId switch
        {
            (int)ProductEnum.OneSite        => await _oneSite.UpdateUsersMigrationStatusAsync(editorPersonaId, migrateUsers, ct),
            (int)ProductEnum.FinancialSuite  => await _oneSiteAccounting.UpdateUsersMigrationStatusAsync(editorPersonaId, migrateUsers, ct),
            (int)ProductEnum.VendorServices  => await _vendorServices.UpdateUsersMigrationStatusAsync(editorPersonaId, migrateUsers, ct),
            (int)ProductEnum.Lead2Lease      => await _lead2Lease.UpdateUsersMigrationStatusAsync(editorPersonaId, migrateUsers, ct),
            (int)ProductEnum.ResidentPortal  => await _residentPortal.UpdateUsersMigrationStatusAsync(editorPersonaId, migrateUsers, ct),
            (int)ProductEnum.OnSite          => await _onSite.UpdateUsersMigrationStatusAsync(editorPersonaId, migrateUsers, ct),
            (int)ProductEnum.Insurance       => await _rentersInsurance.UpdateUsersMigrationStatusAsync(editorPersonaId, migrateUsers, ct),
            (int)ProductEnum.UtilityManagement => await _rum.UpdateUsersMigrationStatusAsync(editorPersonaId, migrateUsers, ct),
            (int)ProductEnum.AoBusinessIntelligence or (int)ProductEnum.AoInvestmentAnalytics
            or (int)ProductEnum.AoPerformanceAnalytics or (int)ProductEnum.AoRevenueManagement
            or (int)ProductEnum.AoBenchmarking or (int)ProductEnum.AoLeaseRentOption
            or (int)ProductEnum.AoAmenityOptimization or (int)ProductEnum.AoAIRevenueManagement
            or (int)ProductEnum.AoRentControl or (int)ProductEnum.AoBIX or (int)ProductEnum.AoLuminaAscent
                => await _assetOptimization.UpdateUsersMigrationStatusAsync(editorPersonaId, migrateUsers, ct),
            (int)ProductEnum.AdminSupportPortal or (int)ProductEnum.SalesForce
                => await _adminSupportPortal.UpdateUsersMigrationStatusAsync(editorPersonaId, migrateUsers, ct),
            (int)ProductEnum.ProspectContactCenter => await _prospectContact.UpdateUsersMigrationStatusAsync(editorPersonaId, migrateUsers, ct),
            (int)ProductEnum.RPDocumentManagement  => await _rpDocumentMgmt.UpdateUsersMigrationStatusAsync(editorPersonaId, migrateUsers, ct),
            _ => await UpdateMigrationStatusViaStandardV1Async(editorPersonaId, migrateUsers, ct)
        };
    }

    // ── External Profile Sync ─────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<bool> ExternalUserProfileChangeAsync(
        long editorPersonaId, ProductUserProfile productUserProfile, CancellationToken ct = default)
    {
        var sv1 = await CreateStandardV1Async(editorPersonaId, editorPersonaId, ct);
        return await sv1.ExternalProductUserProfileChangeAsync(productUserProfile, ct);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Creates and initialises the correct <see cref="StandardV1ProductIntegrationAsync"/>
    /// subclass for the current product, scoped to the given personas.
    /// Delegates to <see cref="ManageProductFactoryAsync.CreateAndInitAsync"/> which
    /// selects <see cref="LeadManagementAsync"/>, <see cref="PortfolioManagementAsync"/>,
    /// <see cref="DepositAlternativeManagementAsync"/>, <see cref="ClickPayManagementAsync"/>,
    /// or <see cref="SelfGuidedTourAsync"/> based on <c>_productId</c>.
    /// </summary>
    private Task<StandardV1ProductIntegrationAsync> CreateStandardV1Async(
        long editorPersonaId, long subjectPersonaId, CancellationToken ct)
        => ManageProductFactoryAsync.CreateAndInitAsync(
               _productId, editorPersonaId, subjectPersonaId,
               _dataCollector, _productRepository, _managePersona, _manageUserLogin,
               _userClaimsAccessor, _httpClientFactory, _tokenHelper, _cacheService, _loggerFactory,
               _samlAttributeService, ct);

    /// <summary>
    /// Resolves the BooksProductCode for Asset Optimization products from the repository.
    /// </summary>
    private async Task<string> GetAoProductCodeAsync(CancellationToken ct)
    {
        var products = await _productRepository.GetAllProductsAsync(ct);
        return ProductEnumHelper.GetBooksSourceCodeByProductId(_productId, products);
    }

    private async Task<ListResponse> GetMigrationUsersViaStandardV1Async(
        long editorPersonaId, RequestParameter dataFilter, CancellationToken ct)
    {
        var sv1 = await CreateStandardV1Async(editorPersonaId, editorPersonaId, ct);
        return await sv1.GetMigrationUsersAsync(dataFilter, ct);
    }

    private async Task<MigrateResponse> UpdateMigrationStatusViaStandardV1Async(
        long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken ct)
    {
        var sv1 = await CreateStandardV1Async(editorPersonaId, editorPersonaId, ct);
        return await sv1.UpdateUsersMigrationStatusAsync(migrateUsers, ct);
    }

    /// <summary>
    /// ResidentPortal profile update — uses <c>ManageResidentPortalUserAsync</c> with
    /// an empty payload (profile-only update path handles the rest internally).
    /// </summary>
    private async Task<string> UpdateResidentPortalProfileAsync(
        long editorPersonaId, long userPersonaId, CancellationToken ct)
    {
        var (output, _) = await _residentPortal.ManageResidentPortalUserAsync(
            editorPersonaId, userPersonaId,
            new ResidentPortal(), BatchProcessType.ProfileUpdate, ct);
        return output?.Status?.Success == false ? output.Status?.ErrorMsg ?? "Error" : string.Empty;
    }

    /// <summary>
    /// RentersInsurance profile update — uses <c>ManageRentersInsuranceUserAsync</c>
    /// with an empty payload (profile-only update path handles the rest internally).
    /// </summary>
    private async Task<string> UpdateRentersInsuranceProfileAsync(
        long editorPersonaId, long userPersonaId, CancellationToken ct)
    {
        var (output, _) = await _rentersInsurance.ManageRentersInsuranceUserAsync(
            editorPersonaId, userPersonaId,
            new RentersInsuranceRoleAndPropertyList(), BatchProcessType.ProfileUpdate, ct);
        return output?.Status?.Success == false ? output.Status?.ErrorMsg ?? "Error" : string.Empty;
    }

    private static ListResponse ToListResponse<T>(IList<T> items) where T : class
    {
        if (items is null) return new ListResponse();
        return new ListResponse
        {
            Records      = items.Cast<object>().ToList(),
            TotalRows    = items.Count,
            RowsPerPage  = items.Count,
            TotalPages   = 1,
            ErrorReason  = string.Empty
        };
    }

    private static T? TryDeserialize<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;
        try { return System.Text.Json.JsonSerializer.Deserialize<T>(json.Trim()); }
        catch { return default; }
    }
}
