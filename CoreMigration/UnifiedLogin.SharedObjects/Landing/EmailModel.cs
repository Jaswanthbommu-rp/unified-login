using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class EmailModel
    {

        public EmailModel()
        {
            To = new List<UserEmail>();
            Cc = new List<UserEmail>();
            Bcc = new List<UserEmail>();
        }

        public string Subject { get; set; }
        public string Body { get; set; }
        public List<UserEmail> To { get; set; }
        public List<UserEmail> Cc { get; set; }
        public List<UserEmail> Bcc { get; set; }
    }
}
