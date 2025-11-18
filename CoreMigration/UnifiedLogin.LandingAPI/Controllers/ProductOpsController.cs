using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.Ops;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Product Ops Controller - Manages Ops (Spend Management) product-specific operations
    /// </summary>
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/products/ops")]
    public class ProductOpsController : ControllerBase
    {
        private readonly IUserClaimsAccessor _userClaimsAccessor;
        private readonly IManageProductOps _manageProductOps;
        private readonly IManageOrganization _manageOrganization;
        private readonly IManagePersona _managePersona;
        private readonly IManagePerson _managePerson;
        private readonly IManageUserLogin _manageUserLogin;
        private readonly IManageUserRoleRight _manageUserRoleRight;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProductOpsController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageProductOps manageProductOps,
            IManageOrganization manageOrganization,
            IManagePersona managePersona,
            IManagePerson managePerson,
            IManageUserLogin manageUserLogin,
            IManageUserRoleRight manageUserRoleRight)
        {
            _userClaimsAccessor = userClaimsAccessor ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
            _manageProductOps = manageProductOps ?? throw new ArgumentNullException(nameof(manageProductOps));
            _manageOrganization = manageOrganization ?? throw new ArgumentNullException(nameof(manageOrganization));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _managePerson = managePerson ?? throw new ArgumentNullException(nameof(managePerson));
            _manageUserLogin = manageUserLogin ?? throw new ArgumentNullException(nameof(manageUserLogin));
            _manageUserRoleRight = manageUserRoleRight ?? throw new ArgumentNullException(nameof(manageUserRoleRight));
        }

        /// <summary>
        /// Used to get a list of roles
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">User persona ID</param>
        /// <param name="datafilter">A datafilter used to filter the roles</param>
        /// <param name="assetGroup">Asset group filter</param>
        /// <param name="upfmId">UPFM ID for internal API calls</param>
        /// <returns>List of Ops roles</returns>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOpsRoles(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter, string assetGroup = "", Guid? upfmId = null)
        {
            var currentEditorPersonaId = editorPersonaId;

            if (currentEditorPersonaId == 0)
            {
                ClaimsPrincipal currentClaimPrincipal = HttpContext.User;
                if (currentClaimPrincipal.HasClaim("scope", "internalapi") && _userClaimsAccessor.PersonaId == 0)
                {
                    if (!string.IsNullOrEmpty(upfmId.ToString()))
                    {
                        currentEditorPersonaId = await GetPersonaIdForUpfmAsync(upfmId ?? Guid.Empty);
                        if (currentEditorPersonaId == 0)
                        {
                            return BadRequest("Invalid UPFMId.");
                        }
                    }
                }
            }

            var response = await Task.Run(() =>
                _manageProductOps.GetRoles(currentEditorPersonaId, userPersonaId, assetGroup, datafilter));

            return Ok(response);
        }

        /// <summary>
        /// Used to get a list of assets
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">User persona ID</param>
        /// <param name="datafilter">A datafilter used to filter the assets</param>
        /// <param name="includeDisabled">Include disabled assets in the result</param>
        /// <returns>List of Ops assets</returns>
        [HttpGet("assets")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOpsAssets(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter, bool includeDisabled = false)
        {
            var response = await Task.Run(() =>
                _manageProductOps.GetCompanyAssets(editorPersonaId, userPersonaId, includeDisabled, datafilter));

            return Ok(response);
        }

        /// <summary>
        /// Used to get a count of roles
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <returns>Count of roles</returns>
        [HttpGet("rolescount")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRolesCount(long editorPersonaId)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            var response = await Task.Run(() =>
                _manageProductOps.GetRolesCount(editorPersonaId, string.Empty));

            return Ok(response);
        }

        /// <summary>
        /// Used to get a list of rights
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <returns>List of Ops rights</returns>
        [HttpGet("rights")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRights(long editorPersonaId)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            var response = await Task.Run(() =>
                _manageProductOps.GetRights(editorPersonaId));

            return Ok(response);
        }

        #region Users

        /// <summary>
        /// Used to create a new account for the given GreenBook user
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">The persona to use to find the login to update</param>
        /// <param name="rolepropList">Used to update the list of roles and properties given to a user</param>
        /// <returns>Creation result</returns>
        [HttpPost("user")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateOpsUser(long editorPersonaId, long userPersonaId, [FromBody] OpsRoleAndPropertyList rolepropList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0)
            {
                return BadRequest("editorPersonaId or userPersonaId not supplied.");
            }

            if (rolepropList == null)
            {
                rolepropList = new OpsRoleAndPropertyList();
            }

            var result = await Task.Run(() =>
                _manageProductOps.ManageOpsUser(editorPersonaId, userPersonaId, rolepropList.RoleList, rolepropList.PropertyList, out List<AdditionalParameters> additionalParameters));

            if (string.IsNullOrEmpty(result))
            {
                return StatusCode((int)HttpStatusCode.Created);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Used to update an account for the given GreenBook user
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">The persona to use to find the login to update</param>
        /// <param name="rolepropList">Used to update the list of roles and properties given to a user</param>
        /// <returns>Update result</returns>
        [HttpPut("user")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateOpsUser(long editorPersonaId, long userPersonaId, [FromBody] OpsRoleAndPropertyList rolepropList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0)
            {
                return BadRequest("editorPersonaId or userPersonaId not supplied.");
            }

            if (rolepropList == null)
            {
                rolepropList = new OpsRoleAndPropertyList();
            }

            var result = await Task.Run(() =>
                _manageProductOps.ManageOpsUser(editorPersonaId, userPersonaId, rolepropList.RoleList, rolepropList.PropertyList, out List<AdditionalParameters> additionalParameters));

            if (string.IsNullOrEmpty(result))
            {
                return Ok();
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Used to delete an existing account for the given GreenBook user persona
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">The persona to use to find the login to delete</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("user")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteOpsUser(long editorPersonaId, long userPersonaId)
        {
            if (editorPersonaId == 0 || userPersonaId == 0)
            {
                return BadRequest("editorPersonaId or userPersonaId not supplied.");
            }

            var result = await Task.Run(() =>
                _manageProductOps.EnableUser(editorPersonaId, userPersonaId, isActive: false, deleteUser: true));

            if (string.IsNullOrEmpty(result))
            {
                return NoContent();
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Used to update the status of a given user
        /// </summary>
        /// <param name="editorPersonaId">The persona to use to find the login to update</param>
        /// <param name="userPersonaId">User persona ID</param>
        /// <param name="active">Active status</param>
        /// <returns>Update result</returns>
        [HttpPut("user/status")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateOpsUserStatus(long editorPersonaId, long userPersonaId, bool active)
        {
            if (editorPersonaId == 0 || userPersonaId == 0)
            {
                return BadRequest("editorPersonaId or userPersonaId not supplied.");
            }

            var result = await Task.Run(() =>
                _manageProductOps.EnableUser(editorPersonaId, userPersonaId, isActive: active, deleteUser: false));

            if (string.IsNullOrEmpty(result))
            {
                return Ok();
            }

            return BadRequest(result);
        }

        #endregion

        #region User-Status

        /// <summary>
        /// Disable ops user
        /// </summary>
        /// <param name="productUser">The product user</param>
        /// <returns>Status update result</returns>
        [HttpPut("user/MT/status")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateOpsUserStatusMT([FromBody] ProductUser productUser)
        {
            var personaId = _userClaimsAccessor.PersonaId;

            var result = await Task.Run(() =>
                _manageProductOps.ChangeUserStatus(personaId, productUser.UserName, productUser.UserId.ToString()));

            if (!result)
            {
                return BadRequest("Disabling Ops user failed.");
            }

            return Ok("Successfully disabled product user.");
        }

        #endregion

        #region Migration API

        /// <summary>
        /// Returns product users for given organization
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="datafilter">A datafilter used to filter the users</param>
        /// <returns>List of spend management migration users</returns>
        [HttpGet("migration-users")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListOpsMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter)
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

                var manageProductOps = new ManageProductOps(userClaim);
                return (object)manageProductOps.GetMigrationUsers(editorPersonaId, datafilter);
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
                _manageProductOps.UpdateUsersMigrationStatus(personaId, migrateUsers));

            return Ok(result);
        }

        #endregion

        /// <summary>
        /// Used to get a list of roles assigned to the right
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="rightId">Right ID</param>
        /// <returns>List of roles</returns>
        [HttpGet("right/roles")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRolesForRight(long editorPersonaId, int rightId)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (rightId == 0)
            {
                return BadRequest("rightId not supplied.");
            }

            var response = await Task.Run(() =>
                _manageProductOps.GetRolesForRight(editorPersonaId, rightId));

            return Ok(response);
        }

        /// <summary>
        /// Used to get a list of rights for a role
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="roleId">Role ID</param>
        /// <param name="upfmId">UPFM ID for internal API calls</param>
        /// <returns>List of rights</returns>
        [HttpGet("role/rights")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRightsByRole(long editorPersonaId, int roleId, Guid? upfmId = null)
        {
            var currentEditorPersonaId = editorPersonaId;

            if (currentEditorPersonaId == 0)
            {
                ClaimsPrincipal currentClaimPrincipal = HttpContext.User;
                if (currentClaimPrincipal.HasClaim("scope", "internalapi") && _userClaimsAccessor.PersonaId == 0)
                {
                    if (!string.IsNullOrEmpty(upfmId.ToString()))
                    {
                        currentEditorPersonaId = await GetPersonaIdForUpfmAsync(upfmId ?? Guid.Empty);
                        if (currentEditorPersonaId == 0)
                        {
                            return BadRequest("Invalid request.");
                        }
                    }
                }
            }

            if (roleId == 0)
            {
                return BadRequest("roleId not supplied.");
            }

            var response = await Task.Run(() =>
                _manageProductOps.GetRightsByRole(currentEditorPersonaId, roleId));

            return Ok(response);
        }

        /// <summary>
        /// Used to create role with rights
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="input">The input containing role details</param>
        /// <param name="roleId">Role ID</param>
        /// <returns>Creation result</returns>
        [HttpPost("role/rights")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateRole(long editorPersonaId, [FromBody] OpsInput input, long roleId)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (input == null)
            {
                input = new OpsInput();
            }

            if (input.RoleName.Trim() == string.Empty)
            {
                return BadRequest("RoleName not supplied.");
            }

            var response = await Task.Run(() =>
                _manageProductOps.CreateRole(editorPersonaId, input, roleId));

            return Ok(response);
        }

        /// <summary>
        /// Used to update role with rights
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="input">The input containing role details</param>
        /// <param name="roleId">Role ID</param>
        /// <returns>Update result</returns>
        [HttpPut("role/rights")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateRole(long editorPersonaId, [FromBody] OpsInput input, long roleId)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (roleId == 0)
            {
                return BadRequest("roleId not supplied.");
            }

            if (input == null)
            {
                input = new OpsInput();
            }

            if (input.RoleName.Trim() == string.Empty)
            {
                return BadRequest("RoleName not supplied.");
            }

            var response = await Task.Run(() =>
                _manageProductOps.CreateRole(editorPersonaId, input, roleId));

            return Ok(response);
        }

        #region Private Methods

        /// <summary>
        /// Helper method to get persona ID for UPFM ID
        /// </summary>
        private async Task<long> GetPersonaIdForUpfmAsync(Guid upfmId)
        {
            return await Task.Run(() =>
            {
                var adminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(upfmId);
                if (adminCreatorRealPageId == Guid.Empty)
                {
                    return 0;
                }

                var person = _managePerson.GetPerson(adminCreatorRealPageId);
                if (person == null)
                {
                    return 0;
                }

                var persona = _managePersona.GetActivePersonaWithoutRights(adminCreatorRealPageId);
                return persona?.PersonaId ?? 0;
            });
        }

        #endregion
    }
}
