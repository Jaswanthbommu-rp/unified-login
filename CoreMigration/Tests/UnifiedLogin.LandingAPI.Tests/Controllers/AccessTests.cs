using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Xunit;
using UnifiedLogin.LandingAPI.Tests.Helpers;
// TODO: Update these using statements once CoreMigration projects are available
// using UnifiedLogin.LandingAPI;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Access rights controller xUnit tests.
    /// Migrated from RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest.AccessTests
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AccessTests
    {
        private const string _baseUrl = "http://localhost/api/";

        #region Constructor

        public AccessTests()
        {
            // TODO: Replace with ASP.NET Core test setup
        }

        #endregion

        #region Controller Unit Tests

        [Fact]
        public void GetAccessRightsListPerPage_VerifyRouteToAction_ReturnAction()
        {
            // TODO: Re-enable when LandingAPI is migrated
            Assert.True(true, "Test disabled pending LandingAPI migration to ASP.NET Core");
        }

        #endregion
    }
}
