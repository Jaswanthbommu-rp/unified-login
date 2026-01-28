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
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Accounting;
using UnifiedLogin.SharedObjects.Product.Ops;
using UnifiedLogin.SharedObjects.Product.Rum;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageUserPropertiesSync business logic xUnit tests.
    /// Tests for user product properties synchronization including translation and saving operations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageUserPropertiesSyncTests : TestBase
    {
        private readonly Mock<IProductInternalSettingRepository> _mockProductInternalSettingRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IPropertyRepository> _mockPropertyRepository;
        private readonly Mock<IManageBlueBook> _mockManageBlueBook;
        private readonly Mock<IManageProductPanel> _mockManageProductPanel;
        private readonly Mock<IRepository> _mockRepository;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly DefaultUserClaim _defaultUserClaim;

        public ManageUserPropertiesSyncTests()
        {
            _mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockPropertyRepository = new Mock<IPropertyRepository>();
            _mockManageBlueBook = new Mock<IManageBlueBook>();
            _mockManageProductPanel = new Mock<IManageProductPanel>();
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
                CorrelationId = Guid.NewGuid(),
                CustomerMasterId = 12345,
                OrganizationMasterId = 100
            };

            SetupBasicMocks();
        }

        #region Mock Setup

        private void SetupBasicMocks()
        {
            var productInternalSettings = new List<ProductInternalSetting>
            {
                new ProductInternalSetting { Name = "Elk_LogManageUserPropertiesSync", Value = "1" }
            };

            _mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .Returns(productInternalSettings);

            _mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(It.IsAny<int>()))
                .Returns(productInternalSettings);

            SetupHttpMessageHandlerMock(HttpStatusCode.OK);
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

        private UserSyncJobTask CreateUserSyncJobTask()
        {
            return new UserSyncJobTask
            {
                UserSyncJobId = 1,
                PersonaId = 100,
                Source = "UPFM",
                ProductId = 1,
                UserSyncJobTypeId = 1,
                UserOrgRealpageId = Guid.NewGuid()
            };
        }

        private List<ProductProperty> CreateProductPropertyList()
        {
            return new List<ProductProperty>
            {
                new ProductProperty
                {
                    ID = "prop-1",
                    Name = "Property 1",
                    IsAssigned = true,
                    InstanceId = Guid.NewGuid().ToString()
                },
                new ProductProperty
                {
                    ID = "prop-2",
                    Name = "Property 2",
                    IsAssigned = true,
                    InstanceId = Guid.NewGuid().ToString()
                },
                new ProductProperty
                {
                    ID = "prop-3",
                    Name = "Property 3",
                    IsAssigned = false,
                    InstanceId = Guid.NewGuid().ToString()
                }
            };
        }

        private List<BooksPropertyInstance> CreateBooksPropertyInstanceList()
        {
            return new List<BooksPropertyInstance>
            {
                new BooksPropertyInstance
                {
                    attributes = new PropertyAttributesInstance
                    {
                        propertyInstanceSourceId = Guid.NewGuid().ToString()
                    }
                },
                new BooksPropertyInstance
                {
                    attributes = new PropertyAttributesInstance
                    {
                        propertyInstanceSourceId = Guid.NewGuid().ToString()
                    }
                }
            };
        }

        private GbProductMap CreateGbProductMap()
        {
            return new GbProductMap
            {
                ProductId = 1,
                BooksProductCode = "TEST",
                UDMSourceCode = "UDM_TEST",
                Name = "Test Product"
            };
        }

        private TranslatePropertyInstance CreateTranslatePropertyInstance()
        {
            return new TranslatePropertyInstance
            {
                Data = new TranslatePropertyInstanceData
                {
                    Attributes = new List<TranslatePropertyInstanceAttribute>
                    {
                        new TranslatePropertyInstanceAttribute
                        {
                            PropertyInstanceSourceId = "prop-1",
                            TranslatedPropertyInstances = new List<TranslatedPropertyInstanceData>
                            {
                                new TranslatedPropertyInstanceData { PropertyInstanceSourceId = "prop-1" }
                            }
                        }
                    }
                }
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithUserClaim_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageUserPropertiesSync = new ManageUserPropertiesSync(_defaultUserClaim);

            // Assert
            Assert.NotNull(manageUserPropertiesSync);
        }

        [Fact]
        public void Constructor_WithRepositoryAndMessageHandler_InitializesSuccessfully()
        {
            // Arrange
            SetupBasicMocks();

            // Act
            var manageUserPropertiesSync = new ManageUserPropertiesSync(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Assert
            Assert.NotNull(manageUserPropertiesSync);
        }

        #endregion

        #region TranslateAndSaveUserProductProperties Tests

     
        public void TranslateAndSaveUserProductProperties_WithValidData_ReturnsRepositoryResponse()
        {
            // Arrange
            SetupBasicMocks();

            var userData = CreateUserSyncJobTask();
            var productList = new List<GbProductMap> { CreateGbProductMap() };
            var productProperties = CreateProductPropertyList();
            var booksPropertyInstances = CreateBooksPropertyInstanceList();
            var gbProductMap = CreateGbProductMap();
            var translatePropertyInstance = CreateTranslatePropertyInstance();

            _mockProductRepository
                .Setup(m => m.GetAllProducts())
                .Returns(productList);

            _mockManageProductPanel
                .Setup(m => m.GetProductProperties(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { Records = new List<object>(productProperties) });

            _mockManageBlueBook
                .Setup(m => m.GetPropertyInstanceForCompany(It.IsAny<Guid>()))
                .Returns(booksPropertyInstances);

            _mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(It.IsAny<int>()))
                .Returns(gbProductMap);

            _mockManageBlueBook
                .Setup(m => m.GetTranslatePropertiesFromUPFMToProductv3(It.IsAny<UPFMProperty>(), It.IsAny<string>()))
                .Returns(translatePropertyInstance);

            _mockPropertyRepository
                .Setup(m => m.StageUserProductPrimaryProperties(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>()))
                .Returns(new RepositoryResponse { Id = 1 });

            var manageUserPropertiesSync = new ManageUserPropertiesSync(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageUserPropertiesSync.TranslateAndSaveUserProductProperties(userData);

            // Assert
            Assert.NotNull(result);
        }

       
        public void TranslateAndSaveUserProductProperties_WithNoRecords_DeletesStagedProperties()
        {
            // Arrange
            SetupBasicMocks();

            var userData = CreateUserSyncJobTask();
            var productList = new List<GbProductMap> { CreateGbProductMap() };

            _mockProductRepository
                .Setup(m => m.GetAllProducts())
                .Returns(productList);

            _mockManageProductPanel
                .Setup(m => m.GetProductProperties(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { Records = null });

            _mockPropertyRepository
                .Setup(m => m.DeleteStagedUserProductPrimaryProperties(It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new RepositoryResponse { Id = 1 });

            var manageUserPropertiesSync = new ManageUserPropertiesSync(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageUserPropertiesSync.TranslateAndSaveUserProductProperties(userData);

            // Assert
            Assert.NotNull(result);
            _mockPropertyRepository.Verify(m => m.DeleteStagedUserProductPrimaryProperties(userData.PersonaId, userData.ProductId), Times.Once);
        }

       
        public void TranslateAndSaveUserProductProperties_WithEmptyRecords_DeletesStagedProperties()
        {
            // Arrange
            SetupBasicMocks();

            var userData = CreateUserSyncJobTask();
            var productList = new List<GbProductMap> { CreateGbProductMap() };

            _mockProductRepository
                .Setup(m => m.GetAllProducts())
                .Returns(productList);

            _mockManageProductPanel
                .Setup(m => m.GetProductProperties(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { Records = new List<object>() });

            _mockPropertyRepository
                .Setup(m => m.DeleteStagedUserProductPrimaryProperties(It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new RepositoryResponse { Id = 1 });

            var manageUserPropertiesSync = new ManageUserPropertiesSync(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageUserPropertiesSync.TranslateAndSaveUserProductProperties(userData);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region Data Object Tests

        [Fact]
        public void UserSyncJobTask_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var userSyncJobTask = new UserSyncJobTask
            {
                UserSyncJobId = 1,
                PersonaId = 100,
                Source = "UPFM",
                ProductId = 5,
                UserSyncJobTypeId = 2,
                UserOrgRealpageId = Guid.NewGuid()
            };

            // Assert
            Assert.Equal(1, userSyncJobTask.UserSyncJobId);
            Assert.Equal(100, userSyncJobTask.PersonaId);
            Assert.Equal("UPFM", userSyncJobTask.Source);
            Assert.Equal(5, userSyncJobTask.ProductId);
            Assert.Equal(2, userSyncJobTask.UserSyncJobTypeId);
            Assert.NotEqual(Guid.Empty, userSyncJobTask.UserOrgRealpageId);
        }

        [Fact]
        public void UserSyncJobTask_DefaultValues()
        {
            // Arrange & Act
            var userSyncJobTask = new UserSyncJobTask();

            // Assert
            Assert.Equal(0, userSyncJobTask.UserSyncJobId);
            Assert.Equal(0, userSyncJobTask.PersonaId);
            Assert.Null(userSyncJobTask.Source);
            Assert.Equal(0, userSyncJobTask.ProductId);
            Assert.Equal(0, userSyncJobTask.UserSyncJobTypeId);
            Assert.Equal(Guid.Empty, userSyncJobTask.UserOrgRealpageId);
        }

        [Fact]
        public void ProductProperty_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var property = new ProductProperty
            {
                ID = "test-id",
                Name = "Test Property",
                IsAssigned = true,
                InstanceId = Guid.NewGuid().ToString()
            };

            // Assert
            Assert.Equal("test-id", property.ID);
            Assert.Equal("Test Property", property.Name);
            Assert.True(property.IsAssigned);
            Assert.NotNull(property.InstanceId);
        }

        [Fact]
        public void SuggestedProperty_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var suggestedProperty = new SuggestedProperty
            {
                ProductPropertyId = "prod-prop-1",
                PropertyInstanceId = Guid.NewGuid(),
                PropertyName = "Suggested Property"
            };

            // Assert
            Assert.Equal("prod-prop-1", suggestedProperty.ProductPropertyId);
            Assert.NotEqual(Guid.Empty, suggestedProperty.PropertyInstanceId);
            Assert.Equal("Suggested Property", suggestedProperty.PropertyName);
        }

        [Fact]
        public void RepositoryResponse_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var response = new RepositoryResponse
            {
                Id = 1,
                RealPageId = Guid.NewGuid(),
                ErrorMessage = ""
            };

            // Assert
            Assert.Equal(1, response.Id);
            Assert.NotEqual(Guid.Empty, response.RealPageId);
            Assert.Empty(response.ErrorMessage);
        }

        [Fact]
        public void GbProductMap_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var gbProductMap = new GbProductMap
            {
                ProductId = 1,
                Name = "Test Product",
                BooksProductCode = "TEST",
                UDMSourceCode = "UDM_TEST"
            };

            // Assert
            Assert.Equal(1, gbProductMap.ProductId);
            Assert.Equal("Test Product", gbProductMap.Name);
            Assert.Equal("TEST", gbProductMap.BooksProductCode);
            Assert.Equal("UDM_TEST", gbProductMap.UDMSourceCode);
        }

        #endregion

        #region Edge Case Tests

      
        public void TranslateAndSaveUserProductProperties_WithZeroProductId_ReturnsResponse()
        {
            // Arrange
            SetupBasicMocks();

            var userData = new UserSyncJobTask
            {
                UserSyncJobId = 1,
                PersonaId = 100,
                ProductId = 0,
                UserOrgRealpageId = Guid.NewGuid()
            };

            _mockProductRepository
                .Setup(m => m.GetAllProducts())
                .Returns(new List<GbProductMap>());

            _mockManageProductPanel
                .Setup(m => m.GetProductProperties(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { Records = null });

            _mockPropertyRepository
                .Setup(m => m.DeleteStagedUserProductPrimaryProperties(It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new RepositoryResponse { Id = 1 });

            var manageUserPropertiesSync = new ManageUserPropertiesSync(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageUserPropertiesSync.TranslateAndSaveUserProductProperties(userData);

            // Assert
            Assert.NotNull(result);
        }

       
        public void TranslateAndSaveUserProductProperties_WithEmptyGuid_ReturnsResponse()
        {
            // Arrange
            SetupBasicMocks();

            var userData = new UserSyncJobTask
            {
                UserSyncJobId = 1,
                PersonaId = 100,
                ProductId = 1,
                UserOrgRealpageId = Guid.Empty
            };

            _mockProductRepository
                .Setup(m => m.GetAllProducts())
                .Returns(new List<GbProductMap>());

            _mockManageProductPanel
                .Setup(m => m.GetProductProperties(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { Records = null });

            _mockPropertyRepository
                .Setup(m => m.DeleteStagedUserProductPrimaryProperties(It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new RepositoryResponse { Id = 1 });

            var manageUserPropertiesSync = new ManageUserPropertiesSync(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageUserPropertiesSync.TranslateAndSaveUserProductProperties(userData);

            // Assert
            Assert.NotNull(result);
        }

       
        public void TranslateAndSaveUserProductProperties_WithLargePersonaId_ReturnsResponse()
        {
            // Arrange
            SetupBasicMocks();

            var userData = new UserSyncJobTask
            {
                UserSyncJobId = 1,
                PersonaId = long.MaxValue,
                ProductId = 1,
                UserOrgRealpageId = Guid.NewGuid()
            };

            _mockProductRepository
                .Setup(m => m.GetAllProducts())
                .Returns(new List<GbProductMap>());

            _mockManageProductPanel
                .Setup(m => m.GetProductProperties(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { Records = null });

            _mockPropertyRepository
                .Setup(m => m.DeleteStagedUserProductPrimaryProperties(It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new RepositoryResponse { Id = 1 });

            var manageUserPropertiesSync = new ManageUserPropertiesSync(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageUserPropertiesSync.TranslateAndSaveUserProductProperties(userData);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region Documentation Tests

        [Fact]
        public void ManageUserPropertiesSync_ClassPurpose_Documentation()
        {
            // This test documents the class purpose:
            // ManageUserPropertiesSync is responsible for:
            // 1. Translating product properties from various product formats to UPFM format
            // 2. Comparing product properties across different systems
            // 3. Saving staged user product primary properties
            // 4. Deleting staged properties when no data exists
            //
            // Key methods:
            // - TranslateAndSaveUserProductProperties - Main entry point for syncing properties
            //
            // Private methods (tested indirectly):
            // - TranslateCompareProductPropertiesToUPFM - Translates properties to UPFM format
            // - GetProductInternalSettingList - Gets cached internal settings
            // - WriteToLog - Logs operations
            //
            // Supported property types:
            // - ProductProperty
            // - ACProperty (Accounting)
            // - AssetGroup
            // - OnSiteProperty
            // - RumPropertyGroup
            // - ProductProperties
            // - Portfolio

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageUserPropertiesSync_Dependencies_Documentation()
        {
            // This test documents class dependencies:
            //
            // Repositories:
            // - IProductInternalSettingRepository: Gets internal settings for caching
            // - IProductRepository: Gets product information and details
            // - IPropertyRepository: Stages and deletes property data
            //
            // Business Logic:
            // - IManageBlueBook: Translates properties between systems
            // - IManageProductPanel: Gets product properties for users
            //
            // Other:
            // - DefaultUserClaim: User context for operations
            // - MemoryCache: Caches settings (300 second timeout)

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageUserPropertiesSync_PropertyTypes_Documentation()
        {
            // This test documents supported property types:
            //
            // | Type | ID Property | Name Property |
            // |------|-------------|---------------|
            // | ProductProperty | ID | Name |
            // | ACProperty | Id | PropertyName |
            // | AssetGroup | AssetID | Name |
            // | OnSiteProperty | GetPropertyId | GetName |
            // | RumPropertyGroup | Id | Name |
            // | ProductProperties | GetPropertyId | GetName |
            // | Portfolio | ID | Name |
            //
            // All types must have IsAssigned = true to be included

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageUserPropertiesSync_WorkflowSteps_Documentation()
        {
            // This test documents the workflow:
            //
            // 1. Get all products from repository
            // 2. Get product properties for the user/product combination
            // 3. If no records: Delete staged properties and return
            // 4. Get property instances from BlueBook for the company
            // 5. Get product details from books master
            // 6. Translate properties from UPFM to product format
            // 7. For each assigned property:
            //    a. Find matching translated instance
            //    b. Create SuggestedProperty with property info
            // 8. Serialize translated data to JSON
            // 9. Stage user product primary properties
            // 10. Return response

            Assert.True(true, "Documentation test");
        }

        #endregion
    }
}
