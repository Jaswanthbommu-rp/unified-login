using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model
{
    /// <summary>
    /// Class which binds with product response
    /// </summary>
    public class ProductProperties
    {
        private string _name = string.Empty;
        private string _propertyId;

        [JsonProperty(PropertyName = "id")]
        public string GetPropertyId => _propertyId;

        [JsonProperty(PropertyName = "PropertyId")]
        public string SetPropertyId
        {
            set { this._propertyId = value; }
        }

        [JsonProperty(PropertyName = "name")]
        public string GetName => _name;

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
	}

}
