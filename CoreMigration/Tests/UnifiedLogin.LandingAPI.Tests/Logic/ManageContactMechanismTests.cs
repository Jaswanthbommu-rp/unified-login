using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageContactMechanism business logic xUnit tests.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageContactMechanismTests : TestBase
    {
        private readonly Mock<IContactMechanismRepository> _mockContactMechanismRepository;
        private readonly Mock<IRepository> _mockRepository;

        public ManageContactMechanismTests()
        {
            _mockContactMechanismRepository = new Mock<IContactMechanismRepository>();
            _mockRepository = new Mock<IRepository>();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithContactMechanismRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Assert
            Assert.NotNull(manageContactMechanism);
        }

        [Fact]
        public void Constructor_WithRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageContactMechanism = new ManageContactMechanism(_mockRepository.Object);

            // Assert
            Assert.NotNull(manageContactMechanism);
        }

        [Fact]
        public void Constructor_WithoutParameters_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageContactMechanism = new ManageContactMechanism();

            // Assert
            Assert.NotNull(manageContactMechanism);
        }

        #endregion

        #region CreateContactMechanism Tests

        [Fact]
        public void CreateContactMechanism_ReturnsSuccessResponse()
        {
            // Arrange
            var expectedResponse = new RepositoryResponse
            {
                Id = 100,
                ErrorMessage = ""
            };

            _mockContactMechanismRepository
                .Setup(x => x.CreateContactMechanism())
                .Returns(expectedResponse);

            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act
            var result = manageContactMechanism.CreateContactMechanism();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Id);
            Assert.Equal("", result.ErrorMessage);
            _mockContactMechanismRepository.Verify(x => x.CreateContactMechanism(), Times.Once);
        }

        [Fact]
        public void CreateContactMechanism_WithRepositoryError_ReturnsErrorResponse()
        {
            // Arrange
            var expectedResponse = new RepositoryResponse
            {
                Id = 0,
                ErrorMessage = "Failed to create contact mechanism"
            };

            _mockContactMechanismRepository
                .Setup(x => x.CreateContactMechanism())
                .Returns(expectedResponse);

            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act
            var result = manageContactMechanism.CreateContactMechanism();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Id);
            Assert.Equal("Failed to create contact mechanism", result.ErrorMessage);
        }

        #endregion

        #region ListContactMechanismForPerson Tests

        [Fact]
        public void ListContactMechanismForPerson_WithValidRealPageId_ReturnsAddressList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string usageTypeName = "Personal";
            var expectedAddresses = new List<CommonAddress>
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
                    AddressString = "123 Main St",
                    AddressType = "Postal"
                }
            };

            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(realPageId, usageTypeName))
                .Returns(expectedAddresses);

            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act
            var result = manageContactMechanism.ListContactMechanismForPerson(realPageId, usageTypeName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("test@example.com", result[0].AddressString);
            Assert.Equal("Email", result[0].AddressType);
            _mockContactMechanismRepository.Verify(
                x => x.ListContactMechanismForPerson(realPageId, usageTypeName),
                Times.Once);
        }

        [Fact]
        public void ListContactMechanismForPerson_WithEmptyGuid_ThrowsException()
        {
            // Arrange
            var realPageId = Guid.Empty;
            string usageTypeName = "Personal";
            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageContactMechanism.ListContactMechanismForPerson(realPageId, usageTypeName));

            Assert.Equal("Invalid parameter realPageId.", exception.Message);
            _mockContactMechanismRepository.Verify(
                x => x.ListContactMechanismForPerson(It.IsAny<Guid>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void ListContactMechanismForPerson_WithNullUsageTypeName_ReturnsAllAddresses()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string usageTypeName = null;
            var expectedAddresses = new List<CommonAddress>
            {
                new CommonAddress
                {
                    PartyContactMechanismId = 1,
                    ContactMechanismId = 100,
                    AddressString = "work@example.com",
                    AddressType = "Email"
                },
                new CommonAddress
                {
                    PartyContactMechanismId = 2,
                    ContactMechanismId = 101,
                    AddressString = "personal@example.com",
                    AddressType = "Email"
                }
            };

            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(realPageId, usageTypeName))
                .Returns(expectedAddresses);

            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act
            var result = manageContactMechanism.ListContactMechanismForPerson(realPageId, usageTypeName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void ListContactMechanismForPerson_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string usageTypeName = "NonExistent";
            var expectedAddresses = new List<CommonAddress>();

            _mockContactMechanismRepository
                .Setup(x => x.ListContactMechanismForPerson(realPageId, usageTypeName))
                .Returns(expectedAddresses);

            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act
            var result = manageContactMechanism.ListContactMechanismForPerson(realPageId, usageTypeName);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region LinkContactMechanismToParty Tests

        [Fact]
        public void LinkContactMechanismToParty_WithValidParameters_ReturnsSuccessResponse()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var partyContactMechanism = new PartyContactMechanism
            {
                PartyId = 12345,
                ContactMechanismId = 100,
                FromDate = DateTime.Now
            };

            var expectedResponse = new RepositoryResponse
            {
                Id = 500,
                ErrorMessage = ""
            };

            _mockContactMechanismRepository
                .Setup(x => x.LinkContactMechanismToParty(realPageId, partyContactMechanism))
                .Returns(expectedResponse);

            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act
            var result = manageContactMechanism.LinkContactMechanismToParty(realPageId, partyContactMechanism);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500, result.Id);
            Assert.Equal("", result.ErrorMessage);
            _mockContactMechanismRepository.Verify(
                x => x.LinkContactMechanismToParty(realPageId, partyContactMechanism),
                Times.Once);
        }

        [Fact]
        public void LinkContactMechanismToParty_WithEmptyGuid_ThrowsException()
        {
            // Arrange
            var realPageId = Guid.Empty;
            var partyContactMechanism = new PartyContactMechanism
            {
                PartyId = 12345,
                ContactMechanismId = 100
            };

            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageContactMechanism.LinkContactMechanismToParty(realPageId, partyContactMechanism));

            Assert.Equal("Invalid parameter realPageId.", exception.Message);
            _mockContactMechanismRepository.Verify(
                x => x.LinkContactMechanismToParty(It.IsAny<Guid>(), It.IsAny<IPartyContactMechanism>()),
                Times.Never);
        }

        [Fact]
        public void LinkContactMechanismToParty_WithNullPartyContactMechanism_ThrowsArgumentNullException()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            IPartyContactMechanism partyContactMechanism = null;

            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageContactMechanism.LinkContactMechanismToParty(realPageId, partyContactMechanism));

            Assert.Equal("partyContactMechanism", exception.ParamName);
            Assert.Contains("Null PartyContactMechanism.", exception.Message);
            _mockContactMechanismRepository.Verify(
                x => x.LinkContactMechanismToParty(It.IsAny<Guid>(), It.IsAny<IPartyContactMechanism>()),
                Times.Never);
        }

        [Fact]
        public void LinkContactMechanismToParty_WithRepositoryError_ReturnsErrorResponse()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var partyContactMechanism = new PartyContactMechanism
            {
                PartyId = 12345,
                ContactMechanismId = 100
            };

            var expectedResponse = new RepositoryResponse
            {
                Id = 0,
                ErrorMessage = "Failed to link contact mechanism"
            };

            _mockContactMechanismRepository
                .Setup(x => x.LinkContactMechanismToParty(realPageId, partyContactMechanism))
                .Returns(expectedResponse);

            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act
            var result = manageContactMechanism.LinkContactMechanismToParty(realPageId, partyContactMechanism);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Id);
            Assert.Equal("Failed to link contact mechanism", result.ErrorMessage);
        }

        #endregion

        #region LinkGeographicBoundaryToContactMechanism Tests

        [Fact]
        public void LinkGeographicBoundaryToContactMechanism_WithValidBoundary_ReturnsSuccessResponse()
        {
            // Arrange
            var contactMechanismBoundary = new ContactMechanismBoundary
            {
                ContactMechanismId = 100,
                GeographicBoundaryId = 50,
                FromDate = DateTime.Now,
                ThruDate = DateTime.Now.AddYears(1)
            };

            var expectedResponse = new RepositoryResponse
            {
                Id = 300,
                ErrorMessage = ""
            };

            _mockContactMechanismRepository
                .Setup(x => x.LinkGeographicBoundaryToContactMechanism(contactMechanismBoundary))
                .Returns(expectedResponse);

            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act
            var result = manageContactMechanism.LinkGeographicBoundaryToContactMechanism(contactMechanismBoundary);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(300, result.Id);
            Assert.Equal("", result.ErrorMessage);
            _mockContactMechanismRepository.Verify(
                x => x.LinkGeographicBoundaryToContactMechanism(contactMechanismBoundary),
                Times.Once);
        }

        [Fact]
        public void LinkGeographicBoundaryToContactMechanism_WithNullBoundary_ThrowsArgumentNullException()
        {
            // Arrange
            IContactMechanismBoundary contactMechanismBoundary = null;
            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageContactMechanism.LinkGeographicBoundaryToContactMechanism(contactMechanismBoundary));

            Assert.Equal("contactMechanismBoundary", exception.ParamName);
            Assert.Contains("Null ContactMechanismBoundary.", exception.Message);
            _mockContactMechanismRepository.Verify(
                x => x.LinkGeographicBoundaryToContactMechanism(It.IsAny<IContactMechanismBoundary>()),
                Times.Never);
        }

        [Fact]
        public void LinkGeographicBoundaryToContactMechanism_WithRepositoryError_ReturnsErrorResponse()
        {
            // Arrange
            var contactMechanismBoundary = new ContactMechanismBoundary
            {
                ContactMechanismId = 100,
                GeographicBoundaryId = 50,
                FromDate = DateTime.Now,
                ThruDate = DateTime.Now.AddYears(1)
            };

            var expectedResponse = new RepositoryResponse
            {
                Id = 0,
                ErrorMessage = "Failed to link geographic boundary"
            };

            _mockContactMechanismRepository
                .Setup(x => x.LinkGeographicBoundaryToContactMechanism(contactMechanismBoundary))
                .Returns(expectedResponse);

            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act
            var result = manageContactMechanism.LinkGeographicBoundaryToContactMechanism(contactMechanismBoundary);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Id);
            Assert.Equal("Failed to link geographic boundary", result.ErrorMessage);
        }

        #endregion

        #region LinkUsageTypeToPartyContactMechanism Tests

        [Fact]
        public void LinkUsageTypeToPartyContactMechanism_WithValidParameters_ReturnsSuccessResponse()
        {
            // Arrange
            long partyContactMechanismId = 100;
            int? contactMechanismUsageTypeId = 1;

            var expectedResponse = new RepositoryResponse
            {
                Id = 200,
                ErrorMessage = ""
            };

            _mockContactMechanismRepository
                .Setup(x => x.LinkUsageTypeToPartyContactMechanism(partyContactMechanismId, contactMechanismUsageTypeId))
                .Returns(expectedResponse);

            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act
            var result = manageContactMechanism.LinkUsageTypeToPartyContactMechanism(
                partyContactMechanismId, 
                contactMechanismUsageTypeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Id);
            Assert.Equal("", result.ErrorMessage);
            _mockContactMechanismRepository.Verify(
                x => x.LinkUsageTypeToPartyContactMechanism(partyContactMechanismId, contactMechanismUsageTypeId),
                Times.Once);
        }

        [Fact]
        public void LinkUsageTypeToPartyContactMechanism_WithZeroPartyContactMechanismId_ThrowsException()
        {
            // Arrange
            long partyContactMechanismId = 0;
            int? contactMechanismUsageTypeId = 1;
            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageContactMechanism.LinkUsageTypeToPartyContactMechanism(
                    partyContactMechanismId, 
                    contactMechanismUsageTypeId));

            Assert.Equal("Missing Party Contact Mechanism Id.", exception.Message);
            _mockContactMechanismRepository.Verify(
                x => x.LinkUsageTypeToPartyContactMechanism(It.IsAny<long>(), It.IsAny<int?>()),
                Times.Never);
        }

        [Fact]
        public void LinkUsageTypeToPartyContactMechanism_WithNegativePartyContactMechanismId_ThrowsException()
        {
            // Arrange
            long partyContactMechanismId = -1;
            int? contactMechanismUsageTypeId = 1;
            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageContactMechanism.LinkUsageTypeToPartyContactMechanism(
                    partyContactMechanismId, 
                    contactMechanismUsageTypeId));

            Assert.Equal("Missing Party Contact Mechanism Id.", exception.Message);
        }

        [Fact]
        public void LinkUsageTypeToPartyContactMechanism_WithNullUsageTypeId_ReturnsSuccessResponse()
        {
            // Arrange
            long partyContactMechanismId = 100;
            int? contactMechanismUsageTypeId = null;

            var expectedResponse = new RepositoryResponse
            {
                Id = 200,
                ErrorMessage = ""
            };

            _mockContactMechanismRepository
                .Setup(x => x.LinkUsageTypeToPartyContactMechanism(partyContactMechanismId, contactMechanismUsageTypeId))
                .Returns(expectedResponse);

            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act
            var result = manageContactMechanism.LinkUsageTypeToPartyContactMechanism(
                partyContactMechanismId, 
                contactMechanismUsageTypeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Id);
        }

        #endregion

        #region UpdateContactMechanismUsageForParty Tests

        [Fact]
        public void UpdateContactMechanismUsageForParty_WithValidParameters_ReturnsSuccessResponse()
        {
            // Arrange
            long partyContactMechanismId = 150;
            int? contactMechanismUsageTypeId = 2;

            var expectedResponse = new RepositoryResponse
            {
                Id = 150,
                ErrorMessage = ""
            };

            _mockContactMechanismRepository
                .Setup(x => x.UpdateContactMechanismUsageForParty(partyContactMechanismId, contactMechanismUsageTypeId))
                .Returns(expectedResponse);

            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act
            var result = manageContactMechanism.UpdateContactMechanismUsageForParty(
                partyContactMechanismId, 
                contactMechanismUsageTypeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(150, result.Id);
            Assert.Equal("", result.ErrorMessage);
            _mockContactMechanismRepository.Verify(
                x => x.UpdateContactMechanismUsageForParty(partyContactMechanismId, contactMechanismUsageTypeId),
                Times.Once);
        }

        [Fact]
        public void UpdateContactMechanismUsageForParty_WithZeroPartyContactMechanismId_ThrowsException()
        {
            // Arrange
            long partyContactMechanismId = 0;
            int? contactMechanismUsageTypeId = 2;
            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageContactMechanism.UpdateContactMechanismUsageForParty(
                    partyContactMechanismId, 
                    contactMechanismUsageTypeId));

            Assert.Equal("Missing Party Contact Mechanism Id.", exception.Message);
            _mockContactMechanismRepository.Verify(
                x => x.UpdateContactMechanismUsageForParty(It.IsAny<long>(), It.IsAny<int?>()),
                Times.Never);
        }

        [Fact]
        public void UpdateContactMechanismUsageForParty_WithNegativePartyContactMechanismId_ThrowsException()
        {
            // Arrange
            long partyContactMechanismId = -10;
            int? contactMechanismUsageTypeId = 2;
            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageContactMechanism.UpdateContactMechanismUsageForParty(
                    partyContactMechanismId, 
                    contactMechanismUsageTypeId));

            Assert.Equal("Missing Party Contact Mechanism Id.", exception.Message);
        }

        [Fact]
        public void UpdateContactMechanismUsageForParty_WithNullUsageTypeId_ReturnsSuccessResponse()
        {
            // Arrange
            long partyContactMechanismId = 150;
            int? contactMechanismUsageTypeId = null;

            var expectedResponse = new RepositoryResponse
            {
                Id = 150,
                ErrorMessage = ""
            };

            _mockContactMechanismRepository
                .Setup(x => x.UpdateContactMechanismUsageForParty(partyContactMechanismId, contactMechanismUsageTypeId))
                .Returns(expectedResponse);

            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act
            var result = manageContactMechanism.UpdateContactMechanismUsageForParty(
                partyContactMechanismId, 
                contactMechanismUsageTypeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(150, result.Id);
        }

        [Fact]
        public void UpdateContactMechanismUsageForParty_WithRepositoryError_ReturnsErrorResponse()
        {
            // Arrange
            long partyContactMechanismId = 150;
            int? contactMechanismUsageTypeId = 2;

            var expectedResponse = new RepositoryResponse
            {
                Id = 0,
                ErrorMessage = "Failed to update contact mechanism usage"
            };

            _mockContactMechanismRepository
                .Setup(x => x.UpdateContactMechanismUsageForParty(partyContactMechanismId, contactMechanismUsageTypeId))
                .Returns(expectedResponse);

            var manageContactMechanism = new ManageContactMechanism(_mockContactMechanismRepository.Object);

            // Act
            var result = manageContactMechanism.UpdateContactMechanismUsageForParty(
                partyContactMechanismId, 
                contactMechanismUsageTypeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Id);
            Assert.Equal("Failed to update contact mechanism usage", result.ErrorMessage);
        }

        #endregion
    }
}
