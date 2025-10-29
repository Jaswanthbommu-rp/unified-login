using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;

namespace UnifiedLogin.BusinessLogic.Logic.Security
{
	/// <summary>
	/// Interface for Manage Security
	/// </summary>
	public interface IManageSecurity
	{
		/// <summary>
		/// IManageSecurity
		/// </summary>
		/// <param name="personaId"></param>
		/// <param name="routeId"></param>
		/// <returns></returns>
		ObjectOutput<RouteSecurity, IErrorData> GetPersonaRightsAndActionsByRoute(long personaId, string routeId);
	}
}