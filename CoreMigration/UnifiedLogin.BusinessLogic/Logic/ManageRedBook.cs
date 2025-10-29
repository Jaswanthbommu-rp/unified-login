using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.IdentityConfig;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace UnifiedLogin.BusinessLogic.Logic
{
    /// <summary>
    /// Used to Get Company Custom Fields by calling RedBook API
    /// </summary>
    public class ManageRedBook : IDisposable, IManageRedBook
    {
        #region Constants and Variables
        const string MediaTypeName = "application/vnd.api+json";
        const int CacheTimeSeconds = 300;
        const int AuthTokenRefreshMinutes = 50;
        const int LandingProductID = 3;

        const int MAXRETRYCOUNT = 5;
        private string _accessToken;

        readonly HttpClient _httpClient;
        readonly List<ProductInternalSetting> productInternalSettingList;
        readonly IProductInternalSettingRepository _productInternalSettingRepository;

        readonly AuthTokenData _authTokenInfo = new AuthTokenData();

        ObjectCache _manageRedBookCache = MemoryCache.Default;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public ManageRedBook(string gbtoken)
        {
            string rbUri = "";
            #region GetSettings
            productInternalSettingList = _manageRedBookCache["productInternalSetting_" + LandingProductID.ToString()] as List<ProductInternalSetting>;
            if (productInternalSettingList == null)
            {
                _productInternalSettingRepository = new ProductInternalSettingRepository();
                productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings(LandingProductID);
                CacheItemPolicy policy = new CacheItemPolicy();
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(CacheTimeSeconds);
                _manageRedBookCache.Set("productInternalSetting_" + LandingProductID.ToString(), productInternalSettingList, policy);
            }
            #endregion
            _accessToken = gbtoken;
            rbUri = productInternalSettingList.First(a => a.Name.ToUpper() == "REDBOOKAPIENDPOINT").Value;
            //rbUri = "https://settings-dev.corp.realpage.com/";
            _httpClient = new HttpClient { BaseAddress = new Uri(rbUri) };
        }
        #endregion

        #region Public Methods
        #endregion

        #region Privates

        /// <summary>
        /// Used to set the required token for the books api call and then call the service
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> GetAsync(string uri)
        {
            bool doneProcessing = false;
            int failedCount = 0;

            HttpResponseMessage response = new HttpResponseMessage();

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

            while (!doneProcessing)
            {
                response = _httpClient.GetAsync(uri).Result;
                doneProcessing = response.IsSuccessStatusCode;
                if (!doneProcessing)
                {
                    if (!(response.StatusCode == HttpStatusCode.Unauthorized))
                    {
                        doneProcessing = true;
                    }
                    else
                    {
                        failedCount += 1;
                    }

                    if (failedCount >= MAXRETRYCOUNT)
                    {
                        doneProcessing = true;
                    }
                }
            }
            return response;
        }


        #endregion

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _httpClient.Dispose();
        }

        /// <summary>
        /// Used to get an auth token for the books
        /// </summary>
        private class AuthTokenData
        {
            [JsonProperty("data")]
            public AuthToken Data { get; set; }

            public AuthTokenData()
            {
                Data = new AuthToken();
            }
        }

        /// <summary>
        /// Used to get an auth token for the books
        /// </summary>
        private class AuthToken
        {
            /// <summary>
            /// The email to use for the token
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; set; }
            /// <summary>
            /// The password to use for the token
            /// </summary>
            [JsonProperty("password")]
            public string Password { get; set; }
        }

        /// <summary>
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            var logger = Log.Logger;
            if (logData?.Keys != null)
            {
                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
            }
			logger = logger.ForContext("ProductModule", this.GetType());

            logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
        }
    }
}