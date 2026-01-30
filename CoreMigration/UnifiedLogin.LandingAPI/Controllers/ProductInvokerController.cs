using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Product Invoker Controller - Generic product integration endpoint that routes to specific product implementations
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("products")]
    public class ProductInvokerController : ControllerBase
    {
        private readonly IUserClaimsAccessor _userClaimsAccessor;
        private readonly IIntegrationTypeFactory _integrationTypeFactory;
        private readonly IProductRepository _productRepository;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userClaimsAccessor">Accessor for current authenticated user's claims</param>
        /// <param name="integrationTypeFactory">Factory for creating product integration types</param>
        /// <param name="productRepository">Repository for product data access</param>
        public ProductInvokerController(
            IUserClaimsAccessor userClaimsAccessor,
            IIntegrationTypeFactory integrationTypeFactory,
            IProductRepository productRepository)
        {
            _userClaimsAccessor = userClaimsAccessor ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
            _integrationTypeFactory = integrationTypeFactory ?? throw new ArgumentNullException(nameof(integrationTypeFactory));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        /// <summary>
        /// Returns Roles for given product and user
        /// </summary>
        /// <param name="productType">Product Type</param>
        /// <param name="editorPersonaId">Editor user persona Id</param>
        /// <param name="subjectPersonaId">Subject user persona id</param>
        /// <param name="dataFilter">A dataFilter used to filter the roles</param>
        /// <returns>List of roles</returns>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRoles(ProductEnum productType, long editorPersonaId, long subjectPersonaId, [FromQuery] RequestParameter dataFilter)
        {
            ListResponse result;
            try
            {
                if (editorPersonaId == 0)
                {
                    return BadRequest("editorPersonaId not supplied.");
                }

                if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                {
                    return BadRequest("RealPageId empty.");
                }

                result = await Task.Run(() =>
                {
                    int productId = (int)productType;
                    var integrationType = _integrationTypeFactory.GetIntegration(productId);
                    return integrationType.GetRoles(editorPersonaId, subjectPersonaId, 0, null, dataFilter);
                });

                if (result.IsError)
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is BlueBookException)
                {
                    result = new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message };
                }
                else
                {
                    result = new ListResponse { IsError = true, ErrorReason = "Internal server error." };
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, result);
        }

        /// <summary>
        /// Returns Rights for given roleid and user
        /// </summary>
        /// <param name="productType">Product Type</param>
        /// <param name="editorPersonaId">Editor user persona Id</param>
        /// <param name="subjectPersonaId">Subject user persona id</param>
        /// <param name="roleId">Role unique Id</param>
        /// <param name="dataFilter">A dataFilter used to filter the roles</param>
        /// <returns>List of rights for the role</returns>
        [HttpGet("role/rights")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRightsForRole(ProductEnum productType, long editorPersonaId, long subjectPersonaId, string roleId, [FromQuery] RequestParameter dataFilter)
        {
            // TODO: This method doesn't appear to be in use
            ListResponse result;
            try
            {
                if (editorPersonaId == 0)
                {
                    return BadRequest("editorPersonaId not supplied.");
                }

                if (string.IsNullOrWhiteSpace(roleId) || roleId == "0")
                {
                    return BadRequest("roleId not supplied.");
                }

                if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                {
                    return BadRequest("RealPageId empty.");
                }

                result = await Task.Run(() =>
                {
                    int productId = (int)productType;
                    var integrationType = _integrationTypeFactory.GetIntegration(productId);
                    if (_integrationTypeFactory.GetIntegrationTypeForProductId(productId) == ProductIntegrationTypeEnum.StandardV1)
                    {
                        return integrationType.GetRightsForRole(editorPersonaId, subjectPersonaId, roleId, 0, false, dataFilter);
                    }
                    else
                    {
                        return integrationType.GetRightsForRole(editorPersonaId, subjectPersonaId, int.Parse(roleId), 0, false, dataFilter);
                    }
                });

                if (result.IsError)
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is BlueBookException)
                {
                    result = new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message };
                }
                else
                {
                    result = new ListResponse { IsError = true, ErrorReason = "Internal server error." };
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, result);
        }

        /// <summary>
        /// Returns Rights for given company and user
        /// </summary>
        /// <param name="productType">Product Type</param>
        /// <param name="editorPersonaId">Editor user persona Id</param>
        /// <param name="subjectPersonaId">Subject user persona id</param>
        /// <param name="dataFilter">A dataFilter used to filter the roles</param>
        /// <returns>All rights for the company</returns>
        [HttpGet("company/rights")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllRights(ProductEnum productType, long editorPersonaId, long subjectPersonaId, [FromQuery] RequestParameter dataFilter)
        {
            // TODO: This endpoint appears to not be in use
            ListResponse result;
            try
            {
                if (editorPersonaId == 0)
                {
                    return BadRequest("editorPersonaId not supplied.");
                }

                if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                {
                    return BadRequest("RealPageId empty.");
                }

                result = await Task.Run(() =>
                {
                    int productId = (int)productType;
                    var integrationType = _integrationTypeFactory.GetIntegration(productId);
                    return integrationType.GetAllRights(editorPersonaId, subjectPersonaId, dataFilter);
                });

                if (result.IsError)
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is BlueBookException)
                {
                    result = new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message };
                }
                else
                {
                    result = new ListResponse { IsError = true, ErrorReason = "Internal server error." };
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, result);
        }

        /// <summary>
        /// Returns properties
        /// </summary>
        /// <param name="productType">Product Type</param>
        /// <param name="editorPersonaId">Editor user persona Id</param>
        /// <param name="subjectPersonaId">Subject user persona id</param>
        /// <param name="dataFilter">A dataFilter used to filter the roles</param>
        /// <returns>List of properties</returns>
        [HttpGet("properties")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProperties(ProductEnum productType, long editorPersonaId, long subjectPersonaId, [FromQuery] RequestParameter dataFilter)
        {
            ListResponse result;
            try
            {
                if (editorPersonaId == 0)
                {
                    return BadRequest("editorPersonaId not supplied.");
                }

                if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                {
                    return BadRequest("RealPageId empty.");
                }

                result = await Task.Run(() =>
                {
                    int productId = (int)productType;
                    var integrationType = _integrationTypeFactory.GetIntegration(productId);
                    return integrationType.GetProperties(editorPersonaId, subjectPersonaId, dataFilter);
                });

                if (result.IsError)
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is BlueBookException)
                {
                    result = new ListResponse
                    {
                        IsError = true,
                        ErrorReason = ex.InnerException.Message
                    };
                }
                else
                {
                    result = new ListResponse { IsError = true, ErrorReason = "Internal server error." };
                }
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns product property groups / regions
        /// </summary>
        /// <param name="productType">Product Type</param>
        /// <param name="editorPersonaId">Editor user persona Id</param>
        /// <param name="subjectPersonaId">Subject user persona id</param>
        /// <param name="dataFilter">A dataFilter used to filter the roles</param>
        /// <param name="tabName">Tab Name</param>
        /// <returns>List of property groups</returns>
        [HttpGet("propertygroups")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPropertyGroups(ProductEnum productType, long editorPersonaId, long subjectPersonaId, [FromQuery] RequestParameter dataFilter, string tabName = null)
        {
            ListResponse result;
            try
            {
                if (editorPersonaId == 0)
                {
                    return BadRequest("editorPersonaId not supplied.");
                }

                if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                {
                    return BadRequest("RealPageId empty.");
                }

                result = await Task.Run(() =>
                {
                    int productId = (int)productType;
                    var integrationType = _integrationTypeFactory.GetIntegration(productId);
                    var response = integrationType.GetPropertyGroups(editorPersonaId, subjectPersonaId, dataFilter);

                    if (response.IsError && !string.IsNullOrEmpty(tabName))
                    {
                        if (tabName == TabEnum.Area.ToString())
                        {
                            response.ErrorReason = "No areas available for this user.";
                        }

                        if (tabName == TabEnum.Region.ToString())
                        {
                            response.ErrorReason = "No regions available for this user.";
                        }
                    }

                    return response;
                });

                if (result.IsError)
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is BlueBookException)
                {
                    result = new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message };
                }
                else
                {
                    result = new ListResponse { IsError = true, ErrorReason = "Internal server error." };
                }
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns Properties for given property or region group
        /// </summary>
        /// <param name="productType">Product Type</param>
        /// <param name="editorPersonaId">Editor user persona Id</param>
        /// <param name="subjectPersonaId">Subject user persona id</param>
        /// <param name="groupId">Group or region Id</param>
        /// <param name="dataFilter">A dataFilter used to filter the roles</param>
        /// <returns>Properties in the group</returns>
        [HttpGet("properties/{groupId}")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPropertiesByGroup(ProductEnum productType, long editorPersonaId, long subjectPersonaId, string groupId, [FromQuery] RequestParameter dataFilter)
        {
            ListResponse result;
            try
            {
                if (editorPersonaId == 0)
                {
                    return BadRequest("editorPersonaId not supplied.");
                }

                if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                {
                    return BadRequest("RealPageId empty.");
                }

                if (string.IsNullOrEmpty(groupId))
                {
                    return BadRequest("Group Id is required.");
                }

                result = await Task.Run(() =>
                {
                    int productId = (int)productType;
                    var integrationType = _integrationTypeFactory.GetIntegration(productId);
                    return integrationType.GetPropertiesByGroup(editorPersonaId, subjectPersonaId, groupId, dataFilter);
                });

                if (result.IsError)
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is BlueBookException)
                {
                    result = new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message };
                }
                else
                {
                    result = new ListResponse { IsError = true, ErrorReason = "Internal server error." };
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, result);
        }

        /// <summary>
        /// For a product, returns all organizations or by given organizationId (Used in ClickPay)
        /// </summary>
        /// <param name="productType">Product Type</param>
        /// <param name="editorPersonaId">Editor user persona Id</param>
        /// <param name="subjectPersonaId">Subject user persona id</param>
        /// <param name="organizationRoleId">Role id for organization</param>
        /// <param name="organizationType">Organization type- site, owner, company etc</param>
        /// <returns>List of organizations</returns>
        [HttpGet("organizations")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProductOrganizations(ProductEnum productType, long editorPersonaId, long subjectPersonaId, string organizationRoleId, string organizationType)
        {
            ListResponse result;
            try
            {
                if (editorPersonaId == 0)
                {
                    return BadRequest("editorPersonaId not supplied.");
                }

                if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                {
                    return BadRequest("RealPageId empty.");
                }

                result = await Task.Run(() =>
                {
                    int productId = (int)productType;
                    var integrationType = _integrationTypeFactory.GetIntegration(productId);
                    return integrationType.GetOrganizations(editorPersonaId, subjectPersonaId, organizationRoleId, organizationType);
                });

                if (result.IsError)
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is BlueBookException)
                {
                    result = new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message };
                }
                else
                {
                    result = new ListResponse { IsError = true, ErrorReason = "Internal server error." };
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, result);
        }

        /// <summary>
        /// Returns product users for migration
        /// </summary>
        /// <param name="productType">Product Books Code</param>
        /// <param name="editorPersonaId">Editor user persona Id</param>
        /// <param name="dataFilter">A datafilter used to filter the roles</param>
        /// <returns>List of migration users</returns>
        [HttpGet("users/migration")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListMigrationUsers(string productType, long editorPersonaId, [FromQuery] RequestParameter dataFilter)
        {
            ListResponse result;
            try
            {
                if (editorPersonaId == 0)
                {
                    return BadRequest("editorPersonaId not supplied.");
                }

                result = await Task.Run(() =>
                {
                    var productList = _productRepository.GetAllProducts();
                    int productId = ProductEnumHelper.GetProductIdByProductCode(productType, productList);

                    var integrationType = _integrationTypeFactory.GetIntegration(productId);
                    return integrationType.GetMigrationUsers(editorPersonaId, dataFilter);
                });

                if (result.IsError)
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is BlueBookException)
                {
                    result = new ListResponse
                    {
                        IsError = true,
                        ErrorReason = ex.InnerException.Message
                    };
                }
                else
                {
                    result = new ListResponse { IsError = true, ErrorReason = "Internal server error." };
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, result);
        }

        /// <summary>
        /// Update migration status of users
        /// </summary>
        /// <param name="productType">Product Type</param>
        /// <param name="migrateUsers">A list of users to migrate</param>
        /// <returns>Migration result</returns>
        [HttpPatch("users/migrate")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateUsersMigrationStatus([FromQuery] string productType, [FromBody] IList<MigrateUser> migrateUsers)
        {
            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var result = await Task.Run(() =>
            {
                var productList = _productRepository.GetAllProducts();
                int productId = ProductEnumHelper.GetProductIdByProductCode(productType, productList);

                var integrationType = _integrationTypeFactory.GetIntegration(productId);
                var personaId = _userClaimsAccessor.PersonaId;
                return integrationType.UpdateUsersMigrationStatus(personaId, migrateUsers);
            });

            if (!result.Status)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Used to change product user profile without requiring green-book.
        /// Direct call to product to change profile including isActive (mainly used to
        /// activate-deactivate from Migration tool)
        /// </summary>
        /// <param name="productType">Product Code</param>
        /// <param name="productUserProfile">Product user profile</param>
        /// <returns>Success or failure message</returns>
        [HttpPatch("users/externalprofilechange")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExternalProductUserProfileChange([FromQuery] string productType, [FromBody] ProductUserProfile productUserProfile)
        {
            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            {
                return BadRequest("RealPageId empty.");
            }

            var result = await Task.Run(() =>
            {
                var productList = _productRepository.GetAllProducts();
                int productId = ProductEnumHelper.GetProductIdByProductCode(productType, productList);

                var integrationType = _integrationTypeFactory.GetIntegration(productId);
                var personaId = _userClaimsAccessor.PersonaId;
                return integrationType.ExternalUserProfileChange(personaId, productUserProfile);
            });

            if (result)
            {
                return Ok("Successfully disabled product user.");
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Failed to disabled product user.");
        }
    }
}
