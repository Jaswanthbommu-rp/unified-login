using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
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
		/// <param name="userClaim"></param>
		/// <param name="productRepository">Product Repository</param>
		public ProductPanelController(DefaultUserClaim userClaim, IProductRepository productRepository)
		{
			_userClaims = userClaim;
			_productRepository = productRepository;			
		}

		/// <summary>
		/// Used to initialize the base class  
		/// </summary>
		/// <param name="controllerContext"></param>
		protected override void Initialize(HttpControllerContext controllerContext)
		{
			base.Initialize(controllerContext);		
			_manageProductPanel = new ManageProductPanel(_userClaims);

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
		/// Returns Rights 
		/// </summary>
		/// <param name="editorPersonaId">Assign user Id</param>
		/// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
		/// <param name="partyId">Author user persona id who is creating or editing user</param>
		/// <param name="productId">Author user persona id who is creating or editing user</param>
		/// <param name="datafilter">A datafilter used to filter the roles.</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("product/productrights")]
		[HttpGet]
		public HttpResponseMessage GetRights(long editorPersonaId, long userPersonaId, long partyId, int productId, [FromUri]RequestParameter datafilter)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");
			ListResponse result = new ListResponse();

			result = _manageProductPanel.GetProductRights(editorPersonaId, userPersonaId, partyId, productId, datafilter);


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
		public HttpResponseMessage GetProperties(long editorPersonaId, long userPersonaId, int productId, [FromUri]RequestParameter datafilter, [FromBody] UPFMProperty upfmProperty)
		{
			var completeRoute = this.ControllerContext.RouteData.Route;
			string method = completeRoute.RouteTemplate.Substring(completeRoute.RouteTemplate.IndexOf("/"));

			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			ListResponse result = new ListResponse();

			result = _manageProductPanel.GetProductProperties(editorPersonaId, userPersonaId, productId, datafilter);
			//TODO: compare result and send it back to UI
			if (!result.IsError)
			{
				result = _manageProductPanel.CompareProductAndPrimaryProperties(upfmProperty, productId, result);
			}
				
			if (result.IsError)
				Request.CreateResponse(HttpStatusCode.Forbidden, result);

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
		public HttpResponseMessage GetRights(long editorPersonaId, int productId,int roleId, long partyId,[FromUri]RequestParameter datafilter, bool assignedToRoleOnly = false)
		{
			var completeRoute = this.ControllerContext.RouteData.Route;
			string method = completeRoute.RouteTemplate.Substring(completeRoute.RouteTemplate.IndexOf("/"));

			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

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
		public HttpResponseMessage GetProductGroupProperties(long editorPersonaId, long userPersonaId, int productId, int propertyGroupId, [FromUri]RequestParameter datafilter)
		{
			var completeRoute = this.ControllerContext.RouteData.Route;
			string method = completeRoute.RouteTemplate.Substring(completeRoute.RouteTemplate.IndexOf("/"));

			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			if (propertyGroupId == 0)
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
		public HttpResponseMessage GetLocationGroups(long editorPersonaId, long userPersonaId, int productId, [FromUri]RequestParameter datafilter)
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
		#endregion
	}
}
