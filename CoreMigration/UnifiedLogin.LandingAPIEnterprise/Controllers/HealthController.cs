using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UnifiedLogin.LandingAPIEnterprise.Controllers
{
    /// <summary>
    /// Health check controller for API status verification
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Simple health check endpoint that doesn't require authentication
        /// </summary>
        /// <returns>Status information</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                version = "1.0.0"
            });
        }

        /// <summary>
        /// Ping endpoint for simple connectivity test
        /// </summary>
        [HttpGet("ping")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Ping()
        {
            return Ok("pong");
        }
    }
}
