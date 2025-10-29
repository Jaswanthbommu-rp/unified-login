using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using Serilog;
using Serilog.Events;

namespace UnifiedLogin.BusinessLogic.Logic
{  /// <summary>
   /// 
   /// </summary>
	public class ManageCredential : IManageCredential
	{
        #region Private Variables

        ICredentialRepository _credentialRepository;
        IPasswordPolicyRepository _passwordPolicyRepository;
        //IUserRepository _userRepository;
        private IUserLoginRepository _userLoginRepository;
        IManageUserLogin _manageUserLogin;
        private DefaultUserClaim _userClaim;
        private IManagePerson _managePerson;
		private IManageOrganization _manageOrganization;
		private IUserRepository _userRepository;
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor with injection - for Unit Test  
        /// </summary>
        public ManageCredential(ICredentialRepository credentialRepository, IPasswordPolicyRepository passwordPolicyRepository, IUserLoginRepository userLoginRepository, IManageUserLogin manageUserLogin, IManagePerson managePerson, IUserRepository userRepository, DefaultUserClaim userClaim)
		{
            _credentialRepository = credentialRepository;
            _passwordPolicyRepository = passwordPolicyRepository;
            _userLoginRepository = userLoginRepository;
            _manageUserLogin = manageUserLogin;
            _managePerson = managePerson;
			_userClaim = userClaim;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Default Ctor
        /// </summary>
        public ManageCredential(DefaultUserClaim userClaim)
		{
            _credentialRepository = new CredentialRepository();
            _passwordPolicyRepository = new PasswordPolicyRepository();
            _userLoginRepository = new UserLoginRepository();
            _manageUserLogin = new ManageUserLogin(userClaim);
            _managePerson = new ManagePerson();
			_manageOrganization = new ManageOrganization(userClaim);
			_userRepository = new UserRepository(userClaim);
            _userRepository = new UserRepository(userClaim);
			_userClaim = userClaim;
		}

		#endregion

		#region Forgot Password

		/// <summary>
		/// Get Security Question
		/// </summary>
		/// <param name="enterpriseUserName">UserName</param>
		/// <param name="userDeviceDetails">user Device Details</param>
		/// <returns>SecurityQuestion Response</returns>
		public SecurityQuestionResponse GetSecurityQuestion(string enterpriseUserName, UserDeviceDetails userDeviceDetails)
        {
            var response = new SecurityQuestionResponse();

            if (string.IsNullOrEmpty(enterpriseUserName))
            {
                response.IsError = true;
                response.ErrorReason = "No Username specified.";
                return response;
            }

            //_credentialRepository.UpdateUserActivityAttempts(enterpriseUserName, ActivityType.ForgotPassword, userDeviceDetails, null);

            //var user = _userLoginRepository.GetUserLogin(enterpriseUserName);
			//long orgPartyId = 0;

			response.IsUserExist = true;

	        var userLogin = _manageUserLogin.GetUserLoginOnly(enterpriseUserName);
            if (userLogin == null)
            {
                response.IsError = true;
                response.ErrorReason = $"The Username \"{enterpriseUserName}\" is incorrect or was not found.";
                return response; // user not exists
            }

            //orgPartyId = _userLoginRepository.GetPrimaryOrgIdByUserId(userLogin.UserId);
            OrganizationStatus primaryOrgStatus = _userLoginRepository.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, 0, true);

            response.IsUserPending = (bool)primaryOrgStatus.IsPending;

	        var userLoginIsActive = primaryOrgStatus.IsActive;
            response.IsUserActive = userLoginIsActive != null && (bool)userLoginIsActive;
	        var userLoginIsExpired = primaryOrgStatus.IsExpired;
            response.IsUserExpired = userLoginIsExpired != null && (bool)userLoginIsExpired;
	        var userLoginIsPending = primaryOrgStatus.IsPending;
            response.IsUserPending = userLoginIsPending != null && (bool)userLoginIsPending;

            if (response.IsUserActive == false || response.IsUserExpired == true || response.IsUserPending == true)
            {
                response.IsError = true;
                response.ErrorReason = "The user is not active in the system.";
                return response;
            }

	       	if (primaryOrgStatus.StatusTypeId == (int)UserUiStatusType.Locked && primaryOrgStatus.StatusThruDate != null && primaryOrgStatus.StatusThruDate.Value < DateTime.UtcNow)
			{
				primaryOrgStatus.IsLocked = false;
                // reset attempt count
                // update user status back to active once lockout time ended
                _userRepository.UpdateUserStatusByCompany(userLogin.RealPageId, primaryOrgStatus.PartyId, (int)UserDbStatusType.Active, DateTime.UtcNow, null);
                _userRepository.UpdateUserActivityAttempts(userLogin.LoginName, ActivityType.LoginSuccess, userDeviceDetails, primaryOrgStatus.PartyId, null);
			}
			response.IsUserLocked = (bool)primaryOrgStatus.IsLocked;
			if (response.IsUserLocked == true)
            {
                response.IsError = true;
                response.ErrorReason = "The user account is locked.";
                return response;
            }

            //Post MVP
            //response.IsUserTainted = (bool)profileDetailList[0].userLogin.IsTainted; 
            //if (response.IsUserTainted == true)
            //{
            //    response.IsError = true;
            //    response.ErrorReason = "The user account is blocked."; 
            //    return response;
            //}

            // get user's identity provider
            
            //IdentityProviderType identityProviderType = _credentialRepository.GetIdentityProviderTypeByLoginName(user.LoginName);
            //
            //if (identityProviderType == null || identityProviderType.AuthenticationType == null)
            //{
            //    response.IsError = true;
            //    response.ErrorReason = "Identity Provider is not assigned to user.";
            //    return response;
            //}

            //if (!identityProviderType.AuthenticationType.Equals("idp", StringComparison.OrdinalIgnoreCase) && !identityProviderType.AuthenticationType.Equals("local", StringComparison.OrdinalIgnoreCase))
            if(userLogin.Is3rdPartyIDP)
            {
                response.IsError = true;
                response.ErrorReason = "Forgot password is not applicable to users on external identity provider.";
                return response;
            }

            // User is safe, exists & not locked get questions
            var securityQuestions = _credentialRepository.GetUserSecurityQuestion(enterpriseUserName);

            if (securityQuestions == null || securityQuestions.Count <= 0)
            {
                response.IsError = true;
                response.ErrorReason = "User has no security questions defined.";
                return response;
            }

            //Get random questions
            response.SecurityQuestions = SelectRandomQuestions(securityQuestions);

            // Create Token - also check if any other token already exists & deactivate it
            response.ActivityToken = _credentialRepository.CreateActivityToken(primaryOrgStatus.PartyId, userLogin.RealPageId, (int)ActivityType.ForgotPassword);
            response.EnterpriseUserName = enterpriseUserName;

            if (string.IsNullOrEmpty(response.ActivityToken))
                throw new Exception("Unable to get token."); // TODO; research on this

            return response;
        }

		/// <summary>
		/// Verify Security Answers
		/// </summary>
		/// <param name="userSecurityAnswer">userSecurityAnswer</param>
		/// <param name="userDeviceDetails">userDeviceDetails</param>
		/// <returns>Security Answer Response</returns>
		public SecurityAnswerResponse VerifySecurityAnswers(UserSecurityAnswer userSecurityAnswer, UserDeviceDetails userDeviceDetails)
        {
            var securityAnswerResponse = new SecurityAnswerResponse();
            int activityId = (int)ActivityType.ForgotPassword;
            var questionAttemptActivityId = (int)ActivityType.QuestionAttempts;

            if (string.IsNullOrEmpty(userSecurityAnswer.EnterpriseUserName))
            {
                securityAnswerResponse.IsError = true;
                securityAnswerResponse.ErrorReason = "No Username specified.";
                return securityAnswerResponse;
            }

            // check if SecurityQuestionAnswers received from user
            if (userSecurityAnswer.SecurityQuestionAnswers == null || userSecurityAnswer.SecurityQuestionAnswers.Count <= 0)
            {
                securityAnswerResponse.IsError = true;
                securityAnswerResponse.ErrorReason = "No questions received from user.";
                securityAnswerResponse.IsAnswersCorrect = false;
                return securityAnswerResponse;
            }

            // check if ActivityToken is supplied
            if (string.IsNullOrEmpty(userSecurityAnswer.ActivityToken))
            {
                securityAnswerResponse.IsError = true;
                securityAnswerResponse.ErrorReason = "Null or empty security Forgot Password Activity Token.";
                securityAnswerResponse.IsAnswersCorrect = false;
                return securityAnswerResponse;
            }

            // check if user exists
            var userLogin = _userLoginRepository.GetUserLoginOnly(userSecurityAnswer.EnterpriseUserName);
			long orgPartyId = 0;
			if (userLogin == null)
            {
                securityAnswerResponse.IsError = true;
                securityAnswerResponse.ErrorReason = "User Name is incorrect or not found.";
                return securityAnswerResponse; // user not exists
            }
			else
			{
                orgPartyId = _userLoginRepository.GetPrimaryOrgIdByUserId(userLogin.UserId);
            }

			//var userCurrentStatueses = GetUserCurrentStatus(userLogin.RealPageId);
            OrganizationStatus primaryOrgStatus = _userLoginRepository.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, 0, true);

