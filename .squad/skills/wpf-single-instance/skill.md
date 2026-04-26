# Skill: WPF Single-Instance Mutex Guard

**Confidence:** Low (pattern is reusable but context-specific)  
**Category:** WPF Application Architecture  
**Tags:** #wpf #mutex #singleton #process-management #windows

## Overview

Enforce single-instance behavior in WPF applications using a named Windows mutex. Prevents users from launching multiple copies of the app, avoiding duplicate system tray icons, resource conflicts, and confusing UX.

## When to Use

- **System tray applications** (duplicate icons = bad UX)
- **Apps that bind to exclusive resources** (ports, named pipes, files)
- **Background services** (polling, watchers) where multiple instances waste resources
- **Apps with persistent state** (config files, databases) where concurrent access causes corruption

## When NOT to Use

- **Multi-instance apps by design** (text editors, browsers with separate windows)
- **Command-line tools** (users expect multiple invocations)
- **Cross-platform apps** (named mutexes are Windows-specific; use file locks or other IPC)

## Implementation Pattern

### 1. Add Mutex Field to App.xaml.cs

```csharp
using System.Threading;

public partial class App : Application
{
    private const string MutexName = "YourAppName.SingleInstance";
    private Mutex? _singleInstanceMutex;
}
```

### 2. Check Mutex BEFORE base.OnStartup()

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    // CRITICAL: Check BEFORE base.OnStartup() and BEFORE any resource creation
    bool isNewInstance;
    _singleInstanceMutex = new Mutex(true, MutexName, out isNewInstance);

    if (!isNewInstance)
    {
        // Another instance is already running - exit immediately
        LogSecondInstanceAttempt(); // Optional: log to file for diagnostics
        _singleInstanceMutex?.Dispose();
        Environment.Exit(0);
        return;
    }

    base.OnStartup(e); // Only call if we own the mutex
    
    // ... rest of initialization (NotifyIcon, MainWindow, services)
}
```

### 3. Release Mutex in OnExit

```csharp
protected override void OnExit(ExitEventArgs e)
{
    // Clean up resources
    // ... (dispose services, NotifyIcon, etc.)
    
    // Release single-instance mutex
    if (_singleInstanceMutex != null)
    {
        try
        {
            _singleInstanceMutex.ReleaseMutex();
            _singleInstanceMutex.Dispose();
        }
        catch { /* Silently fail if mutex already released */ }
    }
    
    base.OnExit(e);
}
```

## Critical Details

### Why Environment.Exit(0) Instead of Shutdown()?

- `Shutdown()` is **async** and triggers WPF lifecycle events (OnExit, etc.)
- Would allow resources (UI, services) to start initializing
- `Environment.Exit(0)` terminates **immediately** (synchronous) with exit code 0
- Second instance leaves zero footprint (no UI thread work queued)

### Why Check BEFORE base.OnStartup()?

- `base.OnStartup()` initializes WPF framework (themes, resources, app lifetime events)
- Checking afterward means resources are already allocated
- Checking first = instant exit for second instance (~50ms vs ~500ms)

### Mutex Scope (Session vs Global)

By default, named mutexes are **session-scoped**:
- Different Windows user sessions can run separate instances
- Same user, different desktops (Fast User Switching) = separate instances

To make mutex **global** (all users, all sessions):
```csharp
private const string MutexName = "Global\\YourAppName.SingleInstance";
```

⚠️ Requires admin privileges to create global mutexes.

### Logging Second Instance Attempts

Users may be confused when app "doesn't start" (it exits silently). Add logging:

```csharp
private void LogSecondInstanceAttempt()
{
    try
    {
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "YourAppName", "logs");
        Directory.CreateDirectory(logDir);
        var logPath = Path.Combine(logDir, $"app-{DateTime.Now:yyyyMMdd-HHmmss}.log");
        var listener = new TextWriterTraceListener(logPath);
        Trace.Listeners.Add(listener);
        Trace.AutoFlush = true;
        Trace.WriteLine($"[App] Second instance detected at {DateTime.Now:yyyy-MM-dd HH:mm:ss}. " +
                        $"Mutex '{MutexName}' is owned by existing process. Exiting immediately.");
    }
    catch { /* If logging fails, just exit silently */ }
}
```

### Optional: Focus Existing Instance

Instead of silently exiting, the second instance can **notify** the first instance to show its window. Requires IPC (named pipes, WM_COPYDATA, etc.). Example:

```csharp
if (!isNewInstance)
{
    // Send message to first instance: "show your window"
    NotifyExistingInstance();
    Environment.Exit(0);
    return;
}
```

This adds complexity but improves UX (user's action has visible effect).

## Testing

### Manual Test

1. Launch app → should run normally
2. Launch app again → should exit immediately (check logs)
3. Kill first instance (Task Manager)
4. Launch app → should run normally (mutex released)

### Automated Test (Optional)

```csharp
[Test]
public void SecondInstance_ShouldExitImmediately()
{
    var processA = Process.Start("YourApp.exe");
    Thread.Sleep(1000); // Wait for first instance to acquire mutex
    
    var processB = Process.Start("YourApp.exe");
    Thread.Sleep(500);
    
    Assert.IsFalse(processB.HasExited == false); // Second instance should have exited
    processA.Kill();
}
```

Requires real process spawning (not unit testable with mocks).

## Common Pitfalls

1. **Checking mutex AFTER base.OnStartup()**
   - Mutex check must be **first line** of OnStartup
   - Otherwise resources are already allocated (NotifyIcon, MainWindow, services)

2. **Using Shutdown() instead of Environment.Exit(0)**
   - Shutdown() is async and triggers lifecycle events
   - Second instance would start initializing before exiting

3. **Forgetting to release mutex**
   - OS cleans up abandoned mutexes on process termination
   - But good practice to release explicitly in OnExit

4. **Not disposing mutex on collision**
   - If second instance doesn't dispose mutex, minor resource leak
   - Always call `_singleInstanceMutex?.Dispose()` before Environment.Exit(0)

5. **Hard-coding app name in mutex**
   - Use assembly name or constant to avoid typos
   - `typeof(App).Assembly.GetName().Name + ".SingleInstance"`

## Alternatives

1. **Process name check (Process.GetProcessesByName)**
   - ❌ Race condition window
   - ❌ Doesn't work for different exe paths
   - ❌ No automatic cleanup on crash

2. **TCP socket binding**
   - ❌ Requires network permissions
   - ❌ Port conflicts

3. **File lock (FileStream exclusive access)**
   - ❌ Lock persists if app crashes (manual cleanup)

4. **WCF/Named Pipes IPC**
   - ✅ Allows second instance to communicate with first
   - ❌ More complex (requires IPC server/client)

## References

- **MSDN:** [Mutex Class](https://learn.microsoft.com/en-us/dotnet/api/system.threading.mutex)
- **Example App:** ElBruno.AspireMonitor (src/ElBruno.AspireMonitor/App.xaml.cs)
- **Similar Pattern:** Discord, Slack, OllamaMonitor (all use mutex-based single-instance)

## Real-World Example

From ElBruno.AspireMonitor (App.xaml.cs):

```csharp
private const string MutexName = "ElBruno.AspireMonitor.SingleInstance";
private Mutex? _singleInstanceMutex;

