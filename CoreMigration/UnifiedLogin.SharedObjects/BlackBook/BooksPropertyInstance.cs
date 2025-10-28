using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.BlackBook
{
	public class CustomerPropertyMap
	{
		public int customerPropertyId { get; set; }
		public int propertyInstanceId { get; set; }
		public string propertyInstanceSourceId { get; set; }
		public List<CustomerPropertyInstance> customerProperty { get; set; }
    }

	public class CustomerPropertyInstance
	{
		public int customerPropertyId { get; set; }
		public string propertyName { get; set; }

		public PropertyInstanceAddress address { get; set; }
        public bool isActive { get; set; }

		public string hasMedia { get; set; }
       public List<CustomerPropertyOrderTypeInstance> customerPropertyOrderType { get; set; }
	}

	public class CustomerPropertyOrderTypeInstance
	{
	public int customerPropertyId { get; set; }
	public string orderType { get; set; }
	}

	public class PropertyAttributesInstance
	{
		public string propertyInstanceId { get; set; }
		public string propertyInstanceSourceId { get; set; }
		public string propertyName { get; set; }
		public string source { get; set; }
		public PropertyInstanceAddress address { get; set; }
		public string domain { get; set; }
		
		public string isActive { get; set; } = null;
		public string deletedReason { get; set; }
		public List<CustomerPropertyMap> customerPropertyMap { get; set; }
	}

	public class BooksPropertyInstance
	{
		[JsonIgnore]
		public string id { get; set; }
		[JsonProperty(PropertyName = "attributes", NullValueHandling = NullValueHandling.Ignore)]
		public PropertyAttributesInstance attributes { get; set; }
	}
}