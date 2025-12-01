using UnifiedLogin.SharedObjects.Landing;
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
	[ExcludeFromCodeCoverage]
	public class PersonaTests
	{
        #region Controller Unit Tests
        [Fact]
		public void CreatePersona_InvalidPersona_ExceptionThrown()
		{
			//Arrange
			Guid personRealPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Guid organizationRealPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Persona persona = new Persona();
			PersonaController personaController = new PersonaController();

            //Act
            Exception exception = Record.Exception(() => personaController.CreatePersona(personRealPageId, organizationRealPageId, persona));

			//Assert
			Assert.IsType<Exception>(exception);
		}

		[Fact]
		public void CreatePersona_InvalidPersonRealPageId_ExceptionThrown()
		{
			//Arrange
			Guid personRealPageId = new Guid();
			Guid organizationRealPageId = new Guid("9e9410ae-2c41-47d2-81d1-109c08cd151c");

			Persona persona = new Persona()
			{
				PersonaId = 0,
				PersonaTypeId = 1,
				FromDate = DateTime.UtcNow,
				ThruDate = DateTime.MaxValue.ToUniversalTime()
			};
			PersonaController personaController = new PersonaController();

            //Act
            Exception exception = Record.Exception(() => personaController.CreatePersona(personRealPageId, organizationRealPageId, persona));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void CreatePersona_InvalidOrganizationRealPageId_ExceptionThrown()
		{
			//Arrange
			Guid personRealPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Guid organizationRealPageId = new Guid();

			Persona persona = new Persona()
			{
				PersonaId = 0,
				PersonaTypeId = 1,
				FromDate = DateTime.UtcNow,
				ThruDate = DateTime.MaxValue.ToUniversalTime()
			};
			PersonaController personaController = new PersonaController();

            //Act
            Exception exception = Record.Exception(() => personaController.CreatePersona(personRealPageId, organizationRealPageId, persona));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void CreatePersona_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("CreatePersona" == baseTest.VerifyRouteToAction(
				HttpMethod.Post,
				"http://localhost/api/persona?personRealPageId=13E71DE5-BAFA-469D-9F7A-E12DB3961BA9&organizationRealPageId=9e9410ae-2c41-47d2-81d1-109c08cd151c"
				)
			);
		}

		[Fact]
		public void GetPersona_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("GetPersona" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/persona?personaId=33"
				)
			);
		}

        [Fact]
        public void GetProductsByPersona_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpClient Config = new HttpClient();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("GetProductsByPersona" == baseTest.VerifyRouteToAction(
                HttpMethod.Get,
                "http://localhost/api/personas/products"
                )
            );
        }
        #endregion
    }
}
