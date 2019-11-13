using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// Code for password management
	/// </summary>
	public class CredentialController : BaseApiController
    {
        #region Public Methods
        /// <summary>
        /// Get Security Questions for user on forgot password
        /// </summary>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request when invalid request)")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Security questions by enterprise user name")]
        [Route("credential/getsecurityquestions")]
        [HttpGet]
        [AllowAnonymous]
        public SecurityQuestionResponse GetSecurityQuestions(string enterpriseUserName)
        {
            try
            {
                if (string.IsNullOrEmpty(enterpriseUserName.Trim()))
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

				var userDeviceDetails = UserDeviceDetails.ParseUserDeviceDetails(HttpContext.Current.Request);

				_userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
								
                var credentialManageService = new ManageCredential(_userClaims);
                var securityQuestionResponse = credentialManageService.GetSecurityQuestion(enterpriseUserName.Trim(), userDeviceDetails);

				if (securityQuestionResponse.IsError)
				{
					WriteToDiagnosticLog(message: $"{securityQuestionResponse.ErrorReason}. For User -{enterpriseUserName}");
				}

				return securityQuestionResponse;
			}
			catch (Exception ex)
			{
				WriteToErrorLog(exception: ex);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}
		}

        /// <summary>
        /// Verify Security questions for user
        /// </summary> 
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request when invalid request)")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Verify Security questions for user")]
        [Route("credential/verifysecurityanswers")]
        [HttpPost]
        [AllowAnonymous]
        public SecurityAnswerResponse VerifySecurityAnswers(UserSecurityAnswer userSecurityAnswer)
        {
            try
            {
                if (userSecurityAnswer == null)
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

				var userDeviceDetails = UserDeviceDetails.ParseUserDeviceDetails(HttpContext.Current.Request);

				_userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
								
                var credentialManageService = new ManageCredential(_userClaims);
                var securityAnswerResponse = credentialManageService.VerifySecurityAnswers(userSecurityAnswer, userDeviceDetails);

				if (securityAnswerResponse.IsError)
				{
                    WriteToDiagnosticLog(message: $"{securityAnswerResponse.ErrorReason}. For User -{securityAnswerResponse.EnterpriseUserName}");
				}

				return securityAnswerResponse;
			}
			catch (Exception ex)
			{
				WriteToErrorLog(exception: ex);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}
		}

        /// <summary>
        /// Change forgot password
        /// </summary> 
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request when invalid request)")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Change forgot password")]
        [Route("credential/forgotpassword")]
        [HttpPost]
        [AllowAnonymous]
        public ChangePasswordResponse ForgotPassword(ChangePassword changePassword)
        {
            ChangePasswordResponse changePasswordResponse;
            try
            {
                if (changePassword == null)
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

				_userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
				
                var credentialManageService = new ManageCredential(_userClaims);
                changePasswordResponse = credentialManageService.ForgotPassword(changePassword);

				if (changePasswordResponse.IsError)
				{
                    WriteToDiagnosticLog(message: changePasswordResponse.ErrorReason);
				}
			}
			catch (Exception ex)
			{
				WriteToErrorLog(exception: ex);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}

			return changePasswordResponse;
		}

        /// <summary>
        /// Validate Password for the user using his associated default organization password policy
        /// </summary>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request when invalid request)")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "ValidatePasswordResponse", Type = typeof(ValidatePasswordResponse))]
        [Route("credential/validatepassword")]
        [HttpPost]
        [AllowAnonymous]
        public ValidatePasswordResponse ValidatePassword(string enterpriseUserName, string passwordToValidate)
        {
            try
            {
                if (string.IsNullOrEmpty(enterpriseUserName.Trim()) || string.IsNullOrEmpty(passwordToValidate.Trim()))
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

				var credentialManageService = new ManageCredential(_userClaims);
	            enterpriseUserName = HttpUtility.UrlDecode(enterpriseUserName);
	            passwordToValidate = HttpUtility.UrlDecode(passwordToValidate);
				var validatePasswordResponse = credentialManageService.ValidatePasswordForUser(enterpriseUserName, passwordToValidate);

				if (validatePasswordResponse.IsError)
				{
                    WriteToDiagnosticLog(message: $"{validatePasswordResponse.ErrorReason}. For User -{enterpriseUserName}");
				}

				return validatePasswordResponse;
			}
			catch (Exception ex)
			{
				WriteToErrorLog(exception: ex);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}
		}

        /// <summary>
        /// Check Password Expiration
        /// </summary>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request when invalid request)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Check Password Expiration")]
        [Route("credential/checkpasswordexpiration")]
        [HttpGet]
        [Authorize]
        public CheckPasswordExpirationResponse CheckPasswordExpiration()
        {
            try
            {
                if (_realpageUserId == Guid.Empty)
                {
                    HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.ExpectationFailed)
                    {
                        Content = new StringContent("Unable to get realPageId for user from claims.")
                    };
                    throw new HttpResponseException(message);
                }

				if (_orgPartyId <= 0)
				{
					HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.ExpectationFailed)
					{
						Content = new StringContent("Unable to get organization Id for user from claims.")
					};
					throw new HttpResponseException(message);
				}

				var credentialManageService = new ManageCredential(_userClaims);
                var checkPasswordExpirationResponse = credentialManageService.CheckPasswordExpiration(_userClaims.UserId, _userClaims.UserRealPageGuid);

				if (checkPasswordExpirationResponse.IsError)
				{
                    WriteToDiagnosticLog(message: $"{checkPasswordExpirationResponse.ErrorReason}.");
				}

				return checkPasswordExpirationResponse;
			}
			catch (Exception ex)
			{
				WriteToErrorLog(exception: ex);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}
		}

        /// <summary>
        /// Get All Security Questions
        /// </summary>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request when invalid request)")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get All Security Questions")]
        [Route("credential/userallsecurityquestions")]
        [HttpGet]
        [AllowAnonymous]
        public UserAllSecurityQuestionResponse UserAllSecurityQuestions(string enterpriseUserName)
        {
            try
            {
                if (string.IsNullOrEmpty(enterpriseUserName.Trim()))
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

				_userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
				
                var credentialManageService = new ManageCredential(_userClaims);
                var userAllSecurityQuestionResponse = credentialManageService.UserAllSecurityQuestions(enterpriseUserName.Trim());

				if (userAllSecurityQuestionResponse.IsError)
				{
                    WriteToDiagnosticLog(message: $"{userAllSecurityQuestionResponse.ErrorReason}. For User -{enterpriseUserName}");
				}

				return userAllSecurityQuestionResponse;
			}
			catch (Exception ex)
			{
				WriteToErrorLog(exception: ex);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}
		}

        /// <summary>
        /// Get All Security Questions
        /// </summary>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request when invalid request)")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get User")]
        [Route("credential/getUser")]
        [HttpGet]
        [AllowAnonymous]
        public ListResponse GetUser(string enterpriseUserName)
        {
            try
            {
                if (string.IsNullOrEmpty(enterpriseUserName.Trim()))
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

				_userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
				
                var credentialManageService = new ManageCredential(_userClaims);
                var getUser = credentialManageService.GetUser(enterpriseUserName.Trim());

				if (getUser.IsError)
				{
                    WriteToDiagnosticLog(message: $"{getUser.ErrorReason}. For User -{enterpriseUserName}");
				}
                else if (getUser.Records != null)
                {
                    var manageUserLogin = new ManageUserLogin(_userClaims);
                    OrganizationStatus orgStatus = manageUserLogin.GetUserOrganizationWithStatus(((UserLoginOnly)getUser.Records[0]).UserId, DateTime.MinValue, 0, true);
                    ((UserLoginOnly)getUser.Records[0]).OrganizationPartyId = orgStatus.PartyId;
                }

                return getUser;
			}
			catch (Exception ex)
			{
				WriteToErrorLog(exception: ex);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}
		}

        /// <summary>
        /// Set Password
        /// </summary>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request when invalid request)")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Set Password")]
        [Route("credential/setpassword")]
        [HttpPost]
        [AllowAnonymous]
        public ChangePasswordResponse SetPassword(SetPassword setPassword)
        {
            ChangePasswordResponse changePasswordResponse = null;

			try
			{
				if (setPassword == null)
				{
					throw new HttpResponseException(HttpStatusCode.BadRequest);
				}

				if (string.IsNullOrEmpty(setPassword.EnterpriseUserName.Trim()) || string.IsNullOrEmpty(setPassword.NewPassword.Trim()))
				{
					throw new HttpResponseException(HttpStatusCode.BadRequest);
				}

                var credentialManageService = new ManageCredential(_userClaims);
                changePasswordResponse = credentialManageService.SetPassword(setPassword);

				if (changePasswordResponse.IsError)
				{
                    WriteToDiagnosticLog(message: $"{changePasswordResponse.ErrorReason}. For User -{setPassword.EnterpriseUserName}");
				}
			}
			catch (Exception ex)
			{
				WriteToErrorLog(exception: ex);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}

			return changePasswordResponse;
		}

        /// <summary>
        /// Set User Security Questions
        /// </summary>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request when invalid request)")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Set User Security Questions")]
        [Route("credential/setusersecurityquestions")]
        [HttpPost]
        [AllowAnonymous]
        public SetUserSecurityQuestionsResponse SetUserSecurityQuestions(UserSecurityAnswer userSecurityQuestionsAnswers)
        {
            try
            {
                if (userSecurityQuestionsAnswers == null)
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

				_userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
				
                var credentialManageService = new ManageCredential(_userClaims);
                var setUserSecurityQuestionsResponse = credentialManageService.SetUserSecurityQuestions(userSecurityQuestionsAnswers);

				if (setUserSecurityQuestionsResponse.IsError)
				{
                    WriteToDiagnosticLog(message: $"{setUserSecurityQuestionsResponse.ErrorReason}. For User -{userSecurityQuestionsAnswers.EnterpriseUserName}");
				}

				return setUserSecurityQuestionsResponse;
			}
			catch (Exception ex)
			{
				WriteToErrorLog(exception: ex);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}
		}

        /// <summary>
        /// Reset existing Password to new password
        /// </summary>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request when invalid request)")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Reset Password")]
        [Route("credential/resetpassword")]
        [HttpPost]
        public HttpResponseMessage ResetPassword(UserResetPassword userResetPassword, Guid? realPageId = null)
        {
            ResetPasswordResponse resetPasswordResponse = null;

			try
			{
				realPageId = (realPageId == null || realPageId == Guid.Empty) ? userResetPassword.RealPageId : realPageId;
				realPageId = (realPageId == null || realPageId == Guid.Empty) ? _realpageUserId : realPageId;

				if (realPageId == null || realPageId == Guid.Empty)
				{
					return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
				}

				if (userResetPassword == null)
				{
					return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: userResetPassword");
				}

				var credentialManageService = new ManageCredential(_userClaims);
				resetPasswordResponse = credentialManageService.ResetPassword(realPageId.Value, userResetPassword);

				if (resetPasswordResponse.IsError)
				{
					try
					{
						// log only if user claims & org id
						LogActivity.WriteActivity(new ActivityDetails
						{
							LogActivityTypeName = LogActivityTypeConstants.CHANGE_PASSWORD_FAILURE,
							LogCategoryName = LogActivityCategoryType.Security.ToString(),
							CorrelationId = _userClaims.CorrelationId.ToString(),
							BooksMasterOrganizationId = _userClaims.OrganizationMasterId,
							Message = string.Format("User {0} {1} unable to change password.", _userClaims.FirstName, _userClaims.LastName),
							FromUserLoginName = _userClaims.LoginName,
							FromUserLoginId = _userClaims.UserId,

							FromUserFirstName = _userClaims.FirstName,
							FromUserLastName = _userClaims.LastName,
							FromUserRealpageId = _userClaims.UserRealPageGuid.ToString(),

							ToUserLoginId = null,
							ToUserLoginName = null
						});
					}
					catch (Exception ex)
					{
						// WriteToErrorLog(exception: ex);
					}

                    WriteToDiagnosticLog(message: resetPasswordResponse.ErrorReason);
				}
			}
			catch (Exception ex)
			{
				WriteToErrorLog(exception: ex);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}

			return Request.CreateResponse(HttpStatusCode.OK, resetPasswordResponse);
		}

        /// <summary>
        /// Set Temporary password
        /// </summary>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request when invalid request)")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Reset Password")]
        [Route("credential/settemporarypassword")]
        [HttpPost]
        public HttpResponseMessage SetTemporaryPassword(UserResetPassword userResetPassword)
        {
            ResetPasswordResponse resetPasswordResponse = null;
            try
            {
                if (userResetPassword == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: userResetPassword");
                }

				if (userResetPassword.RealPageId == null || userResetPassword.RealPageId == Guid.Empty)
				{
					return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
				}

				var credentialManageService = new ManageCredential(_userClaims);
				resetPasswordResponse = credentialManageService.SetTemporaryPassword(userResetPassword.RealPageId.Value, userResetPassword);

				if (resetPasswordResponse.IsError)
				{
					IManageUserLogin manageUserLogin = new ManageUserLogin(_userClaims);
					IManagePerson managePerson = new ManagePerson();
					IManageOrganization manageOrganization = new ManageOrganization();
					try
					{
						IPerson person = managePerson.GetPerson(userResetPassword.RealPageId.Value);
                        var userLogin = manageUserLogin.GetUserLoginOnly(userResetPassword.RealPageId.Value);

						// log only if user claims & org id
						LogActivity.WriteActivity(new ActivityDetails
						{
							LogActivityTypeName = LogActivityTypeConstants.CHANGE_PASSWORD_FAILURE,
							LogCategoryName = LogActivityCategoryType.Security.ToString(),
							CorrelationId = _userClaims.CorrelationId.ToString(),
							BooksMasterOrganizationId = _userClaims.OrganizationMasterId,
							Message = string.Format("User {0} {1} unable to insert temporary password for {2} {3}.", _userClaims.FirstName, _userClaims.LastName, person.FirstName, person.LastName),
							FromUserLoginName = _userClaims.LoginName,
							FromUserLoginId = _userClaims.UserId,

							FromUserFirstName = _userClaims.FirstName,
							FromUserLastName = _userClaims.LastName,
							FromUserRealpageId = _userClaims.UserRealPageGuid.ToString(),

							ToUserLoginId = userLogin.UserId,
							ToUserLoginName = userLogin.LoginName
						});
					}
					catch (Exception ex)
					{
						// WriteToErrorLog(exception: ex);
					}
                    WriteToDiagnosticLog(message: resetPasswordResponse.ErrorReason);
				}
			}
			catch (Exception ex)
			{
				WriteToErrorLog(exception: ex);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}

            return Request.CreateResponse(HttpStatusCode.OK, resetPasswordResponse);
        }
        /// <summary>
        /// Get user selected all Security Questions (User Account)
        /// </summary>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request when invalid request)")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Security questions selected by user")]
        // [Route("credential/userselectedsecurityquestions/{realPageId}")]
        [Route("credential/userselectedsecurityquestions")]
        [HttpGet]
        public HttpResponseMessage GetUserSelectedSecurityQuestions(Guid? realPageId = null)
        {
            ObjectListOutput<SecurityQuestion, IErrorData> output = new ObjectListOutput<SecurityQuestion, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            UsersAllSecurityQuestionResponse result = new UsersAllSecurityQuestionResponse();

			try
			{
				realPageId = (realPageId == null || realPageId == Guid.Empty) ? _realpageUserId : realPageId;

				if (realPageId == null || realPageId == Guid.Empty)
				{
					errorStatus.Success = false;
					errorStatus.ErrorCode = "Credential.GetSecurityQuestions.1";
					errorStatus.ErrorMsg = "Invalid parameter: realPageId";
					output.Status = errorStatus;
					return Request.CreateResponse(HttpStatusCode.BadRequest, output);
				}

				var credentialManageService = new ManageCredential(_userClaims);
				result = credentialManageService.GetUserSelectedSecurityQuestions(realPageId.Value);

				if (result.IsError)
				{
					errorStatus.ErrorCode = "Credential.GetSecurityQuestions.2";
					errorStatus.ErrorMsg = result.ErrorReason;
					output.list = SecurityQuestion.SecurityQuestionListExample();
					output.Status = errorStatus;
					//WriteToErrorLog(message: result.ErrorReason);
					return Request.CreateResponse(HttpStatusCode.OK, output);
				}

				output.list = result.SecurityQuestions;
				output.Status = errorStatus;

				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			catch (Exception ex)
			{
				WriteToErrorLog(exception: ex);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}
		}

        /// <summary>
        /// Save user selected Security Questions and answers (User Account)
        /// </summary>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request when invalid request)")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Saves Security questions selected by user & associated answers")]
        [Route("credential/userselectedsecurityquestions")]
        [HttpPost]
        public HttpResponseMessage SaveUserSelectedSecurityQuestions(IList<SecurityQuestionAnswer> securityQuestionAnswer, Guid? realPageId = null)
        {
            try
            {
                realPageId = (realPageId == null || realPageId == Guid.Empty) ? _realpageUserId : realPageId;

				if (realPageId == null || realPageId == Guid.Empty)
				{
					return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
				}

				var credentialManageService = new ManageCredential(_userClaims);
				var result = credentialManageService.SaveUserSelectedSecurityQuestions(realPageId.Value, securityQuestionAnswer);

				if (result.IsError)
				{
					WriteToDiagnosticLog(message: result.ErrorReason);
				}

				return Request.CreateResponse(HttpStatusCode.OK, result);
			}
			catch (Exception ex)
			{
				WriteToErrorLog(exception: ex);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}
		}
		#endregion
	}
}