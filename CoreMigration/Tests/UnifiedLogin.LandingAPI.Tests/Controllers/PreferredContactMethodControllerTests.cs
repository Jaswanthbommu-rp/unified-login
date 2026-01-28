using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    /// Comprehensive unit tests for PreferredContactMethodController.
    /// Tests all endpoints, error cases, and edge cases for 100% code coverage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PreferredContactMethodControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IPreferredContactMethodRepository> _mockPreferredContactMethodRepository;
        private PreferredContactMethodController _preferredContactMethodController;

        #endregion

        #region Constructor

        public PreferredContactMethodControllerTests()
        {
            _mockPreferredContactMethodRepository = new Mock<IPreferredContactMethodRepository>();

            _preferredContactMethodController = new PreferredContactMethodController(
                _mockPreferredContactMethodRepository.Object
            )
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependency_CreatesInstance()
        {
            // Act
            var controller = new PreferredContactMethodController(
                _mockPreferredContactMethodRepository.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullRepository_CreatesInstance()
        {
            // Note: Controller doesn't have null checks, so this documents current behavior
            // Act
            var controller = new PreferredContactMethodController(null!);

            // Assert
            Assert.NotNull(controller);
        }

        #endregion

        #region ListPreferredContactMethod Tests - Success

        [Fact]
        public async Task ListPreferredContactMethod_WithData_ReturnsOkResult()
        {
            // Arrange
            var preferredContactMethodList = new List<PreferredContactMethod>
            {
                new PreferredContactMethod { PreferredContactMethodId = 1, Name = "Email" },
                new PreferredContactMethod { PreferredContactMethodId = 2, Name = "Phone" }
            };

            _mockPreferredContactMethodRepository
                .Setup(x => x.ListPreferredContactMethod())
                .Returns(preferredContactMethodList);

            // Act
            var result = await _preferredContactMethodController.ListPreferredContactMethod();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<PreferredContactMethod, IErrorData>>(okResult.Value);
            Assert.Equal(2, output.list.Count);
        }

        [Fact]
        public async Task ListPreferredContactMethod_WithSingleItem_ReturnsOkResult()
        {
            // Arrange
            var preferredContactMethodList = new List<PreferredContactMethod>
            {
                new PreferredContactMethod { PreferredContactMethodId = 1, Name = "SMS" }
            };

            _mockPreferredContactMethodRepository
                .Setup(x => x.ListPreferredContactMethod())
                .Returns(preferredContactMethodList);

            // Act
            var result = await _preferredContactMethodController.ListPreferredContactMethod();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<PreferredContactMethod, IErrorData>>(okResult.Value);
            Assert.Single(output.list);
            Assert.Equal("SMS", output.list[0].Name);
        }

        [Fact]
        public async Task ListPreferredContactMethod_CallsRepositoryOnce()
        {
            // Arrange
            var preferredContactMethodList = new List<PreferredContactMethod>
            {
                new PreferredContactMethod { PreferredContactMethodId = 1, Name = "Email" }
            };

            _mockPreferredContactMethodRepository
                .Setup(x => x.ListPreferredContactMethod())
                .Returns(preferredContactMethodList);

            // Act
            await _preferredContactMethodController.ListPreferredContactMethod();

            // Assert
            _mockPreferredContactMethodRepository.Verify(
                x => x.ListPreferredContactMethod(),
                Times.Once);
        }

        #endregion

        #region ListPreferredContactMethod Tests - NoContent

        [Fact]
        public async Task ListPreferredContactMethod_WithEmptyList_ReturnsNoContent()
        {
            // Arrange
            _mockPreferredContactMethodRepository
                .Setup(x => x.ListPreferredContactMethod())
                .Returns(new List<PreferredContactMethod>());

            // Act
            var result = await _preferredContactMethodController.ListPreferredContactMethod();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ListPreferredContactMethod_WithNullList_ReturnsNoContent()
        {
            // Arrange
            _mockPreferredContactMethodRepository
                .Setup(x => x.ListPreferredContactMethod())
                .Returns((IList<PreferredContactMethod>)null!);

            // Act
            var result = await _preferredContactMethodController.ListPreferredContactMethod();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task ListPreferredContactMethod_WithManyItems_ReturnsOkResult()
        {
            // Arrange
            var preferredContactMethodList = new List<PreferredContactMethod>();
            for (int i = 1; i <= 100; i++)
            {
                preferredContactMethodList.Add(new PreferredContactMethod
                {
                    PreferredContactMethodId = i,
                    Name = $"Method{i}"
                });
            }

            _mockPreferredContactMethodRepository
                .Setup(x => x.ListPreferredContactMethod())
                .Returns(preferredContactMethodList);

            // Act
            var result = await _preferredContactMethodController.ListPreferredContactMethod();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<PreferredContactMethod, IErrorData>>(okResult.Value);
            Assert.Equal(100, output.list.Count);
        }

        [Fact]
        public async Task ListPreferredContactMethod_WithNullName_ReturnsOkResult()
        {
            // Arrange
            var preferredContactMethodList = new List<PreferredContactMethod>
            {
                new PreferredContactMethod { PreferredContactMethodId = 1, Name = null! }
            };

            _mockPreferredContactMethodRepository
                .Setup(x => x.ListPreferredContactMethod())
                .Returns(preferredContactMethodList);

            // Act
            var result = await _preferredContactMethodController.ListPreferredContactMethod();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<PreferredContactMethod, IErrorData>>(okResult.Value);
            Assert.Single(output.list);
            Assert.Null(output.list[0].Name);
        }

        #endregion

        #region Concurrent Access Tests

        [Fact]
        public async Task ListPreferredContactMethod_MultipleConcurrentCalls_AllComplete()
        {
            // Arrange
            var preferredContactMethodList = new List<PreferredContactMethod>
            {
                new PreferredContactMethod { PreferredContactMethodId = 1, Name = "Email" }
            };

            _mockPreferredContactMethodRepository
                .Setup(x => x.ListPreferredContactMethod())
                .Returns(preferredContactMethodList);

            var tasks = new List<Task<IActionResult>>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(_preferredContactMethodController.ListPreferredContactMethod());
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
            _preferredContactMethodController = null!;
            base.Dispose();
        }

        #endregion
    }
}





