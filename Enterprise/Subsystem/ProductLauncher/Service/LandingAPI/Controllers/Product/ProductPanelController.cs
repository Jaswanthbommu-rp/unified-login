using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.SAML;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;
using Thinktecture.IdentityModel.Client;
using static RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.SAML.RealPageSAML;

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
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("product/roles")]
		[HttpGet]
		public HttpResponseMessage GetRoles(long editorPersonaId, long userPersonaId, long partyId,int productId, [FromUri]RequestParameter datafilter)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");
			ListResponse result = new ListResponse();

			result = _manageProductPanel.GetProductRoles(editorPersonaId, userPersonaId, partyId, productId, datafilter);		


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
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("product/properties")]
		[HttpGet]
		public HttpResponseMessage GetProperties(long editorPersonaId, long userPersonaId, int productId, [FromUri]RequestParameter datafilter)
		{
			var completeRoute = this.ControllerContext.RouteData.Route;
			string method = completeRoute.RouteTemplate.Substring(completeRoute.RouteTemplate.IndexOf("/"));

			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			ListResponse result = new ListResponse();

			result = _manageProductPanel.GetProductProperties(editorPersonaId, userPersonaId, productId, datafilter);

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
		#endregion
	}
}
