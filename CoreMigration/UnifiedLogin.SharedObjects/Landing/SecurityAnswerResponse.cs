using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class SecurityAnswerResponse : ResponseBase
    {
        [JsonProperty(PropertyName = "enterpriseUserName")]
        public string EnterpriseUserName { get; set; }

        [JsonProperty(PropertyName = "isAnswersCorrect")]
        public bool IsAnswersCorrect { get; set; }

        [JsonProperty(PropertyName = "correctAnswerToken")]
        public string CorrectAnswerToken { get; set; }

        [JsonProperty(PropertyName = "securityQuestions")]
        public IList<SecurityQuestion> SecurityQuestions { get; set; }
    }
}
