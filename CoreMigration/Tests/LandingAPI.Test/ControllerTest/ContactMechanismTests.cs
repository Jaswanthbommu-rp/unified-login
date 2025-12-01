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
	/// Contact Mechanism xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ContactMechanismTests
	{
		#region Controller Unit Tests
		[Fact]
		public void ListContactMechanismForPerson_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("ListContactMechanismForPerson" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/persons/13E71DE5-BAFA-469D-9F7A-E12DB3961BA9/contactmechanism"
				)
			);
		}

		[Fact]
		public void ListContactMechanismForPerson_InvalidRealPageId_ExceptionThrown()
		{
			//Arrange
			Guid realPageId = new Guid();
			ContactMechanismController contactMechanismController = new ContactMechanismController();

			//Act
			Exception exception = Record.Exception(() => contactMechanismController.ListContactMechanismForPerson(realPageId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void ListContactMechanismForPerson_MockRepository_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			string ContactMechanismUsageTypeName = "";
			ObjectListOutput<CommonAddress, IErrorData> contactMechanismListOutput = new ObjectListOutput<CommonAddress, IErrorData>();

			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Type type = typeof(CommonAddress);
			ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType()
			{
				ContactMechanismUsageTypeId = 201,
				ParentContactMechanismUsageTypeId = 200,
				Name = "Primary"
			};

			CommonAddress commonAddress = new CommonAddress()
			{
				AddressString = "none@nowhere.com",
				AddressType = "Email",
				ContactMechanismId = 1,
				PartyContactMechanismId = 1,
				ContactMechanismUsageTypeId = 1,
				contactMechanismUsageType = contactMechanismUsageType
			};
			List<CommonAddress> expectedCommonAddressList = new List<CommonAddress>();
			expectedCommonAddressList.Add(commonAddress);

			var mockRepository = new Mock<IContactMechanismRepository>();
			mockRepository
				.Setup(m => m.ListContactMechanismForPerson(realPageId, ContactMechanismUsageTypeName))
				.Returns(() => expectedCommonAddressList);

			ContactMechanismController controller = new ContactMechanismController(mockRepository.Object);
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpClient();

			//Act
			int NumberOfProperties = type.GetProperties().Length;
			HttpResponseMessage response = controller.ListContactMechanismForPerson(realPageId);
			contactMechanismListOutput = response.Content.ReadAsAsync<ObjectListOutput<CommonAddress, IErrorData>>().Result;

			//Assert
			Assert.True(response.IsSuccessStatusCode);
			Assert.Equal(response.RequestMessage.Method.ToString(), "GET");
			Assert.Equal(contactMechanismListOutput.list.Count, 1);
			Assert.True(contactMechanismListOutput.list.Count == expectedCommonAddressList.Count);
		}
		#endregion
	}
}