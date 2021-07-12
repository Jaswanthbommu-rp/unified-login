using Newtonsoft.Json;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.Export
{
	/// <summary>
	/// User data to Export
	/// </summary>
	public class User : IUser
	{
		/// <summary>
		/// Firstname
		/// </summary>
		[JsonProperty(PropertyName = "FirstName")]
		public string FirstName { get; set; }

		/// <summary>
		/// Middle initial
		/// </summary>
		[JsonProperty(PropertyName = "MiddleName")]
		public string MiddleName { get; set; } = "";

		/// <summary>
		/// Lastname
		/// </summary>
		[JsonProperty(PropertyName = "LastName")]
		public string LastName { get; set; }

		/// <summary>
		/// LoginName
		/// </summary>
		[JsonProperty(PropertyName = "LoginName")]
		public string LoginName { get; set; }

		/// <summary>
		/// User last login date
		/// </summary>
		[JsonProperty(PropertyName = "LastLogin")]
		public string LastLogin { get; set; }

		/// <summary>
		/// Sortable user statuses (Active|Disabled|Pending|Expired).
		/// </summary>
		[JsonProperty(PropertyName = "Status")]
		public string Status { get; set; }

		/// <summary>
		/// Use third party identity service provider.  Default to true so that the toggle switch is on on the Add new User.
		/// </summary>
		[JsonProperty(PropertyName = "IDP")]
		public string IDP { get; set; }

		/// <summary>
		/// Number products the user is authorized to access 
		/// </summary>
		[JsonProperty(PropertyName = "Products")]
		public int Products { get; set; }

		/// <summary>
		/// User type as it appears on the User details such Regular User, Regular User (no email), RealPage System Administrator
		/// </summary>
		[JsonProperty(PropertyName = "UserType")]
		public string UserType { get; set; }

		/// <summary>
		/// When the account can be used
		/// </summary>
		[JsonProperty(PropertyName = "EffectiveDate")]
		public string EffectiveDate { get; set; }

		/// <summary>
		/// When the account can no longer be used
		/// </summary>
		[JsonProperty(PropertyName = "ExpireDate")]
		public string ExpireDate { get; set; }

		/// <summary>
		/// Enabled Custom Field value with the smallest sequence
		/// </summary>
		[JsonProperty(PropertyName = "CustomField")]
		public string CustomField { get; set; }

		/// <summary>
		/// Employee Id
		/// </summary>
		[JsonProperty(PropertyName = "EmployeeId")]
		public string EmployeeId { get; set; } = "";

		/// <summary>
		/// Notification Email
		/// </summary>
		[JsonProperty(PropertyName = "Notificationemail")]
		public string Notificationemail { get; set; }
		 
		/// <summary>
		/// PlatformRoles
		/// </summary>
		[JsonProperty(PropertyName = "PlatformRoles")]
		public string PlatformRoles { get; set; }
		 
		/// <summary>
		/// MFAFlag
		/// </summary>
		[JsonProperty(PropertyName = "MFAFlag")]
		public string MFAFlag { get; set; }

		[JsonProperty(PropertyName = "ProdList")]
		public List<KeyValuePair<string, object>> ProdList = new List<KeyValuePair<string, object>>();

		public User(string value)
		{
			this.Value = value;
		}
		public string Value { get; set; }
	}
}
