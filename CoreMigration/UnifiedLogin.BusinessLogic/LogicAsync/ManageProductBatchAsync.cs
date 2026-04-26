using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.ProductImplementation;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

public sealed class ManageProductBatchAsync : IManageProductBatchAsync
{
    // ── Repositories ───────────────────────────────────────────────────────
    private readonly IPropertyRepositoryAsync                _propertyRepo;
    private readonly IProductInternalSettingRepositoryAsync  _productInternalSettingRepo;
    private readonly IUserRoleRightRepositoryAsync           _userRoleRightRepo;
    private readonly ISharedDataRepositoryAsync              _sharedDataRepo;
    private readonly IProductRepositoryAsync                 _productRepo;

    // ── Panel + integration ────────────────────────────────────────────────
    private readonly IManageProductPanelAsync                _manageProductPanel;
    private readonly IIntegrationTypeFactoryAsync            _integrationTypeFactory;

    // ── Product-specific services ──────────────────────────────────────────
    private readonly IManageProductOneSiteAccountingAsync    _accounting;
    private readonly IManageProductVendorServicesAsync       _vendorServices;
    private readonly IManageProductResidentPortalAsync       _residentPortal;
    private readonly IManageProductOnSiteAsync               _onSite;
    private readonly IManageProductRumAsync                  _rum;

    // ── Factory deps (for StandardV1 product integrations) ────────────────
    private readonly IDataCollectorAsync                     _dataCollector;
    private readonly IManagePersonaAsync                     _managePersona;
    private readonly IManageUserLoginAsync                   _manageUserLogin;
    private readonly IHttpClientFactory                      _httpClientFactory;
    private readonly ITokenHelperAsync                       _tokenHelper;
    private readonly ICacheService                           _cacheService;
    private readonly ILoggerFactory                          _loggerFactory;
    private readonly ISamlAttributeServiceAsync              _samlAttributeService;

    // ── Infrastructure ─────────────────────────────────────────────────────
    private readonly IUserClaimsAccessor                     _userClaims;
    private readonly IMemoryCache                            _memoryCache;
    private readonly ILogger<ManageProductBatchAsync>        _logger;

    public ManageProductBatchAsync(
        IPropertyRepositoryAsync                propertyRepo,
        IProductInternalSettingRepositoryAsync  productInternalSettingRepo,
        IUserRoleRightRepositoryAsync           userRoleRightRepo,
        ISharedDataRepositoryAsync              sharedDataRepo,
        IProductRepositoryAsync                 productRepo,
        IManageProductPanelAsync                manageProductPanel,
        IIntegrationTypeFactoryAsync            integrationTypeFactory,
        IManageProductOneSiteAccountingAsync    accounting,
        IManageProductVendorServicesAsync       vendorServices,
        IManageProductResidentPortalAsync       residentPortal,
        IManageProductOnSiteAsync               onSite,
        IManageProductRumAsync                  rum,
        IDataCollectorAsync                     dataCollector,
        IManagePersonaAsync                     managePersona,
        IManageUserLoginAsync                   manageUserLogin,
        IHttpClientFactory                      httpClientFactory,
        ITokenHelperAsync                       tokenHelper,
        ICacheService                           cacheService,
        ILoggerFactory                          loggerFactory,
        ISamlAttributeServiceAsync              samlAttributeService,
        IUserClaimsAccessor                     userClaims,
        IMemoryCache                            memoryCache,
        ILogger<ManageProductBatchAsync>        logger)
    {
        _propertyRepo               = propertyRepo               ?? throw new ArgumentNullException(nameof(propertyRepo));
        _productInternalSettingRepo = productInternalSettingRepo ?? throw new ArgumentNullException(nameof(productInternalSettingRepo));
        _userRoleRightRepo          = userRoleRightRepo          ?? throw new ArgumentNullException(nameof(userRoleRightRepo));
        _sharedDataRepo             = sharedDataRepo             ?? throw new ArgumentNullException(nameof(sharedDataRepo));
        _productRepo                = productRepo                ?? throw new ArgumentNullException(nameof(productRepo));
        _manageProductPanel         = manageProductPanel         ?? throw new ArgumentNullException(nameof(manageProductPanel));
        _integrationTypeFactory     = integrationTypeFactory     ?? throw new ArgumentNullException(nameof(integrationTypeFactory));
        _accounting                 = accounting                 ?? throw new ArgumentNullException(nameof(accounting));
        _vendorServices             = vendorServices             ?? throw new ArgumentNullException(nameof(vendorServices));
        _residentPortal             = residentPortal             ?? throw new ArgumentNullException(nameof(residentPortal));
        _onSite                     = onSite                     ?? throw new ArgumentNullException(nameof(onSite));
        _rum                        = rum                        ?? throw new ArgumentNullException(nameof(rum));
        _dataCollector              = dataCollector              ?? throw new ArgumentNullException(nameof(dataCollector));
        _managePersona              = managePersona              ?? throw new ArgumentNullException(nameof(managePersona));
        _manageUserLogin            = manageUserLogin            ?? throw new ArgumentNullException(nameof(manageUserLogin));
        _httpClientFactory          = httpClientFactory          ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _tokenHelper                = tokenHelper                ?? throw new ArgumentNullException(nameof(tokenHelper));
        _cacheService               = cacheService               ?? throw new ArgumentNullException(nameof(cacheService));
        _loggerFactory              = loggerFactory              ?? throw new ArgumentNullException(nameof(loggerFactory));
        _samlAttributeService       = samlAttributeService       ?? throw new ArgumentNullException(nameof(samlAttributeService));
        _userClaims                 = userClaims                 ?? throw new ArgumentNullException(nameof(userClaims));
        _memoryCache                = memoryCache                ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger                     = logger                     ?? throw new ArgumentNullException(nameof(logger));
    }

