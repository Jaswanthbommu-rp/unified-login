
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

using Serilog;
using Serilog.Events;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;
using UnifiedLogin.BusinessLogic.Attributes;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.ServiceDefaults;
using UnifiedLogin.SharedObjects.Attribute;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.OneSite;
using UnifiedLogin.SharedObjects.Product.Rum;
using UnifiedLogin.SharedObjects.ResponseObject;
using UnifiedLogin.SharedObjects.Swagger;


namespace UnifiedLogin.LandingAPIEnterprise.Controllers
{
	/// <summary>
	/// Used to get product roles
	/// </summary>
	[Authorize]
	[ApiController]
	[ApiVersion("1.0")]
	[Route("v{version:apiVersion}/[controller]")]
	public class RoleController : ControllerBase
	{
		private readonly IRepository _repository;
		private readonly HttpMessageHandler _messageHandler;
		private readonly IOneSiteProductService _oneSiteProductService;
		private IProductRepository _productRepository;
		private IManageUnifiedLogin _manageUnifiedLogin;
		private IManageOrganization _manageOrganization;
		private IManagePersona _managePersona;
		private IManagePerson _managePerson;
		private IManageProductPanel _manageProductPanel;
		private DefaultUserClaim _userClaims;
		private readonly UserClaimsAccessor _userClaimaccessor;
		/// <summary>
		/// Default constructor
		/// </summary>
		public RoleController()
		{
			// DONT USE USERCLAIM IN BASE, IT IS NULL AT THIS POINT. MOVE TO Initialize FUNCTION
		}

		/// <summary>
		/// Unit test constructor v2
		/// </summary>
		/// <param name="repository"></param>
		/// <param name="messageHandler"></param>
		/// <param name="userClaims"></param>
		/// <param name="oneSiteProductService"></param>
		/// <param name="manageProductPanel">Moqing due to dependencies with IntegrationTypeFactory that need to be addressed</param>
		/// <param name="claimsIdentity"></param>
		///  <param name="httpContextAccessor"></param>
		public RoleController(IRepository repository, HttpMessageHandler messageHandler,
			DefaultUserClaim userClaims, IOneSiteProductService oneSiteProductService, IManageProductPanel manageProductPanel,
			ClaimsIdentity claimsIdentity, HttpContextAccessor httpContextAccessor)
		{
			_userClaims = userClaims;
			_userClaimaccessor = new UserClaimsAccessor(httpContextAccessor);
			_repository = repository;
			_messageHandler = messageHandler;
			_oneSiteProductService = oneSiteProductService;

			_productRepository = new ProductRepository(repository, userClaims);
			_manageUnifiedLogin = new ManageUnifiedLogin(repository, userClaims, messageHandler);
			_managePersona = new ManagePersona(repository, userClaims, messageHandler);
			_managePerson = new ManagePerson(repository);

			var manageProductOneSite = new ManageProductOneSite(repository, userClaims, messageHandler, oneSiteProductService);
			_manageOrganization = new ManageOrganization(repository, userClaims, messageHandler, manageProductOneSite);

			_manageProductPanel = manageProductPanel;

			if (claimsIdentity != null)
			{
				ClaimsPrincipal.Current.AddIdentity(claimsIdentity);
			}
			_manageOrganization = new ManageOrganization(_userClaims);
			_productRepository = new ProductRepository(_userClaims);
			_manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
			_managePersona = new ManagePersona(_userClaims);
			_managePerson = new ManagePerson();
			_manageProductPanel = new ManageProductPanel(_userClaims);
		}



