---
date: 2026-05-11T00:00:00-05:00
researcher: Jaswanth Bommu
git_commit: 8b2ea3b4c49008617a163cbee35e23375a08a2ce
branch: master
repository: unified-login-main
topic: "Enterprise Roles and Primary Properties — how they work, gaps and issues"
tags: [research, codebase, enterprise-roles, primary-properties, batch-processor, role-template, multi-tenant, bugs]
status: complete
last_updated: 2026-05-11
last_updated_by: Jaswanth Bommu
---

# Research: Enterprise Roles & Primary Properties — How They Work + Gaps and Issues

**Date**: 2026-05-11
**Researcher**: Jaswanth Bommu
**Git Commit**: 8b2ea3b4c49008617a163cbee35e23375a08a2ce
**Branch**: master
**Repository**: unified-login-main

## Research Question
Explain how enterprise roles and primary properties work. Find the gaps and issues.

## Summary

**Enterprise Roles** are org-scoped role templates (`Security.RoleTemplate`) that bundle a set of products and per-product roles together; assigning one to a user (persona) writes a single mapping row and enqueues a single batch row that fans out into one `ProductBatch` per product in the template. **Primary Properties** are a per-(user × product) designation gated by a three-level toggle (org global / product global / company-product) that drives a different async batch flow but reuses the *same* orchestrator (`ManageEnterpriseRolesPrimaryProperties.ProcessEnterpriseRolesAndPrimaryPropertiesData`).

Both flows share a 3-step pipeline:
1. **Synchronous write** (inside the user-save transaction): mapping rows + insert into a dedicated batch table (`Batch.EnterpriseRoleBatchProcess` or `Batch.PrimaryPropertiesBatchProcess`).
2. **Polling loop** in the `BatchProcessor` Windows service (`RunEnterpriseRoleUpdateProcess` / `RunPrimaryPropertiesUpdateProcess`) picks up rows in status `Waiting=5`, marks them `Running=6`, POSTs to a dedicated API route (`/erpbatchprocessor` / `/ppbatchprocessor`).
3. **The orchestrator** computes a product diff (using temporal `FOR SYSTEM_TIME AS OF` queries against `RoleTemplateProduct*` shadow tables) and inserts one `Batch.ProductBatch` row per affected product — which is then processed by the **separate** `RunPendingProcess` loop via the `ProcessExecutionFactory` → `EnterpriseCreateUpdateProductUser` → `ManageProductUser.CreateEnterpriseRoleProductUser`.

**The system works in the happy path. The gaps are concentrated in three places:** (a) the *dequeue side* of the deactivated-user fix landed only on the enqueue side; (b) the dedicated polling loops silently drop non-2xx HTTP responses and have no batch-level rollback in their outer catch, leaving rows stuck in `Running` forever; (c) downstream product orchestrators (`ManageProductResidentPortal`, etc.) make unguarded `RoleList[0]` / `PropertyList[0]` access — the single largest source of production errors (6,475 occurrences for ResidentPortal alone per the 2026-05-05 legacy errors analysis).

## Detailed Findings

### A. Enterprise Role — Data Model

All tables in `Database/Identity/RoleRightsManagement/Tables/` unless noted. Every table is system-versioned (temporal) with a paired `*History` shadow table.

| Table | Key columns | Purpose |
|---|---|---|
| `Security.RoleTemplate` | `RoleTemplateId` PK, `PartyID` FK → `Enterprise.Organization`, `RoleTemplateName`, `RoleType` default `'Custom'`, `RoleTemplateNotification` NVARCHAR(MAX) JSON | The root entity. Org-scoped. |
| `Security.RoleTemplateProduct` | `RoleTemplateProductId` PK, `RoleTemplateId` FK, `ProductId` FK → `Enterprise.Product` | Products included in the template. |
| `Security.RoleTemplateProductRoleMapping` | `RoleTemplateProductId` FK, `ProductRoleId` VARCHAR(100), `ProductRoleName` VARCHAR(510) | External per-product role IDs (e.g., the OneSite role string). |
| `Security.RoleTemplateAdditionalProductRoleMapping` | `RoleTemplateProductId` FK, `AttributeName`, `AttributeValue` | Key/value attributes per product (e.g., `hasAccessToSiteSpendManagementOnly` for OneSiteAccounting). |
| `Security.RoleTemplateProductAdditionalTab` | `RoleTemplateId` FK, `TabJson` NVARCHAR(MAX) | Serialized tab-layout config. |
| `Security.RoleTemplateUserMapping` | `RoleTemplateId` FK, `PersonaId` FK → `Person.Persona` | **A persona holds at most one role template at a time** — upsert by `PersonaId`, reassignment replaces. |
| `Batch.EnterpriseRoleBatchProcess` (`Database/Identity/Batch/Table/EnterpriseRoleBatchProcess.sql:1`) | `EnterpriseRoleBatchProcessId` PK, `EditorUserPersonaId`, `SubjectUserPersonaId`, `EnterpriseRoleTemplateId`, `BatchProcessTypeId` (hardcoded `15` at enqueue), `StatusTypeId` (5=Waiting, 6=Running), `UseAPIV2` BIT default 0 | Async propagation queue. |

