using JsonApiSerializer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Accounting;
using UnifiedLogin.SharedObjects.Product.Ops;
using UnifiedLogin.SharedObjects.Product.Rum;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using Company = UnifiedLogin.SharedObjects.BlackBook.Company;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first replacement for <see cref="ManageBlueBook"/>.
/// <para>Key improvements over the sync version:</para>
/// <list type="bullet">
///   <item>All HTTP calls use <c>await</c> — no more <c>.Result</c> deadlock hazard.</item>
///   <item><c>MemoryCache.Default</c> + <c>RPObjectCache</c> replaced by injected <see cref="IMemoryCache"/>.</item>
///   <item><c>Parallel.ForEach</c> with blocking HTTP → <see cref="Parallel.ForEachAsync"/>.</item>
///   <item><c>string.Join</c> replaces manual ID-building loops.</item>
///   <item>Product settings loaded once on first use via a thread-safe lazy initializer.</item>
///   <item>Single DI constructor — no <c>new</c> keyword anywhere.</item>
///   <item>Generic <c>ApplyTranslation</c> eliminates per-type repetition in <c>TranslateProductPrimaryPropertiesDataAsync</c>.</item>
/// </list>
/// </summary>
public sealed class ManageBlueBookAsync : IManageBlueBookAsync, IDisposable
{
    #region Fields

    private readonly HttpClient _httpClient;
    private readonly IProductInternalSettingRepositoryAsync _internalSettingRepo;
    private readonly IProductRepositoryAsync _productRepo;
    private readonly IPropertyRepositoryAsync _propertyRepo;
    private readonly IMemoryCache _cache;
    private readonly IUserClaimsAccessor _userClaimAccessor;
    private readonly ILogger<ManageBlueBookAsync> _logger;

    // Lazy settings initializer — loaded once on first HTTP call, thread-safe
    private IList<ProductInternalSetting>? _settings;
    private bool _useDomains;
    private bool _useUPFMId;
    private bool _settingsLoaded;
    private readonly SemaphoreSlim _settingsLock = new(1, 1);

    private static readonly JsonApiSerializerSettings JsonApiSettings = new();
    private const int CacheTimeSeconds = 300;
    private const int LongCacheSeconds = 1800;
    private const int MaxRetryCount = 5;

    #endregion

    #region Constructor

