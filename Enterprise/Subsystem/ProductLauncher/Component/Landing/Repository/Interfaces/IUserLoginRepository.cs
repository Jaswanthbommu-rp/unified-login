using System;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces
{
	/// <summary>
	/// Interface for UserRepository
	/// </summary>
	public interface IUserLoginRepository
	{
		/// <summary>
		/// Create a new UserLogin
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="userLogin">UserLogin object of the parameter values</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse CreateUserLogin(Guid realPageId, IUserLogin userLogin);

		/// <summary>
        /// Get the user detail by party unique identifier and company Id
        /// </summary>
        /// <param name="realPageId">unique identifier</param>
        /// <param name="orgPartyId">Organization party id</param>
        /// <returns>User object</returns>
        UserLogin GetUserLogin(Guid realPageId, long orgPartyId);

        /// <summary>
        /// Update User
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="userLogin">User object of the parameter values</param>
        /// <param name="organizationPartyId">organizationPartyId</param>
        /// <returns>Repository response object</returns>
        RepositoryResponse UpdateUserLogin(Guid realPageId, IUserLogin userLogin, long organizationPartyId);

		/// <summary>
		/// Update Last Login
		/// </summary>
		/// <param name="username"></param>
		/// <returns></returns>
		RepositoryResponse UpdateLastLogin(string username);

	
		/// <summary>
		/// Create if no user status record exists else update existing user status
		/// </summary>        
		/// <param name="userRealPageIdList">User RealPageId List  </param> 
		/// <param name="statusTypeId">statusType Id </param> 
		/// <param name="fromDate">from Date </param> 
		/// <param name="thruDate">thru Date</param> 
		/// <param name="organizationPartyId">Organization PartyId</param>
		/// <returns>int </returns>
		int UpdateBulkUserStatus(IList<Guid> userRealPageIdList, int statusTypeId, DateTime fromDate, DateTime? thruDate, long organizationPartyId);
		
		/// <summary>
		/// Link Identity Provider to a UserLogin
		/// </summary>
		/// <param name="personaId">PersonaId</param>
		/// <param name="userId">UserLogin unique Id</param>
		/// <param name="contactMechanismId">Contact Mechanism Id</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse LinkIdentityProviderToUserLogin(long personaId, long userId, int contactMechanismId);

		/// <summary>
		/// List Persona and Organization by User LoginName
		/// </summary>
		/// <param name="loginName">User login name</param>        
		/// <returns>List of User Persona and Organization detail</returns>
		IList<UserOrganization> ListOrganizationByLoginName(string loginName);

        /// <summary>
		/// List all organization for user without thru data condition
		/// </summary>
		/// <param name="loginName">User login name</param>        
		/// <returns>List of User Persona and Organization detail</returns>
		IList<UserOrganization> ListAllOrganizationByLoginName(string loginName);

        /// <summary>
        /// Get Activity Attempt Exceeds
        /// </summary>
        ActivityAttemptDetails GetActivityAttemptExceeds(long organizationPartyId, string enterpriseUserName, int activityId);

        UserLoginOnly GetUserLoginOnly(string enterpriseUserName);

        UserLoginOnly GetUserLoginOnly(long userId);

        UserLoginOnly GetUserLoginOnly(Guid realPageId);

        /// <summary>
        /// List Organization without status by User id
        /// </summary>
        /// <param name="userId">User id</param>        
        /// <returns>List of Organizations and their status for the user</returns>
        IList<OrganizationStatus> ListOrganizationWithoutStatusByUserId(long userId);

        /// <summary>
        /// list of Organization By Enterprise User Id
        /// </summary>
        /// <param name="realPageId">Unique Identifier - EnterpriseUserId</param>
        /// <param name="relationshipType">Parties Relationhip type name (Optional)</param>
        /// <returns>List of Organization</returns>
        IList<Organization> ListOrganizationByEnterpriseUserId(Guid realPageId, string relationshipType);

        /// <summary>
        /// Get Default OrgId For User
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>Organization Id</returns>
        long GetPrimaryOrgIdByUserId(long userId);

        /// <summary>
        /// The primary org by User id (without status!)
        /// </summary>
        /// <param name="userId">User id</param>        
        /// <returns>The primary organization and its status for the user</returns>
        OrganizationStatus GetPrimaryOrgWithoutStatusByUserId(long userId);

        /// <summary>
        /// Get the primary login org status
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="lastLogin"></param>
        /// <param name="organizationPartyId"></param>
        /// <param name="getPrimaryOrg"></param>
        OrganizationStatus GetUserOrganizationWithStatus(long userId, DateTime? lastLogin, long organizationPartyId, bool getPrimaryOrg);
    }
}