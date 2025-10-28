using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    public class UserProductDetailAttribute : ProductDetailCommon
    {
        /// <summary>
        /// UserAttributes
        /// </summary>
        [JsonProperty(PropertyName = "UserAttribute")]
        public string UserAttribute { get; set; }
    }
}
