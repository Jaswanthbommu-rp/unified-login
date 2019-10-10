using Newtonsoft.Json;
using System.Collections.Generic;


namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal
{
	/// <summary>
	/// List of collection of data
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class DataList<T> : IDataList<T>
	{
		/// <summary>
		/// List of collection of data
		/// </summary>
		[JsonProperty(PropertyName = "data")]
		public IList<T> data { get; set; }
	}
}
