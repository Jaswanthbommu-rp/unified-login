# ManageUserRegistrationEmailAsync — Refactor Changelog

**Date:** 2026-04-23  
**Branch:** `feature/UserRepositoryRefactor`  
**Author:** Vijayasekher Manellore  
**Framework target:** .NET 10 / C# 13.0

---

## Files Created

| File | Role |
|------|------|
| `UnifiedLogin.BusinessLogic/LogicAsync/Interfaces/IManageUserRegistrationEmailAsync.cs` | New async-first interface |
| `UnifiedLogin.BusinessLogic/LogicAsync/ManageUserRegistrationEmailAsync.cs` | Native async implementation |

---

## What Changed

### Architecture

| Before (sync) | After (async) |
|---------------|---------------|
| `DefaultUserClaim` injected via constructor | `IUserClaimsAccessor` injected — no mutable claim object |
| 3 constructors (primary / unit-test / IRepository) | Single DI constructor — test hosts supply mock implementations |
| `new XRepository()` / `new XLogic()` inside constructors | All dependencies injected — zero `new` in constructor body |
| `Serilog.Log.Logger.ForContext(...)` | `ILogger<T>` (Microsoft.Extensions.Logging) with structured parameters |
| `ref bool isSendGridEnabled` out-parameter | Named tuple `(string status, bool isSendGridEnabled)` return value |
| Duplicated 4-step communication-event chain in every method | Extracted to `SaveCommunicationEventChainAsync` private helper |

### Dependency Substitutions

| Sync dependency | Async replacement |
|-----------------|-------------------|
| `IManageEmail` | `IManageEmailAsync` |
| `IContactMechanismRepository` | `IManageContactMechanismAsync` (logic layer, per project convention) |
| `IManageCommunicationEvents` | `IManageCommunicationEventsAsync` |
| `IUserTokenRepository` | `IUserTokenRepositoryAsync` |
| `IManagePerson` | `IManagePersonAsync` |
| `IUserLoginRepository` | `IUserLoginRepositoryAsync` |
| `IProductInternalSettingRepository` | `IProductInternalSettingRepositoryAsync` |

### Email-Send Method Mapping

| Sync method | Async replacement |
|-------------|-------------------|
| `_emailLogic.GetEmailTemplate(...)` | `GetEmailTemplateAsync(...)` |
| `_emailLogic.CreateWelcomeEmail(...)` | `CreateWelcomeEmailAsync(...)` |
| `_emailLogic.SendEmailAsync(emailModel)` → returned `bool` | `SendEmailModelAsync(emailModel, ct)` → `Task<bool>` |
| `_emailLogic.SendGridEmail(sendGridEmail)` | `SendGridEmailAsync(sendGridEmail, ct)` |
| `_emailLogic.SendEmail(cesEmail)` | `SendEmailAsync(cesEmail, ct)` |

---

## Performance Improvements

### `SendNewUserRegistrationEmailAsync`

Three parallel fetch phases reduce wall-clock latency versus sequential sync calls:

```
Phase 1  GetPersonAsync                     ┐ parallel
         ListOrganizationByEnterpriseUserId ┘

Phase 2  (sequential — depends on orgEntry from Phase 1)
         GetUserOrganizationWithStatusAsync (conditional)
         GetUserActivityTokenAsync          (conditional)

Phase 3  GetEmailTemplateAsync              ┐ parallel
         ListContactMechanismForPerson (From)│
         ListContactMechanismForPerson (To)  ┘
```

### `SendPasswordResetEmailAsync`

```
Phase 1  GetPersonAsync       ┐ parallel
         GetUserLoginOnlyAsync┘

Phase 2  GetUserOrganizationWithStatusAsync ┐ parallel
         GetUserActivityTokenAsync          ┘

Phase 3  GetEmailTemplateAsync              ┐ parallel
         ListContactMechanismForPerson (From)│
         ListContactMechanismForPerson (To)  ┘
```

### Product-Settings Lookup

The `GetEmailStatusAsync` helper calls `GetProductInternalSettingsAsync` **once** and resolves both
`IsSendGridEnabled` and `IsUnifiedEmailEnabled` from the cached result, replacing the original code
which called `.ToList()` and iterated the collection **four times** across two separate blocks.

---

## C# 13.0 / .NET 10 Features Applied

| Feature | Usage |
|---------|-------|
| **Switch expression with `or` pattern** | `(UserRoleType)userTypeId switch { UserRoleType.RealPageEmployee or UserRoleType.SuperUser => ... }` — replaces multi-`case` switch statement |
| **Collection expressions `[...]`** | `To = [new UserEmail { ... }]`, `Bcc = []`, `toAddress = [new EmailAddress { ... }]` — replaces `new List<T> { ... }` |
| **`is not null` / `is null` patterns** | Null checks throughout (`orgEntry?.PrimaryOrganization is true`, `cesEmail.EmailBody is not null`) |
| **Null-conditional chaining** | `?.Value?.Equals(...)`, `?.Value?.Trim()`, `orgEntry?.RealPageId ?? Guid.Empty` |
| **File-scoped namespace** | `namespace UnifiedLogin.BusinessLogic.LogicAsync;` |
| **`sealed` class** | Prevents unintended subclassing |
| **`string.Contains` with `StringComparison`** | `emailStatus.Contains("success", StringComparison.OrdinalIgnoreCase)` — explicit culture-safe comparison |
| **`ArgumentNullException` guard** | `?? throw new ArgumentNullException(nameof(...))` on every constructor parameter |

---

## Behavioural Fidelity Notes

- **`IsUnifiedEmailEnabled` default `true`** — When the product setting is absent, the original code
  defaulted to `true`. This is preserved: `unifiedEmailEntry is null || unifiedEmailEntry.Value.Trim() == "1"`.
- **Dead code removed** — `SendPasswordResetEmail` fetched `organizationList` but never used it.
  That dead call is not present in the async version.
- **`organizationPartyId` null-safety** — The sync code would throw `NullReferenceException` if the
  org entry was not found (direct `.RealPageId` access). The async version uses `?.RealPageId ?? Guid.Empty`.
- **CES event guard** — The `CreateCESCommunicationEventEmailAsync` call is intentionally skipped
  when `isSendGridEnabled` is true, exactly matching the original `(!IsSendGridEnabled)` guard.
- **DEBUG build shortcut** — `#if DEBUG … emailStatus = "success" … #endif` preserved from original.

---

## Blocked Dependency Unblocked

This refactor satisfies the `IManageUserRegistrationEmailAsync` dependency noted in  
`UserManagementAsync.SendInvitationEmailAsync` (`// SYNC` annotation).  
The `SendInvitationEmailAsync` call site in `UserManagementAsync` can now be updated to call  
`_manageUserRegistrationEmail.SendNewUserRegistrationEmailAsync(profileDetail, ct)`.

---

## Registration (DI)

```csharp
// Suggested registration in Program.cs / Startup
services.AddScoped<IManageUserRegistrationEmailAsync, ManageUserRegistrationEmailAsync>();
```
