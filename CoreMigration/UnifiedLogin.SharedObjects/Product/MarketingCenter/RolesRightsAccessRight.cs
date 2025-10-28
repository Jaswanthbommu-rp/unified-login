using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Product.MarketingCenter
{
    public class RolesRightsAccessRight
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Used to store the role name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Used to store the role id
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Used to store of the role is active
        /// </summary>
        [JsonProperty("active")]
        public bool Active { get; set; }

        /// <summary>
        /// Used to store of the users
        /// </summary>
        [JsonProperty("users")]
        public int Users { get; set; }

        /// <summary>
        /// Used to store the RoleType
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Used to store the mutable
        /// </summary>
        [JsonProperty("mutable")]
        public bool Mutable { get; set; }

        /// <summary>
        /// Used to store the IsAssigned
        /// </summary>
        [JsonProperty("assigned")]
        public bool IsAssigned { get; set; }

        /// <summary>
        /// Used to store the rightsAssigned
        /// </summary>
        [JsonProperty("rights")]
        public int RightsAssigned { get; set; }
    }
}
