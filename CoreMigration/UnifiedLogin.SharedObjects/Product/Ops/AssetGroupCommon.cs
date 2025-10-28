using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Product.Ops
{
    /// <summary>
    /// Common AssetGroup attributes
    /// </summary>
    public class AssetGroupCommon
    {
        /// <summary>
        /// The name of the asset group
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The status of the asset group
        /// </summary>
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        /// <summary>
        /// The description of the asset group
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }
    }
}
