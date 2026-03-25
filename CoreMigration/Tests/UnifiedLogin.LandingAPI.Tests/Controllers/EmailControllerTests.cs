using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for EmailController.
    /// Tests all endpoints, error cases, and edge cases for 100% code coverage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EmailControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IManageEmailAsync> _mockManageEmail;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private EmailController _emailController;

        #endregion

        #region Constructor

        public EmailControllerTests()
        {
            _mockManageEmail = new Mock<IManageEmailAsync>();
            _mockUserClaimsAccessor = MockUserClaimsAccessor;

            _emailController = new EmailController(
                _mockManageEmail.Object,
                _mockUserClaimsAccessor.Object
            )
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Act
            var controller = new EmailController(_mockManageEmail.Object, _mockUserClaimsAccessor.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullManageEmail_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new EmailController(null!, _mockUserClaimsAccessor.Object));

            Assert.Equal("manageEmail", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new EmailController(_mockManageEmail.Object, null!));

            Assert.Equal("userClaimsAccessor", exception.ParamName);
        }

        #endregion

        #region SendEmail Tests - Success

        [Fact]
        public async Task SendEmail_WithValidSendGridEmail_ReturnsOkWithResult()
        {
            // Arrange
            var sendGridEmail = CreateValidSendGridEmail();
            const string expectedResult = "Email sent successfully.";

            _mockManageEmail
                .Setup(x => x.SendGridEmailAsync(sendGridEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _emailController.SendEmail(sendGridEmail);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResult, okResult.Value);
        }

        [Fact]
        public async Task SendEmail_WithMinimalEmailData_ReturnsOkResult()
        {
            // Arrange
            var sendGridEmail = new SendGridEmail
            {
                fromAddress = new EmailAddress { email = "sender@example.com" },
                toAddress = new List<EmailAddress>
                {
                    new EmailAddress { email = "recipient@example.com" }
                },
                emailSubject = "Test Subject",
                message = "Test Message"
            };
            const string expectedResult = "Email sent successfully.";

            _mockManageEmail
                .Setup(x => x.SendGridEmailAsync(sendGridEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _emailController.SendEmail(sendGridEmail);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResult, okResult.Value);
        }

        [Fact]
        public async Task SendEmail_CallsManageEmailWithCorrectParameter()
        {
            // Arrange
            var sendGridEmail = CreateValidSendGridEmail();

            _mockManageEmail
                .Setup(x => x.SendGridEmailAsync(It.IsAny<ISendGridEmail>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Email sent successfully.");

            // Act
            await _emailController.SendEmail(sendGridEmail);

            // Assert
            _mockManageEmail.Verify(
                x => x.SendGridEmailAsync(sendGridEmail, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SendEmail_WithCcAndBccAddresses_ReturnsOkResult()
        {
            // Arrange
            var sendGridEmail = CreateValidSendGridEmail();
            sendGridEmail.ccAddress = new List<EmailAddress>
            {
                new EmailAddress { email = "cc@example.com", name = "CC User" }
            };
            sendGridEmail.bccAddress = new List<EmailAddress>
            {
                new EmailAddress { email = "bcc@example.com", name = "BCC User" }
            };

            _mockManageEmail
                .Setup(x => x.SendGridEmailAsync(sendGridEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync("Email sent successfully.");

            // Act
            var result = await _emailController.SendEmail(sendGridEmail);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task SendEmail_WithAttachment_ReturnsOkResult()
        {
            // Arrange
            var sendGridEmail = CreateValidSendGridEmail();
            sendGridEmail.attachment = new Attachment
            {
                attachmentContent = "base64content",
                attachmentFileName = "test.pdf",
                attachmentContentType = "application/pdf"
            };

            _mockManageEmail
                .Setup(x => x.SendGridEmailAsync(sendGridEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync("Email sent successfully.");

            // Act
            var result = await _emailController.SendEmail(sendGridEmail);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task SendEmail_WithHighPriority_ReturnsOkResult()
        {
            // Arrange
            var sendGridEmail = CreateValidSendGridEmail();
            sendGridEmail.priority = "high";

            _mockManageEmail
                .Setup(x => x.SendGridEmailAsync(sendGridEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync("Email sent successfully.");

            // Act
            var result = await _emailController.SendEmail(sendGridEmail);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task SendEmail_WithCategory_ReturnsOkResult()
        {
            // Arrange
            var sendGridEmail = CreateValidSendGridEmail();
            sendGridEmail.category = "WelcomeEmail";
            sendGridEmail.transId = "trans-12345";

            _mockManageEmail
                .Setup(x => x.SendGridEmailAsync(sendGridEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync("Email sent successfully.");

            // Act
            var result = await _emailController.SendEmail(sendGridEmail);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region SendEmail Tests - BadRequest

        [Fact]
        public async Task SendEmail_WithNullSendGridEmail_ReturnsBadRequest()
        {
            // Act
            var result = await _emailController.SendEmail(null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Null parameter: sendGridEmail.", badRequestResult.Value);
        }

        #endregion

        #region SendEmail Tests - Service Response Variations

        [Fact]
        public async Task SendEmail_WhenSendGridDisabled_ReturnsOkWithDisabledMessage()
        {
            // Arrange
            var sendGridEmail = CreateValidSendGridEmail();
            const string expectedResult = "SendGrid emails is disabled.";

            _mockManageEmail
                .Setup(x => x.SendGridEmailAsync(sendGridEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _emailController.SendEmail(sendGridEmail);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResult, okResult.Value);
        }

        [Fact]
        public async Task SendEmail_WhenErrorOccurs_ReturnsOkWithErrorMessage()
        {
            // Arrange
            var sendGridEmail = CreateValidSendGridEmail();
            const string expectedResult = "An error occured when sending the email.";

            _mockManageEmail
                .Setup(x => x.SendGridEmailAsync(sendGridEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _emailController.SendEmail(sendGridEmail);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResult, okResult.Value);
        }

        [Fact]
        public async Task SendEmail_WhenInvalidProductSettings_ReturnsOkWithSettingsMessage()
        {
            // Arrange
            var sendGridEmail = CreateValidSendGridEmail();
            const string expectedResult = "Invalid product settings for Unified Platform.";

            _mockManageEmail
                .Setup(x => x.SendGridEmailAsync(sendGridEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _emailController.SendEmail(sendGridEmail);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResult, okResult.Value);
        }

        [Fact]
        public async Task SendEmail_WhenServiceReturnsNull_ReturnsOkWithNull()
        {
            // Arrange
            var sendGridEmail = CreateValidSendGridEmail();

            _mockManageEmail
                .Setup(x => x.SendGridEmailAsync(sendGridEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync((string)null!);

            // Act
            var result = await _emailController.SendEmail(sendGridEmail);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        [Fact]
        public async Task SendEmail_WhenServiceReturnsEmptyString_ReturnsOkWithEmptyString()
        {
            // Arrange
            var sendGridEmail = CreateValidSendGridEmail();

            _mockManageEmail
                .Setup(x => x.SendGridEmailAsync(sendGridEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync(string.Empty);

            // Act
            var result = await _emailController.SendEmail(sendGridEmail);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(string.Empty, okResult.Value);
        }

        #endregion

        #region SendEmail Tests - Multiple Recipients

        [Fact]
        public async Task SendEmail_WithMultipleToAddresses_ReturnsOkResult()
        {
            // Arrange
            var sendGridEmail = new SendGridEmail
            {
                fromAddress = new EmailAddress { email = "sender@example.com", name = "Sender" },
                toAddress = new List<EmailAddress>
                {
                    new EmailAddress { email = "recipient1@example.com", name = "Recipient 1" },
                    new EmailAddress { email = "recipient2@example.com", name = "Recipient 2" },
                    new EmailAddress { email = "recipient3@example.com", name = "Recipient 3" }
                },
                emailSubject = "Test Subject",
                message = "Test Message"
            };

            _mockManageEmail
                .Setup(x => x.SendGridEmailAsync(sendGridEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync("Email sent successfully.");

            // Act
            var result = await _emailController.SendEmail(sendGridEmail);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Concurrent Access Tests

        [Fact]
        public async Task SendEmail_MultipleConcurrentCalls_AllReturnOk()
        {
            // Arrange
            _mockManageEmail
                .Setup(x => x.SendGridEmailAsync(It.IsAny<ISendGridEmail>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Email sent successfully.");

            var tasks = new List<Task<IActionResult>>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                var email = CreateValidSendGridEmail();
                email.emailSubject = $"Test Subject {i}";
                tasks.Add(_emailController.SendEmail(email));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            foreach (var result in results)
            {
                Assert.IsType<OkObjectResult>(result);
            }
        }

        #endregion

        #region Helper Methods

        private static SendGridEmail CreateValidSendGridEmail()
        {
            return new SendGridEmail
            {
                fromAddress = new EmailAddress
                {
                    email = "sender@example.com",
                    name = "Test Sender"
                },
                toAddress = new List<EmailAddress>
                {
                    new EmailAddress
                    {
                        email = "recipient@example.com",
                        name = "Test Recipient"
                    }
                },
                emailSubject = "Test Email Subject",
                message = "<html><body>Test Email Message</body></html>",
                priority = "low"
            };
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _emailController = null!;
            base.Dispose();
        }

        #endregion
    }
}

