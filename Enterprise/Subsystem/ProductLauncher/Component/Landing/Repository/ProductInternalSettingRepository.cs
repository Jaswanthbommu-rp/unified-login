using System.Collections.Generic;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
	/// <summary>
	/// Used to get the internal settings for a product
	/// </summary>
	public class ProductInternalSettingRepository : BaseRepository, IProductInternalSettingRepository
    {
        #region Ctor
        /// <summary>
        /// SAML base Constructor
        /// </summary>
        public ProductInternalSettingRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        public ProductInternalSettingRepository(IRepository repository) : base(repository)
        {
        }
        #endregion

        /// <summary>
        /// Get the product settings by product id
        /// </summary>
        /// <param name="ProductId">ProductId</param>
        /// <returns>list product settings</returns>
        public IList<ProductInternalSetting> GetProductInternalSettings(int ProductId)
        {
            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = $"productinternalsettings_{(int)ProductId}";
            List<ProductInternalSetting> productInternalSettings = rpcache.GetFromCache<List<ProductInternalSetting>>(cacheKey, 300, () =>
            {
                using (var repo = GetRepository())
                {
                    dynamic param = new { ProductId = ProductId };
                    return repo.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, param);
                }
            });

            return productInternalSettings;
        }
    }
}