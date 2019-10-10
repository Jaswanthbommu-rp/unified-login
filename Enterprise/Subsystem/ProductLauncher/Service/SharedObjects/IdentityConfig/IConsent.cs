namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
	/// <summary>
	/// Iterface for Consent.cs
	/// </summary>
	public interface IConsent
	{
		/// <summary>
		/// Gets or sets the subject. UserID
		/// </summary>
		string SubjectCode { get; set; }

		/// <summary>
		/// Gets or sets the client identifier.
		/// </summary>
		string ClientCode { get; set; }

		/// <summary>
		/// Gets or sets the scopes.
		/// </summary>
		string Scopes { get; set; }
	}
}