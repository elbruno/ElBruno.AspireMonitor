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
    private MiniMonitor? _miniMonitor;
    private MainViewModel? ViewModel => DataContext as MainViewModel;
    private readonly IConfigurationService? _configService;

    public MainWindow() : this(null, null, null)
    {
    }

    public MainWindow(IAspirePollingService? pollingService, IConfigurationService? configService, MainViewModel? viewModel = null)
    {
        InitializeComponent();
        _configService = configService;
        DataContext = viewModel ?? new MainViewModel(pollingService, configService);
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
        _notifyIcon.DoubleClick += (s, e) => ToggleWindow();
    }

    private void UpdateTrayIcon()
    {
        if (_notifyIcon == null || ViewModel == null)
            return;

        // Create colored icon based on status
        var brush = ViewModel.OverallStatusColor as SolidColorBrush;
        System.Drawing.Color iconColor;

        if (brush != null)
        {
            var mediaColor = brush.Color;
            iconColor = System.Drawing.Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
        }
        else
        {
            iconColor = System.Drawing.Color.Gray;
        }

        _notifyIcon.Icon = CreateColoredIcon(iconColor);
        _notifyIcon.Text = $"Aspire Monitor - {ViewModel.ConnectionStatus}";
    }

    private System.Drawing.Icon CreateColoredIcon(System.Drawing.Color color)
    {
        var bitmap = new Bitmap(16, 16);
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
        return System.Drawing.Icon.FromHandle(hIcon);
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
        _miniMonitor?.Close();
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
            var settingsWindow = new SettingsWindow(configService)
            {
                Owner = this
            };
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
        if (_miniMonitor == null)
        {
            _miniMonitor = new MiniMonitor
            {
                DataContext = new MiniMonitorViewModel(ViewModel)
            };
            _miniMonitor.Closed += (s, e) => _miniMonitor = null;
            _miniMonitor.Show();
        }
        else if (_miniMonitor.IsVisible)
        {
            _miniMonitor.Hide();
        }
        else
        {
            _miniMonitor.Show();
            _miniMonitor.Activate();
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

    protected override void OnClosed(EventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            ViewModel.Stop();
        }
        _miniMonitor?.Close();
        _notifyIcon?.Dispose();
        base.OnClosed(e);
    }
}
