using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
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
        private readonly IManagePersona _managePersona;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userClaimsAccessor">Accessor for current authenticated user's claims</param>
        /// <param name="manageProductRPDocumentManagement">Service for managing RPDM operations</param>
        /// <param name="managePersona">Service for managing persona operations</param>
        public ProductRPDMController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageProductRPDocumentManagement manageProductRPDocumentManagement,
            IManagePersona managePersona) : base(userClaimsAccessor)
        {
            _manageProductRPDocumentManagement = manageProductRPDocumentManagement ?? throw new ArgumentNullException(nameof(manageProductRPDocumentManagement));
            _managePersona = new ManagePersona(_userClaimsAccessor.GetUserClaim());
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
        public async Task<IActionResult> GetRoles(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var result = await Task.Run(() =>
                _manageProductRPDocumentManagement.GetRoles(editorPersonaId, userPersonaId, datafilter));

            if (result.IsError)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }

            return Ok(result);
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
        public async Task<IActionResult> GetRoleClassifierDataset(long editorPersonaId, long userPersonaId, string roleId, [FromQuery] RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var result = await Task.Run(() =>
                _manageProductRPDocumentManagement.GetRoleClassifierDataset(editorPersonaId, userPersonaId, roleId, datafilter));

            if (result.IsError)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
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
        public async Task<IActionResult> GetDomain(long personaId)
        {
            if (personaId == 0)
            {
                return BadRequest("personaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var result = await Task.Run(() =>
                _manageProductRPDocumentManagement.GetDomain(personaId));

            if (result.IsError)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
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
        public async Task<IActionResult> UpdateRPDUserStatus([FromBody] ProductUser productUser)
        {
            var personaId = _userClaimsAccessor.PersonaId;

            var result = await Task.Run(() =>
                _manageProductRPDocumentManagement.UnassignUser(personaId, 0, productUser.UserId));

            if (!string.IsNullOrEmpty(result))
            {
                return BadRequest("Deactivate DocumentDirectory product user failed.");
            }

            return Ok("Successfully disabled product user.");
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
        public async Task<IActionResult> ListRPDMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            var result = await Task.Run<object>(() =>
            {
                var persona = _managePersona.GetPersona(editorPersonaId);
                if (persona == null)
                {
                    return new { IsError = true, ErrorMessage = "editorPersonaId not found." };
                }

                var userClaim = _userClaimsAccessor.GetUserClaim();
                userClaim.UserRealPageGuid = persona.RealPageId;

                var manageRPDocument = new UnifiedLogin.BusinessLogic.Logic.Product.ManageProductRPDocumentManagement(userClaim);
                return (object)manageRPDocument.GetMigrationUsers(editorPersonaId, datafilter);
            });

            var resultType = result.GetType();
            if (resultType.GetProperty("IsError")?.GetValue(result) as bool? == true)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

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
        public async Task<IActionResult> UpdateUsersMigrationStatus([FromBody] IList<MigrateUser> migrateUsers)
        {
            var personaId = _userClaimsAccessor.PersonaId;

            var result = await Task.Run(() =>
                _manageProductRPDocumentManagement.UpdateUsersMigrationStatus(personaId, migrateUsers));

            return Ok(result);
        }

        #endregion
    }
}
