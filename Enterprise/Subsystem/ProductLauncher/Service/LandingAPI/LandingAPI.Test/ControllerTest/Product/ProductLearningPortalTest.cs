using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest.Product
{
	[ExcludeFromCodeCoverage]
	public class ProductLearningPortalTest
	{
		#region Controller Unit Tests
		[Fact]
		public void ProductLearningPortalUrl_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpConfiguration Config = new HttpConfiguration();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("ProductLearningPortalUrl" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/products/learningportal"
				)
			);
		}
		#endregion
	}
}
