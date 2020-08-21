using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook
{
    public class TranslatePropertyInstance
    {
        [JsonProperty("data")]
        public TranslatePropertyInstanceData Data { get; set; }
    }

    public partial class TranslatePropertyInstanceData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("attributes")]
        public TranslatePropertyInstanceAttributes Attributes { get; set; }
    }

    public partial class TranslatePropertyInstanceAttributes
    {
        [JsonProperty("propertyInstanceSourceId")]
        public string PropertyInstanceSourceId { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("translatedPropertyInstances")]
        public List<TranslatedCompanyInstanceData> TranslatedPropertyInstances { get; set; }
    }

    public partial class TranslatedPropertyInstanceData
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("propertyInstanceSourceId")]
        public string PropertyInstanceSourceId { get; set; }

    }
}
