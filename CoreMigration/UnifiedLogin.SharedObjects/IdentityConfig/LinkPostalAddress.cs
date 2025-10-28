using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Used as the input object to link a postal address to a person
	/// </summary>
	public class LinkPostalAddress : ILinkPostalAddress
	{
		/// <summary>
		/// Party Contact Mechanism
		/// </summary>
		[JsonProperty(PropertyName = "PartyContactMechanism")]
		public PartyContactMechanism PartyContactMechanism { get; set; }

		/// <summary>
		/// Contact Mechanism UsageType
		/// </summary>
		[JsonProperty(PropertyName = "ContactMechanismUsageType")]
		public ContactMechanismUsageType ContactMechanismUsageType { get; set; }

		/// <summary>
		/// Street Address
		/// </summary>
		[JsonProperty(PropertyName = "StreetAddress")]
		public StreetAddress StreetAddress { get; set; }

		/// <summary>
		/// Contact Mechanism Boundary
		/// </summary>
		[JsonProperty(PropertyName = "ContactMechanismBoundary")]
		public ContactMechanismBoundary ContactMechanismBoundary { get; set; }

		/// <summary>
		/// Geographic Boundary
		/// </summary>
		[JsonProperty(PropertyName = "GeographicBoundary")]
		public IList<GeographicBoundary> GeographicBoundary { get; set; }
	}
}
