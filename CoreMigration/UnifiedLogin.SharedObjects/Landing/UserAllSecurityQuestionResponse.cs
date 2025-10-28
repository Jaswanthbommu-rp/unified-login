using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class UserAllSecurityQuestionResponse : ResponseBase
    {
        public string EnterpriseUserName { get; set; }
        public IList<SecurityQuestion> SecurityQuestions { get; set; }
    }
}
