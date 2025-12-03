using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Xunit;
using UnifiedLogin.LandingAPI.Tests.Helpers;
// TODO: Update these using statements once CoreMigration projects are available
// using UnifiedLogin.SharedObjects.Landing;
// using UnifiedLogin.LandingAPI;
// using UnifiedLogin.LandingAPI.Controllers;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// UserLogin controller xUnit tests.
    /// Migrated from RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest.UserLoginTests
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UserLoginTests
    {
        #region Controller Unit Tests

        [Fact]
        public void CreateUserLogin_VerifyRouteToAction_ReturnAction()
        {
            // TODO: Re-enable when LandingAPI is migrated
            Assert.True(true, "Test disabled pending LandingAPI migration to ASP.NET Core");
        }

        [Fact]
        public void GetUserLogin_VerifyRouteToAction_ReturnAction()
        {
            // TODO: Re-enable when LandingAPI is migrated
            Assert.True(true, "Test disabled pending LandingAPI migration to ASP.NET Core");
        }

        [Fact]
        public void UpdateUserLogin_VerifyRouteToAction_ReturnAction()
        {
            // TODO: Re-enable when LandingAPI is migrated
            Assert.True(true, "Test disabled pending LandingAPI migration to ASP.NET Core");
        }

        [Fact]
        public void IsLoginNameExists_VerifyRouteToAction_ReturnAction()
        {
            // TODO: Re-enable when LandingAPI is migrated
            Assert.True(true, "Test disabled pending LandingAPI migration to ASP.NET Core");
        }

        #endregion
    }
}
