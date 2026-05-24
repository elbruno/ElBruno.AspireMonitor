# Feature Proposal: Multi-Worktree Aspire Session Monitoring

**Date:** 2026-05-23  
**Author:** Leia (Lead)  
**Status:** PROPOSAL (Idea-Only)  
**Requested by:** Bruno Capuano

---

## Problem Statement

Developers using **git worktrees** for parallel job execution need to monitor multiple Aspire sessions simultaneously. Worktrees are created under a configurable base location (e.g., `C:\worktrees\MyProject-*`), and each may run an independent `aspire start` session. The current app monitors only a single project folder.

---

## Proposed Solution

### 1. Discovery Model

**Approach:** Scan a user-configured base path for active Aspire sessions.

| Component | Description |
|-----------|-------------|
| **Base Path** | User-configured root directory where worktrees live (e.g., `C:\worktrees`) |
| **Discovery Strategy** | Enumerate subdirectories and check for Aspire lock files or run `aspire describe` per candidate |
| **Lock File Detection** | Aspire CLI creates `.aspire/` metadata when a session is active — check for presence |
| **Fallback** | If no lock file schema is reliable, run `aspire describe --format json` with `WorkingDirectory` set to each candidate folder |

**New Service: `WorktreeDiscoveryService`**
- Accepts `BaseWorktreePath` from configuration
- Scans immediate child directories (depth=1, configurable)
- Returns list of `WorktreeSession` objects with folder paths and discovery status

---

### 2. Session Identity Model

Each monitored session needs a unique identity for UI correlation and state tracking.

```
WorktreeSession
├── SessionId          : GUID (generated at discovery, stable while session lives)
├── WorktreePath       : string (e.g., "C:\worktrees\feature-auth")
├── WorktreeName       : string (derived from folder name: "feature-auth")
├── DiscoveredAt       : DateTime
├── LastPolledAt       : DateTime
├── IsActive           : bool
├── Resources          : List<AspireResource>
└── DashboardUrl       : string (detected from aspire describe or default)
```

**Stability:** Session identity persists across polling cycles as long as the worktree folder exists and responds to `aspire describe`. When a session stops, it transitions to `IsActive = false` rather than being removed immediately (grace period for restart detection).

---

### 3. UI/UX Changes

**Main Window Enhancements:**

| Area | Change |
|------|--------|
| **Session Selector** | Add dropdown/tab strip above resource list to switch between active worktree sessions |
| **Session Badge** | Show session count in tray icon tooltip (e.g., "3 active sessions") |
| **Color Coding** | Apply existing status color scheme per-session; aggregate worst status for tray icon |
| **Mini Monitor** | Add session toggle in mini window header, or show consolidated view |

**New UI Components:**
- `SessionSelectorViewModel` — manages active sessions list and selected session
- `WorktreeSessionViewModel` — wraps `WorktreeSession` for binding
- Update `MainViewModel` to hold `ObservableCollection<WorktreeSessionViewModel>`

**Settings Panel:**
- New "Worktrees" section with:
  - `WorktreeBasePath` text field
  - `EnableWorktreeDiscovery` toggle (default: off for backward compatibility)
  - `WorktreeScanIntervalSeconds` slider (default: 30s)

---

### 4. Configuration Shape

Extend `Configuration.cs`:

```csharp
public class Configuration
{
    // Existing properties...

    // Worktree discovery settings
    public bool EnableWorktreeDiscovery { get; set; } = false;
    public string WorktreeBasePath { get; set; } = string.Empty;
    public int WorktreeScanIntervalSeconds { get; set; } = 30;
    public int MaxWorktreeSessions { get; set; } = 10;
}
```

**JSON representation:**
```json
{
  "enableWorktreeDiscovery": true,
  "worktreeBasePath": "C:\\worktrees",
  "worktreeScanIntervalSeconds": 30,
  "maxWorktreeSessions": 10
}
```

---

### 5. Refresh Behavior

**Two-tier polling architecture:**

