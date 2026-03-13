using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
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

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProductProspectContactController(IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
        }

        /// <summary>
        /// Returns Properties
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <param name="datafilter">A datafilter used to filter the properties.</param>
        /// <returns>List of properties</returns>
        [HttpGet("products/prospectcontactcenter/properties")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProperties(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");

                var userClaim = _userClaimsAccessor.GetUserClaim();
                if (userClaim == null || userClaim.UserRealPageGuid == Guid.Empty)
                    return BadRequest("RealPageId empty.");

                IManageProductProspectContact manageProductProspectContact = new ManageProductProspectContact(userClaim);
                var result = manageProductProspectContact.GetProperties(editorPersonaId, userPersonaId, datafilter);

                return Ok(result);
            });
        }

        /// <summary>
        /// Disable the prospect contact center user.
        /// </summary>
        /// <param name="productUser">The product user.</param>
        /// <returns>Success or error message</returns>
        [HttpPut("products/prospectcontactcenter/user/MT/status")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProspectContactCenterUserStatus(ProductUser productUser)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                if (userClaim == null)
                    return Unauthorized();

                var manageProductProspectContact = new ManageProductProspectContact(userClaim);
                if (!manageProductProspectContact.ChangeUserStatus(userClaim.PersonaId, productUser.UserId))
                {
                    return BadRequest("Deactivate prospectcontactcenter user failed.");
                }
                return Ok("Successfully disabled product user.");
            });
        }

        #region Migration API

        /// <summary>
        /// Returns product users of an organization for given user.
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="datafilter">Data filter for pagination and filtering</param>
        /// <returns>List of Prospect Contact migration users</returns>
        [HttpGet("products/prospectcontactcenter/migration-users")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListProspectContactMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");

                ManagePersona managePersona = new ManagePersona(_userClaimsAccessor.GetUserClaim());
                var persona = managePersona.GetPersona(editorPersonaId);
                if (persona == null)
                    return BadRequest("editorPersonaId not found.");

                var userClaim = _userClaimsAccessor.GetUserClaim();
                userClaim.UserRealPageGuid = persona.RealPageId;
                IManageProductProspectContact manageProductProspectContact = new ManageProductProspectContact(userClaim);

                var result = manageProductProspectContact.GetMigrationUsers(editorPersonaId, datafilter);
                if (!result.IsError)
                    return Ok(result);
                else
                    return StatusCode(StatusCodes.Status403Forbidden, result);
            });
        }

        /// <summary>
        /// Update migration status of users.
        /// </summary>
        /// <param name="migrateUsers">List of users to mark as migrated</param>
        /// <returns>Migration status result</returns>
        [HttpPut("products/prospectcontactcenter/migrate-users")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUsersMigrationStatus(IList<MigrateUser> migrateUsers)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                if (userClaim == null)
                    return Unauthorized();

                IManageProductProspectContact manageProductProspectContact = new ManageProductProspectContact(userClaim);
                return Ok(manageProductProspectContact.UpdateUsersMigrationStatus(userClaim.PersonaId, migrateUsers));
            });
        }

        #endregion
    }
}
