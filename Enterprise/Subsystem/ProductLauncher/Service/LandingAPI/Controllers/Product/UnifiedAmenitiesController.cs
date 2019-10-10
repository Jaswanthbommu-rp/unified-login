using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using Swashbuckle.Swagger.Annotations;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers.Product
{
	/// <summary>
	/// Controller for all product management related APIs
	/// </summary>
	public class UnifiedAmenitiesController : BaseApiController
	{
		private IManageUnifiedAmenities _manageUnifiedAmenities;

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public UnifiedAmenitiesController() : base() { }

		/// <summary>
		/// Used for unit testing
		/// </summary> 
		public UnifiedAmenitiesController(IManageUnifiedAmenities manageUnifiedAmenities)
		{
			_manageUnifiedAmenities = manageUnifiedAmenities;
			base.Request = new HttpRequestMessage();
			base.Request.SetConfiguration(new HttpConfiguration());
		}

		/// <summary>
		/// Used to initialize the base class  
		/// </summary>
		/// <param name="controllerContext"></param>
		protected override void Initialize(HttpControllerContext controllerContext)
		{
			base.Initialize(controllerContext);
			_manageUnifiedAmenities = new ManageUnifiedAmenities(_userClaims);
		}

		#endregion

		/// <summary>
		/// Returns Roles for the given user and company
		/// </summary>
		/// <param name="editorPersonaId">User making the request</param>
		/// <param name="userPersonaId">The user id to merge with the roles list, if used. 0 for all roles</param>  
		/// <param name="partyId">The company id to use for the request</param>
		/// <param name="datafilter">A datafilter used to filter the result. Not currently used</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List of roles by partyid", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/unifiedamenities/roles")]
		[HttpGet]
		public HttpResponseMessage GetRoles(long editorPersonaId, long userPersonaId, long partyId, [FromUri]RequestParameter datafilter)
		{
			if (partyId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "partyId not supplied.");

			var result = _manageUnifiedAmenities.GetRoles(editorPersonaId, userPersonaId, partyId);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Used to get the rights for the given party and role Id
		/// </summary>
		/// <param name="editorPersonaId">User making the request</param>
		/// <param name="partyId">The company id to use for the request</param>
		/// <param name="roleId">The role id to retrieve rights for</param>
		/// <param name="datafilter">A datafilter used to filter the result. Not currently used</param>
		/// <returns></returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List of rights by roleid", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/unifiedamenities/role/rights")]
		[HttpGet]
		public HttpResponseMessage GetRightsByRole(long editorPersonaId, long partyId, long roleId, [FromUri] RequestParameter datafilter)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
			if (partyId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "partyId not supplied.");
			return Request.CreateResponse(HttpStatusCode.OK, _manageUnifiedAmenities.GetRightsByRole(editorPersonaId, partyId, roleId));
		}

		/// <summary>
		/// Used to get the list of properties for the given user
		/// </summary>
		/// <param name="editorPersonaId">User making the request</param>
		/// <param name="userPersonaId">The user id to merge with the property list, if used. 0 for all properties</param>
		/// <param name="assignedOnly">Only return the properties assigned to the given user persona id</param>
		/// <param name="datafilter">A datafilter used to filter the result. Not currently used</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/unifiedamenities/user/properties")]
		[AuthorizeScope("rplandingapi","userinfoapi")]
		[HttpGet]
		public HttpResponseMessage GetProperties(long editorPersonaId, long userPersonaId, bool assignedOnly, [FromUri]RequestParameter datafilter)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			var result = _manageUnifiedAmenities.GetProperties(editorPersonaId, userPersonaId, assignedOnly, datafilter);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}
	}
}