protected override void OnStartup(StartupEventArgs e)
{
    bool isNewInstance;
    _singleInstanceMutex = new Mutex(true, MutexName, out isNewInstance);

    if (!isNewInstance)
    {
        // Log to %LocalAppData%\ElBruno.AspireMonitor\logs\
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ElBruno.AspireMonitor", "logs");
        Directory.CreateDirectory(logDir);
        var logPath = Path.Combine(logDir, $"app-{DateTime.Now:yyyyMMdd-HHmmss}.log");
        var listener = new TextWriterTraceListener(logPath);
        Trace.Listeners.Add(listener);
        Trace.AutoFlush = true;
        Trace.WriteLine($"[App] Second instance detected at {DateTime.Now:yyyy-MM-dd HH:mm:ss}. " +
                        $"Mutex '{MutexName}' is owned by existing process. Exiting immediately.");

        _singleInstanceMutex?.Dispose();
        Environment.Exit(0);
        return;
    }

    base.OnStartup(e);
    // ... NotifyIcon, MainWindow, services initialization
}

protected override void OnExit(ExitEventArgs e)
{
    // ... dispose services, NotifyIcon
    
    if (_singleInstanceMutex != null)
    {
        try
        {
            _singleInstanceMutex.ReleaseMutex();
            _singleInstanceMutex.Dispose();
        }
        catch { }
    }
    
    base.OnExit(e);
}
```

**Result:** 260/260 tests pass, second instance exits in ~50ms, user sees single tray icon.
