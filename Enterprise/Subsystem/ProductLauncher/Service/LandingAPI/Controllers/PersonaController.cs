using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using UL = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// Persona Controller to hold all persona management related APIs
	/// </summary>
	public class PersonaController : BaseApiController
    {
        #region Private variables
        IRepositoryResponse _repositoryResponse = new RepositoryResponse();
        IManagePersona _managePersona = new ManagePersona();
        IProductInternalSettingRepository _productInternalSettingRepository = new ProductInternalSettingRepository();
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public PersonaController() : base() { }
        #endregion

        #region Public Methods

        /// <summary>
        /// Get Persona Environment Type
        /// </summary>
        /// <returns>Response with Success Message</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Person object have invalid entries)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Persona Environments", Type = typeof(IPersonaEnvironment))]
        [HttpGet]
        [Route("persona/environment")]
        public HttpResponseMessage GetPersonaEnvironmentType()
        {
            IList<PersonaEnvironment> personaEnvironmentList = _managePersona.GetPersonaEnvironmentType();
            if (personaEnvironmentList != null)
            {
                ObjectListOutput<PersonaEnvironment, IErrorData> output = new ObjectListOutput<PersonaEnvironment, IErrorData>() { list = personaEnvironmentList };
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
            return Request.CreateResponse(HttpStatusCode.NoContent, "No Data");
        }

        /// <summary>
        /// Create a new persona
        /// </summary>
        /// <param name="personRealPageId">User unique identifier</param>
        /// <param name="organizationRealPageId">Organization unique identifier</param>
        /// <param name="persona">Person object of the parameter values</param>
        /// <returns>Response with Success Message</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Persona object have invalid entries)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Newly created Persona Id", Type = typeof(Persona.PersonaOutputResult))]
		[SwaggerResponseExamples(typeof(Persona.PersonaOutputResult), typeof(NewPersonaOutputResultExample))]
		[HttpPost]
		[Route("persona")]
		public HttpResponseMessage CreatePersona(Guid personRealPageId, Guid organizationRealPageId, [FromBody] Persona persona)
		{
			//Create Persona
			ObjectOutput<IPersona, IErrorData> output = new ObjectOutput<IPersona, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			output.obj = persona;

			personRealPageId = (personRealPageId == Guid.Empty) ? _realpageUserId : personRealPageId;
			if (personRealPageId == Guid.Empty)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "200.3";
				errorStatus.ErrorMsg = "Invalid parameter: personRealPageId";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.BadRequest, output);
			}

			if (organizationRealPageId == Guid.Empty)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "200.3";
				errorStatus.ErrorMsg = "Invalid parameter Organization realPageId";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.BadRequest, output);
			}

			if (persona == null)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "200.3";
				errorStatus.ErrorMsg = "Null parameter: Persona.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.BadRequest, output);
			}

			IManagePersona personaLogic = new ManagePersona();
			_repositoryResponse = personaLogic.CreatePersona(personRealPageId, organizationRealPageId, persona);
			if (_repositoryResponse.Id == 0)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "200.3";
				errorStatus.ErrorMsg = _repositoryResponse.ErrorMessage;
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.BadRequest, output);
			}

			output.obj.PersonaId = _repositoryResponse.Id;
			output.Status = errorStatus;
			return Request.CreateResponse(HttpStatusCode.OK, output);
		}

        /// <summary>
        /// Get Persona details
        /// </summary>
        /// <returns>Persona details</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Persona object have invalid entries)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The current persona for the logged in user", Type = typeof(IPersona))]
        [SwaggerResponseExamples(typeof(IPersona), typeof(PersonaExample))]
        [HttpGet]
        [Route("persona")]
        public Persona GetPersona(long personaId = 0)
        {

            var persona = _managePersona.GetPersona(personaId == 0 ? _userClaims.PersonaId : personaId);
            if (persona == null) return null;
            IList<Persona> personaList = _managePersona.ListActivePersona(persona.RealPageId, false);

            persona.hasMultiPersona = personaList.Count(p => p.OrganizationPartyId == persona.OrganizationPartyId) > 1;
            persona.hasMultiCompany = personaList.Count(p => p.OrganizationPartyId != persona.OrganizationPartyId && p.Organization.RealPageId != DefaultUserClaim.ExternalCompanyRealPageId) > 0;
            return persona;
        }

        /// <summary>
        /// Used to trigger the notification event that the user changed company
        /// </summary>
        /// <param name="personaId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeScope("userinfoapi")]
        [Route("persona/{personaId}/company")]
        public HttpResponseMessage ChangeCompany(long personaId = 0)
        {
            // check for client token from UL
            var productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);
            string unifiedLoginClientId = productInternalSettingList.First(a => a.Name.Equals("UnifiedLoginServerClientName", StringComparison.OrdinalIgnoreCase)).Value;

            if (!_clientCode.Equals(unifiedLoginClientId, StringComparison.OrdinalIgnoreCase))
            {
                if (_userClaims.PersonaId != 0 && personaId == 0)
                {
                    personaId = _userClaims.PersonaId;
                }
            }
            else
            {
                _userClaims.PersonaId = personaId;
            }
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
        /// Get Persona company list
        /// </summary>
        /// <returns>List of persona for the given user</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Persona object have invalid entries)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List of personas", Type = typeof(PersonaCompany))]
        [SwaggerResponseExamples(typeof(PersonaCompany), typeof(PersonaCompanyListExample))]
        [HttpGet]
        [Route("personas")]
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
                        pcl.Add(new PersonaCompany() {CompanyName = persona.Organization.Name, CompanyRealPageId = persona.Organization.RealPageId, Personas = new List<PersonaCompanyDetails>()});
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
        /// Get persona assigned product(s) with optional selection type
        /// </summary>        
        /// <param name="type">Select type of products to return</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the persona products", Type = typeof(IProductUI))]
        [SwaggerResponseExamples(typeof(IProductUI), typeof(ProductMethod2Example))]
        [Route("personas/products")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetProductsByPersona([FromUri] ProductSelectType? type = null)
        {
            ObjectListOutput<PersonaProductUserDetails, IErrorData> output = new ObjectListOutput<PersonaProductUserDetails, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            Persona persona = null;
            //long? activePersonaId = _managePersona.GetActivePersonaId(_realpageUserId);

            //if (activePersonaId == null || activePersonaId == 0)
            //{
            //    errorStatus.Success = false;
            //    errorStatus.ErrorCode = "200.3";
            //    errorStatus.ErrorMsg = "Active persona not found!";
            //    output.Status = errorStatus;
            //    return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            //}

            //persona = _managePersona.GetPersona(activePersonaId.Value);
            persona = _managePersona.GetPersona(_personaId);

            if (persona == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "400";
                errorStatus.ErrorMsg = "Active persona not found!";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }

            IManageProduct manageProduct = new ManageProduct(_userClaims);
            IList<PersonaProductUserDetails> productList = manageProduct.GetUserAssignedProductsByPersona(persona, type);           
            output.list = productList;
            output.Status = errorStatus;
            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// Expire and create a product setting of a persona
        /// </summary>		
        /// <param name="productId">ProductId</param>
        /// <param name="productSetting">Productsetting resource to expire and create</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the persona products", Type = typeof(IRepositoryResponse))]
        [SwaggerResponseExamples(typeof(IRepositoryResponse), typeof(UpdateUserProductSettingExample))]
        [Route("personas/products/{productId}/productSettings")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateUserProductSetting(int? productId, [FromBody]ProductSetting productSetting)
        {
            ObjectOutput<RepositoryResponse, IErrorData> output = new ObjectOutput<RepositoryResponse, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            if (productId == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "200.3";
                errorStatus.ErrorMsg = "Null parameter: productId.";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }

            if (productSetting == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "200.3";
                errorStatus.ErrorMsg = "Null parameter: productSetting.";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }

            Persona persona = _managePersona.GetPersonaWithRightsToggle(_personaId, withRights: false);

            if (persona == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "400";
                errorStatus.ErrorMsg = "Active persona not found!";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }

            var manageProduct = new ManageProduct(_userClaims);
            RepositoryResponse response = manageProduct.UpdateProductSetting(productSetting, persona.PersonaId);

            if (response.Id == 0)
            {
                errorStatus.ErrorCode = "500";
                errorStatus.ErrorMsg = response.ErrorMessage;
                errorStatus.Success = false;
            }

            output.Status = errorStatus;
            output.obj = response;
            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

		/// <summary>
		/// Used to get a list of users roles by persona and productid
		/// </summary>
		/// <param name="personaId"></param>
		/// <param name="productId"></param>
		/// <returns></returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when object has invalid entries)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		//[SwaggerResponse(HttpStatusCode.OK, Description = "Role list", Type = typeof(ProductRole))]
		[HttpGet]
		[Route("persona/{personaId}/product/{productId}/permissions")]
		public HttpResponseMessage GetPersonaRolesByProduct([FromUri]long personaId, ProductEnum productId)
		{
			if (personaId == 0 || productId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
			//var principal = actionContext.ControllerContext.RequestContext.Principal as ClaimsPrincipal;
			System.Security.Claims.ClaimsPrincipal currentClaimPrincipal = System.Security.Claims.ClaimsPrincipal.Current;
			var grantedScopes = currentClaimPrincipal.FindAll("scope").Select(c => c.Value).ToList();
			//bool candoit = currentClaimPrincipal.IsInRole("userpermissions");
			//ManageUserRole userRoleManager = new ManageUserRole();
			//IList<ProductRole> roleList = userRoleManager.GetProductRolesByPersona(personaId, productId);
			IManageUserRoleRight urr = new ManageUserRoleRight();
			IList<UL.Role> roleList = urr.GetAssignedRoleForPersona(productId, personaId, null);

			return Request.CreateResponse(HttpStatusCode.OK, roleList);
		}
        #endregion
        

        #region Get Examples
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
                pclList.Add(new PersonaCompany() {CompanyName = "Company A", CompanyRealPageId = Guid.NewGuid(), Personas = new List<PersonaCompanyDetails>() {new PersonaCompanyDetails() {PersonaId = 1111, Name = "Persona"}}});
                pclList.Add(new PersonaCompany() {CompanyName = "Company B", CompanyRealPageId = Guid.NewGuid(), Personas = new List<PersonaCompanyDetails>() {new PersonaCompanyDetails() {PersonaId = 2222, Name = "Other Persona" } }});
                ObjectOutput<List<PersonaCompany>, IErrorData> output = new ObjectOutput<List<PersonaCompany>, IErrorData>() { obj = pclList };

                return output;
            }
        }

        /// <summary>
        /// Used to document examples of the Person Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
		public class PersonaExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>Persona example</returns>
			public object GetExamples()
			{
				Organization organization = new Organization()
				{
					RealPageId = new Guid("9e9410ae-2c41-47d2-81d1-109c08cd151c"),
					PartyId = 3,
					Name = "RealPage"
				};

				IPersona example = new Persona()
				{
					PersonaId = 33,
					PersonPartyId = 33,
					RealPageId = new Guid("c9167175-0676-4546-bba7-4a49d5809b1f"),
					OrganizationPartyId = 3,
					PersonaTypeId = 1,
					FromDate = DateTime.UtcNow,
					ThruDate = DateTime.MaxValue.ToUniversalTime(),
					IsDefault = true,
					Organization = organization
				};

				ObjectOutput<IPersona, IErrorData> output = new ObjectOutput<IPersona, IErrorData>() { obj = example };

				return output;
			}
		}
        #endregion

        #region Output results for documentation
		/// <summary>
		/// Used to document examples of the New Persona webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class NewPersonaOutputResultExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>Newly created Persona Id</returns>
			public object GetExamples()
			{
				return Persona.GetNewPersonaExample();
			}
		}

        /// <summary>
        /// Used to document examples of the PersonaProductUserDetails Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class ProductMethod2Example : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>ProductMethod2 example</returns>
            public object GetExamples()
            {
                IList<PersonaProductUserDetails> productList = new List<PersonaProductUserDetails>();
                PersonaProductUserDetails product = new PersonaProductUserDetails()
                {
                    ActivitiesList = null,
                    ClassName = "ClassName",
                    ClientId = "1",
                    Family = "Property Management",
                    FamilyId = 100,
                    HasAccess = false,
                    IsAllowFavorite = false,
                    IsFavorite = false,
                    IsNewTab = true,
                    IsResource = false,
                    LearnMore = "https://www.realpage.com/property-management-software/onesite-leasing-rents/",
                    ProductDescription = "Description of the product.",
                    ProductId = 1,
                    ProductName = "Onesite",
                    ProductUrl = "http://example.com",
                    SettingsUrl = "http://settingsurl.com",
                    Solution = "Property Management Solution",
                    SolutionId = 101,
                    Subsolution = "Facilities, Document Mngt.",
                    TitleId = "Onesite",
                    TitleUniqueId = Guid.Empty,
                    MetatagUniqueId = "",
                    OrganizationName = "Company Name",
                    OrganizationPartyId = 1,
                    PersonaId = 1,
                    PersonPartyId = 1,
                    TotalAccounts = 0
                };

                productList.Add(product);
                productList.Add(product);

                Status<IErrorData> errorStatus = new Status<IErrorData>();
                ObjectListOutput<PersonaProductUserDetails, IErrorData> output = new ObjectListOutput<PersonaProductUserDetails, IErrorData>() { list = productList, Status = errorStatus };

                return output;
            }
        }

        /// <summary>
        /// Used to document examples of the udpate product setting result
        /// </summary>
        public class UpdateUserProductSettingExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>UpdateUserProductSetting example</returns>
            public object GetExamples()
            {
                IRepositoryResponse response = new RepositoryResponse();
                response.ErrorMessage = "";
                response.Id = 1;
                response.RealPageId = Guid.Empty;

                Status<IErrorData> errorStatus = new Status<IErrorData>();
                ObjectOutput<IRepositoryResponse, IErrorData> output = new ObjectOutput<IRepositoryResponse, IErrorData>() { obj = response, Status = errorStatus };

                return output;
            }
        }
        
        #endregion
    }
}
