using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
	/// <summary>
	/// Interface for ScopeSecret
	/// </summary>
	public interface IScopeSecret
	{
		/// <summary>
		/// Unique ID of the ScopeSecret
		/// </summary>
		int Id { get; set; }

		/// <summary>
		/// Description of the Secret
		/// </summary>
		string Description { get; set; }

		/// <summary>
		/// A point in time, where this secret will expire
		/// </summary>
		DateTimeOffset? Expiration { get; set; }

		/// <summary>
		/// Some string that gives the secret validator a hint what type of secret to expect (e.g. “SharedSecret” or “X509CertificateThumbprint”)
		/// </summary>
		string Type { get; set; }

		/// <summary>
		/// Secret value
		/// </summary>
		string Value { get; set; }

		/// <summary>
		/// Reference to the Unique ID of the Scope
		/// </summary>
		int ScopeId { get; set; }

		/// <summary>
		/// Scope
		/// </summary>
		Scope Scope { get; set; }
	}
}