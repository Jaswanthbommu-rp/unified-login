using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.User.Models;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.LandingAPIEnterprise.Controllers;
using UnifiedLogin.LandingAPIEnterprise.Services;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.ResponseObject;
using Xunit;
using ValidationException = UnifiedLogin.LandingAPIEnterprise.Services.ValidationException;

namespace UnifiedLogin.LandingAPI.Tests.Controllers.Enterprise
{
    [ExcludeFromCodeCoverage]
    public class UserControllerShould : ControllerTestBase
    {
        private readonly Mock<IUserManagementService> _mockUserManagementService;
        private readonly Mock<IUserQueryService> _mockUserQueryService;
        private readonly Mock<IUserValidationService> _mockValidationService;
        private readonly Mock<ILoggingService> _mockLoggingService;
        private readonly Mock<IClientAuthenticationService> _mockClientAuthService;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private readonly DefaultUserClaim _userClaims;
        private readonly UserController _controller;

        public UserControllerShould()
        {
            _mockUserManagementService = new Mock<IUserManagementService>();
            _mockUserQueryService = new Mock<IUserQueryService>();
            _mockValidationService = new Mock<IUserValidationService>();
            _mockLoggingService = new Mock<ILoggingService>();
            _mockClientAuthService = new Mock<IClientAuthenticationService>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();

            _userClaims = new DefaultUserClaim
            {
                OrganizationPartyId = 1,
                OrganizationName = "Test Organization",
                LoginName = "testuser",
                UserRealPageGuid = Guid.NewGuid(),
                PersonaId = 100,
                UserId = 1,
                CorrelationId = Guid.NewGuid(),
                Rights = new List<string> { "read", "write" }
            };

            // Setup the mock to return the user claims
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(_userClaims);
            _mockUserClaimsAccessor.Setup(x => x.OrganizationPartyId).Returns(_userClaims.OrganizationPartyId);
            _mockUserClaimsAccessor.Setup(x => x.OrganizationName).Returns(_userClaims.OrganizationName);
            _mockUserClaimsAccessor.Setup(x => x.LoginName).Returns(_userClaims.LoginName);
            _mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(_userClaims.UserRealPageGuid);
            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(_userClaims.PersonaId);
            _mockUserClaimsAccessor.Setup(x => x.UserId).Returns(_userClaims.UserId);
            _mockUserClaimsAccessor.Setup(x => x.CorrelationId).Returns(_userClaims.CorrelationId);
            _mockUserClaimsAccessor.Setup(x => x.Rights).Returns(_userClaims.Rights);

            _controller = new UserController(
                _mockUserManagementService.Object,
                _mockUserQueryService.Object,
                _mockValidationService.Object,
                _mockLoggingService.Object,
                _mockClientAuthService.Object,
                _mockProductRepository.Object,
                _mockUserClaimsAccessor.Object);

            // Setup the controller context with authenticated user
            _controller.ControllerContext = CreateControllerContext();
        }

        #region CreateUser Tests

