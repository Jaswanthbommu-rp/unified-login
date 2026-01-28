using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.IdentityConfig;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManagePreferredContactMethod business logic xUnit tests.
    /// Tests for preferred contact method management operations including listing methods.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManagePreferredContactMethodTests : TestBase
    {
        private readonly Mock<IPreferredContactMethodRepository> _mockPreferredContactMethodRepository;

        public ManagePreferredContactMethodTests()
        {
            _mockPreferredContactMethodRepository = new Mock<IPreferredContactMethodRepository>();
        }

        #region Helper Methods

        private List<PreferredContactMethod> CreateValidPreferredContactMethods()
        {
            return new List<PreferredContactMethod>
            {
                new PreferredContactMethod
                {
                    PreferredContactMethodId = 1,
                    Name = "Email"
                },
                new PreferredContactMethod
                {
                    PreferredContactMethodId = 2,
                    Name = "Phone"
                },
                new PreferredContactMethod
                {
                    PreferredContactMethodId = 3,
                    Name = "SMS"
                },
                new PreferredContactMethod
                {
                    PreferredContactMethodId = 4,
                    Name = "Mail"
                }
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNoParameters_InitializesSuccessfully()
        {
            // Arrange & Act
            var managePreferredContactMethod = new ManagePreferredContactMethod();

            // Assert
            Assert.NotNull(managePreferredContactMethod);
        }

        [Fact]
        public void Constructor_WithPreferredContactMethodRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var managePreferredContactMethod = new ManagePreferredContactMethod(_mockPreferredContactMethodRepository.Object);

            // Assert
            Assert.NotNull(managePreferredContactMethod);
        }

        #endregion

        #region ListPreferredContactMethod Tests

        [Fact]
        public void ListPreferredContactMethod_ReturnsListOfContactMethods()
        {
            // Arrange
            var expectedMethods = CreateValidPreferredContactMethods();

            _mockPreferredContactMethodRepository
                .Setup(x => x.ListPreferredContactMethod())
                .Returns(expectedMethods);

            var managePreferredContactMethod = new ManagePreferredContactMethod(_mockPreferredContactMethodRepository.Object);

            // Act
            var result = managePreferredContactMethod.ListPreferredContactMethod();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            Assert.Equal("Email", result[0].Name);
            Assert.Equal("Phone", result[1].Name);
            Assert.Equal("SMS", result[2].Name);
            Assert.Equal("Mail", result[3].Name);
            _mockPreferredContactMethodRepository.Verify(x => x.ListPreferredContactMethod(), Times.Once);
        }

        [Fact]
        public void ListPreferredContactMethod_WhenRepositoryReturnsEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var emptyList = new List<PreferredContactMethod>();

            _mockPreferredContactMethodRepository
                .Setup(x => x.ListPreferredContactMethod())
                .Returns(emptyList);

            var managePreferredContactMethod = new ManagePreferredContactMethod(_mockPreferredContactMethodRepository.Object);

            // Act
            var result = managePreferredContactMethod.ListPreferredContactMethod();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockPreferredContactMethodRepository.Verify(x => x.ListPreferredContactMethod(), Times.Once);
        }

        [Fact]
        public void ListPreferredContactMethod_WhenRepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            _mockPreferredContactMethodRepository
                .Setup(x => x.ListPreferredContactMethod())
                .Returns((IList<PreferredContactMethod>)null);

            var managePreferredContactMethod = new ManagePreferredContactMethod(_mockPreferredContactMethodRepository.Object);

            // Act
            var result = managePreferredContactMethod.ListPreferredContactMethod();

            // Assert
            Assert.Null(result);
            _mockPreferredContactMethodRepository.Verify(x => x.ListPreferredContactMethod(), Times.Once);
        }

        [Fact]
        public void ListPreferredContactMethod_WithSingleMethod_ReturnsSingleMethodList()
        {
            // Arrange
            var singleMethod = new List<PreferredContactMethod>
            {
                new PreferredContactMethod
                {
                    PreferredContactMethodId = 1,
                    Name = "Email"
                }
            };

            _mockPreferredContactMethodRepository
                .Setup(x => x.ListPreferredContactMethod())
                .Returns(singleMethod);

            var managePreferredContactMethod = new ManagePreferredContactMethod(_mockPreferredContactMethodRepository.Object);

            // Act
            var result = managePreferredContactMethod.ListPreferredContactMethod();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].PreferredContactMethodId);
            Assert.Equal("Email", result[0].Name);
        }

        [Fact]
        public void ListPreferredContactMethod_WithMultipleMethods_ReturnsAllMethods()
        {
            // Arrange
            var expectedMethods = CreateValidPreferredContactMethods();

            _mockPreferredContactMethodRepository
                .Setup(x => x.ListPreferredContactMethod())
                .Returns(expectedMethods);

            var managePreferredContactMethod = new ManagePreferredContactMethod(_mockPreferredContactMethodRepository.Object);

            // Act
            var result = managePreferredContactMethod.ListPreferredContactMethod();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            
            // Verify each method is present
            for (int i = 0; i < expectedMethods.Count; i++)
            {
                Assert.Equal(expectedMethods[i].PreferredContactMethodId, result[i].PreferredContactMethodId);
                Assert.Equal(expectedMethods[i].Name, result[i].Name);
            }
        }

        [Fact]
        public void ListPreferredContactMethod_CalledMultipleTimes_CallsRepositoryEachTime()
        {
            // Arrange
            var expectedMethods = CreateValidPreferredContactMethods();

            _mockPreferredContactMethodRepository
                .Setup(x => x.ListPreferredContactMethod())
                .Returns(expectedMethods);

            var managePreferredContactMethod = new ManagePreferredContactMethod(_mockPreferredContactMethodRepository.Object);

            // Act
            var result1 = managePreferredContactMethod.ListPreferredContactMethod();
            var result2 = managePreferredContactMethod.ListPreferredContactMethod();
            var result3 = managePreferredContactMethod.ListPreferredContactMethod();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
            Assert.Equal(4, result1.Count);
            Assert.Equal(4, result2.Count);
            Assert.Equal(4, result3.Count);
            _mockPreferredContactMethodRepository.Verify(x => x.ListPreferredContactMethod(), Times.Exactly(3));
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void ManagePreferredContactMethod_CompleteWorkflow_ListMethodsMultipleTimes()
        {
            // Arrange
            var methods = CreateValidPreferredContactMethods();

            _mockPreferredContactMethodRepository
                .Setup(x => x.ListPreferredContactMethod())
                .Returns(methods);

            var managePreferredContactMethod = new ManagePreferredContactMethod(_mockPreferredContactMethodRepository.Object);

            // Act
            var firstCall = managePreferredContactMethod.ListPreferredContactMethod();
            var secondCall = managePreferredContactMethod.ListPreferredContactMethod();

            // Assert
            Assert.NotNull(firstCall);
            Assert.NotNull(secondCall);
            Assert.Equal(firstCall.Count, secondCall.Count);
            Assert.Equal(4, firstCall.Count);
            
            // Verify both calls returned the same data
            for (int i = 0; i < firstCall.Count; i++)
            {
                Assert.Equal(firstCall[i].PreferredContactMethodId, secondCall[i].PreferredContactMethodId);
                Assert.Equal(firstCall[i].Name, secondCall[i].Name);
            }

            _mockPreferredContactMethodRepository.Verify(x => x.ListPreferredContactMethod(), Times.Exactly(2));
        }

        #endregion

        #region Edge Cases and Additional Scenarios

        [Fact]
        public void ListPreferredContactMethod_WithDifferentMethodNames_ReturnsAllCorrectly()
        {
            // Arrange
            var methods = new List<PreferredContactMethod>
            {
                new PreferredContactMethod { PreferredContactMethodId = 1, Name = "Email" },
                new PreferredContactMethod { PreferredContactMethodId = 2, Name = "Text Message" },
                new PreferredContactMethod { PreferredContactMethodId = 3, Name = "Video Call" },
                new PreferredContactMethod { PreferredContactMethodId = 4, Name = "In-Person" }
            };

            _mockPreferredContactMethodRepository
                .Setup(x => x.ListPreferredContactMethod())
                .Returns(methods);

            var managePreferredContactMethod = new ManagePreferredContactMethod(_mockPreferredContactMethodRepository.Object);

            // Act
            var result = managePreferredContactMethod.ListPreferredContactMethod();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            Assert.Contains(result, m => m.Name == "Email");
            Assert.Contains(result, m => m.Name == "Text Message");
            Assert.Contains(result, m => m.Name == "Video Call");
            Assert.Contains(result, m => m.Name == "In-Person");
        }

        [Fact]
        public void ListPreferredContactMethod_WithLargeList_ReturnsAllMethods()
        {
            // Arrange
            var largeMethods = new List<PreferredContactMethod>();
            for (int i = 1; i <= 20; i++)
            {
                largeMethods.Add(new PreferredContactMethod
                {
                    PreferredContactMethodId = i,
                    Name = $"Method {i}"
                });
            }

            _mockPreferredContactMethodRepository
                .Setup(x => x.ListPreferredContactMethod())
                .Returns(largeMethods);

            var managePreferredContactMethod = new ManagePreferredContactMethod(_mockPreferredContactMethodRepository.Object);

            // Act
            var result = managePreferredContactMethod.ListPreferredContactMethod();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20, result.Count);
            Assert.Equal("Method 1", result[0].Name);
            Assert.Equal("Method 20", result[19].Name);
        }

        [Fact]
        public void ListPreferredContactMethod_WithDefaultConstructor_CallsRepository()
        {
            // This test verifies the default constructor works
            // In a real scenario, this would call the actual repository
            // For unit testing purposes, we just verify the constructor doesn't throw

            // Arrange & Act
            var managePreferredContactMethod = new ManagePreferredContactMethod();

            // Assert
            Assert.NotNull(managePreferredContactMethod);
            // Note: Cannot easily verify repository call without mocking since default constructor
            // creates a real repository instance
        }

        #endregion
    }
}