**Key SPs (Identity DB):**
- Enqueue: `Security.InsertUpdateRoleTemplateAndInsertEnterpriseRoleBatch` (MERGE on `RoleTemplateUserMapping` + insert into `EnterpriseRoleBatchProcess`), `Security.InsertBatchPersonaIdToEnterpriseRoleBatchProcess` (TVP-based bulk enqueue without touching mapping).
- Read: `Security.GetEnterpriseRoleNewProductsByRoleTemplateId`, `GetEnterpriseRoleUpdatedProductsByRoleTemplateId`, `GetEnterpriseRoleDeletedProductsByRoleTemplateId` — all three use **temporal table syntax** (`FOR SYSTEM_TIME AS OF DATEADD(SECOND, -5, GETUTCDATE())`) against `RoleTemplateProduct*` to compute a 5-second-window product diff.
- Batch processor: `Batch.ListEnterpriseRoleBatchProcessor` (status=5, dedupe by `(SubjectUserPersonaId, EnterpriseRoleTemplateId)` and cap 5/editor, mark Running atomically), `Batch.UpdateEnterpriseRoleProductBatch` (set status by ID).

**SP-name constants in `StoredProcNameConstants.cs`:** lines 385–395 (`SP_GetRoleTemplateProductRoleMappings`, the three diff SPs, `SP_GetUserRoleTemplate`, `SP_GetEnterpriseDelegateRole`, etc.) plus lines 313–314 (`SP_InsertUpdateRoleTemplateUserMapping`, `SP_UnassignEnterpriseRoleFromUser`).

### B. Primary Properties — Data Model

Primary Properties are not a single table — they are a per-(persona, product) status governed by three layered settings and persisted in a staging table.

| Object | File | Purpose |
|---|---|---|
| `Batch.PrimaryPropertiesBatchProcess` | `Database/Identity/Batch/Table/PrimaryPropertiesBatchProcess.sql:1-15` | Async queue. Columns: `PrimaryPropertyBatchProcessId` PK, `EditorUserPersonaId`, `SubjectUserPersonaId`, `BatchProcessTypeId`, `StatusTypeId`, `CreatedDateTime`, `CompletedDateTime`, `UseAPIV2` BIT default 0. |
| `Enterprise.UserSyncProductPrimaryPropertiesStaging` | `Database/Identity/Enterprise/Tables/UserSyncProductPrimaryPropertiesStaging.sql` | Staging table for resolved primary properties per user/product. |
| `Batch.ListPrimaryPropertiesBatchProcessor` | `Database/Identity/Batch/Procedure/ListPrimaryPropertiesBatchProcessor.sql:1-69` | Dequeue: status=5, within 3 days, dedupe by `SubjectUserPersonaId`, cap 5/editor, mark Running. |
| `Batch.UpdatePrimaryPropertiesProductBatch` | `Database/Identity/Batch/Procedure/UpdatePrimaryPropertiesProductBatch.sql` | Status write-back. |
| `Batch.InsertBatchPersonaIdToPrimaryPropertiesBatchProcess` | `Database/Identity/Batch/Procedure/...` | TVP-based bulk enqueue. |
| `Enterprise.AddPersonaProductMatchedPrimaryProperties` / `DeletePersonaProductMatchedPrimaryProperties` / `GetPersonaProductPrimaryProperties` | `Database/Identity/Enterprise/Stored Procedures/` | CRUD on staged primary properties. |
| `Enterprise.CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSetting` | `Database/Identity/Enterprise/Stored Procedures/` | Writes the per-(company, product) "use primary properties" master setting row. |
| `Enterprise.CreateUsePrimaryPropertyMasterConfigurationSetting` | same folder | The org-global toggle. |

**SP-name constants:** `StoredProcNameConstants.cs:180` (`SP_GetPersonaProductPrimaryProperties`), `:273` (`SP_UpdatePrimaryPropertyProductBatch`), `:326` (`SP_CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSetting`), `:412` (`SP_AddPersonaProductMatchedPrimaryProperties`), `:414` (`SP_DeletePersonaProductMatchedPrimaryProperties`).

**What makes a property "primary":** it is a *capability mode* — when all three toggles below are `1`, the orchestrator treats the user's persona-product property list as the "primary property set" for that user, and the product is provisioned only for that subset:

1. **Org-global** (`UnifiedSettings.PrimaryProperty` configuration setting)
2. **Product-global** (`ProductInternalSetting.PrimaryProperty` per product)
3. **Company-product** (`Enterprise.CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSetting` row per (org, product))

All three must equal `1` for `GetPrimaryPropertySettingsForCompanyAndProduct` (`ManagePrimaryPropertiesBatch.cs:79-103`) to return `true`. `UnifiedPlatform` (UPFM) is hardcoded to always force `usePrimaryProperties=true`.

### C. Orchestration — The Three Modes of `ProcessEnterpriseRolesAndPrimaryPropertiesData`

The single method `ManageEnterpriseRolesPrimaryProperties.ProcessEnterpriseRolesAndPrimaryPropertiesData` (`Component/Landing/Logic/ManageEnterpriseRolesPrimaryProperties.cs:94`) is the unified core that drives both flows. It branches into three modes:

