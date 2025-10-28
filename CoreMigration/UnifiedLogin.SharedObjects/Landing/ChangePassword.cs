using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class ChangePassword
    {
        [JsonProperty(PropertyName = "enterpriseUserName")]
        public string EnterpriseUserName { get; set; }

        [JsonProperty(PropertyName = "activityToken")]
        public string ActivityToken { get; set; }

        [JsonProperty(PropertyName = "newPassword")]
        public string NewPassword { get; set; }

        [JsonProperty(PropertyName = "correctAnswerToken")]
        public string CorrectAnswerToken { get; set; }
    }
}
