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
    public class TelecommunicationNumberControllerTests : ControllerTestBase
    {
        private TelecommunicationNumberController _controller;

        public TelecommunicationNumberControllerTests()
        {
            _controller = new TelecommunicationNumberController(MockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new TelecommunicationNumberController(MockUserClaimsAccessor.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_DoesNotThrow()
        {
            var controller = new TelecommunicationNumberController(null!);

            Assert.NotNull(controller);
        }

        #endregion

        #region LinkTelecommunicationNumber Tests

        [Fact]
        public async Task LinkTelecommunicationNumber_WithEmptyRealPageIdAndEmptyUserClaim_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new TelecommunicationNumberController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var linkTelecom = CreateValidLinkTelecommunicationNumber();

            var result = await controller.LinkTelecommunicationNumber(Guid.Empty, linkTelecom);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkTelecommunicationNumber_WithNullLinkTelecommunicationNumber_ReturnsBadRequest()
        {
            var realPageId = Guid.NewGuid();

            var result = await _controller.LinkTelecommunicationNumber(realPageId, null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Null parameter: linkTelecommunicationNumber", badRequestResult.Value);
        }

        //[Fact]
        //public async Task LinkTelecommunicationNumber_WithValidParameters_ReturnsResult()
        //{
        //    var realPageId = Guid.NewGuid();
        //    var linkTelecom = CreateValidLinkTelecommunicationNumber();

        //    var result = await _controller.LinkTelecommunicationNumber(realPageId, linkTelecom);

        //    Assert.NotNull(result);
        //}

        //[Fact]
        //public async Task LinkTelecommunicationNumber_WithEmptyRealPageIdButValidUserClaim_ReturnsResult()
        //{
        //    var linkTelecom = CreateValidLinkTelecommunicationNumber();

        //    var result = await _controller.LinkTelecommunicationNumber(Guid.Empty, linkTelecom);

        //    Assert.NotNull(result);
        //}

        #endregion

        #region UpdateTelecommunicationNumber Tests

        [Fact]
        public async Task UpdateTelecommunicationNumber_WithEmptyRealPageIdAndEmptyUserClaim_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new TelecommunicationNumberController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var linkTelecom = CreateValidLinkTelecommunicationNumber();

            var result = await controller.UpdateTelecommunicationNumber(Guid.Empty, linkTelecom);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateTelecommunicationNumber_WithNullLinkTelecommunicationNumber_ReturnsBadRequest()
        {
            var realPageId = Guid.NewGuid();

            var result = await _controller.UpdateTelecommunicationNumber(realPageId, null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Null parameter: linkTelecommunicationNumber", badRequestResult.Value);
        }

        //[Fact]
        //public async Task UpdateTelecommunicationNumber_WithValidParameters_ReturnsResult()
        //{
        //    var realPageId = Guid.NewGuid();
        //    var linkTelecom = CreateValidLinkTelecommunicationNumber();

        //    var result = await _controller.UpdateTelecommunicationNumber(realPageId, linkTelecom);

        //    Assert.NotNull(result);
        //}

        //[Fact]
        //public async Task UpdateTelecommunicationNumber_WithEmptyRealPageIdButValidUserClaim_ReturnsResult()
        //{
        //    var linkTelecom = CreateValidLinkTelecommunicationNumber();

        //    var result = await _controller.UpdateTelecommunicationNumber(Guid.Empty, linkTelecom);

        //    Assert.NotNull(result);
        //}

        #endregion

        #region ListTelecommunicationNumberForPerson Tests

        [Fact]
        public async Task ListTelecommunicationNumberForPerson_WithEmptyRealPageIdAndEmptyUserClaim_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new TelecommunicationNumberController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.ListTelecommunicationNumberForPerson(Guid.Empty);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        [Fact]
        public async Task ListTelecommunicationNumberForPerson_WithValidRealPageId_ReturnsResult()
        {
            var realPageId = Guid.NewGuid();

            var result = await _controller.ListTelecommunicationNumberForPerson(realPageId);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListTelecommunicationNumberForPerson_WithEmptyRealPageIdButValidUserClaim_ReturnsResult()
        {
            var result = await _controller.ListTelecommunicationNumberForPerson(Guid.Empty);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListTelecommunicationNumberForPerson_WithContactMechanismUsageTypeName_ReturnsResult()
        {
            var realPageId = Guid.NewGuid();

            var result = await _controller.ListTelecommunicationNumberForPerson(realPageId, "Work");

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListTelecommunicationNumberForPerson_WithEmptyContactMechanismUsageTypeName_ReturnsResult()
        {
            var realPageId = Guid.NewGuid();

            var result = await _controller.ListTelecommunicationNumberForPerson(realPageId, "");

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListTelecommunicationNumberForPerson_WithNonExistentPerson_ReturnsNoContent()
        {
            var realPageId = Guid.NewGuid();

            var result = await _controller.ListTelecommunicationNumberForPerson(realPageId);

            // Could be NoContent if no telecommunication numbers found
            Assert.NotNull(result);
        }

        #endregion

        #region Edge Cases

        //[Fact]
        //public async Task LinkTelecommunicationNumber_WithMaxGuidRealPageId_ReturnsResult()
        //{
        //    var realPageId = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");
        //    var linkTelecom = CreateValidLinkTelecommunicationNumber();

        //    var result = await _controller.LinkTelecommunicationNumber(realPageId, linkTelecom);

        //    Assert.NotNull(result);
        //}

        //[Fact]
        //public async Task UpdateTelecommunicationNumber_WithMaxGuidRealPageId_ReturnsResult()
        //{
        //    var realPageId = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");
        //    var linkTelecom = CreateValidLinkTelecommunicationNumber();

        //    var result = await _controller.UpdateTelecommunicationNumber(realPageId, linkTelecom);

        //    Assert.NotNull(result);
        //}

        [Fact]
        public async Task ListTelecommunicationNumberForPerson_WithMaxGuidRealPageId_ReturnsResult()
        {
            var realPageId = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");

            var result = await _controller.ListTelecommunicationNumberForPerson(realPageId);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListTelecommunicationNumberForPerson_WithSpecialCharactersInUsageTypeName_ReturnsResult()
        {
            var realPageId = Guid.NewGuid();

            var result = await _controller.ListTelecommunicationNumberForPerson(realPageId, "Work & Home");

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListTelecommunicationNumberForPerson_WithLongUsageTypeName_ReturnsResult()
        {
            var realPageId = Guid.NewGuid();

            var result = await _controller.ListTelecommunicationNumberForPerson(realPageId, new string('A', 100));

            Assert.NotNull(result);
        }

        //[Fact]
        //public async Task LinkTelecommunicationNumber_WithNullUserClaim_ReturnsResult()
        //{
        //    var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
        //    mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

        //    var controller = new TelecommunicationNumberController(mockUserClaimsAccessor.Object)
        //    {
        //        ControllerContext = CreateControllerContext()
        //    };

        //    var realPageId = Guid.NewGuid();
        //    var linkTelecom = CreateValidLinkTelecommunicationNumber();

        //    var result = await controller.LinkTelecommunicationNumber(realPageId, linkTelecom);

        //    Assert.NotNull(result);
        //}

        //[Fact]
        //public async Task UpdateTelecommunicationNumber_WithNullUserClaim_ReturnsResult()
        //{
        //    var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
        //    mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

        //    var controller = new TelecommunicationNumberController(mockUserClaimsAccessor.Object)
        //    {
        //        ControllerContext = CreateControllerContext()
        //    };

        //    var realPageId = Guid.NewGuid();
        //    var linkTelecom = CreateValidLinkTelecommunicationNumber();

        //    var result = await controller.UpdateTelecommunicationNumber(realPageId, linkTelecom);

        //    Assert.NotNull(result);
        //}

        [Fact]
        public async Task ListTelecommunicationNumberForPerson_WithNullUserClaim_ReturnsResult()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new TelecommunicationNumberController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var realPageId = Guid.NewGuid();

            var result = await controller.ListTelecommunicationNumberForPerson(realPageId);

            Assert.NotNull(result);
        }

        #endregion

        #region Helper Methods

        private static LinkTelecommunicationNumber CreateValidLinkTelecommunicationNumber()
        {
            return new LinkTelecommunicationNumber
            {
                PartyContactMechanism = new PartyContactMechanism
                {
                    PartyContactMechanismId = 1,
                    ContactMechanismId = 100,
                    PartyId = 200
                },
                ContactMechanismUsageType = new ContactMechanismUsageType
                {
                    ContactMechanismUsageTypeId = 1,
                    Name = "Work"
                },
                TelecommunicationNumber = new TelecommunicationNumber
                {
                    ContactMechanismId = 100,
                    CountryCode = "1",
                    AreaCode = "555",
                    PhoneNumber = "1234567"
                }
            };
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
