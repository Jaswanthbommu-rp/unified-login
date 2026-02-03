using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// User Notification Controller to hold all user notification related APIs
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class UserNotificationController : BaseController
    {
        #region Private variables
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userClaimsAccessor">User claims accessor</param>
        public UserNotificationController(IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
        }
        #endregion

        /// <summary>
        /// Send Welcome Email Invitations
        /// </summary>
        /// <param name="userLogins">Array of user realpage Ids</param>
        /// <returns>List of patched UserLogins</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPost("usernotification/sendwelcomeemail")]
        [AllowAnonymous]
        public async Task<IActionResult> SendWelcomeEmail([FromBody] IList<UserLogin> userLogins)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                ObjectListOutput<UserLogin, IErrorData> output = new ObjectListOutput<UserLogin, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                bool response = true;
                IManageUserLogin manageUserLogin = new ManageUserLogin();

                if (userLogins.Count > 0)
                {
                    ManageUserLogin userLoginLogic = new ManageUserLogin(userClaim);
                    response = userLoginLogic.ResendInvitation(userLogins, true);

                    if (response)
                    {
                        return Ok(response);
                    }

                    return StatusCode((int)HttpStatusCode.ExpectationFailed, response);
                }
                return Ok(response);
            });
        }
    }
}
