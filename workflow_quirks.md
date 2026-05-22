# Workflow Quirks — Jaswanthbommu-rp/unified-login perf-improver

## Repo default branch is the 2022 release branch

GitHub API reports `default_branch = 0e89acf1-on-release-2022.09.Q3.11.WMU` (a Q3 2022 release branch).
- **Active development branch is `master`** (PR #1 merged `gh-aw-perf-improver` into `master`).
- safeoutputs' base-branch resolution picks the API default, which would target the 2022 branch.
- Files diverge significantly between `master` and the 2022 default — many perf changes won't apply cleanly there.

**How to work around it:** branch off `origin/gh-aw-perf-improver` (the workflow's own branch — basically a snapshot of `master` at workflow setup time). Confirmed working in run 26283093839: `git checkout -b perf-assist/<desc> origin/gh-aw-perf-improver` → edit → commit → safeoutputs create_pull_request. The resulting patch is huge (~73 MB / 1.3 M lines because `master` and `gh-aw-perf-improver` share no merge-base) but safeoutputs accepts it.

## `master` and `gh-aw-perf-improver` share no common history

`git merge-base origin/gh-aw-perf-improver origin/master` returns empty. The two branches were grafted, not derived. PR #1 was a merge-without-common-ancestor. This is why the resulting patch sent to safeoutputs is enormous — it has to encode the entire delta between the two unrelated histories.

## safeoutputs PR patch generation

`/home/runner/work/_temp/gh-aw/safeoutputs/generate_git_patch.cjs` uses `git format-patch GITHUB_SHA..HEAD` (Strategy 2) when GITHUB_SHA is an ancestor of HEAD. With this repo's grafted history, the strategy still works but the resulting patch contains an entire alternate history.

## gh-aw v0.75.0 ENOENT bug (Run 1, 2026-05-22 09:27 UTC)

Run 1 hit `ENOENT: no such file or directory, open '/home/runner/work/_temp/gh-aw/md/workflow_install_note.md'` inside `messages_footer.cjs:134` (`getFooterInstallMessage`). The PR was never created on GitHub even though the local commit existed. The workflow opened issue #2 to report the failure.

Run 2 (2026-05-22 10:42 UTC) did **not** hit the same ENOENT — PR creation succeeded. So the bug was either transient or fixed by the workflow re-recompile in commit b743f80. Monitor future runs; if it recurs, the workaround is to manually create the file via `printf "" > /home/runner/work/_temp/gh-aw/md/workflow_install_note.md` before the safeoutputs call (untested, but the `renderTemplateFromFile` reads a literal file and an empty footer is acceptable).

## No AGENTS.md in repo

`AGENTS.md` does not exist. Style guidance lives in `.github/copilot-instructions.md` (treated as authoritative per CLAUDE.md) and the root `CLAUDE.md`.

## .editorconfig

`Enterprise/Subsystem/ProductLauncher/.editorconfig` only enables `Serilog003` as error. No other formatting/lint rules to honor.

## Memory can lie about PR state

Memory in run 1 claimed a PR was successfully created. It wasn't. Always cross-check with `mcp__github__list_pull_requests` and `git ls-remote origin 'refs/heads/perf-assist/*'` before acting on "in progress" entries in `backlog.md`.