| Tier | Responsibility | Default Interval |
|------|----------------|------------------|
| **Discovery Poll** | Scan base path for new/removed worktree sessions | 30 seconds |
| **Resource Poll** | Query `aspire describe` per active session | 5 seconds (existing `PollingIntervalMs`) |

**Implementation:**
- `WorktreeDiscoveryService` runs discovery on its own timer
- Each discovered session gets its own `AspireCliService` instance with `WorkingDirectory` set
- `AspirePollingService` refactored to manage multiple `AspireCliService` instances (or create `MultiSessionPollingService`)

**Resource Optimization:**
- Skip resource polling for sessions that failed discovery in the previous cycle
- Stagger polling across sessions to avoid CLI contention
- Cap total sessions via `MaxWorktreeSessions`

---

### 6. Failure Handling

| Scenario | Behavior |
|----------|----------|
| **Worktree deleted** | Remove session from list after 2 consecutive failed scans |
| **Aspire stopped in worktree** | Mark session `IsActive = false`; keep in list with "Stopped" status |
| **Base path inaccessible** | Surface error in Settings panel; disable discovery until resolved |
| **aspire describe timeout (single session)** | Backoff that session's polling; don't affect others |
| **Too many sessions** | Cap at `MaxWorktreeSessions`; show warning; newest sessions prioritized |

**Error Isolation:** Each session has independent error state — one failing worktree doesn't block others.

---

## Rollout Plan

### MVP (v1.7.0)

**Goal:** Prove multi-session monitoring works end-to-end.

- [ ] Add `EnableWorktreeDiscovery`, `WorktreeBasePath` to Configuration
- [ ] Create `WorktreeDiscoveryService` (basic folder enumeration)
- [ ] Create `WorktreeSession` model
- [ ] Modify `AspireCliService` to accept per-session `WorkingDirectory`
- [ ] Add session dropdown to MainWindow (basic switching, no fancy UI)
- [ ] Single polling timer iterates through sessions sequentially
- [ ] Basic failure handling (skip failed sessions, log errors)

**Constraints:**
- No mini-monitor multi-session support yet
- No persistent session history
- Discovery only on startup + manual refresh

---

### Iteration 2 (v1.8.0)

**Goal:** Polish UX and add real-time discovery.

- [ ] Add Settings panel UI for worktree configuration
- [ ] Automatic discovery polling (30s timer)
- [ ] Session count badge in tray icon
- [ ] Per-session status colors in dropdown
- [ ] Graceful session removal animation
- [ ] "Open Dashboard" respects selected session's detected URL

---

### Iteration 3 (v1.9.0)

**Goal:** Scale and optimize.

- [ ] Parallel resource polling (staggered)
- [ ] Mini-monitor session toggle
- [ ] Session health metrics (uptime, restart count)
- [ ] Export multi-session telemetry snapshot
- [ ] Optional: Deep scan mode (nested directories)

---

## Alignment with Existing Patterns

| Pattern | Alignment |
|---------|-----------|
| **MVVM** | New ViewModels for session selection; MainViewModel orchestrates |
| **CLI-centric** | All data via `aspire describe` — no direct dashboard polling |
| **Configuration** | Extends existing `Configuration` model and `ConfigurationService` |
| **Polling** | Builds on `AspirePollingService` architecture with multi-instance support |
| **Error handling** | Follows existing backoff/reconnect patterns per session |

---

## Open Questions (For Future Design)

1. Should sessions be persisted across app restarts (remember last-known worktrees)?
2. Should the app auto-detect worktree base path from git config?
3. Should there be a "monitor all" consolidated view vs. per-session switching?
4. How to handle worktrees on network paths with latency?

---

## Decision Requested

This proposal is **idea-only** — no implementation until approved. Requesting:
- ✅ Conceptual approval of discovery + multi-session architecture
- ✅ Agreement on MVP scope for v1.7.0
- ✅ Any constraints or must-haves before implementation begins

---

*Proposal authored by Leia. Pending team review.*
