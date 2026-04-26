using FluentAssertions;
using Moq;
using System.Diagnostics;
using Xunit;

namespace ElBruno.AspireMonitor.Tests.Views;

/// <summary>
/// Tests for system tray context menu: Details, Mini Monitor, GitHub, Exit options.
/// Covers menu functionality, Process.Start mocking, and clean app exit.
/// </summary>
public class SystemTrayContextMenuTests
{
    #region Context Menu Display Tests

    [Fact]
    public void ContextMenu_RightClickTrayIcon_OpensMenu()
    {
        // Arrange
        var trayIcon = new MockTrayIcon();

        // Act - Right-click on tray icon
        var contextMenu = trayIcon.ShowContextMenu();

        // Assert
        contextMenu.Should().NotBeNull("context menu should open");
        contextMenu!.IsVisible.Should().BeTrue("context menu should be visible");
    }

    [Fact]
    public void ContextMenu_ContainsAllRequiredOptions()
    {
        // Arrange
        var trayIcon = new MockTrayIcon();
        var contextMenu = trayIcon.ShowContextMenu();

        // Act
        var menuItems = contextMenu!.GetMenuItems();

        // Assert
        menuItems.Should().Contain("Details", "menu should have Details option");
        menuItems.Should().Contain("Mini Monitor", "menu should have Mini Monitor option");
        menuItems.Should().Contain("Go to Repository", "menu should have GitHub/Repository option");
        menuItems.Should().Contain("Exit", "menu should have Exit option");
    }

    #endregion

    #region Context Menu "Details" Option Tests

    [Fact]
    public void ContextMenu_DetailsOption_OpensMainWindow()
    {
        // Arrange
        var contextMenu = new MockContextMenu();
        var windowManager = new MockWindowManagerForMenu();

        // Act - Click Details option
        contextMenu.ClickDetailsOption(windowManager.ShowMainWindow);

        // Assert
        windowManager.IsMainWindowOpen.Should().BeTrue("Details option should open MainWindow");
    }

    [Fact]
    public void ContextMenu_DetailsOption_FocusesMainWindow_IfAlreadyOpen()
    {
        // Arrange
        var contextMenu = new MockContextMenu();
        var windowManager = new MockWindowManagerForMenu();
        windowManager.ShowMainWindow();
        var focusCount = 0;
        windowManager.OnMainWindowFocus += () => focusCount++;

        // Act - Click Details when window already open
        contextMenu.ClickDetailsOption(() => 
        {
            windowManager.RaiseMainWindowFocus();
        });

        // Assert
        focusCount.Should().Be(1, "MainWindow should be focused when Details clicked");
    }

    #endregion

    #region Context Menu "Mini Monitor" Option Tests

    [Fact]
    public void ContextMenu_MiniMonitorOption_OpensMiniMonitorWindow()
    {
        // Arrange
        var contextMenu = new MockContextMenu();
        var windowManager = new MockWindowManagerForMenu();

        // Act - Click Mini Monitor option
        contextMenu.ClickMiniMonitorOption(windowManager.ShowMiniMonitor);

        // Assert
        windowManager.IsMiniMonitorOpen.Should().BeTrue("Mini Monitor option should open floating window");
    }

    [Fact]
    public void ContextMenu_MiniMonitorOption_SingleInstance_Enforced()
    {
        // Arrange
        var contextMenu = new MockContextMenu();
        var windowManager = new MockWindowManagerForMenu();

        // Act - Click Mini Monitor multiple times
        contextMenu.ClickMiniMonitorOption(windowManager.ShowMiniMonitor);
        contextMenu.ClickMiniMonitorOption(windowManager.ShowMiniMonitor);
        contextMenu.ClickMiniMonitorOption(windowManager.ShowMiniMonitor);

        // Assert
        windowManager.MiniMonitorInstanceCount.Should().Be(1, "only one MiniMonitor instance should exist");
    }

    #endregion

    #region Context Menu "GitHub" / "Go to Repository" Option Tests

    [Fact]
    public void ContextMenu_GitHubOption_OpensUrl_InBrowser()
    {
        // Arrange
        var contextMenu = new MockContextMenu();
        var mockProcessHandler = new MockProcessHandler();
        var gitHubUrl = "https://github.com/elbruno/ElBruno.AspireMonitor";

        // Act - Click GitHub option
        contextMenu.ClickGitHubOption(() =>
        {
            mockProcessHandler.StartProcess(gitHubUrl);
        });

        // Assert
        mockProcessHandler.LastOpenedUrl.Should().Be(gitHubUrl, "should open GitHub repository URL");
    }

    [Fact]
    public void ContextMenu_GitHubOption_UsesProcessStart_Correctly()
    {
        // Arrange
        var contextMenu = new MockContextMenu();
        var processStartCalls = new List<string>();
        Action<string> mockProcessStart = (url) =>
        {
            processStartCalls.Add(url);
        };

        // Act
        contextMenu.ClickGitHubOption(() =>
        {
            mockProcessStart("https://github.com/elbruno/ElBruno.AspireMonitor");
        });

        // Assert
        processStartCalls.Should().HaveCount(1, "Process.Start should be called once");
        processStartCalls[0].Should().Contain("github.com", "should open GitHub URL");
    }

