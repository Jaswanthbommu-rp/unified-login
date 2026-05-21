---
date: 2026-05-20T00:00:00-05:00
researcher: Kalyana Chakravarthy Bingi
git_commit: 41d74c291303d0b02e925e3e0d38d6a2282b9e5c
branch: feature/2876130_loginURI
repository: Unity/unified-login-main
topic: "TFS #2876130 — Increase accepted length of Product Login URI (150 → 500)"
tags: [research, codebase, login-uri, saml-product-settings, database-schema, identity, identity-rr, tfs-2876130]
status: complete
last_updated: 2026-05-20
last_updated_by: Kalyana Chakravarthy Bingi
---

# Research: TFS #2876130 — Increase accepted length of Product Login URI

**Date**: 2026-05-20
**Researcher**: Kalyana Chakravarthy Bingi
**Git Commit**: 41d74c291303d0b02e925e3e0d38d6a2282b9e5c (origin/master at branch creation)
**Branch**: feature/2876130_loginURI (off origin/master)
**Repository**: Unity/unified-login-main

## Research Question

What needs to change across the unified-login codebase to satisfy TFS user story #2876130 — "Increase accepted value of 'Login URI' table in DB" — which asks for the Product Login URI to accept up to 500 characters (the ticket describes the current limit as 150)?

Ticket context (from work item 2876130):
- Area: `Unity\Login\Kingpin`, Sprint 26.11
- Acceptance: an admin editing a product in Configuration → Products must be able to save a Login URI longer than 150 chars (up to 500), and the DB column must accept it.
- Parent: 2779915, Related: 2855881.

## Summary

The Product Login URI is stored in a single column — `LoginUri` — on the SAML product-settings tables of the Identity database. **The current column width in source-controlled DDL is `NVARCHAR(100)`, not 150 as stated in the ticket description.** This discrepancy should be confirmed with the ticket author (JohnErick Gutierrez) before any code change; the target is `NVARCHAR(500)` either way.

The change is overwhelmingly database-layer. There is **no C# `[StringLength]` / `[MaxLength]` / FluentValidation guard** on `LoginUri` anywhere in the C# code, and **no Razor view** edits Login URI — the admin edit form is in the Angular SPA, whose **TypeScript source is not present in this repo** (only compiled JS bundles). The compiled bundles do not show a `maxlength="150"` attribute, suggesting any current UI limit (if one exists) lives in the Angular source repo and would need to be raised there in parallel.

Concrete change surface:
- 3 SQL table definitions (Identity + Identity-RR Ident + Identity-RR Auth)
- 5 stored procedures with `@LoginUri NVARCHAR(100)` / `@LoginURI NVARCHAR(100)` parameters
- 0 required C# changes (optional: add `[StringLength(500)]` on `ProductSamlSettings.LoginUri` for defence in depth)
- 1 deployment migration script (new `ALTER COLUMN` to run against existing DBs)
- Angular SPA source change (in a different repo) to raise any client-side `maxLength` validator
- Tests do not assert on length and need no change unless we choose to add a 500-char fixture

## Detailed Findings

### Discrepancy: ticket says 150, code says 100

Every `LoginUri` declaration in source control is `NVARCHAR(100)`. There is no `NVARCHAR(150)` declaration anywhere. Three possibilities:

1. The ticket author rounded up / misremembered.
2. A production-only ALTER widened the column to 150 outside of source control (drift).
3. "150" refers to a UI-level cap in the Angular source repo (not in this repo).

Action: confirm with the requester. The proposed change to 500 is unambiguous either way, but the "current" value reported in the PR / release notes should match reality.

### Where Login URI is defined (the binding constraint)

The hard limit is enforced at the DB column level. There is no C# validator, no FluentValidation rule, and no DataAnnotation on the `LoginUri` property. Any value passing the UI today is silently truncated to the SP parameter / column width.

The column exists in three table files under two database projects:

- `Enterprise/Subsystem/ProductLauncher/Database/Identity/Ident/Tables/SamlProductSettings.sql:5` — `[LoginUri] NVARCHAR(100) NOT NULL`
- `Enterprise/Subsystem/ProductLauncher/Database/Identity-RR/Identity/Schema-Model/Ident/Tables/SamlProductSettings.sql:5` — `[LoginUri] [nvarchar] (100) NOT NULL`
- `Enterprise/Subsystem/ProductLauncher/Database/Identity-RR/Identity/Schema-Model/Auth/Tables/ProductSamlSettings.sql:5` — `[LoginUri] [nvarchar] (100) NOT NULL` (note: this is a second physical table — `Auth.ProductSamlSettings` — in the RR project)

