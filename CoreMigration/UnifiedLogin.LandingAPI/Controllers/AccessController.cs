using UnifiedLogin.BusinessLogic.Logic.Security;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Controller to provide necessary api's for access rights
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class AccessController : ControllerBase
    {
        #region Ctor
        /// <summary>
        /// 
        /// </summary>
        public AccessController() { }
            
        #endregion

        /// <summary>
        /// List of right a user can have per page
        /// </summary>
        /// <param name="routeId"></param>
        /// <returns></returns>
        [HttpGet("{routeId}/rights")]
        [ProducesResponseType(typeof(RouteSecurity), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public ActionResult GetRights(string routeId)
        {
            var userClaim = new DefaultUserClaim(HttpContext.User);

            if (string.IsNullOrWhiteSpace(routeId))
            {
                return BadRequest("Invalid routeId");
            }

            IManageSecurity _manangeSecurityLogic = new ManageSecurity(userClaim);
            var output = _manangeSecurityLogic.GetPersonaRightsAndActionsByRoute(userClaim.PersonaId, routeId);
            return Ok(output);
        }

    }
}
