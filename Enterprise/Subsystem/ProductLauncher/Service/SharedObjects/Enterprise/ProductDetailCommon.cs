using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise
{
    public class ProductDetailCommon
    {
        /// <summary>
        /// ProductId
        /// </summary>
        [JsonProperty(PropertyName = "ProductId")]
        public int ProductId { get; set; }

        /// <summary>
        /// ProductCode
        /// </summary>
        [JsonProperty(PropertyName = "ProductCode")]
        public string ProductCode { get; set; }

        /// <summary>
        /// Company
        /// </summary>
        [JsonProperty("Company", NullValueHandling = NullValueHandling.Ignore)]
        public string Company { get; set; }
    }
}
