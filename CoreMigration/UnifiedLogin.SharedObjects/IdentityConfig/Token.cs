using System;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Token represents the outcome of an authentication process
	/// </summary>
	public class Token  
	{
		/// <summary>
		/// Unique token key
		/// </summary>
		public string TokenKey { get; set; }

		/// <summary>
		/// AuthorizationCode = 1, TokenHandle = 2, RefreshToken = 3
		/// </summary>
        public int TokenType { get; set; }

		/// <summary>
		/// Client code
		/// </summary>
		public string ClientCode { get; set; }

		/// <summary>
		/// Gets the subject identifier - UserID
		/// </summary>
		public string SubjectCode { get; set; }

		/// <summary>
		/// Token Expiry
		/// </summary>
        public DateTimeOffset Expiry { get; set; }

		/// <summary>
		/// 
		/// </summary>
        public string JsonCode { get; set; }

		/// <summary>
		/// AuthCodeChallenge
		/// </summary>
        public string AuthCodeChallenge { get; set; }

		/// <summary>
		/// AuthCodeChallengeMethod
		/// </summary>
        public string AuthCodeChallengeMethod { get; set; }

		/// <summary>
		/// Is this an OpenID authorization request?
		/// </summary>
		public bool? IsOpenId { get; set; }

		/// <summary>
		/// Gets or sets the nonce.
		/// </summary>
		public string Nonce { get; set; }

		/// <summary>
		/// Gets or sets the redirect URI.
		/// </summary>
		public string RedirectUri { get; set; }

		/// <summary>
		/// 
		/// </summary>
        public string SessionId { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether consent was shown
		/// </summary>
		public bool? WasConsentShown { get; set; }
    }
}
