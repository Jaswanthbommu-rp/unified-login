using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for BlueBookController.
    /// Tests all endpoints, error cases, and edge cases for 100% code coverage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class BlueBookControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private readonly Mock<IManageBlueBook> _mockManageBlueBook;
        private BlueBookController _blueBookController;

        #endregion

        #region Constructor

        public BlueBookControllerTests()
        {
            _mockUserClaimsAccessor = MockUserClaimsAccessor;
            _mockManageBlueBook = new Mock<IManageBlueBook>();

            _blueBookController = new BlueBookController(
                _mockUserClaimsAccessor.Object,
                _mockManageBlueBook.Object
            )
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Act
            var controller = new BlueBookController(
                _mockUserClaimsAccessor.Object,
                _mockManageBlueBook.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_CreatesInstance()
        {
            // Note: The controller doesn't have null checks, so this will create an instance
            // but may fail at runtime when accessing the accessor
            // This test documents the current behavior

            // Act
            var controller = new BlueBookController(
                null!,
                _mockManageBlueBook.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullManageBlueBook_CreatesInstance()
        {
            // Note: The controller doesn't have null checks, so this will create an instance
            // but may fail at runtime when calling the method

            // Act
            var controller = new BlueBookController(
                _mockUserClaimsAccessor.Object,
                null!);

            // Assert
            Assert.NotNull(controller);
        }

        #endregion

        #region GetCustomerProperty Tests

        [Fact]
        public async Task GetCustomerProperty_WithDefaultParameters_ReturnsOkResult()
        {
            // Arrange
            var expectedProperties = new List<ProductProperty>
            {
                new ProductProperty
                {
                    ID = "1",
                    Name = "Test Property",
                    City = "Dallas",
                    State = "TX"
                }
            };

            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(0, null, null, true))
                .Returns(expectedProperties);

            // Act
            var result = await _blueBookController.GetCustomerProperty();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetCustomerProperty_WithBooksCompanyMasterId_ReturnsOkResult()
        {
            // Arrange
            const long booksCompanyMasterId = 12345;
            var expectedProperties = new List<ProductProperty>
            {
                new ProductProperty { ID = "1", Name = "Property 1" },
                new ProductProperty { ID = "2", Name = "Property 2" }
            };

            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(booksCompanyMasterId, null, null, true))
                .Returns(expectedProperties);

            // Act
            var result = await _blueBookController.GetCustomerProperty(booksCompanyMasterId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProperties = Assert.IsAssignableFrom<IList<ProductProperty>>(okResult.Value);
            Assert.Equal(2, returnedProperties.Count);
        }

        [Fact]
        public async Task GetCustomerProperty_WithIncludeParameter_ReturnsOkResult()
        {
            // Arrange
            const string include = "address,status";
            var expectedProperties = new List<ProductProperty>
            {
                new ProductProperty { ID = "1", Name = "Property 1", Street1 = "123 Main St" }
            };

            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(0, include, null, true))
                .Returns(expectedProperties);

            // Act
            var result = await _blueBookController.GetCustomerProperty(include: include);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetCustomerProperty_WithFilterParameter_ReturnsOkResult()
        {
            // Arrange
            const string filter = "status eq 'active'";
            var expectedProperties = new List<ProductProperty>
            {
                new ProductProperty { ID = "1", Name = "Active Property", Status = "active" }
            };

            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(0, null, filter, true))
                .Returns(expectedProperties);

            // Act
            var result = await _blueBookController.GetCustomerProperty(filter: filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetCustomerProperty_WithGetCachedFalse_ReturnsOkResult()
        {
            // Arrange
            var expectedProperties = new List<ProductProperty>
            {
                new ProductProperty { ID = "1", Name = "Fresh Property" }
            };

            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(0, null, null, false))
                .Returns(expectedProperties);

            // Act
            var result = await _blueBookController.GetCustomerProperty(getCached: false);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetCustomerProperty_WithAllParameters_ReturnsOkResult()
        {
            // Arrange
            const long booksCompanyMasterId = 99999;
            const string include = "all";
            const string filter = "name contains 'test'";
            const bool getCached = false;

            var expectedProperties = new List<ProductProperty>
            {
                new ProductProperty
                {
                    ID = "1",
                    Name = "Test Property",
                    Street1 = "123 Test St",
                    City = "Test City",
                    State = "TS",
                    Zip = "12345",
                    Status = "active"
                }
            };

            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(booksCompanyMasterId, include, filter, getCached))
                .Returns(expectedProperties);

            // Act
            var result = await _blueBookController.GetCustomerProperty(
                booksCompanyMasterId, include, filter, getCached);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProperties = Assert.IsAssignableFrom<IList<ProductProperty>>(okResult.Value);
            Assert.Single(returnedProperties);
            Assert.Equal("Test Property", returnedProperties[0].Name);
        }

        [Fact]
        public async Task GetCustomerProperty_WithEmptyResult_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyProperties = new List<ProductProperty>();

            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(emptyProperties);

            // Act
            var result = await _blueBookController.GetCustomerProperty();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProperties = Assert.IsAssignableFrom<IList<ProductProperty>>(okResult.Value);
            Assert.Empty(returnedProperties);
        }

        [Fact]
        public async Task GetCustomerProperty_WithNullResult_ReturnsOkWithNull()
        {
            // Arrange
            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns((IList<ProductProperty>)null!);

            // Act
            var result = await _blueBookController.GetCustomerProperty();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        [Fact]
        public async Task GetCustomerProperty_WithLargeBooksCompanyMasterId_ReturnsOkResult()
        {
            // Arrange
            const long booksCompanyMasterId = long.MaxValue;
            var expectedProperties = new List<ProductProperty>();

            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(booksCompanyMasterId, null, null, true))
                .Returns(expectedProperties);

            // Act
            var result = await _blueBookController.GetCustomerProperty(booksCompanyMasterId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetCustomerProperty_WithNegativeBooksCompanyMasterId_ReturnsOkResult()
        {
            // Arrange
            const long booksCompanyMasterId = -1;
            var expectedProperties = new List<ProductProperty>();

            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(booksCompanyMasterId, null, null, true))
                .Returns(expectedProperties);

            // Act
            var result = await _blueBookController.GetCustomerProperty(booksCompanyMasterId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetCustomerProperty_WithEmptyInclude_ReturnsOkResult()
        {
            // Arrange
            const string include = "";
            var expectedProperties = new List<ProductProperty>
            {
                new ProductProperty { ID = "1", Name = "Property" }
            };

            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(0, include, null, true))
                .Returns(expectedProperties);

            // Act
            var result = await _blueBookController.GetCustomerProperty(include: include);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetCustomerProperty_WithEmptyFilter_ReturnsOkResult()
        {
            // Arrange
            const string filter = "";
            var expectedProperties = new List<ProductProperty>
            {
                new ProductProperty { ID = "1", Name = "Property" }
            };

            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(0, null, filter, true))
                .Returns(expectedProperties);

            // Act
            var result = await _blueBookController.GetCustomerProperty(filter: filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetCustomerProperty_WithWhitespaceInclude_ReturnsOkResult()
        {
            // Arrange
            const string include = "   ";
            var expectedProperties = new List<ProductProperty>();

            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(0, include, null, true))
                .Returns(expectedProperties);

            // Act
            var result = await _blueBookController.GetCustomerProperty(include: include);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetCustomerProperty_WithWhitespaceFilter_ReturnsOkResult()
        {
            // Arrange
            const string filter = "   ";
            var expectedProperties = new List<ProductProperty>();

            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(0, null, filter, true))
                .Returns(expectedProperties);

            // Act
            var result = await _blueBookController.GetCustomerProperty(filter: filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetCustomerProperty_WithSpecialCharactersInFilter_ReturnsOkResult()
        {
            // Arrange
            const string filter = "name eq 'O''Brien & Sons'";
            var expectedProperties = new List<ProductProperty>
            {
                new ProductProperty { ID = "1", Name = "O'Brien & Sons" }
            };

            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(0, null, filter, true))
                .Returns(expectedProperties);

            // Act
            var result = await _blueBookController.GetCustomerProperty(filter: filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetCustomerProperty_WithVeryLongFilter_ReturnsOkResult()
        {
            // Arrange
            var filter = new string('a', 1000);
            var expectedProperties = new List<ProductProperty>();

            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(0, null, filter, true))
                .Returns(expectedProperties);

            // Act
            var result = await _blueBookController.GetCustomerProperty(filter: filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetCustomerProperty_CallsManageBlueBookWithCorrectParameters()
        {
            // Arrange
            const long booksCompanyMasterId = 555;
            const string include = "testInclude";
            const string filter = "testFilter";
            const bool getCached = false;

            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(booksCompanyMasterId, include, filter, getCached))
                .Returns(new List<ProductProperty>());

            // Act
            await _blueBookController.GetCustomerProperty(booksCompanyMasterId, include, filter, getCached);

            // Assert
            _mockManageBlueBook.Verify(
                x => x.GetCustomerProperty(booksCompanyMasterId, include, filter, getCached),
                Times.Once);
        }

        [Fact]
        public async Task GetCustomerProperty_WithMultipleProperties_ReturnsAllProperties()
        {
            // Arrange
            var expectedProperties = new List<ProductProperty>
            {
                new ProductProperty { ID = "1", Name = "Property 1" },
                new ProductProperty { ID = "2", Name = "Property 2" },
                new ProductProperty { ID = "3", Name = "Property 3" },
                new ProductProperty { ID = "4", Name = "Property 4" },
                new ProductProperty { ID = "5", Name = "Property 5" }
            };

            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(expectedProperties);

            // Act
            var result = await _blueBookController.GetCustomerProperty();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProperties = Assert.IsAssignableFrom<IList<ProductProperty>>(okResult.Value);
            Assert.Equal(5, returnedProperties.Count);
        }

        [Fact]
        public async Task GetCustomerProperty_WithFullPropertyDetails_ReturnsCompleteData()
        {
            // Arrange
            var expectedProperties = new List<ProductProperty>
            {
                new ProductProperty
                {
                    ID = "12345",
                    Name = "Complete Property",
                    Street1 = "123 Main Street",
                    Street2 = "Suite 100",
                    City = "Dallas",
                    State = "TX",
                    Zip = "75201",
                    IsAssigned = true,
                    Alias = "Main Office",
                    Active = "Y",
                    InstanceId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                    Latitude = 32.7767m,
                    Longitude = -96.7970m,
                    CustomerPropertyId = "CP-001",
                    Status = "active"
                }
            };

            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(expectedProperties);

            // Act
            var result = await _blueBookController.GetCustomerProperty();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProperties = Assert.IsAssignableFrom<IList<ProductProperty>>(okResult.Value);
            Assert.Single(returnedProperties);

            var property = returnedProperties[0];
            Assert.Equal("12345", property.ID);
            Assert.Equal("Complete Property", property.Name);
            Assert.Equal("Dallas", property.City);
            Assert.Equal("TX", property.State);
            Assert.True(property.IsAssigned);
        }

        #endregion

        #region Concurrent Access Tests

        [Fact]
        public async Task GetCustomerProperty_MultipleConcurrentCalls_AllReturnOk()
        {
            // Arrange
            var expectedProperties = new List<ProductProperty>
            {
                new ProductProperty { ID = "1", Name = "Property" }
            };

            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(expectedProperties);

            var tasks = new List<Task<IActionResult>>();

            // Act - Simulate 10 concurrent calls
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(_blueBookController.GetCustomerProperty(i));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            foreach (var result in results)
            {
                Assert.IsType<OkObjectResult>(result);
            }
        }

        #endregion

        #region Parameter Combination Tests

        [Theory]
        [InlineData(0, null, null, true)]
        [InlineData(100, null, null, true)]
        [InlineData(0, "include", null, true)]
        [InlineData(0, null, "filter", true)]
        [InlineData(0, null, null, false)]
        [InlineData(100, "include", null, true)]
        [InlineData(100, null, "filter", true)]
        [InlineData(100, null, null, false)]
        [InlineData(0, "include", "filter", true)]
        [InlineData(0, "include", null, false)]
        [InlineData(0, null, "filter", false)]
        [InlineData(100, "include", "filter", true)]
        [InlineData(100, "include", null, false)]
        [InlineData(100, null, "filter", false)]
        [InlineData(0, "include", "filter", false)]
        [InlineData(100, "include", "filter", false)]
        public async Task GetCustomerProperty_WithVariousParameterCombinations_ReturnsOkResult(
            long booksCompanyMasterId, string include, string filter, bool getCached)
        {
            // Arrange
            _mockManageBlueBook
                .Setup(x => x.GetCustomerProperty(booksCompanyMasterId, include, filter, getCached))
                .Returns(new List<ProductProperty>());

            // Act
            var result = await _blueBookController.GetCustomerProperty(booksCompanyMasterId, include, filter, getCached);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _blueBookController = null!;
            base.Dispose();
        }

        #endregion
    }
}





