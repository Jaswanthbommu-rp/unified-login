# ManageCommunicationEventsAsync Refactor Changelog

**Date:** 2026-04-20
**Branch:** `feature/UserRepositoryRefactor`
**Target:** .NET 10 / C# 13

---

## Summary

Full async refactor of `UnifiedLogin.BusinessLogic/Logic/ManageCommunicationEvents.cs`
into `LogicAsync/`, with a parallel update to the existing
`Repository/CommunicationEventRepositoryAsync.cs` to adopt the `IDbConnectionFactory`
pattern used across the rest of the async infrastructure.

---

## Files Created / Updated

| File | Action | Notes |
|------|--------|-------|
| `LogicAsync/Interfaces/IManageCommunicationEventsAsync.cs` | **Created** | New async interface |
| `LogicAsync/ManageCommunicationEventsAsync.cs` | **Created** | Async implementation |
| `Repository/CommunicationEventRepositoryAsync.cs` | **Updated** | `IDbConnection` → `IDbConnectionFactory` |

---

## Key Changes

### 1. Constructor overloads removed

The legacy class had three constructors:

```csharp
// Unit-test constructor
public ManageCommunicationEvents(IRepository repository)
{
    _communicationEventRepository = new CommunicationEventRepository(repository);
}

// Bare parameterless constructor — creates a live repository inline
public ManageCommunicationEvents()
{
    _communicationEventRepository = new CommunicationEventRepository();
}
```

Replaced with a single DI constructor:

```csharp
public ManageCommunicationEventsAsync(
    ICommunicationEventRepositoryAsync repository,
    ILogger<ManageCommunicationEventsAsync> logger)
```

The test constructor is no longer needed — inject a mock `ICommunicationEventRepositoryAsync`
directly. The parameterless constructor is gone entirely; `new CommunicationEventRepository()`
is never called.

---

### 2. Guard clauses — semantic correction + .NET 8 throw helpers

The legacy code misused `ArgumentNullException` for integer and long parameters that
cannot be zero. `ArgumentNullException` is semantically incorrect for value types.

| Legacy (wrong) | Async (correct) |
|---|---|
| `if (statusTypeId == 0) throw new ArgumentNullException(nameof(statusTypeId))` | `ArgumentOutOfRangeException.ThrowIfZero(statusTypeId)` |
| `if (fromPartyContactMechanismId == 0) throw new ArgumentNullException(...)` | `ArgumentOutOfRangeException.ThrowIfZero(fromPartyContactMechanismId)` |
| `if (string.IsNullOrEmpty(cesId)) throw new ArgumentNullException(nameof(cesId))` | `ArgumentException.ThrowIfNullOrWhiteSpace(cesId)` |

`ArgumentOutOfRangeException.ThrowIfZero` (.NET 8+) and
`ArgumentException.ThrowIfNullOrWhiteSpace` (.NET 7+) are one-line throw helpers that
avoid the `if (...) throw` boilerplate and produce the correct exception type.

---

### 3. Bug fix — wrong `nameof` in `CreateCommunicationEventEmail`

Legacy guard:
```csharp
if (communicationEventId == 0)
{
    throw new ArgumentNullException(nameof(communicationEmailTemplateId)); // ← wrong name
}
```

The second guard threw with the first parameter's name, so exception messages would
mislead callers. Fixed: `ArgumentOutOfRangeException.ThrowIfZero(communicationEventId)`
uses the correct parameter name automatically.

---

### 4. Repository — `IDbConnection` → `IDbConnectionFactory`

The previous `CommunicationEventRepositoryAsync` injected a single `IDbConnection`
and held it for the lifetime of the scoped service — keeping a SQL connection open
for the full HTTP request duration.

```csharp
// Before: one connection for the scope lifetime
private readonly IDbConnection _db;
// ...
return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(...);
```

Updated to the `IDbConnectionFactory` pattern:

```csharp
// After: short-lived connection per call, returned to ADO.NET pool immediately
await using var connection = _dbFactory.CreateConnection();
await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
return await connection.QuerySingleOrDefaultAsync<RepositoryResponse>(...).ConfigureAwait(false);
```

Each stored-procedure call opens a fresh connection from the pool and disposes it
via `await using` when the call completes — same pattern as `EntUserRepositoryAsync`.

---

### 5. `ConfigureAwait(false)` on all awaits

The previous repository implementation was missing `ConfigureAwait(false)` on
`QuerySingleOrDefaultAsync` calls. All `await` sites in both the logic and repository
layers now use `ConfigureAwait(false)` to avoid unnecessary context-switch overhead
in ASP.NET Core.

---

### 6. `sealed` class on both implementation types

Both `ManageCommunicationEventsAsync` and `CommunicationEventRepositoryAsync` are
declared `sealed` — consistent with the async service pattern across the codebase and
a minor JIT optimisation (no virtual dispatch on method calls).

---

## C# 13 / .NET 10 Features Used

| Feature | Usage |
|---------|-------|
| File-scoped namespaces | All new/updated files |
| `ArgumentOutOfRangeException.ThrowIfZero` (.NET 8+) | Guard clauses on int/long IDs |
| `ArgumentException.ThrowIfNullOrWhiteSpace` (.NET 7+) | Guard on `cesId` |
| `await using` (C# 8) | Per-call `SqlConnection` disposal in repository |
| `ConfigureAwait(false)` | All `await` sites |
| `sealed` classes | Both implementation classes |
| Target-typed `(long?)null` literals | Stored-proc output param initialisation |

---

## DI Registration Template

```csharp
services.AddScoped<ICommunicationEventRepositoryAsync, CommunicationEventRepositoryAsync>();
services.AddScoped<IManageCommunicationEventsAsync, ManageCommunicationEventsAsync>();
```

`IDbConnectionFactory` is already registered by the shared infrastructure
(existing registration — no change needed).
