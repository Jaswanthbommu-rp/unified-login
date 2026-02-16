using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.Helpers;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManagePersona business logic xUnit tests.
    /// Tests for persona management operations including CRUD operations and persona switching.
    /// Migrated from RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.ManagePersonaTest
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManagePersonaTests : TestBase
    {
        private readonly Mock<IPersonaRepository> _mockPersonaRepository;
        private readonly Mock<IRepository> _mockRepository;
        private readonly Mock<ITokenHelper> _mockTokenHelper;
        private readonly Mock<IProductInternalSettingRepository> _mockProductRepository;
        private readonly DefaultUserClaim _defaultUserClaim;

        public ManagePersonaTests()
        {
            _mockPersonaRepository = new Mock<IPersonaRepository>();
            _mockRepository = new Mock<IRepository>();
            _mockTokenHelper = new Mock<ITokenHelper>();
            _mockProductRepository = new Mock<IProductInternalSettingRepository>();

            _defaultUserClaim = new DefaultUserClaim
            {
                UserId = 1,
                LoginName = "testuser@test.com",
                FirstName = "Test",
                LastName = "User",
                UserRealPageGuid = Guid.Parse("F5C090FA-78AB-452F-B504-98AAFEE09121"),
                OrganizationRealPageGuid = Guid.Parse("A5C090FA-78AB-452F-B504-98AAFEE09122"),
                OrganizationMasterId = 379,
                OrganizationPartyId = 1000,
                PersonaId = 5,
                CorrelationId = Guid.NewGuid(),
                OrganizationName = "Test Organization",
                RealPageEmployee = false
            };
        }

        #region Helper Methods

        private Persona CreateValidPersona()
        {
            return new Persona
            {
                PersonaId = 1,
                RealPageId = Guid.NewGuid(),
                OrganizationPartyId = 1000,
                IsDefault = true,
                PersonaName = "Test Persona"
            };
        }

        private IPersona CreateValidIPersona()
        {
            return new Persona
            {
                PersonaId = 1,
                RealPageId = Guid.NewGuid(),
                OrganizationPartyId = 1000,
                IsDefault = true,
                PersonaName = "Test IPersona"
            };
        }

        private List<PersonaEnvironment> CreatePersonaEnvironments()
        {
            return new List<PersonaEnvironment>
            {
                new PersonaEnvironment { PersonaEnvironmentTypeId = 1, Name = "Production" },
                new PersonaEnvironment { PersonaEnvironmentTypeId = 2, Name = "Staging" }
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNoParameters_InitializesSuccessfully()
        {
            // Arrange & Act
            var managePersona = new ManagePersona();

            // Assert
            Assert.NotNull(managePersona);
        }

        [Fact]
        public void Constructor_WithPersonaRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Assert
            Assert.NotNull(managePersona);
        }

        [Fact]
        public void Constructor_WithRepositoryAndUserClaim_InitializesSuccessfully()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();

            // Act
            var managePersona = new ManagePersona(_mockRepository.Object, _defaultUserClaim, mockHandler.Object);

            // Assert
            Assert.NotNull(managePersona);
        }

        [Fact]
        public void Constructor_WithUserClaim_InitializesSuccessfully()
        {
            // Arrange & Act
            var managePersona = new ManagePersona(_defaultUserClaim);

            // Assert
            Assert.NotNull(managePersona);
        }

        #endregion

        #region GetPersonaEnvironmentType Tests

        [Fact]
        public void GetPersonaEnvironmentType_ReturnsEnvironmentList()
        {
            // Arrange
            var expectedEnvironments = CreatePersonaEnvironments();
            _mockPersonaRepository
                .Setup(x => x.GetPersonaEnvironmentType())
                .Returns(expectedEnvironments);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.GetPersonaEnvironmentType();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _mockPersonaRepository.Verify(x => x.GetPersonaEnvironmentType(), Times.Once);
        }

        #endregion

        #region CreatePersona Tests

        [Fact]
        public void CreatePersona_WithValidParameters_ReturnsSuccessResponse()
        {
            // Arrange
            var personRealPageId = Guid.NewGuid();
            var organizationRealPageId = Guid.NewGuid();
            var persona = CreateValidIPersona();
            var expectedResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockPersonaRepository
                .Setup(x => x.CreatePersona(personRealPageId, organizationRealPageId, persona))
                .Returns(expectedResponse);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.CreatePersona(personRealPageId, organizationRealPageId, persona);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Empty(result.ErrorMessage);
        }

        [Fact]
        public void CreatePersona_WithEmptyPersonRealPageId_ThrowsException()
        {
            // Arrange
            var personRealPageId = Guid.Empty;
            var organizationRealPageId = Guid.NewGuid();
            var persona = CreateValidIPersona();
            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePersona.CreatePersona(personRealPageId, organizationRealPageId, persona));

            Assert.Equal("Invalid parameter Person realPageId.", exception.Message);
        }

        [Fact]
        public void CreatePersona_WithEmptyOrganizationRealPageId_ThrowsException()
        {
            // Arrange
            var personRealPageId = Guid.NewGuid();
            var organizationRealPageId = Guid.Empty;
            var persona = CreateValidIPersona();
            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePersona.CreatePersona(personRealPageId, organizationRealPageId, persona));

            Assert.Equal("Invalid parameter Organization realPageId.", exception.Message);
        }

        [Fact]
        public void CreatePersona_WithNullPersona_ThrowsArgumentNullException()
        {
            // Arrange
            var personRealPageId = Guid.NewGuid();
            var organizationRealPageId = Guid.NewGuid();
            IPersona persona = null;
            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                managePersona.CreatePersona(personRealPageId, organizationRealPageId, persona));

            Assert.Equal("persona", exception.ParamName);
        }

        #endregion

        #region CreateAdditionalPersona Tests

        [Fact]
        public void CreateAdditionalPersona_WithValidParameters_ReturnsSuccessResponse()
        {
            // Arrange
            var organizationRealPageId = Guid.NewGuid();
            long userId = 100;
            long createdBy = 200;
            string personaName = "Additional Persona";
            var expectedResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockPersonaRepository
                .Setup(x => x.CreateAdditionalPersona(organizationRealPageId, userId, createdBy, personaName))
                .Returns(expectedResponse);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.CreateAdditionalPersona(organizationRealPageId, userId, createdBy, personaName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void CreateAdditionalPersona_WithZeroUserId_ThrowsException()
        {
            // Arrange
            var organizationRealPageId = Guid.NewGuid();
            long userId = 0;
            long createdBy = 200;
            string personaName = "Test";
            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePersona.CreateAdditionalPersona(organizationRealPageId, userId, createdBy, personaName));

            Assert.Equal("Invalid parameter UserId.", exception.Message);
        }

        [Fact]
        public void CreateAdditionalPersona_WithEmptyOrganizationRealPageId_ThrowsException()
        {
            // Arrange
            var organizationRealPageId = Guid.Empty;
            long userId = 100;
            long createdBy = 200;
            string personaName = "Test";
            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePersona.CreateAdditionalPersona(organizationRealPageId, userId, createdBy, personaName));

            Assert.Equal("Invalid parameter Organization realPageId.", exception.Message);
        }

        #endregion

        #region GetPersona Tests

        [Fact]
        public void GetPersona_WithValidPersonaId_ReturnsPersona()
        {
            // Arrange
            long personaId = 1;
            var expectedPersona = CreateValidPersona();

            _mockPersonaRepository
                .Setup(x => x.GetPersona(personaId))
                .Returns(expectedPersona);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.GetPersona(personaId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(personaId, result.PersonaId);
        }

        [Fact]
        public void GetPersona_WithZeroPersonaId_ThrowsException()
        {
            // Arrange
            long personaId = 0;
            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePersona.GetPersona(personaId));

            Assert.Equal("Invalid parameter personaId.", exception.Message);
        }

        #endregion

        #region GetPersonaWithRightsToggle Tests

        [Fact]
        public void GetPersonaWithRightsToggle_WithRightsTrue_ReturnsPersona()
        {
            // Arrange
            long personaId = 1;
            var expectedPersona = CreateValidPersona();

            _mockPersonaRepository
                .Setup(x => x.GetPersona(personaId, true))
                .Returns(expectedPersona);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.GetPersonaWithRightsToggle(personaId, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(personaId, result.PersonaId);
            _mockPersonaRepository.Verify(x => x.GetPersona(personaId, true), Times.Once);
        }

        [Fact]
        public void GetPersonaWithRightsToggle_WithRightsFalse_ReturnsPersona()
        {
            // Arrange
            long personaId = 1;
            var expectedPersona = CreateValidPersona();

            _mockPersonaRepository
                .Setup(x => x.GetPersona(personaId, false))
                .Returns(expectedPersona);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.GetPersonaWithRightsToggle(personaId, false);

            // Assert
            Assert.NotNull(result);
            _mockPersonaRepository.Verify(x => x.GetPersona(personaId, false), Times.Once);
        }

        [Fact]
        public void GetPersonaWithRightsToggle_WithZeroPersonaId_ThrowsException()
        {
            // Arrange
            long personaId = 0;
            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePersona.GetPersonaWithRightsToggle(personaId));

            Assert.Equal("Invalid parameter personaId.", exception.Message);
        }

        #endregion

        #region ListPersona Tests

        [Fact]
        public void ListPersona_WithValidRealPageId_ReturnsPersonaList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var expectedPersonas = new List<Persona> { CreateValidPersona(), CreateValidPersona() };

            _mockPersonaRepository
                .Setup(x => x.ListPersona(realPageId))
                .Returns(expectedPersonas);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.ListPersona(realPageId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void ListPersona_WithEmptyRealPageId_ThrowsException()
        {
            // Arrange
            var realPageId = Guid.Empty;
            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePersona.ListPersona(realPageId));

            Assert.Equal("Invalid parameter realPageId.", exception.Message);
        }

        #endregion

        #region ListActivePersona Tests

        [Fact]
        public void ListActivePersona_WithIncludeOrganizationTrue_ReturnsPersonaList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var expectedPersonas = new List<Persona> { CreateValidPersona() };

            _mockPersonaRepository
                .Setup(x => x.ListActivePersona(realPageId, true))
                .Returns(expectedPersonas);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.ListActivePersona(realPageId, true);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void ListActivePersona_WithIncludeOrganizationFalse_ReturnsPersonaList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var expectedPersonas = new List<Persona> { CreateValidPersona() };

            _mockPersonaRepository
                .Setup(x => x.ListActivePersona(realPageId, false))
                .Returns(expectedPersonas);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.ListActivePersona(realPageId, false);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void ListActivePersona_WithEmptyRealPageId_ThrowsException()
        {
            // Arrange
            var realPageId = Guid.Empty;
            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePersona.ListActivePersona(realPageId, true));

            Assert.Equal("Invalid parameter realPageId.", exception.Message);
        }

        #endregion

        #region ListEmployeePersonas Tests

        [Fact]
        public void ListEmployeePersonas_WithValidParameters_ReturnsPersonaList()
        {
            // Arrange
            long userId = 100;
            long orgPartyId = 1000;
            var expectedPersonas = new List<Persona> { CreateValidPersona() };

            _mockPersonaRepository
                .Setup(x => x.ListEmployeePersonas(userId, orgPartyId))
                .Returns(expectedPersonas);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.ListEmployeePersonas(userId, orgPartyId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void ListEmployeePersonas_WithZeroUserId_ThrowsException()
        {
            // Arrange
            long userId = 0;
            long orgPartyId = 1000;
            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePersona.ListEmployeePersonas(userId, orgPartyId));

            Assert.Equal("Invalid parameter userId.", exception.Message);
        }

        [Fact]
        public void ListEmployeePersonas_WithZeroOrgPartyId_ThrowsException()
        {
            // Arrange
            long userId = 100;
            long orgPartyId = 0;
            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePersona.ListEmployeePersonas(userId, orgPartyId));

            Assert.Equal("Invalid parameter orgPartyId.", exception.Message);
        }

        #endregion

        #region ListPersonaByOrganizationPartyId Tests

        [Fact]
        public void ListPersonaByOrganizationPartyId_WithValidPartyId_ReturnsPersonaList()
        {
            // Arrange
            long organizationPartyId = 1000;
            var expectedPersonas = new List<Persona> { CreateValidPersona() };

            _mockPersonaRepository
                .Setup(x => x.ListPersonaByOrganizationPartyId(organizationPartyId, null, null))
                .Returns(expectedPersonas);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.ListPersonaByOrganizationPartyId(organizationPartyId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void ListPersonaByOrganizationPartyId_WithIsDefaultTrue_ReturnsPersonaList()
        {
            // Arrange
            long organizationPartyId = 1000;
            var expectedPersonas = new List<Persona> { CreateValidPersona() };

            _mockPersonaRepository
                .Setup(x => x.ListPersonaByOrganizationPartyId(organizationPartyId, true, null))
                .Returns(expectedPersonas);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.ListPersonaByOrganizationPartyId(organizationPartyId, true);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void ListPersonaByOrganizationPartyId_WithZeroPartyId_ThrowsException()
        {
            // Arrange
            long organizationPartyId = 0;
            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePersona.ListPersonaByOrganizationPartyId(organizationPartyId));

            Assert.Equal("Invalid parameter organizationPartyId.", exception.Message);
        }

        #endregion

        #region GetActivePersonaId Tests

        [Fact]
        public void GetActivePersonaId_WithValidRealPageId_ReturnsPersonaId()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            long expectedPersonaId = 5;

            _mockPersonaRepository
                .Setup(x => x.GetActivePersonaId(realPageId))
                .Returns(expectedPersonaId);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.GetActivePersonaId(realPageId);

            // Assert
            Assert.Equal(expectedPersonaId, result);
        }

        #endregion

        #region GetActivePersona Tests

        [Fact]
        public void GetActivePersona_WithValidRealPageId_ReturnsPersona()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            long personaId = 1;
            var expectedPersona = CreateValidPersona();

            _mockPersonaRepository
                .Setup(x => x.GetActivePersonaId(realPageId))
                .Returns(personaId);

            _mockPersonaRepository
                .Setup(x => x.GetPersona(personaId))
                .Returns(expectedPersona);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.GetActivePersona(realPageId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(personaId, result.PersonaId);
        }

        [Fact]
        public void GetActivePersona_WithNoActivePersona_ReturnsEmptyPersona()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            long personaId = 0;

            _mockPersonaRepository
                .Setup(x => x.GetActivePersonaId(realPageId))
                .Returns(personaId);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.GetActivePersona(realPageId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.PersonaId);
        }

        [Fact]
        public void GetActivePersona_WithEmptyRealPageId_ThrowsException()
        {
            // Arrange
            var realPageId = Guid.Empty;
            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePersona.GetActivePersona(realPageId));

            Assert.Equal("Invalid parameter realPageId.", exception.Message);
        }

        #endregion

        #region GetActivePersonaWithoutRights Tests

        [Fact]
        public void GetActivePersonaWithoutRights_WithValidRealPageId_ReturnsPersona()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            long personaId = 1;
            var expectedPersona = CreateValidPersona();

            _mockPersonaRepository
                .Setup(x => x.GetActivePersonaId(realPageId))
                .Returns(personaId);

            _mockPersonaRepository
                .Setup(x => x.GetPersona(personaId, false))
                .Returns(expectedPersona);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.GetActivePersonaWithoutRights(realPageId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(personaId, result.PersonaId);
            _mockPersonaRepository.Verify(x => x.GetPersona(personaId, false), Times.Once);
        }

        [Fact]
        public void GetActivePersonaWithoutRights_WithNoActivePersona_ReturnsEmptyPersona()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            long personaId = 0;

            _mockPersonaRepository
                .Setup(x => x.GetActivePersonaId(realPageId))
                .Returns(personaId);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.GetActivePersonaWithoutRights(realPageId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.PersonaId);
        }

        [Fact]
        public void GetActivePersonaWithoutRights_WithEmptyRealPageId_ThrowsException()
        {
            // Arrange
            var realPageId = Guid.Empty;
            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePersona.GetActivePersonaWithoutRights(realPageId));

            Assert.Equal("Invalid parameter realPageId.", exception.Message);
        }

        #endregion

        #region GetFirstAvailablePersonaByCompany Tests

        [Fact]
        public void GetFirstAvailablePersonaByCompany_WithMatchingOrgPartyId_ReturnsPersona()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            long orgPartyId = 1000;
            var persona = CreateValidPersona();
            persona.OrganizationPartyId = orgPartyId;
            var personaList = new List<Persona> { persona };

            _mockPersonaRepository
                .Setup(x => x.ListPersona(realPageId))
                .Returns(personaList);

            _mockPersonaRepository
                .Setup(x => x.GetPersona(persona.PersonaId))
                .Returns(persona);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.GetFirstAvailablePersonaByCompany(realPageId, orgPartyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(persona.PersonaId, result.PersonaId);
        }

        [Fact]
        public void GetFirstAvailablePersonaByCompany_WithEmptyPersonaList_ReturnsNull()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            long orgPartyId = 1000;
            var personaList = new List<Persona>();

            _mockPersonaRepository
                .Setup(x => x.ListPersona(realPageId))
                .Returns(personaList);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.GetFirstAvailablePersonaByCompany(realPageId, orgPartyId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region UpdateActivePersona Tests

        [Fact]
        public void UpdateActivePersona_WithValidParameters_ReturnsSuccessResponse()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            long personaId = 5;
            var expectedResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockPersonaRepository
                .Setup(x => x.UpdateActivePersona(realPageId, personaId))
                .Returns(expectedResponse);

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var result = managePersona.UpdateActivePersona(realPageId, personaId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void ManagePersona_CompleteWorkflow_CreateGetUpdateActive()
        {
            // Arrange
            var personRealPageId = Guid.NewGuid();
            var organizationRealPageId = Guid.NewGuid();
            var persona = CreateValidIPersona();
            long personaId = 1;

            _mockPersonaRepository
                .Setup(x => x.CreatePersona(personRealPageId, organizationRealPageId, persona))
                .Returns(new RepositoryResponse { Id = personaId, ErrorMessage = "" });

            _mockPersonaRepository
                .Setup(x => x.GetPersona(personaId))
                .Returns(CreateValidPersona());

            _mockPersonaRepository
                .Setup(x => x.UpdateActivePersona(personRealPageId, personaId))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

            var managePersona = new ManagePersona(_mockPersonaRepository.Object);

            // Act
            var createResult = managePersona.CreatePersona(personRealPageId, organizationRealPageId, persona);
            var getResult = managePersona.GetPersona(personaId);
            var updateResult = managePersona.UpdateActivePersona(personRealPageId, personaId);

            // Assert
            Assert.NotNull(createResult);
            Assert.NotNull(getResult);
            Assert.NotNull(updateResult);
            Assert.Equal(personaId, createResult.Id);
        }

        #endregion
    }
}
