using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class ProductUserControllerTests : ControllerTestBase
    {
        private ProductUserController _controller;

        public ProductUserControllerTests()
        {
            _controller = new ProductUserController(MockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new ProductUserController(MockUserClaimsAccessor.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_DoesNotThrow()
        {
            var controller = new ProductUserController(null!);

            Assert.NotNull(controller);
        }

        #endregion

        #region CreateProductUser Tests

        [Fact]
        public async Task CreateProductUser_WithNullProductUser_ReturnsBadRequest()
        {
            var result = await _controller.CreateProductUser(null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("productUser null.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateProductUser_WithEmptyRealPageId_ReturnsBadRequest()
        {
            var productUser = new ProductUserProperitiesRoles
            {
                RealPageId = Guid.Empty,
                ProductId = 1,
                CreateUserPersonaId = 100,
                AssignUserPersonaId = 200
            };

            var result = await _controller.CreateProductUser(productUser);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        //[Fact]
        //public async Task CreateProductUser_WithValidProductUser_ReturnsCreated()
        //{
        //    var productUser = new ProductUserProperitiesRoles
        //    {
        //        RealPageId = Guid.NewGuid(),
        //        ProductId = 1,
        //        CreateUserPersonaId = 100,
        //        AssignUserPersonaId = 200,
        //        BatchProcessType = BatchProcessType.CreateUpdateProductUser
        //    };

        //    var result = await _controller.CreateProductUser(productUser);

        //    var createdResult = Assert.IsType<CreatedResult>(result);
        //    Assert.NotNull(createdResult.Value);
        //}

        [Fact]
        public async Task CreateProductUser_WithAllProperties_ReturnsCreated()
        {
            var productUser = new ProductUserProperitiesRoles
            {
                RealPageId = Guid.NewGuid(),
                ProductId = 5,
                ProductBatchId = 1,
                CreateUserPersonaId = 100,
                AssignUserPersonaId = 200,
                InputJson = "{}",
                BatchProcessType = BatchProcessType.CreateUpdateProductUser,
                CorrelationId = Guid.NewGuid(),
                BatchProcessorGroupId = 1,
                CreateRealPageEmployee = false,
                RealPageEmployeePersonaId = 0,
                ImpersonatorUserId = 0
            };

            var result = await _controller.CreateProductUser(productUser);

            Assert.IsType<CreatedResult>(result);
        }

        #endregion

        #region UpdateProductUserAccountDetails Tests

        [Fact]
        public async Task UpdateProductUserAccountDetails_WithNullProductUser_ReturnsBadRequest()
        {
            var result = await _controller.UpdateProductUserAccountDetails(null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("productUser null.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateProductUserAccountDetails_WithZeroProductId_ReturnsBadRequest()
        {
            var productUser = new ProductUserAccountDetails
            {
                ProductId = 0,
                PersonaId = 100
            };

            var result = await _controller.UpdateProductUserAccountDetails(productUser);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ProductName empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateProductUserAccountDetails_WithNegativeProductId_ReturnsBadRequest()
        {
            var productUser = new ProductUserAccountDetails
            {
                ProductId = -1,
                PersonaId = 100
            };

            var result = await _controller.UpdateProductUserAccountDetails(productUser);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ProductName empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateProductUserAccountDetails_WithValidProductUser_ReturnsOkResult()
        {
            var productUser = new ProductUserAccountDetails
            {
                ProductId = 5,
                PersonaId = 100,
                EmployeeId = "EMP001"
            };

            var result = await _controller.UpdateProductUserAccountDetails(productUser);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateProductUserAccountDetails_WithAllProperties_ReturnsOkResult()
        {
            var productUser = new ProductUserAccountDetails
            {
                ProductId = 5,
                PersonaId = 100,
                ProductStatus = ProductBatchStatusType.Running,
                ProductSettings = new Dictionary<SamlAttributeEnum, string>(),
                SubProducts = new List<string> { "SubProduct1" },
                EmployeeId = "EMP001",
                Origin = "Test"
            };

            var result = await _controller.UpdateProductUserAccountDetails(productUser);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region DeleteSamlUserProductInfoAndStatus Tests

        [Fact]
        public async Task DeleteSamlUserProductInfoAndStatus_WithNullProductUser_ReturnsBadRequest()
        {
            var result = await _controller.DeleteSamlUserProductInfoAndStatus(null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("productUser null.", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteSamlUserProductInfoAndStatus_WithZeroProductId_ReturnsBadRequest()
        {
            var productUser = new ProductUserAccountDetails
            {
                ProductId = 0,
                PersonaId = 100
            };

            var result = await _controller.DeleteSamlUserProductInfoAndStatus(productUser);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ProductName empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteSamlUserProductInfoAndStatus_WithNegativeProductId_ReturnsBadRequest()
        {
            var productUser = new ProductUserAccountDetails
            {
                ProductId = -1,
                PersonaId = 100
            };

            var result = await _controller.DeleteSamlUserProductInfoAndStatus(productUser);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ProductName empty.", badRequestResult.Value);
        }

        //[Fact]
        //public async Task DeleteSamlUserProductInfoAndStatus_WithValidProductUser_ReturnsOkResult()
        //{
        //    var productUser = new ProductUserAccountDetails
        //    {
        //        ProductId = 5,
        //        PersonaId = 100,
        //        EmployeeId = "EMP001"
        //    };

        //    var result = await _controller.DeleteSamlUserProductInfoAndStatus(productUser);

        //    Assert.IsType<OkObjectResult>(result);
        //}

        //[Fact]
        //public async Task DeleteSamlUserProductInfoAndStatus_WithAllProperties_ReturnsOkResult()
        //{
        //    var productUser = new ProductUserAccountDetails
        //    {
        //        ProductId = 5,
        //        PersonaId = 100,
        //        ProductStatus = ProductBatchStatusType.Success,
        //        ProductSettings = new Dictionary<SamlAttributeEnum, string>(),
        //        SubProducts = new List<string> { "SubProduct1", "SubProduct2" },
        //        EmployeeId = "EMP001",
        //        Origin = "Test"
        //    };

        //    var result = await _controller.DeleteSamlUserProductInfoAndStatus(productUser);

        //    Assert.IsType<OkObjectResult>(result);
        //}

        #endregion

        #region GetProductStatuses Tests

        [Fact]
        public async Task GetProductStatuses_WithZeroAssignUserPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetProductStatuses(0);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("assignUserPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProductStatuses_WithNullUserClaim_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new ProductUserController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetProductStatuses(100);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProductStatuses_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new ProductUserController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetProductStatuses(100);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProductStatuses_WithValidAssignUserPersonaId_ReturnsOkResult()
        {
            var result = await _controller.GetProductStatuses(100);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetProductStatuses_WithMaxLongValue_ReturnsOkResult()
        {
            var result = await _controller.GetProductStatuses(long.MaxValue);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetProductStatuses_WithNegativeAssignUserPersonaId_ReturnsOkResult()
        {
            // Negative values are not zero, so they pass the first check
            var result = await _controller.GetProductStatuses(-1);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Edge Cases

        //[Fact]
        //public async Task CreateProductUser_WithMaxLongPersonaIds_ReturnsCreated()
        //{
        //    var productUser = new ProductUserProperitiesRoles
        //    {
        //        RealPageId = Guid.NewGuid(),
        //        ProductId = 1,
        //        CreateUserPersonaId = long.MaxValue,
        //        AssignUserPersonaId = long.MaxValue
        //    };

        //    var result = await _controller.CreateProductUser(productUser);

        //    Assert.IsType<CreatedResult>(result);
        //}

        //[Fact]
        //public async Task CreateProductUser_WithEmptyInputJson_ReturnsCreated()
        //{
        //    var productUser = new ProductUserProperitiesRoles
        //    {
        //        RealPageId = Guid.NewGuid(),
        //        ProductId = 1,
        //        CreateUserPersonaId = 100,
        //        AssignUserPersonaId = 200,
        //        InputJson = ""
        //    };

        //    var result = await _controller.CreateProductUser(productUser);

        //    Assert.IsType<CreatedResult>(result);
        //}

        //[Fact]
        //public async Task CreateProductUser_WithNullInputJson_ReturnsCreated()
        //{
        //    var productUser = new ProductUserProperitiesRoles
        //    {
        //        RealPageId = Guid.NewGuid(),
        //        ProductId = 1,
        //        CreateUserPersonaId = 100,
        //        AssignUserPersonaId = 200,
        //        InputJson = null
        //    };

        //    var result = await _controller.CreateProductUser(productUser);

        //    Assert.IsType<CreatedResult>(result);
        //}

        [Fact]
        public async Task UpdateProductUserAccountDetails_WithEmptySubProducts_ReturnsOkResult()
        {
            var productUser = new ProductUserAccountDetails
            {
                ProductId = 5,
                PersonaId = 100,
                SubProducts = new List<string>()
            };

            var result = await _controller.UpdateProductUserAccountDetails(productUser);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateProductUserAccountDetails_WithNullSubProducts_ReturnsOkResult()
        {
            var productUser = new ProductUserAccountDetails
            {
                ProductId = 5,
                PersonaId = 100,
                SubProducts = null
            };

            var result = await _controller.UpdateProductUserAccountDetails(productUser);

            Assert.IsType<OkObjectResult>(result);
        }

        //[Fact]
        //public async Task DeleteSamlUserProductInfoAndStatus_WithEmptyProductSettings_ReturnsOkResult()
        //{
        //    var productUser = new ProductUserAccountDetails
        //    {
        //        ProductId = 5,
        //        PersonaId = 100,
        //        ProductSettings = new Dictionary<SamlAttributeEnum, string>()
        //    };

        //    var result = await _controller.DeleteSamlUserProductInfoAndStatus(productUser);

        //    Assert.IsType<OkObjectResult>(result);
        //}

        //[Fact]
        //public async Task DeleteSamlUserProductInfoAndStatus_WithNullProductSettings_ReturnsOkResult()
        //{
        //    var productUser = new ProductUserAccountDetails
        //    {
        //        ProductId = 5,
        //        PersonaId = 100,
        //        ProductSettings = null
        //    };

        //    var result = await _controller.DeleteSamlUserProductInfoAndStatus(productUser);

        //    Assert.IsType<OkObjectResult>(result);
        //}

        //[Fact]
        //public async Task CreateProductUser_WithRealPageEmployee_ReturnsCreated()
        //{
        //    var productUser = new ProductUserProperitiesRoles
        //    {
        //        RealPageId = Guid.NewGuid(),
        //        ProductId = 1,
        //        CreateUserPersonaId = 100,
        //        AssignUserPersonaId = 200,
        //        CreateRealPageEmployee = true,
        //        RealPageEmployeePersonaId = 300
        //    };

        //    var result = await _controller.CreateProductUser(productUser);

        //    Assert.IsType<CreatedResult>(result);
        //}

        //[Fact]
        //public async Task CreateProductUser_WithImpersonator_ReturnsCreated()
        //{
        //    var productUser = new ProductUserProperitiesRoles
        //    {
        //        RealPageId = Guid.NewGuid(),
        //        ProductId = 1,
        //        CreateUserPersonaId = 100,
        //        AssignUserPersonaId = 200,
        //        ImpersonatorUserId = 500
        //    };

        //    var result = await _controller.CreateProductUser(productUser);

        //    Assert.IsType<CreatedResult>(result);
        //}

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
