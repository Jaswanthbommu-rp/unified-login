using System.Collections.Generic;
using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// User Exists? User Exists in this Organization?
	/// </summary>
	public class UserOrganizationExists 
	{
		/// <summary>
		/// User with this LoginName Exists?
		/// </summary>
		[JsonProperty(PropertyName = "UserExists")]
		public bool UserExists { get; set; }

        /// <summary>
        /// True if User is External every where
        /// </summary>
        [JsonProperty(PropertyName = "UserIsExternalEverywhere")]
        public bool UserIsExternalEverywhere { get; set; } = true;

        /// <summary>
		/// User with this LoginName exists in the Organization with this RealPageId
		/// </summary>
		[JsonProperty(PropertyName = "UserExistsInThisOrganization")]
		public bool UserExistsInThisOrganization { get; set; }

        /// <summary>
        /// Used to indicate if the user login already used is a user type of Regular User (No Email)
        /// </summary>
        [JsonProperty(PropertyName = "UserExistsAsNoEmail")]
        public bool UserExistsAsNoEmail { get; set; }

        /// <summary>
        /// Used to indicate if the user login exists but is not usable
        /// </summary>
        [JsonProperty(PropertyName = "UserExistsNotAvailable")]
        public bool UserExistsNotAvailable { get; set; }

        ///// <summary>
        ///// User with this LoginName exists as RealPage System Administrator in diffrent domain 
        ///// </summary>
        [JsonProperty(PropertyName = "UserExistsAsAdminInOtherDomain")]
        public bool UserExistsAsAdminInOtherDomain { get; set; }

        ///// <summary>
        ///// User with this LoginName exists as Regular in diffrent domain 
        ///// </summary>
        [JsonProperty(PropertyName = "UserExistsAsRegularUserInOtherDomain")]
        public bool UserExistsAsRegularUserInOtherDomain { get; set; }
        /// <summary>
        /// Used to indicate if the user is disabled in the Primary company
        /// </summary>
        [JsonProperty(PropertyName = "UserIsDisabledInPrimaryCompany")]
        public bool UserIsDisabledInPrimaryCompany { get; set; }

        /// <summary>
        /// True is Organization with this RealPageId is Realpage Employee company
        /// </summary>
        [JsonProperty(PropertyName = "OrgIsRealpageEmployee")]
        public bool OrgIsRealpageEmployee { get; set; }

        /// <summary>
        /// The attributes about the person if it exists
        /// </summary>
        public IPerson Person { get; set; }

        /// <summary>
        /// The features not available if the user exists
        /// </summary>
        public Dictionary<string, List<string>> Restricted { get; set; }

        /// <summary>
        /// The name of the users primary company
        /// </summary>
        public string PrimaryCompanyName { get; set; } = "";

        /// <summary>
        /// Flag to verify valid domain username or not
        /// </summary>
        [JsonProperty(PropertyName = "isValidDomainUsername")]
        public bool IsValidDomainUsername { get; set; }

        public UserInfoLite SuperVisor { get; set; }

        #region Examples
        /// <summary>
        /// Example for UserOrganizationExists
        /// </summary>
        /// <returns>UserOrganizationExists object</returns>
        public static UserOrganizationExists GetUserOrganizationExistsExample()
		{
			UserOrganizationExists result = new UserOrganizationExists
			{
				UserExists = false,
				UserExistsInThisOrganization = false
			};
			return result;
		}
		#endregion
	}
}
