using Moq;
using UnifiedLogin.BusinessLogic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
	[ExcludeFromCodeCoverage]
	public class ManageUserLoginPersonaTests
	{
		#region Private Variables
		IManageUserLoginPersona _manageUserLoginPersona;
		Mock<IUserLoginPersonaRepository> _mockUserLoginPersonaRepository = new Mock<IUserLoginPersonaRepository>();
		DefaultUserClaim _userClaim = new DefaultUserClaim() { PersonaId = 1234, OrganizationRealPageGuid = new Guid(), UserRealPageGuid = new Guid() };
		#endregion

		[Fact]
		public void ListUserLoginPersona_Mock_ReturnValidUserLoginPersonaList()
		{
			//Arrange
			long? userLoginPersonaId = null;
			long? userLoginId = 1;
			long? organizationPartyId = 350;
			Type type = typeof(UserLoginPersona);

			IList<UserLoginPersona> expectedUserLoginPersonaList = new List<UserLoginPersona>()
			{
				new UserLoginPersona()
				{
					UserLoginId = 1,
					FromDate = DateTime.UtcNow,
					PrimaryOrganization = true,
					ThruDate = DateTime.MaxValue,
					StatusThruDate = DateTime.MaxValue,
					StatusTypeId = 1,
					UserLoginPersonaId = 1,
					IsRealPartner = true,
				}
			};

			_mockUserLoginPersonaRepository = new Mock<IUserLoginPersonaRepository>();
			_mockUserLoginPersonaRepository
				.Setup(m => m.ListUserLoginPersona(userLoginPersonaId, userLoginId, organizationPartyId))
				.Returns(() => expectedUserLoginPersonaList);

			//Act
			int NumberOfProperties = type.GetProperties().Length;
			_manageUserLoginPersona = new ManageUserLoginPersona(_mockUserLoginPersonaRepository.Object, _userClaim);
			IList<UserLoginPersona> userLoginPersonaList = _manageUserLoginPersona.ListUserLoginPersona(userLoginPersonaId, userLoginId, organizationPartyId);

			//Assert
			Assert.True(
				userLoginPersonaList.Count == expectedUserLoginPersonaList.Count
				&& userLoginPersonaList.All(
					o => expectedUserLoginPersonaList.Any(
						w => w.UserLoginPersonaId == o.UserLoginPersonaId
						&&
						w.UserLoginId == o.UserLoginId
						&&
						w.FromDate == o.FromDate
						&&
						w.PrimaryOrganization == o.PrimaryOrganization
						&&
						w.ThruDate == o.ThruDate
						&&
						w.StatusThruDate == o.StatusThruDate
						&&
						w.StatusTypeId == o.StatusTypeId
						&& 
						w.IsDelegateAdmin == o.IsDelegateAdmin
						&& w.IsRealPartner == o.IsRealPartner
					)
				) == true
				&& NumberOfProperties == 9
			);
		}
	}
}
