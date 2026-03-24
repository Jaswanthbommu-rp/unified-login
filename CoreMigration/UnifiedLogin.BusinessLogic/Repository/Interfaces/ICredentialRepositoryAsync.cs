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
	public interface ICredentialRepositoryAsync
	{
		/// <summary>
		/// Update Enterprise User Credential
		/// </summary>
		Task<string> UpdateEnterpriseUserCredentialAsync(string enterpriseUserName, string newPasswordHash, string passwordSalt,
			string correctAnswerToken, int activityId, long organizationPartyId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Get User Security Question
		/// </summary>
		Task<IList<SecurityQuestion>> GetUserSecurityQuestionAsync(string enterpriseUserName, CancellationToken cancellationToken = default);

		/// <summary>
		/// Get User Security Question Answer
		/// </summary>
		Task<IList<SecurityQuestionAnswer>> GetUserSecurityQuestionAnswerAsync(string enterpriseUserName, CancellationToken cancellationToken = default);

		/// <summary>
		/// Get Activity Attempt Exceeds
		/// </summary>
		Task<ActivityAttemptDetails> GetActivityAttemptExceedsAsync(long organizationPartyId, string enterpriseUserName, int activityId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update User Activity Attempts
        /// </summary>
        Task<ActivityAttempt> UpdateUserActivityAttemptsAsync(string enterpriseUserName, ActivityType activityType, UserDeviceDetails userDeviceDetails, long organizationPartyId, string authenticationServiceId = "", CancellationToken cancellationToken = default);

		/// <summary>
		/// Get Activity Token
		/// </summary>
		Task<TokenDetail> GetActivityTokenAsync(string enterpriseUserName, string activityToken, int activityId, long organizationPartyId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Create Activity Token
		/// </summary>
		Task<string> CreateActivityTokenAsync(long organizationPartyId, Guid realPageId, int activityId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Get Password History
		/// </summary>
		Task<IList<PasswordDetail>> GetPasswordHistoryAsync(string enterpriseUserName, int numberOfPasswordsToRemember, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reset Enterprise User Credential
        /// </summary>
        Task<RepositoryResponse> ResetEnterpriseUserCredentialAsync(Guid realPageId, string newPasswordHash, string newPasswordSalt, long organizationPartyId, CancellationToken cancellationToken = default);
		/// <summary>
		/// Reset Enterprise User Status
		/// </summary>
		//Task<RepositoryResponse> UpdateUserStatusAsync(Guid realPageId, UserUiStatusType statusType, DateTime? fromDate, DateTime? thruDate);
        /// <summary>
        /// Reset Enterprise User Status
        /// </summary>
        Task<RepositoryResponse> UpdateUserStatusByCompanyAsync(Guid realPageId, long organizationPartyId, UserUiStatusType statusType, DateTime? fromDate, DateTime? thruDate, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reset Enterprise User Credential
        /// </summary>
        Task<RepositoryResponse> SetEnterpriseUserTemporaryPasswordAsync(Guid realPageId, long organizationPartyId, string newPasswordHash, string newPasswordSalt, UserLoginOnly user, OrganizationStatus organizationStatus, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get All Security Questions
        /// </summary>
        Task<IList<SecurityQuestion>> GetAllSecurityQuestionsAsync(string enterpriseUserName, CancellationToken cancellationToken = default);
		/// <summary>
		/// Save Security Question Answers
		/// </summary> 
		Task<long> SaveSecurityQuestionAnswersAsync(UserSecurityAnswer userSecurityQuestionsAnswers, long organizationPartyId, CancellationToken cancellationToken = default);
		/// <summary>
		/// list of Organization RealPage Id
		/// </summary>
		/// <param name="userRealPageId">User unique identifier</param>
		/// <returns>A list of Organization(s) Details for a person</returns>
		Task<IList<Organization>> ListOrganizationByRealPageIdAsync(Guid userRealPageId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Get the Identity Provider Type by User login name
		/// </summary>
		/// <param name="enterpriseUserName">User login Name.</param>
		/// <returns>Identity Provider Type object</returns>
		Task<IdentityProviderType> GetIdentityProviderTypeByLoginNameAsync(string enterpriseUserName, CancellationToken cancellationToken = default);

		/// <summary>
		/// 
		/// </summary>
		Task<IList<SecurityQuestion>> GetUserSelectedSecurityQuestionsAsync(Guid realpageUserId, CancellationToken cancellationToken = default);

		/// <summary>
		/// 
		/// </summary> 
		Task<long> SaveUserSelectedSecurityQuestionsAsync(Guid realpageId, IList<SecurityQuestionAnswer> securityQuestionAnswer, CancellationToken cancellationToken = default);

		/// <summary>
		/// Get User Activity lookup
		/// </summary>
		////// <param name="organizationPartyId">organization PartyId.</param>
		/// <returns>Activities</returns>
		Task<IList<Activity>> GetActivitiesAsync(long organizationPartyId, CancellationToken cancellationToken = default);

	}

}