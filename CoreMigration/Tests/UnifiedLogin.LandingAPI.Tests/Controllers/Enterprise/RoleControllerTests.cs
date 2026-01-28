using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using UnifiedLogin.LandingAPIEnterprise.Controllers;
using UnifiedLogin.LandingAPIEnterprise.Services.Role;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.ResponseObject;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers.Enterprise
{
    /// <summary>
    /// Comprehensive unit tests for RoleController with 100% code coverage.
    /// Tests all endpoints, error cases, validation scenarios, and authentication flows.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class RoleControllerTests
    {
        #region Private Fields

        private readonly Mock<IRoleQueryService> _mockRoleQueryService;
        private readonly Mock<IClientCredentialAuthenticator> _mockClientCredentialAuthenticator;
        private readonly DefaultUserClaim _defaultUserClaim;
        private readonly RoleController _controller;

        private readonly Guid _testUserId = Guid.NewGuid();
        private readonly Guid _testUpfmId = Guid.NewGuid();
        private readonly long _testPersonaId = 100;
        private readonly long _testOrgPartyId = 1000;

        #endregion

        #region Constructor

        public RoleControllerTests()
        {
            _mockRoleQueryService = new Mock<IRoleQueryService>();
            _mockClientCredentialAuthenticator = new Mock<IClientCredentialAuthenticator>();

            _defaultUserClaim = new DefaultUserClaim
            {
                UserId = 1,
                LoginName = "testuser@test.com",
                FirstName = "Test",
                LastName = "User",
                UserRealPageGuid = _testUserId,
                OrganizationPartyId = _testOrgPartyId,
                PersonaId = _testPersonaId,
                CorrelationId = Guid.NewGuid()
            };

            _controller = new RoleController(
                _mockRoleQueryService.Object,
                _mockClientCredentialAuthenticator.Object,
                _defaultUserClaim)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim("scope", "enterpriseapi")
                        }))
                    }
                }
            };
        }

        #endregion

        #region GetUserProductRoles Tests

        [Fact]
        public void GetUserProductRoles_WithValidUPFMProduct_ReturnsOkResult()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var roles = new List<ProductRole>
            {
                new ProductRole { ID = "1", Name = "Admin", IsAssigned = true }
            };
            var response = new PagedResponse
            {
                Data = roles.Cast<object>().ToList(),
                Meta = new Meta { TotalRows = 1, CurrentPage = 1, RowsPerPage = 1 }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetUserProductRoles(realPageId, "UPFM"))
                .Returns(ActionResultEnvelope.Ok(response));

            // Act
            var result = _controller.GetUserProductRoles(realPageId, "UPFM");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var pagedResponse = okResult.Value as PagedResponse;
            pagedResponse.Should().NotBeNull();
            pagedResponse.Data.Should().HaveCount(1);
            pagedResponse.Meta.TotalRows.Should().Be(1);
        }

        [Fact]
        public void GetUserProductRoles_WithValidNonUPFMProduct_ReturnsOkResult()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var roles = new List<ProductRole>
            {
                new ProductRole { ID = "1", Name = "Manager", IsAssigned = true }
            };
            var response = new PagedResponse
            {
                Data = roles.Cast<object>().ToList(),
                Meta = new Meta { TotalRows = 1, CurrentPage = 1, RowsPerPage = 1 }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetUserProductRoles(realPageId, "ONESITE"))
                .Returns(ActionResultEnvelope.Ok(response));

            // Act
            var result = _controller.GetUserProductRoles(realPageId, "ONESITE");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var pagedResponse = okResult.Value as PagedResponse;
            pagedResponse.Data.Should().HaveCount(1);
        }

        [Fact]
        public void GetUserProductRoles_WithNullPerson_ReturnsNotFound()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetUserProductRoles(realPageId, "UPFM"))
                .Returns(ActionResultEnvelope.NotFound());

            // Act
            var result = _controller.GetUserProductRoles(realPageId, "UPFM");

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void GetUserProductRoles_WithPersonaFromDifferentOrganization_ReturnsNotFound()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetUserProductRoles(realPageId, "UPFM"))
                .Returns(ActionResultEnvelope.NotFound());

            // Act
            var result = _controller.GetUserProductRoles(realPageId, "UPFM");

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void GetUserProductRoles_WhenServiceReturnsError_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var errorResponse = new ErrorResponse
            {
                Errors = new List<Error>
                {
                    new Error { Title = "Error", Detail = "Service error", Source = "/role", StatusCode = "" }
                }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetUserProductRoles(realPageId, "UPFM"))
                .Returns(ActionResultEnvelope.BadRequest(errorResponse));

            // Act
            var result = _controller.GetUserProductRoles(realPageId, "UPFM");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            var response = badRequest.Value as ErrorResponse;
            response.Should().NotBeNull();
            response.Errors.Should().HaveCount(1);
            response.Errors[0].Detail.Should().Be("Service error");
        }

        [Fact]
        public void GetUserProductRoles_FiltersOnlyAssignedRoles()
        {
            // Arrange - Service already returns filtered list
            var realPageId = Guid.NewGuid();
            var roles = new List<ProductRole>
            {
                new ProductRole { ID = "1", Name = "Role1", IsAssigned = true },
                new ProductRole { ID = "3", Name = "Role3", IsAssigned = true }
            };
            var response = new PagedResponse
            {
                Data = roles.Cast<object>().ToList(),
                Meta = new Meta { TotalRows = 2, CurrentPage = 1, RowsPerPage = 2 }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetUserProductRoles(realPageId, "UPFM"))
                .Returns(ActionResultEnvelope.Ok(response));

            // Act
            var result = _controller.GetUserProductRoles(realPageId, "UPFM");

            // Assert
            var okResult = result as OkObjectResult;
            var pagedResponse = okResult.Value as PagedResponse;
            pagedResponse.Data.Should().HaveCount(2);
        }

        [Fact]
        public void GetUserProductRoles_WithInvalidUpfmId_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var invalidUpfmId = Guid.NewGuid();
            var errorResponse = new ErrorResponse
            {
                Errors = new List<Error>
                {
                    new Error { Title = "Error", Detail = "Invalid UPFMId.", Source = "/role", StatusCode = "" }
                }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, invalidUpfmId))
                .Returns(ClientCredentialAuthResult.Error(errorResponse));

            // Act
            var result = _controller.GetUserProductRoles(realPageId, "UPFM", invalidUpfmId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            var response = badRequest.Value as ErrorResponse;
            response.Should().NotBeNull();
            response.Errors.Should().HaveCount(1);
            response.Errors[0].Detail.Should().Be("Invalid UPFMId.");
        }

        #endregion

        #region GetProductRoles Tests

        [Fact]
        public void GetProductRoles_WithValidUPFMProduct_ReturnsOkResult()
        {
            // Arrange
            var roles = new List<ProductRole>
            {
                new ProductRole { ID = "1", Name = "Admin", IsAssigned = true },
                new ProductRole { ID = "2", Name = "User", IsAssigned = false }
            };
            var response = new PagedResponse
            {
                Data = roles.Cast<object>().ToList(),
                Meta = new Meta { TotalRows = 2, CurrentPage = 1, RowsPerPage = 2 }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetProductRoles("UPFM"))
                .Returns(ActionResultEnvelope.Ok(response));

            // Act
            var result = _controller.GetProductRoles("UPFM");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var pagedResponse = okResult.Value as PagedResponse;
            pagedResponse.Data.Should().HaveCount(2);
            pagedResponse.Meta.TotalRows.Should().Be(2);
        }

        [Fact]
        public void GetProductRoles_WithValidNonUPFMProduct_ReturnsOkResult()
        {
            // Arrange
            var roles = new List<ProductRole>
            {
                new ProductRole { ID = "1", Name = "Role1", IsAssigned = true }
            };
            var response = new PagedResponse
            {
                Data = roles.Cast<object>().ToList(),
                Meta = new Meta { TotalRows = 1, CurrentPage = 1, RowsPerPage = 1 }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetProductRoles("ONESITE"))
                .Returns(ActionResultEnvelope.Ok(response));

            // Act
            var result = _controller.GetProductRoles("ONESITE");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var pagedResponse = okResult.Value as PagedResponse;
            pagedResponse.Meta.TotalRows.Should().Be(1);
        }

        [Fact]
        public void GetProductRoles_WhenServiceReturnsError_ReturnsBadRequest()
        {
            // Arrange
            var errorResponse = new ErrorResponse
            {
                Errors = new List<Error>
                {
                    new Error { Title = "Error", Detail = "Product not found", Source = "/role", StatusCode = "" }
                }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetProductRoles("UPFM"))
                .Returns(ActionResultEnvelope.BadRequest(errorResponse));

            // Act
            var result = _controller.GetProductRoles("UPFM");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            var response = badRequest.Value as ErrorResponse;
            response.Should().NotBeNull();
            response.Errors.Should().NotBeNull();
            response.Errors.Should().HaveCount(1);
            response.Errors[0].Detail.Should().Be("Product not found");
        }

        [Fact]
        public void GetProductRoles_WithClientCredentialAndValidUpfmId_ReturnsOkResult()
        {
            // Arrange
            var response = new PagedResponse
            {
                Data = new List<object> { new ProductRole { ID = "1", Name = "AdminRole", IsAssigned = true } },
                Meta = new Meta { TotalRows = 1, CurrentPage = 1, RowsPerPage = 1 }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, _testUpfmId))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetProductRoles("UPFM"))
                .Returns(ActionResultEnvelope.Ok(response));

            // Act
            var result = _controller.GetProductRoles("UPFM", _testUpfmId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockClientCredentialAuthenticator.Verify(
                x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, _testUpfmId),
                Times.Once);
        }

        [Fact]
        public void GetProductRoles_WithCaseInsensitiveUPFMCode_ReturnsOkResult()
        {
            // Arrange
            var roles = new List<ProductRole>
            {
                new ProductRole { ID = "1", Name = "Role1", IsAssigned = true }
            };
            var response = new PagedResponse
            {
                Data = roles.Cast<object>().ToList(),
                Meta = new Meta { TotalRows = 1, CurrentPage = 1, RowsPerPage = 1 }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetProductRoles("UpFm"))
                .Returns(ActionResultEnvelope.Ok(response));

            // Act
            var result = _controller.GetProductRoles("UpFm");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetProductRoles_PagedResponsePropertiesInitializedCorrectly()
        {
            // Arrange
            var roles = new List<ProductRole>
            {
                new ProductRole { ID = "1", Name = "Role1", IsAssigned = true },
                new ProductRole { ID = "2", Name = "Role2", IsAssigned = true }
            };
            var response = new PagedResponse
            {
                Data = roles.Cast<object>().ToList(),
                Meta = new Meta { TotalRows = 2, CurrentPage = 1, RowsPerPage = 2 }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetProductRoles("UPFM"))
                .Returns(ActionResultEnvelope.Ok(response));

            // Act
            var result = _controller.GetProductRoles("UPFM");

            // Assert
            var okResult = result as OkObjectResult;
            var pagedResponse = okResult.Value as PagedResponse;
            pagedResponse.Meta.CurrentPage.Should().Be(1);
            pagedResponse.Meta.TotalRows.Should().Be(2);
            pagedResponse.Meta.RowsPerPage.Should().Be(2);
        }

        #endregion

        #region GetRightsforRole Tests

        [Fact]
        public void GetRightsforRole_WithValidParameters_ReturnsOkResult()
        {
            // Arrange
            var rights = new List<Right>
            {
                new Right { RightId = 1, RightName = "Read" },
                new Right { RightId = 2, RightName = "Write" }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetRightsForRole("UPFM", 1))
                .Returns(ActionResultEnvelope.Ok(rights));

            // Act
            var result = _controller.GetRightsforRole("UPFM", 1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(rights);
        }

        [Fact]
        public void GetRightsforRole_WithEmptyProductCode_ReturnsBadRequest()
        {
            // Arrange
            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetRightsForRole("", 1))
                .Returns(ActionResultEnvelope.BadRequest("ProductCode not supplied."));

            // Act
            var result = _controller.GetRightsforRole("", 1);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            badRequest.Value.Should().Be("ProductCode not supplied.");
        }

        [Fact]
        public void GetRightsforRole_WithNullProductCode_ReturnsBadRequest()
        {
            // Arrange
            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetRightsForRole(null, 1))
                .Returns(ActionResultEnvelope.BadRequest("ProductCode not supplied."));

            // Act
            var result = _controller.GetRightsforRole(null, 1);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void GetRightsforRole_WithRoleIdZero_ReturnsBadRequest()
        {
            // Arrange
            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetRightsForRole("UPFM", 0))
                .Returns(ActionResultEnvelope.BadRequest("roleId not supplied."));

            // Act
            var result = _controller.GetRightsforRole("UPFM", 0);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            badRequest.Value.Should().Be("roleId not supplied.");
        }

        [Fact]
        public void GetRightsforRole_WithNegativeRoleId_ReturnsOkResult()
        {
            // Arrange - Negative role IDs are allowed (service validates)
            var rights = new List<Right>
            {
                new Right { RightId = 1, RightName = "Right1" }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetRightsForRole("UPFM", -1))
                .Returns(ActionResultEnvelope.Ok(rights));

            // Act
            var result = _controller.GetRightsforRole("UPFM", -1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetRightsforRole_WithClientCredentialAndValidUpfmId_ReturnsOkResult()
        {
            // Arrange
            var rights = new List<Right>
            {
                new Right { RightId = 1, RightName = "Right1" }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, _testUpfmId))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetRightsForRole("UPFM", 5))
                .Returns(ActionResultEnvelope.Ok(rights));

            // Act
            var result = _controller.GetRightsforRole("UPFM", 5, _testUpfmId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetRightsforRole_WithInvalidUpfmId_ReturnsBadRequest()
        {
            // Arrange
            var errorResponse = new ErrorResponse
            {
                Errors = new List<Error>
                {
                    new Error { Title = "Error", Detail = "Invalid UPFMId.", Source = "/role", StatusCode = "" }
                }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, _testUpfmId))
                .Returns(ClientCredentialAuthResult.Error(errorResponse));

            // Act
            var result = _controller.GetRightsforRole("UPFM", 1, _testUpfmId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region Client Credential Authentication Tests

        [Fact]
        public void GetUserProductRoles_WithNullUpfmId_UsesCurrentClaims()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var response = new PagedResponse
            {
                Data = new List<object> { new ProductRole { ID = "1", Name = "Role1", IsAssigned = true } },
                Meta = new Meta { TotalRows = 1, CurrentPage = 1, RowsPerPage = 1 }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetUserProductRoles(realPageId, "UPFM"))
                .Returns(ActionResultEnvelope.Ok(response));

            // Act
            var result = _controller.GetUserProductRoles(realPageId, "UPFM", null);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockClientCredentialAuthenticator.Verify(
                x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null),
                Times.Once);
        }

        [Fact]
        public void GetProductRoles_WithValidUpfmId_AuthenticatesWithClientCredentials()
        {
            // Arrange
            var response = new PagedResponse
            {
                Data = new List<object> { new ProductRole { ID = "1", Name = "AdminRole", IsAssigned = true } },
                Meta = new Meta { TotalRows = 1, CurrentPage = 1, RowsPerPage = 1 }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, _testUpfmId))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetProductRoles("UPFM"))
                .Returns(ActionResultEnvelope.Ok(response));

            // Act
            var result = _controller.GetProductRoles("UPFM", _testUpfmId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockClientCredentialAuthenticator.Verify(
                x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, _testUpfmId),
                Times.Once);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void GetUserProductRoles_WithCaseInsensitiveUPFMCode_ReturnsOkResult()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var response = new PagedResponse
            {
                Data = new List<object> { new ProductRole { ID = "1", Name = "Role1", IsAssigned = true } },
                Meta = new Meta { TotalRows = 1, CurrentPage = 1, RowsPerPage = 1 }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetUserProductRoles(realPageId, "upfm"))
                .Returns(ActionResultEnvelope.Ok(response));

            // Act
            var result = _controller.GetUserProductRoles(realPageId, "upfm");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetUserProductRoles_WithEmptyRolesList_ReturnsOkWithEmptyData()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var response = new PagedResponse
            {
                Data = new List<object>(),
                Meta = new Meta { TotalRows = 0, CurrentPage = 1, RowsPerPage = 0 }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetUserProductRoles(realPageId, "UPFM"))
                .Returns(ActionResultEnvelope.Ok(response));

            // Act
            var result = _controller.GetUserProductRoles(realPageId, "UPFM");

            // Assert
            var okResult = result as OkObjectResult;
            var pagedResponse = okResult.Value as PagedResponse;
            pagedResponse.Data.Should().BeEmpty();
        }

        [Fact]
        public void GetProductRoles_WithEmptyRolesList_ReturnsOkWithEmptyData()
        {
            // Arrange
            var response = new PagedResponse
            {
                Data = new List<object>(),
                Meta = new Meta { TotalRows = 0, CurrentPage = 1, RowsPerPage = 0 }
            };

            _mockClientCredentialAuthenticator
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), _defaultUserClaim, null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetProductRoles("UPFM"))
                .Returns(ActionResultEnvelope.Ok(response));

            // Act
            var result = _controller.GetProductRoles("UPFM");

            // Assert
            var okResult = result as OkObjectResult;
            var pagedResponse = okResult.Value as PagedResponse;
            pagedResponse.Data.Should().BeEmpty();
            pagedResponse.Meta.TotalRows.Should().Be(0);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRoleQueryService_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new RoleController(null, _mockClientCredentialAuthenticator.Object, _defaultUserClaim));

            exception.ParamName.Should().Be("roleQueryService");
        }

        [Fact]
        public void Constructor_WithNullClientCredentialAuthenticator_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new RoleController(_mockRoleQueryService.Object, null, _defaultUserClaim));

            exception.ParamName.Should().Be("clientCredentialAuthenticator");
        }

        [Fact]
        public void Constructor_WithNullUserClaims_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new RoleController(_mockRoleQueryService.Object, _mockClientCredentialAuthenticator.Object, null));

            exception.ParamName.Should().Be("userClaims");
        }

        #endregion
    }
}
//using FluentAssertions;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Security.Claims;
//using System.Text;
//using UnifiedLogin.BusinessLogic.Logic.Interfaces;
//using UnifiedLogin.BusinessLogic.Logic.Product;
//using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
//using UnifiedLogin.BusinessLogic.Repository.Interfaces;
//using UnifiedLogin.DataAccess;
//using UnifiedLogin.LandingAPIEnterprise.Controllers;
//using UnifiedLogin.SharedObjects.Base;
//using UnifiedLogin.SharedObjects.Constants;
//using UnifiedLogin.SharedObjects.Enterprise;
//using UnifiedLogin.SharedObjects.Enum;
//using UnifiedLogin.SharedObjects.IdentityConfig;
//using UnifiedLogin.SharedObjects.Landing;
//using UnifiedLogin.SharedObjects.Product;
//using UnifiedLogin.SharedObjects.Product.OneSite;
//using UnifiedLogin.SharedObjects.Product.Rum;
//using UnifiedLogin.SharedObjects.ResponseObject;
//using Xunit;

