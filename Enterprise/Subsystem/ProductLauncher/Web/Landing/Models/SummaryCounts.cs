using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Models
{
    public class SummaryCounts
    {
        [JsonProperty(PropertyName = "properties")]
        public int TotalAssignedProperties { get; set; }
        [JsonProperty(PropertyName = "products")]
        public int TotalAssignedProducts { get; set; }
        [JsonProperty(PropertyName = "roles")]
        public int TotalAssignedRoles { get; set; }
    }
}