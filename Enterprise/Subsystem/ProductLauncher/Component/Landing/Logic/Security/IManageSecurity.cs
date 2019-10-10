using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.Security;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Security
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