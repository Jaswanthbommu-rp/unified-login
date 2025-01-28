using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.SAML;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
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
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using Thinktecture.IdentityModel.Client;
using static RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.SAML.RealPageSAML;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Base;
using Serilog.Events;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    /// <summary>
    /// Controller for product related APIs
    /// </summary>
    public class ProductController : BaseApiController
    {
        #region Private variables

        private IManageProduct _manageProduct;
        private IUserLoginRepository _userLoginRepository;
        private ISamlRepository _samlRepository;
        private Guid emptyGuid = Guid.Empty;
        private string _key = "4AD12A31-680A-476F-863E-26749D2E7DD4";

        private IRepository _repository;
        private IProductRepository _productRepository;
        private IManageBlueBook _manageBlueBook;
        private IManagePersona _managePersona;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ProductController()
        {
            // DONT USE USERCLAIM IN BASE, IT IS NULL AT THIS POINT. MOVE TO Initialize FUNCTION
        }

        /// <summary>
        /// Testing Constructor
        /// </summary>
        /// <param name="userClaim"></param>
        /// <param name="repository"></param>
        /// <param name="productRepository"></param>
        /// <param name="messageHandler"></param>
        public ProductController(DefaultUserClaim userClaim, IRepository repository, IProductRepository productRepository, HttpMessageHandler messageHandler)
        {
            _userClaims = userClaim;
            _repository = repository;
            var productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _manageBlueBook = new ManageBlueBook(userClaim, repository, productInternalSettingRepository, messageHandler);
            _manageProduct = new ManageProduct(_repository, _userClaims, messageHandler);
            _productRepository = productRepository;
            _managePersona = new ManagePersona(_repository, _userClaims, messageHandler);
            _userLoginRepository = new UserLoginRepository(repository);
            _samlRepository = new SamlRepository(repository);
        }

        /// <summary>
        /// Used to initialize DI classes with userclaim
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            _manageProduct = new ManageProduct(_userClaims);
            _productRepository = new ProductRepository(_userClaims);
            _manageBlueBook = new ManageBlueBook(_userClaims);
            _managePersona = new ManagePersona(_userClaims);
            _userLoginRepository = new UserLoginRepository();
            _samlRepository = new SamlRepository();
        }
        #endregion

        #region Methods
        
        /// <summary>
        /// List product user(s) for an organization
        /// </summary>
        /// <param name="productId">Unique ProductId</param>
        /// <param name="companyInstanceId">Unique blueBook CompanyInstanceId</param>
        /// <param name="personaId">Optional Unique PersonaId</param>
        /// <returns>Response with Success Message</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List product users", Type = typeof(ProductUsers))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/{productId}/organization/{companyInstanceId}")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage ListProductUsers(int productId, long companyInstanceId, long personaId = 0)
        {
            ObjectListOutput<ProductUsers, IErrorData> output = new ObjectListOutput<ProductUsers, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            output.Status = errorStatus;

            if (((string.IsNullOrWhiteSpace(_clientCode)) || (_clientCode.ToUpper() != "BBA")) && ((_realpageUserId == Guid.Empty) || (_realpageUserId == null)))
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Product.ListProductUsers.1";
                errorStatus.ErrorMsg = "Product - Invalid ClientCode and Enterprise User Id";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }

            if (!Enum.IsDefined(typeof(ProductEnum), productId))
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Product.ListProductUsers.2";
                errorStatus.ErrorMsg = "Product - Invalid parameter Product Id";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }

            if ((companyInstanceId < -1) || (companyInstanceId == 0))
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Product.ListProductUsers.3";
                errorStatus.ErrorMsg = "Product - Invalid parameter company Instance Id";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }

            if (personaId < 0)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Product.ListProductUsers.4";
                errorStatus.ErrorMsg = "Product - Invalid parameter Persona Id";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }

            ListResponse listResponse = new ListResponse();
            IList<ProductUsers> listProductUsers = _manageProduct.GetProductUsers(productId, companyInstanceId, personaId);
            if (listProductUsers == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Product.ListProductUsers.5";
                errorStatus.ErrorMsg = "Product - List Product Users: No data";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }

            output.list = listProductUsers;
            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// Get product families
        /// </summary>
        /// <param name="personRealPageId">The user unique identifier (Optional - Use Logged-in user unique identifier if parameter value is Guid.Empty)</param>
        /// <param name="accessFilter">filter products by area (such as "user details" or "rolesandrights")</param>
        /// <param name="loginName">The login name of the user</param>
        /// <returns>list of product families and products</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the product families", Type = typeof(IProductFamily))]
        [SwaggerResponseExamples(typeof(IProductFamily), typeof(ProductFamilyMethodExample))]
        [Route("productfamilies")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetProductFamilies(Guid? personRealPageId = null, string accessFilter = null, string loginName = null)
        {
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            ObjectListOutput<ProductFamily, IErrorData> output = new ObjectListOutput<ProductFamily, IErrorData>();
            IList<ProductFamily> productFamilyList = new List<ProductFamily>();

            Guid? userRealPageId = (personRealPageId == Guid.Empty || personRealPageId == null) ? _realpageUserId : personRealPageId;

            //TODO: FIX PRODUCTS SO WE DONT CLONE PRODUCTS THIS USER DOESN'T HAVE
            productFamilyList = _manageProduct.GetProductFamilies(_userClaims.OrganizationRealPageGuid, _realpageUserId, personRealPageId, accessFilter, loginName);

            if (productFamilyList != null)
            {
                output.list = productFamilyList;
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }

            //When trying to get a list of Product Families that doesn't exists
            errorStatus.Success = false;
            errorStatus.ErrorCode = "Product.GetProductFamilies.1";
            errorStatus.ErrorMsg = "Product Families: No data found.";
            output.Status = errorStatus;
            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// Used to get product internal settings
        /// </summary>
        /// <param name="ProductId">The id of the product to get the settings for</param>
        /// <param name="Key">A guid needed to retrieve the information. Only the system should get this information, no user should need to call this method.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the productsolutions", Type = typeof(ProductInternalSetting))]
        [SwaggerResponseExamples(typeof(ProductInternalSetting), typeof(ProductInternalSettingExample))]
        [Route("product/internalsettings")]
        [Authorize]
        [HttpGet]
        public List<ProductInternalSetting> GetProductInternalSettings(int ProductId, Guid Key)
        {
            ObjectListOutput<ProductInternalSetting, IErrorData> output = new ObjectListOutput<ProductInternalSetting, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            var productInternalSettingsList = new List<ProductInternalSetting>();
            if (Key != new Guid(_key))
            {
                return productInternalSettingsList;
            }

            productInternalSettingsList = _manageProduct.GetProductInternalSettings(ProductId);
            output.list = productInternalSettingsList;
            output.Status = errorStatus;
            return productInternalSettingsList;
        }

        
        /// <summary>
        /// Used to get product internal settings
        /// </summary>
        /// <param name="productid">The id of the product to get the settings for</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the product settings", Type = typeof(ProductInternalSetting))]
        [SwaggerResponseExamples(typeof(ProductInternalSetting), typeof(ProductInternalSettingExample))]
        [Route("product/{productid:int}/settings")]
        [Authorize]
        [HttpGet]
        public List<ProductInternalSetting> GetProductNonSensitiveSettings(int productid)
        {
            return _manageProduct.GetProductInternalSettings(productid)?.Where(p => !p.SensitiveData).OrderBy(p => p.Name).ToList();
        }

        /// <summary>
        /// Used to get all non-sensitive product internal settings for the given product type
        /// </summary>
        /// <param name="productSettingType">The id of the product to get the settings for</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the product settings", Type = typeof(ProductInternalSettingByType))]
        [SwaggerResponseExamples(typeof(ProductInternalSettingByType), typeof(ProductInternalSettingByTypeExample))]
        [Route("product/{productSettingType}/settings")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetAllProductNonSensitiveSettingsByType(string productSettingType, string orgType = null)
        {
            var listResult = _manageProduct.GetProductSettingByType(productSettingType,orgType)?.Where(p => !p.SensitiveData).OrderBy(p => p.ProductName).ToList();
            ObjectListOutput<ProductInternalSettingByType, IErrorData> output = new ObjectListOutput<ProductInternalSettingByType, IErrorData> {list = listResult, Status = new Status<IErrorData>(), pagingSummary = new PagingSummary() {TotalRecords = listResult?.Count ?? 0, TotalPages = 1}};
            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// Used to update a product internal setting
        /// </summary>
        /// <param name="productId">The id of the product to get the settings for</param>
        /// <param name="productInternalSetting"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update information about a product setting", Type = typeof(ProductInternalSetting))]
        [SwaggerResponseExamples(typeof(ProductInternalSetting), typeof(ProductInternalSettingExample))]
        [Route("product/{productid}/settings")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateProductSettingAndLinkToConfiguration(int productId, ProductInternalSetting productInternalSetting)
        {
            RepositoryResponse response = _manageProduct.CreateProductSettingAndLinkToConfiguration(productId, productInternalSetting);

            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, response.ErrorMessage);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Get a list of product setting types
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List of product setting types", Type = typeof(ProductSettingType))]
        [Route("product/settingtypes")]
        [Authorize]
        [HttpGet]
        public IList<ProductSettingType> ListProductSettingType()
        {
            return _manageProduct.ListProductSettingType();
        }


        /// <summary>
        /// Used to get product saml login
        /// </summary>
        /// <param name="ProductId">The id of the product to get the settings for</param>
        /// <param name="PersonaId">Persona Id.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the productsolutions", Type = typeof(ProductInternalSetting))]
        [SwaggerResponseExamples(typeof(ProductInternalSetting), typeof(ProductInternalSettingExample))]
        [Route("product/login")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetProductLoginInfo(int ProductId, long PersonaId)
        {
            var saml = new RealPageSAML(_userClaims);
            try
            {
                return null;
                //return saml.GetSaml(Request, ProductId, PersonaId);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        /// <summary>
        /// Get productTypes
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the productTypes", Type = typeof(ProductType))]
        [SwaggerResponseExamples(typeof(ProductType), typeof(ProductTypeMethodExample))]
        [Route("productTypes")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetProductTypes()
        {
            ObjectListOutput<ProductType, IErrorData> output = new ObjectListOutput<ProductType, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            IList<ProductType> productTypes = _manageProduct.GetProductTypes();
            output.list = productTypes;
            output.Status = errorStatus;
            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// List Products
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the books products", Type = typeof(GbProductMap))]
        [Route("booksproductmap")]
        [Authorize]
        [HttpGet]
        public IList<GbProductMap> GetBooksProductMap()
        {
            IList<GbProductMap> booksProductMap = _manageProduct.ListProducts();

            return booksProductMap;
        }

        /// <summary>
        /// Get the list of UDM Sources
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the books products", Type = typeof(UDMSource))]
        [Route("udmsources")]
        [Authorize]
        [HttpGet]
        public IEnumerable<UDMSource> GetUDMSourceList()
        {
            return _manageBlueBook.GetUDMSourceList();
        }

        /// <summary>
        /// Get the list of operators for the current UPFM company
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The list of operators for the current UPFM company", Type = typeof(UPFMOperators))]
        [Route("operators")]
        [Authorize]
        [HttpGet]
        public IEnumerable<UPFMOperators> GetUDMOperators()
        {
            return _manageBlueBook.GetOperatorListForUPFMCompany(_userClaims.OrganizationRealPageGuid, "UPFM");
        }


        [Route("product/{productId:int}/persona/{personaId}")]
        [Authorize]
        [HttpGet]
        public ProductLoginResponse GetProductLoginDetails(int productId, long personaId)
        {
            ProductLoginResponse productLoginResponse;
            bool isProductReport = false;
            string userAccessToken = string.Empty;

            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            string accessToken = (from nvp in currentClaimPrincipal.Claims where nvp.Type == "token" select nvp.Value).FirstOrDefault();

            if (personaId == 0)
                personaId = _userClaims.PersonaId;

            RealPageSAML rpsaml = new RealPageSAML(_userClaims);

            var productInternalSettingsList = _manageProduct.GetProductInternalSettings(productId);

            var userLoginOnly = _userLoginRepository.GetUserLoginOnly(_userClaims.UserRealPageGuid);
            var orgStatus = _userLoginRepository.GetUserOrganizationWithStatus(_userClaims.UserId, userLoginOnly.LastLogin, _userClaims.OrganizationPartyId, false);

            if ((orgStatus.IsActive.HasValue && !orgStatus.IsActive.Value) || orgStatus.IsLocked == true)
            {
                return new ProductLoginResponse() {ErrorMessage = "User not active"};
            }
            
            bool isUserExistsInProductCheckRequired = (productInternalSettingsList?.FirstOrDefault(s => s.Name.Equals("isUserExistsInProductCheckRequired", StringComparison.OrdinalIgnoreCase))?.Value) == "1";
            if (isUserExistsInProductCheckRequired)
            {
                getUserAccessToken(productId, productInternalSettingsList, out var productLoginResponseMessage);
                if (!string.IsNullOrEmpty(productLoginResponseMessage.ErrorMessage))
                {
                    return new ProductLoginResponse() { ErrorMessage = productLoginResponseMessage.ErrorMessage };
                }
                else
                {
                    userAccessToken = productLoginResponseMessage.AccessToken;
                }
            }
            IList<SamlAttributes> samlAttributeDetails = new List<SamlAttributes>();
            bool IsUserCreationOnTileClick = (productInternalSettingsList?.FirstOrDefault(s => s.Name.Equals("IsUserCreationOnTileClick", StringComparison.OrdinalIgnoreCase))?.Value) == "true";
            if (IsUserCreationOnTileClick)
            {
                samlAttributeDetails = rpsaml.createUserBatchIfRequired(personaId, productId);
                if (samlAttributeDetails.Count == 0)
                {
                    return new ProductLoginResponse() { ErrorMessage = "UserCreationFailed" };
                }
            }
            if (DenyEmployeeAccessByADGroup(productId, productInternalSettingsList, out var productLoginResponseDenied)) return productLoginResponseDenied;

            string authenticationType = productInternalSettingsList.FirstOrDefault(a => a.Name.Equals("AuthenticationType", StringComparison.OrdinalIgnoreCase))?.Value;
            switch (authenticationType)
            {
                case "Redirect":
                    productLoginResponse = GetProductRedirectUrl(productId);
                    productLoginResponse.RedirectUrl = string.IsNullOrEmpty(userAccessToken) ? productLoginResponse.RedirectUrl : productLoginResponse.RedirectUrl + "?access_token=" + userAccessToken;
                    break;

                case "SAML":
                    string relayState = productInternalSettingsList.FirstOrDefault(a => a.Name.Equals("Authentication_SAML_RelayState", StringComparison.OrdinalIgnoreCase))?.Value;
                    string fallBackUrl = productInternalSettingsList.FirstOrDefault(a => a.Name.Equals("Authentication_SAML_FallbackUrl", StringComparison.OrdinalIgnoreCase))?.Value;
                    try
                    {
                        productLoginResponse = rpsaml.GetProductDetailsSAML(ConfigReader.GetLandingUri, productId, personaId, accessToken, relayState, fallBackUrl, false, null);
                    }
                    catch (Exception exception)
                    {
                        WriteToLog(LogEventLevel.Error, exception: exception, message: "{ActionName} - {state}", messageProperties: new object[] { "GetProductLoginDetails", $"Error : {exception.Message}" });
                        return new ProductLoginResponse() {ErrorMessage = exception.Message};
                    }
                    break;

                case "OpenIdCustom":
                    string productName = productInternalSettingsList.FirstOrDefault(a => a.Name.Equals("Authentication_OpenId_ProductName", StringComparison.OrdinalIgnoreCase))?.Value;
                    string responseType = productInternalSettingsList.FirstOrDefault(a => a.Name.Equals("Authentication_OpenId_ResponseType", StringComparison.OrdinalIgnoreCase))?.Value;
                    string scopesForAuth = productInternalSettingsList.FirstOrDefault(a => a.Name.Equals("Authentication_OpenId_ScopesForAuth", StringComparison.OrdinalIgnoreCase))?.Value;
                    string responseMode = productInternalSettingsList.FirstOrDefault(a => a.Name.Equals("Authentication_OpenId_ResponseMode", StringComparison.OrdinalIgnoreCase))?.Value;

                    productLoginResponse = BuildLoginToken((ProductEnum) productId, productName, personaId, responseType, scopesForAuth, responseMode);
                    break;

                default:
                    productLoginResponse = GetProductRedirectUrl(productId);
                    break;
            }

            // add activity log
            if (isProductReport)
            {
                AddActivityLog(productId, isEmailLinkActivity: isProductReport);
            }
            else
            {
                AddActivityLog(productId);
            }

            var impersonatorUserLoginOnly = _userLoginRepository.GetUserLoginOnly(_userClaims.ImpersonatedBy);
            var impersonatorUserId = impersonatorUserLoginOnly != null ? impersonatorUserLoginOnly.UserId : 0;
            _productRepository.InsertProductLoginActivitybyUser(productId, personaId, impersonatorUserId);

            return productLoginResponse;
        }


        private bool DenyEmployeeAccessByADGroup(int productId, List<ProductInternalSetting> productInternalSettingsList, out ProductLoginResponse productLoginResponseDenied)
        {
            productLoginResponseDenied = null;

            if (string.IsNullOrEmpty(_userClaims.ImpersonatedByName))
            {
                return false;
            }

            string AdGroupProductSetting = productInternalSettingsList.FirstOrDefault(s => s.Name.Equals("CheckADGroupProductAccess", StringComparison.OrdinalIgnoreCase))?.Value;

            if (!string.IsNullOrEmpty(AdGroupProductSetting) && AdGroupProductSetting.Equals("1", StringComparison.OrdinalIgnoreCase))
            {
                //Get all ADgroups for this product
                var productAccessGroupName = productInternalSettingsList.FirstOrDefault(s => s.Name.Equals("CheckADGroupProductAccessGroupNames", StringComparison.OrdinalIgnoreCase))?.Value;
                var productSettingArray = productAccessGroupName.Split(',');

                var AdGroupsProduct = _manageProduct.GetAdGroupsForProduct(productId);

                if (AdGroupsProduct.Count > 0)
                {
                    var accessibleAdGroupsProduct = new List<AdGroup>();

                    foreach (var group in AdGroupsProduct)
                    {
                        foreach (var restriction in productSettingArray)
                        {
                            if (@group.ADGroupName.Contains(restriction) && !@group.ADGroupName.Contains("Manage_Product_Access"))
                            {
                                accessibleAdGroupsProduct.Add(@group);
                            }
                        }
                    }

                    //If there is at least one AdGroup for this product
                    if (accessibleAdGroupsProduct.Count > 0)
                    {
                        var personaList = _managePersona.ListPersona(_userClaims.ImpersonatedBy);
                        var employeePersona = personaList.FirstOrDefault(p => p.Organization.RealPageId == EmployeeCompanyRealPageId);
                        if (employeePersona == null)
                        {
                            return false;
                        }

                        var ADGroupsForUser = _manageProduct.GetAdGroupsForUser(employeePersona?.PersonaId ?? 0);

                        var productAdGroupIds = accessibleAdGroupsProduct.Select(gp => gp.ADGroupId).ToList();
                        var userAdGroupIds = ADGroupsForUser.Select(gu => gu.ADGroupId).ToList();

                        //If there is not a single ADgroup for the product, that exits for the user, block
                        if ((productAdGroupIds.Intersect(userAdGroupIds)).ToList().Count == 0)
                        {
                            {
                                productLoginResponseDenied = new ProductLoginResponse() { ErrorMessage = "AccessDenied" };
                                return true;
                            }
                        }
                    }
                }
            }
            
            return false;
        }

        private void getUserAccessToken(int productId, List<ProductInternalSetting> productInternalSettingsList, out ProductLoginResponse productLoginResponseMessage)
        {
            productLoginResponseMessage = new ProductLoginResponse();
            long companyId = 0, userId = 0;
            string apiUser = productInternalSettingsList.First(a => a.Name.Equals("APIUserName", StringComparison.OrdinalIgnoreCase)).Value;
            string apiPassword = Encoding.UTF8.GetString(Convert.FromBase64String(productInternalSettingsList.First(a => a.Name.Equals("APIPassword", StringComparison.OrdinalIgnoreCase)).Value));
            var byteArray = Encoding.ASCII.GetBytes($"{apiUser}:{apiPassword}");
            //getting companyid
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                client.DefaultRequestHeaders.Add("clientID", productInternalSettingsList.First(a => a.Name.Equals("ClientId", StringComparison.OrdinalIgnoreCase)).Value);
                string companyEndpoint = productInternalSettingsList.First(a => a.Name.Equals("GetCompanyEndpoint", StringComparison.OrdinalIgnoreCase)).Value;
                string companyURL = string.Format(companyEndpoint, _userClaims.OrganizationRealPageGuid);
                
                var response = client.GetAsync(companyURL).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    List<dynamic> companyResult = JsonConvert.DeserializeObject<List<dynamic>>(jsonContent.ToString().Replace("\r\n", ""));
                    if(companyResult != null && companyResult.Count > 0)
                    {
                        companyId = companyResult[0].id.Value;
                    }
                    else
                    {
                        productLoginResponseMessage.ErrorMessage = "This product is not yet implemented for this company.";    
                    }
                    
                }
                else
                {
                    productLoginResponseMessage.ErrorMessage = "This product is not yet implemented for this company.";
                }

                //Getting User Id information
                string userEndpoint = productInternalSettingsList.First(a => a.Name.Equals("GetUserEndpoint", StringComparison.OrdinalIgnoreCase)).Value;
                string userURL = string.Format(userEndpoint, _userClaims.LoginName, companyId);

                var result = client.GetAsync(userURL).Result;

                if (result.IsSuccessStatusCode)
                {
                    var jsonContent = result.Content.ReadAsStringAsync().Result;
                    List<dynamic> userResult = JsonConvert.DeserializeObject<List<dynamic>>(jsonContent.ToString().Replace("\r\n", ""));
                    if(userResult != null && userResult.Count > 0)
                    {
                        userId = userResult[0].id.Value;
                    }
                    else
                    {
                        productLoginResponseMessage.ErrorMessage = "This user is not yet implemented for this product.";
                    }
                    
                }
                else
                {
                    productLoginResponseMessage.ErrorMessage = "This user is not yet implemented for this product.";
                }
            }
            
            //Getting user accesstoken
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                string apiKey = productInternalSettingsList.First(a => a.Name.Equals("APIKey", StringComparison.OrdinalIgnoreCase)).Value;
                string tokenEndPoint = productInternalSettingsList.First(a => a.Name.Equals("TokenEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("DdApi", apiKey);
                string request = string.Format(tokenEndPoint, userId);
                
                var response = client.GetAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;

                    JObject jsonResponse = JObject.Parse(jsonContent);
                    if (jsonResponse != null)
                    {
                        JToken jsonTokenObject = jsonResponse.SelectToken("userKey");
                        productLoginResponseMessage.AccessToken = jsonTokenObject.ToString();
                    }
                }
                else
                {
                    productLoginResponseMessage.ErrorMessage = "This user is not yet implemented for this product.";
                }
            }
        }
        [Route("product/{productCode}/persona/{personaId}")]
        [Authorize]
        [HttpGet]
        public ProductLoginResponse GetProductLoginDetailsFromProductCode(string productCode, long personaId)
        {
            try
            {
                var productList = _productRepository.GetAllProducts();
                int productEnum = ProductEnumHelper.GetProductIdByProductCode(productCode, productList);
                return GetProductLoginDetails(productEnum, personaId);
            }
            catch (Exception ex)
            {
                return new ProductLoginResponse() { ErrorMessage = ex.Message };
            }            
        }

        

        #endregion

        /// <summary>
        /// Used to log a user into an OAuth client
        /// </summary>
        /// <param name="product"></param>
        /// <param name="productName"></param>
        /// <param name="personaId"></param>
        /// <param name="responseType"></param>
        /// <param name="scopesForAuth"></param>
        /// <param name="responseMode"></param>
        /// <param name="usertoken"></param>
        /// <returns></returns>
        private ProductLoginResponse BuildLoginToken(ProductEnum product, string productName, long personaId, string responseType, string scopesForAuth, string responseMode)
        {
            ProductLoginResponse response = new ProductLoginResponse();

            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;

            Persona persona = GetPersona(_userClaims.UserRealPageGuid, personaId);

            if (persona == null)
            {
                throw new Exception("Error getting product details, no persona found");
            }

            if (RealPageSAML.ProductDetails((int) product, persona, out var getOneSitePMCURL, out var getDocMgtDomain, out var getMarketingCenterUrl, out var productList))
            {
                throw new Exception("Error getting product details");
            }

            // only check status for products that have a status
            if (productList?.Count > 0)
            {
                PersonaProductUserDetails productDetail = productList[0];

                if (productDetail.ProductStatus != (int) ProductBatchStatusType.Success)
                {
                    response.ErrorMessage = "AccessDenied";
                    return response;
                }
            }

            // get the product details
            var idp = (from nvp in currentClaimPrincipal.Claims where nvp.Type.Equals("idp", StringComparison.OrdinalIgnoreCase) || nvp.Type.Equals("http://schemas.microsoft.com/identity/claims/identityprovider", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault();

            if (string.IsNullOrEmpty(idp))
                throw new Exception("No idp included in redirect querystring!!");

            var state = Guid.NewGuid().ToString("N");
            var nonce = Guid.NewGuid().ToString("N");
            var client = new OAuth2Client(new Uri(ConfigReader.GetIssuerUri + "/connect/authorize"));

            // get the product Login url
            var productRedirectUrl = GetProductRedirectUrl((int) product);

            string loginUri = productRedirectUrl.RedirectUrl;



            response.IsRedirect = true;
            response.RedirectUrl = client.CreateAuthorizeUrl(productName, responseType, scopesForAuth,
                loginUri, state, nonce, acrValues: $"idp:{idp}", responseMode: responseMode);
            return response;
        }

        /// <summary>
        /// Used to get the persona for the given RealPage user
        /// </summary>
        /// <param name="realPageId">The id of the person</param>
        /// <param name="personaId">The personaid for the person</param>
        /// <returns>Persona object</returns>
        private Persona GetPersona(Guid realPageId, long personaId)
        {
            Persona persona = new Persona();
            ManagePersona personaManager = new ManagePersona();

            if (personaId == 0)
            {
                // get the current users default persona
                try
                {
                    //persona = personaManager.GetActivePersona(realPageId);
                    personaId = _userClaims.PersonaId;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            else
            {
                try
                {
                    // verify the persona belongs to the caller
                    persona = personaManager.GetPersona(personaId);
                    bool hasImpersonate = _userClaims.Rights.Any(p => p.Equals("AccessToUnifiedPlatform", StringComparison.OrdinalIgnoreCase));
                    if (persona == null || (persona.RealPageId != realPageId && !hasImpersonate))
                    {
                        throw new Exception("Invalid persona");
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

            return persona;
        }

        private ProductLoginResponse GetProductRedirectUrl(int productId)
        {
            ProductLoginResponse productLoginResponse = new ProductLoginResponse();

            if (productId == (int)ProductEnum.CIMPL)
            {
                if (CheckForCIMPLViewOnlyAccess())
                {
                    productLoginResponse.ErrorMessage = "ReadOnly";
                    return productLoginResponse;
                }
            }
            else if (productId == (int)ProductEnum.MigrationTool)
            {
                if (CheckForMigrationToolAccess())
                {
                    productLoginResponse.ErrorMessage = "ReadOnly";
                    return productLoginResponse;
                }
            }
            else if (CheckForViewOnlyAccess() && !(productId == (int)ProductEnum.ResearchApplication || productId == (int)ProductEnum.ProductUpdates || productId == (int)ProductEnum.ProductUpdatesDashboard || productId == (int)ProductEnum.HelpCenter
                     || productId ==(int)ProductEnum.VendorMarketplace || productId ==(int)ProductEnum.ProductLearningPortal || productId == (int)ProductEnum.HandsOnTrainingSystem
                     || productId == (int)ProductEnum.LRConversionPortal || productId == (int)ProductEnum.ESupply || productId == (int)ProductEnum.ManagedServices || productId == (int)ProductEnum.TrustDashboard))
            {
                productLoginResponse.ErrorMessage = "ReadOnly";
                return productLoginResponse;
            }


            // get the SAML settings for the given product
            var productSamlSettings = new ProductSamlSettings();
            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = $"productSamlSettings_{productId}";
            productSamlSettings = rpcache.GetFromCache<ProductSamlSettings>(cacheKey, 600, () =>
            {
                // load from api
                return _samlRepository.GetProductSamlSettingsByProductId(productId);
            });

            string loginUri = productSamlSettings.LoginUri; 
            if (productId == (int)ProductEnum.VendorMarketplace && _userClaims.RealPageEmployee)
            {
                var productInternalSetting = _manageProduct.GetProductInternalSettings(productId);
                loginUri = productInternalSetting.First(a => a.Name.Equals("AlternateLoginURL", StringComparison.OrdinalIgnoreCase)).Value;                
            }

            productLoginResponse.IsRedirect = true;
            productLoginResponse.RedirectUrl = loginUri;


            return productLoginResponse;
        }

        private bool CheckForCIMPLViewOnlyAccess()
        {
            return (_userClaims.Rights.All(p => !p.Equals("ViewCIMPLQuestions", StringComparison.OrdinalIgnoreCase) &&
                                                !p.Equals("EmployeeViewCIMPLQuestions", StringComparison.OrdinalIgnoreCase)));
        }

        private bool CheckForMigrationToolAccess()
        {
            if(_userClaims.Rights.Any(p => p == "ViewOnlySupportToolAccess"))
            {
                List<string> impersonatedUserRights = BaseUserRights.GetImpersonatedUserRights(_userClaims.ImpersonatedBy, _userClaims);
                return !impersonatedUserRights.Any(p => p.Equals("MigrationTool", StringComparison.OrdinalIgnoreCase));
            }
            else if(_userClaims.Rights.Any(p => p == "AccessToUnifiedPlatform") || _userClaims.Rights.Any(p => p == "MigrationTool"))
            {
                return false;
            }
            return true;
        }

        private bool CheckForViewOnlyAccess()
        {
            return (_userClaims.Rights.Any(p => p.Equals("ViewOnlySupportToolAccess", StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Add Activity Log
        /// </summary>
        /// <param name="productId">productId</param>
        /// <param name="message">message</param>
        /// <param name="isEmailLinkActivity">Indicates if activity because user accessed link from email (e.g. report) </param>
        private void AddActivityLog(int productId, string message = "", bool isEmailLinkActivity = false)
        {
            try
            {
                GbProductMap booksProductDetail = GetBooksMasterProductDetail(_userClaims, productId);

                var logActivityTypeName = "Product Access";

                if (isEmailLinkActivity)
                {
                    logActivityTypeName = "Product Activity";
                    message = $"{_userClaims.FirstName} {_userClaims.LastName} opened {booksProductDetail.Name} Scheduled Reports from an email link..";
                }

                if (string.IsNullOrEmpty(message))
                {
                    if (string.IsNullOrEmpty(_userClaims.ImpersonatedByName))
                    {
                        message = $"User {_userClaims.FirstName} {_userClaims.LastName} accessed product {booksProductDetail.Name}.";
                    }
                    else
                    {
                        message = $"RealPage user {_userClaims.ImpersonatedByName} accessed product {booksProductDetail.Name}.";
                    }
                }

                LogActivity.WriteActivity(new ActivityDetails
                {
                    LogActivityTypeName = logActivityTypeName,
                    LogCategoryName = LogActivityCategoryType.ProductAccess.ToString(),
                    CorrelationId = _userClaims.CorrelationId.ToString(),
                    BooksMasterOrganizationId = _userClaims.OrganizationMasterId,
                    OrganizationPartyId = _userClaims.OrganizationPartyId,
                    Message = message,

                    FromUserLoginName = _userClaims.LoginName,
                    FromUserLoginId = _userClaims.UserId,
                    FromUserFirstName = _userClaims.FirstName,
                    FromUserLastName = _userClaims.LastName,
                    FromUserRealpageId = _userClaims.UserRealPageGuid.ToString(),

                    BooksProductCode = booksProductDetail.BooksProductCode
                });
            }
            catch
            {
            }
        }

        public GbProductMap GetBooksMasterProductDetail(DefaultUserClaim userClaim, int gbProductId)
        {
            var gbProductMap = GetGbProductMap(userClaim).FirstOrDefault(x => x.ProductId == gbProductId);
            return gbProductMap;
        }

        private IList<GbProductMap> GetGbProductMap(DefaultUserClaim userClaim)
        {
            // Get products
            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = "GB-BB-ProductMap";
            var products = rpcache.GetFromCache<IList<GbProductMap>>(cacheKey, 360, () =>
            {
                return _manageProduct.ListProducts();
            });

            return products;
        }

        #region Output results for documentation

        /// <summary>
        /// Used to document examples of the ProductFamily Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class ProductFamilyMethodExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>ProductFamilyMethodExample example</returns>
            public object GetExamples()
            {
                IList<ProductFamily> productFamilyList = new List<ProductFamily>();
                IList<Solution> solutionList = new List<Solution>()
                {
                    new Solution()
                    {
                        FamilyId = 100,
                        IsAssigned = false,
                        ProductId = 1,
                        ProductName = "OneSite",
                        SubSolution = "OneSite Leasing & Rents, Facilities, Purchasing, Doc. Mgmt"
                    },
                    new Solution()
                    {
                        FamilyId = 100,
                        IsAssigned = false,
                        ProductId = 8,
                        ProductName = "Financial Suite",
                        SubSolution = ""
                    },
                    new Solution()
                    {
                        FamilyId = 100,
                        IsAssigned = false,
                        ProductId = 13,
                        ProductName = "Spend Management",
                        SubSolution = null
                    },
                    new Solution()
                    {
                        FamilyId = 100,
                        IsAssigned = false,
                        ProductId = 16,
                        ProductName = "Vendor Credentialing",
                        SubSolution = "Vendor Compliance"
                    }
                };
                productFamilyList.Add(new ProductFamily()
                {
                    ProductTypeId = 100,
                    Name = "Property Management",
                    Description = "Property Management",
                    Solutions = solutionList
                });

                solutionList = new List<Solution>()
                {
                    new Solution()
                    {
                        FamilyId = 200,
                        IsAssigned = false,
                        ProductId = 15,
                        ProductName = "Renters Insurance",
                        SubSolution = null
                    },
                    new Solution()
                    {
                        FamilyId = 200,
                        IsAssigned = false,
                        ProductId = 17,
                        ProductName = "Active Building",
                        SubSolution = null
                    },
                    new Solution()
                    {
                        FamilyId = 200,
                        IsAssigned = false,
                        ProductId = 18,
                        ProductName = "Utility Management",
                        SubSolution = null
                    }
                };
                productFamilyList.Add(new ProductFamily()
                {
                    ProductTypeId = 200,
                    Name = "Resident Services",
                    Description = "Resident Services",
                    Solutions = solutionList
                });

                solutionList = new List<Solution>()
                {
                    new Solution()
                    {
                        FamilyId = 300,
                        IsAssigned = false,
                        ProductId = 6,
                        ProductName = "Lead2Lease",
                        SubSolution = null
                    },
                    new Solution()
                    {
                        FamilyId = 300,
                        IsAssigned = false,
                        ProductId = 9,
                        ProductName = "Websites & Syndication",
                        SubSolution = null
                    },
                    new Solution()
                    {
                        FamilyId = 300,
                        IsAssigned = false,
                        ProductId = 10,
                        ProductName = "Prospect Contact Center",
                        SubSolution = null
                    }
                };
                productFamilyList.Add(new ProductFamily()
                {
                    ProductTypeId = 300,
                    Name = "Lease Management",
                    Description = "Lease Management",
                    Solutions = solutionList
                });

                solutionList = new List<Solution>()
                {
                    new Solution()
                    {
                        FamilyId = 400,
                        IsAssigned = false,
                        ProductId = 4,
                        ProductName = "Asset Optimization",
                        SubSolution = null
                    },
                    new Solution()
                    {
                        FamilyId = 400,
                        IsAssigned = false,
                        ProductId = 7,
                        ProductName = "YieldStar, LRO",
                        SubSolution = null
                    }
                };
                productFamilyList.Add(new ProductFamily()
                {
                    ProductTypeId = 400,
                    Name = "Asset Optimization",
                    Description = "Asset Optimization",
                    Solutions = solutionList
                });

                Status<IErrorData> errorStatus = new Status<IErrorData>();
                ObjectListOutput<ProductFamily, IErrorData> output = new ObjectListOutput<ProductFamily, IErrorData>() {list = productFamilyList, Status = errorStatus};

                return output;
            }
        }

        /// <summary>
        /// Used to document examples of the ProductType Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class ProductTypeMethodExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>ProductTypeMethodExample example</returns>
            public object GetExamples()
            {
                IList<ProductType> productTypes = new List<ProductType>();
                productTypes.Add(new ProductType
                {
                    Description = "",
                    Name = "Property Management",
                    ProductTypeGuid = Guid.NewGuid(),
                    ProductTypeId = 100,
                    ParentProductTypeId = null
                });

                productTypes.Add(new ProductType
                {
                    Description = null,
                    Name = "Property Management",
                    ProductTypeGuid = Guid.NewGuid(),
                    ProductTypeId = 101,
                    ParentProductTypeId = 100
                });

                productTypes.Add(new ProductType
                {
                    Description = null,
                    Name = "Lease Management",
                    ProductTypeGuid = Guid.NewGuid(),
                    ProductTypeId = 200,
                    ParentProductTypeId = 100
                });

                Status<IErrorData> errorStatus = new Status<IErrorData>();
                ObjectListOutput<ProductType, IErrorData> output = new ObjectListOutput<ProductType, IErrorData>() {list = productTypes, Status = errorStatus};

                return output;
            }
        }

        /// <summary>
        /// Used to document examples of the ProductInternalSettingExample webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class ProductInternalSettingExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>ProductFamilyMethodExample example</returns>
            public object GetExamples()
            {
                var productInternalSettingList = new List<ProductInternalSetting>();
                productInternalSettingList.Add(new ProductInternalSetting
                {
                    Name = "Some setting",
                    Value = "some value"
                });

                productInternalSettingList.Add(new ProductInternalSetting
                {
                    Name = "Another setting",
                    Value = "another value"
                });
                return productInternalSettingList;
            }
        }

        /// <summary>
        /// Used to document examples of the ProductInternalSettingExample webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class ProductInternalSettingByTypeExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>ProductFamilyMethodExample example</returns>
            public object GetExamples()
            {
                var settingList = new List<ProductInternalSettingByType>
                {
                    new ProductInternalSettingByType
                    {
                        ProductConfigurationId = "1234",
                        Name = "ShowInUserDetails",
                        Value = "1",
                        ProductId = 1,
                        ProductName = "OneSite",
                        BooksProductCode = "OS"
                    },
                    new ProductInternalSettingByType
                    {
                        ProductConfigurationId = "1235",
                        Name = "ShowInUserDetails",
                        Value = "0",
                        ProductId = 37,
                        ProductName = "Property Photos",
                        BooksProductCode = "PHOTO"
                    }
                };

                Status<IErrorData> errorStatus = new Status<IErrorData>();
                ObjectOutput<List<ProductInternalSettingByType>, IErrorData> output = new ObjectOutput<List<ProductInternalSettingByType>, IErrorData>()
                {
                    obj = settingList,
                    Status = errorStatus
                };
                return output;
            }
        }

        #endregion
    }
}
