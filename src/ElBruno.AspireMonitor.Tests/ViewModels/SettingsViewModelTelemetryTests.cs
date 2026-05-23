using ElBruno.AspireMonitor.Services;
using ElBruno.AspireMonitor.ViewModels;
using FluentAssertions;
using Moq;
using Xunit;
using AppConfig = ElBruno.AspireMonitor.Models.Configuration;

namespace ElBruno.AspireMonitor.Tests.ViewModels;

public class SettingsViewModelTelemetryTests
{
    [Fact]
    public void Constructor_LoadsWorktreeDiscoverySettings_FromConfig()
    {
        var configService = new Mock<IConfigurationService>();
        configService.Setup(service => service.LoadConfiguration())
            .Returns(new AppConfig
            {
                EnableWorktreeDiscovery = true,
                WorktreeBasePath = @"C:\worktrees"
            });

        var viewModel = new SettingsViewModel(configService.Object);

        viewModel.EnableWorktreeDiscovery.Should().BeTrue();
        viewModel.WorktreeBasePath.Should().Be(@"C:\worktrees");
    }

    [Fact]
    public void SaveSettings_PersistsWorktreeDiscoverySettings()
    {
        var worktreeBasePath = Path.Combine(Path.GetTempPath(), $"Worktrees_{Guid.NewGuid():N}");
        Directory.CreateDirectory(worktreeBasePath);

        var configService = new Mock<IConfigurationService>();
        configService.Setup(service => service.LoadConfiguration())
            .Returns(new AppConfig());
        AppConfig? saved = null;
        configService.Setup(service => service.SaveConfiguration(It.IsAny<AppConfig>()))
            .Callback<AppConfig>(configuration => saved = configuration);

        try
        {
            var viewModel = new SettingsViewModel(configService.Object)
            {
                EnableWorktreeDiscovery = true,
                WorktreeBasePath = worktreeBasePath
            };

            viewModel.SaveSettings();

            saved.Should().NotBeNull();
            saved!.EnableWorktreeDiscovery.Should().BeTrue();
            saved.WorktreeBasePath.Should().Be(worktreeBasePath);
        }
        finally
        {
            if (Directory.Exists(worktreeBasePath))
                Directory.Delete(worktreeBasePath, recursive: true);
        }
    }

    [Fact]
    public void ValidateConfiguration_InvalidWorktreeBasePath_ReturnsFalseAndMessage()
    {
        var configService = new Mock<IConfigurationService>();
        configService.Setup(service => service.LoadConfiguration()).Returns(new AppConfig());
        var viewModel = new SettingsViewModel(configService.Object)
        {
            EnableWorktreeDiscovery = true,
            WorktreeBasePath = @"C:\definitely\does\not\exist"
        };

        var result = viewModel.Validate();

        result.Should().BeFalse();
        viewModel.ValidationMessage.Should().Be("Worktree base path does not exist.");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_LoadsMiniWindowResourceTelemetryToggle_FromConfig(bool showTelemetry)
    {
        var configService = new Mock<IConfigurationService>();
        configService.Setup(service => service.LoadConfiguration())
            .Returns(new AppConfig { ShowMiniWindowResourceTelemetry = showTelemetry });

        var viewModel = new SettingsViewModel(configService.Object);

        viewModel.ShowMiniWindowResourceTelemetry.Should().Be(showTelemetry);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SaveSettings_PersistsMiniWindowResourceTelemetryToggle(bool showTelemetry)
    {
        var configService = new Mock<IConfigurationService>();
        configService.Setup(service => service.LoadConfiguration())
            .Returns(new AppConfig());
        AppConfig? saved = null;
        configService.Setup(service => service.SaveConfiguration(It.IsAny<AppConfig>()))
            .Callback<AppConfig>(configuration => saved = configuration);

        var viewModel = new SettingsViewModel(configService.Object)
        {
            ShowMiniWindowResourceTelemetry = showTelemetry
        };

        viewModel.SaveSettings();

        saved.Should().NotBeNull();
        saved!.ShowMiniWindowResourceTelemetry.Should().Be(showTelemetry);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_LoadsMiniWindowMainResourceFilterToggle_FromConfig(bool showOnlyMainResources)
    {
        var configService = new Mock<IConfigurationService>();
        configService.Setup(service => service.LoadConfiguration())
            .Returns(new AppConfig { ShowOnlyMainMiniWindowResources = showOnlyMainResources });

        var viewModel = new SettingsViewModel(configService.Object);

        viewModel.ShowOnlyMainMiniWindowResources.Should().Be(showOnlyMainResources);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SaveSettings_PersistsMiniWindowMainResourceFilterToggle(bool showOnlyMainResources)
    {
        var configService = new Mock<IConfigurationService>();
        configService.Setup(service => service.LoadConfiguration())
            .Returns(new AppConfig());
        AppConfig? saved = null;
        configService.Setup(service => service.SaveConfiguration(It.IsAny<AppConfig>()))
            .Callback<AppConfig>(configuration => saved = configuration);

        var viewModel = new SettingsViewModel(configService.Object)
        {
            ShowOnlyMainMiniWindowResources = showOnlyMainResources
        };

        viewModel.SaveSettings();

        saved.Should().NotBeNull();
        saved!.ShowOnlyMainMiniWindowResources.Should().Be(showOnlyMainResources);
    }


    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_LoadsAspireStateNotificationToggle_FromConfig(bool enableNotifications)
    {
        var configService = new Mock<IConfigurationService>();
        configService.Setup(service => service.LoadConfiguration())
            .Returns(new AppConfig { EnableAspireStateNotifications = enableNotifications });

        var viewModel = new SettingsViewModel(configService.Object);

        viewModel.EnableAspireStateNotifications.Should().Be(enableNotifications);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SaveSettings_PersistsAspireStateNotificationToggle(bool enableNotifications)
    {
        var configService = new Mock<IConfigurationService>();
        configService.Setup(service => service.LoadConfiguration())
            .Returns(new AppConfig());
        AppConfig? saved = null;
        configService.Setup(service => service.SaveConfiguration(It.IsAny<AppConfig>()))
            .Callback<AppConfig>(configuration => saved = configuration);

        var viewModel = new SettingsViewModel(configService.Object)
        {
            EnableAspireStateNotifications = enableNotifications
        };

        viewModel.SaveSettings();

        saved.Should().NotBeNull();
        saved!.EnableAspireStateNotifications.Should().Be(enableNotifications);
    }
}
