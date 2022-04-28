using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model
{
    public class ProductUserGroup
    {
        /// <summary>
        /// Get UserGroupId
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string UserGroupId { get; set; }

        /// <summary>
        /// Get User Group Name.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string UserGroupName { get; set; }

        /// <summary>
        /// IsAssigned
        /// </summary>
        [JsonProperty(PropertyName = "IsAssigned")]
        public bool IsAssigned { get; set; }
    }
}
