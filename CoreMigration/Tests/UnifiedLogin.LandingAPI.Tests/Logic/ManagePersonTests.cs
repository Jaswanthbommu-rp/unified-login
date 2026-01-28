using System;
using System.Diagnostics.CodeAnalysis;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManagePerson business logic xUnit tests.
    /// Tests for person management operations including CRUD operations.
    /// Migrated from RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.ManagePersonTests
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManagePersonTests : TestBase
    {
        private readonly Mock<IPersonRepository> _mockPersonRepository;
        private readonly Mock<IRepository> _mockRepository;

        public ManagePersonTests()
        {
            _mockPersonRepository = new Mock<IPersonRepository>();
            _mockRepository = new Mock<IRepository>();
        }

        #region Helper Methods

        private Person CreateValidPerson()
        {
            return new Person
            {
                RealPageId = Guid.NewGuid(),
                PartyId = 1000,
                FirstName = "John",
                MiddleName = "M",
                LastName = "Doe",
                EmployeeId = "EMP001",
                Suffix = "Jr.",
                Title = "Manager"
            };
        }

        private IPerson CreateValidIPerson()
        {
            return new Person
            {
                RealPageId = Guid.NewGuid(),
                PartyId = 1000,
                FirstName = "Jane",
                MiddleName = "A",
                LastName = "Smith",
                EmployeeId = "EMP002",
                Suffix = "Sr.",
                Title = "Director"
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNoParameters_InitializesSuccessfully()
        {
            // Arrange & Act
            var managePerson = new ManagePerson();

            // Assert
            Assert.NotNull(managePerson);
        }

        [Fact]
        public void Constructor_WithRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var managePerson = new ManagePerson(_mockRepository.Object);

            // Assert
            Assert.NotNull(managePerson);
        }

        [Fact]
        public void Constructor_WithPersonRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var managePerson = new ManagePerson(_mockPersonRepository.Object);

            // Assert
            Assert.NotNull(managePerson);
        }

        #endregion

        #region CreatePerson Tests

        [Fact]
        public void CreatePerson_WithValidPerson_ReturnsSuccessResponse()
        {
            // Arrange
            var person = CreateValidIPerson();
            var expectedResponse = new RepositoryResponse
            {
                Id = 1,
                RealPageId = Guid.NewGuid(),
                ErrorMessage = ""
            };

            _mockPersonRepository
                .Setup(x => x.CreatePerson(person))
                .Returns(expectedResponse);

            var managePerson = new ManagePerson(_mockPersonRepository.Object);

            // Act
            var result = managePerson.CreatePerson(person);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            Assert.Empty(result.ErrorMessage);
            _mockPersonRepository.Verify(x => x.CreatePerson(person), Times.Once);
        }

        [Fact]
        public void CreatePerson_WithNullPerson_ThrowsArgumentNullException()
        {
            // Arrange
            IPerson person = null;
            var managePerson = new ManagePerson(_mockPersonRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                managePerson.CreatePerson(person));

            Assert.Equal("person", exception.ParamName);
            Assert.Contains("Null Person", exception.Message);
        }

        [Fact]
        public void CreatePerson_WithDifferentPersonData_CallsRepositoryCorrectly()
        {
            // Arrange
            var person = CreateValidIPerson();
            ((Person)person).FirstName = "Michael";
            ((Person)person).LastName = "Johnson";

            var expectedResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockPersonRepository
                .Setup(x => x.CreatePerson(person))
                .Returns(expectedResponse);

            var managePerson = new ManagePerson(_mockPersonRepository.Object);

            // Act
            var result = managePerson.CreatePerson(person);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            _mockPersonRepository.Verify(x => x.CreatePerson(person), Times.Once);
        }

        #endregion

        #region GetPerson Tests

        [Fact]
        public void GetPerson_WithValidRealPageId_ReturnsPerson()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var expectedPerson = CreateValidPerson();
            expectedPerson.RealPageId = realPageId;

            _mockPersonRepository
                .Setup(x => x.GetPerson(realPageId))
                .Returns(expectedPerson);

            var managePerson = new ManagePerson(_mockPersonRepository.Object);

            // Act
            var result = managePerson.GetPerson(realPageId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPerson.PartyId, result.PartyId);
            Assert.Equal(expectedPerson.RealPageId, result.RealPageId);
            Assert.Equal(expectedPerson.FirstName, result.FirstName);
            Assert.Equal(expectedPerson.LastName, result.LastName);
            _mockPersonRepository.Verify(x => x.GetPerson(realPageId), Times.Once);
        }

        [Fact]
        public void GetPerson_WithEmptyRealPageId_ThrowsException()
        {
            // Arrange
            var realPageId = Guid.Empty;
            var managePerson = new ManagePerson(_mockPersonRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePerson.GetPerson(realPageId));

            Assert.Equal("Invalid parameter realPageId.", exception.Message);
        }

        [Fact]
        public void GetPerson_WhenRepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            _mockPersonRepository
                .Setup(x => x.GetPerson(realPageId))
                .Returns((Person)null);

            var managePerson = new ManagePerson(_mockPersonRepository.Object);

            // Act
            var result = managePerson.GetPerson(realPageId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region UpdatePerson Tests

        [Fact]
        public void UpdatePerson_WithValidParameters_ReturnsSuccessResponse()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var person = CreateValidIPerson();
            var expectedResponse = new RepositoryResponse
            {
                Id = 1,
                RealPageId = realPageId,
                ErrorMessage = ""
            };

            _mockPersonRepository
                .Setup(x => x.UpdatePerson(realPageId, person))
                .Returns(expectedResponse);

            var managePerson = new ManagePerson(_mockPersonRepository.Object);

            // Act
            var result = managePerson.UpdatePerson(realPageId, person);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            Assert.Empty(result.ErrorMessage);
            _mockPersonRepository.Verify(x => x.UpdatePerson(realPageId, person), Times.Once);
        }

        [Fact]
        public void UpdatePerson_WithEmptyRealPageId_ThrowsException()
        {
            // Arrange
            var realPageId = Guid.Empty;
            var person = CreateValidIPerson();
            var managePerson = new ManagePerson(_mockPersonRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePerson.UpdatePerson(realPageId, person));

            Assert.Equal("Invalid parameter realPageId.", exception.Message);
        }

        [Fact]
        public void UpdatePerson_WithNullPerson_ThrowsArgumentNullException()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            IPerson person = null;
            var managePerson = new ManagePerson(_mockPersonRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                managePerson.UpdatePerson(realPageId, person));

            Assert.Equal("person", exception.ParamName);
            Assert.Contains("Null Person", exception.Message);
        }

        [Fact]
        public void UpdatePerson_WithBothInvalidParameters_ThrowsExceptionForRealPageId()
        {
            // Arrange
            var realPageId = Guid.Empty;
            IPerson person = null;
            var managePerson = new ManagePerson(_mockPersonRepository.Object);

            // Act & Assert - Should throw for the first parameter checked (realPageId)
            var exception = Assert.Throws<Exception>(() =>
                managePerson.UpdatePerson(realPageId, person));

            Assert.Equal("Invalid parameter realPageId.", exception.Message);
        }

        [Fact]
        public void UpdatePerson_WithModifiedPersonData_CallsRepositoryCorrectly()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var person = CreateValidIPerson();
            ((Person)person).FirstName = "Updated";
            ((Person)person).LastName = "Name";

            var expectedResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockPersonRepository
                .Setup(x => x.UpdatePerson(realPageId, person))
                .Returns(expectedResponse);

            var managePerson = new ManagePerson(_mockPersonRepository.Object);

            // Act
            var result = managePerson.UpdatePerson(realPageId, person);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            _mockPersonRepository.Verify(x => x.UpdatePerson(realPageId, person), Times.Once);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void ManagePerson_CompleteWorkflow_CreateGetUpdate()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var person = CreateValidIPerson();
            var expectedPerson = CreateValidPerson();
            expectedPerson.RealPageId = realPageId;
            var createResponse = new RepositoryResponse { Id = 1, RealPageId = realPageId, ErrorMessage = "" };
            var updateResponse = new RepositoryResponse { Id = 1, RealPageId = realPageId, ErrorMessage = "" };

            _mockPersonRepository
                .Setup(x => x.CreatePerson(person))
                .Returns(createResponse);

            _mockPersonRepository
                .Setup(x => x.GetPerson(realPageId))
                .Returns(expectedPerson);

            _mockPersonRepository
                .Setup(x => x.UpdatePerson(realPageId, person))
                .Returns(updateResponse);

            var managePerson = new ManagePerson(_mockPersonRepository.Object);

            // Act
            var createResult = managePerson.CreatePerson(person);
            var getResult = managePerson.GetPerson(realPageId);
            var updateResult = managePerson.UpdatePerson(realPageId, person);

            // Assert
            Assert.NotNull(createResult);
            Assert.NotNull(getResult);
            Assert.NotNull(updateResult);
            Assert.Equal(1, createResult.Id);
            Assert.Equal(1000, getResult.PartyId);
            Assert.Equal(1, updateResult.Id);
        }

        [Fact]
        public void ManagePerson_MultiplePeople_HandlesCorrectly()
        {
            // Arrange
            var realPageId1 = Guid.NewGuid();
            var realPageId2 = Guid.NewGuid();
            var person1 = CreateValidPerson();
            var person2 = CreateValidPerson();
            person1.RealPageId = realPageId1;
            person2.RealPageId = realPageId2;
            person2.PartyId = 2;

            _mockPersonRepository
                .Setup(x => x.GetPerson(realPageId1))
                .Returns(person1);

            _mockPersonRepository
                .Setup(x => x.GetPerson(realPageId2))
                .Returns(person2);

            var managePerson = new ManagePerson(_mockPersonRepository.Object);

            // Act
            var result1 = managePerson.GetPerson(realPageId1);
            var result2 = managePerson.GetPerson(realPageId2);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Equal(realPageId1, result1.RealPageId);
            Assert.Equal(realPageId2, result2.RealPageId);
            Assert.NotEqual(result1.PartyId, result2.PartyId);
        }

        #endregion

        #region Edge Cases and Additional Scenarios

        [Fact]
        public void CreatePerson_WithMinimalData_CallsRepositoryCorrectly()
        {
            // Arrange
            var person = CreateValidIPerson();
            ((Person)person).MiddleName = null;
            ((Person)person).EmployeeId = null;

            var expectedResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockPersonRepository
                .Setup(x => x.CreatePerson(person))
                .Returns(expectedResponse);

            var managePerson = new ManagePerson(_mockPersonRepository.Object);

            // Act
            var result = managePerson.CreatePerson(person);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void GetPerson_WithDifferentRealPageIds_CallsRepositoryCorrectly()
        {
            // Arrange
            var realPageId1 = Guid.NewGuid();
            var realPageId2 = Guid.NewGuid();
            var person1 = CreateValidPerson();
            var person2 = CreateValidPerson();

            _mockPersonRepository
                .Setup(x => x.GetPerson(realPageId1))
                .Returns(person1);

            _mockPersonRepository
                .Setup(x => x.GetPerson(realPageId2))
                .Returns(person2);

            var managePerson = new ManagePerson(_mockPersonRepository.Object);

            // Act
            var result1 = managePerson.GetPerson(realPageId1);
            var result2 = managePerson.GetPerson(realPageId2);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            _mockPersonRepository.Verify(x => x.GetPerson(realPageId1), Times.Once);
            _mockPersonRepository.Verify(x => x.GetPerson(realPageId2), Times.Once);
        }

        [Fact]
        public void UpdatePerson_WithCompletePersonData_CallsRepositoryCorrectly()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var person = CreateValidIPerson();
            ((Person)person).FirstName = "Complete";
            ((Person)person).MiddleName = "Middle";
            ((Person)person).LastName = "Data";
            ((Person)person).EmployeeId = "EMP999";
            ((Person)person).Title = "CEO";

            var expectedResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockPersonRepository
                .Setup(x => x.UpdatePerson(realPageId, person))
                .Returns(expectedResponse);

            var managePerson = new ManagePerson(_mockPersonRepository.Object);

            // Act
            var result = managePerson.UpdatePerson(realPageId, person);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        #endregion
    }
}
