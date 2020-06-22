using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI;
using System;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest
{
    public class WebControllerFixture : IDisposable
    {
        public HttpConfiguration Config {get; private set;}
        public DefaultHttpControllerSelector ControllerSelector {get; private set;}

        public WebControllerFixture()
        {
            Config = new HttpConfiguration();

			//Act
            WebApiConfig.Register(Config);
			//Config.MapHttpAttributeRoutes();
			Config.EnsureInitialized();
			ControllerSelector = new DefaultHttpControllerSelector(Config);
        }

        public void Dispose()
        {
            // do something
        }
    }

    [CollectionDefinition("WebcontrollerCollection")]
    public class WebControllerTester : ICollectionFixture<WebControllerFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
