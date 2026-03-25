using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for PartyRelationshipController.
    /// Tests all endpoints, error cases, and edge cases for 100% code coverage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PartyRelationshipControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IPartyRelationshipRepositoryAsync> _mockPartyRelationshipRepository;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private PartyRelationshipController _partyRelationshipController;

        #endregion

        #region Constructor

        public PartyRelationshipControllerTests()
        {
            _mockPartyRelationshipRepository = new Mock<IPartyRelationshipRepositoryAsync>();
            _mockUserClaimsAccessor = MockUserClaimsAccessor;

            _partyRelationshipController = new PartyRelationshipController(
                _mockPartyRelationshipRepository.Object,
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
            var controller = new PartyRelationshipController(
                _mockPartyRelationshipRepository.Object,
                _mockUserClaimsAccessor.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullPartyRelationshipRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new PartyRelationshipController(
                    null!,
                    _mockUserClaimsAccessor.Object));

            Assert.Equal("partyRelationshipRepository", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new PartyRelationshipController(
                    _mockPartyRelationshipRepository.Object,
                    null!));

            Assert.Equal("userClaimsAccessor", exception.ParamName);
        }

        #endregion

        #region LinkOrganizationToOrganization Tests - Success

        [Fact]
        public async Task LinkOrganizationToOrganization_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();

            _mockPartyRelationshipRepository
                .Setup(x => x.LinkOrganizationToOrganizationAsync(realPageIdFrom, partyRelationship, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 12345 });

            // Act
            var result = await _partyRelationshipController.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var outputResult = Assert.IsType<PartyRelationship.PartyRelationshipOutputResult>(okResult.Value);
            Assert.Equal(12345, outputResult.NewPartyRelationshipId);
        }

        [Fact]
        public async Task LinkOrganizationToOrganization_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();

            _mockPartyRelationshipRepository
                .Setup(x => x.LinkOrganizationToOrganizationAsync(realPageIdFrom, partyRelationship, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            // Act
            await _partyRelationshipController.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            _mockPartyRelationshipRepository.Verify(
                x => x.LinkOrganizationToOrganizationAsync(realPageIdFrom, partyRelationship, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region LinkOrganizationToOrganization Tests - BadRequest

        [Fact]
        public async Task LinkOrganizationToOrganization_WithEmptyRealPageIdFrom_ReturnsBadRequest()
        {
            // Arrange
            var partyRelationship = CreateValidPartyRelationship();

            // Act
            var result = await _partyRelationshipController.LinkOrganizationToOrganization(Guid.Empty, partyRelationship);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: RealPageIdFrom.", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkOrganizationToOrganization_WithNullPartyRelationship_ReturnsBadRequest()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();

            // Act
            var result = await _partyRelationshipController.LinkOrganizationToOrganization(realPageIdFrom, null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Null parameter: partyRelationship.", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkOrganizationToOrganization_WithEmptyRealPageIdTo_ReturnsBadRequest()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RealPageIdTo = Guid.Empty;

            // Act
            var result = await _partyRelationshipController.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: RealPageIdTo.", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkOrganizationToOrganization_WithZeroRoleTypeIdFrom_ReturnsBadRequest()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdFrom = 0;

            // Act
            var result = await _partyRelationshipController.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: RoleTypeIdFrom.", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkOrganizationToOrganization_WithNegativeRoleTypeIdFrom_ReturnsBadRequest()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdFrom = -1;

            // Act
            var result = await _partyRelationshipController.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: RoleTypeIdFrom.", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkOrganizationToOrganization_WithZeroRoleTypeIdTo_ReturnsBadRequest()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdTo = 0;

            // Act
            var result = await _partyRelationshipController.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: RoleTypeIdTo.", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkOrganizationToOrganization_WithNegativeRoleTypeIdTo_ReturnsBadRequest()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdTo = -5;

            // Act
            var result = await _partyRelationshipController.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: RoleTypeIdTo.", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkOrganizationToOrganization_WhenRepositoryReturnsZeroId_ReturnsBadRequest()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            const string errorMessage = "Failed to link organizations";

            _mockPartyRelationshipRepository
                .Setup(x => x.LinkOrganizationToOrganizationAsync(realPageIdFrom, partyRelationship, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            // Act
            var result = await _partyRelationshipController.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        #endregion

        #region LinkPersonToOrganization Tests - Success

        [Fact]
        public async Task LinkPersonToOrganization_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();

            _mockPartyRelationshipRepository
                .Setup(x => x.LinkPersonToOrganizationAsync(realPageIdFrom, partyRelationship, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 54321 });

            // Act
            var result = await _partyRelationshipController.LinkPersonToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var outputResult = Assert.IsType<PartyRelationship.PartyRelationshipOutputResult>(okResult.Value);
            Assert.Equal(54321, outputResult.NewPartyRelationshipId);
        }

        [Fact]
        public async Task LinkPersonToOrganization_WithEmptyGuid_UsesUserClaimsRealPageId()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            var userClaim = new DefaultUserClaim { UserRealPageGuid = userRealPageId };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var partyRelationship = CreateValidPartyRelationship();

            _mockPartyRelationshipRepository
                .Setup(x => x.LinkPersonToOrganizationAsync(userRealPageId, partyRelationship, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _partyRelationshipController.LinkPersonToOrganization(Guid.Empty, partyRelationship);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockPartyRelationshipRepository.Verify(
                x => x.LinkPersonToOrganizationAsync(userRealPageId, partyRelationship, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task LinkPersonToOrganization_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();

            _mockPartyRelationshipRepository
                .Setup(x => x.LinkPersonToOrganizationAsync(realPageIdFrom, partyRelationship, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            // Act
            await _partyRelationshipController.LinkPersonToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            _mockPartyRelationshipRepository.Verify(
                x => x.LinkPersonToOrganizationAsync(realPageIdFrom, partyRelationship, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region LinkPersonToOrganization Tests - BadRequest

        [Fact]
        public async Task LinkPersonToOrganization_WithEmptyGuidAndEmptyUserClaim_ReturnsBadRequest()
        {
            // Arrange
            var userClaim = new DefaultUserClaim { UserRealPageGuid = Guid.Empty };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var partyRelationship = CreateValidPartyRelationship();

            // Act
            var result = await _partyRelationshipController.LinkPersonToOrganization(Guid.Empty, partyRelationship);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: PersonRealPageId.", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkPersonToOrganization_WithEmptyGuidAndNullUserClaim_ReturnsBadRequest()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var partyRelationship = CreateValidPartyRelationship();

            // Act
            var result = await _partyRelationshipController.LinkPersonToOrganization(Guid.Empty, partyRelationship);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: PersonRealPageId.", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkPersonToOrganization_WithNullPartyRelationship_ReturnsBadRequest()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();

            // Act
            var result = await _partyRelationshipController.LinkPersonToOrganization(realPageIdFrom, null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Null parameter: partyRelationship.", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkPersonToOrganization_WithEmptyRealPageIdTo_ReturnsBadRequest()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RealPageIdTo = Guid.Empty;

            // Act
            var result = await _partyRelationshipController.LinkPersonToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: RealPageIdTo", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkPersonToOrganization_WithZeroRoleTypeIdFrom_ReturnsBadRequest()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdFrom = 0;

            // Act
            var result = await _partyRelationshipController.LinkPersonToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: RoleTypeIdFrom.", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkPersonToOrganization_WithNegativeRoleTypeIdFrom_ReturnsBadRequest()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdFrom = -10;

            // Act
            var result = await _partyRelationshipController.LinkPersonToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: RoleTypeIdFrom.", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkPersonToOrganization_WithZeroRoleTypeIdTo_ReturnsBadRequest()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdTo = 0;

            // Act
            var result = await _partyRelationshipController.LinkPersonToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: RoleTypeIdTo.", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkPersonToOrganization_WithNegativeRoleTypeIdTo_ReturnsBadRequest()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdTo = -3;

            // Act
            var result = await _partyRelationshipController.LinkPersonToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: RoleTypeIdTo.", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkPersonToOrganization_WhenRepositoryReturnsZeroId_ReturnsBadRequest()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            const string errorMessage = "Failed to link person to organization";

            _mockPartyRelationshipRepository
                .Setup(x => x.LinkPersonToOrganizationAsync(realPageIdFrom, partyRelationship, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            // Act
            var result = await _partyRelationshipController.LinkPersonToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task LinkOrganizationToOrganization_WithMinRoleTypeIds_ReturnsOkResult()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdFrom = 1;
            partyRelationship.RoleTypeIdTo = 1;

            _mockPartyRelationshipRepository
                .Setup(x => x.LinkOrganizationToOrganizationAsync(realPageIdFrom, partyRelationship, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _partyRelationshipController.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task LinkPersonToOrganization_WithMaxRoleTypeIds_ReturnsOkResult()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdFrom = int.MaxValue;
            partyRelationship.RoleTypeIdTo = int.MaxValue;

            _mockPartyRelationshipRepository
                .Setup(x => x.LinkPersonToOrganizationAsync(realPageIdFrom, partyRelationship, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _partyRelationshipController.LinkPersonToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Concurrent Access Tests

        [Fact]
        public async Task LinkOrganizationToOrganization_MultipleConcurrentCalls_AllComplete()
        {
            // Arrange
            _mockPartyRelationshipRepository
                .Setup(x => x.LinkOrganizationToOrganizationAsync(It.IsAny<Guid>(), It.IsAny<PartyRelationship>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            var tasks = new List<Task<IActionResult>>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                var partyRelationship = CreateValidPartyRelationship();
                tasks.Add(_partyRelationshipController.LinkOrganizationToOrganization(Guid.NewGuid(), partyRelationship));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            foreach (var result in results)
            {
                Assert.IsType<OkObjectResult>(result);
            }
        }

        [Fact]
        public async Task LinkPersonToOrganization_MultipleConcurrentCalls_AllComplete()
        {
            // Arrange
            _mockPartyRelationshipRepository
                .Setup(x => x.LinkPersonToOrganizationAsync(It.IsAny<Guid>(), It.IsAny<PartyRelationship>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            var tasks = new List<Task<IActionResult>>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                var partyRelationship = CreateValidPartyRelationship();
                tasks.Add(_partyRelationshipController.LinkPersonToOrganization(Guid.NewGuid(), partyRelationship));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            foreach (var result in results)
            {
                Assert.IsType<OkObjectResult>(result);
            }
        }

        #endregion

        #region Helper Methods

        private static PartyRelationship CreateValidPartyRelationship()
        {
            return new PartyRelationship
            {
                RealPageIdTo = Guid.NewGuid(),
                RoleTypeIdFrom = 1,
                RoleTypeIdTo = 2,
                PartyRelationshipTypeId = 1
            };
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _partyRelationshipController = null!;
            base.Dispose();
        }

        #endregion
    }
}