The `sqlproj` registrations for these files:
- `Enterprise/Subsystem/ProductLauncher/Database/Identity/Identity.sqlproj:213`
- `Enterprise/Subsystem/ProductLauncher/Database/Identity-RR/Identity/Identity.sqlproj:408`

### Stored procedures with NVARCHAR(100) parameters (write path)

All five procs declare a 100-char URI parameter; widening the column without widening these will fail SSDT publish or silently truncate at the proc boundary.

- `Enterprise/Subsystem/ProductLauncher/Database/Identity/Ident/Stored Procedures/CreateSamlProductSetting.sql:3` — `,@LoginUri nvarchar(100)`
- `Enterprise/Subsystem/ProductLauncher/Database/Identity/Enterprise/Stored Procedures/ProductConfigurationSetup.sql:3` — `@LoginUri NVARCHAR(100),` (upsert SP — primary writer)
- `Enterprise/Subsystem/ProductLauncher/Database/Identity/Enterprise/Stored Procedures/CreateNewProduct.sql:3` — `@LoginURI NVARCHAR(100),`
- `Enterprise/Subsystem/ProductLauncher/Database/Identity/Enterprise/Stored Procedures/SetProduct.sql:12` — `@LoginURI NVARCHAR(100),`
- `Enterprise/Subsystem/ProductLauncher/Database/Identity/Enterprise/Stored Procedures/ImportProduct.sql:12` — `@LoginURI NVARCHAR(100),`

Suggestive prior-work hints — **someone has tried this before and stopped halfway**:
- `CreateNewProduct.sql:7` already declares `@LoginURL NVARCHAR(500),` — a 500-wide parameter sits alongside the 100-wide `@LoginURI`, and the value flows through `@LoginURI` (truncating). Lines 114, 117, 120, 122 in that file pass `@LoginURL` through `@LoginURI` into `LoginUri`.
- `SetProduct.sql:15` contains a **commented-out** `--@LoginURL NVARCHAR(500),    -- Same as @LoginURI`.

Read-only procs that select `LoginUri` (no parameter change needed but worth noting — the returned column will reflect the new width automatically):
- `Enterprise/Subsystem/ProductLauncher/Database/Identity/Ident/Stored Procedures/GetProductSamlSettings.sql:8`
- `Enterprise/Subsystem/ProductLauncher/Database/Identity-RR/Identity/Programmable Objects/Ident/Stored Procedures/GetProductSamlSettings.sql:16`
- `Enterprise/Subsystem/ProductLauncher/Database/Identity-RR/Identity/Programmable Objects/Auth/Stored Procedures/GetProductSamlSettings.sql:21`
- `Enterprise/Subsystem/ProductLauncher/Database/Identity/Enterprise/Stored Procedures/GetAllProducts.sql:9`

### Post-deployment data scripts (local @LoginURI NVARCHAR(100) declarations)

These insert seeded product rows during a fresh deploy. They use local `DECLARE @LoginURI nvarchar(100)` and pass into `CreateSamlProductSetting` / `ProductConfigurationSetup`. Today's seeded URIs are under 100 chars so no truncation. They are historical scripts — leaving them at 100 is safe unless a future release script adds a URI > 100 chars.

- `Enterprise/Subsystem/ProductLauncher/Database/Identity/Scripts/PostDeployment/PostDeploymentScripts/November2018Release.sql:13`
- `Enterprise/Subsystem/ProductLauncher/Database/Identity/Scripts/PostDeployment/PostDeploymentScripts/October2018Release.sql:2749` and `:2773`
- Several other release scripts under the same folder perform UPDATEs to `LoginUri` with literal values (≤100 chars) and need no change: `Version0.0.0.0.sql`, `Version0.0.0.1.sql`, `Version0.0.0.9.sql`, `2021Q2Release.sql`, plus other dated releases. Note `2021Q2Release.sql:4481–4530` already uses `@LoginURL NVARCHAR(500)` for a different setting — another prior-art breadcrumb.

### Migration baseline (Identity-RR)

Historical record only — `Enterprise/Subsystem/ProductLauncher/Database/Identity-RR/Identity/Migrations/0001_20170727-1259_cford.sql:3669` and `:5406`: original `CREATE TABLE` snapshots with `[LoginUri] [nvarchar] (100) NOT NULL`. Editing rewrites history; leave untouched. The fix should be a new forward-migration / DACPAC publish that emits `ALTER TABLE ... ALTER COLUMN LoginUri NVARCHAR(500) NOT NULL` for both tables.

