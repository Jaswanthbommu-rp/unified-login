using System.Collections.Generic;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for RoleType Repository
	/// </summary>
	public interface IRoleTypeRepository
	{
		/// <summary>
		/// Get RoleType
		/// </summary>
		/// <param name="roleTypeName">Role Type name</param>
		/// <param name="partyId">The organization to filter the role type if used</param>
		/// <returns>RoleType object</returns>
		IList<RoleType> GetRoleType(string roleTypeName, long? partyId);

        /// <summary>
		/// Get RoleType Dependency
		/// </summary>
		/// <param name="userTypeId">User Type Id</param>
		/// <param name="partyId">The organization to filter the role type if used</param>
		/// <returns>RoleType object</returns>
        IList<RoleType> GetRoleTypeDependency(long? userTypeId, long? partyId);

    }
}