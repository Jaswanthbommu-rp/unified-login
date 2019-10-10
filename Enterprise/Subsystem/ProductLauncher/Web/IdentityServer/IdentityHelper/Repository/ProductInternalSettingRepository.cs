using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Enum;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository
{
    public class ProductInternalSettingRepository : BaseRepository
    {
        #region Ctor
        /// <summary>
        /// SAML base Constructor
        /// </summary>
        public ProductInternalSettingRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }
        #endregion

        /// <summary>
        /// Get the product settings by product id
        /// </summary>
        /// <param name="ProductId">ProductId</param>
        /// <returns>list product settings</returns>
        public IEnumerable<ProductInternalSetting> GetProductSettings(int ProductId)
        {
            using (var repo = GetRepository())
            {
                return repo.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, new { ProductId });
            }
        }
    }
}