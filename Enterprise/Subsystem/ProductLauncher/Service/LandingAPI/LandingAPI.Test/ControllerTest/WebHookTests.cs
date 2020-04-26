using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.WebHook;
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
        private static long _BooksCompanyMasterId = 12345;
        private static int _organizationTypeId = 6;
        private static string _organizationTypeName = "Multifamily";

        private string _mockTiboWebHookSigningSecret = "1234567890";

        private string _mocjJson_books_customercompany_deleted = "{\r\n\t\"id\": \"601e13a6-7360-ceda-bf0c-41c62fa694c7\",\r\n\t\"topic\": \"books.customercompany.deleted\",\r\n\t\"createdAt\": \"2020-04-21T08:25:31-05:00\",\r\n\t\"payload\": {\r\n\t\t\"link\": \"/customercompany?filter[customerCompanyId]={customerCompanyId}&filter[deletedAt]=not:null\",\r\n\t\t\"payload\": {\r\n    \t\t\"customerCompanyId\": 15862,\r\n    \t\t\"deletedAt\": \"2020-02-21 11:35:43.000000-0600\",\r\n    \t\t\"replacementCustomerCompanyId\"  : 9999\r\n\t\t}\r\n\t}\r\n}\r\n";
        private string _mockJson_books_customercompany_deleted_invalidata = "{\r\n\t\"id\": \"601e13a6-7360-ceda-bf0c-41c62fa694c7\",\r\n\t\"topic\": \"books.customercompany.deleted\",\r\n\t\"createdAt\": \"2020-04-21T08:25:31-05:00\",\r\n\t\"payload\": {\r\n\t\t\"link\": \"/customercompany?filter[customerCompanyId]={customerCompanyId}&filter[deletedAt]=not:null\",\r\n\t\t\"payload\": {\r\n    \t\t\"deletedAt\": \"2020-02-21 11:35:43.000000-0600\",\r\n    \t\t\"replacementCustomerCompanyId\"  : 9999\r\n\t\t}\r\n\t}\r\n}\r\n";

        private readonly string _mockJson_books_customercompany_deleted_Signature = "2c136a645e98e682babdaba914c3ff2a81ac0d9fd41e60c9cd27e7fb74aef05d";
        private readonly string _mockJson_books_customercompany_deleted_invalidata_Signature = "37ba2498b33d96dfdcce3cb6993ad66b48e879d0904b1090a6a7857ef4e0775c";

        private List<OrganizationType> _organizationTypeList;
        private Organization _organization = null;
        private List<ProductInternalSetting> _productInternalSettings;

        public WebHookTests()
        {
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

            _productInternalSettings = new List<ProductInternalSetting>() {new ProductInternalSetting() {Name = "TiboWebHookSigningSecret", Value = _mockTiboWebHookSigningSecret}};
        }

        [Fact]
        public void Post_Books_Update_CustomerCompany_BooksMasterId_Success()
        {
            var cacheKey = "productInternalSetting_" + (int)ProductEnum.UnifiedLogin;
            MemoryCache.Default.Remove(cacheKey);

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
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DataImportMappingUpdate, It.IsAny<object>()))
                .Returns(new RepositoryResponse {Id = 1, ErrorMessage = ""});

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(
                mockRepository.Object
            )
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books")
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mocjJson_books_customercompany_deleted);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customercompany_deleted_Signature);

            webHookController.Configuration = new HttpConfiguration();
            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mocjJson_books_customercompany_deleted);

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
            WebHookController webHookController = new WebHookController(
                mockRepository.Object
            )
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books")
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mocjJson_books_customercompany_deleted);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customercompany_deleted_Signature);

            webHookController.Configuration = new HttpConfiguration();
            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mocjJson_books_customercompany_deleted);

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
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DataImportMappingUpdate, It.IsAny<object>()))
                .Returns(new RepositoryResponse {Id = 0, ErrorMessage = "SQL Error happened here"});

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            //Arrange
            WebHookController webHookController = new WebHookController(
                mockRepository.Object
            )
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books")
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mocjJson_books_customercompany_deleted);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customercompany_deleted_Signature);

            webHookController.Configuration = new HttpConfiguration();
            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mocjJson_books_customercompany_deleted);

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
            WebHookController webHookController = new WebHookController(
                mockRepository.Object
            )
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books")
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted_invalidata);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customercompany_deleted_invalidata_Signature);

            webHookController.Configuration = new HttpConfiguration();
            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted_invalidata);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
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
            WebHookController webHookController = new WebHookController(
                mockRepository.Object
            )
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books")
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted_invalidata);
            webHookController.Request.Headers.Add("signature", "12345");

            webHookController.Configuration = new HttpConfiguration();
            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted_invalidata);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);
            
            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"Invalid Signature.\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
        }

        [Fact]
        public void Post_Books_NullInput()
        {
            Mock<IRepository> mockRepository = new Mock<IRepository>();

            //Arrange
            WebHookController webHookController = new WebHookController(
                mockRepository.Object
            )
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books")
            };

            webHookController.Configuration = new HttpConfiguration();
            ThinEvent<JToken> thinEvent = null;

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);
            
            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"Missing Content.\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
        }
    }
}
