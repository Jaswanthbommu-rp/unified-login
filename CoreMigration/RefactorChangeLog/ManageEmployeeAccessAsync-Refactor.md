# ManageEmployeeAccessAsync Refactor Changelog

**Date:** 2026-04-20
**Branch:** `feature/UserRepositoryRefactor`
**Target:** .NET 10 / C# 13

---

## Summary

Full async refactor of `UnifiedLogin.BusinessLogic/Logic/ManageEmployeeAccess.cs`
into `LogicAsync/`, eliminating `ManageProductBase` inheritance, all inline `new Xxx(_userClaim)`
instantiations, and the `DefaultUserClaim` parameter on `GetOrCreateEmployeePersonaId`.
The previous `ManageEmployeeAccessAsync.cs` was a broken self-referential stub (injected
itself and delegated back) — replaced with a full true-async implementation.

---

## Files Created / Updated

| File | Action | Notes |
|------|--------|-------|
| `LogicAsync/ManageEmployeeAccessAsync.cs` | **Rewritten** | Full async implementation; prior stub removed |
| `LogicAsync/Interfaces/IManageEmployeeAccessAsync.cs` | **Updated** | `DefaultUserClaim userClaim` param removed from `GetOrCreateEmployeePersonaIdAsync` |
| `Repository/Interfaces/IUnifiedLoginRepositoryAsync.cs` | **Updated** | `ListUsersAsync` added (implementation already existed in `UnifiedLoginRepositoryAsync`) |

---

## Key Changes

### 1. Self-referential stub eliminated

The previous implementation was a stepping-stone that injected `IManageEmployeeAccessAsync`
and delegated back to it — guaranteed to either deadlock or stack-overflow at runtime:

```csharp
// Before: circular dependency stub
public sealed class ManageEmployeeAccessAsync : IManageEmployeeAccessAsync
{
    private readonly IManageEmployeeAccessAsync _manageEmployeeAccess;
    // ...delegates all calls back to _manageEmployeeAccess
}
```

Replaced with a proper implementation that directly orchestrates all dependencies.

---

### 2. `ManageProductBase` inheritance removed

The legacy class inherited `ManageProductBase` which pulled in sync base-class fields
(`_productId`, `_editorPersona`, `_userLoginRepository`, etc.) and the mutable
`GetCompanyEditorAndUserDetails` method:

```csharp
// Before: sync context loaded with side-effects
ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
if (result.IsError) return result;
```

Replaced with the immutable `IProductContextServiceAsync`:

```csharp
// After: immutable context, no shared state
var (_, error) = await _productContext
    .GetUserContextAsync(editorPersonaId, editorPersonaId, (int)ProductEnum.SupportTool, ct)
    .ConfigureAwait(false);
if (error is not null) return error;
```

---

### 3. 12+ inline `new Xxx(_userClaim)` instantiations removed

All dependencies were constructed inside the constructor body:

```csharp
// Before — anti-pattern: live objects created inline
_blueBook          = new ManageBlueBook(_userClaim);
_manageOrganization = new ManageOrganization(_userClaim);
_managePersona     = new ManagePersona(_userClaim);
_manageUnifiedLogin = new ManageUnifiedLogin(_userClaim);
// ... 8+ more
_unifiedLoginRepository = new UnifiedLoginRepository(); // also inline inside methods!
```

All replaced with DI-injected async interfaces on the constructor.

---

### 4. `DefaultUserClaim userClaim` parameter removed from `GetOrCreateEmployeePersonaIdAsync`

The legacy sync method passed `DefaultUserClaim userClaim` explicitly because the class
also held `_userClaim` as a field — two copies of the same data with potential inconsistency.
The async version uses `IUserClaimsAccessor` injected via DI for all claim access:

```csharp
// Before: redundant parameter + field
public EmployeePersona GetOrCreateEmployeePersonaId(Guid companyRealPageId, DefaultUserClaim userClaim)

// After: claims from injected accessor, no parameter
public async Task<EmployeePersona> GetOrCreateEmployeePersonaIdAsync(Guid companyRealPageId, CancellationToken ct = default)
```

---

### 5. `new UnifiedLoginRepository()` inline instantiation eliminated

The original `GetCompanies` and `GetUsers` both constructed a fresh repository inline:

```csharp
// Before: new repository instantiated inside the method body
UnifiedLoginRepository umr = new UnifiedLoginRepository();
List<UnifiedLoginCompany> gbAllCompanies = umr.ListCompanies(filter, OrganizationTypeIds);
List<UserDetail> ulUsersByFilter = umr.ListUsers(filter, OrganizationTypeIds);
```