    // ── Public methods ─────────────────────────────────────────────────────

    public async Task<ProductBatch> GetProductBatchRecordAsync(
        long editorPersonaId,
        long subjectPersonaId,
        IList<ProductRole> productRoles,
        ListResponse propertiesResponse,
        ListResponse rolesResponse,
        int product,
        bool usePrimaryProperties,
        CancellationToken cancellationToken = default)
    {
        switch ((ProductEnum)product)
        {
            case ProductEnum.FinancialSuite:
            {
                var propGroupTask = _accounting.GetUserPropertyGroupsAsync(editorPersonaId, subjectPersonaId, null, cancellationToken);
                var companiesTask = _accounting.GetUserCompaniesAsync(editorPersonaId, subjectPersonaId, null, cancellationToken);
                await Task.WhenAll(propGroupTask, companiesTask).ConfigureAwait(false);
                return BatchHelper.CreateFinancialSuiteProductBatchRecord(
                    propertiesResponse, rolesResponse, product,
                    companiesTask.Result, propGroupTask.Result, usePrimaryProperties);
            }

            case ProductEnum.VendorServices:
            {
                var notificationsTask    = _vendorServices.GetNotificationSettingsAsync(editorPersonaId, subjectPersonaId, cancellationToken);
                var propertyGroupsTask   = _vendorServices.GetPropertyGroupsAsync(editorPersonaId, subjectPersonaId, null, cancellationToken);
                await Task.WhenAll(notificationsTask, propertyGroupsTask).ConfigureAwait(false);
                return BatchHelper.CreateVendorServiceProductBatchRecord(
                    propertiesResponse, rolesResponse, propertyGroupsTask.Result,
                    notificationsTask.Result, product, usePrimaryProperties);
            }

            case ProductEnum.ResidentPortal:
            {
                var levelList = productRoles
                    .Select(r => (ILevel)new Level { Id = r.ID, Name = r.Name, IsAssigned = r.IsAssigned })
                    .ToList();

                var notificationsTask   = _residentPortal.GetNotificationSettingsAsync(editorPersonaId, subjectPersonaId, cancellationToken);
                var messagingGroupsTask = _residentPortal.ListMessageGroupsAsync(editorPersonaId, subjectPersonaId, cancellationToken);
                await Task.WhenAll(notificationsTask, messagingGroupsTask).ConfigureAwait(false);
                return BatchHelper.CreateResidentPortalProductBatchRecord(
                    propertiesResponse, levelList,
                    notificationsTask.Result, messagingGroupsTask.Result,
                    product, usePrimaryProperties);
            }

            case ProductEnum.OnSite:
            {
                var regionResponse = await _onSite
                    .GetRegionsAsync(editorPersonaId, subjectPersonaId, null, cancellationToken)
                    .ConfigureAwait(false);
                return BatchHelper.CreateOnSiteBatchRecord(propertiesResponse, rolesResponse, regionResponse, product, usePrimaryProperties);
            }

            case ProductEnum.DepositAlternative:
            {
                var productLogic = await ManageProductFactoryAsync.CreateAndInitAsync(
                    product, editorPersonaId, subjectPersonaId,
                    _dataCollector, _productRepo, _managePersona, _manageUserLogin,
                    _userClaims, _httpClientFactory, _tokenHelper, _cacheService,
                    _loggerFactory, _samlAttributeService, cancellationToken)
                    .ConfigureAwait(false);
                var productUser = (await productLogic.GetProductUserAsync(ct: cancellationToken).ConfigureAwait(false))!;
                productUser.RoleList = productRoles.Select(p => p.ID).ToList();
                return BatchHelper.CreateProductBatchRecordForDepositIQ(productUser, usePrimaryProperties);
            }

            case ProductEnum.IntegrationMarketplace:
            {
                var existingRoleId = Convert.ToInt32(productRoles.Select(p => p.ID).FirstOrDefault());
                return BatchHelper.CreateIntegrationMarketplaceBatchRecord(existingRoleId, product, usePrimaryProperties);
            }

            case ProductEnum.LeadManagement:
            {
                var productLogic = await ManageProductFactoryAsync.CreateAndInitAsync(
                    product, editorPersonaId, subjectPersonaId,
                    _dataCollector, _productRepo, _managePersona, _manageUserLogin,
                    _userClaims, _httpClientFactory, _tokenHelper, _cacheService,
                    _loggerFactory, _samlAttributeService, cancellationToken)
                    .ConfigureAwait(false);
                var productUser = (await productLogic.GetProductUserAsync(ct: cancellationToken).ConfigureAwait(false))!;
                return BatchHelper.CreateILMProductBatchRecord(
                    ProductEnum.LeadManagement,
                    productUser.Properties,
                    productRoles.Select(p => p.ID).ToList(),
                    [],
                    usePrimaryProperties);
            }

            case ProductEnum.LeadAnalytics:
            {
                var productLogic = await ManageProductFactoryAsync.CreateAndInitAsync(
                    product, editorPersonaId, subjectPersonaId,
                    _dataCollector, _productRepo, _managePersona, _manageUserLogin,
                    _userClaims, _httpClientFactory, _tokenHelper, _cacheService,
                    _loggerFactory, _samlAttributeService, cancellationToken)
                    .ConfigureAwait(false);
                var productUser = (await productLogic.GetProductUserAsync(ct: cancellationToken).ConfigureAwait(false))!;
                return BatchHelper.CreateILMProductBatchRecord(
                    ProductEnum.LeadAnalytics,
                    productUser.Properties,
                    productRoles.Select(p => p.ID).ToList(),
                    productUser.PropertyGroups,
                    usePrimaryProperties);
            }

            case ProductEnum.PortfolioManagement:
            {
                var productLogic = await ManageProductFactoryAsync.CreateAndInitAsync(
                    product, editorPersonaId, subjectPersonaId,
                    _dataCollector, _productRepo, _managePersona, _manageUserLogin,
                    _userClaims, _httpClientFactory, _tokenHelper, _cacheService,
                    _loggerFactory, _samlAttributeService, cancellationToken)
                    .ConfigureAwait(false);
                var productUser = (await productLogic.GetProductUserAsync(ct: cancellationToken).ConfigureAwait(false))!;
                return BatchHelper.CreateProductBatchRecordForPortfolioManagement(
                    productUser.PropertyRoleList,
                    productRoles.Select(p => p.ID).ToList(),
                    usePrimaryProperties);
            }

            case ProductEnum.UtilityManagement:
            {
                var propGroupsTask  = _rum.GetPropertyGroupsAsync(editorPersonaId, subjectPersonaId, null, cancellationToken);
                var regionsTask     = _rum.GetRegionsAsync(editorPersonaId, subjectPersonaId, null, cancellationToken);
                await Task.WhenAll(propGroupsTask, regionsTask).ConfigureAwait(false);
                return BatchHelper.CreateRumProductBatchRecord(
                    propertiesResponse, propGroupsTask.Result,
                    regionsTask.Result, rolesResponse, usePrimaryProperties);
            }

            default:
            {
                var integrationType = _integrationTypeFactory.GetIntegrationTypeForProductId(product);
                return BatchHelper.CreateProductBatchRecord(
                    propertiesResponse, rolesResponse, product, usePrimaryProperties, integrationType);
            }
        }
    }

