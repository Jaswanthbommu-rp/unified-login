using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	public class UnifiedLoginController : BaseApiController
	{
		private IManageUnifiedLogin _manageUnifiedLogin;

		public UnifiedLoginController() : base()
		{
		}

		public UnifiedLoginController(IManageUnifiedLogin manageUserManagement)
		{
			_manageUnifiedLogin = manageUserManagement;

			base.Request = new HttpRequestMessage();
			base.Request.SetConfiguration(new HttpConfiguration());
		}

		protected override void Initialize(HttpControllerContext controllerContext)
		{
			base.Initialize(controllerContext);
			_manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
		}

		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List of roles by partyid", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/unifiedlogin/user/roles")]
		[HttpGet]
		public HttpResponseMessage GetUserRoles(long editorPersonaId, long userPersonaId, long partyId, [FromUri] RequestParameter datafilter)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
			if (partyId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "partyId not supplied.");
			return Request.CreateResponse(HttpStatusCode.OK, _manageUnifiedLogin.GetUserRoles(editorPersonaId, userPersonaId, partyId));
		}

		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Delete a Role", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[Route("products/unifiedlogin/role")]
		[Authorize]
		[HttpDelete]
		public HttpResponseMessage DeleteRole(long editorPersonaId, int roleId)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
			if (roleId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "roleId not supplied.");
			ListResponse listResponse = _manageUnifiedLogin.DeleteRole(editorPersonaId, (long) roleId);
			if (!string.IsNullOrEmpty(listResponse.ErrorReason))
				return Request.CreateResponse(HttpStatusCode.BadRequest, listResponse);
			return Request.CreateResponse(HttpStatusCode.OK, listResponse);
		}

		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Set a Default Role", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[Route("products/unifiedlogin/setdefaultrole")]
		[Authorize]
		[HttpPut]
		public HttpResponseMessage SetDefaultRole(long editorPersonaId, int roleId)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
			if (roleId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "roleId not supplied.");
			ListResponse listResponse = _manageUnifiedLogin.SetDefaultRole(editorPersonaId,_userClaims.OrganizationPartyId, (long) roleId);
			if (!string.IsNullOrEmpty(listResponse.ErrorReason))
				return Request.CreateResponse(HttpStatusCode.BadRequest, listResponse);
			return Request.CreateResponse(HttpStatusCode.OK, listResponse);
		}

		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List of roles by partyid", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/unifiedlogin/roles")]
		[HttpGet]
		public HttpResponseMessage GetRoles(long editorPersonaId, long partyId, [FromUri] RequestParameter datafilter)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
			if (partyId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "partyId not supplied.");
			return Request.CreateResponse(HttpStatusCode.OK, _manageUnifiedLogin.GetRoles(editorPersonaId, partyId));
		}

		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List of roles by partyid", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/unifiedlogin/rolesCount")]
		[HttpGet]
		public HttpResponseMessage GetRolesWithCount(long editorPersonaId, long partyId, [FromUri] RequestParameter datafilter)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
			if (partyId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "partyId not supplied.");
			return Request.CreateResponse(HttpStatusCode.OK, _manageUnifiedLogin.GetRolesWithCount(editorPersonaId, partyId));
		}

		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List of roles by partyid", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/unifiedlogin/rights")]
		[HttpGet]
		public HttpResponseMessage GetRights(long editorPersonaId, long partyId, [FromUri] RequestParameter datafilter)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
			if (partyId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "partyId not supplied.");
			return Request.CreateResponse(HttpStatusCode.OK, _manageUnifiedLogin.GetRights(editorPersonaId, partyId));
		}

		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List of Rights with Count by partyid", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/unifiedlogin/rightsCount")]
		[HttpGet]
		public HttpResponseMessage GetRightsWithCount(long editorPersonaId, long partyId, [FromUri] RequestParameter datafilter)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
			if (partyId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "partyId not supplied.");
			return Request.CreateResponse(HttpStatusCode.OK, _manageUnifiedLogin.GetRightsWithCount(editorPersonaId, partyId));
		}

		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List of rights by roleid", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/unifiedlogin/role/allrights")]
		[HttpGet]
		public HttpResponseMessage GetAllRightsByRole(long editorPersonaId, long partyId, long roleId, [FromUri] RequestParameter datafilter)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
			if (partyId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "partyId not supplied.");
			return Request.CreateResponse(HttpStatusCode.OK, _manageUnifiedLogin.GetAllRightsByRole(editorPersonaId, partyId, roleId));
		}

		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List of rights by roleid", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/unifiedlogin/role/rights")]
		[HttpGet]
		public HttpResponseMessage GetRightsByRole(long editorPersonaId, long partyId, long roleId, [FromUri] RequestParameter datafilter)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
			if (partyId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "partyId not supplied.");
			return Request.CreateResponse(HttpStatusCode.OK, _manageUnifiedLogin.GetRightsByRole(editorPersonaId, partyId, roleId));
		}

		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List of Roles by rightid", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/unifiedlogin/right/roles")]
		[HttpGet]
		public HttpResponseMessage GetRolesByRight(long editorPersonaId, long partyId, long rightId, [FromUri] RequestParameter datafilter)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
			if (partyId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "partyId not supplied.");
			return Request.CreateResponse(HttpStatusCode.OK, _manageUnifiedLogin.GetRolesByRight(editorPersonaId, partyId, rightId));
		}

		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Create New Role", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[Route("products/unifiedlogin/role")]
		[Authorize]
		[HttpPost]
		public HttpResponseMessage AddRole(long editorPersonaId, long partyId, string roleName, string inheritRoleId = null)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
			if (partyId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "partyId not supplied.");
			if (string.IsNullOrEmpty(roleName.Trim()))
				return Request.CreateResponse(HttpStatusCode.BadRequest, "roleName not supplied.");
			ListResponse listResponse = _manageUnifiedLogin.AddUpdateRole(editorPersonaId, partyId, 0, roleName, inheritRoleId);
			//if (!string.IsNullOrEmpty(listResponse.ErrorReason))
			//    return Request.CreateResponse(HttpStatusCode.BadRequest, listResponse);
			return Request.CreateResponse(HttpStatusCode.OK, listResponse);
		}

		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Create New Role", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[Route("products/unifiedlogin/role")]
		[Authorize]
		[HttpPut]
		public HttpResponseMessage UpdateRole(long editorPersonaId, long partyId, long roleid, string roleName, string inheritRoleId = null)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
			if (partyId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "partyId not supplied.");
			if (roleid == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "roleid not supplied.");
			if (string.IsNullOrEmpty(roleName.Trim()))
				return Request.CreateResponse(HttpStatusCode.BadRequest, "roleName not supplied.");
			ListResponse listResponse = _manageUnifiedLogin.AddUpdateRole(editorPersonaId, partyId, roleid, roleName, inheritRoleId);
			if (!string.IsNullOrEmpty(listResponse.ErrorReason))
				return Request.CreateResponse(HttpStatusCode.BadRequest, listResponse);
			return Request.CreateResponse(HttpStatusCode.OK, listResponse);
		}

		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles for the given PMC", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[Route("products/unifiedlogin/role/rights")]
		[Authorize]
		[HttpPut]
		public HttpResponseMessage UpdateRoleRights(long editorPersonaId, int roleId, ULRightsAddRemoveList rightsToAddRemove)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
			if (roleId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "roleId not supplied.");
			if (rightsToAddRemove.RightsToAdd == null && rightsToAddRemove.RightsToDelete == null)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "No Data");
			ListResponse response = _manageUnifiedLogin.UpdateRightsToRole(editorPersonaId, (long) roleId, rightsToAddRemove.RightsToAdd, rightsToAddRemove.RightsToDelete);
			//if (!string.IsNullOrEmpty(response.ErrorReason))
			//    return Request.CreateResponse(HttpStatusCode.BadRequest, response);
			return Request.CreateResponse(HttpStatusCode.OK, response);
		}

        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles for the given PMC", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [Route("products/unifiedlogin/clone/rights")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage CloneRoleRights(long editorPersonaId, int roleId, ULRightsAddRemoveList rightsToAddRemove)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
            if (roleId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "roleId not supplied.");
            if (rightsToAddRemove.RightsToAdd == null && rightsToAddRemove.RightsToDelete == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No Data");
            ListResponse response = _manageUnifiedLogin.CloneRightsToRole(editorPersonaId, (long)roleId, rightsToAddRemove.RightsToAdd, rightsToAddRemove.RightsToDelete);
           
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles for the given PMC", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[Route("products/unifiedlogin/right/roles")]
		[Authorize]
		[HttpPut]
		public HttpResponseMessage UpdateRightRoles(long editorPersonaId, int rightId, RolesAddRemoveList rolesToAddRemove)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
			if (rightId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "roleId not supplied.");
			if (rolesToAddRemove.RolesToAdd == null && rolesToAddRemove.RolesToDelete == null)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "No Data");
			ListResponse response = _manageUnifiedLogin.UpdateRolesByRight(editorPersonaId, (long) rightId, rolesToAddRemove.RolesToAdd, rolesToAddRemove.RolesToDelete);
			if (!string.IsNullOrEmpty(response.ErrorReason))
				return Request.CreateResponse(HttpStatusCode.BadRequest, response);
			return Request.CreateResponse(HttpStatusCode.OK, response);
		}
	}
}
