using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
    /// Comprehensive unit tests for ElectronicAddressController.
    /// Tests all endpoints, error cases, and edge cases for 100% code coverage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ElectronicAddressControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IElectronicAddressRepositoryAsync> _mockElectronicAddressRepository;
        private readonly Mock<IContactMechanismRepositoryAsync> _mockContactMechanismRepository;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private ElectronicAddressController _electronicAddressController;

        #endregion

        #region Constructor

        public ElectronicAddressControllerTests()
        {
            _mockElectronicAddressRepository = new Mock<IElectronicAddressRepositoryAsync>();
            _mockContactMechanismRepository = new Mock<IContactMechanismRepositoryAsync>();
            _mockUserClaimsAccessor = MockUserClaimsAccessor;

            _electronicAddressController = new ElectronicAddressController(
                _mockElectronicAddressRepository.Object,
                _mockContactMechanismRepository.Object,
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
            var controller = new ElectronicAddressController(
                _mockElectronicAddressRepository.Object,
                _mockContactMechanismRepository.Object,
                _mockUserClaimsAccessor.Object);

            // Assert
            Assert.NotNull(controller);
        }

        #endregion

        #region LinkElectronicAddress Tests - Success

        [Fact]
        public async Task LinkElectronicAddress_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkElectronicAddress = CreateValidLinkElectronicAddress();

            _mockContactMechanismRepository
                .Setup(x => x.CreateContactMechanismAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 100 });

            _mockContactMechanismRepository
                .Setup(x => x.LinkContactMechanismToPartyAsync(realPageId, It.IsAny<IPartyContactMechanism>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 200 });

            _mockContactMechanismRepository
                .Setup(x => x.LinkUsageTypeToPartyContactMechanismAsync(It.IsAny<long>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            _mockElectronicAddressRepository
                .Setup(x => x.CreateElectronicAddressAsync(It.IsAny<IElectronicAddress>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _electronicAddressController.LinkElectronicAddress(realPageId, linkElectronicAddress);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var outputResult = Assert.IsType<ElectronicAddress.ElectronicAddressOutputResult>(okResult.Value);
            Assert.Equal(100, outputResult.ContactMechanismId);
        }

        [Fact]
        public async Task LinkElectronicAddress_WithEmptyGuid_UsesUserClaimsRealPageId()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            var userClaim = new DefaultUserClaim { UserRealPageGuid = userRealPageId };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var linkElectronicAddress = CreateValidLinkElectronicAddress();

            _mockContactMechanismRepository
                .Setup(x => x.CreateContactMechanismAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 100 });

            _mockContactMechanismRepository
                .Setup(x => x.LinkContactMechanismToPartyAsync(userRealPageId, It.IsAny<IPartyContactMechanism>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 200 });

            _mockContactMechanismRepository
                .Setup(x => x.LinkUsageTypeToPartyContactMechanismAsync(It.IsAny<long>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            _mockElectronicAddressRepository
                .Setup(x => x.CreateElectronicAddressAsync(It.IsAny<IElectronicAddress>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _electronicAddressController.LinkElectronicAddress(Guid.Empty, linkElectronicAddress);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockContactMechanismRepository.Verify(
                x => x.LinkContactMechanismToPartyAsync(userRealPageId, It.IsAny<IPartyContactMechanism>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region LinkElectronicAddress Tests - BadRequest

        [Fact]
        public async Task LinkElectronicAddress_WithEmptyGuidAndEmptyUserClaim_ReturnsBadRequest()
        {
            // Arrange
            var userClaim = new DefaultUserClaim { UserRealPageGuid = Guid.Empty };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var linkElectronicAddress = CreateValidLinkElectronicAddress();

            // Act
            var result = await _electronicAddressController.LinkElectronicAddress(Guid.Empty, linkElectronicAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkElectronicAddress_WithNullLinkElectronicAddress_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            // Act
            var result = await _electronicAddressController.LinkElectronicAddress(realPageId, null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Null parameter: linkElectronicAddress.", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkElectronicAddress_WhenCreateContactMechanismFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkElectronicAddress = CreateValidLinkElectronicAddress();

            _mockContactMechanismRepository
                .Setup(x => x.CreateContactMechanismAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = "Failed to create contact mechanism" });

            // Act
            var result = await _electronicAddressController.LinkElectronicAddress(realPageId, linkElectronicAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to create contact mechanism", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkElectronicAddress_WhenLinkContactMechanismToPartyFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkElectronicAddress = CreateValidLinkElectronicAddress();

            _mockContactMechanismRepository
                .Setup(x => x.CreateContactMechanismAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 100 });

            _mockContactMechanismRepository
                .Setup(x => x.LinkContactMechanismToPartyAsync(realPageId, It.IsAny<IPartyContactMechanism>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = "Failed to link contact mechanism to party" });

            // Act
            var result = await _electronicAddressController.LinkElectronicAddress(realPageId, linkElectronicAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to link contact mechanism to party", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkElectronicAddress_WhenLinkUsageTypeFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkElectronicAddress = CreateValidLinkElectronicAddress();

            _mockContactMechanismRepository
                .Setup(x => x.CreateContactMechanismAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 100 });

            _mockContactMechanismRepository
                .Setup(x => x.LinkContactMechanismToPartyAsync(realPageId, It.IsAny<IPartyContactMechanism>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 200 });

            _mockContactMechanismRepository
                .Setup(x => x.LinkUsageTypeToPartyContactMechanismAsync(It.IsAny<long>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = "Failed to link usage type" });

            // Act
            var result = await _electronicAddressController.LinkElectronicAddress(realPageId, linkElectronicAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to link usage type", badRequestResult.Value);
        }

        [Fact]
        public async Task LinkElectronicAddress_WhenCreateElectronicAddressFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkElectronicAddress = CreateValidLinkElectronicAddress();

            _mockContactMechanismRepository
                .Setup(x => x.CreateContactMechanismAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 100 });

            _mockContactMechanismRepository
                .Setup(x => x.LinkContactMechanismToPartyAsync(realPageId, It.IsAny<IPartyContactMechanism>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 200 });

            _mockContactMechanismRepository
                .Setup(x => x.LinkUsageTypeToPartyContactMechanismAsync(It.IsAny<long>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            _mockElectronicAddressRepository
                .Setup(x => x.CreateElectronicAddressAsync(It.IsAny<IElectronicAddress>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = "Failed to create electronic address" });

            // Act
            var result = await _electronicAddressController.LinkElectronicAddress(realPageId, linkElectronicAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to create electronic address", badRequestResult.Value);
        }

        #endregion

        #region UpdateElectronicAddress Tests - Success

        [Fact]
        public async Task UpdateElectronicAddress_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkElectronicAddress = CreateValidLinkElectronicAddress();
            linkElectronicAddress.PartyContactMechanism.ContactMechanismId = 100;
            linkElectronicAddress.PartyContactMechanism.PartyContactMechanismId = 200;

            _mockContactMechanismRepository
                .Setup(x => x.UpdateContactMechanismUsageForPartyAsync(200, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            _mockElectronicAddressRepository
                .Setup(x => x.CreateElectronicAddressAsync(It.IsAny<IElectronicAddress>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _electronicAddressController.UpdateElectronicAddress(realPageId, linkElectronicAddress);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var outputResult = Assert.IsType<ElectronicAddress.ElectronicAddressOutputResult>(okResult.Value);
            Assert.Equal(100, outputResult.ContactMechanismId);
        }

        [Fact]
        public async Task UpdateElectronicAddress_WithEmptyGuid_UsesUserClaimsRealPageId()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            var userClaim = new DefaultUserClaim { UserRealPageGuid = userRealPageId };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var linkElectronicAddress = CreateValidLinkElectronicAddress();
            linkElectronicAddress.PartyContactMechanism.ContactMechanismId = 100;
            linkElectronicAddress.PartyContactMechanism.PartyContactMechanismId = 200;

            _mockContactMechanismRepository
                .Setup(x => x.UpdateContactMechanismUsageForPartyAsync(It.IsAny<long>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            _mockElectronicAddressRepository
                .Setup(x => x.CreateElectronicAddressAsync(It.IsAny<IElectronicAddress>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _electronicAddressController.UpdateElectronicAddress(Guid.Empty, linkElectronicAddress);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdateElectronicAddress Tests - BadRequest

        [Fact]
        public async Task UpdateElectronicAddress_WithEmptyGuidAndEmptyUserClaim_ReturnsBadRequest()
        {
            // Arrange
            var userClaim = new DefaultUserClaim { UserRealPageGuid = Guid.Empty };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var linkElectronicAddress = CreateValidLinkElectronicAddress();

            // Act
            var result = await _electronicAddressController.UpdateElectronicAddress(Guid.Empty, linkElectronicAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateElectronicAddress_WithNullLinkElectronicAddress_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            // Act
            var result = await _electronicAddressController.UpdateElectronicAddress(realPageId, null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Null parameter: linkElectronicAddress.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateElectronicAddress_WhenUpdateContactMechanismUsageFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkElectronicAddress = CreateValidLinkElectronicAddress();
            linkElectronicAddress.PartyContactMechanism.PartyContactMechanismId = 200;

            _mockContactMechanismRepository
                .Setup(x => x.UpdateContactMechanismUsageForPartyAsync(200, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = "Failed to update usage" });

            // Act
            var result = await _electronicAddressController.UpdateElectronicAddress(realPageId, linkElectronicAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to update usage", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateElectronicAddress_WhenCreateElectronicAddressFails_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkElectronicAddress = CreateValidLinkElectronicAddress();
            linkElectronicAddress.PartyContactMechanism.ContactMechanismId = 100;
            linkElectronicAddress.PartyContactMechanism.PartyContactMechanismId = 200;

            _mockContactMechanismRepository
                .Setup(x => x.UpdateContactMechanismUsageForPartyAsync(200, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            _mockElectronicAddressRepository
                .Setup(x => x.CreateElectronicAddressAsync(It.IsAny<IElectronicAddress>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 0, ErrorMessage = "Failed to create electronic address" });

            // Act
            var result = await _electronicAddressController.UpdateElectronicAddress(realPageId, linkElectronicAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to create electronic address", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateElectronicAddress_WithNullPartyContactMechanism_SetsContactMechanismIdToZero()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var linkElectronicAddress = CreateValidLinkElectronicAddress();
            linkElectronicAddress.PartyContactMechanism.ContactMechanismId = 0;
            linkElectronicAddress.PartyContactMechanism.PartyContactMechanismId = 200;

            _mockContactMechanismRepository
                .Setup(x => x.UpdateContactMechanismUsageForPartyAsync(200, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            _mockElectronicAddressRepository
                .Setup(x => x.CreateElectronicAddressAsync(It.IsAny<IElectronicAddress>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _electronicAddressController.UpdateElectronicAddress(realPageId, linkElectronicAddress);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var outputResult = Assert.IsType<ElectronicAddress.ElectronicAddressOutputResult>(okResult.Value);
            Assert.Equal(0, outputResult.ContactMechanismId);
        }

        #endregion

        #region ListElectronicAddressForPerson Tests - Success

        [Fact]
        public async Task ListElectronicAddressForPerson_WithValidRealPageId_ReturnsOkWithData()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var expectedAddresses = new List<ElectronicAddress>
            {
                new ElectronicAddress { ContactMechanismId = 1, AddressString = "test1@example.com" },
                new ElectronicAddress { ContactMechanismId = 2, AddressString = "test2@example.com" }
            };

            _mockElectronicAddressRepository
                .Setup(x => x.ListElectronicAddressForPersonAsync(realPageId, "", It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAddresses);

            // Act
            var result = await _electronicAddressController.ListElectronicAddressForPerson(realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ElectronicAddress, IErrorData>>(okResult.Value);
            Assert.Equal(2, output.list.Count);
        }

        [Fact]
        public async Task ListElectronicAddressForPerson_WithUsageTypeName_ReturnsFilteredData()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string usageTypeName = "Personal";
            var expectedAddresses = new List<ElectronicAddress>
            {
                new ElectronicAddress { ContactMechanismId = 1, AddressString = "personal@example.com" }
            };

            _mockElectronicAddressRepository
                .Setup(x => x.ListElectronicAddressForPersonAsync(realPageId, usageTypeName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAddresses);

            // Act
            var result = await _electronicAddressController.ListElectronicAddressForPerson(realPageId, usageTypeName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ElectronicAddress, IErrorData>>(okResult.Value);
            Assert.Single(output.list);
        }

        [Fact]
        public async Task ListElectronicAddressForPerson_WithEmptyGuid_UsesUserClaimsRealPageId()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            var userClaim = new DefaultUserClaim { UserRealPageGuid = userRealPageId };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var expectedAddresses = new List<ElectronicAddress>
            {
                new ElectronicAddress { ContactMechanismId = 1, AddressString = "user@example.com" }
            };

            _mockElectronicAddressRepository
                .Setup(x => x.ListElectronicAddressForPersonAsync(userRealPageId, "", It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAddresses);

            // Act
            var result = await _electronicAddressController.ListElectronicAddressForPerson(Guid.Empty);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockElectronicAddressRepository.Verify(
                x => x.ListElectronicAddressForPersonAsync(userRealPageId, "", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region ListElectronicAddressForPerson Tests - NoContent

        [Fact]
        public async Task ListElectronicAddressForPerson_WhenRepositoryReturnsNull_ReturnsNoContent()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            _mockElectronicAddressRepository
                .Setup(x => x.ListElectronicAddressForPersonAsync(realPageId, "", It.IsAny<CancellationToken>()))
                .ReturnsAsync((IList<ElectronicAddress>)null!);

            // Act
            var result = await _electronicAddressController.ListElectronicAddressForPerson(realPageId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ListElectronicAddressForPerson_WhenRepositoryReturnsEmptyList_ReturnsNoContent()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            _mockElectronicAddressRepository
                .Setup(x => x.ListElectronicAddressForPersonAsync(realPageId, "", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ElectronicAddress>());

            // Act
            var result = await _electronicAddressController.ListElectronicAddressForPerson(realPageId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        #endregion

        #region ListElectronicAddressForPerson Tests - BadRequest

        [Fact]
        public async Task ListElectronicAddressForPerson_WithEmptyGuidAndEmptyUserClaim_ReturnsBadRequest()
        {
            // Arrange
            var userClaim = new DefaultUserClaim { UserRealPageGuid = Guid.Empty };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            // Act
            var result = await _electronicAddressController.ListElectronicAddressForPerson(Guid.Empty);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        [Fact]
        public async Task ListElectronicAddressForPerson_WithNullUserClaim_ReturnsBadRequest()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            // Act
            var result = await _electronicAddressController.ListElectronicAddressForPerson(Guid.Empty);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        #endregion

        #region Helper Methods

        private static LinkElectronicAddress CreateValidLinkElectronicAddress()
        {
            return new LinkElectronicAddress
            {
                PartyContactMechanism = new PartyContactMechanism
                {
                    ContactMechanismId = 0,
                    PartyContactMechanismId = 0
                },
                ContactMechanismUsageType = new ContactMechanismUsageType
                {
                    ContactMechanismUsageTypeId = 1,
                    Name = "Personal"
                },
                ElectronicAddress = new ElectronicAddress
                {
                    AddressString = "test@example.com",
                    AddressType = "Email"
                }
            };
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _electronicAddressController = null!;
            base.Dispose();
        }

        #endregion
    }
}
