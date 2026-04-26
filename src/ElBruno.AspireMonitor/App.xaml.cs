using System.Windows;

namespace ElBruno.AspireMonitor;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Hide the main window on startup (run silently in background)
        // Only the system tray icon should be visible
        if (Current.MainWindow != null)
        {
            Current.MainWindow.Visibility = Visibility.Hidden;
            Current.MainWindow.ShowInTaskbar = false;
        }
    }
}
