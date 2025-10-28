using System;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for Party Contact Mechanism
	/// </summary>
	public interface IPartyContactMechanism
	{
		/// <summary>
		/// Contact Contact Mechanism unique Id
		/// </summary>
		int ContactMechanismId { get; set; }

		/// <summary>
		/// Contact Mechanism From Date
		/// </summary>
		DateTime? FromDate { get; set; }

		/// <summary>
		/// Party Contact Contact Mechanism unique Id
		/// </summary>
		long PartyContactMechanismId { get; set; }

		/// <summary>
		/// PartyId
		/// </summary>
		long PartyId { get; set; }

		/// <summary>
		/// Contact Mechanism thru Date
		/// </summary>
		DateTime? ThruDate { get; set; }
	}
}