using System;
using System.Collections.Generic;
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
    /// ManagePartyRole business logic xUnit tests.
    /// Tests for party role management operations including CRUD operations.
    /// Migrated from RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.ManagePartyRoleTest
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManagePartyRoleTests : TestBase
    {
        private readonly Mock<IPartyRoleRepository> _mockPartyRoleRepository;
        private readonly Mock<IRepository> _mockRepository;

        public ManagePartyRoleTests()
        {
            _mockPartyRoleRepository = new Mock<IPartyRoleRepository>();
            _mockRepository = new Mock<IRepository>();
        }

        #region Helper Methods

        private PartyRole CreateValidPartyRole()
        {
            return new PartyRole
            {
                PartyRoleId = 1,
                PartyId = 1000,
                RoleTypeId = 1
            };
        }

        private IPartyRole CreateValidIPartyRole()
        {
            return new PartyRole
            {
                PartyRoleId = 1,
                PartyId = 1000,
                RoleTypeId = 1
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNoParameters_InitializesSuccessfully()
        {
            // Arrange & Act
            var managePartyRole = new ManagePartyRole();

            // Assert
            Assert.NotNull(managePartyRole);
        }

        [Fact]
        public void Constructor_WithRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var managePartyRole = new ManagePartyRole(_mockRepository.Object);

            // Assert
            Assert.NotNull(managePartyRole);
        }

        [Fact]
        public void Constructor_WithPartyRoleRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Assert
            Assert.NotNull(managePartyRole);
        }

        #endregion

        #region CreatePartyRoleEnterpriseUserID Tests

        [Fact]
        public void CreatePartyRoleEnterpriseUserID_WithValidParameters_ReturnsSuccessResponse()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var partyRole = CreateValidIPartyRole();
            var expectedResponse = new RepositoryResponse
            {
                Id = 1,
                RealPageId = Guid.NewGuid(),
                ErrorMessage = ""
            };

            _mockPartyRoleRepository
                .Setup(x => x.CreatePartyRoleEnterpriseUserID(realPageId, partyRole))
                .Returns(expectedResponse);

            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Act
            var result = managePartyRole.CreatePartyRoleEnterpriseUserID(realPageId, partyRole);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            Assert.Empty(result.ErrorMessage);
            _mockPartyRoleRepository.Verify(x => x.CreatePartyRoleEnterpriseUserID(
                realPageId,
                partyRole), Times.Once);
        }

        [Fact]
        public void CreatePartyRoleEnterpriseUserID_WithEmptyRealPageId_ThrowsException()
        {
            // Arrange
            var realPageId = Guid.Empty;
            var partyRole = CreateValidIPartyRole();
            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePartyRole.CreatePartyRoleEnterpriseUserID(realPageId, partyRole));

            Assert.Equal("Invalid parameter realPageId.", exception.Message);
        }

        [Fact]
        public void CreatePartyRoleEnterpriseUserID_WithNullPartyRole_ThrowsArgumentNullException()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            IPartyRole partyRole = null;
            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                managePartyRole.CreatePartyRoleEnterpriseUserID(realPageId, partyRole));

            Assert.Equal("partyRole", exception.ParamName);
            Assert.Contains("Null PartyRole", exception.Message);
        }

        [Fact]
        public void CreatePartyRoleEnterpriseUserID_WithBothInvalidParameters_ThrowsExceptionForRealPageId()
        {
            // Arrange
            var realPageId = Guid.Empty;
            IPartyRole partyRole = null;
            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Act & Assert - Should throw for the first parameter checked (realPageId)
            var exception = Assert.Throws<Exception>(() =>
                managePartyRole.CreatePartyRoleEnterpriseUserID(realPageId, partyRole));

            Assert.Equal("Invalid parameter realPageId.", exception.Message);
        }

        #endregion

        #region GetPartyRole Tests

        [Fact]
        public void GetPartyRole_WithValidRealPageId_ReturnsPartyRole()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var expectedPartyRole = CreateValidPartyRole();

            _mockPartyRoleRepository
                .Setup(x => x.GetPartyRoleByEnterpriseUserID(realPageId))
                .Returns(expectedPartyRole);

            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Act
            var result = managePartyRole.GetPartyRole(realPageId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPartyRole.PartyRoleId, result.PartyRoleId);
            Assert.Equal(expectedPartyRole.PartyId, result.PartyId);
            Assert.Equal(expectedPartyRole.RoleTypeId, result.RoleTypeId);
            _mockPartyRoleRepository.Verify(x => x.GetPartyRoleByEnterpriseUserID(realPageId), Times.Once);
        }

        [Fact]
        public void GetPartyRole_WithEmptyRealPageId_ThrowsException()
        {
            // Arrange
            var realPageId = Guid.Empty;
            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePartyRole.GetPartyRole(realPageId));

            Assert.Equal("Invalid parameter realPageId.", exception.Message);
        }

        [Fact]
        public void GetPartyRole_WhenRepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            _mockPartyRoleRepository
                .Setup(x => x.GetPartyRoleByEnterpriseUserID(realPageId))
                .Returns((PartyRole)null);

            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Act
            var result = managePartyRole.GetPartyRole(realPageId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetPartyRoles Tests

        [Fact]
        public void GetPartyRoles_WithValidPartyId_ReturnsPartyRoleList()
        {
            // Arrange
            long partyId = 1000;
            var expectedPartyRoles = new List<PartyRole>
            {
                CreateValidPartyRole(),
                CreateValidPartyRole()
            };

            _mockPartyRoleRepository
                .Setup(x => x.GetPartyRoles(partyId))
                .Returns(expectedPartyRoles);

            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Act
            var result = managePartyRole.GetPartyRoles(partyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _mockPartyRoleRepository.Verify(x => x.GetPartyRoles(partyId), Times.Once);
        }

        [Fact]
        public void GetPartyRoles_WithNullPartyId_ThrowsException()
        {
            // Arrange
            long? partyId = null;
            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePartyRole.GetPartyRoles(partyId));

            Assert.Equal("Invalid parameter partyId.", exception.Message);
        }

        [Fact]
        public void GetPartyRoles_WithZeroPartyId_ReturnsPartyRoleList()
        {
            // Arrange
            long partyId = 0;
            var expectedPartyRoles = new List<PartyRole>();

            _mockPartyRoleRepository
                .Setup(x => x.GetPartyRoles(partyId))
                .Returns(expectedPartyRoles);

            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Act
            var result = managePartyRole.GetPartyRoles(partyId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetPartyRoles_WithNegativePartyId_ReturnsPartyRoleList()
        {
            // Arrange
            long partyId = -1;
            var expectedPartyRoles = new List<PartyRole>();

            _mockPartyRoleRepository
                .Setup(x => x.GetPartyRoles(partyId))
                .Returns(expectedPartyRoles);

            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Act
            var result = managePartyRole.GetPartyRoles(partyId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region UpdatePartyRole Tests

        [Fact]
        public void UpdatePartyRole_WithValidPartyRole_ReturnsSuccessResponse()
        {
            // Arrange
            var partyRole = CreateValidIPartyRole();
            var expectedResponse = new RepositoryResponse
            {
                Id = 1,
                RealPageId = Guid.NewGuid(),
                ErrorMessage = ""
            };

            _mockPartyRoleRepository
                .Setup(x => x.UpdatePartyRoleEnterpriseUserID(partyRole))
                .Returns(expectedResponse);

            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Act
            var result = managePartyRole.UpdatePartyRole(partyRole);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            Assert.Empty(result.ErrorMessage);
            _mockPartyRoleRepository.Verify(x => x.UpdatePartyRoleEnterpriseUserID(partyRole), Times.Once);
        }

        [Fact]
        public void UpdatePartyRole_WithNullPartyRole_ThrowsArgumentNullException()
        {
            // Arrange
            IPartyRole partyRole = null;
            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                managePartyRole.UpdatePartyRole(partyRole));

            Assert.Equal("partyRole", exception.ParamName);
            Assert.Contains("Null PartyRole", exception.Message);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void ManagePartyRole_CompleteWorkflow_CreateGetUpdate()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var partyRole = CreateValidIPartyRole();
            var expectedPartyRole = CreateValidPartyRole();
            var createResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };
            var updateResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockPartyRoleRepository
                .Setup(x => x.CreatePartyRoleEnterpriseUserID(realPageId, partyRole))
                .Returns(createResponse);

            _mockPartyRoleRepository
                .Setup(x => x.GetPartyRoleByEnterpriseUserID(realPageId))
                .Returns(expectedPartyRole);

            _mockPartyRoleRepository
                .Setup(x => x.UpdatePartyRoleEnterpriseUserID(partyRole))
                .Returns(updateResponse);

            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Act
            var createResult = managePartyRole.CreatePartyRoleEnterpriseUserID(realPageId, partyRole);
            var getResult = managePartyRole.GetPartyRole(realPageId);
            var updateResult = managePartyRole.UpdatePartyRole(partyRole);

            // Assert
            Assert.NotNull(createResult);
            Assert.NotNull(getResult);
            Assert.NotNull(updateResult);
            Assert.Equal(1, createResult.Id);
            Assert.Equal(1, getResult.PartyRoleId);
            Assert.Equal(1, updateResult.Id);
        }

        [Fact]
        public void ManagePartyRole_GetPartyRolesWorkflow_RetrievesMultipleRoles()
        {
            // Arrange
            long partyId = 1000;
            var expectedPartyRoles = new List<PartyRole>
            {
                CreateValidPartyRole(),
                CreateValidPartyRole()
            };
            expectedPartyRoles[0].PartyRoleId = 1;
            expectedPartyRoles[1].PartyRoleId = 2;

            _mockPartyRoleRepository
                .Setup(x => x.GetPartyRoles(partyId))
                .Returns(expectedPartyRoles);

            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Act
            var result = managePartyRole.GetPartyRoles(partyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].PartyRoleId);
            Assert.Equal(2, result[1].PartyRoleId);
        }

        #endregion

        #region Edge Cases and Additional Scenarios

        [Fact]
        public void CreatePartyRoleEnterpriseUserID_WithDifferentRoleTypes_CallsRepositoryCorrectly()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var partyRole = CreateValidIPartyRole();
            ((PartyRole)partyRole).RoleTypeId = 5;

            var expectedResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockPartyRoleRepository
                .Setup(x => x.CreatePartyRoleEnterpriseUserID(realPageId, partyRole))
                .Returns(expectedResponse);

            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Act
            var result = managePartyRole.CreatePartyRoleEnterpriseUserID(realPageId, partyRole);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            _mockPartyRoleRepository.Verify(x => x.CreatePartyRoleEnterpriseUserID(
                realPageId,
                partyRole), Times.Once);
        }

        [Fact]
        public void UpdatePartyRole_WithDifferentPartyIds_CallsRepositoryCorrectly()
        {
            // Arrange
            var partyRole = CreateValidIPartyRole();
            ((PartyRole)partyRole).PartyId = 5000;

            var expectedResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockPartyRoleRepository
                .Setup(x => x.UpdatePartyRoleEnterpriseUserID(partyRole))
                .Returns(expectedResponse);

            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Act
            var result = managePartyRole.UpdatePartyRole(partyRole);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void GetPartyRoles_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            long partyId = 1000;
            var expectedPartyRoles = new List<PartyRole>();

            _mockPartyRoleRepository
                .Setup(x => x.GetPartyRoles(partyId))
                .Returns(expectedPartyRoles);

            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Act
            var result = managePartyRole.GetPartyRoles(partyId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetPartyRoles_WithLargePartyId_ReturnsPartyRoleList()
        {
            // Arrange
            long partyId = long.MaxValue;
            var expectedPartyRoles = new List<PartyRole> { CreateValidPartyRole() };

            _mockPartyRoleRepository
                .Setup(x => x.GetPartyRoles(partyId))
                .Returns(expectedPartyRoles);

            var managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            // Act
            var result = managePartyRole.GetPartyRoles(partyId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        #endregion
    }
}
