using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.Helpers;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Landing;
using Serilog;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;

namespace UnifiedLogin.BusinessLogic.Logic
{
    public class ManageMicrosoftAzure : IManageMicrosoftAzure
    {
        private readonly DefaultUserClaim _defaultUserClaim;
        private IProductInternalSettingRepository _productInternalSettingRepository;
        private HttpMessageHandler _httpGetMessageHandler = new HttpClientHandler();
        private HttpClient _httpClient;
        private readonly ITokenHelper _tokenHelper;
        private readonly string _azureTokenAddress;
        private readonly string _azureUnifiedLoginUserClientSecret;
        private readonly string _azureUnifiedLoginUserClientId;
        private readonly string _azureUnifiedLoginUserClientScopes;

        public ManageMicrosoftAzure(DefaultUserClaim defaultUserClaim)
        {
            _defaultUserClaim = defaultUserClaim;
            _productInternalSettingRepository = new ProductInternalSettingRepository();

            var productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings(3);

            _azureTokenAddress = productInternalSettingList.First(a => a.Name.Equals("AzureTokenAddress", StringComparison.OrdinalIgnoreCase)).Value;
            _azureUnifiedLoginUserClientSecret = productInternalSettingList.First(a => a.Name.Equals("AzureUnifiedLoginUserClientSecret", StringComparison.OrdinalIgnoreCase)).Value;
            _azureUnifiedLoginUserClientId = productInternalSettingList.First(a => a.Name.Equals("AzureUnifiedLoginUserClientId", StringComparison.OrdinalIgnoreCase)).Value;
            _azureUnifiedLoginUserClientScopes = productInternalSettingList.First(a => a.Name.Equals("AzureUnifiedLoginUserClientScopes", StringComparison.OrdinalIgnoreCase)).Value;

            var azureUserGraphAPI = productInternalSettingList.First(a => a.Name.Equals("AzureUserGraphAPI", StringComparison.OrdinalIgnoreCase)).Value;
            _tokenHelper = new TokenHelper();
            _httpClient = new HttpClient { BaseAddress = new Uri(azureUserGraphAPI) };
        }

        public ManageMicrosoftAzure(DefaultUserClaim defaultUserClaim, IRepository repository, HttpMessageHandler httpMessageHandler)
        {
            _defaultUserClaim = defaultUserClaim;
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);

            _tokenHelper = new TokenHelper(repository);
            _httpClient = new HttpClient(httpMessageHandler);
        }

        /// <summary>
        /// Used to get employee information for the given user name
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public AzureUser GetADUserInfo(string userName)
        {
            try
            {
                var token = _tokenHelper.GetExternalClientCredentialServerToken(_azureTokenAddress+"/token", _azureUnifiedLoginUserClientId, _azureUnifiedLoginUserClientSecret, _azureUnifiedLoginUserClientScopes);
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = _httpClient.GetAsync($"/v1.0/users?$filter = userPrincipalName eq '{userName}' &$select = userPrincipalName,onPremisesSamAccountName,displayName,mail,userPrincipalName,id").Result;
                var logger = Log.Logger;
                logger = logger.ForContext("ProductModule", this.GetType());
                logger.Write(Serilog.Events.LogEventLevel.Debug, "{ActionName} - {state}", propertyValue0: "GetADUserInfo", propertyValue1: $" url: /v1.0/users?$filter = userPrincipalName eq '{userName}' &$select = userPrincipalName,onPremisesSamAccountName,displayName,mail,userPrincipalName,id");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    var userInfo = JsonConvert.DeserializeObject<AzureUser>(responseContent);
                    if (userInfo != null && (userInfo?.value?.FirstOrDefault()?.userPrincipalName.Equals(userName, StringComparison.OrdinalIgnoreCase) ?? false))
                    {
                        return userInfo;
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = Log.Logger;
                logger = logger.ForContext("ProductModule", this.GetType());
                logger.Write(Serilog.Events.LogEventLevel.Error, ex, "{ActionName} - {state}", propertyValue0: "GetADUserInfo", propertyValue1: "Error when attempting to get Azure user info");
            }

            return null;
        }
    }
}
