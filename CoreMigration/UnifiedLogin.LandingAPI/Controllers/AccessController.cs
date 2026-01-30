using UnifiedLogin.BusinessLogic.Logic.Security;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Net;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Controller to provide necessary api's for access rights.
    /// Refactored to use dependency injection for user claims instead of manual instantiation.
    /// </summary>
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    //[Route("v{version:apiVersion}/[controller]")]
    public class AccessController : ControllerBase
    {
        private readonly IManageSecurity _manageSecurityLogic;
        private readonly IUserClaimsAccessor _userClaimsAccessor;

        #region Ctor
        /// <summary>
        /// Constructor with dependency injection for security logic and user claims accessor.
        /// This follows modern ASP.NET Core patterns for testable, maintainable code.
        /// </summary>
        /// <param name="manageSecurity">Service for managing security rights and actions</param>
        /// <param name="userClaimsAccessor">Accessor for current authenticated user's claims</param>
        public AccessController(IManageSecurity manageSecurity, IUserClaimsAccessor userClaimsAccessor)
        {
            _manageSecurityLogic = manageSecurity ?? throw new ArgumentNullException(nameof(manageSecurity));
            _userClaimsAccessor = userClaimsAccessor ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
        }

        #endregion

        /// <summary>
        /// Gets the list of rights a user can have per page/route.
        /// </summary>
        /// <param name="routeId">The route identifier to check permissions for</param>
        /// <returns>Route security information including rights and actions</returns>
        [HttpGet("{routeId}/rights")]
        [ProducesResponseType(typeof(RouteSecurity), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public ActionResult GetRights(string routeId)
        {
            if (string.IsNullOrWhiteSpace(routeId))
            {
                return BadRequest("Invalid routeId");
            }

            // Use the injected user claims accessor instead of creating DefaultUserClaim manually
            var personaId = _userClaimsAccessor.PersonaId;
            var output = _manageSecurityLogic.GetPersonaRightsAndActionsByRoute(personaId, routeId);
            return Ok(output);
        }

    }
}
