using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class UserSecurityAnswer
    {
        public IList<SecurityQuestionAnswer> SecurityQuestionAnswers { get; set; }
        public string EnterpriseUserName { get; set; }
        public string ActivityToken { get; set; }
    }
}
