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
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.ResponseObject;
using System.Collections.Generic;
using System.Security.Claims;
using System;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	public class UnifiedLoginController : BaseApiController
	{
        private IManageOrganization _manageOrganization;
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
			_manageOrganization = new ManageOrganization(_userClaims);
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
        public HttpResponseMessage GetRoles(long editorPersonaId, long partyId, [FromUri] RequestParameter datafilter, Guid? upfmId = null)
        {
            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            if (editorPersonaId == 0)
            {
                if (currentClaimPrincipal.HasClaim("scope", "internalapi") && _userClaims.PersonaId == 0)
                {
                    if (!string.IsNullOrEmpty(upfmId.ToString()))
                    {
                        Guid AdminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(upfmId ?? default(Guid));
                        if (AdminCreatorRealPageId == Guid.Empty)
                        {
                            var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                            errorResponse.Errors.Add(new Error { Title = "Error", Source = "/product", Detail = "Invalid UPFMId.", StatusCode = "" });
                        }
                        RecreateClaimsForClient(AdminCreatorRealPageId);
                        editorPersonaId = _userClaims.PersonaId;
                        if (editorPersonaId == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "invalid request.");
                        }
                        _manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
                    }
                }
            }
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
		public HttpResponseMessage GetAllRightsByRole(long editorPersonaId, long partyId, long roleId, [FromUri] RequestParameter datafilter, Guid? upfmId = null)
		{
            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            if (editorPersonaId == 0)
            {
                if (currentClaimPrincipal.HasClaim("scope", "internalapi") && _userClaims.PersonaId == 0)
                {
                    if (!string.IsNullOrEmpty(upfmId.ToString()))
                    {
                        Guid AdminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(upfmId ?? default(Guid));
                        if (AdminCreatorRealPageId == Guid.Empty)
                        {
                            var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                            errorResponse.Errors.Add(new Error { Title = "Error", Source = "/product", Detail = "Invalid UPFMId.", StatusCode = "" });
                        }
                        RecreateClaimsForClient(AdminCreatorRealPageId);
                        editorPersonaId = _userClaims.PersonaId;
                        if (editorPersonaId == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "invalid request.");
                        }
                        _manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
                    }
                }
            }
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

        /// <summary>
        /// Used to recreate claims for client
        /// </summary>
        /// <param name="realpageUserId">RealPage UserId</param>
        private void RecreateClaimsForClient(Guid realpageUserId)
        {
            if (string.IsNullOrEmpty(realpageUserId.ToString())) return;

            var rpCache = new RPObjectCache();
            _realpageUserId = realpageUserId;

            var cacheKey = $"recreateClaimsForClient_{realpageUserId}";
            _userClaims = rpCache.GetFromCache<DefaultUserClaim>(cacheKey, 180, () =>
            {
                IManagePerson personLogic = new ManagePerson();
                Person person = personLogic.GetPerson(realpageUserId);
                if (person == null)
                {
                    throw new Exception($"Missing persona information for client_info user while Recreation of Claims For Client.  realPageId: {realpageUserId}");
                }

                IManageUserLogin userLoginLogic = new ManageUserLogin();
                IManageUserRoleRight userRoleRight = new ManageUserRoleRight();
                var userLogin = userLoginLogic.GetUserLoginOnly(realpageUserId);

                IManagePersona managePersona = new ManagePersona();
                //Active Persona is linked to one organization
                Persona persona = managePersona.GetActivePersonaWithoutRights(realpageUserId); // this user can only be under 1 company to work correctly
                var roles = userRoleRight.GetAssignedRoleForPersona(ProductEnum.UnifiedPlatform, persona.PersonaId);
                var claim = new DefaultUserClaim
                {
                    UserId = (int)userLogin.UserId,
                    OrganizationPartyId = persona.Organization.PartyId,
                    LoginName = userLogin.LoginName,
                    OrganizationMasterId = (long)persona.Organization.BooksMasterId,
                    CustomerMasterId = (long)persona.Organization.BooksMasterId,
                    OrganizationName = persona.Organization.Name,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    PersonaId = persona.PersonaId,
                    OrganizationRealPageGuid = persona.Organization.RealPageId,
                    UserRealPageGuid = _realpageUserId,
                    CorrelationId = Guid.NewGuid(),
                    RealPageEmployee = persona.Organization.RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId,
                };
                ClaimsPrincipal userPrincipal = ClaimsPrincipal.Current;
                var identity = (ClaimsIdentity)userPrincipal.Identity;
                identity.AddClaims(roles.Select(r => new Claim("roleid", r.RoleID.ToString())).ToList());

                claim.Rights = BaseUserRights.GetUserRightsBy(userPrincipal, claim);
                return claim;

            });

        }
    }
}
