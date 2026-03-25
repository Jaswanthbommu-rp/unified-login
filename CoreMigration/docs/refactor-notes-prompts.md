1) Master “Kick-off” Prompt (references the file by filename)

	You are a senior C# / ASP.NET Core architect.

	Use this file as the authoritative reference (single source of truth):
	CoreMigration\docs\refactor-notes.md

	Rules:
	- Follow the document’s scope, constraints, steps, and output format exactly.
	- Limit all scanning, refactoring, and tracking strictly to files under: coremigration/
	- Target framework is .NET 10; apply modern C#/.NET 10 standards and best practices.
	- Do not change public API contracts (routes, request/response shapes, status codes) unless the document explicitly allows it.
	- Maintain/update the conversion-status CSV exactly as described in the document. Track only classes located under coremigration/.

	Missing classes/methods handling (mandatory):
	When you cannot find a referenced class/method/interface:
	1) State clearly: “Not found in scanned paths under coremigration/”.
	2) Provide the nearest matches you did find (similar names/usages).
	3) Identify the call site(s) referencing it (file + line or method).
	4) Propose a safe action plan:
	   - If it should exist in scope: create a minimal stub in the correct layer (LogicAsync/Repository/etc.) and mark in CSV with Status=Not-Converted until implemented.
	   - If it appears external/out-of-scope: do NOT modify; note it in the referring class row’s Notes in the CSV.
	5) Never guess behavior—only infer from local usage; highlight assumptions explicitly.

	Execution:
	Start with Step 1 from the document (“Controller Inventory”).
	Return results in review-friendly Markdown and include the initial CSV content in a copy/paste-ready code block.
	Do ONLY Step 1 in this response.
	``

2) Step-by-Step Prompt Sequence (paste these one at a time)
	Prompt 1 — Step 1: Inventory + Initial CSV
		Using CoreMigration\docs\refactor-notes.md as the authoritative plan, execute Step 1 only:
		- List all controllers in coremigration/UnifiedLogin.LandingAPI
		- Identify each controller’s route base, actions (verb/route/method), and constructor dependencies
		- Create/Update the initial conversion-status CSV (tracking only classes under coremigration/)
		Output:
		1) Controller Inventory table
		2) Initial CSV content (full CSV in a code block)
		Stop after Step 1.
		
	Prompt 2 — Step 2: Dependency & Logic Mapping + CSV update
		Execute Step 2 from CoreMigration\docs\refactor-notes.md:
		- For each controller action, map dependencies to business logic in coremigration/UnifiedLogin.BusinessLogic
		- Identify repository usage under coremigration/
		- Flag business logic embedded in controllers that must be extracted
		- Apply the “missing classes/methods handling” rules when anything is not found
		Update the conversion-status CSV accordingly:
		- Add relevant BusinessLogic and Repository classes under coremigration/
		- Set Status=Not-Converted for items not yet refactored
		Output:
		1) Mapping by controller/action
		2) Findings (embedded logic, duplication, risks)
		3) Updated full CSV in a code block
		Stop after Step 2.
		
	Prompt 3 — Step 3: Refactor Design (LogicAsync + Repository plan) + planned CSV entries
		Execute Step 3 from CoreMigration\docs\refactor-notes.md:
		- Propose the LogicAsync design under coremigration/UnifiedLogin.BusinessLogic/LogicAsync/
		- Define interfaces/classes/method signatures (async end-to-end)
		- Define repository changes required (async methods, responsibility boundaries)
		- Ensure .NET 10 and modern C# patterns
		Update the CSV:
		- Add planned new LogicAsync classes with Status=Not-Converted (until implemented)
		- Fill ReplacedBy and Notes where applicable
		Output:
		1) Refactor design proposal (diagram or structured bullets)
		2) Proposed class list (LogicAsync + repo updates)
		3) Updated full CSV in a code block
		Stop after Step 3.
		
	Prompt 4 — Step 4A: Implement refactor for ONE controller (smallest first) + diffs + CSV
		Execute Step 4 for ONE controller only (pick the simplest/smallest controller first):
		- Create required LogicAsync class(es) in coremigration/UnifiedLogin.BusinessLogic/LogicAsync/
		- Refactor controller to call LogicAsync (thin controller)
		- Ensure data access goes through repositories (update/introduce repository methods if needed under coremigration/)
		- Preserve API contract and behavior
		- Follow “missing classes/methods handling” rules
		Output:
		1) New files (full content)
		2) Modified files (unified diffs preferred)
		3) DI changes if any (diffs)
		4) Updated full CSV with Status updated (Converted/Not-Converted)
		Stop after completing this ONE controller conversion.

	Prompt 5 — Step 4B: Continue controller-by-controller conversions
		Continue Step 4 conversions for the next 5 controllers:
		- Get first 5 names with status not-converted from the CoreMigration\conversion-status.csv
		- Implement LogicAsync + repository updates as required
		- Refactor controller to thin pattern
		- Keep .NET 10 standards, async end-to-end, no contract breaking
		- Build the code at the end and make sure code compiled successfully.
		- Fix the unit tests if any and fix the failed tests if any
		- Update CSV statuses accordingly and output the full CSV each time
		Output:
		1) New files (full content)
		2) Modified files (unified diffs)
		3) Code Compiled successfully including tests
		5) Updated full CSV

		Work on 5 files each and stop for review. 
		After the 5 files are converted, show me count of files converted and not-converted.
		Continue untill all files are refactored.

	Prompt 6 — Step 5: Quality gate + cross-cutting cleanup + final CSV
		Execute Step 5 from CoreMigration\docs\refactor-notes.md:
		- Validate no business logic remains in controllers
		- Validate async end-to-end, no blocking calls
		- Validate consistent error handling/logging
		- Validate repository boundaries
		- Identify duplication and suggest consolidation (within coremigration/ only)
		- Provide a test plan (unit/integration) and edge cases
		Output:
		1) Post-refactor checklist results
		2) Risks/edge cases + recommended tests
		3) Final conversion-status CSV (full content)
		Stop after Step 5.

3) Extra: A “Missing Symbols” Micro-Prompt (use anytime Copilot gets stuck)
	Apply the missing classes/methods handling rules from the instruction file:
	- Report “Not found in scanned paths under coremigration/”
	- Show nearest matches and call sites
	- Decide: create minimal stub in-scope OR mark as external/out-of-scope in Notes
	- Update the CSV accordingly
	Proceed with the safest refactor path without guessing behavior.