			if (primaryOrgStatus.IsLocked.Value)
            {
                securityAnswerResponse.IsError = true;
                securityAnswerResponse.ErrorReason = "Your account is locked";
                securityAnswerResponse.IsAnswersCorrect = false;
                return securityAnswerResponse;
            }

            // check Activity Attempts < 3
            var activityAttemptsExceeds = _credentialRepository.GetActivityAttemptExceeds(orgPartyId, userSecurityAnswer.EnterpriseUserName, (int)ActivityType.QuestionAttempts);
            if (activityAttemptsExceeds.AttemptCount >= activityAttemptsExceeds.MaxActivitycount)
            {
                // apply lock on user status
                _userRepository.UpdateUserStatusByCompany(userLogin.RealPageId, primaryOrgStatus.PartyId, (int)UserDbStatusType.Locked,
                    DateTime.UtcNow, DateTime.UtcNow.AddMinutes(activityAttemptsExceeds.ActivityTokenExpirationMinutes));

                securityAnswerResponse.IsError = true;
                securityAnswerResponse.ErrorReason = "Max attempts to answer security questions exceeded. Your account is locked";
                securityAnswerResponse.IsAnswersCorrect = false;
                return securityAnswerResponse;
            }

            // check token is matching & not expired
            var tokenResult = _credentialRepository.GetActivityToken(userSecurityAnswer.EnterpriseUserName, userSecurityAnswer.ActivityToken, activityId, orgPartyId);
            if (tokenResult == null || string.IsNullOrEmpty(tokenResult.Token))
            {
                securityAnswerResponse.IsError = true;
                securityAnswerResponse.ErrorReason = "Forgot Password Activity Token is expired.";
                securityAnswerResponse.IsAnswersCorrect = false;
                return securityAnswerResponse;
            }

            // get security question answers from database            
            var savedAnswers = _credentialRepository.GetUserSecurityQuestionAnswer(userSecurityAnswer.EnterpriseUserName);
            if (savedAnswers == null || savedAnswers.Count <= 0)
            {
                securityAnswerResponse.IsError = true;
                securityAnswerResponse.ErrorReason = "User has no security questions defined.";
                securityAnswerResponse.IsAnswersCorrect = false;
                return securityAnswerResponse;
            }

            // Get hash of answers
            userSecurityAnswer.SecurityQuestionAnswers = GetHashedAnswers(userSecurityAnswer.SecurityQuestionAnswers);
			// Update activity attempts		
			_credentialRepository.UpdateUserActivityAttempts(userSecurityAnswer.EnterpriseUserName, ActivityType.QuestionAttempts, userDeviceDetails, orgPartyId, null);

			// check if user answers matches with saved answers
			foreach (var userQa in userSecurityAnswer.SecurityQuestionAnswers)
            {
                var match = savedAnswers.Find(x => x.SecurityQuestionId == userQa.SecurityQuestionId);
                if (match == null || match.Answer != userQa.Answer)
                {
                    // Check attempts made to check security answers
                    // if more then return error else then another set of Q
                    var ativityAttemptDetails = _credentialRepository.GetActivityAttemptExceeds(orgPartyId, userSecurityAnswer.EnterpriseUserName, questionAttemptActivityId);

                    if (ativityAttemptDetails.AttemptCount <= ativityAttemptDetails.MaxActivitycount - 1)
                    {
                        securityAnswerResponse.IsError = true;
                        securityAnswerResponse.ErrorReason = "One or more of your answers are incorrect. Please try again with a new set of questions.";
                        securityAnswerResponse.IsAnswersCorrect = false;

                        // get all questions
                        var securityQuestions = _credentialRepository.GetUserSecurityQuestion(userSecurityAnswer.EnterpriseUserName);

                        // select random questions
                        securityAnswerResponse.SecurityQuestions = SelectRandomQuestions(securityQuestions);

                        return securityAnswerResponse;
                    }

                    // apply lock on user status
                    _userRepository.UpdateUserStatusByCompany(userLogin.RealPageId, orgPartyId, (int) UserDbStatusType.Locked,
                        DateTime.UtcNow, DateTime.UtcNow.AddMinutes(activityAttemptsExceeds.ActivityTokenExpirationMinutes));
                    securityAnswerResponse.IsError = true;
                    securityAnswerResponse.ErrorReason = "Max attempts to answer security questions exceeded.Your account is locked."; //"One or more of your answers are incorrect. Please try again with a new set of questions.";
                    securityAnswerResponse.IsAnswersCorrect = false;
                    return securityAnswerResponse;
                }
            }

            // all good
            securityAnswerResponse.IsError = false;
            securityAnswerResponse.ErrorReason = string.Empty;
            securityAnswerResponse.IsAnswersCorrect = true;
            securityAnswerResponse.EnterpriseUserName = userSecurityAnswer.EnterpriseUserName;

            // Create & get token for Verify security answers
            securityAnswerResponse.CorrectAnswerToken = _credentialRepository.CreateActivityToken(orgPartyId, userLogin.RealPageId, (int)ActivityType.VerifyAnswers);

            return securityAnswerResponse;
        }

