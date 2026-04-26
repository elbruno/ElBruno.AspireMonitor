using System.Windows;
using ElBruno.AspireMonitor.ViewModels;

namespace ElBruno.AspireMonitor.Views;

public partial class MiniMonitor : Window
{
    private System.Windows.Point _dragStartPoint;

    public MiniMonitor()
    {
        InitializeComponent();
    }

    private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(this);
        DragMove();
    }

    private void Start_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as MiniMonitorViewModel;
        viewModel?.StartAspireCommand?.Execute(null);
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as MiniMonitorViewModel;
        viewModel?.StopAspireCommand?.Execute(null);
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
}
