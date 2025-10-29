using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model
{
    public class ProductUserGroup
    {
        /// <summary>
        /// Get UserGroupId
        /// </summary>
        private string _groupId;

        [JsonProperty(PropertyName = "id")]
        public string GetGroupId => _groupId;

        [JsonProperty(PropertyName = "UserGroupId")]
        public string SetGroupId
        {
            set { _groupId = value; }
        }

        /// <summary>
        /// Get User Group Name.
        /// </summary>
        [JsonProperty(PropertyName = "userGroupName")]
        public string UserGroupName { get; set; }

        /// <summary>
        /// Get User Group Type.
        /// </summary>
        [JsonProperty(PropertyName = "userGroupType")]
        public string userGroupType { get; set; }

        /// <summary>
        /// IsAssigned
        /// </summary>
        [JsonProperty(PropertyName = "IsAssigned")]
        public bool IsAssigned { get; set; }
    }
}
