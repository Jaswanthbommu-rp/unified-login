using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Product.OmniChannel
{
    public class RoleType
    {
        public string RoleID { get; set; }
        public string RoleName { get; set; }
        public string RightsAssigned { get; set; }
        public bool IsInternal { get; set; }
        public string Roletype { get; set; }
        public bool IsAssigned { get; set; }
        public string InheritedRoleName { get; set; }
    }

    public class RoleList
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<RoleType> Role { get; set; }
        
        public int TotalRoles { get; set; }

    }

}
