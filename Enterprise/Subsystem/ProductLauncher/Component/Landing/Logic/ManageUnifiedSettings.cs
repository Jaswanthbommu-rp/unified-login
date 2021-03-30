using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Enterprise.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Reflection;
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
            var unifiedSettings = rpCache.GetFromCache<IList<Setting>>(cacheKey, 30, () =>
            {
                return GetUnifiedSettingsData(category, partyId);
            });
            return unifiedSettings;
        }


        public ISettingResponse GetUnifiedSettings(string category, long partyId)
        {
            ISettingResponse settingResponse = new SettingResponse();
            Guid correlationId = Guid.NewGuid();
            Dictionary<string, object> logData = new Dictionary<string, object>
            {
                { "Get UnifiedSettings", $"Organization Id: {partyId}, Category: {category}" }
            };
            WriteToLog(LogEventLevel.Debug, "GetUnifiedSettings: Begin", correlationId, logData, null);

            if (partyId == 0)
            {
                throw new Exception("Missing Organization Id.");
            }
            try
            {
                switch (category.ToUpper())
                {
                    case "SECURITY":
                        {
                            var settings = GetUnifiedSettingsData(category, partyId);

                            if (settings != null && settings.Count > 0)
                            {
                                if (settingResponse.Keys == null)
                                {
                                    settingResponse.Keys = new List<ISetting>();
                                }

                                settingResponse.Keys.AddRange(settings);
                            }

                            break;
                        }
                    case "CUSTOMFIELDS":
                        {
                            List<ISettingRow> rows = GetTableSettings(partyId, category);
                            ISettingTable tableSetting = new SettingTable()
                            {
                                name = category,
                                editable = true,
                                hidden = false,                                
                                Value = rows
                            };

                            if (settingResponse.Tables == null)
                            {
                                settingResponse.Tables = new List<ISettingTable>();
                            }

                            settingResponse.Tables.Add(tableSetting);
                            break;
					    }
                }
            }
            catch (Exception exception)
            {
                logData = new Dictionary<string, object>
                {
                    { "Get Unified Settings: Data", "Exception" }
                };
                WriteToLog(LogEventLevel.Error, "unfiedSettingList: Exception", correlationId, logData, exception);
            }

            logData = new Dictionary<string, object>
            {
                { "Get UnifiedSettings: Data", settingResponse }
            };
            WriteToLog(LogEventLevel.Debug, "unfiedSettingList: End", correlationId, logData, null);

            return settingResponse;

        }
          

        public IList<Picklist> GetSettingsPickList(string category)
        {
            IList<Picklist> picklists = new List<Picklist>();

            Guid correlationId = Guid.NewGuid();
            Dictionary<string, object> logData = new Dictionary<string, object>
            {
                { "Get SettingsPickList",$"Category: {category}"}
            };
            WriteToLog(LogEventLevel.Debug, "GetSettingsPickList: Begin", correlationId, logData, null);

            try
            {
                picklists = _unifiedSettingsRepository.GetSettingsPickList(category);
            }
            catch (Exception exception)
            {
                var a = exception.ToString();
            }

            return picklists;
        }

        /// <summary>
        /// Update an existing unified Settings 
        /// </summary>
        /// <param name="settings">Security Settings (Password and Activity Configuration Security Settings) object of the parameter values</param>
        /// <param name="category">category</param>
        /// <param name="includes">includes</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse UpdateUnifiedSettings(IList<Setting> settings, string category, long partyId, string[] includes)
        {
            RepositoryResponse repositoryResponse = new RepositoryResponse();
            Guid correlationId = Guid.NewGuid();
            Dictionary<string, object> logData = new Dictionary<string, object>
            {
                { "Update SecuritySettings", $"Organization Id: {partyId}, category: {category}, Settings: {settings}" }
            };
            WriteToLog(LogEventLevel.Debug, "UpdateUnifiedSettings: Begin", correlationId, logData, null);
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings), "Null  Settings ");
            }

            try
            {
                repositoryResponse = _unifiedSettingsRepository.UpdateUnifiedSettings(settings, partyId, category, _userClaim.UserId);
            }
            catch (Exception exception)
            {
                logData = new Dictionary<string, object>
                {
                    { "Update UnifiedSettings", "Exception" }
                };
                WriteToLog(LogEventLevel.Debug, "UpdateUnifiedSettings: Exception", correlationId, logData, exception);
            }

            logData = new Dictionary<string, object>
            {
                { "Update UnifiedSettings", settings }
            };
            WriteToLog(LogEventLevel.Debug, "UpdateUnifiedSettings: End", correlationId, logData, null);

            return repositoryResponse;
        }

        public RepositoryResponse SaveTableSettings(   long partyId,
                                                       string category,
                                                       string operation,
                                                       List<SettingRow> rows)
        {

            RepositoryResponse repositoryResponse = new RepositoryResponse();
            Guid correlationId = Guid.NewGuid();
            string operationName = $"Update {category} Settings";
            Dictionary<string, object> logData = new Dictionary<string, object>
            {
                { operationName, $"Organization Id: {partyId}, category: {category}, Settings: {rows}" }
            };
            WriteToLog(LogEventLevel.Debug, operationName + ": Begin", correlationId, logData, null);
            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows), "Null  Settings ");
            }

            try
            {
                DataTable dataTable = new DataTable(category);
                string json = "";
                dataTable.Columns.Add("OrganizationId");
                foreach (Setting col in rows[0].Columns)
                {
                    dataTable.Columns.Add(col.Name.Trim());
                }

                foreach (SettingRow row in rows)
                {
                    DataRow dataRow = dataTable.NewRow();

                    foreach (Setting col in row.Columns)
                    {
                        dataRow[col.Name.Trim()] = col.Value;
                    }
                    dataRow["OrganizationId"] = partyId;
                    dataTable.Rows.Add(dataRow);

                    json = JsonConvert.SerializeObject(dataTable);
                }
                repositoryResponse = _unifiedSettingsRepository.AddUpdateCustomFields(json,  partyId, operation, _userClaim.UserId);
            }
            catch (Exception exception)
            {
                logData = new Dictionary<string, object>
                {
                    { "Update UnifiedSettings", "Exception" }
                };
                WriteToLog(LogEventLevel.Debug, operationName + ": Exception", correlationId, logData, exception);
            }

            logData = new Dictionary<string, object>
            {
                { operationName, rows }
            };
            WriteToLog(LogEventLevel.Debug, operationName + ": End", correlationId, logData, null);

            return repositoryResponse;
            

        }

            #region Send Property to Settings

            /// <summary>
            ///Send Property Instance to Unified settings to add/update property
            /// </summary>
            /// <param name="upfmProperties"></param>
            /// <param name="requestType"></param>
            /// <returns></returns>
            public bool CreateUpdatePropertyInSetting(UnifiedSettingPropertyPayload upfmProperties, HttpMethod requestType)
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
            WriteToLog(LogEventLevel.Debug, "CreatePropertyInSetting - Adding info.", correlationId, logData);

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
		/// <param name="propertyInstance">UPFM PropertyInstance</param>
		/// <returns></returns>
		public bool DeletePropertyInSetting(Guid propertyInstance)
        {
            GetConfigurationSetting();
            string ulClientToken = string.Empty;
            if (!_ignoreUnitTest)
            {
                ulClientToken = _tokenHelper.GetUnifiedLoginServerToken("unifiedsettingsapi");
            }
            Guid correlationId = Guid.NewGuid();
            //https://settingsapi-dev.realpage.com/v2/provisioning/property/{propertyId}         
            string uri = $"v2/provisioning/property/{propertyInstance}";
            Dictionary<string, object> logData = new Dictionary<string, object>() { { "uri", _httpClient.BaseAddress + uri }, { "propertyInstance", propertyInstance } };
            WriteToLog(LogEventLevel.Debug, "CreatePropertyInSetting - Adding info.", correlationId, logData);           
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

        #endregion
        #endregion

        #region Private Methods
        /// <summary>
        /// Used to write to the log
        /// </summary>
        /// <param name="logType">logType</param>
        /// <param name="message">message</param>
        /// <param name="logData">logData</param>
        /// <param name="exception">exception</param>
        /// <param name="correlationId">correlationId</param>
        private void WriteToLog(LogEventLevel logType, string message, Guid correlationId, Dictionary<string, object> logData = null, Exception exception = null)
        {
            var logger = Log.Logger;
            if (logData?.Keys != null)
            {
                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
            }
            logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", correlationId.ToString());
            logger.Write(logType, exception, message);
        }

        private void GetConfigurationSetting()
        {
            string bbUri = "";

            #region GetSettings
            IList<ProductInternalSetting> productInternalSettingList;
            productInternalSettingList = _manageSettingCache["productInternalSetting_" + (int)ProductEnum.UnifiedPlatform] as List<ProductInternalSetting>;
            if (productInternalSettingList == null)
            {
                productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);
                CacheItemPolicy policy = new CacheItemPolicy();
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(CacheTimeSeconds);
                _manageSettingCache.Set("productInternalSetting_" + (int)ProductEnum.UnifiedPlatform, productInternalSettingList, policy);
            }

            #endregion

            bbUri = productInternalSettingList.First(a => a.Name.Equals("SettingsApiEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
            //useDomains = GetBooleanProductSettings("BooksUseDomains");
            //useUPFMId = GetBooleanProductSettings("BooksUseUPFMId");
            //useTranslatev2 = GetBooleanProductSettings("BooksUseTranslatev2");
            _httpClient.BaseAddress = new Uri(bbUri);
        }

        private List<ISettingRow> GetTableSettings (long partyId, string category)
        {
            List<ISettingRow> rows = new List<ISettingRow>();
            RepositoryResponse repositoryResponse = new RepositoryResponse();

            var customFields = _unifiedSettingsRepository.GetUnifiedSettingsCustomFields(partyId);

            foreach (var field in customFields)
            {
                ISettingRow row = CreateSettingRow(field);
                rows.Add(row);
            }

            return rows;
        }

        #region "CreateSettingRow"
        private ISettingRow CreateSettingRow(CustomField field)
        {
            ISettingRow settingRow = new SettingRow()
            {
                Editable = true,
                Deletable = true,
                Selectable = true
            };

            settingRow.Columns = new List<Setting>();

            List<ISettingColumn> settingColumns = new List<ISettingColumn>();

            Type classType = field.GetType();
            PropertyInfo[] properties = classType.GetProperties();

            foreach (PropertyInfo pi in field.GetType().GetProperties())
            {
                string columnName = pi.Name;
                object columnValue = pi.GetValue(field, null)?.ToString();

                ISettingColumn settingColumn = new SettingColumn()
                {
                    Name = columnName,
                    Value = columnValue == null ? string.Empty : columnValue.ToString()
                };
                settingColumns.Add(settingColumn);
            }


            foreach (SettingColumn settingColumn in settingColumns)
            {
                string columnName = settingColumn.Name;
                object columnValue = settingColumn.Value;

                Setting columnSetting = new Setting()
                {
                    Name = columnName,
                    Value = columnValue.ToString(),
                    Editable = true
                };

                settingRow.Columns.Add(columnSetting);
            }            

            return settingRow;
        }
        #endregion "CreateSettingRow"
        private IList<Setting> GetUnifiedSettingsData(string category, long partyId)
        {
            IList<Setting> unfiedSettingList = new List<Setting>();
           
            unfiedSettingList = _unifiedSettingsRepository.GetUnifiedSettings(partyId, category);           

            return unfiedSettingList;
        }
        #endregion
    }
}
