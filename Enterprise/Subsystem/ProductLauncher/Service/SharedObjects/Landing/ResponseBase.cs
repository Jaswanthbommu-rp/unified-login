using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    public class ResponseBase
    {
        [JsonProperty(PropertyName = "isError")]
        public bool IsError { get; set; }

        [JsonProperty(PropertyName = "errorReason")]
        public string ErrorReason { get; set; }
    }
}
