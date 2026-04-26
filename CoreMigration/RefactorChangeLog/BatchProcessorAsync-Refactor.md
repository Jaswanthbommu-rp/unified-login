# BatchProcessorAsync Refactor Changelog

**Date:** 2026-04-16  
**Branch:** `feature/UserRepositoryRefactor`  
**Target:** .NET 10 / C# 13  

---

## Summary

Full async refactor of `UnifiedLogin.BusinessLogic/Logic/BatchProcessor/` into
`UnifiedLogin.BusinessLogic/LogicAsync/BatchProcessAsync/`.

Removes every instance of:
- `new ManageProductUser(new DefaultUserClaim { CorrelationId = ... })` inside process classes
- `Activator.CreateInstance(Factories[processType])` inside factory classes
- `Dictionary<BatchProcessType, Type>` / `Dictionary<ProductEnum, Type>` reflection-based dispatch

Replaces with true DI-injected async implementations backed by `IManageProductUserAsync`
and a `FrozenDictionary`-based factory for zero-allocation, zero-reflection dispatch.

---

## Files Created — `LogicAsync/BatchProcessAsync/`

### Factory Layer

| File | Replaces | Key change |
|------|----------|------------|
| `Factory/IProcessExecutionAsync.cs` | `Factory/IProcessExecution.cs` | `Task<string> ExecuteProcessAsync(…, CT)` signature |
| `Factory/IProductAsync.cs` | `Factory/IProduct.cs` | `Task<string> UpdateProductUserProfileAsync(…, CT)` signature |
| `Factory/ProcessExecutionFactoryAsync.cs` | `Factory/ProcessExecutionFactory.cs` | `FrozenDictionary<BatchProcessType, IProcessExecutionAsync>` replaces `Dictionary<BatchProcessType, Type>` + `Activator.CreateInstance` |
| `Factory/ProductFactoryAsync.cs` | `Factory/ProductFactory.cs` | `FrozenDictionary<ProductEnum, IProductAsync>` accepts registrations via constructor; includes `TryGetProductLogic` overload |

#### `ProcessExecutionFactoryAsync` — BatchProcessType mappings (unchanged from sync)

| `BatchProcessType` | Handler |
|--------------------|---------|
| `CreateUpdateProductUser` | `CreateUpdateProductUserAsync` |
| `ProfileUpdate` | `UpdateProductUserProfileAsync` |
| `DeactivateProductUser` | `DeactivateProductUserAsync` |
| `UserTypeAdminToRegular` | `ChangeProductUserTypeAsync` |
| `UserTypeRegularToAdmin` | `ChangeProductUserTypeAsync` |
| `UserTypeAdminToExternal` | `ChangeProductUserTypeAsync` |
| `UserTypeExternalToAdmin` | `ChangeProductUserTypeAsync` |
| `EnterpriseRoleCreateUpdateProductUser` | `EnterpriseCreateUpdateProductUserAsync` |
| `PrimaryPropertiesUpdateProductUser` | `CreateUpdateProductUserAsync` |

### Process Layer

| File | Replaces | Notes |
|------|----------|-------|
| `Process/CreateUpdateProductUserAsync.cs` | `Process/CreateUpdateProductUser.cs` | Delegates to `IManageProductUserAsync.CreateProductUserAsync` |
| `Process/UpdateProductUserProfileAsync.cs` | `Process/UpdateProductUserProfile.cs` | Delegates to `IManageProductUserAsync.UpdateProductUserProfileAsync` |
| `Process/DeactivateProductUserAsync.cs` | `Process/DeactivateProductUser.cs` | Preserved `NotImplementedException` — not yet ported |
| `Process/ChangeProductUserTypeAsync.cs` | `Process/ChangeProductUserType.cs` | Delegates to `IManageProductUserAsync.ChangeUserTypeAsync` |
| `Process/EnterpriseCreateUpdateProductUserAsync.cs` | `Process/EnterpriseCreateUpdateProductUser.cs` | Delegates to `IManageProductUserAsync.CreateEnterpriseRoleProductUserAsync` |

### Entry Point

| File | Replaces | Notes |
|------|----------|-------|
| `BatchProcessorLogicAsync.cs` | `BatchProcessorLogic.cs` | Injects `ProcessExecutionFactoryAsync`; delegates to `IProcessExecutionAsync.ExecuteProcessAsync` |

---

## Files Modified

### `LogicAsync/Interfaces/IManageProductUserAsync.cs`

Added 3 new batch-pipeline methods required to fully remove sync `ManageProductUser` from the
batch dispatch path:

```csharp
Task<string> ChangeUserTypeAsync(ProductUserProperitiesRoles batchRecord, CT);
Task<string> CreateEnterpriseRoleProductUserAsync(ProductUserProperitiesRoles batchRecord, CT);
Task<string> UpdateProductUserProfileAsync(ProductUserProperitiesRoles batchRecord, CT);
```

### `LogicAsync/ManageProductUserAsync.cs`

Implemented the 3 new interface members.  
All three retain `// SYNC` annotations on the `IIntegrationTypeFactory` call site pending
`IIntegrationTypeFactoryAsync` being available.  
Batch-status repository calls (`UpdateProductBatchAsync`, `UpdateBatchProcessorLogAsync`,
`GetProductActivityLogAsync`, `WriteActivityLogAsync`, `ClearPersonaErrorAsync`,
`DeleteProductActivityLogAsync`) are fully async.

