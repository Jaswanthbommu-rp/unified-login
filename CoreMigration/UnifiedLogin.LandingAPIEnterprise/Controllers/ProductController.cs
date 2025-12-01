using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Enterprise;

namespace UnifiedLogin.LandingAPIEnterprise.Controllers
{
    [ApiController]
    [Route("product")] // consolidate legacy routes under /product
    public class ProductController : BaseApiController
    {
        private readonly ILogger<ProductController> _logger;

        public ProductController(ILogger<ProductController> logger)
        {
            _logger = logger;
        }

        [HttpGet("products")]
        [Authorize(Policy = "userinfoapi")]
        public IActionResult GetProducts()
        {
            // TODO: Implement repository-backed product list.
            return Ok(new List<object>());
        }

        [HttpGet("usersbycompanyproducts")]
        [Authorize(Policy = "userinfoapi")]
        public IActionResult GetUsersByCompanyOrProducts(string? companyId = null, string? upfmId = null, [FromQuery] IList<int?>? products = null, [FromQuery] string? userType = null, [FromQuery] string? userStatus = null)
        {
            return Ok(new { companyId, upfmId, products = products ?? Array.Empty<int?>(), userType, userStatus, users = new List<object>() });
        }

        [HttpPost("ulusermappingidbycompanyproductUserId")]
        [Authorize(Policy = "userinfoapi")]
        public IActionResult GetULUserIdMappedToProductUserIdByCompanyAndProducts([FromBody] ProductUserIDMappingRequest? productUserIDMappingRequest)
        {
            if (productUserIDMappingRequest == null)
                return BadRequest(new { error = "Invalid request" });
            return Ok(new { productUserIDMappingRequest, mapped = new List<object>() });
        }

        [HttpGet("users")]
        [Authorize(Policy = "userinfoapi")]
        public IActionResult GetUsersByCompanyOrProductCodes([FromQuery] List<string> productcode, string? companyid = null, string? upfmId = null, int? rowsPerPage = 5000, int? pageNumber = 1,
            [FromQuery] List<string>? roles = null, [FromQuery] List<string>? rights = null, [FromQuery] List<string>? propertyIds = null, string? companyDomain = null)
        {
            var response = new PagedResponse { Meta = new Meta { CurrentPage = pageNumber ?? 1, RowsPerPage = rowsPerPage ?? 0, TotalRows = 0 }, Data = new List<object>() };
            return Ok(response);
        }
    }

    // Placeholder request model (adapt from legacy definitions as needed)
    public class ProductUserIDMappingRequest
    {
        public int CompanyId { get; set; }
        public string? ProductCode { get; set; }
        public string? upfmId { get; set; }
        public List<int>? ProductUserId { get; set; }
    }
}
