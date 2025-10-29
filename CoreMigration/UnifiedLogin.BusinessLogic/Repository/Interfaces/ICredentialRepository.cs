using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// 
	/// </summary>
	public interface ICredentialRepository
	{
		/// <summary>
		/// Update Enterprise User Credential
		/// </summary>
		string UpdateEnterpriseUserCredential(string enterpriseUserName, string newPasswordHash, string passwordSalt,
			string correctAnswerToken, int activityId, long organizationPartyId);

		/// <summary>
		/// Get User Security Question
		/// </summary>
		IList<SecurityQuestion> GetUserSecurityQuestion(string enterpriseUserName);

		/// <summary>
		/// Get User Security Question Answer
		/// </summary>
		List<SecurityQuestionAnswer> GetUserSecurityQuestionAnswer(string enterpriseUserName);

		/// <summary>
		/// Get Activity Attempt Exceeds
		/// </summary>
		ActivityAttemptDetails GetActivityAttemptExceeds(long organizationPartyId, string enterpriseUserName, int activityId);

        /// <summary>
        /// Update User Activity Attempts
        /// </summary>
        ActivityAttempt UpdateUserActivityAttempts(string enterpriseUserName, ActivityType activityType, UserDeviceDetails userDeviceDetails, long organizationPartyId, string authenticationServiceId = "");

		/// <summary>
		/// Get Activity Token
		/// </summary>
		TokenDetail GetActivityToken(string enterpriseUserName, string activityToken, int activityId, long organizationPartyId);

		/// <summary>
		/// Create Activity Token
		/// </summary>
		string CreateActivityToken(long organizationPartyId, Guid realPageId, int activityId);

		/// <summary>
		/// Get Password History
		/// </summary>
		IList<PasswordDetail> GetPasswordHistory(string enterpriseUserName, int numberOfPasswordsToRemember);

        /// <summary>
        /// Reset Enterprise User Credential
        /// </summary>
        RepositoryResponse ResetEnterpriseUserCredential(Guid realPageId, string newPasswordHash, string newPasswordSalt, long organizationPartyId);
		/// <summary>
		/// Reset Enterprise User Status
		/// </summary>
		//RepositoryResponse UpdateUserStatus(Guid realPageId, UserUiStatusType statusType, DateTime? fromDate, DateTime? thruDate);

        /// <summary>
        /// Reset Enterprise User Status
        /// </summary>
        RepositoryResponse UpdateUserStatusByCompany(Guid realPageId, long organizationPartyId, UserUiStatusType statusType, DateTime? fromDate, DateTime? thruDate);

        /// <summary>
        /// Reset Enterprise User Credential
        /// </summary>
        RepositoryResponse SetEnterpriseUserTemporaryPassword(Guid realPageId, long organizationPartyId, string newPasswordHash, string newPasswordSalt, UserLoginOnly user, OrganizationStatus organizationStatus);
        /// <summary>
        /// Get All Security Questions
        /// </summary>
        IList<SecurityQuestion> GetAllSecurityQuestions(string enterpriseUserName);

		/// <summary>
		/// Save Security Question Answers
		/// </summary> 
		long SaveSecurityQuestionAnswers(UserSecurityAnswer userSecurityQuestionsAnswers, long organizationPartyId);

		/// <summary>
		/// list of Organization RealPage Id
		/// </summary>
		/// <param name="userRealPageId">User unique identifier</param>
		/// <returns>A list of Organization(s) Details for a person</returns>
		IList<Organization> ListOrganizationByRealPageId(Guid userRealPageId);

		/// <summary>
		/// Get the Identity Provider Type by User login name
		/// </summary>
		/// <param name="enterpriseUserName">User login Name.</param>
		/// <returns>Identity Provider Type object</returns>
		IdentityProviderType GetIdentityProviderTypeByLoginName(string enterpriseUserName);

		/// <summary>
		/// 
		/// </summary>
		IList<SecurityQuestion> GetUserSelectedSecurityQuestions(Guid realpageUserId);

		/// <summary>
		/// 
		/// </summary> 
		long SaveUserSelectedSecurityQuestions(Guid realpageId, IList<SecurityQuestionAnswer> securityQuestionAnswer);

		/// <summary>
		/// Get User Activity lookup
		/// </summary>
		////// <param name="organizationPartyId">organization PartyId.</param>
		/// <returns>Activities</returns>
		IList<Activity> GetActivities(long organizationPartyId);

	}

}