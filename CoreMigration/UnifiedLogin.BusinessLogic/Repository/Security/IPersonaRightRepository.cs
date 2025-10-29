using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing.Security;

namespace UnifiedLogin.BusinessLogic.Repository.Security
{
	/// <summary>
	/// 
	/// </summary>
	public interface IPersonaRightRepository
	{
		/// <summary>
		/// List Rights By PersonaId for given route
		/// </summary>
		/// <param name="personaId"></param>
		/// <param name="routeId"></param>
		/// <returns></returns>
		IEnumerable<PersonaActionRight> ListRightsAndActionsByPersonaId(long personaId, string routeId);
	}
}