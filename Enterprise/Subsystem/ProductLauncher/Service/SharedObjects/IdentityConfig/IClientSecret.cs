using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
	public interface IClientSecret
	{
		/// <summary>
		/// Unique ID of the client Secret
		/// </summary>
		int Id { get; set; }

		/// <summary>
		/// The value of the secret. This is being interpreted by the secret validator 
		/// (e.g. a “password”-like share secret or something else that identifies a credential)
		/// </summary>
		string Value { get; set; }

		/// <summary>
		/// Some string that gives the secret validator a hint what type of secret to 
		/// expect (e.g. “SharedSecret” or “X509CertificateThumbprint”)
		/// </summary>
		string Type { get; set; }

		/// <summary>
		/// The description of the secret - useful for attaching some extra information to the secret
		/// </summary>
		string Description { get; set; }

		/// <summary>
		/// A point in time, where this secret will expire
		/// </summary>
		DateTimeOffset Expiration { get; set; }

		/// <summary>
		/// Reference to the Unique ID of the client
		/// </summary>
		int ClientId { get; set; }

	}
}