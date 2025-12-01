using Moq;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Saml;
using UnifiedLogin.LandingAPI.Test.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace UnifiedLogin.LandingAPI.Test.Logic.Product
{
    [ExcludeFromCodeCoverage]
    public class ManageMarketingCenterProductTests : ManageProductBaseTests
    {
        #region Private Variables
        private int _blueBookId = 123;
        private string _companyInstanceSourceId = "123456";
        private string _apiEndPoint = "http://localhost";
        private string _username = "MCUser";
        private string _password = "MCPassword";
		private string _marketingCenterApiSourceID = "261";

        private IList<CustomerCompanyMap> mapCompany;
        private Mock<IManageBlueBook> mockManageBlueBook;
        private Mock<IManagePersona> mockManagePersona;
        private Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository;
        private Mock<ISamlRepository> mockSamlRepository;
        private Mock<IProductRepository> mockProductRepository;
        #endregion

        #region Constructor
        public ManageMarketingCenterProductTests() : base((int)ProductEnum.MarketingCenter)
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

            _productSettingType.Add(new ProductSettingType() { ProductSettingTypeId = 1, Name = "ProductStatus" });

            _userProductSettings.Add(new ProductSettingList() { ProductId = 1, Name = "IsFavorite", Value = "1", ProductSettingId = 1234 });
            _userProductSettings.Add(new ProductSettingList() { ProductId = 1, Name = "AllProperties", Value = "1", ProductSettingId = 1234 });

            _electronicAddressList = new List<ElectronicAddress>()
            {
                new ElectronicAddress() { AddressType = "Email", AddressString = "test" }
            };

            _productInternalSettings.Add(new ProductInternalSetting() { Name = "ApiEndPoint", Value = _apiEndPoint });
            _productInternalSettings.Add(new ProductInternalSetting() { Name = "ApiUserName", Value = Encode(_username) });
            _productInternalSettings.Add(new ProductInternalSetting() { Name = "ApiPassword", Value = Encode(_password) });
			_productInternalSettings.Add(new ProductInternalSetting() { Name = "MarketingCenterApiSourceID", Value = _marketingCenterApiSourceID });
            _repositoryResponseProductStatus.ErrorMessage = "";

            mapCompany = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CustomerCompanyId = _blueBookId,
                    CompanyInstanceSourceId = _companyInstanceSourceId,
                    Source = ProductEnum.MarketingCenter.ToEnumDescription()
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
                    It.Is<int>(l => l == (int)ProductEnum.MarketingCenter)
                ))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.MarketingCenter))))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _editorPersonaId)
                    , It.Is<int>(l => l == (int)ProductEnum.MarketingCenter)
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
                    It.IsAny<int>()))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.MarketingCenter));

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

        }
        #endregion

        #region XUnit tests
        [Fact]
        public void GetMigrationUsers_GivenEditorIdAndDataFilter_ShouldReturnListOfMarketingCenterUsers()
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
            var url = $"{_apiEndPoint}/external/api/{_companyInstanceSourceId}/users?filter-type=NonMigrated&startRow={dataFilter.Pages.StartRow}&resultsperpage={dataFilter.Pages.ResultsPerPage}";
            var editorPersonaId = _editorPersonaId;
            var actual = new MigrationResponse<List<MigrationUser>>()
            {
                Data = new List<MigrationUser>()
                {
                    new MigrationUser() { FirstName = "Person", LastName = "1", Email = "person1@test.com", Username = "person1" },
                    new MigrationUser() { FirstName = "Person", LastName = "2", Email = "person2@test.com", Username = "person2", Properties = new List<MigrationProperty>(){
                    new MigrationProperty() { PropertyInstanceSourceId = "1" }, new MigrationProperty() { PropertyInstanceSourceId = "2" }
                } },
                    new MigrationUser() { FirstName = "Person", LastName = "3", Email = "person3@test.com", Username = "person3" },
                    new MigrationUser() { FirstName = "Person", LastName = "4", Email = "person4@test.com", Username = "person4" },
                    new MigrationUser() { FirstName = "Person", LastName = "5", Email = "person5@test.com", Username = "person5" }
                }
            };
            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(actual))
            };

            mockHttpMessageHandler.Setup(HttpMethod.Get, url, userResponse);

            var manageProductMarketingCenter = new ManageProductMarketingCenter(_editorRealPageId, _editorUserClaim, mockHttpMessageHandler.Object, mockProductInternalSettingRepository.Object,
                mockManagePersona.Object, mockSamlRepository.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockRepository.Object);

            //Act
            var expected = manageProductMarketingCenter.GetMigrationUsers(editorPersonaId, dataFilter);

            //Assert
            Assert.IsType<MigrationUser>(expected.Records[0]);
            Assert.True(expected.Records.Count == totalRecords);
            Assert.Same(expected.ErrorReason, "");
        }

        [Fact]
        public void UpdateUsersMigrationStatus_GivenEditorIdUserIdAndMigrateUser_ShouldReturnSuccessMessage()
        {
			Mock<IPersonaRepository> _mockPersonaRepository = new Mock<IPersonaRepository>();
			string ContactMechanismUsageTypeName = "";
			Type type = typeof(ElectronicAddress);
			IElectronicAddress electronicAddress = new ElectronicAddress();
			IList<IElectronicAddress> expectedElectronicAddressList = new List<IElectronicAddress>();
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			
			long personaId = 33;

			_mockPersonaRepository
				.Setup(m => m.GetActivePersonaId(realPageId))
				.Returns(personaId);

			var mockObject = new Mock<IElectronicAddressRepository>();
			mockObject
				.Setup(m => m.ListElectronicAddressForPerson("test@test.com", personaId, ContactMechanismUsageTypeName))
				.Returns(() => new List<ElectronicAddress> { new ElectronicAddress() { } });

			//Act
			expectedElectronicAddressList.Add(electronicAddress);
			IManageElectronicAddress manageElectronicAddress = new ManageElectronicAddress(mockObject.Object);
			IList<ElectronicAddress> electronicAddressList = manageElectronicAddress.ListElectronicAddressForPerson("test@test.com", 1, ContactMechanismUsageTypeName);

			//Arrange
			var migrateUsers = new List<MigrateUser>()
            {
                new MigrateUser(){
                    UserId = "123456",
                    UnifiedLoginUserName = "abc@test.com",
                    UsingUnifiedLogin = true,
					LeadEmailAddress = "abc@test.com"
                },
                new MigrateUser(){
                    UserId = "123457",
                    UnifiedLoginUserName = "abc@test.com",
                    UsingUnifiedLogin = true,
					LeadEmailAddress = "abc@test.com"
				}
            };
            var expected = new MigrateResponse()
            {
                Message = "Success",
                Status = true
            };
            var url = $"{_apiEndPoint}/external/api/{_companyInstanceSourceId}/migrate-users";
            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new MigrationResponse<MigrateResponse>() { Data = expected }))
            };

            mockHttpMessageHandler.Setup(HttpMethod.Post, url, userResponse);

            var manageProductMarketingCenter = new ManageProductMarketingCenter(_editorRealPageId, _editorUserClaim, mockHttpMessageHandler.Object, mockProductInternalSettingRepository.Object,
                mockManagePersona.Object, mockSamlRepository.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockRepository.Object);

            //Act
            var actual = manageProductMarketingCenter.UpdateUsersMigrationStatus(_editorPersonaId, migrateUsers);

            //Assert
            Assert.Equal(actual.Message, expected.Message);
            Assert.True(actual.Status);
        }

        [Fact]
        public void ChangeUserStatus_Given_UserName_Disable_ShouldReturn_False()
        {
            //Arrange
            var productUserId = "123";
            var userName = "abc@test.com";
            var isDeactivate = true;

            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
            string url = $"{_apiEndPoint}/external/contact/{productUserId}/status";

            mockHttpMessageHandler.Setup(HttpMethod.Put, url, userResponse);

            var manageProductMarketingCenter = new ManageProductMarketingCenter(_editorRealPageId, _editorUserClaim, mockHttpMessageHandler.Object, mockProductInternalSettingRepository.Object,
                mockManagePersona.Object, mockSamlRepository.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockRepository.Object);

            //Act
            var actual = manageProductMarketingCenter.ChangeUserStatus(_editorPersonaId, userName, productUserId, isDeactivate);

            //Assert
            Assert.False(actual);
        }

        [Fact]
        public void ChangeUserStatus_Given_UserName_Disable_ShouldReturn_True()
        {
            //Arrange
            var productUserId = "123";
            var userName = "abc@test.com";
            var isDeactivate = true;

            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK);
            userResponse.Content = new StringContent("{}");
            string url = $"{_apiEndPoint}/external/contact/{productUserId}/status";

            mockHttpMessageHandler.Setup(HttpMethod.Put, url, userResponse);

            var manageProductMarketingCenter = new ManageProductMarketingCenter(_editorRealPageId, _editorUserClaim, mockHttpMessageHandler.Object, mockProductInternalSettingRepository.Object,
                mockManagePersona.Object, mockSamlRepository.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockRepository.Object);

            //Act
            var actual = manageProductMarketingCenter.ChangeUserStatus(_editorPersonaId, userName, productUserId, isDeactivate);

            //Assert
                Assert.True(actual);
        }
        #endregion
    }
}
