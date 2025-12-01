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
	/// StatusType xUnit tests
	/// </summary>
	public class StatusTypeTests
	{
		#region Controller Unit Tests
		[Fact]
		public void GetStatusType_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("GetStatusType" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"https://landingapi.local/api/statustype/categorytype/Status/categoryname/User%20Status"
				)
			);
		}
		#endregion
	}
}
