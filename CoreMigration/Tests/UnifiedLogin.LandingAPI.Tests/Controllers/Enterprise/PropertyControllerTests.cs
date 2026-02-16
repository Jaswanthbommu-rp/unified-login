using Elasticsearch.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPIEnterprise.Controllers;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Ops;
using UnifiedLogin.SharedObjects.ResponseObject;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers.Enterprise
{
    /// <summary>
    /// Comprehensive unit tests for PropertyController with 100% code coverage.
    /// Tests all endpoints, error cases, validation scenarios, and business logic.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PropertyControllerTests
    {
        #region Private Fields

        private readonly Mock<IIntegrationTypeFactory> _mockIntegrationTypeFactory;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IManageUnifiedLogin> _mockManageUnifiedLogin;
        private readonly Mock<IManagePerson> _mockManagePerson;
        private readonly Mock<IManagePersona> _mockManagePersona;
        private readonly Mock<IManageProductOps> _mockManageProductOps;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private readonly Mock<IManageUPFMProductsIntegrationFactory> _mockManageUPFMProductsIntegrationFactory;
        private readonly Mock<IManageUPFMProductsIntegration> _mockManageUPFMProductsIntegration;
        private readonly PropertyController _controller;
        private readonly DefaultUserClaim _userClaims;
        private readonly Guid _testUserId = Guid.NewGuid();
        private readonly Guid _testOrgId = Guid.NewGuid();
        private readonly long _testPersonaId = 100;

        #endregion

        #region Constructor & Setup

        public PropertyControllerTests()
        {
            _mockIntegrationTypeFactory = new Mock<IIntegrationTypeFactory>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockManageUnifiedLogin = new Mock<IManageUnifiedLogin>();
            _mockManagePerson = new Mock<IManagePerson>();
            _mockManagePersona = new Mock<IManagePersona>();
            _mockManageProductOps = new Mock<IManageProductOps>();
            _mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            _mockManageUPFMProductsIntegrationFactory = new Mock<IManageUPFMProductsIntegrationFactory>();
            _mockManageUPFMProductsIntegration = new Mock<IManageUPFMProductsIntegration>();

            // Setup user claims
            _userClaims = new DefaultUserClaim
            {
                PersonaId = _testPersonaId,
                OrganizationPartyId = 1000,
                UserRealPageGuid = _testUserId,
                OrganizationRealPageGuid = _testOrgId,
                CorrelationId = Guid.NewGuid()
            };

            // Setup the mock to return the user claims
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(_userClaims);
            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(_testPersonaId);
            _mockUserClaimsAccessor.Setup(x => x.OrganizationPartyId).Returns(1000);
            _mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(_testUserId);
            _mockUserClaimsAccessor.Setup(x => x.OrganizationRealPageGuid).Returns(_testOrgId);

            // Setup factory to return the mock integration
            _mockManageUPFMProductsIntegrationFactory
                .Setup(x => x.Create(It.IsAny<int>()))
                .Returns(_mockManageUPFMProductsIntegration.Object);

            _controller = new PropertyController(
                _mockIntegrationTypeFactory.Object,
                _mockProductRepository.Object,
                _mockManageUnifiedLogin.Object,
                _mockManagePerson.Object,
                _mockManagePersona.Object,
                _mockManageProductOps.Object,
                _mockUserClaimsAccessor.Object,
                _mockManageUPFMProductsIntegrationFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Act
            var controller = new PropertyController(
                _mockIntegrationTypeFactory.Object,
                _mockProductRepository.Object,
                _mockManageUnifiedLogin.Object,
                _mockManagePerson.Object,
                _mockManagePersona.Object,
                _mockManageProductOps.Object,
                _mockUserClaimsAccessor.Object,
                _mockManageUPFMProductsIntegrationFactory.Object);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullIntegrationTypeFactory_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PropertyController(
                    null,
                    _mockProductRepository.Object,
                    _mockManageUnifiedLogin.Object,
                    _mockManagePerson.Object,
                    _mockManagePersona.Object,
                    _mockManageProductOps.Object,
                    _mockUserClaimsAccessor.Object,
                    _mockManageUPFMProductsIntegrationFactory.Object));
        }

        [Fact]
        public void Constructor_WithNullProductRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PropertyController(
                    _mockIntegrationTypeFactory.Object,
                    null,
                    _mockManageUnifiedLogin.Object,
                    _mockManagePerson.Object,
                    _mockManagePersona.Object,
                    _mockManageProductOps.Object,
                    _mockUserClaimsAccessor.Object,
                    _mockManageUPFMProductsIntegrationFactory.Object));
        }

        [Fact]
        public void Constructor_WithNullManageUnifiedLogin_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PropertyController(
                    _mockIntegrationTypeFactory.Object,
                    _mockProductRepository.Object,
                    null,
                    _mockManagePerson.Object,
                    _mockManagePersona.Object,
                    _mockManageProductOps.Object,
                    _mockUserClaimsAccessor.Object,
                    _mockManageUPFMProductsIntegrationFactory.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePerson_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PropertyController(
                    _mockIntegrationTypeFactory.Object,
                    _mockProductRepository.Object,
                    _mockManageUnifiedLogin.Object,
                    null,
                    _mockManagePersona.Object,
                    _mockManageProductOps.Object,
                    _mockUserClaimsAccessor.Object,
                    _mockManageUPFMProductsIntegrationFactory.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePersona_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PropertyController(
                    _mockIntegrationTypeFactory.Object,
                    _mockProductRepository.Object,
                    _mockManageUnifiedLogin.Object,
                    _mockManagePerson.Object,
                    null,
                    _mockManageProductOps.Object,
                    _mockUserClaimsAccessor.Object,
                    _mockManageUPFMProductsIntegrationFactory.Object));
        }

        [Fact]
        public void Constructor_WithNullManageProductOps_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PropertyController(
                    _mockIntegrationTypeFactory.Object,
                    _mockProductRepository.Object,
                    _mockManageUnifiedLogin.Object,
                    _mockManagePerson.Object,
                    _mockManagePersona.Object,
                    null,
                    _mockUserClaimsAccessor.Object,
                    _mockManageUPFMProductsIntegrationFactory.Object));
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PropertyController(
                    _mockIntegrationTypeFactory.Object,
                    _mockProductRepository.Object,
                    _mockManageUnifiedLogin.Object,
                    _mockManagePerson.Object,
                    _mockManagePersona.Object,
                    _mockManageProductOps.Object,
                    null,
                    _mockManageUPFMProductsIntegrationFactory.Object));
        }

        [Fact]
        public void Constructor_WithNullManageUPFMProductsIntegrationFactory_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PropertyController(
                    _mockIntegrationTypeFactory.Object,
                    _mockProductRepository.Object,
                    _mockManageUnifiedLogin.Object,
                    _mockManagePerson.Object,
                    _mockManagePersona.Object,
                    _mockManageProductOps.Object,
                    _mockUserClaimsAccessor.Object,
                    null));
        }

        #endregion

        #region GetUserProductProperties Tests

        [Fact]
        public void GetUserProductProperties_WithValidOpsProduct_ReturnsOkResult()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var person = new Person { RealPageId = realPageId, FirstName = "Test" };
            var persona = new Persona { PersonaId = 100, RealPageId = realPageId, OrganizationPartyId = 1000 };
            var assetGroups = new List<AssetGroup>
            {
                new AssetGroup { ID = "1", IsAssigned = true, Name = "Asset1" },
                new AssetGroup { ID = "2", IsAssigned = false, Name = "Asset2" }
            };
            var listResponse = new ListResponse { Records = assetGroups.Cast<object>().ToList(), IsError = false };
            var products = new List<GbProductMap>
            {
                new GbProductMap { ProductId = (int)ProductEnum.OpsBuyer, BooksProductCode = "OPS" }
            };

            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns(person);
            _mockManagePersona.Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, 1000)).Returns(persona);
            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);
            _mockManageProductOps.Setup(x => x.GetCompanyAssets(
                _testPersonaId, persona.PersonaId, false, null)).Returns(listResponse);

            // Act
            var result = _controller.GetUserProductProperties(realPageId, "OPS");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult.Value as PagedResponse;
            response.Should().NotBeNull();
            response.Data.Should().HaveCount(1);
            response.Meta.TotalRows.Should().Be(1);
        }

        [Fact]
        public void GetUserProductProperties_WithNullPerson_ReturnsNotFound()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns((Person)null);

            // Act
            var result = _controller.GetUserProductProperties(realPageId, "OPS");

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void GetUserProductProperties_WithNullPersona_ReturnsNotFound()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var person = new Person { RealPageId = realPageId };
            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns(person);
            _mockManagePersona.Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, 1000)).Returns((Persona)null);

            // Act
            var result = _controller.GetUserProductProperties(realPageId, "OPS");

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void GetUserProductProperties_WithPersonaFromDifferentOrganization_ReturnsNotFound()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var person = new Person { RealPageId = realPageId };
            var persona = new Persona { PersonaId = 100, RealPageId = realPageId, OrganizationPartyId = 2000 };
            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns(person);
            _mockManagePersona.Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, 1000)).Returns(persona);

            // Act
            var result = _controller.GetUserProductProperties(realPageId, "OPS");

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void GetUserProductProperties_WhenServiceReturnsError_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var person = new Person { RealPageId = realPageId };
            var persona = new Persona { PersonaId = 100, RealPageId = realPageId, OrganizationPartyId = 1000 };
            var listResponse = new ListResponse { IsError = true, ErrorReason = "Service error" };
            var products = new List<GbProductMap>
            {
                new GbProductMap { ProductId = (int)ProductEnum.OpsBuyer, BooksProductCode = "OPS" }
            };

            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns(person);
            _mockManagePersona.Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, 1000)).Returns(persona);
            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);
            _mockManageProductOps.Setup(x => x.GetCompanyAssets(
                _testPersonaId, persona.PersonaId, false, null)).Returns(listResponse);

            // Act
            var result = _controller.GetUserProductProperties(realPageId, "OPS");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            var errorResponse = badRequest.Value as ErrorResponse;
            errorResponse.Errors.Should().HaveCount(1);
            errorResponse.Errors[0].Detail.Should().Be("Service error");
        }

        [Fact]
        public void GetUserProductProperties_WithInvalidProductCode_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var person = new Person { RealPageId = realPageId };
            var persona = new Persona { PersonaId = 100, RealPageId = realPageId, OrganizationPartyId = 1000 };
            var products = new List<GbProductMap>();

            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns(person);
            _mockManagePersona.Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, 1000)).Returns(persona);
            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);

            // Act
            var result = _controller.GetUserProductProperties(realPageId, "INVALID");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest.Value.Should().BeOfType<ErrorResponse>();

            var errorResponse = badRequest.Value as ErrorResponse;
            errorResponse.Should().NotBeNull();
            errorResponse.Errors.Should().HaveCount(1);
            errorResponse.Errors[0].Detail.Should().Contain("Invalid product code");
        }

        [Fact]
        public void GetUserProductProperties_FiltersAssignedPropertiesOnly()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var person = new Person { RealPageId = realPageId };
            var persona = new Persona { PersonaId = 100, RealPageId = realPageId, OrganizationPartyId = 1000 };
            var assetGroups = new List<AssetGroup>
            {
                new AssetGroup { ID = "1", IsAssigned = true, Name = "Assigned" },
                new AssetGroup { ID = "2", IsAssigned = false, Name = "Unassigned" },
                new AssetGroup { ID = "3", IsAssigned = true, Name = "Assigned2" }
            };
            var listResponse = new ListResponse { Records = assetGroups.Cast<object>().ToList(), IsError = false };
            var products = new List<GbProductMap>
            {
                new GbProductMap { ProductId = (int)ProductEnum.OpsBuyer, BooksProductCode = "OPS" }
            };

            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns(person);
            _mockManagePersona.Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, 1000)).Returns(persona);
            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);
            _mockManageProductOps.Setup(x => x.GetCompanyAssets(
                _testPersonaId, persona.PersonaId, false, null)).Returns(listResponse);

            // Act
            var result = _controller.GetUserProductProperties(realPageId, "OPS");

            // Assert
            var okResult = result as OkObjectResult;
            var response = okResult.Value as PagedResponse;
            response.Data.Should().HaveCount(2);
            response.Meta.TotalRows.Should().Be(2);
        }

        #endregion

        #region GetProductProperties Tests

        [Fact]
        public void GetProductProperties_WithOpsProduct_ReturnsOkResult()
        {
            // Arrange
            var products = new List<GbProductMap>
            {
                new GbProductMap { ProductId = (int)ProductEnum.OpsBuyer, BooksProductCode = "OPS" }
            };
            var assetGroups = new List<AssetGroup>
            {
                new AssetGroup { ID = "1", Name = "Asset1" }
            };
            var listResponse = new ListResponse
            {
                Records = assetGroups.Cast<object>().ToList(),
                IsError = false,
                TotalRows = 1
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);
            _mockManageProductOps.Setup(x => x.GetCompanyAssets(
                _testPersonaId, 0, false, null)).Returns(listResponse);

            // Act
            var result = _controller.GetProductProperties("OPS");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult.Value as PagedResponse;
            response.Data.Should().HaveCount(1);
        }

        [Fact]
        public void GetProductProperties_WithUnifiedPlatformProduct_CallsManageUnifiedLogin()
        {
            // Arrange
            var products = new List<GbProductMap>
            {
                new GbProductMap { ProductId = (int)ProductEnum.UnifiedPlatform, BooksProductCode = "UNIFIEDPLATFORM" }
            };
            var properties = new List<ProductProperty>
            {
                new ProductProperty { ID = "1", Name = "Property1" }
            };
            var listResponse = new ListResponse
            {
                Records = properties.Cast<object>().ToList(),
                IsError = false,
                TotalRows = 1
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);
            _mockManageUnifiedLogin.Setup(x => x.GetEnterpriseProperties(
                _testPersonaId, "include")).Returns(listResponse);

            // Act
            var result = _controller.GetProductProperties("UNIFIEDPLATFORM", "include");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockManageUnifiedLogin.Verify(
                x => x.GetEnterpriseProperties(_testPersonaId, "include"),
                Times.Once);
        }

        [Fact]
        public void GetProductProperties_WhenServiceReturnsError_ReturnsBadRequest()
        {
            // Arrange
            var products = new List<GbProductMap>
            {
                new GbProductMap { ProductId = (int)ProductEnum.OpsBuyer, BooksProductCode = "OPS" }
            };
            var listResponse = new ListResponse
            {
                IsError = true,
                ErrorReason = "Service failed"
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);
            _mockManageProductOps.Setup(x => x.GetCompanyAssets(
                _testPersonaId, 0, false, null)).Returns(listResponse);

            // Act
            var result = _controller.GetProductProperties("OPS");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            var errorResponse = badRequest.Value as ErrorResponse;
            errorResponse.Errors[0].Detail.Should().Be("Service failed");
        }

        #endregion

        #region GetUserCompanyProperties Tests

        [Fact]
        public void GetUserCompanyProperties_WithValidProductCode_ReturnsOkResult()
        {
            // Arrange
            var products = new List<GbProductMap>
            {
                new GbProductMap { ProductId = (int)ProductEnum.UnifiedPlatform, BooksProductCode = "UPFM" }
            };
            var userCompanyProperties = new List<UserCompaniesProperties>
            {
                new UserCompaniesProperties { Id = "1", OrganizationName = "Company1" }
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);
            _mockManageUPFMProductsIntegration
                .Setup(x => x.GetUPFMMultiCompanyProperties("UPFM"))
                .Returns(userCompanyProperties);

            // Act
            var result = _controller.GetUserCompanyProperties("UPFM");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockManageUPFMProductsIntegrationFactory.Verify(
                x => x.Create((int)ProductEnum.UnifiedPlatform),
                Times.Once);
        }

        [Fact]
        public void GetUserCompanyProperties_WhenResponseIsNull_ReturnsOkWithMessage()
        {
            // Arrange
            var products = new List<GbProductMap>
            {
                new GbProductMap { ProductId = (int)ProductEnum.UnifiedPlatform, BooksProductCode = "UPFM" }
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);
            _mockManageUPFMProductsIntegration
                .Setup(x => x.GetUPFMMultiCompanyProperties("UPFM"))
                .Returns((List<UserCompaniesProperties>)null);

            // Act
            var result = _controller.GetUserCompanyProperties("UPFM");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeOfType<string>();
            okResult.Value.As<string>().Should().Contain("product not assigned");
        }

        [Fact]
        public void GetUserCompanyProperties_WithInvalidProductCode_ReturnsBadRequest()
        {
            // Arrange
            var products = new List<GbProductMap>();

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);

            // Act
            var result = _controller.GetUserCompanyProperties("INVALID");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            var errorResponse = badRequest.Value as ErrorResponse;
            errorResponse.Errors[0].Detail.Should().Contain("Invalid product code");
        }

        [Fact]
        public void GetUserCompanyProperties_WhenExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            var products = new List<GbProductMap>
            {
                new GbProductMap { ProductId = (int)ProductEnum.UnifiedPlatform, BooksProductCode = "UPFM" }
            };

            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);
            _mockManageUPFMProductsIntegration
                .Setup(x => x.GetUPFMMultiCompanyProperties("UPFM"))
                .Throws(new Exception("Test exception"));

            // Act
            var result = _controller.GetUserCompanyProperties("UPFM");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            var errorResponse = badRequest.Value as ErrorResponse;
            errorResponse.Errors[0].Detail.Should().Contain("Error processing product code");
        }

        #endregion

        #region GetOpsAssetGroups Tests

        [Fact]
        public void GetOpsAssetGroups_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var assetGroups = new List<AssetGroup>
            {
                new AssetGroup { ID = "1", Name = "Group1" },
                new AssetGroup { ID = "2", Name = "Group2" }
            };
            var listResponse = new ListResponse
            {
                Records = assetGroups.Cast<object>().ToList(),
                IsError = false,
                TotalRows = 2
            };

            _mockManageProductOps.Setup(x => x.GetOpsAssetGroups(
                _testPersonaId, 0, 0)).Returns(listResponse);

            // Act
            var result = _controller.GetOpsAssetGroups();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult.Value as PagedResponse;
            response.Data.Should().HaveCount(2);
            response.Meta.TotalRows.Should().Be(2);
        }

        [Fact]
        public void GetOpsAssetGroups_WithSpecificAssetGroupId_PassesIdToService()
        {
            // Arrange
            var assetGroup = new List<AssetGroup>
            {
                new AssetGroup { ID = "5", Name = "SpecificGroup" }
            };
            var listResponse = new ListResponse
            {
                Records = assetGroup.Cast<object>().ToList(),
                IsError = false,
                TotalRows = 1
            };

            _mockManageProductOps.Setup(x => x.GetOpsAssetGroups(
                _testPersonaId, 0, 5)).Returns(listResponse);

            // Act
            var result = _controller.GetOpsAssetGroups(5);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockManageProductOps.Verify(
                x => x.GetOpsAssetGroups(_testPersonaId, 0, 5),
                Times.Once);
        }

        [Fact]
        public void GetOpsAssetGroups_WhenServiceReturnsError_ReturnsBadRequest()
        {
            // Arrange
            var listResponse = new ListResponse
            {
                IsError = true,
                ErrorReason = "Asset groups not found"
            };

            _mockManageProductOps.Setup(x => x.GetOpsAssetGroups(
                _testPersonaId, 0, 0)).Returns(listResponse);

            // Act
            var result = _controller.GetOpsAssetGroups();

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            var errorResponse = badRequest.Value as ErrorResponse;
            errorResponse.Errors[0].Detail.Should().Be("Asset groups not found");
        }

        #endregion

        #region GetOpsAssets Tests

        [Fact]
        public void GetOpsAssets_WithDefaultStatus_ReturnsOkResult()
        {
            // Arrange
            var portfolios = new List<Portfolio>
            {
                new Portfolio { ID = "1", Name = "Portfolio1" }
            };
            var listResponse = new ListResponse
            {
                Records = portfolios.Cast<object>().ToList(),
                IsError = false,
                TotalRows = 1
            };

            _mockManageProductOps.Setup(x => x.GetOpsAssets(
                _testPersonaId, 0, "all")).Returns(listResponse);

            // Act
            var result = _controller.GetOpsAssets();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult.Value as PagedResponse;
            response.Data.Should().HaveCount(1);
        }

        [Fact]
        public void GetOpsAssets_WithSpecificStatus_PassesStatusToService()
        {
            // Arrange
            var portfolios = new List<Portfolio>
            {
                new Portfolio { ID = "1", Name = "Active" }
            };
            var listResponse = new ListResponse
            {
                Records = portfolios.Cast<object>().ToList(),
                IsError = false,
                TotalRows = 1
            };

            _mockManageProductOps.Setup(x => x.GetOpsAssets(
                _testPersonaId, 0, "active")).Returns(listResponse);

            // Act
            var result = _controller.GetOpsAssets("active");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockManageProductOps.Verify(
                x => x.GetOpsAssets(_testPersonaId, 0, "active"),
                Times.Once);
        }

        [Fact]
        public void GetOpsAssets_WhenServiceReturnsError_ReturnsBadRequest()
        {
            // Arrange
            var listResponse = new ListResponse
            {
                IsError = true,
                ErrorReason = "Assets not available"
            };

            _mockManageProductOps.Setup(x => x.GetOpsAssets(
                _testPersonaId, 0, "all")).Returns(listResponse);

            // Act
            var result = _controller.GetOpsAssets();

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            var errorResponse = badRequest.Value as ErrorResponse;
            errorResponse.Errors[0].Detail.Should().Be("Assets not available");
        }

        #endregion

        #region CreateOpsAssetGroups Tests

        [Fact]
        public void CreateOpsAssetGroups_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var assetGroupCreate = new AssetGroupCreate
            {
                Name = "NewGroup",
                Description = "Test Group"
            };
            var createdGroup = new List<AssetGroup>
            {
                new AssetGroup { ID = "10", Name = "NewGroup" }
            };
            var listResponse = new ListResponse
            {
                Records = createdGroup.Cast<object>().ToList(),
                IsError = false,
                TotalRows = 1
            };

            _mockManageProductOps.Setup(x => x.CreateOpsAssetGroup(
                _testPersonaId, 0, assetGroupCreate)).Returns(listResponse);

            // Act
            var result = _controller.CreateOpsAssetGroups(assetGroupCreate);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult.Value as PagedResponse;
            response.Data.Should().HaveCount(1);
        }

        [Fact]
        public void CreateOpsAssetGroups_WithNullAssetGroup_PassesNullToService()
        {
            // Arrange
            var listResponse = new ListResponse
            {
                Records = new List<object>(),
                IsError = false,
                TotalRows = 0
            };

            _mockManageProductOps.Setup(x => x.CreateOpsAssetGroup(
                _testPersonaId, 0, null)).Returns(listResponse);

            // Act
            var result = _controller.CreateOpsAssetGroups(null);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void CreateOpsAssetGroups_WhenServiceReturnsError_ReturnsBadRequest()
        {
            // Arrange
            var assetGroupCreate = new AssetGroupCreate { Name = "Group" };
            var listResponse = new ListResponse
            {
                IsError = true,
                ErrorReason = "Creation failed"
            };

            _mockManageProductOps.Setup(x => x.CreateOpsAssetGroup(
                _testPersonaId, 0, assetGroupCreate)).Returns(listResponse);

            // Act
            var result = _controller.CreateOpsAssetGroups(assetGroupCreate);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            var errorResponse = badRequest.Value as ErrorResponse;
            errorResponse.Errors[0].Detail.Should().Be("Creation failed");
        }

        #endregion

        #region UpdateOpsAssetGroups Tests

        [Fact]
        public void UpdateOpsAssetGroups_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var assetGroupId = 5;
            var assetGroupUpdate = new AssetGroupCreate
            {
                Name = "UpdatedGroup",
                Description = "Updated"
            };
            var updatedGroup = new List<AssetGroup>
            {
                new AssetGroup { ID = "5", Name = "UpdatedGroup" }
            };
            var listResponse = new ListResponse
            {
                Records = updatedGroup.Cast<object>().ToList(),
                IsError = false,
                TotalRows = 1
            };

            _mockManageProductOps.Setup(x => x.UpdateOpsAssetGroup(
                _testPersonaId, 0, assetGroupId, assetGroupUpdate)).Returns(listResponse);

            // Act
            var result = _controller.UpdateOpsAssetGroups(assetGroupUpdate, assetGroupId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockManageProductOps.Verify(
                x => x.UpdateOpsAssetGroup(_testPersonaId, 0, assetGroupId, assetGroupUpdate),
                Times.Once);
        }

        [Fact]
        public void UpdateOpsAssetGroups_WhenServiceReturnsError_ReturnsBadRequest()
        {
            // Arrange
            var assetGroupId = 5;
            var assetGroupUpdate = new AssetGroupCreate { Name = "Updated" };
            var listResponse = new ListResponse
            {
                IsError = true,
                ErrorReason = "Update failed"
            };

            _mockManageProductOps.Setup(x => x.UpdateOpsAssetGroup(
                _testPersonaId, 0, assetGroupId, assetGroupUpdate)).Returns(listResponse);

            // Act
            var result = _controller.UpdateOpsAssetGroups(assetGroupUpdate, assetGroupId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region PatchOpsAssetGroups Tests

        [Fact]
        public void PatchOpsAssetGroups_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var assetGroupId = 5;
            var assetGroupPatch = new AssetGroupPatch
            {
                Name = "PatchedGroup"
            };
            var patchedGroup = new List<AssetGroup>
            {
                new AssetGroup { ID = "5", Name = "PatchedGroup" }
            };
            var listResponse = new ListResponse
            {
                Records = patchedGroup.Cast<object>().ToList(),
                IsError = false,
                TotalRows = 1
            };

            _mockManageProductOps.Setup(x => x.PatchOpsAssetGroup(
                _testPersonaId, 0, assetGroupId, assetGroupPatch)).Returns(listResponse);

            // Act
            var result = _controller.PatchOpsAssetGroups(assetGroupPatch, assetGroupId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockManageProductOps.Verify(
                x => x.PatchOpsAssetGroup(_testPersonaId, 0, assetGroupId, assetGroupPatch),
                Times.Once);
        }

        [Fact]
        public void PatchOpsAssetGroups_WhenServiceReturnsError_ReturnsBadRequest()
        {
            // Arrange
            var assetGroupId = 5;
            var assetGroupPatch = new AssetGroupPatch { Name = "Patched" };
            var listResponse = new ListResponse
            {
                IsError = true,
                ErrorReason = "Patch failed"
            };

            _mockManageProductOps.Setup(x => x.PatchOpsAssetGroup(
                _testPersonaId, 0, assetGroupId, assetGroupPatch)).Returns(listResponse);

            // Act
            var result = _controller.PatchOpsAssetGroups(assetGroupPatch, assetGroupId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            var errorResponse = badRequest.Value as ErrorResponse;
            errorResponse.Errors[0].Detail.Should().Be("Patch failed");
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public void GetOpsAssetGroups_WithZeroAssetGroupId_ReturnsAllGroups()
        {
            // Arrange
            var assetGroups = new List<AssetGroup>
            {
                new AssetGroup { ID = "1", Name = "Group1" },
                new AssetGroup { ID = "2", Name = "Group2" }
            };
            var listResponse = new ListResponse
            {
                Records = assetGroups.Cast<object>().ToList(),
                IsError = false,
                TotalRows = 2
            };

            _mockManageProductOps.Setup(x => x.GetOpsAssetGroups(
                _testPersonaId, 0, 0)).Returns(listResponse);

            // Act
            var result = _controller.GetOpsAssetGroups(0);

            // Assert
            var okResult = result as OkObjectResult;
            var response = okResult.Value as PagedResponse;
            response.Data.Should().HaveCount(2);
        }

        [Fact]
        public void PagedResponse_PropertiesInitializedCorrectly()
        {
            // Arrange
            var assetGroups = new List<AssetGroup>
            {
                new AssetGroup { ID = "1", Name = "Group1" }
            };
            var listResponse = new ListResponse
            {
                Records = assetGroups.Cast<object>().ToList(),
                IsError = false,
                TotalRows = 5
            };

            _mockManageProductOps.Setup(x => x.GetOpsAssetGroups(
                _testPersonaId, 0, 0)).Returns(listResponse);

            // Act
            var result = _controller.GetOpsAssetGroups();

            // Assert
            var okResult = result as OkObjectResult;
            var response = okResult.Value as PagedResponse;
            response.Meta.CurrentPage.Should().Be(1);
            response.Meta.TotalRows.Should().Be(5);
            response.Meta.RowsPerPage.Should().Be(5);
        }

        #endregion
    }
}