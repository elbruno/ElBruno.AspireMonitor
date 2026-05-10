# WPF Tray Icon Ownership Pattern

**Category:** UI Architecture
**Language/Framework:** WPF .NET
**Related:** System Tray, NotifyIcon, Application Lifecycle

---

## Problem

When building WPF applications with system tray integration, developers often create `NotifyIcon` in the `MainWindow` constructor. This causes duplicate tray icons when:
- `MainWindow` is instantiated multiple times (show/hide cycles)
- Multiple windows are created during application lifecycle
- `App.xaml` uses `StartupUri`, auto-creating `MainWindow` before manual instantiation

**Symptoms:**
- User sees 2+ tray icons in the system tray
- Icons persist even after closing windows
- Memory leaks from undisposed `NotifyIcon` instances

---

## Solution

Move `NotifyIcon` ownership from `MainWindow` to `App.xaml.cs` (application-level).

**Pattern:**

1. **Remove `StartupUri` from App.xaml:**
   ```xml
   <Application x:Class="MyApp.App"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
       <!-- NO StartupUri here -->
   </Application>
   ```

2. **Create `NotifyIcon` in `App.OnStartup`:**
   ```csharp
   public partial class App : Application
   {
       private NotifyIcon? _notifyIcon;
       private MainWindow? _mainWindow;

       protected override void OnStartup(StartupEventArgs e)
       {
           base.OnStartup(e);

           // Create MainWindow explicitly
           _mainWindow = new MainWindow();
           MainWindow = _mainWindow;
           MainWindow.Visibility = Visibility.Hidden; // Start in tray
           MainWindow.ShowInTaskbar = false;

           // Initialize tray icon ONCE (process-scoped)
           InitializeSystemTray();
       }

       private void InitializeSystemTray()
       {
           _notifyIcon = new NotifyIcon
           {
               Text = "My Application",
               Icon = SystemIcons.Application, // or load from resources
               Visible = true
           };

           var contextMenu = new ContextMenuStrip();
           contextMenu.Items.Add("Show", null, (s, e) => ShowMainWindow());
           contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());

           _notifyIcon.ContextMenuStrip = contextMenu;
           _notifyIcon.MouseDoubleClick += (s, e) => ShowMainWindow();
       }

       private void ShowMainWindow()
       {
           if (_mainWindow != null)
           {
               _mainWindow.Show();
               _mainWindow.WindowState = WindowState.Normal;
               _mainWindow.Activate();
           }
       }

       private void ExitApplication()
       {
           _mainWindow?.Close();
           _notifyIcon?.Dispose();
           Shutdown();
       }

       protected override void OnExit(ExitEventArgs e)
       {
           _notifyIcon?.Dispose();
           base.OnExit(e);
       }
   }
   ```

3. **Remove `NotifyIcon` from MainWindow.xaml.cs:**
   ```csharp
   public partial class MainWindow : Window
   {
       // NO NotifyIcon field here
       public MainWindow()
       {
           InitializeComponent();
           // No tray icon initialization
       }
   }
   ```

---

## Benefits

- **Single Tray Icon:** Only one `NotifyIcon` instance per application lifetime
- **Clean Lifecycle:** Dispose on `App.OnExit`, not dependent on window closure
- **Separation of Concerns:** `MainWindow` is just a UI view; `App` manages process-level resources
- **Matches System Behavior:** Tray icon persists independently of window state

---

## Reference Implementations

- **ElBruno.OllamaMonitor:** `src/ElBruno.OllamaMonitor/App.xaml.cs` (TrayIconService pattern)
- **ElBruno.AspireMonitor:** `src/ElBruno.AspireMonitor/App.xaml.cs` (NotifyIcon owned by App)

---

## Common Mistakes

1. ❌ Creating `NotifyIcon` in `MainWindow` constructor
2. ❌ Using `StartupUri` in `App.xaml` (causes auto-instantiation)
3. ❌ Not disposing `NotifyIcon` in `App.OnExit`
4. ❌ Forgetting to set `MainWindow.ShowInTaskbar = false` for tray-only apps

---

## When to Use

- WPF applications with system tray integration
- Background/daemon-style apps (minimize to tray, not taskbar)
- Single-instance applications with show/hide window behavior

---

## Related Patterns

- **Singleton Window Pattern:** Ensure only one `MainWindow` instance exists
- **Service Locator Pattern:** Access `NotifyIcon` from other components via `Application.Current` cast
- **MVVM Pattern:** Bind tray icon updates to ViewModel property changes (e.g., status color)

