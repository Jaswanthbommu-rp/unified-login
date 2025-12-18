using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.LandingAPI.Attributes;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// UserLogin Controller to hold all user management related APIs
    /// </summary>
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class UserLoginController : ControllerBase
    {
        #region Private variables
        private readonly IUserClaimsAccessor _userClaimsAccessor;
        private readonly IRepositoryResponse _repositoryResponse;
        private readonly IManageUserLogin _manageUserLogin;
        private readonly IRepository _repository;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userClaimsAccessor">User claims accessor</param>
        /// <param name="repository">Repository</param>
        public UserLoginController(IUserClaimsAccessor userClaimsAccessor, IRepository repository = null)
        {
            _userClaimsAccessor = userClaimsAccessor;
            _repository = repository;
            _repositoryResponse = new RepositoryResponse();

            var userClaim = _userClaimsAccessor.GetUserClaim();
            if (_repository != null)
            {
                _manageUserLogin = new ManageUserLogin(repository, userClaim, null);
            }
            else
            {
                _manageUserLogin = new ManageUserLogin(userClaim);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Create a new UserLogin
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="userLogin">UserLogin object of the parameter values</param>
        /// <returns>Response with Success Message</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UserLogin.UserLoginOutputResult), (int)HttpStatusCode.OK)]
        [HttpPost("userlogins/{realPageId}")]
        public async Task<IActionResult> CreateUserLogin(Guid realPageId, [FromBody] UserLogin userLogin)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();

                realPageId = ((realPageId == Guid.Empty) || (realPageId == null)) ? userClaim.UserRealPageGuid : realPageId;
                if ((realPageId == Guid.Empty) || (realPageId == null))
                {
                    return BadRequest("Invalid parameter: realPageId");
                }

                if (userLogin == null)
                {
                    return BadRequest("Null parameter: UserLogin");
                }

                //CreateUserLogin
                IManageUserLogin userLoginLogic = new ManageUserLogin();
                var repositoryResponse = userLoginLogic.CreateUserLogin(realPageId, userLogin);
                if (repositoryResponse.Id == 0 || !string.IsNullOrWhiteSpace(repositoryResponse.ErrorMessage))
                {
                    return BadRequest(repositoryResponse.ErrorMessage);
                }

                UserLogin.UserLoginOutputResult result = new UserLogin.UserLoginOutputResult
                {
                    NewUserId = repositoryResponse.Id
                };

                return Ok(result);
            });
        }

        /// <summary>
        /// Get UserLogin detail
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <returns>UserLogin object</returns>
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(IUserLogin), (int)HttpStatusCode.OK)]
        [HttpGet("userlogins/{realPageId}")]
        public async Task<IActionResult> GetUserLogin(Guid realPageId)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();

                realPageId = (realPageId == Guid.Empty) ? userClaim.UserRealPageGuid : realPageId;
                if (realPageId == Guid.Empty)
                {
                    return BadRequest("Invalid parameter: realPageId");
                }

                IManageUserLogin userLoginLogic = new ManageUserLogin();
                var userLogin = userLoginLogic.GetUserLogin(realPageId, userClaim.OrganizationPartyId);

                if (userLogin != null)
                {
                    return Ok(userLogin);
                }

                return NoContent();
            });
        }

        /// <summary>
        /// Get UserLogin detail by organization
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="orgRealPageId">Organization unique identifier</param>
        /// <returns>UserLogin object</returns>
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(IUserLoginOnly), (int)HttpStatusCode.OK)]
        [HttpGet("userlogins/{realPageId}/organization/{orgRealPageId}")]
        public async Task<IActionResult> GetUserLoginByCompany(Guid realPageId, Guid orgRealPageId)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();

                realPageId = (realPageId == Guid.Empty) ? userClaim.UserRealPageGuid : realPageId;
                if (realPageId == Guid.Empty)
                {
                    return BadRequest("Invalid parameter: realPageId");
                }
                if (orgRealPageId == Guid.Empty)
                {
                    return BadRequest("Invalid parameter: orgRealPageId");
                }

                IManageUserLogin userLoginLogic = new ManageUserLogin(userClaim);
                IManageOrganization manageOrganization = new ManageOrganization(userClaim);
                var organization = manageOrganization.GetOrganization(orgRealPageId);
                var userLogin = userLoginLogic.GetUserLogin(realPageId, organization.PartyId);

                if (userLogin != null)
                {
                    return Ok(userLogin);
                }

                return NoContent();
            });
        }

        /// <summary>
        /// Update UserLogin
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="userLogin">UserLogin object of the parameter values</param>
        /// <returns>Response with Success Message</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [HttpPut("userlogins/{realPageId}")]
        public async Task<IActionResult> UpdateUserLogin(Guid realPageId, [FromBody] UserLogin userLogin)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();

                realPageId = (realPageId == Guid.Empty) ? userClaim.UserRealPageGuid : realPageId;

                if ((realPageId == Guid.Empty))
                {
                    return BadRequest("Invalid parameter: realPageId");
                }

                if (userLogin == null)
                {
                    return BadRequest("Null parameter: UserLogin");
                }

                if (userLogin.ThruDate != null && userLogin.ThruDate < DateTime.UtcNow)
                {
                    return BadRequest("ThruDate should be greater than current date.");
                }

                if (userLogin.ThruDate != null && userLogin.ThruDate < userLogin.FromDate)
                {
                    return BadRequest("ThruDate should be greater than FromDate.");
                }

                IManageUserLogin userLoginLogic = new ManageUserLogin();
                var repositoryResponse = userLoginLogic.UpdateUserLogin(realPageId, userLogin);

                if (repositoryResponse.Id == 0)
                {
                    return BadRequest(repositoryResponse.ErrorMessage);
                }

                return Ok(userLogin);
            });
        }

        /// <summary>
        /// Patch UserLogins' Active|Inactive|Lock|Unlock
        /// </summary>
        /// <param name="updateType">Patches by Batch or AllRecords</param>
        /// <param name="userLoginStatusType">Active|Inactive|Lock|Unlock</param>
        /// <param name="userLogins">Array of userlogins. Pass an empty array if updating by AllRecords i.e []</param>
        /// <returns>List of patched UserLogins</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPatch("userlogins")]
        public async Task<IActionResult> UpdateUserLogins(
            [FromQuery] UserUiStatusType? userLoginStatusType,
            [FromQuery] UserLoginUpdateType? updateType,
            [FromBody] IList<UserLogin> userLogins)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                ObjectListOutput<UserLogin, IErrorData> output = new ObjectListOutput<UserLogin, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();

                if (userLoginStatusType == null)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "200.3";
                    errorStatus.ErrorMsg = "Null parameter: userLoginStatusType";
                    output.Status = errorStatus;
                    return BadRequest(output);
                }

                if (userLogins == null && updateType == UserLoginUpdateType.Batch)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "200.3";
                    errorStatus.ErrorMsg = "Null parameter on updating batch: userLogins";
                    output.Status = errorStatus;
                    return BadRequest(output);
                }

                IManageUserLogin manageUserLogin = new ManageUserLogin(userClaim);

                if (updateType == UserLoginUpdateType.AllRecords)
                {
                    IManageProfile profileLogic = new ManageProfile(userClaim);
                    profileLogic.ListProfileDetails(new Dictionary<object, object>())
                        .ToList()
                        .ForEach(p => { userLogins.Add((UserLogin)p.userLogin); }
                        );
                }

                //Prevents the currently logged in user from being patched.
                userLogins = userLogins.Where(u => u.RealPageId != userClaim.UserRealPageGuid).ToList();

                List<RepositoryResponse> repositoryResponses = new List<RepositoryResponse>();
                IRepositoryResponse repositoryResponse = new RepositoryResponse();

                if (userLogins.Count > 0)
                {
                    //filter out unnecessary records based on action
                    if (userLoginStatusType.Value == UserUiStatusType.Locked)
                    {
                        userLogins = userLogins.ToList().Where(u => u.Status == UserUiStatusType.Active).ToList();
                    }
                    else if (userLoginStatusType.Value == UserUiStatusType.Unlocked)
                    {
                        userLogins = userLogins.ToList().Where(u => u.Status == UserUiStatusType.Locked).ToList();
                    }
                    else if (userLoginStatusType.Value == UserUiStatusType.Active)
                    {
                        userLogins = userLogins.ToList().Where(u => u.Status == UserUiStatusType.Locked || u.Status == UserUiStatusType.Deactivated).ToList();
                    }
                    else if (userLoginStatusType.Value == UserUiStatusType.Deactivated)
                    {
                        userLogins = userLogins.ToList().Where(
                            u => u.Status == UserUiStatusType.Active ||
                            u.Status == UserUiStatusType.Pending ||
                            u.Status == UserUiStatusType.Locked ||
                            u.Status == UserUiStatusType.Expired).ToList();
                    }

                    if (userLogins.Count > 0)
                    {
                        // TODO: check status - should be either Active, Disable, Lock or Unlock only
                        if (!ValidateBulkUpdateStatus(userLoginStatusType.Value))
                        {
                            errorStatus.Success = false;
                            errorStatus.ErrorCode = "200.3";
                            errorStatus.ErrorMsg = $"Bulk update is not supported for {userLoginStatusType.Value.ToString()} status.";
                        }
                        else
                        {
                            List<UserLoginOnly> userLoginsOnly = new List<UserLoginOnly>();
                            foreach (UserLogin ul in userLogins)
                            {
                                userLoginsOnly.Add(new UserLoginOnly() { RealPageId = ul.RealPageId });
                            }
                            repositoryResponse = manageUserLogin.UpdateBulkUserLogins(userLoginsOnly, userLoginStatusType.Value);

                            if (!string.IsNullOrEmpty(repositoryResponse.ErrorMessage))
                            {
                                errorStatus.Success = false;
                                errorStatus.ErrorCode = "200.3";
                                errorStatus.ErrorMsg = "Error(s) occured during bulk update!";
                            }
                            else
                            {
                                repositoryResponse = UpdateUserProductStatus(userLoginsOnly, userLoginStatusType, userClaim);
                            }
                        }
                    }
                }

                switch (userLoginStatusType.Value)
                {
                    case UserUiStatusType.Active:
                        userLogins.ToList().ForEach(u => u.IsActive = true);
                        break;
                    case UserUiStatusType.Disabled:
                        userLogins.ToList().ForEach(u => u.IsActive = false);
                        break;
                    case UserUiStatusType.Locked:
                        userLogins.ToList().ForEach(u => u.IsLocked = true);
                        break;
                    case UserUiStatusType.Unlocked:
                        userLogins.ToList().ForEach(u => u.IsLocked = false);
                        break;
                }

                output.list = userLogins;
                output.Status = errorStatus;
                return Ok(output);
            });
        }

        /// <summary>
        /// Resend Email Invitations
        /// </summary>
        /// <param name="userLogins">Array of user realpage Ids</param>
        /// <returns>List of patched UserLogins</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPost("userlogins/resendinvitation")]
        [AuthorizeRight("resendinvitation")]
        public async Task<IActionResult> ResendInvitation([FromBody] IList<UserLogin> userLogins)
        {
            return await Task.Run<IActionResult>(() =>
            {
                ObjectListOutput<UserLogin, IErrorData> output = new ObjectListOutput<UserLogin, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                bool response = true;

                if (userLogins.Count > 0)
                {
                    response = _manageUserLogin.ResendInvitation(userLogins, false);

                    if (response)
                    {
                        return Ok(response);
                    }

                    return StatusCode((int)HttpStatusCode.ExpectationFailed, response);
                }
                return Ok(response);
            });
        }

        /// <summary>
        /// Resend Email Invitations For External
        /// </summary>
        /// <param name="realpageId"></param>
        /// <returns></returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPost("userlogins/resendinvitationexternal/{realpageId}")]
        [AuthorizeScope("enterpriseapi")]
        public async Task<IActionResult> ResendInvitationExternal(Guid realpageId)
        {
            return await Task.Run<IActionResult>(() =>
            {
                ObjectListOutput<UserLogin, IErrorData> output = new ObjectListOutput<UserLogin, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                bool response = true;
                var userList = new List<UserLogin>() { new UserLogin() { RealPageId = realpageId } };

                ClaimsPrincipal currentClaimPrincipal = User;
                IManageUserLogin manageUserLogin = _manageUserLogin;

                if (!currentClaimPrincipal.Claims.Any(a => a.Type == "sub"))
                {
                    var userClaim = _manageUserLogin.GetUserClaimsFromNonUser(realpageId);

                    if (userClaim == null)
                    {
                        return StatusCode((int)HttpStatusCode.ExpectationFailed, false);
                    }

                    var identity = (ClaimsIdentity)currentClaimPrincipal.Identity;
                    identity.AddClaim(new Claim("orgPartyId", userClaim.OrganizationPartyId.ToString()));
                    identity.AddClaim(new Claim("ORGID", userClaim.OrganizationRealPageGuid.ToString()));
                    identity.AddClaim(new Claim("sub", userClaim.UserId.ToString()));
                    identity.AddClaim(new Claim("LOGINNAME", userClaim.LoginName));
                    identity.AddClaim(new Claim("ORGMASTERID", userClaim.OrganizationMasterId.ToString()));
                    identity.AddClaim(new Claim("ORGNAME", userClaim.OrganizationName));
                    identity.AddClaim(new Claim("FIRSTNAME", userClaim.FirstName));
                    identity.AddClaim(new Claim("LASTNAME", userClaim.LastName));
                    identity.AddClaim(new Claim("PERSONAID", userClaim.PersonaId.ToString()));
                    identity.AddClaim(new Claim("ImpersonatedBy", userClaim.ImpersonatedBy.ToString()));
                    identity.AddClaim(new Claim("ImpersonatedByName", userClaim.ImpersonatedByName));

                    manageUserLogin = new ManageUserLogin(userClaim);
                }

                manageUserLogin.LogUserRequestedEmailLinkResent(realpageId);

                response = manageUserLogin.ResendInvitation(userList, false);

                if (response)
                {
                    return Ok(response);
                }

                return StatusCode((int)HttpStatusCode.ExpectationFailed, false);
            });
        }

        /// <summary>
        /// Clear user password and security questions
        /// </summary>
        /// <param name="realPageId">The guid of the user to reset</param>
        /// <returns>List of patched UserLogins</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPut("userlogins/clearpasswordandquestions")]
        [AuthorizeRight("resendinvitation")]
        public async Task<IActionResult> ClearPasswordAndQuestions(Guid realPageId)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();

                var userLogin = _manageUserLogin.GetUserLogin(realPageId, userClaim.OrganizationPartyId);
                if (userLogin == null)
                {
                    return BadRequest("Invalid company");
                }

                var response = _manageUserLogin.ClearPasswordAndQuestions(realPageId);

                if (response)
                {
                    return NoContent();
                }

                return StatusCode((int)HttpStatusCode.ExpectationFailed);
            });
        }

        /// <summary>
        /// Process Future User Login Status
        /// </summary>
        /// <param name="userLogins">Array of user realpage Ids</param>
        /// <returns>List of patched UserLogins</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPost("userlogins/processfutureuserlogins")]
        [AllowAnonymous]
        public async Task<IActionResult> ProcessFutureUserLogins([FromBody] IList<ProcessUserLogin> userLogins)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                ObjectListOutput<ProcessUserLogin, IErrorData> output = new ObjectListOutput<ProcessUserLogin, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                bool response = true;
                IManageUserLogin manageUserLogin = new ManageUserLogin();

                if (userLogins.Count > 0)
                {
                    ManageUserLogin userLoginLogic = new ManageUserLogin(userClaim);
                    response = userLoginLogic.ProcessFutureUserLogins(userLogins);

                    if (response)
                    {
                        return Ok(response);
                    }

                    return StatusCode((int)HttpStatusCode.ExpectationFailed, response);
                }
                return Ok(response);
            });
        }

        /// <summary>
        /// Get user auto logout interval when expiration (thru) date is set
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("userlogin/autologoutinterval")]
        public async Task<LogOutIntervalResponse> GetLogOutInterval(Guid realPageId)
        {
            return await Task.Run(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();

                if (realPageId == null)
                {
                    throw new ArgumentException("Invalid parameter: realPageId");
                }

                IManageUserLogin userLoginLogic = new ManageUserLogin(userClaim);
                LogOutIntervalResponse response = userLoginLogic.GetLogOutInterval(realPageId, userClaim.OrganizationPartyId);
                return response;
            });
        }

        /// <summary>
        /// Create or Update User status based on statusTypeName
        /// </summary>
        /// <param name="statusTypeName">Status Type Name</param>
        /// <param name="realPageId">User unique identifier</param>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [HttpPost("userlogin/status")]
        [AuthorizeRight("lockunlockusers", "activatedeactivateusers")]
        public async Task<IActionResult> CreateUpdateUserStatus(UserUiStatusType statusTypeName, Guid? realPageId = null)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();

                if (realPageId == userClaim.UserRealPageGuid)
                {
                    return BadRequest("Invalid parameter: Cannot update currently logged-in user's status");
                }

                if (realPageId == null || realPageId == Guid.Empty)
                {
                    return BadRequest("Invalid parameter: realPageId");
                }

                IManageUserLogin userLoginLogic = new ManageUserLogin(userClaim);
                bool response = userLoginLogic.CreateUpdateUserStatus(realPageId.Value, statusTypeName);

                if (response)
                {
                    IList<UserLoginOnly> userLogins = new List<UserLoginOnly>();
                    UserLoginOnly ul = new UserLoginOnly() { RealPageId = realPageId.Value };
                    userLogins.Add(ul);
                    var repositoryResponse = UpdateUserProductStatus(userLogins, statusTypeName, userClaim);
                    return Ok(response);
                }

                return StatusCode((int)HttpStatusCode.ExpectationFailed, response);
            });
        }

        /// <summary>
        /// disable users which is called from service
        /// </summary>
        /// <param name="userLogins">Array of user realpage Ids</param>
        /// <returns>List of patched UserLogins</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [HttpPost("disableexpiredusers")]
        [AllowAnonymous]
        public async Task<IActionResult> DisableUsersFromProducts([FromBody] IList<ProcessUserLogin> userLogins)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                ObjectListOutput<UserLogin, IErrorData> output = new ObjectListOutput<UserLogin, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                IRepositoryResponse response = new RepositoryResponse();

                if (userLogins.Count > 0)
                {
                    IManageUser manageUser = new ManageUser(userClaim);
                    response = manageUser.DisableUsersFromProducts(userLogins);

                    if (string.IsNullOrEmpty(response.ErrorMessage))
                    {
                        return Ok(true);
                    }
                    return StatusCode((int)HttpStatusCode.ExpectationFailed, false);
                }
                return Ok(response);
            });
        }

        /// <summary>
        /// User Exists? User Exists in this Organization?
        /// </summary>
        /// <param name="loginName">User LoginName</param>
        /// <param name="OrganizationRealPageId">Unique Identifier - OrganizationRealPageId</param>
        /// <param name="userRealPageId">The id of the user if editing</param>
        /// <param name="isFromExport"></param>
        /// <param name="userType"></param>
        /// <returns>UserOrganizationExists object</returns>
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UserOrganizationExists), (int)HttpStatusCode.OK)]
        [HttpGet("userlogins/loginnameexists")]
        public async Task<IActionResult> IsLoginNameExists(string loginName, Guid OrganizationRealPageId, Guid? userRealPageId = null, string firstName = null, string lastName = null, int userType = 0, bool isFromExport = false)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                ObjectOutput<UserOrganizationExists, IErrorData> output = new ObjectOutput<UserOrganizationExists, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                UserOrganizationExists userOrganizationExists = new UserOrganizationExists();

                if (!userRealPageId.HasValue)
                {
                    userRealPageId = Guid.Empty;
                }

                if (OrganizationRealPageId == Guid.Empty)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "UserLogin.IsLoginNameExists.1";
                    errorStatus.ErrorMsg = "IsLoginNameExists: Invalid parameter enterprise organization Id";
                    output.obj = userOrganizationExists;
                    output.Status = errorStatus;
                    return Ok(output);
                }

                if (string.IsNullOrWhiteSpace(loginName))
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "UserLogin.IsLoginNameExists.2";
                    errorStatus.ErrorMsg = "IsLoginNameExists: Invalid parameter loginName";
                    output.obj = userOrganizationExists;
                    output.Status = errorStatus;
                    return Ok(output);
                }

                IManageUserLogin userLoginLogic = _manageUserLogin ?? new ManageUserLogin(userClaim);
                userOrganizationExists = userLoginLogic.IsLoginNameExists(loginName, OrganizationRealPageId, userRealPageId.Value, firstName, lastName, userType, isFromExport);

                output.Status = errorStatus;
                output.obj = userOrganizationExists;
                return Ok(output);
            });
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Update user product status
        /// </summary>
        /// <param name="userLogins"></param>
        /// <param name="userLoginStatusType"></param>
        /// <param name="userClaim"></param>
        /// <returns></returns>
        private IRepositoryResponse UpdateUserProductStatus(IList<UserLoginOnly> userLogins, UserUiStatusType? userLoginStatusType, DefaultUserClaim userClaim)
        {
            IManageUser manageUser = new ManageUser(userClaim);

            return manageUser.UpdateUserStatus(userClaim.UserRealPageGuid, userClaim.PersonaId, userLogins, userLoginStatusType);
        }

        private bool ValidateBulkUpdateStatus(UserUiStatusType userLoginStatusType)
        {
            bool result = false;
            if (userLoginStatusType == UserUiStatusType.Active || userLoginStatusType == UserUiStatusType.Disabled ||
                userLoginStatusType == UserUiStatusType.Locked || userLoginStatusType == UserUiStatusType.Unlocked)
            {
                result = true;
            }

            return result;
        }
        #endregion
    }
}
