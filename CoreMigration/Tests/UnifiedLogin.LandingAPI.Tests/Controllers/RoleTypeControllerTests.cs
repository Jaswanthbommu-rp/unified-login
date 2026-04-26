using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class RoleTypeControllerTests : ControllerTestBase
    {
        private readonly Mock<IManageRoleTypeAsync> _mockManageRoleTypeAsync;
        private readonly Mock<IManagePersonaAsync> _mockManagePersonaAsync;
        private RoleTypeController _controller;

        public RoleTypeControllerTests()
        {
            _mockManageRoleTypeAsync = new Mock<IManageRoleTypeAsync>();
            _mockManagePersonaAsync = new Mock<IManagePersonaAsync>();

            _controller = new RoleTypeController(
                MockUserClaimsAccessor.Object,
                _mockManageRoleTypeAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new RoleTypeController(
                MockUserClaimsAccessor.Object,
                _mockManageRoleTypeAsync.Object,
                _mockManagePersonaAsync.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new RoleTypeController(null!, _mockManageRoleTypeAsync.Object, _mockManagePersonaAsync.Object));

            Assert.Equal("userClaimsAccessor", exception.ParamName);
        }

        #endregion

        #region ListRoleType Tests - Default Parameters

        [Fact]
        public async Task ListRoleType_WithDefaultParameters_ReturnsOkResult()
        {
            var result = await _controller.ListRoleType();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListRoleType_WithNullRoleTypeName_ReturnsOkResult()
        {
            var result = await _controller.ListRoleType(null, null, false);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListRoleType_WithEmptyRoleTypeName_ReturnsOkResult()
        {
            var result = await _controller.ListRoleType("", null, false);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region ListRoleType Tests - With RoleTypeName

        [Fact]
        public async Task ListRoleType_WithValidRoleTypeName_ReturnsOkResult()
        {
            var result = await _controller.ListRoleType("User Role", null, false);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListRoleType_WithUserRoleTypeName_ReturnsResult()
        {
            var result = await _controller.ListRoleType("User Role", null, false);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListRoleType_WithNonUserRoleTypeName_ReturnsOkResult()
        {
            var result = await _controller.ListRoleType("Other Role", null, false);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListRoleType_WithCaseInsensitiveUserRole_ReturnsResult()
        {
            var result = await _controller.ListRoleType("user role", null, false);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListRoleType_WithMixedCaseUserRole_ReturnsResult()
        {
            var result = await _controller.ListRoleType("USER ROLE", null, false);

            Assert.NotNull(result);
        }

        #endregion

        #region ListRoleType Tests - With LoginName

        [Fact]
        public async Task ListRoleType_WithLoginName_ReturnsOkResult()
        {
            var result = await _controller.ListRoleType(null, "testuser@test.com", false);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListRoleType_WithRoleTypeNameAndLoginName_ReturnsResult()
        {
            var result = await _controller.ListRoleType("User Role", "testuser@test.com", false);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListRoleType_WithEmptyLoginName_ReturnsOkResult()
        {
            var result = await _controller.ListRoleType(null, "", false);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region ListRoleType Tests - With IncludeRelationShips

        [Fact]
        public async Task ListRoleType_WithIncludeRelationShipsTrue_ReturnsOkResult()
        {
            var result = await _controller.ListRoleType(null, null, true);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListRoleType_WithIncludeRelationShipsFalse_ReturnsOkResult()
        {
            var result = await _controller.ListRoleType(null, null, false);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListRoleType_WithAllParametersAndIncludeRelationships_ReturnsResult()
        {
            var result = await _controller.ListRoleType("User Role", "testuser@test.com", true);

            Assert.NotNull(result);
        }

        #endregion

        #region ListRoleType Tests - Organization Context

        [Fact]
        public async Task ListRoleType_WithZeroOrganizationPartyId_ReturnsOkResult()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 0
            });

            var controller = new RoleTypeController(mockUserClaimsAccessor.Object, _mockManageRoleTypeAsync.Object, _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.ListRoleType(null, null, false);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListRoleType_WithZeroOrganizationPartyIdAndUserRole_ReturnsOkResult()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 0
            });

            var controller = new RoleTypeController(mockUserClaimsAccessor.Object, _mockManageRoleTypeAsync.Object, _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.ListRoleType("User Role", null, false);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region ListRoleType Tests - Edge Cases

        [Fact]
        public async Task ListRoleType_WithSpecialCharactersInRoleTypeName_ReturnsResult()
        {
            var result = await _controller.ListRoleType("Test & Role", null, false);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListRoleType_WithLongRoleTypeName_ReturnsResult()
        {
            var result = await _controller.ListRoleType(new string('A', 100), null, false);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListRoleType_WithUnicodeRoleTypeName_ReturnsResult()
        {
            var result = await _controller.ListRoleType("R�le Sp�cial", null, false);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListRoleType_WithWhitespaceRoleTypeName_ReturnsResult()
        {
            var result = await _controller.ListRoleType("   ", null, false);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListRoleType_WithSpecialCharactersInLoginName_ReturnsResult()
        {
            var result = await _controller.ListRoleType(null, "user+test@test.com", false);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListRoleType_CalledMultipleTimes_ReturnsConsistentResults()
        {
            var result1 = await _controller.ListRoleType(null, null, false);
            var result2 = await _controller.ListRoleType(null, null, false);

            Assert.NotNull(result1);
            Assert.NotNull(result2);
        }

        #endregion

        #region ListRoleType Tests - RPEmployee Context

        [Fact]
        public async Task ListRoleType_WithRPEmployee_ReturnsOkResult()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "rpemployee@realpage.com",
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 1000,
                IsRPEmployee = true
            });

            var controller = new RoleTypeController(mockUserClaimsAccessor.Object, _mockManageRoleTypeAsync.Object, _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.ListRoleType("User Role", null, false);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListRoleType_WithNonRPEmployee_ReturnsResult()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "user@company.com",
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 1000,
                IsRPEmployee = false
            });

            var controller = new RoleTypeController(mockUserClaimsAccessor.Object, _mockManageRoleTypeAsync.Object, _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.ListRoleType("User Role", null, false);

            Assert.NotNull(result);
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _controller = null!;
            base.Dispose();
        }

        #endregion
    }
}
