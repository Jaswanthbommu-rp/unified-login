# Performance Opportunities Backlog

Ranked by feasibility (safe mechanical changes first) × likely impact.

## Backlog cursor (round-robin)

Next file to target: **`Component/Landing/Logic/ManageUserLogin.cs`** (six `Where(...).FirstOrDefault()` instances on the impersonation path; also a hoist candidate). Then `Repository/UserRepository.cs`, then per-product files.

## In progress / submitted

- **`BaseUserRights.cs` LINQ refactor** — PR opened in run 26283093839 (2026-05-22 10:42 UTC), draft, awaiting maintainer review. Branch: `perf-assist/baseuserrights-linq-2026-05-22`. Three mechanical wins (two `Where().FirstOrDefault()` collapses and one double-`ToList()` removal). Build/test must run on Windows + VS 2022.

## High confidence, mechanically verifiable (LINQ anti-patterns)

These match Roslyn analyzer recommendations CA1827 / CA1828 / similar.

### `.Where(pred).First[OrDefault]()` → `.First[OrDefault](pred)` (one allocation each)

Identified locations (from grep of `Component/Landing`):

- ~~`Component/Landing/Base/BaseUserRights.cs:68, 90` — submitted in PR (run 2)~~
- `Repository/UserRepository.cs:2899` — `_managePersona.ListPersona(userLogin.RealPageId).Where(c => c.OrganizationPartyId == org.OrganizationPartyId).FirstOrDefault()`
- `Repository/UserRepository.cs:2905` — `userLoginPersonaList.Where(x => x.PrimaryOrganization == true).FirstOrDefault()`
- `Repository/ProfileRepository.cs:1116, 1118` — telecommunications filters
- `Logic/ManageEmployeeAccess.cs:654` — `roleRights.Where(x => x.Role == defaultRole && x.Roletype == "System").FirstOrDefault()`
- `Logic/ManageUserLogin.cs:450, 457, 951, 956, 1338, 1342` — six `Where(...).FirstOrDefault()` instances **(next round-robin target)**
- `Logic/ManageOrganization.cs:1980` — `booksCompanyDetails.Where(add => add.Id == items.BooksCustomerMasterId).FirstOrDefault()?`

### `.Count() > 0` / `.Count() == 0` on filtered enumerables → `.Any()` / `!.Any()`

- `Repository/UserRepository.cs:1706, 6356` — including a notable `companyList.Where(...).Distinct().Count() > 0` which can drop the `Distinct()` entirely
- `Logic/Product/ManageProductAssetOptimization.cs:970, 1278, 2062`
- `Logic/Product/ManageProductProspectContact.cs:703, 847`
- `Logic/Enterprise/User/UserManagement.cs:93, 209`
- `Logic/Product/ManageProductOneSiteAccounting.cs:668`

### Double-`.ToList()` materialization patterns

The `BaseUserRights.cs` PR exposed a `productSettingList.ToList().Any(...)` + `productSettingList.ToList().FirstOrDefault(...)` shape. Worth a repo-wide grep for the same anti-pattern (`\.ToList\(\)\.[A-Z]` followed by a second `\.ToList\(\)\.` on the same variable).

## Medium-confidence opportunities (need more inspection)

- **`ManageUserLogin.cs` hoist candidate** — the impersonation block re-fetches and re-filters persona lists 3+ times in the same method (lines 450/457, 951/956, 1338/1342). Likely candidate for hoisting the `userLoginPersonaList` lookup once and reusing it.
- **`BaseUserRights.GetCompanyRoles`** wraps a 120-second `RPObjectCache` around a stored-proc call but computes a per-request `productListHash` via `GetHashCode()` on an `IList<int>`. The default `GetHashCode` on `List<int>` is reference-based — cache key may not be stable across calls. Worth confirming with maintainers before changing.

## Low-confidence / needs maintainer input

- **Build.Net.bat hardcoded MSBuild path** — Windows-only, but the VS 2017 path is stale (VS 2022 is the modern target per CLAUDE.md). A small developer-experience win to make the path dynamic via `vswhere`.
- **Solution-wide nullable reference types** — would require maintainer buy-in, not a perf change per se.

## Notes about the codebase

- `RPObjectCache.GetFromCache<T>` (Service/SharedObjects/Base/RPObjectCache.cs) — generic in-memory cache with per-key TTL in seconds. Used widely (productInternalSetting cached 120 s, getRoleByPersona 30 s). Good place to look for cache-hit improvements.
- `BaseRepository` uses Dapper + stored procs (names centralized in `StoredProcNameConstants`).
- DI is constructor-based; controllers expose test constructors with `IRepository`, `HttpMessageHandler`, `ILdClient`, `DefaultUserClaim` mocks.
- Per CLAUDE.md, "ProductEnum and BlueBookProductConstants mappings have test coverage that enforces consistency — keep them in lockstep."
