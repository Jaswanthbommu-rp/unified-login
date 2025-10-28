using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.BlackBook
{
    public class ProductCenterCancellation
    {
        public int Id { get; set; }

        public string CancelledBy { get; set; }

        public List<ProductCenterCancellationSettings> Details { get; set; }
    }

    public class ProductCenterCancellationSettings
    {

        public string CompanyInstanceSourceId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string PropertyInstanceSourceId { get; set; }        
        
        public string ProductCenterSourceId { get; set; }
        
        public string Source { get; set; }
    }
}
