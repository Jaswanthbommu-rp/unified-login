using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.SharedObjects.TwoFactor;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Multi-Factor Authentication Controller
    /// </summary>
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class MultiFactorAuthController : ControllerBase
    {
        private readonly ITwoFactorLogic _twoFactorLogic;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public MultiFactorAuthController(ITwoFactorLogic twoFactorLogic)
        {
            _twoFactorLogic = twoFactorLogic;
        }

        /// <summary>
        /// Used to update information for the AppAuth user
        /// </summary>
        /// <param name="realPageId">The id of the user being edited</param>
        /// <param name="appAuthUser">The settings to add/update</param>
        /// <returns>Success response</returns>
        [HttpPut("multifactorauth/appauth/user/{realPageId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserAppAuth(Guid realPageId, [FromBody] AppAuthUser appAuthUser)
        {
            var result = await Task.Run(() => _twoFactorLogic.UpdateUserTwoFactorStatus(realPageId, appAuthUser.Status));

            if (result == 0)
            {
                return BadRequest("No records updated");
            }

            return NoContent();
        }

        /// <summary>
        /// Used to delete a users App Auth token setup
        /// </summary>
        /// <param name="realPageId">RealPage ID of the user</param>
        /// <returns>Success response</returns>
        [HttpDelete("multifactorauth/appauth/user/{realPageId}/token")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUserAppAuthToken(Guid realPageId)
        {
            var result = await Task.Run(() => _twoFactorLogic.DeleteUserAppAuthToken(realPageId));

            if (result == 0)
            {
                return BadRequest("No records deleted");
            }

            // Make sure to reset authentication two factor state to pending (status = 2)
            result = await Task.Run(() => _twoFactorLogic.UpdateUserTwoFactorStatus(realPageId, 2));

            if (result == 0)
            {
                return BadRequest("No records updated");
            }

            return NoContent();
        }
    }
}
