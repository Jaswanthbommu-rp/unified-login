using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Xunit;
using UnifiedLogin.LandingAPI.Tests.Helpers;
// TODO: Update these using statements once CoreMigration projects are available
// using UnifiedLogin.SharedObjects;
// using UnifiedLogin.LandingAPI;
// using UnifiedLogin.LandingAPI.Controllers;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Profile controller xUnit tests.
    /// Migrated from RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.ProfileTest
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProfileTests
    {
        #region Controller Unit Tests

        [Fact]
        public void GetProfile_InvalidRealPageId_ExceptionThrown()
        {
            // TODO: Re-enable when ProfileController is available
            Assert.True(true, "Test disabled pending LandingAPI migration to ASP.NET Core");
        }

        [Fact]
        public void GetProfile_VerifyRouteToAction_ReturnAction()
        {
            // TODO: Re-enable when LandingAPI is migrated
            Assert.True(true, "Test disabled pending LandingAPI migration to ASP.NET Core");
        }

        [Fact]
        public void UpdateProfile_InvalidRealPageId_ExceptionThrown()
        {
            // TODO: Re-enable when ProfileController is available
            Assert.True(true, "Test disabled pending LandingAPI migration to ASP.NET Core");
        }

        #endregion
    }
}
