using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.ResidentPortal
{
	/// <summary>
	/// Interface for Resident Portal user
	/// </summary>
	public interface IResidentPortalUser
	{
		#region Common Properties
		/// <summary>
		/// Contact email
		/// </summary>
		string ContactEmail { get; set; }
		
		/// <summary>
		/// The email address of the user
		/// </summary>
		string Email { get; set; }

		/// <summary>
		/// The first name of the user
		/// </summary>
		string FirstName { get; set; }

		/// <summary>
		/// The last name of the user
		/// </summary>
		string LastName { get; set; }

		/// <summary>
		/// level of access - Format from and to Resident Portal
		/// </summary>
		string Level { get; set; }

		/// <summary>
		/// level of access object - Format from and to UI
		/// </summary>
		List<ILevel> Levels { get; set; }

		/// <summary>
		/// Staff user notification settings (optional)
		/// </summary>
		Notifications Notifications { get; set; }

		/// <summary>
		/// Roles data from Resident Portal
		/// </summary>
		Dictionary<string, string> canCreateRoles { get; set; }

		/// <summary>
		/// Allowed Roles data from Resident Portal when editing a user
		/// </summary>
		Dictionary<string, string> allowedRoles { get; set; }
		#endregion

		#region Enterprise
		/// <summary>
		/// Array of Communities (Format from UI into ProductBatch AND Format from Resident Portal API for Enterprise Users with Limited CommunityAccessLevel)
		/// </summary>
		List<long> CommunityIds { get; set; }
		
		/// <summary>
		/// Community Access Level
		/// </summary>
		string CommunityAccessLevel { get; set; }

		/// <summary>
		/// Does the enterprise user have access to all properties in OneSite
		/// </summary>
		bool AllProperties { get; set; }

		/// <summary>
		/// Enterprise userId
		/// </summary>
		long EnterpriseUserId { get; set; }

		/// <summary>
		/// UserId
		/// </summary>
		long UserId { get; set; }

		/// <summary>
		/// The id for the company in Resident Portal
		/// </summary>
		long CompanyId { get; set; }
		#endregion

		#region Manager
		/// <summary>
		/// List of communities the Staff/Manager has access to
		/// </summary>
		List<Community> Communities { get; set; }

		/// <summary>
		/// Community Id
		/// </summary>
		long CommunityId { get; set; }
		
		/// <summary>
		/// UserId (Manager Id)
		/// </summary>
		long ManagerId { get; set; }

		/// <summary>
		/// Public Profile Id
		/// </summary>
		long PublicProfileId { get; set; }

		/// <summary>
		/// Title (Manager custom title)
		/// </summary>
		string Title { get; set; }

		/// <summary>
		/// Office Phone
		/// </summary>
		string OfficePhone { get; set; }

		/// <summary>
		/// Mobile Phone
		/// </summary>
		string MobilePhone { get; set; }

		/// <summary>
		/// Display In Corner
		/// </summary>
		bool? DisplayInCorner { get; set; }

		/// <summary>
		/// Date Created
		/// </summary>
		DateTime? DateCreated { get; set; }

		/// <summary>
		/// Date Updated
		/// </summary>
		DateTime? DateUpdated { get; set; }

		/// <summary>
		/// Groups (Messaging groups) - Format from and to Resident Portal : Used in the GET from Resident Portal
		/// </summary>
		List<string> MessageGroups { get; set; }

		/// <summary>
		/// Groups (Messaging groups) - Format from and to Resident Portal : Used in the POST to Resident Portal
		/// </summary>
		List<string> Groups { get; set; }


		/// <summary>
		/// Groups (Messaging groups) - Format from and to UI
		/// </summary>
		List<IMessagingGroups> MessagingGroups { get; set; }
		#endregion
	}
}