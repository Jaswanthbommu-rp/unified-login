# Workflow Quirks — Jaswanthbommu-rp/unified-login perf-improver

## CRITICAL #1: branch off master HEAD (= GITHUB_SHA), NOT origin/gh-aw-perf-improver

`master` and `origin/gh-aw-perf-improver` share **no merge-base** (grafted, not derived). Branching off `gh-aw-perf-improver` makes safeoutputs' patch generator (`git format-patch GITHUB_SHA..HEAD`) encode the full delta between unrelated histories → 73 MB patch, 2335 files, hard E003. Confirmed in runs 26285110829 + 26285981135: branching off HEAD = master = GITHUB_SHA produces small, clean patches (6.2 KB and 2.7 KB respectively). PR #5 (`gh-aw-base-branch-fix`, 2026-05-22 11:26 UTC) put master at HEAD for the workflow.

```
git checkout -b perf-assist/<desc>   # off HEAD
# edit + commit
safeoutputs create_pull_request . < /tmp/gh-aw/agent/pr_body.json
```

## CRITICAL #2: GitHub Actions cannot create real PRs in this repo

`mcp__github__list_pull_requests` always returns `[]` even after a successful safeoutputs call. The repo setting *Settings → Actions → General → Allow GitHub Actions to create and approve pull requests* is OFF. Safeoutputs falls back to: pushing the branch + opening a numbered **issue** with the patch + a `compare/master...<branch>?expand=1` link.

Memory + Monthly Activity rules:
- Never write "PR created" — write "branch pushed + issue-fallback opened".
- Source of truth for PR state: `mcp__github__list_pull_requests`, NOT the safeoutputs success JSON.
- In the Monthly Activity issue, suggested actions say "Create real PR from branch X" with the compare link, not "Review PR #N".
- Permanent fix: maintainer enables the Actions PR setting (tracked as a Suggested Action in #4).

## Default branch is the Q3 2022 release branch

GitHub API reports `default_branch = 0e89acf1-on-release-2022.09.Q3.11.WMU`. Active dev branch is `master`. The workflow now checks out `master`, so this no longer affects PRs.

## Legacy: gh-aw v0.75.0 ENOENT (Run 1 only)

ENOENT on `/home/runner/work/_temp/gh-aw/md/workflow_install_note.md` in `messages_footer.cjs:134`. Did not reproduce in runs 2–4. If it recurs: `printf "" > <that path>` before the safeoutputs call.

## Misc

- `max-patch-files: 100` is the safeoutputs default — keep PRs focused.
- No `AGENTS.md` in repo. Style guidance: `.github/copilot-instructions.md` (authoritative per CLAUDE.md) and root `CLAUDE.md`.
- `Enterprise/Subsystem/ProductLauncher/.editorconfig` enables only `Serilog003` as error.

## Memory hygiene

Memory in runs 1 and 2 falsely claimed PR success. Always cross-check before acting on `In-flight` entries:
- `mcp__github__list_pull_requests` — authoritative; in this repo always `[]`.
- `git ls-remote origin 'refs/heads/perf-assist/*'` — branch existence.
- `mcp__github__list_issues` — look for `[perf-improver] perf(...)` titles (these are the issue-fallbacks).
