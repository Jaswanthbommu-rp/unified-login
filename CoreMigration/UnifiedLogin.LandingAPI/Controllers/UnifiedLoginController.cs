using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using UnifiedLogin.SharedObjects.ResponseObject;

namespace UnifiedLogin.LandingAPI.Controllers
{
    [ApiController]
    [Authorize]
    public class UnifiedLoginController : ControllerBase
    {
        private readonly IUserClaimsAccessor _userClaimsAccessor;
        private IManageOrganization _manageOrganization;
        private IManageUnifiedLogin _manageUnifiedLogin;
        private DefaultUserClaim _userClaims;
        private Guid _realpageUserId;

        public UnifiedLoginController(IUserClaimsAccessor userClaimsAccessor)
        {
            _userClaimsAccessor = userClaimsAccessor;
            var userClaim = _userClaimsAccessor.GetUserClaim();
            _userClaims = userClaim;
            _manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
            _manageOrganization = new ManageOrganization(_userClaims);
        }

        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpGet("products/unifiedlogin/user/roles")]
        public async Task<IActionResult> GetUserRoles(long editorPersonaId, long userPersonaId, long partyId, [FromQuery] RequestParameter datafilter)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");
                if (partyId == 0)
                    return BadRequest("partyId not supplied.");
                return Ok(_manageUnifiedLogin.GetUserRoles(editorPersonaId, userPersonaId, partyId));
            });
        }

        [ProducesResponseType(typeof(ListResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpDelete("products/unifiedlogin/role")]
        public async Task<IActionResult> DeleteRole(long editorPersonaId, int roleId)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");
                if (roleId == 0)
                    return BadRequest("roleId not supplied.");
                ListResponse listResponse = _manageUnifiedLogin.DeleteRole(editorPersonaId, (long)roleId);
                if (!string.IsNullOrEmpty(listResponse.ErrorReason))
                    return BadRequest(listResponse);
                return Ok(listResponse);
            });
        }

        [ProducesResponseType(typeof(ListResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpPut("products/unifiedlogin/setdefaultrole")]
        public async Task<IActionResult> SetDefaultRole(long editorPersonaId, int roleId)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");
                if (roleId == 0)
                    return BadRequest("roleId not supplied.");
                ListResponse listResponse = _manageUnifiedLogin.SetDefaultRole(editorPersonaId, userClaim.OrganizationPartyId, (long)roleId);
                if (!string.IsNullOrEmpty(listResponse.ErrorReason))
                    return BadRequest(listResponse);
                return Ok(listResponse);
            });
        }

        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpGet("products/unifiedlogin/roles")]
        public async Task<IActionResult> GetRoles(long editorPersonaId, long partyId, [FromQuery] RequestParameter datafilter, Guid? upfmId = null)
        {
            return await Task.Run<IActionResult>(() =>
            {
                ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
                if (editorPersonaId == 0)
                {
                    if (currentClaimPrincipal.HasClaim("scope", "internalapi") && _userClaims.PersonaId == 0)
                    {
                        if (!string.IsNullOrEmpty(upfmId.ToString()))
                        {
                            Guid AdminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(upfmId ?? default(Guid));
                            if (AdminCreatorRealPageId == Guid.Empty)
                            {
                                var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                                errorResponse.Errors.Add(new Error { Title = "Error", Source = "/product", Detail = "Invalid UPFMId.", StatusCode = "" });
                            }
                            RecreateClaimsForClient(AdminCreatorRealPageId);
                            editorPersonaId = _userClaims.PersonaId;
                            if (editorPersonaId == 0)
                            {
                                return BadRequest("invalid request.");
                            }
                            _manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
                        }
                    }
                }
                if (partyId == 0)
                    return BadRequest("partyId not supplied.");
                return Ok(_manageUnifiedLogin.GetRoles(editorPersonaId, partyId));
            });
        }

        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpGet("products/unifiedlogin/rolesCount")]
        public async Task<IActionResult> GetRolesWithCount(long editorPersonaId, long partyId, [FromQuery] RequestParameter datafilter)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");
                if (partyId == 0)
                    return BadRequest("partyId not supplied.");
                return Ok(_manageUnifiedLogin.GetRolesWithCount(editorPersonaId, partyId));
            });
        }

        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpGet("products/unifiedlogin/rights")]
        public async Task<IActionResult> GetRights(long editorPersonaId, long partyId, [FromQuery] RequestParameter datafilter)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");
                if (partyId == 0)
                    return BadRequest("partyId not supplied.");
                return Ok(_manageUnifiedLogin.GetRights(editorPersonaId, partyId));
            });
        }

        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpGet("products/unifiedlogin/rightsCount")]
        public async Task<IActionResult> GetRightsWithCount(long editorPersonaId, long partyId, [FromQuery] RequestParameter datafilter)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");
                if (partyId == 0)
                    return BadRequest("partyId not supplied.");
                return Ok(_manageUnifiedLogin.GetRightsWithCount(editorPersonaId, partyId));
            });
        }

        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpGet("products/unifiedlogin/role/allrights")]
        public async Task<IActionResult> GetAllRightsByRole(long editorPersonaId, long partyId, long roleId, [FromQuery] RequestParameter datafilter)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");
                if (partyId == 0)
                    return BadRequest("partyId not supplied.");
                return Ok(_manageUnifiedLogin.GetAllRightsByRole(editorPersonaId, partyId, roleId));
            });
        }

        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpGet("products/unifiedlogin/role/rights")]
        public async Task<IActionResult> GetRightsByRole(long editorPersonaId, long partyId, long roleId, [FromQuery] RequestParameter datafilter, Guid? upfmId = null)
        {
            return await Task.Run<IActionResult>(() =>
            {
                ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
                if (editorPersonaId == 0)
                {
                    if (currentClaimPrincipal.HasClaim("scope", "internalapi") && _userClaims.PersonaId == 0)
                    {
                        if (!string.IsNullOrEmpty(upfmId.ToString()))
                        {
                            Guid AdminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(upfmId ?? default(Guid));
                            if (AdminCreatorRealPageId == Guid.Empty)
                            {
                                var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                                errorResponse.Errors.Add(new Error { Title = "Error", Source = "/product", Detail = "Invalid UPFMId.", StatusCode = "" });
                            }
                            RecreateClaimsForClient(AdminCreatorRealPageId);
                            editorPersonaId = _userClaims.PersonaId;
                            if (editorPersonaId == 0)
                            {
                                return BadRequest("invalid request.");
                            }
                            _manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
                        }
                    }
                }
                if (partyId == 0)
                    return BadRequest("partyId not supplied.");
                return Ok(_manageUnifiedLogin.GetRightsByRole(editorPersonaId, partyId, roleId));
            });
        }

        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpGet("products/unifiedlogin/right/roles")]
        public async Task<IActionResult> GetRolesByRight(long editorPersonaId, long partyId, long rightId, [FromQuery] RequestParameter datafilter)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");
                if (partyId == 0)
                    return BadRequest("partyId not supplied.");
                return Ok(_manageUnifiedLogin.GetRolesByRight(editorPersonaId, partyId, rightId));
            });
        }

        [ProducesResponseType(typeof(ListResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpPost("products/unifiedlogin/role")]
        public async Task<IActionResult> AddRole(long editorPersonaId, long partyId, string roleName, string inheritRoleId = null)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");
                if (partyId == 0)
                    return BadRequest("partyId not supplied.");
                if (string.IsNullOrEmpty(roleName.Trim()))
                    return BadRequest("roleName not supplied.");
                ListResponse listResponse = _manageUnifiedLogin.AddUpdateRole(editorPersonaId, partyId, 0, roleName, inheritRoleId);
                //if (!string.IsNullOrEmpty(listResponse.ErrorReason))
                //    return BadRequest(listResponse);
                return Ok(listResponse);
            });
        }

        [ProducesResponseType(typeof(ListResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpPut("products/unifiedlogin/role")]
        public async Task<IActionResult> UpdateRole(long editorPersonaId, long partyId, long roleid, string roleName, string inheritRoleId = null)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");
                if (partyId == 0)
                    return BadRequest("partyId not supplied.");
                if (roleid == 0)
                    return BadRequest("roleid not supplied.");
                if (string.IsNullOrEmpty(roleName.Trim()))
                    return BadRequest("roleName not supplied.");
                ListResponse listResponse = _manageUnifiedLogin.AddUpdateRole(editorPersonaId, partyId, roleid, roleName, inheritRoleId);
                if (!string.IsNullOrEmpty(listResponse.ErrorReason))
                    return BadRequest(listResponse);
                return Ok(listResponse);
            });
        }

        [ProducesResponseType(typeof(ListResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpPut("products/unifiedlogin/role/rights")]
        public async Task<IActionResult> UpdateRoleRights(long editorPersonaId, int roleId, ULRightsAddRemoveList rightsToAddRemove)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");
                if (roleId == 0)
                    return BadRequest("roleId not supplied.");
                if (rightsToAddRemove.RightsToAdd == null && rightsToAddRemove.RightsToDelete == null)
                    return BadRequest("No Data");
                ListResponse response = _manageUnifiedLogin.UpdateRightsToRole(editorPersonaId, (long)roleId, rightsToAddRemove.RightsToAdd, rightsToAddRemove.RightsToDelete);
                //if (!string.IsNullOrEmpty(response.ErrorReason))
                //    return BadRequest(response);
                return Ok(response);
            });
        }

        [ProducesResponseType(typeof(ListResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpPut("products/unifiedlogin/clone/rights")]
        public async Task<IActionResult> CloneRoleRights(long editorPersonaId, int roleId, ULRightsAddRemoveList rightsToAddRemove)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");
                if (roleId == 0)
                    return BadRequest("roleId not supplied.");
                if (rightsToAddRemove.RightsToAdd == null && rightsToAddRemove.RightsToDelete == null)
                    return BadRequest("No Data");
                ListResponse response = _manageUnifiedLogin.CloneRightsToRole(editorPersonaId, (long)roleId, rightsToAddRemove.RightsToAdd, rightsToAddRemove.RightsToDelete);

                return Ok(response);
            });
        }

        [ProducesResponseType(typeof(ListResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpPut("products/unifiedlogin/right/roles")]
        public async Task<IActionResult> UpdateRightRoles(long editorPersonaId, int rightId, RolesAddRemoveList rolesToAddRemove)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");
                if (rightId == 0)
                    return BadRequest("roleId not supplied.");
                if (rolesToAddRemove.RolesToAdd == null && rolesToAddRemove.RolesToDelete == null)
                    return BadRequest("No Data");
                ListResponse response = _manageUnifiedLogin.UpdateRolesByRight(editorPersonaId, (long)rightId, rolesToAddRemove.RolesToAdd, rolesToAddRemove.RolesToDelete);
                if (!string.IsNullOrEmpty(response.ErrorReason))
                    return BadRequest(response);
                return Ok(response);
            });
        }

        /// <summary>
        /// Used to recreate claims for client
        /// </summary>
        /// <param name="realpageUserId">RealPage UserId</param>
        private void RecreateClaimsForClient(Guid realpageUserId)
        {
            if (string.IsNullOrEmpty(realpageUserId.ToString())) return;

            var rpCache = new RPObjectCache();
            _realpageUserId = realpageUserId;

            var cacheKey = $"recreateClaimsForClient_{realpageUserId}";
            _userClaims = rpCache.GetFromCache<DefaultUserClaim>(cacheKey, 180, () =>
            {
                IManagePerson personLogic = new ManagePerson();
                Person person = personLogic.GetPerson(realpageUserId);
                if (person == null)
                {
                    throw new Exception($"Missing persona information for client_info user while Recreation of Claims For Client.  realPageId: {realpageUserId}");
                }

                IManageUserLogin userLoginLogic = new ManageUserLogin();
                IManageUserRoleRight userRoleRight = new ManageUserRoleRight();
                var userLogin = userLoginLogic.GetUserLoginOnly(realpageUserId);

                IManagePersona managePersona = new ManagePersona();
                //Active Persona is linked to one organization
                Persona persona = managePersona.GetActivePersonaWithoutRights(realpageUserId); // this user can only be under 1 company to work correctly
                var roles = userRoleRight.GetAssignedRoleForPersona(ProductEnum.UnifiedPlatform, persona.PersonaId);
                var claim = new DefaultUserClaim
                {
                    UserId = (int)userLogin.UserId,
                    OrganizationPartyId = persona.Organization.PartyId,
                    LoginName = userLogin.LoginName,
                    OrganizationMasterId = (long)persona.Organization.BooksMasterId,
                    CustomerMasterId = (long)persona.Organization.BooksMasterId,
                    OrganizationName = persona.Organization.Name,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    PersonaId = persona.PersonaId,
                    OrganizationRealPageGuid = persona.Organization.RealPageId,
                    UserRealPageGuid = _realpageUserId,
                    CorrelationId = Guid.NewGuid(),
                    RealPageEmployee = persona.Organization.RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId,
                };
                ClaimsPrincipal userPrincipal = ClaimsPrincipal.Current;
                var identity = (ClaimsIdentity)userPrincipal.Identity;
                identity.AddClaims(roles.Select(r => new Claim("roleid", r.RoleID.ToString())).ToList());

                claim.Rights = BaseUserRights.GetUserRightsBy(userPrincipal, claim);
                return claim;

            });

        }
    }
}