		/// <summary>
		/// ForgotPassword
		/// </summary>
		/// <param name="changePassword">changePassword</param>
		/// <returns>Change Password Response</returns>
        public ChangePasswordResponse ForgotPassword(ChangePassword changePassword)
        {
            var changePasswordResponse = new ChangePasswordResponse();

            if (string.IsNullOrEmpty(changePassword.EnterpriseUserName))
            {
                SetErrorOnchangePasswordResponse(true, false, "No Username specified.", changePasswordResponse);
                return changePasswordResponse;
            }

            if (string.IsNullOrEmpty(changePassword.ActivityToken))
            {
                SetErrorOnchangePasswordResponse(true, false, "Forgot Password Activity Token is not specified.", changePasswordResponse);
                return changePasswordResponse;
            }

            if (string.IsNullOrEmpty(changePassword.CorrectAnswerToken))
            {
                SetErrorOnchangePasswordResponse(true, false, "Correct Answer Token is not specified.", changePasswordResponse);
                return changePasswordResponse;
            }

            if (string.IsNullOrEmpty(changePassword.NewPassword))
            {
                SetErrorOnchangePasswordResponse(true, false, "New Password is not specified.", changePasswordResponse);
                return changePasswordResponse;
            }

            // check if user exists
            var user = _userLoginRepository.GetUserLoginOnly(changePassword.EnterpriseUserName);
			if (user == null)
            {
                SetErrorOnchangePasswordResponse(true, false, "User name is incorrect or not found.", changePasswordResponse);
                return changePasswordResponse;
            }

            // Get user's org id
            long orgPartyId = _userLoginRepository.GetPrimaryOrgIdByUserId(user.UserId);

            // Validate Password Criteria
            var respose = ValidatePassword(new ValidatePassword
            {
                EnterpriseUserName = changePassword.EnterpriseUserName,
                PasswordToValidate = changePassword.NewPassword,
                PartyId = orgPartyId,
                PasswordModifiedDate = user.PasswordModifiedDate,
                PasswordHash = user.PasswordHash,
                PasswordSalt = user.PasswordSalt,
                CheckPasswordHistory = true
            });

            if (respose.IsError)
            {
                SetErrorOnchangePasswordResponse(true, false, respose.ErrorReason, changePasswordResponse);
                return changePasswordResponse;
            }

            var newPasswordHashDetail = changePassword.NewPassword.PasswordHash();

            var tokenResult = _credentialRepository.GetActivityToken(changePassword.EnterpriseUserName, changePassword.ActivityToken, (int)ActivityType.ForgotPassword, orgPartyId);
            if (tokenResult == null || string.IsNullOrEmpty(tokenResult.Token))
            {
                SetErrorOnchangePasswordResponse(true, false, "Forgot Password Activity Token is expired.", changePasswordResponse);
                return changePasswordResponse;
            }

            var correctAnswerTokenResult = _credentialRepository.GetActivityToken(changePassword.EnterpriseUserName, changePassword.CorrectAnswerToken, (int)ActivityType.VerifyAnswers, orgPartyId);
            if (correctAnswerTokenResult == null || string.IsNullOrEmpty(correctAnswerTokenResult.Token))
            {
                SetErrorOnchangePasswordResponse(true, false, "Correct Answer Token is expired.", changePasswordResponse);
                return changePasswordResponse;
            }

            string enterpriseUserId = _credentialRepository.UpdateEnterpriseUserCredential(changePassword.EnterpriseUserName, newPasswordHashDetail.PasswordHash, newPasswordHashDetail.PasswordSalt, changePassword.CorrectAnswerToken, (int)ActivityType.VerifyAnswers, orgPartyId);

            // for activity logging
            var person = _managePerson.GetPerson(user.RealPageId);
            var booksMasterOrgId = GetDefaultBooksMasterOrgIdForUser(user.RealPageId);
            if (string.IsNullOrEmpty(enterpriseUserId))
            {
                SetErrorOnchangePasswordResponse(true, false, "Error while updating password.", changePasswordResponse);

                // activity logging
                try
                {
                    LogActivity.WriteActivity(new ActivityDetails
                    {
                        LogActivityTypeName = LogActivityTypeConstants.CHANGE_PASSWORD_FAILURE,
                        LogCategoryName = LogActivityCategoryType.Security.ToString(),
                        CorrelationId = Guid.NewGuid().ToString(),
                        BooksMasterOrganizationId = booksMasterOrgId,
                        OrganizationPartyId = orgPartyId,
                        Message = $"User {person.FirstName} {person.LastName} unable to change forgotten password.",
                        FromUserLoginName = changePassword.EnterpriseUserName,
                        FromUserLoginId = user.UserId,

                        FromUserFirstName = person.FirstName,
                        FromUserLastName = person.LastName,
                        FromUserRealpageId = user.RealPageId.ToString(),

                        ToUserLoginId = null,
                        ToUserLoginName = null
                    });
                }
                catch (Exception ex)
                {
                    // WriteToErrorLog(exception: ex);
				}
			}
            else
            {
                SetErrorOnchangePasswordResponse(false, true, string.Empty, changePasswordResponse);
                changePasswordResponse.EnterpriseUserName = changePassword.EnterpriseUserName;

                // add successful activity status
                try
                {
                    LogActivity.WriteActivity(new ActivityDetails
                    {
                        LogActivityTypeName = LogActivityTypeConstants.CHANGE_PASSWORD_SUCCESS,
                        LogCategoryName = LogActivityCategoryType.Security.ToString(),
                        CorrelationId = Guid.NewGuid().ToString(),
                        BooksMasterOrganizationId = booksMasterOrgId,
                        OrganizationPartyId = orgPartyId,
                        Message = $"User {person.FirstName} {person.LastName} successfully changed forgotten password.",
                        FromUserLoginName = changePassword.EnterpriseUserName,
                        FromUserLoginId = user.UserId,

                        FromUserFirstName = person.FirstName,
                        FromUserLastName = person.LastName,
                        FromUserRealpageId = user.RealPageId.ToString(),

                        ToUserLoginId = null,
                        ToUserLoginName = null
                    });
                }
                catch (Exception ex)
                {
                    // WriteToErrorLog(exception: ex);
				}
			}

            return changePasswordResponse;
        }

		/// <summary>
		/// Validate Password
		/// </summary>
		/// <param name="enterpriseUserName">UserName</param>
		/// <param name="passwordToValidate">passwordToValidate</param>
		/// <returns>Validate Password Response</returns>
		public ValidatePasswordResponse ValidatePasswordForUser(string enterpriseUserName, string passwordToValidate)
        {
            var response = new ValidatePasswordResponse { IsSuccess = true, ErrorReason = string.Empty };

            // check if user exists
            var user = _userLoginRepository.GetUserLoginOnly(enterpriseUserName);
            if (user == null)
            {
                response.IsError = true;
                response.ErrorReason = "User name is incorrect or not found.";
                return response;
            }

            var orgPartyId = _userLoginRepository.GetPrimaryOrgIdByUserId(user.UserId);

            return ValidatePassword(new ValidatePassword
            {
                EnterpriseUserName = enterpriseUserName,
                PartyId = orgPartyId,
                PasswordToValidate = passwordToValidate,
                PasswordModifiedDate = user.PasswordModifiedDate,
                PasswordHash = user.PasswordHash,
                PasswordSalt = user.PasswordSalt,
                CheckPasswordHistory = true
            });
        }

