using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Product.ResidentPortal
{
	/// <summary>
	/// Manager custom title
	/// </summary>
	public class Title : ITitle
	{
		/// <summary>
		/// Manager custom title Id
		/// </summary>
		[JsonProperty(PropertyName = "Id")]
		public string Id { get; set; }

		/// <summary>
		/// Manager custom title Name
		/// </summary>
		[JsonProperty(PropertyName = "Name")]
		public string Name { get; set; }
	}
}
