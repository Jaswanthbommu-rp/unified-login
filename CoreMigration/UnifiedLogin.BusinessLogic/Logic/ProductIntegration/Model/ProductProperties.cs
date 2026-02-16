using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// Class which binds with product response
    /// </summary>
    public class ProductProperties : IProductProperties
    {
        private string _name = string.Empty;
        private string _propertyId;

        [JsonProperty(PropertyName = "id")]
        public string GetPropertyId
        {
            get { return _propertyId; }
        }

        [JsonProperty(PropertyName = "PropertyId")]
        public string SetPropertyId
        {
            set { this._propertyId = value; }
        }

        [JsonProperty(PropertyName = "name")]
        public string GetName 
        {
            get { return _name; }
        }

        [JsonProperty(PropertyName = "PropertyName")]
        public string SetName
        {
            set { this._name = value; }
        }

        public bool IsAssigned { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string GroupId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PropertyType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string City { get; set; }

        /// <summary>
        /// The UPFM property instance id
        /// </summary>
        public string InstanceId { get; set; }

    }
}