**Mode A — Bulk add/update enterprise role** (`batchProcessTypeId == BulkAddUpdateEnterpriseRole = 15`, line 114):
- Called *twice* by `ManageEnterpriseRoleProductBatch.GenerateEnterpriseRoleUserProductBatch` (`ManageEnterpriseRoleProductBatch.cs:69-76`) — first with `isUnassignAllProducts=true` (strip existing products, hardcoded excluding `UnifiedPlatform` and `AssetOptimizer`, optionally `AdminSupportPortal`), then with `false` (apply template products from `SP_GetRoleTemplateProductRoleMappings`, force-include `UnifiedPlatform`).

**Mode B — Single-persona enterprise role** (`enterpriseRoleTemplateId != null`, line 145):
- Computes a product diff via the three temporal SPs (`GetEnterpriseRoleNew/Updated/DeletedProductsByRoleTemplateId`).
- New + updated products are merged into `roleTemplateNewProducts` (line 151).

**Mode C — Primary properties only** (`enterpriseRoleTemplateId == null`, line 162):
- Gets user's existing products via `ListProductsByPersonaId`.
- **Then calls `GetEnterpriseRoleForPersona(subjectUserPersonaId)`** and merges that template's roles into the working set — meaning a primary-properties batch CAN re-trigger enterprise-role role-mapping side effects.

**Per-product loop (line 212):** For each product, compute `usePrimaryProperties` flag, fetch roles, build a `ProductBatch`. Special-cases for `UnifiedPlatform` (synchronous `Security.LinkPersonaToRole` delete-then-insert at line 352), AO family (bundled via `BatchHelper.CreateAoBatchRecords`), `AdminSupportPortal` (real properties), `RealConnect` (license merge), `OneSiteAccounting product 8` (read three template attributes), `KnockCRM` (property groups), and UPFM (compute `propertiesToRemove` as diff vs. UnifiedUI properties).

**Persistence (line 606):** `_enterpriseRoleProductRepository.SaveProductBatch(...)` opens a group (`Batch.CreateBatchProcessorGroup`), then `Batch.CreateProductBatch` per product, with `BatchProcessTypeId = EnterpriseRoleCreateUpdateProductUser = 10` for assigned products. These `ProductBatch` rows are then picked up by the **normal pending loop** (`RunPendingProcess`), not the enterprise-role loop.

### D. End-to-End Flow

```
[UI: Save user]
  │
  ▼
UserController/UserRepository (sync, transactional)
  ├── Security.InsertUpdateRoleTemplateUserMapping  (writes RoleTemplateUserMapping row)
  └── (optional) inserts row into Batch.EnterpriseRoleBatchProcess via InsertUpdate...EnterpriseRoleBatch SP
                                            │
[BatchProcessor.RunEnterpriseRoleUpdateProcess (Windows service, polls every PollingInterval s)]
  │
  ▼
SP Batch.ListEnterpriseRoleBatchProcessor (status=5, mark 6)  →  rows
  │
  ▼ Parallel.ForEach (MaxDegreeOfParallelism = BatchProcessorEnterpriseRoleThread)
HTTP POST /erpbatchprocessor (config endpoint, no timeout, per-request HttpClient)
  │
  ▼
BatchProcessController.EnterpriseRoleProductProcessBatch  ([AllowAnonymous])
  │
  ▼
ManageEnterpriseRoleProductBatch.GenerateEnterpriseRoleUserProductBatch
  │
  ▼ Mode A (bulk) or Mode B (single)
ManageEnterpriseRolesPrimaryProperties.ProcessEnterpriseRolesAndPrimaryPropertiesData
  │  computes new/updated/deleted products via temporal SPs
  │  builds one ProductBatch per product (excluding UnifiedPlatform — that's synchronous)
  │
  ▼
BatchProductBulkUpdateRepository.SaveProductBatch
  ├── Batch.CreateBatchProcessorGroup
  └── for each product:  Batch.CreateProductBatch  (status=5, type=10 EnterpriseRoleCreateUpdateProductUser)
  │
  ▼
[BatchProcessor.RunPendingProcess polls Batch.ListBatchProcessor]
  │
  ▼ ProcessExecutionFactory → EnterpriseCreateUpdateProductUser
ManageProductUser.CreateEnterpriseRoleProductUser  →  per-product HTTP calls (OneSite, AO, MC, etc.)
  │
  ▼
Status write-back:
  - ProductBatch row    → Batch.UpdateProductBatch          (set Success/Error)
  - Parent ER row        → Batch.UpdateEnterpriseRoleProductBatch (set Success/Error, in /erpbatchprocessor catch)
```

Primary properties follow the same shape but enter via `Batch.PrimaryPropertiesBatchProcess` → `RunPrimaryPropertiesUpdateProcess` → `/ppbatchprocessor` → `ManagePrimaryPropertiesBatch.GeneratePrimaryPropertiesUserProductBatch` → Mode C of the same orchestrator.

### E. Recent Commits Touching This Area

From `git log` (last 40 commits):

