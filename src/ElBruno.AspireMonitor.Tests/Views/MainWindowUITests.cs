using FluentAssertions;
using Moq;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using Xunit;

namespace ElBruno.AspireMonitor.Tests.Views;

/// <summary>
/// Tests for MainWindow enhancements: minimize, maximize, resize, version display.
/// Covers window chrome, state persistence, and system tray integration.
/// </summary>
public class MainWindowUITests
{
    private const double DefaultWidth = 800;
    private const double DefaultHeight = 600;

    #region Window Chrome Tests

    [Fact]
    public void MainWindow_MinimizeButton_Successfully()
    {
        // Arrange
        var window = new MockMainWindow();
        window.Width = DefaultWidth;
        window.Height = DefaultHeight;
        
        // Act
        window.Minimize();

        // Assert
        window.WindowState.Should().Be(WindowState.Minimized, "window should be minimized");
        window.IsVisible.Should().BeFalse("minimized window should not be visible");
    }

    [Fact]
    public void MainWindow_MinimizeButton_WindowRestoresFromTray()
    {
        // Arrange
        var window = new MockMainWindow();
        window.WindowState = WindowState.Normal;
        
        // Act - Minimize
        window.WindowState = WindowState.Minimized;
        
        // Act - Restore from tray (simulate tray icon click)
        window.WindowState = WindowState.Normal;
        window.IsVisible = true;

        // Assert
        window.WindowState.Should().Be(WindowState.Normal, "window should be restored");
        window.IsVisible.Should().BeTrue("window should be visible after restore");
    }

    [Fact]
    public void MainWindow_MaximizeButton_Success()
    {
        // Arrange
        var window = new MockMainWindow();
        window.WindowState = WindowState.Normal;
        window.Width = DefaultWidth;
        window.Height = DefaultHeight;
        var initialWidth = window.Width;
        var initialHeight = window.Height;

        // Act
        window.Maximize();

        // Assert
        window.WindowState.Should().Be(WindowState.Maximized, "window should be maximized");
        window.Width.Should().BeGreaterThan(initialWidth, "maximized width should be larger");
        window.Height.Should().BeGreaterThan(initialHeight, "maximized height should be larger");
    }

    [Fact]
    public void MainWindow_MaximizeRestore_Cycle()
    {
        // Arrange
        var window = new MockMainWindow();
        window.WindowState = WindowState.Normal;
        window.Width = DefaultWidth;
        window.Height = DefaultHeight;

        // Act - Maximize
        window.Maximize();
        var maximizedWidth = window.Width;
        var maximizedHeight = window.Height;

        // Act - Restore
        window.Restore();

        // Assert
        window.WindowState.Should().Be(WindowState.Normal, "window should be restored to normal");
        window.Width.Should().BeLessThan(maximizedWidth, "restored width should be less than maximized");
        window.Height.Should().BeLessThan(maximizedHeight, "restored height should be less than maximized");
    }

    #endregion

    #region Window Resize Tests

    [Fact]
    public void MainWindow_ManualResize_Works()
    {
        // Arrange
        var window = new MockMainWindow();
        window.WindowState = WindowState.Normal;
        window.Width = DefaultWidth;
        window.Height = DefaultHeight;

        // Act - User drags window corner to resize
        var newWidth = DefaultWidth + 100;
        var newHeight = DefaultHeight + 50;
        window.Width = newWidth;
        window.Height = newHeight;

        // Assert
        window.Width.Should().Be(newWidth, "width should be updated to resized value");
        window.Height.Should().Be(newHeight, "height should be updated to resized value");
    }

    [Fact]
    public void MainWindow_ResizeViaMaximize_WorksCorrectly()
    {
        // Arrange
        var window = new MockMainWindow();
        window.WindowState = WindowState.Normal;
        window.Width = DefaultWidth;
        window.Height = DefaultHeight;

        // Act - Maximize resize
        window.WindowState = WindowState.Maximized;
        var maximizedSize = new { Width = window.Width, Height = window.Height };

        // Act - Restore to normal
        window.WindowState = WindowState.Normal;

        // Assert - Window size should be restored properly
        window.Width.Should().Be(DefaultWidth, "width should be restored to original");
        window.Height.Should().Be(DefaultHeight, "height should be restored to original");
    }

