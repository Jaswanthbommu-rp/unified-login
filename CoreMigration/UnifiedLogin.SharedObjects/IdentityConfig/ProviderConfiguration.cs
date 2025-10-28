namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Identity Provider Configuration
	/// </summary>
    public class ProviderConfiguration : IProviderConfiguration
	{
		/// <summary>
		/// Provider unique id
		/// </summary>
        public int ProviderPortfolioId { get; set; }

		/// <summary>
		/// The unique id for the Organization in GreenBook
		/// </summary>
		public int PortfolioIdId { get; set; }

		/// <summary>
		/// Authentication Mode
		/// </summary>
        public int AuthenticationMode { get; set; }

		/// <summary>
		/// Token Validation Parameters: ValidateIssuer
		/// </summary>
		public bool ValidateIssuer { get; set; }

		/// <summary>
		/// Identity Provider Type Name
		/// </summary>
        public string ProviderName { get; set; }

		/// <summary>
		/// Identity Provider Type Description
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Must be a unique value to identify the external identity provider
		/// </summary>
		public string AuthenticationType { get; set; }

		/// <summary>
		/// Specifies the label of the button on the login page for the identity provider
		/// </summary>
		public string Caption { get; set; }

		/// <summary>
		/// A client is a piece of software that requests tokens from IdentityServer
		/// </summary>
		public string ProviderClientId { get; set; }

		/// <summary>
		/// Provider AuthorityUri
		/// </summary>
		public string AuthorityUri { get; set; }

		/// <summary>
		/// Post Logout RedirectUri
		/// </summary>
		public string PostLogoutRedirectUri { get; set; }

		/// <summary>
		/// Redirect users to a registered URI
		/// </summary>
		public string RedirectUri { get; set; }

		/// <summary>
		/// Token Validation AuthenticationType
		/// </summary>
		public string TokenValidationAuthenticationType { get; set; }

		/// <summary>
		/// identifiers for resources that a client wants to access
		/// </summary>
		public string Scope { get; set; }

		/// <summary>
		/// The EntityId
		/// </summary>
		public string EntityId { get; set; }

		/// <summary>
		/// The IDP Metadata Location
		/// </summary>
		public string MetadataLocation { get; set; }

		/// <summary>
		/// Used for authorizing a client
		/// </summary>
		public string ClientSecret { get; set; }

		/// <summary>
		/// Used to store the valid audience for the token
		/// </summary>
		public string ValidAudience { get; set; }

		/// <summary>
		/// Used to store the claim type to use to local user name information for the identity provider
		/// </summary>
		public string UserLoginClaim { get; set; }

        /// <summary>
        /// Used to set if the Authenticate Request Signing Behavior should be signed
        /// </summary>
        public int SigningBehavior { get; set; } = 3; //Never

    }
}
