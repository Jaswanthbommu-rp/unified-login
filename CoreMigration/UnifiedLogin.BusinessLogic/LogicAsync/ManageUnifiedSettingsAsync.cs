using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.Helpers;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first replacement for <see cref="ManageUnifiedSettings"/>.
/// <para>Key improvements over the sync version:</para>
/// <list type="bullet">
///   <item>All HTTP calls use <c>await</c> — no <c>.Result</c> or <c>Task.Run(...).Result</c>.</item>
///   <item><c>_httpClient.DefaultRequestHeaders</c> mutation (not thread-safe) replaced by per-request headers.</item>
///   <item><c>response.Content.ReadAsStringAsync().Result</c> double-read bug fixed — content is read once.</item>
///   <item><c>_httpClient.BaseAddress</c> lazy-mutation replaced by per-call URI construction from cached settings.</item>
///   <item><c>RPObjectCache</c> + <c>MemoryCache.Default</c> replaced by injected <see cref="IMemoryCache"/>.</item>
///   <item>Serilog <c>Log.Logger</c> replaced by injected <see cref="ILogger{T}"/>.</item>
///   <item><c>_ignoreUnitTest</c> flag removed — DI mock injection replaces the bypass.</item>
///   <item>Single DI constructor — no <c>new</c> keyword anywhere.</item>
///   <item><c>IHttpClientFactory</c> used instead of a shared mutable <c>HttpClient</c> field.</item>
/// </list>
/// </summary>
public sealed class ManageUnifiedSettingsAsync : IManageUnifiedSettingsAsync
{
    #region Fields

    private readonly IUnifiedSettingsRepositoryAsync _settingsRepo;
    private readonly IProductInternalSettingRepositoryAsync _internalSettingRepo;
    private readonly ITokenHelperAsync _tokenHelper;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ManageUnifiedSettingsAsync> _logger;

    private const int ProductSettingsCacheSeconds = 120;
    private const int UnifiedSettingsCacheSeconds = 120;

    private static readonly JsonSerializerOptions SystemTextOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    #endregion

    #region Constructor

