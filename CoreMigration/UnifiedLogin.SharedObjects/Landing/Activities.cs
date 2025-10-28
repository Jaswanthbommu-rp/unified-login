using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class Activities : IActivities
    {
        [JsonProperty(PropertyName = "metaTagGUIDs")]
        public List<string> MetatagUniqueId { get; set; }
    }
}