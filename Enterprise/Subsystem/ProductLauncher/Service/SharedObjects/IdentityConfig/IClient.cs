using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
	/// <summary>
	/// Interface for Client.cs
	/// </summary>
	public interface IClient
	{
		/// <summary>
		/// Unique ID of the client
		/// </summary>
		int ClientId { get; set; }

		/// <summary>
		/// Specifies if client is enabled. Defaults to true
		/// </summary>
		bool Enabled { get; set; }

		/// <summary>
		/// Client code
		/// </summary>
		string ClientCode { get; set; }

		/// <summary>
		/// Client display name (used for logging and consent screen)
		/// </summary>
		string ClientName { get; set; }

		/// <summary>
		/// URI to further information about client (used on consent screen)
		/// </summary>
		string ClientUri { get; set; }

		/// <summary>
		/// URI to client logo (used on consent screen)
		/// </summary>
		string LogoUri { get; set; }

		/// <summary>
		/// Specifies whether a consent screen is required. Defaults to true
		/// </summary>
		bool RequireConsent { get; set; }

		/// <summary>
		/// Specifies whether user can choose to store consent decisions. Defaults to true
		/// </summary>
		bool AllowRememberConsent { get; set; }

		/// <summary>
		/// Specifies allowed flow for client (either AuthorizationCode, Implicit, Hybrid, 
		/// ResourceOwner, ClientCredentials or Custom). Defaults to Implicit
		/// </summary>
		int Flow { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this client is allowed to request token 
		/// using client credentials only. This is useful when you want a client to be able 
		/// to use both a user-centric flow like implicit and additionally client credentials 
		/// flow. Defaults to false. Should only be used for confidential clients (e.g. not Implicit)
		/// </summary>
		bool AllowClientCredentialsOnly { get; set; }

		/// <summary>
		/// Specifies logout URI at client for HTTP based logout
		/// </summary>
		string LogoutUri { get; set; }

		/// <summary>
		/// Specifies if the user’s session id should be sent to the LogoutUri. Defaults to true
		/// </summary>
		bool LogoutSessionRequired { get; set; }

		/// <summary>
		/// Specifies if the client will always show a confirmation page for sign-out. Defaults to false
		/// </summary>
		bool RequireSignOutPrompt { get; set; }

		/// <summary>
		/// By default a client has no access to any scopes.  Set AllowAccessToAllScopes 
		/// to true to give the client access to all scopes
		/// </summary>
		bool AllowAccessToAllScopes { get; set; }

		/// <summary>
		/// Lifetime to identity token in seconds (defaults to 300 seconds / 5 minutes)
		/// </summary>
		int IdentityTokenLifetime { get; set; }

		/// <summary>
		/// Lifetime of access token in seconds (defaults to 3600 seconds / 1 hour)
		/// </summary>
		int AccessTokenLifetime { get; set; }

		/// <summary>
		/// Lifetime of authorization code in seconds (defaults to 300 seconds / 5 minutes)
		/// </summary>
		int AuthorizationCodeLifetime { get; set; }

		/// <summary>
		/// Maximum lifetime of a refresh token in seconds. Defaults to 2592000 seconds / 30 days
		/// </summary>
		int AbsoluteRefreshTokenLifetime { get; set; }

		/// <summary>
		/// Sliding lifetime of a refresh token in seconds. Defaults to 1296000 seconds / 15 days
		/// </summary>
		int SlidingRefreshTokenLifetime { get; set; }

		/// <summary>
		/// ReUse: the refresh token handle will stay the same when refreshing tokens
		/// OneTime: the refresh token handle will be updated when refreshing tokens
		/// </summary>
		int RefreshTokenUsage { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the access token (and its claims) should be updated on a refresh token request
		/// </summary>
		bool UpdateAccessTokenOnRefresh { get; set; }

		/// <summary>
		/// Absolute: the refresh token will expire on a fixed point in time (specified by the AbsoluteRefreshTokenLifetime)
		/// Sliding: when refreshing the token, the lifetime of the refresh token will be 
		/// renewed(by the amount specified in SlidingRefreshTokenLifetime).
		/// The lifetime will not exceed AbsoluteRefreshTokenLifetime
		/// </summary>
		int RefreshTokenExpiration { get; set; }

		/// <summary>
		/// Specifies whether the access token is a reference token or a self contained JWT token (defaults to Jwt)
		/// </summary>
		int AccessTokenType { get; set; }

		/// <summary>
		/// Specifies if this client can use local accounts, or external IdPs only. Defaults to true
		/// </summary>
		bool EnableLocalLogin { get; set; }

		/// <summary>
		/// Specifies whether JWT access tokens should have an embedded unique ID (via the jti claim)
		/// </summary>
		bool IncludeJwtId { get; set; }

		/// <summary>
		/// If set, the client claims will be sent for every flow. If not, only for client credentials flow (default is false)
		/// </summary>
		bool AlwaysSendClientClaims { get; set; }

		/// <summary>
		/// If set, all client claims will be prefixed with client_ to make sure they don’t accidentally collide with user claims. Default is true
		/// </summary>
		bool PrefixClientClaims { get; set; }

		/// <summary>
		/// By default a client has no access to any GrantTypes.  Set AllowAccessToAllGrantTypes 
		/// to true to give the client access to all GrantTypes
		/// </summary>
		bool AllowAccessToAllGrantTypes { get; set; }

		/// <summary>
		/// Collection of Client Secrets
		/// </summary>
		IEnumerable<ClientSecret> ClientSecrets { get; set; }

		/// <summary>
		/// Collection of Client RedirectUris
		/// </summary>
		IEnumerable<ClientRedirectUri> ClientRedirectUris { get; set; }

		/// <summary>
		/// Collection of Client Scopes
		/// </summary>
		IEnumerable<ClientScope> ClientScopes { get; set; }
	}
}