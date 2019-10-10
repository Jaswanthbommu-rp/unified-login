using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal
{
	/// <summary>
	/// Root level of JSON object
	/// </summary>
	public class DataObject<T> : IDataObject<T>
	{
		/// <summary>
		/// Data level
		/// </summary>
		[JsonProperty(PropertyName = "data")]
		public T data { get; set; }
	}
}
