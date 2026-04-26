# ManageCustomFieldsAsync Refactor Changelog

**Date:** 2026-04-20
**Branch:** `feature/UserRepositoryRefactor`
**Target:** .NET 10 / C# 13

---

## Summary

Full async refactor of `UnifiedLogin.BusinessLogic/Logic/ManageCustomFields.cs` into
`LogicAsync/`, with updates to the repository interface and implementation to adopt
the `IDbConnectionFactory` pattern and resolve several code-quality issues in the
existing async stub.

---

## Files Created / Updated

| File | Action | Notes |
|------|--------|-------|
| `LogicAsync/Interfaces/IManageCustomFieldsAsync.cs` | **Created** | New async interface; `globals` bag removed |
| `LogicAsync/ManageCustomFieldsAsync.cs` | **Created** | Async implementation |
| `Repository/CustomFieldsRepositoryAsync.cs` | **Updated** | `IDbConnection` → `IDbConnectionFactory`; five additional fixes (see below) |
| `Repository/Interfaces/ICustomFieldsRepositoryAsync.cs` | **Updated** | Correct namespace; file-scoped; `RequestParameter?` nullable annotation |

---

## Key Changes

### 1. `IDictionary<object, object> globals` bag removed

The legacy sync methods passed a weakly-typed `globals` dictionary carrying a single
`RequestParameter` value:

```csharp
// Legacy — extract the only useful item from the bag every time
if (globals.ContainsKey(BaseType.RequestParameter))
    dataFilter = globals[BaseType.RequestParameter] as RequestParameter;
```

The async interface drops the bag entirely. Callers pass `RequestParameter?` directly:

```csharp
// Async — typed parameter, no dictionary lookup
Task<IList<CustomField>> GetCustomFieldAsync(
    long partyId, RequestParameter? dataFilter = null, ...)
```

---

### 2. Dead `GetCustomField(booksCustomerMasterId, bookMasterTypeId)` overload removed

This overload existed in the sync class but its repository call had been commented out:

```csharp
// customFieldList = _customFieldsRepository.GetCustomField(booksCustomerMasterId, bookMasterTypeId, dataFilter);
```

The method body did nothing except log and return an empty list. Not ported.

---

### 3. `DefaultUserClaim` → `IUserClaimsAccessor`

`DefaultUserClaim` was only used to read `CorrelationId` for the custom logging helper.
Replaced with `IUserClaimsAccessor` — `CorrelationId` is a first-class property on the
interface:

```csharp
// Before
_userClaim.CorrelationId

// After
_userClaims.CorrelationId
```

---

### 4. `Serilog.Log.Logger` + hand-rolled `WriteToLog` → `ILogger<T>`

The legacy class used the static `Serilog.Log.Logger`, constructing a new
`Dictionary<string, object> logData` on every call and routing through a private
`WriteToLog` helper with six parameters:

```csharp
// Legacy — 6 parameters, dictionary allocation per call, static logger
WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}",
    _userClaim.CorrelationId, logData, null,
    messageProperties: new object[] { "AddUpdateFieldValue", "Begin" });
```

Replaced with injected `ILogger<ManageCustomFieldsAsync>` and structured log calls:

```csharp
// Async — zero allocations, structured, testable
_logger.LogDebug("{CorrelationId} AddUpdateFieldValueAsync begin — createdBy={CreatedBy}",
    _userClaims.CorrelationId, createdBy);
```

---

### 5. `throw new Exception(...)` → typed guard helpers

| Legacy | Async |
|--------|-------|
| `if (createdBy == 0) throw new Exception("Missing CreatedBy UserId.")` | `ArgumentOutOfRangeException.ThrowIfZero(createdBy)` |
| `if (booksCustomerMasterId == 0) throw new Exception("Missing Book Master Id.")` | (dead overload removed) |
| `if (partyId == 0) throw new Exception("Missing Organization PartyId.")` | `ArgumentOutOfRangeException.ThrowIfZero(partyId)` |
| `if (organizationPartyId == 0) throw new Exception("Missing organization partyId.")` | `ArgumentOutOfRangeException.ThrowIfZero(organizationPartyId)` |
| `if (string.IsNullOrWhiteSpace(json)) throw new ArgumentNullException(...)` | `ArgumentException.ThrowIfNullOrWhiteSpace(customFieldsValuesJson)` |

---

### 6. Repository — `IDbConnection` → `IDbConnectionFactory`

The previous async repository held a single `IDbConnection` for the scope lifetime:

```csharp
// Before: one connection open for the whole scope; OpenIfClosed() hack needed
private readonly IDbConnection _db;
private void OpenIfClosed()
{
    if (_db.State != ConnectionState.Open) _db.Open();
}
OpenIfClosed();
using var tx = _db.BeginTransaction();
```

Updated to `IDbConnectionFactory` — short-lived connection per call:

```csharp
// After: connection opened and disposed within each call
await using var connection = _dbFactory.CreateConnection();
await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
await using var tx = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
```

The `OpenIfClosed()` helper is gone entirely.

---

### 7. `tx.Commit()` / `tx.Rollback()` → async variants

The previous transaction management used synchronous `Commit`/`Rollback`, blocking
the thread during I/O:

```csharp
// Before: sync — blocks thread during commit/rollback
tx.Commit();
// ...
tx.Rollback();
```

Updated to the async versions, consistent with the async-all-the-way principle:

```csharp
// After: async commit/rollback — no thread blocking
await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
// ...
await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
```

---

### 8. `ConfigureAwait(false)` on all awaits

All `await` sites in both logic and repository layers now use `ConfigureAwait(false)`
to avoid synchronisation-context captures in ASP.NET Core request handling.
The previous async repository stub was missing `ConfigureAwait(false)` on every call.

---

### 9. Repository interface — namespace and nullability fixes

`ICustomFieldsRepositoryAsync` was incorrectly declared in `UnifiedLogin.BusinessLogic.Repository`
(not `...Repository.Interfaces`). Updated to the correct namespace with a file-scoped
declaration and `RequestParameter?` nullable annotation on the filter parameters.

---

## C# 13 / .NET 10 Features Used

| Feature | Usage |
|---------|-------|
| File-scoped namespaces | All new/updated files |
| `sealed` classes | `ManageCustomFieldsAsync`, `CustomFieldsRepositoryAsync` |
| `ArgumentOutOfRangeException.ThrowIfZero` (.NET 8+) | Guard on `long` ID params |
| `ArgumentException.ThrowIfNullOrWhiteSpace` (.NET 7+) | Guard on JSON string param |
| `await using` (C# 8) | Per-call `SqlConnection` and `DbTransaction` disposal |
| `ConfigureAwait(false)` | All `await` sites |
| `CommitAsync` / `RollbackAsync` (.NET 6+) | Async transaction management |
| Range operator `[..N]` (C# 8) | `f.Value[..Math.Min(128, f.Value.Length)]` in filter/sort builders |
| Collection expression `[new Setting {...}]` | Single-element list return |
| Nullable reference types (`RequestParameter?`) | All filter parameters |

---

## DI Registration Template

```csharp
services.AddScoped<ICustomFieldsRepositoryAsync, CustomFieldsRepositoryAsync>();
services.AddScoped<IManageCustomFieldsAsync, ManageCustomFieldsAsync>();
```

`IDbConnectionFactory` and `IUserClaimsAccessor` are already registered by the
shared infrastructure (existing registrations — no change needed).
