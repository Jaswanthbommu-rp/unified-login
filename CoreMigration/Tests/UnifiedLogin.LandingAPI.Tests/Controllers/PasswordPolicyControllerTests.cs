using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for PasswordPolicyController.
    /// Tests all endpoints, error cases, and edge cases for 100% code coverage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PasswordPolicyControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IManagePasswordPolicy> _mockManagePasswordPolicy;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private PasswordPolicyController _passwordPolicyController;

        #endregion

        #region Constructor

        public PasswordPolicyControllerTests()
        {
            _mockManagePasswordPolicy = new Mock<IManagePasswordPolicy>();
            _mockUserClaimsAccessor = MockUserClaimsAccessor;

            _passwordPolicyController = new PasswordPolicyController(
                _mockManagePasswordPolicy.Object,
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
            var controller = new PasswordPolicyController(
                _mockManagePasswordPolicy.Object,
                _mockUserClaimsAccessor.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullManagePasswordPolicy_CreatesInstance()
        {
            // Note: Controller doesn't have null checks, so this documents current behavior
            // Act
            var controller = new PasswordPolicyController(
                null!,
                _mockUserClaimsAccessor.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_CreatesInstance()
        {
            // Act
            var controller = new PasswordPolicyController(
                _mockManagePasswordPolicy.Object,
                null!);

            // Assert
            Assert.NotNull(controller);
        }

        #endregion

        #region CreatePasswordPolicy Tests - Success

        [Fact]
        public async Task CreatePasswordPolicy_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var passwordPolicy = CreateValidPasswordPolicy();
            var userClaim = new DefaultUserClaim { UserId = 100 };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            _mockManagePasswordPolicy
                .Setup(x => x.CreatePasswordPolicy(It.IsAny<IPasswordPolicy>()))
                .Returns(new RepositoryResponse { Id = 12345 });

            // Act
            var result = await _passwordPolicyController.CreatePasswordPolicy(passwordPolicy);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var outputResult = Assert.IsType<PasswordPolicy.PasswordPolicyOutputResult>(okResult.Value);
            Assert.Equal(12345, outputResult.NewPasswordPolicyId);
        }

        [Fact]
        public async Task CreatePasswordPolicy_SetsUserIdFromClaims()
        {
            // Arrange
            var passwordPolicy = CreateValidPasswordPolicy();
            var userClaim = new DefaultUserClaim { UserId = 999 };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            PasswordPolicy capturedPolicy = null!;
            _mockManagePasswordPolicy
                .Setup(x => x.CreatePasswordPolicy(It.IsAny<IPasswordPolicy>()))
                .Callback<IPasswordPolicy>(p => capturedPolicy = (PasswordPolicy)p)
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            await _passwordPolicyController.CreatePasswordPolicy(passwordPolicy);

            // Assert
            Assert.Equal(999, capturedPolicy.UserId);
        }

        [Fact]
        public async Task CreatePasswordPolicy_WithNullUserClaim_SetsUserIdToZero()
        {
            // Arrange
            var passwordPolicy = CreateValidPasswordPolicy();
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            PasswordPolicy capturedPolicy = null!;
            _mockManagePasswordPolicy
                .Setup(x => x.CreatePasswordPolicy(It.IsAny<IPasswordPolicy>()))
                .Callback<IPasswordPolicy>(p => capturedPolicy = (PasswordPolicy)p)
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            await _passwordPolicyController.CreatePasswordPolicy(passwordPolicy);

            // Assert
            Assert.Equal(0, capturedPolicy.UserId);
        }

        [Fact]
        public async Task CreatePasswordPolicy_CallsServiceWithCorrectPolicy()
        {
            // Arrange
            var passwordPolicy = CreateValidPasswordPolicy();
            var userClaim = new DefaultUserClaim { UserId = 100 };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            _mockManagePasswordPolicy
                .Setup(x => x.CreatePasswordPolicy(It.IsAny<IPasswordPolicy>()))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            await _passwordPolicyController.CreatePasswordPolicy(passwordPolicy);

            // Assert
            _mockManagePasswordPolicy.Verify(
                x => x.CreatePasswordPolicy(passwordPolicy),
                Times.Once);
        }

        #endregion

        #region CreatePasswordPolicy Tests - BadRequest

        [Fact]
        public async Task CreatePasswordPolicy_WithNullPasswordPolicy_ReturnsBadRequest()
        {
            // Act
            var result = await _passwordPolicyController.CreatePasswordPolicy(null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Null parameter: Password Policy.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreatePasswordPolicy_WhenRepositoryReturnsZeroId_ReturnsBadRequest()
        {
            // Arrange
            var passwordPolicy = CreateValidPasswordPolicy();
            const string errorMessage = "Failed to create password policy";
            var userClaim = new DefaultUserClaim { UserId = 100 };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            _mockManagePasswordPolicy
                .Setup(x => x.CreatePasswordPolicy(It.IsAny<IPasswordPolicy>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            // Act
            var result = await _passwordPolicyController.CreatePasswordPolicy(passwordPolicy);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        #endregion

        #region GetPasswordPolicy Tests - Success

        [Fact]
        public async Task GetPasswordPolicy_WithValidPartyId_ReturnsOkWithPolicy()
        {
            // Arrange
            const long partyId = 12345;
            var expectedPolicy = CreateValidPasswordPolicy();
            expectedPolicy.PartyId = partyId;

            _mockManagePasswordPolicy
                .Setup(x => x.GetPasswordPolicy(partyId))
                .Returns(expectedPolicy);

            // Act
            var result = await _passwordPolicyController.GetPasswordPolicy(partyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IPasswordPolicy, IErrorData>>(okResult.Value);
            Assert.NotNull(output.obj);
            Assert.True(output.Status.Success);
        }

        [Fact]
        public async Task GetPasswordPolicy_CallsServiceWithCorrectPartyId()
        {
            // Arrange
            const long partyId = 99999;

            _mockManagePasswordPolicy
                .Setup(x => x.GetPasswordPolicy(partyId))
                .Returns(CreateValidPasswordPolicy());

            // Act
            await _passwordPolicyController.GetPasswordPolicy(partyId);

            // Assert
            _mockManagePasswordPolicy.Verify(
                x => x.GetPasswordPolicy(partyId),
                Times.Once);
        }

        #endregion

        #region GetPasswordPolicy Tests - Error Responses

        [Fact]
        public async Task GetPasswordPolicy_WithZeroPartyId_ReturnsOkWithError()
        {
            // Act
            var result = await _passwordPolicyController.GetPasswordPolicy(0);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IPasswordPolicy, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("PasswordPolicy.GetPasswordPolicy.1", output.Status.ErrorCode);
            Assert.Equal("Invalid parameter: Company PartyId", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task GetPasswordPolicy_WithNegativePartyId_ReturnsOkWithError()
        {
            // Act
            var result = await _passwordPolicyController.GetPasswordPolicy(-1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IPasswordPolicy, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("PasswordPolicy.GetPasswordPolicy.1", output.Status.ErrorCode);
        }

        [Fact]
        public async Task GetPasswordPolicy_WhenPolicyNotFound_ReturnsOkWithError()
        {
            // Arrange
            const long partyId = 12345;

            _mockManagePasswordPolicy
                .Setup(x => x.GetPasswordPolicy(partyId))
                .Returns((PasswordPolicy)null!);

            // Act
            var result = await _passwordPolicyController.GetPasswordPolicy(partyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IPasswordPolicy, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("PasswordPolicy.GetPasswordPolicy.2", output.Status.ErrorCode);
            Assert.Equal("Get PasswordPolicy details: No data.", output.Status.ErrorMsg);
        }

        #endregion

        #region UpdatePasswordPolicy Tests - Success

        [Fact]
        public async Task UpdatePasswordPolicy_WithValidData_ReturnsOkWithPolicy()
        {
            // Arrange
            var passwordPolicy = CreateValidPasswordPolicy();
            var userClaim = new DefaultUserClaim { UserId = 100 };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            _mockManagePasswordPolicy
                .Setup(x => x.UpdatePasswordPolicy(It.IsAny<IPasswordPolicy>()))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _passwordPolicyController.UpdatePasswordPolicy(passwordPolicy);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPolicy = Assert.IsType<PasswordPolicy>(okResult.Value);
            Assert.Equal(passwordPolicy.PartyId, returnedPolicy.PartyId);
        }

        [Fact]
        public async Task UpdatePasswordPolicy_SetsUserIdFromClaims()
        {
            // Arrange
            var passwordPolicy = CreateValidPasswordPolicy();
            var userClaim = new DefaultUserClaim { UserId = 888 };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            PasswordPolicy capturedPolicy = null!;
            _mockManagePasswordPolicy
                .Setup(x => x.UpdatePasswordPolicy(It.IsAny<IPasswordPolicy>()))
                .Callback<IPasswordPolicy>(p => capturedPolicy = (PasswordPolicy)p)
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            await _passwordPolicyController.UpdatePasswordPolicy(passwordPolicy);

            // Assert
            Assert.Equal(888, capturedPolicy.UserId);
        }

        [Fact]
        public async Task UpdatePasswordPolicy_WithNullUserClaim_SetsUserIdToZero()
        {
            // Arrange
            var passwordPolicy = CreateValidPasswordPolicy();
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            PasswordPolicy capturedPolicy = null!;
            _mockManagePasswordPolicy
                .Setup(x => x.UpdatePasswordPolicy(It.IsAny<IPasswordPolicy>()))
                .Callback<IPasswordPolicy>(p => capturedPolicy = (PasswordPolicy)p)
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            await _passwordPolicyController.UpdatePasswordPolicy(passwordPolicy);

            // Assert
            Assert.Equal(0, capturedPolicy.UserId);
        }

        [Fact]
        public async Task UpdatePasswordPolicy_CallsServiceWithCorrectPolicy()
        {
            // Arrange
            var passwordPolicy = CreateValidPasswordPolicy();
            var userClaim = new DefaultUserClaim { UserId = 100 };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            _mockManagePasswordPolicy
                .Setup(x => x.UpdatePasswordPolicy(It.IsAny<IPasswordPolicy>()))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            await _passwordPolicyController.UpdatePasswordPolicy(passwordPolicy);

            // Assert
            _mockManagePasswordPolicy.Verify(
                x => x.UpdatePasswordPolicy(passwordPolicy),
                Times.Once);
        }

        #endregion

        #region UpdatePasswordPolicy Tests - BadRequest

        [Fact]
        public async Task UpdatePasswordPolicy_WithNullPasswordPolicy_ReturnsBadRequest()
        {
            // Act
            var result = await _passwordPolicyController.UpdatePasswordPolicy(null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Null parameter: Password Policy.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdatePasswordPolicy_WhenRepositoryReturnsZeroId_ReturnsBadRequest()
        {
            // Arrange
            var passwordPolicy = CreateValidPasswordPolicy();
            const string errorMessage = "Failed to update password policy";
            var userClaim = new DefaultUserClaim { UserId = 100 };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            _mockManagePasswordPolicy
                .Setup(x => x.UpdatePasswordPolicy(It.IsAny<IPasswordPolicy>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            // Act
            var result = await _passwordPolicyController.UpdatePasswordPolicy(passwordPolicy);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task GetPasswordPolicy_WithMinValidPartyId_ReturnsOkWithPolicy()
        {
            // Arrange
            const long partyId = 1;
            var expectedPolicy = CreateValidPasswordPolicy();

            _mockManagePasswordPolicy
                .Setup(x => x.GetPasswordPolicy(partyId))
                .Returns(expectedPolicy);

            // Act
            var result = await _passwordPolicyController.GetPasswordPolicy(partyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IPasswordPolicy, IErrorData>>(okResult.Value);
            Assert.NotNull(output.obj);
        }

        [Fact]
        public async Task GetPasswordPolicy_WithMaxPartyId_ReturnsOkWithPolicy()
        {
            // Arrange
            const long partyId = long.MaxValue;
            var expectedPolicy = CreateValidPasswordPolicy();

            _mockManagePasswordPolicy
                .Setup(x => x.GetPasswordPolicy(partyId))
                .Returns(expectedPolicy);

            // Act
            var result = await _passwordPolicyController.GetPasswordPolicy(partyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IPasswordPolicy, IErrorData>>(okResult.Value);
            Assert.NotNull(output.obj);
        }

        [Fact]
        public async Task CreatePasswordPolicy_WithFullyPopulatedPolicy_ReturnsOkResult()
        {
            // Arrange
            var passwordPolicy = new PasswordPolicy
            {
                PartyId = 12345,
                Name = "Test Policy",
                MinimumLength = 8,
                MaximumLength = 128,
                MinimumLowercase = 1,
                MinimumUppercase = 1,
                MinimumNumeric = 1,
                MinimumSpecialCharacter = 1,
                AllowUsersToChangeOwnPassword = true,
                EnablePasswordExpiration = true,
                PreventPasswordReuse = true
            };
            var userClaim = new DefaultUserClaim { UserId = 100 };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            _mockManagePasswordPolicy
                .Setup(x => x.CreatePasswordPolicy(It.IsAny<IPasswordPolicy>()))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _passwordPolicyController.CreatePasswordPolicy(passwordPolicy);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Concurrent Access Tests

        [Fact]
        public async Task CreatePasswordPolicy_MultipleConcurrentCalls_AllComplete()
        {
            // Arrange
            var userClaim = new DefaultUserClaim { UserId = 100 };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            _mockManagePasswordPolicy
                .Setup(x => x.CreatePasswordPolicy(It.IsAny<IPasswordPolicy>()))
                .Returns(new RepositoryResponse { Id = 1 });

            var tasks = new List<Task<IActionResult>>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                var policy = CreateValidPasswordPolicy();
                policy.PartyId = i;
                tasks.Add(_passwordPolicyController.CreatePasswordPolicy(policy));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            foreach (var result in results)
            {
                Assert.IsType<OkObjectResult>(result);
            }
        }

        [Fact]
        public async Task GetPasswordPolicy_MultipleConcurrentCalls_AllComplete()
        {
            // Arrange
            _mockManagePasswordPolicy
                .Setup(x => x.GetPasswordPolicy(It.IsAny<long>()))
                .Returns(CreateValidPasswordPolicy());

            var tasks = new List<Task<IActionResult>>();

            // Act
            for (int i = 1; i <= 10; i++)
            {
                tasks.Add(_passwordPolicyController.GetPasswordPolicy(i));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            foreach (var result in results)
            {
                var okResult = Assert.IsType<OkObjectResult>(result);
                var output = Assert.IsType<ObjectOutput<IPasswordPolicy, IErrorData>>(okResult.Value);
                Assert.NotNull(output.obj);
            }
        }

        #endregion

        #region Helper Methods

        private static PasswordPolicy CreateValidPasswordPolicy()
        {
            return new PasswordPolicy
            {
                PartyId = 12345,
                Name = "Test Policy",
                MinimumLength = 8,
                MaximumLength = 128,
                MinimumLowercase = 0,
                MinimumUppercase = 0,
                MinimumNumeric = 0,
                MinimumSpecialCharacter = 0,
                AllowUsersToChangeOwnPassword = true,
                EnablePasswordExpiration = false,
                PreventPasswordReuse = false
            };
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _passwordPolicyController = null!;
            base.Dispose();
        }

        #endregion
    }
}





