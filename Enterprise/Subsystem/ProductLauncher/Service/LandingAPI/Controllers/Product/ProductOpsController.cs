using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.ResponseObject;
using Swashbuckle.Swagger.Annotations;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers.Product
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductOpsController : BaseApiController
    {
        private IManageOrganization _manageOrganization;
        IManageProductOps _manageProductOps;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ProductOpsController() { }

        /// <summary>
        /// Used for unit testing
        /// </summary>
        /// <param name="manageProductOps"></param>
        public ProductOpsController(IManageProductOps manageProductOps)
        {
            _manageProductOps = manageProductOps;
            base.Request = new HttpRequestMessage();
            base.Request.SetConfiguration(new HttpConfiguration());
        }

        /// <summary>
        /// Used to initialize the base class before the controller to get the users persona
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            _manageOrganization = new ManageOrganization(_userClaims);
            //When API is NOT called from test class
            _manageProductOps = new ManageProductOps(base._userClaims);
        }

        /// <summary>
        /// Used to get a list of roles
        /// </summary>
        /// <remarks>For now filtering and sorting will be done on the UI side</remarks>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="assetGroup"></param>
        /// <param name="datafilter">A datafilter used to filter the roles.</param>
        /// <param name="upfmId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles for the given company", Type = typeof(ProductRole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(ProductRole), typeof(ProductRole.RoleExample))]
        [Route("products/ops/roles")]
        [Authorize]
        [HttpGet]
        public ListResponse GetOpsRoles(long editorPersonaId, long userPersonaId, [FromUri] RequestParameter datafilter, string assetGroup = "", Guid? upfmId = null)
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
                        _manageProductOps = new ManageProductOps(_userClaims);
                    }
                }
            }
            ListResponse response = _manageProductOps.GetRoles(editorPersonaId, userPersonaId, assetGroup, datafilter);
            return response;
        }

        /// <summary>
        /// Used to get a list of assets
        /// </summary>
        /// <remarks>For now filtering and sorting will be done on the UI side</remarks>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="includeDisabled">Include disabled assets in the result</param>
        /// <param name="datafilter">A datafilter used to filter the roles.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of assets for the given company", Type = typeof(ProductProperty))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(ProductProperty), typeof(ProductProperty.PropertyExample))]
        [Route("products/ops/assets")]
        [Authorize]
        [HttpGet]
        public ListResponse GetOpsAssets(long editorPersonaId, long userPersonaId, [FromUri]RequestParameter datafilter, bool includeDisabled = false)
        {
            ListResponse response = _manageProductOps.GetCompanyAssets(editorPersonaId, userPersonaId, includeDisabled, datafilter);
            return response;
        }

        /// <summary>
        /// Used to get a list of roles 
        /// </summary>
        /// <remarks>A datafilter can be used to filter the roles using name</remarks>
        /// <param name="editorPersonaId"></param>                
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [Route("products/ops/rolescount")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetRolesCount(long editorPersonaId)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
            ListResponse response = _manageProductOps.GetRolesCount(editorPersonaId, string.Empty);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Used to get a list of rights 
        /// </summary>
        /// <remarks></remarks>
        /// <param name="editorPersonaId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of rights for the given company", Type = typeof(object))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [Route("products/ops/rights")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetRights(long editorPersonaId)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
            ListResponse response = _manageProductOps.GetRights(editorPersonaId);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        #region Users
        /// <summary>
        /// Used to create a new account for the given GreenBook user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId">The persona to use to find the login to update</param>
        /// <param name="rolepropList">Used to update the list of roles and properties given to a user</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.Created, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request (when role or property data has invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/ops/user")]
        [Authorize]
        [HttpPost]
        public HttpResponseMessage CreateOpsUser(long editorPersonaId, long userPersonaId, OpsRoleAndPropertyList rolepropList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            if (rolepropList == null)
            {
                rolepropList = new OpsRoleAndPropertyList();
            }

            string result = _manageProductOps.ManageOpsUser(editorPersonaId, userPersonaId, rolepropList.RoleList, rolepropList.PropertyList, out List<AdditionalParameters> additionalParameters);
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.Created);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, result);
        }

        /// <summary>
        /// Used to create a new account for the given GreenBook user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId">The persona to use to find the login to update</param>
        /// <param name="rolepropList">Used to update the list of roles and properties given to a user</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request (when role or property data has invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/ops/user")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateOpsUser(long editorPersonaId, long userPersonaId, OpsRoleAndPropertyList rolepropList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            if (rolepropList == null)
            {
                rolepropList = new OpsRoleAndPropertyList();
            }

            string result = _manageProductOps.ManageOpsUser(editorPersonaId, userPersonaId, rolepropList.RoleList, rolepropList.PropertyList, out List<AdditionalParameters> additionalParameters);
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, result);
        }

        /// <summary>
        /// Used to delete an existing account for the given GreenBook user persona
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId">The persona to use to find the login to delete</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Delete successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request (when a user doesn't exist or information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/ops/user")]
        [Authorize]
        [HttpDelete]
        public HttpResponseMessage DeleteOpsUser(long editorPersonaId, long userPersonaId)
        {
            if (editorPersonaId == 0 || userPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }

            string result = _manageProductOps.EnableUser(editorPersonaId, userPersonaId, isActive: false, deleteUser: true);
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, result);
        }

        /// <summary>
        /// Used to update the status of a given user
        /// </summary>
        /// <param name="editorPersonaId">The persona to use to find the OneSite login to update</param>
        /// <param name="userPersonaId"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/ops/user/status")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateOpsUserStatus(long editorPersonaId, long userPersonaId, bool active)
        {
            if (editorPersonaId == 0 || userPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }

            string result = _manageProductOps.EnableUser(editorPersonaId, userPersonaId, isActive: active, deleteUser: false);
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, result);
        }

        #endregion

        #region User-Status

        /// <summary>
        /// Disable ops user
        /// </summary>
        /// <param name="productUser"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "User Disabled Successfully", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad Request")]
        [Route("products/ops/user/MT/status")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateOpsUserStatus(ProductUser productUser)
        {
            if (!_manageProductOps.ChangeUserStatus(_personaId, productUser.UserName, productUser.UserId.ToString()))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Disabling Ops user failed.");
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Successfully disabled product user.");
        }
        #endregion

        #region Migration API
        /// <summary>
        /// Returns product users for given organization
        /// </summary>
        /// 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List spend management users", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/ops/migration-users")]
        [Authorize] // Todo: Need to implement Resource Scope Based Authorization
        [HttpGet]
        public HttpResponseMessage ListOpsMigrationUsers(long editorPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            ManagePersona managePersona = new ManagePersona();
            var persona = managePersona.GetPersona(editorPersonaId);
            if (persona == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not found.");

            base._userClaims.UserRealPageGuid = persona.RealPageId;
            _manageProductOps = new ManageProductOps(base._userClaims);

            var result = _manageProductOps.GetMigrationUsers(editorPersonaId, datafilter);
            if (!result.IsError)
                return Request.CreateResponse(HttpStatusCode.OK, result);
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden, result);
        }

        /// <summary>
        /// Update migration status of users.
        /// </summary>
        /// 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Mark spend management users to migrated", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/ops/migrate-users")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateUsersMigrationStatus(IList<MigrateUser> migrateUsers)
        {
            return Request.CreateResponse(HttpStatusCode.OK, _manageProductOps.UpdateUsersMigrationStatus(_personaId, migrateUsers));
        }
        #endregion
        /// <summary>
        /// Used to get a list of roles assigned to the right
        /// </summary>
        /// <remarks>A datafilter can be used to filter the roles using rolename, roletype (1=Internal, 0=Custom) or excludeassigned (0/1)</remarks>
        /// <param name="editorPersonaId"></param>        
        /// <param name="rightId"></param> 
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles for a given right", Type = typeof(ProductRole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/ops/right/roles")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetRolesForRight(long editorPersonaId, int rightId)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
            if (rightId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "rightId not supplied.");

            ListResponse response = _manageProductOps.GetRolesForRight(editorPersonaId, rightId);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Used to get a list of rights  for a role
        /// </summary>
        /// <remarks></remarks>
        /// <param name="editorPersonaId"></param> 
        /// <param name="roleId"></param>
        /// <param name="upfmId"></param> 
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of rights for the given company", Type = typeof(object))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [Route("products/ops/role/rights")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetRights(long editorPersonaId, int roleId, Guid? upfmId = null)
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
                        _manageProductOps = new ManageProductOps(_userClaims);
                    }
                }
            }
            if (roleId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "roleId not supplied.");
            ListResponse response = _manageProductOps.GetRightsByRole(editorPersonaId, roleId);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Used to create role with rights
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="input">The persona to use to find the login to update</param>
        /// <param name="roleId">Used to update the list of roles and properties given to a user</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.Created, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request (when role or property data has invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/ops/role/rights")]
        [Authorize]
        [HttpPost]
        public HttpResponseMessage CreateRole(long editorPersonaId, OpsInput input, long roleId)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            if (input == null)
            {
                input = new OpsInput();
            }

            if (input.RoleName.Trim() == string.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "RoleName not supplied.");

            ListResponse response = _manageProductOps.CreateRole(editorPersonaId, input, roleId);

            return Request.CreateResponse(HttpStatusCode.OK, response);


        }

        /// <summary>
        /// Used to update role with rights
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="input">The persona to use to find the login to update</param>
        /// <param name="roleId">Used to update the list of roles and properties given to a user</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.Created, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request (when role or property data has invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/ops/role/rights")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateRole(long editorPersonaId, OpsInput input, long roleId)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
            if (roleId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "roleId not supplied.");

            if (input == null)
            {
                input = new OpsInput();
            }

            if (input.RoleName.Trim() == string.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "RoleName not supplied.");

            ListResponse response = _manageProductOps.CreateRole(editorPersonaId, input, roleId);

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


        #region Get Examples

        /// <summary>
        /// Used to document examples of the Response webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class ResponseExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Response example</returns>
            public object GetExamples()
            {
                HttpResponseMessage example = new HttpResponseMessage(HttpStatusCode.OK);

                return example;
            }
        }
        #endregion
    }
}