| Commit | Title | Relevance |
|---|---|---|
| `26b2c5c18` | Bug 2811407: Enterprise Role Updates Propagate to Deactivated Users | Added `ulp.StatusTypeId NOT IN (19,24)` filter to enqueue SPs |
| `da9002713` | Update user with Null or empty batch data | Defensive null handling on batch DTO |
| `263a1f39b` | Remove Enterprise PersonaSuggested Properties and Referenced SPs | Cleanup of an older suggested-property mechanism |
| `4caf9496a` | Removing enterprise.PersonaSuggestedProperties table and references | Same cleanup at DB level |
| `659d35821` | StandardV1ProductIntegration BatchProcessor error fixes | Generic integration-type fixes |
| `8ba392c51` | Add null checks for productBatch to prevent exceptions | Per-product null hardening |
| `34e94e0bb` | Refactor property assignment to user with transactions | Property-assignment transactional rewrite |
| `74e144bfc` | 2767076: Product unassigned when enterprise role updated | Past production bug — enterprise role update incorrectly unassigning products |
| `829493956` | Update PROC: [Batch].[UpdateProductBatch] to add 10ms delay | Throughput tuning; signals contention on UpdateProductBatch |
| `3f268a7a3` | 2718529: property group settings configuration | Property-group settings work |
| `b32033633` | 2718529: Script changes | Same work item |
| `b1899219` | Bug 2780867: Unable to See Roles Assigned to Rights in Financial Suite | Role-rights display bug |

`26b2c5c18` is the most directly relevant — but **the fix only landed on the enqueue side**, see Gap #1 below.

---

## Gaps and Issues

### G1. Bug 2811407 fix is dequeue-blind — race window remains

The fix to "Enterprise Role Updates Propagate to Deactivated Users" added `ulp.StatusTypeId NOT IN (19,24)` to `InsertUpdateRoleTemplateAndInsertEnterpriseRoleBatch.sql:28` and `InsertBatchPersonaIdToEnterpriseRoleBatchProcess.sql:16`. But the dequeue SPs `ListEnterpriseRoleBatchProcessor.sql:31` and `ListPrimaryPropertiesBatchProcessor.sql:29` filter only by `StatusTypeID = 5` (batch row status, not user status) and a 3-day window. They do **not** re-check persona status. The orchestrator `ManageEnterpriseRoleProductBatch.GenerateEnterpriseRoleUserProductBatch` (`ManageEnterpriseRoleProductBatch.cs:61-65`) calls `_managePersona.GetPersona` on both editor and subject unconditionally — no status guard.

**Race:** enqueue at T0 (user active) → user deactivated at T1 → dequeue at T2 → product provisioning fires on a deactivated user.

**Fix:** add the same `ulp.StatusTypeId NOT IN (19,24)` filter inside the two `List*BatchProcessor` SPs (or add a status check at the top of `GenerateEnterpriseRoleUserProductBatch`).

### G2. Non-2xx HTTP response silently dropped — rows stuck in `Running`

`Helper/ApiCaller.cs:11-27` returns `null` on non-success; it doesn't throw. The normal loop's `CallApiToProcessBatchRecord` checks for null and throws (lines 570-573), causing the outer catch to write `BatchStatusType.Error`. But **the enterprise-role and primary-properties analogs lack this null check**:

- `BatchProcessorService.CallApiToProcessEnterpriseRoleBatchRecord` (lines 598-646) — no null check on the `result` from `ProductApiCaller.ProcessEnterpriseRoleBatchRecord`.
- `BatchProcessorService.CallApiToProcessPrimaryPropertiesBatchRecord` (lines 648-693) — same.

A 4xx or 5xx from `/erpbatchprocessor` produces a null result, gets logged at `Information` level, and the parent batch row remains at `StatusTypeId=6 (Running)` indefinitely. `ListEnterpriseRoleBatchProcessor` only picks up status=5 rows, so the stuck row will never be retried.

**Fix:** add the same null-check-and-throw at the call sites; let the outer catch write `BatchStatusType.Error`.

### G3. Loop-level outer catch has no bulk rollback

`RunEnterpriseRoleUpdateProcess` catch (`BatchProcessorService.cs:384-388`) and `RunPrimaryPropertiesUpdateProcess` catch (`:446-450`) log + sleep. The normal loop's catch (`:242-252`) calls `new BatchRepository().UpdateBatch(batch, BatchStatusType.Error, ...)` to roll back every record in the dequeued batch. If an exception fires between dequeue (which marks records Running) and per-record dispatch — say, in `GetProductInternalSettings` at line 367 — the in-memory `batch` list is lost and the records remain in `Running` with no rollback.

**Fix:** mirror the normal loop's catch — iterate the dequeued batch and mark each record Error.

### G4. `RunRetryProcess` does not cover the dedicated tables

`RunRetryProcess` (`BatchProcessorService.cs:129-189`) calls `GetBatchToProcess(BatchSize, isRetry=true)` — SP `Batch.ListBatchProcessor` against the `Batch.ProductBatch` table. It does not touch `Batch.EnterpriseRoleBatchProcess` or `Batch.PrimaryPropertiesBatchProcess`. Rows in those tables that end up in `Error` (status 7) have **no automatic retry mechanism** in this service. The Core API v2 service (gated by `use-core-api-v2-for-service`) may or may not provide one — that's TBD per the related migration.

### G5. Partial-success path returns "success" without setting parent row to Success

