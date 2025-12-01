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
	/// Person xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class PersonTests
	{
        #region Controller Unit Tests
        [Fact]
		public void CreatePerson_InvalidPerson_ExceptionThrown()
		{
			//Arrange
			Person person = new Person();
			PersonController personController = new PersonController();

			//Act
			Exception exception = Record.Exception(() => personController.CreatePerson(person));

			//Assert
			Assert.IsType<Exception>(exception);
		}

		[Fact]
		public void CreatePerson_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("CreatePerson" == baseTest.VerifyRouteToAction(
				HttpMethod.Post,
				"http://localhost/api/persons"
				)
			);
		}

		[Fact]
		public void GetPerson_InvalidRealPageId_ExceptionThrown()
		{
			//Arrange
			Guid realPageId = new Guid();
			PersonController personController = new PersonController();

			//Act
			Exception exception = Record.Exception(() => personController.GetPerson(realPageId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void GetPerson_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("GetPerson" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/persons/13E71DE5-BAFA-469D-9F7A-E12DB3961BA9"
				)
			);
		}

		[Fact]
		public void UpdatePerson_InvalidRealPageId_ExceptionThrown()
		{
			//Arrange
			Guid realPageId = new Guid();
			Person person = new Person()
			{
				FirstName = "Jack",
				MiddleName = "",
				LastName = "Bauer",
				Title = "CTU Agent",
				Suffix = ""
			};
			PersonController personController = new PersonController();

			//Act
			Exception exception = Record.Exception(() => personController.UpdatePerson(realPageId, person));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void UpdatePerson_InvalidPerson_ExceptionThrown()
		{
			//Arrange
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Person person = new Person();
			PersonController personController = new PersonController();

			//Act
			Exception exception = Record.Exception(() => personController.UpdatePerson(realPageId, person));

			//Assert
			Assert.IsType<Exception>(exception);
		}

		[Fact]
		public void UpdatePerson_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("UpdatePerson" == baseTest.VerifyRouteToAction(
				HttpMethod.Put,
				"http://localhost/api/persons/13E71DE5-BAFA-469D-9F7A-E12DB3961BA9"
				)
			);
		}

        [Fact]
        public void ListPersons_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpClient Config = new HttpClient();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("ListPersons" == baseTest.VerifyRouteToAction(
                HttpMethod.Get,
                "http://localhost/api/persons"
                )
            );
        }

		[Fact]
		public void ListUsersExport_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("ListUsersExport" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/persons/export"
				)
			);
		}
		#endregion
	}
}
