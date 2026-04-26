# ManageProductProspectContactAsync Refactor Changelog

**Date:** 2026-04-13
**Branch:** feature/UserRepositoryRefactor

---

## Overview

This refactor transforms `ManageProductProspectContact.cs` (~1 230 lines, two constructors,
`DefaultUserClaim`-bound, synchronous blocking HTTP calls) into
`ManageProductProspectContactAsync.cs` (~560 lines), a fully async, DI-first implementation
of the Prospect Contact Center (PCC) product user-management service.

---

## Constructor Expansion

### Legacy (two constructors)

```csharp
// Production constructor
public ManageProductProspectContact(DefaultUserClaim userClaims)
    : base((int)ProductEnum.ProspectContactCenter, userClaims, ...) { ... }

// Test/override constructor
public ManageProductProspectContact(Guid editorRealPageId, DefaultUserClaim userClaims,
    HttpMessageHandler, IProductInternalSettingRepository, IManagePersona,
    ISamlRepository, IManageBlueBook, IRepository)
    : base(...) { ... }
```

Both constructors loaded internal product settings synchronously from
`ProductInternalSettingRepository` at construction time, storing `_apiEndPoint` as a
mutable static field.

### Refactored (single DI constructor, 11 deps)

| Dependency | Replaces |
|---|---|
| `IProductContextServiceAsync` | `GetCompanyEditorAndUserDetails` / `DefaultUserClaim` |
| `IProductSettingServiceAsync` | `_productInternalSettingList.First(...)` loaded at ctor time; `UpdateProductSettingProductStatus` |
| `IManagePersonaAsync` | `_managePersona.GetPersona(...)` |
| `IManagePersonAsync` | `_managePerson.GetPerson(...)` |
| `IManageUserLoginAsync` | `_manageUserLogin.GetUserLoginOnly(...)`, `GetUserPersonaOrganization(...)` |
| `IManageElectronicAddressAsync` | `new ManageElectronicAddress().ListElectronicAddressForPerson(...)` per-method |
| `ISamlRepositoryAsync` | `_samlRepository.CreateSamlUserAttribute(...)`, `GetProductSamlDetails(...)`, `UpdateSamlUserAttribute(...)` |
| `IProductRepositoryAsync` | `_productRepository.GetProductUsersByCompany(...)` |
| `IManageBlueBookAsync` | `GetProductCompanyInstanceId(_udmSourceCode)` via base class |
| `IHttpClientFactory` | `new HttpClient()` per-call (×7 distinct sites) + shared base-class `_client` |
| `ILogger<ManageProductProspectContactAsync>` | `WriteToDiagnosticLog` / `WriteToErrorLog` |

All guards use `ArgumentNullException.ThrowIfNull` (.NET 6+).

---

## Mutable State Eliminated

| Legacy mutable field | Replaced by |
|---|---|
| `static string _apiEndPoint` | `GetApiEndPointAsync(ct)` — resolves per-call via `IProductSettingServiceAsync` |
| `_productRepository` (instance field) | Injected `IProductRepositoryAsync` |
| `_productUserId`, `_productUsername`, `_editorProductUserId` | `ProductCallContext.ProductUserId`, `.ProductUsername`, `.EditorProductUserId` (from `IProductContextServiceAsync`) |
| `_udmSourceCode` | `BlueBookProductConstants.ProspectContactCenter` constant; company mapped via `IManageBlueBookAsync.GetProductCompanyMappingAsync` |

---

## `out` Parameters Removed

Both methods with `out List<AdditionalParameters>` are incompatible with `async`.
Replaced by named tuple returns:

```csharp
// Legacy
string ManageProductProspectContactUser(long, long, ProspectContactPropertyRole,
    out List<AdditionalParameters>, BatchProcessType)

// Async
Task<(string error, List<AdditionalParameters> additionalParameters)>
    ManageProductProspectContactUserAsync(long, long, ProspectContactPropertyRole,
        BatchProcessType, CancellationToken)
```

The same applies to `ChangeProspectContactUserType` which now delegates to
`ManageProductProspectContactUserAsync` and propagates the tuple.

---

## HTTP Fixes — `.Result` / Blocking Removed

