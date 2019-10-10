using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
    public class UserSecurityQuestionAnswer
    {
        public int UserId { get; set; }
        public string ValidationToken { get; set; }
        public IList<QuestionAnswer> QuestionAnswer { get; set; }
        public string LoginId { get; set; }
    }
}