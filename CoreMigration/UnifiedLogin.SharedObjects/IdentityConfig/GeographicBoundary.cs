using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Geographic Boundary
	/// </summary>
	public class GeographicBoundary : GeographicBoundaryType, IGeographicBoundary
	{
		/// <summary>
		/// Geographic Boundary unique Id
		/// </summary>
		[JsonProperty(PropertyName = "GeographicBoundaryId")]
		public int GeographicBoundaryId { get; set; }

		/// <summary>
		/// Geographic Boundary type
		/// </summary>
		[JsonProperty(PropertyName = "GeographicBoundaryType")]
		public GeographicBoundaryType GeographicBoundaryType { get; set; }

		/// <summary>
		/// Geographic Boundary Name
		/// </summary>
		[JsonProperty(PropertyName = "Name")]
		public string Name { get; set; }

		/// <summary>
		/// Geographic Boundary Code
		/// </summary>
		[JsonProperty(PropertyName = "GeographicBoundaryCode")]
		public string GeographicBoundaryCode { get; set; }

		/// <summary>
		/// Geographic Boundary Name Abbreviation
		/// </summary>
		[JsonProperty(PropertyName = "Abbreviation")]
		public string Abbreviation { get; set; }
	}
}
