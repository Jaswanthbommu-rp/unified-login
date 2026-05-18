---
name: add-serilog-context
description: Attach the required Serilog enrichment properties (CorrelationId, ProductModule, AdditionalInfo) for a log statement in unified-login-main, and persist domain communication outcomes via IManageCommunicationEvents when applicable. Use when adding any log line in a controller, Manage{Entity}, repository, or WinService — Kibana queries tagged 'ULE' filter on these properties, so forgetting them silently hides the log from oncall. Captures the private WriteToLog helper pattern, the {ActionName} - {state} message template, and the CES/SendGrid persistence hook.
---

# Add Serilog logging with required context

Every log call in this repo must enrich `Log.Logger` with three properties so the Kibana index (tag: `ULE`) can find it:

| Property | Source | Purpose |
|---|---|---|
| `CorrelationId` | `_userClaims.CorrelationId` (logic / controllers) or `batch.CorrelationId` (WinService) or `""` if absent | Cross-request trace correlation |
| `ProductModule` | `this.GetType()` | Which class emitted the log — filterable in Kibana |
| `AdditionalInfo` | `JsonConvert.SerializeObject(logData, Formatting.Indented)` — optional | Structured payload, only when there's real data to attach |

The message template is **always** `"{ActionName} - {state}"` with two positional values (the function name and a short status). Free-form strings break the parsed-log dashboards.

## Inputs to confirm

1. **Where the log is going** — a controller, a `Manage{Entity}` logic class, a repository, or a WinService worker. Each already has a private `WriteToLog` helper; if you are adding the *first* log to a brand-new class, copy the helper from the nearest sibling.
2. **Level** — `Information` for normal flow, `Warning` for recoverable issues, `Error` for failures, `Debug` for detailed traces (only when actively investigating).
3. **`ActionName`** — the method/operation name, PascalCase (e.g., `"CreateUser"`, `"SendGridEmail"`). Stays consistent across the start/success/error logs of the same operation so they group in Kibana.
4. **State** — one short phrase: `"Started"`, `"Success"`, `"Data returned"`, `"Error while creating user"`. Avoid logging payload contents in the state string — payloads go in `AdditionalInfo`.
5. **Structured data?** — if you want to log a request DTO, response, IDs, etc., pass them as a `Dictionary<string, object>` to `logData`; do **not** stuff them into the message string.
6. **Exception** — pass through as the `exception` argument, never `ex.ToString()` in the message string.

## The helper

Every consumer class owns a private helper. Mirror [Web/Landing/BaseController.cs:86](../../Enterprise/Subsystem/ProductLauncher/Web/Landing/BaseController.cs):

```csharp
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;

private void WriteToLog(
    LogEventLevel logType,
    string message,
    Dictionary<string, object> logData = null,
    Exception exception = null,
    object[] messageProperties = null)
{
    try
    {
        string correlationId = "";
        if (_userClaims != null && _userClaims.CorrelationId != Guid.Empty)
        {
            correlationId = _userClaims.CorrelationId.ToString();
        }

        var logger = Log.Logger;
        if (logData?.Keys != null)
        {
            logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
        }
        logger = logger.ForContext("ProductModule", this.GetType());
        logger = logger.ForContext("CorrelationId", correlationId);

        logger.Write(
            level: logType,
            exception: exception,
            messageTemplate: message,
            propertyValue0: messageProperties?[0],
            propertyValue1: messageProperties?[1]);
    }
    catch
    {
        /* never let logging break the request */
    }
}
```

**Notes that match existing call sites:**

- `_userClaims` may be `null` in some controller paths — the helper must defend against that.
- The `try/catch` wrapper is intentional: logging must never throw into the request.
- `ForContext("AdditionalInfo", ..., destructureObjects: false)` is the production setting in most call sites. `ApiExceptionLogger` uses `true` because it is logging exception payload metadata that contains nested objects; default to `false` unless you have a reason.
- For WinService batches, replace `_userClaims.CorrelationId` with `batch.CorrelationId` (see [WinService/BatchProcessor/BatchProcessorService.cs:548](../../Enterprise/Subsystem/ProductLauncher/WinService/BatchProcessor/BatchProcessorService.cs)).

## Call sites

```csharp
// Plain info log
WriteToLog(LogEventLevel.Information, "{ActionName} - {state}",
    messageProperties: new object[] { "CreateUser", "Started" });

// Info with structured payload
var logData = new Dictionary<string, object>
{
    { "LoginName", request.LoginName },
    { "OrganizationId", _userClaims.OrganizationName }
};
WriteToLog(LogEventLevel.Information, "{ActionName} - {state}",
    logData: logData,
    messageProperties: new object[] { "CreateUser", "Data returned" });

// Error with exception
WriteToLog(LogEventLevel.Error, "{ActionName} - {state}",
    exception: ex,
    messageProperties: new object[] {
        "CreateUser",
        $"Error while creating new user. BooksMasterOrganizationId={_userClaims.OrganizationName}, login={request?.LoginName}"
    });
```

