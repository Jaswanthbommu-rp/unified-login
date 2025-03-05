using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    public class IDPNames
    {

        /// <summary>
        /// Identity Provider type Name
        /// </summary>
        [JsonProperty(PropertyName = "IDPName")]
        public string IDPName { get; set; }

        /// <summary>
        /// Identity Provider Type value Contact Mechanism unique Id. 
        /// e.g. local = 46, oktacamden = 47, Google = 77,....
        /// </summary>
        [JsonProperty("ContactMechanismId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int ContactMechanismId { get; set; }
    }
}