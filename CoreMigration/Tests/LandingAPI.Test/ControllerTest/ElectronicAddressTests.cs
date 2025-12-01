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
	/// Electronic Address xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ElectronicAddressTests
	{
		#region Controller Unit Tests
		[Fact]
		public void LinkElectronicAddress_InvalidRealPageId_ExceptionThrown()
		{
			//Arrange
			Guid realPageId = new Guid();
			ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType()
			{
				ContactMechanismUsageTypeId = 201,
				ParentContactMechanismUsageTypeId = 200,
				Name = "Primary"
			};

			ElectronicAddress electronicAddress = new ElectronicAddress()
			{
				PartyContactMechanismId = 1,
				ContactMechanismId = 1,
				AddressString = "none@nowhere.com",
				AddressType = "Email",
				contactMechanismUsageType = contactMechanismUsageType
			};

			PartyContactMechanism partyContactMechanism = new PartyContactMechanism()
			{
				FromDate = DateTime.UtcNow,
				ThruDate = DateTime.MaxValue.ToUniversalTime()
			};

			LinkElectronicAddress linkElectronicAddress = new LinkElectronicAddress()
			{
				ElectronicAddress = electronicAddress,
				PartyContactMechanism = partyContactMechanism,
				ContactMechanismUsageType = contactMechanismUsageType
			};

			ElectronicAddressController electronicAddressController = new ElectronicAddressController();

			//Act
			Exception exception = Record.Exception(() => electronicAddressController.LinkElectronicAddress(realPageId, linkElectronicAddress));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void LinkElectronicAddress_InvalidLinkElectronicAddress_ExceptionThrown()
		{
			//Arrange
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			LinkElectronicAddress linkElectronicAddress = new LinkElectronicAddress();
			ElectronicAddressController electronicAddressController = new ElectronicAddressController();

			//Act
			Exception exception = Record.Exception(() => electronicAddressController.LinkElectronicAddress(realPageId, linkElectronicAddress));

			//Assert
			Assert.IsType<Exception>(exception);
		}

		[Fact]
		public void LinkElectronicAddress_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("LinkElectronicAddress" == baseTest.VerifyRouteToAction(
				HttpMethod.Post,
				"https://landingapi.local/api/persons/13E71DE5-BAFA-469D-9F7A-E12DB3961BA9/electronicaddress?contactMechanismUsageTypeId=1"
				)
			);
		}

		[Fact]
		public void ListElectronicAddressForPerson_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("ListElectronicAddressForPerson" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/persons/13E71DE5-BAFA-469D-9F7A-E12DB3961BA9/electronicaddress"
				)
			);
		}

		[Fact]
		public void ListElectronicAddressForPerson_InvalidRealPageId_ExceptionThrown()
		{
			//Arrange
			Guid realPageId = new Guid();
			ElectronicAddressController electronicAddressController = new ElectronicAddressController();

			//Act
			Exception exception = Record.Exception(() => electronicAddressController.ListElectronicAddressForPerson(realPageId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void ListElectronicAddressForPerson_MockRepository_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			string ContactMechanismUsageTypeName = "";
			ObjectListOutput<ElectronicAddress, IErrorData> electronicAddressListOutput = new ObjectListOutput<ElectronicAddress, IErrorData>();
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Type type = typeof(ElectronicAddress);
			ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType()
			{
				ContactMechanismUsageTypeId = 201,
				ParentContactMechanismUsageTypeId = 200,
				Name = "Primary"
			};
			ElectronicAddress electronicAddress = new ElectronicAddress()
			{
				PartyContactMechanismId = 1,
				ContactMechanismId = 1,
				AddressString = "none@nowhere.com",
				AddressType = "Email",
				contactMechanismUsageType = contactMechanismUsageType
			};

			List<ElectronicAddress> expectedElectronicAddressList = new List<ElectronicAddress>();
			expectedElectronicAddressList.Add(electronicAddress);

			var mockRepository = new Mock<IElectronicAddressRepository>();
			mockRepository
				.Setup(m => m.ListElectronicAddressForPerson(realPageId, ContactMechanismUsageTypeName))
				.Returns(() => expectedElectronicAddressList);

			ElectronicAddressController controller = new ElectronicAddressController(mockRepository.Object);
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpClient();

			//Act
			int NumberOfProperties = type.GetProperties().Length;
			HttpResponseMessage response = controller.ListElectronicAddressForPerson(realPageId);
			electronicAddressListOutput = response.Content.ReadAsAsync<ObjectListOutput<ElectronicAddress, IErrorData>>().Result;

			//Assert
			Assert.True(response.IsSuccessStatusCode);
			Assert.Equal(response.RequestMessage.Method.ToString(), "GET");
			Assert.Equal(electronicAddressListOutput.list.Count, 1);
			Assert.True(electronicAddressListOutput.list.Count == expectedElectronicAddressList.Count);
		}
		#endregion
	}
}
