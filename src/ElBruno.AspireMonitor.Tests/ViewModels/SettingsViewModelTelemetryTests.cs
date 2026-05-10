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
}