		/// <summary>
		/// validate Password
		/// </summary>
		/// <param name="validatePassword">validatePassword</param>
		/// <returns>Validate Password Response</returns>
        public ValidatePasswordResponse ValidatePassword(ValidatePassword validatePassword)
        {
            var response = new ValidatePasswordResponse { IsSuccess = true, ErrorReason = string.Empty };
            string validationErrorMessage = string.Empty;

            // Get password policy
            var managePasswordPolicy = new ManagePasswordPolicy(_passwordPolicyRepository);
            var passwordPolicy = managePasswordPolicy.GetPasswordPolicy(validatePassword.PartyId);

            if (passwordPolicy == null)
            {
                response.IsError = true;
                response.ErrorReason = $"Unable to find password policy for organization - {validatePassword.PartyId}";
                return response;
            }

            // password should not be usename
            if (validatePassword.PasswordToValidate.IndexOf(validatePassword.EnterpriseUserName, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                response.IsError = true;
                response.ErrorReason = $"Your password cannot be the same as your Username.";
                return response;
            }

            // password length
            if (validatePassword.PasswordToValidate.Length < passwordPolicy.MinimumLength)
            {
				validationErrorMessage += (string.IsNullOrWhiteSpace(validationErrorMessage)) ? string.Empty : " ";
				validationErrorMessage += $"Your password must be at least {passwordPolicy.MinimumLength} characters.";
                response.IsError = true;
            }

            if (validatePassword.PasswordToValidate.Length > passwordPolicy.MaximumLength)
            {
				validationErrorMessage += (string.IsNullOrWhiteSpace(validationErrorMessage)) ? string.Empty : " ";
				validationErrorMessage += $"Your password must be {passwordPolicy.MaximumLength} characters or less.";
                response.IsError = true;
            }

            // min lowercase / uppercase
            int uppderCharCount = validatePassword.PasswordToValidate.Count(char.IsUpper);
            int lowerCharCount = validatePassword.PasswordToValidate.Count(char.IsLower);

            if (lowerCharCount < passwordPolicy.MinimumLowercase ||
                uppderCharCount < passwordPolicy.MinimumUppercase)
            {
				validationErrorMessage += (string.IsNullOrWhiteSpace(validationErrorMessage)) ? string.Empty : " ";
				validationErrorMessage += $"Your password must include minimum {passwordPolicy.MinimumLowercase} lower case characters and minimum {passwordPolicy.MinimumUppercase} upper case characters.";
                response.IsError = true;
            }

            // min numeric
            var numericCharCount = validatePassword.PasswordToValidate.Count(char.IsDigit);
            if (numericCharCount < passwordPolicy.MinimumNumeric)
            {
				validationErrorMessage += (string.IsNullOrWhiteSpace(validationErrorMessage)) ? string.Empty : " ";
				validationErrorMessage += $"Your password must include {passwordPolicy.MinimumNumeric} numeric characters.";
                response.IsError = true;
            }

            Match match = Regex.Match(validatePassword.PasswordToValidate, "[^a-z0-9]", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
				validationErrorMessage += (string.IsNullOrWhiteSpace(validationErrorMessage)) ? string.Empty : " ";
				validationErrorMessage += $"Your password must include minimum {passwordPolicy.MinimumSpecialCharacter} special characters.";
                response.IsError = true;
            }

            // Check for new users, password should not be the same with the temporary password
            if (validatePassword.PasswordModifiedDate == null && validatePassword.PasswordHash != null && validatePassword.CheckPasswordHistory)
            {
                var hashedPwd = validatePassword.PasswordToValidate.PasswordHashBySalt(Convert.FromBase64String(validatePassword.PasswordSalt));
                if (hashedPwd == validatePassword.PasswordHash)
                {
                    validationErrorMessage = $"Your password cannot be the same as your temporary password.";
                    response.IsError = true;
                }
            }

            if (!response.IsError && validatePassword.CheckPasswordHistory) // make db call only if other password criteria meets
            {
                if (passwordPolicy.PreventPasswordReuse && passwordPolicy.NumberOfPasswordsToRemember > 0)
                {
                    // check if new password is not from history
                    var passwordsFromHistory =
                        _credentialRepository.GetPasswordHistory(validatePassword.EnterpriseUserName,
                            passwordPolicy.NumberOfPasswordsToRemember);
                    if (passwordsFromHistory != null)
                    {
                        foreach (var pwdHistory in passwordsFromHistory)
                        {
                            if (pwdHistory.PasswordHash != null)
                            {
                                var hashedSaltPwd =
                                validatePassword.PasswordToValidate.PasswordHashBySalt(
                                    Convert.FromBase64String(pwdHistory.PasswordSalt));

                                if (hashedSaltPwd == pwdHistory.PasswordHash)
                                {
                                    validationErrorMessage =
                                        $"Your password should not be from past {passwordPolicy.NumberOfPasswordsToRemember} passwords";
                                    response.IsError = true;
                                    break;
                                }
                            }

                        }
                    }
                }
            }

            if (response.IsError)
            {
                response.IsSuccess = false;
                response.ErrorReason = validationErrorMessage;
            }

            return response;
        }

		#endregion

		#region Reset Password

		/// <summary>
		/// Reset Enterprise User Password
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="userResetPassword">userResetPassword</param>
		/// <returns>Reset Password Response</returns>
		public ResetPasswordResponse ResetPassword(Guid realPageId, UserResetPassword userResetPassword)
        {
            ResetPasswordResponse response = new ResetPasswordResponse();

            if (userResetPassword == null)
            {
				throw new ArgumentNullException(nameof(userResetPassword), "Null UserResetPassword.");
			}

			if (realPageId == null || realPageId == Guid.Empty)
            {
                response.IsError = true;
                response.ErrorReason = "RealPage Id for user not provided.";
                return response;
            }

            if (string.IsNullOrEmpty(userResetPassword.OldPassword))
            {
                response.IsError = true;
                response.ErrorReason = "Old Password is not specified.";
                return response; // user not exists
            }

            if (string.IsNullOrEmpty(userResetPassword.NewPassword))
            {
                response.IsError = true;
                response.ErrorReason = "New Password is not specified.";
                return response;
            }

            var user = _userLoginRepository.GetUserLoginOnly(realPageId);
            if (user == null)
            {
                response.IsError = true;
                response.ErrorReason = "User Name is incorrect or not found.";
                return response; // user not exists
            }

            response.EnterpriseUserName = user.LoginName;
            response.UserId = user.UserId;

            // check if the old password is not the users realpage guid. if it is, skip checking the old password matches
            if (!userResetPassword.OldPassword.Equals(_userClaim.UserRealPageGuid.ToString(), StringComparison.OrdinalIgnoreCase))
	        {
				// check user old password matches with one in db
				var hashedSaltPwd = userResetPassword.OldPassword.PasswordHashBySalt(Convert.FromBase64String(user.PasswordSalt));
		        if (hashedSaltPwd != user.PasswordHash)
		        {
			        response.IsError = true;
			        response.ErrorReason = "Current password is incorrect.";
			        return response; // user not exists
		        }
			}

            var primaryOrg = _userLoginRepository.GetUserOrganizationWithStatus(user.UserId, user.LastLogin, 0, true);

            // Validate Password Criteria for new password
            var validatePasswordRespose = ValidatePassword(new ValidatePassword
            {
                EnterpriseUserName = user.LoginName,
                PasswordToValidate = userResetPassword.NewPassword,
                PartyId = primaryOrg.PartyId,
                PasswordModifiedDate = user.PasswordModifiedDate,
                PasswordHash = user.PasswordHash,
                PasswordSalt = user.PasswordSalt,
                CheckPasswordHistory = true
            });

            if (validatePasswordRespose.IsError)
            {
                response.IsError = true;
                response.ErrorReason = validatePasswordRespose.ErrorReason;
                return response;
            }

            // get hash /salt for new password
            var newPasswordHashSalt = userResetPassword.NewPassword.PasswordHash();

            // all good - change password - update history table
            var spResponse = _credentialRepository.ResetEnterpriseUserCredential(realPageId, newPasswordHashSalt.PasswordHash, newPasswordHashSalt.PasswordSalt, primaryOrg.PartyId);

            if (spResponse == null || spResponse.Id == 0)
            {
                response.IsError = true;
                if (spResponse != null)
                    response.ErrorReason = spResponse.ErrorMessage; // todo: throw error
                return response;
            }

			//update user status to active
			var updateUserStatusResponse = _credentialRepository.UpdateUserStatusByCompany(realPageId, primaryOrg.PartyId, UserUiStatusType.Active, primaryOrg.FromDate, null);
			if (updateUserStatusResponse == null || updateUserStatusResponse.Id == 0)
			{
				response.IsError = true;
				if (updateUserStatusResponse != null)
					response.ErrorReason = updateUserStatusResponse.ErrorMessage;
				return response;
			}

			LogCredentialActivityWithClaim(LogActivityTypeConstants.CHANGE_PASSWORD_SUCCESS,
                user.LoginName, user.UserId,
                  "User {0} {1} successfully reset password.",
                  System.Reflection.MethodBase.GetCurrentMethod().Name);

            response.IsSuccess = true;
            return response;
        }

		/// <summary>
		/// Reset Enterprise User with temporary Password
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="userResetPassword">userResetPassword</param>
		/// <returns>Reset Password Response</returns>
		public ResetPasswordResponse SetTemporaryPassword(Guid realPageId, UserResetPassword userResetPassword)
        {
            ResetPasswordResponse response = new ResetPasswordResponse();

            if (userResetPassword == null)
            {
                throw new ArgumentNullException(nameof(userResetPassword));
            }

            if (realPageId == null || realPageId == Guid.Empty)
            {
                response.IsError = true;
                response.ErrorReason = "Real Page Id for user not provided.";
                return response;
            }

            if (string.IsNullOrEmpty(userResetPassword.NewPassword))
            {
                response.IsError = true;
                response.ErrorReason = "New Password is not specified";
                return response;
            }

            var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);
            if (userLogin == null)
            {
                response.IsError = true;
                response.ErrorReason = "User Name is incorrect or not found.";
                return response; // user not exists
            }

            //var primaryOrg = _userLoginRepository.ListOrganizationStatusByUserId(userLogin.UserId).FirstOrDefault(p => p.PrimaryOrganization);
            //var orgPartyId = organizationList.FirstOrDefault(p => p.PrimaryOrganization).PartyId;
            OrganizationStatus primaryOrgStatus = _userLoginRepository.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, 0, true);

            // Validate Password Criteria for new password
            var validatePasswordResponse = ValidatePassword(new ValidatePassword
            {
                EnterpriseUserName = userLogin.LoginName,
                PasswordToValidate = userResetPassword.NewPassword,
                PartyId = primaryOrgStatus.PartyId,
                PasswordModifiedDate = userLogin.PasswordModifiedDate,
                PasswordHash = userLogin.PasswordHash,
                PasswordSalt = userLogin.PasswordSalt,
                CheckPasswordHistory = false
            });
            if (validatePasswordResponse.IsError)
            {
                response.IsError = true;
                response.ErrorReason = validatePasswordResponse.ErrorReason;
                return response;
            }
            
