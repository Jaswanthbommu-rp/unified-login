using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Product Asset Optimization Controller - Handles APIs for all sub-products under Asset Optimization
    /// including companies, property groups, roles, and migration functionality
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("products/ao")]
    public class ProductAssetOptimizationController : BaseController
    {
        private readonly IManageProductAssetOptimizationAsync _manageProductAo;
        private readonly IManagePersonaAsync _managePersona;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProductAssetOptimizationController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageProductAssetOptimizationAsync manageProductAo,
            IManagePersonaAsync managePersona) : base(userClaimsAccessor)
        {
            _manageProductAo = manageProductAo ?? throw new ArgumentNullException(nameof(manageProductAo));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
        }

        /// <summary>
        /// Returns Companies
        /// </summary>
        [HttpGet("companies")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCompanies(long editorPersonaId, long userPersonaId, string productName, [FromQuery] RequestParameter datafilter, string userLoginName = "", CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var result = await _manageProductAo.GetCompaniesAsync(editorPersonaId, userPersonaId, productName, datafilter, userLoginName, cancellationToken);

            if (result?.IsError == true)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns assignable or assigned Property Groups for user
        /// </summary>
        [HttpGet("propertygroups")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPropertyGroups(long editorPersonaId, long userPersonaId, string productName, [FromQuery] IList<string> selectedCompanies, [FromQuery] RequestParameter datafilter, string userLoginName = "", CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var result = await _manageProductAo.GetPropertyGroupsAsync(editorPersonaId, userPersonaId, productName, selectedCompanies, cancellationToken);

            if (result?.IsError == true)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns properties in group
        /// </summary>
        [HttpGet("groupproperties")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPropertiesInGroups(long editorPersonaId, long userPersonaId, int propertyGroupId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var result = await _manageProductAo.GetPropertiesInGroupAsync(editorPersonaId, userPersonaId, propertyGroupId, cancellationToken);

            if (result?.IsError == true)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns companies and associated roles for a product
        /// </summary>
        [HttpGet("companyroles")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCompaniesWithRoles(long editorPersonaId, long userPersonaId, string productName, [FromQuery] RequestParameter datafilter, string userLoginName = "", CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var result = await _manageProductAo.GetCompaniesWithRolesAsync(editorPersonaId, userPersonaId, productName, datafilter, userLoginName, cancellationToken);

            if (result?.IsError == true)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns companies and associated properties for a product
        /// </summary>
        [HttpGet("companyproperties")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCompaniesWithProperties(long editorPersonaId, long userPersonaId, string productName, [FromQuery] RequestParameter datafilter, string userLoginName = "", CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var result = await _manageProductAo.GetCompaniesWithPropertiesAsync(editorPersonaId, userPersonaId, productName, datafilter, userLoginName, cancellationToken);

            if (result?.IsError == true)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns operators with properties
        /// </summary>
        [HttpGet("operatorproperties")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOperatorsWithProperties(long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var result = await _manageProductAo.GetOperatorsAsync(editorPersonaId, userPersonaId, cancellationToken);

            if (result?.IsError == true)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns properties filtered by operators
        /// </summary>
        [HttpGet("propertiesbyoperators")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPropertiesWithOperators(long editorPersonaId, long userPersonaId, string operatorCode, string operatorValue, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var result = await _manageProductAo.GetPropertiesWithOperatorsAsync(editorPersonaId, userPersonaId, operatorCode, operatorValue, cancellationToken);

            if (result?.IsError == true)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates the AO user status (activate/deactivate)
        /// </summary>
        [HttpPut("user/MT/status")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateAOUserStatus([FromBody] ProductUser productUser, CancellationToken cancellationToken = default)
        {
            var result = await _manageProductAo.ChangeUserStatusAsync(
                _userClaimsAccessor.PersonaId, productUser.UserName, productUser.FirstName, productUser.LastName, cancellationToken);

            if (!result)
            {
                return BadRequest(productUser.IsAssigned ? "Activate ao user failed." : "Deactivate ao user failed.");
            }

            return Ok("Successfully disabled product user.");
        }

        #region Migration API

        /// <summary>
        /// Returns product users of an organization for migration purposes
        /// </summary>
        [HttpGet("migration-users")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListAssetOptimizationMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
            {
                return BadRequest("editorPersonaId not supplied.");
            }

            var persona = await _managePersona.GetPersonaAsync(editorPersonaId, withRights: false, cancellationToken);
            if (persona == null)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new { IsError = true, ErrorMessage = "editorPersonaId not found." });
            }

            var result = await _manageProductAo.GetMigrationUsersAsync(editorPersonaId, datafilter, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Update migration status of users
        /// </summary>
        [HttpPut("migrate-users")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateUsersMigrationStatus([FromBody] IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        {
            var result = await _manageProductAo.UpdateUsersMigrationStatusAsync(
                _userClaimsAccessor.PersonaId, migrateUsers, cancellationToken);
            return Ok(result);
        }

        #endregion
    }
}
