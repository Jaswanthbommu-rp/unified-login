using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Product.RPDocumentManagement
{
	/// <summary>
	/// A Role in RPDM
	/// </summary>
	public class RPDMRole
	{
		/// <summary>
		/// The id of the role
		/// </summary>
		[JsonProperty("id")]
		public string ID { get; set; }

		/// <summary>
		/// The name of the role
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[JsonProperty(PropertyName = "href")]
		public string HRef { get; set; }
	}
}
