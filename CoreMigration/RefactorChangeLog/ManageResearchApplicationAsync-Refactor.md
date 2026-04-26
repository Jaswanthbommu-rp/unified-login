# ManageResearchApplicationAsync – Refactor Changelog

## Source
`UnifiedLogin.BusinessLogic/Logic/Product/ManageResearchApplication.cs`  
_(File is excluded from compilation: `<Compile Remove="Logic\Product\ManageResearchApplication.cs" />`)_

## Output
| File | Action |
|------|--------|
| `LogicAsync/Interfaces/IManageResearchApplicationAsync.cs`  | Created – true-async interface (no `DefaultUserClaim`) |
| `LogicAsync/Product/ManageResearchApplicationAsync.cs`       | Created – native async implementation |
| `LogicAsync/ManageResearchApplicationAsync.cs`               | Created – namespace-only placeholder |

---

## Breaking Changes

### `DefaultUserClaim` removed from all public APIs
Constructor no longer accepts `DefaultUserClaim`. Editor/user context (personas, product user ID/username) is
resolved internally per-call via `IProductContextServiceAsync.GetUserContextAsync`.

### `partyId` remains a parameter on `GetRolesAsync` and `GetRightsByRoleAsync`
The original sync code passed `partyId` explicitly to these methods. That convention is preserved in the
async interface because the caller knows the org party ID at the controller layer.

### Dead code eliminated — `GetProductIdsByOrg()` call
The original `GetRoles` and `GetRightsByRole` called `GetProductIdsByOrg()` (which used the editor's
`OrganizationRealPageGuid`) and immediately discarded the result. The subsequent
`pr.GetProductIdsByCompany(partyId)` (using the explicit `partyId` param) was the one actually used.
The dead call is removed from the async version.

---

## Constructor — from 1 to 9 dependencies

| # | Dependency | Replaces |
|---|-----------|---------|
| 1 | `IProductContextServiceAsync` | `DefaultUserClaim` + `GetCompanyEditorAndUserDetails` |
| 2 | `IProductRepositoryAsync` | `new ProductRepository()` inline constructions |
| 3 | `IUnifiedLoginRepositoryAsync` | `new UnifiedLoginRepository()` inline in `GetRightsByRole` |
| 4 | `IManageUserRoleRightAsync` | `new UserRoleRightRepository()` inline + `GetAssignedRoleForPersona` base helper |
| 5 | `IManageUserLoginAsync` | `_manageUserLogin.GetUserLoginOnly(realPageId)` (sync base helper) |
| 6 | `IHttpClientFactory` | `_client` (`HttpClient`) instance field set in constructor |
| 7 | `IMemoryCache` | `MemoryCache.Default` (`ObjectCache`) for token |
| 8 | `IConfiguration` | `ConfigReader.GetIssuerUri` (reads `UnifiedPlatform:Authority`) |
| 9 | `ILogger<ManageResearchApplicationAsync>` | `WriteToDiagnosticLog` / `WriteToErrorLog` base helpers |

`ArgumentNullException.ThrowIfNull` guard applied to each dependency.

---

## Mutable instance fields eliminated

| Old field | Problem | New approach |
|-----------|---------|-------------|
| `string _researchApplicationUrl` | Set in constructor via sync DB fetch | `config.ProductUrl` per call (cached 1 hr) |
| `string _researchApplicationApiEndPoint` | Set in constructor | `config.ApiEndpoint` per call |
| `string _accessToken` | Shared across requests — not thread-safe | Per-call from `GetAccessTokenAsync` (cached 9 min in `IMemoryCache`) |
| `string _UnifiedLoginResearchApplicationClientSecret` | Constructor-time base-64 decode | Cached in `ResearchAppConfig` record |
| `DefaultUserClaim _userClaims` | Per-request context tied to instance | `ProductCallContext` returned per call |

---

## Blocking `.Result` calls — all eliminated

| Original | Async replacement |
|----------|-------------------|
| `tokenClient.RequestClientCredentialsAsync(scope).Result` | `await tokenClient.PostAsync(tokenEndpoint, form, ct)` |
| `_client.SendAsync(req).Result` | `await client.SendAsync(req, ct)` |
| `postResponse.Content.ReadAsStringAsync().Result` | `await postResponse.Content.ReadAsStringAsync(ct)` |

---

## `IdentityModel.Client.TokenClient` eliminated

### Original (deprecated IdentityModel.Client)
```csharp
TokenClient tokenClient = new TokenClient($"{tokenUri}/connect/token", "UnifiedLoginResearchApp", _secret);
var tokenResponse = tokenClient.RequestClientCredentialsAsync(scope).Result;
_accessToken = tokenResponse.AccessToken;
```