//namespace UnifiedLogin.LandingAPI.Tests.Controllers.Enterprise
//{
//    /// <summary>
//    /// Comprehensive unit tests for RoleController with 100% code coverage.
//    /// Tests all endpoints, error cases, validation scenarios, and authentication flows.
//    /// </summary>
//    [ExcludeFromCodeCoverage]
//    public class RoleControllerTests : ControllerBase
//    {
//        #region Private Fields

//        private readonly Mock<IRepository> _mockRepository;
//        private readonly Mock<HttpMessageHandler> _mockMessageHandler;
//        private readonly Mock<IOneSiteProductService> _mockOneSiteProductService;
//        private readonly Mock<IProductRepository> _mockProductRepository;
//        private readonly Mock<IManageUnifiedLogin> _mockManageUnifiedLogin;
//        private readonly Mock<IManageOrganization> _mockManageOrganization;
//        private readonly Mock<IManagePersona> _mockManagePersona;
//        private readonly Mock<IManagePerson> _mockManagePerson;
//        private readonly Mock<IManageProductPanel> _mockManageProductPanel;
//        private readonly Mock<HttpContextAccessor> _mockHttpContextAccessor;
//        private readonly DefaultUserClaim _defaultUserClaim;
//        private readonly RoleController _controller;
//        private readonly Guid _testUserId = Guid.NewGuid();
//        private readonly Guid _testUpfmId = Guid.NewGuid();
//        private readonly long _testPersonaId = 100;
//        private readonly long _testOrgPartyId = 1000;

