using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Controller for product related APIs
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class ProductUserController : BaseController
    {
        private readonly IManageProductUserAsync _manageProductUserAsync;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProductUserController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageProductUserAsync manageProductUserAsync) : base(userClaimsAccessor)
        {
            _manageProductUserAsync = manageProductUserAsync;
        }

        /// <summary>
        /// Used to create a new Realpage product (One site, AO etc) user for the given GreenBook user
        /// </summary>
        /// <param name="productUser">Details to send to Realpage product for a user</param>
        /// <returns>Success message or error</returns>
        [HttpPost("productuser/user")]
        [AllowAnonymous] // TODO: Make it authorize by having client id for Windows Service in ID server
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateProductUser([FromBody] ProductUserProperitiesRoles productUser, CancellationToken cancellationToken = default)
        {
            if (productUser == null)
                return BadRequest("productUser null.");

            if (productUser.RealPageId == Guid.Empty)
                return BadRequest("RealPageId empty.");

            var result = await _manageProductUserAsync.CreateProductUserAsync(productUser, cancellationToken);

            if (string.IsNullOrEmpty(result))
                result = "Success";

            return Created(string.Empty, result);
        }

        /// <summary>
        /// Used to update details for a Realpage product (OneSite, Accounting, VendorServices) user for the given GreenBook user
        /// </summary>
        /// <param name="productUser">Details to save for a user</param>
        /// <returns>Success message or error</returns>
        [HttpPut("productuser/details")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProductUserAccountDetails([FromBody] ProductUserAccountDetails productUser, CancellationToken cancellationToken = default)
        {
            if (productUser == null)
                return BadRequest("productUser null.");

            if (productUser.ProductId <= 0)
                return BadRequest("ProductName empty.");

            var result = await _manageProductUserAsync.UpdateProductUserAccountDetailsAsync(productUser, cancellationToken);

            if (string.IsNullOrEmpty(result))
                result = "Success";

            return Ok(new { success = true, message = result });
        }

        /// <summary>
        /// Used to delete all SAML product information and status for a user
        /// </summary>
        /// <param name="productUser">Details to save for a user</param>
        /// <returns>Success message or error</returns>
        [HttpDelete("productuser/details")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteSamlUserProductInfoAndStatus([FromBody] ProductUserAccountDetails productUser, CancellationToken cancellationToken = default)
        {
            if (productUser == null)
                return BadRequest("productUser null.");

            if (productUser.ProductId <= 0)
                return BadRequest("ProductName empty.");

            var result = await _manageProductUserAsync.DeleteSamlUserProductInfoAndStatusAsync(productUser, cancellationToken);

            if (string.IsNullOrEmpty(result))
                result = "Success";

            return Ok(result);
        }

        /// <summary>
        /// Returns product statuses for the given user
        /// </summary>
        /// <param name="assignUserPersonaId">Assign user Id</param>
        /// <returns>List of product statuses</returns>
        [HttpGet("productuser/status")]
        [ProducesResponseType(typeof(ListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductStatuses(long assignUserPersonaId, CancellationToken cancellationToken = default)
        {
            if (assignUserPersonaId == 0)
                return BadRequest("assignUserPersonaId not supplied.");

            if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
                return BadRequest("RealPageId empty.");

            var result = await _manageProductUserAsync.GetProductStatusesAsync(assignUserPersonaId, cancellationToken);
            ListResponse output = null;

            if (result?.Count > 0)
            {
                output = new ListResponse()
                {
                    Records = result.Cast<object>().ToList()
                };
            }

            return Ok(output);
        }
    }
}
