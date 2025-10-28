namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for ElectronicAddress.cs
	/// </summary>
	public interface IElectronicAddress
	{
		/// <summary>
		/// Contact Contact Mechanism unique Id
		/// </summary>
		int ContactMechanismId { get; set; }

		/// <summary>
		/// Contact AddressString: Electronic address
		/// </summary>
		string AddressString { get; set; }

		/// <summary>
		/// Contact AddressType: Email
		/// </summary>
		string AddressType { get; set; }

		/// <summary>
		/// Party Contact Mechanism unique Id
		/// </summary>
		long PartyContactMechanismId { get; set; }
		
		/// <summary>
		/// Contact Mechanism usage type Id
		/// </summary>
		int ContactMechanismUsageTypeId { get; set; }

		/// <summary>
		/// Contact Mechanism usage type detail
		/// </summary>
		ContactMechanismUsageType contactMechanismUsageType { get; set; }
	}
}