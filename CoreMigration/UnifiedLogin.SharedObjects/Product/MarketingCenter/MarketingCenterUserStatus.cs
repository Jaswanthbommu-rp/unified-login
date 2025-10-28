namespace UnifiedLogin.SharedObjects.Product.MarketingCenter
{
	/// <summary>
	/// THe Marketing Center user Status
	/// </summary>
	public class MarketingCenterUserStatus : IMarketingCenterUserStatus
	{
		public bool isActive { get; set; }

		public bool isActiveUnifiedUser { get; set; }

		public long auditUserId { get; set; }
	}
}
