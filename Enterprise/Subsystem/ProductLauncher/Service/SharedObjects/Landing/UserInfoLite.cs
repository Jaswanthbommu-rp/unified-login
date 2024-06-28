using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    public class UserInfoLite
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LoginName { get; set; }
        public long UserId { get; set; }
        public long SuperVisorUserId { get; set; }
    }
}
