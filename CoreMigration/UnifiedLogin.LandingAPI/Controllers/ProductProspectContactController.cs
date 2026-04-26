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
    /// ProductProspectContact Controller for property and user management
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class ProductProspectContactController : BaseController
    {
        private readonly IManageProductProspectContactAsync _manageProductProspectContactAsync;
        private readonly IManagePersonaAsync _managePersonaAsync;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProductProspectContactController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageProductProspectContactAsync manageProductProspectContactAsync,
            IManagePersonaAsync managePersonaAsync) : base(userClaimsAccessor)
        {
            _manageProductProspectContactAsync = manageProductProspectContactAsync ?? throw new ArgumentNullException(nameof(manageProductProspectContactAsync));
            _managePersonaAsync = managePersonaAsync ?? throw new ArgumentNullException(nameof(managePersonaAsync));
        }

        /// <summary>
        /// Returns Properties
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <param name="datafilter">A datafilter used to filter the properties.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of properties</returns>
        [HttpGet("products/prospectcontactcenter/properties")]
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

            var result = await _manageProductProspectContactAsync.GetPropertiesAsync(editorPersonaId, userPersonaId, datafilter, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Disable the prospect contact center user.
        /// </summary>
        /// <param name="productUser">The product user.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success or error message</returns>
        [HttpPut("products/prospectcontactcenter/user/MT/status")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProspectContactCenterUserStatus(ProductUser productUser, CancellationToken cancellationToken = default)
        {
            if (!await _manageProductProspectContactAsync.ChangeUserStatusAsync(
                _userClaimsAccessor.PersonaId, productUser.UserId, cancellationToken))
            {
                return BadRequest("Deactivate prospectcontactcenter user failed.");
            }

            return Ok("Successfully disabled product user.");
        }

        #region Migration API

        /// <summary>
        /// Returns product users of an organization for given user.
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="datafilter">Data filter for pagination and filtering</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of Prospect Contact migration users</returns>
        [HttpGet("products/prospectcontactcenter/migration-users")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListProspectContactMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            var persona = await _managePersonaAsync.GetPersonaAsync(editorPersonaId, false, cancellationToken);
            if (persona == null)
                return BadRequest("editorPersonaId not found.");

            var result = await _manageProductProspectContactAsync.GetMigrationUsersAsync(editorPersonaId, datafilter, cancellationToken);

            if (!result.IsError)
                return Ok(result);

            return StatusCode(StatusCodes.Status403Forbidden, result);
        }

        /// <summary>
        /// Update migration status of users.
        /// </summary>
        /// <param name="migrateUsers">List of users to mark as migrated</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Migration status result</returns>
        [HttpPut("products/prospectcontactcenter/migrate-users")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUsersMigrationStatus(IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        {
            var result = await _manageProductProspectContactAsync.UpdateUsersMigrationStatusAsync(
                _userClaimsAccessor.PersonaId, migrateUsers, cancellationToken);

            return Ok(result);
        }

        #endregion
    }
}
