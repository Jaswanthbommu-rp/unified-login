using System;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for Contact Mechanism Boundary
	/// </summary>
	public interface IContactMechanismBoundary
	{
		/// <summary>
		/// Contact Mechanism Boundary unique Id
		/// </summary>
		int ContactMechanismBoundaryId { get; set; }

		/// <summary>
		/// Contact Mechanism unique Id
		/// </summary>
		int ContactMechanismId { get; set; }

		/// <summary>
		/// Contact Mechanism Boundary From Date
		/// </summary>
		DateTime FromDate { get; set; }

		/// <summary>
		/// Geographic Boundary unique Id
		/// </summary>
		int GeographicBoundaryId { get; set; }

		/// <summary>
		/// Contact Mechanism Boundary thru Date
		/// </summary>
		DateTime ThruDate { get; set; }
	}
}