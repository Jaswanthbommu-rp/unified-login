using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
	/// <summary>
	/// Interface for Scope.cs
	/// </summary>
	public interface IScope
	{
		/// <summary>
		/// Unique ID of the Scope
		/// </summary>
		int ScopeId { get; set; }

		/// <summary>
		/// Indicates if scope is enabled and can be requested. Defaults to true
		/// </summary>
		bool Enabled { get; set; }

		/// <summary>
		/// Name of the scope. This is the value a client will use to request the scope
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Display name for consent screen
		/// </summary>
		string DisplayName { get; set; }

		/// <summary>
		/// Description for the consent screen
		/// </summary>
		string Description { get; set; }

		/// <summary>
		/// Specifies whether the user can de-select the scope on the consent screen. Defaults to false
		/// </summary>
		bool Required { get; set; }

		/// <summary>
		/// Specifies whether the consent screen will emphasize this scope. Use this 
		/// setting for sensitive or important scopes. Defaults to false
		/// </summary>
		bool Emphasize { get; set; }

		/// <summary>
		/// Either Identity (OpenID Connect related) or Resource (OAuth2 resources). Defaults to Resource
		/// </summary>
		int Type { get; set; }

		/// <summary>
		/// If enabled, all claims for the user will be included in the token. Defaults to false
		/// </summary>
		bool IncludeAllClaimsForUser { get; set; }

		/// <summary>
		/// Rule for determining which claims should be included in the token (this is implementation specific)
		/// </summary>
		string ClaimsRule { get; set; }

		/// <summary>
		/// Specifies whether this scope is shown in the discovery document. Defaults to true
		/// </summary>
		bool ShowInDiscoveryDocument { get; set; }

		/// <summary>
		/// Allows this scope to see all other scopes in the access token when using the introspection endpoint
		/// </summary>
		bool AllowUnrestrictedIntrospection { get; set; }

		/// <summary>
		/// Scope can also specify claims that go into the corresponding token
		/// </summary>
		ICollection<ScopeClaim> ScopeClaims { get; set; }

		/// <summary>
		/// Added secret to scope (for the introspection endpoint)
		/// </summary>
		ICollection<ScopeSecret> ScopeSecrets { get; set; }
	}
}