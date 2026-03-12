using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for CredentialController.
    /// Tests all endpoints, error cases, and edge cases for 100% code coverage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CredentialControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IUserLoginRepository> _mockUserLoginRepository;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private readonly Mock<IManageCredential> _mockManageCredential;
        private readonly Mock<ILogger<CredentialController>> _mockLogger;
        private CredentialController _credentialController;

        #endregion

        #region Constructor

        public CredentialControllerTests()
        {
            _mockUserLoginRepository = new Mock<IUserLoginRepository>();
            _mockUserClaimsAccessor = MockUserClaimsAccessor;
            _mockManageCredential = new Mock<IManageCredential>();
            _mockLogger = new Mock<ILogger<CredentialController>>();

            _credentialController = new CredentialController(
                _mockUserLoginRepository.Object,
                _mockManageCredential.Object,
                _mockLogger.Object,
                _mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            // The controller constructor creates a concrete ManageCredential instead of using the injected one.
            // Use reflection to replace it with the mock so unit tests can control behavior.
            var field = typeof(CredentialController).GetField("_manageCredential", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(_credentialController, _mockManageCredential.Object);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Act
            var controller = new CredentialController(
                _mockUserLoginRepository.Object,
                _mockManageCredential.Object,
                _mockLogger.Object,
                _mockUserClaimsAccessor.Object);

            // Assert
            Assert.NotNull(controller);
        }

        #endregion

        #region GetSecurityQuestions Tests

        [Fact]
        public async Task GetSecurityQuestions_WithValidUserName_ReturnsOkResult()
        {
            // Arrange
            const string enterpriseUserName = "testuser@example.com";
            var expectedResponse = new SecurityQuestionResponse
            {
                IsError = false,
                EnterpriseUserName = enterpriseUserName,
                SecurityQuestions = new List<SecurityQuestion>
                {
                    new SecurityQuestion { SecurityQuestionId = 1, Question = "What is your pet's name?" }
                }
            };

            _mockManageCredential
                .Setup(x => x.GetSecurityQuestion(enterpriseUserName, It.IsAny<UserDeviceDetails>()))
                .Returns(expectedResponse);

            // Act
            var result = await _credentialController.GetSecurityQuestions(enterpriseUserName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<SecurityQuestionResponse>(okResult.Value);
            Assert.False(response.IsError);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetSecurityQuestions_WithEmptyUserName_ReturnsBadRequest(string enterpriseUserName)
        {
            // Act
            var result = await _credentialController.GetSecurityQuestions(enterpriseUserName);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("enterpriseUserName is required", badRequestResult.Value);
        }

        [Fact]
        public async Task GetSecurityQuestions_WhenServiceReturnsError_ReturnsOkWithError()
        {
            // Arrange
            const string enterpriseUserName = "testuser@example.com";
            var expectedResponse = new SecurityQuestionResponse
            {
                IsError = true,
                ErrorReason = "User not found"
            };

            _mockManageCredential
                .Setup(x => x.GetSecurityQuestion(enterpriseUserName, It.IsAny<UserDeviceDetails>()))
                .Returns(expectedResponse);

            // Act
            var result = await _credentialController.GetSecurityQuestions(enterpriseUserName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<SecurityQuestionResponse>(okResult.Value);
            Assert.True(response.IsError);
        }

        [Fact]
        public async Task GetSecurityQuestions_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            const string enterpriseUserName = "testuser@example.com";

            _mockManageCredential
                .Setup(x => x.GetSecurityQuestion(It.IsAny<string>(), It.IsAny<UserDeviceDetails>()))
                .Throws(new Exception("Database error"));

            // Act
            var result = await _credentialController.GetSecurityQuestions(enterpriseUserName);

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetSecurityQuestions_TrimsUserName_CallsServiceWithTrimmedValue()
        {
            // Arrange
            const string enterpriseUserName = "  testuser@example.com  ";
            var expectedResponse = new SecurityQuestionResponse { IsError = false };

            _mockManageCredential
                .Setup(x => x.GetSecurityQuestion("testuser@example.com", It.IsAny<UserDeviceDetails>()))
                .Returns(expectedResponse);

            // Act
            await _credentialController.GetSecurityQuestions(enterpriseUserName);

            // Assert
            _mockManageCredential.Verify(
                x => x.GetSecurityQuestion("testuser@example.com", It.IsAny<UserDeviceDetails>()),
                Times.Once);
        }

        #endregion

        #region VerifySecurityAnswers Tests

        [Fact]
        public async Task VerifySecurityAnswers_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var userSecurityAnswer = new UserSecurityAnswer
            {
                EnterpriseUserName = "testuser@example.com",
                SecurityQuestionAnswers = new List<SecurityQuestionAnswer>
                {
                    new SecurityQuestionAnswer { SecurityQuestionId = 1, Answer = "Fluffy" }
                }
            };
            var expectedResponse = new SecurityAnswerResponse { IsError = false };

            _mockManageCredential
                .Setup(x => x.VerifySecurityAnswers(userSecurityAnswer, It.IsAny<UserDeviceDetails>()))
                .Returns(expectedResponse);

            // Act
            var result = await _credentialController.VerifySecurityAnswers(userSecurityAnswer);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task VerifySecurityAnswers_WithNullData_ReturnsBadRequest()
        {
            // Act
            var result = await _credentialController.VerifySecurityAnswers(null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("userSecurityAnswer is required", badRequestResult.Value);
        }

        [Fact]
        public async Task VerifySecurityAnswers_WhenServiceReturnsError_ReturnsOkWithError()
        {
            // Arrange
            var userSecurityAnswer = new UserSecurityAnswer { EnterpriseUserName = "test@example.com" };
            var expectedResponse = new SecurityAnswerResponse
            {
                IsError = true,
                ErrorReason = "Incorrect answers"
            };

            _mockManageCredential
                .Setup(x => x.VerifySecurityAnswers(userSecurityAnswer, It.IsAny<UserDeviceDetails>()))
                .Returns(expectedResponse);

            // Act
            var result = await _credentialController.VerifySecurityAnswers(userSecurityAnswer);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<SecurityAnswerResponse>(okResult.Value);
            Assert.True(response.IsError);
        }

        [Fact]
        public async Task VerifySecurityAnswers_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userSecurityAnswer = new UserSecurityAnswer { EnterpriseUserName = "test@example.com" };

            _mockManageCredential
                .Setup(x => x.VerifySecurityAnswers(It.IsAny<UserSecurityAnswer>(), It.IsAny<UserDeviceDetails>()))
                .Throws(new Exception("Service error"));

            // Act
            var result = await _credentialController.VerifySecurityAnswers(userSecurityAnswer);

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region ForgotPassword Tests

        [Fact]
        public async Task ForgotPassword_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var changePassword = new ChangePassword { EnterpriseUserName = "test@example.com" };
            var expectedResponse = new ChangePasswordResponse { IsError = false, IsSuccess = true };

            _mockManageCredential
                .Setup(x => x.ForgotPassword(changePassword))
                .Returns(expectedResponse);

            // Act
            var result = await _credentialController.ForgotPassword(changePassword);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ChangePasswordResponse>(okResult.Value);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task ForgotPassword_WithNullData_ReturnsBadRequest()
        {
            // Act
            var result = await _credentialController.ForgotPassword(null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("changePassword is required", badRequestResult.Value);
        }

        [Fact]
        public async Task ForgotPassword_WhenServiceReturnsError_ReturnsOkWithError()
        {
            // Arrange
            var changePassword = new ChangePassword { EnterpriseUserName = "test@example.com" };
            var expectedResponse = new ChangePasswordResponse { IsError = true, ErrorReason = "User not found" };

            _mockManageCredential
                .Setup(x => x.ForgotPassword(changePassword))
                .Returns(expectedResponse);

            // Act
            var result = await _credentialController.ForgotPassword(changePassword);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ChangePasswordResponse>(okResult.Value);
            Assert.True(response.IsError);
        }

        [Fact]
        public async Task ForgotPassword_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var changePassword = new ChangePassword { EnterpriseUserName = "test@example.com" };

            _mockManageCredential
                .Setup(x => x.ForgotPassword(It.IsAny<ChangePassword>()))
                .Throws(new Exception("Service error"));

            // Act
            var result = await _credentialController.ForgotPassword(changePassword);

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region ValidatePassword Tests

        [Fact]
        public async Task ValidatePassword_WithValidData_ReturnsOkResult()
        {
            // Arrange
            const string enterpriseUserName = "test@example.com";
            const string passwordToValidate = "StrongPass123!";
            var expectedResponse = new ValidatePasswordResponse { IsError = false };

            _mockManageCredential
                .Setup(x => x.ValidatePasswordForUser(enterpriseUserName, passwordToValidate))
                .Returns(expectedResponse);

            // Act
            var result = await _credentialController.ValidatePassword(enterpriseUserName, passwordToValidate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Theory]
        [InlineData(null, "password")]
        [InlineData("", "password")]
        [InlineData("   ", "password")]
        [InlineData("user@example.com", null)]
        [InlineData("user@example.com", "")]
        [InlineData("user@example.com", "   ")]
        public async Task ValidatePassword_WithMissingParameters_ReturnsBadRequest(string enterpriseUserName, string passwordToValidate)
        {
            // Act
            var result = await _credentialController.ValidatePassword(enterpriseUserName, passwordToValidate);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("enterpriseUserName and passwordToValidate are required", badRequestResult.Value);
        }

        [Fact]
        public async Task ValidatePassword_WhenServiceReturnsError_ReturnsOkWithError()
        {
            // Arrange
            const string enterpriseUserName = "test@example.com";
            const string passwordToValidate = "weak";
            var expectedResponse = new ValidatePasswordResponse
            {
                IsError = true,
                ErrorReason = "Password too weak"
            };

            _mockManageCredential
                .Setup(x => x.ValidatePasswordForUser(enterpriseUserName, passwordToValidate))
                .Returns(expectedResponse);

            // Act
            var result = await _credentialController.ValidatePassword(enterpriseUserName, passwordToValidate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ValidatePasswordResponse>(okResult.Value);
            Assert.True(response.IsError);
        }

        [Fact]
        public async Task ValidatePassword_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            _mockManageCredential
                .Setup(x => x.ValidatePasswordForUser(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Service error"));

            // Act
            var result = await _credentialController.ValidatePassword("user@example.com", "password");

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region CheckPasswordExpiration Tests

        [Fact]
        public async Task CheckPasswordExpiration_WithValidClaims_ReturnsOkResult()
        {
            // Arrange
            var userClaim = new DefaultUserClaim
            {
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 1000,
                UserId = 123
            };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var expectedResponse = new CheckPasswordExpirationResponse { IsError = false };
            _mockManageCredential
                .Setup(x => x.CheckPasswordExpiration(userClaim.UserId, userClaim.UserRealPageGuid))
                .Returns(expectedResponse);
            _mockUserLoginRepository
                .Setup(x => x.GetPrimaryOrgWithoutStatusByUserId(123))
                .Returns(new OrganizationStatus() { PartyId = 234 });

            _mockUserLoginRepository
                .Setup(x => x.GetPrimaryOrgIdByUserId(userClaim.UserId))
                .Returns(1000);

            // Act
            var result = await _credentialController.CheckPasswordExpiration();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task CheckPasswordExpiration_WithEmptyRealPageId_ReturnsBadRequest()
        {
            // Arrange
            var userClaim = new DefaultUserClaim
            {
                UserRealPageGuid = Guid.Empty,
                OrganizationPartyId = 1000
            };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            // Act
            var result = await _credentialController.CheckPasswordExpiration();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Unable to get realPageId for user from claims.", badRequestResult.Value);
        }

        [Fact]
        public async Task CheckPasswordExpiration_WithZeroOrgPartyId_ReturnsBadRequest()
        {
            // Arrange
            var userClaim = new DefaultUserClaim
            {
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 0
            };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            // Act
            var result = await _credentialController.CheckPasswordExpiration();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Unable to get organization Id for user from claims.", badRequestResult.Value);
        }

        [Fact]
        public async Task CheckPasswordExpiration_WithNullClaims_ReturnsBadRequest()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            // Act
            var result = await _credentialController.CheckPasswordExpiration();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Unable to get realPageId for user from claims.", badRequestResult.Value);
        }

        [Fact]
        public async Task CheckPasswordExpiration_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userClaim = new DefaultUserClaim
            {
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 1000,
                UserId = 123
            };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            _mockManageCredential
                .Setup(x => x.CheckPasswordExpiration(It.IsAny<long>(), It.IsAny<Guid>()))
                .Throws(new Exception("Service error"));

            // Act
            var result = await _credentialController.CheckPasswordExpiration();

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region UserAllSecurityQuestions Tests

        [Fact]
        public async Task UserAllSecurityQuestions_WithValidUserName_ReturnsOkResult()
        {
            // Arrange
            const string enterpriseUserName = "test@example.com";
            var expectedResponse = new UserAllSecurityQuestionResponse { IsError = false };

            _mockManageCredential
                .Setup(x => x.UserAllSecurityQuestions(enterpriseUserName))
                .Returns(expectedResponse);

            // Act
            var result = await _credentialController.UserAllSecurityQuestions(enterpriseUserName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UserAllSecurityQuestions_WithEmptyUserName_ReturnsBadRequest(string enterpriseUserName)
        {
            // Act
            var result = await _credentialController.UserAllSecurityQuestions(enterpriseUserName);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("enterpriseUserName is required", badRequestResult.Value);
        }

        [Fact]
        public async Task UserAllSecurityQuestions_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            _mockManageCredential
                .Setup(x => x.UserAllSecurityQuestions(It.IsAny<string>()))
                .Throws(new Exception("Service error"));

            // Act
            var result = await _credentialController.UserAllSecurityQuestions("test@example.com");

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region GetUser Tests

        [Fact]
        public async Task GetUser_WithValidUserName_ReturnsOkResult()
        {
            // Arrange
            const string enterpriseUserName = "test@example.com";
            var userLoginOnly = new UserLoginOnly
            {
                UserId = 123,
                PartyId = 456,
                RealPageId = Guid.NewGuid(),
                LoginName = enterpriseUserName
            };
            var expectedResponse = new ListResponse
            {
                IsError = false,
                Records = new List<object> { userLoginOnly }
            };

            _mockManageCredential
                .Setup(x => x.GetUser(enterpriseUserName))
                .Returns(expectedResponse);

            _mockUserLoginRepository
                .Setup(x => x.GetPrimaryOrgIdByUserId(It.IsAny<long>()))
                .Returns(1000);

            // Act
            var result = await _credentialController.GetUser(enterpriseUserName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetUser_WithEmptyUserName_ReturnsBadRequest(string enterpriseUserName)
        {
            // Act
            var result = await _credentialController.GetUser(enterpriseUserName);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("enterpriseUserName is required", badRequestResult.Value);
        }

        [Fact]
        public async Task GetUser_WhenServiceReturnsError_ReturnsOkWithError()
        {
            // Arrange
            const string enterpriseUserName = "test@example.com";
            var expectedResponse = new ListResponse
            {
                IsError = true,
                ErrorReason = "User not found"
            };

            _mockManageCredential
                .Setup(x => x.GetUser(enterpriseUserName))
                .Returns(expectedResponse);

            // Act
            var result = await _credentialController.GetUser(enterpriseUserName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ListResponse>(okResult.Value);
            Assert.True(response.IsError);
        }

        [Fact]
        public async Task GetUser_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            _mockManageCredential
                .Setup(x => x.GetUser(It.IsAny<string>()))
                .Throws(new Exception("Service error"));

            // Act
            var result = await _credentialController.GetUser("test@example.com");

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region SetPassword Tests

        [Fact]
        public async Task SetPassword_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var setPassword = new SetPassword
            {
                EnterpriseUserName = "test@example.com",
                NewPassword = "NewStrongPass123!"
            };
            var expectedResponse = new ChangePasswordResponse { IsError = false, IsSuccess = true };

            _mockManageCredential
                .Setup(x => x.SetPassword(setPassword))
                .Returns(expectedResponse);

            // Act
            var result = await _credentialController.SetPassword(setPassword);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task SetPassword_WithNullData_ReturnsBadRequest()
        {
            // Act
            var result = await _credentialController.SetPassword(null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("setPassword is required", badRequestResult.Value);
        }

        [Theory]
        [InlineData(null, "password")]
        [InlineData("", "password")]
        [InlineData("   ", "password")]
        [InlineData("user@example.com", null)]
        [InlineData("user@example.com", "")]
        [InlineData("user@example.com", "   ")]
        public async Task SetPassword_WithMissingFields_ReturnsBadRequest(string enterpriseUserName, string newPassword)
        {
            // Arrange
            var setPassword = new SetPassword
            {
                EnterpriseUserName = enterpriseUserName,
                NewPassword = newPassword
            };

            // Act
            var result = await _credentialController.SetPassword(setPassword);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("enterpriseUserName and newPassword are required", badRequestResult.Value);
        }

        [Fact]
        public async Task SetPassword_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var setPassword = new SetPassword
            {
                EnterpriseUserName = "test@example.com",
                NewPassword = "Password123!"
            };

            _mockManageCredential
                .Setup(x => x.SetPassword(It.IsAny<SetPassword>()))
                .Throws(new Exception("Service error"));

            // Act
            var result = await _credentialController.SetPassword(setPassword);

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region SetUserSecurityQuestions Tests

        [Fact]
        public async Task SetUserSecurityQuestions_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var userSecurityAnswer = new UserSecurityAnswer
            {
                EnterpriseUserName = "test@example.com",
                SecurityQuestionAnswers = new List<SecurityQuestionAnswer>()
            };
            var expectedResponse = new SetUserSecurityQuestionsResponse { IsError = false };

            _mockManageCredential
                .Setup(x => x.SetUserSecurityQuestions(userSecurityAnswer))
                .Returns(expectedResponse);

            // Act
            var result = await _credentialController.SetUserSecurityQuestions(userSecurityAnswer);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task SetUserSecurityQuestions_WithNullData_ReturnsBadRequest()
        {
            // Act
            var result = await _credentialController.SetUserSecurityQuestions(null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("userSecurityQuestionsAnswers is required", badRequestResult.Value);
        }

        [Fact]
        public async Task SetUserSecurityQuestions_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userSecurityAnswer = new UserSecurityAnswer { EnterpriseUserName = "test@example.com" };

            _mockManageCredential
                .Setup(x => x.SetUserSecurityQuestions(It.IsAny<UserSecurityAnswer>()))
                .Throws(new Exception("Service error"));

            // Act
            var result = await _credentialController.SetUserSecurityQuestions(userSecurityAnswer);

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region ResetPassword Tests

        [Fact]
        public async Task ResetPassword_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var userResetPassword = new UserResetPassword
            {
                RealPageId = realPageId,
                NewPassword = "NewPass123!",
                OldPassword = "OldPass123!"
            };
            var expectedResponse = new ResetPasswordResponse { IsError = false };

            _mockManageCredential
                .Setup(x => x.ResetPassword(realPageId, userResetPassword))
                .Returns(expectedResponse);

            // Act
            var result = await _credentialController.ResetPassword(userResetPassword, realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task ResetPassword_WithNullUserResetPassword_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            // Act
            var result = await _credentialController.ResetPassword(null!, realPageId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: userResetPassword", badRequestResult.Value);
        }

        [Fact]
        public async Task ResetPassword_WithEmptyRealPageId_ReturnsBadRequest()
        {
            // Arrange
            var userResetPassword = new UserResetPassword
            {
                RealPageId = Guid.Empty,
                NewPassword = "NewPass123!"
            };
            var userClaim = new DefaultUserClaim { UserRealPageGuid = Guid.Empty };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            // Act
            var result = await _credentialController.ResetPassword(userResetPassword, null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        [Fact]
        public async Task ResetPassword_UsesUserClaimsWhenRealPageIdNotProvided()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            var userClaim = new DefaultUserClaim { UserRealPageGuid = userRealPageId, OrganizationPartyId = 1000 };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var userResetPassword = new UserResetPassword
            {
                RealPageId = null,
                NewPassword = "NewPass123!"
            };
            var expectedResponse = new ResetPasswordResponse { IsError = false };

            _mockManageCredential
                .Setup(x => x.ResetPassword(userRealPageId, userResetPassword))
                .Returns(expectedResponse);

            // Act
            var result = await _credentialController.ResetPassword(userResetPassword);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockManageCredential.Verify(x => x.ResetPassword(userRealPageId, userResetPassword), Times.Once);
        }

        [Fact]
        public async Task ResetPassword_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var userResetPassword = new UserResetPassword { RealPageId = realPageId, NewPassword = "Pass123!" };

            _mockManageCredential
                .Setup(x => x.ResetPassword(It.IsAny<Guid>(), It.IsAny<UserResetPassword>()))
                .Throws(new Exception("Service error"));

            // Act
            var result = await _credentialController.ResetPassword(userResetPassword, realPageId);

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region SetTemporaryPassword Tests

        [Fact]
        public async Task SetTemporaryPassword_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var userResetPassword = new UserResetPassword
            {
                RealPageId = realPageId,
                NewPassword = "TempPass123!"
            };

            // Note: SetTemporaryPassword creates its own ManageCredential instance,
            // so we need to test the controller flow, not the mock

            // Act
            var result = await _credentialController.SetTemporaryPassword(userResetPassword);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task SetTemporaryPassword_WithNullData_ReturnsBadRequest()
        {
            // Act
            var result = await _credentialController.SetTemporaryPassword(null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: userResetPassword", badRequestResult.Value);
        }

        [Fact]
        public async Task SetTemporaryPassword_WithEmptyRealPageId_ReturnsBadRequest()
        {
            // Arrange
            var userResetPassword = new UserResetPassword
            {
                RealPageId = Guid.Empty,
                NewPassword = "TempPass123!"
            };

            // Act
            var result = await _credentialController.SetTemporaryPassword(userResetPassword);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        [Fact]
        public async Task SetTemporaryPassword_WithNullRealPageId_ReturnsBadRequest()
        {
            // Arrange
            var userResetPassword = new UserResetPassword
            {
                RealPageId = null,
                NewPassword = "TempPass123!"
            };

            // Act
            var result = await _credentialController.SetTemporaryPassword(userResetPassword);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        #endregion

        #region GetUserSelectedSecurityQuestions Tests

        [Fact]
        public async Task GetUserSelectedSecurityQuestions_WithValidRealPageId_ReturnsOkResult()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var expectedResponse = new UsersAllSecurityQuestionResponse
            {
                IsError = false,
                SecurityQuestions = new List<SecurityQuestion>()
            };

            _mockManageCredential
                .Setup(x => x.GetUserSelectedSecurityQuestions(realPageId))
                .Returns(expectedResponse);

            // Act
            var result = await _credentialController.GetUserSelectedSecurityQuestions(realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetUserSelectedSecurityQuestions_WithEmptyRealPageId_UsesUserClaims()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            var userClaim = new DefaultUserClaim { UserRealPageGuid = userRealPageId };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var expectedResponse = new UsersAllSecurityQuestionResponse { IsError = false };
            _mockManageCredential
                .Setup(x => x.GetUserSelectedSecurityQuestions(userRealPageId))
                .Returns(expectedResponse);

            // Act
            var result = await _credentialController.GetUserSelectedSecurityQuestions(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockManageCredential.Verify(x => x.GetUserSelectedSecurityQuestions(userRealPageId), Times.Once);
        }

        [Fact]
        public async Task GetUserSelectedSecurityQuestions_WithNoValidRealPageId_ReturnsBadRequest()
        {
            // Arrange
            var userClaim = new DefaultUserClaim { UserRealPageGuid = Guid.Empty };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            // Act
            var result = await _credentialController.GetUserSelectedSecurityQuestions(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<SecurityQuestion, IErrorData>>(badRequestResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("Credential.GetSecurityQuestions.1", output.Status.ErrorCode);
        }

        [Fact]
        public async Task GetUserSelectedSecurityQuestions_WhenServiceReturnsError_ReturnsOkWithError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var expectedResponse = new UsersAllSecurityQuestionResponse
            {
                IsError = true,
                ErrorReason = "User not found"
            };

            _mockManageCredential
                .Setup(x => x.GetUserSelectedSecurityQuestions(realPageId))
                .Returns(expectedResponse);

            // Act
            var result = await _credentialController.GetUserSelectedSecurityQuestions(realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<SecurityQuestion, IErrorData>>(okResult.Value);
            Assert.Equal("Credential.GetSecurityQuestions.2", output.Status.ErrorCode);
        }

        [Fact]
        public async Task GetUserSelectedSecurityQuestions_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            _mockManageCredential
                .Setup(x => x.GetUserSelectedSecurityQuestions(It.IsAny<Guid>()))
                .Throws(new Exception("Service error"));

            // Act
            var result = await _credentialController.GetUserSelectedSecurityQuestions(realPageId);

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region SaveUserSelectedSecurityQuestions Tests

        [Fact]
        public async Task SaveUserSelectedSecurityQuestions_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var securityQuestionAnswers = new List<SecurityQuestionAnswer>
            {
                new SecurityQuestionAnswer { SecurityQuestionId = 1, Answer = "Answer1" }
            };
            var expectedResponse = new SaveUserSelectedSecurityQuestionResponse { IsError = false };

            _mockManageCredential
                .Setup(x => x.SaveUserSelectedSecurityQuestions(realPageId, securityQuestionAnswers))
                .Returns(expectedResponse);

            // Act
            var result = await _credentialController.SaveUserSelectedSecurityQuestions(securityQuestionAnswers, realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task SaveUserSelectedSecurityQuestions_WithEmptyRealPageId_UsesUserClaims()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            var userClaim = new DefaultUserClaim { UserRealPageGuid = userRealPageId };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var securityQuestionAnswers = new List<SecurityQuestionAnswer>();
            var expectedResponse = new SaveUserSelectedSecurityQuestionResponse { IsError = false };

            _mockManageCredential
                .Setup(x => x.SaveUserSelectedSecurityQuestions(userRealPageId, securityQuestionAnswers))
                .Returns(expectedResponse);

            // Act
            var result = await _credentialController.SaveUserSelectedSecurityQuestions(securityQuestionAnswers, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockManageCredential.Verify(x => x.SaveUserSelectedSecurityQuestions(userRealPageId, securityQuestionAnswers), Times.Once);
        }

        [Fact]
        public async Task SaveUserSelectedSecurityQuestions_WithNoValidRealPageId_ReturnsBadRequest()
        {
            // Arrange
            var userClaim = new DefaultUserClaim { UserRealPageGuid = Guid.Empty };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var securityQuestionAnswers = new List<SecurityQuestionAnswer>();

            // Act
            var result = await _credentialController.SaveUserSelectedSecurityQuestions(securityQuestionAnswers, null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        [Fact]
        public async Task SaveUserSelectedSecurityQuestions_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var securityQuestionAnswers = new List<SecurityQuestionAnswer>();

            _mockManageCredential
                .Setup(x => x.SaveUserSelectedSecurityQuestions(It.IsAny<Guid>(), It.IsAny<IList<SecurityQuestionAnswer>>()))
                .Throws(new Exception("Service error"));

            // Act
            var result = await _credentialController.SaveUserSelectedSecurityQuestions(securityQuestionAnswers, realPageId);

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _credentialController = null!;
            base.Dispose();
        }

        #endregion
    }
}




