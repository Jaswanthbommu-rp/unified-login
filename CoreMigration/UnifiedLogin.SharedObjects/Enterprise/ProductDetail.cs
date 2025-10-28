using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Enterprise
{
	public class ProductDetail
	{
		public string ProductCode { get; set; }
		public List<string> PropertiesAssigned { get; set; }
		public List<string> RolesAssigned { get; set; }
		public List<string> RegionsAssigned { get; set; }
		public bool IsAssigned { get; set; }

		public Dictionary<string, string> AdditionalFields = new Dictionary<string, string>();
	}
}
