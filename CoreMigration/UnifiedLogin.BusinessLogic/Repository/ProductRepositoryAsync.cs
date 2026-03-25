using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess.Helper;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.EnterpriseRole;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using UnifiedLogin.SharedObjects.Saml;
using EnterpriseProductUser = UnifiedLogin.SharedObjects.Enterprise.ProductUsers;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Product Repository — pure data-access layer.
/// Faithfully ports <see cref="ProductRepository"/> using Dapper + <see cref="IDbConnectionFactory"/>.
/// <para>Each public method obtains a fresh <see cref="IDbConnection"/> from the factory so that
/// concurrent callers never share a connection, eliminating the MARS requirement.</para>
/// </summary>
public sealed class ProductRepositoryAsync : IProductRepositoryAsync
{
    // ── Fields ───────────────────────────────────────────────────────────
    private readonly IDbConnectionFactory _dbFactory;
    private readonly ICacheService _cache;
    private readonly IProductInternalSettingRepositoryAsync _internalSettingRepo;
    private readonly ILogger<ProductRepositoryAsync> _logger;
    private readonly IUserClaimsAccessor _userClaimAccessor;
    // Cache TTLs match sync RPObjectCache durations
    private static readonly CacheEntryOptions ProductCacheOptions = new() { ExpirationTimeInMinutes = 3 };
    private static readonly CacheEntryOptions ProductTypeCacheOptions = new() { ExpirationTimeInMinutes = 10 };
    private static readonly CacheEntryOptions SettingTypeCacheOptions = new() { ExpirationTimeInMinutes = 2 };
    private static readonly CacheEntryOptions AllProductsCacheOptions = new() { ExpirationTimeInMinutes = 5 };
    private static readonly CacheEntryOptions InternalSettingOptions = new() { ExpirationTimeInMinutes = 3 };
    private static readonly CacheEntryOptions OrgSettingCacheOptions = new() { ExpirationTimeInMinutes = 2 };

