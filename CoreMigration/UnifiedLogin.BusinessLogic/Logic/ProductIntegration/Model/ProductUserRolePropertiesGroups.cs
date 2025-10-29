using System.Collections.Generic;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model.SeniorLeadManagement;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.RealConnect;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model
{
    /// <summary>
    /// Maps with Input JSON from batch
    /// </summary>
    public class ProductUserRolePropertiesGroups
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> RoleList { get; set; }

	    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	    public List<string> RoleListString { get; set; } // products which uses rolesId as string (E.g. DIQ)

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> PropertyList { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> PropertyGroupList { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<PropertyRoleList> PropertyRoleList { get; set; }

	    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	    public List<OrganizationRole> OrganizationRoleList { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<PAMRolePropertyList> RolePropertiesList { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public bool CanReceiveMonthlyReport { get; set; } // used in DIQ

		public bool IsAssigned { get; set; }

        [JsonProperty(PropertyName = "oneSiteUserInfo", NullValueHandling = NullValueHandling.Ignore)]
        public OneSiteUserInfo OneSiteUserInfo { get; set; } //SLM

        [JsonProperty(PropertyName = "usergroups", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> UserGroups { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public RCProductBatch RCLicenseDetails { get; set; }
    }
}