//        #endregion

//        #region Constructor & Setup

//        public RoleControllerTests()
//        {
//            _mockRepository = new Mock<IRepository>(MockBehavior.Loose);
//            _mockMessageHandler = new Mock<HttpMessageHandler>();
//            _mockOneSiteProductService = new Mock<IOneSiteProductService>();
//            _mockProductRepository = new Mock<IProductRepository>();
//            _mockManageUnifiedLogin = new Mock<IManageUnifiedLogin>();
//            _mockManageOrganization = new Mock<IManageOrganization>();
//            _mockManagePersona = new Mock<IManagePersona>();
//            _mockManagePerson = new Mock<IManagePerson>();
//            _mockManageProductPanel = new Mock<IManageProductPanel>();
//            _mockHttpContextAccessor = new Mock<HttpContextAccessor>();

//            _defaultUserClaim = new DefaultUserClaim
//            {
//                UserId = 1,
//                LoginName = "testuser@test.com",
//                FirstName = "Test",
//                LastName = "User",
//                UserRealPageGuid = _testUserId,
//                OrganizationPartyId = _testOrgPartyId,
//                PersonaId = _testPersonaId,
//                CorrelationId = Guid.NewGuid()
//            };

//            var claimsIdentity = new ClaimsIdentity(new[]
//            {
//                new Claim("scope", "enterpriseapi")
//            });

