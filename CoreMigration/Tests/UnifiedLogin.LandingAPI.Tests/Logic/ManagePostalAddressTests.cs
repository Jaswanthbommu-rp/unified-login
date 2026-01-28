using System;
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
    /// ManagePostalAddress business logic xUnit tests.
    /// Tests for postal address management operations including listing addresses.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManagePostalAddressTests : TestBase
    {
        private readonly Mock<IPostalAddressRepository> _mockPostalAddressRepository;

        public ManagePostalAddressTests()
        {
            _mockPostalAddressRepository = new Mock<IPostalAddressRepository>();
        }

        #region Helper Methods

        private List<PostalAddress> CreateValidPostalAddresses()
        {
            return new List<PostalAddress>
            {
                new PostalAddress
                {
                    PartyContactMechanismId = 1,
                    ContactMechanismId = 100,
                    AddressString = "123 Main St, Dallas, TX 75001",
                    AddressType = "POSTAL",
                    ContactMechanismUsageTypeId = 1,
                    contactMechanismUsageType = new ContactMechanismUsageType
                    {
                        ContactMechanismUsageTypeId = 1,
                        Name = "HOME"
                    }
                },
                new PostalAddress
                {
                    PartyContactMechanismId = 2,
                    ContactMechanismId = 101,
                    AddressString = "456 Oak Ave, Austin, TX 78701",
                    AddressType = "POSTAL",
                    ContactMechanismUsageTypeId = 2,
                    contactMechanismUsageType = new ContactMechanismUsageType
                    {
                        ContactMechanismUsageTypeId = 2,
                        Name = "WORK"
                    }
                }
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNoParameters_InitializesSuccessfully()
        {
            // Arrange & Act
            var managePostalAddress = new ManagePostalAddress();

            // Assert
            Assert.NotNull(managePostalAddress);
        }

        [Fact]
        public void Constructor_WithPostalAddressRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var managePostalAddress = new ManagePostalAddress(_mockPostalAddressRepository.Object);

            // Assert
            Assert.NotNull(managePostalAddress);
        }

        #endregion

        #region ListPostalAddressForPerson Tests

        [Fact]
        public void ListPostalAddressForPerson_WithValidParameters_ReturnsPostalAddressList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string contactMechanismUsageTypeName = "HOME";
            var expectedAddresses = CreateValidPostalAddresses();

            _mockPostalAddressRepository
                .Setup(x => x.ListPostalAddressForPerson(realPageId, contactMechanismUsageTypeName))
                .Returns(expectedAddresses);

            var managePostalAddress = new ManagePostalAddress(_mockPostalAddressRepository.Object);

            // Act
            var result = managePostalAddress.ListPostalAddressForPerson(realPageId, contactMechanismUsageTypeName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("123 Main St, Dallas, TX 75001", result[0].AddressString);
            Assert.Equal("456 Oak Ave, Austin, TX 78701", result[1].AddressString);
            _mockPostalAddressRepository.Verify(x => x.ListPostalAddressForPerson(
                realPageId,
                contactMechanismUsageTypeName), Times.Once);
        }

        [Fact]
        public void ListPostalAddressForPerson_WithEmptyRealPageId_ThrowsException()
        {
            // Arrange
            var realPageId = Guid.Empty;
            string contactMechanismUsageTypeName = "HOME";
            var managePostalAddress = new ManagePostalAddress(_mockPostalAddressRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                managePostalAddress.ListPostalAddressForPerson(realPageId, contactMechanismUsageTypeName));

            Assert.Equal("Invalid parameter realPageId.", exception.Message);
        }

        [Fact]
        public void ListPostalAddressForPerson_WithNullContactMechanismUsageTypeName_ReturnsAddresses()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string contactMechanismUsageTypeName = null;
            var expectedAddresses = CreateValidPostalAddresses();

            _mockPostalAddressRepository
                .Setup(x => x.ListPostalAddressForPerson(realPageId, contactMechanismUsageTypeName))
                .Returns(expectedAddresses);

            var managePostalAddress = new ManagePostalAddress(_mockPostalAddressRepository.Object);

            // Act
            var result = managePostalAddress.ListPostalAddressForPerson(realPageId, contactMechanismUsageTypeName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void ListPostalAddressForPerson_WithEmptyContactMechanismUsageTypeName_ReturnsAddresses()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string contactMechanismUsageTypeName = string.Empty;
            var expectedAddresses = CreateValidPostalAddresses();

            _mockPostalAddressRepository
                .Setup(x => x.ListPostalAddressForPerson(realPageId, contactMechanismUsageTypeName))
                .Returns(expectedAddresses);

            var managePostalAddress = new ManagePostalAddress(_mockPostalAddressRepository.Object);

            // Act
            var result = managePostalAddress.ListPostalAddressForPerson(realPageId, contactMechanismUsageTypeName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void ListPostalAddressForPerson_WithDifferentUsageTypes_ReturnsCorrectAddresses()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string homeUsageType = "HOME";
            string workUsageType = "WORK";
            var homeAddresses = new List<PostalAddress> { CreateValidPostalAddresses()[0] };
            var workAddresses = new List<PostalAddress> { CreateValidPostalAddresses()[1] };

            _mockPostalAddressRepository
                .Setup(x => x.ListPostalAddressForPerson(realPageId, homeUsageType))
                .Returns(homeAddresses);

            _mockPostalAddressRepository
                .Setup(x => x.ListPostalAddressForPerson(realPageId, workUsageType))
                .Returns(workAddresses);

            var managePostalAddress = new ManagePostalAddress(_mockPostalAddressRepository.Object);

            // Act
            var homeResult = managePostalAddress.ListPostalAddressForPerson(realPageId, homeUsageType);
            var workResult = managePostalAddress.ListPostalAddressForPerson(realPageId, workUsageType);

            // Assert
            Assert.Single(homeResult);
            Assert.Single(workResult);
            Assert.Equal("123 Main St, Dallas, TX 75001", homeResult[0].AddressString);
            Assert.Equal("456 Oak Ave, Austin, TX 78701", workResult[0].AddressString);
        }

        [Fact]
        public void ListPostalAddressForPerson_WhenRepositoryReturnsEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string contactMechanismUsageTypeName = "HOME";
            var emptyList = new List<PostalAddress>();

            _mockPostalAddressRepository
                .Setup(x => x.ListPostalAddressForPerson(realPageId, contactMechanismUsageTypeName))
                .Returns(emptyList);

            var managePostalAddress = new ManagePostalAddress(_mockPostalAddressRepository.Object);

            // Act
            var result = managePostalAddress.ListPostalAddressForPerson(realPageId, contactMechanismUsageTypeName);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ListPostalAddressForPerson_WhenRepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string contactMechanismUsageTypeName = "HOME";

            _mockPostalAddressRepository
                .Setup(x => x.ListPostalAddressForPerson(realPageId, contactMechanismUsageTypeName))
                .Returns((IList<PostalAddress>)null);

            var managePostalAddress = new ManagePostalAddress(_mockPostalAddressRepository.Object);

            // Act
            var result = managePostalAddress.ListPostalAddressForPerson(realPageId, contactMechanismUsageTypeName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ListPostalAddressForPerson_WithSingleAddress_ReturnsSingleAddressList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string contactMechanismUsageTypeName = "BILLING";
            var singleAddress = new List<PostalAddress> { CreateValidPostalAddresses()[0] };

            _mockPostalAddressRepository
                .Setup(x => x.ListPostalAddressForPerson(realPageId, contactMechanismUsageTypeName))
                .Returns(singleAddress);

            var managePostalAddress = new ManagePostalAddress(_mockPostalAddressRepository.Object);

            // Act
            var result = managePostalAddress.ListPostalAddressForPerson(realPageId, contactMechanismUsageTypeName);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("123 Main St, Dallas, TX 75001", result[0].AddressString);
        }

        [Fact]
        public void ListPostalAddressForPerson_WithMultipleRealPageIds_ReturnsCorrectAddresses()
        {
            // Arrange
            var realPageId1 = Guid.NewGuid();
            var realPageId2 = Guid.NewGuid();
            string contactMechanismUsageTypeName = "HOME";
            var addresses1 = new List<PostalAddress> { CreateValidPostalAddresses()[0] };
            var addresses2 = new List<PostalAddress> { CreateValidPostalAddresses()[1] };

            _mockPostalAddressRepository
                .Setup(x => x.ListPostalAddressForPerson(realPageId1, contactMechanismUsageTypeName))
                .Returns(addresses1);

            _mockPostalAddressRepository
                .Setup(x => x.ListPostalAddressForPerson(realPageId2, contactMechanismUsageTypeName))
                .Returns(addresses2);

            var managePostalAddress = new ManagePostalAddress(_mockPostalAddressRepository.Object);

            // Act
            var result1 = managePostalAddress.ListPostalAddressForPerson(realPageId1, contactMechanismUsageTypeName);
            var result2 = managePostalAddress.ListPostalAddressForPerson(realPageId2, contactMechanismUsageTypeName);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Single(result1);
            Assert.Single(result2);
            Assert.NotEqual(result1[0].AddressString, result2[0].AddressString);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void ManagePostalAddress_CompleteWorkflow_ListAddressesForDifferentUsageTypes()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var allAddresses = CreateValidPostalAddresses();

            _mockPostalAddressRepository
                .Setup(x => x.ListPostalAddressForPerson(realPageId, "HOME"))
                .Returns(new List<PostalAddress> { allAddresses[0] });

            _mockPostalAddressRepository
                .Setup(x => x.ListPostalAddressForPerson(realPageId, "WORK"))
                .Returns(new List<PostalAddress> { allAddresses[1] });

            _mockPostalAddressRepository
                .Setup(x => x.ListPostalAddressForPerson(realPageId, null))
                .Returns(allAddresses);

            var managePostalAddress = new ManagePostalAddress(_mockPostalAddressRepository.Object);

            // Act
            var homeAddresses = managePostalAddress.ListPostalAddressForPerson(realPageId, "HOME");
            var workAddresses = managePostalAddress.ListPostalAddressForPerson(realPageId, "WORK");
            var allAddressesResult = managePostalAddress.ListPostalAddressForPerson(realPageId, null);

            // Assert
            Assert.Single(homeAddresses);
            Assert.Single(workAddresses);
            Assert.Equal(2, allAddressesResult.Count);
            Assert.Contains("Dallas", homeAddresses[0].AddressString);
            Assert.Contains("Austin", workAddresses[0].AddressString);
        }

        #endregion

        #region Edge Cases and Additional Scenarios

        [Fact]
        public void ListPostalAddressForPerson_WithCaseSensitiveUsageTypeName_CallsRepositoryCorrectly()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string upperCaseType = "HOME";
            string lowerCaseType = "home";
            var expectedAddresses = CreateValidPostalAddresses();

            _mockPostalAddressRepository
                .Setup(x => x.ListPostalAddressForPerson(realPageId, upperCaseType))
                .Returns(expectedAddresses);

            _mockPostalAddressRepository
                .Setup(x => x.ListPostalAddressForPerson(realPageId, lowerCaseType))
                .Returns(new List<PostalAddress>());

            var managePostalAddress = new ManagePostalAddress(_mockPostalAddressRepository.Object);

            // Act
            var upperResult = managePostalAddress.ListPostalAddressForPerson(realPageId, upperCaseType);
            var lowerResult = managePostalAddress.ListPostalAddressForPerson(realPageId, lowerCaseType);

            // Assert
            Assert.Equal(2, upperResult.Count);
            Assert.Empty(lowerResult);
            _mockPostalAddressRepository.Verify(x => x.ListPostalAddressForPerson(
                realPageId,
                upperCaseType), Times.Once);
            _mockPostalAddressRepository.Verify(x => x.ListPostalAddressForPerson(
                realPageId,
                lowerCaseType), Times.Once);
        }

        [Fact]
        public void ListPostalAddressForPerson_WithWhitespaceUsageTypeName_CallsRepositoryCorrectly()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string whitespaceType = "  ";
            var expectedAddresses = CreateValidPostalAddresses();

            _mockPostalAddressRepository
                .Setup(x => x.ListPostalAddressForPerson(realPageId, whitespaceType))
                .Returns(expectedAddresses);

            var managePostalAddress = new ManagePostalAddress(_mockPostalAddressRepository.Object);

            // Act
            var result = managePostalAddress.ListPostalAddressForPerson(realPageId, whitespaceType);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        #endregion
    }
}
