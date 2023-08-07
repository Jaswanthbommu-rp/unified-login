using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    public class DelegateAdminRoleTemplate : IDelegateRoleTemplate
    {
        [JsonProperty("UserLoginPersonaId")]
        public long UserLoginPersonaId { get; set; }

        [JsonProperty("DelegateRoleTemplates")]
        public IList<DelegateRoleTemplate> DelegateRoleTemplates { get; set; }
    }
}
