using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
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

        private readonly Mock<IManageCredentialAsync> _mockManageCredential;
        private readonly Mock<IUserLoginRepositoryAsync> _mockUserLoginRepository;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private CredentialController _credentialController;

        #endregion

        #region Constructor

        public CredentialControllerTests()
        {
            _mockManageCredential = new Mock<IManageCredentialAsync>();
            _mockUserLoginRepository = new Mock<IUserLoginRepositoryAsync>();
            _mockUserClaimsAccessor = MockUserClaimsAccessor;

            _credentialController = new CredentialController(
                _mockManageCredential.Object,
                _mockUserLoginRepository.Object,
                _mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new CredentialController(
                _mockManageCredential.Object,
                _mockUserLoginRepository.Object,
                _mockUserClaimsAccessor.Object);

            Assert.NotNull(controller);
        }

        #endregion

        #region GetSecurityQuestions Tests

        [Fact]
        public async Task GetSecurityQuestions_WithValidUserName_ReturnsOkResult()
        {
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
                .Setup(x => x.GetSecurityQuestionAsync(enterpriseUserName, It.IsAny<UserDeviceDetails>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.GetSecurityQuestions(enterpriseUserName);

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
            var result = await _credentialController.GetSecurityQuestions(enterpriseUserName);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("enterpriseUserName is required", badRequestResult.Value);
        }

        [Fact]
        public async Task GetSecurityQuestions_WhenServiceReturnsError_ReturnsOkWithError()
        {
            const string enterpriseUserName = "testuser@example.com";
            var expectedResponse = new SecurityQuestionResponse { IsError = true, ErrorReason = "User not found" };

            _mockManageCredential
                .Setup(x => x.GetSecurityQuestionAsync(enterpriseUserName, It.IsAny<UserDeviceDetails>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.GetSecurityQuestions(enterpriseUserName);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<SecurityQuestionResponse>(okResult.Value);
            Assert.True(response.IsError);
        }

        [Fact]
        public async Task GetSecurityQuestions_WhenExceptionThrown_ReturnsInternalServerError()
        {
            const string enterpriseUserName = "testuser@example.com";

            _mockManageCredential
                .Setup(x => x.GetSecurityQuestionAsync(It.IsAny<string>(), It.IsAny<UserDeviceDetails>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var result = await _credentialController.GetSecurityQuestions(enterpriseUserName);

            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetSecurityQuestions_TrimsUserName_CallsServiceWithTrimmedValue()
        {
            const string enterpriseUserName = "  testuser@example.com  ";
            var expectedResponse = new SecurityQuestionResponse { IsError = false };

            _mockManageCredential
                .Setup(x => x.GetSecurityQuestionAsync("testuser@example.com", It.IsAny<UserDeviceDetails>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            await _credentialController.GetSecurityQuestions(enterpriseUserName);

            _mockManageCredential.Verify(
                x => x.GetSecurityQuestionAsync("testuser@example.com", It.IsAny<UserDeviceDetails>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region VerifySecurityAnswers Tests

        [Fact]
        public async Task VerifySecurityAnswers_WithValidData_ReturnsOkResult()
        {
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
                .Setup(x => x.VerifySecurityAnswersAsync(userSecurityAnswer, It.IsAny<UserDeviceDetails>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.VerifySecurityAnswers(userSecurityAnswer);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task VerifySecurityAnswers_WithNullData_ReturnsBadRequest()
        {
            var result = await _credentialController.VerifySecurityAnswers(null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("userSecurityAnswer is required", badRequestResult.Value);
        }

        [Fact]
        public async Task VerifySecurityAnswers_WhenServiceReturnsError_ReturnsOkWithError()
        {
            var userSecurityAnswer = new UserSecurityAnswer { EnterpriseUserName = "test@example.com" };
            var expectedResponse = new SecurityAnswerResponse { IsError = true, ErrorReason = "Incorrect answers" };

            _mockManageCredential
                .Setup(x => x.VerifySecurityAnswersAsync(userSecurityAnswer, It.IsAny<UserDeviceDetails>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.VerifySecurityAnswers(userSecurityAnswer);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<SecurityAnswerResponse>(okResult.Value);
            Assert.True(response.IsError);
        }

        [Fact]
        public async Task VerifySecurityAnswers_WhenExceptionThrown_ReturnsInternalServerError()
        {
            var userSecurityAnswer = new UserSecurityAnswer { EnterpriseUserName = "test@example.com" };

            _mockManageCredential
                .Setup(x => x.VerifySecurityAnswersAsync(It.IsAny<UserSecurityAnswer>(), It.IsAny<UserDeviceDetails>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Service error"));

            var result = await _credentialController.VerifySecurityAnswers(userSecurityAnswer);

            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region ForgotPassword Tests

        [Fact]
        public async Task ForgotPassword_WithValidData_ReturnsOkResult()
        {
            var changePassword = new ChangePassword { EnterpriseUserName = "test@example.com" };
            var expectedResponse = new ChangePasswordResponse { IsError = false, IsSuccess = true };

            _mockManageCredential
                .Setup(x => x.ForgotPasswordAsync(changePassword, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.ForgotPassword(changePassword);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ChangePasswordResponse>(okResult.Value);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task ForgotPassword_WithNullData_ReturnsBadRequest()
        {
            var result = await _credentialController.ForgotPassword(null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("changePassword is required", badRequestResult.Value);
        }

        [Fact]
        public async Task ForgotPassword_WhenServiceReturnsError_ReturnsOkWithError()
        {
            var changePassword = new ChangePassword { EnterpriseUserName = "test@example.com" };
            var expectedResponse = new ChangePasswordResponse { IsError = true, ErrorReason = "User not found" };

            _mockManageCredential
                .Setup(x => x.ForgotPasswordAsync(changePassword, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.ForgotPassword(changePassword);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ChangePasswordResponse>(okResult.Value);
            Assert.True(response.IsError);
        }

        [Fact]
        public async Task ForgotPassword_WhenExceptionThrown_ReturnsInternalServerError()
        {
            var changePassword = new ChangePassword { EnterpriseUserName = "test@example.com" };

            _mockManageCredential
                .Setup(x => x.ForgotPasswordAsync(It.IsAny<ChangePassword>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Service error"));

            var result = await _credentialController.ForgotPassword(changePassword);

            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region ValidatePassword Tests

        [Fact]
        public async Task ValidatePassword_WithValidData_ReturnsOkResult()
        {
            const string enterpriseUserName = "test@example.com";
            const string passwordToValidate = "StrongPass123!";
            var expectedResponse = new ValidatePasswordResponse { IsError = false };

            _mockManageCredential
                .Setup(x => x.ValidatePasswordForUserAsync(enterpriseUserName, passwordToValidate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.ValidatePassword(enterpriseUserName, passwordToValidate);

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
            var result = await _credentialController.ValidatePassword(enterpriseUserName, passwordToValidate);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("enterpriseUserName and passwordToValidate are required", badRequestResult.Value);
        }

        [Fact]
        public async Task ValidatePassword_WhenServiceReturnsError_ReturnsOkWithError()
        {
            const string enterpriseUserName = "test@example.com";
            const string passwordToValidate = "weak";
            var expectedResponse = new ValidatePasswordResponse { IsError = true, ErrorReason = "Password too weak" };

            _mockManageCredential
                .Setup(x => x.ValidatePasswordForUserAsync(enterpriseUserName, passwordToValidate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.ValidatePassword(enterpriseUserName, passwordToValidate);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ValidatePasswordResponse>(okResult.Value);
            Assert.True(response.IsError);
        }

        [Fact]
        public async Task ValidatePassword_WhenExceptionThrown_ReturnsInternalServerError()
        {
            _mockManageCredential
                .Setup(x => x.ValidatePasswordForUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Service error"));

            var result = await _credentialController.ValidatePassword("user@example.com", "password");

            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region CheckPasswordExpiration Tests

        [Fact]
        public async Task CheckPasswordExpiration_WithValidClaims_ReturnsOkResult()
        {
            var userClaim = new DefaultUserClaim
            {
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 1000,
                UserId = 123
            };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var expectedResponse = new CheckPasswordExpirationResponse { IsError = false };
            _mockManageCredential
                .Setup(x => x.CheckPasswordExpirationAsync(userClaim.UserId, userClaim.UserRealPageGuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.CheckPasswordExpiration();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task CheckPasswordExpiration_WithEmptyRealPageId_ReturnsBadRequest()
        {
            var userClaim = new DefaultUserClaim { UserRealPageGuid = Guid.Empty, OrganizationPartyId = 1000 };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var result = await _credentialController.CheckPasswordExpiration();

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Unable to get realPageId for user from claims.", badRequestResult.Value);
        }

        [Fact]
        public async Task CheckPasswordExpiration_WithZeroOrgPartyId_ReturnsBadRequest()
        {
            var userClaim = new DefaultUserClaim { UserRealPageGuid = Guid.NewGuid(), OrganizationPartyId = 0 };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var result = await _credentialController.CheckPasswordExpiration();

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Unable to get organization Id for user from claims.", badRequestResult.Value);
        }

        [Fact]
        public async Task CheckPasswordExpiration_WithNullClaims_ReturnsBadRequest()
        {
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var result = await _credentialController.CheckPasswordExpiration();

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Unable to get realPageId for user from claims.", badRequestResult.Value);
        }

        [Fact]
        public async Task CheckPasswordExpiration_WhenExceptionThrown_ReturnsInternalServerError()
        {
            var userClaim = new DefaultUserClaim
            {
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 1000,
                UserId = 123
            };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            _mockManageCredential
                .Setup(x => x.CheckPasswordExpirationAsync(It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Service error"));

            var result = await _credentialController.CheckPasswordExpiration();

            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region UserAllSecurityQuestions Tests

        [Fact]
        public async Task UserAllSecurityQuestions_WithValidUserName_ReturnsOkResult()
        {
            const string enterpriseUserName = "test@example.com";
            var expectedResponse = new UserAllSecurityQuestionResponse { IsError = false };

            _mockManageCredential
                .Setup(x => x.UserAllSecurityQuestionsAsync(enterpriseUserName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.UserAllSecurityQuestions(enterpriseUserName);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UserAllSecurityQuestions_WithEmptyUserName_ReturnsBadRequest(string enterpriseUserName)
        {
            var result = await _credentialController.UserAllSecurityQuestions(enterpriseUserName);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("enterpriseUserName is required", badRequestResult.Value);
        }

        [Fact]
        public async Task UserAllSecurityQuestions_WhenExceptionThrown_ReturnsInternalServerError()
        {
            _mockManageCredential
                .Setup(x => x.UserAllSecurityQuestionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Service error"));

            var result = await _credentialController.UserAllSecurityQuestions("test@example.com");

            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region GetUser Tests

        [Fact]
        public async Task GetUser_WithValidUserName_ReturnsOkResult()
        {
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
                .Setup(x => x.GetUserAsync(enterpriseUserName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            _mockUserLoginRepository
                .Setup(x => x.GetPrimaryOrgIdByUserIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1000L);

            var result = await _credentialController.GetUser(enterpriseUserName);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetUser_WithEmptyUserName_ReturnsBadRequest(string enterpriseUserName)
        {
            var result = await _credentialController.GetUser(enterpriseUserName);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("enterpriseUserName is required", badRequestResult.Value);
        }

        [Fact]
        public async Task GetUser_WhenServiceReturnsError_ReturnsOkWithError()
        {
            const string enterpriseUserName = "test@example.com";
            var expectedResponse = new ListResponse { IsError = true, ErrorReason = "User not found" };

            _mockManageCredential
                .Setup(x => x.GetUserAsync(enterpriseUserName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.GetUser(enterpriseUserName);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ListResponse>(okResult.Value);
            Assert.True(response.IsError);
        }

        [Fact]
        public async Task GetUser_WhenExceptionThrown_ReturnsInternalServerError()
        {
            _mockManageCredential
                .Setup(x => x.GetUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Service error"));

            var result = await _credentialController.GetUser("test@example.com");

            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region SetPassword Tests

        [Fact]
        public async Task SetPassword_WithValidData_ReturnsOkResult()
        {
            var setPassword = new SetPassword { EnterpriseUserName = "test@example.com", NewPassword = "NewStrongPass123!" };
            var expectedResponse = new ChangePasswordResponse { IsError = false, IsSuccess = true };

            _mockManageCredential
                .Setup(x => x.SetPasswordAsync(setPassword, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.SetPassword(setPassword);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task SetPassword_WithNullData_ReturnsBadRequest()
        {
            var result = await _credentialController.SetPassword(null!);

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
            var setPassword = new SetPassword { EnterpriseUserName = enterpriseUserName, NewPassword = newPassword };

            var result = await _credentialController.SetPassword(setPassword);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("enterpriseUserName and newPassword are required", badRequestResult.Value);
        }

        [Fact]
        public async Task SetPassword_WhenExceptionThrown_ReturnsInternalServerError()
        {
            var setPassword = new SetPassword { EnterpriseUserName = "test@example.com", NewPassword = "Password123!" };

            _mockManageCredential
                .Setup(x => x.SetPasswordAsync(It.IsAny<SetPassword>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Service error"));

            var result = await _credentialController.SetPassword(setPassword);

            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region SetUserSecurityQuestions Tests

        [Fact]
        public async Task SetUserSecurityQuestions_WithValidData_ReturnsOkResult()
        {
            var userSecurityAnswer = new UserSecurityAnswer
            {
                EnterpriseUserName = "test@example.com",
                SecurityQuestionAnswers = new List<SecurityQuestionAnswer>()
            };
            var expectedResponse = new SetUserSecurityQuestionsResponse { IsError = false };

            _mockManageCredential
                .Setup(x => x.SetUserSecurityQuestionsAsync(userSecurityAnswer, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.SetUserSecurityQuestions(userSecurityAnswer);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task SetUserSecurityQuestions_WithNullData_ReturnsBadRequest()
        {
            var result = await _credentialController.SetUserSecurityQuestions(null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("userSecurityQuestionsAnswers is required", badRequestResult.Value);
        }

        [Fact]
        public async Task SetUserSecurityQuestions_WhenExceptionThrown_ReturnsInternalServerError()
        {
            var userSecurityAnswer = new UserSecurityAnswer { EnterpriseUserName = "test@example.com" };

            _mockManageCredential
                .Setup(x => x.SetUserSecurityQuestionsAsync(It.IsAny<UserSecurityAnswer>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Service error"));

            var result = await _credentialController.SetUserSecurityQuestions(userSecurityAnswer);

            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region ResetPassword Tests

        [Fact]
        public async Task ResetPassword_WithValidData_ReturnsOkResult()
        {
            var realPageId = Guid.NewGuid();
            var userResetPassword = new UserResetPassword
            {
                RealPageId = realPageId,
                NewPassword = "NewPass123!",
                OldPassword = "OldPass123!"
            };
            var expectedResponse = new ResetPasswordResponse { IsError = false };

            _mockManageCredential
                .Setup(x => x.ResetPasswordAsync(realPageId, userResetPassword, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.ResetPassword(userResetPassword, realPageId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task ResetPassword_WithNullUserResetPassword_ReturnsBadRequest()
        {
            var realPageId = Guid.NewGuid();

            var result = await _credentialController.ResetPassword(null!, realPageId);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: userResetPassword", badRequestResult.Value);
        }

        [Fact]
        public async Task ResetPassword_WithEmptyRealPageId_ReturnsBadRequest()
        {
            var userResetPassword = new UserResetPassword { RealPageId = Guid.Empty, NewPassword = "NewPass123!" };
            var userClaim = new DefaultUserClaim { UserRealPageGuid = Guid.Empty };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var result = await _credentialController.ResetPassword(userResetPassword, null);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        [Fact]
        public async Task ResetPassword_UsesUserClaimsWhenRealPageIdNotProvided()
        {
            var userRealPageId = Guid.NewGuid();
            var userClaim = new DefaultUserClaim { UserRealPageGuid = userRealPageId, OrganizationPartyId = 1000 };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var userResetPassword = new UserResetPassword { RealPageId = null, NewPassword = "NewPass123!" };
            var expectedResponse = new ResetPasswordResponse { IsError = false };

            _mockManageCredential
                .Setup(x => x.ResetPasswordAsync(userRealPageId, userResetPassword, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.ResetPassword(userResetPassword);

            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockManageCredential.Verify(x => x.ResetPasswordAsync(userRealPageId, userResetPassword, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ResetPassword_WhenExceptionThrown_ReturnsInternalServerError()
        {
            var realPageId = Guid.NewGuid();
            var userResetPassword = new UserResetPassword { RealPageId = realPageId, NewPassword = "Pass123!" };

            _mockManageCredential
                .Setup(x => x.ResetPasswordAsync(It.IsAny<Guid>(), It.IsAny<UserResetPassword>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Service error"));

            var result = await _credentialController.ResetPassword(userResetPassword, realPageId);

            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region SetTemporaryPassword Tests

        [Fact]
        public async Task SetTemporaryPassword_WithValidData_ReturnsOkResult()
        {
            var realPageId = Guid.NewGuid();
            var userResetPassword = new UserResetPassword { RealPageId = realPageId, NewPassword = "TempPass123!" };
            var expectedResponse = new ResetPasswordResponse { IsError = false };

            _mockManageCredential
                .Setup(x => x.SetTemporaryPasswordAsync(realPageId, userResetPassword, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.SetTemporaryPassword(userResetPassword);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task SetTemporaryPassword_WithNullData_ReturnsBadRequest()
        {
            var result = await _credentialController.SetTemporaryPassword(null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: userResetPassword", badRequestResult.Value);
        }

        [Fact]
        public async Task SetTemporaryPassword_WithEmptyRealPageId_ReturnsBadRequest()
        {
            var userResetPassword = new UserResetPassword { RealPageId = Guid.Empty, NewPassword = "TempPass123!" };

            var result = await _credentialController.SetTemporaryPassword(userResetPassword);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        [Fact]
        public async Task SetTemporaryPassword_WithNullRealPageId_ReturnsBadRequest()
        {
            var userResetPassword = new UserResetPassword { RealPageId = null, NewPassword = "TempPass123!" };

            var result = await _credentialController.SetTemporaryPassword(userResetPassword);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        #endregion

        #region GetUserSelectedSecurityQuestions Tests

        [Fact]
        public async Task GetUserSelectedSecurityQuestions_WithValidRealPageId_ReturnsOkResult()
        {
            var realPageId = Guid.NewGuid();
            var expectedResponse = new UsersAllSecurityQuestionResponse
            {
                IsError = false,
                SecurityQuestions = new List<SecurityQuestion>()
            };

            _mockManageCredential
                .Setup(x => x.GetUserSelectedSecurityQuestionsAsync(realPageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.GetUserSelectedSecurityQuestions(realPageId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetUserSelectedSecurityQuestions_WithEmptyRealPageId_UsesUserClaims()
        {
            var userRealPageId = Guid.NewGuid();
            var userClaim = new DefaultUserClaim { UserRealPageGuid = userRealPageId };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var expectedResponse = new UsersAllSecurityQuestionResponse { IsError = false };
            _mockManageCredential
                .Setup(x => x.GetUserSelectedSecurityQuestionsAsync(userRealPageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.GetUserSelectedSecurityQuestions(null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockManageCredential.Verify(x => x.GetUserSelectedSecurityQuestionsAsync(userRealPageId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUserSelectedSecurityQuestions_WithNoValidRealPageId_ReturnsBadRequest()
        {
            var userClaim = new DefaultUserClaim { UserRealPageGuid = Guid.Empty };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var result = await _credentialController.GetUserSelectedSecurityQuestions(null);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<SecurityQuestion, IErrorData>>(badRequestResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("Credential.GetSecurityQuestions.1", output.Status.ErrorCode);
        }

        [Fact]
        public async Task GetUserSelectedSecurityQuestions_WhenServiceReturnsError_ReturnsOkWithError()
        {
            var realPageId = Guid.NewGuid();
            var expectedResponse = new UsersAllSecurityQuestionResponse { IsError = true, ErrorReason = "User not found" };

            _mockManageCredential
                .Setup(x => x.GetUserSelectedSecurityQuestionsAsync(realPageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.GetUserSelectedSecurityQuestions(realPageId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<SecurityQuestion, IErrorData>>(okResult.Value);
            Assert.Equal("Credential.GetSecurityQuestions.2", output.Status.ErrorCode);
        }

        [Fact]
        public async Task GetUserSelectedSecurityQuestions_WhenExceptionThrown_ReturnsInternalServerError()
        {
            var realPageId = Guid.NewGuid();

            _mockManageCredential
                .Setup(x => x.GetUserSelectedSecurityQuestionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Service error"));

            var result = await _credentialController.GetUserSelectedSecurityQuestions(realPageId);

            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        #endregion

        #region SaveUserSelectedSecurityQuestions Tests

        [Fact]
        public async Task SaveUserSelectedSecurityQuestions_WithValidData_ReturnsOkResult()
        {
            var realPageId = Guid.NewGuid();
            var securityQuestionAnswers = new List<SecurityQuestionAnswer>
            {
                new SecurityQuestionAnswer { SecurityQuestionId = 1, Answer = "Answer1" }
            };
            var expectedResponse = new SaveUserSelectedSecurityQuestionResponse { IsError = false };

            _mockManageCredential
                .Setup(x => x.SaveUserSelectedSecurityQuestionsAsync(realPageId, securityQuestionAnswers, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.SaveUserSelectedSecurityQuestions(securityQuestionAnswers, realPageId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task SaveUserSelectedSecurityQuestions_WithEmptyRealPageId_UsesUserClaims()
        {
            var userRealPageId = Guid.NewGuid();
            var userClaim = new DefaultUserClaim { UserRealPageGuid = userRealPageId };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var securityQuestionAnswers = new List<SecurityQuestionAnswer>();
            var expectedResponse = new SaveUserSelectedSecurityQuestionResponse { IsError = false };

            _mockManageCredential
                .Setup(x => x.SaveUserSelectedSecurityQuestionsAsync(userRealPageId, securityQuestionAnswers, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _credentialController.SaveUserSelectedSecurityQuestions(securityQuestionAnswers, null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockManageCredential.Verify(x => x.SaveUserSelectedSecurityQuestionsAsync(userRealPageId, securityQuestionAnswers, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SaveUserSelectedSecurityQuestions_WithNoValidRealPageId_ReturnsBadRequest()
        {
            var userClaim = new DefaultUserClaim { UserRealPageGuid = Guid.Empty };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var securityQuestionAnswers = new List<SecurityQuestionAnswer>();

            var result = await _credentialController.SaveUserSelectedSecurityQuestions(securityQuestionAnswers, null);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        [Fact]
        public async Task SaveUserSelectedSecurityQuestions_WhenExceptionThrown_ReturnsInternalServerError()
        {
            var realPageId = Guid.NewGuid();
            var securityQuestionAnswers = new List<SecurityQuestionAnswer>();

            _mockManageCredential
                .Setup(x => x.SaveUserSelectedSecurityQuestionsAsync(It.IsAny<Guid>(), It.IsAny<IList<SecurityQuestionAnswer>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Service error"));

            var result = await _credentialController.SaveUserSelectedSecurityQuestions(securityQuestionAnswers, realPageId);

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
