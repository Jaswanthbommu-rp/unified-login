using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Enterprise
{
	/// <summary>
	/// User Data
	/// </summary>
	public class UserData : UserDataCommon
	{
		/// <summary>
		/// Password
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Phone
		/// </summary>
		public string Phone { get; set; }

		/// <summary>
		/// Title
		/// </summary>
		public string Title { get; set; }
		 
		/// <summary>
		/// Organization RealPageId
		/// </summary>
		public Guid OrganizationRealPageId { get; set; }

		/// <summary>
		/// PasswordHash
		/// </summary>
		public string PasswordHash { get; set; }

		/// <summary>
		/// PasswordSalt
		/// </summary>
		public string PasswordSalt { get; set; }

		/// <summary>
		/// PhoneType
		/// </summary>
		public string PhoneType { get; set; }

		/// <summary>
		/// PreferredContactMethod
		/// </summary>
		public string PreferredContactMethod { get; set; }

		/// <summary>
		/// User Type
		/// </summary>
		public int UserType { get; set; }

		/// <summary>
		/// CustomFields
		/// </summary>
		public Dictionary<string, string> CustomFields { get; set; }

		/// <summary>
		/// AdditionalFields
		/// </summary>
		public Dictionary<string, string> AdditionalFields { get; set; }

		/// <summary>
		/// CreateUserSourceType
		/// </summary>
		public string CreateUserSourceType { get; set; }

		/// <summary>
		/// CompanyName
		/// </summary>
		public string CompanyName { get; set; }

		/// <summary>
		/// Suffix
		/// </summary>
		public string Suffix { get; set; }

		/// <summary>
		/// Organization Party Id
		/// </summary>
		public long OrganizationPartyId { get; set; }

		/// <summary>
		/// SendInvitationEmail
		/// </summary>
		public bool? SendInvitationEmail { get; set; }
	}
}