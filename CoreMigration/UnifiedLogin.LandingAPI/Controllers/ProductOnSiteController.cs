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
    /// Product OnSite Controller - Manages OnSite product-specific operations
    /// including roles, properties, regions, and migration functionality
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("products/onsite")]
    public class ProductOnSiteController : BaseController
    {
        private readonly IManageProductOneSiteAsync _manageProductOnSiteAsync;
        private readonly IManagePersonaAsync _managePersonaAsync;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userClaimsAccessor">Accessor for current authenticated user's claims</param>
        /// <param name="manageProductOnSiteAsync">Async service for OnSite product operations</param>
        /// <param name="managePersonaAsync">Async service for managing persona operations</param>
        public ProductOnSiteController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageProductOneSiteAsync manageProductOnSiteAsync,
            IManagePersonaAsync managePersonaAsync) : base(userClaimsAccessor)
        {
            _manageProductOnSiteAsync = manageProductOnSiteAsync ?? throw new ArgumentNullException(nameof(manageProductOnSiteAsync));
            _managePersonaAsync = managePersonaAsync ?? throw new ArgumentNullException(nameof(managePersonaAsync));
        }

        /// <summary>
        /// Returns Roles (User Access Groups in OnSite)
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">User persona ID who is being created or edited</param>
        /// <param name="datafilter">A datafilter used to filter the roles</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of OnSite roles</returns>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
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

            var userClaim = _userClaimsAccessor.GetUserClaim();
            var result = await _manageProductOnSiteAsync.GetRolesAsync(editorPersonaId, userPersonaId, datafilter, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Returns Properties
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">User persona ID who is being created or edited</param>
        /// <param name="datafilter">A datafilter used to filter the properties</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of OnSite properties</returns>
        [HttpGet("properties")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProperties(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            var userClaim = _userClaimsAccessor.GetUserClaim();
            if (userClaim == null || userClaim.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var result = await _manageProductOnSiteAsync.GetPropertiesAsync(editorPersonaId, userPersonaId, datafilter, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Returns Regions
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">User persona ID who is being created or edited</param>
        /// <param name="datafilter">A datafilter used to filter the regions</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of OnSite regions</returns>
        [HttpGet("regions")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRegions(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var userClaim = _userClaimsAccessor.GetUserClaim();
            var result = await _manageProductOnSiteAsync.GetRegionsAsync(editorPersonaId, userPersonaId, datafilter, cancellationToken);

            return Ok(result);
        }

        #region Migration API

        /// <summary>
        /// Returns product users of an organization for migration purposes
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="datafilter">A datafilter used to filter the users</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of OnSite migration users</returns>
        [HttpGet("migration-users")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListOnSiteMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            var persona = await _managePersonaAsync.GetPersonaAsync(editorPersonaId, false, cancellationToken);
            if (persona == null)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new { IsError = true, ErrorMessage = "editorPersonaId not found." });
            }

            var userClaim = _userClaimsAccessor.GetUserClaim();
            userClaim.UserRealPageGuid = persona.RealPageId;

            var result = await _manageProductOnSiteAsync.GetMigrationUsersAsync(editorPersonaId, datafilter, cancellationToken);

            if (result.IsError)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Update migration status of users
        /// </summary>
        /// <param name="migrateUsers">List of users to mark as migrated</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Update result</returns>
        [HttpPut("migrate-users")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateUsersMigrationStatus([FromBody] IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var personaId = _userClaimsAccessor.PersonaId;
            var result = await _manageProductOnSiteAsync.UpdateUsersMigrationStatusAsync(personaId, migrateUsers, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Updates the OnSite user status (enable/disable)
        /// </summary>
        /// <param name="productUser">The product user</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Status update result</returns>
        [HttpPut("user/MT/status")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateOnSiteUserStatus([FromBody] ProductUser productUser, CancellationToken cancellationToken = default)
        {
            var personaId = _userClaimsAccessor.PersonaId;
            var result = await _manageProductOnSiteAsync.ChangeUserStatusAsync(personaId, productUser.UserId.ToString(), cancellationToken);

            if (!result)
            {
                return BadRequest("Disabling on-site user failed.");
            }

            return Ok("Successfully disabled product user.");
        }

        #endregion
    }
}
