using System.Collections.Generic;
using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model.ClickPay
{
	public class ClickPayRoles
	{
		[JsonProperty("results")]
		public List<ClickPayRole> ClickPayRoleList { get; set; }
		public long Start { get; set; }
		public long Limit { get; set; }
		public long Size { get; set; }
	}
	public class ClickPayRole
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string OrgType { get; set; }
		[JsonProperty("orgsAssigned")]
		public int OrgsAssignedCount { get; set; }
	}
}