`ManageEnterpriseRoleProductBatch.GenerateEnterpriseRoleUserProductBatch` (`ManageEnterpriseRoleProductBatch.cs:80-92`): if `ProcessEnterpriseRolesAndPrimaryPropertiesData` returns a non-empty string (indicating a per-product failure), the code falls through to `return ""` at line 92. The caller in the WinService treats `""` as success. But `UpdateEnterpriseRoleProductBatch(..., Success)` only fires inside the success branch at line 83. **Result:** the parent batch row stays in `Running` forever even though the WinService thinks the call succeeded. No rollback to Error.

**Fix:** in the partial-fail path, call `UpdateEnterpriseRoleProductBatch(..., Error)` and return `"Error"`.

### G6. Magic-number drift between SQL and C#

`InsertUpdateRoleTemplateAndInsertEnterpriseRoleBatch.sql` hardcodes `BatchProcessTypeId = 15` for the inserted batch row. The C# code reads `BatchProcessType.BulkAddUpdateEnterpriseRole = 15` from the enum (`Service/SharedObjects/Enum/BatchProcessType.cs`). The two are independent and could drift if the enum is renumbered.

### G7. Three-way primary-property toggle is uncovered and fragile

`ManagePrimaryPropertiesBatch.GetPrimaryPropertySettingsForCompanyAndProduct` (`:79-103`) requires *all three* of `organizationUsePrimaryProperties`, `productUsePrimaryPropertiesGlobal`, `companyProductUsePrimaryPropertySetting` to be `1`. No test exercises this method. The "primary properties on but no translation table" production failure (Legacy-Errors-Analysis row 9, 168 occurrences of "No active properties found") originates here: the toggle was flipped at org+product+company level but the property→community translation rows were never seeded, leaving `_residentPortalUser` with zero active properties.

### G8. Unbounded `RoleList[0]` / `PropertyList[0]` access — the largest production error class

Grep across `Component/Landing/Logic/Product/Manage*.cs` found **38 hits** for `RoleList[0]` or `PropertyList[0]`. Some are guarded by `.Count > 0`; many are not. The largest offender is `ManageProductResidentPortal.cs` — accountable for **6,475 NRE occurrences (~47% of all production errors)** per `Legacy-Errors-Analysis-05.05.2026.md`. Specific unguarded or partially-guarded sites:

- `ManageProductResidentPortal.cs:466` — `residentPortal.RoleList[0].ToUpper().StartsWith("ENTERPRISE")` — guards `Count == 1` but **not** `Count > 0`; a `Count == 0` short-circuits via the outer `||`, but a `Count == 2` skips the guard yet still hits `[0]` — safe but brittle.
- `ManageProductResidentPortal.cs:506`, `:508`, `:593`, `:633`, `:1323` — all access `RoleList[0]` after a `Count > 0` guard buried in a conditional; readability hazard, easy to regress.
- `ManageProductResidentPortal.cs:1929` — `roleproperty.PropertyList.Count == 1 && roleproperty.PropertyList[0] == "all"` — safe.
- `ManageProductMarketingCenter.cs:616, 619, 623, 624, 625` — multiple `RoleList[0]` accesses; the preceding flow likely guards but the indexer access is unconditional.
- `ManageProductRum.cs:807`, `ManageProductProspectContact.cs:392`, `ManageUPFMProductsIntegration.cs:804` — `PropertyList[0].ToUpper() == "ALL"` without an explicit empty-list guard at the call site.
- `ManageIntelligentBuilding.cs:142, 185`, `ManageUnifiedAmenities.cs:134, 177`, `ManageResearchApplication.cs:239` — `RoleList[0]` access in role-mapping construction.
- `ManageProductOps.cs:848, 854, 870, 876, 913, 920` — six sites with `RoleList[0].ToString()` / `PropertyList[0].ToString()`.
- `ManageProductRentersInsurance.cs:631, 641, 681` — partially guarded with ternaries.
- `ManageProductAdminSupportPortal.cs:303-306, 411, 426` and `ManageProductClientPortal.cs:293-296, 407` — `[0]` access guarded by `Count > 0` AND length checks (relatively safe).
- `ManageProductOneSiteAccounting.cs:1689`, `ManageProductUser.cs:327` — guarded with `Count > 0`.

The Legacy-Errors-Analysis recommends hardening `ManageProductResidentPortal` to short-circuit with a clean `ErrorReason` — that recommendation also applies to the unguarded sites in `ManageProductMarketingCenter`, `ManageProductRum`, `ManageProductProspectContact`, `ManageUPFMProductsIntegration`, and `ManageProductOps`.

### G9. Test coverage gaps

| File | Coverage |
|---|---|
| `ManageEnterpriseRoleProductBatch.cs` | **No test file exists.** The `BulkAddUpdateEnterpriseRole` double-call pattern, `GetSelectedProperties` (7 property-type cases), and `GetProductRoleList` are all untested. |
| `ManagePrimaryPropertiesBatch.cs` | **No test file exists.** `GeneratePrimaryPropertiesUserProductBatch` and the three-way toggle method are untested. |
| `ManageEnterpriseRolesPrimaryPropertiesTest.cs` | **13 tests exist, but all assert only SP-invocation names.** No assertion on what product IDs / role IDs / property IDs are actually passed to `Batch.CreateProductBatch`. The helper `GetroleTemplateProductRoleList()` at line 1179 returns an **empty list** — the role-mapping loop is never exercised against real role data. |
| Single-persona enterprise-role path (Mode B, `enterpriseRoleTemplateId != null` + `batchProcessTypeId == 0`, `ManageEnterpriseRolesPrimaryProperties.cs:145-159`) | Not covered by any of the 13 tests; all tests use `batchProcessTypeId = 15`. |
| Impersonation branch (`_userClaim.ImpersonatedBy != Guid.Empty`, `ManageEnterpriseRolesPrimaryProperties.cs:196-199`) | No test. |
| `EnterpriseUserRolesTests.cs` | Covers `RoleController.GetProductRoles`, `GetUserProductRoles`, `GetRightsforRole` (13 tests). No coverage of the enqueue path or the Bug 2811407 deactivated-user guard. |

