using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Hots
{
    public class HotsUser
    {
        public long BaselineUserId { get; set; }
        public string BaselineUserName { get; set; }
        public long CloneUserId { get; set; }
        public string CloneUserName { get; set; }
        public string ClonePassword { get; set; }
    }

    public class ClonedUsers
    {
        public string Status { get; set; }
        public Guid CloneCustomerCompanyId { get; set; }
        public string CloneCustomerEnvironment { get; set; }
        public List<HotsUser> Users { get; set; }
    }
}
