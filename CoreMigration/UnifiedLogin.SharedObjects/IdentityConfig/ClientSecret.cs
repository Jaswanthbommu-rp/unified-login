using System;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Secrets define how a client can authenticate with IdentityServer.
	/// </summary>
	public class ClientSecret  
	{
		/// <summary>
		/// Unique ID of the client Secret
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The value of the secret. This is being interpreted by the secret validator 
		/// (e.g. a “password”-like share secret or something else that identifies a credential)
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Some string that gives the secret validator a hint what type of secret to 
		/// expect (e.g. “SharedSecret” or “X509CertificateThumbprint”)
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// The description of the secret - useful for attaching some extra information to the secret
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// A point in time, where this secret will expire
		/// </summary>
		public DateTimeOffset Expiration { get; set; }

		/// <summary>
		/// Reference to the Unique ID of the client
		/// </summary>
		public int ClientId { get; set; }
    }
}
