# ManageProductRealConnectAsync – Refactor Changelog

## Source
`UnifiedLogin.BusinessLogic/Logic/Product/ManageProductRealConnect.cs`

## Output
| File | Action |
|------|--------|
| `LogicAsync/Interfaces/IManageProductRealConnectAsync.cs` | Created – true-async interface |
| `LogicAsync/Product/ManageProductRealConnectAsync.cs`     | Created – native async implementation |

---

## Breaking Changes

### `DefaultUserClaim` removed from all public APIs
All five public methods previously required a `DefaultUserClaim`-seeded constructor.
Per-call context (editor persona, SAML attributes for `ProductLearnerId` / `ProductManagerId`)
is now resolved by injecting `IProductContextServiceAsync.GetUserContextAsync`.

### No inheritance from `ManageProductBase`
The new class is a plain `sealed class` — no base-class fields, no protected helpers,
no `_productInternalSettingList` from the constructor side-effect chain.

---

## Constructor — from 1 to 11 dependencies

| # | Dependency | Replaces |
|---|-----------|---------|
| 1 | `IProductContextServiceAsync` | `DefaultUserClaim` + `GetCompanyEditorAndUserDetails` |
| 2 | `IProductRepositoryAsync` | `_productRepository` (sync) |
| 3 | `ISamlAttributeServiceAsync` | `_samlRepository.CreateSamlUserAttribute` / `UpdateSamlUserAttribute` / `RemoveSamlUserAttributeBySamlAttributeId` |
| 4 | `IManageBlueBookAsync` | `_blueBook.GetCompanyInstanceByUPFMCompanyId` + `GetCustomerCompanyMapByCustomerCompanyId` |
| 5 | `IManagePersonaAsync` | `_managePersona.GetPersona` |
| 6 | `IManagePersonAsync` | `_managePerson.GetPerson` |
| 7 | `IManageUserLoginAsync` | `_manageUserLogin.GetUserLoginOnly` |
| 8 | `IManageElectronicAddressAsync` | `_manageContactMechanism.ListContactMechanismForPerson` (no-email path) |
| 9 | `IMemoryCache` | `RPObjectCache` (1800 s TTL) |
| 10 | `IHttpClientFactory` | `new HttpClient(policyHandler)` per-instance |
| 11 | `ILogger<ManageProductRealConnectAsync>` | `WriteToErrorLog` / `WriteToDiagnosticLog` base helpers |

`ArgumentNullException.ThrowIfNull` guard applied to each dependency.

---

## Field changes

| Old | New |
|-----|-----|
| `static string _apiEndPoint` | Cached per-call via `GetProductConfigAsync` (1-hour `IMemoryCache` entry) |
| `string _clientId` (set in ctor via UDM sync call) | Resolved per-call via `GetClientIdAsync` (30-min cache keyed by `orgRealPageId`) |
| `static List<string> ref1Data` | `static readonly FrozenSet<string> ValidRef1Types` — O(1) `Contains`, zero allocation |
| `RPObjectCache` license cache | `IMemoryCache` entry `"RC_Licenses_{orgPartyId}"` (1800 s) |
| `_manageUnifiedSettings` | Removed — `GetApiKeyForPanoramaFromSettings` was unused in any public method |

---

## Blocking `.Result` — all eliminated

Every `.Result` / `.GetAwaiter().GetResult()` call in the original is replaced with `await`:

| Original | Async replacement |
|----------|-------------------|
| `GetClientLicenseDetailsCaching().Result` | `await GetClientLicenseDetailsAsync(...)` |
| `GetClientLicenseDetailsPaging(cursor).Result` | `await FetchLicensePageAsync(...)` |
| `GetUser(_productLearnerId).Result` | `await GetRcUserAsync(...)` |
| `_client.PutAsJsonAsync(...).Result` | `await client.PutAsync(url, content, ct)` |
| `_client.PostAsJsonAsync(...).Result` | `await client.PostAsync(url, content, ct)` |
| `_client.GetAsync(url).Result` | `await client.GetAsync(url, ct)` |
| `response.Content.ReadAsStringAsync().Result` | `await response.Content.ReadAsStringAsync(ct)` |

---

## HTTP client

### `IHttpClientFactory` with named client `"RealConnect"`
`new HttpClient(policyHandler)` in the constructor is replaced with
`_httpClientFactory.CreateClient("RealConnect")`. The Polly `RateLimitPolicyHandler`
is registered on the named client at DI time:

```csharp
services.AddHttpClient("RealConnect")
        .AddHttpMessageHandler(() =>
            new RcRateLimitPolicyHandler(
                ManageProductRealConnectAsync.GetRateLimitPolicy()));
```

### `PutAsJsonAsync` / `PostAsJsonAsync` ambiguity removed
The RC shared-object DTOs carry Newtonsoft `[JsonProperty]` attributes — STJ-based
extension methods would serialize incorrect property names.  All HTTP payloads are
now serialised with `JsonConvert.SerializeObject` and wrapped in `StringContent`
via the private helper `SerializeToContent<T>`.

### Bearer token
The API key is loaded once from product internal settings (cached 1 hour) and applied
to each `HttpClient` instance via
`client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey)`.

---

## Paging — recursive → iterative

### Original (problematic)
```csharp
private Task<ClientLicenseDetails> GetClientLicenseDetails(string cursor = "")
{
    ClientLicenseDetails result = GetClientLicenseDetailsPaging(cursor).Result; // blocking + recursive
    if (result.PageInfo.HasMore)
    {
        var next = GetClientLicenseDetails(result.PageInfo.Cursor).Result;
        result.Licenses.AddRange(next.Licenses);
    }
    return Task.FromResult(result);
}
```

