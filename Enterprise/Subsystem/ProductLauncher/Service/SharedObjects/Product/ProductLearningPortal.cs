using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product
{
	/// <summary>
	/// Product Learning Portal Url
	/// </summary>
	public class ProductLearningPortal : IProductLearningPortal
	{
		/// <summary>
		/// Product Learning Portal Url
		/// </summary>
		[JsonProperty(PropertyName = "Url")]
		public string Url { get; set; }
	}
}
