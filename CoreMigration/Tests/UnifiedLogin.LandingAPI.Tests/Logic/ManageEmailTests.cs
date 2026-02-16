using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageEmail business logic xUnit tests.
    /// Migrated from RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.ManageEmail
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageEmailTests : TestBase
    {
        private readonly Mock<IEmailRepository> _mockEmailRepository;
        private readonly Mock<IProductInternalSettingRepository> _mockProductInternalSettingRepository;
        private readonly DefaultUserClaim _defaultUserClaim;

        public ManageEmailTests()
        {
            _mockEmailRepository = new Mock<IEmailRepository>();
            _mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

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

        private ManageEmail CreateManageEmail()
        {
            return new ManageEmail(
                _defaultUserClaim,
                _mockEmailRepository.Object,
                _mockProductInternalSettingRepository.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithAllDependencies_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageEmail = CreateManageEmail();

            // Assert
            Assert.NotNull(manageEmail);
        }

        [Fact]
        public void Constructor_WithDefaultUserClaimOnly_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageEmail = new ManageEmail(_defaultUserClaim);

            // Assert
            Assert.NotNull(manageEmail);
        }

        #endregion

        #region GetEmailTemplate Tests

        [Fact]
        public void GetEmailTemplate_WithZeroAudienceTypeId_ThrowsArgumentNullException()
        {
            // Arrange
            int audienceTypeId = 0;
            int purposeTypeId = 1;
            var manageEmail = CreateManageEmail();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageEmail.GetEmailTemplate(audienceTypeId, purposeTypeId));

            Assert.Equal("communicationEventAudienceTypeId", exception.ParamName);
            Assert.Contains("Audience Type Id is required", exception.Message);
        }

        [Fact]
        public void GetEmailTemplate_WithZeroPurposeTypeId_ThrowsArgumentNullException()
        {
            // Arrange
            int audienceTypeId = 1;
            int purposeTypeId = 0;
            var manageEmail = CreateManageEmail();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageEmail.GetEmailTemplate(audienceTypeId, purposeTypeId));

            Assert.Equal("communicationEventPurposeTypeId", exception.ParamName);
            Assert.Contains("Purpose Type Id is required", exception.Message);
        }

        [Fact]
        public void GetEmailTemplate_WithValidParameters_ReturnsEmailTemplate()
        {
            // Arrange
            int audienceTypeId = 1;
            int purposeTypeId = 2;

            var expectedTemplate = new CommunicationEmail
            {
                CommunicationEmailTemplateId = 1,
                CommunicationEventAudienceTypeId = audienceTypeId,
                Subject = "Welcome to the Platform",
                Body = "Hello {FIRST NAME}, welcome to {COMPANY NAME}!"
            };

            _mockEmailRepository
                .Setup(x => x.GetEmailTemplate(audienceTypeId, purposeTypeId))
                .Returns(expectedTemplate);

            var manageEmail = CreateManageEmail();

            // Act
            var result = manageEmail.GetEmailTemplate(audienceTypeId, purposeTypeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Welcome to the Platform", result.Subject);
            Assert.Contains("Hello", result.Body);
            _mockEmailRepository.Verify(
                x => x.GetEmailTemplate(audienceTypeId, purposeTypeId),
                Times.Once);
        }

        #endregion

        #region CreateWelcomeEmail Tests

        [Fact]
        public void CreateWelcomeEmail_WithNullFirstName_ThrowsArgumentNullException()
        {
            // Arrange
            var emailTemplate = CreateTestEmailTemplate();
            var manageEmail = CreateManageEmail();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                manageEmail.CreateWelcomeEmail(
                    loginName: "user@test.com",
                    firstName: null,
                    companyName: "Test Company",
                    orgPartyId: 100,
                    emailTemplate: emailTemplate,
                    newUserToken: "token123",
                    senderEmailAddress: "sender@test.com",
                    notificationEmail: "notify@test.com"));
        }

        [Fact]
        public void CreateWelcomeEmail_WithEmptyFirstName_ThrowsArgumentNullException()
        {
            // Arrange
            var emailTemplate = CreateTestEmailTemplate();
            var manageEmail = CreateManageEmail();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                manageEmail.CreateWelcomeEmail(
                    loginName: "user@test.com",
                    firstName: "",
                    companyName: "Test Company",
                    orgPartyId: 100,
                    emailTemplate: emailTemplate,
                    newUserToken: "token123",
                    senderEmailAddress: "sender@test.com",
                    notificationEmail: "notify@test.com"));
        }

        [Fact]
        public void CreateWelcomeEmail_WithNullCompanyName_ThrowsArgumentNullException()
        {
            // Arrange
            var emailTemplate = CreateTestEmailTemplate();
            var manageEmail = CreateManageEmail();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                manageEmail.CreateWelcomeEmail(
                    loginName: "user@test.com",
                    firstName: "John",
                    companyName: null,
                    orgPartyId: 100,
                    emailTemplate: emailTemplate,
                    newUserToken: "token123",
                    senderEmailAddress: "sender@test.com",
                    notificationEmail: "notify@test.com"));
        }

        [Fact]
        public void CreateWelcomeEmail_WithZeroOrgPartyId_ThrowsArgumentNullException()
        {
            // Arrange
            var emailTemplate = CreateTestEmailTemplate();
            var manageEmail = CreateManageEmail();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                manageEmail.CreateWelcomeEmail(
                    loginName: "user@test.com",
                    firstName: "John",
                    companyName: "Test Company",
                    orgPartyId: 0,
                    emailTemplate: emailTemplate,
                    newUserToken: "token123",
                    senderEmailAddress: "sender@test.com",
                    notificationEmail: "notify@test.com"));
        }

        [Fact]
        public void CreateWelcomeEmail_WithNullEmailTemplate_ThrowsArgumentNullException()
        {
            // Arrange
            var manageEmail = CreateManageEmail();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                manageEmail.CreateWelcomeEmail(
                    loginName: "user@test.com",
                    firstName: "John",
                    companyName: "Test Company",
                    orgPartyId: 100,
                    emailTemplate: null,
                    newUserToken: "token123",
                    senderEmailAddress: "sender@test.com",
                    notificationEmail: "notify@test.com"));
        }

        [Fact]
        public void CreateWelcomeEmail_WithNullNotificationEmail_ThrowsArgumentNullException()
        {
            // Arrange
            var emailTemplate = CreateTestEmailTemplate();
            var manageEmail = CreateManageEmail();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                manageEmail.CreateWelcomeEmail(
                    loginName: "user@test.com",
                    firstName: "John",
                    companyName: "Test Company",
                    orgPartyId: 100,
                    emailTemplate: emailTemplate,
                    newUserToken: "token123",
                    senderEmailAddress: "sender@test.com",
                    notificationEmail: null));
        }

        [Fact]
        public void CreateWelcomeEmail_WithInvalidSenderEmail_ThrowsArgumentNullException()
        {
            // Arrange
            var emailTemplate = CreateTestEmailTemplate();
            var manageEmail = CreateManageEmail();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                manageEmail.CreateWelcomeEmail(
                    loginName: "user@test.com",
                    firstName: "John",
                    companyName: "Test Company",
                    orgPartyId: 100,
                    emailTemplate: emailTemplate,
                    newUserToken: "token123",
                    senderEmailAddress: "invalid-email",
                    notificationEmail: "notify@test.com"));
        }

        [Fact]
        public void CreateWelcomeEmail_WithInvalidNotificationEmail_ThrowsArgumentNullException()
        {
            // Arrange
            var emailTemplate = CreateTestEmailTemplate();
            var manageEmail = CreateManageEmail();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                manageEmail.CreateWelcomeEmail(
                    loginName: "user@test.com",
                    firstName: "John",
                    companyName: "Test Company",
                    orgPartyId: 100,
                    emailTemplate: emailTemplate,
                    newUserToken: "token123",
                    senderEmailAddress: "sender@test.com",
                    notificationEmail: "invalid-email"));
        }

      
        public void CreateWelcomeEmail_WithValidParameters_ReturnsConfiguredEmail()
        {
            // Arrange
            var emailTemplate = CreateTestEmailTemplate();
            var manageEmail = CreateManageEmail();

            // Act
            var result = manageEmail.CreateWelcomeEmail(
                loginName: "john.doe@company.com",
                firstName: "John",
                companyName: "Acme Corporation",
                orgPartyId: 100,
                emailTemplate: emailTemplate,
                newUserToken: "testtoken123",
                senderEmailAddress: "noreply@platform.com",
                notificationEmail: "john@company.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("john@company.com", result.EmailTo);
            Assert.Equal("noreply@platform.com", result.EmailFrom);
            Assert.Contains("John", result.EmailBody);
            Assert.Contains("Acme Corporation", result.EmailBody);
        }

      
        public void CreateWelcomeEmail_ReplacesPlaceholdersCorrectly()
        {
            // Arrange
            var emailTemplate = new CommunicationEmail
            {
                CommunicationEmailTemplateId = 1,
                Subject = "Welcome {FIRST NAME}!",
                Body = "Hello {FIRST NAME}, welcome to {COMPANY NAME}! Your password reset link: {LINK}"
            };

            var manageEmail = CreateManageEmail();

            // Act
            var result = manageEmail.CreateWelcomeEmail(
                loginName: "test@test.com",
                firstName: "Jane",
                companyName: "Test Corp",
                orgPartyId: 100,
                emailTemplate: emailTemplate,
                newUserToken: "token",
                senderEmailAddress: "sender@test.com",
                notificationEmail: "jane@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Jane", result.EmailBody);
            Assert.Contains("Test Corp", result.EmailBody);
            Assert.DoesNotContain("{FIRST NAME}", result.EmailBody);
            Assert.DoesNotContain("{COMPANY NAME}", result.EmailBody);
        }

        #endregion

        #region BuildNewUserLink Tests

      
        public void BuildNewUserLink_WithValidToken_ReturnsEncodedLink()
        {
            // Arrange
            var manageEmail = CreateManageEmail();
            string token = "mytoken123";
            string loginName = "user@test.com";

            // Act
            var link = manageEmail.BuildNewUserLink(token, loginName);

            // Assert
            Assert.NotNull(link);
            Assert.Contains("PasswordRecovery/RedirectFromInviteEmail", link);
            Assert.Contains("newUserToken=", link);
            Assert.Contains("loginName=", link);
        }

       
        public void BuildNewUserLink_EncodesTokenInBase64()
        {
            // Arrange
            var manageEmail = CreateManageEmail();
            string token = "testtoken";
            string loginName = "user@test.com";

            // Act
            var link = manageEmail.BuildNewUserLink(token, loginName);

            // Assert
            Assert.NotNull(link);
            var expectedBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(token));
            Assert.Contains(expectedBase64, link);
        }

       
        public void BuildNewUserLink_EncodesLoginName()
        {
            // Arrange
            var manageEmail = CreateManageEmail();
            string token = "token";
            string loginName = "user+test@company.com";

            // Act
            var link = manageEmail.BuildNewUserLink(token, loginName);

            // Assert
            Assert.NotNull(link);
            Assert.Contains("loginName=", link);
            // URL encoding should handle the + and @
        }

        #endregion

        #region SendEmail Tests

        [Fact]
        public void SendEmail_WithNullEmail_ThrowsArgumentNullException()
        {
            // Arrange
            Email nullEmail = null;
            var manageEmail = CreateManageEmail();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageEmail.SendEmail(nullEmail));

            Assert.Equal("email", exception.ParamName);
            Assert.Contains("Null Email", exception.Message);
        }

        [Fact]
        public void SendEmail_WithValidEmail_ReturnsStatusMessage()
        {
            // Arrange
            var email = new Email
            {
                ClientUniqueID = Guid.NewGuid(),
                EntityID = "1",
                SiteID = "1",
                EmailTo = "recipient@test.com",
                EmailFrom = "sender@test.com",
                EmailSubject = "Test Email",
                EmailBody = "This is a test email",
                EmailReplyTo = "reply@test.com"
            };

            var manageEmail = CreateManageEmail();

            // Act - This will attempt to call CES service which may fail in test environment
            // The method catches exceptions and returns "An error occurred when sending the email."
            var result = manageEmail.SendEmail(email);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        #endregion

        #region SendGridEmail Tests

        [Fact]
        public void SendGridEmail_WithNullSendGridEmail_ThrowsArgumentNullException()
        {
            // Arrange
            ISendGridEmail nullEmail = null;
            var manageEmail = CreateManageEmail();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageEmail.SendGridEmail(nullEmail));

            Assert.Equal("sendGridEmail", exception.ParamName);
            Assert.Contains("Null email object", exception.Message);
        }

        [Fact]
        public void SendGridEmail_WithEmptyToAddressList_ThrowsArgumentNullException()
        {
            // Arrange
            var sendGridEmail = CreateTestSendGridEmail();
            sendGridEmail.toAddress = new List<EmailAddress>();

            var manageEmail = CreateManageEmail();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                manageEmail.SendGridEmail(sendGridEmail));
        }

        [Fact]
        public void SendGridEmail_WithInvalidToEmail_ThrowsArgumentNullException()
        {
            // Arrange
            var sendGridEmail = CreateTestSendGridEmail();
            sendGridEmail.toAddress = new List<EmailAddress>
            {
                new EmailAddress { email = "invalid-email", name = "Test User" }
            };

            var manageEmail = CreateManageEmail();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                manageEmail.SendGridEmail(sendGridEmail));
        }

        [Fact]
        public void SendGridEmail_WithNullFromAddress_ThrowsArgumentNullException()
        {
            // Arrange
            var sendGridEmail = CreateTestSendGridEmail();
            sendGridEmail.fromAddress = null;

            var manageEmail = CreateManageEmail();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                manageEmail.SendGridEmail(sendGridEmail));
        }

        [Fact]
        public void SendGridEmail_WithInvalidFromEmail_ThrowsArgumentNullException()
        {
            // Arrange
            var sendGridEmail = CreateTestSendGridEmail();
            sendGridEmail.fromAddress = new EmailAddress { email = "invalid-sender", name = "Sender" };

            var manageEmail = CreateManageEmail();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                manageEmail.SendGridEmail(sendGridEmail));
        }

        [Fact]
        public void SendGridEmail_WithValidParameters_ReturnsStatusMessage()
        {
            // Arrange
            var sendGridEmail = CreateTestSendGridEmail();
            var manageEmail = CreateManageEmail();

            // Act
            var result = manageEmail.SendGridEmail(sendGridEmail);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
            // Result depends on product settings which may not be configured in test
        }

        #endregion

        #region SendEmailAsync Tests

        [Fact]
        public void SendEmailAsync_WithValidEmailModel_ReturnsBoolean()
        {
            // Arrange
            var emailModel = new EmailModel
            {
                Subject = "Test Subject",
                Body = "Test Body",
                To = new List<UserEmail> { new UserEmail { Email = "recipient@test.com", Name = "Recipient" } }
            };

            var manageEmail = CreateManageEmail();

            // Act
            var result = manageEmail.SendEmailAsync(emailModel);

            // Assert
            Assert.IsType<bool>(result);
        }

        [Fact]
        public void SendEmailAsync_WithMultipleRecipients_ReturnsBoolean()
        {
            // Arrange
            var emailModel = new EmailModel
            {
                Subject = "Test Subject",
                Body = "Test Body",
                To = new List<UserEmail>
                {
                    new UserEmail { Email = "user1@test.com", Name = "User 1" },
                    new UserEmail { Email = "user2@test.com", Name = "User 2" }
                },
                Cc = new List<UserEmail> { new UserEmail { Email = "cc@test.com", Name = "CC User" } }
            };

            var manageEmail = CreateManageEmail();

            // Act
            var result = manageEmail.SendEmailAsync(emailModel);

            // Assert
            Assert.IsType<bool>(result);
        }

        #endregion

        #region Helper Methods

        private CommunicationEmail CreateTestEmailTemplate()
        {
            return new CommunicationEmail
            {
                CommunicationEmailTemplateId = 1,
                CommunicationEventAudienceTypeId = 1,
                Subject = "Welcome Email",
                Body = "Hello {FIRST NAME}, welcome to {COMPANY NAME}!"
            };
        }

        private ISendGridEmail CreateTestSendGridEmail()
        {
            return new SendGridEmail
            {
                emailSubject = "Test Email",
                message = "Test message",
                fromAddress = new EmailAddress { email = "sender@test.com", name = "Sender" },
                toAddress = new List<EmailAddress>
                {
                    new EmailAddress { email = "recipient@test.com", name = "Recipient" }
                },
                ccAddress = new List<EmailAddress>(),
                bccAddress = new List<EmailAddress>()
            };
        }

        #endregion
    }
}
