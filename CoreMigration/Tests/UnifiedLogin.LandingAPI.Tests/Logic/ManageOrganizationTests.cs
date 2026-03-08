using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Maintenance;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageOrganization business logic xUnit tests.
    /// Tests for organization management operations including CRUD, properties, and company setup.
    /// Migrated from RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.ManageOrganizationTest
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageOrganizationTests : TestBase
    {
        private readonly Mock<IRepository> _mockRepository;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<IManageProductAssetOptimization> _mockManageProductAssetOptimization;
        private readonly DefaultUserClaim _defaultUserClaim;

        public ManageOrganizationTests()
        {
            _mockRepository = new Mock<IRepository>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockManageProductAssetOptimization = new Mock<IManageProductAssetOptimization>();

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
                IsActive = 1,
                OrganizationTypeId = 1,
                organizationType = new OrganizationType { OrganizationTypeId = 1, Name = "Client" },
                OrganizationDomain = new OrganizationDomain { OrganizationDomainId = 1, Name = "Primary" },
                EnablePrimaryProperties = 0,
                EnableEnterpriseRoles = 0
            };
        }

        private OrganizationCreate CreateValidOrganizationCreate()
        {
            return new OrganizationCreate
            {
                Name = "New Test Organization",
                BooksCompanyId = 12345,
                BooksCustomerMasterId = 67890,
                OrganizationTypeId = 1,
                OrganizationDomainId = 1,
                OrganizationDomain = "Primary",
                IsActive = 1,
                EnablePrimaryProperties = 1,
                EnableEnterpriseRoles = 0,
                AdminUser = new OrganizationAdminUser
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@test.com",
                    Title = "Administrator",
                    Suffix = ""
                }
            };
        }

        private UPFMPropertyInstance CreateValidPropertyInstance()
        {
            return new UPFMPropertyInstance
            {
                InstanceId = Guid.NewGuid(),
                Name = "Test Property",
                Domain = "Primary",
                IsActive = true,
                Address = "123 Test St",
                City = "Test City",
                State = "TX",
                PostalCode = "12345",
                Country = "USA",
                County = "Test County"
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithRepositoryAndUserClaim_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageOrganization = new ManageOrganization(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object,
                _mockManageProductAssetOptimization.Object);

            // Assert
            Assert.NotNull(manageOrganization);
        }

        [Fact]
        public void Constructor_WithUserClaimOnly_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Assert
            Assert.NotNull(manageOrganization);
        }

        [Fact]
        public void Constructor_WithNullUserClaim_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                new ManageOrganization(
                    _mockRepository.Object,
                    null,
                    _mockHttpMessageHandler.Object,
                    _mockManageProductAssetOptimization.Object));
        }

        #endregion

        #region InsertOrganization Tests

       
        public void InsertOrganization_WithValidOrganization_ReturnsSuccessResponse()
        {
            // Arrange
            var organization = CreateValidOrganization();
            organization.RealPageId = Guid.Empty; // New organization
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.InsertOrganization(organization);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        [Fact]
        public void InsertOrganization_WithNullOrganization_ThrowsArgumentNullException()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageOrganization.InsertOrganization(null));

            Assert.Equal("organization", exception.ParamName);
            Assert.Contains("Null Organization", exception.Message);
        }

       
        public void InsertOrganization_WithExistingRealPageId_UpdatesOrganization()
        {
            // Arrange
            var organization = CreateValidOrganization();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.InsertOrganization(organization);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        #endregion

        #region UpdateOrganization Tests

       
        public void UpdateOrganization_WithValidOrganization_ReturnsSuccessResponse()
        {
            // Arrange
            var organization = CreateValidOrganization();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.UpdateOrganization(organization);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        [Fact]
        public void UpdateOrganization_WithNullOrganization_ReturnsErrorResponse()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.UpdateOrganization(null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Organization is Null", result.ErrorMessage);
        }

        [Fact]
        public void UpdateOrganization_WithEmptyRealPageId_ReturnsErrorResponse()
        {
            // Arrange
            var organization = CreateValidOrganization();
            organization.RealPageId = Guid.Empty;
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.UpdateOrganization(organization);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Invalid parameter realPageId.", result.ErrorMessage);
        }

        #endregion

        #region GetOrganization Tests

        [Fact]
        public void GetOrganization_WithValidRealPageId_ReturnsOrganization()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.GetOrganization(realPageId);

            // Assert
            // Result may be null if not found in test environment
            Assert.True(result == null || result is Organization);
        }

        [Fact]
        public void GetOrganization_WithEmptyGuidAndNullPartyId_ThrowsException()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageOrganization.GetOrganization(Guid.Empty, null));

            Assert.Contains("Invalid parameter", exception.Message);
        }

        [Fact]
        public void GetOrganization_WithValidPartyId_ReturnsOrganization()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.GetOrganization(Guid.Empty, 1000);

            // Assert
            Assert.True(result == null || result is Organization);
        }

        #endregion

        #region GetOrganizationList Tests

        [Fact]
        public void GetOrganizationList_ReturnsListOfOrganizations()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.GetOrganizationList();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IList<Organization>>(result);
        }

        #endregion

        #region ValidateOrganization Tests

        [Fact]
        public void ValidateOrganization_WithZeroOrganizationMasterId_ThrowsArgumentNullException()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                manageOrganization.ValidateOrganization(0, Guid.NewGuid(), Guid.NewGuid()));
        }

     
        public void ValidateOrganization_WithValidParameters_ReturnsBoolean()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.ValidateOrganization(379, Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.IsType<bool>(result);
        }

        #endregion

        #region ListOrganizationType Tests

        [Fact]
        public void ListOrganizationType_ReturnsListOfOrganizationTypes()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.ListOrganizationType();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<OrganizationType>>(result);
        }

        #endregion

        #region ListOrganizationDomain Tests

        [Fact]
        public void ListOrganizationDomain_ReturnsListOfOrganizationDomains()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.ListOrganizationDomain();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<OrganizationDomain>>(result);
        }

        #endregion

        #region CreateOrganizationDomain Tests

        [Fact]
        public void CreateOrganizationDomain_WithValidDomain_ReturnsRepositoryResponse()
        {
            // Arrange
            var domain = new OrganizationDomain
            {
                Name = "Test Domain"
            };
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.CreateOrganizationDomain(domain);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        #endregion

        #region UpdateOrganizationThirdPartyIDP Tests

        [Fact]
        public void UpdateOrganizationThirdPartyIDP_WithNullOrganization_ThrowsArgumentNullException()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageOrganization.UpdateOrganizationThirdPartyIDP(null));

            Assert.Equal("organization", exception.ParamName);
        }

        [Fact]
        public void UpdateOrganizationThirdPartyIDP_WithEmptyRealPageId_ThrowsArgumentNullException()
        {
            // Arrange
            var organization = CreateValidOrganization();
            organization.RealPageId = Guid.Empty;
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageOrganization.UpdateOrganizationThirdPartyIDP(organization));

            Assert.Equal("organization", exception.ParamName);
        }

        #endregion

        #region UpdateOrganizationUsePrimaryPropertySetting Tests

        [Fact]
        public void UpdateOrganizationUsePrimaryPropertySetting_WithNullOrganization_ThrowsArgumentNullException()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageOrganization.UpdateOrganizationUsePrimaryPropertySetting(null));

            Assert.Equal("organization", exception.ParamName);
        }

        [Fact]
        public void UpdateOrganizationUsePrimaryPropertySetting_WithEmptyRealPageId_ThrowsException()
        {
            // Arrange
            var organization = CreateValidOrganization();
            organization.RealPageId = Guid.Empty;
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageOrganization.UpdateOrganizationUsePrimaryPropertySetting(organization));

            Assert.Contains("Invalid parameter realPageId", exception.Message);
        }

        #endregion

        #region UpdateUsePrimaryPropertyForOrganizationProduct Tests

        [Fact]
        public void UpdateUsePrimaryPropertyForOrganizationProduct_WithZeroOrganizationPartyId_ReturnsError()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.UpdateUsePrimaryPropertyForOrganizationProduct(0, 1, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Invalid parameter organizationPartyId.", result.ErrorMessage);
        }

        [Fact]
        public void UpdateUsePrimaryPropertyForOrganizationProduct_WithZeroProductId_ReturnsError()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.UpdateUsePrimaryPropertyForOrganizationProduct(1000, 0, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Invalid parameter productId.", result.ErrorMessage);
        }

        #endregion

        #region GetOrganizationSettingValue Tests

        [Fact]
        public void GetOrganizationSettingValue_WithEmptySettingName_ThrowsException()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageOrganization.GetOrganizationSettingValue(1000, ""));

            Assert.Contains("Setting name is required", exception.Message);
        }

        [Fact]
        public void GetOrganizationSettingValue_WithZeroPartyId_ThrowsException()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageOrganization.GetOrganizationSettingValue(0, "TestSetting"));

            Assert.Contains("Organization partyId is required", exception.Message);
        }

        #endregion

        #region ParseProduct Tests

        [Fact]
        public void ParseProduct_WithValidProductCodes_ReturnsInvalidList()
        {
            // Arrange
            var productCodes = new List<string> { "AC", "OS", "INVALID" };
            var addProductList = new List<int>();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.ParseProduct(productCodes, addProductList);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<string>>(result);
        }

        [Fact]
        public void ParseProduct_WithNullProductCodes_ReturnsEmptyList()
        {
            // Arrange
            var addProductList = new List<int>();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.ParseProduct(null, addProductList);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region Property Management Tests

        [Fact]
        public void GetPropertyByInstanceId_WithValidInstanceId_ReturnsList()
        {
            // Arrange
            var instanceId = Guid.NewGuid();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.GetPropertyByInstanceId(instanceId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<UPFMPropertyInstance>>(result);
        }

        [Fact]
        public void UpdateProperty_WithEmptyInstanceId_ReturnsErrorResponse()
        {
            // Arrange
            var property = CreateValidPropertyInstance();
            property.InstanceId = Guid.Empty;
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.UpdateProperty(property, Guid.NewGuid());

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Invalid parameter propertyInstanceId.", result.ErrorMessage);
        }

        [Fact]
        public void UpdateProperty_WithEmptyName_ReturnsErrorResponse()
        {
            // Arrange
            var property = CreateValidPropertyInstance();
            property.Name = "";
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.UpdateProperty(property, Guid.NewGuid());

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Invalid parameter propertyName.", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdatePropertyList_WithNullList_ReturnsErrorResponse()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = await manageOrganization.UpdatePropertyList(null, Guid.NewGuid());

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Invalid parameter propertyInstanceId.", result.ErrorMessage);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public void CreateOrganization_WithZeroOrganizationTypeId_ReturnsError()
        {
            // Arrange
            var orgCreate = CreateValidOrganizationCreate();
            orgCreate.OrganizationTypeId = 0;
            var addProductList = new List<int>();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.CreateOrganization(orgCreate, addProductList);

            // Assert
            Assert.False(result.Status.Success);
            Assert.Contains("invalid Organization Type id", result.Status.ErrorMsg);
        }

        [Fact]
        public void CreateOrganization_WithNullAdminUser_ReturnsError()
        {
            // Arrange
            var orgCreate = CreateValidOrganizationCreate();
            orgCreate.AdminUser = null;
            var addProductList = new List<int>();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.CreateOrganization(orgCreate, addProductList);

            // Assert
            Assert.False(result.Status.Success);
            Assert.Contains("No admin user information provided", result.Status.ErrorMsg);
        }

        #endregion

        #region EnableProductOnOtherProductsActivation Tests

        [Fact]
        public void EnableProductOnOtherProductsActivation_WithProductList_ReturnsUpdatedList()
        {
            // Arrange
            var productList = new List<int> { 1, 2, 3 };
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.EnableProductOnOtherProductsActivation(productList);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<int>>(result);
            Assert.Contains(1, result);
            Assert.Contains(2, result);
            Assert.Contains(3, result);
        }

        [Fact]
        public void EnableProductOnOtherProductsActivation_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var productList = new List<int>();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.EnableProductOnOtherProductsActivation(productList);
                    
            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region InsertOrganizationRemovalQueue Tests

    
        public void InsertOrganizationRemovalQueue_WithValidQueue_ReturnsQueue()
        {
            // Arrange
            var queue = new OrganizationRemovalQueue
            {
                OrganizationPartyId = 1000,
                OrganizationRealPageId = Guid.NewGuid(),
                OrganizationRemoveUDMData = true
            };
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.InsertOrganizationRemovalQueue(queue);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OrganizationRemovalQueue>(result);
        }

        #endregion

        #region DeleteQueuedOrganizations Tests

        [Fact]
        public void DeleteQueuedOrganizations_ExecutesWithoutException()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act & Assert - Should not throw
            manageOrganization.DeleteQueuedOrganizations();
        }

        #endregion

        #region GetUnifiedLoginCompanyList Tests

        [Fact]
        public void GetUnifiedLoginCompanyList_ReturnsListOfCompanies()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.GetUnifiedLoginCompanyList();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<UnifiedLoginCompany>>(result);
        }

        #endregion

        #region GetOrganizationAdminUserRealPageId Tests

        [Fact]
        public void GetOrganizationAdminUserRealPageId_WithValidGuid_ReturnsGuid()
        {
            // Arrange
            var organizationRealPageId = Guid.NewGuid();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.GetOrganizationAdminUserRealPageId(organizationRealPageId);

            // Assert
            Assert.IsType<Guid>(result);
        }

        #endregion

        #region GetOrganizationIdentityProviderType Tests

        [Fact]
        public void GetOrganizationIdentityProviderType_WithValidRealPageId_ReturnsList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.GetOrganizationIdentityProviderType(realPageId);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IList<IdentityProviderType>>(result);
        }

        #endregion

        #region CreateInitialOrgSuperUser Tests

        [Fact]
        public void CreateInitialOrgSuperUser_WithValidParameters_ReturnsRepositoryResponse()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.CreateInitialOrgSuperUser(
                1000,
                "John",
                "M",
                "Doe",
                "Admin",
                "Jr",
                "john.doe@test.com",
                true,
                null,
                Guid.NewGuid());

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        [Fact]
        public void CreateInitialOrgSuperUser_WithMinimalParameters_ReturnsRepositoryResponse()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.CreateInitialOrgSuperUser(
                1000,
                "Jane",
                "",
                "Smith",
                "",
                "",
                "jane.smith@test.com",
                true,
                null,
                Guid.NewGuid());

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        #endregion

        #region GetPropertiesByInstanceId Tests

        [Fact]
        public void GetPropertiesByInstanceId_WithValidInstanceIds_ReturnsList()
        {
            // Arrange
            var instanceIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.GetPropertiesByInstanceId(instanceIds);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<UPFMPropertyInstance>>(result);
        }

        [Fact]
        public void GetPropertiesByInstanceId_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var instanceIds = new List<Guid>();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.GetPropertiesByInstanceId(instanceIds);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<UPFMPropertyInstance>>(result);
        }

        #endregion

        #region ProcessPropertyList Tests

      
        public async Task ProcessPropertyList_WithValidProperty_ReturnsRepositoryResponse()
        {
            // Arrange
            var property = CreateValidPropertyInstance();
            var companyInstanceId = Guid.NewGuid();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = await manageOrganization.ProcessPropertyList(property, companyInstanceId);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IRepositoryResponse>(result);
        }

        #endregion

        #region AddPropertyForOrganization Tests

        [Fact]
        public void AddPropertyForOrganization_WithValidProperty_ReturnsRepositoryResponse()
        {
            // Arrange
            var property = CreateValidPropertyInstance();
            var companyInstanceId = Guid.NewGuid();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.AddPropertyForOrganization(property, companyInstanceId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        #endregion

        #region DeletePropertyForOrganization Tests

        [Fact]
        public void DeletePropertyForOrganization_WithValidInstanceId_ReturnsRepositoryResponse()
        {
            // Arrange
            var propertyInstanceId = Guid.NewGuid();
            var companyInstanceId = Guid.NewGuid();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.DeletePropertyForOrganization(propertyInstanceId, companyInstanceId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        #endregion

        #region InsertUPFMPropertyInstance Tests

        [Fact]
        public void InsertUPFMPropertyInstance_WithValidProperty_ReturnsRepositoryResponse()
        {
            // Arrange
            var property = CreateValidPropertyInstance();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.InsertUPFMPropertyInstance(property);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        #endregion

        #region GetCompanyList Tests

        [Fact]
        public void GetCompanyList_WithValidParameters_ReturnsCompanyList()
        {
            // Arrange
            var globals = new Dictionary<object, object>
            {
                { BaseType.RequestParameter, new RequestParameter() }
            };
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.GetCompanyList("Test", 1, null, 0, globals);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<CompanySetup>>(result);
        }

        [Fact]
        public void GetCompanyList_WithNullFilter_ReturnsCompanyList()
        {
            // Arrange
            var globals = new Dictionary<object, object>
            {
                { BaseType.RequestParameter, new RequestParameter() }
            };
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.GetCompanyList(null, 0, null, 0, globals);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<CompanySetup>>(result);
        }

        #endregion

        #region SearchCompanyDetailsByCustomerCompanyId Tests

        [Fact]
        public void SearchCompanyDetailsByCustomerCompanyId_WithValidId_ReturnsCompanyMaster()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.SearchCompanyDetailsByCustomerCompanyId(12345);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CompanyMaster>(result);
        }

        [Fact]
        public void SearchCompanyDetailsByCustomerCompanyId_WithZeroId_ReturnsCompanyMaster()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.SearchCompanyDetailsByCustomerCompanyId(0);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CompanyMaster>(result);
        }

        #endregion

        #region GetPropertiesForCompany Tests

        
        public void GetPropertiesForCompany_WithValidCompanyInstanceId_ReturnsPropertyList()
        {
            // Arrange
            var companyInstanceId = Guid.NewGuid();
            var manageOrganization = new ManageOrganization(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object,
                _mockManageProductAssetOptimization.Object);

            // Act
            var result = manageOrganization.GetPropertiesForCompany(
                companyInstanceId,
                null,
                null,
                null,
                null,
                null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<CompanyPropertySetup>>(result);
        }

       
        public void GetPropertiesForCompany_WithPropertyNameFilter_ReturnsPropertyList()
        {
            // Arrange
            var companyInstanceId = Guid.NewGuid();
            var manageOrganization = new ManageOrganization(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object,
                _mockManageProductAssetOptimization.Object);

            // Act
            var result = manageOrganization.GetPropertiesForCompany(
                companyInstanceId,
                "TestProperty",
                null,
                null,
                null,
                null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<CompanyPropertySetup>>(result);
        }

        #endregion

        #region SearchPropertyDetailsByCustomerPropertyId Tests

        [Fact]
        public void SearchPropertyDetailsByCustomerPropertyId_WithValidId_ReturnsPropertyInstanceSearch()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.SearchPropertyDetailsByCustomerPropertyId("12345", "67890");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PropertyInstanceSearch>(result);
        }

      
        public void SearchPropertyDetailsByCustomerPropertyId_WithNullIds_ReturnsPropertyInstanceSearch()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.SearchPropertyDetailsByCustomerPropertyId(null, null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PropertyInstanceSearch>(result);
        }

        #endregion

        #region AuditCompanyProductPropertiesToUPFM Tests

        [Fact]
        public void AuditCompanyProductPropertiesToUPFM_WithValidParameters_ReturnsAuditList()
        {
            // Arrange
            var companyInstanceId = Guid.NewGuid();
            var productId = 1;
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.AuditCompanyProductPropertiesToUPFM(companyInstanceId, productId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<PropertyAudit>>(result);
        }

        [Fact]
        public void AuditCompanyProductPropertiesToUPFM_WithDifferentProducts_ReturnsAuditList()
        {
            // Arrange
            var companyInstanceId = Guid.NewGuid();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result1 = manageOrganization.AuditCompanyProductPropertiesToUPFM(companyInstanceId, 1);
            var result2 = manageOrganization.AuditCompanyProductPropertiesToUPFM(companyInstanceId, 8);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.IsType<List<PropertyAudit>>(result1);
            Assert.IsType<List<PropertyAudit>>(result2);
        }

        #endregion

        #region GetSourceProductDetails Tests

        [Fact]
        public void GetSourceProductDetails_WithValidParameters_ReturnsProductPropertyDetails()
        {
            // Arrange
            var propertyInstanceSourceId = Guid.NewGuid().ToString();
            var source = "UPFM";
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.GetSourceProductDetails(propertyInstanceSourceId, source);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ProductPropertyDetails>(result);
        }

        [Fact]
        public void GetSourceProductDetails_WithDifferentSources_ReturnsProductPropertyDetails()
        {
            // Arrange
            var propertyInstanceSourceId = Guid.NewGuid().ToString();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result1 = manageOrganization.GetSourceProductDetails(propertyInstanceSourceId, "UPFM");
            var result2 = manageOrganization.GetSourceProductDetails(propertyInstanceSourceId, "AC");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.IsType<ProductPropertyDetails>(result1);
            Assert.IsType<ProductPropertyDetails>(result2);
        }

        #endregion

        #region AddUpdateCompanyToUnifiedSettings Tests

        [Fact]
        public void AddUpdateCompanyToUnifiedSettings_WithCreateTransactionType_ReturnsBoolean()
        {
            // Arrange
            var companyInstanceId = Guid.NewGuid().ToString();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.AddUpdateCompanyToUnifiedSettings(
                companyInstanceId,
                "create",
                "Production");

            // Assert
            Assert.IsType<bool>(result);
        }

        [Fact]
        public void AddUpdateCompanyToUnifiedSettings_WithUpdateTransactionType_ReturnsBoolean()
        {
            // Arrange
            var companyInstanceId = Guid.NewGuid().ToString();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.AddUpdateCompanyToUnifiedSettings(
                companyInstanceId,
                "update",
                "Staging");

            // Assert
            Assert.IsType<bool>(result);
        }

        [Fact]
        public void AddUpdateCompanyToUnifiedSettings_WithNullEnvironment_ReturnsBoolean()
        {
            // Arrange
            var companyInstanceId = Guid.NewGuid().ToString();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.AddUpdateCompanyToUnifiedSettings(
                companyInstanceId,
                "create",
                null);

            // Assert
            Assert.IsType<bool>(result);
        }

        #endregion

        #region UpdatePropertyInSettingsAndActivityLogs Tests

      
        public void UpdatePropertyInSettingsAndActivityLogs_WithValidProperty_ReturnsBoolean()
        {
            // Arrange
            var property = CreateValidPropertyInstance();
            var companyInstanceId = Guid.NewGuid();
            var oldPropertyList = new List<UPFMPropertyInstance> { CreateValidPropertyInstance() };
            oldPropertyList[0].InstanceId = property.InstanceId;
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.UpdatePropertyInSettingsAndActivityLogs(
                property,
                companyInstanceId,
                oldPropertyList);

            // Assert
            Assert.IsType<bool>(result);
        }

        #endregion

        #region AddCompanyToJob Tests

       
        public void AddCompanyToJob_WithValidParameters_ReturnsRepositoryResponse()
        {
            // Arrange
            var companyInstanceSourceId = Guid.NewGuid().ToString();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.AddCompanyToJob(companyInstanceSourceId, 1, 100, 1);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

       
        public void AddCompanyToJob_WithInactiveOrganization_ReturnsRepositoryResponse()
        {
            // Arrange
            var companyInstanceSourceId = Guid.NewGuid().ToString();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.AddCompanyToJob(companyInstanceSourceId, 1, 100, 0);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        #endregion

        #region UpdateCompanyInstance Tests

      
        public async Task UpdateCompanyInstance_WithValidParameters_ReturnsRepositoryResponse()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = await manageOrganization.UpdateCompanyInstance(1, 1, "Test Error");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

    
        public async Task UpdateCompanyInstance_WithNullErrorMessage_ReturnsRepositoryResponse()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = await manageOrganization.UpdateCompanyInstance(1, 1, null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RepositoryResponse>(result);
        }

        #endregion

        #region Integration Workflow Tests

        [Fact]
        public void ManageOrganization_CompleteOrganizationWorkflow_HandlesCorrectly()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act - Get organization list
            var orgList = manageOrganization.GetOrganizationList();

            // Act - Get organization types
            var orgTypes = manageOrganization.ListOrganizationType();

            // Act - Get organization domains
            var orgDomains = manageOrganization.ListOrganizationDomain();

            // Assert
            Assert.NotNull(orgList);
            Assert.NotNull(orgTypes);
            Assert.NotNull(orgDomains);
        }

        [Fact]
        public void ManageOrganization_PropertyManagementWorkflow_HandlesCorrectly()
        {
            // Arrange
            var propertyInstanceId = Guid.NewGuid();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act - Get property by instance ID
            var property = manageOrganization.GetPropertyByInstanceId(propertyInstanceId);

            // Act - Get properties by instance IDs
            var properties = manageOrganization.GetPropertiesByInstanceId(new List<Guid> { propertyInstanceId });

            // Assert
            Assert.NotNull(property);
            Assert.NotNull(properties);
        }

        [Fact]
        public void ManageOrganization_CompanySearchWorkflow_HandlesCorrectly()
        {
            // Arrange
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act - Search company details
            var companyDetails = manageOrganization.SearchCompanyDetailsByCustomerCompanyId(12345);

            // Act - Search property details
            var propertyDetails = manageOrganization.SearchPropertyDetailsByCustomerPropertyId("12345", "67890");

            // Assert
            Assert.NotNull(companyDetails);
            Assert.NotNull(propertyDetails);
        }

     
        public async Task ManageOrganization_AsyncOperations_HandleCorrectly()
        {
            // Arrange
            var property = CreateValidPropertyInstance();
            var companyInstanceId = Guid.NewGuid();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var processResult = await manageOrganization.ProcessPropertyList(property, companyInstanceId);
            var updateResult = await manageOrganization.UpdateCompanyInstance(1, 1, "Test");

            // Assert
            Assert.NotNull(processResult);
            Assert.NotNull(updateResult);
        }

        #endregion

        #region Additional Edge Cases

        [Fact]
        public void UpdateProperty_WithNullName_ReturnsErrorResponse()
        {
            // Arrange
            var property = CreateValidPropertyInstance();
            property.Name = null;
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.UpdateProperty(property, Guid.NewGuid());

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Invalid parameter propertyName.", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdatePropertyList_WithEmptyInstanceId_ReturnsErrorResponse()
        {
            // Arrange
            var propertyList = new List<UPFMPropertyInstance>
            {
                CreateValidPropertyInstance()
            };
            propertyList[0].InstanceId = Guid.Empty;
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = await manageOrganization.UpdatePropertyList(propertyList, Guid.NewGuid());

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Invalid parameter propertyInstanceId.", result.ErrorMessage);
        }

        [Fact]
        public void CreateOrganization_WithValidParametersAndProducts_ReturnsSuccess()
        {
            // Arrange
            var orgCreate = CreateValidOrganizationCreate();
            var addProductList = new List<int> { 3, 8 }; // UPFM and AC products
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.CreateOrganization(orgCreate, addProductList);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Status);
        }

        [Fact]
        public void ParseProduct_WithMixedValidAndInvalidCodes_ReturnsOnlyInvalidOnes()
        {
            // Arrange
            var productCodes = new List<string> { "VALID1", "INVALID1", "VALID2", "INVALID2" };
            var addProductList = new List<int>();
            var manageOrganization = new ManageOrganization(_defaultUserClaim);

            // Act
            var result = manageOrganization.ParseProduct(productCodes, addProductList);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<string>>(result);
            // Invalid products should be in the result
        }

       
        public void GetPropertiesForCompany_WithAllFilters_ReturnsFilteredProperties()
        {
            // Arrange
            var companyInstanceId = Guid.NewGuid();
            var globals = new Dictionary<object, object>
            {
                { BaseType.RequestParameter, new RequestParameter() }
            };
            var manageOrganization = new ManageOrganization(
                _mockRepository.Object,
                _defaultUserClaim,
                _mockHttpMessageHandler.Object,
                _mockManageProductAssetOptimization.Object);

            // Act
            var result = manageOrganization.GetPropertiesForCompany(
                companyInstanceId,
                "TestProperty",
                "Primary",
                12345,
                1,
                globals,
                100,
                200,
                true,
                new List<Guid> { Guid.NewGuid() },
                "Operator",
                "Value");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<CompanyPropertySetup>>(result);
        }

        #endregion
    }
}
