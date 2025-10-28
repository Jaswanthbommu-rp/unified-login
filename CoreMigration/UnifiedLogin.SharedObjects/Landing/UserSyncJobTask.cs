using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class UserSyncJobTask
    {
        public long UserSyncJobId { get; set; }
        public long PersonaId { get; set; }
        public string Source { get; set; }
        public int ProductId { get; set; }
        public int UserSyncJobTypeId { get; set; } 
        public Guid UserOrgRealpageId { get; set; }
    }
}
