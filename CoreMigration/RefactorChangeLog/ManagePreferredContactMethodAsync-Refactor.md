# ManagePreferredContactMethodAsync Refactor Changelog

**Date:** 2026-04-21
**Branch:** `feature/UserRepositoryRefactor`
**Target:** .NET 10 / C# 13

---

## Summary

Async refactor of `Logic/ManagePreferredContactMethod.cs` into `LogicAsync/`.
Original class retained as-is for legacy consumers. Two new files created.

---

## Files Created

| File | Notes |
|------|-------|
| `LogicAsync/Interfaces/IManagePreferredContactMethodAsync.cs` | New async interface |
| `LogicAsync/ManagePreferredContactMethodAsync.cs` | Full async implementation |
| `ManagePreferredContactMethodAsync-Refactor.md` | This file |

---

## Key Changes

### 1. Parameterless constructor / `new PreferredContactMethodRepository()` removed

**Before:**
```csharp
public ManagePreferredContactMethod()
{
    _preferredContactMethodRepository = new PreferredContactMethodRepository();
}
```
Violated DI principles — hidden dependency, untestable, tightly coupled to a concrete type.

**After:** single constructor that accepts `IPreferredContactMethodRepositoryAsync` via DI. No parameterless overload.

---

### 2. `new XxxRepository()` inline instantiation replaced with DI

**Before:** `IPreferredContactMethodRepository` injected or newed up directly.

**After:** `IPreferredContactMethodRepositoryAsync` DI-injected — testable, mockable, no hidden I/O.

---

### 3. Sync `ListPreferredContactMethod()` → async `ListPreferredContactMethodAsync(CancellationToken)`

**Before:**
```csharp
public IList<PreferredContactMethod> ListPreferredContactMethod()
    => _preferredContactMethodRepository.ListPreferredContactMethod();
```
Blocking call — occupies a thread for the duration of the DB round-trip.

**After:**
```csharp
public Task<IList<PreferredContactMethod>> ListPreferredContactMethodAsync(
    CancellationToken cancellationToken = default)
    => _repository.ListPreferredContactMethodAsync(cancellationToken);
```
Returns the `Task` directly without `async/await` — avoids allocating a compiler-generated state machine for a pure pass-through method (zero awaits).

---

### 4. `sealed` class

Prevents unintended inheritance on a class with no virtual members — signals final design intent and allows JIT devirtualisation.

---

### 5. File-scoped namespace

```csharp
// Before
namespace UnifiedLogin.BusinessLogic.Logic { ... }

// After
namespace UnifiedLogin.BusinessLogic.LogicAsync;
```
Reduces indentation and matches the .NET 10 / C# 10+ convention used across the rest of the `LogicAsync` layer.

---

### 6. Redundant XML doc comments removed

The original class duplicated the repository summary verbatim. The async implementation omits redundant `<summary>` blocks — the interface contract is the authoritative description.

---

### 7. Guard clause on constructor injection

**Before:** no null check — silent `NullReferenceException` at call time.

**After:**
```csharp
_repository = repository ?? throw new ArgumentNullException(nameof(repository));
```

---

## C# 13 / .NET 10 Features Used

| Feature | Usage |
|---------|-------|
| File-scoped namespace | Both new files |
| `sealed` class | `ManagePreferredContactMethodAsync` |
| Expression-bodied method | `ListPreferredContactMethodAsync` — single-expression return |
| Implicit `using` / global usings | `Task`, `CancellationToken` — no explicit `using System.Threading.Tasks` |
| Nullable-aware project | `ArgumentNullException.ThrowIfNull` pattern consistent with rest of layer |

---

## DI Registration Template

```csharp
services.AddScoped<IManagePreferredContactMethodAsync, ManagePreferredContactMethodAsync>();
// IPreferredContactMethodRepositoryAsync must already be registered by data-access bootstrap.
```