    // ── Constructor ───────────────────────────────────────────────────────
    public ProductRepositoryAsync(
        IDbConnectionFactory dbFactory,
        ICacheService cache,
        IProductInternalSettingRepositoryAsync internalSettingRepo,
        IUserClaimsAccessor userClaimAccessor,
        ILogger<ProductRepositoryAsync> logger)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _internalSettingRepo = internalSettingRepo ?? throw new ArgumentNullException(nameof(internalSettingRepo));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userClaimAccessor = userClaimAccessor ?? throw new ArgumentNullException(nameof(userClaimAccessor));
    }

    // ════════════════════════════════════════════════════════════════════
    // Persona products
    // ════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<IList<PersonaProduct>> GetAllProductsByPersonaAsync(
        long personaId, ProductBatchStatusType statusType, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<PersonaProduct>(new CommandDefinition(
            StoredProcNameConstants.SP_GetProductsByPersonaId,
            new { PersonaId = personaId, StatusTypeId = (int)statusType },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<PersonaProductUserDetails>> ListProductsByPersonaIdAsync(
        long personaId, int statusType, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<PersonaProductUserDetails>(new CommandDefinition(
            StoredProcNameConstants.SP_ListProductsByPersonaId,
            new { PersonaId = personaId, ProductStatusValue = statusType },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<bool> IsProductAssignedAsync(
        long personaId, int productStatus, int productId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<PersonaProductUserDetails>(new CommandDefinition(
            StoredProcNameConstants.SP_ListProductsByPersonaId,
            new { PersonaId = personaId, ProductStatusValue = productStatus.ToString() },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.Any(x => x.ProductId == productId);
    }

    // ════════════════════════════════════════════════════════════════════
    // Organisation / company products
    // ════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<IList<ProductUI>> GetProductsByCompanyAsync(
        long organizationPartyId, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(
            $"getProductsByCompanyPartyId_{organizationPartyId}",
            async _ =>
            {
                using var db = _dbFactory.CreateConnection();
                var result = await db.QueryAsync<ProductUI>(new CommandDefinition(
                    StoredProcNameConstants.SP_ListProductsByOrganization,
                    new { PartyId = organizationPartyId },
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
                return (IList<ProductUI>)result.ToList();
            },
            ProductCacheOptions) ?? [];
    }

    /// <inheritdoc/>
    public async Task<IList<int>> GetProductIdsByCompanyAsync(
        Guid organizationRealPageId, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(
            $"getProductIdListByCompanyGuid_{organizationRealPageId}",
            async _ => await FetchProductIdsAsync(null, organizationRealPageId, cancellationToken),
            ProductCacheOptions) ?? [];
    }

    /// <inheritdoc/>
    public async Task<IList<int>> GetProductIdsByCompanyAsync(
        long organizationPartyId, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(
            $"getProductIdsByCompanyPartyId_{organizationPartyId}",
            async _ => await FetchProductIdsAsync(organizationPartyId, null, cancellationToken),
            ProductCacheOptions) ?? [];
    }

    /// <inheritdoc/>
    public async Task<IList<OrganizationProductUser>> GetProductUsersByCompanyAsync(
        long organizationPartyId, string productId, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(
            $"getProductUsersByCompanyPartyId_{organizationPartyId}",
            async _ =>
            {
                using var db = _dbFactory.CreateConnection();
                var result = await db.QueryAsync<OrganizationProductUser>(new CommandDefinition(
                    StoredProcNameConstants.SP_ListProductUsersForOrganization,
                    new { OrgPartyId = organizationPartyId, ProductId = productId },
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
                return (IList<OrganizationProductUser>)result.ToList();
            },
            ProductCacheOptions) ?? [];
    }

    // ════════════════════════════════════════════════════════════════════
    // GetProductsAsync  (with per-product enrichment — mirrors sync)
    // ════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<IList<ProductUI>> GetProductsAsync(
        Guid organizationRealPageId, long personaId = 0,
        bool resourceOnly = false, bool allProducts = false,
        bool replaceProductCodeWithUDMIfExists = true,
        CancellationToken cancellationToken = default)
    {
        // ── 1. Products for the org (cached) ─────────────────────────────
        var cacheKey = $"getListProductsByOrganization_{organizationRealPageId}";
        IList<ProductUI> products = await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                using var db = _dbFactory.CreateConnection();
                var r = await db.QueryAsync<ProductUI>(new CommandDefinition(
                    StoredProcNameConstants.SP_ListProductsByOrganization,
                    new { OrganizationRealPageId = organizationRealPageId },
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
                return (IList<ProductUI>)r.ToList();
            },
            ProductCacheOptions) ?? [];

        // ── 2. Persona product settings + all-products map ────────────────
        IList<ProductSettingList> personaSettings = personaId > 0
            ? await GetProductSettingsByPersonaAsync(personaId, cancellationToken)
            : [];
        var allProductsList = await GetAllProductsAsync(cancellationToken);

        // ── 3. Per-product enrichment (ProductCode, internal settings) ────
        foreach (var p in products)
        {
            p.ProductCode = ProductEnumHelper.GetProductCodeByProductId(p.ProductId, allProductsList);
            p.UDMSourceCode = ProductEnumHelper.GetUDMSourceCodeByProductId(p.ProductId, allProductsList);
            if (replaceProductCodeWithUDMIfExists && !string.IsNullOrEmpty(p.UDMSourceCode))
                p.ProductCode = p.UDMSourceCode;

            foreach (var s in personaSettings.Where(i => i.ProductId == p.ProductId))
                if (s.Name.Equals("ProductStatus", StringComparison.OrdinalIgnoreCase) && Convert.ToInt32(s.Value) > 0)
                    p.ProductStatus = Convert.ToInt32(s.Value);

            var internalSettings = await GetGlobalSettingsCachedAsync(p.ProductId, cancellationToken);
            EnrichProductUI(p, internalSettings);
        }

        // ── 4. Filter (mirrors sync exactly) ─────────────────────────────
        if (allProducts) return products;
        if (!resourceOnly)
            return products.Where(p =>
                p.IsResource != true ||
                p.ProductId == 14 || p.ProductId == 89 ||
                p.ProductId == 104 || p.ProductId == 38).ToList();

        return products;
    }

    /// <inheritdoc/>
    public async Task<IList<ProductUI>> GetProductsResourceTypeAsync(
        Guid organizationRealPageId, CancellationToken cancellationToken = default)
    {
        var products = await GetProductsAsync(organizationRealPageId, resourceOnly: true,
            cancellationToken: cancellationToken);
        var resources = products.Where(p => p.IsResource == true).ToList();

        if (!resources.Any(p => p.ProductId == (int)ProductEnum.ProductLearningPortal))
            resources.Add(new ProductUI
            {
                ProductId = (int)ProductEnum.ProductLearningPortal,
                ProductName = "Product Learning Portal",
                TitleId = "Product Learning Portal",
                ProductUrl = "product/productlearningportal",
                HasAccess = false,
                IsResource = true,
                IsNewTab = true
            });

        if (!resources.Any(p => p.ProductId == (int)ProductEnum.HelpCenter))
            resources.Add(new ProductUI
            {
                ProductId = (int)ProductEnum.HelpCenter,
                ProductName = "Simon Help Center",
                TitleId = "Simon Help Center",
                ProductUrl = "product/helpcenter",
                HasAccess = false,
                IsResource = true,
                IsNewTab = true
            });

        return resources;
    }

    // ════════════════════════════════════════════════════════════════════
    // GetAllProductsAsync / ListProductsAsync / GetBooksMasterProductDetailAsync
    // ════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<IList<GbProductMap>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(
            "GB-BB-ProductMap",
            async _ =>
            {
                var result = await ListProductsAsync(null, null, null, null, cancellationToken);
                return (IList<GbProductMap>)result.ToList();
            },
            AllProductsCacheOptions) ?? [];
    }

    /// <inheritdoc/>
    public async Task<IList<GbProductMap>> ListProductsAsync(
        int? productId, Guid? guid, string name, string booksProductCode,
        CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<GbProductMap>(new CommandDefinition(
            StoredProcNameConstants.SP_ListProduct,
            new { ProductId = productId, ProductGUID = guid, Name = name, BooksProductCode = booksProductCode },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<GbProductMap> GetBooksMasterProductDetailAsync(
        int gbProductId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllProductsAsync(cancellationToken);
        return all.FirstOrDefault(x => x.ProductId == gbProductId);
    }

    // ════════════════════════════════════════════════════════════════════
    // Product settings
    // ════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<IList<ProductSettingList>> GetProductSettingsAsync(
        Guid organizationRealPageId, int productId, CancellationToken cancellationToken = default)
        => await GetProductSettingsAsync(organizationRealPageId, (int?)productId, cancellationToken);

    /// <inheritdoc/>
    public Task<IList<ProductSettingList>> GetProductSettingsAsync(
        Guid organizationRealPageId, CancellationToken cancellationToken = default)
        => GetProductSettingsAsync(organizationRealPageId, (int?)null, cancellationToken);

    /// <inheritdoc/>
    public async Task<IList<ProductSettingList>> GetProductSettingsByPersonaAsync(
        long personaId, CancellationToken cancellationToken = default)
    {
        if (personaId == 0) return [];
        try
        {
            using var db = _dbFactory.CreateConnection();
            var result = await db.QueryAsync<ProductSettingList>(new CommandDefinition(
                StoredProcNameConstants.SP_ListProductSettingsByPersonaId,
                new { PersonaId = personaId },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return result.ToList();
        }
        catch { return []; }
    }

    /// <inheritdoc/>
    public async Task<IList<ProductSettingType>> ListProductSettingTypeAsync(
        CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(
            "listProductSettingType",
            async _ =>
            {
                using var db = _dbFactory.CreateConnection();
                var result = await db.QueryAsync<ProductSettingType>(new CommandDefinition(
                    StoredProcNameConstants.SP_ListProductSettingType,
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
                return (IList<ProductSettingType>)result.ToList();
            },
            SettingTypeCacheOptions) ?? [];
    }

    /// <inheritdoc/>
    public async Task<int> GetProductSettingTypeAsync(
        string productSettingName, CancellationToken cancellationToken = default)
    {
        var param = new DynamicParameters();
        param.Add("@Name", productSettingName, DbType.String, ParameterDirection.Input);
        param.Add("@ProductSettingTypeId", 0, DbType.Int32, ParameterDirection.Output);
        try
        {
            using var db = _dbFactory.CreateConnection();
            await db.ExecuteAsync(new CommandDefinition(StoredProcNameConstants.SP_GetProductSettingType,
                param, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return param.Get<int>("@ProductSettingTypeId");
        }
        catch { return 0; }
    }

    // ════════════════════════════════════════════════════════════════════
    // CreateProductSettingAsync / UpdateProductSettingProductStatusAsync / ClearPersonaErrorAsync
    // ════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateProductSettingAsync(
        long personaId, int productId, int productSettingTypeId, string value,
        CancellationToken cancellationToken = default)
    {
        // All three sequential SP calls share one connection — no concurrent access here.
        var response = new RepositoryResponse();
        try
        {
            using var db = _dbFactory.CreateConnection();

            response = await db.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                StoredProcNameConstants.SP_CreatePersonaConfiguration,
                new { PersonaId = personaId, ProductId = productId },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))
                ?? new RepositoryResponse();
            if (response.Id == 0) { response.ErrorMessage = "CreateProductSetting Error: CreatePersonaConfiguration failed."; return response; }
            var configId = Convert.ToInt32(response.Id);

            response = await db.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                StoredProcNameConstants.SP_CreateProductSetting,
                new { ProductId = productId, ProductSettingTypeId = productSettingTypeId, Value = value, FromDate = DateTime.UtcNow, ProductSettingId = 0 },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))
                ?? new RepositoryResponse();
            if (response.Id == 0) { response.ErrorMessage = "CreateProductSetting Error: CreateProductSetting failed."; return response; }
            var settingId = Convert.ToInt32(response.Id);

            response = await db.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
                StoredProcNameConstants.SP_CreateProductConfigurationbyPersonaId,
                new { PersonaId = personaId, ConfigurationId = configId, ProductId = productId, ProductSettingID = settingId },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))
                ?? new RepositoryResponse();
            if (response.Id == 0) { response.ErrorMessage = "CreateProductSetting Error: CreateProductConfigurationbyPersonaId failed."; return response; }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method}(IRepository) failed personaId={P} productId={Prod}",
                nameof(CreateProductSettingAsync), personaId, productId);
            return new RepositoryResponse { ErrorMessage = $"Create/Update Product Setting Error: {ex.Message}" };
        }
    }

    /// <inheritdoc/>
    public async Task UpdateProductSettingProductStatusAsync<T>(
        long userPersonaId, int productId, string settingType, T value,
        CancellationToken cancellationToken = default)
    {
        var settingTypes = await ListProductSettingTypeAsync(cancellationToken);
        string statusValue = value!.ToString()!;
        var typeEntry = settingTypes.FirstOrDefault(t =>
            t.Name.Equals(settingType, StringComparison.OrdinalIgnoreCase));
        if (typeEntry is not null)
            await CreateProductSettingAsync(userPersonaId, productId, typeEntry.ProductSettingTypeId,
                statusValue, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task ClearPersonaErrorAsync(long personaId, int productId,
        CancellationToken cancellationToken = default)
    {
        var settings = await _internalSettingRepo.GetProductSettingByTypeAsync("WarnOnProductError", cancellationToken);
        if (!settings.Any(p => p.ProductId == productId && p.Value == "1")) return;
        using var db = _dbFactory.CreateConnection();
        await db.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
            StoredProcNameConstants.SP_ManagePersonaProductError,
            new { PersonaId = personaId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
    }

    // ════════════════════════════════════════════════════════════════════
    // Roles / rights
    // ════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<List<ProductRole>> ListRolesForProductByPartyAsync(
        long partyId, IList<int> productIdList, int productId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var rows = (await db.QueryAsync<dynamic>(new CommandDefinition(
            StoredProcNameConstants.SP_ListRolesForProductsByPartyId,
            new
            {
                PartyId = partyId,
                ProductId = productId,
                TargetProductId = TableValueParamHelper.ConvertToTableValuedParameter(
                    productIdList, "enterprise.productidtype")
            },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();

        return rows.Select(item =>
        {
            IList<ProductRoleAttribute> attrs = ParseRoleAttributes(item.RoleAttribute?.ToString());
            bool accessAll = attrs.Any(a =>
                a.AttributeName.Equals("AccessAllProperties", StringComparison.OrdinalIgnoreCase) &&
                a.AttributeValue.Equals("1", StringComparison.OrdinalIgnoreCase));

            return new ProductRole
            {
                ID = item.RoleId.ToString(),
                Name = item.value,
                IsAssigned = false,
                Roletype = item.RoleType,
                DefaultRole = item.DefaultRole.ToString(),
                Alias = item.RoleNickName,
                Description = item.Description,
                accessAllProperties = accessAll
            };
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<RightRoleDetail>> ListRoleWithRightsAsync(
        long partyId, int productId, List<int> productIdList, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var rows = (await db.QueryAsync<dynamic>(new CommandDefinition(
            StoredProcNameConstants.SP_ListRolesAssociatedWithRights,
            new
            {
                PartyId = partyId,
                ProductId = productId,
                TargetProductId = TableValueParamHelper.ConvertToTableValuedParameter(
                    productIdList, "enterprise.productidtype")
            },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();

        return rows.Select(item => new RightRoleDetail
        {
            RoleId = item.RoleId,
            RoleName = item.Role,
            IsAssigned = false,
            RoleType = item.RoleType,
            RightName = item.Right,
            RightId = item.RightId,
            RightValueTypeId = item.RightValueTypeId,
            RightNickName = item.RightNickName
        }).ToList();
    }

    // ════════════════════════════════════════════════════════════════════
    // Internal settings / product error
    // ════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public Task<IList<ProductInternalSetting>> GetProductInternalSettingsAsync(
        int productId, CancellationToken cancellationToken = default)
        => _internalSettingRepo.GetProductInternalSettingsAsync(productId, cancellationToken);

    /// <inheritdoc/>
    public async Task<bool> GetPersonaHasProductErrorAsync(
        long personaId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        return await db.QuerySingleOrDefaultAsync<bool>(new CommandDefinition(
            StoredProcNameConstants.SP_GetPersonaProductError,
            new { PersonaId = personaId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
    }

    // ════════════════════════════════════════════════════════════════════
    // Product families
    // ════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<IList<ProductFamily>> GetProductFamiliesAsync(
        Guid organizationRealPageId, Guid editorRealPageId,
        Guid? personRealPageId = null, string accessFilter = null, string loginName = null,
        CancellationToken cancellationToken = default)
    {
        bool setIsAssigned = personRealPageId.HasValue &&
                             personRealPageId.Value != Guid.Empty;

        // ── Fetch base data ───────────────────────────────────────────────
        // One connection for all sequential direct queries in this method.
        using var db = _dbFactory.CreateConnection();

        var productFamilies = (await db.QueryAsync<ProductFamily>(new CommandDefinition(
            StoredProcNameConstants.SP_ListProductFamilies,
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();

        var orgProducts = (await db.QueryAsync<Solution>(new CommandDefinition(
            StoredProcNameConstants.SP_ListProductsByOrganization,
            new { OrganizationRealPageId = organizationRealPageId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();

        // ── Org-level settings (cached) ───────────────────────────────────
        // Cache lambda gets its own connection so it is safe on a cache miss.
        IList<ProductSettingList> orgSettings = await _cache.GetOrSetAsync(
            $"ProductSettingsByOrganization_{organizationRealPageId}",
            async _ =>
            {
                using var cacheDb = _dbFactory.CreateConnection();
                var r = await cacheDb.QueryAsync<ProductSettingList>(new CommandDefinition(
                    StoredProcNameConstants.SP_ListProductSettingsByOrganization,
                    new { OrganizationRealPageId = organizationRealPageId },
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
                return (IList<ProductSettingList>)r.ToList();
            },
            OrgSettingCacheOptions) ?? [];

        // ── Persona products & settings (only when editing a user) ────────
        long editPersonaId = 0;
        IList<PersonaProductUserDetails> userProducts = [];
        IList<ProductSettingList> personaSettings = [];

        if (setIsAssigned)
        {
            var personas = (await db.QueryAsync<Persona>(new CommandDefinition(
                StoredProcNameConstants.SP_ListPersona,
                new { RealPageId = personRealPageId },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();

            editPersonaId = personas.FirstOrDefault()?.PersonaId ?? 0;

            if (editPersonaId > 0)
            {
                int successStatus = (int)UserUiStatusType.AccountCreationSuccessful;
                userProducts = (await db.QueryAsync<PersonaProductUserDetails>(new CommandDefinition(
                    StoredProcNameConstants.SP_ListProductsByPersonaId,
                    new { PersonaId = editPersonaId, ProductStatusValue = successStatus.ToString() },
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken))).ToList();

                // GetProductSettingsByPersonaAsync creates its own connection.
                personaSettings = await GetProductSettingsByPersonaAsync(editPersonaId, cancellationToken);
            }
        }

        // ── Enrich each solution ──────────────────────────────────────────
        // GetGlobalSettingsCachedAsync and GetProductSamlDetailsAsync each create their own connections.
        if (productFamilies.Count > 0)
        {
            foreach (var family in productFamilies)
            {
                family.Solutions = orgProducts.Where(p => p.FamilyId == family.ProductTypeId).ToList();

                foreach (var s in family.Solutions)
                {
                    var internalSettings = await GetGlobalSettingsCachedAsync(s.ProductId, cancellationToken);

                    s.SubSolution = SettingValue(internalSettings, "SubSolution");
                    s.ShowInUserDetails = SettingBool(internalSettings, "ShowInUserDetails");
                    s.ShowInRolesAndRights = SettingBool(internalSettings, "ShowInRolesAndRights");
                    s.LockOnProductAccess = SettingBoolDefault(internalSettings, "LockOnProductAccess", defaultValue: true);
                    s.ProductAPIRequiresUser = SettingBoolDefault(internalSettings, "ProductAPIRequiresUser", true);
                    s.NotificationEmailRequiredForUserWithNoEmail = SettingBoolDefault(internalSettings, "NotificationEmailRequiredForUserWithNoEmail", true);
                    s.ProductNotAvailableForRegularUserNoEmail = SettingBoolDefault(internalSettings, "ProductNotAvailableForRegularUserNoEmail", true);
                    s.ShowInRoleTemplate = SettingBool(internalSettings, "ShowInRoleTemplate");
                    s.EnableProductForAdminUserEdit = SettingBool(internalSettings, "EnableProductForAdminUserEdit");

                    int orgSettingProductId = family.ProductTypeId == 400
                        ? (int)ProductEnum.AssetOptimizer
                        : s.ProductId;
                    var orgSetting = orgSettings.FirstOrDefault(i =>
                        i.ProductId == orgSettingProductId &&
                        i.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase));
                    s.UsePrimaryProperties = orgSetting?.Value.Trim() == "1";

                    if (setIsAssigned && editPersonaId > 0)
                    {
                        s.IsAssigned = userProducts.Any(u => u.ProductId == s.ProductId);

                        string? tileClickSetting = SettingValue(internalSettings, "IsUserCreationOnTileClick");
                        if (string.Equals(tileClickSetting, "true", StringComparison.OrdinalIgnoreCase))
                        {
                            var saml = await GetProductSamlDetailsAsync(editPersonaId, s.ProductId, cancellationToken);
                            if (!saml.Any()) s.IsAssigned = false;
                        }

                        var statusSetting = personaSettings.FirstOrDefault(i =>
                            i.Name.Equals("ProductStatus", StringComparison.OrdinalIgnoreCase) &&
                            i.ProductId == s.ProductId);
                        if (statusSetting is not null)
                        {
                            s.ProductStatus = Convert.ToInt32(statusSetting.Value);
                            if (s.ProductStatus == (int)ProductBatchStatusType.Deleted ||
                                s.ProductStatus == (int)ProductBatchStatusType.Inactive)
                                s.IsAssigned = false;
                        }

                        var primaryPropSetting = personaSettings.FirstOrDefault(i =>
                            i.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase) &&
                            i.ProductId == s.ProductId);
                        if (primaryPropSetting is not null)
                            s.PersonaUsedPrimaryProperties = primaryPropSetting.Value.Trim() == "1";
                    }
                }
            }
        }

        // ── Access filter ─────────────────────────────────────────────────
        if (accessFilter is not null)
        {
            foreach (var family in productFamilies)
            {
                family.Solutions = accessFilter switch
                {
                    "UserDetails" => family.Solutions.Where(s => s.ShowInUserDetails).ToList(),
                    "RolesAndRights" => family.Solutions.Where(s => s.ShowInRolesAndRights).ToList(),
                    "RoleTemplate" => family.Solutions.Where(s => s.ShowInRoleTemplate).ToList(),
                    _ => family.Solutions
                };
            }
        }

        // ── Remove empty families, sort solutions ─────────────────────────
        productFamilies.RemoveAll(f => f.Solutions.Count == 0);
        productFamilies.ForEach(f =>
            f.Solutions = f.Solutions.OrderBy(s => s.ProductName).ToList());

        return productFamilies;
    }

    /// <inheritdoc/>
    public async Task<IList<ProductFamily>> GetProductFamiliesRawAsync(CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<ProductFamily>(new CommandDefinition(
            StoredProcNameConstants.SP_ListProductFamilies,
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    public async Task<IList<Solution>> GetSolutionsByOrganizationAsync(
        Guid organizationRealPageId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<Solution>(new CommandDefinition(
            StoredProcNameConstants.SP_ListProductsByOrganization,
            new { OrganizationRealPageId = organizationRealPageId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    // ════════════════════════════════════════════════════════════════════
    // SAML
    // ════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<IList<SamlAttributes>> GetProductSamlDetailsAsync(
        long personaId, int productId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<SamlAttributes>(new CommandDefinition(
            StoredProcNameConstants.SP_GetProductSamlDetails,
            new { personaId, productId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<ProductSamlDetails>> ListPersonaProductsSamlDetailsAsync(
        long personaId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<ProductSamlDetails>(new CommandDefinition(
            StoredProcNameConstants.SP_ListPersonaProductsSamlDetails,
            new { PersonaId = personaId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    // ════════════════════════════════════════════════════════════════════
    // Product types
    // ════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<IList<ProductType>> GetProductTypesAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(
            "getProductTypesCache",
            async _ =>
            {
                using var db = _dbFactory.CreateConnection();
                var r = await db.QueryAsync<ProductType>(new CommandDefinition(
                    StoredProcNameConstants.SP_ListProductTypes,
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
                return (IList<ProductType>)r.ToList();
            },
            ProductTypeCacheOptions) ?? [];
    }

    // ════════════════════════════════════════════════════════════════════
    // Shared product ID list
    // ════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<IList<int>> GetProductSharedwithOtherProductIdListAsync(
        IList<int> organizationProducts, CancellationToken cancellationToken = default)
    {
        if (organizationProducts is null || organizationProducts.Count == 0) return [];

        var sharedList = await _internalSettingRepo.GetProductSettingByTypeAsync(
            SettingConstants.SharedProductSettingName, cancellationToken);

        var swapMap = sharedList
            .Where(p => !string.IsNullOrEmpty(p.Value) && int.TryParse(p.Value, out _))
            .ToDictionary(p => p.ProductId, p => int.Parse(p.Value));

        var result = organizationProducts.ToList();
        for (int i = 0; i < result.Count; i++)
            if (swapMap.TryGetValue(result[i], out int target))
                result[i] = target;

        return result;
    }

    // ════════════════════════════════════════════════════════════════════
    // Enterprise roles
    // ════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<RoleTemplate> GetEnterpriseRoleForPersonaAsync(
        long personaId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        return await db.QuerySingleOrDefaultAsync<RoleTemplate>(new CommandDefinition(
            StoredProcNameConstants.SP_GetUserRoleTemplate,
            new { PersonaId = personaId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<List<RoleTemplate>> GetRoleTemplateListAsync(
        long partyId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<RoleTemplate>(new CommandDefinition(
            StoredProcNameConstants.SP_GetRoleTemplate,
            new { PartyId = partyId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<List<RoleTemplateProductRole>> GetRoleTemplateProductRoleMappingAsync(
        int roleTemplateId, long partyId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<RoleTemplateProductRole>(new CommandDefinition(
            StoredProcNameConstants.SP_GetRoleTemplateProductRoleMappings,
            new { RoleTemplateId = roleTemplateId, PartyId = partyId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<List<int>> GetEnterpriseRoleProductsByOrganizationAsync(
        int roleTemplateId, Guid organizationRealPageId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<int>(new CommandDefinition(
            StoredProcNameConstants.SP_GetEnterpriseRoleProductsByOrganization,
            new { RoleTemplateId = roleTemplateId, OrganizationRealPageId = organizationRealPageId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<List<int>> GetEnterpriseRoleProductsByRoleTemplateIdAsync(
        int roleTemplateId, long organizationPartyId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<int>(new CommandDefinition(
            StoredProcNameConstants.SP_GetEnterpriseRoleProductsByOrganization,
            new { RoleTemplateId = roleTemplateId, PartyId = organizationPartyId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<List<int>> GetEnterpriseRoleUpdatedProductsByRoleTemplateIdAsync(
        int roleTemplateId, DateTime createdDateTime, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<int>(new CommandDefinition(
            StoredProcNameConstants.SP_GetEnterpriseRoleUpdatedProductsByRoleTemplateId,
            new { RoleTemplateId = roleTemplateId, CreatedDateTime = createdDateTime },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<List<int>> GetEnterpriseRoleDeletedProductsByRoleTemplateIdAsync(
        int roleTemplateId, DateTime createdDateTime, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<int>(new CommandDefinition(
            StoredProcNameConstants.SP_GetEnterpriseRoleDeletedProductsByRoleTemplateId,
            new { RoleTemplateId = roleTemplateId, CreatedDateTime = createdDateTime },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<List<int>> GetEnterpriseRoleNewProductsByRoleTemplateIdAsync(
        int roleTemplateId, DateTime createdDateTime, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<int>(new CommandDefinition(
            StoredProcNameConstants.SP_GetEnterpriseRoleNewProductsByRoleTemplateId,
            new { RoleTemplateId = roleTemplateId, CreatedDateTime = createdDateTime },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    // ════════════════════════════════════════════════════════════════════
    // AD groups
    // ════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<List<AdGroup>> GetAdGroupsForUserAsync(
        long personaId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<AdGroup>(new CommandDefinition(
            StoredProcNameConstants.SP_GetADGroupsForUser,
            new { PersonaId = personaId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<List<AdGroupProduct>> GetAdGroupsForProductAsync(
        int productId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<AdGroupProduct>(new CommandDefinition(
            StoredProcNameConstants.SP_GetADGroupsForProduct,
            new { ProductId = productId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<List<AdGroup>> GetUserManagementADGroupsByProductAsync(
        long productId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<AdGroup>(new CommandDefinition(
            StoredProcNameConstants.SP_GetUserManagementADGroupsByProduct,
            new { ProductId = productId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<List<ProductAdGroupsCount>> GetPersonaProductsAdGroupsCountAsync(
        long personaId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<ProductAdGroupsCount>(new CommandDefinition(
            StoredProcNameConstants.SP_GetPersonaProductADGroupCount,
            new { PersonaId = personaId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<List<AdGroupRole>> GetAdGroupRolesByPersonaAsync(
        long personaId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<AdGroupRole>(new CommandDefinition(
            StoredProcNameConstants.SP_GetRolesForADGroupByPersona,
            new { PersonaId = personaId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    // ════════════════════════════════════════════════════════════════════
    // Batch / activity log
    // ════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<bool> UpdateProductBatchAsync(
        int productBatchId, int statusTypeId, string inputJson = null, string errorDetails = null,
        CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.ExecuteScalarAsync<int>(new CommandDefinition(
            StoredProcNameConstants.SP_UpdateProductBatch,
            new { productBatchId, statusTypeId, inputJson, errorDetails },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result == 1;
    }

    /// <inheritdoc/>
    public async Task UpdateProductActivityLogAsync(
        long batchProcessorGroupId, int productId, string jsonString,
        CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        await db.ExecuteAsync(new CommandDefinition(
            StoredProcNameConstants.SP_SaveProductActivityLog,
            new { batchProcessorGroupId, productId, jsonstring = jsonString },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<IList<AdditionalParameters>> GetProductActivityLogAsync(
        long batchProcessorGroupId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<AdditionalParameters>(new CommandDefinition(
            StoredProcNameConstants.SP_GetProductActivityLog,
            new { BatchProcessorGroupId = batchProcessorGroupId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task DeleteProductActivityLogAsync(
        long batchProcessorGroupId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        await db.ExecuteAsync(new CommandDefinition(
            StoredProcNameConstants.SP_DeleteProductActivityLog,
            new { batchProcessorGroupId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<bool> SavePersonaProductPropertiesAsync(
        long personaId, int productId, string personaProductProperties,
        CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        return await db.ExecuteScalarAsync<bool>(new CommandDefinition(
            StoredProcNameConstants.SP_SavePersonaProductProperties,
            new { PersonaId = personaId, ProductId = productId, json = personaProductProperties },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<IList<ProductBatchStatus>> ListProductBatchStatusesAsync(
        Guid realPageId, long assignUserPersonaId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<ProductBatchStatus>(new CommandDefinition(
            StoredProcNameConstants.SP_ListProductBatchStatusesByRealPageId,
            new { realPageId, assignUserPersonaId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<RolePropertyList> GetUserProductDataFromProductBatchAsync(
        long personaId, int productId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var json = await db.QuerySingleOrDefaultAsync<string>(new CommandDefinition(
            StoredProcNameConstants.SP_GetUserProductBatchJsonData,
            new { ProductId = productId, PersonaId = personaId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return !string.IsNullOrEmpty(json)
            ? JsonConvert.DeserializeObject<RolePropertyList>(json.Trim())
            : new RolePropertyList();
    }

    /// <inheritdoc/>
    public async Task<IList<UserBatchProductDetail>> GetUserBatchDetailsAsync(
        int batchGroupId, long editorPersonaId, long subjectPersonaId,
        CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<UserBatchProductDetail>(new CommandDefinition(
            StoredProcNameConstants.SP_GetUserBatchRecords,
            new { batchProcessorGroupId = batchGroupId, editorUserPersonId = editorPersonaId, subjectUserPersonId = subjectPersonaId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task UpdateBatchGroupStatusAsync(
        int groupId, bool isLogged, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        await db.ExecuteAsync(new CommandDefinition(
            StoredProcNameConstants.SP_UpdateProcessorGroupStatus,
            new { groupId, activiityLogged = isLogged },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task UpdateBatchProcessorLogAsync(
        int batchProcessorId, DateTime? startDateTime, DateTime? endDateTime,
        CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        await db.ExecuteAsync(new CommandDefinition(
            StoredProcNameConstants.SP_InsertBatchProcessorLog,
            new { batchProcessorId, startDateTime, endDateTime },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<List<PersonaProductProperty>> GetPersonaProductPrimaryPropertiesAsync(
        long personaId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<PersonaProductProperty>(new CommandDefinition(
            StoredProcNameConstants.SP_GetPersonaProductPrimaryProperties,
            new { PersonaId = personaId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    // ════════════════════════════════════════════════════════════════════
    // Enterprise users
    // ════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<IList<EnterpriseProductUser>> GetUsersByCompanyOrProductsAsync(
        string companyId, string upfmId, IList<int> products,
        int rowsPerPage, int pageNumber,
        IList<string> roles, IList<string> rights,
        List<string> propertyIds = null, string companyDomain = null,
        CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<EnterpriseProductUser>(new CommandDefinition(
            EnterpriseStoredProcNameConstants.SP_ListUsersWithCompanyId_Ver3,
            new
            {
                CompanyId = companyId,
                UPFMId = upfmId,
                ProductId = products?.Any() == true ? string.Join(",", products) : null,
                RowsPerPage = rowsPerPage,
                PageNumber = pageNumber,
                Roles = roles?.Any() == true ? string.Join(",", roles) : null,
                Rights = rights?.Any() == true ? string.Join(",", rights) : null,
                Properties = propertyIds?.Any() == true ? string.Join(",", propertyIds) : null,
                CompanyDomain = companyDomain
            },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<EnterpriseProductUser>> GetUsersByCompanyOrProductsAsync(
        string companyId, IList<int?> products,
        string upfmId = null, string userType = null, string userStatus = null,
        CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var (proc, param) = !string.IsNullOrEmpty(upfmId)
            ? (EnterpriseStoredProcNameConstants.SP_ListUsersWithCompanyId_Ver4,
               (object)new { UpfmId = upfmId, UserType = userType, UserStatus = userStatus, ProductId = products?.Any() == true ? string.Join(",", products) : null })
            : (EnterpriseStoredProcNameConstants.SP_ListUsersWithCompanyId_Ver3,
               (object)new { CompanyId = companyId, ProductId = products?.Any() == true ? string.Join(",", products) : null });
        var result = await db.QueryAsync<EnterpriseProductUser>(
            new CommandDefinition(proc, param, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<List<ULMappedPersonaIds>> GetULMappingPersonaIDsByCompanyAndProductsAsync(
        int companyId, string upfmId, int productId, List<string> productUserIds,
        CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var ids = productUserIds?.Count > 0 ? string.Join(",", productUserIds) : string.Empty;
        var (proc, param) = !string.IsNullOrEmpty(upfmId)
            ? (EnterpriseStoredProcNameConstants.SP_ListULMappingPersonaIdForProductUserId_v2,
               (object)new { UPFMId = upfmId, ProductId = productId, TargetProductUserIds = ids })
            : (EnterpriseStoredProcNameConstants.SP_ListULMappingPersonaIdForProductUserId,
               (object)new { CompanyId = companyId, UPFMId = upfmId, ProductId = productId, TargetProductUserIds = ids });
        var result = await db.QueryAsync<ULMappedPersonaIds>(
            new CommandDefinition(proc, param, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task InsertProductLoginActivityByUserAsync(
        int productId, long personaId, long userId, CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        await db.ExecuteAsync(new CommandDefinition(
            StoredProcNameConstants.SP_InsertProductLoginActivitybyUser,
            new { productId, personaId, impersonatorUserId = userId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
    }

    // ════════════════════════════════════════════════════════════════════
    // Explicit interface implementations — fix casing mismatches
    // ════════════════════════════════════════════════════════════════════

    Task<IList<EnterpriseProductUser>> IProductRepositoryAsync.GetUsersByCompanyorProductsAsync(
        string companyId, string upfmId, IList<int> products, int rowsPerPage, int pageNumber,
        IList<string> roles, IList<string> rights, List<string> propertyIds, string companyDomain,
        CancellationToken cancellationToken)
        => GetUsersByCompanyOrProductsAsync(companyId, upfmId, products,
            rowsPerPage, pageNumber, roles, rights, propertyIds, companyDomain, cancellationToken);

    Task<IList<EnterpriseProductUser>> IProductRepositoryAsync.GetUsersByCompanyorProductsAsync(
        string companyId, IList<int?> products,
        string upfmId, string userType, string userStatus, CancellationToken cancellationToken)
        => GetUsersByCompanyOrProductsAsync(companyId, products, upfmId, userType, userStatus, cancellationToken);

    Task IProductRepositoryAsync.InsertProductLoginActivitybyUserAsync(
        int productId, long personaId, long userId, CancellationToken cancellationToken)
        => InsertProductLoginActivityByUserAsync(productId, personaId, userId, cancellationToken);

    // ════════════════════════════════════════════════════════════════════
    // Private helpers
    // ════════════════════════════════════════════════════════════════════

    private async Task<IList<int>> FetchProductIdsAsync(
        long? partyId, Guid? realPageId, CancellationToken cancellationToken)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<ProductUI>(new CommandDefinition(
            StoredProcNameConstants.SP_ListProductsByOrganization,
            new { PartyId = partyId, OrganizationRealPageId = realPageId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        return result.Select(p => p.ProductId).ToList();
    }

    /// <summary>
    /// Fetches SP_ListGlobalSettingsForProduct per product with a 3-minute cache.
    /// The connection is created inside the cache-miss lambda so cache hits incur zero DB cost.
    /// </summary>
    private async Task<IList<ProductInternalSetting>> GetGlobalSettingsCachedAsync(
        int productId, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync(
            $"productInternalSetting_{productId}",
            async _ =>
            {
                using var db = _dbFactory.CreateConnection();
                var r = await db.QueryAsync<ProductInternalSetting>(new CommandDefinition(
                    StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    new { ProductId = productId },
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
                return (IList<ProductInternalSetting>)r.ToList();
            },
            InternalSettingOptions) ?? [];
    }

    private async Task<IList<ProductSettingList>> GetProductSettingsAsync(
        Guid organizationRealPageId, int? productId, CancellationToken cancellationToken)
    {
        try
        {
            using var db = _dbFactory.CreateConnection();
            var result = await db.QueryAsync<ProductSettingList>(new CommandDefinition(
                StoredProcNameConstants.SP_ListProductSettingsByOrganization,
                new { OrganizationRealPageId = organizationRealPageId, ProductId = productId },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed org={Org}", nameof(GetProductSettingsAsync), organizationRealPageId);
            return [];
        }
    }

    private static void EnrichProductUI(ProductUI p, IList<ProductInternalSetting> settings)
    {
        p.ClientId = SettingValue(settings, "ClientId");
        p.ClassName = SettingValue(settings, "ClassName");
        p.IsNewTab = SettingBool(settings, "IsNewTab");
        p.ProductUrl = SettingValue(settings, "ProductUrl");
        p.SettingsUrl = SettingValue(settings, "SettingsUrl");
        p.TitleId = SettingValue(settings, "TitleId");
        p.Subsolution = SettingValue(settings, "Subsolution");
        p.IsAllowFavorite = SettingBool(settings, "IsFavorite");
        p.LearnMore = SettingValue(settings, "LearnMore");
        p.HasAccess = SettingBool(settings, "HasAccess");
        p.ShowInUserListFilter = SettingBool(settings, "ShowInUserListFilter");
        p.ShowInAppSwitcher = SettingBool(settings, "ShowInAppSwitcher");
        p.IsInUDM = SettingBool(settings, "UpdateProductinUDM");

        if (settings.Any(s => s.Name.Equals("IsResource", StringComparison.OrdinalIgnoreCase)))
            p.IsResource = SettingBool(settings, "IsResource");

        Guid.TryParse(SettingValue(settings, "TitleUniqueId"), out Guid titleGuid);
        p.TitleUniqueId = titleGuid;
    }

    private static string? SettingValue(IList<ProductInternalSetting> settings, string name)
        => settings.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value?.Trim();

    private static bool SettingBool(IList<ProductInternalSetting> settings, string name)
        => SettingValue(settings, name) == "1";

    private static bool SettingBoolDefault(IList<ProductInternalSetting> settings, string name, bool defaultValue)
    {
        string? v = SettingValue(settings, name);
        return v is null ? defaultValue : v == "1";
    }

    private static IList<ProductRoleAttribute> ParseRoleAttributes(string? json)
    {
        if (string.IsNullOrEmpty(json)) return [];
        try { return JsonConvert.DeserializeObject<IList<ProductRoleAttribute>>(json) ?? []; }
        catch { return []; }
    }
}