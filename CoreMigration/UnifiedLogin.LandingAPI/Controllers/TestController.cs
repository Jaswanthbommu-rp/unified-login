using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Test Controller for API health checks
    /// </summary>
    [Route("")]
    [ApiController]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// Test API endpoint
        /// </summary>
        /// <returns>Success result with generated GUID</returns>
        [AllowAnonymous]
        [HttpGet("test/testapi")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSuccessResult()
        {
            return await Task.Run<IActionResult>(() => Ok(Guid.NewGuid()));
        }
    }
}
