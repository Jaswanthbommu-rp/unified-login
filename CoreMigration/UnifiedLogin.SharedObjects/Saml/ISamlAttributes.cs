namespace UnifiedLogin.SharedObjects.Saml
{
	/// <summary>
	/// Interface for SamlAttributes
	/// </summary>
    public interface ISamlAttributes
    {
		/// <summary>
		/// The unique id for the SAML attribute
		/// </summary>
		int SamlAttributeId { get; set; }

		/// <summary>
		/// The name of the SAML attribute
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// The value of the SAML attribute
		/// </summary>
		string Value { get; set; }

		/// <summary>
		/// The data type of the SAML attribute
		/// </summary>
		string Type { get; set; }

		/// <summary>
		/// Unique Saml User Attribute Id
		/// </summary>
		int SamlUserAttributeId { get; set; }
	}
}