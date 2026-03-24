using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for PartyRole Repository
	/// </summary>
	public interface IPartyRoleRepositoryAsync
	{
		/// <summary>
		/// Create a party Role (User Job Title) by Enterprise UserID
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="partyRole">PartyRole object of the parameter values</param>
		/// <returns>Repository response object</returns>
		Task<RepositoryResponse> CreatePartyRoleEnterpriseUserIdAsync(Guid realPageId, IPartyRole partyRole, CancellationToken cancellationToken = default);

		/// <summary>
		/// Get the person party role (Job Title) by unique identifier
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <returns>PartyRole object</returns>
		Task<PartyRole> GetPartyRoleByEnterpriseUserIdAsync(Guid realPageId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partyId"></param>
        /// <returns></returns>
       Task<IList<PartyRole>> GetPartyRolesAsync(long partyId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update a party Role by Enterprise UserID
        /// </summary>
        /// <param name="partyRole">PartyRole object of the parameter values</param>
        /// <returns>Repository response object</returns>
        Task<RepositoryResponse> UpdatePartyRoleEnterpriseUserIdAsync(IPartyRole partyRole, CancellationToken cancellationToken = default);
	}
}