using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model.SeniorLeadManagement;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model
{
	/// <summary>
	/// Product User 
	/// </summary>
	public class IntegrationProductUser : BaseIntegrationProductUser// ProductUser creates ambiguity as it defined somewhere
	{
		[JsonProperty(PropertyName = "properties", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> Properties { get; set; }
		[JsonProperty(PropertyName = "propertyGroups", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> PropertyGroups { get; set; }
		[JsonProperty(PropertyName = "roles", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> Roles { get; set; }
		[JsonProperty(PropertyName = "propertyRoles", NullValueHandling = NullValueHandling.Ignore)]
		public List<PropertyRoleList> PropertyRoles { get; set; }
		[JsonProperty(PropertyName = "organizationRoles", NullValueHandling = NullValueHandling.Ignore)]
		public List<OrganizationRole> OrganizationRoles { get; set; }
		[JsonProperty(PropertyName = "receiveMonthlyUsageReport", NullValueHandling = NullValueHandling.Ignore)]
		public bool CanReceiveMonthlyReport { get; set; } // used in DIQ
		[JsonProperty(PropertyName = "PropertyRoleList", NullValueHandling = NullValueHandling.Ignore)]
		public List<PAMRolePropertyList> PropertyRoleList { get; set; } // used in PAM
		[JsonProperty(PropertyName = "RoleType", NullValueHandling = NullValueHandling.Ignore)]
        public string RoleType { get; set; } // used in DIQ
        [JsonProperty(PropertyName = "RoleList", NullValueHandling = NullValueHandling.Ignore)]

		public List<string> RoleList { get; set; } // used in PAM

		[JsonProperty(PropertyName = "oneSiteUserInfo", NullValueHandling = NullValueHandling.Ignore)]
		public OneSiteUserInfo OneSiteUserInfo { get; set; } //SLM

		[JsonProperty(PropertyName = "phoneNumbers")]
		public List<string> PhoneNumbers { get; set; } //SLM

        [JsonProperty(PropertyName = "countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty(PropertyName = "ISOCode")]
        public string ISOCode { get; set; }

        [JsonProperty(PropertyName = "additionalFields")]
		//public Dictionary<string, string> AdditionalFields { get; set; } //SLM
		public List<KeyValuePair<string, string>> AdditionalFields { get; set; } //SLM

		[JsonProperty(PropertyName = "userGroups", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> UserGroups { get; set; }
	}

	public class BaseIntegrationProductUser
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
		[JsonProperty(PropertyName = "isActive", NullValueHandling = NullValueHandling.Ignore)]
		public bool IsActive { get; set; }

		[JsonProperty(PropertyName = "phone", NullValueHandling = NullValueHandling.Ignore)]
		public string Phone { get; set; }

		[JsonProperty(PropertyName = "companyId", NullValueHandling = NullValueHandling.Ignore)]
		public string CompanyId { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is migrated user.
		/// </summary>
		[JsonProperty(PropertyName = "isMigratedUser", NullValueHandling = NullValueHandling.Ignore)]
		public bool IsMigratedUser { get; set; }

		[JsonProperty(PropertyName = "lastActivity", NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? LastActivity { get; set; }

		[JsonProperty(PropertyName = "isAdminUser", NullValueHandling = NullValueHandling.Ignore)]
		public bool IsAdminUser { get; set; }

        [JsonProperty(PropertyName = "isRealPageEmployee", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsRealPageEmployee { get; set; }

        [JsonProperty(PropertyName = "EmployeeAdditional", NullValueHandling = NullValueHandling.Ignore)]
		public EmployeeAdditional EmployeeAdditional { get; set; }

        [JsonProperty(PropertyName = "UnifiedLoginUserID", NullValueHandling = NullValueHandling.Ignore)]
        public long UnifiedLoginUserID { get; set; }

        [JsonProperty(PropertyName = "UnifiedLoginPersonaID", NullValueHandling = NullValueHandling.Ignore)]
        public long UnifiedLoginPersonaID { get; set; }
    }

    
	public class EmployeeAdditional
    {
        [JsonProperty(PropertyName = "samAccountName", NullValueHandling = NullValueHandling.Ignore)]
		public string SAMAccountName { get; set; }

        [JsonProperty(PropertyName = "azureADGroup", NullValueHandling = NullValueHandling.Ignore)]
		public string AzureADGroup { get; set; }

        [JsonIgnore]
		public int AzureADGroupId { get; set; }
    }
}