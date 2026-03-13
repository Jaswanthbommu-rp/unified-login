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
    /// Product Lead2Lease Controller for user and migration management
    /// </summary>
    [ApiController]
    [Route("")]
    [Authorize]
    public class ProductLead2LeaseController : BaseController
    {
        private readonly IManageProductLead2Lease _manageProductLead2Lease;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProductLead2LeaseController(IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            _manageProductLead2Lease = new ManageProductLead2Lease(userClaim);
        }

        /// <summary>
        /// Used to get a list of roles
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">User persona ID</param>
        /// <param name="datafilter">A datafilter used to filter the roles</param>
        /// <returns>List of roles for the given company</returns>
        [HttpGet("products/lead2lease/roles")]
        [ProducesResponseType(typeof(ListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLead2LeaseRoles(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter)
        {
            return await Task.Run<IActionResult>(() =>
            {
                ListResponse response = _manageProductLead2Lease.GetRoles(editorPersonaId, userPersonaId, datafilter);
                return Ok(response);
            });
        }

        /// <summary>
        /// Used to get a list of properties
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">User persona ID</param>
        /// <param name="datafilter">A datafilter used to filter the properties</param>
        /// <returns>List of properties for the given company</returns>
        [HttpGet("products/lead2lease/properties")]
        [ProducesResponseType(typeof(ListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLead2LeaseProperties(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter)
        {
            return await Task.Run<IActionResult>(() =>
            {
                ListResponse response = _manageProductLead2Lease.GetProperties(editorPersonaId, userPersonaId, datafilter);
                return Ok(response);
            });
        }

        /// <summary>
        /// Update Lead2Lease user status
        /// </summary>
        /// <param name="productUser">Product user object with status details</param>
        /// <returns>Success or error message</returns>
        [HttpPut("products/lead2lease/user/MT/status")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLead2LeaseUserStatus([FromBody] ProductUser productUser)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                if (userClaim == null)
                    return Unauthorized();

                if (!_manageProductLead2Lease.ChangeUserStatus(userClaim.PersonaId, productUser.UserName, productUser.UserId.ToString()))
                {
                    return BadRequest("Deactivate Lead2Lease user failed.");
                }

                return Ok("Successfully disabled product user.");
            });
        }

        /// <summary>
        /// Returns product users of an organization for given user
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="datafilter">Data filter for pagination and filtering</param>
        /// <returns>List of Lead2Lease migration users</returns>
        [HttpGet("products/lead2lease/migration-users")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListLead2LeaseMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter)
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
                var manageProductLead2Lease = new ManageProductLead2Lease(userClaim);

                var result = manageProductLead2Lease.GetMigrationUsers(editorPersonaId, datafilter);
                if (!result.IsError)
                    return Ok(result);
                else
                    return StatusCode(StatusCodes.Status403Forbidden, result);
            });
        }

        /// <summary>
        /// Update migration status of users
        /// </summary>
        /// <param name="migrateUsers">List of users to mark as migrated</param>
        /// <returns>Migration status result</returns>
        [HttpPut("products/lead2lease/migrate-users")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUsersMigrationStatus([FromBody] IList<MigrateUser> migrateUsers)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                if (userClaim == null)
                    return Unauthorized();

                var result = _manageProductLead2Lease.UpdateUsersMigrationStatus(userClaim.PersonaId, migrateUsers);
                return Ok(result);
            });
        }
    }
}
