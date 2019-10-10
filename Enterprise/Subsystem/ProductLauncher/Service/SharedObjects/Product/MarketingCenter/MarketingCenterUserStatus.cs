using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.MarketingCenter
{
	/// <summary>
	 /// THe Marketing Center user Status
	 /// </summary>
	public class MarketingCenterUserStatus
	{
		public bool isActive { get; set; }
		public bool isActiveUnifiedUser { get; set; }
		public long auditUserId { get; set; }
	}
}
