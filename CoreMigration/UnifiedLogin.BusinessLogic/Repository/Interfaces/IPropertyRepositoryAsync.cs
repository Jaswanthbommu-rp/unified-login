using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
    /// <summary>
    /// Property Repository interface
    /// </summary>
    public interface IPropertyRepositoryAsync
    {
        /// <summary>
        /// List of Roles by User Persona ID
        /// </summary>
        /// <param name="userPersonaId">Persona ID</param>   
        /// <param name="productId">Product ID</param>   
        /// <returns>List of Properties/Role assigned to Persona</returns>
        Task<List<ProductProperty>> ListPropertiesByPersonaAsync(long userPersonaId, int productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        Task<List<UPFMPropertyInstance>> ListUPFMPropertyInstanceByPersonaAsync(long userPersonaId, ProductEnum productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Used to get the list of the internal UPFM property instance ids for the given persona and product
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        Task<List<int>> ListUPFMPropertyInstanceIdByPersonaAsync(long userPersonaId, ProductEnum productId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Used to get the list of the internal UPFM property instance ids for the given persona and product
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        Task<List<int>> ListUPFMPropertyInstanceIdByPersonaAsync(long userPersonaId, int productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Used to get the UPFM property details for the given instance ids
        /// </summary>
        /// <param name="propertyInstanceIds"></param>
        /// <returns></returns>
        Task<List<UPFMPropertyInstance>> ListUPFMPropertyInstanceIdByInstanceIdsAsync(List<Guid> propertyInstanceIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Insert or Remove a Property for the given User
        /// </summary>
        /// <param name="userPersonaId">User Persona ID</param>      
        /// <param name="productId">Product ID</param>      
        /// <param name="propertyId">Property ID</param>      
        /// <param name="remove">isDeleted</param>   
        /// <returns>List of Roles assigned to Persona</returns>
        Task<RepositoryResponse> InsertRemoveAssignedPropertyToUserAsync(long userPersonaId, ProductEnum productId, long propertyId, int remove = 0, CancellationToken cancellationToken = default);

        //Task<RepositoryResponse> AddUpdatePropertyMappingAsync(long personaId, ProductEnum productId, string propertyJSON);

        /// <summary>
        /// Used to update any property mapping records that match the old id to a new id
        /// </summary>
        /// <param name="originalPropertyId"></param>
        /// <param name="newPropertyId"></param>
        /// <returns></returns>
        Task<RepositoryResponse> UpdatePropertyMappingReMapAsync(long originalPropertyId, long newPropertyId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="productId"></param>
        /// <param name="propertyInstanceId"></param>
        /// <param name="remove"></param>
        /// <returns></returns>
        Task<RepositoryResponse> InsertRemoveAssignedPropertyInstanceToUserAsync(long userPersonaId, int productId, long propertyInstanceId, int remove = 0, CancellationToken cancellationToken = default);

        /// <summary>
        /// Used to insert new UPFM property instances into the database
        /// </summary>
        /// <param name="propertyInstance"></param>
        /// <returns></returns>
        Task<RepositoryResponse> InsertUPFMPropertyInstanceAsync(UPFMPropertyInstance propertyInstance, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Properties for a Organization
        /// </summary>
        /// <param name="propertyInstanceIds">propertyInstanceIds</param>
        /// <param name="propertyName">PropertyName</param>
        /// <param name="blueId">blueId</param>
        /// <param name="status">Status</param>
        /// <param name="dataFilterSort">datafilter</param>
        /// <returns>List of Properties for a company </returns>
        Task<List<PropertySetup>> GetPropertiesForCompanyAsync(List<Guid> propertyInstanceIds, string propertyName = null, int? propertyMasterid = null, int? status = null, RequestParameter dataFilterSort = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update Property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        Task<RepositoryResponse> UpdatePropertyAsync(UPFMPropertyInstance property, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update Property List
        /// </summary>
        /// <param name="propertyInstanceIds"></param>
        /// <returns></returns>
        Task<RepositoryResponse> UpdateUPFMPropertyListAsync(List<UPFMPropertyInstance> propertyInstanceIds, CancellationToken cancellationToken = default);
        /// <summary>
        /// Delete Property instance
        /// </summary>
        /// <param name="instanceId">property InstanceID</param>
        /// <returns></returns>
        Task<RepositoryResponse> DeleteUPFMPropertyInstanceAsync(Guid instanceId, CancellationToken cancellationToken = default);

        Task<RepositoryResponse> StageUserProductPrimaryPropertiesAsync(string stagingData, long userPersonaId, int productId, long createdBy, CancellationToken cancellationToken = default);
        Task<RepositoryResponse> DeleteStagedUserProductPrimaryPropertiesAsync(long userPersonaId, int productId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Bulk insert/delete property instance mappings for a user using TVP
        /// </summary>
        Task<RepositoryResponse> BulkInsertRemovePropertyInstanceMappingsAsync(long userPersonaId, int productId, List<UPFMPropertyInstanceMapping> propertyMappings, CancellationToken cancellationToken = default);
    }
}