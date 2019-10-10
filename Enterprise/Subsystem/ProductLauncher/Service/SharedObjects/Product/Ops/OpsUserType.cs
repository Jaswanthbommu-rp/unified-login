using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops
{
	/// <summary>
	/// Used to store user type info for Ops users
	/// </summary>
	public class OpsUserType
	{
		/// <summary>
		/// The id of the user type
		/// </summary>
		[JsonProperty("id")]
		public string Id { get; set; }

		/// <summary>
		/// The name of the user type
		/// </summary>
		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
		public string Name { get; set; }

		/// <summary>
		/// The description for the user type
		/// </summary>
		[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
		public string Description { get; set; }

	}
}
