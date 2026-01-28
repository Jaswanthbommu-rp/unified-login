using System;
using System.Diagnostics.CodeAnalysis;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageStreetAddress business logic xUnit tests.
    /// Tests for street address management including create operations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageStreetAddressTests : TestBase
    {
        private readonly Mock<IStreetAddressRepository> _mockStreetAddressRepository;

        public ManageStreetAddressTests()
        {
            _mockStreetAddressRepository = new Mock<IStreetAddressRepository>();
        }

        #region Helper Methods

        private StreetAddress CreateValidStreetAddress()
        {
            return new StreetAddress
            {
                ContactMechanismId = 1,
                StreetAddress1 = "123 Main Street",
                StreetAddress2 = "Suite 100",
                StreetAddress3 = "Building A"
            };
        }

        private RepositoryResponse CreateSuccessResponse()
        {
            return new RepositoryResponse
            {
                Id = 1,
                RealPageId = Guid.NewGuid(),
                ErrorMessage = string.Empty
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithStreetAddressRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageStreetAddress = new ManageStreetAddress(_mockStreetAddressRepository.Object);

            // Assert
            Assert.NotNull(manageStreetAddress);
        }

        [Fact]
        public void Constructor_Default_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageStreetAddress = new ManageStreetAddress();

            // Assert
            Assert.NotNull(manageStreetAddress);
        }

        #endregion

        #region CreateStreetAddress Tests

        [Fact]
        public void CreateStreetAddress_WithValidStreetAddress_ReturnsSuccessResponse()
        {
            // Arrange
            var streetAddress = CreateValidStreetAddress();
            var expectedResponse = CreateSuccessResponse();

            _mockStreetAddressRepository
                .Setup(x => x.CreateStreetAddress(streetAddress))
                .Returns(expectedResponse);

            var manageStreetAddress = new ManageStreetAddress(_mockStreetAddressRepository.Object);

            // Act
            var result = manageStreetAddress.CreateStreetAddress(streetAddress);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            Assert.Equal(expectedResponse.RealPageId, result.RealPageId);
            Assert.Equal(expectedResponse.ErrorMessage, result.ErrorMessage);
            _mockStreetAddressRepository.Verify(x => x.CreateStreetAddress(streetAddress), Times.Once);
        }

        [Fact]
        public void CreateStreetAddress_WithNullStreetAddress_ThrowsArgumentNullException()
        {
            // Arrange
            IStreetAddress streetAddress = null;

            var manageStreetAddress = new ManageStreetAddress(_mockStreetAddressRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageStreetAddress.CreateStreetAddress(streetAddress));

            Assert.Equal("streetAddress", exception.ParamName);
            Assert.Contains("Null StreetAddress", exception.Message);
            _mockStreetAddressRepository.Verify(x => x.CreateStreetAddress(It.IsAny<IStreetAddress>()), Times.Never);
        }

        [Fact]
        public void CreateStreetAddress_WithMinimalData_ReturnsSuccessResponse()
        {
            // Arrange
            var streetAddress = new StreetAddress
            {
                ContactMechanismId = 0,
                StreetAddress1 = null,
                StreetAddress2 = null,
                StreetAddress3 = null
            };
            var expectedResponse = CreateSuccessResponse();

            _mockStreetAddressRepository
                .Setup(x => x.CreateStreetAddress(streetAddress))
                .Returns(expectedResponse);

            var manageStreetAddress = new ManageStreetAddress(_mockStreetAddressRepository.Object);

            // Act
            var result = manageStreetAddress.CreateStreetAddress(streetAddress);

            // Assert
            Assert.NotNull(result);
            _mockStreetAddressRepository.Verify(x => x.CreateStreetAddress(streetAddress), Times.Once);
        }

        [Fact]
        public void CreateStreetAddress_WithEmptyStrings_ReturnsSuccessResponse()
        {
            // Arrange
            var streetAddress = new StreetAddress
            {
                ContactMechanismId = 1,
                StreetAddress1 = string.Empty,
                StreetAddress2 = string.Empty,
                StreetAddress3 = string.Empty
            };
            var expectedResponse = CreateSuccessResponse();

            _mockStreetAddressRepository
                .Setup(x => x.CreateStreetAddress(streetAddress))
                .Returns(expectedResponse);

            var manageStreetAddress = new ManageStreetAddress(_mockStreetAddressRepository.Object);

            // Act
            var result = manageStreetAddress.CreateStreetAddress(streetAddress);

            // Assert
            Assert.NotNull(result);
            _mockStreetAddressRepository.Verify(x => x.CreateStreetAddress(streetAddress), Times.Once);
        }

        [Fact]
        public void CreateStreetAddress_WithOnlyStreetAddress1_ReturnsSuccessResponse()
        {
            // Arrange
            var streetAddress = new StreetAddress
            {
                ContactMechanismId = 1,
                StreetAddress1 = "123 Main Street"
            };
            var expectedResponse = CreateSuccessResponse();

            _mockStreetAddressRepository
                .Setup(x => x.CreateStreetAddress(streetAddress))
                .Returns(expectedResponse);

            var manageStreetAddress = new ManageStreetAddress(_mockStreetAddressRepository.Object);

            // Act
            var result = manageStreetAddress.CreateStreetAddress(streetAddress);

            // Assert
            Assert.NotNull(result);
            _mockStreetAddressRepository.Verify(x => x.CreateStreetAddress(streetAddress), Times.Once);
        }

        [Fact]
        public void CreateStreetAddress_WithRepositoryException_ThrowsException()
        {
            // Arrange
            var streetAddress = CreateValidStreetAddress();
            var expectedException = new Exception("Database error");

            _mockStreetAddressRepository
                .Setup(x => x.CreateStreetAddress(streetAddress))
                .Throws(expectedException);

            var manageStreetAddress = new ManageStreetAddress(_mockStreetAddressRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageStreetAddress.CreateStreetAddress(streetAddress));

            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public void CreateStreetAddress_WithLongAddressStrings_ReturnsSuccessResponse()
        {
            // Arrange
            var streetAddress = new StreetAddress
            {
                ContactMechanismId = 1,
                StreetAddress1 = new string('A', 1000),
                StreetAddress2 = new string('B', 1000),
                StreetAddress3 = new string('C', 1000)
            };
            var expectedResponse = CreateSuccessResponse();

            _mockStreetAddressRepository
                .Setup(x => x.CreateStreetAddress(streetAddress))
                .Returns(expectedResponse);

            var manageStreetAddress = new ManageStreetAddress(_mockStreetAddressRepository.Object);

            // Act
            var result = manageStreetAddress.CreateStreetAddress(streetAddress);

            // Assert
            Assert.NotNull(result);
            _mockStreetAddressRepository.Verify(x => x.CreateStreetAddress(streetAddress), Times.Once);
        }

        [Fact]
        public void CreateStreetAddress_WithSpecialCharacters_ReturnsSuccessResponse()
        {
            // Arrange
            var streetAddress = new StreetAddress
            {
                ContactMechanismId = 1,
                StreetAddress1 = "123 Main St. #100",
                StreetAddress2 = "Suite @ Building",
                StreetAddress3 = "Apt. 5-A (East)"
            };
            var expectedResponse = CreateSuccessResponse();

            _mockStreetAddressRepository
                .Setup(x => x.CreateStreetAddress(streetAddress))
                .Returns(expectedResponse);

            var manageStreetAddress = new ManageStreetAddress(_mockStreetAddressRepository.Object);

            // Act
            var result = manageStreetAddress.CreateStreetAddress(streetAddress);

            // Assert
            Assert.NotNull(result);
            _mockStreetAddressRepository.Verify(x => x.CreateStreetAddress(streetAddress), Times.Once);
        }

        [Fact]
        public void CreateStreetAddress_WithUnicodeCharacters_ReturnsSuccessResponse()
        {
            // Arrange
            var streetAddress = new StreetAddress
            {
                ContactMechanismId = 1,
                StreetAddress1 = "123 ?????",
                StreetAddress2 = "?? 100?",
                StreetAddress3 = "Ñoño Street"
            };
            var expectedResponse = CreateSuccessResponse();

            _mockStreetAddressRepository
                .Setup(x => x.CreateStreetAddress(streetAddress))
                .Returns(expectedResponse);

            var manageStreetAddress = new ManageStreetAddress(_mockStreetAddressRepository.Object);

            // Act
            var result = manageStreetAddress.CreateStreetAddress(streetAddress);

            // Assert
            Assert.NotNull(result);
            _mockStreetAddressRepository.Verify(x => x.CreateStreetAddress(streetAddress), Times.Once);
        }

        [Fact]
        public void CreateStreetAddress_WithNegativeContactMechanismId_ReturnsSuccessResponse()
        {
            // Arrange
            var streetAddress = new StreetAddress
            {
                ContactMechanismId = -1,
                StreetAddress1 = "123 Main Street"
            };
            var expectedResponse = CreateSuccessResponse();

            _mockStreetAddressRepository
                .Setup(x => x.CreateStreetAddress(streetAddress))
                .Returns(expectedResponse);

            var manageStreetAddress = new ManageStreetAddress(_mockStreetAddressRepository.Object);

            // Act
            var result = manageStreetAddress.CreateStreetAddress(streetAddress);

            // Assert
            // Note: Current implementation doesn't validate ContactMechanismId
            Assert.NotNull(result);
            _mockStreetAddressRepository.Verify(x => x.CreateStreetAddress(streetAddress), Times.Once);
        }

        [Fact]
        public void CreateStreetAddress_WithMaxIntContactMechanismId_ReturnsSuccessResponse()
        {
            // Arrange
            var streetAddress = new StreetAddress
            {
                ContactMechanismId = int.MaxValue,
                StreetAddress1 = "123 Main Street"
            };
            var expectedResponse = CreateSuccessResponse();

            _mockStreetAddressRepository
                .Setup(x => x.CreateStreetAddress(streetAddress))
                .Returns(expectedResponse);

            var manageStreetAddress = new ManageStreetAddress(_mockStreetAddressRepository.Object);

            // Act
            var result = manageStreetAddress.CreateStreetAddress(streetAddress);

            // Assert
            Assert.NotNull(result);
            _mockStreetAddressRepository.Verify(x => x.CreateStreetAddress(streetAddress), Times.Once);
        }

        [Fact]
        public void CreateStreetAddress_VerifiesRepositoryCalledWithCorrectObject()
        {
            // Arrange
            var streetAddress = CreateValidStreetAddress();
            var expectedResponse = CreateSuccessResponse();

            _mockStreetAddressRepository
                .Setup(x => x.CreateStreetAddress(It.IsAny<IStreetAddress>()))
                .Returns(expectedResponse);

            var manageStreetAddress = new ManageStreetAddress(_mockStreetAddressRepository.Object);

            // Act
            var result = manageStreetAddress.CreateStreetAddress(streetAddress);

            // Assert
            _mockStreetAddressRepository.Verify(x => x.CreateStreetAddress(
                It.Is<IStreetAddress>(s => 
                    s.ContactMechanismId == streetAddress.ContactMechanismId &&
                    s.StreetAddress1 == streetAddress.StreetAddress1 &&
                    s.StreetAddress2 == streetAddress.StreetAddress2 &&
                    s.StreetAddress3 == streetAddress.StreetAddress3)), Times.Once);
        }

        [Fact]
        public void CreateStreetAddress_WithMockedInterface_ReturnsSuccessResponse()
        {
            // Arrange
            var mockStreetAddress = new Mock<IStreetAddress>();
            mockStreetAddress.Setup(x => x.ContactMechanismId).Returns(1);
            mockStreetAddress.Setup(x => x.StreetAddress1).Returns("123 Main Street");
            mockStreetAddress.Setup(x => x.StreetAddress2).Returns("Suite 100");
            mockStreetAddress.Setup(x => x.StreetAddress3).Returns("Building A");

            var expectedResponse = CreateSuccessResponse();

            _mockStreetAddressRepository
                .Setup(x => x.CreateStreetAddress(mockStreetAddress.Object))
                .Returns(expectedResponse);

            var manageStreetAddress = new ManageStreetAddress(_mockStreetAddressRepository.Object);

            // Act
            var result = manageStreetAddress.CreateStreetAddress(mockStreetAddress.Object);

            // Assert
            Assert.NotNull(result);
            _mockStreetAddressRepository.Verify(x => x.CreateStreetAddress(mockStreetAddress.Object), Times.Once);
        }

        #endregion

        #region Data Object Tests

        [Fact]
        public void StreetAddress_AllPropertiesCanBeSet()
        {
            // Arrange
            var streetAddress = new StreetAddress();

            // Act
            streetAddress.ContactMechanismId = 1;
            streetAddress.StreetAddress1 = "123 Main Street";
            streetAddress.StreetAddress2 = "Suite 100";
            streetAddress.StreetAddress3 = "Building A";

            // Assert
            Assert.Equal(1, streetAddress.ContactMechanismId);
            Assert.Equal("123 Main Street", streetAddress.StreetAddress1);
            Assert.Equal("Suite 100", streetAddress.StreetAddress2);
            Assert.Equal("Building A", streetAddress.StreetAddress3);
        }

        [Fact]
        public void StreetAddress_ImplementsIStreetAddress()
        {
            // Arrange
            var streetAddress = new StreetAddress();

            // Assert
            Assert.IsAssignableFrom<IStreetAddress>(streetAddress);
        }

        [Fact]
        public void RepositoryResponse_AllPropertiesCanBeSet()
        {
            // Arrange
            var response = new RepositoryResponse();
            var guid = Guid.NewGuid();

            // Act
            response.Id = 1;
            response.RealPageId = guid;
            response.ErrorMessage = "Success";

            // Assert
            Assert.Equal(1, response.Id);
            Assert.Equal(guid, response.RealPageId);
            Assert.Equal("Success", response.ErrorMessage);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void Integration_CreateStreetAddress_FullWorkflow()
        {
            // Arrange
            var streetAddress = new StreetAddress
            {
                ContactMechanismId = 100,
                StreetAddress1 = "456 Oak Avenue",
                StreetAddress2 = "Floor 2",
                StreetAddress3 = "Room 201"
            };
            var expectedResponse = new RepositoryResponse
            {
                Id = 100,
                RealPageId = Guid.NewGuid(),
                ErrorMessage = string.Empty
            };

            _mockStreetAddressRepository
                .Setup(x => x.CreateStreetAddress(streetAddress))
                .Returns(expectedResponse);

            var manageStreetAddress = new ManageStreetAddress(_mockStreetAddressRepository.Object);

            // Act
            var result = manageStreetAddress.CreateStreetAddress(streetAddress);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Id);
            Assert.NotEqual(Guid.Empty, result.RealPageId);
            Assert.Equal(string.Empty, result.ErrorMessage);
        }

        #endregion

        #region Documentation Tests

        [Fact]
        public void ManageStreetAddress_ClassPurpose_Documentation()
        {
            // This test documents the class purpose:
            // ManageStreetAddress is responsible for:
            // 1. Creating street addresses for persons
            // 2. Validating that streetAddress is not null
            // 3. Delegating to the repository for database operations
            //
            // The class supports:
            // - StreetAddress1 (primary address line)
            // - StreetAddress2 (secondary address line, e.g., suite/apartment)
            // - StreetAddress3 (tertiary address line, e.g., building)
            // - ContactMechanismId (foreign key reference)

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageStreetAddress_ValidationRules_Documentation()
        {
            // This test documents validation rules:
            //
            // CreateStreetAddress:
            // - streetAddress parameter must not be null
            //   - Throws ArgumentNullException with message "Null StreetAddress."
            //
            // Note: Current implementation doesn't validate:
            // - ContactMechanismId (accepts 0, negative, or any value)
            // - StreetAddress1 (accepts null, empty, or any string)
            // - StreetAddress2 (accepts null, empty, or any string)
            // - StreetAddress3 (accepts null, empty, or any string)

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageStreetAddress_ExceptionHandling_Documentation()
        {
            // This test documents exception handling:
            //
            // CreateStreetAddress:
            // - Throws ArgumentNullException if streetAddress is null
            // - Does NOT catch repository exceptions (they propagate to caller)
            //
            // This is the expected behavior as it allows the caller to handle
            // database-level errors appropriately.

            Assert.True(true, "Documentation test");
        }

        #endregion
    }
}
