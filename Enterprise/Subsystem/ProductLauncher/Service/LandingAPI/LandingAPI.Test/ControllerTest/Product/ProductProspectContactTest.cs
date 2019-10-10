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
    public class ProductProspectContactTest
    {
        private readonly RouteTestBase _baseTest;
        private const string _baseUrl = "http://localhost/api/products/prospectcontactcenter";

        #region Constructor
        public ProductProspectContactTest()
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
        public void ListProspectContactMigrationUsers_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "ListProspectContactMigrationUsers";
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
        #endregion
    }
}
