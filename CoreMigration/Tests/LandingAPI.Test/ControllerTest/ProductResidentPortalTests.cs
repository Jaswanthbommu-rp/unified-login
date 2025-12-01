using UnifiedLogin.SharedObjects.Base;
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
	/// Product Resident Portal xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ProductResidentPortalTests
	{
		#region Controller Unit Tests
		[Fact]
		public void GetNotificationSettings_InvalidEditorPersonaId_ExceptionThrown()
		{
			//Arrange
			long editorPersonaId = 0;
			long userPersonaId = 10;
			ProductResidentPortalController productResidentPortalController = new ProductResidentPortalController();

			//Act
			Exception exception = Record.Exception(() => productResidentPortalController.GetNotificationSettings(editorPersonaId, userPersonaId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void GetNotificationSettings_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("GetNotificationSettings" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/products/residentportal/notifications?editorPersonaId=12&userPersonaId=123"
				)
			);
		}

		[Fact]
		public void GetResidentPortalUser_InvalidEditorPersonaId_ExceptionThrown()
		{
			//Arrange
			long editorPersonaId = 0;
			long userPersonaId = 10;
			ProductResidentPortalController productResidentPortalController = new ProductResidentPortalController();

			//Act
			Exception exception = Record.Exception(() => productResidentPortalController.GetResidentPortalUser(editorPersonaId, userPersonaId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void GetResidentPortalUser_InvalidUserPersonaId_ExceptionThrown()
		{
			//Arrange
			long editorPersonaId = 10;
			long userPersonaId = 0;
			ProductResidentPortalController productResidentPortalController = new ProductResidentPortalController();

			//Act
			Exception exception = Record.Exception(() => productResidentPortalController.GetResidentPortalUser(editorPersonaId, userPersonaId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void GetResidentPortalUser_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("GetResidentPortalUser" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/products/residentportal/user?editorPersonaId=12&userPersonaId=123"
				)
			);
		}

		[Fact]
		public void ListMessagingGroups_InvalidEditorPersonaId_ExceptionThrown()
		{
			//Arrange
			long editorPersonaId = 0;
			long userPersonaId = 10;
			ProductResidentPortalController productResidentPortalController = new ProductResidentPortalController();

			//Act
			Exception exception = Record.Exception(() => productResidentPortalController.ListMessagingGroups(editorPersonaId, userPersonaId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void ListMessagingGroups_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("ListMessagingGroups" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/products/residentportal/messagegroups?editorPersonaId=12&userPersonaId=123&assignedOnly=false"
				)
			);
		}

		[Fact]
		public void ListLevels_InvalidEditorPersonaId_ExceptionThrown()
		{
			//Arrange
			long editorPersonaId = 0;
			long userPersonaId = 10;
			ProductResidentPortalController productResidentPortalController = new ProductResidentPortalController();

			//Act
			Exception exception = Record.Exception(() => productResidentPortalController.ListLevels(editorPersonaId, userPersonaId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void ListLevels_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("ListLevels" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/products/residentportal/levels?editorPersonaId=12&userPersonaId=123&accessLevel=ALL"
				)
			);
		}

		[Fact]
		public void ListProperties_InvalidEditorPersonaId_ExceptionThrown()
		{
			//Arrange
			long editorPersonaId = 0;
			long userPersonaId = 10;
			RequestParameter requestParameter = new RequestParameter();
			ProductResidentPortalController productResidentPortalController = new ProductResidentPortalController();

			//Act
			Exception exception = Record.Exception(() => productResidentPortalController.ListProperties(editorPersonaId, userPersonaId, requestParameter));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void ListProperties_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("ListProperties" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/products/residentportal/properties?editorPersonaId=12&userPersonaId=123&assignedOnly=false"
				)
			);
		}

        [Fact]
        public void ListResidentPortalMigrationUsers_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpClient Config = new HttpClient();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("ListResidentPortalMigrationUsers" == baseTest.VerifyRouteToAction(
                HttpMethod.Get,
                "http://localhost/api/products/residentportal/migration-users?editorPersonaId=0"
                )
            );
        }

        [Fact]
        public void UpdateUsersMigrationStatus_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpClient Config = new HttpClient();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("UpdateUsersMigrationStatus" == baseTest.VerifyRouteToAction(
                HttpMethod.Put,
                "http://localhost/api/products/residentportal/migrate-users"
                )
            );
        }

        [Fact]
        public void DeactivateResidentPortalUser_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpClient Config = new HttpClient();
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            var expected = "UpdateResidentPortalUserStatus";
            var url = "http://localhost/api/products/residentportal/user/MT/status";
            var method = HttpMethod.Put;

            //Act
            var actual = baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
