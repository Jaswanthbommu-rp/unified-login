using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    public class MappedUnifiedLoginUserDetails
    {
        public int CompanyId { get; set; }
        public string ProductCode { get; set; }
        public string upfmId { get; set; }
        public List<ULMappedPersonaIds> ULMappedPersonaId { get; set; }
    }
    public class ULMappedPersonaIds
    {
        public string ProductUserId { get; set; }
        public long UnifiedLoginPersonaId { get; set; }
        public string PreferredPhoneNumber { get; set; }
        public string Email { get; set; }
    }
}
