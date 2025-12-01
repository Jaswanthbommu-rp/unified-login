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
	/// User xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class UserTests
	{
        private readonly RouteTestBase _baseTest;

		#region Constructor
		public UserTests()
        {
            HttpClient config = new HttpClient();
            WebApiConfig.Register(config);
            config.EnsureInitialized();
            DefaultHttpControllerSelector controllerSelector = new DefaultHttpControllerSelector(config);
            _baseTest = new RouteTestBase(config, controllerSelector);
        }
		#endregion

		#region Controller Unit Tests		
		[Fact]
		public void GetUserProfile_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("GetUserProfile" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/user/13E71DE5-BAFA-469D-9F7A-E12DB3961BA9"
				)
			);
		}

		[Fact]
		public void UserCustomFields_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("UserCustomFields" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/customfields"
				)
			);
		}

		[Fact]
        public void CreateNewUser_VerifyRouteToAction_ReturnAction()
        {
            //Arrange

            //Act

            //Assert
            Assert.True("CreateUser" == _baseTest.VerifyRouteToAction(
                HttpMethod.Post,
                "http://localhost/api/user"
                )
            );

            Assert.True("CreateUser" == _baseTest.VerifyRouteToAction(
                HttpMethod.Post,
                "http://localhost/api/user"
                )
            );

            Assert.True("CreateUser" == _baseTest.VerifyRouteToAction(
                HttpMethod.Post,
                "http://localhost/api/user"
                )
            );
        }

        [Fact]
        public void UpdateNewUser_VerifyRouteToAction_ReturnAction()
        {
            //Arrange

            //Act

            //Assert
            Assert.True("UpdateNewUser" == _baseTest.VerifyRouteToAction(
                HttpMethod.Post,
                "http://localhost/api/newuser/profile?activityToken=41E2469D-4CAF-4FF4-B251-46CBC161A1C6&companyJobTitle=Leasing+Agent+I&userLogin=test@test1.com"
                )
            );
        }

		[Fact]
		public void AssignProductsToAdministrators_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("AssignProductsToAdministrators" == _baseTest.VerifyRouteToAction(
				HttpMethod.Post,
				"http://localhost/api/user/assignproductstoadministrators?organizationRealPageId=8DA6737A-55FA-4DEA-BE2B-6AA2BE620A57&assignUserPersonaId=1"
				)
			);
		}

		[Fact]
		public void AssignProductsToAdministrators_InvalidOrganizationRealPageId_ExceptionThrown()
		{
			//Arrange
			Guid realPageId = Guid.Empty;
			UserController userController = new UserController();

			//Act
			Exception exception = Record.Exception(() => userController.AssignProductsToAdministrators(realPageId, 1));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void AssignProductsToAdministrators_InvalidAssignUserPersonaId_ExceptionThrown()
		{
			//Arrange
			Guid realPageId = Guid.Empty;
			UserController userController = new UserController();

			//Act
			Exception exception = Record.Exception(() => userController.AssignProductsToAdministrators(realPageId, -1));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}
		#endregion
	}
}
