using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.Migration
{
    /// <summary>
    /// 
    /// </summary>
    public class MigrationUser
    {
        #region Ctor
        /// <summary>
        /// Migration User
        /// </summary>
        public MigrationUser()
        {
            Properties = new List<MigrationProperty>();
        }
        #endregion
        /// <summary>
        /// The comapny instance source id of the user
        /// </summary>
        /// 
        [JsonProperty("CompanyInstanceSourceId")]
        public string CompanyInstanceSourceId { get; set; }

        /// <summary>
        /// The comapny instance source id of the user
        /// </summary>
        /// 
        [JsonProperty("CompanySourceInstanceId")]
        private string CompanySourceInstanceId
        {
            set
            {
                CompanyInstanceSourceId = value;
            }
        }

        /// <summary>
        /// The user id of the user
        /// </summary>
        [JsonProperty("UserId")]
        public string UserId { get; set; }

        /// <summary>
        /// The first name of the user
        /// </summary>
        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        /// <summary>
        /// The middle name of the user
        /// </summary>
        [JsonProperty("MiddleName")]
        public string MiddleName { get; set; }

        /// <summary>
        /// The last name of the user
        /// </summary>
        [JsonProperty("LastName")]
        public string LastName { get; set; }

        /// <summary>
        /// The email of the user
        /// </summary>
        [JsonProperty("Email")]
        public string Email { get; set; }

		/// <summary>
		/// The Notification email address of the user
		/// </summary>
		[JsonProperty(PropertyName = "leadEmailAddress")]
		public string LeadEmailAddress { get; set; }

		/// <summary>
		/// The user name of the user
		/// </summary>
		[JsonProperty("Username")]
        public string Username { get; set; }

        /// <summary>
        /// The title of the user
        /// </summary>
        [JsonProperty("Title")]
        public string Title { get; set; }

        /// <summary>
        /// The status of the user
        /// </summary>
        [JsonProperty("Status")]
        public string Status { get; set; }

        /// <summary>
        /// The phone number of the user
        /// </summary>
        [JsonProperty("Phone")]
        public string Phone { get; set; }

        /// <summary>
        /// The last activity of the user
        /// </summary>
        [JsonProperty("LastActivity")]
        public string LastActivity { get; set; }

        /// <summary>
        /// Extra
        /// </summary>
        public string Extra { get; set; }

        /// <summary>
        /// List of properties for a user
        /// </summary>
        [JsonProperty("Properties")]
        public IList<MigrationProperty> Properties { get; set; }

        /// <summary>
        /// Nwp User Type
        /// </summary>
        [JsonProperty("NwpUserType")]
        public string NwpUserType
        {
            set
            {
                Extra = value;
            }
        }
        /// <summary>
        /// Employee Id of the user
        /// </summary>
        public string EmployeeId { get; set; }

        [JsonProperty("isActive")]
        public bool isActive { get; set; }

        /// <summary>
        /// isMigratedUser for user
        /// </summary>
        /// 
        [JsonProperty("isMigratedUser")]
        public bool isMigratedUser { get; set; }

        /// <summary>
        /// isAdminUser for user
        /// </summary>
        /// 
        [JsonProperty("isAdminUser")]
        public bool isAdminUser { get; set; }

        /// <summary>
        /// isRealPageEmployee for user
        /// </summary>
        /// 
        [JsonProperty("isRealPageEmployee")]
        public bool isRealPageEmployee { get; set; }

        /// <summary>
        /// isReadOnly for user
        /// </summary>
        /// 
        [JsonProperty("isReadOnly")]
        public bool isReadOnly { get; set; }
    }    
}
