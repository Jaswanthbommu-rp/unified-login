You are a senior C# / ASP.NET Core architect. Review and refactor code under the repository folder coremigration/ only.
Target framework: .NET 10.

🎯 Objectives

List all controllers in coremigration/UnifiedLogin.LandingAPI.
For each controller, identify all dependencies (constructor-injected services, repositories, helpers, options/config, external clients, etc.).
Find the relevant business logic classes used by those controllers in coremigration/UnifiedLogin.BusinessLogic.
Do work in reviewable chunks.
If business logic is missing or embedded in controllers, extract/refactor/create it under:

coremigration/UnifiedLogin.BusinessLogic/LogicAsync/


Refactor controllers to use the new async logic classes from LogicAsync (controllers must be thin).
Ensure data access is routed through repository classes (introduce/update repositories where needed). Avoid direct DB/context usage from controllers.
Refactor using latest C# standards compatible with .NET 10, while preserving existing API contracts (routes, request/response shapes, status codes).


✅ Non‑Negotiable Constraints

Scope restriction: Only scan, refactor, and track files inside coremigration/.
If a dependency points outside, note it but do not modify it (mark in CSV as “External/Out of scope” in Notes).
No breaking changes: Do not change public endpoints/contracts unless explicitly required; document any unavoidable changes clearly.
Prefer async/await end-to-end, no .Result / .Wait().
Keep controllers thin: validate input, call logic, return response.


🧾 CSV Conversion Tracker (Core Requirement)
Create/maintain a conversion status CSV limited to classes found under coremigration/ only.
CSV filename (default): coremigration/conversion-status.csv
CSV Columns (minimum required)

Project (e.g., UnifiedLogin.LandingAPI / UnifiedLogin.BusinessLogic / UnifiedLogin.DataAccess)
FilePath (relative to repo root; must start with coremigration/)
ClassName
ClassType (Controller / Logic / Repository / DTO / Other)
Status (Converted / Not-Converted)
ReplacedBy (new class path/name if moved or replaced)
Notes (e.g., “external dependency”, “await chain fixed”, “logic extracted to LogicAsync”)

CSV Rules

Include only classes whose file paths are under coremigration/.
Add every discovered controller + related business logic + related repositories under coremigration/.
Any class refactored to async Logic layer gets Status=Converted.
Any class still using old/sync patterns or not yet refactored remains Status=Not-Converted.
If a controller is refactored to use LogicAsync, mark that controller as Converted.
If a dependency is outside coremigration/, do not add it as a class row, but mention it in Notes of the referring class row.

Important output constraint: You cannot physically write files here—so output the CSV content in a copy/paste-ready code block each time it changes.

🧭 Step-by-step Plan (Do these in order)
Step 1 — Controller Inventory
Scan coremigration/UnifiedLogin.LandingAPI and output a Markdown table with:

Controller Name
File Path
Route base/attributes
Actions (HTTP verb + route template + method name)
Constructor dependencies (type + name)

Also populate the initial CSV entries for these controllers.
Step 2 — Dependency + Business Logic Mapping
For each controller action:

Provide a 1–2 line functional summary
Identify where business logic currently exists in coremigration/UnifiedLogin.BusinessLogic (class + method)
Identify repository usage (class + method) under coremigration/
Mark logic embedded in controllers that should be moved

Update CSV for the relevant BusinessLogic/Repository classes found under coremigration/ and set their current status.
Step 3 — Refactor Design (LogicAsync + Repositories)
Propose:

What new async logic classes/interfaces belong in coremigration/UnifiedLogin.BusinessLogic/LogicAsync/
What controller code moves to which logic method
What repository methods need creation/changes for clean data access
How to keep behavior identical

Update CSV with the planned new LogicAsync classes marked Not-Converted until implemented.
Step 4 — Implement Refactor (Show Patches)
For each controller:

Create/update async logic classes in LogicAsync/
Ensure async all the way down
Refactor controllers to call LogicAsync
Ensure repositories are used properly
Update DI registration if needed (within coremigration/ only)

Output changes as:

New files: full content
Modified files: unified diffs (or clear before/after blocks)

Update CSV statuses to Converted for each completed controller/logic/repository file.
Step 5 — Quality Bar + Verification

No business rules remain in controllers
Exception handling consistent with existing patterns
Logging is structured; avoid sensitive data
Nullable reference types respected
C#/.NET 10 conventions applied

Provide a “Post-refactor checklist” and recommended tests.

📌 Output Format
Respond in Markdown with:

Controller Inventory
Dependency & Logic Mapping
Refactor Plan (LogicAsync + repository changes)
Code Changes (New files + Diffs)
Updated CSV (full content in a code block)
Post‑Refactor Checklist
Risks / Edge cases / Recommended tests

Start with Step 1 — Controller Inventory.

Tiny improvement tip

stay inside coremigration/ (prevents scope creep),
keep a persistent conversion ledger (CSV),
do work in reviewable chunks (inventory → mapping → plan → diffs),
align with .NET 10 + modern C# without breaking public APIs.