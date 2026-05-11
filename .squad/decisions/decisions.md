# Decisions Log

## Decision 1: Path Truncation and Tray Icon Ownership

**Date:** 2026-04-26
**Session:** 8
**Agents:** Han (Frontend Dev)
**Status:** APPROVED & IMPLEMENTED

---

### Decision 1.1: Long File Paths in UI

**Context:**
Long Windows paths like `d:\aitourfy26\aitour26-BRK445-building-enterprise-ready-ai-agents-with-azure-ai-foundry\src\` overflow UI windows (MainWindow, MiniMonitorWindow). Users cannot read the Working Folder path.

**Decision:**
Use **middle-ellipsis truncation** for long paths via Win32 PathCompactPathEx (with managed fallback).

**Rationale:**
- Win32 PathCompactPathEx: built into Windows, zero dependencies, returns "C:\foo\...\baz\file.txt"
- Segment-based fallback: works cross-platform (split on DirectorySeparatorChar, keep first 1-2 + last 1-2 segments)
- No third-party NuGet packages needed (Humanizer does NOT support path truncation)
- WPF TextTrimming="CharacterEllipsis" only trims the END (drops the most informative part — the deepest folder)

**Implementation:**
1. Created `Helpers/PathHumanizer.cs` with Win32 P/Invoke + managed fallback
2. Added derived properties: `ProjectFolderDisplay` (MainViewModel, 50 chars), `WorkingFolderDisplay` (MiniMonitorViewModel, 35 chars)
3. XAML binds to `*Display` properties, ToolTip binds to full path

**Alternative Considered:**
- **WPF TextTrimming="CharacterEllipsis"** → Rejected (drops deepest folder, the most informative part)
- **Humanizer NuGet** → Rejected (does NOT have path truncation feature)
- **Custom middle-ellipsis only** → Partial (used as fallback, but Win32 is preferred)

**Files:**
- `src/ElBruno.AspireMonitor/Helpers/PathHumanizer.cs`
- `src/ElBruno.AspireMonitor/ViewModels/MainViewModel.cs`
- `src/ElBruno.AspireMonitor/ViewModels/MiniMonitorViewModel.cs`
- `src/ElBruno.AspireMonitor/Views/MainWindow.xaml`
- `src/ElBruno.AspireMonitor/Views/MiniMonitorWindow.xaml`

---

### Decision 1.2: NotifyIcon Ownership Model

**Context:**
User reported seeing TWO tray icons every launch. Investigation revealed:
- App.xaml had `StartupUri="Views/MainWindow.xaml"` → auto-creates MainWindow
- MainWindow.xaml.cs creates NotifyIcon in constructor
- If MainWindow is instantiated multiple times (show/hide cycles), multiple NotifyIcons appear

**Decision:**
Move NotifyIcon ownership from **MainWindow** to **App.xaml.cs** (process-scoped).

**Rationale:**
- **OllamaMonitor Pattern:** NotifyIcon is created once in App.OnStartup, disposed in App.OnExit
- MainWindow is a UI view, NOT the owner of system tray state
- App.xaml.cs owns process-level resources (NotifyIcon, polling services)
- Prevents duplicate icons on MainWindow show/hide cycles

**Implementation:**
1. Removed `StartupUri="Views/MainWindow.xaml"` from App.xaml
2. Moved NotifyIcon initialization from MainWindow.xaml.cs to App.xaml.cs:OnStartup
3. App.xaml.cs now manages:
   - NotifyIcon creation/disposal
   - Context menu (Details, Mini Monitor, Settings, GitHub, Exit)
   - Icon updates based on ViewModel.OverallStatusColor
   - Delegates to MainWindow for ToggleMiniMonitor, ShowSettings (via reflection)
4. MainWindow no longer creates or disposes NotifyIcon

**Alternative Considered:**
- **Keep NotifyIcon in MainWindow** → Rejected (causes duplicate icons if MainWindow is reinstantiated)
- **Static NotifyIcon field in MainWindow** → Rejected (violates separation of concerns; App.xaml.cs is the right owner)

**Reference:**
OllamaMonitor: `src/ElBruno.OllamaMonitor/App.xaml.cs` (lines 45-62: TrayIconService initialization)

**Files:**
- `src/ElBruno.AspireMonitor/App.xaml`
- `src/ElBruno.AspireMonitor/App.xaml.cs`
- `src/ElBruno.AspireMonitor/Views/MainWindow.xaml.cs`

---

## Validation

**Build:** ✅ Clean (2 warnings: unrelated to tray icon)
**Tests:** ✅ 260/260 passed (updated WorkingFolderTests for "(no working folder set)" message)
**Launch:** ✅ ONE process, ONE tray icon confirmed

**Deployment:** Ready for Phase 4 integration testing

---

## Decision 2: v1.2.0 Release — Pinned Resources & UI Improvements

**Date:** 2026-04-26  
**Author:** Leia (Lead)  
**Status:** ✅ SHIPPED  
**Version:** 1.2.0  
**Commit:** 5fd4d41  
**Tag:** v1.2.0  

### Summary

Released v1.2.0 of ElBruno.AspireMonitor with significant mini window enhancements (pinned resources, auto-resize), critical Aspire integration fixes (dashboard token preservation, CLI path), and UI polish (transparent tray icons, removed unused columns).

### Release Artifacts

**GitHub Release:**  
https://github.com/elbruno/ElBruno.AspireMonitor/releases/tag/v1.2.0

**NuGet Package (pending push):**  
- Package ID: `ElBruno.AspireMonitor`
- Version: `1.2.0`
- Files: `ElBruno.AspireMonitor.1.2.0.nupkg` (103,817 bytes), `ElBruno.AspireMonitor.1.2.0.snupkg` (38,287 bytes)
- Both attached to GitHub release as assets

**Commit:** `5fd4d412f40c4ca828dc5bc693bd696debd5450a`

### What Shipped (v1.1.0 → v1.2.0)

#### ✨ Features

1. **Configurable Pinned Resources (Mini Window)**
   - Users can pin specific Aspire resources to always display in mini window
   - Pinned resources remain visible even when resource is not running
   - Commit: beda7cb

2. **Auto-Resize Mini Window Height**
   - Mini window dynamically adjusts height based on number of displayed resources
   - Eliminates wasted space and scrolling issues
   - Commit: d90c563

3. **Dashboard Link in Mini Window**
   - Quick access to Aspire dashboard from mini window
   - Commit: 60eedb4

#### 🐛 Fixes

1. **Dashboard Token Preservation**
   - Dashboard URLs now retain authentication tokens in query string
   - Fixes broken authenticated dashboard links
   - Commit: ffec33e

2. **Aspire Stop Recovery (Mini Window)**
   - Mini window gracefully handles Aspire service stops
   - Provides clear feedback and recovery options
   - Commit: 60eedb4

3. **Remove Unused CPU/Memory Columns (Main Window)**
   - Cleaned up main window to remove non-functional columns
   - Start/Stop buttons now properly gate on connection state
   - Commit: cd41186

4. **Transparent Tray Icon Background**
   - System tray icons now properly display with transparent backgrounds
   - Fixes visual artifacts on light/dark taskbars
   - Commit: 349223d

5. **AspireCliService Path Fix**
   - CLI commands now execute from configured ProjectFolder
   - Fixes directory-dependent Aspire operations
   - Commit: faca7fd

#### 🧪 Tests

- Added comprehensive tests for pinned resources feature (commit 71f7ed7)
- **Total:** 273 tests passing (100% success rate)

#### 📚 Documentation

- Multiple squad documentation updates (orchestration logs, decisions merge, history)
- Design documentation for tray icon transparency fix
- README refresh with real app icon references

### Quality Metrics

- **Tests:** 273/273 passing (100%)
- **Build:** Zero errors (2 nullable warnings in test code only)
- **Configuration:** Release
- **Package Size:** 103.8 KB (.nupkg), 38.3 KB (.snupkg)

### Release Process

1. ✅ Version bumped in .csproj (1.1.0 → 1.2.0)
2. ✅ Build successful (Release mode)
3. ✅ All 273 tests passed
4. ✅ NuGet packages created (.nupkg and .snupkg)
5. ✅ Committed version bump (5fd4d41)
6. ✅ Pushed to GitHub (main branch)
7. ✅ Tagged v1.2.0 and pushed tag
8. ✅ GitHub release created with comprehensive notes
9. ⏳ NuGet.org push pending (Bruno's API key required)

### Manual NuGet Push Required

No NUGET_API_KEY found in environment or NuGet.Config.

**Bruno — To publish to NuGet.org:**

```powershell
# Push main package
dotnet nuget push .\artifacts\ElBruno.AspireMonitor.1.2.0.nupkg `
  -s https://api.nuget.org/v3/index.json `
  -k <YOUR_API_KEY> `
  --skip-duplicate

# Push symbols package
dotnet nuget push .\artifacts\ElBruno.AspireMonitor.1.2.0.snupkg `
  -s https://api.nuget.org/v3/index.json `
  -k <YOUR_API_KEY> `
  --skip-duplicate
```

