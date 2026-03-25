using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Product Lead2Lease Controller for user and migration management
    /// </summary>
    [ApiController]
    [Route("")]
    [Authorize]
    public class ProductLead2LeaseController : BaseController
    {
        private readonly IManageProductLead2LeaseAsync _manageProductLead2Lease;
        private readonly IManagePersonaAsync _managePersona;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProductLead2LeaseController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageProductLead2LeaseAsync manageProductLead2Lease,
            IManagePersonaAsync managePersona) : base(userClaimsAccessor)
        {
            _manageProductLead2Lease = manageProductLead2Lease ?? throw new ArgumentNullException(nameof(manageProductLead2Lease));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
        }

        /// <summary>
        /// Used to get a list of roles
        /// </summary>
        [HttpGet("products/lead2lease/roles")]
        [ProducesResponseType(typeof(ListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLead2LeaseRoles(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            ListResponse response = await _manageProductLead2Lease.GetRolesAsync(userClaim, editorPersonaId, userPersonaId, datafilter, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Used to get a list of properties
        /// </summary>
        [HttpGet("products/lead2lease/properties")]
        [ProducesResponseType(typeof(ListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLead2LeaseProperties(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            ListResponse response = await _manageProductLead2Lease.GetPropertiesAsync(userClaim, editorPersonaId, userPersonaId, datafilter, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Update Lead2Lease user status
        /// </summary>
        [HttpPut("products/lead2lease/user/MT/status")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLead2LeaseUserStatus([FromBody] ProductUser productUser, CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            if (userClaim == null)
                return Unauthorized();

            bool success = await _manageProductLead2Lease.ChangeUserStatusAsync(userClaim, userClaim.PersonaId, productUser.UserName, productUser.UserId.ToString(), cancellationToken);
            if (!success)
                return BadRequest("Deactivate Lead2Lease user failed.");

            return Ok("Successfully disabled product user.");
        }

        /// <summary>
        /// Returns product users of an organization for given user
        /// </summary>
        [HttpGet("products/lead2lease/migration-users")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListLead2LeaseMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            var persona = await _managePersona.GetPersonaAsync(editorPersonaId, false, cancellationToken);
            if (persona == null)
                return BadRequest("editorPersonaId not found.");

            var userClaim = _userClaimsAccessor.GetUserClaim();
            userClaim.UserRealPageGuid = persona.RealPageId;

            var result = await _manageProductLead2Lease.GetMigrationUsersAsync(userClaim, editorPersonaId, datafilter, cancellationToken);
            if (!result.IsError)
                return Ok(result);

            return StatusCode(StatusCodes.Status403Forbidden, result);
        }

        /// <summary>
        /// Update migration status of users
        /// </summary>
        [HttpPut("products/lead2lease/migrate-users")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUsersMigrationStatus([FromBody] IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            if (userClaim == null)
                return Unauthorized();

            var result = await _manageProductLead2Lease.UpdateUsersMigrationStatusAsync(userClaim, userClaim.PersonaId, migrateUsers, cancellationToken);
            return Ok(result);
        }
    }
}
