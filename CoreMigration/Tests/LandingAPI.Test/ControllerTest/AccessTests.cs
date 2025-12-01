using System.Diagnostics.CodeAnalysis;
using UnifiedLogin.LandingAPI.Test.Logic;

namespace UnifiedLogin.LandingAPI.Test.ControllerTest
{
	[ExcludeFromCodeCoverage]
	public class AccessTests
    {
        private readonly RouteTestBase _baseTest;
        private const string _baseUrl = "http://localhost/api/";

        #region Constructor
        public AccessTests()
        {
            HttpClient config = new HttpClient();
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