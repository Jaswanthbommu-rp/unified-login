using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.PortofolioManagement
{
    public class PortofolioManagementRoleProperties
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
        public List<PAMRolePropertyList> RolePropertiesList { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<OrganizationRole> OrganizationRoleList { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool CanReceiveMonthlyReport { get; set; } // used in DIQ

        public bool IsAssigned { get; set; }
    }

    public class PAMRolePropertyList
    {
        public string RoleId { get; set; }
        public List<string> PropertyIds { get; set; }
    }
}
