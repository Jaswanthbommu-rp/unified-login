using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    public interface IDelegateRoleTemplate
    {
        /// <summary>
        /// userLoginPersonaID
        /// </summary>
        long UserLoginPersonaId { get; set; }

        IList<DelegateRoleTemplate> DelegateRoleTemplates { get; set; }
    }
}
