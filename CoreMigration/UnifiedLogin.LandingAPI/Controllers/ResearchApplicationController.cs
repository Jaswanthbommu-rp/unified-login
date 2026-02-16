using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.Core;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Research Application Controller - Handles research application product operations
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("products/ResearchApplication")]
    public class ResearchApplicationController : BaseController
    {
        private readonly IManageResearchApplication _manageResearchApplication;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userClaimsAccessor">Accessor for current authenticated user's claims</param>
        /// <param name="manageResearchApplication">Service for managing research application operations</param>
        public ResearchApplicationController(IUserClaimsAccessor userClaimsAccessor, IManageResearchApplication manageResearchApplication)
        {
            _manageResearchApplication = manageResearchApplication ?? throw new ArgumentNullException(nameof(manageResearchApplication));
        }

        /// <summary>
        /// Returns Roles by PartyID
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <param name="partyId">Organization PartyID</param>
        /// <param name="datafilter">A datafilter used to filter the roles</param>
        /// <returns>List of roles</returns>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRoles(long editorPersonaId, long userPersonaId, long partyId, [FromQuery] RequestParameter datafilter)
        {
            if (partyId == 0)
            {
                return BadRequest("partyId not supplied.");
            }

            var result = await Task.Run(() =>
                _manageResearchApplication.GetRoles(editorPersonaId, userPersonaId, partyId));

            return Ok(result);
        }

        /// <summary>
        /// Used to get the rights for the given party and role Id
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="partyId">Organization PartyID</param>
        /// <param name="roleId">Role ID</param>
        /// <param name="datafilter">A datafilter used to filter</param>
        /// <returns>List of rights</returns>
        [HttpGet("role/rights")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRightsByRole(long editorPersonaId, long partyId, long roleId, [FromQuery] RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (partyId == 0)
            {
                return BadRequest("partyId not supplied.");
            }

            var result = await Task.Run(() =>
                _manageResearchApplication.GetRightsByRole(editorPersonaId, partyId, roleId));

            return Ok(result);
        }
    }
}
