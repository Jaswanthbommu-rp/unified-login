using Newtonsoft.Json;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.Rum
{
    /// <summary>
    /// Used to create/update an Rum user
    /// </summary>
    public class RumUser
    {
        ///// <summary>
        ///// The id of the user
        ///// </summary>
        //[JsonProperty("userId", NullValueHandling = NullValueHandling.Ignore)]
        //public int UserId { get; set; }

        /// <summary>
        /// The first name of the user
        /// </summary>
        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        /// <summary>
        /// The RealPag eName of the user
        /// </summary>
        [JsonProperty("RealmNm")]
        public string RealPageName { get; set; } = "";

        /// <summary>
        /// The last name of the user
        /// </summary>
        [JsonProperty("LastName")]
        public string LastName { get; set; }

        /// <summary>
        /// The login name of the user
        /// </summary>
        [JsonProperty("UserNm")]
        public string UserName { get; set; }

        /// <summary>
        /// The Code of the user type for the user
        /// </summary>
        [JsonProperty("UserTypeCd", NullValueHandling = NullValueHandling.Ignore)]
        public string UserTypeCode { get; set; }

        /// <summary>
        /// The phone number for the user
        /// </summary>
        [JsonProperty("PhoneNumber")]
        public string Phone { get; set; } = "";

        /// <summary>
        /// The email for the user
        /// </summary>
        [JsonProperty("Email")]
        public string Email { get; set; }

        /// <summary>
        /// The asset code of the user
        /// </summary>
        [JsonProperty("AccessIds", NullValueHandling = NullValueHandling.Ignore)]
        public List<int> AssetIds { get; set; }

        /// <summary>
        /// The Roles of the user
        /// </summary>
        [JsonProperty("Roles", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Roles { get; set; }

        /// <summary>
        /// PMC
        /// </summary>
        [JsonProperty("PortfolioId", NullValueHandling = NullValueHandling.Ignore)]
        public int PortfolioId { get; set; }

    }
    public class UserType
    {
        /// <summary>
        /// Portfolio Manager
        /// </summary>
        public const string PortfolioManager = "PM";
        /// <summary>
        /// Regional Manager
        /// </summary>
        public const string RegionalManager = "RM";
        /// <summary>
        /// Group Manager
        /// </summary>
        public const string GroupManager = "GM";
        /// <summary>
        /// Property Manager
        /// </summary>
        public const string PropertyManager = "PR";
        /// <summary>
        /// SubContractor
        /// </summary>
        public const string SubContractor = "SU";
    }
}
