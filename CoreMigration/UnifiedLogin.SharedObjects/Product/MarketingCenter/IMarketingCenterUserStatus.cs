namespace UnifiedLogin.SharedObjects.Product.MarketingCenter
{
	public interface IMarketingCenterUserStatus
	{
		long auditUserId { get; set; }
		bool isActive { get; set; }
		bool isActiveUnifiedUser { get; set; }
	}
}