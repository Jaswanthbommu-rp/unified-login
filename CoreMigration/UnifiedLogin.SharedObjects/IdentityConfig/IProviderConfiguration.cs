namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	public interface IProviderConfiguration
	{
		/// <summary>
		/// Provider unique id
		/// </summary>
		int ProviderPortfolioId { get; set; }

		/// <summary>
		/// The unique id for the Organization in GreenBook
		/// </summary>
		int PortfolioIdId { get; set; }

		/// <summary>
		/// Authentication Mode
		/// </summary>
		int AuthenticationMode { get; set; }

		/// <summary>
		/// Token Validation Parameters: ValidateIssuer
		/// </summary>
		bool ValidateIssuer { get; set; }

		/// <summary>
		/// Identity Provider Type Name
		/// </summary>
		string ProviderName { get; set; }

		/// <summary>
		/// Identity Provider Type Description
		/// </summary>
		string Description { get; set; }

		/// <summary>
		/// Must be a unique value to identify the external identity provider
		/// </summary>
		string AuthenticationType { get; set; }

		/// <summary>
		/// Specifies the label of the button on the login page for the identity provider
		/// </summary>
		string Caption { get; set; }

		/// <summary>
		/// A client is a piece of software that requests tokens from IdentityServer
		/// </summary>
		string ProviderClientId { get; set; }

		/// <summary>
		/// Provider AuthorityUri
		/// </summary>
		string AuthorityUri { get; set; }

		/// <summary>
		/// Post Logout RedirectUri
		/// </summary>
		string PostLogoutRedirectUri { get; set; }

		/// <summary>
		/// Redirect users to a registered URI
		/// </summary>
		string RedirectUri { get; set; }

		/// <summary>
		/// Token Validation AuthenticationType
		/// </summary>
		string TokenValidationAuthenticationType { get; set; }

		/// <summary>
		/// identifiers for resources that a client wants to access
		/// </summary>
		string Scope { get; set; }

		/// <summary>
		/// Okta EntityId
		/// </summary>
		string EntityId { get; set; }

		/// <summary>
		/// Okta Metadata Location
		/// </summary>
		string MetadataLocation { get; set; }

		/// <summary>
		/// Used for authorizing a client
		/// </summary>
		string ClientSecret { get; set; }
	}
}