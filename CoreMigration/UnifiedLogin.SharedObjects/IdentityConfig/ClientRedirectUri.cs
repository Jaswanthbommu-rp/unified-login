namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Client Redirect Uri
	/// </summary>
    public class ClientRedirectUri  
	{
		/// <summary>
		/// Unique ID of the client Redirect Uri
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Uri
		/// </summary>
        public string Uri { get; set; }

		/// <summary>
		/// Reference to the Unique ID of the client
		/// </summary>
		public int ClientId { get; set; }
    }
}
