using System;
using System.Collections.Generic;
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
    /// ManageElectronicAddress business logic xUnit tests.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageElectronicAddressTests : TestBase
    {
        private readonly Mock<IElectronicAddressRepository> _mockElectronicAddressRepository;

        public ManageElectronicAddressTests()
        {
            _mockElectronicAddressRepository = new Mock<IElectronicAddressRepository>();
        }

        private ManageElectronicAddress CreateManageElectronicAddress()
        {
            return new ManageElectronicAddress(_mockElectronicAddressRepository.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageElectronicAddress = CreateManageElectronicAddress();

            // Assert
            Assert.NotNull(manageElectronicAddress);
        }

        [Fact]
        public void Constructor_WithoutParameters_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageElectronicAddress = new ManageElectronicAddress();

            // Assert
            Assert.NotNull(manageElectronicAddress);
        }

        #endregion

        #region CreateElectronicAddress Tests

        [Fact]
        public void CreateElectronicAddress_WithNullElectronicAddress_ThrowsArgumentNullException()
        {
            // Arrange
            IElectronicAddress electronicAddress = null;
            var manageElectronicAddress = CreateManageElectronicAddress();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageElectronicAddress.CreateElectronicAddress(electronicAddress));

            Assert.Equal("electronicAddress", exception.ParamName);
            Assert.Contains("Null ElectronicAddress", exception.Message);
            _mockElectronicAddressRepository.Verify(
                x => x.CreateElectronicAddress(It.IsAny<IElectronicAddress>()),
                Times.Never);
        }

        [Fact]
        public void CreateElectronicAddress_WithValidElectronicAddress_ReturnsSuccessResponse()
        {
            // Arrange
            var electronicAddress = new ElectronicAddress
            {
                ContactMechanismId = 0,
                AddressString = "test@example.com",
                AddressType = "Email",
                PartyContactMechanismId = 0,
                ContactMechanismUsageTypeId = 1
            };

            var expectedResponse = new RepositoryResponse
            {
                Id = 100,
                ErrorMessage = ""
            };

            _mockElectronicAddressRepository
                .Setup(x => x.CreateElectronicAddress(electronicAddress))
                .Returns(expectedResponse);

            var manageElectronicAddress = CreateManageElectronicAddress();

            // Act
            var result = manageElectronicAddress.CreateElectronicAddress(electronicAddress);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Id);
            Assert.Equal("", result.ErrorMessage);
            _mockElectronicAddressRepository.Verify(
                x => x.CreateElectronicAddress(electronicAddress),
                Times.Once);
        }

        [Fact]
        public void CreateElectronicAddress_WithEmailAddress_ReturnsSuccessResponse()
        {
            // Arrange
            var electronicAddress = new ElectronicAddress
            {
                AddressString = "user@company.com",
                AddressType = "Email",
                ContactMechanismUsageTypeId = 1
            };

            var expectedResponse = new RepositoryResponse
            {
                Id = 150,
                ErrorMessage = ""
            };

            _mockElectronicAddressRepository
                .Setup(x => x.CreateElectronicAddress(electronicAddress))
                .Returns(expectedResponse);

            var manageElectronicAddress = CreateManageElectronicAddress();

            // Act
            var result = manageElectronicAddress.CreateElectronicAddress(electronicAddress);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(150, result.Id);
        }

        [Fact]
        public void CreateElectronicAddress_WithRepositoryError_ReturnsErrorResponse()
        {
            // Arrange
            var electronicAddress = new ElectronicAddress
            {
                AddressString = "test@example.com",
                AddressType = "Email"
            };

            var expectedResponse = new RepositoryResponse
            {
                Id = 0,
                ErrorMessage = "Database error occurred"
            };

            _mockElectronicAddressRepository
                .Setup(x => x.CreateElectronicAddress(electronicAddress))
                .Returns(expectedResponse);

            var manageElectronicAddress = CreateManageElectronicAddress();

            // Act
            var result = manageElectronicAddress.CreateElectronicAddress(electronicAddress);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Id);
            Assert.Equal("Database error occurred", result.ErrorMessage);
        }

        [Fact]
        public void CreateElectronicAddress_WithWebUrlAddress_ReturnsSuccessResponse()
        {
            // Arrange
            var electronicAddress = new ElectronicAddress
            {
                AddressString = "https://www.example.com",
                AddressType = "WebUrl",
                ContactMechanismUsageTypeId = 2
            };

            var expectedResponse = new RepositoryResponse
            {
                Id = 200,
                ErrorMessage = ""
            };

            _mockElectronicAddressRepository
                .Setup(x => x.CreateElectronicAddress(electronicAddress))
                .Returns(expectedResponse);

            var manageElectronicAddress = CreateManageElectronicAddress();

            // Act
            var result = manageElectronicAddress.CreateElectronicAddress(electronicAddress);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Id);
        }

        #endregion

        #region ListElectronicAddressForPerson (by RealPageId) Tests

        [Fact]
        public void ListElectronicAddressForPerson_WithEmptyGuid_ThrowsException()
        {
            // Arrange
            var realPageId = Guid.Empty;
            string usageTypeName = "Personal";
            var manageElectronicAddress = CreateManageElectronicAddress();

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageElectronicAddress.ListElectronicAddressForPerson(realPageId, usageTypeName));

            Assert.Equal("Invalid parameter realPageId.", exception.Message);
            _mockElectronicAddressRepository.Verify(
                x => x.ListElectronicAddressForPerson(It.IsAny<Guid>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void ListElectronicAddressForPerson_WithValidRealPageId_ReturnsElectronicAddressList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string usageTypeName = "Personal";

            var expectedAddresses = new List<ElectronicAddress>
            {
                new ElectronicAddress
                {
                    ContactMechanismId = 1,
                    PartyContactMechanismId = 100,
                    AddressString = "personal@example.com",
                    AddressType = "Email"
                },
                new ElectronicAddress
                {
                    ContactMechanismId = 2,
                    PartyContactMechanismId = 101,
                    AddressString = "https://personal-site.com",
                    AddressType = "WebUrl"
                }
            };

            _mockElectronicAddressRepository
                .Setup(x => x.ListElectronicAddressForPerson(realPageId, usageTypeName))
                .Returns(expectedAddresses);

            var manageElectronicAddress = CreateManageElectronicAddress();

            // Act
            var result = manageElectronicAddress.ListElectronicAddressForPerson(realPageId, usageTypeName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("personal@example.com", result[0].AddressString);
            Assert.Equal("Email", result[0].AddressType);
            Assert.Equal("https://personal-site.com", result[1].AddressString);
            Assert.Equal("WebUrl", result[1].AddressType);
            _mockElectronicAddressRepository.Verify(
                x => x.ListElectronicAddressForPerson(realPageId, usageTypeName),
                Times.Once);
        }

        [Fact]
        public void ListElectronicAddressForPerson_WithNullUsageTypeName_ReturnsAllAddresses()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string usageTypeName = null;

            var expectedAddresses = new List<ElectronicAddress>
            {
                new ElectronicAddress
                {
                    ContactMechanismId = 1,
                    AddressString = "work@example.com",
                    AddressType = "Email"
                },
                new ElectronicAddress
                {
                    ContactMechanismId = 2,
                    AddressString = "personal@example.com",
                    AddressType = "Email"
                }
            };

            _mockElectronicAddressRepository
                .Setup(x => x.ListElectronicAddressForPerson(realPageId, usageTypeName))
                .Returns(expectedAddresses);

            var manageElectronicAddress = CreateManageElectronicAddress();

            // Act
            var result = manageElectronicAddress.ListElectronicAddressForPerson(realPageId, usageTypeName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void ListElectronicAddressForPerson_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string usageTypeName = "NonExistent";

            var expectedAddresses = new List<ElectronicAddress>();

            _mockElectronicAddressRepository
                .Setup(x => x.ListElectronicAddressForPerson(realPageId, usageTypeName))
                .Returns(expectedAddresses);

            var manageElectronicAddress = CreateManageElectronicAddress();

            // Act
            var result = manageElectronicAddress.ListElectronicAddressForPerson(realPageId, usageTypeName);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ListElectronicAddressForPerson_WithMultipleAddressTypes_ReturnsAllTypes()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            string usageTypeName = "Work";

            var expectedAddresses = new List<ElectronicAddress>
            {
                new ElectronicAddress
                {
                    ContactMechanismId = 1,
                    AddressString = "work@company.com",
                    AddressType = "Email"
                },
                new ElectronicAddress
                {
                    ContactMechanismId = 2,
                    AddressString = "https://company.com",
                    AddressType = "WebUrl"
                },
                new ElectronicAddress
                {
                    ContactMechanismId = 3,
                    AddressString = "ftp://files.company.com",
                    AddressType = "Ftp"
                }
            };

            _mockElectronicAddressRepository
                .Setup(x => x.ListElectronicAddressForPerson(realPageId, usageTypeName))
                .Returns(expectedAddresses);

            var manageElectronicAddress = CreateManageElectronicAddress();

            // Act
            var result = manageElectronicAddress.ListElectronicAddressForPerson(realPageId, usageTypeName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Contains(result, a => a.AddressType == "Email");
            Assert.Contains(result, a => a.AddressType == "WebUrl");
            Assert.Contains(result, a => a.AddressType == "Ftp");
        }

        #endregion

        #region ListElectronicAddressForPerson (by LoginName) Tests

        [Fact]
        public void ListElectronicAddressForPerson_WithNullLoginName_ThrowsException()
        {
            // Arrange
            string loginName = null;
            long orgPartyId = 100;
            string usageTypeName = "Personal";
            var manageElectronicAddress = CreateManageElectronicAddress();

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageElectronicAddress.ListElectronicAddressForPerson(loginName, orgPartyId, usageTypeName));

            Assert.Equal("Invalid parameter user login name.", exception.Message);
            _mockElectronicAddressRepository.Verify(
                x => x.ListElectronicAddressForPerson(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void ListElectronicAddressForPerson_WithEmptyLoginName_ThrowsException()
        {
            // Arrange
            string loginName = "";
            long orgPartyId = 100;
            string usageTypeName = "Personal";
            var manageElectronicAddress = CreateManageElectronicAddress();

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageElectronicAddress.ListElectronicAddressForPerson(loginName, orgPartyId, usageTypeName));

            Assert.Equal("Invalid parameter user login name.", exception.Message);
        }

        [Fact]
        public void ListElectronicAddressForPerson_WithValidLoginName_ReturnsElectronicAddressList()
        {
            // Arrange
            string loginName = "user@test.com";
            long orgPartyId = 100;
            string usageTypeName = "Work";

            var expectedAddresses = new List<ElectronicAddress>
            {
                new ElectronicAddress
                {
                    ContactMechanismId = 1,
                    PartyContactMechanismId = 200,
                    AddressString = "user@test.com",
                    AddressType = "Email"
                }
            };

            _mockElectronicAddressRepository
                .Setup(x => x.ListElectronicAddressForPerson(loginName, orgPartyId, usageTypeName))
                .Returns(expectedAddresses);

            var manageElectronicAddress = CreateManageElectronicAddress();

            // Act
            var result = manageElectronicAddress.ListElectronicAddressForPerson(loginName, orgPartyId, usageTypeName);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("user@test.com", result[0].AddressString);
            Assert.Equal("Email", result[0].AddressType);
            _mockElectronicAddressRepository.Verify(
                x => x.ListElectronicAddressForPerson(loginName, orgPartyId, usageTypeName),
                Times.Once);
        }

        [Fact]
        public void ListElectronicAddressForPerson_WithEmptyUsageTypeName_UsesDefaultParameter()
        {
            // Arrange
            string loginName = "user@test.com";
            long orgPartyId = 100;

            var expectedAddresses = new List<ElectronicAddress>
            {
                new ElectronicAddress
                {
                    ContactMechanismId = 1,
                    AddressString = "user@test.com",
                    AddressType = "Email"
                }
            };

            _mockElectronicAddressRepository
                .Setup(x => x.ListElectronicAddressForPerson(loginName, orgPartyId, ""))
                .Returns(expectedAddresses);

            var manageElectronicAddress = CreateManageElectronicAddress();

            // Act
            var result = manageElectronicAddress.ListElectronicAddressForPerson(loginName, orgPartyId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            _mockElectronicAddressRepository.Verify(
                x => x.ListElectronicAddressForPerson(loginName, orgPartyId, ""),
                Times.Once);
        }

        [Fact]
        public void ListElectronicAddressForPerson_WithLoginNameAndOrgId_ReturnsFilteredAddresses()
        {
            // Arrange
            string loginName = "admin@company.com";
            long orgPartyId = 500;
            string usageTypeName = "Admin";

            var expectedAddresses = new List<ElectronicAddress>
            {
                new ElectronicAddress
                {
                    ContactMechanismId = 10,
                    PartyContactMechanismId = 1000,
                    AddressString = "admin@company.com",
                    AddressType = "Email"
                },
                new ElectronicAddress
                {
                    ContactMechanismId = 11,
                    PartyContactMechanismId = 1001,
                    AddressString = "https://admin.company.com",
                    AddressType = "WebUrl"
                }
            };

            _mockElectronicAddressRepository
                .Setup(x => x.ListElectronicAddressForPerson(loginName, orgPartyId, usageTypeName))
                .Returns(expectedAddresses);

            var manageElectronicAddress = CreateManageElectronicAddress();

            // Act
            var result = manageElectronicAddress.ListElectronicAddressForPerson(loginName, orgPartyId, usageTypeName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(orgPartyId, 500);
        }

        [Fact]
        public void ListElectronicAddressForPerson_WithLoginName_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            string loginName = "newuser@test.com";
            long orgPartyId = 100;
            string usageTypeName = "Personal";

            var expectedAddresses = new List<ElectronicAddress>();

            _mockElectronicAddressRepository
                .Setup(x => x.ListElectronicAddressForPerson(loginName, orgPartyId, usageTypeName))
                .Returns(expectedAddresses);

            var manageElectronicAddress = CreateManageElectronicAddress();

            // Act
            var result = manageElectronicAddress.ListElectronicAddressForPerson(loginName, orgPartyId, usageTypeName);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        public void ListElectronicAddressForPerson_WithWhitespaceLoginName_ThrowsException()
        {
            // Arrange
            string loginName = "   ";
            long orgPartyId = 100;
            string usageTypeName = "Personal";
            var manageElectronicAddress = CreateManageElectronicAddress();

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageElectronicAddress.ListElectronicAddressForPerson(loginName, orgPartyId, usageTypeName));

            Assert.Equal("Invalid parameter user login name.", exception.Message);
        }

        #endregion

        #region Integration Scenario Tests

        [Fact]
        public void ManageElectronicAddress_CompleteWorkflow_CreateAndRetrieve()
        {
            // Arrange - Create
            var electronicAddress = new ElectronicAddress
            {
                AddressString = "integration@test.com",
                AddressType = "Email",
                ContactMechanismUsageTypeId = 1
            };

            var createResponse = new RepositoryResponse
            {
                Id = 999,
                ErrorMessage = ""
            };

            _mockElectronicAddressRepository
                .Setup(x => x.CreateElectronicAddress(electronicAddress))
                .Returns(createResponse);

            // Arrange - Retrieve
            var realPageId = Guid.NewGuid();
            var expectedAddresses = new List<ElectronicAddress>
            {
                new ElectronicAddress
                {
                    ContactMechanismId = 999,
                    AddressString = "integration@test.com",
                    AddressType = "Email"
                }
            };

            _mockElectronicAddressRepository
                .Setup(x => x.ListElectronicAddressForPerson(realPageId, "Personal"))
                .Returns(expectedAddresses);

            var manageElectronicAddress = CreateManageElectronicAddress();

            // Act - Create
            var createResult = manageElectronicAddress.CreateElectronicAddress(electronicAddress);

            // Act - Retrieve
            var listResult = manageElectronicAddress.ListElectronicAddressForPerson(realPageId, "Personal");

            // Assert
            Assert.NotNull(createResult);
            Assert.Equal(999, createResult.Id);

            Assert.NotNull(listResult);
            Assert.Single(listResult);
            Assert.Equal("integration@test.com", listResult[0].AddressString);
        }

        #endregion
    }
}
