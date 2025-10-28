using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.ResidentPortal
{
	/// <summary>
	/// Community a Staff user has access to
	/// </summary>
	public class Community : ICommunity
	{
		/// <summary>
		/// Role Id
		/// </summary>
		[JsonProperty("RoleId", NullValueHandling = NullValueHandling.Ignore)]
		public long RoleId { get; set; }

		/// <summary>
		/// User Id
		/// </summary>
		[JsonProperty("UserId", NullValueHandling = NullValueHandling.Ignore)]
		public long UserId { get; set; }

		/// <summary>
		/// Company Id
		/// </summary>
		[JsonProperty("companyId", NullValueHandling = NullValueHandling.Ignore)]
		public long CompanyId { get; set; }

		/// <summary>
		/// Community Id
		/// </summary>
		[JsonProperty("communityId", NullValueHandling = NullValueHandling.Ignore)]
		public long CommunityId { get; set; }

		/// <summary>
		/// Unit Id
		/// </summary>
		[JsonProperty("UnitId", NullValueHandling = NullValueHandling.Ignore)]
		public long UnitId { get; set; }

		/// <summary>
		/// Unit Revision Id
		/// </summary>
		[JsonProperty("UnitRevisionId", NullValueHandling = NullValueHandling.Ignore)]
		public long UnitRevisionId { get; set; }

		/// <summary>
		/// Role Type
		/// </summary>
		[JsonProperty("RoleType", NullValueHandling = NullValueHandling.Ignore)]
		public string RoleType { get; set; }

		/// <summary>
		/// Resident Groups
		/// </summary>
		[JsonProperty("ResidentGroups", NullValueHandling = NullValueHandling.Ignore)]
		public string ResidentGroups { get; set; }

		/// <summary>
		/// resident Display Unit
		/// </summary>
		[JsonProperty("ResidentDisplayUnit", NullValueHandling = NullValueHandling.Ignore)]
		public string ResidentDisplayUnit { get; set; }

		/// <summary>
		/// resident Display Public Profile
		/// </summary>
		[JsonProperty("ResidentDisplayPublicProfile", NullValueHandling = NullValueHandling.Ignore)]
		public string ResidentDisplayPublicProfile { get; set; }

		/// <summary>
		/// Resident Allow Send Message
		/// </summary>
		[JsonProperty("ResidentAllowSendMessage", NullValueHandling = NullValueHandling.Ignore)]
		public string ResidentAllowSendMessage { get; set; }

		/// <summary>
		/// Admin Level
		/// </summary>
		[JsonProperty("AdminLevel", NullValueHandling = NullValueHandling.Ignore)]
		public string AdminLevel { get; set; }

		/// <summary>
		/// Community Title
		/// </summary>
		[JsonProperty("CommunityTitle", NullValueHandling = NullValueHandling.Ignore)]
		public string CommunityTitle { get; set; }

		/// <summary>
		/// Manager Title
		/// </summary>
		[JsonProperty("ManagerTitle", NullValueHandling = NullValueHandling.Ignore)]
		public string ManagerTitle { get; set; }
	}
}
