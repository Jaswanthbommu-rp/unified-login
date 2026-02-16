using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    /// Comprehensive unit tests for ContactMechanismUsageTypeController.
    /// Tests all endpoints, error cases, and edge cases for 100% code coverage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ContactMechanismUsageTypeControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IContactMechanismUsageTypeRepository> _mockContactMechanismUsageTypeRepository;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private ContactMechanismUsageTypeController _contactMechanismUsageTypeController;

        #endregion

        #region Constructor

        public ContactMechanismUsageTypeControllerTests()
        {
            _mockContactMechanismUsageTypeRepository = new Mock<IContactMechanismUsageTypeRepository>();
            _mockUserClaimsAccessor = MockUserClaimsAccessor;

            _contactMechanismUsageTypeController = new ContactMechanismUsageTypeController(
                _mockContactMechanismUsageTypeRepository.Object,
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
            var controller = new ContactMechanismUsageTypeController(
                _mockContactMechanismUsageTypeRepository.Object,
                _mockUserClaimsAccessor.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new ContactMechanismUsageTypeController(null!, _mockUserClaimsAccessor.Object));

            Assert.Equal("contactMechanismUsageTypeRepository", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new ContactMechanismUsageTypeController(_mockContactMechanismUsageTypeRepository.Object, null!));

            Assert.Equal("userClaimsAccessor", exception.ParamName);
        }

        #endregion

        #region ListContactMechanismUsageType Tests - Success Scenarios

        [Fact]
        public async Task ListContactMechanismUsageType_WithDefaultParameter_ReturnsOkWithData()
        {
            // Arrange
            var expectedUsageTypes = new List<ContactMechanismUsageType>
            {
                new ContactMechanismUsageType { ContactMechanismUsageTypeId = 1, Name = "Personal" },
                new ContactMechanismUsageType { ContactMechanismUsageTypeId = 2, Name = "Work" },
                new ContactMechanismUsageType { ContactMechanismUsageTypeId = 3, Name = "AccountRecovery" }
            };

            _mockContactMechanismUsageTypeRepository
                .Setup(x => x.ListContactMechanismUsageType(null))
                .Returns(expectedUsageTypes);

            // Act
            var result = await _contactMechanismUsageTypeController.ListContactMechanismUsageType();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ContactMechanismUsageType, IErrorData>>(okResult.Value);
            Assert.Equal(3, output.list.Count);
        }

        [Fact]
        public async Task ListContactMechanismUsageType_WithSpecificName_ReturnsFilteredData()
        {
            // Arrange
            const string usageTypeName = "Personal";
            var expectedUsageTypes = new List<ContactMechanismUsageType>
            {
                new ContactMechanismUsageType { ContactMechanismUsageTypeId = 1, Name = "Personal" }
            };

            _mockContactMechanismUsageTypeRepository
                .Setup(x => x.ListContactMechanismUsageType(usageTypeName))
                .Returns(expectedUsageTypes);

            // Act
            var result = await _contactMechanismUsageTypeController.ListContactMechanismUsageType(usageTypeName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ContactMechanismUsageType, IErrorData>>(okResult.Value);
            Assert.Single(output.list);
            Assert.Equal("Personal", output.list[0].Name);
        }

        [Fact]
        public async Task ListContactMechanismUsageType_WithSingleResult_ReturnsOkWithData()
        {
            // Arrange
            var expectedUsageTypes = new List<ContactMechanismUsageType>
            {
                new ContactMechanismUsageType { ContactMechanismUsageTypeId = 1, Name = "Work" }
            };

            _mockContactMechanismUsageTypeRepository
                .Setup(x => x.ListContactMechanismUsageType(null))
                .Returns(expectedUsageTypes);

            // Act
            var result = await _contactMechanismUsageTypeController.ListContactMechanismUsageType();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ContactMechanismUsageType, IErrorData>>(okResult.Value);
            Assert.Single(output.list);
        }

        [Fact]
        public async Task ListContactMechanismUsageType_CallsRepositoryWithCorrectParameter()
        {
            // Arrange
            const string usageTypeName = "TestType";

            _mockContactMechanismUsageTypeRepository
                .Setup(x => x.ListContactMechanismUsageType(usageTypeName))
                .Returns(new List<ContactMechanismUsageType> { new ContactMechanismUsageType() });

            // Act
            await _contactMechanismUsageTypeController.ListContactMechanismUsageType(usageTypeName);

            // Assert
            _mockContactMechanismUsageTypeRepository.Verify(
                x => x.ListContactMechanismUsageType(usageTypeName),
                Times.Once);
        }

        #endregion

        #region ListContactMechanismUsageType Tests - No Content Scenarios

        [Fact]
        public async Task ListContactMechanismUsageType_WhenRepositoryReturnsNull_ReturnsNoContent()
        {
            // Arrange
            _mockContactMechanismUsageTypeRepository
                .Setup(x => x.ListContactMechanismUsageType(It.IsAny<string>()))
                .Returns((IList<ContactMechanismUsageType>)null!);

            // Act
            var result = await _contactMechanismUsageTypeController.ListContactMechanismUsageType();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ListContactMechanismUsageType_WhenRepositoryReturnsEmptyList_ReturnsNoContent()
        {
            // Arrange
            _mockContactMechanismUsageTypeRepository
                .Setup(x => x.ListContactMechanismUsageType(It.IsAny<string>()))
                .Returns(new List<ContactMechanismUsageType>());

            // Act
            var result = await _contactMechanismUsageTypeController.ListContactMechanismUsageType();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ListContactMechanismUsageType_WithNonExistentName_ReturnsNoContent()
        {
            // Arrange
            const string nonExistentName = "NonExistentType";

            _mockContactMechanismUsageTypeRepository
                .Setup(x => x.ListContactMechanismUsageType(nonExistentName))
                .Returns(new List<ContactMechanismUsageType>());

            // Act
            var result = await _contactMechanismUsageTypeController.ListContactMechanismUsageType(nonExistentName);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        #endregion

        #region ListContactMechanismUsageType Tests - Various Parameter Values

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Personal")]
        [InlineData("Work")]
        [InlineData("AccountRecovery")]
        [InlineData("Home")]
        public async Task ListContactMechanismUsageType_WithVariousParameterValues_ReturnsOkWithData(string usageTypeName)
        {
            // Arrange
            var expectedUsageTypes = new List<ContactMechanismUsageType>
            {
                new ContactMechanismUsageType { ContactMechanismUsageTypeId = 1, Name = usageTypeName ?? "Default" }
            };

            _mockContactMechanismUsageTypeRepository
                .Setup(x => x.ListContactMechanismUsageType(usageTypeName))
                .Returns(expectedUsageTypes);

            // Act
            var result = await _contactMechanismUsageTypeController.ListContactMechanismUsageType(usageTypeName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task ListContactMechanismUsageType_WithWhitespaceName_PassesToRepository()
        {
            // Arrange
            const string whitespaceName = "   ";
            var expectedUsageTypes = new List<ContactMechanismUsageType>
            {
                new ContactMechanismUsageType { ContactMechanismUsageTypeId = 1, Name = "Test" }
            };

            _mockContactMechanismUsageTypeRepository
                .Setup(x => x.ListContactMechanismUsageType(whitespaceName))
                .Returns(expectedUsageTypes);

            // Act
            var result = await _contactMechanismUsageTypeController.ListContactMechanismUsageType(whitespaceName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockContactMechanismUsageTypeRepository.Verify(
                x => x.ListContactMechanismUsageType(whitespaceName),
                Times.Once);
        }

        [Fact]
        public async Task ListContactMechanismUsageType_WithSpecialCharacters_PassesToRepository()
        {
            // Arrange
            const string specialCharsName = "Type & Special <>";
            var expectedUsageTypes = new List<ContactMechanismUsageType>
            {
                new ContactMechanismUsageType { ContactMechanismUsageTypeId = 1, Name = specialCharsName }
            };

            _mockContactMechanismUsageTypeRepository
                .Setup(x => x.ListContactMechanismUsageType(specialCharsName))
                .Returns(expectedUsageTypes);

            // Act
            var result = await _contactMechanismUsageTypeController.ListContactMechanismUsageType(specialCharsName);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListContactMechanismUsageType_WithVeryLongName_PassesToRepository()
        {
            // Arrange
            var longName = new string('A', 500);
            var expectedUsageTypes = new List<ContactMechanismUsageType>
            {
                new ContactMechanismUsageType { ContactMechanismUsageTypeId = 1, Name = longName }
            };

            _mockContactMechanismUsageTypeRepository
                .Setup(x => x.ListContactMechanismUsageType(longName))
                .Returns(expectedUsageTypes);

            // Act
            var result = await _contactMechanismUsageTypeController.ListContactMechanismUsageType(longName);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region ListContactMechanismUsageType Tests - Full Data Verification

        [Fact]
        public async Task ListContactMechanismUsageType_WithFullUsageTypeDetails_ReturnsCompleteData()
        {
            // Arrange
            var expectedUsageTypes = new List<ContactMechanismUsageType>
            {
                new ContactMechanismUsageType
                {
                    ContactMechanismUsageTypeId = 100,
                    ParentContactMechanismUsageTypeId = 0,
                    Name = "Personal"
                },
                new ContactMechanismUsageType
                {
                    ContactMechanismUsageTypeId = 200,
                    ParentContactMechanismUsageTypeId = 100,
                    Name = "Home"
                }
            };

            _mockContactMechanismUsageTypeRepository
                .Setup(x => x.ListContactMechanismUsageType(null))
                .Returns(expectedUsageTypes);

            // Act
            var result = await _contactMechanismUsageTypeController.ListContactMechanismUsageType();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ContactMechanismUsageType, IErrorData>>(okResult.Value);
            Assert.Equal(2, output.list.Count);

            var firstType = output.list[0];
            Assert.Equal(100, firstType.ContactMechanismUsageTypeId);
            Assert.Equal(0, firstType.ParentContactMechanismUsageTypeId);
            Assert.Equal("Personal", firstType.Name);

            var secondType = output.list[1];
            Assert.Equal(200, secondType.ContactMechanismUsageTypeId);
            Assert.Equal(100, secondType.ParentContactMechanismUsageTypeId);
            Assert.Equal("Home", secondType.Name);
        }

        [Fact]
        public async Task ListContactMechanismUsageType_WithManyUsageTypes_ReturnsAllTypes()
        {
            // Arrange
            var expectedUsageTypes = new List<ContactMechanismUsageType>();
            for (int i = 1; i <= 50; i++)
            {
                expectedUsageTypes.Add(new ContactMechanismUsageType
                {
                    ContactMechanismUsageTypeId = i,
                    Name = $"Type{i}"
                });
            }

            _mockContactMechanismUsageTypeRepository
                .Setup(x => x.ListContactMechanismUsageType(null))
                .Returns(expectedUsageTypes);

            // Act
            var result = await _contactMechanismUsageTypeController.ListContactMechanismUsageType();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ContactMechanismUsageType, IErrorData>>(okResult.Value);
            Assert.Equal(50, output.list.Count);
        }

        #endregion

        #region Concurrent Access Tests

        [Fact]
        public async Task ListContactMechanismUsageType_MultipleConcurrentCalls_AllReturnCorrectResults()
        {
            // Arrange
            var expectedUsageTypes = new List<ContactMechanismUsageType>
            {
                new ContactMechanismUsageType { ContactMechanismUsageTypeId = 1, Name = "Test" }
            };

            _mockContactMechanismUsageTypeRepository
                .Setup(x => x.ListContactMechanismUsageType(It.IsAny<string>()))
                .Returns(expectedUsageTypes);

            var tasks = new List<Task<IActionResult>>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(_contactMechanismUsageTypeController.ListContactMechanismUsageType());
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            foreach (var result in results)
            {
                Assert.IsType<OkObjectResult>(result);
            }
        }

        [Fact]
        public async Task ListContactMechanismUsageType_ConcurrentCallsWithDifferentParams_AllReturnCorrectResults()
        {
            // Arrange
            _mockContactMechanismUsageTypeRepository
                .Setup(x => x.ListContactMechanismUsageType(It.IsAny<string>()))
                .Returns(new List<ContactMechanismUsageType> { new ContactMechanismUsageType() });

            var tasks = new List<Task<IActionResult>>();
            var parameterValues = new[] { null, "Personal", "Work", "Home", "" };

            // Act
            foreach (var param in parameterValues)
            {
                tasks.Add(_contactMechanismUsageTypeController.ListContactMechanismUsageType(param));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            foreach (var result in results)
            {
                Assert.IsType<OkObjectResult>(result);
            }
        }

        #endregion

        #region Repository Interaction Tests

        [Fact]
        public async Task ListContactMechanismUsageType_WithNullParameter_CallsRepositoryWithNull()
        {
            // Arrange
            _mockContactMechanismUsageTypeRepository
                .Setup(x => x.ListContactMechanismUsageType(null))
                .Returns(new List<ContactMechanismUsageType> { new ContactMechanismUsageType() });

            // Act
            await _contactMechanismUsageTypeController.ListContactMechanismUsageType(null);

            // Assert
            _mockContactMechanismUsageTypeRepository.Verify(
                x => x.ListContactMechanismUsageType(null),
                Times.Once);
        }

        [Fact]
        public async Task ListContactMechanismUsageType_RepositoryCalledOnlyOnce()
        {
            // Arrange
            _mockContactMechanismUsageTypeRepository
                .Setup(x => x.ListContactMechanismUsageType(It.IsAny<string>()))
                .Returns(new List<ContactMechanismUsageType> { new ContactMechanismUsageType() });

            // Act
            await _contactMechanismUsageTypeController.ListContactMechanismUsageType("Test");

            // Assert
            _mockContactMechanismUsageTypeRepository.Verify(
                x => x.ListContactMechanismUsageType(It.IsAny<string>()),
                Times.Once);
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _contactMechanismUsageTypeController = null!;
            base.Dispose();
        }

        #endregion
    }
}









