namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Represents the permissions (in terms of scopes) granted to a client by a subject
	/// </summary>
	public class Consent  
	{
		/// <summary>
		/// Gets or sets the subject. UserID
		/// </summary>
		public string SubjectCode { get; set; }

		/// <summary>
		/// Gets or sets the client identifier.
		/// </summary>
		public string ClientCode { get; set; }

		/// <summary>
		/// Gets or sets the scopes.
		/// </summary>
		public string Scopes { get; set; }
    }
}
