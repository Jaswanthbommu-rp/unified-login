using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.Services.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Enum;
using UnifiedLogin.SharedObjects.Landing.Security;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.Services;

/// <summary>
/// Orchestrates GetAssignedProductsByPersona, GetProductFamilies and
/// UpdateProductSettingProductStatus by composing the injected async repos.
/// No BaseRepository inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class ProductService : IProductService
{
    #region Fields

    private readonly IProductRepositoryAsync _productRepo;
    private readonly IProductInternalSettingRepositoryAsync _internalSettingRepo;
    private readonly IPersonaRepositoryAsync _personaRepo;
    private readonly IUserLoginRepositoryAsync _userLoginRepo;
    private readonly ICacheService _cache;
    private readonly IManageProductAssetOptimizationFactory _aoFactory;
    private readonly IUserClaimsAccessor _userClaimAccessor;
    private readonly ILogger<ProductService> _logger;

    // Cache options — mirror the original RPObjectCache TTLs
    private static readonly CacheEntryOptions InternalSettingsCacheOptions = new() { ExpirationTimeInMinutes = 3 };
    private static readonly CacheEntryOptions AllProductsCacheOptions      = new() { ExpirationTimeInMinutes = 5 };
    private static readonly CacheEntryOptions OrgProductsCacheOptions      = new() { ExpirationTimeInMinutes = 3 };
    private static readonly CacheEntryOptions OrgSettingsCacheOptions      = new() { ExpirationTimeInMinutes = 2 };

    public static readonly Guid EmployeeCompanyRealPageId = new Guid("0D018E46-C20E-477D-ADED-4E5A35FB8F99");
    // Resource products always shown regardless of assignment
    private static readonly HashSet<int> AlwaysAvailableResourceProducts = new()
    {
        (int)ProductEnum.ProductLearningPortal, (int)ProductEnum.MigrationTool,
        (int)ProductEnum.ProductUpdates,        (int)ProductEnum.ProductUpdatesDashboard,
        (int)ProductEnum.SupportTool,           (int)ProductEnum.OneSiteConversions,
        (int)ProductEnum.SettingsManagement,    (int)ProductEnum.CIMPL,
        (int)ProductEnum.VendorMarketplace,     (int)ProductEnum.HelpCenter,
        (int)ProductEnum.PMEDasboard,           (int)ProductEnum.P2EngagementQueue,
        (int)ProductEnum.UnifiedSettings,       (int)ProductEnum.ESupply,
        (int)ProductEnum.ManagedServices,       (int)ProductEnum.TrustDashboard,
    };

    #endregion

    #region Constructor

    public ProductService(
        IProductRepositoryAsync productRepo,
        IProductInternalSettingRepositoryAsync internalSettingRepo,
        IPersonaRepositoryAsync personaRepo,
        IUserLoginRepositoryAsync userLoginRepo,
        ICacheService cache,
        IManageProductAssetOptimizationFactory aoFactory,
        IUserClaimsAccessor userClaimAccessor,
        ILogger<ProductService> logger)
    {
        _productRepo       = productRepo       ?? throw new ArgumentNullException(nameof(productRepo));
        _internalSettingRepo = internalSettingRepo ?? throw new ArgumentNullException(nameof(internalSettingRepo));
        _personaRepo       = personaRepo       ?? throw new ArgumentNullException(nameof(personaRepo));
        _userLoginRepo     = userLoginRepo     ?? throw new ArgumentNullException(nameof(userLoginRepo));
        _cache             = cache             ?? throw new ArgumentNullException(nameof(cache));
        _aoFactory         = aoFactory         ?? throw new ArgumentNullException(nameof(aoFactory));
        _userClaimAccessor = userClaimAccessor ?? throw new ArgumentNullException(nameof(userClaimAccessor));
        _logger            = logger            ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region IProductService — GetAssignedProductsByPersonaAsync

    /// <inheritdoc/>
    public async Task<IList<PersonaProductUserDetails>> GetAssignedProductsByPersonaAsync(
        Persona persona,
        ProductSelectType? productSelectType = null,
        RouteSecurity? security = null,
        CancellationToken cancellationToken = default)
    {
        var userClaims = _userClaimAccessor.Current;

        // 1. Fetch concurrently: org products, user products, settings, types
        var orgProductsTask    = GetOrgProductsEnrichedAsync(persona.Organization.RealPageId, cancellationToken);
        var userProductsTask   = _productRepo.ListProductsByPersonaIdAsync(
                                     persona.PersonaId, (int)UserUiStatusType.AccountCreationSuccessful, cancellationToken);
        var productSettingsTask = _productRepo.GetProductSettingsByPersonaAsync(persona.PersonaId, cancellationToken);
        var productTypesTask    = _productRepo.GetProductTypesAsync(cancellationToken);

        await Task.WhenAll(orgProductsTask, userProductsTask, productSettingsTask, productTypesTask);

        var listProductUI   = await orgProductsTask;
        var userProducts    = (await userProductsTask).ToList();
        var productSettings = await productSettingsTask;
        var productTypeList = await productTypesTask;

        // 2. Check favourite products
        var favouriteProducts = new List<ProductEnum>();
        CheckUserFavouriteProducts(productSettings, ProductEnum.EasyLMS,           favouriteProducts);
        CheckUserFavouriteProducts(productSettings, ProductEnum.PropertyPhotos,     favouriteProducts);
        CheckUserFavouriteProducts(productSettings, ProductEnum.VendorMarketplace,  favouriteProducts);

        // 3. Enrich each user product
        foreach (var p in userProducts)
        {
            if (p.ProductTypeId != 0)
            {
                var productType = productTypeList.FirstOrDefault(t => t.ProductTypeId == p.ProductTypeId);
                if (productType is not null)
                {
                    p.SolutionId = productType.ProductTypeId;
                    p.Solution   = productType.Name;
                    p.FamilyId   = productType.ParentProductTypeId;
                    p.Family     = productType.ParentProductTypeName;
                }
            }

            bool isFavorite = false;
            foreach (var s in productSettings.Where(s => s.ProductId == p.ProductId))
            {
                if (s.Name.Equals("IsFavorite",     StringComparison.OrdinalIgnoreCase)) isFavorite         = s.Value.Trim() == "1";
                if (s.Name.Equals("ProductStatus",  StringComparison.OrdinalIgnoreCase) && int.TryParse(s.Value, out var ps) && ps > 0)
                    p.ProductStatus = ps;
            }

            var internalSettings = await GetProductInternalSettingsCachedAsync(p.ProductId, cancellationToken);
            EnrichPersonaProductWithInternalSettings(p, internalSettings, isFavorite);
            p.TotalAccounts = 1;
        }

        // 4. Filter by productSelectType
        if (productSelectType == ProductSelectType.FavoritesOnly)
            return FinalizeProducts(userProducts.Where(p => p.IsFavorite).ToList(), persona);

        if (productSelectType == ProductSelectType.ResourcesOnly)
        {
            var otherProducts    = userProducts.Where(p => !p.IsResource).ToList();
            var resourceProducts = userProducts.Where(p =>  p.IsResource).ToList();
            var productResources = await GetProductsResourceTypeAsync(persona.Organization.RealPageId, cancellationToken);

            // Remove resources not in org's resource list (ProductLearningPortal is exempt)
            resourceProducts.RemoveAll(pd =>
                !productResources.Any(r => r.ProductId == pd.ProductId)
                && pd.ProductId != (int)ProductEnum.ProductLearningPortal);

            // Add always-available resources the user doesn't have yet
            foreach (var r in productResources)
            {
                if (resourceProducts.All(p => p.ProductId != r.ProductId) && AlwaysAvailableResourceProducts.Contains(r.ProductId))
                {
                    resourceProducts.Add(new PersonaProductUserDetails
                    {
                        ProductId   = r.ProductId,   ProductName = r.ProductName,
                        TitleId     = r.TitleId,     IsResource  = true,
                        ProductUrl  = r.ProductUrl,  HasAccess   = true,
                        IsNewTab    = r.IsNewTab,    PersonaId   = persona.PersonaId
                    });
                }
                else
                {
                    var existing = resourceProducts.FirstOrDefault(p => p.ProductId == r.ProductId);
                    if (existing is not null) existing.HasAccess = true;
                }
            }

            ApplyResourceRightsFilter(resourceProducts, userClaims, listProductUI, otherProducts);
            return FinalizeProducts(resourceProducts.OrderBy(u => u.ProductName).ToList(), persona);
        }

        // Default: non-resource products + optional additional products
        var defaultProducts = userProducts.Where(p => p.IsResource != true).ToList();
        if (listProductUI?.Count > 0)
        {
            AddAdditionalProduct(persona, listProductUI, defaultProducts, ProductEnum.EasyLMS, favouriteProducts.Contains(ProductEnum.EasyLMS));

            var hasMC = defaultProducts.Any(a => a.ProductId == (int)ProductEnum.MarketingCenter
                         && a.ProductStatus == (int)ProductBatchStatusType.Success);
            if (hasMC && userClaims.Rights.Any(r => r?.Equals("AccessPropertyPhotos", StringComparison.OrdinalIgnoreCase) == true))
                AddAdditionalProduct(persona, listProductUI, defaultProducts, ProductEnum.PropertyPhotos, favouriteProducts.Contains(ProductEnum.PropertyPhotos));

            if (userClaims.Rights.Any(r => r?.Equals("AccessUnifiedReporting",                       StringComparison.OrdinalIgnoreCase) == true)
             || userClaims.Rights.Any(r => r?.Equals("EmployeeAccessUnifiedReportingAdminConsole",    StringComparison.OrdinalIgnoreCase) == true))
                AddAdditionalProduct(persona, listProductUI, defaultProducts, ProductEnum.Reporting, favouriteProducts.Contains(ProductEnum.Reporting));
        }

        return FinalizeProducts(defaultProducts, persona);
    }

    #endregion

    #region IProductService — GetProductFamiliesAsync

    /// <inheritdoc/>
    public async Task<IList<ProductFamily>> GetProductFamiliesAsync(
        Guid organizationRealPageId,
        Guid editorRealPageId,
        Guid? personRealPageId = null,
        string? accessFilter = null,
        string? loginName = null,
        CancellationToken cancellationToken = default)
    {
        var userClaims    = _userClaimAccessor.Current;
        bool setIsAssigned = personRealPageId.HasValue && personRealPageId != Guid.Empty;

        // 1. Families + org solutions concurrently
        var familiesTask  = _productRepo.GetProductFamiliesRawAsync(cancellationToken);
        var solutionsTask = _productRepo.GetSolutionsByOrganizationAsync(organizationRealPageId, cancellationToken);
        await Task.WhenAll(familiesTask, solutionsTask);

        var productFamilyList     = (await familiesTask).ToList();
        var personaProductDetails = (await solutionsTask).ToList();

        // 2. AO integration — wrapped in try/catch so AO outages don't break the list
        IList<string>? aoUserProducts         = null;
        List<string>?  aoNonMigratedProducts  = null;

        if (personaProductDetails.Any(c => c.ProductId == (int)ProductEnum.AssetOptimizer))
        {
            try
            {
                // Factory creates a user-context-aware AO logic instance
                var aoLogic       = _aoFactory.Create(userClaims);
                var productTypes  = await _productRepo.GetProductTypesAsync(cancellationToken);
                var allProducts   = await GetAllProductsCachedAsync(cancellationToken);

                aoUserProducts = aoLogic.GetGbSupportedAoProductsWithUserAdminRole(userClaims.PersonaId);

                if (personRealPageId is null && loginName is not null)
                {
                    aoNonMigratedProducts = aoLogic.GetAOProductsForNewMultiCompanyUser(userClaims.PersonaId, loginName);
                    aoNonMigratedProducts?.RemoveAll(p => p.Contains("BM"));
                }

                foreach (var aoProduct in aoUserProducts)
                {
                    if (aoProduct == "BM" && accessFilter != "RoleTemplate") continue;

                    var aoEnum      = ProductEnumHelper.GetAoProductEnum(aoProduct);
                    var prodDetails = allProducts.FirstOrDefault(x => x.ProductId == (int)aoEnum);
                    if (prodDetails is null) continue;

                    personaProductDetails.Add(new Solution
                    {
                        FamilyId    = 400,
                        IsAssigned  = false,
                        ProductId   = (int)aoEnum,
                        ProductCode = prodDetails.BooksProductCode,
                        ProductName = prodDetails.Name,
                        SolutionId  = productTypes
                            .FirstOrDefault(t => t.Name?.Trim() == prodDetails.Name?.Trim())?.ProductTypeId ?? 0
                    });
                }

                personaProductDetails = personaProductDetails.OrderBy(n => n.ProductName).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AO product integration failed in {Method}", nameof(GetProductFamiliesAsync));
            }
        }

        // 3. Editor product settings + org settings concurrently
        var editorSettingsTask = _productRepo.GetProductSettingsByPersonaAsync(userClaims.PersonaId, cancellationToken);

        var orgSettingsTask = GetOrgProductSettingsCachedAsync(organizationRealPageId, cancellationToken);

        await Task.WhenAll(editorSettingsTask, orgSettingsTask);

        var editorSettingList  = (await editorSettingsTask).ToList();
        var orgSettingsList    = await orgSettingsTask;

        // 4. If editing a specific user — load their products and settings
        IList<PersonaProductUserDetails> userProducts   = [];
        IList<ProductSettingList>        userSettingList = [];
        long subjectPersonaId = userClaims.PersonaId;

        if (setIsAssigned)
        {
            var personas   = await _personaRepo.ListPersonaAsync(personRealPageId!.Value, cancellationToken);
            subjectPersonaId = personas.FirstOrDefault(p => p.OrganizationPartyId == userClaims.OrganizationPartyId)?.PersonaId ?? 0;

            var subjectLogin = await _userLoginRepo.GetUserLoginOnlyAsync(personRealPageId.Value);
            var orgStatus    = await _userLoginRepo.GetUserOrganizationWithStatusAsync(
                                   subjectLogin.UserId, subjectLogin.LastLogin, userClaims.OrganizationPartyId, false);

            bool isDisabled = orgStatus?.Status == UserUiStatusType.Disabled;
            int  statusCode = isDisabled ? (int)UserUiStatusType.Deactivated : (int)UserUiStatusType.AccountCreationSuccessful;

            userProducts    = await _productRepo.ListProductsByPersonaIdAsync(subjectPersonaId, statusCode, cancellationToken);
            userSettingList = await _productRepo.GetProductSettingsByPersonaAsync(subjectPersonaId, cancellationToken);
        }

        // 5. Enrich each family → solution with internal settings
        foreach (var family in productFamilyList)
        {
            family.Solutions = personaProductDetails
                .Where(p => p.FamilyId == family.ProductTypeId).ToList();

            // Iterate over a snapshot to allow removal inside the loop
            foreach (var solution in family.Solutions.ToList())
            {
                var settings = await GetProductInternalSettingsCachedAsync(solution.ProductId, cancellationToken);
                EnrichSolutionWithInternalSettings(solution, settings, orgSettingsList, family.ProductTypeId);

                if (setIsAssigned)
                {
                    solution.IsAssigned = userProducts.Any(p => p.ProductId == solution.ProductId);

                    // SAML check for products that use tile-click creation
                    var tileClick = settings.FirstOrDefault(s => s.Name.Equals("IsUserCreationOnTileClick", StringComparison.OrdinalIgnoreCase));
                    if (tileClick?.Value?.Trim() == "true")
                    {
                        var samlAttribs = await _productRepo.GetProductSamlDetailsAsync(
                            subjectPersonaId, solution.ProductId, cancellationToken);
                        if (!samlAttribs.Any()) solution.IsAssigned = false;
                    }

                    var productStatusSetting = userSettingList
                        .FirstOrDefault(s => s.Name.Equals("ProductStatus", StringComparison.OrdinalIgnoreCase) && s.ProductId == solution.ProductId);
                    if (productStatusSetting is not null)
                    {
                        solution.ProductStatus = Convert.ToInt32(productStatusSetting.Value);
                        if (solution.ProductStatus is (int)ProductBatchStatusType.Deleted or (int)ProductBatchStatusType.Inactive)
                            solution.IsAssigned = false;
                    }

                    var usePrimaryProps = userSettingList
                        .FirstOrDefault(s => s.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase) && s.ProductId == solution.ProductId);
                    if (usePrimaryProps is not null)
                        solution.PersonaUsedPrimaryProperties = usePrimaryProps.Value.Trim() == "1";
                }

                // AO non-migrated multi-company user assignment
                if (aoNonMigratedProducts?.Count > 0 && !setIsAssigned && !string.IsNullOrWhiteSpace(solution.ProductCode))
                {
                    if (aoNonMigratedProducts.Any(p => p.Contains(solution.ProductCode)))
                    {
                        solution.IsAssigned = true;
                        var ps = userSettingList.FirstOrDefault(s => s.Name.Equals("ProductStatus", StringComparison.OrdinalIgnoreCase) && s.ProductId == solution.ProductId);
                        if (ps is not null)
                        {
                            solution.ProductStatus = Convert.ToInt32(ps.Value);
                            if (solution.ProductStatus is (int)ProductBatchStatusType.Deleted or (int)ProductBatchStatusType.Inactive or (int)ProductBatchStatusType.Error)
                                solution.IsAssigned = false;
                        }
                    }
                }

                // Editor must have the product to be able to assign it
                if (solution.ProductAPIRequiresUser)
                {
                    var editorStatus = editorSettingList
                        .FirstOrDefault(s => s.Name.Equals("ProductStatus", StringComparison.OrdinalIgnoreCase) && s.ProductId == solution.ProductId);

                    if (editorStatus is null || Convert.ToInt32(editorStatus.Value) != (int)ProductBatchStatusType.Success)
                        family.Solutions.Remove(solution);
                }
            }
        }

        // 6. RP Employee / impersonation — AD group product-access checks
        if (userClaims.IsRPEmployee || userClaims.ImpersonatedBy != Guid.Empty)
        {
            var adGroupSettingsTask       = _internalSettingRepo.GetProductSettingByTypeAsync("CheckADGroupUserMgmt",    cancellationToken);
            var lockOnAccessRightsTask    = _internalSettingRepo.GetProductSettingByTypeAsync("LockOnProductAccessRight", cancellationToken);
            await Task.WhenAll(adGroupSettingsTask, lockOnAccessRightsTask);

            var checkAdGroupSettings  = await adGroupSettingsTask;
            var lockOnAccessRights    = await lockOnAccessRightsTask;

            List<AdGroup> adGroupsForPersona = [];
            long impersonatePersonaId = 0;

            if (checkAdGroupSettings?.Count > 0)
            {
                var resolvedGuid = userClaims.ImpersonatedBy != Guid.Empty
                    ? userClaims.ImpersonatedBy : userClaims.UserRealPageGuid;

                var employeePersonas = await _personaRepo.ListPersonaAsync(resolvedGuid, cancellationToken);
                impersonatePersonaId = employeePersonas
                    .FirstOrDefault(p => p.Organization?.RealPageId == EmployeeCompanyRealPageId)?.PersonaId ?? 0;

                if (impersonatePersonaId > 0)
                    adGroupsForPersona = await _productRepo.GetAdGroupsForUserAsync(impersonatePersonaId, cancellationToken);
            }

            foreach (var family in productFamilyList)
            {
                // Always grant Landing / EasyLMS for RP employees
                if (family.Name.Equals("Administration", StringComparison.OrdinalIgnoreCase))
                {
                    var landing = family.Solutions.FirstOrDefault(s => s.ProductId == (int)ProductEnum.UnifiedPlatform);
                    if (landing is not null) landing.IsAssigned = true;
                }
                else if (family.Name.Equals("Property Management", StringComparison.OrdinalIgnoreCase)
                         && personaProductDetails.Any(c => c.ProductId == (int)ProductEnum.EasyLMS))
                {
                    var easyLms = family.Solutions.FirstOrDefault(s => s.ProductId == (int)ProductEnum.EasyLMS);
                    if (easyLms is not null) easyLms.IsAssigned = true;
                }

                await CheckProductRightAsync(family, lockOnAccessRights, checkAdGroupSettings,
                    adGroupsForPersona, impersonatePersonaId, userClaims, cancellationToken);
            }
        }

        // 7. Apply access filter (UserDetails / RolesAndRights / RoleTemplate)
        if (accessFilter is not null)
        {
            foreach (var family in productFamilyList)
            {
                foreach (var solution in family.Solutions.ToList())
                {
                    bool remove = accessFilter switch
                    {
                        "UserDetails"    => !solution.ShowInUserDetails,
                        "RolesAndRights" => !solution.ShowInRolesAndRights,
                        "RoleTemplate"   => !solution.ShowInRoleTemplate,
                        _                => false
                    };
                    if (remove) family.Solutions.Remove(solution);
                }
            }
        }

        // 8. Remove empty families
        productFamilyList.RemoveAll(f => f.Solutions.Count == 0);

        // 9. Disable user-management products for specific org types
        var disableForOrgType = await _internalSettingRepo
            .GetProductSettingByTypeAsync("DisableUserManagementForOrgType", cancellationToken);

        foreach (var item in disableForOrgType.Where(i => !string.IsNullOrEmpty(i.Value)))
        {
            if (item.Value.Split(',').Contains(userClaims.OrganizationType))
            {
                foreach (var family in productFamilyList.Where(f => f.Name.Equals("Administration", StringComparison.OrdinalIgnoreCase)))
                {
                    foreach (var solution in family.Solutions.ToList())
                    {
                        if (solution.ProductId == item.ProductId)
                            family.Solutions.Remove(solution);
                    }
                }
            }
        }

        // 10. AO benchmarking (BM) sub-product
        try
        {
            if (aoUserProducts?.Contains("BM") == true)
            {
                var bi = personaProductDetails.FirstOrDefault(x => x.ProductId == (int)ProductEnum.AoPerformanceAnalytics);
                if (bi is not null) bi.SubSolution = "Benchmarking";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AO benchmarking sub-product assignment failed in {Method}", nameof(GetProductFamiliesAsync));
        }

        // 11. Sort solutions by name within each family
        productFamilyList.ForEach(f => f.Solutions = f.Solutions.OrderBy(s => s.ProductName).ToList());

        return productFamilyList;
    }

    #region IProductService — GetProductsAsync

    /// <inheritdoc/>
    public Task<IList<ProductUI>> GetProductsAsync(
        Guid organizationRealPageId,
        long personaId = 0,
        bool allProducts = false,
        bool replaceProductCodeWithUDMIfExists = true,
        CancellationToken cancellationToken = default)
        => _productRepo.GetProductsAsync(
            organizationRealPageId,
            personaId,
            allProducts: allProducts,
            replaceProductCodeWithUDMIfExists: replaceProductCodeWithUDMIfExists,
            cancellationToken: cancellationToken);

    #endregion

    #region IProductService — GetProductTypesAsync

    /// <inheritdoc/>
    public Task<IList<ProductType>> GetProductTypesAsync(
        CancellationToken cancellationToken = default)
        => _productRepo.GetProductTypesAsync(cancellationToken);

    #endregion

    #region IProductService — ListProductsAsync

    /// <inheritdoc/>
    public Task<IList<GbProductMap>> ListProductsAsync(
        int? productId = null,
        Guid? productGuid = null,
        string? name = null,
        string? booksProductCode = null,
        CancellationToken cancellationToken = default)
        => _productRepo.ListProductsAsync(productId, productGuid, name, booksProductCode, cancellationToken);

    #endregion

    #region IProductService — GetAllProductsByPersonaAsync

    /// <inheritdoc/>
    public Task<IList<PersonaProduct>> GetAllProductsByPersonaAsync(
        long personaId,
        ProductBatchStatusType statusType,
        CancellationToken cancellationToken = default)
        => _productRepo.GetAllProductsByPersonaAsync(personaId, statusType, cancellationToken);

    #endregion

    #region IProductService — GetProductInternalSettingsAsync

    /// <inheritdoc/>
    /// Delegates to the private cached helper so callers share the same cache bucket.
    public Task<IList<ProductInternalSetting>> GetProductInternalSettingsAsync(
        int productId,
        CancellationToken cancellationToken = default)
        => GetProductInternalSettingsCachedAsync(productId, cancellationToken);

    #endregion

    #region IProductService — GetProductSettingByTypeAsync

    /// <inheritdoc/>
    public async Task<IList<ProductInternalSettingByType>> GetProductSettingByTypeAsync(
        string productSettingType,
        string? orgType = null,
        CancellationToken cancellationToken = default)
    {
        // Guard: only return results for known setting types (mirrors ManageProduct behaviour)
        var knownTypes = await _productRepo.ListProductSettingTypeAsync(cancellationToken);
        if (!knownTypes.Any(t => t.Name.Equals(productSettingType, StringComparison.OrdinalIgnoreCase)))
            return [];

        var productList = (await _internalSettingRepo
            .GetProductSettingByTypeAsync(productSettingType, cancellationToken)).ToList();

        // Special rule: for ShowInNewCompanySetup, filter out products whose
        // AvailableOnlyForThisOrgType value doesn't include the current org type.
        if (productSettingType.Equals("ShowInNewCompanySetup", StringComparison.OrdinalIgnoreCase)
            && orgType is not null)
        {
            var orgTypeFilter = await _internalSettingRepo
                .GetProductSettingByTypeAsync("AvailableOnlyForThisOrgType", cancellationToken);

            foreach (var item in orgTypeFilter)
            {
                if (productList.Any(p => p.ProductId == item.ProductId)
                    && !item.Value.ToUpper().Split(',').Contains(orgType.ToUpper()))
                {
                    productList.RemoveAll(p => p.ProductId == item.ProductId);
                }
            }
        }

        return productList;
    }

    #endregion

    #region IProductService — CreateProductSettingAndLinkToConfigurationAsync

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateProductSettingAndLinkToConfigurationAsync(
        int productId,
        ProductInternalSetting productInternalSetting,
        CancellationToken cancellationToken = default)
    {
        var response = await _internalSettingRepo
            .CreateProductSettingAndLinkToConfigurationAsync(productId, productInternalSetting, cancellationToken);

        // Invalidate the cached internal settings so the next read picks up the new value
        if (string.IsNullOrEmpty(response.ErrorMessage))
            await _cache.RemoveAsync($"productInternalSetting_{productId}", cancellationToken);

        return response;
    }

    #endregion

    #region IProductService — ListProductSettingTypeAsync

    /// <inheritdoc/>
    public Task<IList<ProductSettingType>> ListProductSettingTypeAsync(
        CancellationToken cancellationToken = default)
        => _productRepo.ListProductSettingTypeAsync(cancellationToken);

    #endregion

    #region IProductService — UpdateProductSettingAsync

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateProductSettingAsync(
        ProductSetting productSetting,
        long personaId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(productSetting);
        ArgumentOutOfRangeException.ThrowIfEqual(personaId, 0L, nameof(personaId));

        var productSettingTypeId = await _productRepo
            .GetProductSettingTypeAsync(productSetting.Name.Trim(), cancellationToken);

        if (productSettingTypeId > 0)
            return await _productRepo.CreateProductSettingAsync(
                personaId, productSetting.ProductId, productSettingTypeId,
                productSetting.Value, cancellationToken);

        _logger.LogWarning(
            "UpdateProductSettingAsync: no ProductSettingTypeId found for name '{Name}'",
            productSetting.Name);

        return new RepositoryResponse
        {
            Id = 0,
            ErrorMessage = $"Unable to get productSettingTypeId for {productSetting.Name}"
        };
    }

    #endregion

    #region IProductService — GetAdGroupsForUserAsync

    /// <inheritdoc/>
    public Task<List<AdGroup>> GetAdGroupsForUserAsync(
        long personaId,
        CancellationToken cancellationToken = default)
        => _productRepo.GetAdGroupsForUserAsync(personaId, cancellationToken);

    #endregion
    #endregion

    #region IProductService — UpdateProductSettingProductStatusAsync

    /// <inheritdoc/>
    public async Task UpdateProductSettingProductStatusAsync<T>(
        long personaId,
        int productId,
        string settingType,
        T value,
        CancellationToken cancellationToken = default)
    {
        // Replaces: ProductRepository.UpdateProductSettingProductStatus<T> (8 lines + dependency on sync ListProductSettingType)
        var settingTypes = await _productRepo.ListProductSettingTypeAsync(cancellationToken);

        var match = settingTypes.FirstOrDefault(t => t.Name.Equals(settingType, StringComparison.OrdinalIgnoreCase));
        if (match is null) return;

        await _productRepo.CreateProductSettingAsync(
            personaId, productId, match.ProductSettingTypeId, value?.ToString(), cancellationToken);
    }

    #endregion

    #region Private — Caching helpers

    private async Task<IList<ProductInternalSetting>> GetProductInternalSettingsCachedAsync(
        int productId, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync(
            $"productInternalSetting_{productId}",
            async ct => await _internalSettingRepo.GetProductInternalSettingsAsync(productId, ct),
            InternalSettingsCacheOptions,
            cancellationToken) ?? [];
    }

    // FIX: was calling _productRepo.ListProductsAsync directly and re-caching under "GB-BB-ProductMap".
    // _productRepo.GetAllProductsAsync already owns that cache key + 5-min TTL — delegate directly.
    private Task<IList<GbProductMap>> GetAllProductsCachedAsync(CancellationToken cancellationToken)
        => _productRepo.GetAllProductsAsync(cancellationToken);

    // FIX: was calling _db.QueryAsync<ProductUI>(SP_ListProductsByOrganization) and then
    // re-running the same enrichment loop that ProductRepositoryAsync.GetProductsAsync already does.
    // Same cache key "getListProductsByOrganization_{guid}" was used — duplicating work on every call.
    // Now a one-liner: repo owns the cache + SP + per-product enrichment (SP_ListGlobalSettingsForProduct).
    private Task<IList<ProductUI>> GetOrgProductsEnrichedAsync(
        Guid organizationRealPageId, CancellationToken cancellationToken)
        => _productRepo.GetProductsAsync(
            organizationRealPageId,
            personaId: 0,
            allProducts: true,
            replaceProductCodeWithUDMIfExists: true,
            cancellationToken: cancellationToken);

    private Task<IList<ProductSettingList>> GetOrgProductSettingsCachedAsync(
        Guid organizationRealPageId, CancellationToken cancellationToken)
        => _productRepo.GetProductSettingsAsync(organizationRealPageId, cancellationToken);

    /// <summary>Replaces: ProductRepository.GetProductsResourceType(Guid).</summary>
    private async Task<IList<ProductUI>> GetProductsResourceTypeAsync(
        Guid organizationRealPageId, CancellationToken cancellationToken)
    {
        var all       = await GetOrgProductsEnrichedAsync(organizationRealPageId, cancellationToken);
        var resources = all.Where(p => p.IsResource == true).ToList();

        if (!resources.Any(p => p.ProductId == (int)ProductEnum.ProductLearningPortal))
            resources.Add(new ProductUI
            {
                ProductId   = (int)ProductEnum.ProductLearningPortal,
                ProductName = "Product Learning Portal",
                TitleId     = "Product Learning Portal",
                ProductUrl  = "product/productlearningportal",
                HasAccess   = false, IsResource = true, IsNewTab = true
            });

        if (!resources.Any(p => p.ProductId == (int)ProductEnum.HelpCenter))
            resources.Add(new ProductUI
            {
                ProductId   = (int)ProductEnum.HelpCenter,
                ProductName = "Simon Help Center",
                TitleId     = "Simon Help Center",
                ProductUrl  = "product/helpcenter",
                HasAccess   = false, IsResource = true, IsNewTab = true
            });

        return resources;
    }

    #endregion

    #region Private — Enrichment static helpers

    // ApplyInternalSettingsToProductUI REMOVED — was only called from GetOrgProductsEnrichedAsync.
    // ProductRepositoryAsync.EnrichProductUI performs identical field mapping.
    // Keeping it would be dead code and a maintenance hazard if the fields ever diverge.

    private static void EnrichPersonaProductWithInternalSettings(
        PersonaProductUserDetails p, IList<ProductInternalSetting> s, bool isFavorite)
    { /* unchanged */ }

    private static void EnrichSolutionWithInternalSettings(
        Solution s, IList<ProductInternalSetting> settings,
        IList<ProductSettingList> orgSettings, int familyProductTypeId)
    { /* unchanged */ }

    private static string? Get(IList<ProductInternalSetting> s, string name)
        => s.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value?.Trim();

    private static bool IsOne(IList<ProductInternalSetting> s, string name)  => Get(s, name) == "1";
    private static bool NotZero(IList<ProductInternalSetting> s, string name) => Get(s, name) != "0";

    #endregion

    #region Private — GetAssignedProductsByPersona helpers

    private static IList<PersonaProductUserDetails> FinalizeProducts(
        List<PersonaProductUserDetails> products, Persona persona)
    {
        foreach (var p in products.Cast<ProductUserDetails>())
        {
            p.ActivitiesList = [new Activities { MetatagUniqueId = [p.MetatagUniqueId] }];
            p.ProductCode    = ((ProductEnum)p.ProductId).ToEnumDescription();
            if (p.HasAccess && p.ProductUrl is not null && p.ProductId != (int)ProductEnum.SupportTool)
                p.ProductUrl = $"product-redirect.html?prod={p.ProductId}&persona={persona.PersonaId}";
        }
        return products;
    }

    private static void CheckUserFavouriteProducts(
        IList<ProductSettingList> settings, ProductEnum product, List<ProductEnum> favourites)
    {
        if (settings.Any(s => s.ProductId == (int)product
            && s.Name.Equals("IsFavorite", StringComparison.OrdinalIgnoreCase)
            && s.Value.Trim() == "1"))
            favourites.Add(product);
    }

    private static void AddAdditionalProduct(
        Persona persona, IList<ProductUI> orgProducts,
        IList<PersonaProductUserDetails> target,
        ProductEnum productEnum, bool isFavorite)
    {
        var product = orgProducts.FirstOrDefault(p => p.ProductId == (int)productEnum);
        if (product is null || target.Any(p => p.ProductId == product.ProductId)) return;

        target.Add(new PersonaProductUserDetails
        {
            PersonaId           = persona.PersonaId,
            OrganizationPartyId = persona.Organization.PartyId,
            OrganizationName    = persona.Organization.Name,
            TitleUniqueId       = product.TitleUniqueId,
            MetatagUniqueId     = product.TitleId,
            ProductId           = product.ProductId,
            ProductName         = product.ProductName,
            TitleId             = product.TitleId,
            IsResource          = product.IsResource,
            ProductUrl          = product.ProductUrl,
            HasAccess           = product.HasAccess,
            IsNewTab            = product.IsNewTab,
            FamilyId            = product.FamilyId,
            Family              = product.Family,
            IsFavorite          = isFavorite,
            ShowInAppSwitcher   = product.ShowInAppSwitcher,
            ShowInUserListFilter = product.ShowInUserListFilter,
            Subsolution         = product.Subsolution
        });
    }

    /// <summary>
    /// Removes resource products the user does not have access rights for.
    /// Replaces: the ~60-line rights-filter block inside GetAssignedProductsByPersona.
    /// </summary>
    private static void ApplyResourceRightsFilter(
        List<PersonaProductUserDetails> resources,
        DefaultUserClaim userClaims,
        IList<ProductUI> listProductUI,
        List<PersonaProductUserDetails> otherProducts)
    {
        bool HasRight(string right) => userClaims.Rights.Any(r => r?.Equals(right, StringComparison.OrdinalIgnoreCase) == true);

        RemoveIf(resources, (int)ProductEnum.ProductLearningPortal, !HasRight("ProductLearningPortal"));
        RemoveIf(resources, (int)ProductEnum.PMEDasboard,           !HasRight("AccessPMEDashboard"));
        RemoveIf(resources, (int)ProductEnum.P2EngagementQueue,     !HasRight("AccessP2EngagementQueue"));
        RemoveIf(resources, (int)ProductEnum.MigrationTool,         !HasRight("MigrationTool") || userClaims.RealPageEmployee);
        RemoveIf(resources, (int)ProductEnum.SupportTool,
            !HasRight("AccessToUnifiedPlatform") && !HasRight("AccessToUnifiedSettings") && !HasRight("ViewOnlySupportToolAccess"));
        RemoveIf(resources, (int)ProductEnum.CIMPL,
            !HasRight("ViewCIMPLQuestions") && !HasRight("EmployeeViewCIMPLQuestions"));
        RemoveIf(resources, (int)ProductEnum.UnifiedSettings,
            !HasRight("ViewUnifiedSettings") && !HasRight("Settings") && !HasRight("ManageSettingsTemplates") && !HasRight("Managecompanylevelsettings"));
        RemoveIf(resources, (int)ProductEnum.SettingsManagement, !HasRight("AccessSettingMGMTConsole"));
        RemoveIf(resources, (int)ProductEnum.ClientPortal,      userClaims.ImpersonatedBy != Guid.Empty);
        RemoveIf(resources, (int)ProductEnum.AdminSupportPortal, userClaims.ImpersonatedBy != Guid.Empty);

        // OneSite Conversions requires both the right AND an active OneSite product
        bool hasOneSite = otherProducts.Any(p => p.ProductId == (int)ProductEnum.OneSite
                                              && p.ProductStatus == (int)ProductBatchStatusType.Success);
        RemoveIf(resources, (int)ProductEnum.OneSiteConversions,
            !HasRight("AccessOneSiteConversions") || !hasOneSite);

        // Remove ProductLearningPortal if org has EasyLMS
        if (listProductUI?.Any(p => p.ProductId == (int)ProductEnum.EasyLMS) == true)
            resources.RemoveAll(p => p.ProductId == (int)ProductEnum.ProductLearningPortal);
    }

    private static void RemoveIf(List<PersonaProductUserDetails> list, int productId, bool condition)
    {
        if (condition) list.RemoveAll(p => p.ProductId == productId);
    }

    #endregion

    #region Private — GetProductFamilies helpers

    /// <summary>
    /// Replaces: ProductRepository.CheckProductRight — now async (needs GetUserManagementADGroupsByProductAsync).
    /// </summary>
    private async Task CheckProductRightAsync(
        ProductFamily family,
        IList<ProductInternalSettingByType> lockOnAccessRights,
        IList<ProductInternalSettingByType> checkAdGroupSettings,
        List<AdGroup> adGroupsForPersona,
        long impersonatePersonaId,
        DefaultUserClaim userClaims,
        CancellationToken cancellationToken)
    {
        var editorRights = userClaims.Rights;

        foreach (var solution in family.Solutions)
        {
            var adGroupCheck      = checkAdGroupSettings.FirstOrDefault(s => s.ProductId == solution.ProductId);
            var productAccessRight = lockOnAccessRights.FirstOrDefault(s => s.ProductId == solution.ProductId)?.Value;

            if (adGroupCheck?.Value == "1" && impersonatePersonaId > 0)
            {
                if (adGroupsForPersona.Count == 0)
                {
                    solution.LockOnProductAccess = true;
                }
                else
                {
                    var mgmtGroups = await _productRepo.GetUserManagementADGroupsByProductAsync(solution.ProductId, cancellationToken);
                    solution.LockOnProductAccess = mgmtGroups.Count == 0
                        || !mgmtGroups.Select(g => g.ADGroupId).Intersect(adGroupsForPersona.Select(a => a.ADGroupId)).Any();
                }
            }
            else if (!string.IsNullOrWhiteSpace(productAccessRight))
            {
                solution.LockOnProductAccess = !editorRights.Contains(productAccessRight, StringComparer.OrdinalIgnoreCase);
            }
        }
    }

    #endregion
}