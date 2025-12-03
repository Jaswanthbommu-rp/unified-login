using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Enterprise.User;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Enterprise.User.Models;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Security;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Security;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.Security;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSite;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.ResponseObject;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Dto;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Helpers;
using Serilog;
using Serilog.Events;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;
using ProductRole = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ProductRole;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Controllers
{
    /// <summary>
    /// User Controller
    /// </summary>
    public class UserController : BaseApiController
    {
        #region Private variables

        private IRepository _repository;
        private IOneSiteProductService _oneSiteProductService;
        private HttpMessageHandler _messageHandler;

        //private IRepositoryResponse _repositoryResponse;
        private IManagePersona _managePersona;
        private IManagePerson _personLogic;
        private IManageProduct _manageProduct;
        private IManageOrganization _manageOrganization;
        private IManageUnifiedSettings _manageSettings;
        private IProductRepository _productRepository;
        private IUserRepository _userRepository;
        private IManageSecurity _manangeSecurityLogic;
        private IManageProductPanel _manageProductPanel;
        private IIntegrationTypeFactory _integrationTypeFactory;
        private UserManagement _userManagement;
        private ManageUser _manageUser;
        private IManageUserLogin _userLoginLogic;
        private IManageProductUser _manageProductUser;
        private SamlRepository _samlRepository;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public UserController()
        {
            // DONT USE USERCLAIM IN BASE, IT IS NULL AT THIS POINT. MOVE TO Initialize FUNCTION
        }

        /// <summary>
        /// Unit test constructor v2
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="messageHandler"></param>
        /// <param name="userClaims"></param>
        /// <param name="oneSiteProductService"></param>
        public UserController(IRepository repository, HttpMessageHandler messageHandler, DefaultUserClaim userClaims, IOneSiteProductService oneSiteProductService)
        {
            _repository = repository;
            _messageHandler = messageHandler;
            _oneSiteProductService = oneSiteProductService;

            var productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            var manageBlueBook = new ManageBlueBook(userClaims, repository, productInternalSettingRepository, messageHandler);
            var personaRightRepository = new PersonaRightRepository(repository);
            var manageUnifiedLogin = new ManageUnifiedLogin(repository, userClaims, messageHandler);
            var manageProductOneSite = new ManageProductOneSite(repository, userClaims, messageHandler, oneSiteProductService);

            _managePersona = new ManagePersona(repository, userClaims, messageHandler);
            _personLogic = new ManagePerson(repository);
            _manageOrganization = new ManageOrganization(repository, userClaims, messageHandler);
            _manageSettings = new ManageUnifiedSettings(repository, userClaims, messageHandler);
            _manageProduct = new ManageProduct(repository, userClaims, messageHandler);
            _manageProductPanel = new ManageProductPanel(userClaims, repository, manageBlueBook, messageHandler, manageProductOneSite);
            _productRepository = new ProductRepository(repository, userClaims);
            _userClaims = userClaims;
            _userRepository = new UserRepository(repository, userClaims, messageHandler);
            _manangeSecurityLogic = new ManageSecurity(userClaims, personaRightRepository);
            _integrationTypeFactory = new IntegrationTypeFactory(_manageProduct, manageUnifiedLogin, manageProductOneSite, _productRepository, productInternalSettingRepository, _userClaims);
            _userManagement = new UserManagement(userClaims);
            _manageUser = new ManageUser(repository, userClaims, messageHandler);
            _userLoginLogic = new ManageUserLogin(repository, userClaims, messageHandler);
            
            _manageProductUser = new ManageProductUser(repository, userClaims, messageHandler, oneSiteProductService);
            _samlRepository = new SamlRepository(repository);
        }

        /// <summary>
        /// Used to initialize DI classes with userclaim
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            var manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
            var manageProductOneSite = new ManageProductOneSite(_userClaims);
            var productInternalSettingRepository = new ProductInternalSettingRepository();

            _managePersona = new ManagePersona(_userClaims);
            _personLogic = new ManagePerson();
            _manageProduct = new ManageProduct(_userClaims);
            _manageProductPanel = new ManageProductPanel(_userClaims);
            _manageOrganization = new ManageOrganization(_userClaims);
            _manageSettings = new ManageUnifiedSettings(_userClaims);
            _productRepository = new ProductRepository(_userClaims);
            _userRepository = new UserRepository(_userClaims);
            _manangeSecurityLogic = new ManageSecurity(_userClaims);
            _integrationTypeFactory = new IntegrationTypeFactory(_manageProduct, manageUnifiedLogin, manageProductOneSite, _productRepository, productInternalSettingRepository, _userClaims);
            _userManagement = new UserManagement(_userClaims);
            _manageUser = new ManageUser(_userClaims);
            _userLoginLogic = new ManageUserLogin(_userClaims);
            _manageProductUser = new ManageProductUser(_userClaims);
            _samlRepository = new SamlRepository();
        }

        #endregion

        /// <summary>
        /// Create a user in RealPage Unified platform and assign product(s).
        /// </summary>
        /// <returns>If success then returns real page id for newly created user else error object.</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request when Request object have invalid entries.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error.", Type = typeof(UserProductDetailsDto))]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Create a user in RealPage Unified platform and allocate product(s).", Type = typeof(UserProductDetailsDto))]
        [Route("user")]
        [HttpPost]
        public HttpResponseMessage CreateUser(UserProductDetailsDto userProductDetailsDto, Guid? upfmId = null)
        {
            try
            {
                var errorResponse = new ErrorResponse { Errors = new List<Error>() };

                var clientCredentialLogin = AttemptClientCredentialAuthentication(upfmId);
                if (clientCredentialLogin != null && clientCredentialLogin.Errors.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, clientCredentialLogin.Errors);
                }

                if (userProductDetailsDto.UserProfileDetails.UserType == UserTypeDto.SuperUser && _userClaims.OrganizationRealPageGuid != Guid.Empty)
                {
                    var data = _manageSettings.GetCompanyInternalSettings(_userClaims.OrganizationRealPageGuid, "UPFM", "SMB");
                    var isEnableSmb = data?.Keys?.FirstOrDefault(k => k.Name.Equals("enablesmb", System.StringComparison.OrdinalIgnoreCase));
                    if (isEnableSmb != null && isEnableSmb.Value == "1")
                    {
                        var superUserCount = _userRepository.GetSuperUserCountByOrganizationAsync(_userClaims.OrganizationPartyId);
                        if (superUserCount >= 2)
                        {
                            errorResponse.Errors.Add(new Error { Title = CommonMessageConstants.ErrorTitle, Source = CommonMessageConstants.ErrorSource, Detail = "You have reached the maximum number of System Administrators (2). Please update the User Relationship of the user.", StatusCode = "" });
                            return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                        }
                    }
                }

                if (userProductDetailsDto == null)
                {
                    errorResponse.Errors.Add(new Error { Title = "Error", Source = "/user", Detail = "Null request received.", StatusCode = "" });
                }

                // request object validation
                var errorList = DtoValidator.ValidateObject(userProductDetailsDto.UserProfileDetails).ToList();

                // check product-code for each product
                if (userProductDetailsDto.ProductList != null)
                {
                    foreach (var productDetailDto in userProductDetailsDto.ProductList)
                    {
                        errorList.AddRange(DtoValidator.ValidateObject(productDetailDto).ToList());
                    }
                }

                if (_userClaims.OrganizationRealPageGuid == DefaultUserClaim.ExternalCompanyRealPageId)
                {
                    errorList.Add(new ValidationResult("Cannot create new user in External User company."));
                }

                // loginName & email check
                if (!string.IsNullOrEmpty(userProductDetailsDto.UserProfileDetails.LoginName))
                {
                    if (userProductDetailsDto.UserProfileDetails.UserType == UserTypeDto.Regular)
                    {
                        // Check email & loginName are same for regular user
                        if (string.IsNullOrEmpty(userProductDetailsDto.UserProfileDetails.Email))
                        {
                            errorList.Add(new ValidationResult("Email is required for Regular user type."));
                        }
                        else if (!userProductDetailsDto.UserProfileDetails.Email.Equals(userProductDetailsDto.UserProfileDetails.LoginName, StringComparison.OrdinalIgnoreCase))
                        {
                            errorList.Add(new ValidationResult("Email and loginName should be same for Regular user type."));
                        }
                    }
                }

                // send validation error back
                if (errorList.Any())
                {
                    foreach (var item in errorList)
                    {
                        errorResponse.Errors.Add(new Error { Title = "Validation Error", Source = "/user", Detail = item.ToString(), StatusCode = "" });
                    }
                }

                // map dto to business object
                var userProductDetails = GetUserBusinessObject(userProductDetailsDto);

                // date validations
                if (userProductDetailsDto.UserProfileDetails.UserEffectiveDate.HasValue && userProductDetailsDto.UserProfileDetails.UserExpirationDate.HasValue)
                {
                    if (userProductDetailsDto.UserProfileDetails.UserExpirationDate.Value < userProductDetailsDto.UserProfileDetails.UserEffectiveDate.Value)
                    {
                        errorResponse.Errors.Add(new Error { Title = "Error", Source = "/user", Detail = "UserExpirationDate should be greater than UserEffectiveDate.", StatusCode = "" });
                    }
                }

                // assign default dates if not supplied
                userProductDetailsDto.UserProfileDetails.UserEffectiveDate = userProductDetailsDto.UserProfileDetails.UserEffectiveDate ?? DateTime.UtcNow;
                userProductDetailsDto.UserProfileDetails.UserExpirationDate = userProductDetailsDto.UserProfileDetails.UserExpirationDate ?? new DateTime(9999, 12, 31);

                //Validate Product data
                var productData = _userManagement.ValidateProductData(userProductDetails.ProductList);
                if (productData.Count > 0)
                {
                    foreach (var item in productData)
                        errorResponse.Errors.Add(new Error { Title = "Error", Source = "/user", Detail = item, StatusCode = "" });
                }

                //Check if User type is noemail and no password
                if(GetGbUserType(userProductDetailsDto.UserProfileDetails.UserType) == UserTypeConstants.RegularUserNoEmail && string.IsNullOrEmpty(userProductDetailsDto.UserProfileDetails.Password))
                {
                    errorResponse.Errors.Add(new Error { Title = "Error", Source = "/user", Detail = "Password field is required for User type with NoEmail.", StatusCode = "" });
                }

                //Check if user is available in other company
                IManageRoleType roleTypeLogic = new ManageRoleType(new RoleTypeRepository());
                var roleTypeList = (List<Component.SharedObjects.IdentityConfig.RoleType>)roleTypeLogic.GetRoleType(roleTypeName: "user role", partyId: null, orgMasterId: null, loginName: userProductDetailsDto.UserProfileDetails.LoginName);
                var userTypeId = GetGbUserType(userProductDetailsDto.UserProfileDetails.UserType);
                if (!roleTypeList.Any(x => x.PartyRoleTypeId == userTypeId))
                {
                    errorResponse.Errors.Add(new Error { Title = "Error", Source = "/user", Detail = "User type with Regular already exists. Please create user type as an External.", StatusCode = "" });
                }

                // custom fields
                IList<CustomFieldValue> userCustomFields = null;
                var userCustomFieldValueJson = string.Empty;
                string customFieldsData = _userManagement.ValidateAndAssignCustomFieldValues(null, userProductDetailsDto.UserProfileDetails.CustomFields, out userCustomFields);
                if (!string.IsNullOrEmpty(customFieldsData))
                {
                    errorResponse.Errors.Add(new Error { Title = "Error", Source = "/user", Detail = customFieldsData, StatusCode = "" });
                }

                List<Persona> userPersona = new List<Persona>();
                ProfileDetail profile = BuildProfileByInput(userProductDetailsDto, userCustomFields);

                //Default Persona
                IManagePersona managePersona = new ManagePersona(_userClaims);
                IList<PersonaEnvironment> personaEnvironment = managePersona.GetPersonaEnvironmentType();
                var personaEnv = personaEnvironment.SingleOrDefault<PersonaEnvironment>(p => p.Name == "Production");
                if (personaEnv == null)
                {
                    errorResponse.Errors.Add(new Error { Title = "Error", Source = "/user", Detail = "Persona environment is missing.", StatusCode = "" });
                }
                else
                {
                    Persona persona = new Persona();
                    persona.Name = profile.UserTypeId == (int)UserRoleType.SuperUser ? "System Administrator" : "Primary";
                    persona.PersonaEnvironmentTypeId = (int)personaEnv.PersonaEnvironmentTypeId;
                    persona.FromDate = DateTime.UtcNow;
                    persona.ThruDate = null;
                    userPersona.Add(persona);
                    profile.Persona = userPersona;
                }

                if (profile.organization.Count == 0)
                {
                    //Active Persona is linked to one organization
                    var persona = managePersona.GetFirstAvailablePersonaByCompany(_userClaims.UserRealPageGuid, _userClaims.OrganizationPartyId);
                    profile.organization.Add(persona.Organization);
                }

                if (errorResponse.Errors.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                }

                bool IsValidDomainUsername = _userLoginLogic.IsUserEmailDomainValid(profile.userLogin.LoginName);
                if (!IsValidDomainUsername)
                {
                    errorResponse.Errors.Add(new Error { Title = CommonMessageConstants.ErrorTitle, Source = CommonMessageConstants.ErrorSource, Detail = "Email domain is not allowed.", StatusCode = "" });
                    return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                }

                var manageUser = new ManageUser(_userClaims);
                var response = manageUser.CreateUser(profile, userPersona, true);

                // check response has error
                if (!response.Status.Success)
                {
                    //var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                    errorResponse.Errors.Add(new Error
                    { Title = "Error", Source = "/user", Detail = response?.Status?.ErrorMsg + " \n " + response?.Status?.ErrorData, StatusCode = "" });

                    // return errors with bad request
                    return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                }
                var objectResponse = new ObjectResponse { Data = response.UserRealPageGuid, IsError = false, ErrorReason = null };

                // everything good send newly created user real page id
                return Request.CreateResponse(HttpStatusCode.Created, objectResponse);
            }
            catch (Exception ex)
            {
                var corrId = Guid.NewGuid();
                if (_userClaims.CorrelationId == Guid.Empty)
                {
                    _userClaims.CorrelationId = corrId;
                }

                // elastic logging
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", exception: ex, messageProperties: new object[] { "CreateUser", $"Error while creating new user. BooksMasterOrganizationId{_userClaims.OrganizationName}, new user login name {userProductDetailsDto?.UserProfileDetails.LoginName}" });

                // return 500
                return Request.CreateResponse(HttpStatusCode.InternalServerError, $"Internal system error. Please contact RealPage support with correlation Id - {_userClaims.CorrelationId}");
            }
        }
        
        /// <summary>
        /// Update the user in RealPage Unified platform and if product(s) are provided .
        /// </summary>
        /// <returns>If success then returns updated user else error object.</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request when Request object have invalid entries.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error.", Type = typeof(UserProductDetailsDto))]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update the user in RealPage Unified platform and of product(s) are provided.", Type = typeof(UserProductDetailsDto))]
        [Route("user")]
        [HttpPut]
        public HttpResponseMessage UpdateUser(UserProductDetailsDto userProductDetailsDto, Guid? upfmId = null)
        {
            try
            {
                var errorResponse = new ErrorResponse { Errors = new List<Error>() };

                var clientCredentialLogin = AttemptClientCredentialAuthentication(upfmId);
                if (clientCredentialLogin != null && clientCredentialLogin.Errors.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, clientCredentialLogin.Errors);
                }
                
                if (userProductDetailsDto == null)
                {
                    errorResponse.Errors.Add(new Error { Title = "Error", Source = "/user", Detail = "Null request received.", StatusCode = "" });
                }

                // validate realpage guid supplied
                if (userProductDetailsDto.UserProfileDetails.UnityRealPageUserId == Guid.Empty)
                {
                    errorResponse.Errors.Add(new Error { Title = "Error", Source = "/user", Detail = "UnityRealPageUserId not supplied.", StatusCode = "" });
                }

                // request object validation
                var errorList = DtoValidator.ValidateObject(userProductDetailsDto.UserProfileDetails).ToList();

                // check product-code for each product
                if (userProductDetailsDto.ProductList != null)
                {
                    foreach (var productDetailDto in userProductDetailsDto.ProductList)
                    {
                        errorList.AddRange(DtoValidator.ValidateObject(productDetailDto).ToList());
                    }
                }

                if (_userClaims.OrganizationRealPageGuid == DefaultUserClaim.ExternalCompanyRealPageId)
                {
                    errorList.Add(new ValidationResult("Cannot create new user in External User company."));
                }

                // loginName & email check
                if (!string.IsNullOrEmpty(userProductDetailsDto.UserProfileDetails.LoginName))
                {
                    if (userProductDetailsDto.UserProfileDetails.UserType == UserTypeDto.Regular)
                    {
                        // Check email & loginName are same for regular user
                        if (string.IsNullOrEmpty(userProductDetailsDto.UserProfileDetails.Email))
                        {
                            errorList.Add(new ValidationResult("Email is required for Regular user type."));
                        }
                        else if (!userProductDetailsDto.UserProfileDetails.Email.Equals(userProductDetailsDto.UserProfileDetails.LoginName, StringComparison.OrdinalIgnoreCase))
                        {
                            errorList.Add(new ValidationResult("Email and loginName should be same for Regular user type."));
                        }
                    }
                }

                // send validation error back
                if (errorList.Any())
                {
                    // iterate through all errors
                    foreach (var item in errorList)
                    {
                        errorResponse.Errors.Add(new Error { Title = "Validation Error", Source = "/user", Detail = item.ToString(), StatusCode = "" });
                    }
                }

                // date validations
                if (userProductDetailsDto.UserProfileDetails.UserEffectiveDate.HasValue && userProductDetailsDto.UserProfileDetails.UserExpirationDate.HasValue)
                {
                    if (userProductDetailsDto.UserProfileDetails.UserExpirationDate.Value < userProductDetailsDto.UserProfileDetails.UserEffectiveDate.Value)
                    {
                        errorResponse.Errors.Add(new Error { Title = "Error", Source = "/user", Detail = "UserExpirationDate should be greater than UserEffectiveDate.", StatusCode = "" });
                    }
                }

                // map dto to business object
                var userProductDetails = GetUserBusinessObject(userProductDetailsDto);

                // date validations
                if (userProductDetailsDto.UserProfileDetails.UserEffectiveDate.HasValue && userProductDetailsDto.UserProfileDetails.UserExpirationDate.HasValue)
                {
                    if (userProductDetailsDto.UserProfileDetails.UserExpirationDate.Value < userProductDetailsDto.UserProfileDetails.UserEffectiveDate.Value)
                    {
                        errorResponse.Errors.Add(new Error { Title = "Error", Source = "/user", Detail = "UserExpirationDate should be greater than UserEffectiveDate.", StatusCode = "" });
                    }
                }

                //Validate Product data
                var productData = _userManagement.ValidateProductData(userProductDetails.ProductList);
                if (productData.Count > 0)
                {
                    foreach (var item in productData)
                        errorResponse.Errors.Add(new Error { Title = "Error", Source = "/user", Detail = item, StatusCode = "" });
                }

                // custom fields
                IList<CustomFieldValue> userCustomFields = null;
                var userCustomFieldValueJson = string.Empty;
                string customFieldsData = _userManagement.ValidateAndAssignCustomFieldValues(null, userProductDetailsDto.UserProfileDetails.CustomFields, out userCustomFields);
                if (!string.IsNullOrEmpty(customFieldsData))
                {
                    errorResponse.Errors.Add(new Error { Title = "Error", Source = "/user", Detail = customFieldsData, StatusCode = "" });
                }

                if (errorResponse.Errors.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                }

                List<Persona> userPersona = new List<Persona>();
                ProfileDetail profile = BuildProfileByInput(userProductDetailsDto, userCustomFields);
                UserDetails userDetails = _userRepository.GetUserDetails(_userClaims.PersonaId, null);

                if (userProductDetailsDto.UserProfileDetails.UserType == UserTypeDto.SuperUser && _userClaims.OrganizationRealPageGuid != Guid.Empty)
                {
                    var data = _manageSettings.GetCompanyInternalSettings(_userClaims.OrganizationRealPageGuid, "UPFM", "SMB");
                    var isEnableSmb = data?.Keys?.FirstOrDefault(k => k.Name.Equals("enablesmb", System.StringComparison.OrdinalIgnoreCase));
                    UserDetails existingUserDetails = _userRepository.GetUserDetails(null, userProductDetailsDto.UserProfileDetails.UnityRealPageUserId.ToString());
                    bool isUserTypeChangedToSuperUser = existingUserDetails != null && existingUserDetails.UserRoleTypeId != (int)UserRoleType.SuperUser && profile.UserTypeId == (int)UserRoleType.SuperUser;

                    if (isEnableSmb != null && isEnableSmb.Value == "1" && isUserTypeChangedToSuperUser)
                    {
                        var superUserCount = _userRepository.GetSuperUserCountByOrganizationAsync(_userClaims.OrganizationPartyId);
                        if (superUserCount >= 2)
                        {
                            errorResponse.Errors.Add(new Error { Title = "Error", Source = "/user", Detail = "You have reached the maximum number of System Administrators (2). Please update the User Relationship of the user.", StatusCode = "" });
                            return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                        }
                    }
                }

                bool IsValidDomainUsername = _userLoginLogic.IsUserEmailDomainValid(profile.userLogin.LoginName);
                if (!IsValidDomainUsername)
                {
                    errorResponse.Errors.Add(new Error { Title = "Error", Source = "/user", Detail = "Email domain is not allowed.", StatusCode = "" });
                    return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                }

                var manageUser = new ManageUser(_userClaims);
                var response = manageUser.UpdateUser(userDetails.UserRealPageId, profile);

                // check response has error
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    errorResponse.Errors.Add(new Error { Title = "Error", Source = "/user", Detail = response.ErrorMessage, StatusCode = "" });
                    return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                }

                var objectResponse = new ObjectResponse { Data = response.RealPageId, IsError = false, ErrorReason = null };
                // everything good send updated user realpageid
                return Request.CreateResponse(HttpStatusCode.OK, objectResponse);
            }
            catch (Exception ex)
            {
                var corrId = Guid.NewGuid();
                if (_userClaims.CorrelationId == Guid.Empty)
                {
                    _userClaims.CorrelationId = corrId;
                }

                // elastic logging
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateUser", $"Error while updating user. BooksMasterOrganizationId{_userClaims.OrganizationName}, update user login name {userProductDetailsDto?.UserProfileDetails.LoginName}" });

                // return 500
                return Request.CreateResponse(HttpStatusCode.InternalServerError, $"Internal system error. Please contact RealPage support with correlation Id - {_userClaims.CorrelationId}");
            }
        }

        /// <summary>
        /// Activate or deactivate a user.
        /// </summary>
        /// <param name="unityRealPageUserId">User unique identifier</param>
        /// <param name="userStatusToChange">User status to change - Activate or Deactivate.</param>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Activate or deactivate user status.")]
        [Route("user/status/{unityRealPageUserId}")]
        [HttpPost]
        public HttpResponseMessage CreateUpdateUserStatus(Guid unityRealPageUserId, EntApiUserStatus userStatusToChange)
        {
            try
            {
                UserUiStatusType statusTypeName;

                switch (userStatusToChange)
                {
                    case EntApiUserStatus.Activate:
                        statusTypeName = UserUiStatusType.Active;
                        break;
                    case EntApiUserStatus.Deactivate:
                        statusTypeName = UserUiStatusType.Disabled;
                        break;
                    default:
                        {
                            var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                            errorResponse.Errors.Add(new Error
                            { Title = "Error", Source = "/user", Detail = "Invalid parameter: Incorrect userStatusToChange supplied.", StatusCode = "" });

                            // return errors with bad request
                            return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                        }
                }

                if (unityRealPageUserId == _realpageUserId)
                {
                    var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                    errorResponse.Errors.Add(new Error
                    { Title = "Error", Source = "/user", Detail = "Invalid parameter: Cannot update API logged-in user's status.", StatusCode = "" });

                    // return errors with bad request
                    return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                }

                if (unityRealPageUserId == Guid.Empty)
                {
                    var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                    errorResponse.Errors.Add(new Error
                    { Title = "Error", Source = "/user", Detail = "Invalid parameter: unityRealPageUserId.", StatusCode = "" });

                    // return errors with bad request
                    return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                }

                // Call Logic
                var userManagement = new UserManagement(_userClaims);
                var response = userManagement.ActivateDeactivateUser(unityRealPageUserId, statusTypeName);

                // check response has error
                if (!string.IsNullOrEmpty(response.ErrorReason))
                {
                    var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                    errorResponse.Errors.Add(new Error
                    { Title = "Error", Source = "/user", Detail = response.ErrorReason, StatusCode = "" });

                    // return errors with bad request
                    return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                }

                var objectResponse = new ObjectResponse { Data = response.Data };

                // everything good send newly created user real page id
                return Request.CreateResponse(HttpStatusCode.OK, objectResponse);
            }
            catch (Exception ex)
            {
                var corrId = Guid.NewGuid();
                if (_userClaims.CorrelationId == Guid.Empty)
                {
                    _userClaims.CorrelationId = corrId;
                }

                // elastic logging
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", exception: ex, messageProperties: new object[] { "CreateUpdateUserStatus", $"Error while changing user status. BooksMasterOrganizationId{_userClaims.OrganizationName}, update user RealPage id {unityRealPageUserId}" });

                // return 500
                return Request.CreateResponse(HttpStatusCode.InternalServerError, $"Internal system error. Please contact RealPage support with correlation Id - {_userClaims.CorrelationId}");
            }
        }

        /// <summary>
        /// Get a list of users
        /// </summary>
        /// <param name="unityRealPageUserId">Optional User EnterpriseId</param>
        /// <param name="name">Optional filter by FirstName, LastName, or UserName</param>
        /// <param name="rowsPerPage">Optional Rows Per page to return</param>
        /// <param name="pageNumber">Optional PageNumber</param>
        /// <param name="userStatus">User Status (Active, Disabled, Expired, Pending, Locked). If the provided status is invalid or empty, all the users will be returned</param>
        /// <returns>A list of User(s)</returns>
        /// <remarks>User Status: 
        /// <para>Active, </para>
        /// <para>Disabled, </para>
        /// <para>Expired, </para>
        /// <para>and Pending.</para>
        /// </remarks>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Not Found")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of User(s)", Type = typeof(UsersDataDto))]
        [SwaggerResponseExamples(typeof(UsersDataDto), typeof(EnterpriseGetUserExample))]
        [Route("user")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public HttpResponseMessage GetUser(Guid? upfmId = null, Guid? unityRealPageUserId = null, string name = null, int rowsPerPage = 1, int pageNumber = 1, string userStatus = null)
        {
            var response = new PagedResponse() { Meta = new Meta() };
            IList<UsersDataDto> usersDataDtoList = new List<UsersDataDto>();
            int statusTypeId = 0;
            if (!string.IsNullOrEmpty(userStatus) && Enum.TryParse<UserUiStatusType>(userStatus, true, out UserUiStatusType parsedStatus))
            {
                statusTypeId = (int)parsedStatus;
            }
            var clientCredentialLogin = AttemptClientCredentialAuthentication(upfmId);
            if (clientCredentialLogin != null && clientCredentialLogin.Errors.Count > 0)
            {
                response.Data = usersDataDtoList.Cast<object>().ToList();
                response.Meta.CurrentPage = pageNumber;
                response.Meta.TotalRows = 0;
                response.Meta.RowsPerPage = rowsPerPage;
                response.IsError = true;
                response.ErrorReason = "Invalid UPFMId.";
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }

            if (rowsPerPage <= 0)
            {
                response.Data = usersDataDtoList.Cast<object>().ToList();
                response.Meta.CurrentPage = pageNumber;
                response.Meta.TotalRows = 0;
                response.Meta.RowsPerPage = rowsPerPage;
                response.IsError = true;
                response.ErrorReason = "rowsPerPage must be 1 or greater.";
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }

            if (pageNumber <= 0)
            {
                response.Data = usersDataDtoList.Cast<object>().ToList();
                response.Meta.CurrentPage = pageNumber;
                response.Meta.TotalRows = 0;
                response.Meta.RowsPerPage = rowsPerPage;
                response.IsError = true;
                response.ErrorReason = "pageNumber must be 1 or greater.";
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }

            try
            {
                UserManagement userManagement = new UserManagement(_userClaims);
                IList<UsersData> usersDataList = userManagement.ListUser(_userClaims.OrganizationPartyId, statusTypeId, unityRealPageUserId, name, rowsPerPage, pageNumber);

                if (usersDataList != null && usersDataList.Any())
                {
                    usersDataList.ToList().ForEach(u =>
                    {
                        Dictionary<String, String> dictionaryCustomFields = new Dictionary<String, String>();
                        if (!string.IsNullOrWhiteSpace(u.CustomFields))
                        {
                            IList<CustomFieldValue> CustomFieldValueList = JsonConvert.DeserializeObject<IList<CustomFieldValue>>(u.CustomFields);

                            CustomFieldValueList.ToList().ForEach(c =>
                            {
                                dictionaryCustomFields.Add(c.Name, c.Value);
                            });
                        }

                        usersDataDtoList.Add(
                            new UsersDataDto()
                            {
                                FirstName = u.FirstName,
                                MiddleName = u.MiddleName,
                                LastName = u.LastName,
                                UnityRealPageUserId = u.UserRealPageId,
                                LoginName = u.LoginName,
                                UserEffectiveDate = u.UserEffectiveDate,
                                UserExpirationDate = u.UserExpirationDate,
                                UserStatus = u.Status,
                                Email = u.Email,
                                CustomFields = dictionaryCustomFields,
                                UserType = u.UserType,
                                IsExternalIdp = u.IsExternalIdp,
                                Product = DeserializeUserProduct(u.Product ?? ""),
                                EmployeeId = u.EmployeeId,
                                LastLogin = u.LastLogin
                            }
                        );
                    });
                }

                response.Data = usersDataDtoList.Cast<object>().ToList();
                response.Meta.CurrentPage = pageNumber;
                response.Meta.TotalRows = ((usersDataList != null) && (usersDataList.Count > 0)) ? usersDataList[0].TotalRecords : 0;
                response.Meta.RowsPerPage = rowsPerPage;
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exception)
            {
                // elastic logging
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", exception: exception, messageProperties: new object[] { "GetUser", $"Error while Get/List user(s). BooksMasterOrganizationId{_userClaims.OrganizationName}, Get/List user(s)" });
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get User role and asset details
        /// </summary>
        /// <param name="realPageId">The guid for the user being requested</param>
        /// <param name="productCode">The code for the product being requested. Supported products OPS-Ops</param>
        /// <returns>User role and asset details</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Not Found")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of User(s)", Type = typeof(UserRoleAssetDto))]
        [SwaggerResponseExamples(typeof(UserRoleAssetDto), typeof(EnterpriseGetUserRoleAssetExample))]
        [Route("user/{realPageId}/product/{productCode}")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public HttpResponseMessage GetUserRoleAsset(Guid realPageId, string productCode)
        {
            UserRoleAssetDto userRoleAssetDto = new UserRoleAssetDto();
            IList<UserRoleAssetDto> userRoleAssetDtoList = new List<UserRoleAssetDto>();
            PagedResponse response = new PagedResponse() { Meta = new Meta() };
            ErrorResponse error = new ErrorResponse()
            {
                Errors = new List<Error>()
            };

            Persona persona = new Persona();
            IManagePerson personLogic = new ManagePerson();
            IPerson person = personLogic.GetPerson(realPageId);

            if (person != null)
            {
                IManagePersona managePersona = new ManagePersona(_userClaims);
                //Active Persona is linked to one organization
                //persona = managePersona.GetActivePersona(realPageId);
                persona = managePersona.GetFirstAvailablePersonaByCompany(realPageId, _orgPartyId);

                //Verify if same company
                //IManageOrganization manageOrganization = new ManageOrganization();
                //bool isValidOrganization = manageOrganization.ValidateOrganization(_userClaims.OrganizationMasterId, _userClaims.UserRealPageGuid, persona.Organization.RealPageId);
                //if (!isValidOrganization)
                if (persona == null || persona.OrganizationPartyId != _orgPartyId)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            ListResponse listResponse = new ListResponse();
            var products = _productRepository.GetAllProducts();
            switch (ProductEnumHelper.GetProductIdByProductCode(productCode, products))
            {
                case (int)ProductEnum.OpsBuyer:
                    var samlRepository = new SamlRepository();
                    IList<PersonaProductUserDetails> productList = samlRepository.ListActiveProductsByPersonaId(persona.PersonaId, (int)ProductEnum.OpsBuyer, null);
                    if (productList.Any(p => p.ProductStatus == (int)ProductBatchStatusType.Success))
                    {
                        IManageProductOps manageProductOps = new ManageProductOps(_userClaims);
                        listResponse = manageProductOps.GetRoles(_userClaims.PersonaId, persona.PersonaId, "", null);
                        userRoleAssetDto.ProductRole = listResponse.Records.Cast<ProductRole>().ToList().FindAll(p => p.IsAssigned);

                        listResponse = manageProductOps.GetCompanyAssets(_userClaims.PersonaId, persona.PersonaId, false, null);
                        userRoleAssetDto.AssetGroups = listResponse.Records.Cast<AssetGroup>().ToList().FindAll(p => p.IsAssigned);

                        userRoleAssetDtoList.Add(userRoleAssetDto);
                    }

                    break;

                default:
                    error.Errors.Add(new Error() { Title = "Bad request", Detail = "No valid product code could be found", Source = "/user", StatusCode = "" });
                    return Request.CreateResponse(HttpStatusCode.BadRequest, error);
            }

            if (!listResponse.IsError)
            {
                response.Data = userRoleAssetDtoList.Cast<object>().ToList();
                response.Meta.CurrentPage = 1;
                response.Meta.TotalRows = userRoleAssetDtoList.Count;
                response.Meta.RowsPerPage = userRoleAssetDtoList.Count;
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }

            error.Errors.Add(new Error() { Title = "Error", Detail = listResponse.ErrorReason, Source = "/user", StatusCode = "" });
            return Request.CreateResponse(HttpStatusCode.BadRequest, error);
        }

        /// <summary>
        /// Get Users for a product, including role and property information
        /// </summary>
        /// <param name="productCode">The code for the product being requested. Supported products OPS-Ops</param>
        /// <param name="rowsPerPage">The number of records you want to return at a time. Maximum 100</param>
        /// <param name="pageNumber">The current page of data being requested</param>
        /// <returns>User role and asset details</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Not Found")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of User(s)")]
        [Route("user/product/{productCode}")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public HttpResponseMessage GetProductUsersWithRoleAsset(string productCode, int rowsPerPage = 1, int pageNumber = 1)
        {
            IList<OpsUserDataDto> opsUserListDto = new List<OpsUserDataDto>();
            PagedResponse response = new PagedResponse() { Meta = new Meta() };
            ErrorResponse error = new ErrorResponse()
            {
                Errors = new List<Error>()
            };

            if (rowsPerPage <= 0)
            {
                response.Data = opsUserListDto.Cast<object>().ToList();
                response.Meta.CurrentPage = pageNumber;
                response.Meta.TotalRows = 0;
                response.Meta.RowsPerPage = rowsPerPage;
                response.IsError = true;
                response.ErrorReason = "rowsPerPage must be 1 or greater.";
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }

            if (pageNumber <= 0)
            {
                response.Data = opsUserListDto.Cast<object>().ToList();
                response.Meta.CurrentPage = pageNumber;
                response.Meta.TotalRows = 0;
                response.Meta.RowsPerPage = rowsPerPage;
                response.IsError = true;
                response.ErrorReason = "pageNumber must be 1 or greater.";
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }

            RequestParameter requestParameter = new RequestParameter() { Pages = new PageRequest() { ResultsPerPage = rowsPerPage, StartRow = pageNumber } };
            var productList = _productRepository.GetAllProducts();
            switch (ProductEnumHelper.GetProductIdByProductCode(productCode, productList))
            {
                case (int)ProductEnum.OpsBuyer:
                    IManageProductOps manageProductOps = new ManageProductOps(_userClaims);
                    ListResponse listResponse = manageProductOps.GetUsers(_userClaims.PersonaId, requestParameter);

                    if (listResponse.Records.Count != 0)
                    {
                        List<OpsUser> userList = listResponse.Records.Cast<OpsUser>().ToList();

                        userList.ForEach(u =>
                        {
                            opsUserListDto.Add(
                                new OpsUserDataDto()
                                {
                                    Id = u.ID,
                                    LoginName = u.Loginname,
                                    Status = u.Status,
                                    UserType = new OpsUserType() { Id = u.UserType.Id, Name = u.UserType.Name },
                                    Asset = new OpsAssetGroupDto() { ID = u.AssetGroup.ID, Name = u.AssetGroup.Name, Code = u.AssetGroup.Code, Status = u.AssetGroup.Status }
                                }
                            );
                        });
                    }

                    response.Data = opsUserListDto.Cast<object>().ToList();
                    response.Meta.CurrentPage = pageNumber;
                    response.Meta.TotalRows = listResponse.TotalRows;
                    response.Meta.RowsPerPage = rowsPerPage;
                    return Request.CreateResponse(HttpStatusCode.OK, response);

                default:
                    error.Errors.Add(new Error() { Title = "Bad request", Detail = "No valid product code could be found", Source = "/user", StatusCode = "" });
                    return Request.CreateResponse(HttpStatusCode.BadRequest, error);
            }
        }

        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Operation successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("user/product/{productCode}/properties")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public HttpResponseMessage GetProductUserProperties(string productCode, [FromUri] RequestParameter dataFilter, Guid? upfmId = null, Guid? userRealPageId = null)
        {
            ListResponse result = new ListResponse();
            var status = HttpStatusCode.OK;

            try
            {
                var productList = _productRepository.GetAllProducts();
                int productId = ProductEnumHelper.GetProductIdByProductCode(productCode, productList);
                if (productId == (int)ProductEnum.VendorMarketplace)
                {
                    var currentClaimPrincipal = ClaimsPrincipal.Current;
                    if (!currentClaimPrincipal.HasClaim("scope", "internalapi"))
                    {
                        var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                        errorResponse.Errors.Add(new Error
                        { Title = "Error", Source = "/GetProductUserProperties", Detail = "Invalid Claim Scope.", StatusCode = "" });

                        // return errors with bad request
                        return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                    }
                    if (upfmId == null || userRealPageId == null)
                    {
                        var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                        errorResponse.Errors.Add(new Error
                        { Title = "Error", Source = "/GetProductUserProperties", Detail = "Invalid upfmId and userRealPageId.", StatusCode = "" });

                        // return errors with bad request
                        return Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                    }
                    else
                    {
                        productId = (int)ProductEnum.UnifiedPlatform;
                        RecreateClaimsForClient((Guid)userRealPageId, (Guid)upfmId);
                        if (_repository == null)
                        {
                            var manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
                            var manageProductOneSite = new ManageProductOneSite(_userClaims);
                            var productInternalSettingRepository = new ProductInternalSettingRepository();
                            _managePersona = new ManagePersona(_userClaims);
                            _manageProduct = new ManageProduct(_userClaims);
                            _userManagement = new UserManagement(_userClaims);
                            _manageUser = new ManageUser(_userClaims);
                            _userLoginLogic = new ManageUserLogin(_userClaims);
                            _integrationTypeFactory = new IntegrationTypeFactory(_manageProduct, manageUnifiedLogin, manageProductOneSite, _productRepository, productInternalSettingRepository, _userClaims);
                        }
                    }
                }

                var integration = _integrationTypeFactory.GetIntegration(productId);
                result = integration.GetEnterpriseProperties(_userClaims.PersonaId, dataFilter);

                if (result.IsError)
                {
                    status = HttpStatusCode.Forbidden;
                }
            }
            catch (Exception exception)
            {
                status = HttpStatusCode.InternalServerError;
                result = new ListResponse { IsError = true, ErrorReason = "Internal server error." };
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", exception: exception, messageProperties: new object[] { "GetProductUserProperties", "Error" });
            }

            return Request.CreateResponse(status, result);
        }

        /// <summary>
        /// Get a specific users product detail
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <returns>Profile object</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Profile object have invalid entries)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get a profile for a Person (User)", Type = typeof(UserProducts))]
        [SwaggerResponseExamples(typeof(UserProducts), typeof(GetUserProductsExample))]
        [Route("user/{realPageId}/products")]
        [AuthorizeScope("userinfoapi")]
        [HttpGet]
        public HttpResponseMessage GetUserProducts(Guid realPageId)
        {
            return GetUserProductDetails(realPageId);
        }

        /// <summary>
        /// Get the products for the given personaid 
        /// </summary>
        /// <param name="personaId">User unique identifier</param>
        /// <param name="withStatus">Need user status in the response</param>
        /// <returns>Profile object</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Profile object have invalid entries)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get a profile for a Person (User)", Type = typeof(UserProducts))]
        [SwaggerResponseExamples(typeof(UserProducts), typeof(GetUserProductsExamplev2))]
        [Route("user/products")]
        [AuthorizeScope("userinfoapi", "internalapi")]
        [HttpGet]
        public HttpResponseMessage GetUserProductsByPersonaId(long? personaId = 0, bool withStatus = false)
        {
            UserProductOutputResultv2 productResult = new UserProductOutputResultv2 { Products = new Dictionary<string, List<UserProducts>>(), Settings = new Dictionary<string, object>(), Resources = new List<UserProducts>() };

            if (!personaId.HasValue || personaId == 0)
            {
                personaId = _userClaims.PersonaId;
            }

            Persona persona = _managePersona.GetPersonaWithRightsToggle(personaId.Value, false);
            if (persona != null)
            {
                if (_userClaims.OrganizationPartyId == 0)
                {
                    List<Claim> claimList = ClaimsPrincipal.Current.Claims.ToList();
                    if (!claimList.Any(p => p.Type.Equals("Scope", StringComparison.OrdinalIgnoreCase) && p.Value.Equals("internalapi", StringComparison.OrdinalIgnoreCase)))
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Get User Products by persona: Invalid company id");
                    }
                }
                else if (!(_userClaims.ImpersonatedBy != Guid.Empty) && _userClaims.OrganizationPartyId != persona.OrganizationPartyId)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Get User Products by persona: Invalid company id");
                }

                var sessionTimeout = 480;
                var settingList = _manageSettings.GetUnifiedSettingsCached("Security", persona.OrganizationPartyId);
                if (settingList.Any(p => p.Name.Equals("SessionTimeout", StringComparison.OrdinalIgnoreCase)))
                {
                    if (Int32.TryParse(settingList.FirstOrDefault(p =>
                        p.Name.Equals("SessionTimeout", StringComparison.OrdinalIgnoreCase))?.Value, out var trySessionTimeout))
                    {
                        sessionTimeout = trySessionTimeout;
                    }
                }
                productResult.Settings.Add("SessionTimeout", sessionTimeout);

                var person = _personLogic.GetPerson(persona.RealPageId);

                if (person != null)
                {
                    IList<Persona> personaList = new List<Persona>();
                    IList<Persona> employeePersonaList = new List<Persona>();

                    personaList = _managePersona.ListActivePersona(persona.RealPageId, false);
                    persona.hasMultiPersona = personaList.Count(p => p.OrganizationPartyId == persona.OrganizationPartyId) > 1;
                    persona.hasMultiCompany = personaList.Count(p => p.OrganizationPartyId != persona.OrganizationPartyId && p.Organization.RealPageId != DefaultUserClaim.ExternalCompanyRealPageId) > 0;
                    
                    productResult.User = new User()
                    {
                        FullName = $"{person.FirstName} {person.LastName}",
                        RealPageId = person.RealPageId,
                        CompanyName = persona.Organization.Name,
                        PersonaId = persona.PersonaId,
                        Title = persona.hasMultiPersona == true ? persona.Name : "",
                        HasMultiCompany = persona.hasMultiCompany,
                        HasMultiPersona = false
                    };

                    if (withStatus)
                    {
                        var userLogin = _userLoginLogic.GetUserLogin(persona.RealPageId, persona.OrganizationPartyId);
                        productResult.User.Status = userLogin.Status.ToEnumDescription();
                    }

                    if (_userClaims.IsRPEmployee)
                    {
                        employeePersonaList = _managePersona.ListEmployeePersonas(_userClaims.UserId, _userClaims.OrganizationPartyId);
                        productResult.User.HasMultiPersona = employeePersonaList?.Count > 1;
                    }
                    var productList = _manageProduct.GetAllProductsByPersona(personaId.Value, ProductBatchStatusType.Success);

                    List<UserProducts> userProducts = ConvertPersonaProductsToRAUL(productList, personaId.Value);

                    productResult.Products.Add("Favorites", userProducts.Where(p => p.IsFavorite).ToList());
                    foreach (UserProducts up in userProducts)
                    {
                        string familyName = up.FamilyName ?? "None";
                        if (!up.IsResource && !productResult.Products.ContainsKey(familyName))
                        {
                            productResult.Products.Add(familyName, userProducts.Where(p => !p.IsFavorite && !p.IsResource && (p.FamilyName ?? "None").Equals(familyName, StringComparison.OrdinalIgnoreCase)).ToList());
                        }

                        if (up.IsResource)
                        {
                            productResult.Resources.Add(up);
                        }
                    }

                    var rights = _manangeSecurityLogic.GetPersonaRightsAndActionsByRoute(_personaId, "sidemenu")?.obj?.Rights;

                    var navigationMenu = _userRepository.GetNavigationMenu();
                    var navigationMenuRights = _userRepository.GetNavigationMenuRights();

                    var filteredMenuEntries = navigationMenu.Where(
                        nmw => !navigationMenuRights.Any(w => w.NavigationMenuId == nmw.Id)
                            || navigationMenuRights.Where(w => w.NavigationMenuId == nmw.Id).Any(a => rights.Contains(a.RightName))
                        ).ToList();


                    var reportingMenuEntry = filteredMenuEntries.FirstOrDefault(f => f.PageId == "reporting");
                    if (reportingMenuEntry != null)
                    {
                        var products = _productRepository.GetAllProducts();
                        string productcode = ProductEnumHelper.GetProductCodeByProductId(67, products);

                        var reportsUrl = new Uri(new Uri(ConfigReader.GetLandingUri), reportingMenuEntry.URL);

                        productResult.Resources.Add(new UserProducts()
                        {
                            Name = "Reports",
                            Description = reportingMenuEntry.Title,
                            Url = reportsUrl.ToString(),
                            Label = "reports",
                            IsNewTab = false,
                            IsResource = true,
                            ShowInAppSwitcher = true,
                            ProductCode = productcode,
                            Status = 8,
                            Id = 67
                        });
                    }

                    var settingsMenuEntry = filteredMenuEntries.FirstOrDefault(f => f.PageId == "manage-settings");
                    if (settingsMenuEntry != null)
                    {
                        var products = _productRepository.GetAllProducts();
                        string productcode = ProductEnumHelper.GetProductCodeByProductId(56, products);

                        var settingsUri = new Uri(new Uri(ConfigReader.GetLandingUri), settingsMenuEntry.URL);

                        productResult.Resources.Add(new UserProducts()
                        {
                            Name = "Settings",
                            Description = settingsMenuEntry.Title,
                            Url = settingsUri.ToString(),
                            Label = "settings",
                            IsNewTab = false,
                            IsResource = true,
                            ShowInAppSwitcher = true,
                            ProductCode = productcode,
                            Status = 8,
                            Id = 56
                        });
                    }

                    // Support Tool User should not have access to Client Portal
                    if (_userClaims.ImpersonatedBy != Guid.Empty)
                    {
                        if (productResult.Resources.Any(a => a.Id == (int)ProductEnum.ClientPortal))
                        {
                            productResult.Resources.Remove(productResult.Resources.First(a => a.Id == (int)ProductEnum.ClientPortal));
                        }
                        if (productResult.Resources.Any(a => a.Id == (int)ProductEnum.AdminSupportPortal))
                        {
                            productResult.Resources.Remove(productResult.Resources.First(a => a.Id == (int)ProductEnum.AdminSupportPortal));
                        }
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, productResult);
        }

        /// <summary>
		/// Get Saml product attributes by  ProductId
		/// </summary>
		/// <param name="ProductId"></param>
		/// <returns>List of Saml Attributes</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the user", Type = typeof(SamlProductAttributes))]
        [SwaggerResponseExamples(typeof(SamlProductAttributes), typeof(SamlProductAttributesExample))]
        [Authorize]
        [Route("user/productuser/attributes")]
        [HttpGet]
        public IList<SamlProductAttributes> GetSamlProductAttributes(int ProductId)
        {
            //var samlRepository = new SamlRepository();
            return _samlRepository.GetSamlProductAttributes(ProductId);
        }

        /// <summary>
        /// Used to update details for a Realpage product (OneSite, Accounting, VendorServices) user for the given GreenBook user
        /// </summary>
        /// <param name="productUser">Details to save for a user</param>
        /// <remarks>Possible values for ProductStatus : (Sucess, Hidden, Deactivated, Error,etc). ProductId can be retrieved from /products endpoint. Product Saml Attributes can be retrieved from /user/productuser/attributes endpoint.
        /// </remarks>

        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("user/productuser/details")]
        [AuthorizeScope("internalapi")]
        [HttpPut]
        public HttpResponseMessage UpdateProductUserAccountDetails([FromBody] ProductUserAccountDetails productUser)
        {
            if (productUser == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "productUser null.");

            if (productUser.ProductId <= 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "ProductName empty.");

            var result = _manageProductUser.UpdateProductUserAccountDetails(productUser, true);

            if (string.IsNullOrEmpty(result))
                result = "Success";

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Used to delete details for a Realpage product (OneSite, Accounting, VendorServices) user for the given GreenBook user
        /// </summary>
        /// <param name="productUser">Details to delete for a user</param>
        /// <remarks>Possible values for ProductStatus : (Sucess, Hidden, Deactivated, Error,etc). ProductId can be retrieved from /products endpoint. Product Saml Attributes can be retrieved from /user/productuser/attributes endpoint.
        /// </remarks>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("user/productuser/details")]
        [AuthorizeScope("internalapi")]
        [HttpDelete]
        public HttpResponseMessage DeleteSamlUserProductInfoAndStatus([FromBody] ProductUserAccountDetails productUser)
        {
            if (productUser == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "productUser null.");

            if (productUser.ProductId <= 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "ProductName empty.");

            var result = _manageProductUser.DeleteSamlUserProductInfoAndStatus(productUser, true);

            if (string.IsNullOrEmpty(result))
                result = "Success";

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Get the user details for the OmniBar
        /// </summary>
        /// <returns>Profile object</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Profile object have invalid entries)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get a profile for a Person (User)", Type = typeof(UserProducts))]
        [SwaggerResponseExamples(typeof(UserProducts), typeof(GetUserProductsExample))]
        [Route("user/omnibar")]
        [AuthorizeScope("userinfoapi")]
        [HttpGet]
        public HttpResponseMessage GetOmnibarInfo()
        {
            return GetUserProductDetails_v2(_userClaims.UserRealPageGuid, null);
        }


        /// <summary>
        /// Get Persona company list
        /// </summary>
        /// <returns>List of persona for the given user</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Persona object have invalid entries)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List of personas", Type = typeof(PersonaCompany))]
        [SwaggerResponseExamples(typeof(PersonaCompany), typeof(PersonaCompanyListExample))]
        [Route("user/personas")]
        [AuthorizeScope("userinfoapi")]
        [HttpGet]
        public HttpResponseMessage GetPersonasList()
        {
            ObjectListOutput<PersonaCompany, IErrorData> output = new ObjectListOutput<PersonaCompany, IErrorData>();
            var personaList = _managePersona.ListActivePersona(_realpageUserId, true);
            List<PersonaCompany> pcl = new List<PersonaCompany>();
            foreach (var persona in personaList)
            {
                if (persona.Organization.RealPageId != DefaultUserClaim.ExternalCompanyRealPageId)
                {
                    if (pcl.All(p => p.CompanyRealPageId != persona.Organization.RealPageId))
                    {
                        pcl.Add(new PersonaCompany() { CompanyName = persona.Organization.Name, CompanyRealPageId = persona.Organization.RealPageId, Personas = new List<PersonaCompanyDetails>() });
                    }

                    IList<PersonaCompanyDetails> currentCompanyPersonaList = pcl.Find(p => p.CompanyRealPageId == persona.Organization.RealPageId).Personas;
                    currentCompanyPersonaList.Add(new PersonaCompanyDetails()
                    {
                        PersonaId = persona.PersonaId,
                        Name = persona.Name,
                    });
                }
            }

            pcl = pcl.OrderBy(p => p.CompanyName).ToList();
            output.list = pcl;

            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// Get Employee Personas list
        /// </summary>
        /// <returns>List of persona for the given user</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Persona object have invalid entries)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List of Employee personas", Type = typeof(PersonaCompanyDetails))]
        // [SwaggerResponseExamples(typeof(PersonaCompany), typeof(PersonaCompanyListExample))]
        [Route("user/employeepersonas")]
        [AuthorizeScope("userinfoapi")]
        [HttpGet]
        public HttpResponseMessage GetEmployeePersonasList()
        {
            ObjectListOutput<PersonaCompanyDetails, IErrorData> output = new ObjectListOutput<PersonaCompanyDetails, IErrorData>();
            var personaList = _managePersona.ListEmployeePersonas(_userClaims.UserId, _userClaims.OrganizationPartyId);
            List<PersonaCompanyDetails> pcl = new List<PersonaCompanyDetails>();
            foreach (var persona in personaList)
            {
                pcl.Add(new PersonaCompanyDetails()
                {
                    PersonaId = persona.PersonaId,
                    Name = persona.Name,
                });
            }

            output.list = pcl;

            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// Used to trigger the notification event that the user changed company
        /// </summary>
        /// <param name="personaId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("user/persona/{personaId}/company")]
        public HttpResponseMessage ChangeCompany(long personaId = 0)
        {
            // accept client token from UL?

            Persona persona = _managePersona.GetPersona(_userClaims.PersonaId);

            IList<Persona> personaList = _managePersona.ListActivePersona(persona.RealPageId, false);
            if (personaList.Any(p => p.PersonaId == personaId))
            {
                var result = _managePersona.ChangeCompanyNotification(personaId);
                return new HttpResponseMessage(result == Guid.Empty ? HttpStatusCode.BadRequest : HttpStatusCode.Accepted);
            }

            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// Get Users Product Detail Login By PersonaId
        /// </summary>
        /// <param name="personaId"></param>
        /// <returns>A list of User(s) with the product details.</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Not Found")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of User(s) with the product details.", Type = typeof(UserProductDetailLogin))]
        [SwaggerResponseExamples(typeof(UserProductDetailLogin), typeof(GetUserProductsDetailsLoginExample))]
        [Route("user/products/details/login")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public HttpResponseMessage GetUserProductsDetailsLoginByPersonaId()
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, _userManagement.ListUserProductDetailsLoginByPersonaId(_userClaims.PersonaId));
            }
            catch (Exception ex)
            {
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetUserProductsDetailsLoginByPersonaId", "Error" });
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        /// <summary>
        /// Get the user (Regular and External) with the product login details and company by LoginName
        /// </summary>
        /// <returns>User with SAML attributes for all companies</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Not Found")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get the user (Regular and External) with the product login details and company.", Type = typeof(UserProductDetailLogin))]
        [SwaggerResponseExamples(typeof(UserProductDetailLogin), typeof(GetUserProductsDetailsLoginCompanyExample))]
        [Route("user/products/details/login/company")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public HttpResponseMessage GetUserProductsDetailsLoginByLoginName()
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, _userManagement.ListUserProductDetailsLoginByLoginName(_userClaims.LoginName));
            }
            catch (Exception ex)
            {
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetUserProductsDetailsLoginByLoginName", "Error" });
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        /// <summary>
        /// Gets the list of rights for the current authenticated user
        /// </summary>
        /// <returns>A list of the users rights</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get the users UnifiedLogin rights")]
        [Route("user/rights/current")]
        [HttpGet]
        [AuthorizeScope("userinfoapi", "landingapi")]
        public HttpResponseMessage GetCurrentUserRights()
        {
            List<string> userRights = _userClaims.Rights;

            return Request.CreateResponse(HttpStatusCode.OK, userRights);
        }

        /// <summary>
        /// List User Custom Fields
        /// </summary>
        /// <returns>A list of user's customfields</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Gets the User Custom Fields")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List Organization Types", Type = typeof(ICustomFieldValue))]
        [SwaggerResponseExamples(typeof(ICustomFieldValue), typeof(UserCustomFieldsExample))]
        [Route("customfieldsmaster")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public HttpResponseMessage UserCustomFields(long? userLoginPersonaId = null)
        {
            IManageCustomFields manageCustomFields = new ManageCustomFields(_userClaims);

            IList<CustomFieldValue> customFieldValueList = manageCustomFields.GetCustomFieldsValues(organizationPartyId: _userClaims.OrganizationPartyId, userLoginPersonaId: userLoginPersonaId, enabled: true);

            ListResponse response = new ListResponse()
            {
                Records = customFieldValueList.Cast<object>().ToList(),
                TotalRows = customFieldValueList.Count(),
                RowsPerPage = customFieldValueList.Count(),
                ErrorReason = string.Empty,
                TotalPages = 1
            };
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        #region Private Methods

        private ProfileDetail BuildProfileByInput(UserProductDetailsDto userProductDetailsDto, IList<CustomFieldValue> userCustomFields)
        {
            ProfileDetail profileDetail = new ProfileDetail();
            profileDetail.CustomFields = new List<CustomFieldValue>();
            profileDetail.productBatch = new List<ProductBatch>();
            profileDetail.userLogin = new UserLogin();
            profileDetail.ExternalUserRelationship = new ExternalUserRelationship();

            profileDetail.Suffix = userProductDetailsDto.UserProfileDetails.Suffix;
            profileDetail.Title = userProductDetailsDto.UserProfileDetails.Title;
            profileDetail.UserTypeId = GetGbUserType(userProductDetailsDto.UserProfileDetails.UserType);
            profileDetail.ExternalUserRelationship = GetUserRelationship(userProductDetailsDto.UserProfileDetails.UserType);
            profileDetail.FirstName = userProductDetailsDto.UserProfileDetails.FirstName;
            profileDetail.LastName = userProductDetailsDto.UserProfileDetails.LastName;
            profileDetail.MiddleName = userProductDetailsDto.UserProfileDetails.MiddleName;
            profileDetail.CreateUserSourceType = CreateUserSourceType.UnifiedPlatform;
            profileDetail.NotificationEmail = userProductDetailsDto.UserProfileDetails.Email;
            profileDetail.CustomFields = userCustomFields;
            profileDetail.EmployeeId = userProductDetailsDto.UserProfileDetails.EmployeeId;

            //Add support for phone number
            if (!string.IsNullOrEmpty(userProductDetailsDto.UserProfileDetails.Phone))
            {
                profileDetail.TelecommunicationNumber = new List<TelecommunicationNumber>();
                TelecommunicationNumber number = new TelecommunicationNumber();
                number.CountryCode = "+1";
                number.ISOCode = "US";
                number.PhoneNumber = userProductDetailsDto.UserProfileDetails.Phone;
                number.IsDeleted = false;
                number.IsPreferred = false;
                number.IsDefault = false;
                number.AreaCode = String.Empty;
                number.contactMechanismUsageType = new ContactMechanismUsageType() { ContactMechanismUsageTypeId = 203, Name = String.Empty };
                number.PartyContactMechanismId = 0;
                number.ContactMechanismId = 0;
                profileDetail.TelecommunicationNumber.Add(number);
            }

            if (userProductDetailsDto.UserProfileDetails.UnityRealPageUserId != Guid.Empty)
            {
                //Update existing user                
                profileDetail.userLogin = _userLoginLogic.GetUserLogin(userProductDetailsDto.UserProfileDetails.UnityRealPageUserId, _userClaims.OrganizationPartyId);
                profileDetail.organization.Add(_manageOrganization.GetOrganization(Guid.Empty, _userClaims.OrganizationPartyId));
                profileDetail.Persona.Add(_managePersona.GetActivePersona(userProductDetailsDto.UserProfileDetails.UnityRealPageUserId));
                UserRepository userRepository = new UserRepository(_userClaims);
                profileDetail.PartyId = profileDetail.userLogin.PartyId;
                profileDetail.RealPageId = userProductDetailsDto.UserProfileDetails.UnityRealPageUserId;

                IManageUserLoginPersona manageUserLoginPersona = new ManageUserLoginPersona(_userClaims);
                IList<UserLoginPersona> userLoginPersonaList = manageUserLoginPersona.ListUserLoginPersona(userLoginPersonaId: null, userLoginId: profileDetail.userLogin.UserId, organizationPartyId: _userClaims.OrganizationPartyId);
                profileDetail.ExternalUserRelationship = userRepository.GetExternalUserRelationship(userLoginPersonaList[0].UserLoginPersonaId);
            }
            else
            {
                //Create new user
                profileDetail.userLogin.ThruDate = null;
                profileDetail.userLogin.IsActive = true;
                profileDetail.userLogin.IsPending = false;
                profileDetail.userLogin.IsExpired = false;
                profileDetail.userLogin.FromDate = userProductDetailsDto.UserProfileDetails.UserEffectiveDate;
                profileDetail.userLogin.Is3rdPartyIDP = userProductDetailsDto.UserProfileDetails.IsExternalIdp;
                profileDetail.Password = userProductDetailsDto.UserProfileDetails.Password;
                profileDetail.userLogin.doNotForceChangePassword = userProductDetailsDto.UserProfileDetails.doNotForceChangePassword;
            }

            profileDetail.userLogin.LoginName = userProductDetailsDto.UserProfileDetails.LoginName;
            if(userProductDetailsDto.ProductList != null) 
            {
                foreach (var pl in userProductDetailsDto.ProductList)
                {
                    ProductBatch pBatch = new ProductBatch();
                    pBatch.InputJson = new RolePropertyList();

                    var productList = _productRepository.GetAllProducts();
                    pBatch.ProductId = ProductEnumHelper.GetProductIdByProductCode(pl.ProductCode, productList);
                    pBatch.StatusTypeId = (int)ProductBatchStatusType.Waiting;
                    pBatch.RetryCount = 0;
                    pBatch.InputJson.RoleList = pl.RolesAssigned;
                    pBatch.InputJson.PropertyList = pl.PropertiesAssigned;
                    pBatch.InputJson.IsAssigned = pl.IsAssigned;
                    if (pBatch.ProductId == (int)ProductEnum.OpsBuyer && userProductDetailsDto.UserProfileDetails.UnityRealPageUserId != Guid.Empty)
                    {
                        var response = _manageProductPanel.GetProductProperties(_userClaims.PersonaId, profileDetail.Persona[0].PersonaId, pBatch.ProductId, null);
                        var removeProp = response.Records?.Cast<AssetGroup>()?.Where(c => c.IsAssigned == true)?.Select(y => y.ID)?.ToList();
                        pBatch.InputJson.RemovedPropertyList = removeProp ?? new List<string>();
                    }

                    profileDetail.productBatch.Add(pBatch);
                }
            }

            return profileDetail;
        }

        private HttpResponseMessage GetUserProductDetails(Guid realPageId)
        {
            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            if (!currentClaimPrincipal.Identity.IsAuthenticated)
            {
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }

            ObjectOutput<IProfileDetail, IErrorData> output = new ObjectOutput<IProfileDetail, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            IPerson person = new Person();

            if (realPageId == Guid.Empty)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "User.GetUserProducts.1";
                errorStatus.ErrorMsg = "Get User Products: Invalid parameter realPageId";
                output.Status = errorStatus;
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, output);
                }
            }

            IManagePerson personLogic = new ManagePerson();
            person = personLogic.GetPerson(realPageId);

            if (person != null)
            {
                UserProductOutputResult productResult = new UserProductOutputResult();

                productResult.User = new User()
                {
                    FullName = $"{person.FirstName} {person.LastName}",
                    RealPageId = person.RealPageId
                };

                IManagePersona managePersona = new ManagePersona(_userClaims);
                //Active Persona is linked to one organization
                //Persona persona = managePersona.GetActivePersona(realPageId);
                Persona persona = managePersona.GetFirstAvailablePersonaByCompany(realPageId, _orgPartyId);

                //Verify if same company
                //IManageOrganization manageOrganization = new ManageOrganization();
                //bool isValidOrganization = manageOrganization.ValidateOrganization(_userClaims.OrganizationMasterId, _userClaims.UserRealPageGuid, persona.Organization.RealPageId);
                //if (!isValidOrganization)
                if (persona == null || persona.OrganizationPartyId != _orgPartyId)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.GetProfile.3";
                    errorStatus.ErrorMsg = "Get User Profile: User exists in a different organization.";
                    output.Status = errorStatus;
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, output);
                    }
                }

                productResult.User.Title = persona.Name;
                productResult.User.CompanyName = persona.Organization.Name;

                IManageSecurity manangeSecurityLogic = new ManageSecurity(_userClaims);
                ObjectOutput<RouteSecurity, IErrorData> routeSecurity = manangeSecurityLogic.GetPersonaRightsAndActionsByRoute(persona.PersonaId, "dashboard");
                RouteSecurity security = routeSecurity.obj;

                IManageProduct manageProduct = new ManageProduct(_userClaims);
                IList<PersonaProductUserDetails> products = manageProduct.GetUserAssignedProductsByPersona(persona);
                IList<PersonaProductUserDetails> resources = manageProduct.GetUserAssignedProductsByPersona(persona: persona, productSelectType: ProductSelectType.ResourcesOnly, security: security);
                products = products.Where(p => p.ShowInAppSwitcher).ToList();
                resources = resources.Where(p => p.ShowInAppSwitcher).ToList();

                productResult.Products = ConvertDashboardProductsToRAUL(products);

                productResult.Resources = ConvertDashboardProductsToRAUL(resources);
                return Request.CreateResponse(HttpStatusCode.OK, productResult);
            }

            errorStatus.Success = false;
            errorStatus.ErrorCode = "User.GetUserProducts.2";
            errorStatus.ErrorMsg = "Get User Products: No data.";
            output.Status = errorStatus;
            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        private HttpResponseMessage GetUserProductDetails_v2(Guid realPageId, long? personaId)
        {
            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            if (!currentClaimPrincipal.Identity.IsAuthenticated)
            {
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }

            ObjectOutput<IProfileDetail, IErrorData> output = new ObjectOutput<IProfileDetail, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            IPerson person = new Person();

            if (!personaId.HasValue && realPageId == Guid.Empty)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "User.GetUserProducts.1";
                errorStatus.ErrorMsg = "Get User Products: Invalid parameter realPageId";
                output.Status = errorStatus;
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, output);
                }
            }

            IManagePerson personLogic = new ManagePerson();
            IManagePersona managePersona = new ManagePersona();
            Persona persona = null;

            if (!personaId.HasValue)
            {
                person = personLogic.GetPerson(realPageId);
                persona = managePersona.GetFirstAvailablePersonaByCompany(realPageId, _orgPartyId);
            }
            else
            {
                persona = managePersona.GetPersona(personaId.Value);
                person = personLogic.GetPerson(persona.RealPageId);
                realPageId = persona.RealPageId;
            }

            if (person != null)
            {
                UserProductOutputResultv2 productResult = new UserProductOutputResultv2();

                productResult.User = new User()
                {
                    FullName = $"{person.FirstName} {person.LastName}",
                    RealPageId = person.RealPageId
                };


                //Active Persona is linked to one organization
                //Persona persona = managePersona.GetActivePersona(realPageId);


                //Verify if same company
                //IManageOrganization manageOrganization = new ManageOrganization();
                //bool isValidOrganization = manageOrganization.ValidateOrganization(_userClaims.OrganizationMasterId, _userClaims.UserRealPageGuid, persona.Organization.RealPageId);
                //if (!isValidOrganization)
                if (persona == null) // || persona.OrganizationPartyId != _orgPartyId)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.GetProfile.3";
                    errorStatus.ErrorMsg = "Get User Profile: User exists in a different organization.";
                    output.Status = errorStatus;
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, output);
                    }
                }

                productResult.User.CompanyName = persona.Organization.Name;
                productResult.User.PersonaId = persona.PersonaId;
                productResult.User.Title = persona.Name;

                IManageSecurity manangeSecurityLogic = new ManageSecurity(_userClaims);
                ObjectOutput<RouteSecurity, IErrorData> routeSecurity = manangeSecurityLogic.GetPersonaRightsAndActionsByRoute(persona.PersonaId, "dashboard");
                RouteSecurity security = routeSecurity.obj;
                IManageProduct manageProduct = new ManageProduct(_userClaims);
                IList<PersonaProductUserDetails> products = manageProduct.GetUserAssignedProductsByPersona(persona);
                IList<PersonaProductUserDetails> resources = manageProduct.GetUserAssignedProductsByPersona(persona: persona, productSelectType: ProductSelectType.ResourcesOnly, security: security);
                products = products.Where(p => p.ShowInAppSwitcher).ToList();

                resources = resources.Where(p => p.ShowInAppSwitcher).ToList();
                List<UserProducts> userProducts = ConvertDashboardProductsToRAULv2(products);
                productResult.Products = new Dictionary<string, List<UserProducts>>();

                productResult.Products.Add("Favorites", userProducts.Where(p => p.IsFavorite).ToList());
                foreach (UserProducts up in userProducts)
                {
                    if (!productResult.Products.ContainsKey(up.FamilyName))
                    {
                        productResult.Products.Add(up.FamilyName, userProducts.Where(p => !p.IsFavorite && p.FamilyName.Equals(up.FamilyName, StringComparison.OrdinalIgnoreCase)).ToList());
                    }
                }


                productResult.Resources = ConvertDashboardProductsToRAUL(resources);
                return Request.CreateResponse(HttpStatusCode.OK, productResult);
            }

            errorStatus.Success = false;
            errorStatus.ErrorCode = "User.GetUserProducts.2";
            errorStatus.ErrorMsg = "Get User Products: No data.";
            output.Status = errorStatus;
            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        private UserProductDetails GetUserBusinessObject(UserProductDetailsDto userProductDetailsDto)
        {
            var userProductDetails = new UserProductDetails
            {
                EditorRealPageId = _userClaims.UserRealPageGuid,
                UserProfileDetails = new UserData
                {
                    UserRealPageId = userProductDetailsDto.UserProfileDetails.UnityRealPageUserId,
                    //AdditionalFields = userProductDetailsDto.UserProfileDetails.AdditionalFields,
                    MiddleName = userProductDetailsDto.UserProfileDetails.MiddleName,
                    Password = userProductDetailsDto.UserProfileDetails.Password,
                    LoginName = userProductDetailsDto.UserProfileDetails.LoginName,
                    Title = userProductDetailsDto.UserProfileDetails.Title,
                    Email = userProductDetailsDto.UserProfileDetails.Email,
                    FirstName = userProductDetailsDto.UserProfileDetails.FirstName,
                    UserType = GetGbUserType(userProductDetailsDto.UserProfileDetails.UserType),
                    IsExternalIdp = userProductDetailsDto.UserProfileDetails.IsExternalIdp,
                    LastName = userProductDetailsDto.UserProfileDetails.LastName,
                    OrganizationRealPageId = _userClaims.OrganizationRealPageGuid,
                    OrganizationPartyId = _userClaims.OrganizationPartyId,
                    Phone = userProductDetailsDto.UserProfileDetails.Phone,
                    UserEffectiveDate = userProductDetailsDto.UserProfileDetails.UserEffectiveDate,
                    UserExpirationDate = userProductDetailsDto.UserProfileDetails.UserExpirationDate,
                    CreateUserSourceType = CreateUserSourceType.RPX.ToString(),
                    Suffix = userProductDetailsDto.UserProfileDetails.Suffix,
                    CustomFields = userProductDetailsDto.UserProfileDetails.CustomFields,
                    EmployeeId = userProductDetailsDto.UserProfileDetails.EmployeeId,
                    SendInvitationEmail = userProductDetailsDto.UserProfileDetails.SendInvitationEmail
                },
                ProductList = new List<ProductDetail>()
            };
            if (userProductDetailsDto.ProductList != null)
            {
                foreach (var product in userProductDetailsDto.ProductList)
                {
                    userProductDetails.ProductList.Add(new ProductDetail
                    {
                        ProductCode = product.ProductCode,
                        AdditionalFields = product.AdditionalFields,
                        PropertiesAssigned = product.PropertiesAssigned,
                        RegionsAssigned = product.RegionsAssigned,
                        RolesAssigned = product.RolesAssigned,
                        IsAssigned = product.IsAssigned
                    });
                }
            }

            return userProductDetails;
        }

        private int GetGbUserType(UserTypeDto userTypeDto)
        {
            int userType;

            switch (userTypeDto)
            {
                case UserTypeDto.Regular:
                    userType = (int)UserRoleType.User;
                    break;
                case UserTypeDto.NoEmail:
                    userType = (int)UserRoleType.UserNoEmail; //"Regular User (No Email)";
                    break;
                case UserTypeDto.Employee:
                    userType = (int)UserRoleType.RealPageEmployee;
                    break;
                case UserTypeDto.External:
                    userType = (int)UserRoleType.ExternalUser;
                    break;
                case UserTypeDto.SuperUser:
                    userType = (int)UserRoleType.SuperUser;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(userTypeDto), userTypeDto, null);
            }

            return userType;
        }

        private ExternalUserRelationship GetUserRelationship(UserTypeDto userTypeDto)
        {
            ExternalUserRelationship obj = new ExternalUserRelationship();
            switch (userTypeDto)
            {
                case UserTypeDto.Regular:
                    obj.ThirdPartyRelationShipId = 4;
                    obj.ThirdPartyRelationShip = "4";
                    break;
                case UserTypeDto.NoEmail:
                    obj.ThirdPartyRelationShipId = 6;
                    obj.ThirdPartyRelationShip = "6";
                    break;
                case UserTypeDto.SuperUser:
                    obj.ThirdPartyRelationShipId = 8;
                    obj.ThirdPartyRelationShip = "8";
                    break;
                case UserTypeDto.External:
                    obj.ThirdPartyRelationShipId = 5;
                    obj.ThirdPartyRelationShip = "5";
                    break;
                case UserTypeDto.Employee:
                    obj.ThirdPartyRelationShipId = 9;
                    obj.ThirdPartyRelationShip = "9";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(userTypeDto), userTypeDto, null);
            }
            return obj;
        }

        /// <summary>
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            try
            {
                var logger = Log.Logger;
                if (logData?.Keys != null)
                {
                    logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
                }
                logger = logger.ForContext("ProductModule", this.GetType());
                logger = logger.ForContext("CorrelationId", _userClaims.CorrelationId.ToString());

                logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
            }
            catch
            {
                /*ignored*/
            }
        }

        /// <summary>
		/// List of User Product and SAML attributes 
		/// </summary>
		/// <param name="userProductJSON">CustomFields JSON string</param>
		/// <returns>List of User Product and SAML attributes objects</returns>
		private IList<UserProductSAMLDetail> DeserializeUserProduct(string userProductJSON)
        {
            IList<UserProductSAMLDetail> productlist = new List<UserProductSAMLDetail>();

            if (userProductJSON == string.Empty)
            {
                return productlist;
            }

            productlist = JsonConvert.DeserializeObject<IList<UserProductSAMLDetail>>(userProductJSON);
            return productlist;
        }

        /// <summary>
        /// Used to return the product list of the user to the RAUL UI component
        /// </summary>
        /// <param name="products"></param>
        /// <returns></returns>
        private List<UserProducts> ConvertDashboardProductsToRAUL(IList<PersonaProductUserDetails> products)
        {
            var productIconSettings = _manageProduct.GetProductSettingByType("ProductIcon");

            List<UserProducts> productList = new List<UserProducts>();
            foreach (PersonaProductUserDetails prodDetail in products)
            {
                if (!string.IsNullOrEmpty(prodDetail.ProductUrl))
                {
                    UserProducts up = new UserProducts()
                    {
                        Id = prodDetail.ProductId,
                        Name = prodDetail.ProductName,
                        Description = prodDetail.ProductDescription,
                        Url = prodDetail.ProductUrl.ToUpper().Contains("HTTP") ? prodDetail.ProductUrl : ConfigReader.GetLandingUri + prodDetail.ProductUrl,
                        Label = productIconSettings.FirstOrDefault(f => f.ProductId == prodDetail.ProductId)?.Value,
                        FamilyId = prodDetail?.FamilyId,
                        FamilyName = prodDetail.Family,
                        IsFavorite = prodDetail.IsFavorite,
                        IsNewTab = prodDetail.IsNewTab,
                        IsResource = prodDetail.IsResource,
                        Status = prodDetail.ProductStatus,
                        ProductCode = ((ProductEnum)prodDetail.ProductId).ToEnumDescription()
                    };
                    productList.Add(up);
                }
            }

            return productList.OrderBy(p => p.FamilyName).ThenBy(p => p.Name).ToList();
        }

        private List<UserProducts> ConvertPersonaProductsToRAUL(IList<PersonaProduct> products, long personaId)
        {
            var productIconSettings = _manageProduct.GetProductSettingByType("ProductIcon");

            List<UserProducts> productList = new List<UserProducts>();

            foreach (PersonaProduct prodDetail in products)
            {
                UserProducts up = new UserProducts()
                {
                    Id = prodDetail.ProductId,
                    Name = prodDetail.Name,
                    Description = prodDetail.Description,
                    Url = prodDetail.Url != null && prodDetail.Url.ToUpper().Contains("HTTP") ? prodDetail.Url : ConfigReader.GetLandingUri + $"product-redirect.html?prod={prodDetail.ProductId}&persona={personaId}",
                    Label = productIconSettings.FirstOrDefault(f => f.ProductId == prodDetail.ProductId)?.Value,
                    FamilyId = prodDetail?.FamilyId,
                    FamilyName = prodDetail.FamilyName,
                    IsFavorite = prodDetail.isFavorite,
                    IsNewTab = prodDetail.IsNewTab,
                    IsResource = prodDetail.IsResource,
                    Status = prodDetail.StatusTypeId,
                    ProductCode = prodDetail.BooksProductCode,
                    ShowInAppSwitcher = prodDetail.ShowInAppSwitcher
                };
                productList.Add(up);
            }

            return productList;
        }

        /// <summary>
        /// Used to return the product list of the user to the RAUL UI component
        /// </summary>
        /// <param name="products"></param>
        /// <returns></returns>
        private List<UserProducts> ConvertDashboardProductsToRAULv2(IList<PersonaProductUserDetails> products)
        {
            var productIconSettings = _manageProduct.GetProductSettingByType("ProductIcon");

            List<UserProducts> productList = new List<UserProducts>();
            foreach (PersonaProductUserDetails prodDetail in products)
            {
                if (!string.IsNullOrEmpty(prodDetail.ProductUrl))
                {
                    UserProducts up = new UserProducts()
                    {
                        Id = prodDetail.ProductId,
                        Name = prodDetail.ProductName,
                        Description = prodDetail.ProductDescription,
                        Url = prodDetail.ProductUrl.ToUpper().Contains("HTTP") ? prodDetail.ProductUrl : ConfigReader.GetLandingUri + $"product-redirect.html?prod={prodDetail.ProductId}&persona={prodDetail.PersonaId}",
                        Label = productIconSettings.FirstOrDefault(f => f.ProductId == prodDetail.ProductId)?.Value,
                        FamilyId = prodDetail?.FamilyId,
                        FamilyName = prodDetail.Family,
                        IsFavorite = prodDetail.IsFavorite,
                        IsNewTab = prodDetail.IsNewTab,
                        IsResource = prodDetail.IsResource,
                        Status = prodDetail.ProductStatus,
                        ProductCode = ((ProductEnum)prodDetail.ProductId).ToEnumDescription()
                    };
                    productList.Add(up);
                }
            }

            return productList.OrderBy(p => p.FamilyName).ThenBy(p => p.Name).ToList();
        }

        /// <summary>
        /// Used for client credential calls to assign the support tool admin user when attempting to get company information
        /// </summary>
        /// <param name="upfmId"></param>
        /// <returns></returns>
        private ErrorResponse AttemptClientCredentialAuthentication(Guid? upfmId)
        {
            if (string.IsNullOrEmpty(upfmId.ToString())) return null;
            var currentClaimPrincipal = ClaimsPrincipal.Current;

            if ((!currentClaimPrincipal.HasClaim("scope", "usermanagement") && !currentClaimPrincipal.HasClaim("scope", "internalapi")) || _userClaims.PersonaId != 0) return null;
            

            var adminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(upfmId ?? default(Guid));
            //recreate clams
            if (adminCreatorRealPageId == Guid.Empty)
            {
                return new ErrorResponse { Errors = new List<Error>() { new Error { Title = "Error", Source = "/user", Detail = "Invalid UPFMId.", StatusCode = "" } } };
            }

            RecreateClaimsForClient(adminCreatorRealPageId);
            if (_repository == null)
            {
                _managePersona = new ManagePersona(_userClaims);
                _manageProduct = new ManageProduct(_userClaims);
                _userManagement = new UserManagement(_userClaims);
                _manageUser = new ManageUser(_userClaims);
                _userLoginLogic = new ManageUserLogin(_userClaims);
            }
            else
            {
                // Unit test
                var productInternalSettingRepository = new ProductInternalSettingRepository(_repository);
                var manageBlueBook = new ManageBlueBook(_userClaims, _repository, productInternalSettingRepository, _messageHandler);
                var personaRightRepository = new PersonaRightRepository(_repository);
                var manageUnifiedLogin = new ManageUnifiedLogin(_repository, _userClaims, _messageHandler);
                var manageProductOneSite = new ManageProductOneSite(_repository, _userClaims, _messageHandler, _oneSiteProductService);

                _managePersona = new ManagePersona(_repository, _userClaims, _messageHandler);
                _manageOrganization = new ManageOrganization(_repository, _userClaims, _messageHandler);
                _manageSettings = new ManageUnifiedSettings(_repository, _userClaims, _messageHandler);
                _manageProduct = new ManageProduct(_repository, _userClaims, _messageHandler);
                _manageProductPanel = new ManageProductPanel(_userClaims, _repository, manageBlueBook, _messageHandler, manageProductOneSite);
                _productRepository = new ProductRepository(_repository, _userClaims);
                
                _userRepository = new UserRepository(_repository, _userClaims, _messageHandler);
                _manangeSecurityLogic = new ManageSecurity(_userClaims, personaRightRepository);
                _integrationTypeFactory = new IntegrationTypeFactory(_manageProduct, manageUnifiedLogin, manageProductOneSite, _productRepository, productInternalSettingRepository, _userClaims);
                _manageUser = new ManageUser(_repository, _userClaims, _messageHandler);
                _userLoginLogic = new ManageUserLogin(_repository, _userClaims, _messageHandler);

                _manageProductUser = new ManageProductUser(_repository, _userClaims, _messageHandler, _oneSiteProductService);
                _samlRepository = new SamlRepository(_repository);
            }

            return null;
        }


        #endregion

        /// <summary>
        /// The user details
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class User
        {
            /// <summary>
            /// The users full name
            /// </summary>
            public string FullName { get; set; }

            /// <summary>
            /// The users title
            /// </summary>

            public string Title { get; set; }

            /// <summary>
            /// the users company
            /// </summary>

            public string CompanyName { get; set; }

            /// <summary>
            /// the users realpage id
            /// </summary>

            public Guid RealPageId { get; set; }

            /// <summary>
            /// The users current persona id
            /// </summary>
            public long PersonaId { get; set; }

            /// <summary>
            /// Does the user have multiple companies
            /// </summary>
            public bool HasMultiCompany { get; set; }
            /// <summary>
            /// Does the user have multiple personas
            /// </summary>
            public bool HasMultiPersona { get; set; } = false;

            /// <summary>
            /// User status
            /// </summary>
            public string Status { get; set; }

        }

        /// <summary>
        /// Output result for newly created user
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class UserProductOutputResult
        {
            /// <summary>
            /// The user
            /// </summary>
            public User User { get; set; }

            /// <summary>
            /// User Product list
            /// </summary>
            public List<UserProducts> Products { get; set; }

            /// <summary>
            /// User Resource list
            /// </summary>
            public List<UserProducts> Resources { get; set; }
        }

        /// <summary>
        /// Output result for newly created user
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class UserProductOutputResultv2
        {
            /// <summary>
            /// The user
            /// </summary>
            public User User { get; set; }

            /// <summary>
            /// User Product list
            /// </summary>
            public Dictionary<string, List<UserProducts>> Products { get; set; }

            /// <summary>
            /// User Resource list
            /// </summary>
            public List<UserProducts> Resources { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public Dictionary<string, object> Settings { get; set; }
        }

        /// <summary>
        /// Details about the products assigned to the user
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class UserProducts
        {
            /// <summary>
            /// Product id
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The name of the product
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The url to the product for login
            /// </summary>
            public string Url { get; set; }

            /// <summary>
            /// The product description
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// The icon for the product
            /// </summary>
            public string Label { get; set; }

            /// <summary>
            /// The Family id the product belongs to
            /// </summary>
            public int? FamilyId { get; set; }

            /// <summary>
            /// The Family the product belongs to
            /// </summary>
            public string FamilyName { get; set; }

            /// <summary>
            /// Does the product open in a new window
            /// </summary>
            public bool IsNewTab { get; set; }

            /// <summary>
            /// Has the product been favourited
            /// </summary>
            public bool IsFavorite { get; set; }

            /// <summary>
            /// Is the product a resource
            /// </summary>
            public bool IsResource { get; set; }

            /// <summary>
            /// /The status of the product, 7 errored, 8 success, 10 deleted
            /// </summary>
            public int Status { get; set; }

            /// <summary>
            /// Books product code
            /// </summary>
            public string ProductCode { get; set; }

            /// <summary>
            /// Should the application show in the app switcher
            /// </summary>
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShowInAppSwitcher { get; set; }
        }

        /// <summary>
        /// Output result for newly created user
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class NewUserIDOutputResult
        {
            /// <summary>
            /// Newly created user id
            /// </summary>
            public int UserId { get; set; }
        }

        /// <summary>
        /// Output result for Update user
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class LastModifiedDateOutputResult
        {
            /// <summary>
            /// User last modified date time
            /// </summary>
            public int UserId { get; set; }
        }

        /// <summary>
        /// Used to document examples of the New User webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class NewUserOutputResultExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Newly created user id</returns>
            public object GetExamples()
            {
                return Component.SharedObjects.User.GetNewUserExample();
            }
        }

        #region GetExamples


        //SamlProductAttributes
        /// <summary>
        /// Used to document examples of the Product Saml setting webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class SamlProductAttributesExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>User List example</returns>
            public object GetExamples()
            {
                List<SamlProductAttributes> example = new List<SamlProductAttributes>();
                example.Add(new SamlProductAttributes()
                {
                    DisplayName = "Username",
                    ProductID = 1,
                    SamlAttributeName = "productUsername"
                });
                example.Add(new SamlProductAttributes()
                {
                    DisplayName = "PMC ID",
                    ProductID = 1,
                    SamlAttributeName = "PMCID"
                });
                example.Add(new SamlProductAttributes()
                {
                    DisplayName = "User ID",
                    ProductID = 1,
                    SamlAttributeName = "UserId"
                });

                return example;
            }
        }

        /// <summary>
        /// Used to document examples of the UserProducts result
        /// </summary>

        [ExcludeFromCodeCoverage]
        public class GetUserProductsExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Newly created user id</returns>
            public object GetExamples()
            {
                UserProductOutputResult productResult = new UserProductOutputResult();

                List<UserProducts> userProducts = new List<UserProducts>()
                {
                    new UserProducts()
                    {
                        Id = 15,
                        Name = "Renters Insurance",
                        Url = "https://mylocal.corp.realpage.com/product/insurance?personaid=33",
                        Description = "RealPage Renters Insurance makes covering your assets, and those of your residents, simple. Our eRenterPlan program offers residents affordable, comprehensive coverage, while optional RenterProtection provides gap coverage for vacant units or uninsured residents.",
                        Label = "insurance",
                        FamilyId = 200,
                        FamilyName = "Resident Services",
                        IsNewTab = true,
                        IsFavorite = false,
                        IsResource = false,
                        ProductCode = ProductEnum.Insurance.ToEnumDescription()
                    },
                    new UserProducts()
                    {
                        Id = 1,
                        Name = "OneSite",
                        Url = "https://mylocal.corp.realpage.com/product/onesite?personaid=33",
                        Description = "The OneSite environment provides access to Leasing and Rents, Facilities, Purchasing, and Document Management for your properties, depending the mix of products which are licensed.  Use this logo for future state Leasing & Rents.  Also need to discuss whether one tile for Leasing & Rents will apply to Affordable, Senior, and Student. ",
                        Label = "onesite",
                        FamilyId = 100,
                        FamilyName = "Property Management",
                        IsNewTab = true,
                        IsFavorite = false,
                        IsResource = false,
                        ProductCode = ProductEnum.OneSite.ToEnumDescription()
                    },
                };

                productResult.User = new User() { FullName = "Full name", Title = "User title", CompanyName = "User Company", RealPageId = new Guid() };

                productResult.Products = userProducts;
                //productResult.Resources = userResources;

                return productResult;

            }
        }

        /// <summary>
        /// Used to document examples of the UserProducts result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class GetUserProductsExamplev2 : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Newly created user id</returns>
            public object GetExamples()
            {
                UserProductOutputResult productResult = new UserProductOutputResult();

                List<UserProducts> userProducts = new List<UserProducts>()
                {
                    new UserProducts()
                    {
                        Id = 15,
                        Name = "Renters Insurance",
                        Url = "https://www-local.realpage.com/home/product-redirect.html?prod=15&persona=28793",
                        Description = "RealPage Renters Insurance makes covering your assets, and those of your residents, simple. Our eRenterPlan program offers residents affordable, comprehensive coverage, while optional RenterProtection provides gap coverage for vacant units or uninsured residents.",
                        Label = "insurance",
                        FamilyId = 200,
                        FamilyName = "Resident Services",
                        IsNewTab = true,
                        IsFavorite = false,
                        IsResource = false,
                        ProductCode = ProductEnum.Insurance.ToEnumDescription(),
                        ShowInAppSwitcher = true,
                        Status = 8
                    },
                    new UserProducts()
                    {
                        Id = 1,
                        Name = "OneSite",
                        Url = "https://www-local.realpage.com/home/product-redirect.html?prod=1&persona=28793",
                        Description = "The OneSite environment provides access to Leasing and Rents, Facilities, Purchasing, and Document Management for your properties, depending the mix of products which are licensed.  Use this logo for future state Leasing & Rents.  Also need to discuss whether one tile for Leasing & Rents will apply to Affordable, Senior, and Student. ",
                        Label = "onesite",
                        FamilyId = 100,
                        FamilyName = "Property Management",
                        IsNewTab = true,
                        IsFavorite = false,
                        IsResource = false,
                        ProductCode = ProductEnum.OneSite.ToEnumDescription(),
                        ShowInAppSwitcher = true,
                        Status = 8
                    },
                };

                List<UserProducts> userResources = new List<UserProducts>()
                {
                    new UserProducts()
                    {
                        Id = 45,
                        Name = "CIMPL",
                        Url = "https://www-local.realpage.com/home/product-redirect.html?prod=45&persona=28793",
                        Description = "CIMPL",
                        Label = "cimpl",
                        FamilyId = 500,
                        FamilyName = "Administration",
                        IsNewTab = true,
                        IsFavorite = false,
                        IsResource = true,
                        ProductCode = ProductEnum.CIMPL.ToEnumDescription(),
                        ShowInAppSwitcher = true,
                        Status = 8
                    },
                    new UserProducts()
                    {
                        Id = 49,
                        Name = "Help Center",
                        Url = "https://helpcenterqa.realpage.com",
                        Description = "Help Center",
                        Label = "help-center",
                        FamilyId = 500,
                        FamilyName = "Administration",
                        IsNewTab = true,
                        IsFavorite = false,
                        IsResource = true,
                        ProductCode = ProductEnum.HelpCenter.ToEnumDescription(),
                        ShowInAppSwitcher = false,
                        Status = 8
                    },
                };

                productResult.User = new User() { FullName = "Full name", Title = "User title", CompanyName = "User Company", RealPageId = new Guid() };

                productResult.Products = userProducts;
                productResult.Resources = userResources;

                return productResult;

            }
        }

        /// <summary>
        /// Used to document examples of the webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class EnterpriseGetUserExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>EnterpriseRole example</returns>
            public object GetExamples()
            {
                IList<CustomFieldValue> customFieldsList = new List<CustomFieldValue>()
                {
                    new CustomFieldValue()
                    {
                        FieldId = 1,
                        OrganizationId = 350,
                        Enabled = true,
                        Name = "Employee ID",
                        Description = "Employee ID",
                        FieldTypeId = 1,
                        FieldTypeName = "alphanumeric",
                        Required = true,
                        ReadOnly = false,
                        DefaultValue = "",
                        SyncField = "",
                        Sequence = 1,
                        HelpText = "Employee ID",
                        MinCharLength = 1,
                        MaxCharLength = 10,
                        UserLoginPersonaId = 600,
                        FieldValueId = 1,
                        Value = "12345"
                    },
                    new CustomFieldValue()
                    {
                        FieldId = 2,
                        OrganizationId = 350,
                        Enabled = true,
                        Name = "Status",
                        Description = "Status",
                        FieldTypeId = 1,
                        FieldTypeName = "alphanumeric",
                        Required = true,
                        ReadOnly = false,
                        DefaultValue = "",
                        SyncField = "",
                        Sequence = 1,
                        HelpText = "Status",
                        MinCharLength = 1,
                        MaxCharLength = 10,
                        UserLoginPersonaId = 600,
                        FieldValueId = 1,
                        Value = "Active"
                    }
                };

                IList<UserProductSAMLDetail> UserProductSAMLDetaillist = new List<UserProductSAMLDetail>()
                {
                    new UserProductSAMLDetail()
                    {
                        Id = "1192422|jreames1",
                        UserName = "jreames1",
                        ProductCode = "OS"
                    },
                    new UserProductSAMLDetail()
                    {
                        Id = "80172",
                        UserName = "lanecoadmin",
                        ProductCode = "OPS"
                    }
                };

                Dictionary<String, String> dictionaryCustomFields = new Dictionary<String, String>();

                customFieldsList.ToList().ForEach(c =>
                {
                    dictionaryCustomFields.Add(c.Name, c.Value);
                });

                IList<UsersDataDto> usersDataDtoList = new List<UsersDataDto>()
                {
                    new UsersDataDto()
                    {
                        UserStatus = "active",
                        UserType = "RealPage System Administrator",
                        UnityRealPageUserId = new Guid("c9167175-0676-4546-bba7-4a49d5809b1f"),
                        FirstName = "James",
                        MiddleName = "X",
                        LastName = "Jackson",
                        IsExternalIdp = false,
                        LoginName = "james.jackson@example.com",
                        Email = "james.jackson@example.com",
                        UserEffectiveDate = DateTime.Now,
                        UserExpirationDate = DateTime.Now,
                        CustomFields = dictionaryCustomFields,
                        Product = UserProductSAMLDetaillist,
                        EmployeeId = "2020EmployeeId"
                    }
                };

                PagedResponse response = new PagedResponse()
                {
                    Meta = new Meta() { TotalRows = usersDataDtoList.Count, CurrentPage = 1, RowsPerPage = usersDataDtoList.Count },
                    Data = usersDataDtoList.Cast<object>().ToList(),

                };

                return response;
            }
        }

        /// <summary>
        /// Used to document examples of the webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class EnterpriseGetUserRoleAssetExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>EnterpriseRole example</returns>
            public object GetExamples()
            {
                IList<UserRoleAssetDto> userRoleAssetDtoList = new List<UserRoleAssetDto>()
                {
                    new UserRoleAssetDto()
                    {
                        ProductRole = new List<ProductRole>()
                        {
                            new ProductRole()
                            {
                                ID = "15088",
                                Name = "Marketplace Administrator",
                                IsAssigned = true,
                                isEditorHasRight = false,
                                Roletype = "1"
                            }
                        },
                        AssetGroups = new List<AssetGroup>()
                        {
                            new AssetGroup()
                            {
                                ID = "1125",
                                Name = "[G] CF Real Estate Services",
                                Code = null,
                                Description = "",
                                Status = "active",
                                GroupType = "company",
                                AssetID = "204955",
                                IsAssigned = true
                            }
                        }
                    }
                };

                PagedResponse response = new PagedResponse()
                {
                    Meta = new Meta() { TotalRows = userRoleAssetDtoList.Count, CurrentPage = 1, RowsPerPage = userRoleAssetDtoList.Count },
                    Data = userRoleAssetDtoList.Cast<object>().ToList(),

                };

                return response;
            }
        }

        /// <summary>
        /// Used to document examples of the PersonaCompany webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class PersonaCompanyListExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Persona example</returns>
            public object GetExamples()
            {
                List<PersonaCompany> pclList = new List<PersonaCompany>();
                pclList.Add(new PersonaCompany() { CompanyName = "Company A", CompanyRealPageId = Guid.NewGuid(), Personas = new List<PersonaCompanyDetails>() { new PersonaCompanyDetails() { PersonaId = 1111, Name = "Persona" } } });
                pclList.Add(new PersonaCompany() { CompanyName = "Company B", CompanyRealPageId = Guid.NewGuid(), Personas = new List<PersonaCompanyDetails>() { new PersonaCompanyDetails() { PersonaId = 2222, Name = "Other Persona" } } });
                ObjectOutput<List<PersonaCompany>, IErrorData> output = new ObjectOutput<List<PersonaCompany>, IErrorData>() { obj = pclList };

                return output;
            }
        }

        /// <summary>
		/// Used to document examples of the webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
        public class GetUserProductsDetailsLoginExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Enterprise User Product Details Login example</returns>
            public object GetExamples()
            {
                List<Dictionary<string, string>> detailsProduct1 = new List<Dictionary<string, string>>();
                List<Dictionary<string, string>> detailsProduct2 = new List<Dictionary<string, string>>();


                Dictionary<string, string> detail1 = new Dictionary<string, string>
                {
                    {"name","productUsername" },
                    { "value","unity"}
                };

                Dictionary<string, string> detail2 = new Dictionary<string, string>
                {
                    {"name","UserId" },
                    { "value","rpi-vidya|unity"}
                };

                detailsProduct1.Add(detail1);
                detailsProduct1.Add(detail2);

                Dictionary<string, string> detail3 = new Dictionary<string, string>
                {
                    {"name","productUsername" },
                    { "value","cfadmin@test.com"}
                };

                Dictionary<string, string> detail4 = new Dictionary<string, string>
                {
                    {"name","UserId" },
                    { "value","00529000001F1nnAAC"}
                };

                Dictionary<string, string> detail5 = new Dictionary<string, string>
                {
                    {"name","portal_id" },
                    { "value","060000000005YDy"}
                };

                Dictionary<string, string> detail6 = new Dictionary<string, string>
                {
                    {"name","organization_id" },
                    { "value","00D290000000XyT"}
                };

                detailsProduct2.Add(detail3);
                detailsProduct2.Add(detail4);
                detailsProduct2.Add(detail5);
                detailsProduct2.Add(detail6);

                List<UserProductDetailLogin> response = new List<UserProductDetailLogin>
                {
                    new UserProductDetailLogin
                    {
                        ProductCode = "ACCT",
                        ProductId = 8,
                        Details = detailsProduct1
                    },

                    new UserProductDetailLogin
                    {
                        ProductCode = "OMS",
                        ProductId = 14,
                        Details = detailsProduct2
                    }
                };

                return response;
            }
        }

        /// <summary>
		/// Used to document examples of the webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
        public class GetUserProductsDetailsLoginCompanyExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Enterprise User Product Details Login example</returns>
            public object GetExamples()
            {
                List<Dictionary<string, string>> detailsProduct1 = new List<Dictionary<string, string>>();
                List<Dictionary<string, string>> detailsProduct2 = new List<Dictionary<string, string>>();
                List<Dictionary<string, string>> detailsProduct3 = new List<Dictionary<string, string>>();


                Dictionary<string, string> detail1 = new Dictionary<string, string>
                {
                    {"name","productUsername" },
                    { "value","Cattribute1@noreply.com"}
                };

                Dictionary<string, string> detail2 = new Dictionary<string, string>
                {
                    {"name","UserId" },
                    { "value","1222013090"}
                };

                detailsProduct1.Add(detail1);
                detailsProduct1.Add(detail2);

                Dictionary<string, string> detail3 = new Dictionary<string, string>
                {
                    {"name","productUsername" },
                    { "value","cristianattri@test.com.co"}
                };

                Dictionary<string, string> detail4 = new Dictionary<string, string>
                {
                    {"name","UserId" },
                    { "value","169335"}
                };

                detailsProduct2.Add(detail3);
                detailsProduct2.Add(detail4);

                Dictionary<string, string> detail5 = new Dictionary<string, string>
                {
                    {"name","productUsername" },
                    { "value","cristianattri@test.com.co"}
                };

                Dictionary<string, string> detail6 = new Dictionary<string, string>
                {
                    {"name","UserId" },
                    { "value","103388"}
                };


                detailsProduct3.Add(detail5);
                detailsProduct3.Add(detail6);

                List<UserProductDetailLogin> response = new List<UserProductDetailLogin>
                {
                    new UserProductDetailLogin
                    {
                        ProductCode = "LS",
                        ProductId = 9,
                        Company = "JVM REALTY CORPORATION",
                        RealPageId = new Guid("7e52666c-9737-4406-b144-ad1530ba18f0"),
                        UserType = "ExternalUser",
                        Details = detailsProduct1
                    },

                    new UserProductDetailLogin
                    {
                        ProductCode = "L2L",
                        ProductId = 6,
                        Company = "RP Northstar Management Demo",
                        RealPageId = new Guid("e087788f-0765-4d00-9b8b-47663370f701"),
                        UserType = "User",
                        Details = detailsProduct2
                    },

                    new UserProductDetailLogin
                    {
                        ProductCode = "AB",
                        ProductId = 17,
                        Company = "RP Northstar Management Demo",
                        RealPageId = new Guid("e087788f-0765-4d00-9b8b-47663370f701"),
                        UserType = "User",
                        Details = detailsProduct3
                    }
                };

                return response;
            }
        }

        [ExcludeFromCodeCoverage]
        public class UserCustomFieldsExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>List of User CustomFields example</returns>
            public object GetExamples()
            {
                IList<CustomFieldValue> customFieldValueList = new List<CustomFieldValue>()
            {
                new CustomFieldValue()
                {
                    FieldValueId = 1,
                    UserLoginPersonaId = 1,
                    Value = "12345",
                    FieldId = 15,
                    OrganizationId = 350,
                    Enabled = true,
                    Name = "Employee ID",
                    Description = null,
                    FieldTypeId = 1,
                    FieldTypeName = "Alphanumeric",
                    Required = false,
                    ReadOnly = false,
                    DefaultValue = null,
                    SyncField = null,
                    Sequence = 1,
                    HelpText = null,
                    MinCharLength = 1,
                    MaxCharLength = 10
                }
            };

                ListResponse response = new ListResponse()
                {
                    Records = customFieldValueList.Cast<object>().ToList(),
                    TotalRows = customFieldValueList.Count(),
                    RowsPerPage = customFieldValueList.Count(),
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
                return response;
            }
        }

        #endregion
    }
}
