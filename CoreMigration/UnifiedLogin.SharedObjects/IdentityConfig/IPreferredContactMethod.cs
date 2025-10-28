namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for Preferred Contact Method
	/// </summary>
	public interface IPreferredContactMethod
	{
		/// <summary>
		/// Preferred Contact Method Name
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Unique Identifier - Preferred Contact Method Id
		/// </summary>
		int PreferredContactMethodId { get; set; }
	}
}