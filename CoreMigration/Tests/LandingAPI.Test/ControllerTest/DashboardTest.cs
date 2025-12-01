using UnifiedLogin.LandingAPI.Test.Logic;
using UnifiedLogin.LandingAPI;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace UnifiedLogin.LandingAPI.Test.ControllerTest
{
	/// <summary>
	/// Dashboard xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class DashboardTest
    {
        [Fact]
        public void GetDashboardContent_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpClient Config = new HttpClient();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("GetDashboardContent" == baseTest.VerifyRouteToAction(
                HttpMethod.Get,
                "http://localhost/api/dashboard"
                )
            );
        }
    }
}
