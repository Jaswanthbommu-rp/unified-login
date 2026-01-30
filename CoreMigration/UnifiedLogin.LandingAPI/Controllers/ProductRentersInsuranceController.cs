using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// ProductRentersInsurance Controller for property and role management
    /// </summary>
    [ApiController]
    [Authorize]
    public class ProductRentersInsuranceController : ControllerBase
    {
        private readonly IUserClaimsAccessor _userClaimsAccessor;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProductRentersInsuranceController(IUserClaimsAccessor userClaimsAccessor)
        {
            _userClaimsAccessor = userClaimsAccessor;
        }

        /// <summary>
        /// Returns Properties
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <param name="datafilter">A datafilter used to filter the properties.</param>
        /// <returns>List of properties</returns>
        [HttpGet("products/rentersinsurance/properties")]
        [ProducesResponseType(typeof(ListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListProperties(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (editorPersonaId == 0)
                {
                    return Ok("editorPersonaId not supplied.");
                }

                var userClaim = _userClaimsAccessor.GetUserClaim();
                if (userClaim == null || userClaim.UserRealPageGuid == Guid.Empty)
                {
                    return Ok("RealPageId empty.");
                }

                ListResponse listResponse = new ListResponse();
                ManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(userClaim);
                listResponse = manageProductRentersInsurance.ListProperties(editorPersonaId, userPersonaId, datafilter);

                return Ok(listResponse);
            });
        }

        /// <summary>
        /// Returns Roles
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <returns>List of roles</returns>
        [HttpGet("products/rentersinsurance/roles")]
        [ProducesResponseType(typeof(ObjectListOutput<ProductRole, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListRoles(long editorPersonaId, long userPersonaId)
        {
            return await Task.Run<IActionResult>(() =>
            {
                ObjectListOutput<ProductRole, IErrorData> output = new ObjectListOutput<ProductRole, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                output.Status = errorStatus;

                if (editorPersonaId == 0)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "ProductRentersInsurance.ListRoles.1";
                    errorStatus.ErrorMsg = "Invalid parameter - editorPersonaId";
                    output.Status = errorStatus;
                    return Ok(output);
                }

                var userClaim = _userClaimsAccessor.GetUserClaim();
                if (userClaim == null || userClaim.UserRealPageGuid == Guid.Empty)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "ProductRentersInsurance.ListRoles.2";
                    errorStatus.ErrorMsg = "Invalid - Enterprise User Id";
                    output.Status = errorStatus;
                    return Ok(output);
                }

                ListResponse listResponse = new ListResponse();
                ManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(userClaim);
                IList<ProductRole> productRoleList = manageProductRentersInsurance.ListRoles(editorPersonaId, userPersonaId);
                if (productRoleList == null)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "ProductRentersInsurance.ListRoles.3";
                    errorStatus.ErrorMsg = "Product Renters Insurance - List Roles: No data";
                    output.Status = errorStatus;
                    return Ok(output);
                }
                output.list = productRoleList;
                return Ok(output);
            });
        }

        #region Migration API

        /// <summary>
        /// Returns product users of an organization for given user.
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="datafilter">Data filter for pagination and filtering</param>
        /// <returns>List of Renters Insurance migration users</returns>
        [HttpGet("products/rentersinsurance/migration-users")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListRentersInsuranceMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");

                ManagePersona managePersona = new ManagePersona();
                var persona = managePersona.GetPersona(editorPersonaId);
                if (persona == null)
                    return BadRequest("editorPersonaId not found.");

                var userClaim = _userClaimsAccessor.GetUserClaim();
                userClaim.UserRealPageGuid = persona.RealPageId;
                var manageProductRentersInsurance = new ManageProductRentersInsurance(userClaim);

                var result = manageProductRentersInsurance.GetMigrationUsers(editorPersonaId, datafilter);
                if (!result.IsError)
                    return Ok(result);
                else
                    return StatusCode(StatusCodes.Status403Forbidden, result);
            });
        }

        /// <summary>
        /// Update migration status of users.
        /// </summary>
        /// <param name="migrateUsers">List of users to mark as migrated</param>
        /// <returns>Migration status result</returns>
        [HttpPut("products/rentersinsurance/migrate-users")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUsersMigrationStatus(IList<MigrateUser> migrateUsers)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                if (userClaim == null)
                    return Unauthorized();

                var manageProductRentersInsurance = new ManageProductRentersInsurance(userClaim);
                return Ok(manageProductRentersInsurance.UpdateUsersMigrationStatus(userClaim.PersonaId, migrateUsers));
            });
        }

        /// <summary>
        /// Disables the Renters Insurance user.
        /// </summary>
        /// <param name="productUser">The product user.</param>
        /// <returns>Success or error message</returns>
        [HttpPut("products/rentersinsurance/user/MT/status")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateRentersInsuranceUserStatus(ProductUser productUser)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                if (userClaim == null)
                    return Unauthorized();

                var manageProductRentersInsurance = new ManageProductRentersInsurance(userClaim);
                if (!manageProductRentersInsurance.ChangeUserStatus(userClaim.PersonaId, productUser.UserId))
                {
                    return BadRequest("Disabling Renters Insurance user failed.");
                }
                return Ok("Successfully disabled product user.");
            });
        }

        #endregion
    }
}
