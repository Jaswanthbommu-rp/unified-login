using System;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace UnifiedLogin.SharedObjects.Product.OmniChannel
{
    public class RightList 
    {

        public string Error { get; set; }
        

        
        public IList< RightType> Right { get; set; }


        
        public int TotalRights { get; set; }

    }

    public  class RightType
    {

        public string RightID { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RightDescription { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CenterName { get; set; }
       

        /// <remarks/>
        public bool Assigned { get; set; }
        
        
        public int RolesAssigned { get; set; }

    }
}