Replaced by the injected `IUnifiedLoginRepositoryAsync`. `ListUsersAsync` was added to the
interface (the implementation already existed in `UnifiedLoginRepositoryAsync.cs`).

---

### 6. Duplicate SP call eliminated (`GetEmployeeProductADGroupMapping`)

The original `CreateEmployeeProductUser` called the same repository method twice:

```csharp
// Before: two identical DB calls
var userProductToADGroups   = _userRepository.GetEmployeeProductADGroupMapping(personaId, productId);
var existingProductAdGroupInfo = _userRepository.GetEmployeeProductADGroupMapping(personaId, productId).FirstOrDefault();
```

Async version calls it once and derives `existingProductAdGroupInfo` from the same result:

```csharp
// After: single call
var userProductToADGroups  = await _userRepository.GetEmployeeProductADGroupMappingAsync(personaId, productId, ct).ConfigureAwait(false);
var existingProductAdGroupInfo = userProductToADGroups.FirstOrDefault();
```

---

### 7. `ManageProductBase.UpdateProductSettingProductStatus` wrapper removed

The legacy code created a fresh `ManageProductBase` object just to call `UpdateProductSettingProductStatus`:

```csharp
// Before: unnecessary base class instantiation
ManageProductBase mpb = new ManageProductBase(productId, _userClaim, _productInternalSettingRepository, _productRepository);
mpb.UpdateProductSettingProductStatus(personaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);
```

Replaced with the direct repository call:

```csharp
// After: direct async repository call
await _productRepository
    .UpdateProductSettingProductStatusAsync(personaId, productId, ProductStatusSettingType, (int)ProductBatchStatusType.Deleted, ct)
    .ConfigureAwait(false);
```

---

### 8. `GetCompanyIds` dead helper removed

The private `GetCompanyIds(List<UnifiedLoginCompany>)` method was already commented out at
its only call site (`//string comIdsRpUp = GetCompanyIds(gbAllCompanies);`) and never called.
Removed entirely from the async port.

---

### 9. `ManageUser.CreateUser` replaced with `IUserCreationService.CreateUserAsync`

`CreatePersonaInCompany` called `_manageUser.CreateUser(newProfile, newProfile.Persona)`.
The async layer uses `IUserCreationService.CreateUserAsync`, which returns
`CreateUserResponse<IErrorData>` with a `PersonaId` property:

```csharp
// After
var createResponse = await _userCreationService
    .CreateUserAsync(newProfile, newProfile.Persona, cancellationToken)
    .ConfigureAwait(false);
return createResponse?.PersonaId ?? 0;
```

---

### 10. `WriteToDiagnosticLog` / `WriteToErrorLog` replaced with `ILogger<T>`

All diagnostic/error logging migrated to structured `ILogger<ManageEmployeeAccessAsync>`:

```csharp
// Before: Serilog static + dictionary allocation
WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanies", $"..." });

// After: structured ILogger, zero allocation
_logger.LogDebug("{CorrelationId} GetCompaniesAsync start EditorPersonaId={EditorPersonaId}",
    _userClaims.CorrelationId, editorPersonaId);
```

---

### 11. `MergeUserCompanies` logic tightened

The original used a nested loop with an unused `CompanyDetails` local variable. Rewritten
with a flat `foreach` + `Where` filter — same semantics, no dead allocations.

---

## C# 13 / .NET 10 Features Used

| Feature | Usage |
|---------|-------|
| File-scoped namespace | `ManageEmployeeAccessAsync.cs` |
| `sealed` class | `ManageEmployeeAccessAsync` |
| `ConfigureAwait(false)` | All `await` sites |
| `ArgumentOutOfRangeException.ThrowIfZero` | `productId`, `personaId`, `editorPersonaId` guards |
| `is not null` / `is null` pattern | Null checks throughout |
| Collection expressions `[]` | Empty list returns, `PropertyList = ["-1"]`, product batch list |
| `??=` null-coalescing assignment | `defaultRole ??= platformAdminRole` |
| Tuple deconstruction | `var (_, error) = await _productContext.GetUserContextAsync(...)` |

---

## DI Registration Template

```csharp
services.AddScoped<IManageEmployeeAccessAsync, ManageEmployeeAccessAsync>();
```

All injected interfaces (`IProductContextServiceAsync`, `IUnifiedLoginRepositoryAsync`,
`IManageBlueBookAsync`, `IManagePersonaAsync`, `IManageOrganizationAsync`,
`IProductRepositoryAsync`, `IUserRepositoryAsync`, `IUserLoginRepositoryAsync`,
`IManageUPFMProductsIntegrationAsync`, `IIntegrationTypeFactoryAsync`,
`IUserCreationService`, `IUserClaimsAccessor`) are already registered by existing
infrastructure — no additional registrations required.
