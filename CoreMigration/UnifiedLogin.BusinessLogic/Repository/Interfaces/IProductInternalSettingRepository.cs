using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
    /// <summary>
    /// Used to get the internal settings for a product
    /// </summary>
    public interface IProductInternalSettingRepository
    {
        /// <summary>
        /// Get the product settings by product id
        /// </summary>
        /// <param name="productId">ProductId</param>
        /// <returns>list product settings</returns>
        List<ProductInternalSetting> GetProductInternalSettings(int productId);

        /// <summary>
        /// Used to get all internal settings by product setting type
        /// </summary>
        /// <param name="productSettingType"></param>
        /// <returns></returns>
        IList<ProductInternalSettingByType> GetProductSettingByType(string productSettingType);

        /// <summary>
        /// Used to link a product setting to a given configuration
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="productInternalSetting"></param>
        /// <returns></returns>
        RepositoryResponse CreateProductSettingAndLinkToConfiguration(int productId, ProductInternalSetting productInternalSetting);
    }
}