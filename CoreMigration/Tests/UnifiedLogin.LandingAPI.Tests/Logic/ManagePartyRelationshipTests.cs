using System;
using System.Diagnostics.CodeAnalysis;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManagePartyRelationship business logic xUnit tests.
    /// Tests for party relationship management operations including linking persons and organizations.
    /// Migrated from RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.ManagePartyRelationshipTests
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManagePartyRelationshipTests : TestBase
    {
        private readonly Mock<IPartyRelationshipRepository> _mockPartyRelationshipRepository;
        private readonly Mock<IRepository> _mockRepository;

        public ManagePartyRelationshipTests()
        {
            _mockPartyRelationshipRepository = new Mock<IPartyRelationshipRepository>();
            _mockRepository = new Mock<IRepository>();
        }

        #region Helper Methods

        private PartyRelationship CreateValidPartyRelationship()
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

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNoParameters_InitializesSuccessfully()
        {
            // Arrange & Act
            var managePartyRelationship = new ManagePartyRelationship();

            // Assert
            Assert.NotNull(managePartyRelationship);
        }

        [Fact]
        public void Constructor_WithRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var managePartyRelationship = new ManagePartyRelationship(_mockRepository.Object);

            // Assert
            Assert.NotNull(managePartyRelationship);
        }

        [Fact]
        public void Constructor_WithPartyRelationshipRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Assert
            Assert.NotNull(managePartyRelationship);
        }

        #endregion

        #region GetPartyRelationship Tests

        [Fact]
        public void GetPartyRelationship_WithValidParameters_ReturnsPartyRelationship()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var realPageIdTo = Guid.NewGuid();
            var roleTypeNameFrom = "Employee";
            var roleTypeNameTo = "Organization";
            var relationshipTypeName = "Employment";
            var expectedRelationship = CreateValidPartyRelationship();

            _mockPartyRelationshipRepository
                .Setup(x => x.GetPartyRelationship(
                    realPageIdFrom,
                    realPageIdTo,
                    roleTypeNameFrom,
                    roleTypeNameTo,
                    relationshipTypeName))
                .Returns(expectedRelationship);

            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act
            var result = managePartyRelationship.GetPartyRelationship(
                realPageIdFrom,
                realPageIdTo,
                roleTypeNameFrom,
                roleTypeNameTo,
                relationshipTypeName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedRelationship, result);
            _mockPartyRelationshipRepository.Verify(x => x.GetPartyRelationship(
                realPageIdFrom,
                realPageIdTo,
                roleTypeNameFrom,
                roleTypeNameTo,
                relationshipTypeName), Times.Once);
        }

        [Fact]
        public void GetPartyRelationship_WithNullRoleTypes_ReturnsPartyRelationship()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var realPageIdTo = Guid.NewGuid();
            var expectedRelationship = CreateValidPartyRelationship();

            _mockPartyRelationshipRepository
                .Setup(x => x.GetPartyRelationship(
                    realPageIdFrom,
                    realPageIdTo,
                    null,
                    null,
                    null))
                .Returns(expectedRelationship);

            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act
            var result = managePartyRelationship.GetPartyRelationship(
                realPageIdFrom,
                realPageIdTo,
                null,
                null,
                null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedRelationship, result);
        }

        [Fact]
        public void GetPartyRelationship_WithEmptyRealPageIdFrom_ThrowsException()
        {
            // Arrange
            var realPageIdFrom = Guid.Empty;
            var realPageIdTo = Guid.NewGuid();
            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePartyRelationship.GetPartyRelationship(
                    realPageIdFrom,
                    realPageIdTo,
                    null,
                    null,
                    null));

            Assert.Equal("Invalid parameter realPageIdFrom.", exception.Message);
        }

        [Fact]
        public void GetPartyRelationship_WithEmptyRealPageIdTo_ThrowsException()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var realPageIdTo = Guid.Empty;
            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePartyRelationship.GetPartyRelationship(
                    realPageIdFrom,
                    realPageIdTo,
                    null,
                    null,
                    null));

            Assert.Equal("Invalid parameter realPageIdTo.", exception.Message);
        }

        [Fact]
        public void GetPartyRelationship_WithBothEmptyGuids_ThrowsExceptionForFrom()
        {
            // Arrange
            var realPageIdFrom = Guid.Empty;
            var realPageIdTo = Guid.Empty;
            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act & Assert - Should throw for the first parameter checked
            var exception = Assert.Throws<Exception>(() =>
                managePartyRelationship.GetPartyRelationship(
                    realPageIdFrom,
                    realPageIdTo,
                    null,
                    null,
                    null));

            Assert.Equal("Invalid parameter realPageIdFrom.", exception.Message);
        }

        [Fact]
        public void GetPartyRelationship_WithSpecificRoleTypeNames_CallsRepositoryCorrectly()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var realPageIdTo = Guid.NewGuid();
            var roleTypeNameFrom = "Manager";
            var roleTypeNameTo = "Company";
            var relationshipTypeName = "Manages";
            var expectedRelationship = CreateValidPartyRelationship();

            _mockPartyRelationshipRepository
                .Setup(x => x.GetPartyRelationship(realPageIdFrom, realPageIdTo, roleTypeNameFrom, roleTypeNameTo, relationshipTypeName))
                .Returns(expectedRelationship);

            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act
            var result = managePartyRelationship.GetPartyRelationship(
                realPageIdFrom,
                realPageIdTo,
                roleTypeNameFrom,
                roleTypeNameTo,
                relationshipTypeName);

            // Assert
            Assert.NotNull(result);
            _mockPartyRelationshipRepository.Verify(x => x.GetPartyRelationship(
                realPageIdFrom,
                realPageIdTo,
                roleTypeNameFrom,
                roleTypeNameTo,
                relationshipTypeName), Times.Once);
        }

        #endregion

        #region LinkOrganizationToOrganization Tests

        [Fact]
        public void LinkOrganizationToOrganization_WithValidParameters_ReturnsSuccessResponse()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            var expectedResponse = new RepositoryResponse
            {
                Id = 1,
                ErrorMessage = ""
            };

            _mockPartyRelationshipRepository
                .Setup(x => x.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship))
                .Returns(expectedResponse);

            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act
            var result = managePartyRelationship.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            Assert.Empty(result.ErrorMessage);
            _mockPartyRelationshipRepository.Verify(x => x.LinkOrganizationToOrganization(
                realPageIdFrom,
                partyRelationship), Times.Once);
        }

        [Fact]
        public void LinkOrganizationToOrganization_WithEmptyRealPageIdFrom_ThrowsException()
        {
            // Arrange
            var realPageIdFrom = Guid.Empty;
            var partyRelationship = CreateValidPartyRelationship();
            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePartyRelationship.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship));

            Assert.Equal("Invalid parameter RealPageIdFrom.", exception.Message);
        }

        [Fact]
        public void LinkOrganizationToOrganization_WithNullPartyRelationship_ThrowsArgumentNullException()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            PartyRelationship partyRelationship = null;
            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                managePartyRelationship.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship));

            Assert.Equal("partyRelationship", exception.ParamName);
            Assert.Contains("Null PartyRelationship", exception.Message);
        }

        [Fact]
        public void LinkOrganizationToOrganization_WithEmptyRealPageIdTo_ThrowsException()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RealPageIdTo = Guid.Empty;
            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePartyRelationship.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship));

            Assert.Equal("Invalid parameter partyRelationship.RealPageIdTo.", exception.Message);
        }

        [Fact]
        public void LinkOrganizationToOrganization_WithZeroRoleTypeIdFrom_ThrowsException()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdFrom = 0;
            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePartyRelationship.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship));

            Assert.Equal("Invalid parameter partyRelationship.RoleTypeIdFrom.", exception.Message);
        }

        [Fact]
        public void LinkOrganizationToOrganization_WithNegativeRoleTypeIdFrom_ThrowsException()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdFrom = -1;
            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePartyRelationship.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship));

            Assert.Equal("Invalid parameter partyRelationship.RoleTypeIdFrom.", exception.Message);
        }

        [Fact]
        public void LinkOrganizationToOrganization_WithZeroRoleTypeIdTo_ThrowsException()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdTo = 0;
            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePartyRelationship.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship));

            Assert.Equal("Invalid parameter partyRelationship.RoleTypeIdTo.", exception.Message);
        }

        [Fact]
        public void LinkOrganizationToOrganization_WithNegativeRoleTypeIdTo_ThrowsException()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdTo = -1;
            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePartyRelationship.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship));

            Assert.Equal("Invalid parameter partyRelationship.RoleTypeIdTo.", exception.Message);
        }

        [Fact]
        public void LinkOrganizationToOrganization_WithDifferentRoleTypeIds_ValidatesCorrectly()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdFrom = 5;
            partyRelationship.RoleTypeIdTo = 10;

            var expectedResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockPartyRelationshipRepository
                .Setup(x => x.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship))
                .Returns(expectedResponse);

            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act
            var result = managePartyRelationship.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        #endregion

        #region LinkPersonToOrganization Tests

        [Fact]
        public void LinkPersonToOrganization_WithValidParameters_ReturnsSuccessResponse()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            var expectedResponse = new RepositoryResponse
            {
                Id = 1,
                ErrorMessage = ""
            };

            _mockPartyRelationshipRepository
                .Setup(x => x.LinkPersonToOrganization(realPageIdFrom, partyRelationship))
                .Returns(expectedResponse);

            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act
            var result = managePartyRelationship.LinkPersonToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            Assert.Empty(result.ErrorMessage);
            _mockPartyRelationshipRepository.Verify(x => x.LinkPersonToOrganization(
                realPageIdFrom,
                partyRelationship), Times.Once);
        }

        [Fact]
        public void LinkPersonToOrganization_WithEmptyRealPageIdFrom_ThrowsException()
        {
            // Arrange
            var realPageIdFrom = Guid.Empty;
            var partyRelationship = CreateValidPartyRelationship();
            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePartyRelationship.LinkPersonToOrganization(realPageIdFrom, partyRelationship));

            Assert.Equal("Invalid parameter RealPageIdFrom.", exception.Message);
        }

        [Fact]
        public void LinkPersonToOrganization_WithNullPartyRelationship_ThrowsArgumentNullException()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            PartyRelationship partyRelationship = null;
            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                managePartyRelationship.LinkPersonToOrganization(realPageIdFrom, partyRelationship));

            Assert.Equal("partyRelationship", exception.ParamName);
            Assert.Contains("Null PartyRelationship", exception.Message);
        }

        [Fact]
        public void LinkPersonToOrganization_WithEmptyRealPageIdTo_ThrowsException()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RealPageIdTo = Guid.Empty;
            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePartyRelationship.LinkPersonToOrganization(realPageIdFrom, partyRelationship));

            Assert.Equal("Invalid parameter partyRelationship.RealPageIdTo.", exception.Message);
        }

        [Fact]
        public void LinkPersonToOrganization_WithZeroRoleTypeIdFrom_ThrowsException()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdFrom = 0;
            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePartyRelationship.LinkPersonToOrganization(realPageIdFrom, partyRelationship));

            Assert.Equal("Invalid parameter partyRelationship.RoleTypeIdFrom.", exception.Message);
        }

        [Fact]
        public void LinkPersonToOrganization_WithNegativeRoleTypeIdFrom_ThrowsException()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdFrom = -1;
            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePartyRelationship.LinkPersonToOrganization(realPageIdFrom, partyRelationship));

            Assert.Equal("Invalid parameter partyRelationship.RoleTypeIdFrom.", exception.Message);
        }

        [Fact]
        public void LinkPersonToOrganization_WithZeroRoleTypeIdTo_ThrowsException()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdTo = 0;
            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePartyRelationship.LinkPersonToOrganization(realPageIdFrom, partyRelationship));

            Assert.Equal("Invalid parameter partyRelationship.RoleTypeIdTo.", exception.Message);
        }

        [Fact]
        public void LinkPersonToOrganization_WithNegativeRoleTypeIdTo_ThrowsException()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdTo = -1;
            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePartyRelationship.LinkPersonToOrganization(realPageIdFrom, partyRelationship));

            Assert.Equal("Invalid parameter partyRelationship.RoleTypeIdTo.", exception.Message);
        }

        [Fact]
        public void LinkPersonToOrganization_WithDifferentRoleTypeIds_ValidatesCorrectly()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RoleTypeIdFrom = 3;
            partyRelationship.RoleTypeIdTo = 7;

            var expectedResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockPartyRelationshipRepository
                .Setup(x => x.LinkPersonToOrganization(realPageIdFrom, partyRelationship))
                .Returns(expectedResponse);

            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act
            var result = managePartyRelationship.LinkPersonToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void ManagePartyRelationship_CompleteWorkflow_GetAndLinkOrganization()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var realPageIdTo = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RealPageIdTo = realPageIdTo;

            var expectedRelationship = CreateValidPartyRelationship();
            var expectedResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockPartyRelationshipRepository
                .Setup(x => x.GetPartyRelationship(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(expectedRelationship);

            _mockPartyRelationshipRepository
                .Setup(x => x.LinkOrganizationToOrganization(It.IsAny<Guid>(), It.IsAny<PartyRelationship>()))
                .Returns(expectedResponse);

            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act
            var getResult = managePartyRelationship.GetPartyRelationship(realPageIdFrom, realPageIdTo, null, null, null);
            var linkResult = managePartyRelationship.LinkOrganizationToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            Assert.NotNull(getResult);
            Assert.NotNull(linkResult);
            Assert.Equal(1, linkResult.Id);
        }

        [Fact]
        public void ManagePartyRelationship_CompleteWorkflow_GetAndLinkPerson()
        {
            // Arrange
            var realPageIdFrom = Guid.NewGuid();
            var realPageIdTo = Guid.NewGuid();
            var partyRelationship = CreateValidPartyRelationship();
            partyRelationship.RealPageIdTo = realPageIdTo;

            var expectedRelationship = CreateValidPartyRelationship();
            var expectedResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockPartyRelationshipRepository
                .Setup(x => x.GetPartyRelationship(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(expectedRelationship);

            _mockPartyRelationshipRepository
                .Setup(x => x.LinkPersonToOrganization(It.IsAny<Guid>(), It.IsAny<PartyRelationship>()))
                .Returns(expectedResponse);

            var managePartyRelationship = new ManagePartyRelationship(_mockPartyRelationshipRepository.Object);

            // Act
            var getResult = managePartyRelationship.GetPartyRelationship(realPageIdFrom, realPageIdTo, null, null, null);
            var linkResult = managePartyRelationship.LinkPersonToOrganization(realPageIdFrom, partyRelationship);

            // Assert
            Assert.NotNull(getResult);
            Assert.NotNull(linkResult);
            Assert.Equal(1, linkResult.Id);
        }

        #endregion
    }
}
