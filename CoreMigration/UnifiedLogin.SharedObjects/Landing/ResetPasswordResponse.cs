using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class ResetPasswordResponse : ResponseBase
    {
        public bool IsSuccess { get; set; }

        [JsonIgnore]// used only for logging activity
        public string EnterpriseUserName { get; set; }

        [JsonIgnore]// used only for logging activity
        public long UserId { get; set; }
    }
}
