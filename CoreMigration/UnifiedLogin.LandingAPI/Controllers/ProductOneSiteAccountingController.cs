using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Security.Claims;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Accounting;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.ResponseObject;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Controller for all OneSite Accounting product management related APIs
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("products/onesiteaccounting")]
    public class ProductOneSiteAccountingController : BaseController
    {
        private readonly IManageProductOneSiteAccounting _manageProductOneSiteAccounting;
        private readonly IManageProductOneSiteAccountingAsync _manageProductOneSiteAccountingAsync;
        private readonly IManageOrganization _manageOrganization;
        private readonly IManagePersona _managePersona;
        private readonly IManagePerson _managePerson;
        private readonly IManageUserLogin _manageUserLogin;
        private readonly IManageUserRoleRight _manageUserRoleRight;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProductOneSiteAccountingController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageProductOneSiteAccounting manageProductOneSiteAccounting,
            IManageProductOneSiteAccountingAsync manageProductOneSiteAccountingAsync,
            IManageOrganization manageOrganization,
            IManagePersona managePersona,
            IManagePerson managePerson,
            IManageUserLogin manageUserLogin,
            IManageUserRoleRight manageUserRoleRight) : base(userClaimsAccessor)
        {
            _manageProductOneSiteAccounting = manageProductOneSiteAccounting ?? throw new ArgumentNullException(nameof(manageProductOneSiteAccounting));
            _manageProductOneSiteAccountingAsync = manageProductOneSiteAccountingAsync ?? throw new ArgumentNullException(nameof(manageProductOneSiteAccountingAsync));
            _manageOrganization = manageOrganization ?? throw new ArgumentNullException(nameof(manageOrganization));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _managePerson = managePerson ?? throw new ArgumentNullException(nameof(managePerson));
            _manageUserLogin = manageUserLogin ?? throw new ArgumentNullException(nameof(manageUserLogin));
            _manageUserRoleRight = manageUserRoleRight ?? throw new ArgumentNullException(nameof(manageUserRoleRight));
        }

        /// <summary>
        /// Used to get a list of properties for the given user
        /// </summary>
        [HttpGet("user/properties")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetUserProperties(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter)
        {
            ListResponse response = _manageProductOneSiteAccounting.GetUserPropertiesNew(editorPersonaId, userPersonaId, datafilter);
            return Ok(response);
        }

        /// <summary>
        /// Used to get a list of companies for the given user
        /// </summary>
        [HttpGet("user/companies")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetUserCompanies(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter)
        {
            ListResponse response = _manageProductOneSiteAccounting.GetUserCompanies(editorPersonaId, userPersonaId, datafilter);
            return Ok(response);
        }

        /// <summary>
        /// Used to get a list of roles for the given user
        /// </summary>
        [HttpGet("user/roles")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetUserRoles(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter)
        {
            ListResponse response = _manageProductOneSiteAccounting.GetUserRoles(editorPersonaId, userPersonaId, datafilter);
            return Ok(response);
        }

        /// <summary>
        /// Used to update the properties assigned to the user
        /// </summary>
        [HttpPut("user/properties")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateUserProperties(long editorPersonaId, long userPersonaId, [FromBody] List<string> propertyList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0)
                return BadRequest("Editor and user persona IDs are required.");

            if (propertyList == null || propertyList.Count == 0)
                return BadRequest("No Data");

            var (result, _) = UpdatePropertiesToUserWithTuple(editorPersonaId, userPersonaId, propertyList, false);

            if (string.IsNullOrEmpty(result))
                return Ok("Records Updated");

            return StatusCode((int)HttpStatusCode.NoContent, result);
        }

        /// <summary>
        /// Used to update the roles assigned to the user
        /// </summary>
        [HttpPut("user/roles")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateUserRoles(long editorPersonaId, long userPersonaId, [FromBody] List<string> roleList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0)
                return BadRequest("Editor and user persona IDs are required.");

            if (roleList == null || roleList.Count == 0)
                return BadRequest("No Data");

            var (result, _) = UpdateRolesToUserWithTuple(editorPersonaId, userPersonaId, roleList, false);

            if (string.IsNullOrEmpty(result))
                return Ok("Records Updated");

            return StatusCode((int)HttpStatusCode.NoContent, result);
        }

        #region User

        /// <summary>
        /// Used to create a new Accounting account for the given GreenBook user
        /// </summary>
        [HttpPost("user")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateAccountingUser(long editorPersonaId, long userPersonaId, [FromBody] AccountingRoleAndPropertyList rolepropList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0)
                return BadRequest("Editor and user persona IDs are required.");

            rolepropList ??= new AccountingRoleAndPropertyList();

            var (result, _) = ManageAccountingUserWithTuple(editorPersonaId, userPersonaId, rolepropList.RoleList, rolepropList.PropertyList, rolepropList.CompaniesList, rolepropList.IsAccountingAdmin, rolepropList.HasAccessToSiteSpendManagementOnly, rolepropList.HasAccessToAllCurrentFutureProperties);

            if (string.IsNullOrEmpty(result))
                return StatusCode((int)HttpStatusCode.Created);

            return BadRequest(result);
        }

        /// <summary>
        /// Used to update an existing Accounting account for the given GreenBook user
        /// </summary>
        [HttpPut("user")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateAccountingUser(long editorPersonaId, long userPersonaId, [FromBody] AccountingRoleAndPropertyList rolepropList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0)
                return BadRequest("Editor and user persona IDs are required.");

            rolepropList ??= new AccountingRoleAndPropertyList();

            var (result, _) = ManageAccountingUserWithTuple(editorPersonaId, userPersonaId, rolepropList.RoleList, rolepropList.PropertyList, rolepropList.CompaniesList, rolepropList.IsAccountingAdmin, rolepropList.HasAccessToSiteSpendManagementOnly, rolepropList.HasAccessToAllCurrentFutureProperties);

            if (string.IsNullOrEmpty(result))
                return Ok();

            return BadRequest(result);
        }

        /// <summary>
        /// Used to delete an existing Accounting account for the given GreenBook user persona
        /// </summary>
        [HttpDelete("user")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DeleteAccountingUser(long editorPersonaId, long userPersonaId)
        {
            if (editorPersonaId == 0 || userPersonaId == 0)
                return BadRequest("Editor and user persona IDs are required.");

            string result = _manageProductOneSiteAccounting.DeleteAccountingUser(editorPersonaId, userPersonaId);
            if (string.IsNullOrEmpty(result))
                return NoContent();

            return BadRequest(result);
        }

        /// <summary>
        /// Used to update the status of a given user
        /// </summary>
        [HttpPut("user/status")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateAccountingUserStatus(long editorPersonaId, long userPersonaId, bool active, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("Editor persona ID is required.");

            var userClaim = _userClaimsAccessor.GetUserClaim();
            string result = await _manageProductOneSiteAccountingAsync.ChangeStatusAccountingUserAsync(userClaim, editorPersonaId, userPersonaId, active, cancellationToken);
            if (string.IsNullOrEmpty(result))
                return Ok();

            return BadRequest();
        }

        /// <summary>
        /// Changing User SSO Claim Status
        /// </summary>
        [HttpPut("user/claimstatus")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateAccountingUserClaimStatus(long editorPersonaId, long userPersonaId, bool isLinked, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0 || userPersonaId == 0)
                return BadRequest("Editor and user persona IDs are required.");

            var userClaim = _userClaimsAccessor.GetUserClaim();
            bool result = await _manageProductOneSiteAccountingAsync.ChangeAccountingUserClaimStatusAsync(userClaim, editorPersonaId, userPersonaId, isLinked, cancellationToken);
            if (result)
                return Ok();

            return BadRequest();
        }

        #endregion

        #region Migration API

        /// <summary>
        /// Returns product users of an organization for given user.
        /// </summary>
        [HttpGet("migration-users")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListOneSiteAccountingMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            var persona = _managePersona.GetPersona(editorPersonaId);
            if (persona == null)
                return BadRequest("editorPersonaId not found.");

            var userClaim = _userClaimsAccessor.GetUserClaim();
            userClaim.UserRealPageGuid = persona.RealPageId;

            var result = await _manageProductOneSiteAccountingAsync.GetMigrationUsersAsync(userClaim, editorPersonaId, datafilter, cancellationToken);
            if (!result.IsError)
                return Ok(result);

            return StatusCode((int)HttpStatusCode.Forbidden, result);
        }

        /// <summary>
        /// Update migration status of users.
        /// </summary>
        [HttpPut("migrate-users")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateUsersMigrationStatus([FromBody] IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var result = await _manageProductOneSiteAccountingAsync.UpdateUsersMigrationStatusAsync(userClaim, userClaim.PersonaId, migrateUsers, cancellationToken);
            return Ok(result);
        }

        #endregion

        #region Roles & Rights

        /// <summary>
        /// Used to get a count of roles
        /// </summary>
        [HttpGet("rolescount")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRolesCount(long editorPersonaId, [FromQuery] RequestParameter datafilter, Guid? upfmId = null, CancellationToken cancellationToken = default)
        {
            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            var userClaim = _userClaimsAccessor.GetUserClaim();

            if (editorPersonaId == 0)
            {
                if (currentClaimPrincipal.HasClaim("scope", "internalapi") && userClaim.PersonaId == 0)
                {
                    if (!string.IsNullOrEmpty(upfmId.ToString()))
                    {
                        Guid adminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(upfmId ?? default(Guid));
                        if (adminCreatorRealPageId == Guid.Empty)
                        {
                            var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                            errorResponse.Errors.Add(new Error { Title = "Error", Source = "/product", Detail = "Invalid UPFMId.", StatusCode = "" });
                            return BadRequest(errorResponse);
                        }
                        RecreateClaimsForClient(adminCreatorRealPageId, ref userClaim);
                        editorPersonaId = userClaim.PersonaId;
                        if (editorPersonaId == 0)
                            return BadRequest("invalid request.");
                    }
                }
            }

            ListResponse response = await _manageProductOneSiteAccountingAsync.GetRolesCountAsync(userClaim, editorPersonaId, datafilter, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Used to get a list of all roles
        /// </summary>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetAllRoles(long editorPersonaId, [FromQuery] RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            ListResponse response = _manageProductOneSiteAccounting.GetAllRoles(editorPersonaId, datafilter);
            return Ok(response);
        }

        /// <summary>
        /// Used to get a list of rights
        /// </summary>
        [HttpGet("rights")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetRights(long editorPersonaId)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            ListResponse response = _manageProductOneSiteAccounting.GetRights(editorPersonaId);
            return Ok(response);
        }

        /// <summary>
        /// Used to get a list of applications
        /// </summary>
        [HttpGet("applications")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetApplications(long editorPersonaId)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            ListResponse response = _manageProductOneSiteAccounting.GetApplications(editorPersonaId);
            return Ok(response);
        }

        /// <summary>
        /// Used to get a list of roles assigned to the right
        /// </summary>
        [HttpGet("right/roles")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetRolesForRight(long editorPersonaId, [FromQuery] RequestParameter datafilter, int rightId, bool assignedOnly, [FromQuery] string right)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");
            if (rightId == 0)
                return BadRequest("rightId not supplied.");
            if (string.IsNullOrEmpty(right?.Trim()))
                return BadRequest("right not supplied.");

            ListResponse response = _manageProductOneSiteAccounting.GetRolesForRight(editorPersonaId, datafilter, rightId, assignedOnly, JsonConvert.DeserializeObject<ProductRightAcct>(right));
            return Ok(response);
        }

        /// <summary>
        /// Used to update roles for a right
        /// </summary>
        [HttpPut("right/roles")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateRolesForRight(long editorPersonaId, int rightId, [FromBody] RolesAddRemove rolesToAddRemove, [FromQuery] string right)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");
            if (rightId == 0)
                return BadRequest("rightId not supplied.");

            rolesToAddRemove.RolesToAdd ??= new List<ProductRoleAcct>();
            rolesToAddRemove.RolesToDelete ??= new List<ProductRoleAcct>();

            if (rolesToAddRemove.RolesToAdd.Count == 0 && rolesToAddRemove.RolesToDelete.Count == 0)
                return BadRequest("Roles not supplied to Add or Remove.");
            if (string.IsNullOrEmpty(right?.Trim()))
                return BadRequest("right not supplied.");

            ListResponse response = _manageProductOneSiteAccounting.UpdateRolesForRight(editorPersonaId, rightId, rolesToAddRemove.RolesToAdd, rolesToAddRemove.RolesToDelete, JsonConvert.DeserializeObject<ProductRightAcct>(right));
            return Ok(response);
        }

        /// <summary>
        /// Used to get a list of rights assigned to the role
        /// </summary>
        [HttpGet("role/rights")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRightsForRole(long editorPersonaId, [FromQuery] RequestParameter datafilter, int roleId, string roleName, Guid? upfmId = null, CancellationToken cancellationToken = default)
        {
            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            var userClaim = _userClaimsAccessor.GetUserClaim();

            if (editorPersonaId == 0)
            {
                if (currentClaimPrincipal.HasClaim("scope", "internalapi") && userClaim.PersonaId == 0)
                {
                    if (!string.IsNullOrEmpty(upfmId.ToString()))
                    {
                        Guid adminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(upfmId ?? default(Guid));
                        if (adminCreatorRealPageId == Guid.Empty)
                        {
                            var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                            errorResponse.Errors.Add(new Error { Title = "Error", Source = "/product", Detail = "Invalid UPFMId.", StatusCode = "" });
                            return BadRequest(errorResponse);
                        }
                        RecreateClaimsForClient(adminCreatorRealPageId, ref userClaim);
                        editorPersonaId = userClaim.PersonaId;
                        if (editorPersonaId == 0)
                            return BadRequest("invalid request.");
                    }
                }
            }
            if (roleId == 0)
                return BadRequest("roleId not supplied.");
            if (string.IsNullOrEmpty(roleName?.Trim()))
                return BadRequest("roleName not supplied.");

            ListResponse response = await _manageProductOneSiteAccountingAsync.GetRightsForRoleAsync(userClaim, editorPersonaId, datafilter, roleName, roleId, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Used to update a list of rights assigned to the role
        /// </summary>
        [HttpPut("role/rights")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateRightsForRole(long editorPersonaId, int roleId, string roleName, [FromBody] RightsAddRemove rightsToAddRemove)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");
            if (roleId == 0)
                return BadRequest("roleId not supplied.");

            rightsToAddRemove.RightsToAdd ??= new List<ProductRightAcct>();
            rightsToAddRemove.RightsToRemove ??= new List<ProductRightAcct>();

            if (rightsToAddRemove.RightsToAdd.Count == 0 && rightsToAddRemove.RightsToRemove.Count == 0)
                return BadRequest("Rights not supplied to Add or Remove.");

            ListResponse response = _manageProductOneSiteAccounting.UpdateRightsForRole(editorPersonaId, roleId, roleName, rightsToAddRemove.RightsToAdd, rightsToAddRemove.RightsToRemove);
            return Ok(response);
        }

        /// <summary>
        /// Used to add a new custom role
        /// </summary>
        [HttpPost("role")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateRole(long editorPersonaId, string roleName)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");
            if (string.IsNullOrEmpty(roleName?.Trim()))
                return BadRequest("roleName not supplied.");

            ListResponse response = _manageProductOneSiteAccounting.CreateRole(editorPersonaId, roleName);
            if (!string.IsNullOrEmpty(response.ErrorReason))
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Used to clone a custom role from existing role
        /// </summary>
        [HttpPut("role")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CloneRole(long editorPersonaId, string inheritedRoleName, string roleName)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");
            if (string.IsNullOrEmpty(inheritedRoleName?.Trim()))
                return BadRequest("inheritRoleName not supplied.");
            if (string.IsNullOrEmpty(roleName?.Trim()))
                return BadRequest("roleName not supplied.");

            ListResponse listResponse = _manageProductOneSiteAccounting.CloneRole(editorPersonaId, roleName, inheritedRoleName);
            if (!string.IsNullOrEmpty(listResponse.ErrorReason))
                return BadRequest(listResponse);

            return Ok(listResponse);
        }

        /// <summary>
        /// Used to delete a custom role
        /// </summary>
        [HttpDelete("role")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DeleteRole(long editorPersonaId, long roleId, string roleName)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");
            if (roleId == 0)
                return BadRequest("roleId not supplied.");
            if (string.IsNullOrEmpty(roleName?.Trim()))
                return BadRequest("roleName not supplied.");

            ListResponse listResponse = _manageProductOneSiteAccounting.DeleteRole(editorPersonaId, roleId, roleName);
            if (!string.IsNullOrEmpty(listResponse.ErrorReason))
                return BadRequest(listResponse);

            return Ok(listResponse);
        }

        #endregion

        #region User-Status

        /// <summary>
        /// Disables the Accounting user.
        /// </summary>
        [HttpPut("user/MT/status")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateAccountingUserStatus([FromBody] ProductUser productUser, CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            bool success = await _manageProductOneSiteAccountingAsync.ChangeUserStatusAsync(userClaim, userClaim.PersonaId, productUser.UserName, cancellationToken);
            if (!success)
                return BadRequest("Disabling Accounting user failed.");

            return Ok("Successfully disabled product user.");
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Used to recreate claims for client
        /// </summary>
        private void RecreateClaimsForClient(Guid realpageUserId, ref DefaultUserClaim userClaim)
        {
            if (string.IsNullOrEmpty(realpageUserId.ToString())) return;

            var rpCache = new RPObjectCache();

            var cacheKey = $"recreateClaimsForClient_{realpageUserId}";
            userClaim = rpCache.GetFromCache<DefaultUserClaim>(cacheKey, 180, () =>
            {
                Person person = _managePerson.GetPerson(realpageUserId);
                if (person == null)
                {
                    throw new Exception($"Missing persona information for client_info user while Recreation of Claims For Client. realPageId: {realpageUserId}");
                }

                var userLogin = _manageUserLogin.GetUserLoginOnly(realpageUserId);

                // Active Persona is linked to one organization
                Persona persona = _managePersona.GetActivePersonaWithoutRights(realpageUserId);
                var roles = _manageUserRoleRight.GetAssignedRoleForPersona(ProductEnum.UnifiedPlatform, persona.PersonaId);
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
                    UserRealPageGuid = realpageUserId,
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

        private (string result, List<AdditionalParameters> additionalParameters) UpdatePropertiesToUserWithTuple(
            long editorPersonaId, long userPersonaId, List<string> propertyList, bool isDeleted)
        {
            List<AdditionalParameters> additionalParameters;
            string result = _manageProductOneSiteAccounting.UpdatePropertiesToUser(editorPersonaId, userPersonaId, propertyList, isDeleted, out additionalParameters);
            return (result, additionalParameters);
        }

        private (string result, List<AdditionalParameters> additionalParameters) UpdateRolesToUserWithTuple(
            long editorPersonaId, long userPersonaId, List<string> roleList, bool isDeleted)
        {
            List<AdditionalParameters> additionalParameters;
            string result = _manageProductOneSiteAccounting.UpdateRolesToUser(editorPersonaId, userPersonaId, roleList, isDeleted, out additionalParameters);
            return (result, additionalParameters);
        }

        private (string result, List<AdditionalParameters> additionalParameters) ManageAccountingUserWithTuple(
            long editorPersonaId, long userPersonaId, List<string> roleList, List<string> propertyList, List<string> companiesList,
            bool isAccountingAdmin, bool hasAccessToSiteSpendManagementOnly, bool hasAccessToAllCurrentFutureProperties)
        {
            List<AdditionalParameters> additionalParameters;
            string result = _manageProductOneSiteAccounting.ManageAccountingUser(editorPersonaId, userPersonaId, roleList, propertyList, companiesList, isAccountingAdmin, hasAccessToSiteSpendManagementOnly, hasAccessToAllCurrentFutureProperties, out additionalParameters);
            return (result, additionalParameters);
        }

        #endregion
    }
}
