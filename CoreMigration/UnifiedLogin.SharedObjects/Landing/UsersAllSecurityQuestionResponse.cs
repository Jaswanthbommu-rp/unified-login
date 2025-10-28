using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// 
    /// </summary>
    public class UsersAllSecurityQuestionResponse : ResponseBase
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(PropertyName = "securityQuestions")]
        public IList<SecurityQuestion> SecurityQuestions { get; set; }
    }
}
