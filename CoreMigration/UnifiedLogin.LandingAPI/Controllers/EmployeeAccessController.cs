using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Employee Access Controller for managing employee access to companies and products
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class EmployeeAccessController : BaseController
    {
        private readonly IManageEmployeeAccess _manageEmployeeAccess;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public EmployeeAccessController(
            IManageEmployeeAccess manageEmployeeAccess,
            IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
            _manageEmployeeAccess = manageEmployeeAccess ?? throw new ArgumentNullException(nameof(manageEmployeeAccess));
        }

        /// <summary>
        /// Get companies for employee access
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="filter">Filter string</param>
        /// <returns>List of companies</returns>
        [HttpGet("employeeaccess/companies")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCompanies(long editorPersonaId, string filter)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            var result = await Task.Run(() => _manageEmployeeAccess.GetCompanies(editorPersonaId, filter));

            return Ok(result);
        }

        /// <summary>
        /// Get users for employee access
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="filter">Filter string</param>
        /// <returns>List of users</returns>
        [HttpGet("employeeaccess/users")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsers(long editorPersonaId, string filter)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            var result = await Task.Run(() => _manageEmployeeAccess.GetUsers(editorPersonaId, filter));

            return Ok(result);
        }

        /// <summary>
        /// Gets company persona Id, if exists else creates user in company and gets, and user realpage guid for employee as a user.
        /// </summary>
        /// <param name="companyRealPageId">Company RealPage ID</param>
        /// <returns>Employee persona ID</returns>
        [HttpGet("employeeaccess/company/{companyRealPageId}/persona")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEmployeePersonaId(Guid companyRealPageId)
        {
            if (companyRealPageId == Guid.Empty)
                return BadRequest("Company ID not supplied.");

            var userClaim = _userClaimsAccessor.GetUserClaim();
            if (userClaim == null)
                return Unauthorized();

            var result = await Task.Run(() =>
                _manageEmployeeAccess.GetOrCreateEmployeePersonaId(companyRealPageId, userClaim));

            return Ok(result);
        }

        /// <summary>
        /// Creates an employee in the given product and company if it does not exist.
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="personaId">Persona ID</param>
        /// <returns>Employee access response</returns>
        [HttpPost("employeeaccess/product/{productId}/persona/{personaId}")]
        [ProducesResponseType(typeof(EmployeeAccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateEmployeeProductUser(int productId, long personaId)
        {
            var response = new EmployeeAccessResponse();

            if (productId == 0)
            {
                response.ErrorMessage = "Product ID not supplied.";
                return BadRequest(response);
            }

            if (personaId == 0)
            {
                response.ErrorMessage = "Persona ID not supplied.";
                return BadRequest(response);
            }

            var result = await Task.Run(() =>
            {
                var createResult = _manageEmployeeAccess.CreateEmployeeProductUser(productId, personaId);
                if (createResult.Equals("DeletedProductLogin", StringComparison.OrdinalIgnoreCase))
                {
                    // the product login was disabled, so try again and see if any other groups are assignable
                    createResult = _manageEmployeeAccess.CreateEmployeeProductUser(productId, personaId);
                }
                return createResult;
            });

            if (string.IsNullOrEmpty(result))
            {
                response.Status = true;
            }
            else
            {
                response.ErrorMessage = result;
            }

            return Ok(response);
        }

        /// <summary>
        /// Employee access response model
        /// </summary>
        public class EmployeeAccessResponse
        {
            /// <summary>
            /// Status of the operation
            /// </summary>
            public bool Status { get; set; }

            /// <summary>
            /// Error message if any
            /// </summary>
            public string ErrorMessage { get; set; } = "";
        }
    }
}