    public async Task<ListResponse> GetUserPrimaryPropertiesDataAsync(
        long editorPersonaId,
        long userPersonaId,
        int productId,
        CancellationToken cancellationToken = default)
    {
        // Load user properties and panel properties concurrently.
        var userPropsTask = _propertyRepo.ListUPFMPropertyInstanceByPersonaAsync(userPersonaId, ProductEnum.UnifiedPlatform, cancellationToken);
        var panelTask     = _manageProductPanel.GetProductPropertiesAsync(editorPersonaId, userPersonaId, productId, null, cancellationToken);
        await Task.WhenAll(userPropsTask, panelTask).ConfigureAwait(false);

        var result = panelTask.Result;
        if (!result.IsError)
        {
            var upfmProperty = new UPFMProperty
            {
                id = userPropsTask.Result?.Select(p => p.InstanceId.ToString()).ToList()
            };
            result = await _manageProductPanel
                .CompareProductAndPrimaryPropertiesAsync(upfmProperty, productId, result, cancellationToken)
                .ConfigureAwait(false);
        }

        return result;
    }

    public async Task<ListResponse> GetEnterpriseRoleUserPrimaryPropertiesDataAsync(
        long editorPersonaId,
        long userPersonaId,
        int productId,
        bool usePrimaryProperties = true,
        CancellationToken cancellationToken = default)
    {
        var result = new ListResponse();

        if (productId == (int)ProductEnum.KnockCRM)
        {
            // KnockCRM: properties come directly from UPFM assignments, no panel call.
            var userProperties = await _propertyRepo
                .ListUPFMPropertyInstanceByPersonaAsync(userPersonaId, ProductEnum.UnifiedPlatform, cancellationToken)
                .ConfigureAwait(false);

            if (usePrimaryProperties)
            {
                result = new ListResponse
                {
                    Records   = userProperties.Cast<object>().ToList(),
                    TotalRows = userProperties.Count
                };
                var upfmProperty = new UPFMProperty { id = userProperties.Select(p => p.InstanceId.ToString()).ToList() };
                result = await _manageProductPanel
                    .CompareProductAndPrimaryPropertiesAsync(upfmProperty, productId, result, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }

        // All other products: load user properties and panel properties concurrently.
        var userPropsTask = _propertyRepo.ListUPFMPropertyInstanceByPersonaAsync(userPersonaId, ProductEnum.UnifiedPlatform, cancellationToken);
        var panelTask     = _manageProductPanel.GetProductPropertiesAsync(editorPersonaId, userPersonaId, productId, null, cancellationToken);
        await Task.WhenAll(userPropsTask, panelTask).ConfigureAwait(false);

        result = panelTask.Result;
        if (!result.IsError && usePrimaryProperties)
        {
            var upfmProperty = new UPFMProperty
            {
                id = userPropsTask.Result?.Select(p => p.InstanceId.ToString()).ToList()
            };
            result = await _manageProductPanel
                .CompareProductAndPrimaryPropertiesAsync(upfmProperty, productId, result, cancellationToken)
                .ConfigureAwait(false);
        }

        return result;
    }

    public Task<List<int>> GetExistingUserPrimaryPropertiesDataAsync(
        long userPersonaId,
        int productId,
        CancellationToken cancellationToken = default)
        => _propertyRepo.ListUPFMPropertyInstanceIdByPersonaAsync(userPersonaId, productId, cancellationToken);

    public Task<ListResponse> GetProductPropertiesAsync(
        long editorPersonaId,
        long userPersonaId,
        int productId,
        CancellationToken cancellationToken = default)
        => _manageProductPanel.GetProductPropertiesAsync(editorPersonaId, userPersonaId, productId, null, cancellationToken);

    public Task<ListResponse> GetProductRolesAsync(
        long editorPersonaId,
        long userPersonaId,
        int productId,
        long partyId,
        CancellationToken cancellationToken = default)
        => _manageProductPanel.GetProductRolesAsync(editorPersonaId, userPersonaId, partyId, productId, null, null, cancellationToken);

    public async Task<bool> IsProductEnabledForUsePrimaryPropertyAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var settings = await _productInternalSettingRepo
            .GetProductInternalSettingsAsync(productId, cancellationToken)
            .ConfigureAwait(false);

        var setting = settings.FirstOrDefault(s =>
            s.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase));

        return setting?.Value?.Trim() == "1";
    }

