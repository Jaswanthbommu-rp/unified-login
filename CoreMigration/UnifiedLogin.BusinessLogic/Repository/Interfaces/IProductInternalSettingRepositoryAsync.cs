using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
    /// <summary>
    /// Used to get the internal settings for a product
    /// </summary>
    public interface IProductInternalSettingRepositoryAsync
    {
        /// <summary>
        /// Get the product settings by product id
        /// </summary>
        /// <param name="productId">ProductId</param>
        /// <returns>list product settings</returns>
        Task<IList<ProductInternalSetting>> GetProductInternalSettingsAsync(int productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Used to get all internal settings by product setting type
        /// </summary>
        /// <param name="productSettingType"></param>
        /// <returns></returns>
        Task<IList<ProductInternalSettingByType>> GetProductSettingByTypeAsync(string productSettingType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Used to link a product setting to a given configuration
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="productInternalSetting"></param>
        /// <returns></returns>
        Task<RepositoryResponse> CreateProductSettingAndLinkToConfigurationAsync(int productId, ProductInternalSetting productInternalSetting, CancellationToken cancellationToken = default);
    }
}