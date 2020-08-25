using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IdentityModel.Client;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Enterprise.Helpers
{
    public interface ITokenHelper
    {
        /// <summary>
        /// Used to get a Unified Login client credential token
        /// </summary>
        /// <param name="scopes">The scope required for the token</param>
        /// <returns></returns>
        string GetUnifiedLoginServerToken(string scopes);

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
                string issueUri = ConfigReader.GetIssuerUri;
                string clientId = productInternalSettingList.First(a => a.Name.Equals("UnifiedLoginServerClientName", StringComparison.OrdinalIgnoreCase)).Value;
                string apiSecret = Encoding.UTF8.GetString(Convert.FromBase64String(productInternalSettingList.First(a => a.Name.Equals("UnifiedLoginServerClientSecret", StringComparison.OrdinalIgnoreCase)).Value));

                RPObjectCache rpCache = new RPObjectCache();
                var cacheKey = $"GetToken_{clientId}_{scopes}";

                string accessToken = rpCache.GetFromCache<string>(cacheKey, 300, () =>
                {
                    TokenClient tokenClient = new TokenClient($"{issueUri}/connect/token", clientId, apiSecret);

                    var tokenResponse = tokenClient.RequestClientCredentialsAsync(scopes).Result;

                    if (tokenResponse.IsError)
                    {
                        throw new Exception($"TokenHelper.GetToken - Received null or empty token. {tokenResponse.Error}");
                    }

                    return tokenResponse.AccessToken;
                });

                return accessToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in ManageProductRum.GetToken- {ex.Message}");
            }
        }

        private IList<ProductInternalSetting> GetProductInternalSettings(ProductEnum product)
        {
            var rpcache = new RPObjectCache();
            var cacheKey = $"productInternalSetting_{(int)product}";
            IList<ProductInternalSetting> productInternalSettingList = rpcache.GetFromCache<IList<ProductInternalSetting>>(cacheKey, 600, () =>
            {
                // load from database
                return _productRepository.GetProductInternalSettings((int)product).ToList();
            });

            return productInternalSettingList;
        }
    }
}
