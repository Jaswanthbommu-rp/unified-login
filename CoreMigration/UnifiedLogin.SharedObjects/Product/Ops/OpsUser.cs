using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.Ops
{
    /// <summary>
    /// Used to create/update an Ops user
    /// </summary>
    public class OpsUser
    {
        /// <summary>
        /// The id of the user
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string ID { get; set; }

        /// <summary>
        /// The first name of the user
        /// </summary>
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        /// <summary>
        /// The middle name of the user
        /// </summary>
        [JsonProperty("middle_name")]
        public string MiddleName { get; set; } = "";

        /// <summary>
        /// The last name of the user
        /// </summary>
        [JsonProperty("last_name")]
        public string LastName { get; set; }

        /// <summary>
        /// The login name of the user
        /// </summary>
        [JsonProperty("login_name")]
        public string Loginname { get; set; }

        /// <summary>
        /// The password of the user
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }

        /// <summary>
        /// The name of the role to assign to the user
        /// </summary>
        [JsonProperty("role_name", NullValueHandling = NullValueHandling.Ignore)]
        public string RoleName { get; set; }

        /// <summary>
        /// The id of the user type for the user
        /// </summary>
        [JsonProperty("user_type_id", NullValueHandling = NullValueHandling.Ignore)]
        public string UserTypeId { get; set; }

        /// <summary>
        /// EmployeeId for the user
        /// </summary>
        [JsonProperty("employee_id", NullValueHandling = NullValueHandling.Ignore)]
        public string EmployeeId { get; set; }

        /// <summary>
        /// The user type information for the user
        /// </summary>
        [JsonProperty("user_type", NullValueHandling = NullValueHandling.Ignore)]
	    public OpsUserType UserType { get; set; }
		
		/// <summary>
		/// 
		/// </summary>
		/// 
		[JsonProperty("asset", NullValueHandling = NullValueHandling.Ignore)]
        public AssetGroup AssetGroup { get; set; }

        /// <summary>
        /// The asset code of the user
        /// </summary>
        [JsonProperty("assetid", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetID { get; set; } = "";

        /// <summary>
        /// The name of the role to assign to the user
        /// </summary>
        [JsonProperty("asset_code", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetCode { get; set; }

        /// <summary>
        /// The name of the role to assign to the user
        /// </summary>
        [JsonProperty("asset_name", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetName { get; set; }

        /// <summary>
        /// The date the user is out of office, not used
        /// </summary>
        [JsonProperty("out_of_office_from", NullValueHandling = NullValueHandling.Ignore)]
        public string OutOfOfficeFrom { get; set; }

        /// <summary>
        /// The date the user is returning to the office, not used
        /// </summary>
        [JsonProperty("out_of_office_to", NullValueHandling = NullValueHandling.Ignore)]
        public string OutOfOfficeTo { get; set; }

        /// <summary>
        /// The phone number for the user
        /// </summary>
        [JsonProperty("phone")]
        public string Phone { get; set; } = "";

        /// <summary>
        /// The fax number for the user
        /// </summary>
        [JsonProperty("fax", NullValueHandling = NullValueHandling.Ignore)]
        public string Fax { get; set; }

        /// <summary>
        /// The email for the user
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// The status for the user, active, inactive
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

    }

	/// <summary>
	/// Used to create/update an Ops user
	/// </summary>
	public class OpsUserPatch
	{
		/// <summary>
		/// The first name of the user
		/// </summary>
		[JsonProperty("first_name", NullValueHandling = NullValueHandling.Ignore)]
		public string FirstName { get; set; }

		/// <summary>
		/// The middle name of the user
		/// </summary>
		[JsonProperty("middle_name", NullValueHandling = NullValueHandling.Ignore)]
		public string MiddleName { get; set; }

		/// <summary>
		/// The last name of the user
		/// </summary>
		[JsonProperty("last_name", NullValueHandling = NullValueHandling.Ignore)]
		public string LastName { get; set; }

        /// <summary>
        /// EmployeeId for the user
        /// </summary>
        [JsonProperty("employee_id", NullValueHandling = NullValueHandling.Ignore)]
        public string EmployeeId { get; set; }

        /// <summary>
        /// The login name of the user
        /// </summary>
        [JsonProperty("login_name", NullValueHandling = NullValueHandling.Ignore)]
		public string Loginname { get; set; }

		/// <summary>
		/// The password of the user
		/// </summary>
		[JsonProperty("password", NullValueHandling = NullValueHandling.Ignore)]
		public string Password { get; set; }

		/// <summary>
		/// The phone number for the user
		/// </summary>
		[JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
		public string Phone { get; set; }

		/// <summary>
		/// The email for the user
		/// </summary>
		[JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
		public string Email { get; set; }

		/// <summary>
		/// The status for the user, active, inactive
		/// </summary>
		[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
		public string Status { get; set; }

	}
}