### G10. Dead diagnostic code in production

`ManageEnterpriseRolesPrimaryProperties.cs:188`:
```csharp
// Kept this for only for logs, Will remove this logic once testing is done,
// Start
```
…with matching `// End.` at line 192. The bracketed code builds `newproducts`/`updateproducts`/`deletedProducts` strings purely for log output. It's still in production. The "testing" referenced has presumably either completed (cleanup forgotten) or stalled.

`ManageEnterpriseRoleProductBatch.cs:189-191` contains commented-out variable declarations from a prior rename — should be deleted.

### G11. Hardcoded product exclusions are implicit business rules

In `ProcessEnterpriseRolesAndPrimaryPropertiesData` Mode A `isUnassignAllProducts=true` (lines 114-129):
- Always strips `UnifiedPlatform` and `AssetOptimizer` from the "products to unassign" list.
- Conditionally strips `AdminSupportPortal` (only if no SAML attributes are set).

These are undocumented and untested as independent rules. A future product addition that should NOT be unassigned during bulk enterprise-role replacement will silently be unassigned unless someone adds it to this list.

### G12. Mode C silently re-applies the user's current role template

`ProcessEnterpriseRolesAndPrimaryPropertiesData` Mode C (lines 162-181) — when called for a *primary properties only* batch — still calls `GetEnterpriseRoleForPersona(subjectUserPersonaId)` and merges the user's current template's roles into the working product set. **A primary-properties change therefore triggers role-mapping side effects.** This is not obvious from the method name `GeneratePrimaryPropertiesUserProductBatch` and is not tested explicitly.

### G13. `UseAPIV2` migration flag — two implementations alive simultaneously

Both `Batch.EnterpriseRoleBatchProcess.UseAPIV2` and `Batch.PrimaryPropertiesBatchProcess.UseAPIV2` default to 0, and both `ListEnterpriseRoleBatchProcessor` and `ListPrimaryPropertiesBatchProcessor` accept `@UseAPIV2 BIT = 0` as a parameter. The BatchProcessor in this repo calls the SP with `UseAPIV2 = 0`, so rows enqueued with `UseAPIV2 = 1` will be invisible to this service — presumably picked up by a future Core API v2 service. Until that flips, this is dead infrastructure. Once it flips, the four LD-gated loops (`use-core-api-v2-for-service`) plus the `UseAPIV2` column gating represents **two parallel implementations of the same flow** running side by side — a long-lived technical debt.

### G14. Shared `ThreadCount` field across 6 loops (from `BatchProcessor-Bottlenecks.md` §8)

`private int ThreadCount` in `BatchProcessorService.cs:29` is reassigned by the enterprise-role loop at `:367` and the primary-properties loop at `:430` from different `ProductInternalSetting` keys (`BatchProcessorEnterpriseRoleThread`, `BatchProcessorPrimaryPropertiesThread`). The two loops can overwrite each other mid-cycle, so the `Parallel.ForEach` `MaxDegreeOfParallelism` becomes whichever loop wrote last. Not a correctness bug (each loop reads-then-uses), but the parallelism is not what the config implies.

### G15. Per-request `HttpClient` in the WinService (from `BatchProcessor-Bottlenecks.md` §1)

`ApiCaller.cs:11-27` does `using (var client = new HttpClient())` per call. Combined with no timeout (default 100 s) and `Parallel.ForEach` blocking on `.Result`, this is the canonical socket-exhaustion anti-pattern. Affects all 6 loops including enterprise-role and primary-properties.

### G16. Stuck-in-`Running` cleanup missing

There is no cleanup job that resets rows in `EnterpriseRoleBatchProcess` or `PrimaryPropertiesBatchProcess` from `Running=6` back to `Waiting=5` after a service crash or unhandled exception. Combined with G2, G3, and G5, this means any record that fails to write back gets parked forever. The 3-day filter in the `List…BatchProcessor` SPs (`createddatetime > dateadd(dd, -3, getutcdate())`) at least bounds the staleness blast radius — but stuck records become silently orphaned after 3 days.

---

## Code References

### Data model
- `Database/Identity/RoleRightsManagement/Tables/RoleTemplate.sql:1` — root entity
- `Database/Identity/RoleRightsManagement/Tables/RoleTemplateUserMapping.sql:1` — persona → template (1:1)
- `Database/Identity/Batch/Table/EnterpriseRoleBatchProcess.sql:1` — async queue
- `Database/Identity/Batch/Table/PrimaryPropertiesBatchProcess.sql:1-15` — async queue (primary props)
- `Database/Identity/Enterprise/Tables/UserSyncProductPrimaryPropertiesStaging.sql` — staged primary properties

