using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.Rum
{
    public class RumUserPropertyRegionRole
    {
        /// <summary>
        /// A list of properties to assign to the user
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> PropertyList { get; set; }
        /// <summary>
        /// Regions to assign to the user
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> RegionList { get; set; }
        /// <summary>
		/// A Property Group to assign to the user
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> PropertyGroupList { get; set; }
        /// <summary>
        /// A list of roles to assign to the user
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> RoleList { get; set; }

        /// <summary>
        /// A list of regions to assign to the user
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UserAccessType { get; set; }

        public bool IsAssigned { get; set; } = true;

    }
}
