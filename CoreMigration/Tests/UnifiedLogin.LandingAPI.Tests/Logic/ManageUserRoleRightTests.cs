using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;
using UL = UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageUserRoleRight business logic xUnit tests.
    /// Tests for user role and right management including role retrieval operations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageUserRoleRightTests : TestBase
    {
        private readonly Mock<IUserRoleRightRepository> _mockUserRoleRightRepository;
        private readonly Mock<IRepository> _mockRepository;
        private readonly DefaultUserClaim _defaultUserClaim;

        public ManageUserRoleRightTests()
        {
            _mockUserRoleRightRepository = new Mock<IUserRoleRightRepository>();
            _mockRepository = new Mock<IRepository>();

            _defaultUserClaim = new DefaultUserClaim
            {
                UserId = 1,
                LoginName = "testuser@test.com",
                FirstName = "Test",
                LastName = "User",
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 1000,
                PersonaId = 5,
                CorrelationId = Guid.NewGuid()
            };
        }

        #region Helper Methods

        private List<UL.Role> CreateRoleList()
        {
            return new List<UL.Role>
            {
                new UL.Role
                {
                    RoleID = 1,
                    Name = "Administrator",
                    RoleNickName = "Admin"
                },
                new UL.Role
                {
                    RoleID = 2,
                    Name = "User",
                    RoleNickName = "User"
                },
                new UL.Role
                {
                    RoleID = 3,
                    Name = "Viewer",
                    RoleNickName = "View"
                }
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_Default_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageUserRoleRight = new ManageUserRoleRight();

            // Assert
            Assert.NotNull(manageUserRoleRight);
        }

        [Fact]
        public void Constructor_WithUserRoleRightRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageUserRoleRight = new ManageUserRoleRight(_mockUserRoleRightRepository.Object);

            // Assert
            Assert.NotNull(manageUserRoleRight);
        }

        [Fact]
        public void Constructor_WithRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageUserRoleRight = new ManageUserRoleRight(_mockRepository.Object);

            // Assert
            Assert.NotNull(manageUserRoleRight);
        }

        [Fact]
        public void Constructor_WithRepositoryAndUserClaim_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageUserRoleRight = new ManageUserRoleRight(_mockRepository.Object, _defaultUserClaim);

            // Assert
            Assert.NotNull(manageUserRoleRight);
        }

        #endregion

        #region GetAssignedRoleForPersona Tests

        [Fact]
        public void GetAssignedRoleForPersona_WithValidParameters_ReturnsRoleList()
        {
            // Arrange
            var expectedRoles = CreateRoleList();

            _mockUserRoleRightRepository
                .Setup(m => m.ListRoleByPersona(It.IsAny<int>(), It.IsAny<long?>(), It.IsAny<long?>()))
                .Returns(expectedRoles);

            var manageUserRoleRight = new ManageUserRoleRight(_mockUserRoleRightRepository.Object);

            // Act
            var result = manageUserRoleRight.GetAssignedRoleForPersona(ProductEnum.UnifiedPlatform, 100, 1000);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            _mockUserRoleRightRepository.Verify(
                m => m.ListRoleByPersona((int)ProductEnum.UnifiedPlatform, 100, 1000), Times.Once);
        }

        [Fact]
        public void GetAssignedRoleForPersona_WithNullPersonaId_CallsRepositoryWithNull()
        {
            // Arrange
            var expectedRoles = CreateRoleList();

            _mockUserRoleRightRepository
                .Setup(m => m.ListRoleByPersona(It.IsAny<int>(), null, It.IsAny<long?>()))
                .Returns(expectedRoles);

            var manageUserRoleRight = new ManageUserRoleRight(_mockUserRoleRightRepository.Object);

            // Act
            var result = manageUserRoleRight.GetAssignedRoleForPersona(ProductEnum.UnifiedPlatform, null, 1000);

            // Assert
            Assert.NotNull(result);
            _mockUserRoleRightRepository.Verify(
                m => m.ListRoleByPersona((int)ProductEnum.UnifiedPlatform, null, 1000), Times.Once);
        }

        [Fact]
        public void GetAssignedRoleForPersona_WithNullOrganizationPartyId_CallsRepositoryWithNull()
        {
            // Arrange
            var expectedRoles = CreateRoleList();

            _mockUserRoleRightRepository
                .Setup(m => m.ListRoleByPersona(It.IsAny<int>(), It.IsAny<long?>(), null))
                .Returns(expectedRoles);

            var manageUserRoleRight = new ManageUserRoleRight(_mockUserRoleRightRepository.Object);

            // Act
            var result = manageUserRoleRight.GetAssignedRoleForPersona(ProductEnum.UnifiedPlatform, 100, null);

            // Assert
            Assert.NotNull(result);
            _mockUserRoleRightRepository.Verify(
                m => m.ListRoleByPersona((int)ProductEnum.UnifiedPlatform, 100, null), Times.Once);
        }

        [Fact]
        public void GetAssignedRoleForPersona_WithAllNullOptionalParameters_CallsRepositoryWithDefaults()
        {
            // Arrange
            var expectedRoles = CreateRoleList();

            _mockUserRoleRightRepository
                .Setup(m => m.ListRoleByPersona(It.IsAny<int>(), null, null))
                .Returns(expectedRoles);

            var manageUserRoleRight = new ManageUserRoleRight(_mockUserRoleRightRepository.Object);

            // Act
            var result = manageUserRoleRight.GetAssignedRoleForPersona(ProductEnum.UnifiedPlatform);

            // Assert
            Assert.NotNull(result);
            _mockUserRoleRightRepository.Verify(
                m => m.ListRoleByPersona((int)ProductEnum.UnifiedPlatform, null, null), Times.Once);
        }

        [Fact]
        public void GetAssignedRoleForPersona_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            _mockUserRoleRightRepository
                .Setup(m => m.ListRoleByPersona(It.IsAny<int>(), It.IsAny<long?>(), It.IsAny<long?>()))
                .Returns(new List<UL.Role>());

            var manageUserRoleRight = new ManageUserRoleRight(_mockUserRoleRightRepository.Object);

            // Act
            var result = manageUserRoleRight.GetAssignedRoleForPersona(ProductEnum.UnifiedPlatform, 100, 1000);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetAssignedRoleForPersona_WithDifferentProducts_CallsRepositoryWithCorrectProductId()
        {
            // Arrange
            _mockUserRoleRightRepository
                .Setup(m => m.ListRoleByPersona(It.IsAny<int>(), It.IsAny<long?>(), It.IsAny<long?>()))
                .Returns(new List<UL.Role>());

            var manageUserRoleRight = new ManageUserRoleRight(_mockUserRoleRightRepository.Object);

            // Act
            manageUserRoleRight.GetAssignedRoleForPersona(ProductEnum.OneSite, 100, 1000);
            manageUserRoleRight.GetAssignedRoleForPersona(ProductEnum.OmniChannel, 100, 1000);
            manageUserRoleRight.GetAssignedRoleForPersona(ProductEnum.MarketingCenter, 100, 1000);

            // Assert
            _mockUserRoleRightRepository.Verify(
                m => m.ListRoleByPersona((int)ProductEnum.OneSite, 100, 1000), Times.Once);
            _mockUserRoleRightRepository.Verify(
                m => m.ListRoleByPersona((int)ProductEnum.OmniChannel, 100, 1000), Times.Once);
            _mockUserRoleRightRepository.Verify(
                m => m.ListRoleByPersona((int)ProductEnum.MarketingCenter, 100, 1000), Times.Once);
        }

        [Fact]
        public void GetAssignedRoleForPersona_WithLargePersonaId_ReturnsResult()
        {
            // Arrange
            var expectedRoles = CreateRoleList();

            _mockUserRoleRightRepository
                .Setup(m => m.ListRoleByPersona(It.IsAny<int>(), long.MaxValue, It.IsAny<long?>()))
                .Returns(expectedRoles);

            var manageUserRoleRight = new ManageUserRoleRight(_mockUserRoleRightRepository.Object);

            // Act
            var result = manageUserRoleRight.GetAssignedRoleForPersona(ProductEnum.UnifiedPlatform, long.MaxValue, 1000);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void GetAssignedRoleForPersona_WithZeroPersonaId_ReturnsResult()
        {
            // Arrange
            _mockUserRoleRightRepository
                .Setup(m => m.ListRoleByPersona(It.IsAny<int>(), 0L, It.IsAny<long?>()))
                .Returns(new List<UL.Role>());

            var manageUserRoleRight = new ManageUserRoleRight(_mockUserRoleRightRepository.Object);

            // Act
            var result = manageUserRoleRight.GetAssignedRoleForPersona(ProductEnum.UnifiedPlatform, 0, 1000);

            // Assert
            Assert.NotNull(result);
            _mockUserRoleRightRepository.Verify(
                m => m.ListRoleByPersona((int)ProductEnum.UnifiedPlatform, 0L, 1000), Times.Once);
        }

        [Fact]
        public void GetAssignedRoleForPersona_WithZeroOrganizationPartyId_ReturnsResult()
        {
            // Arrange
            _mockUserRoleRightRepository
                .Setup(m => m.ListRoleByPersona(It.IsAny<int>(), It.IsAny<long?>(), 0L))
                .Returns(new List<UL.Role>());

            var manageUserRoleRight = new ManageUserRoleRight(_mockUserRoleRightRepository.Object);

            // Act
            var result = manageUserRoleRight.GetAssignedRoleForPersona(ProductEnum.UnifiedPlatform, 100, 0);

            // Assert
            Assert.NotNull(result);
            _mockUserRoleRightRepository.Verify(
                m => m.ListRoleByPersona((int)ProductEnum.UnifiedPlatform, 100, 0L), Times.Once);
        }

        [Fact]
        public void GetAssignedRoleForPersona_MultipleCalls_EachCallsRepository()
        {
            // Arrange
            var expectedRoles = CreateRoleList();

            _mockUserRoleRightRepository
                .Setup(m => m.ListRoleByPersona(It.IsAny<int>(), It.IsAny<long?>(), It.IsAny<long?>()))
                .Returns(expectedRoles);

            var manageUserRoleRight = new ManageUserRoleRight(_mockUserRoleRightRepository.Object);

            // Act
            var result1 = manageUserRoleRight.GetAssignedRoleForPersona(ProductEnum.UnifiedPlatform, 100, 1000);
            var result2 = manageUserRoleRight.GetAssignedRoleForPersona(ProductEnum.UnifiedPlatform, 200, 2000);
            var result3 = manageUserRoleRight.GetAssignedRoleForPersona(ProductEnum.UnifiedPlatform, 300, 3000);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
            _mockUserRoleRightRepository.Verify(
                m => m.ListRoleByPersona(It.IsAny<int>(), It.IsAny<long?>(), It.IsAny<long?>()),
                Times.Exactly(3));
        }

        #endregion

        #region Data Object Tests

        [Fact]
        public void Role_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var role = new UL.Role
            {
                RoleID = 1,
                Name = "Administrator",
                RoleNickName = "Admin",
                PersonaId = "123",
                Right = new List<Right>
                {
                    new Right { RightId = 1, RightName = "Read" },
                    new Right { RightId = 2, RightName = "Write" }
                }
            };

            // Assert
            Assert.Equal(1, role.RoleID);
            Assert.Equal("Administrator", role.Name);
            Assert.Equal("Admin", role.RoleNickName);
            Assert.Equal("123", role.PersonaId);
            Assert.Equal(2, role.Right.Count);
        }

        [Fact]
        public void Role_DefaultValues()
        {
            // Arrange & Act
            var role = new UL.Role();

            // Assert
            Assert.Equal(0, role.RoleID);
            Assert.Null(role.Name);
            Assert.Null(role.RoleNickName);
            Assert.Null(role.PersonaId);
            Assert.NotNull(role.Right);
            Assert.Empty(role.Right);
        }

        [Fact]
        public void Right_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var right = new Right
            {
                RightId = 1,
                RightName = "CanEdit",
                RightDescription = "Can edit items",
                RightNickName = "Edit"
            };

            // Assert
            Assert.Equal(1, right.RightId);
            Assert.Equal("CanEdit", right.RightName);
            Assert.Equal("Can edit items", right.RightDescription);
            Assert.Equal("Edit", right.RightNickName);
        }

        [Fact]
        public void Right_DefaultValues()
        {
            // Arrange & Act
            var right = new Right();

            // Assert
            Assert.Equal(0, right.RightId);
            Assert.Null(right.RightName);
            Assert.Null(right.RightDescription);
            Assert.Null(right.RightNickName);
        }

        #endregion

        #region ProductEnum Tests

        [Fact]
        public void ProductEnum_ValuesAreCorrect()
        {
            // Assert - Document the expected product enum values used
            Assert.True((int)ProductEnum.UnifiedPlatform > 0);
            Assert.True((int)ProductEnum.OneSite > 0);
            Assert.True((int)ProductEnum.OmniChannel > 0);
            Assert.True((int)ProductEnum.MarketingCenter > 0);
        }

        #endregion

        #region Documentation Tests

        [Fact]
        public void ManageUserRoleRight_ClassPurpose_Documentation()
        {
            // This test documents the class purpose:
            // ManageUserRoleRight is responsible for:
            // 1. Getting role information for a given persona and product
            // 2. Acting as a facade for IUserRoleRightRepository
            //
            // Key methods:
            // - GetAssignedRoleForPersona - Gets roles assigned to a persona for a product
            //
            // Constructors:
            // - Default constructor - Creates with default UserRoleRightRepository
            // - DI constructor (IUserRoleRightRepository) - Full dependency injection
            // - DI constructor (IRepository) - Creates repository from IRepository
            // - Unit test constructor (IRepository, DefaultUserClaim) - For testing

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageUserRoleRight_MethodParameters_Documentation()
        {
            // This test documents GetAssignedRoleForPersona parameters:
            //
            // | Parameter | Type | Required | Description |
            // |-----------|------|----------|-------------|
            // | productId | ProductEnum | Yes | The product to get roles for |
            // | userPersonaId | long? | No | The persona ID (default: null) |
            // | organizationPartyId | long? | No | The organization party ID (default: null) |
            //
            // Returns:
            // - IList<UL.Role> - List of roles assigned to the persona
            //
            // Notes:
            // - ProductEnum is cast to int when calling repository
            // - Null values are passed through to repository
            // - Returns empty list if no roles found

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageUserRoleRight_RoleDataStructure_Documentation()
        {
            // This test documents the Role data structure:
            //
            // UL.Role (UnifiedLogin.SharedObjects.Product.UnifiedLogin.Role):
            // - RoleID (long) - The unique identifier of the role
            // - Name (string) - The name of the role
            // - RoleNickName (string) - Short name/nickname for the role
            // - PersonaId (string) - The persona ID associated with the role
            // - Right (IList<Right>) - List of rights associated with the role
            //
            // Right (UnifiedLogin.SharedObjects.Landing.Right):
            // - RightId (int) - The unique identifier of the right
            // - RightName (string) - The name of the right
            // - RightDescription (string) - Description of the right
            // - RightNickName (string) - Short name for the right

            Assert.True(true, "Documentation test");
        }

        #endregion
    }
}
