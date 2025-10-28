using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class SecurityQuestionResponse : ResponseBase
    {
        [JsonProperty(PropertyName = "enterpriseUserName")]
        public string EnterpriseUserName { get; set; }

        [JsonProperty(PropertyName = "securityQuestions")]
        public IList<SecurityQuestion> SecurityQuestions { get; set; }

        [JsonProperty(PropertyName = "isUserExist")]
        public bool IsUserExist { get; set; }

        [JsonProperty(PropertyName = "isUserActive")]
        public bool IsUserActive { get; set; }

        [JsonProperty(PropertyName = "isUserLocked")]
        public bool IsUserLocked { get; set; }

        [JsonProperty(PropertyName = "isUserExpired")]
        public bool IsUserExpired { get; set; }

        [JsonProperty(PropertyName = "isUserTainted")]
        public bool IsUserTainted { get; set; }

        [JsonProperty(PropertyName = "isUserPending")]
        public bool IsUserPending { get; set; }
        
        [JsonProperty(PropertyName = "activityToken")]
        public string ActivityToken { get; set; }

        //[JsonProperty(PropertyName = "isLocked")]
        //public bool IsLocked { get; set; }

        //[JsonProperty(PropertyName = "lockReason")]
        //public string LockReason { get; set; }
    }
}
