using Newtonsoft.Json;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model
{
    /// <summary>
    /// Model for get all rights method
    /// </summary>
    public sealed class ProductRightRole
    {
        #region "Properties"

        /// <summary>
        /// Right id
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string RightId { get; set; }

        /// <summary>
        /// Right name
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string RightName { get; set; }

        /// <summary>
        /// Is Assigned property
        /// </summary>
        [JsonProperty(PropertyName = "isAssigned")]
        public bool IsAssigned { get; set; }

        /// <summary>
        /// Access all properties
        /// </summary>
        [JsonProperty(PropertyName = "accessAllProperties", NullValueHandling = NullValueHandling.Ignore)]
        public bool AccessAllProperties { get; set; }

        /// <summary>
        /// The Editor has right
        /// </summary>
        [JsonProperty(PropertyName = "isEditorHasRight", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsEditorHasRight { get; set; }

        /// <summary>
        /// Role Type 
        /// </summary>
        [JsonProperty(PropertyName = "roletype", NullValueHandling = NullValueHandling.Ignore)]
        public string RoleType { get; set; }

        #endregion 
    }
}
