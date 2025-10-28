using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Product.Accounting
{
    public class RolesAddRemove
    {
        /// <summary>
        /// A list of roles to add to the right
        /// </summary>
        public List<ProductRoleAcct> RolesToAdd;
        /// <summary>
        /// A list of roles to remove from the right
        /// </summary>
        public List<ProductRoleAcct> RolesToDelete;
    }
}
