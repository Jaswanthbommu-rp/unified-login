using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Controller for product panel related APIs
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("product")]
    public class ProductPanelController : ControllerBase
    {
        private readonly IUserClaimsAccessor _userClaimsAccessor;
        private readonly IManageProductPanel _manageProductPanel;
        private readonly IManagePersona _managePersona;
        private readonly IManageOrganization _manageOrganization;
        private readonly IManagePerson _managePerson;
        private readonly IManageUserLogin _manageUserLogin;
        private readonly IManageUserRoleRight _manageUserRoleRight;

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
            IManageUserRoleRight manageUserRoleRight)
        {
            _userClaimsAccessor = userClaimsAccessor ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
            _manageProductPanel = manageProductPanel ?? throw new ArgumentNullException(nameof(manageProductPanel));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _manageOrganization = manageOrganization ?? throw new ArgumentNullException(nameof(manageOrganization));
            _managePerson = managePerson ?? throw new ArgumentNullException(nameof(managePerson));
            _manageUserLogin = manageUserLogin ?? throw new ArgumentNullException(nameof(manageUserLogin));
            _manageUserRoleRight = manageUserRoleRight ?? throw new ArgumentNullException(nameof(manageUserRoleRight));
        }

        /// <summary>
        /// Returns Roles
        /// </summary>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRoles(long editorPersonaId, long userPersonaId, long partyId, int productId, [FromQuery] RequestParameter datafilter, AccessType? accessType = null)
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
                _manageProductPanel.GetProductRoles(currentEditorPersonaId, userPersonaId, partyId, productId, datafilter, accessType));

            if (result.IsError)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns user groups
        /// </summary>
        [HttpGet("usergroups")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUserGroups(long editorPersonaId, long userPersonaId, long partyId, int productId, [FromQuery] RequestParameter datafilter)
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
                _manageProductPanel.GetProductUserGroups(editorPersonaId, userPersonaId, partyId, productId, datafilter));

            if (result.IsError)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns user product Roles
        /// </summary>
        [HttpGet("userproductroles")]
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

            var result = await Task.Run(() =>
            {
                var persona = _managePersona.GetFirstAvailablePersonaByCompany(realPageId, partyId);
                if (persona == null || persona.PersonaId == 0)
                {
                    return null;
                }

                return _manageProductPanel.GetUserProductRoles(editorPersonaId, persona.PersonaId, partyId);
            });

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Get active persona: Invalid parameter enterprise User Id");
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns Rights
        /// </summary>
        [HttpGet("productrights")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRights(long editorPersonaId, long userPersonaId, long partyId, int productId, [FromQuery] RequestParameter datafilter)
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
                _manageProductPanel.GetProductRights(editorPersonaId, userPersonaId, partyId, productId, datafilter));

            if (result.IsError)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns Properties
        /// </summary>
        [HttpGet("properties")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProperties(long editorPersonaId, long userPersonaId, int productId, [FromQuery] RequestParameter datafilter)
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
                _manageProductPanel.GetProductProperties(editorPersonaId, userPersonaId, productId, datafilter));

            if (result.IsError)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns Properties (POST version with UPFM property translation)
        /// </summary>
        [HttpPost("properties")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPropertiesPost(long editorPersonaId, long userPersonaId, int productId, [FromQuery] RequestParameter datafilter, [FromBody] UPFMProperty upfmProperty, bool? donotTranslate = null)
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

                if (!response.IsError)
                {
                    response = _manageProductPanel.CompareProductAndPrimaryProperties(upfmProperty, productId, response);
                }

                return response;
            });

            if (result.IsError)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get Persona Product Primary Properties
        /// </summary>
        [HttpGet("productPrimaryProperties")]
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

            var result = await Task.Run(() =>
                _manageProductPanel.GetPersonaProductPrimaryProperties(userPersonaId));

            return Ok(result);
        }

        /// <summary>
        /// Returns translated properties (Obsolete)
        /// </summary>
        [HttpPost("{productId}/translateProductProperties")]
        [Obsolete]
        [ProducesResponseType(typeof(UPFMProperty), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTranslatedProperties([FromBody] UPFMProperty upfmProperty, int productId)
        {
            var result = await Task.Run(() =>
            {
                if (upfmProperty?.id != null)
                {
                    return _manageProductPanel.TranslateProductProperties(upfmProperty, productId);
                }
                return new UPFMProperty();
            });

            return Ok(result);
        }

        /// <summary>
        /// Returns Property groups
        /// </summary>
        [HttpGet("propertygroups")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPropertyGroups(long editorPersonaId, long userPersonaId, int productId, [FromQuery] RequestParameter datafilter)
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
                _manageProductPanel.GetProductPropertyGroups(currentEditorPersonaId, userPersonaId, productId, datafilter));

            if (result.IsError)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns Rights for role
        /// </summary>
        [HttpGet("rightsforrole")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRightsForRole(long editorPersonaId, int productId, string roleId, long partyId, [FromQuery] RequestParameter datafilter, bool assignedToRoleOnly = false)
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

            var result = await Task.Run(() =>
                _manageProductPanel.GetProductRightsForRole(editorPersonaId, roleId, partyId, productId, datafilter, assignedToRoleOnly));

            if (result.IsError)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns group Properties
        /// </summary>
        [HttpGet("groupproperties")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProductGroupProperties(long editorPersonaId, long userPersonaId, int productId, string propertyGroupId, [FromQuery] RequestParameter datafilter)
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

            var result = await Task.Run(() =>
                _manageProductPanel.GetProductGroupProperties(editorPersonaId, userPersonaId, productId, propertyGroupId, datafilter));

            if (result.IsError)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns product organizations
        /// </summary>
        [HttpGet("organizations")]
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

            var result = await Task.Run(() =>
                _manageProductPanel.GetProductOrganizations(editorPersonaId, userPersonaId, productId, organizationRoleId, organizationType));

            if (result.IsError)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns Location groups
        /// </summary>
        [HttpGet("locationgroups")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLocationGroups(long editorPersonaId, long userPersonaId, int productId, [FromQuery] RequestParameter datafilter)
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
                _manageProductPanel.GetProductLocationGroups(editorPersonaId, userPersonaId, productId, datafilter));

            if (result.IsError)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        #region Private Methods

        /// <summary>
        /// Helper method to get support user details and change context
        /// </summary>
        private async Task<long> GetSupportUserDetailsAndChangeContextAsync(RequestParameter datafilter)
        {
            return await Task.Run(() =>
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

                // Note: In the new DI model, we would need to re-create the service with new claims
                // For now, we return the persona ID and let the caller handle context changes
                var person = _managePerson.GetPerson(adminCreatorRealPageId);
                if (person == null)
                {
                    return 0;
                }

                var persona = _managePersona.GetActivePersonaWithoutRights(adminCreatorRealPageId);
                datafilter.FilterBy.Remove("upfmid");
                return persona?.PersonaId ?? 0;
            });
        }

        #endregion
    }
}
