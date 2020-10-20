using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise
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
