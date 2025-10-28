using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.BlackBook
{
    public class SystemProductCenter
    {
        public long Id { get; set; }
        public string CreatedBy { get; set; }
        public string Source { get; set; }
        public string ProductCenterSourceId { get; set; }
        public string CompanyInstanceSourceId { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string PropertyInstanceSourceId { get; set; }
    }
}
