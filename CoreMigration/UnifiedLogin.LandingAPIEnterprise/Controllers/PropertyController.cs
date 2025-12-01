using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Enterprise;

namespace UnifiedLogin.LandingAPIEnterprise.Controllers
{
    [ApiController]
    [Route("property")] // legacy root mapping
    [Authorize(Policy = "enterpriseapi")]
    public class PropertyController : BaseApiController
    {
        private readonly ILogger<PropertyController> _logger;

        public PropertyController(ILogger<PropertyController> logger)
        {
            _logger = logger;
        }

        [HttpGet("user/{realPageId:guid}/product/{productCode}/properties")]
        [ProducesResponseType(typeof(PagedResponse), 200)]
        public IActionResult GetUserProductProperties(Guid realPageId, string productCode)
        {
            var response = new PagedResponse { Meta = new Meta { CurrentPage = 1, RowsPerPage = 0, TotalRows = 0 }, Data = new List<object>() };
            return Ok(response);
        }

        [HttpGet("product/{productCode}/properties")]
        [ProducesResponseType(typeof(PagedResponse), 200)]
        public IActionResult GetProductProperties(string productCode, string? include = null)
        {
            var response = new PagedResponse { Meta = new Meta { CurrentPage = 1, RowsPerPage = 0, TotalRows = 0 }, Data = new List<object>() };
            return Ok(response);
        }

        [HttpGet("user/getusercompanyproperties")]
        [ProducesResponseType(200)]
        public IActionResult GetUserCompanyProperties(string productCode)
        {
            return Ok(new { productCode, companies = Array.Empty<object>() });
        }

        [HttpGet("product/ops/assetgroups")]
        [ProducesResponseType(typeof(PagedResponse), 200)]
        public IActionResult GetOpsAssetGroups(int assetGroupId = 0)
        {
            var response = new PagedResponse { Meta = new Meta { CurrentPage = 1, RowsPerPage = 0, TotalRows = 0 }, Data = new List<object>() };
            return Ok(response);
        }

        [HttpGet("product/ops/properties")]
        [ProducesResponseType(typeof(PagedResponse), 200)]
        public IActionResult GetOpsAssets(string status = "all")
        {
            var response = new PagedResponse { Meta = new Meta { CurrentPage = 1, RowsPerPage = 0, TotalRows = 0 }, Data = new List<object>() };
            return Ok(response);
        }

        [HttpPost("product/ops/assetgroups")]
        [ProducesResponseType(typeof(PagedResponse), 200)]
        public IActionResult CreateOpsAssetGroups([FromBody] object assetGroup)
        {
            var response = new PagedResponse { Meta = new Meta { CurrentPage = 1, RowsPerPage = 0, TotalRows = 0 }, Data = new List<object>() };
            return Ok(response);
        }

        [HttpPut("product/ops/assetgroups/{assetGroupId:int}")]
        [ProducesResponseType(typeof(PagedResponse), 200)]
        public IActionResult UpdateOpsAssetGroups([FromBody] object assetGroup, int assetGroupId)
        {
            var response = new PagedResponse { Meta = new Meta { CurrentPage = 1, RowsPerPage = 0, TotalRows = 0 }, Data = new List<object>() };
            return Ok(response);
        }

        [HttpPatch("product/ops/assetgroups/{assetGroupId:int}")]
        [ProducesResponseType(typeof(PagedResponse), 200)]
        public IActionResult PatchOpsAssetGroups([FromBody] object assetGroup, int assetGroupId)
        {
            var response = new PagedResponse { Meta = new Meta { CurrentPage = 1, RowsPerPage = 0, TotalRows = 0 }, Data = new List<object>() };
            return Ok(response);
        }
    }
}
