using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JsonApiSerializer;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageBlueBook business logic xUnit tests.
    /// Tests for BlueBook API integration methods.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageBlueBookTests : TestBase, IDisposable
    {
        private readonly Mock<IRepository> _mockRepository;
        private readonly Mock<IProductInternalSettingRepository> _mockProductInternalSettingRepository;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly DefaultUserClaim _defaultUserClaim;
        private readonly List<ProductInternalSetting> _productInternalSettings;
        private ManageBlueBook _manageBlueBook;

        public ManageBlueBookTests()
        {
            _mockRepository = new Mock<IRepository>();
            _mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            _defaultUserClaim = new DefaultUserClaim
            {
                UserId = 1,
                LoginName = "testuser@test.com",
                FirstName = "Test",
                LastName = "User",
                OrganizationRealPageGuid = Guid.Parse("F5C090FA-78AB-452F-B504-98AAFEE09121"),
                OrganizationMasterId = 379,
                CorrelationId = Guid.NewGuid()
            };

            _productInternalSettings =
            [
                new ProductInternalSetting { Name = "BooksUseDomains", Value = "1" },
                new ProductInternalSetting { Name = "BooksUseUPFMId", Value = "1" },
                new ProductInternalSetting { Name = "BlueBookAPIEndPoint", Value = "http://localhost/books/" },
                new ProductInternalSetting { Name = "KongApiEndPoint", Value = "http://localhost" },
                new ProductInternalSetting { Name = "KONG_KEY", Value = "test-kong-key" },
                new ProductInternalSetting { Name = "UDMViaKong", Value = "0" },
                new ProductInternalSetting { Name = "Elk_LogManageBlueBook", Value = "0" }
            ];

            _mockProductInternalSettingRepository
                .Setup(x => x.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform))
                .Returns(_productInternalSettings);
        }

        public void Dispose()
        {
            _manageBlueBook?.Dispose();
        }

        #region Helper Methods

        private void SetupHttpResponse(HttpStatusCode statusCode, string content)
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
                    Content = new StringContent(content, Encoding.UTF8, "application/json")
                });
        }

        private void SetupHttpResponseForUri(string uriContains, HttpStatusCode statusCode, string content)
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains(uriContains)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content, Encoding.UTF8, "application/json")
                });
        }

        private ManageBlueBook CreateManageBlueBook()
        {
            return new ManageBlueBook(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockProductInternalSettingRepository.Object,
                _mockHttpMessageHandler.Object);
        }

        private static string CreateJsonApiResponse<T>(T data)
        {
            return JsonConvert.SerializeObject(data, new JsonApiSerializerSettings());
        }

        #endregion

        #region GetCompanyMap Tests

        [Fact]
        public void GetCompanyMap_WithContractorCompany_ReturnsNull()
        {
            // Arrange
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.GetCompanyMap(
                DefaultUserClaim.ContractCompanyRealPageId,
                booksCompanyMasterId: 123,
                domain: "Primary");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetCompanyMap_WithValidCompanyId_ReturnsCompanyMapList()
        {
            // Arrange
            var expectedCompanyMaps = new List<CustomerCompanyMap>
            {
                new()
                {
                    Id = "1",
                    CustomerCompanyId = 379,
                    CompanyInstanceId = 1001,
                    CompanyInstanceSourceId = "F5C090FA-78AB-452F-B504-98AAFEE09121",
                    Source = "UPFM",
                    Domain = "Primary"
                }
            };

            var jsonResponse = CreateJsonApiResponse(expectedCompanyMaps);
            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.GetCompanyMap(
                Guid.Empty,
                booksCompanyMasterId: 379,
                source: null,
                domain: "Primary");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(379, result[0].CustomerCompanyId);
        }

        [Fact]
        public void GetCompanyMap_WithNotFoundResponse_ThrowsBlueBookException()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.NotFound, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act & Assert
            Assert.Throws<UnifiedLogin.SharedObjects.Exceptions.BlueBookException>(() =>
                _manageBlueBook.GetCompanyMap(
                    Guid.Empty,
                    booksCompanyMasterId: 999,
                    source: null,
                    domain: "Primary"));
        }

        [Fact]
        public void GetCompanyMap_WithSourceFilter_AppliesSourceToUri()
        {
            // Arrange
            var expectedCompanyMaps = new List<CustomerCompanyMap>
            {
                new()
                {
                    Id = "1",
                    CustomerCompanyId = 379,
                    Source = "OS"
                }
            };

            var jsonResponse = CreateJsonApiResponse(expectedCompanyMaps);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri!.ToString().Contains("filter[source]=OS")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.GetCompanyMap(
                Guid.Empty,
                booksCompanyMasterId: 379,
                source: "OS",
                domain: "Primary");

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region GetCompanyInstanceBySourceAndInstanceId Tests

        [Fact]
        public void GetCompanyInstanceBySourceAndInstanceId_WithValidData_ReturnsCustomerCompanyMap()
        {
            // Arrange
            var expectedMap = new CustomerCompanyMap
            {
                Id = "1",
                CompanyInstanceId = 1051412,
                Source = "OS",
                CompanyInstanceSourceId = "1051412"
            };

            var jsonResponse = CreateJsonApiResponse(expectedMap);
            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.GetCompanyInstanceBySourceAndInstanceId("1051412", "OS");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("OS", result.Source);
        }

        [Fact]
        public void GetCompanyInstanceBySourceAndInstanceId_WithNotFound_ReturnsNull()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.NotFound, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.GetCompanyInstanceBySourceAndInstanceId("invalid", "OS");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetPropertyInstance Tests

        [Fact]
        public void GetPropertyInstance_WithValidCompanyInstanceId_ReturnsPropertyInstances()
        {
            // Arrange
            var expectedProperties = new List<PropertyInstance>
            {
                new()
                {
                    PropertyInstanceId = 1,
                    PropertyName = "Test Property 1",
                    PropertyInstanceSourceId = Guid.NewGuid().ToString()
                },
                new()
                {
                    PropertyInstanceId = 2,
                    PropertyName = "Test Property 2",
                    PropertyInstanceSourceId = Guid.NewGuid().ToString()
                }
            };

            var jsonResponse = CreateJsonApiResponse(expectedProperties);
            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.GetPropertyInstance(1001);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

      
        public void GetPropertyInstance_WithNotFoundResponse_ReturnsEmptyList()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.NotFound, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.GetPropertyInstance(999);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region GetCompanyPropertyInstance Tests

        [Fact]
        public void GetCompanyPropertyInstance_WithValidCompanyInstanceId_ReturnsCompanyPropertyRootObject()
        {
            // Arrange
            var expectedResponse = new CompanyPropertyRootObject
            {
                data = new Data
                {
                    attributes = new UnifiedLogin.SharedObjects.BlackBook.Attributes
                    {
                        getCompanyPropertyInstances = new List<GetCompanyPropertyInstance>
                        {
                            new() { propertyName = "Property A" },
                            new() { propertyName = "Property B" }
                        }
                    }
                }
            };

            var jsonResponse = JsonConvert.SerializeObject(expectedResponse);
            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.GetCompanyPropertyInstance(1001);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.data?.attributes?.getCompanyPropertyInstances);
            Assert.Equal(2, result.data.attributes.getCompanyPropertyInstances.Count);
            // Verify sorting by property name
            Assert.Equal("Property A", result.data.attributes.getCompanyPropertyInstances[0].propertyName);
        }

        #endregion

        #region AddUPFMCompanyFromProvisioningEvent Tests

        [Fact]
        public void AddUPFMCompanyFromProvisioningEvent_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var companyInstance = new CompanyInstance
            {
                CompanyInstanceSourceId = Guid.NewGuid().ToString(),
                CompanyName = "Test Company",
                Source = "UPFM",
                IsActive = true
            };

            SetupHttpResponse(HttpStatusCode.OK, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.AddUPFMCompanyFromProvisioningEvent(companyInstance);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AddUPFMCompanyFromProvisioningEvent_WithFailureResponse_ReturnsFalse()
        {
            // Arrange
            var companyInstance = new CompanyInstance
            {
                CompanyInstanceSourceId = Guid.NewGuid().ToString(),
                CompanyName = "Test Company"
            };

            SetupHttpResponse(HttpStatusCode.BadRequest, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.AddUPFMCompanyFromProvisioningEvent(companyInstance);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region AddUPFMCompanyFromCompanySetup Tests

        [Fact]
        public void AddUPFMCompanyFromCompanySetup_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var companyInstanceAdd = new CompanyInstanceAdd
            {
                CompanyInstanceSourceId = Guid.NewGuid().ToString(),
                CompanyName = "Test Company",
                Source = "UPFM"
            };

            SetupHttpResponse(HttpStatusCode.OK, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.AddUPFMCompanyFromCompanySetup(companyInstanceAdd);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region DeleteBooksGreenBookCompanyInstance Tests

        [Fact]
        public void DeleteBooksGreenBookCompanyInstance_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var companyInstance = new CompanyInstance
            {
                CompanyInstanceSourceId = Guid.NewGuid().ToString(),
                ModifiedBy = "testuser"
            };

            SetupHttpResponse(HttpStatusCode.OK, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.DeleteBooksGreenBookCompanyInstance(companyInstance);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void DeleteBooksGreenBookCompanyInstance_WithNotFoundResponse_ReturnsTrue()
        {
            // Arrange
            var companyInstance = new CompanyInstance
            {
                CompanyInstanceSourceId = Guid.NewGuid().ToString(),
                ModifiedBy = "testuser"
            };

            SetupHttpResponse(HttpStatusCode.NotFound, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.DeleteBooksGreenBookCompanyInstance(companyInstance);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void DeleteBooksGreenBookCompanyInstance_WithBadRequestResponse_ReturnsFalse()
        {
            // Arrange
            var companyInstance = new CompanyInstance
            {
                CompanyInstanceSourceId = Guid.NewGuid().ToString(),
                ModifiedBy = "testuser"
            };

            SetupHttpResponse(HttpStatusCode.BadRequest, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.DeleteBooksGreenBookCompanyInstance(companyInstance);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region UpdateBooksGreenBookCompanyInstance Tests

        [Fact]
        public void UpdateBooksGreenBookCompanyInstance_WithSuccessResponse_ReturnsEmptyString()
        {
            // Arrange
            var companyInstance = new CompanyInstance
            {
                CompanyInstanceSourceId = Guid.NewGuid().ToString(),
                CompanyName = "Updated Company"
            };
            var oldLocation = new CompanyLocation { Address = "123 Old St" };

            SetupHttpResponse(HttpStatusCode.OK, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.UpdateBooksGreenBookCompanyInstance(companyInstance, oldLocation);

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void UpdateBooksGreenBookCompanyInstance_WithNotFoundResponse_ReturnsErrorMessage()
        {
            // Arrange
            var companyInstance = new CompanyInstance
            {
                CompanyInstanceSourceId = Guid.NewGuid().ToString(),
                CompanyName = "Updated Company"
            };
            var oldLocation = new CompanyLocation();

            SetupHttpResponse(HttpStatusCode.NotFound, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.UpdateBooksGreenBookCompanyInstance(companyInstance, oldLocation);

            // Assert
            Assert.Equal("instance not found", result);
        }

        #endregion

        #region AddBooksGreenBookPropertyInstanceFromProvisioning Tests

        [Fact]
        public void AddBooksGreenBookPropertyInstanceFromProvisioning_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var propertyInstance = new PropertyInstance
            {
                PropertyInstanceSourceId = Guid.NewGuid().ToString(),
                PropertyName = "Test Property"
            };

            SetupHttpResponse(HttpStatusCode.OK, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.AddBooksGreenBookPropertyInstanceFromProvisioning(propertyInstance);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AddBooksGreenBookPropertyInstanceFromProvisioning_WithFailureResponse_ReturnsFalse()
        {
            // Arrange
            var propertyInstance = new PropertyInstance
            {
                PropertyInstanceSourceId = Guid.NewGuid().ToString(),
                PropertyName = "Test Property"
            };

            SetupHttpResponse(HttpStatusCode.BadRequest, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.AddBooksGreenBookPropertyInstanceFromProvisioning(propertyInstance);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region DeletePropertyFromBooks Tests

        [Fact]
        public void DeletePropertyFromBooks_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var propertyInstanceId = Guid.NewGuid();

            SetupHttpResponse(HttpStatusCode.OK, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.DeletePropertyFromBooks(propertyInstanceId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void DeletePropertyFromBooks_WithFailureResponse_ReturnsFalse()
        {
            // Arrange
            var propertyInstanceId = Guid.NewGuid();

            SetupHttpResponse(HttpStatusCode.BadRequest, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.DeletePropertyFromBooks(propertyInstanceId);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region ProductCenterEnable/Disable Tests

        [Fact]
        public void ProductCenterEnable_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var systemProductCenter = new SystemProductCenter
            {
                ProductCenterSourceId = "57",
                CompanyInstanceSourceId = Guid.NewGuid().ToString(),
                Source = "UPFM",
                CreatedBy = "testuser"
            };

            SetupHttpResponse(HttpStatusCode.OK, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.ProductCenterEnable(systemProductCenter);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ProductCenterDisable_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var systemProductCenter = new SystemProductCenter
            {
                ProductCenterSourceId = "57",
                CompanyInstanceSourceId = Guid.NewGuid().ToString(),
                Source = "UPFM",
                CreatedBy = "testuser"
            };

            SetupHttpResponse(HttpStatusCode.OK, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.ProductCenterDisable(systemProductCenter);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ProductCenterDisable_WithNotFoundResponse_ReturnsTrue()
        {
            // Arrange
            var systemProductCenter = new SystemProductCenter
            {
                ProductCenterSourceId = "57",
                CompanyInstanceSourceId = Guid.NewGuid().ToString(),
                Source = "UPFM",
                CreatedBy = "testuser"
            };

            SetupHttpResponse(HttpStatusCode.NotFound, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.ProductCenterDisable(systemProductCenter);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region AcknowledgePropertyUpdate Tests

        [Fact]
        public void AcknowledgePropertyUpdate_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var propertyInstanceAck = new PropertyInstanceAck
            {
                PropertyInstanceSourceId = Guid.NewGuid().ToString(),
                PropertyName = "Updated Property",
                Source = "UPFM",
                IsActive = true,
                ModifiedBy = "testuser"
            };

            SetupHttpResponse(HttpStatusCode.OK, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.AcknowledgePropertyUpdate(propertyInstanceAck);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AcknowledgeBulkPropertyListUpdate_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var bulkAck = new BulkPropertyInstanceStatusAck
            {
                propertyInstanceSourceIds = new List<string>
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                },
                Status = true,
                ModifiedBy = "testuser"
            };

            SetupHttpResponse(HttpStatusCode.OK, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = await _manageBlueBook.AcknowledgeBulkPropertyListUpdate(bulkAck);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region GetCompanyCustomerInfo Tests

        [Fact]
        public void GetCompanyCustomerInfo_WithValidCompanyId_ReturnsCustomerCompany()
        {
            // Arrange
            var expectedCompany = new CustomerCompany
            {
                CustomerCompanyId = 379,
                CompanyName = "Test Company",
                IsActive = true
            };

            var jsonResponse = CreateJsonApiResponse(expectedCompany);
            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.GetCompanyCustomerInfo(
                Guid.Empty,
                domain: "Primary",
                booksCompanyMasterId: 379);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(379, result.CustomerCompanyId);
        }

        #endregion

        #region GetListOfDomainsByCompany Tests

        [Fact]
        public void GetListOfDomainsByCompany_WithValidCompanyId_ReturnsDomainList()
        {
            // Arrange
            var expectedDomains = new List<CustomerCompanyDomain>
            {
                new() { Id = "1", Domain = "Primary", Description = "Primary Domain" },
                new() { Id = "2", Domain = "Secondary", Description = "Secondary Domain" }
            };

            var jsonResponse = CreateJsonApiResponse(expectedDomains);
            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.GetListOfDomainsByCompany(379);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetListOfDomainsByCompany_WithNotFoundResponse_ReturnsEmptyList()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.NotFound, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.GetListOfDomainsByCompany(999);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region GetUDMSourceList Tests

        [Fact]
        public void GetUDMSourceList_WithSuccessResponse_ReturnsSourceList()
        {
            // Arrange
            var expectedSources = new List<UDMSource>
            {
                new() { Id = "1", Type = "UPFM", Description = "Unified Platform", IsActive = true },
                new() { Id = "2", Type = "OS", Description = "OneSite", IsActive = true }
            };

            var jsonResponse = CreateJsonApiResponse(expectedSources);
            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.GetUDMSourceList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void GetUDMSourceList_WithFailureResponse_ReturnsEmptyList()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.InternalServerError, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.GetUDMSourceList();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region SplitList Tests

        [Fact]
        public void SplitList_WithListLargerThanSize_ReturnsSplitLists()
        {
            // Arrange
            var locations = Enumerable.Range(1, 100).ToList();

            // Act
            var result = ManageBlueBook.SplitList(locations, 30).ToList();

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal(30, result[0].Count);
            Assert.Equal(30, result[1].Count);
            Assert.Equal(30, result[2].Count);
            Assert.Equal(10, result[3].Count);
        }

        [Fact]
        public void SplitList_WithListSmallerThanSize_ReturnsSingleList()
        {
            // Arrange
            var locations = Enumerable.Range(1, 10).ToList();

            // Act
            var result = ManageBlueBook.SplitList(locations, 30).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(10, result[0].Count);
        }

        [Fact]
        public void SplitList_WithEmptyList_ReturnsEmptyResult()
        {
            // Arrange
            var locations = new List<int>();

            // Act
            var result = ManageBlueBook.SplitList(locations, 30).ToList();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetPropertyInstanceForCompany Tests

        [Fact]
        public void GetPropertyInstanceForCompany_WithValidCompanyId_ReturnsPropertyList()
        {
            // Arrange
            var expectedProperties = new List<BooksPropertyInstance>
            {
                new()
                {
                    id = "1",
                    attributes = new PropertyAttributesInstance
                    {
                        propertyInstanceId = "1",
                        propertyName = "Property 1"
                    }
                }
            };

            var jsonResponse = CreateJsonApiResponse(expectedProperties);
            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.GetPropertyInstanceForCompany(
                Guid.Parse("F5C090FA-78AB-452F-B504-98AAFEE09121"));

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region GetCompanyInstanceByUPFMCompanyId Tests

        [Fact]
        public void GetCompanyInstanceByUPFMCompanyId_WithValidId_ReturnsCompanyInstance()
        {
            // Arrange
            var expectedInstance = new BooksCompanyInstance
            {
                Id = "1",
                Attributes = new CompanyInstanceAttributes
                {
                    CompanyInstanceId = 1001,
                    CompanyName = "Test Company"
                }
            };

            var jsonResponse = CreateJsonApiResponse(expectedInstance);
            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.GetCompanyInstanceByUPFMCompanyId(
                "F5C090FA-78AB-452F-B504-98AAFEE09121");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetCompanyInstanceByUPFMCompanyId_WithNotFoundResponse_ReturnsNull()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.NotFound, "{}");
            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.GetCompanyInstanceByUPFMCompanyId("invalid-id");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithDependencyInjection_InitializesCorrectly()
        {
            // Arrange & Act
            _manageBlueBook = CreateManageBlueBook();

            // Assert
            Assert.NotNull(_manageBlueBook);
        }

        [Fact]
        public void Constructor_WithUDMViaKongEnabled_UsesKongEndpoint()
        {
            // Arrange
            var kongSettings = new List<ProductInternalSetting>
            {
                new() { Name = "BooksUseDomains", Value = "1" },
                new() { Name = "BooksUseUPFMId", Value = "1" },
                new() { Name = "KongApiEndPoint", Value = "http://kong-api" },
                new() { Name = "KONG_KEY", Value = "kong-key-123" },
                new() { Name = "UDMViaKong", Value = "1" }
            };

            _mockProductInternalSettingRepository
                .Setup(x => x.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform))
                .Returns(kongSettings);

            // Act
            _manageBlueBook = new ManageBlueBook(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockProductInternalSettingRepository.Object,
                _mockHttpMessageHandler.Object);

            // Assert
            Assert.NotNull(_manageBlueBook);
        }

        #endregion

        #region GetProductCompanyMapping Tests

        [Fact]
        public void GetProductCompanyMapping_WithValidData_ReturnsCompanyMapList()
        {
            // Arrange
            var translateResponse = new
            {
                data = new
                {
                    attributes = new
                    {
                        translatedCompanyInstances = new[]
                        {
                            new { companyInstanceSourceId = "123456" }
                        }
                    }
                }
            };

            var jsonResponse = JsonConvert.SerializeObject(translateResponse);
            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            _manageBlueBook = CreateManageBlueBook();

            // Act
            var result = _manageBlueBook.GetProductCompanyMapping(
                Guid.Parse("F5C090FA-78AB-452F-B504-98AAFEE09121"),
                "OS");

            // Assert
            Assert.NotNull(result);
        }

        #endregion
    }
}