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
    /// ProductRum Controller for Resident Utility Management
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class ProductRumController : BaseController
    {
        private readonly IManageProductRumAsync _manageProductRumAsync;
        private readonly IManagePersonaAsync _managePersonaAsync;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProductRumController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageProductRumAsync manageProductRumAsync,
            IManagePersonaAsync managePersonaAsync) : base(userClaimsAccessor)
        {
            _manageProductRumAsync = manageProductRumAsync;
            _managePersonaAsync = managePersonaAsync;
        }

        /// <summary>
        /// Returns Roles (User Access Groups in On Site)
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <param name="datafilter">A datafilter used to filter the roles.</param>
        /// <returns>List of roles</returns>
        [HttpGet("products/rum/roles")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRoles(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                return BadRequest("RealPageId empty.");

            var result = await _manageProductRumAsync.GetRolesAsync(editorPersonaId, userPersonaId, datafilter, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Returns Properties
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <param name="datafilter">A datafilter used to filter the properties.</param>
        /// <returns>List of properties</returns>
        [HttpGet("products/rum/properties")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProperties(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                return BadRequest("RealPageId empty.");

            var result = await _manageProductRumAsync.GetPropertiesAsync(editorPersonaId, userPersonaId, datafilter, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Returns Regions
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <param name="datafilter">A datafilter used to filter the regions.</param>
        /// <returns>List of regions</returns>
        [HttpGet("products/rum/regions")]
        [Obsolete("This endpoint is obsolete")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRegionss(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                return BadRequest("RealPageId empty.");

            var result = await _manageProductRumAsync.GetRegionsAsync(editorPersonaId, userPersonaId, datafilter, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Returns Property Groups
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <param name="datafilter">A datafilter used to filter the property groups.</param>
        /// <returns>List of property groups</returns>
        [HttpGet("products/rum/propertygroups")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPropertyGroups(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                return BadRequest("RealPageId empty.");

            var result = await _manageProductRumAsync.GetPropertyGroupsAsync(editorPersonaId, userPersonaId, datafilter, cancellationToken);
            return Ok(result);
        }

        #region Migration API

        /// <summary>
        /// Returns product users of an organization for given user.
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="datafilter">Data filter for pagination and filtering</param>
        /// <returns>List of RUM migration users</returns>
        [HttpGet("products/rum/migration-users")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListRUMMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            var persona = await _managePersonaAsync.GetPersonaAsync(editorPersonaId, false, cancellationToken);
            if (persona == null)
                return BadRequest("editorPersonaId not found.");

            var result = await _manageProductRumAsync.GetMigrationUsersAsync(editorPersonaId, datafilter, cancellationToken);
            if (!result.IsError)
                return Ok(result);
            else
                return StatusCode(StatusCodes.Status403Forbidden, result);
        }

        /// <summary>
        /// Update migration status of users.
        /// </summary>
        /// <param name="migratedUsers">List of users to mark as migrated</param>
        /// <returns>Migration status result</returns>
        [HttpPut("products/rum/migrate-users")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUsersMigrationStatus(List<MigrateUser> migratedUsers, CancellationToken cancellationToken = default)
        {
            var result = await _manageProductRumAsync.UpdateUsersMigrationStatusAsync(
                _userClaimsAccessor.PersonaId, migratedUsers, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Disables the Utility Management user.
        /// </summary>
        /// <param name="productUser">The product user.</param>
        /// <returns>Success or error message</returns>
        [HttpPut("products/rum/user/MT/status")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateRUMUserStatus(ProductUser productUser, CancellationToken cancellationToken = default)
        {
            if (!await _manageProductRumAsync.ChangeUserStatusAsync(
                _userClaimsAccessor.PersonaId, productUser.UserId.ToString(), cancellationToken))
                return BadRequest("Disabling Utility Management user failed.");

            return Ok("Successfully disabled product user.");
        }

        #endregion
    }
}
