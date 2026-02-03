using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for DashboardController.
    /// Tests all endpoints, error cases, and edge cases for 100% code coverage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DashboardControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private readonly Mock<IManagePersona> _mockManagePersona;
        private readonly Mock<IManageDashboardContent> _mockManageDashboardContent;
        private readonly Mock<IManageCredential> _mockManageCredential;
        private DashboardController _dashboardController;

        #endregion

        #region Constructor

        public DashboardControllerTests()
        {
            _mockUserClaimsAccessor = MockUserClaimsAccessor;
            _mockManagePersona = new Mock<IManagePersona>();
            _mockManageDashboardContent = new Mock<IManageDashboardContent>();
            _mockManageCredential = new Mock<IManageCredential>();

            _dashboardController = new DashboardController(
                _mockManagePersona.Object,
                _mockManageDashboardContent.Object,
                _mockManageCredential.Object,
                _mockUserClaimsAccessor.Object
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
            var controller = new DashboardController(
                _mockManagePersona.Object,
                _mockManageDashboardContent.Object,
                _mockManageCredential.Object,
                _mockUserClaimsAccessor.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new DashboardController(
                    _mockManagePersona.Object,
                    _mockManageDashboardContent.Object,
                    _mockManageCredential.Object,
                    null));

            Assert.Equal("userClaimsAccessor", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullManagePersona_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new DashboardController(
                    null!,
                    _mockManageDashboardContent.Object,
                    _mockManageCredential.Object,
                    _mockUserClaimsAccessor.Object));

            Assert.Equal("managePersona", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullManageDashboardContent_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new DashboardController(
                    _mockManagePersona.Object,
                    null!,
                    _mockManageCredential.Object,
                    _mockUserClaimsAccessor.Object));

            Assert.Equal("manageDashboardContent", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullManageCredential_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new DashboardController(
                    _mockManagePersona.Object,
                    _mockManageDashboardContent.Object,
                    null,
                    _mockUserClaimsAccessor.Object));

            Assert.Equal("manageCredential", exception.ParamName);
        }

        #endregion

        #region GetDashboardContent Tests - Success Scenarios

        [Fact]
        public async Task GetDashboardContent_WithValidClaimsAndPersona_ReturnsOkResult()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            var personaId = 12345L;
            var userClaim = new DefaultUserClaim
            {
                UserRealPageGuid = userRealPageId,
                PersonaId = personaId,
                UserId = 100
            };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var persona = new Persona
            {
                PersonaId = personaId,
                PersonaName = "Test Persona"
            };
            _mockManagePersona
                .Setup(x => x.GetPersona(personaId))
                .Returns(persona);

            var dashboardResponse = new DashboardElementResponse
            {
                IsError = false,
                DashboardElements = new DashboardElements
                {
                    ProfileDetail = new ProfileDetail(),
                    Resources = new List<PersonaProductUserDetails>(),
                    TrainingAchievements = new List<TrainingAchievement>()
                }
            };
            _mockManageDashboardContent
                .Setup(x => x.GetDashboardElementResponse(userRealPageId, persona))
                .Returns(dashboardResponse);

            var passwordResponse = new CheckPasswordExpirationResponse
            {
                IsPasswordExpired = false
            };
            _mockManageCredential
                .Setup(x => x.CheckPasswordExpiration(userClaim.UserId, userClaim.UserRealPageGuid))
                .Returns(passwordResponse);

            // Act
            var result = await _dashboardController.GetDashboardContent();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<DashboardElementResponse>(okResult.Value);
            Assert.NotNull(response.DashboardElements);
        }

        [Fact]
        public async Task GetDashboardContent_WithValidData_CallsAllDependencies()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            var personaId = 100L;
            var userId = 999;
            var userClaim = new DefaultUserClaim
            {
                UserRealPageGuid = userRealPageId,
                PersonaId = personaId,
                UserId = userId
            };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var persona = new Persona { PersonaId = personaId };
            _mockManagePersona.Setup(x => x.GetPersona(personaId)).Returns(persona);

            var dashboardResponse = new DashboardElementResponse
            {
                DashboardElements = new DashboardElements
                {
                    ProfileDetail = new ProfileDetail(),
                    Resources = new List<PersonaProductUserDetails>()
                }
            };
            _mockManageDashboardContent
                .Setup(x => x.GetDashboardElementResponse(userRealPageId, persona))
                .Returns(dashboardResponse);

            var passwordResponse = new CheckPasswordExpirationResponse { IsPasswordExpired = false };
            _mockManageCredential
                .Setup(x => x.CheckPasswordExpiration(userId, userRealPageId))
                .Returns(passwordResponse);

            // Act
            await _dashboardController.GetDashboardContent();

            // Assert
            _mockUserClaimsAccessor.Verify(x => x.GetUserClaim(), Times.Once);
            _mockManagePersona.Verify(x => x.GetPersona(personaId), Times.Once);
            _mockManageDashboardContent.Verify(x => x.GetDashboardElementResponse(userRealPageId, persona), Times.Once);
            _mockManageCredential.Verify(x => x.CheckPasswordExpiration(userId, userRealPageId), Times.Once);
        }

        #endregion

        #region GetDashboardContent Tests - Unauthorized Scenarios

        [Fact]
        public async Task GetDashboardContent_WithNullUserClaim_ReturnsUnauthorized()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            // Act
            var result = await _dashboardController.GetDashboardContent();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        #endregion

        #region GetDashboardContent Tests - BadRequest Scenarios

        [Fact]
        public async Task GetDashboardContent_WithNullPersona_ReturnsBadRequest()
        {
            // Arrange
            var userClaim = new DefaultUserClaim
            {
                UserRealPageGuid = Guid.NewGuid(),
                PersonaId = 12345,
                UserId = 100
            };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            _mockManagePersona
                .Setup(x => x.GetPersona(userClaim.PersonaId))
                .Returns((Persona)null!);

            // Act
            var result = await _dashboardController.GetDashboardContent();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Persona not found", badRequestResult.Value);
        }

        #endregion

        #region GetDashboardContent Tests - Password Expiration Scenarios

        [Fact]
        public async Task GetDashboardContent_WhenPasswordExpired_ClearsResourcesAndAssignedProducts()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            var personaId = 12345L;
            var userId = 100;
            var userClaim = new DefaultUserClaim
            {
                UserRealPageGuid = userRealPageId,
                PersonaId = personaId,
                UserId = userId
            };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var persona = new Persona { PersonaId = personaId };
            _mockManagePersona.Setup(x => x.GetPersona(personaId)).Returns(persona);

            var profileDetail = new ProfileDetail
            {
                AssignedProducts = new List<PersonaProductUserDetails> { new PersonaProductUserDetails() }
            };
            var dashboardResponse = new DashboardElementResponse
            {
                DashboardElements = new DashboardElements
                {
                    ProfileDetail = profileDetail,
                    Resources = new List<PersonaProductUserDetails>
                    {
                        new PersonaProductUserDetails { ProductName = "Product1" }
                    }
                }
            };
            _mockManageDashboardContent
                .Setup(x => x.GetDashboardElementResponse(userRealPageId, persona))
                .Returns(dashboardResponse);

            var passwordResponse = new CheckPasswordExpirationResponse
            {
                IsPasswordExpired = true
            };
            _mockManageCredential
                .Setup(x => x.CheckPasswordExpiration(userId, userRealPageId))
                .Returns(passwordResponse);

            // Act
            var result = await _dashboardController.GetDashboardContent();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<DashboardElementResponse>(okResult.Value);
            Assert.Null(response.DashboardElements.Resources);
            Assert.Null(response.DashboardElements.ProfileDetail.AssignedProducts);
        }

        [Fact]
        public async Task GetDashboardContent_WhenPasswordNotExpired_KeepsResourcesAndAssignedProducts()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            var personaId = 12345L;
            var userId = 100;
            var userClaim = new DefaultUserClaim
            {
                UserRealPageGuid = userRealPageId,
                PersonaId = personaId,
                UserId = userId
            };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var persona = new Persona { PersonaId = personaId };
            _mockManagePersona.Setup(x => x.GetPersona(personaId)).Returns(persona);

            var resources = new List<PersonaProductUserDetails>
            {
                new PersonaProductUserDetails { ProductName = "Product1" },
                new PersonaProductUserDetails { ProductName = "Product2" }
            };
            var assignedProducts = new List<PersonaProductUserDetails>
            {
                new PersonaProductUserDetails { ProductName = "Assigned1" },
                new PersonaProductUserDetails { ProductName = "Assigned2" }
            };
            var profileDetail = new ProfileDetail
            {
                AssignedProducts = assignedProducts
            };
            var dashboardResponse = new DashboardElementResponse
            {
                DashboardElements = new DashboardElements
                {
                    ProfileDetail = profileDetail,
                    Resources = resources
                }
            };
            _mockManageDashboardContent
                .Setup(x => x.GetDashboardElementResponse(userRealPageId, persona))
                .Returns(dashboardResponse);

            var passwordResponse = new CheckPasswordExpirationResponse
            {
                IsPasswordExpired = false
            };
            _mockManageCredential
                .Setup(x => x.CheckPasswordExpiration(userId, userRealPageId))
                .Returns(passwordResponse);

            // Act
            var result = await _dashboardController.GetDashboardContent();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<DashboardElementResponse>(okResult.Value);
            Assert.NotNull(response.DashboardElements.Resources);
            Assert.Equal(2, response.DashboardElements.Resources.Count);
            Assert.NotNull(response.DashboardElements.ProfileDetail.AssignedProducts);
            Assert.Equal(2, response.DashboardElements.ProfileDetail.AssignedProducts.Count);
        }

        [Fact]
        public async Task GetDashboardContent_WhenCheckPasswordExpirationReturnsNull_KeepsResourcesAndAssignedProducts()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            var personaId = 12345L;
            var userId = 100;
            var userClaim = new DefaultUserClaim
            {
                UserRealPageGuid = userRealPageId,
                PersonaId = personaId,
                UserId = userId
            };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var persona = new Persona { PersonaId = personaId };
            _mockManagePersona.Setup(x => x.GetPersona(personaId)).Returns(persona);

            var resources = new List<PersonaProductUserDetails>
            {
                new PersonaProductUserDetails { ProductName = "Product1" }
            };
            var assignedProducts = new List<PersonaProductUserDetails>
            {
                new PersonaProductUserDetails { ProductName = "Assigned1" }
            };
            var profileDetail = new ProfileDetail
            {
                AssignedProducts = assignedProducts
            };
            var dashboardResponse = new DashboardElementResponse
            {
                DashboardElements = new DashboardElements
                {
                    ProfileDetail = profileDetail,
                    Resources = resources
                }
            };
            _mockManageDashboardContent
                .Setup(x => x.GetDashboardElementResponse(userRealPageId, persona))
                .Returns(dashboardResponse);

            _mockManageCredential
                .Setup(x => x.CheckPasswordExpiration(userId, userRealPageId))
                .Returns((CheckPasswordExpirationResponse)null!);

            // Act
            var result = await _dashboardController.GetDashboardContent();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<DashboardElementResponse>(okResult.Value);
            Assert.NotNull(response.DashboardElements.Resources);
            Assert.NotNull(response.DashboardElements.ProfileDetail.AssignedProducts);
        }

        #endregion

        #region GetDashboardContent Tests - Edge Cases

        [Fact]
        public async Task GetDashboardContent_WithEmptyDashboardElements_ReturnsOkResult()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            var personaId = 12345L;
            var userClaim = new DefaultUserClaim
            {
                UserRealPageGuid = userRealPageId,
                PersonaId = personaId,
                UserId = 100
            };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var persona = new Persona { PersonaId = personaId };
            _mockManagePersona.Setup(x => x.GetPersona(personaId)).Returns(persona);

            var dashboardResponse = new DashboardElementResponse
            {
                DashboardElements = new DashboardElements
                {
                    ProfileDetail = new ProfileDetail(),
                    Resources = new List<PersonaProductUserDetails>(),
                    TrainingAchievements = new List<TrainingAchievement>()
                }
            };
            _mockManageDashboardContent
                .Setup(x => x.GetDashboardElementResponse(userRealPageId, persona))
                .Returns(dashboardResponse);

            var passwordResponse = new CheckPasswordExpirationResponse { IsPasswordExpired = false };
            _mockManageCredential
                .Setup(x => x.CheckPasswordExpiration(It.IsAny<int>(), It.IsAny<Guid>()))
                .Returns(passwordResponse);

            // Act
            var result = await _dashboardController.GetDashboardContent();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<DashboardElementResponse>(okResult.Value);
            Assert.Empty(response.DashboardElements.Resources);
        }

        [Fact]
        public async Task GetDashboardContent_WithZeroPersonaId_CallsGetPersonaWithZero()
        {
            // Arrange
            var userClaim = new DefaultUserClaim
            {
                UserRealPageGuid = Guid.NewGuid(),
                PersonaId = 0,
                UserId = 100
            };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            _mockManagePersona.Setup(x => x.GetPersona(0)).Returns((Persona)null!);

            // Act
            var result = await _dashboardController.GetDashboardContent();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            _mockManagePersona.Verify(x => x.GetPersona(0), Times.Once);
        }

        [Fact]
        public async Task GetDashboardContent_WithLargePersonaId_ReturnsOkResult()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            var personaId = long.MaxValue;
            var userClaim = new DefaultUserClaim
            {
                UserRealPageGuid = userRealPageId,
                PersonaId = personaId,
                UserId = 100
            };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var persona = new Persona { PersonaId = personaId };
            _mockManagePersona.Setup(x => x.GetPersona(personaId)).Returns(persona);

            var dashboardResponse = new DashboardElementResponse
            {
                DashboardElements = new DashboardElements
                {
                    ProfileDetail = new ProfileDetail(),
                    Resources = new List<PersonaProductUserDetails>()
                }
            };
            _mockManageDashboardContent
                .Setup(x => x.GetDashboardElementResponse(userRealPageId, persona))
                .Returns(dashboardResponse);

            var passwordResponse = new CheckPasswordExpirationResponse { IsPasswordExpired = false };
            _mockManageCredential
                .Setup(x => x.CheckPasswordExpiration(It.IsAny<int>(), It.IsAny<Guid>()))
                .Returns(passwordResponse);

            // Act
            var result = await _dashboardController.GetDashboardContent();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetDashboardContent_WithFullPersonaDetails_ReturnsCompleteData()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            var personaId = 12345L;
            var userClaim = new DefaultUserClaim
            {
                UserRealPageGuid = userRealPageId,
                PersonaId = personaId,
                UserId = 100
            };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var persona = new Persona
            {
                PersonaId = personaId,
                PersonaName = "Primary Persona",
                PersonaTypeId = 1,
                PersonaEnvironmentTypeId = 1,
                IsDefault = true,
                hasMultiCompany = true,
                hasMultiPersona = false,
                Organization = new Organization { PartyId = 1000, Name = "Test Org" }
            };
            _mockManagePersona.Setup(x => x.GetPersona(personaId)).Returns(persona);

            var dashboardResponse = new DashboardElementResponse
            {
                DashboardElements = new DashboardElements
                {
                    ProfileDetail = new ProfileDetail
                    {
                        FirstName = "John",
                        LastName = "Doe"
                    },
                    Resources = new List<PersonaProductUserDetails>
                    {
                        new PersonaProductUserDetails { ProductName = "Product1" },
                        new PersonaProductUserDetails { ProductName = "Product2" }
                    },
                    TrainingAchievements = new List<TrainingAchievement>()
                }
            };
            _mockManageDashboardContent
                .Setup(x => x.GetDashboardElementResponse(userRealPageId, persona))
                .Returns(dashboardResponse);

            var passwordResponse = new CheckPasswordExpirationResponse { IsPasswordExpired = false };
            _mockManageCredential
                .Setup(x => x.CheckPasswordExpiration(It.IsAny<int>(), It.IsAny<Guid>()))
                .Returns(passwordResponse);

            // Act
            var result = await _dashboardController.GetDashboardContent();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<DashboardElementResponse>(okResult.Value);
            Assert.Equal(2, response.DashboardElements.Resources.Count);
            Assert.Equal("John", response.DashboardElements.ProfileDetail.FirstName);
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _dashboardController = null!;
            base.Dispose();
        }

        #endregion
    }
}





