using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// EasyLMS Controller to hold product EasyLMS management related APIs
    /// </summary>
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class ProductEasyLMSController : ControllerBase
    {
        private readonly IUserClaimsAccessor _userClaimsAccessor;
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProductEasyLMSController(
            IUserClaimsAccessor userClaimsAccessor,
            IHttpClientFactory httpClientFactory)
        {
            _userClaimsAccessor = userClaimsAccessor;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Get the Product EasyLMS Url
        /// </summary>
        /// <param name="userName">Optional userLogin - User email address associated with EasyLMS</param>
        /// <param name="createUser">Flag to create user if not exists</param>
        /// <returns>Product EasyLMS URL with authentication token</returns>
        [HttpGet("products/easylms")]
        [ProducesResponseType(typeof(ObjectOutput<IProductEasyLMS, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProductEasyLMSUrl(string userName = "", bool createUser = false)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                if (userClaim == null)
                    return Unauthorized();

                bool bUserEnteredUserName = !string.IsNullOrWhiteSpace(userName);
                int samlUserAttributeId = 0;
                int productId = Convert.ToInt32(ProductEnum.EasyLMS);
                string productEasyLMSUri = string.Empty;
                string productUserName = string.Empty;
                string requestUri = string.Empty;
                string apiCode = string.Empty;
                string apiKey = string.Empty;

                ObjectOutput<IProductEasyLMS, IErrorData> output = new ObjectOutput<IProductEasyLMS, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                IProductEasyLMS productEasyLMS = new ProductEasyLMS();

                IManagePersona personaLogic = new ManagePersona();
                IManageProduct productLogic = new ManageProduct(userClaim);

                // Get the Unified login UserName to Update the SamlUserAttribute if exists OR to create SamlUserAttribute
                productUserName = bUserEnteredUserName ? userName : userClaim.LoginName;

                ManageProductEasyLMS manageProductEasyLMS = new ManageProductEasyLMS(userClaim);

                CustomerCompanyMap companyMap = manageProductEasyLMS.GetCompanyAPICodeAndKey(userClaim.PersonaId, userClaim.PersonaId);
                if (companyMap == null || Convert.ToInt32(companyMap.CompanyInstanceSourceId) == 0)
                {
                    output.obj = productEasyLMS;
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "ProductEasyLMS.PELMSUrl.1";
                    errorStatus.ErrorMsg = "There is no active Company Instance record from easyLMS in BlueBook.";
                    output.Status = errorStatus;
                    return Ok(output);
                }

                long companyInstanceSourceId = Convert.ToInt32(companyMap.CompanyInstanceSourceId);
                IList<InstanceAttribute> listInstanceAttribute = companyMap.CompanyInstance[0].Attributes;
                bool easyLMSAttributes = listInstanceAttribute != null && listInstanceAttribute.Count > 0;
                bool existsAPICode = easyLMSAttributes && listInstanceAttribute.ToList().Any(a => a.AttributeName.ToUpper() == "API CODE");
                bool existsAPIKey = easyLMSAttributes && listInstanceAttribute.ToList().Any(a => a.AttributeName.ToUpper() == "API KEY");

                if (existsAPICode && existsAPIKey)
                {
                    InstanceAttribute instanceAttribute = listInstanceAttribute.ToList().Find(a => a.AttributeName.ToUpper() == "API CODE");
                    apiCode = instanceAttribute.AttributeValue;
                    instanceAttribute = listInstanceAttribute.ToList().Find(a => a.AttributeName.ToUpper() == "API KEY");
                    apiKey = instanceAttribute.AttributeValue;

                    if (string.IsNullOrWhiteSpace(apiCode) || string.IsNullOrWhiteSpace(apiKey))
                    {
                        output.obj = productEasyLMS;
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "ProductEasyLMS.PELMSUrl.2";
                        errorStatus.ErrorMsg = "Invalid ApiCode and/or ApiKey in the active Company Instance record from easyLMS in BlueBook.";
                        output.Status = errorStatus;
                        return Ok(output);
                    }
                }
                else
                {
                    output.obj = productEasyLMS;
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "ProductEasyLMS.PELMSUrl.3";
                    errorStatus.ErrorMsg = "Invalid ApiCode and/or ApiKey attributes in the active Company Instance record from easyLMS in BlueBook.";
                    output.Status = errorStatus;
                    return Ok(output);
                }

                // Get the Product Internal Setting for EasyLMS
                var productInternalSettingList = productLogic.GetProductInternalSettings(productId);
                if (productInternalSettingList.Count == 0)
                {
                    output.obj = productEasyLMS;
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "ProductEasyLMS.PELMSUrl.4";
                    errorStatus.ErrorMsg = "Product EasyLMS Error: Required Internal settings are missing.";
                    output.Status = errorStatus;
                    return Ok(output);
                }

                string productUrl = productInternalSettingList.First(item => item.Name.ToUpper() == "PRODUCTURL").Value;
                if (string.IsNullOrWhiteSpace(productUrl))
                {
                    output.obj = productEasyLMS;
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "ProductEasyLMS.PELMSUrl.5";
                    errorStatus.ErrorMsg = "Product EasyLMS Error: Invalid Internal settings.";
                    output.Status = errorStatus;
                    return Ok(output);
                }

                IManageSaml samlLogic = new ManageSaml();
                ISamlAttributes samlAttribute = new SamlAttributes();
                IList<SamlAttributes> samlAttributesList = samlLogic.GetProductSamlDetails(userClaim.PersonaId, productId);

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

                productEasyLMSUri = string.Format("{0}?apicode={1}&apikey={2}&email={3}&type=json", productUrl, apiCode, apiKey, productUserName).Trim();

                if (createUser)
                {
                    requestUri = productEasyLMSUri + "&task=createuser";
                    Result resultResponse = GetAsync(requestUri);
                    if (resultResponse.status == 0 && !resultResponse.statusmsg.Contains("User Already Exists"))
                    {
                        output.obj = productEasyLMS;
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "ProductEasyLMS.PELMSUrl.7";
                        errorStatus.ErrorMsg = "EasyLMS Error: " + resultResponse.statusmsg;
                        output.Status = errorStatus;
                        return Ok(output);
                    }
                }

                requestUri = productEasyLMSUri + "&task=token";
                Result result = GetAsync(requestUri);

                if (result.status == 1)
                {
                    productEasyLMSUri = result.loginURL;

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
                        samlLogic.CreateSamlUserAttribute(userClaim.PersonaId, productId, SamlAttributeEnum.productUsername, productUserName);
                        ProductRepository productRepository = new ProductRepository();
                        productRepository.UpdateProductSettingProductStatus(userClaim.PersonaId, productId, "ProductStatus", (int)ProductBatchStatusType.Success);
                    }
                }
                else
                {
                    if (samlAttributesList.Count == 0 && !createUser && !bUserEnteredUserName)
                    {
                        output.obj = productEasyLMS;
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "ProductEasyLMS.PELMSUrl.6";
                        errorStatus.ErrorMsg = "EasyLMS Error: User email associated with product Not Found.";
                        output.Status = errorStatus;
                        return Ok(output);
                    }

                    output.obj = productEasyLMS;
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "ProductEasyLMS.PELMSUrl.8";
                    errorStatus.ErrorMsg = "EasyLMS Error: " + result.statusmsg;
                    output.Status = errorStatus;
                    return Ok(output);
                }

                productEasyLMS.Url = productEasyLMSUri + "&realpageid=" + userClaim.UserRealPageGuid;
                output.obj = productEasyLMS;
                output.Status = errorStatus;

                string message = string.IsNullOrEmpty(userClaim.ImpersonatedByName)
                    ? $"User {userClaim.FirstName} {userClaim.LastName} accessed product {manageProductEasyLMS.getProductName(ProductEnum.EasyLMS)}."
                    : $"RealPage user {userClaim.ImpersonatedByName} accessed product {manageProductEasyLMS.getProductName(ProductEnum.EasyLMS)}.";

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
                    BooksProductCode = BlueBookProductConstants.EasyLMS
                });

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
        /// Used to store the response from posting to EasyLMS
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
            /// User ID from EasyLMS
            /// </summary>
            [JsonProperty(PropertyName = "UserID")]
            public long UserID { get; set; }
        }
    }
}
