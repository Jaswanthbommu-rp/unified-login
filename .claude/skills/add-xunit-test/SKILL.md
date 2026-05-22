---
name: add-xunit-test
description: Scaffold an xUnit + Moq test for a Manage{Entity} or Product{Entity} class in unified-login-main's LandingAPI.Test project. Use when adding a new test class or a new [Fact] for an existing test class. Captures the TestBase inheritance, [ExcludeFromCodeCoverage], standard mock fields (IRepository, IProductRepository, IManageBlueBook, HttpMessageHandler, ILdClient, DefaultUserClaim), StoredProcNameConstants-based SP mocking, RPObjectCache.BustCache() reset, and It.Is<object>(predicate) parameter matching.
---

# Scaffold an xUnit test in `LandingAPI.Test`

Tests live in `Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/LandingAPI.Test/`. Stack is **xUnit** + **Moq** + occasional **FluentAssertions**. Logic tests live in `Logic/`; controller tests live in `ControllerTest/`.

## Inputs to confirm

1. **Target** — class under test (e.g., `ManageOrganization`, `ManageProductOps`). This drives the test file name `Manage{Entity}Tests.cs`.
2. **Folder** — `Logic/` for `Manage{Entity}` tests, `Logic/Product/` for `ManageProduct{Name}` tests, `ControllerTest/` for controller tests.
3. **Method under test** — and the scenario you want to cover (happy path, validation error, SP returns empty, LD flag off, etc.).
4. **External dependencies** — does the SUT call an HTTP API (mock `HttpMessageHandler`)? Does it consult LaunchDarkly (mock `ILdClient`)? Does it cache (need `RPObjectCache.BustCache()`)?

## Files to touch

- Create or extend `Service/LandingAPI/LandingAPI.Test/Logic/{Manage{Entity}}Tests.cs` (or `ControllerTest/{Entity}Tests.cs`).
- If `LandingAPI.Test.csproj` does not auto-include `*.cs`, also add the new file to the csproj `<Compile Include>` list.

## Test class template

Mirror [LandingAPI.Test/Logic/ManageOrganizationTest.cs:27](../../Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/LandingAPI.Test/Logic/ManageOrganizationTest.cs) and [LandingAPI.Test/Logic/ManageProductTest.cs:26](../../Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/LandingAPI.Test/Logic/ManageProductTest.cs):

```csharp
using FluentAssertions;
using LaunchDarkly.Sdk.Server.Interfaces;
using Moq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
    [ExcludeFromCodeCoverage]
    public class Manage{Entity}Tests : TestBase
    {
        private readonly Mock<IRepository> _mockRepository = new Mock<IRepository>();
        private readonly Mock<I{Entity}Repository> _mockEntityRepository = new Mock<I{Entity}Repository>();
        private readonly Mock<HttpMessageHandler> _mockMessageHandler = new Mock<HttpMessageHandler>();
        private readonly Mock<ILdClient> _mockLdClient = new Mock<ILdClient>();
        private readonly DefaultUserClaim _userClaim = new DefaultUserClaim
        {
            CorrelationId = Guid.Empty,
            UserRealPageGuid = Guid.NewGuid(),
        };

        public Manage{Entity}Tests()
        {
            // shared setup: seed common SP returns and LD defaults
            _mockLdClient
                .Setup(m => m.BoolVariation(It.IsAny<string>(), It.IsAny<LaunchDarkly.Sdk.User>(), false))
                .Returns(false);
        }

        [Fact]
        public void {Method}_{Scenario}_{Expected}()
        {
            // Arrange
            new RPObjectCache().BustCache();

            _mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(
                    StoredProcNameConstants.SP_{Name},
                    It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = Guid.Empty });

            var sut = new Manage{Entity}(/* test ctor mocks */);

            // Act
            var result = sut.{Method}(/* args */);

            // Assert
            result.Should().NotBeNull();
            Assert.Equal(1, result.Id);
        }
    }
}
```

### Conventions to follow

- **Class-level:** `[ExcludeFromCodeCoverage]` and inherit `TestBase` (it seeds common things like config + base mocks).
- **Naming:** test method `{MethodUnderTest}_{Scenario}_{ExpectedOutcome}` — e.g., `CreatePasswordPolicy_InvalidInput_ExceptionThrown`.
- **Cache:** every test that exercises a cached SP path must call `new RPObjectCache().BustCache();` before Act, or stale entries from a prior test leak in.
- **SP parameters:** prefer `It.IsAny<object>()` for the simple case; switch to `It.Is<object>(o => ...)` (with reflection helper, see below) when the test asserts on the *parameter shape*.
- **LD flag mocks:** when the SUT reads a flag, set the default in the constructor and override per-test as needed.
- **HTTP:** mock `HttpMessageHandler.SendAsync` via `Mock<HttpMessageHandler>` (`Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ...)`); never call the real network.

### Reflection helper for parameter matching

```csharp
private static T GetProp<T>(object o, string name)
    => (T)o.GetType().GetProperty(name).GetValue(o);

// usage:
_mockRepository.Setup(m => m.GetOne<RepositoryResponse>(
    StoredProcNameConstants.SP_CreateUser,
    It.Is<object>(o => GetProp<string>(o, "LoginId") == "expected@example.com"
                    && GetProp<int>(o, "PersonaId") == 5)))
    .Returns(new RepositoryResponse { Id = 1 });
```

### Controller tests (under `ControllerTest/`)

When the controller news up its logic internally, the easiest path is mocking the underlying `IRepository` + `RPObjectCache.BustCache()` (the logic class then runs against the mock). When the controller takes interfaces via a test constructor, instantiate it via that ctor directly — see [ControllerTest/OrganizationTests.cs:2174](../../Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/LandingAPI.Test/ControllerTest/OrganizationTests.cs) for the `Mock<ILdClient>` setup pattern.

## Running

```powershell
# Single test class:
dotnet test Enterprise\Subsystem\ProductLauncher\Service\LandingAPI\LandingAPI.Test\LandingAPI.Test.csproj `
  --filter "FullyQualifiedName~Manage{Entity}Tests"

# Single test method:
dotnet test Enterprise\Subsystem\ProductLauncher\Service\LandingAPI\LandingAPI.Test\LandingAPI.Test.csproj `
  --filter "FullyQualifiedName~Manage{Entity}Tests.{Method}_{Scenario}_{Expected}"
```

Or use VS Test Explorer with `MasterProjectSolution.sln` open.

## Important

- **Never** hit a real database, real Books API, real SendGrid, or real Kafka in a test. Mock the boundary.
- **Never** instantiate `Manage{Entity}` via its production constructor in a test — use the unit-test constructor that takes mocks. If a class lacks one, add it first (the rest of the file already does this; mirror that style).
- **Never** rely on test order. Reset state with `BustCache()` in Arrange — do not assume previous tests cleaned up.
- **Do not** introduce a new test framework. xUnit + Moq is the standard; FluentAssertions is fine where existing tests use it.
- **Do not** create a `TestSetupFixture` / collection fixture — `TestBase` already covers shared setup.
