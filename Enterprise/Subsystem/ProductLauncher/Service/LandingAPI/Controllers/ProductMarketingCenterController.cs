using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.MarketingCenter;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.ResponseObject;
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

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    /// <summary>
    /// Used to manage Marketing Center users
    /// </summary>
    public class ProductMarketingCenterController : BaseApiController
    {
        private IRepository _repository;
        private IManageOrganization _manageOrganization;
        private IManageProductMarketingCenter _manageProductMarketingCenter;
        public ProductMarketingCenterController()
        {
            // DONT USE USERCLAIM IN BASE, IT IS NULL AT THIS POINT. MOVE TO Initialize FUNCTION
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="messageHandler"></param>
        /// <param name="userClaims"></param>
        public ProductMarketingCenterController(IRepository repository, DefaultUserClaim userClaims, HttpMessageHandler messageHandler, Guid editorRealPageId)
        {
            _repository = repository;
            _userClaims = userClaims;
            _manageOrganization = new ManageOrganization(repository, userClaims, messageHandler);
            var productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            var manageBlueBook = new ManageBlueBook(userClaims, repository, productInternalSettingRepository, messageHandler);
            var managePersona = new ManagePersona(repository, userClaims, messageHandler);
            var samlRepository = new SamlRepository(repository);
            var productRepository = new ProductRepository(repository, userClaims);
            _manageProductMarketingCenter = new ManageProductMarketingCenter(editorRealPageId, userClaims, messageHandler, productInternalSettingRepository, managePersona, samlRepository, manageBlueBook, productRepository
                                                                             , repository);
        }

        /// <summary>
        /// Used to initialize DI classes with userclaim
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            _manageOrganization = new ManageOrganization(base._userClaims);
            _manageProductMarketingCenter = new ManageProductMarketingCenter(base._userClaims);
        }

        #region User management
        /// <summary>
        /// Used to get a list of roles
        /// </summary>
        /// <remarks>For now filtering and sorting will be done on the UI side</remarks>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter">A datafilter used to filter the roles.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles for the given company", Type = typeof(ProductRole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(ProductRole), typeof(ProductRole.RoleExample))]
        [Route("products/marketingcenter/roles")]
        [Authorize]
        [HttpGet]
        public ListResponse GetMarketingCenterRoles(long editorPersonaId, long userPersonaId, [FromUri]RequestParameter datafilter)
        {
            ManageProductMarketingCenter mg = new ManageProductMarketingCenter(base._userClaims);
            ListResponse response = mg.GetRoles(editorPersonaId, userPersonaId, datafilter);
            return response;
        }

        /// <summary>
        /// Used to get a list of properties
        /// </summary>
        /// <remarks>For now filtering and sorting will be done on the UI side</remarks>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter">A datafilter used to filter the properties. RightDescription or CenterName can be used</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of properties for the given company", Type = typeof(ProductProperty))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(ProductProperty), typeof(ProductProperty.PropertyExample))]
        [Route("products/marketingcenter/properties")]
        [Authorize]
        [HttpGet]
        public ListResponse GetMarketingCenterProperties(long editorPersonaId, long userPersonaId, [FromUri]RequestParameter datafilter)
        {
            ManageProductMarketingCenter mg = new ManageProductMarketingCenter(base._userClaims);
            ListResponse response = mg.GetProperties(editorPersonaId, userPersonaId, datafilter);
            return response;
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
        [SwaggerResponse(HttpStatusCode.Created, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request (when role or property data has invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/marketingcenter/user")]
        [Authorize]
        [HttpPost]
        public HttpResponseMessage CreateMarketingCenterUser(long editorPersonaId, long userPersonaId, MarketingCenterRoleAndPropertyList rolepropList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            if (rolepropList == null)
            {
                rolepropList = new MarketingCenterRoleAndPropertyList();
            }
            ManageProductMarketingCenter mg = new ManageProductMarketingCenter(base._userClaims);

            string result = mg.ManageMarketingCenterUser(editorPersonaId, userPersonaId, rolepropList.RoleList, rolepropList.PropertyList, rolepropList.IsAssignedNewPropertyByDefault, out List<AdditionalParameters> additionalParameters);
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.Created);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, result);
        }

        /// <summary>
        /// Used to update an existing account for the given GreenBook user
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
        [Route("products/marketingcenter/user")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateMarketingCenterUser(long editorPersonaId, long userPersonaId, MarketingCenterRoleAndPropertyList rolepropList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            if (rolepropList == null)
            {
                rolepropList = new MarketingCenterRoleAndPropertyList();
            }
            ManageProductMarketingCenter mg = new ManageProductMarketingCenter(base._userClaims);

            string result = mg.ManageMarketingCenterUser(editorPersonaId, userPersonaId, rolepropList.RoleList, rolepropList.PropertyList, rolepropList.IsAssignedNewPropertyByDefault, out List<AdditionalParameters> additionalParameters);
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, result);
        }

        #endregion

        #region User-Status

        /// <summary>
        /// Disable the resident portal user.
        /// </summary>
        /// <param name="produtUser">The produt user.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "User Deleted Successfully", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad Request")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/marketingcenter/user/MT/status")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateMarketingCenterUserStatus(ProductUser produtUser)
        {
            var manageProductMarketingCenter = new ManageProductMarketingCenter(base._userClaims);
            if (!manageProductMarketingCenter.ChangeUserStatus(_personaId, produtUser.UserName, produtUser.UserId.ToString()))
            {
                if(produtUser.IsAssigned)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Activate MarketingCenter user failed.");
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Deactivate MarketingCenter user failed.");
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Successfully disabled product user.");
        }

        #endregion

        #region Roles Rights Setup
        /// <summary>
        /// Used to get a list of roles 
        /// </summary>
        /// <remarks>A datafilter can be used to filter the roles using name</remarks>
        /// <param name="editorPersonaId"></param>  
        /// <param name="upfmId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [Route("products/marketingcenter/rolescount")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetRolesCount(long editorPersonaId, Guid? upfmId = null)
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
                        _manageProductMarketingCenter = new ManageProductMarketingCenter(_userClaims);
                    }
                }
            }
            ListResponse response = _manageProductMarketingCenter.GetRolesCount(editorPersonaId);
            return Request.CreateResponse(!response.IsError ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response);
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
        [Route("products/marketingcenter/rights")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetRights(long editorPersonaId)
        {
            if (editorPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            ManageProductMarketingCenter mc = new ManageProductMarketingCenter(base._userClaims);
            ListResponse response = mc.GetRights(editorPersonaId);
            return Request.CreateResponse(!response.IsError ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response);
        }

        /// <summary>
        /// Disable the resident portal user.
        /// </summary>
        /// <param name="editorPersonaId">The editorPersonaId.</param>
        /// <param name="roleId">The roleId.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Role Deleted Successfully", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad Request")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/marketingcenter/role")]
        [Authorize]
        [HttpDelete]
        public HttpResponseMessage DeleteMarketingCenterRole(long editorPersonaId, int roleId)
        {
            if (editorPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            ManageProductMarketingCenter mc = new ManageProductMarketingCenter(base._userClaims);
            return Request.CreateResponse(HttpStatusCode.OK, mc.DeleteRole(editorPersonaId, roleId));
        }

        /// <summary>
        /// Disable the MC portal user.
        /// </summary>
        /// <param name="roleId">The roleId.</param>
        /// <param name="isActive">The isActive.</param>
        /// <param name="editorPersonaId">The editorPersonaId.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Role Deleted Successfully", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad Request")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/marketingcenter/role/status")]
        [Authorize]
        [HttpPost]
        public HttpResponseMessage UpdateMarketingCenterRoleStatus(long editorPersonaId, int roleId, bool isActive)
        {
            if (editorPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            ManageProductMarketingCenter mc = new ManageProductMarketingCenter(base._userClaims);
            return Request.CreateResponse(HttpStatusCode.OK, mc.UpdateRoleStatus(editorPersonaId, roleId, isActive));
        }

        /// <summary>
        /// Get roles for the right id
        /// </summary>
        /// <param name="rightId">The rightId.</param>
        /// <param name="editorPersonaId">The editorPersonaId.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Role Deleted Successfully", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad Request")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/marketingcenter/right/roles")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetRolesForRightId(long editorPersonaId, int rightId)
        {
            if (editorPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            ManageProductMarketingCenter mc = new ManageProductMarketingCenter(base._userClaims);
            ListResponse response = mc.GetRolesForRightId(editorPersonaId, rightId);
            return Request.CreateResponse(!response.IsError ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response);
        }

        /// <summary>
        /// Update roles for right
        /// </summary>
        /// <param name="rightId">The rightId.</param>
        /// <param name="roleList">The roleList.</param>
        /// <param name="editorPersonaId">The editorPersonaId.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Role Deleted Successfully", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad Request")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/marketingcenter/right/roles")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateRolesForRight(long editorPersonaId, int rightId, List<string> roleList)
        {
            if (editorPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            ManageProductMarketingCenter mc = new ManageProductMarketingCenter(base._userClaims);
            ListResponse response = mc.UpdateRolesForRight(editorPersonaId, rightId, roleList);
            if (!response.IsError)
                return Request.CreateResponse(HttpStatusCode.OK, "Roles Updated");
            else
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
        }

        /// <summary>
        /// Get Rights for roleId
        /// </summary>
        /// <param name="roleId">The roleId.</param>
        /// <param name="editorPersonaId">The editorPersonaId.</param>
        /// <param name="upfmId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Role Deleted Successfully", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad Request")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/marketingcenter/role/allrights")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetRightsForRoleId(long editorPersonaId, int roleId = 0, Guid? upfmId = null)
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
                        _manageProductMarketingCenter = new ManageProductMarketingCenter(_userClaims);
                    }
                }
            }
            ListResponse response = _manageProductMarketingCenter.GetRightsForRoleId(editorPersonaId, roleId);
            return Request.CreateResponse(!response.IsError ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response);
        }

        /// <summary>
        /// Create New Role with rights, 
        /// Don't pass Id for new role
        /// </summary>
        /// <param name="mcRole">Role Object to save</param>
        /// <param name="editorPersonaId">The editorPersonaId.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Role Created Successfully", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad Request")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/marketingcenter/role")]
        [Authorize]
        [HttpPost]
        public HttpResponseMessage CreateNewMCRoleWithRights(long editorPersonaId, MCRole mcRole)
        {
            if (editorPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            ManageProductMarketingCenter mc = new ManageProductMarketingCenter(base._userClaims);
            ListResponse response = mc.CreateNewMCRoleWithRights(editorPersonaId, mcRole);
            return Request.CreateResponse(Convert.ToString(response.Additional) == "RoleError" ? HttpStatusCode.OK : (!response.IsError ? HttpStatusCode.OK : HttpStatusCode.BadRequest), response);
        }

        /// <summary>
        /// Update Role and rights
        /// </summary>
        /// <param name="mcRole">Role Object to save</param>
        /// <param name="editorPersonaId">The editorPersonaId.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Role Updated Successfully", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad Request")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/marketingcenter/role")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateMCRoleWithRights(long editorPersonaId, MCRole mcRole)
        {
            if (editorPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            ManageProductMarketingCenter mc = new ManageProductMarketingCenter(base._userClaims);
            ListResponse response = mc.UpdateNewMCRoleWithRights(editorPersonaId, mcRole);

            HttpStatusCode responseStatus;
            if (Convert.ToString(response.Additional) == "RoleError")
            {
                responseStatus = HttpStatusCode.OK;
            }
            else
            {
                if (!response.IsError)
                {
                    responseStatus = HttpStatusCode.OK;
                }
                else
                {
                    responseStatus = HttpStatusCode.BadRequest;
                }
            }

            return Request.CreateResponse(responseStatus, response);
        }
        #endregion

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
                    OrganizationMasterId = persona.Organization.BooksMasterId,
                    CustomerMasterId = persona.Organization.BooksMasterId,
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

        #region Migration API
        /// <summary>
        /// Returns product users of an organization for given user.
        /// </summary>
        /// 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List Marketing Center users", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/marketingcenter/migration-users")]
        [Authorize] // Todo: Need to implement Resource Scope Based Authorization
        [HttpGet]
        public HttpResponseMessage ListMarketingCenterMigrationUsers(long editorPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            ManagePersona managePersona = new ManagePersona();
            var persona = managePersona.GetPersona(editorPersonaId);
            if (persona == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not found.");

	        base._userClaims.UserRealPageGuid = persona.RealPageId;
            var manageProductMarketingCenter = new ManageProductMarketingCenter(base._userClaims);

            var result = manageProductMarketingCenter.GetMigrationUsers(editorPersonaId, datafilter);
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
        [SwaggerResponse(HttpStatusCode.OK, Description = "Mark Marketing Center users to migrated", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/marketingcenter/migrate-users")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateUsersMigrationStatus(IList<MigrateUser> migrateUsers)
        {
			var manageProductMarketingCenter = new ManageProductMarketingCenter(base._userClaims);
            return Request.CreateResponse(HttpStatusCode.OK, manageProductMarketingCenter.UpdateUsersMigrationStatus(_personaId, migrateUsers));
        }
        #endregion

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
