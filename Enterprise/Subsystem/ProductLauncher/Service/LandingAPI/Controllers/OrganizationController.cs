using Aspose.Cells;
using LaunchDarkly.Sdk.Server.Interfaces;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.ThirdParty;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.Export;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Maintenance;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    /// <summary>
    /// Used to insert/update the Blue Book Organization in the Green Book system
    /// </summary>
    public class OrganizationController : BaseApiController
    {
        #region Private variables

        private IRepositoryResponse _repositoryResponse;
        private IManageOrganizationProduct _manageOrganizationProduct;
        private IManageCustomFields _manageCustomFields;
        private IManageUserLogin _manageUserLogin;
        private IManagePartyRelationship _managePartyRelationship;
        private IManageOrganization _manageOrganization;
        private IManageBlueBook _manageBlueBook;
        private IProductInternalSettingRepository _productInternalSettingRepository;
        private readonly HttpMessageHandler _messageHandler;
        private readonly IRepository _repository;
        private readonly IManageProductOneSite _manageProductOneSite;
        private IManageProduct _manageProduct;
        private IManageCredential _manageCredential;
        private IManagePerson _managePerson;
        private IManagePersona _managePersona;
        private readonly int _maxDOPSetting = 6;
        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public OrganizationController()
        {
            // DONT USE USERCLAIM IN BASE, IT IS NULL AT THIS POINT. MOVE TO Initialize FUNCTION
        }

        /// <summary>
        /// Unit test constructor v2
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="repositoryResponse"></param>
        /// <param name="messageHandler"></param>
        /// <param name="userClaims"></param>
        public OrganizationController(IRepository repository, IRepositoryResponse repositoryResponse, HttpMessageHandler messageHandler, DefaultUserClaim userClaims)
        {
            _repository = repository;
            _repositoryResponse = repositoryResponse;
            _manageCustomFields = new ManageCustomFields(new CustomFieldsRepository(repository), userClaims);
            _manageUserLogin = new ManageUserLogin(repository, userClaims, messageHandler);
            _managePartyRelationship = new ManagePartyRelationship(new PartyRelationshipRepository(repository));
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _manageBlueBook = new ManageBlueBook(userClaims, repository, _productInternalSettingRepository, messageHandler);
            _manageOrganization = new ManageOrganization(repository, userClaims, messageHandler);
            _messageHandler = messageHandler;
            _userClaims = userClaims;
            _manageProduct = new ManageProduct(repository, userClaims, messageHandler);
            _manageOrganizationProduct = new ManageOrganizationProduct(userClaims, repository, _manageBlueBook, _manageProduct);
            _managePerson = new ManagePerson(repository);
            _managePersona = new ManagePersona(userClaims);
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="repositoryResponse"></param>
        /// <param name="messageHandler"></param>
        /// <param name="ldClient"></param>
        /// <param name="manageProductAssetOptimization"></param>
        /// <param name="userClaims"></param>
        public OrganizationController(IRepository repository, IRepositoryResponse repositoryResponse, HttpMessageHandler messageHandler, ILdClient ldClient, IManageProductAssetOptimization manageProductAssetOptimization, DefaultUserClaim userClaims)
        {
            _repository = repository;
            _repositoryResponse = repositoryResponse;
            _manageCustomFields = new ManageCustomFields(new CustomFieldsRepository(repository), userClaims);
            _manageUserLogin = new ManageUserLogin(repository, userClaims, messageHandler);
            _managePartyRelationship = new ManagePartyRelationship(new PartyRelationshipRepository(repository));
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _manageBlueBook = new ManageBlueBook(userClaims, repository, _productInternalSettingRepository, messageHandler);
            _manageOrganization = new ManageOrganization(repository, userClaims, messageHandler, manageProductAssetOptimization);
            _messageHandler = messageHandler;
            _userClaims = userClaims;
            _manageProduct = new ManageProduct(repository, userClaims, messageHandler);
            _manageOrganizationProduct = new ManageOrganizationProduct(userClaims, repository, _manageBlueBook, _manageProduct);
            FeatureFlag.LdClient = ldClient;
            _managePerson = new ManagePerson(repository);
            _managePersona = new ManagePersona(userClaims);
        }

        /// <summary>
        /// Audit Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="repositoryResponse"></param>
        /// <param name="messageHandler"></param>
        /// <param name="manageProductOneSite"></param>
        /// <param name="userClaims"></param>
        public OrganizationController(IRepository repository, IRepositoryResponse repositoryResponse, HttpMessageHandler messageHandler, IManageProductOneSite manageProductOneSite, DefaultUserClaim userClaims)
        {
            _repository = repository;
            _repositoryResponse = repositoryResponse;
            _manageCustomFields = new ManageCustomFields(new CustomFieldsRepository(repository), userClaims);
            _manageUserLogin = new ManageUserLogin(repository, userClaims, messageHandler);
            _managePartyRelationship = new ManagePartyRelationship(new PartyRelationshipRepository(repository));
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _manageBlueBook = new ManageBlueBook(userClaims, repository, _productInternalSettingRepository, messageHandler);
            _manageOrganization = new ManageOrganization(repository, userClaims, messageHandler);
            _messageHandler = messageHandler;
            _userClaims = userClaims;
            _manageProductOneSite = manageProductOneSite;
            _managePerson = new ManagePerson(repository);
            _managePersona = new ManagePersona(userClaims);
        }

        /// <summary>
        /// Used to initialize DI classes with userclaim
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            _repositoryResponse = new RepositoryResponse();
            _manageOrganizationProduct = new ManageOrganizationProduct(_userClaims);
            _manageUserLogin = new ManageUserLogin(_userClaims);
            _managePartyRelationship = new ManagePartyRelationship();
            _manageOrganization = new ManageOrganization(_userClaims);
            _manageBlueBook = new ManageBlueBook(_userClaims);
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _manageProduct = new ManageProduct(_userClaims);
            _manageCredential = new ManageCredential(_userClaims);
            _managePerson = new ManagePerson();
            _managePersona = new ManagePersona(_userClaims);
        }

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

            IList<CustomField> customFieldList = _manageCustomFields.GetCustomField(globals: globals, partyId: _userClaims.OrganizationPartyId);

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
            if (!string.IsNullOrEmpty(organization.CompanyInstancePartnerSourceId) && !string.IsNullOrEmpty(organization.CompanyInstancePartner))
            {
                var partnerInstance = _manageBlueBook.GetCompanyInstanceBySourceAndInstanceId(organization.CompanyInstancePartnerSourceId, organization.CompanyInstancePartner);
                if (partnerInstance == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Partner instance could not be found");
                }
            }

            var organizationDomainList = _manageOrganization.ListOrganizationDomain();

            if (!organizationDomainList.Exists(d => d.Name.Equals(organization.OrganizationDomain, StringComparison.OrdinalIgnoreCase)))
            {
                RepositoryResponse response = _manageOrganization.CreateOrganizationDomain(new OrganizationDomain() { Name = organization.OrganizationDomain });
                if (response.Id > 0)
                {
                    organization.OrganizationDomainId = Convert.ToInt32(response.Id);
                }
            }
            else
            {
                organization.OrganizationDomainId = organizationDomainList.Find(p => p.Name.Equals(organization.OrganizationDomain, StringComparison.OrdinalIgnoreCase)).OrganizationDomainId;
            }

            var addProductList = new List<int>();
            // verify the products, if any, exist and can be added to the customer
            List<string> invalidProductList = _manageOrganization.ParseProduct(organization.Products, addProductList);

            if (invalidProductList.Count > 0)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "An invalid product was given : " + String.Join(",", invalidProductList));
            }

            var result = _manageOrganization.CreateOrganization(organization, addProductList, processBlueBookMessage);

            if (!result.Status.Success)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, result.Status.ErrorMsg);
            }

            IList<CustomerCompanyMap> companyMapResource = null;
            try
            {
                companyMapResource = _manageBlueBook.GetCompanyMap(companyRealPageId: result.obj.Org.RealPageId, booksCompanyMasterId: (long)organization.BooksCustomerMasterId, source: ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform), domain: result.obj.Org.OrganizationDomain.Name, includeGreenBookCares: false);
            }
            catch (Exception)
            {
                // ignored
            }

            var companyTypeName = _manageOrganization.ListOrganizationType()?.Find(t => t.OrganizationTypeId == organization.OrganizationTypeId)?.Name;
            // add the new company to books
            var companyInstance = new CompanyInstanceAdd()
            {
                Id = (long)organization.BooksCustomerMasterId,
                CustomerCompanyId = null,
                CompanyInstanceSourceId = result.obj.Org.RealPageId.ToString().ToLower(),
                CompanyName = result.obj.Org.Name,
                Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                IsActive = organization.IsActive == 1,
                ModifiedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation",
                CustomerEnvironment = result.obj.Org.OrganizationDomain.Name,
                CompanyType = companyTypeName
            };

            if (organization.CompanyAddress != null)
            {
                CompanyInstanceAddress address = new CompanyInstanceAddress()
                {
                    Address = organization.CompanyAddress.Address,
                    City = organization.CompanyAddress.City,
                    State = organization.CompanyAddress.State,
                    PostalCode = organization.CompanyAddress.PostalCode,
                    County = organization.CompanyAddress.County,
                    Country = organization.CompanyAddress.Country
                };
                companyInstance.CompanyInstanceLocation = new List<CompanyInstanceAddress>() { address };
            }

            if (!string.IsNullOrEmpty(organization.CompanyInstancePartner) && !string.IsNullOrEmpty(organization.CompanyInstancePartnerSourceId))
            {
                companyInstance.CompanyInstancePartners = new List<CompanyInstancePartner>() { new CompanyInstancePartner() { TargetSource = organization.CompanyInstancePartner, TargetCompanyInstanceSourceId = organization.CompanyInstancePartnerSourceId } };
            }

            bool addInstance = true;

            if (companyMapResource != null)
            {
                // remove any existing instance and add a new one
                foreach (var customerCompanyMap in companyMapResource)
                {
                    customerCompanyMap.CompanyInstance?.ForEach(i =>
                    {
                        if (i.CustomerEnvironment != null && i.CustomerEnvironment.Equals(companyInstance.CustomerEnvironment, StringComparison.OrdinalIgnoreCase))
                        {
                            addInstance = false;
                        }

                        if (i.Domain != null && i.Domain.Equals(companyInstance.CustomerEnvironment, StringComparison.OrdinalIgnoreCase))
                        {
                            addInstance = false;
                        }
                    });
                }
            }

            // add the new company data to books
            if (addInstance)
            {
                var companyCreatedSuccessfully = _manageBlueBook.AddUPFMCompanyFromCompanySetup(companyInstance);

                if (!companyCreatedSuccessfully)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There was a problem adding the UPFM instance to UDM");
                }

                if (!_manageOrganization.AddUpdateCompanyToUnifiedSettings(companyInstance.CompanyInstanceSourceId, "Create", companyInstance.CustomerEnvironment))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, $"Unified Login and MDM company was updated successfully but Settings data update failed.");
                }

                // add the products assigned to the new company
                var cacheKey = $"getListProductsByOrganization_{result.obj.Org.RealPageId}";
                MemoryCache.Default.Remove(cacheKey);

                IList<ProductUI> productList = _manageProduct.GetProducts(result.obj.Org.RealPageId, 0, true);
                foreach (var product in productList)
                {
                    var productInternalSettings = _manageProduct.GetProductInternalSettings(product.ProductId);
                    var updateInUDM = productInternalSettings.Find(x => x.Name.Equals("UpdateProductInUDM", StringComparison.OrdinalIgnoreCase));

                    if (updateInUDM?.Value == "1")
                    {
                        SystemProductCenter spc = new SystemProductCenter()
                        {
                            Id = 0,
                            CompanyInstanceSourceId = companyInstance.CompanyInstanceSourceId,
                            CreatedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation",
                            ProductCenterSourceId = product.ProductId.ToString(),
                            Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)
                        };
                        _manageBlueBook.ProductCenterEnable(spc);
                    }
                }
            }

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
            CompanyLocation oldAddress = null;

            if (organization != null)
            {
                // get the org by UL realpageID
                org = _manageOrganization.GetOrganization(organization.RealPageId);
                oldAddress = _manageOrganization.GetCompanyList(org.Name, 0, null, 0, new Dictionary<object, object>())?.FirstOrDefault()?.CompanyLocation;
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Not found");
            }

            if (org == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Not found");
            }

            bool orgNameChanged = org.Name != organization.Name;
            bool orgStatusChanged = org.IsActive != organization.IsActive;
            bool orgTypeChanged = org.OrganizationTypeId != organization.OrganizationTypeId;
            bool orgAddressChanged = false;

            //Did the address change
            if (organization.CompanyAddress != null &&
                (oldAddress == null ||
                 organization.CompanyAddress.Address.Equals(oldAddress.Address, StringComparison.OrdinalIgnoreCase) ||
                 organization.CompanyAddress.City.Equals(oldAddress.City, StringComparison.OrdinalIgnoreCase) ||
                 organization.CompanyAddress.County.Equals(oldAddress.County, StringComparison.OrdinalIgnoreCase) ||
                 organization.CompanyAddress.Country.Equals(oldAddress.Country, StringComparison.OrdinalIgnoreCase) ||
                 organization.CompanyAddress.State.Equals(oldAddress.State, StringComparison.OrdinalIgnoreCase) ||
                 organization.CompanyAddress.PostalCode.Equals(oldAddress.PostalCode, StringComparison.OrdinalIgnoreCase)))
            {
                orgAddressChanged = true;
            }


            org.Name = organization.Name;
            org.IsActive = organization.IsActive;
            org.EnableEnterpriseRoles = organization.EnableEnterpriseRoles;
            org.EnablePrimaryProperties = organization.EnablePrimaryProperties;

            var orgTypes = _manageOrganization.ListOrganizationType();
            if (organization.OrganizationTypeId != 0)
            {
                if (!orgTypes.Exists(o => o.OrganizationTypeId == organization.OrganizationTypeId))
                {
                    // error
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid organization type id");
                }
            }
            else if (!string.IsNullOrEmpty(organization.OrganizationTypeName))
            {
                if (orgTypes.Exists(o => o.Name.Equals(organization.OrganizationTypeName, StringComparison.OrdinalIgnoreCase)))
                {
                    organization.OrganizationTypeId = orgTypes.Find(o => o.Name.Equals(organization.OrganizationTypeName, StringComparison.OrdinalIgnoreCase)).OrganizationTypeId;
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
                if (orgDomains.TrueForAll(o => o.OrganizationDomainId != organization.OrganizationDomainId))
                {
                    // error
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid organization domain id");
                }
            }
            else if (!string.IsNullOrEmpty(organization.OrganizationDomainName))
            {
                if (orgDomains.Exists(o => o.Name.Equals(organization.OrganizationDomainName, StringComparison.OrdinalIgnoreCase)))
                {
                    organization.OrganizationDomainId = orgDomains.Find(o => o.Name.Equals(organization.OrganizationDomainName, StringComparison.OrdinalIgnoreCase)).OrganizationDomainId;
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

            org.OrganizationTypeId = organization.OrganizationTypeId;
            org.organizationType = _manageOrganization.ListOrganizationType()?.Find(t => t.OrganizationTypeId == organization.OrganizationTypeId);
            org.OrganizationDomain.OrganizationDomainId = organization.OrganizationDomainId;
            org.ThirdPartyIDP = organization.ThirdPartyIDP;

            _repositoryResponse = _manageOrganization.UpdateOrganization(org);

            if (!String.IsNullOrEmpty(_repositoryResponse.ErrorMessage))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, _repositoryResponse.ErrorMessage);
            }

            _manageOrganization.UpdateOrganizationUsePrimaryPropertySetting(org);
            _manageOrganization.UpdateOrganizationThirdPartyIDP(org);

            if (orgNameChanged || orgStatusChanged || orgAddressChanged || orgTypeChanged)
            {
                // update the name in MDM
                IList<CustomerCompanyMap> companyMapResource = null;
                companyMapResource = _manageBlueBook.GetCompanyMap(companyRealPageId: org.RealPageId, booksCompanyMasterId: org.BooksCustomerMasterId, source: ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform), domain: organization.OrganizationDomainName, includeGreenBookCares: false);

                if (companyMapResource != null && companyMapResource.Any(c => c.CompanyInstanceSourceId == org.RealPageId.ToString()))
                {
                    var companyMap = companyMapResource.FirstOrDefault(c => c.CompanyInstanceSourceId == org.RealPageId.ToString());
                    var companyTypeName = _manageOrganization.ListOrganizationType()?.Find(t => t.OrganizationTypeId == organization.OrganizationTypeId)?.Name;

                    CompanyInstanceAdd updateCompanyInstance = new CompanyInstanceAdd()
                    {
                        CompanyInstanceId = null,
                        CompanyInstanceSourceId = companyMap.CompanyInstanceSourceId,
                        CompanyName = org.Name,
                        CustomerCompanyId = null,
                        IsActive = organization.IsActive == 1,
                        Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                        CustomerEnvironment = null,
                        ModifiedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation",
                        CompanyType = companyTypeName
                    };

                    if (organization.CompanyAddress != null)
                    {
                        CompanyInstanceAddress address = new CompanyInstanceAddress()
                        {
                            Address = organization.CompanyAddress.Address,
                            City = organization.CompanyAddress.City,
                            State = organization.CompanyAddress.State,
                            PostalCode = organization.CompanyAddress.PostalCode,
                            County = organization.CompanyAddress.County,
                            Country = organization.CompanyAddress.Country
                        };
                        updateCompanyInstance.CompanyInstanceLocation = new List<CompanyInstanceAddress>() { address };
                    }

                    var booksResult = _manageBlueBook.UpdateBooksGreenBookCompanyInstance(updateCompanyInstance, oldAddress);
                    if (!string.IsNullOrEmpty(booksResult))
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, $"Unified Login company was updated successfully but MDM data update failed. Error: " + booksResult);
                    }
                    else
                    {
                        if (!_manageOrganization.AddUpdateCompanyToUnifiedSettings(org.RealPageId.ToString(), "Update", null))
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, $"Unified Login and MDM company was updated successfully but Settings data update failed.");
                        }
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, $"Unified Login company was updated successfully but MDM data failed because the {ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)} company instance could not be found");
                }
            }

            org = _manageOrganization.GetOrganization(org.RealPageId);

            return Request.CreateResponse(HttpStatusCode.OK, org);
        }


        /// <summary>
        /// Used to get details for the given Organization id
        /// </summary>
        /// <param name="realPageId">The unique identifier for the organization</param>
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
                foreach (var organization in organizationList)
                {
                    var partyRelationship = _managePartyRelationship.GetPartyRelationship(realPageId, organization.RealPageId, roleTypeFrom, roleTypeTo, relationshipType);
                    if (partyRelationship != null)
                    {
                        organization.partyRelationship = partyRelationship;
                    }
                }

                ObjectListOutput<Organization, IErrorData> output = new ObjectListOutput<Organization, IErrorData>() { list = organizationList };
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }

            //When trying to get a list of Organization(s) for a Person that doesn't exists
            return Request.CreateResponse(HttpStatusCode.NoContent, "Invalid realPageId");
        }

        /// <summary>
        /// List organization product(s)
        /// </summary>
        /// <param name="realPageId">The unique identifier for the organization</param>
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

            long personaId = 0;

            if (mergePersonaAccess.HasValue && mergePersonaAccess.Value)
            {
                IManagePersona managePersona = new ManagePersona();
                var persona = managePersona.GetPersona(_userClaims.PersonaId);

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

            CheckPasswordExpirationResponse checkPasswordExpirationResponse = _manageCredential.CheckPasswordExpiration(_userClaims.UserId, _userClaims.UserRealPageGuid);
            if (checkPasswordExpirationResponse != null && !checkPasswordExpirationResponse.IsPasswordExpired)
            {
                var cacheKey = $"getListProductsByOrganization_{org.RealPageId}";
                MemoryCache.Default.Remove(cacheKey);

                IList<ProductUI> productList = _manageProduct.GetProducts(realPageId: org.RealPageId, personaId: personaId, allProducts: (allProducts.HasValue ? allProducts.Value : false), replaceProductCodeWithUDMIfExists: false);
                if (allProducts.HasValue && allProducts.Value)
                {
                    output.list = _manageProduct.AddProductSourceAndGreenBookCareFlagToProducts(org.RealPageId, org.PartyId, productList);
                }
                else
                {
                    output.list = productList;
                }
            }

            output.Status = errorStatus;

            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// Add/Updates products to an organization
        /// </summary>
        /// <param name="realPageId">The unique identifier for the organization</param>
        /// <param name="enableDisableProducts">A list of BlueBook product names. i.e. </param>     
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.Created, Description = "Success")]
        [Route("organization/{realPageId}/product")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage AddProductToOrganization([FromUri] Guid realPageId, [FromBody] EnableDisableProducts enableDisableProducts)
        {
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            if (realPageId == Guid.Empty)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "200.3";
                errorStatus.ErrorMsg = "Invalid parameter: realPageId";
                return Request.CreateResponse(HttpStatusCode.BadRequest, errorStatus);
            }
            if (enableDisableProducts.AddProducts == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "400";
                errorStatus.ErrorMsg = "Products not found!";
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
            List<int> addProductList = new List<int>();
            List<int> removeProductList = new List<int>();
            // verify the products, if any, exist and can be added to the customer
            List<string> invalidProductList = _manageOrganization.ParseProduct(enableDisableProducts.AddProducts, addProductList);
            if (enableDisableProducts.Removeproducts != null)
            {
       
                _manageOrganization.ParseProduct(enableDisableProducts.Removeproducts, removeProductList);
            }
            if (invalidProductList.Count > 0)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "An invalid product was given : " + String.Join(",", invalidProductList));
            }

            // add the given products to the new company
            if (addProductList.Count > 0)
            {                
               IList<ProductUI> productList = _manageProduct.GetProducts(realPageId: org.RealPageId, personaId: 0, allProducts: true, replaceProductCodeWithUDMIfExists: false);
                _repositoryResponse = _manageOrganizationProduct.CheckSharedProductsEnabled(productList, addProductList, removeProductList);

                if (!string.IsNullOrEmpty(_repositoryResponse.ErrorMessage))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, _repositoryResponse.ErrorMessage);
                }
                _manageOrganization.EnableProductOnOtherProductsActivation(addProductList);
                _repositoryResponse = _manageOrganizationProduct.InsertUpdateOrganizationProduct(org, addProductList);
               
                if (!string.IsNullOrEmpty(_repositoryResponse.ErrorMessage))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, _repositoryResponse.ErrorMessage);
                }
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }


        /// <summary>
        /// Update Use primary properties for products
        /// </summary>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get company details by customer company id", Type = typeof(CompanySetup))]
        [SwaggerResponseExamples(typeof(CompanySetup), typeof(CompanySetupExample))]
        [Route("companysetup/party/{organizationPartyId}/product/{productId}/usePrimaryProperty/{usePrimaryProperty}")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [HttpPut]
        public HttpResponseMessage UpdateUsePrimaryPropertyForOrganizationProduct(long organizationPartyId, int productId, bool usePrimaryProperty)
        {
            if (organizationPartyId == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "organizationPartyId not supplied");
            }

            if (productId == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "productId not supplied");
            }

            _repositoryResponse = _manageOrganization.UpdateUsePrimaryPropertyForOrganizationProduct(organizationPartyId, productId, usePrimaryProperty);
            if (_repositoryResponse.Id == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, _repositoryResponse.ErrorMessage);
            }

            return Request.CreateResponse(HttpStatusCode.OK, _repositoryResponse);
        }

        /// <summary>
        /// Remove products from an organization
        /// </summary>
        /// <param name="realPageId">The unique identifier for the organization</param>
        /// <param name="enableDisableProducts"></param>    
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Success")]
        [Route("organization/{realPageId}/product")]
        [Authorize]
        [HttpDelete]
        public HttpResponseMessage DeleteProductFromOrganization([FromUri] Guid realPageId, [FromBody] EnableDisableProducts enableDisableProducts)
        {
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            if (realPageId == Guid.Empty)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "200.3";
                errorStatus.ErrorMsg = "Invalid parameter: realPageId";
                return Request.CreateResponse(HttpStatusCode.BadRequest, errorStatus);
            }
            if (enableDisableProducts.Removeproducts == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "400";
                errorStatus.ErrorMsg = "Products not found!";
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

            List<int> unassignProductList = new List<int>();
            List<int> assignProductList = new List<int>();
            //Validate Products
            List<string> invalidProductList = _manageOrganization.ParseProduct(enableDisableProducts.Removeproducts, unassignProductList);
            if(enableDisableProducts.AddProducts != null )
            _manageOrganization.ParseProduct(enableDisableProducts.AddProducts, assignProductList);
            if (invalidProductList.Count > 0)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "An invalid product was given : " + String.Join(",", invalidProductList));
            }

            // Delete the given products to the company
            if (unassignProductList.Count > 0)
            {
                IList<ProductUI> productList = _manageProduct.GetProducts(realPageId: org.RealPageId, personaId: 0, allProducts: true, replaceProductCodeWithUDMIfExists: false);
                _repositoryResponse = _manageOrganizationProduct.CheckSharedProductsEnabled(productList, assignProductList, unassignProductList);

                if (!string.IsNullOrEmpty(_repositoryResponse.ErrorMessage))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, _repositoryResponse.ErrorMessage);
                }
                _repositoryResponse = _manageOrganizationProduct.DeleteProductsFromOrganization(unassignProductList, org);
                if (!string.IsNullOrEmpty(_repositoryResponse.ErrorMessage))
                {
                    Request.CreateErrorResponse(HttpStatusCode.BadRequest, _repositoryResponse.ErrorMessage);
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
            MemoryCache.Default.Remove("getListOrganizationDomain");
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

        #region Get Organizations

        /// <summary>
        /// Get List of Organizations
        /// </summary>
        /// <param name="organizationName">OrganizationName</param>
        /// <param name="domain">Domain</param>
        /// <param name="blueId">BlueId</param>
        /// <param name="organizationId">organizationId</param>
        /// <param name="datafilter">datafilter</param>
        /// <returns>List of Organizations</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about a list of Organizations", Type = typeof(CompanySetup))]
        [SwaggerResponseExamples(typeof(CompanySetup), typeof(CompanySetupExample))]
        [Route("CompanySetup")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [HttpGet]
        public HttpResponseMessage GetCompanyList(string organizationName = null, int? domain = null, int? blueId = null, int? organizationId = null, [FromUri] RequestParameter datafilter = null)
        {
            IDictionary<object, object> globals = new Dictionary<object, object>();
            ObjectListOutput<CompanySetup, IErrorData> output = new ObjectListOutput<CompanySetup, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            if (datafilter == null)
            {
                datafilter = new RequestParameter();
            }

            globals.Add(BaseType.RequestParameter, datafilter);

            List<CompanySetup> companyList = _manageOrganization.GetCompanyList(organizationName, domain ?? 0, blueId, organizationId ?? 0, globals);

            int totalRecords = companyList.Count > 0 ? companyList[0].TotalRecords : 0;
            decimal resultsPerPage = ((datafilter.Pages.ResultsPerPage == 100) && (totalRecords > 0)) ? totalRecords : datafilter.Pages.ResultsPerPage;
            resultsPerPage = (resultsPerPage == 0) ? totalRecords : resultsPerPage;
            PagingSummary pagingSummary = new PagingSummary()
            {
                TotalRecords = totalRecords,
                TotalPages = (resultsPerPage == 0) ? 0 : (int)Math.Ceiling(totalRecords / resultsPerPage)
            };
            output = new ObjectListOutput<CompanySetup, IErrorData>() { list = companyList, Status = errorStatus };
            output.pagingSummary = pagingSummary;
            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// Search Company By CustomerCompanyId
        /// </summary>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get company details by customer company id", Type = typeof(CompanySetup))]
        [SwaggerResponseExamples(typeof(CompanySetup), typeof(CompanySetupExample))]
        [Route("companysetup/companymaster/{customerCompanyId}")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [HttpGet]
        public HttpResponseMessage GetCompanyMasterByCustomerCompanyId(long customerCompanyId)
        {
            if (customerCompanyId == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "CompanyMasterId not supplied");
            }

            var companyMaster = _manageOrganization.SearchCompanyDetailsByCustomerCompanyId(customerCompanyId);


            return Request.CreateResponse(HttpStatusCode.OK, companyMaster);
        }

        /// <summary>
        /// Export Companies
        /// </summary>
        /// <param name="organizationName">OrganizationName</param>
        /// <param name="domain">Domain</param>
        /// <param name="blueId">BlueId</param>
        /// <param name="organizationId">organizationId</param>
        /// <param name="datafilter">Filter, Sort, Paginate</param>
        /// <param name="dataFormat">Return data in this format (default = CSV)</param>
        /// <returns>List of Companies object</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about a list of Companies", Type = typeof(CompanySetup))]
        [SwaggerResponseExamples(typeof(CompanySetup), typeof(CompanySetupExample))]
        [Route("CompanySetup/export")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [HttpGet]
        public HttpResponseMessage ListCompanyExport(string organizationName = null, int? domain = null, int? blueId = null, int? organizationId = null, [FromUri] RequestParameter datafilter = null, SaveFormat dataFormat = SaveFormat.CSV)
        {
            byte[] plainBytes;
            IDictionary<object, object> globals = new Dictionary<object, object>();
            ObjectOutput<string, IErrorData> output = new ObjectOutput<string, IErrorData>();

            Status<IErrorData> errorStatus = new Status<IErrorData>();

            if (datafilter == null)
            {
                datafilter = new RequestParameter();
            }

            globals.Add(BaseType.RequestParameter, datafilter);

            List<CompanySetup> companyList = _manageOrganization.GetCompanyList(organizationName, domain ?? 0, blueId, organizationId ?? 0, globals);

            if (companyList != null)
            {
                errorStatus = DataExport.SetAsposeLicense();
                if (errorStatus.Success)
                {
                    List<ExportDataFileConfiguration> exportConfigurations = new List<ExportDataFileConfiguration>
                    {
                        new ExportDataFileConfiguration { Header = "UPFM Company Name", MappedField = "OrganizationName", PDFColumnWidth = "2.85", Preference = 1 },
                        new ExportDataFileConfiguration { Header = "Contracted Name", MappedField = "ContractedName", PDFColumnWidth = "2.85", Preference = 2 },
                        new ExportDataFileConfiguration { Header = "Domain", MappedField = "Domain", PDFColumnWidth = "0.85", Preference = 3 },
                        new ExportDataFileConfiguration { Header = "Address", MappedField = "Address", PDFColumnWidth = "3.25", Preference = 4 },
                        new ExportDataFileConfiguration { Header = "RPUP ID", MappedField = "BooksCustomerMasterId", PDFColumnWidth = "0.70", Preference = 5 },
                        new ExportDataFileConfiguration { Header = "Type", MappedField = "OrganizationType", PDFColumnWidth = "1.00", Preference = 6 },
                        new ExportDataFileConfiguration { Header = "Products", MappedField = "Products", PDFColumnWidth = "0.50", Preference = 7 },
                        new ExportDataFileConfiguration { Header = "UPFM Company ID", MappedField = "RealPageId", PDFColumnWidth = "3.25", Preference = 8 },
                        new ExportDataFileConfiguration { Header = "Status", MappedField = "Status", PDFColumnWidth = "2.25", Preference = 9 }
                    };

                    plainBytes = DataExport.ExportDataToFile<CompanySetup>(exportConfigurations.OrderBy(p => p.Preference).ToList(), companyList, dataFormat);
                    output = new ObjectOutput<string, IErrorData>()
                    {
                        obj = Convert.ToBase64String(plainBytes),
                        Status = errorStatus
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, output);
                }
                else
                {
                    output.Status = errorStatus;
                    return Request.CreateResponse(HttpStatusCode.OK, output);
                }
            }
            else
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Company.ListCompanyExport.1";
                errorStatus.ErrorMsg = "List Company Export: No data";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
        }

        #endregion

        #region Property

        #region Get Properties for a Organization

        /// <summary>
        /// Get Properties for a Organization
        /// </summary>
        /// <param name="companyInstanceId">companyInstanceId</param>
        /// <param name="propertyName">PropertyName</param>
        /// <param name="domain">Domain</param>
        /// <param name="blueId">blueId</param>
        /// <param name="status"></param>
        /// <param name="datafilter">datafilter</param>
        /// <param name="userPersonaId">userPersonaId</param>
        /// <param name="editorPersonaId">editorPersonaId</param>
        /// <param name="isSelectedProperties">isSelectedProperties</param>
        /// <param name="selectedProperties"></param>
        /// <returns>List of Properties for a company </returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about a list of Properties for an Organization", Type = typeof(CompanyPropertySetup))]
        [SwaggerResponseExamples(typeof(CompanyPropertySetup), typeof(PropertyListExample))]
        [Route("CompanySetup/CompanyPropertyList")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [HttpPost]
        public HttpResponseMessage GetPropertiesForCompany(Guid companyInstanceId, [FromBody] List<Guid> selectedProperties, string domain = null, string propertyName = null, int? blueId = null, int? status = null, [FromUri] RequestParameter datafilter = null, long userPersonaId = 0, long editorPersonaId = 0, bool? isSelectedProperties = null, [FromUri] string operatorCode = null, [FromUri] string operatorValue = null)
        {
            if (companyInstanceId == Guid.Empty)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Company Instance Id not supplied");
            }

            IDictionary<object, object> globals = new Dictionary<object, object>();
            ObjectListOutput<CompanyPropertySetup, IErrorData> output = new ObjectListOutput<CompanyPropertySetup, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            if (datafilter == null)
            {
                datafilter = new RequestParameter();
            }

            globals.Add(BaseType.RequestParameter, datafilter);
            var cacheKey = $"getPropertyInstanceForCompany_{companyInstanceId}";

            if (!FeatureFlag.GetUserCompanyAssociationFeatureFlag())
            {
                operatorCode = null;
                operatorValue = null;
            }
            else
            {
                if (string.IsNullOrEmpty(operatorCode) && string.IsNullOrEmpty(operatorValue))
                {
                    cacheKey = $"getPropertyInstanceForCompanyByOperatorId_{companyInstanceId}_{operatorCode}_{operatorValue}";
                }
            }

            RPObjectCache.RemoveFromCache(cacheKey);
            List<CompanyPropertySetup> companyPropertySetup = _manageOrganization.GetPropertiesForCompany(companyInstanceId, propertyName, domain, blueId, status, globals, editorPersonaId, userPersonaId, isSelectedProperties, selectedProperties, operatorCode, operatorValue);

            int totalRecords = 0;
            if (companyPropertySetup.Count > 0)
            {
                totalRecords = companyPropertySetup[0]?.Property.Count > 0 ? companyPropertySetup[0].Property[0].TotalRecords : 0;
            }

            decimal resultsPerPage = ((datafilter.Pages.ResultsPerPage == 100) && (totalRecords > 0)) ? totalRecords : datafilter.Pages.ResultsPerPage;
            resultsPerPage = (resultsPerPage == 0) ? totalRecords : resultsPerPage;
            PagingSummary pagingSummary = new PagingSummary()
            {
                TotalRecords = totalRecords,
                TotalPages = (resultsPerPage == 0) ? 0 : (int)Math.Ceiling(totalRecords / resultsPerPage)
            };
            output = new ObjectListOutput<CompanyPropertySetup, IErrorData>() { list = companyPropertySetup, Status = errorStatus };
            output.pagingSummary = pagingSummary;
            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        #endregion

        #region AuditCompanyProperties

        /// <summary>
        /// Audit the given product properties to UPFM properties
        /// </summary>
        /// <param name="companyInstanceId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the properties for the given company and their mapping to UPFM instances", Type = typeof(PropertyAudit))]
        [SwaggerResponseExamples(typeof(PropertyAudit), typeof(PropertyAuditExample))]
        [Route("CompanySetup/{companyInstanceId}/product/{productId}/audit")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [HttpGet]
        public HttpResponseMessage AuditCompanyProductPropertiesToUPFM(Guid companyInstanceId, int productId)
        {
            if (companyInstanceId == Guid.Empty)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Company Instance Id not supplied");
            }

            Status<IErrorData> errorStatus = new Status<IErrorData>();
            var currentClaimPrincipal = ClaimsPrincipal.Current;


            if (currentClaimPrincipal.HasClaim("scope", "internalapi"))
            {
                var adminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(companyInstanceId);
                //recreate clams
                if (adminCreatorRealPageId == Guid.Empty)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Admin Creator RealPageId is empty.");
                }
                RecreateClaimsForClient(adminCreatorRealPageId);
                // need to alter the user being used to match the company or the product calls will not have the correct context
            }
            else if (_userClaims.OrganizationRealPageGuid != EmployeeCompanyRealPageId)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid company context");
            }

            var orgDetails = _manageOrganization.GetOrganization(companyInstanceId);

            if (orgDetails == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid company");
            }

            _userClaims.CustomerMasterId = orgDetails.BooksCustomerMasterId;
            _userClaims.OrganizationMasterId = orgDetails.BooksMasterId;
            _userClaims.OrganizationName = orgDetails.Name;
            _userClaims.OrganizationPartyId = orgDetails.PartyId;
            _userClaims.OrganizationRealPageGuid = orgDetails.RealPageId;

            var adminUserGuid = _manageOrganization.GetOrganizationAdminUserRealPageId(orgDetails.RealPageId);
            if (adminUserGuid == Guid.Empty)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Unable to locate company user");
            }

            _userClaims.UserRealPageGuid = adminUserGuid;
            var auditResult = GetAuditProductProperties(companyInstanceId, productId, adminUserGuid, orgDetails.PartyId);
            ObjectListOutput<PropertyAudit, IErrorData> output = new ObjectListOutput<PropertyAudit, IErrorData> { list = auditResult, Status = errorStatus, pagingSummary = new PagingSummary() { TotalRecords = auditResult.Count, TotalPages = 1 } };
            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        public void RecreateClaimsForClient(Guid _realpageUserId)
        {
            if (!string.IsNullOrEmpty(_realpageUserId.ToString()))
            {

                var person = _managePerson.GetPerson(_realpageUserId);
                if (person == null)
                {
                    throw new Exception($"Missing persona information for client_info user while Recreation of Claims For Client.  realPageId: {_realpageUserId}");
                }
                var userLogin = _manageUserLogin.GetUserLoginOnly(_realpageUserId);

                //Active Persona is linked to one organization
                var persona = _managePersona.GetActivePersonaWithoutRights(_realpageUserId); // this user can only be under 1 company to work correctly

                _userClaims = new DefaultUserClaim
                {
                    UserId = (int)userLogin.UserId,
                    OrganizationPartyId = persona.Organization.PartyId,
                    LoginName = userLogin.LoginName,
                    OrganizationMasterId = (long)persona.Organization.BooksMasterId,
                    CustomerMasterId = (long)persona.Organization.BooksMasterId,
                    OrganizationName = persona.Organization.Name.ToString(),
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    PersonaId = persona.PersonaId,
                    OrganizationRealPageGuid = persona.Organization.RealPageId,
                    UserRealPageGuid = _realpageUserId,
                    CorrelationId = Guid.NewGuid(),
                    RealPageEmployee = persona.Organization.Name.ToUpper() == "REALPAGE EMPLOYEE"
                };
            }
        }
        /// <summary>
        /// Audit the given product properties to UPFM properties to export
        /// </summary>
        /// <param name="companyInstanceId"></param>
        /// <param name="productId"></param>
        /// <param name="dataFormat">Return data in this format (default = CSV)</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the properties for the given company and their mapping to UPFM instances", Type = typeof(PropertyAudit))]
        [SwaggerResponseExamples(typeof(PropertyAudit), typeof(PropertyAuditExample))]
        [Route("CompanySetup/{companyInstanceId}/product/{productId}/audit/export")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [HttpGet]
        public HttpResponseMessage AuditCompanyProductPropertiesToUPFMExport(Guid companyInstanceId, int productId, SaveFormat dataFormat = SaveFormat.CSV)
        {
            byte[] plainBytes;
            ObjectOutput<string, IErrorData> output = new ObjectOutput<string, IErrorData>();

            Status<IErrorData> errorStatus = new Status<IErrorData>();
            if (companyInstanceId == Guid.Empty)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Company Instance Id not supplied");
            }

            // need to alter the user being used to match the company or the product calls will not have the correct context
            if (_userClaims.OrganizationRealPageGuid != EmployeeCompanyRealPageId)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid company context");
            }

            var orgDetails = _manageOrganization.GetOrganization(companyInstanceId);

            if (orgDetails == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid company");
            }

            _userClaims.CustomerMasterId = orgDetails.BooksCustomerMasterId;
            _userClaims.OrganizationMasterId = orgDetails.BooksMasterId;
            _userClaims.OrganizationName = orgDetails.Name;
            _userClaims.OrganizationPartyId = orgDetails.PartyId;
            _userClaims.OrganizationRealPageGuid = orgDetails.RealPageId;

            var adminUserGuid = _manageOrganization.GetOrganizationAdminUserRealPageId(orgDetails.RealPageId);
            if (adminUserGuid == Guid.Empty)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Unable to locate company user");
            }

            _userClaims.UserRealPageGuid = adminUserGuid;
            List<PropertyAudit> propertyAudit = GetAuditProductProperties(companyInstanceId, productId, adminUserGuid, orgDetails.PartyId);

            if (propertyAudit != null)
            {
                errorStatus = DataExport.SetAsposeLicense();
                if (errorStatus.Success)
                {
                    List<ExportDataFileConfiguration> exportConfigurations = new List<ExportDataFileConfiguration>
                    {
                        new ExportDataFileConfiguration { Header = "Property", MappedField = "Name", PDFColumnWidth = "3.85", Preference = 1 },
                        new ExportDataFileConfiguration { Header = "Instance ID", MappedField = "ProductInstanceId", PDFColumnWidth = "2.85", Preference = 2 },
                        new ExportDataFileConfiguration { Header = "Platform Property name", MappedField = "UPFMName", PDFColumnWidth = "3.25", Preference = 3 },
                        new ExportDataFileConfiguration { Header = "Status", MappedField = "Status", PDFColumnWidth = "1.25", Preference = 4 }
                    };
                    plainBytes = DataExport.ExportDataToFile<PropertyAudit>(exportConfigurations.OrderBy(p => p.Preference).ToList(), propertyAudit, dataFormat);
                    output = new ObjectOutput<string, IErrorData>()
                    {
                        obj = Convert.ToBase64String(plainBytes),
                        Status = errorStatus
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, output);
                }
                else
                {
                    output.Status = errorStatus;
                    return Request.CreateResponse(HttpStatusCode.OK, output);
                }
            }
            else
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Company.ListPropertyAuditExport.1";
                errorStatus.ErrorMsg = "List Property Audit Export: No data";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
        }

        #endregion

        #region Update Property

        /// <summary>
        ///Update Properties for a Organization
        /// </summary>
        /// <param name="propertyList">properties Object</param>
        /// <param name="companyInstanceId">companyInstanceId</param>
        /// <param name="isFromBulkPropertyUpdate"></param>     
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [Route("CompanySetup/CompanyPropertyList")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [HttpPut]
        public async Task<HttpResponseMessage> UpdatePropertyForOrganization([FromBody] List<UPFMPropertyInstance> propertyList, Guid companyInstanceId, bool isFromBulkPropertyUpdate = false)
        {
            if (companyInstanceId == Guid.Empty)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: companyInstanceId");
            }

            if (propertyList.Any(m => m.InstanceId == Guid.Empty))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: propertyInstanceId");
            }

            if (propertyList.Any(m => m.Name == string.Empty))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: propertyName");
            }

            if (!isFromBulkPropertyUpdate)
            {
                var options = new ParallelOptions() { MaxDegreeOfParallelism = _maxDOPSetting };
                Parallel.ForEach(propertyList, options, async (property, cancelToken) =>
                {
                    var manageOrganization = new ManageOrganization(_userClaims);
                    _repositoryResponse = await manageOrganization.ProcessPropertyList(property, companyInstanceId);
                });
                await Task.WhenAll();
            }
            else
            {
                var manageOrganization = new ManageOrganization(_userClaims);
                List<UPFMPropertyInstance> oldPropertyList = manageOrganization.GetPropertiesByInstanceId(propertyList.Select(m => m.InstanceId).ToList());
                _repositoryResponse = await manageOrganization.UpdatePropertyList(propertyList, companyInstanceId);
                if (_repositoryResponse.Id > 0)
                {                    
                    _ = Task.Run(() =>
                    {
                        var options = new ParallelOptions() { MaxDegreeOfParallelism = _maxDOPSetting };
                        Parallel.ForEach(propertyList, options, property =>
                        {
                            var orgManager = new ManageOrganization(_userClaims);
                            orgManager.UpdatePropertyInSettingsAndActivityLogs(property, companyInstanceId, oldPropertyList);
                        });
                    });
                }
            }


            if (_repositoryResponse.Id == 0 || !string.IsNullOrEmpty(_repositoryResponse.ErrorMessage))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, _repositoryResponse.ErrorMessage);
            }
            var instancesList = propertyList.Select(m => m.InstanceId);
            return Request.CreateResponse(HttpStatusCode.OK, instancesList);
        }

        /// <summary>
        ///Update Properties for a Organization (Action from UDM)
        /// </summary>
        /// <param name="property">property Object</param>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [Route("CompanySetup/CompanyPropertyList/UDM")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [HttpPut]
        public HttpResponseMessage UpdateUDMPropertyForOrganization([FromBody] UPFMPropertyInstance property)
        {
            if (String.IsNullOrEmpty(property.CustomerPropertyId))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: CustomerPropertyId");
            }

            if (property.InstanceId == Guid.Empty)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: propertyInstanceId");
            }

            if (String.IsNullOrEmpty(property.Name))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: propertyName");
            }

            var companyInstances = _manageBlueBook.GetCompanyInstancesByCustomerCompanyId(property.CustomerCompanyId);
            string companyInstanceId = string.Empty;
            foreach (var instance in companyInstances)
            {
                var attributes = instance?.attributes;
                if (attributes != null && attributes.Domain == property.Domain)
                {
                    companyInstanceId = instance.attributes.companyInstanceSourceId;
                }
            }

            var currentProperty = _manageOrganization.GetPropertyByInstanceId(property.InstanceId).FirstOrDefault();


            if (currentProperty != null && !string.IsNullOrEmpty(companyInstanceId))
            {
                currentProperty.IsActive = property.IsActive;
                currentProperty.Name = property.Name;
                _repositoryResponse = _manageOrganization.UpdateProperty(property, new Guid(companyInstanceId));
                if (_repositoryResponse.Id == 0 || !string.IsNullOrEmpty(_repositoryResponse.ErrorMessage))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, _repositoryResponse.ErrorMessage);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, property.InstanceId);
        }

        /// <summary>
        /// Export Properties
        /// </summary>
        /// <param name="companyInstanceId">companyInstanceId</param>
        /// <param name="propertyName">propertyName</param>
        /// <param name="domain">domain</param>
        /// <param name="blueId">blueId</param>
        /// <param name="status"></param>
        /// <param name="datafilter">Filter, Sort, Paginate</param>
        /// <param name="dataFormat">Return data in this format (default = CSV)</param>
        /// <returns>List of Properties object</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about a list of Properties", Type = typeof(PropertySetup))]
        [SwaggerResponseExamples(typeof(PropertySetup), typeof(PropertyListExample))]
        [Route("CompanySetup/CompanyPropertyList/export")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [HttpGet]
        public HttpResponseMessage ListPropertyExport(Guid companyInstanceId, string propertyName = null, string domain = null, int? blueId = null, int? status = null, [FromUri] RequestParameter datafilter = null, SaveFormat dataFormat = SaveFormat.CSV)
        {
            byte[] plainBytes;
            IDictionary<object, object> globals = new Dictionary<object, object>();
            ObjectOutput<string, IErrorData> output = new ObjectOutput<string, IErrorData>();

            Status<IErrorData> errorStatus = new Status<IErrorData>();
            if (companyInstanceId == Guid.Empty)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Company Instance Id not supplied");
            }

            if (datafilter == null)
            {
                datafilter = new RequestParameter();
            }

            globals.Add(BaseType.RequestParameter, datafilter);
            var cacheKey = $"getPropertyInstanceForCompany_{companyInstanceId}";
            RPObjectCache.RemoveFromCache(cacheKey);
            List<CompanyPropertySetup> propertyList = _manageOrganization.GetPropertiesForCompany(companyInstanceId, propertyName, domain, blueId, status, globals);

            if (propertyList != null && propertyList.Count > 0)
            {
                errorStatus = DataExport.SetAsposeLicense();
                if (errorStatus.Success)
                {
                    List<ExportDataFileConfiguration> exportConfigurations = new List<ExportDataFileConfiguration>
                    {
                        new ExportDataFileConfiguration { Header = "UPFM Property Name", MappedField = "Name", PDFColumnWidth = "2.85", Preference = 1 },
                        new ExportDataFileConfiguration { Header = "Contracted Name", MappedField = "ContractedName", PDFColumnWidth = "2.85", Preference = 2 },
                        new ExportDataFileConfiguration { Header = "RPUP ID", MappedField = "CustomerPropertyId", PDFColumnWidth = "0.70", Preference = 3 },
                        new ExportDataFileConfiguration { Header = "Domain", MappedField = "Domain", PDFColumnWidth = "0.85", Preference = 4 },
                        new ExportDataFileConfiguration { Header = "Address", MappedField = "PropertyAddress", PDFColumnWidth = "3.25", Preference = 5 },
                        new ExportDataFileConfiguration { Header = "UPFM Property ID", MappedField = "InstanceId", PDFColumnWidth = "3.25", Preference = 6 },
                        new ExportDataFileConfiguration { Header = "Status", MappedField = "IsActive", PDFColumnWidth = "2.25", Preference = 7 }
                    };

                    plainBytes = DataExport.ExportDataToFile<PropertySetup>(exportConfigurations.OrderBy(p => p.Preference).ToList(), propertyList[0]?.Property, dataFormat);
                    output = new ObjectOutput<string, IErrorData>()
                    {
                        obj = Convert.ToBase64String(plainBytes),
                        Status = errorStatus
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, output);
                }
                else
                {
                    output.Status = errorStatus;
                    return Request.CreateResponse(HttpStatusCode.OK, output);
                }
            }
            else
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "CompanySetup.ListPropertyExport.1";
                errorStatus.ErrorMsg = "List Property Export: No data";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
        }

        #endregion

        #region AddPropertyInstance

        /// <summary>
        ///Add Properties for a Organization
        /// </summary>
        /// <param name="property">property</param>
        /// <param name="companyInstanceID">companyInstanceID</param>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [Route("CompanySetup/CompanyProperty")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [HttpPost]
        public HttpResponseMessage AddPropertyForOrganization([FromBody] UPFMPropertyInstance property, Guid companyInstanceID)
        {
            if (companyInstanceID == Guid.Empty)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: companyInstanceID");
            }

            if (property == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: Property Object");
            }

            if (((string.IsNullOrEmpty(property.Name)) || (property.Name.Trim().Length == 0))
                || ((string.IsNullOrEmpty(property.Domain)) || (property.Domain.Trim().Length == 0)))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "PropertyName,Domain should not be empty");
            }

            if ((string.IsNullOrEmpty(property.CustomerPropertyId)) || (property.CustomerPropertyId.Trim().Length == 0))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "CustomerPropertyId should not be empty or null");
            }

            if ((Convert.ToInt64(property.CustomerPropertyId.Trim()) == 0)
                || (Convert.ToInt64(property.CustomerPropertyId.Trim()) <= 0))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "CustomerPropertyId should be greater then zero");
            }

            _repositoryResponse = _manageOrganization.AddPropertyForOrganization(property, companyInstanceID);
            return Request.CreateResponse(HttpStatusCode.OK, _repositoryResponse);
        }

        #endregion

        #region Delete Property

        /// <summary>
        /// Delete Properties for a Organization
        /// </summary>
        /// <param name="propertyInstanceID"></param>
        /// <param name="companyInstanceID"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [Route("CompanySetup/CompanyProperty/propertyinstance/{propertyInstanceID}/{companyInstanceID}")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [HttpDelete]
        public HttpResponseMessage DeleteProperty(Guid propertyInstanceID, Guid companyInstanceID)
        {
            if (propertyInstanceID == Guid.Empty)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: propertyInstanceID");
            }

            _repositoryResponse = _manageOrganization.DeletePropertyForOrganization(propertyInstanceID, companyInstanceID);
            return Request.CreateResponse(HttpStatusCode.OK, _repositoryResponse);
        }

        #endregion

        #region SearchPropertyByBlueId

        /// <summary>
        ///Search Property By BlueId
        /// </summary>
        /// <param name="customerPropertyId">customerPropertyId</param>
        /// <param name="booksCustomerMasterId">booksCustomerMasterId</param>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [Route("CompanySetup/CompanyProperty/propertyinstance/{customerPropertyId}")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [HttpGet]
        public HttpResponseMessage SearchPropertyByBlueId(string customerPropertyId, string booksCustomerMasterId)
        {
            if ((string.IsNullOrEmpty(customerPropertyId)) || (customerPropertyId == "0"))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: companyInstanceID");
            }

            if ((string.IsNullOrEmpty(booksCustomerMasterId)) || (booksCustomerMasterId == "0"))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: companyInstanceID");
            }

            PropertyInstanceSearch _propertySearchList = _manageOrganization.SearchPropertyDetailsByCustomerPropertyId(customerPropertyId, booksCustomerMasterId);
            return Request.CreateResponse(HttpStatusCode.OK, _propertySearchList);
        }

        #endregion

        #region GetProductStatusDetails

        /// <summary>
        /// Get Product Status Details
        /// </summary>
        /// <param name="productInstanceId">productInstanceId</param>
        /// <param name="source">source</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the product", Type = typeof(ProductPropertyDetails))]
        [SwaggerResponseExamples(typeof(ProductPropertyDetails), typeof(ProductPropertyDetailsExample))]
        [Route("CompanySetup/Audit/product/{productInstanceId}/source/{source}/productStatus")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [HttpGet]
        public HttpResponseMessage GetProductStatusDetails(string productInstanceId, string source)
        {
            if ((string.IsNullOrEmpty(productInstanceId)) || (productInstanceId == "0"))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: companyInstanceID");
            }

            if (string.IsNullOrEmpty(source))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: source");
            }

            ProductPropertyDetails _productPropertyDetails = _manageOrganization.GetSourceProductDetails(productInstanceId, source);
            return Request.CreateResponse(HttpStatusCode.OK, _productPropertyDetails);
        }

        #endregion

        #endregion

        /// <summary>
        /// Run the Organization clean up
        /// </summary>
        /// <returns>List of Organizations</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Clean up complete")]
        [Route("companysetup/cleanup")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [HttpDelete]
        public HttpResponseMessage RunCompanyDatabaseDeleteAndUDMCleanUp()
        {
            _manageOrganization.DeleteQueuedOrganizations();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Insert the Organization into the delete queuing table
        /// </summary>
        /// <param name="orgToDelete">The organization to add to the queue to be deleted</param>
        /// <param name="removeUPFMInstanceInUDM">Should the UPFM instance in UDM be removed</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request (when Organization object has missing data)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.Created, Description = "Organization queued", Type = typeof(OrganizationDelete))]
        [SwaggerOperation("InsertOrganizationToDelete")]
        [Route("companysetup/cleanup")]
        [HttpPost]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        public HttpResponseMessage InsertOrganizationToDelete([FromBody] OrganizationDelete orgToDelete, bool removeUPFMInstanceInUDM = true)
        {
            var errorList = ValidateObject(orgToDelete).ToList();

            if (errorList.Count > 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, errorList);
            }

            var org = _manageOrganization.GetOrganization(realPageId: (Guid)orgToDelete.OrganizationRealPageId);
            if (org == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Unknown Organization Id.");
            }

            OrganizationRemovalQueue organizationRemovalQueue = new OrganizationRemovalQueue()
            {
                OrganizationRealPageId = org.RealPageId,
                OrganizationCustomerMasterId = org.BooksCustomerMasterId,
                OrganizationPartyId = org.PartyId,
                OrganizationName = org.Name,
                OrganizationDomain = org.OrganizationDomain.Name,
                OrganizationRemovalQueueStatusId = 0,
                OrganizationRemovalRetryCount = 0,
                OrganizationRemoveUDMData = removeUPFMInstanceInUDM,
                RequestedBy = orgToDelete.RequestedBy
            };

            var result = _manageOrganization.InsertOrganizationRemovalQueue(organizationRemovalQueue);

            return Request.CreateResponse(HttpStatusCode.Created, result);
        }

        #region Private functions

        private List<PropertyAudit> GetAuditProductProperties(Guid companyInstanceId, int productId, Guid adminUserGuid, long partyId)
        {
            var userLogin = _manageUserLogin.GetUserLogin(adminUserGuid, partyId);

            if (userLogin != null)
            {
                _userClaims.LoginName = userLogin.LoginName;
                _userClaims.UserId = Convert.ToInt32(userLogin.UserId);

                var userPersonas = _manageUserLogin.GetUserPersonaOrganization(userLogin.LoginName);
                if (userPersonas != null && userPersonas.Any(p => p.OrganizationPartyId == partyId))
                {
                    _userClaims.PersonaId = userPersonas.First(p => p.OrganizationPartyId == partyId).PersonaId;
                }

                _userClaims.FirstName = "XX";
                _userClaims.LastName = "XX";
            }

            if (_messageHandler == null)
            {
                _manageOrganization = new ManageOrganization(_userClaims);
                _manageBlueBook = new ManageBlueBook(_userClaims);
            }
            else
            {
                _manageBlueBook = new ManageBlueBook(_userClaims, _repository, _productInternalSettingRepository, _messageHandler);
                _manageOrganization = new ManageOrganization(_repository, _userClaims, _messageHandler, _manageProductOneSite);
            }

            return _manageOrganization.AuditCompanyProductPropertiesToUPFM(companyInstanceId, productId);
        }

        private IEnumerable<ValidationResult> ValidateObject(object source)
        {
            var result = new List<ValidationResult>();

            if (source == null)
            {
                result.Add(new ValidationResult("Request object is null."));
                return result;
            }

            // Dto Validation
            var valContext = new ValidationContext(source, null, null);
            Validator.TryValidateObject(source, valContext, result, true);
            return result;
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

                IdentityProviderTypeOutput output = new IdentityProviderTypeOutput() { identityProviderType = example };

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
                ObjectListOutput<ProductUI, IErrorData> output = new ObjectListOutput<ProductUI, IErrorData>() { list = productList, Status = errorStatus };

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

        /// <summary>
        /// Used to document examples of the Companysetup Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class CompanySetupExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>List of Companies example</returns>
            public object GetExamples()
            {
                CompanySetup example = new CompanySetup()
                {
                    OrganizationPartyId = 3,
                    OrganizationName = "RealPage",
                    ContractedName = "RealPage",
                    RealPageId = Guid.Empty,
                    BooksMasterId = "1",
                    BooksCustomerMasterId = "3",
                    OrganizationTypeId = 1,
                    OrganizationType = "Multifamily",
                    OrganizationDomainId = 1,
                    Domain = "Primary",
                    Products = 3
                };
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                ObjectOutput<CompanySetup, IErrorData> output = new ObjectOutput<CompanySetup, IErrorData>()
                {
                    obj = example,
                    Status = errorStatus
                };

                return output;
            }
        }

        /// <summary>
        /// Used to document examples of the Companysetup Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class PropertyListExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>List of Companies example</returns>
            public object GetExamples()
            {
                List<PropertySetup> propertySetupExample = new List<PropertySetup>()
                {
                    new PropertySetup()
                    {
                        PropertyInstanceId = 105294,
                        Name = "WOODVILLE VILLAGE",
                        ContractedName = "WOODVILLE VILLAGE",
                        Address = "151 CO. RD. 63",
                        City = "WOODVILLE",
                        State = "AL",
                        PostalCode = "35776",
                        Country = "UNITED STATES",
                        County = null,
                        InstanceId = Guid.Parse("1e38a88a-b986-416e-b0cf-5944935a92be"),
                        CustomerPropertyId = "1409051",
                        Domain = "Primary",
                        TotalRecords = 573
                    }
                };

                List<string> domain = new List<string>()
                {
                    "Primary",
                    "UAT"
                };
                CompanyPropertySetup company = new CompanyPropertySetup()
                {
                    Property = propertySetupExample,
                    Domain = domain
                };
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                ObjectOutput<CompanyPropertySetup, IErrorData> output = new ObjectOutput<CompanyPropertySetup, IErrorData>()
                {
                    obj = company,
                    Status = errorStatus
                };

                return output;
            }
        }

        [ExcludeFromCodeCoverage]
        public class PropertyAuditExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>List of Companies example</returns>
            public object GetExamples()
            {
                List<PropertyAudit> propertyAuditExample = new List<PropertyAudit>()
                {
                    new PropertyAudit()
                    {
                        Name = "Property 1",
                        ProductInstanceId = "1234567",
                        UPFMInstanceId = "11111111-1111-2222-3333-3C0F434AE62D".ToLower(),
                        UPFMName = "Property 1",
                        Status = "OK",
                        Domain = "Primary",
                        ContractedName = "Property 1"
                    },
                    new PropertyAudit()
                    {
                        Name = "Property 2",
                        ProductInstanceId = "7654321",
                        UPFMInstanceId = null,
                        UPFMName = null,
                        Status = "No ID",
                        Domain = "Primary",
                        ContractedName = "Property 2"
                    },
                    new PropertyAudit()
                    {
                        Name = "",
                        ProductInstanceId = "66557788",
                        UPFMInstanceId = "44444444-4444-2222-3333-3C0F434AE62D".ToLower(),
                        UPFMName = "Property 3",
                        Status = "No Product",
                        Domain = "Primary",
                        ContractedName = "Property 3 UAT"
                    },
                };

                Status<IErrorData> errorStatus = new Status<IErrorData>();
                ObjectOutput<List<PropertyAudit>, IErrorData> output = new ObjectOutput<List<PropertyAudit>, IErrorData>()
                {
                    obj = propertyAuditExample,
                    Status = errorStatus
                };

                return output;
            }
        }

        [ExcludeFromCodeCoverage]
        public class ProductPropertyDetailsExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>List of Companies example</returns>
            public object GetExamples()
            {
                ProductPropertyDetails propertyProductExample = new ProductPropertyDetails
                {
                    ProductStatusDetail = new ProductStatusDetail()
                    {
                        CustomerPropertyId = "1234567",
                        ProductInstanceId = "1234567",
                        ContractedName = "Property 1",
                        IsActive = "true",
                        Domain = "Primary"
                    },
                    PropertyDetails = new List<PropertySetup>()
                    {
                        new PropertySetup()
                        {
                            Name = "WOODVILLE VILLAGE",
                            ContractedName = "WOODVILLE VILLAGE",
                            InstanceId = Guid.Parse("1e38a88a-b986-416e-b0cf-5944935a92be"),
                            Domain = "Primary",
                            IsActive = "true "
                        }
                    }
                };

                Status<IErrorData> errorStatus = new Status<IErrorData>();
                ObjectOutput<ProductPropertyDetails, IErrorData> output = new ObjectOutput<ProductPropertyDetails, IErrorData>()
                {
                    obj = propertyProductExample,
                    Status = errorStatus
                };

                return output;
            }
        }

        #endregion
    }
}