    /// <param name="httpClient">
    /// Named client <c>"ManageBlueBook"</c> registered in DI via
    /// <c>services.AddHttpClient("ManageBlueBook", c => c.BaseAddress = ...)</c>
    /// with the base address and API key header resolved from product settings at startup.
    /// </param>
    public ManageBlueBookAsync(
        HttpClient httpClient,
        IProductInternalSettingRepositoryAsync internalSettingRepo,
        IProductRepositoryAsync productRepo,
        IPropertyRepositoryAsync propertyRepo,
        IMemoryCache cache,
        IUserClaimsAccessor userClaimAccessor,
        ILogger<ManageBlueBookAsync> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _internalSettingRepo = internalSettingRepo ?? throw new ArgumentNullException(nameof(internalSettingRepo));
        _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
        _propertyRepo = propertyRepo ?? throw new ArgumentNullException(nameof(propertyRepo));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _userClaimAccessor = userClaimAccessor ?? throw new ArgumentNullException(nameof(userClaimAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManageBlueBookAsync — Company map
    // ════════════════════════════════════════════════════════════════════════

    #region GetCompanyMapAsync

    /// <inheritdoc/>
    public Task<IList<CustomerCompanyMap>?> GetCompanyMapAsync(
        Guid companyRealPageId, long booksCompanyMasterId, string domain,
        CancellationToken cancellationToken = default)
        => GetCompanyMapAsync(companyRealPageId, booksCompanyMasterId,
            source: null, domain: domain, cancellationToken: cancellationToken);

    /// <inheritdoc/>
    public async Task<IList<CustomerCompanyMap>?> GetCompanyMapAsync(
        Guid companyRealPageId, long booksCompanyMasterId,
        string? source, string domain,
        string includeExtra = "", bool includeGreenBookCares = true, bool useTranslate = true,
        CancellationToken cancellationToken = default)
    {
        await EnsureSettingsLoadedAsync(cancellationToken);

        if (companyRealPageId == DefaultUserClaim.ContractCompanyRealPageId)
            return null;

        // UPFM translate shortcut
        if (useTranslate && _useUPFMId && companyRealPageId != Guid.Empty
            && string.IsNullOrEmpty(includeExtra)
            && !string.IsNullOrEmpty(source)
            && !source.Equals(ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)))
        {
            var translated = await TranslateFromUPFMToProductAsync(companyRealPageId.ToString(), source, cancellationToken);
            if (translated != null || companyRealPageId == DefaultUserClaim.EmployeeCompanyRealPageId)
                return translated;
        }

        if (_useUPFMId && companyRealPageId != Guid.Empty)
        {
            long masterId = await GetCompanyMasterIdForRPDMIDAsync(companyRealPageId.ToString(), domain, cancellationToken);
            if (masterId != 0) booksCompanyMasterId = masterId;
        }

        source ??= string.Empty;
        string cacheKey = $"getCompanyMapResource_{booksCompanyMasterId}_{source}_{includeExtra}_{domain}";

        return await _cache.GetOrCreateAsync<IList<CustomerCompanyMap>?>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheTimeSeconds);

            string domainFilter = !string.IsNullOrEmpty(domain) && _useDomains
                ? $"filter[companyInstance.domain]={domain}&"
                : string.Empty;

            string uri = "customercompanymap?"
                + (includeGreenBookCares ? "filter[companyInstance.greenBookCares]=true&" : string.Empty)
                + domainFilter
                + $"filter[customerCompanyId]={booksCompanyMasterId}&"
                + "include=companyInstance&include=companyInstance.attributes";

            if (!string.IsNullOrEmpty(source)) uri += $"&filter[source]={source}";
            if (!string.IsNullOrEmpty(includeExtra)) uri += $"&include={includeExtra}";

            LogDebug("GetCompanyMapAsync", uri);
            var response = await GetAsync(uri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new BlueBookException(CommonMessageConstants.CompanyErrorMessage);
                return null;
            }

            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<List<CustomerCompanyMap>>(content, JsonApiSettings);
        });
    }

    #endregion

    #region GetProductCompanyMappingAsync

    /// <inheritdoc/>
    public Task<IList<CustomerCompanyMap>?> GetProductCompanyMappingAsync(
        Guid companyRealPageId, string source, CancellationToken cancellationToken = default)
        => TranslateFromUPFMToProductAsync(companyRealPageId.ToString(), source, cancellationToken);

    #endregion

    #region GetCompanyInstanceBySourceAndInstanceIdAsync

    /// <inheritdoc/>
    public async Task<CustomerCompanyMap?> GetCompanyInstanceBySourceAndInstanceIdAsync(
        string instanceId, string productSource, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"GetCompanyInstanceBySourceAndInstanceId_{instanceId}_{productSource}";

        return await _cache.GetOrCreateAsync<CustomerCompanyMap?>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            string uri = $"companyinstance/{instanceId}/{productSource}";
            LogDebug("GetCompanyInstanceBySourceAndInstanceIdAsync", uri);
            var response = await GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<CustomerCompanyMap>(content, JsonApiSettings);
        });
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManageBlueBookAsync — Property translation
    // ════════════════════════════════════════════════════════════════════════

    #region GetTranslatePropertiesFromUPFMToProductv3Async / FromProductToUPFM

    /// <inheritdoc/>
    public async Task<TranslatePropertyInstance> GetTranslatePropertiesFromUPFMToProductv3Async(
        UPFMProperty upfmProperties, string productSource, CancellationToken cancellationToken = default)
    {
        string uri = $"translate/v3/propertyinstance/{ProductEnum.UnifiedPlatform.ToEnumDescription()}/{productSource}";
        string payload = JsonConvert.SerializeObject(upfmProperties);
        using var req = BuildJsonRequest(HttpMethod.Post, uri, payload);

        LogDebug("GetTranslatePropertiesFromUPFMToProductv3Async", uri);
        var response = await _httpClient.SendAsync(req, cancellationToken);
        if (response?.IsSuccessStatusCode != true) return new TranslatePropertyInstance();

        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<TranslatePropertyInstance>(content) ?? new TranslatePropertyInstance();
    }

    /// <inheritdoc/>
    public async Task<TranslatePropertyInstance> GetTranslatePropertiesFromProductToUPFMAsync(
        UPFMProperty properties, string productSource, CancellationToken cancellationToken = default)
    {
        string uri = $"translate/v3/propertyinstance/{productSource}/{ProductEnum.UnifiedPlatform.ToEnumDescription()}";
        string payload = JsonConvert.SerializeObject(properties);
        using var req = BuildJsonRequest(HttpMethod.Post, uri, payload);

        LogDebug("GetTranslatePropertiesFromProductToUPFMAsync", uri);
        var response = await _httpClient.SendAsync(req, cancellationToken);
        if (response?.IsSuccessStatusCode != true) return new TranslatePropertyInstance();

        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<TranslatePropertyInstance>(content) ?? new TranslatePropertyInstance();
    }

    #endregion

    #region TranslateProductPrimaryPropertiesDataAsync

    /// <inheritdoc/>
    /// <remarks>
    /// Replaces: <c>TranslateProductPrimaryPropertiesData</c> sync method —
    /// all blocking calls (<c>GetUPFMPropertyInstances</c>, <c>ListUPFMPropertyInstanceIdByInstanceIds</c>,
    /// <c>GetAllProducts</c>, <c>GetTranslatePropertiesFromUPFMToProductv3</c>) are now properly awaited.
    /// The generic <c>ApplyTranslation</c> helper replaces the per-type copy-paste blocks.
    /// </remarks>
    public async Task<ListResponse> TranslateProductPrimaryPropertiesDataAsync(
        UPFMProperty upfmProperty, int productId, ListResponse productResult,
        CancellationToken cancellationToken = default)
    {
        if (productId == 3) return productResult; // product 3 needs no translation
        await EnsureSettingsLoadedAsync(cancellationToken);

        var userClaims = _userClaimAccessor.Current;
        bool isPrimary = upfmProperty?.id != null;
        bool dirtyData = false;

        // ── Build UPFM primary property ID list ──────────────────────────────
        var upfmAll = new UPFMProperty();
        var booksGuids = await GetUPFMPropertyInstancesAsync(userClaims.OrganizationRealPageGuid.ToString(), cancellationToken);

        if (booksGuids?.Count > 0)
        {
            var instances = await _propertyRepo.ListUPFMPropertyInstanceIdByInstanceIdsAsync(booksGuids, cancellationToken);
            upfmAll.id = instances.Select(p => p.InstanceId.ToString()).ToList();
        }

        var primaryIds = new UPFMProperty { id = upfmAll.id?.ConvertAll(d => d.ToLower()) ?? [] };

        // ── UnifiedPlatform — direct ID comparison (no HTTP round-trip) ──────
        if (productId == (int)ProductEnum.UnifiedPlatform)
        {
            if (productResult.Records?.Count > 0 && productResult.Records[0] is ProductProperty)
            {
                var list = productResult.Records.Cast<ProductProperty>().ToList();
                list.ForEach(p => p.IsAssigned = primaryIds.id.Contains(p.ID));
            }
            return productResult;
        }

        // ── Resolve UDM product source code ───────────────────────────────────
        var allProducts = await _productRepo.GetAllProductsAsync();
        string udmSource = ProductEnumHelper.GetUDMSourceCodeByProductId(productId, allProducts);
        string productCode = !string.IsNullOrEmpty(udmSource)
            ? udmSource
            : ProductEnumHelper.GetProductCodeByProductId(productId, allProducts);

        var translated = await GetTranslatePropertiesFromUPFMToProductv3Async(primaryIds, productCode, cancellationToken);

        if (productResult.Records is null || productResult.Records.Count == 0)
            return productResult;

        var settingsByType = await _internalSettingRepo.GetProductSettingByTypeAsync("ProductIntegrationType", cancellationToken);
        bool isUPFMType = settingsByType?.FirstOrDefault(p => p.ProductId == productId)?.Value == "UPFM";

        // ── Per-property-type translation (generic helper) ────────────────────
        switch (productResult.Records[0])
        {
            case ProductProperty when isUPFMType:
                ApplyTranslation(
                    productResult.Records.Cast<ProductProperty>(),
                    translated, upfmProperty, isPrimary, ref dirtyData,
                    r => r.InstanceId, (r, v) => r.InstanceId = v,
                    r => r.IsAssigned ?? false, (r, v) => r.IsAssigned = v,
                    matchByUpfmId: true);
                break;

            case ProductProperty:
                ApplyTranslation(
                    productResult.Records.Cast<ProductProperty>(),
                    translated, upfmProperty, isPrimary, ref dirtyData,
                    r => r.ID, (r, v) => r.InstanceId = v,
                    r => r.IsAssigned ?? false, (r, v) => r.IsAssigned = v,
                    matchByUpfmId: false);
                break;

            case ACProperty:
                ApplyTranslation(
                    productResult.Records.Cast<ACProperty>(),
                    translated, upfmProperty, isPrimary, ref dirtyData,
                    r => r.BookID, (r, v) => r.InstanceId = v.ToLower(),
                    r => r.IsAssigned, (r, v) => r.IsAssigned = v,
                    matchByUpfmId: false);
                break;

            case AssetGroup:
                ApplyTranslation(
                    productResult.Records.Cast<AssetGroup>(),
                    translated, upfmProperty, isPrimary, ref dirtyData,
                    r => r.AssetID, (r, v) => r.InstanceId = v.ToLower(),
                    r => r.IsAssigned, (r, v) => r.IsAssigned = v,
                    matchByUpfmId: false);
                break;
            
            case OnSiteProperty:
                ApplyTranslation(
                    productResult.Records.Cast<OnSiteProperty>(),
                    translated, upfmProperty, isPrimary, ref dirtyData,
                    r => r.GetPropertyId.ToString(), (r, v) => r.InstanceId = v.ToLower(),
                    r => r.IsAssigned, (r, v) => r.IsAssigned = v,
                    matchByUpfmId: false);
                break;

            case RumPropertyGroup:
                ApplyTranslation(
                    productResult.Records.Cast<RumPropertyGroup>(),
                    translated, upfmProperty, isPrimary, ref dirtyData,
                    r => r.Id.ToString(), (r, v) => r.InstanceId = v.ToLower(),
                    r => r.IsAssigned, (r, v) => r.IsAssigned = v,
                    matchByUpfmId: false);
                break;

            case Portfolio:
                ApplyTranslation(
                    productResult.Records.Cast<Portfolio>(),
                    translated, upfmProperty, isPrimary, ref dirtyData,
                    r => r.ID, (r, v) => r.InstanceId = v.ToLower(),
                    r => r.IsAssigned, (r, v) => r.IsAssigned = v,
                    matchByUpfmId: false);
                break;

            case UPFMPropertyInstance:
                ApplyUPFMInstanceTranslation(
                    productResult.Records.Cast<UPFMPropertyInstance>().ToList(),
                    translated, upfmProperty, isPrimary, ref dirtyData);
                break;
        }

        if (productResult.Additional is Dictionary<string, bool> extra)
            extra["dirtyProductPropertyData"] = dirtyData;

        return productResult;
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManageBlueBookAsync — Property instances
    // ════════════════════════════════════════════════════════════════════════

    #region GetPropertyInstanceAsync / GetCompanyPropertyInstanceAsync

    /// <inheritdoc/>
    public async Task<IList<PropertyInstance>> GetPropertyInstanceAsync(
        long companyInstanceId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"getPropertyInstance_{companyInstanceId}";

        return await _cache.GetOrCreateAsync<IList<PropertyInstance>>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheTimeSeconds);
            string uri = $"dashboard/gb/getCompanyPropertyInstances?funcargs={companyInstanceId}";
            LogDebug("GetPropertyInstanceAsync", uri);
            var response = await GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound) return [];
                response.EnsureSuccessStatusCode();
                return null;
            }
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<List<PropertyInstance>>(content, JsonApiSettings) ?? [];
        }) ?? [];
    }

    /// <inheritdoc/>
    public async Task<CompanyPropertyRootObject> GetCompanyPropertyInstanceAsync(
        long companyInstanceId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"getCompanyPropertyInstance_{companyInstanceId}";

        return await _cache.GetOrCreateAsync<CompanyPropertyRootObject>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheTimeSeconds);
            string uri = $"dashboard/gb/getCompanyPropertyInstances?funcargs={companyInstanceId}";
            LogDebug("GetCompanyPropertyInstanceAsync", uri);
            var response = await GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound) return new CompanyPropertyRootObject();
                response.EnsureSuccessStatusCode();
                return null;
            }
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonConvert.DeserializeObject<CompanyPropertyRootObject>(content);
            if (result?.data?.attributes?.getCompanyPropertyInstances is not null)
                result.data.attributes.getCompanyPropertyInstances =
                    result.data.attributes.getCompanyPropertyInstances.OrderBy(r => r.propertyName).ToList();
            return result ?? new CompanyPropertyRootObject();
        }) ?? new CompanyPropertyRootObject();
    }

    #endregion

    #region GetUPFMPropertyInstancesAsync / GetPropertiesPerProductCenterAsync / GetProductPropertyInstancesAsync

    /// <inheritdoc/>
    public async Task<List<Guid>> GetUPFMPropertyInstancesAsync(
        string companyRealPageId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"getUPFMPropertyInstances_{companyRealPageId}";

        return await _cache.GetOrCreateAsync<List<Guid>>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
            string uri = "companypropertyinstancemap?include=propertyInstance&filter[source]=UPFM"
                + $"&filter[companyinstance.companyInstanceSourceId]={companyRealPageId}"
                + "&fields[propertyInstance]=propertyInstanceSourceId,propertyName,domain,isActive"
                + "&filter[propertyInstance.isActive]=true&page[size]=9999";

            LogDebug("GetUPFMPropertyInstancesAsync", uri);
            var response = await GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return response.StatusCode == HttpStatusCode.NotFound ? [] : null;

            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            var root = JsonConvert.DeserializeObject<UPFMPropertyInstanceRootObject>(content);
            return root?.data?.SelectMany(p => p.attributes.propertyInstance)
                               .Select(d => new Guid(d.PropertyInstanceSourceId))
                               .ToList() ?? [];
        }) ?? [];
    }

    /// <inheritdoc/>
    public async Task<List<Guid>> GetPropertiesPerProductCenterAsync(
        string companyRealPageId, int productId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"getPropertiesPerProductCenter_{companyRealPageId}_{productId}";

        return await _cache.GetOrCreateAsync<List<Guid>>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
            string uri = $"propertiesperproductcenter/UPFM/{companyRealPageId}/{productId}";
            LogDebug("GetPropertiesPerProductCenterAsync", uri);
            var response = await GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return response.StatusCode == HttpStatusCode.NotFound ? [] : null;

            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            var root = JsonConvert.DeserializeObject<UPFMProductPropertyInstanceMap>(content);
            return root?.data?.attributes?.Select(p => new Guid(p.propertyInstanceSourceId)).ToList() ?? [];
        }) ?? [];
    }

    /// <inheritdoc/>
    public async Task<List<Guid>> GetProductPropertyInstancesAsync(
        int companyInstanceSourceId, string source, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"getProductPropertyInstances_{companyInstanceSourceId}_{source}";

        return await _cache.GetOrCreateAsync<List<Guid>>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
            string uri = "companypropertyinstancemap?include=propertyInstance"
                + $"&filter[source]={source}"
                + $"&filter[companyinstance.companyInstanceSourceId]={companyInstanceSourceId}"
                + "&fields[propertyInstance]=propertyInstanceSourceId,propertyName,domain,isActive"
                + "&filter[propertyInstance.isActive]=true&page[size]=9999";

            LogDebug("GetProductPropertyInstancesAsync", uri);
            var response = await GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return response.StatusCode == HttpStatusCode.NotFound ? [] : null;

            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            var root = JsonConvert.DeserializeObject<UPFMPropertyInstanceRootObject>(content);
            return root?.data?.SelectMany(p => p.attributes.propertyInstance)
                               .Select(d => new Guid(d.PropertyInstanceSourceId))
                               .ToList() ?? [];
        }) ?? [];
    }

    #endregion

    #region GetPropertyInstanceForCompanyAsync / ByOperatorId / ByCustomerPropertyId / AllProducts / BySource

    /// <inheritdoc/>
    public async Task<List<BooksPropertyInstance>> GetPropertyInstanceForCompanyAsync(
        Guid companyRealPageId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"getPropertyInstanceForCompany_{companyRealPageId}";

        return await _cache.GetOrCreateAsync<List<BooksPropertyInstance>>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(600);
            string uri = $"propertyinstance?filter[source]=UPFM"
                + $"&filter[companyPropertyInstanceMap.companyInstance.companyInstanceSourceId]={companyRealPageId.ToString().ToLower()}"
                + "&page[size]=9999&include=customerPropertyMap.customerProperty.customerPropertyOrderType"
                + "&fields[propertyinstance]=propertyInstanceId,propertyInstanceSourceId,propertyName,source,domain,address"
                + "&fields[customerPropertyMap]=customerPropertyId,propertyInstanceId"
                + "&fields[customerPropertyMap.customerProperty]=customerPropertyId,propertyName,isActive";

            LogDebug("GetPropertyInstanceForCompanyAsync", uri);
            var response = await GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<List<BooksPropertyInstance>>(content, JsonApiSettings);
        }) ?? [];
    }

    /// <inheritdoc/>
    public async Task<List<BooksPropertyInstance>> GetPropertyInstanceForCompanyByOperatorIdAsync(
        Guid companyRealPageId, Guid operatorRealPageId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"getPropertyInstanceForCompanyByOperatorId_{companyRealPageId}_{operatorRealPageId}";

        return await _cache.GetOrCreateAsync<List<BooksPropertyInstance>>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(600);
            string uri = $"propertyinstance?scope[operatedBy]={companyRealPageId.ToString().ToLower()},UPFM,{operatorRealPageId.ToString().ToLower()}"
                + "&page[size]=9999&include=customerPropertyMap.customerProperty"
                + "&fields[propertyinstance]=propertyInstanceId,propertyInstanceSourceId,propertyName,source,domain,address"
                + "&fields[customerPropertyMap]=customerPropertyId,propertyInstanceId"
                + "&fields[customerPropertyMap.customerProperty]=customerPropertyId,propertyName";

            LogDebug("GetPropertyInstanceForCompanyByOperatorIdAsync", uri);
            var response = await GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<List<BooksPropertyInstance>>(content, JsonApiSettings);
        }) ?? [];
    }

    /// <inheritdoc/>
    public async Task<List<BooksPropertyInstance>?> GetUPFMPropertyInstancesByCustomerPropertyIdAsync(
        string customerPropertyId, CancellationToken cancellationToken = default)
    {
        string uri = $"propertyinstance?filter[source]=UPFM"
            + $"&filter[customerPropertyMap.customerPropertyId]={customerPropertyId.ToLower()}"
            + "&page[size]=9999&fields[propertyinstance]=propertyInstanceId,propertyName,domain,propertyInstanceSourceId,isActive"
            + "&include=customerPropertyMap.customerProperty"
            + "&fields[customerPropertyMap]=customerPropertyId,propertyInstanceId"
            + "&fields[customerPropertyMap.customerProperty]=customerPropertyId,propertyName,address";

        LogDebug("GetUPFMPropertyInstancesByCustomerPropertyIdAsync", uri);
        var response = await GetAsync(uri, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<List<BooksPropertyInstance>>(content, JsonApiSettings) ?? [];
    }

    /// <inheritdoc/>
    public async Task<List<BooksPropertyInstance>?> GetAllProductsPropertyInstanceFromBooksAsync(
        string customerPropertyId, CancellationToken cancellationToken = default)
    {
        string uri = "propertyinstance?"
            + $"filter[customerPropertyMap.customerPropertyId]={customerPropertyId}"
            + "&page[size]=9999&fields[propertyinstance]=propertyInstanceId,propertyName,domain,propertyInstanceSourceId,isActive,source"
            + "&include=customerPropertyMap.customerProperty"
            + "&fields[customerPropertyMap]=customerPropertyId,propertyInstanceId"
            + "&fields[customerPropertyMap.customerProperty]=customerPropertyId,propertyName,address";

        LogDebug("GetAllProductsPropertyInstanceFromBooksAsync", uri);
        var response = await GetAsync(uri, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<List<BooksPropertyInstance>>(content, JsonApiSettings) ?? [];
    }

    /// <inheritdoc/>
    public async Task<BooksPropertyInstance?> GetPropertyDetailsByPropertyInstanceIdAndSourceAsync(
        string propertyInstanceSourceId, string source, CancellationToken cancellationToken = default)
    {
        string uri = $"propertyinstance/{propertyInstanceSourceId}/{source}?include=customerPropertyMap.customerProperty";
        LogDebug("GetPropertyDetailsByPropertyInstanceIdAndSourceAsync", uri);
        var response = await GetAsync(uri, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<BooksPropertyInstance>(content, JsonApiSettings);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManageBlueBookAsync — Company read
    // ════════════════════════════════════════════════════════════════════════

    #region GetCompanyListByCompIdsAsync / GetUPFMCompanyDetailsByInstanceIdsAsync

    /// <inheritdoc/>
    /// <remarks>
    /// Replaces: blocking <c>Parallel.ForEach</c> + <c>.Result</c> inside —
    /// now uses <see cref="Parallel.ForEachAsync"/> with a non-blocking HTTP call per chunk.
    /// </remarks>
    public async Task<IList<Company>> GetCompanyListByCompIdsAsync(
        List<UnifiedLoginCompany> booksCompanyMasterList, CancellationToken cancellationToken = default)
    {
        string allIds = GetCompanyIds(booksCompanyMasterList);
        string cacheKey = $"getCompanysByCompIds_{allIds.GetHashCode()}";

        return await _cache.GetOrCreateAsync<IList<Company>>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(LongCacheSeconds);
            int splitSize = Math.Max(10, (int)(booksCompanyMasterList.Count * .08));
            var bag = new ConcurrentBag<Company>();

            await Parallel.ForEachAsync(
                SplitList(booksCompanyMasterList, splitSize),
                new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = cancellationToken },
                async (chunk, ct) =>
                {
                    foreach (var c in await FetchCompanyDetailsAsync(chunk, ct))
                        bag.Add(c);
                });

            return bag.ToList();
        }) ?? [];
    }

    /// <inheritdoc/>
    public async Task<IList<CustomerCompanyInstance>> GetUPFMCompanyDetailsByInstanceIdsAsync(
        List<string> companyInstanceIds, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"GetUPFMCompanyDetailsByInstanceIds_{string.Join(",", companyInstanceIds).GetHashCode()}";

        return await _cache.GetOrCreateAsync<IList<CustomerCompanyInstance>>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(120);
            var bag = new ConcurrentBag<CustomerCompanyInstance>();

            await Parallel.ForEachAsync(
                SplitList(companyInstanceIds, 50),
                new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = cancellationToken },
                async (chunk, ct) =>
                {
                    foreach (var c in await FetchUPFMCompanyDetailsAsync(chunk, ct))
                        bag.Add(c);
                });

            return bag.ToList();
        }) ?? [];
    }

    #endregion

    #region GetBooksCompanyDetailsByCompanyMasterIdAsync / GetCompanyInstancesByCustomerCompanyIdAsync / GetListOfDomainsByCompanyAsync

    /// <inheritdoc/>
    public async Task<Company> GetBooksCompanyDetailsByCompanyMasterIdAsync(
        long companyMasterId, CancellationToken cancellationToken = default)
    {
        string uri = $"customercompany?filter[customerCompanyId]=in:{companyMasterId}&include=customerCompanyLocation";
        LogDebug("GetBooksCompanyDetailsByCompanyMasterIdAsync", uri);
        var response = await GetAsync(uri, cancellationToken);
        if (!response.IsSuccessStatusCode) return new Company();
        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<List<Company>>(content, JsonApiSettings)?.FirstOrDefault() ?? new Company();
    }

    /// <inheritdoc/>
    public async Task<List<CustomerCompanyInstance>> GetCompanyInstancesByCustomerCompanyIdAsync(
        long customerCompanyId, CancellationToken cancellationToken = default)
    {
        string uri = $"companyinstance?filter[source]=UPFM"
            + $"&filter[customerCompanyMap.customerCompanyId]={customerCompanyId}"
            + "&fields[companyinstance]=companyInstanceId,source,companyInstanceSourceId,companyName,companyType,isActive,domain";

        LogDebug("GetCompanyInstancesByCustomerCompanyIdAsync", uri);
        var response = await GetAsync(uri, cancellationToken);
        if (!response.IsSuccessStatusCode) return [];
        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<List<CustomerCompanyInstance>>(content, JsonApiSettings) ?? [];
    }

    /// <inheritdoc/>
    public async Task<List<CustomerCompanyDomain>> GetListOfDomainsByCompanyAsync(
        long companyMasterId, CancellationToken cancellationToken = default)
    {
        string uri = $"domain/customercompany/{companyMasterId}";
        LogDebug("GetListOfDomainsByCompanyAsync", uri);
        var response = await GetAsync(uri, cancellationToken);
        if (!response.IsSuccessStatusCode) return [];
        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<List<CustomerCompanyDomain>>(content, JsonApiSettings) ?? [];
    }

    #endregion

    #region GetCompanyCustomerInfoAsync / GetVCompanyPropertyMapAsync / GetCompanyInstanceByUPFMCompanyIdAsync / GetCustomerCompanyMapByCustomerCompanyIdAsync

    /// <inheritdoc/>
    public async Task<CustomerCompany?> GetCompanyCustomerInfoAsync(
        Guid companyRealPageId, string domain, long booksCompanyMasterId,
        CancellationToken cancellationToken = default)
    {
        await EnsureSettingsLoadedAsync(cancellationToken);

        if (_useUPFMId && companyRealPageId != Guid.Empty)
        {
            long masterId = await GetCompanyMasterIdForRPDMIDAsync(companyRealPageId.ToString(), domain, cancellationToken);
            if (masterId != 0) booksCompanyMasterId = masterId;
        }

        string cacheKey = $"getCompanyCustomerInfo_{booksCompanyMasterId}";

        return await _cache.GetOrCreateAsync<CustomerCompany?>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheTimeSeconds);
            string uri = $"customercompany/{booksCompanyMasterId}";
            LogDebug("GetCompanyCustomerInfoAsync", uri);
            var response = await GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<CustomerCompany>(content, JsonApiSettings);
        });
    }

    /// <inheritdoc/>
    public async Task<IList<CustomerCompanyPropertyMap>> GetVCompanyPropertyMapAsync(
        long booksCompanyMasterId, string filter, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"getVCompanyPropertyMap_{booksCompanyMasterId}";

        return await _cache.GetOrCreateAsync<IList<CustomerCompanyPropertyMap>>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheTimeSeconds);
            string uri = $"customercompanyproperty?filter[customerCompanyId]={booksCompanyMasterId}"
                + "&filter[migrationStatus]=in:%27staged%27,%27migrated%27"
                + "&sort=PropertyName&page[number]=1&page[size]=9999";
            LogDebug("GetVCompanyPropertyMapAsync", uri);
            var response = await GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<List<CustomerCompanyPropertyMap>>(content, JsonApiSettings);
        }) ?? [];
    }

    /// <inheritdoc/>
    public async Task<BooksCompanyInstance> GetCompanyInstanceByUPFMCompanyIdAsync(
        string upfmCompanyId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"getCompanyInstanceByUPFMCompanyId_{upfmCompanyId}";

        return await _cache.GetOrCreateAsync<BooksCompanyInstance>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            string uri = $"companyinstance/{upfmCompanyId.ToLower()}/UPFM?include=customerCompanyMap";
            LogDebug("GetCompanyInstanceByUPFMCompanyIdAsync", uri);
            var response = await GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<BooksCompanyInstance>(content, JsonApiSettings);
        }) ?? new BooksCompanyInstance();
    }

    /// <inheritdoc/>
    public async Task<List<CustomerCompanyMap>> GetCustomerCompanyMapByCustomerCompanyIdAsync(
        int customerCompanyId, string companyDomain, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"getCustomerCompanyMapByCustomerCompanyId_{customerCompanyId}_{companyDomain}";

        return await _cache.GetOrCreateAsync<List<CustomerCompanyMap>>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            string uri = $"customercompanymap?filter[customerCompanyId]={customerCompanyId}"
                + $"&filter[companyInstance.domain]={companyDomain}"
                + "&include=companyInstance&fields[companyInstance]=greenBookCares,companyInstanceSourceId,domain";
            LogDebug("GetCustomerCompanyMapByCustomerCompanyIdAsync", uri);
            var response = await GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<List<CustomerCompanyMap>>(content, JsonApiSettings);
        }) ?? [];
    }

    #endregion

    #region GetCustomerPropertyAsync / GetCustomerPropertyDetailsAsync

    /// <inheritdoc/>
    public async Task<IList<ProductProperty>> GetCustomerPropertyAsync(
        long booksCompanyMasterId = 0, string? include = null, string? filter = null,
        bool getCached = true, CancellationToken cancellationToken = default)
    {
        var userClaims = _userClaimAccessor.Current;
        if (booksCompanyMasterId == 0) booksCompanyMasterId = userClaims.CustomerMasterId;
        if (booksCompanyMasterId == 0) throw new InvalidOperationException("Invalid parameter booksCompanyMasterId.");

        bool hasInclude = !string.IsNullOrWhiteSpace(include);
        string includeFields = hasInclude ? $"fields[customerproperty]={include!.Replace(" ", "")}& " : string.Empty;
        filter ??= "&filter[isActive]=true&page[size]=9999";

        string cacheKey = $"getCustomerProperty_{booksCompanyMasterId}"
            + (hasInclude ? "_" + include!.Replace(",", "") : string.Empty);

        if (!getCached)
            return await FetchCustomerPropertyAsync(booksCompanyMasterId, filter, includeFields, cancellationToken) ?? [];

        return await _cache.GetOrCreateAsync<IList<ProductProperty>>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheTimeSeconds);
            return await FetchCustomerPropertyAsync(booksCompanyMasterId, filter, includeFields, cancellationToken);
        }) ?? [];
    }

    /// <inheritdoc/>
    public async Task<CustomerProperty?> GetCustomerPropertyDetailsAsync(
        string propertyInstanceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(propertyInstanceId))
            throw new InvalidOperationException("Invalid parameter propertyInstanceId.");

        string cacheKey = $"getCustomerPropertyDetails_{propertyInstanceId}";

        return await _cache.GetOrCreateAsync<CustomerProperty?>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheTimeSeconds);
            string uri = $"customerproperty/{propertyInstanceId}";
            LogDebug("GetCustomerPropertyDetailsAsync", uri);
            var response = await GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<CustomerProperty>(content, JsonApiSettings);
        });
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManageBlueBookAsync — Company write
    // ════════════════════════════════════════════════════════════════════════

    #region AddUPFMCompanyFromProvisioningEventAsync / AddUPFMCompanyFromCompanySetupAsync / DeleteBooksGreenBookCompanyInstanceAsync / UpdateBooksGreenBookCompanyInstanceAsync

    /// <inheritdoc/>
    public async Task<bool> AddUPFMCompanyFromProvisioningEventAsync(
        CompanyInstance companyInstance, CancellationToken cancellationToken = default)
    {
        string payload = JsonConvert.SerializeObject(companyInstance, JsonApiSettings)
            .Replace("companyinstanceadd", "companyinstance");
        using var req = BuildJsonRequest(HttpMethod.Post, "companyinstance", payload);
        var response = await _httpClient.SendAsync(req, cancellationToken);
        return response?.IsSuccessStatusCode == true;
    }

    /// <inheritdoc/>
    public async Task<bool> AddUPFMCompanyFromCompanySetupAsync(
        CompanyInstanceAdd companyInstance, CancellationToken cancellationToken = default)
    {
        string uri = $"companyinstance/{companyInstance.CompanyInstanceSourceId}/UPFM";
        string payload = JsonConvert.SerializeObject(companyInstance, JsonApiSettings)
            .Replace("companyinstanceadd", "companyinstance");
        using var req = BuildJsonRequest(HttpMethod.Put, uri, payload);
        var response = await _httpClient.SendAsync(req, cancellationToken);
        return response?.IsSuccessStatusCode == true;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteBooksGreenBookCompanyInstanceAsync(
        CompanyInstance companyInstance, CancellationToken cancellationToken = default)
    {
        string uri = $"companyinstance/{companyInstance.CompanyInstanceSourceId}/UPFM"
            + $"?modifiedBy={Uri.EscapeDataString(companyInstance.ModifiedBy)}";
        var response = await _httpClient.DeleteAsync(uri, cancellationToken);
        return response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound;
    }

    /// <inheritdoc/>
    public async Task<string> UpdateBooksGreenBookCompanyInstanceAsync(
        CompanyInstance companyInstance, CompanyLocation oldCompanyLocation,
        CancellationToken cancellationToken = default)
    {
        string uri = $"companyinstance/{companyInstance.CompanyInstanceSourceId}/{ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)}";
        string payload = JsonConvert.SerializeObject(companyInstance, JsonApiSettings)
            .Replace("companyinstanceadd", "companyinstance");
        using var req = BuildJsonRequest(new HttpMethod("PATCH"), uri, payload);
        var response = await _httpClient.SendAsync(req, cancellationToken);

        if (response?.IsSuccessStatusCode == false)
            return response.StatusCode == HttpStatusCode.NotFound
                ? "instance not found"
                : $"an unknown error occurred. {response.StatusCode}";

        // Audit address change
        // Audit address change
        var newLocation = companyInstance.CompanyInstanceLocation?.FirstOrDefault();
        if (newLocation is not null)
        {
            static string oFmt(CompanyLocation? l) => l is null ? string.Empty
                : $"{l.Address}, {l.City}, {l.County}, {l.State}, {l.Country}, {l.PostalCode}";

            static string nFmt(CompanyInstanceAddress? l) => l is null ? string.Empty
                : $"{l.Address}, {l.City}, {l.County}, {l.State}, {l.Country}, {l.PostalCode}";

            string oldAddr = oFmt(oldCompanyLocation), newAddr = nFmt(newLocation);
            if (!string.Equals(oldAddr, newAddr, StringComparison.OrdinalIgnoreCase))
            {
                var uc = _userClaimAccessor.Current;
                LogAuditActivity(
                    LogActivityTypeConstants.COMPANY_UPDATED,
                    LogActivityCategoryType.CompanySetup,
                    $"{uc.FirstName} {uc.LastName} updated the company address for {companyInstance.CompanyName}",
                    [new AdditionalParameters { Key = "Address", Value = $"{{\"old\":\"{oldAddr}\",\"new\":\"{newAddr}\"}}" }]);
            }
        }
        return string.Empty;
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManageBlueBookAsync — Property provisioning events
    // ════════════════════════════════════════════════════════════════════════

    #region Property provisioning

    /// <inheritdoc/>
    public async Task<bool> AddBooksGreenBookPropertyInstanceFromProvisioningAsync(
        PropertyInstance propertyInstance, CancellationToken cancellationToken = default)
    {
        string uri = $"propertyinstance/{propertyInstance.PropertyInstanceSourceId}/{ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)}";
        string payload = JsonConvert.SerializeObject(propertyInstance, JsonApiSettings)
            .Replace("\"propertyInstanceId\":0,", "");
        using var req = BuildJsonRequest(HttpMethod.Put, uri, payload);
        var response = await _httpClient.SendAsync(req, cancellationToken);
        if (response?.IsSuccessStatusCode == true) return true;
        _logger.LogError("AddBooksGreenBookPropertyInstanceFromProvisioningAsync failed for {Id}",
            propertyInstance.PropertyInstanceSourceId);
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> DeletePropertyFromBooksAsync(
        Guid propertyInstance, CancellationToken cancellationToken = default)
    {
        string uri = $"propertyinstance/{propertyInstance.ToString().ToLower()}/UPFM"
            + $"?modifiedBy={Uri.EscapeDataString(ProductEnum.UnifiedPlatform.ToString())}";
        var response = await _httpClient.DeleteAsync(uri, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc/>
    public async Task<bool> AcknowledgePropertyUpdateAsync(
        PropertyInstanceAck propertyInstanceAck, CancellationToken cancellationToken = default)
    {
        string uri = $"propertyinstance/{propertyInstanceAck.PropertyInstanceSourceId}/UPFM";
        string payload = JsonConvert.SerializeObject(propertyInstanceAck, JsonApiSettings)
            .Replace("propertyinstanceack", "propertyinstance");
        using var req = BuildJsonRequest(HttpMethod.Put, uri, payload);
        var response = await _httpClient.SendAsync(req, cancellationToken);
        return response?.IsSuccessStatusCode == true;
    }

    /// <inheritdoc/>
    public async Task<bool> AcknowledgeBulkPropertyListUpdateAsync(
        BulkPropertyInstanceStatusAck propertyInstanceAck, CancellationToken cancellationToken = default)
    {
        string payload = JsonConvert.SerializeObject(propertyInstanceAck, JsonApiSettings)
            .Replace("propertyinstanceack", "propertyinstance");
        using var req = BuildJsonRequest(HttpMethod.Post, "propertyinstance/bulk-status/UPFM", payload);
        var response = await _httpClient.SendAsync(req, cancellationToken);
        return response?.IsSuccessStatusCode == true;
    }

    #endregion

    #region Product center events

    /// <inheritdoc/>
    public async Task<bool> AcknowledgeProvisioningEventAsync(
        ProductCenterEnablement productCenterEnablement, CancellationToken cancellationToken = default)
    {
        string payload = JsonConvert.SerializeObject(productCenterEnablement, JsonApiSettings)
            .Replace("\"details\"", "\"productCenterEnablement\"");
        using var req = BuildJsonRequest(HttpMethod.Post, "productcenterenablement/enable", payload);
        var response = await _httpClient.SendAsync(req, cancellationToken);
        return response?.IsSuccessStatusCode == true;
    }

    /// <inheritdoc/>
    public async Task<bool> ProductCenterEnableAsync(
        SystemProductCenter systemProductCenter, CancellationToken cancellationToken = default)
    {
        string payload = JsonConvert.SerializeObject(systemProductCenter, JsonApiSettings)
            .Replace("\"id\":\"0\",", "");
        using var req = BuildJsonRequest(HttpMethod.Post, "systemproductcenter", payload);
        var response = await _httpClient.SendAsync(req, cancellationToken);
        if (response?.IsSuccessStatusCode == true) return true;
        _logger.LogError("ProductCenterEnableAsync failed for {Id}", systemProductCenter.ProductCenterSourceId);
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> ProductCenterDisableAsync(
        SystemProductCenter systemProductCenter, CancellationToken cancellationToken = default)
    {
        string uri = $"systemproductcenter/{systemProductCenter.Source}/{systemProductCenter.ProductCenterSourceId}"
            + $"/{systemProductCenter.CompanyInstanceSourceId}"
            + $"?modifiedBy={Uri.EscapeDataString(systemProductCenter.CreatedBy)}";
        using var req = new HttpRequestMessage(HttpMethod.Delete, uri);
        var response = await _httpClient.SendAsync(req, cancellationToken);
        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound) return true;
        _logger.LogError("ProductCenterDisableAsync failed for {Id}", systemProductCenter.ProductCenterSourceId);
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> AcknowledgeProvisioningCancelEventAsync(
        ProductCenterCancellation productCenterCancellation, CancellationToken cancellationToken = default)
    {
        string payload = JsonConvert.SerializeObject(productCenterCancellation, JsonApiSettings)
            .Replace("\"details\"", "\"productCenterCancellation\"");
        using var req = BuildJsonRequest(HttpMethod.Post, "productcenteractivation/cancel", payload);
        var response = await _httpClient.SendAsync(req, cancellationToken);
        return response?.IsSuccessStatusCode == true;
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManageBlueBookAsync — Sources / operators
    // ════════════════════════════════════════════════════════════════════════

    #region GetUDMSourceListAsync / GetAllOperatorDetailsForUPFMCompanyAsync / GetOperatorListForUPFMCompanyAsync

    /// <inheritdoc/>
    public async Task<IEnumerable<UDMSource>> GetUDMSourceListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await GetAsync("source", cancellationToken);
            if (!response.IsSuccessStatusCode) return [];
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<IEnumerable<UDMSource>>(content, JsonApiSettings)
                       ?.OrderBy(p => p.Id) ?? Enumerable.Empty<UDMSource>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUDMSourceListAsync failed");
            return [];
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<UDMOperators>?> GetAllOperatorDetailsForUPFMCompanyAsync(
        Guid companyRealPageId, string source, CancellationToken cancellationToken = default)
    {
        string uri = $"operators/{companyRealPageId}/{source}";
        var response = await GetAsync(uri, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<UDMOperatorsRootObject>(content)?.Data?.attributes?.booksOperators ?? [];
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<UPFMOperators>> GetOperatorListForUPFMCompanyAsync(
        Guid companyRealPageId, string source, CancellationToken cancellationToken = default)
    {
        var operatorList = await GetAllOperatorDetailsForUPFMCompanyAsync(companyRealPageId, source, cancellationToken);
        if (operatorList is null) return [];

        var result = new List<UPFMOperators>();
        foreach (var op in operatorList)
        {
            if (op.Translations is null) continue;
            foreach (var t in op.Translations)
                if (!result.Any(p => p.CompanyName.Equals(t.CompanyInstanceSourceId, StringComparison.OrdinalIgnoreCase)))
                    result.Add(new UPFMOperators { CompanyName = t.CompanyName, CompanyGuid = new Guid(t.CompanyInstanceSourceId) });
        }
        return result.OrderBy(o => o.CompanyName);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Private — lazy settings initializer
    // ════════════════════════════════════════════════════════════════════════

    #region EnsureSettingsLoadedAsync / GetBooleanSetting

    /// <summary>
    /// Loads product internal settings exactly once across the lifetime of this instance,
    /// using a <see cref="SemaphoreSlim"/> to prevent concurrent double-loads.
    /// Replaces: <c>new RPObjectCache().GetFromCache(...)</c> in every sync constructor.
    /// </summary>
    private async Task EnsureSettingsLoadedAsync(CancellationToken ct)
    {
        if (_settingsLoaded) return;
        await _settingsLock.WaitAsync(ct);
        try
        {
            if (_settingsLoaded) return;
            _settings = await _internalSettingRepo.GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, ct);
            _useDomains = GetBooleanSetting("BooksUseDomains");
            _useUPFMId = GetBooleanSetting("BooksUseUPFMId");
            _settingsLoaded = true;
        }
        finally { _settingsLock.Release(); }
    }

    private bool GetBooleanSetting(string name)
    {
        var s = _settings?.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return s is not null && Convert.ToBoolean(int.Parse(s.Value));
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Private — HTTP helpers
    // ════════════════════════════════════════════════════════════════════════

    #region GetAsync / BuildJsonRequest

    /// <summary>
    /// Replaces: the original <c>GetAsync</c> which called <c>_httpClient.GetAsync(...).Result</c>
    /// internally despite being declared <c>async</c> — a classic sync-over-async deadlock hazard.
    /// </summary>
    private async Task<HttpResponseMessage> GetAsync(string uri, CancellationToken ct = default)
    {
        HttpResponseMessage response = new();
        int failedCount = 0;

        while (true)
        {
            response = await _httpClient.GetAsync(uri, ct);
            if (response.IsSuccessStatusCode) return response;
            if (response.StatusCode != HttpStatusCode.Unauthorized) return response;
            if (++failedCount >= MaxRetryCount) return response;
        }
    }

    private HttpRequestMessage BuildJsonRequest(HttpMethod method, string uri, string jsonPayload)
        => new(method, uri)
        {
            Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        };

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Private — translate helpers
    // ════════════════════════════════════════════════════════════════════════

    #region TranslateFromUPFMToProductAsync / GetCompanyMasterIdForRPDMIDAsync / FetchCompanyDetailsAsync / FetchUPFMCompanyDetailsAsync / FetchCustomerPropertyAsync

    private async Task<IList<CustomerCompanyMap>?> TranslateFromUPFMToProductAsync(
        string companyRealPageId, string productSource, CancellationToken ct)
    {
        string cacheKey = $"GetTranslateFromUPFMToProductv2_{companyRealPageId}_{productSource}";

        return await _cache.GetOrCreateAsync<IList<CustomerCompanyMap>?>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(180);
            string uri = $"translate/v2/companyinstance/{companyRealPageId}"
                + $"/{ProductEnum.UnifiedPlatform.ToEnumDescription()}/{productSource}"
                + "?filter[greenbookCares]=true";

            LogDebug("TranslateFromUPFMToProductAsync", uri);
            var response = await GetAsync(uri, ct);
            if (!response.IsSuccessStatusCode) return null;

            string content = await response.Content.ReadAsStringAsync(ct);
            var data = JsonConvert.DeserializeObject<TranslateCompanyInstance>(content);
            if (data?.Data?.Attributes?.TranslatedCompanyInstances?.Count < 1) return null;

            return new List<CustomerCompanyMap>
            {
                new()
                {
                    CompanyInstanceSourceId = data.Data.Attributes.TranslatedCompanyInstances[0].CompanyInstanceSourceId,
                    Source          = productSource,
                    CompanyInstance = []
                }
            };
        });
    }

    private async Task<long> GetCompanyMasterIdForRPDMIDAsync(string companyRealPageId, string domain, CancellationToken ct)
    {
        string cacheKey = $"GetCompanyMasterIdForRPDMID_{companyRealPageId}";

        var result = await _cache.GetOrCreateAsync<CustomerCompanyMap?>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(180);
            string uri = $"customercompanymap?filter[companyInstanceSourceId]={companyRealPageId}&include=companyInstance";
            var response = await GetAsync(uri, ct);
            if (!response.IsSuccessStatusCode) return null;
            string content = await response.Content.ReadAsStringAsync(ct);
            var maps = JsonConvert.DeserializeObject<List<CustomerCompanyMap>>(content, JsonApiSettings);
            return maps?.Count > 0 ? maps[0] : null;
        });

        return result?.CustomerCompanyId ?? 0;
    }

    private async Task<List<Company>> FetchCompanyDetailsAsync(
        List<UnifiedLoginCompany> companyList, CancellationToken ct)
    {
        string ids = GetCompanyIds(companyList);
        string uri = $"customercompany?filter[customerCompanyId]=in:{ids}"
            + "&include=customerCompanyLocation"
            + "&fields[customercompany]=customerCompanyId,companyName,phoneNumber"
            + "&fields[customerCompanyLocation]=customerCompanyLocationId,customerCompanyId,address,city,state,country,postalCode,isPrimary"
            + "&page[size]=9999";

        var response = await GetAsync(uri, ct);
        if (!response.IsSuccessStatusCode) return [];
        string content = await response.Content.ReadAsStringAsync(ct);
        return JsonConvert.DeserializeObject<List<Company>>(content, JsonApiSettings) ?? [];
    }

    private async Task<List<CustomerCompanyInstance>> FetchUPFMCompanyDetailsAsync(
        List<string> upfmIds, CancellationToken ct)
    {
        string ids = string.Join(",", upfmIds);
        string uri = $"companyinstance?filter[source]=UPFM&include=companyInstanceLocation&filter[companyInstanceSourceId]=in:{ids}";
        var response = await GetAsync(uri, ct);
        if (!response.IsSuccessStatusCode) return [];
        string content = await response.Content.ReadAsStringAsync(ct);
        return JsonConvert.DeserializeObject<List<CustomerCompanyInstance>>(content, JsonApiSettings) ?? [];
    }

    private async Task<IList<ProductProperty>?> FetchCustomerPropertyAsync(
        long masterId, string filter, string includeFields, CancellationToken ct)
    {
        string uri = $"customerproperty?{includeFields}filter[customerCompanyId]={masterId}{filter}";
        var response = await GetAsync(uri, ct);
        if (!response.IsSuccessStatusCode) return null;
        string content = await response.Content.ReadAsStringAsync(ct);
        var customerList = JsonConvert.DeserializeObject<List<CustomerProperty>>(content, JsonApiSettings);
        return customerList?.Select(p => new ProductProperty
        {
            ID = p.attributes?.customerPropertyId,
            Name = p.attributes?.propertyName,
            Street1 = p.attributes?.address?.address,
            City = p.attributes?.address?.city,
            State = p.attributes?.address?.state,
            Zip = p.attributes?.address?.postalCode
        }).OrderBy(p => p.Name).ToList() ?? [];
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Private — generic translation apply helpers
    // ════════════════════════════════════════════════════════════════════════

    #region ApplyTranslation<T> / ApplyUPFMInstanceTranslation

    /// <summary>
    /// Generic translation kernel.
    /// Replaces: the 8 near-identical per-type blocks that existed in the original sync method.
    /// <para>
    /// <paramref name="matchByUpfmId"/> controls how the <see cref="TranslatePropertyInstance"/>
    /// lookup is performed:
    /// <list type="bullet">
    ///   <item><c>true</c> — match by <c>PropertyInstanceSourceId</c> directly (UPFM type).</item>
    ///   <item><c>false</c> — match via <c>TranslatedPropertyInstances</c> collection (all other types).</item>
    /// </list>
    /// </para>
    /// </summary>
    private static void ApplyTranslation<T>(
        IEnumerable<T> items,
        TranslatePropertyInstance translated,
        UPFMProperty? upfmProperty,
        bool isPrimary,
        ref bool dirty,
        Func<T, string> getId,
        Action<T, string> setInstance,
        Func<T, bool> getAssigned,
        Action<T, bool> setAssigned,
        bool matchByUpfmId)
    {
        foreach (var item in items)
        {
            var matched = matchByUpfmId
                ? translated.Data?.Attributes.FirstOrDefault(p =>
                      p.PropertyInstanceSourceId == getId(item))
                : translated.Data?.Attributes.FirstOrDefault(p =>
                      p.TranslatedPropertyInstances.Any(o => o.PropertyInstanceSourceId == getId(item)));

            if (matched is not null)
            {
                if (upfmProperty is not null && isPrimary)
                {
                    if (upfmProperty.id.Contains("-1") || upfmProperty.id.Contains(matched.PropertyInstanceSourceId))
                        setAssigned(item, true);
                    else if (!upfmProperty.id.Contains(matched.PropertyInstanceSourceId))
                        setAssigned(item, false);
                }
                setInstance(item, matched.PropertyInstanceSourceId);
            }
            else if (isPrimary)
            {
                if (getAssigned(item)) dirty = true;
                setAssigned(item, false);
            }
        }
    }

    private static void ApplyUPFMInstanceTranslation(
        List<UPFMPropertyInstance> items,
        TranslatePropertyInstance translated,
        UPFMProperty? upfmProperty,
        bool isPrimary, ref bool dirty)
    {
        foreach (var item in items)
        {
            var matched = translated.Data?.Attributes
                .FirstOrDefault(p => p.PropertyInstanceSourceId == item.InstanceId.ToString());

            if (matched is not null)
            {
                if (upfmProperty is not null && isPrimary)
                {
                    if (upfmProperty.id.Contains("-1") || upfmProperty.id.Contains(matched.PropertyInstanceSourceId))
                        item.IsAssigned = true;
                    else if (!upfmProperty.id.Contains(matched.PropertyInstanceSourceId))
                        item.IsAssigned = false;
                }
                var srcList = matched.TranslatedPropertyInstances;
                if (srcList?.Count > 0 && !string.IsNullOrEmpty(srcList[0].PropertyInstanceSourceId))
                    item.PropertyInstanceId = Convert.ToInt32(srcList[0].PropertyInstanceSourceId);
            }
            else if (isPrimary)
            {
                if (item.IsAssigned) dirty = true;
                item.IsAssigned = false;
            }
        }
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Private — audit logging
    // ════════════════════════════════════════════════════════════════════════

    #region LogAuditActivity / LogDebug

    private void LogAuditActivity(
        string logActivityType, LogActivityCategoryType categoryType,
        string message, List<AdditionalParameters> additionalParameters)
    {
        var uc = _userClaimAccessor.Current;
        try
        {
            LogActivity.WriteActivity(new ActivityDetails
            {
                LogActivityTypeName = logActivityType,
                LogCategoryName = categoryType.ToString(),
                CorrelationId = uc.CorrelationId.ToString(),
                BooksMasterOrganizationId = uc.OrganizationMasterId,
                OrganizationPartyId = uc.OrganizationPartyId,
                Message = message,
                FromUserLoginName = uc.LoginName,
                FromUserLoginId = uc.UserId,
                FromUserFirstName = uc.FirstName,
                FromUserLastName = uc.LastName,
                FromUserRealpageId = uc.UserRealPageGuid.ToString(),
                AdditionalInformation = additionalParameters
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LogAuditActivity failed for type={T}", logActivityType);
        }
    }

    /// <summary>
    /// Replaces: inline <c>WriteToLog(LogEventLevel.Debug, ...)</c> calls that required checking
    /// the <c>Elk_LogManageBlueBook</c> product setting.
    /// <see cref="ILogger{T}"/> respects the configured minimum log level automatically.
    /// </summary>
    private void LogDebug(string method, string uri)
        => _logger.LogDebug("{Method} → {Uri}", method, _httpClient.BaseAddress + uri);

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Private — static utilities
    // ════════════════════════════════════════════════════════════════════════

    #region GetCompanyIds / SplitList

    /// <summary>
    /// Replaces: manual <c>foreach</c> loop that built a comma-separated string.
    /// </summary>
    private static string GetCompanyIds(IEnumerable<UnifiedLoginCompany> companies)
        => string.Join(",", companies
            .Where(c => c.BooksCustomerMasterId > 0)
            .Select(c => c.BooksCustomerMasterId));

    /// <summary>
    /// Yields sub-lists of at most <paramref name="chunkSize"/> items.
    /// Kept as a utility for <see cref="GetCompanyListByCompIdsAsync"/> and
    /// <see cref="GetUPFMCompanyDetailsByInstanceIdsAsync"/>.
    /// </summary>
    public static IEnumerable<List<T>> SplitList<T>(List<T> source, int chunkSize = 30)
    {
        for (int i = 0; i < source.Count; i += chunkSize)
            yield return source.GetRange(i, Math.Min(chunkSize, source.Count - i));
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IDisposable
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Disposes the <see cref="SemaphoreSlim"/> used for thread-safe lazy settings loading.
    /// The injected <see cref="HttpClient"/> is intentionally NOT disposed here —
    /// it is managed by the DI container / <c>IHttpClientFactory</c>.
    /// </summary>
    public void Dispose() => _settingsLock.Dispose();
}