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

    private void DashboardLink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MiniMonitorWindow] Failed to open dashboard URL: {ex.Message}");
        }
    }

    private void ResourceLink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MiniMonitorWindow] Failed to open resource URL: {ex.Message}");
        }
    }

    private void StartAspire_Click(object sender, RoutedEventArgs e)
    {
        InvokeMainViewModelCommand(vm => vm.StartAspireCommand, "start");
    }

    private void StopAspire_Click(object sender, RoutedEventArgs e)
    {
        InvokeMainViewModelCommand(vm => vm.StopAspireCommand, "stop");
    }

    private void InvokeMainViewModelCommand(Func<MainViewModel, ICommand?> commandSelector, string commandName)
    {
        try
        {
            if (DataContext is not MiniMonitorViewModel { MainViewModel: { } mainVm })
            {
                System.Diagnostics.Debug.WriteLine($"[MiniMonitorWindow] Cannot invoke aspire {commandName}: MainViewModel unavailable");
                return;
            }

            var command = commandSelector(mainVm);
            if (command is null)
            {
                System.Diagnostics.Debug.WriteLine($"[MiniMonitorWindow] Cannot invoke aspire {commandName}: command is null");
                return;
            }

            if (!command.CanExecute(null))
            {
                System.Diagnostics.Debug.WriteLine($"[MiniMonitorWindow] Aspire {commandName} command CanExecute returned false (already running or busy)");
                return;
            }

            command.Execute(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MiniMonitorWindow] Failed to invoke aspire {commandName}: {ex.Message}");
        }
    }
}
