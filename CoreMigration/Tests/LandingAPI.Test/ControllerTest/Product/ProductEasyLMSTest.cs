using UnifiedLogin.LandingAPI.Test.Logic;
using UnifiedLogin.LandingAPI;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace UnifiedLogin.LandingAPI.Test.ControllerTest.Product
{
	/// <summary>
	/// Summary description for ProductEasyLMSTest
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ProductEasyLMSTest
	{
		#region Controller Unit Tests
		[Fact]
		public void ProductEasyLMSlUrl_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("ProductEasyLMSlUrl" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/products/easylms"
				)
			);
		}
		#endregion
	}
}
