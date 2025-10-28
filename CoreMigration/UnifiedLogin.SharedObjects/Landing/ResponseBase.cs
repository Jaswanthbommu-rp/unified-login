using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class ResponseBase
    {
        [JsonProperty(PropertyName = "isError")]
        public bool IsError { get; set; }

        [JsonProperty(PropertyName = "errorReason")]
        public string ErrorReason { get; set; }
    }
}
