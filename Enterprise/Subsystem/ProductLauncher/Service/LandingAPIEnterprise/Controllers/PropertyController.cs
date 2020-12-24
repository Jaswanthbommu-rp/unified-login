using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;
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
    /// PropertyController
    /// </summary>
    public class PropertyController : BaseApiController
    {
	    /// <summary>
	    /// Get a list of roles for the given user and product
	    /// </summary>
	    /// <param name="realPageId">The guid for the user being requested</param>
	    /// <param name="productCode">The code for the product being requested. Supported products OPS-Ops</param>
	    /// <returns></returns>
	    [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
	    [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
	    [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
	    [SwaggerResponse(HttpStatusCode.OK, Description = "A list of properties for the given company", Type = typeof(ProductProperty))]
	    [SwaggerResponseExamples(typeof(ProductProperty), typeof(ProductProperty.PropertySimpleExample))]
	    [Route("user/{realPageId}/product/{productCode}/properties")]
	    [AuthorizeScope("enterpriseapi")]
	    [HttpGet]
	    public HttpResponseMessage GetUserProductProperties(Guid realPageId, string productCode)
	    {
		    PagedResponse response = new PagedResponse() {Meta = new Meta()};
		    ErrorResponse error = new ErrorResponse()
		    {
			    Errors = new List<Error>()
		    };
		    IList<object> filteredList = new List<object>();

		    Persona persona = new Persona();
		    IManagePerson personLogic = new ManagePerson();
		    IPerson person = personLogic.GetPerson(realPageId);

		    if (person != null)
		    {
			    IManagePersona managePersona = new ManagePersona(_userClaims);
                //Active Persona is linked to one organization
                persona = managePersona.GetFirstAvailablePersonaByCompany(realPageId, _orgPartyId);

                //Verify if same company
                if (persona == null || persona.OrganizationPartyId != _orgPartyId)
                {
				    return Request.CreateResponse(HttpStatusCode.NotFound);
			    }
		    }
		    else
		    {
			    return Request.CreateResponse(HttpStatusCode.NotFound);
		    }

		    ListResponse productResponse;
		    switch (ProductEnumHelper.GetProductEnumByProductCode(productCode))
		    {
			    case ProductEnum.OpsBuyer:
				    IManageProductOps manageProductOps = new ManageProductOps(_userClaims);
				    productResponse = manageProductOps.GetCompanyAssets(_userClaims.PersonaId, persona.PersonaId, false, null);
				    List<AssetGroup> opsFilteredList = productResponse.Records.Cast<AssetGroup>().ToList().FindAll(p => p.IsAssigned);
				    filteredList = opsFilteredList.Cast<object>().ToList();
					break;
			    default:
				    error.Errors.Add(new Error() {Title = "Bad request", Detail = "No valid product code could be found", Source = "/property", StatusCode = ""});
				    return Request.CreateResponse(HttpStatusCode.BadRequest, error);
		    }

		    if (!productResponse.IsError)
		    {
			    response.Data = filteredList;
			    response.Meta.CurrentPage = 1;
			    response.Meta.TotalRows = filteredList.Count;
			    response.Meta.RowsPerPage = filteredList.Count;
			    return Request.CreateResponse(HttpStatusCode.OK, response);
		    }
		    else
		    {
			    error.Errors.Add(new Error() {Title = "Error", Detail = productResponse.ErrorReason, Source = "/property", StatusCode = ""});
			    return Request.CreateResponse(HttpStatusCode.BadRequest, error);
		    }
	    }

        /// <summary>
        /// Get a list of properties for the given product
        /// </summary>
        /// <param name="productCode">The code for the product being requested. Supported products OPS-Ops, UPFM-Unified Login, IB-Intelligent Building</param>
        /// <param name="include">Optional List of serialize properties names (comma delimited) to return in the response: ID, Name, Street1, City, State, Zip, InstanceId, Longitude, Latitude.  Supported products: Unified Login only</param>
        /// <returns>http Response</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
	    [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
	    [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
	    [SwaggerResponse(HttpStatusCode.OK, Description = "A list of properties for the given company", Type = typeof(ProductProperty))]
	    [SwaggerResponseExamples(typeof(ProductProperty), typeof(ProductProperty.PropertySimpleExample))]
	    [Route("product/{productCode}/properties")]
	    [AuthorizeScope("enterpriseapi")]
	    [HttpGet]
	    public HttpResponseMessage GetProductProperties(string productCode, string include = null)
	    {
		    PagedResponse response = new PagedResponse() {Meta = new Meta()};
		    ErrorResponse error = new ErrorResponse()
		    {
			    Errors = new List<Error>()
		    };
            ManageUnifiedLogin manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
            int productId = (int)ProductEnumHelper.GetProductEnumByProductCode(productCode);
            ListResponse productResponse;
            switch (ProductEnumHelper.GetProductEnumByProductCode(productCode))
            {
                case ProductEnum.OpsBuyer:
                    IManageProductOps manageProductOps = new ManageProductOps(_userClaims);
                    productResponse = manageProductOps.GetCompanyAssets(_userClaims.PersonaId, 0, false, null);
                    break;
                case ProductEnum.CIMPL:
                case ProductEnum.UnifiedPlatform:
                    productResponse = manageUnifiedLogin.GetProperties(_userClaims.PersonaId, include);
                    break;
                //case ProductEnum.IntelligentBuilding:
                //    IManageIntelligentBuilding manageIntelligentBuilding = new ManageIntelligentBuilding(_userClaims);
                //    productResponse = manageIntelligentBuilding.GetUPFMProperties(_userClaims.PersonaId, include);
                //    break;
                case ProductEnum.IntelligentBuildingTrash:
                case ProductEnum.IntelligentBuildingEnergy:
                case ProductEnum.IntelligentBuildingWater:
                case ProductEnum.HospitalityService:
                    ManageUPFMProductsIntegration upfmProductIntegration = new ManageUPFMProductsIntegration(productId, _userClaims);
                    var upfmProduct = ProductEnumHelper.GetUPFMProductEnum(productId);
                    productResponse = upfmProductIntegration.GetUPFMProperties(_userClaims.PersonaId, upfmProduct, include);
                    break;
                default:
                    error.Errors.Add(new Error() { Title = "Bad request", Detail = "No valid product code could be found", Source = "/property", StatusCode = "" });
                    return Request.CreateResponse(HttpStatusCode.BadRequest, error);
            }

            if (!productResponse.IsError)
            {
                response.Data = productResponse.Records;
                response.Meta.CurrentPage = 1;
                response.Meta.TotalRows = productResponse.TotalRows;
                response.Meta.RowsPerPage = productResponse.TotalRows;
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            else
            {
                error.Errors.Add(new Error() { Title = "Error", Detail = productResponse.ErrorReason, Source = "/property", StatusCode = "" });
                return Request.CreateResponse(HttpStatusCode.BadRequest, error);
            }
		}

        /// <summary>
        /// Get a list of companies and its properties for the given user
        /// </summary>        
        /// <param name="productCode">The productid is to get the product properties</param>
        /// <returns>http Response</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of companies and its properties for the given user", Type = typeof(UserCompaniesProperties))]
        [SwaggerResponseExamples(typeof(UserCompaniesProperties), typeof(ProductProperty.CompanyPropertiesSimpleExample))]
        [Route("user/getusercompanyproperties")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public HttpResponseMessage GetUserCompanyProperties(string productCode)
        {
            var propertyResponse = new ListResponse();
            ErrorResponse error = new ErrorResponse()
            {
                Errors = new List<Error>()
            };
            int productId = (int)ProductEnumHelper.GetProductEnumByProductCode(productCode);
            ManageUPFMProductsIntegration upfmProductIntegration = new ManageUPFMProductsIntegration(productId, _userClaims);
            IManageUserLogin manageUserLogin = new ManageUserLogin(_userClaims);
            List<UserCompaniesProperties> userCompaniesProperties = new List<UserCompaniesProperties>();

            var companyResponse = manageUserLogin.GetUserPersonaOrganization(_userClaims.LoginName);
            var upfmProduct = ProductEnumHelper.GetUPFMProductEnum(productId);

            foreach (var company in companyResponse)
            {
                propertyResponse = upfmProductIntegration.GetUPFMProperties(company.PersonaId, upfmProduct, null, company.OrganizationRealPageId.ToString());
                if (propertyResponse.Records.Count == 0) return Request.CreateResponse(HttpStatusCode.ExpectationFailed, $"Properties are not loaded from Blue Book {propertyResponse.ErrorReason}");

                var userCompanyProperties = new UserCompaniesProperties()
                {
                    Id = company.BooksCustomerMasterId,
                    OrganizationName = company.OrganizationName,
                    InstanceId = company.OrganizationRealPageId,
                    Properties = new List<Properties>()
                };
                foreach (var product in propertyResponse.Records.ToList())
                {
                    var properties = new Properties()
                    {
                        Id = ((ProductProperty)product).ID,
                        InstanceId = ((ProductProperty)product).InstanceId,
                        PropertyName = ((ProductProperty)product).Name
                    };
                    userCompanyProperties.Properties.Add(properties);
                }
                userCompaniesProperties.Add(userCompanyProperties);
            }
            if (!propertyResponse.IsError)
            {
                return Request.CreateResponse(HttpStatusCode.OK, userCompaniesProperties);
            }
            else
            {
                error.Errors.Add(new Error() { Title = "Error", Detail = propertyResponse.ErrorReason, Source = "/property", StatusCode = "" });
                return Request.CreateResponse(HttpStatusCode.BadRequest, error);
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
        public HttpResponseMessage GetOpsAssetGroups(int assetGroupId = 0)
        {
            PagedResponse response = new PagedResponse() { Meta = new Meta() };
            ErrorResponse error = new ErrorResponse()
            {
                Errors = new List<Error>()
            };

            IManageProductOps manageProductOps = new ManageProductOps(_userClaims);
            ListResponse listResponse = manageProductOps.GetOpsAssetGroups(_userClaims.PersonaId, 0, assetGroupId);
            if (!listResponse.IsError)
            {
                response.Data = listResponse.Records;
                response.Meta.CurrentPage = 1;
                response.Meta.TotalRows = listResponse.TotalRows;
                response.Meta.RowsPerPage = listResponse.TotalRows;
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            else
            {
                error.Errors.Add(new Error() { Title = "Error", Detail = listResponse.ErrorReason, Source = "/assetgroups", StatusCode = "" });
                return Request.CreateResponse(HttpStatusCode.BadRequest, error);
            }
        }

        /// <summary>
        /// Return all available properties based on the logon user's context
        /// </summary>
        /// <param name="status">Status is optional, and default value is ‘all’ which will return both active and inactive properties</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of properties", Type = typeof(Portfolio))]
        [SwaggerResponseExamples(typeof(Portfolio), typeof(ProductProperty.GetOpsAssetsResponse))]
        [Route("product/ops/properties")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public HttpResponseMessage GetOpsAssets(string status = "all")
        {
            PagedResponse response = new PagedResponse() { Meta = new Meta() };
            ErrorResponse error = new ErrorResponse()
            {
                Errors = new List<Error>()
            };

            IManageProductOps manageProductOps = new ManageProductOps(_userClaims);
            ListResponse productResponse = manageProductOps.GetOpsAssets(_userClaims.PersonaId, 0, status);
            if (!productResponse.IsError)
            {
                response.Data = productResponse.Records;
                response.Meta.CurrentPage = 1;
                response.Meta.TotalRows = productResponse.TotalRows;
                response.Meta.RowsPerPage = productResponse.TotalRows;
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            else
            {
                error.Errors.Add(new Error() { Title = "Error", Detail = productResponse.ErrorReason, Source = "/properties", StatusCode = "" });
                return Request.CreateResponse(HttpStatusCode.BadRequest, error);
            }
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
        public HttpResponseMessage CreateOpsAssetGroups([FromBody] AssetGroupCreate assetGroup)
        {
            PagedResponse response = new PagedResponse() { Meta = new Meta() };
            ErrorResponse error = new ErrorResponse()
            {
                Errors = new List<Error>()
            };

            IManageProductOps manageProductOps = new ManageProductOps(_userClaims);
            ListResponse listResponse = manageProductOps.CreateOpsAssetGroup(_userClaims.PersonaId, 0, assetGroup);
            if (!listResponse.IsError)
            {
                response.Data = listResponse.Records;
                response.Meta.CurrentPage = 1;
                response.Meta.TotalRows = listResponse.TotalRows;
                response.Meta.RowsPerPage = listResponse.TotalRows;
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            else
            {
                error.Errors.Add(new Error() { Title = "Error", Detail = listResponse.ErrorReason, Source = "/assetgroups", StatusCode = "" });
                return Request.CreateResponse(HttpStatusCode.BadRequest, error);
            }
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
        public HttpResponseMessage UpdateOpsAssetGroups([FromBody] AssetGroupCreate assetGroup, int assetGroupId)
        {
            PagedResponse response = new PagedResponse() { Meta = new Meta() };
            ErrorResponse error = new ErrorResponse()
            {
                Errors = new List<Error>()
            };

            IManageProductOps manageProductOps = new ManageProductOps(_userClaims);
            ListResponse listResponse = manageProductOps.UpdateOpsAssetGroup(_userClaims.PersonaId, 0, assetGroupId, assetGroup);
            if (!listResponse.IsError)
            {
                response.Data = listResponse.Records;
                response.Meta.CurrentPage = 1;
                response.Meta.TotalRows = listResponse.TotalRows;
                response.Meta.RowsPerPage = listResponse.TotalRows;
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            else
            {
                error.Errors.Add(new Error() { Title = "Error", Detail = listResponse.ErrorReason, Source = "/assetgroups", StatusCode = "" });
                return Request.CreateResponse(HttpStatusCode.BadRequest, error);
            }
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
        public HttpResponseMessage PatchOpsAssetGroups([FromBody] AssetGroupPatch assetGroup, int assetGroupId)
        {
            PagedResponse response = new PagedResponse() { Meta = new Meta() };
            ErrorResponse error = new ErrorResponse()
            {
                Errors = new List<Error>()
            };

            IManageProductOps manageProductOps = new ManageProductOps(_userClaims);
            ListResponse listResponse = manageProductOps.PatchOpsAssetGroup(_userClaims.PersonaId, 0, assetGroupId, assetGroup);
            if (!listResponse.IsError)
            {
                response.Data = listResponse.Records;
                response.Meta.CurrentPage = 1;
                response.Meta.TotalRows = listResponse.TotalRows;
                response.Meta.RowsPerPage = listResponse.TotalRows;
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            else
            {
                error.Errors.Add(new Error() { Title = "Error", Detail = listResponse.ErrorReason, Source = "/assetgroups", StatusCode = "" });
                return Request.CreateResponse(HttpStatusCode.BadRequest, error);
            }
        }

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
			    AssetGroup opsExample = new AssetGroup()
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

			    PagedResponse response = new PagedResponse()
			    {
				    Meta = new Meta() { TotalRows = list.Count, CurrentPage = 1, RowsPerPage = list.Count },
				    Data = list.Cast<object>().ToList(),

			    };

			    return response;
		    }
	    }
	    #endregion
	}
}
