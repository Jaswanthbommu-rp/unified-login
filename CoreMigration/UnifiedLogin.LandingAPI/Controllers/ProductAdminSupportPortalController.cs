using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Product Admin Support Portal Controller - Manages Admin Support Portal product-specific operations
    /// including roles, properties, and migration functionality
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("")]
    public class ProductAdminSupportPortalController : BaseController
    {
        private readonly IManageProductAdminSupportPortalAsync _manageProductAdminSupportPortal;
        private readonly IManagePersonaAsync _managePersona;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProductAdminSupportPortalController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageProductAdminSupportPortalAsync manageProductAdminSupportPortal,
            IManagePersonaAsync managePersona) : base(userClaimsAccessor)
        {
            _manageProductAdminSupportPortal = manageProductAdminSupportPortal ?? throw new ArgumentNullException(nameof(manageProductAdminSupportPortal));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
        }

        /// <summary>
        /// Returns Roles
        /// </summary>
        [HttpGet("products/clientportal/roles")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRoles(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var result = await _manageProductAdminSupportPortal.GetRolesAsync(editorPersonaId, userPersonaId, datafilter, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Returns Properties
        /// </summary>
        [HttpGet("products/clientportal/properties")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProperties(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var result = await _manageProductAdminSupportPortal.GetPropertiesAsync(editorPersonaId, userPersonaId, datafilter, cancellationToken);
            return Ok(result);
        }

        #region Migration API

        /// <summary>
        /// List Client portal users
        /// </summary>
        [HttpGet("products/clientportal_v1/migration-users")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListClientPortalMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            var persona = await _managePersona.GetPersonaAsync(editorPersonaId, withRights: false, cancellationToken);
            if (persona == null)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new { IsError = true, ErrorMessage = "editorPersonaId not found." });
            }

            var result = await _manageProductAdminSupportPortal.GetMigrationUsersAsync(editorPersonaId, datafilter, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Update migration Client portal users
        /// </summary>
        [HttpPut("products/clientportal_v1/migrate-users")]
        [ProducesResponseType(typeof(MigrateResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateUsersMigrationStatus([FromBody] IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        {
            var personaId = _userClaimsAccessor.PersonaId;
            var result = await _manageProductAdminSupportPortal.UpdateUsersMigrationStatusAsync(personaId, migrateUsers, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Disables the Client Portal product user.
        /// </summary>
        [HttpPut("products/clientportal_v1/user/MT/status")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateClientPortalUserStatus([FromBody] ProductUser productUser, CancellationToken cancellationToken = default)
        {
            var personaId = _userClaimsAccessor.PersonaId;
            var result = await _manageProductAdminSupportPortal.ChangeUserStatusAsync(personaId, productUser.UserLogin, cancellationToken);

            if (!result)
            {
                return BadRequest("Disabling Client Portal user failed.");
            }

            return Ok("Successfully disabled product user.");
        }

        #endregion
    }
}
