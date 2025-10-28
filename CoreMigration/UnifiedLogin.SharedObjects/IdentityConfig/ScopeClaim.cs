namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Scope can also specify claims that go into the corresponding token
	/// </summary>
	public class ScopeClaim  
	{
		/// <summary>
		/// Unique ID of the ScopeClaim
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Name of the claim
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Description of the claim
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Specifies whether this claim should always be present in the identity token (even if an access 
		/// token has been requested as well). Applies to identity scopes only. Defaults to false
		/// </summary>
		public bool AlwaysIncludeInIdToken { get; set; }

		/// <summary>
		/// Reference to the Unique ID of the Scope
		/// </summary>
		public int ScopeId { get; set; }

    }
}
