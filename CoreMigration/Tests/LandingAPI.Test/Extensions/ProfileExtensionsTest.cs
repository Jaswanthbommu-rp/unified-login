using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace UnifiedLogin.LandingAPI.Test.Extensions
{
	[ExcludeFromCodeCoverage]
    public class ProfileExtensionsTest
    {
        [Fact]
        public void HasAnyUserRole_Should_Return_True_When_List_Of_Organizations_Has_User_Without_Email()
        {
            //Arrange
            
            var organizations = new List<Organization>()
            {
                new Organization() {
                     partyRelationship = new PartyRelationship() {
                         RoleTypeIdFrom = 404
                     }
                }
            };


            var userTypes = new List<UserRoleType>() {
                UserRoleType.UserNoEmail,
                UserRoleType.User
            };

            //Act
            var userWithoutEmail = organizations.HasAnyUserRole(userTypes);
            //Assert
            Assert.Equal(userWithoutEmail, true);
        }

        [Fact]
        public void HasAnyUserRole_Should_Return_False_When_List_Of_Organizations_Has_User_Without_Email()
        {
            //Arrange

            var organizations = new List<Organization>()
            {
                new Organization() {
                     partyRelationship = new PartyRelationship() {
                         RoleTypeIdFrom = 328
                     }
                },
                new Organization() {
                     partyRelationship = new PartyRelationship() {
                         RoleTypeIdFrom = 404
                     }
                }
            };


            var userTypes = new List<UserRoleType>() {
                UserRoleType.SuperUser,
                UserRoleType.User
            };

            //Act
            var userWithoutEmail = organizations.HasAnyUserRole(userTypes);
            //Assert
            Assert.Equal(userWithoutEmail, false);
        }

        [Fact]
        public void HasAnyUserRole_Should_Not_Throw_Exception_When_List_Of_Organizations_Does_Not_Have_Any_Party_Relation()
        {
            //Arrange

            var organizations = new List<Organization>()
            {
                new Organization() {},
                new Organization() {}
            };


            var userTypes = new List<UserRoleType>() {
                UserRoleType.SuperUser,
                UserRoleType.UserNoEmail,
                UserRoleType.User
            };

            //Act
            var userWithoutEmail = organizations.HasAnyUserRole(userTypes);
            //Assert
            Assert.Equal(userWithoutEmail, false);
        }
    }
}