### C# layer — read and write paths

The shared DTO that carries `LoginUri` between layers has **no length annotation**:
- `Enterprise/Subsystem/ProductLauncher/Service/SharedObjects/Landing/ProductSamlSettings.cs:21` — `public string LoginUri { get; set; }`

Repository (read only — uses Dapper, maps `LoginUri` column → property by name; no length param):
- `Enterprise/Subsystem/ProductLauncher/Component/Landing/Repository/Interfaces/ISamlRepository.cs:86` — `GetProductSamlSettingsByProductId`
- `Enterprise/Subsystem/ProductLauncher/Component/Landing/Repository/SamlRepository.cs:110–114` — calls `Ident.GetProductSamlSettings`
- SP-name constant: `Enterprise/Subsystem/ProductLauncher/Service/SharedObjects/Constants/StoredProcNameConstants.cs:234` — `SP_GetProductSamlSettings = "Ident.GetProductSamlSettings"`. **No equivalent constant exists for the write SP** — writes go through `ProductConfigurationSetup` and `CreateNewProduct` which are invoked from SQL deployment scripts and admin tooling, not from a typed Dapper helper. (Spot-check this if a write flow is added later.)

Consumers of `LoginUri` (read at request-time — no validation, no change needed):
- `Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/Controllers/ProductController.cs:344` — `[Route("product/login")] GET GetProductLoginInfo` (line 869: `string loginUri = productSamlSettings.LoginUri;`, line 873: `AlternateLoginURL` internal-setting override, line 877: assigned to `RedirectUrl`)
- `Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/Controllers/SamlController.cs:151–154` — `GET saml/product/setting` returns `ProductSamlSettings` (line 335 has a Swagger example)
- `Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/Saml/ProductSaml.cs:195,197` — uses `LoginUri` as SAML POST action URL
- `Enterprise/Subsystem/ProductLauncher/Component/Landing/Logic/Product/SAML/RealPageSAML.cs:377,471` — sets `samlEndpointURL = productSamlSettings.LoginUri;`
- `Enterprise/Subsystem/ProductLauncher/Web/Landing/Controllers/ProductController.cs:648,927,1144` — Web MVC redirects use `LoginUri`

**`ProductPanelController.cs` does not reference `LoginUri`.** Although it is on the modified-file list for the AO branch (feature/2625227), the LoginUri field is not touched there — there is no risk of merge collision with the AO ProductUser changes.

### UI layer — Angular SPA (source not in this repo)

There are **no Razor `.cshtml` views** that edit `LoginUri`. The product edit screen is an Angular SPA, of which only compiled, minified output is checked in:

- `Enterprise/Subsystem/ProductLauncher/Web/Landing/3037.a0e828c5ea7ca408.js` (line 1) — contains the input declaration `["type","text","name","productLoginUri","placeholder","Login URI",3,"ngModel"]` and the save-payload map `productLoginUri → loginUri`.
- `Enterprise/Subsystem/ProductLauncher/Web/Landing/unityui/chunk-NW45K3NU.js` (line 4) — same content for the `unityui` build variant.

**Neither bundle contains `maxlength="150"` or a `Validators.maxLength(150)` call.** Either there is no client-side cap today (and the perceived "150" is a different concept — see next paragraph) or the cap is hidden inside the minified token stream and is enforced by an Angular `Validators.maxLength` in the source repo.

A distinct, **unrelated** "Initiate Login URI" field exists for IdentityServer OIDC client config (Configuration → Clients):
- `Enterprise/Subsystem/ProductLauncher/Web/Landing/1220.7bf94b57bc9300cb.js` — `n.clientDetails.initiateLoginUri`
- `Enterprise/Subsystem/ProductLauncher/Web/Landing/unityui/chunk-ITRHLRKB.js` — same

Do not confuse the OIDC `initiateLoginUri` with the product `loginUri` from `SamlProductSettings`. The ticket targets the latter.

The Angular TypeScript source for the Configuration → Products edit form must be located in its own repo and updated to raise any `Validators.maxLength` cap to 500. That is a separate PR outside this codebase.

### Tests (no required change)

