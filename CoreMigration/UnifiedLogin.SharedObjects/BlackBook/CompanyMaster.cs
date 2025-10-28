using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.BlackBook
{
    public class CompanyMaster
    {
        public CompanyMaster()
        {
            DomainList = new List<CustomerCompanyDomain>();
            CompanyInstance = new List<CompanyInstanceAttribute>();
        }
        public Company CompanyDetail { get; set; }
        public List<CustomerCompanyDomain> DomainList { get; set; }
        public List<CompanyInstanceAttribute> CompanyInstance { get; set; }
    }
}