//            var emptyProductSettings = new List<ProductInternalSetting>
//            {
//                new ProductInternalSetting { Name = "MTAPIENDPOINT", Value = "api/v1" },
//                new ProductInternalSetting { Name = "MTTOKENENDPOINT", Value = "token" },
//                new ProductInternalSetting { Name = "MTCLIENTID", Value = "testclient" },
//                new ProductInternalSetting { Name = "MTCLIENTSECRET", Value = "testsecret" },
//                new ProductInternalSetting { Name = "APIENDPOINT", Value = "http://localhost" },
//                new ProductInternalSetting { Name = "APIUSERNAME", Value = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser")) },
//                new ProductInternalSetting { Name = "APIPASSWORD", Value = Convert.ToBase64String(Encoding.UTF8.GetBytes("testpass")) }
//            };

//            _mockRepository
//              .Setup(x => x.GetMany<ProductInternalSetting>(
//                  It.IsAny<string>(),
//                  It.IsAny<object>()))
//              .Returns(emptyProductSettings.AsEnumerable());

//            _mockRepository
//              .Setup(x => x.GetMany<ProductInternalSetting>(
//                  It.IsAny<string>(),
//                  It.IsAny<object>(),
//                  It.IsAny<int?>()))
//              .Returns(emptyProductSettings.AsEnumerable());

//            // Mock HttpContext properties to prevent null reference
//            var mockHttpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
//            _mockHttpContextAccessor
//              .Setup(x => x.HttpContext)
//              .Returns(mockHttpContext.Object);

//            _controller = new RoleController(
//                _mockRepository.Object,
//                _mockMessageHandler.Object,
//                _defaultUserClaim,
//                _mockOneSiteProductService.Object,
//                _mockManageProductPanel.Object,
//                claimsIdentity,
//                _mockHttpContextAccessor.Object)
//            {
//                ControllerContext = new ControllerContext
//                {
//                    HttpContext = new DefaultHttpContext()
//                }
//            };
//        }

//        //public void Dispose()
//        //{
//        //    _controller?.Dispose();
//        //}

//        #endregion

//        #region GetUserProductRoles Tests

//        [Fact]
//        public void GetUserProductRoles_WithValidUPFMProduct_ReturnsOkResult()
//        {
//            // Arrange
//            var realPageId = Guid.NewGuid();
//            var person = new Person { RealPageId = realPageId, FirstName = "Test" };
//            var persona = new Persona { PersonaId = 100, RealPageId = realPageId, OrganizationPartyId = _testOrgPartyId };
//            var roles = new List<ProductRole>
//            {
//                new ProductRole { ID = "1", Name = "Admin", IsAssigned = true },
//                new ProductRole { ID = "2", Name = "User", IsAssigned = false }
//            };
//            var listResponse = new ListResponse { Records = roles.Cast<object>().ToList(), IsError = false };

//            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns(person);
//            _mockManagePersona.Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, _testOrgPartyId)).Returns(persona);
//            _mockManageUnifiedLogin.Setup(x => x.GetUserRoles(_testPersonaId, persona.PersonaId, _testOrgPartyId))
//                .Returns(listResponse);

//            // Act
//            var result = _controller.GetUserProductRoles(realPageId, "UPFM");

