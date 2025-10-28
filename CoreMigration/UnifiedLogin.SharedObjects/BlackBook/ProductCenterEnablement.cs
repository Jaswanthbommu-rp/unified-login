using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.BlackBook
{
    public class ProductCenterEnablement
    {
        public int Id { get; set; }
        public string EnabledBy { get; set; }


        public List<ProductCenterEnablementSettings> Details { get; set; }
    }

    public class ProductCenterEnablementSettings
    {

        public int CustomerCompanyId { get; set; }
        
        public string CompanyInstanceSourceId { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string CustomerPropertyId { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string PropertyInstanceSourceId { get; set; }
        
        public string ProductCenterSourceId { get; set; }
        
        public string Source { get; set; }
        
        public string CustomerEnvironment { get; set; }
    }
}
