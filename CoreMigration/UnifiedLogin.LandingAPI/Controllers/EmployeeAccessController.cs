using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.EmployeeAccess;

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
        private readonly IManageEmployeeAccessAsync _manageEmployeeAccess;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public EmployeeAccessController(
            IManageEmployeeAccessAsync manageEmployeeAccess,
            IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
            _manageEmployeeAccess = manageEmployeeAccess ?? throw new ArgumentNullException(nameof(manageEmployeeAccess));
        }

        /// <summary>
        /// Get companies for employee access
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="filter">Filter string</param>
        /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
        /// <returns>List of companies</returns>
        [HttpGet("employeeaccess/companies")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCompanies(long editorPersonaId, string filter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            var result = await _manageEmployeeAccess.GetCompaniesAsync(editorPersonaId, filter, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Get users for employee access
        /// </summary>
        /// <param name="editorPersonaId">Editor persona ID</param>
        /// <param name="filter">Filter string</param>
        /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
        /// <returns>List of users</returns>
        [HttpGet("employeeaccess/users")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsers(long editorPersonaId, string filter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            var result = await _manageEmployeeAccess.GetUsersAsync(editorPersonaId, filter, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Gets company persona Id, if exists else creates user in company and gets, and user realpage guid for employee as a user.
        /// </summary>
        /// <param name="companyRealPageId">Company RealPage ID</param>
        /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
        /// <returns>Employee persona ID</returns>
        [HttpGet("employeeaccess/company/{companyRealPageId}/persona")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEmployeePersonaId(Guid companyRealPageId, CancellationToken cancellationToken = default)
        {
            if (companyRealPageId == Guid.Empty)
                return BadRequest("Company ID not supplied.");

            var userClaim = _userClaimsAccessor.GetUserClaim();
            if (userClaim == null)
                return Unauthorized();

            var result = await _manageEmployeeAccess.GetOrCreateEmployeePersonaIdAsync(companyRealPageId, userClaim, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Creates an employee in the given product and company if it does not exist.
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="personaId">Persona ID</param>
        /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
        /// <returns>Employee access response</returns>
        [HttpPost("employeeaccess/product/{productId}/persona/{personaId}")]
        [ProducesResponseType(typeof(EmployeeAccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateEmployeeProductUser(int productId, long personaId, CancellationToken cancellationToken = default)
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

            var result = await _manageEmployeeAccess.CreateEmployeeProductUserAsync(productId, personaId, cancellationToken);

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
