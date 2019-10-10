using Newtonsoft.Json;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Shared.Models
{
    public class ResponseBase
    {
        [JsonProperty(PropertyName = "isError")]
        public bool IsError { get; set; }

        [JsonProperty(PropertyName = "errorReason")]
        public string ErrorReason { get; set; }
    }
}