### Enqueue / read SPs
- `Database/Identity/RoleRightsManagement/Stored Procedures/InsertUpdateRoleTemplateAndInsertEnterpriseRoleBatch.sql:28` — `ulp.StatusTypeId NOT IN (19,24)` (Bug 2811407 fix)
- `Database/Identity/RoleRightsManagement/Stored Procedures/InsertBatchPersonaIdToEnterpriseRoleBatchProcess.sql:16` — same filter
- `Database/Identity/Batch/Procedure/ListEnterpriseRoleBatchProcessor.sql:31` — **missing** the persona-status filter
- `Database/Identity/Batch/Procedure/ListPrimaryPropertiesBatchProcessor.sql:29` — **missing** the persona-status filter
- `Database/Identity/RoleRightsManagement/Stored Procedures/GetEnterpriseRoleNewProductsByRoleTemplateId.sql` (and Updated, Deleted variants) — temporal diff via `FOR SYSTEM_TIME AS OF DATEADD(SECOND, -5, GETUTCDATE())`

### Orchestration
- `Component/Landing/Logic/ManageEnterpriseRoleProductBatch.cs:56-93` — WinService entry for ER batches; partial-success gap at `:80-92`
- `Component/Landing/Logic/ManagePrimaryPropertiesBatch.cs:54-77` — WinService entry for PP batches
- `Component/Landing/Logic/ManageEnterpriseRolesPrimaryProperties.cs:94` — unified core (Modes A/B/C)
- `Component/Landing/Logic/ManageEnterpriseRolesPrimaryProperties.cs:114-181` — Mode A/B/C branching
- `Component/Landing/Logic/ManageEnterpriseRolesPrimaryProperties.cs:188-192` — dead diagnostic code
- `Component/Landing/Logic/ManageEnterpriseRolesPrimaryProperties.cs:352-380` — synchronous `Security.LinkPersonaToRole` for UPFM
- `Component/Landing/Logic/ManageEnterpriseRolesPrimaryProperties.cs:603` — `BundleAoProducts`
- `Component/Landing/Logic/ManageEnterpriseRolesPrimaryProperties.cs:606` — `SaveProductBatch`

### BatchProcessor WinService
- `WinService/BatchProcessor/BatchProcessorService.cs:331-389` — `RunEnterpriseRoleUpdateProcess`
- `WinService/BatchProcessor/BatchProcessorService.cs:395-452` — `RunPrimaryPropertiesUpdateProcess`
- `WinService/BatchProcessor/BatchProcessorService.cs:598-646` — `CallApiToProcessEnterpriseRoleBatchRecord` (no null check)
- `WinService/BatchProcessor/BatchProcessorService.cs:648-693` — `CallApiToProcessPrimaryPropertiesBatchRecord` (no null check)
- `WinService/BatchProcessor/BatchProcessorService.cs:384-388, 446-450` — catch with no bulk rollback
- `WinService/BatchProcessor/Helper/ApiCaller.cs:11-27` — per-request `HttpClient`, no timeout
- `WinService/BatchProcessor/Repository/ProductBatchRepository.cs:46-119` — list + update methods

### Controllers / receiving endpoints
- `Service/LandingAPI/Controllers/BatchProcessController.cs:56-71` — `POST /erpbatchprocessor` `[AllowAnonymous]`
- `Service/LandingAPI/Controllers/BatchProcessController.cs:77-95` — `POST /ppbatchprocessor` `[AllowAnonymous]`
- `Service/LandingAPIEnterprise/Controllers/RoleController.cs` — UI-facing role queries (read-only)
- `Service/LandingAPIEnterprise/Controllers/PropertyController.cs` — UI-facing property queries

### Per-product NRE risk sites (highest-impact subset)
- `Component/Landing/Logic/Product/ManageProductResidentPortal.cs:466, 506, 508, 593, 633, 1323, 1929`
- `Component/Landing/Logic/Product/ManageProductMarketingCenter.cs:616, 619, 623-625`
- `Component/Landing/Logic/Product/ManageProductOps.cs:848, 854, 870, 876, 913, 920`
- `Component/Landing/Logic/Product/ManageIntelligentBuilding.cs:142, 185`
- `Component/Landing/Logic/Product/ManageProductProspectContact.cs:392`
- `Component/Landing/Logic/Product/ManageUPFMProductsIntegration.cs:804`
- `Component/Landing/Logic/Product/ManageProductRentersInsurance.cs:631, 641, 681`

### Tests
- `Service/LandingAPI/LandingAPI.Test/Logic/ManageEnterpriseRolesPrimaryPropertiesTest.cs` — 13 tests, all SP-invocation assertions
- `Service/LandingAPI/LandingAPI.Test/Logic/ManageEnterpriseRolesPrimaryPropertiesTest.cs:1179` — `GetroleTemplateProductRoleList()` returns empty list

### Constants
- `Service/SharedObjects/Constants/StoredProcNameConstants.cs:180, 273, 313-314, 326, 385-395, 412, 414` — all relevant SP constants
- `Service/SharedObjects/Enum/BatchProcessType.cs` — `BulkAddUpdateEnterpriseRole = 15`, `EnterpriseRoleCreateUpdateProductUser = 10`, `PrimaryPropertiesUpdateProductUser = 14`
- `Service/SharedObjects/Enum/ProductBatchStatusType.cs` — `Waiting=5, Running=6, Error=7, Success=8`

