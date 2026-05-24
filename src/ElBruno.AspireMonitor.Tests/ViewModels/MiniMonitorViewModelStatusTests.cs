using ElBruno.AspireMonitor.Models;
using ElBruno.AspireMonitor.Services;
using ElBruno.AspireMonitor.ViewModels;
using FluentAssertions;
using Moq;
using Xunit;
using AppConfig = ElBruno.AspireMonitor.Models.Configuration;

namespace ElBruno.AspireMonitor.Tests.ViewModels;

public class MiniMonitorViewModelStatusTests
{
    [Fact]
    public void DefaultState_ShowsIdleValuesAndNoDashboard()
    {
        var miniVm = new MiniMonitorViewModel();

        miniVm.StatusLine.Should().Be("Aspire not running");
        miniVm.CanStartAspire.Should().BeTrue();
        miniVm.CanStopAspire.Should().BeFalse();
        miniVm.DashboardUrl.Should().BeEmpty();
        miniVm.HasDashboard.Should().BeFalse();
        miniVm.HasPinnedResources.Should().BeFalse();
    }

    [Fact]
    public void MiniMonitor_RefreshesWhenMainViewModelStateChanges()
    {
        var mainVm = CreateMainViewModel();
        var miniVm = new MiniMonitorViewModel(mainVm);

        miniVm.StatusLine.Should().Be("Aspire not running");

        mainVm.Resources.Add(CreateRunningResource("api", 20, 20));
        mainVm.HostUrl = "http://localhost:18888";
        mainVm.LastUpdated = DateTime.Now.AddMinutes(-5);
        mainVm.IsConnected = true;

        miniVm.StatusEmoji.Should().Be("🟢");
        miniVm.StatusLine.Should().Be("1 resource | healthy");
        miniVm.CanStartAspire.Should().BeFalse();
        miniVm.CanStopAspire.Should().BeTrue();
        miniVm.DashboardUrl.Should().Be("http://localhost:18888");
        miniVm.HasDashboard.Should().BeTrue();
        miniVm.LastUpdateText.Should().Contain("5m ago");
    }

    [Theory]
    [InlineData(20d, 20d, "🟢", "healthy")]
    [InlineData(80d, 80d, "🟡", "warning")]
    [InlineData(95d, 95d, "🔴", "critical")]
    public void MiniMonitor_MapsOverallHealthToStatusSummary(double cpuUsage, double memoryUsage, string expectedEmoji, string expectedStatus)
    {
        var mainVm = CreateMainViewModel(isConnected: true);
        mainVm.Resources.Add(CreateRunningResource("api", cpuUsage, memoryUsage));

        var miniVm = new MiniMonitorViewModel(mainVm);

        miniVm.StatusEmoji.Should().Be(expectedEmoji);
        miniVm.StatusLine.Should().Be($"1 resource | {expectedStatus}");
    }

    [Fact]
    public void MiniMonitor_HasDashboard_RequiresConnectionResourcesAndUrl()
    {
        var mainVm = CreateMainViewModel(isConnected: false);
        mainVm.Resources.Add(CreateRunningResource("api", 20, 20));
        mainVm.HostUrl = "http://localhost:18888";

        var miniVm = new MiniMonitorViewModel(mainVm);

        miniVm.HasDashboard.Should().BeFalse("the dashboard link should stay hidden until Aspire is connected");

        mainVm.IsConnected = true;

        miniVm.HasDashboard.Should().BeTrue();
    }

    private static MainViewModel CreateMainViewModel(bool isConnected = false)
    {
        var configService = new Mock<IConfigurationService>();
        configService.Setup(service => service.LoadConfiguration()).Returns(new AppConfig());

        var mainVm = new MainViewModel(null, configService.Object, null);
        mainVm.IsConnected = isConnected;
        return mainVm;
    }

    private static ResourceViewModel CreateRunningResource(string name, double cpuUsage, double memoryUsage)
    {
        return new ResourceViewModel
        {
            Name = name,
            Status = ResourceStatus.Running,
            CpuUsage = cpuUsage,
            MemoryUsage = memoryUsage
        };
    }
}
