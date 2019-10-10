using Newtonsoft.Json;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
    public class Activities : IActivities
    {
        [JsonProperty(PropertyName = "metaTagGUIDs")]
        public List<string> MetatagUniqueId { get; set; }
    }
}