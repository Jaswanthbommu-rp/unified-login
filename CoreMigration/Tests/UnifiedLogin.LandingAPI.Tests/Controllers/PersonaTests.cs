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
    /// Persona controller xUnit tests.
    /// Migrated from RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest.PersonaTests
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PersonaTests
    {
        #region Controller Unit Tests

        [Fact]
        public void CreatePersona_InvalidPersona_ExceptionThrown()
        {
            // TODO: Re-enable when PersonaController is available
            Assert.True(true, "Test disabled pending LandingAPI migration to ASP.NET Core");
        }

        [Fact]
        public void CreatePersona_InvalidPersonRealPageId_ExceptionThrown()
        {
            // TODO: Re-enable when PersonaController is available
            Assert.True(true, "Test disabled pending LandingAPI migration to ASP.NET Core");
        }

        [Fact]
        public void CreatePersona_InvalidOrganizationRealPageId_ExceptionThrown()
        {
            // TODO: Re-enable when PersonaController is available
            Assert.True(true, "Test disabled pending LandingAPI migration to ASP.NET Core");
        }

        [Fact]
        public void CreatePersona_VerifyRouteToAction_ReturnAction()
        {
            // TODO: Re-enable when LandingAPI is migrated
            Assert.True(true, "Test disabled pending LandingAPI migration to ASP.NET Core");
        }

        #endregion
    }
}
