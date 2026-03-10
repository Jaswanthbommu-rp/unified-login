using Moq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest
{
    [ExcludeFromCodeCoverage]
    public class CompanyAddressInsertionTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitofWork;
        private readonly Mock<IRepository> _mockRepository;
        private readonly long _partyId =1234;

        public CompanyAddressInsertionTests()
        {
            _mockUnitofWork = new Mock<IUnitOfWork>();
            _mockRepository = new Mock<IRepository>();
        }

        public long Get_Id()
        {
            return _partyId;
        }

        [Fact]
        public void InsertCompanyAddress_WithValidAddress_ReturnsSuccess()
        {
            //Arrange
            var _testRealPageId = Guid.NewGuid();
            var companyAddress = new CompanyInstanceAddress
            {
                Address = "123 Main Street",
                City = "Springfield",
                State = "IL",
                Country = "USA",
                PostalCode = "62701"
            };

            var expectedResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };
            _mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_InsertCompanyAddress, It.IsAny<object>()))
                .Returns(expectedResponse);
            _mockRepository.Setup(m => m.UnitOfWork).Returns(_mockUnitofWork.Object);

            //Act
            var organizationRepository = new OrganizationRepository(_mockRepository.Object);
            var result = organizationRepository.InsertCompanyAddress(_partyId, companyAddress);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Empty(result.ErrorMessage);
        }

        [Fact]
        public void InsertCompanyAddress_WithNullAddress_ReturnsErrorResponse()
        {
            //Arrange
            var organizationRepository = new OrganizationRepository(_mockRepository.Object);

            //Act
            var result = organizationRepository.InsertCompanyAddress(_partyId, null);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Id);
            Assert.Equal("Company address is null.", result.ErrorMessage);
        }

        [Fact]
        public void InsertCompanyAddress_WithAllFields_InsertsSuccessfully()
        {
            //Arrange
            var companyAddress = new CompanyInstanceAddress
            {
                Address = "456 Oak Avenue",
                City = "Chicago",
                State = "IL",
                Country = "USA",
                PostalCode = "60601",
                County = "Cook"
            };

            var expectedResponse = new RepositoryResponse { Id = 2, ErrorMessage = "" };
            _mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_InsertCompanyAddress, It.IsAny<object>()))
                .Returns(expectedResponse);
            _mockRepository.Setup(m => m.UnitOfWork).Returns(_mockUnitofWork.Object);

            //Act
            var organizationRepository = new OrganizationRepository(_mockRepository.Object);
            var result = organizationRepository.InsertCompanyAddress(_partyId, companyAddress);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Id);
            _mockRepository.Verify(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_InsertCompanyAddress, It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void InsertCompanyAddress_WithDatabaseError_ReturnsErrorResponse()
        {
            //Arrange
            var companyAddress = new CompanyInstanceAddress
            {
                Address = "789 Elm Street",
                City = "Naperville",
                State = "IL",
                Country = "USA",
                PostalCode = "60540"
            };

            var errorResponse = new RepositoryResponse { Id = 0, ErrorMessage = "Database error occurred", RealPageId = Guid.Empty };
            _mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_InsertCompanyAddress, It.IsAny<object>()))
                .Returns(errorResponse);
            _mockRepository.Setup(m => m.UnitOfWork).Returns(_mockUnitofWork.Object);

            //Act
            var organizationRepository = new OrganizationRepository(_mockRepository.Object);
            var result = organizationRepository.InsertCompanyAddress(_partyId, companyAddress);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Id);
            Assert.Equal("Database error occurred", result.ErrorMessage);
        }
    }
}
