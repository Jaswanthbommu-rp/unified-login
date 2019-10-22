using System.Collections.Generic;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	/// <summary>
	/// User Exists? User Exists in this Organization?
	/// </summary>
	public class UserOrganizationExists : IUserOrganizationExists
	{
		/// <summary>
		/// User with this LoginName Exists?
		/// </summary>
		[JsonProperty(PropertyName = "UserExists")]
		public bool UserExists { get; set; }

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

        /// <summary>
        /// The attributes about the person if it exists
        /// </summary>
        public IPerson Person { get; set; }

        /// <summary>
        /// The features not available if the user exists
        /// </summary>
        public Dictionary<string, List<string>> Restricted { get; set; }

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