---

## Architecture Insights

1. **Two-stage batching, one orchestrator.** The system batches *twice*: once into the dedicated `EnterpriseRoleBatchProcess`/`PrimaryPropertiesBatchProcess` tables, then a second time into the per-product `ProductBatch` table. The first stage exists so that a single user-save fans into N product writes off the request thread; the second stage exists so the existing pending-loop infrastructure handles the per-product API calls. The orchestrator (`ManageEnterpriseRolesPrimaryProperties`) is shared between flows via three behavioral modes — clever, but it creates surprising coupling where a primary-properties change can pull in the user's current role template.

2. **Temporal tables drive the product diff.** Rather than persist a "what changed" delta, the system uses SQL Server temporal versioning on `RoleTemplateProduct*` and computes the diff at processing time via `FOR SYSTEM_TIME AS OF DATEADD(SECOND, -5, GETUTCDATE())`. This is elegant but has a hidden timing assumption: the batch must be processed within ~5 seconds of the template edit. If the WinService is offline for longer than the polling interval, the 5-second window may miss the change.

3. **Status writeback is the weakest link.** Three of the 16 gaps (G2, G3, G5, plus G16) involve the same failure mode: a row gets dequeued and marked `Running`, then something prevents the success or error write-back, and the row sits in `Running` forever. The 3-day dequeue filter is the only safeguard, and it just hides the rows after 3 days rather than recovering them.

4. **Two implementations live side-by-side.** The `UseAPIV2` column on both batch tables and the `use-core-api-v2-for-service` LD flag together describe a long-lived migration: the .NET 4.8 BatchProcessor processes `UseAPIV2=0` rows; the (presumably) Core API v2 service in `unified-login-coreapi` will process `UseAPIV2=1` rows. Until cutover, dead branches and duplicate paths exist in both repos.

5. **Per-product NRE risk is the dominant production error class.** The Legacy-Errors-Analysis already established this — 6,475/16,781 errors (~39%) trace to a single file's unguarded indexer access. The architectural root cause is that `RoleList` / `PropertyList` are `IList<string>` shapes with no schema-level guarantee of `Count >= 1`; the contract is implicit and the legacy UI doesn't enforce it. The right structural fix is to introduce a typed `RoleAssignment` value object that can't be constructed without at least one role and property.

---

## Historical Context (from thoughts/)

The `thoughts/` directory was empty when previously researched. The root-level analysis docs are the only prior artifacts:

- `BatchProcessor-Bottlenecks.md` — performance + concurrency issues in the WinService. Several items here (#1 HttpClient lifecycle, #3 sync-over-async, #8 shared `ThreadCount`, #10 missing cancellation token) directly amplify the gaps documented above.
- `Legacy-Errors-Analysis-05.05.2026.md` — 16,781 production errors analyzed; row 1 (6,475 ResidentPortal NREs) and row 9 ("No active properties found", 168 occurrences) are *directly* about the gaps in this research (G7, G8).

Both docs are already cross-referenced in this report.

---

## Related Research

- `thoughts/shared/research/2026-05-11-unified-login-architecture.md` — system-wide architecture overview; this document drills into the enterprise-role and primary-property subsystems specifically.

---

## Open Questions

1. **Status reconciliation policy.** Is there a manual or scheduled job (SSIS package, SQL Agent job, or DB job) that resets stuck-`Running` rows in `EnterpriseRoleBatchProcess` and `PrimaryPropertiesBatchProcess`? If not, what's the operational pattern when batches get stuck?

2. **Core API v2 migration scope.** Once `use-core-api-v2-for-service` flips on permanently and `UseAPIV2=1` rows start flowing — does the .NET 4.8 BatchProcessor get decommissioned, or is it kept as a fallback? If kept, the two-implementation drift gets worse over time.

3. **Why hardcoded UnifiedPlatform / AssetOptimizer / AdminSupportPortal exclusion in bulk unassign?** Is this a business rule (these products must not be auto-unassigned during enterprise role replacement) or a workaround for a bug in those product orchestrators? Documenting the original reasoning would prevent future regressions.

4. **5-second temporal diff window — is it always sufficient?** Under load (BatchProcessor offline, polling lag, retry loop activation), can the dequeue happen more than 5 seconds after the template edit? If so, `GetEnterpriseRoleUpdatedProductsByRoleTemplateId` may return an empty diff and the batch will silently no-op.

5. **`InsertBatchPersonaIdToEnterpriseRoleBatchProcess` use case.** It enqueues batch rows for a TVP of persona IDs WITHOUT touching `RoleTemplateUserMapping`. What flow uses it? (Possibly a re-sync admin operation, but worth confirming.)

6. **Why are `/erpbatchprocessor` and `/ppbatchprocessor` `[AllowAnonymous]`?** The CLAUDE.md note about `BatchProcessController` says "All four actions are `[AllowAnonymous]` with inline comments noting this is temporary pending Windows Service client credentials." Closing this would tighten the security perimeter — and the LD-gated migration to Core API v2 is the natural time to do it.