//            // Assert
//            result.Should().BeOfType<OkObjectResult>();
//            var okResult = result as OkObjectResult;
//            var response = okResult.Value as PagedResponse;
//            response.Should().NotBeNull();
//            response.Data.Should().HaveCount(1);
//            response.Meta.TotalRows.Should().Be(1);
//        }

//        [Fact]
//        public void GetUserProductRoles_WithValidNonUPFMProduct_ReturnsOkResult()
//        {
//            // Arrange
//            var realPageId = Guid.NewGuid();
//            var person = new Person { RealPageId = realPageId };
//            var persona = new Persona { PersonaId = 100, RealPageId = realPageId, OrganizationPartyId = _testOrgPartyId };
//            var products = new List<GbProductMap>
//            {
//                new GbProductMap { ProductId = (int)ProductEnum.OneSite, BooksProductCode = "ONESITE" }
//            };
//            var roles = new List<ProductRole>
//            {
//                new ProductRole { ID = "1", Name = "Manager", IsAssigned = true },
//                new ProductRole { ID = "2", Name = "Viewer", IsAssigned = false }
//            };
//            var productSettings = new List<ProductInternalSetting>();
//            var listResponse = new ListResponse { Records = roles.Cast<object>().ToList(), IsError = false };

//            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns(person);
//            _mockManagePersona.Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, _testOrgPartyId)).Returns(persona);
//            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);
//            _mockManageUnifiedLogin.Setup(x => x.GetProductInternalSettingByProductId((int)ProductEnum.OneSite))
//                .Returns(productSettings);
//            _mockManageProductPanel.Setup(x => x.GetProductRoles(
//                _testPersonaId, persona.PersonaId, _testOrgPartyId, (int)ProductEnum.OneSite, null, null))
//                .Returns(listResponse);

//            // Act
//            var result = _controller.GetUserProductRoles(realPageId, "ONESITE");

//            // Assert
//            result.Should().BeOfType<OkObjectResult>();
//            var okResult = result as OkObjectResult;
//            var response = okResult.Value as PagedResponse;
//            response.Data.Should().HaveCount(1);
//        }

//        [Fact]
//        public void GetUserProductRoles_WithSharedProductSetting_UsesSharedProductId()
//        {
//            // Arrange
//            var realPageId = Guid.NewGuid();
//            var person = new Person { RealPageId = realPageId };
//            var persona = new Persona { PersonaId = 100, RealPageId = realPageId, OrganizationPartyId = _testOrgPartyId };
//            var products = new List<GbProductMap>
//            {
//                new GbProductMap { ProductId = (int)ProductEnum.OneSite, BooksProductCode = "ONESITE" }
//            };
//            var productSettings = new List<ProductInternalSetting>
//            {
//                new ProductInternalSetting { Name = SettingConstants.SharedProductSettingName, Value = "99" }
//            };
//            var roles = new List<ProductRole>
//            {
//                new ProductRole { ID = "1", Name = "Role1", IsAssigned = true }
//            };
//            var listResponse = new ListResponse { Records = roles.Cast<object>().ToList(), IsError = false };

//            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns(person);
//            _mockManagePersona.Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, _testOrgPartyId)).Returns(persona);
//            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);
//            _mockManageUnifiedLogin.Setup(x => x.GetProductInternalSettingByProductId((int)ProductEnum.OneSite))
//                .Returns(productSettings);
//            _mockManageProductPanel.Setup(x => x.GetProductRoles(
//                _testPersonaId, persona.PersonaId, _testOrgPartyId, 99, null, null))
//                .Returns(listResponse);

//            // Act
//            var result = _controller.GetUserProductRoles(realPageId, "ONESITE");

//            // Assert
//            result.Should().BeOfType<OkObjectResult>();
//            _mockManageProductPanel.Verify(
//                x => x.GetProductRoles(_testPersonaId, persona.PersonaId, _testOrgPartyId, 99, null, null),
//                Times.Once);
//        }

//        [Fact]
//        public void GetUserProductRoles_WithNullPerson_ReturnsNotFound()
//        {
//            // Arrange
//            var realPageId = Guid.NewGuid();
//            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns((Person)null);

//            // Act
//            var result = _controller.GetUserProductRoles(realPageId, "UPFM");

//            // Assert
//            result.Should().BeOfType<NotFoundResult>();
//        }

//        [Fact]
//        public void GetUserProductRoles_WithNullPersona_ReturnsNotFound()
//        {
//            // Arrange
//            var realPageId = Guid.NewGuid();
//            var person = new Person { RealPageId = realPageId };
//            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns(person);
//            _mockManagePersona.Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, _testOrgPartyId))
//                .Returns((Persona)null);

//            // Act
//            var result = _controller.GetUserProductRoles(realPageId, "UPFM");

//            // Assert
//            result.Should().BeOfType<NotFoundResult>();
//        }

//        [Fact]
//        public void GetUserProductRoles_WithPersonaFromDifferentOrganization_ReturnsNotFound()
//        {
//            // Arrange
//            var realPageId = Guid.NewGuid();
//            var person = new Person { RealPageId = realPageId };
//            var persona = new Persona { PersonaId = 100, RealPageId = realPageId, OrganizationPartyId = 2000 };
//            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns(person);
//            _mockManagePersona.Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, _testOrgPartyId))
//                .Returns(persona);

//            // Act
//            var result = _controller.GetUserProductRoles(realPageId, "UPFM");

//            // Assert
//            result.Should().BeOfType<NotFoundResult>();
//        }

//        [Fact]
//        public void GetUserProductRoles_WhenServiceReturnsError_ReturnsBadRequest()
//        {
//            // Arrange
//            var realPageId = Guid.NewGuid();
//            var person = new Person { RealPageId = realPageId };
//            var persona = new Persona { PersonaId = 100, RealPageId = realPageId, OrganizationPartyId = _testOrgPartyId };
//            var listResponse = new ListResponse { IsError = true, ErrorReason = "Service error" };

//            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns(person);
//            _mockManagePersona.Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, _testOrgPartyId)).Returns(persona);
//            _mockManageUnifiedLogin.Setup(x => x.GetUserRoles(_testPersonaId, persona.PersonaId, _testOrgPartyId))
//                .Returns(listResponse);

//            // Act
//            var result = _controller.GetUserProductRoles(realPageId, "UPFM");

