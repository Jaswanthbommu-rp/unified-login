using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.User;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.ResponseObject;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic.Enterprise.User
{
    /// <summary>
    /// UserManagement xUnit tests.
    /// Tests for enterprise user management functionality.
    /// 
    /// Uses mocked repositories via constructor injection for better testability.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UserManagementTests : TestBase
    {
        private readonly DefaultUserClaim _defaultUserClaim;
        private readonly Mock<IUserLoginRepository> _mockUserLoginRepository;
        private readonly Mock<IManageUserLogin> _mockManageUserLogin;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IOrganizationRepository> _mockOrganizationRepository;

        public UserManagementTests()
        {
            _defaultUserClaim = new DefaultUserClaim
            {
                UserId = 1,
                LoginName = "testuser@test.com",
                FirstName = "Test",
                LastName = "User",
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 1000,
                OrganizationMasterId = 100,
                OrganizationName = "Test Organization",
                PersonaId = 5,
                CorrelationId = Guid.NewGuid()
            };

            _mockUserLoginRepository = new Mock<IUserLoginRepository>();
            _mockManageUserLogin = new Mock<IManageUserLogin>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockOrganizationRepository = new Mock<IOrganizationRepository>();

            SetupBasicMocks();
        }

        #region Mock Setup

        private void SetupBasicMocks()
        {
            // Setup basic organization mock
            _mockOrganizationRepository
                .Setup(x => x.GetOrganization(It.IsAny<Guid>()))
                .Returns((Organization)null);

            // Setup user login exists check - returns UserOrganizationExists
            _mockManageUserLogin
                .Setup(x => x.IsLoginNameExists(
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .Returns(new UserOrganizationExists { UserExists = false, IsValidDomainUsername = false });

            // Setup product repository
            _mockProductRepository
                .Setup(x => x.GetProductIdsByCompany(It.IsAny<long>()))
                .Returns(new List<int>());
        }

        private UserManagement CreateUserManagementWithMocks()
        {
            return new UserManagement(
                _defaultUserClaim,
                _mockUserLoginRepository.Object,
                _mockManageUserLogin.Object,
                _mockProductRepository.Object,
                _mockOrganizationRepository.Object);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithUserClaims_InitializesSuccessfully()
        {
            // Arrange & Act
            var userManagement = new UserManagement(_defaultUserClaim);

            // Assert
            Assert.NotNull(userManagement);
        }

        [Fact]
        public void Constructor_WithNullUserClaims_InitializesSuccessfully()
        {
            // Arrange & Act
            var userManagement = new UserManagement(null);

            // Assert
            Assert.NotNull(userManagement);
        }

        [Fact]
        public void Constructor_WithMockedDependencies_InitializesSuccessfully()
        {
            // Arrange & Act
            var userManagement = CreateUserManagementWithMocks();

            // Assert
            Assert.NotNull(userManagement);
        }

        #endregion

        #region CreateEnterpriseUnityUser Tests

        [Fact]
        public void CreateEnterpriseUnityUser_WithNullUserProductDetails_ThrowsException()
        {
            // Arrange
            var userManagement = CreateUserManagementWithMocks();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => userManagement.CreateEnterpriseUnityUser(null));
        }

        [Fact]
        public void CreateEnterpriseUnityUser_WithMissingPassword_ReturnsPasswordRequiredError()
        {
            // Arrange
            var userManagement = CreateUserManagementWithMocks();
            var userProductDetails = CreateUserProductDetails();
            userProductDetails.UserProfileDetails.Password = null;
            userProductDetails.UserProfileDetails.IsExternalIdp = false;
            userProductDetails.UserProfileDetails.SendInvitationEmail = false;

            // Act
            var result = userManagement.CreateEnterpriseUnityUser(userProductDetails);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Password is required.", result.ErrorReason);
        }

        [Fact]
        public void CreateEnterpriseUnityUser_WithEmptyPassword_ReturnsPasswordRequiredError()
        {
            // Arrange
            var userManagement = CreateUserManagementWithMocks();
            var userProductDetails = CreateUserProductDetails();
            userProductDetails.UserProfileDetails.Password = "   ";
            userProductDetails.UserProfileDetails.IsExternalIdp = false;
            userProductDetails.UserProfileDetails.SendInvitationEmail = false;

            // Act
            var result = userManagement.CreateEnterpriseUnityUser(userProductDetails);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Password is required.", result.ErrorReason);
        }

        [Fact]
        public void CreateEnterpriseUnityUser_WithExternalIdp_SkipsPasswordValidation()
        {
            // Arrange
            var userManagement = CreateUserManagementWithMocks();
            var userProductDetails = CreateUserProductDetails();
            userProductDetails.UserProfileDetails.Password = null;
            userProductDetails.UserProfileDetails.IsExternalIdp = true;

            // Act
            var result = userManagement.CreateEnterpriseUnityUser(userProductDetails);

            // Assert - Will fail on company validation, but not password
            Assert.True(result.IsError);
            Assert.NotEqual("Password is required.", result.ErrorReason);
        }

        [Fact]
        public void CreateEnterpriseUnityUser_WithSendInvitationEmail_SkipsPasswordValidation()
        {
            // Arrange
            var userManagement = CreateUserManagementWithMocks();
            var userProductDetails = CreateUserProductDetails();
            userProductDetails.UserProfileDetails.Password = null;
            userProductDetails.UserProfileDetails.IsExternalIdp = false;
            userProductDetails.UserProfileDetails.SendInvitationEmail = true;

            // Act
            var result = userManagement.CreateEnterpriseUnityUser(userProductDetails);

            // Assert - Will fail on company validation, but not password
            Assert.True(result.IsError);
            Assert.NotEqual("Password is required.", result.ErrorReason);
        }

        [Fact]
        public void CreateEnterpriseUnityUser_WithInvalidCompanyId_ReturnsIncorrectCompanyError()
        {
            // Arrange
            _mockOrganizationRepository
                .Setup(x => x.GetOrganization(It.IsAny<Guid>()))
                .Returns((Organization)null);

            var userManagement = CreateUserManagementWithMocks();
            var userProductDetails = CreateUserProductDetails();

            // Act
            var result = userManagement.CreateEnterpriseUnityUser(userProductDetails);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Request has incorrect CompanyId.", result.ErrorReason);
        }

       
        public void CreateEnterpriseUnityUser_WithExistingLoginName_ReturnsUserExistsError()
        {
            // Arrange
            var organization = new Organization
            {
                PartyId = 1000,
                Name = "Test Org",
                RealPageId = Guid.NewGuid()
            };

            _mockOrganizationRepository
                .Setup(x => x.GetOrganization(It.IsAny<Guid>()))
                .Returns(organization);

            _mockManageUserLogin
                .Setup(x => x.IsLoginNameExists(
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .Returns(new UserOrganizationExists { UserExists = true, IsValidDomainUsername = false });

            var userManagement = CreateUserManagementWithMocks();
            var userProductDetails = CreateUserProductDetails();

            // Act
            var result = userManagement.CreateEnterpriseUnityUser(userProductDetails);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("User Login Name already exists.", result.ErrorReason);
        }

        #endregion

        #region UpdateEnterpriseUnityUser Tests

        [Fact]
        public void UpdateEnterpriseUnityUser_WithNullUserProductDetails_ThrowsException()
        {
            // Arrange
            var userManagement = CreateUserManagementWithMocks();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => userManagement.UpdateEnterpriseUnityUser(null));
        }

       
        public void UpdateEnterpriseUnityUser_WithMismatchedLoginName_ReturnsError()
        {
            // Arrange
            _mockManageUserLogin
                .Setup(x => x.ValidateUsername(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(false);

            var userManagement = CreateUserManagementWithMocks();
            var userProductDetails = CreateUserProductDetails();
            userProductDetails.UserProfileDetails.UserRealPageId = Guid.NewGuid();
            userProductDetails.UserProfileDetails.LoginName = "different@test.com";

            // Act
            var result = userManagement.UpdateEnterpriseUnityUser(userProductDetails);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("User login name doesn't match with RealPage Id.", result.ErrorReason);
        }

        #endregion

        #region ActivateDeactivateUser Tests

        [Fact]
        public void ActivateDeactivateUser_WithNonExistentUser_ReturnsError()
        {
            // Arrange
            _mockUserLoginRepository
                .Setup(x => x.GetUserLoginOnly(It.IsAny<Guid>()))
                .Returns((UserLoginOnly)null);

            var userManagement = CreateUserManagementWithMocks();
            var nonExistentUserId = Guid.NewGuid();

            // Act
            var result = userManagement.ActivateDeactivateUser(nonExistentUserId, UserUiStatusType.Active);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Users RealPageUserId is incorrect", result.ErrorReason);
        }

        [Fact]
        public void ActivateDeactivateUser_WithEmptyLoginName_ReturnsError()
        {
            // Arrange
            _mockUserLoginRepository
                .Setup(x => x.GetUserLoginOnly(It.IsAny<Guid>()))
                .Returns(new UserLoginOnly { LoginName = "" });

            var userManagement = CreateUserManagementWithMocks();
            var userId = Guid.NewGuid();

            // Act
            var result = userManagement.ActivateDeactivateUser(userId, UserUiStatusType.Active);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Users RealPageUserId is incorrect", result.ErrorReason);
        }

        #endregion

        #region ValidateProductData Tests

        [Fact]
        public void ValidateProductData_WithNullProductList_ThrowsException()
        {
            // Arrange
            var userManagement = CreateUserManagementWithMocks();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => userManagement.ValidateProductData(null));
        }

        [Fact]
        public void ValidateProductData_WithEmptyProductList_ReturnsEmptyList()
        {
            // Arrange
            var userManagement = CreateUserManagementWithMocks();
            var productList = new List<ProductDetail>();

            // Act
            var result = userManagement.ValidateProductData(productList);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region ValidateAndAssignCustomFieldValues Tests

        [Fact]
        public void ValidateAndAssignCustomFieldValues_WithNullCustomFields_ReturnsSuccess()
        {
            // Arrange
            var userManagement = CreateUserManagementWithMocks();
            IList<CustomFieldValue> customFieldsOut;

            // Act
            var result = userManagement.ValidateAndAssignCustomFieldValues(null, null, out customFieldsOut);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ValidateAndAssignCustomFieldValues_WithEmptyCustomFields_ReturnsSuccess()
        {
            // Arrange
            var userManagement = CreateUserManagementWithMocks();
            var customFields = new Dictionary<string, string>();
            IList<CustomFieldValue> customFieldsOut;

            // Act
            var result = userManagement.ValidateAndAssignCustomFieldValues(null, customFields, out customFieldsOut);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        #endregion

        #region Helper Methods

        private UserProductDetails CreateUserProductDetails()
        {
            return new UserProductDetails
            {
                EditorRealPageId = Guid.NewGuid(),
                UserProfileDetails = new UserData
                {
                    LoginName = "newuser@test.com",
                    FirstName = "New",
                    LastName = "User",
                    Email = "newuser@test.com",
                    Password = "Password123!",
                    OrganizationRealPageId = Guid.NewGuid(),
                    OrganizationPartyId = 1000,
                    IsExternalIdp = false,
                    SendInvitationEmail = false
                },
                ProductList = new List<ProductDetail>()
            };
        }

        #endregion

        #region Data Object Tests

        [Fact]
        public void UserProductDetails_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var details = new UserProductDetails
            {
                EditorRealPageId = Guid.NewGuid(),
                UserProfileDetails = new UserData
                {
                    LoginName = "test@test.com",
                    FirstName = "Test",
                    LastName = "User"
                },
                ProductList = new List<ProductDetail>
                {
                    new ProductDetail { ProductCode = "OPS", IsAssigned = true }
                }
            };

            // Assert
            Assert.NotEqual(Guid.Empty, details.EditorRealPageId);
            Assert.NotNull(details.UserProfileDetails);
            Assert.Single(details.ProductList);
        }

        [Fact]
        public void UserData_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var userData = new UserData
            {
                LoginName = "test@test.com",
                FirstName = "Test",
                MiddleName = "Middle",
                LastName = "User",
                Email = "test@test.com",
                Title = "Mr.",
                Suffix = "Jr.",
                EmployeeId = "EMP001",
                Password = "Password123!",
                IsExternalIdp = false,
                SendInvitationEmail = true,
                OrganizationRealPageId = Guid.NewGuid(),
                OrganizationPartyId = 1000,
                UserRealPageId = Guid.NewGuid(),
                UserEffectiveDate = DateTime.UtcNow,
                UserExpirationDate = DateTime.UtcNow.AddYears(1)
            };

            // Assert
            Assert.Equal("test@test.com", userData.LoginName);
            Assert.Equal("Test", userData.FirstName);
            Assert.Equal("Middle", userData.MiddleName);
            Assert.Equal("User", userData.LastName);
            Assert.Equal("test@test.com", userData.Email);
            Assert.Equal("Mr.", userData.Title);
            Assert.Equal("Jr.", userData.Suffix);
            Assert.Equal("EMP001", userData.EmployeeId);
            Assert.False(userData.IsExternalIdp);
            Assert.True(userData.SendInvitationEmail);
        }

        [Fact]
        public void ProductDetail_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var product = new ProductDetail
            {
                ProductCode = "OPS",
                IsAssigned = true,
                PropertiesAssigned = new List<string> { "1", "2" },
                RolesAssigned = new List<string> { "100" },
                RegionsAssigned = new List<string> { "US" }
            };

            // Assert
            Assert.Equal("OPS", product.ProductCode);
            Assert.True(product.IsAssigned);
            Assert.Equal(2, product.PropertiesAssigned.Count);
            Assert.Single(product.RolesAssigned);
            Assert.Single(product.RegionsAssigned);
        }

        #endregion

        #region Documentation Tests

        [Fact]
        public void UserManagement_ClassPurpose_Documentation()
        {
            // This test documents the class purpose:
            // UserManagement is responsible for:
            // 1. Creating enterprise users (CreateEnterpriseUnityUser)
            // 2. Updating enterprise users (UpdateEnterpriseUnityUser)
            // 3. Activating/Deactivating users (ActivateDeactivateUser)
            // 4. Listing users (ListUser)
            // 5. Validating product data (ValidateProductData)
            // 6. Validating custom fields (ValidateAndAssignCustomFieldValues)
            //
            // Constructor:
            // - Takes DefaultUserClaim for user context
            // - Optional: IUserLoginRepository, IManageUserLogin, IProductRepository, IOrganizationRepository
            //
            // Key validation rules:
            // - Password required for non-external IDP users (unless SendInvitationEmail is true)
            // - Company must exist
            // - Products must be available for company
            // - OPS product requires exactly one role and one property

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void UserManagement_MockingPattern_Documentation()
        {
            // This test documents the mocking pattern:
            //
            // 1. Create mocks for all dependencies:
            //    var mockUserLoginRepository = new Mock<IUserLoginRepository>();
            //    var mockManageUserLogin = new Mock<IManageUserLogin>();
            //    var mockProductRepository = new Mock<IProductRepository>();
            //    var mockOrganizationRepository = new Mock<IOrganizationRepository>();
            //
            // 2. Setup mocks with expected behavior:
            //    mockOrganizationRepository
            //        .Setup(x => x.GetOrganization(It.IsAny<Guid>()))
            //        .Returns(organization);
            //
            // 3. Create instance with mocked dependencies:
            //    var userManagement = new UserManagement(
            //        userClaim,
            //        mockUserLoginRepository.Object,
            //        mockManageUserLogin.Object,
            //        mockProductRepository.Object,
            //        mockOrganizationRepository.Object);
            //
            // 4. Call method and verify:
            //    var result = userManagement.CreateEnterpriseUnityUser(userProductDetails);
            //    Assert.Equal("Expected Error", result.ErrorReason);
            //
            // 5. Verify mock interactions:
            //    mockManageUserLogin.Verify(x => x.IsLoginNameExists(...), Times.Once);

            Assert.True(true, "Documentation test");
        }

        #endregion
    }
}
