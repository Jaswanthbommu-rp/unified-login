using Moq;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.LandingAPI.Test.Logic;
using UnifiedLogin.LandingAPI;
using UnifiedLogin.LandingAPI.Controllers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace UnifiedLogin.LandingAPI.Test.ControllerTest
{
	/// <summary>
	/// Party Relationship xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class PartyRelationshipTests
	{
		[Fact]
		public void LinkOrganizationToOrganization_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("LinkOrganizationToOrganization" == baseTest.VerifyRouteToAction(
				HttpMethod.Post,
				"http://localhost/api/organizations/13E71DE5-BAFA-469D-9F7A-E12DB3961BA9/relationships/organizations"
				)
			);
		}

		[Fact]
		public void LinkOrganizationToOrganization_InvalidOrganizationRealPageIdFrom_ExceptionThrown()
		{
			//Arrange
			Guid RealPageIdFrom = new Guid();
			Guid RealPageIdTo = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 1,
				RoleTypeIdTo = 2
			};

			PartyRelationshipController partyRelationshipController = new PartyRelationshipController();

			//Act
			Exception exception = Record.Exception(() => partyRelationshipController.LinkOrganizationToOrganization(RealPageIdFrom, partyRelationShip));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void LinkOrganizationToOrganization_InvalidOrganizationRealPageIdTo_ExceptionThrown()
		{
			//Arrange
			Guid RealPageIdFrom = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Guid RealPageIdTo = new Guid();
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 1,
				RoleTypeIdTo = 2
			};

			PartyRelationshipController partyRelationshipController = new PartyRelationshipController();

			//Act
			Exception exception = Record.Exception(() => partyRelationshipController.LinkOrganizationToOrganization(RealPageIdFrom, partyRelationShip));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void LinkOrganizationToOrganization_InvalidRoleTypeIdFrom_ExceptionThrown()
		{
			//Arrange
			Guid RealPageIdFrom = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Guid RealPageIdTo = new Guid("8946d26d-8ede-40d1-b6c3-d52bc903f202");
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 0,
				RoleTypeIdTo = 2
			};

			PartyRelationshipController partyRelationshipController = new PartyRelationshipController();

			//Act
			Exception exception = Record.Exception(() => partyRelationshipController.LinkOrganizationToOrganization(RealPageIdFrom, partyRelationShip));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void LinkOrganizationToOrganization_InvalidRoleTypeIdTo_ExceptionThrown()
		{
			//Arrange
			Guid RealPageIdFrom = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Guid RealPageIdTo = new Guid("8946d26d-8ede-40d1-b6c3-d52bc903f202");
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 1,
				RoleTypeIdTo = 0
			};

			PartyRelationshipController partyRelationshipController = new PartyRelationshipController();

			//Act
			Exception exception = Record.Exception(() => partyRelationshipController.LinkOrganizationToOrganization(RealPageIdFrom, partyRelationShip));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void LinkOrganizationToOrganization_MockRepository_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			PartyRelationship.PartyRelationshipOutputResult partyRelationshipOutputResult = new PartyRelationship.PartyRelationshipOutputResult();
			Type type = typeof(PartyRelationship);
			Guid realPageIdFrom = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Guid RealPageIdTo = new Guid("8946d26d-8ede-40d1-b6c3-d52bc903f202");
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 201,
				RoleTypeIdTo = 202
			};

			RepositoryResponse expectedRepositoryResponse = new RepositoryResponse()
			{
				Id = 1,
				ErrorMessage = "",
				RealPageId = Guid.Empty
			};

			var mockRepository = new Mock<IPartyRelationshipRepository>();
			mockRepository
				.Setup(m => m.LinkOrganizationToOrganization(realPageIdFrom, partyRelationShip))
				.Returns(() => expectedRepositoryResponse);

			PartyRelationshipController controller = new PartyRelationshipController(mockRepository.Object);
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpClient();

			//Act
			int NumberOfProperties = type.GetProperties().Length;
			HttpResponseMessage response = controller.LinkOrganizationToOrganization(realPageIdFrom, partyRelationShip);
			partyRelationshipOutputResult = response.Content.ReadAsAsync<PartyRelationship.PartyRelationshipOutputResult>().Result;

			//Assert
			Assert.True(response.IsSuccessStatusCode);
			Assert.Equal(partyRelationshipOutputResult.NewPartyRelationshipId, expectedRepositoryResponse.Id);
		}

		[Fact]
		public void LinkPersonToOrganization_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("LinkPersonToOrganization" == baseTest.VerifyRouteToAction(
				HttpMethod.Post,
				"http://localhost/api/persons/8946d26d-8ede-40d1-b6c3-d52bc903f202/relationships/organizations"
				)
			);
		}

		[Fact]
		public void LinkPersonToOrganization_InvalidPersonRealPageIdFrom_ExceptionThrown()
		{
			//Arrange
			Guid RealPageIdFrom = new Guid();
			Guid RealPageIdTo = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 1,
				RoleTypeIdTo = 2
			};

			PartyRelationshipController partyRelationshipController = new PartyRelationshipController();

			//Act
			Exception exception = Record.Exception(() => partyRelationshipController.LinkPersonToOrganization(RealPageIdFrom, partyRelationShip));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void LinkPersonToOrganization_InvalidOrganizationRealPageIdTo_ExceptionThrown()
		{
			//Arrange
			Guid RealPageIdFrom = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Guid RealPageIdTo = new Guid();
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 1,
				RoleTypeIdTo = 2
			};

			PartyRelationshipController partyRelationshipController = new PartyRelationshipController();

			//Act
			Exception exception = Record.Exception(() => partyRelationshipController.LinkPersonToOrganization(RealPageIdFrom, partyRelationShip));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void LinkPersonToOrganization_InvalidRoleTypeIdFrom_ExceptionThrown()
		{
			//Arrange
			Guid RealPageIdFrom = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Guid RealPageIdTo = new Guid("8946d26d-8ede-40d1-b6c3-d52bc903f202");
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 0,
				RoleTypeIdTo = 2
			};

			PartyRelationshipController partyRelationshipController = new PartyRelationshipController();

			//Act
			Exception exception = Record.Exception(() => partyRelationshipController.LinkPersonToOrganization(RealPageIdFrom, partyRelationShip));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void LinkPersonToOrganization_InvalidRoleTypeIdTo_ExceptionThrown()
		{
			//Arrange
			Guid RealPageIdFrom = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Guid RealPageIdTo = new Guid("8946d26d-8ede-40d1-b6c3-d52bc903f202");
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 1,
				RoleTypeIdTo = 0
			};

			PartyRelationshipController partyRelationshipController = new PartyRelationshipController();

			//Act
			Exception exception = Record.Exception(() => partyRelationshipController.LinkPersonToOrganization(RealPageIdFrom, partyRelationShip));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void LinkPersonToOrganization_MockRepository_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			PartyRelationship.PartyRelationshipOutputResult partyRelationshipOutputResult = new PartyRelationship.PartyRelationshipOutputResult();
			Type type = typeof(PartyRelationship);
			Guid realPageIdFrom = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Guid RealPageIdTo = new Guid("8946d26d-8ede-40d1-b6c3-d52bc903f202");
			PartyRelationship partyRelationShip = new PartyRelationship()
			{
				RealPageIdTo = RealPageIdTo,
				RoleTypeIdFrom = 201,
				RoleTypeIdTo = 202
			};

			RepositoryResponse expectedRepositoryResponse = new RepositoryResponse()
			{
				Id = 1,
				ErrorMessage = "",
				RealPageId = Guid.Empty
			};

			var mockRepository = new Mock<IPartyRelationshipRepository>();
			mockRepository
				.Setup(m => m.LinkPersonToOrganization(realPageIdFrom, partyRelationShip))
				.Returns(() => expectedRepositoryResponse);

			PartyRelationshipController controller = new PartyRelationshipController(mockRepository.Object);
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpClient();

			//Act
			int NumberOfProperties = type.GetProperties().Length;
			HttpResponseMessage response = controller.LinkPersonToOrganization(realPageIdFrom, partyRelationShip);
			partyRelationshipOutputResult = response.Content.ReadAsAsync<PartyRelationship.PartyRelationshipOutputResult>().Result;

			//Assert
			Assert.True(response.IsSuccessStatusCode);
			Assert.Equal(partyRelationshipOutputResult.NewPartyRelationshipId, expectedRepositoryResponse.Id);
		}
	}
}
