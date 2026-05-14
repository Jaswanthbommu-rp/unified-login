---
name: add-launchdarkly-flag
description: Wire a new LaunchDarkly boolean flag end-to-end in unified-login-main — flag-key constant, ILdClient call, mocked test default. Use when adding a new feature flag for runtime gating. Captures the two consumption shapes (static FeatureFlag helper for the Landing API surface, FeatureFlagService for WinService projects with 5-min MemoryCache), the LaunchDarkly.Sdk.User key choice, and the Mock<ILdClient>.BoolVariation(...) test pattern.
---

# Add a LaunchDarkly boolean flag

This repo evaluates LaunchDarkly flags via `LaunchDarkly.Sdk.Server.Interfaces.ILdClient.BoolVariation(string flagKey, LaunchDarkly.Sdk.User user, bool defaultValue)`. Two consumption shapes exist; pick the right one for the project you are editing.

## Inputs to confirm

1. **LD flag key** — the literal LaunchDarkly key (kebab-case, e.g., `"user-company-association"`, `"use-core-api-v2-for-service"`). Confirm the key is already created in the LaunchDarkly project; this skill does **not** create the flag in LaunchDarkly.
2. **Default value** — always `false` for "off until rolled out". The default is the safety net when LD is unreachable.
3. **Consumer project** — Landing API surface (`Component/Landing/`) vs WinService (`WinService/BatchProcessor/` or `WinService/UserNotification/`). Each has a different evaluation wrapper.
4. **User context** — almost always an *application* user (`LaunchDarkly.Sdk.User.WithKey("app")` or `WithKey("<flag-key>")`). Per-user targeting is rare in this codebase; only use it if the flag must vary per `DefaultUserClaim.UserRealPageGuid`.

## Pick the shape

### Shape A — Landing API (`Component/Landing/ThirdParty/FeatureFlag.cs`)

A static helper with one method per flag. Lives at [Component/Landing/ThirdParty/FeatureFlag.cs](../../Enterprise/Subsystem/ProductLauncher/Component/Landing/ThirdParty/FeatureFlag.cs). Add a new static method:

```csharp
public static bool GetMyNewFeatureFlag()
{
    var flagValue = false;
    try
    {
        if (_ldClient == null)
        {
            var config = Configuration.Default(ConfigReader.GetLaunchdarklySdkKey);
            _ldClient = new LdClient(config);
        }

        flagValue = _ldClient.BoolVariation(
            "my-new-feature-flag",
            LaunchDarkly.Sdk.User.WithKey("app"),
            false);
    }
    catch (Exception)
    {
        // bypass in unit tests; default false stands
    }

    return flagValue;
}
```

Call sites use `FeatureFlag.GetMyNewFeatureFlag()`. The static `_ldClient` is settable for tests (`FeatureFlag.LdClient = mockLdClient.Object;`).

### Shape B — WinService (`WinService/{BatchProcessor|UserNotification}/Helper/FeatureFlagService.cs`)

A per-service `FeatureFlagService` class with **5-minute MemoryCache** to reduce LD traffic. Each WinService project owns its own copy. See [WinService/BatchProcessor/Helper/FeatureFlagService.cs](../../Enterprise/Subsystem/ProductLauncher/WinService/BatchProcessor/Helper/FeatureFlagService.cs).

**Do not add a new method per flag here.** The class is already generic via `GetBoolFlag(string flagKey, bool defaultValue = false)`. To add a new flag, just call it with the new key:

```csharp
// In whichever batch worker class consumes the flag:
private readonly FeatureFlagService _featureFlags;

public MyBatchWorker(FeatureFlagService featureFlags)
{
    _featureFlags = featureFlags;
}

public void Process()
{
    if (_featureFlags.GetBoolFlag("my-new-feature-flag", defaultValue: false))
    {
        // new code path
    }
}
```

If you do need a typed accessor (for IntelliSense and refactor safety), add a thin wrapper method on `FeatureFlagService` itself — do **not** create a parallel cache. Use the existing `GetBoolFlag` internally.

### Flag-key constants

There is **no central `FeatureFlagConstants` class** in this repo today — flag keys appear as string literals at the call site (see `FeatureFlag.cs:30` and the research note in `thoughts/shared/research/2026-05-11-unified-login-architecture.md:212`). If you add more than 1–2 flags in one project, introduce a local `internal static class FeatureFlagKeys` next to the consumer with `public const string` entries. Do not invent a global constants file — it would cross project boundaries that don't otherwise share code.

## Test pattern

LandingAPI tests mock `ILdClient` directly. Mirror [LandingAPI.Test/ControllerTest/OrganizationTests.cs:2175](../../Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/LandingAPI.Test/ControllerTest/OrganizationTests.cs):

```csharp
using LaunchDarkly.Sdk.Server.Interfaces;
using Moq;

Mock<ILdClient> mockLdClient = new Mock<ILdClient>();
mockLdClient
    .Setup(m => m.BoolVariation(It.IsAny<string>(), It.IsAny<LaunchDarkly.Sdk.User>(), false))
    .Returns(true);   // simulate flag ON

// If the SUT is the static FeatureFlag helper:
FeatureFlag.LdClient = mockLdClient.Object;

// Then exercise the code path that reads the flag.
```

For per-flag-key targeting, narrow the predicate:

```csharp
mockLdClient
    .Setup(m => m.BoolVariation("my-new-feature-flag", It.IsAny<LaunchDarkly.Sdk.User>(), false))
    .Returns(true);
```

For WinService `FeatureFlagService`, the easiest test path is to construct it with a stubbed `LdClient` or wrap the call behind an interface in your consumer. The `MemoryCache` inside `FeatureFlagService` will cache the first result for 5 minutes — tests that flip the flag mid-run must construct a fresh `FeatureFlagService` per case.

## After adding — checklist

1. Confirm the flag key exists in LaunchDarkly (Dev environment) and is set to your expected default — otherwise the first hit will return the SDK default and your test setup won't match staging.
2. Build the affected solution (`MasterProjectSolution.sln` for Landing API, `UnityBatchProcessor.sln` / `UserNotification.sln` for WinServices).
3. Add a single test for each branch of the flag (on / off).
4. When the flag becomes permanently on or off, **delete** the call site and the LaunchDarkly entry — long-lived dead flags pile up here (see existing flag `user-company-association` in `FeatureFlag.cs` for a comparison point).

## Important

- **Default to `false`.** If LD is unreachable, off should be the safe path. Avoid `defaultValue: true` unless the off-path would brick the system.
- **Do not swallow exceptions silently.** The existing `FeatureFlag.GetUserCompanyAssociationFeatureFlag` catches and discards — that's tolerable for a non-critical flag, but for anything load-bearing, log via Serilog with `CorrelationId`, `ProductModule`, `AdditionalInfo`.
- **Do not create a new `LdClient` per call.** Reuse the static `_ldClient` (Shape A) or the per-service singleton (Shape B). Constructing one per call leaks connections.
- **Do not put per-user targeting** (`LaunchDarkly.Sdk.User.WithKey(userClaim.UserRealPageGuid.ToString())`) into a hot path without confirming the flag was created for user targeting in LaunchDarkly — otherwise LD evaluates as anonymous anyway.
- **Do not rely on LaunchDarkly for auth or PII gating.** Flags here are operational toggles, not security controls.
