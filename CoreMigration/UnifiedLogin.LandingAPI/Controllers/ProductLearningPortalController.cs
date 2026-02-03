using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// ProductLearningPortal Controller to hold product learning portal management related APIs
    /// </summary>
    [ApiController]
    [Route("")]
    [Authorize]
    public class ProductLearningPortalController : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProductLearningPortalController(
            IUserClaimsAccessor userClaimsAccessor,
            IHttpClientFactory httpClientFactory) : base(userClaimsAccessor)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        /// <summary>
        /// Get the Product Learning Portal Url
        /// </summary>
        /// <param name="userName">Optional userLogin - User email address associated with Product Learning Portal</param>
        /// <param name="createUser">Flag to create user if not exists</param>
        /// <returns>Product Learning Portal URL with authentication token</returns>
        [HttpGet("products/learningportal")]
        [ProducesResponseType(typeof(ObjectOutput<IProductLearningPortal, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProductLearningPortalUrl(string userName = "", bool createUser = false)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                if (userClaim == null)
                    return Unauthorized();

                bool bUserEnteredUserName = !string.IsNullOrWhiteSpace(userName);
                int samlUserAttributeId = 0;
                int productId = Convert.ToInt32(ProductEnum.ProductLearningPortal);
                string productLearningPortalUri = "";
                string productUserName = "";
                string requestUri = "";

                ObjectOutput<IProductLearningPortal, IErrorData> output = new ObjectOutput<IProductLearningPortal, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                IProductLearningPortal productLearningPortal = new ProductLearningPortal();

                IManagePersona personaLogic = new ManagePersona();
                IManageProduct productLogic = new ManageProduct(userClaim);

                productUserName = bUserEnteredUserName ? userName : userClaim.LoginName;

                List<ProductInternalSetting> productInternalSettingList = productLogic.GetProductInternalSettings(productId);
                if (productInternalSettingList.Count == 0)
                {
                    output.obj = productLearningPortal;
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "ProductLearningPortal.PLPUrl.1";
                    errorStatus.ErrorMsg = "Product Learning Portal Error: Required Internal settings are missing.";
                    output.Status = errorStatus;
                    return Ok(output);
                }

                string productUrl = productInternalSettingList.First(item => item.Name.ToUpper() == "PRODUCTURL").Value;
                string apiCode = productInternalSettingList.First(item => item.Name.ToUpper() == "APICODE").Value;
                string apiKey = productInternalSettingList.First(item => item.Name.ToUpper() == "APIKEY").Value;

                if (string.IsNullOrWhiteSpace(productUrl) || string.IsNullOrWhiteSpace(apiCode) || string.IsNullOrWhiteSpace(apiKey))
                {
                    output.obj = productLearningPortal;
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "ProductLearningPortal.PLPUrl.2";
                    errorStatus.ErrorMsg = "Product Learning Portal Error: Invalid Internal settings.";
                    output.Status = errorStatus;
                    return Ok(output);
                }

                productLearningPortalUri = string.Format("{0}?apicode={1}&apikey={2}&email={3}&type=json", productUrl, apiCode, apiKey, productUserName).Trim();
                requestUri = productLearningPortalUri + "&task=token";
                Result resultResponse = GetAsync(requestUri);

                IManageSaml samlLogic = new ManageSaml();
                ISamlAttributes samlAttribute = new SamlAttributes();
                IList<SamlAttributes> samlAttributesList = samlLogic.GetProductSamlDetails(userClaim.PersonaId, productId);

                if (samlAttributesList.Count == 0 && resultResponse.status == 1)
                {
                    samlLogic.CreateSamlUserAttribute(userClaim.PersonaId, productId, SamlAttributeEnum.productUsername, productUserName);
                    productLearningPortalUri = resultResponse.loginURL;
                }
                else
                {
                    if (samlAttributesList.Count == 0 && !createUser && !bUserEnteredUserName)
                    {
                        output.obj = productLearningPortal;
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "ProductLearningPortal.PLPUrl.3";
                        errorStatus.ErrorMsg = "Product Learning Portal Error: User email associated with product Not Found.";
                        output.Status = errorStatus;
                        return Ok(output);
                    }

                    if (samlAttributesList.Count > 0)
                    {
                        samlAttribute = samlAttributesList.First(item => item.Name.ToUpper() == "PRODUCTUSERNAME");
                        if (samlAttribute != null)
                        {
                            if (!bUserEnteredUserName && !string.IsNullOrWhiteSpace(samlAttribute.Value))
                            {
                                productUserName = samlAttribute.Value;
                            }
                            samlUserAttributeId = Convert.ToInt32(samlAttribute.SamlUserAttributeId);
                        }
                    }

                    productLearningPortalUri = string.Format("{0}?apicode={1}&apikey={2}&email={3}&type=json", productUrl, apiCode, apiKey, productUserName).Trim();

                    if (createUser)
                    {
                        requestUri = productLearningPortalUri + "&task=createuser";
                        resultResponse = GetAsync(requestUri);
                        if (resultResponse.status == 0 && !resultResponse.statusmsg.Contains("User Already Exists"))
                        {
                            output.obj = productLearningPortal;
                            errorStatus.Success = false;
                            errorStatus.ErrorCode = "ProductLearningPortal.PLPUrl.4";
                            errorStatus.ErrorMsg = "Product Learning Portal Error: " + resultResponse.statusmsg;
                            output.Status = errorStatus;
                            return Ok(output);
                        }
                    }

                    requestUri = productLearningPortalUri + "&task=token";
                    resultResponse = GetAsync(requestUri);

                    if (resultResponse.status == 1)
                    {
                        productLearningPortalUri = resultResponse.loginURL;

                        if (samlAttributesList.Count > 0)
                        {
                            if (string.IsNullOrWhiteSpace(samlAttribute.Value) || samlAttribute.Value != productUserName)
                            {
                                SamlAttributes samlAttributes = new SamlAttributes
                                {
                                    SamlUserAttributeId = samlUserAttributeId,
                                    Value = productUserName
                                };
                                samlLogic.UpdateSamlUserAttribute(samlAttributes);
                            }
                        }
                        else
                        {
                            if (bUserEnteredUserName || createUser)
                            {
                                samlLogic.CreateSamlUserAttribute(userClaim.PersonaId, productId, SamlAttributeEnum.productUsername, productUserName);
                            }
                        }
                    }
                    else
                    {
                        output.obj = productLearningPortal;
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "ProductLearningPortal.PLPUrl.5";
                        errorStatus.ErrorMsg = "Product Learning Portal Error: " + resultResponse.statusmsg;
                        output.Status = errorStatus;
                        return Ok(output);
                    }
                }

                productLearningPortal.Url = productLearningPortalUri;
                output.obj = productLearningPortal;
                output.Status = errorStatus;

                AddActivityLog(userClaim, productId);

                return Ok(output);
            });
        }

        /// <summary>
        /// Send a GET request to the specified Uri
        /// </summary>
        private Result GetAsync(string requestUri)
        {
            Result resultResponse = new Result();

            using (var client = _httpClientFactory.CreateClient())
            {
                var response = client.GetAsync(new Uri(requestUri)).Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonContent = response.Content.ReadAsStringAsync().Result.Replace("-status", "status");
                    JObject jsonResponse = JObject.Parse(jsonContent);
                    if (jsonResponse != null)
                    {
                        JToken jsonTokenObject = jsonResponse.SelectToken("*.result");
                        Result jsonResultObject = JsonConvert.DeserializeObject<Result>(jsonTokenObject.ToString());
                        resultResponse = jsonResultObject;
                    }
                }
            }

            return resultResponse;
        }

        /// <summary>
        /// Add activity log for product access
        /// </summary>
        private void AddActivityLog(DefaultUserClaim userClaim, int productId)
        {
            try
            {
                var productRepository = new ProductRepository(userClaim);
                var booksProductDetail = productRepository.GetBooksMasterProductDetail(productId);

                string message = string.IsNullOrEmpty(userClaim.ImpersonatedByName)
                    ? $"User {userClaim.FirstName} {userClaim.LastName} accessed product {booksProductDetail.Name}."
                    : $"RealPage user {userClaim.ImpersonatedByName} accessed product {booksProductDetail.Name}.";

                LogActivity.WriteActivity(new ActivityDetails
                {
                    LogActivityTypeName = "Product Access",
                    LogCategoryName = LogActivityCategoryType.ProductAccess.ToString(),
                    CorrelationId = userClaim.CorrelationId.ToString(),
                    BooksMasterOrganizationId = userClaim.OrganizationMasterId,
                    OrganizationPartyId = userClaim.OrganizationPartyId,
                    Message = message,
                    FromUserLoginName = userClaim.LoginName,
                    FromUserLoginId = userClaim.UserId,
                    FromUserFirstName = userClaim.FirstName,
                    FromUserLastName = userClaim.LastName,
                    FromUserRealpageId = userClaim.UserRealPageGuid.ToString(),
                    BooksProductCode = booksProductDetail.BooksProductCode
                });
            }
            catch { }
        }

        /// <summary>
        /// Used to store the response from posting to Product Learning Portal
        /// </summary>
        public class Result
        {
            /// <summary>
            /// Result Status (1 = success)
            /// </summary>
            [JsonProperty(PropertyName = "status")]
            public int status { get; set; }

            /// <summary>
            /// login Url with token
            /// </summary>
            [JsonProperty(PropertyName = "loginURL")]
            public string loginURL { get; set; } = string.Empty;

            /// <summary>
            /// Result status message
            /// </summary>
            [JsonProperty(PropertyName = "statusmsg")]
            public string statusmsg { get; set; } = string.Empty;

            /// <summary>
            /// User ID from Learning Portal
            /// </summary>
            [JsonProperty(PropertyName = "UserID")]
            public long UserID { get; set; }
        }
    }
}
