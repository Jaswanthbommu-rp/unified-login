using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class UserSecurityQuestionAnswer
    {
        public int UserId { get; set; }
        public string ValidationToken { get; set; }
        public IList<QuestionAnswer> QuestionAnswer { get; set; }
        public string LoginId { get; set; }
    }
}