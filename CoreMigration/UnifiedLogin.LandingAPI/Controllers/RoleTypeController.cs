using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// RoleType Controller to hold all RoleType management related APIs
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class RoleTypeController : BaseController
    {
        private readonly IManageRoleTypeAsync _manageRoleTypeAsync;
        private readonly IManagePersonaAsync _managePersonaAsync;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public RoleTypeController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageRoleTypeAsync manageRoleTypeAsync,
            IManagePersonaAsync managePersonaAsync) : base(userClaimsAccessor)
        {
            _manageRoleTypeAsync = manageRoleTypeAsync;
            _managePersonaAsync = managePersonaAsync;
        }

        /// <summary>
        /// List Role type details
        /// </summary>
        /// <param name="roleTypeName">RoleType Name</param>
        /// <param name="loginName">Optional User LoginName</param>
        /// <param name="includeRelationShips">Include relationship types</param>
        /// <returns>A list of Role type details</returns>
        [HttpGet("roletypes")]
        [ProducesResponseType(typeof(ObjectListOutput<RoleType, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListRoleType(string roleTypeName = null, string loginName = null, bool includeRelationShips = false, CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();

            Persona persona = null;
            if (userClaim.OrganizationPartyId != 0 && roleTypeName != null && roleTypeName.Equals("User Role", StringComparison.OrdinalIgnoreCase))
            {
                persona = await _managePersonaAsync.GetPersonaAsync(userClaim.PersonaId, false, cancellationToken);
                if (persona == null)
                    return BadRequest("editorPersonaId not found.");
            }

            var roleTypeList = await _manageRoleTypeAsync.ListRoleTypeAsync(roleTypeName, loginName, includeRelationShips, persona, userClaim, cancellationToken);

            if (roleTypeList == null)
                return NoContent();

            var output = new ObjectListOutput<RoleType, IErrorData>() { list = roleTypeList };
            return Ok(output);
        }
    }
}
