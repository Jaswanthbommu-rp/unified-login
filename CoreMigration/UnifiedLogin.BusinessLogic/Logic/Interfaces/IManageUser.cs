using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
	/// <summary>
	/// Interface for Manage User repository calls
	/// </summary>
	public interface IManageUser
    {
		/// <summary>
		/// Give administrators access to missing products based on a customer company
		/// </summary>
		/// <param name="organizationRealPageId">Organization enterprise Id</param>
		/// <param name="assignUserPersonaId">Assigned to user PersonaId</param>
		RepositoryResponse AssignProductsToAdministrators(Guid organizationRealPageId, long assignUserPersonaId = 0);

        /// <summary>
        /// Validate New User
        /// </summary> 
        /// <param name="enterpriseUserName">Enterprise UserName</param>
        /// <param name="newUserRegistrationToken">new User Registration Token</param>
        /// <returns>ValidateUserResponse object</returns>
        ValidateUserResponse ValidateUser(string enterpriseUserName, string newUserRegistrationToken);

		/// <summary>
		/// Get Starter Profile Options
		/// </summary> 
		/// <param name="enterpriseUserName">Enterprise UserName</param>
		/// <returns>StarterProfileOptionsResponse object</returns>
		StarterProfileOptionsResponse GetStarterProfileOptions(string enterpriseUserName);

		/// <summary>
		/// Set Starter Profile  
		/// </summary> 
		/// <param name="starterProfile">StarterProfile object</param>
		/// <returns>SetStarterProfile object</returns>
		SetStarterProfile SetStarterProfile(StarterProfile starterProfile);

        /// <summary>
        /// Create user
        /// </summary> 
        /// <param name="profile">profiledetails of the new user</param>
        /// <param name="persona">Persona of the new user</param>
        /// <param name="createdByEnterpriseAPI">To change the activity log</param>
        /// <returns>CreateUserResponse and Error object</returns>
        CreateUserResponse<IErrorData> CreateUser(ProfileDetail profile, IList<Persona> persona, bool createdByEnterpriseAPI = false);

		/// <summary>
		/// Update New User Profile
		/// </summary> 
		/// <param name="userLogin">User Login of the New User</param>
		/// <param name="newProfile">Profile of the New User</param>
		/// <param name="partyRoleTypeId">PartyRoleTypeId of the New User</param>
		/// <param name="companyJobTitle">Job Title of the New User</param>
		/// <param name="activityToken">Activity Token for Validation</param>
		/// <returns>RepositoryResponse object</returns>
		RepositoryResponse UpdateNewUser(string userLogin, Profile newProfile, int partyRoleTypeId, string companyJobTitle, string activityToken);

		/// <summary>
		/// Validate registration verification token is associated with user name
		/// </summary>
		/// <param name="enterpriseUserName">Enterprise UserName</param>
		/// <param name="verificationToken">verification Token</param>
		/// <returns>ValidateUserResponse object</returns>
		ValidateUserResponse ValidateRegistrationVerificationToken(string enterpriseUserName, string verificationToken);

		/// <summary>
		/// Update User Detail and Products
		/// </summary>
		/// <param name="loggedInUserRealPageId">Logged-In User unique identifier</param>
		/// <param name="profile">Edited User detail and Products</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse UpdateUser(Guid loggedInUserRealPageId, IProfileDetail profile);

	    /// <summary>
	    /// Used to update the product status for the given list of users
	    /// </summary>
	    /// <param name="editorRealPageId"></param>
	    /// <param name="editorPersonaId"></param>
	    /// <param name="userLogins"></param>
	    /// <param name="userLoginStatusType"></param>
	    /// <returns></returns>
	    RepositoryResponse UpdateUserStatus(Guid editorRealPageId, long editorPersonaId, IList<UserLoginOnly> userLogins, UserUiStatusType? userLoginStatusType);

        /// <summary>
        /// Used to disable the product status for the given list of users
        /// </summary>	
        /// <param name="userLogins"></param>		
        /// <returns></returns>
        RepositoryResponse DisableUsersFromProducts(IList<ProcessUserLogin> userLogins);

		/// <summary>
		/// Used to disable the product status for the given list of users
		/// </summary>	
		/// <param name="userIds"></param>
		/// <param name="isEnabled"></param>
		/// <returns></returns>
		RepositoryResponse ThirdPartyIdpBulkUpdate(IList<long> userIds, bool isEnabled);

		/// <summary>
		/// Used to check if the user has the right
		/// </summary>	
		/// <param name="productBatch"> Product Batch</param>		
		/// <returns></returns>
		bool CheckProductRight(ProductBatch productBatch);

		/// <summary>
		/// Get the user profile
		/// </summary>
		/// <param name="realPageId">Real page identifier</param>
		/// <param name="realpageUserId">Real page user identifier</param>
		/// <param name="orgPartyId">Organization party identifier</param>
		/// <returns>A detail of profile</returns>
		ObjectOutput<IProfileDetail, IErrorData> GetUserProfile(Guid realPageId, Guid realpageUserId, long orgPartyId);

		/// <summary>
		/// Get the an UserEmployee by UserLoginPersonaId and OrganizationPartyId
		/// </summary>
		/// <param name="UserLoginPersonaId"></param>
		/// <param name="OrganizationPartyId"></param>
		IUserEmployeeId GetUserEmployeeId(long UserLoginPersonaId, long OrganizationPartyId);
		
	}
}