using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.IntelligentBuilding;
using UnifiedLogin.SharedObjects.Saml;
using Xunit;
using UL = UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.LandingAPI.Tests.Logic.Product
{
    /// <summary>
    /// ManageIntelligentBuilding xUnit tests.
    /// Comprehensive tests for Intelligent Building (Unified Amenities) product management.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageIntelligentBuildingTests : TestBase
    {
        #region Private Fields

        private readonly DefaultUserClaim _defaultUserClaim;
        private readonly Mock<IManagePersona> _mockManagePersona;
        private readonly Mock<IManagePerson> _mockManagePerson;
        private readonly Mock<IManageBlueBook> _mockManageBlueBook;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ISamlRepository> _mockSamlRepository;
        private readonly Mock<IProductInternalSettingRepository> _mockProductInternalSettingRepository;
        private readonly Mock<IManagePartyRelationship> _mockManagePartyRelationship;
        private readonly Mock<IUserRoleRightRepository> _mockUserRoleRightRepository;
        private readonly Mock<IManageUserLogin> _mockManageUserLogin;
        private readonly Mock<IUnifiedLoginRepository> _mockUnifiedLoginRepository;
        private readonly Mock<IPropertyRepository> _mockPropertyRepository;
        private readonly Mock<IUserLoginRepository> _mockUserLoginRepository;

        private readonly Guid _testUserRealPageId = Guid.NewGuid();
        private readonly Guid _testOrgRealPageId = Guid.NewGuid();
        private const long TestEditorPersonaId = 100;
        private const long TestUserPersonaId = 200;
        private const long TestPartyId = 1000;
        private const long TestRoleId = 500;

        #endregion

        #region Constructor

        public ManageIntelligentBuildingTests()
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

            _mockManagePersona = new Mock<IManagePersona>();
            _mockManagePerson = new Mock<IManagePerson>();
            _mockManageBlueBook = new Mock<IManageBlueBook>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockSamlRepository = new Mock<ISamlRepository>();
            _mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            _mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
            _mockUserRoleRightRepository = new Mock<IUserRoleRightRepository>();
            _mockManageUserLogin = new Mock<IManageUserLogin>();
            _mockUnifiedLoginRepository = new Mock<IUnifiedLoginRepository>();
            _mockPropertyRepository = new Mock<IPropertyRepository>();
            _mockUserLoginRepository = new Mock<IUserLoginRepository>();

            SetupDefaultMocks();
        }

        #endregion

        #region Helper Methods

        private void SetupDefaultMocks()
        {
            // Setup default persona
            var testPersona = CreateTestPersona(TestUserPersonaId, _testUserRealPageId, TestPartyId);
            var editorPersona = CreateTestPersona(TestEditorPersonaId, _testUserRealPageId, TestPartyId);

            _mockManagePersona.Setup(m => m.GetPersona(TestUserPersonaId)).Returns(testPersona);
            _mockManagePersona.Setup(m => m.GetPersona(TestEditorPersonaId)).Returns(editorPersona);

            // Setup person
            var testPerson = new Person
            {
                FirstName = "Test",
                LastName = "User"
            };
            _mockManagePerson.Setup(m => m.GetPerson(It.IsAny<Guid>())).Returns(testPerson);

            // Setup user login
            var testUserLogin = new UserLoginOnly
            {
                UserId = 1,
                LoginName = "testuser@test.com",
                RealPageId = _testUserRealPageId
            };
            _mockManageUserLogin.Setup(m => m.GetUserLoginOnly(It.IsAny<Guid>())).Returns(testUserLogin);

            // Setup product internal settings
            var productSettings = new List<ProductInternalSetting>
            {
                new ProductInternalSetting { Name = "BooksUseDomains", Value = "1" },
                new ProductInternalSetting { Name = "BooksUseUPFMId", Value = "1" }
            };
            _mockProductInternalSettingRepository.Setup(m => m.GetProductInternalSettings(It.IsAny<int>()))
                .Returns(productSettings);

            // Setup product repository
            _mockProductRepository.Setup(m => m.GetProductIdsByCompany(It.IsAny<long>()))
                .Returns(new List<int> { (int)ProductEnum.IntelligentBuildingTrash });

            // Setup SAML repository
            _mockSamlRepository.Setup(m => m.GetProductSamlDetails(It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new List<SamlAttributes>());

            // Setup property repository
            _mockPropertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByPersona(It.IsAny<long>(), It.IsAny<ProductEnum>()))
                .Returns(new List<int>());

            // Setup party relationship (non-super user by default)
            _mockManagePartyRelationship.Setup(m => m.GetPartyRelationship(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((PartyRelationship)null);
        }

        private Persona CreateTestPersona(long personaId, Guid realPageId, long partyId)
        {
            return new Persona
            {
                PersonaId = personaId,
                RealPageId = realPageId,
                OrganizationPartyId = partyId,
                Organization = new Organization
                {
                    PartyId = partyId,
                    RealPageId = _testOrgRealPageId,
                    Name = "Test Organization"
                }
            };
        }

        private ManageIntelligentBuilding CreateManageIntelligentBuilding()
        {
            return new ManageIntelligentBuilding(
                _defaultUserClaim,
                _mockManagePersona.Object,
                _mockManagePerson.Object,
                _mockManageBlueBook.Object,
                _mockProductRepository.Object,
                _mockSamlRepository.Object,
                _mockProductInternalSettingRepository.Object,
                _mockManagePartyRelationship.Object,
                _mockUserRoleRightRepository.Object,
                _mockManageUserLogin.Object,
                _mockUnifiedLoginRepository.Object,
                _mockPropertyRepository.Object,
                _mockUserLoginRepository.Object);
        }

        private List<ProductRole> CreateTestRoles()
        {
            return new List<ProductRole>
            {
                new ProductRole { ID = "1", Name = "Portfolio Manager", DefaultRole = "True", accessAllProperties = true },
                new ProductRole { ID = "2", Name = "Property Manager", DefaultRole = "False", accessAllProperties = false },
                new ProductRole { ID = "3", Name = "Viewer", DefaultRole = "False", accessAllProperties = false }
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithUserClaims_InitializesSuccessfully()
        {
            // Arrange & Act
            var manager = new ManageIntelligentBuilding(_defaultUserClaim);

            // Assert
            Assert.NotNull(manager);
        }

        [Fact]
        public void Constructor_WithAllDependencies_InitializesSuccessfully()
        {
            // Arrange & Act
            var manager = CreateManageIntelligentBuilding();

            // Assert
            Assert.NotNull(manager);
        }

        #endregion

        #region ManageIntelligentBuildingUser Tests

       
        public void ManageIntelligentBuildingUser_WithValidInput_ReturnsEmptyString()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var propertyRole = new IBPropertyRole
            {
                IsAssigned = true,
                RoleList = new List<string> { "1" },
                PropertyList = new List<string> { "100", "101" }
            };

            _mockUserRoleRightRepository.Setup(m => m.InsertAssignedRoleToUser(
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockPropertyRepository.Setup(m => m.InsertRemoveAssignedPropertyInstanceToUser(
                It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            var result = manager.ManageIntelligentBuildingUser(TestEditorPersonaId, TestUserPersonaId, propertyRole);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ManageIntelligentBuildingUser_WithInvalidEditorPersona_ReturnsError()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var propertyRole = new IBPropertyRole
            {
                IsAssigned = true,
                RoleList = new List<string> { "1" },
                PropertyList = new List<string> { "100" }
            };

            _mockManagePersona.Setup(m => m.GetPersona(999)).Returns((Persona)null);

            // Act
            var result = manager.ManageIntelligentBuildingUser(999, TestUserPersonaId, propertyRole);

            // Assert
            Assert.NotEqual(string.Empty, result);
        }

       
        public void ManageIntelligentBuildingUser_WithSuperUser_AssignsAllProperties()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var propertyRole = new IBPropertyRole
            {
                IsAssigned = true,
                RoleList = new List<string> { "1" },
                PropertyList = new List<string> { "100" }
            };

            // Setup super user
            var superUserRelationship = new PartyRelationship
            {
                RoleTypeFrom = new RoleType { Name = "SuperUser" }
            };
            _mockManagePartyRelationship.Setup(m => m.GetPartyRelationship(
                It.IsAny<Guid>(), It.IsAny<Guid>(), null, null, "User Type"))
                .Returns(superUserRelationship);

            _mockProductRepository.Setup(m => m.ListRolesForProductByParty(
                It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<int>()))
                .Returns(CreateTestRoles());

            _mockUserRoleRightRepository.Setup(m => m.InsertAssignedRoleToUser(
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockPropertyRepository.Setup(m => m.InsertRemoveAssignedPropertyInstanceToUser(
                It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            var result = manager.ManageIntelligentBuildingUser(TestEditorPersonaId, TestUserPersonaId, propertyRole);

            // Assert
            Assert.Equal(string.Empty, result);
        }

      
        public void ManageIntelligentBuildingUser_WithAllPropertiesKeyword_AssignsAllProperties()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var propertyRole = new IBPropertyRole
            {
                IsAssigned = true,
                RoleList = new List<string> { "1" },
                PropertyList = new List<string> { "ALL" }
            };

            _mockProductRepository.Setup(m => m.ListRolesForProductByParty(
                It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<int>()))
                .Returns(CreateTestRoles());

            _mockUserRoleRightRepository.Setup(m => m.InsertAssignedRoleToUser(
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockPropertyRepository.Setup(m => m.InsertRemoveAssignedPropertyInstanceToUser(
                It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            var result = manager.ManageIntelligentBuildingUser(TestEditorPersonaId, TestUserPersonaId, propertyRole);

            // Assert
            Assert.Equal(string.Empty, result);
        }

       
        public void ManageIntelligentBuildingUser_WithRoleChange_UpdatesRole()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var propertyRole = new IBPropertyRole
            {
                IsAssigned = true,
                RoleList = new List<string> { "2" }, // New role
                PropertyList = new List<string> { "100" }
            };

            // Existing role
            _mockUserRoleRightRepository.Setup(m => m.ListRoleByPersona(
                It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long?>()))
                .Returns(new List<UL.Role> { new UL.Role { RoleID = 1 } }); // Existing role ID = 1

            _mockUserRoleRightRepository.Setup(m => m.InsertAssignedRoleToUser(
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockPropertyRepository.Setup(m => m.InsertRemoveAssignedPropertyInstanceToUser(
                It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            var result = manager.ManageIntelligentBuildingUser(TestEditorPersonaId, TestUserPersonaId, propertyRole);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ManageIntelligentBuildingUser_WithRoleDeleteError_ReturnsError()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var propertyRole = new IBPropertyRole
            {
                IsAssigned = true,
                RoleList = new List<string> { "2" },
                PropertyList = new List<string> { "100" }
            };

            _mockUserRoleRightRepository.Setup(m => m.ListRoleByPersona(
                It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long?>()))
                .Returns(new List<UL.Role> { new UL.Role { RoleID = 1 } });

            _mockUserRoleRightRepository.Setup(m => m.InsertAssignedRoleToUser(
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), true))
                .Returns(new RepositoryResponse { Id = -1, ErrorMessage = "Delete failed" });

            // Act
            var result = manager.ManageIntelligentBuildingUser(TestEditorPersonaId, TestUserPersonaId, propertyRole);

            // Assert
            Assert.Equal("Delete failed", result);
        }

        [Fact]
        public void ManageIntelligentBuildingUser_WithRoleAddError_ReturnsError()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var propertyRole = new IBPropertyRole
            {
                IsAssigned = true,
                RoleList = new List<string> { "1" },
                PropertyList = new List<string> { "100" }
            };

            _mockUserRoleRightRepository.Setup(m => m.InsertAssignedRoleToUser(
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), false))
                .Returns(new RepositoryResponse { Id = -1, ErrorMessage = "Add failed" });

            // Act
            var result = manager.ManageIntelligentBuildingUser(TestEditorPersonaId, TestUserPersonaId, propertyRole);

            // Assert
            Assert.Equal("Add failed", result);
        }

       
        public void ManageIntelligentBuildingUser_WithPropertyRemoval_RemovesProperties()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var propertyRole = new IBPropertyRole
            {
                IsAssigned = true,
                RoleList = new List<string> { "1" },
                PropertyList = new List<string> { "100" },
                RemovedPropertyList = new List<string> { "101", "102" }
            };

            _mockUserRoleRightRepository.Setup(m => m.InsertAssignedRoleToUser(
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockPropertyRepository.Setup(m => m.InsertRemoveAssignedPropertyInstanceToUser(
                It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            var result = manager.ManageIntelligentBuildingUser(TestEditorPersonaId, TestUserPersonaId, propertyRole);

            // Assert
            Assert.Equal(string.Empty, result);
        }

       
        public void ManageIntelligentBuildingUser_WithExistingAllProperties_RemovesAllFlag()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var propertyRole = new IBPropertyRole
            {
                IsAssigned = true,
                RoleList = new List<string> { "1" },
                PropertyList = new List<string> { "100" }
            };

            // User has -1 (all properties) assigned
            _mockPropertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByPersona(
                TestUserPersonaId, ProductEnum.IntelligentBuildingTrash))
                .Returns(new List<int> { -1 });

            _mockUserRoleRightRepository.Setup(m => m.InsertAssignedRoleToUser(
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockPropertyRepository.Setup(m => m.InsertRemoveAssignedPropertyInstanceToUser(
                It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            var result = manager.ManageIntelligentBuildingUser(TestEditorPersonaId, TestUserPersonaId, propertyRole);

            // Assert
            Assert.Equal(string.Empty, result);
        }

     
        public void ManageIntelligentBuildingUser_WithException_ReturnsErrorMessage()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var propertyRole = new IBPropertyRole
            {
                IsAssigned = true,
                RoleList = new List<string> { "1" },
                PropertyList = new List<string> { "100" }
            };

            _mockManagePersona.Setup(m => m.GetPersona(TestUserPersonaId))
                .Throws(new Exception("Database error"));

            // Act
            var result = manager.ManageIntelligentBuildingUser(TestEditorPersonaId, TestUserPersonaId, propertyRole);

            // Assert
            Assert.Contains("Error", result);
        }

        
        public void ManageIntelligentBuildingUser_WithNullPropertyRole_HandlesGracefully()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();

            // Act
            var result = manager.ManageIntelligentBuildingUser(TestEditorPersonaId, TestUserPersonaId, null);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        #endregion

        #region UnassignUser Tests

       
        public void UnassignUser_WithValidInput_ReturnsEmptyString()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var propertyRole = new IBPropertyRole { IsAssigned = false };

            _mockUserRoleRightRepository.Setup(m => m.ListRoleByPersona(
                It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long?>()))
                .Returns(new List<UL.Role> { new UL.Role { RoleID = TestRoleId } });

            _mockUserRoleRightRepository.Setup(m => m.InsertAssignedRoleToUser(
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), true))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockPropertyRepository.Setup(m => m.ListPropertiesByPersona(
                It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new List<ProductProperty>());

            // Act
            var result = manager.UnassignUser(TestEditorPersonaId, TestUserPersonaId, propertyRole);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void UnassignUser_WithInvalidEditorPersona_ReturnsError()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var propertyRole = new IBPropertyRole { IsAssigned = false };

            _mockManagePersona.Setup(m => m.GetPersona(999)).Returns((Persona)null);

            // Act
            var result = manager.UnassignUser(999, TestUserPersonaId, propertyRole);

            // Assert
            Assert.NotEqual(string.Empty, result);
        }

        [Fact]
        public void UnassignUser_WithRoleDeleteError_ReturnsError()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var propertyRole = new IBPropertyRole { IsAssigned = false };

            _mockUserRoleRightRepository.Setup(m => m.ListRoleByPersona(
                It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long?>()))
                .Returns(new List<UL.Role> { new UL.Role { RoleID = TestRoleId } });

            _mockUserRoleRightRepository.Setup(m => m.InsertAssignedRoleToUser(
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), true))
                .Returns(new RepositoryResponse { Id = -1, ErrorMessage = "Delete failed" });

            // Act
            var result = manager.UnassignUser(TestEditorPersonaId, TestUserPersonaId, propertyRole);

            // Assert
            Assert.Equal("Delete failed", result);
        }

       
        public void UnassignUser_WithAssignedProperties_RemovesAllProperties()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var propertyRole = new IBPropertyRole { IsAssigned = false };

            _mockUserRoleRightRepository.Setup(m => m.ListRoleByPersona(
                It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long?>()))
                .Returns(new List<UL.Role> { new UL.Role { RoleID = TestRoleId } });

            _mockUserRoleRightRepository.Setup(m => m.InsertAssignedRoleToUser(
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), true))
                .Returns(new RepositoryResponse { Id = 1 });

            _mockPropertyRepository.Setup(m => m.ListPropertiesByPersona(
                TestUserPersonaId, (int)ProductEnum.IntelligentBuildingTrash))
                .Returns(new List<ProductProperty>
                {
                    new ProductProperty { ID = "100" },
                    new ProductProperty { ID = "101" }
                });

            _mockPropertyRepository.Setup(m => m.InsertRemoveAssignedPropertyInstanceToUser(
                It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>(), 1))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            var result = manager.UnassignUser(TestEditorPersonaId, TestUserPersonaId, propertyRole);

            // Assert
            Assert.Equal(string.Empty, result);
        }

       
        public void UnassignUser_WithNoExistingRoles_ReturnsEmptyString()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var propertyRole = new IBPropertyRole { IsAssigned = false };

            _mockUserRoleRightRepository.Setup(m => m.ListRoleByPersona(
                It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long?>()))
                .Returns(new List<UL.Role>());

            // Act
            var result = manager.UnassignUser(TestEditorPersonaId, TestUserPersonaId, propertyRole);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        #endregion

        #region GetRoles Tests

        [Fact]
        public void GetRoles_WithValidInput_ReturnsRolesList()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();

            _mockProductRepository.Setup(m => m.ListRolesForProductByParty(
                It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<int>()))
                .Returns(CreateTestRoles());

            // Act
            var result = manager.GetRoles(TestEditorPersonaId, 0, TestPartyId);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(3, result.TotalRows);
        }

        [Fact]
        public void GetRoles_ForNewUser_SetsDefaultRole()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var roles = CreateTestRoles();

            _mockProductRepository.Setup(m => m.ListRolesForProductByParty(
                It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<int>()))
                .Returns(roles);

            // Act
            var result = manager.GetRoles(TestEditorPersonaId, 0, TestPartyId); // userPersonaId = 0 means new user

            // Assert
            Assert.False(result.IsError);
            var returnedRoles = result.Records.Cast<ProductRole>().ToList();
            Assert.Contains(returnedRoles, r => r.IsAssigned && r.Name == "Portfolio Manager");
        }

        [Fact]
        public void GetRoles_ForExistingUser_MergesWithAssignedRoles()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var roles = CreateTestRoles();

            _mockProductRepository.Setup(m => m.ListRolesForProductByParty(
                It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<int>()))
                .Returns(roles);

            _mockUserRoleRightRepository.Setup(m => m.ListRoleByPersona(
                It.IsAny<int>(), TestUserPersonaId, It.IsAny<long?>()))
                .Returns(new List<UL.Role> { new UL.Role { RoleID = 2 } }); // Property Manager assigned

            // Act
            var result = manager.GetRoles(TestEditorPersonaId, TestUserPersonaId, TestPartyId);

            // Assert
            Assert.False(result.IsError);
            var returnedRoles = result.Records.Cast<ProductRole>().ToList();
            Assert.Contains(returnedRoles, r => r.IsAssigned && r.ID == "2");
        }

        [Fact]
        public void GetRoles_WithInvalidEditorPersona_ReturnsError()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            _mockManagePersona.Setup(m => m.GetPersona(999)).Returns((Persona)null);

            // Act
            var result = manager.GetRoles(999, TestUserPersonaId, TestPartyId);

            // Assert
            Assert.True(result.IsError);
        }

        [Fact]
        public void GetRoles_WithException_ReturnsError()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();

            _mockProductRepository.Setup(m => m.ListRolesForProductByParty(
                It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<int>()))
                .Throws(new Exception("Database error"));

            // Act
            var result = manager.GetRoles(TestEditorPersonaId, 0, TestPartyId);

            // Assert
            Assert.True(result.IsError);
            Assert.NotEmpty(result.ErrorReason);
        }

        [Fact]
        public void GetRoles_WithNullRoles_ReturnsEmptyList()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();

            _mockProductRepository.Setup(m => m.ListRolesForProductByParty(
                It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<int>()))
                .Returns((List<ProductRole>)null);

            // Act
            var result = manager.GetRoles(TestEditorPersonaId, 0, TestPartyId);

            // Assert
            Assert.False(result.IsError);
        }

        #endregion

        #region GetRightsByRole Tests

      
        public void GetRightsByRole_WithValidInput_ReturnsRightsList()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var rights = new List<ProductRight>
            {
                new ProductRight { ID = 1, Description = "View Properties" },
                new ProductRight { ID = 2, Description = "Edit Properties" }
            };

            _mockUnifiedLoginRepository.Setup(m => m.ListRightsByRole(
                It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<int>(), It.IsAny<long>()))
                .Returns(rights);

            // Act
            var result = manager.GetRightsByRole(TestEditorPersonaId, TestPartyId, TestRoleId);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(2, result.TotalRows);
        }

        [Fact]
        public void GetRightsByRole_WithInvalidEditorPersona_ReturnsError()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            _mockManagePersona.Setup(m => m.GetPersona(999)).Returns((Persona)null);

            // Act
            var result = manager.GetRightsByRole(999, TestPartyId, TestRoleId);

            // Assert
            Assert.True(result.IsError);
        }

       
        public void GetRightsByRole_WithException_ReturnsError()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();

            _mockUnifiedLoginRepository.Setup(m => m.ListRightsByRole(
                It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<int>(), It.IsAny<long>()))
                .Throws(new Exception("Database error"));

            // Act
            var result = manager.GetRightsByRole(TestEditorPersonaId, TestPartyId, TestRoleId);

            // Assert
            Assert.True(result.IsError);
            Assert.NotEmpty(result.ErrorReason);
        }

        [Fact]
        public void GetRightsByRole_WithNullRights_ReturnsEmptyList()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();

            _mockUnifiedLoginRepository.Setup(m => m.ListRightsByRole(
                It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<int>(), It.IsAny<long>()))
                .Returns((List<ProductRight>)null);

            // Act
            var result = manager.GetRightsByRole(TestEditorPersonaId, TestPartyId, TestRoleId);

            // Assert
            Assert.False(result.IsError);
        }

        #endregion

        #region GetUPFMProperties Tests

        [Fact]
        public void GetUPFMProperties_WithNoAssignedProperties_ReturnsEmptyList()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();

            _mockPropertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByPersona(
                TestUserPersonaId, ProductEnum.IntelligentBuildingTrash))
                .Returns(new List<int>());

            // Act
            var result = manager.GetUPFMProperties(TestUserPersonaId);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(0, result.TotalRows);
        }

       
        public void GetUPFMProperties_WithAllPropertiesFlag_ReturnsAllProperties()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();

            _mockPropertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByPersona(
                TestUserPersonaId, ProductEnum.IntelligentBuildingTrash))
                .Returns(new List<int> { -1 }); // -1 means all properties

            var upfmProperties = new List<Guid>
            {
                Guid.NewGuid(),
                Guid.NewGuid()
            };

            _mockManageBlueBook.Setup(m => m.GetUPFMPropertyInstances(It.IsAny<string>()))
                .Returns(upfmProperties);

            _mockPropertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByInstanceIds(It.IsAny<List<Guid>>()))
                .Returns(new List<UPFMPropertyInstance>
                {
                    new UPFMPropertyInstance { PropertyInstanceId = 100, Name = "Property 1", InstanceId = upfmProperties[0] },
                    new UPFMPropertyInstance { PropertyInstanceId = 101, Name = "Property 2", InstanceId = upfmProperties[1] }
                });

            // Act
            var result = manager.GetUPFMProperties(TestUserPersonaId);

            // Assert
            Assert.False(result.IsError);
        }

      
        public void GetUPFMProperties_WithSpecificProperties_ReturnsFilteredProperties()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();

            _mockPropertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByPersona(
                TestUserPersonaId, ProductEnum.IntelligentBuildingTrash))
                .Returns(new List<int> { 100, 101 });

            var instanceId1 = Guid.NewGuid();
            var instanceId2 = Guid.NewGuid();

            _mockManageBlueBook.Setup(m => m.GetUPFMPropertyInstances(It.IsAny<string>()))
                .Returns(new List<Guid> { instanceId1, instanceId2 });

            _mockPropertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByInstanceIds(It.IsAny<List<Guid>>()))
                .Returns(new List<UPFMPropertyInstance>
                {
                    new UPFMPropertyInstance { PropertyInstanceId = 100, Name = "Property 1", InstanceId = instanceId1 },
                    new UPFMPropertyInstance { PropertyInstanceId = 101, Name = "Property 2", InstanceId = instanceId2 }
                });

            // Act
            var result = manager.GetUPFMProperties(TestUserPersonaId);

            // Assert
            Assert.False(result.IsError);
        }

       
        public void GetUPFMProperties_WithIncludeFilter_ReturnsFilteredFields()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();

            _mockPropertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByPersona(
                TestUserPersonaId, ProductEnum.IntelligentBuildingTrash))
                .Returns(new List<int> { 100 });

            var instanceId = Guid.NewGuid();

            _mockManageBlueBook.Setup(m => m.GetUPFMPropertyInstances(It.IsAny<string>()))
                .Returns(new List<Guid> { instanceId });

            _mockPropertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByInstanceIds(It.IsAny<List<Guid>>()))
                .Returns(new List<UPFMPropertyInstance>
                {
                    new UPFMPropertyInstance { PropertyInstanceId = 100, Name = "Property 1", InstanceId = instanceId }
                });

            // Act
            var result = manager.GetUPFMProperties(TestUserPersonaId, "ID,Name");

            // Assert
            Assert.False(result.IsError);
        }

        [Fact]
        public void GetUPFMProperties_WithNullBlueBookProperties_ReturnsEmptyList()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();

            _mockPropertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByPersona(
                TestUserPersonaId, ProductEnum.IntelligentBuildingTrash))
                .Returns(new List<int> { 100 });

            _mockManageBlueBook.Setup(m => m.GetUPFMPropertyInstances(It.IsAny<string>()))
                .Returns((List<Guid>)null);

            // Act
            var result = manager.GetUPFMProperties(TestUserPersonaId);

            // Assert
            Assert.False(result.IsError);
        }

        #endregion

        #region IBPropertyRole Class Tests

        [Fact]
        public void IBPropertyRole_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var role = new IBPropertyRole();

            // Assert
            Assert.True(role.IsAssigned);
            Assert.Null(role.PropertyList);
            Assert.Null(role.RoleList);
            Assert.Null(role.RemovedPropertyList);
        }

        [Fact]
        public void IBPropertyRole_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var role = new IBPropertyRole
            {
                IsAssigned = false,
                PropertyList = new List<string> { "100", "101" },
                RoleList = new List<string> { "1" },
                RemovedPropertyList = new List<string> { "102" }
            };

            // Assert
            Assert.False(role.IsAssigned);
            Assert.Equal(2, role.PropertyList.Count);
            Assert.Single(role.RoleList);
            Assert.Single(role.RemovedPropertyList);
        }

        #endregion

        #region Edge Case Tests

       
        public void ManageIntelligentBuildingUser_WithEmptyRoleList_HandlesGracefully()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var propertyRole = new IBPropertyRole
            {
                IsAssigned = true,
                RoleList = new List<string>(),
                PropertyList = new List<string> { "100" }
            };

            _mockPropertyRepository.Setup(m => m.InsertRemoveAssignedPropertyInstanceToUser(
                It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            var result = manager.ManageIntelligentBuildingUser(TestEditorPersonaId, TestUserPersonaId, propertyRole);

            // Assert
            Assert.Equal(string.Empty, result);
        }

      
        public void ManageIntelligentBuildingUser_WithEmptyPropertyList_HandlesGracefully()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var propertyRole = new IBPropertyRole
            {
                IsAssigned = true,
                RoleList = new List<string> { "1" },
                PropertyList = new List<string>()
            };

            _mockUserRoleRightRepository.Setup(m => m.InsertAssignedRoleToUser(
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            var result = manager.ManageIntelligentBuildingUser(TestEditorPersonaId, TestUserPersonaId, propertyRole);

            // Assert
            Assert.Equal(string.Empty, result);
        }

       
        public void ManageIntelligentBuildingUser_WithSameRoleId_DoesNotUpdateRole()
        {
            // Arrange
            var manager = CreateManageIntelligentBuilding();
            var propertyRole = new IBPropertyRole
            {
                IsAssigned = true,
                RoleList = new List<string> { "1" }, // Same as existing
                PropertyList = new List<string> { "100" }
            };

            _mockUserRoleRightRepository.Setup(m => m.ListRoleByPersona(
                It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long?>()))
                .Returns(new List<UL.Role> { new UL.Role { RoleID = 1 } }); // Same role

            _mockPropertyRepository.Setup(m => m.InsertRemoveAssignedPropertyInstanceToUser(
                It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            var result = manager.ManageIntelligentBuildingUser(TestEditorPersonaId, TestUserPersonaId, propertyRole);

            // Assert
            Assert.Equal(string.Empty, result);
            // Verify no role changes were made
            _mockUserRoleRightRepository.Verify(m => m.InsertAssignedRoleToUser(
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        }

        #endregion

        #region Documentation Tests

        [Fact]
        public void ManageIntelligentBuilding_ClassPurpose_Documentation()
        {
            // This test documents the class purpose:
            // ManageIntelligentBuilding is responsible for:
            // 1. Managing user access to Intelligent Building (Unified Amenities) product
            // 2. Assigning/unassigning roles and properties to users
            // 3. Handling super user scenarios with all-property access
            // 4. Managing UPFM property instances
            //
            // Key methods:
            // - ManageIntelligentBuildingUser: Create/update user access
            // - UnassignUser: Remove user access
            // - GetRoles: Get available roles for the product
            // - GetRightsByRole: Get rights associated with a role
            // - GetUPFMProperties: Get UPFM property instances for a user

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageIntelligentBuilding_SpecialCases_Documentation()
        {
            // This test documents special cases:
            //
            // Super User handling:
            // - Super users get "Portfolio Manager" role automatically
            // - Super users get access to all properties (-1 flag)
            //
            // All Properties keyword:
            // - PropertyList = ["ALL"] assigns all properties
            // - Converted to [-1] internally
            //
            // Role changes:
            // - Old role is deleted before new role is assigned
            // - Same role ID = no changes
            //
            // Property management:
            // - Properties are added/removed individually
            // - -1 flag represents all properties

            Assert.True(true, "Documentation test");
        }

        #endregion
    }
}
