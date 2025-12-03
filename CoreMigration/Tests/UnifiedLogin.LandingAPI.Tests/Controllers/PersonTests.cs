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
    /// Person controller xUnit tests.
    /// Migrated from RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest.PersonTests
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PersonTests
    {
        #region Controller Unit Tests

        [Fact]
        public void CreatePerson_InvalidPerson_ExceptionThrown()
        {
            // TODO: Re-enable when PersonController is available
            Assert.True(true, "Test disabled pending LandingAPI migration to ASP.NET Core");
        }

        [Fact]
        public void CreatePerson_VerifyRouteToAction_ReturnAction()
        {
            // TODO: Re-enable when LandingAPI is migrated
            Assert.True(true, "Test disabled pending LandingAPI migration to ASP.NET Core");
        }

        [Fact]
        public void GetPerson_InvalidRealPageId_ExceptionThrown()
        {
            // TODO: Re-enable when PersonController is available
            Assert.True(true, "Test disabled pending LandingAPI migration to ASP.NET Core");
        }

        [Fact]
        public void GetPerson_VerifyRouteToAction_ReturnAction()
        {
            // TODO: Re-enable when LandingAPI is migrated
            Assert.True(true, "Test disabled pending LandingAPI migration to ASP.NET Core");
        }

        [Fact]
        public void UpdatePerson_InvalidRealPageId_ExceptionThrown()
        {
            // TODO: Re-enable when PersonController is available
            Assert.True(true, "Test disabled pending LandingAPI migration to ASP.NET Core");
        }

        #endregion
    }
}
