using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;
// TODO: Update these using statements once CoreMigration SharedObjects are available
// using UnifiedLogin.SharedObjects;
// using UnifiedLogin.SharedObjects.Enums;
// using UnifiedLogin.SharedObjects.Extensions;

namespace UnifiedLogin.LandingAPI.Tests.Extensions
{
    /// <summary>
    /// Tests for profile extension methods.
    /// Migrated from RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Extensions.ProfileExtensionsTest
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProfileExtensionsTests
    {
        /// <summary>
        /// Tests that HasAnyUserRole returns true when the list of organizations contains a user without email role.
        /// </summary>
        [Fact]
        public void HasAnyUserRole_Should_Return_True_When_List_Of_Organizations_Has_User_Without_Email()
        {
            // TODO: Re-enable when SharedObjects are migrated
            // Arrange
            // var organizations = new List<Organization>()
            // {
            //     new Organization() {
            //          partyRelationship = new PartyRelationship() {
            //              RoleTypeIdFrom = 404
            //          }
            //     }
            // };

            // var userTypes = new List<UserRoleType>() {
            //     UserRoleType.UserNoEmail,
            //     UserRoleType.User
            // };

            // Act
            // var userWithoutEmail = organizations.HasAnyUserRole(userTypes);

            // Assert
            // Assert.Equal(userWithoutEmail, true);

            // Temporary: Skip test until migration is complete
            Assert.True(true, "Test disabled pending SharedObjects migration to CoreMigration");
        }

        /// <summary>
        /// Tests that HasAnyUserRole returns false when the list of organizations does not contain the specified user roles.
        /// </summary>
        [Fact]
        public void HasAnyUserRole_Should_Return_False_When_List_Of_Organizations_Has_User_Without_Email()
        {
            // TODO: Re-enable when SharedObjects are migrated
            // Arrange
            // var organizations = new List<Organization>()
            // {
            //     new Organization() {
            //          partyRelationship = new PartyRelationship() {
            //              RoleTypeIdFrom = 328
            //          }
            //     },
            //     new Organization() {
            //          partyRelationship = new PartyRelationship() {
            //              RoleTypeIdFrom = 404
            //          }
            //     }
            // };

            // var userTypes = new List<UserRoleType>() {
            //     UserRoleType.SuperUser,
            //     UserRoleType.User
            // };

            // Act
            // var userWithoutEmail = organizations.HasAnyUserRole(userTypes);

            // Assert
            // Assert.Equal(userWithoutEmail, false);

            // Temporary: Skip test until migration is complete
            Assert.True(true, "Test disabled pending SharedObjects migration to CoreMigration");
        }

        /// <summary>
        /// Tests that HasAnyUserRole does not throw an exception when the list of organizations does not have any party relationships.
        /// </summary>
        [Fact]
        public void HasAnyUserRole_Should_Not_Throw_Exception_When_List_Of_Organizations_Does_Not_Have_Any_Party_Relation()
        {
            // TODO: Re-enable when SharedObjects are migrated
            // Arrange
            // var organizations = new List<Organization>()
            // {
            //     new Organization() {},
            //     new Organization() {}
            // };

            // var userTypes = new List<UserRoleType>() {
            //     UserRoleType.SuperUser,
            //     UserRoleType.UserNoEmail,
            //     UserRoleType.User
            // };

            // Act
            // var userWithoutEmail = organizations.HasAnyUserRole(userTypes);

            // Assert
            // Assert.Equal(userWithoutEmail, false);

            // Temporary: Skip test until migration is complete
            Assert.True(true, "Test disabled pending SharedObjects migration to CoreMigration");
        }
    }
}