**Verify after push (may take 5-10 minutes to index):**  
https://www.nuget.org/packages/ElBruno.AspireMonitor/1.2.0

### Breaking Changes

None — all changes backward-compatible with v1.1.0.

### Known Limitations

- Windows-only (WPF dependency)
- Polling model (not real-time push)
- .NET 10 required
- Dashboard token preservation requires Aspire CLI 0.3+ (most common version)

### Team Contributions

- **Han (Frontend):** Mini window pinned resources UI, auto-resize logic, dashboard link
- **Luke (Backend):** Dashboard token URL handling, CLI path fix, Aspire stop recovery
- **Yoda (Testing):** Pinned resources test suite (273 tests total)
- **Lando (Design):** Transparent tray icon background fix
- **Leia (Lead):** Release coordination, version bump, GitHub release, NuGet packaging

### Sign-Off

✅ **v1.2.0 APPROVED AND RELEASED**

**Signed:** Leia (Lead & Release Manager)  
**Date:** 2026-04-26  
**Release URL:** https://github.com/elbruno/ElBruno.AspireMonitor/releases/tag/v1.2.0  
**Tag:** v1.2.0  
**Commit:** 5fd4d412f40c4ca828dc5bc693bd696debd5450a

---

## Decision 3: What's New Documentation for Aspire 13.3 Alignment

