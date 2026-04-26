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
