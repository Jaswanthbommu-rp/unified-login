using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
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
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using System.Linq;
using Serilog;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using Newtonsoft.Json;
using Serilog.Events;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    /// <summary>
    /// Code for password management
    /// </summary>
    public class CredentialController : BaseApiController
    {
        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public CredentialController()
        {
            _userLoginRepository = new UserLoginRepository();
        }

        /// <summary>
        /// Used for dependency injection
        /// </summary>

        /// <param name="userLoginRepository"></param>

        public CredentialController(IUserLoginRepository userLoginRepository)
        {
            _userLoginRepository = userLoginRepository;
        }
        #endregion
        #region Private variables

        IUserLoginRepository _userLoginRepository;

        #endregion

        #region Private Methods
        private OrgUserData MapOrganizationUserData(UserLoginOnly objUserLoginOnly)
        {
            OrgUserData objOrgUserData = new OrgUserData();
            objOrgUserData.PartyId = objUserLoginOnly.PartyId;
            objOrgUserData.RealPageId = objUserLoginOnly.RealPageId;
            objOrgUserData.PersonaId = objUserLoginOnly.PersonaId;
            objOrgUserData.LoginNameType = objUserLoginOnly.LoginNameType;
            objOrgUserData.PasswordHash = objUserLoginOnly.PasswordHash;
            objOrgUserData.PasswordSalt = objUserLoginOnly.PasswordSalt;
            objOrgUserData.PasswordModifiedDate = objUserLoginOnly.PasswordModifiedDate;
            objOrgUserData.LastLogin = objUserLoginOnly.LastLogin;
            objOrgUserData.Password = objUserLoginOnly.Password;
            objOrgUserData.Is3rdPartyIDP = objUserLoginOnly.Is3rdPartyIDP;
            objOrgUserData.LoginName = objUserLoginOnly.LoginName;
            objOrgUserData.UserId = objUserLoginOnly.UserId;
            return objOrgUserData;
        }
        #endregion
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
                    WriteToDiagnosticLog(message: "{errorReason}. For User -{enterpriseUserName}", logData: null, messageProperties: new object[] { securityQuestionResponse.ErrorReason, enterpriseUserName });
                }

                return securityQuestionResponse;
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{exception}", logData: null, exception: ex, messageProperties: null);
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
                    WriteToDiagnosticLog(message: "{errorReason}. For User -{enterpriseUserName}", logData: null, messageProperties: new object[] { securityAnswerResponse.ErrorReason, securityAnswerResponse.EnterpriseUserName });
                }

                return securityAnswerResponse;
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{exception}", logData: null, exception: ex, messageProperties: null);
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
                    WriteToDiagnosticLog(message: "{errorReason}", logData: null, messageProperties: new object[] { changePasswordResponse.ErrorReason });
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{exception}", logData: null, exception: ex, messageProperties: null);
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
                    WriteToDiagnosticLog(message: "{errorReason}. For User -{enterpriseUserName}", logData: null, messageProperties: new object[] { validatePasswordResponse.ErrorReason, enterpriseUserName });
                }

                return validatePasswordResponse;
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{exception}", logData: null, exception: ex, messageProperties: null);
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
                    WriteToDiagnosticLog(message: "{errorReason}", logData: null, messageProperties: new object[] { checkPasswordExpirationResponse.ErrorReason });
                }

                return checkPasswordExpirationResponse;
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{exception}", logData: null, exception: ex, messageProperties: null);
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
                    WriteToDiagnosticLog(message: "{errorReason}. For User -{enterpriseUserName}", logData: null, messageProperties: new object[] { userAllSecurityQuestionResponse.ErrorReason, enterpriseUserName });
                }

                return userAllSecurityQuestionResponse;
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{exception}", logData: null, exception: ex, messageProperties: null);
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
                    WriteToDiagnosticLog(message: "{errorReason}. For User -{enterpriseUserName}", logData: null, messageProperties: new object[] { getUser.ErrorReason, enterpriseUserName });
                }
                else if (getUser.Records != null)
                {
                    var manageUserLogin = new ManageUserLogin(_userClaims);
                    //OrgUserData objOrgUserData = new OrgUserData();
                    var objOrgUserData = MapOrganizationUserData((UserLoginOnly)getUser.Records[0]);
                    objOrgUserData.OrganizationPartyId = _userLoginRepository.GetPrimaryOrgIdByUserId(((UserLoginOnly)getUser.Records[0]).UserId);

                    IList<object> list = new List<object>();
                    list.Add(objOrgUserData);
                    var response = new ListResponse()
                    {
                        Records = list,
                        TotalRows = list.Count(),
                        RowsPerPage = 9999,
                        ErrorReason = string.Empty,
                        TotalPages = 1
                    };
                    getUser = response;
                }

                return getUser;
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{exception}", logData: null, exception: ex, messageProperties: null);
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
                    WriteToDiagnosticLog(message: "{errorReason}. For User -{enterpriseUserName}", logData: null, messageProperties: new object[] { changePasswordResponse.ErrorReason, setPassword.EnterpriseUserName });
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{exception}", logData: null, exception: ex, messageProperties: null);
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
                    WriteToDiagnosticLog(message: "{errorReason}. For User -{enterpriseUserName}", logData: null, messageProperties: new object[] { setUserSecurityQuestionsResponse.ErrorReason, userSecurityQuestionsAnswers.EnterpriseUserName });
                }

                return setUserSecurityQuestionsResponse;
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{exception}", logData: null, exception: ex, messageProperties: null);
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
                            OrganizationPartyId = _userClaims.OrganizationPartyId,
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

                    WriteToDiagnosticLog(message: "{errorReason}", logData: null, messageProperties: new object[] { resetPasswordResponse.ErrorReason });
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{exception}", logData: null, exception: ex, messageProperties: null);
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
                    IManageOrganization manageOrganization = new ManageOrganization(_userClaims);
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
                            OrganizationPartyId = _userClaims.OrganizationPartyId,
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
                    WriteToDiagnosticLog(message: "{errorReason}", logData: null, messageProperties: new object[] { resetPasswordResponse.ErrorReason });
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{exception}", logData: null, exception: ex, messageProperties: null);
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
                    WriteToDiagnosticLog(message: "{errorReason}", logData: null, messageProperties: new object[] { result.ErrorReason });
                    return Request.CreateResponse(HttpStatusCode.OK, output);
                }

                output.list = result.SecurityQuestions;
                output.Status = errorStatus;

                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{exception}", logData: null, exception: ex, messageProperties: null);
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
                    WriteToDiagnosticLog(message: "{errorReason}", logData: null, messageProperties: new object[] { result.ErrorReason });
                }

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{exception}", logData: null, exception: ex, messageProperties: null);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        /// <summary>
        /// Used to write to the error log
        /// </summary>
        private void WriteToErrorLog(string message = null, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            var logger = Log.Logger;
            if (logData?.Keys != null)
            {
                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
            }
            logger = logger.ForContext("ProductModule", this.GetType());
            logger.Write(LogEventLevel.Error, exception, message, messageProperties);
        }

        /// <summary>
        /// Write to the diagnostic log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logData"></param>
        /// <param name="messageProperties"></param>
        private void WriteToDiagnosticLog(string message = null, Dictionary<string, object> logData = null, object[] messageProperties = null)
        {
            var logger = Log.Logger;
            if (logData?.Keys != null)
            {
                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
            }
            logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", _userClaims.CorrelationId.ToString());
            logger.Write(LogEventLevel.Debug, message, messageProperties);
        }

    }
}