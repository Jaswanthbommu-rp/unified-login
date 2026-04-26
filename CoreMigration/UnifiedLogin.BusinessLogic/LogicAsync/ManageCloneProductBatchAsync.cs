using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.Model.ClickPay;
using UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.ProductImplementation;
using UnifiedLogin.BusinessLogic.LogicAsync.Helper;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Accounting;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;
using UnifiedLogin.SharedObjects.Product.Rum;
using UnifiedLogin.SharedObjects.Product.VendorServices;
using ProductRole = UnifiedLogin.SharedObjects.Product.ProductRole;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first service for building clone-user product batch records.
/// <para>
/// Replaces <c>ManageCloneProductBatch</c> which accepted <c>DefaultUserClaim</c>
/// via its constructor and instantiated all product services inline with
/// <c>new ManageProductXxx(_userClaim)</c>.
/// </para>
/// <para>
/// Key improvements:
/// <list type="bullet">
///   <item>All product service calls are genuinely async.</item>
///   <item><c>IsProductEnabledForUsePrimaryProperty</c> replaced by a single parallel
///         pre-fetch (<see cref="PrefetchProductEnabledFlagsAsync"/>) for all unique
///         product IDs — eliminates N serial <c>new ProductInternalSettingRepository()</c>
///         instantiations.</item>
///   <item>20+ if/else product branches replaced by a <c>switch</c> expression dispatch
///         (<see cref="ResolveProductBatchAsync"/>).</item>
///   <item>Three duplicate flag helpers (<c>CheckForAllProperties</c>,
///         <c>CheckForAllRegions</c>, <c>CheckForIsAssignedNewPropertyFlag</c>)
///         collapsed into a single static <see cref="GetBoolFlag"/> helper.</item>
/// </list>
/// </para>
/// <para><b>DI registration:</b> <c>Scoped</c>.</para>
/// </summary>
public sealed class ManageCloneProductBatchAsync : IManageCloneProductBatchAsync
{
    #region Fields

    private readonly IManageProductOneSiteAsync             _manageOneSite;
    private readonly IManageProductOneSiteAccountingAsync   _manageOneSiteAccounting;
    private readonly IManageProductMarketingCenterAsync     _manageMarketingCenter;
    private readonly IManageProductOpsAsync                 _manageOps;
    private readonly IManageProductVendorServicesAsync      _manageVendorServices;
    private readonly IManageProductProspectContactAsync     _manageProspectContact;
    private readonly IManageProductLead2LeaseAsync          _manageLead2Lease;
    private readonly IManageProductResidentPortalAsync      _manageResidentPortal;
    private readonly IManageProductRentersInsuranceAsync    _manageRentersInsurance;
    private readonly IManageProductOnSiteAsync              _manageOnSite;
    private readonly IManageProductRumAsync                 _manageRum;
    private readonly IManageProductAdminSupportPortalAsync   _manageAdminSupportPortal;
    private readonly IManageBlueBookAsync                    _manageBlueBook;
    private readonly IProductInternalSettingRepositoryAsync  _productInternalSettingRepository;
    private readonly IIntegrationTypeFactoryAsync            _integrationTypeFactory;
    private readonly IDocManagementBatchServiceAsync         _docManagementBatchService;
    private readonly ISamlRepositoryAsync                    _samlRepository;
    private readonly IManageProductAssetOptimizationAsync    _manageAssetOptimization;
    private readonly IUserClaimsAccessor                     _userClaims;
    private readonly ILogger<ManageCloneProductBatchAsync>   _logger;

    #endregion

    #region Constructor

    public ManageCloneProductBatchAsync(
        IManageProductOneSiteAsync             manageOneSite,
        IManageProductOneSiteAccountingAsync   manageOneSiteAccounting,
        IManageProductMarketingCenterAsync     manageMarketingCenter,
        IManageProductOpsAsync                 manageOps,
        IManageProductVendorServicesAsync      manageVendorServices,
        IManageProductProspectContactAsync     manageProspectContact,
        IManageProductLead2LeaseAsync          manageLead2Lease,
        IManageProductResidentPortalAsync      manageResidentPortal,
        IManageProductRentersInsuranceAsync    manageRentersInsurance,
        IManageProductOnSiteAsync              manageOnSite,
        IManageProductRumAsync                 manageRum,
        IManageProductAdminSupportPortalAsync  manageAdminSupportPortal,
        IManageBlueBookAsync                   manageBlueBook,
        IProductInternalSettingRepositoryAsync productInternalSettingRepository,
        IIntegrationTypeFactoryAsync           integrationTypeFactory,
        IDocManagementBatchServiceAsync        docManagementBatchService,
        ISamlRepositoryAsync                   samlRepository,
        IManageProductAssetOptimizationAsync   manageAssetOptimization,
        IUserClaimsAccessor                    userClaims,
        ILogger<ManageCloneProductBatchAsync>  logger)
    {
        _manageOneSite                    = manageOneSite                    ?? throw new ArgumentNullException(nameof(manageOneSite));
        _manageOneSiteAccounting          = manageOneSiteAccounting          ?? throw new ArgumentNullException(nameof(manageOneSiteAccounting));
        _manageMarketingCenter            = manageMarketingCenter            ?? throw new ArgumentNullException(nameof(manageMarketingCenter));
        _manageOps                        = manageOps                        ?? throw new ArgumentNullException(nameof(manageOps));
        _manageVendorServices             = manageVendorServices             ?? throw new ArgumentNullException(nameof(manageVendorServices));
        _manageProspectContact            = manageProspectContact            ?? throw new ArgumentNullException(nameof(manageProspectContact));
        _manageLead2Lease                 = manageLead2Lease                 ?? throw new ArgumentNullException(nameof(manageLead2Lease));
        _manageResidentPortal             = manageResidentPortal             ?? throw new ArgumentNullException(nameof(manageResidentPortal));
        _manageRentersInsurance           = manageRentersInsurance           ?? throw new ArgumentNullException(nameof(manageRentersInsurance));
        _manageOnSite                     = manageOnSite                     ?? throw new ArgumentNullException(nameof(manageOnSite));
        _manageRum                        = manageRum                        ?? throw new ArgumentNullException(nameof(manageRum));
        _manageAdminSupportPortal         = manageAdminSupportPortal         ?? throw new ArgumentNullException(nameof(manageAdminSupportPortal));
        _manageBlueBook                   = manageBlueBook                   ?? throw new ArgumentNullException(nameof(manageBlueBook));
        _productInternalSettingRepository = productInternalSettingRepository ?? throw new ArgumentNullException(nameof(productInternalSettingRepository));
        _integrationTypeFactory           = integrationTypeFactory           ?? throw new ArgumentNullException(nameof(integrationTypeFactory));
        _docManagementBatchService        = docManagementBatchService        ?? throw new ArgumentNullException(nameof(docManagementBatchService));
        _samlRepository                   = samlRepository                   ?? throw new ArgumentNullException(nameof(samlRepository));
        _manageAssetOptimization          = manageAssetOptimization          ?? throw new ArgumentNullException(nameof(manageAssetOptimization));
        _userClaims                       = userClaims                       ?? throw new ArgumentNullException(nameof(userClaims));
        _logger                           = logger                           ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Public Methods

    /// <inheritdoc/>
    public async Task<IList<ProductBatch>> GetUserProductBatchDataAsync(
        long personaId,
        List<PersonaProductUserDetails> userProducts,
        long baseOrgAdminPersonaId,
        UPFMProperty upfmProperty,
        List<ProductSettingList> productSettingList,
        bool externalUser = false,
        CancellationToken cancellationToken = default)
    {
        var productListToCreate = new List<ProductBatch>();

        // Pre-fetch all UsePrimaryProperties enabled flags in parallel before the loop,
        // replacing N serial new ProductInternalSettingRepository() instantiations.
        var enabledFlags = await PrefetchProductEnabledFlagsAsync(userProducts, cancellationToken)
            .ConfigureAwait(false);

        foreach (var product in userProducts)
        {
            // AO products are handled as a group after this loop
            if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)product.ProductId))
                continue;

