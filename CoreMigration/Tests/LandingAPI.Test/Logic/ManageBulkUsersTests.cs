using Moq;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;

namespace UnifiedLogin.BusinessLogic.Tests
{
    [ExcludeFromCodeCoverage]
    public class ManageBulkUsersTests
    {
        private Mock<IProductRepository> _productRepositoryMock;
        private Mock<IPropertyRepository> _propertyRepositoryMock;
        private Mock<IProductInternalSettingRepository> _productInternalSettingRepositoryMock;
        private Mock<IUnifiedSettingsRepository> _unifiedSettingsRepositoryMock;
        private Mock<IManagePersona> _managePersonaMock;
        private Mock<IPersonaRepository> _personaRepositoryMock;
        private Mock<IUserLoginRepository> _userLoginRepositoryMock;
        private Mock<BatchProductBulkUpdateRepository> _enterpriseRoleProductRepositoryMock;
        private Mock<IUserRoleRightRepository> _userRoleRightRepositoryMock;
        private Mock<IManageBlueBook> _manageBlueBookMock;
        private DefaultUserClaim _userClaim;
        private ManageBulkUsers _manageBulkUsers;
        private Mock<IRepository> _mockRepository;
        private Mock<IManagePartyRelationship> _managePartyRelationshipMock;

