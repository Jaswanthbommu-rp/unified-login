namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
	/// <summary>
	/// Interface for ScopeClaim.cs
	/// </summary>
	public interface IScopeClaim
	{
		/// <summary>
		/// Unique ID of the ScopeClaim
		/// </summary>
		int Id { get; set; }

		/// <summary>
		/// Name of the claim
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Description of the claim
		/// </summary>
		string Description { get; set; }

		/// <summary>
		/// Specifies whether this claim should always be present in the identity token (even if an access 
		/// token has been requested as well). Applies to identity scopes only. Defaults to false
		/// </summary>
		bool AlwaysIncludeInIdToken { get; set; }

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