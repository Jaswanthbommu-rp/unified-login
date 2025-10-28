namespace UnifiedLogin.SharedObjects.Product.ResidentPortal
{
	/// <summary>
	/// Interface for Community
	/// </summary>
	public interface ICommunity
	{
		/// <summary>
		/// Admin Level
		/// </summary>
		string AdminLevel { get; set; }

		/// <summary>
		/// Community Id
		/// </summary>
		long CommunityId { get; set; }

		/// <summary>
		/// Community Title
		/// </summary>
		string CommunityTitle { get; set; }

		/// <summary>
		/// Company Id
		/// </summary>
		long CompanyId { get; set; }

		/// <summary>
		/// Manager Title
		/// </summary>
		string ManagerTitle { get; set; }

		/// <summary>
		/// Resident Allow Send Message
		/// </summary>
		string ResidentAllowSendMessage { get; set; }

		/// <summary>
		/// resident Display Public Profile
		/// </summary>
		string ResidentDisplayPublicProfile { get; set; }

		/// <summary>
		/// resident Display Unit
		/// </summary>
		string ResidentDisplayUnit { get; set; }

		/// <summary>
		/// Resident Groups
		/// </summary>
		string ResidentGroups { get; set; }

		/// <summary>
		/// Role Id
		/// </summary>
		long RoleId { get; set; }

		/// <summary>
		/// Role Type
		/// </summary>
		string RoleType { get; set; }

		/// <summary>
		/// Unit Id
		/// </summary>
		long UnitId { get; set; }

		/// <summary>
		/// Unit Revision Id
		/// </summary>
		long UnitRevisionId { get; set; }

		/// <summary>
		/// User Id
		/// </summary>
		long UserId { get; set; }
	}
}