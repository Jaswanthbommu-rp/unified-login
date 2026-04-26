using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.MarketingCenter;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.ResponseObject;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Used to manage Marketing Center users
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("")]
    public class ProductMarketingCenterController : ControllerBase
    {
        private readonly IManageProductMarketingCenterAsync _manageProductMarketingCenter;
        private readonly IManageOrganization _manageOrganization;
        private readonly IManagePersonaAsync _managePersona;
        private readonly IManagePerson _managePerson;
        private readonly IManageUserLogin _manageUserLogin;
        private readonly IManageUserRoleRight _manageUserRoleRight;
        private readonly IUserClaimsAccessor _userClaimsAccessor;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProductMarketingCenterController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageProductMarketingCenterAsync manageProductMarketingCenter,
            IManageOrganization manageOrganization,
            IManagePersonaAsync managePersona,
            IManagePerson managePerson,
            IManageUserLogin manageUserLogin,
            IManageUserRoleRight manageUserRoleRight)
        {
            _userClaimsAccessor = userClaimsAccessor ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
            _manageProductMarketingCenter = manageProductMarketingCenter ?? throw new ArgumentNullException(nameof(manageProductMarketingCenter));
            _manageOrganization = manageOrganization ?? throw new ArgumentNullException(nameof(manageOrganization));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _managePerson = managePerson ?? throw new ArgumentNullException(nameof(managePerson));
            _manageUserLogin = manageUserLogin ?? throw new ArgumentNullException(nameof(manageUserLogin));
            _manageUserRoleRight = manageUserRoleRight ?? throw new ArgumentNullException(nameof(manageUserRoleRight));
        }

        #region User management

        /// <summary>
        /// Used to get a list of roles
        /// </summary>
        [HttpGet("products/marketingcenter/roles")]
        [ProducesResponseType(typeof(ProductRole), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetMarketingCenterRoles(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            ListResponse response = await _manageProductMarketingCenter.GetRolesAsync(editorPersonaId, userPersonaId, datafilter, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Used to get a list of properties
        /// </summary>
        [HttpGet("products/marketingcenter/properties")]
        [ProducesResponseType(typeof(ProductProperty), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetMarketingCenterProperties(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            ListResponse response = await _manageProductMarketingCenter.GetPropertiesAsync(editorPersonaId, userPersonaId, datafilter, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Used to create a new account for the given GreenBook user
        /// </summary>
        [HttpPost("products/marketingcenter/user")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateMarketingCenterUser(long editorPersonaId, long userPersonaId, [FromBody] MarketingCenterRoleAndPropertyList rolepropList, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0 || userPersonaId == 0)
                return BadRequest("Invalid editorPersonaId or userPersonaId");

            rolepropList ??= new MarketingCenterRoleAndPropertyList();

            var (result, _) = await _manageProductMarketingCenter.ManageMarketingCenterUserAsync(
                editorPersonaId, userPersonaId,
                rolepropList.RoleList, rolepropList.PropertyList,
                rolepropList.IsAssignedNewPropertyByDefault, cancellationToken);

            if (string.IsNullOrEmpty(result))
                return StatusCode((int)HttpStatusCode.Created);

            return BadRequest(result);
        }

        /// <summary>
        /// Used to update an existing account for the given GreenBook user
        /// </summary>
        [HttpPut("products/marketingcenter/user")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateMarketingCenterUser(long editorPersonaId, long userPersonaId, [FromBody] MarketingCenterRoleAndPropertyList rolepropList, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0 || userPersonaId == 0)
                return BadRequest("Invalid editorPersonaId or userPersonaId");

            rolepropList ??= new MarketingCenterRoleAndPropertyList();

            var (result, _) = await _manageProductMarketingCenter.ManageMarketingCenterUserAsync(
                editorPersonaId, userPersonaId,
                rolepropList.RoleList, rolepropList.PropertyList,
                rolepropList.IsAssignedNewPropertyByDefault, cancellationToken);

            if (string.IsNullOrEmpty(result))
                return Ok();

            return BadRequest(result);
        }

        #endregion

        #region User-Status

        /// <summary>
        /// Disable the Marketing Center user.
        /// </summary>
        [HttpPut("products/marketingcenter/user/MT/status")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateMarketingCenterUserStatus([FromBody] ProductUser produtUser, CancellationToken cancellationToken = default)
        {
            var personaId = _userClaimsAccessor.PersonaId;

            bool success = await _manageProductMarketingCenter.ChangeUserStatusAsync(personaId, produtUser.UserName, produtUser.UserId.ToString(), produtUser.IsAssigned, cancellationToken);
            if (!success)
            {
                return produtUser.IsAssigned
                    ? BadRequest("Activate MarketingCenter user failed.")
                    : BadRequest("Deactivate MarketingCenter user failed.");
            }

            return Ok("Successfully disabled product user.");
        }

        #endregion

        #region Roles Rights Setup

        /// <summary>
        /// Used to get a count of roles
        /// </summary>
        [HttpGet("products/marketingcenter/rolescount")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRolesCount(long editorPersonaId, Guid? upfmId = null, CancellationToken cancellationToken = default)
        {
            ListResponse response = await _manageProductMarketingCenter.GetRolesCountAsync(editorPersonaId, cancellationToken);
            return !response.IsError ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Used to get a list of rights
        /// </summary>
        [HttpGet("products/marketingcenter/rights")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRights(long editorPersonaId, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("Invalid editorPersonaId");

            ListResponse response = await _manageProductMarketingCenter.GetRightsAsync(editorPersonaId, cancellationToken);
            return !response.IsError ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Delete a Marketing Center role.
        /// </summary>
        [HttpDelete("products/marketingcenter/role")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteMarketingCenterRole(long editorPersonaId, int roleId, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("Invalid editorPersonaId");

            return Ok(await _manageProductMarketingCenter.DeleteRoleAsync(editorPersonaId, roleId, cancellationToken));
        }

        /// <summary>
        /// Update a Marketing Center role status.
        /// </summary>
        [HttpPost("products/marketingcenter/role/status")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateMarketingCenterRoleStatus(long editorPersonaId, int roleId, bool isActive, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("Invalid editorPersonaId");

            return Ok(await _manageProductMarketingCenter.UpdateRoleStatusAsync(editorPersonaId, roleId, isActive, cancellationToken));
        }

        /// <summary>
        /// Get roles for the right id
        /// </summary>
        [HttpGet("products/marketingcenter/right/roles")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRolesForRightId(long editorPersonaId, int rightId, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("Invalid editorPersonaId");

            ListResponse response = await _manageProductMarketingCenter.GetRolesForRightIdAsync(editorPersonaId, rightId, cancellationToken);
            return !response.IsError ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Update roles for right
        /// </summary>
        [HttpPut("products/marketingcenter/right/roles")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateRolesForRight(long editorPersonaId, int rightId, [FromBody] List<string> roleList, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("Invalid editorPersonaId");

            ListResponse response = await _manageProductMarketingCenter.UpdateRolesForRightAsync(editorPersonaId, rightId, roleList, cancellationToken);
            if (!response.IsError)
                return Ok("Roles Updated");

            return BadRequest(response);
        }

        /// <summary>
        /// Get Rights for roleId
        /// </summary>
        [HttpGet("products/marketingcenter/role/allrights")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRightsForRoleId(long editorPersonaId, int roleId = 0, Guid? upfmId = null, CancellationToken cancellationToken = default)
        {
            var userClaims = _userClaimsAccessor.GetUserClaim();

            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            if (editorPersonaId == 0)
            {
                if (currentClaimPrincipal.HasClaim("scope", "internalapi") && userClaims.PersonaId == 0)
                {
                    if (!string.IsNullOrEmpty(upfmId.ToString()))
                    {
                        Guid AdminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(upfmId ?? default(Guid));
                        if (AdminCreatorRealPageId == Guid.Empty)
                        {
                            var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                            errorResponse.Errors.Add(new Error { Title = "Error", Source = "/product", Detail = "Invalid UPFMId.", StatusCode = "" });
                        }
                        RecreateClaimsForClient(AdminCreatorRealPageId, ref userClaims, cancellationToken);
                        editorPersonaId = userClaims.PersonaId;
                        if (editorPersonaId == 0)
                            return BadRequest("invalid request.");
                    }
                }
            }

            ListResponse response = await _manageProductMarketingCenter.GetRightsForRoleIdAsync(editorPersonaId, roleId, cancellationToken);
            return !response.IsError ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Create New Role with rights
        /// </summary>
        [HttpPost("products/marketingcenter/role")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateNewMCRoleWithRights(long editorPersonaId, [FromBody] MCRole mcRole, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("Invalid editorPersonaId");

            ListResponse response = await _manageProductMarketingCenter.CreateNewMCRoleWithRightsAsync(editorPersonaId, mcRole, cancellationToken);
            return Convert.ToString(response.Additional) == "RoleError" ? Ok(response) : (!response.IsError ? Ok(response) : BadRequest(response));
        }

        /// <summary>
        /// Update Role and rights
        /// </summary>
        [HttpPut("products/marketingcenter/role")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateMCRoleWithRights(long editorPersonaId, [FromBody] MCRole mcRole, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("Invalid editorPersonaId");

            ListResponse response = await _manageProductMarketingCenter.UpdateMCRoleWithRightsAsync(editorPersonaId, mcRole, cancellationToken);

            HttpStatusCode responseStatus;
            if (Convert.ToString(response.Additional) == "RoleError" || !response.IsError)
                responseStatus = HttpStatusCode.OK;
            else
                responseStatus = HttpStatusCode.BadRequest;

            return StatusCode((int)responseStatus, response);
        }

        #endregion

        #region Migration API

        /// <summary>
        /// Returns product users of an organization for given user.
        /// </summary>
        [HttpGet("products/marketingcenter/migration-users")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListMarketingCenterMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            var persona = await _managePersona.GetPersonaAsync(editorPersonaId, false, cancellationToken);
            if (persona == null)
                return BadRequest("editorPersonaId not found.");

            var result = await _manageProductMarketingCenter.GetMigrationUsersAsync(editorPersonaId, datafilter, cancellationToken);
            if (!result.IsError)
                return Ok(result);

            return StatusCode((int)HttpStatusCode.Forbidden, result);
        }

        /// <summary>
        /// Update migration status of users.
        /// </summary>
        [HttpPut("products/marketingcenter/migrate-users")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateUsersMigrationStatus([FromBody] IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        {
            var personaId = _userClaimsAccessor.PersonaId;
            return Ok(await _manageProductMarketingCenter.UpdateUsersMigrationStatusAsync(personaId, migrateUsers, cancellationToken));
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Used to recreate claims for client
        /// </summary>
        private void RecreateClaimsForClient(Guid realpageUserId, ref DefaultUserClaim userClaims, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(realpageUserId.ToString())) return;

            var rpCache = new RPObjectCache();

            var cacheKey = $"recreateClaimsForClient_{realpageUserId}";
            userClaims = rpCache.GetFromCache<DefaultUserClaim>(cacheKey, 180, () =>
            {
                Person person = _managePerson.GetPerson(realpageUserId);
                if (person == null)
                {
                    throw new Exception($"Missing persona information for client_info user while Recreation of Claims For Client.  realPageId: {realpageUserId}");
                }

                var userLogin = _manageUserLogin.GetUserLoginOnly(realpageUserId);

                // Active Persona is linked to one organization
                Persona persona = _managePersona.GetActivePersonaWithoutRightsAsync(realpageUserId, cancellationToken).GetAwaiter().GetResult();
                var roles = _manageUserRoleRight.GetAssignedRoleForPersona(ProductEnum.UnifiedPlatform, persona.PersonaId);
                var claim = new DefaultUserClaim
                {
                    UserId = (int)userLogin.UserId,
                    OrganizationPartyId = persona.Organization.PartyId,
                    LoginName = userLogin.LoginName,
                    OrganizationMasterId = persona.Organization.BooksMasterId,
                    CustomerMasterId = persona.Organization.BooksMasterId,
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

        #endregion
    }
}
