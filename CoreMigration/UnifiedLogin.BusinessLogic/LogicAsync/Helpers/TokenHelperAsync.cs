using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Helpers;

/// <summary>
/// Async-first replacement for <see cref="TokenHelper"/>.
/// <para>Key improvements over the sync version:</para>
/// <list type="bullet">
///   <item><c>_httpClient.Send(request)</c> (blocking sync HTTP) → <c>await client.SendAsync(request, ct)</c>.</item>
///   <item><c>response.Content.ReadAsStringAsync().Result</c> → <c>await response.Content.ReadAsStringAsync(ct)</c> — read once into a local variable, no double-read hazard.</item>
///   <item>Static <c>HttpClient</c> field replaced by <see cref="IHttpClientFactory"/> — correct lifetime management, DNS rotation support.</item>
///   <item><c>RPObjectCache</c> replaced by injected <see cref="IMemoryCache"/> — no static shared state.</item>
///   <item>Single DI constructor — no <c>new</c> keyword anywhere.</item>
///   <item>Inner exceptions preserved via <c>InvalidOperationException(msg, inner)</c> — original code discarded stack traces.</item>
///   <item><c>tokenUri.GetHashCode()</c> cache key (unstable across processes) replaced by URI-escaped string.</item>
///   <item><c>ArgumentException.ThrowIfNullOrWhiteSpace</c> (.NET 10 guard API) replaces manual checks.</item>
/// </list>
/// </summary>
public sealed class TokenHelperAsync : ITokenHelperAsync
{
    #region Fields

    private readonly IProductInternalSettingRepositoryAsync _productRepo;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TokenHelperAsync> _logger;

    /// <summary>
    /// Named <see cref="HttpClient"/> registered via
    /// <c>services.AddHttpClient("TokenHelper")</c> in DI startup.
    /// No base address is required — all requests use absolute URIs.
    /// </summary>
    private const string HttpClientName = "TokenHelper";
    private const int TokenCacheSeconds = 300; // 5 min — well within typical token lifetime
    private const int SettingsCacheSeconds = 120; // 2 min — matches sync RPObjectCache TTL

    #endregion

    #region Constructor

