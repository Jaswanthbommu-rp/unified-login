using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.EnterpriseRole;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers.Product
{
    /// <summary>
    /// Controller for product panel related APIs
    /// </summary>
    public class ProductPanelController : BaseApiController
    {

		#region Private variables

		private readonly IProductRepository _productRepository;
		private Guid emptyGuid = Guid.Empty;		
		private IManageProductPanel _manageProductPanel;
		private ProductInternalSettingRepository _productInternalSettingRepository;
		private IManageBlueBook _manageBlueBook;
		private bool _excludeTest = false;
		private IManagePersona _personaManager;
		public HttpMessageHandler MessageHandler { get; }
		#endregion
		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public ProductPanelController() : base()
		{
		}

		/// <summary>
		/// Testing Constructor
		/// </summary>
		/// <param name="_defaultUserClaim"></param>
		/// <param name="repository">Product Repository</param>
		/// <param name="repositoryResponse"></param>
		/// <param name="messageHandler"></param>
		/// <param name="manageProductOneSite"></param>
		public ProductPanelController(DefaultUserClaim _defaultUserClaim, IRepository repository, IRepositoryResponse repositoryResponse, HttpMessageHandler messageHandler, IManageProductOneSite manageProductOneSite)
		{
			_userClaims = _defaultUserClaim;
			_productRepository = new ProductRepository(repository, _defaultUserClaim);
			_productInternalSettingRepository = new ProductInternalSettingRepository(repository);
			_manageBlueBook = new ManageBlueBook(_defaultUserClaim, repository, _productInternalSettingRepository, messageHandler);
			_manageProductPanel = new ManageProductPanel(_defaultUserClaim, repository, _manageBlueBook, messageHandler, manageProductOneSite);
			_excludeTest = true;
			MessageHandler = messageHandler;
			_personaManager = new ManagePersona(_defaultUserClaim);
		}

		/// <summary>
		/// Used to initialize the base class  
		/// </summary>
		/// <param name="controllerContext"></param>
		protected override void Initialize(HttpControllerContext controllerContext)
		{
			base.Initialize(controllerContext);		
			_manageProductPanel = new ManageProductPanel(_userClaims);
			_personaManager = new ManagePersona(_userClaims);

		}
		#endregion
		#region Public methods
		/// <summary>
		/// Returns Roles 
		/// </summary>
		/// <param name="editorPersonaId">Assign user Id</param>
		/// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
		/// <param name="partyId">Author user persona id who is creating or editing user</param>
		/// <param name="productId">Author user persona id who is creating or editing user</param>
		/// <param name="datafilter">A datafilter used to filter the roles.</param>
		/// <param name="accessType">Variable used for Vendor Credential product</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("product/roles")]
		[HttpGet]
		public HttpResponseMessage GetRoles(long editorPersonaId, long userPersonaId, long partyId,int productId, [FromUri]RequestParameter datafilter, AccessType? accessType = null)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");
			ListResponse result = new ListResponse();

			result = _manageProductPanel.GetProductRoles(editorPersonaId, userPersonaId, partyId, productId, datafilter, accessType);		


			if (result.IsError)
				Request.CreateResponse(HttpStatusCode.Forbidden, result);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Returns user product Roles 
		/// </summary>
		/// <param name="editorPersonaId">Author user persona id who is creating or editing user</param>
		/// <param name="realPageId">user realPageId</param>
		/// <param name="partyId">Organization partyid</param>		
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("product/userproductroles")]
		[HttpGet]
		public HttpResponseMessage GetUserProductRoles(long editorPersonaId, long partyId, Guid realPageId)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			if (realPageId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "User RealPageId empty.");
			
			var persona = _personaManager.GetFirstAvailablePersonaByCompany(realPageId, partyId);
			if ((persona == null) || (persona.PersonaId == 0))
			{				
				return Request.CreateResponse(HttpStatusCode.Forbidden, "Get active persona: Invalid parameter enterprise User Id");
			}

			var userProductRoles = _manageProductPanel.GetUserProductRoles(editorPersonaId, persona.PersonaId, partyId);

			return Request.CreateResponse(HttpStatusCode.OK, userProductRoles);
		}

		//TODO: Make this API as [Obsolete] after UI Integration of Primary properties
		/// <summary>
		/// Returns Properties  
		/// </summary>
		/// <param name="editorPersonaId">Assign user Id</param>
		/// <param name="userPersonaId">Author user persona id who is creating or editing user</param> 
		/// <param name="productId">Author user persona id who is creating or editing user</param>
		/// <param name="datafilter">A datafilter used to filter the properties.</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("product/properties")]		
		[HttpGet]
		public HttpResponseMessage GetProperties(long editorPersonaId, long userPersonaId, int productId, [FromUri]RequestParameter datafilter)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (!_excludeTest && _realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			ListResponse result = new ListResponse();
			result = _manageProductPanel.GetProductProperties(editorPersonaId, userPersonaId, productId, datafilter);
				
			if (result.IsError)
				Request.CreateResponse(HttpStatusCode.Forbidden, result);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Returns Properties  
		/// </summary>
		/// <param name="editorPersonaId">Assign user Id</param>
		/// <param name="userPersonaId">Author user persona id who is creating or editing user</param> 
		/// <param name="productId">Author user persona id who is creating or editing user</param>
		/// <param name="datafilter">A datafilter used to filter the properties.</param>
		/// <param name="upfmProperty">upfmProperty</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("product/properties")]
		[HttpPost]
		public HttpResponseMessage GetProperties(long editorPersonaId, long userPersonaId, int productId, [FromUri] RequestParameter datafilter, [FromBody] UPFMProperty upfmProperty)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (!_excludeTest && _realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			ListResponse result = new ListResponse();
			result = _manageProductPanel.GetProductProperties(editorPersonaId, userPersonaId, productId, datafilter);

			if (!result.IsError) // && result.Records.Count > 0 && upfmProperty?.id != null
			{
				result = _manageProductPanel.CompareProductAndPrimaryProperties(upfmProperty, productId, result);
			}
			if (result.IsError)
				Request.CreateResponse(HttpStatusCode.Forbidden, result);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Get Persona Product Primary Properties
		/// </summary>
		/// <param name="userPersonaId"></param>
		/// <returns></returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("product/productPrimaryProperties")]
		[HttpGet]
		public HttpResponseMessage GetPersonaProductPrimaryProperties(long userPersonaId)
		{
			if (userPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "userPersonaId not supplied.");

			List<PersonaProductProperty> result = new List<PersonaProductProperty>();
			result = _manageProductPanel.GetPersonaProductPrimaryProperties(userPersonaId);
			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Returns Properties  
		/// </summary>
		/// <param name="productId">Author user persona id who is creating or editing user</param>
		/// <param name="upfmProperty">Properties to be translated</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("product/{productId}/translateProductProperties")]
		[HttpPost]
		[Obsolete]
		public HttpResponseMessage GetTranslatedProperties([FromBody] UPFMProperty upfmProperty, int productId)
		{
			var result = new UPFMProperty();
			
			if (upfmProperty?.id != null)
			{
				result = _manageProductPanel.TranslateProductProperties(upfmProperty, productId);
			}

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Returns Property groups
		/// </summary>
		/// <param name="editorPersonaId">Assign user Id</param>
		/// <param name="userPersonaId">Author user persona id who is creating or editing user</param> 
		/// <param name="productId">Author user persona id who is creating or editing user</param>
		/// <param name="datafilter">A datafilter used to filter the properties.</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("product/propertygroups")]
		[HttpGet]
		public HttpResponseMessage GetPropertyGroups(long editorPersonaId, long userPersonaId, int productId, [FromUri]RequestParameter datafilter)
		{
			var completeRoute = this.ControllerContext.RouteData.Route;
			string method = completeRoute.RouteTemplate.Substring(completeRoute.RouteTemplate.IndexOf("/"));

			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			ListResponse result = new ListResponse();

			result = _manageProductPanel.GetProductPropertyGroups(editorPersonaId, userPersonaId, productId, datafilter);

			if (result.IsError)
				Request.CreateResponse(HttpStatusCode.Forbidden, result);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}
		/// <summary>
		/// Returns Rights  
		/// </summary>
		/// <param name="editorPersonaId">Assign user Id</param>
		/// <param name="productId">Author user persona id who is creating or editing user</param>
		/// <param name="datafilter">A datafilter used to filter the properties.</param>
		/// <param name="assignedToRoleOnly"></param>
		/// <param name="partyId"></param>
		/// <param name="roleId"></param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("product/rightsforrole")]
		[HttpGet]
		public HttpResponseMessage GetRights(long editorPersonaId, int productId,string roleId, long partyId,[FromUri]RequestParameter datafilter, bool assignedToRoleOnly = false)
		{
			var completeRoute = this.ControllerContext.RouteData.Route;
			string method = completeRoute.RouteTemplate.Substring(completeRoute.RouteTemplate.IndexOf("/"));

			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			if (String.IsNullOrWhiteSpace(roleId) || roleId == "0")
				return Request.CreateResponse(HttpStatusCode.BadRequest, "roleId not supplied.");

			ListResponse result = new ListResponse();

			result = _manageProductPanel.GetProductRightsForRole(editorPersonaId, roleId, partyId , productId, datafilter, assignedToRoleOnly);

			if (result.IsError)
				Request.CreateResponse(HttpStatusCode.Forbidden, result);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Returns  group Properties
		/// </summary>
		/// <param name="editorPersonaId">Assign user Id</param>
		/// <param name="userPersonaId">Author user persona id who is creating or editing user</param> 
		/// <param name="productId">Author user persona id who is creating or editing user</param>
		/// <param name="propertyGroupId"></param>
		/// <param name="datafilter">A datafilter used to filter the properties.</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("product/groupproperties")]
		[HttpGet]
		public HttpResponseMessage GetProductGroupProperties(long editorPersonaId, long userPersonaId, int productId, string propertyGroupId, [FromUri]RequestParameter datafilter)
		{
			var completeRoute = this.ControllerContext.RouteData.Route;
			string method = completeRoute.RouteTemplate.Substring(completeRoute.RouteTemplate.IndexOf("/"));

			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			if ( string.IsNullOrEmpty(propertyGroupId) ||  propertyGroupId == "0")
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Group Id.");

			ListResponse result = new ListResponse();

			result = _manageProductPanel.GetProductGroupProperties(editorPersonaId, userPersonaId, productId, propertyGroupId, datafilter);

			if (result.IsError)
				Request.CreateResponse(HttpStatusCode.Forbidden, result);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}



		/// <summary>
		/// Returns  group Properties
		/// </summary>
		/// <param name="editorPersonaId">Assign user Id</param>
		/// <param name="userPersonaId">Author user persona id who is creating or editing user</param> 
		/// <param name="productId">Author user persona id who is creating or editing user</param>
		/// <param name="organizationRoleId"></param>
		/// <param name="organizationType">A datafilter used to filter the properties.</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("product/organizations")]
		[HttpGet]
		public HttpResponseMessage GetProductOrganizations(long editorPersonaId, long userPersonaId, int productId, string organizationRoleId, string organizationType)
		{
			var completeRoute = this.ControllerContext.RouteData.Route;
			string method = completeRoute.RouteTemplate.Substring(completeRoute.RouteTemplate.IndexOf("/"));

			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			if (string.IsNullOrEmpty(organizationRoleId))
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Organization Role Id.");

			if(string.IsNullOrEmpty(organizationType))
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Organization Type");

			ListResponse result = new ListResponse();

			result = _manageProductPanel.GetProductOrganizations(editorPersonaId, userPersonaId, productId, organizationRoleId, organizationType);

			if (result.IsError)
				Request.CreateResponse(HttpStatusCode.Forbidden, result);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Returns Location groups
		/// </summary>
		/// <param name="editorPersonaId">Assign user Id</param>
		/// <param name="userPersonaId">Author user persona id who is creating or editing user</param> 
		/// <param name="productId">Author user persona id who is creating or editing user</param>
		/// <param name="datafilter">A datafilter used to filter the properties.</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("product/locationgroups")]
		[HttpGet]
		public HttpResponseMessage GetLocationGroups(long editorPersonaId, long userPersonaId, int productId, [FromUri] RequestParameter datafilter)
		{
			var completeRoute = this.ControllerContext.RouteData.Route;
			string method = completeRoute.RouteTemplate.Substring(completeRoute.RouteTemplate.IndexOf("/"));

			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			ListResponse result = new ListResponse();

			result = _manageProductPanel.GetProductLocationGroups(editorPersonaId, userPersonaId, productId, datafilter);

			if (result.IsError)
				Request.CreateResponse(HttpStatusCode.Forbidden, result);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Product Access Types
		/// </summary>
		/// <param name="editorPersonaId">Assign user Id</param>
		/// <param name="userPersonaId">Author user persona id who is creating or editing user</param> 
		/// <param name="productId">Author user persona id who is creating or editing user</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Successfully received access types", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when information is out of sync with the server)")]
		[Route("product/accessTypes")]
		[HttpGet]
		public HttpResponseMessage GetAccessTypes(long editorPersonaId, long userPersonaId, int productId)
		{
			if (editorPersonaId == 0)
            {
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
			}
			else if (_realpageUserId == Guid.Empty)
            {
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");
			}

			var result = _manageProductPanel.GetProductAccessTypes(editorPersonaId, userPersonaId, productId);

			if (result.IsError)
            {
				return Request.CreateResponse(HttpStatusCode.Forbidden, result);
			}

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}
		#endregion
	}
}
