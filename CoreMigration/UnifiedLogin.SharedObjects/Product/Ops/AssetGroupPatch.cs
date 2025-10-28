using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Product.Ops
{
	/// <summary>
	/// AssetGroup Patch
	/// </summary>
	public class AssetGroupPatch : AssetGroupCommon
	{
		/// <summary>
		/// Asset group property list to add
		/// </summary>
		[JsonProperty("new_property_list", NullValueHandling = NullValueHandling.Ignore)]
		public AssetGroupProperty new_property_list { get; set; }
		/// <summary>
		/// Asset group property list to remove
		/// </summary>
		[JsonProperty("unassigned_property_list", NullValueHandling = NullValueHandling.Ignore)]
		public AssetGroupProperty unassigned_property_list { get; set; }
	}
}
