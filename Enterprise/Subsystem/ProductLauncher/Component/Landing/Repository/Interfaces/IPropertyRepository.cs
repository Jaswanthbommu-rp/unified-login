using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces
{
    /// <summary>
    /// Property Repository interface
    /// </summary>
    public interface IPropertyRepository
    {
        /// <summary>
        /// List of Roles by User Persona ID
        /// </summary>
        /// <param name="userPersonaId">Persona ID</param>   
        /// <param name="productId">Product ID</param>   
        /// <returns>List of Properties/Role assigned to Persona</returns>
        List<ProductProperty> ListPropertiesByPersona(long userPersonaId, int productId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        List<UPFMPropertyInstance> ListUPFMPropertyInstanceByPersona(long userPersonaId, ProductEnum productId);

        /// <summary>
        /// Used to get the list of the internal UPFM property instance ids for the given persona and product
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        List<int> ListUPFMPropertyInstanceIdByPersona(long userPersonaId, ProductEnum productId);
        /// <summary>
        /// Used to get the list of the internal UPFM property instance ids for the given persona and product
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        List<int> ListUPFMPropertyInstanceIdByPersona(long userPersonaId, int productId);

        /// <summary>
        /// Used to get the UPFM property details for the given instance ids
        /// </summary>
        /// <param name="propertyInstanceIds"></param>
        /// <returns></returns>
        List<UPFMPropertyInstance> ListUPFMPropertyInstanceIdByInstanceIds(List<Guid> propertyInstanceIds);

        /// <summary>
        /// Insert or Remove a Property for the given User
        /// </summary>
        /// <param name="userPersonaId">User Persona ID</param>      
        /// <param name="productId">Product ID</param>      
        /// <param name="propertyId">Property ID</param>      
        /// <param name="remove">isDeleted</param>   
        /// <returns>List of Roles assigned to Persona</returns>
        RepositoryResponse InsertRemoveAssignedPropertyToUser(long userPersonaId, ProductEnum productId, long propertyId, int remove = 0);

        //RepositoryResponse AddUpdatePropertyMapping(long personaId, ProductEnum productId, string propertyJSON);

        /// <summary>
        /// Used to update any property mapping records that match the old id to a new id
        /// </summary>
        /// <param name="originalPropertyId"></param>
        /// <param name="newPropertyId"></param>
        /// <returns></returns>
        RepositoryResponse UpdatePropertyMappingReMap(long originalPropertyId, long newPropertyId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="productId"></param>
        /// <param name="propertyInstanceId"></param>
        /// <param name="remove"></param>
        /// <returns></returns>
        RepositoryResponse InsertRemoveAssignedPropertyInstanceToUser(long userPersonaId, int productId, long propertyInstanceId, int remove = 0);

        /// <summary>
        /// Used to insert new UPFM property instances into the database
        /// </summary>
        /// <param name="propertyInstance"></param>
        /// <returns></returns>
        RepositoryResponse InsertUPFMPropertyInstance(UPFMPropertyInstance propertyInstance);

        /// <summary>
        /// Get Properties for a Organization
        /// </summary>
        /// <param name="propertyInstanceIds">propertyInstanceIds</param>
        /// <param name="propertyName">PropertyName</param>
        /// <param name="blueId">blueId</param>
        /// <param name="status">Status</param>
        /// <param name="dataFilterSort">datafilter</param>
        /// <returns>List of Properties for a company </returns>
        List<PropertySetup> GetPropertiesForCompany(List<Guid> propertyInstanceIds, string propertyName = null, int? blueId = null, int? status = null, RequestParameter dataFilterSort = null);

        /// <summary>
        /// Update Property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        RepositoryResponse UpdateProperty(UPFMPropertyInstance property);

        /// <summary>
        /// Update Property List
        /// </summary>
        /// <param name="propertyInstanceIds"></param>
        /// <returns></returns>
        RepositoryResponse UpdateUPFMPropertyList(List<UPFMPropertyInstance> propertyInstanceIds);
        /// <summary>
        /// Delete Property instance
        /// </summary>
        /// <param name="propertyInstanceID">property InstanceID</param>
        /// <returns></returns>
        RepositoryResponse DeleteUPFMPropertyInstance(Guid propertyInstanceID);

        RepositoryResponse StageUserProductPrimaryProperties(string stagingData, long userPersonaId, int productId, long createdBy);
        RepositoryResponse DeleteStagedUserProductPrimaryProperties(long userPersonaId, int productId);
    }
}