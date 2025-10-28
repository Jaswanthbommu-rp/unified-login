using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Product.MarketingCenter
{
    public class Right
    {
        /// <summary>
        /// Right id
        /// </summary>
        [JsonProperty("privilegeId")]
        public int RightId { get; set; }

        /// <summary>
        /// description
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// GroupName
        /// </summary>
        [JsonProperty("privilegeGroupName")]
        public string GroupName { get; set; }

        /// <summary>
        /// GroupId
        /// </summary>
        [JsonProperty("privilegeGroupId")]
        public int GroupId { get; set; }

        /// <summary>
        /// Used to store of the Sub Group name
        /// </summary>
        [JsonProperty("privilegeSubGroupName")]
        public string SubGroupName { get; set; }

        /// <summary>
        /// Sub Group Id
        /// </summary>
        [JsonProperty("privilegeSubGroupId")]
        public string SubGroupId { get; set; }

        /// <summary>
        /// Display Sequence
        /// </summary>
        [JsonProperty("displaySequence")]
        public int DisplaySequence { get; set; }

        /// <summary>
        /// Right name
        /// </summary>
        [JsonProperty("right")]
        public string RightName { get; set; }

        /// <summary>
        /// Used to store of the Action
        /// </summary>
        [JsonProperty("actionLabel")]
        public string Action { get; set; }

        /// <summary>
        /// Used to store of the Role count
        /// </summary>
        [JsonProperty("roles")] 
        public int RoleCount { get; set; }

        /// <summary>
        /// Is right assigned to the role
        /// </summary>
        [JsonProperty("assigned")]
        public bool IsAssigned { get; set; }
    }

    public class MCRight
    {
        /// <summary>
        /// Right id
        /// </summary>
        public int RightId { get; set; }

        /// <summary>
        /// description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// GroupName
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// GroupId
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Used to store of the Sub Group name
        /// </summary>
        public string SubGroupName { get; set; }

        /// <summary>
        /// Sub Group Id
        /// </summary>
        public string SubGroupId { get; set; }

        /// <summary>
        /// Display Sequence
        /// </summary>
        public int DisplaySequence { get; set; }

        /// <summary>
        /// Right name
        /// </summary>
        public string RightName { get; set; }

        /// <summary>
        /// Used to store of the Action
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Used to store of the Role count
        /// </summary>
        public int RolesAssigned { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsAssigned { get; set; }
    }

    public class MCRole
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "rights")]
        public List<int> Rights { get; set;}
        [JsonProperty(PropertyName = "active")]
        public bool Active { get; set; }
    }
}
