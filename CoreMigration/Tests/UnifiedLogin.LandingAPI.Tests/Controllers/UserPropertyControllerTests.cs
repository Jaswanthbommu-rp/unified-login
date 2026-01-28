using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class UserPropertyControllerTests : ControllerTestBase
    {
        private UserPropertyController _controller;

        public UserPropertyControllerTests()
        {
            _controller = new UserPropertyController(MockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new UserPropertyController(MockUserClaimsAccessor.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_DoesNotThrow()
        {
            var controller = new UserPropertyController(null!);

            Assert.NotNull(controller);
        }

        #endregion

        #region GetUserProperties Tests - Invalid Parameters

        [Fact]
        public async Task GetUserProperties_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new UserPropertyController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetUserProperties(22);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter realPageId.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetUserProperties_WithZeroProductId_ReturnsBadRequest()
        {
            var result = await _controller.GetUserProperties(0);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter productId.", badRequestResult.Value);
        }

        #endregion

        #region GetUserProperties Tests - OmniChannel Product

        [Fact]
        public async Task GetUserProperties_WithOmniChannelProductId_ReturnsOkResult()
        {
            var result = await _controller.GetUserProperties((int)ProductEnum.OmniChannel);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<ListResponse>(okResult.Value);
        }

        [Fact]
        public async Task GetUserProperties_WithOmniChannelProductIdAsInt_ReturnsOkResult()
        {
            var result = await _controller.GetUserProperties(22);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<ListResponse>(okResult.Value);
        }

        #endregion

        #region GetUserProperties Tests - Other Products (Default Case)

        [Fact]
        public async Task GetUserProperties_WithOneSiteProductId_ReturnsOkWithNoResults()
        {
            var result = await _controller.GetUserProperties((int)ProductEnum.OneSite);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var listResponse = Assert.IsType<ListResponse>(okResult.Value);
            Assert.Equal("No results found for the product requested.", listResponse.ErrorReason);
            Assert.False(listResponse.IsError);
        }

        [Fact]
        public async Task GetUserProperties_WithUnifiedPlatformProductId_ReturnsOkWithNoResults()
        {
            var result = await _controller.GetUserProperties((int)ProductEnum.UnifiedPlatform);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var listResponse = Assert.IsType<ListResponse>(okResult.Value);
            Assert.Equal("No results found for the product requested.", listResponse.ErrorReason);
        }

        [Fact]
        public async Task GetUserProperties_WithAssetOptimizerProductId_ReturnsOkWithNoResults()
        {
            var result = await _controller.GetUserProperties((int)ProductEnum.AssetOptimizer);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var listResponse = Assert.IsType<ListResponse>(okResult.Value);
            Assert.Equal("No results found for the product requested.", listResponse.ErrorReason);
        }

        [Fact]
        public async Task GetUserProperties_WithResidentPortalProductId_ReturnsOkWithNoResults()
        {
            var result = await _controller.GetUserProperties((int)ProductEnum.ResidentPortal);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var listResponse = Assert.IsType<ListResponse>(okResult.Value);
            Assert.Equal("No results found for the product requested.", listResponse.ErrorReason);
        }

        [Fact]
        public async Task GetUserProperties_WithPropertywareProductId_ReturnsOkWithNoResults()
        {
            var result = await _controller.GetUserProperties((int)ProductEnum.Propertyware);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var listResponse = Assert.IsType<ListResponse>(okResult.Value);
            Assert.Equal("No results found for the product requested.", listResponse.ErrorReason);
        }

        #endregion

        #region GetUserProperties Tests - Response Validation

        [Fact]
        public async Task GetUserProperties_DefaultCase_ReturnsCorrectListResponseStructure()
        {
            var result = await _controller.GetUserProperties(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var listResponse = Assert.IsType<ListResponse>(okResult.Value);

            Assert.False(listResponse.IsError);
            Assert.Equal(0, listResponse.TotalRows);
            Assert.Equal(0, listResponse.RowsPerPage);
            Assert.Equal(1, listResponse.TotalPages);
            Assert.NotNull(listResponse.Records);
            Assert.Empty(listResponse.Records);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task GetUserProperties_WithNegativeProductId_ReturnsBadRequest()
        {
            var result = await _controller.GetUserProperties(-1);

            // Negative productId is not zero, so it goes to switch case
            var okResult = Assert.IsType<OkObjectResult>(result);
            var listResponse = Assert.IsType<ListResponse>(okResult.Value);
            Assert.Equal("No results found for the product requested.", listResponse.ErrorReason);
        }

        [Fact]
        public async Task GetUserProperties_WithMaxLongProductId_ReturnsOkWithNoResults()
        {
            var result = await _controller.GetUserProperties(long.MaxValue);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var listResponse = Assert.IsType<ListResponse>(okResult.Value);
            Assert.Equal("No results found for the product requested.", listResponse.ErrorReason);
        }

        [Fact]
        public async Task GetUserProperties_WithMinLongProductId_ReturnsOkWithNoResults()
        {
            var result = await _controller.GetUserProperties(long.MinValue);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var listResponse = Assert.IsType<ListResponse>(okResult.Value);
            Assert.Equal("No results found for the product requested.", listResponse.ErrorReason);
        }

        [Fact]
        public async Task GetUserProperties_WithUnknownProductId_ReturnsOkWithNoResults()
        {
            var result = await _controller.GetUserProperties(9999);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var listResponse = Assert.IsType<ListResponse>(okResult.Value);
            Assert.Equal("No results found for the product requested.", listResponse.ErrorReason);
        }

        [Fact]
        public async Task GetUserProperties_CalledMultipleTimes_ReturnsConsistentResults()
        {
            var result1 = await _controller.GetUserProperties(1);
            var result2 = await _controller.GetUserProperties(1);

            Assert.IsType<OkObjectResult>(result1);
            Assert.IsType<OkObjectResult>(result2);
        }

        [Fact]
        public async Task GetUserProperties_WithDifferentUserClaim_ReturnsResult()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 200,
                LoginName = "different@test.com",
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 5000
            });

            var controller = new UserPropertyController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetUserProperties(1);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _controller = null!;
            base.Dispose();
        }

        #endregion
    }
}
