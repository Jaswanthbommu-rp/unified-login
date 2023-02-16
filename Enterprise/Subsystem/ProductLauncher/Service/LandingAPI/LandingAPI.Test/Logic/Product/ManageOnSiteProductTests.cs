using System;
using Moq;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Extensions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.Product
{
	[ExcludeFromCodeCoverage]
	public class ManageOnSiteProductTests : ManageProductBaseTests
    {
        #region Private Variables
        private int _blueBookId = 123;
        private string _companyInstanceSourceId = "123456";
        private string _apiEndPoint = "http://localhost";
        private string _tokenUrl = "http://producturl.com/token";
        private string _clientId = "OnSiteClient";
        private string _clientSecret = "OnSiteClientSecret";
        private GbProductMap _gbProductMap = new GbProductMap();
        private IManageProductOnSite manageProductOnSite;
        private ListResponse _listResponse = new ListResponse();
        private IList<CustomerCompanyMap> mapCompany;
        private Mock<IManageBlueBook> mockManageBlueBook;
        private Mock<IManagePersona> mockManagePersona;
        private Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository;
        private Mock<ISamlRepository> mockSamlRepository;
        private Mock<IProductRepository> mockProductRepository;

        #endregion

        #region Constructor
        public ManageOnSiteProductTests() : base((int)ProductEnum.OnSite)
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
            _productInternalSettings.Add(new ProductInternalSetting() { Name = "TokenUrl", Value = _tokenUrl });
            _productInternalSettings.Add(new ProductInternalSetting() { Name = "ClientID", Value = _clientId });
            _productInternalSettings.Add(new ProductInternalSetting() { Name = "ApiSecret", Value = _clientSecret });
            _gbProductMap = new GbProductMap() { BooksProductCode = "ONST", Name = "On-Site", ProductId = 23, UDMSourceCode = "ONST" };
            _repositoryResponseProductStatus.ErrorMessage = "";

            mapCompany = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CustomerCompanyId = _blueBookId,
                    CompanyInstanceSourceId = _companyInstanceSourceId,
                    Source = "ONST"
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
                    It.Is<int>(l => l == (int)ProductEnum.OnSite)
                ))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _editorPersonaId)
                    , It.Is<int>(l => l == (int)ProductEnum.OnSite)
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == _editorPersonaId)
                 ))
                 .Returns(_editorPersona);

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

            HttpResponseMessage tokenResponse = new HttpResponseMessage(HttpStatusCode.OK);
            tokenResponse.Content = new StringContent(JsonConvert.SerializeObject(new { access_token = "mocked access token" }));
            mockHttpMessageHandler.Setup(HttpMethod.Post, _tokenUrl, tokenResponse);

            manageProductOnSite = new ManageProductOnSite(_editorRealPageId, mockHttpMessageHandler.Object,
                mockProductInternalSettingRepository.Object, mockManagePersona.Object, mockSamlRepository.Object, mockManageBlueBook.Object, mockProductRepository.Object);

        }
        #endregion

        #region XUnit tests
        [Fact]
        public void GetMigrationUsers_GivenEditorId_ShouldReturnListOfOnSiteUsers()
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
                    { "filter" , "UnMigrated" }
                }
            };
            var url = $"{_apiEndPoint}/users?company_id={_companyInstanceSourceId}&filter=UnMigrated&page={dataFilter.Pages.StartRow}&per_page={dataFilter.Pages.ResultsPerPage}";
            var editorPersonaId = _editorPersonaId;
            var userList = new List<OnSiteUser>(){
                new OnSiteUser() {
                     OnSiteUserProfile = new OnSiteUserProfile()
                     {
                          FirstName = "Person",
                          LastName = "1",
                          UserName = "person1",
                          UserId = "1",
                          IsActive = true,
                          Email = "person1@test.com",
                          Properties = new PropertyAcsess() { PropertyIdList = new List<int>() { 1, 2, 3} },
                          Roles = new List<OnSiteRole>()
                     }
                },
                new OnSiteUser() {
                     OnSiteUserProfile = new OnSiteUserProfile()
                     {
                          FirstName = "Person",
                          LastName = "2",
                          UserName = "person2",
                          UserId = "2",
                          IsActive = true,
                          Email = "person2@test.com",
                          Properties = new PropertyAcsess(),
                          Roles = new List<OnSiteRole>()
                     }
                },
                new OnSiteUser() {
                     OnSiteUserProfile = new OnSiteUserProfile()
                     {
                          FirstName = "Person",
                          LastName = "3",
                          UserName = "person3",
                          UserId = "3",
                          IsActive = true,
                          Email = "person3@test.com",
                          Properties = new PropertyAcsess(),
                          Roles = new List<OnSiteRole>()
                     }
                },
                new OnSiteUser() {
                     OnSiteUserProfile = new OnSiteUserProfile()
                     {
                          FirstName = "Person",
                          LastName = "4",
                          UserName = "person4",
                          UserId = "4",
                          IsActive = true,
                          Email = "person4@test.com",
                          Properties = new PropertyAcsess(),
                          Roles = new List<OnSiteRole>()
                     }
                },
                new OnSiteUser() {
                     OnSiteUserProfile = new OnSiteUserProfile()
                     {
                          FirstName = "Person",
                          LastName = "5",
                          UserName = "person5",
                          UserId = "5",
                          IsActive = true,
                          Email = "person5@test.com",
                          Properties = new PropertyAcsess(),
                          Roles = new List<OnSiteRole>()
                     }
                }
            };
            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK);
            userResponse.Content = new StringContent(JsonConvert.SerializeObject(userList));

            mockHttpMessageHandler.Setup(HttpMethod.Get, url, userResponse);

            //Act
            var expected = manageProductOnSite.GetMigrationUsers(editorPersonaId, dataFilter);

            //Assert
            Assert.IsType<MigrationUser>(expected.Records[0]);
            Assert.True(expected.Records.Count == userList.Count);
            Assert.Same(expected.ErrorReason, "");
        }

        [Fact]
        public void UpdateUsersMigrationStatus_GivenEditorIdUserIdAndMigrateUser_ShouldMigrateAndReturnSuccessMessage()
        {
            //Arrange
            var migrateUsers = new List<MigrateUser>()
            {
                new MigrateUser(){
                    UserId = "123456",
                    UnifiedLoginUserName = "abc@test.com",
                    UsingUnifiedLogin = true
                },
                new MigrateUser(){
                    UserId = "123457",
                    UnifiedLoginUserName = "abc@test.com",
                    UsingUnifiedLogin = true
                }
            };
            var expected = new MigrateResponse
            {
                Message = "2",
                Status = true
            };
            var migrateUrl = $"{_apiEndPoint}/users/migrate_users";
            HttpResponseMessage migrateResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new { count = 2 }))
            };

            mockHttpMessageHandler.Setup(HttpMethod.Post, migrateUrl, migrateResponse);

            //Act
            var actual = manageProductOnSite.UpdateUsersMigrationStatus(_editorPersonaId, migrateUsers);

            //Assert
            Assert.Equal(actual.Message, expected.Message);
            Assert.True(actual.Status);
        }

        [Fact]
        public void UpdateUsersMigrationStatus_GivenEditorIdUserIdAndMigrateUser_ShouldUnMigrateAndReturnSuccessMessage()
        {
            //Arrange
            var migrateUsers = new List<MigrateUser>()
            {
                new MigrateUser(){
                    UserId = "123456",
                    UnifiedLoginUserName = "abc@test.com",
                    UsingUnifiedLogin = false
                },
                new MigrateUser(){
                    UserId = "123457",
                    UnifiedLoginUserName = "abc@test.com",
                    UsingUnifiedLogin = false
                }
            };
            var expected = new MigrateResponse
            {
                Message = " 2",
                Status = true
            };
            var unmigrateUrl = $"{_apiEndPoint}/users/unmigrate_users";
            HttpResponseMessage unmigrateResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new { count = 2 }))
            };

            mockHttpMessageHandler.Setup(HttpMethod.Post, unmigrateUrl, unmigrateResponse);

            //Act
            var actual = manageProductOnSite.UpdateUsersMigrationStatus(_editorPersonaId, migrateUsers);

            //Assert
            Assert.Equal(actual.Message, expected.Message);
            Assert.True(actual.Status);
        }

        [Fact]
        public void UpdateUsersMigrationStatus_GivenEditorIdUserIdAndMigrateUser_ShouldMigrateUnMigrateAndReturnSuccessMessage()
        {
            //Arrange
            var migrateUsers = new List<MigrateUser>()
            {
                new MigrateUser(){
                    UserId = "123456",
                    UnifiedLoginUserName = "abc@test.com",
                    UsingUnifiedLogin = true
                },
                new MigrateUser(){
                    UserId = "123457",
                    UnifiedLoginUserName = "abc@test.com",
                    UsingUnifiedLogin = false
                }
            };
            var expected = new MigrateResponse
            {
                Message = "1 1",
                Status = true
            };
            var migrateUrl = $"{_apiEndPoint}/users/migrate_users";
            var unmigrateUrl = $"{_apiEndPoint}/users/unmigrate_users";
            HttpResponseMessage migrateResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new { count = 1 }))
            };
            HttpResponseMessage unmigrateResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new { count = 1 }))
            };

            mockHttpMessageHandler.Setup(HttpMethod.Post, migrateUrl, migrateResponse);
            mockHttpMessageHandler.Setup(HttpMethod.Post, unmigrateUrl, unmigrateResponse);

            //Act
            var actual = manageProductOnSite.UpdateUsersMigrationStatus(_editorPersonaId, migrateUsers);

            //Assert
            Assert.Equal(actual.Message, expected.Message);
            Assert.True(actual.Status);
        }

        #region User-Status
        [Fact]
        public void ChangeUserStatus_Given_UserName_Disable_ShouldReturn_True()
        {
            //Arrange
            var productUserId = "123";
            var isDeactivate = true;

            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK);
            string url = $"{_apiEndPoint}/users/{productUserId}/deactivate?company_id={_companyInstanceSourceId}";

            mockHttpMessageHandler.Setup(HttpMethod.Post, url, userResponse);

            ////Act
            var actual = manageProductOnSite.ChangeUserStatus(_editorPersonaId, productUserId, isDeactivate);

            ////Assert
            Assert.True(actual);
        }

        [Fact]
        public void ChangeUserStatus_Given_UserName_Disable_Should_Return_False()
        {
            //Arrange
            var productUserId = "123";
            var isDeactivate = true;

            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
            userResponse.Content = new StringContent("error");
            string url = $"{_apiEndPoint}/users/{productUserId}/deactivate?company_id={_companyInstanceSourceId}";

            mockHttpMessageHandler.Setup(HttpMethod.Post, url, userResponse);
            ////Act
            var actual = manageProductOnSite.ChangeUserStatus(_editorPersonaId, productUserId, isDeactivate);

            ////Assert
            Assert.False(actual);
        }
        #endregion
        #endregion
    }
}
