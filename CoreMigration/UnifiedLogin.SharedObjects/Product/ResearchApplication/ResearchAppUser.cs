using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.ResearchApplication
{
    

    public class UserLocation
    {
        public string PropertyId { get; set; }
    }

    public class UserAccessGroup
    {
        public string AccessGroupCode { get; set; }
        //public bool IsAssigned { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AccessGroupName { get; set; }
    }

    

    /// <summary>
    /// Object to map with Input Json from UI
    /// </summary>
    public class UserAssignProductPropertyRole
    {
        /// <summary>
        /// A list of properties to assign to the user
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> PropertyList { get; set; }


        /// <summary>
        /// A role to assign to the user
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> RoleList { get; set; }


        /// <summary>
        /// Is product assigned or removed
        /// </summary>
        public bool IsAssigned { get; set; }
    }
}
