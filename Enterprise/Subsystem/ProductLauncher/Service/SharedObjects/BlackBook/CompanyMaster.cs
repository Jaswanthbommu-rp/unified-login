using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook
{
    public class CompanyMaster
    {
        public CompanyMaster()
        {
            DomainList = new List<CustomerCompanyDomain>();
        }
        public Company CompanyDetail { get; set; }
        public List<CustomerCompanyDomain> DomainList { get; set; }
    }
}
