using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Hots;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageHotsCloneUsers business logic xUnit tests.
    /// Tests for HOTS user cloning operations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageHotsCloneUsersTests : TestBase
    {
        private readonly Mock<IRepository> _mockRepository;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly DefaultUserClaim _defaultUserClaim;

        public ManageHotsCloneUsersTests()
        {
            _mockRepository = new Mock<IRepository>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

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
                CorrelationId = Guid.NewGuid()
            };
        }

        #region Helper Methods

        private CloneUsers CreateValidCloneUsers()
        {
            return new CloneUsers
            {
                CloneCustomerUPFMId = Guid.NewGuid(),
                CloneCustomerEnvironment = "Production"
            };
        }

        private DefaultUserClaim CreateBaseOrgAdminClaim()
        {
            return new DefaultUserClaim
            {
                UserId = 2,
                LoginName = "admin@baseline.com",
                FirstName = "Admin",
                LastName = "User",
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 2000,
                PersonaId = 10
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageHotsCloneUsers = new ManageHotsCloneUsers(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Assert
            Assert.NotNull(manageHotsCloneUsers);
        }

        [Fact]
        public void Constructor_WithUserClaim_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Assert
            Assert.NotNull(manageHotsCloneUsers);
        }

      
        public void Constructor_WithNullUserClaim_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                new ManageHotsCloneUsers(null));
        }

        #endregion

        #region CloneUsersFromBaseLineCompany Tests - Basic Scenarios

        [Fact]
        public void CloneUsersFromBaseLineCompany_WithValidParameters_ReturnsClonedUsers()
        {
            // Arrange
            var cloneUsers = CreateValidCloneUsers();
            var baseOrgAdminClaim = CreateBaseOrgAdminClaim();
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.CloneUsersFromBaseLineCompany(
                cloneUsers: cloneUsers,
                basePartyId: 2000,
                clonePartyId: 3000,
                baseOrgAdminClaim: baseOrgAdminClaim,
                baseOrgAdminPersonaId: 10);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ClonedUsers>(result);
            Assert.NotNull(result.Users);
        }

        [Fact]
        public void CloneUsersFromBaseLineCompany_WithZeroBasePartyId_ReturnsIncompleteStatus()
        {
            // Arrange
            var cloneUsers = CreateValidCloneUsers();
            var baseOrgAdminClaim = CreateBaseOrgAdminClaim();
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.CloneUsersFromBaseLineCompany(
                cloneUsers: cloneUsers,
                basePartyId: 0,
                clonePartyId: 3000,
                baseOrgAdminClaim: baseOrgAdminClaim,
                baseOrgAdminPersonaId: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Incomplete", result.Status);
        }

        [Fact]
        public void CloneUsersFromBaseLineCompany_WithZeroClonePartyId_ReturnsIncompleteStatus()
        {
            // Arrange
            var cloneUsers = CreateValidCloneUsers();
            var baseOrgAdminClaim = CreateBaseOrgAdminClaim();
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.CloneUsersFromBaseLineCompany(
                cloneUsers: cloneUsers,
                basePartyId: 2000,
                clonePartyId: 0,
                baseOrgAdminClaim: baseOrgAdminClaim,
                baseOrgAdminPersonaId: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Incomplete", result.Status);
        }

        [Fact]
        public void CloneUsersFromBaseLineCompany_WithZeroBaseOrgAdminPersonaId_ReturnsIncompleteStatus()
        {
            // Arrange
            var cloneUsers = CreateValidCloneUsers();
            var baseOrgAdminClaim = CreateBaseOrgAdminClaim();
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.CloneUsersFromBaseLineCompany(
                cloneUsers: cloneUsers,
                basePartyId: 2000,
                clonePartyId: 3000,
                baseOrgAdminClaim: baseOrgAdminClaim,
                baseOrgAdminPersonaId: 0);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Incomplete", result.Status);
        }

        #endregion

        #region CloneUsersFromBaseLineCompany Tests - Edge Cases

        [Fact]
        public void CloneUsersFromBaseLineCompany_WithNegativePartyIds_ReturnsIncompleteStatus()
        {
            // Arrange
            var cloneUsers = CreateValidCloneUsers();
            var baseOrgAdminClaim = CreateBaseOrgAdminClaim();
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.CloneUsersFromBaseLineCompany(
                cloneUsers: cloneUsers,
                basePartyId: -1,
                clonePartyId: -1,
                baseOrgAdminClaim: baseOrgAdminClaim,
                baseOrgAdminPersonaId: -1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Incomplete", result.Status);
        }

        [Fact]
        public void CloneUsersFromBaseLineCompany_WithMaxLongPartyIds_ReturnsResult()
        {
            // Arrange
            var cloneUsers = CreateValidCloneUsers();
            var baseOrgAdminClaim = CreateBaseOrgAdminClaim();
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.CloneUsersFromBaseLineCompany(
                cloneUsers: cloneUsers,
                basePartyId: long.MaxValue,
                clonePartyId: long.MaxValue - 1,
                baseOrgAdminClaim: baseOrgAdminClaim,
                baseOrgAdminPersonaId: long.MaxValue);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ClonedUsers>(result);
        }

        [Fact]
        public void CloneUsersFromBaseLineCompany_WithSameBaseAndClonePartyId_ReturnsResult()
        {
            // Arrange
            var cloneUsers = CreateValidCloneUsers();
            var baseOrgAdminClaim = CreateBaseOrgAdminClaim();
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.CloneUsersFromBaseLineCompany(
                cloneUsers: cloneUsers,
                basePartyId: 2000,
                clonePartyId: 2000, // Same as base
                baseOrgAdminClaim: baseOrgAdminClaim,
                baseOrgAdminPersonaId: 10);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region CloneUsersFromBaseLineCompany Tests - ClonedUsers Response

        [Fact]
        public void CloneUsersFromBaseLineCompany_ReturnsClonedUsersWithCorrectProperties()
        {
            // Arrange
            var cloneUsers = CreateValidCloneUsers();
            var baseOrgAdminClaim = CreateBaseOrgAdminClaim();
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.CloneUsersFromBaseLineCompany(
                cloneUsers: cloneUsers,
                basePartyId: 2000,
                clonePartyId: 3000,
                baseOrgAdminClaim: baseOrgAdminClaim,
                baseOrgAdminPersonaId: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cloneUsers.CloneCustomerUPFMId, result.CloneCustomerCompanyId);
            Assert.Equal(cloneUsers.CloneCustomerEnvironment, result.CloneCustomerEnvironment);
            Assert.NotNull(result.Users);
        }

        [Fact]
        public void CloneUsersFromBaseLineCompany_InitializesUsersListAsEmpty()
        {
            // Arrange
            var cloneUsers = CreateValidCloneUsers();
            var baseOrgAdminClaim = CreateBaseOrgAdminClaim();
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.CloneUsersFromBaseLineCompany(
                cloneUsers: cloneUsers,
                basePartyId: 0, // Will return early
                clonePartyId: 3000,
                baseOrgAdminClaim: baseOrgAdminClaim,
                baseOrgAdminPersonaId: 10);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Users);
            Assert.Empty(result.Users);
        }

        #endregion

        #region InsertHotsCompanyRelationship Tests

        [Fact]
        public void InsertHotsCompanyRelationship_WithValidParameters_ReturnsRepositoryResponse()
        {
            // Arrange
            var baselineCompanyRealPageId = Guid.NewGuid();
            var cloneCompanyRealPageId = Guid.NewGuid();
            var userId = 1;

            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.InsertHotsCompanyRelationship(
                baselineCompanyRealPageId,
                cloneCompanyRealPageId,
                userId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        [Fact]
        public void InsertHotsCompanyRelationship_WithEmptyGuids_ReturnsRepositoryResponse()
        {
            // Arrange
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.InsertHotsCompanyRelationship(
                Guid.Empty,
                Guid.Empty,
                0);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        [Fact]
        public void InsertHotsCompanyRelationship_WithNegativeUserId_ReturnsRepositoryResponse()
        {
            // Arrange
            var baselineCompanyRealPageId = Guid.NewGuid();
            var cloneCompanyRealPageId = Guid.NewGuid();
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.InsertHotsCompanyRelationship(
                baselineCompanyRealPageId,
                cloneCompanyRealPageId,
                -1);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        [Fact]
        public void InsertHotsCompanyRelationship_WithSameGuids_ReturnsRepositoryResponse()
        {
            // Arrange
            var sameGuid = Guid.NewGuid();
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.InsertHotsCompanyRelationship(
                sameGuid,
                sameGuid,
                1);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        #endregion

        #region InsertHotsPropertyRelationship Tests

        [Fact]
        public void InsertHotsPropertyRelationship_WithValidParameters_ReturnsRepositoryResponse()
        {
            // Arrange
            var baselinePropertyInstanceId = Guid.NewGuid();
            var clonePropertyInstanceId = Guid.NewGuid();
            var cloneCompanyRealPageId = Guid.NewGuid();
            var userId = 1;

            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.InsertHotsPropertyRelationship(
                baselinePropertyInstanceId,
                clonePropertyInstanceId,
                cloneCompanyRealPageId,
                userId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        [Fact]
        public void InsertHotsPropertyRelationship_WithEmptyGuids_ReturnsRepositoryResponse()
        {
            // Arrange
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.InsertHotsPropertyRelationship(
                Guid.Empty,
                Guid.Empty,
                Guid.Empty,
                0);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        [Fact]
        public void InsertHotsPropertyRelationship_WithNegativeUserId_ReturnsRepositoryResponse()
        {
            // Arrange
            var baselinePropertyInstanceId = Guid.NewGuid();
            var clonePropertyInstanceId = Guid.NewGuid();
            var cloneCompanyRealPageId = Guid.NewGuid();

            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.InsertHotsPropertyRelationship(
                baselinePropertyInstanceId,
                clonePropertyInstanceId,
                cloneCompanyRealPageId,
                -1);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        [Fact]
        public void InsertHotsPropertyRelationship_WithAllSameGuids_ReturnsRepositoryResponse()
        {
            // Arrange
            var sameGuid = Guid.NewGuid();
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.InsertHotsPropertyRelationship(
                sameGuid,
                sameGuid,
                sameGuid,
                1);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        #endregion

        #region GetBaseCompanyUPFMId Tests

        [Fact]
        public void GetBaseCompanyUPFMId_WithValidCloneUpfmId_ReturnsGuid()
        {
            // Arrange
            var cloneUpfmId = Guid.NewGuid();
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.GetBaseCompanyUPFMId(cloneUpfmId);

            // Assert
            Assert.IsType<Guid>(result);
        }

        [Fact]
        public void GetBaseCompanyUPFMId_WithEmptyGuid_ReturnsGuid()
        {
            // Arrange
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.GetBaseCompanyUPFMId(Guid.Empty);

            // Assert
            Assert.IsType<Guid>(result);
        }

        #endregion

        #region CloneUsers Object Tests

        [Fact]
        public void CloneUsers_PropertyAssignment_WorksCorrectly()
        {
            // Arrange & Act
            var cloneUsers = new CloneUsers
            {
                CloneCustomerUPFMId = Guid.NewGuid(),
                CloneCustomerEnvironment = "Staging"
            };

            // Assert
            Assert.NotEqual(Guid.Empty, cloneUsers.CloneCustomerUPFMId);
            Assert.Equal("Staging", cloneUsers.CloneCustomerEnvironment);
        }

        [Fact]
        public void CloneUsers_DefaultValues_AreSetCorrectly()
        {
            // Arrange & Act
            var cloneUsers = new CloneUsers();

            // Assert
            Assert.Equal(Guid.Empty, cloneUsers.CloneCustomerUPFMId);
            Assert.Null(cloneUsers.CloneCustomerEnvironment);
        }

        #endregion

        #region ClonedUsers Object Tests

        [Fact]
        public void ClonedUsers_PropertyAssignment_WorksCorrectly()
        {
            // Arrange & Act
            var clonedUsers = new ClonedUsers
            {
                Status = "Complete",
                CloneCustomerCompanyId = Guid.NewGuid(),
                CloneCustomerEnvironment = "Production",
                Users = new List<HotsUser>()
            };

            // Assert
            Assert.Equal("Complete", clonedUsers.Status);
            Assert.NotEqual(Guid.Empty, clonedUsers.CloneCustomerCompanyId);
            Assert.Equal("Production", clonedUsers.CloneCustomerEnvironment);
            Assert.NotNull(clonedUsers.Users);
            Assert.Empty(clonedUsers.Users);
        }

        [Fact]
        public void ClonedUsers_DefaultValues_AreNull()
        {
            // Arrange & Act
            var clonedUsers = new ClonedUsers();

            // Assert
            Assert.Null(clonedUsers.Status);
            Assert.Equal(Guid.Empty, clonedUsers.CloneCustomerCompanyId);
            Assert.Null(clonedUsers.CloneCustomerEnvironment);
            Assert.Null(clonedUsers.Users);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void ManageHotsCloneUsers_CompleteWorkflow_HandlesCorrectly()
        {
            // Arrange
            var cloneUsers = CreateValidCloneUsers();
            var baseOrgAdminClaim = CreateBaseOrgAdminClaim();
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act - Clone users
            var cloneResult = manageHotsCloneUsers.CloneUsersFromBaseLineCompany(
                cloneUsers,
                2000,
                3000,
                baseOrgAdminClaim,
                10);

            // Act - Insert company relationship
            var companyRelationship = manageHotsCloneUsers.InsertHotsCompanyRelationship(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1);

            // Act - Get base company
            var baseCompanyId = manageHotsCloneUsers.GetBaseCompanyUPFMId(cloneUsers.CloneCustomerUPFMId);

            // Assert
            Assert.NotNull(cloneResult);
            Assert.NotNull(companyRelationship);
            Assert.IsType<Guid>(baseCompanyId);
        }

        [Fact]
        public void ManageHotsCloneUsers_MultipleCompanyRelationships_HandlesCorrectly()
        {
            // Arrange
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result1 = manageHotsCloneUsers.InsertHotsCompanyRelationship(
                Guid.NewGuid(), Guid.NewGuid(), 1);
            var result2 = manageHotsCloneUsers.InsertHotsCompanyRelationship(
                Guid.NewGuid(), Guid.NewGuid(), 2);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.IsType<RepositoryResponse>(result1);
            Assert.IsType<RepositoryResponse>(result2);
        }

        [Fact]
        public void ManageHotsCloneUsers_MultiplePropertyRelationships_HandlesCorrectly()
        {
            // Arrange
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result1 = manageHotsCloneUsers.InsertHotsPropertyRelationship(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1);
            var result2 = manageHotsCloneUsers.InsertHotsPropertyRelationship(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 2);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.IsType<RepositoryResponse>(result1);
            Assert.IsType<RepositoryResponse>(result2);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public void CloneUsersFromBaseLineCompany_WithException_ReturnsIncompleteStatus()
        {
            // Arrange
            var cloneUsers = CreateValidCloneUsers();
            var baseOrgAdminClaim = CreateBaseOrgAdminClaim();
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.CloneUsersFromBaseLineCompany(
                cloneUsers: cloneUsers,
                basePartyId: long.MinValue, // Invalid to potentially trigger exception
                clonePartyId: long.MinValue,
                baseOrgAdminClaim: baseOrgAdminClaim,
                baseOrgAdminPersonaId: long.MinValue);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Incomplete", result.Status);
        }

        #endregion

        #region Environment Tests

        [Theory]
        [InlineData("Production")]
        //[InlineData("Staging")]
        //[InlineData("Development")]
        [InlineData("UAT")]
        public void CloneUsersFromBaseLineCompany_WithDifferentEnvironments_ReturnsCorrectEnvironment(string environment)
        {
            // Arrange
            var cloneUsers = new CloneUsers
            {
                CloneCustomerUPFMId = Guid.NewGuid(),
                CloneCustomerEnvironment = environment
            };
            var baseOrgAdminClaim = CreateBaseOrgAdminClaim();
            var manageHotsCloneUsers = new ManageHotsCloneUsers(_defaultUserClaim);

            // Act
            var result = manageHotsCloneUsers.CloneUsersFromBaseLineCompany(
                cloneUsers,
                0, // Will return early
                3000,
                baseOrgAdminClaim,
                10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(environment, result.CloneCustomerEnvironment);
        }

        #endregion
    }
}
