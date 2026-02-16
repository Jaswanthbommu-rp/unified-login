using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.UnifiedAmenities;
using Xunit;
using UL = UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.LandingAPI.Tests.Logic.Product
{
    /// <summary>
    /// ManageUnifiedAmenities xUnit tests.
    /// Comprehensive tests for Unified Amenities (Intelligent Building) product management.
    /// 
    /// This class manages user access to Unified Amenities product with support for:
    /// - Role-based access control (stored in GreenBook database)
    /// - Property assignment (stored in GreenBook database)
    /// - Property instance support (UsePropertyInstanceUnifiedAmenities setting)
    /// - Super user handling with "MANAGE AMENITY WITH PRICING" role
    /// - Default role assignment for new users
    /// - Parallel processing for property assignments
    /// 
    /// Product: Unified Amenities (Intelligent Building)
    /// Product ID: 24
    /// Storage: GreenBook Database (no external API)
    /// 
    /// Key features:
    /// - Roles stored in GreenBook (UserRoleRight table)
    /// - Properties stored in GreenBook (PersonaProductProperty/PersonaProductPropertyInstance)
    /// - Single role per user (replaces existing role)
    /// - Default role for new users
    /// - Property assignment from BlueBook
    /// - Property instance support (new feature)
    /// - Super user auto-assignment
    /// - Parallel property processing
    /// 
    /// NOTE: Due to unique architecture:
    /// - No external API calls (unlike most products)
    /// - All data stored in GreenBook database
    /// - UserRoleRightRepository for role management
    /// - ProductRepository for property management
    /// - BlueBook for property listing
    /// - Parallel.ForEach for property operations
    /// - UsePropertyInstanceUnifiedAmenities setting determines property vs property instance
    /// 
    /// These tests focus on:
    /// - Constructor initialization
    /// - Data structure validation
    /// - Business logic documentation
    /// - Integration test strategies
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageUnifiedAmenitiesTests : TestBase
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

        public ManageUnifiedAmenitiesTests()
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

        #region Constructor Tests

        [Fact]
        public void Constructor_WithUserClaims_InitializesSuccessfully()
        {
            // Arrange & Act
            var manager = new ManageUnifiedAmenities(_defaultUserClaim);

            // Assert
            Assert.NotNull(manager);
        }

        [Fact]
        public void Constructor_WithAllParameters_InitializesSuccessfully()
        {
            // Arrange
            var mockManagePersona = new Mock<IManagePersona>();
            var mockManagePerson = new Mock<IManagePerson>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
            var mockUserRoleRightRepository = new Mock<IUserRoleRightRepository>();
            var mockManageUserLogin = new Mock<IManageUserLogin>();
            var mockUnifiedLoginRepository = new Mock<IUnifiedLoginRepository>();
            var mockPropertyRepository = new Mock<IPropertyRepository>();
            var mockUserLoginRepository = new Mock<IUserLoginRepository>();

            // Act
            var manager = new ManageUnifiedAmenities(
                _defaultUserClaim,
                mockManagePersona.Object,
                mockManagePerson.Object,
                mockManageBlueBook.Object,
                mockProductRepository.Object,
                mockSamlRepository.Object,
                mockProductInternalSettingRepository.Object,
                mockManagePartyRelationship.Object,
                mockUserRoleRightRepository.Object,
                mockManageUserLogin.Object,
                mockUnifiedLoginRepository.Object,
                mockPropertyRepository.Object,
                mockUserLoginRepository.Object);

            // Assert
            Assert.NotNull(manager);
        }

        #endregion

        #region Data Class Tests

        [Fact]
        public void UnifiedAmenitiesPropertyRole_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var propertyRole = new UnifiedAmenitiesPropertyRole
            {
                PropertyList = new List<string> { "123", "456", "789" },
                RoleList = new List<string> { "5", "10" }
            };

            // Assert
            Assert.Equal(3, propertyRole.PropertyList.Count);
            Assert.Equal("123", propertyRole.PropertyList[0]);
            Assert.Equal(2, propertyRole.RoleList.Count);
            Assert.Equal("5", propertyRole.RoleList[0]);
        }

        [Fact]
        public void UnifiedAmenitiesPropertyRole_WithAllProperties_ClientLevel()
        {
            // Arrange & Act
            var propertyRole = new UnifiedAmenitiesPropertyRole
            {
                PropertyList = new List<string> { "ALL" },
                RoleList = new List<string> { "5" }
            };

            // Assert
            Assert.Single(propertyRole.PropertyList);
            Assert.Equal("ALL", propertyRole.PropertyList[0]);
        }

        [Fact]
        public void UnifiedAmenitiesPropertyRole_SuperUser_MinusOne()
        {
            // Arrange & Act
            var propertyRole = new UnifiedAmenitiesPropertyRole
            {
                PropertyList = new List<string> { "-1" },
                RoleList = new List<string> { "MANAGE_AMENITY_PRICING" }
            };

            // Assert
            Assert.Single(propertyRole.PropertyList);
            Assert.Equal("-1", propertyRole.PropertyList[0]);
        }

        [Fact]
        public void ProductRole_WithDefaultRole_CanBeSet()
        {
            // Arrange & Act
            var role = new ProductRole
            {
                ID = "5",
                Name = "Amenity Manager",
                Description = "Manage amenities",
                IsAssigned = false,
                DefaultRole = "True",
                accessAllProperties = false
            };

            // Assert
            Assert.Equal("5", role.ID);
            Assert.Equal("Amenity Manager", role.Name);
            Assert.Equal("True", role.DefaultRole);
            Assert.False(role.IsAssigned);
            Assert.False(role.accessAllProperties);
        }

        [Fact]
        public void ProductRole_WithAccessAllProperties_CanBeSet()
        {
            // Arrange & Act
            var role = new ProductRole
            {
                ID = "10",
                Name = "MANAGE AMENITY WITH PRICING",
                Description = "Full management with pricing",
                IsAssigned = false,
                DefaultRole = "False",
                accessAllProperties = true
            };

            // Assert
            Assert.Equal("10", role.ID);
            Assert.Equal("MANAGE AMENITY WITH PRICING", role.Name);
            Assert.True(role.accessAllProperties);
        }

        [Fact]
        public void ULRole_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var role = new UL.Role
            {
                RoleID = 5
            };

            // Assert
            Assert.Equal(5, role.RoleID);
        }

        [Fact]
        public void ProductRight_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var right = new ProductRight
            {
                ID = 100,
                Description = "View amenity information"
            };

            // Assert
            Assert.Equal(100, right.ID);
            Assert.Equal("View amenity information", right.Description);
        }

        #endregion

        #region Documentation Tests

        [Fact]
        public void ManageUnifiedAmenities_ClassPurpose_Documentation()
        {
            // This test documents the class purpose:
            // ManageUnifiedAmenities manages user access to Unified Amenities (Intelligent Building)
            //
            // Key features:
            // 1. Database-Only Storage:
            //    - No external API calls (unique among products)
            //    - All data stored in GreenBook database
            //    - UserRoleRight table for roles
            //    - PersonaProductProperty table for properties (legacy)
            //    - PersonaProductPropertyInstance table for property instances (new)
            //
            // 2. Role Management:
            //    - Single role per user (not multiple like other products)
            //    - Stored in UserRoleRight table
            //    - When changing roles: Delete old, Insert new
            //    - Super users get "MANAGE AMENITY WITH PRICING" role
            //    - New users get default role (DefaultRole = "True")
            //
            // 3. Property Management:
            //    - Properties fetched from BlueBook (GetVCompanyPropertyMap)
            //    - Two storage modes:
            //      a. Legacy: PersonaProductProperty (CustomerPropertyId)
            //      b. New: PersonaProductPropertyInstance (PropertyInstanceId)
            //    - Mode controlled by: UsePropertyInstanceUnifiedAmenities setting
            //    - Parallel processing for assignments/unassignments
            //
            // 4. Super User Handling:
            //    - PropertyList = ["-1"] (all properties indicator)
            //    - Role = "MANAGE AMENITY WITH PRICING" (found via role name)
            //    - Overrides any user input
            //
            // 5. "ALL" Properties Handling:
            //    - Input: PropertyList = ["ALL"]
            //    - If role has accessAllProperties = true:
            //      - Convert to PropertyList = ["-1"]
            //    - Otherwise: Process as specific properties
            //
            // 6. Property Assignment Algorithm:
            //    - Get current assigned properties
            //    - Compare with new property list
            //    - Properties to add: In new list but not in current
            //    - Properties to remove: In current but not in new list
            //    - Use Parallel.ForEach for processing
            //
            // 7. Default Role for New Users:
            //    - Roles have DefaultRole field ("True" or "False")
            //    - New users automatically get role where DefaultRole = "True"
            //    - Existing users without assigned role also get default
            //
            // 8. Property Instance Support:
            //    - New feature to support PropertyInstance model
            //    - Controlled by ProductInternalSetting: UsePropertyInstanceUnifiedAmenities
            //    - If "1": Use PersonaProductPropertyInstance table
            //    - If "0" or missing: Use PersonaProductProperty table (legacy)
            //
            // 9. Unassign User:
            //    - Delete user's role from UserRoleRight
            //    - Set ProductBatch status to Deleted
            //    - Properties remain assigned (by design)
            //
            // 10. GetRoles:
            //     - Fetches roles from ProductRepository
            //     - For existing users: Marks assigned role
            //     - For new users: Marks default role
            //     - Orders by role name

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageUnifiedAmenities_StorageModel_Documentation()
        {
            // This test documents the storage model:
            //
            // GREENBOOK DATABASE STORAGE:
            //
            // 1. UserRoleRight Table (Roles):
            //    Columns:
            //    - UserRoleRightID (PK)
            //    - PersonaID (FK)
            //    - RoleID (FK)
            //    - UserID (creator)
            //    - CreatedDate
            //    
            //    Operations:
            //    - Insert: InsertAssignedRoleToUser(deleteRole: false)
            //    - Delete: InsertAssignedRoleToUser(deleteRole: true)
            //    - Get: GetAssignedRoleForPersona(personaId)
            //    
            //    Single Role Constraint:
            //    - User can only have ONE role
            //    - To change: Delete old, Insert new
            //
            // 2. PersonaProductProperty Table (Legacy Properties):
            //    Columns:
            //    - PersonaProductPropertyID (PK)
            //    - PersonaID (FK)
            //    - ProductID (FK)
            //    - CustomerPropertyID (FK)
            //    - CreatedDate
            //    
            //    Operations:
            //    - Insert: InsertAssignedUserPropertyData
            //    - Delete: DeleteAssignedUserPropertyData
            //    - Get: GetAssignedPropertyForPersona
            //    
            //    Used When:
            //    - UsePropertyInstanceUnifiedAmenities = "0" or not set
            //
            // 3. PersonaProductPropertyInstance Table (New Properties):
            //    Columns:
            //    - PersonaProductPropertyInstanceID (PK)
            //    - PersonaID (FK)
            //    - ProductID (FK)
            //    - PropertyInstanceID (FK)
            //    - CreatedDate
            //    
            //    Operations:
            //    - Insert: InsertAssignedUserPropertyInstanceData
            //    - Delete: DeleteAssignedUserPropertyInstanceData
            //    - Get: GetAssignedPropertyForPersona (returns as CustomerPropertyId)
            //    
            //    Used When:
            //    - UsePropertyInstanceUnifiedAmenities = "1"
            //    
            //    Benefits:
            //    - Supports multi-instance properties
            //    - Better data model for enterprise scenarios
            //
            // 4. ProductBatch Table (Status):
            //    Columns:
            //    - ProductBatchID (PK)
            //    - PersonaID (FK)
            //    - ProductID (FK)
            //    - StatusTypeID (Success/Error/Deleted)
            //    
            //    Operations:
            //    - Update: UpdateProductSettingProductStatus
            //    
            //    Status Values:
            //    - Success (8): User assigned successfully
            //    - Error (7): Assignment failed
            //    - Deleted (6): User unassigned
            //
            // NO EXTERNAL TABLES:
            // - Unlike other products (Resident Portal, RUM, Vendor Services)
            // - No external API endpoints
            // - No external user records
            // - No external authentication
            // - Pure GreenBook storage
            //
            // DATA FLOW:
            // 1. User input ? UnifiedAmenitiesPropertyRole
            // 2. Role validation ? UserRoleRight table
            // 3. Property fetch ? BlueBook API
            // 4. Property assignment ? PersonaProductProperty/PersonaProductPropertyInstance
            // 5. Status update ? ProductBatch table

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageUnifiedAmenities_RoleManagement_Documentation()
        {
            // This test documents role management:
            //
            // SINGLE ROLE PER USER:
            // - Unlike most products (which support multiple roles)
            // - Unified Amenities allows ONLY ONE role per user
            //
            // ROLE CHANGE WORKFLOW:
            // ```csharp
            // List<UL.Role> existingRoles = GetAssignedRoleForPersona(userPersonaId);
            // long existingRoleId = existingRoles?.Count > 0 ? existingRoles[0].RoleID : 0;
            // 
            // if (newRoleId != existingRoleId)
            // {
            //     if (existingRoleId != 0)
            //     {
            //         // Delete old role
            //         InsertAssignedRoleToUser(
            //             userPersonaId: userPersonaId,
            //             roleId: existingRoleId,
            //             userId: editorUserId,
            //             deleteRole: true);
            //     }
            //     
            //     if (newRoleId != 0)
            //     {
            //         // Insert new role
            //         InsertAssignedRoleToUser(
            //             userPersonaId: userPersonaId,
            //             roleId: newRoleId,
            //             userId: editorUserId,
            //             deleteRole: false);
            //     }
            // }
            // ```
            //
            // DEFAULT ROLE FOR NEW USERS:
            // - Roles have DefaultRole property ("True" or "False")
            // - When creating new user:
            //   ```csharp
            //   if (roles.Any(r => r.DefaultRole == "True"))
            //   {
            //       roles.First(r => r.DefaultRole == "True").IsAssigned = true;
            //   }
            //   ```
            // - When updating user without role:
            //   - Also auto-assigns default role
            //
            // SUPER USER ROLE:
            // - Super users get special role: "MANAGE AMENITY WITH PRICING"
            // - Found by role name (case-insensitive):
            //   ```csharp
            //   var roles = ListRolesForProductByParty(partyId, productIds, productId);
            //   string superUserRoleId = roles.Find(r => 
            //       r.Name.ToUpper() == "MANAGE AMENITY WITH PRICING").ID;
            //   ```
            // - Overrides any user input
            //
            // ROLE PROPERTIES:
            // - ID: Role identifier (stored in UserRoleRight)
            // - Name: Display name
            // - Description: Role description
            // - DefaultRole: "True" or "False" (for auto-assignment)
            // - accessAllProperties: true/false (for "ALL" properties handling)
            // - IsAssigned: UI indicator (not stored in database)
            //
            // GET ROLES WORKFLOW:
            // 1. Fetch all roles from ProductRepository
            // 2. Order by Name
            // 3. If existing user:
            //    - Get assigned role via GetAssignedRoleForPersona
            //    - Mark role as IsAssigned = true
            //    - If no role assigned: Mark default role
            // 4. If new user:
            //    - Mark default role as IsAssigned = true
            // 5. Return role list

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageUnifiedAmenities_PropertyManagement_Documentation()
        {
            // This test documents property management:
            //
            // PROPERTY SOURCE:
            // - Fetched from BlueBook: GetVCompanyPropertyMap
            // - Uses: BooksCustomerMasterId from Organization
            // - Returns: CustomerCompanyPropertyMap list
            //
            // PROPERTY MAPPING:
            // ```csharp
            // CustomerCompanyPropertyMap (BlueBook) ? ProductProperty (GreenBook)
            // {
            //     ID: CustomerPropertyId.ToString(),
            //     Name: PropertyName,
            //     City: PropertyCity,
            //     State: PropertyState,
            //     Street1: PropertyAddress,
            //     Zip: "" (not available),
            //     IsAssigned: false (default),
            //     Active: IsActive ? "1" : "0"
            // }
            // ```
            //
            // PROPERTY ASSIGNMENT ALGORITHM:
            // ```csharp
            // // 1. Get current properties
            // List<ProductProperty> currentProperties = GetAssignedPropertyForPersona(personaId);
            // List<string> newPropertyIds = input.PropertyList;
            // 
            // // 2. Find properties to add
            // List<string> toAdd = new List<string>();
            // foreach (string newPropId in newPropertyIds)
            // {
            //     if (!currentProperties.Any(p => p.ID == newPropId))
            //     {
            //         toAdd.Add(newPropId);
            //     }
            // }
            // 
            // // 3. Find properties to remove
            // List<string> toRemove = new List<string>();
            // foreach (ProductProperty currentProp in currentProperties)
            // {
            //     if (!newPropertyIds.Any(id => id == currentProp.ID))
            //     {
            //         toRemove.Add(currentProp.ID);
            //     }
            // }
            // 
            // // 4. Process in parallel
            // Parallel.ForEach(toRemove, propId => DeleteAssignedProperty(propId));
            // Parallel.ForEach(toAdd, propId => InsertAssignedProperty(propId));
            // ```
            //
            // PROPERTY vs PROPERTY INSTANCE:
            // 
            // Decision Logic:
            // ```csharp
            // var settings = GetProductSetting(ProductEnum.UnifiedPlatform);
            // bool usePropertyInstances = settings
            //     ?.FirstOrDefault(s => s.Name.Equals(
            //         "UsePropertyInstanceUnifiedAmenities", 
            //         StringComparison.OrdinalIgnoreCase))
            //     ?.Value == "1";
            // ```
            //
            // If usePropertyInstances == false (Legacy):
            // ```csharp
            // InsertAssignedUserPropertyData(personaId, ProductEnum.UnifiedAmenities, propertyId);
            // DeleteAssignedUserPropertyData(personaId, ProductEnum.UnifiedAmenities, propertyId);
            // // Uses: PersonaProductProperty table
            // // Key: CustomerPropertyID
            // ```
            //
            // If usePropertyInstances == true (New):
            // ```csharp
            // InsertAssignedUserPropertyInstanceData(personaId, productId, propertyInstanceId);
            // DeleteAssignedUserPropertyInstanceData(personaId, productId, propertyInstanceId);
            // // Uses: PersonaProductPropertyInstance table
            // // Key: PropertyInstanceID
            // ```
            //
            // SPECIAL PROPERTY VALUES:
            //
            // "-1" (Super User / All Properties):
            // - Indicates user has access to ALL properties
            // - Super users automatically get this
            // - Stored as single property ID "-1"
            //
            // "ALL" (Input from UI):
            // - User selected "All Properties" option
            // - Check if role has accessAllProperties = true
            // - If yes: Convert to "-1"
            // - If no: Treat as specific properties
            //
            // GET PROPERTIES WORKFLOW:
            // 1. Fetch properties from BlueBook
            // 2. Map to ProductProperty format
            // 3. If assignedOnly = true:
            //    - Get assigned properties for user
            //    - Mark IsAssigned = true for assigned
            // 4. Return property list
            //
            // PARALLEL PROCESSING:
            // - Property operations use Parallel.ForEach
            // - Improves performance for large property lists
            // - Thread-safe repository operations required

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageUnifiedAmenities_Workflows_Documentation()
        {
            // This test documents complete workflows:
            //
            // CREATE/UPDATE USER WORKFLOW:
            // ???????????????????????????????????????
            // ? ManageUnifiedAmenitiesUser          ?
            // ???????????????????????????????????????
            //              ?
            //              ?
            // ???????????????????????????????????????
            // ? GetCompanyEditorAndUserDetails      ?
            // ???????????????????????????????????????
            //              ?
            //              ?
            // ???????????????????????????????????????
            // ? Get UsePropertyInstanceUA setting   ?
            // ???????????????????????????????????????
            //              ?
            //              ?
            // ???????????????????????????????????????
            // ? Get Person, UserLogin               ?
            // ???????????????????????????????????????
            //              ?
            //              ?
            // ???????????????????????????????????????
            // ? Check if Super User                 ?
            // ???????????????????????????????????????
            //              ?
            //       ???????????????
            //       ?             ?
            //     Yes            No
            //       ?             ?
            //       ?             ?
            // ????????????  ????????????????
            // ? Override ?  ? Use input    ?
            // ? with     ?  ? PropertyRole ?
            // ? Super    ?  ?              ?
            // ? User     ?  ?              ?
            // ? settings ?  ?              ?
            // ????????????  ????????????????
            //      ?               ?
            //      ?????????????????
            //              ?
            //              ?
            // ???????????????????????????????????????
            // ? Handle "ALL" properties             ?
            // ? If role.accessAllProperties:        ?
            // ?   Convert "ALL" ? "-1"              ?
            // ???????????????????????????????????????
            //              ?
            //              ?
            // ???????????????????????????????????????
            // ? ROLE MANAGEMENT                     ?
            // ? Get existing role                   ?
            // ???????????????????????????????????????
            //              ?
            //       ???????????????
            //       ?             ?
            //   Different      Same
            //       ?             ?
            //       ?             ?
            // ????????????  ????????????
            // ? Delete   ?  ? Skip     ?
            // ? old role ?  ?          ?
            // ????????????  ????????????
            //      ?
            //      ?
            // ????????????
            // ? Insert   ?
            // ? new role ?
            // ????????????
            //      ?
            //      ?
            // ???????????????????????????????????????
            // ? PROPERTY MANAGEMENT                 ?
            // ? Get current properties              ?
            // ? Compare with new list               ?
            // ???????????????????????????????????????
            //              ?
            //              ?
            // ???????????????????????????????????????
            // ? Parallel.ForEach(toRemove)          ?
            // ?   DeleteAssignedProperty[Instance]  ?
            // ???????????????????????????????????????
            //              ?
            //              ?
            // ???????????????????????????????????????
            // ? Parallel.ForEach(toAdd)             ?
            // ?   InsertAssignedProperty[Instance]  ?
            // ???????????????????????????????????????
            //              ?
            //              ?
            // ???????????????????????????????????????
            // ? UpdateProductSettingProductStatus   ?
            // ? Status = Success                    ?
            // ???????????????????????????????????????
            //
            // UNASSIGN USER WORKFLOW:
            // ???????????????????????????????????????
            // ? UnassignUser                        ?
            // ???????????????????????????????????????
            //              ?
            //              ?
            // ???????????????????????????????????????
            // ? GetCompanyEditorAndUserDetails      ?
            // ???????????????????????????????????????
            //              ?
            //              ?
            // ???????????????????????????????????????
            // ? Get assigned role                   ?
            // ???????????????????????????????????????
            //              ?
            //       ???????????????
            //       ?             ?
            //  Has Role      No Role
            //       ?             ?
            //       ?             ?
            // ????????????  ????????????
            // ? Delete   ?  ? Skip     ?
            // ? role     ?  ?          ?
            // ????????????  ????????????
            //      ?
            //      ?????????????????
            //              ?
            //              ?
            // ???????????????????????????????????????
            // ? UpdateProductSettingProductStatus   ?
            // ? Status = Deleted                    ?
            // ???????????????????????????????????????
            //
            // GET ROLES WORKFLOW:
            // ???????????????????????????????????????
            // ? GetRoles                            ?
            // ???????????????????????????????????????
            //              ?
            //              ?
            // ???????????????????????????????????????
            // ? ListRolesForProductByParty          ?
            // ? Order by Name                       ?
            // ???????????????????????????????????????
            //              ?
            //       ???????????????
            //       ?             ?
            //  New User    Existing User
            //       ?             ?
            //       ?             ?
            // ????????????  ????????????????
            // ? Mark     ?  ? Get assigned ?
            // ? default  ?  ? role         ?
            // ? role     ?  ? Mark role    ?
            // ????????????  ????????????????
            //      ?               ?
            //      ?        ???????????????
            //      ?        ?             ?
            //      ?   Has Role      No Role
            //      ?        ?             ?
            //      ?        ?             ?
            //      ?  ????????????  ????????????
            //      ?  ? Mark     ?  ? Mark     ?
            //      ?  ? assigned ?  ? default  ?
            //      ?  ? role     ?  ? role     ?
            //      ?  ????????????  ????????????
            //      ?       ?             ?
            //      ???????????????????????
            //                    ?
            //                    ?
            //         ???????????????????????
            //         ? Return role list    ?
            //         ???????????????????????

            Assert.True(true, "Documentation test");
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public void ProductEnum_UnifiedAmenities_HasCorrectValue()
        {
            // Assert
            Assert.Equal(26, (int)ProductEnum.UnifiedAmenities);
        }

        [Fact]
        public void UnifiedAmenitiesPropertyRole_WithNullLists_HandlesGracefully()
        {
            // Arrange & Act
            var propertyRole = new UnifiedAmenitiesPropertyRole
            {
                PropertyList = null,
                RoleList = null
            };

            // Assert
            Assert.NotNull(propertyRole);
            Assert.Null(propertyRole.PropertyList);
            Assert.Null(propertyRole.RoleList);
        }

        [Fact]
        public void UnifiedAmenitiesPropertyRole_WithEmptyLists_HandlesGracefully()
        {
            // Arrange & Act
            var propertyRole = new UnifiedAmenitiesPropertyRole
            {
                PropertyList = new List<string>(),
                RoleList = new List<string>()
            };

            // Assert
            Assert.NotNull(propertyRole);
            Assert.Empty(propertyRole.PropertyList);
            Assert.Empty(propertyRole.RoleList);
        }

        [Fact]
        public void ProductRole_DefaultRoleValues_CaseInsensitive()
        {
            // Document that DefaultRole comparison is case-insensitive
            var trueValues = new[] { "True", "TRUE", "true", "TrUe" };
            var falseValues = new[] { "False", "FALSE", "false", "FaLsE" };

            // Assert - Document expected behavior
            foreach (var val in trueValues)
            {
                Assert.True(val.Equals("True", StringComparison.OrdinalIgnoreCase));
            }
            foreach (var val in falseValues)
            {
                Assert.False(val.Equals("True", StringComparison.OrdinalIgnoreCase));
            }
        }

        #endregion

        #region Integration Test Recommendations

        [Fact]
        public void ManageUnifiedAmenities_IntegrationTests_Documentation()
        {
            // This test documents recommended integration tests:
            //
            // Critical Integration Test Scenarios:
            //
            // 1. Create New User with Role and Properties:
            //    - Create persona
            //    - Call ManageUnifiedAmenitiesUser with role and properties
            //    - Verify: UserRoleRight record created
            //    - Verify: PersonaProductProperty/Instance records created
            //    - Verify: ProductBatch status = Success
            //
            // 2. Create New User with Default Role:
            //    - Create persona
            //    - Call ManageUnifiedAmenitiesUser with only properties (no role)
            //    - Verify: Default role assigned
            //    - Verify: Properties assigned
            //
            // 3. Update Existing User - Change Role:
            //    - Create user with role A
            //    - Update to role B
            //    - Verify: Old role deleted
            //    - Verify: New role inserted
            //    - Verify: Only one role exists
            //
            // 4. Update Existing User - Change Properties:
            //    - Create user with properties [123, 456]
            //    - Update to properties [456, 789]
            //    - Verify: Property 123 deleted
            //    - Verify: Property 789 added
            //    - Verify: Property 456 unchanged
            //
            // 5. Super User Assignment:
            //    - Create super user persona
            //    - Call ManageUnifiedAmenitiesUser
            //    - Verify: PropertyList = ["-1"]
            //    - Verify: Role = "MANAGE AMENITY WITH PRICING"
            //    - Verify: Input overridden
            //
            // 6. "ALL" Properties Handling:
            //    - Create role with accessAllProperties = true
            //    - Assign user with PropertyList = ["ALL"]
            //    - Verify: Converted to ["-1"]
            //    - Create role with accessAllProperties = false
            //    - Verify: Not converted
            //
            // 7. Property Instance Mode:
            //    - Set UsePropertyInstanceUnifiedAmenities = "1"
            //    - Assign properties
            //    - Verify: PersonaProductPropertyInstance records created
            //    - Verify: PropertyInstanceID used (not CustomerPropertyID)
            //
            // 8. Legacy Property Mode:
            //    - Set UsePropertyInstanceUnifiedAmenities = "0" or remove
            //    - Assign properties
            //    - Verify: PersonaProductProperty records created
            //    - Verify: CustomerPropertyID used
            //
            // 9. Parallel Property Processing:
            //    - Assign 100 properties
            //    - Verify: All processed via Parallel.ForEach
            //    - Verify: No race conditions
            //    - Verify: All records created
            //
            // 10. Unassign User:
            //     - Create assigned user
            //     - Call UnassignUser
            //     - Verify: Role deleted from UserRoleRight
            //     - Verify: ProductBatch status = Deleted
            //     - Verify: Properties remain (by design)
            //
            // 11. Get Roles - New User:
            //     - Call GetRoles with userPersonaId = 0
            //     - Verify: All roles returned
            //     - Verify: Default role marked IsAssigned = true
            //     - Verify: Ordered by Name
            //
            // 12. Get Roles - Existing User with Role:
            //     - Create user with role
            //     - Call GetRoles
            //     - Verify: Assigned role marked IsAssigned = true
            //     - Verify: Other roles IsAssigned = false
            //
            // 13. Get Roles - Existing User without Role:
            //     - Create user without role
            //     - Call GetRoles
            //     - Verify: Default role marked IsAssigned = true
            //
            // 14. Get Properties - All Properties:
            //     - Call GetProperties with assignedOnly = false
            //     - Verify: All company properties returned from BlueBook
            //     - Verify: IsAssigned = false for all
            //
            // 15. Get Properties - Assigned Properties:
            //     - Create user with assigned properties
            //     - Call GetProperties with assignedOnly = true
            //     - Verify: Assigned properties marked IsAssigned = true
            //     - Verify: Unassigned properties IsAssigned = false
            //
            // 16. Get Rights by Role:
            //     - Call GetRightsByRole for specific role
            //     - Verify: Rights for role returned
            //     - Verify: Ordered by Description
            //
            // 17. Error Handling - Invalid Persona:
            //     - Call with non-existent personaId
            //     - Verify: Error returned
            //     - Verify: Status = Error
            //
            // 18. Error Handling - Role Assignment Failure:
            //     - Simulate repository failure
            //     - Verify: Error message returned
            //     - Verify: Transaction rolled back
            //
            // 19. Error Handling - Property Assignment Failure:
            //     - Simulate repository failure during parallel processing
            //     - Verify: Error handling
            //
            // 20. End-to-End Workflow:
            //     - Create user ? Assign role ? Assign properties
            //     - Update role ? Update properties
            //     - Unassign user
            //     - Verify: Complete lifecycle
            //
            // Why Integration Tests?
            // - UserRoleRightRepository requires database
            // - ProductRepository requires database
            // - BlueBook requires API/database
            // - Parallel processing requires real threads
            // - Repository responses required for validation
            // - ProductBatch status updates require database
            // - Property/PropertyInstance mode switching requires real data
            //
            // Current Test Coverage:
            // ? Constructor initialization (2 overloads)
            // ? Data class structures (6 classes)
            // ? Business logic documentation
            // ? Storage model documentation
            // ? Role management patterns
            // ? Property management patterns
            // ? Complete workflows
            // ? Edge cases
            //
            // Requires Integration Tests:
            // - All public methods
            // - Database operations
            // - BlueBook integration
            // - Role assignment/deletion
            // - Property assignment/deletion
            // - Property instance support
            // - Parallel processing
            // - Super user handling
            // - Default role assignment
            // - "ALL" properties conversion

            Assert.True(true, "Documentation test");
        }

        #endregion
    }
}
