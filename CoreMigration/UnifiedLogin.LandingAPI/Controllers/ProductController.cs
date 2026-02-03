using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product.SAML;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using static UnifiedLogin.BusinessLogic.Logic.Product.SAML.RealPageSAML;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Controller for product related APIs - Migrated to .NET Core 8.0
    /// Handles complex authentication hub with SAML, OpenID, and redirect authentication flows
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("")]
    public class ProductController : BaseController
    {
        #region Private Fields

        private readonly IManageProduct _manageProduct;
        private readonly IUserLoginRepository _userLoginRepository;
        private readonly ISamlRepository _samlRepository;
        private readonly IProductRepository _productRepository;
        private readonly IManageBlueBook _manageBlueBook;
        private readonly IManagePersona _managePersona;
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        private readonly Guid _emptyGuid = Guid.Empty;
        private readonly string _key = "4AD12A31-680A-476F-863E-26749D2E7DD4";

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor with dependency injection for all required services
        /// </summary>
        public ProductController(
            IManageProduct manageProduct,
            IUserLoginRepository userLoginRepository,
            ISamlRepository samlRepository,
            IProductRepository productRepository,
            IManageBlueBook manageBlueBook,
            IManagePersona managePersona,
            IUserClaimsAccessor userClaimsAccessor,
            IMemoryCache memoryCache,
            IHttpClientFactory httpClientFactory,
            ILogger logger) : base(userClaimsAccessor)
        {
            _manageProduct = manageProduct ?? throw new ArgumentNullException(nameof(manageProduct));
            _userLoginRepository = userLoginRepository ?? throw new ArgumentNullException(nameof(userLoginRepository));
            _samlRepository = samlRepository ?? throw new ArgumentNullException(nameof(samlRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _manageBlueBook = manageBlueBook ?? throw new ArgumentNullException(nameof(manageBlueBook));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// List product user(s) for an organization
        /// </summary>
        /// <param name="productId">Unique ProductId</param>
        /// <param name="companyInstanceId">Unique blueBook CompanyInstanceId</param>
        /// <param name="personaId">Optional Unique PersonaId</param>
        /// <returns>Response with Success Message</returns>
        [HttpGet("products/{productId}/organization/{companyInstanceId}")]
        [ProducesResponseType(typeof(ObjectListOutput<ProductUsers, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListProductUsers(int productId, long companyInstanceId, long personaId = 0)
        {
            ObjectListOutput<ProductUsers, IErrorData> output = new ObjectListOutput<ProductUsers, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            output.Status = errorStatus;

            if ((string.IsNullOrWhiteSpace(_userClaimsAccessor.ClientCode) || _userClaimsAccessor.ClientCode.ToUpper() != "BBA") &&
                (_userClaimsAccessor.UserRealPageGuid == Guid.Empty))
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Product.ListProductUsers.1";
                errorStatus.ErrorMsg = "Product - Invalid ClientCode and Enterprise User Id";
                output.Status = errorStatus;
                return Ok(output);
            }

            if (!Enum.IsDefined(typeof(ProductEnum), productId))
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Product.ListProductUsers.2";
                errorStatus.ErrorMsg = "Product - Invalid parameter Product Id";
                output.Status = errorStatus;
                return Ok(output);
            }

            if ((companyInstanceId < -1) || (companyInstanceId == 0))
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Product.ListProductUsers.3";
                errorStatus.ErrorMsg = "Product - Invalid parameter company Instance Id";
                output.Status = errorStatus;
                return Ok(output);
            }

            if (personaId < 0)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Product.ListProductUsers.4";
                errorStatus.ErrorMsg = "Product - Invalid parameter Persona Id";
                output.Status = errorStatus;
                return Ok(output);
            }

            var listProductUsers = await Task.Run(() =>
                _manageProduct.GetProductUsers(productId, companyInstanceId, personaId));

            if (listProductUsers == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Product.ListProductUsers.5";
                errorStatus.ErrorMsg = "Product - List Product Users: No data";
                output.Status = errorStatus;
                return Ok(output);
            }

            output.list = listProductUsers;
            return Ok(output);
        }

        /// <summary>
        /// Get product families
        /// </summary>
        /// <param name="personRealPageId">The user unique identifier (Optional - Use Logged-in user unique identifier if parameter value is Guid.Empty)</param>
        /// <param name="accessFilter">filter products by area (such as "user details" or "rolesandrights")</param>
        /// <param name="loginName">The login name of the user</param>
        /// <returns>list of product families and products</returns>
        [HttpGet("productfamilies")]
        [ProducesResponseType(typeof(ObjectListOutput<ProductFamily, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProductFamilies(Guid? personRealPageId = null, string accessFilter = null, string loginName = null)
        {
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            ObjectListOutput<ProductFamily, IErrorData> output = new ObjectListOutput<ProductFamily, IErrorData>();

            Guid? userRealPageId = (personRealPageId == Guid.Empty || personRealPageId == null)
                ? _userClaimsAccessor.UserRealPageGuid
                : personRealPageId;

            var productFamilyList = await Task.Run(() =>
                _manageProduct.GetProductFamilies(
                    _userClaimsAccessor.OrganizationRealPageGuid,
                    _userClaimsAccessor.UserRealPageGuid,
                    personRealPageId,
                    accessFilter,
                    loginName));

            if (productFamilyList != null)
            {
                output.list = productFamilyList;
                output.Status = errorStatus;
                return Ok(output);
            }

            errorStatus.Success = false;
            errorStatus.ErrorCode = "Product.GetProductFamilies.1";
            errorStatus.ErrorMsg = "Product Families: No data found.";
            output.Status = errorStatus;
            return Ok(output);
        }

        /// <summary>
        /// Used to get product internal settings (protected by secret key)
        /// </summary>
        /// <param name="ProductId">The id of the product to get the settings for</param>
        /// <param name="Key">A guid needed to retrieve the information. Only the system should get this information, no user should need to call this method.</param>
        /// <returns>List of product internal settings</returns>
        [HttpGet("product/internalsettings")]
        [ProducesResponseType(typeof(List<ProductInternalSetting>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProductInternalSettings(int ProductId, Guid Key)
        {
            var productInternalSettingsList = new List<ProductInternalSetting>();

            if (Key != new Guid(_key))
            {
                return Ok(productInternalSettingsList);
            }

            productInternalSettingsList = await Task.Run(() =>
                _manageProduct.GetProductInternalSettings(ProductId));

            return Ok(productInternalSettingsList);
        }

        /// <summary>
        /// Used to get product non-sensitive settings
        /// </summary>
        /// <param name="productid">The id of the product to get the settings for</param>
        /// <returns>List of non-sensitive product settings</returns>
        [HttpGet("product/{productid:int}/settings")]
        [ProducesResponseType(typeof(List<ProductInternalSetting>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProductNonSensitiveSettings(int productid)
        {
            var settings = await Task.Run(() =>
                _manageProduct.GetProductInternalSettings(productid)?
                    .Where(p => !p.SensitiveData)
                    .OrderBy(p => p.Name)
                    .ToList());

            return Ok(settings);
        }

        /// <summary>
        /// Used to get all non-sensitive product internal settings for the given product type
        /// </summary>
        /// <param name="productSettingType">The id of the product to get the settings for</param>
        /// <param name="orgType">Optional organization type filter</param>
        /// <returns>List of product settings by type</returns>
        [HttpGet("product/{productSettingType}/settings")]
        [ProducesResponseType(typeof(ObjectListOutput<ProductInternalSettingByType, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllProductNonSensitiveSettingsByType(string productSettingType, string orgType = null)
        {
            var listResult = await Task.Run(() =>
                _manageProduct.GetProductSettingByType(productSettingType, orgType)?
                    .Where(p => !p.SensitiveData)
                    .OrderBy(p => p.ProductName)
                    .ToList());

            ObjectListOutput<ProductInternalSettingByType, IErrorData> output = new ObjectListOutput<ProductInternalSettingByType, IErrorData>
            {
                list = listResult,
                Status = new Status<IErrorData>(),
                pagingSummary = new PagingSummary
                {
                    TotalRecords = listResult?.Count ?? 0,
                    TotalPages = 1
                }
            };

            return Ok(output);
        }

        /// <summary>
        /// Used to update a product internal setting
        /// </summary>
        /// <param name="productId">The id of the product to get the settings for</param>
        /// <param name="productInternalSetting">Product internal setting to update</param>
        /// <returns>Update result</returns>
        [HttpPut("product/{productId}/settings")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateProductSettingAndLinkToConfiguration(int productId, [FromBody] ProductInternalSetting productInternalSetting)
        {
            var response = await Task.Run(() =>
                _manageProduct.CreateProductSettingAndLinkToConfiguration(productId, productInternalSetting));

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                return BadRequest(response.ErrorMessage);
            }

            return Ok();
        }

        /// <summary>
        /// Get a list of product setting types
        /// </summary>
        /// <returns>List of product setting types</returns>
        [HttpGet("product/settingtypes")]
        [ProducesResponseType(typeof(IList<ProductSettingType>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListProductSettingType()
        {
            var result = await Task.Run(() => _manageProduct.ListProductSettingType());
            return Ok(result);
        }

        /// <summary>
        /// Used to get product saml login (deprecated)
        /// </summary>
        /// <param name="ProductId">The id of the product to get the settings for</param>
        /// <param name="PersonaId">Persona Id.</param>
        /// <returns>SAML login information</returns>
        [HttpGet("product/login")]
        [ProducesResponseType(typeof(ProductInternalSetting), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetProductLoginInfo(int ProductId, long PersonaId)
        {
            // Deprecated endpoint - returns null
            return Ok(null);
        }

        /// <summary>
        /// Get productTypes
        /// </summary>
        /// <returns>List of product types</returns>
        [HttpGet("productTypes")]
        [ProducesResponseType(typeof(ObjectListOutput<ProductType, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProductTypes()
        {
            ObjectListOutput<ProductType, IErrorData> output = new ObjectListOutput<ProductType, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            var productTypes = await Task.Run(() => _manageProduct.GetProductTypes());

            output.list = productTypes;
            output.Status = errorStatus;
            return Ok(output);
        }

        /// <summary>
        /// List Products
        /// </summary>
        /// <returns>List of GB product maps</returns>
        [HttpGet("booksproductmap")]
        [ProducesResponseType(typeof(IList<GbProductMap>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetBooksProductMap()
        {
            var booksProductMap = await Task.Run(() => _manageProduct.ListProducts());
            return Ok(booksProductMap);
        }

        /// <summary>
        /// Get the list of UDM Sources
        /// </summary>
        /// <returns>List of UDM sources</returns>
        [HttpGet("udmsources")]
        [ProducesResponseType(typeof(IEnumerable<UDMSource>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUDMSourceList()
        {
            var result = await Task.Run(() => _manageBlueBook.GetUDMSourceList());
            return Ok(result);
        }

        /// <summary>
        /// Get the list of operators for the current UPFM company
        /// </summary>
        /// <returns>List of UPFM operators</returns>
        [HttpGet("operators")]
        [ProducesResponseType(typeof(IEnumerable<UPFMOperators>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUDMOperators()
        {
            var result = await Task.Run(() =>
                _manageBlueBook.GetOperatorListForUPFMCompany(_userClaimsAccessor.OrganizationRealPageGuid, "UPFM"));
            return Ok(result);
        }

        /// <summary>
        /// Get product login details - Main authentication hub endpoint
        /// Handles SAML, OpenID, and redirect authentication flows
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="personaId">Persona ID</param>
        /// <returns>Product login response with redirect URL or SAML assertion</returns>
        [HttpGet("product/{productId:int}/persona/{personaId}")]
        [ProducesResponseType(typeof(ProductLoginResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProductLoginDetails(int productId, long personaId)
        {
            ProductLoginResponse productLoginResponse;
            bool isProductReport = false;
            string userAccessToken = string.Empty;

            var currentClaimPrincipal = User as ClaimsPrincipal;
            string accessToken = currentClaimPrincipal?.Claims
                .FirstOrDefault(c => c.Type == "token")?.Value;

            if (personaId == 0)
                personaId = _userClaimsAccessor.PersonaId;

            var userClaim = _userClaimsAccessor.GetUserClaim();
            var rpsaml = new RealPageSAML(userClaim);

            var productInternalSettingsList = await Task.Run(() =>
                _manageProduct.GetProductInternalSettings(productId));

            // Check user organization status
            var userLoginOnly = await Task.Run(() =>
                _userLoginRepository.GetUserLoginOnly(_userClaimsAccessor.UserRealPageGuid));

            var orgStatus = await Task.Run(() =>
                _userLoginRepository.GetUserOrganizationWithStatus(
                    _userClaimsAccessor.UserId,
                    userLoginOnly.LastLogin,
                    _userClaimsAccessor.OrganizationPartyId,
                    false));

            if ((orgStatus.IsActive.HasValue && !orgStatus.IsActive.Value) || orgStatus.IsLocked == true)
            {
                return Ok(new ProductLoginResponse { ErrorMessage = "User not active" });
            }

            // Check if user exists in product
            bool isUserExistsInProductCheckRequired = (productInternalSettingsList?
                .FirstOrDefault(s => s.Name.Equals("isUserExistsInProductCheckRequired", StringComparison.OrdinalIgnoreCase))?.Value) == "1";

            if (isUserExistsInProductCheckRequired)
            {
                var productLoginResponseMessage = await GetUserAccessTokenAsync(productId, productInternalSettingsList);
                if (!string.IsNullOrEmpty(productLoginResponseMessage.ErrorMessage))
                {
                    return Ok(new ProductLoginResponse { ErrorMessage = productLoginResponseMessage.ErrorMessage });
                }
                else
                {
                    userAccessToken = productLoginResponseMessage.AccessToken;
                }
            }

            // Create user batch if required
            bool isUserCreationOnTileClick = (productInternalSettingsList?
                .FirstOrDefault(s => s.Name.Equals("IsUserCreationOnTileClick", StringComparison.OrdinalIgnoreCase))?.Value) == "true";

            if (isUserCreationOnTileClick)
            {
                var samlAttributeDetails = await Task.Run(() =>
                    rpsaml.createUserBatchIfRequired(personaId, productId));

                if (samlAttributeDetails.Count == 0)
                {
                    return Ok(new ProductLoginResponse { ErrorMessage = "UserCreationFailed" });
                }
            }

            // Check AD Group access restrictions
            var (isDenied, productLoginResponseDenied) = await DenyEmployeeAccessByADGroupAsync(productId, productInternalSettingsList);
            if (isDenied)
            {
                return Ok(productLoginResponseDenied);
            }

            // Determine authentication type and build response
            string authenticationType = productInternalSettingsList
                .FirstOrDefault(a => a.Name.Equals("AuthenticationType", StringComparison.OrdinalIgnoreCase))?.Value;

            switch (authenticationType)
            {
                case "Redirect":
                    productLoginResponse = await GetProductRedirectUrlAsync(productId);
                    productLoginResponse.RedirectUrl = string.IsNullOrEmpty(userAccessToken)
                        ? productLoginResponse.RedirectUrl
                        : productLoginResponse.RedirectUrl + "?access_token=" + userAccessToken;
                    break;

                case "SAML":
                    string relayState = productInternalSettingsList
                        .FirstOrDefault(a => a.Name.Equals("Authentication_SAML_RelayState", StringComparison.OrdinalIgnoreCase))?.Value;
                    string fallBackUrl = productInternalSettingsList
                        .FirstOrDefault(a => a.Name.Equals("Authentication_SAML_FallbackUrl", StringComparison.OrdinalIgnoreCase))?.Value;

                    try
                    {
                        productLoginResponse = await Task.Run(() =>
                            rpsaml.GetProductDetailsSAML(
                                ConfigReader.GetLandingUri,
                                productId,
                                personaId,
                                accessToken,
                                relayState,
                                fallBackUrl,
                                false,
                                null));
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "{ActionName} - {state}", "GetProductLoginDetails", $"Error : {exception.Message}");
                        return Ok(new ProductLoginResponse { ErrorMessage = exception.Message });
                    }
                    break;

                case "OpenIdCustom":
                    string productName = productInternalSettingsList
                        .FirstOrDefault(a => a.Name.Equals("Authentication_OpenId_ProductName", StringComparison.OrdinalIgnoreCase))?.Value;
                    string responseType = productInternalSettingsList
                        .FirstOrDefault(a => a.Name.Equals("Authentication_OpenId_ResponseType", StringComparison.OrdinalIgnoreCase))?.Value;
                    string scopesForAuth = productInternalSettingsList
                        .FirstOrDefault(a => a.Name.Equals("Authentication_OpenId_ScopesForAuth", StringComparison.OrdinalIgnoreCase))?.Value;
                    string responseMode = productInternalSettingsList
                        .FirstOrDefault(a => a.Name.Equals("Authentication_OpenId_ResponseMode", StringComparison.OrdinalIgnoreCase))?.Value;

                    productLoginResponse = await BuildLoginTokenAsync(
                        (ProductEnum)productId,
                        productName,
                        personaId,
                        responseType,
                        scopesForAuth,
                        responseMode);
                    break;

                default:
                    productLoginResponse = await GetProductRedirectUrlAsync(productId);
                    break;
            }

            // Add activity log
            if (isProductReport)
            {
                await AddActivityLogAsync(productId, isEmailLinkActivity: isProductReport);
            }
            else
            {
                await AddActivityLogAsync(productId);
            }

            // Insert product login activity
            var impersonatorUserLoginOnly = await Task.Run(() =>
                _userLoginRepository.GetUserLoginOnly(_userClaimsAccessor.ImpersonatedBy));

            var impersonatorUserId = impersonatorUserLoginOnly != null ? impersonatorUserLoginOnly.UserId : 0;

            var sharedProductIdValue = productInternalSettingsList
                .FirstOrDefault(s => s.Name.Equals(SettingConstants.SharedProductSettingName, StringComparison.OrdinalIgnoreCase))?.Value;

            if (!string.IsNullOrWhiteSpace(sharedProductIdValue) && int.TryParse(sharedProductIdValue, out int sharedProductId))
            {
                productId = sharedProductId;
            }

            await Task.Run(() =>
                _productRepository.InsertProductLoginActivitybyUser(productId, personaId, impersonatorUserId));

            return Ok(productLoginResponse);
        }

        /// <summary>
        /// Get product login details from product code
        /// </summary>
        /// <param name="productCode">Product code</param>
        /// <param name="personaId">Persona ID</param>
        /// <returns>Product login response</returns>
        [HttpGet("product/{productCode}/persona/{personaId}")]
        [ProducesResponseType(typeof(ProductLoginResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProductLoginDetailsFromProductCode(string productCode, long personaId)
        {
            try
            {
                var productList = await Task.Run(() => _productRepository.GetAllProducts());
                int productEnum = ProductEnumHelper.GetProductIdByProductCode(productCode, productList);
                return await GetProductLoginDetails(productEnum, personaId);
            }
            catch (Exception ex)
            {
                return Ok(new ProductLoginResponse { ErrorMessage = ex.Message });
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Check and deny employee access based on AD Group validation
        /// </summary>
        private async Task<(bool isDenied, ProductLoginResponse response)> DenyEmployeeAccessByADGroupAsync(
            int productId,
            List<ProductInternalSetting> productInternalSettingsList)
        {
            ProductLoginResponse productLoginResponseDenied = null;

            if (string.IsNullOrEmpty(_userClaimsAccessor.ImpersonatedByName))
            {
                return (false, null);
            }

            string adGroupProductSetting = productInternalSettingsList
                .FirstOrDefault(s => s.Name.Equals("CheckADGroupProductAccess", StringComparison.OrdinalIgnoreCase))?.Value;

            if (!string.IsNullOrEmpty(adGroupProductSetting) &&
                adGroupProductSetting.Equals("1", StringComparison.OrdinalIgnoreCase))
            {
                // Get all AD groups for this product
                var productAccessGroupName = productInternalSettingsList
                    .FirstOrDefault(s => s.Name.Equals("CheckADGroupProductAccessGroupNames", StringComparison.OrdinalIgnoreCase))?.Value;

                var productSettingArray = productAccessGroupName.Split(',');

                var adGroupsProduct = await Task.Run(() => _manageProduct.GetAdGroupsForProduct(productId));

                if (adGroupsProduct.Count > 0)
                {
                    var accessibleAdGroupsProduct = new List<AdGroup>();

                    foreach (var group in adGroupsProduct)
                    {
                        foreach (var restriction in productSettingArray)
                        {
                            if (group.ADGroupName.Contains(restriction) &&
                                !group.ADGroupName.Contains("Manage_Product_Access"))
                            {
                                accessibleAdGroupsProduct.Add(group);
                            }
                        }
                    }

                    // If there is at least one AD Group for this product
                    if (accessibleAdGroupsProduct.Count > 0)
                    {
                        var personaList = await Task.Run(() =>
                            _managePersona.ListPersona(_userClaimsAccessor.ImpersonatedBy));

                        var employeePersona = personaList
                            .FirstOrDefault(p => p.Organization.RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId);

                        if (employeePersona == null)
                        {
                            return (false, null);
                        }

                        var adGroupsForUser = await Task.Run(() =>
                            _manageProduct.GetAdGroupsForUser(employeePersona?.PersonaId ?? 0));

                        var productAdGroupIds = accessibleAdGroupsProduct.Select(gp => gp.ADGroupId).ToList();
                        var userAdGroupIds = adGroupsForUser.Select(gu => gu.ADGroupId).ToList();

                        // If there is not a single AD group for the product that exists for the user, block
                        if ((productAdGroupIds.Intersect(userAdGroupIds)).ToList().Count == 0)
                        {
                            productLoginResponseDenied = new ProductLoginResponse { ErrorMessage = "AccessDenied" };
                            return (true, productLoginResponseDenied);
                        }
                    }
                }
            }

            return (false, null);
        }

        /// <summary>
        /// Get user access token for product validation
        /// </summary>
        private async Task<ProductLoginResponse> GetUserAccessTokenAsync(
            int productId,
            List<ProductInternalSetting> productInternalSettingsList)
        {
            ProductLoginResponse productLoginResponseMessage = new ProductLoginResponse();
            long companyId = 0, userId = 0;

            string apiUser = productInternalSettingsList
                .First(a => a.Name.Equals("APIUserName", StringComparison.OrdinalIgnoreCase)).Value;
            string apiPassword = Encoding.UTF8.GetString(Convert.FromBase64String(
                productInternalSettingsList.First(a => a.Name.Equals("APIPassword", StringComparison.OrdinalIgnoreCase)).Value));

            var byteArray = Encoding.ASCII.GetBytes($"{apiUser}:{apiPassword}");

            // Get company ID
            using (var client = _httpClientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                client.DefaultRequestHeaders.Add("clientID",
                    productInternalSettingsList.First(a => a.Name.Equals("ClientId", StringComparison.OrdinalIgnoreCase)).Value);

                string companyEndpoint = productInternalSettingsList
                    .First(a => a.Name.Equals("GetCompanyEndpoint", StringComparison.OrdinalIgnoreCase)).Value;
                string companyURL = string.Format(companyEndpoint, _userClaimsAccessor.OrganizationRealPageGuid);

                var response = await client.GetAsync(companyURL);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    List<dynamic> companyResult = JsonConvert.DeserializeObject<List<dynamic>>(jsonContent.Replace("\r\n", ""));

                    if (companyResult != null && companyResult.Count > 0)
                    {
                        companyId = companyResult[0].id.Value;
                    }
                    else
                    {
                        productLoginResponseMessage.ErrorMessage = "This product is not yet implemented for this company.";
                        return productLoginResponseMessage;
                    }
                }
                else
                {
                    productLoginResponseMessage.ErrorMessage = "This product is not yet implemented for this company.";
                    return productLoginResponseMessage;
                }

                // Get User ID information
                string userEndpoint = productInternalSettingsList
                    .First(a => a.Name.Equals("GetUserEndpoint", StringComparison.OrdinalIgnoreCase)).Value;
                string userURL = string.Format(userEndpoint, _userClaimsAccessor.LoginName, companyId);

                var result = await client.GetAsync(userURL);

                if (result.IsSuccessStatusCode)
                {
                    var jsonContent = await result.Content.ReadAsStringAsync();
                    List<dynamic> userResult = JsonConvert.DeserializeObject<List<dynamic>>(jsonContent.Replace("\r\n", ""));

                    if (userResult != null && userResult.Count > 0)
                    {
                        userId = userResult[0].id.Value;
                    }
                    else
                    {
                        productLoginResponseMessage.ErrorMessage = "This user is not yet implemented for this product.";
                        return productLoginResponseMessage;
                    }
                }
                else
                {
                    productLoginResponseMessage.ErrorMessage = "This user is not yet implemented for this product.";
                    return productLoginResponseMessage;
                }
            }

            // Get user access token
            using (var client = _httpClientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                string apiKey = productInternalSettingsList
                    .First(a => a.Name.Equals("APIKey", StringComparison.OrdinalIgnoreCase)).Value;
                string tokenEndPoint = productInternalSettingsList
                    .First(a => a.Name.Equals("TokenEndPoint", StringComparison.OrdinalIgnoreCase)).Value;

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("DdApi", apiKey);
                string request = string.Format(tokenEndPoint, userId);

                var response = await client.GetAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
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

            return productLoginResponseMessage;
        }

        /// <summary>
        /// Build login token for OAuth client
        /// </summary>
        private async Task<ProductLoginResponse> BuildLoginTokenAsync(
            ProductEnum product,
            string productName,
            long personaId,
            string responseType,
            string scopesForAuth,
            string responseMode)
        {
            ProductLoginResponse response = new ProductLoginResponse();

            var currentClaimPrincipal = User as ClaimsPrincipal;

            var persona = await GetPersonaAsync(_userClaimsAccessor.UserRealPageGuid, personaId);

            if (persona == null)
            {
                throw new Exception("Error getting product details, no persona found");
            }

            if (RealPageSAML.ProductDetails((int)product, persona, out var getOneSitePMCURL,
                out var getDocMgtDomain, out var getMarketingCenterUrl, out var productList))
            {
                throw new Exception("Error getting product details");
            }

            // Only check status for products that have a status
            if (productList?.Count > 0)
            {
                PersonaProductUserDetails productDetail = productList[0];

                if (productDetail.ProductStatus != (int)ProductBatchStatusType.Success)
                {
                    response.ErrorMessage = "AccessDenied";
                    return response;
                }
            }

            // Get the product details
            var idp = currentClaimPrincipal?.Claims
                .FirstOrDefault(c => c.Type.Equals("idp", StringComparison.OrdinalIgnoreCase) ||
                                   c.Type.Equals("http://schemas.microsoft.com/identity/claims/identityprovider", StringComparison.OrdinalIgnoreCase))?.Value;

            if (string.IsNullOrEmpty(idp))
                throw new Exception("No idp included in redirect querystring!!");

            var state = Guid.NewGuid().ToString("N");
            var nonce = Guid.NewGuid().ToString("N");

            // Get the product Login url
            var productRedirectUrl = await GetProductRedirectUrlAsync((int)product);
            string loginUri = productRedirectUrl.RedirectUrl;

            // Build OAuth2 authorization URL manually
            var authorizeEndpoint = $"{ConfigReader.GetIssuerUri}/connect/authorize";
            var queryParams = new Dictionary<string, string>
            {
                { "client_id", productName },
                { "response_type", responseType },
                { "scope", scopesForAuth },
                { "redirect_uri", loginUri },
                { "state", state },
                { "nonce", nonce },
                { "acr_values", $"idp:{idp}" }
            };

            if (!string.IsNullOrEmpty(responseMode))
            {
                queryParams.Add("response_mode", responseMode);
            }

            var queryString = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            response.IsRedirect = true;
            response.RedirectUrl = $"{authorizeEndpoint}?{queryString}";

            return response;
        }

        /// <summary>
        /// Get persona for the given RealPage user
        /// </summary>
        private async Task<Persona> GetPersonaAsync(Guid realPageId, long personaId)
        {
            Persona persona = new Persona();

            if (personaId == 0)
            {
                try
                {
                    personaId = _userClaimsAccessor.PersonaId;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                try
                {
                    persona = await Task.Run(() => _managePersona.GetPersona(personaId));
                    bool hasImpersonate = _userClaimsAccessor.Rights
                        .Any(p => p.Equals("AccessToUnifiedPlatform", StringComparison.OrdinalIgnoreCase));

                    if (persona == null || (persona.RealPageId != realPageId && !hasImpersonate))
                    {
                        throw new Exception("Invalid persona");
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return persona;
        }

        /// <summary>
        /// Get product redirect URL with caching
        /// </summary>
        private async Task<ProductLoginResponse> GetProductRedirectUrlAsync(int productId)
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
                if (await CheckForMigrationToolAccessAsync())
                {
                    productLoginResponse.ErrorMessage = "ReadOnly";
                    return productLoginResponse;
                }
            }
            else if (CheckForViewOnlyAccess() &&
                     !(productId == (int)ProductEnum.ResearchApplication ||
                       productId == (int)ProductEnum.ProductUpdates ||
                       productId == (int)ProductEnum.ProductUpdatesDashboard ||
                       productId == (int)ProductEnum.HelpCenter ||
                       productId == (int)ProductEnum.VendorMarketplace ||
                       productId == (int)ProductEnum.ProductLearningPortal ||
                       productId == (int)ProductEnum.HandsOnTrainingSystem ||
                       productId == (int)ProductEnum.LRConversionPortal ||
                       productId == (int)ProductEnum.ESupply ||
                       productId == (int)ProductEnum.ManagedServices ||
                       productId == (int)ProductEnum.TrustDashboard))
            {
                productLoginResponse.ErrorMessage = "ReadOnly";
                return productLoginResponse;
            }

            // Get the SAML settings for the given product with caching
            var cacheKey = $"productSamlSettings_{productId}";
            var productSamlSettings = await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await Task.Run(() => _samlRepository.GetProductSamlSettingsByProductId(productId));
            });

            string loginUri = productSamlSettings.LoginUri;

            if (productId == (int)ProductEnum.VendorMarketplace && _userClaimsAccessor.RealPageEmployee)
            {
                var productInternalSetting = await Task.Run(() =>
                    _manageProduct.GetProductInternalSettings(productId));
                loginUri = productInternalSetting
                    .First(a => a.Name.Equals("AlternateLoginURL", StringComparison.OrdinalIgnoreCase)).Value;
            }

            productLoginResponse.IsRedirect = true;
            productLoginResponse.RedirectUrl = loginUri;

            return productLoginResponse;
        }

        /// <summary>
        /// Check for CIMPL view-only access
        /// </summary>
        private bool CheckForCIMPLViewOnlyAccess()
        {
            return _userClaimsAccessor.Rights.All(p =>
                !p.Equals("ViewCIMPLQuestions", StringComparison.OrdinalIgnoreCase) &&
                !p.Equals("EmployeeViewCIMPLQuestions", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Check for migration tool access
        /// </summary>
        private async Task<bool> CheckForMigrationToolAccessAsync()
        {
            if (_userClaimsAccessor.Rights.Any(p => p == "ViewOnlySupportToolAccess"))
            {
                List<string> impersonatedUserRights = await Task.Run(() =>
                    BaseUserRights.GetImpersonatedUserRights(
                        _userClaimsAccessor.ImpersonatedBy,
                        _userClaimsAccessor.GetUserClaim()));

                return !impersonatedUserRights.Any(p => p.Equals("MigrationTool", StringComparison.OrdinalIgnoreCase));
            }
            else if (_userClaimsAccessor.Rights.Any(p => p == "AccessToUnifiedPlatform") ||
                     _userClaimsAccessor.Rights.Any(p => p == "MigrationTool"))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check for view-only access
        /// </summary>
        private bool CheckForViewOnlyAccess()
        {
            return _userClaimsAccessor.Rights.Any(p =>
                p.Equals("ViewOnlySupportToolAccess", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Add activity log for product access
        /// </summary>
        private async Task AddActivityLogAsync(int productId, string message = "", bool isEmailLinkActivity = false)
        {
            try
            {
                var booksProductDetail = await GetBooksMasterProductDetailAsync(productId);

                var logActivityTypeName = "Product Access";

                if (isEmailLinkActivity)
                {
                    logActivityTypeName = "Product Activity";
                    message = $"{_userClaimsAccessor.FirstName} {_userClaimsAccessor.LastName} opened {booksProductDetail.Name} Scheduled Reports from an email link..";
                }

                if (string.IsNullOrEmpty(message))
                {
                    if (string.IsNullOrEmpty(_userClaimsAccessor.ImpersonatedByName))
                    {
                        message = $"User {_userClaimsAccessor.FirstName} {_userClaimsAccessor.LastName} accessed product {booksProductDetail.Name}.";
                    }
                    else
                    {
                        message = $"RealPage user {_userClaimsAccessor.ImpersonatedByName} accessed product {booksProductDetail.Name}.";
                    }
                }

                await Task.Run(() => LogActivity.WriteActivity(new ActivityDetails
                {
                    LogActivityTypeName = logActivityTypeName,
                    LogCategoryName = LogActivityCategoryType.ProductAccess.ToString(),
                    CorrelationId = _userClaimsAccessor.CorrelationId.ToString(),
                    BooksMasterOrganizationId = _userClaimsAccessor.OrganizationMasterId,
                    OrganizationPartyId = _userClaimsAccessor.OrganizationPartyId,
                    Message = message,
                    FromUserLoginName = _userClaimsAccessor.LoginName,
                    FromUserLoginId = _userClaimsAccessor.UserId,
                    FromUserFirstName = _userClaimsAccessor.FirstName,
                    FromUserLastName = _userClaimsAccessor.LastName,
                    FromUserRealpageId = _userClaimsAccessor.UserRealPageGuid.ToString(),
                    BooksProductCode = booksProductDetail.BooksProductCode
                }));
            }
            catch
            {
                // Swallow exceptions in activity logging
            }
        }

        /// <summary>
        /// Get Books Master product detail
        /// </summary>
        private async Task<GbProductMap> GetBooksMasterProductDetailAsync(int gbProductId)
        {
            var gbProductMap = await GetGbProductMapAsync();
            return gbProductMap.FirstOrDefault(x => x.ProductId == gbProductId);
        }

        /// <summary>
        /// Get GB Product Map with caching
        /// </summary>
        private async Task<IList<GbProductMap>> GetGbProductMapAsync()
        {
            var cacheKey = "GB-BB-ProductMap";
            var products = await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);
                return await Task.Run(() => _manageProduct.ListProducts());
            });

            return products;
        }

        #endregion
    }
}
