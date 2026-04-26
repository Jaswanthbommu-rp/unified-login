# ManageMicrosoftAzureAsync Refactor Changelog

**Date:** 2026-04-21  
**Branch:** `feature/UserRepositoryRefactor`  
**Target:** .NET 10 / C# 13  

---

## Summary

Full async refactor of `Logic/ManageMicrosoftAzure.cs` into `LogicAsync/`.  
Original class retained as-is for legacy consumers. Two new files created.

---

## Files Created

| File | Notes |
|------|-------|
| `LogicAsync/Interfaces/IManageMicrosoftAzureAsync.cs` | New interface — `GetADUserInfoAsync` returns `AzureUser?` |
| `LogicAsync/ManageMicrosoftAzureAsync.cs` | Full async implementation |
| `ManageMicrosoftAzureAsync-Refactor.md` | This file |

---

## Key Changes

### 1. Async DB settings load replaces constructor sync call

**Before:** constructor called `_productInternalSettingRepository.GetProductInternalSettings(3)` synchronously — blocking on startup and preventing DI from being async-safe.

```csharp
public ManageMicrosoftAzure(DefaultUserClaim defaultUserClaim)
{
    _productInternalSettingRepository = new ProductInternalSettingRepository();
    var productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings(3);
    _azureTokenAddress = productInternalSettingList.First(a => a.Name.Equals("AzureTokenAddress", ...)).Value;
    // ... 3 more First() calls
    _httpClient = new HttpClient { BaseAddress = new Uri(azureUserGraphAPI) };
}
```

**After:** settings are loaded inside `GetADUserInfoAsync` with `GetProductInternalSettingsAsync`, keeping the constructor free of I/O. For a scoped service this is one load per request — same effective cost with no constructor blocking.

---

### 2. `new HttpClient` / `new HttpClientHandler` replaced with `IHttpClientFactory`

**Before:**
```csharp
private HttpMessageHandler _httpGetMessageHandler = new HttpClientHandler();
private HttpClient _httpClient;
// ...
_httpClient = new HttpClient { BaseAddress = new Uri(azureUserGraphAPI) };
```
Risks: socket exhaustion, DNS staleness, undisposed handlers.

**After:**
```csharp
using var client  = _httpClientFactory.CreateClient();
using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
```
`using var` on both `HttpClient` and `HttpRequestMessage` ensures deterministic disposal. Base address is constructed from settings per-call — no stale URI risk.

---

### 3. `.Result` deadlock eliminated

**Before:**
```csharp
var response        = _httpClient.GetAsync(...).Result;          // sync-over-async
var responseContent = response.Content.ReadAsStringAsync().Result; // sync-over-async
```
Both `.Result` calls can deadlock on ASP.NET synchronisation contexts.

**After:**
```csharp
var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
var json     = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
```
`ReadAsStringAsync(CancellationToken)` overload (.NET 5+) threads cancellation all the way through.

---

### 4. `DefaultUserClaim` constructor injection removed

**Before:** `DefaultUserClaim _defaultUserClaim` was stored but only used implicitly (for Serilog context in the original).

**After:** `IUserClaimsAccessor` is injected; `_userClaims.CorrelationId` is used for structured log correlation. No claim mutation.

---

### 5. Inline `new TokenHelper()` / `new ProductInternalSettingRepository()` replaced with DI

**Before:**
```csharp
_productInternalSettingRepository = new ProductInternalSettingRepository();
_tokenHelper = new TokenHelper();
```

**After:** `IProductInternalSettingRepositoryAsync` and `ITokenHelperAsync` are DI-injected — testable, mockable, no hidden dependencies.

---

### 6. Serilog static `Log.Write` replaced with `ILogger<T>`

**Before:**
```csharp
var logger = Log.Logger;
logger = logger.ForContext("ProductModule", this.GetType());
logger.Write(Serilog.Events.LogEventLevel.Debug, "{ActionName} - {state}", ...);
logger.Write(Serilog.Events.LogEventLevel.Error, ex, ...);
```

**After:**
```csharp
_logger.LogDebug("{CorrelationId} GetADUserInfoAsync Url={Url}", _userClaims.CorrelationId, requestUrl);
_logger.LogWarning("{CorrelationId} GetADUserInfoAsync non-success StatusCode={StatusCode} ...", ...);
_logger.LogError(ex, "{CorrelationId} GetADUserInfoAsync failed UserName={UserName}", ...);
```
Non-success HTTP responses now emit a `LogWarning` (previously swallowed silently).

---

### 7. OData query URL cleaned up

**Before:** spaces around `=` and `&` were non-standard:
```
/v1.0/users?$filter = userPrincipalName eq '{userName}' &$select = userPrincipalName,...
```

**After:** standard OData syntax, duplicate `userPrincipalName` in `$select` removed:
```
/v1.0/users?$filter=userPrincipalName eq '{userName}'&$select=userPrincipalName,onPremisesSamAccountName,displayName,mail,id
```

---

### 8. Missing-setting `First()` → `GetRequired` helper with `InvalidOperationException`

**Before:** `First(a => a.Name.Equals(...)).Value` — throws `InvalidOperationException` with an unhelpful "sequence contains no elements" message if a setting is missing.

**After:** `GetRequired(settings, name)` throws with a message that names the missing key:
```
Required Azure setting 'AzureTokenAddress' is missing from product internal settings (productId=3).
```

---

### 9. Nullable return type made explicit

**Before:** `AzureUser GetADUserInfo(string)` — returns `null` on failure with no compiler hint.

**After:** `Task<AzureUser?> GetADUserInfoAsync(string, CancellationToken)` — nullable annotation forces callers to handle the null case at compile time.

---

## C# 13 / .NET 10 Features Used

| Feature | Usage |
|---------|-------|
| File-scoped namespace | All new files |
| `sealed` class | `ManageMicrosoftAzureAsync` |
| `ConfigureAwait(false)` | All `await` sites |
| Nullable reference type `AzureUser?` | Interface + return site |
| `using var` | `HttpClient`, `HttpRequestMessage` |
| `ReadAsStringAsync(CancellationToken)` | .NET 5+ overload, full cancellation chain |
| `is not null` / `?.` null-conditional | `userInfo?.value?.FirstOrDefault()?.userPrincipalName` |

---

## DI Registration Template

```csharp
services.AddScoped<IManageMicrosoftAzureAsync, ManageMicrosoftAzureAsync>();
services.AddHttpClient(); // required for IHttpClientFactory
```

All other injected interfaces (`IProductInternalSettingRepositoryAsync`, `ITokenHelperAsync`, `IUserClaimsAccessor`) are already registered by existing infrastructure.

---

## Known Limitations

### Settings loaded per-call (no caching)

`GetProductInternalSettingsAsync(3, ct)` is called on every `GetADUserInfoAsync` invocation. If this method is called in a hot path, add `IMemoryCache` caching for the Azure settings (TTL ~5 min) to avoid redundant DB round-trips.

### Token not cached

`GetExternalClientCredentialServerTokenAsync` is called on every invocation. `ITokenHelperAsync` implementations typically cache tokens internally for 5 minutes — confirm this is the case before adding a second caching layer.