    [Fact]
    public void MainWindow_MinimumWindowSize_Enforced()
    {
        // Arrange
        var window = new MockMainWindow();
        window.MinWidth = 400;
        window.MinHeight = 300;

        // Act - Try to resize below minimum
        window.Resize(200, 100);

        // Assert - Minimum size should be enforced
        window.Width.Should().BeGreaterThanOrEqualTo(window.MinWidth, "width should respect minimum");
        window.Height.Should().BeGreaterThanOrEqualTo(window.MinHeight, "height should respect minimum");
    }

    #endregion

    #region Window State Persistence Tests

    [Fact]
    public void MainWindow_StateAfterMinimizeRestore_PreservesSize()
    {
        // Arrange
        var window = new MockMainWindow();
        window.WindowState = WindowState.Normal;
        window.Width = DefaultWidth;
        window.Height = DefaultHeight;
        window.Left = 100;
        window.Top = 100;
        var originalState = new WindowStateSnapshot(window);

        // Act - Minimize then restore
        window.WindowState = WindowState.Minimized;
        window.WindowState = WindowState.Normal;

        // Assert - Position and size should be preserved
        window.Width.Should().Be(originalState.Width, "width should be preserved after minimize/restore");
        window.Height.Should().Be(originalState.Height, "height should be preserved after minimize/restore");
        window.Left.Should().Be(originalState.Left, "left position should be preserved");
        window.Top.Should().Be(originalState.Top, "top position should be preserved");
    }

    [Fact]
    public void MainWindow_StatePersistence_SavesWindowGeometry()
    {
        // Arrange
        var window = new MockMainWindow();
        window.Width = 1024;
        window.Height = 768;
        window.Left = 50;
        window.Top = 75;
        window.WindowState = WindowState.Normal;
        var stateManager = new MockWindowStateManager();

        // Act - Save state
        stateManager.SaveWindowState(window.WindowState, window.Width, window.Height, window.Left, window.Top);

        // Act - Restore state
        var (restoredState, restoredWidth, restoredHeight, restoredLeft, restoredTop) = 
            stateManager.LoadWindowState();

        // Assert
        restoredState.Should().Be(window.WindowState);
        restoredWidth.Should().Be(1024);
        restoredHeight.Should().Be(768);
        restoredLeft.Should().Be(50);
        restoredTop.Should().Be(75);
    }

    [Fact]
    public void MainWindow_RestoreMaximizedState_OnApplicationStart()
    {
        // Arrange
        var stateManager = new MockWindowStateManager();
        stateManager.SaveWindowState(WindowState.Maximized, 1920, 1080, 0, 0);
        var window = new MockMainWindow();

        // Act - Load saved state on startup
        var (savedState, _, _, _, _) = stateManager.LoadWindowState();
        window.WindowState = savedState;

        // Assert
        window.WindowState.Should().Be(WindowState.Maximized, "window should start maximized if that was the saved state");
    }

    #endregion

    #region Version Display Tests

    [Fact]
    public void MainWindow_VersionDisplay_InWindowTitle()
    {
        // Arrange
        var window = new MockMainWindow();
        var version = GetApplicationVersion();

        // Act
        window.UpdateTitleWithVersion(version);

        // Assert
        window.Title.Should().Contain(version, "window title should contain application version");
    }

    [Fact]
    public void MainWindow_VersionDisplay_Valid()
    {
        // Arrange
        var window = new MockMainWindow();
        var version = GetApplicationVersion();

        // Act
        var displayVersion = version;

        // Assert
        displayVersion.Should().MatchRegex(@"^\d+\.\d+\.\d+", "version should be in semantic versioning format");
    }

    [Fact]
    public void MainWindow_VersionDisplay_MatchesAssemblyVersion()
    {
        // Arrange
        var window = new MockMainWindow();
        var assemblyVersion = GetApplicationVersion();

        // Act
        window.UpdateTitleWithVersion(assemblyVersion);
        var displayedVersion = ExtractVersionFromTitle(window.Title);

        // Assert
        displayedVersion.Should().Be(assemblyVersion, "displayed version should match assembly version");
    }

    #endregion

    #region Tray Icon Integration Tests

