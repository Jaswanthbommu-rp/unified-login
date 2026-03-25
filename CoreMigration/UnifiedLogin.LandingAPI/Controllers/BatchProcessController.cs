using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Batch Process Controller - Handles asynchronous batch processing operations
    /// for user product assignments, enterprise roles, primary properties, and bulk user updates
    /// </summary>
    [Route("")]
    [ApiController]
    public class BatchProcessController : BaseController
    {
        private readonly IBatchProcessServiceAsync _batchProcessService;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public BatchProcessController(
            IBatchProcessServiceAsync batchProcessService,
            IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
            _batchProcessService = batchProcessService ?? throw new ArgumentNullException(nameof(batchProcessService));
        }

        /// <summary>
        /// Used to process batch record by windows service
        /// </summary>
        [HttpPost("batchprocessor")]
        [AllowAnonymous] // TODO: Make it authorize by having client id for Windows Service in ID server
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ProcessBatch([FromBody] ProductUserProperitiesRoles batchRecord, CancellationToken cancellationToken = default)
        {
            if (batchRecord == null)
            {
                return BadRequest("batchRecord is null.");
            }

            string result = await _batchProcessService.ProcessBatchAsync(batchRecord, cancellationToken);

            if (string.IsNullOrEmpty(result))
            {
                result = "Success";
            }

            return StatusCode((int)HttpStatusCode.Created, result);
        }

        /// <summary>
        /// Used to process Enterprise role product update to user batch record by windows service
        /// </summary>
        [HttpPost("erpbatchprocessor")]
        [AllowAnonymous] // TODO: Make it authorize by having client id for Windows Service in ID server
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> EnterpriseRoleProductProcessBatch([FromBody] EnterpriseRoleBatch batchRecord, CancellationToken cancellationToken = default)
        {
            if (batchRecord == null)
            {
                return BadRequest("enterprise role product batchRecord null.");
            }

            var userClaim = _userClaimsAccessor.GetUserClaim();
            string result = await _batchProcessService.ProcessEnterpriseRoleProductBatchAsync(batchRecord, userClaim, cancellationToken);

            if (string.IsNullOrEmpty(result))
            {
                result = "Success";
            }

            return StatusCode((int)HttpStatusCode.Created, result);
        }

        /// <summary>
        /// Used to process product Primary Properties update to user batch record by windows service
        /// </summary>
        [HttpPost("ppbatchprocessor")]
        [AllowAnonymous] // TODO: Make it authorize by having client id for Windows Service in ID server
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ProductPrimaryPropertyProcessBatch([FromBody] PrimaryPropertyBatch batchRecord, CancellationToken cancellationToken = default)
        {
            if (batchRecord == null)
            {
                return BadRequest("product primary property batchRecord null.");
            }

            var userClaim = _userClaimsAccessor.GetUserClaim();
            string result = await _batchProcessService.ProcessPrimaryPropertyBatchAsync(batchRecord, userClaim, cancellationToken);

            if (string.IsNullOrEmpty(result))
            {
                result = "Success";
            }

            return StatusCode((int)HttpStatusCode.Created, result);
        }

        /// <summary>
        /// Used to process bulk user update to user batch record by windows service
        /// </summary>
        [HttpPost("bulkuserbatchprocessor")]
        [AllowAnonymous] // TODO: Make it authorize by having client id for Windows Service in ID server
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> BulkUserProcessBatch([FromBody] BulkUserBatch batchRecord, CancellationToken cancellationToken = default)
        {
            if (batchRecord == null)
            {
                return BadRequest("bulk user batchRecord null.");
            }

            var userClaim = _userClaimsAccessor.GetUserClaim();
            string result = await _batchProcessService.ProcessBulkUserBatchAsync(batchRecord, userClaim, cancellationToken);

            if (string.IsNullOrEmpty(result))
            {
                result = "Success";
            }

            return StatusCode((int)HttpStatusCode.Created, result);
        }
    }
}
