using Newtonsoft.Json;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model.SeniorLeadManagement
{
    internal class ProductPropertiesSLM : IProductProperties
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
        public string State
        {
            get; set;
        }
        [JsonProperty(PropertyName = "OneSitePropertyId", NullValueHandling = NullValueHandling.Ignore)]
        public string OneSitePropertyId { get; set; }
    }
}
