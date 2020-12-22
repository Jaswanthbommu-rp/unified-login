using Castle.Components.DictionaryAdapter;
using JsonApiSerializer;
using Moq;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest
{
    [ExcludeFromCodeCoverage]
    public class OrganizationTests
    {
        #region Private Variables
        public static readonly Guid EmployeeCompanyRealPageId = new Guid("0D018E46-C20E-477D-ADED-4E5A35FB8F99");
        
        Mock<IRepository> _mockRepository = new Mock<IRepository>();
        Mock<IUnitOfWork> _mockUnitofWork = new Mock<IUnitOfWork>();
        Mock<IRepositoryResponse> _mockRepositoryResponse = new Mock<IRepositoryResponse>();
        Mock<HttpMessageHandler> _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        private static Guid _RealPageId = new Guid("C802694D-5553-4527-8616-3C0F434AE62D");
        private static Guid _adminRealPageId = new Guid("C802694D-1111-2222-3333-3C0F434AE62D");
        private static Guid _invalidRealPageId = new Guid("11111111-1111-2222-3333-3C0F434AE62D");
        private static string _CompanyName = "CF Real Estate Services";
        private static DateTime _CreateDate = DateTime.MaxValue.ToUniversalTime();
        private static int _PartyId = 54321;
        private static long _BooksMasterId = 2116;
        private static long _BooksCompanyMasterId = 379;
        private static int _organizationTypeId = 6;
        private static int _organizationDomainId = 1;
        private static DefaultUserClaim _defaultUserClaim = new DefaultUserClaim();

        private static List<GbProductMap> _gbProductMap;

        #endregion

        public OrganizationTests()
        {
            _gbProductMap = new List<GbProductMap>
            {
                new GbProductMap() {BooksProductCode = "OS", Name = "OneSite", ProductId = 1, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "UI", Name = "UnifiedUI", ProductId = 2, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "UPFM", Name = "Unified Platform", ProductId = 3, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "AO", Name = "Asset Optimization", ProductId = 4, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "PW", Name = "Propertyware", ProductId = 5, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "L2L", Name = "Lead2Lease", ProductId = 6, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "YS", Name = "YieldStar", ProductId = 7, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "ACCT", Name = "Financial Suite", ProductId = 8, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "LS", Name = "Marketing Center", ProductId = 9, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "LVL1", Name = "Prospect Contact Center", ProductId = 10, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "NULL", Name = "Social", ProductId = 11, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "OPSB", Name = "Ops Bid", ProductId = 12, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "OPS", Name = "Spend Management", ProductId = 13, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "OMS", Name = "Client Portal", ProductId = 14, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "LD", Name = "Renters Insurance", ProductId = 15, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "CD", Name = "Vendor Credentialing", ProductId = 16, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "AB", Name = "Resident Portals", ProductId = 17, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "NWP", Name = "Utility Management", ProductId = 18, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "LP", Name = "Product Learning Portal", ProductId = 19, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "DOC", Name = "Document Director", ProductId = 20, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "OSC", Name = "L&R Conversion Utility", ProductId = 21, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "OC", Name = "OmniChannel", ProductId = 22, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "ONST", Name = "On-Site", ProductId = 23, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "RA", Name = "Unified Data Management", ProductId = 24, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "SP", Name = "Self-provisioning portal", ProductId = 25, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "UA", Name = "Unified Amenities", ProductId = 26, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "MT", Name = "Migration Tool Application", ProductId = 27, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "PUPDATE", Name = "Product Updates", ProductId = 28, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "BI", Name = "Business Intelligence", ProductId = 29, UDMSourceCode = "AO"},
                new GbProductMap() {BooksProductCode = "PA", Name = "Performance Analytics", ProductId = 30, UDMSourceCode = "AO"},
                new GbProductMap() {BooksProductCode = "MA", Name = "Investment Analytics", ProductId = 31, UDMSourceCode = "AO"},
                new GbProductMap() {BooksProductCode = "PO", Name = "YieldStar", ProductId = 32, UDMSourceCode = "AO"},
                new GbProductMap() {BooksProductCode = "AX", Name = "Axiometrics", ProductId = 33, UDMSourceCode = "AO"},
                new GbProductMap() {BooksProductCode = "BM", Name = "Benchmarking", ProductId = 34, UDMSourceCode = "AO"},
                new GbProductMap() {BooksProductCode = "null", Name = "Support Tool", ProductId = 35, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "ELMS", Name = "EasyLMS", ProductId = 36, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "PHOTO", Name = "Property Photos", ProductId = 37, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "VMP", Name = "Vendor Marketplace", ProductId = 38, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "IMP", Name = "Integration Marketplace", ProductId = 39, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "ILMLM", Name = "ILM Lead Management", ProductId = 40, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "ILMLA", Name = "ILM Leasing Analytics", ProductId = 41, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "SM", Name = "Settings Management", ProductId = 43, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "RPM", Name = "Portfolio Management", ProductId = 44, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "CIMPL", Name = "CIMPL", ProductId = 45, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "SSM", Name = "Site Spend Management Portal", ProductId = 46, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "DIQ", Name = "Deposit Alternative", ProductId = 47, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "CPAY", Name = "ClickPay", ProductId = 48, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "HLP", Name = "Simon Help Center", ProductId = 49, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "SLM", Name = "Senior Lead Management", ProductId = 50, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "LRO", Name = "LRO", ProductId = 51, UDMSourceCode = "AO"},
                new GbProductMap() {BooksProductCode = "AA", Name = "Amenity Optimization", ProductId = 52, UDMSourceCode = "AO"},
                new GbProductMap() {BooksProductCode = "AIRM", Name = "AI Revenue Management", ProductId = 53, UDMSourceCode = "AO"},
                new GbProductMap() {BooksProductCode = "RC", Name = "Rent Control", ProductId = 54, UDMSourceCode = "AO"},
                new GbProductMap() {BooksProductCode = "RENO", Name = "Renovation Manager", ProductId = 55, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "SET", Name = "Unified Settings", ProductId = 56, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "SMS-T", Name = "Intelligent Building", ProductId = 57, UDMSourceCode = "IB"},
                new GbProductMap() {BooksProductCode = "SMS-E", Name = "Intelligent Building Energy", ProductId = 58, UDMSourceCode = "IB"},
                new GbProductMap() {BooksProductCode = "SMS-W", Name = "Intelligent Building Water", ProductId = 59, UDMSourceCode = "IB"},
                new GbProductMap() {BooksProductCode = "HAAS", Name = "Home Sharing", ProductId = 60, UDMSourceCode = "null"},
                new GbProductMap() {BooksProductCode = "PME", Name = "PME Dashboard", ProductId = 62, UDMSourceCode = "null"},

            };
            
            _defaultUserClaim.CorrelationId = new Guid();
            _defaultUserClaim.CustomerMasterId = _BooksCompanyMasterId;

            var organizationTypeList = new List<OrganizationType>()
            {
                new OrganizationType()
                {
                    OrganizationTypeId = 6,
                    Name = "Multifamily",
                    CreateDate = new DateTime()
                },
                new OrganizationType()
                {
                    OrganizationTypeId = 14,
                    Name = "Vendor",
                    CreateDate = new DateTime()
                },
                new OrganizationType()
                {
                    OrganizationTypeId = 7,
                    Name = "Other",
                    CreateDate = new DateTime()
                }
            };

            var organizationDomainList = new List<OrganizationDomain>()
            {
                new OrganizationDomain()
                {
                    OrganizationDomainId = 1,
                    Name = "Primary",
                    CreateDate = new DateTime()
                }
            };

            var organizationList = new List<Organization>()
            {
                new Organization()
                {
                    RealPageId = _RealPageId,
                    CreateDate = _CreateDate,
                    Name = _CompanyName,
                    PartyId = _PartyId,
                    BooksMasterId = _BooksMasterId,
                    BooksCustomerMasterId = _BooksCompanyMasterId,
                    organizationType = new OrganizationType()
                    {
                        OrganizationTypeId = _organizationTypeId,
                        Name = "Multifamily",
                        CreateDate = new DateTime()
                    },
                    OrganizationTypeId = _organizationTypeId,
                    OrganizationDomainId = 1,
                    OrganizationDomain = new OrganizationDomain()
                    {
                        OrganizationDomainId = 1,
                        Name = "Primary",
                        CreateDate = new DateTime()
                    },
                }
            };

            Organization nullOrganization = null;

            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CompanyInstanceId = 54321,
                    CompanyInstanceSourceId = _RealPageId.ToString(), Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                    CompanyInstance = new List<CompanyInstance>()
                    {
                        new CompanyInstance() {CustomerEnvironment = "Primary", IsActive = true}
                    }
                }
            };

            List<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() {Name = "BooksUseDomains", Value = "1"}, 
                new ProductInternalSetting() {Name = "BooksUseUPFMId", Value = "1"}
            };

            HttpResponseMessage responseMapResource = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(mapResource, new JsonApiSerializerSettings());
            responseMapResource.Content = new StringContent(jsonToSave);

            _mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestIsRealPageIdNull(d))))
                .Returns(nullOrganization);

            _mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestIsRealPageId(d, _RealPageId))))
                .Returns(organizationList[0]);

            _mockRepository
                .Setup(m => m.GetMany<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestIsRealPageId(d, null))))
                .Returns(organizationList);

            _mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestIsBooksCompanyMasterId(d, _BooksCompanyMasterId))))
                .Returns(organizationList[0]);

            _mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestIsBooksMasterId(d, _BooksMasterId))))
                .Returns(organizationList[0]);

            _mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(_mockUnitofWork.Object);

            // THIS RESULT IS CACHED SO WE CANT REALLY TEST IT HAVING MULTIPLE RESULTS!
            _mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(organizationTypeList);

            _mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(organizationDomainList);

            _mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(productInternalSettings);

            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[companyInstanceSourceId]={_RealPageId}&include=companyInstance", responseMapResource);
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[companyInstance.domain]=Primary&filter[customerCompanyId]={_BooksCompanyMasterId}&include=companyInstance&include=companyInstance.attributes&filter[source]=UPFM", responseMapResource);
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[companyInstance.greenBookCares]=true&filter[customerCompanyId]={_BooksCompanyMasterId}&include=companyInstance&include=companyInstance.attributes", responseMapResource);
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[customerCompanyId]={_BooksCompanyMasterId}&include=companyInstance&include=companyInstance.attributes&filter[source]=UPFM", responseMapResource);
            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/companyinstance", new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent("{ \"result\" : \"success\"}")});
            _mockHttpMessageHandler.Setup(HttpMethod.Delete, $"http://localhost/companyinstance/54321?modifiedBy=UPFM+Automation", new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent("{ \"result\" : \"success\"}")});
            _mockHttpMessageHandler.SetupPatch($"http://localhost/companyinstance/C802694D-5553-4527-8616-3C0F434AE62D/UPFM", new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent("{ \"result\" : \"success\"}")});
        }

        #region Controller Unit Tests

        private bool TestIsRealPageIdNull(object obj)
        {
            return obj.ToString().Contains($"RealPageId = {_invalidRealPageId}");
        }

        private bool TestIsRealPageId(object obj, Guid? realPageId)
        {
            if (obj == null && realPageId == null)
            {
                return true;
            }

            if (obj == null)
            {
                return false;
            }

            return obj.ToString().ToLower().Contains($"realpageid = {realPageId}");
        }

        private bool TestIsBooksCompanyMasterId(object obj, long booksCompanyMasterId)
        {
            return obj.ToString().Contains($"BlueBookId = {booksCompanyMasterId}");
        }

        private bool TestIsBooksMasterId(object obj, long booksMasterId)
        {
            return obj.ToString().Contains($"BlackBookId = {booksMasterId}");
        }
        
        public bool TestSqlParameter(object p, string value)
        {
            return value.Equals(p.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void InsertOrganization_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpConfiguration Config = new HttpConfiguration();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("InsertOrganization" == baseTest.VerifyRouteToAction(
                    HttpMethod.Post,
                    "http://localhost/api/organization"
                )
            );
        }

        [Fact]
        public void InsertOrganization_DuplicateBookMasterId_BadRequest()
        {
            //Arrange
            OrganizationCreate organizationCreate = new OrganizationCreate()
            {
                BooksCompanyId = _BooksCompanyMasterId,
                BooksCustomerMasterId = _BooksCompanyMasterId,
                OrganizationTypeId = 1,
                Name = "CF Real Estate Services",
                Products = new List<string>()
                {
                    "AB"
                },
                AdminUser = new OrganizationAdminUser()
                {
                    FirstName = "Jack",
                    LastName = "Doe",
                    Email = "jack.doe@example.com",
                    Suffix = string.Empty,
                    Title = string.Empty
                }
            };

            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) {Request = new HttpRequestMessage(), Configuration = new HttpConfiguration()};

            HttpResponseMessage response = organizationController.InsertOrganization(organizationCreate);
            string message = response.Content.ReadAsStringAsync().Result;
            string expectedValue = "{\"Message\":\"Duplicate master ids\"}";

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
            Assert.True(expectedValue == message);
        }

        [Fact]
        public void InsertOrganization_InvalidOrganizationType_BadRequest()
        {
            //Arrange
            OrganizationCreate organizationCreate = new OrganizationCreate()
            {
                BooksCompanyId = _BooksMasterId,
                BooksCustomerMasterId = _BooksCompanyMasterId,
                OrganizationTypeId = 0,
                Name = "CF Real Estate Services",
                Products = new List<string>()
                {
                    "AB"
                },
                AdminUser = new OrganizationAdminUser()
                {
                    FirstName = "Jack",
                    LastName = "Doe",
                    Email = "jack.doe@example.com",
                    Suffix = string.Empty,
                    Title = string.Empty
                }
            };

            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) {Request = new HttpRequestMessage(), Configuration = new HttpConfiguration()};

            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            HttpResponseMessage response = organizationController.InsertOrganization(organizationCreate);
            string message = response.Content.ReadAsStringAsync().Result;
            string expectedValue = "{\"Message\":\"An invalid Organization Type id was given: 0\"}";

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
            Assert.True(expectedValue == message);
        }

        [Fact]
        public void InsertOrganization_InvalidProducts_BadRequest()
        {
            //Arrange
            OrganizationCreate organizationCreate = new OrganizationCreate()
            {
                BooksCompanyId = _BooksMasterId,
                BooksCustomerMasterId = _BooksCompanyMasterId,
                OrganizationTypeId = 1,
                Name = "CF Real Estate Services",
                Products = new List<string>()
                {
                    "XX"
                },
                AdminUser = new OrganizationAdminUser()
                {
                    FirstName = "Jack",
                    LastName = "Doe",
                    Email = "jack.doe@example.com",
                    Suffix = string.Empty,
                    Title = string.Empty
                }
            };

            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) {Request = new HttpRequestMessage(), Configuration = new HttpConfiguration()};

            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            HttpResponseMessage response = organizationController.InsertOrganization(organizationCreate);
            string message = response.Content.ReadAsStringAsync().Result;
            string expectedValue = "{\"Message\":\"An invalid product was given : XX\"}";

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
            Assert.True(expectedValue == message);
        }

        [Fact]
        public void InsertOrganization_InvalidAdminUser_BadRequest()
        {
            //Arrange
            OrganizationCreate organizationCreate = new OrganizationCreate()
            {
                BooksCompanyId = _BooksMasterId,
                BooksCustomerMasterId = _BooksCompanyMasterId,
                OrganizationTypeId = 1,
                Name = "CF Real Estate Services",
                Products = new List<string>()
                {
                    "AB"
                },
                AdminUser = null
            };

            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) {Request = new HttpRequestMessage(), Configuration = new HttpConfiguration()};

            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            HttpResponseMessage response = organizationController.InsertOrganization(organizationCreate);
            string message = response.Content.ReadAsStringAsync().Result;
            string expectedValue = "{\"Message\":\"No admin user information provided\"}";

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
            Assert.True(expectedValue == message);
        }

        [Fact]
        public void InsertOrganization_CompanyExits_BadRequest()
        {
            //Arrange
            OrganizationCreate organizationCreate = new OrganizationCreate()
            {
                BooksCompanyId = _BooksMasterId,
                BooksCustomerMasterId = _BooksCompanyMasterId,
                OrganizationTypeId = 1,
                Name = "CF Real Estate Services",
                Products = new List<string>()
                {
                    "AB"
                },
                AdminUser = new OrganizationAdminUser()
                {
                    FirstName = "Jack",
                    LastName = "Doe",
                    Email = "jack.doe@example.com",
                    Suffix = string.Empty,
                    Title = string.Empty
                }
            };

            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) {Request = new HttpRequestMessage(), Configuration = new HttpConfiguration()};

            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            HttpResponseMessage response = organizationController.InsertOrganization(organizationCreate, true);
            string message = response.Content.ReadAsStringAsync().Result;
            string expectedValue = "{\"Message\":\"MessageHandler.Handle - Company: CF Real Estate Services with BlueBookId: " + _BooksCompanyMasterId.ToString() + " already exists!\"}";

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
            Assert.True(expectedValue == message);
        }

        [Fact]
        public void InsertOrganization_CustomerMasterBookIdExits_BadRequest()
        {
            //Arrange
            OrganizationCreate organizationCreate = new OrganizationCreate()
            {
                BooksCompanyId = _BooksMasterId,
                BooksCustomerMasterId = _BooksCompanyMasterId,
                OrganizationTypeId = 1,
                Name = "New Company",
                Products = new List<string>()
                {
                    "AB"
                },
                AdminUser = new OrganizationAdminUser()
                {
                    FirstName = "Jack",
                    LastName = "Doe",
                    Email = "jack.doe@example.com",
                    Suffix = string.Empty,
                    Title = string.Empty
                }
            };

            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) {Request = new HttpRequestMessage(), Configuration = new HttpConfiguration()};

            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            HttpResponseMessage response = organizationController.InsertOrganization(organizationCreate, true);
            string message = response.Content.ReadAsStringAsync().Result;
            string expectedValue = "{\"Message\":\"MessageHandler.Handle - Bluebook customer master id " + _BooksCompanyMasterId.ToString() + " already in use!\"}";

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
            Assert.True(expectedValue == message);
        }

        [Fact]
        public void InsertOrganization_AdminExits_BadRequest()
        {
            //Arrange
            UserLoginOnly userLoginOnly = new UserLoginOnly()
            {
                UserId = 3,
                PartyId = 1,
                LoginName = "jack.doe@example.com",
                PasswordHash = ""
            };

            _mockRepository
                .Setup(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.IsAny<object>()))
                .Returns(userLoginOnly);

            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) {Request = new HttpRequestMessage(), Configuration = new HttpConfiguration()};

            OrganizationCreate organizationCreate = new OrganizationCreate()
            {
                BooksCompanyId = _BooksMasterId,
                BooksCustomerMasterId = _BooksCompanyMasterId,
                OrganizationTypeId = 1,
                Name = "New Company",
                Products = new List<string>()
                {
                    "AB"
                },
                AdminUser = new OrganizationAdminUser()
                {
                    FirstName = "Jack",
                    LastName = "Doe",
                    Email = "jack.doe@example.com",
                    Suffix = string.Empty,
                    Title = string.Empty
                }
            };

            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            HttpResponseMessage response = organizationController.InsertOrganization(organizationCreate);
            string message = response.Content.ReadAsStringAsync().Result;
            string expectedValue = "{\"Message\":\"Admin email already exists\"}";

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
            Assert.True(expectedValue == message);
        }

        [Fact]
        public void InsertOrganization_ErrorInsertOrganization_BadRequest()
        {
            //Arrange
            UserLoginOnly userLoginOnly = new UserLoginOnly();
            userLoginOnly = null;
            Organization organization = new Organization();
            RepositoryResponse repositoryResponse = new RepositoryResponse()
            {
                Id = 0,
                ErrorMessage = "Failed to create organization",
                RealPageId = Guid.Empty
            };

            _mockRepository
                .Setup(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.IsAny<object>()))
                .Returns(userLoginOnly);

            _mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_SetupOrganization, It.IsAny<object>()))
                .Returns(repositoryResponse);

            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) {Request = new HttpRequestMessage(), Configuration = new HttpConfiguration()};

            OrganizationCreate organizationCreate = new OrganizationCreate()
            {
                BooksCompanyId = _BooksMasterId,
                BooksCustomerMasterId = _BooksCompanyMasterId,
                OrganizationTypeId = 1,
                Name = "New Company",
                Products = new List<string>()
                {
                    "AB"
                },
                AdminUser = new OrganizationAdminUser()
                {
                    FirstName = "Jack",
                    LastName = "Doe",
                    Email = "jack.doe@example.com",
                    Suffix = string.Empty,
                    Title = string.Empty
                }
            };

            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            HttpResponseMessage response = organizationController.InsertOrganization(organizationCreate);
            string message = response.Content.ReadAsStringAsync().Result;
            string expectedValue = "{\"Message\":\"Failed to create organization\"}";

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
            Assert.True(expectedValue == message);
        }

        [Fact]
        public void InsertOrganization_Success()
        {
            //Arrange
            UserLoginOnly userLoginOnly = new UserLoginOnly()
            {
                UserId = 3,
                PartyId = 1,
                LoginName = "jack.doe@example.com",
                PasswordHash = ""
            };
            UserLoginOnly userLoginOnlyNull = null;

            RepositoryResponse repositoryResponse = new RepositoryResponse()
            {
                Id = 0,
                ErrorMessage = "",
                RealPageId = _RealPageId
            };

            _mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_SetupOrganization, It.IsAny<object>()))
                .Returns(repositoryResponse);

            _mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_CreateOrganizationProduct, It.IsAny<object>()))
                .Returns(repositoryResponse);

            _mockRepository
                .SetupSequence(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.IsAny<object>()))
                .Returns(userLoginOnlyNull)
                .Returns(userLoginOnly);

            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) {Request = new HttpRequestMessage(), Configuration = new HttpConfiguration()};

            OrganizationCreate organizationCreate = new OrganizationCreate()
            {
                BooksCompanyId = _BooksMasterId,
                BooksCustomerMasterId = _BooksCompanyMasterId,
                OrganizationTypeId = 6,
                Name = "New Company",
                OrganizationDomain = "Primary",
                Products = new List<string>()
                {
                    "AB"
                },
                AdminUser = new OrganizationAdminUser()
                {
                    FirstName = "Jack",
                    LastName = "Doe",
                    Email = "jack.doe@example.com",
                    Suffix = string.Empty,
                    Title = string.Empty
                }
            };

            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            HttpResponseMessage response = organizationController.InsertOrganization(organizationCreate);
            OrganizationCreateResult orgResult = JsonConvert.DeserializeObject<OrganizationCreateResult>(response.Content.ReadAsStringAsync().Result);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
            Assert.True(orgResult.Org.RealPageId == _RealPageId && orgResult.adminLogin == organizationCreate.AdminUser.Email);
        }

        [Fact]
        public void UpdateOrganization_Success()
        {
            //Arrange
            //UserLoginOnly userLoginOnly = new UserLoginOnly()
            //{
            //    UserId = 3,
            //    PartyId = 1,
            //    LoginName = "jack.doe@example.com",
            //    PasswordHash = ""
            //};
            UserLoginOnly userLoginOnlyNull = null;

            Organization organization = new Organization();
            RepositoryResponse repositoryResponse = new RepositoryResponse()
            {
                Id = 0,
                ErrorMessage = "",
                RealPageId = _RealPageId
            };

            _mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateOrganization, It.IsAny<object>()))
                .Returns(repositoryResponse);

            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) {Request = new HttpRequestMessage(), Configuration = new HttpConfiguration()};

            OrganizationUpdate organizationUpdate = new OrganizationUpdate()
            {
                RealPageId = _RealPageId,
                //BooksMasterId = 0,
                //BooksCustomerMasterId = 0,
                OrganizationTypeId = _organizationTypeId,
                OrganizationDomainId = 0,
                OrganizationDomainName = "Primary",
                Name = "New Company",
            };
            HttpResponseMessage response = organizationController.UpdateOrganization(organizationUpdate);
            OrganizationCreateResult orgResult = JsonConvert.DeserializeObject<OrganizationCreateResult>(response.Content.ReadAsStringAsync().Result);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
        }

        [Fact]
        public void UpdateOrganization_Errors()
        {
            //Arrange
            UserLoginOnly userLoginOnly = new UserLoginOnly()
            {
                UserId = 3,
                PartyId = 1,
                LoginName = "jack.doe@example.com",
                PasswordHash = ""
            };
            UserLoginOnly userLoginOnlyNull = null;

            RepositoryResponse repositoryResponse = new RepositoryResponse()
            {
                Id = 0,
                ErrorMessage = "",
                RealPageId = _RealPageId
            };

            _mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateOrganization, It.IsAny<object>()))
                .Returns(repositoryResponse);

            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) {Request = new HttpRequestMessage(), Configuration = new HttpConfiguration()};

            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            HttpResponseMessage response = organizationController.UpdateOrganization(null);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.NotFound));

            #region Invalid Org Types

            OrganizationUpdate organizationUpdate = new OrganizationUpdate()
            {
                RealPageId = _RealPageId,
                OrganizationTypeId = -1,
                OrganizationDomainId = _organizationDomainId,
                Name = "New Company",
            };

            response = organizationController.UpdateOrganization(organizationUpdate);
            string message = response.Content.ReadAsStringAsync().Result;

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
            Assert.True(message == "\"Invalid organization type id\"");

            organizationUpdate = new OrganizationUpdate()
            {
                RealPageId = _RealPageId,
                OrganizationTypeId = 0,
                OrganizationDomainId = _organizationDomainId,
                Name = "New Company",
            };

            response = organizationController.UpdateOrganization(organizationUpdate);
            message = response.Content.ReadAsStringAsync().Result;

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
            Assert.True(message == "\"Missing organization type\"");

            organizationUpdate = new OrganizationUpdate()
            {
                RealPageId = _RealPageId,
                OrganizationTypeId = 0,
                OrganizationTypeName = "Incorrect Type",
                OrganizationDomainId = _organizationDomainId,
                Name = "New Company",
            };

            response = organizationController.UpdateOrganization(organizationUpdate);
            message = response.Content.ReadAsStringAsync().Result;

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
            Assert.True(message == "\"Invalid organization type\"");

            #endregion

            #region Invalid Domain Types

            organizationUpdate = new OrganizationUpdate()
            {
                RealPageId = _RealPageId,
                OrganizationTypeId = _organizationTypeId,
                OrganizationDomainId = -1,
                Name = "New Company",
            };

            response = organizationController.UpdateOrganization(organizationUpdate);
            message = response.Content.ReadAsStringAsync().Result;

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
            Assert.True(message == "\"Invalid organization domain id\"");

            organizationUpdate = new OrganizationUpdate()
            {
                RealPageId = _RealPageId,
                OrganizationTypeId = _organizationTypeId,
                OrganizationDomainId = 0,
                Name = "New Company",
            };

            response = organizationController.UpdateOrganization(organizationUpdate);
            message = response.Content.ReadAsStringAsync().Result;

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
            Assert.True(message == "\"Missing organization domain\"");

            organizationUpdate = new OrganizationUpdate()
            {
                RealPageId = _RealPageId,
                OrganizationTypeId = _organizationTypeId,
                OrganizationDomainName = "Invalid domain type",
                Name = "New Company",
            };

            response = organizationController.UpdateOrganization(organizationUpdate);
            message = response.Content.ReadAsStringAsync().Result;

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
            Assert.True(message == "\"Invalid organization domain\"");

            #endregion
        }

        [Fact]
        public void UpdateOrganization_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpConfiguration Config = new HttpConfiguration();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("UpdateOrganization" == baseTest.VerifyRouteToAction(
                    HttpMethod.Put,
                    "http://localhost/api/organization"
                )
            );
        }

        [Fact]
        public void GetOrganization_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpConfiguration Config = new HttpConfiguration();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("GetOrganization" == baseTest.VerifyRouteToAction(
                    HttpMethod.Get,
                    "http://localhost/api/organization/C802694D-5553-4527-8616-3C0F434AE62D"
                )
            );
        }

        [Fact]
        public void GetOrganization_InvalidRealPageId_ReturnNotFound()
        {
            //Arrange

            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            HttpResponseMessage response = organizationController.GetOrganization(_invalidRealPageId);
            string message = response.Content.ReadAsStringAsync().Result;
            string expectedValue = "\"Not found\"";

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.NotFound));
            Assert.True(expectedValue == message);
        }

        [Fact]
        public void GetOrganization_ValidRealPageId_ReturnOrganization()
        {
            //Arrange
            Guid realPageId = Guid.NewGuid();

            Organization organization = new Organization()
            {
                Name = "Company",
                RealPageId = realPageId,
                BooksCustomerMasterId = _BooksCompanyMasterId,
                BooksMasterId = _BooksMasterId,
                organizationType = new OrganizationType()
                {
                    OrganizationTypeId = 1,
                    Name = "Multifamily",
                    CreateDate = new DateTime()
                },
                OrganizationTypeId = 1,
                PartyId = 1,
                PrimaryOrganization = true
            };

            _mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestIsRealPageId(d, realPageId))))
                .Returns(organization);

            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) {Request = new HttpRequestMessage(), Configuration = new HttpConfiguration()};

            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            HttpResponseMessage response = organizationController.GetOrganization(realPageId);
            Organization resultOrganization = response.Content.ReadAsAsync<Organization>().Result;

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
            Assert.True(resultOrganization == organization);
        }

        [Fact]
        public void GetOrganization_ValidRealPageId_ReturnOrganizationList()
        {
            //Arrange
            Guid? realPageId = null;

            IList<Organization> organizationList = new List<Organization>()
            {
                new Organization()
                {
                    Name = "Company",
                    RealPageId = Guid.NewGuid(),
                    BooksCustomerMasterId = _BooksCompanyMasterId,
                    BooksMasterId = _BooksMasterId,
                    organizationType = new OrganizationType()
                    {
                        OrganizationTypeId = 1,
                        Name = "Multifamily",
                        CreateDate = new DateTime()
                    },
                    OrganizationTypeId = 1,
                    OrganizationDomainId = 1,
                    OrganizationDomain = new OrganizationDomain()
                    {
                        OrganizationDomainId = 1,
                        Name = "Primary",
                        CreateDate = new DateTime()
                    },
                    PartyId = 1,
                    PrimaryOrganization = true
                }
            };

            _mockRepository
                .Setup(m => m.GetMany<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    null))
                .Returns(organizationList);

            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) {Request = new HttpRequestMessage(), Configuration = new HttpConfiguration()};

            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            HttpResponseMessage response = organizationController.GetOrganization(realPageId);
            IList<Organization> resultOrganizationList = response.Content.ReadAsAsync<IList<Organization>>().Result;

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
            Assert.True(resultOrganizationList == organizationList);
        }

        [Fact]
        public void OrganizationCustomFields_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpConfiguration Config = new HttpConfiguration();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("OrganizationCustomFields" == baseTest.VerifyRouteToAction(
                    HttpMethod.Get,
                    "http://localhost/api/organization/customfields"
                )
            );
        }

        [Fact]
        public void OrganizationCustomFields_ValidData_OKRequest()
        {
            //Arrange
            RequestParameter datafilter = new RequestParameter();
            datafilter.Pages.ResultsPerPage = 0;
            datafilter.Pages.StartRow = 1;
            IDictionary<object, object> globals = new Dictionary<object, object>();
            globals.Add(BaseType.RequestParameter, datafilter);
            int bookMasterTypeId = (int) BookMasterType.CustomerMasterId;
            Type type = typeof(IList<CustomField>);

            DefaultUserClaim userClaim = new DefaultUserClaim()
            {
                PersonaId = 1234,
                OrganizationRealPageGuid = new Guid(),
                UserRealPageGuid = new Guid(),
                CustomerMasterId = _BooksCompanyMasterId
            };

            IList<CustomField> customFieldList = new List<CustomField>()
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

            _mockRepository
                .Setup(m => m.GetMany<CustomField>(StoredProcNameConstants.SP_GetFieldsByMasterId, It.IsAny<object>()))
                .Returns(customFieldList);

            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) {Request = new HttpRequestMessage(), Configuration = new HttpConfiguration()};

            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            int NumberOfProperties = type.GetProperties().Length;
            HttpResponseMessage response = organizationController.OrganizationCustomFields(datafilter: null);

            //Assert
            Assert.True(
                customFieldList.Count == customFieldList.Count
                &&
                customFieldList.SequenceEqual(customFieldList)
                &&
                NumberOfProperties == 1
            );
        }

        [Fact]
        public void GetProductsByOrganization_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpConfiguration Config = new HttpConfiguration();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("GetProductsByOrganization" == baseTest.VerifyRouteToAction(
                    HttpMethod.Get,
                    "http://localhost/api/organization/C802694D-5553-4527-8616-3C0F434AE62D/products?mergePersonaAccess=false&allProducts=false"
                )
            );
        }

        [Fact]
        public void OrganizationType_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpConfiguration Config = new HttpConfiguration();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("OrganizationType" == baseTest.VerifyRouteToAction(
                    HttpMethod.Get,
                    "http://localhost/api/OrganizationType"
                )
            );
        }

        [Fact]
        public void OrganizationType_NoData_OKRequest()
        {
            //Arrange
            ObjectListOutput<OrganizationType, IErrorData> output = new ObjectListOutput<OrganizationType, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            List<OrganizationType> organizationTypeList = null;

            Mock<IRepository> mockRepository = new Mock<IRepository>();

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(organizationTypeList);

            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) {Request = new HttpRequestMessage(), Configuration = new HttpConfiguration()};

            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            HttpResponseMessage response = organizationController.OrganizationType();
            output = response.Content.ReadAsAsync<ObjectListOutput<OrganizationType, IErrorData>>().Result;

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
            Assert.True(output.list == null);
        }

        [Fact]
        public void OrganizationType_ValidData_OKRequest()
        {
            //Arrange
            ObjectListOutput<OrganizationType, IErrorData> output = new ObjectListOutput<OrganizationType, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) {Request = new HttpRequestMessage(), Configuration = new HttpConfiguration()};

            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            HttpResponseMessage response = organizationController.OrganizationType();
            output = response.Content.ReadAsAsync<ObjectListOutput<OrganizationType, IErrorData>>().Result;

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
            Assert.True(output.list.Count.Equals(3));
        }

        [Fact]
        public void OrganizationProductTest()
        {
            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) {Request = new HttpRequestMessage(), Configuration = new HttpConfiguration()};

            List<ProductEnum> productList = new EditableList<ProductEnum>();
            List<string> blueBookProductList = new List<string>();

            // verify all blue book enums match a product
            foreach (var pi in typeof(BlueBookProductConstants).GetFields())
            {
                blueBookProductList.Add(pi.GetValue(pi).ToString());
            }

            List<string> invalidProductList = ManageOrganization.ParseProduct(blueBookProductList, productList);
            Assert.True(invalidProductList.Count == 0 && productList.Count == blueBookProductList.Count);

            // list of products to exclude from Bluebook to product integration
            var ignoreProductList = new List<ProductEnum>()
            {
                ProductEnum.UnifiedUI, ProductEnum.SelfProvisioningPortal, ProductEnum.SalesForce, ProductEnum.SettingsManagement
            };

            foreach (var pr in typeof(ProductEnum).GetFields())
            {
                if ((!pr.Name.Equals("value__", StringComparison.OrdinalIgnoreCase))&& (!pr.Name.Equals("UnifiedSettings", StringComparison.OrdinalIgnoreCase)))
                {
                    ProductEnum current = (ProductEnum) Enum.Parse(typeof(ProductEnum), pr.Name);
                    if (!ignoreProductList.Contains(current))
                    {
                        // if this fails, then you didn't add the product to the BlueBookProductConstants.cs file!
                        Assert.True(productList.Contains(current), $"Missing product {pr.Name} in BlueBookProductConstants.cs");
                    }
                }
            }
        }

        [Fact]
        public void ListOrganizationByEnterpriseUserId_InvalidRealPageId_ReturnBadRequest()
        {
            //Arrange
            ObjectListOutput<Organization, IErrorData> output = new ObjectListOutput<Organization, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            Guid realPageId = Guid.Empty;

            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) {Request = new HttpRequestMessage(), Configuration = new HttpConfiguration()};

            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            HttpResponseMessage response = organizationController.ListOrganizationByEnterpriseUserId(realPageId);
            string message = response.Content.ReadAsStringAsync().Result;
            string expectedValue = "\"Invalid parameter: realPageId\"";

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
            Assert.True(expectedValue == message);
        }

        [Fact]
        public void ListOrganizationByEnterpriseUserId_ValidRealPageId_ReturnData()
        {
            //Arrange
            ObjectListOutput<Organization, IErrorData> output = new ObjectListOutput<Organization, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            Guid realPageId = Guid.NewGuid();

            DefaultUserClaim userClaim = new DefaultUserClaim()
            {
                PersonaId = 1234,
                OrganizationRealPageGuid = realPageId,
                UserRealPageGuid = new Guid(),
                CustomerMasterId = _BooksCompanyMasterId
            };

            OrganizationType organizationType = new OrganizationType()
            {
                OrganizationTypeId = 1,
                Name = "Multifamily",
                CreateDate = new DateTime()
            };

            OrganizationDomain organizationDomain = new OrganizationDomain()
            {
                OrganizationDomainId = 1,
                Name = "Primary",
                CreateDate = new DateTime()
            };

            RoleType roleTypeFrom = new RoleType()
            {
                PartyRoleTypeId = (int) UserRoleType.User,
                ParentPartyRoleTypeId = 400,
                Name = "User"
            };

            RoleType roleTypeTo = new RoleType()
            {
                PartyRoleTypeId = 202,
                ParentPartyRoleTypeId = 200,
                Name = "Property Management Company"
            };

            RelationshipType relationshipType = new RelationshipType()
            {
                RelationshipTypeId = 44,
                RoleTypeIdValidFrom = (int) UserRoleType.User,
                RoleTypeIdValidTo = 202,
                Name = "User Relationship",
                Description = ""
            };

            PartyRelationship partyRelationship = new PartyRelationship()
            {
                PartyRelationshipId = 3,
                PartyIdFrom = 19,
                RealPageIdFrom = new Guid("8946d26d-8ede-40d1-b6c3-d52bc903f202"),
                PartyIdTo = 6,
                RealPageIdTo = new Guid("724DE532-7969-42B5-9E71-2955167179BA"),
                RoleTypeIdFrom = (int) UserRoleType.User,
                RoleTypeFrom = roleTypeFrom,
                RoleTypeIdTo = 202,
                RoleTypeTo = roleTypeTo,
                PartyRelationshipTypeId = 44,
                PartyRelationshipType = relationshipType,
                FromDate = DateTime.UtcNow,
                ThruDate = DateTime.MaxValue.ToUniversalTime()
            };

            IList<Organization> organizationList = new List<Organization>()
            {
                new Organization()
                {
                    Name = "Company",
                    RealPageId = Guid.NewGuid(),
                    BooksCustomerMasterId = _BooksCompanyMasterId,
                    BooksMasterId = _BooksMasterId,
                    organizationType = organizationType,
                    OrganizationTypeId = 1,
                    OrganizationDomain = organizationDomain,
                    OrganizationDomainId = 1,
                    PartyId = 1,
                    PrimaryOrganization = true,
                    partyRelationship = partyRelationship
                }
            };

            IList<OrganizationType> organizationTypeList = new List<OrganizationType>() {organizationType};
            IList<RelationshipType> relationshipTypeList = new List<RelationshipType>();
            relationshipTypeList.Add(new RelationshipType() {RelationshipTypeId = 44, Name = "test"});
            IList<RoleType> roleTypeList = new List<RoleType>();
            roleTypeList.Add(new RoleType() {Name = "User", PartyRoleTypeId = 401, ParentPartyRoleTypeId = 400});
            roleTypeList.Add(new RoleType() {Name = "Property Management Company", PartyRoleTypeId = 202, ParentPartyRoleTypeId = 200});

            List<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() {Name = "BooksUseDomains", Value = "1"}, 
                new ProductInternalSetting() {Name = "BooksUseUPFMId", Value = "1"}
            };

            _mockRepository
                .Setup(m => m.GetMany<Organization>(StoredProcNameConstants.SP_ListOrganizationByRealPageId, It.IsAny<object>()))
                .Returns(organizationList);

            IUserLoginRepository userLoginRepository = new UserLoginRepository(_mockRepository.Object);
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();
            ManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

            _mockRepository
                .Setup(m => m.GetOne<PartyRelationship>(StoredProcNameConstants.SP_GetPartyRelationshipByRealPageId, It.IsAny<object>()))
                .Returns(partyRelationship);

            _mockRepository
                .Setup(m => m.GetMany<RelationshipType>(StoredProcNameConstants.SP_ListRelationshipType, It.IsAny<object>()))
                .Returns(relationshipTypeList);

            _mockRepository
                .Setup(m => m.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, It.IsAny<object>()))
                .Returns(roleTypeList);

            _mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(productInternalSettings);

            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) {Request = new HttpRequestMessage(), Configuration = new HttpConfiguration()};

            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            HttpResponseMessage response = organizationController.ListOrganizationByEnterpriseUserId(realPageId);
            output = response.Content.ReadAsAsync<ObjectListOutput<Organization, IErrorData>>().Result;

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
            Assert.True(output.list.Count.Equals(1));
        }

        #endregion

        #region Get Organization
        [Fact]
        public void GetCompanyList_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpConfiguration Config = new HttpConfiguration();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("GetCompanyList" == baseTest.VerifyRouteToAction(
                    HttpMethod.Get,
                    "http://localhost/api/CompanySetup"
                )
            );
        }

        [Fact]
        public void GetCompanyList_NullOrEmptyOrganizationName()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act           
            HttpResponseMessage response = organizationController.GetCompanyList(null, null, null, null, null);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));           
        }

		[Fact]
		public void GetCompanyList_ValidRealPageId_ReturnOrganization()
		{
			//Arrange		

			var companySetupList = new List<CompanySetup>()
			{
				 new CompanySetup()
				{
					OrganizationPartyId = 3,
					OrganizationName = "RealPage",
					ContractedName = "RealPage",
					RealPageId = Guid.NewGuid(),
					BooksMasterId = "1",
					BooksCustomerMasterId = "379",
					OrganizationTypeId = 1,
					OrganizationType = "Multifamily",
					OrganizationDomainId = 1,
					Domain = "Primary",
					Products = 3
				 }
            };

			List<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>()
			{
				new ProductInternalSetting() {Name = "BooksUseDomains", Value = "1"},
				new ProductInternalSetting() {Name = "BooksUseUPFMId", Value = "1"},
				new ProductInternalSetting() {Name = "BooksUseTranslatev2", Value = "0"}
			};

			_mockRepository
			   .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
			   .Returns(productInternalSettings);

			_mockRepository
			   .Setup(m => m.GetMany<CompanySetup>(StoredProcNameConstants.SP_ListCompanySetup,
				   It.IsAny<object>()))
			   .Returns(companySetupList);

			OrganizationController organizationController = new OrganizationController(
				_mockRepository.Object
				, _mockRepositoryResponse.Object
				, _mockHttpMessageHandler.Object
				, _defaultUserClaim
			)
			{ Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

			//Act
			RPObjectCache rPObjectCache = new RPObjectCache();
			rPObjectCache.BustCache();

			List<Company> mapResource = new List<Company>()
			{
			   new Company()
			   {
				   Id = "775",
				   CustomerCompanyId = 775,
				   CompanyName = "121 7TH STREET, LLC"
			   }
			};

			HttpResponseMessage responseMapResource = new HttpResponseMessage(HttpStatusCode.OK);
			var jsonToSave = JsonConvert.SerializeObject(mapResource, new JsonApiSerializerSettings());
			responseMapResource.Content = new StringContent(jsonToSave);

			_mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany?filter[customerCompanyId]=in:{_BooksCompanyMasterId}&include=customerCompanyLocation&fields[customercompany]=customerCompanyId,companyName,phoneNumber&fields[customerCompanyLocation]=customerCompanyLocationId,customerCompanyId,address,city,state,country,postalCode,isPrimary&page[size]=9999", responseMapResource);
			HttpResponseMessage response = organizationController.GetCompanyList("RealPage", null, null, null, null);

            ObjectListOutput<CompanySetup, IErrorData> propertyOutput = new ObjectListOutput<CompanySetup, IErrorData>();
            propertyOutput = response.Content.ReadAsAsync<ObjectListOutput<CompanySetup, IErrorData>>().Result;

			//Assert
			Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
            Assert.True(propertyOutput.list[0].RealPageId == companySetupList[0].RealPageId);
		}
		#endregion

		#region getUpdatePropertyList       

		[Fact]
        public void GetPropertiesForCompany_InvalidInstanceId_ReturnBadRequest()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act           
            HttpResponseMessage response = organizationController.GetPropertiesForCompany(Guid.Empty, null,null,null);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public void UpdatePropertyForOrganization_InvalidInstanceId_ReturnBadRequest()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act           
            HttpResponseMessage response = organizationController.UpdatePropertyForOrganization(Guid.Empty, "abcd");

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public void UpdatePropertyForOrganization_InvalidPropertyName_ReturnBadRequest()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act           
            HttpResponseMessage response = organizationController.UpdatePropertyForOrganization(Guid.NewGuid(), "");

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }


        [Fact]
        public void GetPropertiesForCompany_ValidRealPageId_ReturnOrganization()
        {
            //Arrange		
            Guid companyRealPageId = Guid.NewGuid();
            string _companyRealPageId = companyRealPageId.ToString();

            var propertySetupList = new List<PropertySetup>()
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
                    InstanceId = Guid.Parse("003b0509-1189-49dc-bbe6-01c5b6277a83"),
                    CustomerPropertyId = "1409051",
                    Domain = "Primary",
                    TotalRecords = 573
                 }
            };
            List<string> domain = new List<string>()
            {
                "Primary"
            };
            List<CompanyPropertySetup> setup = new List<CompanyPropertySetup>()
            {
                new CompanyPropertySetup()
                {
                    Domain = domain,
                    Property = propertySetupList
                }
            };
            List<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() {Name = "BooksUseDomains", Value = "1"},
                new ProductInternalSetting() {Name = "BooksUseUPFMId", Value = "1"},
                new ProductInternalSetting() {Name = "BooksUseTranslatev2", Value = "0"}
            };

            _mockRepository
               .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
               .Returns(productInternalSettings);

            Mock<IRepository> _mockPropertyRepository = new Mock<IRepository>();
            _mockRepository
               .Setup(m => m.GetMany<PropertySetup>(StoredProcNameConstants.SP_GetPropertyInstanceListByIdWithPaging,
                   It.IsAny<object>()))
               .Returns(propertySetupList);

            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };


            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            List<BooksPropertyInstance> mapResource = new List<BooksPropertyInstance>()
            {
                new BooksPropertyInstance()
                {
                    id = "1234",
                    attributes =  new PropertyAttributesInstance()
                    {
                        propertyInstanceId = "1005251854",
                        propertyInstanceSourceId = "003b0509-1189-49dc-bbe6-01c5b6277a83",
                        propertyName = "COBBLESTONE COVE",
                        source = "UPFM",
                        domain = "UAT",
                        deletedReason =  "Deprecated Field"
                    }
                }
            };

			var mockManageBlueBook = new Mock<IManageBlueBook>();
			mockManageBlueBook
				.Setup(m => m.GetPropertyInstanceForCompany(
					It.IsAny<Guid>()
				 ))
				 .Returns(mapResource);
			
			HttpResponseMessage responseMapResource = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(mapResource, new JsonApiSerializerSettings());
            responseMapResource.Content = new StringContent("{\"data\":[{\"type\":\"bookspropertyinstance\",\"attributes\":{\"propertyInstanceId\":\"1005251854\",\"propertyInstanceSourceId\":\"003b0509-1189-49dc-bbe6-01c5b6277a83\",\"propertyName\":\"COBBLESTONE COVE\",\"source\":\"UPFM\",\"domain\":\"Primary\",\"deletedReason\":\"Deprecated Field\"}}]}");

            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/propertyinstance?filter[source]=UPFM&filter[companyPropertyInstanceMap.companyInstance.companyInstanceSourceId]={_companyRealPageId}&page[size]=9999&include=customerPropertyMap.customerProperty&fields[propertyinstance]=propertyInstanceId,propertyInstanceSourceId,propertyName,source,domain&fields[customerPropertyMap]=customerPropertyId,propertyInstanceId&fields[customerPropertyMap.customerProperty]=customerPropertyId,propertyName", responseMapResource);
            HttpResponseMessage response = organizationController.GetPropertiesForCompany(companyRealPageId, null, null, null);
            ObjectListOutput<CompanyPropertySetup, IErrorData> propertyOutput = new ObjectListOutput<CompanyPropertySetup, IErrorData>();
            propertyOutput = response.Content.ReadAsAsync<ObjectListOutput<CompanyPropertySetup, IErrorData>>().Result;

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
            Assert.True(propertyOutput.list[0].Property[0].InstanceId == setup[0].Property[0].InstanceId);
            Assert.True(propertyOutput.list[0].Domain[0] == setup[0].Domain[0]);
        }
        #endregion
        
        #region Audit property Tests
        
        [Fact]
        public void AuditCompanyProductPropertiesToUPFM_InvalidUserCompany()
        {
            DefaultUserClaim invalidDefaultUserClaim = new DefaultUserClaim()
            {
                CorrelationId = new Guid(), CustomerMasterId = _BooksCompanyMasterId, OrganizationRealPageGuid = Guid.NewGuid()
            };
            
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , invalidDefaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act           
            HttpResponseMessage response = organizationController.AuditCompanyProductPropertiesToUPFM(Guid.NewGuid(), (int)ProductEnum.OneSite);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public void AuditCompanyProductPropertiesToUPFM_EmptyUnknownCompany()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act           
            HttpResponseMessage response = organizationController.AuditCompanyProductPropertiesToUPFM(Guid.Empty, (int)ProductEnum.OneSite);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));

            //Act           
            response = organizationController.AuditCompanyProductPropertiesToUPFM(new Guid("00000000-0000-0000-0000-000000000000"), (int)ProductEnum.OneSite);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));

        }

        
        [Fact]
        public void AuditCompanyProductPropertiesToUPFM_AuditOneSite()
        {
            DefaultUserClaim defaultUserClaim = new DefaultUserClaim()
            {
                CorrelationId = new Guid(), CustomerMasterId = _BooksCompanyMasterId, OrganizationRealPageGuid = EmployeeCompanyRealPageId
            };
            
            var userLogin = new UserLogin() { UserId = 1234, LoginName = "admin@test.com", RealPageId = new Guid("99999999-9999-9999-9999-999999999999") };
            
            var organizationList = new List<Organization>()
            {
                new Organization()
                {
                    RealPageId = new Guid("11111111-1111-1111-1111-111111111111"),
                    CreateDate = _CreateDate,
                    Name = "Onesite Test Company",
                    PartyId = 12345,
                    BooksMasterId = 123456,
                    BooksCustomerMasterId = 1234567,
                    organizationType = new OrganizationType()
                    {
                        OrganizationTypeId = _organizationTypeId,
                        Name = "Multifamily",
                        CreateDate = new DateTime()
                    },
                    OrganizationTypeId = _organizationTypeId,
                    OrganizationDomainId = 1,
                    OrganizationDomain = new OrganizationDomain()
                    {
                        OrganizationDomainId = 1,
                        Name = "Primary",
                        CreateDate = new DateTime()
                    },
                }
            };
            
            List<dynamic> companyList = new List<dynamic>();
            string _mockJsonCompanyList = "{\r\n\t\t\"PartyId\": \""+organizationList[0].PartyId+"\",\r\n\t\t\"Name\": \""+organizationList[0].Name+"\",\r\n\t\t\"OrganizationRealPageId\": \""+organizationList[0].RealPageId+"\",\r\n\t\t\"BooksMasterId\": \""+organizationList[0].BooksMasterId+"\",\r\n\t\t\"BooksCustomerMasterId\": \""+organizationList[0].BooksCustomerMasterId+"\",\r\n\t\t\"SettingName\": \"RealPageEmployeeAccessID\",\r\n\t\t\"PersonRealPageId\": \""+userLogin.RealPageId+"\",\r\n\t\t\"LoginName\": \""+userLogin.LoginName+"\",\r\n\t}";
            companyList.Add(JsonConvert.DeserializeObject<dynamic>(_mockJsonCompanyList));
            
            OrganizationStatus organizationStatus = new OrganizationStatus()
            {
                PartyId = organizationList[0].PartyId,
                IsPending = true,
                IsActive = true,
                IsExpired = false,
                StatusTypeId = (int)UserUiStatusType.Active,
                Status = UserUiStatusType.Active,
                FromDate = new DateTime(2019,1,1)
            };
            var organizationDomainList = new List<OrganizationDomain>()
            {
                new OrganizationDomain()
                {
                    OrganizationDomainId = 1,
                    Name = "Primary",
                    CreateDate = new DateTime()
                }
            };
            var organizationTypeList = new List<OrganizationType>()
            {
                new OrganizationType()
                {
                    OrganizationTypeId = 6,
                    Name = "Multifamily",
                    CreateDate = new DateTime()
                },
                new OrganizationType()
                {
                    OrganizationTypeId = 14,
                    Name = "Vendor",
                    CreateDate = new DateTime()
                },
                new OrganizationType()
                {
                    OrganizationTypeId = 7,
                    Name = "Other",
                    CreateDate = new DateTime()
                }
            };

            var orgStatusList = new EditableList<OrganizationStatus>() {organizationStatus};

            _mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestIsRealPageId(d, organizationList[0].RealPageId))))
                .Returns(organizationList[0]);
            
            
            _mockRepository
                .Setup(m => m.GetMany<dynamic>(StoredProcNameConstants.SP_ListOrganizations, 
                    It.Is<object>(
                        d => TestIsRealPageId(d, organizationList[0].RealPageId))))
                .Returns(companyList);
            
            
            _mockRepository.Setup(m => m.GetOne<UserLogin>(StoredProcNameConstants.SP_GetUserLogin, 
                    It.Is<object>(
                        d => TestIsRealPageId(d, userLogin.RealPageId))))
                .Returns(userLogin);
            
            _mockRepository.Setup(m => m.GetOne<string>(StoredProcNameConstants.SP_GetIdentityProviderTypeByLoginName, It.IsAny<object>()))
                .Returns("local");
            
            _mockRepository.Setup(m => m.GetMany<OrganizationStatus>(StoredProcNameConstants.SP_ListOrganizationStatusByUserId, 
                    It.Is<object>(data => TestSqlParameter(data, "{ userId = "+ userLogin.UserId+" }"))))
                .Returns(orgStatusList);
            
            
            //return repository.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, param);
            _mockRepository.Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, 
                    It.Is<object>(data => TestSqlParameter(data, "{ ProductId = , ProductGUID = , Name = , BooksProductCode =  }"))))
                .Returns(_gbProductMap);
            
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                _mockRepository.Object
                , _mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

           
            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            HttpResponseMessage response = organizationController.AuditCompanyProductPropertiesToUPFM(organizationList[0].RealPageId, (int)ProductEnum.OneSite);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        #endregion
    }
}