**Date:** 2026-05-10  
**Author:** Chewie (DevRel/Docs)  
**Status:** ✅ IMPLEMENTED  
**Type:** Documentation Strategy

---

### Context

ElBruno.AspireMonitor v1.6.0 aligns with Aspire 13.3, bringing:
- Standard dashboard endpoint alignment
- Rich resource telemetry (type, disk, endpoints, environment badges)
- Maintained sample harness for validation

Users need clear guidance on:
1. What's new in the monitor
2. How it relates to Aspire 13.3 features
3. Upgrade path for existing users

---

### Decision

**Create a two-tiered "What's New" documentation structure:**

#### Tier 1: Root README
- **Location:** `README.md` → "✨ What's New in v1.6.0" section
- **Content:** 3 bullet points (dashboard alignment, telemetry, sample harness)
- **Purpose:** Immediate visibility; link to detailed guide
- **Placement:** After "What It Does", before "Features" table

#### Tier 2: Detailed Guide
- **Location:** `docs/whats-new.md` (NEW file)
- **Content:** 6.5 KB comprehensive guide including:
  - Detailed explanations of each new capability
  - Why each matters to the user
  - Tables showing telemetry examples
  - Feature alignment matrix (Aspire 13.3 → Monitor)
  - Upgrade guide for v1.5.0 → v1.6.0 users
  - Quick start with v1.6.0
  - Links to configuration and reference docs

#### Supporting Updates
- **docs/README.md:** Added whats-new.md to structure and links
- **root README:** Added whats-new.md to documentation links list

---

### Rationale

#### Why Two Tiers?
1. **Root README** — Discoverable, engaging, drives to fuller content
2. **Detailed Guide** — In-depth context, tables, upgrade guidance, feature matrices
3. **Separation of concerns** — Keeps root README concise; docs folder has rich content

