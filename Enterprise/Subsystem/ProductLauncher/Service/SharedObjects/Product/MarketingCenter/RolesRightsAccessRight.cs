using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.MarketingCenter
{
    public class RolesRightsAccessRight
    {
        [JsonProperty("id")]
        public int RoleId { get; set; }

        /// <summary>
        /// Used to store the role name
        /// </summary>
        [JsonProperty("name")]
        public string RoleName { get; set; }

        /// <summary>
        /// Used to store the rightsAssigned
        /// </summary>
        [JsonProperty("rightsAssigned")]
        public int RightsAssigned { get; set; }

        /// <summary>
        /// Used to store the role id
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Used to store of the role is active
        /// </summary>
        [JsonProperty("active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Used to store of the users
        /// </summary>
        [JsonProperty("users")]
        public int UserCount { get; set; }

        /// <summary>
        /// Used to store the RoleType
        /// </summary>
        [JsonProperty("type")]
        public string RoleType { get; set; }

        /// <summary>
        /// Used to store the mutable
        /// </summary>
        [JsonProperty("mutable")]
        public bool IsMutable { get; set; }
    }
}
