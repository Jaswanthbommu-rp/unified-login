using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.BlackBook
{
    public class TranslateCompanyInstance
    {
        [JsonProperty("data")]
        public TranslateCompanyInstanceData Data { get; set; }
    }

    public partial class TranslateCompanyInstanceData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("attributes")]
        public TranslateCompanyInstanceAttributes Attributes { get; set; }
    }

    public partial class TranslateCompanyInstanceAttributes
    {
        [JsonProperty("companyInstanceSourceId")]
        public string CompanyInstanceSourceId { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("translatedCompanyInstances")]
        public List<TranslatedCompanyInstanceData> TranslatedCompanyInstances { get; set; }
    }

    public partial class TranslatedCompanyInstanceData
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("companyInstanceSourceId")]
        public string CompanyInstanceSourceId { get; set; }

        [JsonProperty("customerEnvironment")]
        public string CustomerEnvironment { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }
    }
}
