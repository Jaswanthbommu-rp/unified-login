using System.Collections.Generic;
using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model.ClickPay
{
	public class ClickPayUsers
	{
		[JsonProperty("results")]
		public List<IntegrationProductUser> ClickPayUserList { get; set; }
		public long Start { get; set; }
		public long Limit { get; set; }
		public long Size { get; set; }
	}
}
