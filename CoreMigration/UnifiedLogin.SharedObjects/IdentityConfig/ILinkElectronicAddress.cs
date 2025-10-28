namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for LinkElectronicAddress
	/// </summary>
	public interface ILinkElectronicAddress
	{
		/// <summary>
		/// Electronic Address
		/// </summary>
		ElectronicAddress ElectronicAddress { get; set; }

		/// <summary>
		/// Contact Mechanism UsageType
		/// </summary>
		ContactMechanismUsageType ContactMechanismUsageType { get; set; }

		/// <summary>
		/// Party Contact Mechanism
		/// </summary>
		PartyContactMechanism PartyContactMechanism { get; set; }
	}
}