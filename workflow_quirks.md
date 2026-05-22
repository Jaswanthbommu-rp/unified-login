# Workflow Quirks — Jaswanthbommu-rp/unified-login perf-improver

## Repo default branch is the 2022 release branch

GitHub API reports `default_branch = 0e89acf1-on-release-2022.09.Q3.11.WMU` (a Q3 2022 release branch).
- **Active development branch is `master`** (PR #1 merged `gh-aw-perf-improver` into `master`).
- safeoutputs' base-branch resolution picks the API default, which would target the 2022 branch.
- Files diverge significantly between `master` and the 2022 default — many perf changes won't apply cleanly there.

**How to work around it:** branch off `gh-aw-perf-improver` (the workflow's own branch, which is essentially a snapshot of `master` at workflow setup time). My branch must have HEAD = `gh-aw-perf-improver` commit + my new commits. That keeps the patch tractable for safeoutputs.

## safeoutputs PR patch generation

`/home/runner/work/_temp/gh-aw/safeoutputs/generate_git_patch.cjs` uses `git format-patch GITHUB_SHA..HEAD` (Strategy 2) when GITHUB_SHA is an ancestor of HEAD.
- If your branch is reset to `master`, you'll be ~3,600 commits ahead of GITHUB_SHA — patch is huge and may fail.
- **Best practice:** `git reset --hard gh-aw-perf-improver` before adding commits, so HEAD = GITHUB_SHA + your new commits.

## First-run gotcha I hit (2026-05-22)

Initial attempt to create a PR off `master` (876 commits ahead of the 2022 default branch via merge-base) failed with "No commits were found to create a pull request." After rebasing onto `gh-aw-perf-improver` (1 commit ahead), PR creation succeeded.

## No AGENTS.md in repo

`AGENTS.md` does not exist. Style guidance lives in `.github/copilot-instructions.md` (treated as authoritative per CLAUDE.md) and the root `CLAUDE.md`.

## .editorconfig

`Enterprise/Subsystem/ProductLauncher/.editorconfig` only enables `Serilog003` as error. No other formatting/lint rules to honor.
