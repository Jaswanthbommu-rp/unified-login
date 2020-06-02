using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.ResponseObject;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

            var result = GetAllProducts();

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

            var result = GetUsersByCompanyorProductsDetails(companyId , products);
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
            int productId = 0;

            WriteToLog(LogType.Information, "Enterprise - ProductController - GetULUserIdMappedToProductUserIdByCompanyAndProducts - Started");
            MappedUnifiedLoginUserDetails mappedUnifiedLoginUserDetails = new MappedUnifiedLoginUserDetails
            {
                CompanyId = productUserIDMappingRequest.CompanyId,
                ProductCode = productUserIDMappingRequest.ProductCode,
                ULMappedPersonaId = new List<ULMappedPersonaIds>()
            };

            var result = GetAllProducts();
            productId = result.Where(x => x.BooksProductCode == productUserIDMappingRequest.ProductCode).FirstOrDefault().ProductId;

            if (productUserIDMappingRequest == null ||
                productUserIDMappingRequest.CompanyId <= 0||
                string.IsNullOrEmpty(productUserIDMappingRequest.ProductCode) ||
                productId <=0)
            {                
                return Request.CreateResponse(HttpStatusCode.BadRequest, mappedUnifiedLoginUserDetails);
            }

            IProductRepository productRepository = new ProductRepository();
            mappedUnifiedLoginUserDetails.ULMappedPersonaId = productRepository.GetULMappingPersonaIDsByCompanyAndProducts(productUserIDMappingRequest.CompanyId, 
                                                                                 productId,
                                                                                 productUserIDMappingRequest.ProductUserId);
            var logData = new Dictionary<string, object>();
            logData.Add("result", mappedUnifiedLoginUserDetails);
            WriteToLog(LogType.Information, "Enterprise - ProductController - GetULUserIdMappedToProductUserIdByCompanyAndProducts - Data returned", logData);

            return Request.CreateResponse(HttpStatusCode.OK, mappedUnifiedLoginUserDetails);
        }

        /// <summary>
        /// Get list of users by companyid or productcodes
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get list of users by company and product codess", Type = typeof(ProductUsers))]
        [SwaggerResponseExamples(typeof(ProductUsers), typeof(UserCompanyProductCodeExample))]

        [Route("users")]
        [AuthorizeScope("userinfoapi")]
        [HttpGet]
        public HttpResponseMessage GetUsersByCompanyorProductCodess(string companyid, [FromUri] List<string> productcode, int? rowsPerPage = 5000, int? pageNumber = 1)
        {
            WriteToLog(LogType.Information, "Enterprise - ProductController - GetUsersByCompanyorProducts - Started");

            PagedResponse response = new PagedResponse() { Meta = new Meta() };

            IList<int?> productIds = new List<int?>();

            productcode.ForEach(x => productIds.Add((int)ProductEnumHelper.GetProductEnumByProductCode(x)));

            var result = GetUsersByCompanyorProductsDetails(companyid, productIds, 2, rowsPerPage, pageNumber);

            if (result == null)
            {
                IList<ProductUsers> productUsers = new List<ProductUsers>();

                response.Data = productUsers.Cast<object>().ToList();
                response.Meta.CurrentPage = pageNumber.Value;
                response.Meta.TotalRows = 0;
                response.Meta.RowsPerPage = rowsPerPage.Value;
                response.IsError = true;
                response.ErrorReason = "BadRequest";
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }

            response.Data = result.Cast<object>().ToList();
            response.Meta.CurrentPage = pageNumber.Value;
            response.Meta.TotalRows = result[0].TotalRecords;
            response.Meta.RowsPerPage = rowsPerPage.Value;

            var logData = new Dictionary<string, object>
            {
                { "result", response }
            };

            WriteToLog(LogType.Information, "Enterprise - ProductController - GetUsersByCompanyorProducts - Data returned", logData);

            return Request.CreateResponse(HttpStatusCode.OK, response);

        }
        #endregion

        #region Private methods

        private IList<GbProductMap> GetAllProducts()
        {
            IProductRepository productRepository = new ProductRepository();

            //Passing null to get all the Products
            var result = productRepository.GetAllProducts();
            IList<string> excludeProducts = new List<string>()
            {
                ProductEnum.SalesForce.ToEnumDescription(),
                ProductEnum.UnifiedPlatform.ToEnumDescription(),
                ProductEnum.UnifiedUI.ToEnumDescription()
            };
            result = result.Where(x => !excludeProducts.Contains(x.BooksProductCode)).ToList();

            return result;
        }
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

        private IList<ProductUsers> GetUsersByCompanyorProductsDetails(string companyId , IList<int?> products = null , int? version = 1, int? rowsPerPage = 1, int? pageNumber = 1)
        {
            if (string.IsNullOrEmpty(companyId) && products == null)
            {
                return null;
            }
            int compId;
            if (!int.TryParse(companyId, out compId) && products == null)
            {
                return null;
            }
            IProductRepository productRepository = new ProductRepository();
            return productRepository.GetUsersByCompanyorProducts(companyId, products , version.Value , rowsPerPage.Value , pageNumber.Value);
        }
        #endregion

        #region Examples

        /// <summary>
        /// Used to document examples of the UserCompanyProductCode result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class UserCompanyProductCodeExample : IProvideExamples
        {
            public object GetExamples()
            {
                PagedResponse response = new PagedResponse() { Meta = new Meta() };
                IList<ProductUsers> productUsers = new List<ProductUsers>()
                {
                    new ProductUsers
                    {
                        UserId= 2659,
                        LoginName = "notificationsuser@test.com",
                        FirstName = "Notifications",
                        LastName = "User",
                        PersonaId = 2649
                    },
                    new ProductUsers
                    {
                        UserId= 2660,
                        LoginName = "multiuser1@test.com",
                        FirstName = "multi",
                        LastName = "user1",
                        PersonaId = 2657
                    }
                };

                response.Data = productUsers.Cast<object>().ToList();
                response.Meta.CurrentPage = 1;
                response.Meta.TotalRows = 2;
                response.Meta.RowsPerPage = 5000;
                response.IsError = false;
                response.ErrorReason = null;

                return response;
            }
        }
        #endregion
    }
}
