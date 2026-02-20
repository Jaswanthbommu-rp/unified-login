using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for PostalAddressController.
    /// Tests all endpoints, error cases, and edge cases for 100% code coverage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PostalAddressControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IPostalAddressRepository> _mockPostalAddressRepository;
        private readonly Mock<IManageContactMechanism> _mockManageContactMechanism;
        private readonly Mock<IManageStreetAddress> _mockManageStreetAddress;
        private readonly Mock<IManageGeographicBoundary> _mockManageGeographicBoundary;
        private readonly Mock<IManagePostalAddress> _mockManagePostalAddress;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private PostalAddressController _postalAddressController;

        #endregion

        #region Constructor

        public PostalAddressControllerTests()
        {
            _mockPostalAddressRepository = new Mock<IPostalAddressRepository>();
            _mockManageContactMechanism = new Mock<IManageContactMechanism>();
            _mockManageStreetAddress = new Mock<IManageStreetAddress>();
            _mockManageGeographicBoundary = new Mock<IManageGeographicBoundary>();
            _mockManagePostalAddress = new Mock<IManagePostalAddress>();
            _mockUserClaimsAccessor = MockUserClaimsAccessor;

            // Default setup: return a non-empty Guid so tests that don't override
            // this won't fail when the controller falls through to GetUserClaim().
            _mockUserClaimsAccessor
                .Setup(x => x.GetUserClaim())
                .Returns(new DefaultUserClaim { UserRealPageGuid = Guid.NewGuid() });

            _postalAddressController = new PostalAddressController(
                _mockPostalAddressRepository.Object,
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
            // Act
            var controller = new PostalAddressController(
                _mockPostalAddressRepository.Object,
                _mockManageContactMechanism.Object,
                _mockManageStreetAddress.Object,
                _mockManageGeographicBoundary.Object,
                _mockManagePostalAddress.Object,
                _mockUserClaimsAccessor.Object);

            // Assert
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

            SetupSuccessfulLinkPostalAddressMocks();

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
            var userClaim = new DefaultUserClaim { UserRealPageGuid = userRealPageId };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var linkPostalAddress = CreateValidLinkPostalAddress();

            SetupSuccessfulLinkPostalAddressMocks();

            // Act
            var result = await _postalAddressController.LinkPostalAddress(Guid.Empty, linkPostalAddress);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockManageContactMechanism.Verify(
                x => x.LinkContactMechanismToParty(userRealPageId, It.IsAny<IPartyContactMechanism>()),
                Times.Once);
        }

        #endregion

        #region LinkPostalAddress Tests - BadRequest

        [Fact]
        public async Task LinkPostalAddress_WithEmptyGuidAndEmptyUserClaims_ReturnsBadRequest()
        {
            // Arrange
            var userClaim = new DefaultUserClaim { UserRealPageGuid = Guid.Empty };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var linkPostalAddress = CreateValidLinkPostalAddress();

            // Act
            var result = await _postalAddressController.LinkPostalAddress(Guid.Empty, linkPostalAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkPostalAddress_WithNullLinkPostalAddress_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            // Act
            var result = await _postalAddressController.LinkPostalAddress(realPageId, null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Null parameter: linkPostalAddress.", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkPostalAddress_WhenCreateContactMechanismFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkPostalAddress = CreateValidLinkPostalAddress();
            const string errorMessage = "Failed to create contact mechanism";

            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanism())
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            // Act
            var result = await _postalAddressController.LinkPostalAddress(realPageId, linkPostalAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task LinkPostalAddress_WhenLinkContactMechanismToPartyFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkPostalAddress = CreateValidLinkPostalAddress();
            const string errorMessage = "Failed to link contact mechanism to party";

            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanism())
                .Returns(new RepositoryResponse { Id = 100 });

            _mockManageContactMechanism
                .Setup(x => x.LinkContactMechanismToParty(realPageId, It.IsAny<IPartyContactMechanism>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            // Act
            var result = await _postalAddressController.LinkPostalAddress(realPageId, linkPostalAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task LinkPostalAddress_WhenLinkUsageTypeToPartyContactMechanismFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkPostalAddress = CreateValidLinkPostalAddress();
            const string errorMessage = "Failed to link usage type";

            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanism())
                .Returns(new RepositoryResponse { Id = 100 });

            _mockManageContactMechanism
                .Setup(x => x.LinkContactMechanismToParty(realPageId, It.IsAny<IPartyContactMechanism>()))
                .Returns(new RepositoryResponse { Id = 200 });

            _mockManageContactMechanism
                .Setup(x => x.LinkUsageTypeToPartyContactMechanism(200, It.IsAny<int?>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            // Act
            var result = await _postalAddressController.LinkPostalAddress(realPageId, linkPostalAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task LinkPostalAddress_WhenCreateStreetAddressFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkPostalAddress = CreateValidLinkPostalAddress();
            const string errorMessage = "Failed to create street address";

            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanism())
                .Returns(new RepositoryResponse { Id = 100 });

            _mockManageContactMechanism
                .Setup(x => x.LinkContactMechanismToParty(realPageId, It.IsAny<IPartyContactMechanism>()))
                .Returns(new RepositoryResponse { Id = 200 });

            _mockManageContactMechanism
                .Setup(x => x.LinkUsageTypeToPartyContactMechanism(200, It.IsAny<int?>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockManageStreetAddress
                .Setup(x => x.CreateStreetAddress(It.IsAny<IStreetAddress>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            // Act
            var result = await _postalAddressController.LinkPostalAddress(realPageId, linkPostalAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task LinkPostalAddress_WhenCreateGeographicBoundaryFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkPostalAddress = CreateValidLinkPostalAddress();
            const string errorMessage = "Failed to create geographic boundary";

            SetupLinkPostalAddressMocksUntilStreetAddress(realPageId);

            _mockManageGeographicBoundary
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            // Act
            var result = await _postalAddressController.LinkPostalAddress(realPageId, linkPostalAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task LinkPostalAddress_WhenLinkGeographicBoundaryToContactMechanismFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkPostalAddress = CreateValidLinkPostalAddress();
            const string errorMessage = "Failed to link geographic boundary";

            SetupLinkPostalAddressMocksUntilStreetAddress(realPageId);

            _mockManageGeographicBoundary
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Returns(new RepositoryResponse { Id = 300 });

            _mockManageContactMechanism
                .Setup(x => x.LinkGeographicBoundaryToContactMechanism(It.IsAny<IContactMechanismBoundary>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            // Act
            var result = await _postalAddressController.LinkPostalAddress(realPageId, linkPostalAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
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
            var userClaim = new DefaultUserClaim { UserRealPageGuid = userRealPageId };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var linkPostalAddress = CreateValidLinkPostalAddress();

            SetupSuccessfulUpdatePostalAddressMocks(userRealPageId);

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(Guid.Empty, linkPostalAddress);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdatePostalAddress Tests - BadRequest

        [Fact]
        public async Task UpdatePostalAddress_WithEmptyGuidAndEmptyUserClaims_ReturnsBadRequest()
        {
            // Arrange
            var userClaim = new DefaultUserClaim { UserRealPageGuid = Guid.Empty };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var linkPostalAddress = CreateValidLinkPostalAddress();

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(Guid.Empty, linkPostalAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdatePostalAddress_WithNullLinkPostalAddress_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(realPageId, null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Null parameter: linkPostalAddress.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdatePostalAddress_WhenExpireLinkContactMechanismToPartyFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkPostalAddress = CreateValidLinkPostalAddress();
            const string errorMessage = "Failed to expire contact mechanism";

            _mockManageContactMechanism
                .Setup(x => x.LinkContactMechanismToParty(realPageId, It.IsAny<IPartyContactMechanism>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(realPageId, linkPostalAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task UpdatePostalAddress_WhenCreateContactMechanismFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkPostalAddress = CreateValidLinkPostalAddress();
            const string errorMessage = "Failed to create contact mechanism";

            // First call for expire
            _mockManageContactMechanism
                .SetupSequence(x => x.LinkContactMechanismToParty(realPageId, It.IsAny<IPartyContactMechanism>()))
                .Returns(new RepositoryResponse { Id = 1 })
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = "Second call failed" });

            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanism())
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(realPageId, linkPostalAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task UpdatePostalAddress_WhenLinkNewContactMechanismToPartyFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkPostalAddress = CreateValidLinkPostalAddress();
            const string errorMessage = "Failed to link new contact mechanism";

            // First call for expire succeeds, second call for new link fails
            _mockManageContactMechanism
                .SetupSequence(x => x.LinkContactMechanismToParty(realPageId, It.IsAny<IPartyContactMechanism>()))
                .Returns(new RepositoryResponse { Id = 1 })
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanism())
                .Returns(new RepositoryResponse { Id = 100 });

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(realPageId, linkPostalAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task UpdatePostalAddress_WhenLinkUsageTypeFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkPostalAddress = CreateValidLinkPostalAddress();
            const string errorMessage = "Failed to link usage type";

            _mockManageContactMechanism
                .SetupSequence(x => x.LinkContactMechanismToParty(realPageId, It.IsAny<IPartyContactMechanism>()))
                .Returns(new RepositoryResponse { Id = 1 })
                .Returns(new RepositoryResponse { Id = 200 });

            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanism())
                .Returns(new RepositoryResponse { Id = 100 });

            _mockManageContactMechanism
                .Setup(x => x.LinkUsageTypeToPartyContactMechanism(200, It.IsAny<int?>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(realPageId, linkPostalAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task UpdatePostalAddress_WhenCreateStreetAddressFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkPostalAddress = CreateValidLinkPostalAddress();
            const string errorMessage = "Failed to create street address";

            _mockManageContactMechanism
                .SetupSequence(x => x.LinkContactMechanismToParty(realPageId, It.IsAny<IPartyContactMechanism>()))
                .Returns(new RepositoryResponse { Id = 1 })
                .Returns(new RepositoryResponse { Id = 200 });

            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanism())
                .Returns(new RepositoryResponse { Id = 100 });

            _mockManageContactMechanism
                .Setup(x => x.LinkUsageTypeToPartyContactMechanism(200, It.IsAny<int?>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockManageStreetAddress
                .Setup(x => x.CreateStreetAddress(It.IsAny<IStreetAddress>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(realPageId, linkPostalAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task UpdatePostalAddress_WhenCreateGeographicBoundaryFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkPostalAddress = CreateValidLinkPostalAddress();
            const string errorMessage = "Failed to create geographic boundary";

            SetupUpdatePostalAddressMocksUntilStreetAddress(realPageId);

            _mockManageGeographicBoundary
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(realPageId, linkPostalAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task UpdatePostalAddress_WhenLinkGeographicBoundaryFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkPostalAddress = CreateValidLinkPostalAddress();
            const string errorMessage = "Failed to link geographic boundary";

            SetupUpdatePostalAddressMocksUntilStreetAddress(realPageId);

            _mockManageGeographicBoundary
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Returns(new RepositoryResponse { Id = 300 });

            _mockManageContactMechanism
                .Setup(x => x.LinkGeographicBoundaryToContactMechanism(It.IsAny<IContactMechanismBoundary>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            // Act
            var result = await _postalAddressController.UpdatePostalAddress(realPageId, linkPostalAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        #endregion

        #region ListPostalAddressForPerson Tests

        [Fact]
        public async Task ListPostalAddressForPerson_WithValidData_ReturnsOkWithList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var postalAddressList = new List<PostalAddress>
            {
                new PostalAddress(),
                new PostalAddress()
            };

            _mockManagePostalAddress
                .Setup(x => x.ListPostalAddressForPerson(It.IsAny<Guid>(), ""))
                .Returns(postalAddressList);

            // Act
            var result = await _postalAddressController.ListPostalAddressForPerson(realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<PostalAddress, IErrorData>>(okResult.Value);
            Assert.Equal(2, output.list.Count);
        }

        [Fact]
        public async Task ListPostalAddressForPerson_WithUsageTypeName_PassesToRepository()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string usageTypeName = "Home";
            var postalAddressList = new List<PostalAddress> { new PostalAddress() };

            _mockManagePostalAddress
                .Setup(x => x.ListPostalAddressForPerson(It.IsAny<Guid>(), usageTypeName))
                .Returns(postalAddressList);

            // Act
            var result = await _postalAddressController.ListPostalAddressForPerson(realPageId, usageTypeName);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _mockManagePostalAddress.Verify(
                x => x.ListPostalAddressForPerson(realPageId, usageTypeName),
                Times.Once);
        }

        [Fact]
        public async Task ListPostalAddressForPerson_WithEmptyGuid_UsesUserClaimsRealPageId()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            var userClaim = new DefaultUserClaim { UserRealPageGuid = userRealPageId };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var postalAddressList = new List<PostalAddress> { new PostalAddress() };

            _mockManagePostalAddress
                .Setup(x => x.ListPostalAddressForPerson(It.IsAny<Guid>(), ""))
                .Returns(postalAddressList);

            // Act
            var result = await _postalAddressController.ListPostalAddressForPerson(Guid.Empty);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _mockManagePostalAddress.Verify(
                x => x.ListPostalAddressForPerson(userRealPageId, ""),
                Times.Once);
        }

        [Fact]
        public async Task ListPostalAddressForPerson_WithEmptyGuidAndEmptyUserClaims_ReturnsBadRequest()
        {
            // Arrange
            var userClaim = new DefaultUserClaim { UserRealPageGuid = Guid.Empty };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            // Act
            var result = await _postalAddressController.ListPostalAddressForPerson(Guid.Empty);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        [Fact]
        public async Task ListPostalAddressForPerson_WhenNoAddressesFound_ReturnsOkWithEmptyList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            _mockManagePostalAddress
                .Setup(x => x.ListPostalAddressForPerson(It.IsAny<Guid>(), ""))
                .Returns(new List<PostalAddress>());

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
                .Setup(x => x.ListPostalAddressForPerson(It.IsAny<Guid>(), ""))
                .Returns((IList<PostalAddress>)null!);

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

        private void SetupSuccessfulLinkPostalAddressMocks()
        {
            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanism())
                .Returns(new RepositoryResponse { Id = 100 });

            _mockManageContactMechanism
                .Setup(x => x.LinkContactMechanismToParty(It.IsAny<Guid>(), It.IsAny<IPartyContactMechanism>()))
                .Returns(new RepositoryResponse { Id = 200 });

            _mockManageContactMechanism
                .Setup(x => x.LinkUsageTypeToPartyContactMechanism(200, It.IsAny<int?>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockManageStreetAddress
                .Setup(x => x.CreateStreetAddress(It.IsAny<IStreetAddress>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockManageGeographicBoundary
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Returns(new RepositoryResponse { Id = 300 });

            _mockManageContactMechanism
                .Setup(x => x.LinkGeographicBoundaryToContactMechanism(It.IsAny<IContactMechanismBoundary>()))
                .Returns(new RepositoryResponse { Id = 1 });
        }

        private void SetupLinkPostalAddressMocksUntilStreetAddress(Guid realPageId)
        {
            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanism())
                .Returns(new RepositoryResponse { Id = 100 });

            _mockManageContactMechanism
                .Setup(x => x.LinkContactMechanismToParty(realPageId, It.IsAny<IPartyContactMechanism>()))
                .Returns(new RepositoryResponse { Id = 200 });

            _mockManageContactMechanism
                .Setup(x => x.LinkUsageTypeToPartyContactMechanism(200, It.IsAny<int?>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockManageStreetAddress
                .Setup(x => x.CreateStreetAddress(It.IsAny<IStreetAddress>()))
                .Returns(new RepositoryResponse { Id = 1 });
        }

        private void SetupSuccessfulUpdatePostalAddressMocks(Guid realPageId)
        {
            _mockManageContactMechanism
                .SetupSequence(x => x.LinkContactMechanismToParty(realPageId, It.IsAny<IPartyContactMechanism>()))
                .Returns(new RepositoryResponse { Id = 1 })
                .Returns(new RepositoryResponse { Id = 200 });

            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanism())
                .Returns(new RepositoryResponse { Id = 100 });

            _mockManageContactMechanism
                .Setup(x => x.LinkUsageTypeToPartyContactMechanism(200, It.IsAny<int?>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockManageStreetAddress
                .Setup(x => x.CreateStreetAddress(It.IsAny<IStreetAddress>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockManageGeographicBoundary
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Returns(new RepositoryResponse { Id = 300 });

            _mockManageContactMechanism
                .Setup(x => x.LinkGeographicBoundaryToContactMechanism(It.IsAny<IContactMechanismBoundary>()))
                .Returns(new RepositoryResponse { Id = 1 });
        }

        private void SetupUpdatePostalAddressMocksUntilStreetAddress(Guid realPageId)
        {
            _mockManageContactMechanism
                .SetupSequence(x => x.LinkContactMechanismToParty(realPageId, It.IsAny<IPartyContactMechanism>()))
                .Returns(new RepositoryResponse { Id = 1 })
                .Returns(new RepositoryResponse { Id = 200 });

            _mockManageContactMechanism
                .Setup(x => x.CreateContactMechanism())
                .Returns(new RepositoryResponse { Id = 100 });

            _mockManageContactMechanism
                .Setup(x => x.LinkUsageTypeToPartyContactMechanism(200, It.IsAny<int?>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockManageStreetAddress
                .Setup(x => x.CreateStreetAddress(It.IsAny<IStreetAddress>()))
                .Returns(new RepositoryResponse { Id = 1 });
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

