		/// <summary>
		/// Get a list of roles for the given user and product
		/// </summary>
		/// <param name="realPageId">The guid for the user being requested</param>
		/// <param name="productCode">The code for the product being requested.All Products are supported</param>
		/// <param name="upfmId">UPFM company id, can only be used with client credential token and internalapi scope</param>
		/// <returns>A list of product roles</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.NotFound, Description = "Not Found")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles", Type = typeof(ProductRole))]
		[SwaggerResponseExamples(typeof(ProductRole), typeof(EnterpriseRoleExample))]
		[Route("user/{realPageId}/product/{productCode}/roles")]
		[AuthorizeScope("enterpriseapi")]
		[Microsoft.AspNetCore.Mvc.HttpGet]
		public ActionResult GetUserProductRoles(Guid realPageId, string productCode, Guid? upfmId = null)
		{
			var errorResponse = new ErrorResponse { Errors = new List<Error>() };

			var clientCredentialLogin = AttemptClientCredentialAuthentication(upfmId);
			if (clientCredentialLogin != null && clientCredentialLogin.Errors.Count > 0)
			{
				return BadRequest(clientCredentialLogin);
			}

			var response = new PagedResponse() { Meta = new Meta() };
			var person = _managePerson.GetPerson(realPageId);
			if (person == null) return NotFound();

			var persona = _managePersona.GetFirstAvailablePersonaByCompany(realPageId, _userClaims.OrganizationPartyId);
			//Verify if same company
			if (persona == null || persona.OrganizationPartyId != _userClaims.OrganizationPartyId)
			{
				return NotFound();
			}

			ListResponse productResponse;
			List<UnifiedLogin.SharedObjects.Product.ProductRole> filteredList = new List<UnifiedLogin.SharedObjects.Product.ProductRole>();
			if (productCode.Equals("UPFM", StringComparison.OrdinalIgnoreCase))
			{
				productResponse = _manageUnifiedLogin.GetUserRoles(_userClaims.PersonaId, persona.PersonaId, _userClaims.OrganizationPartyId);
				if (productResponse != null && !productResponse.IsError)
				{
					filteredList = productResponse.Records.Cast<UnifiedLogin.SharedObjects.Product.ProductRole>().ToList().FindAll(p => p.IsAssigned);
				}
			}
			else
			{
				var productList = _productRepository.GetAllProducts();
				var productId = (int)ProductEnumHelper.GetProductIdByProductCode(productCode, productList);
				var productInternalSettings = _manageUnifiedLogin.GetProductInternalSettingByProductId(productId);

				// Check for sharedProductId setting and update productId if present
				string sharedProductSetting = productInternalSettings.FirstOrDefault(a => a.Name.Equals(SettingConstants.SharedProductSettingName, StringComparison.OrdinalIgnoreCase))?.Value;
				if (sharedProductSetting != null && int.TryParse(sharedProductSetting, out int sharedProductId))
				{
					productId = sharedProductId;
				}
				productResponse = _manageProductPanel.GetProductRoles(_userClaims.PersonaId, persona.PersonaId, _userClaims.OrganizationPartyId, productId, null, null);
				if (productResponse != null)
				{
					filteredList = productResponse.Records.Cast<UnifiedLogin.SharedObjects.Product.ProductRole>().ToList().FindAll(p => p.IsAssigned);
				}
			}

			if (productResponse != null && !productResponse.IsError)
			{
				response.Data = filteredList.Cast<object>().ToList();
				response.Meta.CurrentPage = 1;
				response.Meta.TotalRows = filteredList.Count;
				response.Meta.RowsPerPage = filteredList.Count;
				return Ok(response);
			}

			errorResponse.Errors.Add(new Error() { Title = "Error", Detail = productResponse?.ErrorReason, Source = "/role", StatusCode = "" });
			return BadRequest(errorResponse);

		}

		/// <summary>
		/// Get a list of roles for a product
		/// </summary>
		/// <param name="productCode">The code for the product being requested. All Products are supported</param>
		/// <param name="upfmId">UPFM company id, can only be used with client credential token and internalapi scope</param>
		/// <returns>A list of product roles</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[SwaggerResponse(HttpStatusCode.NotFound, Description = "Not Found")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles", Type = typeof(ProductRole))]
		[SwaggerResponseExamples(typeof(ProductRole), typeof(EnterpriseRoleExample))]
		[Route("product/{productCode}/roles")]
		[AuthorizeScope("enterpriseapi")]
		[Microsoft.AspNetCore.Mvc.HttpGet]
		public ActionResult GetProductRoles(string productCode, [FromUri] Guid? upfmId = null)
		{
			var errorResponse = new ErrorResponse { Errors = new List<Error>() };

			var clientCredentialLogin = AttemptClientCredentialAuthentication(upfmId);
			if (clientCredentialLogin != null && clientCredentialLogin.Errors.Count > 0)
			{
				return BadRequest(clientCredentialLogin);
			}

			ListResponse productResponse;

			if (productCode.Equals("UPFM", StringComparison.OrdinalIgnoreCase))
			{
				productResponse = _manageUnifiedLogin.GetRoles(_userClaims.PersonaId, _userClaims.OrganizationPartyId);
			}
			else
			{
				var productList = _productRepository.GetAllProducts();
				var productId = (int)ProductEnumHelper.GetProductIdByProductCode(productCode, productList);
				productResponse = _manageProductPanel.GetProductRoles(_userClaims.PersonaId, _userClaims.PersonaId, _userClaims.OrganizationPartyId, productId, null, null);
			}

			var response = new PagedResponse() { Meta = new Meta() };

			if (!productResponse.IsError)
			{
				response.Data = productResponse.Records;
				response.Meta.CurrentPage = 1;
				response.Meta.TotalRows = productResponse.TotalRows;
				response.Meta.RowsPerPage = productResponse.TotalRows;
				return Ok(response);
			}

			errorResponse.Errors.Add(new Error() { Title = "Error", Detail = productResponse.ErrorReason, Source = "/role", StatusCode = "" });
			return BadRequest(errorResponse);
		}

