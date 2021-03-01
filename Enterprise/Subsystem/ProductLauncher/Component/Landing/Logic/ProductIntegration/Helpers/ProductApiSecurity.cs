using IdentityModel.Client;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers
{
	/// <summary>
	/// Handles Product Api Security
	/// </summary>
	public class ProductApiSecurity
    {
        #region Private members & Ctor
        private readonly ProductEnum _productType;
        private readonly IList<ProductInternalSetting> _productIntegrationDetails;

        /// <summary>
        /// Ctor
        /// </summary>
        public ProductApiSecurity(ProductEnum productType, IList<ProductInternalSetting> productInternalSettingList)
        {
            _productType = productType;
            _productIntegrationDetails = productInternalSettingList;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Applies Security To HttpClient based on product
        /// </summary>
        public void ApplySecurityToHttpClient(HttpClient httpClient)
        {
            switch (_productType)
            {
                case ProductEnum.VendorServices:				
					UnityOAuthApiSecurity(httpClient);
					break;
				case ProductEnum.RenovationManager:
					RenoOAuthApiSecurity(httpClient);
                    break;
                case ProductEnum.LeadManagement: //ILM-LM
                case ProductEnum.LeadAnalytics: //ILM-LA
				case ProductEnum.DepositAlternative:
					BasicAuthApiSecurity(httpClient);
					break;
				case ProductEnum.PortfolioManagement:
					PortfolioManagementOAuthApiSecurity(httpClient);
					break;
	            
	            case ProductEnum.ClickPay:
		            ClickPayApiSecurity(httpClient);
		            break;
                case ProductEnum.SeniorLeadManagement:
                    SeniorLeadManagementApiSecurity(httpClient);
                    break;
            }
        }

        private void SeniorLeadManagementApiSecurity(HttpClient httpClient)
        {            
            string apiKey = _productIntegrationDetails.First(a => a.Name.ToUpper() == "APIKEY").Value;

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-ExternalClientId", apiKey);
        }

        private void ClickPayApiSecurity(HttpClient httpClient)
	    {
			string apiUser = _productIntegrationDetails.First(a => a.Name.ToUpper() == "APIUSERNAME").Value;
		    string apiPassword = _productIntegrationDetails.First(a => a.Name.ToUpper() == "APIPASSWORD").Value;
		    string apiKey = _productIntegrationDetails.First(a => a.Name.ToUpper() == "APIKEY").Value;

			httpClient.DefaultRequestHeaders.Clear();
		    httpClient.SetBasicAuthentication(apiUser, apiPassword);
		    httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
		}

	    private void BasicAuthApiSecurity(HttpClient httpClient)
	    {
		    string apiUser = _productIntegrationDetails.First(a => a.Name.ToUpper() == "APIUSERNAME").Value;
		    string apiPassword = _productIntegrationDetails.First(a => a.Name.ToUpper() == "APIPASSWORD").Value;

			httpClient.DefaultRequestHeaders.Clear();
		    httpClient.SetBasicAuthentication(apiUser, apiPassword);
		}	
		#endregion

		#region Private Methods - Portfolio Management
		/// <summary>
		/// Get product settings and setup the HTTP request and response
		/// </summary>
		/// <param name="httpClient"></param>
		private void PortfolioManagementOAuthApiSecurity(HttpClient httpClient)
		{
			string tokenClientId = _productIntegrationDetails.First(a => a.Name.ToUpper() == "TOKENCLIENTID").Value;
			string tokenClientSecret = _productIntegrationDetails.First(a => a.Name.ToUpper() == "TOKENCLIENTSECRET").Value;
			string tokenIssueUri = _productIntegrationDetails.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;

			string accessToken = GetPortfolioManagementAccessToken(tokenIssueUri, tokenClientId, tokenClientSecret);
			httpClient.DefaultRequestHeaders.Clear();
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
		}

		/// <summary>
		/// Get Portfolio Management AccessToken
		/// </summary>
		/// <param name="tokenIssueUri">Token Url</param>
		/// <param name="tokenClientId">Username</param>
		/// <param name="tokenClientSecret">Password</param>
		/// <returns>Access Token</returns>
		private string GetPortfolioManagementAccessToken(string tokenIssueUri, string tokenClientId, string tokenClientSecret)
		{
			string accessToken = string.Empty;
			try
			{
				HttpClient client = new HttpClient();
				client.SetBasicAuthentication(tokenClientId, tokenClientSecret);
				Dictionary<string, string> dictionary = new Dictionary<string, string>()
				{
					{
						"grant_type",
						"client_credentials"
					},
					{	"scope",
						""
					}
				};

				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, tokenIssueUri + "/token")
				{
					Content = new FormUrlEncodedContent(dictionary)
				};
				HttpResponseMessage postResponse = client.SendAsync(request).Result;
				if (postResponse.IsSuccessStatusCode)
				{
					dynamic resultObject = JsonConvert.DeserializeObject<dynamic>(postResponse.Content.ReadAsStringAsync().Result);
					accessToken = resultObject.access_token;
				}
				return accessToken;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in GetToken- {ex.Message}");
			}
		}
		#endregion

		#region Private Methods - Unity
		private void UnityOAuthApiSecurity(HttpClient httpClient)
        {
            var apiSecret = _productIntegrationDetails.First(a => a.Name.ToUpper() == "APISECRET").Value;
            var clientId = _productIntegrationDetails.First(a => a.Name.ToUpper() == "CLIENTID").Value;
            var tokenIssueUri = _productIntegrationDetails.First(a => a.Name.ToUpper() == "TOKENENDPOINT").Value;

            var token = GetToken(tokenIssueUri, clientId, apiSecret);
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

		private void RenoOAuthApiSecurity(HttpClient httpClient)
		{
			var apiSecret = _productIntegrationDetails.First(a => a.Name.ToUpper() == "APISECRET").Value;
			var clientId = _productIntegrationDetails.First(a => a.Name.ToUpper() == "CLIENTID").Value;
			var tokenIssueUri = _productIntegrationDetails.First(a => a.Name.ToUpper() == "TOKENENDPOINT").Value;

			var token = GetAccessToken(tokenIssueUri, clientId, apiSecret);
			httpClient.DefaultRequestHeaders.Clear();
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			//httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", "5b0fcd3596946df5fb88d45f68a5a9ecf85625a0");
		}
		private string GetToken(string tokenIssueUri, string clientId, string apiSecret)
        {
            try
            {
                //TODO: cache based on clientId
                var tokenClient = new TokenClient($"{tokenIssueUri}", clientId, apiSecret);

                var tokenResponse = tokenClient.RequestClientCredentialsAsync(clientId).Result;

                if (tokenResponse.IsError)
                {
                    throw new Exception($"Received null or empty token. {tokenResponse.Error}");
                }

                return tokenResponse.AccessToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetToken- {ex.Message}");
            }
        }
		private string GetAccessToken(string tokenIssueUri, string clientId, string apiSecret)
		{
			try
			{
				string scope = _productIntegrationDetails.First(a => a.Name.ToUpper() == "CLIENTSCOPE").Value;
				ObjectCache tokenCache = MemoryCache.Default;

				// Get token values from cache
				string accessToken = tokenCache[clientId] as string;				

				if (string.IsNullOrEmpty(accessToken))
				{
					var tokenClient = new TokenClient($"{tokenIssueUri}", clientId, apiSecret);
					var tokenResponse = tokenClient.RequestClientCredentialsAsync(scope).Result;

					if (tokenResponse.IsError)
					{
						throw new Exception($"Received null or empty token. {tokenResponse.Error}");
					}

					var cachePolicy = new CacheItemPolicy
					{
						// Expier cache every after 9 minutes (assuming 10 min is token expiration time)
						AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(9)
					};

					accessToken = tokenResponse.AccessToken;
					tokenCache.Set(clientId, accessToken, cachePolicy);					
				}
				return accessToken;
			}			
			catch (Exception ex)
			{
				throw new Exception($"Error in GetToken- {ex.Message}");
			}
		}
		#endregion
	}
}