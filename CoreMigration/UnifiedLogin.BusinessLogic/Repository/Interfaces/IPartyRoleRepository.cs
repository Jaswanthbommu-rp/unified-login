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
	public interface IPartyRoleRepository
	{
		/// <summary>
		/// Create a party Role (User Job Title) by Enterprise UserID
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="partyRole">PartyRole object of the parameter values</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse CreatePartyRoleEnterpriseUserID(Guid realPageId, IPartyRole partyRole);

		/// <summary>
		/// Get the person party role (Job Title) by unique identifier
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <returns>PartyRole object</returns>
		PartyRole GetPartyRoleByEnterpriseUserID(Guid realPageId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partyId"></param>
        /// <returns></returns>
        IList<PartyRole> GetPartyRoles(long partyId);

        /// <summary>
        /// Update a party Role by Enterprise UserID
        /// </summary>
        /// <param name="partyRole">PartyRole object of the parameter values</param>
        /// <returns>Repository response object</returns>
        RepositoryResponse UpdatePartyRoleEnterpriseUserID(IPartyRole partyRole);
	}
}