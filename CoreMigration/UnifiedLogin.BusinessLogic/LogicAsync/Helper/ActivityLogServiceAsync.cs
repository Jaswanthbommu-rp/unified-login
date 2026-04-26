using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Helper;

/// <summary>
/// Async-first, DI-injectable activity logging service.
/// <para>
/// Replaces the static <c>LogActivity</c> helper. Key improvements over the sync version:
/// <list type="bullet">
///   <item><c>IHttpClientFactory</c> replaces the static <c>HttpClient</c> field —
///         avoids socket exhaustion and DNS-change blindness.</item>
///   <item><c>IServiceScopeFactory</c> is used to create a transient scope when
///         fetching product settings, resolving the captive-dependency problem of a
///         Singleton consuming a Scoped repository.</item>
///   <item><c>SemaphoreSlim</c> replaces <c>lock()</c> for both the settings cache and
///         the token refresh path — <c>lock</c> cannot be held across <c>await</c>.</item>
///   <item>All HTTP I/O is genuinely async — no <c>.Result</c> or <c>.GetAwaiter().GetResult()</c>.</item>
/// </list>
/// </para>
/// <para><b>DI registration:</b> Singleton.</para>
/// </summary>
public sealed class ActivityLogServiceAsync : IActivityLogServiceAsync
{
    private const int    ProductId          = 3;   // UnifiedLogin product
    private const int    MessageChunkSize   = 400;
    private const int    TokenExpiryMarginS = 60;
    private const string ApiWritePath       = "api/write";
    private const string TokenScope         = "activityreader";

    private readonly IHttpClientFactory   _httpClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ActivityLogServiceAsync> _logger;

    // ── Settings cache ────────────────────────────────────────────────────────
    private IList<ProductInternalSetting>? _cachedSettings;
    private DateTime                       _settingsCacheExpiry = DateTime.MinValue;
    private string?                        _baseUrl;
    private readonly SemaphoreSlim         _settingsSemaphore = new(1, 1);
    private static readonly TimeSpan       SettingsCacheDuration = TimeSpan.FromDays(1);

