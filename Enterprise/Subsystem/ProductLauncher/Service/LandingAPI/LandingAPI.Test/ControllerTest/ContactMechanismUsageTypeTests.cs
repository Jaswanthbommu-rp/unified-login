using Moq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest
{
	/// <summary>
	/// ContactMechanismUsageType xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ContactMechanismUsageTypeTests
	{
		#region Controller Unit Tests
		[Fact]
		public void ListContactMechanismUsageType_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpConfiguration Config = new HttpConfiguration();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("ListContactMechanismUsageType" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/contactmechanismusagetypes"
				)
			);
		}

		[Fact]
		public void ListContactMechanismUsageType_MockRepository_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			string ContactMechanismUsageTypeName = "";
			ObjectListOutput<ContactMechanismUsageType, IErrorData> contactMechanismUsageTypeListOutput = new ObjectListOutput<ContactMechanismUsageType, IErrorData>();
			Type type = typeof(ContactMechanismUsageType);
			ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType()
			{
				ContactMechanismUsageTypeId = 101,
				ParentContactMechanismUsageTypeId = 100,
				Name = "Primary"
			};

			List<ContactMechanismUsageType> expectedContactMechanismUsageTypeList = new List<ContactMechanismUsageType>();
			expectedContactMechanismUsageTypeList.Add(contactMechanismUsageType);

			contactMechanismUsageType = new ContactMechanismUsageType()
			{
				ContactMechanismUsageTypeId = 102,
				ParentContactMechanismUsageTypeId = 100,
				Name = "Secondary"
			};
			expectedContactMechanismUsageTypeList.Add(contactMechanismUsageType);

			var mockRepository = new Mock<IContactMechanismUsageTypeRepository>();
			mockRepository
				.Setup(m => m.ListContactMechanismUsageType(ContactMechanismUsageTypeName))
				.Returns(() => expectedContactMechanismUsageTypeList);

			ContactMechanismUsageTypeController controller = new ContactMechanismUsageTypeController(mockRepository.Object);
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpConfiguration();

			//Act
			int NumberOfProperties = type.GetProperties().Length;
			HttpResponseMessage response = controller.ListContactMechanismUsageType(ContactMechanismUsageTypeName);
			contactMechanismUsageTypeListOutput = response.Content.ReadAsAsync<ObjectListOutput<ContactMechanismUsageType, IErrorData>>().Result;

			//Assert
			Assert.True(response.IsSuccessStatusCode);
			Assert.Equal(response.RequestMessage.Method.ToString(), "GET");
			Assert.Equal(contactMechanismUsageTypeListOutput.list.Count, 2);
			Assert.True(contactMechanismUsageTypeListOutput.list.Count == expectedContactMechanismUsageTypeList.Count);
		}
		#endregion
	}
}
