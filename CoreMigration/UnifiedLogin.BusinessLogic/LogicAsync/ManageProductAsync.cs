using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.Services.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first implementation of <see cref="IManageProductAsync"/>.
/// Replaces the <c>Task.FromResult</c> stepping-stone. All DB calls are awaited via
/// injected async interfaces. Three sync dependencies remain until their async counterparts exist:
/// <see cref="IProductRepository"/> (GetAssignedProductsByPersona — commented out in async repo),
/// <see cref="IManageUserRoleRight"/> and <see cref="IUnifiedSettingsRepository"/>.
/// </summary>
public sealed class ManageProductAsync : IManageProductAsync
{
    #region Fields

    private readonly IProductRepositoryAsync                  _productRepository;
    private readonly IProductInternalSettingRepositoryAsync   _productInternalSettingRepository;
    private readonly IManagePersonaAsync                      _managePersona;
    private readonly IManageBlueBookAsync                     _manageBlueBook;
    private readonly IManageProfileAsync                      _manageProfile;
    private readonly IOrganizationRepositoryAsync             _organizationRepository;
    private readonly ICacheService                            _cache;
    private readonly IProductService                          _productService;
    private readonly IManageUserRoleRightAsync                       _manageUserRoleRight;
    private readonly IUnifiedSettingsRepositoryAsync               _unifiedSettingsRepository;
    private readonly ILogger<ManageProductAsync>              _logger;

    private static readonly Guid EmployeeCompanyRealPageId = new("0D018E46-C20E-477D-ADED-4E5A35FB8F99");
    private static readonly CacheEntryOptions ProductSettingsCacheOptions = new() { ExpirationTimeInMinutes = 2 };
    private readonly Lock _rightLock = new();

    #endregion

    #region Constructor

    public ManageProductAsync(
        IProductRepositoryAsync                 productRepository,
        IProductInternalSettingRepositoryAsync  productInternalSettingRepository,
        IManagePersonaAsync                     managePersona,
        IManageBlueBookAsync                    manageBlueBook,
        IManageProfileAsync                     manageProfile,
        IOrganizationRepositoryAsync            organizationRepository,
        ICacheService                           cache,
        IManageUserRoleRightAsync               manageUserRoleRight,
        IUnifiedSettingsRepositoryAsync         unifiedSettingsRepository,
        IProductService                         productService,
        ILogger<ManageProductAsync>             logger)
    {
        _productRepository                = productRepository                ?? throw new ArgumentNullException(nameof(productRepository));
        _productInternalSettingRepository = productInternalSettingRepository ?? throw new ArgumentNullException(nameof(productInternalSettingRepository));
        _managePersona                    = managePersona                    ?? throw new ArgumentNullException(nameof(managePersona));
        _manageBlueBook                   = manageBlueBook                   ?? throw new ArgumentNullException(nameof(manageBlueBook));
        _manageProfile                    = manageProfile                    ?? throw new ArgumentNullException(nameof(manageProfile));
        _organizationRepository           = organizationRepository           ?? throw new ArgumentNullException(nameof(organizationRepository));
        _cache                            = cache                            ?? throw new ArgumentNullException(nameof(cache));
      //  _productRepositorySync            = productRepositorySync            ?? throw new ArgumentNullException(nameof(productRepositorySync));
        _manageUserRoleRight              = manageUserRoleRight              ?? throw new ArgumentNullException(nameof(manageUserRoleRight));
        _unifiedSettingsRepository        = unifiedSettingsRepository        ?? throw new ArgumentNullException(nameof(unifiedSettingsRepository));
        _logger                           = logger                           ?? throw new ArgumentNullException(nameof(logger));
        _productService                   = productService                   ?? throw new ArgumentNullException(nameof(productService));
    }

    #endregion

    #region Products

