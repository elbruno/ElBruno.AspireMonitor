using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Specialized;
using ElBruno.AspireMonitor.ViewModels;

namespace ElBruno.AspireMonitor.Views;

public partial class MiniMonitor : Window
{
    private System.Windows.Point _dragStartPoint;
    private System.Windows.Controls.ListBox? _logListBox;

    public MiniMonitor()
    {
        InitializeComponent();
        Loaded += MiniMonitor_Loaded;
    }

    private void MiniMonitor_Loaded(object sender, RoutedEventArgs e)
    {
        _logListBox = FindLogListBox(this);
        if (_logListBox != null && _logListBox.ItemsSource is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += LogCollection_CollectionChanged;
        }
    }

    private System.Windows.Controls.ListBox? FindLogListBox(DependencyObject obj)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            var child = VisualTreeHelper.GetChild(obj, i);
            if (child is System.Windows.Controls.ListBox lb && lb.Name != "")
            {
                return lb;
            }

            var found = FindLogListBox(child);
            if (found != null)
                return found;
        }
        return null;
    }

    private void LogCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_logListBox?.Items.Count > 0)
        {
            _logListBox.ScrollIntoView(_logListBox.Items[_logListBox.Items.Count - 1]);
        }
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
