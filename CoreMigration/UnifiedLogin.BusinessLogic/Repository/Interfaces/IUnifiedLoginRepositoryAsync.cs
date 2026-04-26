using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Product.EmployeeAccess;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

/// <summary>
/// Interface for UnifiedLogin Repository
/// </summary>
public interface IUnifiedLoginRepositoryAsync
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
    Task<RepositoryResponse> AddCustomRoleAsync(string roleName, string desc, long roleTypeId, long roleCategoryId, long partyId, int userId, string OrganizationType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete role
    /// </summary>
    /// <param name="roleId">User Role</param>                    
    /// <returns>Deletes Role Response</returns>
    Task<RepositoryResponse> DeleteRoleAsync(long roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// List of Roles by User Persona ID
    /// </summary>       
    /// <returns>List of Category Types</returns>
    Task<List<CategoryType>> GetCategoryTypeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Insert Property/Role to User
    /// </summary>
    /// <param name="userPersonaId">User Persona ID</param>             
    /// <param name="role">User Role</param>   
    /// <param name="del">isDeleted</param>   
    /// <param name="userId"></param>
    /// <returns>List of Roles assigned to Persona</returns>
    Task<RepositoryResponse> InsertAssignedRoleToUserAsync(long userPersonaId, UserAccessGroup role, int userId, long del = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Insert Property/Role to User
    /// </summary>
    /// <param name="userPersonaId">User Persona ID</param>      
    /// <param name="productId">Product ID</param>      
    /// <param name="property">Property</param>      
    /// <param name="role">User Role</param>   
    /// <param name="del">isDeleted</param>   
    /// <returns>List of Roles assigned to Persona</returns>
    Task<RepositoryResponse> InsertDelAssignedPropRoleToUserAsync(long userPersonaId, long productId, UserLocation property, UserAccessGroup role, long del = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Insert Property/Role to User
    /// </summary>
    /// <param name="userPersonaId">User Persona ID</param>      
    /// <param name="productId">Product ID</param>      
    /// <param name="propertyId">Property ID</param>      
    /// <param name="roleId">User Role ID</param>   
    /// <param name="del">isDeleted</param>   
    /// <returns>List of Roles assigned to Persona</returns>
    Task<RepositoryResponse> InsertDelAssignedPropRoleToUserNewAsync(long userPersonaId, long productId, long propertyId, long roleId, long del = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update Rights to Role
    /// </summary>
    Task<int> LinkRightsToRoleAsync(IEnumerable<RightRoleAddRem> rightsList, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// List ALL Rights by Party Product
    /// </summary>
    /// <param name="partyId">Party ID</param>   
    /// <param name="productId">Product ID</param>   
    /// <returns>List ALL  rights by PartyId and Product</returns>
    Task<List<ProductRight>> ListAllRightsForProductsByPartyIdAsync(long partyId, long productId, List<int> productIdList, CancellationToken cancellationToken = default);

    /// <summary>
    /// List of Roles by User Persona ID
    /// </summary>
    /// <param name="userPersonaId">Persona ID</param>   
    /// <param name="productId">Product ID</param>   
    /// <returns>List of Properties/Role assigned to Persona</returns>
    Task<List<Property>> ListPropByPersonaAsync(long userPersonaId, long productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// List of Roles by User Persona ID
    /// </summary>
    /// <param name="userPersonaId">Persona ID</param>   
    /// <param name="productId">Product ID</param>   
    /// <returns>List of Properties/Role assigned to Persona</returns>
    Task<List<PropertyRole>> ListPropertyMappingByPersonaAsync(long userPersonaId, long productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// List of Rights by Party Product
    /// </summary>
    /// <param name="partyId">Party ID</param>   
    /// <param name="productId">Product ID</param>   
    /// <returns>List of  rights by PartyId and Product</returns>
    Task<List<ProductRight>> ListRightForProductsByPartyIdAsync(long partyId, long productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// List of Rights by Role
    /// </summary>
    /// <param name="partyId">Party ID</param>
    /// <param name="productIdList">Product ID</param>
    /// <param name="productId">Product ID</param>
    /// <param name="roleId">Role ID</param>   
    /// <returns>List of Rights by RoleId and Product</returns>
    Task<List<ProductRight>> ListRightsByRoleAsync(long partyId, IList<int> productIdList, long productId, long roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// List of Roles with Rights count
    /// </summary>
    /// <param name="partyId">Party ID</param>   
    /// <param name="productId">Product ID</param>   
    /// <returns>List of Roles and rights count by PartyId and Product</returns>
    Task<List<RightRoleDetail>> ListRightWithRolesAsync(long partyId, long productId, IList<int> productIdList, CancellationToken cancellationToken = default);

    /// <summary>
    /// List of Role and Right Det by Party Product
    /// </summary>
    /// <param name="partyId">Party ID</param>   
    /// <param name="productId">Product ID</param>   
    /// <returns>List of Roles by PartyId and Product</returns>
    Task<List<RightRoleDetail>> ListRoleRightDetForProductsByPartyIdAsync(long partyId, long productId, List<int> productIdList, CancellationToken cancellationToken = default);

    /// <summary>
    /// List of Roles by User Persona ID
    /// </summary>
    /// <param name="userPersonaId">Persona ID</param>        
    /// <returns>List of Roles assigned to Persona</returns>
    Task<List<ProductRole>> ListRolesAssignedToPersonaAsync(long userPersonaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// List of Roles by Party ID
    /// </summary>
    /// <param name="partyId">Party ID</param>        
    /// <returns>List of Roles</returns>
    Task<List<ProductRole>> ListRolesByPartyAsync(long partyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// List of  Rights by Role
    /// </summary>
    /// <param name="partyId">Party ID</param>   
    /// <param name="productId">Product ID</param>   
    /// <param name="rightId">Role ID</param>   
    /// <returns>List of Roles by RightId and Product</returns>
    Task<List<ProductRole>> ListRolesByRightAsync(long partyId, long productId, long rightId, CancellationToken cancellationToken = default);

    /// <summary>
    /// List of Roles by Party Product
    /// </summary>
    /// <param name="partyId">Party ID</param>   
    /// <param name="productId">Product ID</param>
    /// <param name="productIdList"></param>   
    /// <returns>List of Roles by PartyId and Product</returns>
    Task<List<ProductRole>> ListRolesForProductsByPartyIdAsync(long partyId, long productId, List<int> productIdList, CancellationToken cancellationToken = default);

    /// <summary>
    /// List of Roles with Rights count
    /// </summary>
    /// <param name="partyId">Party ID</param>   
    /// <param name="productId">Product ID</param>
    /// <param name="productIdList"></param>   
    /// <returns>List of Roles and rights count by PartyId and Product</returns>
    Task<List<RightRoleDetail>> ListRoleWithRightsAsync(long partyId, long productId, List<int> productIdList, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new role
    /// </summary>
    /// <param name="roleId">User Role</param>   
    /// <param name="roleName"></param>
    /// <param name="desc">User Persona ID</param>
    /// <param name="userId"></param>
    /// <returns>Add new Role Response</returns>
    Task<RepositoryResponse> UpdateCustomRoleAsync(long roleId, string roleName, string desc, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get persona AD groups by Persona ID
    /// </summary>
    /// <param name="personaId">PERSONAID</param>  
    /// <returns>PersonaADGroup</returns>
    Task<List<PersonaADGroup>> GetPersonaADGroupsAsync(long personaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get organization type AD Groups
    /// </summary>
    /// <returns>OrgTypesADGroups</returns>
    Task<List<OrgTypesADGroups>> GetOrgTypesADGroupsAsync(CancellationToken cancellationToken = default);
    Task<RepositoryResponse> SetDefaultRoleAsync(
        long roleId, long partyId, int userId, CancellationToken cancellationToken = default);
    Task<List<UnifiedLoginCompany>> ListCompaniesAsync(
        string filter = "", string organizationTypeIds = null, CancellationToken cancellationToken = default);

    Task<List<UserDetail>> ListUsersAsync(
        string filter, string organizationTypeIds = null, CancellationToken cancellationToken = default);
}