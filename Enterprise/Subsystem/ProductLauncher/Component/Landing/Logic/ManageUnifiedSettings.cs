using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Enterprise.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    public class ManageUnifiedSettings : IManageUnifiedSettings
    {
        #region Private Variables
        IUnifiedSettingsRepository _unifiedSettingsRepository;
        private DefaultUserClaim _userClaim;

        const string MediaTypeName = "application/vnd.api+json";
        const int CacheTimeSeconds = 300;
        const int AuthTokenRefreshMinutes = 50;

        const int MAXRETRYCOUNT = 5;

        private HttpClient _httpClient;        
        readonly IProductInternalSettingRepository _productInternalSettingRepository;
        readonly ITokenHelper _tokenHelper;
        private bool _ignoreUnitTest = false;

        ObjectCache _manageSettingCache = MemoryCache.Default;
        #endregion

        #region Constructors
        /// <summary>
        /// ManageSecuritySettings Constructor
        /// </summary>
        /// <param name="repository">Repository</param>
        /// <param name="userClaim">Information about the user</param>
        /// <param name="messageHandler">messageHandler</param>
        public ManageUnifiedSettings(IRepository repository, DefaultUserClaim userClaim, HttpMessageHandler messageHandler)
        {
            _unifiedSettingsRepository = new UnifiedSettingsRepository(repository);
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _userClaim = userClaim;
            _tokenHelper = new TokenHelper(repository);
            _ignoreUnitTest = true;
            _httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("http://localhost") };
        }

        /// <summary>
        /// Create a basic instance of the ManageSecuritySettings class
        /// </summary>
        /// <param name="userClaim">Information about the user</param>
        public ManageUnifiedSettings(DefaultUserClaim userClaim)
        {
            _unifiedSettingsRepository = new UnifiedSettingsRepository();
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _userClaim = userClaim;
            _tokenHelper = new TokenHelper();
            _httpClient = new HttpClient();
        }
        #endregion

        #region Public ManageSecuritySettings methods

        /// <summary>
        /// Get Company Settings cached
        /// </summary>
        /// <param name="category">Setting Category type</param>
        /// <param name="partyId">Company Id</param>
        /// <returns>Security Settings List objects (KeyValue pairs)</returns>
        public IList<Setting> GetUnifiedSettingsCached(string category, long partyId)
        {
            RPObjectCache rpCache = new RPObjectCache();
            string cacheKey = $"GetUnifiedSettingsCached{category}_{partyId}";
            var unifiedSettings = rpCache.GetFromCache<IList<Setting>>(cacheKey, 120, () =>
            {
                return GetUnifiedSettings(category, partyId);
            });
            return unifiedSettings;
        }

        /// <summary>
        /// Get Company Settings
        /// </summary>
        /// <param name="category">Setting Category type</param>
        /// <param name="partyId">Company Id</param>
        /// <returns>Security Settings List objects (KeyValue pairs)</returns>
        public IList<Setting> GetUnifiedSettings(string category, long partyId)
        {
            IList<Setting> unfiedSettingList = new List<Setting>();
            Guid correlationId = Guid.NewGuid();
            Dictionary<string, object> logData = new Dictionary<string, object>
            {
                { "Get UnifiedSettings", $"Organization Id: {partyId}, Category: {category}" }
            };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, null, messageProperties: new object[] { "GetUnifiedSettings", "Begin" });

            if (partyId == 0)
            {
                throw new Exception("Missing Organization Id.");
            }

            try
            {
                unfiedSettingList = _unifiedSettingsRepository.GetUnifiedSettings(partyId, category);
            }
            catch (Exception exception)
            {
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", correlationId, null, exception, messageProperties: new object[] { "GetUnifiedSettings", "Exception" });
            }

            logData = new Dictionary<string, object>
            {
                { "Get UnifiedSettings: Data", unfiedSettingList }
            };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, null, messageProperties: new object[] { "GetUnifiedSettings", "End" });

            return unfiedSettingList;
        }


        #region Send Property and Company to Settings

        /// <summary>
        ///Send Company Instance to Unified settings to add/update Company
        /// </summary>
        /// <param name="upfmCompany"></param>
        /// <param name="requestType"></param>
        /// <returns></returns>
        public bool CreateUpdateCompanyInSetting(UnifiedSettingCompanyPropertyPayload upfmCompany, HttpMethod requestType)
        {
            GetConfigurationSetting();
            string ulClientToken = string.Empty;
            if (!_ignoreUnitTest)
            {
                ulClientToken = _tokenHelper.GetUnifiedLoginServerToken("unifiedsettingsapi");
            }
            Guid correlationId = Guid.NewGuid();
            //https://settingsapi-dev.realpage.com/v2/provisioning/company         
            string uri = $"v2/provisioning/company";
            Dictionary<string, object> logData = new Dictionary<string, object>() { { "uri", _httpClient.BaseAddress + uri }, { "upfmCompany", upfmCompany } };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, messageProperties: new object[] { "CreateUpdateCompanyInSetting", "Adding info" });

            var jsonToSave = JsonConvert.SerializeObject(upfmCompany);
            var request = new HttpRequestMessage
            {
                Method = requestType,
                Content = new StringContent(jsonToSave, Encoding.UTF8, "application/json"),
                RequestUri = new Uri(_httpClient.BaseAddress + uri)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ulClientToken);
            var response = _httpClient.SendAsync(request).Result;
            if (response != null && response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///Send Property Instance to Unified settings to add/update property
        /// </summary>
        /// <param name="upfmProperties"></param>
        /// <param name="requestType"></param>
        /// <returns></returns>
        public bool CreateUpdatePropertyInSetting(UnifiedSettingCompanyPropertyPayload upfmProperties, HttpMethod requestType)
        {
            GetConfigurationSetting();
            string ulClientToken = string.Empty;
			if (!_ignoreUnitTest)
			{
                ulClientToken = _tokenHelper.GetUnifiedLoginServerToken("unifiedsettingsapi");
            }
            Guid correlationId = Guid.NewGuid();           
            //https://settingsapi-dev.realpage.com/v2/provisioning/property           
            string uri = $"v2/provisioning/property";
            Dictionary<string, object> logData = new Dictionary<string, object>() { { "uri", _httpClient.BaseAddress + uri }, { "upfmProperties", upfmProperties } };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, messageProperties: new object[] { "CreateUpdatePropertyInSetting", "Adding info" });

            var jsonToSave = JsonConvert.SerializeObject(upfmProperties);
            var request = new HttpRequestMessage
            {
                Method = requestType,
                Content = new StringContent(jsonToSave, Encoding.UTF8, "application/json"),
                RequestUri = new Uri(_httpClient.BaseAddress + uri)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ulClientToken);
            var response = _httpClient.SendAsync(request).Result;
            if (response != null && response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }



        /// <summary>
        ///Send Property Instance to Unified settings to delete instance
        /// </summary>
        /// <param name="settingsPropertyInstanceID">Settings PropertyInstance</param>
        /// <returns></returns>
        public bool DeletePropertyInSetting(string settingsPropertyInstanceID)
        {
            GetConfigurationSetting();
            string ulClientToken = string.Empty;
            if (!_ignoreUnitTest)
            {
                ulClientToken = _tokenHelper.GetUnifiedLoginServerToken("unifiedsettingsapi");
            }
            Guid correlationId = Guid.NewGuid();
            //https://settingsapi-dev.realpage.com/v2/provisioning/property/{propertyId}         
            string uri = $"v2/provisioning/property/{settingsPropertyInstanceID}";
            Dictionary<string, object> logData = new Dictionary<string, object>() { { "uri", _httpClient.BaseAddress + uri }, { "propertyInstance", settingsPropertyInstanceID } };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, messageProperties: new object[] { "DeletePropertyInSetting", "Delete info" });
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_httpClient.BaseAddress + uri)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ulClientToken);
            var response = _httpClient.SendAsync(request).Result;
            if (response != null && response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }


        public InternalSettingResponse GetCompanyInternalSettings(Guid companyId, string source, string settingType)
        {
            Guid correlationId = Guid.NewGuid();
            WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.Entry", $"GetCompanyInternalSettings ENTRY - CompanyId: {companyId}, Source: {source}, SettingType: {settingType}" });

            InternalSettingResponse internalSetting = new InternalSettingResponse();
            try
            {
                if (string.IsNullOrEmpty(settingType))
                {
                    WriteToLog(LogEventLevel.Warning, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.EarlyExit", $"SettingType is null/empty. CompanyId: {companyId}, Source: {source}. Returning empty response." });
                    return internalSetting;
                }

                string kongUri = string.Empty;
                string kongVanityUrl = string.Empty;

                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.Step", $"Step: LoadProductInternalSettings, CompanyId: {companyId}, ProductId: {(int)ProductEnum.UnifiedPlatform}" });
                var productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);
                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.Step.Result", $"Step: LoadProductInternalSettings, CompanyId: {companyId}, SettingCount: {productInternalSettingList?.Count() ?? 0}" });

                var kongUriSetting = productInternalSettingList?.FirstOrDefault(a => a.Name.Equals("KongApiEndPoint", StringComparison.OrdinalIgnoreCase));
                kongUri = kongUriSetting?.Value ?? string.Empty;
                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.Step", $"Step: Read KongApiEndPoint, CompanyId: {companyId}, Found: {kongUriSetting != null}, HasValue: {!string.IsNullOrEmpty(kongUri)}" });

                var kongKeySetting = productInternalSettingList?.FirstOrDefault(a => a.Name.Equals("KONG_KEY", StringComparison.OrdinalIgnoreCase));
                string kongKey = kongKeySetting?.Value ?? string.Empty;
                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.Step", $"Step: Read KONG_KEY, CompanyId: {companyId}, Found: {kongKeySetting != null}, HasValue: {!string.IsNullOrEmpty(kongKey)}" });

                var vanityUrl = productInternalSettingList?.FirstOrDefault(p => p.Name == "Kong-Vanity-url");
                if (vanityUrl != null)
                {
                    kongVanityUrl = vanityUrl.Value;
                }
                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.Step", $"Step: Read Kong-Vanity-url, CompanyId: {companyId}, Found: {vanityUrl != null}, HasValue: {!string.IsNullOrEmpty(kongVanityUrl)}" });

                var companySettingsApiSetting = productInternalSettingList?.FirstOrDefault(a => a.Name.Equals("CompanyInternationalSettingsAPI", StringComparison.OrdinalIgnoreCase));
                string companyinternalSettingsAPI = companySettingsApiSetting?.Value ?? string.Empty;
                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.Step", $"Step: Read CompanyInternationalSettingsAPI, CompanyId: {companyId}, Found: {companySettingsApiSetting != null}, HasValue: {!string.IsNullOrEmpty(companyinternalSettingsAPI)}" });

                if (!string.IsNullOrEmpty(kongUri) && !string.IsNullOrEmpty(kongKey) && !string.IsNullOrEmpty(companyinternalSettingsAPI))
                {
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.Step", $"Step: Configure HttpClient, CompanyId: {companyId}, BaseAddressAlreadySet: {_httpClient.BaseAddress != null}" });
                    if (_httpClient.BaseAddress is null)
                    {
                        _httpClient.BaseAddress = new Uri(kongUri);
                        _httpClient.DefaultRequestHeaders.Add("apikey", kongKey);
                        if (!string.IsNullOrEmpty(kongVanityUrl))
                        {
                            _httpClient.DefaultRequestHeaders.Add("vanity-host", kongVanityUrl);
                        }
                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.Step.Result", $"Step: Configure HttpClient, CompanyId: {companyId}, BaseAddress: {_httpClient.BaseAddress}, VanityHostHeaderSet: {!string.IsNullOrEmpty(kongVanityUrl)}" });
                    }

                    string uri = string.Format(companyinternalSettingsAPI, source, companyId, settingType);
                    string fullUri = _httpClient.BaseAddress + uri;
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.Step", $"Step: BuildUri, CompanyId: {companyId}, RelativeUri: {uri}, FullUri: {fullUri}" });
                    uri = fullUri;

                    var logData = new Dictionary<string, object>() { { "Uri", _httpClient.BaseAddress + uri } };
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, messageProperties: new object[] { "GetCompanySettings via Kong", "Getting info" });
                    var options = new System.Text.Json.JsonSerializerOptions
					{
                        PropertyNameCaseInsensitive = true
                    };

                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.Step", $"Step: BeforeHttpGet, CompanyId: {companyId}, Uri: {uri}" });
                    HttpResponseMessage response;
                    try
                    {
                        response = _httpClient.GetAsync(uri).Result;
                    }
                    catch (Exception httpEx)
                    {
                        WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", correlationId, null, httpEx, messageProperties: new object[] { "GetCompanyInternalSettings.HttpException", $"HTTP GET threw exception. CompanyId: {companyId}, Uri: {uri}, ExceptionType: {httpEx.GetType().FullName}, Message: {httpEx.Message}" });
                        throw;
                    }
                    WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.Step.Result", $"Step: HttpGet, CompanyId: {companyId}, StatusCode: {(int)response.StatusCode} {response.StatusCode}, IsSuccess: {response.IsSuccessStatusCode}, ReasonPhrase: {response.ReasonPhrase}" });

                    if (response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                        {
                            WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.Exit", $"HTTP 204 NoContent. CompanyId: {companyId}, returning empty InternalSettingResponse." });
                            return internalSetting;
                        }
                        string responseBody = response.Content.ReadAsStringAsync().Result;
                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.Step.Result", $"Step: ReadResponseBody, CompanyId: {companyId}, BodyLength: {responseBody?.Length ?? 0}" });

                        if (!string.IsNullOrEmpty(responseBody))
                        {
                            try
                            {
                                internalSetting = System.Text.Json.JsonSerializer.Deserialize<InternalSettingResponse>(responseBody, options);
                                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.Step.Result", $"Step: Deserialize, CompanyId: {companyId}, ResultIsNull: {internalSetting == null}" });
                            }
                            catch (Exception deserEx)
                            {
                                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", correlationId, null, deserEx, messageProperties: new object[] { "GetCompanyInternalSettings.DeserializeException", $"JSON deserialize failed. CompanyId: {companyId}, BodyLength: {responseBody.Length}, ExceptionType: {deserEx.GetType().FullName}, Message: {deserEx.Message}" });
                                throw;
                            }
                        }
                        else
                        {
                            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, messageProperties: new object[] { "GetCompanySettings via Kong", "No content" });
                        }
                        logData = new Dictionary<string, object>() { { "InternationalCompanySetting", internalSetting } };
                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, messageProperties: new object[] { "GetCompanySettings via Kong", "Got info" });
                        WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.Exit", $"SUCCESS. CompanyId: {companyId}, Source: {source}, SettingType: {settingType}." });
                        return internalSetting;
                    }
                    logData = new Dictionary<string, object>() { { "response", response } };
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, messageProperties: new object[] { "GetCompanySettings via Kong", "No info found" });
                    WriteToLog(LogEventLevel.Warning, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.NonSuccess", $"Non-success status. CompanyId: {companyId}, StatusCode: {(int)response.StatusCode} {response.StatusCode}, ReasonPhrase: {response.ReasonPhrase}" });
                }
                else
                {
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanySettings via Kong", "KongApiEndPoint/KONG_KEY/CompanyInternationalSettingsAPI not found in database" });
                    WriteToLog(LogEventLevel.Warning, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.EarlyExit", $"Missing required settings. CompanyId: {companyId}, KongUriPresent: {!string.IsNullOrEmpty(kongUri)}, KongKeyPresent: {!string.IsNullOrEmpty(kongKey)}, CompanySettingsApiPresent: {!string.IsNullOrEmpty(companyinternalSettingsAPI)}" });
                }
            }
            catch (Exception ex)
            {
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", correlationId, null, ex, messageProperties: new object[] { "GetCompanyInternalSettings.Exception", $"Unhandled exception. CompanyId: {companyId}, Source: {source}, SettingType: {settingType}, ExceptionType: {ex.GetType().FullName}, Message: {ex.Message}" });
                throw;
            }
            WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanyInternalSettings.Exit", $"EXIT (default path). CompanyId: {companyId}, Source: {source}, SettingType: {settingType}." });
            return internalSetting;
        }

        #endregion
        #endregion

        #region Private Methods

        /// <summary>
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        /// <param name="correlationId">Correlation Id</param>
        private void WriteToLog(LogEventLevel logType, string message, Guid correlationId, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            var productInternalSettingList = GetProductInternalSettingList();
            string logSettings = null;
            if (productInternalSettingList != null)
            {
                logSettings = productInternalSettingList.FirstOrDefault(p => p.Name.Equals("Elk_LogManageUnifiedSettings", StringComparison.OrdinalIgnoreCase))?.Value;
            }

            if (logSettings != "1" && exception == null) return;

            var logger = Log.Logger;
            if (logData?.Keys != null)
            {
                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
            }

            logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", correlationId.ToString());

            logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
        }

        private void GetConfigurationSetting()
        {
            if (_httpClient.BaseAddress != null)
            {
                return;
            }
            string bbUri = "";

            var productInternalSettingList = GetProductInternalSettingList();

            bbUri = productInternalSettingList.First(a => a.Name.Equals("SettingsApiEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
            //useDomains = GetBooleanProductSettings("BooksUseDomains");
            //useUPFMId = GetBooleanProductSettings("BooksUseUPFMId");
            //useTranslatev2 = GetBooleanProductSettings("BooksUseTranslatev2");
            _httpClient.BaseAddress = new Uri(bbUri);
        }

        private List<ProductInternalSetting> GetProductInternalSettingList()
        {
            var rpcache = new RPObjectCache();
            var cacheKey = $"productInternalSetting_{(int)ProductEnum.UnifiedPlatform}";
            return rpcache.GetFromCache(cacheKey, 120, () => _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform));
        }

        #endregion
    }
}
