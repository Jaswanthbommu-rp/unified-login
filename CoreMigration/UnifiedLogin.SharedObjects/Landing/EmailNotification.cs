using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class EmailNotification
    {
        public Guid RealPageId { get; set; }
        public string LoginName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string OrgnizationName { get; set; }
        public string EmailFrom { get; set; }
        public string EmailTo { get; set; }

    }
}
