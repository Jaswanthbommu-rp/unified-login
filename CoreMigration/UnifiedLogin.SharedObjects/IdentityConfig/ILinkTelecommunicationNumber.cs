namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for Link Telecommunication Number
	/// </summary>
	public interface ILinkTelecommunicationNumber
	{
		/// <summary>
		/// Contact Mechanism UsageType
		/// </summary>
		ContactMechanismUsageType ContactMechanismUsageType { get; set; }

		/// <summary>
		/// Party Contact Mechanism
		/// </summary>
		PartyContactMechanism PartyContactMechanism { get; set; }

		/// <summary>
		/// Telecommunication Number
		/// </summary>
		TelecommunicationNumber TelecommunicationNumber { get; set; }
	}
}