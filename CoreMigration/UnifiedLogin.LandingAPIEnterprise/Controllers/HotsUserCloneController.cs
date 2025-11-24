using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace UnifiedLogin.LandingAPIEnterprise.Controllers
{
    [ApiController]
    [Route("hots")] // legacy grouping
    public class HotsUserCloneController : BaseApiController
    {
        private readonly ILogger<HotsUserCloneController> _logger;

        public HotsUserCloneController(ILogger<HotsUserCloneController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Clone HOTS users (placeholder implementation).
        /// </summary>
        [HttpPost("userclone")]
        [Authorize(Policy = "enterpriseapi")]
        public IActionResult HOTCloneUsers([FromBody] CloneUsers? cloneUsers)
        {
            if (cloneUsers == null)
            {
                return BadRequest(new ErrorResponse { Errors = new List<Error> { new() { Title = "Error", Source = "/HotsCloneUser", Detail = "Null request received." } } });
            }
            // TODO: Implement claim recreation & clone logic once services migrated.
            return Accepted(new { status = "accepted", requested = cloneUsers });
        }
    }

    // Placeholder request model
    public class CloneUsers
    {
        public Guid? CloneCustomerUPFMId { get; set; }
        public List<Guid>? SourceUserIds { get; set; }
    }
}
