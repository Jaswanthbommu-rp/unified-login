using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Product.Ops
{
    /// <summary>
    /// AssetGroup Create
    /// </summary>
    public class AssetGroupCreate : AssetGroupCommon
    {
        /// <summary>
        /// Asset group property list
        /// </summary>
        [JsonProperty("property_list", NullValueHandling = NullValueHandling.Ignore)]
        public AssetGroupProperty property_list { get; set; }
    }
}
