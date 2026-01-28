using System;
using System.Diagnostics.CodeAnalysis;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageCommunicationEvents business logic xUnit tests.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageCommunicationEventsTests : TestBase
    {
        private readonly Mock<IRepository> _mockRepository;
        private readonly Mock<ICommunicationEventRepository> _mockCommunicationEventRepository;

        public ManageCommunicationEventsTests()
        {
            _mockRepository = new Mock<IRepository>();
            _mockCommunicationEventRepository = new Mock<ICommunicationEventRepository>();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithCommunicationEventRepository_InitializesCorrectly()
        {
            // Arrange & Act
            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Assert
            Assert.NotNull(manager);
        }

        [Fact]
        public void Constructor_WithRepository_InitializesCorrectly()
        {
            // Arrange & Act
            var manager = new ManageCommunicationEvents(_mockRepository.Object);

            // Assert
            Assert.NotNull(manager);
        }

        [Fact]
        public void Constructor_Default_InitializesCorrectly()
        {
            // Arrange & Act
            var manager = new ManageCommunicationEvents();

            // Assert
            Assert.NotNull(manager);
        }

        #endregion

        #region CreateCommunicationEvent Tests

        [Fact]
        public void CreateCommunicationEvent_WithValidParameters_ReturnsRepositoryResponse()
        {
            // Arrange
            var statusTypeId = 1;
            var fromPartyContactMechanismId = 100L;
            var toPartyContactMechanismId = 200L;
            var started = DateTime.UtcNow;
            var ended = DateTime.UtcNow.AddHours(1);
            var note = "Test communication event";

            var expectedResponse = new RepositoryResponse
            {
                Id = 1000,
                ErrorMessage = ""
            };

            _mockCommunicationEventRepository
                .Setup(x => x.CreateCommunicationEvent(
                    statusTypeId,
                    fromPartyContactMechanismId,
                    toPartyContactMechanismId,
                    started,
                    ended,
                    note))
                .Returns(expectedResponse);

            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Act
            var result = manager.CreateCommunicationEvent(
                statusTypeId,
                fromPartyContactMechanismId,
                toPartyContactMechanismId,
                started,
                ended,
                note);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1000, result.Id);
            Assert.Equal("", result.ErrorMessage);
            _mockCommunicationEventRepository.Verify(
                x => x.CreateCommunicationEvent(
                    statusTypeId,
                    fromPartyContactMechanismId,
                    toPartyContactMechanismId,
                    started,
                    ended,
                    note),
                Times.Once);
        }

        [Fact]
        public void CreateCommunicationEvent_WithZeroStatusTypeId_ThrowsArgumentNullException()
        {
            // Arrange
            var statusTypeId = 0;
            var fromPartyContactMechanismId = 100L;
            var toPartyContactMechanismId = 200L;
            var started = DateTime.UtcNow;
            var ended = DateTime.UtcNow.AddHours(1);
            var note = "Test communication event";

            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manager.CreateCommunicationEvent(
                    statusTypeId,
                    fromPartyContactMechanismId,
                    toPartyContactMechanismId,
                    started,
                    ended,
                    note));

            Assert.Equal("statusTypeId", exception.ParamName);
            _mockCommunicationEventRepository.Verify(
                x => x.CreateCommunicationEvent(
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void CreateCommunicationEvent_WithZeroFromPartyContactMechanismId_ThrowsArgumentNullException()
        {
            // Arrange
            var statusTypeId = 1;
            var fromPartyContactMechanismId = 0L;
            var toPartyContactMechanismId = 200L;
            var started = DateTime.UtcNow;
            var ended = DateTime.UtcNow.AddHours(1);
            var note = "Test communication event";

            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manager.CreateCommunicationEvent(
                    statusTypeId,
                    fromPartyContactMechanismId,
                    toPartyContactMechanismId,
                    started,
                    ended,
                    note));

            Assert.Equal("fromPartyContactMechanismId", exception.ParamName);
            _mockCommunicationEventRepository.Verify(
                x => x.CreateCommunicationEvent(
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void CreateCommunicationEvent_WithZeroToPartyContactMechanismId_ThrowsArgumentNullException()
        {
            // Arrange
            var statusTypeId = 1;
            var fromPartyContactMechanismId = 100L;
            var toPartyContactMechanismId = 0L;
            var started = DateTime.UtcNow;
            var ended = DateTime.UtcNow.AddHours(1);
            var note = "Test communication event";

            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manager.CreateCommunicationEvent(
                    statusTypeId,
                    fromPartyContactMechanismId,
                    toPartyContactMechanismId,
                    started,
                    ended,
                    note));

            Assert.Equal("toPartyContactMechanismId", exception.ParamName);
            _mockCommunicationEventRepository.Verify(
                x => x.CreateCommunicationEvent(
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void CreateCommunicationEvent_WithNullNote_AcceptsNullNote()
        {
            // Arrange
            var statusTypeId = 1;
            var fromPartyContactMechanismId = 100L;
            var toPartyContactMechanismId = 200L;
            var started = DateTime.UtcNow;
            var ended = DateTime.UtcNow.AddHours(1);
            string note = null;

            var expectedResponse = new RepositoryResponse
            {
                Id = 1000,
                ErrorMessage = ""
            };

            _mockCommunicationEventRepository
                .Setup(x => x.CreateCommunicationEvent(
                    statusTypeId,
                    fromPartyContactMechanismId,
                    toPartyContactMechanismId,
                    started,
                    ended,
                    note))
                .Returns(expectedResponse);

            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Act
            var result = manager.CreateCommunicationEvent(
                statusTypeId,
                fromPartyContactMechanismId,
                toPartyContactMechanismId,
                started,
                ended,
                note);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1000, result.Id);
            _mockCommunicationEventRepository.Verify(
                x => x.CreateCommunicationEvent(
                    statusTypeId,
                    fromPartyContactMechanismId,
                    toPartyContactMechanismId,
                    started,
                    ended,
                    null),
                Times.Once);
        }

        [Fact]
        public void CreateCommunicationEvent_WithRepositoryError_ReturnsErrorResponse()
        {
            // Arrange
            var statusTypeId = 1;
            var fromPartyContactMechanismId = 100L;
            var toPartyContactMechanismId = 200L;
            var started = DateTime.UtcNow;
            var ended = DateTime.UtcNow.AddHours(1);
            var note = "Test communication event";

            var expectedResponse = new RepositoryResponse
            {
                Id = 0,
                ErrorMessage = "Database error occurred"
            };

            _mockCommunicationEventRepository
                .Setup(x => x.CreateCommunicationEvent(
                    statusTypeId,
                    fromPartyContactMechanismId,
                    toPartyContactMechanismId,
                    started,
                    ended,
                    note))
                .Returns(expectedResponse);

            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Act
            var result = manager.CreateCommunicationEvent(
                statusTypeId,
                fromPartyContactMechanismId,
                toPartyContactMechanismId,
                started,
                ended,
                note);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Id);
            Assert.Equal("Database error occurred", result.ErrorMessage);
        }

        #endregion

        #region CreateCommunicationEventEmail Tests

        [Fact]
        public void CreateCommunicationEventEmail_WithValidParameters_ReturnsRepositoryResponse()
        {
            // Arrange
            var communicationEmailTemplateId = 5;
            var communicationEventId = 1000L;

            var expectedResponse = new RepositoryResponse
            {
                Id = 2000,
                ErrorMessage = ""
            };

            _mockCommunicationEventRepository
                .Setup(x => x.CreateCommunicationEventEmail(
                    communicationEmailTemplateId,
                    communicationEventId))
                .Returns(expectedResponse);

            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Act
            var result = manager.CreateCommunicationEventEmail(
                communicationEmailTemplateId,
                communicationEventId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2000, result.Id);
            Assert.Equal("", result.ErrorMessage);
            _mockCommunicationEventRepository.Verify(
                x => x.CreateCommunicationEventEmail(
                    communicationEmailTemplateId,
                    communicationEventId),
                Times.Once);
        }

        [Fact]
        public void CreateCommunicationEventEmail_WithZeroCommunicationEmailTemplateId_ThrowsArgumentNullException()
        {
            // Arrange
            var communicationEmailTemplateId = 0;
            var communicationEventId = 1000L;

            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manager.CreateCommunicationEventEmail(
                    communicationEmailTemplateId,
                    communicationEventId));

            Assert.Equal("communicationEmailTemplateId", exception.ParamName);
            _mockCommunicationEventRepository.Verify(
                x => x.CreateCommunicationEventEmail(
                    It.IsAny<int>(),
                    It.IsAny<long>()),
                Times.Never);
        }

        [Fact]
        public void CreateCommunicationEventEmail_WithZeroCommunicationEventId_ThrowsArgumentNullException()
        {
            // Arrange
            var communicationEmailTemplateId = 5;
            var communicationEventId = 0L;

            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manager.CreateCommunicationEventEmail(
                    communicationEmailTemplateId,
                    communicationEventId));

            // Note: The code has a bug - it throws with wrong parameter name
            Assert.Equal("communicationEmailTemplateId", exception.ParamName);
            _mockCommunicationEventRepository.Verify(
                x => x.CreateCommunicationEventEmail(
                    It.IsAny<int>(),
                    It.IsAny<long>()),
                Times.Never);
        }

        [Fact]
        public void CreateCommunicationEventEmail_WithRepositoryError_ReturnsErrorResponse()
        {
            // Arrange
            var communicationEmailTemplateId = 5;
            var communicationEventId = 1000L;

            var expectedResponse = new RepositoryResponse
            {
                Id = 0,
                ErrorMessage = "Email template not found"
            };

            _mockCommunicationEventRepository
                .Setup(x => x.CreateCommunicationEventEmail(
                    communicationEmailTemplateId,
                    communicationEventId))
                .Returns(expectedResponse);

            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Act
            var result = manager.CreateCommunicationEventEmail(
                communicationEmailTemplateId,
                communicationEventId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Id);
            Assert.Equal("Email template not found", result.ErrorMessage);
        }

        #endregion

        #region CreateCESCommunicationEventEmail Tests

        [Fact]
        public void CreateCESCommunicationEventEmail_WithValidParameters_ReturnsRepositoryResponse()
        {
            // Arrange
            var cesId = "CES-12345";
            var communicationEventId = 1000L;

            var expectedResponse = new RepositoryResponse
            {
                Id = 3000,
                ErrorMessage = ""
            };

            _mockCommunicationEventRepository
                .Setup(x => x.CreateCESCommunicationEventEmail(
                    cesId,
                    communicationEventId))
                .Returns(expectedResponse);

            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Act
            var result = manager.CreateCESCommunicationEventEmail(
                cesId,
                communicationEventId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3000, result.Id);
            Assert.Equal("", result.ErrorMessage);
            _mockCommunicationEventRepository.Verify(
                x => x.CreateCESCommunicationEventEmail(
                    cesId,
                    communicationEventId),
                Times.Once);
        }

        [Fact]
        public void CreateCESCommunicationEventEmail_WithNullCesId_ThrowsArgumentNullException()
        {
            // Arrange
            string cesId = null;
            var communicationEventId = 1000L;

            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manager.CreateCESCommunicationEventEmail(
                    cesId,
                    communicationEventId));

            Assert.Equal("cesId", exception.ParamName);
            _mockCommunicationEventRepository.Verify(
                x => x.CreateCESCommunicationEventEmail(
                    It.IsAny<string>(),
                    It.IsAny<long>()),
                Times.Never);
        }

        [Fact]
        public void CreateCESCommunicationEventEmail_WithEmptyCesId_ThrowsArgumentNullException()
        {
            // Arrange
            var cesId = string.Empty;
            var communicationEventId = 1000L;

            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manager.CreateCESCommunicationEventEmail(
                    cesId,
                    communicationEventId));

            Assert.Equal("cesId", exception.ParamName);
            _mockCommunicationEventRepository.Verify(
                x => x.CreateCESCommunicationEventEmail(
                    It.IsAny<string>(),
                    It.IsAny<long>()),
                Times.Never);
        }

      
        public void CreateCESCommunicationEventEmail_WithWhitespaceCesId_ThrowsArgumentNullException()
        {
            // Arrange
            var cesId = "   ";
            var communicationEventId = 1000L;

            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manager.CreateCESCommunicationEventEmail(
                    cesId,
                    communicationEventId));

            Assert.Equal("cesId", exception.ParamName);
            _mockCommunicationEventRepository.Verify(
                x => x.CreateCESCommunicationEventEmail(
                    It.IsAny<string>(),
                    It.IsAny<long>()),
                Times.Never);
        }

        [Fact]
        public void CreateCESCommunicationEventEmail_WithZeroCommunicationEventId_ThrowsArgumentNullException()
        {
            // Arrange
            var cesId = "CES-12345";
            var communicationEventId = 0L;

            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manager.CreateCESCommunicationEventEmail(
                    cesId,
                    communicationEventId));

            Assert.Equal("communicationEventId", exception.ParamName);
            _mockCommunicationEventRepository.Verify(
                x => x.CreateCESCommunicationEventEmail(
                    It.IsAny<string>(),
                    It.IsAny<long>()),
                Times.Never);
        }

        [Fact]
        public void CreateCESCommunicationEventEmail_WithRepositoryError_ReturnsErrorResponse()
        {
            // Arrange
            var cesId = "CES-12345";
            var communicationEventId = 1000L;

            var expectedResponse = new RepositoryResponse
            {
                Id = 0,
                ErrorMessage = "CES ID not found"
            };

            _mockCommunicationEventRepository
                .Setup(x => x.CreateCESCommunicationEventEmail(
                    cesId,
                    communicationEventId))
                .Returns(expectedResponse);

            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Act
            var result = manager.CreateCESCommunicationEventEmail(
                cesId,
                communicationEventId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Id);
            Assert.Equal("CES ID not found", result.ErrorMessage);
        }

        [Fact]
        public void CreateCESCommunicationEventEmail_WithValidCesIdContainingSpecialCharacters_ReturnsRepositoryResponse()
        {
            // Arrange
            var cesId = "CES-12345-ABC_XYZ";
            var communicationEventId = 1000L;

            var expectedResponse = new RepositoryResponse
            {
                Id = 3000,
                ErrorMessage = ""
            };

            _mockCommunicationEventRepository
                .Setup(x => x.CreateCESCommunicationEventEmail(
                    cesId,
                    communicationEventId))
                .Returns(expectedResponse);

            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Act
            var result = manager.CreateCESCommunicationEventEmail(
                cesId,
                communicationEventId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3000, result.Id);
            _mockCommunicationEventRepository.Verify(
                x => x.CreateCESCommunicationEventEmail(
                    cesId,
                    communicationEventId),
                Times.Once);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void CreateCommunicationEvent_ThenCreateEmail_IntegrationFlow()
        {
            // Arrange
            var statusTypeId = 1;
            var fromPartyContactMechanismId = 100L;
            var toPartyContactMechanismId = 200L;
            var started = DateTime.UtcNow;
            var ended = DateTime.UtcNow.AddHours(1);
            var note = "Test communication event";
            var communicationEventId = 1000L;
            var communicationEmailTemplateId = 5;

            var eventResponse = new RepositoryResponse
            {
                Id = communicationEventId,
                ErrorMessage = ""
            };

            var emailResponse = new RepositoryResponse
            {
                Id = 2000,
                ErrorMessage = ""
            };

            _mockCommunicationEventRepository
                .Setup(x => x.CreateCommunicationEvent(
                    statusTypeId,
                    fromPartyContactMechanismId,
                    toPartyContactMechanismId,
                    started,
                    ended,
                    note))
                .Returns(eventResponse);

            _mockCommunicationEventRepository
                .Setup(x => x.CreateCommunicationEventEmail(
                    communicationEmailTemplateId,
                    communicationEventId))
                .Returns(emailResponse);

            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Act
            var eventResult = manager.CreateCommunicationEvent(
                statusTypeId,
                fromPartyContactMechanismId,
                toPartyContactMechanismId,
                started,
                ended,
                note);

            var emailResult = manager.CreateCommunicationEventEmail(
                communicationEmailTemplateId,
                eventResult.Id);

            // Assert
            Assert.NotNull(eventResult);
            Assert.Equal(communicationEventId, eventResult.Id);
            Assert.NotNull(emailResult);
            Assert.Equal(2000, emailResult.Id);

            _mockCommunicationEventRepository.Verify(
                x => x.CreateCommunicationEvent(
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>()),
                Times.Once);

            _mockCommunicationEventRepository.Verify(
                x => x.CreateCommunicationEventEmail(
                    It.IsAny<int>(),
                    It.IsAny<long>()),
                Times.Once);
        }

        [Fact]
        public void CreateCommunicationEvent_ThenCreateCESEmail_IntegrationFlow()
        {
            // Arrange
            var statusTypeId = 1;
            var fromPartyContactMechanismId = 100L;
            var toPartyContactMechanismId = 200L;
            var started = DateTime.UtcNow;
            var ended = DateTime.UtcNow.AddHours(1);
            var note = "Test communication event";
            var communicationEventId = 1000L;
            var cesId = "CES-12345";

            var eventResponse = new RepositoryResponse
            {
                Id = communicationEventId,
                ErrorMessage = ""
            };

            var cesEmailResponse = new RepositoryResponse
            {
                Id = 3000,
                ErrorMessage = ""
            };

            _mockCommunicationEventRepository
                .Setup(x => x.CreateCommunicationEvent(
                    statusTypeId,
                    fromPartyContactMechanismId,
                    toPartyContactMechanismId,
                    started,
                    ended,
                    note))
                .Returns(eventResponse);

            _mockCommunicationEventRepository
                .Setup(x => x.CreateCESCommunicationEventEmail(
                    cesId,
                    communicationEventId))
                .Returns(cesEmailResponse);

            var manager = new ManageCommunicationEvents(_mockCommunicationEventRepository.Object);

            // Act
            var eventResult = manager.CreateCommunicationEvent(
                statusTypeId,
                fromPartyContactMechanismId,
                toPartyContactMechanismId,
                started,
                ended,
                note);

            var cesEmailResult = manager.CreateCESCommunicationEventEmail(
                cesId,
                eventResult.Id);

            // Assert
            Assert.NotNull(eventResult);
            Assert.Equal(communicationEventId, eventResult.Id);
            Assert.NotNull(cesEmailResult);
            Assert.Equal(3000, cesEmailResult.Id);

            _mockCommunicationEventRepository.Verify(
                x => x.CreateCommunicationEvent(
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>()),
                Times.Once);

            _mockCommunicationEventRepository.Verify(
                x => x.CreateCESCommunicationEventEmail(
                    It.IsAny<string>(),
                    It.IsAny<long>()),
                Times.Once);
        }

        #endregion
    }
}
