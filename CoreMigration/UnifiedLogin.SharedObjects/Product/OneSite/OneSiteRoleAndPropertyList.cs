using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.OneSite
{
    /// <summary>
    /// Used to grant a user roles and properties
    /// </summary>
    public class OneSiteRoleAndPropertyList
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
        public bool IsAssigned { get; set; } = true;
    }
}
