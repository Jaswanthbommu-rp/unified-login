# ManageContactMechanismUsageTypeAsync Refactor Changelog

**Date:** 2026-04-20
**Branch:** `feature/UserRepositoryRefactor`
**Target:** .NET 10 / C# 13

---

## Summary

Full async refactor of `UnifiedLogin.BusinessLogic/Logic/ManageContactMechanismUsageType.cs`
into `LogicAsync/`, with updates to the repository interface and implementation to adopt
the `IDbConnectionFactory` pattern and fix several code-quality issues in the existing
async stub.

---

## Files Created / Updated

| File | Action | Notes |
|------|--------|-------|
| `LogicAsync/Interfaces/IManageContactMechanismUsageTypeAsync.cs` | **Created** | New async interface |
| `LogicAsync/ManageContactMechanismUsageTypeAsync.cs` | **Created** | Async implementation |
| `Repository/ContactMechanismUsageTypeRepositoryAsync.cs` | **Updated** | `IDbConnection` → `IDbConnectionFactory`; four additional fixes (see below) |
| `Repository/Interfaces/IContactMechanismUsageTypeRepositoryAsync.cs` | **Updated** | File-scoped namespace; param renamed; `string?` nullability annotation |

---

## Key Changes

### 1. Parameterless constructor removed

The legacy logic class had two constructors:

```csharp
// Bare constructor — instantiates a live DB repository inline
public ManageContactMechanismUsageType()
{
    _contactMechanismUsageTypeRepository = new ContactMechanismUsageTypeRepository();
}
```

Replaced with a single DI constructor:

```csharp
public ManageContactMechanismUsageTypeAsync(
    IContactMechanismUsageTypeRepositoryAsync repository,
    ILogger<ManageContactMechanismUsageTypeAsync> logger)
```

The parameterless constructor is gone entirely; `new ContactMechanismUsageTypeRepository()`
is never called in the new layer.

---

### 2. Silent exception swallowing removed (legacy repository bug)

The legacy `ContactMechanismUsageTypeRepository` contained a bare `catch` that returned
`null` on any failure, making debugging impossible:

```csharp
// Legacy — exceptions silently discarded, null propagated to callers
catch {
    return null;
}
```

The async repository retains a `try/catch` but logs the error and returns an empty list,
giving callers a safe, non-null result while preserving observability:

```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "{Method} failed for UsageTypeName={Name}",
        nameof(ListContactMechanismUsageTypeAsync), contactMechanismUsageTypeName);
    return [];
}
```

The return contract on both the interface and implementation is now explicit:
_never null, empty list on error_.

---

### 3. Repository — `IDbConnection` → `IDbConnectionFactory`

The previous async repository stub injected a single `IDbConnection` held for the
scope lifetime:

```csharp
// Before: one connection open for the whole scope
private readonly IDbConnection _db;
// ...
var result = await _db.QueryAsync<ContactMechanismUsageType>(...);
```

Updated to the `IDbConnectionFactory` pattern — short-lived connection per call:

```csharp
// After: connection opened and disposed within each call
await using var connection = _dbFactory.CreateConnection();
await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
var result = await connection.QueryAsync<ContactMechanismUsageType>(...).ConfigureAwait(false);
```

---

### 4. Missing `ConfigureAwait(false)` added

The previous async repository was missing `ConfigureAwait(false)` on the
`QueryAsync` call, causing unnecessary synchronisation-context captures in
ASP.NET Core request handling. All `await` sites now use `ConfigureAwait(false)`.

---

### 5. `dynamic` removed from repository parameter

Legacy sync repository used `dynamic` for the Dapper parameter object:

```csharp
// Legacy: dynamic is unnecessary and disables compile-time checking
dynamic param = new { ContactMechanismUsageTypeName };
```

Replaced with a typed anonymous object (Dapper accepts both; the anonymous type
is preferred — compile-time safe and avoids DLR overhead):

```csharp
new { ContactMechanismUsageTypeName = contactMechanismUsageTypeName }
```

---

### 6. PascalCase parameter name corrected

The legacy interface and implementations used `ContactMechanismUsageTypeName`
(PascalCase) as a method parameter name — violating C# naming conventions
(parameters must be camelCase per [CA1707](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1707)).

All signatures updated to `contactMechanismUsageTypeName`.

---

### 7. Nullable annotation — `string?` parameter

The filter parameter is optional (null/empty returns all types). Annotated as
`string?` in both the interface and implementation to express this intent clearly
and enable nullable reference type analysis.

---

## C# 13 / .NET 10 Features Used

| Feature | Usage |
|---------|-------|
| File-scoped namespaces | All new/updated files |
| `sealed` classes | `ManageContactMechanismUsageTypeAsync`, `ContactMechanismUsageTypeRepositoryAsync` |
| `await using` (C# 8) | Per-call `SqlConnection` disposal in repository |
| `ConfigureAwait(false)` | All `await` sites |
| Collection expression `[]` | Empty-list return in `catch` branch |
| Nullable reference types (`string?`) | Filter parameter in interface and implementation |

---

## DI Registration Template

```csharp
services.AddScoped<IContactMechanismUsageTypeRepositoryAsync, ContactMechanismUsageTypeRepositoryAsync>();
services.AddScoped<IManageContactMechanismUsageTypeAsync, ManageContactMechanismUsageTypeAsync>();
```

`IDbConnectionFactory` is already registered by the shared infrastructure
(existing registration — no change needed).
