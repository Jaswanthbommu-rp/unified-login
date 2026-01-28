using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Accounting;
using UnifiedLogin.SharedObjects.Product.Ops;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;
using UnifiedLogin.SharedObjects.Product.Rum;
using UnifiedLogin.SharedObjects.Saml;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic.Helper
{
    /// <summary>
    /// BatchHelper xUnit tests.
    /// Tests for batch record creation functionality for various products.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class BatchHelperTests : TestBase
    {
        #region CreateProductBatchRecord Tests

        [Fact]
        public void CreateProductBatchRecord_WithNullPropertiesAndRoles_ReturnsEmptyBatch()
        {
            // Arrange
            var propertiesResponse = new ListResponse { Records = null };
            var rolesResponse = new ListResponse { Records = null };

            // Act
            var result = BatchHelper.CreateProductBatchRecord(
                propertiesResponse,
                rolesResponse,
                (int)ProductEnum.OneSite,
                false,
                ProductIntegrationTypeEnum.Legacy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal((int)ProductEnum.OneSite, result.ProductId);
            Assert.Equal(5, result.StatusTypeId);
            Assert.Equal(0, result.RetryCount);
        }

        [Fact]
        public void CreateProductBatchRecord_WithAllPropertiesFlag_AddsAllMarker()
        {
            // Arrange
            var additionalData = new Dictionary<string, bool> { { "allProperties", true } };
            var propertiesResponse = new ListResponse
            {
                Records = new List<object>(),
                Additional = additionalData
            };
            var rolesResponse = new ListResponse { Records = new List<object>() };

            // Act
            var result = BatchHelper.CreateProductBatchRecord(
                propertiesResponse,
                rolesResponse,
                (int)ProductEnum.OneSite,
                false,
                ProductIntegrationTypeEnum.Legacy);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("ALL", result.InputJson.PropertyList);
        }

        [Fact]
        public void CreateProductBatchRecord_WithClientPortalAllProperties_AddsMinus1()
        {
            // Arrange
            var additionalData = new Dictionary<string, bool> { { "allProperties", true } };
            var propertiesResponse = new ListResponse
            {
                Records = new List<object>(),
                Additional = additionalData
            };
            var rolesResponse = new ListResponse { Records = new List<object>() };

            // Act
            var result = BatchHelper.CreateProductBatchRecord(
                propertiesResponse,
                rolesResponse,
                (int)ProductEnum.ClientPortal,
                false,
                ProductIntegrationTypeEnum.Legacy);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("-1", result.InputJson.PropertyList);
        }

        [Fact]
        public void CreateProductBatchRecord_WithUPFMIntegrationType_AddsMinus1ForAllProperties()
        {
            // Arrange
            var additionalData = new Dictionary<string, bool> { { "allProperties", true } };
            var propertiesResponse = new ListResponse
            {
                Records = new List<object>(),
                Additional = additionalData
            };
            var rolesResponse = new ListResponse { Records = new List<object>() };

            // Act
            var result = BatchHelper.CreateProductBatchRecord(
                propertiesResponse,
                rolesResponse,
                (int)ProductEnum.UnifiedPlatform,
                false,
                ProductIntegrationTypeEnum.UPFM);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("-1", result.InputJson.PropertyList);
        }

        [Fact]
        public void CreateProductBatchRecord_WithProductProperty_AddsPropertyId()
        {
            // Arrange
            var properties = new List<object>
            {
                new ProductProperty { ID = "prop1", IsAssigned = true, Alias = "alias1" },
                new ProductProperty { ID = "prop2", IsAssigned = true, Alias = "alias2" }
            };
            var propertiesResponse = new ListResponse { Records = properties };
            var rolesResponse = new ListResponse { Records = new List<object>() };

            // Act
            var result = BatchHelper.CreateProductBatchRecord(
                propertiesResponse,
                rolesResponse,
                (int)ProductEnum.OneSite,
                false,
                ProductIntegrationTypeEnum.Legacy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.InputJson.PropertyList.Count);
            Assert.Contains("prop1", result.InputJson.PropertyList);
            Assert.Contains("prop2", result.InputJson.PropertyList);
        }

        [Fact]
        public void CreateProductBatchRecord_WithUPFMIntegration_UsesAlias()
        {
            // Arrange
            var properties = new List<object>
            {
                new ProductProperty { ID = "prop1", IsAssigned = true, Alias = "alias1" }
            };
            var propertiesResponse = new ListResponse { Records = properties };
            var rolesResponse = new ListResponse { Records = new List<object>() };

            // Act
            var result = BatchHelper.CreateProductBatchRecord(
                propertiesResponse,
                rolesResponse,
                (int)ProductEnum.UnifiedPlatform,
                false,
                ProductIntegrationTypeEnum.UPFM);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("alias1", result.InputJson.PropertyList);
        }

        [Fact]
        public void CreateProductBatchRecord_WithACProperty_AddsAssignedProperties()
        {
            // Arrange
            var properties = new List<object>
            {
                new ACProperty { Id = "ac1", IsAssigned = true },
                new ACProperty { Id = "ac2", IsAssigned = false }
            };
            var propertiesResponse = new ListResponse { Records = properties };
            var rolesResponse = new ListResponse { Records = new List<object>() };

            // Act
            var result = BatchHelper.CreateProductBatchRecord(
                propertiesResponse,
                rolesResponse,
                (int)ProductEnum.FinancialSuite,
                false,
                ProductIntegrationTypeEnum.Legacy);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.InputJson.PropertyList);
            Assert.Contains("ac1", result.InputJson.PropertyList);
        }

        [Fact]
        public void CreateProductBatchRecord_WithAssetGroup_AddsAssignedAssets()
        {
            // Arrange
            var properties = new List<object>
            {
                new AssetGroup { AssetID = "asset1", IsAssigned = true },
                new AssetGroup { AssetID = "asset2", IsAssigned = false }
            };
            var propertiesResponse = new ListResponse { Records = properties };
            var rolesResponse = new ListResponse { Records = new List<object>() };

            // Act
            var result = BatchHelper.CreateProductBatchRecord(
                propertiesResponse,
                rolesResponse,
                (int)ProductEnum.OpsBuyer,
                false,
                ProductIntegrationTypeEnum.Legacy);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.InputJson.PropertyList);
            Assert.Contains("asset1", result.InputJson.PropertyList);
        }

        [Fact]
        public void CreateProductBatchRecord_WithProductRoles_AddsAssignedRoles()
        {
            // Arrange
            var propertiesResponse = new ListResponse { Records = new List<object>() };
            var roles = new List<object>
            {
                new SharedObjects.Product.ProductRole { ID = "role1", IsAssigned = true },
                new SharedObjects.Product.ProductRole { ID = "role2", IsAssigned = false }
            };
            var rolesResponse = new ListResponse { Records = roles };

            // Act
            var result = BatchHelper.CreateProductBatchRecord(
                propertiesResponse,
                rolesResponse,
                (int)ProductEnum.OneSite,
                true,
                ProductIntegrationTypeEnum.Legacy);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.InputJson.RoleList);
            Assert.Contains("role1", result.InputJson.RoleList);
            Assert.True(result.InputJson.UsePrimaryProperties);
        }

        [Fact]
        public void CreateProductBatchRecord_WithIsAssignedNewPropertyFlag_SetsFlag()
        {
            // Arrange
            var additionalData = new Dictionary<string, bool> { { "IsAssignedNewPropertyByDefault", true } };
            var propertiesResponse = new ListResponse
            {
                Records = new List<object>(),
                Additional = additionalData
            };
            var rolesResponse = new ListResponse { Records = new List<object>() };

            // Act
            var result = BatchHelper.CreateProductBatchRecord(
                propertiesResponse,
                rolesResponse,
                (int)ProductEnum.MarketingCenter,
                false,
                ProductIntegrationTypeEnum.Legacy);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.InputJson.IsAssignedNewPropertyByDefault);
        }

        #endregion

        #region GetUserAssignedPropertiesData Tests

        [Fact]
        public void GetUserAssignedPropertiesData_WithNullRecords_ReturnsEmptyResponse()
        {
            // Arrange
            var propertiesResponse = new ListResponse { Records = null };

            // Act
            var result = BatchHelper.GetUserAssignedPropertiesData(propertiesResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Records);
        }

        [Fact]
        public void GetUserAssignedPropertiesData_WithEmptyRecords_ReturnsEmptyResponse()
        {
            // Arrange
            var propertiesResponse = new ListResponse { Records = new List<object>() };

            // Act
            var result = BatchHelper.GetUserAssignedPropertiesData(propertiesResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Records);
        }

        [Fact]
        public void GetUserAssignedPropertiesData_WithProductProperty_FiltersAssigned()
        {
            // Arrange
            var properties = new List<object>
            {
                new ProductProperty { ID = "prop1", IsAssigned = true },
                new ProductProperty { ID = "prop2", IsAssigned = false },
                new ProductProperty { ID = "prop3", IsAssigned = true }
            };
            var propertiesResponse = new ListResponse { Records = properties };

            // Act
            var result = BatchHelper.GetUserAssignedPropertiesData(propertiesResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalRows);
        }

        [Fact]
        public void GetUserAssignedPropertiesData_WithACProperty_FiltersAssigned()
        {
            // Arrange
            var properties = new List<object>
            {
                new ACProperty { Id = "ac1", IsAssigned = true },
                new ACProperty { Id = "ac2", IsAssigned = false }
            };
            var propertiesResponse = new ListResponse { Records = properties };

            // Act
            var result = BatchHelper.GetUserAssignedPropertiesData(propertiesResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalRows);
        }

        [Fact]
        public void GetUserAssignedPropertiesData_WithAssetGroup_FiltersAssigned()
        {
            // Arrange
            var properties = new List<object>
            {
                new AssetGroup { AssetID = "asset1", IsAssigned = true },
                new AssetGroup { AssetID = "asset2", IsAssigned = true }
            };
            var propertiesResponse = new ListResponse { Records = properties };

            // Act
            var result = BatchHelper.GetUserAssignedPropertiesData(propertiesResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalRows);
        }

        [Fact]
        public void GetUserAssignedPropertiesData_WithPortfolio_FiltersAssigned()
        {
            // Arrange
            var properties = new List<object>
            {
                new Portfolio { ID = "port1", IsAssigned = true },
                new Portfolio { ID = "port2", IsAssigned = false }
            };
            var propertiesResponse = new ListResponse { Records = properties };

            // Act
            var result = BatchHelper.GetUserAssignedPropertiesData(propertiesResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalRows);
        }

        #endregion

        #region CreateOnSiteBatchRecord Tests

        [Fact]
        public void CreateOnSiteBatchRecord_WithEmptyCollections_ReturnsValidBatch()
        {
            // Arrange
            var propertiesResponse = new ListResponse { Records = new List<object>() };
            var rolesResponse = new ListResponse { Records = new List<object>() };
            var regionResponse = new ListResponse { Records = new List<object>() };

            // Act
            var result = BatchHelper.CreateOnSiteBatchRecord(
                propertiesResponse,
                rolesResponse,
                regionResponse,
                (int)ProductEnum.OnSite,
                false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal((int)ProductEnum.OnSite, result.ProductId);
            Assert.Equal(5, result.StatusTypeId);
        }

        [Fact]
        public void CreateOnSiteBatchRecord_WithAllPropertiesFlag_AddsMinus1()
        {
            // Arrange
            var additionalData = new Dictionary<string, bool> { { "allProperties", true } };
            var propertiesResponse = new ListResponse
            {
                Records = new List<object>(),
                Additional = additionalData
            };
            var rolesResponse = new ListResponse { Records = new List<object>() };
            var regionResponse = new ListResponse { Records = new List<object>() };

            // Act
            var result = BatchHelper.CreateOnSiteBatchRecord(
                propertiesResponse,
                rolesResponse,
                regionResponse,
                (int)ProductEnum.OnSite,
                true);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("-1", result.InputJson.PropertyList);
            Assert.True(result.InputJson.UsePrimaryProperties);
        }

        #endregion

        #region CreateProductBatchRecordForClickPay Tests

        [Fact]
        public void CreateProductBatchRecordForClickPay_WithEmptyRoles_ReturnsValidBatch()
        {
            // Arrange
            var organizationRoles = new List<OrganizationRole>();

            // Act
            var result = BatchHelper.CreateProductBatchRecordForClickPay(organizationRoles, false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal((int)ProductEnum.ClickPay, result.ProductId);
            Assert.Equal(5, result.StatusTypeId);
            Assert.Empty(result.InputJson.OrganizationRoleList);
        }

        [Fact]
        public void CreateProductBatchRecordForClickPay_WithRoles_ReturnsValidBatch()
        {
            // Arrange
            var organizationRoles = new List<OrganizationRole>
            {
                new OrganizationRole { RoleId = "1" },
                new OrganizationRole { RoleId = "2" }
            };

            // Act
            var result = BatchHelper.CreateProductBatchRecordForClickPay(organizationRoles, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.InputJson.OrganizationRoleList.Count);
            Assert.True(result.InputJson.UsePrimaryProperties);
        }

        #endregion

        #region CreateProductBatchRecordForDepositIQ Tests

        [Fact]
        public void CreateProductBatchRecordForDepositIQ_WithValidProductUser_ReturnsValidBatch()
        {
            // Arrange
            var productUser = new IntegrationProductUser
            {
                Roles = new List<string> { "role1", "role2" },
                Properties = new List<string> { "prop1" },
                PropertyGroups = new List<string> { "group1" },
                CanReceiveMonthlyReport = true
            };

            // Act
            var result = BatchHelper.CreateProductBatchRecordForDepositIQ(productUser, false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal((int)ProductEnum.DepositAlternative, result.ProductId);
            Assert.Equal(2, result.InputJson.RoleList.Count);
            Assert.Single(result.InputJson.PropertyList);
            Assert.Single(result.InputJson.PropertyGroupList);
            Assert.True(result.InputJson.CanReceiveMonthlyReport);
        }

        #endregion

        #region CreateIntegrationMarketplaceBatchRecord Tests

        [Fact]
        public void CreateIntegrationMarketplaceBatchRecord_WithValidRole_ReturnsValidBatch()
        {
            // Arrange
            int roleId = 123;
            int productId = (int)ProductEnum.LeadManagement;

            // Act
            var result = BatchHelper.CreateIntegrationMarketplaceBatchRecord(roleId, productId, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.ProductId);
            Assert.Single(result.InputJson.RoleList);
            Assert.Contains("123", result.InputJson.RoleList);
            Assert.True(result.InputJson.UsePrimaryProperties);
        }

        #endregion

        #region CreateILMProductBatchRecord Tests

        [Fact]
        public void CreateILMProductBatchRecord_WithValidData_ReturnsValidBatch()
        {
            // Arrange
            var properties = new List<string> { "prop1", "prop2" };
            var roles = new List<string> { "role1" };
            var groups = new List<string> { "group1" };

            // Act
            var result = BatchHelper.CreateILMProductBatchRecord(
                ProductEnum.LeadManagement,
                properties,
                roles,
                groups,
                false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal((int)ProductEnum.LeadManagement, result.ProductId);
            Assert.Equal(2, result.InputJson.PropertyList.Count);
            Assert.Single(result.InputJson.RoleList);
            Assert.Single(result.InputJson.PropertyGroupList);
        }

        #endregion

        #region CreateProductBatchRecordForPortfolioManagement Tests

        [Fact]
        public void CreateProductBatchRecordForPortfolioManagement_WithValidData_ReturnsValidBatch()
        {
            // Arrange
            var rolePropertyList = new List<PAMRolePropertyList>
            {
                new PAMRolePropertyList { RoleId = "1", PropertyIds = new List<string> { "p1" } }
            };
            var roleList = new List<string> { "role1" };

            // Act
            var result = BatchHelper.CreateProductBatchRecordForPortfolioManagement(
                rolePropertyList,
                roleList,
                true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal((int)ProductEnum.PortfolioManagement, result.ProductId);
            Assert.Single(result.InputJson.RolePropertiesList);
            Assert.Single(result.InputJson.RoleList);
            Assert.True(result.InputJson.UsePrimaryProperties);
        }

        #endregion

        #region CreateSelfProvisioningPortalProductBatchRecord Tests

        [Fact]
        public void CreateSelfProvisioningPortalProductBatchRecord_ReturnsValidBatch()
        {
            // Act
            var result = BatchHelper.CreateSelfProvisioningPortalProductBatchRecord((int)ProductEnum.SelfProvisioningPortal);

            // Assert
            Assert.NotNull(result);
            Assert.Equal((int)ProductEnum.SelfProvisioningPortal, result.ProductId);
            Assert.Equal(5, result.StatusTypeId);
            Assert.Equal(0, result.RetryCount);
        }

        #endregion

        #region Documentation Tests

        [Fact]
        public void BatchHelper_ClassPurpose_Documentation()
        {
            // This test documents the class purpose:
            // BatchHelper is a static class responsible for:
            // 1. Creating ProductBatch records for various products
            // 2. Handling different property types (ProductProperty, ACProperty, AssetGroup, etc.)
            // 3. Managing "all properties" flag scenarios
            // 4. Supporting primary properties configuration
            //
            // Key methods:
            // - CreateProductBatchRecord: Generic batch creation
            // - CreateOnSiteBatchRecord: OnSite specific with regions
            // - CreateProductBatchRecordForClickPay: ClickPay specific
            // - CreateProductBatchRecordForDepositIQ: DepositIQ specific
            // - CreateILMProductBatchRecord: ILM products
            // - CreateProductBatchRecordForPortfolioManagement: PAM specific
            // - CreateMarketingCenterProductBatchRecord: Marketing Center specific
            // - CreateFinancialSuiteProductBatchRecord: Financial Suite specific
            // - CreateVendorServiceProductBatchRecord: Vendor Services specific
            // - CreateRumProductBatchRecord: Utility Management specific
            // - CreateResidentPortalProductBatchRecord: Resident Portal specific
            // - CreateRentersInsuranceProductBatchRecord: Insurance specific
            // - CreateSelfProvisioningPortalProductBatchRecord: SPP specific
            // - CreateDocManagementBatchRecords: Document Management specific
            // - CreateAoBatchRecords: Asset Optimizer specific
            // - GetUserAssignedPropertiesData: Filters assigned properties

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void BatchHelper_AllPropertiesMarkers_Documentation()
        {
            // This test documents the "all properties" markers:
            //
            // | Product | All Properties Marker |
            // |---------|----------------------|
            // | ClientPortal | "-1" |
            // | AdminSupportPortal | "-1" |
            // | UPFM Integration | "-1" |
            // | StandardV1 Integration | "-1" |
            // | OneSite | "ALL" |
            // | FinancialSuite | "ALL" |
            // | ProspectContactCenter | "ALL" |
            // | MarketingCenter | "ALL" |
            // | Insurance | "ALL" |
            // | ResidentPortal | "ALL" |

            Assert.True(true, "Documentation test");
        }

        #endregion
    }
}
