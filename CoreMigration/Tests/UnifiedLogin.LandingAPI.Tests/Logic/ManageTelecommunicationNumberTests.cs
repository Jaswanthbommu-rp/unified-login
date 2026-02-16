using System;
using System.Collections.Generic;
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
    /// ManageTelecommunicationNumber business logic xUnit tests.
    /// Tests for telecommunication number management including create and list operations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageTelecommunicationNumberTests : TestBase
    {
        private readonly Mock<ITelecommunicationNumberRepository> _mockTelecommunicationNumberRepository;

        public ManageTelecommunicationNumberTests()
        {
            _mockTelecommunicationNumberRepository = new Mock<ITelecommunicationNumberRepository>();
        }

        #region Helper Methods

        private TelecommunicationNumber CreateValidTelecommunicationNumber()
        {
            return new TelecommunicationNumber
            {
                PartyContactMechanismId = 1,
                ContactMechanismId = 100,
                CountryCode = "1",
                AreaCode = "214",
                PhoneNumber = "5551234",
                IsDeleted = false,
                IsPreferred = true,
                ContactMechanismUsageTypeId = 1,
                ISOCode = "US",
                IsDefault = true
            };
        }

        private List<TelecommunicationNumber> CreateTelecommunicationNumberList()
        {
            return new List<TelecommunicationNumber>
            {
                new TelecommunicationNumber
                {
                    PartyContactMechanismId = 1,
                    ContactMechanismId = 100,
                    CountryCode = "1",
                    AreaCode = "214",
                    PhoneNumber = "5551234",
                    IsPreferred = true
                },
                new TelecommunicationNumber
                {
                    PartyContactMechanismId = 2,
                    ContactMechanismId = 101,
                    CountryCode = "1",
                    AreaCode = "972",
                    PhoneNumber = "5555678",
                    IsPreferred = false
                }
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
        public void Constructor_WithTelecommunicationNumberRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageTelecommunicationNumber = new ManageTelecommunicationNumber(_mockTelecommunicationNumberRepository.Object);

            // Assert
            Assert.NotNull(manageTelecommunicationNumber);
        }

        [Fact]
        public void Constructor_Default_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageTelecommunicationNumber = new ManageTelecommunicationNumber();

            // Assert
            Assert.NotNull(manageTelecommunicationNumber);
        }

        #endregion

        #region CreateTelecommunicationNumber Tests

        [Fact]
        public void CreateTelecommunicationNumber_WithValidTelecommunicationNumber_ReturnsSuccessResponse()
        {
            // Arrange
            var telecommunicationNumber = CreateValidTelecommunicationNumber();
            var expectedResponse = CreateSuccessResponse();

            _mockTelecommunicationNumberRepository
                .Setup(x => x.CreateTelecommunicationNumber(telecommunicationNumber))
                .Returns(expectedResponse);

            var manageTelecommunicationNumber = new ManageTelecommunicationNumber(_mockTelecommunicationNumberRepository.Object);

            // Act
            var result = manageTelecommunicationNumber.CreateTelecommunicationNumber(telecommunicationNumber);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            Assert.Equal(expectedResponse.RealPageId, result.RealPageId);
            Assert.Equal(expectedResponse.ErrorMessage, result.ErrorMessage);
            _mockTelecommunicationNumberRepository.Verify(x => x.CreateTelecommunicationNumber(telecommunicationNumber), Times.Once);
        }

        [Fact]
        public void CreateTelecommunicationNumber_WithNullTelecommunicationNumber_ThrowsArgumentNullException()
        {
            // Arrange
            ITelecommunicationNumber telecommunicationNumber = null;

            var manageTelecommunicationNumber = new ManageTelecommunicationNumber(_mockTelecommunicationNumberRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageTelecommunicationNumber.CreateTelecommunicationNumber(telecommunicationNumber));

            Assert.Equal("telecommunicationNumber", exception.ParamName);
            Assert.Contains("Null TelecommunicationNumber", exception.Message);
            _mockTelecommunicationNumberRepository.Verify(x => x.CreateTelecommunicationNumber(It.IsAny<ITelecommunicationNumber>()), Times.Never);
        }

        [Fact]
        public void CreateTelecommunicationNumber_WithMinimalData_ReturnsSuccessResponse()
        {
            // Arrange
            var telecommunicationNumber = new TelecommunicationNumber();
            var expectedResponse = CreateSuccessResponse();

            _mockTelecommunicationNumberRepository
                .Setup(x => x.CreateTelecommunicationNumber(telecommunicationNumber))
                .Returns(expectedResponse);

            var manageTelecommunicationNumber = new ManageTelecommunicationNumber(_mockTelecommunicationNumberRepository.Object);

            // Act
            var result = manageTelecommunicationNumber.CreateTelecommunicationNumber(telecommunicationNumber);

            // Assert
            Assert.NotNull(result);
            _mockTelecommunicationNumberRepository.Verify(x => x.CreateTelecommunicationNumber(telecommunicationNumber), Times.Once);
        }

        [Fact]
        public void CreateTelecommunicationNumber_WithRepositoryException_ThrowsException()
        {
            // Arrange
            var telecommunicationNumber = CreateValidTelecommunicationNumber();
            var expectedException = new Exception("Database error");

            _mockTelecommunicationNumberRepository
                .Setup(x => x.CreateTelecommunicationNumber(telecommunicationNumber))
                .Throws(expectedException);

            var manageTelecommunicationNumber = new ManageTelecommunicationNumber(_mockTelecommunicationNumberRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageTelecommunicationNumber.CreateTelecommunicationNumber(telecommunicationNumber));

            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public void CreateTelecommunicationNumber_WithMockedInterface_ReturnsSuccessResponse()
        {
            // Arrange
            var mockTelecommunicationNumber = new Mock<ITelecommunicationNumber>();
            mockTelecommunicationNumber.Setup(x => x.CountryCode).Returns("1");
            mockTelecommunicationNumber.Setup(x => x.AreaCode).Returns("214");
            mockTelecommunicationNumber.Setup(x => x.PhoneNumber).Returns("5551234");

            var expectedResponse = CreateSuccessResponse();

            _mockTelecommunicationNumberRepository
                .Setup(x => x.CreateTelecommunicationNumber(mockTelecommunicationNumber.Object))
                .Returns(expectedResponse);

            var manageTelecommunicationNumber = new ManageTelecommunicationNumber(_mockTelecommunicationNumberRepository.Object);

            // Act
            var result = manageTelecommunicationNumber.CreateTelecommunicationNumber(mockTelecommunicationNumber.Object);

            // Assert
            Assert.NotNull(result);
            _mockTelecommunicationNumberRepository.Verify(x => x.CreateTelecommunicationNumber(mockTelecommunicationNumber.Object), Times.Once);
        }

        [Fact]
        public void CreateTelecommunicationNumber_VerifiesRepositoryCalledWithCorrectObject()
        {
            // Arrange
            var telecommunicationNumber = CreateValidTelecommunicationNumber();
            var expectedResponse = CreateSuccessResponse();

            _mockTelecommunicationNumberRepository
                .Setup(x => x.CreateTelecommunicationNumber(It.IsAny<ITelecommunicationNumber>()))
                .Returns(expectedResponse);

            var manageTelecommunicationNumber = new ManageTelecommunicationNumber(_mockTelecommunicationNumberRepository.Object);

            // Act
            var result = manageTelecommunicationNumber.CreateTelecommunicationNumber(telecommunicationNumber);

            // Assert
            _mockTelecommunicationNumberRepository.Verify(x => x.CreateTelecommunicationNumber(
                It.Is<ITelecommunicationNumber>(t =>
                    t.CountryCode == telecommunicationNumber.CountryCode &&
                    t.AreaCode == telecommunicationNumber.AreaCode &&
                    t.PhoneNumber == telecommunicationNumber.PhoneNumber)), Times.Once);
        }

        #endregion

        #region ListTelecommunicationNumberForPerson Tests

        [Fact]
        public void ListTelecommunicationNumberForPerson_WithValidParameters_ReturnsList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string contactMechanismUsageTypeName = "Mobile";
            var expectedList = CreateTelecommunicationNumberList();

            _mockTelecommunicationNumberRepository
                .Setup(x => x.ListTelecommunicationNumberForPerson(realPageId, contactMechanismUsageTypeName))
                .Returns(expectedList);

            var manageTelecommunicationNumber = new ManageTelecommunicationNumber(_mockTelecommunicationNumberRepository.Object);

            // Act
            var result = manageTelecommunicationNumber.ListTelecommunicationNumberForPerson(realPageId, contactMechanismUsageTypeName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _mockTelecommunicationNumberRepository.Verify(x => x.ListTelecommunicationNumberForPerson(realPageId, contactMechanismUsageTypeName), Times.Once);
        }

        [Fact]
        public void ListTelecommunicationNumberForPerson_WithEmptyGuid_ThrowsException()
        {
            // Arrange
            var realPageId = Guid.Empty;
            string contactMechanismUsageTypeName = "Mobile";

            var manageTelecommunicationNumber = new ManageTelecommunicationNumber(_mockTelecommunicationNumberRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageTelecommunicationNumber.ListTelecommunicationNumberForPerson(realPageId, contactMechanismUsageTypeName));

            Assert.Equal("Invalid parameter realPageId.", exception.Message);
            _mockTelecommunicationNumberRepository.Verify(x => x.ListTelecommunicationNumberForPerson(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ListTelecommunicationNumberForPerson_WithNullContactMechanismUsageTypeName_CallsRepository()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string contactMechanismUsageTypeName = null;
            var expectedList = CreateTelecommunicationNumberList();

            _mockTelecommunicationNumberRepository
                .Setup(x => x.ListTelecommunicationNumberForPerson(realPageId, contactMechanismUsageTypeName))
                .Returns(expectedList);

            var manageTelecommunicationNumber = new ManageTelecommunicationNumber(_mockTelecommunicationNumberRepository.Object);

            // Act
            var result = manageTelecommunicationNumber.ListTelecommunicationNumberForPerson(realPageId, contactMechanismUsageTypeName);

            // Assert
            // Note: Current implementation doesn't validate ContactMechanismUsageTypeName
            Assert.NotNull(result);
            _mockTelecommunicationNumberRepository.Verify(x => x.ListTelecommunicationNumberForPerson(realPageId, contactMechanismUsageTypeName), Times.Once);
        }

        [Fact]
        public void ListTelecommunicationNumberForPerson_WithEmptyContactMechanismUsageTypeName_CallsRepository()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string contactMechanismUsageTypeName = string.Empty;
            var expectedList = CreateTelecommunicationNumberList();

            _mockTelecommunicationNumberRepository
                .Setup(x => x.ListTelecommunicationNumberForPerson(realPageId, contactMechanismUsageTypeName))
                .Returns(expectedList);

            var manageTelecommunicationNumber = new ManageTelecommunicationNumber(_mockTelecommunicationNumberRepository.Object);

            // Act
            var result = manageTelecommunicationNumber.ListTelecommunicationNumberForPerson(realPageId, contactMechanismUsageTypeName);

            // Assert
            Assert.NotNull(result);
            _mockTelecommunicationNumberRepository.Verify(x => x.ListTelecommunicationNumberForPerson(realPageId, contactMechanismUsageTypeName), Times.Once);
        }

        [Fact]
        public void ListTelecommunicationNumberForPerson_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string contactMechanismUsageTypeName = "Mobile";

            _mockTelecommunicationNumberRepository
                .Setup(x => x.ListTelecommunicationNumberForPerson(realPageId, contactMechanismUsageTypeName))
                .Returns(new List<TelecommunicationNumber>());

            var manageTelecommunicationNumber = new ManageTelecommunicationNumber(_mockTelecommunicationNumberRepository.Object);

            // Act
            var result = manageTelecommunicationNumber.ListTelecommunicationNumberForPerson(realPageId, contactMechanismUsageTypeName);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ListTelecommunicationNumberForPerson_WithNullResponse_ReturnsNull()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string contactMechanismUsageTypeName = "Mobile";

            _mockTelecommunicationNumberRepository
                .Setup(x => x.ListTelecommunicationNumberForPerson(realPageId, contactMechanismUsageTypeName))
                .Returns((IList<TelecommunicationNumber>)null);

            var manageTelecommunicationNumber = new ManageTelecommunicationNumber(_mockTelecommunicationNumberRepository.Object);

            // Act
            var result = manageTelecommunicationNumber.ListTelecommunicationNumberForPerson(realPageId, contactMechanismUsageTypeName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ListTelecommunicationNumberForPerson_WithRepositoryException_ThrowsException()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string contactMechanismUsageTypeName = "Mobile";
            var expectedException = new Exception("Database error");

            _mockTelecommunicationNumberRepository
                .Setup(x => x.ListTelecommunicationNumberForPerson(realPageId, contactMechanismUsageTypeName))
                .Throws(expectedException);

            var manageTelecommunicationNumber = new ManageTelecommunicationNumber(_mockTelecommunicationNumberRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageTelecommunicationNumber.ListTelecommunicationNumberForPerson(realPageId, contactMechanismUsageTypeName));

            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public void ListTelecommunicationNumberForPerson_VerifiesRepositoryCalledWithCorrectParameters()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string contactMechanismUsageTypeName = "Work";
            var expectedList = CreateTelecommunicationNumberList();

            _mockTelecommunicationNumberRepository
                .Setup(x => x.ListTelecommunicationNumberForPerson(realPageId, contactMechanismUsageTypeName))
                .Returns(expectedList);

            var manageTelecommunicationNumber = new ManageTelecommunicationNumber(_mockTelecommunicationNumberRepository.Object);

            // Act
            var result = manageTelecommunicationNumber.ListTelecommunicationNumberForPerson(realPageId, contactMechanismUsageTypeName);

            // Assert
            _mockTelecommunicationNumberRepository.Verify(x => x.ListTelecommunicationNumberForPerson(
                It.Is<Guid>(g => g == realPageId),
                It.Is<string>(s => s == "Work")), Times.Once);
        }

        [Fact]
        public void ListTelecommunicationNumberForPerson_WithDifferentUsageTypes_CallsRepository()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var expectedList = CreateTelecommunicationNumberList();

            _mockTelecommunicationNumberRepository
                .Setup(x => x.ListTelecommunicationNumberForPerson(realPageId, It.IsAny<string>()))
                .Returns(expectedList);

            var manageTelecommunicationNumber = new ManageTelecommunicationNumber(_mockTelecommunicationNumberRepository.Object);

            // Act - Test different usage types
            var mobileResult = manageTelecommunicationNumber.ListTelecommunicationNumberForPerson(realPageId, "Mobile");
            var workResult = manageTelecommunicationNumber.ListTelecommunicationNumberForPerson(realPageId, "Work");
            var homeResult = manageTelecommunicationNumber.ListTelecommunicationNumberForPerson(realPageId, "Home");

            // Assert
            Assert.NotNull(mobileResult);
            Assert.NotNull(workResult);
            Assert.NotNull(homeResult);
            _mockTelecommunicationNumberRepository.Verify(x => x.ListTelecommunicationNumberForPerson(realPageId, "Mobile"), Times.Once);
            _mockTelecommunicationNumberRepository.Verify(x => x.ListTelecommunicationNumberForPerson(realPageId, "Work"), Times.Once);
            _mockTelecommunicationNumberRepository.Verify(x => x.ListTelecommunicationNumberForPerson(realPageId, "Home"), Times.Once);
        }

        #endregion

        #region Data Object Tests

        [Fact]
        public void TelecommunicationNumber_AllPropertiesCanBeSet()
        {
            // Arrange
            var telecommunicationNumber = new TelecommunicationNumber();

            // Act
            telecommunicationNumber.PartyContactMechanismId = 1;
            telecommunicationNumber.ContactMechanismId = 100;
            telecommunicationNumber.CountryCode = "1";
            telecommunicationNumber.AreaCode = "214";
            telecommunicationNumber.PhoneNumber = "5551234";
            telecommunicationNumber.IsDeleted = false;
            telecommunicationNumber.IsPreferred = true;
            telecommunicationNumber.ContactMechanismUsageTypeId = 1;
            telecommunicationNumber.ISOCode = "US";
            telecommunicationNumber.IsDefault = true;
            telecommunicationNumber.contactMechanismUsageType = new ContactMechanismUsageType { ContactMechanismUsageTypeId = 1 };

            // Assert
            Assert.Equal(1, telecommunicationNumber.PartyContactMechanismId);
            Assert.Equal(100, telecommunicationNumber.ContactMechanismId);
            Assert.Equal("1", telecommunicationNumber.CountryCode);
            Assert.Equal("214", telecommunicationNumber.AreaCode);
            Assert.Equal("5551234", telecommunicationNumber.PhoneNumber);
            Assert.False(telecommunicationNumber.IsDeleted);
            Assert.True(telecommunicationNumber.IsPreferred);
            Assert.Equal(1, telecommunicationNumber.ContactMechanismUsageTypeId);
            Assert.Equal("US", telecommunicationNumber.ISOCode);
            Assert.True(telecommunicationNumber.IsDefault);
            Assert.NotNull(telecommunicationNumber.contactMechanismUsageType);
        }

        [Fact]
        public void TelecommunicationNumber_ImplementsITelecommunicationNumber()
        {
            // Arrange
            var telecommunicationNumber = new TelecommunicationNumber();

            // Assert
            Assert.IsAssignableFrom<ITelecommunicationNumber>(telecommunicationNumber);
        }

        [Fact]
        public void TelecommunicationNumber_DefaultValues()
        {
            // Arrange & Act
            var telecommunicationNumber = new TelecommunicationNumber();

            // Assert
            Assert.Equal("", telecommunicationNumber.CountryCode);
            Assert.Equal("", telecommunicationNumber.AreaCode);
            Assert.Equal("", telecommunicationNumber.PhoneNumber);
            Assert.False(telecommunicationNumber.IsDeleted);
            Assert.False(telecommunicationNumber.IsPreferred);
            Assert.Equal("", telecommunicationNumber.ISOCode);
            Assert.False(telecommunicationNumber.IsDefault);
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
        public void Integration_CreateAndListTelecommunicationNumber_WorkflowSucceeds()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var telecommunicationNumber = CreateValidTelecommunicationNumber();
            var createResponse = CreateSuccessResponse();
            var listResult = CreateTelecommunicationNumberList();

            _mockTelecommunicationNumberRepository
                .Setup(x => x.CreateTelecommunicationNumber(telecommunicationNumber))
                .Returns(createResponse);

            _mockTelecommunicationNumberRepository
                .Setup(x => x.ListTelecommunicationNumberForPerson(realPageId, "Mobile"))
                .Returns(listResult);

            var manageTelecommunicationNumber = new ManageTelecommunicationNumber(_mockTelecommunicationNumberRepository.Object);

            // Act
            var createResult = manageTelecommunicationNumber.CreateTelecommunicationNumber(telecommunicationNumber);
            var listTelNumbers = manageTelecommunicationNumber.ListTelecommunicationNumberForPerson(realPageId, "Mobile");

            // Assert
            Assert.NotNull(createResult);
            Assert.NotNull(listTelNumbers);
            Assert.Equal(2, listTelNumbers.Count);
        }

        #endregion

        #region Documentation Tests

        [Fact]
        public void ManageTelecommunicationNumber_ClassPurpose_Documentation()
        {
            // This test documents the class purpose:
            // ManageTelecommunicationNumber is responsible for:
            // 1. Creating/linking telecommunication numbers to persons
            // 2. Listing telecommunication numbers for a person by usage type
            // 3. Validating input parameters before repository calls
            //
            // Telecommunication numbers include:
            // - CountryCode (e.g., "1" for US)
            // - AreaCode (e.g., "214" for Dallas)
            // - PhoneNumber (the local number)
            // - IsPreferred, IsDefault, IsDeleted flags
            // - ContactMechanismUsageType (Mobile, Work, Home, etc.)

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageTelecommunicationNumber_ValidationRules_Documentation()
        {
            // This test documents validation rules:
            //
            // CreateTelecommunicationNumber:
            // - telecommunicationNumber parameter must not be null
            //   - Throws ArgumentNullException with message "Null TelecommunicationNumber."
            //
            // ListTelecommunicationNumberForPerson:
            // - realPageId must not be Guid.Empty
            //   - Throws Exception with message "Invalid parameter realPageId."
            //
            // Note: Current implementation doesn't validate:
            // - ContactMechanismUsageTypeName (accepts null or empty)
            // - Phone number format
            // - Country code format

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageTelecommunicationNumber_ExceptionHandling_Documentation()
        {
            // This test documents exception handling:
            //
            // CreateTelecommunicationNumber:
            // - Throws ArgumentNullException if telecommunicationNumber is null
            // - Does NOT catch repository exceptions (they propagate to caller)
            //
            // ListTelecommunicationNumberForPerson:
            // - Throws Exception if realPageId is Guid.Empty
            // - Does NOT catch repository exceptions (they propagate to caller)
            //
            // This is the expected behavior as it allows the caller to handle
            // database-level errors appropriately.

            Assert.True(true, "Documentation test");
        }

        #endregion
    }
}
