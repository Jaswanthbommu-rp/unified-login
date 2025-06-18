using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.BatchProcessor;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Batch;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// Batch Process Controller
	/// </summary>
	public class BatchProcessController : BaseApiController
    {    
        #region Public methods

        /// <summary>
        /// Used to process batch record by windows service
        /// </summary>
        /// <param name="batchRecord">Details to send to Realpage product for a user</param> 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("batchprocessor")]
        [AllowAnonymous]//TODO: Make it authorize by having client id for Windows Service in ID server
        [HttpPost]
        public HttpResponseMessage ProcessBatch(ProductUserProperitiesRoles batchRecord)
        {
            if (batchRecord == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "batchRecord null.");

            //if (batchRecord.RealPageId == Guid.Empty)
            //    return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

            var manageBatchProcess = new BatchProcessorLogic();
            string result = manageBatchProcess.ProcessBatch(batchRecord);
             
            if (string.IsNullOrEmpty(result))
                result = "Success";

            return Request.CreateResponse(HttpStatusCode.Created, result);
        }

        /// <summary>
        /// Used to process Entrprise role product update to user batch record by windows service
        /// </summary>
        /// <param name="batchRecord">Details to send to Realpage product for a user</param> 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("erpbatchprocessor")]
        [AllowAnonymous]//TODO: Make it authorize by having client id for Windows Service in ID server
        [HttpPost]
        public HttpResponseMessage EnterpriseRoleProductProcessBatch(EnterpriseRoleBatch batchRecord)
        {
            if (batchRecord == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "enterprise role product batchRecord null.");

            ManageEnterpriseRoleProductBatch manageBatchProcess = new ManageEnterpriseRoleProductBatch(_userClaims);
            string result = manageBatchProcess.GenerateEnterpriseRoleUserProductBatch(batchRecord);

            if (string.IsNullOrEmpty(result))
                result = "Success";

            return Request.CreateResponse(HttpStatusCode.Created, result);
        }

        /// <summary>
        /// Used to process product Primary Properties  update to user batch record by windows service
        /// </summary>
        /// <param name="batchRecord">Details to send to Realpage product for a user</param> 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("ppbatchprocessor")]
        [AllowAnonymous]//TODO: Make it authorize by having client id for Windows Service in ID server
        [HttpPost]
        public HttpResponseMessage ProductPrimaryPropertyProcessBatch(PrimaryPropertyBatch batchRecord)
        {
            if (batchRecord == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "product primary property batchRecord null.");

            ManagePrimaryPropertiesBatch manageBatchProcess = new ManagePrimaryPropertiesBatch(_userClaims);
            string result = manageBatchProcess.GeneratePrimaryPropertiesUserProductBatch(batchRecord);

            if (string.IsNullOrEmpty(result))
                result = "Success";

            return Request.CreateResponse(HttpStatusCode.Created, result);
        }

        /// <summary>
        /// Used to process bulk user update to user batch record by windows service
        /// </summary>
        /// <param name="batchRecord">Details to send to Realpage product for a user</param> 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("bulkuserbatchprocessor")]
        [AllowAnonymous]//TODO: Make it authorize by having client id for Windows Service in ID server
        [HttpPost]
        public HttpResponseMessage BulkUserProcessBatch(BulkUserBatch batchRecord)
        {
            if (batchRecord == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "bulk user batchRecord null.");
         
            ManageBulkUserBatch manageBatchProcess = new ManageBulkUserBatch(_userClaims);
            string result = manageBatchProcess.GenerateProductUnAssignProductBatch(batchRecord);

            if (string.IsNullOrEmpty(result))
                result = "Success";

            return Request.CreateResponse(HttpStatusCode.Created, result);
        }

     #endregion
    }
} 
