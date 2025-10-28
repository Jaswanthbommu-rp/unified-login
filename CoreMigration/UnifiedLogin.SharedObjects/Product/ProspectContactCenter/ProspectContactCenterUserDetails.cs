using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.ProspectContactCenter
{
    /// <summary>
    /// Used to store information about a prospect contact center user
    /// </summary>
    public class ProspectContactCenterUserDetails
    {
        /// <summary>
        /// Used to store the id of the user
        /// </summary>
        [JsonProperty("SystemIdentifier")]
        public long SystemIdentifier { get; set; }
        /// <summary>
        /// Used to store the first name of the user
        /// </summary>
        [JsonProperty("FirstName")]
        public string FirstName { get; set; }
        /// <summary>
        /// Used to store the last name of the user
        /// </summary>
        [JsonProperty("LastName")]
        public string LastName { get; set; }
        /// <summary>
        /// Used to store the login name of the user
        /// </summary>
        [JsonProperty("LoginName")]
        public string LoginName { get; set; }
        /// <summary>
        /// Used to store the login name of the user
        /// </summary>
        [JsonProperty("UserActive")]
        public bool UserActive { get; set; }
        /// <summary>
        /// Used to store the login name of the user
        /// </summary>
        [JsonProperty("UserType")]
        public string UserType { get; set; }
        /// <summary>
        /// Used to store the login name of the user
        /// </summary>
        [JsonProperty("ManagementCompanyID")]
        public int ManagementCompanyID { get; set; }
        /// <summary>
        /// Used to store the login name of the user
        /// </summary>
        [JsonProperty("PropertyID")]
        public int PropertyID { get; set; }
        /// <summary>
        /// Used to store the login name of the user
        /// </summary>
        [JsonProperty("Email")]
        public string Email { get; set; }

    }
}
