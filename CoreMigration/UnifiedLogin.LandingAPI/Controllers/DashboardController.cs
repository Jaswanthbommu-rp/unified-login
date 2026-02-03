using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// APIs for Landing App Dashboard
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly IManagePersona _managePersona;
        private readonly IManageDashboardContent _manageDashboardContent;
        private readonly IManageCredential _manageCredential;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public DashboardController(
            IManagePersona managePersona,
            IManageDashboardContent manageDashboardContent,
            IManageCredential manageCredential,
            IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _manageDashboardContent = manageDashboardContent ?? throw new ArgumentNullException(nameof(manageDashboardContent));
            _manageCredential = manageCredential ?? throw new ArgumentNullException(nameof(manageCredential));
        }

        /// <summary>
        /// Get dashboard content for the logged in user with its active persona
        /// </summary>
        /// <remarks>Get dashboard content for the logged in persona</remarks>
        /// <returns>Information about the user persona's profile and assigned products</returns>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(DashboardElementResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDashboardContent()
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            if (userClaim == null)
            {
                return Unauthorized();
            }

            var realPageUserId = userClaim.UserRealPageGuid;

            var persona = await Task.Run(() => _managePersona.GetPersona(userClaim.PersonaId));

            if (persona == null)
            {
                return BadRequest("Persona not found");
            }

            var dashboardElementResponse = await Task.Run(() =>
                _manageDashboardContent.GetDashboardElementResponse(realPageUserId, persona));

            var checkPasswordExpirationResponse = await Task.Run(() =>
                _manageCredential.CheckPasswordExpiration(userClaim.UserId, userClaim.UserRealPageGuid));

            if (checkPasswordExpirationResponse != null && checkPasswordExpirationResponse.IsPasswordExpired)
            {
                dashboardElementResponse.DashboardElements.Resources = null;
                dashboardElementResponse.DashboardElements.ProfileDetail.AssignedProducts = null;
            }

            return Ok(dashboardElementResponse);
        }
    }
}
