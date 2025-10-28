using Newtonsoft.Json;
using System;

namespace UnifiedLogin.SharedObjects.Enterprise
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

        /// <summary>
        /// RealPageId
        /// </summary>
        [JsonProperty("RealPageId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? RealPageId { get; set; }

        /// <summary>
        /// RealPageId
        /// </summary>
        [JsonProperty("UserType", NullValueHandling = NullValueHandling.Ignore)]
        public string UserType { get; set; }
    }
}
