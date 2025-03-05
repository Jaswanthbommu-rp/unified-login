using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
    public class ThirdPartyIDPs
    {
        /// <summary>
        /// IDP Name
        /// </summary>
        [JsonProperty(PropertyName = "IDPName")]
        public string IDPName { get; set; }


        /// <summary>  
        /// IsAssigned
        /// </summary>
        [JsonProperty(PropertyName = "IsAssigned")]
        public bool IsAssigned { get; set; } = false;
    }
}