using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Enterprise.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Enterprise
{

	public class SettingsRepository : ISettingRepository
	{
		private DefaultUserClaim _userClaim;
		private HttpClient _httpClient;
		readonly ITokenHelper _tokenHelper;
		IProductInternalSettingRepository _productInternalSettingRepository;

		#region Ctor
		/// <summary>
		/// Ctor
		/// </summary>
		public SettingsRepository(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository){
			_userClaim = userClaim;
			_httpClient = new HttpClient();
			_productInternalSettingRepository = productInternalSettingRepository;
		}
        public SettingsRepository(DefaultUserClaim userClaim)
        {
            _userClaim = userClaim;
        }
        #endregion

        public SettingResponse GetCompanyInternationalSettings(string companyId, string source, string settingType)
		{
            SettingResponse internationalSetting = new SettingResponse();
            if (!string.IsNullOrEmpty(settingType))
            {
                string kongUri = string.Empty;
                string kongVanityUrl = string.Empty;
                var productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings(productId: (int)ProductEnum.UnifiedPlatform);
                kongUri = productInternalSettingList.First(a => a.Name.Equals("KongApiEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
                string kongKey = productInternalSettingList.First(a => a.Name.Equals("KONG_KEY", StringComparison.OrdinalIgnoreCase)).Value;
                var vanityUrl = productInternalSettingList.FirstOrDefault(p => p.Name == "Kong-Vanity-url");
                if (productInternalSettingList.FirstOrDefault(p => p.Name == "Kong-Vanity-url") != null)
                {
                    kongVanityUrl = productInternalSettingList.First(a => a.Name.Equals("Kong-Vanity-url", StringComparison.OrdinalIgnoreCase)).Value;
                }
                string companyInternationalSettingsAPI = productInternalSettingList.First(a => a.Name.Equals("CompanyInternationalSettingsAPI", StringComparison.OrdinalIgnoreCase)).Value;
                if (!string.IsNullOrEmpty(kongUri) && !string.IsNullOrEmpty(kongKey) && !string.IsNullOrEmpty(companyInternationalSettingsAPI))
                {
                    _httpClient.BaseAddress = new Uri(kongUri);
                    string uri = string.Format(companyInternationalSettingsAPI, source, companyId, settingType);
                    uri = _httpClient.BaseAddress + uri;
                    var logData = new Dictionary<string, object>() { { "uri", _httpClient.BaseAddress + uri } };
                    WriteToLog(LogEventLevel.Debug, $"GetCompanySettings using Kong API - Getting info", logData);
                    //var options = new JsonSerializerOptions
                    //{
                    //    PropertyNameCaseInsensitive = true
                    //};
                    _httpClient = new HttpClient { BaseAddress = new Uri(kongUri) };
                    _httpClient.DefaultRequestHeaders.Add("apikey", kongKey);
                    if (!string.IsNullOrEmpty(kongVanityUrl))
                    {
                        _httpClient.DefaultRequestHeaders.Add("vanity-host", kongVanityUrl);
                    }
                    var response = _httpClient.GetAsync(uri).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        internationalSetting = JsonConvert.DeserializeObject<SettingResponse>(response.Content.ReadAsStringAsync().Result);
                        logData = new Dictionary<string, object>() { { "InternationalCompanySetting", internationalSetting } };
                        WriteToLog(LogEventLevel.Debug, $"GetCompanySettings using Kong API - Got info - ", logData);
                        return internationalSetting;
                    }
                    logData = new Dictionary<string, object>() { { "response", response } };
                    WriteToLog(LogEventLevel.Debug, $"GetCompanySettings using Kong API - No info found", logData);
                }
                else
                {
                    WriteToLog(LogEventLevel.Debug, $"GetCompanySettings using Kong API - KongApiEndPoint/KONG_KEY/CompanyInternationalSettingsAPI not found in database");
                }
            }
            return internationalSetting;
        }

        #region Private Methods
        /// <summary>
        /// Used to write to the log
        /// </summary>
        /// <param name="logType">Message log type</param>
        /// <param name="message">Message</param>
        /// <param name="logData">Additional log data</param>
        /// <param name="exception">Exception</param>
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null)
        {
            string correlationId = "";
            if (_userClaim != null)
            {
                correlationId = (_userClaim.CorrelationId != Guid.Empty) ? _userClaim.CorrelationId.ToString() : "";
            }

            var logger = Log.Logger;
            if (logData?.Keys != null)
            {
                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Newtonsoft.Json.Formatting.Indented), false);
            }
            logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", correlationId);
            logger.Write(logType, exception, message);
        }

        #endregion
    }
}