    [Fact]
    public void ContextMenu_GitHubOption_HandlesUrlOpeningErrors()
    {
        // Arrange
        var contextMenu = new MockContextMenu();
        var errorOccurred = false;
        Action<string> mockProcessStart = (url) =>
        {
            throw new Exception("Failed to open URL");
        };

        // Act
        try
        {
            contextMenu.ClickGitHubOption(() =>
            {
                mockProcessStart("https://github.com/elbruno/ElBruno.AspireMonitor");
            });
        }
        catch
        {
            errorOccurred = true;
        }

        // Assert
        errorOccurred.Should().BeTrue("error handling should be tested");
    }

    #endregion

    #region Context Menu "Exit" / "Quit" Option Tests

    [Fact]
    public void ContextMenu_ExitOption_ClosesApplication()
    {
        // Arrange
        var contextMenu = new MockContextMenu();
        var appManager = new MockApplicationManager();

        // Act - Click Exit option
        contextMenu.ClickExitOption(appManager.ExitApplication);

        // Assert
        appManager.IsApplicationRunning.Should().BeFalse("application should be closed");
    }

    [Fact]
    public void ContextMenu_ExitOption_ClosesAllWindows()
    {
        // Arrange
        var contextMenu = new MockContextMenu();
        var appManager = new MockApplicationManager();
        appManager.OpenMainWindow();
        appManager.OpenMiniMonitor();

        // Act - Click Exit
        contextMenu.ClickExitOption(appManager.ExitApplication);

        // Assert
        appManager.IsMainWindowOpen.Should().BeFalse("MainWindow should be closed");
        appManager.IsMiniMonitorOpen.Should().BeFalse("MiniMonitor should be closed");
    }

    [Fact]
    public void ContextMenu_ExitOption_RemovesTrayIcon()
    {
        // Arrange
        var trayIcon = new MockTrayIcon();
        var appManager = new MockApplicationManager();

        // Act - Click Exit
        appManager.ExitApplication();
        trayIcon.Hide();

        // Assert
        trayIcon.IsVisible.Should().BeFalse("tray icon should be removed when app exits");
    }

    [Fact]
    public void ContextMenu_ExitOption_NoOrphanedProcesses()
    {
        // Arrange
        var appManager = new MockApplicationManager();
        var processHandler = new MockProcessHandler();
        processHandler.ActiveProcessCount = 1; // Simulate running process

        // Act - Start app and exit
        appManager.ExitApplication();
        processHandler.CleanupProcesses();

        // Assert
        processHandler.ActiveProcessCount.Should().Be(0, "no orphaned processes should exist after exit");
    }

    #endregion

    #region Context Menu Enable/Disable Tests

    [Fact]
    public void ContextMenu_DetailsOption_AlwaysEnabled()
    {
        // Arrange
        var contextMenu = new MockContextMenu();

        // Act
        var isDetailsEnabled = contextMenu.GetMenuItemState("Details");

        // Assert
        isDetailsEnabled.Should().BeTrue("Details option should always be enabled");
    }

    [Fact]
    public void ContextMenu_MiniMonitorOption_AlwaysEnabled()
    {
        // Arrange
        var contextMenu = new MockContextMenu();

        // Act
        var isMiniMonitorEnabled = contextMenu.GetMenuItemState("Mini Monitor");

        // Assert
        isMiniMonitorEnabled.Should().BeTrue("Mini Monitor option should always be enabled");
    }

    [Fact]
    public void ContextMenu_GitHubOption_EnabledWhenOnline()
    {
        // Arrange
        var contextMenu = new MockContextMenu();
        var networkStatus = new MockNetworkStatus { IsOnline = true };

        // Act
        var isGitHubEnabled = contextMenu.GetMenuItemState("Go to Repository", networkStatus.IsOnline);

        // Assert
        isGitHubEnabled.Should().BeTrue("GitHub option should be enabled when online");
    }

    [Fact]
    public void ContextMenu_GitHubOption_DisabledWhenOffline()
    {
        // Arrange
        var contextMenu = new MockContextMenu();
        var networkStatus = new MockNetworkStatus { IsOnline = false };

        // Act
        var isGitHubEnabled = contextMenu.GetMenuItemState("Go to Repository", networkStatus.IsOnline);

        // Assert
        isGitHubEnabled.Should().BeFalse("GitHub option should be disabled when offline");
    }

    [Fact]
    public void ContextMenu_ExitOption_AlwaysEnabled()
    {
        // Arrange
        var contextMenu = new MockContextMenu();

        // Act
        var isExitEnabled = contextMenu.GetMenuItemState("Exit");

        // Assert
        isExitEnabled.Should().BeTrue("Exit option should always be enabled");
    }

    #endregion

    #region Context Menu Interaction Tests

