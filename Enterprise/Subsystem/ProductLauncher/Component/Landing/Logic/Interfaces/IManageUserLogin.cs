using System;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
	/// <summary>
	/// Interface for ManageUserLogin
	/// </summary>
	public interface IManageUserLogin
	{
		/// <summary>
		/// Create a Person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="userLogin">UserLogin object of the parameter values</param>
		/// <returns>RepositoryResponse object</returns>
		RepositoryResponse CreateUserLogin(Guid realPageId, IUserLogin userLogin);

        /// <summary>
        /// GetUserLogin
        /// </summary>
        /// <param name="realPageId"></param>
        /// <param name="orgPartyId"></param>
        /// <returns>UserLogin with statuses</returns>
        UserLogin GetUserLogin(Guid realPageId, long orgPartyId);

        /// <summary>
        /// GetUserLogin
        /// </summary>
        /// <param name="userLogin"></param>
        /// <param name="orgPartyId"></param>
        /// <returns>UserLogin with statuses</returns>
        UserLogin GetUserLogin(UserLogin userLogin, long orgPartyId);

        /// <summary>
        /// GetUserLogin
        /// </summary>
        /// <param name="realPageId"></param>
        /// <returns>UserLogin without statuses</returns>
        UserLoginOnly GetUserLoginOnly(Guid realPageId);

        /// <summary>
        /// GetUserLogin
        /// </summary>
        /// <param name="enterpriseUserName"></param>
        /// <returns>UserLogin without statuses</returns>
        UserLoginOnly GetUserLoginOnly(string enterpriseUserName);

        /// <summary>
        /// GetUserLogin
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>UserLogin without statuses</returns>
        UserLoginOnly GetUserLoginOnly(long userId);

        /// <summary>
        /// Update an existing UserLogin
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="userLogin">UserLogin object of the parameter values</param>
        /// <returns>RepositoryResponse object</returns>
        RepositoryResponse UpdateUserLogin(Guid realPageId, IUserLogin userLogin);

		/// <summary>
		/// Patch userlogin
		/// </summary>
		/// <param name="userLogins">List of userLogin that will be patched</param>
		/// <param name="userLoginStatusType">Status to patch</param>
		/// <returns>RepositoryResponse object</returns>
		IList<RepositoryResponse> UpdateUserLogins(IList<UserLogin> userLogins, UserUiStatusType userLoginStatusType);

		/// <summary>
		/// Patch userlogin
		/// </summary>
		/// <param name="userLogins">List of userLogin that will be patched</param>
		/// <param name="userLoginStatusType">Status to patch</param>
		/// <returns>RepositoryResponse object</returns>
		RepositoryResponse UpdateBulkUserLogins(IList<UserLoginOnly> userLogins, UserUiStatusType userLoginStatusType);

        /// <summary>
        /// Get the user expiration auto logout interval
        /// </summary>
        /// <param name="realPageId"></param>
        /// <param name="orgPartyId"></param>
        /// <returns>RepositoryResponse object</returns>
        LogOutIntervalResponse GetLogOutInterval(Guid realPageId, long orgPartyId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        RepositoryResponse UpdateLastLogin(string username);

		/// <summary>
		/// Create or Update User status  
		/// </summary>
		bool CreateUpdateUserStatus(Guid realPageId, UserUiStatusType uiStatusTypeName);

		/// <summary>
		/// Active user Update status  
		/// </summary>
		bool UpdateActiveUserStatus(Guid realPageId, UserUiStatusType uiStatusTypeName);

		/// <summary>
		/// Link Identity Provider to a UserLogin
		/// </summary>
		/// <param name="PersonaId">PersonaId</param>
		/// <param name="UserId">UserLogin unique Id</param>
		/// <param name="ContactMechanismId">Contact Mechanism Id</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse LinkIdentityProviderToUserLogin(long PersonaId, long UserId, int ContactMechanismId);

		/// <summary>
		/// Check if username is already existing
		/// </summary>
		/// <param name="realPageId">User RealPageId</param>
		/// <param name="enterpriseUsername">Username</param>
		bool ValidateUsername(Guid realPageId, string enterpriseUsername);

        /// <summary>
        /// User Exists? User Exists in this Organization?
        /// </summary>
        /// <param name="loginName">User LoginName</param>
        /// <param name="organizationRealPageId">Unique Identifier - OrganizationRealPageId</param>
        /// <param name="userRealPageId">The id of the user if editing</param>
        /// <param name="isFromExport"></param>
        /// <param name="userType"></param>
        /// <returns>UserOrganizationExists object</returns>
        UserOrganizationExists IsLoginNameExists(string loginName, Guid organizationRealPageId, Guid userRealPageId, string firstName = null, string lastName = null, int userType = 0, bool isFromExport = false);

        /// <summary>
        /// Gets a list of organizations for the given login name
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="organizationRealPageId"></param>
        /// <returns></returns>
        IList<UserOrganization> GetUserPersonaOrganization(string loginName, Guid? organizationRealPageId = null);

        /// <summary>
        /// list of Organization By Enterprise User Id
        /// </summary>
        /// <param name="realPageId">Unique Identifier - EnterpriseUserId</param>
        /// <param name="relationshipType">Parties Relationhip type name (Optional)</param>
        /// <returns>List of Organization</returns>
        IList<Organization> ListOrganizationByEnterpriseUserId(Guid realPageId, string relationshipType = null);

        /// <summary>
        /// Used to get the specified organization for the given user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="lastLogin"></param>
        /// <param name="orgPartyId"></param>
        /// <param name="getPrimaryOrg"></param>
        /// <returns>Organization status</returns>
        OrganizationStatus GetUserOrganizationWithStatus(long userId, DateTime lastLogin, long orgPartyId, bool getPrimaryOrg);

        /// <summary>
        /// Used to resend email invitation to users
        /// </summary>		
        /// <param name="userLogins"></param> 	
        /// <param name="isCalledFromService"></param> 
        /// <returns></returns>
        bool ResendInvitation(IList<UserLogin> userLogins, bool isCalledFromService = false);
        bool ClearPasswordAndQuestions(Guid realPageId);

        /// <summary>
        /// Gets user claims for non user
        /// </summary>
        /// <param name="userRealPageId"></param>
        /// <returns></returns>
        DefaultUserClaim GetUserClaimsFromNonUser(Guid userRealPageId);

        /// <summary>
        /// Log that an existing user requested to resemd an email link
        /// </summary>
        /// <param name="userRealPageId"></param>
        void LogUserRequestedEmailLinkResent(Guid userRealPageId);

        /// <summary>
        /// Check user domain valid or not
        /// </summary>
        bool IsUserEmailDomainValid(string loginName,string firstName = null,string lastName = null);
    }
}