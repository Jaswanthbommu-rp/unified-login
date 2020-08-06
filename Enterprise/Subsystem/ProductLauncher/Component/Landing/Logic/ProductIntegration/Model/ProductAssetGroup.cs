using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model
{
    public class ProductAssetGroup
    {
        /// <summary>
        /// Group id
        /// </summary>
        [JsonProperty(PropertyName = "Id")]
        public int Id { get; set; }

        /// <summary>
        /// Group name
        /// </summary>
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }
    }
}