- `Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/LandingAPI.Test/ControllerTest/ProductTests.cs:609` — fixture `LoginUri = "https://qa-mc.realpage.com/mcauth/sso/oauth?productCode=PP"` (well under 100 chars; no length assertion).
- `Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/LandingAPI.Test/Logic/ProductIntegration/PamTestData.cs:62` — unrelated constant `Name = "GetLoginUrlEndpoint"`.
- Swagger example `Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/Controllers/SamlController.cs:335` — `LoginUri = "https://someproducturl/loginsaml"` (illustrative only).

No test asserts a max length. Optional improvement: add a unit test that round-trips a 500-char URI through `ProductConfigurationSetup` → `GetProductSamlSettings` (only meaningful if the test stack hits a real DB).

## Code References

### Must change

- `Enterprise/Subsystem/ProductLauncher/Database/Identity/Ident/Tables/SamlProductSettings.sql:5` — Identity / `Ident.SamlProductSettings.LoginUri` column DDL: 100 → 500
- `Enterprise/Subsystem/ProductLauncher/Database/Identity-RR/Identity/Schema-Model/Ident/Tables/SamlProductSettings.sql:5` — Identity-RR mirror of the above
- `Enterprise/Subsystem/ProductLauncher/Database/Identity-RR/Identity/Schema-Model/Auth/Tables/ProductSamlSettings.sql:5` — Identity-RR `Auth.ProductSamlSettings.LoginUri` column DDL: 100 → 500 (NOT included in this branch's scope per user direction)
- `Enterprise/Subsystem/ProductLauncher/Database/Identity/Ident/Stored Procedures/CreateSamlProductSetting.sql:3` — `@LoginUri nvarchar(100)` → `nvarchar(500)`
- `Enterprise/Subsystem/ProductLauncher/Database/Identity/Enterprise/Stored Procedures/ProductConfigurationSetup.sql:3` — `@LoginUri NVARCHAR(100)` → `NVARCHAR(500)`
- `Enterprise/Subsystem/ProductLauncher/Database/Identity/Enterprise/Stored Procedures/CreateNewProduct.sql:3` — `@LoginURI NVARCHAR(100)` → `NVARCHAR(500)`. Also consider removing the now-redundant `@LoginURL NVARCHAR(500)` (line 7) intermediate, or at minimum stop truncating it.
- `Enterprise/Subsystem/ProductLauncher/Database/Identity/Enterprise/Stored Procedures/SetProduct.sql:12` — `@LoginURI NVARCHAR(100)` → `NVARCHAR(500)`. The commented-out `@LoginURL NVARCHAR(500)` at line 15 can be removed once the rename is consolidated.
- `Enterprise/Subsystem/ProductLauncher/Database/Identity/Enterprise/Stored Procedures/ImportProduct.sql:12` — `@LoginURI NVARCHAR(100)` → `NVARCHAR(500)`
- A new release migration script under `Database/Identity/Scripts/PostDeployment/PostDeploymentScripts/` containing `ALTER TABLE Ident.SamlProductSettings ALTER COLUMN LoginUri NVARCHAR(500) NOT NULL` and the same for `Auth.ProductSamlSettings`.

### Optional (defence in depth)

- `Enterprise/Subsystem/ProductLauncher/Service/SharedObjects/Landing/ProductSamlSettings.cs:21` — add `[StringLength(500)]` so a future caller without a real DB still fails fast.

### Read-only references (no change required — listed for awareness)

- `Enterprise/Subsystem/ProductLauncher/Component/Landing/Repository/SamlRepository.cs:110–114`
- `Enterprise/Subsystem/ProductLauncher/Service/SharedObjects/Constants/StoredProcNameConstants.cs:234`
- `Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/Controllers/ProductController.cs:344,869,873,877`
- `Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/Controllers/SamlController.cs:151–154,335`
- `Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/Saml/ProductSaml.cs:195,197`
- `Enterprise/Subsystem/ProductLauncher/Component/Landing/Logic/Product/SAML/RealPageSAML.cs:377,471`
- `Enterprise/Subsystem/ProductLauncher/Web/Landing/Controllers/ProductController.cs:648,927,1144`
- `Enterprise/Subsystem/ProductLauncher/Database/Identity/Ident/Stored Procedures/GetProductSamlSettings.sql:8`
- `Enterprise/Subsystem/ProductLauncher/Database/Identity-RR/Identity/Programmable Objects/Ident/Stored Procedures/GetProductSamlSettings.sql:16`
- `Enterprise/Subsystem/ProductLauncher/Database/Identity-RR/Identity/Programmable Objects/Auth/Stored Procedures/GetProductSamlSettings.sql:21`

### UI (out-of-repo)

- `Enterprise/Subsystem/ProductLauncher/Web/Landing/3037.a0e828c5ea7ca408.js:1` — compiled Angular product edit form
- `Enterprise/Subsystem/ProductLauncher/Web/Landing/unityui/chunk-NW45K3NU.js:4` — compiled Angular product edit form (unityui variant)

These two bundles are downstream artifacts. Real change must be made in the Angular source repo and rebuilt into these bundles.

## Architecture Insights

- **The DB is the only validator.** The lack of a `[StringLength]` on `ProductSamlSettings.LoginUri` means any C# caller can submit any-length string; truncation/error happens at the SP parameter boundary. This is fragile — a long URI today produces silent truncation (since SQL Server truncates to the declared NVARCHAR(n) of the parameter, not the column). Adding `[StringLength(500)]` after the DB widening would make a too-long URI fail loudly at the API boundary.
- **The column lives in two physically-distinct tables** (`Ident.SamlProductSettings` in Identity, and the same name + an `Auth.ProductSamlSettings` in Identity-RR). Both must move together to keep the read-replica schema consistent.
- **Prior half-attempt detected.** `CreateNewProduct.sql` defines both `@LoginURI NVARCHAR(100)` and `@LoginURL NVARCHAR(500)`, and `SetProduct.sql` has a commented-out 500-char alias. Someone previously planned this widening, scoped it, and abandoned. The TFS story explains the trigger ("some products have new URI that has more than 150 characters") — the original spike never landed.
- **Read-replica project (`Identity-RR`)** carries its own schema-model copy. SSDT-based DACPAC publishes will detect a drift if only one project is updated. Keep both projects in lockstep in the same PR.
- **The admin edit UI is decoupled.** The Angular SPA is consumed as pre-built JS — the Razor/MVC layer is just the host. This means a backend-only PR will technically allow ≥100-char saves through the API, but the form may still cap the input. Coordinating the Angular-source PR with this DB PR is necessary for the end-to-end story to land.

## Scope Implemented on This Branch (feature/2876130_loginURI)

Per direction from the assignee, only the 7 edits below are made on this branch. The `Auth.ProductSamlSettings` widening and any C# annotation are intentionally deferred.

- `Database/Identity/Ident/Tables/SamlProductSettings.sql` — NVARCHAR(500)
- `Database/Identity-RR/Identity/Schema-Model/Ident/Tables/SamlProductSettings.sql` — NVARCHAR(500)
- `Database/Identity/Ident/Stored Procedures/CreateSamlProductSetting.sql` — nvarchar(500)
- `Database/Identity/Enterprise/Stored Procedures/ProductConfigurationSetup.sql` — NVARCHAR(500)
- `Database/Identity/Enterprise/Stored Procedures/CreateNewProduct.sql` — NVARCHAR(500)
- `Database/Identity/Enterprise/Stored Procedures/SetProduct.sql` — NVARCHAR(500)
- `Database/Identity/Enterprise/Stored Procedures/ImportProduct.sql` — NVARCHAR(500)

## Open Questions

1. **Is the current limit 150 or 100?** Every source-controlled DDL says `NVARCHAR(100)`. Confirm with the ticket author whether production has been ALTERed to 150 (drift), or whether "150" was a misstatement. The target — 500 — is unchanged either way, but the PR description should report the true starting value.
2. **Where is the Angular source repo for Configuration → Products?** The compiled bundles `3037.*.js` and `unityui/chunk-NW45K3NU.js` are checked in here, but the `.ts` / `.html` source for the `productLoginUri` form control is not. To remove any client-side `maxLength` cap we need to find and modify that source repo and rebuild the bundles.
3. **Should `SetProduct.sql` / `CreateNewProduct.sql` consolidate `@LoginURI` and `@LoginURL`?** Both procs currently carry both parameters (one truncated, one not). After widening, the duplication serves no purpose. This is a cleanup question — out of scope for the ticket but cheap to do at the same time.
4. **Does anything outside this repo (e.g. an external app or downstream consumer) read the `LoginUri` and assume `≤100` / `≤150` chars?** Worth a grep across the wider Unity codebase if you suspect cross-repo coupling.
5. **Should we add `[StringLength(500)]` on `ProductSamlSettings.LoginUri`?** Trade-off: defence in depth vs. yet another place to keep in sync if a future PR widens further. Lean yes, since the property is currently unprotected.
