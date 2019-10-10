using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces
{
    /// <summary>
    /// Used to get the internal settings for a product
    /// </summary>
    public interface IProductInternalSettingRepository
    {
        /// <summary>
        /// Get the product settings by product id
        /// </summary>
        /// <param name="ProductId">ProductId</param>
        /// <returns>list product settings</returns>
        IList<ProductInternalSetting> GetProductInternalSettings(int ProductId);
    }
}