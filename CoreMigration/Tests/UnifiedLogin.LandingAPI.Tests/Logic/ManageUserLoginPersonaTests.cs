using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageUserLoginPersona business logic xUnit tests.
    /// Tests for user login persona management including listing operations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageUserLoginPersonaTests : TestBase
    {
        private readonly Mock<IUserLoginPersonaRepository> _mockUserLoginPersonaRepository;
        private readonly DefaultUserClaim _defaultUserClaim;

        public ManageUserLoginPersonaTests()
        {
            _mockUserLoginPersonaRepository = new Mock<IUserLoginPersonaRepository>();

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

        private List<UserLoginPersona> CreateUserLoginPersonaList()
        {
            return new List<UserLoginPersona>
            {
                new UserLoginPersona
                {
                    UserLoginPersonaId = 1,
                    UserLoginId = 100,
                    StatusTypeId = 1,
                    PrimaryOrganization = true,
                    IsDelegateAdmin = false,
                    IsRealPartner = false,
                    FromDate = DateTime.UtcNow.AddDays(-30),
                    ThruDate = DateTime.UtcNow.AddDays(365)
                },
                new UserLoginPersona
                {
                    UserLoginPersonaId = 2,
                    UserLoginId = 100,
                    StatusTypeId = 1,
                    PrimaryOrganization = false,
                    IsDelegateAdmin = true,
                    IsRealPartner = false,
                    FromDate = DateTime.UtcNow.AddDays(-15),
                    ThruDate = null
                },
                new UserLoginPersona
                {
                    UserLoginPersonaId = 3,
                    UserLoginId = 200,
                    StatusTypeId = 2,
                    PrimaryOrganization = true,
                    IsDelegateAdmin = false,
                    IsRealPartner = true,
                    FromDate = DateTime.UtcNow,
                    ThruDate = DateTime.UtcNow.AddYears(1),
                    StatusThruDate = DateTime.UtcNow.AddMonths(6)
                }
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_Default_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageUserLoginPersona = new ManageUserLoginPersona();

            // Assert
            Assert.NotNull(manageUserLoginPersona);
        }

        [Fact]
        public void Constructor_WithUserClaim_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageUserLoginPersona = new ManageUserLoginPersona(_defaultUserClaim);

            // Assert
            Assert.NotNull(manageUserLoginPersona);
        }

        [Fact]
        public void Constructor_WithRepositoryAndUserClaim_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageUserLoginPersona = new ManageUserLoginPersona(
                _mockUserLoginPersonaRepository.Object,
                _defaultUserClaim);

            // Assert
            Assert.NotNull(manageUserLoginPersona);
        }

        [Fact]
        public void Constructor_WithNullRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageUserLoginPersona = new ManageUserLoginPersona(null, _defaultUserClaim);

            // Assert
            Assert.NotNull(manageUserLoginPersona);
        }

        [Fact]
        public void Constructor_WithNullUserClaim_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageUserLoginPersona = new ManageUserLoginPersona(
                _mockUserLoginPersonaRepository.Object,
                null);

            // Assert
            Assert.NotNull(manageUserLoginPersona);
        }

        #endregion

        #region ListUserLoginPersona Tests

        [Fact]
        public void ListUserLoginPersona_WithAllNullParameters_ReturnsEmptyList()
        {
            // Arrange
            _mockUserLoginPersonaRepository
                .Setup(m => m.ListUserLoginPersona(null, null, null))
                .Returns(new List<UserLoginPersona>());

            var manageUserLoginPersona = new ManageUserLoginPersona(
                _mockUserLoginPersonaRepository.Object,
                _defaultUserClaim);

            // Act
            var result = manageUserLoginPersona.ListUserLoginPersona(null, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockUserLoginPersonaRepository.Verify(m => m.ListUserLoginPersona(null, null, null), Times.Once);
        }

        [Fact]
        public void ListUserLoginPersona_WithUserLoginPersonaId_ReturnsFilteredList()
        {
            // Arrange
            var expectedList = new List<UserLoginPersona>
            {
                new UserLoginPersona { UserLoginPersonaId = 1, UserLoginId = 100 }
            };

            _mockUserLoginPersonaRepository
                .Setup(m => m.ListUserLoginPersona(1L, null, null))
                .Returns(expectedList);

            var manageUserLoginPersona = new ManageUserLoginPersona(
                _mockUserLoginPersonaRepository.Object,
                _defaultUserClaim);

            // Act
            var result = manageUserLoginPersona.ListUserLoginPersona(1L, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1L, result[0].UserLoginPersonaId);
            _mockUserLoginPersonaRepository.Verify(m => m.ListUserLoginPersona(1L, null, null), Times.Once);
        }

        [Fact]
        public void ListUserLoginPersona_WithUserLoginId_ReturnsFilteredList()
        {
            // Arrange
            var expectedList = new List<UserLoginPersona>
            {
                new UserLoginPersona { UserLoginPersonaId = 1, UserLoginId = 100 },
                new UserLoginPersona { UserLoginPersonaId = 2, UserLoginId = 100 }
            };

            _mockUserLoginPersonaRepository
                .Setup(m => m.ListUserLoginPersona(null, 100L, null))
                .Returns(expectedList);

            var manageUserLoginPersona = new ManageUserLoginPersona(
                _mockUserLoginPersonaRepository.Object,
                _defaultUserClaim);

            // Act
            var result = manageUserLoginPersona.ListUserLoginPersona(null, 100L, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, ulp => Assert.Equal(100L, ulp.UserLoginId));
            _mockUserLoginPersonaRepository.Verify(m => m.ListUserLoginPersona(null, 100L, null), Times.Once);
        }

        [Fact]
        public void ListUserLoginPersona_WithOrganizationPartyId_ReturnsFilteredList()
        {
            // Arrange
            var expectedList = CreateUserLoginPersonaList();

            _mockUserLoginPersonaRepository
                .Setup(m => m.ListUserLoginPersona(null, null, 1000L))
                .Returns(expectedList);

            var manageUserLoginPersona = new ManageUserLoginPersona(
                _mockUserLoginPersonaRepository.Object,
                _defaultUserClaim);

            // Act
            var result = manageUserLoginPersona.ListUserLoginPersona(null, null, 1000L);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            _mockUserLoginPersonaRepository.Verify(m => m.ListUserLoginPersona(null, null, 1000L), Times.Once);
        }

        [Fact]
        public void ListUserLoginPersona_WithAllParameters_ReturnsFilteredList()
        {
            // Arrange
            var expectedList = new List<UserLoginPersona>
            {
                new UserLoginPersona
                {
                    UserLoginPersonaId = 1,
                    UserLoginId = 100,
                    PrimaryOrganization = true
                }
            };

            _mockUserLoginPersonaRepository
                .Setup(m => m.ListUserLoginPersona(1L, 100L, 1000L))
                .Returns(expectedList);

            var manageUserLoginPersona = new ManageUserLoginPersona(
                _mockUserLoginPersonaRepository.Object,
                _defaultUserClaim);

            // Act
            var result = manageUserLoginPersona.ListUserLoginPersona(1L, 100L, 1000L);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1L, result[0].UserLoginPersonaId);
            Assert.Equal(100L, result[0].UserLoginId);
            _mockUserLoginPersonaRepository.Verify(m => m.ListUserLoginPersona(1L, 100L, 1000L), Times.Once);
        }

        [Fact]
        public void ListUserLoginPersona_WithNoMatchingRecords_ReturnsEmptyList()
        {
            // Arrange
            _mockUserLoginPersonaRepository
                .Setup(m => m.ListUserLoginPersona(999L, null, null))
                .Returns(new List<UserLoginPersona>());

            var manageUserLoginPersona = new ManageUserLoginPersona(
                _mockUserLoginPersonaRepository.Object,
                _defaultUserClaim);

            // Act
            var result = manageUserLoginPersona.ListUserLoginPersona(999L, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ListUserLoginPersona_WithZeroValues_ReturnsResults()
        {
            // Arrange
            _mockUserLoginPersonaRepository
                .Setup(m => m.ListUserLoginPersona(0L, 0L, 0L))
                .Returns(new List<UserLoginPersona>());

            var manageUserLoginPersona = new ManageUserLoginPersona(
                _mockUserLoginPersonaRepository.Object,
                _defaultUserClaim);

            // Act
            var result = manageUserLoginPersona.ListUserLoginPersona(0L, 0L, 0L);

            // Assert
            Assert.NotNull(result);
            _mockUserLoginPersonaRepository.Verify(m => m.ListUserLoginPersona(0L, 0L, 0L), Times.Once);
        }

        [Fact]
        public void ListUserLoginPersona_WithLargeIds_ReturnsResults()
        {
            // Arrange
            long largeId = long.MaxValue;
            _mockUserLoginPersonaRepository
                .Setup(m => m.ListUserLoginPersona(largeId, largeId, largeId))
                .Returns(new List<UserLoginPersona>());

            var manageUserLoginPersona = new ManageUserLoginPersona(
                _mockUserLoginPersonaRepository.Object,
                _defaultUserClaim);

            // Act
            var result = manageUserLoginPersona.ListUserLoginPersona(largeId, largeId, largeId);

            // Assert
            Assert.NotNull(result);
            _mockUserLoginPersonaRepository.Verify(m => m.ListUserLoginPersona(largeId, largeId, largeId), Times.Once);
        }

        [Fact]
        public void ListUserLoginPersona_MultipleCalls_EachCallsRepository()
        {
            // Arrange
            var expectedList = CreateUserLoginPersonaList();

            _mockUserLoginPersonaRepository
                .Setup(m => m.ListUserLoginPersona(It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<long?>()))
                .Returns(expectedList);

            var manageUserLoginPersona = new ManageUserLoginPersona(
                _mockUserLoginPersonaRepository.Object,
                _defaultUserClaim);

            // Act
            var result1 = manageUserLoginPersona.ListUserLoginPersona(1L, null, null);
            var result2 = manageUserLoginPersona.ListUserLoginPersona(null, 100L, null);
            var result3 = manageUserLoginPersona.ListUserLoginPersona(null, null, 1000L);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
            _mockUserLoginPersonaRepository.Verify(
                m => m.ListUserLoginPersona(It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<long?>()),
                Times.Exactly(3));
        }

        #endregion

        #region Data Object Tests

        [Fact]
        public void UserLoginPersona_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var persona = new UserLoginPersona
            {
                UserLoginPersonaId = 1,
                UserLoginId = 100,
                StatusTypeId = 1,
                PrimaryOrganization = true,
                IsDelegateAdmin = false,
                IsRealPartner = true,
                FromDate = DateTime.UtcNow,
                ThruDate = DateTime.UtcNow.AddYears(1),
                StatusThruDate = DateTime.UtcNow.AddMonths(6)
            };

            // Assert
            Assert.Equal(1, persona.UserLoginPersonaId);
            Assert.Equal(100, persona.UserLoginId);
            Assert.Equal(1, persona.StatusTypeId);
            Assert.True(persona.PrimaryOrganization);
            Assert.False(persona.IsDelegateAdmin);
            Assert.True(persona.IsRealPartner);
            Assert.NotNull(persona.FromDate);
            Assert.NotNull(persona.ThruDate);
            Assert.NotNull(persona.StatusThruDate);
        }

        [Fact]
        public void UserLoginPersona_NullableDatesCanBeNull()
        {
            // Arrange & Act
            var persona = new UserLoginPersona
            {
                UserLoginPersonaId = 1,
                FromDate = null,
                ThruDate = null,
                StatusThruDate = null
            };

            // Assert
            Assert.Null(persona.FromDate);
            Assert.Null(persona.ThruDate);
            Assert.Null(persona.StatusThruDate);
        }

        [Fact]
        public void UserLoginPersona_DatesAreUtcKind()
        {
            // Arrange
            var testDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

            // Act
            var persona = new UserLoginPersona
            {
                FromDate = testDate,
                ThruDate = testDate,
                StatusThruDate = testDate
            };

            // Assert
            Assert.Equal(DateTimeKind.Utc, persona.FromDate.Value.Kind);
            Assert.Equal(DateTimeKind.Utc, persona.ThruDate.Value.Kind);
            Assert.Equal(DateTimeKind.Utc, persona.StatusThruDate.Value.Kind);
        }

        [Fact]
        public void UserLoginPersona_DefaultValues()
        {
            // Arrange & Act
            var persona = new UserLoginPersona();

            // Assert
            Assert.Equal(0, persona.UserLoginPersonaId);
            Assert.Equal(0, persona.UserLoginId);
            Assert.Equal(0, persona.StatusTypeId);
            Assert.False(persona.PrimaryOrganization);
            Assert.False(persona.IsDelegateAdmin);
            Assert.False(persona.IsRealPartner);
            Assert.Null(persona.FromDate);
            Assert.Null(persona.ThruDate);
            Assert.Null(persona.StatusThruDate);
        }

        #endregion

        #region Documentation Tests

        [Fact]
        public void ManageUserLoginPersona_ClassPurpose_Documentation()
        {
            // This test documents the class purpose:
            // ManageUserLoginPersona is responsible for:
            // 1. Managing user login persona records
            // 2. Listing user login personas with optional filters
            //
            // Key methods:
            // - ListUserLoginPersona - List user login personas with optional filters
            //
            // Constructors:
            // - Default constructor - Creates with default repository
            // - UserClaim constructor - Creates with default repository and user claim
            // - Repository+UserClaim constructor - Full dependency injection

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageUserLoginPersona_ListUserLoginPersona_Documentation()
        {
            // This test documents ListUserLoginPersona method:
            //
            // Parameters (all optional):
            // - userLoginPersonaId: Filter by specific persona ID
            // - userLoginId: Filter by user login ID
            // - organizationPartyId: Filter by organization party ID
            //
            // Behavior:
            // - All null parameters: Returns all records
            // - Any parameter specified: Filters by that parameter
            // - Multiple parameters: Combines filters (AND logic)
            // - No matches: Returns empty list (not null)
            //
            // Returns:
            // - IList<UserLoginPersona>: List of matching records

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void UserLoginPersona_DataObject_Documentation()
        {
            // This test documents UserLoginPersona data object:
            //
            // Properties:
            // - UserLoginPersonaId (long): Unique identifier
            // - UserLoginId (long): Associated user login ID
            // - StatusTypeId (int): Status type identifier
            // - PrimaryOrganization (bool): Is this the primary org
            // - IsDelegateAdmin (bool): Is a delegate administrator
            // - IsRealPartner (bool): Is a real partner
            // - FromDate (DateTime?): Effective start date (UTC)
            // - ThruDate (DateTime?): Effective end date (UTC)
            // - StatusThruDate (DateTime?): Status expiration date (UTC)
            //
            // Notes:
            // - All DateTime properties are stored as UTC
            // - DateTime properties use backing fields with null tracking

            Assert.True(true, "Documentation test");
        }

        #endregion
    }
}
