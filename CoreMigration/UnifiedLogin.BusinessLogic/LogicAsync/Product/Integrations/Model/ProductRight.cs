using Newtonsoft.Json;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.Model
{
    public class ProductRight
    {
        private string _name = string.Empty;
        private string _rightId;

        /// <summary>
        /// Get RoleId
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string GetRightId
        {
            get { return _rightId; }
        }

        /// <summary>
        /// Set RoleId
        /// </summary>
        [JsonProperty(PropertyName = "RightId")]
        public string SetRightId
        {
            set { this._rightId = value; }
        }

        [JsonProperty(PropertyName = "name")]
        public string GetName => _name;

        /// <summary>
        /// Role Name
        /// </summary>
        [JsonProperty(PropertyName = "RightName")]
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
        /// RightType 
        /// </summary>
        [JsonProperty(PropertyName = "RightType", NullValueHandling = NullValueHandling.Ignore)]
        public string RightType { get; set; }
    }
}