### New (standard HttpClient form POST)
```csharp
using var tokenClient = _httpClientFactory.CreateClient("ResearchApplicationToken");
using var form = new FormUrlEncodedContent(new Dictionary<string, string>
{
    ["grant_type"]    = "client_credentials",
    ["client_id"]     = "UnifiedLoginResearchApp",
    ["client_secret"] = config.ClientSecret,
    ["scope"]         = "UnifiedLoginResearchApp"
});
var response = await tokenClient.PostAsync(config.TokenEndpoint, form, ct);
```

Removes the dependency on the deprecated `IdentityModel.Client` (`TokenClient`) package.

---

## `ConfigReader.GetIssuerUri` → `IConfiguration`

### Original
```csharp
string tokenUri = ConfigReader.GetIssuerUri;   // reads "UnifiedPlatform:Authority" via static accessor
```

### New
```csharp
string issuerUri = _configuration["UnifiedPlatform:Authority"]
    ?? throw new InvalidOperationException("UnifiedPlatform:Authority is not configured");
string tokenEndpoint = $"{issuerUri.TrimEnd('/')}/connect/token";
```

`ConfigReader` is a static class that wraps `ConfigurationManager`/`IConfiguration`.
The async version injects `IConfiguration` directly — testable, no static dependency.

---

## Token caching — `MemoryCache.Default` → `IMemoryCache`

### Original
```csharp
ObjectCache tokenCache = MemoryCache.Default;
_accessToken = tokenCache["access_token_UnifiedLoginResearchApp"] as string;
tokenCache.Set("access_token_UnifiedLoginResearchApp", _accessToken, new CacheItemPolicy { AbsoluteExpiration = ... });
```

### New
```csharp
if (_cache.TryGetValue(TokenCacheKey, out string? cached) && !string.IsNullOrEmpty(cached))
    return cached!;
// POST to token endpoint…
_cache.Set(TokenCacheKey, token, TokenCacheTtl);
```

Cache key: `"RA_AccessToken"`, TTL: 9 minutes (matches original).

---

## Config caching

Product settings (`APIENDPOINT`, `PRODUCTURL` from ResearchApplication; `UNIFIEDLOGINRESEARCHAPPLICATIONCLIENTSECRET` from UnifiedPlatform) are loaded once and cached 1 hour as:

```csharp
internal sealed record ResearchAppConfig(
    string ApiEndpoint,
    string ProductUrl,
    string ClientSecret,
    string TokenEndpoint);
```

Cache key: `"RA_Config"`. Both product setting fetches are parallelised with `Task.WhenAll`.

---

## `ResearchApplicationRepository` / `UserRoleRightRepository` inline construction eliminated

### Original
```csharp
ResearchApplicationRepository ocr = new ResearchApplicationRepository();
IUserRoleRightRepository userRoleRightRepository = new UserRoleRightRepository();
```

### New
All DB operations go through the injected `IManageUserRoleRightAsync` service:
```csharp
await _userRoleRight.InsertAssignedRoleToUserAsync(userPersonaId, roleId, editorUserId, deleteRole, ct);
var roles = await _userRoleRight.GetAssignedRoleForPersonaAsync(ProductEnum.ResearchApplication, userPersonaId, ct);
```

`IProductRepositoryAsync` and `IUnifiedLoginRepositoryAsync` replace `ProductRepository` and
`UnifiedLoginRepository` inline constructions.

---

## `ManagePersona.GetPersona` eliminated

### Original
```csharp
var userPersona = _managePersona.GetPersona(userPersonaId);
var realPageId = userPersona.RealPageId;
```

### New
`realPageId` is obtained from `ctx.UserPersona!.RealPageId` — the persona is already resolved
by `IProductContextServiceAsync.GetUserContextAsync`, eliminating a redundant DB call.

---

## `Person` fetch eliminated

The sync code fetched `_managePerson.GetPerson(realPageId)` in `ManageResearchApplicationUser`
but the returned `Person` object was never used in the method body (no product login name
generation, unlike RUM/VendorServices). The async version skips this fetch entirely.

---

## `editorUserId` — resolved from editor user login

The original used `_userClaims.UserId` (from `DefaultUserClaim`). The async version fetches it once:

```csharp
var editorLogin  = await _userLogin.GetUserLoginOnlyAsync(ctx.EditorPersona.RealPageId, ct);
int editorUserId = (int)(editorLogin?.UserId ?? 0L);
```

