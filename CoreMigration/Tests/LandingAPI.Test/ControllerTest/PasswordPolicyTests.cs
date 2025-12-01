using UnifiedLogin.SharedObjects;
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
	/// PasswordPolicy Controller Tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class PasswordPolicyTests
	{
		#region Controller Unit Tests
		[Fact]
		public void CreatePasswordPolicy_InvalidPasswordPolicy_ExceptionThrown()
		{
			//Arrange
			PasswordPolicy passwordPolicy = new PasswordPolicy();
			PasswordPolicyController passwordPolicyController = new PasswordPolicyController();

			//Act
			Exception exception = Record.Exception(() => passwordPolicyController.CreatePasswordPolicy(passwordPolicy));

			//Assert
			Assert.IsType<Exception>(exception);
		}

		[Fact]
		public void CreatePasswordPolicy_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("CreatePasswordPolicy" == baseTest.VerifyRouteToAction(
				HttpMethod.Post,
				"http://localhost/api/passwordpolicies"
				)
			);
		}

		[Fact]
		public void GetPasswordPolicy_InvalidPortfolioId_ExceptionThrown()
		{
			//Arrange
			int PortfolioId = 0;
			PasswordPolicyController passwordPolicyController = new PasswordPolicyController();

			//Act
			Exception exception = Record.Exception(() => passwordPolicyController.GetPasswordPolicy(PortfolioId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void GetPasswordPolicy_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("GetPasswordPolicy" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/passwordpolicies/1"
				)
			);
		}

		[Fact]
		public void UpdatePasswordPolicy_InvalidPasswordPolicy_ExceptionThrown()
		{
			//Arrange
			PasswordPolicy passwordPolicy = new PasswordPolicy();
			PasswordPolicyController passwordPolicyController = new PasswordPolicyController();

			//Act
			Exception exception = Record.Exception(() => passwordPolicyController.UpdatePasswordPolicy(passwordPolicy));

			//Assert
			Assert.IsType<Exception>(exception);
		}

		[Fact]
		public void UpdatePasswordPolicy_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("UpdatePasswordPolicy" == baseTest.VerifyRouteToAction(
				HttpMethod.Put,
				"http://localhost/api/passwordpolicies"
				)
			);
		}
		#endregion
	}
}
