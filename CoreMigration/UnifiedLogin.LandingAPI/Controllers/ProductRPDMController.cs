using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Product RPDM (RealPage Document Management) Controller
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("products/rpdm")]
    public class ProductRPDMController : BaseController
    {
        private readonly IManageProductRPDocumentManagement _manageProductRPDocumentManagement;
        private readonly IManagePersonaAsync _managePersonaAsync;
        private readonly IManageProductRPDocumentManagementAsync _manageProductRPDocumentManagementAsync;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userClaimsAccessor">Accessor for current authenticated user's claims</param>
        /// <param name="manageProductRPDocumentManagement">Service for managing RPDM operations</param>
        /// <param name="managePersonaAsync">Async service for managing persona operations</param>
        /// <param name="manageProductRPDocumentManagementAsync">Async service for RPDM migration operations</param>
        public ProductRPDMController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageProductRPDocumentManagement manageProductRPDocumentManagement,
            IManagePersonaAsync managePersonaAsync,
            IManageProductRPDocumentManagementAsync manageProductRPDocumentManagementAsync) : base(userClaimsAccessor)
        {
            _manageProductRPDocumentManagement = manageProductRPDocumentManagement ?? throw new ArgumentNullException(nameof(manageProductRPDocumentManagement));
            _managePersonaAsync = managePersonaAsync ?? throw new ArgumentNullException(nameof(managePersonaAsync));
            _manageProductRPDocumentManagementAsync = manageProductRPDocumentManagementAsync ?? throw new ArgumentNullException(nameof(manageProductRPDocumentManagementAsync));
        }

        /// <summary>
        /// Used to get a list of roles
        /// </summary>
        /// <param name="editorPersonaId">The user who is adding/editing the user</param>
        /// <param name="userPersonaId">The user being added/edited</param>
        /// <param name="datafilter">A datafilter used to filter the roles</param>
        /// <returns>A list of product roles</returns>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public Task<IActionResult> GetRoles(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return Task.FromResult<IActionResult>(BadRequest("editorPersonaId not supplied."));

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                return Task.FromResult<IActionResult>(BadRequest("RealPageId empty."));

            var result = _manageProductRPDocumentManagement.GetRoles(editorPersonaId, userPersonaId, datafilter);

            if (result.IsError)
                return Task.FromResult<IActionResult>(StatusCode((int)HttpStatusCode.InternalServerError, result));

            return Task.FromResult<IActionResult>(Ok(result));
        }

        /// <summary>
        /// Used to get a list of additional information related to the role id requested
        /// </summary>
        /// <param name="editorPersonaId">The user who is adding/editing the user</param>
        /// <param name="userPersonaId">The user being added/edited</param>
        /// <param name="roleId">The id of the role to get information for</param>
        /// <param name="datafilter">A datafilter used to filter the roles</param>
        /// <returns>A list of items associated to the role given</returns>
        [HttpGet("role/classifier")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public Task<IActionResult> GetRoleClassifierDataset(long editorPersonaId, long userPersonaId, string roleId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return Task.FromResult<IActionResult>(BadRequest("editorPersonaId not supplied."));

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                return Task.FromResult<IActionResult>(BadRequest("RealPageId empty."));

            var result = _manageProductRPDocumentManagement.GetRoleClassifierDataset(editorPersonaId, userPersonaId, roleId, datafilter);

            if (result.IsError)
                return Task.FromResult<IActionResult>(StatusCode((int)HttpStatusCode.Forbidden, result));

            return Task.FromResult<IActionResult>(Ok(result));
        }

        /// <summary>
        /// Used to get the domain for the given user id
        /// </summary>
        /// <param name="personaId">The user to get the domain for</param>
        /// <returns>The domain for Doc Management for the given persona</returns>
        [HttpGet("domain")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public Task<IActionResult> GetDomain(long personaId, CancellationToken cancellationToken = default)
        {
            if (personaId == 0)
                return Task.FromResult<IActionResult>(BadRequest("personaId not supplied."));

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                return Task.FromResult<IActionResult>(BadRequest("RealPageId empty."));

            var result = _manageProductRPDocumentManagement.GetDomain(personaId);

            if (result.IsError)
                return Task.FromResult<IActionResult>(StatusCode((int)HttpStatusCode.Forbidden, result));

            return Task.FromResult<IActionResult>(Ok(result));
        }

        #region User-Status

        /// <summary>
        /// Disable the Document Directory user
        /// </summary>
        /// <param name="productUser">Product User</param>
        /// <returns>Status update result</returns>
        [HttpPut("user/MT/status")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public Task<IActionResult> UpdateRPDUserStatus([FromBody] ProductUser productUser, CancellationToken cancellationToken = default)
        {
            var personaId = _userClaimsAccessor.PersonaId;
            var result = _manageProductRPDocumentManagement.UnassignUser(personaId, 0, productUser.UserId);

            if (!string.IsNullOrEmpty(result))
                return Task.FromResult<IActionResult>(BadRequest("Deactivate DocumentDirectory product user failed."));

            return Task.FromResult<IActionResult>(Ok("Successfully disabled product user."));
        }

        #endregion

        #region Migration API

        /// <summary>
        /// Returns product users of an organization for given user
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="datafilter">A datafilter used to filter the users</param>
        /// <returns>List of Document Directory migration users</returns>
        [HttpGet("migration-users")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListRPDMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            var persona = await _managePersonaAsync.GetPersonaAsync(editorPersonaId, false, cancellationToken);
            if (persona == null)
                return BadRequest("editorPersonaId not found.");

            var userClaim = _userClaimsAccessor.GetUserClaim();
            userClaim.UserRealPageGuid = persona.RealPageId;

            var result = await _manageProductRPDocumentManagementAsync.GetMigrationUsersAsync(userClaim, editorPersonaId, datafilter, cancellationToken);
            if (result.IsError)
                return StatusCode((int)HttpStatusCode.Forbidden, result);

            return Ok(result);
        }

        /// <summary>
        /// Update migration status of users
        /// </summary>
        /// <param name="migrateUsers">List of users to mark as migrated</param>
        /// <returns>Update result</returns>
        [HttpPut("migrate-users")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public Task<IActionResult> UpdateUsersMigrationStatus([FromBody] IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        {
            var personaId = _userClaimsAccessor.PersonaId;
            var result = _manageProductRPDocumentManagement.UpdateUsersMigrationStatus(personaId, migrateUsers);
            return Task.FromResult<IActionResult>(Ok(result));
        }

        #endregion
    }
}