| Legacy site | Fix |
|---|---|
| `client.GetAsync(...).Result` in `GetProductProperties` | `await client.GetAsync(...)` |
| `response.Content.ReadAsStringAsync().Result` (×3 in private helpers) | `await response.Content.ReadAsStringAsync(ct)` |
| `client.PutAsJsonAsync(url, migrateUsers).Result` | `await client.PutAsJsonAsync(url, migrateUsers, ct)` |
| `client.SendAsync(request).Result` in `IsUsernameAvailable` | `await client.SendAsync(request, ct)` |
| `_client.GetAsync(baseUrlAndQuery).Result` in `GetResultFromApi` | `await client.GetAsync(url, ct)` |
| `client.DeleteAsync(requestUrl).Result` in `DeactivateCurrentUser` | `await client.DeleteAsync(url, ct)` |
| `client.PutAsJsonAsync(productApiUrl, ...).Result` in `UpdateProspectContactCenterUser` | `await client.PutAsJsonAsync(apiUrl, ..., ct)` |
| `client.PostAsJsonAsync(productApiUrl, ...).Result` in `InsertProspectContactCenterUser` | `await client.PostAsJsonAsync(apiUrl, ..., ct)` |

Every `new HttpClient()` / `using (var client = new HttpClient())` site replaced with
`_httpClientFactory.CreateClient()`.

---

## `ExpandoObject` + `ObjectContent<dynamic>` Eliminated

`UpdateUserProperty` used the legacy WebAPI client helpers:

```csharp
// Legacy (System.Net.Http.Formatting — removed in .NET 5+)
dynamic userPropObj = new ExpandoObject();
userPropObj.PropertyID = 0;
userPropObj.ModifyingUser = modifyingUserId;
userPropObj.Properties = propertyId;
userPropObj.ManagementCompanyID = companyID;
var content = new ObjectContent<dynamic>(userPropObj, new JsonMediaTypeFormatter());
var request = new HttpRequestMessage(new HttpMethod("PATCH"), apiUrl) { Content = content };
```

Replaced with an anonymous type + `JsonContent.Create`:

```csharp
var body = new { PropertyID = 0, ModifyingUser = modifyingUserId,
                 Properties = propertyIds, ManagementCompanyID = companyId };
using var request = new HttpRequestMessage(new HttpMethod("PATCH"), apiUrl)
    { Content = JsonContent.Create(body) };
```

---

## `dynamic` JSON Result Eliminated

`InsertProspectContactCenterUser` used `dynamic userResult = JsonConvert.DeserializeObject<dynamic>(jsonContent)`.
Replaced with `System.Text.Json.JsonDocument.Parse`:

```csharp
using var doc = JsonDocument.Parse(json);
string result = doc.RootElement.ValueKind == JsonValueKind.String
    ? doc.RootElement.GetString() ?? string.Empty
    : doc.RootElement.TryGetProperty("id", out var idEl)
        ? idEl.GetString() ?? string.Empty
        : json.Trim('"');
```

---

## Local DTO Classes — Newtonsoft → STJ Attributes

The two local API payload models (`ProspectContactCenterUser`, `ProspectContactCenterUserProfile`)
are renamed to `PccUser` / `PccUserProfile` and moved to file-scope sealed classes.
`[JsonProperty]` (Newtonsoft) attributes are replaced with `[JsonPropertyName]` /
`[JsonIgnore]` (STJ) so `PutAsJsonAsync` / `PostAsJsonAsync` serialise correctly without
an additional Newtonsoft pass.

Newtonsoft is retained only for:
- `JsonConvert.DeserializeObject<MigrateResponse>(responseContent)` — `MigrateResponse` carries Newtonsoft `[JsonProperty]` attributes from SharedObjects.
- `JsonConvert.DeserializeObject(..., typeof(IList<ProductPropertyMap>))` — `ProductPropertyMap` is a SharedObjects type.

---

## `ResolveUserEmailAsync` Extracted

Three identical email-lookup blocks (in `UnassignUser`, `ManageProductProspectContactUser`,
`UpdateProspectContactCenterUserProfile`) are replaced by a single private helper:

```csharp
private async Task<string> ResolveUserEmailAsync(
    Guid realPageId, string loginName, CancellationToken ct)
{
    var addresses = await _manageElectronicAddress.ListElectronicAddressForPersonAsync(realPageId, ct: ct);
    string email = addresses?
        .FirstOrDefault(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase))
        ?.AddressString ?? string.Empty;
    return email is { Length: > 0 } ? email : ValidateAndReturnEmailAddress(loginName);
}
```

---

## `IsUsernameAvailable` Renamed