            //IPersonaRepository personaRepository = new PersonaRepository();
            //IOrganizationRepository organizationRepository = new OrganizationRepository();
            //IList<IdentityProviderType> identityProviderTypeList = new List<IdentityProviderType>();
            //identityProviderTypeList = organizationRepository.GetOrganizationIdentityProviderType(primaryOrg.RealPageId);
            //
            //IIdentityProviderType idpt = (from a in identityProviderTypeList where a.IsLocal == (userLogin.Is3rdPartyIDP ? false : true) select a).FirstOrDefault();
            //if (idpt == null) { idpt = identityProviderTypeList[0]; }

            if (!userLogin.Is3rdPartyIDP)
            {
                // get hash /salt for new password
                var newPasswordHashSalt = userResetPassword.NewPassword.PasswordHash();

                // all good - change password - update history table
                var spResponse = _credentialRepository.SetEnterpriseUserTemporaryPassword(realPageId, primaryOrgStatus.PartyId, newPasswordHashSalt.PasswordHash, newPasswordHashSalt.PasswordSalt, userLogin, primaryOrgStatus);

                if (spResponse != null && spResponse.Id == 0)
                {
                    response.IsError = true;
                    if (spResponse != null)
                        response.ErrorReason = spResponse.ErrorMessage; // todo: throw error
                    return response;
                }
            }

			// activity logging
			try
			{
				// for activity logging
				IPerson person = _managePerson.GetPerson(userLogin.RealPageId);
				long booksMasterOrgId = GetDefaultBooksMasterOrgIdForUser(userLogin.RealPageId);
                
                string message = _userClaim.ImpersonatedBy == Guid.Empty
                    ? string.Format("User {0} {1} inserted a temporary password for user {2} {3}.", _userClaim.FirstName, _userClaim.LastName, person.FirstName, person.LastName)
                    : string.Format("User RealPage Access ({0}) inserted a temporary password for user {1} {2}.", _userClaim.ImpersonatedByName, person.FirstName, person.LastName);


                LogActivity.WriteActivity(new ActivityDetails
				{
					LogActivityTypeName = LogActivityTypeConstants.CHANGE_PASSWORD_SUCCESS,
					LogCategoryName = LogActivityCategoryType.Security.ToString(),
					CorrelationId = Guid.NewGuid().ToString(),
					BooksMasterOrganizationId = booksMasterOrgId,
                    OrganizationPartyId = _userClaim.OrganizationPartyId,
					Message = message,
					FromUserLoginName = _userClaim.LoginName,
					FromUserLoginId = _userClaim.UserId,

					FromUserFirstName = _userClaim.FirstName,
					FromUserLastName = _userClaim.LastName,
					FromUserRealpageId = _userClaim.UserRealPageGuid.ToString(),

					ToUserLoginId = userLogin.UserId,
					ToUserLoginName = userLogin.LoginName
				});
			}
			catch (Exception ex)
			{
				//WriteToErrorLog($"ManageCredential.SetTemporaryPassword error.", exception: ex);
			}

