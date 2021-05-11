using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Controllers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest
{
	/// <summary>
	/// User xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class HotsUserCloneControllerTest
	{
			
		
		public void CloneUser_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpConfiguration Config = new HttpConfiguration();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			Assert.True("HOTCloneUsers" == baseTest.VerifyRouteToAction(
				HttpMethod.Post,
				"http://localhost/apienterprise/userclone"
				)
			);
		}
		
	}
}
