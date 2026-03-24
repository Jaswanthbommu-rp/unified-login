using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	public interface IUserLoginPersonaRepositoryAsync
	{
		/// <summary>
		/// Create UserLoginPersona
		/// </summary>
		/// <param name="userLoginPersona">userLoginPersona object</param>
		/// <returns>Repository response object</returns>
		Task<RepositoryResponse> CreateUserLoginPersonaAsync(UserLoginPersona userLoginPersona, CancellationToken cancellationToken = default);

		/// <summary>
		/// List one or more UserLogin Persona records
		/// </summary>
		/// <param name="userLoginPersonaId">Optional userLoginPersonaId</param>
		/// <param name="userLoginId">Optional UserLoginId</param>
		/// <param name="organizationPartyId">Optional Organization PartyId</param>
		/// <returns>List of UserLoginPersona</returns>
		Task<IList<UserLoginPersona>> ListUserLoginPersonaAsync(long? userLoginPersonaId, long? userLoginId, long? organizationPartyId, CancellationToken cancellationToken = default);
	}
}