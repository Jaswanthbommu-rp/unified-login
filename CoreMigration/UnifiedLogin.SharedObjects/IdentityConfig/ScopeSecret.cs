using System;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Collection scope's secrets (for the introspection endpoint)
	/// </summary>
	public class ScopeSecret  
	{
		/// <summary>
		/// Unique ID of the ScopeSecret
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Description of the Secret
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// A point in time, where this secret will expire
		/// </summary>
		public DateTimeOffset? Expiration { get; set; }

		/// <summary>
		/// Some string that gives the secret validator a hint what type of secret to expect (e.g. “SharedSecret” or “X509CertificateThumbprint”)
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Secret value
		/// </summary>
        public string Value { get; set; }

		/// <summary>
		/// Reference to the Unique ID of the Scope
		/// </summary>
		public int ScopeId { get; set; }
    }
}