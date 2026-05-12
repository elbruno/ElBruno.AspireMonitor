roperties nullable in Configuration model

**Quality Metrics:**
- 50 unit tests for SettingsWindow/SettingsViewModel
- 100% of folder picker code paths tested
- 100% of URL validation tested
- Integration tests verify save/load roundtrip
- All tests passing ✅

---

### 2026-04-27 — Single-Instance Mutex Implementation (Session 7)

**Feature Scope:**
- Prevent multiple instances of ElBruno.AspireMonitor from running simultaneously
- Detect second instance at app startup, log and exit gracefully
- Use Windows named mutex for process-level singleton enforcement
- Ensure clean resource cleanup

**Implementation Completed:**

1. **Mutex Guard Pattern (App.xaml.cs)** ✅
   - Named mutex: `"ElBruno.AspireMonitor.SingleInstance"`
   - Check BEFORE base.OnStartup() and BEFORE any UI/resource initialization
   - Environment.Exit(0) on collision (not Shutdown() — Shutdown() is async and would allow resources to init)
   - Mutex held for entire app lifetime
   - Released in OnExit (lines 375-387)

2. **Critical Ordering** ✅
   - Lines 28-60: Single-instance check FIRST (before base.OnStartup)
   - Line 32: Create mutex with `new Mutex(true, MutexName, out isNewInstance)`
   - Line 34-60: If collision detected, log and exit immediately
   - Line 62: Only call base.OnStartup() if we own the mutex
   - Prevents duplicate NotifyIcon, MainWindow, services from being created

3. **Logging for Second Instance** ✅
   - Lines 38-54: Set up minimal FileLogger before exiting
   - Log path: `%LocalAppData%\ElBruno.AspireMonitor\logs\app-yyyyMMdd-HHmmss.log`
   - Log message: "Second instance detected at {timestamp}. Mutex '{MutexName}' is owned by existing process. Exiting immediately."
   - Allows user to grep logs to verify single-instance behavior

4. **Resource Cleanup** ✅
   - Lines 375-387: OnExit releases mutex via ReleaseMutex() and Dispose()
   - Try/catch to handle case where mutex already released
   - Log message: "Single-instance mutex released."

**Why Environment.Exit(0) vs Shutdown():**
- Shutdown() is async and triggers OnExit lifecycle events
- Would allow base.OnStartup() continuation and resource initialization
- Environment.Exit(0) terminates immediately (synchronous, clean exit code)
- Prevents any UI resources from being allocated in second instance

**Technical Pattern:**
```csharp
// 1. Mutex field at class level
private Mutex? _singleInstanceMutex;

// 2. Check BEFORE base.OnStartup()
protected override void OnStartup(StartupEventArgs e)
{
    bool isNewInstance;
    _singleInstanceMutex = new Mutex(true, MutexName, out isNewInstance);
    
    if (!isNewInstance)
    {
        // Log and exit immediately
        Environment.Exit(0);
        return;
    }
    
    base.OnStartup(e);  // Only if we own the mutex
    // ... rest of initialization
}

// 3. Release in OnExit
protected override void OnExit(ExitEventArgs e)
{
    _singleInstanceMutex?.ReleaseMutex();
    _singleInstanceMutex?.Dispose();
    base.OnExit(e);
}
```

**Quality Assurance:**
- Build: 0 errors, 1 pre-existing warning (CA2024) ✅
- Test suite: 260/260 passing ✅
- Code review: Mutex check happens at line 28-60 (before base.OnStartup at line 62) ✅
- Mutex lifetime: Held from OnStartup to OnExit ✅

**Manual Test Instructions (for user):**
1. Launch ElBruno.AspireMonitor.exe (first instance should run normally)
2. Launch ElBruno.AspireMonitor.exe again (second instance should exit immediately)
3. Check logs at `%LocalAppData%\ElBruno.AspireMonitor\logs\` for message containing "Second instance detected"

**Learnings:**
1. **Mutex Guard Pattern:** Check mutex ownership BEFORE base.OnStartup() and BEFORE any resource creation
2. **Environment.Exit(0) vs Shutdown():** Use Environment.Exit(0) for immediate termination without lifecycle events
3. **Mutex Lifetime:** Hold mutex for entire app lifetime, release only in OnExit
4. **Logging Second Instance:** Set up minimal file logger before exiting so behavior is observable
5. **Resource Safety:** Dispose mutex immediately if collision detected (lines 56-57) before exit

---

### 2026-04-26 — UI/UX Enhancements: Hidden Startup & Tray Settings (Session 6)

**Feature Scope:**
- Hide MainWindow on app startup (only system tray icon visible)
- Move Settings from MainWindow button to system tray context menu
- App runs silently in background until user clicks "Details"

**Implementation Completed:**

1. **App.xaml.cs - Startup Behavior** ✅
   - Modified OnStartup() to hide MainWindow on application launch
   - Set Visibility to Hidden and ShowInTaskbar to false
   - App runs in background with only system tray icon active
   - When user clicks "Details" from tray, MainWindow appears and becomes visible

2. **MainWindow.xaml - Removed Settings Button** ✅
   - Removed Settings button from control panel (was between "Mini Monitor" and "Close")
   - Settings now only accessible via tray menu
   - Control buttons now: Refresh | Mini Monitor | Close
   - Simplifies UI and encourages tray interaction

3. **MainWindow.xaml.cs - Tray Menu Enhancement** ✅
   - Added "Settings" menu item to system tray context menu
   - Position: Between "Mini Monitor" and first separator
   - Calls existing ShowSettings() method
   - New tray menu structure:
     * Details → Opens/restores main window
     * Mini Monitor → Toggles floating monitor panel
     * Settings → Opens settings dialog (via tray)
     * (separator)
     * GitHub → Opens repository in browser
     * (separator)
     * Exit → Clean shutdown

4. **Code Cleanup** ✅
   - Removed Settings_Click event handler (was button click handler in MainWindow.xaml.cs)
   - ShowSettings() method remains for tray menu integration
   - All event wiring updated and consistent

5. **Build Verification** ✅
   - Project builds successfully: 0 errors, 0 warnings
   - All XAML changes valid and compiled
   - Code-behind updates correct and complete

**Technical Decisions Made:**

1. **Silent Background Launch:**
   - MainWindow hidden from start, not minimized
   - Distinguishes from minimize behavior (can't see window in taskbar)
   - Users expect system tray apps to launch silently
   - Prevents accidental window focus on startup

2. **Settings in Tray Only:**
   - Removes UI clutter from MainWindow (one less button)
   - Encourages tray menu exploration
   - Consistent with typical system tray app patterns
   - Settings still easily accessible via tray right-click

**XAML/MVVM Patterns Reinforced:**

1. **Window Lifecycle:**
   - OnStartup() controls initial visibility (not XAML)
   - ShowWindow() method already existed, now primary entry point
   - Window remains in memory but hidden until needed

2. **Context Menu Structure:**
   - Menu mirrors primary features: Details (show window) + Monitor (mini) + Settings (config)
   - Separators group related functions (utility


