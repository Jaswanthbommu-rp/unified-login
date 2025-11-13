using Microsoft.AspNetCore.Mvc;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Test controller for basic API health and connectivity checks
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Simple test endpoint that returns a greeting message
        /// </summary>
        /// <returns>A greeting message from .NET Core 8.0</returns>
        [HttpGet("hello")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<object> GetHello()
        {
            _logger.LogInformation("Test endpoint 'hello' was called");

            return Ok(new
            {
                Message = "Hello from .NET Core 8.0",
                Timestamp = DateTime.UtcNow,
                Framework = ".NET Core 8.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            });
        }
    }
}
