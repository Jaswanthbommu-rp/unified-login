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
	/// Product Renters Insurance xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ProductRentersInsuranceTests
	{
		#region Controller Unit Tests
		[Fact]
		public void ListProperties_InvalidEditorPersonaId_ExceptionThrown()
		{
			//Arrange
			long editorPersonaId = 0;
			long userPersonaId = 10;
			RequestParameter requestParameter = new RequestParameter();
			ProductRentersInsuranceController productRentersInsuranceController = new ProductRentersInsuranceController();

			//Act
			Exception exception = Record.Exception(() => productRentersInsuranceController.ListProperties(editorPersonaId, userPersonaId, requestParameter));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
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
				"http://localhost/api/products/rentersinsurance/properties?editorPersonaId=12&userPersonaId=123&assignedOnly=false"
				)
			);
		}

		[Fact]
		public void ListRoles_InvalidEditorPersonaId_ExceptionThrown()
		{
			//Arrange
			long editorPersonaId = 0;
			long userPersonaId = 10;
			ProductRentersInsuranceController productRentersInsuranceController = new ProductRentersInsuranceController();

			//Act
			Exception exception = Record.Exception(() => productRentersInsuranceController.ListRoles(editorPersonaId, userPersonaId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void ListRoles_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("ListRoles" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/products/rentersinsurance/roles?editorPersonaId=12&userPersonaId=123"
				)
			);
		}

        [Fact]
        public void ListRentersInsuranceMigrationUsers_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpClient Config = new HttpClient();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("ListRentersInsuranceMigrationUsers" == baseTest.VerifyRouteToAction(
                HttpMethod.Get,
                "http://localhost/api/products/rentersinsurance/migration-users?editorPersonaId=0"
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
                "http://localhost/api/products/rentersinsurance/migrate-users"
                )
            );
        }

        [Fact]
        public void DisableRentersInsuranceUser_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpClient Config = new HttpClient();
            var expected = "UpdateRentersInsuranceUserStatus";
            var url = "http://localhost/api/products/rentersinsurance/user/MT/status";
            var method = HttpMethod.Put;

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);
            var actual = baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }
        #endregion
    }
}
