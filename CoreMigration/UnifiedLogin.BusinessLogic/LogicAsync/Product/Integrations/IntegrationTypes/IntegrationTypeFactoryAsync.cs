using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.IntegrationTypes;

/// <summary>
/// Native-async implementation of <see cref="IIntegrationTypeFactoryAsync"/>.
/// Replaces <c>IntegrationTypeFactory</c> (sync).
/// <para>
/// Reads the <c>ProductIntegrationType</c> internal setting once per factory instance
/// (lazy, thread-safe) to build a <c>productId → <see cref="ProductIntegrationTypeEnum"/></c>
/// dictionary, then instantiates the correct wrapper on each call:
/// <list type="bullet">
///   <item><see cref="LegacyIntegrationTypeAsync"/> for <c>Legacy</c> products</item>
///   <item><see cref="UPFMIntegrationTypeAsync"/> for <c>UPFM</c> products</item>
///   <item><see cref="StandardV1IntegrationTypeAsync"/> for <c>Standard v1</c> products</item>
/// </list>
/// </para>
/// <para>
/// Factory-resolution methods are synchronous (no <c>Task</c> return) — all I/O is
/// confined to the lazy initialisation of the internal type map, performed once on the
/// first call to <see cref="GetIntegration"/> or <see cref="GetIntegrationTypeForProductId"/>.
/// </para>
/// </summary>
public sealed class IntegrationTypeFactoryAsync : IIntegrationTypeFactoryAsync
{
    // ── String → enum map (case-insensitive, matches sync factory) ────────────
    private static readonly IReadOnlyDictionary<string, ProductIntegrationTypeEnum> _stringToEnum =
        new Dictionary<string, ProductIntegrationTypeEnum>(StringComparer.OrdinalIgnoreCase)
        {
            ["Legacy"]      = ProductIntegrationTypeEnum.Legacy,
            ["UPFM"]        = ProductIntegrationTypeEnum.UPFM,
            ["Standard v1"] = ProductIntegrationTypeEnum.StandardV1
        };

    // ── Setting repository ────────────────────────────────────────────────────
    private readonly IProductInternalSettingRepositoryAsync _productInternalSettingRepo;

    // ── Legacy wrapper dependencies ───────────────────────────────────────────
    private readonly IManageProductOneSiteAsync              _oneSite;
    private readonly IManageProductMarketingCenterAsync      _marketingCenter;
    private readonly IManageProductOneSiteAccountingAsync    _oneSiteAccounting;
    private readonly IManageProductOpsAsync                  _ops;
    private readonly IManageProductVendorServicesAsync       _vendorServices;
    private readonly IManageProductLead2LeaseAsync           _lead2Lease;
    private readonly IManageProductResidentPortalAsync       _residentPortal;
    private readonly IManageProductOnSiteAsync               _onSite;
    private readonly IManageProductRentersInsuranceAsync     _rentersInsurance;
    private readonly IManageProductRumAsync                  _rum;
    private readonly IManageProductAssetOptimizationAsync    _assetOptimization;
    private readonly IManageProductAdminSupportPortalAsync   _adminSupportPortal;
    private readonly IManageProductRealConnectAsync          _realConnect;
    private readonly IManageProductProspectContactAsync      _prospectContact;
    private readonly IManageProductRPDocumentManagementAsync _rpDocumentMgmt;
    private readonly IManageUnifiedLoginAsync                _manageUnifiedLogin;
    private readonly IManageProductAsync                     _manageProduct;
    private readonly IProductRepositoryAsync                 _productRepository;
    private readonly ISamlAttributeServiceAsync              _samlAttributeService;

    // ── UPFM wrapper dependency ───────────────────────────────────────────────
    private readonly IManageUPFMProductsIntegrationAsync _upfm;

    // ── StandardV1 / fallback dependencies ───────────────────────────────────
    private readonly IDataCollectorAsync    _dataCollector;
    private readonly IManagePersonaAsync    _managePersona;
    private readonly IManageUserLoginAsync  _manageUserLogin;
    private readonly IUserClaimsAccessor    _userClaimsAccessor;
    private readonly IHttpClientFactory     _httpClientFactory;
    private readonly ITokenHelperAsync      _tokenHelper;
    private readonly ICacheService          _cacheService;
    private readonly ILoggerFactory         _loggerFactory;

    // ── Lazy product-id → integration-type map ────────────────────────────────
    private readonly Lazy<IReadOnlyDictionary<int, ProductIntegrationTypeEnum>> _typeMap;

