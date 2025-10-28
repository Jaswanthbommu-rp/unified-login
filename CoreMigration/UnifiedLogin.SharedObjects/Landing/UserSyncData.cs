using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class UserSyncData
    {
        public long UserPersonaId { get; set; }
        public long EditorPersonaId { get; set; }
        public string ProductSource { get; set; }
        public Guid UserOrgRealpageId { get; set; }
    }
}