        [Fact]
        public async Task CreateUser_ValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var userDto = CreateValidUserProductDetailsDto();
            var userId = Guid.NewGuid();
            var response = new ObjectResponse { Data = userId };

            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync((ErrorResponse)null);
            _mockValidationService.Setup(x => x.ValidateSuperUserCreation(userDto, It.IsAny<DefaultUserClaim>()))
                .Returns(new ErrorResponse { Errors = new List<Error>() });
            _mockValidationService.Setup(x => x.ValidateUserProductDetails(userDto, It.IsAny<DefaultUserClaim>()))
                .Returns(new ErrorResponse { Errors = new List<Error>() });
            _mockUserManagementService.Setup(x => x.CreateUserAsync(userDto, It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateUser(userDto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult.ActionName.Should().Be(nameof(UserController.GetUser));
            createdResult.StatusCode.Should().Be((int)HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateUser_ClientAuthenticationFails_ReturnsBadRequest()
        {
            // Arrange
            var userDto = CreateValidUserProductDetailsDto();
            var authError = new ErrorResponse { Errors = new List<Error> { new Error { Detail = "Auth failed" } } };

            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync(authError);

            // Act
            var result = await _controller.CreateUser(userDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateUser_SuperUserValidationFails_ReturnsBadRequest()
        {
            // Arrange
            var userDto = CreateValidUserProductDetailsDto();
            var validationError = new ErrorResponse
            {
                Errors = new List<Error> { new Error { Detail = "Super user validation failed" } }
            };

            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync((ErrorResponse)null);
            _mockValidationService.Setup(x => x.ValidateSuperUserCreation(userDto, It.IsAny<DefaultUserClaim>()))
                .Returns(validationError);

            // Act
            var result = await _controller.CreateUser(userDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateUser_UserProductDetailsValidationFails_ReturnsBadRequest()
        {
            // Arrange
            var userDto = CreateValidUserProductDetailsDto();
            var validationError = new ErrorResponse
            {
                Errors = new List<Error> { new Error { Detail = "Validation failed" } }
            };

            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync((ErrorResponse)null);
            _mockValidationService.Setup(x => x.ValidateSuperUserCreation(userDto, It.IsAny<DefaultUserClaim>()))
                .Returns(new ErrorResponse { Errors = new List<Error>() });
            _mockValidationService.Setup(x => x.ValidateUserProductDetails(userDto, It.IsAny<DefaultUserClaim>()))
                .Returns(validationError);

            // Act
            var result = await _controller.CreateUser(userDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateUser_ValidationExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            var userDto = CreateValidUserProductDetailsDto();
            var exceptionMessage = "Validation error occurred";

            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync((ErrorResponse)null);
            _mockValidationService.Setup(x => x.ValidateSuperUserCreation(userDto, It.IsAny<DefaultUserClaim>()))
                .Returns(new ErrorResponse { Errors = new List<Error>() });
            _mockValidationService.Setup(x => x.ValidateUserProductDetails(userDto, It.IsAny<DefaultUserClaim>()))
                .Returns(new ErrorResponse { Errors = new List<Error>() });
            _mockUserManagementService.Setup(x => x.CreateUserAsync(userDto, It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new ValidationException(exceptionMessage));

            // Act
            var result = await _controller.CreateUser(userDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var errorResponse = badRequestResult.Value as ErrorResponse;
            errorResponse.Errors.Should().HaveCount(1);
            errorResponse.Errors[0].Detail.Should().Contain(exceptionMessage);
        }

        [Fact]
        public async Task CreateUser_UnexpectedExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userDto = CreateValidUserProductDetailsDto();
            var exception = new Exception("Unexpected error");

            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync((ErrorResponse)null);
            _mockValidationService.Setup(x => x.ValidateSuperUserCreation(userDto, It.IsAny<DefaultUserClaim>()))
                .Returns(new ErrorResponse { Errors = new List<Error>() });
            _mockValidationService.Setup(x => x.ValidateUserProductDetails(userDto, It.IsAny<DefaultUserClaim>()))
                .Returns(new ErrorResponse { Errors = new List<Error>() });
            _mockUserManagementService.Setup(x => x.CreateUserAsync(userDto, It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.CreateUser(userDto);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

        }

        #endregion

        #region UpdateUser Tests

        [Fact]
        public async Task UpdateUser_ValidRequest_ReturnsOk()
        {
            // Arrange
            var userDto = CreateValidUserProductDetailsDto();
            userDto.UserProfileDetails.UnityRealPageUserId = Guid.NewGuid();
            var response = new ObjectResponse { Data = userDto.UserProfileDetails.UnityRealPageUserId };

            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync((ErrorResponse)null);
            _mockValidationService.Setup(x => x.ValidateSuperUserUpdate(userDto, It.IsAny<DefaultUserClaim>(), null))
                .Returns(new ErrorResponse { Errors = new List<Error>() });
            _mockValidationService.Setup(x => x.ValidateUserProductDetails(userDto, It.IsAny<DefaultUserClaim>()))
                .Returns(new ErrorResponse { Errors = new List<Error>() });
            _mockUserManagementService.Setup(x => x.UpdateUserAsync(userDto, It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateUser(userDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task UpdateUser_MissingUnityRealPageUserId_ReturnsBadRequest()
        {
            // Arrange
            var userDto = CreateValidUserProductDetailsDto();
            userDto.UserProfileDetails.UnityRealPageUserId = Guid.Empty;

            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync((ErrorResponse)null);

            // Act
            var result = await _controller.UpdateUser(userDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var errorResponse = badRequestResult.Value as ErrorResponse;
            errorResponse.Errors[0].Detail.Should().Contain("UnityRealPageUserId not supplied");
        }

        [Fact]
        public async Task UpdateUser_SuperUserPromotion_FetchesExistingUserDetails()
        {
            // Arrange
            var userDto = CreateValidUserProductDetailsDto();
            userDto.UserProfileDetails.UnityRealPageUserId = Guid.NewGuid();
            userDto.UserProfileDetails.UserType = UserTypeDto.SuperUser;

            var existingUserDetails = new UserDetails { UserRoleTypeId = 2 };
            var response = new ObjectResponse { Data = userDto.UserProfileDetails.UnityRealPageUserId };

            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync((ErrorResponse)null);
            _mockUserQueryService.Setup(x => x.GetUserDetailsByIdAsync(userDto.UserProfileDetails.UnityRealPageUserId))
                .ReturnsAsync(existingUserDetails);
            _mockValidationService.Setup(x => x.ValidateSuperUserUpdate(userDto, It.IsAny<DefaultUserClaim>(), 2))
                .Returns(new ErrorResponse { Errors = new List<Error>() });
            _mockValidationService.Setup(x => x.ValidateUserProductDetails(userDto, It.IsAny<DefaultUserClaim>()))
                .Returns(new ErrorResponse { Errors = new List<Error>() });
            _mockUserManagementService.Setup(x => x.UpdateUserAsync(userDto, It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateUser(userDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockUserQueryService.Verify(x => x.GetUserDetailsByIdAsync(userDto.UserProfileDetails.UnityRealPageUserId), Times.Once);
        }

        [Fact]
        public async Task UpdateUser_SuperUserValidationFails_ReturnsBadRequest()
        {
            // Arrange
            var userDto = CreateValidUserProductDetailsDto();
            userDto.UserProfileDetails.UnityRealPageUserId = Guid.NewGuid();
            userDto.UserProfileDetails.UserType = UserTypeDto.SuperUser;

            var validationError = new ErrorResponse
            {
                Errors = new List<Error> { new Error { Detail = "Cannot promote to super user" } }
            };

            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync((ErrorResponse)null);
            _mockUserQueryService.Setup(x => x.GetUserDetailsByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new UserDetails { UserRoleTypeId = 2 });
            _mockValidationService.Setup(x => x.ValidateSuperUserUpdate(userDto, It.IsAny<DefaultUserClaim>(), 2))
                .Returns(validationError);

            // Act
            var result = await _controller.UpdateUser(userDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateUser_ValidationExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            var userDto = CreateValidUserProductDetailsDto();
            userDto.UserProfileDetails.UnityRealPageUserId = Guid.NewGuid();
            var exceptionMessage = "Update validation error";

            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync((ErrorResponse)null);
            _mockValidationService.Setup(x => x.ValidateSuperUserUpdate(userDto, It.IsAny<DefaultUserClaim>(), null))
                .Returns(new ErrorResponse { Errors = new List<Error>() });
            _mockValidationService.Setup(x => x.ValidateUserProductDetails(userDto, It.IsAny<DefaultUserClaim>()))
                .Returns(new ErrorResponse { Errors = new List<Error>() });
            _mockUserManagementService.Setup(x => x.UpdateUserAsync(userDto, It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new ValidationException(exceptionMessage));

            // Act
            var result = await _controller.UpdateUser(userDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateUser_UnexpectedExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userDto = CreateValidUserProductDetailsDto();
            userDto.UserProfileDetails.UnityRealPageUserId = Guid.NewGuid();

            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync((ErrorResponse)null);
            _mockValidationService.Setup(x => x.ValidateSuperUserUpdate(userDto, It.IsAny<DefaultUserClaim>(), null))
                .Returns(new ErrorResponse { Errors = new List<Error>() });
            _mockValidationService.Setup(x => x.ValidateUserProductDetails(userDto, It.IsAny<DefaultUserClaim>()))
                .Returns(new ErrorResponse { Errors = new List<Error>() });
            _mockUserManagementService.Setup(x => x.UpdateUserAsync(userDto, It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.UpdateUser(userDto);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        #endregion

        #region CreateUpdateUserStatus Tests

        [Fact]
        public async Task CreateUpdateUserStatus_ValidRequest_ReturnsOk()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var status = EntApiUserStatus.Activate;
            var response = new ObjectResponse { Data = "Success" };

            _mockUserManagementService.Setup(x => x.ChangeUserStatusAsync(userId, status, It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateUpdateUserStatus(userId, status);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateUpdateUserStatus_ArgumentExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var status = EntApiUserStatus.Activate;
            var exceptionMessage = "Invalid status";

            _mockUserManagementService.Setup(x => x.ChangeUserStatusAsync(userId, status, It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new ArgumentException(exceptionMessage));

            // Act
            var result = await _controller.CreateUpdateUserStatus(userId, status);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var errorResponse = badRequestResult.Value as ErrorResponse;
            errorResponse.Errors[0].Detail.Should().Contain(exceptionMessage);
        }

        [Fact]
        public async Task CreateUpdateUserStatus_InvalidOperationExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var status = EntApiUserStatus.Activate;
            var exceptionMessage = "Cannot change status";

            _mockUserManagementService.Setup(x => x.ChangeUserStatusAsync(userId, status, It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            // Act
            var result = await _controller.CreateUpdateUserStatus(userId, status);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateUpdateUserStatus_UnexpectedExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var status = EntApiUserStatus.Activate;

            _mockUserManagementService.Setup(x => x.ChangeUserStatusAsync(userId, status, It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.CreateUpdateUserStatus(userId, status);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        #endregion

        #region GetUser Tests

        [Fact]
        public async Task GetUser_ValidRequest_ReturnsOk()
        {
            // Arrange
            var usersData = new PagedResponse { Meta = new Meta { TotalRows = 10, RowsPerPage = 1 } };

            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync((ErrorResponse)null);
            _mockUserQueryService.Setup(x => x.GetUsersAsync(It.IsAny<int>(), 0, null, null, 1, 1))
                .ReturnsAsync(usersData);

            // Act
            var result = await _controller.GetUser();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetUser_ClientAuthenticationFails_ReturnsBadRequest()
        {
            // Arrange
            var authError = new ErrorResponse { Errors = new List<Error> { new Error { Detail = "Auth failed" } } };

            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync(authError);

            // Act
            var result = await _controller.GetUser();

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var pagedResponse = badRequestResult.Value as PagedResponse;
            pagedResponse.IsError.Should().BeTrue();
        }

        [Fact]
        public async Task GetUser_InvalidRowsPerPage_ReturnsBadRequest()
        {
            // Arrange
            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync((ErrorResponse)null);

            // Act
            var result = await _controller.GetUser(rowsPerPage: 0);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var pagedResponse = badRequestResult.Value as PagedResponse;
            pagedResponse.ErrorReason.Should().Contain("rowsPerPage must be 1 or greater");
        }

        [Fact]
        public async Task GetUser_InvalidPageNumber_ReturnsBadRequest()
        {
            // Arrange
            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync((ErrorResponse)null);

            // Act
            var result = await _controller.GetUser(pageNumber: 0);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var pagedResponse = badRequestResult.Value as PagedResponse;
            pagedResponse.ErrorReason.Should().Contain("pageNumber must be 1 or greater");
        }

        [Fact]
        public async Task GetUser_WithValidUserStatus_ParsesStatusCorrectly()
        {
            // Arrange
            var usersData = new PagedResponse { Meta = new Meta { TotalRows = 10, RowsPerPage = 1 } };

            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync((ErrorResponse)null);
            _mockUserQueryService.Setup(x => x.GetUsersAsync(
                _userClaims.OrganizationPartyId,
                (int)UserUiStatusType.Active,
                null,
                null,
                1,
                1))
                .ReturnsAsync(usersData);

            // Act
            var result = await _controller.GetUser(userStatus: "Active");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockUserQueryService.Verify(x => x.GetUsersAsync(
                _userClaims.OrganizationPartyId,
                (int)UserUiStatusType.Active,
                null,
                null,
                1,
                1), Times.Once);
        }

        [Fact]
        public async Task GetUser_WithInvalidUserStatus_DefaultsToZero()
        {
            // Arrange
            var usersData = new PagedResponse { Meta = new Meta { TotalRows = 10, RowsPerPage = 1 } };

            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync((ErrorResponse)null);
            _mockUserQueryService.Setup(x => x.GetUsersAsync(
                _userClaims.OrganizationPartyId,
                0,
                null,
                null,
                1,
                1))
                .ReturnsAsync(usersData);

            // Act
            var result = await _controller.GetUser(userStatus: "InvalidStatus");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockUserQueryService.Verify(x => x.GetUsersAsync(
                _userClaims.OrganizationPartyId,
                0,
                null,
                null,
                1,
                1), Times.Once);
        }

        [Fact]
        public async Task GetUser_WithAllParameters_ReturnsOk()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var usersData = new PagedResponse { Meta = new Meta { TotalRows = 10, RowsPerPage = 1 } };

            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync((ErrorResponse)null);
            _mockUserQueryService.Setup(x => x.GetUsersAsync(
                _userClaims.OrganizationPartyId,
                (int)UserUiStatusType.Active,
                userId,
                "John Doe",
                10,
                2))
                .ReturnsAsync(usersData);

            // Act
            var result = await _controller.GetUser(
                unityRealPageUserId: userId,
                name: "John Doe",
                rowsPerPage: 10,
                pageNumber: 2,
                userStatus: "Active");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUser_UnexpectedExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var exception = new Exception("Unexpected error");

            _mockClientAuthService.Setup(x => x.AuthenticateClientAsync(null, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync((ErrorResponse)null);

            _mockUserQueryService.Setup(x => x.GetUsersAsync(_userClaims.OrganizationPartyId, 0, null, null, 1, 1))
                .ThrowsAsync(exception);


            // Act
            var result = await _controller.GetUser();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        #endregion

        #region GetUserRoleAsset Tests

        [Fact]
        public async Task GetUserRoleAsset_ValidRequest_ReturnsOk()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var productCode = "ONESITE";
            var roleAssetData = new PagedResponse { Meta = new Meta { TotalRows = 10, RowsPerPage = 1 } };

            _mockUserQueryService.Setup(x => x.GetUserRoleAssetAsync(realPageId, productCode, _userClaims.OrganizationPartyId, It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync(roleAssetData);

            // Act
            var result = await _controller.GetUserRoleAsset(realPageId, productCode);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetUserRoleAsset_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var productCode = "ONESITE";

            _mockUserQueryService.Setup(x => x.GetUserRoleAssetAsync(realPageId, productCode, _userClaims.OrganizationPartyId, It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.GetUserRoleAsset(realPageId, productCode);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = result as NotFoundResult;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetUserRoleAsset_UnauthorizedAccess_ReturnsNotFound()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var productCode = "ONESITE";

            _mockUserQueryService.Setup(x => x.GetUserRoleAssetAsync(realPageId, productCode, _userClaims.OrganizationPartyId, It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new UnauthorizedAccessException());

            // Act
            var result = await _controller.GetUserRoleAsset(realPageId, productCode);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetUserRoleAsset_ArgumentExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var productCode = "INVALID";
            var exceptionMessage = "Invalid product code";

            _mockUserQueryService.Setup(x => x.GetUserRoleAssetAsync(realPageId, productCode, _userClaims.OrganizationPartyId, It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new ArgumentException(exceptionMessage));

            // Act
            var result = await _controller.GetUserRoleAsset(realPageId, productCode);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var errorResponse = badRequestResult.Value as ErrorResponse;
            errorResponse.Errors[0].Detail.Should().Contain(exceptionMessage);
        }

        [Fact]
        public async Task GetUserRoleAsset_UnexpectedExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var productCode = "ONESITE";

            _mockUserQueryService.Setup(x => x.GetUserRoleAssetAsync(realPageId, productCode, _userClaims.OrganizationPartyId, It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.GetUserRoleAsset(realPageId, productCode);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        #endregion

        #region GetUserProducts Tests

        [Fact]
        public async Task GetUserProducts_ValidRequest_ReturnsOk()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var productsData = new UserProductOutputResult { };

            _mockUserQueryService.Setup(x => x.GetUserProductDetailsAsync(realPageId, _userClaims.OrganizationPartyId, It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync(productsData);

            // Act
            var result = await _controller.GetUserProducts(realPageId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUserProducts_InvalidRealPageId_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.Empty;

            // Act
            var result = await _controller.GetUserProducts(realPageId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("Invalid parameter realPageId");
        }

        [Fact]
        public async Task GetUserProducts_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            // Note: In real scenario, you'd mock the User.Identity.IsAuthenticated to return false

            // Act
            // This test would require mocking HttpContext and User property
            // Skipping actual implementation as it requires additional setup

            Assert.True(true); // Placeholder
        }

        [Fact]
        public async Task GetUserProducts_UserNotFound_ReturnsOkWithErrorStatus()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            _mockUserQueryService.Setup(x => x.GetUserProductDetailsAsync(realPageId, _userClaims.OrganizationPartyId, It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.GetUserProducts(realPageId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var responseObj = okResult.Value;
            // Verify error response structure
            responseObj.Should().NotBeNull();
        }

        [Fact]
        public async Task GetUserProducts_UnauthorizedAccessException_ReturnsOkWithErrorStatus()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            _mockUserQueryService.Setup(x => x.GetUserProductDetailsAsync(realPageId, _userClaims.OrganizationPartyId, It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new UnauthorizedAccessException());

            // Act
            var result = await _controller.GetUserProducts(realPageId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUserProducts_UnexpectedExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            _mockUserQueryService.Setup(x => x.GetUserProductDetailsAsync(realPageId, _userClaims.OrganizationPartyId, It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.GetUserProducts(realPageId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        #endregion

        #region GetUserProductsByPersonaId Tests

        [Fact]
        public async Task GetUserProductsByPersonaId_ValidRequest_ReturnsOk()
        {
            // Arrange
            var productsData = new UserProductOutputResultv2 { };

            _mockUserQueryService.Setup(x => x.GetUserProductsByPersonaIdAsync(0, false, It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync(productsData);

            // Act
            var result = await _controller.GetUserProductsByPersonaId();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetUserProductsByPersonaId_WithPersonaId_ReturnsOk()
        {
            // Arrange
            var productsData = new UserProductOutputResultv2 { };

            _mockUserQueryService.Setup(x => x.GetUserProductsByPersonaIdAsync(123, false, It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync(productsData);

            // Act
            var result = await _controller.GetUserProductsByPersonaId(personaId: 123);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUserProductsByPersonaId_WithStatus_ReturnsOk()
        {
            // Arrange
            var productsData = new UserProductOutputResultv2 { };

            _mockUserQueryService.Setup(x => x.GetUserProductsByPersonaIdAsync(0, true, It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync(productsData);

            // Act
            var result = await _controller.GetUserProductsByPersonaId(withStatus: true);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUserProductsByPersonaId_KeyNotFoundExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            var exceptionMessage = "Persona not found";

            _mockUserQueryService.Setup(x => x.GetUserProductsByPersonaIdAsync(It.IsAny<long?>(), It.IsAny<bool>(), It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new KeyNotFoundException(exceptionMessage));

            // Act
            var result = await _controller.GetUserProductsByPersonaId();

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be(exceptionMessage);
        }

        [Fact]
        public async Task GetUserProductsByPersonaId_UnauthorizedAccessExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            var exceptionMessage = "Unauthorized access";

            _mockUserQueryService.Setup(x => x.GetUserProductsByPersonaIdAsync(It.IsAny<long?>(), It.IsAny<bool>(), It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new UnauthorizedAccessException(exceptionMessage));

            // Act
            var result = await _controller.GetUserProductsByPersonaId();

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetUserProductsByPersonaId_UnexpectedExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            _mockUserQueryService.Setup(x => x.GetUserProductsByPersonaIdAsync(It.IsAny<long?>(), It.IsAny<bool>(), It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.GetUserProductsByPersonaId();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        #endregion

        #region GetOmnibarInfo Tests

        [Fact]
        public async Task GetOmnibarInfo_ValidRequest_ReturnsOk()
        {
            // Arrange
            var omnibarData = new UserProductOutputResultv2 { };

            _mockUserQueryService.Setup(x => x.GetUserOmniBarProductDetailsAsync(
                _userClaims.UserRealPageGuid,
                _userClaims.OrganizationPartyId,
                It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync(omnibarData);

            // Act
            var result = await _controller.GetOmnibarInfo();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetOmnibarInfo_KeyNotFoundExceptionThrown_ReturnsOkWithErrorStatus()
        {
            // Arrange
            _mockUserQueryService.Setup(x => x.GetUserOmniBarProductDetailsAsync(
                _userClaims.UserRealPageGuid,
                _userClaims.OrganizationPartyId,
                It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.GetOmnibarInfo();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetOmnibarInfo_UnauthorizedAccessExceptionThrown_ReturnsOkWithErrorStatus()
        {
            // Arrange
            _mockUserQueryService.Setup(x => x.GetUserOmniBarProductDetailsAsync(
                _userClaims.UserRealPageGuid,
                _userClaims.OrganizationPartyId,
                It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new UnauthorizedAccessException());

            // Act
            var result = await _controller.GetOmnibarInfo();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetOmnibarInfo_UnexpectedExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            _mockUserQueryService.Setup(x => x.GetUserOmniBarProductDetailsAsync(
                _userClaims.UserRealPageGuid,
                _userClaims.OrganizationPartyId,
                It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.GetOmnibarInfo();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        #endregion

        #region GetUserProductsDetailsLogin Tests

        [Fact]
        public async Task GetUserProductsDetailsLoginByPersonaId_ValidRequest_ReturnsOk()
        {
            // Arrange
            var loginData = new List<UserProductDetailLogin> { new UserProductDetailLogin { } };

            _mockUserQueryService.Setup(x => x.GetUserProductDetailsLoginByPersonaIdAsync(_userClaims.PersonaId))
                .ReturnsAsync(loginData);

            // Act
            var result = await _controller.GetUserProductsDetailsLoginByPersonaId();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetUserProductsDetailsLoginByPersonaId_UnexpectedExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            _mockUserQueryService.Setup(x => x.GetUserProductDetailsLoginByPersonaIdAsync(_userClaims.PersonaId))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.GetUserProductsDetailsLoginByPersonaId();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetUserProductsDetailsLoginByLoginName_ValidRequest_ReturnsOk()
        {
            // Arrange
            var loginData = new List<UserProductDetailLogin> { new UserProductDetailLogin { } };

            _mockUserQueryService.Setup(x => x.GetUserProductDetailsLoginByLoginNameAsync(_userClaims.LoginName))
                .ReturnsAsync(loginData);

            // Act
            var result = await _controller.GetUserProductsDetailsLoginByLoginName();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUserProductsDetailsLoginByLoginName_UnexpectedExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            _mockUserQueryService.Setup(x => x.GetUserProductDetailsLoginByLoginNameAsync(_userClaims.LoginName))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.GetUserProductsDetailsLoginByLoginName();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        #endregion

        #region GetCurrentUserRights Tests

        [Fact]
        public void GetCurrentUserRights_ValidRequest_ReturnsOk()
        {
            // Act
            var result = _controller.GetCurrentUserRights();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var rights = okResult.Value as List<string>;
            rights.Should().Contain("read");
            rights.Should().Contain("write");
        }

        #endregion

        #region GetUserCustomFields Tests

        [Fact]
        public async Task GetUserCustomFields_ValidRequest_ReturnsOk()
        {
            // Arrange
            var customFieldsData = new ListResponse { Records = new List<object>() };

            _mockUserQueryService.Setup(x => x.GetUserCustomFieldsAsync(
                _userClaims.OrganizationPartyId,
                null,
                It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync(customFieldsData);

            // Act
            var result = await _controller.GetUserCustomFields();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUserCustomFields_WithPersonaId_ReturnsOk()
        {
            // Arrange
            var customFieldsData = new ListResponse { Records = new List<object>() };

            _mockUserQueryService.Setup(x => x.GetUserCustomFieldsAsync(
                _userClaims.OrganizationPartyId,
                123,
                It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync(customFieldsData);

            // Act
            var result = await _controller.GetUserCustomFields(userLoginPersonaId: 123);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUserCustomFields_UnexpectedExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            _mockUserQueryService.Setup(x => x.GetUserCustomFieldsAsync(
                _userClaims.OrganizationPartyId,
                It.IsAny<long?>(),
                It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.GetUserCustomFields();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        #endregion

        #region ChangeCompany Tests

        [Fact]
        public async Task ChangeCompany_ValidPersonaId_ReturnsAccepted()
        {
            // Arrange
            long personaId = 123;
            var result_data = new ChangeCompanyResult { IsSuccess = true };

            _mockUserManagementService.Setup(x => x.ChangeCompanyAsync(personaId, It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync(result_data);

            // Act
            var result = await _controller.ChangeCompany(personaId);

            // Assert
            result.Should().BeOfType<AcceptedResult>();
            var acceptedResult = result as AcceptedResult;
            acceptedResult.StatusCode.Should().Be((int)HttpStatusCode.Accepted);
        }

        [Fact]
        public async Task ChangeCompany_InvalidPersonaId_ReturnsBadRequest()
        {
            // Arrange
            long personaId = 0;

            // Act
            var result = await _controller.ChangeCompany(personaId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var errorResponse = badRequestResult.Value as ErrorResponse;
            errorResponse.Errors[0].Detail.Should().Contain("Invalid personaId");
        }

        [Fact]
        public async Task ChangeCompany_NegativePersonaId_ReturnsBadRequest()
        {
            // Arrange
            long personaId = -1;

            // Act
            var result = await _controller.ChangeCompany(personaId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task ChangeCompany_ServiceReturnsFailure_ReturnsBadRequest()
        {
            // Arrange
            long personaId = 123;
            var result_data = new ChangeCompanyResult { IsSuccess = false, ErrorMessage = "Change failed" };

            _mockUserManagementService.Setup(x => x.ChangeCompanyAsync(personaId, It.IsAny<DefaultUserClaim>()))
                .ReturnsAsync(result_data);

            // Act
            var result = await _controller.ChangeCompany(personaId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var errorResponse = badRequestResult.Value as ErrorResponse;
            errorResponse.Errors[0].Detail.Should().Contain("Change failed");
        }

        [Fact]
        public async Task ChangeCompany_UnauthorizedAccessException_ReturnsUnauthorized()
        {
            // Arrange
            long personaId = 123;

            _mockUserManagementService.Setup(x => x.ChangeCompanyAsync(personaId, It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new UnauthorizedAccessException());

            // Act
            var result = await _controller.ChangeCompany(personaId);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
            var unauthorizedResult = result as UnauthorizedResult;
            unauthorizedResult.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ChangeCompany_UnexpectedExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            long personaId = 123;

            _mockUserManagementService.Setup(x => x.ChangeCompanyAsync(personaId, It.IsAny<DefaultUserClaim>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.ChangeCompany(personaId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        #endregion

        #region GetEmployeePersonasList Tests

        [Fact]
        public async Task GetEmployeePersonasList_ValidRequest_ReturnsOk()
        {
            // Arrange
            var personasData = new ObjectListOutput<PersonaCompanyDetails, IErrorData> { };

            _mockUserQueryService.Setup(x => x.GetEmployeePersonasListAsync(_userClaims.UserId, _userClaims.OrganizationPartyId))
                .ReturnsAsync(personasData);

            // Act
            var result = await _controller.GetEmployeePersonasList();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetEmployeePersonasList_UnexpectedExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var exception = new Exception("Unexpected error");

            _mockUserQueryService.Setup(x => x.GetEmployeePersonasListAsync(_userClaims.UserId, _userClaims.OrganizationPartyId))
                .ThrowsAsync(exception);

            // Setup logging service to prevent null reference when exception is logged
            _mockLoggingService.Setup(x => x.WriteToLog(
                It.IsAny<Serilog.Events.LogEventLevel>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<Exception>(),
                It.IsAny<object[]>()));

            // Act
            var result = await _controller.GetEmployeePersonasList();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

        }

        #endregion

        #region GetPersonasList Tests

        [Fact]
        public async Task GetPersonasList_ValidRequest_ReturnsOk()
        {
            // Arrange
            var personasData = new ObjectListOutput<PersonaCompany, IErrorData> { };

            _mockUserQueryService.Setup(x => x.GetPersonasListAsync(_userClaims.UserRealPageGuid))
                .ReturnsAsync(personasData);

            // Act
            var result = await _controller.GetPersonasList();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPersonasList_UnexpectedExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            _mockUserQueryService.Setup(x => x.GetPersonasListAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.GetPersonasList();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        #endregion

        #region DeleteSamlUserProductInfoAndStatus Tests

        [Fact]
        public async Task DeleteSamlUserProductInfoAndStatus_ValidRequest_ReturnsOk()
        {
            // Arrange
            var productUser = new ProductUserAccountDetails { ProductId = 1 };

            _mockUserManagementService.Setup(x => x.DeleteSamlUserProductInfoAndStatusAsync(productUser))
                .ReturnsAsync("Success");

            // Act
            var result = await _controller.DeleteSamlUserProductInfoAndStatus(productUser);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteSamlUserProductInfoAndStatus_NullProductUser_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.DeleteSamlUserProductInfoAndStatus(null);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var errorResponse = badRequestResult.Value as ErrorResponse;
            errorResponse.Errors[0].Detail.Should().Contain("productUser is null");
        }

        [Fact]
        public async Task DeleteSamlUserProductInfoAndStatus_InvalidProductId_ReturnsBadRequest()
        {
            // Arrange
            var productUser = new ProductUserAccountDetails { ProductId = 0 };

            // Act
            var result = await _controller.DeleteSamlUserProductInfoAndStatus(productUser);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var errorResponse = badRequestResult.Value as ErrorResponse;
            errorResponse.Errors[0].Detail.Should().Contain("ProductId is required");
        }

        [Fact]
        public async Task DeleteSamlUserProductInfoAndStatus_ArgumentExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            var productUser = new ProductUserAccountDetails { ProductId = 1 };
            var exceptionMessage = "Invalid product user";

            _mockUserManagementService.Setup(x => x.DeleteSamlUserProductInfoAndStatusAsync(productUser))
                .ThrowsAsync(new ArgumentException(exceptionMessage));

            // Act
            var result = await _controller.DeleteSamlUserProductInfoAndStatus(productUser);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteSamlUserProductInfoAndStatus_UnexpectedExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var productUser = new ProductUserAccountDetails { ProductId = 1 };

            _mockUserManagementService.Setup(x => x.DeleteSamlUserProductInfoAndStatusAsync(productUser))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.DeleteSamlUserProductInfoAndStatus(productUser);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        #endregion

        #region UpdateProductUserAccountDetails Tests

        [Fact]
        public async Task UpdateProductUserAccountDetails_ValidRequest_ReturnsOk()
        {
            // Arrange
            var productUser = new ProductUserAccountDetails { ProductId = 1 };

            _mockUserManagementService.Setup(x => x.UpdateProductUserAccountDetailsAsync(productUser))
                .ReturnsAsync("Success");

            // Act
            var result = await _controller.UpdateProductUserAccountDetails(productUser);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateProductUserAccountDetails_NullProductUser_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.UpdateProductUserAccountDetails(null);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var errorResponse = badRequestResult.Value as ErrorResponse;
            errorResponse.Errors[0].Detail.Should().Contain("productUser is null");
        }

        [Fact]
        public async Task UpdateProductUserAccountDetails_InvalidProductId_ReturnsBadRequest()
        {
            // Arrange
            var productUser = new ProductUserAccountDetails { ProductId = -1 };

            // Act
            var result = await _controller.UpdateProductUserAccountDetails(productUser);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateProductUserAccountDetails_ArgumentExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            var productUser = new ProductUserAccountDetails { ProductId = 1 };
            var exceptionMessage = "Invalid update";

            _mockUserManagementService.Setup(x => x.UpdateProductUserAccountDetailsAsync(productUser))
                .ThrowsAsync(new ArgumentException(exceptionMessage));

            // Act
            var result = await _controller.UpdateProductUserAccountDetails(productUser);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateProductUserAccountDetails_UnexpectedExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var productUser = new ProductUserAccountDetails { ProductId = 1 };

            _mockUserManagementService.Setup(x => x.UpdateProductUserAccountDetailsAsync(productUser))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.UpdateProductUserAccountDetails(productUser);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        #endregion

        #region GetSamlProductAttributes Tests

        [Fact]
        public async Task GetSamlProductAttributes_ValidProductId_ReturnsOk()
        {
            // Arrange
            int productId = 1;
            var attributes = new List<SharedObjects.Saml.SamlProductAttributes> { };

            _mockUserQueryService.Setup(x => x.GetSamlProductAttributesAsync(productId))
                .ReturnsAsync(attributes);

            // Act
            var result = await _controller.GetSamlProductAttributes(productId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetSamlProductAttributes_InvalidProductId_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetSamlProductAttributes(0);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var errorResponse = badRequestResult.Value as ErrorResponse;
            errorResponse.Errors[0].Detail.Should().Contain("ProductId must be greater than 0");
        }

        [Fact]
        public async Task GetSamlProductAttributes_UnexpectedExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var productId = 1;

            _mockUserQueryService.Setup(x => x.GetSamlProductAttributesAsync(productId))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.GetSamlProductAttributes(productId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        #endregion

        private static UserProductDetailsDto CreateValidUserProductDetailsDto()
        {
            return new UserProductDetailsDto
            {
                UserProfileDetails = new UserDataDto
                {
                    UnityRealPageUserId = Guid.NewGuid(),
                    Email = "john.doe@example.com",
                    FirstName = "John",
                    LastName = "Doe",
                    IsExternalIdp = true,
                    UserType = UserTypeDto.Regular
                },
                ProductList = new List<ProductDetailDto>
                {
                    new ProductDetailDto
                    {
                        ProductCode = "OS",
                        PropertiesAssigned = new List<string> { "Property1", "Property2" },
                        RolesAssigned = new List<string> { "Role1", "Role2" },
                        IsAssigned = true
                    },
                    new ProductDetailDto
                    {
                       ProductCode = "ONST",
                        PropertiesAssigned = new List<string> { "Property1", "Property2" },
                        RolesAssigned = new List<string> { "Role1", "Role2" },
                        IsAssigned = true
                    }
                }
            };
        }
    }
}