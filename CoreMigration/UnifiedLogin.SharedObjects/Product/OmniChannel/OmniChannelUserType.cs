using Newtonsoft.Json;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.OmniChannel
{
    
    public  class UserList
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList< UserType> User { get; set; }
        
                
        public int TotalUsers { get; set; }
        
    }

    public  class UserType
    {

      
        public int UserId { get; set; }        

        
        public string UserLogin { get; set; }

        
        public string UserName { get; set; }
        
        
        public bool Assigned { get; set; }

    }
}
