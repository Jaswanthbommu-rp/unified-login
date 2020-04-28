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
        public int ProductId { get; set; }
        public List<ULMappedUserIds> ULMappedUserId { get; set; }
    }

    public class ULMappedUserIds
    {
        public int ProductUserId { get; set; }
        public int UnifiedLoginUserId { get; set; }
    }
}
