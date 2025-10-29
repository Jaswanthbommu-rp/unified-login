using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IdentityModel.Client;
using UnifiedLogin.DataAccess.Component;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.DataAccess;

namespace UnifiedLogin.BusinessLogic.Logic.Enterprise.Helpers
{
    public interface ITokenHelper
    {
        /// <summary>
        /// Used to get a Unified Login client credential token
        /// </summary>
        /// <param name="scopes">The scope required for the token</param>
        /// <returns></returns>
        string GetUnifiedLoginServerToken(string scopes);


        /// <summary>
        /// Used to get an Identity Server token with the requested scopes for the given client and secret
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        string GetClientCredentialServerToken(string clientId, string clientSecret, string scopes);

        /// <summary>
        /// Used to get a client token from an external identity server
        /// </summary>
        /// <param name="tokenUri"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        string GetExternalClientCredentialServerToken(string tokenUri, string clientId, string clientSecret, string scopes);
    }

    public class TokenHelper : ITokenHelper
    {
        private readonly IProductInternalSettingRepository _productRepository;

        public TokenHelper()
        {
            _productRepository = new ProductInternalSettingRepository();
        }

        public TokenHelper(IRepository repository)
        {
            _productRepository = new ProductInternalSettingRepository(repository);
        }

        /// <summary>
        /// Used to get a Unified Login client credential token
        /// </summary>
        /// <param name="scopes">The scope required for the token</param>
        /// <returns></returns>
        public string GetUnifiedLoginServerToken(string scopes)
        {
            var productInternalSettingList = GetProductInternalSettings(ProductEnum.UnifiedPlatform);
            try
            {
                string tokenEndPoint = productInternalSettingList.First(a => a.Name.Equals("TokenEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
                string clientId = productInternalSettingList.First(a => a.Name.Equals("UnifiedLoginServerClientName", StringComparison.OrdinalIgnoreCase)).Value;
                string apiSecret = Encoding.UTF8.GetString(Convert.FromBase64String(productInternalSettingList.First(a => a.Name.Equals("UnifiedLoginServerClientSecret", StringComparison.OrdinalIgnoreCase)).Value));

                RPObjectCache rpCache = new RPObjectCache();
                var cacheKey = $"GetUnifiedLoginServerToken_{clientId}_{scopes}";

                string accessToken = rpCache.GetFromCache<string>(cacheKey, 300, () =>
                {
                    TokenClient tokenClient = new TokenClient(tokenEndPoint, clientId, apiSecret);

                    var tokenResponse = tokenClient.RequestClientCredentialsAsync(scopes).Result;

                    if (tokenResponse.IsError)
                    {
                        throw new Exception($"TokenHelper.GetUnifiedLoginServerToken - Received null or empty token. {tokenResponse.Error}");
                    }

                    return tokenResponse.AccessToken;
                });

                return accessToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in TokenHelper.GetUnifiedLoginServerToken- {ex.Message}");
            }
        }

        /// <summary>
        /// Used to get an Identity Server token with the requested scopes for the given client and secret
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        public string GetClientCredentialServerToken(string clientId, string clientSecret, string scopes)
        {
            try
            {
                string issuerUri = ConfigReader.GetIssuerUri;
                RPObjectCache rpCache = new RPObjectCache();
                var cacheKey = $"GetClientCredentialServerToken_{issuerUri}_{clientId}_{scopes}";

                string accessToken = rpCache.GetFromCache<string>(cacheKey, 300, () =>
                {
                    TokenClient tokenClient = new TokenClient($"{issuerUri}/connect/token", clientId, clientSecret);

                    var tokenResponse = tokenClient.RequestClientCredentialsAsync(scopes).Result;

                    if (tokenResponse.IsError)
                    {
                        throw new Exception($"TokenHelper.GetClientCredentialServerToken - Received null or empty token. {tokenResponse.Error}");
                    }

                    return tokenResponse.AccessToken;
                });

                return accessToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in TokenHelper.GetClientCredentialServerToken- {ex.Message}");
            }
        }

        /// <summary>
        /// Used to get a client token from an external identity server
        /// </summary>
        /// <param name="tokenUri"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        public string GetExternalClientCredentialServerToken(string tokenUri, string clientId, string clientSecret, string scopes)
        {
            try
            {
                RPObjectCache rpCache = new RPObjectCache();
                var issuerHash = tokenUri.GetHashCode();
                var cacheKey = $"GetExternalClientCredentialServerToken_{issuerHash}_{clientId}_{scopes}";

                string accessToken = rpCache.GetFromCache<string>(cacheKey, 300, () =>
                {
                    TokenClient tokenClient = new TokenClient($"{tokenUri}", clientId, clientSecret);

                    var tokenResponse = tokenClient.RequestClientCredentialsAsync(scopes).Result;

                    if (tokenResponse.IsError)
                    {
                        throw new Exception($"TokenHelper.GetClientCredentialServerToken - Received null or empty token. {tokenResponse.Error}");
                    }

                    return tokenResponse.AccessToken;
                });

                return accessToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in TokenHelper.GetClientCredentialServerToken- {ex.Message}");
            }
        }

        private List<ProductInternalSetting> GetProductInternalSettings(ProductEnum product)
        {
            var rpcache = new RPObjectCache();
            var cacheKey = $"productInternalSetting_{(int)product}";
            var productInternalSettingList = rpcache.GetFromCache(cacheKey, 120, () =>
            {
                // load from database
                return _productRepository.GetProductInternalSettings((int)product).ToList();
            });

            return productInternalSettingList;
        }
    }
}
