namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
    /// <summary>
    /// Factory for creating IManageUPFMProductsIntegration instances with runtime product ID
    /// </summary>
    public interface IManageUPFMProductsIntegrationFactory
    {
        /// <summary>
        /// Creates a new instance of IManageUPFMProductsIntegration for the specified product
        /// </summary>
        /// <param name="productId">The product ID to create the integration for</param>
        /// <returns>An instance of IManageUPFMProductsIntegration</returns>
        IManageUPFMProductsIntegration Create(int productId);
    }
}
