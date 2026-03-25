using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Product Invoker Controller - Generic product integration endpoint that routes to specific product implementations
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("products")]
    public class ProductInvokerController : BaseController
    {
        private readonly IIntegrationTypeFactory _integrationTypeFactory;
        private readonly IProductRepositoryAsync _productRepository;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userClaimsAccessor">Accessor for current authenticated user's claims</param>
        /// <param name="integrationTypeFactory">Factory for creating product integration types</param>
        /// <param name="productRepository">Repository for product data access</param>
        public ProductInvokerController(
            IUserClaimsAccessor userClaimsAccessor,
            IIntegrationTypeFactory integrationTypeFactory,
            IProductRepositoryAsync productRepository) : base(userClaimsAccessor)
        {
            _integrationTypeFactory = integrationTypeFactory ?? throw new ArgumentNullException(nameof(integrationTypeFactory));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        /// <summary>
        /// Returns Roles for given product and user
        /// </summary>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRoles(ProductEnum productType, long editorPersonaId, long subjectPersonaId, [FromQuery] RequestParameter dataFilter, CancellationToken cancellationToken = default)
        {
            ListResponse result;
            try
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");

                if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                    return BadRequest("RealPageId empty.");

                int productId = (int)productType;
                var integrationType = _integrationTypeFactory.GetIntegration(productId);
                result = integrationType.GetRoles(editorPersonaId, subjectPersonaId, 0, null, dataFilter);

                if (result.IsError)
                    return StatusCode((int)HttpStatusCode.Forbidden, result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                result = ex.InnerException is BlueBookException
                    ? new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message }
                    : new ListResponse { IsError = true, ErrorReason = "Internal server error." };
            }

            return StatusCode((int)HttpStatusCode.Forbidden, result);
        }

        /// <summary>
        /// Returns Rights for given roleid and user
        /// </summary>
        [HttpGet("role/rights")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRightsForRole(ProductEnum productType, long editorPersonaId, long subjectPersonaId, string roleId, [FromQuery] RequestParameter dataFilter, CancellationToken cancellationToken = default)
        {
            // TODO: This method doesn't appear to be in use
            ListResponse result;
            try
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");

                if (string.IsNullOrWhiteSpace(roleId) || roleId == "0")
                    return BadRequest("roleId not supplied.");

                if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                    return BadRequest("RealPageId empty.");

                int productId = (int)productType;
                var integrationType = _integrationTypeFactory.GetIntegration(productId);
                result = _integrationTypeFactory.GetIntegrationTypeForProductId(productId) == ProductIntegrationTypeEnum.StandardV1
                    ? integrationType.GetRightsForRole(editorPersonaId, subjectPersonaId, roleId, 0, false, dataFilter)
                    : integrationType.GetRightsForRole(editorPersonaId, subjectPersonaId, int.Parse(roleId), 0, false, dataFilter);

                if (result.IsError)
                    return StatusCode((int)HttpStatusCode.Forbidden, result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                result = ex.InnerException is BlueBookException
                    ? new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message }
                    : new ListResponse { IsError = true, ErrorReason = "Internal server error." };
            }

            return StatusCode((int)HttpStatusCode.Forbidden, result);
        }

        /// <summary>
        /// Returns Rights for given company and user
        /// </summary>
        [HttpGet("company/rights")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllRights(ProductEnum productType, long editorPersonaId, long subjectPersonaId, [FromQuery] RequestParameter dataFilter, CancellationToken cancellationToken = default)
        {
            // TODO: This endpoint appears to not be in use
            ListResponse result;
            try
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");

                if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                    return BadRequest("RealPageId empty.");

                int productId = (int)productType;
                var integrationType = _integrationTypeFactory.GetIntegration(productId);
                result = integrationType.GetAllRights(editorPersonaId, subjectPersonaId, dataFilter);

                if (result.IsError)
                    return StatusCode((int)HttpStatusCode.Forbidden, result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                result = ex.InnerException is BlueBookException
                    ? new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message }
                    : new ListResponse { IsError = true, ErrorReason = "Internal server error." };
            }

            return StatusCode((int)HttpStatusCode.Forbidden, result);
        }

        /// <summary>
        /// Returns properties
        /// </summary>
        [HttpGet("properties")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProperties(ProductEnum productType, long editorPersonaId, long subjectPersonaId, [FromQuery] RequestParameter dataFilter, CancellationToken cancellationToken = default)
        {
            ListResponse result;
            try
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");

                if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                    return BadRequest("RealPageId empty.");

                int productId = (int)productType;
                var integrationType = _integrationTypeFactory.GetIntegration(productId);
                result = integrationType.GetProperties(editorPersonaId, subjectPersonaId, dataFilter);

                if (result.IsError)
                    return StatusCode((int)HttpStatusCode.Forbidden, result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                result = ex.InnerException is BlueBookException
                    ? new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message }
                    : new ListResponse { IsError = true, ErrorReason = "Internal server error." };
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns product property groups / regions
        /// </summary>
        [HttpGet("propertygroups")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPropertyGroups(ProductEnum productType, long editorPersonaId, long subjectPersonaId, [FromQuery] RequestParameter dataFilter, string tabName = null, CancellationToken cancellationToken = default)
        {
            ListResponse result;
            try
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");

                if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                    return BadRequest("RealPageId empty.");

                int productId = (int)productType;
                var integrationType = _integrationTypeFactory.GetIntegration(productId);
                result = integrationType.GetPropertyGroups(editorPersonaId, subjectPersonaId, dataFilter);

                if (result.IsError && !string.IsNullOrEmpty(tabName))
                {
                    if (tabName == TabEnum.Area.ToString())
                        result.ErrorReason = "No areas available for this user.";

                    if (tabName == TabEnum.Region.ToString())
                        result.ErrorReason = "No regions available for this user.";
                }

                if (result.IsError)
                    return StatusCode((int)HttpStatusCode.Forbidden, result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                result = ex.InnerException is BlueBookException
                    ? new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message }
                    : new ListResponse { IsError = true, ErrorReason = "Internal server error." };
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns Properties for given property or region group
        /// </summary>
        [HttpGet("properties/{groupId}")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPropertiesByGroup(ProductEnum productType, long editorPersonaId, long subjectPersonaId, string groupId, [FromQuery] RequestParameter dataFilter, CancellationToken cancellationToken = default)
        {
            ListResponse result;
            try
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");

                if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                    return BadRequest("RealPageId empty.");

                if (string.IsNullOrEmpty(groupId))
                    return BadRequest("Group Id is required.");

                int productId = (int)productType;
                var integrationType = _integrationTypeFactory.GetIntegration(productId);
                result = integrationType.GetPropertiesByGroup(editorPersonaId, subjectPersonaId, groupId, dataFilter);

                if (result.IsError)
                    return StatusCode((int)HttpStatusCode.Forbidden, result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                result = ex.InnerException is BlueBookException
                    ? new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message }
                    : new ListResponse { IsError = true, ErrorReason = "Internal server error." };
            }

            return StatusCode((int)HttpStatusCode.Forbidden, result);
        }

        /// <summary>
        /// For a product, returns all organizations or by given organizationId (Used in ClickPay)
        /// </summary>
        [HttpGet("organizations")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProductOrganizations(ProductEnum productType, long editorPersonaId, long subjectPersonaId, string organizationRoleId, string organizationType, CancellationToken cancellationToken = default)
        {
            ListResponse result;
            try
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");

                if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                    return BadRequest("RealPageId empty.");

                int productId = (int)productType;
                var integrationType = _integrationTypeFactory.GetIntegration(productId);
                result = integrationType.GetOrganizations(editorPersonaId, subjectPersonaId, organizationRoleId, organizationType);

                if (result.IsError)
                    return StatusCode((int)HttpStatusCode.Forbidden, result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                result = ex.InnerException is BlueBookException
                    ? new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message }
                    : new ListResponse { IsError = true, ErrorReason = "Internal server error." };
            }

            return StatusCode((int)HttpStatusCode.Forbidden, result);
        }

        /// <summary>
        /// Returns product users for migration
        /// </summary>
        [HttpGet("users/migration")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListMigrationUsers(string productType, long editorPersonaId, [FromQuery] RequestParameter dataFilter, CancellationToken cancellationToken = default)
        {
            ListResponse result;
            try
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");

                var productList = await _productRepository.GetAllProductsAsync(cancellationToken);
                int productId = ProductEnumHelper.GetProductIdByProductCode(productType, productList);
                var integrationType = _integrationTypeFactory.GetIntegration(productId);
                result = integrationType.GetMigrationUsers(editorPersonaId, dataFilter);

                if (result.IsError)
                    return StatusCode((int)HttpStatusCode.Forbidden, result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                result = ex.InnerException is BlueBookException
                    ? new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message }
                    : new ListResponse { IsError = true, ErrorReason = "Internal server error." };
            }

            return StatusCode((int)HttpStatusCode.Forbidden, result);
        }

        /// <summary>
        /// Update migration status of users
        /// </summary>
        [HttpPatch("users/migrate")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateUsersMigrationStatus([FromQuery] string productType, [FromBody] IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        {
            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                return BadRequest("RealPageId empty.");

            var productList = await _productRepository.GetAllProductsAsync(cancellationToken);
            int productId = ProductEnumHelper.GetProductIdByProductCode(productType, productList);
            var integrationType = _integrationTypeFactory.GetIntegration(productId);
            var personaId = _userClaimsAccessor.PersonaId;
            var result = integrationType.UpdateUsersMigrationStatus(personaId, migrateUsers);

            if (!result.Status)
                return StatusCode((int)HttpStatusCode.Forbidden, result);

            return Ok(result);
        }

        /// <summary>
        /// Used to change product user profile without requiring green-book.
        /// </summary>
        [HttpPatch("users/externalprofilechange")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExternalProductUserProfileChange([FromQuery] string productType, [FromBody] ProductUserProfile productUserProfile, CancellationToken cancellationToken = default)
        {
            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                return BadRequest("RealPageId empty.");

            var productList = await _productRepository.GetAllProductsAsync(cancellationToken);
            int productId = ProductEnumHelper.GetProductIdByProductCode(productType, productList);
            var integrationType = _integrationTypeFactory.GetIntegration(productId);
            var personaId = _userClaimsAccessor.PersonaId;
            var result = integrationType.ExternalUserProfileChange(personaId, productUserProfile);

            if (result)
                return Ok("Successfully disabled product user.");

            return StatusCode((int)HttpStatusCode.Forbidden, "Failed to disabled product user.");
        }
    }
}
