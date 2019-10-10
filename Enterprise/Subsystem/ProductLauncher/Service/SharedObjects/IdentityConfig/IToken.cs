using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
	/// <summary>
	/// Interface for Token.cs
	/// </summary>
	public interface IToken
	{
		/// <summary>
		/// Unique token key
		/// </summary>
		string TokenKey { get; set; }

		/// <summary>
		/// AuthorizationCode = 1, TokenHandle = 2, RefreshToken = 3
		/// </summary>
		int TokenType { get; set; }

		/// <summary>
		/// Client code
		/// </summary>
		string ClientCode { get; set; }

		/// <summary>
		/// Gets the subject identifier - UserID
		/// </summary>
		string SubjectCode { get; set; }

		/// <summary>
		/// Token Expiry
		/// </summary>
		DateTimeOffset Expiry { get; set; }

		/// <summary>
		/// 
		/// </summary>
		string JsonCode { get; set; }

		/// <summary>
		/// AuthCodeChallenge
		/// </summary>
		string AuthCodeChallenge { get; set; }

		/// <summary>
		/// AuthCodeChallengeMethod
		/// </summary>
		string AuthCodeChallengeMethod { get; set; }

		/// <summary>
		/// Is this an OpenID authorization request?
		/// </summary>
		bool? IsOpenId { get; set; }

		/// <summary>
		/// Gets or sets the nonce.
		/// </summary>
		string Nonce { get; set; }

		/// <summary>
		/// Gets or sets the redirect URI.
		/// </summary>
		string RedirectUri { get; set; }

		/// <summary>
		/// 
		/// </summary>
		string SessionId { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether consent was shown
		/// </summary>
		bool? WasConsentShown { get; set; }
	}
}