    public TokenHelperAsync(
        IProductInternalSettingRepositoryAsync productRepo,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<TokenHelperAsync> logger)
    {
        _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // ITokenHelperAsync — public token acquisition
    // ════════════════════════════════════════════════════════════════════════

    #region GetUnifiedLoginServerTokenAsync

    /// <inheritdoc/>
    /// <remarks>
    /// Replaces: <c>RPObjectCache.GetFromCache(key, 300, () => RequestClientCredentialsToken(...))</c>
    /// with <see cref="IMemoryCache.GetOrCreateAsync{T}"/> — same TTL, no static shared state.
    /// Base-64 decoding of the client secret is preserved via <see cref="TryFromBase64"/>.
    /// </remarks>
    public async Task<string> GetUnifiedLoginServerTokenAsync(
        string scopes, CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await GetProductInternalSettingsAsync(ProductEnum.UnifiedPlatform, cancellationToken);

            string tokenEndPoint = Required(settings, "TokenEndPoint");
            string clientId = Required(settings, "UnifiedLoginServerClientName");
            string apiSecret = TryFromBase64(Required(settings, "UnifiedLoginServerClientSecret"));

            string cacheKey = $"GetUnifiedLoginServerToken_{clientId}_{scopes}";

            return await _cache.GetOrCreateAsync<string>(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(TokenCacheSeconds);
                _logger.LogDebug("GetUnifiedLoginServerTokenAsync → requesting new token for scope={Scope}", scopes);
                return await RequestClientCredentialsTokenAsync(tokenEndPoint, clientId, apiSecret, scopes, cancellationToken);
            }) ?? string.Empty;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException(
                $"TokenHelperAsync.GetUnifiedLoginServerTokenAsync failed: {ex.Message}", ex);
        }
    }

    #endregion

    #region GetClientCredentialServerTokenAsync

    /// <inheritdoc/>
    public async Task<string> GetClientCredentialServerTokenAsync(
        string clientId, string clientSecret, string scopes,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string issuerUri = ConfigReader.GetIssuerUri;
            string cacheKey = $"GetClientCredentialServerToken_{issuerUri}_{clientId}_{scopes}";

            return await _cache.GetOrCreateAsync<string>(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(TokenCacheSeconds);
                string endpoint = $"{issuerUri.TrimEnd('/')}/connect/token";
                _logger.LogDebug("GetClientCredentialServerTokenAsync → requesting token from {Ep}", endpoint);
                return await RequestClientCredentialsTokenAsync(endpoint, clientId, clientSecret, scopes, cancellationToken);
            }) ?? string.Empty;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException(
                $"TokenHelperAsync.GetClientCredentialServerTokenAsync failed: {ex.Message}", ex);
        }
    }

    #endregion

    #region GetExternalClientCredentialServerTokenAsync

    /// <inheritdoc/>
    /// <remarks>
    /// FIX: original used <c>tokenUri.GetHashCode()</c> in the cache key —
    /// <c>string.GetHashCode()</c> is <b>not stable</b> across process restarts in .NET.
    /// Replaced with <see cref="Uri.EscapeDataString"/> so the key is deterministic.
    /// </remarks>
    public async Task<string> GetExternalClientCredentialServerTokenAsync(
        string tokenUri, string clientId, string clientSecret, string scopes,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string cacheKey = $"GetExternalClientCredentialServerToken_{Uri.EscapeDataString(tokenUri)}_{clientId}_{scopes}";

            return await _cache.GetOrCreateAsync<string>(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(TokenCacheSeconds);
                _logger.LogDebug("GetExternalClientCredentialServerTokenAsync → requesting token from {Uri}", tokenUri);
                return await RequestClientCredentialsTokenAsync(tokenUri, clientId, clientSecret, scopes, cancellationToken);
            }) ?? string.Empty;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException(
                $"TokenHelperAsync.GetExternalClientCredentialServerTokenAsync failed: {ex.Message}", ex);
        }
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Private — core HTTP token request
    // ════════════════════════════════════════════════════════════════════════

    #region RequestClientCredentialsTokenAsync

    /// <summary>
    /// Executes an OAuth 2.0 client-credentials token request against <paramref name="tokenEndpoint"/>.
    /// <para>
    /// Replaces the sync version which used:
    /// <list type="bullet">
    ///   <item><c>_httpClient.Send(request)</c> — blocking call on a shared static client.</item>
    ///   <item><c>response.Content.ReadAsStringAsync().Result</c> — <c>.Result</c> deadlock hazard.</item>
    /// </list>
    /// </para>
    /// </summary>
    private async Task<string> RequestClientCredentialsTokenAsync(
        string tokenEndpoint, string clientId, string clientSecret, string scopes,
        CancellationToken ct)
    {
        // .NET 10 guard APIs — replaces manual if/throw blocks
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenEndpoint, nameof(tokenEndpoint));
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId, nameof(clientId));
        ArgumentException.ThrowIfNullOrWhiteSpace(clientSecret, nameof(clientSecret));

        using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("grant_type",    "client_credentials"),
            new KeyValuePair<string, string>("client_id",     clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("scope",         scopes ?? string.Empty)
        ]);

        var client = _httpClientFactory.CreateClient(HttpClientName);

        // ── FIX: was _httpClient.Send(request) — blocking sync HTTP on a static client ──
        using var response = await client.SendAsync(request, ct);

        // ── FIX: was ReadAsStringAsync().Result called separately in each branch —
        //         read once to avoid consuming the stream twice ──
        string json = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Token request failed {(int)response.StatusCode} {response.ReasonPhrase}. Body: {json}");
        }

        try
        {
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("access_token", out var tokenElement))
                throw new InvalidOperationException("access_token field missing in token response.");

            string accessToken = tokenElement.GetString();
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new InvalidOperationException("access_token is empty in token response.");

            return accessToken;
        }
        catch (JsonException jex)
        {
            // ── FIX: original swallowed inner exception — preserve via InnerException ──
            throw new InvalidOperationException(
                $"Invalid JSON in token response: {jex.Message}. Raw: {json}", jex);
        }
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Private — helpers
    // ════════════════════════════════════════════════════════════════════════

    #region GetProductInternalSettingsAsync

    /// <summary>
    /// Replaces: <c>new RPObjectCache().GetFromCache(key, 120, () => _productRepository.GetProductInternalSettings(...))</c>
    /// — same 120-second TTL, uses <see cref="IMemoryCache"/> (DI-managed, no static shared state).
    /// </summary>
    private async Task<List<ProductInternalSetting>> GetProductInternalSettingsAsync(
        ProductEnum product, CancellationToken ct)
    {
        string cacheKey = $"productInternalSetting_{(int)product}";

        return await _cache.GetOrCreateAsync<List<ProductInternalSetting>>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(SettingsCacheSeconds);
            var result = await _productRepo.GetProductInternalSettingsAsync((int)product, ct);
            return result?.ToList() ?? [];
        }) ?? [];
    }

    #endregion

    #region TryFromBase64 / Required

    /// <summary>
    /// Attempts to Base64-decode <paramref name="raw"/>.
    /// Returns the decoded value only when it contains at least one alphanumeric character
    /// (guards against accidentally double-decoding binary data).
    /// Preserved exactly from the sync version.
    /// </summary>
    private static string TryFromBase64(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return raw;
        try
        {
            byte[] bytes = Convert.FromBase64String(raw);
            string decoded = Encoding.UTF8.GetString(bytes);
            return decoded.Any(char.IsLetterOrDigit) ? decoded : raw;
        }
        catch
        {
            return raw;
        }
    }

    /// <summary>
    /// Retrieves a required setting value by name; throws <see cref="InvalidOperationException"/>
    /// with a clear message when the setting is missing — replaces silent <c>NullReferenceException</c>.
    /// </summary>
    private static string Required(IEnumerable<ProductInternalSetting> settings, string name)
    {
        string? value = settings
            .FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?.Value;

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException(
                $"Required product setting '{name}' is missing or empty.");

        return value;
    }

    #endregion
}