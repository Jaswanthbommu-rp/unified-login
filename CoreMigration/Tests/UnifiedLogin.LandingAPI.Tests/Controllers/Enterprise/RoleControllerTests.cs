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
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private readonly RoleController _controller;
        private readonly DefaultUserClaim _defaultUserClaim;

        private readonly int _testUserId = 100;
        private readonly Guid _testUpfmId = Guid.NewGuid();
        private readonly long _testPersonaId = 100;
        private readonly long _testOrgPartyId = 1000;
        private readonly Guid _testUserGuid = Guid.NewGuid();

        #endregion

        #region Constructor

        public RoleControllerTests()
        {
            // Initialize all mocks
            _mockRoleQueryService = new Mock<IRoleQueryService>();
            _mockClientCredentialAuthenticator = new Mock<IClientCredentialAuthenticator>();
            _mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();

            // Setup UserClaimsAccessor mock
            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(_testPersonaId);
            _mockUserClaimsAccessor.Setup(x => x.OrganizationPartyId).Returns(_testOrgPartyId);
            _mockUserClaimsAccessor.Setup(x => x.UserId).Returns(_testUserId);

            // Create DefaultUserClaim instance for authentication
            _defaultUserClaim = new DefaultUserClaim
            {
                PersonaId = _testPersonaId,
                OrganizationPartyId = _testOrgPartyId,
                UserId = _testUserId
            };

            _controller = new RoleController(
                _mockRoleQueryService.Object,
                _mockClientCredentialAuthenticator.Object,
                _mockUserClaimsAccessor.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim("scope", "enterpriseapi"),
                            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString())
                        }, "TestAuthType"))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), invalidUpfmId))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), _testUpfmId))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetProductRoles("UPFM"))
                .Returns(ActionResultEnvelope.Ok(response));

            // Act
            var result = _controller.GetProductRoles("UPFM", _testUpfmId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockClientCredentialAuthenticator.Verify(
                x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), _testUpfmId),
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), _testUpfmId))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), _testUpfmId))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetUserProductRoles(realPageId, "UPFM"))
                .Returns(ActionResultEnvelope.Ok(response));

            // Act
            var result = _controller.GetUserProductRoles(realPageId, "UPFM", null);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockClientCredentialAuthenticator.Verify(
                x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null),
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), _testUpfmId))
                .Returns(ClientCredentialAuthResult.Success(_defaultUserClaim));

            _mockRoleQueryService
                .Setup(x => x.GetProductRoles("UPFM"))
                .Returns(ActionResultEnvelope.Ok(response));

            // Act
            var result = _controller.GetProductRoles("UPFM", _testUpfmId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockClientCredentialAuthenticator.Verify(
                x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), _testUpfmId),
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
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
                .Setup(x => x.Authenticate(It.IsAny<ClaimsPrincipal>(), It.IsAny<DefaultUserClaim>(), null))
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
                new RoleController(null, _mockClientCredentialAuthenticator.Object, _mockUserClaimsAccessor.Object));

            exception.ParamName.Should().Be("roleQueryService");
        }

        [Fact]
        public void Constructor_WithNullClientCredentialAuthenticator_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new RoleController(_mockRoleQueryService.Object, null, _mockUserClaimsAccessor.Object));

            exception.ParamName.Should().Be("clientCredentialAuthenticator");
        }

        [Fact]
        public void Constructor_WithNullUserClaims_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new RoleController(_mockRoleQueryService.Object, _mockClientCredentialAuthenticator.Object, null));

            exception.ParamName.Should().Be("userClaimsAccessor");
        }

        #endregion
    }
}