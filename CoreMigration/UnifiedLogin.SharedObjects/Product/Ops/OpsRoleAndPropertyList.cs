using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.Ops
{
    public class OpsRoleAndPropertyList
    {
        /// <summary>
        /// A list of roles to assign to the user
        /// </summary>
        public List<int> RoleList;

        /// <summary>
        /// A list of properties to assign to the user
        /// </summary>
        public List<int> PropertyList;

        /// <summary>
        /// Is product assigned or removed
        /// </summary>
        public bool IsAssigned { get; set; }
    }
}
