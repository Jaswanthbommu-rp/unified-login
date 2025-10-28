using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Product.UnifiedLogin
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
