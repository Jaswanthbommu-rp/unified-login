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
    public class ProductLead2LeaseTest
    {
        private readonly RouteTestBase _baseTest;
        private const string _baseUrl = "http://localhost/api/products/lead2lease";

        #region Constructor
        public ProductLead2LeaseTest()
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
        public void ListLead2LeaseMigrationUsers_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "ListLead2LeaseMigrationUsers";
            var url = $"{_baseUrl}/migration-users?editorPersonaId=0";
            var method = HttpMethod.Get;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UpdateUsersMigrationStatus_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "UpdateUsersMigrationStatus";
            var url = $"{_baseUrl}/migrate-users";
            var method = HttpMethod.Put;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DisableLead2LeaseUser_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "UpdateLead2LeaseUserStatus";
            var url = $"{_baseUrl}/user/MT/status";
            var method = HttpMethod.Put;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }
        #endregion
    }
}
