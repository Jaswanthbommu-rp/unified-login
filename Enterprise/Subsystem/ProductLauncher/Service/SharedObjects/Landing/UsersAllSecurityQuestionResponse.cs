using System.Collections.Generic;
using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
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
