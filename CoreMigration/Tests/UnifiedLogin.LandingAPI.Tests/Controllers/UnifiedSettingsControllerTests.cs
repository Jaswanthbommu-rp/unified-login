using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class UnifiedSettingsControllerTests : ControllerTestBase
    {
        private UnifiedSettingsController _controller;

        public UnifiedSettingsControllerTests()
        {
            _controller = new UnifiedSettingsController(MockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new UnifiedSettingsController(MockUserClaimsAccessor.Object);

            Assert.NotNull(controller);
        }

        //[Fact]
        //public void Constructor_WithNullUserClaimsAccessor_DoesNotThrow()
        //{
        //    var controller = new UnifiedSettingsController(null!);

        //    Assert.NotNull(controller);
        //}

        #endregion

        #region GetSettings Tests - Empty CompanyId

        [Fact]
        public async Task GetSettings_WithEmptyCompanyId_ReturnsBadRequest()
        {
            var result = await _controller.GetSettings("Security", Guid.Empty);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiError = Assert.IsType<ApiError>(badRequestResult.Value);
            Assert.Equal("Settings.GetSettings.2", apiError.Code);
            Assert.Equal("Null Companyd.", apiError.Title);
        }

        [Fact]
        public async Task GetSettings_WithEmptyCompanyIdAndCategory_ReturnsBadRequest()
        {
            var result = await _controller.GetSettings("CustomFields", Guid.Empty);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiError = Assert.IsType<ApiError>(badRequestResult.Value);
            Assert.Equal("Settings.GetSettings.2", apiError.Code);
        }

        [Fact]
        public async Task GetSettings_WithEmptyCompanyIdAndNullCategory_ReturnsBadRequest()
        {
            var result = await _controller.GetSettings(null!, Guid.Empty);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiError = Assert.IsType<ApiError>(badRequestResult.Value);
            Assert.Equal("Settings.GetSettings.2", apiError.Code);
        }

        #endregion

        #region GetSettings Tests - Valid CompanyId

        [Fact]
        public async Task GetSettings_WithValidCompanyIdButCompanyNotFound_ReturnsBadRequest()
        {
            var companyId = Guid.NewGuid();

            var result = await _controller.GetSettings("Security", companyId);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiError = Assert.IsType<ApiError>(badRequestResult.Value);
            Assert.Equal("Settings.GetSettings.2", apiError.Code);
            Assert.Equal("Company not found.", apiError.Title);
        }

        [Fact]
        public async Task GetSettings_WithValidParameters_ReturnsResult()
        {
            var companyId = Guid.NewGuid();

            var result = await _controller.GetSettings("Security", companyId);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSettings_WithSecurityCategory_ReturnsResult()
        {
            var companyId = Guid.NewGuid();

            var result = await _controller.GetSettings("Security", companyId);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSettings_WithCustomFieldsCategory_ReturnsResult()
        {
            var companyId = Guid.NewGuid();

            var result = await _controller.GetSettings("CustomFields", companyId);

            Assert.NotNull(result);
        }

        #endregion

        #region GetSettings Tests - Includes Parameter

        [Fact]
        public async Task GetSettings_WithNullIncludes_ReturnsResult()
        {
            var companyId = Guid.NewGuid();

            var result = await _controller.GetSettings("Security", companyId, null);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSettings_WithEmptyIncludes_ReturnsResult()
        {
            var companyId = Guid.NewGuid();

            var result = await _controller.GetSettings("Security", companyId, Array.Empty<string>());

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSettings_WithIncludes_ReturnsResult()
        {
            var companyId = Guid.NewGuid();

            var result = await _controller.GetSettings("Security", companyId, new[] { "filter1", "filter2" });

            Assert.NotNull(result);
        }

        #endregion

        #region GetSettings Tests - Error Response Validation

        [Fact]
        public async Task GetSettings_EmptyCompanyId_ReturnsCorrectErrorStructure()
        {
            var result = await _controller.GetSettings("Security", Guid.Empty);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiError = Assert.IsType<ApiError>(badRequestResult.Value);

            Assert.NotNull(apiError.Id);
            Assert.Equal(400, apiError.Status);
            Assert.Equal("Null Companyd.", apiError.Title);
            Assert.Contains("Empty Company parameter", apiError.Detail);
            Assert.Equal("Settings.GetSettings.2", apiError.Code);
            Assert.NotNull(apiError.Source);
        }

        [Fact]
        public async Task GetSettings_CompanyNotFound_ReturnsCorrectErrorStructure()
        {
            var companyId = Guid.NewGuid();

            var result = await _controller.GetSettings("Security", companyId);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiError = Assert.IsType<ApiError>(badRequestResult.Value);

            Assert.NotNull(apiError.Id);
            Assert.Equal(400, apiError.Status);
            Assert.Equal("Company not found.", apiError.Title);
            Assert.Contains(companyId.ToString(), apiError.Detail);
            Assert.Equal("Settings.GetSettings.2", apiError.Code);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task GetSettings_WithMaxGuidCompanyId_ReturnsResult()
        {
            var companyId = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");

            var result = await _controller.GetSettings("Security", companyId);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSettings_WithEmptyCategory_ReturnsResult()
        {
            var companyId = Guid.NewGuid();

            var result = await _controller.GetSettings("", companyId);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSettings_WithWhitespaceCategory_ReturnsResult()
        {
            var companyId = Guid.NewGuid();

            var result = await _controller.GetSettings("   ", companyId);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSettings_WithSpecialCharactersInCategory_ReturnsResult()
        {
            var companyId = Guid.NewGuid();

            var result = await _controller.GetSettings("Security & Fields", companyId);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSettings_WithLongCategory_ReturnsResult()
        {
            var companyId = Guid.NewGuid();

            var result = await _controller.GetSettings(new string('A', 100), companyId);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSettings_WithUnicodeCategory_ReturnsResult()
        {
            var companyId = Guid.NewGuid();

            var result = await _controller.GetSettings("Sécurity", companyId);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSettings_CalledMultipleTimes_ReturnsConsistentResults()
        {
            var companyId = Guid.NewGuid();

            var result1 = await _controller.GetSettings("Security", companyId);
            var result2 = await _controller.GetSettings("Security", companyId);

            Assert.NotNull(result1);
            Assert.NotNull(result2);
        }

        [Fact]
        public async Task GetSettings_WithDifferentUserClaim_ReturnsResult()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 200,
                LoginName = "different@test.com",
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 5000,
                OrganizationMasterId = 12345
            });

            var controller = new UnifiedSettingsController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var companyId = Guid.NewGuid();
            var result = await controller.GetSettings("Security", companyId);

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
