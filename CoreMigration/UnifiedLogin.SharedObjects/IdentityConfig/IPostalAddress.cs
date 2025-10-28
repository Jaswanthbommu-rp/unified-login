namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for PostalAddress
	/// </summary>
	public interface IPostalAddress
	{
		/// <summary>
		/// Contact AddressString: Postal Address
		/// </summary>
		string AddressString { get; set; }

		/// <summary>
		/// Contact AddressType: Street Address
		/// </summary>
		string AddressType { get; set; }

		/// <summary>
		/// Contact Mechanism unique Id
		/// </summary>
		int ContactMechanismId { get; set; }

		/// <summary>
		/// Contact Mechanism usage type detail
		/// </summary>
		ContactMechanismUsageType contactMechanismUsageType { get; set; }

		/// <summary>
		/// Party Contact Mechanism unique Id
		/// </summary>
		long PartyContactMechanismId { get; set; }

		/// <summary>
		/// Contact Mechanism usage type Id
		/// </summary>
		int ContactMechanismUsageTypeId { get; set; }
	}
}