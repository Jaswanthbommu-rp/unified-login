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
using UnifiedLogin.SharedObjects.Product.OneSite;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Controller for all OneSite product management related APIs
    /// </summary>
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/products/onesite")]
    public class ProductOneSiteController : ControllerBase
    {
        private readonly IUserClaimsAccessor _userClaimsAccessor;
        private readonly IManageProductOneSite _manageProductOneSite;
        private readonly IManageOrganization _manageOrganization;
        private readonly IManagePersona _managePersona;
        private readonly IManagePerson _managePerson;
        private readonly IManageUserLogin _manageUserLogin;
        private readonly IManageUserRoleRight _manageUserRoleRight;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProductOneSiteController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageProductOneSite manageProductOneSite,
            IManageOrganization manageOrganization,
            IManagePersona managePersona,
            IManagePerson managePerson,
            IManageUserLogin manageUserLogin,
            IManageUserRoleRight manageUserRoleRight)
        {
            _userClaimsAccessor = userClaimsAccessor ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
            _manageProductOneSite = manageProductOneSite ?? throw new ArgumentNullException(nameof(manageProductOneSite));
            _manageOrganization = manageOrganization ?? throw new ArgumentNullException(nameof(manageOrganization));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _managePerson = managePerson ?? throw new ArgumentNullException(nameof(managePerson));
            _manageUserLogin = manageUserLogin ?? throw new ArgumentNullException(nameof(manageUserLogin));
            _manageUserRoleRight = manageUserRoleRight ?? throw new ArgumentNullException(nameof(manageUserRoleRight));
        }

        #region Public Methods

        /// <summary>
        /// Used to get a list of properties for the given OneSite user
        /// </summary>
        /// <param name="editorPersonaId">The persona to use to find the OneSite login to update</param>
        /// <param name="userPersonaId">User persona ID</param>
        /// <param name="assignedOnly">Return only assigned properties</param>
        /// <param name="datafilter">A datafilter used to filter the properties</param>
        /// <returns>List of properties</returns>
        [HttpGet("user/properties")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOneSitePropertyList(long editorPersonaId, long userPersonaId, bool assignedOnly, [FromQuery] RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            var response = await Task.Run(() =>
                _manageProductOneSite.GetOneSitePropertyList(editorPersonaId, userPersonaId, assignedOnly, datafilter));

            return Ok(response);
        }

        /// <summary>
        /// Used to get a list of users for the given property
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="datafilter">A datafilter used to filter</param>
        /// <param name="propertyId">The property to get the list of users for</param>
        /// <param name="assignedOnly">only return assigned users</param>
        /// <returns>List of users</returns>
        [HttpGet("property/users")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOneSitePropertyUsersList(long editorPersonaId, [FromQuery] RequestParameter datafilter, int propertyId, bool assignedOnly = false)
        {
            var response = await Task.Run(() =>
                _manageProductOneSite.GetUsersForProperty(editorPersonaId, propertyId, assignedOnly, datafilter));

            return Ok(response);
        }

        /// <summary>
        /// Used to update the list of properties for the given OneSite user
        /// </summary>
        /// <param name="editorPersonaId">The person making the changes</param>
        /// <param name="userPersonaId">The persona to use to find the OneSite login to update</param>
        /// <param name="propertyList">The list of property ids to assign to the user</param>
        /// <returns>Update result</returns>
        [HttpPut("user/properties")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateOneSiteUserProperties(long editorPersonaId, long userPersonaId, [FromBody] List<string> propertyList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0)
            {
                return BadRequest("editorPersonaId or userPersonaId not supplied.");
            }

            if (propertyList.Count == 0)
            {
                return BadRequest("No Data");
            }

            var result = await Task.Run(() =>
                _manageProductOneSite.UpdatePropertiesForUser(editorPersonaId, userPersonaId, propertyList, out List<AdditionalParameters> additionalParameters));

            if (!string.IsNullOrEmpty(result))
            {
                return Ok($"{result} Records Updated");
            }

            return NoContent();
        }

        /// <summary>
        /// Used to get a list of users for the given role
        /// </summary>
        /// <param name="editorPersonaId">The id of the user making the changes</param>
        /// <param name="datafilter">A datafilter used to filter the roles</param>
        /// <param name="roleId">The role to get the list of users for</param>
        /// <param name="assignedOnly">only return assigned users</param>
        /// <returns>List of users</returns>
        [HttpGet("role/users")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOneSiteRoleUsersList(long editorPersonaId, [FromQuery] RequestParameter datafilter, int roleId, bool assignedOnly = false)
        {
            var response = await Task.Run(() =>
                _manageProductOneSite.GetUsersForRole(editorPersonaId, roleId, assignedOnly, datafilter));

            return Ok(response);
        }

        /// <summary>
        /// Used to get a list of roles for the given OneSite user
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">The persona to use to find the OneSite login to update</param>
        /// <param name="assignedOnly">Return only assigned roles</param>
        /// <param name="datafilter">A datafilter used to filter the roles</param>
        /// <returns>List of roles</returns>
        [HttpGet("user/roles")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOneSiteRoleList(long editorPersonaId, long userPersonaId, bool assignedOnly, [FromQuery] RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            var response = await Task.Run(() =>
            {
                if (userPersonaId > 0)
                {
                    return _manageProductOneSite.GetOneSiteRoleList(editorPersonaId, userPersonaId, assignedOnly, datafilter);
                }
                else
                {
                    return _manageProductOneSite.GetOneSiteRoleListAll(editorPersonaId, datafilter);
                }
            });

            return Ok(response);
        }

        /// <summary>
        /// Used to update the list of roles for the given OneSite user
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">The persona to use to find the OneSite login to update</param>
        /// <param name="roleList">The list of role ids to add to the given user</param>
        /// <returns>Update result</returns>
        [HttpPut("user/roles")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateOneSiteUserRoles(long editorPersonaId, long userPersonaId, [FromBody] List<string> roleList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0)
            {
                return BadRequest("editorPersonaId or userPersonaId not supplied.");
            }

            if (roleList.Count == 0)
            {
                return BadRequest("No Data");
            }

            var result = await Task.Run(() =>
                _manageProductOneSite.UpdateRolesForUser(editorPersonaId, userPersonaId, roleList, out List<AdditionalParameters> additionalParameters));

            if (!string.IsNullOrEmpty(result))
            {
                return Ok($"{result} Records Updated");
            }

            return NoContent();
        }

        /// <summary>
        /// Used to get a list of roles for the PMC or the given user if provided
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="datafilter">A datafilter used to filter the roles</param>
        /// <param name="upfmId">UPFM ID for internal API calls</param>
        /// <returns>List of roles</returns>
        [HttpGet("role")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOneSiteRoleListAll(long editorPersonaId, [FromQuery] RequestParameter datafilter, Guid? upfmId = null)
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
                _manageProductOneSite.GetOneSiteRoleListAll(currentEditorPersonaId, datafilter));

            return Ok(response);
        }

        /// <summary>
        /// Used to update the rights assigned to the given custom role
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="roleId">The role being modified</param>
        /// <param name="rightsToAddRemove">A list of rights to add/remove from the role</param>
        /// <returns>Update result</returns>
        [HttpPut("role/rights")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateRoleRights(long editorPersonaId, int roleId, [FromBody] RightsAddRemoveList rightsToAddRemove)
        {
            if (editorPersonaId == 0 || roleId == 0)
            {
                return BadRequest("editorPersonaId or roleId not supplied.");
            }

            if (rightsToAddRemove.RightsToAdd == null && rightsToAddRemove.RightsToDelete == null)
            {
                return BadRequest("No Data");
            }

            var result = await Task.Run(() =>
                _manageProductOneSite.UpdateRoleToRights(editorPersonaId, roleId, rightsToAddRemove.RightsToAdd, rightsToAddRemove.RightsToDelete));

            if (string.IsNullOrEmpty(result))
            {
                return Ok();
            }
            else
            {
                if (result.ToUpper() == "ROLE NOT FOUND")
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest(result);
                }
            }
        }

        /// <summary>
        /// Used to update the status of a given user
        /// </summary>
        /// <param name="editorPersonaId">The persona to use to find the OneSite login to update</param>
        /// <param name="userPersonaId">User persona ID</param>
        /// <param name="active">Active status</param>
        /// <returns>Update result</returns>
        [HttpPut("user/status")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateOneSiteUserStatus(long editorPersonaId, long userPersonaId, bool active)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            var result = await Task.Run(() =>
                _manageProductOneSite.EnableOneSiteUser(editorPersonaId, userPersonaId, active));

            if (string.IsNullOrEmpty(result))
            {
                return Ok();
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Used to create a new OneSite account for the given GreenBook user
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">The persona to use to find the OneSite login to update</param>
        /// <param name="rolepropList">Used to update the list of roles and properties given to a user</param>
        /// <returns>Creation result</returns>
        [HttpPost("user")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateOneSiteUser(long editorPersonaId, long userPersonaId, [FromBody] OneSiteRoleAndPropertyList rolepropList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0)
            {
                return BadRequest("editorPersonaId or userPersonaId not supplied.");
            }

            var result = await Task.Run(() =>
                _manageProductOneSite.ManageOneSiteUser(editorPersonaId, userPersonaId, rolepropList.RoleList, rolepropList.PropertyList, out List<AdditionalParameters> additionalParameters));

            if (string.IsNullOrEmpty(result))
            {
                return StatusCode((int)HttpStatusCode.Created);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Used to update a OneSite account for the given GreenBook user
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">The persona to use to find the OneSite login to update</param>
        /// <param name="rolepropList">Used to update the list of roles and properties given to a user</param>
        /// <returns>Update result</returns>
        [HttpPut("user")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateOneSiteUser(long editorPersonaId, long userPersonaId, [FromBody] OneSiteRoleAndPropertyList rolepropList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0)
            {
                return BadRequest("editorPersonaId or userPersonaId not supplied.");
            }

            if (rolepropList == null)
            {
                rolepropList = new OneSiteRoleAndPropertyList();
            }

            var result = await Task.Run(() =>
                _manageProductOneSite.ManageOneSiteUser(editorPersonaId, userPersonaId, rolepropList.RoleList, rolepropList.PropertyList, out List<AdditionalParameters> additionalParameters));

            if (string.IsNullOrEmpty(result))
            {
                return Ok();
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Used to delete the given OneSite User
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">The persona to use to find the OneSite login to update</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("user")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteOneSiteUser(long editorPersonaId, long userPersonaId)
        {
            if (editorPersonaId == 0 || userPersonaId == 0)
            {
                return BadRequest("editorPersonaId or userPersonaId not supplied.");
            }

            var result = await Task.Run(() =>
                _manageProductOneSite.DeleteOneSiteUser(editorPersonaId, userPersonaId));

            if (string.IsNullOrEmpty(result))
            {
                return NoContent();
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Used to get the PMC URL for the given user
        /// </summary>
        /// <param name="userPersonaId">User persona ID</param>
        /// <returns>PMC information</returns>
        [HttpGet("pmcurl")]
        [ProducesResponseType(typeof(PMCInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPMCURL(long userPersonaId)
        {
            if (userPersonaId == 0)
            {
                return BadRequest("userPersonaId not supplied.");
            }

            var pmc = await Task.Run(() =>
                _manageProductOneSite.GetPMCURL(userPersonaId));

            if (string.IsNullOrEmpty(pmc?.PMCURL) || pmc == null)
            {
                return BadRequest("PMC URL not found.");
            }

            return Ok(pmc);
        }

        #endregion

        /// <summary>
        /// Used to update the roles assigned to a given right
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="rightId">The right being assigned</param>
        /// <param name="roleList">A list of role ids to update</param>
        /// <param name="assignStatus">Is the right being added or removed</param>
        /// <returns>Update result</returns>
        [HttpPut("right/roles")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateOneSiteRolesWithRight(long editorPersonaId, int rightId, [FromBody] List<string> roleList, bool assignStatus)
        {
            if (rightId == 0 || editorPersonaId == 0)
            {
                return BadRequest("rightId or editorPersonaId not supplied.");
            }

            if (roleList.Count == 0)
            {
                return BadRequest("No Data");
            }

            var result = await Task.Run(() =>
                _manageProductOneSite.UpdateRightToRoles(editorPersonaId, rightId, roleList, assignStatus));

            if (string.IsNullOrEmpty(result))
            {
                return Ok("Roles Updated");
            }

            return NoContent();
        }

        /// <summary>
        /// Used to get a list of roles assigned to the right
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="datafilter">A datafilter used to filter the roles</param>
        /// <param name="rightId">Right ID</param>
        /// <param name="assignedOnly">Return only assigned roles</param>
        /// <returns>List of roles</returns>
        [HttpGet("right/roles")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRolesForRight(long editorPersonaId, [FromQuery] RequestParameter datafilter, int rightId, bool assignedOnly)
        {
            var response = await Task.Run(() =>
                _manageProductOneSite.GetRolesForRight(editorPersonaId, rightId, assignedOnly, datafilter));

            return Ok(response);
        }

        /// <summary>
        /// Used to get a list of centers available for the PMC to filter rights
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <returns>List of centers</returns>
        [HttpGet("right/center")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOneSiteRightCenters(long editorPersonaId)
        {
            var response = await Task.Run(() =>
                _manageProductOneSite.GetOneSiteRightsCenters(editorPersonaId));

            return Ok(response);
        }

        /// <summary>
        /// Used to get a list of rights
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="datafilter">A datafilter used to filter the rights</param>
        /// <param name="roleId">If passed, return if the rights are assigned to the given role or not</param>
        /// <param name="assignedToRoleOnly">Only return rights assigned to the given role id, if given</param>
        /// <param name="upfmId">UPFM ID for internal API calls</param>
        /// <returns>List of rights</returns>
        [HttpGet("rights")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOneSiteRights(long editorPersonaId, [FromQuery] RequestParameter datafilter, int roleId = 0, bool assignedToRoleOnly = false, Guid? upfmId = null)
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
                _manageProductOneSite.GetOneSiteRights(currentEditorPersonaId, datafilter, roleId, assignedToRoleOnly));

            return Ok(response);
        }

        /// <summary>
        /// Used to add a new custom role
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="roleName">The name of the role</param>
        /// <param name="inheritRoleId">The id of the role this role was created from</param>
        /// <returns>Creation result</returns>
        [HttpPost("role")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddRole(long editorPersonaId, string roleName, string inheritRoleId = null)
        {
            var response = await Task.Run(() =>
                _manageProductOneSite.AddUpdateRole(editorPersonaId, 0, roleName, inheritRoleId));

            if (!string.IsNullOrEmpty(response.ErrorReason))
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Used to update a custom role
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="roleId">The role being updated</param>
        /// <param name="roleName">The name of the role</param>
        /// <param name="inheritRoleId">The id of the role this role was created from</param>
        /// <returns>Update result</returns>
        [HttpPut("role")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateRole(long editorPersonaId, int roleId, string roleName, string inheritRoleId = null)
        {
            var response = await Task.Run(() =>
                _manageProductOneSite.AddUpdateRole(editorPersonaId, roleId, roleName, inheritRoleId));

            return Ok(response);
        }

        /// <summary>
        /// Used to delete a custom role
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="roleId">The role being deleted</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("role")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteRole(long editorPersonaId, int roleId)
        {
            var result = await Task.Run(() =>
                _manageProductOneSite.DeleteRole(editorPersonaId, roleId));

            if (string.IsNullOrEmpty(result))
            {
                return Ok();
            }
            else
            {
                if (result.ToUpper() == "ROLE NOT FOUND")
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest(result);
                }
            }
        }

        /// <summary>
        /// Used to ResetVerificationCode of OneSiteUser
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">User persona ID</param>
        /// <returns>Reset result</returns>
        [HttpPost("resetverificationcode")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ResetVerificationCode(long editorPersonaId, long userPersonaId)
        {
            var result = await Task.Run(() =>
                _manageProductOneSite.ResetVerificationCode(editorPersonaId, userPersonaId));

            if (string.IsNullOrEmpty(result))
            {
                return Ok();
            }
            else
            {
                if (result.ToUpper() == "NO RESULT")
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest(result);
                }
            }
        }

        #region User-Status

        /// <summary>
        /// Enables or disables the one site user
        /// </summary>
        /// <param name="productUser">The product user</param>
        /// <returns>Status update result</returns>
        [HttpPut("user/MT/status")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateOneSiteUserStatusMT([FromBody] ProductUser productUser)
        {
            var personaId = _userClaimsAccessor.PersonaId;

            var result = await Task.Run(() =>
                _manageProductOneSite.ChangeUserStatus(personaId, productUser.UserName));

            if (!result)
            {
                return BadRequest("Enabling OneSite user failed.");
            }

            return Ok("Success");
        }

        #endregion

        #region Migration API

        /// <summary>
        /// Returns product users of an organization for given user
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="datafilter">A datafilter used to filter the users</param>
        /// <returns>List of OneSite migration users</returns>
        [HttpGet("migration-users")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListOneSiteMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter)
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

                var manageProductOneSite = new ManageProductOneSite(userClaim);
                return (object)manageProductOneSite.GetMigrationUsers(editorPersonaId, datafilter);
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
                _manageProductOneSite.UpdateUsersMigrationStatus(personaId, migrateUsers));

            return Ok(result);
        }

        #endregion

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

                // Recreate claims for the admin user
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

        /// <summary>
        /// Used to add/remove rights from a given role
        /// </summary>
        public class RightsAddRemoveList
        {
            /// <summary>
            /// A list of rights to add to the role
            /// </summary>
            public List<string> RightsToAdd { get; set; }

            /// <summary>
            /// A list of rights to remove from the role
            /// </summary>
            public List<string> RightsToDelete { get; set; }
        }
    }
}
