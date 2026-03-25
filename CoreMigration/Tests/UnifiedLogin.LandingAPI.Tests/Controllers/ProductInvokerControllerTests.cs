using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Types;
using System.Threading;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class ProductInvokerControllerTests : ControllerTestBase
    {
        private readonly Mock<IIntegrationTypeFactory> _mockIntegrationTypeFactory;
        private readonly Mock<IProductRepositoryAsync> _mockProductRepository;
        private readonly Mock<IIntegrationType> _mockIntegrationType;
        private ProductInvokerController _productInvokerController;

        public ProductInvokerControllerTests()
        {
            _mockIntegrationTypeFactory = new Mock<IIntegrationTypeFactory>();
            _mockProductRepository = new Mock<IProductRepositoryAsync>();
            _mockIntegrationType = new Mock<IIntegrationType>();

            _mockIntegrationTypeFactory
                .Setup(x => x.GetIntegration(It.IsAny<int>()))
                .Returns(_mockIntegrationType.Object);

            _productInvokerController = new ProductInvokerController(
                MockUserClaimsAccessor.Object,
                _mockIntegrationTypeFactory.Object,
                _mockProductRepository.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            Assert.NotNull(_productInvokerController);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductInvokerController(
                null!,
                _mockIntegrationTypeFactory.Object,
                _mockProductRepository.Object));
        }

        [Fact]
        public void Constructor_WithNullIntegrationTypeFactory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductInvokerController(
                MockUserClaimsAccessor.Object,
                null!,
                _mockProductRepository.Object));
        }

        [Fact]
        public void Constructor_WithNullProductRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductInvokerController(
                MockUserClaimsAccessor.Object,
                _mockIntegrationTypeFactory.Object,
                null!));
        }

        #endregion

        #region GetRoles Tests

        [Fact]
        public async Task GetRoles_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var dataFilter = new RequestParameter();
            var result = await _productInvokerController.GetRoles(ProductEnum.OneSite, 0, 100, dataFilter);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRoles_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductInvokerController(
                mockUserClaimsAccessor.Object,
                _mockIntegrationTypeFactory.Object,
                _mockProductRepository.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetRoles(ProductEnum.OneSite, 100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRoles_WithValidParameters_ReturnsOkResult()
        {
            var listResponse = new ListResponse { IsError = false, Records = new List<object>() };
            _mockIntegrationType
                .Setup(x => x.GetRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<AccessType?>(), It.IsAny<RequestParameter>()))
                .Returns(listResponse);

            var result = await _productInvokerController.GetRoles(ProductEnum.OneSite, 100, 200, new RequestParameter());

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(listResponse, okResult.Value);
        }

        [Fact]
        public async Task GetRoles_WhenResultIsError_ReturnsForbidden()
        {
            var listResponse = new ListResponse { IsError = true, ErrorReason = "Access denied" };
            _mockIntegrationType
                .Setup(x => x.GetRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<AccessType?>(), It.IsAny<RequestParameter>()))
                .Returns(listResponse);

            var result = await _productInvokerController.GetRoles(ProductEnum.OneSite, 100, 200, new RequestParameter());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetRoles_WhenBlueBookExceptionThrown_ReturnsForbiddenWithMessage()
        {
            _mockIntegrationType
                .Setup(x => x.GetRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<AccessType?>(), It.IsAny<RequestParameter>()))
                .Throws(new Exception("Outer", new BlueBookException("BlueBook error")));

            var result = await _productInvokerController.GetRoles(ProductEnum.OneSite, 100, 200, new RequestParameter());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, objectResult.StatusCode);
            var response = Assert.IsType<ListResponse>(objectResult.Value);
            Assert.Equal("BlueBook error", response.ErrorReason);
        }

        [Fact]
        public async Task GetRoles_WhenGenericExceptionThrown_ReturnsForbiddenWithGenericMessage()
        {
            _mockIntegrationType
                .Setup(x => x.GetRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<AccessType?>(), It.IsAny<RequestParameter>()))
                .Throws(new Exception("Some error"));

            var result = await _productInvokerController.GetRoles(ProductEnum.OneSite, 100, 200, new RequestParameter());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, objectResult.StatusCode);
            var response = Assert.IsType<ListResponse>(objectResult.Value);
            Assert.Equal("Internal server error.", response.ErrorReason);
        }

        #endregion

        #region GetRightsForRole Tests

        [Fact]
        public async Task GetRightsForRole_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _productInvokerController.GetRightsForRole(ProductEnum.OneSite, 0, 100, "1", new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRightsForRole_WithNullRoleId_ReturnsBadRequest()
        {
            var result = await _productInvokerController.GetRightsForRole(ProductEnum.OneSite, 100, 200, null!, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRightsForRole_WithZeroRoleId_ReturnsBadRequest()
        {
            var result = await _productInvokerController.GetRightsForRole(ProductEnum.OneSite, 100, 200, "0", new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRightsForRole_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductInvokerController(
                mockUserClaimsAccessor.Object,
                _mockIntegrationTypeFactory.Object,
                _mockProductRepository.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetRightsForRole(ProductEnum.OneSite, 100, 200, "1", new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRightsForRole_WithStandardV1Integration_UsesStringRoleId()
        {
            _mockIntegrationTypeFactory
                .Setup(x => x.GetIntegrationTypeForProductId(It.IsAny<int>()))
                .Returns(ProductIntegrationTypeEnum.StandardV1);

            var listResponse = new ListResponse { IsError = false };
            _mockIntegrationType
                .Setup(x => x.GetRightsForRole(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<RequestParameter>()))
                .Returns(listResponse);

            var result = await _productInvokerController.GetRightsForRole(ProductEnum.OneSite, 100, 200, "abc123", new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
            _mockIntegrationType.Verify(x => x.GetRightsForRole(100, 200, "abc123", 0, false, It.IsAny<RequestParameter>()), Times.Once);
        }

        //[Fact]
        //public async Task GetRightsForRole_WithNonStandardV1Integration_UsesIntRoleId()
        //{
        //    _mockIntegrationTypeFactory
        //        .Setup(x => x.GetIntegrationTypeForProductId(It.IsAny<int>()))
        //        .Returns(ProductIntegrationTypeEnum.Legacy);

        //    var listResponse = new ListResponse { IsError = false };
        //    _mockIntegrationType
        //        .Setup(x => x.GetRightsForRole(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<RequestParameter>()))
        //        .Returns(listResponse);

        //    var result = await _productInvokerController.GetRightsForRole(ProductEnum.OneSite, 100, 200, "123", new RequestParameter());

        //    Assert.IsType<OkObjectResult>(result);
        //    _mockIntegrationType.Verify(x => x.GetRightsForRole(100, 200, 123, 0, false, It.IsAny<RequestParameter>()), Times.Once);
        //}

        [Fact]
        public async Task GetRightsForRole_WhenResultIsError_ReturnsForbidden()
        {
            _mockIntegrationTypeFactory
                .Setup(x => x.GetIntegrationTypeForProductId(It.IsAny<int>()))
                .Returns(ProductIntegrationTypeEnum.StandardV1);

            var listResponse = new ListResponse { IsError = true };
            _mockIntegrationType
                .Setup(x => x.GetRightsForRole(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<RequestParameter>()))
                .Returns(listResponse);

            var result = await _productInvokerController.GetRightsForRole(ProductEnum.OneSite, 100, 200, "1", new RequestParameter());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, objectResult.StatusCode);
        }

        #endregion

        #region GetAllRights Tests

        [Fact]
        public async Task GetAllRights_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _productInvokerController.GetAllRights(ProductEnum.OneSite, 0, 100, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetAllRights_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductInvokerController(
                mockUserClaimsAccessor.Object,
                _mockIntegrationTypeFactory.Object,
                _mockProductRepository.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetAllRights(ProductEnum.OneSite, 100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetAllRights_WithValidParameters_ReturnsOkResult()
        {
            var listResponse = new ListResponse { IsError = false };
            _mockIntegrationType
                .Setup(x => x.GetAllRights(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<RequestParameter>()))
                .Returns(listResponse);

            var result = await _productInvokerController.GetAllRights(ProductEnum.OneSite, 100, 200, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetAllRights_WhenResultIsError_ReturnsForbidden()
        {
            var listResponse = new ListResponse { IsError = true };
            _mockIntegrationType
                .Setup(x => x.GetAllRights(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<RequestParameter>()))
                .Returns(listResponse);

            var result = await _productInvokerController.GetAllRights(ProductEnum.OneSite, 100, 200, new RequestParameter());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, objectResult.StatusCode);
        }

        #endregion

        #region GetProperties Tests

        [Fact]
        public async Task GetProperties_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _productInvokerController.GetProperties(ProductEnum.OneSite, 0, 100, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProperties_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductInvokerController(
                mockUserClaimsAccessor.Object,
                _mockIntegrationTypeFactory.Object,
                _mockProductRepository.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetProperties(ProductEnum.OneSite, 100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProperties_WithValidParameters_ReturnsOkResult()
        {
            var listResponse = new ListResponse { IsError = false };
            _mockIntegrationType
                .Setup(x => x.GetProperties(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<RequestParameter>()))
                .Returns(listResponse);

            var result = await _productInvokerController.GetProperties(ProductEnum.OneSite, 100, 200, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetProperties_WhenResultIsError_ReturnsForbidden()
        {
            var listResponse = new ListResponse { IsError = true };
            _mockIntegrationType
                .Setup(x => x.GetProperties(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<RequestParameter>()))
                .Returns(listResponse);

            var result = await _productInvokerController.GetProperties(ProductEnum.OneSite, 100, 200, new RequestParameter());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetProperties_WhenBlueBookExceptionThrown_ReturnsOkWithError()
        {
            _mockIntegrationType
                .Setup(x => x.GetProperties(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<RequestParameter>()))
                .Throws(new Exception("Outer", new BlueBookException("BlueBook error")));

            var result = await _productInvokerController.GetProperties(ProductEnum.OneSite, 100, 200, new RequestParameter());

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ListResponse>(okResult.Value);
            Assert.True(response.IsError);
            Assert.Equal("BlueBook error", response.ErrorReason);
        }

        #endregion

        #region GetPropertyGroups Tests

        [Fact]
        public async Task GetPropertyGroups_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _productInvokerController.GetPropertyGroups(ProductEnum.OneSite, 0, 100, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPropertyGroups_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductInvokerController(
                mockUserClaimsAccessor.Object,
                _mockIntegrationTypeFactory.Object,
                _mockProductRepository.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetPropertyGroups(ProductEnum.OneSite, 100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPropertyGroups_WithValidParameters_ReturnsOkResult()
        {
            var listResponse = new ListResponse { IsError = false };
            _mockIntegrationType
                .Setup(x => x.GetPropertyGroups(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<RequestParameter>(), It.IsAny<string>()))
                .Returns(listResponse);

            var result = await _productInvokerController.GetPropertyGroups(ProductEnum.OneSite, 100, 200, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetPropertyGroups_WhenErrorWithAreaTabName_SetsAreaErrorMessage()
        {
            var listResponse = new ListResponse { IsError = true };
            _mockIntegrationType
                .Setup(x => x.GetPropertyGroups(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<RequestParameter>(), It.IsAny<string>()))
                .Returns(listResponse);

            var result = await _productInvokerController.GetPropertyGroups(ProductEnum.OneSite, 100, 200, new RequestParameter(), "Area");

            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<ListResponse>(objectResult.Value);
            Assert.Equal("No areas available for this user.", response.ErrorReason);
        }

        [Fact]
        public async Task GetPropertyGroups_WhenErrorWithRegionTabName_SetsRegionErrorMessage()
        {
            var listResponse = new ListResponse { IsError = true };
            _mockIntegrationType
                .Setup(x => x.GetPropertyGroups(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<RequestParameter>(), It.IsAny<string>()))
                .Returns(listResponse);

            var result = await _productInvokerController.GetPropertyGroups(ProductEnum.OneSite, 100, 200, new RequestParameter(), "Region");

            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<ListResponse>(objectResult.Value);
            Assert.Equal("No regions available for this user.", response.ErrorReason);
        }

        #endregion

        #region GetPropertiesByGroup Tests

        [Fact]
        public async Task GetPropertiesByGroup_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _productInvokerController.GetPropertiesByGroup(ProductEnum.OneSite, 0, 100, "1", new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPropertiesByGroup_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductInvokerController(
                mockUserClaimsAccessor.Object,
                _mockIntegrationTypeFactory.Object,
                _mockProductRepository.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetPropertiesByGroup(ProductEnum.OneSite, 100, 200, "1", new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPropertiesByGroup_WithNullGroupId_ReturnsBadRequest()
        {
            var result = await _productInvokerController.GetPropertiesByGroup(ProductEnum.OneSite, 100, 200, null!, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Group Id is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPropertiesByGroup_WithEmptyGroupId_ReturnsBadRequest()
        {
            var result = await _productInvokerController.GetPropertiesByGroup(ProductEnum.OneSite, 100, 200, "", new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Group Id is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPropertiesByGroup_WithValidParameters_ReturnsOkResult()
        {
            var listResponse = new ListResponse { IsError = false };
            _mockIntegrationType
                .Setup(x => x.GetPropertiesByGroup(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<RequestParameter>()))
                .Returns(listResponse);

            var result = await _productInvokerController.GetPropertiesByGroup(ProductEnum.OneSite, 100, 200, "group1", new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetProductOrganizations Tests

        [Fact]
        public async Task GetProductOrganizations_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _productInvokerController.GetProductOrganizations(ProductEnum.OneSite, 0, 100, "role1", "site");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProductOrganizations_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductInvokerController(
                mockUserClaimsAccessor.Object,
                _mockIntegrationTypeFactory.Object,
                _mockProductRepository.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetProductOrganizations(ProductEnum.OneSite, 100, 200, "role1", "site");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProductOrganizations_WithValidParameters_ReturnsOkResult()
        {
            var listResponse = new ListResponse { IsError = false };
            _mockIntegrationType
                .Setup(x => x.GetOrganizations(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(listResponse);

            var result = await _productInvokerController.GetProductOrganizations(ProductEnum.OneSite, 100, 200, "role1", "site");

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region ListMigrationUsers Tests

        [Fact]
        public async Task ListMigrationUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _productInvokerController.ListMigrationUsers("ONESITE", 0, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task ListMigrationUsers_WithValidParameters_ReturnsOkResult()
        {
            var listResponse = new ListResponse { IsError = false };
            _mockProductRepository
                .Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GbProductMap> { new GbProductMap { ProductId = 1, BooksProductCode = "ONESITE" } });
            _mockIntegrationType
                .Setup(x => x.GetMigrationUsers(It.IsAny<long>(), It.IsAny<RequestParameter>()))
                .Returns(listResponse);

            var result = await _productInvokerController.ListMigrationUsers("ONESITE", 100, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListMigrationUsers_WhenResultIsError_ReturnsForbidden()
        {
            var listResponse = new ListResponse { IsError = true };
            _mockProductRepository
                .Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GbProductMap> { new GbProductMap { ProductId = 1, BooksProductCode = "ONESITE" } });
            _mockIntegrationType
                .Setup(x => x.GetMigrationUsers(It.IsAny<long>(), It.IsAny<RequestParameter>()))
                .Returns(listResponse);

            var result = await _productInvokerController.ListMigrationUsers("ONESITE", 100, new RequestParameter());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, objectResult.StatusCode);
        }

        #endregion

        #region UpdateUsersMigrationStatus Tests

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductInvokerController(
                mockUserClaimsAccessor.Object,
                _mockIntegrationTypeFactory.Object,
                _mockProductRepository.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.UpdateUsersMigrationStatus("ONESITE", new List<MigrateUser>());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithValidParameters_ReturnsOkResult()
        {
            var migrateResponse = new MigrateResponse { Status = true };
            _mockProductRepository
                .Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GbProductMap> { new GbProductMap { ProductId = 1, BooksProductCode = "ONESITE" } });
            _mockIntegrationType
                .Setup(x => x.UpdateUsersMigrationStatus(It.IsAny<long>(), It.IsAny<IList<MigrateUser>>()))
                .Returns(migrateResponse);

            var result = await _productInvokerController.UpdateUsersMigrationStatus("ONESITE", new List<MigrateUser>());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WhenStatusFalse_ReturnsForbidden()
        {
            var migrateResponse = new MigrateResponse { Status = false };
            _mockProductRepository
                .Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GbProductMap> { new GbProductMap { ProductId = 1, BooksProductCode = "ONESITE" } });
            _mockIntegrationType
                .Setup(x => x.UpdateUsersMigrationStatus(It.IsAny<long>(), It.IsAny<IList<MigrateUser>>()))
                .Returns(migrateResponse);

            var result = await _productInvokerController.UpdateUsersMigrationStatus("ONESITE", new List<MigrateUser>());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, objectResult.StatusCode);
        }

        #endregion

        #region ExternalProductUserProfileChange Tests

        [Fact]
        public async Task ExternalProductUserProfileChange_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductInvokerController(
                mockUserClaimsAccessor.Object,
                _mockIntegrationTypeFactory.Object,
                _mockProductRepository.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.ExternalProductUserProfileChange("ONESITE", new ProductUserProfile());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task ExternalProductUserProfileChange_WhenSuccessful_ReturnsOkWithMessage()
        {
            _mockProductRepository
                .Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GbProductMap> { new GbProductMap { ProductId = 1, BooksProductCode = "ONESITE" } });
            _mockIntegrationType
                .Setup(x => x.ExternalUserProfileChange(It.IsAny<long>(), It.IsAny<ProductUserProfile>()))
                .Returns(true);

            var result = await _productInvokerController.ExternalProductUserProfileChange("ONESITE", new ProductUserProfile());

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Successfully disabled product user.", okResult.Value);
        }

        [Fact]
        public async Task ExternalProductUserProfileChange_WhenFails_ReturnsForbidden()
        {
            _mockProductRepository
                .Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GbProductMap> { new GbProductMap { ProductId = 1, BooksProductCode = "ONESITE" } });
            _mockIntegrationType
                .Setup(x => x.ExternalUserProfileChange(It.IsAny<long>(), It.IsAny<ProductUserProfile>()))
                .Returns(false);

            var result = await _productInvokerController.ExternalProductUserProfileChange("ONESITE", new ProductUserProfile());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, objectResult.StatusCode);
            Assert.Equal("Failed to disabled product user.", objectResult.Value);
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _productInvokerController = null!;
            base.Dispose();
        }

        #endregion
    }
}
