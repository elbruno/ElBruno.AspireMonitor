using System.Reflection;
using ElBruno.AspireMonitor.Services;
using FluentAssertions;
using Xunit;
using AppConfig = ElBruno.AspireMonitor.Models.Configuration;

namespace ElBruno.AspireMonitor.Tests.Services;

public class ConfigurationMainResourceFilterTests : IDisposable
{
    private readonly string _root = Path.Combine(AppContext.BaseDirectory, "ConfigurationMainResourceFilter", Guid.NewGuid().ToString("N"));
    private readonly string _configPath;

    public ConfigurationMainResourceFilterTests()
    {
        _configPath = Path.Combine(_root, "config.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public void NewConfiguration_DefaultsMiniMonitorMainResourceFilterToEnabled()
    {
        var config = new AppConfig();

        GetFilterValue(config).Should().BeTrue("the mini monitor should default to endpoint-bearing/main resources only");
    }

    [Fact]
    public void Constructor_WithExistingConfigMissingMainResourceFilter_UsesDefaultEnabled()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
        File.WriteAllText(_configPath, """
        {
          "AspireEndpoint": "http://localhost:18888",
          "PollingIntervalMs": 5000,
          "CpuThresholdWarning": 70,
          "CpuThresholdCritical": 90,
          "MemoryThresholdWarning": 70,
          "MemoryThresholdCritical": 90,
          "MiniWindowResources": "api, worker"
        }
        """);

        var config = new ConfigurationService(_configPath).LoadConfiguration();

        GetFilterValue(config).Should().BeTrue("older config files should opt into the safer filtered mini monitor default");
    }

    [Fact]
    public void SaveConfiguration_PersistsDisabledMiniMonitorMainResourceFilter()
    {
        var service = new ConfigurationService(_configPath);
        var config = new AppConfig
        {
            MiniWindowResources = "api, worker"
        };
        SetFilterValue(config, enabled: false);

        service.SaveConfiguration(config);
        var reloaded = new ConfigurationService(_configPath).LoadConfiguration();

        GetFilterValue(reloaded).Should().BeFalse("users who disable the filter should see non-endpoint resources after restart");
    }

    private static bool GetFilterValue(AppConfig config)
    {
        var property = FindFilterProperty();
        property.Should().NotBeNull("Configuration should expose a persisted bool for mini monitor main-resource filtering");
        return (bool)property!.GetValue(config)!;
    }

    private static void SetFilterValue(AppConfig config, bool enabled)
    {
        var property = FindFilterProperty();
        property.Should().NotBeNull("Configuration should expose a persisted bool for mini monitor main-resource filtering");
        property!.SetValue(config, enabled);
    }

    private static PropertyInfo? FindFilterProperty()
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
            .Select(name => typeof(AppConfig).GetProperty(name, BindingFlags.Instance | BindingFlags.Public))
            .FirstOrDefault(property => property?.PropertyType == typeof(bool));
    }
}
