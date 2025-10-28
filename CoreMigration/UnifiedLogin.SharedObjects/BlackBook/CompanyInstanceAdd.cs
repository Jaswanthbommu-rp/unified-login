using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.BlackBook
{
    public class CompanyInstanceAdd : CompanyInstance
    {
        [JsonIgnore]
        public long Id { get;set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<CompanyInstancePartner> CompanyInstancePartners { get; set; }
    }
}
