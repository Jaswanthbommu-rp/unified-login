using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Enterprise Role
    /// </summary>
    public class EnterpriseRole
    {
        /// <summary>
        /// 
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int PartyRoleTypeId { get; set; }
		/// <summary>
		/// Role Name
		/// </summary>
		public string Role { get; set; }
	}
}
