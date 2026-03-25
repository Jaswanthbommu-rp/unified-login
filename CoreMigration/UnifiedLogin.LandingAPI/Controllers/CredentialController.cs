using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System.Web;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Code for password management
    /// </summary>
    [Route("")]
    [ApiController]
    public class CredentialController : BaseController
    {
        private readonly IManageCredentialAsync _manageCredential;
        private readonly IUserLoginRepositoryAsync _userLoginRepository;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public CredentialController(
            IManageCredentialAsync manageCredential,
            IUserLoginRepositoryAsync userLoginRepository,
            IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
            _manageCredential = manageCredential ?? throw new ArgumentNullException(nameof(manageCredential));
            _userLoginRepository = userLoginRepository ?? throw new ArgumentNullException(nameof(userLoginRepository));
        }

        #region Private Methods
        private OrgUserData MapOrganizationUserData(UserLoginOnly objUserLoginOnly)
        {
            OrgUserData objOrgUserData = new OrgUserData
            {
                PartyId = objUserLoginOnly.PartyId,
                RealPageId = objUserLoginOnly.RealPageId,
                PersonaId = objUserLoginOnly.PersonaId,
                LoginNameType = objUserLoginOnly.LoginNameType,
                PasswordHash = objUserLoginOnly.PasswordHash,
                PasswordSalt = objUserLoginOnly.PasswordSalt,
                PasswordModifiedDate = objUserLoginOnly.PasswordModifiedDate,
                LastLogin = objUserLoginOnly.LastLogin,
                Password = objUserLoginOnly.Password,
                Is3rdPartyIDP = objUserLoginOnly.Is3rdPartyIDP,
                LoginName = objUserLoginOnly.LoginName,
                UserId = objUserLoginOnly.UserId
            };
            return objOrgUserData;
        }

        private UnifiedLogin.SharedObjects.Landing.HttpRequest ConvertToCustomHttpRequest(Microsoft.AspNetCore.Http.HttpRequest aspNetRequest)
        {
            var customRequest = new UnifiedLogin.SharedObjects.Landing.HttpRequest
            {
                UserAgent = aspNetRequest.Headers["User-Agent"].ToString(),
                UserHostAddress = aspNetRequest.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                Headers = aspNetRequest.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            };
            return customRequest;
        }

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

        private void WriteToDiagnosticLog(string message = null, Dictionary<string, object> logData = null, object[] messageProperties = null)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var logger = Log.Logger;
            if (logData?.Keys != null)
            {
                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
            }
            logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", userClaim?.CorrelationId.ToString() ?? string.Empty);
            logger.Write(LogEventLevel.Debug, message, messageProperties);
        }
        #endregion

        /// <summary>
        /// Get Security Questions for user on forgot password
        /// </summary>
        [HttpGet("credential/getsecurityquestions")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SecurityQuestionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSecurityQuestions(string enterpriseUserName, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(enterpriseUserName))
                {
                    return BadRequest("enterpriseUserName is required");
                }

                var customRequest = ConvertToCustomHttpRequest(HttpContext.Request);
                var userDeviceDetails = UserDeviceDetails.ParseUserDeviceDetails(customRequest);

                var securityQuestionResponse = await _manageCredential.GetSecurityQuestionAsync(enterpriseUserName.Trim(), userDeviceDetails, cancellationToken);

                if (securityQuestionResponse.IsError)
                {
                    WriteToDiagnosticLog(message: "{ActionName} - {state}", logData: null, messageProperties: new object[] { "GetSecurityQuestions", $"{securityQuestionResponse.ErrorReason}. For User -{enterpriseUserName}" });
                }

                return Ok(securityQuestionResponse);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{ActionName} - {state}", logData: null, exception: ex, messageProperties: new object[] { "GetSecurityQuestions", $"Error : {ex.Message}" });
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Verify Security questions for user
        /// </summary>
        [HttpPost("credential/verifysecurityanswers")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SecurityAnswerResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VerifySecurityAnswers(UserSecurityAnswer userSecurityAnswer, CancellationToken cancellationToken = default)
        {
            try
            {
                if (userSecurityAnswer == null)
                {
                    return BadRequest("userSecurityAnswer is required");
                }

                var customRequest = ConvertToCustomHttpRequest(HttpContext.Request);
                var userDeviceDetails = UserDeviceDetails.ParseUserDeviceDetails(customRequest);

                var securityAnswerResponse = await _manageCredential.VerifySecurityAnswersAsync(userSecurityAnswer, userDeviceDetails, cancellationToken);

                if (securityAnswerResponse.IsError)
                {
                    WriteToDiagnosticLog(message: "{ActionName} - {state}", logData: null, messageProperties: new object[] { "VerifySecurityAnswers", $"Error : {securityAnswerResponse.ErrorReason}. For User -{securityAnswerResponse.EnterpriseUserName}" });
                }

                return Ok(securityAnswerResponse);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{ActionName} - {state}", logData: null, exception: ex, messageProperties: new object[] { "VerifySecurityAnswers", $"Error : {ex.Message}" });
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Change forgot password
        /// </summary>
        [HttpPost("credential/forgotpassword")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ChangePasswordResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgotPassword(ChangePassword changePassword, CancellationToken cancellationToken = default)
        {
            try
            {
                if (changePassword == null)
                {
                    return BadRequest("changePassword is required");
                }

                var changePasswordResponse = await _manageCredential.ForgotPasswordAsync(changePassword, cancellationToken);

                if (changePasswordResponse.IsError)
                {
                    WriteToDiagnosticLog(message: "{ActionName} - {state}", logData: null, messageProperties: new object[] { "ForgotPassword", $"Error : {changePasswordResponse.ErrorReason}" });
                }

                return Ok(changePasswordResponse);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{ActionName} - {state}", logData: null, exception: ex, messageProperties: new object[] { "ForgotPassword", $"Error : {ex.Message}" });
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Validate Password for the user using his associated default organization password policy
        /// </summary>
        [HttpPost("credential/validatepassword")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ValidatePasswordResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidatePassword(string enterpriseUserName, string passwordToValidate, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(enterpriseUserName) || string.IsNullOrWhiteSpace(passwordToValidate))
                {
                    return BadRequest("enterpriseUserName and passwordToValidate are required");
                }

                enterpriseUserName = HttpUtility.UrlDecode(enterpriseUserName);
                passwordToValidate = HttpUtility.UrlDecode(passwordToValidate);

                var validatePasswordResponse = await _manageCredential.ValidatePasswordForUserAsync(enterpriseUserName, passwordToValidate, cancellationToken);

                if (validatePasswordResponse.IsError)
                {
                    WriteToDiagnosticLog(message: "{ActionName} - {state}", logData: null, messageProperties: new object[] { "ValidatePasswordResponse", $"{validatePasswordResponse.ErrorReason}. For User -{enterpriseUserName}" });
                }

                return Ok(validatePasswordResponse);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{ActionName} - {state}", logData: null, exception: ex, messageProperties: new object[] { "ValidatePassword", $"Error : {ex.Message}" });
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Check Password Expiration
        /// </summary>
        [HttpGet("credential/checkpasswordexpiration")]
        [Authorize]
        [ProducesResponseType(typeof(CheckPasswordExpirationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckPasswordExpiration(CancellationToken cancellationToken = default)
        {
            try
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;
                var orgPartyId = userClaim?.OrganizationPartyId ?? 0;

                if (realPageUserId == Guid.Empty)
                {
                    return BadRequest("Unable to get realPageId for user from claims.");
                }

                if (orgPartyId <= 0)
                {
                    return BadRequest("Unable to get organization Id for user from claims.");
                }

                var checkPasswordExpirationResponse = await _manageCredential.CheckPasswordExpirationAsync(userClaim.UserId, userClaim.UserRealPageGuid, cancellationToken);

                if (checkPasswordExpirationResponse.IsError)
                {
                    WriteToDiagnosticLog(message: "{ActionName} - {state}", logData: null, messageProperties: new object[] { "CheckPasswordExpiration", $"Error - {checkPasswordExpirationResponse.ErrorReason}" });
                }

                return Ok(checkPasswordExpirationResponse);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{ActionName} - {state}", logData: null, exception: ex, messageProperties: new object[] { "ValidatePassword", $"Error : {ex.Message}" });
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get All Security Questions
        /// </summary>
        [HttpGet("credential/userallsecurityquestions")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserAllSecurityQuestionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UserAllSecurityQuestions(string enterpriseUserName, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(enterpriseUserName))
                {
                    return BadRequest("enterpriseUserName is required");
                }

                var userAllSecurityQuestionResponse = await _manageCredential.UserAllSecurityQuestionsAsync(enterpriseUserName.Trim(), cancellationToken);

                if (userAllSecurityQuestionResponse.IsError)
                {
                    WriteToDiagnosticLog(message: "{ActionName} - {state}", logData: null, messageProperties: new object[] { "UserAllSecurityQuestions", $"Error - {userAllSecurityQuestionResponse.ErrorReason}. For User -{enterpriseUserName}" });
                }

                return Ok(userAllSecurityQuestionResponse);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{ActionName} - {state}", logData: null, exception: ex, messageProperties: new object[] { "ValidatePassword", $"Error : {ex.Message}" });
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get User
        /// </summary>
        [HttpGet("credential/getUser")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUser(string enterpriseUserName, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(enterpriseUserName))
                {
                    return BadRequest("enterpriseUserName is required");
                }

                var getUser = await _manageCredential.GetUserAsync(enterpriseUserName.Trim(), cancellationToken);

                if (getUser.IsError)
                {
                    WriteToDiagnosticLog(message: "{ActionName} - {state}", logData: null, messageProperties: new object[] { "GetUser", $"Error - {getUser.ErrorReason}. For User -{enterpriseUserName}" });
                }
                else if (getUser.Records != null && getUser.Records.Any())
                {
                    var userClaim = _userClaimsAccessor.GetUserClaim() ?? new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
                    var objOrgUserData = MapOrganizationUserData((UserLoginOnly)getUser.Records[0]);
                    objOrgUserData.OrganizationPartyId = await _userLoginRepository.GetPrimaryOrgIdByUserIdAsync(((UserLoginOnly)getUser.Records[0]).UserId, cancellationToken);

                    IList<object> list = new List<object> { objOrgUserData };
                    var response = new ListResponse
                    {
                        Records = list,
                        TotalRows = list.Count,
                        RowsPerPage = 9999,
                        ErrorReason = string.Empty,
                        TotalPages = 1
                    };
                    getUser = response;
                }

                return Ok(getUser);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{ActionName} - {state}", logData: null, exception: ex, messageProperties: new object[] { "ValidatePassword", $"Error : {ex.Message}" });
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Set Password
        /// </summary>
        [HttpPost("credential/setpassword")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ChangePasswordResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetPassword(SetPassword setPassword, CancellationToken cancellationToken = default)
        {
            try
            {
                if (setPassword == null)
                {
                    return BadRequest("setPassword is required");
                }

                if (string.IsNullOrWhiteSpace(setPassword.EnterpriseUserName) || string.IsNullOrWhiteSpace(setPassword.NewPassword))
                {
                    return BadRequest("enterpriseUserName and newPassword are required");
                }

                var changePasswordResponse = await _manageCredential.SetPasswordAsync(setPassword, cancellationToken);

                if (changePasswordResponse.IsError)
                {
                    WriteToDiagnosticLog(message: "{ActionName} - {state}", logData: null, messageProperties: new object[] { "SetPassword", $"Error - {changePasswordResponse.ErrorReason}. For User -{setPassword.EnterpriseUserName}" });
                }

                return Ok(changePasswordResponse);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{ActionName} - {state}", logData: null, exception: ex, messageProperties: new object[] { "SetPassword", $"Error : {ex.Message}" });
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Set User Security Questions
        /// </summary>
        [HttpPost("credential/setusersecurityquestions")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SetUserSecurityQuestionsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetUserSecurityQuestions(UserSecurityAnswer userSecurityQuestionsAnswers, CancellationToken cancellationToken = default)
        {
            try
            {
                if (userSecurityQuestionsAnswers == null)
                {
                    return BadRequest("userSecurityQuestionsAnswers is required");
                }

                var setUserSecurityQuestionsResponse = await _manageCredential.SetUserSecurityQuestionsAsync(userSecurityQuestionsAnswers, cancellationToken);

                if (setUserSecurityQuestionsResponse.IsError)
                {
                    WriteToDiagnosticLog(message: "{ActionName} - {state}", logData: null, messageProperties: new object[] { "SetUserSecurityQuestions", $"Error - {setUserSecurityQuestionsResponse.ErrorReason}. For User -{userSecurityQuestionsAnswers.EnterpriseUserName}" });
                }

                return Ok(setUserSecurityQuestionsResponse);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{ActionName} - {state}", logData: null, exception: ex, messageProperties: new object[] { "SetUserSecurityQuestions", $"Error : {ex.Message}" });
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Reset existing Password to new password
        /// </summary>
        [HttpPost("credential/resetpassword")]
        [ProducesResponseType(typeof(ResetPasswordResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPassword(UserResetPassword userResetPassword, Guid? realPageId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;

                realPageId = (realPageId == null || realPageId == Guid.Empty) ? userResetPassword.RealPageId : realPageId;
                realPageId = (realPageId == null || realPageId == Guid.Empty) ? realPageUserId : realPageId;

                if (realPageId == null || realPageId == Guid.Empty)
                {
                    return BadRequest("Invalid parameter: realPageId");
                }

                if (userResetPassword == null)
                {
                    return BadRequest("Invalid parameter: userResetPassword");
                }

                var resetPasswordResponse = await _manageCredential.ResetPasswordAsync(realPageId.Value, userResetPassword, cancellationToken);

                if (resetPasswordResponse.IsError)
                {
                    try
                    {
                        if (userClaim != null && userClaim.OrganizationPartyId > 0)
                        {
                            LogActivity.WriteActivity(new ActivityDetails
                            {
                                LogActivityTypeName = LogActivityTypeConstants.CHANGE_PASSWORD_FAILURE,
                                LogCategoryName = LogActivityCategoryType.Security.ToString(),
                                CorrelationId = userClaim.CorrelationId.ToString(),
                                BooksMasterOrganizationId = userClaim.OrganizationMasterId,
                                OrganizationPartyId = userClaim.OrganizationPartyId,
                                Message = string.Format("User {0} {1} unable to change password.", userClaim.FirstName, userClaim.LastName),
                                FromUserLoginName = userClaim.LoginName,
                                FromUserLoginId = userClaim.UserId,
                                FromUserFirstName = userClaim.FirstName,
                                FromUserLastName = userClaim.LastName,
                                FromUserRealpageId = userClaim.UserRealPageGuid.ToString(),
                                ToUserLoginId = null,
                                ToUserLoginName = null
                            });
                        }
                    }
                    catch
                    {
                        // Swallow logging exception
                    }

                    WriteToDiagnosticLog(message: "{ActionName} - {state}", logData: null, messageProperties: new object[] { "ResetPassword", $"Error - {resetPasswordResponse.ErrorReason}." });
                }

                return Ok(resetPasswordResponse);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{ActionName} - {state}", logData: null, exception: ex, messageProperties: new object[] { "ResetPassword", $"Error : {ex.Message}" });
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Set Temporary password
        /// </summary>
        [HttpPost("credential/settemporarypassword")]
        [ProducesResponseType(typeof(ResetPasswordResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetTemporaryPassword(UserResetPassword userResetPassword, CancellationToken cancellationToken = default)
        {
            try
            {
                if (userResetPassword == null)
                {
                    return BadRequest("Invalid parameter: userResetPassword");
                }

                if (userResetPassword.RealPageId == null || userResetPassword.RealPageId == Guid.Empty)
                {
                    return BadRequest("Invalid parameter: realPageId");
                }

                var userClaim = _userClaimsAccessor.GetUserClaim() ?? new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
                var resetPasswordResponse = await _manageCredential.SetTemporaryPasswordAsync(userResetPassword.RealPageId.Value, userResetPassword, userClaim, cancellationToken);

                if (resetPasswordResponse.IsError)
                {
                    try
                    {
                        var userClaimForLog = _userClaimsAccessor.GetUserClaim();
                        if (userClaimForLog != null)
                        {
                            LogActivity.WriteActivity(new ActivityDetails
                            {
                                LogActivityTypeName = LogActivityTypeConstants.CHANGE_PASSWORD_FAILURE,
                                LogCategoryName = LogActivityCategoryType.Security.ToString(),
                                CorrelationId = userClaimForLog.CorrelationId.ToString(),
                                BooksMasterOrganizationId = userClaimForLog.OrganizationMasterId,
                                OrganizationPartyId = userClaimForLog.OrganizationPartyId,
                                Message = string.Format("User {0} {1} unable to insert temporary password.", userClaimForLog.FirstName, userClaimForLog.LastName),
                                FromUserLoginName = userClaimForLog.LoginName,
                                FromUserLoginId = userClaimForLog.UserId,
                                FromUserFirstName = userClaimForLog.FirstName,
                                FromUserLastName = userClaimForLog.LastName,
                                FromUserRealpageId = userClaimForLog.UserRealPageGuid.ToString(),
                                ToUserLoginId = null,
                                ToUserLoginName = null
                            });
                        }
                    }
                    catch
                    {
                        // Swallow logging exception
                    }

                    WriteToErrorLog(message: "{ActionName} - {state}", logData: null, messageProperties: new object[] { "SetTemporaryPassword", $"Error - {resetPasswordResponse.ErrorReason}." });
                }

                return Ok(resetPasswordResponse);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{ActionName} - {state}", logData: null, exception: ex, messageProperties: new object[] { "SetTemporaryPassword", $"Error : {ex.Message}" });
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get user selected all Security Questions (User Account)
        /// </summary>
        [HttpGet("credential/userselectedsecurityquestions")]
        [ProducesResponseType(typeof(ObjectListOutput<SecurityQuestion, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserSelectedSecurityQuestions(Guid? realPageId = null, CancellationToken cancellationToken = default)
        {
            ObjectListOutput<SecurityQuestion, IErrorData> output = new ObjectListOutput<SecurityQuestion, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            try
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;

                realPageId = (realPageId == null || realPageId == Guid.Empty) ? realPageUserId : realPageId;

                if (realPageId == null || realPageId == Guid.Empty)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "Credential.GetSecurityQuestions.1";
                    errorStatus.ErrorMsg = "Invalid parameter: realPageId";
                    output.Status = errorStatus;
                    return BadRequest(output);
                }

                var result = await _manageCredential.GetUserSelectedSecurityQuestionsAsync(realPageId.Value, cancellationToken);

                if (result.IsError)
                {
                    errorStatus.ErrorCode = "Credential.GetSecurityQuestions.2";
                    errorStatus.ErrorMsg = result.ErrorReason;
                    output.list = SecurityQuestion.SecurityQuestionListExample();
                    output.Status = errorStatus;
                    WriteToDiagnosticLog(message: "{ActionName} - {state}", logData: null, messageProperties: new object[] { "GetUserSelectedSecurityQuestions", $"Error - {result.ErrorReason}." });

                    return Ok(output);
                }

                output.list = result.SecurityQuestions;
                output.Status = errorStatus;

                return Ok(output);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{ActionName} - {state}", logData: null, exception: ex, messageProperties: new object[] { "GetUserSelectedSecurityQuestions", $"Error : {ex.Message}" });
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Save user selected Security Questions and answers (User Account)
        /// </summary>
        [HttpPost("credential/userselectedsecurityquestions")]
        [ProducesResponseType(typeof(SaveUserSelectedSecurityQuestionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveUserSelectedSecurityQuestions(IList<SecurityQuestionAnswer> securityQuestionAnswer, Guid? realPageId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;

                realPageId = (realPageId == null || realPageId == Guid.Empty) ? realPageUserId : realPageId;

                if (realPageId == null || realPageId == Guid.Empty)
                {
                    return BadRequest("Invalid parameter: realPageId");
                }

                var result = await _manageCredential.SaveUserSelectedSecurityQuestionsAsync(realPageId.Value, securityQuestionAnswer, cancellationToken);

                if (result.IsError)
                {
                    WriteToDiagnosticLog(message: "{ActionName} - {state}", logData: null, messageProperties: new object[] { "SaveUserSelectedSecurityQuestions", $"Error - {result.ErrorReason}." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(message: "{ActionName} - {state}", logData: null, exception: ex, messageProperties: new object[] { "SaveUserSelectedSecurityQuestions", $"Error : {ex.Message}" });
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
