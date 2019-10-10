using Newtonsoft.Json;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Models
{
    public class Activities
    {
        [JsonProperty(PropertyName = "metaTagGUIDs")]
        public List<string> MetatagUniqueId { get; set; }
    }
}