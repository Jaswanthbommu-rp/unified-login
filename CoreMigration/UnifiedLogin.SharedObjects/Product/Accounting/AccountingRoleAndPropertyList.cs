using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.Accounting
{
    /// <summary>
    /// Used to grant a user roles and properties
    /// </summary>
    public class AccountingRoleAndPropertyList
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
        /// A list of companies to assign to the user
        /// </summary>
        public List<string> CompaniesList;

        public bool HasAccessToSiteSpendManagementOnly { get; set; } 

        public bool HasAccessToAllCurrentFutureProperties { get; set; } 

        public bool IsAccountingAdmin { get; set; } 

        /// <summary>
        /// Is product assigned or removed
        /// </summary>
        public bool IsAssigned { get; set; }
    }
}
