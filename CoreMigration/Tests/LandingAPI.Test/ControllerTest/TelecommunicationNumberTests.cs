using Moq;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.LandingAPI.Test.Logic;
using UnifiedLogin.LandingAPI;
using UnifiedLogin.LandingAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace UnifiedLogin.LandingAPI.Test.ControllerTest
{
	/// <summary>
	/// TelecommunicationNumber xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class TelecommunicationNumberTests
	{
		#region Controller Unit Tests
		[Fact]
		public void LinkTelecommunicationNumber_InvalidRealPageId_ExceptionThrown()
		{
			//Arrange
			Guid realPageId = new Guid();
			PartyContactMechanism partyContactMechanism = new PartyContactMechanism()
			{
				FromDate = DateTime.UtcNow,
				ThruDate = DateTime.MaxValue.ToUniversalTime()
			};

			ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType()
			{
				ContactMechanismUsageTypeId = 201,
				ParentContactMechanismUsageTypeId = 200,
				Name = "Primary"
			};

			TelecommunicationNumber telecommunicationNumber = new TelecommunicationNumber()
			{
				CountryCode = "44",
				AreaCode = "7911",
				PhoneNumber = "123456",
				IsDefault = true
			};

			LinkTelecommunicationNumber linkTelecommunicationNumber = new LinkTelecommunicationNumber()
			{
				PartyContactMechanism = partyContactMechanism,
				ContactMechanismUsageType = contactMechanismUsageType,
				TelecommunicationNumber = telecommunicationNumber
			};

			TelecommunicationNumberController telecommunicationNumberController = new TelecommunicationNumberController();

			//Act
			Exception exception = Record.Exception(() => telecommunicationNumberController.LinkTelecommunicationNumber(realPageId, linkTelecommunicationNumber));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void LinkTelecommunicationNumber_InvalidLinkTelecommunicationNumber_ExceptionThrown()
		{
			//Arrange
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			LinkTelecommunicationNumber linkTelecommunicationNumber = new LinkTelecommunicationNumber();
			TelecommunicationNumberController telecommunicationNumberController = new TelecommunicationNumberController();

			//Act
			Exception exception = Record.Exception(() => telecommunicationNumberController.LinkTelecommunicationNumber(realPageId, linkTelecommunicationNumber));

			//Assert
			Assert.IsType<Exception>(exception);
		}

		[Fact]
		public void LinkTelecommunicationNumber_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("LinkTelecommunicationNumber" == baseTest.VerifyRouteToAction(
				HttpMethod.Post,
				"https://landingapi.local/api/persons/13E71DE5-BAFA-469D-9F7A-E12DB3961BA9/telecommunicationnumber?contactMechanismUsageTypeId=1"
				)
			);
		}

		[Fact]
		public void ListTelecommunicationNumberForPerson_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("ListTelecommunicationNumberForPerson" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/persons/13E71DE5-BAFA-469D-9F7A-E12DB3961BA9/telecommunicationnumber"
				)
			);
		}

		[Fact]
		public void ListTelecommunicationNumberForPerson_InvalidRealPageId_ExceptionThrown()
		{
			//Arrange
			Guid realPageId = new Guid();
			TelecommunicationNumberController telecommunicationNumberController = new TelecommunicationNumberController();

			//Act
			Exception exception = Record.Exception(() => telecommunicationNumberController.ListTelecommunicationNumberForPerson(realPageId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void ListTelecommunicationNumberForPerson_MockRepository_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			string ContactMechanismUsageTypeName = "";

			ObjectListOutput< TelecommunicationNumber, IErrorData> telecommunicationNumberListOutput = new ObjectListOutput<TelecommunicationNumber, IErrorData>();
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Type type = typeof(TelecommunicationNumber);
			ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType()
			{
				ContactMechanismUsageTypeId = 201,
				ParentContactMechanismUsageTypeId = 200,
				Name = "Primary"
			};
			TelecommunicationNumber telecommunicationNumber = new TelecommunicationNumber()
			{
				PartyContactMechanismId = 1,
				ContactMechanismId = 1,
				CountryCode = "1",
				AreaCode = "972",
				IsDefault = true,
				PhoneNumber = "8203000",
				contactMechanismUsageType = contactMechanismUsageType
			};

			List<TelecommunicationNumber> expectedTelecommunicationNumberList = new List<TelecommunicationNumber>();
			expectedTelecommunicationNumberList.Add(telecommunicationNumber);

			var mockRepository = new Mock<ITelecommunicationNumberRepository>();
			mockRepository
				.Setup(m => m.ListTelecommunicationNumberForPerson(realPageId, ContactMechanismUsageTypeName))
				.Returns(() => expectedTelecommunicationNumberList);

			TelecommunicationNumberController controller = new TelecommunicationNumberController(mockRepository.Object);
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpClient();

			//Act
			int NumberOfProperties = type.GetProperties().Length;
			HttpResponseMessage response = controller.ListTelecommunicationNumberForPerson(realPageId);
			telecommunicationNumberListOutput = response.Content.ReadAsAsync<ObjectListOutput<TelecommunicationNumber, IErrorData>>().Result;

			//Assert
			Assert.True(response.IsSuccessStatusCode);
			Assert.Equal(response.RequestMessage.Method.ToString(), "GET");
			Assert.Equal(telecommunicationNumberListOutput.list.Count, 1);
			Assert.True(telecommunicationNumberListOutput.list.Count == expectedTelecommunicationNumberList.Count);
		}
		#endregion
	}
}
