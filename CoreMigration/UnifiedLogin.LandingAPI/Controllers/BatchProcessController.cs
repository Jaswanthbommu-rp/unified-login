using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.BatchProcessor;
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
        /// <summary>
        /// Constructor with dependency injection for user claims accessor
        /// </summary>
        /// <param name="userClaimsAccessor">Accessor for current authenticated user's claims</param>
        public BatchProcessController(IUserClaimsAccessor userClaimsAccessor): base(userClaimsAccessor)
        {
        }

        /// <summary>
        /// Used to process batch record by windows service
        /// </summary>
        /// <param name="batchRecord">Details to send to RealPage product for a user</param>
        /// <returns>Processing result message</returns>
        [HttpPost("batchprocessor")]
        [AllowAnonymous] // TODO: Make it authorize by having client id for Windows Service in ID server
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ProcessBatch([FromBody] ProductUserProperitiesRoles batchRecord)
        {
            if (batchRecord == null)
            {
                return BadRequest("batchRecord is null.");
            }

            string result = await Task.Run(() =>
            {
                var manageBatchProcess = new BatchProcessorLogic();
                return manageBatchProcess.ProcessBatch(batchRecord);
            });

            if (string.IsNullOrEmpty(result))
            {
                result = "Success";
            }

            return StatusCode((int)HttpStatusCode.Created, result);
        }

        /// <summary>
        /// Used to process Enterprise role product update to user batch record by windows service
        /// </summary>
        /// <param name="batchRecord">Details to send to RealPage product for a user</param>
        /// <returns>Processing result message</returns>
        [HttpPost("erpbatchprocessor")]
        [AllowAnonymous] // TODO: Make it authorize by having client id for Windows Service in ID server
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> EnterpriseRoleProductProcessBatch([FromBody] EnterpriseRoleBatch batchRecord)
        {
            if (batchRecord == null)
            {
                return BadRequest("enterprise role product batchRecord null.");
            }

            string result = await Task.Run(() =>
            {
                // Create DefaultUserClaim for batch processing
                // Note: For anonymous batch processing, we use a system-level claim
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageBatchProcess = new ManageEnterpriseRoleProductBatch(userClaim);
                return manageBatchProcess.GenerateEnterpriseRoleUserProductBatch(batchRecord);
            });

            if (string.IsNullOrEmpty(result))
            {
                result = "Success";
            }

            return StatusCode((int)HttpStatusCode.Created, result);
        }

        /// <summary>
        /// Used to process product Primary Properties update to user batch record by windows service
        /// </summary>
        /// <param name="batchRecord">Details to send to RealPage product for a user</param>
        /// <returns>Processing result message</returns>
        [HttpPost("ppbatchprocessor")]
        [AllowAnonymous] // TODO: Make it authorize by having client id for Windows Service in ID server
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ProductPrimaryPropertyProcessBatch([FromBody] PrimaryPropertyBatch batchRecord)
        {
            if (batchRecord == null)
            {
                return BadRequest("product primary property batchRecord null.");
            }

            string result = await Task.Run(() =>
            {
                // Create DefaultUserClaim for batch processing
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageBatchProcess = new ManagePrimaryPropertiesBatch(userClaim);
                return manageBatchProcess.GeneratePrimaryPropertiesUserProductBatch(batchRecord);
            });

            if (string.IsNullOrEmpty(result))
            {
                result = "Success";
            }

            return StatusCode((int)HttpStatusCode.Created, result);
        }

        /// <summary>
        /// Used to process bulk user update to user batch record by windows service
        /// </summary>
        /// <param name="batchRecord">Details to send to RealPage product for a user</param>
        /// <returns>Processing result message</returns>
        [HttpPost("bulkuserbatchprocessor")]
        [AllowAnonymous] // TODO: Make it authorize by having client id for Windows Service in ID server
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> BulkUserProcessBatch([FromBody] BulkUserBatch batchRecord)
        {
            if (batchRecord == null)
            {
                return BadRequest("bulk user batchRecord null.");
            }

            string result = await Task.Run(() =>
            {
                // Create DefaultUserClaim for batch processing
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageBatchProcess = new ManageBulkUserBatch(userClaim);
                return manageBatchProcess.GenerateProductUnAssignProductBatch(batchRecord);
            });

            if (string.IsNullOrEmpty(result))
            {
                result = "Success";
            }

            return StatusCode((int)HttpStatusCode.Created, result);
        }
    }
}
