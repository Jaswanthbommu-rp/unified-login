namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for ContactMechanismUsageType
	/// </summary>
	public interface IContactMechanismUsageType
	{
		/// <summary>
		/// Contact Mechanism UsageTypeID
		/// </summary>
		int? ContactMechanismUsageTypeId { get; set; }

		/// <summary>
		/// Contact Mechanism UsageType name
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Contact Mechanism UsageTypeID
		/// </summary>
		int ParentContactMechanismUsageTypeId { get; set; }
	}
}