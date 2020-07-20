using System;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
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
		List<ProductProperty> ListPropertiesByPersona(long userPersonaId, ProductEnum productId);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userPersonaId"></param>
		/// <param name="productId"></param>
		/// <returns></returns>
        List<UPFMPropertyInstance> ListUPFMPropertyInstanceByPersona(long userPersonaId, ProductEnum productId);

        /// <summary>
		/// 
		/// </summary>
		/// <param name="userPersonaId"></param>
		/// <param name="productId"></param>
		/// <returns></returns>
        List<int> ListUPFMPropertyInstanceIdByPersona(long userPersonaId, ProductEnum productId);

		/// <summary>
		/// 
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
        RepositoryResponse InsertRemoveAssignedPropertyInstanceToUser(long userPersonaId, ProductEnum productId, long propertyInstanceId, int remove = 0);

    }
}