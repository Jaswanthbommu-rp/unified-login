using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.Lead2Lease
{
    /// <summary>
    /// Used to hold the properties and roles to assign to the user
    /// </summary>
    public class Lead2LeaseRoleAndPropertyList
    {
        /// <summary>
        /// A list of roles to assign to the user
        /// </summary>
        public List<string> RoleList;

        /// <summary>
        /// A list of properties to assign to the user
        /// </summary>
        public List<string> PropertyList;

        /// <summary>
        /// Is product assigned or removed
        /// </summary>            
        public bool IsAssigned { get; set; }
    }
}