			response.IsSuccess = true;
            return response;
        }

        /// <summary>
        /// Check Password Expiration
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="enterpriseUserRealPageId">User unique identifier</param>
        /// <returns>Check Password Expiration Response</returns>
        public CheckPasswordExpirationResponse CheckPasswordExpiration(long userId, Guid enterpriseUserRealPageId)
        {
            var response = new CheckPasswordExpirationResponse { SeverityLevel = SeverityLevelType.None };
            long primaryOrgPartyId = _userLoginRepository.GetPrimaryOrgIdByUserId(userId);

            if (enterpriseUserRealPageId == Guid.Empty)
            {
                response.IsError = true;
                response.ErrorReason = "RealPageId for user not supplied.";
                return response;
            }

            if (primaryOrgPartyId == 0)
            {
                response.IsError = true;
                response.ErrorReason = "Unable to get organization Id for user from claims.";
                return response;
            }

            var managePasswordPolicy = new ManagePasswordPolicy(_passwordPolicyRepository);
            var passwordPolicy = managePasswordPolicy.GetPasswordPolicy(primaryOrgPartyId);

            if (passwordPolicy == null)
            {
                response.IsError = true;
                response.ErrorReason = $"Unable to find password policy for organization - {primaryOrgPartyId}";
                return response;
            }

            if (!passwordPolicy.EnablePasswordExpiration)
            {
                return response; // no expiration policy for organization
            }

            var userLogin = _userLoginRepository.GetUserLoginOnly(enterpriseUserRealPageId);

            if (userLogin == null)
            {
                response.IsError = true;
                response.ErrorReason = "User Name is incorrect or not found.";
                return response;
            }

            if (userLogin.Is3rdPartyIDP)
            {
                response.IsError = false;
                response.ErrorReason = "Password expiration is not applicable to users on external identity provider";
	            response.IsPasswordExpired = false;
                return response;
            }

            if (userLogin.PasswordModifiedDate != null)
            {
                var maxDaysToExpire = passwordPolicy.PasswordExpirationPeriodInDays;
                var starttOfNotice = 10;
                var startofWarning = 4;
                var startOfCritical = 1;

                var daysLeftPasswordToExpire = maxDaysToExpire - Convert.ToInt32((DateTime.Now - userLogin.PasswordModifiedDate.Value).TotalDays);
                response.DaysToExpire = daysLeftPasswordToExpire;

                if (daysLeftPasswordToExpire <= 0)
                {
                    response.IsError = false;
                    response.ErrorReason = "User Password is already expired.";
                    response.SeverityLevel = SeverityLevelType.Critical;
                    response.IsPasswordExpired = true;
                    return response;
                }

                // TODO: days to compare will come from database
                if (daysLeftPasswordToExpire <= startOfCritical)
                {
                    response.SeverityLevel = SeverityLevelType.Critical;
                }
                else if (daysLeftPasswordToExpire <= startofWarning)
                {
                    response.SeverityLevel = SeverityLevelType.Warning;
                }
                else if (daysLeftPasswordToExpire <= starttOfNotice)
                {
                    response.SeverityLevel = SeverityLevelType.Information;
                }
            }

            return response;
        }

		/// <summary>
		/// Returns all Security Questions for user (Custom questions - TBD + System default questions)
		/// </summary>
		/// <param name="enterpriseUserName">UserName</param>
		/// <returns>User All SecurityQuestion Response</returns>
		public UserAllSecurityQuestionResponse UserAllSecurityQuestions(string enterpriseUserName)
        {
            var response = new UserAllSecurityQuestionResponse();

            if (string.IsNullOrEmpty(enterpriseUserName))
            {
                response.IsError = true;
                response.ErrorReason = "User Name is not specified.";
                return response;
            }

            response.EnterpriseUserName = enterpriseUserName;

            // TODO: get custom questions by user name
            var securityQuestions = _credentialRepository.GetAllSecurityQuestions(enterpriseUserName);

            if (securityQuestions == null || securityQuestions.Count <= 0)
            {
                response.IsError = true;
                response.ErrorReason = "Error while getting security questions.";
                return response;
            }

            response.SecurityQuestions = securityQuestions;
            return response;
        }

		/// <summary>
		/// GetUser
		/// </summary>
		/// <param name="enterpriseUserName"></param>
		/// <returns></returns>
		public ListResponse GetUser(string enterpriseUserName)
        {
            var response = new ListResponse();

            if (string.IsNullOrEmpty(enterpriseUserName))
            {
                response.IsError = true;
                response.ErrorReason = "User Name is not specified.";
                return response;
            }

            IUserLoginRepository userLoginRepository = new UserLoginRepository();
            var user = userLoginRepository.GetUserLoginOnly(enterpriseUserName);
            if (user == null)
            {
                response.IsError = true;
                response.ErrorReason = "The Username \"" + enterpriseUserName + "\" is incorrect or was not found.";
                return response; // user not exists
            }

            IList<object> list = new List<object>();
            list.Add(user);
            response = new ListResponse()
            {
                Records = list,
                TotalRows = list.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1
            };
            return response;
        }

        #endregion

        #region Set Password

        /// <summary>
        /// SetPassword
        /// </summary>
        /// <param name="setPassword">Set Password</param>
        /// <returns>Change Password Response</returns>
        public ChangePasswordResponse SetPassword(SetPassword setPassword)
        {
            var setPasswordResponse = new ChangePasswordResponse();

            if (string.IsNullOrEmpty(setPassword.EnterpriseUserName))
            {
                SetErrorOnchangePasswordResponse(true, false, "No Username specified.", setPasswordResponse);
                return setPasswordResponse;
            }

            if (string.IsNullOrEmpty(setPassword.ActivityToken))
            {
                SetErrorOnchangePasswordResponse(true, false, "Set Password Activity Token is not specified.", setPasswordResponse);
                return setPasswordResponse;
            }

            if (string.IsNullOrEmpty(setPassword.NewPassword))
            {
                SetErrorOnchangePasswordResponse(true, false, "New Password is not specified.", setPasswordResponse);
                return setPasswordResponse;
            }

            // check if user exists
            var userLogin = _userLoginRepository.GetUserLoginOnly(setPassword.EnterpriseUserName);
            if (userLogin == null)
            {
                SetErrorOnchangePasswordResponse(true, false, "Username is incorrect or not found.", setPasswordResponse);
                return setPasswordResponse;
            }

            if (userLogin.Is3rdPartyIDP)
            {
                SetErrorOnchangePasswordResponse(true, false, "Set password is not applicable to users on external identity provider.", setPasswordResponse);
                return setPasswordResponse;
            }

            // Get default org for user
            //var orgPartyId = GetDefaultOrgIdForUser(user.UserId);
            OrganizationStatus primaryOrgStatus = _userLoginRepository.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, 0, true);
            Organization primaryOrg = _manageOrganization.GetOrganization(primaryOrgStatus.RealPageId);
            if (_userClaim.OrganizationPartyId == 0)
            {
                // update the claim with the info now that we know it and reinit the repository so it has it
                _userClaim.OrganizationPartyId = primaryOrg.PartyId;
                _userClaim.OrganizationMasterId = primaryOrg.BooksMasterId;
                _userClaim.CustomerMasterId = primaryOrg.BooksCustomerMasterId;
                _userClaim.OrganizationName = primaryOrg.Name;
                _userClaim.OrganizationRealPageGuid = primaryOrg.RealPageId;

                _manageUserLogin = new ManageUserLogin(_userClaim);
                _userRepository = new UserRepository(_userClaim);
            }

            var tokenResult = _credentialRepository.GetActivityToken(setPassword.EnterpriseUserName, setPassword.ActivityToken, (int) ActivityType.NewUserRegistrationVerification, primaryOrg.PartyId);
            if (tokenResult == null || string.IsNullOrEmpty(tokenResult.Token))
            {
                SetErrorOnchangePasswordResponse(true, false, "Activity Token is expired.", setPasswordResponse);

                var additionalInfo = new Dictionary<string, object> {{"ActivityToken", setPassword.ActivityToken}};

                //Log.ForContext("AdditionalInfo", additionalInfo).Write(LogEventLevel.Error, "Activity Token is expired.");
                var logger = Log.Logger;
				logger = logger.ForContext($"AdditionalInfo", JsonConvert.SerializeObject(additionalInfo, Formatting.Indented), true);
				logger = logger.ForContext("ProductModule", this.GetType());

                logger.Write(LogEventLevel.Error, "{ActionName} - {state}", new object[] { "SetPassword", $"Activity Token is expired. setPassword.EnterpriseUserName {setPassword.EnterpriseUserName}" });

                return setPasswordResponse;
            }

            // Validate Password Criteria
            var response = ValidatePassword(new ValidatePassword
            {
                EnterpriseUserName = setPassword.EnterpriseUserName,
                PasswordToValidate = setPassword.NewPassword,
                PartyId = primaryOrgStatus.PartyId,
                PasswordModifiedDate = userLogin.PasswordModifiedDate,
                PasswordHash = userLogin.PasswordHash,
                PasswordSalt = userLogin.PasswordSalt,
                CheckPasswordHistory = true
            });

            if (response.IsError)
            {
                SetErrorOnchangePasswordResponse(true, false, response.ErrorReason, setPasswordResponse);
                return setPasswordResponse;
            }

            var newPasswordHashDetail = setPassword.NewPassword.PasswordHash();

            var enterpriseUserId = _credentialRepository.UpdateEnterpriseUserCredential(setPassword.EnterpriseUserName, newPasswordHashDetail.PasswordHash, newPasswordHashDetail.PasswordSalt, setPassword.ActivityToken, (int) ActivityType.NewUserRegistrationVerification, primaryOrg.PartyId);

            // for activity logging
            var person = _managePerson.GetPerson(userLogin.RealPageId);

            if (string.IsNullOrEmpty(enterpriseUserId))
            {
                SetErrorOnchangePasswordResponse(true, false, "Error while updating password.", setPasswordResponse);
                try
                {
                    // activity logging
                    LogActivity.WriteActivity(new ActivityDetails
                    {
                        LogActivityTypeName = LogActivityTypeConstants.CHANGE_PASSWORD_FAILURE,
                        LogCategoryName = LogActivityCategoryType.Security.ToString(),
                        CorrelationId = Guid.NewGuid().ToString(),
                        BooksMasterOrganizationId = primaryOrg.BooksMasterId,
                        OrganizationPartyId = primaryOrg.PartyId,
                        Message = $"User {person.FirstName} {person.LastName} unable to set the password.",
                        FromUserLoginName = setPassword.EnterpriseUserName,
                        FromUserLoginId = userLogin.UserId,

                        FromUserFirstName = person.FirstName,
                        FromUserLastName = person.LastName,
                        FromUserRealpageId = userLogin.RealPageId.ToString(),

                        ToUserLoginId = null,
                        ToUserLoginName = null
                    });
                }
                catch (Exception ex)
                {
                    //WriteToErrorLog(exception: ex);
                }
            }
            else
            {
                SetErrorOnchangePasswordResponse(false, true, string.Empty, setPasswordResponse);
                setPasswordResponse.EnterpriseUserName = setPassword.EnterpriseUserName;

                // activity logging
                try
                {
                    LogActivity.WriteActivity(new ActivityDetails
                    {
                        LogActivityTypeName = LogActivityTypeConstants.CHANGE_PASSWORD_SUCCESS,
                        LogCategoryName = LogActivityCategoryType.Security.ToString(),
                        CorrelationId = Guid.NewGuid().ToString(),
                        BooksMasterOrganizationId = primaryOrg.BooksMasterId,
                        OrganizationPartyId = primaryOrg.PartyId,
                        Message = $"User {person.FirstName} {person.LastName} successfully set the password.",
                        FromUserLoginName = setPassword.EnterpriseUserName,
                        FromUserLoginId = userLogin.UserId,

                        FromUserFirstName = person.FirstName,
                        FromUserLastName = person.LastName,
                        FromUserRealpageId = userLogin.RealPageId.ToString(),

                        ToUserLoginId = null,
                        ToUserLoginName = null
                    });
                }
                catch (Exception ex)
                {
                    // WriteToErrorLog(exception: ex);
                }
            }

            if (primaryOrgStatus.IsPending != null && primaryOrgStatus.IsPending.Value)
            {
                //Check if user status is Pending then activate user - set status to active   
                if (!_manageUserLogin.UpdateActiveUserStatus(userLogin.RealPageId, UserUiStatusType.Active))
                {
                    SetErrorOnchangePasswordResponse(true, false, "Error while updating user status to active.", setPasswordResponse);
                }
            }
            else if (primaryOrgStatus.IsForceReSetPassword.Value)
            {
                //Check if user status is Force Rest Password 
                if (
                    !_manageUserLogin.CreateUpdateUserStatus(userLogin.RealPageId, UserUiStatusType.ForceResetPassword))
                {
                    SetErrorOnchangePasswordResponse(true, false, "Error while updating user status from forced reset password.",
                        setPasswordResponse);
                }
            }

            return setPasswordResponse;
        }

        #endregion

        #region Set Security Questions

		/// <summary>
		/// Set User Security Questions
		/// </summary>
		/// <param name="userSecurityQuestionsAnswers">user Security Questions Answers</param>
		/// <returns>Set User SecurityQuestions Response</returns>
        public SetUserSecurityQuestionsResponse SetUserSecurityQuestions(UserSecurityAnswer userSecurityQuestionsAnswers)
        {
            var setUserSecurityQuestionsResponse = new SetUserSecurityQuestionsResponse();
            int activityId = (int)ActivityType.NewUserRegistrationVerification;

            if (string.IsNullOrEmpty(userSecurityQuestionsAnswers.EnterpriseUserName))
            {
                setUserSecurityQuestionsResponse.IsError = true;
                setUserSecurityQuestionsResponse.ErrorReason = "No Username specified.";
                return setUserSecurityQuestionsResponse;
            }

            // get user's identity provider
            var identityProviderType = _credentialRepository.GetIdentityProviderTypeByLoginName(userSecurityQuestionsAnswers.EnterpriseUserName);

            if (identityProviderType == null || !identityProviderType.IsLocal)
            {
                setUserSecurityQuestionsResponse.IsError = true;
                setUserSecurityQuestionsResponse.ErrorReason = "Set security questions is not applicable to users on external identity provider.";
                return setUserSecurityQuestionsResponse;
            }

            // check if SecurityQuestionAnswers received from user
            if (userSecurityQuestionsAnswers.SecurityQuestionAnswers == null || userSecurityQuestionsAnswers.SecurityQuestionAnswers.Count <= 0)
            {
                setUserSecurityQuestionsResponse.IsError = true;
                setUserSecurityQuestionsResponse.ErrorReason = "No questions received from user.";
                return setUserSecurityQuestionsResponse;
            }

            // check if 3 SecurityQuestionAnswers received from user
            if (userSecurityQuestionsAnswers.SecurityQuestionAnswers.Count != 3)
            {
                setUserSecurityQuestionsResponse.IsError = true;
                setUserSecurityQuestionsResponse.ErrorReason = "Incorrect number of questions received from user, 3 questions are required.";
                return setUserSecurityQuestionsResponse;
            }

            // check if answer length < 50 chars
            foreach (var ans in userSecurityQuestionsAnswers.SecurityQuestionAnswers)
            {
                if (ans.Answer.Trim().Length > 50)
                {
                    setUserSecurityQuestionsResponse.IsError = true;
                    setUserSecurityQuestionsResponse.ErrorReason = "Answer should be less than 50 chars.";
                    return setUserSecurityQuestionsResponse;
                }
            }

            // check if ActivityToken is supplied
            if (string.IsNullOrEmpty(userSecurityQuestionsAnswers.ActivityToken))
            {
                setUserSecurityQuestionsResponse.IsError = true;
                setUserSecurityQuestionsResponse.ErrorReason = "Null or empty activity Token.";
                return setUserSecurityQuestionsResponse;
            }

			var userLogin = _userLoginRepository.GetUserLoginOnly(userSecurityQuestionsAnswers.EnterpriseUserName);
			if (userLogin == null)
			{
				setUserSecurityQuestionsResponse.IsError = true;
				setUserSecurityQuestionsResponse.ErrorReason = "User name is incorrect or not found.";
				return setUserSecurityQuestionsResponse;
			}
			// Get default org for user
			var orgPartyId = _userLoginRepository.GetPrimaryOrgIdByUserId(userLogin.UserId);

			// check token is matching & not expired
			var tokenResult = _credentialRepository.GetActivityToken(userSecurityQuestionsAnswers.EnterpriseUserName, userSecurityQuestionsAnswers.ActivityToken, activityId, orgPartyId);
            if (string.IsNullOrEmpty(tokenResult?.Token))
            {
                setUserSecurityQuestionsResponse.IsError = true;
                setUserSecurityQuestionsResponse.ErrorReason = "Set Password Activity Token is expired.";
                return setUserSecurityQuestionsResponse;
            }

            // Get hash of answers
            userSecurityQuestionsAnswers.SecurityQuestionAnswers = GetHashedAnswers(userSecurityQuestionsAnswers.SecurityQuestionAnswers);

            // Save security question /answers
            _credentialRepository.SaveSecurityQuestionAnswers(userSecurityQuestionsAnswers, orgPartyId);			

			// activity logging
			try
			{
				// for activity logging
				IPerson person = _managePerson.GetPerson(userLogin.RealPageId);
				long booksMasterOrgId = GetDefaultBooksMasterOrgIdForUser(userLogin.RealPageId);
				LogActivity.WriteActivity(new ActivityDetails
				{
					LogActivityTypeName = LogActivityTypeConstants.CHANGE_SECURITY_QUESTIONS_SUCCESS,
					LogCategoryName = LogActivityCategoryType.Security.ToString(),
					CorrelationId = Guid.NewGuid().ToString(),
					BooksMasterOrganizationId = booksMasterOrgId,
                    OrganizationPartyId = orgPartyId,
					Message = string.Format("User {0} {1} has successfully set their security questions.", person.FirstName, person.LastName),
					FromUserLoginName = userLogin.LoginName,
					FromUserLoginId = userLogin.UserId,

					FromUserFirstName = person.FirstName,
					FromUserLastName = person.LastName,
					FromUserRealpageId = userLogin.RealPageId.ToString(),

					ToUserLoginId = userLogin.UserId,
					ToUserLoginName = userLogin.LoginName
				});
			}
			catch (Exception ex)
			{
				//WriteToErrorLog($"ManageCredential.SetTemporaryPassword error.", exception: ex);
			}
			
			// all good
			setUserSecurityQuestionsResponse.IsError = false;
            setUserSecurityQuestionsResponse.IsSuccess = true;
            setUserSecurityQuestionsResponse.ErrorReason = string.Empty;
            setUserSecurityQuestionsResponse.EnterpriseUserName = userSecurityQuestionsAnswers.EnterpriseUserName;

            return setUserSecurityQuestionsResponse;
        }

		#endregion

		#region GetUserSelectedSecurityQuestions

		/// <summary>
		/// Get User Selected Security Questions
		/// </summary>
		/// <param name="realpageId">User unique identifier</param>
		/// <returns>Users All Security Question Response</returns>
		public UsersAllSecurityQuestionResponse GetUserSelectedSecurityQuestions(Guid realpageId)
        {
            var response = new UsersAllSecurityQuestionResponse { };

            if (realpageId == null || realpageId == Guid.Empty)
            {
                response.IsError = true;
                response.ErrorReason = "RealPageId for user not supplied.";
                return response;
            }

            var result = _credentialRepository.GetUserSelectedSecurityQuestions(realpageId);

            if (result == null || result.Count == 0)
            {
                response.IsError = true;
                response.ErrorReason = "User has not set security question.";
                return response;
            }

            response.SecurityQuestions = result;

            return response;
        }

		#endregion

		#region SaveUserSelectedSecurityQuestionResponse

		/// <summary>
		/// Save User Selected Security Questions
		/// </summary>
		/// <param name="realpageId">User unique identifier</param>
		/// <param name="securityQuestionAnswer">security Question Answer</param>
		/// <returns>Save User Selected SecurityQuestion Response</returns>
		public SaveUserSelectedSecurityQuestionResponse SaveUserSelectedSecurityQuestions(Guid realpageId, IList<SecurityQuestionAnswer> securityQuestionAnswer)
        {
            var response = new SaveUserSelectedSecurityQuestionResponse { };

            if (realpageId == Guid.Empty)
            {
                response.IsError = true;
                response.ErrorReason = "RealPageId for user not supplied.";
                return response;
            }

			var userLogin = _manageUserLogin.GetUserLoginOnly(realpageId);

			if (userLogin == null)
			{
				response.IsError = true;
				response.ErrorReason = "User Name is incorrect or not found.";
				return response; // user not exists
			}

			// check if SecurityQuestionAnswers received from user
			if (securityQuestionAnswer == null || securityQuestionAnswer.Count <= 0)
            {
                response.IsError = true;
                response.ErrorReason = "No questions received from user.";
                return response;
            }

            // check if 3 SecurityQuestionAnswers received from user
            if (securityQuestionAnswer.Count != 3)
            {
                response.IsError = true;
                response.ErrorReason = "Incorrect number of questions received from user.";
                return response;
            }

            // check if answer length < 50 chars
            foreach (var ans in securityQuestionAnswer)
            {
                if (ans.Answer.Length > 50)
                {
                    response.IsError = true;
                    response.ErrorReason = "Answer should be less than 50 chars.";
                    return response;
                }
            }

			var existingSQ = GetUserSelectedSecurityQuestions(realpageId);
			// Get hash of answers
			securityQuestionAnswer = GetHashedAnswers(securityQuestionAnswer);

            var result = _credentialRepository.SaveUserSelectedSecurityQuestions(realpageId, securityQuestionAnswer);

            if (result == 0)
            {
				// activity logging
				LogCredentialActivityWithClaim(LogActivityTypeConstants.CHANGE_SECURITY_QUESTIONS_FAILURE, userLogin.LoginName, userLogin.UserId,
				 "User {0} {1} unable to change security questions.",
				System.Reflection.MethodBase.GetCurrentMethod().Name);

				response.IsError = true;
                response.ErrorReason = "Unable to save security questions.";
                return response;
            }

			if ((existingSQ.SecurityQuestions == null) || (existingSQ.SecurityQuestions.Count == 0)) 
			{
				LogCredentialActivityWithClaim(LogActivityTypeConstants.CHANGE_SECURITY_QUESTIONS_SUCCESS, userLogin.LoginName, userLogin.UserId,
				 "User {0} {1} has successfully set their security questions.",
				System.Reflection.MethodBase.GetCurrentMethod().Name);
			}
			else 
			{
				LogCredentialActivityWithClaim(LogActivityTypeConstants.CHANGE_SECURITY_QUESTIONS_SUCCESS, userLogin.LoginName, userLogin.UserId,
				 "User {0} {1} has successfully changed their security questions.",
				System.Reflection.MethodBase.GetCurrentMethod().Name);
			}			

			return response;
        }

        #endregion

        #region GetIdentityProviderTypeByLoginName

        /// <summary>
        /// Gets Identity Provider By EnterpriseUserName
        /// </summary>
        /// <param name="enterpriseUserName"></param>
        /// <returns>Identity Provider Type</returns>
        public IdentityProviderType GetIdentityProviderTypeByLoginName(string enterpriseUserName)
        {
            return _credentialRepository.GetIdentityProviderTypeByLoginName(enterpriseUserName);
        }

        #endregion

        #region GetNewUserRegistrationVerificationToken

        /// <summary>
        /// Get New User Registration Verification Token Details 
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="realPageId"></param>
        /// <returns>Verification Token</returns>
        public string GetNewUserRegistrationVerificationToken(long userId, Guid realPageId)
        {
			var orgPartyId = _userLoginRepository.GetPrimaryOrgIdByUserId(userId);

			return _credentialRepository.CreateActivityToken(orgPartyId, realPageId, (int)ActivityType.NewUserRegistrationVerification);
        }

        #endregion

        #region Private Methods
		/// <summary>
		/// Select Random Questions
		/// </summary>
		/// <param name="securityQuestions"></param>
		/// <returns>List of SecurityQuestion</returns>
        private IList<SecurityQuestion> SelectRandomQuestions(IList<SecurityQuestion> securityQuestions)
        {
            int MAX_RANDOM_QUESTIONS = 2;

            if (securityQuestions.Count <= MAX_RANDOM_QUESTIONS)
                return securityQuestions;

            var rnd = new Random(DateTime.Now.Millisecond);
            var randomQuestionId = Enumerable.Range(0, securityQuestions.Count).OrderBy(x => rnd.Next()).Take(MAX_RANDOM_QUESTIONS).ToList();

            var randomQ = new List<SecurityQuestion>();

            foreach (var range in randomQuestionId)
            {
                randomQ.Add(securityQuestions[range]);
            }

            return randomQ;
        }

		/// <summary>
		/// Set Error Onchange Password Response
		/// </summary>
		/// <param name="isError"></param>
		/// <param name="isSuccess"></param>
		/// <param name="errorReason"></param>
		/// <param name="changePasswordResponse"></param>
        private void SetErrorOnchangePasswordResponse(bool isError, bool isSuccess, string errorReason, ChangePasswordResponse changePasswordResponse)
        {
            changePasswordResponse.IsError = isError;
            changePasswordResponse.ErrorReason = errorReason;
            changePasswordResponse.IsSuccess = isSuccess;
        }

		/// <summary>
		/// Set Login Success Insert Response
		/// </summary>
		/// <param name="isError"></param>
		/// <param name="isSuccess"></param>
		/// <param name="errorReason"></param>
		/// <param name="loginSuccessResponse"></param>
        private void SetLoginSuccessInsertResponse(bool isError, bool isSuccess, string errorReason, LoginSuccessResponse loginSuccessResponse)
        {
            loginSuccessResponse.IsError = isError;
            loginSuccessResponse.ErrorReason = errorReason;
            loginSuccessResponse.IsSuccess = isSuccess;
        }

		/// <summary>
		/// Get Hashed Answers
		/// </summary>
		/// <param name="securityQuestionAnswers">security Question Answers</param>
		/// <returns>List of SecurityQuestionAnswer</returns>
        private IList<SecurityQuestionAnswer> GetHashedAnswers(IList<SecurityQuestionAnswer> securityQuestionAnswers)
        {
            foreach (var qa in securityQuestionAnswers)
            {
                qa.Answer = qa.Answer.Trim().ToUpper().Sha256();
            }

            return securityQuestionAnswers;
        }

		/// <summary>
		/// Get Default Books Master OrgId For User
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <returns>Book MasterId</returns>
		private long GetDefaultBooksMasterOrgIdForUser(Guid realPageId)
        {
            // Get user's org id
            long booksMasterId = 0;
            var listOrg = _credentialRepository.ListOrganizationByRealPageId(realPageId);
            if (listOrg != null)
            {
                booksMasterId = listOrg[0].BooksMasterId;
            }

            return booksMasterId;
        }

		/// <summary>
		/// Get User Current Status
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <returns>User Current Staus</returns>
		//private UserCurrentStaus GetUserCurrentStatus(Guid realPageId)
        //{
        //    var user = new UserCurrentStaus();
        //    //Get User Statues            
        //    var userLogin = _manageUserLogin.GetUserLogin(realPageId);
        //    user.IsActive = userLogin.IsActive.Value;
        //    user.IsExpired = userLogin.IsExpired.Value;
        //    user.IsLocked = userLogin.IsLocked.Value;
        //    user.IsPending = userLogin.IsPending.Value;
        //    return user;
        //}

		/// <summary>
		/// Log Credential Activit yWith Claim
		/// </summary>
		/// <param name="logActivityTypeName"></param>
		/// <param name="fromLoginName"></param>
		/// <param name="fromLoginId"></param>
		/// <param name="message"></param>
		/// <param name="productModuleStepName"></param>
        private void LogCredentialActivityWithClaim(string logActivityTypeName, string fromLoginName, long fromLoginId, string message, string productModuleStepName)
        {
            try
            {
                // log only if user claims & org id
                LogActivity.WriteActivity(new ActivityDetails
                {
                    LogActivityTypeName = logActivityTypeName,
                    LogCategoryName = LogActivityCategoryType.Security.ToString(),
                    CorrelationId = _userClaim.CorrelationId.ToString(),
                    BooksMasterOrganizationId = _userClaim.OrganizationMasterId,
                    OrganizationPartyId = _userClaim.OrganizationPartyId,
                    Message = string.Format(message, _userClaim.FirstName, _userClaim.LastName),
                    FromUserLoginName = fromLoginName,
                    FromUserLoginId = fromLoginId,

                    FromUserFirstName = _userClaim.FirstName,
                    FromUserLastName = _userClaim.LastName,
                    FromUserRealpageId = _userClaim.UserRealPageGuid.ToString(),

                    ToUserLoginId = null,
                    ToUserLoginName = null
                });
            }
            catch (Exception ex)
            {
                // WriteToErrorLog(exception: ex);
			}
		}

        #endregion
    }

	/// <summary>
	/// User Current Staus
	/// </summary>
    //public class UserCurrentStaus
    //{
	//	/// <summary>
	//	/// IsPending
	//	/// </summary>
    //    public bool IsPending { get; set; }
    //
	//	/// <summary>
	//	/// IsExpired
	//	/// </summary>
    //    public bool IsExpired { get; set; }
    //
	//	/// <summary>
	//	/// IsActive
	//	/// </summary>
    //    public bool IsActive { get; set; }
    //
	//	/// <summary>
	//	/// IsLocked
	//	/// </summary>
    //    public bool IsLocked { get; set; }
    //}
}