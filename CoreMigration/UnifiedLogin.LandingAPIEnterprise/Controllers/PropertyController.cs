using Microsoft.AspNetCore.Mvc;
using Swashbuckle.Swagger.Annotations;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using UnifiedLogin.BusinessLogic.Attributes;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Attribute;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Ops;
using UnifiedLogin.SharedObjects.ResponseObject;
using UnifiedLogin.SharedObjects.Swagger;

namespace UnifiedLogin.LandingAPIEnterprise.Controllers
{
    /// <summary>
    /// Property Controller for managing product properties and asset groups.
    /// Refactored to use modern ASP.NET Core dependency injection patterns.
    /// </summary>
    [ApiController]
    [Route("")]
    public class PropertyController : ControllerBase
    {
        private readonly IIntegrationTypeFactory _integrationTypeFactory;
        private readonly IProductRepository _productRepository;
        private readonly IManageUnifiedLogin _manageUnifiedLogin;
        private readonly IManagePerson _managePerson;
        private readonly IManagePersona _managePersona;
        private readonly IManageProductOps _manageProductOps;
        private readonly IUserClaimsAccessor _userClaimsAccessor;
        private readonly IManageUPFMProductsIntegration _manageUPFMProductsIntegration;
        /// <summary>
        /// Constructor with dependency injection.
        /// Follows modern ASP.NET Core patterns for testable, maintainable code.
        /// All dependencies are injected as interfaces for loose coupling and testability.
        /// </summary>
        /// <param name="integrationTypeFactory">Factory for creating product integration handlers</param>
        /// <param name="productRepository">Repository for product-related operations</param>
        /// <param name="manageUnifiedLogin">Service for unified login management</param>
        /// <param name="managePerson">Service for person management</param>
        /// <param name="managePersona">Service for persona management</param>
        /// <param name="manageProductOps">Service for OPS product operations</param>
        /// <param name="userClaimsAccessor">Accessor for current authenticated user's claims</param>
        public PropertyController(
            IIntegrationTypeFactory integrationTypeFactory,
            IProductRepository productRepository,
            IManageUnifiedLogin manageUnifiedLogin,
            IManagePerson managePerson,
            IManagePersona managePersona,
            IManageProductOps manageProductOps,
            IUserClaimsAccessor userClaimsAccessor,
            IManageUPFMProductsIntegration manageUPFMProductsIntegration)
        {
            _integrationTypeFactory = integrationTypeFactory ?? throw new ArgumentNullException(nameof(integrationTypeFactory));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _manageUnifiedLogin = manageUnifiedLogin ?? throw new ArgumentNullException(nameof(manageUnifiedLogin));
            _managePerson = managePerson ?? throw new ArgumentNullException(nameof(managePerson));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _manageProductOps = manageProductOps ?? throw new ArgumentNullException(nameof(manageProductOps));
            _userClaimsAccessor = userClaimsAccessor ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
            _manageUPFMProductsIntegration = manageUPFMProductsIntegration ?? throw new ArgumentNullException(nameof(manageUPFMProductsIntegration));
        }

