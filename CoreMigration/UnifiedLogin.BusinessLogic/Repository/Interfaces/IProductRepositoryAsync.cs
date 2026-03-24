//using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.EnterpriseRole;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using UnifiedLogin.SharedObjects.Saml;
using EnterpriseProductUser = UnifiedLogin.SharedObjects.Enterprise.ProductUsers;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
    /// <summary>
    /// Product Repository
    /// </summary>
    public interface IProductRepositoryAsync
    {
        /// <summary>
        /// Returns a list of products user has access to, filterable by favorites and resouce only
        /// </summary>
        /// <param name="persona">persona</param>   
        /// <param name="productSelectType">productSelectType</param>
        /// <param name="security">security</param>
        /// <returns></returns>
     //   Task<IList<PersonaProductUserDetails>> GetAssignedProductsByPersonaAsync(Persona persona, ProductSelectType? productSelectType = null, RouteSecurity security = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns list of products that are resource type by filtering organization products
        /// </summary>
        /// <param name="organizationRealPageId">organizationRealPageId</param>
        /// <returns></returns>
        Task<IList<ProductUI>> GetProductsResourceTypeAsync(Guid organizationRealPageId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a list of all product settings that an organization has
        /// </summary>
        /// <param name="organizationRealPageId">organizationRealPageId</param>
        /// <param name="productId">The id of the product to be filtered. null returns all product settings</param>
        /// <returns></returns>
        Task<IList<ProductSettingList>> GetProductSettingsAsync(Guid organizationRealPageId, int productId , CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Product Settings
        /// </summary>
        /// <param name="organizationRealPageId"></param>
        /// <returns></returns>
        Task<IList<ProductSettingList>> GetProductSettingsAsync(Guid organizationRealPageId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a list of all product settings for persona
        /// </summary>
        /// <param name="personaId">personaId</param>
        /// <returns></returns>
        Task<IList<ProductSettingList>> GetProductSettingsByPersonaAsync(long personaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a list of products that an organization has license using its organizationRealPageId
        /// </summary>
        /// <param name="organizationRealPageId">organizationRealPageId</param>
        /// <param name="personaId">personaId</param>
        /// <param name="resourceOnly">Only return resource type products</param>
        /// <param name="allProducts">Return all product types</param>
        /// <param name="replaceProductCodeWithUDMIfExists">True when the product code being returned should be the UDM code if one exists, otherwise leave productcode alone</param>
        /// <returns></returns>
        Task<IList<ProductUI>> GetProductsAsync(Guid organizationRealPageId, long personaId, bool resourceOnly = false, bool allProducts = false, bool replaceProductCodeWithUDMIfExists = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a list of productTypes
        /// </summary>
        /// <returns></returns>
        Task<IList<ProductType>> GetProductTypesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Create ProductSetting
        /// </summary>
        /// <param name="PersonaId"></param>
        /// <param name="ProductId"></param>
        /// <param name="ProductSettingTypeId"></param>
        /// <param name="Value"></param>
        /// <returns>Repository response object</returns>
        Task<RepositoryResponse> CreateProductSettingAsync(long PersonaId, int ProductId, int ProductSettingTypeId, string Value, CancellationToken cancellationToken = default);

      
        /// <summary>
        /// Returns a list of productSettingType
        /// </summary>
        /// <returns></returns>
        Task<IList<ProductSettingType>> ListProductSettingTypeAsync( CancellationToken cancellationToken = default);

        /// <summary>
        /// Update a Product Batch
        /// </summary> 
        /// <returns>Repository response object</returns>
        Task<bool> UpdateProductBatchAsync(int productBatchId, int statusTypeId, string inputJson = null, string errorDetails = null, CancellationToken cancellationToken = default); 

        /// <summary>
        /// Update a Product Activity Log
        /// </summary> 
        /// <returns></returns>
        Task UpdateProductActivityLogAsync(long batchProcessorGroupId, int productId, string jsonString, CancellationToken cancellationToken = default);


        /// <summary>
        /// Returns a list of product activity logs
        /// </summary>
        /// <returns></returns>
        Task<IList<AdditionalParameters>> GetProductActivityLogAsync(long batchProcessorGroupId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clears Product Activity Log
        /// </summary> 
        /// <returns></returns>
        Task DeleteProductActivityLogAsync(long batchProcessorGroupId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Save Persona Product Properties
        /// </summary>
        /// <param name="assignUserPersonaId"></param>
        /// <param name="productId"></param>
        /// <param name="personaProductProperties"></param>
        /// <returns></returns>
        Task<bool> SavePersonaProductPropertiesAsync(long assignUserPersonaId, int productId, string personaProductProperties, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns List of Product Batch Statuses
        /// </summary>
        Task<IList<ProductBatchStatus>> ListProductBatchStatusesAsync(Guid realPageId, long assignUserPersonaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the id of ProductSettingType
        /// </summary>
        Task<int> GetProductSettingTypeAsync(string productSettingName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a list of productfamilies
        /// </summary>
        /// <param name="personRealPageId">Edited User enterprise Id</param>
        /// <param name="accessFilter">Filter products</param>
        /// <param name="loginName">User Login Name</param>
        /// <returns>List of Product Families</returns>
        //IList<ProductFamily> GetProductFamilies(Guid? personRealPageId = null, string accessFilter = null);

        Task<IList<ProductFamily>> GetProductFamiliesAsync(Guid organizationRealPageId, Guid editorRealPageId, Guid? personRealPageId = null, string accessFilter = null, string loginName = null, CancellationToken cancellationToken = default);

        /// <summary>Raw SP_ListProductFamilies — no enrichment.</summary>
        Task<IList<ProductFamily>> GetProductFamiliesRawAsync(CancellationToken cancellationToken = default);

        /// <summary>Raw SP_ListProductsByOrganization returning Solution rows.</summary>
        Task<IList<Solution>> GetSolutionsByOrganizationAsync(Guid organizationRealPageId, CancellationToken cancellationToken = default);

        /// <summary>
        /// List of Roles by Party ID, Product List and Product ID
        /// </summary>
        /// <param name="partyId">Party ID</param>
        /// <param name="productIdList">List of product ids for the party</param>   
        /// <param name="productId">Product ID</param>   
        /// <returns>List of Roles by PartyId and Product</returns>
        Task<List<ProductRole>> ListRolesForProductByPartyAsync(long partyId, IList<int> productIdList, int productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// List of Roles with Rights count
        /// </summary>
        /// <param name="partyId">Party ID</param>   
        /// <param name="productId">Product ID</param>   
        /// <param name="productIdList">Product ID's by Org</param>        
        /// <returns>List of Roles and rights count by PartyId and Product</returns>
        Task<IList<RightRoleDetail>> ListRoleWithRightsAsync(long partyId, int productId, List<int> productIdList, CancellationToken cancellationToken = default);

        /// <summary>
        /// List GB products
        /// </summary>
        Task<IList<GbProductMap>> ListProductsAsync(int? productId, Guid? productGuid, string name, string booksProductCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns product details for given product code.
        /// This will get replaced with Blue book call in future
        /// </summary> 
        Task<GbProductMap> GetBooksMasterProductDetailAsync(int gbProductId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns product propertyId roles details for given product code and persona.		
        /// </summary> 
        Task<RolePropertyList> GetUserProductDataFromProductBatchAsync(long personaId, int productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a list of product IDs that are shared with other products from the given list of organization products.
        /// </summary>
        /// <param name="organizationProducts">List of product IDs belonging to the organization.</param>
        /// <returns>List of product IDs that are shared with other products.</returns>
        Task<IList<int>> GetProductSharedwithOtherProductIdListAsync(IList<int> organizationProducts, CancellationToken cancellationToken = default);


        /// <summary>
        /// Used to get a list of products ids for a company by the company guid
        /// </summary>
        /// <param name="organizationRealPageId"></param>
        /// <returns></returns>
        Task<IList<int>> GetProductIdsByCompanyAsync(Guid organizationRealPageId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Used to get a list of products ids for a company by the company party id
        /// </summary>
        /// <param name="organizationPartyId"></param>
        /// <returns></returns>
        Task<IList<int>> GetProductIdsByCompanyAsync(long organizationPartyId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Used to update the persona product setting type for the given user and setting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userPersonaId"></param>
        /// <param name="productId"></param>
        /// <param name="settingType"></param>
        /// <param name="value"></param>
        Task UpdateProductSettingProductStatusAsync<T>(long userPersonaId, int productId, string settingType, T value, CancellationToken cancellationToken = default);

        Task ClearPersonaErrorAsync(long userPersonaId, int productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all the products
        /// </summary>
        Task<IList<GbProductMap>> GetAllProductsAsync(CancellationToken cancellationToken = default);

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
        Task<IList<EnterpriseProductUser>> GetUsersByCompanyorProductsAsync(string companyId, string upfmId, IList<int> products, int rowsPerPage, int pageNumber,
                                                                 IList<string> roles, IList<string> rights, List<string> propertyIds, string companyDomain, CancellationToken cancellationToken = default);

       /// <summary>
        /// Get Unified Login mapping PersonaId for Product UserId by company or upfmId and product id
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="upfmId"></param>
        /// <param name="productId"></param>
        /// <param name="productUserIds"></param>
        /// <returns>List of Unified Login mapping UserId by product and company</returns>
        Task<List<ULMappedPersonaIds>> GetULMappingPersonaIDsByCompanyAndProductsAsync(int companyId, string upfmId, int productId, List<string> productUserIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Used to get a list of products for the given persona id
        /// </summary>
        /// <param name="personaId"></param>
        /// <param name="statusType"></param>
        /// <returns></returns>
        Task<IList<PersonaProduct>> GetAllProductsByPersonaAsync(long personaId, ProductBatchStatusType statusType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the product internal settings for the given product id
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        Task<IList<ProductInternalSetting>> GetProductInternalSettingsAsync(int productId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Search by company and product ids and returns userlist
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="products"></param>
        /// <returns>List of Users</returns>
        Task<IList<EnterpriseProductUser>> GetUsersByCompanyorProductsAsync(string companyId, IList<int?> products, string upfmId = null, string userType = null,  string userStatus = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get loggedin user assigned products
        /// </summary>
        /// <param name="PersonaId"></param>
        /// <param name="ProductStatus"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        Task<bool> IsProductAssignedAsync(long PersonaId, int ProductStatus, int productId, CancellationToken cancellationToken = default);
        Task<IList<PersonaProductUserDetails>> ListProductsByPersonaIdAsync(long personaId, int statusType, CancellationToken cancellationToken = default);

        Task<IList<UserBatchProductDetail>> GetUserBatchDetailsAsync(int batchGroupId, long editorUserPersonId, long subjectUserPersonId, CancellationToken cancellationToken = default);

        Task UpdateBatchGroupStatusAsync(int groupId, bool isLogged, CancellationToken cancellationToken = default);

        Task UpdateBatchProcessorLogAsync(int batchProcessorId, DateTime? startDateTime, DateTime? endDateTime, CancellationToken cancellationToken = default);

        /// <summary>
        /// GetEnterpriseRoleForPersona
        /// </summary>
        /// <param name="personaId"></param>
        /// <returns></returns>
        Task<RoleTemplate> GetEnterpriseRoleForPersonaAsync(long personaId, CancellationToken cancellationToken = default);
        /// <summary>
        /// GetRoleTemplateProductRoleMapping
        /// </summary>
        /// <param name="roleTemplateId"></param>
        /// <param name="partyId"></param>
        /// <returns></returns>
        Task<List<RoleTemplateProductRole>> GetRoleTemplateProductRoleMappingAsync(int roleTemplateId, long partyId, CancellationToken cancellationToken = default);

        /// <summary>
        /// GetEnterpriseRoleProductsByOrganization
        /// </summary>
        /// <param name="roleTemplateId"></param>
        /// <param name="organizationRealPageId"></param>
        /// <returns></returns>
        Task<List<int>> GetEnterpriseRoleProductsByOrganizationAsync(int roleTemplateId, Guid organizationRealPageId, CancellationToken cancellationToken = default);

        /// <summary>
        /// GetPersonaProductPrimaryProperties
        /// </summary>
        /// <param name="personaId"></param>
        /// <returns></returns>
        Task<List<PersonaProductProperty>> GetPersonaProductPrimaryPropertiesAsync(long personaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// GetEnterpriseRoleUpdatedProductsByRoleTemplateId
        /// </summary>
        /// <param name="roleTemplateId"></param>       
        /// <returns></returns>
        Task<List<int>> GetEnterpriseRoleUpdatedProductsByRoleTemplateIdAsync(int roleTemplateId, DateTime createdDateTime, CancellationToken cancellationToken = default);

        /// <summary>
        /// GetEnterpriseRoleProductsByRoleTemplateId
        /// </summary>
        /// <param name="roleTemplateId"></param>
        /// <param name="organizationPartyId"></param>
        /// <returns></returns>
        Task<List<int>> GetEnterpriseRoleProductsByRoleTemplateIdAsync(int roleTemplateId, long organizationPartyId, CancellationToken cancellationToken = default);

        /// <summary>
        /// GetEnterpriseRoleDeletedProductsByRoleTemplateId
        /// </summary>
        /// <param name="roleTemplateId"></param>

        /// </returns>
        Task<List<int>> GetEnterpriseRoleDeletedProductsByRoleTemplateIdAsync(int roleTemplateId, DateTime createdDateTime, CancellationToken cancellationToken = default);
        Task<List<int>> GetEnterpriseRoleNewProductsByRoleTemplateIdAsync(int roleTemplateId, DateTime createdDateTime, CancellationToken cancellationToken = default);
        /// <summary>
        /// GetPersonaHasProductError
        /// </summary>
        /// <param name="personaId"></param>
        /// <returns></returns>
        Task<bool> GetPersonaHasProductErrorAsync(long personaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get AdGroups For Product
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        Task<List<AdGroupProduct>> GetAdGroupsForProductAsync(int productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get AdGroups For User
        /// </summary>
        /// <param name="personaId"></param>
        /// <returns></returns>
        Task<List<AdGroup>> GetAdGroupsForUserAsync(long personaId, CancellationToken cancellationToken = default);

        Task<List<AdGroup>> GetUserManagementADGroupsByProductAsync(long productId, CancellationToken cancellationToken = default);
        Task<List<ProductAdGroupsCount>> GetPersonaProductsAdGroupsCountAsync(long personaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get roles related to AdGroups
        /// </summary>
        /// <param name="personaId"></param>
        /// <returns></returns>
        Task<List<AdGroupRole>> GetAdGroupRolesByPersonaAsync(long personaId, CancellationToken cancellationToken = default);

        Task InsertProductLoginActivitybyUserAsync(int productId, long personaId, long UserId, CancellationToken cancellationToken = default);

        Task<IList<SamlAttributes>> GetProductSamlDetailsAsync(long personaId, int productId, CancellationToken cancellationToken = default);
        Task<IList<ProductSamlDetails>> ListPersonaProductsSamlDetailsAsync(long PersonaId, CancellationToken cancellationToken = default);

    }
}