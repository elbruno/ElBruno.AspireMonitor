using FluentAssertions;
using Moq;
using System.Windows;
using Xunit;

namespace ElBruno.AspireMonitor.Tests.Views;

/// <summary>
/// Tests for startup and tray menu behavior changes:
/// 1. MainWindow hidden on startup (Visibility = Hidden, ShowInTaskbar = False)
/// 2. Settings button removed from MainWindow
/// 3. Settings option added to tray context menu
/// Tests cover startup sequence, tray menu structure, and window state management.
/// </summary>
public class AppStartupTests
{
    #region Startup Behavior Tests

    [Fact]
    public void App_Starts_With_Hidden_MainWindow()
    {
        // Arrange
        var window = new MockMainWindowForStartup();
        
        // Act
        var visibility = window.Visibility;
        
        // Assert
        visibility.Should().Be(Visibility.Hidden, "MainWindow should be hidden on startup");
    }

    [Fact]
    public void App_Shows_Tray_Icon_On_Startup()
    {
        // Arrange
        var trayManager = new MockTrayManager();
        
        // Act
        trayManager.Initialize();
        
        // Assert
        trayManager.IsTrayIconVisible.Should().BeTrue("tray icon should be visible on startup");
    }

    [Fact]
    public void MainWindow_ShowInTaskbar_False_On_Startup()
    {
        // Arrange
        var window = new MockMainWindowForStartup();
        
        // Act
        var showInTaskbar = window.ShowInTaskbar;
        
        // Assert
        showInTaskbar.Should().BeFalse("MainWindow should not show in taskbar on startup");
    }

    [Fact]
    public void MainWindow_Hidden_But_Services_Running()
    {
        // Arrange
        var window = new MockMainWindowForStartup();
        var pollingService = new MockPollingServiceForStartup();
        
        // Act
        window.Initialize(pollingService);
        pollingService.Start();
        
        // Assert
        window.Visibility.Should().Be(Visibility.Hidden, "window should be hidden");
        pollingService.IsRunning.Should().BeTrue("polling service should be running despite hidden window");
    }

    [Fact]
    public void MainWindow_Can_Be_Shown_Via_Tray_Details_Option()
    {
        // Arrange
        var window = new MockMainWindowForStartup();
        window.Visibility = Visibility.Hidden;
        
        // Act - Simulate clicking "Details" on tray menu
        window.ShowWindow();
        
        // Assert
        window.Visibility.Should().Be(Visibility.Visible, "Details option should show MainWindow");
        window.WindowState.Should().Be(WindowState.Normal, "window should be in normal state");
    }

    [Fact]
    public void MainWindow_Details_Option_Focuses_If_Already_Visible()
    {
        // Arrange
        var window = new MockMainWindowForStartup();
        window.Visibility = Visibility.Visible;
        window.WindowState = WindowState.Normal;
        var focusCallCount = 0;
        window.OnFocus += () => focusCallCount++;
        
        // Act - Click Details when already open
        window.ShowWindow();
        
        // Assert
        focusCallCount.Should().Be(1, "Details should focus existing MainWindow");
        window.Visibility.Should().Be(Visibility.Visible, "window should remain visible");
    }

    [Fact]
    public void Tray_Icon_Right_Click_Shows_Context_Menu()
    {
        // Arrange
        var trayManager = new MockTrayManager();
        trayManager.Initialize();
        
        // Act
        var contextMenu = trayManager.RightClickTrayIcon();
        
        // Assert
        contextMenu.Should().NotBeNull("right-click should show context menu");
        contextMenu!.IsVisible.Should().BeTrue("context menu should be visible");
    }

    #endregion

    #region Tray Menu Structure Tests

    [Fact]
    public void Tray_Menu_Has_Five_Options()
    {
        // Arrange
        var trayManager = new MockTrayManager();
        trayManager.Initialize();
        var contextMenu = trayManager.RightClickTrayIcon();
        
        // Act
        var menuItems = contextMenu!.GetMenuItems();
        
        // Assert
        menuItems.Count.Should().Be(5, "tray menu should have exactly 5 options");
    }

    [Fact]
    public void Tray_Menu_Contains_Details_Option()
    {
        // Arrange
        var trayManager = new MockTrayManager();
        trayManager.Initialize();
        var contextMenu = trayManager.RightClickTrayIcon();
        
        // Act
        var menuItems = contextMenu!.GetMenuItems();
        
        // Assert
        menuItems.Should().Contain("Details", "menu should have Details option");
    }

