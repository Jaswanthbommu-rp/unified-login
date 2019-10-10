using Moq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Security;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Security;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.Security;
using System;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using Xunit;
using System.Diagnostics.CodeAnalysis;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
	/// <summary>
	/// ManageDashboardContent xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ManageDashboardContentTest
    {
        #region Private Variables
        IManageDashboardContent _manageDashboard;
        Mock<IManageProfile> _mockmanageProfile = new Mock<IManageProfile>();
        Mock<IManageProduct> _mockManageProduct = new Mock<IManageProduct>();
        Mock<IManageSecurity> _manangeSecurityLogic = new Mock<IManageSecurity>();
        Mock<IPersonaRightRepository> _personaRightRepository = new Mock<IPersonaRightRepository>();
        #endregion

        [Fact]
        public void GetDashboardElementResponse_InvalidRealPageId_ThrowsException()
        {
            //Arrange
            Guid realPageId = Guid.Empty;
            Persona persona = null;
            DefaultUserClaim defaultUserClaim = new DefaultUserClaim();
            _manageDashboard = new ManageDashboardContent(defaultUserClaim, _mockmanageProfile.Object, _mockManageProduct.Object, _manangeSecurityLogic.Object);

            //Act
            Exception excepton = Record.Exception(() => _manageDashboard.GetDashboardElementResponse(realPageId, persona));

            //Assert
            Assert.IsType<ArgumentNullException>(excepton);
        }

        [Fact]
        public void GetDashboardElementResponse_InvalidPersona_ThrowsException()
        {
            //Arrange
            Guid realPageId = new Guid("c9167175-0676-4546-bba7-4a49d5809b1f");
            Persona persona = null;

            DefaultUserClaim defaultUserClaim = new DefaultUserClaim();
            _manageDashboard = new ManageDashboardContent(defaultUserClaim, _mockmanageProfile.Object, _mockManageProduct.Object, _manangeSecurityLogic.Object);

            //Act
            Exception excepton = Record.Exception(() => _manageDashboard.GetDashboardElementResponse(realPageId, persona));

            //Assert
            Assert.IsType<ArgumentNullException>(excepton);
        }

        [Fact]
        public void GetDashboardElementResponse_MockInputData_ReturnsValidDashboardElementResponse()
        {
            //Arrange
            Type type = typeof(DashboardElementResponse);
            int numberOfProperties = 0;
            DashboardElementResponse response = new DashboardElementResponse();
            Guid realPageId = new Guid("c9167175-0676-4546-bba7-4a49d5809b1f");
            IList<Persona> personas = new List<Persona>();
            
            Persona persona = new Persona()
            {
                PersonaId = 33,
                Name = "Primary",
                FromDate = DateTime.UtcNow
            };
            personas.Add(persona);
            
            IList<PersonaProductUserDetails> assignedProducts = new List<PersonaProductUserDetails>();
            assignedProducts.Add(new PersonaProductUserDetails
            {
                PersonaId = 33,
                ProductId = 1,
                ProductName = "OneSite"
            });
            
            ProfileDetail profileDetail = new ProfileDetail()
            {
                FirstName = "John",
                LastName = "Doe",
                PartyId = 1,
                RealPageId = realPageId,
                Persona = personas,
                AssignedProducts = assignedProducts
            };

	        IEnumerable<PersonaActionRight> personaRightList = new List<PersonaActionRight>()
	        {
		        new PersonaActionRight() {ActionId = 1, Action = "/someroute", ActionvalueTypeId = 1, ObjectType = "ROUTE", ParentActionId = 1},
		        new PersonaActionRight() {ActionId = 2, Action = "some right", ActionvalueTypeId = 2, ObjectType = "Right", ParentActionId = 1}
	        };

            DefaultUserClaim defaultUserClaim = new DefaultUserClaim();

            _mockmanageProfile
                .Setup(m => m.GetProfileDetail(It.IsAny<Guid>(), It.IsAny<long>(), null, null, null, null))
                .Returns(profileDetail);

            _mockManageProduct
                .Setup(m => m.GetUserAssignedProductsByPersona(It.IsAny<Persona>(), null, It.IsAny<RouteSecurity>()))
                .Returns(assignedProducts);

            _personaRightRepository
                .Setup(m => m.ListRightsAndActionsByPersonaId(It.IsAny<long>(), It.IsAny<string>()))
                .Returns(personaRightList);

            ManageSecurity manageSecurity = new ManageSecurity(defaultUserClaim, _personaRightRepository.Object);

            _manageDashboard = new ManageDashboardContent(defaultUserClaim, _mockmanageProfile.Object, _mockManageProduct.Object, manageSecurity);

            //Act
            response = _manageDashboard.GetDashboardElementResponse(realPageId, persona);
            numberOfProperties = type.GetProperties().Length;

            //Assert
            Assert.True(response.DashboardElements.ProfileDetail != null
                        && response.DashboardElements.ProfileDetail.RealPageId == realPageId
                        && response.DashboardElements.ProfileDetail.SummaryCount.TotalAssignedProducts == 1
                        && numberOfProperties == 3);
        }
    }
}