The legacy `IsUsernameAvailable` returned `true` when the username was **in use** (HTTP 2xx
= user found in product), which was counter-intuitive. Renamed to `IsUsernameInUseAsync`
to clarify the semantic. `GetUniqueProductLoginNameAsync` loops while `IsUsernameInUseAsync`
returns `true` — functionally identical, more readable.

---

## .NET 10 / C# 12 Improvements

### Collection Expressions `[]`

| Before | After |
|---|---|
| `additionalParameters = new List<AdditionalParameters>()` | `List<AdditionalParameters> additionalParameters = []` |
| `PropertyList = new List<string> { "ALL" }` | `PropertyList = ["ALL"]` |
| `Properties = new List<string>() { "0" }` | `Properties = ["0"]` |
| `Properties = new List<string>()` | `Properties = []` |
| `?? new List<string>()` fallbacks | `?? []` |
| `.ToList() ?? []` migration user properties | `.ToList() ?? []` |

### Pattern Matching

```csharp
// Before
if (company.CompanyInstanceSourceId == null || company.CompanyInstanceSourceId == "")

// After
if (string.IsNullOrEmpty(company?.CompanyInstanceSourceId))
```

```csharp
// Before
if (pccUser != null && pccUser.Properties.Count() > 0)

// After
if (pccUser.Properties?.Count > 0)
```

### `StringComparison.OrdinalIgnoreCase`

All string comparisons (`"EMAIL"`, `"PRODUCTUSERNAME"`, `"UserId"`, `"ALL"`, `"M"`) use
`StringComparison.OrdinalIgnoreCase` or `.Equals(..., OrdinalIgnoreCase)`.

### `when (ex is not OperationCanceledException)` Catch Guards

All `catch (Exception ex)` blocks use a `when` filter to let task cancellation propagate
naturally without being swallowed.

### `DateTime.UtcNow.Ticks` → was `DateTime.Now.Ticks`

The login-name rename suffix in `UpdateProspectContactCenterPropertyUserAsync` now uses
`DateTime.UtcNow.Ticks` to avoid local-time ambiguity.

### `LINQ .Select()` in `GetMigrationUsersAsync`

The `foreach` loop building `migrationUsers` is replaced with LINQ `.Select(...)`.

---

## Pending / Not Ported

### `WriteUpdateUserTypeActivityLog`

Called in `ManageProductProspectContactUser` (user-type changes) and
`UpdateProspectContactCenterUserProfile` (profile update). This is a
`ManageProductBase` method that calls `PushToQueue` via the sync `ManageUnifiedLogin`.
Marked `// TODO: pending IProductAuditServiceAsync.WriteUpdateUserTypeActivityLogAsync`
in both locations.

---

## Removed / Not Ported

- **`ManageProductBase` base class**: Eliminated. No inheritance. All previously-inherited helpers resolved via injected services or private helpers.
- **`static string _apiEndPoint` field**: Eliminated. Per-call resolution via `GetApiEndPointAsync`.
- **`#if DEBUG WriteToDiagnosticLog` blocks**: Replaced by `ILogger` at `Debug` level throughout.
- **`DefaultUserClaim` constructor parameter**: Removed from all method signatures.
- **Legacy test constructor**: Eliminated. `IHttpClientFactory` and mocked repositories support test doubles without a separate constructor.
- **`System.Dynamic.ExpandoObject`**: Removed. No longer imported.
- **`System.Net.Http.Formatting` (ObjectContent, JsonMediaTypeFormatter)**: Removed. No longer imported.
- **`ManageProductBase.MergeProductPropertiesWithGreenbook` inherited call**: Converted to private async `MergeProductPropertiesWithGreenbookAsync`.

---

## Files Changed

- `UnifiedLogin.BusinessLogic/LogicAsync/Interfaces/IManageProductProspectContactAsync.cs` — interface rewritten; `DefaultUserClaim` removed from all signatures; 4 missing methods added (`UnassignUserAsync`, `ChangeProspectContactUserTypeAsync`, `ManageProductProspectContactUserAsync`, `UpdateProspectContactCenterUserProfileAsync`); `out` params → tuple returns
- `UnifiedLogin.BusinessLogic/LogicAsync/Product/ManageProductProspectContactAsync.cs` — new file, ~560 lines; single 11-dep DI constructor; 8 public methods; 12 private helpers; `PccUser`/`PccUserProfile` sealed file-scope DTOs with STJ attributes; `JsonDocument.Parse` for `dynamic` result; `JsonContent.Create` for PATCH body; `IHttpClientFactory` throughout; all `.Result` blocking removed
