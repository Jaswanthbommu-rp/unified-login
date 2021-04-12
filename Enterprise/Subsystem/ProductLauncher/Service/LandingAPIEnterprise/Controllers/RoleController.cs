using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
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
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Controllers
{
	/// <summary>
	/// Used to get product roles
	/// </summary>
	public class RoleController : BaseApiController
	{
		private readonly IProductRepository _productRepository;

		public RoleController()
        {
			_productRepository = new ProductRepository(_userClaims);
		}

		/// <summary>
		/// Get a list of roles for the given user and product
		/// </summary>
		/// <param name="realPageId">The guid for the user being requested</param>
		/// <param name="productCode">The code for the product being requested.All Products are supported</param>
		/// <returns>A list of product roles</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.NotFound, Description = "Not Found")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles", Type = typeof(ProductRole))]
		[SwaggerResponseExamples(typeof(ProductRole), typeof(EnterpriseRoleExample))]
		[Route("user/{realPageId}/product/{productCode}/roles")]
		[AuthorizeScope("enterpriseapi")]
		[HttpGet]
		public HttpResponseMessage GetUserProductRoles(Guid realPageId, string productCode)
		{
			PagedResponse response = new PagedResponse() {Meta = new Meta()};
			ErrorResponse error = new ErrorResponse()
			{
				Errors = new List<Error>()
			};
			List<Component.SharedObjects.Product.ProductRole> filteredList = new List<Component.SharedObjects.Product.ProductRole>();

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
			if (productCode == "UPFM")
			{
				IManageUnifiedLogin manageProduct = new ManageUnifiedLogin(_userClaims);
				productResponse = manageProduct.GetUserRoles(_userClaims.PersonaId, persona.PersonaId, _userClaims.OrganizationPartyId);
				filteredList = productResponse.Records.Cast<Component.SharedObjects.Product.ProductRole>().ToList().FindAll(p => p.IsAssigned);
			}
			else
			{
				IManageProductPanel productPanelData = new ManageProductPanel(_userClaims);
				var productList = _productRepository.GetAllProducts();
				int productId = (int)ProductEnumHelper.GetProductIdByProductCode(productCode, productList);
				productResponse = productPanelData.GetProductRoles(_userClaims.PersonaId, persona.PersonaId, _userClaims.OrganizationPartyId, productId, null, null);
				filteredList = productResponse.Records.Cast<Component.SharedObjects.Product.ProductRole>().ToList().FindAll(p => p.IsAssigned);
			}
			
			if (!productResponse.IsError)
			{
				response.Data = filteredList.Cast<object>().ToList();
				response.Meta.CurrentPage = 1;
				response.Meta.TotalRows = filteredList.Count;
				response.Meta.RowsPerPage = filteredList.Count;
				return Request.CreateResponse(HttpStatusCode.OK, response);
			}
			else
			{
				error.Errors.Add(new Error() {Title = "Error", Detail = productResponse.ErrorReason, Source = "/role", StatusCode = ""});
				return Request.CreateResponse(HttpStatusCode.BadRequest, error);
			}
		}

		/// <summary>
		/// Get a list of roles for a product
		/// </summary>
		/// <param name="productCode">The code for the product being requested. All Products are supported</param>
		/// <returns>A list of product roles</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[SwaggerResponse(HttpStatusCode.NotFound, Description = "Not Found")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles", Type = typeof(ProductRole))]
		[SwaggerResponseExamples(typeof(ProductRole), typeof(EnterpriseRoleExample))]
		[Route("product/{productCode}/roles")]
		[AuthorizeScope("enterpriseapi")]
		[HttpGet]
		public HttpResponseMessage GetProductRoles(string productCode)
		{
			ErrorResponse error = new ErrorResponse()
			{
				Errors = new List<Error>()
			};

			ListResponse productResponse;

			if (productCode == "UPFM")
			{
				IManageUnifiedLogin manageProduct = new ManageUnifiedLogin(_userClaims);
				productResponse = manageProduct.GetRoles(_userClaims.PersonaId, _userClaims.OrganizationPartyId);
			}
			else
			{
				IManageProductPanel productPanelData = new ManageProductPanel(_userClaims);
				var productList = _productRepository.GetAllProducts();
				int productId = (int)ProductEnumHelper.GetProductIdByProductCode(productCode, productList);
				productResponse = productPanelData.GetProductRoles(_userClaims.PersonaId, _userClaims.PersonaId, _userClaims.OrganizationPartyId, productId, null, null);				
			}

			PagedResponse response = new PagedResponse() {Meta = new Meta()};

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
				error.Errors.Add(new Error() {Title = "Error", Detail = productResponse.ErrorReason, Source = "/role", StatusCode = ""});
				return Request.CreateResponse(HttpStatusCode.BadRequest, error);
			}
		}

		#region GetExamples

		/// <summary>
		/// Used to document examples of the webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class EnterpriseRoleExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>EnterpriseRole example</returns>
			public object GetExamples()
			{

				IList<object> list = new List<object>();
				Component.SharedObjects.Product.ProductRole ulExample = new Component.SharedObjects.Product.ProductRole()
				{
					ID = "1",
					Name = "UnifiedLogin Test Role",
					Description = "UnifiedLogin Test Description",
					IsAssigned = false,
					Roletype = "System",
					Alias = "BasicUser"
				};
				list.Add(ulExample);

				Component.SharedObjects.Product.ProductRole opsExample = new Component.SharedObjects.Product.ProductRole()
				{
					ID = "21",
					Name = "Ops Test Role",
					Description = "Ops Test Description",
					IsAssigned = false,
					Roletype = "Not Used"
				};
				list.Add(opsExample);
				PagedResponse response = new PagedResponse()
				{
					Meta = new Meta() {TotalRows = list.Count, CurrentPage = 1, RowsPerPage = list.Count},
					Data = list.Cast<object>().ToList(),

				};

				return response;
			}
		}
		#endregion
	}
}
