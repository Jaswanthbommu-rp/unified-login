using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.Model
{
	public class ProductUserProfile
	{
		[JsonProperty(PropertyName = "userId")]
		public string UserId { get; set; }
		[JsonProperty(PropertyName = "loginName")]
		public string LoginName { get; set; }

		[JsonProperty(PropertyName = "title", NullValueHandling = NullValueHandling.Ignore)]
		public string Title { get; set; }

		[JsonProperty(PropertyName = "firstName")]
		public string FirstName { get; set; }

		[JsonProperty(PropertyName = "middleName", NullValueHandling = NullValueHandling.Ignore)]
		public string MiddleName { get; set; }

		[JsonProperty(PropertyName = "lastName")]
		public string LastName { get; set; }
		[JsonProperty(PropertyName = "email")]
		public string Email { get; set; }
		[JsonProperty(PropertyName = "isActive")]
		public bool IsActive { get; set; }

		[JsonProperty(PropertyName = "phoneNumbers", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> PhoneNumbers { get; set; } = new List<string>();
        
        [JsonProperty(PropertyName = "phone", NullValueHandling = NullValueHandling.Ignore)]
        public string Phone { get; set; }
		
        [JsonProperty(PropertyName = "companyId", NullValueHandling = NullValueHandling.Ignore)]
		public string CompanyId { get; set; }
	}
}