		/// <summary>
		/// Get a list of Rights for a Role
		/// </summary>
		/// <param name="productCode">The code for the product being requested. Only Unified Login controlled products are supported</param>
		/// <param name="roleId">roleId is being requested</param>
		/// <param name="upfmId">UPFM company id, can only be used with client credential token and internalapi scope</param>
		/// <returns>A list of rights for a Role</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[SwaggerResponse(HttpStatusCode.NotFound, Description = "Not Found")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles", Type = typeof(ProductRole))]
		[SwaggerResponseExamples(typeof(ProductRole), typeof(EnterpriseRoleExample))]
		[Route("product/{productCode}/roles/{roleId}/rights")]
		[AuthorizeScope("enterpriseapi")]
		[Microsoft.AspNetCore.Mvc.HttpGet]
		public ActionResult GetRightsforRole(string productCode, int roleId, Guid? upfmId = null)
		{
			if (string.IsNullOrEmpty(productCode))
				return BadRequest("ProductCode not supplied.");
			if (roleId == 0)
				return BadRequest("roleId not supplied.");

			var clientCredentialLogin = AttemptClientCredentialAuthentication(upfmId);
			if (clientCredentialLogin != null && clientCredentialLogin.Errors.Count > 0)
			{
				return BadRequest(clientCredentialLogin.Errors);
			}

			return Ok(_manageUnifiedLogin.GetListRightbyRole(productCode, roleId));
		}

		/// <summary>
		/// Used for client credential calls to assign the support tool admin user when attempting to get company information
		/// </summary>
		/// <param name="upfmId"></param>
		/// <returns></returns>
		private ErrorResponse AttemptClientCredentialAuthentication(Guid? upfmId)
		{
			if (string.IsNullOrEmpty(upfmId.ToString())) return null;
			var currentClaimPrincipal = ClaimsPrincipal.Current;

			if ((!currentClaimPrincipal.HasClaim("scope", "usermanagement") && !currentClaimPrincipal.HasClaim("scope", "internalapi")) || _userClaims.PersonaId != 0) return null;

			var adminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(upfmId ?? default(Guid));
			//recreate clams
			if (adminCreatorRealPageId == Guid.Empty)
			{
				return new ErrorResponse { Errors = new List<Error>() { new Error { Title = "Error", Source = "/role", Detail = "Invalid UPFMId.", StatusCode = "" } } };
			}

			//userClaimaccessor.RecreateClaimsForClient(adminCreatorRealPageId); //TODO: commented this code as part of upgradation to /net 10.0, need to fix this :-Raghavender
			if (_repository == null)
			{
				_manageOrganization = new ManageOrganization(_userClaims);
				_productRepository = new ProductRepository(_userClaims);
				_manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
				_managePersona = new ManagePersona(_userClaims);
				_managePerson = new ManagePerson();
				_manageProductPanel = new ManageProductPanel(_userClaims);
			}
			else
			{
				// from unit test
				_productRepository = new ProductRepository(_repository, _userClaims);
				_manageUnifiedLogin = new ManageUnifiedLogin(_repository, _userClaims, _messageHandler);
				_managePersona = new ManagePersona(_repository, _userClaims, _messageHandler);
				_managePerson = new ManagePerson(_repository);

				var manageProductOneSite = new ManageProductOneSite(_repository, _userClaims, _messageHandler, _oneSiteProductService);
				_manageOrganization = new ManageOrganization(_repository, _userClaims, _messageHandler, manageProductOneSite);
			}

			return null;
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
				UnifiedLogin.SharedObjects.Product.ProductRole ulExample = new UnifiedLogin.SharedObjects.Product.ProductRole()
				{
					ID = "1",
					Name = "UnifiedLogin Test Role",
					Description = "UnifiedLogin Test Description",
					IsAssigned = false,
					Roletype = "System",
					Alias = "BasicUser"
				};
				list.Add(ulExample);

				UnifiedLogin.SharedObjects.Product.ProductRole opsExample = new UnifiedLogin.SharedObjects.Product.ProductRole()
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
					Meta = new Meta() { TotalRows = list.Count, CurrentPage = 1, RowsPerPage = list.Count },
					Data = list.Cast<object>().ToList(),

				};

				return response;
			}
		}
		#endregion
	}
}
