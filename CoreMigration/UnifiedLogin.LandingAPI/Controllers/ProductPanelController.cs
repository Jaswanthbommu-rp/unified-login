using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Controller for product panel related APIs
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("")]
    public class ProductPanelController : BaseController
    {
        private IManageProductPanel _manageProductPanel;
        private readonly IManagePersona _managePersona;
        private readonly IManageOrganization _manageOrganization;
        private readonly IManagePerson _managePerson;
        private readonly IManageUserLogin _manageUserLogin;
        private readonly IManageUserRoleRight _manageUserRoleRight;
        private readonly IDistributedCache? _distributedCache;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProductPanelController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageProductPanel manageProductPanel,
            IManagePersona managePersona,
            IManageOrganization manageOrganization,
            IManagePerson managePerson,
            IManageUserLogin manageUserLogin,
            IManageUserRoleRight manageUserRoleRight,
            IDistributedCache? distributedCache = null) : base(userClaimsAccessor)
        {
            _manageProductPanel = manageProductPanel ?? throw new ArgumentNullException(nameof(manageProductPanel));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _manageOrganization = manageOrganization ?? throw new ArgumentNullException(nameof(manageOrganization));
            _managePerson = managePerson ?? throw new ArgumentNullException(nameof(managePerson));
            _manageUserLogin = manageUserLogin ?? throw new ArgumentNullException(nameof(manageUserLogin));
            _manageUserRoleRight = manageUserRoleRight ?? throw new ArgumentNullException(nameof(manageUserRoleRight));
            _distributedCache = distributedCache; // optional; when null, skip caching
        }

        /// <summary>
        /// Returns Roles
        /// </summary>
        [HttpGet("product/roles")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRoles(long editorPersonaId, long userPersonaId, long partyId, int productId, [FromQuery] RequestParameter? datafilter, AccessType? accessType = null)
        {
            var currentEditorPersonaId = editorPersonaId;
            datafilter = new RequestParameter() { FilterBy = new Dictionary<string, string>() { } };
            datafilter.FilterBy.Add("upfmid", "f5c090fa-78ab-452f-b504-98aafee09121");

            if (currentEditorPersonaId == 0)
            {
                if (datafilter == null || !datafilter.FilterBy.ContainsKey("upfmid"))
                {
                    return BadRequest("editorPersonaId not supplied.");
                }

                ClaimsPrincipal currentClaimPrincipal = HttpContext.User;
                if (datafilter.FilterBy.ContainsKey("upfmid") && currentClaimPrincipal.HasClaim("scope", "internalapi"))
                {
                    currentEditorPersonaId = await GetSupportUserDetailsAndChangeContextAsync(datafilter);
                    if (currentEditorPersonaId == 0)
                    {
                        return BadRequest("invalid request.");
                    }
                }
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var result = _manageProductPanel.GetProductRoles(currentEditorPersonaId, userPersonaId, partyId, productId, datafilter, accessType);

            return Ok(result);
        }

        /// <summary>
        /// Returns user groups
        /// </summary>
        [HttpGet("product/usergroups")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUserGroups(long editorPersonaId, long userPersonaId, long partyId, int productId, [FromQuery] RequestParameter? datafilter)
        {
            datafilter ??= new RequestParameter();
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var result = await Task.Run(() =>
                _manageProductPanel.GetProductUserGroups(editorPersonaId, userPersonaId, partyId, productId, datafilter));

            return Ok(result);
        }

        /// <summary>
        /// Returns user product Roles
        /// </summary>
        [HttpGet("product/userproductroles")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUserProductRoles(long editorPersonaId, long partyId, Guid realPageId)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            if (realPageId == Guid.Empty)
            {
                return BadRequest("User RealPageId empty.");
            }

            var persona = _managePersona.GetFirstAvailablePersonaByCompany(realPageId, partyId);
            if ((persona == null) || (persona.PersonaId == 0))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Get active persona: Invalid parameter enterprise User Id");
            }

            var userProductRoles = _manageProductPanel.GetUserProductRoles(editorPersonaId, persona.PersonaId, partyId);

            return Ok(userProductRoles);
        }

        /// <summary>
        /// Returns Rights
        /// </summary>
        [HttpGet("product/productrights")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRights(long editorPersonaId, long userPersonaId, long partyId, int productId, [FromQuery] RequestParameter? datafilter)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }
            datafilter ??= new RequestParameter();

            var result = _manageProductPanel.GetProductRights(editorPersonaId, userPersonaId, partyId, productId, datafilter);

            return Ok(result);
        }

        /// <summary>
        /// Returns Properties
        /// </summary>
        [HttpGet("product/properties")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProperties(long editorPersonaId, long userPersonaId, int productId, [FromQuery] RequestParameter? datafilter)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }
            datafilter ??= new RequestParameter();
            var result = _manageProductPanel.GetProductProperties(editorPersonaId, userPersonaId, productId, datafilter);

            return Ok(result);
        }

        /// <summary>
        /// Returns Properties (POST version with UPFM property translation)
        /// </summary>
        [HttpPost("product/properties")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.UnsupportedMediaType)]
        public async Task<IActionResult> GetPropertiesPost(long editorPersonaId, long userPersonaId, int productId, [FromQuery] RequestParameter? datafilter, [FromBody] UPFMProperty? upfmProperty, bool? donotTranslate = null)
        {
            var currentEditorPersonaId = editorPersonaId;

            if (currentEditorPersonaId == 0)
            {
                if (datafilter == null || !datafilter.FilterBy.ContainsKey("upfmid"))
                {
                    return BadRequest("editorPersonaId not supplied.");
                }

                ClaimsPrincipal currentClaimPrincipal = HttpContext.User;
                if (datafilter.FilterBy.ContainsKey("upfmid") && currentClaimPrincipal.HasClaim("scope", "internalapi"))
                {
                    currentEditorPersonaId = await GetSupportUserDetailsAndChangeContextAsync(datafilter);
                    if (currentEditorPersonaId == 0)
                    {
                        return BadRequest("invalid request.");
                    }
                }
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var result = await Task.Run(() =>
            {
                var response = _manageProductPanel.GetProductProperties(currentEditorPersonaId, userPersonaId, productId, datafilter);

                if (donotTranslate ?? false)
                {
                    return response;
                }

                if (!response.IsError && upfmProperty != null)
                {
                    response = _manageProductPanel.CompareProductAndPrimaryProperties(upfmProperty, productId, response);
                }

                return response;
            });

            return Ok(result);
        }

        /// <summary>
        /// Get Persona Product Primary Properties
        /// </summary>
        [HttpGet("product/productPrimaryProperties")]
        [ProducesResponseType(typeof(List<PersonaProductProperty>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPersonaProductPrimaryProperties(long userPersonaId)
        {
            if (userPersonaId == 0)
            {
                return BadRequest("userPersonaId not supplied.");
            }

            var result = _manageProductPanel.GetPersonaProductPrimaryProperties(userPersonaId);
            return Ok(result);
        }

        /// <summary>
        /// Returns translated properties (Obsolete)
        /// </summary>
        [HttpPost("product/{productId}/translateProductProperties")]
        [Obsolete]
        [ProducesResponseType(typeof(UPFMProperty), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTranslatedProperties([FromBody] UPFMProperty upfmProperty, int productId)
        {
            var result = new UPFMProperty();
            if (upfmProperty?.id != null)
            {
                result = _manageProductPanel.TranslateProductProperties(upfmProperty, productId);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns Property groups
        /// </summary>
        [HttpGet("product/propertygroups")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPropertyGroups(long editorPersonaId, long userPersonaId, int productId, [FromQuery] RequestParameter? datafilter)
        {
            var currentEditorPersonaId = editorPersonaId;

            if (currentEditorPersonaId == 0)
            {
                if (datafilter == null || !datafilter.FilterBy.ContainsKey("upfmid"))
                {
                    return BadRequest("editorPersonaId not supplied.");
                }

                ClaimsPrincipal currentClaimPrincipal = HttpContext.User;
                if (datafilter.FilterBy.ContainsKey("upfmid") && currentClaimPrincipal.HasClaim("scope", "internalapi"))
                {
                    currentEditorPersonaId = await GetSupportUserDetailsAndChangeContextAsync(datafilter);
                    if (currentEditorPersonaId == 0)
                    {
                        return BadRequest("invalid request.");
                    }
                }
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var result = _manageProductPanel.GetProductPropertyGroups(currentEditorPersonaId, userPersonaId, productId, datafilter);
            return Ok(result);
        }

        /// <summary>
        /// Returns Rights for role
        /// </summary>
        [HttpGet("product/rightsforrole")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRightsForRole(long editorPersonaId, int productId, string roleId, long partyId, [FromQuery] RequestParameter? datafilter, bool assignedToRoleOnly = false)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            if (string.IsNullOrWhiteSpace(roleId) || roleId == "0")
            {
                return BadRequest("roleId not supplied.");
            }
            datafilter ??= new RequestParameter();
            var result = _manageProductPanel.GetProductRightsForRole(editorPersonaId, roleId, partyId, productId, datafilter, assignedToRoleOnly);
            return Ok(result);
        }

        /// <summary>
        /// Returns group Properties
        /// </summary>
        [HttpGet("product/groupproperties")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProductGroupProperties(long editorPersonaId, long userPersonaId, int productId, string propertyGroupId, [FromQuery] RequestParameter? datafilter)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            if (string.IsNullOrEmpty(propertyGroupId) || propertyGroupId == "0")
            {
                return BadRequest("Invalid Group Id.");
            }
            datafilter ??= new RequestParameter();
            var result = _manageProductPanel.GetProductGroupProperties(editorPersonaId, userPersonaId, productId, propertyGroupId, datafilter);
            return Ok(result);
        }

        /// <summary>
        /// Returns product organizations
        /// </summary>
        [HttpGet("product/organizations")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProductOrganizations(long editorPersonaId, long userPersonaId, int productId, string organizationRoleId, string organizationType)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            if (string.IsNullOrEmpty(organizationRoleId))
            {
                return BadRequest("Invalid Organization Role Id.");
            }

            if (string.IsNullOrEmpty(organizationType))
            {
                return BadRequest("Invalid Organization Type");
            }

            var result = _manageProductPanel.GetProductOrganizations(editorPersonaId, userPersonaId, productId, organizationRoleId, organizationType);
            return Ok(result);
        }

        /// <summary>
        /// Returns Location groups
        /// </summary>
        [HttpGet("product/locationgroups")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLocationGroups(long editorPersonaId, long userPersonaId, int productId, [FromQuery] RequestParameter? datafilter)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }
            datafilter ??= new RequestParameter();
            var result = _manageProductPanel.GetProductLocationGroups(editorPersonaId, userPersonaId, productId, datafilter);
            return Ok(result);
        }

        #region Private Methods

        /// <summary>
        /// Helper method to get support user details and change context
        /// </summary>
        private async Task<long> GetSupportUserDetailsAndChangeContextAsync(RequestParameter datafilter)
        {
            if (!Guid.TryParse(datafilter.FilterBy["upfmid"], out Guid upfmId))
            {
                return 0;
            }
            var adminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(upfmId);
            if (adminCreatorRealPageId == Guid.Empty)
            {
                return 0;
            }
            RecreateClaimsForClient(adminCreatorRealPageId);
            _manageProductPanel = new ManageProductPanel(_userClaimsAccessor.GetUserClaim());
            datafilter.FilterBy.Remove("upfmid");
            return _userClaimsAccessor.GetUserClaim().PersonaId;
        }

        private void RecreateClaimsForClient(Guid realpageUserId)
        {
            if (string.IsNullOrEmpty(realpageUserId.ToString())) return;
            var cacheKey = $"recreateClaimsForClient_{realpageUserId}";

            var cachedBytes = _distributedCache?.Get(cacheKey);
            if (cachedBytes != null)
            {
                var cachedClaim = JsonSerializer.Deserialize<DefaultUserClaim>(cachedBytes);
                if (cachedClaim != null)
                {
                    _userClaimsAccessor.UserClaim = cachedClaim;
                    return;
                }
            }

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
                UserRealPageGuid = realpageUserId,
                CorrelationId = Guid.NewGuid(),
                RealPageEmployee = persona.Organization.RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId,
            };
            ClaimsPrincipal userPrincipal = HttpContext.User;
            var identity = (ClaimsIdentity)userPrincipal.Identity;
            identity.AddClaims(roles.Select(r => new Claim("roleid", r.RoleID.ToString())).ToList());

            claim.Rights = BaseUserRights.GetUserRightsBy(userPrincipal, claim);
            _userClaimsAccessor.UserClaim = claim;

            var bytes = JsonSerializer.SerializeToUtf8Bytes(claim);
            if (_distributedCache != null)
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(180)
                };
                _distributedCache.Set(cacheKey, bytes, options);
            }
        }

        #endregion
    }
}
