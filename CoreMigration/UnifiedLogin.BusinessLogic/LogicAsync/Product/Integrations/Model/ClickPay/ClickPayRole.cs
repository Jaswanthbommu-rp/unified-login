using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.Model.ClickPay
{
    [ExcludeFromCodeCoverage]
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
		public bool IsAssigned { get; set; }
		public List<ClickPaySelectedItems> SelectedItems { get; set; }
	}

	public class ClickPaySelectedItems
	{
		public string Id { get; set; }
		public bool Value { get; set; }
	}
}