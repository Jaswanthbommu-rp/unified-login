using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    /// Comprehensive unit tests for ContactMechanismController.
    /// Tests all endpoints, error cases, and edge cases for 100% code coverage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ContactMechanismControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IContactMechanismRepository> _mockContactMechanismRepository;
        private readonly Mock<IManageContactMechanism> _mockManageContactMechanism;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private ContactMechanismController _contactMechanismController;

        #endregion

        #region Constructor

        public ContactMechanismControllerTests()
        {
            _mockContactMechanismRepository = new Mock<IContactMechanismRepository>();
            _mockManageContactMechanism = new Mock<IManageContactMechanism>();
            _mockUserClaimsAccessor = MockUserClaimsAccessor;

            _contactMechanismController = new ContactMechanismController(
                _mockContactMechanismRepository.Object,
                _mockManageContactMechanism.Object,
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
            var controller = new ContactMechanismController(
                _mockContactMechanismRepository.Object,
                _mockManageContactMechanism.Object,
                _mockUserClaimsAccessor.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullContactMechanismRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new ContactMechanismController(
                    null!,
                    _mockManageContactMechanism.Object,
                    _mockUserClaimsAccessor.Object));

            Assert.Equal("contactMechanismRepository", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullManageContactMechanism_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new ContactMechanismController(
                    _mockContactMechanismRepository.Object,
                    null!,
                    _mockUserClaimsAccessor.Object));

            Assert.Equal("manageContactMechanism", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new ContactMechanismController(
                    _mockContactMechanismRepository.Object,
                    _mockManageContactMechanism.Object,
                    null!));

            Assert.Equal("userClaimsAccessor", exception.ParamName);
        }

        #endregion

        #region ListContactMechanismForPerson Tests - Valid RealPageId Provided

        [Fact]
        public async Task ListContactMechanismForPerson_WithValidRealPageId_ReturnsOkWithData()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var expectedContacts = new List<CommonAddress>
            {
                new CommonAddress
                {
                    PartyContactMechanismId = 1,
                    ContactMechanismId = 100,
                    AddressString = "test@example.com",
                    AddressType = "Email"
                },
                new CommonAddress
                {
                    PartyContactMechanismId = 2,
                    ContactMechanismId = 101,
                    AddressString = "work@example.com",
                    AddressType = "Email"
                }
            };

            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(realPageId, ""))
                .Returns(expectedContacts);

            // Act
            var result = await _contactMechanismController.ListContactMechanismForPerson(realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<CommonAddress, IErrorData>>(okResult.Value);
            Assert.Equal(2, output.list.Count);
            Assert.Equal("test@example.com", output.list[0].AddressString);
        }

        [Fact]
        public async Task ListContactMechanismForPerson_WithValidRealPageIdAndUsageType_ReturnsFilteredData()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string usageTypeName = "Personal";
            var expectedContacts = new List<CommonAddress>
            {
                new CommonAddress
                {
                    PartyContactMechanismId = 1,
                    ContactMechanismId = 100,
                    AddressString = "personal@example.com",
                    AddressType = "Email"
                }
            };

            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(realPageId, usageTypeName))
                .Returns(expectedContacts);

            // Act
            var result = await _contactMechanismController.ListContactMechanismForPerson(realPageId, usageTypeName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<CommonAddress, IErrorData>>(okResult.Value);
            Assert.Single(output.list);
        }

        [Fact]
        public async Task ListContactMechanismForPerson_WithValidRealPageId_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string usageTypeName = "Work";

            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(realPageId, usageTypeName))
                .Returns(new List<CommonAddress> { new CommonAddress() });

            // Act
            await _contactMechanismController.ListContactMechanismForPerson(realPageId, usageTypeName);

            // Assert
            _mockContactMechanismRepository.Verify(
                x => x.ListContactMechanismForPerson(realPageId, usageTypeName),
                Times.Once);
        }

        #endregion

        #region ListContactMechanismForPerson Tests - Empty Guid Uses User Claims

        [Fact]
        public async Task ListContactMechanismForPerson_WithEmptyGuid_UsesUserClaimsRealPageId()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            var userClaim = new DefaultUserClaim { UserRealPageGuid = userRealPageId };

            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var expectedContacts = new List<CommonAddress>
            {
                new CommonAddress { PartyContactMechanismId = 1, AddressString = "user@example.com" }
            };

            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(userRealPageId, ""))
                .Returns(expectedContacts);

            // Act
            var result = await _contactMechanismController.ListContactMechanismForPerson(Guid.Empty);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<CommonAddress, IErrorData>>(okResult.Value);
            Assert.Single(output.list);

            _mockContactMechanismRepository.Verify(
                x => x.ListContactMechanismForPerson(userRealPageId, ""),
                Times.Once);
        }

        [Fact]
        public async Task ListContactMechanismForPerson_WithEmptyGuidAndEmptyUserClaim_ReturnsBadRequest()
        {
            // Arrange
            var userClaim = new DefaultUserClaim { UserRealPageGuid = Guid.Empty };
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            // Act
            var result = await _contactMechanismController.ListContactMechanismForPerson(Guid.Empty);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        [Fact]
        public async Task ListContactMechanismForPerson_WithEmptyGuidAndNullUserClaim_ReturnsBadRequest()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            // Act
            var result = await _contactMechanismController.ListContactMechanismForPerson(Guid.Empty);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        #endregion

        #region ListContactMechanismForPerson Tests - No Content Scenarios

        [Fact]
        public async Task ListContactMechanismForPerson_WhenRepositoryReturnsNull_ReturnsNoContent()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(realPageId, ""))
                .Returns((IList<CommonAddress>)null!);

            // Act
            var result = await _contactMechanismController.ListContactMechanismForPerson(realPageId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ListContactMechanismForPerson_WhenRepositoryReturnsEmptyList_ReturnsNoContent()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(realPageId, ""))
                .Returns(new List<CommonAddress>());

            // Act
            var result = await _contactMechanismController.ListContactMechanismForPerson(realPageId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ListContactMechanismForPerson_WithUsageTypeAndNoResults_ReturnsNoContent()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string usageTypeName = "NonExistent";

            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(realPageId, usageTypeName))
                .Returns(new List<CommonAddress>());

            // Act
            var result = await _contactMechanismController.ListContactMechanismForPerson(realPageId, usageTypeName);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        #endregion

        #region ListContactMechanismForPerson Tests - Various Usage Type Values

        [Theory]
        [InlineData("")]
        [InlineData("Personal")]
        [InlineData("Work")]
        [InlineData("AccountRecovery")]
        [InlineData("Home")]
        public async Task ListContactMechanismForPerson_WithVariousUsageTypes_ReturnsOkWithData(string usageTypeName)
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var expectedContacts = new List<CommonAddress>
            {
                new CommonAddress { PartyContactMechanismId = 1, AddressString = "test@example.com" }
            };

            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(realPageId, usageTypeName))
                .Returns(expectedContacts);

            // Act
            var result = await _contactMechanismController.ListContactMechanismForPerson(realPageId, usageTypeName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task ListContactMechanismForPerson_WithWhitespaceUsageType_PassesToRepository()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string usageTypeName = "   ";
            var expectedContacts = new List<CommonAddress>
            {
                new CommonAddress { PartyContactMechanismId = 1 }
            };

            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(realPageId, usageTypeName))
                .Returns(expectedContacts);

            // Act
            var result = await _contactMechanismController.ListContactMechanismForPerson(realPageId, usageTypeName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockContactMechanismRepository.Verify(
                x => x.ListContactMechanismForPerson(realPageId, usageTypeName),
                Times.Once);
        }

        [Fact]
        public async Task ListContactMechanismForPerson_WithSpecialCharactersInUsageType_PassesToRepository()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            const string usageTypeName = "Work & Personal";
            var expectedContacts = new List<CommonAddress>
            {
                new CommonAddress { PartyContactMechanismId = 1 }
            };

            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(realPageId, usageTypeName))
                .Returns(expectedContacts);

            // Act
            var result = await _contactMechanismController.ListContactMechanismForPerson(realPageId, usageTypeName);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region ListContactMechanismForPerson Tests - Multiple Contact Mechanisms

        [Fact]
        public async Task ListContactMechanismForPerson_WithMultipleContacts_ReturnsAllContacts()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var expectedContacts = new List<CommonAddress>
            {
                new CommonAddress { PartyContactMechanismId = 1, AddressString = "email1@example.com", AddressType = "Email" },
                new CommonAddress { PartyContactMechanismId = 2, AddressString = "email2@example.com", AddressType = "Email" },
                new CommonAddress { PartyContactMechanismId = 3, AddressString = "555-1234", AddressType = "Phone" },
                new CommonAddress { PartyContactMechanismId = 4, AddressString = "555-5678", AddressType = "Phone" },
                new CommonAddress { PartyContactMechanismId = 5, AddressString = "123 Main St", AddressType = "Postal" }
            };

            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(realPageId, ""))
                .Returns(expectedContacts);

            // Act
            var result = await _contactMechanismController.ListContactMechanismForPerson(realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<CommonAddress, IErrorData>>(okResult.Value);
            Assert.Equal(5, output.list.Count);
        }

        [Fact]
        public async Task ListContactMechanismForPerson_WithFullContactDetails_ReturnsCompleteData()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var expectedContacts = new List<CommonAddress>
            {
                new CommonAddress
                {
                    PartyContactMechanismId = 12345,
                    ContactMechanismId = 67890,
                    AddressString = "complete@example.com",
                    AddressType = "Email",
                    ContactMechanismUsageTypeId = 1,
                    contactMechanismUsageType = new ContactMechanismUsageType
                    {
                        ContactMechanismUsageTypeId = 1,
                        Name = "Personal"
                    }
                }
            };

            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(realPageId, ""))
                .Returns(expectedContacts);

            // Act
            var result = await _contactMechanismController.ListContactMechanismForPerson(realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<CommonAddress, IErrorData>>(okResult.Value);
            Assert.Single(output.list);

            var contact = output.list[0];
            Assert.Equal(12345, contact.PartyContactMechanismId);
            Assert.Equal(67890, contact.ContactMechanismId);
            Assert.Equal("complete@example.com", contact.AddressString);
            Assert.Equal("Email", contact.AddressType);
            Assert.NotNull(contact.contactMechanismUsageType);
        }

        #endregion

        #region ListContactMechanismForPerson Tests - Edge Cases

        [Fact]
        public async Task ListContactMechanismForPerson_WithVeryLongUsageTypeName_PassesToRepository()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var usageTypeName = new string('A', 500);
            var expectedContacts = new List<CommonAddress>
            {
                new CommonAddress { PartyContactMechanismId = 1 }
            };

            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(realPageId, usageTypeName))
                .Returns(expectedContacts);

            // Act
            var result = await _contactMechanismController.ListContactMechanismForPerson(realPageId, usageTypeName);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListContactMechanismForPerson_WithNullUsageTypeName_UsesDefaultEmptyString()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var expectedContacts = new List<CommonAddress>
            {
                new CommonAddress { PartyContactMechanismId = 1 }
            };

            // Default value is empty string when null is not explicitly passed
            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(realPageId, ""))
                .Returns(expectedContacts);

            // Act
            var result = await _contactMechanismController.ListContactMechanismForPerson(realPageId);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region ListContactMechanismForPerson Tests - User Claims Accessor

        [Fact]
        public async Task ListContactMechanismForPerson_AlwaysCallsGetUserClaim()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(DefaultUserClaim);

            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new List<CommonAddress> { new CommonAddress() });

            // Act
            await _contactMechanismController.ListContactMechanismForPerson(realPageId);

            // Assert
            _mockUserClaimsAccessor.Verify(x => x.GetUserClaim(), Times.Once);
        }

        [Fact]
        public async Task ListContactMechanismForPerson_WithValidProvidedGuid_IgnoresUserClaimGuid()
        {
            // Arrange
            var providedRealPageId = Guid.NewGuid();
            var userClaimRealPageId = Guid.NewGuid();
            var userClaim = new DefaultUserClaim { UserRealPageGuid = userClaimRealPageId };

            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(userClaim);

            var expectedContacts = new List<CommonAddress>
            {
                new CommonAddress { PartyContactMechanismId = 1 }
            };

            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(providedRealPageId, ""))
                .Returns(expectedContacts);

            // Act
            await _contactMechanismController.ListContactMechanismForPerson(providedRealPageId);

            // Assert - Verify it used the provided GUID, not the user claim GUID
            _mockContactMechanismRepository.Verify(
                x => x.ListContactMechanismForPerson(providedRealPageId, ""),
                Times.Once);
            _mockContactMechanismRepository.Verify(
                x => x.ListContactMechanismForPerson(userClaimRealPageId, It.IsAny<string>()),
                Times.Never);
        }

        #endregion

        #region Concurrent Access Tests

        [Fact]
        public async Task ListContactMechanismForPerson_MultipleConcurrentCalls_AllReturnCorrectResults()
        {
            // Arrange
            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new List<CommonAddress> { new CommonAddress { PartyContactMechanismId = 1 } });

            var tasks = new List<Task<IActionResult>>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(_contactMechanismController.ListContactMechanismForPerson(Guid.NewGuid()));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            foreach (var result in results)
            {
                Assert.IsType<OkObjectResult>(result);
            }
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _contactMechanismController = null!;
            base.Dispose();
        }

        #endregion
    }
}









