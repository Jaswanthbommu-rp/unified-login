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
	/// <summary>
	/// UserLogin xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class UserLoginTests
	{
		#region Controller Unit Tests
		[Fact]
		public void CreateUserLogin_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("CreateUserLogin" == baseTest.VerifyRouteToAction(
				HttpMethod.Post,
				"http://localhost/api/userlogins/13e71de5-bafa-469d-9f7a-e12db3961ba9"
				)
			);
		}

		[Fact]
		public void GetUserLogin_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("GetUserLogin" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/userlogins/13e71de5-bafa-469d-9f7a-e12db3961ba9"
				)
			);
		}

		[Fact]
		public void UpdateUserLogin_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("UpdateUserLogin" == baseTest.VerifyRouteToAction(
				HttpMethod.Put,
				"http://localhost/api/userlogins/13E71DE5-BAFA-469D-9F7A-E12DB3961BA9"
				)
			);
		}

		[Fact]
		public void IsLoginNameExists_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("IsLoginNameExists" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/userlogins/loginnameexists?loginName=james@test.com&OrganizationRealPageId=9E9410AE-2C41-47D2-81D1-109C08CD151C"
				)
			);
		}

		[Fact]
		public void IsLoginNameExists_InvalidOrganizationRealPageId_ReturnErrorData()
		{
			//Arrange
			string loginName = "james@test.com";
			Guid organizationRealPageId = new Guid();
            Guid userRealPageId = Guid.Empty;

            UserLoginController controller = new UserLoginController();
			ObjectOutput<UserOrganizationExists, IErrorData> userOrganizationExistsOutput = new ObjectOutput<UserOrganizationExists, IErrorData>();
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpClient();

			//Act
			HttpResponseMessage response = controller.IsLoginNameExists(loginName, organizationRealPageId, userRealPageId);
			userOrganizationExistsOutput = response.Content.ReadAsAsync<ObjectOutput<UserOrganizationExists, IErrorData>>().Result;

			//Assert
			Assert.False(userOrganizationExistsOutput.Status.Success);
			Assert.Equal("UserLogin.IsLoginNameExists.1", userOrganizationExistsOutput.Status.ErrorCode);
			Assert.Equal("IsLoginNameExists: Invalid parameter enterprise organization Id", userOrganizationExistsOutput.Status.ErrorMsg);
		}

		[Fact]
		public void IsLoginNameExists_InvalidLoginName_ReturnErrorData()
		{
			//Arrange
			string loginName = string.Empty;
			Guid organizationRealPageId = new Guid("9E9410AE-2C41-47D2-81D1-109C08CD151C");
            Guid userRealPageId = Guid.Empty;

            UserLoginController controller = new UserLoginController();
			ObjectOutput<UserOrganizationExists, IErrorData> userOrganizationExistsOutput = new ObjectOutput<UserOrganizationExists, IErrorData>();
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpClient();

			//Act
			HttpResponseMessage response = controller.IsLoginNameExists(loginName, organizationRealPageId, userRealPageId);
			userOrganizationExistsOutput = response.Content.ReadAsAsync<ObjectOutput<UserOrganizationExists, IErrorData>>().Result;

			//Assert
			Assert.False(userOrganizationExistsOutput.Status.Success);
			Assert.Equal("UserLogin.IsLoginNameExists.2", userOrganizationExistsOutput.Status.ErrorCode);
			Assert.Equal("IsLoginNameExists: Invalid parameter loginName", userOrganizationExistsOutput.Status.ErrorMsg);
		}
		#endregion
	}
}
