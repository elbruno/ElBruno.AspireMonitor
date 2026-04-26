using System.Windows;
using System.Windows.Forms;
using ElBruno.AspireMonitor.Models;
using ElBruno.AspireMonitor.Services;
using ElBruno.AspireMonitor.ViewModels;
using ElBruno.AspireMonitor.Views;
using ElBruno.AspireMonitor.Infrastructure;
using System.Drawing;
using System.Windows.Media;
using WinDrawing = System.Drawing;
using System.Threading;

namespace ElBruno.AspireMonitor;

public partial class App : System.Windows.Application
{
    private const string MutexName = "ElBruno.AspireMonitor.SingleInstance";
    private Mutex? _singleInstanceMutex;
    private IAspirePollingService? _pollingService;
    private IConfigurationService? _configService;
    private AspireCliService? _cliService;
    private AspireLiveLogsService? _logsService;
    private IAspireCommandService? _commandService;
    private NotifyIcon? _notifyIcon;
    private MainWindow? _mainWindow;
    private System.Drawing.Icon? _currentIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        // CRITICAL: Single-instance check MUST be first, before any UI resource creation
        bool isNewInstance;
        _singleInstanceMutex = new Mutex(true, MutexName, out isNewInstance);

        if (!isNewInstance)
        {
            // Another instance is already running - exit immediately
            // Set up minimal logging first
            try
            {
                var logDir = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ElBruno.AspireMonitor", "logs");
                System.IO.Directory.CreateDirectory(logDir);
                var logPath = System.IO.Path.Combine(logDir, $"app-{DateTime.Now:yyyyMMdd-HHmmss}.log");
                var listener = new System.Diagnostics.TextWriterTraceListener(logPath) { Name = "FileLogger" };
                System.Diagnostics.Trace.Listeners.Add(listener);
                System.Diagnostics.Trace.AutoFlush = true;
                System.Diagnostics.Debug.WriteLine($"[App] Second instance detected - another instance is already running. Exiting.");
                System.Diagnostics.Trace.WriteLine($"[App] Second instance detected at {DateTime.Now:yyyy-MM-dd HH:mm:ss}. Mutex '{MutexName}' is owned by existing process. Exiting immediately.");
            }
            catch
            {
                // If logging fails, just exit silently
            }

            _singleInstanceMutex?.Dispose();
            _singleInstanceMutex = null;
            Environment.Exit(0);
            return;
        }

        base.OnStartup(e);

        // Mirror Debug/Trace output to a file so logs can be tailed without a debugger
        try
        {
            var logDir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ElBruno.AspireMonitor", "logs");
            System.IO.Directory.CreateDirectory(logDir);
            var logPath = System.IO.Path.Combine(logDir, $"app-{DateTime.Now:yyyyMMdd-HHmmss}.log");
            var listener = new System.Diagnostics.TextWriterTraceListener(logPath) { Name = "FileLogger" };
            System.Diagnostics.Trace.Listeners.Add(listener);
            System.Diagnostics.Trace.AutoFlush = true;
            System.Diagnostics.Debug.WriteLine($"[App] Log file: {logPath}");
        }
        catch (Exception logEx)
        {
            System.Diagnostics.Debug.WriteLine($"[App] Failed to set up file logging: {logEx.Message}");
        }

        System.Diagnostics.Debug.WriteLine("[App] ========== APPLICATION STARTUP ==========");
        System.Diagnostics.Debug.WriteLine("[App] Initializing CLI-based Aspire Monitor...");
        
        // Initialize configuration service
        _configService = new ConfigurationService();
        var configuration = _configService.LoadConfiguration();
        
        System.Diagnostics.Debug.WriteLine($"[App] Loaded configuration:");
        System.Diagnostics.Debug.WriteLine($"[App]   Polling Interval: {configuration.PollingIntervalMs}ms");
        
        // Initialize CLI service (NO HTTP)
        System.Diagnostics.Debug.WriteLine("[App] Creating CLI service...");
        _cliService = new AspireCliService();
        
        // Initialize live logs service
        System.Diagnostics.Debug.WriteLine("[App] Creating live logs service...");
        _logsService = new AspireLiveLogsService(_cliService);
        
