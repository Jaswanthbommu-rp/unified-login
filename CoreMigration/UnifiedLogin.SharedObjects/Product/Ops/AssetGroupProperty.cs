using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Product.Ops
{
    /// <summary>
    /// AssetGroup create a list of Properties Ids OR Codes
    /// </summary>
    public class AssetGroupProperty
    {
        /// <summary>
        /// Asset group property list
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public IList<string> Id { get; set; }

        /// <summary>
        /// Asset group property list
        /// </summary>
        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public IList<string> Code { get; set; }
    }
}