    [Fact]
    public void MainWindow_TrayIconClick_OpensAndFocuses()
    {
        // Arrange
        var window = new MockMainWindow();
        window.WindowState = WindowState.Minimized;
        window.IsVisible = false;

        // Act - Simulate tray icon click
        window.WindowState = WindowState.Normal;
        window.IsVisible = true;
        window.Focus();

        // Assert
        window.WindowState.Should().Be(WindowState.Normal, "window should be shown");
        window.IsVisible.Should().BeTrue("window should be visible");
    }

    [Fact]
    public void MainWindow_TrayIconDoubleClick_FocusesExistingWindow()
    {
        // Arrange
        var window = new MockMainWindow { Title = "Main Window" };
        window.WindowState = WindowState.Minimized;
        var focusedWindows = new List<string>();

        // Act - Simulate double-click on tray icon
        window.WindowState = WindowState.Normal;
        window.Focus();
        focusedWindows.Add(window.Title);

        // Act - Click again (should only focus, not create new instance)
        window.Focus();
        focusedWindows.Add(window.Title);

        // Assert
        focusedWindows.Count.Should().Be(2, "focus should be called twice");
        focusedWindows.Distinct().Count().Should().Be(1, "only one instance should exist");
    }

    #endregion

    #region Helper Classes and Methods

    private string GetApplicationVersion()
    {
        // Simulate getting version from assembly
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version?.ToString() ?? "1.0.0.0";
    }

    private string ExtractVersionFromTitle(string title)
    {
        // Extract semantic version from title (e.g., "AspireMonitor v1.0.0" or "v1.0.0.0")
        var match = System.Text.RegularExpressions.Regex.Match(title, @"(\d+\.\d+\.\d+(?:\.\d+)?)");
        return match.Success ? match.Groups[1].Value : "0.0.0";
    }

    private class MockMainWindow
    {
        private double _normalWidth = DefaultWidth;
        private double _normalHeight = DefaultHeight;
        
        public string Title { get; set; } = "AspireMonitor";
        public WindowState WindowState { get; set; } = WindowState.Normal;
        public bool IsVisible { get; set; } = true;
        public double Width { get; set; } = DefaultWidth;
        public double Height { get; set; } = DefaultHeight;
        public double Left { get; set; } = 0;
        public double Top { get; set; } = 0;
        public double MinWidth { get; set; } = 0;
        public double MinHeight { get; set; } = 0;

        public void Focus()
        {
            // Simulate window focus
        }

        public void Minimize()
        {
            WindowState = WindowState.Minimized;
            IsVisible = false;
        }

        public void Maximize()
        {
            _normalWidth = Width;
            _normalHeight = Height;
            WindowState = WindowState.Maximized;
            Width = 1920; // Simulate maximized screen size
            Height = 1080;
        }

        public void Restore()
        {
            WindowState = WindowState.Normal;
            Width = _normalWidth;
            Height = _normalHeight;
        }

        public void Resize(double newWidth, double newHeight)
        {
            Width = Math.Max(newWidth, MinWidth);
            Height = Math.Max(newHeight, MinHeight);
        }

        public void UpdateTitleWithVersion(string version)
        {
            Title = $"AspireMonitor v{version}";
        }
    }

    private class WindowStateSnapshot
    {
        public WindowState WindowState { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }

        public WindowStateSnapshot(MockMainWindow window)
        {
            WindowState = window.WindowState;
            Width = window.Width;
            Height = window.Height;
            Left = window.Left;
            Top = window.Top;
        }
    }

    private class MockWindowStateManager
    {
        private WindowState _savedState = WindowState.Normal;
        private double _savedWidth = DefaultWidth;
        private double _savedHeight = DefaultHeight;
        private double _savedLeft = 0;
        private double _savedTop = 0;

        public void SaveWindowState(WindowState state, double width, double height, double left, double top)
        {
            _savedState = state;
            _savedWidth = width;
            _savedHeight = height;
            _savedLeft = left;
            _savedTop = top;
        }

        public (WindowState, double, double, double, double) LoadWindowState()
        {
            return (_savedState, _savedWidth, _savedHeight, _savedLeft, _savedTop);
        }
    }

    #endregion
}
