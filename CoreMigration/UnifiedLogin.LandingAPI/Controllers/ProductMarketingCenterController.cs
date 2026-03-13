using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
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
    /// Used to manage Marketing Center users - Migrated to .NET Core 8.0
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("")]
    public class ProductMarketingCenterController : BaseController
    {
        private readonly IManageOrganization _manageOrganization;
        private readonly IManageProductMarketingCenter _manageProductMarketingCenter;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userClaimsAccessor">Accessor for current authenticated user's claims</param>
        /// <param name="repository">Repository for database operations</param>
        /// <param name="manageOrganization">Service for managing organization operations</param>
        /// <param name="manageProductMarketingCenter">Service for managing Marketing Center operations</param>
        public ProductMarketingCenterController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageOrganization manageOrganization,
            IManageProductMarketingCenter manageProductMarketingCenter) : base(userClaimsAccessor)
        {
            _manageOrganization = manageOrganization ?? throw new ArgumentNullException(nameof(manageOrganization));
            _manageProductMarketingCenter = manageProductMarketingCenter ?? throw new ArgumentNullException(nameof(manageProductMarketingCenter));
        }

        #region User management

        /// <summary>
        /// Used to get a list of roles
        /// </summary>
        /// <remarks>For now filtering and sorting will be done on the UI side</remarks>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter">A datafilter used to filter the roles.</param>
        /// <returns></returns>
        [HttpGet("products/marketingcenter/roles")]
        [ProducesResponseType(typeof(ProductRole), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetMarketingCenterRoles(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter)
        {
            var userClaims = _userClaimsAccessor.GetUserClaim();
            ManageProductMarketingCenter mg = new ManageProductMarketingCenter(userClaims);
            ListResponse response = mg.GetRoles(editorPersonaId, userPersonaId, datafilter);
            return Ok(response);
        }

        /// <summary>
        /// Used to get a list of properties
        /// </summary>
        /// <remarks>For now filtering and sorting will be done on the UI side</remarks>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter">A datafilter used to filter the properties. RightDescription or CenterName can be used</param>
        /// <returns></returns>
        [HttpGet("products/marketingcenter/properties")]
        [ProducesResponseType(typeof(ProductProperty), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetMarketingCenterProperties(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter)
        {
            var userClaims = _userClaimsAccessor.GetUserClaim();
            ManageProductMarketingCenter mg = new ManageProductMarketingCenter(userClaims);
            ListResponse response = mg.GetProperties(editorPersonaId, userPersonaId, datafilter);
            return Ok(response);
        }

        /// <summary>
        /// Used to create a new account for the given GreenBook user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId">The persona to use to find the login to update</param>
        /// <param name="rolepropList">Used to update the list of roles and properties given to a user</param>
        /// <returns></returns>
        [HttpPost("products/marketingcenter/user")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateMarketingCenterUser(long editorPersonaId, long userPersonaId, [FromBody] MarketingCenterRoleAndPropertyList rolepropList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0)
            {
                return BadRequest("Invalid editorPersonaId or userPersonaId");
            }

            if (rolepropList == null)
            {
                rolepropList = new MarketingCenterRoleAndPropertyList();
            }

            var userClaims = _userClaimsAccessor.GetUserClaim();
            ManageProductMarketingCenter mg = new ManageProductMarketingCenter(userClaims);

            // Convert out parameter to tuple return
            var (result, additionalParameters) = ManageMarketingCenterUserWrapper(
                mg,
                editorPersonaId,
                userPersonaId,
                rolepropList.RoleList,
                rolepropList.PropertyList,
                rolepropList.IsAssignedNewPropertyByDefault);

            if (string.IsNullOrEmpty(result))
            {
                return StatusCode((int)HttpStatusCode.Created);
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Used to update an existing account for the given GreenBook user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId">The persona to use to find the login to update</param>
        /// <param name="rolepropList">Used to update the list of roles and properties given to a user</param>
        /// <returns></returns>
        [HttpPut("products/marketingcenter/user")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateMarketingCenterUser(long editorPersonaId, long userPersonaId, [FromBody] MarketingCenterRoleAndPropertyList rolepropList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0)
            {
                return BadRequest("Invalid editorPersonaId or userPersonaId");
            }

            if (rolepropList == null)
            {
                rolepropList = new MarketingCenterRoleAndPropertyList();
            }

            var userClaims = _userClaimsAccessor.GetUserClaim();
            ManageProductMarketingCenter mg = new ManageProductMarketingCenter(userClaims);

            // Convert out parameter to tuple return
            var (result, additionalParameters) = ManageMarketingCenterUserWrapper(
                mg,
                editorPersonaId,
                userPersonaId,
                rolepropList.RoleList,
                rolepropList.PropertyList,
                rolepropList.IsAssignedNewPropertyByDefault);

            if (string.IsNullOrEmpty(result))
            {
                return Ok();
            }
            return BadRequest(result);
        }

        #endregion

        #region User-Status

        /// <summary>
        /// Disable the resident portal user.
        /// </summary>
        /// <param name="produtUser">The produt user.</param>
        /// <returns></returns>
        [HttpPut("products/marketingcenter/user/MT/status")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateMarketingCenterUserStatus([FromBody] ProductUser produtUser)
        {
            var userClaims = _userClaimsAccessor.GetUserClaim();
            var personaId = _userClaimsAccessor.PersonaId;
            var manageProductMarketingCenter = new ManageProductMarketingCenter(userClaims);

            if (!manageProductMarketingCenter.ChangeUserStatus(personaId, produtUser.UserName, produtUser.UserId.ToString()))
            {
                if (produtUser.IsAssigned)
                {
                    return BadRequest("Activate MarketingCenter user failed.");
                }
                return BadRequest("Deactivate MarketingCenter user failed.");
            }
            return Ok("Successfully disabled product user.");
        }

        #endregion

        #region Roles Rights Setup

        /// <summary>
        /// Used to get a list of roles
        /// </summary>
        /// <remarks>A datafilter can be used to filter the roles using name</remarks>
        /// <param name="editorPersonaId"></param>
        /// <param name="upfmId"></param>
        /// <returns></returns>
        [HttpGet("products/marketingcenter/rolescount")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetRolesCount(long editorPersonaId, Guid? upfmId = null)
        {
            var userClaims = _userClaimsAccessor.GetUserClaim();
            var manageProductMarketingCenter = _manageProductMarketingCenter;

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
                        RecreateClaimsForClient(AdminCreatorRealPageId, ref userClaims);
                        editorPersonaId = userClaims.PersonaId;
                        if (editorPersonaId == 0)
                        {
                            return BadRequest("invalid request.");
                        }
                        manageProductMarketingCenter = new ManageProductMarketingCenter(userClaims);
                    }
                }
            }

            ListResponse response = manageProductMarketingCenter.GetRolesCount(editorPersonaId);
            return !response.IsError ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Used to get a list of rights
        /// </summary>
        /// <remarks></remarks>
        /// <param name="editorPersonaId"></param>
        /// <returns></returns>
        [HttpGet("products/marketingcenter/rights")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetRights(long editorPersonaId)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("Invalid editorPersonaId");
            }

            var userClaims = _userClaimsAccessor.GetUserClaim();
            ManageProductMarketingCenter mc = new ManageProductMarketingCenter(userClaims);
            ListResponse response = mc.GetRights(editorPersonaId);
            return !response.IsError ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Disable the resident portal user.
        /// </summary>
        /// <param name="editorPersonaId">The editorPersonaId.</param>
        /// <param name="roleId">The roleId.</param>
        /// <returns></returns>
        [HttpDelete("products/marketingcenter/role")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DeleteMarketingCenterRole(long editorPersonaId, int roleId)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("Invalid editorPersonaId");
            }

            var userClaims = _userClaimsAccessor.GetUserClaim();
            ManageProductMarketingCenter mc = new ManageProductMarketingCenter(userClaims);
            return Ok(mc.DeleteRole(editorPersonaId, roleId));
        }

        /// <summary>
        /// Disable the MC portal user.
        /// </summary>
        /// <param name="roleId">The roleId.</param>
        /// <param name="isActive">The isActive.</param>
        /// <param name="editorPersonaId">The editorPersonaId.</param>
        /// <returns></returns>
        [HttpPost("products/marketingcenter/role/status")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateMarketingCenterRoleStatus(long editorPersonaId, int roleId, bool isActive)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("Invalid editorPersonaId");
            }

            var userClaims = _userClaimsAccessor.GetUserClaim();
            ManageProductMarketingCenter mc = new ManageProductMarketingCenter(userClaims);
            return Ok(mc.UpdateRoleStatus(editorPersonaId, roleId, isActive));
        }

        /// <summary>
        /// Get roles for the right id
        /// </summary>
        /// <param name="rightId">The rightId.</param>
        /// <param name="editorPersonaId">The editorPersonaId.</param>
        /// <returns></returns>
        [HttpGet("products/marketingcenter/right/roles")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetRolesForRightId(long editorPersonaId, int rightId)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("Invalid editorPersonaId");
            }

            var userClaims = _userClaimsAccessor.GetUserClaim();
            ManageProductMarketingCenter mc = new ManageProductMarketingCenter(userClaims);
            ListResponse response = mc.GetRolesForRightId(editorPersonaId, rightId);
            return !response.IsError ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Update roles for right
        /// </summary>
        /// <param name="rightId">The rightId.</param>
        /// <param name="roleList">The roleList.</param>
        /// <param name="editorPersonaId">The editorPersonaId.</param>
        /// <returns></returns>
        [HttpPut("products/marketingcenter/right/roles")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateRolesForRight(long editorPersonaId, int rightId, [FromBody] List<string> roleList)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("Invalid editorPersonaId");
            }

            var userClaims = _userClaimsAccessor.GetUserClaim();
            ManageProductMarketingCenter mc = new ManageProductMarketingCenter(userClaims);
            ListResponse response = mc.UpdateRolesForRight(editorPersonaId, rightId, roleList);
            if (!response.IsError)
                return Ok("Roles Updated");
            else
                return BadRequest(response);
        }

        /// <summary>
        /// Get Rights for roleId
        /// </summary>
        /// <param name="roleId">The roleId.</param>
        /// <param name="editorPersonaId">The editorPersonaId.</param>
        /// <param name="upfmId"></param>
        /// <returns></returns>
        [HttpGet("products/marketingcenter/role/allrights")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetRightsForRoleId(long editorPersonaId, int roleId = 0, Guid? upfmId = null)
        {
            var userClaims = _userClaimsAccessor.GetUserClaim();
            var manageProductMarketingCenter = _manageProductMarketingCenter;

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
                        RecreateClaimsForClient(AdminCreatorRealPageId, ref userClaims);
                        editorPersonaId = userClaims.PersonaId;
                        if (editorPersonaId == 0)
                        {
                            return BadRequest("invalid request.");
                        }
                        manageProductMarketingCenter = new ManageProductMarketingCenter(userClaims);
                    }
                }
            }

            ListResponse response = manageProductMarketingCenter.GetRightsForRoleId(editorPersonaId, roleId);
            return !response.IsError ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Create New Role with rights,
        /// Don't pass Id for new role
        /// </summary>
        /// <param name="mcRole">Role Object to save</param>
        /// <param name="editorPersonaId">The editorPersonaId.</param>
        /// <returns></returns>
        [HttpPost("products/marketingcenter/role")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateNewMCRoleWithRights(long editorPersonaId, [FromBody] MCRole mcRole)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("Invalid editorPersonaId");
            }

            var userClaims = _userClaimsAccessor.GetUserClaim();
            ManageProductMarketingCenter mc = new ManageProductMarketingCenter(userClaims);
            ListResponse response = mc.CreateNewMCRoleWithRights(editorPersonaId, mcRole);
            return Convert.ToString(response.Additional) == "RoleError" ? Ok(response) : (!response.IsError ? Ok(response) : BadRequest(response));
        }

        /// <summary>
        /// Update Role and rights
        /// </summary>
        /// <param name="mcRole">Role Object to save</param>
        /// <param name="editorPersonaId">The editorPersonaId.</param>
        /// <returns></returns>
        [HttpPut("products/marketingcenter/role")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateMCRoleWithRights(long editorPersonaId, [FromBody] MCRole mcRole)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("Invalid editorPersonaId");
            }

            var userClaims = _userClaimsAccessor.GetUserClaim();
            ManageProductMarketingCenter mc = new ManageProductMarketingCenter(userClaims);
            ListResponse response = mc.UpdateNewMCRoleWithRights(editorPersonaId, mcRole);

            HttpStatusCode responseStatus;
            if (Convert.ToString(response.Additional) == "RoleError")
            {
                responseStatus = HttpStatusCode.OK;
            }
            else
            {
                if (!response.IsError)
                {
                    responseStatus = HttpStatusCode.OK;
                }
                else
                {
                    responseStatus = HttpStatusCode.BadRequest;
                }
            }

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
        public IActionResult ListMarketingCenterMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            var userClaims = _userClaimsAccessor.GetUserClaim();
            ManagePersona managePersona = new ManagePersona(userClaims);
            var persona = managePersona.GetPersona(editorPersonaId);
            if (persona == null)
                return BadRequest("editorPersonaId not found.");

            userClaims.UserRealPageGuid = persona.RealPageId;
            var manageProductMarketingCenter = new ManageProductMarketingCenter(userClaims);
            var result = manageProductMarketingCenter.GetMigrationUsers(editorPersonaId, datafilter);
            if (!result.IsError)
                return Ok(result);
            else
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
        public IActionResult UpdateUsersMigrationStatus([FromBody] IList<MigrateUser> migrateUsers)
        {
            var userClaims = _userClaimsAccessor.GetUserClaim();
            var personaId = _userClaimsAccessor.PersonaId;
            var manageProductMarketingCenter = new ManageProductMarketingCenter(userClaims);
            return Ok(manageProductMarketingCenter.UpdateUsersMigrationStatus(personaId, migrateUsers));
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Used to recreate claims for client
        /// </summary>
        /// <param name="realpageUserId">RealPage UserId</param>
        /// <param name="userClaims">User claims to update</param>
        private void RecreateClaimsForClient(Guid realpageUserId, ref DefaultUserClaim userClaims)
        {
            if (string.IsNullOrEmpty(realpageUserId.ToString())) return;

            var rpCache = new RPObjectCache();

            var cacheKey = $"recreateClaimsForClient_{realpageUserId}";
            userClaims = rpCache.GetFromCache<DefaultUserClaim>(cacheKey, 180, () =>
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

        /// <summary>
        /// Wrapper method to handle out parameter conversion to tuple
        /// </summary>
        private (string result, List<AdditionalParameters> additionalParameters) ManageMarketingCenterUserWrapper(
            ManageProductMarketingCenter mg,
            long editorPersonaId,
            long userPersonaId,
            List<int> roleList,
            List<string> propertyList,
            bool isAssignedNewPropertyByDefault)
        {
            List<AdditionalParameters> additionalParameters;
            string result = mg.ManageMarketingCenterUser(editorPersonaId, userPersonaId, roleList, propertyList, isAssignedNewPropertyByDefault, out additionalParameters);
            return (result, additionalParameters);
        }

        #endregion
    }
}
