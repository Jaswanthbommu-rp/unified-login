using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Foundation.DataAccess.Component;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
	/// <summary>
	/// Credential Repository
	/// </summary>
	public class CredentialRepository : BaseRepository, ICredentialRepository
	{
		#region Ctor
		/// <summary>
		/// Ctor
		/// </summary>
		public CredentialRepository() : base(DbConnectionEnum.IdpConfigurationDb)
		{
		}

        public CredentialRepository(IRepository repository) : base(repository)
        {
        }
        #endregion

        #region ICredentialRepository Implementation
        /// <summary>
        /// Update Enterprise UserCredential
        /// </summary>
        public string UpdateEnterpriseUserCredential(string enterpriseUserName, string newPasswordHash,
			string passwordSalt, string correctAnswerToken, int activityTypeId, long partyId)
		{
			using (var repository = GetRepository())
			{
				return repository.GetOne<string>(StoredProcNameConstants.SP_UpdateEnterpriseUserCredential,
					new { enterpriseUserName, correctAnswerToken, activityTypeId, newPasswordHash, passwordSalt, partyId });
			}
		}

		/// <summary>
		/// Get User Security Question
		/// </summary>
		public IList<SecurityQuestion> GetUserSecurityQuestion(string enterpriseUserName)
		{
			using (var repository = GetRepository())
			{
				return
					repository.GetMany<SecurityQuestion>(StoredProcNameConstants.SP_GetUserSecurityQuestionAnswers,
						new { enterpriseUserName }).ToList();
			}
		}

		/// <summary>
		/// Get User Security Question Answer
		/// </summary>
		public List<SecurityQuestionAnswer> GetUserSecurityQuestionAnswer(string enterpriseUserName)
		{
			using (var repository = GetRepository())
			{
				return
					repository.GetMany<SecurityQuestionAnswer>(
						StoredProcNameConstants.SP_GetUserSecurityQuestionAnswer, new { enterpriseUserName }).ToList();
			}
		}

		/// <summary>
		/// Get Activity Attempt Exceeds
		/// </summary>
		public ActivityAttemptDetails GetActivityAttemptExceeds(long partyId, string enterpriseUserName, int activityTypeId)
		{
			using (var repository = GetRepository())
			{
				return repository.GetOne<ActivityAttemptDetails>(StoredProcNameConstants.SP_GetActivityAttemptExceeds,
					new { enterpriseUserName, activityTypeId,partyId });
			}
		}

		/// <summary>
		/// Get Password History
		/// </summary>
		public IList<PasswordDetail> GetPasswordHistory(string enterpriseUserName, int numberOfPasswordsToRemember)
		{
			using (var repository = GetRepository())
			{
				return
					repository.GetMany<PasswordDetail>(StoredProcNameConstants.SP_GetPasswordHistory,
						new { enterpriseUserName, numberOfPasswordsToRemember }).ToList();
			}
		}

		/// <summary>
		/// Update User Activity Attempts
		/// </summary>
		public ActivityAttempt UpdateUserActivityAttempts(string enterpriseUserName,
			ActivityType activityType,
			UserDeviceDetails userDeviceDetails,
			long partyId,
			string authenticationServiceId = "")
		{
			var activityTypeId = (int)activityType;

			if (userDeviceDetails == null)
			{
				userDeviceDetails = new UserDeviceDetails();
			}

			dynamic param = new
			{
				enterpriseUserName,
				activityTypeId,
				userDeviceDetails.BrowserName,
				userDeviceDetails.BrowserType,
				userDeviceDetails.IpAddress,
				userDeviceDetails.IsMobile,
				userDeviceDetails.Platform,
				userDeviceDetails.Version,
				userDeviceDetails.DeviceType,
				userDeviceDetails.Timezone,				
				authenticationServiceId,
				partyId
			};

			using (var repository = GetRepository())
			{
				var result = repository.GetOne<ActivityAttempt>(StoredProcNameConstants.SP_UpdateActivityAttempt, param);
				return result;
			}
		}

		/// <summary>
		/// Get Activity Token
		/// </summary>
		public TokenDetail GetActivityToken(string enterpriseUserName, string activityToken, int activityTypeId, long partyId)
		{
			using (var repository = GetRepository())
			{
				return repository.GetOne<TokenDetail>(StoredProcNameConstants.SP_GetActivityToken,
					new { enterpriseUserName, activityToken, activityTypeId, partyId });
			}
		}

		/// <summary>
		/// Create Activity Token
		/// </summary>
		public string CreateActivityToken(long partyId,Guid realPageId, int activityTypeId)
		{
			using (var repository = GetRepository())
			{
				return repository.GetOne<string>(StoredProcNameConstants.SP_CreateActivityToken,
					new {partyId, realPageId, activityTypeId });
			}
		}

		/// <summary>
		/// Reset Enterprise User Credential
		/// </summary>
		public RepositoryResponse ResetEnterpriseUserCredential(Guid realPageId, string newPasswordHash, string newPasswordSalt, long partyId)
		{
			using (var repository = GetRepository())
			{
				return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_ResetEnterpriseUserCredential,
					new { realPageId, newPasswordHash, newPasswordSalt, partyId });
			}
		}

		/// <summary>
		/// Reset Enterprise User Status
		/// </summary>
		//public RepositoryResponse UpdateUserStatus(Guid realPageId, UserUiStatusType statusType, DateTime? fromDate, DateTime? thruDate)
		//{
		//	using (var repository = GetRepository())
		//	{
		//		RepositoryResponse updateUserStatusResponse = repository.Execute<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserStatus,
		//			new
		//			{
		//				RealPageId = realPageId,
		//				StatusTypeId = statusType,
		//				FromDate = fromDate,
		//				ThruDate = thruDate
		//			});
		//		return updateUserStatusResponse;
		//	}
		//}

        /// <summary>
        /// Reset Enterprise User Status
        /// </summary>
        public RepositoryResponse UpdateUserStatusByCompany(Guid realPageId, long organizationPartyId, UserUiStatusType statusType, DateTime? fromDate, DateTime? thruDate)
        {
            using (var repository = GetRepository())
            {
                RepositoryResponse updateUserStatusResponse = repository.Execute<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserStatusByCompany,
                    new
                    {
                        RealPageId = realPageId,
                        OrganizationPartyId = organizationPartyId,
                        StatusTypeId = statusType,
                        FromDate = fromDate,
                        StatusThruDate = thruDate
                    });
                return updateUserStatusResponse;
            }
        }

        /// <summary>
        /// Reset Enterprise User Credential
        /// </summary>
        public RepositoryResponse SetEnterpriseUserTemporaryPassword(Guid realPageId, long organizationPartyId, string newPasswordHash, string newPasswordSalt, UserLoginOnly user, OrganizationStatus organizationStatus)
		{
			RepositoryResponse repositoryResponse = new RepositoryResponse();
			using (var repository = GetRepository())
			{
				//Update UserLogin with pending state and temporary password
				dynamic paramUpdateNewUserLogin = new
				{
					RealPageId = realPageId,
					PasswordHash = newPasswordHash,
					PasswordSalt = newPasswordSalt,
					FromDate = organizationStatus.FromDate,
					ThruDate = organizationStatus.ThruDate,
					PartyId = organizationStatus.PartyId
                };
                // TODO check the logic in this proc
				RepositoryResponse updateUserLoginResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserLogin, paramUpdateNewUserLogin);

				if (updateUserLoginResponse.Id == 0)
				{
					repositoryResponse.ErrorMessage = "Update User Error: Update user login detail failed.";
					repositoryResponse.Id = 0;
					return repositoryResponse;
				}
				else
				{
					repositoryResponse.Id = updateUserLoginResponse.Id;
				}

                // we need to see if the user is in expired pending state. If it is then we need to extend the pending status as of the change password date, otherwise save the force reset password status
                DateTime statusThruDate = DateTime.UtcNow.AddHours(72); // fromDate.Value.AddHours(72); //default

                if (organizationStatus.StatusTypeId == (int)UserUiStatusType.Expired || organizationStatus.StatusTypeId == (int)UserUiStatusType.Pending)
				{
                    var activityDetail = repository.GetMany<Activity>(StoredProcNameConstants.SP_ListActivity, new { PartyId = organizationPartyId }).ToList();
                    var newUserRegistrationActivity = activityDetail.FirstOrDefault(x => x.ActivityTypeId == (int)ActivityType.NewUserRegistration);
                    statusThruDate = newUserRegistrationActivity != null ? DateTime.UtcNow.AddMinutes(newUserRegistrationActivity.ActivityTokenExpirationMinutes) : statusThruDate;
                    // the user has an expired/pending status so extend the period as of the change password
                    repositoryResponse = UpdateStatus(realPageId, organizationPartyId, UserUiStatusType.Pending, organizationStatus.FromDate, statusThruDate, repositoryResponse);
					if (repositoryResponse.Id == 0)
					{
						return repositoryResponse;
					}
				}
				else if (organizationStatus.StatusTypeId == (int)UserUiStatusType.Active || organizationStatus.StatusTypeId == (int)UserUiStatusType.Locked)
				{
					// if the user isn't pending and not expired then force a reset 			
					statusThruDate = DateTime.MaxValue.ToUniversalTime();
					repositoryResponse = UpdateStatus(realPageId, organizationPartyId, UserUiStatusType.ForceResetPassword, organizationStatus.FromDate, statusThruDate, repositoryResponse);
					if (repositoryResponse.Id == 0)
					{
						return repositoryResponse;
					}
				}
						
			}		

			return repositoryResponse;
		}

		/// <summary>
		/// Get All Security Questions
		/// </summary>
		public IList<SecurityQuestion> GetAllSecurityQuestions(string enterpriseUserName)
		{
			using (var repository = GetRepository())
			{
				return
					repository.GetMany<SecurityQuestion>(StoredProcNameConstants.SP_GetAllSecurityQuestions,
						new { enterpriseUserName }).ToList();
			}
		}

		/// <summary>
		/// Save Security Question Answers
		/// </summary> 
		public long SaveSecurityQuestionAnswers(UserSecurityAnswer userSecurityQuestionsAnswers, long organizationPartyId)
		{
			var activityTypeId = (int)ActivityType.NewUserRegistrationVerification;

			using (var repository = GetRepository())
			{
				var result =
					repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_SaveSecurityQuestionAnswers, new
					{
						userSecurityQuestionsAnswers.EnterpriseUserName,
						userSecurityQuestionsAnswers.ActivityToken,
						activityTypeId,

						securityQuestion1Id = userSecurityQuestionsAnswers.SecurityQuestionAnswers[0].SecurityQuestionId,
						securityAnswer1 = userSecurityQuestionsAnswers.SecurityQuestionAnswers[0].Answer,

						securityQuestion2Id = userSecurityQuestionsAnswers.SecurityQuestionAnswers[1].SecurityQuestionId,
						securityAnswer2 = userSecurityQuestionsAnswers.SecurityQuestionAnswers[1].Answer,

						securityQuestion3Id = userSecurityQuestionsAnswers.SecurityQuestionAnswers[2].SecurityQuestionId,
						securityAnswer3 = userSecurityQuestionsAnswers.SecurityQuestionAnswers[2].Answer,
						partyId = organizationPartyId
					});

				if (result.Id <= 0)
					throw new Exception(result.ErrorMessage);

				return result.Id;
			}
		}

		/// <summary>
		/// List Organization By RealPageId
		/// </summary>
		/// <param name="realPageId">Enterprise User RealPage Id</param>
		/// <returns>Organization list</returns>
		public IList<Organization> ListOrganizationByRealPageId(Guid realPageId)
		{
			var relationshipTypeName = "User Type";
			using (var repository = GetRepository())
			{
				IList<Organization> result = repository.GetMany<Organization>(StoredProcNameConstants.SP_ListOrganizationByRealPageId, new { realPageId, relationshipTypeName }).ToList();

				return result;
			}
		}

		/// <summary>
		/// Get the Identity Provider Type by User login name
		/// </summary>
		/// <param name="enterpriseUserName">User login Name.</param>
		/// <returns>Identity Provider Type object</returns>
		public IdentityProviderType GetIdentityProviderTypeByLoginName(string enterpriseUserName)
		{
			using (var repository = GetRepository())
			{
				var result = repository.GetOne<string>(StoredProcNameConstants.SP_GetIdentityProviderTypeByLoginName, new { loginName = enterpriseUserName });
				var authType = new IdentityProviderType { AuthenticationType = result };
				return authType;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public IList<SecurityQuestion> GetUserSelectedSecurityQuestions(Guid realpageId)
		{
			using (var repository = GetRepository())
			{
				return repository.GetMany<SecurityQuestion>(StoredProcNameConstants.SP_GetUserSelectedSecurityQuestions, new { realpageId }).ToList();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public long SaveUserSelectedSecurityQuestions(Guid realpageId, IList<SecurityQuestionAnswer> securityQuestionAnswers)
		{
			using (var repository = GetRepository())
			{
				return repository.Execute<long>(StoredProcNameConstants.SP_CreateUserSelectedSecurityQuestions, new
				{
					realpageId,

					questionId1 = securityQuestionAnswers[0].SecurityQuestionId,
					answer1 = securityQuestionAnswers[0].Answer,

					questionId2 = securityQuestionAnswers[1].SecurityQuestionId,
					answer2 = securityQuestionAnswers[1].Answer,

					questionId3 = securityQuestionAnswers[2].SecurityQuestionId,
					answer3 = securityQuestionAnswers[2].Answer,
				});
			}
		}

		/// <summary>
		/// Get User Activity lookup
		/// </summary>
		/// <returns></returns>
		public IList<Activity> GetActivities(long partyId)
		{
			using (var repository = GetRepository())
			{
				return repository.GetMany<Activity>(StoredProcNameConstants.SP_ListActivity, new { partyId }).ToList();
			}
		}
		#endregion

		#region Privates
		/// <summary>
		/// Used to update a status for a user
		/// </summary>
		/// <param name="realPageId"></param>
		/// <param name="organizationPartyId"></param>
		/// <param name="statusType"></param>
		/// <param name="user"></param>
		/// <param name="statusThruDate"></param>
		/// <param name="repositoryResponse"></param>
		/// <returns></returns>
		private RepositoryResponse UpdateStatus(Guid realPageId, long organizationPartyId, UserUiStatusType statusType, DateTime fromDate, DateTime statusThruDate, RepositoryResponse repositoryResponse)
		{
			RepositoryResponse updateUserStatusResponse = UpdateUserStatusByCompany(realPageId, organizationPartyId, statusType, fromDate, statusThruDate);
			if (updateUserStatusResponse.Id == 0)
			{
				repositoryResponse.ErrorMessage = "Update User Error: Update user status failed.";
				repositoryResponse.Id = 0;
			}
			else
			{
				repositoryResponse.Id = updateUserStatusResponse.Id;
			}
			return repositoryResponse;
		}
		
		#endregion
	}
}