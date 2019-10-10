using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    public class ChangePasswordResponse : ResponseBase
    {
        [JsonProperty(PropertyName = "enterpriseUserName")]
        public string EnterpriseUserName { get; set; }


        [JsonProperty(PropertyName = "isSuccess")]
        public bool IsSuccess { get; set; } 
    }
}
