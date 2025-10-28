using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    public class UserSyncRequest
    {
        public long PersonaId { get; set; }
        public IEnumerable<string> Sources { get; set; }
        public bool ForceCreate { get; set; } = false;
    }
}
