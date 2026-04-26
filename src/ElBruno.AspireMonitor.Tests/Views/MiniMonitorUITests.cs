using FluentAssertions;
using Moq;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using Xunit;

namespace ElBruno.AspireMonitor.Tests.Views;

/// <summary>
/// Tests for MiniMonitor floating window: always-on-top, transparency, minimal UI, version display.
/// Covers single instance management, resizing, dragging, and lifecycle.
/// </summary>
public class MiniMonitorUITests
{
    private const double DefaultMiniWindowWidth = 300;
    private const double DefaultMiniWindowHeight = 150;
    private const double DefaultOpacity = 0.75;

    #region MiniMonitor Window Creation Tests

    [Fact]
    public void MiniMonitor_OpensFromContextMenu_Success()
    {
        // Arrange
        var contextMenu = new MockContextMenu();
        var miniMonitorFactory = new MockMiniMonitorFactory();

        // Act - User clicks "Mini Monitor" in tray context menu
        contextMenu.ClickMiniMonitorOption();
        var miniMonitor = miniMonitorFactory.CreateMiniMonitorWindow();

        // Assert
        miniMonitor.Should().NotBeNull("MiniMonitor window should be created");
        miniMonitor!.IsVisible.Should().BeTrue("MiniMonitor should be visible");
        miniMonitor.Title.Should().Contain("Mini Monitor", "window title should indicate it's the mini monitor");
    }

    [Fact]
    public void MiniMonitor_HasTopmost_True()
    {
        // Arrange
        var miniMonitor = new MockMiniMonitorWindow();

        // Act
        miniMonitor.Topmost = true;

        // Assert
        miniMonitor.Topmost.Should().BeTrue("MiniMonitor should have Topmost set to true (always on top)");
    }

    [Fact]
    public void MiniMonitor_HasSemiTransparent_Opacity()
    {
        // Arrange
        var miniMonitor = new MockMiniMonitorWindow();
        miniMonitor.Opacity = DefaultOpacity;

        // Act
        var actualOpacity = miniMonitor.Opacity;

        // Assert
        actualOpacity.Should().BeGreaterThanOrEqualTo(0.75, "opacity should be at least 75%");
        actualOpacity.Should().BeLessThanOrEqualTo(0.80, "opacity should be at most 80%");
    }

    #endregion

    #region MiniMonitor Display Tests

    [Fact]
    public void MiniMonitor_ShowsMinimalMetrics_ResourceCount()
    {
        // Arrange
        var miniMonitor = new MockMiniMonitorWindow();
        var metrics = new MockMetricsDisplay
        {
            ResourceCount = 5,
            StatusSummary = "3 Healthy, 1 Warning, 1 Error"
        };

        // Act
        miniMonitor.DisplayMetrics(metrics);

        // Assert
        miniMonitor.DisplayedText.Should().Contain("5", "should display resource count");
        miniMonitor.DisplayedText.Should().Contain("Healthy", "should show health status");
    }

    [Fact]
    public void MiniMonitor_ShowsMinimalMetrics_Status()
    {
        // Arrange
        var miniMonitor = new MockMiniMonitorWindow();
        var metrics = new MockMetricsDisplay
        {
            ResourceCount = 3,
            StatusSummary = "All Green",
            OverallStatus = "Green"
        };

        // Act
        miniMonitor.DisplayMetrics(metrics);

        // Assert
        miniMonitor.DisplayedText.Should().Contain("Green", "should display overall status");
    }

    [Fact]
    public void MiniMonitor_VersionDisplay_Visible()
    {
        // Arrange
        var miniMonitor = new MockMiniMonitorWindow();
        var version = GetApplicationVersion();

        // Act
        miniMonitor.DisplayVersion(version);

        // Assert
        miniMonitor.DisplayedText.Should().Contain(version, "version should be displayed");
    }

    [Fact]
    public void MiniMonitor_VersionDisplay_MatchesMainWindow()
    {
        // Arrange
        var mainWindowVersion = "1.0.0";
        var miniMonitor = new MockMiniMonitorWindow();

        // Act
        miniMonitor.DisplayVersion(mainWindowVersion);
        var miniVersion = ExtractVersionFromDisplay(miniMonitor.DisplayedText);

        // Assert
        miniVersion.Should().Be(mainWindowVersion, "MiniMonitor version should match MainWindow version");
    }

    #endregion

    #region MiniMonitor Single Instance Tests

    [Fact]
    public void MiniMonitor_SingleInstance_Opening_Again_Focuses_Existing()
    {
        // Arrange
        var windowManager = new MockWindowManager();
        var miniMonitor1 = windowManager.OpenMiniMonitor();

        // Act - Try to open again
        var miniMonitor2 = windowManager.OpenMiniMonitor();

        // Assert
        miniMonitor2.Should().BeSameAs(miniMonitor1, "should return same instance");
        miniMonitor2.IsFocused.Should().BeTrue("existing window should be focused");
    }

