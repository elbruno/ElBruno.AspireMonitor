using ElBruno.AspireMonitor.Models;
using ElBruno.AspireMonitor.Services;
using ElBruno.AspireMonitor.ViewModels;
using FluentAssertions;
using Moq;
using Xunit;
using AppConfig = ElBruno.AspireMonitor.Models.Configuration;

namespace ElBruno.AspireMonitor.Tests.ViewModels;

/// <summary>
/// Tests verifying that when a working folder is configured in the Settings window,
/// its value propagates to both the main window (MainViewModel.ProjectFolder)
/// and the mini window (MiniMonitorViewModel.WorkingFolder).
/// 
/// Scenario: launch the app, set working folder in Settings, value appears in both windows.
/// </summary>
public class WorkingFolderTests : IDisposable
{
    private readonly string _testFolder;

    // Represents the path a user would set in the Settings window
    private const string SampleWorkingFolder =
        @"d:\aitourfy26\aitour26-BRK445-building-enterprise-ready-ai-agents-with-azure-ai-foundry\src\";

    public WorkingFolderTests()
    {
        _testFolder = Path.Combine(Path.GetTempPath(), $"AspireWorkingFolderTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testFolder);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testFolder))
        {
            try { Directory.Delete(_testFolder, true); }
            catch { /* suppress cleanup errors */ }
        }
    }

    #region SettingsViewModel

    [Fact]
    public void SettingsViewModel_LoadsProjectFolder_FromConfig()
    {
        // Arrange
        var configService = new Mock<IConfigurationService>();
        configService.Setup(s => s.LoadConfiguration())
            .Returns(new AppConfig { ProjectFolder = SampleWorkingFolder });

        // Act
        var vm = new SettingsViewModel(configService.Object);

        // Assert
        vm.ProjectFolder.Should().Be(SampleWorkingFolder,
            "SettingsViewModel should display the ProjectFolder stored in config");
    }

    [Fact]
    public void SettingsViewModel_ProjectFolder_CanBeUpdated_AndRaisesPropertyChanged()
    {
        // Arrange
        var configService = new Mock<IConfigurationService>();
        configService.Setup(s => s.LoadConfiguration()).Returns(new AppConfig());
        var vm = new SettingsViewModel(configService.Object);
        var propertyChangedFired = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SettingsViewModel.ProjectFolder))
                propertyChangedFired = true;
        };

        // Act
        vm.ProjectFolder = SampleWorkingFolder;

        // Assert
        vm.ProjectFolder.Should().Be(SampleWorkingFolder);
        propertyChangedFired.Should().BeTrue("setting ProjectFolder should raise PropertyChanged");
    }

    [Fact]
    public void SettingsViewModel_SaveSettings_PersistsProjectFolder_ToConfigService()
    {
        // Arrange - use an actually existing folder so Validate() passes
        var configService = new Mock<IConfigurationService>();
        configService.Setup(s => s.LoadConfiguration()).Returns(new AppConfig());
        AppConfig? saved = null;
        configService.Setup(s => s.SaveConfiguration(It.IsAny<AppConfig>()))
            .Callback<AppConfig>(c => saved = c);

        var vm = new SettingsViewModel(configService.Object);
        vm.ProjectFolder = _testFolder;

        // Act
        vm.SaveSettings();

        // Assert
        configService.Verify(s => s.SaveConfiguration(It.IsAny<AppConfig>()), Times.Once,
            "SaveConfiguration should be called exactly once");
        saved.Should().NotBeNull();
        saved!.ProjectFolder.Should().Be(_testFolder,
            "saved config should contain the ProjectFolder value from the SettingsViewModel");
    }

    [Fact]
    public void SettingsViewModel_Validate_EmptyProjectFolder_IsValid()
    {
        // Arrange
        var configService = new Mock<IConfigurationService>();
        configService.Setup(s => s.LoadConfiguration()).Returns(new AppConfig());
        var vm = new SettingsViewModel(configService.Object) { ProjectFolder = string.Empty };

        // Act
        var result = vm.Validate();

        // Assert
        result.Should().BeTrue("an empty ProjectFolder is optional and should be valid");
    }

    [Fact]
    public void SettingsViewModel_Validate_NonExistentFolder_IsInvalid()
    {
        // Arrange
        var configService = new Mock<IConfigurationService>();
        configService.Setup(s => s.LoadConfiguration()).Returns(new AppConfig());
        var vm = new SettingsViewModel(configService.Object)
        {
            ProjectFolder = @"C:\Definitely\Does\Not\Exist\Path\XYZ"
        };

        // Act
        var result = vm.Validate();

        // Assert
        result.Should().BeFalse("a non-existent folder should fail validation");
        vm.ValidationMessage.Should().NotBeEmpty("a validation message should be provided");
    }

    #endregion

    #region MainViewModel

    [Fact]
    public void MainViewModel_ProjectFolder_LoadedFromConfig_AtStartup()
    {
        // Arrange
        var configService = new Mock<IConfigurationService>();
        configService.Setup(s => s.LoadConfiguration())
            .Returns(new AppConfig { ProjectFolder = SampleWorkingFolder });

        // Act
        var vm = new MainViewModel(null, configService.Object, null);

        // Assert
        vm.ProjectFolder.Should().Be(SampleWorkingFolder,
            "MainViewModel should initialise ProjectFolder from config at startup, " +
            "so the main window can display it immediately");
    }

    [Fact]
    public void MainViewModel_ProjectFolder_RaisesPropertyChanged_WhenUpdated()
    {
        // Arrange
        var configService = new Mock<IConfigurationService>();
        configService.Setup(s => s.LoadConfiguration()).Returns(new AppConfig());
        var vm = new MainViewModel(null, configService.Object, null);
        var changed = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.ProjectFolder))
                changed = true;
        };

        // Act
        vm.ProjectFolder = SampleWorkingFolder;

        // Assert
        vm.ProjectFolder.Should().Be(SampleWorkingFolder);
        changed.Should().BeTrue("ProjectFolder changes must raise PropertyChanged so the UI binding refreshes");
    }

    #endregion

    #region MiniMonitorViewModel — WorkingFolder propagation

    [Fact]
    public void MiniMonitorViewModel_WorkingFolder_ShowsProjectFolder_WhenAspireIsRunning()
    {
        // Arrange
        var configService = new Mock<IConfigurationService>();
        configService.Setup(s => s.LoadConfiguration())
            .Returns(new AppConfig { ProjectFolder = SampleWorkingFolder });

        var mainVm = new MainViewModel(null, configService.Object, null);

        // Add a resource to put the mini monitor into the "Aspire running" state
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "test-service",
            Status = ResourceStatus.Running
        });

        // Act — create mini monitor AFTER resources are present
        var miniVm = new MiniMonitorViewModel(mainVm);

        // Assert
        miniVm.WorkingFolder.Should().Be(SampleWorkingFolder,
            "the mini window must display the configured ProjectFolder when Aspire is running");
    }

    [Fact]
    public void MiniMonitorViewModel_WorkingFolder_ShowsProjectFolder_EvenWhenAspireIsNotRunning()
    {
        // Arrange
        var configService = new Mock<IConfigurationService>();
        configService.Setup(s => s.LoadConfiguration())
            .Returns(new AppConfig { ProjectFolder = SampleWorkingFolder });

        var mainVm = new MainViewModel(null, configService.Object, null);
        // No resources — Aspire not running

        // Act
        var miniVm = new MiniMonitorViewModel(mainVm);

        // Assert
        miniVm.WorkingFolder.Should().Be(SampleWorkingFolder,
            "the mini window should always show the configured folder path, " +
            "even before Aspire starts, so the user can verify their setting");
    }

    [Fact]
    public void MiniMonitorViewModel_WorkingFolder_UpdatesWhenMainViewModelProjectFolderChanges()
    {
        // Arrange
        var configService = new Mock<IConfigurationService>();
        configService.Setup(s => s.LoadConfiguration()).Returns(new AppConfig());

        var mainVm = new MainViewModel(null, configService.Object, null);
        var miniVm = new MiniMonitorViewModel(mainVm);

        // Act — simulate saving new settings and reloading ProjectFolder into MainViewModel
        mainVm.ProjectFolder = SampleWorkingFolder;

        // Assert
        miniVm.WorkingFolder.Should().Be(SampleWorkingFolder,
            "MiniMonitorViewModel listens to MainViewModel.PropertyChanged(ProjectFolder) " +
            "and must update WorkingFolder so the mini window reflects the latest setting");
    }

    [Fact]
    public void MiniMonitorViewModel_WorkingFolder_ShowsDefaultMessage_WhenNoFolderConfigured()
    {
        // Arrange
        var configService = new Mock<IConfigurationService>();
        configService.Setup(s => s.LoadConfiguration())
            .Returns(new AppConfig { ProjectFolder = string.Empty });

        var mainVm = new MainViewModel(null, configService.Object, null);
        // No resources, no configured folder
        var miniVm = new MiniMonitorViewModel(mainVm);

        // Assert
        miniVm.WorkingFolder.Should().Be("Aspire is not running",
            "when no folder is configured and Aspire is not running, " +
            "a helpful default message should be shown");
    }

    #endregion
}
