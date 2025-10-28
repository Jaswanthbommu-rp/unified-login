using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    public class ProductUserIDMappingRequest
    {
        public int CompanyId { get; set; }
        public string upfmId { get; set; }
        public string ProductCode { get; set; }
        public List<string> ProductUserId { get; set; }
    }
}