        /// <summary>
        /// Get a list of roles for the given user and product
        /// </summary>
        /// <param name="realPageId">The guid for the user being requested</param>
        /// <param name="productCode">The code for the product being requested. Supported products OPS-Ops</param>
        /// <returns>HTTP response with product properties</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of properties for the given company", Type = typeof(ProductProperty))]
        [SwaggerResponseExamples(typeof(ProductProperty), typeof(ProductProperty.PropertySimpleExample))]
        [Route("user/{realPageId}/product/{productCode}/properties")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public ActionResult GetUserProductProperties(Guid realPageId, string productCode)
        {
            var response = new PagedResponse { Meta = new Meta() };
            var error = new ErrorResponse { Errors = new List<Error>() };

            // Get person information
            var person = _managePerson.GetPerson(realPageId);
            if (person == null)
            {
                return NotFound();
            }

            // Get persona for the user in the current organization
            var persona = _managePersona.GetFirstAvailablePersonaByCompany(
                realPageId,
                _userClaimsAccessor.OrganizationPartyId);

            // Verify user belongs to same company
            if (persona == null || persona.OrganizationPartyId != _userClaimsAccessor.OrganizationPartyId)
            {
                return NotFound();
            }

            // Get product properties based on product code
            var productList = _productRepository.GetAllProducts();
            //var productId = ProductEnumHelper.GetProductIdByProductCode(productCode, productList);
            if (!TryGetProductId(productCode, productList, out var productId))
            {
                return BadRequest(CreateErrorResponse($"Invalid product code '{productCode}'", "/property"));
            }

            ListResponse productResponse;
            IList<object> filteredList;

            switch (productId)
            {
                case (int)ProductEnum.OpsBuyer:
                    productResponse = _manageProductOps.GetCompanyAssets(
                        _userClaimsAccessor.PersonaId,
                        persona.PersonaId,
                        false,
                        null);

                    if (productResponse.IsError)
                    {
                        return BadRequest(CreateErrorResponse(productResponse.ErrorReason, "/property"));
                    }

                    var opsFilteredList = productResponse.Records
                        .Cast<AssetGroup>()
                        .Where(p => p.IsAssigned)
                        .ToList();
                    filteredList = opsFilteredList.Cast<object>().ToList();
                    break;

                default:
                    error.Errors.Add(new Error
                    {
                        Title = "Bad request",
                        Detail = "No valid product code could be found",
                        Source = "/property",
                        StatusCode = ""
                    });
                    return BadRequest(error);
            }

            response.Data = filteredList;
            response.Meta.CurrentPage = 1;
            response.Meta.TotalRows = filteredList.Count;
            response.Meta.RowsPerPage = filteredList.Count;
            return Ok(response);
        }

        /// <summary>
        /// Get a list of properties for the given product
        /// </summary>
        /// <param name="productCode">The code for the product being requested. Supported products OPS-Ops, UPFM-Unified Login, IB-Intelligent Building</param>
        /// <param name="include">Optional List of serialize properties names (comma delimited) to return in the response: ID, Name, Street1, City, State, Zip, InstanceId, Longitude, Latitude.  Supported products: Unified Login only</param>
        /// <returns>HTTP response with product properties</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of properties for the given company", Type = typeof(ProductProperty))]
        [SwaggerResponseExamples(typeof(ProductProperty), typeof(ProductProperty.PropertySimpleExample))]
        [Route("product/{productCode}/properties")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public ActionResult GetProductProperties(string productCode, string include = null)
        {
            var response = new PagedResponse { Meta = new Meta() };
            var productList = _productRepository.GetAllProducts();
            // var productId = ProductEnumHelper.GetProductIdByProductCode(productCode, productList);
            if (!TryGetProductId(productCode, productList, out var productId))
            {
                return BadRequest(CreateErrorResponse($"Invalid product code '{productCode}'", "/property"));
            }

            ListResponse productResponse;

            // Default to old logic, fallback to new logic when no case match
            switch (productId)
            {
                case (int)ProductEnum.OpsBuyer:
                    productResponse = _manageProductOps.GetCompanyAssets(
                        _userClaimsAccessor.PersonaId,
                        0,
                        false,
                        null);
                    break;

                case (int)ProductEnum.UnifiedPlatform:
                    productResponse = _manageUnifiedLogin.GetEnterpriseProperties(
                        _userClaimsAccessor.PersonaId,
                        include);
                    break;

                default:
                    var integration = _integrationTypeFactory.GetIntegration(productId);
                    productResponse = integration.GetEnterpriseProperties(
                        _userClaimsAccessor.PersonaId,
                        new RequestParameter());
                    break;
            }

            if (productResponse.IsError)
            {
                return BadRequest(CreateErrorResponse(productResponse.ErrorReason, "/property"));
            }

            response.Data = productResponse.Records;
            response.Meta.CurrentPage = 1;
            response.Meta.TotalRows = productResponse.TotalRows;
            response.Meta.RowsPerPage = productResponse.TotalRows;
            return Ok(response);
        }

        /// <summary>
        /// Get a list of companies and its properties for the given user
        /// </summary>        
        /// <param name="productCode">The productid is to get the product properties</param>
        /// <returns>HTTP response with user company properties</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of companies and its properties for the given user", Type = typeof(UserCompaniesProperties))]
        [SwaggerResponseExamples(typeof(UserCompaniesProperties), typeof(ProductProperty.CompanyPropertiesSimpleExample))]
        [Route("user/getusercompanyproperties")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public ActionResult GetUserCompanyProperties(string productCode)
        {
            var productList = _productRepository.GetAllProducts();
            if (!TryGetProductId(productCode, productList, out var productId))
            {
                return BadRequest(CreateErrorResponse($"Invalid product code '{productCode}'", "/property"));
            }

            try
            {
                // Note: ManageUPFMProductsIntegration still uses manual instantiation
                // This should be refactored to use dependency injection in a future update
                //var upfmProductIntegration = new ManageUPFMProductsIntegration(
                //    productId,
                //    new UnifiedLogin.SharedObjects.Landing.DefaultUserClaim(User));

                var multiCompanyPropertyResponse = _manageUPFMProductsIntegration.GetUPFMMultiCompanyProperties(productCode);

                if (multiCompanyPropertyResponse == null)
                {
                    return Ok($"{productCode} product not assigned to this user in none of the PMC");
                }

                if (multiCompanyPropertyResponse.Count > 0)
                {
                    return Ok(multiCompanyPropertyResponse);
                }

                return BadRequest(CreateErrorResponse(multiCompanyPropertyResponse[0].ErrorReason, "/property"));
            }
            catch (Exception ex)
            {
                return BadRequest(CreateErrorResponse($"Error processing product code '{productCode}': {ex.Message}", "/property"));
            }
        }

        /// <summary>
        /// Get a list of OPS AssetGroups 
        /// </summary>
        /// <param name="assetGroupId">Optional AssetGroup Id</param>
        /// <returns>HTTP response message including the status code and data</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of AssetGroups or Details about an AssetGroup", Type = typeof(AssetGroup))]
        [SwaggerResponseExamples(typeof(AssetGroup), typeof(ProductProperty.GetOpsAssetGroupResponse))]
        [Route("product/ops/assetgroups")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public ActionResult GetOpsAssetGroups(int assetGroupId = 0)
        {
            var response = new PagedResponse { Meta = new Meta() };
            var listResponse = _manageProductOps.GetOpsAssetGroups(
                _userClaimsAccessor.PersonaId,
                0,
                assetGroupId);

            if (listResponse.IsError)
            {
                return BadRequest(CreateErrorResponse(listResponse.ErrorReason, "/assetgroups"));
            }

            response.Data = listResponse.Records;
            response.Meta.CurrentPage = 1;
            response.Meta.TotalRows = listResponse.TotalRows;
            response.Meta.RowsPerPage = listResponse.TotalRows;
            return Ok(response);
        }

        /// <summary>
        /// Return all available properties based on the logon user's context
        /// </summary>
        /// <param name="status">Status is optional, and default value is 'all' which will return both active and inactive properties</param>
        /// <returns>HTTP response with OPS assets</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of properties", Type = typeof(Portfolio))]
        [SwaggerResponseExamples(typeof(Portfolio), typeof(ProductProperty.GetOpsAssetsResponse))]
        [Route("product/ops/properties")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public ActionResult GetOpsAssets(string status = "all")
        {
            var response = new PagedResponse { Meta = new Meta() };
            var productResponse = _manageProductOps.GetOpsAssets(
                _userClaimsAccessor.PersonaId,
                0,
                status);

            if (productResponse.IsError)
            {
                return BadRequest(CreateErrorResponse(productResponse.ErrorReason, "/properties"));
            }

            response.Data = productResponse.Records;
            response.Meta.CurrentPage = 1;
            response.Meta.TotalRows = productResponse.TotalRows;
            response.Meta.RowsPerPage = productResponse.TotalRows;
            return Ok(response);
        }

        /// <summary>
        /// Create an OPS AssetGroup
        /// </summary>
        /// <param name="assetGroup">AssetGroup object (Name, Description, and List of Properties {Ids, OR Codes}</param>
        /// <returns>HTTP response message including the status code and data</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "AssetGroup created", Type = typeof(AssetGroup))]
        [SwaggerResponseExamples(typeof(AssetGroup), typeof(ProductProperty.CreateOpsAssetGroupResponse))]
        [Route("product/ops/assetgroups")]
        [AuthorizeScope("enterpriseapi")]
        [HttpPost]
        public ActionResult CreateOpsAssetGroups([FromBody] AssetGroupCreate assetGroup)
        {
            var response = new PagedResponse { Meta = new Meta() };
            var listResponse = _manageProductOps.CreateOpsAssetGroup(
                _userClaimsAccessor.PersonaId,
                0,
                assetGroup);

            if (listResponse.IsError)
            {
                return BadRequest(CreateErrorResponse(listResponse.ErrorReason, "/assetgroups"));
            }

            response.Data = listResponse.Records;
            response.Meta.CurrentPage = 1;
            response.Meta.TotalRows = listResponse.TotalRows;
            response.Meta.RowsPerPage = listResponse.TotalRows;
            return Ok(response);
        }

        /// <summary>
        /// Edit/Update an OPS AssetGroup
        /// </summary>
        /// <param name="assetGroup">AssetGroup object (Name, Description, and List of Properties {Ids, OR Codes}</param>
        /// <param name="assetGroupId">assetGroupId being updated</param>
        /// <returns>HTTP response message including the status code and data</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "AssetGroup updated", Type = typeof(AssetGroup))]
        [SwaggerResponseExamples(typeof(AssetGroup), typeof(ProductProperty.UpdateOpsAssetGroupResponse))]
        [Route("product/ops/assetgroups/{assetGroupId}")]
        [AuthorizeScope("enterpriseapi")]
        [HttpPut]
        public ActionResult UpdateOpsAssetGroups([FromBody] AssetGroupCreate assetGroup, int assetGroupId)
        {
            var response = new PagedResponse { Meta = new Meta() };
            var listResponse = _manageProductOps.UpdateOpsAssetGroup(
                _userClaimsAccessor.PersonaId,
                0,
                assetGroupId,
                assetGroup);

            if (listResponse.IsError)
            {
                return BadRequest(CreateErrorResponse(listResponse.ErrorReason, "/assetgroups"));
            }

            response.Data = listResponse.Records;
            response.Meta.CurrentPage = 1;
            response.Meta.TotalRows = listResponse.TotalRows;
            response.Meta.RowsPerPage = listResponse.TotalRows;
            return Ok(response);
        }

        /// <summary>
        /// Update Asset Group Name/Status
        /// </summary>
        /// <param name="assetGroup">AssetGroup object (Name, Description, and List of Properties {Ids, OR Codes}</param>
        /// <param name="assetGroupId">assetGroupId being updated</param>
        /// <returns>HTTP response message including the status code and data</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "AssetGroup patched", Type = typeof(AssetGroupPatch))]
        [Route("product/ops/assetgroups/{assetGroupId}")]
        [AuthorizeScope("enterpriseapi")]
        [HttpPatch]
        public ActionResult PatchOpsAssetGroups([FromBody] AssetGroupPatch assetGroup, int assetGroupId)
        {
            var response = new PagedResponse { Meta = new Meta() };
            var listResponse = _manageProductOps.PatchOpsAssetGroup(
                _userClaimsAccessor.PersonaId,
                0,
                assetGroupId,
                assetGroup);

            if (listResponse.IsError)
            {
                return BadRequest(CreateErrorResponse(listResponse.ErrorReason, "/assetgroups"));
            }

            response.Data = listResponse.Records;
            response.Meta.CurrentPage = 1;
            response.Meta.TotalRows = listResponse.TotalRows;
            response.Meta.RowsPerPage = listResponse.TotalRows;
            return Ok(response);
        }

        #region Private Helper Methods

        /// <summary>
        /// Creates a standardized error response
        /// </summary>
        /// <param name="errorDetail">Error detail message</param>
        /// <param name="source">Source of the error</param>
        /// <returns>BadRequest ActionResult with error details</returns>
        private ErrorResponse CreateErrorResponse(string errorDetail, string source)
        {
            return new ErrorResponse
            {
                Errors = new List<Error>
                {
                    new Error
                    {
                        Title = "Error",
                        Detail = errorDetail,
                        Source = source,
                        StatusCode = ""
                    }
                }
            };
        }

        private bool TryGetProductId(string productCode, IList<GbProductMap> products, out int productId)
        {
            productId = 0;

            // Validate input
            if (string.IsNullOrWhiteSpace(productCode) || products == null || products.Count == 0)
            {
                return false;
            }

            try
            {
                productId = ProductEnumHelper.GetProductIdByProductCode(productCode, products);
                return true;
            }
            catch (Exception)
            {
                // ProductEnumHelper throws an exception for invalid product codes
                // We handle it gracefully and return false
                return false;
            }
        }
        #endregion

        #region GetExamples
        /// <summary>
        /// Used to document examples of the webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class EnterprisePropertyExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>EnterpriseProperty example</returns>
            public object GetExamples()
            {
                IList<object> list = new List<object>();
                var opsExample = new AssetGroup
                {
                    ID = "1",
                    Name = "Ops Test Property",
                    Code = "A0001",
                    Description = "",
                    IsAssigned = false,
                    GroupType = "property",
                    AssetID = "1234",
                    Status = "active"
                };
                list.Add(opsExample);

                var response = new PagedResponse
                {
                    Meta = new Meta
                    {
                        TotalRows = list.Count,
                        CurrentPage = 1,
                        RowsPerPage = list.Count
                    },
                    Data = list.Cast<object>().ToList(),
                };

                return response;
            }
        }
        #endregion
    }
}




