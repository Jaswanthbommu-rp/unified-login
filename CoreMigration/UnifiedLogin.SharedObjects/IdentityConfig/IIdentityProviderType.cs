namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    /// <summary>
    /// Interface for IdentityProviderType
    /// </summary>
    public interface IIdentityProviderType
    {
        /// <summary>
        /// Authentication Type
        /// </summary>
        string AuthenticationType { get; set; }

		/// <summary>
		/// Identity Provider Type value Contact Mechanism unique Id. 
		/// e.g. local = 46, oktacamden = 47, Google = 77,....
		/// </summary>
		int ContactMechanismId { get; set; }

		/// <summary>
		/// Get value  indicating whether identity provider is RP System
		/// </summary>
		bool IsLocal { get; }
	}
}