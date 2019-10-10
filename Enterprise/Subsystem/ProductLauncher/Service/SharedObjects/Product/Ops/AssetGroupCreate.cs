using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops
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
