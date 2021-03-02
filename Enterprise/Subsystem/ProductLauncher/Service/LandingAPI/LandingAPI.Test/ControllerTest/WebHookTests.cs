using JsonApiSerializer;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.WebHook;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web.Http;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest
{
    [ExcludeFromCodeCoverage]
    public class WebHookTests
    {
        private static Guid _RealPageId = new Guid("C802694D-5553-4527-8616-3C0F434AE62D");
        private static Guid _adminRealPageId = new Guid("C802694D-1111-2222-3333-3C0F434AE62D");
        private static string _CompanyName = "Test Company";
        private static DateTime _CreateDate = DateTime.MaxValue.ToUniversalTime();
        private static int _PartyId = 54321;
        private static long _BooksMasterId = 12345;
        private static long _BooksCompanyMasterId = 15862;
        private static int _organizationTypeId = 6;
        private static string _organizationTypeName = "Multifamily";
        private static int _organizationDomainId = 1;
        private static string _organizationDomainName = "Primary";

        private static DefaultUserClaim _userClaim;

        private string _mockTiboWebHookSigningSecret = "1234567890";

        private readonly string _mockJson_books_customercompany_deleted = "{\r\n\t\"id\": \"601e13a6-7360-ceda-bf0c-41c62fa694c7\",\r\n\t\"topic\": \"books.customercompany.deleted\",\r\n\t\"createdAt\": \"2020-04-21T08:25:31-05:00\",\r\n\t\"payload\": {\r\n\t\t\"link\": \"/customercompany?filter[customerCompanyId]={customerCompanyId}&filter[deletedAt]=not:null\",\r\n\t\t\"payload\": {\r\n    \t\t\"customerCompanyId\": 15862,\r\n    \t\t\"deletedAt\": \"2020-02-21 11:35:43.000000-0600\",\r\n    \t\t\"replacementCustomerCompanyId\"  : 9999\r\n\t\t}\r\n\t}\r\n}\r\n";
        private readonly string _mockJson_books_customercompany_deleted_Signature = "2c136a645e98e682babdaba914c3ff2a81ac0d9fd41e60c9cd27e7fb74aef05d";
        
        private readonly string _mockJson_books_customercompany_deleted_missing_customercompanyid = "{\r\n\t\"id\": \"601e13a6-7360-ceda-bf0c-41c62fa694c7\",\r\n\t\"topic\": \"books.customercompany.deleted\",\r\n\t\"createdAt\": \"2020-04-21T08:25:31-05:00\",\r\n\t\"payload\": {\r\n\t\t\"link\": \"/customercompany?filter[customerCompanyId]={customerCompanyId}&filter[deletedAt]=not:null\",\r\n\t\t\"payload\": {\r\n    \t\t\"deletedAt\": \"2020-02-21 11:35:43.000000-0600\",\r\n    \t\t\"replacementCustomerCompanyId\"  : 9999\r\n\t\t}\r\n\t}\r\n}\r\n";
        private readonly string _mockJson_books_customercompany_deleted_missing_customercompanyid_Signature = "37ba2498b33d96dfdcce3cb6993ad66b48e879d0904b1090a6a7857ef4e0775c";

        private readonly string _mockJson_books_customercompany_deleted_null_customercompanyid = "{\r\n\t\"id\": \"601e13a6-7360-ceda-bf0c-41c62fa694c7\",\r\n\t\"topic\": \"books.customercompany.deleted\",\r\n\t\"createdAt\": \"2020-04-21T08:25:31-05:00\",\r\n\t\"payload\": {\r\n\t\t\"link\": \"/customercompany?filter[customerCompanyId]={customerCompanyId}&filter[deletedAt]=not:null\",\r\n\t\t\"payload\": {\r\n    \t\t\"customerCompanyId\": null,\r\n    \t\t\"deletedAt\": \"2020-02-21 11:35:43.000000-0600\",\r\n    \t\t\"replacementCustomerCompanyId\"  : 9999\r\n\t\t}\r\n\t}\r\n}\r\n";
        private readonly string _mockJson_books_customercompany_deleted_null_customercompanyid_Signature = "ad0f422783095cb66b87111ec0bdbb1bbc6eb34a2c074f0bd47e5a34410c2676";

        private readonly string _mockJson_books_customercompany_deleted_null_replacementcustomercompanyid = "{\r\n\t\"id\": \"601e13a6-7360-ceda-bf0c-41c62fa694c7\",\r\n\t\"topic\": \"books.customercompany.deleted\",\r\n\t\"createdAt\": \"2020-04-21T08:25:31-05:00\",\r\n\t\"payload\": {\r\n\t\t\"link\": \"/customercompany?filter[customerCompanyId]={customerCompanyId}&filter[deletedAt]=not:null\",\r\n\t\t\"payload\": {\r\n    \t\t\"customerCompanyId\": 15862,\r\n    \t\t\"deletedAt\": \"2020-02-21 11:35:43.000000-0600\",\r\n    \t\t\"replacementCustomerCompanyId\"  : null\r\n\t\t}\r\n\t}\r\n}\r\n";
        private readonly string _mockJson_books_customercompany_deleted_null_replacementcustomercompanyid_Signature = "e17c9b4448e1f3b8b2ce2d2277650199a798c58cced15c1ef257346ffbb3921e";
        
        private readonly string _mockJson_books_customerproperty_deleted = "{\r\n\t\"id\": \"601e13a6-7360-ceda-bf0c-41c62fa694c7\",\r\n\t\"topic\": \"books.customerproperty.deleted\",\r\n\t\"createdAt\": \"2020-04-21T08:25:31-05:00\",\r\n\t\"payload\": {\r\n\t\t  \"link\": \"/customerproperty?filter[customerPropertyId]={customerPropertyId}&filter[deletedAt]=not:null\",\r\n\t\t  \"payload\": {\r\n\t\t    \"customerPropertyId\": 199685,\r\n\t\t    \"deletedAt\": \"2020-02-21 11:35:43.000000-0600\",\r\n\t\t    \"replacementCustomerPropertyId\": 123456789\r\n\t\t  }\r\n\t}\r\n}\r\n";
        private readonly string _mockJson_books_customerproperty_deleted_Signature = "5a29729cf7e401e05905e2146761735e9bc23c80bd0d646c4d1d8674eae51c6c";
        
        private readonly string _mockJson_books_customerproperty_deleted_missing_customerpropertyid = "{\r\n\t\"id\": \"601e13a6-7360-ceda-bf0c-41c62fa694c7\",\r\n\t\"topic\": \"books.customerproperty.deleted\",\r\n\t\"createdAt\": \"2020-04-21T08:25:31-05:00\",\r\n\t\"payload\": {\r\n\t\t  \"link\": \"/customerproperty?filter[customerPropertyId]={customerPropertyId}&filter[deletedAt]=not:null\",\r\n\t\t  \"payload\": {\r\n\t\t    \"deletedAt\": \"2020-02-21 11:35:43.000000-0600\",\r\n\t\t    \"replacementCustomerPropertyId\": 123456789\r\n\t\t  }\r\n\t}\r\n}\r\n";
        private readonly string _mockJson_books_customerproperty_deleted_missing_customerpropertyid_Signature = "244036174789556c720aef46357fda9741aad802a579496014e61221ed99ea2c";

        private readonly string _mockJson_books_customerproperty_deleted_null_customerpropertyid = "{\r\n\t\"id\": \"601e13a6-7360-ceda-bf0c-41c62fa694c7\",\r\n\t\"topic\": \"books.customerproperty.deleted\",\r\n\t\"createdAt\": \"2020-04-21T08:25:31-05:00\",\r\n\t\"payload\": {\r\n\t\t  \"link\": \"/customerproperty?filter[customerPropertyId]={customerPropertyId}&filter[deletedAt]=not:null\",\r\n\t\t  \"payload\": {\r\n\t\t    \"customerPropertyId\": null,\r\n\t\t    \"deletedAt\": \"2020-02-21 11:35:43.000000-0600\",\r\n\t\t    \"replacementCustomerPropertyId\": 123456789\r\n\t\t  }\r\n\t}\r\n}\r\n";
        private readonly string _mockJson_books_customerproperty_deleted_null_customerpropertyid_Signature = "8c43650ba0b1e722120e4b9b7e264b76f6d3c817259d725faadba845361fc7b4";

        private readonly string _mockJson_books_customerproperty_deleted_null_replacementcustomerpropertyid = "{\r\n\t\"id\": \"601e13a6-7360-ceda-bf0c-41c62fa694c7\",\r\n\t\"topic\": \"books.customerproperty.deleted\",\r\n\t\"createdAt\": \"2020-04-21T08:25:31-05:00\",\r\n\t\"payload\": {\r\n\t\t  \"link\": \"/customerproperty?filter[customerPropertyId]={customerPropertyId}&filter[deletedAt]=not:null\",\r\n\t\t  \"payload\": {\r\n\t\t    \"customerPropertyId\": 199685,\r\n\t\t    \"deletedAt\": \"2020-02-21 11:35:43.000000-0600\",\r\n\t\t    \"replacementCustomerPropertyId\": null\r\n\t\t  }\r\n\t}\r\n}\r\n";
        private readonly string _mockJson_books_customerproperty_deleted_null_replacementcustomerpropertyid_Signature = "704e37bab4ae5534cc7f8f459e6b83b58e75c4d5e95a6b9a37ccf11bbb216fcb";

        private readonly string _mockJson_books_provisioning_upfmorder_create = "{\"id\":\"7ac983c3-bb3f-f5a6-baf1-e41b139d690b\",\"topic\":\"provisioning.upfmorder.create\",\"createdAt\":\"2020-05-19T12:59:54-05:00\",\"payload\":{\"source\":\"UPFM\",\"company\":{\"city\":\"SAN FRANCISCO\",\"state\":\"CA\",\"county\":\"SAN FRANCISCO COUNTY\",\"address\":\"1 BUSH ST STE 900\",\"country\":\"UNITED STATES\",\"postalCode\":\"94104-4425\",\"companyName\":\"VERITAS INVESTMENTS\",\"productCenters\":[],\"customerCompanyId\":1948,\"companyInstanceSourceId\":null},\"properties\":[{\"city\":\"SAN FRANCISCO\",\"state\":\"CA\",\"units\":35,\"county\":\"SAN FRANCISCO COUNTY\",\"address\":\"100 BRODERICK ST\",\"country\":\"UNITED STATES\",\"postalCode\":\"94117-3158\",\"propertyName\":\"100 BRODERICK\",\"productCenters\":[{\"productCenterSourceId\":\"17\"},{\"productCenterSourceId\":\"4\"}],\"customerPropertyId\":391411,\"propertyInstanceSourceId\":null}],\"customerEnvironment\":\"Primary\"}}";
        private readonly string _mockJson_books_provisioning_upfmorder_create_Signature = "13b82334dbf47345b48737af5eb59912870b4722aa1b33da9a02cfd418876acb";

        private readonly string _mockJson_books_provisioning_upfmorder_create_update = "{\"id\":\"7ac983c3-bb3f-f5a6-baf1-e41b139d690b\",\"topic\":\"provisioning.upfmorder.create\",\"createdAt\":\"2020-05-19T12:59:54-05:00\",\"payload\":{\"source\":\"UPFM\",\"company\":{\"city\":\"SAN FRANCISCO\",\"state\":\"CA\",\"county\":\"SAN FRANCISCO COUNTY\",\"address\":\"1 BUSH ST STE 900\",\"country\":\"UNITED STATES\",\"postalCode\":\"94104-4425\",\"companyName\":\"VERITAS INVESTMENTS\",\"productCenters\":[],\"customerCompanyId\":1948,\"companyInstanceSourceId\":\""+_RealPageId+"\"},\"properties\":[{\"city\":\"SAN FRANCISCO\",\"state\":\"CA\",\"units\":35,\"county\":\"SAN FRANCISCO COUNTY\",\"address\":\"100 BRODERICK ST\",\"country\":\"UNITED STATES\",\"postalCode\":\"94117-3158\",\"propertyName\":\"100 BRODERICK\",\"productCenters\":[{\"productCenterSourceId\":\"17\"},{\"productCenterSourceId\":\"4\"}],\"customerPropertyId\":391411,\"propertyInstanceSourceId\":null}],\"customerEnvironment\":\"Primary\"}}";
        private readonly string _mockJson_books_provisioning_upfmorder_create_update_Signature = "ab5d3ed246ec6a2ed01a33afc7f699e44b3e18ec7bb5b0a4fcd3dd4787370fed";

        private readonly string _mockJson_books_provisioning_upfmorder_create_nulldomain = "{\"id\":\"7ac983c3-bb3f-f5a6-baf1-e41b139d690b\",\"topic\":\"provisioning.upfmorder.create\",\"createdAt\":\"2020-05-19T12:59:54-05:00\",\"payload\":{\"source\":\"UPFM\",\"company\":{\"city\":\"SAN FRANCISCO\",\"state\":\"CA\",\"county\":\"SAN FRANCISCO COUNTY\",\"address\":\"1 BUSH ST STE 900\",\"country\":\"UNITED STATES\",\"postalCode\":\"94104-4425\",\"companyName\":\"VERITAS INVESTMENTS\",\"productCenters\":[],\"customerCompanyId\":1948,\"companyInstanceSourceId\":null},\"properties\":[{\"city\":\"SAN FRANCISCO\",\"state\":\"CA\",\"units\":35,\"county\":\"SAN FRANCISCO COUNTY\",\"address\":\"100 BRODERICK ST\",\"country\":\"UNITED STATES\",\"postalCode\":\"94117-3158\",\"propertyName\":\"100 BRODERICK\",\"productCenters\":[{\"productCenterSourceId\":\"17\"},{\"productCenterSourceId\":\"4\"}],\"customerPropertyId\":391411,\"propertyInstanceSourceId\":null}],\"customerEnvironment\":null}}";
        private readonly string _mockJson_books_provisioning_upfmorder_create_nulldomain_Signature = "5bf97d010439a93a40251271779f511f2514d15cfaf809dcebdb2b377577f510";
       
        private readonly string _mockJson_books_provisioning_upfmorder_cancel = "{\"id\":\"0ad8351d-c38b-abe2-0619-954d5836683b\",\"topic\":\"provisioning.upfmorder.cancel\",\"createdAt\":\"2020-05-19T12:59:54-05:00\",\"payload\":{\"source\":\"UPFM\",\"company\":{\"productCenters\":[{\"productCenterSourceId\":\"17\"}],\"companyInstanceSourceId\":\""+_RealPageId+ "\"},\r\n\t\t\"properties\":[]}}";
        private readonly string _mockJson_books_provisioning_upfmorder_cancel_Signature = "b64bda1dfd2fd5b77a24d69b780d761902d525ee8ac17cf8a8952a8445cd930c";

        private readonly string _mockJson_books_provisioning_upfmorder_cancel_Invalid_CompanyInstance = "{\"id\":\"0ad8351d-c38b-abe2-0619-954d5836683b\",\"topic\":\"provisioning.upfmorder.cancel\",\"createdAt\":\"2020-05-19T12:59:54-05:00\",\"payload\":{\"source\":\"UPFM\",\"company\":{\"productCenters\":[{\"productCenterSourceId\":\"17\"}],\"companyInstanceSourceId\":\"\"},\r\n\t\t\"properties\":[]}}";
        private readonly string _mockJson_books_provisioning_upfmorder_cancel_Invalid_CompanyInstance_Signature = "7c656ff8db5c511837cf66c34c1bb895f5d82b54081562dba74eb275e0783109";

        private readonly string _mockJson_books_provisioning_upfmorder_cancel_Invalid_ProductInstance = "{\"id\":\"0ad8351d-c38b-abe2-0619-954d5836683b\",\"topic\":\"provisioning.upfmorder.cancel\",\"createdAt\":\"2020-05-19T12:59:54-05:00\",\"payload\":{\"source\":\"UPFM\",\"company\":{\"productCenters\":[{\"productCenterSourceId\":\"\"}],\"companyInstanceSourceId\":\"" + _RealPageId + "\"},\r\n\t\t\"properties\":[]}}";
        private readonly string _mockJson_books_provisioning_upfmorder_cancel_Invalid_ProductInstance_Signature = "83eb77869a79e5e9754eca36024a40a591db928bfa48befff7c210dcb0434819";

        //private readonly string _mockJsonCompanyList = "[\r\n\t{\r\n\t\t\"PartyId\": \""+_PartyId+"\",\r\n\t\t\"Name\": \""+_CompanyName+"\",\r\n\t\t\"OrganizationRealPageId\": \""+_RealPageId+"\",\r\n\t\t\"BooksMasterId\": \""+_BooksMasterId+"\",\r\n\t\t\"BooksCustomerMasterId\": \""+_BooksCompanyMasterId+"\",\r\n\t\t\"SettingName\": \"RealPageEmployeeAccessID\",\r\n\t\t\"PersonRealPageId\": \"guid\",\r\n\t\t\"LoginName\": \"admin@test.com\",\r\n\t}\r\n]";
        private readonly string _mockJsonCompanyList = "{\r\n\t\t\"PartyId\": \""+_PartyId+"\",\r\n\t\t\"Name\": \""+_CompanyName+"\",\r\n\t\t\"OrganizationRealPageId\": \""+_RealPageId+"\",\r\n\t\t\"BooksMasterId\": \""+_BooksMasterId+"\",\r\n\t\t\"BooksCustomerMasterId\": \""+_BooksCompanyMasterId+"\",\r\n\t\t\"SettingName\": \"RealPageEmployeeAccessID\",\r\n\t\t\"PersonRealPageId\": \"guid\",\r\n\t\t\"LoginName\": \"admin@test.com\",\r\n\t}";

        private List<OrganizationType> _organizationTypeList;
        private List<OrganizationDomain> _organizationDomains;

        private Organization _organization = null;
        private List<ProductInternalSetting> _productInternalSettings;
        
        public WebHookTests()
        {
            _userClaim = new DefaultUserClaim() {CorrelationId = Guid.NewGuid()};

            _organization = new Organization()
            {
                RealPageId = _RealPageId,
                CreateDate = _CreateDate,
                Name = _CompanyName,
                PartyId = _PartyId,
                BooksMasterId = _BooksMasterId,
                BooksCustomerMasterId = _BooksCompanyMasterId,
                OrganizationTypeId = _organizationTypeId,
                organizationType = new OrganizationType()
                {
                    OrganizationTypeId = _organizationTypeId
                },
                OrganizationDomain = new OrganizationDomain()
                {
                    OrganizationDomainId = _organizationDomainId
                }
            };

            _organizationTypeList = new List<OrganizationType>()
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

            _organizationDomains = new List<OrganizationDomain>()
            {
                new OrganizationDomain()
                {
                    OrganizationDomainId = 1,
                    Name = "Primary",
                    CreateDate = new DateTime()
                },
                new OrganizationDomain()
                {
                    OrganizationDomainId = 2,
                    Name = "UAT",
                    CreateDate = new DateTime()
                }
            };

            _productInternalSettings = new List<ProductInternalSetting>() {new ProductInternalSetting() {Name = "TiboWebHookSigningSecret", Value = _mockTiboWebHookSigningSecret}};

        }

        [Fact]
        public void Post_Books_NullInput()
        {
            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"), Configuration = new HttpConfiguration()
            };

            //Act
            HttpResponseMessage response = webHookController.PostBooks(null);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);

            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"Missing Content.\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted_missing_customercompanyid);
            //Act
            response = webHookController.PostBooks(thinEvent);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);

            message = response.Content.ReadAsStringAsync().Result;
            expectedValue = "\"Missing Signature.\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
        }

        [Fact]
        public void Post_Books_Update_CustomerCompany_BooksMasterId_InvalidSignature()
        {
            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(() => null);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"), Configuration = new HttpConfiguration()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted_missing_customercompanyid);
            webHookController.Request.Headers.Add("signature", "12345");

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted_missing_customercompanyid);

            //Act
            var cacheKey = "productInternalSetting_" + (int)ProductEnum.UnifiedPlatform;
            ObjectCache cache = MemoryCache.Default;
            cache.Remove(cacheKey);

            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);

            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"Invalid Signature.\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
        }

        [Fact]
        public void Post_Books_Update_CustomerCompany_BooksMasterId_MissingSecretKey()
        {
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(() => null);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(new List<ProductInternalSetting>());

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"), Configuration = new HttpConfiguration()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted_missing_customercompanyid);
            webHookController.Request.Headers.Add("signature", "12345");

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted_missing_customercompanyid);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);

            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"Missing Signing Secret.\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
        }

        [Fact]
        public void Post_Books_Update_CustomerCompany_BooksMasterId_Success()
        {
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organization);

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DataImportMappingUpdate, It.IsAny<object>()))
                .Returns(new RepositoryResponse {Id = 1, ErrorMessage = ""});

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"), Configuration = new HttpConfiguration()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customercompany_deleted_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Update_CustomerCompany_BooksMasterId_CompanyNotFound()
        {
            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(() => null);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"), Configuration = new HttpConfiguration()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customercompany_deleted_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);

        }

        [Fact]
        public void Post_Books_Update_CustomerCompany_BooksMasterId_Failed()
        {
            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            List<dynamic> companyList = new List<dynamic>();
            companyList.Add(JsonConvert.DeserializeObject<dynamic>(_mockJsonCompanyList));

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organization);

            mockRepository
                .Setup(m => m.GetMany<dynamic>(StoredProcNameConstants.SP_ListOrganizations, null))
                .Returns(companyList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DataImportMappingUpdate, It.IsAny<object>()))
                .Returns(new RepositoryResponse {Id = 0, ErrorMessage = "SQL Error happened here"});

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"), Configuration = new HttpConfiguration()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customercompany_deleted_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);

            //Assert
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);

            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"SQL Error happened here id not updated\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
        }

        [Fact]
        public void Post_Books_Update_CustomerCompany_BooksMasterId_CompanyIdAttributeMissing()
        {
            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(() => null);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"), Configuration = new HttpConfiguration()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted_missing_customercompanyid);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customercompany_deleted_missing_customercompanyid_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted_missing_customercompanyid);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Update_CustomerCompany_BooksMasterId_CompanyIdAttributeNull()
        {
            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(() => null);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"), Configuration = new HttpConfiguration()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted_null_customercompanyid);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customercompany_deleted_null_customercompanyid_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted_null_customercompanyid);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Update_CustomerCompany_BooksMasterId_ReplacementCompanyIdAttributeNull()
        {
            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(() => null);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"), Configuration = new HttpConfiguration()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted_null_replacementcustomercompanyid);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customercompany_deleted_null_replacementcustomercompanyid_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted_null_replacementcustomercompanyid);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Update_CustomerProperty_BooksMasterId_Success()
        {
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePropertyMappingReMap, It.IsAny<object>()))
                .Returns(new RepositoryResponse {Id = 0, ErrorMessage = ""});

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"), Configuration = new HttpConfiguration()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customerproperty_deleted);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customerproperty_deleted_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customerproperty_deleted);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Update_CustomerProperty_CustomerPropertyIdAttributeMissing()
        {
            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"), Configuration = new HttpConfiguration()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customerproperty_deleted_missing_customerpropertyid);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customerproperty_deleted_missing_customerpropertyid_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customerproperty_deleted_missing_customerpropertyid);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Update_CustomerProperty_CustomerPropertyIdNull()
        {
            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"), Configuration = new HttpConfiguration()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customerproperty_deleted_null_customerpropertyid);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customerproperty_deleted_null_customerpropertyid_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customerproperty_deleted_null_customerpropertyid);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Update_CustomerProperty_ReplacementCustomerPropertyIdNull()
        {
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"), Configuration = new HttpConfiguration()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customerproperty_deleted_null_replacementcustomerpropertyid);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customerproperty_deleted_null_replacementcustomerpropertyid_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customerproperty_deleted_null_replacementcustomerpropertyid);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Provisioning_UPFMOrder_Create_Success()
        {
            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            CustomerCompany customercompany = new CustomerCompany() { CustomerCompanyId = 1948, IsActive = true, CompanyName = "Test Company",  MigrationStatus = "migrated", CompanyType = _organizationTypeName };//Category = "rpup"
            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>() {new CustomerCompanyMap() {CompanyInstanceSourceId = "1234567", Source = "OS"}};

            UserLoginOnly userLoginOnly = new UserLoginOnly()
            {
                UserId = 3,
                PartyId = 1,
                LoginName = $"{customercompany.CustomerCompanyId}admin@realpage.com",
                PasswordHash = ""
            };
            UserLoginOnly userLoginOnlyNull = null;
            
            Guid propertyGuid = new Guid("5C04F18A-FC9B-4A13-AAAF-E26DA83CE516");

            HttpResponseMessage responseCustomerCompany = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(customercompany, new JsonApiSerializerSettings());
            responseCustomerCompany.Content = new StringContent(jsonToSave);

            HttpResponseMessage responseMapResource  = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = JsonConvert.SerializeObject(mapResource, new JsonApiSerializerSettings());
            responseMapResource.Content = new StringContent(jsonToSave);

            HttpResponseMessage responsePropertyDetail= new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = "{\n\t\"data\": {\n\t\t\"type\": \"customerproperty\",\n\t\t\"id\": \"391411\",\n\t\t\"attributes\": {\n\t\t\t\"customerPropertyId\": 391411,\n\t\t\t\"customerCompanyId\": 1234,\n\t\t\t\"masterPropertyId\": 12345,\n\t\t\t\"propertyName\": \"Test Property\",\n\t\t\t\"address\": {\n\t\t\t\t\"address\": \"11623 Pleasant Meadow Dr\",\n\t\t\t\t\"city\": \"North Potomac\",\n\t\t\t\t\"state\": \"MD\",\n\t\t\t\t\"country\": \"USA\",\n\t\t\t\t\"county\": \"Montgomery\",\n\t\t\t\t\"postalCode\": \"20878-4258\",\n\t\t\t\t\"latitude\": 39.089137,\n\t\t\t\t\"longitude\": -77.238857\n\t\t\t},\n\t\t\t\"units\": null,\n\t\t\t\"stories\": null,\n\t\t\t\"bedCount\": null,\n\t\t\t\"squareFeet\": null,\n\t\t\t\"yearBuilt\": null,\n\t\t\t\"renovationStartDate\": null,\n\t\t\t\"renovationEndDate\": null,\n\t\t\t\"createdAt\": \"2020-08-07 12:49:59.000000-0500\",\n\t\t\t\"modifiedAt\": \"2020-08-07 12:49:59.000000-0500\",\n\t\t\t\"deletedAt\": null,\n\t\t\t\"certifiedAt\": null,\n\t\t\t\"createdBy\": null,\n\t\t\t\"modifiedBy\": null,\n\t\t\t\"geocoded\": true,\n\t\t\t\"isUat\": false,\n\t\t\t\"apn\": \"\",\n\t\t\t\"fips\": \"\",\n\t\t\t\"propertyType\": \"Company\",\n\t\t\t\"propertySubType\": null,\n\t\t\t\"googleLatitude\": null,\n\t\t\t\"googleLongitude\": null,\n\t\t\t\"constructionStatus\": \"Completed\",\n\t\t\t\"constructionType\": null,\n\t\t\t\"assetClass\": null,\n\t\t\t\"buildings\": null,\n\t\t\t\"modifiedSource\": null,\n\t\t\t\"migrationStatus\": null,\n\t\t\t\"hasMedia\": \"Deprecated Field\",\n\t\t\t\"mediaTypeId\": null,\n\t\t\t\"assetType\": \"Not an asset\",\n\t\t\t\"isActive\": false,\n\t\t\t\"companyRelationship\": null,\n\t\t\t\"startDate\": null,\n\t\t\t\"endDate\": null\n\t\t},\n\t\t\"links\": {\n\t\t\t\"self\": \"/customerproperty/391411\"\n\t\t}\n\t}\n}";
            responsePropertyDetail.Content = new StringContent(jsonToSave);

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organization);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_SetupOrganization, It.IsAny<object>()))
                .Returns(new RepositoryResponse {Id = 0, ErrorMessage = "", RealPageId = _RealPageId});

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DataImportMappingUpdate, It.IsAny<object>()))
                .Returns(new RepositoryResponse {Id = 1, ErrorMessage = ""});

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            mockRepository
                .SetupSequence(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.IsAny<object>()))
                .Returns(userLoginOnlyNull)
                .Returns(userLoginOnly);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_CreateOrganizationProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse {Id = 1, ErrorMessage = ""});

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePropertyInstance, It.IsAny<object>()))
                .Returns(new RepositoryResponse {Id = 12345, RealPageId = propertyGuid, ErrorMessage = ""});

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany/{customercompany.CustomerCompanyId}", responseCustomerCompany);
            //mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[companyInstance.greenBookCares]=true&filter[customerCompanyId]={customercompany.CustomerCompanyId}&include=companyInstance&include=companyInstance.attributes", responseMapResource);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[customerCompanyId]={customercompany.CustomerCompanyId}&include=companyInstance&include=companyInstance.attributes", responseMapResource);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customerproperty/391411", responsePropertyDetail);
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/companyinstance", new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent("{ \"result\" : \"success\"}")});
            mockHttpMessageHandler.Setup(HttpMethod.Put, $"http://localhost/propertyinstance/{propertyGuid}/{ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)}", new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent("{ \"result\" : \"success\"}")});
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/productcenterenablement/enable", new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent("{ \"result\" : \"success\"}")});

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"), Configuration = new HttpConfiguration()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmorder_create);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmorder_create_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmorder_create);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

                [Fact]
        public void Post_Books_Provisioning_UPFMOrder_Create_Update_Success()
        {
            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            CustomerCompany customercompany = new CustomerCompany() { CustomerCompanyId = 1948, IsActive = true, CompanyName = "Test Company",  MigrationStatus = "migrated", CompanyType = _organizationTypeName };//Category = "rpup"
            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>() {new CustomerCompanyMap() {CompanyInstanceSourceId = "1234567", Source = "OS"}};

            Guid companyRealPageId = new Guid();
            UserLoginOnly userLoginOnly = new UserLoginOnly()
            {
                UserId = 3,
                PartyId = 1,
                LoginName = $"{customercompany.CustomerCompanyId}admin@realpage.com",
                PasswordHash = ""
            };
            UserLoginOnly userLoginOnlyNull = null;

            Organization organization = new Organization()
            {
                Name = customercompany.CompanyName,
                RealPageId = new Guid("5C04F18A-FC9B-1234-AAAF-E26DA83CE516")
            };

            Guid propertyGuid = new Guid("5C04F18A-FC9B-4A13-AAAF-E26DA83CE516");

            HttpResponseMessage responseCustomerCompany = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(customercompany, new JsonApiSerializerSettings());
            responseCustomerCompany.Content = new StringContent(jsonToSave);

            HttpResponseMessage responseMapResource  = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = JsonConvert.SerializeObject(mapResource, new JsonApiSerializerSettings());
            responseMapResource.Content = new StringContent(jsonToSave);

            HttpResponseMessage responsePropertyDetail= new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = "{\n\t\"data\": {\n\t\t\"type\": \"customerproperty\",\n\t\t\"id\": \"391411\",\n\t\t\"attributes\": {\n\t\t\t\"customerPropertyId\": 391411,\n\t\t\t\"customerCompanyId\": 1234,\n\t\t\t\"masterPropertyId\": 12345,\n\t\t\t\"propertyName\": \"Test Property\",\n\t\t\t\"address\": {\n\t\t\t\t\"address\": \"11623 Pleasant Meadow Dr\",\n\t\t\t\t\"city\": \"North Potomac\",\n\t\t\t\t\"state\": \"MD\",\n\t\t\t\t\"country\": \"USA\",\n\t\t\t\t\"county\": \"Montgomery\",\n\t\t\t\t\"postalCode\": \"20878-4258\",\n\t\t\t\t\"latitude\": 39.089137,\n\t\t\t\t\"longitude\": -77.238857\n\t\t\t},\n\t\t\t\"units\": null,\n\t\t\t\"stories\": null,\n\t\t\t\"bedCount\": null,\n\t\t\t\"squareFeet\": null,\n\t\t\t\"yearBuilt\": null,\n\t\t\t\"renovationStartDate\": null,\n\t\t\t\"renovationEndDate\": null,\n\t\t\t\"createdAt\": \"2020-08-07 12:49:59.000000-0500\",\n\t\t\t\"modifiedAt\": \"2020-08-07 12:49:59.000000-0500\",\n\t\t\t\"deletedAt\": null,\n\t\t\t\"certifiedAt\": null,\n\t\t\t\"createdBy\": null,\n\t\t\t\"modifiedBy\": null,\n\t\t\t\"geocoded\": true,\n\t\t\t\"isUat\": false,\n\t\t\t\"apn\": \"\",\n\t\t\t\"fips\": \"\",\n\t\t\t\"propertyType\": \"Company\",\n\t\t\t\"propertySubType\": null,\n\t\t\t\"googleLatitude\": null,\n\t\t\t\"googleLongitude\": null,\n\t\t\t\"constructionStatus\": \"Completed\",\n\t\t\t\"constructionType\": null,\n\t\t\t\"assetClass\": null,\n\t\t\t\"buildings\": null,\n\t\t\t\"modifiedSource\": null,\n\t\t\t\"migrationStatus\": null,\n\t\t\t\"hasMedia\": \"Deprecated Field\",\n\t\t\t\"mediaTypeId\": null,\n\t\t\t\"assetType\": \"Not an asset\",\n\t\t\t\"isActive\": false,\n\t\t\t\"companyRelationship\": null,\n\t\t\t\"startDate\": null,\n\t\t\t\"endDate\": null\n\t\t},\n\t\t\"links\": {\n\t\t\t\"self\": \"/customerproperty/391411\"\n\t\t}\n\t}\n}";
            responsePropertyDetail.Content = new StringContent(jsonToSave);

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(organization);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_SetupOrganization, It.IsAny<object>()))
                .Returns(new RepositoryResponse {Id = 0, ErrorMessage = "", RealPageId = _RealPageId});

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DataImportMappingUpdate, It.IsAny<object>()))
                .Returns(new RepositoryResponse {Id = 1, ErrorMessage = ""});

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            mockRepository
                .SetupSequence(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.IsAny<object>()))
                .Returns(userLoginOnlyNull)
                .Returns(userLoginOnly);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_CreateOrganizationProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse {Id = 1, ErrorMessage = ""});

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePropertyInstance, It.IsAny<object>()))
                .Returns(new RepositoryResponse {Id = 12345, RealPageId = propertyGuid, ErrorMessage = ""});

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany/{customercompany.CustomerCompanyId}", responseCustomerCompany);
            //mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[companyInstance.greenBookCares]=true&filter[customerCompanyId]={customercompany.CustomerCompanyId}&include=companyInstance&include=companyInstance.attributes", responseMapResource);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[customerCompanyId]={customercompany.CustomerCompanyId}&include=companyInstance&include=companyInstance.attributes", responseMapResource);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customerproperty/391411", responsePropertyDetail);
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/companyinstance", new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent("{ \"result\" : \"success\"}")});
            mockHttpMessageHandler.Setup(HttpMethod.Put, $"http://localhost/propertyinstance/{propertyGuid}/{ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)}", new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent("{ \"result\" : \"success\"}")});
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/productcenterenablement/enable", new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent("{ \"result\" : \"success\"}")});
            
            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"), Configuration = new HttpConfiguration()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmorder_create_update);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmorder_create_update_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmorder_create_update);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }


        [Fact]
        public void Post_Books_Provisioning_UPFMOrder_Create_NullDomain()
        {
            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

           mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);
            
            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"), Configuration = new HttpConfiguration()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmorder_create_nulldomain);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmorder_create_nulldomain_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmorder_create_nulldomain);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);

            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"Missing customerEnvironment\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
        }

        [Fact]
        public void Post_Books_Provisioning_UPFMOrder_Create_MissingBlueBook()
        {
            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            HttpResponseMessage responsePropertyDetail= new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = "{\n\t\"data\": {\n\t\t\"type\": \"customerproperty\",\n\t\t\"id\": \"391411\",\n\t\t\"attributes\": {\n\t\t\t\"customerPropertyId\": 391411,\n\t\t\t\"customerCompanyId\": 1234,\n\t\t\t\"masterPropertyId\": 12345,\n\t\t\t\"propertyName\": \"Test Property\",\n\t\t\t\"address\": {\n\t\t\t\t\"address\": \"11623 Pleasant Meadow Dr\",\n\t\t\t\t\"city\": \"North Potomac\",\n\t\t\t\t\"state\": \"MD\",\n\t\t\t\t\"country\": \"USA\",\n\t\t\t\t\"county\": \"Montgomery\",\n\t\t\t\t\"postalCode\": \"20878-4258\",\n\t\t\t\t\"latitude\": 39.089137,\n\t\t\t\t\"longitude\": -77.238857\n\t\t\t},\n\t\t\t\"units\": null,\n\t\t\t\"stories\": null,\n\t\t\t\"bedCount\": null,\n\t\t\t\"squareFeet\": null,\n\t\t\t\"yearBuilt\": null,\n\t\t\t\"renovationStartDate\": null,\n\t\t\t\"renovationEndDate\": null,\n\t\t\t\"createdAt\": \"2020-08-07 12:49:59.000000-0500\",\n\t\t\t\"modifiedAt\": \"2020-08-07 12:49:59.000000-0500\",\n\t\t\t\"deletedAt\": null,\n\t\t\t\"certifiedAt\": null,\n\t\t\t\"createdBy\": null,\n\t\t\t\"modifiedBy\": null,\n\t\t\t\"geocoded\": true,\n\t\t\t\"isUat\": false,\n\t\t\t\"apn\": \"\",\n\t\t\t\"fips\": \"\",\n\t\t\t\"propertyType\": \"Company\",\n\t\t\t\"propertySubType\": null,\n\t\t\t\"googleLatitude\": null,\n\t\t\t\"googleLongitude\": null,\n\t\t\t\"constructionStatus\": \"Completed\",\n\t\t\t\"constructionType\": null,\n\t\t\t\"assetClass\": null,\n\t\t\t\"buildings\": null,\n\t\t\t\"modifiedSource\": null,\n\t\t\t\"migrationStatus\": null,\n\t\t\t\"hasMedia\": \"Deprecated Field\",\n\t\t\t\"mediaTypeId\": null,\n\t\t\t\"assetType\": \"Not an asset\",\n\t\t\t\"isActive\": false,\n\t\t\t\"companyRelationship\": null,\n\t\t\t\"startDate\": null,\n\t\t\t\"endDate\": null\n\t\t},\n\t\t\"links\": {\n\t\t\t\"self\": \"/customerproperty/391411\"\n\t\t}\n\t}\n}";
            responsePropertyDetail.Content = new StringContent(jsonToSave);


            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);
           
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customerproperty/391411", responsePropertyDetail);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"), Configuration = new HttpConfiguration()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmorder_create);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmorder_create_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmorder_create);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Provisioning_UPFMOrder_Cancel_Success()
        {
            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organization);

			mockRepository
				.Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
				.Returns(_organizationTypeList);

			mockRepository
				.Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
				.Returns(_organizationDomains);

			mockRepository
				.Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
				.Returns(_productInternalSettings);

			mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_DeleteOrganizationProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DisableUsersForProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 12345, ErrorMessage = "" });

           
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/productcenteractivation/cancel", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpConfiguration()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmorder_cancel);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmorder_cancel_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmorder_cancel);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Provisioning_UPFMOrder_Cancel_BadRequest_For_InvalidCompanyInstance()
        {
            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organization);

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_DeleteOrganizationProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DisableUsersForProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 12345, ErrorMessage = "" });


            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/productcenteractivation/cancel", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpConfiguration()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmorder_cancel_Invalid_CompanyInstance);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmorder_cancel_Invalid_CompanyInstance_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmorder_cancel_Invalid_CompanyInstance);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public void Post_Books_Provisioning_UPFMOrder_Cancel_BadRequest_For_InvalidProductInstance()
        {
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            Mock<IRepository> mockRepository = new Mock<IRepository>();
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organization);

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_DeleteOrganizationProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DisableUsersForProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 12345, ErrorMessage = "" });


            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/productcenteractivation/cancel", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpConfiguration()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmorder_cancel_Invalid_ProductInstance);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmorder_cancel_Invalid_ProductInstance_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmorder_cancel_Invalid_ProductInstance);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);
        }
    }

}
