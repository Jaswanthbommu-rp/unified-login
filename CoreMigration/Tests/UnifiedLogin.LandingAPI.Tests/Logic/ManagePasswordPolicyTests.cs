using System;
using System.Diagnostics.CodeAnalysis;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManagePasswordPolicy business logic xUnit tests.
    /// Tests for password policy management operations including CRUD operations.
    /// Migrated from RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.ManagePasswordPolicy
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManagePasswordPolicyTests : TestBase
    {
        private readonly Mock<IPasswordPolicyRepository> _mockPasswordPolicyRepository;

        public ManagePasswordPolicyTests()
        {
            _mockPasswordPolicyRepository = new Mock<IPasswordPolicyRepository>();
        }

        #region Helper Methods

        private PasswordPolicy CreateValidPasswordPolicy()
        {
            return new PasswordPolicy
            {
                PasswordPolicyId = 1,
                PartyId = 1000,
                Name = "Test Policy",
                MinimumLength = 8,
                MaximumLength = 128,
                MinimumLowercase = 1,
                MinimumUppercase = 1,
                MinimumNumeric = 1,
                MinimumSpecialCharacter = 1,
                PasswordExpirationPeriodInDays = 90,
                NumberOfPasswordsToRemember = 5,
                AllowUsersToChangeOwnPassword = true,
                EnablePasswordExpiration = true,
                PreventPasswordReuse = true
            };
        }

        private IPasswordPolicy CreateValidIPasswordPolicy()
        {
            return new PasswordPolicy
            {
                PasswordPolicyId = 1,
                PartyId = 1000,
                Name = "Test Policy",
                MinimumLength = 8,
                MaximumLength = 128,
                MinimumLowercase = 1,
                MinimumUppercase = 1,
                MinimumNumeric = 1,
                MinimumSpecialCharacter = 1,
                PasswordExpirationPeriodInDays = 90,
                NumberOfPasswordsToRemember = 5,
                AllowUsersToChangeOwnPassword = true,
                EnablePasswordExpiration = true,
                PreventPasswordReuse = true
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNoParameters_InitializesSuccessfully()
        {
            // Arrange & Act
            var managePasswordPolicy = new ManagePasswordPolicy();

            // Assert
            Assert.NotNull(managePasswordPolicy);
        }

        [Fact]
        public void Constructor_WithPasswordPolicyRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var managePasswordPolicy = new ManagePasswordPolicy(_mockPasswordPolicyRepository.Object);

            // Assert
            Assert.NotNull(managePasswordPolicy);
        }

        #endregion

        #region CreatePasswordPolicy Tests

        [Fact]
        public void CreatePasswordPolicy_WithValidPasswordPolicy_ReturnsSuccessResponse()
        {
            // Arrange
            var passwordPolicy = CreateValidIPasswordPolicy();
            var expectedResponse = new RepositoryResponse
            {
                Id = 1,
                RealPageId = Guid.NewGuid(),
                ErrorMessage = ""
            };

            _mockPasswordPolicyRepository
                .Setup(x => x.CreatePasswordPolicy(passwordPolicy))
                .Returns(expectedResponse);

            var managePasswordPolicy = new ManagePasswordPolicy(_mockPasswordPolicyRepository.Object);

            // Act
            var result = managePasswordPolicy.CreatePasswordPolicy(passwordPolicy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            Assert.Empty(result.ErrorMessage);
            _mockPasswordPolicyRepository.Verify(x => x.CreatePasswordPolicy(passwordPolicy), Times.Once);
        }

        [Fact]
        public void CreatePasswordPolicy_WithNullPasswordPolicy_ThrowsArgumentNullException()
        {
            // Arrange
            IPasswordPolicy passwordPolicy = null;
            var managePasswordPolicy = new ManagePasswordPolicy(_mockPasswordPolicyRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                managePasswordPolicy.CreatePasswordPolicy(passwordPolicy));

            Assert.Equal("passwordPolicy", exception.ParamName);
            Assert.Contains("Null Password Policy", exception.Message);
        }

        [Fact]
        public void CreatePasswordPolicy_WithDifferentPolicySettings_CallsRepositoryCorrectly()
        {
            // Arrange
            var passwordPolicy = CreateValidIPasswordPolicy();
            ((PasswordPolicy)passwordPolicy).MinimumLength = 12;
            ((PasswordPolicy)passwordPolicy).PasswordExpirationPeriodInDays = 60;

            var expectedResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockPasswordPolicyRepository
                .Setup(x => x.CreatePasswordPolicy(passwordPolicy))
                .Returns(expectedResponse);

            var managePasswordPolicy = new ManagePasswordPolicy(_mockPasswordPolicyRepository.Object);

            // Act
            var result = managePasswordPolicy.CreatePasswordPolicy(passwordPolicy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            _mockPasswordPolicyRepository.Verify(x => x.CreatePasswordPolicy(passwordPolicy), Times.Once);
        }

        #endregion

        #region GetPasswordPolicy Tests

        [Fact]
        public void GetPasswordPolicy_WithValidPartyId_ReturnsPasswordPolicy()
        {
            // Arrange
            long partyId = 1000;
            var expectedPasswordPolicy = CreateValidPasswordPolicy();

            _mockPasswordPolicyRepository
                .Setup(x => x.GetPasswordPolicy(partyId))
                .Returns(expectedPasswordPolicy);

            var managePasswordPolicy = new ManagePasswordPolicy(_mockPasswordPolicyRepository.Object);

            // Act
            var result = managePasswordPolicy.GetPasswordPolicy(partyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPasswordPolicy.PasswordPolicyId, result.PasswordPolicyId);
            Assert.Equal(expectedPasswordPolicy.PartyId, result.PartyId);
            Assert.Equal(expectedPasswordPolicy.MinimumLength, result.MinimumLength);
            _mockPasswordPolicyRepository.Verify(x => x.GetPasswordPolicy(partyId), Times.Once);
        }

        [Fact]
        public void GetPasswordPolicy_WithZeroPartyId_ThrowsException()
        {
            // Arrange
            long partyId = 0;
            var managePasswordPolicy = new ManagePasswordPolicy(_mockPasswordPolicyRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePasswordPolicy.GetPasswordPolicy(partyId));

            Assert.Equal("Missing Party Id.", exception.Message);
        }

        [Fact]
        public void GetPasswordPolicy_WithNegativePartyId_ThrowsException()
        {
            // Arrange
            long partyId = -1;
            var managePasswordPolicy = new ManagePasswordPolicy(_mockPasswordPolicyRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePasswordPolicy.GetPasswordPolicy(partyId));

            Assert.Equal("Missing Party Id.", exception.Message);
        }

        [Fact]
        public void GetPasswordPolicy_WhenRepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            long partyId = 1000;

            _mockPasswordPolicyRepository
                .Setup(x => x.GetPasswordPolicy(partyId))
                .Returns((PasswordPolicy)null);

            var managePasswordPolicy = new ManagePasswordPolicy(_mockPasswordPolicyRepository.Object);

            // Act
            var result = managePasswordPolicy.GetPasswordPolicy(partyId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetPasswordPolicy_WithLargePartyId_ReturnsPasswordPolicy()
        {
            // Arrange
            long partyId = long.MaxValue;
            var expectedPasswordPolicy = CreateValidPasswordPolicy();

            _mockPasswordPolicyRepository
                .Setup(x => x.GetPasswordPolicy(partyId))
                .Returns(expectedPasswordPolicy);

            var managePasswordPolicy = new ManagePasswordPolicy(_mockPasswordPolicyRepository.Object);

            // Act
            var result = managePasswordPolicy.GetPasswordPolicy(partyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPasswordPolicy.PasswordPolicyId, result.PasswordPolicyId);
        }

        #endregion

        #region UpdatePasswordPolicy Tests

        [Fact]
        public void UpdatePasswordPolicy_WithValidPasswordPolicy_ReturnsSuccessResponse()
        {
            // Arrange
            var passwordPolicy = CreateValidIPasswordPolicy();
            var expectedResponse = new RepositoryResponse
            {
                Id = 1,
                RealPageId = Guid.NewGuid(),
                ErrorMessage = ""
            };

            _mockPasswordPolicyRepository
                .Setup(x => x.UpdatePasswordPolicy(passwordPolicy))
                .Returns(expectedResponse);

            var managePasswordPolicy = new ManagePasswordPolicy(_mockPasswordPolicyRepository.Object);

            // Act
            var result = managePasswordPolicy.UpdatePasswordPolicy(passwordPolicy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            Assert.Empty(result.ErrorMessage);
            _mockPasswordPolicyRepository.Verify(x => x.UpdatePasswordPolicy(passwordPolicy), Times.Once);
        }

        [Fact]
        public void UpdatePasswordPolicy_WithNullPasswordPolicy_ThrowsArgumentNullException()
        {
            // Arrange
            IPasswordPolicy passwordPolicy = null;
            var managePasswordPolicy = new ManagePasswordPolicy(_mockPasswordPolicyRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                managePasswordPolicy.UpdatePasswordPolicy(passwordPolicy));

            Assert.Equal("passwordPolicy", exception.ParamName);
            Assert.Contains("Null Password Policy", exception.Message);
        }

        [Fact]
        public void UpdatePasswordPolicy_WithModifiedSettings_CallsRepositoryCorrectly()
        {
            // Arrange
            var passwordPolicy = CreateValidIPasswordPolicy();
            ((PasswordPolicy)passwordPolicy).MinimumLength = 10;
            ((PasswordPolicy)passwordPolicy).NumberOfPasswordsToRemember = 10;

            var expectedResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockPasswordPolicyRepository
                .Setup(x => x.UpdatePasswordPolicy(passwordPolicy))
                .Returns(expectedResponse);

            var managePasswordPolicy = new ManagePasswordPolicy(_mockPasswordPolicyRepository.Object);

            // Act
            var result = managePasswordPolicy.UpdatePasswordPolicy(passwordPolicy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            _mockPasswordPolicyRepository.Verify(x => x.UpdatePasswordPolicy(passwordPolicy), Times.Once);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void ManagePasswordPolicy_CompleteWorkflow_CreateGetUpdate()
        {
            // Arrange
            long partyId = 1000;
            var passwordPolicy = CreateValidIPasswordPolicy();
            var expectedPasswordPolicy = CreateValidPasswordPolicy();
            var createResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };
            var updateResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockPasswordPolicyRepository
                .Setup(x => x.CreatePasswordPolicy(passwordPolicy))
                .Returns(createResponse);

            _mockPasswordPolicyRepository
                .Setup(x => x.GetPasswordPolicy(partyId))
                .Returns(expectedPasswordPolicy);

            _mockPasswordPolicyRepository
                .Setup(x => x.UpdatePasswordPolicy(passwordPolicy))
                .Returns(updateResponse);

            var managePasswordPolicy = new ManagePasswordPolicy(_mockPasswordPolicyRepository.Object);

            // Act
            var createResult = managePasswordPolicy.CreatePasswordPolicy(passwordPolicy);
            var getResult = managePasswordPolicy.GetPasswordPolicy(partyId);
            var updateResult = managePasswordPolicy.UpdatePasswordPolicy(passwordPolicy);

            // Assert
            Assert.NotNull(createResult);
            Assert.NotNull(getResult);
            Assert.NotNull(updateResult);
            Assert.Equal(1, createResult.Id);
            Assert.Equal(1, getResult.PasswordPolicyId);
            Assert.Equal(1, updateResult.Id);
        }

        [Fact]
        public void ManagePasswordPolicy_MultipleParties_HandlesCorrectly()
        {
            // Arrange
            long partyId1 = 1000;
            long partyId2 = 2000;
            var passwordPolicy1 = CreateValidPasswordPolicy();
            var passwordPolicy2 = CreateValidPasswordPolicy();
            passwordPolicy2.PartyId = partyId2;
            passwordPolicy2.PasswordPolicyId = 2;

            _mockPasswordPolicyRepository
                .Setup(x => x.GetPasswordPolicy(partyId1))
                .Returns(passwordPolicy1);

            _mockPasswordPolicyRepository
                .Setup(x => x.GetPasswordPolicy(partyId2))
                .Returns(passwordPolicy2);

            var managePasswordPolicy = new ManagePasswordPolicy(_mockPasswordPolicyRepository.Object);

            // Act
            var result1 = managePasswordPolicy.GetPasswordPolicy(partyId1);
            var result2 = managePasswordPolicy.GetPasswordPolicy(partyId2);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Equal(partyId1, result1.PartyId);
            Assert.Equal(partyId2, result2.PartyId);
            Assert.NotEqual(result1.PasswordPolicyId, result2.PasswordPolicyId);
        }

        #endregion

        #region Edge Cases and Additional Scenarios

        [Fact]
        public void CreatePasswordPolicy_WithMinimalRequirements_CallsRepositoryCorrectly()
        {
            // Arrange
            var passwordPolicy = CreateValidIPasswordPolicy();
            ((PasswordPolicy)passwordPolicy).MinimumLowercase = 0;
            ((PasswordPolicy)passwordPolicy).MinimumUppercase = 0;
            ((PasswordPolicy)passwordPolicy).MinimumNumeric = 0;
            ((PasswordPolicy)passwordPolicy).MinimumSpecialCharacter = 0;

            var expectedResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockPasswordPolicyRepository
                .Setup(x => x.CreatePasswordPolicy(passwordPolicy))
                .Returns(expectedResponse);

            var managePasswordPolicy = new ManagePasswordPolicy(_mockPasswordPolicyRepository.Object);

            // Act
            var result = managePasswordPolicy.CreatePasswordPolicy(passwordPolicy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void UpdatePasswordPolicy_WithStrictRequirements_CallsRepositoryCorrectly()
        {
            // Arrange
            var passwordPolicy = CreateValidIPasswordPolicy();
            ((PasswordPolicy)passwordPolicy).MinimumLength = 16;
            ((PasswordPolicy)passwordPolicy).MinimumUppercase = 2;
            ((PasswordPolicy)passwordPolicy).MinimumLowercase = 2;

            var expectedResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockPasswordPolicyRepository
                .Setup(x => x.UpdatePasswordPolicy(passwordPolicy))
                .Returns(expectedResponse);

            var managePasswordPolicy = new ManagePasswordPolicy(_mockPasswordPolicyRepository.Object);

            // Act
            var result = managePasswordPolicy.UpdatePasswordPolicy(passwordPolicy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(999999)]
        [InlineData(long.MaxValue)]
        public void GetPasswordPolicy_WithVariousValidPartyIds_ReturnsPasswordPolicy(long partyId)
        {
            // Arrange
            var expectedPasswordPolicy = CreateValidPasswordPolicy();

            _mockPasswordPolicyRepository
                .Setup(x => x.GetPasswordPolicy(partyId))
                .Returns(expectedPasswordPolicy);

            var managePasswordPolicy = new ManagePasswordPolicy(_mockPasswordPolicyRepository.Object);

            // Act
            var result = managePasswordPolicy.GetPasswordPolicy(partyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPasswordPolicy.PasswordPolicyId, result.PasswordPolicyId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(long.MinValue)]
        public void GetPasswordPolicy_WithInvalidPartyIds_ThrowsException(long partyId)
        {
            // Arrange
            var managePasswordPolicy = new ManagePasswordPolicy(_mockPasswordPolicyRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePasswordPolicy.GetPasswordPolicy(partyId));

            Assert.Equal("Missing Party Id.", exception.Message);
        }

        #endregion
    }
}
