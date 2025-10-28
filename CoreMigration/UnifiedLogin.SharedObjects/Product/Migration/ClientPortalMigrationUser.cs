using Newtonsoft.Json;
using System;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.Migration
{
    /// <summary>
    /// 
    /// </summary>
    public class ClientPortalMigrationUser
    {
        /// <summary>
        /// The user id of the user
        /// </summary>
        [JsonProperty("Id")]
        public string UserId { get; set; }

        /// <summary>
        /// The first name of the user
        /// </summary>
        [JsonProperty("FirstName")]
        public string FirstName { get; set; }


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
        /// The user name of the user
        /// </summary>
        [JsonProperty("Username")]
        public string Username { get; set; }

        /// <summary>
        /// users last login date
        /// </summary>
        [JsonProperty("LastLoginDate")]
        public DateTime? LastLoginDate { get; set; }
        /// <summary>
        /// The status of the user
        /// </summary>
        [JsonProperty("IsActive")]
        public bool IsActive { get; set; }

        /// <summary>
        /// The status of the user
        /// </summary>
        [JsonProperty("ProfileId")]
        public string ProfileId { get; set; }
    }
}