---

## Key Design Decisions

### 1. `FrozenDictionary` over `Dictionary<BatchProcessType, Type>`

`System.Collections.Frozen.FrozenDictionary<TKey, TValue>` (available since .NET 8, stabilised
in .NET 10) is immutable after construction and uses a specialised lookup algorithm that avoids
the overhead of virtual dispatch and hash-collision probing present in mutable `Dictionary<,>`.
Since the process-type map is fixed at startup, `FrozenDictionary` is the correct .NET 10
primitive.

### 2. DI-injected singletons replace `Activator.CreateInstance`

`Activator.CreateInstance` requires a public parameterless constructor and performs reflection on
every call. The async factory instead accepts concrete process-class instances through its
constructor (all registered as `Scoped` in the DI container). This:
- Eliminates per-call reflection
- Makes the full dependency graph visible to the DI container for health-checking
- Allows each process class to declare its own typed dependencies (e.g., `IManageProductUserAsync`)

### 3. `DefaultUserClaim` removed from process classes

Each sync process class created `new DefaultUserClaim { CorrelationId = Guid.NewGuid() }` and
passed it to `new ManageProductUser(…)`. The async layer removes this pattern entirely:
- `CorrelationId` is guaranteed on the `batchRecord` before the handler is called (guard
  added in each process class)
- Identity context flows through `IUserClaimsAccessor` inside `ManageProductUserAsync`

### 4. `ConfigureAwait(false)` on all await sites

All `await` calls in the new process and logic classes use `.ConfigureAwait(false)` to avoid
capturing the ASP.NET `SynchronizationContext` on the continuation, reducing context-switch
overhead in high-throughput batch scenarios.

### 5. `ArgumentNullException.ThrowIfNull` (C# 13 / .NET 10)

Constructor null-guards use the static `ArgumentNullException.ThrowIfNull(param)` helper
introduced in .NET 6 and now idiomatic in .NET 10. This replaces the verbose
`?? throw new ArgumentNullException(nameof(param))` pattern where the parameter name can be
inferred.

### 6. `ProductFactoryAsync` accepts `IReadOnlyDictionary<ProductEnum, IProductAsync>`

Unlike `ProcessExecutionFactoryAsync` (which takes concrete process-class instances), the product
factory accepts an open registration map. This makes it straightforward to extend with additional
`IProductAsync` implementations without changing the factory constructor — the DI composition
root owns the map.

---

## Remaining SYNC annotations

| Location | Method | Blocked on |
|----------|--------|------------|
| `ManageProductUserAsync.ChangeUserTypeAsync` | `integration.ChangeUserType(batchRecord)` | `IIntegrationTypeFactoryAsync` |
| `ManageProductUserAsync.UpdateProductUserProfileAsync` | `integration.UpdateUserProfile(batchRecord)` | `IIntegrationTypeFactoryAsync` |
| `ManageProductUserAsync.CreateEnterpriseRoleProductUserAsync` | `integration.CreateUser(batchRecord, out _)` | `IIntegrationTypeFactoryAsync` |
| `ManageProductUserAsync.CreateProductUserAsync` (existing) | `integration.CreateUser(productUser, out addParams)` | `IIntegrationTypeFactoryAsync` |
| `Process/DeactivateProductUserAsync` | Entire method body | Deactivation workflow not yet designed |

---

## DI Registration Template

```csharp
// Process classes (Scoped — one per request/batch item)
services.AddScoped<CreateUpdateProductUserAsync>();
services.AddScoped<UpdateProductUserProfileAsync>();
services.AddScoped<DeactivateProductUserAsync>();
services.AddScoped<ChangeProductUserTypeAsync>();
services.AddScoped<EnterpriseCreateUpdateProductUserAsync>();

// Factory (Scoped — depends on Scoped process classes)
services.AddScoped<ProcessExecutionFactoryAsync>();

// Entry point
services.AddScoped<BatchProcessorLogicAsync>();

// Product factory — extend registrations when IProductAsync implementations exist
services.AddScoped<ProductFactoryAsync>(_ =>
    new ProductFactoryAsync(new Dictionary<ProductEnum, IProductAsync>
    {
        // [ProductEnum.LeadAnalytics] = sp.GetRequiredService<LeadManagementProfileUpdaterAsync>()
    }));
```

---

## C# 13 / .NET 10 Features Used

| Feature | Usage |
|---------|-------|
| `System.Collections.Frozen.FrozenDictionary<,>` | `ProcessExecutionFactoryAsync`, `ProductFactoryAsync` — zero-allocation read-only dispatch table |
| `ArgumentNullException.ThrowIfNull` | All constructor guards |
| File-scoped namespaces | All new files |
| `ConfigureAwait(false)` | All `await` sites in process and logic classes |
| Collection expressions `[]` | Empty collection returns in `ManageProductUserAsync` stubs |
| Nullable reference types (`string?`, `IProductAsync?`) | `ProductFactoryAsync.TryGetProductLogic` out parameter |
| Primary constructors (C# 12, idiomatic in .NET 10) | Considered; explicit constructors retained for `ArgumentNullException.ThrowIfNull` guards |
