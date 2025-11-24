using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Enterprise;

namespace UnifiedLogin.LandingAPIEnterprise.Controllers
{
    [ApiController]
    [Route("role")] // legacy root adjusted: /role
    [Authorize(Policy = "enterpriseapi")]
    public class RoleController : BaseApiController
    {
        private readonly ILogger<RoleController> _logger;

        public RoleController(ILogger<RoleController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get a list of roles for the given user and product (placeholder implementation).
        /// </summary>
        [HttpGet("user/{realPageId:guid}/product/{productCode}/roles")]
        [ProducesResponseType(typeof(PagedResponse), 200)]
        public IActionResult GetUserProductRoles(Guid realPageId, string productCode, Guid? upfmId = null)
        {
            // TODO: Wire ManageUnifiedLogin + ProductPanel logic.
            var response = new PagedResponse { Meta = new Meta { CurrentPage = 1, RowsPerPage = 0, TotalRows = 0 }, Data = new List<object>() };
            return Ok(response);
        }

        /// <summary>
        /// Get a list of roles for a product (placeholder implementation).
        /// </summary>
        [HttpGet("product/{productCode}/roles")]
        [ProducesResponseType(typeof(PagedResponse), 200)]
        public IActionResult GetProductRoles(string productCode, Guid? upfmId = null)
        {
            var response = new PagedResponse { Meta = new Meta { CurrentPage = 1, RowsPerPage = 0, TotalRows = 0 }, Data = new List<object>() };
            return Ok(response);
        }

        /// <summary>
        /// Get a list of rights for a role (placeholder).
        /// </summary>
        [HttpGet("product/{productCode}/roles/{roleId:int}/rights")]
        [ProducesResponseType(200)]
        public IActionResult GetRightsForRole(string productCode, int roleId, Guid? upfmId = null)
        {
            if (string.IsNullOrWhiteSpace(productCode) || roleId == 0)
                return BadRequest("ProductCode or roleId not supplied.");
            return Ok(new { roleId, productCode, rights = new List<string>() });
        }
    }
}
