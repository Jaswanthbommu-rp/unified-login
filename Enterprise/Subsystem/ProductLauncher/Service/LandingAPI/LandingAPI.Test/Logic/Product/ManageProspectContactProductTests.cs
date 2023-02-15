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
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.Product
{
    [ExcludeFromCodeCoverage]
    public class ManageProspectContactProductTests : ManageProductBaseTests
    {
        #region Private Variables
        private int _blueBookId = 123;
        private string _companyInstanceSourceId = "123456";
        private string _apiEndPoint = "http://producturl.com";
        private GbProductMap _gbProductMap = new GbProductMap();
        private IManageProductProspectContact manageProductProspectContact;
        private IList<CustomerCompanyMap> mapCompany;
        private Mock<IManageBlueBook> mockManageBlueBook;
        private Mock<IManagePersona> mockManagePersona;
        private Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository;
        private Mock<ISamlRepository> mockSamlRepository;
        private Mock<IProductRepository> mockProductRepository;

        #endregion

        #region Constructor
        public ManageProspectContactProductTests() : base((int)ProductEnum.ProspectContactCenter)
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
            _gbProductMap = new GbProductMap() { BooksProductCode = "LVL1", Name = "Prospect Contact Center", ProductId = 10, UDMSourceCode = "LVL1" };
            _repositoryResponseProductStatus.ErrorMessage = "";

            mapCompany = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CustomerCompanyId = _blueBookId,
                    CompanyInstanceSourceId = _companyInstanceSourceId,
                    Source = ProductEnum.ProspectContactCenter.ToEnumDescription()
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
                    It.Is<int>(l => l == (int)ProductEnum.ProspectContactCenter)
                ))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _editorPersonaId)
                    , It.Is<int>(l => l == (int)ProductEnum.ProspectContactCenter)
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

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(new List<GbProductMap>() { _gbProductMap });

            manageProductProspectContact = new ManageProductProspectContact(_editorRealPageId, _editorUserClaim, mockHttpMessageHandler.Object, mockProductInternalSettingRepository.Object,
                mockManagePersona.Object, mockSamlRepository.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockRepository.Object);

        }
        #endregion

        #region XUnit tests
        [Fact]
        public void GetMigrationUsers_GivenEditorIdAndDataFilter_ShouldReturnListOfProspectContactCenterUsers()
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
            var url = $"{_apiEndPoint}/users/{_companyInstanceSourceId}?filter=NonMigrated&startRow={dataFilter.Pages.StartRow}&resultsPerPage={dataFilter.Pages.ResultsPerPage}";
            var editorPersonaId = _editorPersonaId;
            var actual = new List<ProspectContactCenterUserProfile>()
            {
               new ProspectContactCenterUserProfile() { FirstName = "Person", LastName = "1", Email = "person1@test.com", LoginName = "person1" },
               new ProspectContactCenterUserProfile() { FirstName = "Person", LastName = "2", Email = "person2@test.com", LoginName = "person2" },
                    new ProspectContactCenterUserProfile() { FirstName = "Person", LastName = "3", Email = "person3@test.com", LoginName = "person3" },
                    new ProspectContactCenterUserProfile() { FirstName = "Person", LastName = "4", Email = "person4@test.com", LoginName = "person4", Properties = new List<string>(){"1"}},
                    new ProspectContactCenterUserProfile() { FirstName = "Person", LastName = "5", Email = "person5@test.com", LoginName = "person5" }
            };
            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(actual))
            };

            mockHttpMessageHandler.Setup(HttpMethod.Get, url, userResponse);

            //Act
            var expected = manageProductProspectContact.GetMigrationUsers(editorPersonaId, dataFilter);

            //Assert
            Assert.IsType<MigrationUser>(expected.Records[0]);
            Assert.True(expected.Records.Count == totalRecords);
            Assert.Same(expected.ErrorReason, "");
        }

        [Fact]
        public void UpdateUsersMigrationStatus_GivenEditorIdUserIdAndMigrateUser_ShouldReturnSuccessMessage()
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
            var expected = new MigrateResponse()
            {
                Message = "Success",
                Status = true
            };
            var url = $"{_apiEndPoint}/migrate-users/{_companyInstanceSourceId}";
            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(expected))
            };

            mockHttpMessageHandler.Setup(HttpMethod.Put, url, userResponse);

            //Act
            var actual = manageProductProspectContact.UpdateUsersMigrationStatus(_editorPersonaId, migrateUsers);

            //Assert
            Assert.Equal(actual.Message, expected.Message);
            Assert.True(actual.Status);
        }
        #endregion
    }
}
