using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.ResponseObject;
using Serilog;
using Serilog.Events;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Controllers
{
    /// <summary>
    /// Product Controller
    /// </summary>
    public class ProductController : BaseApiController
    {
        private IProductRepository _productRepository;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ProductController()
        {
            // DONT USE USERCLAIM IN BASE, IT IS NULL AT THIS POINT. MOVE TO Initialize FUNCTION
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            _productRepository = new ProductRepository(_userClaims);
        }

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
            WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", messageProperties: new object[] { "GetProducts", "Started" });

            var result = GetAllProducts();

            var logData = new Dictionary<string, object> { { "result", result } };
            WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", logData, messageProperties: new object[] { "GetProducts", "Data returned" });
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
        public HttpResponseMessage GetUsersByCompanyorProducts(string companyId = null, string upfmId = null, [FromUri] IList<int?> products = null, [FromUri] string userType = null, [FromUri] string userStatus = null)
        {
            WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", messageProperties: new object[] { "GetUsersByCompanyorProducts", "Started" });

            if (!ValidateCompanyProductsDetailsData(companyId, products))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            IProductRepository productRepository = new ProductRepository();
            var result = productRepository.GetUsersByCompanyorProducts(companyId, products, upfmId, userType, userStatus);

            var logData = new Dictionary<string, object> { { "result", result } };
            WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", logData, messageProperties: new object[] { "GetUsersByCompanyorProducts", "Data returned" });

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
	    /// Get Unified Login User Mapping id for given Product user Id's by  Blue Book Company ID or upfmId and ProductId.
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

            WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", messageProperties: new object[] { "GetULUserIdMappedToProductUserIdByCompanyAndProducts", "Started" });
            MappedUnifiedLoginUserDetails mappedUnifiedLoginUserDetails = new MappedUnifiedLoginUserDetails
            {
                CompanyId = productUserIDMappingRequest.CompanyId,
                ProductCode = productUserIDMappingRequest.ProductCode,
                upfmId= productUserIDMappingRequest.upfmId,
                ULMappedPersonaId = new List<ULMappedPersonaIds>()
            };

            var result = GetAllProducts();
            productId = result.Where(x => x.BooksProductCode == productUserIDMappingRequest.ProductCode).FirstOrDefault().ProductId;

            if (productUserIDMappingRequest == null ||
                (productUserIDMappingRequest.CompanyId < 1 &&
                string.IsNullOrEmpty(productUserIDMappingRequest.upfmId)) ||
                string.IsNullOrEmpty(productUserIDMappingRequest.ProductCode) ||
                productUserIDMappingRequest.ProductUserId?.Count == 0 ||
                productId <= 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, mappedUnifiedLoginUserDetails);
            }

            IProductRepository productRepository = new ProductRepository();
            mappedUnifiedLoginUserDetails.ULMappedPersonaId = productRepository.GetULMappingPersonaIDsByCompanyAndProducts(productUserIDMappingRequest.CompanyId,
                                                                                 productUserIDMappingRequest.upfmId,
                                                                                 productId,
                                                                                 productUserIDMappingRequest.ProductUserId);
            var logData = new Dictionary<string, object> { { "result", mappedUnifiedLoginUserDetails } };
            WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", logData, messageProperties: new object[] { "GetULUserIdMappedToProductUserIdByCompanyAndProducts", "Data returned" });

            return Request.CreateResponse(HttpStatusCode.OK, mappedUnifiedLoginUserDetails);
        }

        /// <summary>
        /// Get list of users by companyid or productcodes
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get list of users by company and product codes", Type = typeof(ProductUsers))]
        [SwaggerResponseExamples(typeof(ProductUsers), typeof(UserCompanyProductCodeExample))]

        [Route("users")]
        [AuthorizeScope("userinfoapi")]
        [HttpGet]
        public HttpResponseMessage GetUsersByCompanyorProductCodes([FromUri] List<string> productcode, string companyid = null, string upfmId = null, int? rowsPerPage = 5000, int? pageNumber = 1,
                                                                    [FromUri] List<string> roles = null, [FromUri] List<string> rights = null, [FromUri]List<string> propertyIds = null, string companyDomain = null)
        {
            WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", messageProperties: new object[] { "GetUsersByCompanyorProductCodes", "Started" });

            PagedResponse response = new PagedResponse() { Meta = new Meta() };

            if ((string.IsNullOrEmpty(companyid) && string.IsNullOrEmpty(upfmId)) || !productcode.Any())
            {
                IList<ProductUsers> productUsers = new List<ProductUsers>();

                response.Data = productUsers.Cast<object>().ToList();
                response.Meta.CurrentPage = 1;
                response.Meta.TotalRows = 0;
                response.Meta.RowsPerPage = 0;
                response.IsError = true;
                response.ErrorReason = "BadRequest";
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }

            if (propertyIds.Any())
            {
                propertyIds = propertyIds.Distinct().ToList();
                propertyIds = propertyIds.Where(x => !string.IsNullOrEmpty(x)).ToList();
            }

            IList<int> products = new List<int>();

            var productList = _productRepository.GetAllProducts();
            productcode.ForEach(x => products.Add(ProductEnumHelper.GetProductIdByProductCode(x, productList)));

            IProductRepository productRepository = new ProductRepository();
            var result = productRepository.GetUsersByCompanyorProducts(companyid, upfmId, products, rowsPerPage.Value, pageNumber.Value, roles, rights, propertyIds, companyDomain);

            if (result == null)
            {
                IList<ProductUsers> productUsers = new List<ProductUsers>();

                response.Data = productUsers.Cast<object>().ToList();
                response.Meta.CurrentPage = 1;
                response.Meta.TotalRows = 0;
                response.Meta.RowsPerPage = 0;
                response.IsError = true;
                response.ErrorReason = "BadRequest";
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }

            response.Data = result.Cast<object>().ToList();
            response.Meta.CurrentPage = pageNumber.Value;
            response.Meta.TotalRows = result.Any() ? result[0].TotalRecords : 0;
            response.Meta.RowsPerPage = rowsPerPage.Value;

            var logData = new Dictionary<string, object>
            {
                { "result", response }
            };

            WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", logData, messageProperties: new object[] { "GetUsersByCompanyorProductCodes", "Data returned" });

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
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            try
            {
                var logger = Log.Logger;
				if (logData?.Keys != null)
				{
					logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
				}
				logger = logger.ForContext("ProductModule", this.GetType());
                logger = logger.ForContext("CorrelationId", _userClaims.CorrelationId.ToString());

                logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
            }
            catch
            {
                /*ignored*/
            }
        }

        private bool ValidateCompanyProductsDetailsData(string companyId, IList<int?> products)
        {
            if (string.IsNullOrEmpty(companyId) && products == null)
            {
                return false;
            }

            if (!int.TryParse(companyId, out int compId) && products == null)
            {
                return false;
            }

            return true;
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
                        PersonaId = 2649,
                        PreferredPhoneNumber = "5555555555",
                        Email = "notificationemail@test.com"
                    },
                    new ProductUsers
                    {
                        UserId= 2660,
                        LoginName = "multiuser1@test.com",
                        FirstName = "multi",
                        LastName = "user1",
                        PersonaId = 2657,
                        PreferredPhoneNumber = "8888888888",
                        Email = "notificationemail@test.com"
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
