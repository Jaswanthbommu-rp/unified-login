using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Security;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.ThirdParty;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.Security;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Rum;
using Serilog;
using Serilog.Events;
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
using System.Web.UI.WebControls;
using SO = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    /// <summary>
    /// Controller to hold all user management related APIs
    /// </summary>
    public class UserController : BaseApiController
	{
        #region Public Methods
        /// <summary>
        /// Give administrators access to missing products based on a customer company
        /// </summary> 
        /// <param name="organizationRealPageId">Organization enterprise Id</param>
        /// <param name="assignUserPersonaId">Assigned to user PersonaId</param>
        /// <returns>HTTP response message including the status code and data.</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request (when organizationRealPageId is invalid Guid)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Give administrators access to missing products based on a customer company", Type = typeof(ObjectOutput<Guid, IErrorData>))]
		[Route("user/assignproductstoadministrators")]
		[HttpPost]
		public HttpResponseMessage AssignProductsToAdministrators(Guid organizationRealPageId, long assignUserPersonaId = 0)
		{
			IRepositoryResponse repositoryResponse = new RepositoryResponse();
			ObjectOutput<Guid, IErrorData> output = new ObjectOutput<Guid, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			output.obj = organizationRealPageId;

			if ((organizationRealPageId == Guid.Empty) || (organizationRealPageId == null))
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.AssignProductsToAdministrators.1";
				errorStatus.ErrorMsg = "Invalid parameter: organizationRealPageId";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			if (assignUserPersonaId < 0)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.AssignProductsToAdministrators.2";
				errorStatus.ErrorMsg = "Invalid parameter: assignUserPersonaId";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			IManageUser manageUser = new ManageUser(_userClaims);
			repositoryResponse = manageUser.AssignProductsToAdministrators(organizationRealPageId, assignUserPersonaId);

			if (repositoryResponse.Id == 0 || !string.IsNullOrWhiteSpace(repositoryResponse.ErrorMessage))
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.AssignProductsToAdministrators.3";
				errorStatus.ErrorMsg = repositoryResponse.ErrorMessage;
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			output.Status = errorStatus;
			return Request.CreateResponse(HttpStatusCode.OK, output);
		}

		/// <summary>
		/// Get a user Profile detail
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <returns>Profile object</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Profile object have invalid entries)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Get a profile for a Person (User)", Type = typeof(IProfileDetail))]
		[SwaggerResponseExamples(typeof(IProfileDetail), typeof(GetUserProfileExample))]
		[Route("user/{realPageId}")]
		[HttpGet]
		public HttpResponseMessage GetUserProfile(Guid realPageId)
		{
			IManageUser manageUser =  new ManageUser(_userClaims);

			ObjectOutput<IProfileDetail, IErrorData> output = manageUser.GetUserProfile(realPageId, _realpageUserId, _orgPartyId);

			return Request.CreateResponse(HttpStatusCode.OK, output);
		}

		/// <summary>
		/// Get a users product detail
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
			ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
			if (!currentClaimPrincipal.Identity.IsAuthenticated)
			{
				return Request.CreateResponse(HttpStatusCode.Unauthorized);
			}

			ObjectOutput<IProfileDetail, IErrorData> output = new ObjectOutput<IProfileDetail, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			IPerson person = new Person();

			realPageId = (realPageId == Guid.Empty) ? _realpageUserId : realPageId;
			if (realPageId == Guid.Empty)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.GetUserProducts.1";
				errorStatus.ErrorMsg = "Get User Products: Invalid parameter realPageId";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.BadRequest, output);
			}

			IManagePerson personLogic = new ManagePerson();
			person = personLogic.GetPerson(realPageId);

			if (person != null)
			{
                IManagePersona _managePersona = new ManagePersona(_userClaims);
                IManageOrganization _manageOrganization = new ManageOrganization(_userClaims);
                IManageProduct _manageProduct = new ManageProduct(_userClaims);
                UserProductOutputResult productResult = new UserProductOutputResult();

                Persona persona = _managePersona.GetFirstAvailablePersonaByCompany(realPageId, _userClaims.OrganizationPartyId);
                //Verify if same company
				bool isValidOrganization = _manageOrganization.ValidateOrganization(_userClaims.OrganizationMasterId, _userClaims.UserRealPageGuid, persona.Organization.RealPageId);
				if (!isValidOrganization)
				{
					errorStatus.Success = false;
					errorStatus.ErrorCode = "User.GetProfile.3";
					errorStatus.ErrorMsg = "Get User Profile: User exists in a different organization.";
					output.Status = errorStatus;
					return Request.CreateResponse(HttpStatusCode.OK, output);
				}

				IList<PersonaProductUserDetails> products = _manageProduct.GetUserAssignedProductsByPersona(persona);
				IList<PersonaProductUserDetails> resources = _manageProduct.GetUserAssignedProductsByPersona(persona: persona, productSelectType: ProductSelectType.ResourcesOnly, security: null);
                var productIconSettings = _manageProduct.GetProductSettingByType("ProductIcon");

                productResult.Products = ConvertDashboardProductsToRAUL(products, productIconSettings);
				productResult.Resources = ConvertDashboardProductsToRAUL(resources, productIconSettings);

                if (productResult.Resources.Any(m => m.Id == 89))
                {                 
                    IManageUnifiedSettings manageSettings = new ManageUnifiedSettings(_userClaims);
                    var internalSettings = manageSettings.GetUnifiedSettingsCached("security", _orgPartyId);
					var supportPortalTileAccess = internalSettings.FirstOrDefault(a => a.Name == "hidesupportportaltile");
					string settingValue = supportPortalTileAccess == null ? "null" : supportPortalTileAccess.Value;
                    if (supportPortalTileAccess == null || supportPortalTileAccess.Value == "1")
					{
						var adminSupportPortalResource = productResult.Resources.FirstOrDefault(m => m.Id == 89);
						productResult.Resources.Remove(adminSupportPortalResource);
					}

					if(persona.UserTypeId == 404 && productResult.Resources.Any(m => m.Id == 89))
					{
                        IManageContactMechanism contactMechanism = new ManageContactMechanism();
                        IList<CommonAddress> commonAddressList = contactMechanism.ListContactMechanismForPerson(realPageId, "Email Notification");
						CommonAddress ca = commonAddressList.Where(c => c.contactMechanismUsageType != null).FirstOrDefault();
                        if(ca == null || (ca != null && string.IsNullOrEmpty(ca.AddressString)))
                        {
                            var adminSupportPortalResource = productResult.Resources.FirstOrDefault(m => m.Id == 89);
                            productResult.Resources.Remove(adminSupportPortalResource);
                        }
                    }                    
                }

                return Request.CreateResponse(HttpStatusCode.OK, productResult);
			}

			errorStatus.Success = false;
			errorStatus.ErrorCode = "User.GetUserProducts.2";
			errorStatus.ErrorMsg = "Get User Products: No data.";
			output.Status = errorStatus;
			return Request.CreateResponse(HttpStatusCode.OK, output);
		}

        /// <summary>
        /// Used to return the product list of the user to the RAUL UI component
        /// </summary>
        /// <param name="products"></param>
		/// <param name="productIconSettings"></param>
        /// <returns></returns>
        private List<UserProducts> ConvertDashboardProductsToRAUL(IList<PersonaProductUserDetails> products, IList<ProductInternalSettingByType> productIconSettings)
		{
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
                string correlationId = "";
                if (_userClaims != null)
                {
                    correlationId = (_userClaims.CorrelationId != Guid.Empty) ? _userClaims.CorrelationId.ToString() : "";

                }
                var logger = Log.Logger;
                if (logData?.Keys != null)
                {
                    logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
                }
                logger = logger.ForContext("ProductModule", this.GetType());
                logger = logger.ForContext("CorrelationId", correlationId);
                logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
            }
            catch
            {
                /*ignored*/
            }
        }

        /// <summary>
        /// Get a user Profile detail for clone
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <returns>Profile object</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Profile object have invalid entries)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Get a profile for a Person (User)", Type = typeof(IProfileDetail))]
		[SwaggerResponseExamples(typeof(IProfileDetail), typeof(GetUserProfileExample))]
		[Route("userclone/{realPageId}")]
		[AuthorizeRight("cloneuser")]
		[HttpGet]
		public HttpResponseMessage GetUserProfileForClone(Guid realPageId)
		{
			ObjectOutput<IProfileDetail, IErrorData> output = new ObjectOutput<IProfileDetail, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			IPerson person = new Person();

			realPageId = (realPageId == Guid.Empty) ? _realpageUserId : realPageId;
			if (realPageId == Guid.Empty)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.GetProfile.1";
				errorStatus.ErrorMsg = "Get User Profile: Invalid parameter realPageId";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			IManagePerson personLogic = new ManagePerson();
			person = personLogic.GetPerson(realPageId);

			if (person != null)
			{
				//Include the UserLogin details.  IsActive and Is3rdPartyIDP are used by the Edit User
				IManageUserLogin userLoginLogic = new ManageUserLogin();
				UserLogin userLogin = userLoginLogic.GetUserLogin(realPageId, _orgPartyId); // keep for now, used by ui, need to investigate how

				IProfileDetail profileDetail = new ProfileDetail();
				profileDetail.userLogin.Is3rdPartyIDP = userLogin.Is3rdPartyIDP;
				profileDetail.userLogin.IsActive = userLogin.IsActive;
				profileDetail.userLogin.FromDate = DateTime.UtcNow;
				profileDetail.NotificationEmail = null;
                
                if (profileDetail.TelecommunicationNumber.Count == 0)
                {
                    ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType();
                    TelecommunicationNumber teleCommunicationNumber = new TelecommunicationNumber();
                    teleCommunicationNumber.contactMechanismUsageType = contactMechanismUsageType;
                    profileDetail.TelecommunicationNumber.Add(teleCommunicationNumber);
                }

                IManagePersona managePersona = new ManagePersona();
				//Active Persona is linked to one organization
				Persona persona = managePersona.GetFirstAvailablePersonaByCompany(realPageId, _userClaims.OrganizationPartyId);
				if (persona != null)
				{
					IManagePartyRelationship managePartyRelationship = new ManagePartyRelationship();
					PartyRelationship partyRelationship = managePartyRelationship.GetPartyRelationship(realPageId, persona.Organization.RealPageId, "", "", "User Type");
					if (partyRelationship != null)
					{
						profileDetail.UserTypeId = partyRelationship.RoleTypeIdFrom;
					}

					Persona clonePersona = new Persona
					{
						PersonaId = persona.PersonaId,
						PersonaEnvironmentTypeId = persona.PersonaEnvironmentTypeId,
						PersonaTypeId = persona.PersonaTypeId,
						Name = persona.Name,
						RealPageId = persona.RealPageId
                    };
                    profileDetail.Persona.Add(clonePersona);
                }

                if (FeatureFlag.GetUserCompanyAssociationFeatureFlag())
                {
                    IUserRepository _userRepository = new UserRepository(_userClaims);
                    IManageUserLoginPersona manageUserLoginPersona = new ManageUserLoginPersona(_userClaims);
                    IList<UserLoginPersona> userLoginPersonaList = manageUserLoginPersona.ListUserLoginPersona(userLoginPersonaId: null, userLoginId: userLogin.UserId, organizationPartyId: _userClaims.OrganizationPartyId);
                    var data = _userRepository.GetExternalUserRelationship(userLoginPersonaList[0].UserLoginPersonaId);
                    profileDetail.ExternalUserRelationship = data == null ? new ExternalUserRelationship()
                    {
                        UserLoginPersonaId = userLoginPersonaList[0].UserLoginPersonaId
                    } : data;
                }
                if (profileDetail != null)
				{
					output.obj = profileDetail;
					output.Status = errorStatus;
					return Request.CreateResponse(HttpStatusCode.OK, output);
				}
			}

			errorStatus.Success = false;
			errorStatus.ErrorCode = "User.GetProfile.2";
			errorStatus.ErrorMsg = "Get User Profile: No data.";
			output.Status = errorStatus;
			return Request.CreateResponse(HttpStatusCode.OK, output);
		}

        /// <summary>
        /// Update User Detail and Products
        /// </summary>
        /// <param name="profile">Edited User detail and Products</param>
        /// <returns>Response with Success Message</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when User object have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "User Updated", Type = typeof(IProfileDetail))]
        [SwaggerResponseExamples(typeof(IProfileDetail), typeof(UpdateUserOutputResultExample))]
        [Route("user")]
        [AuthorizeRight("editusers", "editotherprofile", "editownprofile")]
        [HttpPut]
        public HttpResponseMessage UpdateUser([FromBody]ProfileDetail profile)
        {
            IRepositoryResponse repositoryResponse = new RepositoryResponse();
            ObjectOutput<IProfileDetail, IErrorData> output = new ObjectOutput<IProfileDetail, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            output.obj = profile;

			IManageUserLogin manageUserLogin = new ManageUserLogin(_userClaims);
			IUserLoginOnly userLoginOnly = manageUserLogin.GetUserLoginOnly(profile.RealPageId);

			IManageRoleType manageRoleType = new ManageRoleType();
			long bookCustomerMasterId = profile.organization.Select(o => o.BooksCustomerMasterId).FirstOrDefault();
			IList<RoleType> userRoles = manageRoleType.GetRoleTypeDependency(roleTypeId: profile.UserTypeId, partyId: _userClaims.OrganizationPartyId, orgMasterId: bookCustomerMasterId, loginName: userLoginOnly.LoginName);
            if (userRoles == null)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.UpdateUser.3";
				errorStatus.ErrorMsg = "User roles are missing.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			if (profile == null)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.UpdateUser.1";
				errorStatus.ErrorMsg = "Profile is required.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			else if (profile.IsFirstNameNullOrWhiteSpace)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.UpdateUser.10";
				errorStatus.ErrorMsg = "First name is required.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			else if (profile.IsLastNameNullOrWhiteSpace)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.UpdateUser.11";
				errorStatus.ErrorMsg = "Last name is required.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			else if (profile.userLogin.IsLoginNameNullOrWhiteSpace)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.UpdateUser.4";
				errorStatus.ErrorMsg = "Username is required.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			else if (!userRoles.Any(r => r.PartyRoleTypeId == profile.UserTypeId))
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.UpdateUser.5";
				errorStatus.ErrorMsg = "Invalid user type.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			else if (!String.IsNullOrWhiteSpace(profile.NotificationEmail) && !EmailFormatValidation.IsValidEmail(profile.NotificationEmail))
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.UpdateUser.6";
				errorStatus.ErrorMsg = "Notification email is not a valid email address.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			else if (profile.UserTypeId == (int) UserRoleType.User &&
			         !EmailFormatValidation.IsValidEmail(profile.userLogin.LoginName))
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.UpdateUser.7";
				errorStatus.ErrorMsg = "Username is not a valid email address.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			else if (profile.userLogin.FromDate.HasValue == false)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.UpdateUser.8";
				errorStatus.ErrorMsg = "Effective date is required.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			ProductBatch resPortal = profile.productBatch.FirstOrDefault<ProductBatch>((Func<ProductBatch, bool>)(p => p.ProductId == (int)ProductEnum.ResidentPortal));
            if (resPortal != null)
            {
				//verify resident portal user has same or higher access level
				IManageProductResidentPortal manageResidentPortal = new ManageProductResidentPortal(_userClaims);
				bool hasAccess = manageResidentPortal.ValidateUserAccess(_realpageUserId, profile.Persona[0].PersonaId);
				if (!hasAccess)
				{
					errorStatus.Success = false;
					errorStatus.ErrorCode = "User.UpdateUser.13";
					errorStatus.ErrorMsg = "Validate Resident Portal User Access: You do not have the permissions to edit this user's role.";
					output.Status = errorStatus;
					return Request.CreateResponse(HttpStatusCode.OK, output);
				}
			}

			bool isValidUsername = manageUserLogin.ValidateUsername(profile.RealPageId, profile.userLogin.LoginName);
			if (!isValidUsername)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.UpdateUser.2";
				errorStatus.ErrorMsg = "Update User: User already exists";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			IManageOrganization manageOrganization = new ManageOrganization(_userClaims);
			bool isValidOrganization = manageOrganization.ValidateOrganization(_userClaims.OrganizationMasterId, _userClaims.UserRealPageGuid, profile.Persona[0].Organization.RealPageId);
			if (!isValidOrganization)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.UpdateUser.12";
				errorStatus.ErrorMsg = "Update User: User exists in a different organization.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			IManageUser manageUser = new ManageUser(_userClaims);
			string invalidProducts = "";
	        List<int> invalidProductList = new List<int>();
	        foreach (var product in profile.productBatch)
	        {
		        if (!manageUser.CheckProductRight(product))
		        {
			        invalidProductList.Add(product.ProductId);
				}
	        }

	        if (invalidProductList.Count > 0)
	        {
		        var manageProduct = new ManageProduct(_userClaims);
		        IList<ProductUI> productList = manageProduct.GetProducts(profile.Persona[0].Organization.RealPageId, 0, true);
		        foreach (int productId in invalidProductList)
		        {
			        if (!string.IsNullOrWhiteSpace(invalidProducts))
			        {
				        invalidProducts += ", ";
			        }

			        if (productList.Any(p => p.ProductId == productId))
			        {
				        invalidProducts += productList.FirstOrDefault(p => p.ProductId == productId).ProductName;
			        }
			        else
			        {
				        invalidProducts += ((ProductEnum)productId).ToString();
					}
		        }

		        errorStatus.Success = false;
		        errorStatus.ErrorCode = "User.UpdateUser.15";
		        errorStatus.ErrorMsg = $"Update User: you do not have access to edit {invalidProducts} product access";
		        output.Status = errorStatus;
		        return Request.CreateResponse(HttpStatusCode.OK, output);
	        }

            bool IsValidDomainUsername = manageUserLogin.IsUserEmailDomainValid(profile.userLogin.LoginName);
            if (!IsValidDomainUsername)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "User.CreateUser";
                errorStatus.ErrorMsg = "Email domain is not allowed.";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }

            //Pass the _realpageUserId: User unique enterpriseId
            repositoryResponse = manageUser.UpdateUser(_realpageUserId, profile);
			if ((repositoryResponse.Id == 0) && (!string.IsNullOrWhiteSpace(repositoryResponse.ErrorMessage)))
			{
				output.obj = profile;
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.UpdateUser.9";
				errorStatus.ErrorMsg = repositoryResponse.ErrorMessage;
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			output.Status = errorStatus;
			return Request.CreateResponse(HttpStatusCode.OK, output);
		}

		/// <summary>
		/// Validate New User
		/// </summary> 
		/// <param name="enterpriseUserName">Enterprise UserName</param>
		/// <param name="newUserRegistrationToken">new User Registration Token</param>
		/// <returns>ValidateUserResponse object</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[Route("user/validate")]
		[HttpGet]
		[AllowAnonymous]
		public ValidateUserResponse Validate(string enterpriseUserName, string newUserRegistrationToken)
		{
			if (string.IsNullOrEmpty(enterpriseUserName.Trim()))
			{
				throw new HttpResponseException(HttpStatusCode.BadRequest);
			}

			if (string.IsNullOrEmpty(newUserRegistrationToken.Trim()))
			{
				throw new HttpResponseException(HttpStatusCode.BadRequest);
			}

			var manageUser = new ManageUser(_userClaims);
			return manageUser.ValidateUser(enterpriseUserName.Trim(), newUserRegistrationToken);
		}

		/// <summary>
		/// Validate registration verification token is associated with user name
		/// </summary>
		/// <param name="enterpriseUserName">Enterprise UserName</param>
		/// <param name="verificationToken">verification Token</param>
		/// <returns>ValidateUserResponse object</returns>
		[SwaggerResponse(HttpStatusCode.OK, Description = "Validates token for given user name")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[Route("user/validate-token")]
		[HttpGet]
		[AllowAnonymous]
		public ValidateUserResponse ValidateToken(string enterpriseUserName, string verificationToken)
		{
			if (string.IsNullOrEmpty(enterpriseUserName.Trim()) || string.IsNullOrEmpty(verificationToken.Trim()))
			{
				throw new HttpResponseException(HttpStatusCode.BadRequest);
			}

			var manageUser = new ManageUser(_userClaims);
			return manageUser.ValidateRegistrationVerificationToken(enterpriseUserName.Trim(), verificationToken);
		}

		///// <summary>
		///// Get Starter Profile Options
		///// </summary> 
		///// <param name="enterpriseUserName">Enterprise UserName</param>
		///// <returns>StarterProfileOptionsResponse object</returns>
		//[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request (when _EnterpriseUserId is 0)")]
		//[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		//[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		//[Route("user/starterprofileoptions")]
		//[AllowAnonymous]
		//[HttpGet]
		//public StarterProfileOptionsResponse StarterProfileOptions(string enterpriseUserName)
		//{
		//    if (string.IsNullOrEmpty(enterpriseUserName.Trim()))
		//    {
		//        throw new HttpResponseException(HttpStatusCode.BadRequest);
		//    }

		//    var manageUser = new ManageUser(_userClaims);
		//    return manageUser.GetStarterProfileOptions(enterpriseUserName.Trim());
		//}

		/// <summary>
		/// Set Starter Profile  
		/// </summary> 
		/// <param name="starterProfile">StarterProfile object</param>
		/// <returns>SetStarterProfile object</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request (when _EnterpriseUserId is 0)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[Route("user/setstarterprofile")]
		[AllowAnonymous]
		[HttpPost]
		public SetStarterProfile SetStarterProfile(StarterProfile starterProfile)
		{
			if (starterProfile == null)
			{
				throw new HttpResponseException(HttpStatusCode.BadRequest);
			}

			var manageUser = new ManageUser(_userClaims);
			return manageUser.SetStarterProfile(starterProfile);
		}

        /// <summary>
        /// Create a New User and Send Email Notification
        /// </summary>
        /// <param name="newProfile">Profile of the New User</param>
        /// <returns>CreateUser Response object</returns>
        /// <remarks>Next step of this process is to call https://landing.local/new-user/#/validate/{userToken}/{enterpriseUsername}
        /// <para>User Types (MVP):</para>
        /// <para>401 - User</para>
        /// <para>402 - SuperUser</para>
        /// <para>404 - User (No Email)</para>
        /// </remarks>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [Route("user")]
        [AuthorizeRight("createuser","cloneuser")]
        [HttpPost]
		[AuthorizeScope("companyfunctions", "rplandingapi", "migrationapi")]
		public CreateUserResponse<IErrorData> CreateUser([FromBody] ProfileDetail newProfile)
        {
			CreateUserResponse<IErrorData> response = new CreateUserResponse<IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

			IList<Persona> personaList = new List<Persona>();
			Persona persona = new Persona();			

			DateTime utcNow = DateTime.UtcNow;
			DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();

			ManageRoleType roleTypes = new ManageRoleType();
			// use the organization id of the person creating the user
            IList<RoleType> userRoles = roleTypes.GetRoleType("User Role", _userClaims.OrganizationPartyId, _userClaims.OrganizationMasterId, loginName: null);
            if (userRoles == null)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.CreateUser.23";
				errorStatus.ErrorMsg = "User roles are missing.";
				response.Status = errorStatus;
				response.UserStatus = errorStatus.ErrorMsg;
				return response;
			}

			var superUser = userRoles.SingleOrDefault<RoleType>(p => p.Name.Equals("SuperUser", StringComparison.OrdinalIgnoreCase));
			var user = userRoles.SingleOrDefault<RoleType>(p => p.Name.Equals("User", StringComparison.OrdinalIgnoreCase));
			var userNoEmail = userRoles.SingleOrDefault<RoleType>(p => p.Name.Equals("User (No Email)", StringComparison.OrdinalIgnoreCase));
			var rpEmployee = userRoles.SingleOrDefault<RoleType>(p => p.Name.Equals("RealPage Employee", StringComparison.OrdinalIgnoreCase));
            var rpExternalUser = userRoles.SingleOrDefault<RoleType>(p => p.Name.Equals("External User", StringComparison.OrdinalIgnoreCase));

            if (newProfile == null)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.CreateUser.24";
				errorStatus.ErrorMsg = "Profile is required.";
				response.Status = errorStatus;
				response.UserStatus = errorStatus.ErrorMsg;
				return response;
			}
			else if (newProfile.Persona == null)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.CreateUser.25";
				errorStatus.ErrorMsg = "Persona is required.";
				response.Status = errorStatus;
				response.UserStatus = errorStatus.ErrorMsg;
				return response;
			}
			else
			{
				personaList = newProfile.Persona;
			}

			if (newProfile.IsFirstNameNullOrWhiteSpace)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.CreateUser.26";
				errorStatus.ErrorMsg = "First name is required.";
				response.UserStatus = errorStatus.ErrorMsg;
				response.Status = errorStatus;
			}
			else if (newProfile.IsLastNameNullOrWhiteSpace)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.CreateUser.27";
				errorStatus.ErrorMsg = "Last name is required.";
				response.UserStatus = errorStatus.ErrorMsg;
				response.Status = errorStatus;
			}
			else if (newProfile.userLogin.IsLoginNameNullOrWhiteSpace)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.CreateUser.28";
				errorStatus.ErrorMsg = "Username is required.";
				response.UserStatus = errorStatus.ErrorMsg;
				response.Status = errorStatus;
			}
			else if (((user != null) && (newProfile.UserTypeId != (int) user.PartyRoleTypeId)) &&
			         ((userNoEmail != null) && (newProfile.UserTypeId != (int) userNoEmail.PartyRoleTypeId)) &&
			         ((superUser != null) && (newProfile.UserTypeId != (int) superUser.PartyRoleTypeId)) &&
			         ((rpEmployee != null) && (newProfile.UserTypeId != (int) rpEmployee.PartyRoleTypeId)) &&
                     ((rpExternalUser != null) && (newProfile.UserTypeId != (int)rpExternalUser.PartyRoleTypeId))
            )
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.CreateUser.29";
				errorStatus.ErrorMsg = "Invalid user type!";
				response.UserStatus = errorStatus.ErrorMsg;
				response.Status = errorStatus;
			}
			else if (!String.IsNullOrWhiteSpace(newProfile.NotificationEmail) && !EmailFormatValidation.IsValidEmail(newProfile.NotificationEmail))
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.CreateUser.30";
				errorStatus.ErrorMsg = "Notification email is not a valid email address.";
				response.UserStatus = errorStatus.ErrorMsg;
				response.Status = errorStatus;
			}
			else if (newProfile.UserTypeId == (int) UserRoleType.User &&
			         !EmailFormatValidation.IsValidEmail(newProfile.userLogin.LoginName))
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.CreateUser.31";
				errorStatus.ErrorMsg = "Username is not a valid email address.";
				response.UserStatus = errorStatus.ErrorMsg;
				response.Status = errorStatus;
			}
			else if (newProfile.userLogin.FromDate.HasValue == false)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "User.CreateUser.33";
				errorStatus.ErrorMsg = "Effective date is required.";
				response.UserStatus = errorStatus.ErrorMsg;
				response.Status = errorStatus;
			}
            else if (_userClaims.OrganizationRealPageGuid == DefaultUserClaim.ExternalCompanyRealPageId)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "User.CreateUser.36";
                errorStatus.ErrorMsg = "Cannot create new user in External User company.";
                response.UserStatus = errorStatus.ErrorMsg;
                response.Status = errorStatus;
            }

            IManagePersona managePersona = new ManagePersona(_userClaims);

            if (newProfile.Persona.Count == 0)
			{
				//Default Persona
				IList<PersonaEnvironment> personaEnvironment = managePersona.GetPersonaEnvironmentType();
                var personaEnv = personaEnvironment.SingleOrDefault<PersonaEnvironment>(p => p.Name == "Production");
				if (personaEnv == null)
				{
					errorStatus.Success = false;
					errorStatus.ErrorCode = "User.CreateUser.32";
					errorStatus.ErrorMsg = "Persona environment is missing.";
					response.UserStatus = errorStatus.ErrorMsg;
					response.Status = errorStatus;
				}
				else
				{
					persona.Name = newProfile.UserTypeId == (int)UserRoleType.SuperUser ? "System Administrator" : "Primary";
					persona.PersonaEnvironmentTypeId = (int) personaEnv.PersonaEnvironmentTypeId;
					persona.FromDate = utcNow;
					persona.ThruDate = null;
					personaList.Add(persona);

					newProfile.Persona = personaList;
				}
			}

	        if (newProfile.organization.Count == 0)
	        {
                //Active Persona is linked to one organization
                //persona = managePersona.GetActivePersona(_realpageUserId);
                persona = managePersona.GetFirstAvailablePersonaByCompany(_realpageUserId, _orgPartyId);
                newProfile.organization.Add(persona.Organization);
	        }

			ManageUser manageUser = new ManageUser(_userClaims);
	        string invalidProducts = "";
	        List<int> invalidProductList = new List<int>();
	        foreach (var product in newProfile.productBatch)
	        {
		        if (!manageUser.CheckProductRight(product))
		        {
			        invalidProductList.Add(product.ProductId);
		        }
	        }

	        if (invalidProductList.Count > 0)
	        {
		        var manageProduct = new ManageProduct(_userClaims);
		        IList<ProductUI> productList = manageProduct.GetProducts(newProfile.organization[0].RealPageId, 0, true);
		        foreach (int productId in invalidProductList)
		        {
			        if (!string.IsNullOrWhiteSpace(invalidProducts))
			        {
				        invalidProducts += ", ";
			        }

			        if (productList.Any(p => p.ProductId == productId))
			        {
				        invalidProducts += productList.FirstOrDefault(p => p.ProductId == productId).ProductName;
			        }
			        else
			        {
				        invalidProducts += ((ProductEnum)productId).ToString();
			        }
		        }

		        errorStatus.Success = false;
		        errorStatus.ErrorCode = "User.CreateUser.41";
		        errorStatus.ErrorMsg = $"Update User: you do not have access to edit {invalidProducts} product access";
				response.UserStatus = errorStatus.ErrorMsg;
		        response.Status = errorStatus;
		        return response;
			}
			
            // PBI 78569 [All Browsers] - Able to create the user with Disabled status when today's date as 
            // effective and expiration date even after getting 'User Creation Failed' error 
            // -- Should not allow user to enter same date for both Effective and Expiration
            DateTime fromDate = newProfile.userLogin.FromDate.HasValue ? newProfile.userLogin.FromDate.Value : DateTime.UtcNow;

			if (newProfile.userLogin.ThruDate.HasValue)
			{
				TimeZone ctz = TimeZone.CurrentTimeZone;
				// get local time from utc
				DateTime clientLocal = ctz.ToLocalTime(fromDate);
				if (clientLocal.Date.Equals(newProfile.userLogin.ThruDate.Value.Date))
				{
					errorStatus.Success = false;
					errorStatus.ErrorCode = "User.CreateUser.34";
					errorStatus.ErrorMsg = "Effective & Expiration date shouldn't be same.";
					response.Status = errorStatus;
					response.UserStatus = errorStatus.ErrorMsg;
					return response;
				}
			}

			if (errorStatus.Success == false)
			{
				response.UserStatus = response.Status.ErrorMsg;
				return response;
			}

            IManageUserLogin manageUserLogin = new ManageUserLogin(_userClaims);
            bool IsValidDomainUsername = manageUserLogin.IsUserEmailDomainValid(newProfile.userLogin.LoginName);
			if(!IsValidDomainUsername)
			{
                errorStatus.Success = false;
                errorStatus.ErrorCode = "User.CreateUser";
                errorStatus.ErrorMsg = "Email domain is not allowed.";
                response.Status = errorStatus;
                response.UserStatus = errorStatus.ErrorMsg;
                return response;
            }

            if (errorStatus.Success == true)
			{				
                ProductBatch resPortal = newProfile.productBatch.FirstOrDefault<ProductBatch>((Func<ProductBatch, bool>)(p => p.ProductId == (int)ProductEnum.ResidentPortal));
                if (resPortal != null)
                {
                    //verify resident portal user has Enterprise or staff admin role then only allow to create user
                    IManageProductResidentPortal manageResidentPortal = new ManageProductResidentPortal(_userClaims);
                    bool hasAccess = manageResidentPortal.ValidateCreateUserAccess(persona);
                    if (!hasAccess)
                    {
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "User.CreateUser.35";
                        errorStatus.ErrorMsg = "Validate Resident Portal User Access: You do not have the permissions to create user.";
                        response.Status = errorStatus;
                        response.UserStatus = errorStatus.ErrorMsg;
                        return response;
                    }
                }

				return manageUser.CreateUser(newProfile, personaList);
			}
			else
			{
				return response;
			}
		}

		/// <summary>
		/// Update profile of a new user
		/// </summary>
		/// <param name="newProfile">Profile of the New User</param>
		/// <param name="companyJobTitle">Job Title of the New User</param>
		/// <param name="userLogin">User Login of the New User</param>
		/// <param name="activityToken">Activity Token</param>
		/// <returns></returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[Route("newuser/profile")]
		[AllowAnonymous]
		[HttpPost]
		public RepositoryResponse UpdateNewUser([FromBody] Profile newProfile, string companyJobTitle, string userLogin, string activityToken)
		{
			RepositoryResponse response = new RepositoryResponse();
			if (String.IsNullOrWhiteSpace(userLogin))
			{
				response.ErrorMessage = "Userlogin is required.";
			}
			else if (String.IsNullOrWhiteSpace(companyJobTitle))
			{
				response.ErrorMessage = "Company Job Title is required.";
			}
			else if (newProfile == null)
			{
				response.ErrorMessage = "Profile is required.";
			}
			else if (newProfile.PartyRole.RoleTypeId == 0)
			{
				response.ErrorMessage = "PartyRoleTypeId is invalid.";
			}
			else if (newProfile.TelecommunicationNumber.Count == 0)
			{
				response.ErrorMessage = "Telecommunication number is required.";
			}
			else if (String.IsNullOrWhiteSpace(activityToken.Trim()))
			{
				response.ErrorMessage = "Activity Token is required.";
			}

			ManageUser manageUser = new ManageUser(_userClaims);
			return manageUser.UpdateNewUser(userLogin, newProfile, newProfile.PartyRole.RoleTypeId, companyJobTitle, activityToken);
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
		[Route("customfields")]
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

		#endregion
	}

	/// <summary>
	/// Output result for newly created user
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class UserProductOutputResult
	{
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
			return SO.User.GetNewUserExample();
		}
	}

	/// <summary>
	/// Used to document examples of the Update User webapi result
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class UpdateUserOutputResultExample : IProvideExamples
	{
		/// <summary>
		/// Example object data used by Swagger to document the output of the webapi method
		/// </summary>
		/// <returns>Lastmodified date time</returns>
		public object GetExamples()
		{
			DateTime utcNow = DateTime.UtcNow;
			Guid realpPageId = Guid.NewGuid();
			List<string> roleList = new List<string>();
			List<string> propertyList = new List<string>();
			RolePropertyList rolePropertyList = new RolePropertyList();
			IList<ProductBatch> productBatchList = new List<ProductBatch>();

			IUserLogin userLogin = new UserLogin()
			{
				LoginName = "jack.bauer@noemail.com",
				FromDate = utcNow,
				ThruDate = null,
				IsActive = true,
				Is3rdPartyIDP = false
			};

			roleList = new List<string>() {"4"};
			propertyList = new List<string>() {"3523872", "1192425", "2539909"};
			rolePropertyList.RoleList = roleList;
			rolePropertyList.PropertyList = propertyList;
			rolePropertyList.IsAssigned = true;
			productBatchList.Add(new ProductBatch
			{
				ProductId = 1,
				StatusTypeId = 5,
				RetryCount = 0,
				AssignUserPersonaId = 35,
				CreateUserPersonaId = 33,
				RealPageId = realpPageId,
				InputJson = rolePropertyList,
				PersonPartyId = 33
			});

			roleList = new List<string>() {"1"};
			propertyList = new List<string>() {"4829871", "32800", "1212294"};
			rolePropertyList.RoleList = roleList;
			rolePropertyList.PropertyList = propertyList;
			rolePropertyList.IsAssigned = true;
			productBatchList.Add(new ProductBatch
			{
				ProductId = 9,
				StatusTypeId = 5,
				RetryCount = 0,
				AssignUserPersonaId = 35,
				CreateUserPersonaId = 33,
				RealPageId = realpPageId,
				InputJson = rolePropertyList,
				PersonPartyId = 33
			});

			rolePropertyList.IsAssigned = false;
			productBatchList.Add(new ProductBatch
			{
				ProductId = 8,
				StatusTypeId = 5,
				RetryCount = 0,
				AssignUserPersonaId = 35,
				CreateUserPersonaId = 33,
				RealPageId = realpPageId,
				InputJson = rolePropertyList,
				PersonPartyId = 33
			});

			IProfileDetail profileDetail = new ProfileDetail()
			{
				FirstName = "Jack",
				MiddleName = "A",
				LastName = "Bauer",
				RealPageId = realpPageId,
				UserTypeId = (int)UserRoleType.User,
				NotificationEmail = "",
				userLogin = userLogin,
				productBatch = productBatchList
			};

			Status<IErrorData> errorStatus = new Status<IErrorData>();
			ObjectOutput<IProfileDetail, IErrorData> output = new ObjectOutput<IProfileDetail, IErrorData>() {obj = profileDetail, Status = errorStatus};

			return output;
		}
	}

	/// <summary>
	/// Used to document examples of the New User webapi result
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class GetUserProfileExample : IProvideExamples
	{
		/// <summary>
		/// Example object data used by Swagger to document the output of the webapi method
		/// </summary>
		/// <returns>Newly created user id</returns>
		public object GetExamples()
		{
			DateTime utcNow = DateTime.UtcNow;
			Guid personRealPageId = Guid.NewGuid();
			Guid organizationRealPageId = Guid.NewGuid();
			IProfileDetail profileDetail = new ProfileDetail();
			profileDetail.userLogin = new UserLogin()
			{
				IsActive = true,
				FromDate = utcNow.AddDays(-7),
				IsLocked = false,
				IsSuperUser = false,
				IsTainted = false,
				LastLogin = utcNow.AddDays(-30),
				LoginName = "test@test.com",
				LoginNameType = "email",
				PartyId = 100,
				PasswordModifiedDate = utcNow.AddDays(-30),
				RealPageId = personRealPageId,
				//StatusSetDate = utcNow.AddDays(-7),
				ThruDate = utcNow.AddDays(30),
				UserId = 1,
				Is3rdPartyIDP = false
			};

			Organization organization = new Organization()
			{
				RealPageId = organizationRealPageId,
				PartyId = 3,
				BooksMasterId = 2116,
				Name = "RealPage"
			};

			IPersona persona = new Persona()
			{
				PersonaId = 33,
				PersonPartyId = 33,
				RealPageId = personRealPageId,
				OrganizationPartyId = 3,
				Organization = organization,
				PersonaTypeId = 1,
				PersonaEnvironmentTypeId = 1,
				FromDate = utcNow,
				ThruDate = null,
				IsDefault = true,
				Name = "System Administrator"
			};

			profileDetail.RealPageId = Guid.NewGuid();
			profileDetail.Suffix = "Mr.";
			profileDetail.Title = "";
			profileDetail.Avatar = "";
			profileDetail.FirstName = "Test";
			profileDetail.LastName = "User";
			profileDetail.MiddleName = "Middle";
			profileDetail.PreferredContactMethodId = 0;
			profileDetail.contactMechanism = null;
			profileDetail.AssignedProducts = null;
			profileDetail.organization = null;
			profileDetail.PartyRole = null;
			profileDetail.TelecommunicationNumber = null;

			Status<IErrorData> errorStatus = new Status<IErrorData>();
			ObjectOutput<IProfileDetail, IErrorData> output = new ObjectOutput<IProfileDetail, IErrorData>() {obj = profileDetail, Status = errorStatus};

			return output;
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
					ProductCode = (ProductEnum.Insurance).ToEnumDescription()
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
					ProductCode = (ProductEnum.OneSite).ToEnumDescription()
				},
			};
			/*
			 // keeping in case they decide they do want resources
			List<UserProducts> userResources = new List<UserProducts>()
			{
				new UserProducts()
				{
					Id = 27,
					Name = "Migration Tool Application",
					Url = "https://mylocal.corp.realpage.com/product/migrationtool?personaid=33",
					Description = "",
					Label = "migration-tool",
					FamilyId = 0,
					FamilyName = "",
					IsNewTab = true,
					IsFavorite = false,
					IsResource = true
				},
				new UserProducts()
				{
					Id = 19,
					Name = "Product Learning Portal",
					Url = "https://mylocal.corp.realpage.com/product/productlearningportal?personaid=33",
					Description = "",
					Label = "product-learning-portal",
					FamilyId = 0,
					FamilyName = "",
					IsNewTab = true,
					IsFavorite = false,
					IsResource = true
				},
			};
			*/
			productResult.Products = userProducts;
			//productResult.Resources = userResources;

			return productResult;
		}
	}

	/// <summary>
	/// Used to document examples of the User customfields Model webapi result
	/// </summary>
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

}
