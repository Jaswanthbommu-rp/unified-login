using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Unit tests for PostalAddressController (async refactor).
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PostalAddressControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IManageContactMechanismAsync> _mockManageContactMechanism;
        private readonly Mock<IManageStreetAddressAsync> _mockManageStreetAddress;
        private readonly Mock<IManageGeographicBoundaryAsync> _mockManageGeographicBoundary;
        private readonly Mock<IManagePostalAddressAsync> _mockManagePostalAddress;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private PostalAddressController _postalAddressController;

        #endregion

        #region Constructor

        public PostalAddressControllerTests()
        {
            _mockManageContactMechanism = new Mock<IManageContactMechanismAsync>();
            _mockManageStreetAddress = new Mock<IManageStreetAddressAsync>();
            _mockManageGeographicBoundary = new Mock<IManageGeographicBoundaryAsync>();
            _mockManagePostalAddress = new Mock<IManagePostalAddressAsync>();
            _mockUserClaimsAccessor = MockUserClaimsAccessor;

            _mockUserClaimsAccessor
                .Setup(x => x.GetUserClaim())
                .Returns(new DefaultUserClaim { UserRealPageGuid = Guid.NewGuid() });

            _postalAddressController = new PostalAddressController(
                _mockManageContactMechanism.Object,
                _mockManageStreetAddress.Object,
                _mockManageGeographicBoundary.Object,
                _mockManagePostalAddress.Object,
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
            var controller = new PostalAddressController(
                _mockManageContactMechanism.Object,
                _mockManageStreetAddress.Object,
                _mockManageGeographicBoundary.Object,
                _mockManagePostalAddress.Object,
                _mockUserClaimsAccessor.Object);

            Assert.NotNull(controller);
        }

        #endregion

        #region LinkPostalAddress Tests - Success

        [Fact]
        public async Task LinkPostalAddress_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkPostalAddress = CreateValidLinkPostalAddress();
            SetupSuccessfulLinkPostalAddressMocks(realPageId);

            // Act
            var result = await _postalAddressController.LinkPostalAddress(realPageId, linkPostalAddress);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var outputResult = Assert.IsType<PostalAddress.PostalAddressOutputResult>(okResult.Value);
            Assert.Equal(100, outputResult.ContactMechanismId);
        }

        [Fact]
        public async Task LinkPostalAddress_WithEmptyGuid_UsesUserClaimsRealPageId()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim())
                .Returns(new DefaultUserClaim { UserRealPageGuid = userRealPageId });

            var linkPostalAddress = CreateValidLinkPostalAddress();
            SetupSuccessfulLinkPostalAddressMocks(userRealPageId);

            // Act
            var result = await _postalAddressController.LinkPostalAddress(Guid.Empty, linkPostalAddress);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _mockManageContactMechanism.Verify(
                x => x.LinkContactMechanismToPartyAsync(userRealPageId, It.IsAny<IPartyContactMechanism>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }

        #endregion

        #region LinkPostalAddress Tests - BadRequest

        [Fact]
        public async Task LinkPostalAddress_WithEmptyGuidAndEmptyUserClaims_ReturnsBadRequest()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim())
                .Returns(new DefaultUserClaim { UserRealPageGuid = Guid.Empty });

            // Act
            var result = await _postalAddressController.LinkPostalAddress(Guid.Empty, CreateValidLinkPostalAddress());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequest.Value);
        }

        [Fact]
        public async Task LinkPostalAddress_WithNullBody_ReturnsBadRequest()
        {
            // Act
            var result = await _postalAddressController.LinkPostalAddress(Guid.NewGuid(), null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Null parameter: linkPostalAddress.", badRequest.Value);
        }

        [Fact]
        public async Task LinkPostalAddress_WhenCreateContactMechanismFails_ReturnsBadRequest()
        {
            // Arrange
            const string error = "Failed to create contact mechanism";
            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanismAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = error });

            // Act
            var result = await _postalAddressController.LinkPostalAddress(Guid.NewGuid(), CreateValidLinkPostalAddress());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(error, badRequest.Value);
        }

        [Fact]
        public async Task LinkPostalAddress_WhenLinkContactMechanismToPartyFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string error = "Failed to link contact mechanism to party";

            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanismAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 100 });
            _mockManageContactMechanism
                .Setup(x => x.LinkContactMechanismToPartyAsync(realPageId, It.IsAny<IPartyContactMechanism>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = error });

            // Act
            var result = await _postalAddressController.LinkPostalAddress(realPageId, CreateValidLinkPostalAddress());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(error, badRequest.Value);
        }

        [Fact]
        public async Task LinkPostalAddress_WhenLinkUsageTypeFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string error = "Failed to link usage type";

            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanismAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 100 });
            _mockManageContactMechanism
                .Setup(x => x.LinkContactMechanismToPartyAsync(realPageId, It.IsAny<IPartyContactMechanism>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 200 });
            _mockManageContactMechanism
                .Setup(x => x.LinkUsageTypeToPartyContactMechanismAsync(200, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = error });

            // Act
            var result = await _postalAddressController.LinkPostalAddress(realPageId, CreateValidLinkPostalAddress());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(error, badRequest.Value);
        }

        [Fact]
        public async Task LinkPostalAddress_WhenCreateStreetAddressFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string error = "Failed to create street address";

            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanismAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 100 });
            _mockManageContactMechanism
                .Setup(x => x.LinkContactMechanismToPartyAsync(realPageId, It.IsAny<IPartyContactMechanism>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 200 });
            _mockManageContactMechanism
                .Setup(x => x.LinkUsageTypeToPartyContactMechanismAsync(200, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });
            _mockManageStreetAddress
                .Setup(x => x.CreateStreetAddressAsync(It.IsAny<IStreetAddress>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = error });

            // Act
            var result = await _postalAddressController.LinkPostalAddress(realPageId, CreateValidLinkPostalAddress());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(error, badRequest.Value);
        }

        [Fact]
        public async Task LinkPostalAddress_WhenCreateGeographicBoundaryFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string error = "Failed to create geographic boundary";
            SetupLinkPostalAddressMocksUntilStreetAddress(realPageId);
            _mockManageGeographicBoundary
                .Setup(x => x.CreateGeographicBoundaryAsync(It.IsAny<IGeographicBoundary>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = error });

            // Act
            var result = await _postalAddressController.LinkPostalAddress(realPageId, CreateValidLinkPostalAddress());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(error, badRequest.Value);
        }

        [Fact]
        public async Task LinkPostalAddress_WhenLinkGeographicBoundaryFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string error = "Failed to link geographic boundary";
            SetupLinkPostalAddressMocksUntilStreetAddress(realPageId);
            _mockManageGeographicBoundary
                .Setup(x => x.CreateGeographicBoundaryAsync(It.IsAny<IGeographicBoundary>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 300 });
            _mockManageContactMechanism
                .Setup(x => x.LinkGeographicBoundaryToContactMechanismAsync(It.IsAny<IContactMechanismBoundary>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = error });

            // Act
            var result = await _postalAddressController.LinkPostalAddress(realPageId, CreateValidLinkPostalAddress());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(error, badRequest.Value);
        }

        #endregion

        #region UpdatePostalAddress Tests - Success

        [Fact]
        public async Task UpdatePostalAddress_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkPostalAddress = CreateValidLinkPostalAddress();
            linkPostalAddress.PartyContactMechanism.ContactMechanismId = 50;
            SetupSuccessfulUpdatePostalAddressMocks(realPageId);

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(realPageId, linkPostalAddress);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var outputResult = Assert.IsType<PostalAddress.PostalAddressOutputResult>(okResult.Value);
            Assert.Equal(100, outputResult.ContactMechanismId);
        }

        [Fact]
        public async Task UpdatePostalAddress_WithEmptyGuid_UsesUserClaimsRealPageId()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim())
                .Returns(new DefaultUserClaim { UserRealPageGuid = userRealPageId });
            SetupSuccessfulUpdatePostalAddressMocks(userRealPageId);

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(Guid.Empty, CreateValidLinkPostalAddress());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdatePostalAddress Tests - BadRequest

        [Fact]
        public async Task UpdatePostalAddress_WithEmptyGuidAndEmptyUserClaims_ReturnsBadRequest()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim())
                .Returns(new DefaultUserClaim { UserRealPageGuid = Guid.Empty });

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(Guid.Empty, CreateValidLinkPostalAddress());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequest.Value);
        }

        [Fact]
        public async Task UpdatePostalAddress_WithNullBody_ReturnsBadRequest()
        {
            // Act
            var result = await _postalAddressController.UpdatePostalAddress(Guid.NewGuid(), null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Null parameter: linkPostalAddress.", badRequest.Value);
        }

        [Fact]
        public async Task UpdatePostalAddress_WhenExpireLinkContactMechanismFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string error = "Failed to expire contact mechanism";
            _mockManageContactMechanism
                .Setup(x => x.LinkContactMechanismToPartyAsync(realPageId, It.IsAny<IPartyContactMechanism>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = error });

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(realPageId, CreateValidLinkPostalAddress());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(error, badRequest.Value);
        }

        [Fact]
        public async Task UpdatePostalAddress_WhenCreateContactMechanismFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string error = "Failed to create contact mechanism";
            _mockManageContactMechanism
                .SetupSequence(x => x.LinkContactMechanismToPartyAsync(realPageId, It.IsAny<IPartyContactMechanism>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });
            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanismAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = error });

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(realPageId, CreateValidLinkPostalAddress());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(error, badRequest.Value);
        }

        [Fact]
        public async Task UpdatePostalAddress_WhenLinkNewContactMechanismToPartyFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string error = "Failed to link new contact mechanism";
            _mockManageContactMechanism
                .SetupSequence(x => x.LinkContactMechanismToPartyAsync(realPageId, It.IsAny<IPartyContactMechanism>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 })
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = error });
            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanismAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 100 });

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(realPageId, CreateValidLinkPostalAddress());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(error, badRequest.Value);
        }

        [Fact]
        public async Task UpdatePostalAddress_WhenLinkUsageTypeFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string error = "Failed to link usage type";
            _mockManageContactMechanism
                .SetupSequence(x => x.LinkContactMechanismToPartyAsync(realPageId, It.IsAny<IPartyContactMechanism>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 })
                .ReturnsAsync(new RepositoryResponse { Id = 200 });
            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanismAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 100 });
            _mockManageContactMechanism
                .Setup(x => x.LinkUsageTypeToPartyContactMechanismAsync(200, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = error });

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(realPageId, CreateValidLinkPostalAddress());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(error, badRequest.Value);
        }

        [Fact]
        public async Task UpdatePostalAddress_WhenCreateStreetAddressFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string error = "Failed to create street address";
            _mockManageContactMechanism
                .SetupSequence(x => x.LinkContactMechanismToPartyAsync(realPageId, It.IsAny<IPartyContactMechanism>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 })
                .ReturnsAsync(new RepositoryResponse { Id = 200 });
            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanismAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 100 });
            _mockManageContactMechanism
                .Setup(x => x.LinkUsageTypeToPartyContactMechanismAsync(200, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });
            _mockManageStreetAddress
                .Setup(x => x.CreateStreetAddressAsync(It.IsAny<IStreetAddress>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = error });

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(realPageId, CreateValidLinkPostalAddress());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(error, badRequest.Value);
        }

        [Fact]
        public async Task UpdatePostalAddress_WhenCreateGeographicBoundaryFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string error = "Failed to create geographic boundary";
            SetupUpdatePostalAddressMocksUntilStreetAddress(realPageId);
            _mockManageGeographicBoundary
                .Setup(x => x.CreateGeographicBoundaryAsync(It.IsAny<IGeographicBoundary>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = error });

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(realPageId, CreateValidLinkPostalAddress());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(error, badRequest.Value);
        }

        [Fact]
        public async Task UpdatePostalAddress_WhenLinkGeographicBoundaryFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string error = "Failed to link geographic boundary";
            SetupUpdatePostalAddressMocksUntilStreetAddress(realPageId);
            _mockManageGeographicBoundary
                .Setup(x => x.CreateGeographicBoundaryAsync(It.IsAny<IGeographicBoundary>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 300 });
            _mockManageContactMechanism
                .Setup(x => x.LinkGeographicBoundaryToContactMechanismAsync(It.IsAny<IContactMechanismBoundary>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = error });

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(realPageId, CreateValidLinkPostalAddress());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(error, badRequest.Value);
        }

        #endregion

        #region ListPostalAddressForPerson Tests

        [Fact]
        public async Task ListPostalAddressForPerson_WithValidData_ReturnsOkWithList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var postalAddressList = new List<PostalAddress> { new PostalAddress(), new PostalAddress() };
            _mockManagePostalAddress
                .Setup(x => x.ListPostalAddressForPersonAsync(realPageId, "", It.IsAny<CancellationToken>()))
                .ReturnsAsync(postalAddressList);

            // Act
            var result = await _postalAddressController.ListPostalAddressForPerson(realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<PostalAddress, IErrorData>>(okResult.Value);
            Assert.Equal(2, output.list.Count);
        }

        [Fact]
        public async Task ListPostalAddressForPerson_WithUsageTypeName_PassesToService()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string usageTypeName = "Home";
            _mockManagePostalAddress
                .Setup(x => x.ListPostalAddressForPersonAsync(realPageId, usageTypeName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PostalAddress> { new PostalAddress() });

            // Act
            var result = await _postalAddressController.ListPostalAddressForPerson(realPageId, usageTypeName);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _mockManagePostalAddress.Verify(
                x => x.ListPostalAddressForPersonAsync(realPageId, usageTypeName, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ListPostalAddressForPerson_WithEmptyGuid_UsesUserClaimsRealPageId()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim())
                .Returns(new DefaultUserClaim { UserRealPageGuid = userRealPageId });
            _mockManagePostalAddress
                .Setup(x => x.ListPostalAddressForPersonAsync(userRealPageId, "", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PostalAddress> { new PostalAddress() });

            // Act
            var result = await _postalAddressController.ListPostalAddressForPerson(Guid.Empty);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _mockManagePostalAddress.Verify(
                x => x.ListPostalAddressForPersonAsync(userRealPageId, "", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ListPostalAddressForPerson_WithEmptyGuidAndEmptyUserClaims_ReturnsBadRequest()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim())
                .Returns(new DefaultUserClaim { UserRealPageGuid = Guid.Empty });

            // Act
            var result = await _postalAddressController.ListPostalAddressForPerson(Guid.Empty);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequest.Value);
        }

        [Fact]
        public async Task ListPostalAddressForPerson_WhenEmptyListReturned_ReturnsOkWithEmptyList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            _mockManagePostalAddress
                .Setup(x => x.ListPostalAddressForPersonAsync(realPageId, "", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PostalAddress>());

            // Act
            var result = await _postalAddressController.ListPostalAddressForPerson(realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<PostalAddress, IErrorData>>(okResult.Value);
            Assert.Empty(output.list);
        }

        [Fact]
        public async Task ListPostalAddressForPerson_WhenNullReturned_ReturnsNoContent()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            _mockManagePostalAddress
                .Setup(x => x.ListPostalAddressForPersonAsync(realPageId, "", It.IsAny<CancellationToken>()))
                .ReturnsAsync((IList<PostalAddress>)null!);

            // Act
            var result = await _postalAddressController.ListPostalAddressForPerson(realPageId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        #endregion

        #region Helper Methods

        private static LinkPostalAddress CreateValidLinkPostalAddress()
        {
            return new LinkPostalAddress
            {
                PartyContactMechanism = new PartyContactMechanism
                {
                    PartyContactMechanismId = 0,
                    ContactMechanismId = 0
                },
                ContactMechanismUsageType = new ContactMechanismUsageType
                {
                    ContactMechanismUsageTypeId = 1
                },
                StreetAddress = new StreetAddress
                {
                    StreetAddress1 = "123 Main St"
                },
                ContactMechanismBoundary = new ContactMechanismBoundary(),
                GeographicBoundary = new List<GeographicBoundary>
                {
                    new GeographicBoundary { Name = "Texas" }
                }
            };
        }

        private void SetupSuccessfulLinkPostalAddressMocks(Guid realPageId)
        {
            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanismAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 100 });
            _mockManageContactMechanism
                .Setup(x => x.LinkContactMechanismToPartyAsync(realPageId, It.IsAny<IPartyContactMechanism>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 200 });
            _mockManageContactMechanism
                .Setup(x => x.LinkUsageTypeToPartyContactMechanismAsync(200, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });
            _mockManageStreetAddress
                .Setup(x => x.CreateStreetAddressAsync(It.IsAny<IStreetAddress>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });
            _mockManageGeographicBoundary
                .Setup(x => x.CreateGeographicBoundaryAsync(It.IsAny<IGeographicBoundary>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 300 });
            _mockManageContactMechanism
                .Setup(x => x.LinkGeographicBoundaryToContactMechanismAsync(It.IsAny<IContactMechanismBoundary>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });
        }

        private void SetupLinkPostalAddressMocksUntilStreetAddress(Guid realPageId)
        {
            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanismAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 100 });
            _mockManageContactMechanism
                .Setup(x => x.LinkContactMechanismToPartyAsync(realPageId, It.IsAny<IPartyContactMechanism>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 200 });
            _mockManageContactMechanism
                .Setup(x => x.LinkUsageTypeToPartyContactMechanismAsync(200, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });
            _mockManageStreetAddress
                .Setup(x => x.CreateStreetAddressAsync(It.IsAny<IStreetAddress>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });
        }

        private void SetupSuccessfulUpdatePostalAddressMocks(Guid realPageId)
        {
            _mockManageContactMechanism
                .SetupSequence(x => x.LinkContactMechanismToPartyAsync(realPageId, It.IsAny<IPartyContactMechanism>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 })
                .ReturnsAsync(new RepositoryResponse { Id = 200 });
            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanismAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 100 });
            _mockManageContactMechanism
                .Setup(x => x.LinkUsageTypeToPartyContactMechanismAsync(200, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });
            _mockManageStreetAddress
                .Setup(x => x.CreateStreetAddressAsync(It.IsAny<IStreetAddress>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });
            _mockManageGeographicBoundary
                .Setup(x => x.CreateGeographicBoundaryAsync(It.IsAny<IGeographicBoundary>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 300 });
            _mockManageContactMechanism
                .Setup(x => x.LinkGeographicBoundaryToContactMechanismAsync(It.IsAny<IContactMechanismBoundary>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });
        }

        private void SetupUpdatePostalAddressMocksUntilStreetAddress(Guid realPageId)
        {
            _mockManageContactMechanism
                .SetupSequence(x => x.LinkContactMechanismToPartyAsync(realPageId, It.IsAny<IPartyContactMechanism>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 })
                .ReturnsAsync(new RepositoryResponse { Id = 200 });
            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanismAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 100 });
            _mockManageContactMechanism
                .Setup(x => x.LinkUsageTypeToPartyContactMechanismAsync(200, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });
            _mockManageStreetAddress
                .Setup(x => x.CreateStreetAddressAsync(It.IsAny<IStreetAddress>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _postalAddressController = null!;
            base.Dispose();
        }

        #endregion
    }
}
