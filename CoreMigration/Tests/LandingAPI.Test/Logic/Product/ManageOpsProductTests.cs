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
using UnifiedLogin.SharedObjects.Landing.Product.Ops;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.Ops;
using UnifiedLogin.SharedObjects.Saml;
using UnifiedLogin.LandingAPI.Test.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;

namespace UnifiedLogin.LandingAPI.Test.Logic.Product
{
    [ExcludeFromCodeCoverage]
	public class ManageOpsProductTests : ManageProductBaseTests
    {
        #region Private Variables
        private int _blueBookId = 123;
        private string _companyInstanceSourceId = "123456";
        private string _apiEndPoint = "http://localhost";
        private string _apiKey = "some-key";
        private IManageProductOps manageProductOps;
        private ListResponse _listResponse = new ListResponse();
        private IList<CustomerCompanyMap> mapCompany;
        private Mock<IManageBlueBook> mockManageBlueBook;
        private Mock<IManagePersona> mockManagePersona;
        private Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository;
        private Mock<ISamlRepository> mockSamlRepository;
        private Mock<IProductRepository> mockProductRepository;

        #endregion

        #region Constructor
        public ManageOpsProductTests() : base((int)ProductEnum.OpsBuyer)
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
            _productInternalSettings.Add(new ProductInternalSetting() { Name = "ApiKey", Value = Convert.ToBase64String(Encoding.UTF8.GetBytes(_apiKey)) });
            _repositoryResponseProductStatus.ErrorMessage = "";

            mapCompany = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CustomerCompanyId = _blueBookId,
                    CompanyInstanceSourceId = _companyInstanceSourceId,
                    Source = "OPS"
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
                    It.Is<int>(l => l == (int)ProductEnum.OpsBuyer)
                ))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.OpsBuyer))))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _editorPersonaId)
                    , It.Is<int>(l => l == (int)ProductEnum.OpsBuyer)
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
                    It.Is<int>(l => l == (int)ProductEnum.OpsBuyer)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OpsBuyer));

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            HttpResponseMessage tokenResponse = new HttpResponseMessage(HttpStatusCode.OK);
            tokenResponse.Content = new StringContent(JsonConvert.SerializeObject(new { session = new { sid = "mocked sid" } }));
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"{_apiEndPoint}/api/v1.0/sessions", tokenResponse);

            manageProductOps = new ManageProductOps(_editorRealPageId, _editorUserClaim, mockHttpMessageHandler.Object, client,
                mockProductInternalSettingRepository.Object, mockManagePersona.Object, mockSamlRepository.Object, mockManageBlueBook.Object,mockProductRepository.Object, mockRepository.Object);

        }
        #endregion

        #region XUnit tests
        [Fact]
        public void GetMigrationUsers_GivenEditorIdAndDataFilter_ShouldReturnListOfOpsUsers()
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
                    { "include_sub_asset_user" , "1" }
                }
            };
            var totalRecords = 5;
            var url = $"{_apiEndPoint}/api/v1.0/users?page_number={dataFilter.Pages.StartRow}&page_size={dataFilter.Pages.ResultsPerPage}&unify_login_status=inactive&include_sub_asset_user=1";
            var editorPersonaId = _editorPersonaId;
            var userList = new OpsUsers()
            {
                Pagination = new OpsPagination()
                {
                    PageNumber = dataFilter.Pages.StartRow,
                    TotalRecords = totalRecords,
                    PageSize = dataFilter.Pages.ResultsPerPage
                },
                UserList = new List<OpsUser>()
                {
                    new OpsUser() { FirstName = "Person", LastName="1", Email = "person1@test.com", Loginname="person1", AssetGroup = new AssetGroup() { ID = "1" } },
                    new OpsUser() { FirstName = "Person", LastName="2", Email = "person2@test.com", Loginname="person2" },
                    new OpsUser() { FirstName = "Person", LastName="3", Email = "person3@test.com", Loginname="person3" },
                    new OpsUser() { FirstName = "Person", LastName="4", Email = "person4@test.com", Loginname="person4" },
                    new OpsUser() { FirstName = "Person", LastName="5", Email = "person5@test.com", Loginname="person5" }
                }
            };
            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK);
            userResponse.Content = new StringContent(JsonConvert.SerializeObject(userList));

            mockHttpMessageHandler.Setup(HttpMethod.Get, url, userResponse);

            //Act
            var expected = manageProductOps.GetMigrationUsers(editorPersonaId, dataFilter);

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
            var url = $"{_apiEndPoint}/api/v1.0/users";
            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(expected))
            };

            mockHttpMessageHandler.Setup(new HttpMethod("PATCH"), url, userResponse);

            //Act
            var actual = manageProductOps.UpdateUsersMigrationStatus(_editorPersonaId, migrateUsers);

            //Assert
            Assert.Equal(actual.Message, expected.Message);
            Assert.True(actual.Status);
        }
        #endregion

        #region User-Status

        [Fact]
        public void ChangeUserStatus_Given_SystemIdentifier_Enable_ShouldReturn_True()
        {
            var productUserId = "123";
            var userName = "abc@test.com";
            var isDeactivate = true;

            var expected = new OpsUser()
            {
                ID = "123",
                FirstName = "Test",
                MiddleName = "t",
                LastName = "User",
                Loginname = "test.user",
                AssetGroup = new AssetGroup()
                {
                    ID = "123456789"
                },
                Status = "Active",
                UserType = new OpsUserType()
                {
                    Id = "1"
                }
            };
            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK);
            HttpResponseMessage OpsUserResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(expected))
            };

            string url = $"{_apiEndPoint}/api/v1.0/users/{productUserId}";
            string getopsuserUrl = _apiEndPoint + "/api/v1.0/users" + "/" + productUserId;

            mockHttpMessageHandler.Setup(HttpMethod.Get, getopsuserUrl, OpsUserResponse);

            mockHttpMessageHandler.Setup(HttpMethod.Put, url, userResponse);

            ////Act
            var actual = manageProductOps.ChangeUserStatus(_editorPersonaId, userName, productUserId, isDeactivate);
        }

        [Fact]
        public void ChangeUserStatus_Given_SystemIdentifier_Disable_ShouldReturn_True()
        {
            var productUserId = "123";
            var userName = "abc@test.com";
            var isDeactivate = false;

            var expected = new OpsUser()
            {
                ID = "123",
                FirstName = "Test",
                MiddleName = "t",
                LastName = "User",
                Loginname = "test.user",
                Status = "InActive",
                AssetGroup = new AssetGroup()
                {
                      ID = "123456789"
                },
                UserType = new OpsUserType()
                {
                    Id = "1"
                }
            };
            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK);
            HttpResponseMessage OpsUserResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(expected))
            };

            string url = $"{_apiEndPoint}/api/v1.0/users/{productUserId}";
            string getopsuserUrl = _apiEndPoint + "/api/v1.0/users" + "/" + productUserId;

            mockHttpMessageHandler.Setup(HttpMethod.Get, getopsuserUrl, OpsUserResponse);

            mockHttpMessageHandler.Setup(HttpMethod.Put, url, userResponse);

            ////Act
            var actual = manageProductOps.ChangeUserStatus(_editorPersonaId, userName, productUserId, isDeactivate);
        }
        #endregion
    }
}
