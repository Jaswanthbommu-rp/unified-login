using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.ResponseObject;
using Swashbuckle.Swagger.Annotations;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    public class UserPropertiesSyncController : BaseApiController
    {
        private IProductRepository _productRepository;
        private IProductInternalSettingRepository _productInternalSettingRepository;       
        private IManageProduct _manageProduct;
        IRepositoryResponse _repositoryResponse;
        IManageOrganization _manageOrganization;
        IUserPropertiesSyncRepository _userPropertiesSyncRepository;
        IManageUserPropertiesSync _manageUserPropertiesSync;
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public UserPropertiesSyncController()
        {
            // DONT USE USERCLAIM IN BASE, IT IS NULL AT THIS POINT. MOVE TO Initialize FUNCTION
        }

        /// <summary>
        /// Used to initialize DI classes with userclaim
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            _repositoryResponse = new RepositoryResponse();            
            _manageProduct = new ManageProduct(_userClaims);
            _manageOrganization = new ManageOrganization(_userClaims);
            _productRepository = new ProductRepository(_userClaims);
            _userPropertiesSyncRepository = new UserPropertiesSyncRepository();
            _manageUserPropertiesSync = new ManageUserPropertiesSync(_userClaims);
        }
        #endregion
        /// <summary>
        /// Translate and Saves the User Product primary properties based on jon task.
        /// </summary>
        /// <returns>If success then returns ok.</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request when Request object have invalid entries.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error.")]
        [SwaggerResponse(HttpStatusCode.Accepted, Description = "Request has been accepted for further processing.")]
        [Route("userpropertiessync")]
        [HttpPost]
        public HttpResponseMessage UserPropertiesSync(UserSyncJobTask userSyncJobTask)
        {
            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            //UserSyncData userSyncData = new UserSyncData();
            if (userSyncJobTask == null)
            {
                var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                errorResponse.Errors.Add(new Error
                { Title = "Error", Source = "/UserPropertiesSync", Detail = "invalid User Sync Job Task Object.", StatusCode = "" });

                // return errors with bad request
                return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
            }

            if (currentClaimPrincipal.HasClaim("scope", "internalapi"))
            {
                //userSyncData = _userPropertiesSyncRepository.GetUserSyncData(userSyncJobTaskId);
                if (!string.IsNullOrEmpty(userSyncJobTask.UserOrgRealpageId.ToString()))
                {
                    //IManageOrganization manageOrganization = new ManageOrganization(_userClaims);
                    Guid adminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(userSyncJobTask.UserOrgRealpageId);
                    //recreate clams
                    if (adminCreatorRealPageId == Guid.Empty)
                    {
                        var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                        errorResponse.Errors.Add(new Error
                        { Title = "Error", Source = "/UserPropertiesSync", Detail = "Invalid UPFMId.", StatusCode = "" });

                        // return errors with bad request
                        return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                    }

                    _userClaims = RecreateClaimsForClient(adminCreatorRealPageId);
                    _manageProduct = new ManageProduct(_userClaims);

                }
                else
                {
                    var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                    errorResponse.Errors.Add(new Error
                    { Title = "Error", Source = "/UserPropertiesSync", Detail = "Invalid UPFMId.", StatusCode = "" });

                    // return errors with bad request
                    return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                }
            }
            else
            {
                var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                errorResponse.Errors.Add(new Error
                { Title = "Error", Source = "/UserPropertiesSync", Detail = "Invalid Claim Scope.", StatusCode = "" });

                // return errors with bad request
                return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
            }
            _manageUserPropertiesSync = new ManageUserPropertiesSync(_userClaims);
            var response = _manageUserPropertiesSync.TranslateAndSaveUserProductProperties(userSyncJobTask);

            return Request.CreateResponse(HttpStatusCode.Accepted, response);
        }

        private DefaultUserClaim RecreateClaimsForClient(Guid realpageUserId)
        {
            DefaultUserClaim userClaim = new DefaultUserClaim();

            if (!string.IsNullOrEmpty(realpageUserId.ToString()))
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

                userClaim = new DefaultUserClaim
                {
                    UserId = (int)userLogin.UserId,
                    OrganizationPartyId = persona.Organization.PartyId,
                    LoginName = userLogin.LoginName,
                    OrganizationMasterId = (long)persona.Organization.BooksMasterId,
                    CustomerMasterId = (long)persona.Organization.BooksMasterId,
                    OrganizationName = persona.Organization.Name.ToString(),
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    PersonaId = persona.PersonaId,
                    OrganizationRealPageGuid = persona.Organization.RealPageId,
                    UserRealPageGuid = realpageUserId,
                    CorrelationId = Guid.NewGuid(),
                    RealPageEmployee = (persona.Organization.RealPageId == EmployeeCompanyRealPageId)
                };
            }

            return userClaim;
        }
    }
}