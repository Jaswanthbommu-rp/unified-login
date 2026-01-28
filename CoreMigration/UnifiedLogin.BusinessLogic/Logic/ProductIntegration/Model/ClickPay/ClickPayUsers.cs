using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model.ClickPay
{
    [ExcludeFromCodeCoverage]
    public class ClickPayUsers
	{
		[JsonProperty("results")]
		public List<IntegrationProductUser> ClickPayUserList { get; set; }
		public long Start { get; set; }
		public long Limit { get; set; }
		public long Size { get; set; }
	}
}
