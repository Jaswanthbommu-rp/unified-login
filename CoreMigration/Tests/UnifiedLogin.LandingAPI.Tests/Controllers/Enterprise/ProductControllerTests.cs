using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPIEnterprise.Controllers;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.ResponseObject;
using Xunit;
using ProductUsers = UnifiedLogin.SharedObjects.Enterprise.ProductUsers;

namespace UnifiedLogin.LandingAPI.Tests.Controllers.Enterprise
{
    /// <summary>
    /// Comprehensive unit tests for ProductController with 100% code coverage.
    /// Tests all endpoints, error cases, and validation scenarios.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProductControllerTests : ControllerBase
    {
        #region Private Fields

        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private readonly ProductController _controller;
        private readonly DefaultUserClaim _defaultUserClaim;

        #endregion

        #region Constructor & Setup

        public ProductControllerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();

            _defaultUserClaim = new DefaultUserClaim
            {
                UserId = 1,
                LoginName = "testuser@test.com",
                FirstName = "Test",
                LastName = "User",
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 1000,
                CorrelationId = Guid.NewGuid(),
                OrganizationName = "Test Organization"
            };

            _mockUserClaimsAccessor.Setup(x => x.CorrelationId).Returns(_defaultUserClaim.CorrelationId);
            _mockUserClaimsAccessor.Setup(x => x.LoginName).Returns(_defaultUserClaim.LoginName);

            _controller = new ProductController(
                _mockProductRepository.Object,
                _mockUserClaimsAccessor.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        //public void Dispose()
        //{
        //    //_controller = null!;
        //    base.Dispose();
        //}

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Act
            var controller = new ProductController(
                _mockProductRepository.Object,
                _mockUserClaimsAccessor.Object);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullProductRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ProductController(null, _mockUserClaimsAccessor.Object));
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ProductController(_mockProductRepository.Object, null));
        }

        #endregion

        #region GetProducts Tests

        [Fact]
        public void GetProducts_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var products = new List<GbProductMap>
            {
                new GbProductMap { ProductId = 1, BooksProductCode = "ONESITE", Name = "OneSite" },
                new GbProductMap { ProductId = 2, BooksProductCode = "OPS", Name = "Ops" }
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);

            // Act
            var result = _controller.GetProducts();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var returnedProducts = okResult.Value as IList<GbProductMap>;
            returnedProducts.Should().NotBeNull();
            returnedProducts.Should().HaveCount(2);
        }

        [Fact]
        public void GetProducts_ExcludesSystemProducts()
        {
            // Arrange
            var products = new List<GbProductMap>
            {
                new GbProductMap { ProductId = 1, BooksProductCode = "OS", Name = "OneSite" },
                new GbProductMap { ProductId = 42, BooksProductCode = "SF", Name = "SalesForce" },
                new GbProductMap { ProductId = 3, BooksProductCode = "UPFM", Name = "Unified Platform" },
                new GbProductMap { ProductId = 2, BooksProductCode = "UI", Name = "Unified UI" }
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);

            // Act
            var result = _controller.GetProducts();

            // Assert
            var okResult = result as OkObjectResult;
            var returnedProducts = okResult.Value as IList<GbProductMap>;
            returnedProducts.Should().HaveCount(1);
            returnedProducts[0].BooksProductCode.Should().Be("OS");
        }

        [Fact]
        public void GetProducts_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(new List<GbProductMap>());

            // Act
            var result = _controller.GetProducts();

            // Assert
            var okResult = result as OkObjectResult;
            var returnedProducts = okResult.Value as IList<GbProductMap>;
            returnedProducts.Should().BeEmpty();
        }

        #endregion

        #region GetUsersByCompanyorProducts Tests

        [Fact]
        public void GetUsersByCompanyorProducts_WithValidCompanyId_ReturnsOkResult()
        {
            // Arrange
            var productUsers = new List<ProductUsers>
            {
                new ProductUsers { UserId = 1, LoginName = "user1@test.com", FirstName = "User", LastName = "One" }
            };

            _mockProductRepository.Setup(x => x.GetUsersByCompanyorProducts(
                "12345", null, null, null, null)).Returns(productUsers);

            // Act
            var result = _controller.GetUsersByCompanyorProducts("12345");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().NotBeNull();
        }

        [Fact]
        public void GetUsersByCompanyorProducts_WithValidProducts_ReturnsOkResult()
        {
            // Arrange
            var products = new List<int?> { 1, 2 };
            var productUsers = new List<ProductUsers>
            {
                new ProductUsers { UserId = 1, LoginName = "user1@test.com" }
            };

            _mockProductRepository.Setup(x => x.GetUsersByCompanyorProducts(
                 "12345", null, It.IsAny<IList<int>>(), 5000, 1, null, null, null, null))
                 .Returns(productUsers);

            // Act
            var result = _controller.GetUsersByCompanyorProducts(products: products);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetUsersByCompanyorProducts_WithInvalidParameters_ReturnsBadRequest()
        {
            // Act
            var result = _controller.GetUsersByCompanyorProducts();

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void GetUsersByCompanyorProducts_WithNullCompanyIdAndNullProducts_ReturnsBadRequest()
        {
            // Act
            var result = _controller.GetUsersByCompanyorProducts(null, null, null);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void GetUsersByCompanyorProducts_WithInvalidCompanyIdAndNullProducts_ReturnsBadRequest()
        {
            // Act
            var result = _controller.GetUsersByCompanyorProducts("invalid", null, null);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void GetUsersByCompanyorProducts_WithValidCompanyIdAndUserType_PassesUserTypeToRepository()
        {
            // Arrange
            var userType = "Admin";
            _mockProductRepository.Setup(x => x.GetUsersByCompanyorProducts(
                "12345", null, null, userType, null)).Returns(new List<ProductUsers>());

            // Act
            _controller.GetUsersByCompanyorProducts("12345", userType: userType);

            // Assert
            _mockProductRepository.Verify(
                x => x.GetUsersByCompanyorProducts("12345", null, null, userType, null),
                Times.Once);
        }

        #endregion

        #region GetULUserIdMappedToProductUserIdByCompanyAndProducts Tests

        [Fact]
        public void GetULUserIdMappedToProductUserIdByCompanyAndProducts_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var request = new ProductUserIDMappingRequest
            {
                CompanyId = 12345,
                ProductCode = "ONESITE",
                ProductUserId = new List<string> { "1", "2" }
            };

            var products = new List<GbProductMap>
            {
                new GbProductMap { ProductId = 1, BooksProductCode = "ONESITE" }
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);
            _mockProductRepository.Setup(x => x.GetULMappingPersonaIDsByCompanyAndProducts(
                12345, null, 1, request.ProductUserId))
                .Returns(new List<ULMappedPersonaIds>());

            // Act
            var result = _controller.GetULUserIdMappedToProductUserIdByCompanyAndProducts(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var mappedDetails = okResult.Value as MappedUnifiedLoginUserDetails;
            mappedDetails.Should().NotBeNull();
        }

        [Fact]
        public void GetULUserIdMappedToProductUserIdByCompanyAndProducts_WithNullRequest_ReturnsBadRequest()
        {
            // Act
            var result = _controller.GetULUserIdMappedToProductUserIdByCompanyAndProducts(null);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void GetULUserIdMappedToProductUserIdByCompanyAndProducts_WithInvalidProductCode_ReturnsBadRequest()
        {
            // Arrange
            var request = new ProductUserIDMappingRequest
            {
                CompanyId = 12345,
                ProductCode = "INVALID",
                ProductUserId = new List<string> { "1" }
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(new List<GbProductMap>());

            // Act
            var result = _controller.GetULUserIdMappedToProductUserIdByCompanyAndProducts(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void GetULUserIdMappedToProductUserIdByCompanyAndProducts_WithZeroCompanyIdAndNullUpfmId_ReturnsBadRequest()
        {
            // Arrange
            var request = new ProductUserIDMappingRequest
            {
                CompanyId = 0,
                upfmId = null,
                ProductCode = "ONESITE",
                ProductUserId = new List<string> { "1" }
            };

            // Act
            var result = _controller.GetULUserIdMappedToProductUserIdByCompanyAndProducts(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void GetULUserIdMappedToProductUserIdByCompanyAndProducts_WithValidUpfmId_ReturnsOkResult()
        {
            // Arrange
            var upfmId = Guid.NewGuid().ToString();
            var request = new ProductUserIDMappingRequest
            {
                CompanyId = 0,
                upfmId = upfmId,
                ProductCode = "ONESITE",
                ProductUserId = new List<string> { "1" }
            };

            var products = new List<GbProductMap>
            {
                new GbProductMap { ProductId = 1, BooksProductCode = "ONESITE" }
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);
            _mockProductRepository.Setup(x => x.GetULMappingPersonaIDsByCompanyAndProducts(
                0, upfmId, 1, request.ProductUserId))
                .Returns(new List<ULMappedPersonaIds>());

            // Act
            var result = _controller.GetULUserIdMappedToProductUserIdByCompanyAndProducts(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetULUserIdMappedToProductUserIdByCompanyAndProducts_WithNullProductCode_ReturnsBadRequest()
        {
            // Arrange
            var request = new ProductUserIDMappingRequest
            {
                CompanyId = 12345,
                ProductCode = null,
                ProductUserId = new List<string> { "1" }
            };

            // Act
            var result = _controller.GetULUserIdMappedToProductUserIdByCompanyAndProducts(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void GetULUserIdMappedToProductUserIdByCompanyAndProducts_WithEmptyProductUserId_ReturnsBadRequest()
        {
            // Arrange
            var request = new ProductUserIDMappingRequest
            {
                CompanyId = 12345,
                ProductCode = "ONESITE",
                ProductUserId = new List<string>()
            };

            // Act
            var result = _controller.GetULUserIdMappedToProductUserIdByCompanyAndProducts(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region GetUsersByCompanyorProductCodes Tests

        [Fact]
        public void GetUsersByCompanyorProductCodes_WithValidParameters_ReturnsOkResult()
        {
            // Arrange
            var productCodes = new List<string> { "ONESITE" };
            var productMap = new List<GbProductMap>
            {
                new GbProductMap { ProductId = 1, BooksProductCode = "ONESITE" }
            };
            var productUsers = new List<ProductUsers>
            {
                new ProductUsers { UserId = 1, LoginName = "user1@test.com", TotalRecords = 1 }
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(productMap);
            _mockProductRepository.Setup(x => x.GetProductSharedwithOtherProductIdList(It.IsAny<IList<int>>()))
                .Returns(new List<int> { 1 });
            _mockProductRepository.Setup(x => x.GetUsersByCompanyorProducts(
                "12345", null, It.IsAny<IList<int>>(), 5000, 1, null, null, null, null))
                .Returns(productUsers);

            // Act
            var result = _controller.GetUsersByCompanyorProductCodes(productCodes, "12345");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult.Value as PagedResponse;
            response.Should().NotBeNull();
            response.Meta.TotalRows.Should().Be(1);
        }

        [Fact]
        public void GetUsersByCompanyorProductCodes_WithNullCompanyIdAndUpfmId_ReturnsBadRequest()
        {
            // Arrange
            var productCodes = new List<string> { "ONESITE" };

            // Act
            var result = _controller.GetUsersByCompanyorProductCodes(productCodes, null, null);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            var response = badRequestResult.Value as PagedResponse;
            response.IsError.Should().BeTrue();
        }

        [Fact]
        public void GetUsersByCompanyorProductCodes_WithNullProductCodes_ReturnsBadRequest()
        {
            // Act
            var result = _controller.GetUsersByCompanyorProductCodes(null, "12345");

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
        }

        [Fact]
        public void GetUsersByCompanyorProductCodes_WithEmptyProductCodes_ReturnsBadRequest()
        {
            // Arrange
            var productCodes = new List<string>();

            // Act
            var result = _controller.GetUsersByCompanyorProductCodes(productCodes, "12345");

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
        }

        [Fact]
        public void GetUsersByCompanyorProductCodes_WithPropertyIds_CleansDuplicates()
        {
            // Arrange
            var productCodes = new List<string> { "ONESITE" };
            var propertyIds = new List<string> { "prop1", "prop1", "prop2", "", "prop3" };
            var productMap = new List<GbProductMap>
            {
                new GbProductMap { ProductId = 1, BooksProductCode = "ONESITE" }
            };
            var productUsers = new List<ProductUsers>
            {
                new ProductUsers { UserId = 1, LoginName = "user1@test.com", TotalRecords = 1 }
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(productMap);
            _mockProductRepository.Setup(x => x.GetProductSharedwithOtherProductIdList(It.IsAny<IList<int>>()))
                .Returns(new List<int> { 1 });
            _mockProductRepository.Setup(x => x.GetUsersByCompanyorProducts(
                "12345", null, It.IsAny<IList<int>>(), 5000, 1, null, null, It.IsAny<List<string>>(), null))
                .Returns(productUsers);

            // Act
            var result = _controller.GetUsersByCompanyorProductCodes(productCodes, "12345", propertyIds: propertyIds);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockProductRepository.Verify(
                x => x.GetUsersByCompanyorProducts(
                    "12345", null, It.IsAny<IList<int>>(), 5000, 1, null, null,
                    It.Is<List<string>>(list => list.Count == 3), null),
                Times.Once);
        }

        [Fact]
        public void GetUsersByCompanyorProductCodes_WhenRepositoryReturnsNull_ReturnsBadRequest()
        {
            // Arrange
            var productCodes = new List<string> { "ONESITE" };
            var productMap = new List<GbProductMap>
            {
                new GbProductMap { ProductId = 1, BooksProductCode = "ONESITE" }
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(productMap);
            _mockProductRepository.Setup(x => x.GetProductSharedwithOtherProductIdList(It.IsAny<IList<int>>()))
                .Returns(new List<int> { 1 });
            _mockProductRepository.Setup(x => x.GetUsersByCompanyorProducts(
                "12345", null, It.IsAny<IList<int>>(), 5000, 1, null, null, null, null))
                .Returns((IList<ProductUsers>)null);

            // Act
            var result = _controller.GetUsersByCompanyorProductCodes(productCodes, "12345");

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            var response = badRequestResult.Value as PagedResponse;
            response.IsError.Should().BeTrue();
        }

        [Fact]
        public void GetUsersByCompanyorProductCodes_WithRoles_PassesToRepository()
        {
            // Arrange
            var productCodes = new List<string> { "ONESITE" };
            var roles = new List<string> { "Admin", "User" };
            var productMap = new List<GbProductMap>
            {
                new GbProductMap { ProductId = 1, BooksProductCode = "ONESITE" }
            };
            var productUsers = new List<ProductUsers>
            {
                new ProductUsers { UserId = 1, LoginName = "user1@test.com", TotalRecords = 1 }
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(productMap);
            _mockProductRepository.Setup(x => x.GetProductSharedwithOtherProductIdList(It.IsAny<IList<int>>()))
                .Returns(new List<int> { 1 });
            _mockProductRepository.Setup(x => x.GetUsersByCompanyorProducts(
                "12345", null, It.IsAny<IList<int>>(), 5000, 1, roles, null, null, null))
                .Returns(productUsers);

            // Act
            _controller.GetUsersByCompanyorProductCodes(productCodes, "12345", roles: roles);

            // Assert
            _mockProductRepository.Verify(
                x => x.GetUsersByCompanyorProducts(
                    "12345", null, It.IsAny<IList<int>>(), 5000, 1, roles, null, null, null),
                Times.Once);
        }

        [Fact]
        public void GetUsersByCompanyorProductCodes_WithRightsFilter_PassesToRepository()
        {
            // Arrange
            var productCodes = new List<string> { "ONESITE" };
            var rights = new List<string> { "Read", "Write" };
            var productMap = new List<GbProductMap>
            {
                new GbProductMap { ProductId = 1, BooksProductCode = "ONESITE" }
            };
            var productUsers = new List<ProductUsers>
            {
                new ProductUsers { UserId = 1, LoginName = "user1@test.com", TotalRecords = 1 }
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(productMap);
            _mockProductRepository.Setup(x => x.GetProductSharedwithOtherProductIdList(It.IsAny<IList<int>>()))
                .Returns(new List<int> { 1 });
            _mockProductRepository.Setup(x => x.GetUsersByCompanyorProducts(
                "12345", null, It.IsAny<IList<int>>(), 5000, 1, null, rights, null, null))
                .Returns(productUsers);

            // Act
            _controller.GetUsersByCompanyorProductCodes(productCodes, "12345", rights: rights);

            // Assert
            _mockProductRepository.Verify(
                x => x.GetUsersByCompanyorProducts(
                    "12345", null, It.IsAny<IList<int>>(), 5000, 1, null, rights, null, null),
                Times.Once);
        }

        [Fact]
        public void GetUsersByCompanyorProductCodes_WithUpfmId_PassesToRepository()
        {
            // Arrange
            var productCodes = new List<string> { "ONESITE" };
            var upfmId = Guid.NewGuid().ToString();
            var productMap = new List<GbProductMap>
            {
                new GbProductMap { ProductId = 1, BooksProductCode = "ONESITE" }
            };
            var productUsers = new List<ProductUsers>
            {
                new ProductUsers { UserId = 1, LoginName = "user1@test.com", TotalRecords = 1 }
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(productMap);
            _mockProductRepository.Setup(x => x.GetProductSharedwithOtherProductIdList(It.IsAny<IList<int>>()))
                .Returns(new List<int> { 1 });
            _mockProductRepository.Setup(x => x.GetUsersByCompanyorProducts(
                null, upfmId, It.IsAny<IList<int>>(), 5000, 1, null, null, null, null))
                .Returns(productUsers);

            // Act
            _controller.GetUsersByCompanyorProductCodes(productCodes, upfmId: upfmId);

            // Assert
            _mockProductRepository.Verify(
                x => x.GetUsersByCompanyorProducts(
                    null, upfmId, It.IsAny<IList<int>>(), 5000, 1, null, null, null, null),
                Times.Once);
        }

        [Fact]
        public void GetUsersByCompanyorProductCodes_WithPagination_PassesToRepository()
        {
            // Arrange
            var productCodes = new List<string> { "ONESITE" };
            var productMap = new List<GbProductMap>
            {
                new GbProductMap { ProductId = 1, BooksProductCode = "ONESITE" }
            };
            var productUsers = new List<ProductUsers>
            {
                new ProductUsers { UserId = 1, LoginName = "user1@test.com", TotalRecords = 100 }
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(productMap);
            _mockProductRepository.Setup(x => x.GetProductSharedwithOtherProductIdList(It.IsAny<IList<int>>()))
                .Returns(new List<int> { 1 });
            _mockProductRepository.Setup(x => x.GetUsersByCompanyorProducts(
                "12345", null, It.IsAny<IList<int>>(), 10, 2, null, null, null, null))
                .Returns(productUsers);

            // Act
            var result = _controller.GetUsersByCompanyorProductCodes(
                productCodes, "12345", rowsPerPage: 10, pageNumber: 2);

            // Assert
            var okResult = result as OkObjectResult;
            var response = okResult.Value as PagedResponse;
            response.Meta.CurrentPage.Should().Be(2);
            response.Meta.RowsPerPage.Should().Be(10);
        }

        #endregion

        #region Edge Cases and Error Scenarios

        [Fact]
        public void GetUsersByCompanyorProductCodes_WithEmptyPropertyIdsList_ReturnsNullPropertyList()
        {
            // Arrange
            var productCodes = new List<string> { "ONESITE" };
            var propertyIds = new List<string>();
            var productMap = new List<GbProductMap>
            {
                new GbProductMap { ProductId = 1, BooksProductCode = "ONESITE" }
            };
            var productUsers = new List<ProductUsers>
            {
                new ProductUsers { UserId = 1, LoginName = "user1@test.com", TotalRecords = 1 }
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(productMap);
            _mockProductRepository.Setup(x => x.GetProductSharedwithOtherProductIdList(It.IsAny<IList<int>>()))
                .Returns(new List<int> { 1 });
            _mockProductRepository.Setup(x => x.GetUsersByCompanyorProducts(
                "12345", null, It.IsAny<IList<int>>(), 5000, 1, null, null, null, null))
                .Returns(productUsers);

            // Act
            _controller.GetUsersByCompanyorProductCodes(productCodes, "12345", propertyIds: propertyIds);

            // Assert
            _mockProductRepository.Verify(
                x => x.GetUsersByCompanyorProducts(
                    "12345", null, It.IsAny<IList<int>>(), 5000, 1, null, null, null, null),
                Times.Once);
        }

        [Fact]
        public void GetULUserIdMappedToProductUserIdByCompanyAndProducts_PropertyInitialization_IsCorrect()
        {
            // Arrange
            var request = new ProductUserIDMappingRequest
            {
                CompanyId = 12345,
                ProductCode = "ONESITE",
                ProductUserId = new List<string> { "1" }
            };

            var products = new List<GbProductMap>
            {
                new GbProductMap { ProductId = 1, BooksProductCode = "ONESITE" }
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);
            _mockProductRepository.Setup(x => x.GetULMappingPersonaIDsByCompanyAndProducts(
                12345, null, 1, request.ProductUserId))
                .Returns(new List<ULMappedPersonaIds>());

            // Act
            var result = _controller.GetULUserIdMappedToProductUserIdByCompanyAndProducts(request);

            // Assert
            var okResult = result as OkObjectResult;
            var mappedDetails = okResult.Value as MappedUnifiedLoginUserDetails;
            mappedDetails.CompanyId.Should().Be(12345);
            mappedDetails.ProductCode.Should().Be("ONESITE");
            mappedDetails.ULMappedPersonaId.Should().NotBeNull();
        }

        #endregion
    }
}
//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.Extensions.Logging;
//using Moq;
//using UnifiedLogin.BusinessLogic.Logic.Interfaces;
//using UnifiedLogin.BusinessLogic.Repository.Interfaces;
//using UnifiedLogin.LandingAPI.Controllers;
//using UnifiedLogin.LandingAPI.Tests.Helpers;
//using UnifiedLogin.SharedObjects;
//using UnifiedLogin.SharedObjects.BlackBook;
//using UnifiedLogin.SharedObjects.Enum;
//using UnifiedLogin.SharedObjects.IdentityConfig;
//using UnifiedLogin.SharedObjects.Landing;
//using UnifiedLogin.SharedObjects.Product;
//using Xunit;

//namespace UnifiedLogin.LandingAPI.Tests.Controllers
//{
//    [ExcludeFromCodeCoverage]
//    public class ProductControllerTests : ControllerTestBase
//    {
//        private readonly Mock<IManageProduct> _mockManageProduct;
//        private readonly Mock<IUserLoginRepository> _mockUserLoginRepository;
//        private readonly Mock<ISamlRepository> _mockSamlRepository;
//        private readonly Mock<IProductRepository> _mockProductRepository;
//        private readonly Mock<IManageBlueBook> _mockManageBlueBook;
//        private readonly Mock<IManagePersona> _mockManagePersona;
//        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
//        private readonly Mock<ILogger> _mockLogger;
//        private ProductController _productController;

//        public ProductControllerTests()
//        {
//            _mockManageProduct = new Mock<IManageProduct>();
//            _mockUserLoginRepository = new Mock<IUserLoginRepository>();
//            _mockSamlRepository = new Mock<ISamlRepository>();
//            _mockProductRepository = new Mock<IProductRepository>();
//            _mockManageBlueBook = new Mock<IManageBlueBook>();
//            _mockManagePersona = new Mock<IManagePersona>();
//            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
//            _mockLogger = new Mock<ILogger>();

//            _productController = new ProductController(
//                _mockManageProduct.Object,
//                _mockUserLoginRepository.Object,
//                _mockSamlRepository.Object,
//                _mockProductRepository.Object,
//                _mockManageBlueBook.Object,
//                _mockManagePersona.Object,
//                MockUserClaimsAccessor.Object,
//                MockMemoryCache.Object,
//                _mockHttpClientFactory.Object,
//                _mockLogger.Object)
//            {
//                ControllerContext = CreateControllerContext()
//            };
//        }

//        #region Constructor Tests

//        [Fact]
//        public void Constructor_WithValidDependencies_CreatesInstance()
//        {
//            var controller = new ProductController(
//                _mockManageProduct.Object,
//                _mockUserLoginRepository.Object,
//                _mockSamlRepository.Object,
//                _mockProductRepository.Object,
//                _mockManageBlueBook.Object,
//                _mockManagePersona.Object,
//                MockUserClaimsAccessor.Object,
//                MockMemoryCache.Object,
//                _mockHttpClientFactory.Object,
//                _mockLogger.Object);
//            Assert.NotNull(controller);
//        }

//        [Fact]
//        public void Constructor_WithNullManageProduct_ThrowsArgumentNullException()
//        {
//            Assert.Throws<ArgumentNullException>(() => new ProductController(
//                null!,
//                _mockUserLoginRepository.Object,
//                _mockSamlRepository.Object,
//                _mockProductRepository.Object,
//                _mockManageBlueBook.Object,
//                _mockManagePersona.Object,
//                MockUserClaimsAccessor.Object,
//                MockMemoryCache.Object,
//                _mockHttpClientFactory.Object,
//                _mockLogger.Object));
//        }

//        [Fact]
//        public void Constructor_WithNullUserLoginRepository_ThrowsArgumentNullException()
//        {
//            Assert.Throws<ArgumentNullException>(() => new ProductController(
//                _mockManageProduct.Object,
//                null!,
//                _mockSamlRepository.Object,
//                _mockProductRepository.Object,
//                _mockManageBlueBook.Object,
//                _mockManagePersona.Object,
//                MockUserClaimsAccessor.Object,
//                MockMemoryCache.Object,
//                _mockHttpClientFactory.Object,
//                _mockLogger.Object));
//        }

//        [Fact]
//        public void Constructor_WithNullSamlRepository_ThrowsArgumentNullException()
//        {
//            Assert.Throws<ArgumentNullException>(() => new ProductController(
//                _mockManageProduct.Object,
//                _mockUserLoginRepository.Object,
//                null!,
//                _mockProductRepository.Object,
//                _mockManageBlueBook.Object,
//                _mockManagePersona.Object,
//                MockUserClaimsAccessor.Object,
//                MockMemoryCache.Object,
//                _mockHttpClientFactory.Object,
//                _mockLogger.Object));
//        }

//        [Fact]
//        public void Constructor_WithNullProductRepository_ThrowsArgumentNullException()
//        {
//            Assert.Throws<ArgumentNullException>(() => new ProductController(
//                _mockManageProduct.Object,
//                _mockUserLoginRepository.Object,
//                _mockSamlRepository.Object,
//                null!,
//                _mockManageBlueBook.Object,
//                _mockManagePersona.Object,
//                MockUserClaimsAccessor.Object,
//                MockMemoryCache.Object,
//                _mockHttpClientFactory.Object,
//                _mockLogger.Object));
//        }

//        [Fact]
//        public void Constructor_WithNullManageBlueBook_ThrowsArgumentNullException()
//        {
//            Assert.Throws<ArgumentNullException>(() => new ProductController(
//                _mockManageProduct.Object,
//                _mockUserLoginRepository.Object,
//                _mockSamlRepository.Object,
//                _mockProductRepository.Object,
//                null!,
//                _mockManagePersona.Object,
//                MockUserClaimsAccessor.Object,
//                MockMemoryCache.Object,
//                _mockHttpClientFactory.Object,
//                _mockLogger.Object));
//        }

//        [Fact]
//        public void Constructor_WithNullManagePersona_ThrowsArgumentNullException()
//        {
//            Assert.Throws<ArgumentNullException>(() => new ProductController(
//                _mockManageProduct.Object,
//                _mockUserLoginRepository.Object,
//                _mockSamlRepository.Object,
//                _mockProductRepository.Object,
//                _mockManageBlueBook.Object,
//                null!,
//                MockUserClaimsAccessor.Object,
//                MockMemoryCache.Object,
//                _mockHttpClientFactory.Object,
//                _mockLogger.Object));
//        }

//        [Fact]
//        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
//        {
//            Assert.Throws<ArgumentNullException>(() => new ProductController(
//                _mockManageProduct.Object,
//                _mockUserLoginRepository.Object,
//                _mockSamlRepository.Object,
//                _mockProductRepository.Object,
//                _mockManageBlueBook.Object,
//                _mockManagePersona.Object,
//                null!,
//                MockMemoryCache.Object,
//                _mockHttpClientFactory.Object,
//                _mockLogger.Object));
//        }

//        [Fact]
//        public void Constructor_WithNullMemoryCache_ThrowsArgumentNullException()
//        {
//            Assert.Throws<ArgumentNullException>(() => new ProductController(
//                _mockManageProduct.Object,
//                _mockUserLoginRepository.Object,
//                _mockSamlRepository.Object,
//                _mockProductRepository.Object,
//                _mockManageBlueBook.Object,
//                _mockManagePersona.Object,
//                MockUserClaimsAccessor.Object,
//                null!,
//                _mockHttpClientFactory.Object,
//                _mockLogger.Object));
//        }

//        [Fact]
//        public void Constructor_WithNullHttpClientFactory_ThrowsArgumentNullException()
//        {
//            Assert.Throws<ArgumentNullException>(() => new ProductController(
//                _mockManageProduct.Object,
//                _mockUserLoginRepository.Object,
//                _mockSamlRepository.Object,
//                _mockProductRepository.Object,
//                _mockManageBlueBook.Object,
//                _mockManagePersona.Object,
//                MockUserClaimsAccessor.Object,
//                MockMemoryCache.Object,
//                null!,
//                _mockLogger.Object));
//        }

//        [Fact]
//        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
//        {
//            Assert.Throws<ArgumentNullException>(() => new ProductController(
//                _mockManageProduct.Object,
//                _mockUserLoginRepository.Object,
//                _mockSamlRepository.Object,
//                _mockProductRepository.Object,
//                _mockManageBlueBook.Object,
//                _mockManagePersona.Object,
//                MockUserClaimsAccessor.Object,
//                MockMemoryCache.Object,
//                _mockHttpClientFactory.Object,
//                null!));
//        }

//        #endregion

//        #region ListProductUsers Tests

//        //[Fact]
//        //public async Task ListProductUsers_WithValidParameters_ReturnsProductUsers()
//        //{
//        //    var productId = (int)ProductEnum.OneSite;
//        //    var companyInstanceId = 100L;
//        //    var personaId = 200L;

//        //    _mockManageProduct
//        //        .Setup(x => x.GetProductUsers(productId, companyInstanceId, personaId))
//        //        .Returns(new List<ProductUsers> { new ProductUsers() });

//        //    var result = await _productController.ListProductUsers(productId, companyInstanceId, personaId);

//        //    var okResult = Assert.IsType<OkObjectResult>(result);
//        //    var output = Assert.IsType<ObjectListOutput<ProductUsers, IErrorData>>(okResult.Value);
//        //    Assert.True(output.Status.Success);
//        //}

//        //[Fact]
//        //public async Task ListProductUsers_WithInvalidProductId_ReturnsError()
//        //{
//        //    var productId = 9999;
//        //    var companyInstanceId = 100L;
//        //    var personaId = 200L;

//        //    var result = await _productController.ListProductUsers(productId, companyInstanceId, personaId);

//        //    var okResult = Assert.IsType<OkObjectResult>(result);
//        //    var output = Assert.IsType<ObjectListOutput<ProductUsers, IErrorData>>(okResult.Value);
//        //    Assert.False(output.Status.Success);
//        //    Assert.Equal("Product.ListProductUsers.2", output.Status.ErrorCode);
//        //}

//        //[Fact]
//        //public async Task ListProductUsers_WithZeroCompanyInstanceId_ReturnsError()
//        //{
//        //    var productId = (int)ProductEnum.OneSite;
//        //    var companyInstanceId = 0L;
//        //    var personaId = 200L;

//        //    var result = await _productController.ListProductUsers(productId, companyInstanceId, personaId);

//        //    var okResult = Assert.IsType<OkObjectResult>(result);
//        //    var output = Assert.IsType<ObjectListOutput<ProductUsers, IErrorData>>(okResult.Value);
//        //    Assert.False(output.Status.Success);
//        //    Assert.Equal("Product.ListProductUsers.3", output.Status.ErrorCode);
//        //}

//        //[Fact]
//        //public async Task ListProductUsers_WithNegativeCompanyInstanceId_ReturnsError()
//        //{
//        //    var productId = (int)ProductEnum.OneSite;
//        //    var companyInstanceId = -2L;
//        //    var personaId = 200L;

//        //    var result = await _productController.ListProductUsers(productId, companyInstanceId, personaId);

//        //    var okResult = Assert.IsType<OkObjectResult>(result);
//        //    var output = Assert.IsType<ObjectListOutput<ProductUsers, IErrorData>>(okResult.Value);
//        //    Assert.False(output.Status.Success);
//        //    Assert.Equal("Product.ListProductUsers.3", output.Status.ErrorCode);
//        //}

//        //[Fact]
//        //public async Task ListProductUsers_WithNegativePersonaId_ReturnsError()
//        //{
//        //    var productId = (int)ProductEnum.OneSite;
//        //    var companyInstanceId = 100L;
//        //    var personaId = -1L;

//        //    var result = await _productController.ListProductUsers(productId, companyInstanceId, personaId);

//        //    var okResult = Assert.IsType<OkObjectResult>(result);
//        //    var output = Assert.IsType<ObjectListOutput<ProductUsers, IErrorData>>(okResult.Value);
//        //    Assert.False(output.Status.Success);
//        //    Assert.Equal("Product.ListProductUsers.4", output.Status.ErrorCode);
//        //}

//        [Fact]
//        public async Task ListProductUsers_WithEmptyUserRealPageGuidAndEmptyClientCode_ReturnsError()
//        {
//            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
//            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);
//            mockUserClaimsAccessor.Setup(x => x.ClientCode).Returns(string.Empty);

//            var controller = new ProductController(
//                _mockManageProduct.Object,
//                _mockUserLoginRepository.Object,
//                _mockSamlRepository.Object,
//                _mockProductRepository.Object,
//                _mockManageBlueBook.Object,
//                _mockManagePersona.Object,
//                mockUserClaimsAccessor.Object,
//                MockMemoryCache.Object,
//                _mockHttpClientFactory.Object,
//                _mockLogger.Object)
//            {
//                ControllerContext = CreateControllerContext()
//            };

//            var result = await controller.ListProductUsers((int)ProductEnum.OneSite, 100L, 200L);

//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var output = Assert.IsType<ObjectListOutput<ProductUsers, IErrorData>>(okResult.Value);
//            Assert.False(output.Status.Success);
//            Assert.Equal("Product.ListProductUsers.1", output.Status.ErrorCode);
//        }

//        [Fact]
//        public async Task ListProductUsers_WithBBAClientCode_ReturnsProductUsers()
//        {
//            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
//            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);
//            mockUserClaimsAccessor.Setup(x => x.ClientCode).Returns("BBA");

//            _mockManageProduct
//                .Setup(x => x.GetProductUsers(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>()))
//                .Returns(new List<ProductUsers> { new ProductUsers() });

//            var controller = new ProductController(
//                _mockManageProduct.Object,
//                _mockUserLoginRepository.Object,
//                _mockSamlRepository.Object,
//                _mockProductRepository.Object,
//                _mockManageBlueBook.Object,
//                _mockManagePersona.Object,
//                mockUserClaimsAccessor.Object,
//                MockMemoryCache.Object,
//                _mockHttpClientFactory.Object,
//                _mockLogger.Object)
//            {
//                ControllerContext = CreateControllerContext()
//            };

//            var result = await controller.ListProductUsers((int)ProductEnum.OneSite, 100L, 200L);

//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var output = Assert.IsType<ObjectListOutput<ProductUsers, IErrorData>>(okResult.Value);
//            Assert.True(output.Status.Success);
//        }

//        //[Fact]
//        //public async Task ListProductUsers_WhenServiceReturnsNull_ReturnsError()
//        //{
//        //    var productId = (int)ProductEnum.OneSite;

//        //    _mockManageProduct
//        //        .Setup(x => x.GetProductUsers(productId, It.IsAny<long>(), It.IsAny<long>()))
//        //        .Returns((IList<ProductUsers>)null!);

//        //    var result = await _productController.ListProductUsers(productId, 100L, 200L);

//        //    var okResult = Assert.IsType<OkObjectResult>(result);
//        //    var output = Assert.IsType<ObjectListOutput<ProductUsers, IErrorData>>(okResult.Value);
//        //    Assert.False(output.Status.Success);
//        //    Assert.Equal("Product.ListProductUsers.5", output.Status.ErrorCode);
//        //}

//        //[Fact]
//        //public async Task ListProductUsers_WithMinusOneCompanyInstanceId_ReturnsProductUsers()
//        //{
//        //    var productId = (int)ProductEnum.OneSite;
//        //    var companyInstanceId = -1L;

//        //    _mockManageProduct
//        //        .Setup(x => x.GetProductUsers(productId, companyInstanceId, It.IsAny<long>()))
//        //        .Returns(new List<ProductUsers> { new ProductUsers() });

//        //    var result = await _productController.ListProductUsers(productId, companyInstanceId, 0L);

//        //    var okResult = Assert.IsType<OkObjectResult>(result);
//        //    var output = Assert.IsType<ObjectListOutput<ProductUsers, IErrorData>>(okResult.Value);
//        //    Assert.True(output.Status.Success);
//        //}

//        #endregion

//        #region GetProductFamilies Tests

//        [Fact]
//        public async Task GetProductFamilies_WithValidUser_ReturnsProductFamilies()
//        {
//            _mockManageProduct
//                .Setup(x => x.GetProductFamilies(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>()))
//                .Returns(new List<ProductFamily> { new ProductFamily() });

//            var result = await _productController.GetProductFamilies();

//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var output = Assert.IsType<ObjectListOutput<ProductFamily, IErrorData>>(okResult.Value);
//            Assert.True(output.Status.Success);
//        }

//        [Fact]
//        public async Task GetProductFamilies_WithNullResult_ReturnsError()
//        {
//            _mockManageProduct
//                .Setup(x => x.GetProductFamilies(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>()))
//                .Returns((IList<ProductFamily>)null!);

//            var result = await _productController.GetProductFamilies();

//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var output = Assert.IsType<ObjectListOutput<ProductFamily, IErrorData>>(okResult.Value);
//            Assert.False(output.Status.Success);
//            Assert.Equal("Product.GetProductFamilies.1", output.Status.ErrorCode);
//        }

//        [Fact]
//        public async Task GetProductFamilies_WithPersonRealPageId_UsesProvidedId()
//        {
//            var personRealPageId = Guid.NewGuid();
//            _mockManageProduct
//                .Setup(x => x.GetProductFamilies(It.IsAny<Guid>(), It.IsAny<Guid>(), personRealPageId, It.IsAny<string>(), It.IsAny<string>()))
//                .Returns(new List<ProductFamily> { new ProductFamily() });

//            var result = await _productController.GetProductFamilies(personRealPageId);

//            Assert.IsType<OkObjectResult>(result);
//        }

//        [Fact]
//        public async Task GetProductFamilies_WithEmptyPersonRealPageId_UsesUserClaimId()
//        {
//            _mockManageProduct
//                .Setup(x => x.GetProductFamilies(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>()))
//                .Returns(new List<ProductFamily> { new ProductFamily() });

//            var result = await _productController.GetProductFamilies(Guid.Empty);

//            Assert.IsType<OkObjectResult>(result);
//        }

//        [Fact]
//        public async Task GetProductFamilies_WithAccessFilter_PassesToService()
//        {
//            _mockManageProduct
//                .Setup(x => x.GetProductFamilies(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), "rolesandrights", It.IsAny<string>()))
//                .Returns(new List<ProductFamily> { new ProductFamily() });

//            var result = await _productController.GetProductFamilies(null, "rolesandrights");

//            Assert.IsType<OkObjectResult>(result);
//        }

//        [Fact]
//        public async Task GetProductFamilies_WithLoginName_PassesToService()
//        {
//            _mockManageProduct
//                .Setup(x => x.GetProductFamilies(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>(), "user@test.com"))
//                .Returns(new List<ProductFamily> { new ProductFamily() });

//            var result = await _productController.GetProductFamilies(null, null, "user@test.com");

//            Assert.IsType<OkObjectResult>(result);
//        }

//        #endregion

//        #region GetProductInternalSettings Tests

//        [Fact]
//        public async Task GetProductInternalSettings_WithValidKey_ReturnsSettings()
//        {
//            var productId = (int)ProductEnum.OneSite;
//            var validKey = new Guid("4AD12A31-680A-476F-863E-26749D2E7DD4");

//            _mockManageProduct
//                .Setup(x => x.GetProductInternalSettings(productId))
//                .Returns(new List<ProductInternalSetting> { new ProductInternalSetting { Name = "Test", Value = "Value" } });

//            var result = await _productController.GetProductInternalSettings(productId, validKey);

//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var settings = Assert.IsType<List<ProductInternalSetting>>(okResult.Value);
//            Assert.NotEmpty(settings);
//        }

//        [Fact]
//        public async Task GetProductInternalSettings_WithInvalidKey_ReturnsEmptyList()
//        {
//            var productId = (int)ProductEnum.OneSite;
//            var invalidKey = Guid.NewGuid();

//            var result = await _productController.GetProductInternalSettings(productId, invalidKey);

//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var settings = Assert.IsType<List<ProductInternalSetting>>(okResult.Value);
//            Assert.Empty(settings);
//        }

//        #endregion

//        #region GetProductNonSensitiveSettings Tests

//        [Fact]
//        public async Task GetProductNonSensitiveSettings_WithValidProductId_ReturnsNonSensitiveSettings()
//        {
//            var productId = (int)ProductEnum.OneSite;
//            var settings = new List<ProductInternalSetting>
//            {
//                new ProductInternalSetting { Name = "PublicSetting", Value = "Value", SensitiveData = false },
//                new ProductInternalSetting { Name = "SecretSetting", Value = "Secret", SensitiveData = true }
//            };

//            _mockManageProduct
//                .Setup(x => x.GetProductInternalSettings(productId))
//                .Returns(settings);

//            var result = await _productController.GetProductNonSensitiveSettings(productId);

//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var resultSettings = okResult.Value as List<ProductInternalSetting>;
//            Assert.NotNull(resultSettings);
//            Assert.Single(resultSettings);
//            Assert.Equal("PublicSetting", resultSettings[0].Name);
//        }

//        [Fact]
//        public async Task GetProductNonSensitiveSettings_WhenNoSettings_ReturnsEmptyList()
//        {
//            var productId = (int)ProductEnum.OneSite;

//            _mockManageProduct
//                .Setup(x => x.GetProductInternalSettings(productId))
//                .Returns((List<ProductInternalSetting>)null!);

//            var result = await _productController.GetProductNonSensitiveSettings(productId);

//            var okResult = Assert.IsType<OkObjectResult>(result);
//            Assert.Null(okResult.Value);
//        }

//        #endregion

//        #region GetAllProductNonSensitiveSettingsByType Tests

//        [Fact]
//        public async Task GetAllProductNonSensitiveSettingsByType_WithValidType_ReturnsSettings()
//        {
//            var productSettingType = "ProductIcon";
//            var settings = new List<ProductInternalSettingByType>
//            {
//                new ProductInternalSettingByType { ProductName = "Product1", SensitiveData = false },
//                new ProductInternalSettingByType { ProductName = "Product2", SensitiveData = true }
//            };

//            _mockManageProduct
//                .Setup(x => x.GetProductSettingByType(productSettingType, null))
//                .Returns(settings);

//            var result = await _productController.GetAllProductNonSensitiveSettingsByType(productSettingType);

//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var output = Assert.IsType<ObjectListOutput<ProductInternalSettingByType, IErrorData>>(okResult.Value);
//            Assert.Single(output.list);
//        }

//        [Fact]
//        public async Task GetAllProductNonSensitiveSettingsByType_WithOrgType_PassesToService()
//        {
//            var productSettingType = "ProductIcon";
//            var orgType = "PMC";

//            _mockManageProduct
//                .Setup(x => x.GetProductSettingByType(productSettingType, orgType))
//                .Returns(new List<ProductInternalSettingByType>());

//            var result = await _productController.GetAllProductNonSensitiveSettingsByType(productSettingType, orgType);

//            Assert.IsType<OkObjectResult>(result);
//            _mockManageProduct.Verify(x => x.GetProductSettingByType(productSettingType, orgType), Times.Once);
//        }

//        [Fact]
//        public async Task GetAllProductNonSensitiveSettingsByType_WhenNullResult_ReturnsEmptyList()
//        {
//            _mockManageProduct
//                .Setup(x => x.GetProductSettingByType(It.IsAny<string>(), It.IsAny<string>()))
//                .Returns((List<ProductInternalSettingByType>)null!);

//            var result = await _productController.GetAllProductNonSensitiveSettingsByType("TestType");

//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var output = Assert.IsType<ObjectListOutput<ProductInternalSettingByType, IErrorData>>(okResult.Value);
//            Assert.Equal(0, output.pagingSummary.TotalRecords);
//        }

//        #endregion

//        #region UpdateProductSettingAndLinkToConfiguration Tests

//        [Fact]
//        public async Task UpdateProductSettingAndLinkToConfiguration_WithValidSettings_ReturnsOk()
//        {
//            var productId = (int)ProductEnum.OneSite;
//            var setting = new ProductInternalSetting { Name = "TestSetting", Value = "TestValue" };

//            _mockManageProduct
//                .Setup(x => x.CreateProductSettingAndLinkToConfiguration(productId, setting))
//                .Returns(new RepositoryResponse { Id = 1 });

//            var result = await _productController.UpdateProductSettingAndLinkToConfiguration(productId, setting);

//            Assert.IsType<OkResult>(result);
//        }

//        [Fact]
//        public async Task UpdateProductSettingAndLinkToConfiguration_WithError_ReturnsBadRequest()
//        {
//            var productId = (int)ProductEnum.OneSite;
//            var setting = new ProductInternalSetting { Name = "TestSetting", Value = "TestValue" };

//            _mockManageProduct
//                .Setup(x => x.CreateProductSettingAndLinkToConfiguration(productId, setting))
//                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = "Update failed" });

//            var result = await _productController.UpdateProductSettingAndLinkToConfiguration(productId, setting);

//            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
//            Assert.Equal("Update failed", badRequestResult.Value);
//        }

//        #endregion

//        #region ListProductSettingType Tests

//        [Fact]
//        public async Task ListProductSettingType_ReturnsSettingTypes()
//        {
//            var settingTypes = new List<ProductSettingType>
//            {
//                new ProductSettingType { ProductSettingTypeId = 1, Name = "Type1" },
//                new ProductSettingType { ProductSettingTypeId = 2, Name = "Type2" }
//            };

//            _mockManageProduct
//                .Setup(x => x.ListProductSettingType())
//                .Returns(settingTypes);

//            var result = await _productController.ListProductSettingType();

//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var resultTypes = Assert.IsType<List<ProductSettingType>>(okResult.Value);
//            Assert.Equal(2, resultTypes.Count);
//        }

//        #endregion

//        #region GetProductLoginInfo Tests

//        [Fact]
//        public void GetProductLoginInfo_ReturnsNull()
//        {
//            var productId = (int)ProductEnum.OneSite;
//            var personaId = 100L;

//            var result = _productController.GetProductLoginInfo(productId, personaId);

//            var okResult = Assert.IsType<OkObjectResult>(result);
//            Assert.Null(okResult.Value);
//        }

//        #endregion

//        #region GetProductTypes Tests

//        [Fact]
//        public async Task GetProductTypes_ReturnsProductTypes()
//        {
//            var productTypes = new List<ProductType>
//            {
//                new ProductType { ProductTypeId = 1, Name = "Type1" },
//                new ProductType { ProductTypeId = 2, Name = "Type2" }
//            };

//            _mockManageProduct
//                .Setup(x => x.GetProductTypes())
//                .Returns(productTypes);

//            var result = await _productController.GetProductTypes();

//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var output = Assert.IsType<ObjectListOutput<ProductType, IErrorData>>(okResult.Value);
//            Assert.Equal(2, output.list.Count);
//        }

//        #endregion

//        #region GetBooksProductMap Tests

//        [Fact]
//        public async Task GetBooksProductMap_ReturnsProductMap()
//        {
//            var productMap = new List<GbProductMap>
//            {
//                new GbProductMap { ProductId = 1, Name = "Product1" },
//                new GbProductMap { ProductId = 2, Name = "Product2" }
//            };

//            _mockManageProduct
//                .Setup(x => x.ListProducts())
//                .Returns(productMap);

//            var result = await _productController.GetBooksProductMap();

//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var resultMap = Assert.IsType<List<GbProductMap>>(okResult.Value);
//            Assert.Equal(2, resultMap.Count);
//        }

//        #endregion

//        #region GetUDMSourceList Tests

//        [Fact]
//        public async Task GetUDMSourceList_ReturnsUDMSources()
//        {
//            var udmSources = new List<UDMSource>
//            {
//                new UDMSource { Id = "1", Description = "Source1" },
//                new UDMSource { Id = "2", Description = "Source2" }
//            };

//            _mockManageBlueBook
//                .Setup(x => x.GetUDMSourceList())
//                .Returns(udmSources);

//            var result = await _productController.GetUDMSourceList();

//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var resultSources = Assert.IsAssignableFrom<IEnumerable<UDMSource>>(okResult.Value);
//            Assert.Equal(2, resultSources.Count());
//        }

//        #endregion

//        #region GetUDMOperators Tests

//        [Fact]
//        public async Task GetUDMOperators_ReturnsOperators()
//        {
//            var operators = new List<UPFMOperators>
//            {
//                new UPFMOperators { CompanyGuid = Guid.NewGuid(), CompanyName = "Op1" },
//                new UPFMOperators { CompanyGuid = Guid.NewGuid(), CompanyName = "Op2" }
//            };

//            _mockManageBlueBook
//                .Setup(x => x.GetOperatorListForUPFMCompany(It.IsAny<Guid>(), "UPFM"))
//                .Returns(operators);

//            var result = await _productController.GetUDMOperators();

//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var resultOperators = Assert.IsAssignableFrom<IEnumerable<UPFMOperators>>(okResult.Value);
//            Assert.Equal(2, resultOperators.Count());
//        }

//        #endregion

//        #region Dispose

//        public override void Dispose()
//        {
//            _productController = null!;
//            base.Dispose();
//        }

//        #endregion
//    }
//}
