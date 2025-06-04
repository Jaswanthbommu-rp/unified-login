using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.EnterpriseRole;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.Security;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using EnterpriseProductUser = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise.ProductUsers;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces
{
    /// <summary>
    /// Product Repository
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// Returns a list of products user has access to, filterable by favorites and resouce only
        /// </summary>
        /// <param name="persona">persona</param>   
        /// <param name="productSelectType">productSelectType</param>
        /// <param name="security">security</param>
        /// <returns></returns>
        IList<PersonaProductUserDetails> GetAssignedProductsByPersona(Persona persona, ProductSelectType? productSelectType = null, RouteSecurity security = null);

        /// <summary>
        /// Returns list of products that are resource type by filtering organization products
        /// </summary>
        /// <param name="organizationRealPageId">organizationRealPageId</param>
        /// <returns></returns>
        IList<ProductUI> GetProductsResourceType(Guid organizationRealPageId);

        /// <summary>
        /// Returns a list of all product settings that an organization has
        /// </summary>
        /// <param name="organizationRealPageId">organizationRealPageId</param>
        /// <param name="productId">The id of the product to be filtered. null returns all product settings</param>
        /// <returns></returns>
        IList<ProductSettingList> GetProductSettings(Guid organizationRealPageId, int productId);

        /// <summary>
        /// Get Product Settings
        /// </summary>
        /// <param name="organizationRealPageId"></param>
        /// <returns></returns>
        IList<ProductSettingList> GetProductSettings(Guid organizationRealPageId);

        /// <summary>
        /// Returns a list of all product settings for persona
        /// </summary>
        /// <param name="personaId">personaId</param>
        /// <returns></returns>
        IList<ProductSettingList> GetProductSettingsByPersona(long personaId);

        /// <summary>
        /// Returns a list of products that an organization has license using its organizationRealPageId
        /// </summary>
        /// <param name="organizationRealPageId">organizationRealPageId</param>
        /// <param name="personaId">personaId</param>
        /// <param name="resourceOnly">Only return resource type products</param>
        /// <param name="allProducts">Return all product types</param>
        /// <param name="replaceProductCodeWithUDMIfExists">True when the product code being returned should be the UDM code if one exists, otherwise leave productcode alone</param>
        /// <returns></returns>
        IList<ProductUI> GetProducts(Guid organizationRealPageId, long personaId, bool resourceOnly = false, bool allProducts = false, bool replaceProductCodeWithUDMIfExists = true);

        /// <summary>
        /// Returns a list of productTypes
        /// </summary>
        /// <returns></returns>
        IList<ProductType> GetProductTypes();

        /// <summary>
        /// Create ProductSetting
        /// </summary>
        /// <param name="PersonaId"></param>
        /// <param name="ProductId"></param>
        /// <param name="ProductSettingTypeId"></param>
        /// <param name="Value"></param>
        /// <returns>Repository response object</returns>
        RepositoryResponse CreateProductSetting(long PersonaId, int ProductId, int ProductSettingTypeId, string Value);

        /// <summary>
        /// Create ProductSetting
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="PersonaId"></param>
        /// <param name="ProductId"></param>
        /// <param name="ProductSettingTypeId"></param>
        /// <param name="Value"></param>
        /// <returns>Repository response object</returns>
        RepositoryResponse CreateProductSetting(IRepository repository, long PersonaId, int ProductId, int ProductSettingTypeId, string Value);

        /// <summary>
        /// Returns a list of productSettingType
        /// </summary>
        /// <returns></returns>
        IList<ProductSettingType> ListProductSettingType();

        /// <summary>
        /// Update a Product Batch
        /// </summary> 
        /// <returns>Repository response object</returns>
        bool UpdateProductBatch(int productBatchId, int statusTypeId, string inputJson = null, string errorDetails = null);

        /// <summary>
        /// Update a Product Activity Log
        /// </summary> 
        /// <returns></returns>
        void UpdateProductActivityLog(long batchProcessorGroupId, int productId, List<AdditionalParameters> additionalParameters);


        /// <summary>
        /// Returns a list of product activity logs
        /// </summary>
        /// <returns></returns>
        IList<AdditionalParameters> GetProductActivityLog(long batchProcessorGroupId);

        /// <summary>
        /// Clears Product Activity Log
        /// </summary> 
        /// <returns></returns>
        void DeleteProductActivityLog(long batchProcessorGroupId);

        /// <summary>
        /// Save Persona Product Properties
        /// </summary>
        /// <param name="assignUserPersonaId"></param>
        /// <param name="productId"></param>
        /// <param name="personaProductProperties"></param>
        /// <returns></returns>
        bool SavePersonaProductProperties(long assignUserPersonaId, int productId, string personaProductProperties);

        /// <summary>
        /// Returns List of Product Batch Statuses
        /// </summary>
        IList<ProductBatchStatus> ListProductBatchStatuses(Guid realPageId, long assignUserPersonaId);

        /// <summary>
        /// Returns the id of ProductSettingType
        /// </summary>
        int GetProductSettingType(string productSettingName);

        /// <summary>
        /// Returns a list of productfamilies
        /// </summary>
        /// <param name="personRealPageId">Edited User enterprise Id</param>
        /// <param name="accessFilter">Filter products</param>
        /// <param name="loginName">User Login Name</param>
        /// <returns>List of Product Families</returns>
        //IList<ProductFamily> GetProductFamilies(Guid? personRealPageId = null, string accessFilter = null);

        IList<ProductFamily> GetProductFamilies(Guid organizationRealPageId, Guid editorRealPageId, Guid? personRealPageId = null, string accessFilter = null, string loginName = null);

        /// <summary>
        /// List of Roles by Party ID, Product List and Product ID
        /// </summary>
        /// <param name="partyId">Party ID</param>
        /// <param name="productIdList">List of product ids for the party</param>   
        /// <param name="productId">Product ID</param>   
        /// <returns>List of Roles by PartyId and Product</returns>
        List<ProductRole> ListRolesForProductByParty(long partyId, IList<int> productIdList, int productId);

        /// <summary>
        /// List of Roles with Rights count
        /// </summary>
        /// <param name="partyId">Party ID</param>   
        /// <param name="productId">Product ID</param>   
        /// <param name="productIdList">Product ID's by Org</param>        
        /// <returns>List of Roles and rights count by PartyId and Product</returns>
        IList<RightRoleDetail> ListRoleWithRights(long partyId, int productId, List<int> productIdList);

        /// <summary>
        /// List GB products
        /// </summary>
        IList<GbProductMap> ListProducts(int? productId, Guid? productGuid, string name, string booksProductCode);

        /// <summary>
        /// Returns product details for given product code.
        /// This will get replaced with Blue book call in future
        /// </summary> 
        GbProductMap GetBooksMasterProductDetail(int gbProductId);

        /// <summary>
        /// Returns product propertyId roles details for given product code and persona.		
        /// </summary> 
        RolePropertyList GetUserProductDataFromProductBatch(long personaId, int productId);

        /// <summary>
        /// Returns a list of product IDs that are shared with other products from the given list of organization products.
        /// </summary>
        /// <param name="organizationProducts">List of product IDs belonging to the organization.</param>
        /// <returns>List of product IDs that are shared with other products.</returns>
        IList<int> GetProductSharedwithOtherProductIdList(IList<int> organizationProducts);

        /// <summary>
        /// Used to get a list of products ids for a company by the company guid
        /// </summary>
        /// <param name="organizationRealPageId"></param>
        /// <returns></returns>
        IList<int> GetProductIdsByCompany(Guid organizationRealPageId);

        /// <summary>
        /// Used to get a list of products ids for a company by the company party id
        /// </summary>
        /// <param name="organizationPartyId"></param>
        /// <returns></returns>
        IList<int> GetProductIdsByCompany(long organizationPartyId);

        /// <summary>
        /// Used to update the persona product setting type for the given user and setting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userPersonaId"></param>
        /// <param name="productId"></param>
        /// <param name="settingType"></param>
        /// <param name="value"></param>
        void UpdateProductSettingProductStatus<T>(long userPersonaId, int productId, string settingType, T value);

        /// <summary>
        /// Returns all the products
        /// </summary>
        IList<GbProductMap> GetAllProducts();

        /// <summary>
        /// Search by company, product ids, roles, rights propertyId and returns userlist
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="upfmId"></param>
        /// <param name="products"></param>
        /// <param name="rowsPerPage"></param>
        /// <param name="pageNumber"></param>
        /// <param name="roles"></param>
        /// <param name="rights"></param>
        /// <param name="propertyIds"></param>
        /// <param name="companyDomain"></param>
        /// <returns>List of Users by product or company</returns>
        IList<EnterpriseProductUser> GetUsersByCompanyorProducts(string companyId, string upfmId, IList<int> products, int rowsPerPage, int pageNumber,
                                                                 IList<string> roles, IList<string> rights, List<string> propertyIds, string companyDomain);

       /// <summary>
        /// Get Unified Login mapping PersonaId for Product UserId by company or upfmId and product id
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="upfmId"></param>
        /// <param name="productId"></param>
        /// <param name="productUserIds"></param>
        /// <returns>List of Unified Login mapping UserId by product and company</returns>
        List<ULMappedPersonaIds> GetULMappingPersonaIDsByCompanyAndProducts(int companyId, string upfmId, int productId, List<string> productUserIds);

        /// <summary>
        /// Used to get a list of products for the given persona id
        /// </summary>
        /// <param name="personaId"></param>
        /// <param name="statusType"></param>
        /// <returns></returns>
        IList<PersonaProduct> GetAllProductsByPersona(long personaId, ProductBatchStatusType statusType);

        /// <summary>
        /// Search by company and product ids and returns userlist
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="products"></param>
        /// <returns>List of Users</returns>
        IList<EnterpriseProductUser> GetUsersByCompanyorProducts(string companyId, IList<int?> products, string upfmId = null, string userType = null,  string userStatus = null);

        /// <summary>
        /// Get loggedin user assigned products
        /// </summary>
        /// <param name="PersonaId"></param>
        /// <param name="ProductStatus"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        bool isProductAssigned(long PersonaId, int ProductStatus, int productId);
        IList<PersonaProductUserDetails> ListProductsByPersonaId(long personaId, int statusType);

        IList<UserBatchProductDetail> GetUserBatchDetails(int batchGroupId, long editorUserPersonId, long subjectUserPersonId);

        void UpdateBatchGroupStatus(int groupId, bool isLogged);

        void UpdateBatchProcessorLog(int batchProcessorId, DateTime? startDateTime, DateTime? endDateTime);

        /// <summary>
        /// GetEnterpriseRoleForPersona
        /// </summary>
        /// <param name="personaId"></param>
        /// <returns></returns>
        RoleTemplate GetEnterpriseRoleForPersona(long personaId);
        /// <summary>
        /// GetRoleTemplateProductRoleMapping
        /// </summary>
        /// <param name="roleTemplateId"></param>
        /// <param name="partyId"></param>
        /// <returns></returns>
        List<RoleTemplateProductRole> GetRoleTemplateProductRoleMapping(int roleTemplateId, long partyId);

        /// <summary>
        /// GetEnterpriseRoleProductsByOrganization
        /// </summary>
        /// <param name="roleTemplateId"></param>
        /// <param name="organizationRealPageId"></param>
        /// <returns></returns>
        List<int> GetEnterpriseRoleProductsByOrganization(int roleTemplateId, Guid organizationRealPageId);

        /// <summary>
        /// GetPersonaProductPrimaryProperties
        /// </summary>
        /// <param name="personaId"></param>
        /// <returns></returns>
        List<PersonaProductProperty> GetPersonaProductPrimaryProperties(long personaId);

        /// <summary>
        /// GetEnterpriseRoleUpdatedProductsByRoleTemplateId
        /// </summary>
        /// <param name="roleTemplateId"></param>       
        /// <returns></returns>
        List<int> GetEnterpriseRoleUpdatedProductsByRoleTemplateId(int roleTemplateId, DateTime createdDateTime);

        /// <summary>
        /// GetEnterpriseRoleProductsByRoleTemplateId
        /// </summary>
        /// <param name="roleTemplateId"></param>
        /// <param name="organizationPartyId"></param>
        /// <returns></returns>
        List<int> GetEnterpriseRoleProductsByRoleTemplateId(int roleTemplateId, long organizationPartyId);

        /// <summary>
        /// GetEnterpriseRoleDeletedProductsByRoleTemplateId
        /// </summary>
        /// <param name="roleTemplateId"></param>

        /// <returns></returns>
        List<int> GetEnterpriseRoleDeletedProductsByRoleTemplateId(int roleTemplateId, DateTime createdDateTime);
        List<int> GetEnterpriseRoleNewProductsByRoleTemplateId(int roleTemplateId, DateTime createdDateTime);
        /// <summary>
        /// GetPersonaHasProductError
        /// </summary>
        /// <param name="personaId"></param>
        /// <returns></returns>
        bool GetPersonaHasProductError(long personaId);

        /// <summary>
        /// Get AdGroups For Product
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        List<AdGroupProduct> GetAdGroupsForProduct(int productId);

        /// <summary>
        /// Get AdGroups For User
        /// </summary>
        /// <param name="personaId"></param>
        /// <returns></returns>
        List<AdGroup> GetAdGroupsForUser(long personaId);

        List<AdGroup> GetUserManagementADGroupsByProduct(long productId);
        List<ProductAdGroupsCount> GetPersonaProductsAdGroupsCount(long personaId);

        /// <summary>
        /// Get roles related to AdGroups
        /// </summary>
        /// <param name="personaId"></param>
        /// <returns></returns>
        List<AdGroupRole> GetAdGroupRolesByPersona(long personaId);

        void InsertProductLoginActivitybyUser(int productId, long personaId, long UserId);

        IList<SamlAttributes> GetProductSamlDetails(long personaId, int productId);

        IList<ProductSamlDetails> ListPersonaProductsSamlDetails(long PersonaId);

    }
}