    public async Task<List<string>> GetPersonaRoleRightsAsync(
        long personaId,
        long orgPartyId,
        CancellationToken cancellationToken = default)
    {
        var userRoles = await _userRoleRightRepo
            .ListRoleByPersonaAsync((int)ProductEnum.UnifiedPlatform, personaId, orgPartyId)
            .ConfigureAwait(false);

        var cacheKey = $"enterpriseRoleProcessgetRolesByParty_{orgPartyId}_{(int)ProductEnum.UnifiedPlatform}";

        if (!_memoryCache.TryGetValue(cacheKey, out IList<UserRoleRights> roleList))
        {
            var productIds = await _sharedDataRepo
                .GetProductIdsByCompanyAsync(orgPartyId, cancellationToken)
                .ConfigureAwait(false);

            roleList = await _userRoleRightRepo
                .GetAllRoleRightsAsync(orgPartyId, productIds, (int)ProductEnum.UnifiedPlatform)
                .ConfigureAwait(false);

            _memoryCache.Set(cacheKey, roleList, TimeSpan.FromMinutes(60));
        }

        // Use HashSet for O(1) duplicate detection.
        var userRights = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var userRole in userRoles)
        {
            var matchedRole = roleList?.FirstOrDefault(r => r.RoleId == userRole.RoleID);
            if (matchedRole?.UserRights is null)
                continue;

            foreach (var right in matchedRole.UserRights)
            {
                if (!string.IsNullOrWhiteSpace(right.RightNickName))
                    userRights.Add(right.RightNickName);
            }
        }

        return [.. userRights];
    }
}
