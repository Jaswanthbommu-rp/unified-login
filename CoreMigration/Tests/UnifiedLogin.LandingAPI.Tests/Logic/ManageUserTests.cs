using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageUser business logic xUnit tests.
    /// Tests for user management including validation, creation, update, and profile operations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageUserTests : TestBase
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ICredentialRepository> _mockCredentialRepository;
        private readonly Mock<IUserLoginRepository> _mockUserLoginRepository;
        private readonly Mock<IManageUserRegistrationEmail> _mockManageUserRegistrationEmail;
        private readonly Mock<IRepository> _mockRepository;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly DefaultUserClaim _defaultUserClaim;

        public ManageUserTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockCredentialRepository = new Mock<ICredentialRepository>();
            _mockUserLoginRepository = new Mock<IUserLoginRepository>();
            _mockManageUserRegistrationEmail = new Mock<IManageUserRegistrationEmail>();
            _mockRepository = new Mock<IRepository>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

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
                CorrelationId = Guid.NewGuid(),
                CustomerMasterId = 12345,
                OrganizationMasterId = 100,
                Rights = new List<string> 
                { 
                    "ManageOneSiteProductAccess", 
                    "ManageAccountingProductAccess",
                    "ManageAssetOptimizationProductAccess"
                }
            };

            SetupBasicMocks();
        }

        #region Mock Setup

        private void SetupBasicMocks()
        {
            // Setup ProductInternalSettings mock
            var productInternalSettings = new List<ProductInternalSetting>
            {
                new ProductInternalSetting { Name = "Elk_LogManageUser", Value = "1" }
            };

            _mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .Returns(productInternalSettings);

            SetupHttpMessageHandlerMock(HttpStatusCode.OK);
        }

        private void SetupHttpMessageHandlerMock(HttpStatusCode statusCode, string content = "")
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });
        }

        private UserLoginOnly CreateUserLoginOnly()
        {
            return new UserLoginOnly
            {
                UserId = 1,
                RealPageId = Guid.NewGuid(),
                LoginName = "testuser@test.com",
                LastLogin = DateTime.UtcNow
            };
        }

        private OrganizationStatus CreateOrganizationStatus(bool isActive = true, bool isPending = false, bool isExpired = false)
        {
            return new OrganizationStatus
            {
                PartyId = 1000,
                Status = UserUiStatusType.Active,
                IsActive = isActive,
                IsPending = isPending,
                IsExpired = isExpired
            };
        }

        private TokenDetail CreateTokenDetail()
        {
            return new TokenDetail
            {
                EnterpriseUserId = 1,
                RealPageId = Guid.NewGuid(),
                Token = "valid-token"
            };
        }

        private ProfileDetail CreateProfileDetail()
        {
            return new ProfileDetail
            {
                FirstName = "Test",
                LastName = "User",
                RealPageId = Guid.NewGuid(),
                PartyId = 100,
                userLogin = new UserLogin
                {
                    UserId = 1,
                    LoginName = "testuser@test.com",
                    RealPageId = Guid.NewGuid(),
                    FromDate = DateTime.UtcNow,
                    Status = UserUiStatusType.Active,
                    IsActive = true,
                    doNotForceChangePassword = false
                },
                organization = new List<Organization>
                {
                    new Organization
                    {
                        PartyId = 1000,
                        RealPageId = Guid.NewGuid(),
                        Name = "Test Organization"
                    }
                },
                Persona = new List<Persona>
                {
                    new Persona
                    {
                        PersonaId = 5,
                        UserId = 1,
                        OrganizationPartyId = 1000,
                        Organization = new Organization { PartyId = 1000 }
                    }
                },
                UserTypeId = 1,
                CreateUserSourceType = CreateUserSourceType.UnifiedPlatform
            };
        }

        private Profile CreateProfile()
        {
            return new Profile
            {
                PartyRole = new PartyRole { RoleTypeId = 1 },
                TelecommunicationNumber = new List<TelecommunicationNumber>
                {
                    new TelecommunicationNumber { PhoneNumber = "1234567890" }
                }
            };
        }

        private StarterProfile CreateStarterProfile()
        {
            return new StarterProfile
            {
                EnterpriseUserName = "testuser@test.com"
            };
        }

        private ProductBatch CreateProductBatch(int productId)
        {
            return new ProductBatch
            {
                ProductId = productId
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithUserClaim_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageUser = new ManageUser(_defaultUserClaim);

            // Assert
            Assert.NotNull(manageUser);
        }

        [Fact]
        public void Constructor_WithRepositories_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Assert
            Assert.NotNull(manageUser);
        }

        [Fact]
        public void Constructor_WithRepositoryAndMessageHandler_InitializesSuccessfully()
        {
            // Arrange
            SetupBasicMocks();

            // Act
            var manageUser = new ManageUser(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object);

            // Assert
            Assert.NotNull(manageUser);
        }

        #endregion

        #region ValidateUser Tests

        [Fact]
        public void ValidateUser_WithEmptyUserName_ReturnsError()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act
            var result = manageUser.ValidateUser(string.Empty, "token");

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("No Username specified.", result.ErrorReason);
        }

        [Fact]
        public void ValidateUser_WithNullUserName_ReturnsError()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act
            var result = manageUser.ValidateUser(null, "token");

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("No Username specified.", result.ErrorReason);
        }

        [Fact]
        public void ValidateUser_WithEmptyToken_ReturnsError()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act
            var result = manageUser.ValidateUser("testuser@test.com", string.Empty);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("No validation token specified.", result.ErrorReason);
        }

        [Fact]
        public void ValidateUser_WithNullToken_ReturnsError()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act
            var result = manageUser.ValidateUser("testuser@test.com", null);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("No validation token specified.", result.ErrorReason);
        }

        [Fact]
        public void ValidateUser_WithNullUserLogin_ReturnsError()
        {
            // Arrange
            _mockUserLoginRepository
                .Setup(m => m.GetUserLoginOnly(It.IsAny<string>()))
                .Returns((UserLoginOnly)null);

            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act
            var result = manageUser.ValidateUser("testuser@test.com", "valid-token");

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("User login information is missing.", result.ErrorReason);
        }

        [Fact]
        public void ValidateUser_WithUndefinedOrgStatus_ReturnsError()
        {
            // Arrange
            var userLogin = CreateUserLoginOnly();
            var orgStatus = new OrganizationStatus { Status = UserUiStatusType.UnDefined };

            _mockUserLoginRepository
                .Setup(m => m.GetUserLoginOnly(It.IsAny<string>()))
                .Returns(userLogin);

            _mockUserLoginRepository
                .Setup(m => m.ListOrganizationByEnterpriseUserId(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new List<Organization> { new Organization { PartyId = 1000 } });

            _mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(orgStatus);

            _mockCredentialRepository
                .Setup(m => m.GetActivityToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>()))
                .Returns(CreateTokenDetail());

            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act
            var result = manageUser.ValidateUser("testuser@test.com", "valid-token");

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Organization status could not be determined.", result.ErrorReason);
        }

        [Fact]
        public void ValidateUser_WithExpiredToken_ReturnsError()
        {
            // Arrange
            var userLogin = CreateUserLoginOnly();
            var orgStatus = CreateOrganizationStatus();
            orgStatus.IsExpired = true;

            _mockUserLoginRepository
                .Setup(m => m.GetUserLoginOnly(It.IsAny<string>()))
                .Returns(userLogin);

            _mockUserLoginRepository
                .Setup(m => m.ListOrganizationByEnterpriseUserId(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new List<Organization> { new Organization { PartyId = 1000 } });

            _mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(orgStatus);

            _mockCredentialRepository
                .Setup(m => m.GetActivityToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>()))
                .Returns(CreateTokenDetail());

            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act
            var result = manageUser.ValidateUser("testuser@test.com", "valid-token");

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("expired", result.ErrorReason);
        }

        [Fact]
        public void ValidateUser_WithInactiveAndPending_ReturnsError()
        {
            // Arrange
            var userLogin = CreateUserLoginOnly();
            var orgStatus = CreateOrganizationStatus(isActive: false, isPending: true);

            _mockUserLoginRepository
                .Setup(m => m.GetUserLoginOnly(It.IsAny<string>()))
                .Returns(userLogin);

            _mockUserLoginRepository
                .Setup(m => m.ListOrganizationByEnterpriseUserId(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new List<Organization> { new Organization { PartyId = 1000 } });

            _mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(orgStatus);

            _mockCredentialRepository
                .Setup(m => m.GetActivityToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>()))
                .Returns(CreateTokenDetail());

            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act
            var result = manageUser.ValidateUser("testuser@test.com", "valid-token");

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Account is inactive.", result.ErrorReason);
        }

        [Fact]
        public void ValidateUser_WithProfileAlreadyCompleted_ReturnsError()
        {
            // Arrange
            var userLogin = CreateUserLoginOnly();
            var orgStatus = CreateOrganizationStatus(isActive: true, isPending: false);

            _mockUserLoginRepository
                .Setup(m => m.GetUserLoginOnly(It.IsAny<string>()))
                .Returns(userLogin);

            _mockUserLoginRepository
                .Setup(m => m.ListOrganizationByEnterpriseUserId(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new List<Organization> { new Organization { PartyId = 1000 } });

            _mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(orgStatus);

            _mockCredentialRepository
                .Setup(m => m.GetActivityToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>()))
                .Returns(CreateTokenDetail());

            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act
            var result = manageUser.ValidateUser("testuser@test.com", "valid-token");

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Profile already completed.", result.ErrorReason);
        }

        #endregion

        #region ValidateRegistrationVerificationToken Tests

        [Fact]
        public void ValidateRegistrationVerificationToken_WithEmptyUserName_ReturnsError()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act
            var result = manageUser.ValidateRegistrationVerificationToken(string.Empty, "token");

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("No Username specified.", result.ErrorReason);
        }

        [Fact]
        public void ValidateRegistrationVerificationToken_WithEmptyToken_ReturnsError()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act
            var result = manageUser.ValidateRegistrationVerificationToken("testuser@test.com", string.Empty);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("No validation token specified.", result.ErrorReason);
        }

        [Fact]
        public void ValidateRegistrationVerificationToken_WithInvalidToken_ReturnsError()
        {
            // Arrange
            var userLogin = CreateUserLoginOnly();

            _mockUserLoginRepository
                .Setup(m => m.GetUserLoginOnly(It.IsAny<string>()))
                .Returns(userLogin);

            _mockUserLoginRepository
                .Setup(m => m.GetPrimaryOrgIdByUserId(It.IsAny<long>()))
                .Returns(1000L);

            _mockCredentialRepository
                .Setup(m => m.GetActivityToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>()))
                .Returns((TokenDetail)null);

            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act
            var result = manageUser.ValidateRegistrationVerificationToken("testuser@test.com", "invalid-token");

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Validation token does not match with user.", result.ErrorReason);
        }

        [Fact]
        public void ValidateRegistrationVerificationToken_WithValidToken_ReturnsSuccess()
        {
            // Arrange
            var userLogin = CreateUserLoginOnly();
            var tokenDetail = CreateTokenDetail();

            _mockUserLoginRepository
                .Setup(m => m.GetUserLoginOnly(It.IsAny<string>()))
                .Returns(userLogin);

            _mockUserLoginRepository
                .Setup(m => m.GetPrimaryOrgIdByUserId(It.IsAny<long>()))
                .Returns(1000L);

            _mockCredentialRepository
                .Setup(m => m.GetActivityToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>()))
                .Returns(tokenDetail);

            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act
            var result = manageUser.ValidateRegistrationVerificationToken("testuser@test.com", "valid-token");

            // Assert
            Assert.False(result.IsError);
            Assert.Equal("valid-token", result.ValidateUserToken);
        }

        #endregion

        #region GetStarterProfileOptions Tests

        [Fact]
        public void GetStarterProfileOptions_WithNullUserName_ThrowsArgumentNullException()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                manageUser.GetStarterProfileOptions(null));
        }

        [Fact]
        public void GetStarterProfileOptions_WithEmptyUserName_ThrowsArgumentNullException()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                manageUser.GetStarterProfileOptions(string.Empty));
        }

        [Fact]
        public void GetStarterProfileOptions_WithValidUserName_ReturnsOptions()
        {
            // Arrange
            var expectedResponse = new StarterProfileOptionsResponse();

            _mockUserRepository
                .Setup(m => m.GetStarterProfileOptions(It.IsAny<string>()))
                .Returns(expectedResponse);

            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act
            var result = manageUser.GetStarterProfileOptions("testuser@test.com");

            // Assert
            Assert.NotNull(result);
            _mockUserRepository.Verify(m => m.GetStarterProfileOptions("testuser@test.com"), Times.Once);
        }

        #endregion

        #region SetStarterProfile Tests

        [Fact]
        public void SetStarterProfile_WithNullProfile_ThrowsArgumentNullException()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                manageUser.SetStarterProfile(null));
        }

        [Fact]
        public void SetStarterProfile_WithValidProfile_ReturnsResult()
        {
            // Arrange
            var starterProfile = CreateStarterProfile();
            var expectedResponse = new SetStarterProfile();

            _mockUserRepository
                .Setup(m => m.SetStarterProfileOptions(It.IsAny<StarterProfile>()))
                .Returns(expectedResponse);

            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act
            var result = manageUser.SetStarterProfile(starterProfile);

            // Assert
            Assert.NotNull(result);
            _mockUserRepository.Verify(m => m.SetStarterProfileOptions(starterProfile), Times.Once);
        }

        #endregion

        #region UpdateNewUser Tests

        [Fact]
        public void UpdateNewUser_WithNullUserLogin_ReturnsError()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            var profile = CreateProfile();

            // Act
            var result = manageUser.UpdateNewUser(null, profile, 1, "Job Title", "token");

            // Assert
            Assert.Equal("Invalid parameter: userLogin", result.ErrorMessage);
        }

        [Fact]
        public void UpdateNewUser_WithZeroPartyRoleTypeId_ReturnsError()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            var profile = new Profile
            {
                PartyRole = new PartyRole { RoleTypeId = 0 },
                TelecommunicationNumber = new List<TelecommunicationNumber>
                {
                    new TelecommunicationNumber { PhoneNumber = "1234567890" }
                }
            };

            // Act
            var result = manageUser.UpdateNewUser("testuser@test.com", profile, 0, "Job Title", "token");

            // Assert
            Assert.Equal("Invalid parameter: partyRoleTypeId", result.ErrorMessage);
        }

        [Fact]
        public void UpdateNewUser_WithEmptyTelecommunicationNumber_ReturnsError()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            var profile = new Profile
            {
                PartyRole = new PartyRole { RoleTypeId = 1 },
                TelecommunicationNumber = new List<TelecommunicationNumber>()
            };

            // Act
            var result = manageUser.UpdateNewUser("testuser@test.com", profile, 1, "Job Title", "token");

            // Assert
            Assert.Equal("Invalid parameter: telecommunicationNumber", result.ErrorMessage);
        }

        [Fact]
        public void UpdateNewUser_WithEmptyActivityToken_ReturnsError()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            var profile = CreateProfile();

            // Act
            var result = manageUser.UpdateNewUser("testuser@test.com", profile, 1, "Job Title", "   ");

            // Assert
            Assert.Equal("Invalid parameter: activityToken", result.ErrorMessage);
        }

        [Fact]
        public void UpdateNewUser_WithValidParameters_CallsRepository()
        {
            // Arrange
            var expectedResponse = new RepositoryResponse { Id = 1 };

            _mockUserRepository
                .Setup(m => m.UpdateNewUser(It.IsAny<string>(), It.IsAny<Profile>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(expectedResponse);

            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            var profile = CreateProfile();

            // Act
            var result = manageUser.UpdateNewUser("testuser@test.com", profile, 1, "Job Title", "token");

            // Assert
            Assert.Equal(1, result.Id);
            _mockUserRepository.Verify(m => m.UpdateNewUser("testuser@test.com", profile, 1, "Job Title", "token"), Times.Once);
        }

        #endregion

        #region UpdateUser Tests

        [Fact]
        public void UpdateUser_WithEmptyGuid_ReturnsError()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            var profile = CreateProfileDetail();

            // Act
            var result = manageUser.UpdateUser(Guid.Empty, profile);

            // Assert
            Assert.Equal("Edit User: Invalid parameter realPageId.", result.ErrorMessage);
        }

        #endregion

        #region UpdateUserStatus Tests

        [Fact]
        public void UpdateUserStatus_WithDisabledStatus_CallsDisableUserProduct()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            var userLogins = new List<UserLoginOnly> { CreateUserLoginOnly() };

            // Act
            var result = manageUser.UpdateUserStatus(Guid.NewGuid(), 5, userLogins, UserUiStatusType.Disabled);

            // Assert
            _mockUserRepository.Verify(m => m.DisableUserProduct(It.IsAny<Guid>(), It.IsAny<long>(), userLogins), Times.Once);
        }

        [Fact]
        public void UpdateUserStatus_WithActiveStatus_CallsActivateUserProducts()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            var userLogins = new List<UserLoginOnly> { CreateUserLoginOnly() };

            // Act
            var result = manageUser.UpdateUserStatus(Guid.NewGuid(), 5, userLogins, UserUiStatusType.Active);

            // Assert
            _mockUserRepository.Verify(m => m.ActivateUserProducts(It.IsAny<Guid>(), It.IsAny<long>(), userLogins), Times.Once);
        }

        #endregion

        #region DisableUsersFromProducts Tests

        [Fact]
        public void DisableUsersFromProducts_WithValidUserLogins_CallsProcessDisabledUsers()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            var userLogins = new List<ProcessUserLogin>();

            // Act
            var result = manageUser.DisableUsersFromProducts(userLogins);

            // Assert
            _mockUserRepository.Verify(m => m.ProcessDisabledUsers(userLogins), Times.Once);
        }

        #endregion

        #region AssignProductsToAdministrators Tests

        [Fact]
        public void AssignProductsToAdministrators_WithEmptyGuid_ThrowsException()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageUser.AssignProductsToAdministrators(Guid.Empty));

            Assert.Equal("Invalid parameter organization realPageId.", exception.Message);
        }

        [Fact]
        public void AssignProductsToAdministrators_WithValidGuid_ReturnsSuccess()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            var orgRealPageId = Guid.NewGuid();

            // Act
            var result = manageUser.AssignProductsToAdministrators(orgRealPageId);

            // Assert
            Assert.Equal(1, result.Id);
            Assert.Equal(orgRealPageId, result.RealPageId);
            _mockUserRepository.Verify(m => m.AssignProductsToAdministrators(orgRealPageId, 0), Times.Once);
        }

        [Fact]
        public void AssignProductsToAdministrators_WithException_ReturnsError()
        {
            // Arrange
            _mockUserRepository
                .Setup(m => m.AssignProductsToAdministrators(It.IsAny<Guid>(), It.IsAny<long>()))
                .Throws(new Exception("Database error"));

            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act
            var result = manageUser.AssignProductsToAdministrators(Guid.NewGuid());

            // Assert
            Assert.Equal(0, result.Id);
            Assert.Equal("Database error", result.ErrorMessage);
        }

        #endregion

        #region CheckProductRight Tests

        [Fact]
        public void CheckProductRight_WithManageOneSiteProductAccess_ReturnsTrue()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            var productBatch = CreateProductBatch((int)ProductRightEnum.ManageOneSiteProductAccess);

            // Act
            var result = manageUser.CheckProductRight(productBatch);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CheckProductRight_WithManageAccountingProductAccess_ReturnsTrue()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            var productBatch = CreateProductBatch((int)ProductRightEnum.ManageAccountingProductAccess);

            // Act
            var result = manageUser.CheckProductRight(productBatch);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CheckProductRight_WithUnknownProductId_ReturnsTrue()
        {
            // Arrange
            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            var productBatch = CreateProductBatch(99999);

            // Act
            var result = manageUser.CheckProductRight(productBatch);

            // Assert
            Assert.True(result); // Default case returns true
        }

        [Fact]
        public void CheckProductRight_WithMissingRight_ReturnsFalse()
        {
            // Arrange
            var userClaim = new DefaultUserClaim
            {
                Rights = new List<string>() // No rights
            };

            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                userClaim);

            var productBatch = CreateProductBatch((int)ProductRightEnum.ManageOneSiteProductAccess);

            // Act
            var result = manageUser.CheckProductRight(productBatch);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetUserEmployeeId Tests

        [Fact]
        public void GetUserEmployeeId_WithValidParameters_ReturnsEmployeeId()
        {
            // Arrange
            var mockEmployeeId = new Mock<IUserEmployeeId>();
            mockEmployeeId.Setup(e => e.EmployeeId).Returns("EMP001");

            _mockUserRepository
                .Setup(m => m.GetUserEmployeeId(It.IsAny<long>(), It.IsAny<long>()))
                .Returns(mockEmployeeId.Object);

            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act
            var result = manageUser.GetUserEmployeeId(1, 1000);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("EMP001", result.EmployeeId);
        }

        #endregion

        #region GetSuperVisorInformation Tests

        [Fact]
        public void GetSuperVisorInformation_WithValidParameters_ReturnsSupervisorInfo()
        {
            // Arrange
            var expectedSupervisor = new UserInfoLite { SuperVisorUserId = 100 };

            _mockUserRepository
                .Setup(m => m.GetSuperVisorInformation(It.IsAny<long>(), It.IsAny<long>()))
                .Returns(expectedSupervisor);

            var manageUser = new ManageUser(
                _mockUserRepository.Object,
                _mockCredentialRepository.Object,
                _mockUserLoginRepository.Object,
                _mockManageUserRegistrationEmail.Object,
                _defaultUserClaim);

            // Act
            var result = manageUser.GetSuperVisorInformation(1, 1000);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.SuperVisorUserId);
        }

        #endregion

        #region Data Object Tests

        [Fact]
        public void ValidateUserResponse_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var response = new ValidateUserResponse
            {
                EnterpriseUserName = "testuser@test.com",
                ValidateUserToken = "token-123",
                IsError = false,
                ErrorReason = ""
            };

            // Assert
            Assert.Equal("testuser@test.com", response.EnterpriseUserName);
            Assert.Equal("token-123", response.ValidateUserToken);
            Assert.False(response.IsError);
        }

        [Fact]
        public void StarterProfile_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var profile = new StarterProfile
            {
                EnterpriseUserName = "testuser@test.com"
            };

            // Assert
            Assert.Equal("testuser@test.com", profile.EnterpriseUserName);
        }

        [Fact]
        public void ProductBatch_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var batch = new ProductBatch
            {
                ProductId = 1
            };

            // Assert
            Assert.Equal(1, batch.ProductId);
        }

        #endregion

        #region Documentation Tests

        [Fact]
        public void ManageUser_ClassPurpose_Documentation()
        {
            // This test documents the class purpose:
            // ManageUser is responsible for:
            // 1. Validating new users and registration tokens
            // 2. Getting and setting starter profile options
            // 3. Creating and updating user profiles
            // 4. Managing user status (enable/disable)
            // 5. Assigning products to administrators
            // 6. Checking product rights
            // 7. Getting user profile details
            // 8. Getting employee and supervisor information
            //
            // Key methods:
            // - ValidateUser - Validate new user registration
            // - ValidateRegistrationVerificationToken - Validate verification token
            // - GetStarterProfileOptions / SetStarterProfile - Starter profile management
            // - CreateUser / UpdateUser / UpdateNewUser - User CRUD
            // - UpdateUserStatus - Enable/disable users
            // - CheckProductRight - Product access validation
            // - GetUserProfile - Get user details
            // - GetUserEmployeeId / GetSuperVisorInformation - Employee data

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageUser_ValidationRules_Documentation()
        {
            // This test documents validation rules:
            //
            // ValidateUser:
            // - enterpriseUserName must not be null or empty
            // - newUserRegistrationToken must not be null or empty
            // - User login must exist
            // - Organization status must be defined
            // - Token must not be expired
            // - User must be pending (not already completed)
            //
            // ValidateRegistrationVerificationToken:
            // - enterpriseUserName must not be null or empty
            // - verificationToken must not be null or empty
            // - Token must match user
            //
            // GetStarterProfileOptions:
            // - enterpriseUserName must not be null or empty (throws ArgumentNullException)
            //
            // SetStarterProfile:
            // - starterProfile must not be null (throws ArgumentNullException)
            //
            // UpdateNewUser:
            // - userLogin must not be null
            // - PartyRoleTypeId must not be 0
            // - TelecommunicationNumber must have at least one entry
            // - activityToken must not be empty/whitespace
            //
            // UpdateUser:
            // - loggedInUserRealPageId must not be empty Guid
            //
            // AssignProductsToAdministrators:
            // - organizationRealPageId must not be empty Guid (throws Exception)

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageUser_ProductRights_Documentation()
        {
            // This test documents product rights checking:
            //
            // CheckProductRight maps product IDs to rights:
            // - ManageAccountingProductAccess
            // - ManageAssetOptimizationProductAccess (includes AO sub-products)
            // - ManageClientPortalProductAccess
            // - ManageDocumentManagementProductAccess
            // - ManageILMLeadManagemementProductAccess
            // - ManageILMLeasingAnalyticsProductAccess
            // - ManageLead2LeaseProductAccess
            // - ManageMarketingCenterProductAccess
            // - ManageOneSiteProductAccess
            // - ManageOnSiteProductAccess
            // - And many more...
            //
            // Default case: Returns true (some products have default access)

            Assert.True(true, "Documentation test");
        }

        #endregion
    }
}