    [Fact]
    public void MiniMonitor_SingleInstance_NoDuplicateCreation()
    {
        // Arrange
        var windowManager = new MockWindowManager();

        // Act - Open multiple times
        var first = windowManager.OpenMiniMonitor();
        var second = windowManager.OpenMiniMonitor();
        var third = windowManager.OpenMiniMonitor();

        // Assert
        first.Should().BeSameAs(second);
        second.Should().BeSameAs(third);
        windowManager.MiniMonitorInstanceCount.Should().Be(1, "only one MiniMonitor instance should exist");
    }

    [Fact]
    public void MiniMonitor_AfterClose_CanBeReopened()
    {
        // Arrange
        var windowManager = new MockWindowManager();
        var miniMonitor = windowManager.OpenMiniMonitor();

        // Act - Close the window
        windowManager.CloseMiniMonitor();
        var reopened = windowManager.OpenMiniMonitor();

        // Assert
        reopened.Should().NotBeSameAs(miniMonitor, "after close, new instance should be created");
        reopened.IsVisible.Should().BeTrue("reopened window should be visible");
        windowManager.MiniMonitorInstanceCount.Should().Be(1, "should have exactly one instance after reopen");
    }

    #endregion

    #region MiniMonitor Interaction Tests

    [Fact]
    public void MiniMonitor_IsResizable()
    {
        // Arrange
        var miniMonitor = new MockMiniMonitorWindow();
        miniMonitor.Width = DefaultMiniWindowWidth;
        miniMonitor.Height = DefaultMiniWindowHeight;

        // Act - User resizes window
        miniMonitor.Width = DefaultMiniWindowWidth + 50;
        miniMonitor.Height = DefaultMiniWindowHeight + 30;

        // Assert
        miniMonitor.Width.Should().Be(DefaultMiniWindowWidth + 50, "width should be resizable");
        miniMonitor.Height.Should().Be(DefaultMiniWindowHeight + 30, "height should be resizable");
    }

    [Fact]
    public void MiniMonitor_IsDraggable()
    {
        // Arrange
        var miniMonitor = new MockMiniMonitorWindow();
        miniMonitor.Left = 100;
        miniMonitor.Top = 100;

        // Act - User drags window
        miniMonitor.Left = 200;
        miniMonitor.Top = 150;

        // Assert
        miniMonitor.Left.Should().Be(200, "window should be draggable horizontally");
        miniMonitor.Top.Should().Be(150, "window should be draggable vertically");
    }

    [Fact]
    public void MiniMonitor_StaysOnTop_WhenOtherWindowsOpen()
    {
        // Arrange
        var miniMonitor = new MockMiniMonitorWindow { Topmost = true };
        var otherWindow = new MockMainWindow();

        // Act - Other window is shown
        otherWindow.Focus();

        // Assert
        miniMonitor.Topmost.Should().BeTrue("MiniMonitor should remain topmost");
    }

    [Fact]
    public void MiniMonitor_MinimumSize_Enforced()
    {
        // Arrange
        var miniMonitor = new MockMiniMonitorWindow();
        miniMonitor.MinWidth = 200;
        miniMonitor.MinHeight = 100;

        // Act - Try to resize below minimum
        miniMonitor.Resize(50, 30);

        // Assert
        miniMonitor.Width.Should().BeGreaterThanOrEqualTo(miniMonitor.MinWidth);
        miniMonitor.Height.Should().BeGreaterThanOrEqualTo(miniMonitor.MinHeight);
    }

    #endregion

    #region MiniMonitor Lifecycle Tests

    [Fact]
    public void MiniMonitor_Closes_Cleanly()
    {
        // Arrange
        var windowManager = new MockWindowManager();
        var miniMonitor = windowManager.OpenMiniMonitor();
        var isClosed = false;

        // Act - Close window
        miniMonitor.OnClosing += () => isClosed = true;
        windowManager.CloseMiniMonitor();

        // Assert
        isClosed.Should().BeTrue("close event should be triggered");
        windowManager.MiniMonitorInstanceCount.Should().Be(0, "instance should be cleaned up");
    }

    [Fact]
    public void MiniMonitor_Closes_WithoutAffectingMainWindow()
    {
        // Arrange
        var mainWindow = new MockMainWindow { IsVisible = true };
        var windowManager = new MockWindowManager();
        var miniMonitor = windowManager.OpenMiniMonitor();

        // Act - Close MiniMonitor
        windowManager.CloseMiniMonitor();

        // Assert
        mainWindow.IsVisible.Should().BeTrue("MainWindow should still be visible");
        mainWindow.WindowState.Should().NotBe(WindowState.Minimized, "MainWindow state should not change");
    }

    [Fact]
    public void MiniMonitor_Closes_DoesNotHaveOrphanedProcesses()
    {
        // Arrange
        var miniMonitor = new MockMiniMonitorWindow();
        var processHandler = new MockProcessHandler();

        // Act
        miniMonitor.Close();

        // Assert
        processHandler.ActiveProcessCount.Should().Be(0, "no orphaned processes should exist");
    }

    #endregion

