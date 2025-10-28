namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for Geographic Boundary
	/// </summary>
	public interface IGeographicBoundary
	{
		/// <summary>
		/// Geographic Boundary Name Abbreviation
		/// </summary>
		string Abbreviation { get; set; }

		/// <summary>
		/// Geographic Boundary Code
		/// </summary>
		string GeographicBoundaryCode { get; set; }

		/// <summary>
		/// Geographic Boundary type
		/// </summary>
		GeographicBoundaryType GeographicBoundaryType { get; set; }

		/// <summary>
		/// Geographic Boundary unique type Id
		/// </summary>
		int GeographicBoundaryTypeId { get; set; }

		/// <summary>
		/// Geographic Boundary Name
		/// </summary>
		string Name { get; set; }
	}
}