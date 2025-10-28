using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Resources that a client wants to access
	/// </summary>
	public class Scope  
	{

		/// <summary>
		/// Constructor
		/// </summary>
        public Scope()
        {
            this.ScopeClaims = new HashSet<ScopeClaim>();
            this.ScopeSecrets = new HashSet<ScopeSecret>();
        }

		/// <summary>
		/// Unique ID of the Scope
		/// </summary>
		public int ScopeId { get; set; }

		/// <summary>
		/// Indicates if scope is enabled and can be requested. Defaults to true
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// Name of the scope. This is the value a client will use to request the scope
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Display name for consent screen
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Description for the consent screen
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Specifies whether the user can de-select the scope on the consent screen. Defaults to false
		/// </summary>
		public bool Required { get; set; }

		/// <summary>
		/// Specifies whether the consent screen will emphasize this scope. Use this 
		/// setting for sensitive or important scopes. Defaults to false
		/// </summary>
		public bool Emphasize { get; set; }

		/// <summary>
		/// Either Identity (OpenID Connect related) or Resource (OAuth2 resources). Defaults to Resource
		/// </summary>
		public int Type { get; set; }

		/// <summary>
		/// If enabled, all claims for the user will be included in the token. Defaults to false
		/// </summary>
		public bool IncludeAllClaimsForUser { get; set; }

		/// <summary>
		/// Rule for determining which claims should be included in the token (this is implementation specific)
		/// </summary>
		public string ClaimsRule { get; set; }

		/// <summary>
		/// Specifies whether this scope is shown in the discovery document. Defaults to true
		/// </summary>
		public bool ShowInDiscoveryDocument { get; set; }

		/// <summary>
		/// Allows this scope to see all other scopes in the access token when using the introspection endpoint
		/// </summary>
		public bool AllowUnrestrictedIntrospection { get; set; }

		/// <summary>
		/// Scope can also specify claims that go into the corresponding token
		/// </summary>
		public virtual ICollection<ScopeClaim> ScopeClaims { get; set; }

		/// <summary>
		/// Collection scope's secrets (for the introspection endpoint)
		/// </summary>
		public virtual ICollection<ScopeSecret> ScopeSecrets { get; set; }
    }
}
