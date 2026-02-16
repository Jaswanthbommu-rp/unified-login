using System;
using System.Diagnostics.CodeAnalysis;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageGeographicBoundary business logic xUnit tests.
    /// Tests for geographic boundary management operations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageGeographicBoundaryTests : TestBase
    {
        private readonly Mock<IGeographicBoundaryRepository> _mockGeographicBoundaryRepository;

        public ManageGeographicBoundaryTests()
        {
            _mockGeographicBoundaryRepository = new Mock<IGeographicBoundaryRepository>();
        }

        #region Helper Methods

        private GeographicBoundary CreateValidGeographicBoundary()
        {
            return new GeographicBoundary
            {
                GeographicBoundaryId = 1,
                Name = "Richardson",
                GeographicBoundaryCode = "TX-RICHARDSON",
                Abbreviation = "RIC",
                GeographicBoundaryTypeId = 1,
                GeographicBoundaryType = new GeographicBoundaryType
                {
                    GeographicBoundaryTypeId = 1,
                    TypeName = "City"
                }
            };
        }

        private RepositoryResponse CreateSuccessRepositoryResponse()
        {
            return new RepositoryResponse
            {
                Id = 1,
                RealPageId = Guid.NewGuid(),
                ErrorMessage = ""
            };
        }

        private RepositoryResponse CreateErrorRepositoryResponse()
        {
            return new RepositoryResponse
            {
                Id = 0,
                RealPageId = Guid.Empty,
                ErrorMessage = "Error creating geographic boundary"
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageGeographicBoundary = new ManageGeographicBoundary(_mockGeographicBoundaryRepository.Object);

            // Assert
            Assert.NotNull(manageGeographicBoundary);
        }

        [Fact]
        public void Constructor_Default_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageGeographicBoundary = new ManageGeographicBoundary();

            // Assert
            Assert.NotNull(manageGeographicBoundary);
        }

        [Fact]
        public void Constructor_WithNullRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
            {
                var manageGeographicBoundary = new ManageGeographicBoundary(null);
                // Attempting to use the instance will throw
                manageGeographicBoundary.CreateGeographicBoundary(new GeographicBoundary());
            });
        }

        #endregion

        #region CreateGeographicBoundary Tests - Success Scenarios

        [Fact]
        public void CreateGeographicBoundary_WithValidGeographicBoundary_ReturnsSuccessResponse()
        {
            // Arrange
            var geographicBoundary = CreateValidGeographicBoundary();
            var expectedResponse = CreateSuccessRepositoryResponse();

            _mockGeographicBoundaryRepository
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Returns(expectedResponse);

            var manageGeographicBoundary = new ManageGeographicBoundary(_mockGeographicBoundaryRepository.Object);

            // Act
            var result = manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            Assert.Equal(expectedResponse.RealPageId, result.RealPageId);
            Assert.Equal(expectedResponse.ErrorMessage, result.ErrorMessage);

            _mockGeographicBoundaryRepository.Verify(
                x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()),
                Times.Once);
        }

        [Fact]
        public void CreateGeographicBoundary_WithCityBoundary_ReturnsResponse()
        {
            // Arrange
            var geographicBoundary = new GeographicBoundary
            {
                Name = "Dallas",
                GeographicBoundaryCode = "TX-DALLAS",
                Abbreviation = "DAL",
                GeographicBoundaryTypeId = 1
            };

            var expectedResponse = CreateSuccessRepositoryResponse();
            expectedResponse.Id = 100;

            _mockGeographicBoundaryRepository
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Returns(expectedResponse);

            var manageGeographicBoundary = new ManageGeographicBoundary(_mockGeographicBoundaryRepository.Object);

            // Act
            var result = manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Id);
        }

        [Fact]
        public void CreateGeographicBoundary_WithStateBoundary_ReturnsResponse()
        {
            // Arrange
            var geographicBoundary = new GeographicBoundary
            {
                Name = "Texas",
                GeographicBoundaryCode = "US-TX",
                Abbreviation = "TX",
                GeographicBoundaryTypeId = 2
            };

            var expectedResponse = CreateSuccessRepositoryResponse();
            expectedResponse.Id = 50;

            _mockGeographicBoundaryRepository
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Returns(expectedResponse);

            var manageGeographicBoundary = new ManageGeographicBoundary(_mockGeographicBoundaryRepository.Object);

            // Act
            var result = manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(50, result.Id);
        }

        [Fact]
        public void CreateGeographicBoundary_WithCountryBoundary_ReturnsResponse()
        {
            // Arrange
            var geographicBoundary = new GeographicBoundary
            {
                Name = "United States",
                GeographicBoundaryCode = "US",
                Abbreviation = "USA",
                GeographicBoundaryTypeId = 3
            };

            var expectedResponse = CreateSuccessRepositoryResponse();

            _mockGeographicBoundaryRepository
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Returns(expectedResponse);

            var manageGeographicBoundary = new ManageGeographicBoundary(_mockGeographicBoundaryRepository.Object);

            // Act
            var result = manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        [Fact]
        public void CreateGeographicBoundary_WithMinimalData_ReturnsResponse()
        {
            // Arrange
            var geographicBoundary = new GeographicBoundary
            {
                Name = "Test City"
            };

            var expectedResponse = CreateSuccessRepositoryResponse();

            _mockGeographicBoundaryRepository
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Returns(expectedResponse);

            var manageGeographicBoundary = new ManageGeographicBoundary(_mockGeographicBoundaryRepository.Object);

            // Act
            var result = manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region CreateGeographicBoundary Tests - Error Scenarios

        [Fact]
        public void CreateGeographicBoundary_WithNullGeographicBoundary_ThrowsArgumentNullException()
        {
            // Arrange
            var manageGeographicBoundary = new ManageGeographicBoundary(_mockGeographicBoundaryRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageGeographicBoundary.CreateGeographicBoundary(null));

            Assert.Equal("geographicBoundary", exception.ParamName);
            Assert.Contains("Null GeographicBoundary", exception.Message);
        }

        [Fact]
        public void CreateGeographicBoundary_WhenRepositoryReturnsError_ReturnsErrorResponse()
        {
            // Arrange
            var geographicBoundary = CreateValidGeographicBoundary();
            var errorResponse = CreateErrorRepositoryResponse();

            _mockGeographicBoundaryRepository
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Returns(errorResponse);

            var manageGeographicBoundary = new ManageGeographicBoundary(_mockGeographicBoundaryRepository.Object);

            // Act
            var result = manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Id);
            Assert.NotEmpty(result.ErrorMessage);
            Assert.Equal("Error creating geographic boundary", result.ErrorMessage);
        }

        [Fact]
        public void CreateGeographicBoundary_WhenRepositoryThrowsException_ThrowsException()
        {
            // Arrange
            var geographicBoundary = CreateValidGeographicBoundary();

            _mockGeographicBoundaryRepository
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Throws(new Exception("Database connection error"));

            var manageGeographicBoundary = new ManageGeographicBoundary(_mockGeographicBoundaryRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary));

            Assert.Equal("Database connection error", exception.Message);
        }

        #endregion

        #region CreateGeographicBoundary Tests - Validation

        [Fact]
        public void CreateGeographicBoundary_WithEmptyName_CallsRepository()
        {
            // Arrange
            var geographicBoundary = new GeographicBoundary
            {
                Name = "",
                GeographicBoundaryCode = "CODE",
                Abbreviation = "ABB"
            };

            var expectedResponse = CreateSuccessRepositoryResponse();

            _mockGeographicBoundaryRepository
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Returns(expectedResponse);

            var manageGeographicBoundary = new ManageGeographicBoundary(_mockGeographicBoundaryRepository.Object);

            // Act
            var result = manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary);

            // Assert
            Assert.NotNull(result);
            _mockGeographicBoundaryRepository.Verify(
                x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()),
                Times.Once);
        }

        [Fact]
        public void CreateGeographicBoundary_WithNullProperties_CallsRepository()
        {
            // Arrange
            var geographicBoundary = new GeographicBoundary
            {
                Name = "Test",
                GeographicBoundaryCode = null,
                Abbreviation = null
            };

            var expectedResponse = CreateSuccessRepositoryResponse();

            _mockGeographicBoundaryRepository
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Returns(expectedResponse);

            var manageGeographicBoundary = new ManageGeographicBoundary(_mockGeographicBoundaryRepository.Object);

            // Act
            var result = manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void CreateGeographicBoundary_WithZeroGeographicBoundaryTypeId_CallsRepository()
        {
            // Arrange
            var geographicBoundary = new GeographicBoundary
            {
                Name = "Test",
                GeographicBoundaryTypeId = 0
            };

            var expectedResponse = CreateSuccessRepositoryResponse();

            _mockGeographicBoundaryRepository
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Returns(expectedResponse);

            var manageGeographicBoundary = new ManageGeographicBoundary(_mockGeographicBoundaryRepository.Object);

            // Act
            var result = manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region CreateGeographicBoundary Tests - Different Geographic Boundary Types

        [Fact]
        public void CreateGeographicBoundary_WithCountyBoundary_ReturnsResponse()
        {
            // Arrange
            var geographicBoundary = new GeographicBoundary
            {
                Name = "Dallas County",
                GeographicBoundaryCode = "TX-DALLAS-COUNTY",
                Abbreviation = "DAL-C",
                GeographicBoundaryTypeId = 4
            };

            var expectedResponse = CreateSuccessRepositoryResponse();

            _mockGeographicBoundaryRepository
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Returns(expectedResponse);

            var manageGeographicBoundary = new ManageGeographicBoundary(_mockGeographicBoundaryRepository.Object);

            // Act
            var result = manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void CreateGeographicBoundary_WithZipCodeBoundary_ReturnsResponse()
        {
            // Arrange
            var geographicBoundary = new GeographicBoundary
            {
                Name = "75080",
                GeographicBoundaryCode = "ZIP-75080",
                Abbreviation = "75080",
                GeographicBoundaryTypeId = 5
            };

            var expectedResponse = CreateSuccessRepositoryResponse();

            _mockGeographicBoundaryRepository
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Returns(expectedResponse);

            var manageGeographicBoundary = new ManageGeographicBoundary(_mockGeographicBoundaryRepository.Object);

            // Act
            var result = manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region CreateGeographicBoundary Tests - Repository Verification

        [Fact]
        public void CreateGeographicBoundary_CallsRepositoryOnce()
        {
            // Arrange
            var geographicBoundary = CreateValidGeographicBoundary();
            var expectedResponse = CreateSuccessRepositoryResponse();

            _mockGeographicBoundaryRepository
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Returns(expectedResponse);

            var manageGeographicBoundary = new ManageGeographicBoundary(_mockGeographicBoundaryRepository.Object);

            // Act
            manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary);

            // Assert
            _mockGeographicBoundaryRepository.Verify(
                x => x.CreateGeographicBoundary(It.Is<IGeographicBoundary>(
                    gb => gb.Name == geographicBoundary.Name)),
                Times.Once);
        }

        [Fact]
        public void CreateGeographicBoundary_PassesCorrectParameters()
        {
            // Arrange
            var geographicBoundary = CreateValidGeographicBoundary();
            var expectedResponse = CreateSuccessRepositoryResponse();

            IGeographicBoundary capturedBoundary = null;

            _mockGeographicBoundaryRepository
                .Setup(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Callback<IGeographicBoundary>(gb => capturedBoundary = gb)
                .Returns(expectedResponse);

            var manageGeographicBoundary = new ManageGeographicBoundary(_mockGeographicBoundaryRepository.Object);

            // Act
            manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary);

            // Assert
            Assert.NotNull(capturedBoundary);
            Assert.Equal(geographicBoundary.Name, capturedBoundary.Name);
            Assert.Equal(geographicBoundary.GeographicBoundaryCode, capturedBoundary.GeographicBoundaryCode);
            Assert.Equal(geographicBoundary.Abbreviation, capturedBoundary.Abbreviation);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void CreateGeographicBoundary_MultipleCallsWithSameInstance_WorksCorrectly()
        {
            // Arrange
            var geographicBoundary1 = new GeographicBoundary { Name = "City1" };
            var geographicBoundary2 = new GeographicBoundary { Name = "City2" };

            var response1 = new RepositoryResponse { Id = 1 };
            var response2 = new RepositoryResponse { Id = 2 };

            _mockGeographicBoundaryRepository
                .SetupSequence(x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()))
                .Returns(response1)
                .Returns(response2);

            var manageGeographicBoundary = new ManageGeographicBoundary(_mockGeographicBoundaryRepository.Object);

            // Act
            var result1 = manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary1);
            var result2 = manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary2);

            // Assert
            Assert.Equal(1, result1.Id);
            Assert.Equal(2, result2.Id);
            _mockGeographicBoundaryRepository.Verify(
                x => x.CreateGeographicBoundary(It.IsAny<IGeographicBoundary>()),
                Times.Exactly(2));
        }

        #endregion

        #region GeographicBoundary Object Tests

        [Fact]
        public void GeographicBoundary_PropertyAssignment_WorksCorrectly()
        {
            // Arrange & Act
            var geographicBoundary = new GeographicBoundary
            {
                GeographicBoundaryId = 100,
                Name = "Austin",
                GeographicBoundaryCode = "TX-AUSTIN",
                Abbreviation = "AUS",
                GeographicBoundaryTypeId = 1
            };

            // Assert
            Assert.Equal(100, geographicBoundary.GeographicBoundaryId);
            Assert.Equal("Austin", geographicBoundary.Name);
            Assert.Equal("TX-AUSTIN", geographicBoundary.GeographicBoundaryCode);
            Assert.Equal("AUS", geographicBoundary.Abbreviation);
            Assert.Equal(1, geographicBoundary.GeographicBoundaryTypeId);
        }

        [Fact]
        public void GeographicBoundary_DefaultValues_AreSetCorrectly()
        {
            // Arrange & Act
            var geographicBoundary = new GeographicBoundary();

            // Assert
            Assert.Equal(0, geographicBoundary.GeographicBoundaryId);
            Assert.Null(geographicBoundary.Name);
            Assert.Null(geographicBoundary.GeographicBoundaryCode);
            Assert.Null(geographicBoundary.Abbreviation);
            Assert.Equal(0, geographicBoundary.GeographicBoundaryTypeId);
        }

        #endregion

        #region RepositoryResponse Tests

        [Fact]
        public void RepositoryResponse_WithSuccess_HasValidId()
        {
            // Arrange
            var response = new RepositoryResponse
            {
                Id = 123,
                RealPageId = Guid.NewGuid(),
                ErrorMessage = ""
            };

            // Assert
            Assert.Equal(123, response.Id);
            Assert.NotEqual(Guid.Empty, response.RealPageId);
            Assert.Empty(response.ErrorMessage);
        }

        [Fact]
        public void RepositoryResponse_WithError_HasErrorMessage()
        {
            // Arrange
            var response = new RepositoryResponse
            {
                Id = 0,
                RealPageId = Guid.Empty,
                ErrorMessage = "Error occurred"
            };

            // Assert
            Assert.Equal(0, response.Id);
            Assert.Equal(Guid.Empty, response.RealPageId);
            Assert.Equal("Error occurred", response.ErrorMessage);
        }

        #endregion
    }
}
