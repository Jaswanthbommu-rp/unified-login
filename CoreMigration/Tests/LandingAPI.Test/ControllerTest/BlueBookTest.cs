using UnifiedLogin.LandingAPI.Test.Logic;
using UnifiedLogin.LandingAPI;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace UnifiedLogin.LandingAPI.Test.ControllerTest
{
    [ExcludeFromCodeCoverage]
	public class BlueBookTest
	{
		[Fact]
		public void GetCustomerProperty_VerifyRouteToAction_ReturnAction()
		{
            //Arrange
            HttpClient Config = new HttpClient();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Xunit.Assert.True("GetCustomerProperty" == baseTest.VerifyRouteToAction(
                HttpMethod.Get,
                "http://localhost/api/CustomerProperty"
                )
            );
        }
	}
}
