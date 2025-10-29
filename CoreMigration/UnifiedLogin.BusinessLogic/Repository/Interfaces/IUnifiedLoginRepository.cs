using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using System.Collections.Generic;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
    /// <summary>
    /// Interface for UnifiedLogin Repository
    /// </summary>
    public interface IUnifiedLoginRepository
    {
        /// <summary>
        /// Add new role
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="desc">User Persona ID</param>             
        /// <param name="roleTypeId">User Role</param>   
        /// <param name="roleCategoryId">isDeleted</param>   
        /// <param name="partyId">isDeleted</param> 
        /// <param name="userId"></param>
        /// <returns>Add new Role</returns>
        RepositoryResponse AddCustomRole(string roleName, string desc, long roleTypeId, long roleCategoryId, long partyId, int userId, string OrganizationType);

        /// <summary>
        /// Delete role
        /// </summary>
        /// <param name="roleId">User Role</param>                    
        /// <returns>Deletes Role Response</returns>
        RepositoryResponse DeleteRole(long roleId);

        /// <summary>
        /// List of Roles by User Persona ID
        /// </summary>       
        /// <returns>List of Category Types</returns>
        List<CategoryType> GetCategoryType();

        /// <summary>
        /// Insert Property/Role to User
        /// </summary>
        /// <param name="userPersonaId">User Persona ID</param>             
        /// <param name="role">User Role</param>   
        /// <param name="del">isDeleted</param>   
        /// <param name="userId"></param>
        /// <returns>List of Roles assigned to Persona</returns>
        RepositoryResponse InsertAssignedRoleToUser(long userPersonaId, UserAccessGroup role, int userId, long del = 0);

        /// <summary>
        /// Insert Property/Role to User
        /// </summary>
        /// <param name="userPersonaId">User Persona ID</param>      
        /// <param name="productId">Product ID</param>      
        /// <param name="property">Property</param>      
        /// <param name="role">User Role</param>   
        /// <param name="del">isDeleted</param>   
        /// <returns>List of Roles assigned to Persona</returns>
        RepositoryResponse InsertDelAssignedPropRoleToUser(long userPersonaId, long productId, UserLocation property, UserAccessGroup role, long del = 0);

        /// <summary>
        /// Insert Property/Role to User
        /// </summary>
        /// <param name="userPersonaId">User Persona ID</param>      
        /// <param name="productId">Product ID</param>      
        /// <param name="propertyId">Property ID</param>      
        /// <param name="roleId">User Role ID</param>   
        /// <param name="del">isDeleted</param>   
        /// <returns>List of Roles assigned to Persona</returns>
        RepositoryResponse InsertDelAssignedPropRoleToUserNew(long userPersonaId, long productId, long propertyId, long roleId, long del = 0);

        /// <summary>
        /// Update Rights to Role
        /// </summary>
        int LinkRightsToRole(IEnumerable<RightRoleAddRem> rightsList, int userId);

        /// <summary>
        /// List ALL Rights by Party Product
        /// </summary>
        /// <param name="partyId">Party ID</param>   
        /// <param name="productId">Product ID</param>   
        /// <returns>List ALL  rights by PartyId and Product</returns>
        List<ProductRight> ListAllRightsForProductsByPartyId(long partyId, long productId, List<int> productIdList);

        /// <summary>
        /// List of Roles by User Persona ID
        /// </summary>
        /// <param name="userPersonaId">Persona ID</param>   
        /// <param name="productId">Product ID</param>   
        /// <returns>List of Properties/Role assigned to Persona</returns>
        List<Property> ListPropByPersona(long userPersonaId, long productId);

        /// <summary>
        /// List of Roles by User Persona ID
        /// </summary>
        /// <param name="userPersonaId">Persona ID</param>   
        /// <param name="productId">Product ID</param>   
        /// <returns>List of Properties/Role assigned to Persona</returns>
        List<PropertyRole> ListPropertyMappingByPersona(long userPersonaId, long productId);

        /// <summary>
        /// List of Rights by Party Product
        /// </summary>
        /// <param name="partyId">Party ID</param>   
        /// <param name="productId">Product ID</param>   
        /// <returns>List of  rights by PartyId and Product</returns>
        List<ProductRight> ListRightForProductsByPartyId(long partyId, long productId);

        /// <summary>
        /// List of Rights by Role
        /// </summary>
        /// <param name="partyId">Party ID</param>
        /// <param name="productIdList">Product ID</param>
        /// <param name="productId">Product ID</param>
        /// <param name="roleId">Role ID</param>   
        /// <returns>List of Rights by RoleId and Product</returns>
        List<ProductRight> ListRightsByRole(long partyId, IList<int> productIdList, long productId, long roleId);

        /// <summary>
        /// List of Roles with Rights count
        /// </summary>
        /// <param name="partyId">Party ID</param>   
        /// <param name="productId">Product ID</param>   
        /// <returns>List of Roles and rights count by PartyId and Product</returns>
        List<RightRoleDetail> ListRightWithRoles(long partyId, long productId, IList<int> productIdList);

        /// <summary>
        /// List of Role and Right Det by Party Product
        /// </summary>
        /// <param name="partyId">Party ID</param>   
        /// <param name="productId">Product ID</param>   
        /// <returns>List of Roles by PartyId and Product</returns>
        List<RightRoleDetail> ListRoleRightDetForProductsByPartyId(long partyId, long productId, List<int> productIdList);

        /// <summary>
        /// List of Roles by User Persona ID
        /// </summary>
        /// <param name="userPersonaId">Persona ID</param>        
        /// <returns>List of Roles assigned to Persona</returns>
        List<ProductRole> ListRolesAssignedToPersona(long userPersonaId);

        /// <summary>
        /// List of Roles by Party ID
        /// </summary>
        /// <param name="partyId">Party ID</param>        
        /// <returns>List of Roles</returns>
        List<ProductRole> ListRolesByParty(long partyId);

        /// <summary>
        /// List of  Rights by Role
        /// </summary>
        /// <param name="partyId">Party ID</param>   
        /// <param name="productId">Product ID</param>   
        /// <param name="rightId">Role ID</param>   
        /// <returns>List of Roles by RightId and Product</returns>
        List<ProductRole> ListRolesByRight(long partyId, long productId, long rightId);

        /// <summary>
        /// List of Roles by Party Product
        /// </summary>
        /// <param name="partyId">Party ID</param>   
        /// <param name="productId">Product ID</param>
        /// <param name="productIdList"></param>   
        /// <returns>List of Roles by PartyId and Product</returns>
        List<ProductRole> ListRolesForProductsByPartyId(long partyId, long productId, List<int> productIdList);

        /// <summary>
        /// List of Roles with Rights count
        /// </summary>
        /// <param name="partyId">Party ID</param>   
        /// <param name="productId">Product ID</param>
        /// <param name="productIdList"></param>   
        /// <returns>List of Roles and rights count by PartyId and Product</returns>
        List<RightRoleDetail> ListRoleWithRights(long partyId, long productId, List<int> productIdList);

        /// <summary>
        /// Add new role
        /// </summary>
        /// <param name="roleId">User Role</param>   
        /// <param name="roleName"></param>
        /// <param name="desc">User Persona ID</param>
        /// <param name="userId"></param>
        /// <returns>Add new Role Response</returns>
        RepositoryResponse UpdateCustomRole(long roleId, string roleName, string desc, int userId);

        /// <summary>
        /// Get persona AD groups by Persona ID
        /// </summary>
        /// <param name="personaId">PERSONAID</param>  
        /// <returns>PersonaADGroup</returns>
        List<PersonaADGroup> GetPersonaADGroups(long personaId);

        /// <summary>
        /// Get organization type AD Groups
        /// </summary>
        /// <returns>OrgTypesADGroups</returns>
        List<OrgTypesADGroups> GetOrgTypesADGroups();
    }
}