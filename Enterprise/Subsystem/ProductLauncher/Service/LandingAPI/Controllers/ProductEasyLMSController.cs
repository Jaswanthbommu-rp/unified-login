using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
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
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using Serilog;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// EasyLMS Controller to hold (Person, UserLogin, Contact) management related APIs
	/// </summary>
	public class ProductEasyLMSController : BaseApiController
	{
		#region Private variables
		IRepositoryResponse repositoryResponse = new RepositoryResponse();
		private long _companyInstanceSourceId;
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public ProductEasyLMSController() : base() { }
		#endregion

		#region Public Methods
		/// <summary>
		/// Get the Product EasyLMS Url
		/// </summary>
		/// <param name="userName">Optional userLogin - User email address associated with EasyLMS</param>
		/// <param name="createUser"></param>
		/// <returns></returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when PartyRelationship object have invalid entries)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Get a profile for a Person (User)", Type = typeof(IProductEasyLMS))]
		[SwaggerResponseExamples(typeof(IProductEasyLMS), typeof(ProductEasyLMSExample))]
		[Route("products/easylms")]
		[Authorize]
		[HttpGet]
		public HttpResponseMessage ProductEasyLMSlUrl(string userName = "", bool createUser = false)
		{
			bool bUserEnteredUserName = !String.IsNullOrWhiteSpace(userName);
			int SamlUserAttributeId = 0;
			int productId = Convert.ToInt32(ProductEnum.EasyLMS);
			string productEasyLMSUri = string.Empty;
			string productUserName = string.Empty;
			string requestUri = string.Empty;
			string ApiCode = string.Empty;
			string ApiKey = string.Empty;

			ObjectOutput<IProductEasyLMS, IErrorData> output = new ObjectOutput<IProductEasyLMS, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			IProductEasyLMS productEasyLMS = new ProductEasyLMS();
			RepositoryResponse repositoryResponse = new RepositoryResponse();
			SamlAttributes samlAttributes = new SamlAttributes();
			Result resultResponse = new Result();

			IManagePersona personaLogic = new ManagePersona();
			IManageProduct productLogic = new ManageProduct(_userClaims);

			//Get the Unified login UserName (GB UserName) to Update the SamlUserAttribute is exists OR to create SamlUserAttribute
			productUserName = (bUserEnteredUserName == true) ? userName : _loginName;

			ManageProductEasyLMS manageProductEasyLMS = new ManageProductEasyLMS(_userClaims);

			CustomerCompanyMap companyMap = manageProductEasyLMS.GetCompanyAPICodeAndKey(_userClaims.PersonaId, _userClaims.PersonaId);
			if ((companyMap == null) || (Convert.ToInt32(companyMap.CompanyInstanceSourceId) == 0))
			{
				output.obj = productEasyLMS;
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductEasyLMS.PELMSUrl.1";
				errorStatus.ErrorMsg = "There is no active Company Instance record from easyLMS in BlueBook.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			_companyInstanceSourceId = Convert.ToInt32(companyMap.CompanyInstanceSourceId);
			IList<InstanceAttribute> listInstanceAttribute = companyMap.CompanyInstance[0].Attributes;
			bool easyLMSAttributes = ((listInstanceAttribute != null) && (listInstanceAttribute.Count > 0));
			bool existsAPICODE = ((easyLMSAttributes) && (listInstanceAttribute.ToList().Any(a => a.AttributeName.ToUpper() == "API CODE")));
			bool existsAPIKEY = ((easyLMSAttributes) && (listInstanceAttribute.ToList().Any(a => a.AttributeName.ToUpper() == "API KEY")));
			if ((existsAPICODE) && (existsAPIKEY))
			{
				InstanceAttribute instanceAttribute = listInstanceAttribute.ToList().Find(a => a.AttributeName.ToUpper() == "API CODE");
				ApiCode = instanceAttribute.AttributeValue;
				instanceAttribute = listInstanceAttribute.ToList().Find(a => a.AttributeName.ToUpper() == "API KEY");
				ApiKey = instanceAttribute.AttributeValue;
				if ((string.IsNullOrWhiteSpace(ApiCode)) || (string.IsNullOrWhiteSpace(ApiKey)))
				{
					output.obj = productEasyLMS;
					errorStatus.Success = false;
					errorStatus.ErrorCode = "ProductEasyLMS.PELMSUrl.2";
					errorStatus.ErrorMsg = "Invalid ApiCode and/or ApiKey in the active Company Instance record from easyLMS in BlueBook.";
					output.Status = errorStatus;
					return Request.CreateResponse(HttpStatusCode.OK, output);
				}
			}
			else
			{
				output.obj = productEasyLMS;
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductEasyLMS.PELMSUrl.3";
				errorStatus.ErrorMsg = "Invalid ApiCode and/or ApiKey attributes in the active Company Instance record from easyLMS in BlueBook.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			//Get the Product Internal Setting for EasyLMS
			var productInternalSettingList = productLogic.GetProductInternalSettings(productId);
			if (productInternalSettingList.Count == 0)
			{
				output.obj = productEasyLMS;
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductEasyLMS.PELMSUrl.4";
				errorStatus.ErrorMsg = "Product EasyLMS Error: Required Internal settings are missing.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			string productUrl = productInternalSettingList.First(item => item.Name.ToUpper() == "PRODUCTURL").Value;
			//Check if the Product Internal Setting for EasyLMS are valid in the current environment
			if (String.IsNullOrWhiteSpace(productUrl))
			{
				output.obj = productEasyLMS;
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductEasyLMS.PELMSUrl.5";
				errorStatus.ErrorMsg = "Product EasyLMS Error: Invalid Internal settings.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			IManageSaml samlLogic = new ManageSaml();
			//Get the User email associated with EasyLMS
			ISamlAttributes samlAttribute = new SamlAttributes();
			IList<SamlAttributes> samlAttributesList = samlLogic.GetProductSamlDetails(_userClaims.PersonaId, productId);
			//Did you previously create an account on EasyLMS? Yes
			if (samlAttributesList.Count > 0)
			{
				//A EasyLMS UserName Exists
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

			productEasyLMSUri = string.Format("{0}?apicode={1}&apikey={2}&email={3}&type=json", productUrl, ApiCode, ApiKey, productUserName).Trim();
			if (createUser == true)
			{
				//Add the "task=createuser" query string parameter to the API
				requestUri = productEasyLMSUri + "&task=createuser";
				//Add the account in Product EasyLMS
				resultResponse = GetAsync(requestUri);
				if ((resultResponse.status == 0) && (!resultResponse.statusmsg.Contains("User Already Exists")))
				{
					//Error creating user
					output.obj = productEasyLMS;
					errorStatus.Success = false;
					errorStatus.ErrorCode = "ProductEasyLMS.PELMSUrl.7";
					errorStatus.ErrorMsg = "EasyLMS Error: " + resultResponse.statusmsg;
					output.Status = errorStatus;
					return Request.CreateResponse(HttpStatusCode.OK, output);
				}
			}

			//Add the "task=token" query string parameter to the API
			requestUri = productEasyLMSUri + "&task=token";
			//Create a token with the credential
			resultResponse = GetAsync(requestUri);
			if (resultResponse.status == 1)
			{
				//Get the URL with the token in place.  Token is valid for 60 seconds
				productEasyLMSUri = resultResponse.loginURL;

				//Did you previously create an account on EasyLMS? No
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
					//Add productUsername attribute value in Ident.SamlUserAttribute
					repositoryResponse = samlLogic.CreateSamlUserAttribute(_userClaims.PersonaId, productId, SamlAttributeEnum.productUsername, productUserName);
				}
			}
			else
			{
				//Did you previously create an account on EasyLMS? No
				if ((samlAttributesList.Count == 0) && (createUser == false) && (bUserEnteredUserName == false))
				{
					//return error code to UI to display a popup with the following:
					//Please enter the email address associated with your EasyLMS account
					output.obj = productEasyLMS;
					errorStatus.Success = false;
					errorStatus.ErrorCode = "ProductEasyLMS.PELMSUrl.6";
					errorStatus.ErrorMsg = "EasyLMS Error: User email associated with product Not Found.";
					output.Status = errorStatus;
					return Request.CreateResponse(HttpStatusCode.OK, output);
				}

				//Error no token url returned
				//return error code to UI to display a popup with the following:
				//We did not find an account with that username.  We'll create a new account for you. 
				output.obj = productEasyLMS;
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductEasyLMS.PELMSUrl.8";
				errorStatus.ErrorMsg = "EasyLMS Error: " + resultResponse.statusmsg;
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			//return the URL with the token in place to the UI to open a new tab and log the user in.
			productEasyLMS.Url = productEasyLMSUri + "&realpageid=" + _realpageUserId + "&gbtoken=" + _greenBookAccessToken;
			output.obj = productEasyLMS;
			output.Status = errorStatus;
			string _message = string.Empty;

			if (string.IsNullOrEmpty(_userClaims.ImpersonatedByName))
			{
				_message = $"User {_userClaims.FirstName} {_userClaims.LastName} accessed product {manageProductEasyLMS.getProductName(ProductEnum.EasyLMS)}.";
			}
			else
			{
				_message = $"RealPage user {_userClaims.ImpersonatedByName} accessed product {manageProductEasyLMS.getProductName(ProductEnum.EasyLMS)}.";
			}

			LogActivity.WriteActivity(new ActivityDetails
			{
				LogActivityTypeName = "Product Access",
				LogCategoryName = LogActivityCategoryType.ProductAccess.ToString(),
				CorrelationId = _userClaims.CorrelationId.ToString(),
				BooksMasterOrganizationId = _userClaims.OrganizationMasterId,
                OrganizationPartyId = _userClaims.OrganizationPartyId,
				Message = _message,
				FromUserLoginName = _userClaims.LoginName,
				FromUserLoginId = _userClaims.UserId,
				FromUserFirstName = _userClaims.FirstName,
				FromUserLastName = _userClaims.LastName,
				FromUserRealpageId = _userClaims.UserRealPageGuid.ToString(),
				BooksProductCode = BlueBookProductConstants.EasyLMS
			});

			return Request.CreateResponse(HttpStatusCode.OK, output);
		}

		#endregion

		#region Get Examples
		/// <summary>
		/// Used to document examples of the Detailed product EasyLMS,... Model webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class ProductEasyLMSExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>ProductEasyLMS example</returns>
			public object GetExamples()
			{
				IProductEasyLMS productEasyLMS = new ProductEasyLMS();

				ObjectOutput<IProductEasyLMS, IErrorData> output = new ObjectOutput<IProductEasyLMS, IErrorData>() { obj = productEasyLMS };

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
		#endregion

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
