using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Product
{
    /// <summary>
    /// Factory implementation for creating ManageUPFMProductsIntegration instances
    /// </summary>
    public class ManageUPFMProductsIntegrationFactory : IManageUPFMProductsIntegrationFactory
    {
        private readonly DefaultUserClaim _userClaim;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userClaim">User claims from the current request context</param>
        public ManageUPFMProductsIntegrationFactory(DefaultUserClaim userClaim)
        {
            _userClaim = userClaim ?? throw new ArgumentNullException(nameof(userClaim));
        }

        /// <summary>
        /// Creates a new instance of IManageUPFMProductsIntegration for the specified product
        /// </summary>
        /// <param name="productId">The product ID to create the integration for</param>
        /// <returns>An instance of IManageUPFMProductsIntegration</returns>
        public IManageUPFMProductsIntegration Create(int productId)
        {
            return new ManageUPFMProductsIntegration(productId, _userClaim);
        }
    }
}