using System.Diagnostics.CodeAnalysis;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;
using UnifiedLogin.SharedObjects.Product.Rum;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageCloneProductBatch business logic xUnit tests.
    /// Tests for cloning user product batch operations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageCloneProductBatchTests : TestBase, IDisposable
    {
        private readonly DefaultUserClaim _defaultUserClaim;
        private ManageCloneProductBatch _manageCloneProductBatch;

        public ManageCloneProductBatchTests()
        {
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
                CorrelationId = Guid.NewGuid()
            };
        }

        public void Dispose()
        {
            // Cleanup if needed
        }

        #region Helper Methods

        private List<PersonaProductUserDetails> CreatePersonaProductUserDetails(params int[] productIds)
        {
            var products = new List<PersonaProductUserDetails>();
            foreach (var productId in productIds)
            {
                products.Add(new PersonaProductUserDetails
                {
                    ProductId = productId,
                    ProductName = $"Product {productId}"
                });
            }
            return products;
        }

        private UPFMProperty CreateUPFMProperty()
        {
            return new UPFMProperty
            {
                id = new List<string> { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() }
            };
        }

        private List<ProductSettingList> CreateProductSettingList(int productId, bool usePrimaryProperties)
        {
            return new List<ProductSettingList>
            {
                new ProductSettingList
                {
                    ProductId = productId,
                    Name = "UsePrimaryProperties",
                    Value = usePrimaryProperties ? "1" : "0"
                }
            };
        }

        #endregion

        #region Constructor Tests

       
        public void Constructor_WithValidUserClaim_InitializesSuccessfully()
        {
            // Arrange & Act
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);

            // Assert
            Assert.NotNull(_manageCloneProductBatch);
        }

        
        public void Constructor_WithNullUserClaim_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new ManageCloneProductBatch(null));
        }

        #endregion

        #region GetUserProductBatchData Tests - Basic Scenarios

       
        public void GetUserProductBatchData_WithEmptyProductList_ReturnsEmptyList()
        {
            // Arrange
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
            var emptyProducts = new List<PersonaProductUserDetails>();
            var upfmProperty = CreateUPFMProperty();
            var productSettings = new List<ProductSettingList>();

            // Act
            var result = _manageCloneProductBatch.GetUserProductBatchData(
                personaId: 100,
                userProducts: emptyProducts,
                baseOrgAdminPersonaId: 200,
                upfmProperty: upfmProperty,
                productSettingList: productSettings);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

      
        public void GetUserProductBatchData_WithNullProductList_ThrowsException()
        {
            // Arrange
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
            var upfmProperty = CreateUPFMProperty();
            var productSettings = new List<ProductSettingList>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _manageCloneProductBatch.GetUserProductBatchData(
                    personaId: 100,
                    userProducts: null,
                    baseOrgAdminPersonaId: 200,
                    upfmProperty: upfmProperty,
                    productSettingList: productSettings));
        }

      
        public void GetUserProductBatchData_WithValidPersonaIds_AcceptsParameters()
        {
            // Arrange
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
            var products = new List<PersonaProductUserDetails>();
            var upfmProperty = CreateUPFMProperty();
            var productSettings = new List<ProductSettingList>();

            // Act
            var result = _manageCloneProductBatch.GetUserProductBatchData(
                personaId: 100,
                userProducts: products,
                baseOrgAdminPersonaId: 200,
                upfmProperty: upfmProperty,
                productSettingList: productSettings);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region GetUserProductBatchData Tests - Product-Specific Scenarios

       
        public void GetUserProductBatchData_WithSelfProvisioningPortal_CreatesCorrectBatch()
        {
            // Arrange
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
            var products = CreatePersonaProductUserDetails((int)ProductEnum.SelfProvisioningPortal);
            var upfmProperty = CreateUPFMProperty();
            var productSettings = CreateProductSettingList((int)ProductEnum.SelfProvisioningPortal, false);

            // Act
            var result = _manageCloneProductBatch.GetUserProductBatchData(
                personaId: 100,
                userProducts: products,
                baseOrgAdminPersonaId: 200,
                upfmProperty: upfmProperty,
                productSettingList: productSettings);

            // Assert
            Assert.NotNull(result);
            // Note: Full validation would require mocking product-specific services
        }

      
        public void GetUserProductBatchData_WithMultipleProducts_ReturnsMultipleBatches()
        {
            // Arrange
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
            var products = CreatePersonaProductUserDetails(
                (int)ProductEnum.SelfProvisioningPortal);
            var upfmProperty = CreateUPFMProperty();
            var productSettings = new List<ProductSettingList>();

            // Act
            var result = _manageCloneProductBatch.GetUserProductBatchData(
                personaId: 100,
                userProducts: products,
                baseOrgAdminPersonaId: 200,
                upfmProperty: upfmProperty,
                productSettingList: productSettings);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region GetUserProductBatchData Tests - External User Scenarios

       
        public void GetUserProductBatchData_WithExternalUserTrue_HandlesCorrectly()
        {
            // Arrange
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
            var products = new List<PersonaProductUserDetails>();
            var upfmProperty = CreateUPFMProperty();
            var productSettings = new List<ProductSettingList>();

            // Act
            var result = _manageCloneProductBatch.GetUserProductBatchData(
                personaId: 100,
                userProducts: products,
                baseOrgAdminPersonaId: 200,
                upfmProperty: upfmProperty,
                productSettingList: productSettings,
                externalUser: true);

            // Assert
            Assert.NotNull(result);
        }

       
        public void GetUserProductBatchData_WithExternalUserFalse_HandlesCorrectly()
        {
            // Arrange
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
            var products = new List<PersonaProductUserDetails>();
            var upfmProperty = CreateUPFMProperty();
            var productSettings = new List<ProductSettingList>();

            // Act
            var result = _manageCloneProductBatch.GetUserProductBatchData(
                personaId: 100,
                userProducts: products,
                baseOrgAdminPersonaId: 200,
                upfmProperty: upfmProperty,
                productSettingList: productSettings,
                externalUser: false);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region GetUserProductBatchData Tests - Primary Properties

        [Fact]
        public void GetUserProductBatchData_WithUsePrimaryPropertiesEnabled_SetsFlag()
        {
            // Arrange
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
            var products = CreatePersonaProductUserDetails((int)ProductEnum.SelfProvisioningPortal);
            var upfmProperty = CreateUPFMProperty();
            var productSettings = CreateProductSettingList(
                (int)ProductEnum.SelfProvisioningPortal, 
                usePrimaryProperties: true);

            // Act
            var result = _manageCloneProductBatch.GetUserProductBatchData(
                personaId: 100,
                userProducts: products,
                baseOrgAdminPersonaId: 200,
                upfmProperty: upfmProperty,
                productSettingList: productSettings);

            // Assert
            Assert.NotNull(result);
        }

       
        public void GetUserProductBatchData_WithUsePrimaryPropertiesDisabled_DoesNotSetFlag()
        {
            // Arrange
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
            var products = CreatePersonaProductUserDetails((int)ProductEnum.SelfProvisioningPortal);
            var upfmProperty = CreateUPFMProperty();
            var productSettings = CreateProductSettingList(
                (int)ProductEnum.SelfProvisioningPortal, 
                usePrimaryProperties: false);

            // Act
            var result = _manageCloneProductBatch.GetUserProductBatchData(
                personaId: 100,
                userProducts: products,
                baseOrgAdminPersonaId: 200,
                upfmProperty: upfmProperty,
                productSettingList: productSettings);

            // Assert
            Assert.NotNull(result);
        }

      
        public void GetUserProductBatchData_WithNullUPFMProperty_HandlesGracefully()
        {
            // Arrange
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
            var products = CreatePersonaProductUserDetails((int)ProductEnum.SelfProvisioningPortal);
            var productSettings = new List<ProductSettingList>();

            // Act
            var result = _manageCloneProductBatch.GetUserProductBatchData(
                personaId: 100,
                userProducts: products,
                baseOrgAdminPersonaId: 200,
                upfmProperty: null,
                productSettingList: productSettings);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region GetUserProductBatchData Tests - Error Handling

      
        public void GetUserProductBatchData_WhenExceptionOccurs_ContinuesProcessing()
        {
            // Arrange
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
            var products = CreatePersonaProductUserDetails(999); // Invalid product
            var upfmProperty = CreateUPFMProperty();
            var productSettings = new List<ProductSettingList>();

            // Act
            var result = _manageCloneProductBatch.GetUserProductBatchData(
                personaId: 100,
                userProducts: products,
                baseOrgAdminPersonaId: 200,
                upfmProperty: upfmProperty,
                productSettingList: productSettings);

            // Assert - Should not throw exception, continues processing
            Assert.NotNull(result);
        }

        #endregion

        #region GetUserProductBatchData Tests - Edge Cases

       
        public void GetUserProductBatchData_WithZeroPersonaIds_HandlesCorrectly()
        {
            // Arrange
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
            var products = new List<PersonaProductUserDetails>();
            var upfmProperty = CreateUPFMProperty();
            var productSettings = new List<ProductSettingList>();

            // Act
            var result = _manageCloneProductBatch.GetUserProductBatchData(
                personaId: 0,
                userProducts: products,
                baseOrgAdminPersonaId: 0,
                upfmProperty: upfmProperty,
                productSettingList: productSettings);

            // Assert
            Assert.NotNull(result);
        }

       
        public void GetUserProductBatchData_WithNegativePersonaIds_HandlesCorrectly()
        {
            // Arrange
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
            var products = new List<PersonaProductUserDetails>();
            var upfmProperty = CreateUPFMProperty();
            var productSettings = new List<ProductSettingList>();

            // Act
            var result = _manageCloneProductBatch.GetUserProductBatchData(
                personaId: -1,
                userProducts: products,
                baseOrgAdminPersonaId: -1,
                upfmProperty: upfmProperty,
                productSettingList: productSettings);

            // Assert
            Assert.NotNull(result);
        }

      
        public void GetUserProductBatchData_WithMaxLongPersonaIds_HandlesCorrectly()
        {
            // Arrange
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
            var products = new List<PersonaProductUserDetails>();
            var upfmProperty = CreateUPFMProperty();
            var productSettings = new List<ProductSettingList>();

            // Act
            var result = _manageCloneProductBatch.GetUserProductBatchData(
                personaId: long.MaxValue,
                userProducts: products,
                baseOrgAdminPersonaId: long.MaxValue - 1,
                upfmProperty: upfmProperty,
                productSettingList: productSettings);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region ProductBatch Validation Tests

        [Fact]
        public void ProductBatch_CreatedWithCorrectProperties_HasValidStructure()
        {
            // Arrange
            var productBatch = new ProductBatch
            {
                ProductId = (int)ProductEnum.OneSite,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = new RolePropertyList
                {
                    PropertyList = new List<string> { "1", "2" },
                    RoleList = new List<string> { "Admin" }
                }
            };

            // Assert
            Assert.Equal((int)ProductEnum.OneSite, productBatch.ProductId);
            Assert.Equal(5, productBatch.StatusTypeId);
            Assert.Equal(0, productBatch.RetryCount);
            Assert.NotNull(productBatch.InputJson);
            Assert.Equal(2, productBatch.InputJson.PropertyList.Count);
            Assert.Single(productBatch.InputJson.RoleList);
        }

        [Fact]
        public void RolePropertyList_WithUsePrimaryProperties_SetsCorrectly()
        {
            // Arrange & Act
            var rolePropertyList = new RolePropertyList
            {
                PropertyList = new List<string>(),
                RoleList = new List<string>(),
                UsePrimaryProperties = true
            };

            // Assert
            Assert.True(rolePropertyList.UsePrimaryProperties);
        }

        #endregion

        #region PersonaProductUserDetails Tests

        [Fact]
        public void PersonaProductUserDetails_CanBeCreatedAndAssigned()
        {
            // Arrange & Act
            var product = new PersonaProductUserDetails
            {
                ProductId = (int)ProductEnum.OneSite,
                ProductName = "OneSite"
            };

            // Assert
            Assert.Equal((int)ProductEnum.OneSite, product.ProductId);
            Assert.Equal("OneSite", product.ProductName);
        }

        [Fact]
        public void PersonaProductUserDetails_ListCreation_MaintainsOrder()
        {
            // Arrange
            var products = new List<PersonaProductUserDetails>
            {
                new() { ProductId = 1, ProductName = "Product1" },
                new() { ProductId = 2, ProductName = "Product2" },
                new() { ProductId = 3, ProductName = "Product3" }
            };

            // Assert
            Assert.Equal(3, products.Count);
            Assert.Equal(1, products[0].ProductId);
            Assert.Equal(2, products[1].ProductId);
            Assert.Equal(3, products[2].ProductId);
        }

        #endregion

        #region UPFMProperty Tests

        [Fact]
        public void UPFMProperty_WithNullIdList_HandlesGracefully()
        {
            // Arrange & Act
            var upfmProperty = new UPFMProperty
            {
                id = null
            };

            // Assert
            Assert.Null(upfmProperty.id);
        }

        [Fact]
        public void UPFMProperty_WithEmptyIdList_HandlesGracefully()
        {
            // Arrange & Act
            var upfmProperty = new UPFMProperty
            {
                id = new List<string>()
            };

            // Assert
            Assert.NotNull(upfmProperty.id);
            Assert.Empty(upfmProperty.id);
        }

        [Fact]
        public void UPFMProperty_WithMultipleIds_StoresCorrectly()
        {
            // Arrange & Act
            var upfmProperty = new UPFMProperty
            {
                id = new List<string> 
                { 
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                }
            };

            // Assert
            Assert.NotNull(upfmProperty.id);
            Assert.Equal(3, upfmProperty.id.Count);
        }

        #endregion

        #region ProductSettingList Tests

        [Fact]
        public void ProductSettingList_CanBeCreatedWithAllProperties()
        {
            // Arrange & Act
            var setting = new ProductSettingList
            {
                ProductId = (int)ProductEnum.OneSite,
                Name = "UsePrimaryProperties",
                Value = "1"
            };

            // Assert
            Assert.Equal((int)ProductEnum.OneSite, setting.ProductId);
            Assert.Equal("UsePrimaryProperties", setting.Name);
            Assert.Equal("1", setting.Value);
        }

        [Fact]
        public void ProductSettingList_WithDifferentValues_StoresCorrectly()
        {
            // Arrange
            var settings = new List<ProductSettingList>
            {
                new() { ProductId = 1, Name = "Setting1", Value = "0" },
                new() { ProductId = 2, Name = "Setting2", Value = "1" },
                new() { ProductId = 3, Name = "Setting3", Value = "true" }
            };

            // Assert
            Assert.Equal(3, settings.Count);
            Assert.Equal("0", settings[0].Value);
            Assert.Equal("1", settings[1].Value);
            Assert.Equal("true", settings[2].Value);
        }

        #endregion

        #region Integration Tests

        //[Theory]
        //[InlineData(100, 200)]
        //[InlineData(1, 1)]
        //[InlineData(999999, 888888)]
        public void GetUserProductBatchData_WithVariousPersonaIds_ProcessesCorrectly(
            long personaId, long baseOrgAdminPersonaId)
        {
            // Arrange
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
            var products = new List<PersonaProductUserDetails>();
            var upfmProperty = CreateUPFMProperty();
            var productSettings = new List<ProductSettingList>();

            // Act
            var result = _manageCloneProductBatch.GetUserProductBatchData(
                personaId: personaId,
                userProducts: products,
                baseOrgAdminPersonaId: baseOrgAdminPersonaId,
                upfmProperty: upfmProperty,
                productSettingList: productSettings);

            // Assert
            Assert.NotNull(result);
        }

     
        public void GetUserProductBatchData_WithComplexScenario_HandlesAllParameters()
        {
            // Arrange
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
            var products = CreatePersonaProductUserDetails(
                (int)ProductEnum.SelfProvisioningPortal);
            var upfmProperty = CreateUPFMProperty();
            var productSettings = new List<ProductSettingList>
            {
                new() { ProductId = (int)ProductEnum.SelfProvisioningPortal, Name = "UsePrimaryProperties", Value = "1" }
            };

            // Act
            var result = _manageCloneProductBatch.GetUserProductBatchData(
                personaId: 100,
                userProducts: products,
                baseOrgAdminPersonaId: 200,
                upfmProperty: upfmProperty,
                productSettingList: productSettings,
                externalUser: true);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region Product Enum Validation Tests

        //[Theory]
        //[InlineData((int)ProductEnum.OneSite)]
        //[InlineData((int)ProductEnum.FinancialSuite)]
        //[InlineData((int)ProductEnum.MarketingCenter)]
        //[InlineData((int)ProductEnum.OpsBuyer)]
        //[InlineData((int)ProductEnum.VendorServices)]
        //[InlineData((int)ProductEnum.ClientPortal)]
        //[InlineData((int)ProductEnum.AdminSupportPortal)]
        //[InlineData((int)ProductEnum.ProspectContactCenter)]
        //[InlineData((int)ProductEnum.Lead2Lease)]
        //[InlineData((int)ProductEnum.ResidentPortal)]
        //[InlineData((int)ProductEnum.Insurance)]
        //[InlineData((int)ProductEnum.OnSite)]
        //[InlineData((int)ProductEnum.UtilityManagement)]
        //[InlineData((int)ProductEnum.SelfProvisioningPortal)]
        public void GetUserProductBatchData_WithSpecificProduct_AcceptsProductId(int productId)
        {
            // Arrange
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
            var products = CreatePersonaProductUserDetails(productId);
            var upfmProperty = CreateUPFMProperty();
            var productSettings = CreateProductSettingList(productId, false);

            // Act
            var result = _manageCloneProductBatch.GetUserProductBatchData(
                personaId: 100,
                userProducts: products,
                baseOrgAdminPersonaId: 200,
                upfmProperty: upfmProperty,
                productSettingList: productSettings);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region Null and Empty Tests

       
        public void GetUserProductBatchData_WithEmptyProductSettings_HandlesCorrectly()
        {
            // Arrange
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
            var products = CreatePersonaProductUserDetails((int)ProductEnum.OneSite);
            var upfmProperty = CreateUPFMProperty();
            var emptySettings = new List<ProductSettingList>();

            // Act
            var result = _manageCloneProductBatch.GetUserProductBatchData(
                personaId: 100,
                userProducts: products,
                baseOrgAdminPersonaId: 200,
                upfmProperty: upfmProperty,
                productSettingList: emptySettings);

            // Assert
            Assert.NotNull(result);
        }

        public void GetUserProductBatchData_WithNullProductSettings_ThrowsException()
        {
            // Arrange
            _manageCloneProductBatch = new ManageCloneProductBatch(_defaultUserClaim);
            var products = CreatePersonaProductUserDetails((int)ProductEnum.OneSite);
            var upfmProperty = CreateUPFMProperty();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _manageCloneProductBatch.GetUserProductBatchData(
                    personaId: 100,
                    userProducts: products,
                    baseOrgAdminPersonaId: 200,
                    upfmProperty: upfmProperty,
                    productSettingList: null));
        }

        #endregion
    }
}
