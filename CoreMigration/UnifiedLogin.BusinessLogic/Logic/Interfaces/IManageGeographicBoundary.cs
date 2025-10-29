using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
	/// <summary>
	/// Interface for ManageGeographicBoundary
	/// </summary>
	public interface IManageGeographicBoundary
	{
		/// <summary>
		/// Create Geographic Boundary for a Person (e.g. City: Richardson)
		/// </summary>
		/// <param name="geographicBoundary">GeographicBoundary Object.</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse CreateGeographicBoundary(IGeographicBoundary geographicBoundary);
	}
}