using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
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
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/products/adminsupportportal")]
    public class ProductAdminSupportPortalController : ControllerBase
    {
        private readonly IUserClaimsAccessor _userClaimsAccessor;
        private readonly IManagePersona _managePersona;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userClaimsAccessor">Accessor for current authenticated user's claims</param>
        /// <param name="managePersona">Service for managing persona operations</param>
        public ProductAdminSupportPortalController(IUserClaimsAccessor userClaimsAccessor, IManagePersona managePersona)
        {
            _userClaimsAccessor = userClaimsAccessor ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
        }

        /// <summary>
        /// Returns Roles
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <param name="datafilter">A datafilter used to filter the roles.</param>
        /// <returns>List of Admin Support Portal roles</returns>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
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
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageProductAdminSupportPortal = new ManageProductAdminSupportPortal(userClaim);
                return manageProductAdminSupportPortal.GetRoles(editorPersonaId, userPersonaId, datafilter);
            });

            return Ok(result);
        }

        /// <summary>
        /// Returns Properties
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <param name="datafilter">A datafilter used to filter the properties.</param>
        /// <returns>List of Admin Support Portal properties</returns>
        [HttpGet("properties")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProperties(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter)
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
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageProductAdminSupportPortal = new ManageProductAdminSupportPortal(userClaim);
                return manageProductAdminSupportPortal.GetProperties(editorPersonaId, userPersonaId, datafilter);
            });

            return Ok(result);
        }

        #region Migration API

        /// <summary>
        /// List Client portal users
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="datafilter">A datafilter used to filter the users</param>
        /// <returns>List of Client portal migration users</returns>
        [HttpGet("clientportal_v1/migration-users")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListClientPortalMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            var result = await Task.Run<object>(() =>
            {
                var managePersona = new ManagePersona();
                var persona = managePersona.GetPersona(editorPersonaId);
                if (persona == null)
                {
                    return new { IsError = true, ErrorMessage = "editorPersonaId not found." };
                }

                var userClaim = _userClaimsAccessor.GetUserClaim();
                userClaim.UserRealPageGuid = persona.RealPageId;
                var manageProductAdminSupportPortal = new ManageProductAdminSupportPortal(userClaim);

                return (object)manageProductAdminSupportPortal.GetMigrationUsers(editorPersonaId, datafilter);
            });

            var resultType = result.GetType();
            if (resultType.GetProperty("IsError")?.GetValue(result) as bool? == true)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Update migration Client portal users
        /// </summary>
        /// <param name="migrateUsers">List of users to mark as migrated</param>
        /// <returns>Update result</returns>
        [HttpPut("clientportal_v1/migrate-users")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateUsersMigrationStatus([FromBody] IList<MigrateUser> migrateUsers)
        {
            var result = await Task.Run(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageProductAdminSupportPortal = new ManageProductAdminSupportPortal(userClaim);
                var personaId = _userClaimsAccessor.PersonaId;
                return manageProductAdminSupportPortal.UpdateUsersMigrationStatus(personaId, migrateUsers);
            });

            return Ok(result);
        }

        /// <summary>
        /// Disables the Client Portal product user.
        /// </summary>
        /// <param name="productUser">The product user.</param>
        /// <returns>Status update result</returns>
        [HttpPut("clientportal_v1/user/MT/status")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateClientPortalUserStatus([FromBody] ProductUser productUser)
        {
            var result = await Task.Run(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageProductAdminSupportPortal = new ManageProductAdminSupportPortal(userClaim);
                var personaId = _userClaimsAccessor.PersonaId;
                return manageProductAdminSupportPortal.ChangeUserStatus(personaId, productUser.UserLogin);
            });

            if (!result)
            {
                return BadRequest("Disabling Client Portal user failed.");
            }

            return Ok("Successfully disabled product user.");
        }

        #endregion
    }
}