The `$"..."` interpolation in the state string is the standard pattern — it ends up as `propertyValue1` and is searchable in Kibana, but keep PII (passwords, full SSNs, raw tokens) out of it.

## Inline (no helper) — when

Don't. The helper is consistent across hundreds of call sites; inline `Log.Logger.ForContext(...)` chains drift quickly and miss properties. Two exceptions exist in the repo:

- [Service/SharedObjects/Exceptions/ApiExceptionLogger.cs](../../Enterprise/Subsystem/ProductLauncher/Service/SharedObjects/Exceptions/ApiExceptionLogger.cs) — global Web API exception logger (no class instance with `_userClaims`).
- [Service/SharedObjects/Base/RPObjectCache.cs](../../Enterprise/Subsystem/ProductLauncher/Service/SharedObjects/Base/RPObjectCache.cs) — base cache, runs before `DefaultUserClaim` is available.

If you need to log from such a context, follow the inline pattern exactly:

```csharp
var logger = Log.Logger
    .ForContext("AdditionalInfo", JsonConvert.SerializeObject(additionalInfo, Formatting.Indented), true)
    .ForContext("ProductModule", this.GetType())
    .ForContext("CorrelationId", correlationId);
logger.Write(LogEventLevel.Error, ex, "{ActionName} - {state}",
    propertyValue0: "ApiExceptionLogger",
    propertyValue1: $"Error : {ex.Message}");
```

## Persisting communication outcomes

When the log records the result of an **outbound communication** (email send, SMS, push, etc.), the log alone is not enough — Kibana drops events after 30 days. Persist a row via `IManageCommunicationEvents.CreateCommunicationEvent`:

```csharp
private readonly IManageCommunicationEvents _communicationEvents;

// On successful send:
var resp = _communicationEvents.CreateCommunicationEvent(
    statusTypeId: (int)EmailStatusType.EmailSuccess,
    fromPartyContactMechanismId: fromCmId,
    toPartyContactMechanismId: toCmId,
    started: sendStartedUtc,
    ended: DateTime.UtcNow,
    note: $"SendGrid messageId={messageId}");

// On failure:
_communicationEvents.CreateCommunicationEvent(
    statusTypeId: (int)EmailStatusType.EmailError,
    /* same ids */,
    started: sendStartedUtc,
    ended: DateTime.UtcNow,
    note: $"SendGrid HTTP {statusCode} - {body}");
```

See [IManageCommunicationEvents.cs](../../Enterprise/Subsystem/ProductLauncher/Component/Landing/Logic/Interfaces/IManageCommunicationEvents.cs). For SendGrid/CES/Unified Email paths, also call `CreateCommunicationEventEmail` or `CreateCESCommunicationEventEmail` with the resulting `CommunicationEventId` so the email template / CES id is linked.

Logs answer "what happened just now in Kibana." Communication events answer "did we deliver the password reset email last week?" — the latter is the audit record customer support and PME use.

## After adding — checklist

1. Open Kibana with `ProductModule:"RP.Enterprise.Subsystem.ProductLauncher.<your class>"` and your `ActionName` after deploying — confirm the log shows up with all three enrichment fields populated (not `-` / empty).
2. For email/SMS/push code paths, query `[Identity].[Comm].[CommunicationEvent]` (or equivalent) to confirm the row was persisted with the right `StatusTypeId` and `Note`.
3. If you added a new `Manage{Entity}` class without a sibling `WriteToLog`, paste the helper above as-is rather than reinventing the chain order — Kibana parsers are sensitive to property names.

## Important

- **Always** the exact property names `CorrelationId`, `ProductModule`, `AdditionalInfo`. Lowercase or alternate spellings (`correlationId`, `Module`) silently break Kibana filters.
- **Always** the `{ActionName} - {state}` template. Free-form messages don't parse into the structured columns; they end up as opaque text and dashboards miss them.
- **Never** swallow exceptions silently in business logic just because the catch contains a `WriteToLog`. Log AND rethrow (or return an error envelope) — losing the exception hides bugs.
- **Never** put secrets, passwords, or unmasked tokens into the state string or `AdditionalInfo`. Kibana is broadly readable; PCI/SOX scoping applies.
- **Never** call `Log.Logger.Write(...)` directly without enrichment. A naked `Log.Information(...)` will land in Kibana with empty `ProductModule` and `CorrelationId` and be invisible to dashboards.
- **Never** depend on the log to know whether a communication event occurred — persist via `IManageCommunicationEvents` for any outbound notification.
