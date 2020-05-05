using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Controllers
{
    /// <summary>
    /// Product Controller
    /// </summary>
    public class ProductController : BaseApiController
    {
        /// <summary>
	    /// Get list of products
	    /// </summary>
	    /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the books products", Type = typeof(GbProductMap))]
        [Route("products")]
        [AuthorizeScope("userinfoapi")]
        [HttpGet]
        public HttpResponseMessage GetProducts()
        {
            WriteToLog(LogType.Information, "Enterprise - ProductController - GetProducts - Started");
            IProductRepository productRepository = new ProductRepository();
            
            //Passing null to get all the Products
            var result = productRepository.GetAllProducts();
            IList<string> excludeProducts = new List<string>() { "UI", "UL", "SF" };
            result = result.Where(x => !excludeProducts.Contains(x.BooksProductCode)).ToList();

            var logData = new Dictionary<string, object>();
            logData.Add("result", result);

            WriteToLog(LogType.Information, "Enterprise - ProductController - GetProducts - Data returned", logData);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        #region Unified notifications endpoint
        /// <summary>
	    /// Get list of users by companyid or productids
	    /// </summary>
	    /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get list of users by company and products", Type = typeof(ProductUsers))]
        [Route("usersbycompanyproducts")]
        [AuthorizeScope("userinfoapi")]
        [HttpGet]
        public HttpResponseMessage GetUsersByCompanyorProducts(string companyId = null, [FromUri] IList<int?> products = null)
        {
            WriteToLog(LogType.Information, "Enterprise - ProductController - GetUsersByCompanyorProducts - Started");
            if(string.IsNullOrEmpty(companyId) && products == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new List<ProductUsers>());
            }
            int compId;
            if (!int.TryParse(companyId, out compId) && products == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new List<ProductUsers>());
            }
            IProductRepository productRepository = new ProductRepository();
            var result = productRepository.GetUsersByCompanyorProducts(companyId, products);
            var logData = new Dictionary<string, object>();
            logData.Add("result", result);
            WriteToLog(LogType.Information, "Enterprise - ProductController - GetUsersByCompanyorProducts - Data returned", logData);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
	    /// Get Unified Login User Mapping id for given Product user Id's by  Blue Book Company ID and ProductId.
	    /// </summary>
	    /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get list of UL mapping users id by company and products", Type = typeof(MappedUnifiedLoginUserDetails))]
        [Route("ulusermappingidbycompanyproductUserId")]
        [AuthorizeScope("userinfoapi")]
        [HttpPost]
        public HttpResponseMessage GetULUserIdMappedToProductUserIdByCompanyAndProducts([FromBody] ProductUserIDMappingRequest productUserIDMappingRequest)
        {            
            WriteToLog(LogType.Information, "Enterprise - ProductController - GetULUserIdMappedToProductUserIdByCompanyAndProducts - Started");
            MappedUnifiedLoginUserDetails mappedUnifiedLoginUserDetails = new MappedUnifiedLoginUserDetails
            {
                CompanyId = productUserIDMappingRequest.CompanyId,
                ProductId = productUserIDMappingRequest.ProductId,
                ULMappedUserId = new List<ULMappedUserIds>()
            };
            
            if (productUserIDMappingRequest == null ||
                productUserIDMappingRequest.CompanyId <= 0 ||
                productUserIDMappingRequest.ProductId <= 0)
            {                
                return Request.CreateResponse(HttpStatusCode.BadRequest, mappedUnifiedLoginUserDetails);
            }

            IProductRepository productRepository = new ProductRepository();
            mappedUnifiedLoginUserDetails.ULMappedUserId = productRepository.GetULMappingUsersByCompanyAndProducts(productUserIDMappingRequest.CompanyId, 
                                                                                 productUserIDMappingRequest.ProductId,
                                                                                 productUserIDMappingRequest.ProductUserId);
            var logData = new Dictionary<string, object>();
            logData.Add("result", mappedUnifiedLoginUserDetails);
            WriteToLog(LogType.Information, "Enterprise - ProductController - GetULUserIdMappedToProductUserIdByCompanyAndProducts - Data returned", logData);

            return Request.CreateResponse(HttpStatusCode.OK, mappedUnifiedLoginUserDetails);
        }


        #endregion

        #region Private methods
        /// <summary>
        /// Used to write to the log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message to log</param>
        /// <param name="logData">Data to log</param>
        /// <param name="exception">Exception details</param>
        private void WriteToLog(LogType logType, string message, Dictionary<string, object> logData = null, Exception exception = null)
        {
            try
            {
                Log.Write(logType, new LogDetails
                {
                    Message = message,
                    AdditionalInfo = logData,
                    ProductModule = this.GetType().ToString(),
                    UserId = _userClaims.UserId.ToString(),
                    PmcId = _userClaims?.OrganizationPartyId.ToString(),
                    Exception = exception,
                    CorrelationId = _userClaims.CorrelationId.ToString(),
                });
            }
            catch
            {
                /*ignored*/
            }
        }
        #endregion
    }
}
