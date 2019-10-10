using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Models
{
    public class UserSelectedAnswer
    {
        public string LoginId { get; set; }
        public IList<UserSelectedAnswerDetails> UserSelectedAnswerDetails { get; set; }
    }

    public class UserSelectedAnswerDetails
    {
        public int QuestionId { get; set; }
        public string Answer { get; set; }
    }
}