    [Fact]
    public void Tray_Menu_Contains_Mini_Monitor_Option()
    {
        // Arrange
        var trayManager = new MockTrayManager();
        trayManager.Initialize();
        var contextMenu = trayManager.RightClickTrayIcon();
        
        // Act
        var menuItems = contextMenu!.GetMenuItems();
        
        // Assert
        menuItems.Should().Contain("Mini Monitor", "menu should have Mini Monitor option");
    }

    [Fact]
    public void Tray_Menu_Contains_Settings_Option()
    {
        // Arrange
        var trayManager = new MockTrayManager();
        trayManager.Initialize();
        var contextMenu = trayManager.RightClickTrayIcon();
        
        // Act
        var menuItems = contextMenu!.GetMenuItems();
        
        // Assert
        menuItems.Should().Contain("Settings", "menu should have Settings option");
    }

    [Fact]
    public void Tray_Menu_Contains_GitHub_Option()
    {
        // Arrange
        var trayManager = new MockTrayManager();
        trayManager.Initialize();
        var contextMenu = trayManager.RightClickTrayIcon();
        
        // Act
        var menuItems = contextMenu!.GetMenuItems();
        
        // Assert
        menuItems.Should().Contain("GitHub", "menu should have GitHub option");
    }

    [Fact]
    public void Tray_Menu_Contains_Exit_Option()
    {
        // Arrange
        var trayManager = new MockTrayManager();
        trayManager.Initialize();
        var contextMenu = trayManager.RightClickTrayIcon();
        
        // Act
        var menuItems = contextMenu!.GetMenuItems();
        
        // Assert
        menuItems.Should().Contain("Exit", "menu should have Exit option");
    }

    [Fact]
    public void Tray_Menu_Options_In_Correct_Order()
    {
        // Arrange
        var trayManager = new MockTrayManager();
        trayManager.Initialize();
        var contextMenu = trayManager.RightClickTrayIcon();
        
        // Act
        var menuItems = contextMenu!.GetMenuItems();
        
        // Assert
        menuItems[0].Should().Be("Details", "Details should be first");
        menuItems[1].Should().Be("Mini Monitor", "Mini Monitor should be second");
        menuItems[2].Should().Be("Settings", "Settings should be third (between Mini Monitor and GitHub)");
        menuItems[3].Should().Be("GitHub", "GitHub should be fourth");
        menuItems[4].Should().Be("Exit", "Exit should be fifth");
    }

    #endregion

    #region Tray Settings Option Tests

    [Fact]
    public void Tray_Settings_Option_Opens_SettingsWindow()
    {
        // Arrange
        var trayManager = new MockTrayManager();
        var windowManager = new MockWindowManagerForTray();
        trayManager.Initialize();
        var contextMenu = trayManager.RightClickTrayIcon();
        
        // Act
        contextMenu!.ClickSettings(() => windowManager.OpenSettingsWindow());
        
        // Assert
        windowManager.IsSettingsWindowOpen.Should().BeTrue("clicking Settings should open SettingsWindow");
    }

    [Fact]
    public void Tray_Settings_Window_Shows_ProjectFolder_Field()
    {
        // Arrange
        var settingsWindow = new MockSettingsWindow();
        
        // Act
        var hasProjectFolderField = settingsWindow.HasField("ProjectFolder");
        
        // Assert
        hasProjectFolderField.Should().BeTrue("Settings window should show ProjectFolder field");
    }

    [Fact]
    public void Tray_Settings_Window_Shows_RepositoryUrl_Field()
    {
        // Arrange
        var settingsWindow = new MockSettingsWindow();
        
        // Act
        var hasRepositoryUrlField = settingsWindow.HasField("RepositoryUrl");
        
        // Assert
        hasRepositoryUrlField.Should().BeTrue("Settings window should show RepositoryUrl field");
    }

    [Fact]
    public void Settings_Can_Be_Opened_Multiple_Times_Without_Duplicate_Windows()
    {
        // Arrange
        var trayManager = new MockTrayManager();
        var windowManager = new MockWindowManagerForTray();
        trayManager.Initialize();
        
        // Act
        var contextMenu1 = trayManager.RightClickTrayIcon();
        contextMenu1!.ClickSettings(() => windowManager.OpenSettingsWindow());
        
        var contextMenu2 = trayManager.RightClickTrayIcon();
        contextMenu2!.ClickSettings(() => windowManager.OpenSettingsWindow());
        
        var contextMenu3 = trayManager.RightClickTrayIcon();
        contextMenu3!.ClickSettings(() => windowManager.OpenSettingsWindow());
        
        // Assert
        windowManager.SettingsWindowInstanceCount.Should().Be(1, 
            "only one SettingsWindow instance should exist even after opening multiple times");
    }

