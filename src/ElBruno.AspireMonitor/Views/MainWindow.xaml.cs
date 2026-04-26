using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using ElBruno.AspireMonitor.ViewModels;
using ElBruno.AspireMonitor.Services;
using ElBruno.AspireMonitor.Infrastructure;
using WinDrawing = System.Drawing;

namespace ElBruno.AspireMonitor.Views;

public partial class MainWindow : Window
{
    private NotifyIcon? _notifyIcon;
    private MiniMonitorWindow? _miniMonitorWindow;
    private MainViewModel? ViewModel => DataContext as MainViewModel;
    private readonly IConfigurationService? _configService;
    private System.Drawing.Icon? _currentIcon;

    public MainWindow() : this(null, null, null, null)
    {
    }

    public MainWindow(IAspirePollingService? pollingService, IConfigurationService? configService, MainViewModel? viewModel = null, IAspireCommandService? commandService = null)
    {
        InitializeComponent();
        _configService = configService;
        DataContext = viewModel ?? new MainViewModel(pollingService, configService, commandService);
        InitializeSystemTray();
        
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            ViewModel.Start();
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.OverallStatusColor) && _notifyIcon != null)
        {
            UpdateTrayIcon();
        }
    }

    private void InitializeSystemTray()
    {
        _notifyIcon = new NotifyIcon
        {
            Text = $"Aspire Monitor {VersionHelper.GetAppVersion()}",
            Visible = true
        };

        UpdateTrayIcon();

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Details", null, (s, e) => ShowWindow());
        contextMenu.Items.Add("Mini Monitor", null, (s, e) => ToggleMiniMonitor());
        contextMenu.Items.Add("Settings", null, (s, e) => ShowSettings());
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("GitHub", null, (s, e) => OpenGitHub());
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.MouseDoubleClick += (s, e) => ToggleMiniMonitor();
    }

    private void UpdateTrayIcon()
    {
        if (_notifyIcon == null || ViewModel == null)
            return;

        // Determine which logo to use based on status
        string logoPath = GetLogoPathForStatus();
        
        // Dispose old icon if it exists
        if (_currentIcon != null)
        {
            try
            {
                _currentIcon.Dispose();
            }
            catch { }
        }
        
        _currentIcon = CreateIconFromSvgPath(logoPath);
        _notifyIcon.Icon = _currentIcon;
        _notifyIcon.Text = $"Aspire Monitor - {ViewModel.ConnectionStatus}";
    }

    private string GetLogoPathForStatus()
    {
        if (!ViewModel!.IsConnected)
            return "Resources/aspire-logo-gray.svg";

        var brush = ViewModel.OverallStatusColor as SolidColorBrush;
        if (brush == null)
            return "Resources/aspire-logo-gray.svg";

        var color = brush.Color;
        
        // Match against known status colors
        var redColor = System.Windows.Media.Color.FromRgb(0xF4, 0x43, 0x36);
        var yellowColor = System.Windows.Media.Color.FromRgb(0xFF, 0xC1, 0x07);
        var greenColor = System.Windows.Media.Color.FromRgb(0x4C, 0xAF, 0x50);

        if (color == redColor)
            return "Resources/aspire-logo-red.svg";
        if (color == yellowColor)
            return "Resources/aspire-logo-yellow.svg";
        if (color == greenColor)
            return "Resources/aspire-logo-green.svg";

        return "Resources/aspire-logo-gray.svg";
    }

    private System.Drawing.Icon CreateIconFromSvgPath(string resourcePath)
    {
        try
        {
            // Try to load from resources
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream($"ElBruno.AspireMonitor.{resourcePath.Replace('/', '.')}")
                ?? assembly.GetManifestResourceStream($"ElBruno.AspireMonitor.Resources.{System.IO.Path.GetFileName(resourcePath)}"))
            {
                if (stream != null)
                {
                    // For SVG, we'll create a bitmap icon instead
                    // SVG requires rendering, so we'll use a simple colored circle
                    return CreateColoredIcon(GetStatusColor());
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MainWindow] Error loading SVG icon: {ex.Message}");
        }

        // Fallback to colored circle
        return CreateColoredIcon(GetStatusColor());
    }

    private System.Drawing.Color GetStatusColor()
    {
        if (!ViewModel!.IsConnected)
            return System.Drawing.Color.Gray;

        var brush = ViewModel.OverallStatusColor as SolidColorBrush;
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
            
            // Create icon from bitmap safely
            IntPtr hIcon = bitmap.GetHicon();
            var icon = System.Drawing.Icon.FromHandle(hIcon);
            
            // Don't destroy the handle immediately - Icon will own it
            return icon;
        }
        finally
        {
            // Dispose bitmap but keep icon alive
            bitmap?.Dispose();
        }
    }

    private void ShowWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void HideWindow()
    {
        Hide();
    }

    private void ToggleWindow()
    {
        if (IsVisible)
            HideWindow();
        else
            ShowWindow();
    }

    private void ExitApplication()
    {
        ViewModel?.Stop();
        _miniMonitorWindow?.Close();
        _notifyIcon?.Dispose();
        System.Windows.Application.Current.Shutdown();
    }

    private void ShowSettings()
    {
        // Get config service from ViewModel if not already set
        var configService = _configService;
        if (configService == null && ViewModel != null)
        {
            // Try to get from application's service provider or create default
            configService = new ConfigurationService();
        }
        
        if (configService != null)
        {
            var settingsWindow = new SettingsWindow(configService);
            
            // Only set Owner if this window is visible
            if (IsVisible)
            {
                settingsWindow.Owner = this;
            }
            
            if (settingsWindow.ShowDialog() == true)
            {
                // Configuration changed, restart polling service
                ViewModel?.Stop();
                ViewModel?.Start();
            }
        }
    }

    private void ToggleMiniMonitor()
    {
        if (_miniMonitorWindow == null)
        {
            var miniMonitorVm = new MiniMonitorViewModel(ViewModel);
            _miniMonitorWindow = new MiniMonitorWindow
            {
                DataContext = miniMonitorVm
            };
            
            // Set the MiniMonitorViewModel reference in MainViewModel
            if (ViewModel != null)
            {
                ViewModel.MiniMonitorViewModel = miniMonitorVm;
            }
            
            _miniMonitorWindow.Closed += (s, e) => 
            {
                // Clear the reference when the MiniMonitorWindow closes
                if (ViewModel != null)
                {
                    ViewModel.MiniMonitorViewModel = null;
                }
                _miniMonitorWindow = null;
            };
            _miniMonitorWindow.Show();
        }
        else if (_miniMonitorWindow.IsVisible)
        {
            _miniMonitorWindow.Hide();
        }
        else
        {
            _miniMonitorWindow.Show();
            _miniMonitorWindow.Activate();
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

    private void Window_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            HideWindow();
        }
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        HideWindow();
    }

    private void HostUrl_Click(object sender, MouseButtonEventArgs e)
    {
        if (ViewModel?.HostUrl != null)
        {
            OpenUrl(ViewModel.HostUrl);
        }
    }

    private void ResourceUrl_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBlock textBlock && 
            textBlock.DataContext is ResourceViewModel resource && 
            !string.IsNullOrEmpty(resource.Url))
        {
            OpenUrl(resource.Url);
        }
    }

    private void OpenUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return;

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            // Silently fail for now
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.RefreshCommand?.Execute(null);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        HideWindow();
    }

    private void MiniMonitor_Click(object sender, RoutedEventArgs e)
    {
        ToggleMiniMonitor();
    }

    private void Resource_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.Border border && 
            border.DataContext is ResourceViewModel resource)
        {
            ViewModel?.SelectResource(resource);
        }
    }

    private void ClearLogs_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.ClearLogs();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            ViewModel.Stop();
        }
        _miniMonitorWindow?.Close();
        _notifyIcon?.Dispose();
        _currentIcon?.Dispose();
        base.OnClosed(e);
    }
}