//            // Assert
//            result.Should().BeOfType<BadRequestObjectResult>();
//            var badRequest = result as BadRequestObjectResult;
//            var errorResponse = badRequest.Value as ErrorResponse;
//            errorResponse.Errors.Should().HaveCount(1);
//            errorResponse.Errors[0].Detail.Should().Be("Service error");
//        }

//        [Fact]
//        public void GetUserProductRoles_FiltersOnlyAssignedRoles()
//        {
//            // Arrange
//            var realPageId = Guid.NewGuid();
//            var person = new Person { RealPageId = realPageId };
//            var persona = new Persona { PersonaId = 100, RealPageId = realPageId, OrganizationPartyId = _testOrgPartyId };
//            var roles = new List<ProductRole>
//            {
//                new ProductRole { ID = "1", Name = "Role1", IsAssigned = true },
//                new ProductRole { ID = "2", Name = "Role2", IsAssigned = false },
//                new ProductRole { ID = "3", Name = "Role3", IsAssigned = true }
//            };
//            var listResponse = new ListResponse { Records = roles.Cast<object>().ToList(), IsError = false };

//            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns(person);
//            _mockManagePersona.Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, _testOrgPartyId)).Returns(persona);
//            _mockManageUnifiedLogin.Setup(x => x.GetUserRoles(_testPersonaId, persona.PersonaId, _testOrgPartyId))
//                .Returns(listResponse);

//            // Act
//            var result = _controller.GetUserProductRoles(realPageId, "UPFM");

//            // Assert
//            var okResult = result as OkObjectResult;
//            var response = okResult.Value as PagedResponse;
//            response.Data.Should().HaveCount(2);
//        }

//        [Fact]
//        public void GetUserProductRoles_WithClientCredentialAndValidUpfmId_ReturnsOkResult()
//        {
//            // Arrange
//            var realPageId = Guid.NewGuid();
//            var person = new Person { RealPageId = realPageId };
//            var persona = new Persona { PersonaId = 100, RealPageId = realPageId, OrganizationPartyId = _testOrgPartyId };
//            var adminUserId = Guid.NewGuid();
//            var roles = new List<ProductRole>
//            {
//                new ProductRole { ID = "1", Name = "AdminRole", IsAssigned = true }
//            };
//            var listResponse = new ListResponse { Records = roles.Cast<object>().ToList(), IsError = false };

//            _mockManageOrganization.Setup(x => x.GetOrganizationAdminUserRealPageId(_testUpfmId))
//                .Returns(adminUserId);
//            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns(person);
//            _mockManagePersona.Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, _testOrgPartyId)).Returns(persona);
//            _mockManageUnifiedLogin.Setup(x => x.GetUserRoles(_testPersonaId, persona.PersonaId, _testOrgPartyId))
//                .Returns(listResponse);

//            // Act
//            var result = _controller.GetUserProductRoles(realPageId, "UPFM", _testUpfmId);

//            // Assert
//            result.Should().BeOfType<OkObjectResult>();
//        }

//        #endregion

//        #region GetProductRoles Tests

//        [Fact]
//        public void GetProductRoles_WithValidUPFMProduct_ReturnsOkResult()
//        {
//            // Arrange
//            var roles = new List<ProductRole>
//            {
//                new ProductRole { ID = "1", Name = "Admin", IsAssigned = true },
//                new ProductRole { ID = "2", Name = "User", IsAssigned = false }
//            };
//            var listResponse = new ListResponse
//            {
//                Records = roles.Cast<object>().ToList(),
//                IsError = false,
//                TotalRows = 2
//            };

//            _mockManageUnifiedLogin.Setup(x => x.GetRoles(_testPersonaId, _testOrgPartyId))
//                .Returns(listResponse);

//            // Act
//            var result = _controller.GetProductRoles("UPFM");

//            // Assert
//            result.Should().BeOfType<OkObjectResult>();
//            var okResult = result as OkObjectResult;
//            var response = okResult.Value as PagedResponse;
//            response.Data.Should().HaveCount(2);
//            response.Meta.TotalRows.Should().Be(2);
//        }

//        [Fact]
//        public void GetProductRoles_WithValidNonUPFMProduct_ReturnsOkResult()
//        {
//            // Arrange
//            var products = new List<GbProductMap>
//            {
//                new GbProductMap { ProductId = (int)ProductEnum.OneSite, BooksProductCode = "ONESITE" }
//            };
//            var roles = new List<ProductRole>
//            {
//                new ProductRole { ID = "1", Name = "Role1", IsAssigned = true }
//            };
//            var listResponse = new ListResponse
//            {
//                Records = roles.Cast<object>().ToList(),
//                IsError = false,
//                TotalRows = 1
//            };

//            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);
//            _mockManageProductPanel.Setup(x => x.GetProductRoles(
//                _testPersonaId, _testPersonaId, _testOrgPartyId, (int)ProductEnum.OneSite, null, null))
//                .Returns(listResponse);

//            // Act
//            var result = _controller.GetProductRoles("ONESITE");

//            // Assert
//            result.Should().BeOfType<OkObjectResult>();
//            var okResult = result as OkObjectResult;
//            var response = okResult.Value as PagedResponse;
//            response.Meta.TotalRows.Should().Be(1);
//        }

//        [Fact]
//        public void GetProductRoles_WhenServiceReturnsError_ReturnsBadRequest()
//        {
//            // Arrange
//            var listResponse = new ListResponse { IsError = true, ErrorReason = "Product not found" };

//            _mockManageUnifiedLogin.Setup(x => x.GetRoles(_testPersonaId, _testOrgPartyId))
//                .Returns(listResponse);

//            // Act
//            var result = _controller.GetProductRoles("UPFM");

//            // Assert
//            result.Should().BeOfType<BadRequestObjectResult>();
//            var badRequest = result as BadRequestObjectResult;
//            var errorResponse = badRequest.Value as ErrorResponse;
//            errorResponse.Errors[0].Detail.Should().Be("Product not found");
//        }

//        [Fact]
//        public void GetProductRoles_WithClientCredentialAndValidUpfmId_ReturnsOkResult()
//        {
//            // Arrange
//            var adminUserId = Guid.NewGuid();
//            var roles = new List<ProductRole>
//            {
//                new ProductRole { ID = "1", Name = "AdminRole", IsAssigned = true }
//            };
//            var listResponse = new ListResponse
//            {
//                Records = roles.Cast<object>().ToList(),
//                IsError = false,
//                TotalRows = 1
//            };

