using UnifiedLogin.LandingAPI.Test.Logic;
using UnifiedLogin.LandingAPI;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace UnifiedLogin.LandingAPI.Test.ControllerTest
{
	/// <summary>
	/// Email Controller xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class EmailControllerTests
	{
		#region Controller Unit Tests
		[Fact]
		public void ListContactMechanismUsageType_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("SendEmail" == baseTest.VerifyRouteToAction(
				HttpMethod.Post,
				"http://localhost/api/sendemail"
				)
			);
		}
		#endregion
	}
}
