using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.Services.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using Xunit;
using Role = UnifiedLogin.SharedObjects.Product.UnifiedLogin.Role;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Unit tests for PersonaController (async refactor).
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PersonaControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private readonly Mock<IManagePersonaAsync> _mockManagePersona;
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<IProductInternalSettingRepositoryAsync> _mockProductInternalSettingRepository;
        private readonly Mock<IManageUserRoleRightAsync> _mockManageUserRoleRight;
        private PersonaController _personaController;

        #endregion

        #region Constructor

        public PersonaControllerTests()
        {
            _mockUserClaimsAccessor = MockUserClaimsAccessor;
            _mockManagePersona = new Mock<IManagePersonaAsync>();
            _mockProductService = new Mock<IProductService>();
            _mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepositoryAsync>();
            _mockManageUserRoleRight = new Mock<IManageUserRoleRightAsync>();

            _personaController = new PersonaController(
                _mockUserClaimsAccessor.Object,
                _mockManagePersona.Object,
                _mockProductService.Object,
                _mockProductInternalSettingRepository.Object,
                _mockManageUserRoleRight.Object)
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
            var controller = new PersonaController(
                _mockUserClaimsAccessor.Object,
                _mockManagePersona.Object,
                _mockProductService.Object,
                _mockProductInternalSettingRepository.Object,
                _mockManageUserRoleRight.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PersonaController(
                null!,
                _mockManagePersona.Object,
                _mockProductService.Object,
                _mockProductInternalSettingRepository.Object,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePersona_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PersonaController(
                _mockUserClaimsAccessor.Object,
                null!,
                _mockProductService.Object,
                _mockProductInternalSettingRepository.Object,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullProductService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PersonaController(
                _mockUserClaimsAccessor.Object,
                _mockManagePersona.Object,
                null!,
                _mockProductInternalSettingRepository.Object,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullProductInternalSettingRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PersonaController(
                _mockUserClaimsAccessor.Object,
                _mockManagePersona.Object,
                _mockProductService.Object,
                null!,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManageUserRoleRight_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PersonaController(
                _mockUserClaimsAccessor.Object,
                _mockManagePersona.Object,
                _mockProductService.Object,
                _mockProductInternalSettingRepository.Object,
                null!));
        }

        #endregion

        #region GetPersonaEnvironmentType Tests

        [Fact]
        public async Task GetPersonaEnvironmentType_WithData_ReturnsOkResult()
        {
            // Arrange
            var environmentList = new List<PersonaEnvironment>
            {
                new PersonaEnvironment { PersonaEnvironmentTypeId = 1, Name = "Production" },
                new PersonaEnvironment { PersonaEnvironmentTypeId = 2, Name = "Test" }
            };

            _mockManagePersona
                .Setup(x => x.GetPersonaEnvironmentTypeAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(environmentList);

            // Act
            var result = await _personaController.GetPersonaEnvironmentType();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<PersonaEnvironment, IErrorData>>(okResult.Value);
            Assert.Equal(2, output.list.Count);
        }

        [Fact]
        public async Task GetPersonaEnvironmentType_WithNoData_ReturnsNoContent()
        {
            // Arrange
            _mockManagePersona
                .Setup(x => x.GetPersonaEnvironmentTypeAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PersonaEnvironment>());

            // Act
            var result = await _personaController.GetPersonaEnvironmentType();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetPersonaEnvironmentType_WithNullData_ReturnsNoContent()
        {
            // Arrange
            _mockManagePersona
                .Setup(x => x.GetPersonaEnvironmentTypeAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((IList<PersonaEnvironment>)null!);

            // Act
            var result = await _personaController.GetPersonaEnvironmentType();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        #endregion

        #region CreatePersona Tests - Success

        [Fact]
        public async Task CreatePersona_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var personRealPageId = Guid.NewGuid();
            var organizationRealPageId = Guid.NewGuid();
            var persona = new Persona { Name = "Test Persona" };

            _mockManagePersona
                .Setup(x => x.CreatePersonaAsync(personRealPageId, organizationRealPageId, persona, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 12345 });

            // Act
            var result = await _personaController.CreatePersona(personRealPageId, organizationRealPageId, persona);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IPersona, IErrorData>>(okResult.Value);
            Assert.Equal(12345, output.obj.PersonaId);
        }

        [Fact]
        public async Task CreatePersona_WithEmptyPersonRealPageId_UsesUserClaimsId()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            var organizationRealPageId = Guid.NewGuid();
            var persona = new Persona { Name = "Test Persona" };

            _mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(userRealPageId);

            _mockManagePersona
                .Setup(x => x.CreatePersonaAsync(userRealPageId, organizationRealPageId, persona, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _personaController.CreatePersona(Guid.Empty, organizationRealPageId, persona);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockManagePersona.Verify(
                x => x.CreatePersonaAsync(userRealPageId, organizationRealPageId, persona, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region CreatePersona Tests - BadRequest

        [Fact]
        public async Task CreatePersona_WithEmptyPersonRealPageIdAndEmptyUserClaims_ReturnsBadRequest()
        {
            // Arrange
            var organizationRealPageId = Guid.NewGuid();
            var persona = new Persona { Name = "Test Persona" };

            _mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            // Act
            var result = await _personaController.CreatePersona(Guid.Empty, organizationRealPageId, persona);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IPersona, IErrorData>>(badRequestResult.Value);
            Assert.Equal("Invalid parameter: personRealPageId", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task CreatePersona_WithEmptyOrganizationRealPageId_ReturnsBadRequest()
        {
            // Arrange
            var personRealPageId = Guid.NewGuid();
            var persona = new Persona { Name = "Test Persona" };

            // Act
            var result = await _personaController.CreatePersona(personRealPageId, Guid.Empty, persona);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IPersona, IErrorData>>(badRequestResult.Value);
            Assert.Equal("Invalid parameter Organization realPageId", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task CreatePersona_WithNullPersona_ReturnsBadRequest()
        {
            // Arrange
            var personRealPageId = Guid.NewGuid();
            var organizationRealPageId = Guid.NewGuid();

            // Act
            var result = await _personaController.CreatePersona(personRealPageId, organizationRealPageId, null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IPersona, IErrorData>>(badRequestResult.Value);
            Assert.Equal("Null parameter: Persona.", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task CreatePersona_WhenRepositoryReturnsZeroId_ReturnsBadRequest()
        {
            // Arrange
            var personRealPageId = Guid.NewGuid();
            var organizationRealPageId = Guid.NewGuid();
            var persona = new Persona { Name = "Test Persona" };
            const string errorMessage = "Failed to create persona";

            _mockManagePersona
                .Setup(x => x.CreatePersonaAsync(personRealPageId, organizationRealPageId, persona, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            // Act
            var result = await _personaController.CreatePersona(personRealPageId, organizationRealPageId, persona);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IPersona, IErrorData>>(badRequestResult.Value);
            Assert.Equal(errorMessage, output.Status.ErrorMsg);
        }

        #endregion

        #region GetPersona Tests

        [Fact]
        public async Task GetPersona_WithValidPersonaId_ReturnsOkWithPersona()
        {
            // Arrange
            const long personaId = 12345;
            var expectedPersona = CreateValidPersona();
            expectedPersona.PersonaId = personaId;

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(personaId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPersona);

            _mockManagePersona
                .Setup(x => x.ListActivePersonaAsync(expectedPersona.RealPageId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Persona> { expectedPersona });

            // Act
            var result = await _personaController.GetPersona(personaId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var persona = Assert.IsType<Persona>(okResult.Value);
            Assert.Equal(personaId, persona.PersonaId);
        }

        [Fact]
        public async Task GetPersona_WithZeroPersonaId_UsesClaimsPersonaId()
        {
            // Arrange
            const long claimsPersonaId = 99999;
            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(claimsPersonaId);

            var expectedPersona = CreateValidPersona();
            expectedPersona.PersonaId = claimsPersonaId;

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(claimsPersonaId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPersona);

            _mockManagePersona
                .Setup(x => x.ListActivePersonaAsync(expectedPersona.RealPageId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Persona> { expectedPersona });

            // Act
            var result = await _personaController.GetPersona(0);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockManagePersona.Verify(x => x.GetPersonaAsync(claimsPersonaId, true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPersona_WhenPersonaNotFound_ReturnsNoContent()
        {
            // Arrange
            const long personaId = 12345;

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(personaId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Persona)null!);

            // Act
            var result = await _personaController.GetPersona(personaId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetPersona_SetsHasMultiPersonaCorrectly()
        {
            // Arrange
            const long personaId = 12345;
            var expectedPersona = CreateValidPersona();
            expectedPersona.PersonaId = personaId;
            expectedPersona.OrganizationPartyId = 100;

            var personaList = new List<Persona>
            {
                expectedPersona,
                new Persona { PersonaId = 2, OrganizationPartyId = 100, Organization = new Organization { RealPageId = Guid.NewGuid() } }
            };

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(personaId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPersona);

            _mockManagePersona
                .Setup(x => x.ListActivePersonaAsync(expectedPersona.RealPageId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(personaList);

            // Act
            var result = await _personaController.GetPersona(personaId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var persona = Assert.IsType<Persona>(okResult.Value);
            Assert.True(persona.hasMultiPersona);
        }

        [Fact]
        public async Task GetPersona_SetsHasMultiCompanyCorrectly()
        {
            // Arrange
            const long personaId = 12345;
            var expectedPersona = CreateValidPersona();
            expectedPersona.PersonaId = personaId;
            expectedPersona.OrganizationPartyId = 100;

            var personaList = new List<Persona>
            {
                expectedPersona,
                new Persona
                {
                    PersonaId = 2,
                    OrganizationPartyId = 200,
                    Organization = new Organization { RealPageId = Guid.NewGuid() }
                }
            };

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(personaId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPersona);

            _mockManagePersona
                .Setup(x => x.ListActivePersonaAsync(expectedPersona.RealPageId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(personaList);

            // Act
            var result = await _personaController.GetPersona(personaId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var persona = Assert.IsType<Persona>(okResult.Value);
            Assert.True(persona.hasMultiCompany);
        }

        #endregion

        #region ChangeCompany Tests

        [Fact]
        public async Task ChangeCompany_WithValidPersonaId_ReturnsAccepted()
        {
            // Arrange
            const long personaId = 12345;
            const long claimsPersonaId = 99999;
            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(claimsPersonaId);
            _mockUserClaimsAccessor.Setup(x => x.ClientCode).Returns("TestClient");

            var settings = new List<ProductInternalSetting>
            {
                new ProductInternalSetting { Name = "UnifiedLoginServerClientName", Value = "UnifiedLoginClient" }
            };

            _mockProductInternalSettingRepository
                .Setup(x => x.GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, It.IsAny<CancellationToken>()))
                .ReturnsAsync(settings);

            var currentPersona = CreateValidPersona();
            currentPersona.PersonaId = claimsPersonaId;

            var personaList = new List<Persona>
            {
                currentPersona,
                new Persona { PersonaId = personaId, Organization = new Organization { RealPageId = Guid.NewGuid() } }
            };

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(claimsPersonaId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentPersona);

            _mockManagePersona
                .Setup(x => x.ListActivePersonaAsync(currentPersona.RealPageId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(personaList);

            _mockManagePersona
                .Setup(x => x.ChangeCompanyNotificationAsync(personaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid());

            // Act
            var result = await _personaController.ChangeCompany(personaId);

            // Assert
            Assert.IsType<AcceptedResult>(result);
        }

        [Fact]
        public async Task ChangeCompany_WhenCurrentPersonaNotFound_ReturnsBadRequest()
        {
            // Arrange
            const long personaId = 12345;
            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(99999);
            _mockUserClaimsAccessor.Setup(x => x.ClientCode).Returns("TestClient");

            _mockProductInternalSettingRepository
                .Setup(x => x.GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductInternalSetting>());

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(99999, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Persona)null!);

            // Act
            var result = await _personaController.ChangeCompany(personaId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Current persona not found", badRequestResult.Value);
        }

        [Fact]
        public async Task ChangeCompany_WhenPersonaNotInList_ReturnsUnauthorized()
        {
            // Arrange
            const long personaId = 12345;
            const long claimsPersonaId = 99999;
            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(claimsPersonaId);
            _mockUserClaimsAccessor.Setup(x => x.ClientCode).Returns("TestClient");

            _mockProductInternalSettingRepository
                .Setup(x => x.GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductInternalSetting>());

            var currentPersona = CreateValidPersona();
            currentPersona.PersonaId = claimsPersonaId;

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(claimsPersonaId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentPersona);

            _mockManagePersona
                .Setup(x => x.ListActivePersonaAsync(currentPersona.RealPageId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Persona> { currentPersona });

            // Act
            var result = await _personaController.ChangeCompany(personaId);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task ChangeCompany_WhenNotificationReturnsEmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            const long personaId = 12345;
            const long claimsPersonaId = 99999;
            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(claimsPersonaId);
            _mockUserClaimsAccessor.Setup(x => x.ClientCode).Returns("TestClient");

            _mockProductInternalSettingRepository
                .Setup(x => x.GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductInternalSetting>());

            var currentPersona = CreateValidPersona();
            currentPersona.PersonaId = claimsPersonaId;

            var personaList = new List<Persona>
            {
                currentPersona,
                new Persona { PersonaId = personaId, Organization = new Organization { RealPageId = Guid.NewGuid() } }
            };

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(claimsPersonaId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentPersona);

            _mockManagePersona
                .Setup(x => x.ListActivePersonaAsync(currentPersona.RealPageId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(personaList);

            _mockManagePersona
                .Setup(x => x.ChangeCompanyNotificationAsync(personaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.Empty);

            // Act
            var result = await _personaController.ChangeCompany(personaId);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task ChangeCompany_WithUnifiedLoginClient_UsesProvidedPersonaId()
        {
            // Arrange
            const long personaId = 12345;
            const long claimsPersonaId = 99999;
            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(claimsPersonaId);
            _mockUserClaimsAccessor.Setup(x => x.ClientCode).Returns("UnifiedLoginClient");

            var settings = new List<ProductInternalSetting>
            {
                new ProductInternalSetting { Name = "UnifiedLoginServerClientName", Value = "UnifiedLoginClient" }
            };

            _mockProductInternalSettingRepository
                .Setup(x => x.GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, It.IsAny<CancellationToken>()))
                .ReturnsAsync(settings);

            var currentPersona = CreateValidPersona();
            currentPersona.PersonaId = claimsPersonaId;

            var personaList = new List<Persona>
            {
                currentPersona,
                new Persona { PersonaId = personaId, Organization = new Organization { RealPageId = Guid.NewGuid() } }
            };

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(claimsPersonaId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentPersona);

            _mockManagePersona
                .Setup(x => x.ListActivePersonaAsync(currentPersona.RealPageId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(personaList);

            _mockManagePersona
                .Setup(x => x.ChangeCompanyNotificationAsync(personaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid());

            // Act
            var result = await _personaController.ChangeCompany(personaId);

            // Assert
            Assert.IsType<AcceptedResult>(result);
            _mockManagePersona.Verify(x => x.ChangeCompanyNotificationAsync(personaId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ChangeCompany_WithZeroPersonaIdAndNonUnifiedLoginClient_UsesClaimsPersonaId()
        {
            // Arrange
            const long claimsPersonaId = 99999;
            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(claimsPersonaId);
            _mockUserClaimsAccessor.Setup(x => x.ClientCode).Returns("OtherClient");

            var settings = new List<ProductInternalSetting>
            {
                new ProductInternalSetting { Name = "UnifiedLoginServerClientName", Value = "UnifiedLoginClient" }
            };

            _mockProductInternalSettingRepository
                .Setup(x => x.GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, It.IsAny<CancellationToken>()))
                .ReturnsAsync(settings);

            var currentPersona = CreateValidPersona();
            currentPersona.PersonaId = claimsPersonaId;

            var personaList = new List<Persona> { currentPersona };

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(claimsPersonaId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentPersona);

            _mockManagePersona
                .Setup(x => x.ListActivePersonaAsync(currentPersona.RealPageId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(personaList);

            _mockManagePersona
                .Setup(x => x.ChangeCompanyNotificationAsync(claimsPersonaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid());

            // Act
            var result = await _personaController.ChangeCompany(0);

            // Assert
            Assert.IsType<AcceptedResult>(result);
            _mockManagePersona.Verify(x => x.ChangeCompanyNotificationAsync(claimsPersonaId, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region GetPersonasList Tests

        [Fact]
        public async Task GetPersonasList_WithValidData_ReturnsOkWithCompanyList()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            _mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(userRealPageId);

            var personaList = new List<Persona>
            {
                new Persona
                {
                    PersonaId = 1,
                    Name = "Persona 1",
                    Organization = new Organization { Name = "Company A", RealPageId = Guid.NewGuid() }
                },
                new Persona
                {
                    PersonaId = 2,
                    Name = "Persona 2",
                    Organization = new Organization { Name = "Company B", RealPageId = Guid.NewGuid() }
                }
            };

            _mockManagePersona
                .Setup(x => x.ListActivePersonaAsync(userRealPageId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(personaList);

            // Act
            var result = await _personaController.GetPersonasList();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<PersonaCompany, IErrorData>>(okResult.Value);
            Assert.Equal(2, output.list.Count);
        }

        [Fact]
        public async Task GetPersonasList_ExcludesExternalCompany()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            _mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(userRealPageId);

            var personaList = new List<Persona>
            {
                new Persona
                {
                    PersonaId = 1,
                    Name = "Persona 1",
                    Organization = new Organization { Name = "Company A", RealPageId = Guid.NewGuid() }
                },
                new Persona
                {
                    PersonaId = 2,
                    Name = "External Persona",
                    Organization = new Organization { Name = "External Company", RealPageId = DefaultUserClaim.ExternalCompanyRealPageId }
                }
            };

            _mockManagePersona
                .Setup(x => x.ListActivePersonaAsync(userRealPageId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(personaList);

            // Act
            var result = await _personaController.GetPersonasList();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<PersonaCompany, IErrorData>>(okResult.Value);
            Assert.Single(output.list);
        }

        [Fact]
        public async Task GetPersonasList_GroupsPersonasByCompany()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            var companyRealPageId = Guid.NewGuid();
            _mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(userRealPageId);

            var personaList = new List<Persona>
            {
                new Persona
                {
                    PersonaId = 1,
                    Name = "Persona 1",
                    Organization = new Organization { Name = "Company A", RealPageId = companyRealPageId }
                },
                new Persona
                {
                    PersonaId = 2,
                    Name = "Persona 2",
                    Organization = new Organization { Name = "Company A", RealPageId = companyRealPageId }
                }
            };

            _mockManagePersona
                .Setup(x => x.ListActivePersonaAsync(userRealPageId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(personaList);

            // Act
            var result = await _personaController.GetPersonasList();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<PersonaCompany, IErrorData>>(okResult.Value);
            Assert.Single(output.list);
            Assert.Equal(2, output.list[0].Personas.Count);
        }

        #endregion

        #region GetProductsByPersona Tests

        [Fact]
        public async Task GetProductsByPersona_WithValidPersona_ReturnsOkWithProducts()
        {
            // Arrange
            const long personaId = 12345;
            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);

            var persona = CreateValidPersona();
            persona.PersonaId = personaId;

            var productList = new List<PersonaProductUserDetails>
            {
                new PersonaProductUserDetails { PersonaId = personaId, OrganizationName = "Test Org" }
            };

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(personaId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(persona);

            _mockProductService
                .Setup(x => x.GetAssignedProductsByPersonaAsync(persona, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productList);

            // Act
            var result = await _personaController.GetProductsByPersona();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<PersonaProductUserDetails, IErrorData>>(okResult.Value);
            Assert.Single(output.list);
        }

        [Fact]
        public async Task GetProductsByPersona_WhenPersonaNotFound_ReturnsBadRequest()
        {
            // Arrange
            const long personaId = 12345;
            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(personaId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Persona)null!);

            // Act
            var result = await _personaController.GetProductsByPersona();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<PersonaProductUserDetails, IErrorData>>(badRequestResult.Value);
            Assert.Equal("Active persona not found!", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task GetProductsByPersona_WithProductSelectType_PassesToService()
        {
            // Arrange
            const long personaId = 12345;
            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);

            var persona = CreateValidPersona();

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(personaId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(persona);

            _mockProductService
                .Setup(x => x.GetAssignedProductsByPersonaAsync(persona, ProductSelectType.FavoritesOnly, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PersonaProductUserDetails>());

            // Act
            await _personaController.GetProductsByPersona(ProductSelectType.FavoritesOnly);

            // Assert
            _mockProductService.Verify(
                x => x.GetAssignedProductsByPersonaAsync(persona, ProductSelectType.FavoritesOnly, null, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region UpdateUserProductSetting Tests

        [Fact]
        public async Task UpdateUserProductSetting_WithValidData_ReturnsOkResult()
        {
            // Arrange
            const int productId = 1;
            var productSetting = new ProductSetting { ProductId = productId };
            const long personaId = 12345;
            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);

            var persona = CreateValidPersona();
            persona.PersonaId = personaId;

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(personaId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(persona);

            _mockProductService
                .Setup(x => x.UpdateProductSettingAsync(productSetting, personaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _personaController.UpdateUserProductSetting(productId, productSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<RepositoryResponse, IErrorData>>(okResult.Value);
            Assert.Equal(1, output.obj.Id);
        }

        [Fact]
        public async Task UpdateUserProductSetting_WithNullProductId_ReturnsBadRequest()
        {
            // Arrange
            var productSetting = new ProductSetting();

            // Act
            var result = await _personaController.UpdateUserProductSetting(null, productSetting);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<RepositoryResponse, IErrorData>>(badRequestResult.Value);
            Assert.Equal("Null parameter: productId.", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task UpdateUserProductSetting_WithNullProductSetting_ReturnsBadRequest()
        {
            // Act
            var result = await _personaController.UpdateUserProductSetting(1, null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<RepositoryResponse, IErrorData>>(badRequestResult.Value);
            Assert.Equal("Null parameter: productSetting.", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task UpdateUserProductSetting_WhenPersonaNotFound_ReturnsBadRequest()
        {
            // Arrange
            const int productId = 1;
            var productSetting = new ProductSetting();
            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(12345);

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(12345, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Persona)null!);

            // Act
            var result = await _personaController.UpdateUserProductSetting(productId, productSetting);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<RepositoryResponse, IErrorData>>(badRequestResult.Value);
            Assert.Equal("Active persona not found!", output.Status.ErrorMsg);
        }

        #endregion

        #region GetPersonaRolesByProduct Tests

        [Fact]
        public async Task GetPersonaRolesByProduct_WithValidData_ReturnsOkWithRoles()
        {
            // Arrange
            const long personaId = 12345;
            const ProductEnum productId = ProductEnum.UnifiedPlatform;
            var roles = new List<Role>
            {
                new Role { RoleID = 1, Name = "Admin" },
                new Role { RoleID = 2, Name = "User" }
            };

            _mockManageUserRoleRight
                .Setup(x => x.GetAssignedRoleForPersonaAsync(productId, personaId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(roles);

            // Act
            var result = await _personaController.GetPersonaRolesByProduct(personaId, productId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var roleList = Assert.IsAssignableFrom<IList<Role>>(okResult.Value);
            Assert.Equal(2, roleList.Count);
        }

        [Fact]
        public async Task GetPersonaRolesByProduct_WithZeroPersonaId_ReturnsBadRequest()
        {
            // Act
            var result = await _personaController.GetPersonaRolesByProduct(0, ProductEnum.UnifiedPlatform);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid personaId or productId", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPersonaRolesByProduct_WithZeroProductId_ReturnsBadRequest()
        {
            // Act
            var result = await _personaController.GetPersonaRolesByProduct(12345, 0);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid personaId or productId", badRequestResult.Value);
        }

        #endregion

        #region Helper Methods

        private static Persona CreateValidPersona()
        {
            return new Persona
            {
                PersonaId = 12345,
                Name = "Test Persona",
                RealPageId = Guid.NewGuid(),
                OrganizationPartyId = 100,
                Organization = new Organization
                {
                    Name = "Test Organization",
                    RealPageId = Guid.NewGuid(),
                    PartyId = 100
                }
            };
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _personaController = null!;
            base.Dispose();
        }

        #endregion
    }
}
