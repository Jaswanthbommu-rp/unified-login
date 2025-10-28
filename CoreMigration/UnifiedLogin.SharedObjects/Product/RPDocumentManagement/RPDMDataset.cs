using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Product.RPDocumentManagement
{
	/// <summary>
	/// The dataset object
	/// </summary>
	public class RPDMDataset
	{
		/// <summary>
		/// The id of the object
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Id { get; set; }

		/// <summary>
		/// The name of the object
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Name { get; set; }

		/// <summary>
		/// The Href to the object
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string HRef { get; set; }

		/// <summary>
		/// The Rel to the object
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Rel { get; set; }

		/// <summary>
		/// Is it assigned
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public bool IsAssigned { get; set; }
	}
}