    // ── Bearer token ──────────────────────────────────────────────────────────
    private string?  _accessToken;
    private DateTime _accessTokenExpiresUtc;
    private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);

    public ActivityLogServiceAsync(
        IHttpClientFactory   httpClientFactory,
        IServiceScopeFactory scopeFactory,
        ILogger<ActivityLogServiceAsync> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _scopeFactory      = scopeFactory      ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger            = logger            ?? throw new ArgumentNullException(nameof(logger));
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task WriteActivityAsync(
        ActivityDetails activityDetails,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await PostToApiAsync(activityDetails, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WriteActivityAsync failed");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> AddActivityRecordAsync(
        string activityType,
        LogActivityCategoryType activityCategory,
        ClaimsPrincipal user,
        string message,
        string toUserFirstName,
        string toUserLastName,
        long? toUserLoginId,
        string toUserLoginName,
        string toUserRealpageId,
        string productName,
        List<AdditionalParameters>? additionalInformation = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ActivityDetails BuildDetails(string msg) => new()
            {
                LogActivityTypeName       = activityType,
                LogCategoryName           = activityCategory.ToString(),
                CorrelationId             = GetClaim(user, "CorrelationId") ?? Guid.Empty.ToString(),
                Message                   = msg,
                FromUserFirstName         = GetClaim(user, ClaimTypes.GivenName),
                FromUserLastName          = GetClaim(user, ClaimTypes.Surname),
                FromUserLoginName         = GetClaim(user, ClaimTypes.Name),
                FromUserLoginId           = long.TryParse(GetClaim(user, ClaimTypes.NameIdentifier), out var uid) ? uid : 0,
                FromUserRealpageId        = GetClaim(user, "RealPageId") ?? Guid.Empty.ToString(),
                ToUserFirstName           = toUserFirstName,
                ToUserLastName            = toUserLastName,
                ToUserLoginName           = toUserLoginName,
                ToUserLoginId             = toUserLoginId,
                ToUserRealpageId          = toUserRealpageId,
                BooksMasterOrganizationId = long.TryParse(GetClaim(user, "BooksMasterOrganizationId"), out var bmo) ? bmo : 0,
                OrganizationPartyId       = long.TryParse(GetClaim(user, "OrganizationPartyId"), out var opi) ? opi : 0,
                BooksProductCode          = productName,
                AdditionalInformation     = additionalInformation
            };

            return await PostChunkedAsync(message, BuildDetails, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddActivityRecordAsync failed");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> AddActivityRecordWithoutClaimsAsync(
        string activityType,
        LogActivityCategoryType activityCategory,
        string message,
        string firstName,
        string lastName,
        string loginName,
        long userId,
        Guid realPageId,
        long booksMasterOrganizationId,
        long organizationPartyId,
        string toUserFirstName,
        string toUserLastName,
        long? toUserLoginId,
        string toUserLoginName,
        string toUserRealpageId,
        string productName,
        List<AdditionalParameters>? additionalInformation = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ActivityDetails BuildDetails(string msg) => new()
            {
                LogActivityTypeName       = activityType,
                LogCategoryName           = activityCategory.ToString(),
                CorrelationId             = Guid.Empty.ToString(),
                Message                   = msg,
                FromUserFirstName         = firstName,
                FromUserLastName          = lastName,
                FromUserLoginName         = loginName,
                FromUserLoginId           = userId,
                FromUserRealpageId        = realPageId.ToString(),
                ToUserFirstName           = toUserFirstName,
                ToUserLastName            = toUserLastName,
                ToUserLoginName           = toUserLoginName,
                ToUserLoginId             = toUserLoginId,
                ToUserRealpageId          = toUserRealpageId,
                BooksMasterOrganizationId = booksMasterOrganizationId,
                OrganizationPartyId       = organizationPartyId,
                BooksProductCode          = productName,
                AdditionalInformation     = additionalInformation
            };

            return await PostChunkedAsync(message, BuildDetails, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddActivityRecordWithoutClaimsAsync failed");
            return false;
        }
    }

    // ── Private — chunking ────────────────────────────────────────────────────

    private async Task<bool> PostChunkedAsync(
        string message,
        Func<string, ActivityDetails> buildDetails,
        CancellationToken cancellationToken)
    {
        if (message.Length <= MessageChunkSize)
            return await PostToApiAsync(buildDetails(message), cancellationToken).ConfigureAwait(false);

        bool allOk = true;
        foreach (var chunk in SplitIntoChunks(message, MessageChunkSize))
            allOk &= await PostToApiAsync(buildDetails(chunk), cancellationToken).ConfigureAwait(false);

        return allOk;
    }

    // ── Private — HTTP ────────────────────────────────────────────────────────

    private async Task<bool> PostToApiAsync(ActivityDetails details, CancellationToken cancellationToken)
    {
        try
        {
            var settings = await GetCachedSettingsAsync(cancellationToken).ConfigureAwait(false);
            var token    = await EnsureBearerTokenAsync(settings, cancellationToken).ConfigureAwait(false);

            var client  = _httpClientFactory.CreateClient("ActivityLog");
            var fullUrl = _baseUrl!.TrimEnd('/') + "/" + ApiWritePath;

            using var request = new HttpRequestMessage(HttpMethod.Post, fullUrl)
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(details), Encoding.UTF8, "application/json")
            };

            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogError(
                    "ActivityLog POST failed. Status={Status} Body={Body}",
                    response.StatusCode, body);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostToApiAsync failed");
            return false;
        }
    }

    // ── Private — settings cache (async-safe with SemaphoreSlim) ─────────────

    private async Task<IList<ProductInternalSetting>> GetCachedSettingsAsync(CancellationToken cancellationToken)
    {
        // Fast path — no lock needed for volatile read
        if (_cachedSettings is not null && DateTime.UtcNow < _settingsCacheExpiry)
            return _cachedSettings;

        await _settingsSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Double-check inside the semaphore
            if (_cachedSettings is not null && DateTime.UtcNow < _settingsCacheExpiry)
                return _cachedSettings;

            // IProductInternalSettingRepositoryAsync may be Scoped; create a transient scope.
            await using var scope = _scopeFactory.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<IProductInternalSettingRepositoryAsync>();

            var settings = await repo.GetProductInternalSettingsAsync(ProductId, cancellationToken)
                                     .ConfigureAwait(false);

            _baseUrl              = settings.FirstOrDefault(s =>
                                        s.Name.Equals("ActivityLogUri", StringComparison.OrdinalIgnoreCase))?.Value
                                    ?? throw new InvalidOperationException("ActivityLogUri setting not found.");
            _cachedSettings       = settings;
            _settingsCacheExpiry  = DateTime.UtcNow.Add(SettingsCacheDuration);

            return _cachedSettings;
        }
        catch (Exception ex) when (_cachedSettings is not null)
        {
            // Extend expired cache by 1 hour rather than hard-failing
            _logger.LogWarning(ex, "Settings refresh failed — extending cached settings by 1 hour");
            _settingsCacheExpiry = DateTime.UtcNow.AddHours(1);
            return _cachedSettings;
        }
        finally
        {
            _settingsSemaphore.Release();
        }
    }

    // ── Private — token management (async-safe with SemaphoreSlim) ───────────

    private async Task<string?> EnsureBearerTokenAsync(
        IList<ProductInternalSetting> settings,
        CancellationToken cancellationToken)
    {
        // Fast path — token still valid
        if (!string.IsNullOrWhiteSpace(_accessToken)
            && DateTime.UtcNow < _accessTokenExpiresUtc.AddSeconds(-TokenExpiryMarginS))
        {
            return _accessToken;
        }

        await _tokenSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Re-check inside the semaphore
            if (!string.IsNullOrWhiteSpace(_accessToken)
                && DateTime.UtcNow < _accessTokenExpiresUtc.AddSeconds(-TokenExpiryMarginS))
            {
                return _accessToken;
            }

            return await TryAcquireTokenAsync(settings, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _tokenSemaphore.Release();
        }
    }

    private async Task<string?> TryAcquireTokenAsync(
        IList<ProductInternalSetting> settings,
        CancellationToken cancellationToken)
    {
        try
        {
            var tokenEndpoint   = settings.FirstOrDefault(s => s.Name.Equals("TokenEndPoint",                        StringComparison.OrdinalIgnoreCase))?.Value;
            var clientId        = settings.FirstOrDefault(s => s.Name.Equals("UnifiedLoginServerClientName",         StringComparison.OrdinalIgnoreCase))?.Value;
            var clientSecretRaw = settings.FirstOrDefault(s => s.Name.Equals("UnifiedLoginServerClientSecret",       StringComparison.OrdinalIgnoreCase))?.Value;

            if (string.IsNullOrWhiteSpace(tokenEndpoint)
                || string.IsNullOrWhiteSpace(clientId)
                || string.IsNullOrWhiteSpace(clientSecretRaw))
            {
                return null; // missing config — proceed without token
            }

            // Secret may be base-64 encoded
            string clientSecret;
            try   { clientSecret = Encoding.UTF8.GetString(Convert.FromBase64String(clientSecretRaw)); }
            catch { clientSecret = clientSecretRaw; }

            var client = _httpClientFactory.CreateClient("ActivityLog");
            using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(
                [
                    new KeyValuePair<string, string>("grant_type",    "client_credentials"),
                    new KeyValuePair<string, string>("client_id",     clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("scope",         TokenScope)
                ])
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("access_token", out var tokenEl)) return null;

            var token = tokenEl.GetString();
            if (string.IsNullOrWhiteSpace(token)) return null;

            int expiresIn = doc.RootElement.TryGetProperty("expires_in", out var expEl)
                            && expEl.TryGetInt32(out var expVal) && expVal > 0
                            ? expVal
                            : 300;

            _accessToken           = token;
            _accessTokenExpiresUtc = DateTime.UtcNow.AddSeconds(expiresIn);

            return _accessToken;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "ActivityLog token acquisition failed");
            return null;
        }
    }

    // ── Private — utilities ───────────────────────────────────────────────────

    private static IEnumerable<string> SplitIntoChunks(string text, int size)
    {
        for (int offset = 0; offset < text.Length; offset += size)
            yield return text.Substring(offset, Math.Min(size, text.Length - offset));
    }

    private static string? GetClaim(ClaimsPrincipal user, string claimType)
        => user?.FindFirst(claimType)?.Value;
}
