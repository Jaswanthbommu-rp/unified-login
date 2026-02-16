using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model.ClickPay
{
	[ExcludeFromCodeCoverage]
	public class ClickPayOrganizations
	{
		[JsonProperty("results")]
		public List<ClickPayOrganization> ClickPayOrganizationList { get; set; }
		public long Start { get; set; }
		public long Limit { get; set; }
		public long Size { get; set; }
	}
	public class ClickPayOrganization
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
		public string TypeDescription { get; set; }
		public string State { get; set; }
		public string CompanyId { get; set; }
		[JsonProperty("parentOrgId")]
		public string ParentCompanyId { get; set; }
		public bool IsAssigned { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public IList<ProductProperties> SiteList { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string LlcName { get; set; }
	}
}
