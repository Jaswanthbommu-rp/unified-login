using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.BlackBook
{
	public class CompanyInstanceAttribute
	{
		public long CompanyInstanceId { get; set; }
		public string Source { get; set; }
		public string companyInstanceSourceId { get; set; }
		public string CompanyName { get; set; }
		public string CompanyType { get; set; }
		public string IsActive { get; set; }
		public string Domain { get; set; }
		public List<CompanyLocation> CompanyInstanceLocation { get; set; }
	}

	public class CustomerCompanyInstance
	{
		[JsonIgnore]
		public string id { get; set; }
		[JsonProperty(PropertyName = "attributes", NullValueHandling = NullValueHandling.Ignore)]
		public CompanyInstanceAttribute attributes { get; set; }
	}
}