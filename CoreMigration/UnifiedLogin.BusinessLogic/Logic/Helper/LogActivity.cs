using Newtonsoft.Json;
using Serilog;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json; // added
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Logic.Helper;

/// <summary>
/// Log activity class for writing activity logs to external API
/// </summary>
public static class LogActivity
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private static bool _isInitialized = false;
    private static readonly object _lockObject = new object();
    // token cache
    private static string _accessToken;
    private static DateTime _accessTokenExpiresUtc;
    private static readonly object _tokenLock = new object();

    static LogActivity()
    {
        Initialize();
    }

    /// <summary>
    /// Initialize the HTTP client with base URL and timeout settings
    /// </summary>
    public static void Initialize()
    {
        lock (_lockObject)
        {
            if (!_isInitialized)
            {
                //Create instance of ProductInternalSettingRepository and call GetProductInternalSettings method with productId 3 (Unified Login)
                var repository = new ProductInternalSettingRepository();
                var settings = repository.GetProductInternalSettings(3).ToList();
                var baseUrl = settings.First(a => a.Name.Equals("ActivityLogUri", StringComparison.OrdinalIgnoreCase)).Value;

                /* 
                 previous TODO implemented below
                 var identityServerTokenAddress = productInternalSettingList.First(a => a.Name.Equals("TokenEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
                 var unifiedLoginClientid = productInternalSettingList.First(a => a.Name.Equals("UnifiedLoginServerClientName", StringComparison.OrdinalIgnoreCase)).Value;
                 var unifiedLoginClientsecret = productInternalSettingList.First(a => a.Name.Equals("UnifiedLoginServerClientSecret", StringComparison.OrdinalIgnoreCase)).Value;
                 var decodedClientsecret = Encoding.UTF8.GetString(Convert.FromBase64String(unifiedLoginClientsecret));
                 var Scope = "activityreader"
                 Create a ClientCredentialsTokenRequest and inject token into the below httpclient
                 */

                _httpClient.BaseAddress = new Uri(baseUrl);
                _httpClient.Timeout = TimeSpan.FromSeconds(20);
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Attempt to acquire token during initialization (non-fatal if fails)
                TryAcquireToken(settings);

                _isInitialized = true;
            }
        }
    }

    /// <summary>
    /// Ensures a valid bearer token is present on the HttpClient. Refreshes if expired or missing.
    /// </summary>
    private static void EnsureBearerToken(IList<ProductInternalSetting> settings)
    {
        // If token still valid (>60s remaining) do nothing
        if (!string.IsNullOrWhiteSpace(_accessToken) && DateTime.UtcNow < _accessTokenExpiresUtc.AddSeconds(-60))
        {
            return;
        }

        lock (_tokenLock)
        {
            if (!string.IsNullOrWhiteSpace(_accessToken) && DateTime.UtcNow < _accessTokenExpiresUtc.AddSeconds(-60))
            {
                return; // another thread refreshed
            }
            TryAcquireToken(settings);
        }
    }

    /// <summary>
    /// Acquire client credentials token for scope 'activityreader'. Swallows exceptions and leaves headers untouched on failure.
    /// </summary>
    private static void TryAcquireToken(IList<ProductInternalSetting> settings)
    {
        try
        {
            var tokenEndpoint = settings.FirstOrDefault(a => a.Name.Equals("TokenEndPoint", StringComparison.OrdinalIgnoreCase))?.Value;
            var clientId = settings.FirstOrDefault(a => a.Name.Equals("UnifiedLoginServerClientName", StringComparison.OrdinalIgnoreCase))?.Value;
            var clientSecretRaw = settings.FirstOrDefault(a => a.Name.Equals("UnifiedLoginServerClientSecret", StringComparison.OrdinalIgnoreCase))?.Value;
            if (string.IsNullOrWhiteSpace(tokenEndpoint) || string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecretRaw))
            {
                return; // missing config
            }
            string clientSecret;
            try
            {
                clientSecret = Encoding.UTF8.GetString(Convert.FromBase64String(clientSecretRaw));
            }
            catch
            {
                clientSecret = clientSecretRaw; // not base64
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials"),
                new("client_id", clientId),
                new("client_secret", clientSecret),
                new("scope", "activityreader")
            });

            using var response = _httpClient.Send(request); // synchronous because Initialize path
            if (!response.IsSuccessStatusCode)
            {
                return; // do not throw from initializer
            }
            var json = response.Content.ReadAsStringAsync().Result;
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("access_token", out var accessTokenElement))
            {
                return;
            }
            var token = accessTokenElement.GetString();
            if (string.IsNullOrWhiteSpace(token)) return;

            int expiresIn = 300; // default 5 min
            if (doc.RootElement.TryGetProperty("expires_in", out var expEl) && expEl.TryGetInt32(out var expVal) && expVal > 0)
            {
                expiresIn = expVal;
            }

            _accessToken = token;
            _accessTokenExpiresUtc = DateTime.UtcNow.AddSeconds(expiresIn);

            // Update Authorization header
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }
        catch (Exception ex)
        {
            // Non-fatal; log at debug level to avoid noise
            Log.Debug(ex, "ActivityLog token acquisition failed");
        }
    }

    /// <summary>
    /// Write activity in activity database (legacy method for backward compatibility)
    /// </summary>
    public static void WriteActivity(ActivityDetails activityDetails)
    {
        try
        {
            var derivedActivityDetails = new ActivityDetailMessage(activityDetails)
            {
                ServerName = Environment.MachineName,
                ApplicationTimestamp = DateTime.UtcNow,
            };

            LogActivityToApi(activityDetails);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception in Activity Logging");
        }
    }

    /// <summary>
    /// Add activity record with claims principal
    /// </summary>
    public static void AddActivityRecord(string activityType, LogActivityCategoryType activityCategory, ClaimsPrincipal user, string message,
        string toUserFirstName, string toUserLastName, long? toUserLoginId, string toUserLoginName,
        string toUserRealpageId, string productName, List<AdditionalParameters> additionalInformation = null)
    {
        try
        {
            if (message.Length <= 400)
            {
                LogActivityToApi(new ActivityDetails
                {
                    LogActivityTypeName = activityType,
                    LogCategoryName = activityCategory.ToString(),
                    CorrelationId = GetClaimValue(user, "CorrelationId") ?? Guid.Empty.ToString(),
                    Message = message,
                    FromUserFirstName = GetClaimValue(user, ClaimTypes.GivenName),
                    FromUserLastName = GetClaimValue(user, ClaimTypes.Surname),
                    FromUserLoginName = GetClaimValue(user, ClaimTypes.Name),
                    FromUserLoginId = long.TryParse(GetClaimValue(user, ClaimTypes.NameIdentifier), out var userId) ? userId : 0,
                    FromUserRealpageId = GetClaimValue(user, "RealPageId") ?? Guid.Empty.ToString(),
                    ToUserFirstName = toUserFirstName,
                    ToUserLastName = toUserLastName,
                    ToUserLoginName = toUserLoginName,
                    ToUserLoginId = toUserLoginId,
                    ToUserRealpageId = toUserRealpageId,
                    BooksMasterOrganizationId = long.TryParse(GetClaimValue(user, "BooksMasterOrganizationId"), out var booksMasterId) ? booksMasterId : 0,
                    OrganizationPartyId = long.TryParse(GetClaimValue(user, "OrganizationPartyId"), out var orgPartyId) ? orgPartyId : 0,
                    BooksProductCode = productName,
                    AdditionalInformation = additionalInformation
                });
            }
            else
            {
                var logChunks = SplitLogInto400CharacterChunks(message);

                foreach (var chunk in logChunks)
                {
                    LogActivityToApi(new ActivityDetails
                    {
                        LogActivityTypeName = activityType,
                        LogCategoryName = activityCategory.ToString(),
                        CorrelationId = GetClaimValue(user, "CorrelationId") ?? Guid.Empty.ToString(),
                        Message = chunk,
                        FromUserFirstName = GetClaimValue(user, ClaimTypes.GivenName),
                        FromUserLastName = GetClaimValue(user, ClaimTypes.Surname),
                        FromUserLoginName = GetClaimValue(user, ClaimTypes.Name),
                        FromUserLoginId = long.TryParse(GetClaimValue(user, ClaimTypes.NameIdentifier), out var userId) ? userId : 0,
                        FromUserRealpageId = GetClaimValue(user, "RealPageId") ?? Guid.Empty.ToString(),
                        ToUserFirstName = toUserFirstName,
                        ToUserLastName = toUserLastName,
                        ToUserLoginName = toUserLoginName,
                        ToUserLoginId = toUserLoginId,
                        ToUserRealpageId = toUserRealpageId,
                        BooksMasterOrganizationId = long.TryParse(GetClaimValue(user, "BooksMasterOrganizationId"), out var booksMasterId) ? booksMasterId : 0,
                        OrganizationPartyId = long.TryParse(GetClaimValue(user, "OrganizationPartyId"), out var orgPartyId) ? orgPartyId : 0,
                        BooksProductCode = productName,
                        AdditionalInformation = additionalInformation
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception in Activity Logging");
        }
    }

    /// <summary>
    /// Add activity record asynchronously with claims principal
    /// </summary>
    public static async Task<bool> AddActivityRecordAsync(string activityType, LogActivityCategoryType activityCategory, ClaimsPrincipal user, string message,
        string toUserFirstName, string toUserLastName, long? toUserLoginId, string toUserLoginName,
        string toUserRealpageId, string productName, List<AdditionalParameters> additionalInformation = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return await LogActivityToApiAsync(new ActivityDetails
            {
                LogActivityTypeName = activityType,
                LogCategoryName = activityCategory.ToString(),
                CorrelationId = GetClaimValue(user, "CorrelationId") ?? Guid.Empty.ToString(),
                Message = message,
                FromUserFirstName = GetClaimValue(user, ClaimTypes.GivenName),
                FromUserLastName = GetClaimValue(user, ClaimTypes.Surname),
                FromUserLoginName = GetClaimValue(user, ClaimTypes.Name),
                FromUserLoginId = long.TryParse(GetClaimValue(user, ClaimTypes.NameIdentifier), out var userId) ? userId : 0,
                FromUserRealpageId = GetClaimValue(user, "RealPageId") ?? Guid.Empty.ToString(),
                ToUserFirstName = toUserFirstName,
                ToUserLastName = toUserLastName,
                ToUserLoginName = toUserLoginName,
                ToUserLoginId = toUserLoginId,
                ToUserRealpageId = toUserRealpageId,
                BooksMasterOrganizationId = long.TryParse(GetClaimValue(user, "BooksMasterOrganizationId"), out var booksMasterId) ? booksMasterId : 0,
                OrganizationPartyId = long.TryParse(GetClaimValue(user, "OrganizationPartyId"), out var orgPartyId) ? orgPartyId : 0,
                BooksProductCode = productName,
                AdditionalInformation = additionalInformation,
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception in Activity Logging");
        }

        return false;
    }

    /// <summary>
    /// Add activity record without claims principal
    /// </summary>
    public static void AddActivityRecordWithoutClaims(string activityType, LogActivityCategoryType activityCategory, string message,
        string firstName, string lastName, string loginName, long userId, Guid realPageId, long booksMasterOrganizationId,
        long organizationPartyId, string toUserFirstName, string toUserLastName, long? toUserLoginId, string toUserLoginName,
        string toUserRealpageId, string productName, List<AdditionalParameters> additionalInformation = null)
    {
        try
        {
            if (message.Length <= 400)
            {
                LogActivityToApi(new ActivityDetails
                {
                    LogActivityTypeName = activityType,
                    LogCategoryName = activityCategory.ToString(),
                    CorrelationId = Guid.Empty.ToString(),
                    Message = message,
                    FromUserFirstName = firstName,
                    FromUserLastName = lastName,
                    FromUserLoginName = loginName,
                    FromUserLoginId = userId,
                    FromUserRealpageId = realPageId.ToString(),
                    ToUserFirstName = toUserFirstName,
                    ToUserLastName = toUserLastName,
                    ToUserLoginName = toUserLoginName,
                    ToUserLoginId = toUserLoginId,
                    ToUserRealpageId = toUserRealpageId,
                    BooksMasterOrganizationId = booksMasterOrganizationId,
                    OrganizationPartyId = organizationPartyId,
                    BooksProductCode = productName,
                    AdditionalInformation = additionalInformation
                });
            }
            else
            {
                var logChunks = SplitLogInto400CharacterChunks(message);

                foreach (var chunk in logChunks)
                {
                    LogActivityToApi(new ActivityDetails
                    {
                        LogActivityTypeName = activityType,
                        LogCategoryName = activityCategory.ToString(),
                        CorrelationId = Guid.Empty.ToString(),
                        Message = chunk,
                        FromUserFirstName = firstName,
                        FromUserLastName = lastName,
                        FromUserLoginName = loginName,
                        FromUserLoginId = userId,
                        FromUserRealpageId = realPageId.ToString(),
                        ToUserFirstName = toUserFirstName,
                        ToUserLastName = toUserLastName,
                        ToUserLoginName = toUserLoginName,
                        ToUserLoginId = toUserLoginId,
                        ToUserRealpageId = toUserRealpageId,
                        BooksMasterOrganizationId = booksMasterOrganizationId,
                        OrganizationPartyId = organizationPartyId,
                        BooksProductCode = productName,
                        AdditionalInformation = additionalInformation
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception in Activity Logging");
        }
    }

    /// <summary>
    /// Add activity record without claims principal asynchronously
    /// </summary>
    public static async Task<bool> AddActivityRecordWithoutClaimsAsync(string activityType, LogActivityCategoryType activityCategory, string message,
        string firstName, string lastName, string loginName, long userId, Guid realPageId, long booksMasterOrganizationId,
        long organizationPartyId, string toUserFirstName, string toUserLastName, long? toUserLoginId, string toUserLoginName,
        string toUserRealpageId, string productName, List<AdditionalParameters> additionalInformation = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return await LogActivityToApiAsync(new ActivityDetails
            {
                LogActivityTypeName = activityType,
                LogCategoryName = activityCategory.ToString(),
                CorrelationId = Guid.Empty.ToString(),
                Message = message,
                FromUserFirstName = firstName,
                FromUserLastName = lastName,
                FromUserLoginName = loginName,
                FromUserLoginId = userId,
                FromUserRealpageId = realPageId.ToString(),
                ToUserFirstName = toUserFirstName,
                ToUserLastName = toUserLastName,
                ToUserLoginName = toUserLoginName,
                ToUserLoginId = toUserLoginId,
                ToUserRealpageId = toUserRealpageId,
                BooksMasterOrganizationId = booksMasterOrganizationId,
                OrganizationPartyId = organizationPartyId,
                BooksProductCode = productName,
                AdditionalInformation = additionalInformation
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception in Activity Logging");
        }

        return false;
    }

    #region Private Helper Methods

    /// <summary>
    /// Split log message into 400 character chunks
    /// </summary>
    private static IList<string> SplitLogInto400CharacterChunks(string text)
    {
        List<string> chunks = new List<string>();
        int offset = 0;
        while (offset < text.Length)
        {
            int size = Math.Min(400, text.Length - offset);
            chunks.Add(text.Substring(offset, size));
            offset += size;
        }
        return chunks;
    }

    /// <summary>
    /// Log activity to external API (synchronous)
    /// </summary>
    private static void LogActivityToApi(ActivityDetails details)
    {
        try
        {
            EnsureInitialized();
            // refresh token if needed (settings productId 3)
            var repository = new ProductInternalSettingRepository();
            var settings = repository.GetProductInternalSettings(3).ToList();
            EnsureBearerToken(settings);

            var content = new StringContent(JsonConvert.SerializeObject(details), Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync("api/write", content).Result;

            if (!response.IsSuccessStatusCode)
            {
                Log.ForContext("ActivityDetails", JsonConvert.SerializeObject(details))
                   .ForContext("ResponseContent", response.Content.ReadAsStringAsync().Result)
                   .Error("Activity log failed with status code: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Activity log failed");
        }
    }

    /// <summary>
    /// Log activity to external API (asynchronous)
    /// </summary>
    private static async Task<bool> LogActivityToApiAsync(ActivityDetails details, CancellationToken cancellationToken = default)
    {
        try
        {
            EnsureInitialized();
            var repository = new ProductInternalSettingRepository();
            var settings = repository.GetProductInternalSettings(3).ToList();
            EnsureBearerToken(settings);

            var content = new StringContent(JsonConvert.SerializeObject(details), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/write", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Log.ForContext("ActivityDetails", JsonConvert.SerializeObject(details))
                   .ForContext("ResponseContent", responseContent)
                   .Error("Activity log failed with status code: {StatusCode}", response.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Activity log failed");
            return false;
        }
    }

    /// <summary>
    /// Get claim value from claims principal
    /// </summary>
    private static string GetClaimValue(ClaimsPrincipal user, string claimType)
    {
        return user?.FindFirst(claimType)?.Value;
    }

    /// <summary>
    /// Ensure HTTP client is initialized
    /// </summary>
    private static void EnsureInitialized()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("LogActivity must be initialized with Initialize() method before use. Call LogActivity.Initialize(baseUrl) at application startup.");
        }
    }

    #endregion
}