using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Hots;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.ResponseObject;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Controllers
{
    public class HotsUserCloneController : BaseApiController
    {
        private IProductRepository _productRepository;
        private IProductInternalSettingRepository _productInternalSettingRepository;
        private IManagePersona _managePersona;
        private IHOTSCloneUserRepository _hotsCloneUserRepository;
        private IOrganizationRepository _organizationRepository;
        private IManageProfile _manageProfile;
        private IManageProduct _manageProduct;
        private IManageHotsCloneUsers _manageHotsCloneUsers;
        IRepositoryResponse _repositoryResponse;
        IManageOrganization _manageOrganization;
        private HttpMessageHandler _messageHandler;

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public HotsUserCloneController()
        {
            // DONT USE USERCLAIM IN BASE, IT IS NULL AT THIS POINT. MOVE TO Initialize FUNCTION
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="repositoryResponse"></param>
        /// <param name="messageHandler"></param>
        /// <param name="userClaims"></param>
        public HotsUserCloneController(IRepository repository, IRepositoryResponse repositoryResponse, HttpMessageHandler messageHandler, DefaultUserClaim userClaims)
        {
            _repositoryResponse = repositoryResponse;
            _managePersona = new ManagePersona(repository, userClaims, messageHandler);
            ProductRepository productRepository = new ProductRepository(repository, userClaims);
            ProductInternalSettingRepository productInternalSettingRepository = new ProductInternalSettingRepository(repository);

            // ManagePersona managePersona = new ManagePersona(repository, userClaims);
            _manageOrganization = new ManageOrganization(repository, userClaims, messageHandler);
            ManageProfile manageProfile = new ManageProfile(userClaims);
            // _manageProduct = new ManageProduct(productRepository, productInternalSettingRepository, _managePersona, manageBlueBook, managePartyRelationship, _manageOrganization, manageProfile, manageUserRoleRight, userClaims);

            _productRepository = new ProductRepository(repository, userClaims);
            _messageHandler = messageHandler;
            _hotsCloneUserRepository = new HOTSCloneUserRepository(repository);
            //  _manageHotsCloneUsers = new ManageHotsCloneUsers(_productRepository, productInternalSettingRepository, _managePersona, _hotsCloneUserRepository, _manageOrganization, manageProfile, userClaims);
            _userClaims = userClaims;
        }

        /// <summary>
        /// Used to initialize DI classes with userclaim
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            _repositoryResponse = new RepositoryResponse();
            _managePersona = new ManagePersona(_userClaims);
            _manageProduct = new ManageProduct(_userClaims);
            _manageOrganization = new ManageOrganization(_userClaims);
            _manageHotsCloneUsers = new ManageHotsCloneUsers(_userClaims);
            _productRepository = new ProductRepository(_userClaims);
        }

        #endregion

        /// <summary>
        /// Create a user in RealPage Unified platform and assign product(s).
        /// </summary>
        /// <returns>If success then returns real page id for newly created user else error object.</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request when Request object have invalid entries.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error.")]
        [SwaggerResponse(HttpStatusCode.Accepted, Description = "Request has been accepted for further processing.")]
        [Route("userclone")]
        [HttpPost]
        public HttpResponseMessage HOTCloneUsers(CloneUsers cloneUsers)
        {
            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;

            if (cloneUsers == null)
            {
                var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                errorResponse.Errors.Add(new Error
                    { Title = "Error", Source = "/HotsCloneUser", Detail = "Null request received.", StatusCode = "" });

                // return errors with bad request
                return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
            }

            if (currentClaimPrincipal.HasClaim("scope", "usermanagement"))
            {
                if (!string.IsNullOrEmpty(cloneUsers.CloneCustomerUPFMId.ToString()))
                {
                    //IManageOrganization manageOrganization = new ManageOrganization(_userClaims);
                    Guid AdminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(cloneUsers.CloneCustomerUPFMId);
                    //recreate clams
                    if (AdminCreatorRealPageId == Guid.Empty)
                    {
                        var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                        errorResponse.Errors.Add(new Error
                            { Title = "Error", Source = "/HotsCloneUser", Detail = "Invalid UPFMId.", StatusCode = "" });

                        // return errors with bad request
                        return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                    }

                    _userClaims = RecreateClaimsForClient(AdminCreatorRealPageId);
                    _managePersona = new ManagePersona(_userClaims);
                    _manageProduct = new ManageProduct(_userClaims);

                }
                else
                {
                    var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                    errorResponse.Errors.Add(new Error
                        { Title = "Error", Source = "/HotsCloneUser", Detail = "Invalid UPFMId.", StatusCode = "" });

                    // return errors with bad request
                    return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                }
            }
            else
            {
                var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                errorResponse.Errors.Add(new Error
                    { Title = "Error", Source = "/HotsCloneUser", Detail = "Invalid Claim Scope.", StatusCode = "" });

                // return errors with bad request
                return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
            }

            if (cloneUsers.CloneCustomerUPFMId == null || cloneUsers.CloneCustomerUPFMId == Guid.Empty)
            {
                var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                errorResponse.Errors.Add(new Error
                    { Title = "Error", Source = "/HotsCloneUser", Detail = "Invalid Clone Customer UPFMId.", StatusCode = "" });

                // return errors with bad request
                return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
            }

            var baseUpfmId = _manageHotsCloneUsers.GetBaseCompanyUPFMId(cloneUsers.CloneCustomerUPFMId);

            Organization baseOrg = _manageOrganization.GetOrganization(baseUpfmId);

            if (baseOrg == null)
            {
                var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                errorResponse.Errors.Add(new Error
                    { Title = "Error", Source = "/HotsCloneUser", Detail = "Base Line Organization not found.", StatusCode = "" });

                // return errors with bad request
                return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
            }
            Guid baseOrgAdminRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(baseUpfmId);
            var baseOrgAdminPersona = _managePersona.GetActivePersonaWithoutRights(baseOrgAdminRealPageId);
            var baseOrgAdminClaim = RecreateClaimsForClient(baseOrgAdminRealPageId);
            var cloneOrg = _manageOrganization.GetOrganization(cloneUsers.CloneCustomerUPFMId);

            if (cloneOrg == null)
            {
                var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                errorResponse.Errors.Add(new Error
                    { Title = "Error", Source = "/HotsCloneUser", Detail = "Clone Customer Organization not found.", StatusCode = "" });

                // return errors with bad request
                return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
            }

            _manageHotsCloneUsers = new ManageHotsCloneUsers(_userClaims);

            var clonedUserResult = _manageHotsCloneUsers.CloneUsersFromBaseLineCompany(cloneUsers, baseOrg.PartyId, cloneOrg.PartyId, baseOrgAdminClaim, baseOrgAdminPersona.PersonaId);
            return Request.CreateResponse(HttpStatusCode.Accepted, clonedUserResult);
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
                    RealPageEmployee = persona.Organization.Name.ToUpper() == "REALPAGE EMPLOYEE"
                };
            }

            return userClaim;
        }
    }
}