    /// <inheritdoc/>
    public async Task<IList<ProductUI>> GetProductsAsync(
        Guid realPageId, long personaId, bool allProducts,
        bool replaceProductCodeWithUDMIfExists = true,
        CancellationToken cancellationToken = default)
    {
        if (realPageId == Guid.Empty)
            throw new ArgumentException("Null realPageId.", nameof(realPageId));

        var productList = (await _productService.GetProductsAsync(
            realPageId, personaId, allProducts, replaceProductCodeWithUDMIfExists, cancellationToken)).ToList();

        if (personaId <= 0) return productList;

        var persona = await _managePersona.GetPersonaAsync(personaId, cancellationToken: cancellationToken);
        if (persona is null) throw new InvalidOperationException($"Persona {personaId} not found.");

        // GetAssignedProductsByPersona is not yet in IProductRepositoryAsync — sync fallback
        // TODO: await _productRepository.GetAssignedProductsByPersonaAsync once uncommented
        var personaProducts = await _productService.GetAssignedProductsByPersonaAsync(persona,null,null, cancellationToken);

        foreach (var product in productList)
        {
            var personaProduct = personaProducts.SingleOrDefault(u => u.ProductId == product.ProductId);
            if (personaProduct is not null)
            {
                product.HasAccess  = personaProduct.HasAccess;
                product.ProductUrl = personaProduct.ProductUrl;
            }

            if (personaProducts.SingleOrDefault(u => u.ProductId == product.ProductId && u.IsFavorite) is { } fav)
                product.IsFavorite = fav.IsFavorite;
        }

        try
        {
            if (productList.Any(x => x.ProductId == (int)ProductEnum.AssetOptimizer))
            {
                var aoProductList = personaProducts
                    .Where(y => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)y.ProductId))
                    .ToList();

                foreach (var ao in aoProductList)
                {
                    productList.Add(new ProductUI
                    {
                        ProductId    = ao.ProductId,   ProductName  = ao.ProductName,
                        FamilyId     = ao.FamilyId,    Solution     = ao.Solution,
                        ProductTypeId = ao.ProductTypeId, IsFavorite = ao.IsFavorite,
                        SolutionId   = ao.SolutionId,  ActivitiesList = ao.ActivitiesList,
                        ClassName    = ao.ClassName,   ClientId     = ao.ClientId,
                        Family       = ao.Family,      HasAccess    = true,
                        IsAllowFavorite = ao.IsAllowFavorite, IsNewTab = ao.IsNewTab,
                        IsResource   = ao.IsResource,  LearnMore    = ao.LearnMore,
                        ProductDescription = ao.ProductDescription, ProductStatus = ao.ProductStatus,
                        ProductUrl   = ao.ProductUrl,  SettingsUrl  = ao.SettingsUrl,
                        Subsolution  = ao.Subsolution, TitleId      = ao.TitleId,
                        TitleUniqueId = ao.TitleUniqueId, ShowInAppSwitcher = ao.ShowInAppSwitcher,
                        ShowInUserListFilter = ao.ShowInUserListFilter
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AO product enrichment skipped for persona {PersonaId}", personaId);
        }

        return productList.OrderBy(e => e.IsFavorite ? 0 : 1).ThenBy(e => e.TitleId).ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<ProductFamily>> GetProductFamiliesAsync(
        Guid organizationRealPageId, Guid realpageUserId, Guid? personRealPageId,
        string accessFilter = null, string loginName = null,
        CancellationToken cancellationToken = default)
        => await _productRepository.GetProductFamiliesAsync(
            organizationRealPageId, realpageUserId, personRealPageId, accessFilter, loginName, cancellationToken);

    /// <inheritdoc/>
    public async Task<IList<GbProductMap>> ListProductsAsync(CancellationToken cancellationToken = default)
        => await _productRepository.ListProductsAsync(null, null, null, null, cancellationToken);

    /// <inheritdoc/>
    public async Task<IList<ProductType>> GetProductTypesAsync(CancellationToken cancellationToken = default)
        => await _productRepository.GetProductTypesAsync(cancellationToken);

    #endregion

    #region Product Internal Settings

    /// <inheritdoc/>
    public async Task<List<ProductInternalSetting>> GetProductInternalSettingsAsync(
        int productId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"productInternalSetting_{productId}";
        var result = await _cache.GetOrSetAsync(
            cacheKey,
            async _ => await _productInternalSettingRepository
                .GetProductInternalSettingsAsync(productId, cancellationToken),
            ProductSettingsCacheOptions);

        return (result ?? []).ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<ProductInternalSettingByType>> GetProductSettingByTypeAsync(
        string productSettingType, string orgType = null, CancellationToken cancellationToken = default)
    {
        var settingTypes = await ListProductSettingTypeAsync(cancellationToken);
        if (!settingTypes.Any(pst => pst.Name.Equals(productSettingType, StringComparison.OrdinalIgnoreCase)))
            return [];

        var productList = (await _productInternalSettingRepository
            .GetProductSettingByTypeAsync(productSettingType, cancellationToken)).ToList();

        if (productSettingType == "ShowInNewCompanySetup" && orgType is not null)
        {
            var availableOnlyRules = await _productInternalSettingRepository
                .GetProductSettingByTypeAsync("AvailableOnlyForThisOrgType", cancellationToken);

            foreach (var rule in availableOnlyRules)
            {
                if (productList.Any(p => p.ProductId == rule.ProductId)
                    && !rule.Value.ToUpperInvariant().Split(',').Contains(orgType.ToUpperInvariant()))
                {
                    productList = productList.Where(p => p.ProductId != rule.ProductId).ToList();
                }
            }
        }

        return productList;
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateProductSettingAndLinkToConfigurationAsync(
        int productId, ProductInternalSetting productInternalSetting,
        CancellationToken cancellationToken = default)
    {
        var response = await _productInternalSettingRepository
            .CreateProductSettingAndLinkToConfigurationAsync(productId, productInternalSetting, cancellationToken);

        if (string.IsNullOrEmpty(response.ErrorMessage))
            await _cache.RemoveAsync($"productInternalSetting_{productId}");

        return response;
    }

    /// <inheritdoc/>
    public async Task<IList<ProductSettingType>> ListProductSettingTypeAsync(
        CancellationToken cancellationToken = default)
        => await _productRepository.ListProductSettingTypeAsync(cancellationToken);

    #endregion

    #region Product Users

    /// <inheritdoc/>
    public async Task<IList<ProductUsers>> GetProductUsersAsync(
        int productId, long companyInstanceId, long personaId = 0,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(typeof(ProductEnum), productId))
            throw new ArgumentException("Invalid parameter ProductId.", nameof(productId));
        if (companyInstanceId < -1 || companyInstanceId == 0)
            throw new ArgumentException("Invalid parameter blueBook Company InstanceId.", nameof(companyInstanceId));
        if (personaId < 0)
            throw new ArgumentException("Invalid parameter PersonaId.", nameof(personaId));

        // ── 1. Resolve company ────────────────────────────────────────────
        Guid companyRealPageId;
        if (companyInstanceId != -1)
        {
            var orgList = await _organizationRepository.GetUnifiedLoginCompanyListAsync();
            var ulc = orgList.FirstOrDefault(p =>
                p.Domain.Equals("Primary", StringComparison.OrdinalIgnoreCase)
                && p.BooksCustomerMasterId == companyInstanceId)
                ?? throw new InvalidOperationException("No company could be found.");
            companyRealPageId = new Guid(ulc.CompanyRealPageId);
        }
        else
        {
            companyRealPageId = EmployeeCompanyRealPageId;
        }

        var organization = await _organizationRepository.GetOrganizationAsync(realPageId: companyRealPageId);

        IList<RightRoleDetail> rightRoleDetails = [];
        if (organization is not null)
        {
            var productIds = (await _productRepository.GetProductIdsByCompanyAsync(organization.RealPageId, cancellationToken)).ToList();
            rightRoleDetails = await _productRepository.ListRoleWithRightsAsync(organization.PartyId, productId, productIds, cancellationToken);
        }

        // ── 2. Resolve personas ───────────────────────────────────────────
        IList<Persona> personas;
        long? resolvedPersonaId = null;

        if (personaId > 0)
        {
            resolvedPersonaId = personaId;
            var p = await _managePersona.GetPersonaAsync(personaId, cancellationToken: cancellationToken);
            personas = p is not null ? [p] : [];
        }
        else
        {
            personas = await _managePersona.ListPersonaByOrganizationPartyIdAsync(organization!.PartyId, cancellationToken: cancellationToken);
        }

        // ── 3. Build product user + role list ─────────────────────────────
        var productUsers = await _manageProfile.ListPersonsByProductIdAsync(
            productId, organization!.RealPageId, resolvedPersonaId, cancellationToken);

        // IManageUserRoleRight has no async version yet — sync call preserved
        // TODO: await _manageUserRoleRight.GetAssignedRoleForPersonaAsync once available
        var roleList = await _manageUserRoleRight
            .GetAssignedRoleForPersonaAsync((ProductEnum)productId, resolvedPersonaId, organization.PartyId, cancellationToken);

        foreach (var role in roleList)
        {
            var rightsForRole = rightRoleDetails.Where(r => r.RoleId == role.RoleID).ToList();
            foreach (var right in rightsForRole)
            {
                lock (_rightLock)
                {
                    if (!role.Right.Any(r =>
                        r.RightId == right.RightId
                        && r.RightName.Equals(right.RightName, StringComparison.OrdinalIgnoreCase)
                        && r.RightValueTypeId == right.RightValueTypeId
                        && r.RightNickName.Equals(right.RightNickName, StringComparison.OrdinalIgnoreCase)))
                    {
                        role.Right.Add(new Right
                        {
                            RightId          = right.RightId,
                            RightName        = right.RightName,
                            RightValueTypeId = right.RightValueTypeId,
                            RightNickName    = right.RightNickName
                        });
                    }
                }
            }
        }

        foreach (var user in productUsers)
        {
            var userPersonas = personas.Where(p => p.UserId == user.userLogin.UserId).ToList();
            foreach (var p in userPersonas)
            {
                user.persona.Add(new PersonaCommon
                {
                    PersonaId           = p.PersonaId,
                    PersonPartyId       = p.PersonaTypeId,
                    RealPageId          = p.RealPageId,
                    OrganizationPartyId = p.OrganizationPartyId,
                    Name                = p.Name,
                    UserId              = p.UserId,
                    Role                = roleList.Where(r => Convert.ToInt64(r.PersonaId) == p.PersonaId).ToList()
                });
            }
        }

        return productUsers;
    }

    #endregion

    #region Enrichment

    /// <inheritdoc/>
    public async Task<IList<ProductUI>> AddProductSourceAndGreenBookCareFlagToProductsAsync(
        Guid realPageId, long partyId, IList<ProductUI> products,
        CancellationToken cancellationToken = default)
    {
        // ── Concurrent fetches that don't depend on each other ────────────
        var companyInstanceTask   = _manageBlueBook.GetCompanyInstanceByUPFMCompanyIdAsync(realPageId.ToString().ToLower(), cancellationToken);
        var productSettingTask    = _productInternalSettingRepository.GetProductSettingByTypeAsync("UsePrimaryProperties", cancellationToken);
        var companyProductsTask   = _productRepository.GetProductSettingsAsync(realPageId, cancellationToken);

        await Task.WhenAll(companyInstanceTask, productSettingTask, companyProductsTask);

        var booksCompanyInstance  = await companyInstanceTask;
        var productGlobalSettings = await productSettingTask;
        var companyProductSettings = await companyProductsTask;

        int    customerCompanyId = booksCompanyInstance?.Attributes?.CustomerCompanyMap?.FirstOrDefault()?.CustomerCompanyId ?? 0;
        string domain            = booksCompanyInstance?.Attributes?.Domain;

        // IUnifiedSettingsRepository has no async version yet — sync fallback
        // TODO: await _unifiedSettingsRepository.GetUnifiedSettingsAsync once available
        int organizationUsePrimaryProperties = -1;
        var settings = await _unifiedSettingsRepository.GetUnifiedSettingsAsync(partyId, "Company", cancellationToken);
        if (settings.FirstOrDefault(a => a.Name.Equals("PrimaryProperty", StringComparison.OrdinalIgnoreCase)) is { } primarySetting)
            int.TryParse(primarySetting.Value, out organizationUsePrimaryProperties);

        IList<CustomerCompanyMap> customerCompanyMap = [];
        if (!string.IsNullOrEmpty(domain) && customerCompanyId != 0)
        {
            customerCompanyMap = await _manageBlueBook
                .GetCustomerCompanyMapByCustomerCompanyIdAsync(customerCompanyId, domain, cancellationToken);
        }

        foreach (var product in products)
        {
            if (organizationUsePrimaryProperties >= 0)
            {
                string globalStr = productGlobalSettings?
                    .FirstOrDefault(p => p.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase)
                                      && p.ProductId == product.ProductId)?.Value?.Trim();

                int.TryParse(companyProductSettings?
                    .FirstOrDefault(p => p.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase)
                                      && p.ProductId == product.ProductId)?.Value?.Trim(),
                    out int companyProductUsePrimaryProperty);

                if (globalStr is not null
                    && int.TryParse(globalStr, out int globalValue)
                    && globalValue >= 0)
                {
                    product.UsePrimaryProperties = globalValue == 1
                        && organizationUsePrimaryProperties == 1
                        && companyProductUsePrimaryProperty == 1;
                }
            }

            if (!string.IsNullOrEmpty(domain) && customerCompanyId != 0)
            {
                var match = customerCompanyMap
                    .Where(p => p.Source == (!string.IsNullOrEmpty(product.UDMSourceCode)
                        ? product.UDMSourceCode
                        : product.ProductCode))
                    .ToList();

                if (match.Count == 1)
                {
                    product.ProductInstance = match[0].CompanyInstanceSourceId;
                    product.GreenBookCares  = match[0].CompanyInstance.FirstOrDefault()?.GreenBookCares ?? false;
                }

                product.ProductCode = !string.IsNullOrEmpty(product.UDMSourceCode)
                    ? $"{product.UDMSourceCode} ( {product.ProductCode} )"
                    : product.ProductCode;
            }
        }

        return products;
    }

    #endregion

    #region Persona Products

    /// <inheritdoc/>
    public async Task<IList<PersonaProductUserDetails>> GetUserAssignedProductsByPersonaAsync(
        Persona persona,
        ProductSelectType? productSelectType = null,
        RouteSecurity? security = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(persona);

        var userProducts = (await _productService
            .GetAssignedProductsByPersonaAsync(persona, productSelectType, security, cancellationToken)
            .ConfigureAwait(false)).ToList();

        userProducts.RemoveAll(x =>
            x.ProductId == (int)ProductEnum.AssetOptimizer ||
            x.ProductId == (int)ProductEnum.AoBenchmarking);

        return userProducts
            .Where(p => p.ProductStatus != (int)ProductBatchStatusType.Deleted
                     && p.ProductStatus != (int)ProductBatchStatusType.Inactive)
            .OrderBy(e => e.IsFavorite ? 0 : 1)
            .ThenBy(e => e.TitleId)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateProductSettingAsync(
        ProductSetting productSetting,
        long? personaId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(productSetting);
        if (personaId is null)
            throw new ArgumentNullException(nameof(personaId));

        var typeId = await _productRepository
            .GetProductSettingTypeAsync(productSetting.Name.Trim(), cancellationToken)
            .ConfigureAwait(false);

        if (typeId > 0)
            return await _productRepository
                .CreateProductSettingAsync(personaId.Value, productSetting.ProductId, typeId, productSetting.Value, cancellationToken)
                .ConfigureAwait(false);

        return new RepositoryResponse
        {
            ErrorMessage = $"Unable to get productSettingTypeId for {productSetting.Name}",
            Id = 0
        };
    }

    /// <inheritdoc/>
    public Task<IList<PersonaProduct>> GetAllProductsByPersonaAsync(
        long personaId,
        ProductBatchStatusType statusType,
        CancellationToken cancellationToken = default)
        => _productService.GetAllProductsByPersonaAsync(personaId, statusType, cancellationToken);

    #endregion

    #region AD Groups

    /// <inheritdoc/>
    public async Task<List<AdGroupProduct>> GetAdGroupsForProductAsync(
        int productId, CancellationToken cancellationToken = default)
        => await _productRepository.GetAdGroupsForProductAsync(productId, cancellationToken);

    /// <inheritdoc/>
    public async Task<List<AdGroup>> GetAdGroupsForUserAsync(
        long personaId, CancellationToken cancellationToken = default)
        => await _productRepository.GetAdGroupsForUserAsync(personaId, cancellationToken);

    #endregion
}
