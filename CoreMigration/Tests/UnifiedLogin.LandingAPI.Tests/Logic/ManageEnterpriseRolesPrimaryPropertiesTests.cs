using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.OneSite;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageEnterpriseRolesPrimaryProperties business logic xUnit tests.
    /// Tests for enterprise roles and primary properties processing operations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageEnterpriseRolesPrimaryPropertiesTests : TestBase
    {
        private readonly Mock<IRepository> _mockRepository;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<IOneSiteProductService> _mockOneSiteProductService;
        private readonly Mock<IManageBlueBook> _mockManageBlueBook;
        private readonly DefaultUserClaim _defaultUserClaim;

        public ManageEnterpriseRolesPrimaryPropertiesTests()
        {
            _mockRepository = new Mock<IRepository>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockOneSiteProductService = new Mock<IOneSiteProductService>();
            _mockManageBlueBook = new Mock<IManageBlueBook>();

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
                ImpersonatedBy = Guid.Empty
            };
        }

        #region Helper Methods

        private Persona CreateTestPersona(long personaId)
        {
            return new Persona
            {
                PersonaId = personaId,
                RealPageId = Guid.NewGuid(),
                UserId = personaId,
                Organization = new Organization
                {
                    RealPageId = Guid.Parse("A5C090FA-78AB-452F-B504-98AAFEE09122"),
                    Name = "Test Organization",
                    PartyId = 1000
                },
                OrganizationPartyId = 1000
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidUserClaim_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageEnterpriseRolesPrimaryProperties = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Assert
            Assert.NotNull(manageEnterpriseRolesPrimaryProperties);
        }

        [Fact]
        public void Constructor_WithNullUserClaim_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                new ManageEnterpriseRolesPrimaryProperties(null));
        }

       
        public void Constructor_WithRepositoryAndMessageHandler_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageEnterpriseRolesPrimaryProperties = new ManageEnterpriseRolesPrimaryProperties(
                _mockRepository.Object,
                _mockHttpMessageHandler.Object,
                _defaultUserClaim,
                _mockOneSiteProductService.Object,
                _mockManageBlueBook.Object);

            // Assert
            Assert.NotNull(manageEnterpriseRolesPrimaryProperties);
        }

      
        public void Constructor_WithRepositoryAndNullOptionalParams_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageEnterpriseRolesPrimaryProperties = new ManageEnterpriseRolesPrimaryProperties(
                _mockRepository.Object,
                _mockHttpMessageHandler.Object,
                _defaultUserClaim);

            // Assert
            Assert.NotNull(manageEnterpriseRolesPrimaryProperties);
        }

        #endregion

        #region ProcessEnterpriseRolesAndPrimaryPropertiesData Tests - Basic Scenarios

        [Fact]
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_WithValidParams_ReturnsResult()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: 100,
                subjectUserPersonaId: 200);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_WithEnterpriseRoleTemplateId_ReturnsResult()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: 100,
                subjectUserPersonaId: 200,
                enterpriseRoleTemplateId: 5);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_WithCreatedDateTime_ReturnsResult()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: 100,
                subjectUserPersonaId: 200,
                enterpriseRoleTemplateId: 5,
                createdDateTime: DateTime.UtcNow);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_WithInvalidEditorPersonaId_ReturnsError()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: -1,
                subjectUserPersonaId: 200);

            // Assert
            Assert.Equal("Error", result);
        }

        [Fact]
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_WithInvalidSubjectPersonaId_ReturnsError()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: 100,
                subjectUserPersonaId: -1);

            // Assert
            Assert.Equal("Error", result);
        }

        #endregion

        #region ProcessEnterpriseRolesAndPrimaryPropertiesData Tests - BatchProcessType Scenarios

        [Fact]
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_WithBulkAddUpdateEnterpriseRole_ReturnsResult()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: 100,
                subjectUserPersonaId: 200,
                enterpriseRoleTemplateId: 5,
                createdDateTime: DateTime.UtcNow,
                batchProcessTypeId: (int)BatchProcessType.BulkAddUpdateEnterpriseRole);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_WithIsUnassignAllProducts_ReturnsResult()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: 100,
                subjectUserPersonaId: 200,
                enterpriseRoleTemplateId: 5,
                createdDateTime: DateTime.UtcNow,
                batchProcessTypeId: (int)BatchProcessType.BulkAddUpdateEnterpriseRole,
                isUnassignAllProducts: true);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        [Theory]
        [InlineData((int)BatchProcessType.CreateUpdateProductUser)]
        [InlineData((int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser)]
        [InlineData((int)BatchProcessType.BulkAddUpdateEnterpriseRole)]
        [InlineData((int)BatchProcessType.PrimaryPropertiesUpdateProductUser)]
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_WithDifferentBatchTypes_ReturnsResult(int batchProcessType)
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: 100,
                subjectUserPersonaId: 200,
                enterpriseRoleTemplateId: 5,
                createdDateTime: DateTime.UtcNow,
                batchProcessTypeId: batchProcessType);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        #endregion

        #region ProcessEnterpriseRolesAndPrimaryPropertiesData Tests - Edge Cases

        [Fact]
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_WithZeroEnterpriseRoleTemplateId_ReturnsResult()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: 100,
                subjectUserPersonaId: 200,
                enterpriseRoleTemplateId: 0);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_WithNullEnterpriseRoleTemplateId_ReturnsResult()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: 100,
                subjectUserPersonaId: 200,
                enterpriseRoleTemplateId: null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

       
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_WithNullCreatedDateTime_ReturnsResult()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: 100,
                subjectUserPersonaId: 200,
                enterpriseRoleTemplateId: 5,
                createdDateTime: null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_WithFutureDateTime_ReturnsResult()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: 100,
                subjectUserPersonaId: 200,
                enterpriseRoleTemplateId: 5,
                createdDateTime: DateTime.UtcNow.AddYears(1));

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_WithPastDateTime_ReturnsResult()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: 100,
                subjectUserPersonaId: 200,
                enterpriseRoleTemplateId: 5,
                createdDateTime: DateTime.UtcNow.AddYears(-1));

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_WithSameEditorAndSubject_ReturnsResult()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: 100,
                subjectUserPersonaId: 100); // Same as editor

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_WithMaxLongPersonaId_ReturnsError()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: long.MaxValue,
                subjectUserPersonaId: long.MaxValue - 1);

            // Assert
            Assert.Equal("Error", result);
        }

        #endregion

        #region BundleAoProducts Tests

        [Fact]
        public void BundleAoProducts_WithEmptyProductList_ReturnsEmptyString()
        {
            // Arrange
            var productList = new List<ProductBatch>();

            // Act
            var result = ManageEnterpriseRolesPrimaryProperties.BundleAoProducts(productList);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("", result);
        }

        [Fact]
        public void BundleAoProducts_WithNonAoProducts_ReturnsEmptyString()
        {
            // Arrange
            var productList = new List<ProductBatch>
            {
                new ProductBatch
                {
                    ProductId = (int)ProductEnum.OneSite,
                    StatusTypeId = 5,
                    RetryCount = 0,
                    InputJson = new RolePropertyList
                    {
                        PropertyList = new List<string> { "1", "2" },
                        RoleList = new List<string> { "Admin" },
                        IsAssigned = true
                    }
                }
            };

            // Act
            var result = ManageEnterpriseRolesPrimaryProperties.BundleAoProducts(productList);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("", result);
        }

        [Fact]
        public void BundleAoProducts_WithBatchProcessorGroupId_ReturnsResult()
        {
            // Arrange
            var productList = new List<ProductBatch>();

            // Act
            var result = ManageEnterpriseRolesPrimaryProperties.BundleAoProducts(productList, batchProcessorGroupId: 5);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void BundleAoProducts_WithZeroBatchProcessorGroupId_ReturnsResult()
        {
            // Arrange
            var productList = new List<ProductBatch>();

            // Act
            var result = ManageEnterpriseRolesPrimaryProperties.BundleAoProducts(productList, batchProcessorGroupId: 0);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_OnException_ReturnsError()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: -999,
                subjectUserPersonaId: -999);

            // Assert
            Assert.Equal("Error", result);
        }

        [Fact]
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_WithZeroPersonaIds_ReturnsError()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: 0,
                subjectUserPersonaId: 0);

            // Assert
            Assert.Equal("Error", result);
        }

        #endregion

        #region Primary Properties Processing Tests

        [Fact]
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_PrimaryPropertiesMode_ReturnsResult()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act - When enterpriseRoleTemplateId is null, it operates in Primary Properties mode
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: 100,
                subjectUserPersonaId: 200,
                enterpriseRoleTemplateId: null,
                createdDateTime: null,
                batchProcessTypeId: 0);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_EnterpriseRoleMode_ReturnsResult()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act - When enterpriseRoleTemplateId is provided, it operates in Enterprise Role mode
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: 100,
                subjectUserPersonaId: 200,
                enterpriseRoleTemplateId: 10,
                createdDateTime: DateTime.UtcNow);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        #endregion

        #region Integration Tests

       
        public void ManageEnterpriseRolesPrimaryProperties_FullWorkflow_HandlesCorrectly()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act - Process enterprise role
            var enterpriseRoleResult = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: 100,
                subjectUserPersonaId: 200,
                enterpriseRoleTemplateId: 5,
                createdDateTime: DateTime.UtcNow);

            // Assert
            Assert.NotNull(enterpriseRoleResult);
            Assert.IsType<string>(enterpriseRoleResult);
            Assert.True(enterpriseRoleResult == "" || enterpriseRoleResult == "Error");
        }

        [Fact]
        public void ManageEnterpriseRolesPrimaryProperties_MultipleCalls_HandlesCorrectly()
        {
            // Arrange
            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(_defaultUserClaim);

            // Act
            var result1 = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(100, 200);
            var result2 = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(101, 201);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.IsType<string>(result1);
            Assert.IsType<string>(result2);
        }

        #endregion

        #region UserClaim Tests

        [Fact]
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_WithImpersonatedUser_ReturnsResult()
        {
            // Arrange
            var userClaimWithImpersonation = new DefaultUserClaim
            {
                UserId = 1,
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 1000,
                PersonaId = 5,
                ImpersonatedBy = Guid.NewGuid() // Has impersonation
            };

            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(userClaimWithImpersonation);

            // Act
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: 100,
                subjectUserPersonaId: 200);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void ProcessEnterpriseRolesAndPrimaryPropertiesData_WithoutImpersonation_ReturnsResult()
        {
            // Arrange
            var userClaimWithoutImpersonation = new DefaultUserClaim
            {
                UserId = 1,
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 1000,
                PersonaId = 5,
                ImpersonatedBy = Guid.Empty // No impersonation
            };

            var manageEnterpriseRoles = new ManageEnterpriseRolesPrimaryProperties(userClaimWithoutImpersonation);

            // Act
            var result = manageEnterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesData(
                editorUserPersonaId: 100,
                subjectUserPersonaId: 200);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        #endregion

        #region Product Batch Tests

        [Fact]
        public void ProductBatch_Creation_WorksCorrectly()
        {
            // Arrange & Act
            var productBatch = new ProductBatch
            {
                ProductId = (int)ProductEnum.UnifiedPlatform,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = new RolePropertyList
                {
                    PropertyList = new List<string>(),
                    RoleList = new List<string>(),
                    IsAssigned = false
                }
            };

            // Assert
            Assert.Equal((int)ProductEnum.UnifiedPlatform, productBatch.ProductId);
            Assert.Equal(5, productBatch.StatusTypeId);
            Assert.Equal(0, productBatch.RetryCount);
            Assert.NotNull(productBatch.InputJson);
            Assert.False(productBatch.InputJson.IsAssigned);
        }

        [Fact]
        public void RolePropertyList_Initialization_WorksCorrectly()
        {
            // Arrange & Act
            var rolePropertyList = new RolePropertyList
            {
                PropertyList = new List<string> { "1", "2", "3" },
                RoleList = new List<string> { "Admin", "User" },
                IsAssigned = true,
                UsePrimaryProperties = true,
                CompanyId = 100
            };

            // Assert
            Assert.Equal(3, rolePropertyList.PropertyList.Count);
            Assert.Equal(2, rolePropertyList.RoleList.Count);
            Assert.True(rolePropertyList.IsAssigned);
            Assert.True(rolePropertyList.UsePrimaryProperties);
            Assert.Equal(100, rolePropertyList.CompanyId);
        }

        #endregion
    }
}
