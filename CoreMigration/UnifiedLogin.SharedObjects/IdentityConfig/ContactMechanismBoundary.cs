using Newtonsoft.Json;
using System;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Contact Mechanism Boundary
	/// </summary>
	public class ContactMechanismBoundary : IContactMechanismBoundary
	{
		/// <summary>
		/// Contact Mechanism Boundary unique Id
		/// </summary>
		[JsonProperty(PropertyName = "ContactMechanismBoundaryId")]
		public int ContactMechanismBoundaryId { get; set; }

		/// <summary>
		/// Contact Mechanism unique Id
		/// </summary>
		[JsonProperty(PropertyName = "ContactMechanismId")]
		public int ContactMechanismId { get; set; }

		/// <summary>
		/// Geographic Boundary unique Id
		/// </summary>
		[JsonProperty(PropertyName = "GeographicBoundaryId")]
		public int GeographicBoundaryId { get; set; }

		/// <summary>
		/// Contact Mechanism Boundary From Date
		/// </summary>
		[JsonProperty(PropertyName = "FromDate")]
		public DateTime FromDate { get; set; }

		/// <summary>
		/// Contact Mechanism Boundary thru Date
		/// </summary>
		[JsonProperty(PropertyName = "ThruDate")]
		public DateTime ThruDate { get; set; }
	}
}
