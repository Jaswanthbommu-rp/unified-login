using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.ResidentPortal
{
	/// <summary>
	/// Resident Portal User (Enterprise OR Manager)
	/// </summary>
	public class ResidentPortalUser : IResidentPortalUser
	{
		#region Common Properties
		/// <summary>
		/// Contact email
		/// </summary>
		[JsonProperty(PropertyName = "contactEmail", NullValueHandling = NullValueHandling.Ignore)]
		public string ContactEmail { get; set; } = null;

		/// <summary>
		/// The email address of the user
		/// </summary>
		[JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
		public string Email { get; set; } = null;
		
		/// <summary>
		/// The first name of the user
		/// </summary>
		[JsonProperty(PropertyName = "firstName")]
		public string FirstName { get; set; }

		/// <summary>
		/// The last name of the user
		/// </summary>
		[JsonProperty(PropertyName = "lastName")]
		public string LastName { get; set; }

		/// <summary>
		/// level of access - Format from and to Resident Portal
		/// </summary>
		[JsonProperty(PropertyName = "level", NullValueHandling = NullValueHandling.Ignore)]
		public string Level { get; set; }

		/// <summary>
		/// level of access object - Format from and to UI
		/// </summary>
		[JsonProperty("levels", NullValueHandling = NullValueHandling.Ignore)]
		public List<ILevel> Levels { get; set; } = null;

		/// <summary>
		/// Staff user notification settings (optional)
		/// </summary>
		[JsonProperty(PropertyName = "notifications", NullValueHandling = NullValueHandling.Ignore)]
		public Notifications Notifications { get; set; }

		/// <summary>
		/// Roles data from Resident Portal
		/// </summary>
		[JsonProperty(PropertyName = "canCreateRoles", NullValueHandling = NullValueHandling.Ignore)]
		public Dictionary<string, string> canCreateRoles { get; set; }

		/// <summary>
		/// Allowed Roles data from Resident Portal when editing a user
		/// </summary>
		[JsonProperty(PropertyName = "allowedRoles", NullValueHandling = NullValueHandling.Ignore)]
		public Dictionary<string, string> allowedRoles { get; set; }
		#endregion

		#region Enterprise
		/// <summary>
		/// Community Access Level
		/// </summary>
		[JsonProperty("communityAccessLevel", NullValueHandling = NullValueHandling.Ignore)]
		public string CommunityAccessLevel { get; set; } = null;

		/// <summary>
		/// Does the enterprise user have access to all properties in OneSite
		/// </summary>
		[JsonProperty("AllProperties")]
		public bool AllProperties { get; set; } = false;

		/// <summary>
		/// Enterprise userId
		/// </summary>
		[JsonProperty("enterpriseUserId", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
		public long EnterpriseUserId { get; set; } = 0;

		/// <summary>
		/// UserId
		/// </summary>
		[JsonProperty("userId", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
		public long UserId { get; set; } = 0;

		/// <summary>
		/// The id for the company in Resident Portal
		/// </summary>
		[JsonProperty("companyId", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
		public long CompanyId { get; set; } = 0;

		/// <summary>
		/// Array of Communities (Format from UI into ProductBatch AND Format from Resident Portal API for Enterprise Users with Limited CommunityAccessLevel)
		/// </summary>
		[JsonProperty("communityIds", NullValueHandling = NullValueHandling.Ignore)]
		public List<long> CommunityIds { get; set; } = null;
		#endregion

		#region Manager
		/// <summary>
		/// Community Id
		/// </summary>
		[JsonProperty("communityId", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
		public long CommunityId { get; set; } = 0;

		/// <summary>
		/// List of communities the Staff/Manager has access to
		/// </summary>
		[JsonProperty("Communities", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
		public List<Community> Communities { get; set; } = null;

		/// <summary>
		/// UserId (Manager Id)
		/// </summary>
		[JsonProperty("managerId", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
		public long ManagerId { get; set; } = 0;

		/// <summary>
		/// Public Profile Id
		/// </summary>
		[JsonProperty("publicProfileId", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
		public long PublicProfileId { get; set; } = 0;

		/// <summary>
		/// Title (Manager custom title)
		/// </summary>
		[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
		public string Title { get; set; } = null;

		/// <summary>
		/// Office Phone
		/// </summary>
		[JsonProperty("officePhone", NullValueHandling = NullValueHandling.Ignore)]
		public string OfficePhone { get; set; } = null;

		/// <summary>
		/// Mobile Phone
		/// </summary>
		[JsonProperty("mobilePhone", NullValueHandling = NullValueHandling.Ignore)]
		public string MobilePhone { get; set; } = null;

		/// <summary>
		/// Display In Corner
		/// </summary>
		[JsonProperty("displayInCorner", NullValueHandling = NullValueHandling.Ignore)]
		public bool? DisplayInCorner { get; set; } = null;

		/// <summary>
		/// Date Created
		/// </summary>
		[JsonProperty("dateCreated", NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? DateCreated { get; set; } = null;

		/// <summary>
		/// Date Updated
		/// </summary>
		[JsonProperty("dateUpdated", NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? DateUpdated { get; set; } = null;

		/// <summary>
		/// Groups (Messaging groups) - Format from and to Resident Portal : Used in the GET from Resident Portal
		/// </summary>
		[JsonProperty("messageGroups", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> MessageGroups { get; set; } = null;

		/// <summary>
		/// Groups (Messaging groups) - Format from and to Resident Portal : Used in the POST to Resident Portal
		/// </summary>
		[JsonProperty("groups", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> Groups { get; set; } = null;

		/// <summary>
		/// Groups (Messaging groups) - Format from and to UI
		/// </summary>
		[JsonProperty("messagingGroups", NullValueHandling = NullValueHandling.Ignore)]
		public List<IMessagingGroups> MessagingGroups { get; set; } = null;
		#endregion
	}
}
