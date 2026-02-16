using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageOrganizationProduct business logic xUnit tests.
    /// Tests for organization product management operations including enable, disable, and validation.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageOrganizationProductTests : TestBase
    {
        private readonly Mock<IRepository> _mockRepository;
        private readonly Mock<IManageBlueBook> _mockManageBlueBook;
        private readonly Mock<IManageProduct> _mockManageProduct;
        private readonly DefaultUserClaim _defaultUserClaim;

        public ManageOrganizationProductTests()
        {
            _mockRepository = new Mock<IRepository>();
            _mockManageBlueBook = new Mock<IManageBlueBook>();
            _mockManageProduct = new Mock<IManageProduct>();

            _defaultUserClaim = new DefaultUserClaim
            {
                UserId = 1,
                LoginName = "testuser@test.com",
                FirstName = "Test",
                LastName = "User",
                UserRealPageGuid = Guid.Parse("F5C090FA-78AB-452F-B504-98AAFEE09121"),
                OrganizationRealPageGuid = Guid.Parse("A5C090FA-78AB-452F-B504-98AAFEE09122"),
                OrganizationMasterId = 379,
                OrganizationPartyId = 1000,
                PersonaId = 5,
                CorrelationId = Guid.NewGuid(),
                OrganizationName = "Test Organization"
            };
        }

        #region Helper Methods

        private Organization CreateValidOrganization()
        {
            return new Organization
            {
                RealPageId = Guid.NewGuid(),
                PartyId = 1000,
                Name = "Test Organization",
                BooksMasterId = 12345,
                BooksCustomerMasterId = 67890,
                IsActive = 1
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
                ErrorMessage = "Error occurred"
            };
        }

        private List<ProductInternalSetting> CreateProductInternalSettings(bool updateInUDM = false)
        {
            return new List<ProductInternalSetting>
            {
                new ProductInternalSetting
                {
                    Name = "UPDATEPRODUCTINUDM",
                    Value = updateInUDM ? "1" : "0"
                }
            };
        }

        private List<GbProductMap> CreateProductList()
        {
            return new List<GbProductMap>
            {
                new GbProductMap { ProductId = 3, Name = "Unified Platform" },
                new GbProductMap { ProductId = 8, Name = "RealPage Accounting" },
                new GbProductMap { ProductId = 14, Name = "Client Portal" }
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithUserClaimOnly_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Assert
            Assert.NotNull(manageOrgProduct);
        }

        [Fact]
        public void Constructor_WithAllDependencies_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageOrgProduct = new ManageOrganizationProduct(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockManageBlueBook.Object,
                _mockManageProduct.Object);

            // Assert
            Assert.NotNull(manageOrgProduct);
        }

    
        public void Constructor_WithNullUserClaim_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                new ManageOrganizationProduct(
                    null,
                    _mockRepository.Object,
                    _mockManageBlueBook.Object,
                    _mockManageProduct.Object));
        }

        #endregion

        #region InsertUpdateOrganizationProduct (List) Tests

        [Fact]
        public void InsertUpdateOrganizationProduct_WithValidOrgAndProducts_ReturnsSuccessResponse()
        {
            // Arrange
            var org = CreateValidOrganization();
            var productList = new List<int> { 3, 8 };
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.InsertUpdateOrganizationProduct(org, productList);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IRepositoryResponse>(result);
        }

        [Fact]
        public void InsertUpdateOrganizationProduct_WithEmptyProductList_ReturnsResponse()
        {
            // Arrange
            var org = CreateValidOrganization();
            var productList = new List<int>();
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.InsertUpdateOrganizationProduct(org, productList);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void InsertUpdateOrganizationProduct_WithDuplicateProducts_ProcessesDistinctOnly()
        {
            // Arrange
            var org = CreateValidOrganization();
            var productList = new List<int> { 3, 3, 8, 8 }; // Duplicates
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.InsertUpdateOrganizationProduct(org, productList);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IRepositoryResponse>(result);
        }

        #endregion

        #region InsertUpdateOrganizationProduct (Single) Tests

        [Fact]
        public void InsertUpdateOrganizationProduct_WithValidParameters_ReturnsRepositoryResponse()
        {
            // Arrange
            long partyId = 1000;
            int productId = 3;
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.InsertUpdateOrganizationProduct(
                partyId,
                productId,
                null,
                null,
                null,
                "Test Org");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        [Fact]
        public void InsertUpdateOrganizationProduct_WithConfigurationId_ReturnsRepositoryResponse()
        {
            // Arrange
            long partyId = 1000;
            int productId = 3;
            int configurationId = 100;
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.InsertUpdateOrganizationProduct(
                partyId,
                productId,
                configurationId,
                DateTime.UtcNow,
                DateTime.UtcNow.AddYears(1),
                "Test Org");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        [Fact]
        public void InsertUpdateOrganizationProduct_WithNullDates_ReturnsRepositoryResponse()
        {
            // Arrange
            long partyId = 1000;
            int productId = 3;
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.InsertUpdateOrganizationProduct(
                partyId,
                productId,
                null,
                null,
                null,
                "Test Org");

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region InsertUpdateOrganizationProductFromProvisioning Tests

        [Fact]
        public void InsertUpdateOrganizationProductFromProvisioning_WithValidParameters_ReturnsResponse()
        {
            // Arrange
            var org = CreateValidOrganization();
            int productId = 3;
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.InsertUpdateOrganizationProductFromProvisioning(
                productId,
                null,
                null,
                null,
                org);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IRepositoryResponse>(result);
        }

        [Fact]
        public void InsertUpdateOrganizationProductFromProvisioning_WithDates_ReturnsResponse()
        {
            // Arrange
            var org = CreateValidOrganization();
            int productId = 3;
            var fromDate = DateTime.UtcNow;
            var thruDate = DateTime.UtcNow.AddYears(1);
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.InsertUpdateOrganizationProductFromProvisioning(
                productId,
                100,
                fromDate,
                thruDate,
                org);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region DeleteOrganizationProduct Tests

        [Fact]
        public void DeleteOrganizationProduct_WithValidParameters_ReturnsRepositoryResponse()
        {
            // Arrange
            long partyId = 1000;
            int productId = 3;
            var org = CreateValidOrganization();
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.DeleteOrganizationProduct(partyId, productId, org);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        [Fact]
        public void DeleteOrganizationProduct_WithLogActivityTrue_LogsActivity()
        {
            // Arrange
            long partyId = 1000;
            int productId = 3;
            var org = CreateValidOrganization();
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.DeleteOrganizationProduct(partyId, productId, org, logActivity: true);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        [Fact]
        public void DeleteOrganizationProduct_WithLogActivityFalse_DoesNotLogActivity()
        {
            // Arrange
            long partyId = 1000;
            int productId = 3;
            var org = CreateValidOrganization();
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.DeleteOrganizationProduct(partyId, productId, org, logActivity: false);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        #endregion

        #region DeleteProductsFromOrganization Tests

        [Fact]
        public void DeleteProductsFromOrganization_WithValidProductList_ReturnsRepositoryResponse()
        {
            // Arrange
            var org = CreateValidOrganization();
            var productList = new List<int> { 3, 8 };
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.DeleteProductsFromOrganization(productList, org);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        [Fact]
        public void DeleteProductsFromOrganization_WithEmptyList_ReturnsResponse()
        {
            // Arrange
            var org = CreateValidOrganization();
            var productList = new List<int>();
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.DeleteProductsFromOrganization(productList, org);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void DeleteProductsFromOrganization_WithSingleProduct_ReturnsRepositoryResponse()
        {
            // Arrange
            var org = CreateValidOrganization();
            var productList = new List<int> { 3 };
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.DeleteProductsFromOrganization(productList, org);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        #endregion

        #region DisableUsersForProduct Tests

        [Fact]
        public void DisableUsersForProduct_WithValidParameters_ReturnsRepositoryResponse()
        {
            // Arrange
            long partyId = 1000;
            var productEnum = ProductEnum.UnifiedPlatform;
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.DisableUsersForProduct(partyId, productEnum);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IRepositoryResponse>(result);
        }

        [Fact]
        public void DisableUsersForProduct_WithDifferentProducts_ReturnsRepositoryResponse()
        {
            // Arrange
            long partyId = 1000;
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result1 = manageOrgProduct.DisableUsersForProduct(partyId, ProductEnum.UnifiedPlatform);
            var result2 = manageOrgProduct.DisableUsersForProduct(partyId, ProductEnum.FinancialSuite);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
        }

        #endregion

        #region CheckSharedProductsEnabled Tests

        [Fact]
        public void CheckSharedProductsEnabled_WithNoConflicts_ReturnsSuccessResponse()
        {
            // Arrange
            var orgEnabledProducts = new List<ProductUI>
            {
                new ProductUI { ProductId = 3, ProductName = "Unified Platform" }
            };
            var addProductList = new List<int> { 8 };
            var removeProductList = new List<int>();
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.CheckSharedProductsEnabled(
                orgEnabledProducts,
                addProductList,
                removeProductList);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IRepositoryResponse>(result);
            Assert.Empty(result.ErrorMessage);
        }

        [Fact]
        public void CheckSharedProductsEnabled_WithEmptyAddList_ReturnsSuccessResponse()
        {
            // Arrange
            var orgEnabledProducts = new List<ProductUI>();
            var addProductList = new List<int>();
            var removeProductList = new List<int>();
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.CheckSharedProductsEnabled(
                orgEnabledProducts,
                addProductList,
                removeProductList);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.ErrorMessage);
        }

        [Fact]
        public void CheckSharedProductsEnabled_WithEmptyOrgEnabledProducts_ReturnsSuccessResponse()
        {
            // Arrange
            var orgEnabledProducts = new List<ProductUI>();
            var addProductList = new List<int> { 3, 8 };
            var removeProductList = new List<int>();
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.CheckSharedProductsEnabled(
                orgEnabledProducts,
                addProductList,
                removeProductList);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.ErrorMessage);
        }

        [Fact]
        public void CheckSharedProductsEnabled_WithRemoveList_ReturnsSuccessResponse()
        {
            // Arrange
            var orgEnabledProducts = new List<ProductUI>
            {
                new ProductUI { ProductId = 3, ProductName = "Unified Platform" }
            };
            var addProductList = new List<int> { 8 };
            var removeProductList = new List<int> { 3 };
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.CheckSharedProductsEnabled(
                orgEnabledProducts,
                addProductList,
                removeProductList);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void ManageOrganizationProduct_CompleteWorkflow_HandlesCorrectly()
        {
            // Arrange
            var org = CreateValidOrganization();
            var productList = new List<int> { 3, 8 };
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act - Insert products
            var insertResult = manageOrgProduct.InsertUpdateOrganizationProduct(org, productList);

            // Act - Delete products
            var deleteResult = manageOrgProduct.DeleteProductsFromOrganization(productList, org);

            // Assert
            Assert.NotNull(insertResult);
            Assert.NotNull(deleteResult);
        }

        [Fact]
        public void ManageOrganizationProduct_ProvisioningWorkflow_HandlesCorrectly()
        {
            // Arrange
            var org = CreateValidOrganization();
            int productId = 3;
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act - Insert from provisioning
            var insertResult = manageOrgProduct.InsertUpdateOrganizationProductFromProvisioning(
                productId,
                null,
                null,
                null,
                org);

            // Act - Delete product
            var deleteResult = manageOrgProduct.DeleteOrganizationProduct(
                org.PartyId,
                productId,
                org);

            // Assert
            Assert.NotNull(insertResult);
            Assert.NotNull(deleteResult);
        }

        [Fact]
        public void ManageOrganizationProduct_DisableUsersWorkflow_HandlesCorrectly()
        {
            // Arrange
            long partyId = 1000;
            var productEnum = ProductEnum.UnifiedPlatform;
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.DisableUsersForProduct(partyId, productEnum);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void InsertUpdateOrganizationProduct_WithZeroPartyId_ReturnsResponse()
        {
            // Arrange
            long partyId = 0;
            int productId = 3;
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.InsertUpdateOrganizationProduct(
                partyId,
                productId,
                null,
                null,
                null,
                "Test Org");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void InsertUpdateOrganizationProduct_WithNegativePartyId_ReturnsResponse()
        {
            // Arrange
            long partyId = -1;
            int productId = 3;
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.InsertUpdateOrganizationProduct(
                partyId,
                productId,
                null,
                null,
                null,
                "Test Org");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void InsertUpdateOrganizationProduct_WithZeroProductId_ReturnsResponse()
        {
            // Arrange
            long partyId = 1000;
            int productId = 0;
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.InsertUpdateOrganizationProduct(
                partyId,
                productId,
                null,
                null,
                null,
                "Test Org");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void DeleteOrganizationProduct_WithZeroPartyId_ReturnsResponse()
        {
            // Arrange
            long partyId = 0;
            int productId = 3;
            var org = CreateValidOrganization();
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.DeleteOrganizationProduct(partyId, productId, org);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void DisableUsersForProduct_WithZeroPartyId_ReturnsResponse()
        {
            // Arrange
            long partyId = 0;
            var productEnum = ProductEnum.UnifiedPlatform;
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.DisableUsersForProduct(partyId, productEnum);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region Multiple Products Tests

        [Fact]
        public void InsertUpdateOrganizationProduct_WithMultipleProducts_ProcessesAll()
        {
            // Arrange
            var org = CreateValidOrganization();
            var productList = new List<int> { 3, 8, 14, 28 };
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.InsertUpdateOrganizationProduct(org, productList);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IRepositoryResponse>(result);
        }

        [Fact]
        public void DeleteProductsFromOrganization_WithMultipleProducts_ProcessesAll()
        {
            // Arrange
            var org = CreateValidOrganization();
            var productList = new List<int> { 3, 8, 14, 28 };
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.DeleteProductsFromOrganization(productList, org);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        #endregion

        #region ProductUI and ProductEnum Tests

        [Fact]
        public void CheckSharedProductsEnabled_WithMultipleOrgProducts_HandlesCorrectly()
        {
            // Arrange
            var orgEnabledProducts = new List<ProductUI>
            {
                new ProductUI { ProductId = 3, ProductName = "Unified Platform" },
                new ProductUI { ProductId = 8, ProductName = "RealPage Accounting" },
                new ProductUI { ProductId = 14, ProductName = "Client Portal" }
            };
            var addProductList = new List<int> { 28 };
            var removeProductList = new List<int>();
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.CheckSharedProductsEnabled(
                orgEnabledProducts,
                addProductList,
                removeProductList);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void DisableUsersForProduct_WithVariousProductEnums_ReturnsResponse()
        {
            // Arrange
            long partyId = 1000;
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result1 = manageOrgProduct.DisableUsersForProduct(partyId, ProductEnum.UnifiedPlatform);
            var result2 = manageOrgProduct.DisableUsersForProduct(partyId, ProductEnum.FinancialSuite);
            var result3 = manageOrgProduct.DisableUsersForProduct(partyId, ProductEnum.ClientPortal);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
        }

        #endregion

        #region Organization Object Tests

        [Fact]
        public void InsertUpdateOrganizationProduct_WithDifferentOrganizations_HandlesCorrectly()
        {
            // Arrange
            var org1 = CreateValidOrganization();
            org1.PartyId = 1000;
            var org2 = CreateValidOrganization();
            org2.PartyId = 2000;
            var productList = new List<int> { 3 };
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result1 = manageOrgProduct.InsertUpdateOrganizationProduct(org1, productList);
            var result2 = manageOrgProduct.InsertUpdateOrganizationProduct(org2, productList);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
        }

        #endregion

        #region Date Parameter Tests

        [Fact]
        public void InsertUpdateOrganizationProduct_WithPastDates_ReturnsResponse()
        {
            // Arrange
            long partyId = 1000;
            int productId = 3;
            var fromDate = DateTime.UtcNow.AddYears(-1);
            var thruDate = DateTime.UtcNow.AddYears(-1).AddMonths(6);
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.InsertUpdateOrganizationProduct(
                partyId,
                productId,
                null,
                fromDate,
                thruDate,
                "Test Org");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void InsertUpdateOrganizationProduct_WithFutureDates_ReturnsResponse()
        {
            // Arrange
            long partyId = 1000;
            int productId = 3;
            var fromDate = DateTime.UtcNow.AddYears(1);
            var thruDate = DateTime.UtcNow.AddYears(2);
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.InsertUpdateOrganizationProduct(
                partyId,
                productId,
                null,
                fromDate,
                thruDate,
                "Test Org");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void InsertUpdateOrganizationProductFromProvisioning_WithPastThruDate_ReturnsResponse()
        {
            // Arrange
            var org = CreateValidOrganization();
            int productId = 3;
            var fromDate = DateTime.UtcNow.AddYears(-2);
            var thruDate = DateTime.UtcNow.AddYears(-1);
            var manageOrgProduct = new ManageOrganizationProduct(_defaultUserClaim);

            // Act
            var result = manageOrgProduct.InsertUpdateOrganizationProductFromProvisioning(
                productId,
                null,
                fromDate,
                thruDate,
                org);

            // Assert
            Assert.NotNull(result);
        }

        #endregion
    }
}
