namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for Geographic Boundary Type
	/// </summary>
	public interface IGeographicBoundaryType
	{
		/// <summary>
		/// Geographic Boundary unique type Id
		/// </summary>
		int GeographicBoundaryTypeId { get; set; }

		/// <summary>
		/// Geographic Boundary Type Name
		/// </summary>
		string TypeName { get; set; }
	}
}