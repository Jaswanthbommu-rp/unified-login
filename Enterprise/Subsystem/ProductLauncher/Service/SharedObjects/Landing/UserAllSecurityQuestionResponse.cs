using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    public class UserAllSecurityQuestionResponse : ResponseBase
    {
        public string EnterpriseUserName { get; set; }
        public IList<SecurityQuestion> SecurityQuestions { get; set; }
    }
}
