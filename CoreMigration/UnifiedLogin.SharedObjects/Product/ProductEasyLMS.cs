using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Product
{
	/// <summary>
	/// EasyLMS Url
	/// </summary>
	public class ProductEasyLMS : IProductEasyLMS
	{
		/// <summary>
		/// EasyLMS Url
		/// </summary>
		[JsonProperty(PropertyName = "Url")]
		public string Url { get; set; }
	}
}
