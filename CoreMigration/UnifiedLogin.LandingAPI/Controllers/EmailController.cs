using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Email controller
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class EmailController : BaseController
    {
        private readonly IManageEmail _manageEmail;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public EmailController(IManageEmail manageEmail, IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
            _manageEmail = manageEmail ?? throw new ArgumentNullException(nameof(manageEmail));
        }

        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="sendGridEmail">SendGridEmail object of the parameter values</param>
        /// <returns>Response with Success Message</returns>
        [HttpPost("sendemail")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendEmail([FromBody] SendGridEmail sendGridEmail)
        {
            if (sendGridEmail == null)
            {
                return BadRequest("Null parameter: sendGridEmail.");
            }

            var result = await Task.Run(() => _manageEmail.SendGridEmail(sendGridEmail));

            return Ok(result);
        }
    }
}
