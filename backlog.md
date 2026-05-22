# Performance Opportunities Backlog

Ranked by feasibility (safe mechanical changes first) × likely impact.

## Backlog cursor (round-robin)

**Next:** re-submit `BaseUserRights.cs` LINQ refactor — original work from run 2 never landed (issue #3 / E003). Branch off `master` HEAD this time. After that: `Repository/UserRepository.cs`, then `Repository/ProfileRepository.cs`, then per-product files.

## In flight (PR open)

- **`ManageUserLogin.cs`** — run 26285110829. Six `Where().FirstOrDefault()` collapses + 2 `== true` normalisations on user-status/Kafka publish path. Branch `perf-assist/manageuserlogin-linq-2026-05-22`. Patch 6.2 KB / 1 file. Awaiting review.

## Submitted but never landed

- **`BaseUserRights.cs`** — run 26283093839 attempted but safeoutputs rejected (E003, 2335 files) because branched off `origin/gh-aw-perf-improver`. Code edits still safe; re-submit branching off `master`. Wins: lines 68, 90 (`Where().FirstOrDefault()`), and a double-`ToList()` removal.

## High confidence (Roslyn CA1827 / CA1842)

### `.Where(pred).First[OrDefault]()` → `.First[OrDefault](pred)`

- `Component/Landing/Base/BaseUserRights.cs:68, 90` — re-submit (see above)
- `Repository/UserRepository.cs:2899, 2905` — persona / primary-org filters
- `Repository/ProfileRepository.cs:1116, 1118` — telecom filters
- `Logic/ManageEmployeeAccess.cs:654` — role-rights filter
- `Logic/ManageOrganization.cs:1980` — books-company-details filter

### `.Count() > 0` on filtered enumerables → `.Any()`

- `Repository/UserRepository.cs:1706, 6356` — one has `Where(...).Distinct().Count() > 0` that can drop `Distinct()`
- `Logic/Product/ManageProductAssetOptimization.cs:970, 1278, 2062`
- `Logic/Product/ManageProductProspectContact.cs:703, 847`
- `Logic/Enterprise/User/UserManagement.cs:93, 209`
- `Logic/Product/ManageProductOneSiteAccounting.cs:668`

### Double-`.ToList()` materialization

Repo-wide grep for `\.ToList\(\)\.[A-Z]` then second `\.ToList\(\)\.` on same var. Known instance in `BaseUserRights.cs`.

## Medium-confidence (need maintainer input)

- **`ManageUserLogin.cs` extract-method hoist** — three blocks at ~450, ~951, ~1338 are structurally identical (`ListPersona().FirstOrDefault(...)` → `GetUserDetails` → `ListUserLoginPersona` → `FirstOrDefault(...)` → conditional Kafka publish). Candidate for `PublishUserStatusEventIfPrimaryOrg(...)` helper. Do after current LINQ PR lands.
- **`BaseUserRights.GetCompanyRoles` cache key bug** — wraps 120 s `RPObjectCache` around an SP call but computes `productListHash` via `GetHashCode()` on `IList<int>` (reference-based, not stable). Confirm with maintainers; if real, it's a correctness fix that also doubles cache hit-rate.

## Low-confidence

- **`Build.Net.bat` hardcoded MSBuild path** — VS 2017 path is stale (VS 2022 per CLAUDE.md). Could be dynamic via `vswhere`.

## Codebase pointers

- `RPObjectCache.GetFromCache<T>` (Service/SharedObjects/Base/) — per-key TTL cache; productInternalSetting 120 s, getRoleByPersona 30 s.
- DI is constructor-based; controllers have test constructors taking `IRepository`, `HttpMessageHandler`, `ILdClient`, `DefaultUserClaim` mocks.
- `ProductEnum` ↔ `BlueBookProductConstants` must stay in lockstep (test enforces).
