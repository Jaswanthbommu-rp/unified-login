using UnifiedLogin.LandingAPI.Test.Logic;
using UnifiedLogin.LandingAPI;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace UnifiedLogin.LandingAPI.Test.ControllerTest.Product
{
	[ExcludeFromCodeCoverage]
	public class ProductLearningPortalTest
	{
		#region Controller Unit Tests
		[Fact]
		public void ProductLearningPortalUrl_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

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