    /// <param name="httpClientFactory">
    /// Resolves <c>HttpClient</c> instances per call — eliminates the shared mutable
    /// <c>_httpClient</c> field and its thread-unsafe <c>DefaultRequestHeaders</c> mutations.
    /// </param>
    public ManageUnifiedSettingsAsync(
        IUnifiedSettingsRepositoryAsync settingsRepo,
        IProductInternalSettingRepositoryAsync internalSettingRepo,
        ITokenHelperAsync tokenHelper,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<ManageUnifiedSettingsAsync> logger)
    {
        _settingsRepo = settingsRepo ?? throw new ArgumentNullException(nameof(settingsRepo));
        _internalSettingRepo = internalSettingRepo ?? throw new ArgumentNullException(nameof(internalSettingRepo));
        _tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManageUnifiedSettingsAsync — settings reads
    // ════════════════════════════════════════════════════════════════════════

    #region GetUnifiedSettingsCachedAsync / GetUnifiedSettingsAsync

    /// <inheritdoc/>
    /// <remarks>
    /// Replaces: <c>RPObjectCache.GetFromCache(key, 120, () => GetUnifiedSettings(...))</c>
    /// with <see cref="IMemoryCache.GetOrCreateAsync{T}"/> — same TTL, no static shared state.
    /// </remarks>
    public async Task<IList<Setting>> GetUnifiedSettingsCachedAsync(
        long partyId, string category, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"GetUnifiedSettingsCached_{category}_{partyId}";

        return await _cache.GetOrCreateAsync<IList<Setting>>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(UnifiedSettingsCacheSeconds);
            return await GetUnifiedSettingsAsync(partyId, category, cancellationToken);
        }) ?? [];
    }

    /// <inheritdoc/>
    public async Task<IList<Setting>> GetUnifiedSettingsAsync(
        long partyId, string category, CancellationToken cancellationToken = default)
    {
        if (partyId == 0) throw new ArgumentException("Missing Organisation Id.", nameof(partyId));

        _logger.LogDebug("GetUnifiedSettingsAsync → partyId={Id} category={Cat}", partyId, category);

        try
        {
            var result = await _settingsRepo.GetUnifiedSettingsAsync(partyId, category, cancellationToken);
            _logger.LogDebug("GetUnifiedSettingsAsync → returned {Count} setting(s)", result?.Count ?? 0);
            return result ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUnifiedSettingsAsync failed for partyId={Id} category={Cat}", partyId, category);
            return [];
        }
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManageUnifiedSettingsAsync — provisioning write operations
    // ════════════════════════════════════════════════════════════════════════

    #region CreateUpdateCompanyInSettingAsync

    /// <inheritdoc/>
    /// <remarks>
    /// Replaces: <c>_httpClient.SendAsync(request).Result</c> (sync-over-async deadlock hazard)
    /// and <c>_httpClient.DefaultRequestHeaders.Authorization = ...</c> (not thread-safe on shared client).
    /// The Bearer token is now set on the <see cref="HttpRequestMessage"/>, not the client.
    /// </remarks>
    public async Task<bool> CreateUpdateCompanyInSettingAsync(
        UnifiedSettingCompanyPropertyPayload upfmCompany, HttpMethod requestType,
        CancellationToken cancellationToken = default)
    {
        string baseUrl = await GetSettingsApiBaseUrlAsync(cancellationToken);
        string token = await _tokenHelper.GetUnifiedLoginServerTokenAsync("unifiedsettingsapi", cancellationToken);

        string uri = $"{baseUrl}v2/provisioning/company";
        string payload = JsonConvert.SerializeObject(upfmCompany);

        _logger.LogDebug("CreateUpdateCompanyInSettingAsync → {Method} {Uri}", requestType.Method, uri);

        using var request = new HttpRequestMessage(requestType, uri)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var client = _httpClientFactory.CreateClient();
        var response = await client.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode) return true;
        _logger.LogWarning("CreateUpdateCompanyInSettingAsync → {Status}", response.StatusCode);
        return false;
    }

    #endregion

    #region CreateUpdatePropertyInSettingAsync

    /// <inheritdoc/>
    public async Task<bool> CreateUpdatePropertyInSettingAsync(
        UnifiedSettingCompanyPropertyPayload upfmProperties, HttpMethod requestType,
        CancellationToken cancellationToken = default)
    {
        string baseUrl = await GetSettingsApiBaseUrlAsync(cancellationToken);
        string token = await _tokenHelper.GetUnifiedLoginServerTokenAsync("unifiedsettingsapi", cancellationToken);

        string uri = $"{baseUrl}v2/provisioning/property";
        string payload = JsonConvert.SerializeObject(upfmProperties);

        _logger.LogDebug("CreateUpdatePropertyInSettingAsync → {Method} {Uri}", requestType.Method, uri);

        using var request = new HttpRequestMessage(requestType, uri)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var client = _httpClientFactory.CreateClient();
        var response = await client.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode) return true;
        _logger.LogWarning("CreateUpdatePropertyInSettingAsync → {Status}", response.StatusCode);
        return false;
    }

    #endregion

    #region DeletePropertyInSettingAsync

    /// <inheritdoc/>
    public async Task<bool> DeletePropertyInSettingAsync(
        string settingsPropertyInstanceId, CancellationToken cancellationToken = default)
    {
        string baseUrl = await GetSettingsApiBaseUrlAsync(cancellationToken);
        string token = await _tokenHelper.GetUnifiedLoginServerTokenAsync("unifiedsettingsapi", cancellationToken);

        string uri = $"{baseUrl}v2/provisioning/property/{settingsPropertyInstanceId}";
        _logger.LogDebug("DeletePropertyInSettingAsync → {Uri}", uri);

        using var request = new HttpRequestMessage(HttpMethod.Delete, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var client = _httpClientFactory.CreateClient();
        var response = await client.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode) return true;
        _logger.LogWarning("DeletePropertyInSettingAsync → {Status}", response.StatusCode);
        return false;
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManageUnifiedSettingsAsync — Kong internal settings read
    // ════════════════════════════════════════════════════════════════════════

    #region GetCompanyInternalSettingsAsync

    /// <inheritdoc/>
    /// <remarks>
    /// Fixes over the sync version:
    /// <list type="bullet">
    ///   <item><c>_httpClient.GetAsync(uri).Result</c> → <c>await</c>.</item>
    ///   <item><c>response.Content.ReadAsStringAsync().Result</c> called twice (second call on a consumed stream) → read once into a local variable.</item>
    ///   <item>Kong headers (<c>apikey</c>, <c>vanity-host</c>) added to <see cref="HttpRequestMessage"/> — not to <c>DefaultRequestHeaders</c> — making concurrent calls safe.</item>
    ///   <item><c>uri = _httpClient.BaseAddress + uri</c> (which doubled the base path) replaced by clean URI construction.</item>
    /// </list>
    /// </remarks>
    public async Task<InternalSettingResponse> GetCompanyInternalSettingsAsync(
        Guid companyId, string source, string settingType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(settingType)) return new InternalSettingResponse();

        var productSettings = await GetProductInternalSettingsAsync(cancellationToken);

        string Get(string key) => productSettings
            .FirstOrDefault(a => a.Name.Equals(key, StringComparison.OrdinalIgnoreCase))?.Value
            ?? string.Empty;

        string kongBase = Get("KongApiEndPoint");
        string kongKey = Get("KONG_KEY");
        string kongVanityUrl = Get("Kong-Vanity-url");
        string settingsApiFormat = Get("CompanyInternationalSettingsAPI");

        if (string.IsNullOrEmpty(kongBase) || string.IsNullOrEmpty(kongKey) || string.IsNullOrEmpty(settingsApiFormat))
        {
            _logger.LogWarning("GetCompanyInternalSettingsAsync → KongApiEndPoint/KONG_KEY/CompanyInternationalSettingsAPI not found in product settings.");
            return new InternalSettingResponse();
        }

        // ── FIX: original code did uri = _httpClient.BaseAddress + uri which doubled the base path ──
        string relativePath = string.Format(settingsApiFormat, source, companyId, settingType);
        string fullUri = $"{kongBase.TrimEnd('/')}/{relativePath.TrimStart('/')}";

        _logger.LogDebug("GetCompanyInternalSettingsAsync → GET {Uri}", fullUri);

        // ── FIX: headers added to the REQUEST, not DefaultRequestHeaders — safe for concurrent use ──
        using var request = new HttpRequestMessage(HttpMethod.Get, fullUri);
        request.Headers.Add("apikey", kongKey);
        if (!string.IsNullOrEmpty(kongVanityUrl))
            request.Headers.Add("vanity-host", kongVanityUrl);

        var client = _httpClientFactory.CreateClient();
        var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogDebug("GetCompanyInternalSettingsAsync → {Status} for companyId={Id}", response.StatusCode, companyId);
            return new InternalSettingResponse();
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            return new InternalSettingResponse();

        // ── FIX: original called ReadAsStringAsync().Result twice; second call returns "" on a consumed stream ──
        string content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrEmpty(content))
        {
            _logger.LogDebug("GetCompanyInternalSettingsAsync → empty body for companyId={Id}", companyId);
            return new InternalSettingResponse();
        }

        var result = System.Text.Json.JsonSerializer.Deserialize<InternalSettingResponse>(content, SystemTextOptions);
        _logger.LogDebug("GetCompanyInternalSettingsAsync → deserialized for companyId={Id}", companyId);
        return result ?? new InternalSettingResponse();
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Private — helpers
    // ════════════════════════════════════════════════════════════════════════

    #region GetProductInternalSettingsAsync

    /// <summary>
    /// Replaces: <c>new RPObjectCache().GetFromCache(key, 120, () => _productInternalSettingRepository.GetProductInternalSettings(...))</c>
    /// — same 120-second TTL, uses <see cref="IMemoryCache"/> (DI-managed, no static shared state).
    /// </summary>
    private async Task<List<ProductInternalSetting>> GetProductInternalSettingsAsync(
        CancellationToken ct)
    {
        string cacheKey = $"productInternalSetting_{(int)ProductEnum.UnifiedPlatform}";

        return await _cache.GetOrCreateAsync<List<ProductInternalSetting>>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ProductSettingsCacheSeconds);
            var settings = await _internalSettingRepo.GetProductInternalSettingsAsync(
                (int)ProductEnum.UnifiedPlatform, ct);
            return settings?.ToList() ?? [];
        }) ?? [];
    }

    #endregion

    #region GetSettingsApiBaseUrlAsync

    /// <summary>
    /// Replaces: the lazy <c>GetConfigurationSetting()</c> method that mutated
    /// <c>_httpClient.BaseAddress</c> on first call — not safe on a shared client.
    /// Now resolves the Settings API base URL from cached product settings on every call.
    /// The result is cached so the DB is not hit on every request.
    /// </summary>
    private async Task<string> GetSettingsApiBaseUrlAsync(CancellationToken ct)
    {
        var settings = await GetProductInternalSettingsAsync(ct);

        string baseUrl = settings
            .FirstOrDefault(a => a.Name.Equals("SettingsApiEndPoint", StringComparison.OrdinalIgnoreCase))
            ?.Value ?? string.Empty;

        if (string.IsNullOrEmpty(baseUrl))
            throw new InvalidOperationException("SettingsApiEndPoint is not configured in product internal settings.");

        // Guarantee trailing slash so relative paths combine cleanly
        return baseUrl.EndsWith('/') ? baseUrl : baseUrl + '/';
    }

    #endregion
}