            try
            {
                bool productEnabledForPrimaryProperty = enabledFlags.GetValueOrDefault(product.ProductId);

                var productSetting = productSettingList.FirstOrDefault(item =>
                    item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase) &&
                    item.ProductId == product.ProductId);

                bool personaProductUsePrimaryProperty = productSetting?.Value.Trim() == "1";
                bool usePrimaryProperties = productEnabledForPrimaryProperty && personaProductUsePrimaryProperty;
                bool translateProperties  = productEnabledForPrimaryProperty && personaProductUsePrimaryProperty
                                            && upfmProperty.id is not null;

                var batch = await ResolveProductBatchAsync(
                    product, personaId, baseOrgAdminPersonaId,
                    upfmProperty, usePrimaryProperties, translateProperties,
                    cancellationToken).ConfigureAwait(false);

                if (batch is not null)
                    productListToCreate.Add(batch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "GetUserProductBatchDataAsync: exception during clone for product {ProductName} ({ProductId})",
                    product?.ProductName, product?.ProductId);
            }
        }

        // ── AO products: handled as a group ──────────────────────────────────
        if (userProducts.Any(p => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)p.ProductId)))
        {
            bool aoEnabled = enabledFlags.GetValueOrDefault((int)ProductEnum.AssetOptimizer);
            var aoBatches = await CreateAoCloneBatchRecordsAsync(
                baseOrgAdminPersonaId, personaId,
                productSettingList, aoEnabled, externalUser,
                cancellationToken).ConfigureAwait(false);

            foreach (var aoBatch in aoBatches)
            {
                if (userProducts.Any(x => x.ProductId == aoBatch.ProductId))
                    productListToCreate.Add(aoBatch);
            }
        }

        return productListToCreate;
    }

    #endregion

    #region Private — Product dispatch

    private Task<ProductBatch?> ResolveProductBatchAsync(
        PersonaProductUserDetails product,
        long personaId,
        long baseOrgAdminPersonaId,
        UPFMProperty upfmProperty,
        bool usePrimaryProperties,
        bool translateProperties,
        CancellationToken ct)
    {
        var productEnum = (ProductEnum)product.ProductId;

        return productEnum switch
        {
            ProductEnum.OneSite
                => HandleOneSiteAsync(personaId, baseOrgAdminPersonaId, upfmProperty, product.ProductId, usePrimaryProperties, translateProperties, ct),
            ProductEnum.FinancialSuite
                => HandleFinancialSuiteAsync(personaId, baseOrgAdminPersonaId, upfmProperty, product.ProductId, usePrimaryProperties, translateProperties, ct),
            ProductEnum.MarketingCenter
                => HandleMarketingCenterAsync(personaId, baseOrgAdminPersonaId, upfmProperty, product.ProductId, usePrimaryProperties, translateProperties, ct),
            ProductEnum.OpsBuyer
                => HandleOpsBuyerAsync(personaId, baseOrgAdminPersonaId, upfmProperty, product.ProductId, usePrimaryProperties, translateProperties, ct),
            ProductEnum.VendorServices
                => HandleVendorServicesAsync(personaId, baseOrgAdminPersonaId, upfmProperty, product.ProductId, usePrimaryProperties, translateProperties, ct),
            ProductEnum.AdminSupportPortal
                => HandleAdminSupportPortalAsync(personaId, baseOrgAdminPersonaId, upfmProperty, product.ProductId, usePrimaryProperties, translateProperties, ct),
            ProductEnum.ProspectContactCenter
                => HandleProspectContactAsync(personaId, baseOrgAdminPersonaId, upfmProperty, product.ProductId, usePrimaryProperties, translateProperties, ct),
            ProductEnum.Lead2Lease
                => HandleLead2LeaseAsync(personaId, baseOrgAdminPersonaId, upfmProperty, product.ProductId, usePrimaryProperties, translateProperties, ct),
            ProductEnum.ResidentPortal
                => HandleResidentPortalAsync(personaId, baseOrgAdminPersonaId, upfmProperty, product.ProductId, usePrimaryProperties, translateProperties, ct),
            ProductEnum.Insurance
                => HandleRentersInsuranceAsync(personaId, baseOrgAdminPersonaId, upfmProperty, product.ProductId, usePrimaryProperties, translateProperties, ct),
            ProductEnum.OnSite
                => HandleOnSiteAsync(personaId, baseOrgAdminPersonaId, upfmProperty, product.ProductId, usePrimaryProperties, translateProperties, ct),
            ProductEnum.UtilityManagement
                => HandleRumAsync(personaId, baseOrgAdminPersonaId, upfmProperty, usePrimaryProperties, translateProperties, ct),
            ProductEnum.SelfProvisioningPortal
                => Task.FromResult<ProductBatch?>(BuildSelfProvisioningPortalBatch(product.ProductId)),
            ProductEnum.LeadManagement
                => HandleLeadManagementAsync(personaId, baseOrgAdminPersonaId, upfmProperty, product.ProductId, usePrimaryProperties, translateProperties, ct),
            ProductEnum.LeadAnalytics
                => HandleLeadAnalyticsAsync(personaId, baseOrgAdminPersonaId, upfmProperty, product.ProductId, usePrimaryProperties, translateProperties, ct),
            ProductEnum.RPDocumentManagement
                => HandleDocManagementAsync(personaId, baseOrgAdminPersonaId, usePrimaryProperties, ct),
            ProductEnum.PortfolioManagement
                => HandlePortfolioManagementAsync(personaId, baseOrgAdminPersonaId, product.ProductId, usePrimaryProperties, ct),
            ProductEnum.DepositAlternative
                => HandleDepositAlternativeAsync(personaId, baseOrgAdminPersonaId, product.ProductId, usePrimaryProperties, ct),
            ProductEnum.ClickPay
                => HandleClickPayAsync(personaId, baseOrgAdminPersonaId, product.ProductId, usePrimaryProperties, ct),
            ProductEnum.KnockCRM
                => HandleKnockCRMAsync(personaId, baseOrgAdminPersonaId, product.ProductId, usePrimaryProperties, ct),
            _ when !ProductEnumHelper.GetAoProductList().Contains(productEnum)
                => HandleIntegrationTypeAsync(personaId, baseOrgAdminPersonaId, product.ProductId, usePrimaryProperties, ct),
            _ => Task.FromResult<ProductBatch?>(null)   // AO products are handled separately
        };
    }

    #endregion

    #region Private — Product handlers (async)

    private async Task<ProductBatch?> HandleOneSiteAsync(
        long personaId, long baseOrgAdminPersonaId, UPFMProperty upfmProperty,
        int productId, bool usePrimaryProperties, bool translateProperties, CancellationToken ct)
    {
        var props = await _manageOneSite.GetPropertiesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        if (translateProperties)
            props = await _manageBlueBook.TranslateProductPrimaryPropertiesDataAsync(upfmProperty, productId, props, ct).ConfigureAwait(false);

        var roles = await _manageOneSite.GetRolesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        return BuildProductBatch(props, roles, productId, usePrimaryProperties);
    }

    private async Task<ProductBatch?> HandleFinancialSuiteAsync(
        long personaId, long baseOrgAdminPersonaId, UPFMProperty upfmProperty,
        int productId, bool usePrimaryProperties, bool translateProperties, CancellationToken ct)
    {
        var props = await _manageOneSiteAccounting.GetUserPropertiesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        if (translateProperties)
            props = await _manageBlueBook.TranslateProductPrimaryPropertiesDataAsync(upfmProperty, productId, props, ct).ConfigureAwait(false);

        var propGroups = await _manageOneSiteAccounting.GetUserPropertyGroupsAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        var roles      = await _manageOneSiteAccounting.GetUserRolesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        var companies  = await _manageOneSiteAccounting.GetUserCompaniesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        return BuildFinancialSuiteBatch(props, roles, productId, companies, propGroups, usePrimaryProperties);
    }

    private async Task<ProductBatch?> HandleMarketingCenterAsync(
        long personaId, long baseOrgAdminPersonaId, UPFMProperty upfmProperty,
        int productId, bool usePrimaryProperties, bool translateProperties, CancellationToken ct)
    {
        var props = await _manageMarketingCenter.GetPropertiesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        if (translateProperties)
            props = await _manageBlueBook.TranslateProductPrimaryPropertiesDataAsync(upfmProperty, productId, props, ct).ConfigureAwait(false);

        var roles = await _manageMarketingCenter.GetRolesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        return BuildMarketingCenterBatch(props, roles, productId, usePrimaryProperties);
    }

    private async Task<ProductBatch?> HandleOpsBuyerAsync(
        long personaId, long baseOrgAdminPersonaId, UPFMProperty upfmProperty,
        int productId, bool usePrimaryProperties, bool translateProperties, CancellationToken ct)
    {
        var props = await _manageOps.GetCompanyAssetsAsync(baseOrgAdminPersonaId, personaId, false, null!, ct).ConfigureAwait(false);
        if (translateProperties)
            props = await _manageBlueBook.TranslateProductPrimaryPropertiesDataAsync(upfmProperty, productId, props, ct).ConfigureAwait(false);

        var roles = await _manageOps.GetRolesAsync(baseOrgAdminPersonaId, personaId, string.Empty, null!, ct).ConfigureAwait(false);
        return BuildProductBatch(props, roles, productId, usePrimaryProperties);
    }

    private async Task<ProductBatch?> HandleVendorServicesAsync(
        long personaId, long baseOrgAdminPersonaId, UPFMProperty upfmProperty,
        int productId, bool usePrimaryProperties, bool translateProperties, CancellationToken ct)
    {
        var props = await _manageVendorServices.GetPropertiesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        if (translateProperties)
            props = await _manageBlueBook.TranslateProductPrimaryPropertiesDataAsync(upfmProperty, productId, props, ct).ConfigureAwait(false);

        var roles         = await _manageVendorServices.GetRolesAsync(baseOrgAdminPersonaId, personaId, AccessType.Property, null!, ct).ConfigureAwait(false);
        var notification  = await _manageVendorServices.GetNotificationSettingsAsync(baseOrgAdminPersonaId, personaId, ct).ConfigureAwait(false);
        var propGroups    = await _manageVendorServices.GetPropertyGroupsAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        return BuildVendorServicesBatch(props, roles, propGroups, notification, productId, usePrimaryProperties);
    }

    private async Task<ProductBatch?> HandleAdminSupportPortalAsync(
        long personaId, long baseOrgAdminPersonaId, UPFMProperty upfmProperty,
        int productId, bool usePrimaryProperties, bool translateProperties, CancellationToken ct)
    {
        var props = await _manageAdminSupportPortal.GetPropertiesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        if (translateProperties)
            props = await _manageBlueBook.TranslateProductPrimaryPropertiesDataAsync(upfmProperty, productId, props, ct).ConfigureAwait(false);

        var roles = await _manageAdminSupportPortal.GetRolesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        return BuildProductBatch(props, roles, productId, usePrimaryProperties);
    }

    private async Task<ProductBatch?> HandleProspectContactAsync(
        long personaId, long baseOrgAdminPersonaId, UPFMProperty upfmProperty,
        int productId, bool usePrimaryProperties, bool translateProperties, CancellationToken ct)
    {
        var props = await _manageProspectContact.GetPropertiesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        if (translateProperties)
            props = await _manageBlueBook.TranslateProductPrimaryPropertiesDataAsync(upfmProperty, productId, props, ct).ConfigureAwait(false);

        // ProspectContactCenter has no roles — pass empty ListResponse
        return BuildProductBatch(props, new ListResponse(), productId, usePrimaryProperties);
    }

    private async Task<ProductBatch?> HandleLead2LeaseAsync(
        long personaId, long baseOrgAdminPersonaId, UPFMProperty upfmProperty,
        int productId, bool usePrimaryProperties, bool translateProperties, CancellationToken ct)
    {
        var props = await _manageLead2Lease.GetPropertiesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        if (translateProperties)
            props = await _manageBlueBook.TranslateProductPrimaryPropertiesDataAsync(upfmProperty, productId, props, ct).ConfigureAwait(false);

        var roles = await _manageLead2Lease.GetRolesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        return BuildProductBatch(props, roles, productId, usePrimaryProperties);
    }

    private async Task<ProductBatch?> HandleResidentPortalAsync(
        long personaId, long baseOrgAdminPersonaId, UPFMProperty upfmProperty,
        int productId, bool usePrimaryProperties, bool translateProperties, CancellationToken ct)
    {
        var props = await _manageResidentPortal.ListPropertiesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        if (translateProperties)
            props = await _manageBlueBook.TranslateProductPrimaryPropertiesDataAsync(upfmProperty, productId, props, ct).ConfigureAwait(false);

        var levels         = await _manageResidentPortal.ListLevelsAsync(baseOrgAdminPersonaId, personaId, ct).ConfigureAwait(false);
        var notifications  = await _manageResidentPortal.GetNotificationSettingsAsync(baseOrgAdminPersonaId, personaId, ct).ConfigureAwait(false);
        var msgGroups      = await _manageResidentPortal.ListMessageGroupsAsync(baseOrgAdminPersonaId, personaId, ct).ConfigureAwait(false);
        return BuildResidentPortalBatch(props, levels, notifications, msgGroups, productId, usePrimaryProperties);
    }

    private async Task<ProductBatch?> HandleRentersInsuranceAsync(
        long personaId, long baseOrgAdminPersonaId, UPFMProperty upfmProperty,
        int productId, bool usePrimaryProperties, bool translateProperties, CancellationToken ct)
    {
        var props = await _manageRentersInsurance.ListPropertiesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        if (translateProperties)
            props = await _manageBlueBook.TranslateProductPrimaryPropertiesDataAsync(upfmProperty, productId, props, ct).ConfigureAwait(false);

        var roles = await _manageRentersInsurance.ListRolesAsync(baseOrgAdminPersonaId, personaId, ct).ConfigureAwait(false);
        return BuildRentersInsuranceBatch(props, roles, productId, usePrimaryProperties);
    }

    private async Task<ProductBatch?> HandleOnSiteAsync(
        long personaId, long baseOrgAdminPersonaId, UPFMProperty upfmProperty,
        int productId, bool usePrimaryProperties, bool translateProperties, CancellationToken ct)
    {
        var props = await _manageOnSite.GetPropertiesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        if (translateProperties)
            props = await _manageBlueBook.TranslateProductPrimaryPropertiesDataAsync(upfmProperty, productId, props, ct).ConfigureAwait(false);

        var roles   = await _manageOnSite.GetRolesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        var regions = await _manageOnSite.GetRegionsAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        return BuildOnSiteBatch(props, roles, regions, productId, usePrimaryProperties);
    }

    private async Task<ProductBatch?> HandleRumAsync(
        long personaId, long baseOrgAdminPersonaId, UPFMProperty upfmProperty,
        bool usePrimaryProperties, bool translateProperties, CancellationToken ct)
    {
        var props = await _manageRum.GetPropertiesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        if (translateProperties)
            props = await _manageBlueBook.TranslateProductPrimaryPropertiesDataAsync(upfmProperty, (int)ProductEnum.UtilityManagement, props, ct).ConfigureAwait(false);

        var propGroups  = await _manageRum.GetPropertyGroupsAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        var regions     = await _manageRum.GetRegionsAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        var accessTypes = await _manageRum.GetUMGlobalRolesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        var roles       = await _manageRum.GetRolesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        return BuildRumBatch(props, propGroups, regions, roles, accessTypes, usePrimaryProperties);
    }

    private async Task<ProductBatch?> HandleLeadManagementAsync(
        long personaId, long baseOrgAdminPersonaId, UPFMProperty upfmProperty,
        int productId, bool usePrimaryProperties, bool translateProperties, CancellationToken ct)
    {
        var intType   = _integrationTypeFactory.GetIntegration(productId);
        var props     = await intType.GetPropertiesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        if (translateProperties)
            props = await _manageBlueBook.TranslateProductPrimaryPropertiesDataAsync(upfmProperty, productId, props, ct).ConfigureAwait(false);

        var rolesResp = await intType.GetRolesAsync(baseOrgAdminPersonaId, personaId, 0, null, null!, ct).ConfigureAwait(false);
        return BuildILMBatch(ProductEnum.LeadManagement, props,
            ExtractStandardV1Roles(rolesResp), propertyGroups: null, usePrimaryProperties);
    }

    private async Task<ProductBatch?> HandleLeadAnalyticsAsync(
        long personaId, long baseOrgAdminPersonaId, UPFMProperty upfmProperty,
        int productId, bool usePrimaryProperties, bool translateProperties, CancellationToken ct)
    {
        var intType    = _integrationTypeFactory.GetIntegration(productId);
        var props      = await intType.GetPropertiesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        if (translateProperties)
            props = await _manageBlueBook.TranslateProductPrimaryPropertiesDataAsync(upfmProperty, productId, props, ct).ConfigureAwait(false);

        var rolesResp  = await intType.GetRolesAsync(baseOrgAdminPersonaId, personaId, 0, null, null!, ct).ConfigureAwait(false);
        var groupsResp = await intType.GetPropertyGroupsAsync(baseOrgAdminPersonaId, personaId, null!, ct: ct).ConfigureAwait(false);
        return BuildILMBatch(ProductEnum.LeadAnalytics, props,
            ExtractStandardV1Roles(rolesResp), ExtractStandardV1Groups(groupsResp), usePrimaryProperties);
    }

    private async Task<ProductBatch?> HandleDocManagementAsync(
        long personaId, long baseOrgAdminPersonaId,
        bool usePrimaryProperties, CancellationToken ct) =>
        await _docManagementBatchService
            .CreateDocManagementBatchRecordAsync(baseOrgAdminPersonaId, personaId, usePrimaryProperties, ct)
            .ConfigureAwait(false);

    private async Task<ProductBatch?> HandleKnockCRMAsync(
        long personaId, long baseOrgAdminPersonaId,
        int productId, bool usePrimaryProperties, CancellationToken ct)
    {
        var intType     = _integrationTypeFactory.GetIntegration(productId);
        var propGroups  = await intType.GetPropertyGroupsAsync(baseOrgAdminPersonaId, personaId, null!, ct: ct).ConfigureAwait(false);
        var roles       = await intType.GetRolesAsync(baseOrgAdminPersonaId, personaId, _userClaims.OrganizationPartyId, null, null!, ct).ConfigureAwait(false);
        return BuildKnockCRMBatch(propGroups, roles, productId, usePrimaryProperties);
    }

    private async Task<ProductBatch?> HandleIntegrationTypeAsync(
        long personaId, long baseOrgAdminPersonaId,
        int productId, bool usePrimaryProperties, CancellationToken ct)
    {
        var intType = _integrationTypeFactory.GetIntegration(productId);
        var props   = await intType.GetPropertiesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        var roles   = await intType.GetRolesAsync(baseOrgAdminPersonaId, personaId, _userClaims.OrganizationPartyId, null, null!, ct).ConfigureAwait(false);
        return BuildProductBatch(props, roles, productId, usePrimaryProperties);
    }

    private async Task<ProductBatch?> HandlePortfolioManagementAsync(
        long personaId, long baseOrgAdminPersonaId,
        int productId, bool usePrimaryProperties, CancellationToken ct)
    {
        var intType   = _integrationTypeFactory.GetIntegration(productId);
        var rolesResp = await intType.GetRolesAsync(baseOrgAdminPersonaId, personaId, 0, null, null!, ct).ConfigureAwait(false);
        var propsResp = await intType.GetPropertiesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);

        var roleList         = ExtractStandardV1Roles(rolesResp);
        var rolePropertyList = new List<PAMRolePropertyList>();
        if (propsResp.Records is not null)
        {
            foreach (var item in propsResp.Records.Cast<PortfolioRoleProperty>())
            {
                if (item.IsAssigned)
                {
                    rolePropertyList.Add(new PAMRolePropertyList
                    {
                        RoleId            = item.GetRoleId,
                        PropertyIds       = item.PropertiesList.Where(p => p.IsAssigned).Select(p => p.GetPropertyId).ToList(),
                        PropertyGroupList = item.GroupList.Where(g => g.IsAssigned).Select(g => g.GetGroupId).ToList()
                    });
                }
            }
        }

        return BuildPortfolioManagementBatch(rolePropertyList, roleList, usePrimaryProperties);
    }

    private async Task<ProductBatch?> HandleDepositAlternativeAsync(
        long personaId, long baseOrgAdminPersonaId,
        int productId, bool usePrimaryProperties, CancellationToken ct)
    {
        var intType    = _integrationTypeFactory.GetIntegration(productId);
        var rolesResp  = await intType.GetRolesAsync(baseOrgAdminPersonaId, personaId, 0, null, null!, ct).ConfigureAwait(false);
        var propsResp  = await intType.GetPropertiesAsync(baseOrgAdminPersonaId, personaId, null!, ct).ConfigureAwait(false);
        var groupsResp = await intType.GetPropertyGroupsAsync(baseOrgAdminPersonaId, personaId, null!, ct: ct).ConfigureAwait(false);

        var roles = ExtractStandardV1Roles(rolesResp);
        bool canReceive = GetBoolFlag(rolesResp.Additional, "CanReceiveMonthlyReport");

        var properties = new List<string>();
        if (propsResp.Records is not null)
        {
            foreach (var item in propsResp.Records.Cast<Logic.ProductIntegration.Model.ProductProperties>())
            {
                if (item.IsAssigned)
                    properties.Add(item.GetPropertyId);
            }
        }

        return BuildDepositAlternativeBatch(
            roles, canReceive, properties, ExtractStandardV1Groups(groupsResp), usePrimaryProperties);
    }

    private async Task<ProductBatch?> HandleClickPayAsync(
        long personaId, long baseOrgAdminPersonaId,
        int productId, bool usePrimaryProperties, CancellationToken ct)
    {
        var intType   = _integrationTypeFactory.GetIntegration(productId);
        var rolesResp = await intType.GetRolesAsync(baseOrgAdminPersonaId, personaId, 0, null, null!, ct).ConfigureAwait(false);

        var orgRoles = new List<OrganizationRole>();
        if (rolesResp.Records is not null)
        {
            foreach (var item in rolesResp.Records.Cast<ClickPayRole>())
            {
                if (item.IsAssigned && item.SelectedItems is not null)
                {
                    foreach (var selected in item.SelectedItems.Where(s => s.Value))
                    {
                        orgRoles.Add(new OrganizationRole
                        {
                            OrganizationId = selected.Id,
                            RoleId         = item.Id
                        });
                    }
                }
            }
        }

        return BuildClickPayBatch(orgRoles, usePrimaryProperties);
    }

    #endregion

    #region Private — AO batch builder

    private async Task<IList<ProductBatch>> CreateAoCloneBatchRecordsAsync(
        long editorPersonaId,
        long newUserPersonaId,
        List<ProductSettingList> productSettingList,
        bool aoEnabled,
        bool externalUser,
        CancellationToken ct)
    {
        var productBatchList = new List<ProductBatch>();

        IList<AoUserCompanyPropertyRoleDetail> aoBiDetails = [];
        if (externalUser)
        {
            var productAttributes = await _samlRepository
                .GetProductSamlDetailsAsync(newUserPersonaId, (int)ProductEnum.AoBusinessIntelligence, ct)
                .ConfigureAwait(false);

            string aoBiUserName = productAttributes
                .FirstOrDefault(a => a.Name.Equals("ProductUserName", StringComparison.OrdinalIgnoreCase))
                ?.Value ?? string.Empty;

            if (!string.IsNullOrEmpty(aoBiUserName))
                aoBiDetails = await _manageAssetOptimization
                    .CopyRegularUserAsync(editorPersonaId, newUserPersonaId, aoBiUserName, ct)
                    .ConfigureAwait(false);
        }

        var aoDetails = await _manageAssetOptimization
            .CopyRegularUserAsync(editorPersonaId, newUserPersonaId, cancellationToken: ct)
            .ConfigureAwait(false);

        foreach (var biDetail in aoBiDetails)
            aoDetails.Add(biDetail);

        foreach (var detail in aoDetails)
        {
            detail.SelectedPortfolioValues ??= [];
            detail.PropertyGroups          ??= [];

            var productSetting = productSettingList.FirstOrDefault(item =>
                item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase) &&
                item.ProductId == (int)ProductEnumHelper.GetAoProductEnum(detail.ProductName));

            bool usePrimaryProperties = aoEnabled && productSetting?.Value.Trim() == "1";

            productBatchList.Add(new ProductBatch
            {
                ProductId    = (int)ProductEnumHelper.GetAoProductEnum(detail.ProductName),
                StatusTypeId = 5,
                RetryCount   = 0,
                InputJson    = new RolePropertyList
                {
                    PropertyList         = detail.SelectedPortfolioValues.Select(i => i.ToString()).ToList(),
                    RoleList             = [.. detail.SelectedRoleValues],
                    CompanyId            = detail.CompanyId,
                    PropertyGroupList    = detail.PropertyGroups.Select(i => i.ToString()).ToList(),
                    UsePrimaryProperties = usePrimaryProperties
                }
            });
        }

        return productBatchList;
    }

    #endregion

    #region Private — Parallel pre-fetch

    /// <summary>
    /// Fetches <c>UsePrimaryProperties</c> enabled flags for all unique product IDs in a
    /// single parallel fan-out, replacing N serial <c>new ProductInternalSettingRepository()</c>
    /// instantiations. <c>AssetOptimizer</c> is appended when any AO sub-product is present.
    /// </summary>
    private async Task<Dictionary<int, bool>> PrefetchProductEnabledFlagsAsync(
        IReadOnlyCollection<PersonaProductUserDetails> userProducts,
        CancellationToken ct)
    {
        var uniqueIds = userProducts.Select(p => p.ProductId).Distinct().ToList();

        if (userProducts.Any(p => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)p.ProductId))
            && !uniqueIds.Contains((int)ProductEnum.AssetOptimizer))
        {
            uniqueIds.Add((int)ProductEnum.AssetOptimizer);
        }

        var results = await Task.WhenAll(uniqueIds.Select(async id =>
        {
            var settings = await _productInternalSettingRepository
                .GetProductInternalSettingsAsync(id, ct).ConfigureAwait(false);
            var setting = settings?.FirstOrDefault(s =>
                s.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase));
            return (id, enabled: setting?.Value.Trim() == "1");
        })).ConfigureAwait(false);

        return results.ToDictionary(x => x.id, x => x.enabled);
    }

    #endregion

    #region Private — Batch builders

    private ProductBatch BuildProductBatch(
        ListResponse propsResponse, ListResponse rolesResponse,
        int productId, bool usePrimaryProperties)
    {
        var propertyList = new List<string>();
        var roleList     = new List<string>();
        bool allProps    = GetBoolFlag(propsResponse.Additional, "allProperties");

        var intType = _integrationTypeFactory.GetIntegrationTypeForProductId(productId);

        if (intType == ProductIntegrationTypeEnum.StandardV1)
        {
            if (rolesResponse.Records is not null)
            {
                foreach (var item in rolesResponse.Records.Cast<Logic.ProductIntegration.Model.ProductRole>())
                {
                    if (item.IsAssigned)
                        roleList.Add(item.GetRoleId);
                }
            }
        }
        else if (productId != (int)ProductEnum.ProspectContactCenter)
        {
            if (rolesResponse.Records is not null)
            {
                foreach (var item in rolesResponse.Records.Cast<ProductRole>())
                {
                    if (item.IsAssigned)
                        roleList.Add(item.ID);
                }
            }
        }

        if (allProps)
        {
            if (intType == ProductIntegrationTypeEnum.UPFM)
                propertyList.Add("-1");
            else if (productId == (int)ProductEnum.OneSite ||
                     productId == (int)ProductEnum.FinancialSuite ||
                     productId == (int)ProductEnum.ProspectContactCenter ||
                     productId == (int)ProductEnum.MarketingCenter ||
                     intType == ProductIntegrationTypeEnum.StandardV1)
                propertyList.Add("ALL");
        }
        else if (intType == ProductIntegrationTypeEnum.StandardV1)
        {
            if (propsResponse.Records is not null)
            {
                foreach (var item in propsResponse.Records.Cast<Logic.ProductIntegration.Model.ProductProperties>())
                {
                    if (item.IsAssigned)
                        propertyList.Add(item.GetPropertyId);
                }
            }
        }
        else
        {
            if (propsResponse.Records is not null)
            {
                foreach (var item in propsResponse.Records)
                {
                    if (productId == (int)ProductEnum.OpsBuyer)
                    {
                        var assetGroup = (SharedObjects.Product.Ops.AssetGroup)item;
                        if (assetGroup.IsAssigned)
                            propertyList.Add(assetGroup.ID);
                    }
                    else
                    {
                        var prop = (ProductProperty)item;
                        if (prop.IsAssigned.Value)
                            propertyList.Add(intType == ProductIntegrationTypeEnum.UPFM ? prop.Alias : prop.ID);
                    }
                }
            }
        }

        return new ProductBatch
        {
            ProductId    = productId,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList         = propertyList,
                RoleList             = roleList,
                UsePrimaryProperties = usePrimaryProperties
            }
        };
    }

    private static ProductBatch BuildFinancialSuiteBatch(
        ListResponse propsResponse, ListResponse rolesResponse, int productId,
        ListResponse companiesResponse, ListResponse propGroupResponse, bool usePrimaryProperties)
    {
        var propertyList  = new List<string>();
        var roleList      = new List<string>();
        var companiesList = new List<string>();
        bool hasAccessToSiteSpendManagementOnly    = false;
        bool isAccountingAdmin                     = false;
        bool hasAccessToAllCurrentFutureProperties = false;

        if (companiesResponse.Additional is AccountingUser accountingUser)
        {
            hasAccessToSiteSpendManagementOnly    = accountingUser.HasAccessToSiteSpendManagementOnly;
            isAccountingAdmin                     = accountingUser.IsAccountingAdmin;
            hasAccessToAllCurrentFutureProperties = accountingUser.HasAccessToAllCurrentFutureProperties;
        }

        if (companiesResponse.Records is not null)
        {
            foreach (var item in companiesResponse.Records.Cast<ACCompany>())
            {
                if (!string.IsNullOrEmpty(item.Id))
                    companiesList.Add(item.Id);
            }
        }

        if (rolesResponse.Records is not null)
        {
            foreach (var item in rolesResponse.Records.Cast<ProductRole>())
            {
                if (item.IsAssigned)
                    roleList.Add(item.ID);
            }
        }

        if (propsResponse.Records is not null)
        {
            foreach (var item in propsResponse.Records.Cast<ProductProperty>())
            {
                if (item.IsAssigned.Value)
                    propertyList.Add(item.ID);
            }
        }

        // Legacy behaviour: property groups are also flattened into PropertyList
        if (propGroupResponse.Records is not null)
        {
            foreach (var item in propGroupResponse.Records.Cast<ProductPropertyGroup>())
            {
                if (item.IsAssigned.Value)
                    propertyList.Add(item.ID);
            }
        }

        return new ProductBatch
        {
            ProductId    = productId,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList                          = propertyList,
                RoleList                              = roleList,
                HasAccessToSiteSpendManagementOnly    = hasAccessToSiteSpendManagementOnly,
                IsAccountingAdmin                     = isAccountingAdmin,
                HasAccessToAllCurrentFutureProperties = hasAccessToAllCurrentFutureProperties,
                CompaniesList                         = companiesList,
                UsePrimaryProperties                  = usePrimaryProperties
            }
        };
    }

    private static ProductBatch BuildMarketingCenterBatch(
        ListResponse propsResponse, ListResponse rolesResponse,
        int productId, bool usePrimaryProperties)
    {
        var propertyList = new List<string>();
        var roleList     = new List<string>();
        bool isAssignNewPropertyByDefault = GetBoolFlag(propsResponse.Additional, "IsAssignedNewPropertyByDefault");

        if (productId != (int)ProductEnum.ProspectContactCenter && rolesResponse.Records is not null)
        {
            foreach (var item in rolesResponse.Records.Cast<ProductRole>())
            {
                if (item.IsAssigned)
                    roleList.Add(item.ID);
            }
        }

        if (propsResponse.Records is not null)
        {
            foreach (var item in propsResponse.Records.Cast<ProductProperty>())
            {
                if (item.IsAssigned.Value)
                    propertyList.Add(item.ID);
            }
        }

        return new ProductBatch
        {
            ProductId    = productId,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList                   = propertyList,
                RoleList                       = roleList,
                IsAssignedNewPropertyByDefault = isAssignNewPropertyByDefault,
                UsePrimaryProperties           = usePrimaryProperties
            }
        };
    }

    private static ProductBatch BuildVendorServicesBatch(
        ListResponse propsResponse, ListResponse rolesResponse,
        ListResponse propGroupResponse, Notification? notification,
        int productId, bool usePrimaryProperties)
    {
        var propertyList      = new List<string>();
        var roleList          = new List<string>();
        var propertyGroupList = new List<SharedObjects.Product.VendorServices.PropertyGroup>();
        bool allProps         = GetBoolFlag(propsResponse.Additional, "allProperties");

        // When source user has all properties unassigned, treat as "all" (clone-from-all-properties behaviour)
        if (!allProps && propsResponse.Records is not null)
        {
            var records = propsResponse.Records.Cast<ProductProperty>().ToList();
            if (records.Count > 0 && records.All(p => p.IsAssigned == false))
                allProps = true;
        }

        if (rolesResponse.Records is not null)
        {
            foreach (var item in rolesResponse.Records.Cast<ProductRole>())
            {
                if (item.IsAssigned)
                    roleList.Add(item.ID);
            }
        }

        if (propGroupResponse.TotalRows > 0 && propGroupResponse.Records is not null)
        {
            foreach (var item in propGroupResponse.Records.Cast<VendorServicesPropertyGroup>())
            {
                if (item.IsAssigned)
                {
                    propertyGroupList.Add(new SharedObjects.Product.VendorServices.PropertyGroup
                    {
                        Id         = item.PropertyGroupId,
                        IsAssigned = true,
                        Type       = (SharedObjects.Product.VendorServices.AccessTypeEnum)Enum.Parse(
                                         typeof(SharedObjects.Product.VendorServices.AccessTypeEnum),
                                         item.AccessLevel)
                    });
                }
            }
        }

        if (allProps)
        {
            propertyList.Add("-1");
        }
        else if (propsResponse.Records is not null)
        {
            foreach (var item in propsResponse.Records.Cast<ProductProperty>())
            {
                if (item.IsAssigned.Value)
                    propertyList.Add(item.ID);
            }
        }

        return new ProductBatch
        {
            ProductId    = productId,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList                   = propertyList,
                RoleList                       = roleList,
                PropertyGroup                  = propertyGroupList.Count > 0 ? propertyGroupList : null,
                IsInsuranceExpired             = notification?.IsInsuranceExpired ?? false,
                IsVendorRecommendationChanges  = notification?.IsVendorRecommendationChanges ?? false,
                IsVendorNotLinkedToAnyProperty = notification?.IsVendorNotLinkedToAnyProperty ?? false,
                UsePrimaryProperties           = usePrimaryProperties
            }
        };
    }

    private ProductBatch BuildOnSiteBatch(
        ListResponse propsResponse, ListResponse rolesResponse,
        ListResponse regionResponse, int productId, bool usePrimaryProperties)
    {
        var propertyList = new List<string>();
        var roleList     = new List<string>();
        var regionList   = new List<string>();
        bool allProps    = GetBoolFlag(propsResponse.Additional,  "allProperties");
        bool allRegions  = GetBoolFlag(regionResponse.Additional, "allProperties");

        if (allProps)
        {
            propertyList.Add("-1");
        }
        else if (propsResponse.Records is not null)
        {
            foreach (var item in propsResponse.Records)
            {
                var onSiteProp = (dynamic)item;
                if ((bool)onSiteProp.IsAssigned)
                    propertyList.Add(((int)onSiteProp.GetPropertyId).ToString());
            }
        }

        if (allRegions)
        {
            regionList.Add("-1");
        }
        else if (regionResponse.Records is not null)
        {
            foreach (var item in regionResponse.Records)
            {
                var onSiteRegion = (dynamic)item;
                if ((bool)onSiteRegion.IsAssigned)
                    regionList.Add(((int)onSiteRegion.GetRegionId).ToString());
            }
        }

        if (rolesResponse.Records is not null)
        {
            foreach (var item in rolesResponse.Records)
            {
                var onSiteRole = (dynamic)item;
                bool? isAssigned = onSiteRole.IsAssigned;
                if (isAssigned == true)
                    roleList.Add(((int)onSiteRole.Level).ToString());
            }
        }

        return new ProductBatch
        {
            ProductId    = productId,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList         = propertyList,
                RegionList           = regionList,
                RoleList             = roleList,
                UsePrimaryProperties = usePrimaryProperties
            }
        };
    }

    private static ProductBatch BuildResidentPortalBatch(
        ListResponse propsResponse, List<ILevel> levelList,
        Notifications? notifications, List<IMessagingGroups> messagingGroups,
        int productId, bool usePrimaryProperties)
    {
        var propertyList  = new List<string>();
        var roleList      = new List<string>();
        var messageGroups = new List<string>();
        bool allProps     = GetBoolFlag(propsResponse.Additional, "allProperties");

        if (allProps && productId == (int)ProductEnum.ResidentPortal)
        {
            propertyList.Add("ALL");
        }
        else if (propsResponse.Records is not null)
        {
            foreach (var item in propsResponse.Records.Cast<ProductProperty>())
            {
                if (item.IsAssigned.Value)
                    propertyList.Add(item.ID);
            }
        }

        var assignedLevel = levelList.Find(l => l.IsAssigned);
        if (assignedLevel is not null)
            roleList.Add(assignedLevel.Id.ToUpper());

        foreach (var group in messagingGroups.Cast<MessagingGroups>())
        {
            if (group.IsAssigned)
                messageGroups.Add(group.Id);
        }

        return new ProductBatch
        {
            ProductId    = productId,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList         = propertyList,
                RoleList             = roleList,
                Notifications        = notifications,
                MessageGroups        = messageGroups,
                UsePrimaryProperties = usePrimaryProperties
            }
        };
    }

    private static ProductBatch BuildRentersInsuranceBatch(
        ListResponse propsResponse, ListResponse rolesResponse,
        int productId, bool usePrimaryProperties)
    {
        var propertyList = new List<string>();
        var roleList     = new List<string>();
        bool allProps    = GetBoolFlag(propsResponse.Additional, "allProperties");

        if (allProps && productId == (int)ProductEnum.Insurance)
        {
            propertyList.Add("ALL");
        }
        else if (propsResponse.Records is not null)
        {
            foreach (var item in propsResponse.Records.Cast<ProductProperty>())
            {
                if (item.IsAssigned.Value)
                    propertyList.Add(item.ID);
            }
        }

        var assignedRole = rolesResponse.Records?.Cast<ProductRole>().ToList().Find(r => r.IsAssigned);
        if (assignedRole is not null)
            roleList.Add(assignedRole.ID);

        return new ProductBatch
        {
            ProductId    = productId,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList         = propertyList,
                RoleList             = roleList,
                UsePrimaryProperties = usePrimaryProperties
            }
        };
    }

    private static ProductBatch BuildRumBatch(
        ListResponse propsResponse, ListResponse groupResponse,
        ListResponse regionResponse, ListResponse rolesResponse,
        ListResponse accessTypeResponse, bool usePrimaryProperties)
    {
        var propertyList      = new List<string>();
        var propertyGroupList = new List<string>();
        var regionsList       = new List<string>();
        var roleList          = new List<string>();

        if (rolesResponse.Records is not null)
        {
            foreach (var item in rolesResponse.Records.Cast<SharedObjects.Product.Rum.Role>())
            {
                if (item.IsAssigned)
                    roleList.Add(item.Name);
            }
        }

        if (regionResponse.Records is not null)
        {
            foreach (var item in regionResponse.Records.Cast<RumPropertyGroup>())
            {
                if (item.IsAssigned)
                    regionsList.Add(item.Id.ToString());
            }
        }

        if (groupResponse.Records is not null)
        {
            foreach (var item in groupResponse.Records.Cast<RumPropertyGroup>())
            {
                if (item.IsAssigned)
                    propertyGroupList.Add(item.Id.ToString());
            }
        }

        if (accessTypeResponse.Records is not null && propertyGroupList.Count == 0)
        {
            foreach (var item in accessTypeResponse.Records.Cast<ProductRole>())
            {
                if (item.IsAssigned)
                    propertyGroupList.Add(item.ID.ToString());
            }
        }

        if (propsResponse.Records is not null)
        {
            foreach (var item in propsResponse.Records.Cast<RumPropertyGroup>())
            {
                if (item.IsAssigned)
                    propertyList.Add(item.Id.ToString());
            }
        }

        // When source user has all properties unassigned and no groups, treat as "All" (clone-from-all behaviour)
        if (propsResponse.Records is not null && propertyGroupList.Count == 0)
        {
            var records = propsResponse.Records.Cast<RumPropertyGroup>().ToList();
            if (records.Count > 0 && records.All(p => !p.IsAssigned))
                propertyList.Add("All");
        }

        return new ProductBatch
        {
            ProductId    = (int)ProductEnum.UtilityManagement,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList         = propertyList,
                PropertyGroupList    = propertyGroupList,
                RegionList           = regionsList,
                RoleList             = roleList,
                UsePrimaryProperties = usePrimaryProperties
            }
        };
    }

    private static ProductBatch BuildILMBatch(
        ProductEnum ilmProduct, ListResponse propsResponse,
        List<string> productUserRoles, List<string>? propertyGroups,
        bool usePrimaryProperties)
    {
        var propertyList = new List<string>();

        if (propsResponse.Records is not null)
        {
            foreach (var item in propsResponse.Records.Cast<Logic.ProductIntegration.Model.ProductProperties>())
            {
                if (item.IsAssigned)
                    propertyList.Add(item.GetPropertyId.ToString());
            }
        }

        return new ProductBatch
        {
            ProductId    = (int)ilmProduct,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList         = propertyList,
                RoleList             = productUserRoles,
                PropertyGroupList    = propertyGroups,
                UsePrimaryProperties = usePrimaryProperties
            }
        };
    }

    private static ProductBatch BuildKnockCRMBatch(
        ListResponse propGroupResponse, ListResponse rolesResponse,
        int productId, bool usePrimaryProperties)
    {
        var propertyGroupList = new List<string>();
        var roleList          = new List<string>();

        if (rolesResponse.Records is not null)
        {
            foreach (var item in rolesResponse.Records.Cast<Logic.ProductIntegration.Model.ProductRole>())
            {
                if (item.IsAssigned)
                    roleList.Add(item.GetRoleId);
            }
        }

        if (propGroupResponse.Records is not null)
        {
            foreach (var item in propGroupResponse.Records.Cast<ProductPropertyGroups>())
            {
                if (item.IsAssigned)
                    propertyGroupList.Add(item.GetGroupId);
            }
        }

        return new ProductBatch
        {
            ProductId    = productId,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList         = [],
                PropertyGroupList    = propertyGroupList,
                RoleList             = roleList,
                UsePrimaryProperties = usePrimaryProperties
            }
        };
    }

    private static ProductBatch BuildPortfolioManagementBatch(
        List<PAMRolePropertyList> rolePropertyList,
        List<string> roleList,
        bool usePrimaryProperties) =>
        new()
        {
            ProductId    = (int)ProductEnum.PortfolioManagement,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                RolePropertiesList   = rolePropertyList,
                RoleList             = roleList,
                UsePrimaryProperties = usePrimaryProperties
            }
        };

    private static ProductBatch BuildDepositAlternativeBatch(
        List<string>  roles,
        bool          canReceiveMonthlyReport,
        List<string>  properties,
        List<string>? propertyGroups,
        bool          usePrimaryProperties) =>
        new()
        {
            ProductId    = (int)ProductEnum.DepositAlternative,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                RoleList                = roles,
                CanReceiveMonthlyReport = canReceiveMonthlyReport,
                PropertyGroupList       = propertyGroups,
                PropertyList            = properties,
                UsePrimaryProperties    = usePrimaryProperties
            }
        };

    private static ProductBatch BuildClickPayBatch(
        List<OrganizationRole> organizationRoles,
        bool usePrimaryProperties) =>
        new()
        {
            ProductId    = (int)ProductEnum.ClickPay,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                OrganizationRoleList = organizationRoles,
                UsePrimaryProperties = usePrimaryProperties
            }
        };

    private static ProductBatch BuildSelfProvisioningPortalBatch(int productId) =>
        new()
        {
            ProductId    = productId,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList()
        };

    #endregion

    #region Private — Helpers

    /// <summary>
    /// Replaces three duplicate methods: <c>CheckForAllProperties</c>, <c>CheckForAllRegions</c>,
    /// <c>CheckForIsAssignedNewPropertyFlag</c>. Returns the <c>bool</c> value for
    /// <paramref name="key"/> when <paramref name="additional"/> is a
    /// <c>Dictionary&lt;string, bool&gt;</c>; <c>false</c> otherwise.
    /// </summary>
    private static bool GetBoolFlag(object? additional, string key) =>
        additional is Dictionary<string, bool> dict && dict.TryGetValue(key, out var val) && val;

    private static List<string> ExtractStandardV1Roles(ListResponse rolesResponse)
    {
        var result = new List<string>();
        if (rolesResponse.Records is null) return result;
        foreach (var item in rolesResponse.Records.Cast<Logic.ProductIntegration.Model.ProductRole>())
        {
            if (item.IsAssigned)
                result.Add(item.GetRoleId);
        }
        return result;
    }

    private static List<string> ExtractStandardV1Groups(ListResponse groupsResponse)
    {
        var result = new List<string>();
        if (groupsResponse.Records is null) return result;
        foreach (var item in groupsResponse.Records.Cast<Logic.ProductIntegration.Model.ProductPropertyGroups>())
        {
            if (item.IsAssigned)
                result.Add(item.GetGroupId);
        }
        return result;
    }

    #endregion
}
