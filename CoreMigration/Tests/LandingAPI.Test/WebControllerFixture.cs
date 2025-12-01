using UnifiedLogin.LandingAPI;
using System;
using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace UnifiedLogin.LandingAPI.Test.ControllerTest
{
    public class WebControllerFixture : IDisposable
    {
        public HttpClient Config {get; private set;}
        public DefaultHttpControllerSelector ControllerSelector {get; private set;}

        public WebControllerFixture()
        {
            Config = new HttpClient();

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
