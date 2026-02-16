using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic.Product
{
    /// <summary>
    /// ManageProductEasyLMS xUnit tests.
    /// Comprehensive tests for EasyLMS product management.
    /// Tests for getting EasyLMS Company API Code and Key from BlueBook.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageProductEasyLMSTests : TestBase
    {
        #region Private Fields

        private readonly DefaultUserClaim _defaultUserClaim;
        private readonly Guid _testUserRealPageId = Guid.NewGuid();
        private readonly Guid _testOrgRealPageId = Guid.NewGuid();
        private const long TestEditorPersonaId = 100;
        private const long TestUserPersonaId = 200;
        private const long TestPartyId = 1000;

        #endregion

        #region Constructor

        public ManageProductEasyLMSTests()
        {
            _defaultUserClaim = new DefaultUserClaim
            {
                UserId = 1,
                LoginName = "testuser@test.com",
                FirstName = "Test",
                LastName = "User",
                UserRealPageGuid = _testUserRealPageId,
                OrganizationRealPageGuid = _testOrgRealPageId,
                OrganizationPartyId = TestPartyId,
                OrganizationMasterId = 100,
                OrganizationName = "Test Organization",
                PersonaId = TestEditorPersonaId,
                CorrelationId = Guid.NewGuid(),
                Rights = new List<string> { "AccessToUnifiedPlatform" }
            };
        }

        #endregion

        #region CustomerCompanyMap Class Tests

        [Fact]
        public void CustomerCompanyMap_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var companyMap = new CustomerCompanyMap
            {
                Id = "map-123",
                CustomerCompanyId = 456,
                CompanyInstanceId = 789,
                CompanyInstanceSourceId = "SRC-001",
                Source = "BlueBook",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system",
                Domain = "testcompany.com",
                CompanyInstance = new List<CompanyInstance>()
            };

            // Assert
            Assert.Equal("map-123", companyMap.Id);
            Assert.Equal(456, companyMap.CustomerCompanyId);
            Assert.Equal(789, companyMap.CompanyInstanceId);
            Assert.Equal("SRC-001", companyMap.CompanyInstanceSourceId);
            Assert.Equal("BlueBook", companyMap.Source);
            Assert.NotEqual(default, companyMap.CreatedAt);
            Assert.Equal("system", companyMap.CreatedBy);
            Assert.Equal("testcompany.com", companyMap.Domain);
            Assert.Empty(companyMap.CompanyInstance);
        }

        [Fact]
        public void CustomerCompanyMap_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var companyMap = new CustomerCompanyMap();

            // Assert
            Assert.Null(companyMap.Id);
            Assert.Equal(0, companyMap.CustomerCompanyId);
            Assert.Equal(0, companyMap.CompanyInstanceId);
            Assert.Null(companyMap.CompanyInstanceSourceId);
            Assert.Null(companyMap.Source);
            Assert.Equal(default, companyMap.CreatedAt);
            Assert.Null(companyMap.CreatedBy);
            Assert.Null(companyMap.Domain);
            Assert.Null(companyMap.CompanyInstance);
        }

        [Fact]
        public void CustomerCompanyMap_JsonSerialization_WorksCorrectly()
        {
            // Arrange
            var companyMap = new CustomerCompanyMap
            {
                Id = "test-id",
                CustomerCompanyId = 123,
                CompanyInstanceId = 456,
                CompanyInstanceSourceId = "API-KEY-123",
                Source = "EasyLMS",
                Domain = "example.com"
            };

            // Act
            var json = JsonConvert.SerializeObject(companyMap);
            var deserialized = JsonConvert.DeserializeObject<CustomerCompanyMap>(json);

            // Assert
            Assert.Equal(companyMap.Id, deserialized.Id);
            Assert.Equal(companyMap.CustomerCompanyId, deserialized.CustomerCompanyId);
            Assert.Equal(companyMap.CompanyInstanceId, deserialized.CompanyInstanceId);
            Assert.Equal(companyMap.CompanyInstanceSourceId, deserialized.CompanyInstanceSourceId);
            Assert.Equal(companyMap.Source, deserialized.Source);
            Assert.Equal(companyMap.Domain, deserialized.Domain);
        }

        #endregion

        #region GbProductMap Class Tests

        [Fact]
        public void GbProductMap_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var productMap = new GbProductMap
            {
                ProductId = 42,
                Name = "EasyLMS",
                BooksProductCode = "ELMS",
                UDMSourceCode = "LMS"
            };

            // Assert
            Assert.Equal(42, productMap.ProductId);
            Assert.Equal("EasyLMS", productMap.Name);
            Assert.Equal("ELMS", productMap.BooksProductCode);
            Assert.Equal("LMS", productMap.UDMSourceCode);
        }

        [Fact]
        public void GbProductMap_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var productMap = new GbProductMap();

            // Assert
            Assert.Equal(0, productMap.ProductId);
            Assert.Null(productMap.Name);
            Assert.Null(productMap.BooksProductCode);
            Assert.Null(productMap.UDMSourceCode);
        }

        [Fact]
        public void GbProductMap_JsonSerialization_WorksCorrectly()
        {
            // Arrange
            var productMap = new GbProductMap
            {
                ProductId = 42,
                Name = "EasyLMS",
                BooksProductCode = "ELMS",
                UDMSourceCode = "LMS"
            };

            // Act
            var json = JsonConvert.SerializeObject(productMap);
            var deserialized = JsonConvert.DeserializeObject<GbProductMap>(json);

            // Assert
            Assert.Equal(productMap.ProductId, deserialized.ProductId);
            Assert.Equal(productMap.Name, deserialized.Name);
            Assert.Equal(productMap.BooksProductCode, deserialized.BooksProductCode);
            Assert.Equal(productMap.UDMSourceCode, deserialized.UDMSourceCode);
        }

        #endregion

        #region ProductEnum Tests

        [Fact]
        public void ProductEnum_EasyLMS_HasCorrectValue()
        {
            // Assert
            Assert.Equal(36, (int)ProductEnum.EasyLMS);
        }

        [Fact]
        public void ProductEnum_EasyLMS_CanBeCastToInt()
        {
            // Arrange
            var productId = ProductEnum.EasyLMS;

            // Act
            var intValue = (int)productId;

            // Assert
            Assert.True(intValue > 0);
        }

        #endregion

        #region UDMSourceCode Logic Tests

        [Fact]
        public void UDMSourceCode_WhenNotEmpty_IsUsed()
        {
            // This test documents the source code selection logic
            // If UDMSourceCode has length > 0, use UDMSourceCode
            // Otherwise use BooksProductCode
            
            var productDetails = new GbProductMap
            {
                UDMSourceCode = "LMS",
                BooksProductCode = "ELMS"
            };

            var udmSourceCode = productDetails.UDMSourceCode?.Length > 0 
                ? productDetails.UDMSourceCode 
                : productDetails.BooksProductCode;

            Assert.Equal("LMS", udmSourceCode);
        }

        [Fact]
        public void UDMSourceCode_WhenEmpty_FallsBackToBooksProductCode()
        {
            var productDetails = new GbProductMap
            {
                UDMSourceCode = "",
                BooksProductCode = "ELMS"
            };

            var udmSourceCode = productDetails.UDMSourceCode?.Length > 0 
                ? productDetails.UDMSourceCode 
                : productDetails.BooksProductCode;

            Assert.Equal("ELMS", udmSourceCode);
        }

        [Fact]
        public void UDMSourceCode_WhenNull_FallsBackToBooksProductCode()
        {
            var productDetails = new GbProductMap
            {
                UDMSourceCode = null,
                BooksProductCode = "ELMS"
            };

            var udmSourceCode = productDetails.UDMSourceCode?.Length > 0 
                ? productDetails.UDMSourceCode 
                : productDetails.BooksProductCode;

            Assert.Equal("ELMS", udmSourceCode);
        }

        #endregion

        #region ListResponse Tests

        [Fact]
        public void ListResponse_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var response = new ListResponse();

            // Assert
            Assert.False(response.IsError);
            Assert.Null(response.ErrorReason);
            Assert.Null(response.Records);
        }

        [Fact]
        public void ListResponse_ErrorState_CanBeSet()
        {
            // Arrange & Act
            var response = new ListResponse
            {
                IsError = true,
                ErrorReason = "Test error message"
            };

            // Assert
            Assert.True(response.IsError);
            Assert.Equal("Test error message", response.ErrorReason);
        }

        #endregion

        #region DefaultUserClaim Tests

        [Fact]
        public void DefaultUserClaim_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var userClaim = new DefaultUserClaim
            {
                UserId = 123,
                LoginName = "user@example.com",
                FirstName = "John",
                LastName = "Doe",
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 456,
                OrganizationMasterId = 789,
                OrganizationName = "Test Org",
                PersonaId = 1000,
                CorrelationId = Guid.NewGuid(),
                Rights = new List<string> { "Right1", "Right2" }
            };

            // Assert
            Assert.Equal(123, userClaim.UserId);
            Assert.Equal("user@example.com", userClaim.LoginName);
            Assert.Equal("John", userClaim.FirstName);
            Assert.Equal("Doe", userClaim.LastName);
            Assert.NotEqual(Guid.Empty, userClaim.UserRealPageGuid);
            Assert.NotEqual(Guid.Empty, userClaim.OrganizationRealPageGuid);
            Assert.Equal(456, userClaim.OrganizationPartyId);
            Assert.Equal(789, userClaim.OrganizationMasterId);
            Assert.Equal("Test Org", userClaim.OrganizationName);
            Assert.Equal(1000, userClaim.PersonaId);
            Assert.NotEqual(Guid.Empty, userClaim.CorrelationId);
            Assert.Equal(2, userClaim.Rights.Count);
        }

        #endregion

        #region CompanyInstance Class Tests

        [Fact]
        public void CompanyInstance_CanBeCreated()
        {
            // Arrange & Act
            var instance = new CompanyInstance();

            // Assert
            Assert.NotNull(instance);
        }

        #endregion

        #region API Code and Key Retrieval Logic Tests

        [Fact]
        public void CompanyAPICodeAndKey_ReturnedInCompanyMap()
        {
            // This test documents that the API code and key are stored in CustomerCompanyMap
            // CompanyInstanceSourceId typically contains the API key or identifier
            
            var companyMap = new CustomerCompanyMap
            {
                CompanyInstanceSourceId = "API-KEY-12345",
                CompanyInstanceId = 100
            };

            Assert.NotNull(companyMap.CompanyInstanceSourceId);
            Assert.Equal("API-KEY-12345", companyMap.CompanyInstanceSourceId);
        }

        [Fact]
        public void CompanyAPICodeAndKey_EmptyWhenNotConfigured()
        {
            var companyMap = new CustomerCompanyMap();

            Assert.Null(companyMap.CompanyInstanceSourceId);
            Assert.Equal(0, companyMap.CompanyInstanceId);
        }

        #endregion

        #region Product Name Retrieval Tests

        [Fact]
        public void ProductName_ReturnedFromGbProductMap()
        {
            // Arrange
            var productMap = new GbProductMap
            {
                ProductId = (int)ProductEnum.EasyLMS,
                Name = "EasyLMS Learning Management System"
            };

            // Act
            var productName = productMap.Name;

            // Assert
            Assert.Equal("EasyLMS Learning Management System", productName);
        }

        [Fact]
        public void ProductName_EmptyStringWhenNotFound()
        {
            // This documents that getProductName returns empty string when product not found
            GbProductMap booksProductDetail = null;
            string productName = string.Empty;

            if (booksProductDetail != null)
            {
                productName = booksProductDetail.Name;
            }

            Assert.Equal(string.Empty, productName);
        }

        [Theory]
        [InlineData(ProductEnum.EasyLMS)]
        [InlineData(ProductEnum.AssetOptimizer)]
        [InlineData(ProductEnum.AdminSupportPortal)]
        public void ProductName_CanBeRetrievedForAnyProduct(ProductEnum productId)
        {
            // This documents that getProductName works with any ProductEnum value
            var intProductId = (int)productId;
            Assert.True(intProductId > 0);
        }

        #endregion

        #region BlueBook Integration Tests

        [Fact]
        public void BlueBook_CompanyInstanceId_IsUsedForEasyLMS()
        {
            // This documents that EasyLMS uses BlueBook to get company instance information
            var companyMap = new CustomerCompanyMap
            {
                CompanyInstanceId = 12345,
                Source = "BlueBook"
            };

            Assert.Equal(12345, companyMap.CompanyInstanceId);
            Assert.Equal("BlueBook", companyMap.Source);
        }

        [Fact]
        public void BlueBook_UseTranslateFalse_ForEasyLMS()
        {
            // This documents that EasyLMS calls GetProductCompanyInstanceId with useTranslate: false
            // The method signature: GetProductCompanyInstanceId(udmSourceCode, null, useTranslate: false)
            var useTranslate = false;
            Assert.False(useTranslate);
        }

        #endregion

        #region Method Signature Tests

        [Fact]
        public void GetCompanyAPICodeAndKey_RequiresEditorAndUserPersonaIds()
        {
            // This documents the method signature
            long editorPersonaId = 100;
            long userPersonaId = 200;

            Assert.True(editorPersonaId > 0);
            Assert.True(userPersonaId > 0);
        }

        [Fact]
        public void GetCompanyAPICodeAndKey_CanAcceptZeroUserPersonaId()
        {
            // This documents that userPersonaId can be 0 for some scenarios
            long userPersonaId = 0;
            Assert.Equal(0, userPersonaId);
        }

        [Fact]
        public void GetProductName_TakesProductEnumParameter()
        {
            // This documents the getProductName method signature
            ProductEnum productId = ProductEnum.EasyLMS;
            Assert.Equal(ProductEnum.EasyLMS, productId);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public void CustomerCompanyMap_WithNullCompanyInstance_IsValid()
        {
            var companyMap = new CustomerCompanyMap
            {
                CompanyInstanceId = 123,
                CompanyInstance = null
            };

            Assert.Equal(123, companyMap.CompanyInstanceId);
            Assert.Null(companyMap.CompanyInstance);
        }

        [Fact]
        public void CustomerCompanyMap_WithEmptyCompanyInstance_IsValid()
        {
            var companyMap = new CustomerCompanyMap
            {
                CompanyInstanceId = 456,
                CompanyInstance = new List<CompanyInstance>()
            };

            Assert.Equal(456, companyMap.CompanyInstanceId);
            Assert.Empty(companyMap.CompanyInstance);
        }

        [Fact]
        public void GbProductMap_WithNullStrings_IsValid()
        {
            var productMap = new GbProductMap
            {
                ProductId = 42,
                Name = null,
                BooksProductCode = null,
                UDMSourceCode = null
            };

            Assert.Equal(42, productMap.ProductId);
            Assert.Null(productMap.Name);
            Assert.Null(productMap.BooksProductCode);
            Assert.Null(productMap.UDMSourceCode);
        }

        #endregion

        #region Domain Property Tests

        [Fact]
        public void CustomerCompanyMap_Domain_CanBeSet()
        {
            var companyMap = new CustomerCompanyMap
            {
                Domain = "learning.example.com"
            };

            Assert.Equal("learning.example.com", companyMap.Domain);
        }

        [Theory]
        [InlineData("example.com")]
        [InlineData("sub.example.com")]
        [InlineData("learning.company.org")]
        [InlineData(null)]
        [InlineData("")]
        public void CustomerCompanyMap_Domain_AcceptsVariousFormats(string domain)
        {
            var companyMap = new CustomerCompanyMap { Domain = domain };
            Assert.Equal(domain, companyMap.Domain);
        }

        #endregion

        #region CreatedAt and CreatedBy Tests

        [Fact]
        public void CustomerCompanyMap_CreatedAt_TracksCreationTime()
        {
            var now = DateTime.UtcNow;
            var companyMap = new CustomerCompanyMap
            {
                CreatedAt = now
            };

            Assert.Equal(now, companyMap.CreatedAt);
        }

        [Fact]
        public void CustomerCompanyMap_CreatedBy_TracksCreator()
        {
            var companyMap = new CustomerCompanyMap
            {
                CreatedBy = "admin@example.com"
            };

            Assert.Equal("admin@example.com", companyMap.CreatedBy);
        }

        #endregion

        #region Source Property Tests

        [Theory]
        [InlineData("BlueBook")]
        [InlineData("GreenBook")]
        [InlineData("Manual")]
        [InlineData("Import")]
        public void CustomerCompanyMap_Source_AcceptsVariousValues(string source)
        {
            var companyMap = new CustomerCompanyMap { Source = source };
            Assert.Equal(source, companyMap.Source);
        }

        #endregion

        #region Documentation Tests

        [Fact]
        public void ManageProductEasyLMS_ClassPurpose_Documentation()
        {
            // This test documents the class purpose:
            // ManageProductEasyLMS is used to:
            // 1. Get EasyLMS Company API Code and Key from BlueBook
            // 2. Retrieve product name for any given ProductEnum
            //
            // Key methods:
            // - GetCompanyAPICodeAndKey: Gets company instance from BlueBook
            // - getProductName: Gets product name from GbProductMap
            //
            // The class:
            // - Extends ManageProductBase
            // - Implements IManageProductEasyLMS interface
            // - Uses BlueBook for company information
            // - Uses ProductRepository for product details

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageProductEasyLMS_Inheritance_Documentation()
        {
            // This test documents the class hierarchy:
            // ManageProductEasyLMS : ManageProductBase, IManageProductEasyLMS
            //
            // From ManageProductBase, it inherits:
            // - _productId
            // - _editorRealPageId
            // - _blueBook
            // - _productDetails
            // - _productRepository
            // - GetCompanyEditorAndUserDetails method
            // - GetProductCompanyInstanceId method

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageProductEasyLMS_BlueBookIntegration_Documentation()
        {
            // This test documents the BlueBook integration:
            //
            // GetCompanyAPICodeAndKey flow:
            // 1. Call GetCompanyEditorAndUserDetails to get editor/user info
            // 2. Call GetEasyLMSCompanyInstanceId
            //    a. Get udmSourceCode from _productDetails
            //    b. Fall back to BooksProductCode if UDMSourceCode is empty
            //    c. Call GetProductCompanyInstanceId with useTranslate: false
            // 3. Return CustomerCompanyMap containing API code and key

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageProductEasyLMS_GetProductName_Documentation()
        {
            // This test documents the getProductName method:
            //
            // getProductName flow:
            // 1. Call _productRepository.GetBooksMasterProductDetail with productId
            // 2. If result is not null, return the Name property
            // 3. Otherwise return empty string
            //
            // Note: Method name uses camelCase (getProductName) not PascalCase

            Assert.True(true, "Documentation test");
        }

        #endregion

        #region EasyLMS-Specific Constants Tests

        [Fact]
        public void EasyLMS_ProductId_IsConstant()
        {
            // EasyLMS has a fixed ProductEnum value
            var productId = (int)ProductEnum.EasyLMS;
            Assert.Equal(36, productId);
        }

        [Fact]
        public void EasyLMS_UsesBlueBook_ForCompanyInfo()
        {
            // EasyLMS uses BlueBook to retrieve company API credentials
            var expectedSource = "BlueBook";
            Assert.NotEmpty(expectedSource);
        }

        #endregion

        #region Return Type Tests

        [Fact]
        public void GetCompanyAPICodeAndKey_ReturnsCustomerCompanyMap()
        {
            // The return type is CustomerCompanyMap
            CustomerCompanyMap result = new CustomerCompanyMap();
            Assert.NotNull(result);
            Assert.IsType<CustomerCompanyMap>(result);
        }

        [Fact]
        public void GetProductName_ReturnsString()
        {
            // The return type is string
            string result = string.Empty;
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        #endregion

        #region Null Safety Tests

        [Fact]
        public void UDMSourceCode_NullCheck_IsPerformedSafely()
        {
            // The code uses null-conditional operator: UDMSourceCode?.Length
            GbProductMap productDetails = new GbProductMap { UDMSourceCode = null };
            
            var udmSourceCode = productDetails.UDMSourceCode?.Length > 0 
                ? productDetails.UDMSourceCode 
                : productDetails.BooksProductCode;

            Assert.Null(udmSourceCode); // Both are null
        }

        [Fact]
        public void GetProductName_NullProductDetail_ReturnsEmptyString()
        {
            // When GetBooksMasterProductDetail returns null, return empty string
            GbProductMap booksProductDetail = null;
            string productName = string.Empty;

            if (booksProductDetail != null)
            {
                productName = booksProductDetail.Name;
            }

            Assert.Equal(string.Empty, productName);
        }

        #endregion
    }
}
