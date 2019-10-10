using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product
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
