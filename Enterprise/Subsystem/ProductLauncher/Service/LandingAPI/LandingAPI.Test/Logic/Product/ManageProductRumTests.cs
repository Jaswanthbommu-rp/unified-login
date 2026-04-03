using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Rum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.Product
{
    [ExcludeFromCodeCoverage]
    [Collection("ManageProductRumTests")]
    public class ManageProductRumTests : ManageProductBaseTests
    {
        #region Private Variables

        private readonly string _apiEndPoint = "http://localhost";
        private readonly string _clientId = "RumClient";
        private readonly string _clientSecret = "RumClientSecret";
        private readonly string _companyInstanceSourceId = "123456";
        private readonly int _blueBookId = 123;

        // SAML: editor has a product UserId; user also has one (existing user scenarios)
        private readonly string _editorProductUserId = "1234567";
        private readonly string _userProductUserId = "5432";
        private readonly string _userProductUsername = "larry33";

        private IList<CustomerCompanyMap> _mapCompany;
        private Mock<IManageBlueBook> _mockManageBlueBook;
        private Mock<IManagePersona> _mockManagePersona;
        private Mock<IProductInternalSettingRepository> _mockProductInternalSettingRepository;
        private Mock<ISamlRepository> _mockSamlRepository;
        private Mock<IProductRepository> _mockProductRepository;
        private Mock<HttpMessageHandler> _mockTokenHttpMessageHandler;

        // Shared RUM user claims response (returned by GET /user/getuser)
        private readonly RumUserClaims _activeUserClaims = new RumUserClaims
        {
            IsValidUser = true,
            Claims = new List<UserClaim>
            {
                new UserClaim { Type = "crmstatus", Value = "Active" },
                new UserClaim { Type = "nwpusertype", Value = "PM" },
                new UserClaim { Type = "role", Value = "Admin" }
            }
        };

        #endregion

        #region Constructor

        public ManageProductRumTests() : base((int)ProductEnum.UtilityManagement)
        {
            _mockManageBlueBook = new Mock<IManageBlueBook>();
            _mockManagePersona = new Mock<IManagePersona>();
            _mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            _mockSamlRepository = new Mock<ISamlRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockTokenHttpMessageHandler = new Mock<HttpMessageHandler>();

            // SAML attributes — editor has a product user ID
            _editorSamlAttributes = new List<SamlAttributes>
            {
                new SamlAttributes { Name = "UserId", Value = _editorProductUserId },
                new SamlAttributes { Name = "PRODUCTUSERNAME", Value = "bob12" }
            };

            // SAML attributes — user has a product user ID and username (existing user)
            _userSamlAttributes = new List<SamlAttributes>
            {
                new SamlAttributes { Name = "UserId", Value = _userProductUserId },
                new SamlAttributes { Name = "PRODUCTUSERNAME", Value = _userProductUsername }
            };

            _productSettingType.Add(new ProductSettingType { ProductSettingTypeId = 1, Name = "ProductStatus" });
            _userProductSettings.Add(new ProductSettingList { ProductId = (int)ProductEnum.UtilityManagement, Name = "IsFavorite", Value = "1", ProductSettingId = 1234 });

            _electronicAddressList = new List<ElectronicAddress>
            {
                new ElectronicAddress { AddressType = "Email", AddressString = "test@example.com" }
            };

            _productInternalSettings.Add(new ProductInternalSetting { Name = "ApiEndPoint", Value = _apiEndPoint });
            _productInternalSettings.Add(new ProductInternalSetting { Name = "TokenUrl", Value = _apiEndPoint });
            _productInternalSettings.Add(new ProductInternalSetting { Name = "ClientID", Value = _clientId });
            _productInternalSettings.Add(new ProductInternalSetting { Name = "ApiSecret", Value = _clientSecret });

            _repositoryResponseProductStatus.ErrorMessage = "";

            _mapCompany = new List<CustomerCompanyMap>
            {
                new CustomerCompanyMap
                {
                    CustomerCompanyId = _blueBookId,
                    CompanyInstanceSourceId = _companyInstanceSourceId,
                    Source = "NWP"
                }
            };

            var gbProductMap = new GbProductMap
            {
                BooksProductCode = "NWP",
                Name = "Utility Management",
                ProductId = (int)ProductEnum.UtilityManagement,
                UDMSourceCode = "NWP"
            };

            // Token endpoint — returns a mock access token for all tests
            var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new
                {
                    access_token = "mocked-access-token",
                    expires_in = 3600,
                    token_type = "Bearer"
                }))
            };
            _mockTokenHttpMessageHandler.Setup(HttpMethod.Post, $"{_apiEndPoint}/connect/token", tokenResponse);

            // BlueBook
            _mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(_mapCompany);

            // Product internal settings
            _mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(It.Is<int>(l => l == (int)ProductEnum.UtilityManagement)))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(
                    StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.UtilityManagement))))
                .Returns(_productInternalSettings);

            // SAML — editor
            _mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _editorPersonaId),
                    It.Is<int>(l => l == (int)ProductEnum.UtilityManagement)))
                .Returns(_editorSamlAttributes);

            // SAML — user
            _mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _userPersonaId),
                    It.IsAny<int>()))
                .Returns(_userSamlAttributes);

            // Persona — editor
            _mockManagePersona
                .Setup(m => m.GetPersona(It.Is<long>(l => l == _editorPersonaId)))
                .Returns(_editorPersona);

            // Persona — user
            _mockManagePersona
                .Setup(m => m.GetPersona(It.Is<long>(l => l == _userPersonaId)))
                .Returns(_userPersona);

            // Product settings by persona
            _mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(It.IsAny<long>()))
                .Returns(_userProductSettings);

            // Product map
            _mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(It.IsAny<int>()))
                .Returns(gbProductMap);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(new List<GbProductMap> { gbProductMap });

            // Person (for ManageRumUser / UpdateUserProfile)
            var person = new Person { FirstName = "Test", LastName = "User" };
            mockRepository
                .Setup(m => m.GetOne<Person>(StoredProcNameConstants.SP_GetPerson, It.IsAny<object>()))
                .Returns(person);

            // UserLoginOnly (for ManageRumUser / UpdateUserProfile)
            var userLoginOnly = new UserLoginOnly
            {
                RealPageId = _userRealPageId,
                LoginName = "testuser@example.com"
            };
            mockRepository
                .Setup(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.IsAny<object>()))
                .Returns(userLoginOnly);

            // ElectronicAddress for person (email lookup)
            mockRepository
                .Setup(m => m.GetMany<ElectronicAddress>(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(_electronicAddressList);

            // Product setting type (for UpdateProductSettingProductStatus)
            mockRepository
                .Setup(m => m.GetMany<ProductSettingType>(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(_productSettingType);

            // Repository response for product status update
            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(_repositoryResponseProductStatus);

            // ContactMechanismUsageType for ElectronicAddressRepository.ListElectronicAddressForPerson chain
            mockRepository
                .Setup(m => m.GetMany<ContactMechanismUsageType>(StoredProcNameConstants.SP_ListContactMechanismUsageType, It.IsAny<object>()))
                .Returns(new List<ContactMechanismUsageType> { new ContactMechanismUsageType { ContactMechanismUsageTypeId = 0, Name = "Email" } });

            // OrganizationStatus for UpdateProductSettingProductStatus (called when status is Deleted)
            mockRepository
                .Setup(m => m.GetMany<OrganizationStatus>(StoredProcNameConstants.SP_ListOrganizationStatusByUserId, It.IsAny<object>()))
                .Returns(_organizationStatusListUserPersona);

            // IProductRepository mocks for UpdateProductSettingProductStatus
            _mockProductRepository.Setup(m => m.ListProductSettingType()).Returns(_productSettingType);
            _mockProductRepository
                .Setup(m => m.CreateProductSetting(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(_repositoryResponseProductStatus);

            // GetRumUserClaims URL — needed when GetPropertyGroups/GetRegions/GetRoles is called for an existing user
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"{_apiEndPoint}/user/getuser?userId={_userProductUserId}",
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(_activeUserClaims))
                });
        }

        #endregion

        #region Helpers

        private IManageProductRum BuildSut(Mock<HttpMessageHandler> httpHandler = null)
        {
            var handler = httpHandler ?? mockHttpMessageHandler;
            var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri(_apiEndPoint) };
            return new ManageProductRum(
                _editorRealPageId, _editorUserClaim,
                handler.Object, _mockTokenHttpMessageHandler.Object,
                _mockProductInternalSettingRepository.Object,
                _mockManagePersona.Object,
                _mockSamlRepository.Object,
                _mockManageBlueBook.Object,
                _mockProductRepository.Object,
                mockRepository.Object,
                httpClient);
        }

        private IManageProductRum BuildSutWithNewUserSaml(Mock<HttpMessageHandler> httpHandler = null)
        {
            // Override user SAML so _productUserId and _productUsername are empty (new user flow)
            _mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _userPersonaId), It.IsAny<int>()))
                .Returns(new List<SamlAttributes>()); // no SAML = new user

            return BuildSut(httpHandler);
        }

        #endregion

        #region GetPropertyGroups

        [Fact]
        public void GetPropertyGroups_WhenCompanyFound_ReturnsGroups()
        {
            var groups = new List<dynamic>
            {
                new { AccessId = "G1", AccessName = "Group One" },
                new { AccessId = "G2", AccessName = "Group Two" }
            };
            var url = $"{_apiEndPoint}/identity/AccessItems?portfolioId={_companyInstanceSourceId}&accessTypeCd=GM";
            mockHttpMessageHandler.Setup(HttpMethod.Get, url,
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(groups))
                });

            var sut = BuildSut();
            var result = sut.GetPropertyGroups(_editorPersonaId, _userPersonaId, new RequestParameter());

            Assert.False(result.IsError);
            Assert.Equal(2, result.TotalRows);
        }

        [Fact]
        public void GetPropertyGroups_WhenCompanyNotFound_ReturnsError()
        {
            _mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<CustomerCompanyMap>
                {
                    new CustomerCompanyMap { CustomerCompanyId = _blueBookId, CompanyInstanceSourceId = "0", Source = "NWP" }
                });

            var url = $"{_apiEndPoint}/identity/AccessItems?portfolioId=0&accessTypeCd=GM";
            mockHttpMessageHandler.Setup(HttpMethod.Get, url,
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[]")
                });

            var sut = BuildSut();
            var result = sut.GetPropertyGroups(_editorPersonaId, _userPersonaId, new RequestParameter());

            Assert.True(result.IsError);
            Assert.Contains("Company Setup Error", result.ErrorReason);
        }

        [Fact]
        public void GetPropertyGroups_WhenApiReturnsEmpty_ReturnsError()
        {
            var url = $"{_apiEndPoint}/identity/AccessItems?portfolioId={_companyInstanceSourceId}&accessTypeCd=GM";
            mockHttpMessageHandler.Setup(HttpMethod.Get, url,
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[]")
                });

            var sut = BuildSut();
            var result = sut.GetPropertyGroups(_editorPersonaId, _userPersonaId, new RequestParameter());

            Assert.True(result.IsError);
        }

        #endregion

        #region GetProperties

        [Fact]
        public void GetProperties_WhenCompanyFound_ReturnsProperties()
        {
            var properties = new List<object>
            {
                new { PropertyId = "P1", PropertyName = "Property One", State = "TX" },
                new { PropertyId = "P2", PropertyName = "Property Two", State = "CA" }
            };
            // URL must have no spaces — validates the space bug fix
            var url = $"{_apiEndPoint}/identity/Property?companyId={_companyInstanceSourceId}";
            mockHttpMessageHandler.Setup(HttpMethod.Get, url,
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(properties))
                });

            var sut = BuildSut();
            var result = sut.GetProperties(_editorPersonaId, 0, new RequestParameter());

            Assert.False(result.IsError);
            Assert.Equal(2, result.TotalRows);
        }

        [Fact]
        public void GetProperties_WhenApiReturnsEmpty_ReturnsError()
        {
            var url = $"{_apiEndPoint}/identity/Property?companyId={_companyInstanceSourceId}";
            mockHttpMessageHandler.Setup(HttpMethod.Get, url,
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[]")
                });

            var sut = BuildSut();
            var result = sut.GetProperties(_editorPersonaId, 0, new RequestParameter());

            Assert.True(result.IsError);
        }

        [Fact]
        public void GetProperties_WhenApiReturnsError_ReturnsErrorResponse()
        {
            var url = $"{_apiEndPoint}/identity/Property?companyId={_companyInstanceSourceId}";
            mockHttpMessageHandler.Setup(HttpMethod.Get, url,
                new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Internal Server Error")
                });

            var sut = BuildSut();
            var result = sut.GetProperties(_editorPersonaId, 0, new RequestParameter());

            Assert.True(result.IsError);
        }

        #endregion

        #region GetRegions

        [Fact]
        public void GetRegions_WhenCompanyFound_ReturnsRegions()
        {
            var regions = new List<dynamic>
            {
                new { AccessId = "R1", AccessName = "Region One" },
                new { AccessId = "R2", AccessName = "Region Two" },
                new { AccessId = "R3", AccessName = "Region Three" }
            };
            var url = $"{_apiEndPoint}/identity/AccessItems?portfolioId={_companyInstanceSourceId}&accessTypeCd=RM";
            mockHttpMessageHandler.Setup(HttpMethod.Get, url,
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(regions))
                });

            var sut = BuildSut();
            var result = sut.GetRegions(_editorPersonaId, _userPersonaId, new RequestParameter());

            Assert.False(result.IsError);
            Assert.Equal(3, result.TotalRows);
        }

        [Fact]
        public void GetRegions_WhenCompanyNotFound_ReturnsError()
        {
            _mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<CustomerCompanyMap>
                {
                    new CustomerCompanyMap { CustomerCompanyId = _blueBookId, CompanyInstanceSourceId = "0", Source = "NWP" }
                });

            var url = $"{_apiEndPoint}/identity/AccessItems?portfolioId=0&accessTypeCd=RM";
            mockHttpMessageHandler.Setup(HttpMethod.Get, url,
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") });

            var sut = BuildSut();
            var result = sut.GetRegions(_editorPersonaId, _userPersonaId, new RequestParameter());

            Assert.True(result.IsError);
            Assert.Contains("Company Setup Error", result.ErrorReason);
        }

        #endregion

        #region GetRoles

        [Fact]
        public void GetRoles_WhenApiReturnsRoles_ReturnsRoleList()
        {
            var roles = new List<dynamic>
            {
                new { RoleId = 1, RoleName = "Admin", RoleDescription = "Administrator", InternalOnly = false },
                new { RoleId = 2, RoleName = "Viewer", RoleDescription = "Read Only", InternalOnly = false }
            };
            var url = $"{_apiEndPoint}/roleoptions/get?companyId={_companyInstanceSourceId}";
            mockHttpMessageHandler.Setup(HttpMethod.Get, url,
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(roles))
                });

            var sut = BuildSut();
            var result = sut.GetRoles(_editorPersonaId, _userPersonaId, new RequestParameter());

            Assert.False(result.IsError);
            Assert.Equal(2, result.TotalRows);
        }

        [Fact]
        public void GetRoles_WhenApiReturnsEmpty_ReturnsError()
        {
            var url = $"{_apiEndPoint}/roleoptions/get?companyId={_companyInstanceSourceId}";
            mockHttpMessageHandler.Setup(HttpMethod.Get, url,
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") });

            var sut = BuildSut();
            var result = sut.GetRoles(_editorPersonaId, _userPersonaId, new RequestParameter());

            Assert.True(result.IsError);
        }

        #endregion

        #region UnassignRumUser

        [Fact]
        public void UnassignRumUser_WhenDeleteSucceeds_ReturnsEmptyString()
        {
            var url = $"{_apiEndPoint}/user/deleteuser?userId={_userProductUserId}";
            mockHttpMessageHandler.Setup(HttpMethod.Delete, url,
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") });

            var sut = BuildSut();
            var result = sut.UnassignRumUser(_editorPersonaId, _userPersonaId);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void UnassignRumUser_WhenDeleteFails_ReturnsErrorMessage()
        {
            var url = $"{_apiEndPoint}/user/deleteuser?userId={_userProductUserId}";
            mockHttpMessageHandler.Setup(HttpMethod.Delete, url,
                new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("{\"Message\":\"Internal Error\"}")
                });

            var sut = BuildSut();
            var result = sut.UnassignRumUser(_editorPersonaId, _userPersonaId);

            Assert.Contains("There was a problem", result);
            Assert.Contains("500", result);
        }

        [Fact]
        public void UnassignRumUser_WhenUserNotProvisioned_ReturnsProductUserIdMissingError()
        {
            // No SAML attributes = no _productUserId
            _mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _userPersonaId), It.IsAny<int>()))
                .Returns(new List<SamlAttributes>());

            var sut = BuildSut();
            var result = sut.UnassignRumUser(_editorPersonaId, _userPersonaId);

            Assert.Contains("product userId is missing", result);
        }

        [Fact]
        public void UnassignRumUser_WhenEditorPersonaNotFound_ReturnsErrorReason()
        {
            _mockManagePersona
                .Setup(m => m.GetPersona(It.Is<long>(l => l == _editorPersonaId)))
                .Returns((Persona)null);

            var sut = BuildSut();
            var result = sut.UnassignRumUser(_editorPersonaId, _userPersonaId);

            Assert.NotEmpty(result);
        }

        #endregion

        #region ChangeUserStatus

        [Fact]
        public void ChangeUserStatus_WhenDeleteSucceeds_ReturnsTrue()
        {
            var productUserId = "9999";
            var url = $"{_apiEndPoint}/user/deleteuser?userId={productUserId}";
            mockHttpMessageHandler.Setup(HttpMethod.Delete, url,
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") });

            var sut = BuildSut();
            var result = sut.ChangeUserStatus(_editorPersonaId, productUserId);

            Assert.True(result);
        }

        [Fact]
        public void ChangeUserStatus_WhenDeleteFails_ReturnsFalse()
        {
            var productUserId = "9999";
            var url = $"{_apiEndPoint}/user/deleteuser?userId={productUserId}";
            mockHttpMessageHandler.Setup(HttpMethod.Delete, url,
                new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("{\"Message\":\"The request is invalid.\"}")
                });

            var sut = BuildSut();
            var result = sut.ChangeUserStatus(_editorPersonaId, productUserId);

            Assert.False(result);
        }

        [Fact]
        public void ChangeUserStatus_WhenProductUserIdIsEmpty_ReturnsFalse()
        {
            var sut = BuildSut();
            var result = sut.ChangeUserStatus(_editorPersonaId, string.Empty);

            Assert.False(result);
        }

        [Fact]
        public void ChangeUserStatus_WhenProductUserIdIsNull_ReturnsFalse()
        {
            var sut = BuildSut();
            var result = sut.ChangeUserStatus(_editorPersonaId, null);

            Assert.False(result);
        }

        #endregion

        #region ManageRumUser — new user

        [Fact]
        public void ManageRumUser_WhenNewUserAndInsertSucceeds_ReturnsEmptyString()
        {
            var newUserId = "newuser-123";
            var loginName = "testuser@example.com";

            // New user: no existing username collision
            var existsUrl = $"{_apiEndPoint}/user/userexists?userName={loginName}";
            mockHttpMessageHandler.Setup(HttpMethod.Get, existsUrl,
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(false))
                });

            // POST to create user
            var postUrl = $"{_apiEndPoint}/user/postuser";
            mockHttpMessageHandler.Setup(HttpMethod.Post, postUrl,
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(newUserId))
                });

            var userPropertyRegionRole = new RumUserPropertyRegionRole
            {
                PropertyList = new List<string> { "101", "102" },
                PropertyGroupList = new List<string>(),
                RegionList = new List<string>(),
                RoleList = new List<string> { "Admin" }
            };

            var sut = BuildSutWithNewUserSaml();
            var result = sut.ManageRumUser(_editorPersonaId, _userPersonaId, userPropertyRegionRole, out _);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ManageRumUser_WhenNullUserPropertyRegionRole_ReturnsErrorMessage()
        {
            var sut = BuildSutWithNewUserSaml();
            var result = sut.ManageRumUser(_editorPersonaId, _userPersonaId, null, out _);

            Assert.NotEmpty(result);
            Assert.Contains("RumUserPropertyRegionRole", result);
        }

        [Fact]
        public void ManageRumUser_WhenCompanyNotFound_ReturnsCompanySetupError()
        {
            _mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<CustomerCompanyMap>
                {
                    new CustomerCompanyMap { CustomerCompanyId = _blueBookId, CompanyInstanceSourceId = null, Source = "NWP" }
                });

            var userPropertyRegionRole = new RumUserPropertyRegionRole
            {
                PropertyList = new List<string> { "P1" },
                PropertyGroupList = new List<string>(),
                RegionList = new List<string>(),
                RoleList = new List<string>()
            };

            var sut = BuildSutWithNewUserSaml();
            var result = sut.ManageRumUser(_editorPersonaId, _userPersonaId, userPropertyRegionRole, out _);

            Assert.Contains("Company Setup Error", result);
        }

        [Fact]
        public void ManageRumUser_WhenSubcontractorHasNoPropertyGroups_ReturnsError()
        {
            // Make editor same as contract company (subcontractor path)
            var contractRealPageId = _editorPersona.Organization.RealPageId;
            _editorPersona.Organization.RealPageId = new Guid("00000000-0000-0000-0000-000000000001");

            // Setup so contract company matches editor org
            _mockManageBlueBook
                .Setup(m => m.GetCompanyMap(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<CustomerCompanyMap>
                {
                    new CustomerCompanyMap
                    {
                        CustomerCompanyId = _blueBookId,
                        CompanyInstanceSourceId = _companyInstanceSourceId,
                        Source = "NWP"
                    }
                });

            var userPropertyRegionRole = new RumUserPropertyRegionRole
            {
                PropertyList = new List<string>(),
                PropertyGroupList = new List<string>(), // empty — triggers the new guard
                RegionList = new List<string>(),
                RoleList = new List<string>()
            };

            var sut = BuildSutWithNewUserSaml();
            var result = sut.ManageRumUser(_editorPersonaId, _userPersonaId, userPropertyRegionRole, out _);

            // Restore
            _editorPersona.Organization.RealPageId = contractRealPageId;

            Assert.NotEmpty(result);
        }

        [Fact]
        public void ManageRumUser_WhenUsernameExceedsMaxAttempts_ReturnsMaxAttemptsError()
        {
            var loginName = "testuser@example.com";

            // Always return true = username always taken = will hit max attempts
            var existsUrl = $"{_apiEndPoint}/user/userexists?userName=";
            mockHttpMessageHandler.Protected()
                .Setup<System.Threading.Tasks.Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.Method == HttpMethod.Get &&
                        r.RequestUri.ToString().Contains("/user/userexists")),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(true))
                });

            var userPropertyRegionRole = new RumUserPropertyRegionRole
            {
                PropertyList = new List<string> { "101" },
                PropertyGroupList = new List<string>(),
                RegionList = new List<string>(),
                RoleList = new List<string> { "Admin" }
            };

            var sut = BuildSutWithNewUserSaml();
            var result = sut.ManageRumUser(_editorPersonaId, _userPersonaId, userPropertyRegionRole, out _);

            Assert.Contains("Unable to generate a unique username", result);
        }

        [Fact]
        public void ManageRumUser_WhenInsertApiFails_ReturnsErrorWithHttpStatus()
        {
            var loginName = "testuser@example.com";

            // Username doesn't exist
            mockHttpMessageHandler.Protected()
                .Setup<System.Threading.Tasks.Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.Method == HttpMethod.Get &&
                        r.RequestUri.ToString().Contains("/user/userexists")),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(false))
                });

            // POST fails
            var postUrl = $"{_apiEndPoint}/user/postuser";
            mockHttpMessageHandler.Setup(HttpMethod.Post, postUrl,
                new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("Oops! Something went wrong.")
                });

            var userPropertyRegionRole = new RumUserPropertyRegionRole
            {
                PropertyList = new List<string> { "101" },
                PropertyGroupList = new List<string>(),
                RegionList = new List<string>(),
                RoleList = new List<string> { "Admin" }
            };

            var sut = BuildSutWithNewUserSaml();
            var result = sut.ManageRumUser(_editorPersonaId, _userPersonaId, userPropertyRegionRole, out _);

            Assert.Contains("There was a problem creating", result);
            Assert.Contains("401", result);
        }

        #endregion

        #region ManageRumUser — existing user (update)

        [Fact]
        public void ManageRumUser_WhenExistingUserAndUpdateSucceeds_ReturnsEmptyString()
        {
            var getUserUrl = $"{_apiEndPoint}/user/getuser?userId={_userProductUserId}";
            mockHttpMessageHandler.Setup(HttpMethod.Get, getUserUrl,
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(_activeUserClaims))
                });

            var putUrl = $"{_apiEndPoint}/user/putuser?userId={_userProductUserId}";
            mockHttpMessageHandler.Setup(HttpMethod.Put, putUrl,
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { success = true }))
                });

            // Activity log calls — roles, properties, propertygroups, global roles
            SetupActivityLogApiMocks();

            var userPropertyRegionRole = new RumUserPropertyRegionRole
            {
                PropertyList = new List<string> { "101" },
                PropertyGroupList = new List<string>(),
                RegionList = new List<string>(),
                RoleList = new List<string> { "Admin" }
            };

            var sut = BuildSut();
            var result = sut.ManageRumUser(_editorPersonaId, _userPersonaId, userPropertyRegionRole, out _);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ManageRumUser_WhenExistingUserAndUpdateFails_ReturnsErrorWithHttpStatus()
        {
            var getUserUrl = $"{_apiEndPoint}/user/getuser?userId={_userProductUserId}";
            mockHttpMessageHandler.Setup(HttpMethod.Get, getUserUrl,
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(_activeUserClaims))
                });

            var putUrl = $"{_apiEndPoint}/user/putuser?userId={_userProductUserId}";
            mockHttpMessageHandler.Setup(HttpMethod.Put, putUrl,
                new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("Oops! Something went wrong.")
                });

            var userPropertyRegionRole = new RumUserPropertyRegionRole
            {
                PropertyList = new List<string> { "101" },
                PropertyGroupList = new List<string>(),
                RegionList = new List<string>(),
                RoleList = new List<string> { "Admin" }
            };

            var sut = BuildSut();
            var result = sut.ManageRumUser(_editorPersonaId, _userPersonaId, userPropertyRegionRole, out _);

            Assert.Contains("There was a problem updating", result);
            Assert.Contains("401", result);
        }

        #endregion

        #region UpdateUsersMigrationStatus

        [Fact]
        public void UpdateUsersMigrationStatus_WhenApiSucceeds_ReturnsSuccessMessage()
        {
            var migratedUsers = new List<MigrateUser>
            {
                new MigrateUser { UserId = "111", UnifiedLoginUserName = "user1@test.com", UsingUnifiedLogin = true }
            };
            var url = $"{_apiEndPoint}/migration/{_companyInstanceSourceId}/migrate-users";
            mockHttpMessageHandler.Setup(HttpMethod.Post, url,
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { Message = "Success", Status = true }))
                });

            var sut = BuildSut();
            var result = sut.UpdateUsersMigrationStatus(_editorPersonaId, migratedUsers);

            Assert.True(result.Status);
            Assert.Equal("Success", result.Message);
        }

        [Fact]
        public void UpdateUsersMigrationStatus_WhenApiFails_ReturnsFailureMessage()
        {
            var migratedUsers = new List<MigrateUser>
            {
                new MigrateUser { UserId = "111", UnifiedLoginUserName = "user1@test.com", UsingUnifiedLogin = true }
            };
            var url = $"{_apiEndPoint}/migration/{_companyInstanceSourceId}/migrate-users";
            mockHttpMessageHandler.Setup(HttpMethod.Post, url,
                new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Error")
                });

            var sut = BuildSut();
            var result = sut.UpdateUsersMigrationStatus(_editorPersonaId, migratedUsers);

            Assert.False(result.Status);
        }

        #endregion

        #region GetMigrationUsers

        [Fact]
        public void GetMigrationUsers_WhenApiSucceeds_ReturnsUsers()
        {
            var dataFilter = new RequestParameter
            {
                Pages = new PageRequest { StartRow = 0, ResultsPerPage = 1000 },
                FilterBy = new Dictionary<string, string> { { "filter", "NonMigrated" } }
            };
            var users = new List<MigrationUser>
            {
                new MigrationUser { FirstName = "Alice", LastName = "Smith", Email = "alice@test.com", Username = "alice" },
                new MigrationUser { FirstName = "Bob",   LastName = "Jones", Email = "bob@test.com",   Username = "bob" }
            };
            var url = $"{_apiEndPoint}/migration/{_companyInstanceSourceId}/users?filter=NonMigrated&startRow={dataFilter.Pages.StartRow}&resultsPerPage={dataFilter.Pages.ResultsPerPage}";
            mockHttpMessageHandler.Setup(HttpMethod.Get, url,
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(users))
                });

            var sut = BuildSut();
            var result = sut.GetMigrationUsers(_editorPersonaId, dataFilter);

            Assert.False(result.IsError);
            Assert.Equal(2, result.TotalRows);
        }

        [Fact]
        public void GetMigrationUsers_WhenApiReturnsNull_ReturnsError()
        {
            var dataFilter = new RequestParameter
            {
                Pages = new PageRequest { StartRow = 0, ResultsPerPage = 1000 },
                FilterBy = new Dictionary<string, string> { { "filter", "NonMigrated" } }
            };
            var url = $"{_apiEndPoint}/migration/{_companyInstanceSourceId}/users?filter=NonMigrated&startRow={dataFilter.Pages.StartRow}&resultsPerPage={dataFilter.Pages.ResultsPerPage}";
            mockHttpMessageHandler.Setup(HttpMethod.Get, url,
                new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Error")
                });

            var sut = BuildSut();
            var result = sut.GetMigrationUsers(_editorPersonaId, dataFilter);

            Assert.True(result.IsError);
        }

        #endregion

        #region Helpers for activity log API mocks

        /// <summary>
        /// Sets up the 4 API calls that GetUserAccountableData makes for activity logging in ManageRumUser.
        /// These are called twice (old data + new data), so the mocks accept any call.
        /// </summary>
        private void SetupActivityLogApiMocks()
        {
            // Roles
            var rolesUrl = $"{_apiEndPoint}/roleoptions/get?companyId={_companyInstanceSourceId}";
            mockHttpMessageHandler.Protected()
                .Setup<System.Threading.Tasks.Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.Method == HttpMethod.Get && r.RequestUri.ToString().Contains("/roleoptions/get")),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[]")
                });

            // Properties
            mockHttpMessageHandler.Protected()
                .Setup<System.Threading.Tasks.Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.Method == HttpMethod.Get && r.RequestUri.ToString().Contains("/identity/Property")),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[]")
                });

            // Property groups + regions
            mockHttpMessageHandler.Protected()
                .Setup<System.Threading.Tasks.Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.Method == HttpMethod.Get && r.RequestUri.ToString().Contains("/identity/AccessItems")),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[]")
                });

            // User claims (getuser) — needed by UpdateInactiveUser before PUT
            mockHttpMessageHandler.Protected()
                .Setup<System.Threading.Tasks.Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.Method == HttpMethod.Get && r.RequestUri.ToString().Contains("/user/getuser")),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(_activeUserClaims))
                });
        }

        #endregion
    }
}
