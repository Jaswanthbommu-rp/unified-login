using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    /// <summary>
    /// Used to insert/update the Blue Book Organization in the Green Book system
    /// </summary>
    public class OrganizationController : BaseApiController
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public OrganizationController()
        {
            // DONT USE USERCLAIM IN BASE, IT IS NULL AT THIS POINT. MOVE TO Initialize FUNCTION
        }

        /// <summary>
        /// Used for dependency injection
        /// </summary>
        /// <param name="manageOrganization"></param>
        /// <param name="repositoryResponse"></param>
        /// <param name="organizationProductRepository"></param>
        /// <param name="manageOrganizationProduct"></param>
        /// <param name="manageCustomFields"></param>
        /// <param name="manageUserLogin"></param>
        /// <param name="managePartyRelationship"></param>
        /// <param name="userClaims"></param>
        public OrganizationController(IManageOrganization manageOrganization, IRepositoryResponse repositoryResponse, IOrganizationProductRepository organizationProductRepository, IManageOrganizationProduct manageOrganizationProduct, IManageCustomFields manageCustomFields, IManageUserLogin manageUserLogin, IManagePartyRelationship managePartyRelationship, IManageBlueBook manageBlueBook, DefaultUserClaim userClaims)
        {
            _repositoryResponse = repositoryResponse;
            _organizationProductRepository = organizationProductRepository;
            _manageOrganizationProduct = manageOrganizationProduct;
            _manageCustomFields = manageCustomFields;
            _manageUserLogin = manageUserLogin;
            _managePartyRelationship = managePartyRelationship;
            _manageOrganization = manageOrganization;
            _productInternalSettingRepository = null;
            _manageBlueBook = manageBlueBook;
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
            _organizationProductRepository = new OrganizationProductRepository();
            _manageOrganizationProduct = new ManageOrganizationProduct(_organizationProductRepository);
            _manageUserLogin = new ManageUserLogin();
            _managePartyRelationship = new ManagePartyRelationship();
            _manageOrganization = new ManageOrganization(_userClaims);
            _manageBlueBook = new ManageBlueBook(_userClaims);
            _productInternalSettingRepository = new ProductInternalSettingRepository();
        }

        #endregion

        #region Private variables

        IRepository _repository;
        IRepositoryResponse _repositoryResponse;
        IOrganizationProductRepository _organizationProductRepository;
        IManageOrganizationProduct _manageOrganizationProduct;
        IManageCustomFields _manageCustomFields;
        IManageUserLogin _manageUserLogin;
        IManagePartyRelationship _managePartyRelationship;
        private IManageOrganization _manageOrganization;
        private IManageBlueBook _manageBlueBook;
        private IProductInternalSettingRepository _productInternalSettingRepository;

        #endregion

        #region Public Organization Methods

        /// <summary>
        /// List Company Custom Fields
        /// </summary>
        /// <returns>A list of company's customfields</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Organization customfield object has invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Not Found")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Organization CustomFields result", Type = typeof(ICustomField))]
        [SwaggerResponseExamples(typeof(ICustomField), typeof(OrganizationCustomFieldsExample))]
        [SwaggerOperation("OrganizationCustomFields")]
        [Route("organization/customfields")]
        [HttpGet]
        public HttpResponseMessage OrganizationCustomFields([FromUri] RequestParameter datafilter)
        {
            if (_manageCustomFields == null)
            {
                _manageCustomFields = new ManageCustomFields(_userClaims);
            }

            IDictionary<object, object> globals = new Dictionary<object, object>();

            if (datafilter == null)
            {
                datafilter = new RequestParameter();
            }

            globals.Add(BaseType.RequestParameter, datafilter);

            IList<CustomField> customFieldList = _manageCustomFields.GetCustomField(globals: globals, bookMasterId: _userClaims.CustomerMasterId, bookMasterTypeId: (int) BookMasterType.CustomerMasterId);

            ListResponse response = new ListResponse()
            {
                Records = customFieldList.Cast<object>().ToList(),
                TotalRows = customFieldList.Count(),
                RowsPerPage = customFieldList.Count(),
                ErrorReason = string.Empty,
                TotalPages = 1
            };
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Insert the Organization in the GreenBook system
        /// </summary>
        /// <param name="organization">The organization information</param>
        /// <param name="processBlueBookMessage">Process a RabbitMQ BlueBook Message to Create a company, RealPage Employee admin user, and a company Admin user</param>
        /// <returns>The Organization that was created</returns>
        /// <response code="200">Organization id created</response>
        /// <response code="400">Bad request(when Organization object has invalid entries / when Information is out of sync with the server)</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal Server Error</response>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Organization object has invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Organization id created", Type = typeof(Organization))]
        [SwaggerResponseExamples(typeof(Organization), typeof(OrganizationOutputResultExample))]
        [SwaggerOperation("InsertOrganization")]
        [Route("organization")]
        [HttpPost]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        public HttpResponseMessage InsertOrganization([FromBody] OrganizationCreate organization, bool processBlueBookMessage = false)
        {
            var result = _manageOrganization.CreateOrganization(organization, processBlueBookMessage);

            if (!result.Status.Success)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, result.Status.ErrorMsg);
            }

            IList<CustomerCompanyMap> companyMapResource = _manageBlueBook.GetCompanyMap(booksCompanyMasterId: organization.BooksCustomerMasterId, source: ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform), includeGreenBookCares: false);

            // add the new company to books
            var companyInstance = new CompanyInstanceAdd()
            {
                Id = organization.BooksCustomerMasterId,
                CustomerCompanyId = organization.BooksCustomerMasterId,
                CompanyInstanceSourceId = result.obj.Org.RealPageId.ToString(),
                CompanyName = result.obj.Org.Name,
                Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                IsActive = true,
                CreatedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation",
                CustomerEnvironment = result.obj.Org.OrganizationDomain.Name
            };

            if (companyMapResource != null)
            {
                // remove any existing instance and add a new one
                foreach (var customerCompanyMap in companyMapResource)
                {
                    bool deleteInstance = false;
                    customerCompanyMap.CompanyInstance.ForEach(i =>
                    {
                        if (i.CustomerEnvironment == null || i.CustomerEnvironment.Equals(companyInstance.CustomerEnvironment, StringComparison.OrdinalIgnoreCase))
                        {
                            deleteInstance = true;
                        }
                    });
                    if (deleteInstance)
                    {
                        _manageBlueBook.DeleteBooksGreenBookCompanyInstance(new CompanyInstance() {CompanyInstanceId = customerCompanyMap.CompanyInstanceId, ModifiedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation"});
                    }
                }
            }

            // add the new company data back to books
            _manageBlueBook.AddBooksGreenBookCompanyInstance(companyInstance);

            return Request.CreateResponse(HttpStatusCode.OK, result.obj);
        }

        /// <summary>
        /// Update the Organization in the GreenBook system
        /// </summary>
        /// <param name="organization">The organization information to update</param>
        /// <returns>The Organization that was updated</returns>
        /// <response code="200">Organization id updated</response>
        /// <response code="400">Bad request(when Organization object has invalid entries / when Information is out of sync with the server)</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Organization object has invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Not Found")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Organization id updated", Type = typeof(Organization))]
        [SwaggerResponseExamples(typeof(Organization), typeof(OrganizationOutputResultExample))]
        [SwaggerOperation("UpdateOrganization")]
        [Route("organization")]
        [HttpPut]
        public HttpResponseMessage UpdateOrganization([FromBody] OrganizationUpdate organization)
        {
            Organization org = null;
            if (organization.BooksCustomerMasterId != 0)
            {
                // get the organization by customer master id
                org = _manageOrganization.GetOrganization(realPageId: Guid.Empty, blueBookId: organization.BooksCustomerMasterId);
            }
            else if (organization.BooksMasterId != 0)
            {
                // get the organization by Master Data Management (black book) master id
                org = _manageOrganization.GetOrganization(realPageId: Guid.Empty, blackBookId: organization.BooksMasterId);
            }
            else
            {
                // get the org by UL realpageID
                org = _manageOrganization.GetOrganization(organization.RealPageId);
            }

            if (org == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Not found");
            }

            org.Name = organization.Name;

            var orgTypes = _manageOrganization.ListOrganizationType();
            if (organization.OrganizationTypeId != 0)
            {
                if (!orgTypes.Any(o => o.OrganizationTypeId == organization.OrganizationTypeId))
                {
                    // error
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid organization type id");
                }
            }
            else if (!string.IsNullOrEmpty(organization.OrganizationTypeName))
            {
                if (orgTypes.Any(o => o.Name.Equals(organization.OrganizationTypeName, StringComparison.OrdinalIgnoreCase)))
                {
                    organization.OrganizationTypeId = orgTypes.FirstOrDefault(o => o.Name.Equals(organization.OrganizationTypeName, StringComparison.OrdinalIgnoreCase)).OrganizationTypeId;
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid organization type");
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Missing organization type");
            }

            var orgDomains = _manageOrganization.ListOrganizationDomain();
            if (organization.OrganizationDomainId != 0)
            {
                if (orgDomains.All(o => o.OrganizationDomainId != organization.OrganizationDomainId))
                {
                    // error
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid organization domain id");
                }
            }
            else if (!string.IsNullOrEmpty(organization.OrganizationDomainName))
            {
                if (orgDomains.Any(o => o.Name.Equals(organization.OrganizationDomainName, StringComparison.OrdinalIgnoreCase)))
                {
                    organization.OrganizationDomainId = orgDomains.FirstOrDefault(o => o.Name.Equals(organization.OrganizationDomainName, StringComparison.OrdinalIgnoreCase)).OrganizationDomainId;
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid organization domain");
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Missing organization domain");
            }

            org.organizationType.OrganizationTypeId = organization.OrganizationTypeId;
            org.OrganizationDomain.OrganizationDomainId = organization.OrganizationDomainId;

            _repositoryResponse = _manageOrganization.UpdateOrganization(org);

            if (!String.IsNullOrEmpty(_repositoryResponse.ErrorMessage))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, _repositoryResponse.ErrorMessage);
            }

            org = _manageOrganization.GetOrganization(org.RealPageId);

            return Request.CreateResponse(HttpStatusCode.OK, org);
        }

        /// <summary>
        /// Used to get details for the given Organization id
        /// </summary>
        /// <param name="realPageId">The unique identitifier for the organization</param>
        /// <returns>The Organization information for the given id</returns>
        /// <response code="200">Organization result</response>
        /// <response code="400">Bad request(when Organization object has invalid entries / when Information is out of sync with the server)</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal Server Error</response>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Organization object has invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Not Found")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Organization result", Type = typeof(Organization))]
        [SwaggerOperation("GetOrganization")]
        [Route("organization/{realPageId}")]
        [HttpGet]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        public HttpResponseMessage GetOrganization(Guid? realPageId = null)
        {
            if (realPageId.HasValue)
            {
                Organization org = _manageOrganization.GetOrganization(realPageId.Value);
                if (org == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not found");
                }

                return Request.CreateResponse(HttpStatusCode.OK, org);
            }
            else
            {
                IList<Organization> orgList = _manageOrganization.GetOrganizationList();
                return Request.CreateResponse(HttpStatusCode.OK, orgList);
            }
        }

        /// <summary>
        /// Used to sync UPFM records in books
        /// </summary>
        /// <returns>result of sync</returns>
        /// <response code="200">Organization result</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal Server Error</response>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Complete")]
        [Route("organization/syncbooks")]
        [HttpGet]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        public HttpResponseMessage SyncBooksOrganizations(bool commit = false)
        {
            Dictionary<string, CompanyInstanceAdd> processResult = new Dictionary<string, CompanyInstanceAdd>();
            var productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings((int) ProductEnum.UnifiedPlatform);
            var booksUrl = productInternalSettingList.First(a => a.Name.Equals("BlueBookAPIEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
            if (booksUrl.Contains("booksapi.realpage.com"))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "This should not run on production books!");
            }

            IList<Organization> orgList = _manageOrganization.GetOrganizationList();
            foreach (var organization in orgList)
            {
                if (organization.BooksCustomerMasterId <= 0)
                {
                    break;
                }

                IList<CustomerCompanyMap> companyMapResource = _manageBlueBook.GetCompanyMap(booksCompanyMasterId: organization.BooksCustomerMasterId, source: ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform), includeGreenBookCares: false);

                // add the missing company to books
                var companyInstance = new CompanyInstanceAdd()
                {
                    Id = organization.BooksCustomerMasterId,
                    CustomerCompanyId = organization.BooksCustomerMasterId,
                    CompanyInstanceSourceId = organization.RealPageId.ToString(),
                    CompanyName = organization.Name,
                    Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                    IsActive = true,
                    CreatedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation",
                    CustomerEnvironment = organization.OrganizationDomain.Name
                };

                bool foundInstance = false;

                if (companyMapResource != null)
                {
                    // remove any existing instance and add a new one
                    foreach (var customerCompanyMap in companyMapResource)
                    {
                        bool deleteInstance = false;
                        customerCompanyMap.CompanyInstance.ForEach(i =>
                        {
                            if (i.CustomerEnvironment == null)
                            {
                                deleteInstance = true;
                            }
                            else if (i.CustomerEnvironment.Equals(companyInstance.CustomerEnvironment, StringComparison.OrdinalIgnoreCase) && !i.CompanyInstanceSourceId.Equals(companyInstance.CompanyInstanceSourceId, StringComparison.OrdinalIgnoreCase))
                            {
                                deleteInstance = true;
                            }
                            else
                            {
                                foundInstance = true;
                            }
                        });
                        if (deleteInstance)
                        {
                            if (commit)
                            {
                                _manageBlueBook.DeleteBooksGreenBookCompanyInstance(new CompanyInstance() {CompanyInstanceId = customerCompanyMap.CompanyInstanceId, ModifiedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation"});
                            }
                        }
                    }
                }

                if (!foundInstance)
                {
                    // add the company data to books
                    if (commit)
                    {
                        _manageBlueBook.AddBooksGreenBookCompanyInstance(companyInstance);
                    }

                    processResult.Add(organization.BooksCustomerMasterId.ToString(), companyInstance);
                }
            }

            return Request.CreateResponse(HttpStatusCode.NoContent, processResult);
        }


        /// <summary>
        /// list of Organization By Enterprise User Id
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="roleTypeFrom">Person Role Type name in the Relationship (Optional)</param>
        /// <param name="roleTypeTo">Organization Role Type name in the Relationship (Optional)</param>
        /// <param name="relationshipType">Parties Relationship type name (Optional)</param>
        /// <returns>A list of Organization(s) Details for a person</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the password policy", Type = typeof(Organization))]
        [SwaggerResponseExamples(typeof(Organization), typeof(OrganizationOutputResultExample))]
        [Route("organization/person/{realPageId}")]
        [HttpGet]
        public HttpResponseMessage ListOrganizationByEnterpriseUserId(Guid realPageId, string roleTypeFrom = null, string roleTypeTo = null, string relationshipType = null)
        {
            if (realPageId == Guid.Empty)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
            }

            if (_manageUserLogin == null)
            {
                _manageUserLogin = new ManageUserLogin(_userClaims);
            }

            IList<Organization> organizationList = _manageUserLogin.ListOrganizationByEnterpriseUserId(realPageId, relationshipType);

            if (organizationList != null)
            {
                PartyRelationship partyRelationship = new PartyRelationship();
                foreach (var organization in organizationList)
                {
                    partyRelationship = _managePartyRelationship.GetPartyRelationship(realPageId, organization.RealPageId, roleTypeFrom, roleTypeTo, relationshipType);
                    if (partyRelationship != null)
                    {
                        organization.partyRelationship = partyRelationship;
                    }
                }

                ObjectListOutput<Organization, IErrorData> output = new ObjectListOutput<Organization, IErrorData>() {list = organizationList};
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }

            //When trying to get a list of Organization(s) for a Person that doesn't exists
            return Request.CreateResponse(HttpStatusCode.NoContent, "Invalid realPageId");
        }

        /// <summary>
        /// List organization product(s)
        /// </summary>
        /// <param name="realPageId">The unique identitifier for the organization</param>
        /// <param name="mergePersonaAccess">Merge persona product access</param>
        /// <param name="allProducts">Return all product types</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the organization products", Type = typeof(IProductUI))]
        [SwaggerResponseExamples(typeof(IProductUI), typeof(ProductMethodExample))]
        [Route("organization/{realPageId}/products")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetProductsByOrganization([FromUri] Guid? realPageId, [FromUri] bool? mergePersonaAccess, [FromUri] bool? allProducts)
        {
            ObjectListOutput<ProductUI, IErrorData> output = new ObjectListOutput<ProductUI, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            if (realPageId == null || realPageId == Guid.Empty)
            {
                realPageId = _userClaims.OrganizationRealPageGuid;
            }

            Organization org = _manageOrganization.GetOrganization(realPageId.Value);

            if (org == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "400";
                errorStatus.ErrorMsg = "Organization not found!";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.BadRequest, output);
            }

            Persona persona = null;
            IManagePersona managePersona = new ManagePersona();
            long personaId = 0;

            if (mergePersonaAccess.HasValue && mergePersonaAccess.Value == true)
            {
                persona = managePersona.GetPersona(_userClaims.PersonaId);

                if (persona == null)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "400";
                    errorStatus.ErrorMsg = "Persona not found!";
                    output.Status = errorStatus;
                    return Request.CreateResponse(HttpStatusCode.BadRequest, output);
                }

                personaId = persona.PersonaId;
            }

            IManageCredential manageCredential = new ManageCredential(_userClaims);
            CheckPasswordExpirationResponse checkPasswordExpirationResponse = manageCredential.CheckPasswordExpiration(_userClaims.UserId, _userClaims.UserRealPageGuid);
            if ((checkPasswordExpirationResponse != null) && (!checkPasswordExpirationResponse.IsPasswordExpired))
            {
                var manageProduct = new ManageProduct(_userClaims);
                var cacheKey = $"getListProductsByOrganization_{org.RealPageId}";
                MemoryCache.Default.Remove(cacheKey);

                IList<ProductUI> productList = manageProduct.GetProducts(org.RealPageId, personaId, (allProducts.HasValue ? allProducts.Value : false));

                output.list = productList;
            }

            output.Status = errorStatus;

            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// Add/Updates products to an organization
        /// </summary>
        /// <param name="realPageId">The unique identifier for the organization</param>
        /// <param name="products">A list of BlueBook product names. i.e. </param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.Created, Description = "Success")]
        [Route("organization/{realPageId}/product")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage AddProductToOrganization([FromUri] Guid realPageId, [FromBody] List<string> products)
        {
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            if (realPageId == Guid.Empty)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "200.3";
                errorStatus.ErrorMsg = "Invalid parameter: realPageId";
                return Request.CreateResponse(HttpStatusCode.BadRequest, errorStatus);
            }

            Organization org = _manageOrganization.GetOrganization(realPageId);

            if (org == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "400";
                errorStatus.ErrorMsg = "Organization not found!";
                return Request.CreateResponse(HttpStatusCode.BadRequest, errorStatus);
            }

            if (products == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "400";
                errorStatus.ErrorMsg = "Products not found!";
                return Request.CreateResponse(HttpStatusCode.BadRequest, errorStatus);
            }

            List<ProductEnum> addProductList = new List<ProductEnum>();
            // verify the products, if any, exist and can be added to the customer
            List<string> invalidProductList = ManageOrganization.ParseProduct(products, addProductList);
            if (invalidProductList.Count > 0)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "An invalid product was given : " + String.Join(",", invalidProductList));
            }

            // add the given products to the new company
            if (addProductList.Count > 0)
            {
                _manageOrganizationProduct = new ManageOrganizationProduct(_organizationProductRepository);
                _repositoryResponse = _manageOrganizationProduct.InsertUpdateOrganizationProduct(org.PartyId, addProductList);
                if (!string.IsNullOrEmpty(_repositoryResponse.ErrorMessage))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, _repositoryResponse.ErrorMessage);
                }
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        /// <summary>
        /// Remove products from an organization
        /// </summary>
        /// <param name="realPageId">The unique identitifier for the organization</param>
        /// <param name="products"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Success")]
        [Route("organization/{realPageId}/product")]
        [Authorize]
        [HttpDelete]
        public HttpResponseMessage DeleteProductFromOrganization([FromUri] Guid realPageId, [FromBody] List<string> products)
        {
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            if (realPageId == null && realPageId == Guid.Empty)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "200.3";
                errorStatus.ErrorMsg = "Invalid parameter: realPageId";
                return Request.CreateResponse(HttpStatusCode.BadRequest, errorStatus);
            }

            Organization org = _manageOrganization.GetOrganization(realPageId);

            if (org == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "400";
                errorStatus.ErrorMsg = "Organization not found!";
                return Request.CreateResponse(HttpStatusCode.BadRequest, errorStatus);
            }

            if (products == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "400";
                errorStatus.ErrorMsg = "Products not found!";
                return Request.CreateResponse(HttpStatusCode.BadRequest, errorStatus);
            }

            List<ProductEnum> addProductList = new List<ProductEnum>();
            // verify the products, if any, exist and can be added to the customer
            List<string> invalidProductList = ManageOrganization.ParseProduct(products, addProductList);
            if (invalidProductList.Count > 0)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "An invalid product was given : " + String.Join(",", invalidProductList));
            }

            // add the given products to the new company
            if (addProductList.Count > 0)
            {
                IRepositoryResponse response = DeleteProductsFromOrganization(addProductList, org.PartyId);
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    Request.CreateErrorResponse(HttpStatusCode.BadRequest, response.ErrorMessage);
                }
            }

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Used to get the Organization Identity ProviderType by realPageId
        /// </summary>
        /// <param name="realPageId">Organization unique identifier</param>
        /// <returns>Identity Provider Type object</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when input has invalid entries)")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the Identity Provider Type by Enterprise User Name", Type = typeof(IdentityProviderType))]
        [SwaggerResponseExamples(typeof(IdentityProviderType), typeof(IdentityProviderTypeExample))]
        [Route("organization/Providertype")]
        [HttpGet]
        public HttpResponseMessage GetOrganizationIdentityProviderType(Guid? realPageId = null)
        {
            ObjectOutput<IIdentityProviderType, IErrorData> output = new ObjectOutput<IIdentityProviderType, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            if ((realPageId == Guid.Empty) || (realPageId == null))
            {
                realPageId = _userClaims.OrganizationRealPageGuid;
            }

            IManageOrganization manageOrganization = new ManageOrganization(_userClaims);
            IList<IdentityProviderType> identityProviderTypeList = manageOrganization.GetOrganizationIdentityProviderType(realPageId.Value);
            IIdentityProviderType idpt = (from a in identityProviderTypeList where a.IsLocal == (identityProviderTypeList.Count > 1 ? false : true) select a).FirstOrDefault();
            if (idpt != null)
            {
                output.obj = idpt;
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }

            //output.obj = idpt;
            errorStatus.Success = false;
            errorStatus.ErrorCode = "Organization.GetOrganizationIdentityProviderType.2";
            errorStatus.ErrorMsg = "Get Organization Identity ProviderType: No data";
            output.Status = errorStatus;
            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        #endregion

        #region Public Organization Type Methods

        /// <summary>
        /// List Organization Types
        /// </summary>
        /// <returns>Profile object</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Organization Type object have invalid entries)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List Organization Types", Type = typeof(OrganizationType))]
        [SwaggerResponseExamples(typeof(OrganizationType), typeof(OrganizationTypeExample))]
        [Route("organizationtype")]
        [HttpGet]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        public HttpResponseMessage OrganizationType()
        {
            ObjectListOutput<OrganizationType, IErrorData> output = new ObjectListOutput<OrganizationType, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            IList<OrganizationType> organizationTypeList = _manageOrganization.ListOrganizationType();

            if (organizationTypeList != null)
            {
                output.Status = errorStatus;
                output.list = organizationTypeList;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
            else
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Organization.OrganizationType.1";
                errorStatus.ErrorMsg = "List OrganizationType: No data";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
        }

        #endregion

        #region Public Organization Domain Methods

        /// <summary>
        /// List Organization Domains
        /// </summary>
        /// <returns>Company domains</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Organization Domain object has invalid entries)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List Organization Domains", Type = typeof(OrganizationDomain))]
        [SwaggerResponseExamples(typeof(OrganizationDomain), typeof(OrganizationDomainExample))]
        [Route("organizationdomain")]
        [HttpGet]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        public HttpResponseMessage GetOrganizationDomain()
        {
            ObjectListOutput<OrganizationDomain, IErrorData> output = new ObjectListOutput<OrganizationDomain, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            IList<OrganizationDomain> organizationDomainList = _manageOrganization.ListOrganizationDomain();

            if (organizationDomainList != null)
            {
                output.Status = errorStatus;
                output.list = organizationDomainList;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
            else
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Organization.OrganizationDomain.1";
                errorStatus.ErrorMsg = "List OrganizationDomain: No data";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
        }

        #endregion

        #region Private functions

        ///// <summary>
        ///// Used to parse the list of product codes and convert them into ProductEnum
        ///// </summary>
        ///// <param name="productCode"></param>
        ///// <param name="addProductList"></param>
        ///// <returns></returns>
        //public List<string> ParseProduct(List<string> productCode, List<ProductEnum> addProductList)
        //{
        //	List<string> invalidProductList = new List<string>();
        //	if (productCode != null)
        //	{
        //		foreach (string product in productCode)
        //		{
        //			bool foundProduct = AddProductToList(product, addProductList);
        //			if (!foundProduct)
        //			{
        //				invalidProductList.Add(product);
        //			}
        //		}
        //	}
        //
        //	return invalidProductList;
        //}
        //
        ///// <summary>
        ///// Used to convert the BlueBook product name to the UnifiedUI product id
        ///// </summary>
        ///// <param name="product"></param>
        ///// <param name="addProductList"></param>
        ///// <returns></returns>
        //private bool AddProductToList(string product, List<ProductEnum> addProductList)
        //{
        //	bool foundProduct = false;
        //	foreach (var pi in typeof(BlueBookProductConstants).GetFields())
        //	{
        //		if (product.ToUpper() == pi.GetValue(pi).ToString())
        //		{
        //			// found product, so add it to the list to add to the company
        //			// get the product id from the product enum
        //			foreach (var pr in typeof(ProductEnum).GetFields())
        //			{
        //				if (pr.Name.ToUpper() == pi.Name.ToUpper())
        //				{
        //					foundProduct = true;
        //					addProductList.Add((ProductEnum)Enum.Parse(typeof(ProductEnum), pr.Name));
        //					break;
        //				}
        //			}
        //			break;
        //		}
        //	}
        //	return foundProduct;
        //}



        /// <summary>
        /// Used to delete products from an organization
        /// </summary>
        /// <param name="addProductList"></param>
        /// <param name="partyId"></param>
        private IRepositoryResponse DeleteProductsFromOrganization(List<ProductEnum> addProductList, long partyId)
        {
            IRepositoryResponse response = new RepositoryResponse();
            IManageOrganizationProduct manageOrganizationProduct = new ManageOrganizationProduct(_organizationProductRepository);
            foreach (ProductEnum product in addProductList)
            {
                response = manageOrganizationProduct.DeleteOrganizationProduct(partyId: partyId, product: product);
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    return response;
                }
            }

            return response;
        }

        #endregion

        #region Output results for documentation

        /// <summary>
        /// Used to document examples of the IdentityProviderType Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class IdentityProviderTypeExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>IdentityProviderType example</returns>
            public object GetExamples()
            {
                IdentityProviderType example = new IdentityProviderType()
                {
                    AuthenticationType = "ID3"
                };

                IdentityProviderTypeOutput output = new IdentityProviderTypeOutput() {identityProviderType = example};

                return output;
            }
        }

        /// <summary>
        /// Used to document examples of the Organization webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class OrganizationOutputResultExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Newly created organization</returns>
            public object GetExamples()
            {
                return Organization.GetOrganizationExample();
            }
        }

        /// <summary>
        /// Used to document examples of the Product Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class ProductMethodExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>ProductMethodExample example</returns>
            public object GetExamples()
            {
                IList<ProductUI> productList = new List<ProductUI>();
                ProductUI product = new ProductUI()
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
                    TitleUniqueId = Guid.Empty
                };

                productList.Add(product);
                productList.Add(product);

                Status<IErrorData> errorStatus = new Status<IErrorData>();
                ObjectListOutput<ProductUI, IErrorData> output = new ObjectListOutput<ProductUI, IErrorData>() {list = productList, Status = errorStatus};

                return output;
            }
        }

        /// <summary>
        /// Used to document examples of the OrganizationType Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class OrganizationTypeExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>List of Organization Types example</returns>
            public object GetExamples()
            {
                OrganizationType example = new OrganizationType()
                {
                    OrganizationTypeId = 6,
                    Name = "Multifamily",
                    CreateDate = DateTime.Today
                };
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                ObjectOutput<OrganizationType, IErrorData> output = new ObjectOutput<OrganizationType, IErrorData>()
                {
                    obj = example,
                    Status = errorStatus
                };

                return output;
            }
        }

        /// <summary>
        /// Used to document examples of the OrganizationDomain Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class OrganizationDomainExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>List of Organization Types example</returns>
            public object GetExamples()
            {
                OrganizationDomain example = new OrganizationDomain()
                {
                    OrganizationDomainId = 1,
                    Name = "Primary",
                    CreateDate = DateTime.Today
                };
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                ObjectOutput<OrganizationDomain, IErrorData> output = new ObjectOutput<OrganizationDomain, IErrorData>()
                {
                    obj = example,
                    Status = errorStatus
                };

                return output;
            }
        }

        /// <summary>
        /// Used to document examples of the Organization customfields Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class OrganizationCustomFieldsExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>List of User CustomFields example</returns>
            public object GetExamples()
            {
                IList<CustomField> customFieldValueList = new List<CustomField>()
                {
                    new CustomField()
                    {
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