    public IntegrationTypeFactoryAsync(
        IProductInternalSettingRepositoryAsync  productInternalSettingRepo,
        // ── Legacy product services ──────────────────────────────────────────
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
        IManageUnifiedLoginAsync                manageUnifiedLogin,
        IManageProductAsync                     manageProduct,
        IProductRepositoryAsync                 productRepository,
        ISamlAttributeServiceAsync              samlAttributeService,
        // ── UPFM ────────────────────────────────────────────────────────────
        IManageUPFMProductsIntegrationAsync     upfm,
        // ── StandardV1 / fallback ────────────────────────────────────────────
        IDataCollectorAsync                     dataCollector,
        IManagePersonaAsync                     managePersona,
        IManageUserLoginAsync                   manageUserLogin,
        IUserClaimsAccessor                     userClaimsAccessor,
        IHttpClientFactory                      httpClientFactory,
        ITokenHelperAsync                       tokenHelper,
        ICacheService                           cacheService,
        ILoggerFactory                          loggerFactory)
    {
        _productInternalSettingRepo = productInternalSettingRepo;
        _oneSite                    = oneSite;
        _marketingCenter            = marketingCenter;
        _oneSiteAccounting          = oneSiteAccounting;
        _ops                        = ops;
        _vendorServices             = vendorServices;
        _lead2Lease                 = lead2Lease;
        _residentPortal             = residentPortal;
        _onSite                     = onSite;
        _rentersInsurance           = rentersInsurance;
        _rum                        = rum;
        _assetOptimization          = assetOptimization;
        _adminSupportPortal         = adminSupportPortal;
        _realConnect                = realConnect;
        _prospectContact            = prospectContact;
        _rpDocumentMgmt             = rpDocumentMgmt;
        _manageUnifiedLogin         = manageUnifiedLogin;
        _manageProduct              = manageProduct;
        _productRepository          = productRepository;
        _samlAttributeService       = samlAttributeService;
        _upfm                       = upfm;
        _dataCollector              = dataCollector;
        _managePersona              = managePersona;
        _manageUserLogin            = manageUserLogin;
        _userClaimsAccessor         = userClaimsAccessor;
        _httpClientFactory          = httpClientFactory;
        _tokenHelper                = tokenHelper;
        _cacheService               = cacheService;
        _loggerFactory              = loggerFactory;

        _typeMap = new Lazy<IReadOnlyDictionary<int, ProductIntegrationTypeEnum>>(
            BuildTypeMap, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    // ── IIntegrationTypeFactoryAsync ──────────────────────────────────────────

    /// <inheritdoc/>
    public IIntegrationTypeAsync GetIntegration(int productId)
        => GetIntegrationTypeForProductId(productId) switch
        {
            ProductIntegrationTypeEnum.UPFM       => CreateUPFM(productId),
            ProductIntegrationTypeEnum.StandardV1  => CreateStandardV1(productId),
            _                                      => CreateLegacy(productId)
        };

    /// <inheritdoc/>
    public IIntegrationTypeAsync GetIntegrationStandardV1(int productId)
        => CreateStandardV1(productId);

    /// <inheritdoc/>
    public ProductIntegrationTypeEnum GetIntegrationTypeForProductId(int productId)
        => _typeMap.Value.TryGetValue(productId, out var type)
               ? type
               : ProductIntegrationTypeEnum.Legacy;

    // ── Private wrapper factories ─────────────────────────────────────────────

    private LegacyIntegrationTypeAsync CreateLegacy(int productId)
        => new(productId,
               _oneSite, _marketingCenter, _oneSiteAccounting, _ops, _vendorServices,
               _lead2Lease, _residentPortal, _onSite, _rentersInsurance, _rum,
               _assetOptimization, _adminSupportPortal, _realConnect, _prospectContact, _rpDocumentMgmt,
               _manageUnifiedLogin, _manageProduct, _productRepository,
               _dataCollector, _managePersona, _manageUserLogin, _userClaimsAccessor,
               _httpClientFactory, _tokenHelper, _cacheService, _loggerFactory,
               _samlAttributeService);

    private UPFMIntegrationTypeAsync CreateUPFM(int productId)
        => new(productId, _upfm, _productRepository);

    private StandardV1IntegrationTypeAsync CreateStandardV1(int productId)
        => new(productId,
               _dataCollector, _productRepository, _managePersona, _manageUserLogin,
               _userClaimsAccessor, _httpClientFactory, _tokenHelper, _cacheService, _loggerFactory);

    // ── Lazy map builder ──────────────────────────────────────────────────────

    /// <summary>
    /// Synchronously reads the <c>ProductIntegrationType</c> settings from the DB and
    /// builds the product-ID–to–enum lookup.  Called once (thread-safely) on first access
    /// via the <see cref="_typeMap"/> <see cref="Lazy{T}"/>.
    /// </summary>
    private IReadOnlyDictionary<int, ProductIntegrationTypeEnum> BuildTypeMap()
    {
        var settings = _productInternalSettingRepo
            .GetProductSettingByTypeAsync("ProductIntegrationType")
            .GetAwaiter().GetResult();

        var map = new Dictionary<int, ProductIntegrationTypeEnum>();
        foreach (ProductInternalSettingByType setting in settings ?? [])
        {
            if (setting.Value is not null
                && _stringToEnum.TryGetValue(setting.Value, out var enumVal))
            {
                map[setting.ProductId] = enumVal;
            }
        }
        return map;
    }
}
