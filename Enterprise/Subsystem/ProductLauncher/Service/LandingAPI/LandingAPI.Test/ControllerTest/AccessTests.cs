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
	public class AccessTests
    {
        private readonly RouteTestBase _baseTest;
        private const string _baseUrl = "http://localhost/api/";

        #region Constructor
        public AccessTests()
        {
            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
            config.EnsureInitialized();
            DefaultHttpControllerSelector controllerSelector = new DefaultHttpControllerSelector(config);
            _baseTest = new RouteTestBase(config, controllerSelector);
        }
        #endregion

        #region Controller Unit Tests		

        [Fact]
        public void GetAccessRightsListPerPage_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "GetRights";
            var routeId = "Userslist";
            var url = _baseUrl + $"{routeId}/rights";
            var method = HttpMethod.Get;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }
        #endregion

    }
}