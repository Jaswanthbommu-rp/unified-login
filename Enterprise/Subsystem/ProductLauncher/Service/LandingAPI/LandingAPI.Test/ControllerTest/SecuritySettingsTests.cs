using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest
{
	[ExcludeFromCodeCoverage]
	/// <summary>
	/// SecuritySettings xUnit tests
	/// </summary>
	public class SecuritySettingsTests
	{
		#region Controller Unit Tests
		[Fact]
		public void GetSettings_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpConfiguration Config = new HttpConfiguration();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("GetSettings" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"https://landingapi.local/api/settings/company/2116?category=security&bookMasterTypeId=1"
				)
			);
		}
		#endregion
	}
}
