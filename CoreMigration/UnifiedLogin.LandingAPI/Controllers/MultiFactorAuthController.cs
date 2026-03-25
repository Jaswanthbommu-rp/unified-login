using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.TwoFactor;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Multi-Factor Authentication Controller
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class MultiFactorAuthController : BaseController
    {
        private readonly ITwoFactorLogicAsync _twoFactorLogic;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public MultiFactorAuthController(ITwoFactorLogicAsync twoFactorLogic, IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
            _twoFactorLogic = twoFactorLogic ?? throw new ArgumentNullException(nameof(twoFactorLogic));
        }

        /// <summary>
        /// Used to update information for the AppAuth user
        /// </summary>
        /// <param name="realPageId">The id of the user being edited</param>
        /// <param name="appAuthUser">The settings to add/update</param>
        /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
        /// <returns>Success response</returns>
        [HttpPut("multifactorauth/appauth/user/{realPageId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserAppAuth(Guid realPageId, [FromBody] AppAuthUser appAuthUser, CancellationToken cancellationToken = default)
        {
            var result = await _twoFactorLogic.UpdateUserTwoFactorStatusAsync(realPageId, appAuthUser.Status, cancellationToken);

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
        /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
        /// <returns>Success response</returns>
        [HttpDelete("multifactorauth/appauth/user/{realPageId}/token")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUserAppAuthToken(Guid realPageId, CancellationToken cancellationToken = default)
        {
            var result = await _twoFactorLogic.DeleteUserAppAuthTokenAsync(realPageId, cancellationToken);

            if (result == 0)
            {
                return BadRequest("No records deleted");
            }

            // Reset authentication two factor state to pending (status = 2)
            result = await _twoFactorLogic.UpdateUserTwoFactorStatusAsync(realPageId, 2, cancellationToken);

            if (result == 0)
            {
                return BadRequest("No records updated");
            }

            return NoContent();
        }
    }
}
