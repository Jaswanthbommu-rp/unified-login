using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageCredential business logic xUnit tests.
    /// Migrated from RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.ManageCredentialTest
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageCredentialTests : TestBase
    {
        private readonly Mock<ICredentialRepository> _mockCredentialRepository;
        private readonly Mock<IPasswordPolicyRepository> _mockPasswordPolicyRepository;
        private readonly Mock<IUserLoginRepository> _mockUserLoginRepository;
        private readonly Mock<IManageUserLogin> _mockManageUserLogin;
        private readonly Mock<IManagePerson> _mockManagePerson;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly DefaultUserClaim _defaultUserClaim;

        public ManageCredentialTests()
        {
            _mockCredentialRepository = new Mock<ICredentialRepository>();
            _mockPasswordPolicyRepository = new Mock<IPasswordPolicyRepository>();
            _mockUserLoginRepository = new Mock<IUserLoginRepository>();
            _mockManageUserLogin = new Mock<IManageUserLogin>();
            _mockManagePerson = new Mock<IManagePerson>();
            _mockUserRepository = new Mock<IUserRepository>();

            _defaultUserClaim = new DefaultUserClaim
            {
                UserId = 1,
                LoginName = "testuser@test.com",
                FirstName = "Test",
                LastName = "User",
                OrganizationRealPageGuid = Guid.Parse("F5C090FA-78AB-452F-B504-98AAFEE09121"),
                OrganizationMasterId = 379,
                OrganizationPartyId = 100,
                CorrelationId = Guid.NewGuid(),
                UserRealPageGuid = Guid.NewGuid()
            };
        }

        private ManageCredential CreateManageCredential()
        {
            return new ManageCredential(
                _mockCredentialRepository.Object,
                _mockPasswordPolicyRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserLogin.Object,
                _mockManagePerson.Object,
                _mockUserRepository.Object,
                _defaultUserClaim);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithDependencies_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageCredential = CreateManageCredential();

            // Assert
            Assert.NotNull(manageCredential);
        }

        [Fact]
        public void Constructor_WithUserClaim_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageCredential = new ManageCredential(_defaultUserClaim);

            // Assert
            Assert.NotNull(manageCredential);
        }

        #endregion

        #region GetSecurityQuestion Tests

        [Fact]
        public void GetSecurityQuestion_WithEmptyUsername_ReturnsError()
        {
            // Arrange
            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.GetSecurityQuestion(string.Empty, null);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("No Username specified.", result.ErrorReason);
        }

        [Fact]
        public void GetSecurityQuestion_WithNullUsername_ReturnsError()
        {
            // Arrange
            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.GetSecurityQuestion(null, null);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("No Username specified.", result.ErrorReason);
        }

        [Fact]
        public void GetSecurityQuestion_WithNonExistentUser_ReturnsError()
        {
            // Arrange
            string username = "nonexistent@test.com";
            _mockManageUserLogin.Setup(x => x.GetUserLoginOnly(username)).Returns((UserLoginOnly)null);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.GetSecurityQuestion(username, null);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("incorrect or was not found", result.ErrorReason);
        }

        [Fact]
        public void GetSecurityQuestion_WithInactiveUser_ReturnsError()
        {
            // Arrange
            string username = "inactive@test.com";
            var userLogin = new UserLoginOnly
            {
                UserId = 1,
                LoginName = username,
                RealPageId = Guid.NewGuid(),
                Is3rdPartyIDP = false
            };

            var orgStatus = new OrganizationStatus
            {
                PartyId = 100,
                IsActive = false,
                IsExpired = false,
                IsPending = false,
                IsLocked = false
            };

            _mockManageUserLogin.Setup(x => x.GetUserLoginOnly(username)).Returns(userLogin);
            _mockUserLoginRepository.Setup(x => x.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, 0, true))
                .Returns(orgStatus);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.GetSecurityQuestion(username, null);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("The user is not active in the system.", result.ErrorReason);
        }

        [Fact]
        public void GetSecurityQuestion_WithExpiredUser_ReturnsError()
        {
            // Arrange
            string username = "expired@test.com";
            var userLogin = new UserLoginOnly
            {
                UserId = 1,
                LoginName = username,
                RealPageId = Guid.NewGuid(),
                Is3rdPartyIDP = false
            };

            var orgStatus = new OrganizationStatus
            {
                PartyId = 100,
                IsActive = true,
                IsExpired = true,
                IsPending = false,
                IsLocked = false
            };

            _mockManageUserLogin.Setup(x => x.GetUserLoginOnly(username)).Returns(userLogin);
            _mockUserLoginRepository.Setup(x => x.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, 0, true))
                .Returns(orgStatus);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.GetSecurityQuestion(username, null);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("The user is not active in the system.", result.ErrorReason);
        }

        [Fact]
        public void GetSecurityQuestion_WithLockedUser_ReturnsError()
        {
            // Arrange
            string username = "locked@test.com";
            var userLogin = new UserLoginOnly
            {
                UserId = 1,
                LoginName = username,
                RealPageId = Guid.NewGuid(),
                Is3rdPartyIDP = false
            };

            var orgStatus = new OrganizationStatus
            {
                PartyId = 100,
                IsActive = true,
                IsExpired = false,
                IsPending = false,
                IsLocked = true,
                StatusTypeId = (int)UserUiStatusType.Locked,
                StatusThruDate = DateTime.UtcNow.AddHours(1) // Still locked
            };

            _mockManageUserLogin.Setup(x => x.GetUserLoginOnly(username)).Returns(userLogin);
            _mockUserLoginRepository.Setup(x => x.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, 0, true))
                .Returns(orgStatus);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.GetSecurityQuestion(username, null);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("The user account is locked.", result.ErrorReason);
        }

        [Fact]
        public void GetSecurityQuestion_With3rdPartyIDP_ReturnsError()
        {
            // Arrange
            string username = "sso@test.com";
            var userLogin = new UserLoginOnly
            {
                UserId = 1,
                LoginName = username,
                RealPageId = Guid.NewGuid(),
                Is3rdPartyIDP = true
            };

            var orgStatus = new OrganizationStatus
            {
                PartyId = 100,
                IsActive = true,
                IsExpired = false,
                IsPending = false,
                IsLocked = false
            };

            _mockManageUserLogin.Setup(x => x.GetUserLoginOnly(username)).Returns(userLogin);
            _mockUserLoginRepository.Setup(x => x.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, 0, true))
                .Returns(orgStatus);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.GetSecurityQuestion(username, null);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("external identity provider", result.ErrorReason);
        }

        [Fact]
        public void GetSecurityQuestion_WithNoSecurityQuestions_ReturnsError()
        {
            // Arrange
            string username = "noquestions@test.com";
            var userLogin = new UserLoginOnly
            {
                UserId = 1,
                LoginName = username,
                RealPageId = Guid.NewGuid(),
                Is3rdPartyIDP = false
            };

            var orgStatus = new OrganizationStatus
            {
                PartyId = 100,
                IsActive = true,
                IsExpired = false,
                IsPending = false,
                IsLocked = false
            };

            _mockManageUserLogin.Setup(x => x.GetUserLoginOnly(username)).Returns(userLogin);
            _mockUserLoginRepository.Setup(x => x.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, 0, true))
                .Returns(orgStatus);
            _mockCredentialRepository.Setup(x => x.GetUserSecurityQuestion(username)).Returns(new List<SecurityQuestion>());

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.GetSecurityQuestion(username, null);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("User has no security questions defined.", result.ErrorReason);
        }

        [Fact]
        public void GetSecurityQuestion_WithValidUser_ReturnsSecurityQuestions()
        {
            // Arrange
            string username = "valid@test.com";
            var userRealPageId = Guid.NewGuid();
            var userLogin = new UserLoginOnly
            {
                UserId = 1,
                LoginName = username,
                RealPageId = userRealPageId,
                Is3rdPartyIDP = false
            };

            var orgStatus = new OrganizationStatus
            {
                PartyId = 100,
                IsActive = true,
                IsExpired = false,
                IsPending = false,
                IsLocked = false
            };

            var securityQuestions = new List<SecurityQuestion>
            {
                new SecurityQuestion { SecurityQuestionId = 1, Question = "What is your pet's name?" },
                new SecurityQuestion { SecurityQuestionId = 2, Question = "What city were you born in?" },
                new SecurityQuestion { SecurityQuestionId = 3, Question = "What is your mother's maiden name?" }
            };

            _mockManageUserLogin.Setup(x => x.GetUserLoginOnly(username)).Returns(userLogin);
            _mockUserLoginRepository.Setup(x => x.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, 0, true))
                .Returns(orgStatus);
            _mockCredentialRepository.Setup(x => x.GetUserSecurityQuestion(username)).Returns(securityQuestions);
            _mockCredentialRepository.Setup(x => x.CreateActivityToken(orgStatus.PartyId, userRealPageId, (int)ActivityType.ForgotPassword))
                .Returns("test-activity-token");

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.GetSecurityQuestion(username, null);

            // Assert
            Assert.False(result.IsError);
            Assert.NotNull(result.SecurityQuestions);
            Assert.Equal(2, result.SecurityQuestions.Count); // Random 2 questions
            Assert.Equal("test-activity-token", result.ActivityToken);
            Assert.Equal(username, result.EnterpriseUserName);
        }

        #endregion

        #region ValidatePassword Tests

        [Fact]
        public void ValidatePassword_WithPasswordContainingUsername_ReturnsError()
        {
            // Arrange
            var validatePassword = new ValidatePassword
            {
                EnterpriseUserName = "testuser@test.com",
                PasswordToValidate = "testuser@test.com123!",
                PartyId = 100,
                CheckPasswordHistory = false
            };

            var passwordPolicy = CreateDefaultPasswordPolicy();
            _mockPasswordPolicyRepository.Setup(x => x.GetPasswordPolicy(validatePassword.PartyId)).Returns(passwordPolicy);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.ValidatePassword(validatePassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("cannot be the same as your Username", result.ErrorReason);
        }

        [Fact]
        public void ValidatePassword_WithPasswordTooShort_ReturnsError()
        {
            // Arrange
            var validatePassword = new ValidatePassword
            {
                EnterpriseUserName = "testuser@test.com",
                PasswordToValidate = "Short1!",
                PartyId = 100,
                CheckPasswordHistory = false
            };

            var passwordPolicy = CreateDefaultPasswordPolicy();
            passwordPolicy.MinimumLength = 8;
            _mockPasswordPolicyRepository.Setup(x => x.GetPasswordPolicy(validatePassword.PartyId)).Returns(passwordPolicy);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.ValidatePassword(validatePassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("at least 8 characters", result.ErrorReason);
        }

        [Fact]
        public void ValidatePassword_WithPasswordTooLong_ReturnsError()
        {
            // Arrange
            var validatePassword = new ValidatePassword
            {
                EnterpriseUserName = "testuser@test.com",
                PasswordToValidate = new string('A', 150) + "1!a",
                PartyId = 100,
                CheckPasswordHistory = false
            };

            var passwordPolicy = CreateDefaultPasswordPolicy();
            passwordPolicy.MaximumLength = 128;
            _mockPasswordPolicyRepository.Setup(x => x.GetPasswordPolicy(validatePassword.PartyId)).Returns(passwordPolicy);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.ValidatePassword(validatePassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("128 characters or less", result.ErrorReason);
        }

        [Fact]
        public void ValidatePassword_WithNoUppercase_ReturnsError()
        {
            // Arrange
            var validatePassword = new ValidatePassword
            {
                EnterpriseUserName = "testuser@test.com",
                PasswordToValidate = "password123!",
                PartyId = 100,
                CheckPasswordHistory = false
            };

            var passwordPolicy = CreateDefaultPasswordPolicy();
            passwordPolicy.MinimumUppercase = 1;
            _mockPasswordPolicyRepository.Setup(x => x.GetPasswordPolicy(validatePassword.PartyId)).Returns(passwordPolicy);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.ValidatePassword(validatePassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("upper case", result.ErrorReason);
        }

        [Fact]
        public void ValidatePassword_WithNoNumeric_ReturnsError()
        {
            // Arrange
            var validatePassword = new ValidatePassword
            {
                EnterpriseUserName = "testuser@test.com",
                PasswordToValidate = "Password!",
                PartyId = 100,
                CheckPasswordHistory = false
            };

            var passwordPolicy = CreateDefaultPasswordPolicy();
            passwordPolicy.MinimumNumeric = 1;
            _mockPasswordPolicyRepository.Setup(x => x.GetPasswordPolicy(validatePassword.PartyId)).Returns(passwordPolicy);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.ValidatePassword(validatePassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("numeric", result.ErrorReason);
        }

        [Fact]
        public void ValidatePassword_WithNoSpecialCharacter_ReturnsError()
        {
            // Arrange
            var validatePassword = new ValidatePassword
            {
                EnterpriseUserName = "testuser@test.com",
                PasswordToValidate = "Password123",
                PartyId = 100,
                CheckPasswordHistory = false
            };

            var passwordPolicy = CreateDefaultPasswordPolicy();
            passwordPolicy.MinimumSpecialCharacter = 1;
            _mockPasswordPolicyRepository.Setup(x => x.GetPasswordPolicy(validatePassword.PartyId)).Returns(passwordPolicy);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.ValidatePassword(validatePassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("special characters", result.ErrorReason);
        }

        [Fact]
        public void ValidatePassword_WithValidPassword_ReturnsSuccess()
        {
            // Arrange
            var validatePassword = new ValidatePassword
            {
                EnterpriseUserName = "testuser@test.com",
                PasswordToValidate = "ValidPassword123!",
                PartyId = 100,
                CheckPasswordHistory = false
            };

            var passwordPolicy = CreateDefaultPasswordPolicy();
            _mockPasswordPolicyRepository.Setup(x => x.GetPasswordPolicy(validatePassword.PartyId)).Returns(passwordPolicy);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.ValidatePassword(validatePassword);

            // Assert
            Assert.False(result.IsError);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void ValidatePassword_WithNullPasswordPolicy_ReturnsError()
        {
            // Arrange
            var validatePassword = new ValidatePassword
            {
                EnterpriseUserName = "testuser@test.com",
                PasswordToValidate = "ValidPassword123!",
                PartyId = 100,
                CheckPasswordHistory = false
            };

            _mockPasswordPolicyRepository.Setup(x => x.GetPasswordPolicy(validatePassword.PartyId)).Returns((PasswordPolicy)null);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.ValidatePassword(validatePassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("Unable to find password policy", result.ErrorReason);
        }

        #endregion

        #region ResetPassword Tests

        [Fact]
        public void ResetPassword_WithNullUserResetPassword_ThrowsArgumentNullException()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var manageCredential = CreateManageCredential();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageCredential.ResetPassword(realPageId, null));

            Assert.Equal("userResetPassword", exception.ParamName);
        }

        [Fact]
        public void ResetPassword_WithEmptyRealPageId_ReturnsError()
        {
            // Arrange
            var userResetPassword = new UserResetPassword
            {
                OldPassword = "OldPassword123!",
                NewPassword = "NewPassword123!"
            };

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.ResetPassword(Guid.Empty, userResetPassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("RealPage Id for user not provided.", result.ErrorReason);
        }

        [Fact]
        public void ResetPassword_WithEmptyOldPassword_ReturnsError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var userResetPassword = new UserResetPassword
            {
                OldPassword = "",
                NewPassword = "NewPassword123!"
            };

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.ResetPassword(realPageId, userResetPassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Old Password is not specified.", result.ErrorReason);
        }

        [Fact]
        public void ResetPassword_WithEmptyNewPassword_ReturnsError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var userResetPassword = new UserResetPassword
            {
                OldPassword = "OldPassword123!",
                NewPassword = ""
            };

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.ResetPassword(realPageId, userResetPassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("New Password is not specified.", result.ErrorReason);
        }

        [Fact]
        public void ResetPassword_WithNonExistentUser_ReturnsError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var userResetPassword = new UserResetPassword
            {
                OldPassword = "OldPassword123!",
                NewPassword = "NewPassword123!"
            };

            _mockUserLoginRepository.Setup(x => x.GetUserLoginOnly(realPageId)).Returns((UserLoginOnly)null);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.ResetPassword(realPageId, userResetPassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("User Name is incorrect or not found.", result.ErrorReason);
        }

        #endregion

        #region SetTemporaryPassword Tests

        [Fact]
        public void SetTemporaryPassword_WithNullUserResetPassword_ThrowsArgumentNullException()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var manageCredential = CreateManageCredential();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                manageCredential.SetTemporaryPassword(realPageId, null));
        }

        [Fact]
        public void SetTemporaryPassword_WithEmptyRealPageId_ReturnsError()
        {
            // Arrange
            var userResetPassword = new UserResetPassword
            {
                NewPassword = "TempPassword123!"
            };

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.SetTemporaryPassword(Guid.Empty, userResetPassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Real Page Id for user not provided.", result.ErrorReason);
        }

        [Fact]
        public void SetTemporaryPassword_WithEmptyNewPassword_ReturnsError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var userResetPassword = new UserResetPassword
            {
                NewPassword = ""
            };

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.SetTemporaryPassword(realPageId, userResetPassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("New Password is not specified", result.ErrorReason);
        }

        [Fact]
        public void SetTemporaryPassword_WithNonExistentUser_ReturnsError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var userResetPassword = new UserResetPassword
            {
                NewPassword = "TempPassword123!"
            };

            _mockManageUserLogin.Setup(x => x.GetUserLoginOnly(realPageId)).Returns((UserLoginOnly)null);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.SetTemporaryPassword(realPageId, userResetPassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("User Name is incorrect or not found.", result.ErrorReason);
        }

        #endregion

        #region CheckPasswordExpiration Tests

        [Fact]
        public void CheckPasswordExpiration_WithEmptyRealPageId_ReturnsError()
        {
            // Arrange
            long userId = 1;
            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.CheckPasswordExpiration(userId, Guid.Empty);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("RealPageId for user not supplied.", result.ErrorReason);
        }

        [Fact]
        public void CheckPasswordExpiration_WithNoOrgId_ReturnsError()
        {
            // Arrange
            long userId = 1;
            var realPageId = Guid.NewGuid();

            _mockUserLoginRepository.Setup(x => x.GetPrimaryOrgIdByUserId(userId)).Returns(0);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.CheckPasswordExpiration(userId, realPageId);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Unable to get organization Id for user from claims.", result.ErrorReason);
        }

        [Fact]
        public void CheckPasswordExpiration_WithNoPasswordPolicy_ReturnsError()
        {
            // Arrange
            long userId = 1;
            long orgPartyId = 100;
            var realPageId = Guid.NewGuid();

            _mockUserLoginRepository.Setup(x => x.GetPrimaryOrgIdByUserId(userId)).Returns(orgPartyId);
            _mockPasswordPolicyRepository.Setup(x => x.GetPasswordPolicy(orgPartyId)).Returns((PasswordPolicy)null);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.CheckPasswordExpiration(userId, realPageId);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("Unable to find password policy", result.ErrorReason);
        }

        [Fact]
        public void CheckPasswordExpiration_With3rdPartyIDP_ReturnsNotApplicable()
        {
            // Arrange
            long userId = 1;
            long orgPartyId = 100;
            var realPageId = Guid.NewGuid();

            var passwordPolicy = CreateDefaultPasswordPolicy();
            passwordPolicy.EnablePasswordExpiration = true;

            var userLogin = new UserLoginOnly
            {
                UserId = userId,
                RealPageId = realPageId,
                Is3rdPartyIDP = true
            };

            _mockUserLoginRepository.Setup(x => x.GetPrimaryOrgIdByUserId(userId)).Returns(orgPartyId);
            _mockPasswordPolicyRepository.Setup(x => x.GetPasswordPolicy(orgPartyId)).Returns(passwordPolicy);
            _mockUserLoginRepository.Setup(x => x.GetUserLoginOnly(realPageId)).Returns(userLogin);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.CheckPasswordExpiration(userId, realPageId);

            // Assert
            Assert.False(result.IsError);
            Assert.Contains("not applicable to users on external identity provider", result.ErrorReason);
            Assert.False(result.IsPasswordExpired);
        }

        [Fact]
        public void CheckPasswordExpiration_WithExpirationDisabled_ReturnsNoExpiration()
        {
            // Arrange
            long userId = 1;
            long orgPartyId = 100;
            var realPageId = Guid.NewGuid();

            var passwordPolicy = CreateDefaultPasswordPolicy();
            passwordPolicy.EnablePasswordExpiration = false;

            _mockUserLoginRepository.Setup(x => x.GetPrimaryOrgIdByUserId(userId)).Returns(orgPartyId);
            _mockPasswordPolicyRepository.Setup(x => x.GetPasswordPolicy(orgPartyId)).Returns(passwordPolicy);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.CheckPasswordExpiration(userId, realPageId);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(SeverityLevelType.None, result.SeverityLevel);
        }

        #endregion

        #region UserAllSecurityQuestions Tests

        [Fact]
        public void UserAllSecurityQuestions_WithEmptyUsername_ReturnsError()
        {
            // Arrange
            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.UserAllSecurityQuestions(string.Empty);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("User Name is not specified.", result.ErrorReason);
        }

        [Fact]
        public void UserAllSecurityQuestions_WithNullUsername_ReturnsError()
        {
            // Arrange
            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.UserAllSecurityQuestions(null);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("User Name is not specified.", result.ErrorReason);
        }

        [Fact]
        public void UserAllSecurityQuestions_WithNoQuestions_ReturnsError()
        {
            // Arrange
            string username = "test@test.com";
            _mockCredentialRepository.Setup(x => x.GetAllSecurityQuestions(username)).Returns(new List<SecurityQuestion>());

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.UserAllSecurityQuestions(username);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Error while getting security questions.", result.ErrorReason);
        }

        [Fact]
        public void UserAllSecurityQuestions_WithValidUser_ReturnsQuestions()
        {
            // Arrange
            string username = "test@test.com";
            var questions = new List<SecurityQuestion>
            {
                new SecurityQuestion { SecurityQuestionId = 1, Question = "Question 1" },
                new SecurityQuestion { SecurityQuestionId = 2, Question = "Question 2" }
            };

            _mockCredentialRepository.Setup(x => x.GetAllSecurityQuestions(username)).Returns(questions);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.UserAllSecurityQuestions(username);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(username, result.EnterpriseUserName);
            Assert.Equal(2, result.SecurityQuestions.Count);
        }

        #endregion

        #region GetUser Tests

        [Fact]
        public void GetUser_WithEmptyUsername_ReturnsError()
        {
            // Arrange
            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.GetUser(string.Empty);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("User Name is not specified.", result.ErrorReason);
        }

        [Fact]
        public void GetUser_WithNullUsername_ReturnsError()
        {
            // Arrange
            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.GetUser(null);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("User Name is not specified.", result.ErrorReason);
        }

        #endregion

        #region GetUserSelectedSecurityQuestions Tests

        [Fact]
        public void GetUserSelectedSecurityQuestions_WithEmptyRealPageId_ReturnsError()
        {
            // Arrange
            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.GetUserSelectedSecurityQuestions(Guid.Empty);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("RealPageId for user not supplied.", result.ErrorReason);
        }

        [Fact]
        public void GetUserSelectedSecurityQuestions_WithNoQuestions_ReturnsError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            _mockCredentialRepository.Setup(x => x.GetUserSelectedSecurityQuestions(realPageId))
                .Returns(new List<SecurityQuestion>());

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.GetUserSelectedSecurityQuestions(realPageId);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("User has not set security question.", result.ErrorReason);
        }

        [Fact]
        public void GetUserSelectedSecurityQuestions_WithValidUser_ReturnsQuestions()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var questions = new List<SecurityQuestion>
            {
                new SecurityQuestion { SecurityQuestionId = 1, Question = "Question 1" },
                new SecurityQuestion { SecurityQuestionId = 2, Question = "Question 2" },
                new SecurityQuestion { SecurityQuestionId = 3, Question = "Question 3" }
            };

            _mockCredentialRepository.Setup(x => x.GetUserSelectedSecurityQuestions(realPageId)).Returns(questions);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.GetUserSelectedSecurityQuestions(realPageId);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(3, result.SecurityQuestions.Count);
        }

        #endregion

        #region SaveUserSelectedSecurityQuestions Tests

        [Fact]
        public void SaveUserSelectedSecurityQuestions_WithEmptyRealPageId_ReturnsError()
        {
            // Arrange
            var questions = new List<SecurityQuestionAnswer>();
            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.SaveUserSelectedSecurityQuestions(Guid.Empty, questions);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("RealPageId for user not supplied.", result.ErrorReason);
        }

        [Fact]
        public void SaveUserSelectedSecurityQuestions_WithNonExistentUser_ReturnsError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var questions = new List<SecurityQuestionAnswer>
            {
                new SecurityQuestionAnswer { SecurityQuestionId = 1, Answer = "Answer1" },
                new SecurityQuestionAnswer { SecurityQuestionId = 2, Answer = "Answer2" },
                new SecurityQuestionAnswer { SecurityQuestionId = 3, Answer = "Answer3" }
            };

            _mockManageUserLogin.Setup(x => x.GetUserLoginOnly(realPageId)).Returns((UserLoginOnly)null);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.SaveUserSelectedSecurityQuestions(realPageId, questions);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("User Name is incorrect or not found.", result.ErrorReason);
        }

        [Fact]
        public void SaveUserSelectedSecurityQuestions_WithEmptyQuestions_ReturnsError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var userLogin = new UserLoginOnly
            {
                UserId = 1,
                LoginName = "test@test.com",
                RealPageId = realPageId
            };

            _mockManageUserLogin.Setup(x => x.GetUserLoginOnly(realPageId)).Returns(userLogin);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.SaveUserSelectedSecurityQuestions(realPageId, new List<SecurityQuestionAnswer>());

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("No questions received from user.", result.ErrorReason);
        }

        [Fact]
        public void SaveUserSelectedSecurityQuestions_WithIncorrectNumberOfQuestions_ReturnsError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var userLogin = new UserLoginOnly
            {
                UserId = 1,
                LoginName = "test@test.com",
                RealPageId = realPageId
            };

            var questions = new List<SecurityQuestionAnswer>
            {
                new SecurityQuestionAnswer { SecurityQuestionId = 1, Answer = "Answer1" },
                new SecurityQuestionAnswer { SecurityQuestionId = 2, Answer = "Answer2" }
            };

            _mockManageUserLogin.Setup(x => x.GetUserLoginOnly(realPageId)).Returns(userLogin);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.SaveUserSelectedSecurityQuestions(realPageId, questions);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Incorrect number of questions received from user.", result.ErrorReason);
        }

        [Fact]
        public void SaveUserSelectedSecurityQuestions_WithAnswerTooLong_ReturnsError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var userLogin = new UserLoginOnly
            {
                UserId = 1,
                LoginName = "test@test.com",
                RealPageId = realPageId
            };

            var questions = new List<SecurityQuestionAnswer>
            {
                new SecurityQuestionAnswer { SecurityQuestionId = 1, Answer = new string('A', 51) },
                new SecurityQuestionAnswer { SecurityQuestionId = 2, Answer = "Answer2" },
                new SecurityQuestionAnswer { SecurityQuestionId = 3, Answer = "Answer3" }
            };

            _mockManageUserLogin.Setup(x => x.GetUserLoginOnly(realPageId)).Returns(userLogin);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.SaveUserSelectedSecurityQuestions(realPageId, questions);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Answer should be less than 50 chars.", result.ErrorReason);
        }

        #endregion

        #region GetIdentityProviderTypeByLoginName Tests

        [Fact]
        public void GetIdentityProviderTypeByLoginName_CallsRepository()
        {
            // Arrange
            string loginName = "test@test.com";
            var expectedProvider = new IdentityProviderType
            {
                AuthenticationType = "local"
            };

            _mockCredentialRepository.Setup(x => x.GetIdentityProviderTypeByLoginName(loginName)).Returns(expectedProvider);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.GetIdentityProviderTypeByLoginName(loginName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsLocal);
            _mockCredentialRepository.Verify(x => x.GetIdentityProviderTypeByLoginName(loginName), Times.Once);
        }

        #endregion

        #region GetNewUserRegistrationVerificationToken Tests

        [Fact]
        public void GetNewUserRegistrationVerificationToken_ReturnsToken()
        {
            // Arrange
            long userId = 1;
            long orgPartyId = 100;
            var realPageId = Guid.NewGuid();
            string expectedToken = "verification-token-12345";

            _mockUserLoginRepository.Setup(x => x.GetPrimaryOrgIdByUserId(userId)).Returns(orgPartyId);
            _mockCredentialRepository.Setup(x => x.CreateActivityToken(orgPartyId, realPageId, (int)ActivityType.NewUserRegistrationVerification))
                .Returns(expectedToken);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.GetNewUserRegistrationVerificationToken(userId, realPageId);

            // Assert
            Assert.Equal(expectedToken, result);
            _mockCredentialRepository.Verify(
                x => x.CreateActivityToken(orgPartyId, realPageId, (int)ActivityType.NewUserRegistrationVerification),
                Times.Once);
        }

        #endregion

        #region ForgotPassword Tests

        [Fact]
        public void ForgotPassword_WithEmptyUsername_ReturnsError()
        {
            // Arrange
            var changePassword = new ChangePassword
            {
                EnterpriseUserName = "",
                ActivityToken = "token",
                CorrectAnswerToken = "answertoken",
                NewPassword = "NewPassword123!"
            };

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.ForgotPassword(changePassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("No Username specified.", result.ErrorReason);
        }

        [Fact]
        public void ForgotPassword_WithEmptyActivityToken_ReturnsError()
        {
            // Arrange
            var changePassword = new ChangePassword
            {
                EnterpriseUserName = "test@test.com",
                ActivityToken = "",
                CorrectAnswerToken = "answertoken",
                NewPassword = "NewPassword123!"
            };

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.ForgotPassword(changePassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Forgot Password Activity Token is not specified.", result.ErrorReason);
        }

        [Fact]
        public void ForgotPassword_WithEmptyCorrectAnswerToken_ReturnsError()
        {
            // Arrange
            var changePassword = new ChangePassword
            {
                EnterpriseUserName = "test@test.com",
                ActivityToken = "token",
                CorrectAnswerToken = "",
                NewPassword = "NewPassword123!"
            };

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.ForgotPassword(changePassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Correct Answer Token is not specified.", result.ErrorReason);
        }

        [Fact]
        public void ForgotPassword_WithEmptyNewPassword_ReturnsError()
        {
            // Arrange
            var changePassword = new ChangePassword
            {
                EnterpriseUserName = "test@test.com",
                ActivityToken = "token",
                CorrectAnswerToken = "answertoken",
                NewPassword = ""
            };

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.ForgotPassword(changePassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("New Password is not specified.", result.ErrorReason);
        }

        [Fact]
        public void ForgotPassword_WithNonExistentUser_ReturnsError()
        {
            // Arrange
            var changePassword = new ChangePassword
            {
                EnterpriseUserName = "nonexistent@test.com",
                ActivityToken = "token",
                CorrectAnswerToken = "answertoken",
                NewPassword = "NewPassword123!"
            };

            _mockUserLoginRepository.Setup(x => x.GetUserLoginOnly(changePassword.EnterpriseUserName))
                .Returns((UserLoginOnly)null);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.ForgotPassword(changePassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("User name is incorrect or not found.", result.ErrorReason);
        }

        #endregion

        #region SetPassword Tests

        [Fact]
        public void SetPassword_WithEmptyUsername_ReturnsError()
        {
            // Arrange
            var setPassword = new SetPassword
            {
                EnterpriseUserName = "",
                ActivityToken = "token",
                NewPassword = "NewPassword123!"
            };

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.SetPassword(setPassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("No Username specified.", result.ErrorReason);
        }

        [Fact]
        public void SetPassword_WithEmptyActivityToken_ReturnsError()
        {
            // Arrange
            var setPassword = new SetPassword
            {
                EnterpriseUserName = "test@test.com",
                ActivityToken = "",
                NewPassword = "NewPassword123!"
            };

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.SetPassword(setPassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Set Password Activity Token is not specified.", result.ErrorReason);
        }

        [Fact]
        public void SetPassword_WithEmptyNewPassword_ReturnsError()
        {
            // Arrange
            var setPassword = new SetPassword
            {
                EnterpriseUserName = "test@test.com",
                ActivityToken = "token",
                NewPassword = ""
            };

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.SetPassword(setPassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("New Password is not specified.", result.ErrorReason);
        }

        [Fact]
        public void SetPassword_WithNonExistentUser_ReturnsError()
        {
            // Arrange
            var setPassword = new SetPassword
            {
                EnterpriseUserName = "nonexistent@test.com",
                ActivityToken = "token",
                NewPassword = "NewPassword123!"
            };

            _mockUserLoginRepository.Setup(x => x.GetUserLoginOnly(setPassword.EnterpriseUserName))
                .Returns((UserLoginOnly)null);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.SetPassword(setPassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Username is incorrect or not found.", result.ErrorReason);
        }

        [Fact]
        public void SetPassword_With3rdPartyIDP_ReturnsError()
        {
            // Arrange
            var setPassword = new SetPassword
            {
                EnterpriseUserName = "sso@test.com",
                ActivityToken = "token",
                NewPassword = "NewPassword123!"
            };

            var userLogin = new UserLoginOnly
            {
                UserId = 1,
                LoginName = setPassword.EnterpriseUserName,
                RealPageId = Guid.NewGuid(),
                Is3rdPartyIDP = true
            };

            _mockUserLoginRepository.Setup(x => x.GetUserLoginOnly(setPassword.EnterpriseUserName)).Returns(userLogin);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.SetPassword(setPassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("not applicable to users on external identity provider", result.ErrorReason);
        }

        #endregion

        #region SetUserSecurityQuestions Tests

        [Fact]
        public void SetUserSecurityQuestions_WithEmptyUsername_ReturnsError()
        {
            // Arrange
            var userSecurityAnswer = new UserSecurityAnswer
            {
                EnterpriseUserName = "",
                ActivityToken = "token",
                SecurityQuestionAnswers = new List<SecurityQuestionAnswer>()
            };

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.SetUserSecurityQuestions(userSecurityAnswer);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("No Username specified.", result.ErrorReason);
        }

        [Fact]
        public void SetUserSecurityQuestions_WithExternalIDP_ReturnsError()
        {
            // Arrange
            var userSecurityAnswer = new UserSecurityAnswer
            {
                EnterpriseUserName = "sso@test.com",
                ActivityToken = "token",
                SecurityQuestionAnswers = new List<SecurityQuestionAnswer>()
            };

            var idpType = new IdentityProviderType { AuthenticationType = "okta" };
            _mockCredentialRepository.Setup(x => x.GetIdentityProviderTypeByLoginName(userSecurityAnswer.EnterpriseUserName))
                .Returns(idpType);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.SetUserSecurityQuestions(userSecurityAnswer);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("not applicable to users on external identity provider", result.ErrorReason);
        }

        [Fact]
        public void SetUserSecurityQuestions_WithNoQuestions_ReturnsError()
        {
            // Arrange
            var userSecurityAnswer = new UserSecurityAnswer
            {
                EnterpriseUserName = "test@test.com",
                ActivityToken = "token",
                SecurityQuestionAnswers = new List<SecurityQuestionAnswer>()
            };

            var idpType = new IdentityProviderType { AuthenticationType = "local" };
            _mockCredentialRepository.Setup(x => x.GetIdentityProviderTypeByLoginName(userSecurityAnswer.EnterpriseUserName))
                .Returns(idpType);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.SetUserSecurityQuestions(userSecurityAnswer);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("No questions received from user.", result.ErrorReason);
        }

        [Fact]
        public void SetUserSecurityQuestions_WithIncorrectNumberOfQuestions_ReturnsError()
        {
            // Arrange
            var userSecurityAnswer = new UserSecurityAnswer
            {
                EnterpriseUserName = "test@test.com",
                ActivityToken = "token",
                SecurityQuestionAnswers = new List<SecurityQuestionAnswer>
                {
                    new SecurityQuestionAnswer { SecurityQuestionId = 1, Answer = "Answer1" },
                    new SecurityQuestionAnswer { SecurityQuestionId = 2, Answer = "Answer2" }
                }
            };

            var idpType = new IdentityProviderType { AuthenticationType = "local" };
            _mockCredentialRepository.Setup(x => x.GetIdentityProviderTypeByLoginName(userSecurityAnswer.EnterpriseUserName))
                .Returns(idpType);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.SetUserSecurityQuestions(userSecurityAnswer);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Incorrect number of questions received from user, 3 questions are required.", result.ErrorReason);
        }

        [Fact]
        public void SetUserSecurityQuestions_WithEmptyActivityToken_ReturnsError()
        {
            // Arrange
            var userSecurityAnswer = new UserSecurityAnswer
            {
                EnterpriseUserName = "test@test.com",
                ActivityToken = "",
                SecurityQuestionAnswers = new List<SecurityQuestionAnswer>
                {
                    new SecurityQuestionAnswer { SecurityQuestionId = 1, Answer = "Answer1" },
                    new SecurityQuestionAnswer { SecurityQuestionId = 2, Answer = "Answer2" },
                    new SecurityQuestionAnswer { SecurityQuestionId = 3, Answer = "Answer3" }
                }
            };

            var idpType = new IdentityProviderType { AuthenticationType = "local" };
            _mockCredentialRepository.Setup(x => x.GetIdentityProviderTypeByLoginName(userSecurityAnswer.EnterpriseUserName))
                .Returns(idpType);

            var manageCredential = CreateManageCredential();

            // Act
            var result = manageCredential.SetUserSecurityQuestions(userSecurityAnswer);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Null or empty activity Token.", result.ErrorReason);
        }

        #endregion

        #region Helper Methods

        private PasswordPolicy CreateDefaultPasswordPolicy()
        {
            return new PasswordPolicy
            {
                PasswordPolicyId = 1,
                PartyId = 100,
                Name = "Default",
                MinimumLength = 8,
                MaximumLength = 128,
                MinimumLowercase = 1,
                MinimumUppercase = 1,
                MinimumNumeric = 1,
                MinimumSpecialCharacter = 1,
                EnablePasswordExpiration = false,
                PasswordExpirationPeriodInDays = 90,
                PreventPasswordReuse = false,
                NumberOfPasswordsToRemember = 5
            };
        }

        #endregion
    }
}