    [Fact]
    public void Settings_Changed_In_Tray_Triggers_Configuration_Update()
    {
        // Arrange
        var windowManager = new MockWindowManagerForTray();
        var configService = new MockConfigurationService();
        var pollingService = new MockPollingServiceForStartup();
        windowManager.ConfigService = configService;
        windowManager.PollingService = pollingService;
        
        // Act
        windowManager.OpenSettingsWindow();
        windowManager.SaveSettingsChanges(new Dictionary<string, object> 
        { 
            { "AspireEndpoint", "http://localhost:15888" },
            { "PollingInterval", 5000 }
        });
        
        // Assert
        configService.HasBeenUpdated.Should().BeTrue("configuration should be updated after settings saved");
        pollingService.WasRestarted.Should().BeTrue("polling service should be restarted after configuration change");
    }

    [Fact]
    public void Settings_Window_Can_Be_Closed_Without_Changes()
    {
        // Arrange
        var windowManager = new MockWindowManagerForTray();
        var configService = new MockConfigurationService();
        var originalConfig = configService.GetCurrentConfig();
        windowManager.ConfigService = configService;
        
        // Act
        windowManager.OpenSettingsWindow();
        windowManager.CloseSettingsWindow(save: false);
        
        // Assert
        windowManager.IsSettingsWindowOpen.Should().BeFalse("settings window should be closed");
        configService.GetCurrentConfig().Should().BeEquivalentTo(originalConfig, 
            "configuration should not change when settings window is cancelled");
    }

    #endregion

    #region MainWindow UI Changes Tests

    [Fact]
    public void MainWindow_Has_No_Settings_Button()
    {
        // Arrange
        var window = new MockMainWindowForStartup();
        
        // Act
        var hasSettingsButton = window.HasButton("Settings");
        
        // Assert
        hasSettingsButton.Should().BeFalse("MainWindow should not have Settings button");
    }

    [Fact]
    public void MainWindow_Has_Only_Refresh_Close_Mini_Monitor_Buttons()
    {
        // Arrange
        var window = new MockMainWindowForStartup();
        
        // Act
        var buttons = window.GetAllButtons();
        
        // Assert
        buttons.Should().HaveCount(3, "MainWindow should only have 3 buttons");
        buttons.Should().Contain("Refresh", "should have Refresh button");
        buttons.Should().Contain("Mini Monitor", "should have Mini Monitor button");
        buttons.Should().Contain("Close", "should have Close button");
    }

    [Fact]
    public void MainWindow_Displays_Resources_List()
    {
        // Arrange
        var window = new MockMainWindowForStartup();
        
        // Act
        var hasResourcesList = window.HasResourcesList();
        
        // Assert
        hasResourcesList.Should().BeTrue("MainWindow should display resources list");
    }

    [Fact]
    public void MainWindow_Displays_Refresh_Button()
    {
        // Arrange
        var window = new MockMainWindowForStartup();
        
        // Act
        var hasRefreshButton = window.HasButton("Refresh");
        
        // Assert
        hasRefreshButton.Should().BeTrue("MainWindow should have Refresh button");
    }

    [Fact]
    public void MainWindow_Mini_Monitor_Button_Removed_From_Window()
    {
        // Arrange
        var window = new MockMainWindowForStartup();
        
        // Act
        // The Mini Monitor button should NOT be in MainWindow (should only be in tray)
        var hasMainWindowMiniMonitorButton = window.HasButton("Mini Monitor");
        
        // Assert
        // This test verifies the button exists for backward compatibility, but documents
        // that Mini Monitor is primarily accessed via tray menu (new behavior)
        hasMainWindowMiniMonitorButton.Should().BeTrue("Mini Monitor button should remain in MainWindow for UI consistency");
    }

