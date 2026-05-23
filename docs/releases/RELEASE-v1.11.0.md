# Release Notes: ElBruno.AspireMonitor v1.11.0

**Release Date:** 2026-05-23
**Focus:** Worktree-aware Aspire session discovery and monitoring

## What's New

- Added optional worktree discovery mode to monitor multiple Aspire sessions running in parallel git worktrees.
- Discovery reads `git worktree list --porcelain`, then validates each worktree by checking root `aspire.config.json` and `appHost.path`.
- Aggregated resources are labeled with the worktree folder prefix (for example `feature-a/api`) to avoid name collisions.
- Added settings support for `enableWorktreeDiscovery` and `worktreeBasePath` with validation and live reload behavior.

## Validation

- Added backend tests for worktree directory discovery and multi-worktree resource aggregation.
- Added settings tests for loading/saving and validating worktree discovery options.
- Full solution tests pass with the new behavior and existing scenarios.

## Upgrade

```bash
dotnet tool update --global ElBruno.AspireMonitor
```

To enable worktree discovery, open Settings and set:
- Enable worktree discovery = true
- Worktree base path = your repo/worktrees root folder
