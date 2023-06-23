using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.MarketingCenter
{
    public class Right
    {
        /// <summary>
        /// Used to store the right id
        /// </summary>
        [JsonProperty("privilegeId")]
        public int RightId { get; set; }

        /// <summary>
        /// Used to store the Right name
        /// </summary>
        [JsonProperty("right")]
        public string Rightname { get; set; }

        /// <summary>
        /// Used to store of the Group is active
        /// </summary>
        [JsonProperty("privilegeGroupName")]
        public string Groupname { get; set; }

        /// <summary>
        /// Used to store of the Sub Group name
        /// </summary>
        [JsonProperty("privilegeSubGroupName")]
        public string SubGroupname { get; set; }

        /// <summary>
        /// Used to store of the Role count
        /// </summary>
        [JsonProperty("roles")] 
        public int RoleCount { get; set; }

        /// <summary>
        /// Used to store of the Action
        /// </summary>
        [JsonProperty("actionLabel")]
        public string Action { get; set; }

    }
}
