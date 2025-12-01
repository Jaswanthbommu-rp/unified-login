using Moq;
using UnifiedLogin.BusinessLogic.Security;
using UnifiedLogin.DataAccess.Security;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace UnifiedLogin.LandingAPI.Test.Logic.Security
{
	/// <summary>
	/// 
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ManageSecurityTests
    {

        private ManageSecurity _manageSecurity;
        private Mock<IPersonaRightRepository> _mockPersonaRightRepository;

        public ManageSecurityTests()
        {
            _mockPersonaRightRepository = new Mock<IPersonaRightRepository>();
            DefaultUserClaim defaultUserClaim = new DefaultUserClaim();
            _manageSecurity = new ManageSecurity(defaultUserClaim, _mockPersonaRightRepository.Object);
        }

        [Fact]
        public void GetPersonaRightsAndActionsByRoute_When_Passed_Invalid_Parameters_Should_Return_Error()
        {
            //Arrange
            var expected = new ObjectOutput<RouteSecurity, IErrorData>()
            {
                Status = new Status<IErrorData>()
                {
                    ErrorCode = "100.1",
                    ErrorMsg = "Invalid persona Id or route id.",
                    Success = false
                }
            };
            var routeId = "";
            var personaId = 0;

            //Act
            var actual = _manageSecurity.GetPersonaRightsAndActionsByRoute(personaId, routeId);

            //Assert
            Assert.Equal(actual.Status.Success, expected.Status.Success);
            Assert.Equal(actual.Status.ErrorCode, expected.Status.ErrorCode);
            Assert.Equal(actual.Status.ErrorMsg, expected.Status.ErrorMsg);
        }

        [Fact]
        public void GetPersonaRightsAndActionsByRoute_When_Passed_Valid_Parameters_Should_Return_Access_Rights()
        {
            //Arrange

            var expected = new ObjectOutput<RouteSecurity, IErrorData>()
            {
                obj = new RouteSecurity()
                {
                    RouteId = "Userslist",
                    Rights = new List<string>(new[] { "Create User", "Edit User", "Lock/Unlock User" })
                },
                Status = new Status<IErrorData>()
                {
                    Success = true
                }
            };
            var routeId = "Userslist";
            var personaId = 3;

            var accessRights = new List<PersonaActionRight>();
            accessRights.Add(new PersonaActionRight() { ActionId = 1, ObjectType = "Route", Action = "Userslist", ParentActionId = null, ActionvalueTypeId = 1 });
            accessRights.Add(new PersonaActionRight() { ActionId = 1, ObjectType = "Right", Action = "Create User", ParentActionId = null, ActionvalueTypeId = 1 });
            accessRights.Add(new PersonaActionRight() { ActionId = 1, ObjectType = "Right", Action = "Edit User", ParentActionId = null, ActionvalueTypeId = 1 });
            accessRights.Add(new PersonaActionRight() { ActionId = 1, ObjectType = "Right", Action = "Lock/Unlock User", ParentActionId = null, ActionvalueTypeId = 1 });

            _mockPersonaRightRepository
               .Setup(m => m.ListRightsAndActionsByPersonaId(personaId, routeId))
               .Returns(accessRights);

            //Act
            var actual = _manageSecurity.GetPersonaRightsAndActionsByRoute(personaId, routeId);

            //Assert
            Assert.Equal(actual.obj.Rights.Count, expected.obj.Rights.Count);
            Assert.Equal(actual.obj.RouteId, expected.obj.RouteId);
            Assert.Equal(actual.obj.Rights.First(), expected.obj.Rights.First());
            Assert.Equal(actual.obj.Rights.Last(), expected.obj.Rights.Last());

            DefaultUserClaim userClaim = new DefaultUserClaim(){ OrganizationRealPageGuid = DefaultUserClaim.ExternalCompanyRealPageId};
            _manageSecurity = new ManageSecurity(userClaim, _mockPersonaRightRepository.Object);
            actual = _manageSecurity.GetPersonaRightsAndActionsByRoute(personaId, routeId);

            // should exclude Create User because it is external company
            Assert.Equal(actual.obj.Rights.Count, expected.obj.Rights.Count-1);
        }

        [Fact]
        public void GetPersonaRightsAndActionsByRoute_When_Passed_Valid_Parameters_With_No_Rights_Should_Not_Throw_Exception()
        {
            //Arrange

            var expected = new RouteSecurity();
            var routeId = "un-known-route";
            var personaId = 3;

            var accessRights = new List<PersonaActionRight>();
           
            _mockPersonaRightRepository
               .Setup(m => m.ListRightsAndActionsByPersonaId(personaId, routeId))
               .Returns(accessRights);

            //Act
            var actual = _manageSecurity.GetPersonaRightsAndActionsByRoute(personaId, routeId);

            //Assert
            Assert.Equal(actual.obj.Rights.Count, expected.Rights.Count);
            Assert.Equal(actual.obj.RouteId, expected.RouteId);
        }

    }
}
