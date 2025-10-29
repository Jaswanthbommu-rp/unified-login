using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
    /// <summary>
    /// Interface for ManageProduct
    /// </summary>
    public interface IManageProduct
    {
        /// <summary>
        /// Used to return a list of products an organization has with option to merge access to persona access
        /// </summary>
        /// <param name="realPageId">realPageId</param>
        /// <param name="personaId">personaId</param>
        /// <param name="allProducts">Return all product types</param>
        /// <param name="replaceProductCodeWithUDMIfExists">True when the product code being returned should be the UDM code if one exists, otherwise leave productcode alone</param>
        IList<ProductUI> GetProducts(Guid realPageId, long personaId = 0, bool allProducts = false, bool replaceProductCodeWithUDMIfExists = true);

        ///// <summary>
        ///// Used to return a list of productfamilies
        ///// </summary>
        ///// <returns></returns>
        //IList<ProductFamily> GetProductFamilies();

        ///// <summary>
        ///// Used to return a list of productsolutions 
        ///// </summary>
        ///// <returns></returns>
        //IList<ProductSolution> GetProductSolutions();

        /// <summary>
        /// Used to return a list of products user has access to, filterable by favorites and resouce only
        /// </summary>
        /// <param name="persona">persona</param>
        /// <param name="productSelectType">productSelectType</param>
        /// <param name="security">security</param>
        /// <returns></returns>
        IList<PersonaProductUserDetails> GetUserAssignedProductsByPersona(Persona persona, ProductSelectType? productSelectType = null, RouteSecurity security = null);

        /// <summary>
        /// Update a product setting of a persona
        /// </summary>
        /// <param name="productSetting">productSetting</param>
        /// <param name="personaId">personaId</param>
        /// <returns></returns>
        RepositoryResponse UpdateProductSetting(ProductSetting productSetting, long? personaId);

        /// <summary>
        /// Used to get internal settings for a product
        /// </summary>
        /// <param name="productId">The id of the product to get the settings for</param>
        /// <returns>The list of settings</returns>

        List<ProductInternalSetting> GetProductInternalSettings(int productId);

        /// <summary>
        /// Used to get all internal settings by product setting type
        /// </summary>
        /// <param name="productSettingType">The type of the product type to get the settings for</param>
        /// <returns>The list of settings</returns>
        IList<ProductInternalSettingByType> GetProductSettingByType(string productSettingType, string orgType = null);

        /// <summary>
        /// Used to return a list of productTypes
        /// </summary>
        /// <returns></returns>
        IList<ProductType> GetProductTypes();

        /// <summary>
        /// Used to return a list of productfamilies
        /// </summary>
        /// <param name="organizationRealPageId">The unique identitifier for the organization</param>
        /// <param name="realpageUserId"></param>
        /// <param name="personRealPageId">Edited User enterprise Id</param>
        /// <param name="accessFilter">Filter Products</param>
		/// <param name="loginName">User Login Name</param>
        /// <returns>List of Product Families</returns>
        IList<ProductFamily> GetProductFamilies(Guid organizationRealPageId, Guid realpageUserId, Guid? personRealPageId, string accessFilter = null, string loginName = null);

        /// <summary>
        /// List user(s)
        /// </summary>
        /// <param name="productId">Unique ProductId</param>
        /// <param name="blueBookCompanyInstanceId">Unique blueBook CompanyInstanceId</param>
        /// <param name="personaId">Unique PersonaId</param>
        /// <returns>ProductUsers Object</returns>
        IList<ProductUsers> GetProductUsers(int productId, long blueBookCompanyInstanceId, long personaId = 0);

        /// <summary>
        /// List GB products; if pass nothing then returns all products
        /// </summary>
        IList<GbProductMap> ListProducts(int? productId = null, Guid? productGuid = null, string name = null, string booksProductCode = null);

        /// <summary>
        /// Used to get a list of products for the given persona id
        /// </summary>
        /// <param name="personaId"></param>
        /// <param name="statusType"></param>
        /// <returns></returns>
        IList<PersonaProduct> GetAllProductsByPersona(long personaId, ProductBatchStatusType statusType);

        /// <summary>
        /// Used to add or update a product setting for the given configuration
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="productInternalSetting"></param>
        /// <returns></returns>
        RepositoryResponse CreateProductSettingAndLinkToConfiguration(int productId, ProductInternalSetting productInternalSetting);

        /// <summary>
        /// Returns a list of productSettingType
        /// </summary>
        /// <returns></returns>
        IList<ProductSettingType> ListProductSettingType();

        /// <summary>
        /// Add ProductSource And GreenBookCareFlag To Products
        /// </summary>
        /// <param name="upfmCompanyId"></param>
        /// <param name="organizationPartyId"></param>
        /// <param name="productUI"></param>
        /// <returns></returns>
        IList<ProductUI> AddProductSourceAndGreenBookCareFlagToProducts(Guid upfmCompanyId, long organizationPartyId, IList<ProductUI> productUI);

        /// <summary>
        /// Get ADGroups for the product
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        List<AdGroupProduct> GetAdGroupsForProduct(int productId);

        /// <summary>
        /// Get ADGroups for the user
        /// </summary>
        /// <param name="personaId"></param>
        /// <returns></returns>
        List<AdGroup> GetAdGroupsForUser(long personaId);
    }
}