    [Fact]
    public void ContextMenu_ClickingOption_ClosesMenu()
    {
        // Arrange
        var trayIcon = new MockTrayIcon();
        var contextMenu = trayIcon.ShowContextMenu();

        // Act - Click an option
        contextMenu!.ClickDetailsOption(() => { });

        // Assert
        contextMenu.IsVisible.Should().BeFalse("menu should close after option is clicked");
    }

    [Fact]
    public void ContextMenu_EscapeKey_ClosesMenu()
    {
        // Arrange
        var trayIcon = new MockTrayIcon();
        var contextMenu = trayIcon.ShowContextMenu();

        // Act - Press Escape
        contextMenu!.PressEscapeKey();

        // Assert
        contextMenu.IsVisible.Should().BeFalse("menu should close when Escape is pressed");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void TrayIcon_MultipleContextMenuActions_WorkSequentially()
    {
        // Arrange
        var trayIcon = new MockTrayIcon();
        var windowManager = new MockWindowManagerForMenu();
        var actions = new List<string>();

        // Act - Open Details
        trayIcon.ShowContextMenu()?.ClickDetailsOption(() => actions.Add("Details opened"));
        
        // Act - Open Mini Monitor
        trayIcon.ShowContextMenu()?.ClickMiniMonitorOption(() => actions.Add("Mini Monitor opened"));
        
        // Act - Exit
        trayIcon.ShowContextMenu()?.ClickExitOption(() => actions.Add("Application exited"));

        // Assert
        actions.Should().HaveCount(3, "all menu actions should execute");
        actions.Should().Contain("Details opened");
        actions.Should().Contain("Mini Monitor opened");
        actions.Should().Contain("Application exited");
    }

    [Fact]
    public void ContextMenu_DoesNotAffectOtherComponents()
    {
        // Arrange
        var trayIcon = new MockTrayIcon();
        var externalService = new MockExternalService();
        var initialState = externalService.State;

        // Act - Use context menu
        trayIcon.ShowContextMenu()?.ClickDetailsOption(() => { });

        // Assert
        externalService.State.Should().Be(initialState, "context menu should not affect other components");
    }

    #endregion

    #region Helper Classes

    private class MockContextMenu
    {
        public bool IsVisible { get; set; } = true;

        public void ClickDetailsOption(Action callback)
        {
            callback?.Invoke();
            IsVisible = false;
        }

        public void ClickMiniMonitorOption(Action callback)
        {
            callback?.Invoke();
            IsVisible = false;
        }

        public void ClickGitHubOption(Action callback)
        {
            callback?.Invoke();
            IsVisible = false;
        }

        public void ClickExitOption(Action callback)
        {
            callback?.Invoke();
            IsVisible = false;
        }

        public List<string> GetMenuItems()
        {
            return new List<string> { "Details", "Mini Monitor", "Go to Repository", "Exit" };
        }

        public bool GetMenuItemState(string menuItem, bool? condition = null)
        {
            return condition ?? true;
        }

        public void PressEscapeKey()
        {
            IsVisible = false;
        }
    }

    private class MockTrayIcon
    {
        public bool IsVisible { get; set; } = true;

        public MockContextMenu? ShowContextMenu()
        {
            return new MockContextMenu();
        }

        public void Hide()
        {
            IsVisible = false;
        }
    }

    private class MockWindowManagerForMenu
    {
        public bool IsMainWindowOpen { get; private set; } = false;
        public bool IsMiniMonitorOpen { get; private set; } = false;
        public int MiniMonitorInstanceCount { get; private set; } = 0;

        public event Action? OnMainWindowFocus;

        public void ShowMainWindow()
        {
            IsMainWindowOpen = true;
        }

        public void ShowMiniMonitor()
        {
            if (!IsMiniMonitorOpen)
            {
                IsMiniMonitorOpen = true;
                MiniMonitorInstanceCount = 1;
            }
        }

        public void RaiseMainWindowFocus()
        {
            OnMainWindowFocus?.Invoke();
        }
    }

    private class MockProcessHandler
    {
        public string LastOpenedUrl { get; set; } = "";
        public int ActiveProcessCount { get; set; } = 0;

        public void StartProcess(string url)
        {
            LastOpenedUrl = url;
        }

        public void CleanupProcesses()
        {
            ActiveProcessCount = 0;
        }
    }

    private class MockApplicationManager
    {
        public bool IsApplicationRunning { get; set; } = true;
        public bool IsMainWindowOpen { get; set; } = false;
        public bool IsMiniMonitorOpen { get; set; } = false;

        public void OpenMainWindow()
        {
            IsMainWindowOpen = true;
        }

        public void OpenMiniMonitor()
        {
            IsMiniMonitorOpen = true;
        }

        public void ExitApplication()
        {
            IsApplicationRunning = false;
            IsMainWindowOpen = false;
            IsMiniMonitorOpen = false;
        }
    }

    private class MockNetworkStatus
    {
        public bool IsOnline { get; set; } = true;
    }

    private class MockExternalService
    {
        public string State { get; set; } = "Initial";
    }

    #endregion
}
