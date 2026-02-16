using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class ResearchApplicationControllerTests : ControllerTestBase
    {
        private readonly Mock<IManageResearchApplication> _mockManageResearchApplication;
        private ResearchApplicationController _controller;

        public ResearchApplicationControllerTests()
        {
            _mockManageResearchApplication = new Mock<IManageResearchApplication>();

            _controller = new ResearchApplicationController(
                MockUserClaimsAccessor.Object,
                _mockManageResearchApplication.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new ResearchApplicationController(
                MockUserClaimsAccessor.Object,
                _mockManageResearchApplication.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ResearchApplicationController(
                null!,
                _mockManageResearchApplication.Object));
        }

        [Fact]
        public void Constructor_WithNullManageResearchApplication_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ResearchApplicationController(
                MockUserClaimsAccessor.Object,
                null!));
        }

        #endregion

        #region GetRoles Tests

        [Fact]
        public async Task GetRoles_WithZeroPartyId_ReturnsBadRequest()
        {
            var result = await _controller.GetRoles(100, 200, 0, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("partyId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRoles_WithValidParameters_ReturnsOkResult()
        {
            _mockManageResearchApplication
                .Setup(x => x.GetRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRoles(100, 200, 300, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRoles_WithNullDataFilter_ReturnsOkResult()
        {
            _mockManageResearchApplication
                .Setup(x => x.GetRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRoles(100, 200, 300, null!);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRoles_WithZeroEditorPersonaId_ReturnsOkResult()
        {
            _mockManageResearchApplication
                .Setup(x => x.GetRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRoles(0, 200, 300, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRoles_WithZeroUserPersonaId_ReturnsOkResult()
        {
            _mockManageResearchApplication
                .Setup(x => x.GetRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRoles(100, 0, 300, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRoles_VerifiesCorrectParametersPassed()
        {
            _mockManageResearchApplication
                .Setup(x => x.GetRoles(100, 200, 300))
                .Returns(new ListResponse { IsError = false });

            await _controller.GetRoles(100, 200, 300, new RequestParameter());

            _mockManageResearchApplication.Verify(x => x.GetRoles(100, 200, 300), Times.Once);
        }

        #endregion

        #region GetRightsByRole Tests

        [Fact]
        public async Task GetRightsByRole_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRightsByRole(0, 200, 300, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRightsByRole_WithZeroPartyId_ReturnsBadRequest()
        {
            var result = await _controller.GetRightsByRole(100, 0, 300, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("partyId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRightsByRole_WithValidParameters_ReturnsOkResult()
        {
            _mockManageResearchApplication
                .Setup(x => x.GetRightsByRole(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRightsByRole(100, 200, 300, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRightsByRole_WithNullDataFilter_ReturnsOkResult()
        {
            _mockManageResearchApplication
                .Setup(x => x.GetRightsByRole(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRightsByRole(100, 200, 300, null!);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRightsByRole_WithZeroRoleId_ReturnsOkResult()
        {
            _mockManageResearchApplication
                .Setup(x => x.GetRightsByRole(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRightsByRole(100, 200, 0, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRightsByRole_VerifiesCorrectParametersPassed()
        {
            _mockManageResearchApplication
                .Setup(x => x.GetRightsByRole(100, 200, 300))
                .Returns(new ListResponse { IsError = false });

            await _controller.GetRightsByRole(100, 200, 300, new RequestParameter());

            _mockManageResearchApplication.Verify(x => x.GetRightsByRole(100, 200, 300), Times.Once);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task GetRoles_WithMaxLongValues_ReturnsOkResult()
        {
            _mockManageResearchApplication
                .Setup(x => x.GetRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRoles(long.MaxValue, long.MaxValue, long.MaxValue, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRoles_WithNegativePartyId_ReturnsOkResult()
        {
            _mockManageResearchApplication
                .Setup(x => x.GetRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            // Negative values are not zero, so they pass the check
            var result = await _controller.GetRoles(100, 200, -1, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRightsByRole_WithMaxLongValues_ReturnsOkResult()
        {
            _mockManageResearchApplication
                .Setup(x => x.GetRightsByRole(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRightsByRole(long.MaxValue, long.MaxValue, long.MaxValue, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRightsByRole_WithNegativeValues_ReturnsOkResult()
        {
            _mockManageResearchApplication
                .Setup(x => x.GetRightsByRole(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            // Negative values are not zero, so they pass the check
            var result = await _controller.GetRightsByRole(-1, -2, -3, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRoles_WhenServiceReturnsError_StillReturnsOkResult()
        {
            _mockManageResearchApplication
                .Setup(x => x.GetRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(new ListResponse { IsError = true });

            var result = await _controller.GetRoles(100, 200, 300, new RequestParameter());

            // Controller always returns Ok for valid requests
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRightsByRole_WhenServiceReturnsError_StillReturnsOkResult()
        {
            _mockManageResearchApplication
                .Setup(x => x.GetRightsByRole(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(new ListResponse { IsError = true });

            var result = await _controller.GetRightsByRole(100, 200, 300, new RequestParameter());

            // Controller always returns Ok for valid requests
            Assert.IsType<OkObjectResult>(result);
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
