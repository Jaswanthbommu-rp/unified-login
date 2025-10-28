using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Landing
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