        // Initialize polling service with CLI backend
        System.Diagnostics.Debug.WriteLine("[App] Creating polling service with CLI backend...");
        _pollingService = new AspirePollingService(_cliService, configuration.PollingIntervalMs);
        
        // Initialize command service
        _commandService = new AspireCommandService();
        
        System.Diagnostics.Debug.WriteLine("[App] Creating MainViewModel and MainWindow...");
        // Create MainViewModel and MainWindow with dependencies
        var viewModel = new MainViewModel(_pollingService, _configService, _commandService);
        _mainWindow = new MainWindow(_pollingService, _configService, viewModel, _commandService);
        
        // Set as application main window
        MainWindow = _mainWindow;
        
        // Hide the main window on startup (run silently in background)
        // Only the system tray icon should be visible
        MainWindow.Visibility = Visibility.Hidden;
        MainWindow.ShowInTaskbar = false;
        
        // Initialize system tray ONCE in App (not in MainWindow)
        InitializeSystemTray(viewModel);
        
        // Start polling
        System.Diagnostics.Debug.WriteLine("[App] Starting polling service...");
        _pollingService.Start();
        
        // Subscribe to ViewModel property changes for tray icon updates
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.OverallStatusColor))
            {
                UpdateTrayIcon(viewModel);
            }
        };
        
        // Subscribe to errors to show user notifications
        _pollingService.ErrorOccurred += (sender, error) =>
        {
            System.Diagnostics.Debug.WriteLine($"[App] Polling error: {error}");
            if (error.Contains("not found") || error.Contains("not running"))
            {
                System.Diagnostics.Debug.WriteLine("[App] Aspire CLI or app not available");
            }
        };
        
        System.Diagnostics.Debug.WriteLine("[App] ========== STARTUP COMPLETE (CLI MODE) ==========");
    }

    private void InitializeSystemTray(MainViewModel viewModel)
    {
        _notifyIcon = new NotifyIcon
        {
            Text = $"Aspire Monitor {VersionHelper.GetAppVersion()}",
            Visible = true
        };

        UpdateTrayIcon(viewModel);

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Details", null, (s, e) => ShowMainWindow());
        contextMenu.Items.Add("Mini Monitor", null, (s, e) => ToggleMiniMonitor());
        contextMenu.Items.Add("Settings", null, (s, e) => ShowSettings());
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("GitHub", null, (s, e) => OpenGitHub());
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.MouseDoubleClick += (s, e) => ToggleMiniMonitor();
    }

    private void UpdateTrayIcon(MainViewModel viewModel)
    {
        if (_notifyIcon == null)
            return;

        // Determine which logo to use based on status
        string logoPath = GetLogoPathForStatus(viewModel);
        
        // Dispose old icon if it exists
        if (_currentIcon != null)
        {
            try
            {
                _currentIcon.Dispose();
            }
            catch { }
        }
        
        _currentIcon = CreateIconFromPath(logoPath, viewModel);
        _notifyIcon.Icon = _currentIcon;
        _notifyIcon.Text = $"Aspire Monitor - {viewModel.ConnectionStatus}";
    }

    private string GetLogoPathForStatus(MainViewModel viewModel)
    {
        if (!viewModel.IsConnected)
            return "Resources/aspire_trayicon_norunning.png";

        var brush = viewModel.OverallStatusColor as SolidColorBrush;
        if (brush == null)
            return "Resources/aspire_trayicon_norunning.png";

        var color = brush.Color;

        // Match against known status colors
        var redColor = System.Windows.Media.Color.FromRgb(0xF4, 0x43, 0x36);
        var yellowColor = System.Windows.Media.Color.FromRgb(0xFF, 0xC1, 0x07);
        var greenColor = System.Windows.Media.Color.FromRgb(0x4C, 0xAF, 0x50);

        if (color == redColor)
            return "Resources/aspire_trayicon_error.png";
        if (color == yellowColor)
            return "Resources/aspire_trayicon_warning.png";
        if (color == greenColor)
            return "Resources/aspire_trayicon_running.png";

        return "Resources/aspire_trayicon_norunning.png";
    }

    private System.Drawing.Icon CreateIconFromPath(string resourcePath, MainViewModel viewModel)
    {
        try
        {
            var fileName = resourcePath.Replace('/', System.IO.Path.DirectorySeparatorChar);
            var fullPath = System.IO.Path.Combine(AppContext.BaseDirectory, fileName);

            if (System.IO.File.Exists(fullPath))
            {
                using var source = new Bitmap(fullPath);
                return CreateIconFromBitmap(source);
            }

            System.Diagnostics.Debug.WriteLine($"[App] Tray icon not found at: {fullPath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] Error loading PNG icon '{resourcePath}': {ex.Message}");
        }

        // Fallback to colored circle if PNG fails to load
        return CreateColoredIcon(GetStatusColor(viewModel));
    }

    private static System.Drawing.Icon CreateIconFromBitmap(Bitmap source)
    {
        const int size = 32;
        using var resized = new Bitmap(size, size);
        using (var graphics = Graphics.FromImage(resized))
        {
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphics.Clear(System.Drawing.Color.Transparent);
            graphics.DrawImage(source, 0, 0, size, size);
        }

        IntPtr hIcon = resized.GetHicon();
        return System.Drawing.Icon.FromHandle(hIcon);
    }

    private System.Drawing.Color GetStatusColor(MainViewModel viewModel)
    {
        if (!viewModel.IsConnected)
            return System.Drawing.Color.Gray;

        var brush = viewModel.OverallStatusColor as SolidColorBrush;
        if (brush != null)
        {
            var mediaColor = brush.Color;
            return System.Drawing.Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
        }

        return System.Drawing.Color.Gray;
    }

    private System.Drawing.Icon CreateColoredIcon(System.Drawing.Color color)
    {
        var bitmap = new Bitmap(16, 16);
        try
        {
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.Clear(System.Drawing.Color.Transparent);
                using (var brush = new WinDrawing.SolidBrush(color))
                {
                    graphics.FillEllipse(brush, 2, 2, 12, 12);
                }
                using (var pen = new WinDrawing.Pen(System.Drawing.Color.White, 1))
                {
                    graphics.DrawEllipse(pen, 2, 2, 12, 12);
                }
            }
            
            IntPtr hIcon = bitmap.GetHicon();
            var icon = System.Drawing.Icon.FromHandle(hIcon);
            return icon;
        }
        finally
        {
            bitmap?.Dispose();
        }
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

    private void ToggleMiniMonitor()
    {
        if (_mainWindow != null)
        {
            // Delegate to MainWindow's existing ToggleMiniMonitor logic
            var method = _mainWindow.GetType().GetMethod("ToggleMiniMonitor", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(_mainWindow, null);
        }
    }

    private void ShowSettings()
    {
        if (_mainWindow != null)
        {
            // Delegate to MainWindow's existing ShowSettings logic
            var method = _mainWindow.GetType().GetMethod("ShowSettings", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(_mainWindow, null);
        }
    }

    private void OpenGitHub()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/elbruno/ElBruno.AspireMonitor",
                UseShellExecute = true
            });
        }
        catch
        {
            // Silently fail if browser can't be opened
        }
    }

    private void ExitApplication()
    {
        _pollingService?.Stop();
        _mainWindow?.Close();
        _notifyIcon?.Dispose();
        _currentIcon?.Dispose();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[App] ========== APPLICATION SHUTDOWN ==========");
        
        // Clean up resources
        _pollingService?.Stop();
        (_pollingService as IDisposable)?.Dispose();
        _logsService?.Dispose();
        _notifyIcon?.Dispose();
        _currentIcon?.Dispose();
        
        // Release single-instance mutex
        if (_singleInstanceMutex != null)
        {
            try
            {
                _singleInstanceMutex.ReleaseMutex();
                _singleInstanceMutex.Dispose();
                System.Diagnostics.Debug.WriteLine("[App] Single-instance mutex released.");
            }
            catch
            {
                // Silently fail if mutex is already released
            }
        }
        
        base.OnExit(e);
    }
}
