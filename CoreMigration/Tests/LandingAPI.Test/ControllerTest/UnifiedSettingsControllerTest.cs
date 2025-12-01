using UnifiedLogin.LandingAPI.Test.Logic;
using UnifiedLogin.LandingAPI;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace UnifiedLogin.LandingAPI.Test.ControllerTest
{
	[ExcludeFromCodeCoverage]
	/// <summary>
	/// SecuritySettings xUnit tests
	/// </summary>
	public class UnifiedSettingsControllerTest
	{
		#region Controller Unit Tests
		[Fact]
		public void GetSettings_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("GetSettings" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"https://landingapi.local/api/companies/F5C090FA-78AB-452F-B504-98AAFEE09121/settings?category=security&includes=%22%22"
				)
			);
		}
		#endregion
	}
}
