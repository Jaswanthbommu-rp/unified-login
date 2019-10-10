using Moq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
	/// <summary>
	/// PartyRole xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ManagePartyRoleTest
    {
        [Fact]
        public void GetPartyRole_InvalidRealPageId_ExceptionThrown()
        {
            //Arrange
            Guid emptyRealPageId = Guid.Empty;
            Mock<IPartyRoleRepository> _mockPartyRoleRepository = new Mock<IPartyRoleRepository>();

            ManagePartyRole managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            //Act
            Exception exception = Record.Exception(() => managePartyRole.GetPartyRole(emptyRealPageId));

            //Assert
            Assert.IsType<Exception>(exception);
        }

        [Fact]
        public void GetPartyRole_MockInputData_ReturnValidRepositoryResponseObject()
        {
            //Arrange
            Type type = typeof(PartyRole);
            int numberOfProperties = 0;
            Guid realPageId = new Guid("C9167175-0676-4546-BBA7-4A49D5809B1F");
            int partyId = 33;
            int partyRoleId = 7;
            int roleTypeId = 309;
            PartyRole expectedPartyRole = new PartyRole()
            {
                Name = "Chief Accounting Officer",
                PartyId = partyId,
                PartyRoleId = partyRoleId,
                RoleTypeId = roleTypeId
            };

            Mock <IPartyRoleRepository> _mockPartyRoleRepository = new Mock<IPartyRoleRepository>();
            _mockPartyRoleRepository
                .Setup(m => m.GetPartyRoleByEnterpriseUserID(realPageId))
                .Returns(expectedPartyRole);

            ManagePartyRole managePartyRole = new ManagePartyRole(_mockPartyRoleRepository.Object);

            //Act
            PartyRole partyRole = managePartyRole.GetPartyRole(realPageId);
            numberOfProperties = type.GetProperties().Length;

            //Assert
            Assert.True(partyRole != null
                && partyRole.PartyId == partyId
                && partyRole.PartyRoleId == partyRoleId
                && partyRole.RoleTypeId == roleTypeId
                && numberOfProperties == 4);
        }
    }
}
