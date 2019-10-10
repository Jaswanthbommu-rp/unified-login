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
	/// StatusType xUnit tests
	/// </summary>
	public class StatusTypeTests
	{
		#region Controller Unit Tests
		[Fact]
		public void GetStatusType_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpConfiguration Config = new HttpConfiguration();

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
