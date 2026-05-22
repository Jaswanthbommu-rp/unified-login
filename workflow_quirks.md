# Workflow Quirks — Jaswanthbommu-rp/unified-login perf-improver

## CRITICAL: branch off master HEAD, NOT origin/gh-aw-perf-improver

Earlier memory recommended branching off `origin/gh-aw-perf-improver`. That was WRONG and is the root cause of issue #3 (E003: max-patch-files exceeded, 2335 files).

`master` and `origin/gh-aw-perf-improver` share **no merge-base** — they were grafted, not derived. Branching off `gh-aw-perf-improver` makes safeoutputs' patch generator (`generate_git_patch.cjs`, Strategy 2: `git format-patch GITHUB_SHA..HEAD`) encode the entire delta between two unrelated histories. Result: ~73 MB patch, 2335 files, hard E003 rejection.

**Correct approach (confirmed working in run 26285110829):**

```
# GITHUB_SHA is master's tip when the workflow runs
git checkout -b perf-assist/<desc>  # branches off HEAD = master
# edit, commit
safeoutputs create_pull_request . < /tmp/gh-aw/agent/pr_body.json
# Result: small patch (e.g. 6.2 KB / 1 file / 79 lines), no E003
```

PR #5 (`gh-aw-base-branch-fix`, merged 2026-05-22 11:26 UTC) put `master` at HEAD for the workflow runner. Don't undo that by checking out a different ref.

## Repo default branch is the 2022 release branch

GitHub API reports `default_branch = 0e89acf1-on-release-2022.09.Q3.11.WMU` (a Q3 2022 release branch). **Active development branch is `master`**. The workflow now correctly checks out `master`, so this no longer affects PRs.

## gh-aw v0.75.0 ENOENT bug (Run 1 only, 2026-05-22 09:27 UTC)

Run 1 hit `ENOENT: no such file or directory, open '/home/runner/work/_temp/gh-aw/md/workflow_install_note.md'` inside `messages_footer.cjs:134`. Run 2 and run 3 did not reproduce. If it recurs, the workaround is to create the file via `printf "" > /home/runner/work/_temp/gh-aw/md/workflow_install_note.md` before the safeoutputs call.

## safeoutputs PR limits to remember

- `max-patch-files: 100` by default — keep PRs small/focused
- Patch generator uses `git format-patch GITHUB_SHA..HEAD` when GITHUB_SHA is an ancestor of HEAD

## No AGENTS.md in repo

`AGENTS.md` does not exist. Style guidance lives in `.github/copilot-instructions.md` (treated as authoritative per CLAUDE.md) and the root `CLAUDE.md`.

## .editorconfig

`Enterprise/Subsystem/ProductLauncher/.editorconfig` only enables `Serilog003` as error. No other formatting/lint rules to honor.

## Memory can lie about PR state

Memory in run 1 AND run 2 both claimed a PR was successfully created — neither was true. Always cross-check before acting on "in progress" entries in `backlog.md`:
- `mcp__github__list_pull_requests` for the actual remote PR list
- `git ls-remote origin 'refs/heads/perf-assist/*'` for branch-on-remote check
- Look for an open issue titled `[perf-improver] perf(...)` — safeoutputs creates a warning issue when a PR creation fails (e.g. E003)
