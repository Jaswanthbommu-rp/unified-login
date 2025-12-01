using System;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Product.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.RentersInsurance;
using UnifiedLogin.SharedObjects.Saml;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using Xunit;
using IC = UnifiedLogin.SharedObjects.IdentityConfig;
using System.Net.Http;
using JsonApiSerializer;
using Newtonsoft.Json;
using UnifiedLogin.LandingAPI.Test.Extensions;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
    /// <summary>
    /// Renters Insurance Product xUnit tests
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageRentersInsuranceProductTests : ManageProductBaseTests
    {
        #region Private Variables

        private int _blueBookId;

        private List<UserProperty> _PropertyList = new List<UserProperty>();
        private IList<ProductRole> _RoleList = new List<ProductRole>();

        private ListResponse _listResponse = new ListResponse();
        private GbProductMap _gbProductMap = new GbProductMap();
        private string testHostname = "http://localhost";

        #endregion

        #region Constructor

        public ManageRentersInsuranceProductTests() : base((int)ProductEnum.ResidentPortal)
        {
            _blueBookId = 236;

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

            _electronicAddressList = new List<IC.ElectronicAddress>()
            {
                new IC.ElectronicAddress() { AddressType = "Email", AddressString = "test" }
            };

            _productInternalSettings.Add(new IC.ProductInternalSetting() { Name = "ApiEndPoint", Value = testHostname });
            _gbProductMap = new GbProductMap() { BooksProductCode = "LD", Name = "Renters Insurance", ProductId = 15, UDMSourceCode = "LD" };
            _repositoryResponseProductStatus.ErrorMessage = "";

            mockRepository
                .Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.ResidentPortal))))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(new List<GbProductMap>() { _gbProductMap });
        }

        #endregion

        #region XUnit tests

        [Fact]
        public void ListProperties_MockInputData_ReturnValidRepositoryResponseObject()
        {
            //Arrange
            Mock<IInsuranceService> mockRentersInsuranceService = new Mock<IInsuranceService>();
            Mock<IManageBlueBook> mockManageBlueBook = new Mock<IManageBlueBook>();
            Mock<IManagePersona> mockManagePersona = new Mock<IManagePersona>();
            Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();
            Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            Mock<ISamlRepository> mockSamlRepository = new Mock<ISamlRepository>();

            CompanyPropertyRootObject companyPropertyRootObject = new CompanyPropertyRootObject();
            Data data = new Data();
            Attributes attributes = new Attributes();
            ListPropertyByPMCIDResponse listPropertyByPMCIDResponse = new ListPropertyByPMCIDResponse();
            UserInfo userInfo = new UserInfo();
            IList<UserProperty> userPropertyList = new List<UserProperty>()
            {
                new UserProperty()
                {
                    PropertyID = 8528,
                    PropertyName = "Crossings at Danada"
                },
                new UserProperty()
                {
                    PropertyID = 16858,
                    PropertyName = "Desoto Town Center"
                },
                new UserProperty()
                {
                    PropertyID = 268948,
                    PropertyName = "Lincoln On University"
                },
                new UserProperty()
                {
                    PropertyID = 269285,
                    PropertyName = "Palencia Apartments"
                },
                new UserProperty()
                {
                    PropertyID = 418145,
                    PropertyName = "T C The Edge At Flagler Village llc"
                },
                new UserProperty()
                {
                    PropertyID = 4249,
                    PropertyName = "The Venue at Greenville"
                },
                new UserProperty()
                {
                    PropertyID = 4611,
                    PropertyName = "Trails of Valley Ranch"
                }
            };
            listPropertyByPMCIDResponse.PropertyList = userPropertyList.ToArray();

            List<ProductProperty> expectedProductPropertyList = new List<ProductProperty>()
            {
                new ProductProperty()
                {
                    ID = "8528",
                    Name = "Crossings at Danada",
                    IsAssigned = false
                },
                new ProductProperty()
                {
                    ID = "16858",
                    Name = "Desoto Town Center",
                    IsAssigned = false
                },
                new ProductProperty()
                {
                    ID = "268948",
                    Name = "Lincoln On University",
                    IsAssigned = false
                },
                new ProductProperty()
                {
                    ID = "269285",
                    Name = "Palencia Apartments",
                    IsAssigned = false
                },
                new ProductProperty()
                {
                    ID = "418145",
                    Name = "T C The Edge At Flagler Village llc",
                    IsAssigned = false
                },
                new ProductProperty()
                {
                    ID = "4249",
                    Name = "The Venue at Greenville",
                    IsAssigned = false
                },
                new ProductProperty()
                {
                    ID = "4611",
                    Name = "Trails of Valley Ranch",
                    IsAssigned = false
                }
            };

            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CompanyInstanceSourceId = _blueBookId.ToString(),
                    Source = "LD"
                }
            };

            _editorPersona.Organization.BooksCustomerMasterId = _blueBookId;

            mockRentersInsuranceService
                .Setup(m => m.GetListPropertyByPMCID(758241))
                .Returns(listPropertyByPMCIDResponse);

            mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.Is<string>(l => l == "LD"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
                ))
                .Returns(mapResource);

            List<GetCompanyPropertyInstance> propertyInstanceResourceList = new List<GetCompanyPropertyInstance>()
            {
                new GetCompanyPropertyInstance()
                {
                    propertyInstanceSourceId = "418145",
                    propertyName = "T C The Edge At Flagler Village llc",
                    state = "FL",
                    isActive = true
                },
                new GetCompanyPropertyInstance()
                {
                    propertyInstanceSourceId = "269285",
                    propertyName = "Palencia Apartments",
                    state = "TX",
                    isActive = true
                },
                new GetCompanyPropertyInstance()
                {
                    propertyInstanceSourceId = "4611",
                    propertyName = "Trails of Valley Ranch",
                    state = "TX",
                    isActive = true
                },
                new GetCompanyPropertyInstance()
                {
                    propertyInstanceSourceId = "4249",
                    propertyName = "TheVenue at Greenville",
                    state = "TX",
                    isActive = true
                },
                new GetCompanyPropertyInstance()
                {
                    propertyInstanceSourceId = "16858",
                    propertyName = "Desoto Town Center",
                    state = "TX",
                    isActive = true
                },
                new GetCompanyPropertyInstance()
                {
                    propertyInstanceSourceId = "8528",
                    propertyName = "Crossings at Danada",
                    state = "TX",
                    isActive = true
                },
                new GetCompanyPropertyInstance()
                {
                    propertyInstanceSourceId = "268948",
                    propertyName = "Lincoln On University",
                    state = "TX",
                    isActive = true
                }
            };

            attributes.getCompanyPropertyInstances = propertyInstanceResourceList;
            data.attributes = attributes;
            data.type = "DashBoard";
            companyPropertyRootObject.data = data;

            mockManageBlueBook
                .Setup(m => m.GetCompanyPropertyInstance(
                    It.IsAny<long>()
                ))
                .Returns(companyPropertyRootObject);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.Insurance)
                ))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.Insurance)
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.Insurance)
                ))
                .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
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

            //Act
            IManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(
                editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim,
                messageHandler: mockHttpMessageHandler.Object,
                companyInstanceId: 758241,
                rentersInsuraceService: mockRentersInsuranceService.Object,
                listPropertyByPMCIDResponse: listPropertyByPMCIDResponse,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: null,
                manageUserLogin: null,
                managePartyRelationship: null,
                repository: mockRepository.Object
            );

            //Assert
            _listResponse = manageProductRentersInsurance.ListProperties(_editorPersonaId, _userPersonaId, null);
            IList<ProductProperty> productPropertyList = _listResponse.Records.Cast<ProductProperty>().ToList();
            List<ProductProperty> compareResult = productPropertyList.Where(item => expectedProductPropertyList.Select(eItem => eItem.ID).Contains(item.ID)).ToList();
            Assert.True(_listResponse.Records.Count == expectedProductPropertyList.Count);
            Assert.True(compareResult.Count == expectedProductPropertyList.Count);
        }

        [Fact]
        public void GetMigrationUsers_GivenEditorIdAndDataFilter_ShouldReturnListOfRenterInsuranceUser()
        {
            //Arrange
            Mock<IInsuranceService> mockRentersInsuranceService = new Mock<IInsuranceService>();
            Mock<IManageBlueBook> mockManageBlueBook = new Mock<IManageBlueBook>();
            Mock<IManagePersona> mockManagePersona = new Mock<IManagePersona>();
            Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();
            Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            Mock<ISamlRepository> mockSamlRepository = new Mock<ISamlRepository>();

            IList<CustomerCompanyMap> companyMapList = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CompanyInstanceSourceId = "1",
                    Source = BlueBookProductConstants.Insurance
                }
            };
            var listOfUsers = new ListOfUserResponse()
            {
                UserList = new UserInfo[]
                {
                    new UserInfo() { FirstName = "Person", LastName = "1", Email = "person1@test.com", User = "person1" },
                    new UserInfo()
                    {
                        FirstName = "Person", LastName = "2", Email = "person2@test.com", User = "person2", PropertyList = new UserProperty[]
                        {
                            new UserProperty() { PropertyID = 1 }
                        }
                    },
                    new UserInfo() { FirstName = "Person", LastName = "3", Email = "person3@test.com", User = "person3" },
                    new UserInfo() { FirstName = "Person", LastName = "4", Email = "person4@test.com", User = "person4" },
                    new UserInfo() { FirstName = "Person", LastName = "5", Email = "person5@test.com", User = "person5" }
                }
            };

            _editorPersona.Organization.BooksCustomerMasterId = _blueBookId;

            mockRentersInsuranceService
                .Setup(m => m.GetUsersByPMC(It.IsAny<UserActionByPMCIDRequest>()))
                .Returns(listOfUsers);

            mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.Is<string>(l => l == "LD"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
                ))
                .Returns(companyMapList);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.Insurance)
                ))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.Insurance)
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.Insurance)
                ))
                .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
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

            var dataFilter = new RequestParameter()
            {
                Pages = new PageRequest()
                {
                    StartRow = 1,
                    ResultsPerPage = 1000
                },
                FilterBy = new Dictionary<string, string>()
                {
                    { "filter", "NonMigrated" }
                }
            };

            //Act
            IManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(
                editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim,
                messageHandler: mockHttpMessageHandler.Object,
                rentersInsuraceService: mockRentersInsuranceService.Object,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: null,
                manageUserLogin: null,
                managePartyRelationship: null,
                repository: mockRepository.Object);

            //Assert
            var expected = manageProductRentersInsurance.GetMigrationUsers(_editorPersonaId, dataFilter);
            //Assert
            Assert.IsType<MigrationUser>(expected.Records[0]);
            Assert.True(expected.Records.Count == listOfUsers.UserList.Count());
            Assert.Same(expected.ErrorReason, "");
        }

        [Fact]
        public void UpdateUsersMigrationStatus_GivenEditorIdUserIdAndMigrateUser_ShouldReturnSuccessMessage()
        {
            //Arrange
            Mock<IInsuranceService> mockRentersInsuranceService = new Mock<IInsuranceService>();
            Mock<IManageBlueBook> mockManageBlueBook = new Mock<IManageBlueBook>();
            Mock<IManagePersona> mockManagePersona = new Mock<IManagePersona>();
            Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();
            Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            Mock<ISamlRepository> mockSamlRepository = new Mock<ISamlRepository>();

            IList<CustomerCompanyMap> companyMapList = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CompanyInstanceSourceId = "1",
                    Source = BlueBookProductConstants.Insurance
                }
            };
            var listOfUsers = new ListOfUserResponse()
            {
                UserList = new UserInfo[]
                {
                    new UserInfo() { FirstName = "Person", LastName = "1", Email = "person1@test.com", User = "person1" },
                    new UserInfo()
                    {
                        FirstName = "Person", LastName = "2", Email = "person2@test.com", User = "person2", PropertyList = new UserProperty[]
                        {
                            new UserProperty() { PropertyID = 1 }
                        }
                    },
                    new UserInfo() { FirstName = "Person", LastName = "3", Email = "person3@test.com", User = "person3" },
                    new UserInfo() { FirstName = "Person", LastName = "4", Email = "person4@test.com", User = "person4" },
                    new UserInfo() { FirstName = "Person", LastName = "5", Email = "person5@test.com", User = "person5" }
                }
            };

            _editorPersona.Organization.BooksCustomerMasterId = _blueBookId;

            mockRentersInsuranceService
                .Setup(m => m.MigrateUser(It.IsAny<MigrateUserrequest[]>()))
                .Returns("Success");

            mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.Is<string>(l => l == "LD"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
                ))
                .Returns(companyMapList);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.Insurance)
                ))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.ResidentPortal))))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.Insurance)
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.Insurance)
                ))
                .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                ))
                .Returns(_userPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            var migrateUsers = new List<MigrateUser>()
            {
                new MigrateUser()
                {
                    UserId = "123456",
                    UnifiedLoginUserName = "abc@test.com",
                    UsingUnifiedLogin = true
                },
                new MigrateUser()
                {
                    UserId = "123457",
                    UnifiedLoginUserName = "abc@test.com",
                    UsingUnifiedLogin = true
                }
            };

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.IsAny<int>()
                ))
                .Returns(_gbProductMap);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(new List<GbProductMap>() { _gbProductMap });

            //companyMapList
            var responseCompanyMapList = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(companyMapList, new JsonApiSerializerSettings());
            responseCompanyMapList.Content = new StringContent(jsonToSave);

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/=&fields[companyinstance]=companyInstanceId,source,companyInstanceSourceId,companyName,companyType,isActive,domain", responseCompanyMapList);

            //Act
            IManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(
                editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim,
                messageHandler: mockHttpMessageHandler.Object,
                rentersInsuraceService: mockRentersInsuranceService.Object,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: null,
                manageUserLogin: null,
                managePartyRelationship: null,
                repository: mockRepository.Object);

            //Act
            var actual = manageProductRentersInsurance.UpdateUsersMigrationStatus(_editorPersonaId, migrateUsers);

            //Assert
            Assert.Equal("Success", actual.Message);
            Assert.True(actual.Status);
        }

        #region User-Status

        [Fact]
        public void ChangeUserStatus_Given_UserId_Enable_ShouldReturn_True()
        {
            //Arrange
            Mock<IInsuranceService> mockRentersInsuranceService = new Mock<IInsuranceService>();
            Mock<IManageBlueBook> mockManageBlueBook = new Mock<IManageBlueBook>();
            Mock<IManagePersona> mockManagePersona = new Mock<IManagePersona>();
            Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();
            Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            Mock<ISamlRepository> mockSamlRepository = new Mock<ISamlRepository>();
            UserAPIResponse userAPIResponse = new UserAPIResponse()
            {
                IsSuccess = true,
                UserId = 1
            };

            int userId = 1;
            bool isActive = true;

            mockRentersInsuranceService
                .Setup(m => m.EnableUser(It.IsAny<UserActionRequest>()))
                .Returns(userAPIResponse);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.Insurance)
                ))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.Insurance)
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.Insurance)
                ))
                .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
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

            IManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(
                editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim,
                messageHandler: mockHttpMessageHandler.Object,
                rentersInsuraceService: mockRentersInsuranceService.Object,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: null,
                manageUserLogin: null,
                managePartyRelationship: null,
                repository: mockRepository.Object);

            //Act
            var actual = manageProductRentersInsurance.ChangeUserStatus(_editorPersonaId, userId, isActive);

            //Assert
            Assert.True(actual);
        }

        [Fact]
        public void ChangeUserStatus_Given_UserId_Disable_ShouldReturn_True()
        {
            //Arrange
            Mock<IInsuranceService> mockRentersInsuranceService = new Mock<IInsuranceService>();
            Mock<IManageBlueBook> mockManageBlueBook = new Mock<IManageBlueBook>();
            Mock<IManagePersona> mockManagePersona = new Mock<IManagePersona>();
            Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();
            Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            Mock<ISamlRepository> mockSamlRepository = new Mock<ISamlRepository>();
            UserAPIResponse userAPIResponse = new UserAPIResponse()
            {
                IsSuccess = true,
                UserId = 1
            };

            int userId = 1;
            bool isActive = false;

            mockRentersInsuranceService
                .Setup(m => m.DisableUser(It.IsAny<UserActionRequest>()))
                .Returns(userAPIResponse);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.Insurance)
                ))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.Insurance)
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.Insurance)
                ))
                .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
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

            IManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(
                editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim,
                messageHandler: mockHttpMessageHandler.Object,
                rentersInsuraceService: mockRentersInsuranceService.Object,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: null,
                manageUserLogin: null,
                managePartyRelationship: null,
                repository: mockRepository.Object);

            //Act
            var actual = manageProductRentersInsurance.ChangeUserStatus(_editorPersonaId, userId, isActive);

            //Assert
            Assert.True(actual);
        }

        [Fact]
        public void ChangeUserStatus_Given_Wrong_UserId_Enable_ShouldReturn_False()
        {
            //Arrange
            Mock<IInsuranceService> mockRentersInsuranceService = new Mock<IInsuranceService>();
            Mock<IManageBlueBook> mockManageBlueBook = new Mock<IManageBlueBook>();
            Mock<IManagePersona> mockManagePersona = new Mock<IManagePersona>();
            Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();
            Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            Mock<ISamlRepository> mockSamlRepository = new Mock<ISamlRepository>();
            UserAPIResponse userAPIResponse = new UserAPIResponse()
            {
                IsSuccess = false,
                UserId = 1
            };

            int userId = 1;
            bool isActive = true;

            mockRentersInsuranceService
                .Setup(m => m.EnableUser(It.IsAny<UserActionRequest>()))
                .Returns(userAPIResponse);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.Insurance)
                ))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.Insurance)
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.Insurance)
                ))
                .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
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

            IManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(
                editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim,
                messageHandler: mockHttpMessageHandler.Object,
                rentersInsuraceService: mockRentersInsuranceService.Object,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: null,
                manageUserLogin: null,
                managePartyRelationship: null,
                repository: mockRepository.Object);

            //Act
            var actual = manageProductRentersInsurance.ChangeUserStatus(_editorPersonaId, userId, isActive);

            //Assert
            Assert.False(actual);
        }

        #endregion

        #endregion
    }
}
