
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Product.OmniChannel
{
 
    public class PropertyList
    {

        public string Error { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList< PropertyType> Property { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int TotalProperties { get; set; }
        
    }

    public class PropertyType
    {        
        public string PropertyID { get; set; }       
        public string PropertyName { get; set; }        
        public string SiteAddress { get; set; }        
        public string SiteCityName { get; set; }        
        public string SiteState { get; set; }        
        public string SiteZip { get; set; }        
        public string SitePhone { get; set; }        
        public bool IsAssignedToUser { get; set; }

    }
}
