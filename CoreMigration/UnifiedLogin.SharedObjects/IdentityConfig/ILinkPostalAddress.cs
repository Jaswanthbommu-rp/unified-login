using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for LinkPostalAddress
	/// </summary>
	public interface ILinkPostalAddress
	{

		/// <summary>
		/// Contact Mechanism Boundary
		/// </summary>
		ContactMechanismBoundary ContactMechanismBoundary { get; set; }

		/// <summary>
		/// Contact Mechanism UsageType
		/// </summary>
		ContactMechanismUsageType ContactMechanismUsageType { get; set; }

		/// <summary>
		/// Geographic Boundary
		/// </summary>
		IList<GeographicBoundary> GeographicBoundary { get; set; }

		/// <summary>
		/// Party Contact Mechanism
		/// </summary>
		PartyContactMechanism PartyContactMechanism { get; set; }

		/// <summary>
		/// Street Address
		/// </summary>
		StreetAddress StreetAddress { get; set; }
	}
}