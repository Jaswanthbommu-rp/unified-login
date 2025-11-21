using Newtonsoft.Json;
using Serilog;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;

namespace UnifiedLogin.SharedObjects.Extensions
{
    /// <summary>
    /// Log activity class for writing activity logs to external API
    /// </summary>
    public static class LogActivity
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static bool _isInitialized = false;
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Initialize the HTTP client with base URL and timeout settings
        /// </summary>
        /// <param name="baseUrl">Base URL for the Activity Log API</param>
        /// <param name="timeoutSeconds">Timeout in seconds (default: 20)</param>
        public static void Initialize(string baseUrl, int timeoutSeconds = 20)
        {
            lock (_lockObject)
            {
                if (!_isInitialized)
                {
                    _httpClient.BaseAddress = new Uri(baseUrl);
                    _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                    _httpClient.DefaultRequestHeaders.Accept.Clear();
                    _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    _isInitialized = true;
                }
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

                if (string.IsNullOrEmpty(ConfigReader.GetActivityMQName))
                {
                    throw new Exception($"ActivityMQName is missing check config file.");
                }

                LogActivityToApi(activityDetails);

                //TODO-Migration-Core: Call activitylog write api / new activity log write api / write to kafka topic when migrated to new
                //using (var queue = new MessageQueue(ConfigReader.GetActivityMQName))
                //{
                //    var logMessage = new Message(derivedActivityDetails);
                //    queue.Send(logMessage);
                //}
            }
            catch (Exception ex)
            {
                // log exception in elastic
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
                var result = await LogActivityToApiAsync(new ActivityDetails
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
                return result;
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
                var result = await LogActivityToApiAsync(new ActivityDetails
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
                return result;
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

                string apiPathAndQuery = "api/write";
                var content = new StringContent(JsonConvert.SerializeObject(details), Encoding.UTF8, "application/json");
                var response = _httpClient.PostAsync(apiPathAndQuery, content).Result;

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

                string apiPathAndQuery = "api/write";
                var content = new StringContent(JsonConvert.SerializeObject(details), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(apiPathAndQuery, content, cancellationToken);

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
}
