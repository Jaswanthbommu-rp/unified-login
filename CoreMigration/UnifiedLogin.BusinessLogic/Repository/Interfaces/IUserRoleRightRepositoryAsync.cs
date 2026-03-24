using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Used to get product role/right information for the given user, product, role
	/// </summary>
	public interface IUserRoleRightRepositoryAsync
	{
        /// <summary>
        /// List of Roles by User Persona ID
        /// </summary>
        /// <param name="productId">Product ID</param>  
        /// <param name="userPersonaId">Optional Persona ID</param>   
        /// <param name="organizationPartyId">Optional Organization PartyId</param>  
        /// <returns>List of roles assigned to Persona</returns>
        //public List<UL.Role> ListRoleByPersona(long? userPersonaId = null, int productId, long? organizationPartyId = null)
        Task<List<Role>> ListRoleByPersonaAsync(int productId, long? userPersonaId = null, long? organizationPartyId = null);

		/// <summary>
		/// Get a single role id for a given persona and product id
		/// </summary>
		/// <param name="userPersonaId">Persona ID</param>   
		/// <param name="productId">Product ID</param>   
		/// <returns>Role assigned to Persona</returns>
		Task<long> GetRoleIdByPersonaAsync(long userPersonaId, int productId);

		/// <summary>
		/// Get list role ids for a given persona and product id
		/// </summary>
		/// <param name="userPersonaId">Persona ID</param>   
		/// <param name="productId">Product ID</param>   
		/// <returns>Role assigned to Persona</returns>
		Task<List<long>> GetRoleIdsByPersonaAsync(long userPersonaId, int productId);

		/// <summary>
		/// Insert Role to User
		/// </summary>
		/// <param name="userPersonaId">User Persona ID</param>             
		/// <param name="roleId">User Role</param>   
		/// <param name="deleteRole">isDeleted</param>   
		/// <param name="userId"></param>
		/// <returns>List of Roles assigned to Persona</returns>
		Task<RepositoryResponse> InsertAssignedRoleToUserAsync(long userPersonaId, long roleId, int userId, bool deleteRole = false);

		/// <summary>
		/// Get all roles and associated rights in master-detail hierarchy 
		/// </summary>
		/// <param name="partyId"></param>
		/// <param name="productIdList"></param>
		/// <param name="productId"></param>
		/// <returns></returns>
		Task<IList<UserRoleRights>> GetAllRoleRightsAsync(long partyId, IList<int> productIdList, int productId);

		/// <summary>
		/// Get all roles and associated rights in master-detail hirerachy 
		/// </summary>
		/// <param name="partyId"></param>
		/// <param name="productIdList"></param>
		/// <param name="productId"></param>
		/// <returns></returns>
		Task<IList<UnifiedLoginRoleRights>> GetPlatformRoleRightsAsync(long partyId, IList<int> productIdList, int productId);

		/// <summary>
		/// Get ADGroup rights for a persona — used by the rights engine.
		/// </summary>
		Task<IList<Right>> GetADGroupRightsByPersonaIdAsync(
			long personaId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Get all rights that must persist during impersonation.
		/// </summary>
		Task<IList<Right>> GetPersistRightsAsync(
			CancellationToken cancellationToken = default);
	}
}