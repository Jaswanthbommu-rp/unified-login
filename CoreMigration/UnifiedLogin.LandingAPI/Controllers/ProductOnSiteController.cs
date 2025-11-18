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
    /// Product OnSite Controller - Manages OnSite product-specific operations
    /// including roles, properties, regions, and migration functionality
    /// </summary>
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/products/onsite")]
    public class ProductOnSiteController : ControllerBase
    {
        private readonly IUserClaimsAccessor _userClaimsAccessor;
        private readonly IManagePersona _managePersona;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userClaimsAccessor">Accessor for current authenticated user's claims</param>
        /// <param name="managePersona">Service for managing persona operations</param>
        public ProductOnSiteController(IUserClaimsAccessor userClaimsAccessor, IManagePersona managePersona)
        {
            _userClaimsAccessor = userClaimsAccessor ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
        }

        /// <summary>
        /// Returns Roles (User Access Groups in OnSite)
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">User persona ID who is being created or edited</param>
        /// <param name="datafilter">A datafilter used to filter the roles</param>
        /// <returns>List of OnSite roles</returns>
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
                var manageProductOnSite = new ManageProductOnSite(userClaim);
                return manageProductOnSite.GetRoles(editorPersonaId, userPersonaId, datafilter);
            });

            return Ok(result);
        }

        /// <summary>
        /// Returns Properties
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">User persona ID who is being created or edited</param>
        /// <param name="datafilter">A datafilter used to filter the properties</param>
        /// <returns>List of OnSite properties</returns>
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
                var manageProductOnSite = new ManageProductOnSite(userClaim);
                return manageProductOnSite.GetProperties(editorPersonaId, userPersonaId, datafilter);
            });

            return Ok(result);
        }

        /// <summary>
        /// Returns Regions
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">User persona ID who is being created or edited</param>
        /// <param name="datafilter">A datafilter used to filter the regions</param>
        /// <returns>List of OnSite regions</returns>
        [HttpGet("regions")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRegions(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter)
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
                var manageProductOnSite = new ManageProductOnSite(userClaim);
                return manageProductOnSite.GetRegions(editorPersonaId, userPersonaId, datafilter);
            });

            return Ok(result);
        }

        #region Migration API

        /// <summary>
        /// Returns product users of an organization for migration purposes
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="datafilter">A datafilter used to filter the users</param>
        /// <returns>List of OnSite migration users</returns>
        [HttpGet("migration-users")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListOnSiteMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter)
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
                var manageProductOnSite = new ManageProductOnSite(userClaim);

                return (object)manageProductOnSite.GetMigrationUsers(editorPersonaId, datafilter);
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
            var result = await Task.Run(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageProductOnSite = new ManageProductOnSite(userClaim);
                var personaId = _userClaimsAccessor.PersonaId;
                return manageProductOnSite.UpdateUsersMigrationStatus(personaId, migrateUsers);
            });

            return Ok(result);
        }

        /// <summary>
        /// Updates the OnSite user status (enable/disable)
        /// </summary>
        /// <param name="productUser">The product user</param>
        /// <returns>Status update result</returns>
        [HttpPut("user/MT/status")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateOnSiteUserStatus([FromBody] ProductUser productUser)
        {
            var result = await Task.Run(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageProductOnSite = new ManageProductOnSite(userClaim);
                var personaId = _userClaimsAccessor.PersonaId;
                return manageProductOnSite.ChangeUserStatus(personaId, productUser.UserId.ToString());
            });

            if (!result)
            {
                return BadRequest("Disabling on-site user failed.");
            }

            return Ok("Successfully disabled product user.");
        }

        #endregion
    }
}
