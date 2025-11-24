using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Enterprise;
namespace UnifiedLogin.LandingAPIEnterprise.Controllers
{
    [ApiController]
    [Route("shell")] // legacy prefix: /shell
    [Authorize(Policy = "enterpriseapi")]
    public class ShellController : BaseApiController
    {
        private readonly ILogger<ShellController> _logger;

        public ShellController(ILogger<ShellController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get the side menu navigation items (legacy implementation pending service migration).
        /// </summary>
        [HttpGet("sidemenu")]
        [ProducesResponseType(typeof(List<NavigationMenuTree>), 200)]
        [ProducesResponseType(500)]
        public IActionResult GetSideMenuNavigation()
        {
            try
            {
                // TODO: Integrate IUserRepository, IManageSecurity, etc. once migrated.
                return Ok(new List<NavigationMenuTree>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSideMenuNavigation failed. CorrelationId={CorrelationId}", CorrelationId);
                return Problem("Internal error retrieving navigation menu.", statusCode: 500);
            }
        }
    }

    // Minimal placeholder to allow compilation; replace with real model when repositories are migrated.
    public class NavigationMenuTree
    {
        public string? Title { get; set; }
        public string? Icon { get; set; }
        public string? PageId { get; set; }
        public string? URL { get; set; }
        public string? Origin { get; set; }
        public List<NavigationMenuTree>? Items { get; set; }
    }
}
