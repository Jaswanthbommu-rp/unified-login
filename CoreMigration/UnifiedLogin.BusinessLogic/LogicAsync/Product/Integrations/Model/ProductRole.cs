using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;
using System.Collections.Generic;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.Model
{
    /// <summary>
    /// Class which binds with product response
    /// </summary>
    public class ProductRole
    {
        private string _name = string.Empty;
        private string _roleId;

        /// <summary>
        /// Get RoleId
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string GetRoleId
        {
            get { return _roleId; }
        }

        /// <summary>
        /// Set RoleId
        /// </summary>
        [JsonProperty(PropertyName = "RoleId")]
        public string SetRoleId
        {
            set { this._roleId = value; }
        }

        [JsonProperty(PropertyName = "name")]
        public string GetName => _name;

        /// <summary>
        /// Role Name
        /// </summary>
        [JsonProperty(PropertyName = "RoleName")]
        public string SetName
        {
            set { this._name = value; }
        }

        /// <summary>
        /// IsAssigned
        /// </summary>
        [JsonProperty(PropertyName = "IsAssigned")]
        public bool IsAssigned { get; set; }

        /// <summary>
        /// RoleType 
        /// </summary>
        [JsonProperty(PropertyName = "RoleType", NullValueHandling = NullValueHandling.Ignore)]
        public string RoleType { get; set; }

        /// <summary>
        /// List of rights for every role
        /// </summary>
        [JsonProperty(PropertyName = "Rights", NullValueHandling = NullValueHandling.Ignore)]
        public List<Right> Rights { get; set; }

    }

}