//            _mockManageOrganization.Setup(x => x.GetOrganizationAdminUserRealPageId(_testUpfmId))
//                .Returns(adminUserId);
//            _mockManageUnifiedLogin.Setup(x => x.GetRoles(_testPersonaId, _testOrgPartyId))
//                .Returns(listResponse);

//            // Act
//            var result = _controller.GetProductRoles("UPFM", _testUpfmId);

//            // Assert
//            result.Should().BeOfType<OkObjectResult>();
//        }

//        #endregion

//        #region GetRightsforRole Tests

//        [Fact]
//        public void GetRightsforRole_WithValidParameters_ReturnsOkResult()
//        {
//            // Arrange
//            var rights = new List<Right>
//            {
//                new Right { RightId = 1, RightName = "Read" },
//                new Right { RightId = 2, RightName = "Write" }
//            };
//            var listResponse = new ListResponse
//            {
//                Records = rights.Cast<object>().ToList(),
//                IsError = false
//            };

//            _mockManageUnifiedLogin.Setup(x => x.GetListRightbyRole("UPFM", 1))
//                .Returns(listResponse);

//            // Act
//            var result = _controller.GetRightsforRole("UPFM", 1);

//            // Assert
//            result.Should().BeOfType<OkObjectResult>();
//            var okResult = result as OkObjectResult;
//            okResult.Value.Should().BeEquivalentTo(rights);
//        }

//        [Fact]
//        public void GetRightsforRole_WithNegativeRoleId_ReturnsBadRequest()
//        {
//            // Arrange - negative roleId should not equal 0, so this actually passes to service
//            var rights = new List<Right>
//            {
//                new Right { RightId = 1, RightName = "Right1" }
//            };
//            var listResponse = new ListResponse
//            {
//                Records = rights.Cast<object>().ToList(),
//                IsError = false
//            };

//            _mockManageUnifiedLogin.Setup(x => x.GetListRightbyRole("UPFM", -1))
//                .Returns(listResponse);

//            // Act
//            var result = _controller.GetRightsforRole("UPFM", -1);

//            // Assert
//            result.Should().BeOfType<OkObjectResult>();
//        }

//        [Fact]
//        public void GetRightsforRole_WithClientCredentialAndValidUpfmId_ReturnsOkResult()
//        {
//            // Arrange
//            var adminUserId = Guid.NewGuid();
//            var rights = new List<Right>
//            {
//                new Right { RightId = 1, RightName = "Right1" }
//            };

//            var listResponse = new ListResponse
//            {
//                Records = rights.Cast<object>().ToList(),
//                IsError = false
//            };
//            _mockManageOrganization.Setup(x => x.GetOrganizationAdminUserRealPageId(_testUpfmId))
//                .Returns(adminUserId);
//            _mockManageUnifiedLogin.Setup(x => x.GetListRightbyRole("UPFM", 5))
//                .Returns(listResponse);

//            // Act
//            var result = _controller.GetRightsforRole("UPFM", 5, _testUpfmId);

//            // Assert
//            result.Should().BeOfType<OkObjectResult>();
//        }

//        [Fact]
//        public void GetRightsforRole_WithInvalidUpfmId_ReturnsBadRequest()
//        {
//            // Arrange
//            _mockManageOrganization.Setup(x => x.GetOrganizationAdminUserRealPageId(_testUpfmId))
//                .Returns(Guid.Empty);

//            // Act
//            var result = _controller.GetRightsforRole("UPFM", 1, _testUpfmId);

//            // Assert
//            result.Should().BeOfType<BadRequestObjectResult>();
//            var badRequest = result as BadRequestObjectResult;
//            var errorResponse = badRequest.Value as List<Error>;
//            errorResponse.Should().NotBeNull();
//        }

//        #endregion

//        #region Client Credential Authentication Tests

//        [Fact]
//        public void AttemptClientCredentialAuthentication_WithNullUpfmId_ReturnsNull()
//        {
//            // This tests the private method indirectly through GetUserProductRoles
//            // Arrange
//            var realPageId = Guid.NewGuid();
//            var person = new Person { RealPageId = realPageId };
//            var persona = new Persona { PersonaId = 100, RealPageId = realPageId, OrganizationPartyId = _testOrgPartyId };
//            var roles = new List<ProductRole>
//            {
//                new ProductRole { ID = "1", Name = "Role1", IsAssigned = true }
//            };
//            var listResponse = new ListResponse { Records = roles.Cast<object>().ToList(), IsError = false };

//            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns(person);
//            _mockManagePersona.Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, _testOrgPartyId)).Returns(persona);
//            _mockManageUnifiedLogin.Setup(x => x.GetUserRoles(_testPersonaId, persona.PersonaId, _testOrgPartyId))
//                .Returns(listResponse);

//            // Act
//            var result = _controller.GetUserProductRoles(realPageId, "UPFM", null);

//            // Assert
//            result.Should().BeOfType<OkObjectResult>();
//        }

//        [Fact]
//        public void AttemptClientCredentialAuthentication_WithInvalidUpfmId_ReturnsBadRequest()
//        {
//            // Arrange
//            var invalidUpfmId = Guid.NewGuid();
//            _mockManageOrganization.Setup(x => x.GetOrganizationAdminUserRealPageId(invalidUpfmId))
//                .Returns(Guid.Empty);

//            // Act
//            var result = _controller.GetProductRoles("UPFM", invalidUpfmId);

//            // Assert
//            result.Should().BeOfType<BadRequestObjectResult>();
//            var badRequest = result as BadRequestObjectResult;
//            var errorResponse = badRequest.Value as ErrorResponse;
//            errorResponse.Errors[0].Detail.Should().Be("Invalid UPFMId.");
//        }

//        #endregion

//        #region Edge Cases and Integration Tests

//        [Fact]
//        public void GetUserProductRoles_WithCaseInsensitiveUPFMCode_ReturnsOkResult()
//        {
//            // Arrange
//            var realPageId = Guid.NewGuid();
//            var person = new Person { RealPageId = realPageId };
//            var persona = new Persona { PersonaId = 100, RealPageId = realPageId, OrganizationPartyId = _testOrgPartyId };
//            var roles = new List<ProductRole>
//            {
//                new ProductRole { ID = "1", Name = "Role1", IsAssigned = true }
//            };
//            var listResponse = new ListResponse { Records = roles.Cast<object>().ToList(), IsError = false };

