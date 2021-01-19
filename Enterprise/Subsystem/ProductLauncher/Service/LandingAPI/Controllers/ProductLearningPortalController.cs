using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using Serilog;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// ProductLearningPortal Controller to hold (Person, UserLogin, Contact) management related APIs
	/// </summary>
	public class ProductLearningPortalController : BaseApiController
    {
        #region Private variables
        IRepositoryResponse repositoryResponse = new RepositoryResponse();
        IProductLearningPortal productLearningPortal = new ProductLearningPortal();
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public ProductLearningPortalController() : base() { }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get the Product Learning Portal Url
        /// </summary>
        /// <param name="userName">Optional userLogin - User email address associated with Product Learning Portal</param>
        /// <param name="createUser"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when PartyRelationship object have invalid entries)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get a profile for a Person (User)", Type = typeof(IProductLearningPortal))]
        [SwaggerResponseExamples(typeof(IProductLearningPortal), typeof(ProductLearningPortalExample))]
        [Route("products/learningportal")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage ProductLearningPortalUrl(string userName = "", bool createUser = false)
        {
            bool bUserEnteredUserName = !String.IsNullOrWhiteSpace(userName);
            int SamlUserAttributeId = 0;
            int productId = Convert.ToInt32(ProductEnum.ProductLearningPortal);
            string productLearningPortalUri = "";
            string productUserName = "";
            string requestUri = "";

            ObjectOutput<IProductLearningPortal, IErrorData> output = new ObjectOutput<IProductLearningPortal, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            IProductLearningPortal productLearningPortal = new ProductLearningPortal();
            RepositoryResponse repositoryResponse = new RepositoryResponse();
            SamlAttributes samlAttributes = new SamlAttributes();
            Result resultResponse = new Result();

            IManagePersona personaLogic = new ManagePersona();
            IManageProduct productLogic = new ManageProduct(_userClaims);

            //Get the Unified login UserName (GB UserName) to Update the SamlUserAttribute is exists OR to create SamlUserAttribute
            productUserName = (bUserEnteredUserName == true) ? userName : _loginName;

			//Get the Product Internal Setting for Product Learning Portal
			IList<ProductInternalSetting> productInternalSettingList = productLogic.GetProductInternalSettings(productId);
            if (productInternalSettingList.Count == 0)
            {
                output.obj = productLearningPortal;
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ProductLearningPortal.PLPUrl.1";
                errorStatus.ErrorMsg = "Product Learning Portal Error: Required Internal settings are missing.";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }

            string productUrl = productInternalSettingList.First(item => item.Name.ToUpper() == "PRODUCTURL").Value;
            string ApiCode = productInternalSettingList.First(item => item.Name.ToUpper() == "APICODE").Value;
            string ApiKey = productInternalSettingList.First(item => item.Name.ToUpper() == "APIKEY").Value;
            //Check if the Product Internal Setting for Product Learning Portal are valid in the current environment
            if (String.IsNullOrWhiteSpace(productUrl) || String.IsNullOrWhiteSpace(ApiCode) || String.IsNullOrWhiteSpace(ApiKey))
            {
                output.obj = productLearningPortal;
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ProductLearningPortal.PLPUrl.2";
                errorStatus.ErrorMsg = "Product Learning Portal Error: Invalid Internal settings.";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }

			productLearningPortalUri = string.Format("{0}?apicode={1}&apikey={2}&email={3}&type=json", productUrl, ApiCode, ApiKey, productUserName).Trim();
			requestUri = productLearningPortalUri + "&task=token";
			resultResponse = GetAsync(requestUri);

			IManageSaml samlLogic = new ManageSaml();
            //Get the User email associated with Product Learning Portal
            ISamlAttributes samlAttribute = new SamlAttributes();
            IList<SamlAttributes> samlAttributesList = samlLogic.GetProductSamlDetails(_userClaims.PersonaId, productId);

			if ((samlAttributesList.Count == 0) && (resultResponse.status == 1))
			{
				//Add productUsername attribute value in Ident.SamlUserAttribute
				repositoryResponse = samlLogic.CreateSamlUserAttribute(_userClaims.PersonaId, productId, SamlAttributeEnum.productUsername, productUserName);
				productLearningPortalUri = resultResponse.loginURL;
			}
			else
			{
				//Did you previously create an account on RealPage Product Learning Portal? Yes
				if ((samlAttributesList.Count == 0) && (createUser == false) && (bUserEnteredUserName == false))
				{
					//return error code to UI to display a popup with the following:
					//Please enter the email address associated with your RealPage Product Learning Portal account
					output.obj = productLearningPortal;
					errorStatus.Success = false;
					errorStatus.ErrorCode = "ProductLearningPortal.PLPUrl.3";
					errorStatus.ErrorMsg = "Product Learning Portal Error: User email associated with product Not Found.";
					output.Status = errorStatus;
					return Request.CreateResponse(HttpStatusCode.OK, output);
				}

				//Did you previously create an account on RealPage Product Learning Portal? No
				if (samlAttributesList.Count > 0)
				{
					//A Product Learning Portal UserName Exists
					samlAttribute = samlAttributesList.First(item => item.Name.ToUpper() == "PRODUCTUSERNAME");
					if (samlAttribute != null)
					{
						//Use the SamlAttribute Value if the User did not enter a username and the SamlAttribute Value is not null or empty string
						if ((bUserEnteredUserName == false) && (String.IsNullOrWhiteSpace(samlAttribute.Value) == false))
						{
							productUserName = samlAttribute.Value;
						}
						SamlUserAttributeId = Convert.ToInt32(samlAttribute.SamlUserAttributeId);
					}
				}

				productLearningPortalUri = string.Format("{0}?apicode={1}&apikey={2}&email={3}&type=json", productUrl, ApiCode, ApiKey, productUserName).Trim();
				if (createUser == true)
				{
					//Add the "task=createuser" query string parameter to the API
					requestUri = productLearningPortalUri + "&task=createuser";
					//Add the account in Product Learning Portal
					resultResponse = GetAsync(requestUri);
					if ((resultResponse.status == 0) && (!resultResponse.statusmsg.Contains("User Already Exists")))
					{
						//Error creating user
						output.obj = productLearningPortal;
						errorStatus.Success = false;
						errorStatus.ErrorCode = "ProductLearningPortal.PLPUrl.4";
						errorStatus.ErrorMsg = "Product Learning Portal Error: " + resultResponse.statusmsg;
						output.Status = errorStatus;
						return Request.CreateResponse(HttpStatusCode.OK, output);
					}
				}

				//Add the "task=token" query string parameter to the API
				requestUri = productLearningPortalUri + "&task=token";
				//Create a token with the credential
				resultResponse = GetAsync(requestUri);
				if (resultResponse.status == 1)
				{
					//Get the URL with the token in place.  Token is valid for 60 seconds
					productLearningPortalUri = resultResponse.loginURL;

					//Did you previously create an account on RealPage Product Learning Portal? No
					if (samlAttributesList.Count > 0)
					{
						//Update SamlUserAttribute If the stored Username email is blank and the User entered a UserName
						if ((String.IsNullOrWhiteSpace(samlAttribute.Value) == true) || (samlAttribute.Value != productUserName))
						{
							//Update productUsername attribute value in Ident.SamlUserAttribute
							samlAttributes.SamlUserAttributeId = SamlUserAttributeId;
							samlAttributes.Value = productUserName;
							repositoryResponse = samlLogic.UpdateSamlUserAttribute(samlAttributes);
						}
					}
					else
					{
						//No productUsername data exists
						if ((bUserEnteredUserName == true) || (createUser == true))
						{
							//Add productUsername attribute value in Ident.SamlUserAttribute
							repositoryResponse = samlLogic.CreateSamlUserAttribute(_userClaims.PersonaId, productId, SamlAttributeEnum.productUsername, productUserName);
						}
					}
				}
				else
				{
					//Error no token url returned
					//return error code to UI to display a popup with the following:
					//We did not find an account eith that username.  We'll create a new account for you. 
					output.obj = productLearningPortal;
					errorStatus.Success = false;
					errorStatus.ErrorCode = "ProductLearningPortal.PLPUrl.5";
					errorStatus.ErrorMsg = "Product Learning Portal Error: " + resultResponse.statusmsg;
					output.Status = errorStatus;
					return Request.CreateResponse(HttpStatusCode.OK, output);
				}
			}
			//return the URL with the token in place to the UI to open a new tab and log the user in.
			productLearningPortal.Url = productLearningPortalUri;
			output.obj = productLearningPortal;
			output.Status = errorStatus;

            // add activity log
            AddActivityLog();

            return Request.CreateResponse(HttpStatusCode.OK, output);
		}
		#endregion

        #region Get Examples
        /// <summary>
        /// Used to document examples of the Detailed product learning portal,... Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class ProductLearningPortalExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>ProductLearningPortal example</returns>
            public object GetExamples()
            {
                IProductLearningPortal productLearningPortal = new ProductLearningPortal();

                ObjectOutput<IProductLearningPortal, IErrorData> output = new ObjectOutput<IProductLearningPortal, IErrorData>() { obj = productLearningPortal };

                return output;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Send a GET request to the specified Uri as an asynchronous operation
        /// </summary>
        /// <param name="requestUri">The Uri the request is sent to</param>
        /// <returns>Response Object (TaskToken OR TaskCreateUser)</returns>
        private Result GetAsync(string requestUri)
        {
            Result resultResponse = new Result();

            using (var client = new HttpClient())
            {
                using (var response = client.GetAsync(new Uri(requestUri)))
                {
                    if (response.Result.IsSuccessStatusCode)
                    {
                        string jsonContent = response.Result.Content.ReadAsStringAsync().Result.Replace("-status", "status");
                        JObject jsonResponse = JObject.Parse(jsonContent);
                        if (jsonResponse != null)
                        {
                            //Select the result JToken using JPath expression
                            JToken jsonTokenObject = jsonResponse.SelectToken("*.result");
                            Result jsonResultObject = JsonConvert.DeserializeObject<Result>(jsonTokenObject.ToString());
                            resultResponse = jsonResultObject;
                        }
                    }
                }
            }

            return resultResponse;
        }

        private void AddActivityLog()
        {
            try
            {
                var productRepository = new ProductRepository();
                var booksProductDetail = productRepository.GetBooksMasterProductDetail((int)ProductEnum.ProductLearningPortal);
              
                LogActivity.WriteActivity(new ActivityDetails
                {
                    LogActivityTypeName = "Product Access",
                    LogCategoryName = LogActivityCategoryType.ProductAccess.ToString(),
                    CorrelationId = _userClaims.CorrelationId.ToString(),
                    BooksMasterOrganizationId = _userClaims.OrganizationMasterId,
                    OrganizationPartyId = _userClaims.OrganizationPartyId,
                    Message = $"User {_userClaims.FirstName} {_userClaims.LastName} accessed product {booksProductDetail.Name}.",

                    FromUserLoginName = _userClaims.LoginName,
                    FromUserLoginId = _userClaims.UserId,
                    FromUserFirstName = _userClaims.FirstName,
                    FromUserLastName = _userClaims.LastName,
                    FromUserRealpageId = _userClaims.UserRealPageGuid.ToString(),

                    BooksProductCode = booksProductDetail.BooksProductCode
                });
            }
            catch { }
        }

        #endregion

        /// <summary>
        /// Used to store the response from posting to Product Learing Portal
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
            public string loginURL { get; set; }

            /// <summary>
            /// Result status message
            /// </summary>
            [JsonProperty(PropertyName = "statusmsg")]
            public string statusmsg { get; set; }

            /// <summary>
            /// Result status message
            /// </summary>
            [JsonProperty(PropertyName = "UserID")]
            public long UserID { get; set; }
        }

        #region CreateUser/Token Url Response
        /// <summary>
        /// Root level
        /// </summary>
        public class JsonRootObject
        {
            /// <summary>
            /// Result Status (1 = success)
            /// </summary>
            [JsonProperty(PropertyName = "result")]
            public Result result { get; set; }
        }
        #endregion
    }
}