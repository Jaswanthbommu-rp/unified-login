using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class AdGroupRole
    {
        public int ADGroupRoleId { get; set; }
        public int ADGroupId { get; set; }
        public int RoleId { get; set; }
        public int ProductId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
