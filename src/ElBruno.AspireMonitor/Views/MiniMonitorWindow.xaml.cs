using System.Windows;
using System.Windows.Input;
using ElBruno.AspireMonitor.ViewModels;

namespace ElBruno.AspireMonitor.Views;

public partial class MiniMonitorWindow : Window
{
    public MiniMonitorWindow()
    {
        InitializeComponent();
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            OpenDetails_Click(sender, e);
        }
        else
        {
            DragMove();
        }
    }

    private void OpenDetails_Click(object sender, RoutedEventArgs e)
    {
        // Find and focus the main window
        var mainWindow = System.Windows.Application.Current.MainWindow;
        if (mainWindow != null)
        {
            if (!mainWindow.IsVisible)
            {
                mainWindow.Show();
            }
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void StartAspire_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as MiniMonitorViewModel;
        if (viewModel != null)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                ExecuteAspireCommand("start");
            });
        }
    }

    private void StopAspire_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as MiniMonitorViewModel;
        if (viewModel != null)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                ExecuteAspireCommand("stop");
            });
        }
    }

    private void ExecuteAspireCommand(string command)
    {
        try
        {
            var viewModel = DataContext as MiniMonitorViewModel;
            var workingDir = viewModel?.MainViewModel?.ProjectFolder ?? Environment.CurrentDirectory;

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c aspire {command}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDir
            };

            using (var process = System.Diagnostics.Process.Start(psi))
            {
                process?.WaitForExit(5000);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to execute aspire {command}: {ex.Message}");
        }
    }
}
