using Newtonsoft.Json;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.Helpers;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;

namespace UnifiedLogin.BusinessLogic.Logic
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
            InternalSettingResponse internalSetting = new InternalSettingResponse();
            if (!string.IsNullOrEmpty(settingType))
            {
                Guid correlationId = Guid.NewGuid();
                string kongUri = string.Empty;
                string kongVanityUrl = string.Empty;
                var productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);
                kongUri = productInternalSettingList.First(a => a.Name.Equals("KongApiEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
                string kongKey = productInternalSettingList.First(a => a.Name.Equals("KONG_KEY", StringComparison.OrdinalIgnoreCase)).Value;
                var vanityUrl = productInternalSettingList.FirstOrDefault(p => p.Name == "Kong-Vanity-url");
                if (productInternalSettingList.FirstOrDefault(p => p.Name == "Kong-Vanity-url") != null)
                {
                    kongVanityUrl = productInternalSettingList.First(a => a.Name.Equals("Kong-Vanity-url", StringComparison.OrdinalIgnoreCase)).Value;
                }
                string companyinternalSettingsAPI = productInternalSettingList.First(a => a.Name.Equals("CompanyInternationalSettingsAPI", StringComparison.OrdinalIgnoreCase)).Value;
                if (!string.IsNullOrEmpty(kongUri) && !string.IsNullOrEmpty(kongKey) && !string.IsNullOrEmpty(companyinternalSettingsAPI))
                {
                    if (_httpClient.BaseAddress is null)
                    {
                        _httpClient.BaseAddress = new Uri(kongUri);
                        _httpClient.DefaultRequestHeaders.Add("apikey", kongKey);
                        if (!string.IsNullOrEmpty(kongVanityUrl))
                        {
                            _httpClient.DefaultRequestHeaders.Add("vanity-host", kongVanityUrl);
                        }
                    }
                    string uri = string.Format(companyinternalSettingsAPI, source, companyId, settingType);
                    uri = _httpClient.BaseAddress + uri;

                    var logData = new Dictionary<string, object>() { { "Uri", _httpClient.BaseAddress + uri } };
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, messageProperties: new object[] { "GetCompanySettings via Kong", "Getting info" });
                    var options = new System.Text.Json.JsonSerializerOptions
					{
                        PropertyNameCaseInsensitive = true
                    };
                   
                    var response = _httpClient.GetAsync(uri).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.NoContent) { return internalSetting; }
                        if (!string.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                        {
                            internalSetting = System.Text.Json.JsonSerializer.Deserialize<InternalSettingResponse>(response.Content.ReadAsStringAsync().Result, options);
                        }
                        else
                        {
                            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, messageProperties: new object[] { "GetCompanySettings via Kong", "No content" });
                        }
                        logData = new Dictionary<string, object>() { { "InternationalCompanySetting", internalSetting } };
                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, messageProperties: new object[] { "GetCompanySettings via Kong", "Got info" });
                        return internalSetting;
                    }
                    logData = new Dictionary<string, object>() { { "response", response } };
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, messageProperties: new object[] { "GetCompanySettings via Kong", "No info found" });
                }
                else
                {
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, null, messageProperties: new object[] { "GetCompanySettings via Kong", "KongApiEndPoint/KONG_KEY/CompanyInternationalSettingsAPI not found in database" });
                }
            }
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
