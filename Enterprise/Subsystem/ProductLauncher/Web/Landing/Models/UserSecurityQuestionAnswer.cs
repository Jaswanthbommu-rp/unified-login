using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Models
{
    public class UserSecurityQuestionAnswer
    {
        public int UserId { get; set; }
        public string ValidationToken { get; set; }
        public IList<QuestionAnser> QuestionAnswer { get; set; }
        public string LoginId { get; set; }
    }
}