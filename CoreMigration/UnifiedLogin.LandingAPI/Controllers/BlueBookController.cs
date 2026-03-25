using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Controller for BlueBook related APIs
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class BlueBookController : BaseController
    {
        private readonly IManageBlueBookAsync _manageBlueBook;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public BlueBookController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageBlueBookAsync manageBlueBook) : base(userClaimsAccessor)
        {
            _manageBlueBook = manageBlueBook ?? throw new ArgumentNullException(nameof(manageBlueBook));
        }

        /// <summary>
        /// Returns property information for customer
        /// </summary>
        /// <param name="booksCompanyMasterId">Books Company Master ID</param>
        /// <param name="include">Include fields</param>
        /// <param name="filter">Filter</param>
        /// <param name="getCached">Get cached data</param>
        /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
        /// <returns>List of customer properties</returns>
        [HttpGet("CustomerProperty")]
        [ProducesResponseType(typeof(ListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCustomerProperty(
            long booksCompanyMasterId = 0,
            string include = null,
            string filter = null,
            bool getCached = true,
            CancellationToken cancellationToken = default)
        {
            var result = await _manageBlueBook.GetCustomerPropertyAsync(
                booksCompanyMasterId, include, filter, getCached, cancellationToken);

            return Ok(result);
        }
    }
}
