using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Controller for product related APIs
    /// </summary>
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class ProductUserController : ControllerBase
    {
        private readonly IUserClaimsAccessor _userClaimsAccessor;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProductUserController(IUserClaimsAccessor userClaimsAccessor)
        {
            _userClaimsAccessor = userClaimsAccessor;
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
        public async Task<IActionResult> CreateProductUser([FromBody] ProductUserProperitiesRoles productUser)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (productUser == null)
                    return BadRequest("productUser null.");

                if (productUser.RealPageId == Guid.Empty)
                    return BadRequest("RealPageId empty.");

                var userClaim = _userClaimsAccessor.GetUserClaim();
                ManageProductUser manageProduct = new ManageProductUser(userClaim);
                string result = manageProduct.CreateProductUser(productUser);

                if (string.IsNullOrEmpty(result))
                    result = "Success";

                return Created(string.Empty, result);
            });
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
        public async Task<IActionResult> UpdateProductUserAccountDetails([FromBody] ProductUserAccountDetails productUser)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (productUser == null)
                    return BadRequest("productUser null.");

                if (productUser.ProductId <= 0)
                    return BadRequest("ProductName empty.");

                var userClaim = _userClaimsAccessor.GetUserClaim();
                ManageProductUser manageProduct = new ManageProductUser(userClaim);
                string result = manageProduct.UpdateProductUserAccountDetails(productUser, true);

                if (string.IsNullOrEmpty(result))
                    result = "Success";

                return Ok(result);
            });
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
        public async Task<IActionResult> DeleteSamlUserProductInfoAndStatus([FromBody] ProductUserAccountDetails productUser)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (productUser == null)
                    return BadRequest("productUser null.");

                if (productUser.ProductId <= 0)
                    return BadRequest("ProductName empty.");

                var userClaim = _userClaimsAccessor.GetUserClaim();
                ManageProductUser manageProduct = new ManageProductUser(userClaim);
                string result = manageProduct.DeleteSamlUserProductInfoAndStatus(productUser, true);

                if (string.IsNullOrEmpty(result))
                    result = "Success";

                return Ok(result);
            });
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
        public async Task<IActionResult> GetProductStatuses(long assignUserPersonaId)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (assignUserPersonaId == 0)
                    return BadRequest("assignUserPersonaId not supplied.");

                var userClaim = _userClaimsAccessor.GetUserClaim();
                if (userClaim == null || userClaim.UserRealPageGuid == Guid.Empty)
                    return BadRequest("RealPageId empty.");

                var manageProduct = new ManageProductUser(userClaim);
                var result = manageProduct.GetProductStatuses(userClaim.UserRealPageGuid, assignUserPersonaId);
                ListResponse output = null;

                if (result?.Count > 0)
                {
                    output = new ListResponse()
                    {
                        Records = result.Cast<object>().ToList()
                    };
                }

                return Ok(output);
            });
        }
    }
}
