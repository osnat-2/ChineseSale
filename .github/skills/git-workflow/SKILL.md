---
name: git-workflow
description: "Use when managing Git repositories: inspect status, branches, remotes, pull, fetch, merge, rebase, commit, push, tags, or GitHub pull requests. Preserve user changes, verify before publishing, and ask before destructive history operations."
---

# Git Workflow Skill

Own repository operations from inspection through publication. Work from the current repository state and treat every existing modification as user-owned unless the user explicitly says otherwise.

## Operating Rules

- Start with `git status --short --branch`, current branch, upstream, and remotes.
- Inspect the diff and untracked files before committing, especially when the worktree is dirty.
- Never discard, reset, clean, amend, force-push, or overwrite user work without explicit confirmation.
- Do not use `git add -A` by default. Stage only the files belonging to the requested change.
- Keep secrets, credentials, local settings, build output, and generated files out of commits.
- Prefer a linear, reviewable history. Use regular push unless the user explicitly approves force push.
- Do not commit or push merely because a change is complete. Confirm the intended commit scope and remote destination when it is ambiguous.

## Pull And Synchronization

1. Check for local changes and the configured upstream.
2. If local changes could conflict, explain the risk and either ask permission to stash or have the user commit first. Never stash silently.
3. Fetch before comparing remote state when network access is available.
4. Prefer `git pull --ff-only` when the local branch can advance cleanly.
5. If fast-forward is impossible, stop and present the divergence. Ask whether to merge or rebase; do not choose a history rewrite silently.
6. After conflicts, list conflicted files, resolve only requested files, and validate before continuing.

## Commit And Push

- Review `git diff`, `git diff --cached`, and the staged file list before committing.
- Use a concise imperative commit message describing the actual change.
- Run the narrowest relevant tests or build before committing when available.
- After committing, verify the commit with `git show --stat --oneline HEAD`.
- Push the named branch to its configured upstream and report the result. Do not force-push by default.
- After pushing, verify branch/upstream status and provide the commit hash and remote branch.

## Pull Requests

For a PR, inspect the branch relationship to its base, commits, and changed files. Prepare a title and body containing purpose, key changes, validation, and known limitations. Use the repository's configured GitHub tooling if available; otherwise provide the exact `gh pr create` command and ask for approval before publishing. Never claim that a PR exists until the creation command succeeds and its URL is returned.

## Reporting

At each state-changing step, report what changed, what was verified, and what remains. If a command fails, preserve the repository state, show the relevant error, and propose the smallest next step. Never hide conflicts, rejected pushes, authentication failures, or unverified assumptions.

## Useful Commands

```powershell
git status --short --branch
git remote -v
git branch -vv
git diff --stat
git diff --cached --stat
git fetch --prune
git log --oneline --decorate -n 10
```
