using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Product.ClientPortal
{
    public class ClientPortalPropertyRole
    {
        public IList<string> RoleList { get; set; }
        public IList<string> PropertyList { get; set; }

        /// <summary>
        /// Is product assigned or removed
        /// </summary>
        public bool IsAssigned { get; set; }
    }
}
