using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Product.Accounting
{
    public class AccountingUser
    {
        
        public bool HasAccessToSiteSpendManagementOnly { get; set; } = false;

        public bool HasAccessToAllCurrentFutureProperties { get; set; } = false;

        public bool IsAccountingAdmin { get; set; } = false;

        public bool IsSiteSpendManagementAssignedToCompany { get; set; } = false;

        public bool IsMConsolePMC { get; set; } = false;

    }
}
