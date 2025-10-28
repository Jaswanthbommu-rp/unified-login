using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    public class UserProductDetailLogin : ProductDetailCommon
    {
        /// <summary>
        /// Details
        /// </summary>
        [JsonProperty(PropertyName = "Details")]
        public IList<Dictionary<string, string>> Details { get; set; }
    }
}
