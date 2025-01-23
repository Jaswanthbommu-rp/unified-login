using Moq;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers.Product;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest
{
    [ExcludeFromCodeCoverage]
    public class ProductPanelTest : TestBase
    {
        #region Private Variables
        public static readonly Guid EmployeeCompanyRealPageId = new Guid("0D018E46-C20E-477D-ADED-4E5A35FB8F99");

        Mock<IUnitOfWork> _mockUnitofWork = new Mock<IUnitOfWork>();
        Mock<IRepositoryResponse> mockRepositoryResponse = new Mock<IRepositoryResponse>();
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

		#region Constructor
        public ProductPanelTest()
		{
            _gbProductMap = new List<GbProductMap>
            {
                new GbProductMap() {BooksProductCode = "OS", Name = "OneSite", ProductId = 1},
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

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);

            _defaultUserClaim.CorrelationId = new Guid();
            _defaultUserClaim.CustomerMasterId = _BooksCompanyMasterId;

			var booksTranslateOneSiteJson = "{\n\t\"data\": {\n\t\t\"type\": \"propertyinstancetranslations\",\n\t\t\"attributes\": [\n\t\t\t{\n\t\t\t\t\"propertyInstanceSourceId\": \"a5192995-aaaa-bbbb-8df2-f30f1b8dc752\",\n\t\t\t\t\"source\": \"UPFM\",\n\t\t\t\t\"translatedPropertyInstances\": [\n\t\t\t\t\t{\n\t\t\t\t\t\t\"source\": \"OS\",\n\t\t\t\t\t\t\"propertyInstanceSourceId\": \"1234567\"\n\t\t\t\t\t}\n\t\t\t\t]\n\t\t\t},\n\t\t\t{\n\t\t\t\t\"propertyInstanceSourceId\": \"a5192995-aaaa-bbbb-8df2-f30f1b8dc752\",\n\t\t\t\t\"source\": \"UPFM\",\n\t\t\t\t\"translatedPropertyInstances\": [\n\t\t\t\t\t{\n\t\t\t\t\t\t\"source\": \"AB\",\n\t\t\t\t\t\t\"propertyInstanceSourceId\": \"7654321\"\n\t\t\t\t\t}\n\t\t\t\t]\n\t\t\t}\n\t\t]\n\t}\n}";
			HttpResponseMessage booksTranslateOneSiteResponse = new HttpResponseMessage(HttpStatusCode.OK);
			booksTranslateOneSiteResponse.Content = new StringContent(booksTranslateOneSiteJson);
			_mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/OS", booksTranslateOneSiteResponse);


        }
        #endregion
        [Fact]
        public void GetProperties_InvalidEditorPersonaId_ReturnBadRequest()
        {
            DefaultUserClaim defaultUserClaim = new DefaultUserClaim()
            {
                CorrelationId = new Guid(),
                CustomerMasterId = _BooksCompanyMasterId,
                OrganizationRealPageGuid = EmployeeCompanyRealPageId              
            };
            ProductPanelController productPannelController = new ProductPanelController
                       (_defaultUserClaim
                       , mockRepository.Object
                       , mockRepositoryResponse.Object
                       , _mockHttpMessageHandler.Object
                       , null)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            RequestParameter datafilter = new RequestParameter();           
            HttpResponseMessage response = productPannelController.GetProperties(0, 0, (int)ProductEnum.OneSite, datafilter);
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public void GetPropeties_ValidResponse()
		{
            DefaultUserClaim defaultUserClaim = new DefaultUserClaim()
            {
                CorrelationId = new Guid(),
                CustomerMasterId = _BooksCompanyMasterId,
                OrganizationRealPageGuid = EmployeeCompanyRealPageId
            };
            
            List<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() {Name = "BooksUseDomains", Value = "1"},
                new ProductInternalSetting() {Name = "BooksUseUPFMId", Value = "1"}
            };
            Mock<IManageProductOneSite> mockManageProductOneSite = new Mock<IManageProductOneSite>();
            IList<ProductProperty> list = new List<ProductProperty>();
            list.Add(new ProductProperty() { ID = "1234567", Name = "OneSite property 1" });
            var oneSitePropertyResponse = new ListResponse()
            {
                TotalRows = list.Count,
                Records = list.Cast<object>().ToList(),
                IsError = false
            };

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
            new RPObjectCache().BustCache();

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                It.Is<object>(d => TestIsProductId(d, 1))))
                .Returns(productInternalSettings);


            ProductPanelController productPannelController = new ProductPanelController
                        (_defaultUserClaim
                        , mockRepository.Object
                        , mockRepositoryResponse.Object
                        , _mockHttpMessageHandler.Object
                        , mockManageProductOneSite.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            RequestParameter datafilter = new RequestParameter();
            HttpResponseMessage response = productPannelController.GetProperties(4444, 0, (int)ProductEnum.OneSite, null);
           
            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
        }

        [Fact]
        public void GetProperties_POST_InvalidEditorPersonaId_ReturnBadRequest()
        {
            DefaultUserClaim defaultUserClaim = new DefaultUserClaim()
            {
                CorrelationId = new Guid(),
                CustomerMasterId = _BooksCompanyMasterId,
                OrganizationRealPageGuid = EmployeeCompanyRealPageId
            };
            ProductPanelController productPannelController = new ProductPanelController
                       (_defaultUserClaim
                       , mockRepository.Object
                       , mockRepositoryResponse.Object
                       , _mockHttpMessageHandler.Object
                       , null)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            RequestParameter datafilter = new RequestParameter();
            HttpResponseMessage response = productPannelController.GetProperties(0, 0, (int)ProductEnum.OneSite, datafilter);
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public void GetPropeties_POST_ValidResponse()
        {
            DefaultUserClaim defaultUserClaim = new DefaultUserClaim()
            {
                CorrelationId = new Guid(),
                CustomerMasterId = _BooksCompanyMasterId,
                OrganizationRealPageGuid = EmployeeCompanyRealPageId
            };

            List<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() {Name = "BooksUseDomains", Value = "1"},
                new ProductInternalSetting() {Name = "BooksUseUPFMId", Value = "1"}
            };
            Mock<IManageProductOneSite> mockManageProductOneSite = new Mock<IManageProductOneSite>();
            IList<ProductProperty> list = new List<ProductProperty>();
            list.Add(new ProductProperty() { ID = "1234567", Name = "OneSite property 1" });
            var oneSitePropertyResponse = new ListResponse()
            {
                TotalRows = list.Count,
                Records = list.Cast<object>().ToList(),
                IsError = false
            };

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
            new RPObjectCache().BustCache();

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, 1))))
                .Returns(productInternalSettings);

            List<PropertyInstance> propertyInstances = new List<PropertyInstance>();
            var propertyInstanceRoot = new UPFMPropertyInstanceRootObject() { data = new List<UPFMPropertyInstanceData>() { new UPFMPropertyInstanceData() { attributes = new UPFMPropertyInstanceAttributes() { propertyInstance = propertyInstances } } } };

            var responsePropertyInstance = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(propertyInstanceRoot);
            responsePropertyInstance.Content = new StringContent(jsonToSave);
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/companypropertyinstancemap?include=propertyInstance&filter[source]=UPFM&filter[companyinstance.companyInstanceSourceId]={EmployeeCompanyRealPageId}&fields[propertyInstance]=propertyInstanceSourceId,propertyName,domain,isActive&filter[propertyInstance.isActive]=true&page[size]=9999", responsePropertyInstance);

            ProductPanelController productPanelController = new ProductPanelController
                        (defaultUserClaim
                        , mockRepository.Object
                        , mockRepositoryResponse.Object
                        , _mockHttpMessageHandler.Object
                        , mockManageProductOneSite.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            RequestParameter datafilter = new RequestParameter();
            UPFMProperty _upfmProperty = new UPFMProperty();
			List<string> instanceIds = new List<string>
			{
				"a5192995-aaaa-bbbb-8df2-f30f1b8dc752"
			};
			_upfmProperty.id = instanceIds;
            HttpResponseMessage response = productPanelController.GetProperties(4444, 0, (int)ProductEnum.OneSite, null, _upfmProperty);
            var productList = response.Content.ReadAsAsync<ListResponse>().Result.Records.Cast<ProductProperty>();

            //Assert 
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
            Assert.Equal("1234567", productList.FirstOrDefault().ID.ToString());
            Assert.Equal(false, productList.FirstOrDefault().IsAssigned);
            Assert.Equal("OneSite property 1", productList.FirstOrDefault().Name);
        }

        [Fact]
        public void GetPersonaProductPrimaryProperties_ValidResponse()
        {
            List<PersonaProductProperty> personaProductProperty = new List<PersonaProductProperty>()
            {
                new PersonaProductProperty() {PersonaProductPropertyId = 1, PersonaId = 1234, ProductId = 1, PropertyId = "12345",  PropertyInstanceId = "ceb51735-445b-448b-af7a-72f297c1ca16"},
                 new PersonaProductProperty() {PersonaProductPropertyId = 2, PersonaId = 123, ProductId = 2, PropertyId = "1234",  PropertyInstanceId = "ceb51735-445b-448b-af7a-72f297c1ca16"}
            };
            Mock<IManageProductOneSite> mockManageProductOneSite = new Mock<IManageProductOneSite>();
            IList<ProductProperty> list = new List<ProductProperty>();
            list.Add(new ProductProperty() { ID = "1234567", Name = "OneSite property 1" });
            var oneSitePropertyResponse = new ListResponse()
            {
                TotalRows = list.Count,
                Records = list.Cast<object>().ToList(),
                IsError = false
            };

            mockRepository
                .Setup(m => m.GetMany<PersonaProductProperty>(StoredProcNameConstants.SP_GetPersonaProductPrimaryProperties, It.IsAny<Object>()))
                .Returns(personaProductProperty);


            ProductPanelController productPannelController = new ProductPanelController
                        (_defaultUserClaim
                        , mockRepository.Object
                        , mockRepositoryResponse.Object
                        , _mockHttpMessageHandler.Object
                        , mockManageProductOneSite.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            RequestParameter datafilter = new RequestParameter();
            HttpResponseMessage response = productPannelController.GetPersonaProductPrimaryProperties(1234);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.OK));
        }

        public bool TestIsProductId(object obj, int productId)
        {
            return obj.ToString().Contains($"ProductId = {productId}");
        }
    }
}