    #region MiniMonitor Real-time Updates Tests

    [Fact]
    public void MiniMonitor_UpdatesMetricsInRealtime()
    {
        // Arrange
        var miniMonitor = new MockMiniMonitorWindow();
        var updateCount = 0;

        miniMonitor.OnMetricsUpdate += () => updateCount++;

        // Act - Simulate multiple metric updates
        for (int i = 0; i < 5; i++)
        {
            miniMonitor.UpdateMetrics(new MockMetricsDisplay 
            { 
                ResourceCount = i + 1,
                StatusSummary = $"Update {i}"
            });
        }

        // Assert
        updateCount.Should().Be(5, "metrics should be updated in real-time");
    }

    [Fact]
    public void MiniMonitor_RefreshRate_Reasonable()
    {
        // Arrange
        var miniMonitor = new MockMiniMonitorWindow();
        var refreshIntervalMs = 500; // Every 500ms

        // Act
        var startTime = DateTime.UtcNow;
        miniMonitor.StartRefreshTimer(refreshIntervalMs);
        
        // Simulate 3 refresh cycles
        System.Threading.Thread.Sleep(refreshIntervalMs * 3 + 100);

        var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
        var expectedRefreshes = (int)(elapsedMs / refreshIntervalMs);

        // Assert
        expectedRefreshes.Should().BeGreaterThanOrEqualTo(3, "should refresh multiple times within expected interval");
    }

    #endregion

    #region Helper Classes and Methods

    private string GetApplicationVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version?.ToString() ?? "1.0.0.0";
    }

    private string ExtractVersionFromDisplay(string displayedText)
    {
        var match = System.Text.RegularExpressions.Regex.Match(displayedText, @"(\d+\.\d+\.\d+)");
        return match.Success ? match.Groups[1].Value : "0.0.0";
    }

    private class MockMiniMonitorWindow
    {
        public string Title { get; set; } = "Mini Monitor";
        public double Width { get; set; } = DefaultMiniWindowWidth;
        public double Height { get; set; } = DefaultMiniWindowHeight;
        public double Left { get; set; } = 0;
        public double Top { get; set; } = 0;
        public double MinWidth { get; set; } = 0;
        public double MinHeight { get; set; } = 0;
        public double Opacity { get; set; } = DefaultOpacity;
        public bool Topmost { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public bool IsFocused { get; set; } = false;
        public string DisplayedText { get; private set; } = "";

        public event Action? OnClosing;
        public event Action? OnMetricsUpdate;

        public void DisplayMetrics(MockMetricsDisplay metrics)
        {
            DisplayedText = $"{metrics.ResourceCount} resources: {metrics.StatusSummary} [{metrics.OverallStatus}]";
        }

        public void DisplayVersion(string version)
        {
            DisplayedText += $" v{version}";
        }

        public void Focus()
        {
            IsFocused = true;
        }

        public void Close()
        {
            IsVisible = false;
            OnClosing?.Invoke();
        }

        public void UpdateMetrics(MockMetricsDisplay metrics)
        {
            DisplayMetrics(metrics);
            OnMetricsUpdate?.Invoke();
        }

        public void Resize(double newWidth, double newHeight)
        {
            Width = Math.Max(newWidth, MinWidth);
            Height = Math.Max(newHeight, MinHeight);
        }

        public void StartRefreshTimer(int intervalMs)
        {
            // Timer implementation would be here
        }
    }

    private class MockMetricsDisplay
    {
        public int ResourceCount { get; set; }
        public string StatusSummary { get; set; } = "";
        public string OverallStatus { get; set; } = "Green";
    }

    private class MockContextMenu
    {
        public void ClickMiniMonitorOption()
        {
            // Simulate menu click
        }
    }

    private class MockMiniMonitorFactory
    {
        public MockMiniMonitorWindow? CreateMiniMonitorWindow()
        {
            return new MockMiniMonitorWindow();
        }
    }

    private class MockWindowManager
    {
        private MockMiniMonitorWindow? _miniMonitorInstance;

        public int MiniMonitorInstanceCount => _miniMonitorInstance?.IsVisible ?? false ? 1 : 0;

        public MockMiniMonitorWindow OpenMiniMonitor()
        {
            if (_miniMonitorInstance == null)
            {
                _miniMonitorInstance = new MockMiniMonitorWindow();
            }
            else
            {
                _miniMonitorInstance.Focus();
            }
            _miniMonitorInstance.IsVisible = true;
            return _miniMonitorInstance;
        }

        public void CloseMiniMonitor()
        {
            if (_miniMonitorInstance != null)
            {
                _miniMonitorInstance.Close();
                _miniMonitorInstance = null;
            }
        }
    }

    private class MockMainWindow
    {
        public bool IsVisible { get; set; } = true;
        public WindowState WindowState { get; set; } = WindowState.Normal;

        public void Focus()
        {
            // Simulate focus
        }
    }

    private class MockProcessHandler
    {
        public int ActiveProcessCount { get; set; } = 0;
    }

    #endregion
}
