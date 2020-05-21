using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.WebHook;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Castle.Components.DictionaryAdapter;
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
        private static long _BooksCompanyMasterId = 12345;
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

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim)
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

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(() => null);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim)
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
            var expectedValue = "\"Invalid Signature.\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
        }

        [Fact]
        public void Post_Books_Update_CustomerCompany_BooksMasterId_MissingSecretKey()
        {
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            Mock<IRepository> mockRepository = new Mock<IRepository>();

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(() => null);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(new List<ProductInternalSetting>());

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim)
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
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim)
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

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(() => null);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim)
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
                .Returns(new RepositoryResponse {Id = 0, ErrorMessage = "SQL Error happened here"});

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"), Configuration = new HttpConfiguration()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customercompany_deleted_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted);

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

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(() => null);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim)
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

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(() => null);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"), Configuration = new HttpConfiguration()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted_null_customercompanyid);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customercompany_deleted_null_customercompanyid_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted_null_customercompanyid);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Update_CustomerCompany_BooksMasterId_ReplacementCompanyIdAttributeNull()
        {
            Mock<IRepository> mockRepository = new Mock<IRepository>();

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(() => null);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim)
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
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim)
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

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim)
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

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim)
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

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim)
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
    }
}