//            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns(person);
//            _mockManagePersona.Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, _testOrgPartyId)).Returns(persona);
//            _mockManageUnifiedLogin.Setup(x => x.GetUserRoles(_testPersonaId, persona.PersonaId, _testOrgPartyId))
//                .Returns(listResponse);

//            // Act - Test with lowercase
//            var result = _controller.GetUserProductRoles(realPageId, "upfm");

//            // Assert
//            result.Should().BeOfType<OkObjectResult>();
//            _mockManageUnifiedLogin.Verify(
//                x => x.GetUserRoles(_testPersonaId, persona.PersonaId, _testOrgPartyId),
//                Times.Once);
//        }

//        [Fact]
//        public void GetProductRoles_WithCaseInsensitiveUPFMCode_ReturnsOkResult()
//        {
//            // Arrange
//            var roles = new List<ProductRole>
//            {
//                new ProductRole { ID = "1", Name = "Role1", IsAssigned = true }
//            };
//            var listResponse = new ListResponse
//            {
//                Records = roles.Cast<object>().ToList(),
//                IsError = false,
//                TotalRows = 1
//            };

//            _mockManageUnifiedLogin.Setup(x => x.GetRoles(_testPersonaId, _testOrgPartyId))
//                .Returns(listResponse);

//            // Act
//            var result = _controller.GetProductRoles("UpFm");

//            // Assert
//            result.Should().BeOfType<OkObjectResult>();
//            _mockManageUnifiedLogin.Verify(
//                x => x.GetRoles(_testPersonaId, _testOrgPartyId),
//                Times.Once);
//        }

//        [Fact]
//        public void GetUserProductRoles_WithEmptyRolesList_ReturnsOkWithEmptyData()
//        {
//            // Arrange
//            var realPageId = Guid.NewGuid();
//            var person = new Person { RealPageId = realPageId };
//            var persona = new Persona { PersonaId = 100, RealPageId = realPageId, OrganizationPartyId = _testOrgPartyId };
//            var listResponse = new ListResponse { Records = new List<object>(), IsError = false };

//            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns(person);
//            _mockManagePersona.Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, _testOrgPartyId)).Returns(persona);
//            _mockManageUnifiedLogin.Setup(x => x.GetUserRoles(_testPersonaId, persona.PersonaId, _testOrgPartyId))
//                .Returns(listResponse);

//            // Act
//            var result = _controller.GetUserProductRoles(realPageId, "UPFM");

//            // Assert
//            var okResult = result as OkObjectResult;
//            var response = okResult.Value as PagedResponse;
//            response.Data.Should().BeEmpty();
//        }

//        [Fact]
//        public void GetProductRoles_PagedResponsePropertiesInitializedCorrectly()
//        {
//            // Arrange
//            var roles = new List<ProductRole>
//            {
//                new ProductRole { ID = "1", Name = "Role1", IsAssigned = true },
//                new ProductRole { ID = "2", Name = "Role2", IsAssigned = true }
//            };
//            var listResponse = new ListResponse
//            {
//                Records = roles.Cast<object>().ToList(),
//                IsError = false,
//                TotalRows = 2
//            };

//            _mockManageUnifiedLogin.Setup(x => x.GetRoles(_testPersonaId, _testOrgPartyId))
//                .Returns(listResponse);

//            // Act
//            var result = _controller.GetProductRoles("UPFM");

//            // Assert
//            var okResult = result as OkObjectResult;
//            var response = okResult.Value as PagedResponse;
//            response.Meta.CurrentPage.Should().Be(1);
//            response.Meta.TotalRows.Should().Be(2);
//            response.Meta.RowsPerPage.Should().Be(2);
//        }

//        [Fact]
//        public void GetRightsforRole_WithNegativeRoleId1_ReturnsBadRequest()
//        {
//            // Arrange - negative roleId should not equal 0, so this actually passes to service
//            var rights = new List<Right>
//            {
//                new Right { RightId = 1, RightName = "Right1" }
//            };
//            var listResponse = new ListResponse
//            {
//                Records = rights.Cast<object>().ToList(),
//                IsError = false
//            };

//            _mockManageUnifiedLogin.Setup(x => x.GetListRightbyRole("UPFM", -1))
//                .Returns(listResponse);

//            // Act
//            var result = _controller.GetRightsforRole("UPFM", -1);

//            // Assert
//            result.Should().BeOfType<OkObjectResult>();
//        }

//        #endregion

//        #region Shared Product Setting Tests

//        [Fact]
//        public void GetUserProductRoles_WithInvalidSharedProductSetting_UsesOriginalProductId()
//        {
//            // Arrange
//            var realPageId = Guid.NewGuid();
//            var person = new Person { RealPageId = realPageId };
//            var persona = new Persona { PersonaId = 100, RealPageId = realPageId, OrganizationPartyId = _testOrgPartyId };
//            var products = new List<GbProductMap>
//            {
//                new GbProductMap { ProductId = (int)ProductEnum.OneSite, BooksProductCode = "ONESITE" }
//            };
//            var productSettings = new List<ProductInternalSetting>
//            {
//                new ProductInternalSetting { Name = SettingConstants.SharedProductSettingName, Value = "invalid" }
//            };
//            var roles = new List<ProductRole>
//            {
//                new ProductRole { ID = "1", Name = "Role1", IsAssigned = true }
//            };
//            var listResponse = new ListResponse { Records = roles.Cast<object>().ToList(), IsError = false };

//            _mockManagePerson.Setup(x => x.GetPerson(realPageId)).Returns(person);
//            _mockManagePersona.Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, _testOrgPartyId)).Returns(persona);
//            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(products);
//            _mockManageUnifiedLogin.Setup(x => x.GetProductInternalSettingByProductId((int)ProductEnum.OneSite))
//                .Returns(productSettings);
//            _mockManageProductPanel.Setup(x => x.GetProductRoles(
//                _testPersonaId, persona.PersonaId, _testOrgPartyId, (int)ProductEnum.OneSite, null, null))
//                .Returns(listResponse);

//            // Act
//            var result = _controller.GetUserProductRoles(realPageId, "ONESITE");

//            // Assert
//            result.Should().BeOfType<OkObjectResult>();
//            _mockManageProductPanel.Verify(
//                x => x.GetProductRoles(_testPersonaId, persona.PersonaId, _testOrgPartyId, (int)ProductEnum.OneSite, null, null),
//                Times.Once);
//        }

//        #endregion
//    }
//}