using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.BlackBook
{
	public class PropertyAddress
	{
		public string address { get; set; }
		public string city { get; set; }
		public string state { get; set; }
		public string country { get; set; }
		public string county { get; set; }
		public string postalCode { get; set; }
		public string latitude { get; set; }
        public string longitude { get; set; }
    }

	public class PropertyAttributes
	{
		public string customerCompanyId { get; set; }
		public string customerPropertyId { get; set; }
		public string propertyName { get; set; }
		public PropertyAddress address { get; set; }
	}

	public class CustomerProperty
	{
		[JsonIgnore]
		public string id { get; set; }
		[JsonProperty(PropertyName = "attributes", NullValueHandling = NullValueHandling.Ignore)]
		public PropertyAttributes attributes { get; set; }
	}
}