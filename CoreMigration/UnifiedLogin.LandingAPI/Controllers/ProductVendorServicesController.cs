using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Vendor Credentialing Controller
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class ProductVendorServicesController : BaseController
    {
        private readonly IManageProductVendorServicesAsync _manageProductVendorServicesAsync;
        private readonly IManagePersonaAsync _managePersonaAsync;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProductVendorServicesController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageProductVendorServicesAsync manageProductVendorServicesAsync,
            IManagePersonaAsync managePersonaAsync) : base(userClaimsAccessor)
        {
            _manageProductVendorServicesAsync = manageProductVendorServicesAsync;
            _managePersonaAsync = managePersonaAsync;
        }

        /// <summary>
        /// Returns Divisions, Regions and ownership Groups
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">User persona id</param>
        /// <param name="datafilter">A datafilter used to filter the roles.</param>
        /// <returns>List of property groups</returns>
        [HttpGet("products/vendorcompliance/propertygroups")]
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

            var result = await _manageProductVendorServicesAsync.GetPropertyGroupsAsync(editorPersonaId, userPersonaId, datafilter, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Returns Roles (User Access Groups in Vendor Credentialing)
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <param name="accessType">Access type - Property or Client</param>
        /// <param name="datafilter">A datafilter used to filter the roles.</param>
        /// <returns>List of roles</returns>
        [HttpGet("products/vendorcompliance/roles")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRoles(long editorPersonaId, long userPersonaId, AccessType accessType, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                return BadRequest("RealPageId empty.");

            var result = await _manageProductVendorServicesAsync.GetRolesAsync(editorPersonaId, userPersonaId, accessType, datafilter, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Returns Properties
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <param name="datafilter">A datafilter used to filter the properties.</param>
        /// <returns>List of properties</returns>
        [HttpGet("products/vendorcompliance/properties")]
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

            var result = await _manageProductVendorServicesAsync.GetPropertiesAsync(editorPersonaId, userPersonaId, datafilter, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get Notification Settings
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <returns>Notification settings</returns>
        [HttpGet("products/vendorcompliance/notifications")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNotification(long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                return BadRequest("RealPageId empty.");

            var result = await _manageProductVendorServicesAsync.GetNotificationSettingsAsync(editorPersonaId, userPersonaId, cancellationToken);
            return Ok(result);
        }

        #region User-Status

        /// <summary>
        /// Disable vendor compliance user
        /// </summary>
        /// <param name="productUser">Product user details</param>
        /// <returns>Success or error message</returns>
        [HttpPut("products/vendorcompliance/user/MT/status")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateVendorComplianceUserStatus([FromBody] ProductUser productUser, CancellationToken cancellationToken = default)
        {
            var success = await _manageProductVendorServicesAsync.ChangeUserStatusAsync(
                _userClaimsAccessor.PersonaId, productUser.UserName, productUser.UserId.ToString(), cancellationToken: cancellationToken);
            if (!success)
                return BadRequest("Deactivate VendorCompliance user failed.");

            return Ok("Successfully disabled product user.");
        }

        #endregion

        #region Migration API

        /// <summary>
        /// Returns product users of an organization for given user.
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="datafilter">Data filter for pagination and filtering</param>
        /// <returns>List of Vendor Service migration users</returns>
        [HttpGet("products/vendorcompliance/migration-users")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListVendorServiceMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            var persona = await _managePersonaAsync.GetPersonaAsync(editorPersonaId, false, cancellationToken);
            if (persona == null)
                return BadRequest("editorPersonaId not found.");

            var result = await _manageProductVendorServicesAsync.GetMigrationUsersAsync(editorPersonaId, datafilter, cancellationToken);
            if (!result.IsError)
                return Ok(result);
            else
                return StatusCode(StatusCodes.Status403Forbidden, result);
        }

        /// <summary>
        /// Update migration status of users.
        /// </summary>
        /// <param name="migrateUsers">List of users to mark as migrated</param>
        /// <returns>Migration status result</returns>
        [HttpPut("products/vendorcompliance/migrate-users")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUsersMigrationStatus([FromBody] IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        {
            var result = await _manageProductVendorServicesAsync.UpdateUsersMigrationStatusAsync(
                _userClaimsAccessor.PersonaId, migrateUsers, cancellationToken);
            return Ok(result);
        }

        #endregion
    }
}
