using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces
{
	/// <summary>
	/// Used to get product role/right information for the given user, product, role
	/// </summary>
	public interface IUserRoleRightRepository
	{
		/// <summary>
		/// List of Roles by User Persona ID
		/// </summary>
		/// <param name="productId">Product ID</param>  
		/// <param name="userPersonaId">Optional Persona ID</param>   
		/// <param name="organizationPartyId">Optional Organization PartyId</param>  
		/// <returns>List of roles assigned to Persona</returns>
		//public List<UL.Role> ListRoleByPersona(long? userPersonaId = null, int productId, long? organizationPartyId = null)
		List<Role> ListRoleByPersona(int productId, long? userPersonaId = null, long? organizationPartyId = null);

		/// <summary>
		/// Get a single role id for a given persona and product id
		/// </summary>
		/// <param name="userPersonaId">Persona ID</param>   
		/// <param name="productId">Product ID</param>   
		/// <returns>Role assigned to Persona</returns>
		long GetRoleIdByPersona(long userPersonaId, int productId);

		/// <summary>
		/// Get list role ids for a given persona and product id
		/// </summary>
		/// <param name="userPersonaId">Persona ID</param>   
		/// <param name="productId">Product ID</param>   
		/// <returns>Role assigned to Persona</returns>
		List<long> GetRoleIdsByPersona(long userPersonaId, int productId);

		/// <summary>
		/// Insert Role to User
		/// </summary>
		/// <param name="userPersonaId">User Persona ID</param>             
		/// <param name="roleId">User Role</param>   
		/// <param name="deleteRole">isDeleted</param>   
		/// <param name="userId"></param>
		/// <returns>List of Roles assigned to Persona</returns>
		RepositoryResponse InsertAssignedRoleToUser(long userPersonaId, long roleId, int userId, bool deleteRole = false);

		/// <summary>
		/// Get all roles and associated rights in master-detail hierarchy 
		/// </summary>
		/// <param name="partyId"></param>
		/// <param name="productIdList"></param>
		/// <param name="productId"></param>
		/// <returns></returns>
		IList<UserRoleRights> GetAllRoleRights(long partyId, IList<int> productIdList, int productId);

		/// <summary>
		/// Get all roles and associated rights in master-detail hirerachy 
		/// </summary>
		/// <param name="partyId"></param>
		/// <param name="productIdList"></param>
		/// <param name="productId"></param>
		/// <returns></returns>
		IList<UnifiedLoginRoleRights> GetPlatFormRoleRights(long partyId, IList<int> productIdList, int productId);


	}
}