using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement
{
    public class RightRoleAddRem
    {
        /// <summary>
        /// RoleId
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// RightId
        /// </summary>
        public long RightValueTypeID { get; set; }

        /// <summary>
        /// RightId
        /// </summary>
        public int IsDeleted { get; set; }
    }
}
