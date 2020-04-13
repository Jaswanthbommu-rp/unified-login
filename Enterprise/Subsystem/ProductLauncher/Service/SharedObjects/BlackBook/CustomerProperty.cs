using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook
{
	public class PropertyAddress
	{
		public string address { get; set; }
		public string city { get; set; }
		public string state { get; set; }
		public string country { get; set; }
		public string county { get; set; }
		public string postalCode { get; set; }
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
		//[JsonProperty(PropertyName = "Id", NullValueHandling = NullValueHandling.Ignore)]
		[JsonIgnore]
		public string id { get; set; }
		[JsonProperty(PropertyName = "companyId", NullValueHandling = NullValueHandling.Ignore)]
		public string companyId { get; set; }
		[JsonProperty(PropertyName = "propertyId", NullValueHandling = NullValueHandling.Ignore)]
		public string propertyId { get; set; }
		[JsonProperty(PropertyName = "name", NullValueHandling = NullValueHandling.Ignore)]
		public string name { get; set; }
		[JsonProperty(PropertyName = "street", NullValueHandling = NullValueHandling.Ignore)]
		public string street { get; set; }
		[JsonProperty(PropertyName = "city", NullValueHandling = NullValueHandling.Ignore)]
		public string city { get; set; }
		[JsonProperty(PropertyName = "state", NullValueHandling = NullValueHandling.Ignore)]
		public string state { get; set; }
		[JsonProperty(PropertyName = "postalCode", NullValueHandling = NullValueHandling.Ignore)]
		public string postalCode { get; set; }
		[JsonProperty(PropertyName = "attributes", NullValueHandling = NullValueHandling.Ignore)]
		public PropertyAttributes attributes { get; set; }
	}
}