The user login fetch is parallelised alongside the target user's login fetch:
```csharp
var userLoginTask   = _userLogin.GetUserLoginOnlyAsync(ctx.UserPersona.RealPageId, ct);
var editorLoginTask = _userLogin.GetUserLoginOnlyAsync(ctx.EditorPersona.RealPageId, ct);
await Task.WhenAll(userLoginTask, editorLoginTask);
```

---

## `MergeSelRolesWithGreenbook` — O(n²) LINQ → O(1) Dictionary lookup

### Original
```csharp
foreach (var role in roleList)
{
    if (allRoles.Any(a => a.ID == role.RoleID.ToString()))
    {
        ProductRole selrole = (from a in allRoles where a.ID == role.RoleID.ToString() select a).FirstOrDefault();
        if (selrole != null) selrole.IsAssigned = true;
    }
}
```

### New
```csharp
var roleById = allRoles.ToDictionary(r => r.ID);
foreach (var role in assignedRoles)
{
    if (roleById.TryGetValue(role.RoleID.ToString(), out var selRole))
        selRole.IsAssigned = true;
}
```

Single scan to build the dictionary, then O(1) lookup per assigned role.

---

## `ResearchApplicationDeleteUser` — inner class → `internal sealed record`

### Original
```csharp
private class ResearchApplicationDeleteUser
{
    [JsonProperty(PropertyName = "UserId")]
    public long UserId { get; set; }

    [JsonProperty(PropertyName = "PersonaId")]
    public long PersonaId { get; set; }
}
```

### New
```csharp
internal sealed record ResearchApplicationDeleteUser
{
    [Newtonsoft.Json.JsonProperty("UserId")]
    public long UserId { get; init; }

    [Newtonsoft.Json.JsonProperty("PersonaId")]
    public long PersonaId { get; init; }
}
```

`init`-only properties enforce immutability; `sealed record` adds value equality.

---

## `MapGbObjectToProduct` — `foreach` copy → C# 12 spread

### Original
```csharp
result.RoleList = new List<string>();
foreach (var roleId in userProductPropertyRole.RoleList)
    result.RoleList.Add(roleId);
```

### New
```csharp
result.RoleList = [.. source.RoleList];
```

---

## HTTP clients

### Named clients (DI Registration)
```csharp
services.AddHttpClient("ResearchApplication");        // event-api calls
services.AddHttpClient("ResearchApplicationToken");   // token acquisition
services.AddScoped<IManageResearchApplicationAsync, ManageResearchApplicationAsync>();
```

Clients are created per-call via `_httpClientFactory.CreateClient(...)` — no shared `HttpClient` instance field.

---

## C# / .NET improvements

| Feature | Usage |
|---------|-------|
| C# 12 collection expressions `[]` | `new List<>()` / spread `[.. source.RoleList]` |
| `when (ex is not OperationCanceledException)` | All catch guards |
| `is null or { Count: 0 }` pattern | Null/empty role list guard |
| `is { Count: > 0 }` pattern | Role list presence check |
| `internal sealed record ResearchAppConfig` | Immutable config value object |
| `internal sealed record ResearchApplicationDeleteUser` | Immutable API body (replaces private class) |
| `init` properties on records | Enforce post-construction immutability |
| `string.Equals(…, OrdinalIgnoreCase)` | Case-insensitive name comparisons |
| `Dictionary<string, ProductRole>.TryGetValue` | O(1) role merge lookup |
| `Task.WhenAll` | Parallel product settings fetch + parallel user/editor login fetch |
| `ValueTask<T>` returns | `GetConfigAsync` / `GetAccessTokenAsync` — avoid `Task` allocation when cached |
| `ArgumentNullException.ThrowIfNull` | Constructor guards |
| Removed `Person` fetch | `GetPerson` was called but result was unused in source |
| Removed dead `GetProductIdsByOrg()` | Calls discarded immediately in `GetRoles`/`GetRightsByRole` |

---

## Pending / Notes

- **`DataObject<T>` from `ResidentPortal` namespace**: The delete-user API body uses `DataObject<ResearchApplicationDeleteUser>` from `UnifiedLogin.SharedObjects.Product.ResidentPortal`. This shared type is used as-is — the Research Application API contract requires the `{"data": {...}}` wrapper.
- **Token scope = ClientId**: `scope = "UnifiedLoginResearchApp"` (same as `client_id`). Preserved from original. Validate against the IdentityServer configuration if the scope registration changes.
- **`UnassignUserAsync` parameter `userAssignProductPropertyRole`**: The parameter exists in the interface for API compatibility, but the original sync implementation never used it. The async version also ignores it.
