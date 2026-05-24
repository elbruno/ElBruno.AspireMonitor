using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using ElBruno.AspireMonitor.ViewModels;

namespace ElBruno.AspireMonitor.Views;

public partial class MiniConsoleWindow : Window
{
    private NotifyCollectionChangedEventHandler? _logLinesCollectionChangedHandler;

    public MiniConsoleWindow()
    {
        InitializeComponent();
    }

    private MainViewModel? ViewModel => DataContext as MainViewModel;

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null)
            return;

        _logLinesCollectionChangedHandler ??= (_, _) => ScrollLogToEnd();
        ViewModel.LogLines.CollectionChanged += _logLinesCollectionChangedHandler;
        ScrollLogToEnd();
    }

    private void Window_Unloaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null && _logLinesCollectionChangedHandler != null)
        {
            ViewModel.LogLines.CollectionChanged -= _logLinesCollectionChangedHandler;
        }
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            OpenDetails_Click(sender, e);
            return;
        }

        DragMove();
    }

    private void OpenDetails_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = System.Windows.Application.Current.MainWindow;
        if (mainWindow == null)
            return;

        if (!mainWindow.IsVisible)
        {
            mainWindow.Show();
        }

        mainWindow.WindowState = WindowState.Normal;
        mainWindow.Activate();
    }

    private void ClearLogs_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.ClearLogs();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }

    private void ScrollLogToEnd()
    {
        if (!IsLoaded)
            return;

        Dispatcher.BeginInvoke(() => LogScrollViewer.ScrollToEnd());
    }
}