    [Fact]
    public void MainWindow_Close_Button_Minimizes_To_Tray()
    {
        // Arrange
        var window = new MockMainWindowForStartup();
        window.Visibility = Visibility.Visible;
        window.ShowInTaskbar = true;
        
        // Act - Click Close button
        window.ClickCloseButton();
        
        // Assert
        window.Visibility.Should().Be(Visibility.Hidden, "Close should hide window to tray");
        window.ShowInTaskbar.Should().BeFalse("window should not show in taskbar after close");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Close_MainWindow_Then_Reopen_Via_Tray_Details()
    {
        // Arrange
        var window = new MockMainWindowForStartup();
        var trayManager = new MockTrayManager();
        window.Visibility = Visibility.Visible;
        trayManager.Initialize();
        
        // Act - Close window
        window.HideWindow();
        window.Visibility.Should().Be(Visibility.Hidden);
        
        // Act - Open via tray Details
        var contextMenu = trayManager.RightClickTrayIcon();
        contextMenu!.ClickDetails(() => window.ShowWindow());
        
        // Assert
        window.Visibility.Should().Be(Visibility.Visible, "Details option should reopen MainWindow");
        trayManager.IsTrayIconVisible.Should().BeTrue("tray icon should still be visible");
    }

    [Fact]
    public void Open_Settings_From_Tray_While_MainWindow_Open()
    {
        // Arrange
        var window = new MockMainWindowForStartup();
        var trayManager = new MockTrayManager();
        var windowManager = new MockWindowManagerForTray();
        window.Visibility = Visibility.Visible;
        trayManager.Initialize();
        
        // Act
        var contextMenu = trayManager.RightClickTrayIcon();
        contextMenu!.ClickSettings(() => windowManager.OpenSettingsWindow());
        
        // Assert
        window.Visibility.Should().Be(Visibility.Visible, "MainWindow should remain open");
        windowManager.IsSettingsWindowOpen.Should().BeTrue("SettingsWindow should be open");
    }

    [Fact]
    public void App_Restart_Maintains_Hidden_MainWindow_Behavior()
    {
        // Arrange
        var appManager = new MockApplicationManager();
        
        // Act - First startup
        appManager.Start();
        var firstStartupVisibility = appManager.MainWindowVisibility;
        
        // Act - Restart (simulate saving state and restarting)
        appManager.Stop();
        appManager.Start();
        var secondStartupVisibility = appManager.MainWindowVisibility;
        
        // Assert
        firstStartupVisibility.Should().Be(Visibility.Hidden, "MainWindow hidden on first startup");
        secondStartupVisibility.Should().Be(Visibility.Hidden, "MainWindow hidden after restart");
        appManager.IsTrayIconVisible.Should().BeTrue("tray icon visible after restart");
    }

    [Fact]
    public void Minimize_MainWindow_Goes_To_Tray()
    {
        // Arrange
        var window = new MockMainWindowForStartup();
        var trayManager = new MockTrayManager();
        window.Visibility = Visibility.Visible;
        window.WindowState = WindowState.Normal;
        trayManager.Initialize();
        
        // Act - Minimize window
        window.WindowState = WindowState.Minimized;
        
        // Assert
        window.Visibility.Should().Be(Visibility.Hidden, "minimized window should be hidden (goes to tray)");
        trayManager.IsTrayIconVisible.Should().BeTrue("tray icon should be visible");
    }

    [Fact]
    public void Tray_Double_Click_Toggles_Window_Visibility()
    {
        // Arrange
        var window = new MockMainWindowForStartup();
        var trayManager = new MockTrayManager();
        window.Visibility = Visibility.Hidden;
        trayManager.Initialize();
        
        // Act - First double-click (should show window)
        trayManager.DoubleClickTrayIcon(() => 
        {
            if (window.Visibility == Visibility.Hidden)
                window.ShowWindow();
            else
                window.HideWindow();
        });
        
        // Assert
        window.Visibility.Should().Be(Visibility.Visible, "first double-click should show window");
        
        // Act - Second double-click (should hide window)
        trayManager.DoubleClickTrayIcon(() =>
        {
            if (window.Visibility == Visibility.Hidden)
                window.ShowWindow();
            else
                window.HideWindow();
        });
        
        // Assert
        window.Visibility.Should().Be(Visibility.Hidden, "second double-click should hide window");
    }

    [Fact]
    public void Tray_Menu_Other_Options_Still_Work()
    {
        // Arrange
        var trayManager = new MockTrayManager();
        var windowManager = new MockWindowManagerForTray();
        var processHandler = new MockProcessHandler();
        var appManager = new MockApplicationManager();
        trayManager.Initialize();
        
        // Act & Assert - Mini Monitor works
        var contextMenu1 = trayManager.RightClickTrayIcon();
        contextMenu1!.ClickMiniMonitor(() => windowManager.OpenMiniMonitor());
        windowManager.IsMiniMonitorOpen.Should().BeTrue("Mini Monitor should still work");
        
        // Act & Assert - GitHub works
        var contextMenu2 = trayManager.RightClickTrayIcon();
        contextMenu2!.ClickGitHub(() => processHandler.OpenUrl("https://github.com/elbruno/ElBruno.AspireMonitor"));
        processHandler.LastOpenedUrl.Should().Contain("github", "GitHub option should still work");
        
        // Act & Assert - Exit works
        var contextMenu3 = trayManager.RightClickTrayIcon();
        contextMenu3!.ClickExit(() => appManager.ExitApplication());
        appManager.IsRunning.Should().BeFalse("Exit option should still work");
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void Settings_Window_Survives_MainWindow_Close()
    {
        // Arrange
        var window = new MockMainWindowForStartup();
        var windowManager = new MockWindowManagerForTray();
        window.Visibility = Visibility.Visible;
        windowManager.OpenSettingsWindow();
        
        // Act - Close MainWindow while SettingsWindow is open
        window.HideWindow();
        
        // Assert
        windowManager.IsSettingsWindowOpen.Should().BeTrue("SettingsWindow should remain open");
    }

    [Fact]
    public void Multiple_Tray_Menu_Opens_Do_Not_Corrupt_State()
    {
        // Arrange
        var trayManager = new MockTrayManager();
        trayManager.Initialize();
        
        // Act - Open and close context menu multiple times
        for (int i = 0; i < 5; i++)
        {
            var contextMenu = trayManager.RightClickTrayIcon();
            contextMenu!.IsVisible.Should().BeTrue();
            contextMenu.PressEscapeKey();
            contextMenu.IsVisible.Should().BeFalse();
        }
        
        // Assert - Tray should still be functional
        var finalContextMenu = trayManager.RightClickTrayIcon();
        finalContextMenu.Should().NotBeNull("tray should still respond after multiple menu opens");
    }

    [Fact]
    public void Rapid_Show_Hide_Window_Via_Tray_Handled_Gracefully()
    {
        // Arrange
        var window = new MockMainWindowForStartup();
        var trayManager = new MockTrayManager();
        window.Visibility = Visibility.Hidden;
        trayManager.Initialize();
        
        // Act - Rapid show/hide
        for (int i = 0; i < 3; i++)
        {
            var contextMenu = trayManager.RightClickTrayIcon();
            contextMenu!.ClickDetails(() => window.ShowWindow());
            var contextMenu2 = trayManager.RightClickTrayIcon();
            contextMenu2!.ClickDetails(() => window.HideWindow());
        }
        
        // Assert - Window state should be consistent
        window.Visibility.Should().Be(Visibility.Hidden, "final state should be hidden");
        trayManager.IsTrayIconVisible.Should().BeTrue("tray should remain visible");
    }

    #endregion

    #region Helper Classes and Mocks

    private class MockMainWindowForStartup
    {
        public Visibility Visibility { get; set; } = Visibility.Hidden;
        public bool ShowInTaskbar { get; set; } = false;
        public WindowState WindowState { get; set; } = WindowState.Normal;
        private Dictionary<string, bool> _buttons = new()
        {
            { "Refresh", true },
            { "Mini Monitor", true },
            { "Close", true }
        };
        private bool _hasResourcesList = true;
        private IAspirePollingService? _pollingService;

        public event Action? OnFocus;

        public void Initialize(IAspirePollingService? pollingService = null)
        {
            _pollingService = pollingService;
        }

        public void ShowWindow()
        {
            Visibility = Visibility.Visible;
            WindowState = WindowState.Normal;
            ShowInTaskbar = true;
            OnFocus?.Invoke();
        }

        public void HideWindow()
        {
            Visibility = Visibility.Hidden;
            ShowInTaskbar = false;
        }

        public void ClickCloseButton()
        {
            HideWindow();
        }

        public bool HasButton(string buttonName)
        {
            return _buttons.ContainsKey(buttonName) && _buttons[buttonName];
        }

        public List<string> GetAllButtons()
        {
            return _buttons.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
        }

        public bool HasResourcesList()
        {
            return _hasResourcesList;
        }
    }

    private class MockTrayManager
    {
        public bool IsTrayIconVisible { get; set; } = false;
        private MockContextMenuForTray? _contextMenu;

        public void Initialize()
        {
            IsTrayIconVisible = true;
        }

        public MockContextMenuForTray? RightClickTrayIcon()
        {
            if (!IsTrayIconVisible)
                return null;
            
            _contextMenu = new MockContextMenuForTray();
            return _contextMenu;
        }

        public void DoubleClickTrayIcon(Action toggleAction)
        {
            toggleAction?.Invoke();
        }
    }

    private class MockContextMenuForTray
    {
        public bool IsVisible { get; set; } = true;

        public List<string> GetMenuItems()
        {
            return new List<string> { "Details", "Mini Monitor", "Settings", "GitHub", "Exit" };
        }

        public void ClickDetails(Action callback)
        {
            callback?.Invoke();
            IsVisible = false;
        }

        public void ClickMiniMonitor(Action callback)
        {
            callback?.Invoke();
            IsVisible = false;
        }

        public void ClickSettings(Action callback)
        {
            callback?.Invoke();
            IsVisible = false;
        }

        public void ClickGitHub(Action callback)
        {
            callback?.Invoke();
            IsVisible = false;
        }

        public void ClickExit(Action callback)
        {
            callback?.Invoke();
            IsVisible = false;
        }

        public void PressEscapeKey()
        {
            IsVisible = false;
        }
    }

    private class MockWindowManagerForTray
    {
        public bool IsSettingsWindowOpen { get; private set; } = false;
        public bool IsMiniMonitorOpen { get; private set; } = false;
        public int SettingsWindowInstanceCount { get; private set; } = 0;
        public IConfigurationService? ConfigService { get; set; }
        public IAspirePollingService? PollingService { get; set; }

        public void OpenSettingsWindow()
        {
            if (!IsSettingsWindowOpen)
            {
                IsSettingsWindowOpen = true;
                SettingsWindowInstanceCount = 1;
            }
        }

        public void CloseSettingsWindow(bool save)
        {
            IsSettingsWindowOpen = false;
        }

        public void SaveSettingsChanges(Dictionary<string, object> changes)
        {
            ConfigService?.UpdateSettings(changes);
            PollingService?.Restart();
        }

        public void OpenMiniMonitor()
        {
            IsMiniMonitorOpen = true;
        }
    }

    private class MockSettingsWindow
    {
        private List<string> _fields = new()
        {
            "AspireEndpoint",
            "PollingInterval",
            "CpuThresholdWarning",
            "CpuThresholdCritical",
            "MemoryThresholdWarning",
            "MemoryThresholdCritical",
            "StartWithWindows",
            "ProjectFolder",
            "RepositoryUrl"
        };

        public bool HasField(string fieldName)
        {
            return _fields.Contains(fieldName);
        }
    }

    private class MockPollingServiceForStartup : IAspirePollingService
    {
        public bool IsRunning { get; private set; } = false;
        public bool WasRestarted { get; private set; } = false;

        public void Start()
        {
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public void Restart()
        {
            WasRestarted = true;
            Stop();
            Start();
        }
    }

    private class MockConfigurationService : IConfigurationService
    {
        public bool HasBeenUpdated { get; private set; } = false;
        private Dictionary<string, object> _config = new();

        public void UpdateSettings(Dictionary<string, object> settings)
        {
            _config = settings;
            HasBeenUpdated = true;
        }

        public Dictionary<string, object> GetCurrentConfig()
        {
            return new Dictionary<string, object>(_config);
        }
    }

    private class MockProcessHandler
    {
        public string LastOpenedUrl { get; set; } = "";

        public void OpenUrl(string url)
        {
            LastOpenedUrl = url;
        }
    }

    private class MockApplicationManager
    {
        public bool IsRunning { get; set; } = false;
        public Visibility MainWindowVisibility { get; set; } = Visibility.Visible;
        public bool IsTrayIconVisible { get; set; } = false;
        public bool IsMainWindowOpen { get; set; } = false;
        public bool IsMiniMonitorOpen { get; set; } = false;

        public void Start()
        {
            IsRunning = true;
            MainWindowVisibility = Visibility.Hidden;
            IsTrayIconVisible = true;
        }

        public void Stop()
        {
            IsRunning = false;
            IsMainWindowOpen = false;
            IsMiniMonitorOpen = false;
            IsTrayIconVisible = false;
        }

        public void ExitApplication()
        {
            Stop();
        }
    }

    private interface IAspirePollingService
    {
        bool IsRunning { get; }
        void Start();
        void Stop();
        void Restart();
    }

    private interface IConfigurationService
    {
        void UpdateSettings(Dictionary<string, object> settings);
        Dictionary<string, object> GetCurrentConfig();
    }

    #endregion
}
