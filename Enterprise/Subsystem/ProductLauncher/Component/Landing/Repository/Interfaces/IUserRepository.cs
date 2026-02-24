using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using SO = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces
{
	/// <summary>
	/// User Repository
	/// </summary>
	public interface IUserRepository
	{
		/// <summary>
		/// Get user by enterprise username
		/// </summary>
		/// <param name="enterpriseUserName">enterprise UserName</param>
		/// <returns>User object</returns>
		SO.User GetEnterpriseUser(string enterpriseUserName);

		/// <summary>
		/// Get Starter Profile Options
		/// </summary>
		/// <param name="enterpriseUserName">enterprise UserName</param>
		/// <returns>StarterProfileOptionsResponse object</returns>
		StarterProfileOptionsResponse GetStarterProfileOptions(string enterpriseUserName);

		/// <summary>
		/// Set Starter Profile Options
		/// </summary>
		/// <param name="starterProfileOptions">starterProfile Options object</param>
		/// <returns>SetStarterProfile object</returns>
		SetStarterProfile SetStarterProfileOptions(StarterProfile starterProfileOptions);

		/// <summary>
		/// Update user login
		/// </summary>
		/// <param name="realPageId">enterprise User Id</param>
		/// <param name="organizationPartyId">organizationPartyId</param>
		/// <param name="loginId">loginId</param>
		/// <param name="isActive">isActive</param>
		/// <param name="passwordHash">passwordHash</param>
		/// <param name="passwordSalt">passwordSalt</param>
		/// <param name="isLocked">isLocked</param>
		/// <param name="isTainted">isTainted</param>
		/// <param name="fromDate">fromDate</param>
		/// <param name="thruDate">thruDate</param>
		/// <returns>UserLogin object</returns>
		UserLogin UpdateUserLogin(Guid realPageId, long organizationPartyId, string loginId = null, bool? isActive = null,
			string passwordHash = null, string passwordSalt = null, bool? isLocked = null, bool? isTainted = null, DateTime? fromDate = null, DateTime? thruDate = null);

		/// <summary>
		/// Get enterprise user
		/// </summary>
		/// <param name="realPageId">enterprise User Id</param>
		/// <returns>UserLogin object</returns>
		UserLogin GetEnterpriseUser(Guid realPageId);

		/// <summary>
		/// Create User
		/// </summary>
		/// <param name="newProfile">New Profile object</param>
		/// <param name="persona">Persona list</param>
		/// <returns>CreateUserResponse object with a Error Status object</returns>
		CreateUserResponse<IErrorData> CreateUser(ProfileDetail newProfile, IList<Persona> persona);

		/// <summary>
		/// Update new user profile
		/// </summary>
		/// <param name="userLogin">userLogin</param>
		/// <param name="newProfile">newProfile object</param>
		/// <param name="partyRoleTypeId">party RoleType Id</param>
		/// <param name="companyJobTitle">company Job Title</param>
		/// <param name="activityToken">activity Token</param>
		/// <returns>RepositoryResponse object</returns>
		RepositoryResponse UpdateNewUser(string userLogin, Profile newProfile, int partyRoleTypeId, string companyJobTitle, string activityToken);

		/// <summary>
		/// Update User Detail and Products
		/// </summary>
		/// <param name="loggedInUserRealPageId">Logged-In User unique identifier</param>
		/// <param name="profile">Edited User detail and Products</param>
		/// <param name="oldProfile">Old detail profile from database</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse UpdateUser(Guid loggedInUserRealPageId, IProfileDetail profile, IProfileDetail oldProfile);

		/// <summary>
		/// Used to disable a list of users across all products the users have access to
		/// </summary>
		/// <param name="createUserRealPageId"></param>
		/// <param name="createUserPersonaId"></param>
		/// <param name="userLogins"></param>
		void DisableUserProduct(Guid createUserRealPageId, long createUserPersonaId, IList<UserLoginOnly> userLogins);

		/// <summary>
		/// Used to activate products for the given list of users whom previously disabled
		/// </summary>
		/// <param name="createUserRealPageId"></param>
		/// <param name="createUserPersonaId"></param>
		/// <param name="userLogins"></param>
		void ActivateUserProducts(Guid createUserRealPageId, long createUserPersonaId, IList<UserLoginOnly> userLogins);
		
		/// <summary>
		/// Get User details by Persona Id or user realpage id
		/// </summary>
		UserDetails GetUserDetails(long? personaId = null, string userRealPageId = null);

        /// <summary>
        /// Gets the count of super users for a given organization.
        /// </summary>
        /// <param name="OrganizationPartyId">The unique identifier of the organization party.</param>
        /// <returns>The number of super users in the specified organization.</returns>
        long GetSuperUserCountByOrganizationAsync(long? OrganizationPartyId);

        /// <summary>
        /// Set UnifiedLoginUser in Salesforce
        /// </summary>
        void ActivateSalesForceUser(Guid createUserRealPageId, long createUserPersonaId, IList<UserLoginOnly> userLogins,  bool isAssigned);

        /// <summary>
        /// Used to disable products for the given list of users which is called from windows service
        /// </summary>		
        /// <param name="userLogins"></param>
        void ProcessDisabledUsers(IList<ProcessUserLogin> userLogins);

        /// <summary>
        /// Used to disable products for the given list of users which is called from windows service
        /// </summary>		
        /// <param name="userIds"></param>
        /// <param name="isEnabled"></param>
        RepositoryResponse ThirdPartyIdpBulkUpdate(IList<long> userIds, bool isEnabled);

		/// <summary>
		/// Give administrators access to missing products based on a customer company
		/// </summary>
		/// <param name="organizationRealPageId">Organization enterprise Id</param>
		/// <param name="assignUserPersonaId">Assigned to user PersonaId</param>
		void AssignProductsToAdministrators(Guid organizationRealPageId, long assignUserPersonaId = 0);

		/// <summary>
		/// ProcessDisableUserProductData
		/// </summary>
		/// <param name="repository"></param>
		/// <param name="assignUserPersonaId"></param>
		/// <param name="createUserRealPageId"></param>
		/// <param name="createUserPersonaId"></param>
		/// <param name="impersonatorUserId"></param>
		/// <param name="userTypeId"></param>
		void ProcessDisableUserProductData(IRepository repository, long assignUserPersonaId, Guid createUserRealPageId, long createUserPersonaId, int? userTypeId, long impersonatorUserId);

		/// <summary>
		/// Get the an UserEmployee by UserLoginPersonaId and OrganizationPartyId
		/// </summary>
		/// <param name="UserLoginPersonaId"></param>
		/// <param name="OrganizationPartyId"></param>
		/// <returns>IUserEmployeeId</returns>
		IUserEmployeeId GetUserEmployeeId(long UserLoginPersonaId, long OrganizationPartyId);

		/// <summary>
		/// Update user Employee Id
		/// </summary>
		/// <param name="employeeIdDetail"></param>
		/// <returns></returns>
		RepositoryResponse UpdateUserEmployeeId(IUserEmployeeId employeeIdDetail);

		IList<NavigationMenuEntry> GetNavigationMenu();

		IList<NavigationMenuRightEntry> GetNavigationMenuRights();

		IList<NavigationMenuSetting> GetNavigationMenuSettingsUnaccessable(long partyId);

        AdUserDetail GetAzureUserDetails(long userId);

        IList<EmployeeProductMapping> GetEmployeeProductADGroupMapping(long personaId, int productId);

        RepositoryResponse AddUpdateEmployeeProductADGroupMapping(long personaId, int productId, int adGroupId);

		ExternalUserRelationship GetExternalUserRelationship(long userLoginPersonaId);

        /// <summary>
        /// Insert or update User Status by company
        /// </summary>
        /// <param name="realPageId">User enterprise Id</param>
        /// <param name="organizationPartyId">User enterprise Id</param>
        /// <param name="statusTypeId">statusType Id</param>
        /// <param name="fromDate">FromDate</param>
        /// <param name="thruDate">ThruDate</param>
        /// <returns>RepositoryResponse object</returns>
        RepositoryResponse UpdateUserStatusByCompany(Guid realPageId, long organizationPartyId, int statusTypeId, DateTime fromDate, DateTime? thruDate);

        /// <summary>
        /// Update User Activity Attempts
        /// </summary>
        ActivityAttempt UpdateUserActivityAttempts(string enterpriseUserName, ActivityType activityType, UserDeviceDetails userDeviceDetails, long organizationPartyId, string authenticationServiceId = "");

		/// <summary>
		/// DelegateAdmin Role Template
		/// </summary>
		/// <param name="UserLoginPersonaId"></param>
		/// <returns></returns>
		List<int> GetDelegateAdminRoleTemplate(long UserLoginPersonaId);
		/// <summary>
		/// Get Settings Data
		/// </summary>
		/// <param name="settingName"></param>
		/// <returns></returns>
        bool GetUnifiedSettingData(string settingName);

		UserInfoLite GetSuperVisorInformation(long UserId, long OrganizationPartyId);

		bool CheckOrganizationAdminUser(Guid userRealpageId, long orgPartyId);


		void InsertNewPhoneNumberFromImport(IRepository repository, IProfileDetail profile);
    }
}