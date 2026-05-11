using System.Reflection;
using ElBruno.AspireMonitor.Models;
using ElBruno.AspireMonitor.Services;
using ElBruno.AspireMonitor.ViewModels;
using FluentAssertions;
using Moq;
using Xunit;
using AppConfig = ElBruno.AspireMonitor.Models.Configuration;

namespace ElBruno.AspireMonitor.Tests.ViewModels;

public class MiniWindowResourceFilteringTests
{
    [Fact]
    public void PinnedResources_DefaultFilter_ShowsEndpointResourcesAndHidesNonEndpointResources()
    {
        var configService = CreateConfig("api");
        var mainVm = new MainViewModel(null, configService.Object, null);
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "api-public",
            Url = "http://localhost:5001",
            Type = "Project",
            Status = ResourceStatus.Running
        });
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "api-worker",
            Url = null,
            Type = "Project",
            Status = ResourceStatus.Running
        });

        var miniVm = new MiniMonitorViewModel(mainVm);

        miniVm.PinnedResources.Should().ContainSingle("the default mini monitor filter should keep endpoint-bearing/main resources only");
        miniVm.PinnedResources[0].Name.Should().Be("api-public");
        miniVm.PinnedResources.Should().NotContain(item => item.Name == "api-worker");
    }

    [Fact]
    public void PinnedResources_FilterDisabled_IncludesEndpointAndNonEndpointResources()
    {
        var config = CreateConfigObject("api");
        SetMainResourceFilter(config, enabled: false);
        var configService = CreateConfig(config);
        var mainVm = new MainViewModel(null, configService.Object, null);
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "api-public",
            Url = "http://localhost:5001",
            Type = "Project",
            Status = ResourceStatus.Running
        });
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "api-worker",
            Url = null,
            Type = "Project",
            Status = ResourceStatus.Running
        });

        var miniVm = new MiniMonitorViewModel(mainVm);

        miniVm.PinnedResources.Select(item => item.Name).Should().Equal("api-public", "api-worker");
        miniVm.PinnedResources[1].HasUrl.Should().BeFalse();
        miniVm.PinnedResources[1].FallbackText.Should().Be("Project");
    }

    [Fact]
    public void PinnedResources_LiveToggleMainResourceFilter_RefreshesMiniMonitorRows()
    {
        var configService = CreateConfig("api");
        var mainVm = new MainViewModel(null, configService.Object, null);
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "api-public",
            Url = "http://localhost:5001",
            Status = ResourceStatus.Running
        });
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "api-worker",
            Url = null,
            Type = "Project",
            Status = ResourceStatus.Running
        });
        var miniVm = new MiniMonitorViewModel(mainVm);
        miniVm.PinnedResources.Select(item => item.Name).Should().Equal("api-public");

        SetMainResourceFilter(mainVm, enabled: false);

        miniVm.PinnedResources.Select(item => item.Name).Should().Equal("api-public", "api-worker");
    }

    private static Mock<IConfigurationService> CreateConfig(string resources)
    {
        return CreateConfig(CreateConfigObject(resources));
    }

    private static Mock<IConfigurationService> CreateConfig(AppConfig config)
    {
        var configService = new Mock<IConfigurationService>();
        configService.Setup(service => service.LoadConfiguration()).Returns(config);
        return configService;
    }

    private static AppConfig CreateConfigObject(string resources)
    {
        var config = new AppConfig
        {
            MiniWindowResources = resources,
            ShowMiniWindowResourceTelemetry = true
        };
        SetMainResourceFilter(config, enabled: true);
        return config;
    }

    private static void SetMainResourceFilter(object target, bool enabled)
    {
        var property = FindMainResourceFilterProperty(target.GetType());
        property.Should().NotBeNull($"{target.GetType().Name} should expose the persisted mini monitor main-resource filter setting");
        property!.SetValue(target, enabled);
    }

    private static PropertyInfo? FindMainResourceFilterProperty(Type type)
    {
        var candidateNames = new[]
        {
            "ShowOnlyMainResourcesInMiniWindow",
            "ShowOnlyMainMiniWindowResources",
            "ShowMainResourcesOnlyInMiniWindow",
            "MiniWindowMainResourcesOnly",
            "MiniWindowResourcesMainOnly",
            "ShowOnlyMiniWindowMainResources",
            "ShowOnlyMiniWindowResourcesWithEndpoints",
            "MiniWindowShowOnlyResourcesWithEndpoints",
            "ShowOnlyResourcesWithEndpointsInMiniWindow",
            "OnlyShowResourcesWithEndpointsInMiniWindow"
        };

        return candidateNames
            .Select(name => type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public))
            .FirstOrDefault(property => property?.PropertyType == typeof(bool));
    }
}
