using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise
{
    public class MappedUnifiedLoginUserDetails
    {
        public int CompanyId { get; set; }
        public string ProductCode { get; set; }
        public List<ULMappedPersonaIds> ULMappedPersonaId { get; set; }
    }
    public class ULMappedPersonaIds
    {
        public string ProductUserId { get; set; }
        public long UnifiedLoginPersonaId { get; set; }
    }
}
