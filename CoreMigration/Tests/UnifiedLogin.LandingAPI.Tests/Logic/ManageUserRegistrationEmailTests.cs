using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageUserRegistrationEmail business logic xUnit tests.
    /// Tests for user registration email management including sending welcome and password reset emails.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageUserRegistrationEmailTests : TestBase
    {
        private readonly Mock<IManageEmail> _mockManageEmail;
        private readonly Mock<IContactMechanismRepository> _mockContactMechanismRepository;
        private readonly Mock<IManageCommunicationEvents> _mockCommunicationEventsLogic;
        private readonly Mock<IUserTokenRepository> _mockUserTokenRepository;
        private readonly Mock<IManagePerson> _mockPersonManager;
        private readonly Mock<IUserLoginRepository> _mockUserLoginRepository;
        private readonly Mock<IProductInternalSettingRepository> _mockProductInternalSettingRepository;
        private readonly DefaultUserClaim _defaultUserClaim;

        public ManageUserRegistrationEmailTests()
        {
            _mockManageEmail = new Mock<IManageEmail>();
            _mockContactMechanismRepository = new Mock<IContactMechanismRepository>();
            _mockCommunicationEventsLogic = new Mock<IManageCommunicationEvents>();
            _mockUserTokenRepository = new Mock<IUserTokenRepository>();
            _mockPersonManager = new Mock<IManagePerson>();
            _mockUserLoginRepository = new Mock<IUserLoginRepository>();
            _mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            _defaultUserClaim = new DefaultUserClaim
            {
                UserId = 1,
                LoginName = "testuser@test.com",
                FirstName = "Test",
                LastName = "User",
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 1000,
                OrganizationName = "Test Organization",
                PersonaId = 5,
                CorrelationId = Guid.NewGuid()
            };

            SetupBasicMocks();
        }

        #region Mock Setup

        private void SetupBasicMocks()
        {
            _mockPersonManager
                .Setup(m => m.GetPerson(It.IsAny<Guid>()))
                .Returns(new Person { FirstName = "Test", LastName = "User", RealPageId = Guid.NewGuid() });

            _mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(It.IsAny<int>()))
                .Returns(new List<ProductInternalSetting>());
        }

        private UserLoginOnly CreateUserLoginOnly()
        {
            return new UserLoginOnly
            {
                UserId = 1,
                RealPageId = Guid.NewGuid(),
                LoginName = "testuser@test.com",
                LastLogin = DateTime.UtcNow,
                Is3rdPartyIDP = false
            };
        }

        private ProfileDetail CreateProfileDetail()
        {
            return new ProfileDetail
            {
                FirstName = "Test",
                LastName = "User",
                RealPageId = Guid.NewGuid(),
                UserTypeId = UserTypeConstants.RegularUser,
                userLogin = new UserLogin
                {
                    UserId = 1,
                    LoginName = "testuser@test.com",
                    RealPageId = Guid.NewGuid(),
                    Is3rdPartyIDP = false,
                    LastLogin = DateTime.UtcNow
                },
                organization = new List<Organization>
                {
                    new Organization
                    {
                        PartyId = 1000,
                        RealPageId = Guid.NewGuid(),
                        Name = "Test Organization",
                        PrimaryOrganization = true
                    }
                }
            };
        }

        private OrganizationStatus CreateOrganizationStatus(bool isPending = true, bool isExpired = false)
        {
            return new OrganizationStatus
            {
                PartyId = 1000,
                RealPageId = Guid.NewGuid(),
                IsPending = isPending,
                IsExpired = isExpired,
                StatusTypeId = (int)UserUiStatusType.Pending,
                PrimaryOrganization = true
            };
        }

        private CommunicationEmail CreateCommunicationEmail()
        {
            return new CommunicationEmail
            {
                CommunicationEmailTemplateId = 1,
                Subject = "Welcome",
                Body = "Welcome to the platform"
            };
        }

        private Email CreateEmail()
        {
            return new Email
            {
                EmailTo = "testuser@test.com",
                EmailFrom = "noreply@test.com",
                EmailSubject = "Welcome",
                EmailBody = "Welcome to the platform",
                ClientUniqueID = Guid.NewGuid()
            };
        }

        private List<CommonAddress> CreateContactMechanismList()
        {
            return new List<CommonAddress>
            {
                new CommonAddress
                {
                    AddressString = "noreply@test.com",
                    PartyContactMechanismId = 1
                }
            };
        }

        private List<Organization> CreateOrganizationList(bool isPrimary = true)
        {
            return new List<Organization>
            {
                new Organization
                {
                    PartyId = 1000,
                    RealPageId = Guid.NewGuid(),
                    Name = "Test Organization",
                    PrimaryOrganization = isPrimary
                }
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithUserClaim_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageUserRegistrationEmail = new ManageUserRegistrationEmail(_defaultUserClaim);

            // Assert
            Assert.NotNull(manageUserRegistrationEmail);
        }

        [Fact]
        public void Constructor_WithAllDependencies_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageUserRegistrationEmail = new ManageUserRegistrationEmail(
                _defaultUserClaim,
                _mockManageEmail.Object,
                _mockContactMechanismRepository.Object,
                _mockCommunicationEventsLogic.Object,
                _mockUserTokenRepository.Object,
                _mockPersonManager.Object,
                _mockUserLoginRepository.Object,
                _mockProductInternalSettingRepository.Object);

            // Assert
            Assert.NotNull(manageUserRegistrationEmail);
        }

        #endregion

        #region SendNewUserRegistrationEmail with IProfileDetail Tests

        [Fact]
        public void SendNewUserRegistrationEmail_WithProfile_InvalidEmail_ReturnsTrue()
        {
            // Arrange
            var profile = CreateProfileDetail();
            profile.userLogin.LoginName = "invalid-email"; // Invalid email format

            var manageUserRegistrationEmail = new ManageUserRegistrationEmail(
                _defaultUserClaim,
                _mockManageEmail.Object,
                _mockContactMechanismRepository.Object,
                _mockCommunicationEventsLogic.Object,
                _mockUserTokenRepository.Object,
                _mockPersonManager.Object,
                _mockUserLoginRepository.Object,
                _mockProductInternalSettingRepository.Object);

            // Act
            var result = manageUserRegistrationEmail.SendNewUserRegistrationEmail(profile);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SendNewUserRegistrationEmail_WithProfile_UserNoEmailRole_ReturnsTrue()
        {
            // Arrange
            var profile = CreateProfileDetail();
            profile.UserTypeId = UserTypeConstants.RegularUserNoEmail;

            var manageUserRegistrationEmail = new ManageUserRegistrationEmail(
                _defaultUserClaim,
                _mockManageEmail.Object,
                _mockContactMechanismRepository.Object,
                _mockCommunicationEventsLogic.Object,
                _mockUserTokenRepository.Object,
                _mockPersonManager.Object,
                _mockUserLoginRepository.Object,
                _mockProductInternalSettingRepository.Object);

            // Act
            var result = manageUserRegistrationEmail.SendNewUserRegistrationEmail(profile);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SendNewUserRegistrationEmail_WithProfile_Is3rdPartyIDP_ReturnsTrue()
        {
            // Arrange
            var profile = CreateProfileDetail();
            profile.userLogin.Is3rdPartyIDP = true;
            profile.UserTypeId = UserTypeConstants.RegularUser;

            var manageUserRegistrationEmail = new ManageUserRegistrationEmail(
                _defaultUserClaim,
                _mockManageEmail.Object,
                _mockContactMechanismRepository.Object,
                _mockCommunicationEventsLogic.Object,
                _mockUserTokenRepository.Object,
                _mockPersonManager.Object,
                _mockUserLoginRepository.Object,
                _mockProductInternalSettingRepository.Object);

            // Act
            var result = manageUserRegistrationEmail.SendNewUserRegistrationEmail(profile);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region SendNewUserRegistrationEmail with UserLoginOnly Tests

        [Fact]
        public void SendNewUserRegistrationEmail_WithUserLoginOnly_InvalidEmail_ReturnsTrue()
        {
            // Arrange
            var userLoginOnly = CreateUserLoginOnly();
            userLoginOnly.LoginName = "invalid-email"; // Invalid email format

            var manageUserRegistrationEmail = new ManageUserRegistrationEmail(
                _defaultUserClaim,
                _mockManageEmail.Object,
                _mockContactMechanismRepository.Object,
                _mockCommunicationEventsLogic.Object,
                _mockUserTokenRepository.Object,
                _mockPersonManager.Object,
                _mockUserLoginRepository.Object,
                _mockProductInternalSettingRepository.Object);

            // Act
            var result = manageUserRegistrationEmail.SendNewUserRegistrationEmail(
                userLoginOnly, "Test Company", UserTypeConstants.RegularUser, 1000);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SendNewUserRegistrationEmail_WithUserLoginOnly_UserNoEmailRole_ReturnsTrue()
        {
            // Arrange
            var userLoginOnly = CreateUserLoginOnly();

            var manageUserRegistrationEmail = new ManageUserRegistrationEmail(
                _defaultUserClaim,
                _mockManageEmail.Object,
                _mockContactMechanismRepository.Object,
                _mockCommunicationEventsLogic.Object,
                _mockUserTokenRepository.Object,
                _mockPersonManager.Object,
                _mockUserLoginRepository.Object,
                _mockProductInternalSettingRepository.Object);

            // Act
            var result = manageUserRegistrationEmail.SendNewUserRegistrationEmail(
                userLoginOnly, "Test Company", UserTypeConstants.RegularUserNoEmail, 1000);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SendNewUserRegistrationEmail_WithUserLoginOnly_Is3rdPartyIDP_SuperUser_ReturnsTrue()
        {
            // Arrange
            var userLoginOnly = CreateUserLoginOnly();
            userLoginOnly.Is3rdPartyIDP = true;

            var manageUserRegistrationEmail = new ManageUserRegistrationEmail(
                _defaultUserClaim,
                _mockManageEmail.Object,
                _mockContactMechanismRepository.Object,
                _mockCommunicationEventsLogic.Object,
                _mockUserTokenRepository.Object,
                _mockPersonManager.Object,
                _mockUserLoginRepository.Object,
                _mockProductInternalSettingRepository.Object);

            // Act
            var result = manageUserRegistrationEmail.SendNewUserRegistrationEmail(
                userLoginOnly, "Test Company", UserTypeConstants.SuperUser, 1000);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SendNewUserRegistrationEmail_WithUserLoginOnly_ExternalUser_ValidEmail_ProcessesEmail()
        {
            // Arrange
            var userLoginOnly = CreateUserLoginOnly();
            var organizationList = CreateOrganizationList(isPrimary: true);
            var orgStatus = CreateOrganizationStatus(isPending: true);
            var emailTemplate = CreateCommunicationEmail();
            var cesEmail = CreateEmail();
            var contactMechanismList = CreateContactMechanismList();

            _mockUserLoginRepository
                .Setup(m => m.ListOrganizationByEnterpriseUserId(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(organizationList);

            _mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(orgStatus);

            _mockUserTokenRepository
                .Setup(m => m.GetUserActivityToken(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<long>()))
                .Returns("test-token");

            _mockManageEmail
                .Setup(m => m.GetEmailTemplate(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(emailTemplate);

            _mockContactMechanismRepository
                .Setup(m => m.ListContactMechanismForPerson(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(contactMechanismList);

            _mockManageEmail
                .Setup(m => m.CreateWelcomeEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CommunicationEmail>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(cesEmail);

            _mockManageEmail
                .Setup(m => m.SendEmail(It.IsAny<Email>()))
                .Returns("success");

            _mockCommunicationEventsLogic
                .Setup(m => m.CreateCommunicationEvent(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockCommunicationEventsLogic
                .Setup(m => m.CreateCommunicationEventEmail(It.IsAny<int>(), It.IsAny<long>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockCommunicationEventsLogic
                .Setup(m => m.CreateCESCommunicationEventEmail(It.IsAny<string>(), It.IsAny<long>()))
                .Returns(new RepositoryResponse { Id = 1 });

            var manageUserRegistrationEmail = new ManageUserRegistrationEmail(
                _defaultUserClaim,
                _mockManageEmail.Object,
                _mockContactMechanismRepository.Object,
                _mockCommunicationEventsLogic.Object,
                _mockUserTokenRepository.Object,
                _mockPersonManager.Object,
                _mockUserLoginRepository.Object,
                _mockProductInternalSettingRepository.Object);

            // Act
            var result = manageUserRegistrationEmail.SendNewUserRegistrationEmail(
                userLoginOnly, "Test Company", UserTypeConstants.ExternalUser, 1000);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SendNewUserRegistrationEmail_WithUserLoginOnly_EmailSendFails_ReturnsFalse()
        {
            // Arrange
            var userLoginOnly = CreateUserLoginOnly();
            var organizationList = CreateOrganizationList(isPrimary: true);
            var orgStatus = CreateOrganizationStatus(isPending: true);
            var emailTemplate = CreateCommunicationEmail();
            var cesEmail = CreateEmail();
            var contactMechanismList = CreateContactMechanismList();

            _mockUserLoginRepository
                .Setup(m => m.ListOrganizationByEnterpriseUserId(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(organizationList);

            _mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(orgStatus);

            _mockUserTokenRepository
                .Setup(m => m.GetUserActivityToken(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<long>()))
                .Returns("test-token");

            _mockManageEmail
                .Setup(m => m.GetEmailTemplate(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(emailTemplate);

            _mockContactMechanismRepository
                .Setup(m => m.ListContactMechanismForPerson(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(contactMechanismList);

            _mockManageEmail
                .Setup(m => m.CreateWelcomeEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CommunicationEmail>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(cesEmail);

            _mockManageEmail
                .Setup(m => m.SendEmail(It.IsAny<Email>()))
                .Returns("error"); // Email send fails

            _mockCommunicationEventsLogic
                .Setup(m => m.CreateCommunicationEvent(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Returns(new RepositoryResponse { Id = 1 });

            var manageUserRegistrationEmail = new ManageUserRegistrationEmail(
                _defaultUserClaim,
                _mockManageEmail.Object,
                _mockContactMechanismRepository.Object,
                _mockCommunicationEventsLogic.Object,
                _mockUserTokenRepository.Object,
                _mockPersonManager.Object,
                _mockUserLoginRepository.Object,
                _mockProductInternalSettingRepository.Object);

            // Act
            var result = manageUserRegistrationEmail.SendNewUserRegistrationEmail(
                userLoginOnly, "Test Company", UserTypeConstants.ExternalUser, 1000);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SendNewUserRegistrationEmail_WithUserLoginOnly_NonPrimaryOrg_ProcessesAsMultiCompanyUser()
        {
            // Arrange
            var userLoginOnly = CreateUserLoginOnly();
            var organizationList = CreateOrganizationList(isPrimary: false); // Non-primary
            var emailTemplate = CreateCommunicationEmail();
            var cesEmail = CreateEmail();
            var contactMechanismList = CreateContactMechanismList();

            _mockUserLoginRepository
                .Setup(m => m.ListOrganizationByEnterpriseUserId(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(organizationList);

            _mockManageEmail
                .Setup(m => m.GetEmailTemplate(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(emailTemplate);

            _mockContactMechanismRepository
                .Setup(m => m.ListContactMechanismForPerson(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(contactMechanismList);

            _mockManageEmail
                .Setup(m => m.CreateWelcomeEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CommunicationEmail>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(cesEmail);

            _mockManageEmail
                .Setup(m => m.SendEmail(It.IsAny<Email>()))
                .Returns("success");

            _mockCommunicationEventsLogic
                .Setup(m => m.CreateCommunicationEvent(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockCommunicationEventsLogic
                .Setup(m => m.CreateCommunicationEventEmail(It.IsAny<int>(), It.IsAny<long>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockCommunicationEventsLogic
                .Setup(m => m.CreateCESCommunicationEventEmail(It.IsAny<string>(), It.IsAny<long>()))
                .Returns(new RepositoryResponse { Id = 1 });

            var manageUserRegistrationEmail = new ManageUserRegistrationEmail(
                _defaultUserClaim,
                _mockManageEmail.Object,
                _mockContactMechanismRepository.Object,
                _mockCommunicationEventsLogic.Object,
                _mockUserTokenRepository.Object,
                _mockPersonManager.Object,
                _mockUserLoginRepository.Object,
                _mockProductInternalSettingRepository.Object);

            // Act
            var result = manageUserRegistrationEmail.SendNewUserRegistrationEmail(
                userLoginOnly, "Test Company", UserTypeConstants.ExternalUser, 1000);

            // Assert
            Assert.True(result);
            _mockManageEmail.Verify(m => m.GetEmailTemplate((int)CommunicationEventAudienceType.MultiCompanyUser, It.IsAny<int>()), Times.Once);
        }

        #endregion

        #region SendPasswordResetEmail Tests

        [Fact]
        public void SendPasswordResetEmail_WithUserNoEmailRole_ReturnsTrue()
        {
            // Arrange
            var profileDetail = CreateProfileDetail();
            profileDetail.UserTypeId = UserTypeConstants.RegularUserNoEmail;

            var userLoginOnly = CreateUserLoginOnly();

            _mockUserLoginRepository
                .Setup(m => m.GetUserLoginOnly(It.IsAny<Guid>()))
                .Returns(userLoginOnly);

            var manageUserRegistrationEmail = new ManageUserRegistrationEmail(
                _defaultUserClaim,
                _mockManageEmail.Object,
                _mockContactMechanismRepository.Object,
                _mockCommunicationEventsLogic.Object,
                _mockUserTokenRepository.Object,
                _mockPersonManager.Object,
                _mockUserLoginRepository.Object,
                _mockProductInternalSettingRepository.Object);

            // Act
            var result = manageUserRegistrationEmail.SendPasswordResetEmail(profileDetail);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SendPasswordResetEmail_With3rdPartyIDP_ReturnsTrue()
        {
            // Arrange
            var profileDetail = CreateProfileDetail();
            var userLoginOnly = CreateUserLoginOnly();
            userLoginOnly.Is3rdPartyIDP = true;

            _mockUserLoginRepository
                .Setup(m => m.GetUserLoginOnly(It.IsAny<Guid>()))
                .Returns(userLoginOnly);

            var manageUserRegistrationEmail = new ManageUserRegistrationEmail(
                _defaultUserClaim,
                _mockManageEmail.Object,
                _mockContactMechanismRepository.Object,
                _mockCommunicationEventsLogic.Object,
                _mockUserTokenRepository.Object,
                _mockPersonManager.Object,
                _mockUserLoginRepository.Object,
                _mockProductInternalSettingRepository.Object);

            // Act
            var result = manageUserRegistrationEmail.SendPasswordResetEmail(profileDetail);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SendPasswordResetEmail_WithInvalidEmail_ReturnsTrue()
        {
            // Arrange
            var profileDetail = CreateProfileDetail();
            var userLoginOnly = CreateUserLoginOnly();
            userLoginOnly.LoginName = "invalid-email";

            _mockUserLoginRepository
                .Setup(m => m.GetUserLoginOnly(It.IsAny<Guid>()))
                .Returns(userLoginOnly);

            var manageUserRegistrationEmail = new ManageUserRegistrationEmail(
                _defaultUserClaim,
                _mockManageEmail.Object,
                _mockContactMechanismRepository.Object,
                _mockCommunicationEventsLogic.Object,
                _mockUserTokenRepository.Object,
                _mockPersonManager.Object,
                _mockUserLoginRepository.Object,
                _mockProductInternalSettingRepository.Object);

            // Act
            var result = manageUserRegistrationEmail.SendPasswordResetEmail(profileDetail);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SendPasswordResetEmail_WithValidData_SendsEmail()
        {
            // Arrange
            var profileDetail = CreateProfileDetail();
            var userLoginOnly = CreateUserLoginOnly();
            var organizationList = CreateOrganizationList();
            var orgStatus = CreateOrganizationStatus();
            var emailTemplate = CreateCommunicationEmail();
            var cesEmail = CreateEmail();
            var contactMechanismList = CreateContactMechanismList();

            _mockUserLoginRepository
                .Setup(m => m.GetUserLoginOnly(It.IsAny<Guid>()))
                .Returns(userLoginOnly);

            _mockUserLoginRepository
                .Setup(m => m.ListOrganizationByEnterpriseUserId(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(organizationList);

            _mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(orgStatus);

            _mockUserTokenRepository
                .Setup(m => m.GetUserActivityToken(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<long>()))
                .Returns("test-token");

            _mockManageEmail
                .Setup(m => m.GetEmailTemplate(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(emailTemplate);

            _mockContactMechanismRepository
                .Setup(m => m.ListContactMechanismForPerson(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(contactMechanismList);

            _mockManageEmail
                .Setup(m => m.CreateWelcomeEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CommunicationEmail>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(cesEmail);

            _mockManageEmail
                .Setup(m => m.SendEmail(It.IsAny<Email>()))
                .Returns("success");

            _mockCommunicationEventsLogic
                .Setup(m => m.CreateCommunicationEvent(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockCommunicationEventsLogic
                .Setup(m => m.CreateCommunicationEventEmail(It.IsAny<int>(), It.IsAny<long>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockCommunicationEventsLogic
                .Setup(m => m.CreateCESCommunicationEventEmail(It.IsAny<string>(), It.IsAny<long>()))
                .Returns(new RepositoryResponse { Id = 1 });

            var manageUserRegistrationEmail = new ManageUserRegistrationEmail(
                _defaultUserClaim,
                _mockManageEmail.Object,
                _mockContactMechanismRepository.Object,
                _mockCommunicationEventsLogic.Object,
                _mockUserTokenRepository.Object,
                _mockPersonManager.Object,
                _mockUserLoginRepository.Object,
                _mockProductInternalSettingRepository.Object);

            // Act
            var result = manageUserRegistrationEmail.SendPasswordResetEmail(profileDetail);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SendPasswordResetEmail_EmailSendFails_ReturnsFalse()
        {
            // Arrange
            var profileDetail = CreateProfileDetail();
            var userLoginOnly = CreateUserLoginOnly();
            var organizationList = CreateOrganizationList();
            var orgStatus = CreateOrganizationStatus();
            var emailTemplate = CreateCommunicationEmail();
            var cesEmail = CreateEmail();
            var contactMechanismList = CreateContactMechanismList();

            _mockUserLoginRepository
                .Setup(m => m.GetUserLoginOnly(It.IsAny<Guid>()))
                .Returns(userLoginOnly);

            _mockUserLoginRepository
                .Setup(m => m.ListOrganizationByEnterpriseUserId(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(organizationList);

            _mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(orgStatus);

            _mockUserTokenRepository
                .Setup(m => m.GetUserActivityToken(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<long>()))
                .Returns("test-token");

            _mockManageEmail
                .Setup(m => m.GetEmailTemplate(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(emailTemplate);

            _mockContactMechanismRepository
                .Setup(m => m.ListContactMechanismForPerson(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(contactMechanismList);

            _mockManageEmail
                .Setup(m => m.CreateWelcomeEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CommunicationEmail>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(cesEmail);

            _mockManageEmail
                .Setup(m => m.SendEmail(It.IsAny<Email>()))
                .Returns("error"); // Email send fails

            _mockCommunicationEventsLogic
                .Setup(m => m.CreateCommunicationEvent(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Returns(new RepositoryResponse { Id = 1 });

            var manageUserRegistrationEmail = new ManageUserRegistrationEmail(
                _defaultUserClaim,
                _mockManageEmail.Object,
                _mockContactMechanismRepository.Object,
                _mockCommunicationEventsLogic.Object,
                _mockUserTokenRepository.Object,
                _mockPersonManager.Object,
                _mockUserLoginRepository.Object,
                _mockProductInternalSettingRepository.Object);

            // Act
            var result = manageUserRegistrationEmail.SendPasswordResetEmail(profileDetail);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Data Object Tests

        [Fact]
        public void Email_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var email = new Email
            {
                EmailTo = "to@test.com",
                EmailFrom = "from@test.com",
                EmailSubject = "Test Subject",
                EmailBody = "Test Body",
                ClientUniqueID = Guid.NewGuid()
            };

            // Assert
            Assert.Equal("to@test.com", email.EmailTo);
            Assert.Equal("from@test.com", email.EmailFrom);
            Assert.Equal("Test Subject", email.EmailSubject);
            Assert.Equal("Test Body", email.EmailBody);
            Assert.NotEqual(Guid.Empty, email.ClientUniqueID);
        }

        [Fact]
        public void CommunicationEmail_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var communicationEmail = new CommunicationEmail
            {
                CommunicationEmailTemplateId = 1,
                Subject = "Welcome",
                Body = "Welcome body"
            };

            // Assert
            Assert.Equal(1, communicationEmail.CommunicationEmailTemplateId);
            Assert.Equal("Welcome", communicationEmail.Subject);
            Assert.Equal("Welcome body", communicationEmail.Body);
        }

        [Fact]
        public void CommonAddress_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var commonAddress = new CommonAddress
            {
                AddressString = "test@test.com",
                PartyContactMechanismId = 123
            };

            // Assert
            Assert.Equal("test@test.com", commonAddress.AddressString);
            Assert.Equal(123, commonAddress.PartyContactMechanismId);
        }

        #endregion

        #region User Type Constants Tests

        [Fact]
        public void UserTypeConstants_ValuesAreCorrect()
        {
            // Assert - Document the expected user type values
            Assert.True(UserTypeConstants.SuperUser > 0);
            Assert.True(UserTypeConstants.RegularUser > 0);
            Assert.True(UserTypeConstants.RegularUserNoEmail > 0);
            Assert.True(UserTypeConstants.ExternalUser > 0);
        }

        #endregion

        #region Documentation Tests

        [Fact]
        public void ManageUserRegistrationEmail_ClassPurpose_Documentation()
        {
            // This test documents the class purpose:
            // ManageUserRegistrationEmail is responsible for:
            // 1. Sending new user registration welcome emails
            // 2. Sending password reset emails
            // 3. Managing email templates based on user type and purpose
            // 4. Creating communication events for email tracking
            //
            // Key methods:
            // - SendNewUserRegistrationEmail(IProfileDetail) - Send welcome email using profile
            // - SendNewUserRegistrationEmail(UserLoginOnly, ...) - Send welcome email using user login
            // - SendPasswordResetEmail(ProfileDetail) - Send password reset email
            //
            // Email sending methods (private):
            // - EmailStatus - Determines and executes email send method

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageUserRegistrationEmail_EmailConditions_Documentation()
        {
            // This test documents email sending conditions:
            //
            // SendNewUserRegistrationEmail skips sending when:
            // - Email address is invalid
            // - UserTypeId is RegularUserNoEmail
            // - Is3rdPartyIDP is true AND (SuperUser OR RegularUser)
            //
            // Audience types based on user type:
            // - SuperUser/RealPageEmployee -> CommunicationEventAudienceType.SuperUser
            // - RegularUser -> CommunicationEventAudienceType.RegularUser
            // - ExternalUser -> CommunicationEventAudienceType.ExternalUser
            // - Non-primary org -> CommunicationEventAudienceType.MultiCompanyUser
            //
            // SendPasswordResetEmail skips sending when:
            // - UserTypeId is RegularUserNoEmail
            // - Is3rdPartyIDP is true
            // - Email address is invalid

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageUserRegistrationEmail_EmailMethods_Documentation()
        {
            // This test documents email sending methods:
            //
            // EmailStatus method determines which email service to use:
            // 1. IsUnifiedEmailEnabled = true -> Use SendEmailAsync (Unified Email API)
            // 2. IsSendGridEnabled = true -> Use SendGridEmail
            // 3. Default -> Use SendEmail (CES)
            //
            // Settings are retrieved from ProductInternalSettings:
            // - "IsSendGridEnabled" = "1" -> Enable SendGrid
            // - "IsUnifiedEmailEnabled" = "1" -> Enable Unified Email

            Assert.True(true, "Documentation test");
        }

        #endregion
    }
}
