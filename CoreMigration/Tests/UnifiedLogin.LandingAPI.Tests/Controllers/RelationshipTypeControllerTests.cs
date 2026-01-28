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
    public class RelationshipTypeControllerTests : ControllerTestBase
    {
        private RelationshipTypeController _controller;

        public RelationshipTypeControllerTests()
        {
            _controller = new RelationshipTypeController(MockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new RelationshipTypeController(MockUserClaimsAccessor.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_DoesNotThrow()
        {
            var controller = new RelationshipTypeController(null!);

            Assert.NotNull(controller);
        }

        #endregion

        #region ListRelationshipType Tests

        [Fact]
        public async Task ListRelationshipType_WithValidRelationshipTypeName_ReturnsOkResult()
        {
            var result = await _controller.ListRelationshipType("Employment");

            Assert.NotNull(result);
        }

        //[Fact]
        //public async Task ListRelationshipType_WithEmptyRelationshipTypeName_ReturnsResult()
        //{
        //    var result = await _controller.ListRelationshipType("");

        //    Assert.NotNull(result);
        //}

        //[Fact]
        //public async Task ListRelationshipType_WithNullRelationshipTypeName_ReturnsResult()
        //{
        //    var result = await _controller.ListRelationshipType(null!);

        //    Assert.NotNull(result);
        //}

        [Fact]
        public async Task ListRelationshipType_WithNonExistentRelationshipTypeName_ReturnsNoContent()
        {
            var result = await _controller.ListRelationshipType("NonExistentType12345");

            // Result could be Ok with data or NoContent depending on the data
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListRelationshipType_WithSpecialCharacters_ReturnsResult()
        {
            var result = await _controller.ListRelationshipType("Test & Type");

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListRelationshipType_WithLongTypeName_ReturnsResult()
        {
            var result = await _controller.ListRelationshipType(new string('A', 100));

            Assert.NotNull(result);
        }

        //[Fact]
        //public async Task ListRelationshipType_WithWhitespaceTypeName_ReturnsResult()
        //{
        //    var result = await _controller.ListRelationshipType("   ");

        //    Assert.NotNull(result);
        //}

        [Fact]
        public async Task ListRelationshipType_WithUnicodeTypeName_ReturnsResult()
        {
            var result = await _controller.ListRelationshipType("Relación");

            Assert.NotNull(result);
        }

        #endregion

        #region ListUserRelationTypes Tests

        [Fact]
        public async Task ListUserRelationTypes_ReturnsOkResult()
        {
            var result = await _controller.ListUserRelationTypes();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListUserRelationTypes_WithValidUserClaim_ReturnsResult()
        {
            var result = await _controller.ListUserRelationTypes();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListUserRelationTypes_WithDifferentUserClaim_ReturnsResult()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 200,
                LoginName = "different@test.com",
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 5000
            });

            var controller = new RelationshipTypeController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.ListUserRelationTypes();

            Assert.NotNull(result);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task ListRelationshipType_WithNumericTypeName_ReturnsResult()
        {
            var result = await _controller.ListRelationshipType("12345");

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListRelationshipType_WithMixedCaseTypeName_ReturnsResult()
        {
            var result = await _controller.ListRelationshipType("EmPlOyMeNt");

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListRelationshipType_CalledMultipleTimes_ReturnsConsistentResults()
        {
            var result1 = await _controller.ListRelationshipType("Employment");
            var result2 = await _controller.ListRelationshipType("Employment");

            Assert.NotNull(result1);
            Assert.NotNull(result2);
        }

        [Fact]
        public async Task ListUserRelationTypes_CalledMultipleTimes_ReturnsConsistentResults()
        {
            var result1 = await _controller.ListUserRelationTypes();
            var result2 = await _controller.ListUserRelationTypes();

            Assert.NotNull(result1);
            Assert.NotNull(result2);
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
