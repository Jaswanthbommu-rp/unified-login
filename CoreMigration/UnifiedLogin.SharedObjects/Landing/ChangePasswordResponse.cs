using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class ChangePasswordResponse : ResponseBase
    {
        [JsonProperty(PropertyName = "enterpriseUserName")]
        public string EnterpriseUserName { get; set; }


        [JsonProperty(PropertyName = "isSuccess")]
        public bool IsSuccess { get; set; } 
    }
}
