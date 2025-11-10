using JsonApiSerializer;
using LaunchDarkly.Sdk.Server.Interfaces;
using Moq;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Enterprise.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Batch;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Maintenance;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;
using Xunit;
using Xunit.Abstractions;
using HttpConfiguration = System.Web.Http.HttpConfiguration;
using RoleType = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest
{
    [ExcludeFromCodeCoverage]
    public class OrganizationTests : TestBase
    {
        #region Private Variables
        private readonly ITestOutputHelper _output;
        public static readonly Guid EmployeeCompanyRealPageId = new Guid("0D018E46-C20E-477D-ADED-4E5A35FB8F99");

        Mock<IUnitOfWork> _mockUnitofWork = new Mock<IUnitOfWork>();
        Mock<IRepositoryResponse> mockRepositoryResponse = new Mock<IRepositoryResponse>();
        Mock<HttpMessageHandler> _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        Mock<ITokenHelper> _mockTokenHelper = new Mock<ITokenHelper>();
        Mock<IManageUnifiedSettings> _manageUnifiedSettings = new Mock<IManageUnifiedSettings>();

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
        public static Guid propertyGuid = new Guid("5C04F18A-FC9B-4A13-AAAF-E26DA83CE516");

        private static List<ProductInternalSetting> _productInternalSettings;

        private static List<Organization> _organizationList;

        private static List<GbProductMap> _gbProductMap;

        #endregion

        public OrganizationTests(ITestOutputHelper output)
        {
            _output = output;

            _gbProductMap = new List<GbProductMap>
            {
                new GbProductMap() { BooksProductCode = "OS", Name = "OneSite", ProductId = 1, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "UI", Name = "UnifiedUI", ProductId = 2, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "UPFM", Name = "Unified Platform", ProductId = 3, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "AO", Name = "Asset Optimization", ProductId = 4, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "PW", Name = "Propertyware", ProductId = 5, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "L2L", Name = "Lead2Lease", ProductId = 6, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "YS", Name = "YieldStar", ProductId = 7, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ACCT", Name = "Financial Suite", ProductId = 8, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LS", Name = "Marketing Center", ProductId = 9, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LVL1", Name = "Prospect Contact Center", ProductId = 10, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "??", Name = "Social", ProductId = 11, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OPSB", Name = "Ops Bid", ProductId = 12, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OPS", Name = "Spend Management", ProductId = 13, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OMS", Name = "Client Portal", ProductId = 14, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LD", Name = "Renters Insurance", ProductId = 15, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "CD", Name = "Vendor Credentialing", ProductId = 16, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "AB", Name = "Resident Portals", ProductId = 17, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "NWP", Name = "Utility Management", ProductId = 18, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LP", Name = "Product Learning Portal", ProductId = 19, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "DOC", Name = "Document Director", ProductId = 20, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OSC", Name = "L&R Conversion Utility", ProductId = 21, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OC", Name = "OmniChannel", ProductId = 22, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ONST", Name = "On-Site", ProductId = 23, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "RA", Name = "Unified Data Management", ProductId = 24, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SP", Name = "Self-provisioning portal", ProductId = 25, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "UA", Name = "Unified Amenities", ProductId = 26, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "MT", Name = "Migration Tool Application", ProductId = 27, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "PUPDATE", Name = "Product Updates", ProductId = 28, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "BI", Name = "Business Intelligence", ProductId = 29, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "BIX", Name = "BIX", ProductId = 95, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "PA", Name = "Performance Analytics", ProductId = 30, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "MA", Name = "Investment Analytics", ProductId = 31, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "PO", Name = "YieldStar", ProductId = 32, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "AX", Name = "Axiometrics", ProductId = 33, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "BM", Name = "Benchmarking", ProductId = 34, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "null", Name = "Support Tool", ProductId = 35, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ELMS", Name = "EasyLMS", ProductId = 36, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "PHOTO", Name = "Property Photos", ProductId = 37, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "VMP", Name = "Vendor Marketplace", ProductId = 38, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "IMP", Name = "Integration Marketplace", ProductId = 39, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ILMLM", Name = "ILM Lead Management", ProductId = 40, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ILMLA", Name = "ILM Leasing Analytics", ProductId = 41, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SM", Name = "Settings Management", ProductId = 43, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "RPM", Name = "Portfolio Management", ProductId = 44, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "CIMPL", Name = "CIMPL", ProductId = 45, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SSM", Name = "Site Spend Management Portal", ProductId = 46, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "DIQ", Name = "Deposit Alternative", ProductId = 47, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "CPAY", Name = "ClickPay", ProductId = 48, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "HLP", Name = "Simon Help Center", ProductId = 49, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SLM", Name = "Senior Lead Management", ProductId = 50, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LRO", Name = "LRO", ProductId = 51, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "AA", Name = "Amenity Optimization", ProductId = 52, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "AIRM", Name = "AI Revenue Management", ProductId = 53, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "RC", Name = "Rent Control", ProductId = 54, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "RENO", Name = "Renovation Manager", ProductId = 55, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SET", Name = "Unified Settings", ProductId = 56, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SMS-T", Name = "Intelligent Building", ProductId = 57, UDMSourceCode = "IB" },
                new GbProductMap() { BooksProductCode = "SMS-E", Name = "Intelligent Building Energy", ProductId = 58, UDMSourceCode = "IB" },
                new GbProductMap() { BooksProductCode = "SMS-W", Name = "Intelligent Building Water", ProductId = 59, UDMSourceCode = "IB" },
                new GbProductMap() { BooksProductCode = "PME", Name = "PME Dashboard", ProductId = 62, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "RMA", Name = "Market Analytics", ProductId = 66, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ST", Name = "Support Tool", ProductId = 35, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "HOTS", Name = "Hands On Training System", ProductId = 63, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "PEQ", Name = "P2 Engagement Queue", ProductId = 64, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LeaseLabs", Name = "LeaseLabs", ProductId = 68, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "RPT", Name = "Reporting", ProductId = 67, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "6247", Name = "Self-Guided Tour", ProductId = 65, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LST", Name = "Lead Scoring", ProductId = 69, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SMS-TC", Name = "Smart Waste Commercial", ProductId = 70, UDMSourceCode = "IB" },
                new GbProductMap() { BooksProductCode = "OS", Name = "Facilities", ProductId = 75, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "RCLMS", Name = "Real Connect", ProductId = 94, UDMSourceCode = "RC" },
                new GbProductMap() { BooksProductCode = "PUDASH", Name = "Product Updates Dashboard", ProductId = 98, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LA", Name = "Lumina Ascent", ProductId = 103, UDMSourceCode = "AO" }
            };

            _defaultUserClaim.CorrelationId = new Guid();
            _defaultUserClaim.CustomerMasterId = _BooksCompanyMasterId;
            _defaultUserClaim.OrganizationPartyId = _PartyId;
            _defaultUserClaim.OrganizationRealPageGuid = _RealPageId;

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

            _organizationList = new List<Organization>()
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
                        new CompanyInstance() { CustomerEnvironment = "Primary", IsActive = true }
                    }
                }
            };

            _productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
                new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
                new ProductInternalSetting() { Name = "SettingsApiEndPoint", Value = "http://localhost" },
                new ProductInternalSetting() { Name = "UnifiedLoginServerClientName", Value = "unifiedlogin-server" },
                new ProductInternalSetting() { Name = "UnifiedLoginServerClientSecret", Value = "abcdefgh" },
                new ProductInternalSetting() { Name = "UpdateProductInUDM", Value = "1" },
                new ProductInternalSetting() { Name = "productintegrationtype", Value = "Legacy" },
            };

            HttpResponseMessage responseMapResource = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(mapResource, new JsonApiSerializerSettings());
            responseMapResource.Content = new StringContent(jsonToSave);

            var booksPropertyJson = "{\n\t\"data\": [\n\t\t{\n\t\t\t\"type\": \"propertyinstance\",\n\t\t\t\"id\": \"1005251685\",\n\t\t\t\"attributes\": {\n\t\t\t\t\"propertyInstanceId\": 1005251685,\n\t\t\t\t\"propertyInstanceSourceId\": \"a5192995-aaaa-bbbb-8df2-f30f1b8dc752\",\n\t\t\t\t\"propertyName\": \"TEST SITE 1\",\n\t\t\t\t\"source\": \"UPFM\",\n\t\t\t\t\"domain\": \"Primary\",\n\t\t\t\t\"deletedReason\": \"Deprecated Field\",\n\t\t\t\t\"customerPropertyMap\": [\n\t\t\t\t\t{\n\t\t\t\t\t\t\"customerPropertyId\": 1426972,\n\t\t\t\t\t\t\"propertyInstanceId\": 1005251685,\n\t\t\t\t\t\t\"customerProperty\": [\n\t\t\t\t\t\t\t{\n\t\t\t\t\t\t\t\t\"customerPropertyId\": 1426972,\n\t\t\t\t\t\t\t\t\"propertyName\": \"TEST SITE 1\",\n\t\t\t\t\t\t\t\t\"hasMedia\": \"Deprecated Field\"\n\t\t\t\t\t\t\t}\n\t\t\t\t\t\t]\n\t\t\t\t\t}\n\t\t\t\t]\n\t\t\t},\n\t\t\t\"links\": {\n\t\t\t\t\"self\": \"/propertyinstance/1005251685\"\n\t\t\t}\n\t\t},\n\t\t{\n\t\t\t\"type\": \"propertyinstance\",\n\t\t\t\"id\": \"1005262392\",\n\t\t\t\"attributes\": {\n\t\t\t\t\"propertyInstanceId\": 1005262392,\n\t\t\t\t\"propertyInstanceSourceId\": \"22c59953-7145-416b-9e98-f17f4cb3fa97\",\n\t\t\t\t\"propertyName\": \"Adams Station\",\n\t\t\t\t\"source\": \"UPFM\",\n\t\t\t\t\"domain\": \"Primary\",\n\t\t\t\t\"deletedReason\": \"Deprecated Field\",\n\t\t\t\t\"customerPropertyMap\": [\n\t\t\t\t\t{\n\t\t\t\t\t\t\"customerPropertyId\": 384066,\n\t\t\t\t\t\t\"propertyInstanceId\": 1005262392,\n\t\t\t\t\t\t\"customerProperty\": [\n\t\t\t\t\t\t\t{\n\t\t\t\t\t\t\t\t\"customerPropertyId\": 384066,\n\t\t\t\t\t\t\t\t\"propertyName\": \"Adams Station\",\n\t\t\t\t\t\t\t\t\"hasMedia\": \"Deprecated Field\"\n\t\t\t\t\t\t\t}\n\t\t\t\t\t\t]\n\t\t\t\t\t}\n\t\t\t\t]\n\t\t\t},\n\t\t\t\"links\": {\n\t\t\t\t\"self\": \"/propertyinstance/1005262392\"\n\t\t\t}\n\t\t}\n\t],\n\t\"meta\": {\n\t\t\"totalEntities\": 3,\n\t\t\"totalPages\": 1\n\t},\n\t\"links\": {\n\t\t\"self\": \"http://booksapi-qa.realpage.com/propertyinstance?page%5Bsize%5D=9999&filter%5Bsource%5D=UPFM&filter%5BcompanyPropertyInstanceMap.companyInstance.companyInstanceSourceId%5D=6c594cbd-62c2-4fbc-9c44-5c7e4f0ee971&include=customerPropertyMap.customerProperty&fields%5Bpropertyinstance%5D=propertyInstanceId%2CpropertyInstanceSourceId%2CpropertyName%2Csource%2Cdomain&fields%5BcustomerPropertyMap%5D=customerPropertyId%2CpropertyInstanceId&fields%5BcustomerPropertyMap.customerProperty%5D=customerPropertyId%2CpropertyName&page%5Bnumber%5D=1\",\n\t\t\"first\": \"http://booksapi-qa.realpage.com/propertyinstance?page%5Bsize%5D=9999&filter%5Bsource%5D=UPFM&filter%5BcompanyPropertyInstanceMap.companyInstance.companyInstanceSourceId%5D=6c594cbd-62c2-4fbc-9c44-5c7e4f0ee971&include=customerPropertyMap.customerProperty&fields%5Bpropertyinstance%5D=propertyInstanceId%2CpropertyInstanceSourceId%2CpropertyName%2Csource%2Cdomain&fields%5BcustomerPropertyMap%5D=customerPropertyId%2CpropertyInstanceId&fields%5BcustomerPropertyMap.customerProperty%5D=customerPropertyId%2CpropertyName&page%5Bnumber%5D=1\",\n\t\t\"prev\": null,\n\t\t\"next\": null,\n\t\t\"last\": \"http://booksapi-qa.realpage.com/propertyinstance?page%5Bsize%5D=9999&filter%5Bsource%5D=UPFM&filter%5BcompanyPropertyInstanceMap.companyInstance.companyInstanceSourceId%5D=6c594cbd-62c2-4fbc-9c44-5c7e4f0ee971&include=customerPropertyMap.customerProperty&fields%5Bpropertyinstance%5D=propertyInstanceId%2CpropertyInstanceSourceId%2CpropertyName%2Csource%2Cdomain&fields%5BcustomerPropertyMap%5D=customerPropertyId%2CpropertyInstanceId&fields%5BcustomerPropertyMap.customerProperty%5D=customerPropertyId%2CpropertyName&page%5Bnumber%5D=1\"\n\t}\n}";
            HttpResponseMessage booksPropertyResponse = new HttpResponseMessage(HttpStatusCode.OK);
            booksPropertyResponse.Content = new StringContent(booksPropertyJson);

            HttpResponseMessage booksEmptyPropertyResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

            var booksTranslateOneSiteJson = "{\n\t\"data\": {\n\t\t\"type\": \"propertyinstancetranslations\",\n\t\t\"attributes\": [\n\t\t\t{\n\t\t\t\t\"propertyInstanceSourceId\": \"a5192995-aaaa-bbbb-8df2-f30f1b8dc752\",\n\t\t\t\t\"source\": \"UPFM\",\n\t\t\t\t\"translatedPropertyInstances\": [\n\t\t\t\t\t{\n\t\t\t\t\t\t\"source\": \"OS\",\n\t\t\t\t\t\t\"propertyInstanceSourceId\": \"1234567\"\n\t\t\t\t\t}\n\t\t\t\t]\n\t\t\t},\n\t\t\t{\n\t\t\t\t\"propertyInstanceSourceId\": \"a5192995-aaaa-bbbb-8df2-f30f1b8dc752\",\n\t\t\t\t\"source\": \"UPFM\",\n\t\t\t\t\"translatedPropertyInstances\": [\n\t\t\t\t\t{\n\t\t\t\t\t\t\"source\": \"AB\",\n\t\t\t\t\t\t\"propertyInstanceSourceId\": \"7654321\"\n\t\t\t\t\t}\n\t\t\t\t]\n\t\t\t}\n\t\t]\n\t}\n}";
            HttpResponseMessage booksTranslateOneSiteResponse = new HttpResponseMessage(HttpStatusCode.OK);
            booksTranslateOneSiteResponse.Content = new StringContent(booksTranslateOneSiteJson);

            var booksCustomerCompanyPropertyJson = "{\r\n\"data\": [\r\n{\r\n\"type\": \"customercompany\",\r\n\"id\": \"379\",\r\n\"attributes\": {\r\n\"customerCompanyId\": 379,\r\n\"companyName\": \"CF REAL ESTATE SERVICES\",\r\n\"phoneNumber\": null,\r\n\"formerlyKnownAs\": null,\r\n\"legalEntityName\": null,\r\n\"companyType\": \"Multifamily\",\r\n\"marketSegment\": [\r\n\"Conventional\",\r\n\"Student Living\"\r\n],\r\n\"website\": null,\r\n\"isActive\": true,\r\n\"isUat\": false,\r\n\"createdAt\": \"2019-05-23 22:48:19.000000-0500\",\r\n\"modifiedAt\": \"2019-05-23 22:48:19.000000-0500\",\r\n\"deletedAt\": null,\r\n\"createdBy\": null,\r\n\"modifiedBy\": null,\r\n\"modifiedSource\": null,\r\n\"migrationStatus\": \"staged\",\r\n\"masterCompanyId\": 2116\r\n},\r\n\"links\": {\r\n\"self\": \"/customercompany/379\"\r\n}\r\n}\r\n],\r\n\"meta\": {\r\n\"totalEntities\": 1,\r\n\"totalPages\": 1\r\n}\r\n}";
            HttpResponseMessage booksCustomerCompanyResponse = new HttpResponseMessage(HttpStatusCode.OK);
            booksCustomerCompanyResponse.Content = new StringContent(booksCustomerCompanyPropertyJson);

            var booksCompanyMasterDomainListJson = "{\r\n\"data\": [\r\n{\r\n\"type\": \"domain\",\r\n\"id\": \"Primary\",\r\n\"attributes\": {\r\n\"domain\": \"Primary\",\r\n\"description\": \"Primary\"\r\n},\r\n \"links\": {\r\n\"self\": \"/domain/Primary\"\r\n}\r\n},\r\n{\r\n\"type\": \"domain\",\r\n\"id\": \"UAT\",\r\n\"attributes\": {\r\n\"domain\": \"UAT\",\r\n\"description\": \"UAT\"\r\n},\r\n\"links\": {\r\n\"self\": \"/domain/UAT\"\r\n}\r\n}\r\n]\r\n}";
            HttpResponseMessage booksCompanyMasterDomainListResponse = new HttpResponseMessage(HttpStatusCode.OK);
            booksCompanyMasterDomainListResponse.Content = new StringContent(booksPropertyJson);

            var booksCompanyInstancesJson = "{\"data\":[{\"type\":\"companyinstance\",\"id\":\"1049316\",\"attributes\":{\"companyInstanceId\":1049316,\"source\":\"UPFM\",\"companyInstanceSourceId\":\"f5c090fa-78ab-452f-b504-98aafee09121\",\"companyName\":\"CF Real Estate Services\",\"companyType\":\"Multifamily\",\"isActive\":true,\"domain\":\"Primary\",\"deletedReason\":\"Deprecated Field\",\"marketSegment\":[]},\"links\":{\"self\":\"\\/companyinstance\\/1049316\"}},{\"type\":\"companyinstance\",\"id\":\"1068792\",\"attributes\":{\"companyInstanceId\":1068792,\"source\":\"UPFM\",\"companyInstanceSourceId\":\"e072dcfc-99b8-493d-8f8d-26786c965d08\",\"companyName\":\"CF REAL ESTATE SERVICES - UAT\",\"companyType\":null,\"isActive\":true,\"domain\":\"UAT\",\"deletedReason\":\"Deprecated Field\",\"marketSegment\":[]},\"links\":{\"self\":\"\\/companyinstance\\/1068792\"}}]}";
            HttpResponseMessage booksCompanyInstancesResponse = new HttpResponseMessage(HttpStatusCode.OK);
            booksCompanyInstancesResponse.Content = new StringContent(booksCompanyInstancesJson);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestIsRealPageIdNull(d))))
                .Returns(nullOrganization);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestIsRealPageId(d, _RealPageId))))
                .Returns(_organizationList[0]);

            mockRepository
                .Setup(m => m.GetMany<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestIsRealPageId(d, null))))
                .Returns(_organizationList);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestIsBooksCompanyMasterId(d, _BooksCompanyMasterId))))
                .Returns(_organizationList[0]);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestIsBooksMasterId(d, _BooksMasterId))))
                .Returns(_organizationList[0]);

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(_mockUnitofWork.Object);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            // THIS RESULT IS CACHED SO WE CANT REALLY TEST IT HAVING MULTIPLE RESULTS!
            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(organizationDomainList);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            _mockTokenHelper
                .Setup(m => m.GetUnifiedLoginServerToken("unifiedsettingsapi"))
                .Returns("abcdedfghijklmnilol");

            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[companyInstanceSourceId]={_RealPageId}&include=companyInstance", responseMapResource);
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[companyInstance.domain]=Primary&filter[customerCompanyId]={_BooksCompanyMasterId}&include=companyInstance&include=companyInstance.attributes&filter[source]=UPFM", responseMapResource);
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[companyInstance.greenBookCares]=true&filter[customerCompanyId]={_BooksCompanyMasterId}&include=companyInstance&include=companyInstance.attributes", responseMapResource);
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[customerCompanyId]={_BooksCompanyMasterId}&include=companyInstance&include=companyInstance.attributes&filter[source]=UPFM", responseMapResource);
            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/companyinstance", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            _mockHttpMessageHandler.Setup(HttpMethod.Delete, $"http://localhost/companyinstance/54321?modifiedBy=UPFM+Automation", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            _mockHttpMessageHandler.SetupPatch($"http://localhost/companyinstance/C802694D-5553-4527-8616-3C0F434AE62D/UPFM", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/propertyinstance?filter[source]=UPFM&filter[companyPropertyInstanceMap.companyInstance.companyInstanceSourceId]=11111111-1111-1111-1111-111111111111&page[size]=9999&include=customerPropertyMap.customerProperty.customerPropertyOrderType&fields[propertyinstance]=propertyInstanceId,propertyInstanceSourceId,propertyName,source,domain,address&fields[customerPropertyMap]=customerPropertyId,propertyInstanceId&fields[customerPropertyMap.customerProperty]=customerPropertyId,propertyName,isActive", booksPropertyResponse);
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/propertyinstance?filter[source]=UPFM&filter[companyPropertyInstanceMap.companyInstance.companyInstanceSourceId]=22222222-2222-2222-2222-222222222222&page[size]=9999&include=customerPropertyMap.customerProperty.customerPropertyOrderType&fields[propertyinstance]=propertyInstanceId,propertyInstanceSourceId,propertyName,source,domain,address&fields[customerPropertyMap]=customerPropertyId,propertyInstanceId&fields[customerPropertyMap.customerProperty]=customerPropertyId,propertyName,isActive", booksEmptyPropertyResponse);

            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/OS", booksTranslateOneSiteResponse);
            _mockHttpMessageHandler.Setup(HttpMethod.Put, $"http://localhost/propertyinstance/{propertyGuid}/{ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)}", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            _mockHttpMessageHandler.Setup(HttpMethod.Put, $"http://localhost/companyinstance/" + _RealPageId.ToString().ToLower() + "/UPFM", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            //Comment
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany?filter[customerCompanyId]=in:1&include=customerCompanyLocation", booksCustomerCompanyResponse);
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/domain/customercompany/1", booksCompanyMasterDomainListResponse);
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/companyinstance?filter[source]=UPFM&filter[customerCompanyMap.customerCompanyId]=1&fields[companyinstance]=companyInstanceId,source,companyInstanceSourceId,companyName,companyType,isActive,domain", booksCompanyInstancesResponse);
            
            _mockHttpMessageHandler.Setup(HttpMethod.Put, $"http://localhost/v2/provisioning/property", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });

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

        public bool TestSqlParameterContains(object p, string value)
        {
            return p.ToString().ToLower().Contains(value);
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
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

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
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

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
            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

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
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

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
            

        [Fact(Skip = "Scenario not in use")]
        public void InsertOrganization_AdminExits_BadRequest()
        {
            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            //Arrange
            UserLoginOnly userLoginOnly = new UserLoginOnly()
            {
                UserId = 3,
                PartyId = 1,
                LoginName = "jack.doe@example.com",
                PasswordHash = ""
            };

            mockRepository
                .Setup(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.IsAny<object>()))
                .Returns(userLoginOnly);

            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

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
            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

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

            mockRepository
                .Setup(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.IsAny<object>()))
                .Returns(userLoginOnly);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_SetupOrganization, It.IsAny<object>()))
                .Returns(repositoryResponse);

            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

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


            RepositoryResponse repositoryResponse = new RepositoryResponse()
            {
                Id = 0,
                ErrorMessage = "",
                RealPageId = _RealPageId
            };

            List<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() { Name = "UpdateProductInUDM", Value = "1" },
                new ProductInternalSetting() { Name = "SettingsApiEndPoint", Value = "http://localhost" }
            };

            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_SetupOrganization, It.IsAny<object>()))
                .Returns(repositoryResponse);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_CreateOrganizationProduct, It.IsAny<object>()))
                .Returns(repositoryResponse);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSetting, It.IsAny<object>()))
                .Returns(repositoryResponse);

            mockRepository
                .SetupSequence(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.IsAny<object>()))
                .Returns(userLoginOnly);

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(organizationDomainList);

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(_mockUnitofWork.Object);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestIsRealPageId(d, _RealPageId))))
                .Returns(_organizationList[0]);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            mockRepository
                .Setup(m => m.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization,
                    It.Is<object>(
                        d => TestIsRealPageId(d, _RealPageId))))
                .Returns(new List<ProductUI>() { new PersonaProductUserDetails() { ProductId = 1 } });

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(productInternalSettings);

            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/companyinstance", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            mockHttpMessageHandler.Setup(HttpMethod.Put, $"http://localhost/companyinstance/" + _RealPageId.ToString().ToLower() + "/UPFM", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/systemproductcenter", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/companyinstance/1051412/OS", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("\r\n{\"data\":{\"type\":\"companyinstance\",\"id\":\"651250\",\"attributes\":{\"companyInstanceId\":651250,\"source\":\"OS\",\"companyInstanceSourceId\":\"1051412\",\"companyName\":\"Camden\",\"phoneNumber\":\"(713) 354-2500\",\"formerlyKnownAs\":null,\"legalEntityName\":null,\"companyType\":null,\"website\":\"camdenproperty.onesite.realpage.com\",\"isActive\":true,\"isUat\":false,\"createdAt\":\"2017-04-13 11:56:12.000000-0500\",\"modifiedAt\":\"2020-09-14 16:35:16.000000-0500\",\"createdBy\":null,\"modifiedBy\":\"x - API\",\"deletedAt\":null,\"nrrReason\":null,\"deletedBy\":null,\"greenBookCares\":true,\"marketSegment\":[],\"nrr\":false,\"modifiedSource\":null,\"isAcquired\":false,\"customerEnvironment\":\"Primary\",\"domain\":\"Primary\",\"deletedReason\":\"Deprecated Field\"},\"links\":{\"self\":\"\\/companyinstance\\/651250\"}}}") });
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/v2/provisioning/company", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

            OrganizationCreate organizationCreate = new OrganizationCreate()
            {
                BooksCompanyId = _BooksMasterId,
                BooksCustomerMasterId = _BooksCompanyMasterId,
                OrganizationTypeId = 6,
                Name = "New Company",
                OrganizationDomain = "Primary",
                EnablePrimaryProperties = 0,
                EnableEnterpriseRoles = 0,
                Products = new List<string>()
                {
                    "OS"
                },
                AdminUser = new OrganizationAdminUser()
                {
                    FirstName = "Jack",
                    LastName = "Doe",
                    Email = "jack.doe@example.com",
                    Suffix = string.Empty,
                    Title = string.Empty
                },
                CompanyInstancePartner = "OS",
                CompanyInstancePartnerSourceId = "1051412",
                CompanyAddress = new CompanyInstanceAddress() { Address = "1234 Address", City = "Some City", State = "State", Country = "USA", PostalCode = "12345" }
            };

            //Act
            new RPObjectCache().BustCache();

            HttpResponseMessage response = organizationController.InsertOrganization(organizationCreate);
            OrganizationCreateResult orgResult = JsonConvert.DeserializeObject<OrganizationCreateResult>(response.Content.ReadAsStringAsync().Result);
            _output.WriteLine("httpstatuscode: " + response.StatusCode);
            _output.WriteLine("orgResult: " + response.Content.ReadAsStringAsync().Result);
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

            var companySetupList = new List<CompanySetup>()
            {
                new CompanySetup()
                {
                    OrganizationPartyId = 3,
                    OrganizationName = "RealPage",
                    ContractedName = "RealPage",
                    RealPageId = new Guid("daf71f77-4558-4cb0-91b8-29d8b0e62f15"),
                    BooksMasterId = "1",
                    BooksCustomerMasterId = "379",
                    OrganizationTypeId = 1,
                    OrganizationType = "Multifamily",
                    OrganizationDomainId = 1,
                    Domain = "Primary",
                    Products = 3
                }
            };

            Organization organization = new Organization();
            RepositoryResponse repositoryResponse = new RepositoryResponse()
            {
                Id = 0,
                ErrorMessage = "",
                RealPageId = _RealPageId
            };

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateOrganization, It.IsAny<object>()))
                .Returns(repositoryResponse);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSetting, It.IsAny<object>()))
                .Returns(repositoryResponse);

            mockRepository
                .Setup(m => m.Execute<string>(StoredProcNameConstants.SP_GetOrganizationSettingValue, null))
                .Returns("0");

            mockRepository
                .Setup(m => m.GetMany<CompanySetup>(StoredProcNameConstants.SP_ListCompanySetup,
                    It.IsAny<object>()))
                .Returns(companySetupList);

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_InsertBatchCompanyJob,
                    It.IsAny<object>()))
                .Returns(repositoryResponse);

            var IDPList = new List<IDPNames>()
            {
                new IDPNames() { IDPName = "Azure AD", ContactMechanismId = 45 },
                new IDPNames() { IDPName = "IdentityServer", ContactMechanismId = 46 }
            };

            mockRepository
                .Setup(m => m.GetMany<IDPNames>(StoredProcNameConstants.SP_OrganizationIDPList, It.IsAny<object>()))
                .Returns(IDPList);

            new RPObjectCache().BustCache();

            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

            OrganizationUpdate organizationUpdate = new OrganizationUpdate()
            {
                RealPageId = _RealPageId,
                //BooksMasterId = 0,
                //BooksCustomerMasterId = 0,
                OrganizationTypeId = _organizationTypeId,
                OrganizationDomainId = 0,
                OrganizationDomainName = "Primary",
                Name = "New Company",
                EnablePrimaryProperties = 0,
                EnableEnterpriseRoles = 0,
                ThirdPartyIDP = "None",
                CompanyAddress = new CompanyInstanceAddress() { Address = "1234 Address", City = "Some City", State = "State", Country = "USA", PostalCode = "12345" }
            };

            List<Company> mapResource = new List<Company>()
            {
                new Company()
                {
                    Id = "775",
                    CustomerCompanyId = 775,
                    CompanyName = "121 7TH STREET, LLC"
                }
            };

            var upfmCompanyInstancesJson = "{\"data\":[{\"type\":\"companyinstance\",\"id\":\"1049316\",\"attributes\":{\"companyInstanceId\":1049316,\"source\":\"UPFM\",\"companyInstanceSourceId\":\"daf71f77-4558-4cb0-91b8-29d8b0e62f15\",\"companyName\":\"CF Real Estate Services\",\"companyType\":\"Multifamily\",\"isActive\":true,\"domain\":\"Primary\",\"deletedReason\":\"Deprecated Field\",\"marketSegment\":[]},\"links\":{\"self\":\"\\/companyinstance\\/1049316\"}},{\"type\":\"companyinstance\",\"id\":\"1068792\",\"attributes\":{\"companyInstanceId\":1068792,\"source\":\"UPFM\",\"companyInstanceSourceId\":\"e072dcfc-99b8-493d-8f8d-26786c965d08\",\"companyName\":\"CF REAL ESTATE SERVICES - UAT\",\"companyType\":null,\"isActive\":true,\"domain\":\"UAT\",\"deletedReason\":\"Deprecated Field\",\"marketSegment\":[]},\"links\":{\"self\":\"\\/companyinstance\\/1068792\"}}]}";
            HttpResponseMessage upfmCompanyInstancesResponse = new HttpResponseMessage(HttpStatusCode.OK);
            upfmCompanyInstancesResponse.Content = new StringContent(upfmCompanyInstancesJson);

            HttpResponseMessage responseMapResource = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(mapResource, new JsonApiSerializerSettings());
            responseMapResource.Content = new StringContent(jsonToSave);

            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany?filter[customerCompanyId]=in:{_BooksCompanyMasterId}&include=customerCompanyLocation&fields[customercompany]=customerCompanyId,companyName,phoneNumber&fields[customerCompanyLocation]=customerCompanyLocationId,customerCompanyId,address,city,state,country,postalCode,isPrimary&page[size]=9999", responseMapResource);
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/companyinstance?filter[source]=UPFM&include=companyInstanceLocation&filter[companyInstanceSourceId]=in:daf71f77-4558-4cb0-91b8-29d8b0e62f15", upfmCompanyInstancesResponse);
            _mockHttpMessageHandler.Setup(HttpMethod.Put, $"http://localhost/v2/provisioning/company", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
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
            //Arrange		

            var companySetupList = new List<CompanySetup>()
            {
                new CompanySetup()
                {
                    OrganizationPartyId = 3,
                    OrganizationName = "RealPage",
                    ContractedName = "RealPage",
                    RealPageId = new Guid("daf71f77-4558-4cb0-91b8-29d8b0e62f15"),
                    BooksMasterId = "1",
                    BooksCustomerMasterId = "379",
                    OrganizationTypeId = 1,
                    OrganizationType = "Multifamily",
                    OrganizationDomainId = 1,
                    Domain = "Primary",
                    Products = 3
                }
            };

            UserLoginOnly userLoginOnlyNull = null;

            RepositoryResponse repositoryResponse = new RepositoryResponse()
            {
                Id = 0,
                ErrorMessage = "",
                RealPageId = _RealPageId
            };

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateOrganization, It.IsAny<object>()))
                .Returns(repositoryResponse);

            mockRepository
                .Setup(m => m.GetMany<CompanySetup>(StoredProcNameConstants.SP_ListCompanySetup,
                    It.IsAny<object>()))
                .Returns(companySetupList);

            var IDPList = new List<IDPNames>()
            {
                new IDPNames() { IDPName = "Azure AD", ContactMechanismId = 45 },
                new IDPNames() { IDPName = "IdentityServer", ContactMechanismId = 46 }
            };

            mockRepository
                .Setup(m => m.GetMany<IDPNames>(StoredProcNameConstants.SP_OrganizationIDPList, It.IsAny<object>()))
                .Returns(IDPList);


            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

            List<Company> mapResource = new List<Company>()
            {
                new Company()
                {
                    Id = "775",
                    CustomerCompanyId = 775,
                    CompanyName = "121 7TH STREET, LLC"
                }
            };

            var upfmCompanyInstancesJson = "{\"data\":[{\"type\":\"companyinstance\",\"id\":\"1049316\",\"attributes\":{\"companyInstanceId\":1049316,\"source\":\"UPFM\",\"companyInstanceSourceId\":\"daf71f77-4558-4cb0-91b8-29d8b0e62f15\",\"companyName\":\"CF Real Estate Services\",\"companyType\":\"Multifamily\",\"isActive\":true,\"domain\":\"Primary\",\"deletedReason\":\"Deprecated Field\",\"marketSegment\":[]},\"links\":{\"self\":\"\\/companyinstance\\/1049316\"}},{\"type\":\"companyinstance\",\"id\":\"1068792\",\"attributes\":{\"companyInstanceId\":1068792,\"source\":\"UPFM\",\"companyInstanceSourceId\":\"e072dcfc-99b8-493d-8f8d-26786c965d08\",\"companyName\":\"CF REAL ESTATE SERVICES - UAT\",\"companyType\":null,\"isActive\":true,\"domain\":\"UAT\",\"deletedReason\":\"Deprecated Field\",\"marketSegment\":[]},\"links\":{\"self\":\"\\/companyinstance\\/1068792\"}}]}";
            HttpResponseMessage upfmCompanyInstancesResponse = new HttpResponseMessage(HttpStatusCode.OK);
            upfmCompanyInstancesResponse.Content = new StringContent(upfmCompanyInstancesJson);

            HttpResponseMessage responseMapResource = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(mapResource, new JsonApiSerializerSettings());
            responseMapResource.Content = new StringContent(jsonToSave);

            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany?filter[customerCompanyId]=in:{_BooksCompanyMasterId}&include=customerCompanyLocation&fields[customercompany]=customerCompanyId,companyName,phoneNumber&fields[customerCompanyLocation]=customerCompanyLocationId,customerCompanyId,address,city,state,country,postalCode,isPrimary&page[size]=9999", responseMapResource);
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/companyinstance?filter[source]=UPFM&include=companyInstanceLocation&filter[companyInstanceSourceId]=in:daf71f77-4558-4cb0-91b8-29d8b0e62f15", upfmCompanyInstancesResponse);

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
                mockRepository.Object
                , mockRepositoryResponse.Object
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

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestIsRealPageId(d, realPageId))))
                .Returns(organization);

            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

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

            mockRepository
                .Setup(m => m.GetMany<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    null))
                .Returns(organizationList);

            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

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
            int bookMasterTypeId = (int)BookMasterType.CustomerMasterId;
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

            mockRepository
                .Setup(m => m.GetMany<CustomField>(StoredProcNameConstants.SP_GetFieldsByPartyId, It.IsAny<object>()))
                .Returns(customFieldList);

            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

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

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(organizationTypeList);

            mockRepository
                .Setup(m => m.GetOne<long>(StoredProcNameConstants.SP_DeleteOrganization,
                    It.IsAny<object>()))
                .Returns(0);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

            //Act
            new RPObjectCache().BustCache();

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
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

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
                    mockRepository.Object
                    , mockRepositoryResponse.Object
                    , _mockHttpMessageHandler.Object
                    , _defaultUserClaim
                )
                { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

            List<int> productList = new List<int>();
            List<string> blueBookProductList = new List<string>();

            // verify all blue book enums match a product
            foreach (var pi in typeof(BlueBookProductConstants).GetFields())
            {
                blueBookProductList.Add(pi.GetValue(pi).ToString());
            }

            foreach (var productCode in blueBookProductList)
            {
                productList.Add(ProductEnumHelper.GetProductIdByProductCode(productCode, _gbProductMap));
            }

            // list of products to exclude from Bluebook to product integration
            var ignoreProductList = new List<int>()
            {
                (int)ProductEnum.UnifiedUI, (int)ProductEnum.SelfProvisioningPortal, (int)ProductEnum.SalesForce, (int)ProductEnum.SettingsManagement, (int)ProductEnum.Facilities,(int)ProductEnum.LRConversionPortal,(int)ProductEnum.AdminSupportPortal,(int)ProductEnum.KnockCRM,(int)ProductEnum.G5LLMarketing,(int)ProductEnum.DataHub,(int)ProductEnum.SustainabilityServices , (int)ProductEnum.Web2PrintSocial, (int)ProductEnum.ESupply, (int)ProductEnum.ManagedServices, (int)ProductEnum.TrustDashboard
            };

            foreach (var pr in typeof(ProductEnum).GetFields())
            {
                if ((!pr.Name.Equals("value__", StringComparison.OrdinalIgnoreCase)) && (!pr.Name.Equals("UnifiedSettings", StringComparison.OrdinalIgnoreCase)))
                {
                    int current = (int)Enum.Parse(typeof(ProductEnum), pr.Name);
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
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

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
                PartyRoleTypeId = (int)UserRoleType.User,
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
                RoleTypeIdValidFrom = (int)UserRoleType.User,
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
                RoleTypeIdFrom = (int)UserRoleType.User,
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

            IList<OrganizationType> organizationTypeList = new List<OrganizationType>() { organizationType };
            IList<RelationshipType> relationshipTypeList = new List<RelationshipType>();
            relationshipTypeList.Add(new RelationshipType() { RelationshipTypeId = 44, Name = "test" });
            IList<RoleType> roleTypeList = new List<RoleType>();
            roleTypeList.Add(new RoleType() { Name = "User", PartyRoleTypeId = 401, ParentPartyRoleTypeId = 400 });
            roleTypeList.Add(new RoleType() { Name = "Property Management Company", PartyRoleTypeId = 202, ParentPartyRoleTypeId = 200 });

            List<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
                new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" }
            };

            mockRepository
                .Setup(m => m.GetMany<Organization>(StoredProcNameConstants.SP_ListOrganizationByRealPageId, It.IsAny<object>()))
                .Returns(organizationList);

            IUserLoginRepository userLoginRepository = new UserLoginRepository(mockRepository.Object);
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();
            ManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

            mockRepository
                .Setup(m => m.GetOne<PartyRelationship>(StoredProcNameConstants.SP_GetPartyRelationshipByRealPageId, It.IsAny<object>()))
                .Returns(partyRelationship);

            mockRepository
                .Setup(m => m.GetMany<RelationshipType>(StoredProcNameConstants.SP_ListRelationshipType, It.IsAny<object>()))
                .Returns(relationshipTypeList);

            mockRepository
                .Setup(m => m.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, It.IsAny<object>()))
                .Returns(roleTypeList);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(productInternalSettings);

            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            ) { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

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
                mockRepository.Object
                , mockRepositoryResponse.Object
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
                    RealPageId = new Guid("daf71f77-4558-4cb0-91b8-29d8b0e62f15"),
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
                new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
                new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
                new ProductInternalSetting() { Name = "BooksUseTranslatev2", Value = "0" }
            };

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<CompanySetup>(StoredProcNameConstants.SP_ListCompanySetup,
                    It.IsAny<object>()))
                .Returns(companySetupList);

            var IDPList = new List<IDPNames>()
            {
                new IDPNames() { IDPName = "Azure AD", ContactMechanismId = 45 },
                new IDPNames() { IDPName = "IdentityServer", ContactMechanismId = 46 }
            };

            mockRepository
                .Setup(m => m.GetMany<IDPNames>(StoredProcNameConstants.SP_OrganizationIDPList, It.IsAny<object>()))
                .Returns(IDPList);

            OrganizationController organizationController = new OrganizationController(
                    mockRepository.Object
                    , mockRepositoryResponse.Object
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

            var upfmCompanyInstancesJson = "{\"data\":[{\"type\":\"companyinstance\",\"id\":\"1049316\",\"attributes\":{\"companyInstanceId\":1049316,\"source\":\"UPFM\",\"companyInstanceSourceId\":\"daf71f77-4558-4cb0-91b8-29d8b0e62f15\",\"companyName\":\"CF Real Estate Services\",\"companyType\":\"Multifamily\",\"isActive\":true,\"domain\":\"Primary\",\"deletedReason\":\"Deprecated Field\",\"marketSegment\":[]},\"links\":{\"self\":\"\\/companyinstance\\/1049316\"}},{\"type\":\"companyinstance\",\"id\":\"1068792\",\"attributes\":{\"companyInstanceId\":1068792,\"source\":\"UPFM\",\"companyInstanceSourceId\":\"e072dcfc-99b8-493d-8f8d-26786c965d08\",\"companyName\":\"CF REAL ESTATE SERVICES - UAT\",\"companyType\":null,\"isActive\":true,\"domain\":\"UAT\",\"deletedReason\":\"Deprecated Field\",\"marketSegment\":[]},\"links\":{\"self\":\"\\/companyinstance\\/1068792\"}}]}";
            HttpResponseMessage upfmCompanyInstancesResponse = new HttpResponseMessage(HttpStatusCode.OK);
            upfmCompanyInstancesResponse.Content = new StringContent(upfmCompanyInstancesJson);

            HttpResponseMessage responseMapResource = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(mapResource, new JsonApiSerializerSettings());
            responseMapResource.Content = new StringContent(jsonToSave);

            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany?filter[customerCompanyId]=in:{_BooksCompanyMasterId}&include=customerCompanyLocation&fields[customercompany]=customerCompanyId,companyName,phoneNumber&fields[customerCompanyLocation]=customerCompanyLocationId,customerCompanyId,address,city,state,country,postalCode,isPrimary&page[size]=9999", responseMapResource);
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/companyinstance?filter[source]=UPFM&include=companyInstanceLocation&filter[companyInstanceSourceId]=in:daf71f77-4558-4cb0-91b8-29d8b0e62f15", upfmCompanyInstancesResponse);


            HttpResponseMessage response = organizationController.GetCompanyList("RealPage", null, null, null, null);

            ObjectListOutput<CompanySetup, IErrorData> propertyOutput = new ObjectListOutput<CompanySetup, IErrorData>();
            propertyOutput = response.Content.ReadAsAsync<ObjectListOutput<CompanySetup, IErrorData>>().Result;

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
            Assert.True(propertyOutput.list[0].RealPageId == companySetupList[0].RealPageId);
        }

        #endregion

        #region Update Company property list
        [Fact]
        public async Task UpdateCompanyProperties_InvalidCompanyInstanceSourceId_ReturnsBadRequest()
        {
            // Arrange
            var controller = new OrganizationController(
                mockRepository.Object,
                mockRepositoryResponse.Object,
                _mockHttpMessageHandler.Object,
                _defaultUserClaim)
            { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

            var batch = new CompanyPropertyBatch
            {
                CompanyInstanceSourceId = "not-a-guid",
                CompanyBatchJobId = 10,
                CreateUserPersonaId = 1,
                IsActive = 1
            };

            // Act
            var response = await controller.UpdateCompanyProperties(batch);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("\"Company Instance Id not supplied\"", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task UpdateCompanyProperties_InvalidPropertyInstanceId_ReturnsBadRequest()
        {
            // Arrange: mock _manageOrganization to return a property with empty InstanceId
            var mockManageOrg = new Mock<IManageOrganization>();
            var badPropertySetup = new CompanyPropertySetup
            {
                Property = new List<PropertySetup>
                {
                    new PropertySetup { InstanceId = Guid.Empty, Name = "PropA" }
                }
            };
            mockManageOrg
                .Setup(m => m.GetPropertiesForCompany(
                    It.IsAny<Guid>(),
                    null, null, null, null,
                    It.IsAny<IDictionary<object, object>>(),
                    It.IsAny<long>(),
                    0, null, null, null, null))
                .Returns(new List<CompanyPropertySetup> { badPropertySetup });

            InjectManageOrganizationIntoController(mockManageOrg);

            var controller = CreateController();
            var batch = new CompanyPropertyBatch
            {
                CompanyInstanceSourceId = Guid.NewGuid().ToString(),
                CompanyBatchJobId = 11,
                CreateUserPersonaId = 2,
                IsActive = 1
            };

            // Act
            var response = await controller.UpdateCompanyProperties(batch);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("\"Invalid parameter: propertyInstanceId\"", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task UpdateCompanyProperties_InvalidPropertyName_ReturnsBadRequest()
        {
            // Arrange: mock _manageOrganization to return a property with empty Name
            var mockManageOrg = new Mock<IManageOrganization>();
            var badPropertySetup = new CompanyPropertySetup
            {
                Property = new List<PropertySetup>
                {
                    new PropertySetup { InstanceId = Guid.NewGuid(), Name = "" }
                }
            };
            mockManageOrg
                .Setup(m => m.GetPropertiesForCompany(
                    It.IsAny<Guid>(),
                    null, null, null, null,
                    It.IsAny<IDictionary<object, object>>(),
                    It.IsAny<long>(),
                    0, null, null, null, null))
                .Returns(new List<CompanyPropertySetup> { badPropertySetup });

            InjectManageOrganizationIntoController(mockManageOrg);

            var controller = CreateController();
            var batch = new CompanyPropertyBatch
            {
                CompanyInstanceSourceId = Guid.NewGuid().ToString(),
                CompanyBatchJobId = 12,
                CreateUserPersonaId = 3,
                IsActive = 1
            };

            // Act
            var response = await controller.UpdateCompanyProperties(batch);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("\"Null parameter: propertyName\"", await response.Content.ReadAsStringAsync());
        }

        // Helper to inject mock IManageOrganization into private field (permitted without controller changes)
        private void InjectManageOrganizationIntoController(Mock<IManageOrganization> mockManageOrg)
        {
            var controller = CreateController();
            var field = typeof(OrganizationController).GetField("_manageOrganization", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(controller, mockManageOrg.Object);
            _injectedController = controller;
        }

        private OrganizationController _injectedController;

        private OrganizationController CreateController()
        {
            return _injectedController ?? new OrganizationController(
                mockRepository.Object,
                mockRepositoryResponse.Object,
                _mockHttpMessageHandler.Object,
                _defaultUserClaim)
            { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };
        }
        #endregion
        #region getUpdatePropertyList

        [Fact]
        public void GetPropertiesForCompany_InvalidInstanceId_ReturnBadRequest()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act           
            HttpResponseMessage response = organizationController.GetPropertiesForCompany(Guid.Empty, null, null, null);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public async void UpdatePropertyForOrganization_InvalidInstanceId_ReturnBadRequest()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            var upfmPropertyInstances = new List<UPFMPropertyInstance>()
            { new UPFMPropertyInstance() {   InstanceId = Guid.Empty,
                Name = "test property 1" } };

            //Act           
            HttpResponseMessage response = await organizationController.UpdatePropertyForOrganization(upfmPropertyInstances, Guid.NewGuid());

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public async void UpdatePropertyForOrganization_InvalidCompanyInstanceId_ReturnBadRequest()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

         
            var upfmPropertyInstances = new List<UPFMPropertyInstance>()
            { new UPFMPropertyInstance() {   InstanceId = Guid.Empty,
                Name = "test property 1" } };
            //Act           
            HttpResponseMessage response =await organizationController.UpdatePropertyForOrganization(upfmPropertyInstances, Guid.NewGuid());
      

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public async void UpdatePropertyForOrganization_InvalidPropertyName_ReturnBadRequest()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            var upfmPropertyInstances = new List<UPFMPropertyInstance>() 
            { new UPFMPropertyInstance() {   InstanceId = Guid.Empty,
                Name = "" } };
           
            //Act           
            HttpResponseMessage response =await organizationController.UpdatePropertyForOrganization(upfmPropertyInstances, Guid.NewGuid());

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
                    TotalRecords = 573,
                    OrderType = "PROXY Only",
                    CustomerStatus = true
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
                new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
                new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
                new ProductInternalSetting() { Name = "BooksUseTranslatev2", Value = "0" }
            };

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(productInternalSettings);

            Mock<IRepository> _mockPropertyRepository = new Mock<IRepository>();
            mockRepository
                .Setup(m => m.GetMany<PropertySetup>(StoredProcNameConstants.SP_GetPropertyInstanceListByIdWithPaging,
                    It.IsAny<object>()))
                .Returns(propertySetupList);

            Mock<ILdClient> mockLdClient = new Mock<ILdClient>();
            mockLdClient.Setup(m => m.BoolVariation(It.IsAny<string>(), It.IsAny<LaunchDarkly.Sdk.User>(), false))
                .Returns(true);

            OrganizationController organizationController = new OrganizationController(
                    mockRepository.Object
                    , mockRepositoryResponse.Object
                    , _mockHttpMessageHandler.Object
                    , mockLdClient.Object
                    ,null
                    , _defaultUserClaim)
                { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

            //Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            List<BooksPropertyInstance> mapResource = new List<BooksPropertyInstance>()
            {
                new BooksPropertyInstance()
                {
                    id = "1234",
                    attributes = new PropertyAttributesInstance()
                    {
                        propertyInstanceId = "1005251854",
                        propertyInstanceSourceId = "003b0509-1189-49dc-bbe6-01c5b6277a83",
                        propertyName = "COBBLESTONE COVE",
                        source = "UPFM",
                        domain = "UAT",
                        deletedReason = "Deprecated Field"
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

            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/propertyinstance?filter[source]=UPFM&filter[companyPropertyInstanceMap.companyInstance.companyInstanceSourceId]={_companyRealPageId}&page[size]=9999&include=customerPropertyMap.customerProperty.customerPropertyOrderType&fields[propertyinstance]=propertyInstanceId,propertyInstanceSourceId,propertyName,source,domain,address&fields[customerPropertyMap]=customerPropertyId,propertyInstanceId&fields[customerPropertyMap.customerProperty]=customerPropertyId,propertyName,isActive", responseMapResource);
            HttpResponseMessage response = organizationController.GetPropertiesForCompany(companyRealPageId, null, null, null);
            ObjectListOutput<CompanyPropertySetup, IErrorData> propertyOutput = new ObjectListOutput<CompanyPropertySetup, IErrorData>();
            propertyOutput = response.Content.ReadAsAsync<ObjectListOutput<CompanyPropertySetup, IErrorData>>().Result;

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
            Assert.True(propertyOutput.list[0].Property[0].InstanceId == setup[0].Property[0].InstanceId);
            Assert.True(propertyOutput.list[0].Domain[0] == setup[0].Domain[0]);
        }

        [Fact]
        public void GetPropertiesForCompanyByOperator_ValidRealPageId_ReturnOrganization()
        {
            //Arrange		
            Guid companyRealPageId = new Guid("11111111-1111-1111-1111-111111111111");
            
            string operatorCode = "operatorcode";
            string operatorValue = "operatorvalue";
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
                    OrderType = "PROXY Only",
					CustomerStatus = true,
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
            Dictionary<string, bool> allProperties = new Dictionary<string, bool>();
            IList<ProductProperty> companyProperties = new List<ProductProperty>
            {
                new ProductProperty
                {
                    ID = "e89233ef-6fae-4da6-8953-8a2b6814c960",
                    Name = "Ao Property One"
                }
            };

            IList<ProductProperty> noCompanyProperties = new List<ProductProperty>();

            ListResponse aoListResponse = new ListResponse()
            {
                Records = companyProperties.Cast<object>().ToList(),
                TotalRows = companyProperties.Count,
                RowsPerPage = companyProperties.Count,
                ErrorReason = string.Empty,
                TotalPages = 1,
                Additional = allProperties
            };

            ListResponse aoEmptyResponse = new ListResponse()
            {
                Records = noCompanyProperties.Cast<object>().ToList(),
                TotalRows = 0,
                RowsPerPage = 0,
                ErrorReason = string.Empty,
                TotalPages = 1,
                Additional = allProperties
            };

            List<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
                new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
                new ProductInternalSetting() { Name = "BooksUseTranslatev2", Value = "0" }
            };

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(productInternalSettings);

            Mock<IRepository> _mockPropertyRepository = new Mock<IRepository>();
            mockRepository
                .Setup(m => m.GetMany<PropertySetup>(StoredProcNameConstants.SP_GetPropertyInstanceListByIdWithPaging,
                    It.IsAny<object>()))
                .Returns(propertySetupList);

            Mock<ILdClient> mockLdClient = new Mock<ILdClient>();
            mockLdClient.Setup(m => m.BoolVariation(It.IsAny<string>(), It.IsAny<LaunchDarkly.Sdk.User>(), false))
                .Returns(true);

            Mock<IManageProductAssetOptimization> mockProductAssetOptimization = new Mock<IManageProductAssetOptimization>();
            mockProductAssetOptimization.Setup(m => m.GetPropertiesWithOperators(It.IsAny<long>(), It.IsAny<long>(), operatorCode, operatorValue))
                .Returns(aoListResponse);

            mockProductAssetOptimization.Setup(m => m.GetPropertiesWithOperators(It.IsAny<long>(), It.IsAny<long>(), operatorCode, "Invalid"))
                .Returns(aoEmptyResponse);

            OrganizationController organizationController = new OrganizationController(
                    mockRepository.Object
                    , mockRepositoryResponse.Object
                    , _mockHttpMessageHandler.Object
                    , mockLdClient.Object
                    , mockProductAssetOptimization.Object
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
                    attributes = new PropertyAttributesInstance()
                    {
                        propertyInstanceId = "1005251854",
                        propertyInstanceSourceId = "003b0509-1189-49dc-bbe6-01c5b6277a83",
                        propertyName = "COBBLESTONE COVE",
                        source = "UPFM",
                        domain = "UAT",
                        deletedReason = "Deprecated Field"
                    }
                }
            };

            List<UPFMPropertyInstance> upfmPropertyInstances = new List<UPFMPropertyInstance>()
            {
                new UPFMPropertyInstance() { InstanceId = new Guid("cb1f5a51-56cc-415c-9d8e-3d5e3f0f8b68"), Name = "test property 1" }
            };

            var mockManageBlueBook = new Mock<IManageBlueBook>();
            mockManageBlueBook
                .Setup(m => m.GetPropertyInstanceForCompany(
                    It.IsAny<Guid>()
                ))
                .Returns(mapResource);

            var booksTranslateOneSiteJson = "{\n\t\"data\": {\n\t\t\"type\": \"propertyinstancetranslations\",\n\t\t\"attributes\": [\n\t\t\t{\n\t\t\t\t\"propertyInstanceSourceId\": \"a5192995-aaaa-bbbb-8df2-f30f1b8dc752\",\n\t\t\t\t\"source\": \"UPFM\",\n\t\t\t\t\"translatedPropertyInstances\": [\n\t\t\t\t\t{\n\t\t\t\t\t\t\"source\": \"AO\",\n\t\t\t\t\t\t\"propertyInstanceSourceId\": \"e89233ef-6fae-4da6-8953-8a2b6814c960\"\n\t\t\t\t\t}\n\t\t\t\t]\n\t\t\t},\n\t\t\t{\n\t\t\t\t\"propertyInstanceSourceId\": \"a5192995-aaaa-bbbb-8df2-f30f1b8dc752\",\n\t\t\t\t\"source\": \"UPFM\",\n\t\t\t\t\"translatedPropertyInstances\": [\n\t\t\t\t\t{\n\t\t\t\t\t\t\"source\": \"AB\",\n\t\t\t\t\t\t\"propertyInstanceSourceId\": \"7654321\"\n\t\t\t\t\t}\n\t\t\t\t]\n\t\t\t}\n\t\t]\n\t}\n}";
            HttpResponseMessage booksTranslateOneSiteResponse = new HttpResponseMessage(HttpStatusCode.OK);

            booksTranslateOneSiteResponse.Content = new StringContent(booksTranslateOneSiteJson);
            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/AO", booksTranslateOneSiteResponse);

            List<PropertyInstance> propertyInstances = new List<PropertyInstance>()
            {
                new PropertyInstance() {PropertyInstanceSourceId = "cb1f5a51-56cc-415c-9d8e-3d5e3f0f8b68"},
                new PropertyInstance() {PropertyInstanceSourceId = "b6f475fc-7408-424b-a749-129035dcf57b"},
                new PropertyInstance() {PropertyInstanceSourceId = "a61481fc-5779-4546-8d5a-b29ecf139095"},
                new PropertyInstance() {PropertyInstanceSourceId = "d0ab0e33-4c04-4028-97f8-cda5a8423a30"},
            };

            UPFMPropertyInstanceRootObject propertyInstanceRoot = new UPFMPropertyInstanceRootObject() { data = new List<UPFMPropertyInstanceData>() { new UPFMPropertyInstanceData() { attributes = new UPFMPropertyInstanceAttributes() { propertyInstance = propertyInstances } } } };

            var jsonToSave = JsonConvert.SerializeObject(propertyInstanceRoot);
            HttpResponseMessage responsePropertyInstance = new HttpResponseMessage(HttpStatusCode.OK);
            responsePropertyInstance.Content = new StringContent(jsonToSave);
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/companypropertyinstancemap?include=propertyInstance&filter[source]=UPFM&filter[companyinstance.companyInstanceSourceId]={_defaultUserClaim.OrganizationRealPageGuid}&fields[propertyInstance]=propertyInstanceSourceId,propertyName,domain,isActive&filter[propertyInstance.isActive]=true&page[size]=9999", responsePropertyInstance);

            mockRepository.Setup(m => m.GetMany<UPFMPropertyInstance>(StoredProcNameConstants.SP_GetPropertyInstanceListById,
                                It.Is<object>(data => TestSqlParameter(data, "{ InstanceList = Dapper.TableValuedParameter }"))))
                            .Returns(upfmPropertyInstances);

            HttpResponseMessage response = organizationController.GetPropertiesForCompany(companyRealPageId, null, null, null, operatorCode: operatorCode, operatorValue: operatorValue);
            ObjectListOutput<CompanyPropertySetup, IErrorData> propertyOutput = new ObjectListOutput<CompanyPropertySetup, IErrorData>();
            propertyOutput = response.Content.ReadAsAsync<ObjectListOutput<CompanyPropertySetup, IErrorData>>().Result;

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
            Assert.True(propertyOutput.list[0].Property[0].InstanceId == setup[0].Property[0].InstanceId);
            Assert.True(propertyOutput.list[0].Domain[0] == setup[0].Domain[0]);

            response = organizationController.GetPropertiesForCompany(companyRealPageId, null, null, null, operatorCode: operatorCode, operatorValue: "Invalid");
            propertyOutput = response.Content.ReadAsAsync<ObjectListOutput<CompanyPropertySetup, IErrorData>>().Result;
            Assert.Empty(propertyOutput.list[0].Property);
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
                mockRepository.Object
                , mockRepositoryResponse.Object
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
                mockRepository.Object
                , mockRepositoryResponse.Object
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
            var userLogin2 = new UserLogin() { UserId = 4321, LoginName = "admin2@other.com", RealPageId = new Guid("88888888-8888-8888-8888-888888888888") };

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
                },
                new Organization()
                {
                    RealPageId = new Guid("22222222-2222-2222-2222-222222222222"),
                    CreateDate = _CreateDate,
                    Name = "Onesite Invalid Test Company",
                    PartyId = 54321,
                    BooksMasterId = 654321,
                    BooksCustomerMasterId = 7654321,
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
            List<dynamic> companyList2 = new List<dynamic>();

            var org = organizationList.FirstOrDefault(p => p.PartyId == 12345);
            string _mockJsonCompanyList = "{\r\n\t\t\"PartyId\": \"" + org.PartyId + "\",\r\n\t\t\"Name\": \"" + org.Name + "\",\r\n\t\t\"OrganizationRealPageId\": \"" + org.RealPageId + "\",\r\n\t\t\"BooksMasterId\": \"" + org.BooksMasterId + "\",\r\n\t\t\"BooksCustomerMasterId\": \"" + org.BooksCustomerMasterId + "\",\r\n\t\t\"SettingName\": \"RealPageEmployeeAccessID\",\r\n\t\t\"PersonRealPageId\": \"" + userLogin.RealPageId + "\",\r\n\t\t\"LoginName\": \"" + userLogin.LoginName + "\",\r\n\t}";
            companyList.Add(JsonConvert.DeserializeObject<dynamic>(_mockJsonCompanyList));

            var org2 = organizationList.FirstOrDefault(p => p.PartyId == 54321);
            _mockJsonCompanyList = "{\r\n\t\t\"PartyId\": \"" + org2.PartyId + "\",\r\n\t\t\"Name\": \"" + org2.Name + "\",\r\n\t\t\"OrganizationRealPageId\": \"" + org2.RealPageId + "\",\r\n\t\t\"BooksMasterId\": \"" + org2.BooksMasterId + "\",\r\n\t\t\"BooksCustomerMasterId\": \"" + org2.BooksCustomerMasterId + "\",\r\n\t\t\"SettingName\": \"RealPageEmployeeAccessID\",\r\n\t\t\"PersonRealPageId\": \"" + userLogin2.RealPageId + "\",\r\n\t\t\"LoginName\": \"" + userLogin2.LoginName + "\",\r\n\t}";
            companyList2.Add(JsonConvert.DeserializeObject<dynamic>(_mockJsonCompanyList));

            OrganizationStatus organizationStatus = new OrganizationStatus()
            {
                PartyId = org.PartyId,
                IsPending = true,
                IsActive = true,
                IsExpired = false,
                StatusTypeId = (int)UserUiStatusType.Active,
                Status = UserUiStatusType.Active,
                FromDate = new DateTime(2019, 1, 1)
            };

            var orgStatusList = new List<OrganizationStatus>() { organizationStatus };

            OrganizationStatus organizationStatus2 = new OrganizationStatus()
            {
                PartyId = org2.PartyId,
                IsPending = true,
                IsActive = true,
                IsExpired = false,
                StatusTypeId = (int)UserUiStatusType.Active,
                Status = UserUiStatusType.Active,
                FromDate = new DateTime(2019, 1, 1)
            };

            var org2StatusList = new List<OrganizationStatus>() { organizationStatus2 };

            var userOrganizationList = new List<UserOrganization>()
            {
                new UserOrganization()
                {
                    Name = org.Name,
                    OrganizationPartyId = org.PartyId,
                    OrganizationRealPageId = org.RealPageId,
                    PartyRoleTypeId = 402,
                    PersonaId = 4444
                }
            };

            var userOrganizationList2 = new List<UserOrganization>()
            {
                new UserOrganization()
                {
                    Name = org2.Name,
                    OrganizationPartyId = org2.PartyId,
                    OrganizationRealPageId = org2.RealPageId,
                    PartyRoleTypeId = 402,
                    PersonaId = 5555
                }
            };

            IList<ProductProperty> list = new List<ProductProperty>();
            list.Add(new ProductProperty() { ID = "1234567", Name = "OneSite property 1" });

            var oneSitePropertyResponse = new ListResponse()
            {
                TotalRows = list.Count,
                Records = list.Cast<object>().ToList(),
                IsError = false
            };

            var oneSitePropertyResponseEmpty = new ListResponse()
            {
                TotalRows = 0,
                IsError = false
            };

            List<UPFMPropertyInstance> upfmPropertyInstances = new List<UPFMPropertyInstance>()
            {
                new UPFMPropertyInstance() { InstanceId = new Guid("a5192995-aaaa-bbbb-8df2-f30f1b8dc752"), Name = "test property 1" }
            };

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestIsRealPageId(d, org.RealPageId))))
                .Returns(org);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestIsRealPageId(d, org2.RealPageId))))
                .Returns(org2);

            mockRepository
                .Setup(m => m.GetMany<dynamic>(StoredProcNameConstants.SP_ListOrganizations,
                    It.Is<object>(
                        d => TestIsRealPageId(d, org.RealPageId))))
                .Returns(companyList);

            mockRepository
                .Setup(m => m.GetMany<dynamic>(StoredProcNameConstants.SP_ListOrganizations,
                    It.Is<object>(
                        d => TestIsRealPageId(d, org2.RealPageId))))
                .Returns(companyList2);

            mockRepository.Setup(m => m.GetOne<UserLogin>(StoredProcNameConstants.SP_GetUserLogin,
                    It.Is<object>(
                        d => TestIsRealPageId(d, userLogin.RealPageId))))
                .Returns(userLogin);

            mockRepository.Setup(m => m.GetOne<UserLogin>(StoredProcNameConstants.SP_GetUserLogin,
                    It.Is<object>(
                        d => TestIsRealPageId(d, userLogin2.RealPageId))))
                .Returns(userLogin2);

            mockRepository.Setup(m => m.GetOne<string>(StoredProcNameConstants.SP_GetIdentityProviderTypeByLoginName, It.IsAny<object>()))
                .Returns("local");

            mockRepository.Setup(m => m.GetMany<OrganizationStatus>(StoredProcNameConstants.SP_ListOrganizationStatusByUserId,
                    It.Is<object>(data => TestSqlParameter(data, "{ userId = " + userLogin.UserId + " }"))))
                .Returns(orgStatusList);

            mockRepository.Setup(m => m.GetMany<OrganizationStatus>(StoredProcNameConstants.SP_ListOrganizationStatusByUserId,
                    It.Is<object>(data => TestSqlParameter(data, "{ userId = " + userLogin2.UserId + " }"))))
                .Returns(org2StatusList);

            mockRepository.Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.Is<object>(data => TestSqlParameter(data, "{ ProductId = , ProductGUID = , Name = , BooksProductCode =  }"))))
                .Returns(_gbProductMap);

            mockRepository.Setup(m => m.GetMany<UserOrganization>(StoredProcNameConstants.SP_ListOrganizationByLoginName,
                    It.Is<object>(data => TestSqlParameterContains(data, userLogin.LoginName))))
                .Returns(userOrganizationList);

            mockRepository.Setup(m => m.GetMany<UserOrganization>(StoredProcNameConstants.SP_ListOrganizationByLoginName,
                    It.Is<object>(data => TestSqlParameterContains(data, userLogin2.LoginName))))
                .Returns(userOrganizationList2);

            mockRepository.Setup(m => m.GetMany<UPFMPropertyInstance>(StoredProcNameConstants.SP_GetPropertyInstanceListById,
                    It.Is<object>(data => TestSqlParameter(data, "{ InstanceList = Dapper.TableValuedParameter }"))))
                .Returns(upfmPropertyInstances);


            Mock<IManageProductOneSite> mockManageProductOneSite = new Mock<IManageProductOneSite>();

            mockManageProductOneSite.Setup(m => m.GetOneSitePropertyList(
                    It.Is<long>(l => l == 4444)
                    , It.Is<long>(l => l == 0)
                    , It.Is<bool>(l => l == false)
                    , null
                ))
                .Returns(oneSitePropertyResponse);

            mockManageProductOneSite.Setup(m => m.GetOneSitePropertyList(
                    It.Is<long>(l => l == 5555)
                    , It.Is<long>(l => l == 0)
                    , It.Is<bool>(l => l == false)
                    , null
                ))
                .Returns(oneSitePropertyResponse);

            //Arrange
            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , mockManageProductOneSite.Object
                , defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };


            //Act
            new RPObjectCache().BustCache();

            HttpResponseMessage response = organizationController.AuditCompanyProductPropertiesToUPFM(new Guid("11111111-1111-1111-1111-111111111111"), (int)ProductEnum.OneSite);

            var responseResult = response.Content.ReadAsAsync<ObjectListOutput<PropertyAudit, IErrorData>>().Result;

            var validProductResponse = responseResult.list.FirstOrDefault(p => p.ProductInstanceId == "1234567");

            //Assert
            Assert.True(validProductResponse != null
                        && validProductResponse.Status.Equals("OK", StringComparison.OrdinalIgnoreCase)
                        && validProductResponse.Name.Equals("OneSite property 1", StringComparison.OrdinalIgnoreCase)
                        && validProductResponse.Domain.Equals("Primary", StringComparison.OrdinalIgnoreCase)
                        && validProductResponse.ProductInstanceId.Equals("1234567", StringComparison.OrdinalIgnoreCase)
                        && validProductResponse.UPFMInstanceId.Equals("a5192995-aaaa-bbbb-8df2-f30f1b8dc752", StringComparison.OrdinalIgnoreCase)
                        && validProductResponse.UPFMName.Equals("test property 1", StringComparison.OrdinalIgnoreCase)
                        && response.StatusCode.Equals(HttpStatusCode.OK));

            defaultUserClaim = new DefaultUserClaim()
            {
                CorrelationId = new Guid(), CustomerMasterId = _BooksCompanyMasterId, OrganizationRealPageGuid = EmployeeCompanyRealPageId
            };

            organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , mockManageProductOneSite.Object
                , defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            new RPObjectCache().BustCache();

            // OneSite data exists but no UPFM property instances can be found
            response = organizationController.AuditCompanyProductPropertiesToUPFM(new Guid("22222222-2222-2222-2222-222222222222"), (int)ProductEnum.OneSite);

            responseResult = response.Content.ReadAsAsync<ObjectListOutput<PropertyAudit, IErrorData>>().Result;
            Assert.True(responseResult.list[0].Status.Equals("No ID", StringComparison.OrdinalIgnoreCase)
                        && responseResult.list[0].Name.Equals("OneSite property 1", StringComparison.OrdinalIgnoreCase)
                        && responseResult.list[0].ProductInstanceId.Equals("1234567", StringComparison.OrdinalIgnoreCase)
                        && responseResult.list[0].UPFMInstanceId.Equals("a5192995-aaaa-bbbb-8df2-f30f1b8dc752", StringComparison.OrdinalIgnoreCase)
                        && String.IsNullOrEmpty(responseResult.list[0].UPFMName)
                        && response.StatusCode.Equals(HttpStatusCode.OK));
        }

        #endregion

        #region AuditPropertyExport

        [Fact]
        public void AuditCompanyProductPropertiesToUPFMExport_InvalidUserCompany()
        {
            DefaultUserClaim invalidDefaultUserClaim = new DefaultUserClaim()
            {
                CorrelationId = new Guid(),
                CustomerMasterId = _BooksCompanyMasterId,
                OrganizationRealPageGuid = Guid.NewGuid()
            };

            //Arrange
            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , invalidDefaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act           
            HttpResponseMessage response = organizationController.AuditCompanyProductPropertiesToUPFMExport(Guid.NewGuid(), (int)ProductEnum.OneSite);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public void AuditCompanyProductPropertiesToUPFMExport_EmptyUnknownCompany()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act           
            HttpResponseMessage response = organizationController.AuditCompanyProductPropertiesToUPFMExport(Guid.Empty, (int)ProductEnum.OneSite);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));

            //Act           
            response = organizationController.AuditCompanyProductPropertiesToUPFMExport(new Guid("00000000-0000-0000-0000-000000000000"), (int)ProductEnum.OneSite);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));

        }

        #endregion

        #region AddProperty

        [Fact]
        public void AddPropertyForOrganization_InvalidPropertyObject_ReturnBadRequest()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act           
            HttpResponseMessage response = organizationController.AddPropertyForOrganization(null, new Guid());

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public void AddPropertyForOrganization_InvalidCompanyInstanceId_ReturnBadRequest()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            UPFMPropertyInstance _propertyInstance = new UPFMPropertyInstance()
            {
                Name = "LACY COURT",
                Address = "8 W NELSON AVE  ALEXANDRIA  VA  22301 - 2059",
                City = "ALEXANDRIA",
                State = "VA",
                PostalCode = "22301-2059",
                County = "ALEXANDRIA CITY",
                Country = "UNITED STATES",
                CustomerPropertyId = "1234"
            };
            //Act           
            HttpResponseMessage response = organizationController.AddPropertyForOrganization(_propertyInstance, Guid.Empty);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public void AddPropertyForOrganization_EmptyName_ReturnBadRequest()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            UPFMPropertyInstance _propertyInstance = new UPFMPropertyInstance()
            {
                Name = "",
                Address = "8 W NELSON AVE  ALEXANDRIA  VA  22301 - 2059",
                City = "ALEXANDRIA",
                State = "VA",
                PostalCode = "22301-2059",
                County = "ALEXANDRIA CITY",
                Country = "UNITED STATES",
                CustomerPropertyId = "1234",
                Domain = "Primary"
            };

            //Act           
            HttpResponseMessage response = organizationController.AddPropertyForOrganization(_propertyInstance, new Guid());

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public void AddPropertyForOrganization_EmptyDomain_ReturnBadRequest()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            UPFMPropertyInstance _propertyInstance = new UPFMPropertyInstance()
            {
                Name = "LACY COURT",
                Address = "8 W NELSON AVE  ALEXANDRIA  VA  22301 - 2059",
                City = "ALEXANDRIA",
                State = "VA",
                PostalCode = "22301-2059",
                County = "ALEXANDRIA CITY",
                Country = "UNITED STATES",
                CustomerPropertyId = "1234",
                Domain = "Primary"
            };

            //Act           
            HttpResponseMessage response = organizationController.AddPropertyForOrganization(_propertyInstance, new Guid());

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public void AddPropertyForOrganization_EmptyCustomerPropertyId_ReturnBadRequest()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            UPFMPropertyInstance _propertyInstance = new UPFMPropertyInstance()
            {
                Name = "LACY COURT",
                Address = "8 W NELSON AVE  ALEXANDRIA  VA  22301 - 2059",
                City = "ALEXANDRIA",
                State = "VA",
                PostalCode = "22301-2059",
                County = "ALEXANDRIA CITY",
                Country = "UNITED STATES",
                CustomerPropertyId = "",
                Domain = ""
            };

            //Act           
            HttpResponseMessage response = organizationController.AddPropertyForOrganization(_propertyInstance, new Guid());

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }


        [Fact]
        public void AddPropertyForOrganization_ValidResponse()
        {
            //Arrange
            UPFMPropertyInstance _propertyInstance = new UPFMPropertyInstance()
            {
                Name = "LACY COURT",
                Address = "8 W NELSON AVE  ALEXANDRIA  VA  22301 - 2059",
                City = "ALEXANDRIA",
                State = "VA",
                PostalCode = "22301-2059",
                County = "ALEXANDRIA CITY",
                Country = "UNITED STATES",
                CustomerPropertyId = "1234",
                Domain = "Primary"
            };
            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePropertyInstance, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 12345, RealPageId = propertyGuid, ErrorMessage = "" });

            List<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
                new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
                new ProductInternalSetting() { Name = "BooksUseTranslatev2", Value = "0" },
                new ProductInternalSetting() { Name = "SettingsApiEndPoint", Value = "http://localhost" },
                new ProductInternalSetting() { Name = "UnifiedLoginServerClientName", Value = "unifiedlogin-server" },
                new ProductInternalSetting() { Name = "UnifiedLoginServerClientSecret", Value = "abcdefgh" }
            };

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(productInternalSettings);

            _mockTokenHelper
                .Setup(m => m.GetUnifiedLoginServerToken("unifiedsettingsapi"))
                .Returns("abcdedfghijklmnilol");
            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/v2/provisioning/property", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });

            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act           
            HttpResponseMessage response = organizationController.AddPropertyForOrganization(_propertyInstance, EmployeeCompanyRealPageId);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
        }

        #endregion

        #region SearchPropertyByPropertyId

        [Fact]
        public void SearchPropertyByBlueId_InvalidPropertyObject_ReturnBadRequest()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                    mockRepository.Object
                    , mockRepositoryResponse.Object
                    , _mockHttpMessageHandler.Object
                    , _defaultUserClaim
                )
                { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

            //Act           
            HttpResponseMessage response = organizationController.SearchPropertyByBlueId("0", "1");

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public void SearchPropertyByBlueId_InvalidCompanyInstance_ReturnBadRequest()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                    mockRepository.Object
                    , mockRepositoryResponse.Object
                    , _mockHttpMessageHandler.Object
                    , _defaultUserClaim
                )
                { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

            //Act           
            HttpResponseMessage response = organizationController.SearchPropertyByBlueId("1234", "0");

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public void SearchPropertyByBlueId_ReturnsValidResponse()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            var booksPropertyInstanceJson = "{\r\n\"data\": [\r\n{\r\n\"type\": \"propertyinstance\",\r\n\"id\": \"1005251854\",\r\n\"attributes\": {\r\n\"propertyInstanceId\": 1005251854,\r\n\"propertyInstanceSourceId\": \"003b0509-1189-49dc-bbe6-01c5b6277a83\",\r\n\"propertyName\": \"Cobblestone Cove\",\r\n\"source\": \"UPFM\",\r\n\"address\": {\r\n\"address\": null,\r\n\"city\": null,\r\n\"state\": null,\r\n\"country\": null,\r\n\"county\": null,\r\n\"postalCode\": null\r\n},\r\n\"domain\": \"Primary\",\r\n\"deletedReason\": \"Deprecated Field\",\r\n\"customerPropertyMap\": [\r\n{\r\n\"customerPropertyId\": 1234,\r\n\"propertyInstanceId\": 1005251854,\r\n\"customerProperty\": [\r\n{\r\n\"customerPropertyId\": 1405001,\r\n\"propertyName\": \"COBBLESTONE COVE\",\r\n\"hasMedia\": \"Deprecated Field\"\r\n}\r\n]\r\n}\r\n]\r\n}\r\n}\r\n],\r\n\"meta\": {\r\n\"totalEntities\": 610,\r\n\"totalPages\": 610\r\n}\r\n}";
            ;
            HttpResponseMessage booksPropertyInstanceResponse = new HttpResponseMessage(HttpStatusCode.OK);
            booksPropertyInstanceResponse.Content = new StringContent(booksPropertyInstanceJson);

            var booksCustomerPropertyInstanceJson = "{\r\n\"data\": {\r\n\"type\": \"customerproperty\",\r\n\"id\": \"1234\",\r\n\"attributes\": {\r\n\"customerPropertyId\": 149555,\r\n\"customerCompanyId\": 379,\r\n\"masterPropertyId\": 240208,\r\n\"propertyName\": \"501 CONNELL\",\r\n\"phaseParentId\": null,\r\n\"legalSiteName\": null,\r\n\"phoneNumber\": null,\r\n\"website\": null,\r\n\"email\": null,\r\n\"address\": {\r\n\"address\": \"501 CONNELL AVE SW\",\r\n\"city\": \"ATLANTA\",\r\n\"state\": \"GA\",\r\n\"country\": \"UNITED STATES\",\r\n\"county\": \"FULTON COUNTY\",\r\n\"postalCode\": \"30315-8123\",\r\n\"latitude\": 33.6833,\r\n\"longitude\": -84.405843\r\n},\r\n\"units\": 84,\r\n\"stories\": null,\r\n\"bedCount\": null,\r\n\"squareFeet\": null,\r\n\"yearBuilt\": null,\r\n\"renovationStartDate\": null,\r\n\"renovationEndDate\": null,\r\n\"createdAt\": \"2019-05-23 22:48:19.000000-0500\",\r\n\"modifiedAt\": \"2019-05-23 22:48:19.000000-0500\",\r\n\"deletedAt\": null,\r\n\"certifiedAt\": null,\r\n\"createdBy\": null,\r\n\"modifiedBy\": null,\r\n\"geocoded\": true,\r\n\"isUat\": false,\r\n\"apn\": null,\r\n\"fips\": null,\r\n\"propertyType\": \"Conventional\",\r\n\"propertySubType\": null,\r\n\"googleLatitude\": 33.6837717,\r\n\"googleLongitude\": -84.4058924,\r\n\"constructionStatus\": null,\r\n\"constructionType\": null,\r\n\"assetClass\": null,\r\n\"buildings\": null,\r\n\"modifiedSource\": null,\r\n\"migrationStatus\": \"staged\",\r\n\"hasMedia\": \"Deprecated Field\",\r\n\"mediaTypeId\": null,\r\n\"assetType\": null,\r\n\"isActive\": true,\r\n\"companyRelationship\": null,\r\n\"startDate\": null,\r\n\"endDate\": null\r\n}\r\n}\r\n}";
            HttpResponseMessage booksCustomerPropertyInstanceResponse = new HttpResponseMessage(HttpStatusCode.OK);
            booksCustomerPropertyInstanceResponse.Content = new StringContent(booksCustomerPropertyInstanceJson);

            var booksAssignedPropertyInstanceJson = "{\r\n\"data\": [\r\n{\r\n\"type\": \"propertyinstance\",\r\n\"id\": \"1005297957\",\r\n\"attributes\": {\r\n\"propertyInstanceId\": 1005297957,\r\n\"propertyInstanceSourceId\": \"974693db-23bd-4cc3-a984-fbf9e5bf189c\",\r\n\"propertyName\": \"141 LOFTS\",\r\n\"isActive\": false,\r\n\"domain\": \"Primary\",\r\n\"deletedReason\": \"Deprecated Field\",\r\n\"customerPropertyMap\": [\r\n{\r\n\"customerPropertyId\": 1234,\r\n\"propertyInstanceId\": 1005297957,\r\n\"customerProperty\": [\r\n{\r\n\"customerPropertyId\": 239608,\r\n\"propertyName\": \"141 LOFTS\",\r\n\"address\": {\r\n\"address\": \"141 DESIARD ST\",\r\n\"city\": \"MONROE\",\r\n\"state\": \"LA\",\r\n\"country\": \"UNITED STATES\",\r\n\"county\": \"OUACHITA PARISH\",\r\n\"postalCode\": \"71201-7385\",\r\n\"latitude\": 32.502026,\r\n\"longitude\": -92.117078\r\n},\r\n\"hasMedia\": \"Deprecated Field\"\r\n}\r\n]\r\n}\r\n]\r\n}\r\n}\r\n],\r\n\"meta\": {\r\n\"totalEntities\": 7,\r\n\"totalPages\": 7\r\n}\r\n}";
            HttpResponseMessage booksAssignedPropertyInstanceResponse = new HttpResponseMessage(HttpStatusCode.OK);
            booksAssignedPropertyInstanceResponse.Content = new StringContent(booksAssignedPropertyInstanceJson);

            var booksAssignedDomainPropertyInstanceJson = "{\r\n\"data\": [\r\n{\r\n\"type\": \"propertyinstance\",\r\n\"id\": \"12345\",\r\n\"attributes\": {\r\n\"propertyInstanceId\": 12345,\r\n\"propertyInstanceSourceId\": \"3102523\",\r\n\"propertyName\": \"141 Lofts\",\r\n\"source\": \"OS\",\r\n\"isActive\": true,\r\n\"domain\": \"Primary\",\r\n\"deletedReason\": \"Deprecated Field\",\r\n\"customerPropertyMap\": [\r\n{\r\n\"customerPropertyId\": 239608,\r\n\"propertyInstanceId\": 12345,\r\n\"customerProperty\": [\r\n{\r\n\"customerPropertyId\": 239608,\r\n\"propertyName\": \"141 LOFTS\",\r\n\"address\": {\r\n\"address\": \"141 DESIARD ST\",\r\n\"city\": \"MONROE\",\r\n\"state\": \"LA\",\r\n\"country\": \"UNITED STATES\",\r\n\"county\": \"OUACHITA PARISH\",\r\n\"postalCode\": \"71201-7385\",\r\n\"latitude\": 32.502026,\r\n\"longitude\": -92.117078\r\n},\r\n\"hasMedia\": \"Deprecated Field\"\r\n}\r\n]\r\n}\r\n]\r\n}\r\n}\r\n],\r\n\"meta\": {\r\n\"totalEntities\": 1,\r\n\"totalPages\": 1\r\n}\r\n}";
            HttpResponseMessage booksAssignedDomainPropertyInstanceResponse = new HttpResponseMessage(HttpStatusCode.OK);
            booksAssignedDomainPropertyInstanceResponse.Content = new StringContent(booksAssignedDomainPropertyInstanceJson);

            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/propertyinstance?filter[source]=UPFM&filter[companyPropertyInstanceMap.companyInstance.companyInstanceSourceId]=f5c090fa-78ab-452f-b504-98aafee09121&page[size]=9999&include=customerPropertyMap.customerProperty.customerPropertyOrderType&fields[propertyinstance]=propertyInstanceId,propertyInstanceSourceId,propertyName,source,domain,address&fields[customerPropertyMap]=customerPropertyId,propertyInstanceId&fields[customerPropertyMap.customerProperty]=customerPropertyId,propertyName,isActive", booksPropertyInstanceResponse);
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customerproperty/1234", booksCustomerPropertyInstanceResponse);
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/propertyinstance?filter[source]=UPFM&filter[customerPropertyMap.customerPropertyId]=1234&page[size]=9999& fields[propertyinstance]=propertyInstanceId,propertyName,domain,propertyInstanceSourceId,isActive&include=customerPropertyMap.customerProperty&fields[customerPropertyMap]=customerPropertyId,propertyInstanceId&fields[customerPropertyMap.customerProperty]=customerPropertyId,propertyName,address", booksAssignedPropertyInstanceResponse);
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/propertyinstance?filter[customerPropertyMap.customerPropertyId]=12345&page[size]%20=9999&%20fields[propertyinstance]=propertyInstanceId,propertyName,domain,propertyInstanceSourceId,isActive,source&include=customerPropertyMap.customerProperty&fields[customerPropertyMap]=customerPropertyId,propertyInstanceId&fields[customerPropertyMap.customerProperty]=customerPropertyId,propertyName,address", booksAssignedPropertyInstanceResponse);

            //Act           
            HttpResponseMessage response = organizationController.SearchPropertyByBlueId("1234", "12345");

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
        }

        #endregion

        #region Delete Property

        [Fact]
        public void DeletePropertyForOrganization_InvalidPropertyObject_ReturnBadRequest()
        {
            //Arrange           

            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act           
            HttpResponseMessage response = organizationController.DeleteProperty(Guid.Empty, Guid.NewGuid());

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public void DeletePropertyForOrganization_InValidPropertyObject_Returns_ErrorResponse()
        {
            //Arrange

            List<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
                new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
                new ProductInternalSetting() { Name = "BooksUseTranslatev2", Value = "0" },
                new ProductInternalSetting() { Name = "SettingsApiEndPoint", Value = "http://localhost" },
                new ProductInternalSetting() { Name = "UnifiedLoginServerClientName", Value = "unifiedlogin-server" },
                new ProductInternalSetting() { Name = "UnifiedLoginServerClientSecret", Value = "abcdefgh" }
            };

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(productInternalSettings);
            Guid propertyInstance = new Guid("a1ef0ac9-2f84-4288-b369-e59b1d6c13de");
            RepositoryResponse repository = new RepositoryResponse()
            {
                RealPageId = propertyInstance,
                Id = 0,
                ErrorMessage = "Property is in use. Cannot delete a property that is in use"
            };

            Mock<ManageBlueBook> _manageBlueBook = new Mock<ManageBlueBook>();

            Mock<IRepository> _mockPropertyRepository = new Mock<IRepository>();
            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DeletePropertyInstance,
                    It.IsAny<object>()))
                .Returns(repository);

            var booksTranslateOneSiteJson = "{\n\t\"data\": {\n\t\t\"type\": \"propertyinstancetranslations\",\n\t\t\"attributes\": [\n\t\t\t{\n\t\t\t\t\"propertyInstanceSourceId\": \"a1ef0ac9-2f84-4288-b369-e59b1d6c13de\",\n\t\t\t\t\"source\": \"UPFM\",\n\t\t\t\t\"translatedPropertyInstances\": [\n\t\t\t\t\t{\n\t\t\t\t\t\t\"source\": \"SET\",\n\t\t\t\t\t\t\"propertyInstanceSourceId\": \"e89233ef-6fae-4da6-8953-8a2b6814c960\"\n\t\t\t\t\t}\n\t\t\t\t]\n\t\t\t},\n\t\t\t{\n\t\t\t\t\"propertyInstanceSourceId\": \"a5192995-aaaa-bbbb-8df2-f30f1b8dc752\",\n\t\t\t\t\"source\": \"UPFM\",\n\t\t\t\t\"translatedPropertyInstances\": [\n\t\t\t\t\t{\n\t\t\t\t\t\t\"source\": \"AB\",\n\t\t\t\t\t\t\"propertyInstanceSourceId\": \"7654321\"\n\t\t\t\t\t}\n\t\t\t\t]\n\t\t\t}\n\t\t]\n\t}\n}";
            HttpResponseMessage booksTranslateOneSiteResponse = new HttpResponseMessage(HttpStatusCode.OK);
            booksTranslateOneSiteResponse.Content = new StringContent(booksTranslateOneSiteJson);
            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/SET", booksTranslateOneSiteResponse);
            _mockHttpMessageHandler.Setup(HttpMethod.Delete, $"http://localhost/propertyinstance/a1ef0ac9-2f84-4288-b369-e59b1d6c13de/UPFM?modifiedBy=UnifiedPlatform", new HttpResponseMessage(HttpStatusCode.NoContent) { Content = new StringContent("{ \"result\" : \"success\"}") });
            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestIsRealPageIdNull(d))))
                .Returns((_organizationList[0]));
            //Act           
            HttpResponseMessage response = organizationController.DeleteProperty(propertyInstance, Guid.NewGuid());

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
            RepositoryResponse repositoryResponse = JsonConvert.DeserializeObject<RepositoryResponse>(response.Content.ReadAsStringAsync().Result);
            Assert.True(repositoryResponse.ErrorMessage == repository.ErrorMessage);
        }

        [Fact]
        public void DeletePropertyForOrganization_ValidPropertyObject_Returns_Success()
        {
            //Arrange

            List<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
                new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
                new ProductInternalSetting() { Name = "BooksUseTranslatev2", Value = "0" },
                new ProductInternalSetting() { Name = "SettingsApiEndPoint", Value = "http://localhost" },
                new ProductInternalSetting() { Name = "UnifiedLoginServerClientName", Value = "unifiedlogin-server" },
                new ProductInternalSetting() { Name = "UnifiedLoginServerClientSecret", Value = "abcdefgh" }
            };

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(productInternalSettings);
            Guid propertyInstance = new Guid("a1ef0ac9-2f84-4288-b369-e59b1d6c13de");
            RepositoryResponse repository = new RepositoryResponse()
            {
                RealPageId = propertyInstance,
                Id = 0,
                ErrorMessage = ""
            };

            Mock<ManageBlueBook> _manageBlueBook = new Mock<ManageBlueBook>();

            Mock<IRepository> _mockPropertyRepository = new Mock<IRepository>();
            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DeletePropertyInstance,
                    It.IsAny<object>()))
                .Returns(repository);
            List<UPFMPropertyInstance> upfmPropertyInstances = new List<UPFMPropertyInstance>()
            {
                new UPFMPropertyInstance() { InstanceId = new Guid("a5192995-aaaa-bbbb-8df2-f30f1b8dc752"), Name = "test property 1" }
            };
            var booksTranslateOneSiteJson = "{\n\t\"data\": {\n\t\t\"type\": \"propertyinstancetranslations\",\n\t\t\"attributes\": [\n\t\t\t{\n\t\t\t\t\"propertyInstanceSourceId\": \"a1ef0ac9-2f84-4288-b369-e59b1d6c13de\",\n\t\t\t\t\"source\": \"UPFM\",\n\t\t\t\t\"translatedPropertyInstances\": [\n\t\t\t\t\t{\n\t\t\t\t\t\t\"source\": \"SET\",\n\t\t\t\t\t\t\"propertyInstanceSourceId\": \"e89233ef-6fae-4da6-8953-8a2b6814c960\"\n\t\t\t\t\t}\n\t\t\t\t]\n\t\t\t},\n\t\t\t{\n\t\t\t\t\"propertyInstanceSourceId\": \"a5192995-aaaa-bbbb-8df2-f30f1b8dc752\",\n\t\t\t\t\"source\": \"UPFM\",\n\t\t\t\t\"translatedPropertyInstances\": [\n\t\t\t\t\t{\n\t\t\t\t\t\t\"source\": \"AB\",\n\t\t\t\t\t\t\"propertyInstanceSourceId\": \"7654321\"\n\t\t\t\t\t}\n\t\t\t\t]\n\t\t\t}\n\t\t]\n\t}\n}";
            HttpResponseMessage booksTranslateOneSiteResponse = new HttpResponseMessage(HttpStatusCode.OK);
            booksTranslateOneSiteResponse.Content = new StringContent(booksTranslateOneSiteJson);

            mockRepository.Setup(m => m.GetMany<UPFMPropertyInstance>(StoredProcNameConstants.SP_GetPropertyInstanceListById,
                    It.Is<object>(data => TestSqlParameter(data, "{ InstanceList = Dapper.TableValuedParameter }"))))
                .Returns(upfmPropertyInstances);
            _mockHttpMessageHandler.Setup(HttpMethod.Delete, $"http://localhost/propertyinstance/a1ef0ac9-2f84-4288-b369-e59b1d6c13de/UPFM?modifiedBy=UnifiedPlatform", new HttpResponseMessage(HttpStatusCode.NoContent) { Content = new StringContent("{ \"result\" : \"success\"}") });
            _mockHttpMessageHandler.Setup(HttpMethod.Delete, $"http://localhost/v2/provisioning/property/e89233ef-6fae-4da6-8953-8a2b6814c960", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/SET", booksTranslateOneSiteResponse);
            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            //Act           
            HttpResponseMessage response = organizationController.DeleteProperty(propertyInstance, Guid.NewGuid());

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
        }

        #endregion

        #region GetProductStatusDetails

        [Fact]
        public void GetProductStatusDetails_InvalidProductInstanceId_ReturnBadRequest()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                    mockRepository.Object
                    , mockRepositoryResponse.Object
                    , _mockHttpMessageHandler.Object
                    , _defaultUserClaim
                )
                { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

            //Act           
            HttpResponseMessage response = organizationController.GetProductStatusDetails("0", "OS");

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public void GetProductStatusDetails_InvalidSource_ReturnBadRequest()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                    mockRepository.Object
                    , mockRepositoryResponse.Object
                    , _mockHttpMessageHandler.Object
                    , _defaultUserClaim
                )
                { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

            //Act           
            HttpResponseMessage response = organizationController.GetProductStatusDetails("1234", "");

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public void GetProductStatusDetails_ReturnsValidResponse()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            var booksProductDetailJson = "{\"data\":{\"type\":\"propertyinstance\",\"id\":\"1790436\",\"attributes\":{\"propertyInstanceId\":1790436,\"propertyInstanceSourceId\":\"12345\",\"propertyName\":\"Historical-Cobblestone Cove\",\"source\":\"OS\",\"phaseParentId\":null,\"legalSiteName\":null,\"website\":null,\"email\":null,\"notAPhysicalPropertyAddress\":null,\"isActive\":true,\"greenBookCares\":false,\"buildings\":null,\"nrr\":false,\"modifiedSource\":null,\"isAcquired\":false,\"isSoldOrTransfer\":null,\"soldOrTransferAt\":null,\"customerEnvironment\":\"Primary\",\"domain\":\"Primary\",\"customerPropertyMap\":[{\"customerPropertyId\":123456,\"propertyInstanceId\":1790436,\"propertyInstanceSourceId\":\"12345\",\"source\":\"OS\",\"customerProperty\":[{\"customerPropertyId\":123456,\"customerCompanyId\":379,\"masterPropertyId\":47093,\"propertyName\":\"COBBLESTONE COVE\",\"phaseParentId\":null,\"legalSiteName\":null,\"phoneNumber\":null,\"website\":null,\"email\":null,\"migrationStatus\":\"staged\",\"hasMedia\":\"Deprecated Field\",\"mediaTypeId\":null,\"assetType\":null,\"isActive\":false,\"companyRelationship\r\n\":\"Other\",\"startDate\":null,\"endDate\":null}]}]}}}";
            HttpResponseMessage booksProductDetailResponse = new HttpResponseMessage(HttpStatusCode.OK);
            booksProductDetailResponse.Content = new StringContent(booksProductDetailJson);

            var booksCustomerPropertyInstanceJson = "{\"data\":[{\"type\":\"propertyinstance\",\"id\":\"1005251854\",\"attributes\":{\"propertyInstanceId\":1005251854,\"propertyInstanceSourceId\":\"003b0509-1189-49dc-bbe6-01c5b6277a83\",\"propertyName\":\"Cobblestone Cove\",\"isActive\":true,\"domain\":\"Primary\",\"deletedReason\":\"Deprecated Field\",\"customerPropertyMap\":[{\"customerPropertyId\":123456,\"propertyInstanceId\":1005251854,\"customerProperty\":[{\"customerPropertyId\":123456,\"propertyName\":\"COBBLESTONE COVE\",\"hasMedia\":\"Deprecated Field\"}]}]}}],\"meta\":{\"totalEntities\":1,\"totalPages\":1}}";
            HttpResponseMessage booksCustomerPropertyInstanceResponse = new HttpResponseMessage(HttpStatusCode.OK);
            booksCustomerPropertyInstanceResponse.Content = new StringContent(booksCustomerPropertyInstanceJson);

            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/propertyinstance/12345/OS?include=customerPropertyMap.customerProperty", booksProductDetailResponse);
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/propertyinstance?filter[source]=UPFM&filter[customerPropertyMap.customerPropertyId]=123456&page[size]=9999& fields[propertyinstance]=propertyInstanceId,propertyName,domain,propertyInstanceSourceId,isActive&include=customerPropertyMap.customerProperty&fields[customerPropertyMap]=customerPropertyId,propertyInstanceId&fields[customerPropertyMap.customerProperty]=customerPropertyId,propertyName,address", booksCustomerPropertyInstanceResponse);


            //Act           
            HttpResponseMessage response = organizationController.GetProductStatusDetails("12345", "OS");

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
        }

        #endregion

        #region GetCompanyMasterByCustomerCompanyId

        [Fact]
        public void SearchCompanyDetailsByCustomerCompanyId_InvalidCustomerCompanyId_ReturnBadRequest()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act           
            HttpResponseMessage response = organizationController.GetCompanyMasterByCustomerCompanyId(0);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public void SearchCompanyDetailsByCustomerCompanyId_ValidCustomerCompanyId_ValidResponse()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act           
            HttpResponseMessage response = organizationController.GetCompanyMasterByCustomerCompanyId(1);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
        }

        #endregion

        #region UpdateUsePrimaryPropertyForOrganizationProduct

        [Fact]
        public void UpdateUsePrimaryPropertyForOrganizationProduct_InvalidorganizationPartyId_ReturnBadRequest()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                    mockRepository.Object
                    , mockRepositoryResponse.Object
                    , _mockHttpMessageHandler.Object
                    , _defaultUserClaim
                )
                { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

            //Act           
            HttpResponseMessage response = organizationController.UpdateUsePrimaryPropertyForOrganizationProduct(0, 1, false);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public void UpdateUsePrimaryPropertyForOrganizationProduct_InvalidProductId_ReturnBadRequest()
        {
            //Arrange
            OrganizationController organizationController = new OrganizationController(
                    mockRepository.Object
                    , mockRepositoryResponse.Object
                    , _mockHttpMessageHandler.Object
                    , _defaultUserClaim
                )
                { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };

            //Act           
            HttpResponseMessage response = organizationController.UpdateUsePrimaryPropertyForOrganizationProduct(1234, 0, true);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        #endregion


        #region Delete Organization

        [Fact]
        public void InsertOrganizationRemovalQueue_Success()
        {
            //Arrange
            OrganizationDelete organizationDelete = new OrganizationDelete() { OrganizationRealPageId = _RealPageId, RequestedBy = "Unit test" };
            OrganizationRemovalQueue organizationRemovalQueue = new OrganizationRemovalQueue()
            {
                OrganizationRealPageId = (Guid)organizationDelete.OrganizationRealPageId,
                RequestedBy = organizationDelete.RequestedBy,
                OrganizationName = _CompanyName,
                OrganizationDomain = "Primary",
                OrganizationPartyId = _PartyId,

            };

            mockRepository
                .Setup(m => m.GetOne<OrganizationRemovalQueue>(StoredProcNameConstants.SP_InsertOrganizationRemovalQueue,
                    It.IsAny<object>()))
                .Returns(organizationRemovalQueue);

            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act           
            HttpResponseMessage response = organizationController.InsertOrganizationToDelete(organizationDelete, false);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.Created));
        }

        [Fact]
        public void InsertOrganizationRemovalQueue_InvalidCompanyId()
        {
            //Arrange
            OrganizationDelete organizationDelete = new OrganizationDelete() { OrganizationRealPageId = Guid.Empty, RequestedBy = "Unit test" };

            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act           
            HttpResponseMessage response = organizationController.InsertOrganizationToDelete(organizationDelete, false);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public void OrganizationCleanup_Success()
        {
            //Arrange
            List<OrganizationRemovalQueue> organizationToDeletes =
                new List<OrganizationRemovalQueue>() { new OrganizationRemovalQueue() { OrganizationPartyId = 123, OrganizationRealPageId = _RealPageId, OrganizationRemovalQueueId = 20, OrganizationRemoveUDMData = true, OrganizationRemovalQueueStatusId = 0, OrganizationRemovalRetryCount = 0 } };

            mockRepository
                .Setup(m => m.GetMany<OrganizationRemovalQueue>(StoredProcNameConstants.SP_ListOrganizationToDelete,
                    It.IsAny<object>()))
                .Returns(organizationToDeletes);

            mockRepository
                .Setup(m => m.GetOne<long>(StoredProcNameConstants.SP_DeleteOrganization,
                    It.IsAny<object>()))
                .Returns(organizationToDeletes[0].OrganizationPartyId);

            mockRepository
                .Setup(m => m.GetOne<int>(StoredProcNameConstants.SP_UpdateOrganizationRemovalQueueStatus,
                    It.IsAny<object>()))
                .Returns(organizationToDeletes[0].OrganizationRemovalQueueId);

            mockRepository
                .Setup(m => m.GetOne<long>(StoredProcNameConstants.SP_DeleteOrganization,
                    It.IsAny<object>()))
                .Returns(0);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act           
            HttpResponseMessage response = organizationController.RunCompanyDatabaseDeleteAndUDMCleanUp();

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
        }

        [Fact]
        public void OrganizationCleanup_Error()
        {
            //Arrange
            List<OrganizationRemovalQueue> organizationToDeletes =
                new List<OrganizationRemovalQueue>() { new OrganizationRemovalQueue() { OrganizationPartyId = 123, OrganizationRealPageId = _RealPageId, OrganizationRemovalQueueId = 20, OrganizationRemoveUDMData = true, OrganizationRemovalQueueStatusId = 0, OrganizationRemovalRetryCount = 0 } };

            mockRepository
                .Setup(m => m.GetMany<OrganizationRemovalQueue>(StoredProcNameConstants.SP_ListOrganizationToDelete,
                    It.IsAny<object>()))
                .Returns(organizationToDeletes);

            mockRepository
                .Setup(m => m.GetOne<long>(StoredProcNameConstants.SP_DeleteOrganization,
                    It.IsAny<object>()))
                .Returns(0);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.ProspectContactCenter))))
                .Returns(_productInternalSettings);

            OrganizationController organizationController = new OrganizationController(
                mockRepository.Object
                , mockRepositoryResponse.Object
                , _mockHttpMessageHandler.Object
                , _defaultUserClaim
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act           
            HttpResponseMessage response = organizationController.RunCompanyDatabaseDeleteAndUDMCleanUp();

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
        }

        #endregion

        public bool TestIsProductId(object obj, int productId)
        {
            return obj.ToString().Contains($"ProductId = {productId}");
        }
    }
}
       