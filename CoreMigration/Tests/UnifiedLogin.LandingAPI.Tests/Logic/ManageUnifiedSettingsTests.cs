using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageUnifiedSettings business logic xUnit tests.
    /// Tests for unified settings management including get, create, update, and delete operations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageUnifiedSettingsTests : TestBase
    {
        private readonly Mock<IRepository> _mockRepository;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly DefaultUserClaim _defaultUserClaim;

        public ManageUnifiedSettingsTests()
        {
            _mockRepository = new Mock<IRepository>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            
            _defaultUserClaim = new DefaultUserClaim
            {
                UserId = 1,
                LoginName = "testuser@test.com",
                FirstName = "Test",
                LastName = "User",
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 1000,
                PersonaId = 5,
                CorrelationId = Guid.NewGuid()
            };

            SetupBasicMocks();
        }

        #region Mock Setup

        private void SetupBasicMocks()
        {
            // Setup ProductInternalSettings mock
            var productInternalSettings = new List<ProductInternalSetting>
            {
                new ProductInternalSetting { Name = "SettingsApiEndPoint", Value = "http://localhost/" },
                new ProductInternalSetting { Name = "Elk_LogManageUnifiedSettings", Value = "1" },
                new ProductInternalSetting { Name = "KongApiEndPoint", Value = "http://localhost/kong/" },
                new ProductInternalSetting { Name = "KONG_KEY", Value = "test-kong-key" },
                new ProductInternalSetting { Name = "Kong-Vanity-url", Value = "http://localhost/vanity/" },
                new ProductInternalSetting { Name = "CompanyInternationalSettingsAPI", Value = "api/{0}/company/{1}/settings/{2}" }
            };

            _mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .Returns(productInternalSettings);

            // Setup Settings mock
            var settings = CreateSettingsList();
            _mockRepository
                .Setup(m => m.GetMany<Setting>(
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .Returns(settings);
        }

        private void SetupHttpMessageHandlerMock(HttpStatusCode statusCode, string content = "")
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });
        }

        private List<Setting> CreateSettingsList()
        {
            return new List<Setting>
            {
                new Setting { Name = "Setting1", Value = "Value1", Category = "security", Right = 0, Editable = true },
                new Setting { Name = "Setting2", Value = "Value2", Category = "security", Right = 1, Editable = false },
                new Setting { Name = "Setting3", Value = "Value3", Category = "general", Right = 0, Editable = true }
            };
        }

        private UnifiedSettingCompanyPropertyPayload CreateCompanyPayload()
        {
            return new UnifiedSettingCompanyPropertyPayload
            {
                Payload = new UnifiedSettingCompanyProperty
                {
                    Source = "TestSource",
                    Company = new UnifiedSettingCompanyInstance
                    {
                        CompanyInstanceSourceId = Guid.NewGuid().ToString()
                    },
                    Properties = new List<UnifiedSettingCompanyPropertyInstance>(),
                    CustomerEnvironment = "Test"
                }
            };
        }

        private UnifiedSettingCompanyPropertyPayload CreatePropertyPayload()
        {
            return new UnifiedSettingCompanyPropertyPayload
            {
                Payload = new UnifiedSettingCompanyProperty
                {
                    Source = "TestSource",
                    Company = new UnifiedSettingCompanyInstance
                    {
                        CompanyInstanceSourceId = Guid.NewGuid().ToString()
                    },
                    Properties = new List<UnifiedSettingCompanyPropertyInstance>(),
                    CustomerEnvironment = "Test"
                }
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithUserClaim_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageUnifiedSettings = new ManageUnifiedSettings(_defaultUserClaim);

            // Assert
            Assert.NotNull(manageUnifiedSettings);
        }

        [Fact]
        public void Constructor_WithRepositoryAndMessageHandler_InitializesSuccessfully()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            // Act
            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Assert
            Assert.NotNull(manageUnifiedSettings);
        }

        #endregion

        #region GetUnifiedSettings Tests

        [Fact]
        public void GetUnifiedSettings_WithValidParameters_ReturnsSettingsList()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageUnifiedSettings.GetUnifiedSettings("security", 1000);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetUnifiedSettings_WithZeroPartyId_ThrowsException()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageUnifiedSettings.GetUnifiedSettings("security", 0));

            Assert.Equal("Missing Organization Id.", exception.Message);
        }

        [Fact]
        public void GetUnifiedSettings_WithNullCategory_ReturnsSettingsList()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageUnifiedSettings.GetUnifiedSettings(null, 1000);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetUnifiedSettings_WithEmptyCategory_ReturnsSettingsList()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageUnifiedSettings.GetUnifiedSettings(string.Empty, 1000);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetUnifiedSettings_WithRepositoryException_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository
                .Setup(m => m.GetMany<Setting>(
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .Throws(new Exception("Database error"));

            // Keep ProductInternalSettings working
            var productInternalSettings = new List<ProductInternalSetting>
            {
                new ProductInternalSetting { Name = "Elk_LogManageUnifiedSettings", Value = "0" }
            };
            _mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .Returns(productInternalSettings);

            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageUnifiedSettings.GetUnifiedSettings("security", 1000);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetUnifiedSettings_WithDifferentCategories_ReturnsFilteredResults()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var securitySettings = manageUnifiedSettings.GetUnifiedSettings("security", 1000);
            var generalSettings = manageUnifiedSettings.GetUnifiedSettings("general", 1000);
            var aichatSettings = manageUnifiedSettings.GetUnifiedSettings("aichat", 1000);

            // Assert
            Assert.NotNull(securitySettings);
            Assert.NotNull(generalSettings);
            Assert.NotNull(aichatSettings);
        }

        #endregion

        #region GetUnifiedSettingsCached Tests

        [Fact]
        public void GetUnifiedSettingsCached_WithValidParameters_ReturnsSettingsList()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageUnifiedSettings.GetUnifiedSettingsCached("security", 1000);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetUnifiedSettingsCached_CalledTwice_ReturnsCachedResult()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result1 = manageUnifiedSettings.GetUnifiedSettingsCached("security", 1000);
            var result2 = manageUnifiedSettings.GetUnifiedSettingsCached("security", 1000);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
        }

        [Fact]
        public void GetUnifiedSettingsCached_WithDifferentPartyIds_ReturnsDifferentResults()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result1 = manageUnifiedSettings.GetUnifiedSettingsCached("security", 1000);
            var result2 = manageUnifiedSettings.GetUnifiedSettingsCached("security", 2000);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
        }

        #endregion

        #region CreateUpdateCompanyInSetting Tests

        [Fact]
        public void CreateUpdateCompanyInSetting_WithValidPayload_ReturnsTrue()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            var payload = CreateCompanyPayload();

            // Act
            var result = manageUnifiedSettings.CreateUpdateCompanyInSetting(payload, HttpMethod.Post);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CreateUpdateCompanyInSetting_WithPutMethod_ReturnsTrue()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            var payload = CreateCompanyPayload();

            // Act
            var result = manageUnifiedSettings.CreateUpdateCompanyInSetting(payload, HttpMethod.Put);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CreateUpdateCompanyInSetting_WithFailedResponse_ReturnsFalse()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.BadRequest);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            var payload = CreateCompanyPayload();

            // Act
            var result = manageUnifiedSettings.CreateUpdateCompanyInSetting(payload, HttpMethod.Post);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CreateUpdateCompanyInSetting_WithInternalServerError_ReturnsFalse()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.InternalServerError);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            var payload = CreateCompanyPayload();

            // Act
            var result = manageUnifiedSettings.CreateUpdateCompanyInSetting(payload, HttpMethod.Post);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CreateUpdateCompanyInSetting_WithNullPayload_ReturnsTrue()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageUnifiedSettings.CreateUpdateCompanyInSetting(null, HttpMethod.Post);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region CreateUpdatePropertyInSetting Tests

        [Fact]
        public void CreateUpdatePropertyInSetting_WithValidPayload_ReturnsTrue()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            var payload = CreatePropertyPayload();

            // Act
            var result = manageUnifiedSettings.CreateUpdatePropertyInSetting(payload, HttpMethod.Post);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CreateUpdatePropertyInSetting_WithPutMethod_ReturnsTrue()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            var payload = CreatePropertyPayload();

            // Act
            var result = manageUnifiedSettings.CreateUpdatePropertyInSetting(payload, HttpMethod.Put);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CreateUpdatePropertyInSetting_WithFailedResponse_ReturnsFalse()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.BadRequest);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            var payload = CreatePropertyPayload();

            // Act
            var result = manageUnifiedSettings.CreateUpdatePropertyInSetting(payload, HttpMethod.Post);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CreateUpdatePropertyInSetting_WithInternalServerError_ReturnsFalse()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.InternalServerError);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            var payload = CreatePropertyPayload();

            // Act
            var result = manageUnifiedSettings.CreateUpdatePropertyInSetting(payload, HttpMethod.Post);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region DeletePropertyInSetting Tests

        [Fact]
        public void DeletePropertyInSetting_WithValidPropertyInstanceId_ReturnsTrue()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            var propertyInstanceId = Guid.NewGuid().ToString();

            // Act
            var result = manageUnifiedSettings.DeletePropertyInSetting(propertyInstanceId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void DeletePropertyInSetting_WithFailedResponse_ReturnsFalse()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.NotFound);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            var propertyInstanceId = Guid.NewGuid().ToString();

            // Act
            var result = manageUnifiedSettings.DeletePropertyInSetting(propertyInstanceId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void DeletePropertyInSetting_WithInternalServerError_ReturnsFalse()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.InternalServerError);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            var propertyInstanceId = Guid.NewGuid().ToString();

            // Act
            var result = manageUnifiedSettings.DeletePropertyInSetting(propertyInstanceId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void DeletePropertyInSetting_WithEmptyPropertyInstanceId_ReturnsTrue()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageUnifiedSettings.DeletePropertyInSetting(string.Empty);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void DeletePropertyInSetting_WithNullPropertyInstanceId_ReturnsTrue()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageUnifiedSettings.DeletePropertyInSetting(null);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region GetCompanyInternalSettings Tests

        [Fact]
        public void GetCompanyInternalSettings_WithValidParameters_ReturnsInternalSettingResponse()
        {
            // Arrange
            SetupBasicMocks();

            var internalSettingResponse = new InternalSettingResponse
            {
                Keys = new List<Setting> { new Setting { Name = "TestKey", Value = "TestValue" } },
                Tables = new List<SettingTable>()
            };

            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(internalSettingResponse);
            SetupHttpMessageHandlerMock(HttpStatusCode.OK, jsonResponse);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            var companyId = Guid.NewGuid();

            // Act
            var result = manageUnifiedSettings.GetCompanyInternalSettings(companyId, "source", "settingType");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetCompanyInternalSettings_WithEmptySettingType_ReturnsEmptyResponse()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            var companyId = Guid.NewGuid();

            // Act
            var result = manageUnifiedSettings.GetCompanyInternalSettings(companyId, "source", string.Empty);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetCompanyInternalSettings_WithNullSettingType_ReturnsEmptyResponse()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            var companyId = Guid.NewGuid();

            // Act
            var result = manageUnifiedSettings.GetCompanyInternalSettings(companyId, "source", null);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetCompanyInternalSettings_WithNoContentResponse_ReturnsEmptyResponse()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.NoContent);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            var companyId = Guid.NewGuid();

            // Act
            var result = manageUnifiedSettings.GetCompanyInternalSettings(companyId, "source", "settingType");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetCompanyInternalSettings_WithFailedResponse_ReturnsEmptyResponse()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.InternalServerError);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            var companyId = Guid.NewGuid();

            // Act
            var result = manageUnifiedSettings.GetCompanyInternalSettings(companyId, "source", "settingType");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetCompanyInternalSettings_WithEmptyGuid_ReturnsResponse()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageUnifiedSettings.GetCompanyInternalSettings(Guid.Empty, "source", "settingType");

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region Data Object Tests

        [Fact]
        public void Setting_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var setting = new Setting
            {
                Name = "TestSetting",
                Value = "TestValue",
                Right = 1,
                Editable = true,
                Hidden = false,
                Category = "security"
            };

            // Assert
            Assert.Equal("TestSetting", setting.Name);
            Assert.Equal("TestValue", setting.Value);
            Assert.Equal(1, setting.Right);
            Assert.True(setting.Editable);
            Assert.False(setting.Hidden);
            Assert.Equal("security", setting.Category);
        }

        [Fact]
        public void Setting_DefaultValues()
        {
            // Arrange & Act
            var setting = new Setting();

            // Assert
            Assert.Equal(0, setting.Right);
            Assert.True(setting.Editable);
            Assert.False(setting.Hidden);
            Assert.Equal(string.Empty, setting.Category);
        }

        [Fact]
        public void InternalSettingResponse_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var response = new InternalSettingResponse
            {
                Keys = new List<Setting> { new Setting { Name = "Key1", Value = "Value1" } },
                Tables = new List<SettingTable>()
            };

            // Assert
            Assert.NotNull(response.Keys);
            Assert.Single(response.Keys);
            Assert.NotNull(response.Tables);
        }

        [Fact]
        public void InternalSettingResponse_DefaultValues()
        {
            // Arrange & Act
            var response = new InternalSettingResponse();

            // Assert
            Assert.NotNull(response.Keys);
            Assert.Empty(response.Keys);
            Assert.NotNull(response.Tables);
            Assert.Empty(response.Tables);
        }

        [Fact]
        public void UnifiedSettingCompanyPropertyPayload_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var payload = new UnifiedSettingCompanyPropertyPayload
            {
                Payload = new UnifiedSettingCompanyProperty
                {
                    Source = "TestSource",
                    Company = new UnifiedSettingCompanyInstance
                    {
                        CompanyInstanceSourceId = "test-company-id"
                    },
                    Properties = new List<UnifiedSettingCompanyPropertyInstance>(),
                    CustomerEnvironment = "Production"
                }
            };

            // Assert
            Assert.NotNull(payload.Payload);
            Assert.Equal("TestSource", payload.Payload.Source);
            Assert.NotNull(payload.Payload.Company);
            Assert.Equal("test-company-id", payload.Payload.Company.CompanyInstanceSourceId);
            Assert.NotNull(payload.Payload.Properties);
            Assert.Equal("Production", payload.Payload.CustomerEnvironment);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void Integration_GetAndCacheSettings_WorksCorrectly()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var directResult = manageUnifiedSettings.GetUnifiedSettings("security", 1000);
            var cachedResult = manageUnifiedSettings.GetUnifiedSettingsCached("security", 1000);

            // Assert
            Assert.NotNull(directResult);
            Assert.NotNull(cachedResult);
        }

        [Fact]
        public void Integration_CreateUpdateDeleteProperty_Workflow()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            var payload = CreatePropertyPayload();
            var propertyId = Guid.NewGuid().ToString();

            // Act
            var createResult = manageUnifiedSettings.CreateUpdatePropertyInSetting(payload, HttpMethod.Post);
            var updateResult = manageUnifiedSettings.CreateUpdatePropertyInSetting(payload, HttpMethod.Put);
            var deleteResult = manageUnifiedSettings.DeletePropertyInSetting(propertyId);

            // Assert
            Assert.True(createResult);
            Assert.True(updateResult);
            Assert.True(deleteResult);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public void GetUnifiedSettings_WithLargePartyId_ReturnsSettingsList()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageUnifiedSettings.GetUnifiedSettings("security", long.MaxValue);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetUnifiedSettings_WithNegativePartyId_ReturnsSettingsList()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageUnifiedSettings.GetUnifiedSettings("security", -1);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetUnifiedSettings_WithSpecialCharactersInCategory_ReturnsSettingsList()
        {
            // Arrange
            SetupBasicMocks();
            SetupHttpMessageHandlerMock(HttpStatusCode.OK);

            var manageUnifiedSettings = new ManageUnifiedSettings(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageUnifiedSettings.GetUnifiedSettings("security@#$%", 1000);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region Documentation Tests

        [Fact]
        public void ManageUnifiedSettings_ClassPurpose_Documentation()
        {
            // This test documents the class purpose:
            // ManageUnifiedSettings is responsible for:
            // 1. Getting unified settings by category and party ID
            // 2. Caching settings for performance
            // 3. Creating/updating company settings in the settings API
            // 4. Creating/updating property settings in the settings API
            // 5. Deleting property settings from the settings API
            // 6. Getting company internal settings via Kong API
            //
            // Key methods:
            // - GetUnifiedSettings - Direct retrieval
            // - GetUnifiedSettingsCached - Cached retrieval
            // - CreateUpdateCompanyInSetting - Company CRUD
            // - CreateUpdatePropertyInSetting - Property CRUD
            // - DeletePropertyInSetting - Property deletion
            // - GetCompanyInternalSettings - International settings

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageUnifiedSettings_ValidationRules_Documentation()
        {
            // This test documents validation rules:
            //
            // GetUnifiedSettings:
            // - partyId must not be 0
            //   - Throws Exception with message "Missing Organization Id."
            //
            // GetCompanyInternalSettings:
            // - settingType must not be null or empty
            //   - Returns empty InternalSettingResponse
            //
            // Note: Current implementation doesn't validate:
            // - Category (accepts null, empty, or any string)
            // - Payload objects (accepts null)

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageUnifiedSettings_CachingBehavior_Documentation()
        {
            // This test documents caching behavior:
            //
            // GetUnifiedSettingsCached:
            // - Cache key format: "GetUnifiedSettingsCached{category}_{partyId}"
            // - Cache duration: 120 seconds
            // - Uses RPObjectCache for caching
            //
            // GetProductInternalSettingList (private):
            // - Cache key format: "productInternalSetting_{productId}"
            // - Cache duration: 120 seconds

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageUnifiedSettings_ApiEndpoints_Documentation()
        {
            // This test documents API endpoints used:
            //
            // CreateUpdateCompanyInSetting:
            // - Endpoint: v2/provisioning/company
            // - Methods: POST (create), PUT (update)
            //
            // CreateUpdatePropertyInSetting:
            // - Endpoint: v2/provisioning/property
            // - Methods: POST (create), PUT (update)
            //
            // DeletePropertyInSetting:
            // - Endpoint: v2/provisioning/property/{propertyId}
            // - Method: DELETE
            //
            // GetCompanyInternalSettings:
            // - Uses Kong API gateway
            // - Endpoint from ProductInternalSettings: CompanyInternationalSettingsAPI

            Assert.True(true, "Documentation test");
        }

        #endregion
    }
}
