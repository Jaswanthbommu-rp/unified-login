using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
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
        private readonly IManagePersona _managePersona;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userClaimsAccessor">Accessor for current authenticated user's claims</param>
        /// <param name="managePersona">Service for managing persona operations</param>
        public ProductAssetOptimizationController(IUserClaimsAccessor userClaimsAccessor, IManagePersona managePersona): base(userClaimsAccessor)
        {
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
        }

        /// <summary>
        /// Returns Companies
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">User persona ID who is being created or edited</param>
        /// <param name="productName">Product division name in AO (BI/AX/PO/PA)</param>
        /// <param name="datafilter">A datafilter used to filter the companies</param>
        /// <param name="userLoginName">User Login Name</param>
        /// <returns>List of AO companies</returns>
        [HttpGet("companies")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCompanies(long editorPersonaId, long userPersonaId, string productName, [FromQuery] RequestParameter datafilter, string userLoginName = "")
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
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageProductAo = new ManageProductAssetOptimization(userClaim);
                return manageProductAo.GetCompanies(editorPersonaId, userPersonaId, productName, datafilter, userLoginName);
            });

            if (result?.IsError == true)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns assignable or assigned Property Groups for user
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">User persona ID</param>
        /// <param name="productName">Product name</param>
        /// <param name="selectedCompanies">List of selected company IDs</param>
        /// <param name="datafilter">A datafilter used to filter the property groups</param>
        /// <param name="userLoginName">User Login Name</param>
        /// <returns>List of property groups</returns>
        [HttpGet("propertygroups")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPropertyGroups(long editorPersonaId, long userPersonaId, string productName, [FromQuery] IList<string> selectedCompanies, [FromQuery] RequestParameter datafilter, string userLoginName = "")
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
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageProductAo = new ManageProductAssetOptimization(userClaim);
                return manageProductAo.GetPropertyGroups(editorPersonaId, userPersonaId, productName, selectedCompanies);
            });

            if (result?.IsError == true)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns properties in group
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">User persona ID</param>
        /// <param name="propertyGroupId">Property Group ID to select properties</param>
        /// <param name="datafilter">A datafilter used to filter the properties</param>
        /// <returns>List of properties in the group</returns>
        [HttpGet("groupproperties")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPropertiesInGroups(long editorPersonaId, long userPersonaId, int propertyGroupId, [FromQuery] RequestParameter datafilter)
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
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageProductAo = new ManageProductAssetOptimization(userClaim);
                return manageProductAo.GetPropertiesInGroup(editorPersonaId, userPersonaId, propertyGroupId);
            });

            if (result?.IsError == true)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns companies and associated roles for a product
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">User persona ID</param>
        /// <param name="productName">AO product name</param>
        /// <param name="datafilter">A datafilter used to filter</param>
        /// <param name="userLoginName">User Login Name</param>
        /// <returns>Companies with roles</returns>
        [HttpGet("companyroles")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCompaniesWithRoles(long editorPersonaId, long userPersonaId, string productName, [FromQuery] RequestParameter datafilter, string userLoginName = "")
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
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageProductAo = new ManageProductAssetOptimization(userClaim);
                return manageProductAo.GetCompaniesWithRoles(editorPersonaId, userPersonaId, productName, datafilter, userLoginName);
            });

            if (result?.IsError == true)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns companies and associated properties for a product
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">User persona ID</param>
        /// <param name="productName">AO product name</param>
        /// <param name="datafilter">A datafilter used to filter</param>
        /// <param name="userLoginName">User Login Name</param>
        /// <returns>Companies with properties</returns>
        [HttpGet("companyproperties")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCompaniesWithProperties(long editorPersonaId, long userPersonaId, string productName, [FromQuery] RequestParameter datafilter, string userLoginName = "")
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
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageProductAo = new ManageProductAssetOptimization(userClaim);
                return manageProductAo.GetCompaniesWithProperties(editorPersonaId, userPersonaId, productName, datafilter, userLoginName);
            });

            if (result?.IsError == true)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns operators with properties
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">User persona ID</param>
        /// <returns>Operators with properties</returns>
        [HttpGet("operatorproperties")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOperatorsWithProperties(long editorPersonaId, long userPersonaId)
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
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageProductAo = new ManageProductAssetOptimization(userClaim);
                return manageProductAo.GetOperators(editorPersonaId, userPersonaId);
            });

            if (result?.IsError == true)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns properties filtered by operators
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="userPersonaId">User persona ID</param>
        /// <param name="operatorCode">AO operator code</param>
        /// <param name="operatorValue">AO operator value</param>
        /// <returns>Properties with operators</returns>
        [HttpGet("propertiesbyoperators")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPropertiesWithOperators(long editorPersonaId, long userPersonaId, string operatorCode, string operatorValue)
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
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageProductAo = new ManageProductAssetOptimization(userClaim);
                return manageProductAo.GetPropertiesWithOperators(editorPersonaId, userPersonaId, operatorCode, operatorValue);
            });

            if (result?.IsError == true)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates the AO user status (activate/deactivate)
        /// </summary>
        /// <param name="productUser">The product user</param>
        /// <returns>Status update result</returns>
        [HttpPut("user/MT/status")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateAOUserStatus([FromBody] ProductUser productUser)
        {
            var result = await Task.Run(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageProductAo = new ManageProductAssetOptimization(userClaim);
                var personaId = _userClaimsAccessor.PersonaId;
                return manageProductAo.ChangeUserStatus(personaId, productUser.UserName, productUser.FirstName, productUser.LastName);
            });

            if (!result)
            {
                if (productUser.IsAssigned)
                {
                    return BadRequest("Activate ao user failed.");
                }
                return BadRequest("Deactivate ao user failed.");
            }

            return Ok("Successfully disabled product user.");
        }

        #region Migration API

        /// <summary>
        /// Returns product users of an organization for migration purposes
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="datafilter">A datafilter used to filter the users</param>
        /// <returns>List of Asset Optimization migration users</returns>
        [HttpGet("migration-users")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListAssetOptimizationMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter)
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
                var manageProductAo = new ManageProductAssetOptimization(userClaim);

                return (object)manageProductAo.GetMigrationUsers(editorPersonaId, datafilter);
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
            var result = await Task.Run(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageProductAo = new ManageProductAssetOptimization(userClaim);
                var personaId = _userClaimsAccessor.PersonaId;
                return manageProductAo.UpdateUsersMigrationStatus(personaId, migrateUsers);
            });

            return Ok(result);
        }

        #endregion
    }
}
