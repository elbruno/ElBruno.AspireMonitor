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
