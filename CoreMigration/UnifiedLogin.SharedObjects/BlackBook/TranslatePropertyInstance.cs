using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.BlackBook
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
        public List<TranslatePropertyInstanceAttribute> Attributes { get; set; }
    }

    public partial class TranslatePropertyInstanceAttribute
    {
        [JsonProperty("propertyInstanceSourceId")]
        public string PropertyInstanceSourceId { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("translatedPropertyInstances")]
        public List<TranslatedPropertyInstanceData> TranslatedPropertyInstances { get; set; }
    }

    public partial class TranslatedPropertyInstanceData
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("propertyInstanceSourceId")]
        public string PropertyInstanceSourceId { get; set; }

        [JsonProperty("customerPropertyId")]
        public string CustomerPropertyId { get; set; }        
    }
}
