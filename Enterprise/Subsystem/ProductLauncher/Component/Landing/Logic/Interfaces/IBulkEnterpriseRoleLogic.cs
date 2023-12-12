using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.EnterpriseRole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
    public interface IBulkEnterpriseRoleLogic
    {
        string UpdateEnterpriseToUsers(long editorUserPersonaId, List<long> userPersonaIds, RoleTemplateProductRoleMapping roleTemplateProductRole);
    }
}
