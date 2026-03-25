using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for UserRepository
	/// </summary>
	public interface IUserLoginRepositoryAsync
	{
		/// <summary>
		/// Create a new UserLogin
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="userLogin">UserLogin object of the parameter values</param>
		/// <returns>Repository response object</returns>
		Task<RepositoryResponse> CreateUserLoginAsync(Guid realPageId, IUserLogin userLogin);

		/// <summary>
        /// Get the user detail by party unique identifier and company Id
        /// </summary>
        /// <param name="realPageId">unique identifier</param>
        /// <param name="orgPartyId">Organization party id</param>
        /// <returns>User object</returns>
        Task<UserLogin> GetUserLoginAsync(Guid realPageId, long orgPartyId);

        /// <summary>
        /// Update User
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="userLogin">User object of the parameter values</param>
        /// <param name="organizationPartyId">organizationPartyId</param>
        /// <returns>Repository response object</returns>
        Task<RepositoryResponse> UpdateUserLoginAsync(Guid realPageId, IUserLogin userLogin, long organizationPartyId);

		/// <summary>
		/// Update Last Login
		/// </summary>
		/// <param name="username"></param>
		/// <returns></returns>
		Task<RepositoryResponse> UpdateLastLoginAsync(string username);

	
		/// <summary>
		/// Create if no user status record exists else update existing user status
		/// </summary>        
		/// <param name="userRealPageIdList">User RealPageId List  </param> 
		/// <param name="statusTypeId">statusType Id </param> 
		/// <param name="fromDate">from Date </param> 
		/// <param name="thruDate">thru Date</param> 
		/// <param name="organizationPartyId">Organization PartyId</param>
		/// <returns>int </returns>
		Task<int> UpdateBulkUserStatusAsync(IList<Guid> userRealPageIdList, int statusTypeId, DateTime fromDate, DateTime? thruDate, long organizationPartyId);
		
		/// <summary>
		/// Link Identity Provider to a UserLogin
		/// </summary>
		/// <param name="personaId">PersonaId</param>
		/// <param name="userId">UserLogin unique Id</param>
		/// <param name="contactMechanismId">Contact Mechanism Id</param>
		/// <returns>Repository response object</returns>
		Task<RepositoryResponse> LinkIdentityProviderToUserLoginAsync(long personaId, long userId, int contactMechanismId);

        /// <summary>
        /// List Persona and Organization by User LoginName
        /// </summary>
        /// <param name="loginName">User login name</param>
        /// <param name="organizationRealPageId"></param>        
        /// <returns>List of User Persona and Organization detail</returns>
        Task<IList<UserOrganization>> ListOrganizationByLoginNameAsync(string loginName, Guid? organizationRealPageId = null);

        /// <summary>
		/// List all organization for user without thru data condition
		/// </summary>
		/// <param name="loginName">User login name</param>        
		/// <returns>List of User Persona and Organization detail</returns>
		Task<IList<UserOrganization>> ListAllOrganizationByLoginNameAsync(string loginName);

        /// <summary>
        /// Get Activity Attempt Exceeds
        /// </summary>
        Task<ActivityAttemptDetails> GetActivityAttemptExceedsAsync(long organizationPartyId, string enterpriseUserName, int activityId);

        Task<UserLoginOnly> GetUserLoginOnlyAsync(string enterpriseUserName);

        Task<UserLoginOnly> GetUserLoginOnlyAsync(long userId);
        Task<UserLoginOnly> GetUserLoginOnlyAsync(Guid realPageId);

        /// <summary>
        /// List Organization without status by User id
        /// </summary>
        /// <param name="userId">User id</param>        
        /// <returns>List of Organizations and their status for the user</returns>
        Task<IList<OrganizationStatus>> ListOrganizationWithoutStatusByUserIdAsync(long userId);

        /// <summary>
        /// list of Organization By Enterprise User Id
        /// </summary>
        /// <param name="realPageId">Unique Identifier - EnterpriseUserId</param>
        /// <param name="relationshipType">Parties Relationhip type name (Optional)</param>
        /// <returns>List of Organization</returns>
        Task<IList<Organization>> ListOrganizationByEnterpriseUserIdAsync(Guid realPageId, string relationshipType);

        /// <summary>
        /// Get Default OrgId For User
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>Organization Id</returns>
        Task<long> GetPrimaryOrgIdByUserIdAsync(long userId, CancellationToken token);

        /// <summary>
        /// The primary org by User id (without status!)
        /// </summary>
        /// <param name="userId">User id</param>        
        /// <returns>The primary organization and its status for the user</returns>
        Task<OrganizationStatus> GetPrimaryOrgWithoutStatusByUserIdAsync(long userId);

        /// <summary>
        /// Get the primary login org status
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="lastLogin"></param>
        /// <param name="organizationPartyId"></param>
        /// <param name="getPrimaryOrg"></param>
        Task<OrganizationStatus> GetUserOrganizationWithStatusAsync(long userId, DateTime? lastLogin, long organizationPartyId, bool getPrimaryOrg);

        /// <summary>
        /// Get the Blacklisted Domains
        /// </summary>
        /// <returns>List of blacklisted domains</returns>
        Task<IList<string>> GetBlacklistedDomainsAsync();
    }
}