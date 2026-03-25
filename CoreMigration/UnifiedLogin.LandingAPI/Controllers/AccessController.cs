using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Controller to provide necessary api's for access rights.
    /// </summary>
    [Authorize]
    [Route("")]
    [ApiController]
    public class AccessController : BaseController
    {
        private readonly IManageSecurityAsync _manageSecurityLogic;

        /// <summary>
        /// Constructor with dependency injection for security logic and user claims accessor.
        /// </summary>
        public AccessController(IManageSecurityAsync manageSecurity, IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
            _manageSecurityLogic = manageSecurity ?? throw new ArgumentNullException(nameof(manageSecurity));
        }

        /// <summary>
        /// Gets the list of rights a user can have per page/route.
        /// </summary>
        /// <param name="routeId">The route identifier to check permissions for</param>
        /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
        /// <returns>Route security information including rights and actions</returns>
        [HttpGet("{routeId}/rights")]
        [ProducesResponseType(typeof(RouteSecurity), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRights(string routeId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(routeId))
                return BadRequest("Invalid routeId");

            var personaId = _userClaimsAccessor.PersonaId;
            var output = await _manageSecurityLogic.GetPersonaRightsAndActionsByRouteAsync(personaId, routeId, cancellationToken);
            return Ok(output);
        }
    }
}
