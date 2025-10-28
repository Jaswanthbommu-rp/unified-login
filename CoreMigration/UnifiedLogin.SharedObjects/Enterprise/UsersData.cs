using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Enterprise
{
	/// <summary>
	/// Users List
	/// </summary>
	public class UsersData : UserDataCommon
	{
		/// <summary>
		/// String JSON of CustomFields
		/// </summary>
		[JsonProperty("CustomFields")]
		public string CustomFields { get; set; }

		/// <summary>
		/// User Role
		/// </summary>
		[JsonProperty(PropertyName = "RoleName")]
		public string RoleName { get; set; }

		/// <summary>
		/// User Status
		/// </summary>
		[JsonProperty("Status")]
		public string Status { get; set; }

		/// <summary>
		/// UserType
		/// </summary>
		[JsonProperty("UserType")]
		public string UserType { get; set; }

		/// <summary>
		/// Comma delimited Products codes by Persona
		/// </summary>
		[JsonProperty("Product")]
		public string Product { get; set; }

		/// <summary>
		/// UserId
		/// </summary>
		[JsonProperty("UserId")]
		public long UserID { get; set; }

		/// <summary>
		/// Total Records
		/// </summary>
		[JsonProperty("TotalRecords")]
		public int TotalRecords { get; set; }
				
	}
}
