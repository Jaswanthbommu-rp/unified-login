# Performance Opportunities Backlog

Ranked by feasibility (safe mechanical changes first) × likely impact.

## Backlog cursor (round-robin)

**Next:** `Repository/UserRepository.cs` 2899 + 2905 (persona / primary-org `Where().FirstOrDefault()`), then `ProfileRepository.cs` 1116/1118, then `BaseUserRights.cs:110-112` double-`.ToList()`.

Alternate Task 3 (implement) with Task 4 (PR maintenance / nudging) until at least one in-flight item lands.

## In flight (branch pushed; awaiting maintainer to convert to real PR)

Reminder: `list_pull_requests` returns `[]` in this repo (see workflow_quirks). Each item below is a safeoutputs issue-fallback + remote branch.

- `ManageUserLogin.cs` — issue #6, branch `perf-assist/manageuserlogin-linq-2026-05-22-1511425c286d9c29`. Six `Where().FirstOrDefault()` collapses + 2 `==true` normalisations on user-status/Kafka publish path. Patch 6.2 KB / 1 file.
- `BaseUserRights.cs` — branch `perf-assist/baseuserrights-linq-2026-05-22` (run 26285981135). Two `Where(c => c.Organization.RealPageId == ...).FirstOrDefault()` collapses at lines 68 + 90. Patch 2.7 KB / 1 file / 46 lines.

## High confidence (Roslyn CA1827 / CA1842)

### `.Where(pred).First[OrDefault]()` → `.First[OrDefault](pred)`

- `Repository/UserRepository.cs:2899, 2905` — persona / primary-org filters **(NEXT)**
- `Repository/ProfileRepository.cs:1116, 1118` — telecom filters
- `Logic/ManageEmployeeAccess.cs:654` — role-rights filter
- `Logic/ManageOrganization.cs:1980` — books-company-details filter

### `.Count() > 0` on filtered enumerables → `.Any()`

- `Repository/UserRepository.cs:1706, 6356` — one has `Where(...).Distinct().Count() > 0` (drop `Distinct()` too)
- `Logic/Product/ManageProductAssetOptimization.cs:970, 1278, 2062`
- `Logic/Product/ManageProductProspectContact.cs:703, 847`
- `Logic/Enterprise/User/UserManagement.cs:93, 209`
- `Logic/Product/ManageProductOneSiteAccounting.cs:668`

### Double-`.ToList()` materialization

- `BaseUserRights.cs:110-112` — `productSettingList.ToList().Any(...)` then `.ToList().FirstOrDefault(...)`. Both `.ToList()` unnecessary on `IEnumerable<T>`. Follow-up to current BaseUserRights PR.
- Repo-wide: grep `\.ToList\(\)\.[A-Z]` then second `\.ToList\(\)\.` on same var.

## Medium-confidence (need maintainer input)

- `ManageUserLogin.cs` extract-method hoist — three structurally identical user-status-publish blocks at ~450/~951/~1338. Candidate: `PublishUserStatusEventIfPrimaryOrg(...)` helper. Do after the LINQ PR lands.
- `BaseUserRights.GetCompanyRoles` cache-key bug — `productListHash` via `IList<int>.GetHashCode()` (reference-based on `List<T>`). 120s `RPObjectCache` key may not be stable across calls → potential correctness fix that also doubles cache hit-rate.

## Low-confidence

- `Build.Net.bat` hardcoded VS 2017 MSBuild path — stale per CLAUDE.md; could use `vswhere`.

## Codebase pointers

- `RPObjectCache.GetFromCache<T>` (`Service/SharedObjects/Base/RPObjectCache.cs:27`) — per-key TTL cache `where T : class`; productInternalSetting 120s, getRoleByPersona 30s.
- DI is constructor-based; controllers have test constructors taking `IRepository`, `HttpMessageHandler`, `ILdClient`, `DefaultUserClaim` mocks.
- `ProductEnum` ↔ `BlueBookProductConstants` lockstep enforced by test.