### New (iterative async)
```csharp
private async Task<ClientLicenseDetails> GetClientLicenseDetailsAsync(...)
{
    var result = await FetchLicensePageAsync("", ...);
    while (result.PageInfo?.HasMore == true && !string.IsNullOrEmpty(result.PageInfo.Cursor))
    {
        var next = await FetchLicensePageAsync(result.PageInfo.Cursor, ...);
        if (next is null) break;
        result.Licenses.AddRange(next.Licenses);
        result.PageInfo = next.PageInfo;
    }
    return result;
}
```

Stack growth proportional to page count is eliminated; each page is processed as a
flat loop iteration.

---

## SAML operations

All SAML writes use `ISamlAttributeServiceAsync`:

| Operation | Old (base class) | New |
|-----------|-----------------|-----|
| Create LearnerId | `_samlRepository.CreateSamlUserAttribute(...)` | `_samlService.UpsertAttributesAsync(...)` |
| Create ManagerId | `_samlRepository.CreateSamlUserAttribute(...)` | `_samlService.UpsertAttributesAsync(...)` |
| Update ProductUsername | `UpdateSamlUserAttribute(...)` (base helper) | `_samlService.UpsertAttributeAsync(...)` |
| Remove ManagerId | `_samlRepository.RemoveSamlUserAttributeBySamlAttributeId(...)` | `_samlService.RemoveAttributeAsync(...)` |
| Remove DualRole | `_samlRepository.RemoveSamlUserAttributeBySamlAttributeId(...)` | `_samlService.RemoveAttributeAsync(...)` |

`UpsertAttributesAsync` batches the LearnerId + ProductUsername create in a single round-trip.

---

## Dual-role refactor

`TagDualRoleToUser` + `AddDualRoleToUser` were two separate `string`-returning sync methods
that shared mutable state (`_productManagerId` assignment in the middle of `TagDualRoleToUser`).

The async version uses:
- `TagDualRoleToUserAsync` returns `(string error, string managerId)` — no side-effects.
- `AddDualRoleToUserAsync` accepts `currentManagerId` as a parameter (immutable path).

`RemoveDualRoleToUser` → `RemoveDualRoleFromUserAsync` — identical logic, fully `await`-ed.

---

## `FormattedEmail` → `FormattedEmailAsync`

- `IsRegularUserNoEmail(personaId)` (sync base-class lookup) →
  `await _contextService.IsRegularUserNoEmailAsync(userPersona, ct)`.
- `_manageContactMechanism.ListContactMechanismForPerson(realPageId, null)` →
  `await _manageElectronicAddress.ListElectronicAddressForPersonAsync(realPageId, "EMAIL", ct)`.
- `email.ToLower()` → `email.ToLowerInvariant()` (culture-safe).

---

## `GetClientIdFromUDM` → `GetClientIdAsync` + `ResolveClientIdFromUdmAsync`

- Result cached in `IMemoryCache` for 30 minutes per company (`RC_ClientId_{orgRealPageId}`).
- All sync `_blueBook.*` calls replaced with `await _manageBlueBook.*Async(...)`.
- `_productRepository.ListProducts(...)` → `await _productRepository.ListProductsAsync(ProductId, ...)`.

---

## `GetApiKeyForPanoramaFromSettings` removed

The panorama API key was fetched in the constructor but never used by any public method.
The dead call is removed entirely.

---

## C# 13.0 / .NET improvements

| Feature | Usage |
|---------|-------|
| `file sealed class RcRateLimitPolicyHandler` | File-scope access modifier — type invisible outside this compilation unit |
| `System.Collections.Frozen.FrozenSet<string>` | `ValidRef1Types` — allocation-free `Contains` for license Ref1 filter |
| `\e` string escape | ANSI colour in retry console output (`\e[33m` … `\e[0m`) |
| C# 12 collection expressions `[]` | `new List<>()` → `[]` throughout |
| `switch` expression | `ManagerLicenseSortId` / `RoleSortId` private helpers |
| `when (ex is not OperationCanceledException)` | All catch guards allow cancellation to propagate |
| `params ReadOnlySpan` compatible site | `ListRolesForProductByPartyAsync([ProductId], ProductId, ct)` — single-element array via collection expression |
| Primary constructor syntax | `RcRateLimitPolicyHandler(IAsyncPolicy<HttpResponseMessage> policy)` |
| Null-conditional + pattern matching | `result.PageInfo?.HasMore == true` instead of guarded property access |

---

## DI Registration (required)

```csharp
// Program.cs / ServiceCollectionExtensions.cs

services.AddHttpClient("RealConnect")
        .AddHttpMessageHandler(() =>
            new RcRateLimitPolicyHandler(
                ManageProductRealConnectAsync.GetRateLimitPolicy()));

services.AddScoped<IManageProductRealConnectAsync, ManageProductRealConnectAsync>();
```

---

## Compile fixes applied

| Error | Cause | Fix |
|-------|-------|-----|
| `CS0246 ProductUserRolePropertiesGroups` | Missing using directive | Added `using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;` |
| `CS0246 Person` / `UserLoginOnly` | Missing using directive | Added `using UnifiedLogin.SharedObjects.IdentityConfig;` |
| `CS0019 ?? on value tuple` | `GetOrCreateAsync` returns `(string,string)` (non-nullable struct); `??` is invalid | Moved the `throw` inside the factory via `?? throw` on each individual setting lookup |

---

## Pending

- **`GetApiKeyForPanoramaFromSettings`**: If the Panorama API key is needed by a future
  method, inject `IManageUnifiedSettingsAsync` and cache the result in `IMemoryCache`
  (key: `$"PanoramaKey_{orgRealPageId}"`, TTL: 180 s). The removal is intentional —
  dead code at the call site in the constructor.
