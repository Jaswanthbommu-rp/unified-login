using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
	/// <summary>
	/// Interface for Manage PartyRole repository calls
	/// </summary>
	public interface IManagePartyRole
	{

		/// <summary>
		/// Create a PartyRole
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="partyRole">PartyRole data object</param>
		/// <returns>RepositoryResponse object</returns>
		RepositoryResponse CreatePartyRoleEnterpriseUserID(Guid realPageId, IPartyRole partyRole);

		/// <summary>
		/// Get PartyRole
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <returns>PartyRole object</returns>
		PartyRole GetPartyRole(Guid realPageId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partyId"></param>
        /// <returns></returns>
        IList<PartyRole> GetPartyRoles(long? partyId);

        /// <summary>
        /// Update an existing PartyRole
        /// </summary>
        /// <param name="partyRole">PartyRole object of the parameter values</param>
        /// <returns>RepositoryResponse object</returns>
        RepositoryResponse UpdatePartyRole(IPartyRole partyRole);
	}
}