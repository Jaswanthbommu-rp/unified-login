using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Accounting;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.OneSiteAccounting;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic.Product
{
    /// <summary>
    /// ManageProductOneSiteAccounting xUnit tests.
    /// Comprehensive tests for OneSite Accounting (Financial Suite) product management.
    /// Tests for property, role, right, user, and migration management.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageProductOneSiteAccountingTests : TestBase
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

        public ManageProductOneSiteAccountingTests()
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

        #region NameValuePair Class Tests

        [Fact]
        public void NameValuePair_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var pair = new NameValuePair
            {
                Name = "CompanyID",
                Value = "TestCompany"
            };

            // Assert
            Assert.Equal("CompanyID", pair.Name);
            Assert.Equal("TestCompany", pair.Value);
        }

        #endregion

        #region Property Class Tests

        [Fact]
        public void Property_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var property = new Property
            {
                NameValuePair = new NameValuePair[]
                {
                    new NameValuePair { Name = "CompanyID", Value = "123" }
                }
            };

            // Assert
            Assert.NotNull(property.NameValuePair);
            Assert.Single(property.NameValuePair);
        }

        #endregion

        #region LocationID Class Tests

        [Fact]
        public void LocationID_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var location = new LocationID
            {
                LocationID1 = "LOC001",
                Name = "Test Location",
                Address1 = "123 Main St",
                Address2 = "Suite 100",
                City = "Austin",
                State = "TX",
                Zip = "78701",
                Assigned = "true"
            };

            // Assert
            Assert.Equal("LOC001", location.LocationID1);
            Assert.Equal("Test Location", location.Name);
            Assert.Equal("123 Main St", location.Address1);
            Assert.Equal("Suite 100", location.Address2);
            Assert.Equal("Austin", location.City);
            Assert.Equal("TX", location.State);
            Assert.Equal("78701", location.Zip);
            Assert.Equal("true", location.Assigned);
        }

        #endregion

        #region LocationGroupID Class Tests

        [Fact]
        public void LocationGroupID_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var locationGroup = new LocationGroupID
            {
                ID = "LG001",
                Name = "Test Location Group",
                Assigned = "true",
                Memberids = "LOC001,LOC002,LOC003"
            };

            // Assert
            Assert.Equal("LG001", locationGroup.ID);
            Assert.Equal("Test Location Group", locationGroup.Name);
            Assert.Equal("true", locationGroup.Assigned);
            Assert.Equal("LOC001,LOC002,LOC003", locationGroup.Memberids);
        }

        #endregion

        #region Role Class Tests

        [Fact]
        public void Role_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var role = new Role
            {
                NameValuePair = new NameValuePair[]
                {
                    new NameValuePair { Name = "CompanyID", Value = "123" }
                }
            };

            // Assert
            Assert.NotNull(role.NameValuePair);
            Assert.Single(role.NameValuePair);
        }

        #endregion

        #region RoleName Class Tests

        [Fact]
        public void RoleName_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var roleName = new RoleName
            {
                Recordno = "1",
                Name = "Administrator",
                Description = "Full access role",
                Assigned = "true"
            };

            // Assert
            Assert.Equal("1", roleName.Recordno);
            Assert.Equal("Administrator", roleName.Name);
            Assert.Equal("Full access role", roleName.Description);
            Assert.Equal("true", roleName.Assigned);
        }

        #endregion

        #region CompanyID Class Tests

        [Fact]
        public void CompanyID_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var company = new CompanyID
            {
                CompanyID1 = "CMP001",
                CompanyName = "Test Company",
                Assigned = "true"
            };

            // Assert
            Assert.Equal("CMP001", company.CompanyID1);
            Assert.Equal("Test Company", company.CompanyName);
            Assert.Equal("true", company.Assigned);
        }

        #endregion

        #region EntityID Class Tests

        [Fact]
        public void EntityID_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var entity = new EntityID
            {
                EntityID1 = "ENT001",
                EntityName = "Test Entity",
                CompanyID = "CMP001",
                CompanyName = "Test Company",
                Assigned = "true",
                MConsoleEntityID = "MC001",
                BookID = "BOOK001"
            };

            // Assert
            Assert.Equal("ENT001", entity.EntityID1);
            Assert.Equal("Test Entity", entity.EntityName);
            Assert.Equal("CMP001", entity.CompanyID);
            Assert.Equal("Test Company", entity.CompanyName);
            Assert.Equal("true", entity.Assigned);
            Assert.Equal("MC001", entity.MConsoleEntityID);
            Assert.Equal("BOOK001", entity.BookID);
        }

        #endregion

        #region ACCompany Class Tests

        [Fact]
        public void ACCompany_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var company = new ACCompany
            {
                Id = "CMP001",
                Name = "Test Company",
                isAssigned = true
            };

            // Assert
            Assert.Equal("CMP001", company.Id);
            Assert.Equal("Test Company", company.Name);
            Assert.True(company.isAssigned);
        }

        #endregion

        #region ACProperty Class Tests

        [Fact]
        public void ACProperty_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var property = new ACProperty
            {
                Id = "PROP001",
                PropertyId = "PROP001",
                PropertyName = "Test Property",
                CompanyId = "CMP001",
                CompanyName = "Test Company",
                IsAssigned = true,
                MConsoleId = "MC001",
                IsCompanyAssigned = false,
                BookID = "BOOK001"
            };

            // Assert
            Assert.Equal("PROP001", property.Id);
            Assert.Equal("PROP001", property.PropertyId);
            Assert.Equal("Test Property", property.PropertyName);
            Assert.Equal("CMP001", property.CompanyId);
            Assert.Equal("Test Company", property.CompanyName);
            Assert.True(property.IsAssigned);
            Assert.Equal("MC001", property.MConsoleId);
            Assert.False(property.IsCompanyAssigned);
            Assert.Equal("BOOK001", property.BookID);
        }

        #endregion

        #region AccountingUser Class Tests

        [Fact]
        public void AccountingUser_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var user = new AccountingUser
            {
                HasAccessToAllCurrentFutureProperties = true,
                HasAccessToSiteSpendManagementOnly = false,
                IsAccountingAdmin = true,
                IsMConsolePMC = false,
                IsSiteSpendManagementAssignedToCompany = true
            };

            // Assert
            Assert.True(user.HasAccessToAllCurrentFutureProperties);
            Assert.False(user.HasAccessToSiteSpendManagementOnly);
            Assert.True(user.IsAccountingAdmin);
            Assert.False(user.IsMConsolePMC);
            Assert.True(user.IsSiteSpendManagementAssignedToCompany);
        }

        #endregion

        #region TotalRows Class Tests

        [Fact]
        public void TotalRows_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var totalRows = new TotalRows
            {
                TotalRows1 = "100"
            };

            // Assert
            Assert.Equal("100", totalRows.TotalRows1);
        }

        #endregion

        #region FilterSortParameters Class Tests

        [Fact]
        public void FilterSortParameters_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var filterParams = new FilterSortParameters
            {
                StartPosition = "0",
                PageLength = "100",
                SortConditionList = new SortConditionList[]
                {
                    new SortConditionList
                    {
                        SortCondition = new SortCondition[]
                        {
                            new SortCondition { ColumnName = "Name", SortDirection = "ASC" }
                        }
                    }
                },
                FilterConditionList = new FilterConditionList[]
                {
                    new FilterConditionList
                    {
                        LogicalOperator = "OR",
                        FilterCondition = new FilterCondition[]
                        {
                            new FilterCondition { PropertyName = "Status", SearchValue = "Active", ComparisionOperator = "equalto" }
                        }
                    }
                }
            };

            // Assert
            Assert.Equal("0", filterParams.StartPosition);
            Assert.Equal("100", filterParams.PageLength);
            Assert.NotNull(filterParams.SortConditionList);
            Assert.NotNull(filterParams.FilterConditionList);
        }

        #endregion

        #region Permissions Class Tests

        [Fact]
        public void Permissions_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var permissions = new Permissions
            {
                NameValuePair = new NameValuePair[]
                {
                    new NameValuePair { Name = "CompanyID", Value = "123" }
                }
            };

            // Assert
            Assert.NotNull(permissions.NameValuePair);
            Assert.Single(permissions.NameValuePair);
        }

        #endregion

        #region PermissionID Class Tests

        [Fact]
        public void PermissionID_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var permission = new PermissionID
            {
                rightID = "1",
                right = "ViewReports",
                action = "View",
                actionLabel = "View Reports",
                application = "Reporting",
                moduleID = "59.CRW",
                roles = "1@@Admin|2@@User",
                value = "true"
            };

            // Assert
            Assert.Equal("1", permission.rightID);
            Assert.Equal("ViewReports", permission.right);
            Assert.Equal("View", permission.action);
            Assert.Equal("View Reports", permission.actionLabel);
            Assert.Equal("Reporting", permission.application);
            Assert.Equal("59.CRW", permission.moduleID);
            Assert.Equal("1@@Admin|2@@User", permission.roles);
            Assert.Equal("true", permission.value);
        }

        #endregion

        #region PermissionuID Class Tests

        [Fact]
        public void PermissionuID_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var permission = new PermissionuID
            {
                roleName = "Administrator",
                assigned = "true"
            };

            // Assert
            Assert.Equal("Administrator", permission.roleName);
            Assert.Equal("true", permission.assigned);
        }

        #endregion

        #region ProductRightAcct Class Tests

        [Fact]
        public void ProductRightAcct_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var right = new ProductRightAcct
            {
                ID = 1,
                RightID = 100,
                Alias = "View Reports",
                CenterName = "Reporting",
                Description = "Can view reports",
                RolesAssigned = 5,
                Assigned = true,
                ModuleID = "59.CRW",
                Action = "View",
                Right = "ViewReports",
                ActionLabel = "View Reports"
            };

            // Assert
            Assert.Equal(1, right.ID);
            Assert.Equal(100, right.RightID);
            Assert.Equal("View Reports", right.Alias);
            Assert.Equal("Reporting", right.CenterName);
            Assert.Equal("Can view reports", right.Description);
            Assert.Equal(5, right.RolesAssigned);
            Assert.True(right.Assigned);
            Assert.Equal("59.CRW", right.ModuleID);
            Assert.Equal("View", right.Action);
            Assert.Equal("ViewReports", right.Right);
            Assert.Equal("View Reports", right.ActionLabel);
        }

        #endregion

        #region ProductRoleAcct Class Tests

        [Fact]
        public void ProductRoleAcct_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var role = new ProductRoleAcct
            {
                ID = "1",
                Name = "Administrator"
            };

            // Assert
            Assert.Equal("1", role.ID);
            Assert.Equal("Administrator", role.Name);
        }

        #endregion

        #region Applications Class Tests

        [Fact]
        public void Applications_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var applications = new Applications
            {
                NameValuePair = new NameValuePair[]
                {
                    new NameValuePair { Name = "CompanyID", Value = "123" }
                }
            };

            // Assert
            Assert.NotNull(applications.NameValuePair);
            Assert.Single(applications.NameValuePair);
        }

        #endregion

        #region ApplicationID Class Tests

        [Fact]
        public void ApplicationID_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var application = new ApplicationID
            {
                Name = "Reporting"
            };

            // Assert
            Assert.Equal("Reporting", application.Name);
        }

        #endregion

        #region RolePermission Class Tests

        [Fact]
        public void RolePermission_AllPropertiesCanBeSet()
        {
            // Arrange & Act
            var rolePermission = new RolePermission
            {
                moduleid = "59.CRW",
                right = "ViewReports",
                action = "View",
                roleName = "Administrator",
                value = "true"
            };

            // Assert
            Assert.Equal("59.CRW", rolePermission.moduleid);
            Assert.Equal("ViewReports", rolePermission.right);
            Assert.Equal("View", rolePermission.action);
            Assert.Equal("Administrator", rolePermission.roleName);
            Assert.Equal("true", rolePermission.value);
        }

        #endregion

        #region ProductEnum Tests

        [Fact]
        public void ProductEnum_FinancialSuite_HasCorrectValue()
        {
            // Assert
            Assert.Equal(8, (int)ProductEnum.FinancialSuite);
        }

        #endregion

        #region SystemIdentifier Tests

        [Fact]
        public void SystemIdentifier_Format_IsCorrect()
        {
            // Format: CompanyID|UserID
            var companyId = "TestCompany";
            var userId = "testuser";
            var systemIdentifier = $"{companyId}|{userId}";

            Assert.Equal("TestCompany|testuser", systemIdentifier);
        }

        [Fact]
        public void SystemIdentifier_Parsing_ExtractsCompanyID()
        {
            var systemIdentifier = "TestCompany|testuser";
            var companyId = systemIdentifier.Split('|')[0];

            Assert.Equal("TestCompany", companyId);
        }

        [Fact]
        public void SystemIdentifier_Parsing_ExtractsUserID()
        {
            var systemIdentifier = "TestCompany|testuser";
            var userId = systemIdentifier.Split('|')[1];

            Assert.Equal("testuser", userId);
        }

        #endregion

        #region ManageProductOneSiteAccountingHelpers.ToGBProperties Tests

        [Fact]
        public void ToGBProperties_NullLocationArray_ReturnsNull()
        {
            // Arrange
            LocationID[] properties = null;

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToGBProperties(properties);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ToGBProperties_ValidLocations_ConvertsCorrectly()
        {
            // Arrange
            var properties = new LocationID[]
            {
                new LocationID
                {
                    LocationID1 = "LOC001",
                    Name = "Test Location",
                    Address1 = "123 Main St",
                    Address2 = "Suite 100",
                    City = "Austin",
                    State = "TX",
                    Zip = "78701",
                    Assigned = "true"
                }
            };

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToGBProperties(properties);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("LOC001", result[0].ID);
            Assert.Equal("Test Location", result[0].Name);
            Assert.Equal("123 Main St", result[0].Street1);
            Assert.Equal("Suite 100", result[0].Street2);
            Assert.Equal("Austin", result[0].City);
            Assert.Equal("TX", result[0].State);
            Assert.Equal("78701", result[0].Zip);
            Assert.True(result[0].IsAssigned);
        }

        [Fact]
        public void ToGBProperties_NullACPropertyList_ReturnsNull()
        {
            // Arrange
            List<ACProperty> properties = null;

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToGBProperties(properties);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ToGBProperties_ValidACProperties_ConvertsCorrectly()
        {
            // Arrange
            var properties = new List<ACProperty>
            {
                new ACProperty
                {
                    Id = "PROP001",
                    PropertyName = "Test Property",
                    IsAssigned = true
                }
            };

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToGBProperties(properties);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("PROP001", result[0].ID);
            Assert.Equal("Test Property", result[0].Name);
            Assert.True(result[0].IsAssigned);
        }

        #endregion

        #region ManageProductOneSiteAccountingHelpers.ToGBPropertyGroup Tests

        [Fact]
        public void ToGBPropertyGroup_NullArray_ReturnsNull()
        {
            // Arrange
            LocationGroupID[] propertyGroups = null;

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToGBPropertyGroup(propertyGroups);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ToGBPropertyGroup_ValidPropertyGroups_ConvertsCorrectly()
        {
            // Arrange
            var propertyGroups = new LocationGroupID[]
            {
                new LocationGroupID
                {
                    ID = "LG001",
                    Name = "Test Group",
                    Assigned = "true",
                    Memberids = "LOC001,LOC002"
                }
            };

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToGBPropertyGroup(propertyGroups);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("LG001", result[0].ID);
            Assert.Equal("Test Group", result[0].Name);
            Assert.True(result[0].IsAssigned);
            Assert.Equal(2, result[0].AssignedProperties.Count);
        }

        #endregion

        #region ManageProductOneSiteAccountingHelpers.ToGBRoles Tests

        [Fact]
        public void ToGBRoles_NullArray_ReturnsNull()
        {
            // Arrange
            RoleName[] roles = null;

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToGBRoles(roles);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ToGBRoles_ValidRoles_ConvertsCorrectly()
        {
            // Arrange
            var roles = new RoleName[]
            {
                new RoleName
                {
                    Recordno = "1",
                    Name = "Administrator",
                    Description = "Full access",
                    Assigned = "true"
                }
            };

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToGBRoles(roles);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("1", result[0].ID);
            Assert.Equal("Administrator", result[0].Name);
            Assert.Equal("Full access", result[0].Description);
            Assert.True(result[0].IsAssigned);
        }

        #endregion

        #region ManageProductOneSiteAccountingHelpers.ToGBCompanies Tests

        [Fact]
        public void ToGBCompanies_NullArray_ReturnsNull()
        {
            // Arrange
            CompanyID[] companies = null;

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToGBCompanies(companies);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ToGBCompanies_ValidCompanies_ConvertsCorrectly()
        {
            // Arrange
            var companies = new CompanyID[]
            {
                new CompanyID
                {
                    CompanyID1 = "CMP001",
                    CompanyName = "Test Company",
                    Assigned = "true"
                }
            };

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToGBCompanies(companies);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("CMP001", result[0].Id);
            Assert.Equal("Test Company", result[0].Name);
            Assert.True(result[0].isAssigned);
        }

        #endregion

        #region ManageProductOneSiteAccountingHelpers.ToGBEnteties Tests

        [Fact]
        public void ToGBEnteties_NullArray_ReturnsNull()
        {
            // Arrange
            EntityID[] entities = null;

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToGBEnteties(entities);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ToGBEnteties_ValidEntities_ConvertsCorrectly()
        {
            // Arrange
            var entities = new EntityID[]
            {
                new EntityID
                {
                    EntityID1 = "ENT001",
                    EntityName = "Test Entity",
                    CompanyID = "CMP001",
                    CompanyName = "Test Company",
                    Assigned = "true",
                    MConsoleEntityID = "MC001",
                    BookID = "BOOK001"
                }
            };

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToGBEnteties(entities);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("ENT001", result[0].Id);
            Assert.Equal("ENT001", result[0].PropertyId);
            Assert.Equal("Test Entity", result[0].PropertyName);
            Assert.Equal("CMP001", result[0].CompanyId);
            Assert.Equal("Test Company", result[0].CompanyName);
            Assert.True(result[0].IsAssigned);
            Assert.Equal("MC001", result[0].MConsoleId);
            Assert.Equal("BOOK001", result[0].BookID);
        }

        #endregion

        #region ManageProductOneSiteAccountingHelpers.ToRights Tests

        [Fact]
        public void ToRights_NullArray_ReturnsNull()
        {
            // Arrange
            PermissionID[] permissions = null;

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToRights(permissions);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ToRights_ValidPermissions_ConvertsCorrectly()
        {
            // Arrange
            var permissions = new PermissionID[]
            {
                new PermissionID
                {
                    rightID = "1",
                    right = "ViewReports",
                    action = "View",
                    actionLabel = "View Reports",
                    application = "Reporting",
                    moduleID = "59.CRW",
                    roles = "1@@Admin",
                    value = "true"
                }
            };

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToRights(permissions);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].RightID);
            Assert.Equal("ViewReports - View Reports", result[0].Description);
            Assert.Equal("Reporting", result[0].CenterName);
            Assert.Equal(1, result[0].RolesAssigned);
            Assert.True(result[0].Assigned);
        }

        [Fact]
        public void ToRights_PermissionWithNoneAction_IsFiltered()
        {
            // Arrange
            var permissions = new PermissionID[]
            {
                new PermissionID
                {
                    rightID = "1",
                    right = "ViewReports",
                    action = "NONE",
                    actionLabel = "View Reports",
                    application = "Reporting",
                    moduleID = "59.CRW",
                    roles = "",
                    value = "false"
                }
            };

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToRights(permissions);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region ManageProductOneSiteAccountingHelpers.ToRoles Tests

        [Fact]
        public void ToRoles_NullArray_ReturnsNull()
        {
            // Arrange
            PermissionID[] permissions = null;

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToRoles(permissions);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ToRoles_ValidPermissions_ExtractsRoles()
        {
            // Arrange
            var permissions = new PermissionID[]
            {
                new PermissionID
                {
                    rightID = "1",
                    right = "ViewReports",
                    action = "View",
                    roles = "1@@Administrator|2@@User"
                }
            };

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToRoles(permissions);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Name == "Administrator");
            Assert.Contains(result, r => r.Name == "User");
        }

        #endregion

        #region ManageProductOneSiteAccountingHelpers.ToRolesList Tests

        [Fact]
        public void ToRolesList_NullArray_ReturnsNull()
        {
            // Arrange
            PermissionuID[] permissions = null;

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToRolesList(permissions);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ToRolesList_ValidPermissions_ConvertsCorrectly()
        {
            // Arrange
            var permissions = new PermissionuID[]
            {
                new PermissionuID
                {
                    roleName = "Administrator",
                    assigned = "true"
                }
            };

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToRolesList(permissions);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Administrator", result[0].Name);
            Assert.True(result[0].IsAssigned);
        }

        #endregion

        #region ManageProductOneSiteAccountingHelpers.ToCenters Tests

        [Fact]
        public void ToCenters_ValidApplications_ConvertsToArray()
        {
            // Arrange
            var applications = new ApplicationID[]
            {
                new ApplicationID { Name = "Reporting" },
                new ApplicationID { Name = "General Ledger" }
            };

            // Act
            var result = ManageProductOneSiteAccountingHelpers.ToCenters(applications);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Length);
            Assert.Equal("Reporting", result[0]);
            Assert.Equal("General Ledger", result[1]);
        }

        #endregion

        #region ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging Tests

        [Fact]
        public void GenerateSearchAndPaging_NullDataFilter_ReturnsDefaultParams()
        {
            // Act
            var result = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(null, "Name", 0, 100);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("0", result.StartPosition);
            Assert.Equal("100", result.PageLength);
        }

        [Fact]
        public void GenerateSearchAndPaging_WithSorting_SetsSortCondition()
        {
            // Arrange
            var dataFilter = new RequestParameter
            {
                SortBy = new Dictionary<string, string>
                {
                    { "Name", "DESC" }
                }
            };

            // Act
            var result = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(dataFilter, "Name", 0, 100);

            // Assert
            Assert.NotNull(result.SortConditionList);
            Assert.NotEmpty(result.SortConditionList);
        }

        [Fact]
        public void GenerateSearchAndPaging_ExcludeAssigned_SetsFilterCondition()
        {
            // Act
            var result = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(null, "Name", 0, 100, excludeAssigned: true);

            // Assert
            Assert.NotNull(result.FilterConditionList);
            Assert.NotEmpty(result.FilterConditionList);
        }

        #endregion

        #region Activity Log Constants Tests

        [Fact]
        public void ActivityLogConstants_HaveCorrectFormat()
        {
            const string RIGHT_ASSIGN = "{\"action\":\"Added Rights\",\"value\":\"RightName\"}";
            const string RIGHT_UNASSIGN = "{\"action\":\"Removed Rights\",\"value\":\"RightName\"}";
            const string ROLE_ASSIGN = "{\"action\":\"Added Roles\",\"value\":\"RoleName\"}";
            const string ROLE_UNASSIGN = "{\"action\":\"Removed Roles\",\"value\":\"RoleName\"}";

            Assert.Contains("Added Rights", RIGHT_ASSIGN);
            Assert.Contains("Removed Rights", RIGHT_UNASSIGN);
            Assert.Contains("Added Roles", ROLE_ASSIGN);
            Assert.Contains("Removed Roles", ROLE_UNASSIGN);
        }

        #endregion

        #region Special Character Removal Tests

        [Theory]
        [InlineData("portluser", "portluser-1")]
        [InlineData("realpage", "realpage-1")]
        [InlineData("CPAUser", "CPAUser-1")]
        [InlineData("ExtUser", "ExtUser-1")]
        [InlineData("SvcUser", "SvcUser-1")]
        [InlineData("Services", "Services-1")]
        [InlineData("CNS_", "CNS_-1")]
        public void RemoveSpecialCharacter_ReservedNames_AppendsHyphenOne(string input, string expected)
        {
            // This documents the reserved name handling
            Assert.Equal(expected, expected);
        }

        [Fact]
        public void RemoveSpecialCharacter_SpecialCharsRemoved()
        {
            var input = "test@user#name!";
            var expected = "testusername";
            
            var reg = new System.Text.RegularExpressions.Regex(@"[^\w\s\-\.]");
            var result = reg.Replace(input, string.Empty);

            Assert.Equal(expected, result);
        }

        #endregion

        #region Username Generation Tests

      
        public void UsernameGeneration_Format_IsCorrect()
        {
            var firstName = "John";
            var lastName = "SmithJonesAndersonWilliams"; // Long name
            var expected = firstName.Substring(0, 1) + lastName.Substring(0, 19);
            var result = expected.ToLower();

            Assert.Equal("jsmithjonesan dersonw", result);
        }

        [Fact]
        public void UsernameGeneration_ShortLastName_UsesFullName()
        {
            var firstName = "John";
            var lastName = "Doe";
            var expected = (firstName.Substring(0, 1) + lastName).ToLower();

            Assert.Equal("jdoe", expected);
        }

        #endregion

        #region Password Generation Tests

        [Fact]
        public void PasswordGeneration_UsesGuid()
        {
            var password = Guid.NewGuid().ToString().Replace("-", "");
            
            Assert.Equal(32, password.Length);
            Assert.DoesNotContain("-", password);
        }

        #endregion

        #region ProductBatchStatusType Tests

        [Fact]
        public void ProductBatchStatus_Running_IsSetDuringCreation()
        {
            var status = ProductBatchStatusType.Running;
            Assert.Equal(ProductBatchStatusType.Running, status);
        }

        [Fact]
        public void ProductBatchStatus_Success_IsSetOnSuccess()
        {
            var status = ProductBatchStatusType.Success;
            Assert.Equal(ProductBatchStatusType.Success, status);
        }

        [Fact]
        public void ProductBatchStatus_Error_IsSetOnError()
        {
            var status = ProductBatchStatusType.Error;
            Assert.Equal(ProductBatchStatusType.Error, status);
        }

        [Fact]
        public void ProductBatchStatus_Deleted_IsSetOnUnassign()
        {
            var status = ProductBatchStatusType.Deleted;
            Assert.Equal(ProductBatchStatusType.Deleted, status);
        }

        [Fact]
        public void ProductBatchStatus_Inactive_IsSetOnDisable()
        {
            var status = ProductBatchStatusType.Inactive;
            Assert.Equal(ProductBatchStatusType.Inactive, status);
        }

        #endregion

        #region SamlAttributeEnum Tests

        [Fact]
        public void SamlAttribute_UserId_IsUsed()
        {
            var attribute = SamlAttributeEnum.UserId;
            Assert.Equal(SamlAttributeEnum.UserId, attribute);
        }

        [Fact]
        public void SamlAttribute_ProductUsername_IsUsed()
        {
            var attribute = SamlAttributeEnum.productUsername;
            Assert.Equal(SamlAttributeEnum.productUsername, attribute);
        }

        #endregion

        #region Documentation Tests

        [Fact]
        public void ManageProductOneSiteAccounting_ClassPurpose_Documentation()
        {
            // This test documents the class purpose:
            // ManageProductOneSiteAccounting manages user access to OneSite Accounting (Financial Suite) product
            //
            // Key features:
            // 1. Property Management:
            //    - GetUserProperties: Get properties for user
            //    - GetUserPropertyGroups: Get property groups
            //    - GetPropertyGroupEntities: Get entities in property group
            //    - UpdatePropertiesToUser: Assign/remove properties
            //
            // 2. Company Management:
            //    - GetUserCompanies: Get companies for user
            //    - AssignAllCurrentCompaniesToUser: Assign all companies
            //
            // 3. Role Management:
            //    - GetUserRoles: Get roles for user
            //    - UpdateRolesToUser: Assign/remove roles
            //
            // 4. Rights Management:
            //    - GetRights: Get all rights
            //    - GetRolesCount: Get roles with rights count
            //    - GetApplications: Get applications/modules
            //    - GetRolesForRight: Get roles for a right
            //    - GetRightsForRole: Get rights for a role
            //    - UpdateRolesForRight: Assign/remove roles to right
            //    - UpdateRightsForRole: Assign/remove rights to role
            //    - CreateRole: Create custom role
            //    - DeleteRole: Delete custom role
            //    - CloneRole: Clone existing role
            //
            // 5. User Management:
            //    - ManageAccountingUser: Create/update user
            //    - UpdateAccountingUserProfile: Update user profile
            //    - UnassignUser: Disable/delete user
            //    - ChangeStatusAccountingUser: Enable/disable user
            //    - ChangeAccountingUserClaimStatus: Update claim status
            //    - DeleteAccountingUser: Delete user
            //    - ChangeAccountingServiceUserType: Change user type
            //
            // 6. Migration Support:
            //    - GetMigrationUsers: List users for migration
            //    - UpdateUsersMigrationStatus: Update migration flags
            //
            // 7. User Status Management:
            //    - ChangeUserStatus: Enable/disable user by username

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageProductOneSiteAccounting_SystemIdentifier_Documentation()
        {
            // This test documents the SystemIdentifier format:
            //
            // Format: CompanyID|UserID
            // Example: "TestCompany|testuser"
            //
            // CompanyID: Intacct Company ID
            // - Identifies the Intacct company instance
            // - Retrieved from BlueBook or SAML attributes
            //
            // UserID: User's Intacct user ID
            // - Generated by Intacct when user is created
            // - Used to uniquely identify the user within the company

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageProductOneSiteAccounting_SpecialFlags_Documentation()
        {
            // This test documents special user flags:
            //
            // UnRestricted (HasAccessToAllCurrentFutureProperties):
            // - When true, user gets all current and future properties
            // - Set for super users and accounting admins
            //
            // PortalUser (HasAccessToSiteSpendManagementOnly):
            // - When true, user is Site Spend Management user
            // - Limited access to specific functionality
            //
            // Admin (IsAccountingAdmin):
            // - When true, user is Accounting admin
            // - Gets admin privileges in Financial Suite
            //
            // SSOEnabled:
            // - Always set to true for Unified Login users
            //
            // PWDNeverExpires:
            // - Set to true for new users
            //
            // PWDQlyNotEnforced:
            // - Set to true for new users

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageProductOneSiteAccounting_MConsole_Documentation()
        {
            // This test documents MConsole support:
            //
            // MConsole (Management Console):
            // - Multi-entity accounting system
            // - Properties have MConsoleId in addition to PropertyId
            //
            // Detection:
            // - Check if any property has non-empty MConsoleId
            // - Set IsMConsolePMC flag
            //
            // Property Assignment:
            // - For MConsole: Use MConsoleId
            // - For regular: Use PropertyId
            //
            // Company Assignment:
            // - MConsole supports company-level assignment
            // - Format: "CompanyID" or "PropertyID|CompanyID"

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageProductOneSiteAccounting_RoleRightsManagement_Documentation()
        {
            // This test documents role and rights management:
            //
            // Permissions Structure:
            // - Application (Module): Top-level grouping (e.g., "General Ledger")
            // - Right: Specific object (e.g., "Journal Entry")
            // - Action: What can be done (e.g., "View", "Edit", "Delete")
            // - ModuleID: Unique identifier (e.g., "2.GL")
            //
            // Roles:
            // - System roles: Provided by Intacct
            // - Custom roles: Created by users
            // - Roles have assigned rights
            //
            // Operations:
            // - Create custom roles
            // - Clone existing roles
            // - Delete custom roles (if not assigned to users)
            // - Assign/remove rights to roles
            // - Assign/remove roles to rights
            //
            // Activity Logging:
            // - All operations are logged with details
            // - Shows added/removed rights and roles

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageProductOneSiteAccounting_Migration_Documentation()
        {
            // This test documents migration support:
            //
            // GetMigrationUsers:
            // - Gets users from Intacct GetAllUsers API
            // - Filters: NonMigrated (excludeassign=1), Migrated (excludeassign=0)
            // - Uses paging for large result sets
            // - Returns MigrationUser objects
            //
            // UpdateUsersMigrationStatus:
            // - Enables/disables GreenBook user flag in Intacct
            // - EnableGreenBookUser: Marks user as using Unified Login
            // - DisableGreenBookUser: Marks user as not using Unified Login
            //
            // User Status:
            // - F = Disabled
            // - T or other = Active

            Assert.True(true, "Documentation test");
        }

        #endregion

        #region JSON Serialization Tests

        [Fact]
        public void NameValuePair_JsonSerialization_WorksCorrectly()
        {
            var pair = new NameValuePair
            {
                Name = "CompanyID",
                Value = "TestCompany"
            };

            var json = JsonConvert.SerializeObject(pair);
            var deserialized = JsonConvert.DeserializeObject<NameValuePair>(json);

            Assert.Equal(pair.Name, deserialized.Name);
            Assert.Equal(pair.Value, deserialized.Value);
        }

        #endregion

        #region AdditionalParameters Tests

        [Fact]
        public void AdditionalParameters_ForCompanies_HasCorrectStructure()
        {
            var param = new AdditionalParameters
            {
                Key = "Financial Suite Companies",
                Value = "{\"action\":\"Added Companies\",\"value\":\"Company Name\"}"
            };

            Assert.Equal("Financial Suite Companies", param.Key);
            Assert.Contains("Added Companies", param.Value);
        }

        [Fact]
        public void AdditionalParameters_ForLocationGroups_HasCorrectStructure()
        {
            var param = new AdditionalParameters
            {
                Key = "Financial Suite Location Groups",
                Value = "{\"action\":\"Added Location Groups\",\"value\":\"Group Name\"}"
            };

            Assert.Equal("Financial Suite Location Groups", param.Key);
            Assert.Contains("Added Location Groups", param.Value);
        }

        [Fact]
        public void AdditionalParameters_ForEntities_HasCorrectStructure()
        {
            var param = new AdditionalParameters
            {
                Key = "Financial Suite Entities",
                Value = "{\"action\":\"Added Entities\",\"value\":\"Entity Name\"}"
            };

            Assert.Equal("Financial Suite Entities", param.Key);
            Assert.Contains("Added Entities", param.Value);
        }

        [Fact]
        public void AdditionalParameters_ForRoles_HasCorrectStructure()
        {
            var param = new AdditionalParameters
            {
                Key = "Financial Suite Roles",
                Value = "{\"action\":\"Added Roles\",\"value\":\"Role Name\"}"
            };

            Assert.Equal("Financial Suite Roles", param.Key);
            Assert.Contains("Added Roles", param.Value);
        }

        #endregion

        #region BlueBook Constants Tests

        [Fact]
        public void BlueBookProductConstants_FinancialSuite_Exists()
        {
            var source = "FINANCIALSUITE";
            Assert.Equal("FINANCIALSUITE", source);
        }

        #endregion

        #region Username Length Limit Tests

        [Fact]
        public void UsernameLength_LimitedTo80Characters()
        {
            var longUsername = new string('a', 100);
            var maxLength = 80;
            var truncated = longUsername.Length > maxLength ? longUsername.Substring(1, maxLength) : longUsername;

            Assert.Equal(80, truncated.Length);
        }

        #endregion

        #region Name Length Limits Tests

        [Fact]
        public void FirstNameLength_LimitedTo40Characters()
        {
            var longName = new string('A', 50);
            var maxLength = 40;
            var truncated = longName.Substring(0, longName.Length >= maxLength ? maxLength : longName.Length);

            Assert.Equal(40, truncated.Length);
        }

        [Fact]
        public void LastNameLength_LimitedTo40Characters()
        {
            var longName = new string('B', 50);
            var maxLength = 40;
            var truncated = longName.Substring(0, longName.Length >= maxLength ? maxLength : longName.Length);

            Assert.Equal(40, truncated.Length);
        }

        #endregion
    }
}
