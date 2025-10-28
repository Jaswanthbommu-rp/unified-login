namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for StreetAddress
	/// </summary>
	public interface IStreetAddress
	{
		/// <summary>
		/// Contact Contact Mechanism unique Id
		/// </summary>
		int ContactMechanismId { get; set; }

		/// <summary>
		/// Street Address1
		/// </summary>
		string StreetAddress1 { get; set; }

		/// <summary>
		/// Street Address2
		/// </summary>
		string StreetAddress2 { get; set; }

		/// <summary>
		/// Street Address3
		/// </summary>
		string StreetAddress3 { get; set; }
	}
}