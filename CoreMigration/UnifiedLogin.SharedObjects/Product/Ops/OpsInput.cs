using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.Ops
{
    /// <summary>
    /// Used to grant a user roles 
    /// </summary>
    public class OpsInput
    {

        public string RoleName { get; set; }
        public string RoleDesc { get; set; }
        //public string IsMarketPlaceAdmin { get; set; }

        public string OrderWorkflowTimeout { get; set; }

        public string InvoiceWorkflowTimeout { get; set; }

        //public string SupplierWorkflowTimeout { get; set; }

        public string OrderEndorseEmailReminderFlag { get; set; }

        public string InvoiceEndorseEmailReminderFlag { get; set; }

        public List<OpsRight> rightsList { get; set; }

    }
}



