using System.Windows;
using ElBruno.AspireMonitor.Services;
using ElBruno.AspireMonitor.Views;

namespace ElBruno.AspireMonitor;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configService = new ConfigurationService();
        var mainWindow = new MainWindow(null, configService);
        MainWindow = mainWindow;
        mainWindow.Show();
    }
}