#### Why This Structure?
- Grounded in **official sources** (https://aspire.dev/whats-new/aspire-13-3/)
- **Honest capability mapping** — Distinguishes "Monitor exposes" vs. "View on dashboard" vs. "Use CLI"
- **User-centric** — Explains "Why it matters" for each feature
- **Practical** — Includes upgrade path and configuration references

#### Feature Alignment Principle
Not every Aspire 13.3 feature is exposed by the monitor. Honesty builds trust:

| Aspire 13.3 Feature | Monitor Support | Notes |
|---|---|---|
| `aspire destroy` | ℹ️ See CLI | Direct CLI usage |
| Browser logs & screenshots | 📊 Dashboard | View in main dashboard |
| `aspire deploy` to Kubernetes | 📊 Dashboard | Tracked as resources |
| Standard dashboard endpoint | ✅ Implemented | **Monitor feature** |
| Richer resource metadata | ✅ Implemented | **Monitor feature** |

This prevents overclaiming and sets accurate expectations.

---

### Files Changed

1. **README.md**
   - Added "✨ What's New in v1.6.0" section
   - Updated documentation links to include whats-new.md

2. **docs/README.md**
   - Added whats-new.md to "Getting Started" section
   - Updated documentation structure tree
   - Marked as "NEW!"

3. **docs/whats-new.md** (NEW)
   - 6,544 bytes
   - Comprehensive guide with tables, matrices, upgrade guidance

---

### References

- **Official Source:** https://aspire.dev/whats-new/aspire-13-3/
- **Release Notes:** docs/releases/RELEASE-v1.6.0.md
- **CHANGELOG:** CHANGELOG.md (v1.6.0 entry)

---

### Success Criteria

✅ "What's New in v1.6.0" section visible on root README  
✅ Comprehensive guide at `docs/whats-new.md` with tables and matrices  
✅ All documentation links updated and tested  
✅ Feature alignment is honest (no overclaiming)  
✅ Upgrade path documented for existing users  
✅ Links to official Aspire 13.3 page provided  

---

### Follow-Up

- Monitor GitHub Issues for user questions about new features
- Track adoption of telemetry features in analytics (if available)
- Update guide as monitor features expand in future releases
- Coordinate with Release Manager (Leia) for NuGet release notes

---

**Chewie**  
DevRel/Docs, ElBruno.AspireMonitor

---

## Decision 3.1: What's New Documentation — QA Review & Approval

**Date:** 2026-05-10  
**Reviewer:** Yoda (QA/Tester)  
**Status:** ✅ APPROVED  
**Type:** Quality Gate

---

### Review Scope

- Dashboard endpoint language and implementation accuracy
- Resource telemetry descriptions vs. actual codebase
- SampleHarness composition claims
- Container tunnel and JavaScript publishing support claims
- Feature alignment matrix truthfulness

### Corrections Applied

✅ **Dashboard endpoint language** — Narrowed to: "app defaults to the local Aspire dashboard URL documented by Aspire 13.3 and preserves user overrides"

✅ **Resource telemetry descriptions** — Updated to: "disk usage as a percentage and environment badges as compact environment-variable summaries"

✅ **SampleHarness composition** — Corrected to: "two API services plus one worker service, not database/cache/background-job coverage"

✅ **Container tunnel & JavaScript publishing support** — Removed implications of dedicated app integration beyond resources exposed by Aspire CLI output

---

### Approval

Docs approved for merge into v1.6.0 release. Chewie remains unblocked for future revisions.

---

**Yoda**  
QA/Tester, ElBruno.AspireMonitor

---

## Decision 4: Mini Monitor Main-Resource Filter & v1.7.0 Implementation

**Date:** 2026-05-11  
**Author:** Leia (Lead) — Planning; Luke (Backend) — Filter Logic; Yoda (QA) — Tests; Han (Frontend) — UI  
**Status:** ✅ IMPLEMENTED & VALIDATED  
**Version:** 1.7.0  

### Summary

Implemented configurable resource filtering for the mini monitor to default to showing only endpoint-bearing resources (executables, containers with endpoints, databases with endpoints). Users can toggle `ShowOnlyMainMiniWindowResources` setting to display all resource types, including no-endpoint containers and data-only services.

### Context

Aspire projects often include duplicate resources (same ASP.NET project appears as both `{project-name}` executable with endpoints and `{project-name}-no-endpoint` container without). Mini monitor displayed both by default, cluttering the pinned resource list. Setting allows users to:
- **Default (`true`):** Show only main resources with endpoints
- **Toggle (`false`):** Restore previous behavior (show all resources)

### Decision

**Core Feature:** Add `ShowOnlyMainMiniWindowResources: bool` configuration property  
**Default:** `true` (endpoint-bearing resources only)  
**Behavior:** Filter applied AFTER pinned resource matching (preserves user-pinned resources)  
**UI:** SettingsWindow checkbox to toggle behavior  
**Version:** 1.7.0  

### Implementation Details

#### Phase 1: Configuration & Filtering Service
- ✅ Added `ShowOnlyMainMiniWindowResources: bool` property to `Configuration.cs` (default: `true`)
- ✅ Created `ResourceFilterService.cs` with `IsMainResource()` method
  - Returns `true` if resource has endpoints (`EndpointCount > 0` or `HasUrl`)
  - Returns `true` if `ShowOnlyMainMiniWindowResources` is `false` (shows all)
- ✅ Bumped version: 1.6.0 → 1.7.0 in `.csproj`

#### Phase 2: ViewModel Integration
- ✅ Updated `MainViewModel.Resources` property to apply `ResourceFilterService.IsMainResource()` filter
- ✅ `SettingsViewModel` loads/saves `ShowOnlyMainMiniWindowResources`
- ✅ `MiniMonitorViewModel` refreshes when filter setting changes

#### Phase 3: UI
- ✅ Added CheckBox to `SettingsWindow.xaml`
- ✅ Bound to `SettingsViewModel.ShowOnlyMainMiniWindowResources`
- ✅ Tooltip: "Show all resource types, including executables without endpoints and data-only containers"

#### Phase 4: Testing
- ✅ Unit tests for `ResourceFilterService.IsMainResource()`
- ✅ Integration tests for settings persistence
- ✅ UI tests for mini monitor refresh when filter is toggled
- ✅ Regression tests: Pinned resources still work with filter enabled

### Validation

**Build:** ✅ Clean (Release mode)  
**Tests:** ✅ 387/387 passing (updated test suite includes filter tests)  
**SampleHarness:** ✅ `aspire describe --format json` confirmed duplicate endpoint/no-endpoint shape  
**Behavior:** ✅ Mini monitor defaults to main resources; toggle shows all  

### Files Modified

- `src/ElBruno.AspireMonitor/Services/ResourceFilterService.cs` (NEW)
- `src/ElBruno.AspireMonitor/Models/Configuration.cs`
- `src/ElBruno.AspireMonitor/ViewModels/MainViewModel.cs`
- `src/ElBruno.AspireMonitor/ViewModels/SettingsViewModel.cs`
- `src/ElBruno.AspireMonitor/ViewModels/MiniMonitorViewModel.cs`
- `src/ElBruno.AspireMonitor/Views/SettingsWindow.xaml`
- `ElBruno.AspireMonitor.csproj` (version 1.7.0)
- `src/SampleHarness/SampleHarness/Program.cs` (filter validation harness)

### Commits

- 5a498fe: Implement main-resource filter and ResourceFilterService
- 5d23973: Add SampleHarness filter-probe-api for duplicate endpoint/no-endpoint validation

### Quality Metrics

- **Test Suite:** 387/387 passing (100%)
- **Code Coverage:** >80% (maintained)
- **Backward Compatibility:** ✅ Default behavior hides no-endpoint duplicates; toggle restores all
- **Release Ready:** ✅ All PR constraints met

### PR Readiness Checklist

- ✅ No hardcoded strings (ResourceFilterService uses resource properties)
- ✅ MVVM binding patterns (SettingsWindow → SettingsViewModel → MainViewModel)
- ✅ Configuration validated on load (JSON deserialization with defaults)
- ✅ Settings persisted correctly (JSON file system)
- ✅ Unit tests for filter logic
- ✅ Integration tests for settings persistence
- ✅ UI tests for mini monitor refresh
- ✅ Documentation: README.md and CHANGELOG.md updated

### Sign-Off

✅ **Implementation Complete & Validated**

**Leads:**
- **Leia:** Planning and version management (1.7.0)
- **Luke:** Filter logic and ResourceFilterService implementation
- **Yoda:** Test suite (387 tests, 100% passing) and QA validation
- **Han:** SettingsWindow UI binding and mini monitor refresh

**Commits:** 5a498fe, 5d23973  
**Version:** 1.7.0  
**Status:** Ready for merge to main

---

## Decision 4.1: Mini Monitor Telemetry Toggle

**Date:** 2026-05-10  
**Author:** Han (Frontend Dev)  
**Status:** ✅ IMPLEMENTED  
**Type:** UI Feature

### Decision

Add `ShowMiniWindowResourceTelemetry` setting to control visibility of compact telemetry rows on pinned mini-window resources.

**Default:** `true` (show telemetry)  
**Behavior:** Toggle controls only telemetry visibility; pinned resources remain visible as quick links

### Rationale

Pinned resources remain useful as quick links and missing-resource indicators even when telemetry is hidden. Gating only the telemetry row preserves existing pin behavior and avoids fake or stale metrics.

### Implementation

- ✅ Added `Configuration.ShowMiniWindowResourceTelemetry` property (serializes via JSON configuration)
- ✅ `SettingsViewModel` loads/saves the value
- ✅ `SettingsWindow` exposes toggle beside mini window resources
- ✅ `MainViewModel.ShowMiniWindowResourceTelemetry` notifies `MiniMonitorViewModel`
- ✅ `MiniMonitorViewModel` refreshes `MiniResourceItem.HasTelemetry` while keeping names, links, fallbacks, and missing states visible

### Sign-Off

✅ **Feature Implemented & Integrated**

**Han**  
Frontend Dev, ElBruno.AspireMonitor
