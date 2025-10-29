using System.Collections.Generic;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Logic
{
	/// <summary>
	/// Interface for ManageUserLoginPersona
	/// </summary>
	public interface IManageUserLoginPersona
	{
		/// <summary>
		/// List one or more UserLogin Persona records
		/// </summary>
		/// <param name="userLoginPersonaId">Optional userLoginPersonaId</param>
		/// <param name="userLoginId">Optional UserLoginId</param>
		/// <param name="organizationPartyId">Optional Organization PartyId</param>
		/// <returns>List of UserLoginPersona</returns>
		IList<UserLoginPersona> ListUserLoginPersona(long? userLoginPersonaId, long? userLoginId, long? organizationPartyId);
	}
}