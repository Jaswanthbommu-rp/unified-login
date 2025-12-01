using Moq;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Product.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Saml;
using UnifiedLogin.LandingAPI.Test.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace UnifiedLogin.LandingAPI.Test.Logic.Product
{
    [ExcludeFromCodeCoverage]
	public class ManageRumProductTests : ManageProductBaseTests
    {
        #region Private Variables
        private int _blueBookId = 123;
        private string _companyInstanceSourceId = "123456";
        private string _apiEndPoint = "http://localhost";
        private string _clientId = "RumClient";
        private string _clientSecret = "RumClientSecret";

        private GbProductMap _gbProductMap = new GbProductMap();
        private IManageProductRum manageProductRum;
        private ListResponse _listResponse = new ListResponse();
        private IList<CustomerCompanyMap> mapCompany;
        private Mock<IManageBlueBook> mockManageBlueBook;
        private Mock<IManagePersona> mockManagePersona;
        private Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository;
        private Mock<ISamlRepository> mockSamlRepository;
        private Mock<IProductRepository> mockProductRepository;

        #endregion

        #region Constructor
        public ManageRumProductTests() : base((int)ProductEnum.UtilityManagement)
        {
            mockManageBlueBook = new Mock<IManageBlueBook>();
            mockManagePersona = new Mock<IManagePersona>();
            mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            mockSamlRepository = new Mock<ISamlRepository>();
            mockProductRepository = new Mock<IProductRepository>();

            _editorSamlAttributes = new List<SamlAttributes>()
            {
                new SamlAttributes() { Name = "UserId", Value = "1234567" },
                new SamlAttributes() { Name = "PRODUCTUSERNAME", Value = "bob12" }
            };

            _userSamlAttributes = new List<SamlAttributes>()
            {
                new SamlAttributes() { Name = "UserId", Value = "5432" },
                new SamlAttributes() { Name = "PRODUCTUSERNAME", Value = "larry33" }
            };

            _productSettingType.Add(new ProductSettingType() { ProductSettingTypeId = 1, Name = "ProductStatus" });

            _userProductSettings.Add(new ProductSettingList() { ProductId = 1, Name = "IsFavorite", Value = "1", ProductSettingId = 1234 });
            _userProductSettings.Add(new ProductSettingList() { ProductId = 1, Name = "AllProperties", Value = "1", ProductSettingId = 1234 });

            _electronicAddressList = new List<ElectronicAddress>()
            {
                new ElectronicAddress() { AddressType = "Email", AddressString = "test" }
            };

            _productInternalSettings.Add(new ProductInternalSetting() { Name = "ApiEndPoint", Value = _apiEndPoint });
            _productInternalSettings.Add(new ProductInternalSetting() { Name = "TokenUrl", Value = _apiEndPoint });
            _productInternalSettings.Add(new ProductInternalSetting() { Name = "ClientID", Value = _clientId });
            _productInternalSettings.Add(new ProductInternalSetting() { Name = "ApiSecret", Value = _clientSecret });
            _gbProductMap = new GbProductMap() { BooksProductCode = "NWP", Name = "Utility Management", ProductId = 18, UDMSourceCode = "NWP" };
            _repositoryResponseProductStatus.ErrorMessage = "";

            mapCompany = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CustomerCompanyId = _blueBookId,
                    CompanyInstanceSourceId = _companyInstanceSourceId,
                    Source = "NWP"
                }
            };

            mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
				))
                .Returns(mapCompany);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.UtilityManagement)
                ))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.UtilityManagement))))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _editorPersonaId)
                    , It.Is<int>(l => l == (int)ProductEnum.UtilityManagement)
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == _editorPersonaId)
                 ))
                 .Returns(_editorPersona);

            mockSamlRepository
               .Setup(m => m.GetProductSamlDetails(
                   It.Is<long>(l => l == _userPersonaId)
                   , It.IsAny<int>()
                ))
                .Returns(_userSamlAttributes);

            mockManagePersona
              .Setup(m => m.GetPersona(
                  It.Is<long>(l => l == _userPersonaId)
               ))
               .Returns(_userPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductRepository
            .Setup(m => m.GetBooksMasterProductDetail(
                It.IsAny<int>()
            ))
            .Returns(_gbProductMap);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(new List<GbProductMap>() { _gbProductMap });

            var mockTokenHttpMessageHandler = new Mock<HttpMessageHandler>();

            HttpResponseMessage tokenResponse = new HttpResponseMessage(HttpStatusCode.OK);
            tokenResponse.Content = new StringContent(JsonConvert.SerializeObject(new { access_token = "mocked access token" }));
            mockTokenHttpMessageHandler.Setup(HttpMethod.Post, $"{_apiEndPoint}/connect/token", tokenResponse);

        }
        #endregion

        #region XUnit tests
        [Fact]
        public void GetMigrationUsers_GivenEditorIdAndDataFilter_ShouldReturnListOfRumUsers()
        {
            //Arrange
            var dataFilter = new RequestParameter()
            {
                Pages = new PageRequest()
                {
                    StartRow = 1,
                    ResultsPerPage = 1000
                },
                FilterBy = new Dictionary<string, string>() {
                    { "filter" , "NonMigrated" }
                }
            };
            var totalRecords = 5;
            var url = $"{_apiEndPoint}/migration/{_companyInstanceSourceId}/users?filter=NonMigrated&startRow={dataFilter.Pages.StartRow}&resultsPerPage={dataFilter.Pages.ResultsPerPage}";
            var editorPersonaId = _editorPersonaId;
            var actual = new List<MigrationUser>()
            {
                new MigrationUser() { FirstName = "Person", LastName="1", Email = "person1@test.com", Username="person1" },
                new MigrationUser() { FirstName = "Person", LastName="2", Email = "person2@test.com", Username="person2" },
                new MigrationUser() { FirstName = "Person", LastName="3", Email = "person3@test.com", Username="person3" },
                new MigrationUser() { FirstName = "Person", LastName="4", Email = "person4@test.com", Username="person4" },
                new MigrationUser() { FirstName = "Person", LastName="5", Email = "person5@test.com", Username="person5" }
            };
            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK);
            userResponse.Content = new StringContent(JsonConvert.SerializeObject(actual));

            mockHttpMessageHandler.Setup(HttpMethod.Get, url, userResponse);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object) { BaseAddress = new Uri(_apiEndPoint) };

            manageProductRum = new ManageProductRum(_editorRealPageId, _editorUserClaim, mockHttpMessageHandler.Object, mockTokenHttpMessageHandler.Object,
                mockProductInternalSettingRepository.Object, mockManagePersona.Object, mockSamlRepository.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockRepository.Object, httpClient);

            //Act
            var expected = manageProductRum.GetMigrationUsers(editorPersonaId, dataFilter);

            //Assert
            Assert.True(expected.Records.Count == totalRecords);
            Assert.Same(expected.ErrorReason, "");
        }

        [Fact]
        public void UpdateUsersMigrationStatus_GivenEditorIdUserIdAndMigrateUser_ShouldReturnSuccessMessage()
        {
            //Arrange
            var migratedUser = new List<MigrateUser>()
            {
                new MigrateUser() {
                    UserId = "123456",
                    UnifiedLoginUserName = "abc@test.com",
                    UsingUnifiedLogin = true
                }
            };
            var expected = new
            {
                Message = "Success",
                Status = true
            };
            var url = $"{_apiEndPoint}/migration/{_companyInstanceSourceId}/migrate-users";
            var userResponse = new HttpResponseMessage(HttpStatusCode.OK);
            userResponse.Content = new StringContent(JsonConvert.SerializeObject(expected));

            var mockHttpMessageHandler2 = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler2.Setup(HttpMethod.Post, url, userResponse);
            var httpClient = new HttpClient(mockHttpMessageHandler2.Object) { BaseAddress = new Uri(_apiEndPoint) };

            manageProductRum = new ManageProductRum(_editorRealPageId, _editorUserClaim, mockHttpMessageHandler2.Object, mockTokenHttpMessageHandler.Object,
                mockProductInternalSettingRepository.Object, mockManagePersona.Object, mockSamlRepository.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockRepository.Object, httpClient);

            //Act
            var actual = manageProductRum.UpdateUsersMigrationStatus(_editorPersonaId, migratedUser);

            //Assert
            Assert.Same(expected.Message, actual.Message);
            Assert.True(actual.Status);
        }

        #region User-Status

        [Fact]
        public void UpdateRUMActiveUser_Given_ProductUserId_DeleteRUMUser_ShouldReturnTrue()
        {
            //Arrange
            var productUserId = "123";
            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK);
            userResponse.Content = new StringContent("");
            string url = $"{_apiEndPoint}/user/deleteuser?userId={productUserId}";

            mockHttpMessageHandler.Setup(HttpMethod.Delete, url, userResponse);
            var httpClient = new HttpClient(mockHttpMessageHandler.Object) { BaseAddress = new Uri(_apiEndPoint) };

            manageProductRum = new ManageProductRum(_editorRealPageId, _editorUserClaim, mockHttpMessageHandler.Object, mockTokenHttpMessageHandler.Object,
                mockProductInternalSettingRepository.Object, mockManagePersona.Object, mockSamlRepository.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockRepository.Object, httpClient);

            //Act
            var actual = manageProductRum.ChangeUserStatus(_editorPersonaId, productUserId);

            //Assert
            Assert.True(actual);
        }

        [Fact]
        public void UpdateRUMActiveUser_Given_ProductUserId_DeleteRUMUser_Should_Return_False()
        {
            //Arrange
            var productUserId = "123";
            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
            userResponse.Content = new StringContent("error");
            string url = $"{_apiEndPoint}/user/deleteuser?userId={productUserId}";

            mockHttpMessageHandler.Setup(HttpMethod.Delete, url, userResponse);
            var httpClient = new HttpClient(mockHttpMessageHandler.Object) { BaseAddress = new Uri(_apiEndPoint) };

            manageProductRum = new ManageProductRum(_editorRealPageId, _editorUserClaim, mockHttpMessageHandler.Object, mockTokenHttpMessageHandler.Object,
                mockProductInternalSettingRepository.Object, mockManagePersona.Object, mockSamlRepository.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockRepository.Object, httpClient);

            //Act
            var actual = manageProductRum.ChangeUserStatus(_editorPersonaId, productUserId);

            //Assert
            Assert.False(actual);
        }

        #endregion
        #endregion
    }
}
