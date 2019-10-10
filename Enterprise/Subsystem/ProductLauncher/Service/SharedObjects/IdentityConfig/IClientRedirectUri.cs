namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
	/// <summary>
	/// Interface for ClientRedirectUri.cs
	/// </summary>
	public interface IClientRedirectUri
	{
		/// <summary>
		/// Unique ID of the client Redirect Uri
		/// </summary>
		int Id { get; set; }

		/// <summary>
		/// Uri
		/// </summary>
		string Uri { get; set; }

		/// <summary>
		/// Reference to the Unique ID of the client
		/// </summary>
		int ClientId { get; set; }
	}
}