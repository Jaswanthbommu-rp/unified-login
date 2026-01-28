using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.EmployeeAccess;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for EmployeeAccessController.
    /// Tests all endpoints, error cases, and edge cases for 100% code coverage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EmployeeAccessControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IManageEmployeeAccess> _mockManageEmployeeAccess;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private EmployeeAccessController _employeeAccessController;

        #endregion

        #region Constructor

        public EmployeeAccessControllerTests()
        {
            _mockManageEmployeeAccess = new Mock<IManageEmployeeAccess>();
            _mockUserClaimsAccessor = MockUserClaimsAccessor;

            _employeeAccessController = new EmployeeAccessController(
                _mockManageEmployeeAccess.Object,
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
            var controller = new EmployeeAccessController(
                _mockManageEmployeeAccess.Object,
                _mockUserClaimsAccessor.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullManageEmployeeAccess_CreatesInstance()
        {
            // Note: Controller doesn't have null checks, so this documents current behavior
            // Act
            var controller = new EmployeeAccessController(
                null!,
                _mockUserClaimsAccessor.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_CreatesInstance()
        {
            // Act
            var controller = new EmployeeAccessController(
                _mockManageEmployeeAccess.Object,
                null!);

            // Assert
            Assert.NotNull(controller);
        }

        #endregion

        #region GetCompanies Tests - Success

        [Fact]
        public async Task GetCompanies_WithValidParameters_ReturnsOkResult()
        {
            // Arrange
            const long editorPersonaId = 12345;
            const string filter = "test";
            var expectedResponse = new ListResponse
            {
                Records = new List<object> { new { Id = 1, Name = "Company 1" } },
                TotalRows = 1
            };

            _mockManageEmployeeAccess
                .Setup(x => x.GetCompanies(editorPersonaId, filter))
                .Returns(expectedResponse);

            // Act
            var result = await _employeeAccessController.GetCompanies(editorPersonaId, filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Fact]
        public async Task GetCompanies_WithEmptyFilter_ReturnsOkResult()
        {
            // Arrange
            const long editorPersonaId = 12345;
            const string filter = "";
            var expectedResponse = new ListResponse { Records = new List<object>() };

            _mockManageEmployeeAccess
                .Setup(x => x.GetCompanies(editorPersonaId, filter))
                .Returns(expectedResponse);

            // Act
            var result = await _employeeAccessController.GetCompanies(editorPersonaId, filter);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetCompanies_WithNullFilter_ReturnsOkResult()
        {
            // Arrange
            const long editorPersonaId = 12345;
            var expectedResponse = new ListResponse { Records = new List<object>() };

            _mockManageEmployeeAccess
                .Setup(x => x.GetCompanies(editorPersonaId, null))
                .Returns(expectedResponse);

            // Act
            var result = await _employeeAccessController.GetCompanies(editorPersonaId, null!);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetCompanies_CallsServiceWithCorrectParameters()
        {
            // Arrange
            const long editorPersonaId = 99999;
            const string filter = "searchTerm";

            _mockManageEmployeeAccess
                .Setup(x => x.GetCompanies(editorPersonaId, filter))
                .Returns(new ListResponse());

            // Act
            await _employeeAccessController.GetCompanies(editorPersonaId, filter);

            // Assert
            _mockManageEmployeeAccess.Verify(
                x => x.GetCompanies(editorPersonaId, filter),
                Times.Once);
        }

        #endregion

        #region GetCompanies Tests - BadRequest

        [Fact]
        public async Task GetCompanies_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            // Arrange
            const long editorPersonaId = 0;
            const string filter = "test";

            // Act
            var result = await _employeeAccessController.GetCompanies(editorPersonaId, filter);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        #endregion

        #region GetUsers Tests - Success

        [Fact]
        public async Task GetUsers_WithValidParameters_ReturnsOkResult()
        {
            // Arrange
            const long editorPersonaId = 12345;
            const string filter = "test";
            var expectedResponse = new ListResponse
            {
                Records = new List<object> { new { Id = 1, Name = "User 1" } },
                TotalRows = 1
            };

            _mockManageEmployeeAccess
                .Setup(x => x.GetUsers(editorPersonaId, filter))
                .Returns(expectedResponse);

            // Act
            var result = await _employeeAccessController.GetUsers(editorPersonaId, filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Fact]
        public async Task GetUsers_WithEmptyFilter_ReturnsOkResult()
        {
            // Arrange
            const long editorPersonaId = 12345;
            const string filter = "";
            var expectedResponse = new ListResponse { Records = new List<object>() };

            _mockManageEmployeeAccess
                .Setup(x => x.GetUsers(editorPersonaId, filter))
                .Returns(expectedResponse);

            // Act
            var result = await _employeeAccessController.GetUsers(editorPersonaId, filter);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetUsers_CallsServiceWithCorrectParameters()
        {
            // Arrange
            const long editorPersonaId = 88888;
            const string filter = "userSearch";

            _mockManageEmployeeAccess
                .Setup(x => x.GetUsers(editorPersonaId, filter))
                .Returns(new ListResponse());

            // Act
            await _employeeAccessController.GetUsers(editorPersonaId, filter);

            // Assert
            _mockManageEmployeeAccess.Verify(
                x => x.GetUsers(editorPersonaId, filter),
                Times.Once);
        }

        #endregion

        #region GetUsers Tests - BadRequest

        [Fact]
        public async Task GetUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            // Arrange
            const long editorPersonaId = 0;
            const string filter = "test";

            // Act
            var result = await _employeeAccessController.GetUsers(editorPersonaId, filter);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        #endregion

        #region GetEmployeePersonaId Tests - Success

        [Fact]
        public async Task GetEmployeePersonaId_WithValidCompanyId_ReturnsOkResult()
        {
            // Arrange
            var companyRealPageId = Guid.NewGuid();
            var userClaim = new DefaultUserClaim
            {
                UserId = 100,
                UserRealPageGuid = Guid.NewGuid()
            };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var expectedResponse = new EmployeePersona
            {
                PersonaId = 12345,
                RealpageUserId = Guid.NewGuid()
            };

            _mockManageEmployeeAccess
                .Setup(x => x.GetOrCreateEmployeePersonaId(companyRealPageId, userClaim))
                .Returns(expectedResponse);

            // Act
            var result = await _employeeAccessController.GetEmployeePersonaId(companyRealPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<EmployeePersona>(okResult.Value);
            Assert.Equal(12345, response.PersonaId);
        }

        [Fact]
        public async Task GetEmployeePersonaId_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var companyRealPageId = Guid.NewGuid();
            var userClaim = new DefaultUserClaim { UserId = 100 };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            _mockManageEmployeeAccess
                .Setup(x => x.GetOrCreateEmployeePersonaId(companyRealPageId, userClaim))
                .Returns(new EmployeePersona());

            // Act
            await _employeeAccessController.GetEmployeePersonaId(companyRealPageId);

            // Assert
            _mockManageEmployeeAccess.Verify(
                x => x.GetOrCreateEmployeePersonaId(companyRealPageId, userClaim),
                Times.Once);
        }

        #endregion

        #region GetEmployeePersonaId Tests - BadRequest

        [Fact]
        public async Task GetEmployeePersonaId_WithEmptyCompanyId_ReturnsBadRequest()
        {
            // Act
            var result = await _employeeAccessController.GetEmployeePersonaId(Guid.Empty);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Company ID not supplied.", badRequestResult.Value);
        }

        #endregion

        #region GetEmployeePersonaId Tests - Unauthorized

        [Fact]
        public async Task GetEmployeePersonaId_WithNullUserClaim_ReturnsUnauthorized()
        {
            // Arrange
            var companyRealPageId = Guid.NewGuid();
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            // Act
            var result = await _employeeAccessController.GetEmployeePersonaId(companyRealPageId);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        #endregion

        #region CreateEmployeeProductUser Tests - Success

        [Fact]
        public async Task CreateEmployeeProductUser_WithValidParameters_ReturnsOkWithSuccessStatus()
        {
            // Arrange
            const int productId = 100;
            const long personaId = 12345;

            _mockManageEmployeeAccess
                .Setup(x => x.CreateEmployeeProductUser(productId, personaId))
                .Returns(string.Empty);

            // Act
            var result = await _employeeAccessController.CreateEmployeeProductUser(productId, personaId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<EmployeeAccessController.EmployeeAccessResponse>(okResult.Value);
            Assert.True(response.Status);
            Assert.Equal("", response.ErrorMessage);
        }

        //[Fact]
        //public async Task CreateEmployeeProductUser_WhenServiceReturnsNull_ReturnsOkWithSuccessStatus()
        //{
        //    // Arrange
        //    const int productId = 100;
        //    const long personaId = 12345;

        //    _mockManageEmployeeAccess
        //        .Setup(x => x.CreateEmployeeProductUser(productId, personaId))
        //        .Returns((string)null!);

        //    // Act
        //    var result = await _employeeAccessController.CreateEmployeeProductUser(productId, personaId);

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var response = Assert.IsType<EmployeeAccessController.EmployeeAccessResponse>(okResult.Value);
        //    Assert.True(response.Status);
        //}

        [Fact]
        public async Task CreateEmployeeProductUser_WhenServiceReturnsDeletedProductLogin_RetriesAndReturnsSuccess()
        {
            // Arrange
            const int productId = 100;
            const long personaId = 12345;
            var callCount = 0;

            _mockManageEmployeeAccess
                .Setup(x => x.CreateEmployeeProductUser(productId, personaId))
                .Returns(() =>
                {
                    callCount++;
                    return callCount == 1 ? "DeletedProductLogin" : string.Empty;
                });

            // Act
            var result = await _employeeAccessController.CreateEmployeeProductUser(productId, personaId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<EmployeeAccessController.EmployeeAccessResponse>(okResult.Value);
            Assert.True(response.Status);
            _mockManageEmployeeAccess.Verify(
                x => x.CreateEmployeeProductUser(productId, personaId),
                Times.Exactly(2));
        }

        [Fact]
        public async Task CreateEmployeeProductUser_WhenServiceReturnsDeletedProductLoginCaseInsensitive_Retries()
        {
            // Arrange
            const int productId = 100;
            const long personaId = 12345;
            var callCount = 0;

            _mockManageEmployeeAccess
                .Setup(x => x.CreateEmployeeProductUser(productId, personaId))
                .Returns(() =>
                {
                    callCount++;
                    return callCount == 1 ? "DELETEDPRODUCTLOGIN" : string.Empty;
                });

            // Act
            var result = await _employeeAccessController.CreateEmployeeProductUser(productId, personaId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<EmployeeAccessController.EmployeeAccessResponse>(okResult.Value);
            Assert.True(response.Status);
            _mockManageEmployeeAccess.Verify(
                x => x.CreateEmployeeProductUser(productId, personaId),
                Times.Exactly(2));
        }

        #endregion

        #region CreateEmployeeProductUser Tests - Error Response

        [Fact]
        public async Task CreateEmployeeProductUser_WhenServiceReturnsError_ReturnsOkWithErrorMessage()
        {
            // Arrange
            const int productId = 100;
            const long personaId = 12345;
            const string errorMessage = "Failed to create employee product user";

            _mockManageEmployeeAccess
                .Setup(x => x.CreateEmployeeProductUser(productId, personaId))
                .Returns(errorMessage);

            // Act
            var result = await _employeeAccessController.CreateEmployeeProductUser(productId, personaId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<EmployeeAccessController.EmployeeAccessResponse>(okResult.Value);
            Assert.False(response.Status);
            Assert.Equal(errorMessage, response.ErrorMessage);
        }

        [Fact]
        public async Task CreateEmployeeProductUser_WhenDeletedProductLoginReturnsError_ReturnsOkWithErrorMessage()
        {
            // Arrange
            const int productId = 100;
            const long personaId = 12345;
            const string errorMessage = "No assignable groups found";
            var callCount = 0;

            _mockManageEmployeeAccess
                .Setup(x => x.CreateEmployeeProductUser(productId, personaId))
                .Returns(() =>
                {
                    callCount++;
                    return callCount == 1 ? "DeletedProductLogin" : errorMessage;
                });

            // Act
            var result = await _employeeAccessController.CreateEmployeeProductUser(productId, personaId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<EmployeeAccessController.EmployeeAccessResponse>(okResult.Value);
            Assert.False(response.Status);
            Assert.Equal(errorMessage, response.ErrorMessage);
        }

        #endregion

        #region CreateEmployeeProductUser Tests - BadRequest

        [Fact]
        public async Task CreateEmployeeProductUser_WithZeroProductId_ReturnsBadRequest()
        {
            // Arrange
            const int productId = 0;
            const long personaId = 12345;

            // Act
            var result = await _employeeAccessController.CreateEmployeeProductUser(productId, personaId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<EmployeeAccessController.EmployeeAccessResponse>(badRequestResult.Value);
            Assert.Equal("Product ID not supplied.", response.ErrorMessage);
        }

        [Fact]
        public async Task CreateEmployeeProductUser_WithZeroPersonaId_ReturnsBadRequest()
        {
            // Arrange
            const int productId = 100;
            const long personaId = 0;

            // Act
            var result = await _employeeAccessController.CreateEmployeeProductUser(productId, personaId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<EmployeeAccessController.EmployeeAccessResponse>(badRequestResult.Value);
            Assert.Equal("Persona ID not supplied.", response.ErrorMessage);
        }

        [Fact]
        public async Task CreateEmployeeProductUser_WithBothZeroIds_ReturnsBadRequestForProductId()
        {
            // Arrange - productId is checked first
            const int productId = 0;
            const long personaId = 0;

            // Act
            var result = await _employeeAccessController.CreateEmployeeProductUser(productId, personaId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<EmployeeAccessController.EmployeeAccessResponse>(badRequestResult.Value);
            Assert.Equal("Product ID not supplied.", response.ErrorMessage);
        }

        #endregion

        #region EmployeeAccessResponse Tests

        [Fact]
        public void EmployeeAccessResponse_DefaultValues_AreCorrect()
        {
            // Act
            var response = new EmployeeAccessController.EmployeeAccessResponse();

            // Assert
            Assert.False(response.Status);
            Assert.Equal("", response.ErrorMessage);
        }

        [Fact]
        public void EmployeeAccessResponse_CanSetProperties()
        {
            // Arrange
            var response = new EmployeeAccessController.EmployeeAccessResponse();

            // Act
            response.Status = true;
            response.ErrorMessage = "Test error";

            // Assert
            Assert.True(response.Status);
            Assert.Equal("Test error", response.ErrorMessage);
        }

        #endregion

        #region Concurrent Access Tests

        [Fact]
        public async Task GetCompanies_MultipleConcurrentCalls_AllReturnOk()
        {
            // Arrange
            _mockManageEmployeeAccess
                .Setup(x => x.GetCompanies(It.IsAny<long>(), It.IsAny<string>()))
                .Returns(new ListResponse());

            var tasks = new List<Task<IActionResult>>();

            // Act
            for (int i = 1; i <= 10; i++)
            {
                tasks.Add(_employeeAccessController.GetCompanies(i, $"filter{i}"));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            foreach (var result in results)
            {
                Assert.IsType<OkObjectResult>(result);
            }
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _employeeAccessController = null!;
            base.Dispose();
        }

        #endregion
    }
}