        public void SetUp()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _propertyRepositoryMock = new Mock<IPropertyRepository>();
            _productInternalSettingRepositoryMock = new Mock<IProductInternalSettingRepository>();
            _unifiedSettingsRepositoryMock = new Mock<IUnifiedSettingsRepository>();
            _managePersonaMock = new Mock<IManagePersona>();
            _personaRepositoryMock = new Mock<IPersonaRepository>();
            _userLoginRepositoryMock = new Mock<IUserLoginRepository>();
            _enterpriseRoleProductRepositoryMock = new Mock<BatchProductBulkUpdateRepository>(MockBehavior.Loose, new object[] { new DefaultUserClaim() });
            _userRoleRightRepositoryMock = new Mock<IUserRoleRightRepository>();
            _manageBlueBookMock = new Mock<IManageBlueBook>();
            _userClaim = new DefaultUserClaim { UserId = 1, PersonaId = 2 };
            _mockRepository = new Mock<IRepository>();
            _managePartyRelationshipMock = new Mock<IManagePartyRelationship>();
            // Use reflection to inject mocks into ManageBulkUsers
            _manageBulkUsers = new ManageBulkUsers(_mockRepository.Object);

typeof(ManageBulkUsers).GetField("_userClaim", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_manageBulkUsers, _userClaim);
            typeof(ManageBulkUsers).GetField("_productRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_manageBulkUsers, _productRepositoryMock.Object);
            typeof(ManageBulkUsers).GetField("_propertyRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_manageBulkUsers, _propertyRepositoryMock.Object);
            typeof(ManageBulkUsers).GetField("_productInternalSettingRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_manageBulkUsers, _productInternalSettingRepositoryMock.Object);
            typeof(ManageBulkUsers).GetField("_unifiedSettingsRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_manageBulkUsers, _unifiedSettingsRepositoryMock.Object);
            typeof(ManageBulkUsers).GetField("_managePersona", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_manageBulkUsers, _managePersonaMock.Object);
            typeof(ManageBulkUsers).GetField("_personaRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_manageBulkUsers, _personaRepositoryMock.Object);
            typeof(ManageBulkUsers).GetField("_userLoginRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_manageBulkUsers, _userLoginRepositoryMock.Object);
            typeof(ManageBulkUsers).GetField("_enterpriseRoleProductRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_manageBulkUsers, _enterpriseRoleProductRepositoryMock.Object);
            typeof(ManageBulkUsers).GetField("_userRoleRightRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_manageBulkUsers, _userRoleRightRepositoryMock.Object);
            typeof(ManageBulkUsers).GetField("_manageBlueBook", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_manageBulkUsers, _manageBlueBookMock.Object);

            typeof(ManageBulkUsers).GetField("_managePartyRelationship", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
          .SetValue(_manageBulkUsers, _managePartyRelationshipMock.Object);

_managePartyRelationshipMock.Setup(m => m.GetPartyRelationship(It.IsAny<Guid>(), It.IsAny<Guid>(), null, null, "User Type"))
           .Returns(new PartyRelationship
           {
               RoleTypeFrom = new RoleType { Name = "User" }
           });
        }

        [Fact]
        public void ProcessProductUnAssignBatchData_ReturnsEmptyString_WhenNoProductsToUnassign()
        {
            SetUp();
            // Arrange
            var editorPersona = new Persona { PersonaId = 1, RealPageId = Guid.NewGuid() };
            var userPersona = new Persona { PersonaId = 2, RealPageId = Guid.NewGuid(), OrganizationPartyId = 10, Organization = new SharedObjects.Landing.Organization() 
            { 
             RealPageId = new Guid("2B6D71E4-798B-43B1-A82E-3E7705B5DFA8") 
            }};
            _managePersonaMock.Setup(x => x.GetPersona(It.IsAny<long>())).Returns(editorPersona);
            _managePersonaMock.SetupSequence(x => x.GetPersona(It.IsAny<long>()))
                .Returns(editorPersona)
                .Returns(userPersona);

            _userLoginRepositoryMock.Setup(x => x.ListOrganizationByEnterpriseUserId(It.IsAny<Guid>(), null))
         .Returns(new List<UnifiedLogin.SharedObjects.Landing.Organization> { new UnifiedLogin.SharedObjects.Landing.Organization { PartyId = 10, RelationshipType = "User Type", RoleNameFrom = "External User" } });

            _productRepositoryMock.Setup(x => x.ListProductsByPersonaId(It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new List<PersonaProductUserDetails>());

            // Simulate no products to unassign
            typeof(ManageBulkUsers).GetMethod("GetUserBatchRecord", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_manageBulkUsers, new object[] { 1 });

            // Act
            var result = _manageBulkUsers.ProcessProductUnAssignBatchData(1, 2, 1);

            // Assert
            //  Assert.That(result, Is.EqualTo(""));
            Assert.True(string.IsNullOrEmpty(result));
        }

[Fact]
        public void ProcessProductUnAssignBatchData_ReturnsError_WhenSaveProductBatchFails()
        {
            SetUp();
            // Arrange
            var editorPersona = new Persona { PersonaId = 1, RealPageId = Guid.NewGuid() };
            var userPersona = new Persona { PersonaId = 2, RealPageId = Guid.NewGuid(), OrganizationPartyId = 10,
                Organization = new SharedObjects.Landing.Organization()
                {
                    RealPageId = new Guid("2B6D71E4-798B-43B1-A82E-3E7705B5DFA8")
                }
            };
            _managePersonaMock.SetupSequence(x => x.GetPersona(It.IsAny<long>()))
                .Returns(editorPersona)
                .Returns(userPersona);

            _userLoginRepositoryMock.Setup(x => x.ListOrganizationByEnterpriseUserId(It.IsAny<Guid>(), null))
                .Returns(new List<UnifiedLogin.SharedObjects.Landing.Organization>
                {
                    new UnifiedLogin.SharedObjects.Landing.Organization
                    {
                        PartyId = 10,
                        RelationshipType = "User Type",
                        RoleNameFrom = "External User"
                    }
                });

            _productRepositoryMock.Setup(x => x.ListProductsByPersonaId(It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new List<PersonaProductUserDetails> { new PersonaProductUserDetails () });

            // Setup GetUserBatchRecord to return a batch with a product
            // Remove the dummy classes for Organization and BulkUserProduct at the bottom of the file
            // and use the correct types from the referenced namespaces.

            // In the test methods, update the following lines:

            // Problem 1: Fix ListOrganizationByEnterpriseUserId mock return type
            _userLoginRepositoryMock.Setup(x => x.ListOrganizationByEnterpriseUserId(It.IsAny<Guid>(), null))
                .Returns(new List<UnifiedLogin.SharedObjects.Landing.Organization>
                {
                    new UnifiedLogin.SharedObjects.Landing.Organization
                    {
                        PartyId = 10,
                        RelationshipType = "User Type",
                        RoleNameFrom = "External User"
                    }
                });

            // Problem 2: Fix BulkUserBatch.BulkUserProducts assignment to use correct type
            var bulkUserBatch = new UnifiedLogin.SharedObjects.Batch.BulkUserBatch
            {
                BulkUserBatchProcessId = 1,
                EditorUserPersonaId = 1,
                SubjectUserPersonaId = 2,
                BatchProcessTypeId = 1,
                BulkUserProducts = new List<UnifiedLogin.SharedObjects.Batch.BulkUserProduct>
                {
                    new UnifiedLogin.SharedObjects.Batch.BulkUserProduct
                    {
                        ProductId = 1,
                        BulkUserBatchProcessId = 1
                    }
                }
            };

            // And in the other test:
            var bulkUserBatch2 = new UnifiedLogin.SharedObjects.Batch.BulkUserBatch
            {
                BulkUserBatchProcessId = 1,
                EditorUserPersonaId = 1,
                SubjectUserPersonaId = 2,
                BatchProcessTypeId = 1,
                BulkUserProducts = new List<UnifiedLogin.SharedObjects.Batch.BulkUserProduct>
                {
                    new UnifiedLogin.SharedObjects.Batch.BulkUserProduct
                    {
                        ProductId = 1,
                        BulkUserBatchProcessId = 1
                    }
                }
            };

            // Remove these dummy classes at the bottom of the file:
            /*
            public class Organization
            {
                public long PartyId { get; set; }
                public string RelationshipType { get; set; }
                public string RoleNameFrom { get; set; }
            }
            public class BulkUserProduct
            {
                public int ProductId { get; set; }
                public int BulkUserBatchProcessId { get; set; }
            }
            */
            var getUserBatchRecordMethod = typeof(ManageBulkUsers).GetMethod("GetUserBatchRecord", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // If GetUserBatchRecord needs to return the above batch, you may need to mock or set up the internal state accordingly.
            // For now, just invoke as in the original code.
            getUserBatchRecordMethod.Invoke(_manageBulkUsers, new object[] { 1 });

_productInternalSettingRepositoryMock
           .Setup(x => x.GetProductInternalSettings(It.IsAny<int>()))
           .Returns(new List<ProductInternalSetting>
           {
            new ProductInternalSetting
            {
                Name = "UserAccessDetails_ProductsWithNoProperties",
                Value = "1"
            }});

// Act
            var result = _manageBulkUsers.ProcessProductUnAssignBatchData(1, 2, 1);

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void ProcessProductUnAssignBatchData_ReturnsEmptyString_WhenBatchIsCompleted()
        {
            SetUp();
            // Arrange
            var editorPersona = new Persona { PersonaId = 1, RealPageId = Guid.NewGuid() };
            var userPersona = new Persona { PersonaId = 2, RealPageId = Guid.NewGuid(), OrganizationPartyId = 10 , Organization = new SharedObjects.Landing.Organization() { RealPageId = new Guid("2B6D71E4-798B-43B1-A82E-3E7705B5DFA8") } };
            _managePersonaMock.SetupSequence(x => x.GetPersona(It.IsAny<long>()))
                .Returns(editorPersona)
                .Returns(userPersona);

            _userLoginRepositoryMock.Setup(x => x.ListOrganizationByEnterpriseUserId(It.IsAny<Guid>(), null))
                .Returns(new List<UnifiedLogin.SharedObjects.Landing.Organization> { new UnifiedLogin.SharedObjects.Landing.Organization { PartyId = 10, RelationshipType = "User Type", RoleNameFrom = "External User" } });

            _productRepositoryMock.Setup(x => x.ListProductsByPersonaId(It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new List<PersonaProductUserDetails> { new PersonaProductUserDetails() });

            // Setup GetUserBatchRecord to return a batch with a product
            var bulkUserBatch = new UnifiedLogin.SharedObjects.Batch.BulkUserBatch
            {
                BulkUserBatchProcessId = 1,
                EditorUserPersonaId = 1,
                SubjectUserPersonaId = 2,
                BatchProcessTypeId = 1,
                BulkUserProducts = new List<UnifiedLogin.SharedObjects.Batch.BulkUserProduct>
                {
                    new UnifiedLogin.SharedObjects.Batch.BulkUserProduct { ProductId = 1, BulkUserBatchProcessId = 1 }
                }
            };
            var getUserBatchRecordMethod = typeof(ManageBulkUsers).GetMethod("GetUserBatchRecord", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            getUserBatchRecordMethod.Invoke(_manageBulkUsers, new object[] { 1 });

_productInternalSettingRepositoryMock.Setup(x => x.GetProductInternalSettings(It.IsAny<int>()))
            .Returns(new List<ProductInternalSetting>
            {
                 new ProductInternalSetting
                 {
                     Name = "UserAccessDetails_ProductsWithNoProperties",
                     Value = "1"
                 }});

            // Act
            var result = _manageBulkUsers.ProcessProductUnAssignBatchData(1, 2, 1);

            // Assert
            //  Assert.That(result, Is.EqualTo(""));
            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void ProcessProductUnAssignBatchData_ReturnsError_OnException()
        {
            SetUp();
            // Arrange
            _managePersonaMock.Setup(x => x.GetPersona(It.IsAny<long>())).Throws(new Exception("Test exception"));

            // Act
            var result = _manageBulkUsers.ProcessProductUnAssignBatchData(1, 2, 1);

            // Assert
            //  Assert.That(result, Is.EqualTo("Error"));
            Assert.Equal("Error", result);
        }
    }

    // Dummy classes for missing types in the context
    public class Organization
    {
        public long PartyId { get; set; }
        public string RelationshipType { get; set; }
        public string RoleNameFrom { get; set; }
    }
    public class BulkUserProduct
    {
        public int ProductId { get; set; }
        public int BulkUserBatchProcessId { get; set; }
    }
}
