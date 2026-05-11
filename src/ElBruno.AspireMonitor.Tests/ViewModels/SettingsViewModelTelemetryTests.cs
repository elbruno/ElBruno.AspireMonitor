using ElBruno.AspireMonitor.Services;
using ElBruno.AspireMonitor.ViewModels;
using FluentAssertions;
using Moq;
using Xunit;
using AppConfig = ElBruno.AspireMonitor.Models.Configuration;

namespace ElBruno.AspireMonitor.Tests.ViewModels;

public class SettingsViewModelTelemetryTests
{
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
}
