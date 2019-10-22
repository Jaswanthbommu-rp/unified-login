using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	/// <summary>
	/// Interface for UserOrganizationExists
	/// </summary>
	public interface IUserOrganizationExists
	{
		/// <summary>
		/// User with this LoginName Exists?
		/// </summary>
		bool UserExists { get; set; }

		/// <summary>
		/// User with this LoginName exists in the Organization with this RealPageId
		/// </summary>
		bool UserExistsInThisOrganization { get; set; }

        /// <summary>
        /// Used to indicate if the user login already used is a user type of Regular User (No Email)
        /// </summary>
        bool UserExistsAsNoEmail { get; set; }
        
        /// <summary>
        /// Used to indicate if the user login exists but is not usable
        /// </summary>
        bool UserExistsNotAvailable { get; set; }

        /// <summary>
        /// The attributes about the person if it exists
        /// </summary>
        IPerson Person { get; set; }

        /// <summary>
        /// The features not available if the user exists
        /// </summary>
        Dictionary<string, List<string>> Restricted { get; set; }
    }
}