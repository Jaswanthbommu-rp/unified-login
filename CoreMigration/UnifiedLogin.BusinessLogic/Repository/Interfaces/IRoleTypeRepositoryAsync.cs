using System.Collections.Generic;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for RoleType Repository
	/// </summary>
	public interface IRoleTypeRepositoryAsync
	{
		/// <summary>
		/// Get RoleType
		/// </summary>
		/// <param name="roleTypeName">Role Type name</param>
		/// <param name="partyId">The organization to filter the role type if used</param>
		/// <param name="cancellationToken">Optional. A cancellation token</param>
		/// <returns>RoleType object</returns>
		Task<IList<RoleType>> GetRoleTypeAsync(string roleTypeName, long? partyId, CancellationToken cancellationToken = default);

        /// <summary>
		/// Get RoleType Dependency
		/// </summary>
		/// <param name="roleTypeId">Role Type Id</param>
		/// <param name="partyId">The organization to filter the role type if used</param>
		/// <param name="cancellationToken">Optional. A cancellation token</param>
		/// <returns>RoleType object</returns>
        Task<IList<RoleType>> GetRoleTypeDependencyAsync(long? roleTypeId, long? partyId, CancellationToken cancellationToken = default);

    }
}